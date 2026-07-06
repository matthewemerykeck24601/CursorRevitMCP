using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDiag2024Bridge.Utilities;

namespace RevitDiag2024Bridge.Services;

// HTTP gateway for the read-only Revit 2024 diagnostic bridge. Same shape as the 2025
// bridge gateway: an HttpListener accepts POST routes, deserializes the body, marshals
// the work onto Revit's API thread via the dispatcher, and returns a GatewayEnvelope<T>.
// This bridge is READ-ONLY — it opens no Transaction and writes nothing to the model.
//
// Revit 2024 API note: ElementId is 64-bit here. The int-based members
// (the IntegerValue property / the ElementId(int) constructor) are [Obsolete] in Revit 2024 — this bridge uses
// the Int64 accessor ElementId.Value and the ElementId(long) constructor throughout, and all
// element-id fields on the wire are long. (The deprecated int members appeared in Revit
// 2023 and earlier.)
//
// All spatial outputs are millimetres (Revit internal feet * 304.8).
public sealed class RevitApiGatewayService
{
    private readonly BridgeLogger _logger;
    private readonly RevitRequestDispatcher _dispatcher;
    private readonly int _port;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public RevitApiGatewayService(BridgeLogger logger, RevitRequestDispatcher dispatcher, int port)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _port = port;
    }

    public bool IsRunning => _listener?.IsListening == true;

    public string BaseUrl => $"http://127.0.0.1:{_port}";

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        var listener = new HttpListener();
        var prefix = $"{BaseUrl}/";
        listener.Prefixes.Add(prefix);
        listener.Start();

        _cts = new CancellationTokenSource();
        _listener = listener;
        _loopTask = Task.Run(() => AcceptLoop(listener, _cts.Token));
        _logger.Info($"Diagnostic gateway listening on {prefix}");
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _loopTask?.Wait(1000);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Gateway stop warning: {ex.GetBaseException().Message}");
        }
        finally
        {
            _listener = null;
            _cts = null;
            _loopTask = null;
        }
    }

    private async Task AcceptLoop(HttpListener listener, CancellationToken token)
    {
        while (!token.IsCancellationRequested && listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync();
            }
            catch when (token.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Gateway accept loop error: {ex.GetBaseException().Message}");
                continue;
            }

            _ = Task.Run(() => HandleRequest(context), token);
        }
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url?.AbsolutePath?.TrimEnd('/') ?? string.Empty;
            if (path.Equals("/health", StringComparison.OrdinalIgnoreCase))
            {
                var revitConnected = _dispatcher.LastUiApp is not null;
                if (!revitConnected)
                {
                    try
                    {
                        revitConnected = await _dispatcher.Ping(timeoutMs: 1000);
                    }
                    catch
                    {
                        revitConnected = false;
                    }
                }

                await WriteJson(context, 200, new GatewayEnvelope<object>
                {
                    Success = true,
                    Message = "ok",
                    Data = new { gateway = "revit-2024-diag-bridge", revitConnected, port = _port },
                });
                return;
            }

            if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                await WriteJson(context, 405, new GatewayEnvelope<object>
                {
                    Success = false,
                    Message = "Only POST is supported for API routes.",
                });
                return;
            }

            var body = await ReadBody(context.Request);
            switch (path.ToLowerInvariant())
            {
                // ─── Core read tools ───
                case "/api/get_document_metadata":
                    await HandleTyped<object, DocumentMetadataResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetDocumentMetadata, timeoutMs: 10000));
                    return;
                case "/api/get_active_document_context":
                    await HandleTyped<object, GetActiveContextResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetActiveContext, timeoutMs: 10000));
                    return;
                case "/api/get_element_by_id":
                    await HandleTyped<GetElementByIdRequest, ElementDetailRecord>(
                        context, body,
                        req => _dispatcher.Enqueue(app => GetElementById(app, req), timeoutMs: 10000));
                    return;
                case "/api/get_element_metadata":
                    await HandleTyped<ElementMetadataRequest, ElementMetadataResult>(
                        context, body,
                        req => _dispatcher.Enqueue(app => GetElementMetadata(app, req), timeoutMs: 10000));
                    return;
                case "/api/get_element_types":
                    await HandleTyped<GetElementTypesRequest, GetElementTypesResult>(
                        context, body,
                        req => _dispatcher.Enqueue(app => GetElementTypes(app, req), timeoutMs: 10000));
                    return;
                case "/api/get_levels":
                    await HandleTyped<object, GetLevelsResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetLevels, timeoutMs: 10000));
                    return;
                case "/api/get_grids":
                    await HandleTyped<object, GetGridsResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetGrids, timeoutMs: 10000));
                    return;
                case "/api/query_elements":
                    // Large models (800k+ elements) need headroom to enumerate + filter; the
                    // 2025 bridge uses 120s for geometry/save, this goes to 180s.
                    await HandleTyped<QueryElementsRequest, QueryElementsResult>(
                        context, body,
                        req => _dispatcher.Enqueue(app => QueryElements(app, req), timeoutMs: 180000));
                    return;
                case "/api/get_current_view_elements":
                    await HandleTyped<CurrentViewElementsRequest, List<CurrentViewElementRecord>>(
                        context, body,
                        req => _dispatcher.Enqueue(app => GetCurrentViewElements(app, req), timeoutMs: 15000));
                    return;
                case "/api/get_schedules":
                    await HandleTyped<object, GetSchedulesResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetSchedules, timeoutMs: 180000));
                    return;
                case "/api/get_schedule_data":
                    // TableData regeneration on a large schedule can be slow; matches the
                    // query_elements / get_schedules 180s ceiling.
                    await HandleTyped<GetScheduleDataRequest, GetScheduleDataResult>(
                        context, body,
                        req => _dispatcher.Enqueue(app => GetScheduleData(app, req), timeoutMs: 180000));
                    return;

                // ─── Diagnostic tools ───
                case "/api/diag/get_warnings":
                    await HandleTyped<object, GetWarningsResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetWarnings, timeoutMs: 15000));
                    return;
                case "/api/diag/get_missing_links":
                    await HandleTyped<object, GetMissingLinksResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetMissingLinks, timeoutMs: 15000));
                    return;
                case "/api/diag/get_journal_tail":
                    // Pure file IO — does NOT touch the Revit API, so it runs on the HTTP
                    // thread (outside the dispatcher) and never blocks the Revit message loop.
                    await HandleTyped<JournalTailRequest, JournalTailResult>(
                        context, body,
                        req => Task.FromResult(GetJournalTail(req)));
                    return;
                case "/api/diag/get_corrupt_elements":
                    await HandleTyped<object, GetCorruptElementsResult>(
                        context, body,
                        _ => _dispatcher.Enqueue(GetCorruptElements, timeoutMs: 30000));
                    return;

                default:
                    await WriteJson(context, 404, new GatewayEnvelope<object>
                    {
                        Success = false,
                        Message = $"Unknown route: {path}",
                    });
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Gateway request failed.", ex);
            var baseEx = ex.GetBaseException();
            await WriteJson(context, 500, new GatewayEnvelope<object>
            {
                Success = false,
                Message = $"{baseEx.GetType().Name}: {baseEx.Message}",
            });
        }
    }

    private async Task HandleTyped<T, TResult>(
        HttpListenerContext context,
        string body,
        Func<T, Task<TResult>> handler) where T : class, new()
    {
        var payload = string.IsNullOrWhiteSpace(body)
            ? new T()
            : JsonSerializer.Deserialize<T>(body, _jsonOptions) ?? new T();
        var data = await handler(payload);
        await WriteJson(context, 200, new GatewayEnvelope<object>
        {
            Success = true,
            Message = "ok",
            Data = data,
        });
    }

    private static async Task<string> ReadBody(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private async Task WriteJson(HttpListenerContext context, int statusCode, object payload)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonOptions);
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        context.Response.OutputStream.Close();
    }

    // ───────────────────────────────────────────────────────────────────────
    // Core read tools
    // ───────────────────────────────────────────────────────────────────────

    private static DocumentMetadataResult GetDocumentMetadata(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");

        var centralPath = string.Empty;
        var notes = string.Empty;
        if (doc.IsWorkshared)
        {
            try
            {
                var central = doc.GetWorksharingCentralModelPath();
                centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(central);
            }
            catch (Exception ex)
            {
                notes = $"Central path unavailable: {ex.GetBaseException().Message}";
            }
        }

        var linkCount = doc.IsFamilyDocument
            ? 0
            : new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).GetElementCount();

        return new DocumentMetadataResult
        {
            RevitVersion = app.Application.VersionNumber,
            DocumentTitle = doc.Title,
            DocumentPath = doc.PathName ?? string.Empty,
            IsModified = doc.IsModified,
            IsWorkshared = doc.IsWorkshared,
            IsFamilyDocument = doc.IsFamilyDocument,
            CurrentUser = app.Application.Username,
            ActiveViewName = doc.ActiveView?.Name ?? string.Empty,
            CentralModelPath = centralPath,
            LinkCount = linkCount,
            Notes = notes,
            ProjectInformation = ReadProjectInformation(doc),
        };
    }

    private static ProjectInformationRecord ReadProjectInformation(Document doc)
    {
        if (doc.IsFamilyDocument)
        {
            return new ProjectInformationRecord();
        }

        try
        {
            var info = doc.ProjectInformation;
            if (info is null)
            {
                return new ProjectInformationRecord();
            }

            return new ProjectInformationRecord
            {
                Name = info.Name ?? string.Empty,
                Number = info.Number ?? string.Empty,
                ClientName = info.ClientName ?? string.Empty,
                Status = info.Status ?? string.Empty,
            };
        }
        catch
        {
            return new ProjectInformationRecord();
        }
    }

    private static GetActiveContextResult GetActiveContext(UIApplication app)
    {
        var uidoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uidoc.Document;

        if (doc.IsFamilyDocument)
        {
            return new GetActiveContextResult
            {
                Context = "family",
                DocumentTitle = doc.Title,
                IsFamilyDocument = true,
                Category = doc.OwnerFamily?.FamilyCategory?.Name,
            };
        }

        return new GetActiveContextResult
        {
            Context = "project",
            DocumentTitle = doc.Title,
            IsFamilyDocument = false,
        };
    }

    private static ElementDetailRecord GetElementById(UIApplication app, GetElementByIdRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (request.ElementId <= 0)
        {
            throw new InvalidOperationException("elementId must be a positive integer.");
        }

        var element = doc.GetElement(new ElementId(request.ElementId));
        if (element is null)
        {
            return new ElementDetailRecord
            {
                Found = false,
                Id = request.ElementId,
                Note = "Element not found in active document.",
            };
        }

        return BuildElementDetail(doc, element, request.IncludeParameters);
    }

    private static ElementMetadataResult GetElementMetadata(UIApplication app, ElementMetadataRequest request)
    {
        var uidoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uidoc.Document;

        var ids = new List<ElementId>();
        if (request.ElementIds is { Count: > 0 })
        {
            ids.AddRange(request.ElementIds.Where(i => i > 0).Distinct().Select(i => new ElementId(i)));
        }
        else if (request.UseSelection)
        {
            ids.AddRange(uidoc.Selection.GetElementIds());
        }

        if (ids.Count == 0)
        {
            throw new InvalidOperationException("Provide elementIds or set useSelection=true.");
        }

        var records = new List<ElementDetailRecord>();
        foreach (var id in ids)
        {
            var element = doc.GetElement(id);
            if (element is null)
            {
                records.Add(new ElementDetailRecord
                {
                    Found = false,
                    Id = id.Value,
                    Note = "Element not found in active document.",
                });
                continue;
            }

            records.Add(BuildElementDetail(doc, element, request.IncludeParameters));
        }

        return new ElementMetadataResult { Elements = records };
    }

    private static ElementDetailRecord BuildElementDetail(Document doc, Element element, bool includeParameters)
    {
        var typeId = element.GetTypeId();
        var elementType = typeId != ElementId.InvalidElementId ? doc.GetElement(typeId) as ElementType : null;

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (includeParameters)
        {
            foreach (Parameter p in element.Parameters)
            {
                if (p?.Definition is null || !p.HasValue)
                {
                    continue;
                }

                var key = p.Definition.Name;
                if (string.IsNullOrWhiteSpace(key) || parameters.ContainsKey(key))
                {
                    continue;
                }

                parameters[key] = ParameterValueToString(doc, p);
            }
        }

        return new ElementDetailRecord
        {
            Found = true,
            Id = element.Id.Value,
            Name = element.Name,
            Category = element.Category is not null ? ToCategoryToken(element.Category) : string.Empty,
            FamilyName = ResolveElementFamilyName(doc, element),
            TypeName = elementType?.Name ?? string.Empty,
            Location = ElementCenter(element),
            Parameters = parameters,
        };
    }

    private static GetElementTypesResult GetElementTypes(UIApplication app, GetElementTypesRequest request)
    {
        var doc = RequireProjectDocument(app);
        var limit = request.Limit <= 0 ? 100 : request.Limit;
        var categoryFilterId = ResolveOptionalCategoryId(request.Category);

        var symbols = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .Where(s => categoryFilterId is null || s.Category?.Id.Value == categoryFilterId.Value)
            .ToList();

        var records = symbols
            .Take(limit)
            .Select(s => new ElementTypeRecord
            {
                Id = s.Id.Value,
                Name = s.Name,
                FamilyName = s.FamilyName,
                Category = s.Category is not null ? ToCategoryToken(s.Category) : string.Empty,
                IsActive = s.IsActive,
            })
            .ToList();

        return new GetElementTypesResult { Types = records, TotalCount = symbols.Count };
    }

    private static GetLevelsResult GetLevels(UIApplication app)
    {
        var doc = RequireProjectDocument(app);
        var levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .ToList();

        // Ground floor = the level whose elevation is closest to 0.
        var ground = levels.OrderBy(l => Math.Abs(l.Elevation)).FirstOrDefault();

        var records = levels
            .OrderBy(l => l.Elevation)
            .Select(l => new LevelRecord
            {
                Id = l.Id.Value,
                Name = l.Name,
                ElevationMm = FeetToMm(l.Elevation),
                IsGroundFloor = ground is not null && l.Id == ground.Id,
            })
            .ToList();

        return new GetLevelsResult { Levels = records };
    }

    private static GetGridsResult GetGrids(UIApplication app)
    {
        var doc = RequireProjectDocument(app);
        var grids = new FilteredElementCollector(doc)
            .OfClass(typeof(Grid))
            .Cast<Grid>()
            .ToList();

        var records = new List<GridRecord>();
        foreach (var grid in grids)
        {
            GeomPoint? start = null;
            GeomPoint? end = null;
            try
            {
                var curve = grid.Curve;
                if (curve is not null)
                {
                    start = ToGeomPoint(curve.GetEndPoint(0));
                    end = ToGeomPoint(curve.GetEndPoint(1));
                }
            }
            catch
            {
                // Multi-segment / non-linear grids may not expose a single curve.
            }

            records.Add(new GridRecord
            {
                Id = grid.Id.Value,
                Name = grid.Name,
                Start = start,
                End = end,
            });
        }

        return new GetGridsResult { Grids = records };
    }

    // The critical tool for the piece-mark split workflow. All inputs are optional and AND'd:
    // category (OST token), familyName (exact), parameterFilters (instance OR type params,
    // case-insensitive name match). Returns every readable instance parameter per element.
    // limit defaults to 100 when unspecified; a positive limit has no upper bound.
    private static QueryElementsResult QueryElements(UIApplication app, QueryElementsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var limit = request.Limit <= 0 ? 100 : request.Limit;

        var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();

        var categoryFilterId = ResolveOptionalCategoryId(request.Category);
        if (categoryFilterId is not null)
        {
            collector = collector.OfCategory((BuiltInCategory)categoryFilterId.Value);
        }

        if (request.LevelId is > 0)
        {
            collector = collector.WherePasses(new ElementLevelFilter(new ElementId(request.LevelId.Value)));
        }

        var familyNameFilter = request.FamilyName?.Trim();
        var hasFamilyNameFilter = !string.IsNullOrWhiteSpace(familyNameFilter);

        var paramFilters = (request.ParameterFilters ?? new List<ParameterFilter>())
            .Where(f => f is not null && !string.IsNullOrWhiteSpace(f.Name))
            .ToList();

        var records = new List<QueriedElementRecord>();
        var matchedCount = 0;

        foreach (var element in collector)
        {
            if (hasFamilyNameFilter
                && !string.Equals(ResolveElementFamilyName(doc, element), familyNameFilter, StringComparison.Ordinal))
            {
                continue;
            }

            if (paramFilters.Count > 0)
            {
                var passes = true;
                foreach (var filter in paramFilters)
                {
                    // Targeted lookup of just this filter's parameter — far cheaper than
                    // coercing every parameter on every element.
                    var actual = ResolveParameterValueByName(doc, element, filter.Name);
                    if (!MatchesParameterFilter(actual, filter))
                    {
                        passes = false;
                        break;
                    }
                }

                if (!passes)
                {
                    continue;
                }
            }

            matchedCount++;
            if (records.Count >= limit)
            {
                continue;
            }

            // Only materialize the full parameter payload for the elements actually returned.
            // Building it for every one of ~840k elements (the count needs the full pass) is
            // what stalled the call; the cap keeps the heavy coercion to <= limit elements.
            records.Add(new QueriedElementRecord
            {
                ElementId = element.Id.Value,
                Category = element.Category is not null ? ToCategoryToken(element.Category) : string.Empty,
                FamilyName = ResolveElementFamilyName(doc, element),
                TypeName = ResolveTypeName(doc, element),
                Level = ResolveLevelName(doc, element),
                Parameters = ReadInstanceParameters(doc, element),
            });
        }

        return new QueryElementsResult { Elements = records, TotalCount = matchedCount };
    }

    private static List<CurrentViewElementRecord> GetCurrentViewElements(
        UIApplication app,
        CurrentViewElementsRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        var view = doc.ActiveView ?? throw new InvalidOperationException("No active view.");

        var collector = new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType();
        var categorySet = (request.ModelCategoryList ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var results = new List<CurrentViewElementRecord>();
        foreach (var element in collector)
        {
            if (element.Category is null)
            {
                continue;
            }

            var categoryName = ToCategoryToken(element.Category);
            if (categorySet.Count > 0 && !categorySet.Contains(categoryName))
            {
                continue;
            }

            results.Add(new CurrentViewElementRecord
            {
                ElementId = element.Id.Value,
                Name = element.Name,
                Category = categoryName,
                FamilyName = ResolveElementFamilyName(doc, element),
                TypeName = ResolveTypeName(doc, element),
                LevelName = ResolveLevelName(doc, element),
            });

            if (request.Limit is > 0 && results.Count >= request.Limit.Value)
            {
                break;
            }
        }

        return results;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Schedule reading (read-only TableData access — no Transaction)
    // ───────────────────────────────────────────────────────────────────────

    // Index of every non-template ViewSchedule: id, name, categoryName, and data-row count.
    private static GetSchedulesResult GetSchedules(UIApplication app)
    {
        var doc = RequireProjectDocument(app);

        var schedules = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSchedule))
            .Cast<ViewSchedule>()
            .Where(s => !s.IsTemplate)
            .ToList();

        var records = new List<ScheduleSummaryRecord>();
        foreach (var schedule in schedules)
        {
            var rowCount = 0;
            try
            {
                rowCount = schedule.GetTableData().GetSectionData(SectionType.Body).NumberOfRows;
            }
            catch
            {
                // Some schedule kinds (e.g. key/graphical column) may not expose a body section.
            }

            records.Add(new ScheduleSummaryRecord
            {
                ScheduleId = schedule.Id.Value,
                Name = schedule.Name,
                CategoryName = ResolveScheduleCategoryName(doc, schedule),
                RowCount = rowCount,
            });
        }

        return new GetSchedulesResult { Schedules = records, TotalCount = records.Count };
    }

    // Read one schedule's TableData: column headers (Header section), data rows (Body section),
    // each row a header->cell map. The schedule is identified by scheduleName (preferred, first
    // non-template match, case-insensitive) or scheduleId. Read-only: uses
    // TableSectionData.GetCellText, never a write path that would require a Transaction. Rows
    // are capped (default 1000, max 5000).
    private static GetScheduleDataResult GetScheduleData(UIApplication app, GetScheduleDataRequest request)
    {
        var doc = RequireProjectDocument(app);

        ViewSchedule schedule;
        var scheduleName = request.ScheduleName?.Trim();
        if (!string.IsNullOrWhiteSpace(scheduleName))
        {
            // Resolve by name directly — no get_schedules round-trip needed.
            schedule = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .FirstOrDefault(s => !s.IsTemplate
                    && string.Equals(s.Name, scheduleName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"No non-template schedule named '{scheduleName}' found.");
        }
        else if (request.ScheduleId > 0)
        {
            schedule = doc.GetElement(new ElementId(request.ScheduleId)) as ViewSchedule
                ?? throw new InvalidOperationException($"No ViewSchedule found with id {request.ScheduleId}.");
            if (schedule.IsTemplate)
            {
                throw new InvalidOperationException("Schedule is a view template and has no table data.");
            }
        }
        else
        {
            throw new InvalidOperationException("Provide a positive scheduleId or a scheduleName.");
        }

        var maxRows = request.MaxRows is > 0 ? Math.Min(request.MaxRows.Value, 5000) : 1000;

        var tableData = schedule.GetTableData();
        var body = tableData.GetSectionData(SectionType.Body);
        var header = tableData.GetSectionData(SectionType.Header);

        var columnCount = body?.NumberOfColumns ?? 0;
        var headers = BuildScheduleHeaders(header, columnCount);

        var totalRows = body?.NumberOfRows ?? 0;
        var take = Math.Min(totalRows, maxRows);

        var rows = new List<Dictionary<string, string>>();
        for (var r = 0; r < take; r++)
        {
            var row = new Dictionary<string, string>();
            for (var c = 0; c < columnCount; c++)
            {
                var cell = string.Empty;
                try
                {
                    cell = body!.GetCellText(r, c) ?? string.Empty;
                }
                catch
                {
                    // Merged/empty cells can throw; treat as blank.
                }

                row[headers[c]] = cell;
            }

            rows.Add(row);
        }

        return new GetScheduleDataResult
        {
            ScheduleId = schedule.Id.Value,
            Name = schedule.Name,
            CategoryName = ResolveScheduleCategoryName(doc, schedule),
            Headers = headers,
            Rows = rows,
            RowCount = rows.Count,
            ColumnCount = columnCount,
            Truncated = totalRows > take,
            TotalRows = totalRows,
        };
    }

    // Column headers from the Header section. The header block can carry a title row plus the
    // column-heading row, so the last non-empty cell per column wins. Duplicate/blank headings
    // are uniquified so every column survives as a distinct row-map key.
    private static List<string> BuildScheduleHeaders(TableSectionData? header, int columnCount)
    {
        var headers = new List<string>(columnCount);
        var headerRows = header?.NumberOfRows ?? 0;
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var c = 0; c < columnCount; c++)
        {
            var name = string.Empty;
            for (var r = 0; r < headerRows; r++)
            {
                try
                {
                    var text = header!.GetCellText(r, c);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        name = text.Trim();
                    }
                }
                catch
                {
                    // Skip unreadable header cells.
                }
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Column{c + 1}";
            }

            if (seen.TryGetValue(name, out var count))
            {
                seen[name] = count + 1;
                name = $"{name} ({count + 1})";
            }
            else
            {
                seen[name] = 1;
            }

            headers.Add(name);
        }

        return headers;
    }

    private static string ResolveScheduleCategoryName(Document doc, ViewSchedule schedule)
    {
        try
        {
            var catId = schedule.Definition?.CategoryId;
            if (catId is null || catId == ElementId.InvalidElementId)
            {
                return string.Empty;
            }

            return Category.GetCategory(doc, catId)?.Name ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    // Diagnostic tools
    // ───────────────────────────────────────────────────────────────────────

    private static GetWarningsResult GetWarnings(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");

        var warnings = doc.GetWarnings();
        const int cap = 200;

        var records = new List<WarningRecord>();
        foreach (var w in warnings.Take(cap))
        {
            var resolution = string.Empty;
            try
            {
                if (w.HasResolutions())
                {
                    resolution = w.GetDefaultResolutionCaption() ?? string.Empty;
                }
            }
            catch
            {
                // Some failures expose no resolution caption; leave empty.
            }

            records.Add(new WarningRecord
            {
                WarningText = w.GetDescriptionText() ?? string.Empty,
                Severity = w.GetSeverity().ToString(),
                FailingElementIds = w.GetFailingElements().Select(id => id.Value).ToList(),
                ResolutionDescription = resolution,
            });
        }

        return new GetWarningsResult
        {
            Warnings = records,
            TotalCount = warnings.Count,
            Truncated = warnings.Count > cap,
        };
    }

    private static GetMissingLinksResult GetMissingLinks(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");

        var instances = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .ToList();

        var records = new List<LinkRecord>();
        foreach (var instance in instances)
        {
            var linkType = doc.GetElement(instance.GetTypeId()) as RevitLinkType;

            var statusText = "Unknown";
            var isLoaded = false;
            var linkPath = string.Empty;

            if (linkType is not null)
            {
                try
                {
                    var extRef = linkType.GetExternalFileReference();
                    if (extRef is not null)
                    {
                        var status = extRef.GetLinkedFileStatus();
                        statusText = status.ToString();
                        isLoaded = status == LinkedFileStatus.Loaded;

                        var modelPath = extRef.GetAbsolutePath();
                        if (modelPath is not null && !modelPath.Empty)
                        {
                            linkPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusText = $"Error: {ex.GetBaseException().Message}";
                }
            }

            records.Add(new LinkRecord
            {
                ElementId = instance.Id.Value,
                LinkName = instance.Name,
                LinkPath = linkPath,
                IsLoaded = isLoaded,
                LinkedFileStatus = statusText,
            });
        }

        return new GetMissingLinksResult { Links = records, TotalCount = records.Count };
    }

    private static JournalTailResult GetJournalTail(JournalTailRequest request)
    {
        var lines = request.Lines <= 0 ? 50 : Math.Min(request.Lines, 500);

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var journalFolder = Path.Combine(localAppData, "Autodesk", "Revit", "Autodesk Revit 2024", "Journals");
        if (!Directory.Exists(journalFolder))
        {
            throw new InvalidOperationException($"Journal folder not found: {journalFolder}");
        }

        var newest = new DirectoryInfo(journalFolder)
            .GetFiles("*.txt")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"No journal (*.txt) files found in {journalFolder}");

        // FileShare.ReadWrite so Revit's own open write handle does not block us.
        var allLines = new List<string>();
        using (var stream = new FileStream(newest.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                allLines.Add(line);
            }
        }

        var tail = allLines.Count <= lines
            ? allLines
            : allLines.GetRange(allLines.Count - lines, lines);

        return new JournalTailResult
        {
            JournalPath = newest.FullName,
            LastWriteTimeUtc = newest.LastWriteTimeUtc.ToString("o", CultureInfo.InvariantCulture),
            LineCount = tail.Count,
            Lines = tail,
        };
    }

    private static GetCorruptElementsResult GetCorruptElements(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        const int cap = 1000;

        var elements = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .ToElements();

        var accessible = 0;
        var scanned = 0;
        var failed = new List<CorruptElementRecord>();

        foreach (var element in elements)
        {
            if (scanned >= cap)
            {
                break;
            }

            scanned++;
            try
            {
                // Touch the two members most likely to throw on a damaged element.
                _ = element.Name;
                _ = element.Category;
                accessible++;
            }
            catch (Exception ex)
            {
                long id = -1;
                try { id = element.Id.Value; } catch { /* even the id may throw */ }
                failed.Add(new CorruptElementRecord
                {
                    ElementId = id,
                    Error = ex.GetBaseException().Message,
                });
            }
        }

        return new GetCorruptElementsResult
        {
            ScannedCount = scanned,
            AccessibleCount = accessible,
            Failed = failed,
            Truncated = elements.Count > cap,
            Note = "Best-effort scan: reads Name and Category on up to 1000 non-type elements and "
                + "records any that throw. Not a guarantee of model integrity.",
        };
    }

    // ───────────────────────────────────────────────────────────────────────
    // Helpers (Revit 2024 int-based ElementId API)
    // ───────────────────────────────────────────────────────────────────────

    private static double FeetToMm(double feet) => feet * 304.8;

    private static double MmToFeet(double mm) => mm / 304.8;

    private static GeomPoint ToGeomPoint(XYZ p) => new()
    {
        X = FeetToMm(p.X),
        Y = FeetToMm(p.Y),
        Z = FeetToMm(p.Z),
    };

    private static Document RequireProjectDocument(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (doc.IsFamilyDocument)
        {
            throw new InvalidOperationException(
                "Active document is a family document. Open a project (.rvt) document first.");
        }

        return doc;
    }

    // Parses an optional "OST_*" category token; null/empty -> no filter; unknown -> clear error.
    private static int? ResolveOptionalCategoryId(string? categoryToken)
    {
        var token = categoryToken?.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        if (Enum.TryParse<BuiltInCategory>(token, ignoreCase: true, out var bic))
        {
            return (int)bic;
        }

        throw new InvalidOperationException(
            $"Unknown category token '{token}'. Use an OST_* BuiltInCategory token (e.g. OST_Walls).");
    }

    private static string ToCategoryToken(Category category)
    {
        // BuiltInCategory's underlying type is Int64 in Revit 2024, so Enum.IsDefined must be
        // given the Int64 id directly. Narrowing to Int32 first throws ArgumentException
        // ("Type passed in was 'System.Int32'; the enum underlying type was 'System.Int64'").
        var idValue = category.Id.Value;
        return Enum.IsDefined(typeof(BuiltInCategory), idValue)
            ? ((BuiltInCategory)idValue).ToString()
            : category.Name;
    }

    private static string ResolveElementFamilyName(Document doc, Element element)
    {
        if (element is FamilyInstance fi)
        {
            return fi.Symbol?.Family?.Name ?? string.Empty;
        }

        var typeId = element.GetTypeId();
        if (typeId != ElementId.InvalidElementId && doc.GetElement(typeId) is ElementType et)
        {
            return et is FamilySymbol fs ? fs.FamilyName : et.FamilyName ?? string.Empty;
        }

        return string.Empty;
    }

    private static string ResolveTypeName(Document doc, Element element)
    {
        var typeId = element.GetTypeId();
        if (typeId != ElementId.InvalidElementId && doc.GetElement(typeId) is ElementType et)
        {
            return et.Name;
        }

        return string.Empty;
    }

    private static string ResolveLevelName(Document doc, Element element)
    {
        var levelParam = element.get_Parameter(BuiltInParameter.LEVEL_PARAM)
            ?? element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
        if (levelParam is null || levelParam.AsElementId() == ElementId.InvalidElementId)
        {
            return string.Empty;
        }

        return doc.GetElement(levelParam.AsElementId())?.Name ?? string.Empty;
    }

    private static GeomPoint ElementCenter(Element element)
    {
        var box = element.get_BoundingBox(null);
        if (box is not null)
        {
            return ToGeomPoint((box.Min + box.Max) * 0.5);
        }

        return element.Location is LocationPoint lp ? ToGeomPoint(lp.Point) : new GeomPoint();
    }

    private static string ParameterValueToString(Document doc, Parameter p)
    {
        try
        {
            switch (p.StorageType)
            {
                case StorageType.String:
                    return p.AsString() ?? string.Empty;
                case StorageType.Integer:
                    return p.AsValueString() ?? p.AsInteger().ToString(CultureInfo.InvariantCulture);
                case StorageType.Double:
                    return p.AsValueString() ?? p.AsDouble().ToString("G17", CultureInfo.InvariantCulture);
                case StorageType.ElementId:
                {
                    var id = p.AsElementId();
                    if (id == ElementId.InvalidElementId || id.Value <= 0)
                    {
                        return string.Empty;
                    }

                    return doc.GetElement(id)?.Name ?? id.Value.ToString(CultureInfo.InvariantCulture);
                }

                default:
                    return string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    // Every readable instance parameter, string-coerced. First writer wins on duplicate names
    // (case-insensitive), mirroring BuildElementDetail's parameter projection.
    private static Dictionary<string, string> ReadInstanceParameters(Document doc, Element element)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Parameter p in element.Parameters)
        {
            if (p?.Definition is null || !p.HasValue)
            {
                continue;
            }

            var key = p.Definition.Name;
            if (string.IsNullOrWhiteSpace(key) || parameters.ContainsKey(key))
            {
                continue;
            }

            parameters[key] = ParameterValueToString(doc, p);
        }

        return parameters;
    }

    // Resolves a single parameter value by name for filter evaluation: instance parameter
    // first, then the element type's parameter. Name matching is case-insensitive. Returns ""
    // when absent or valueless. Reads only the named parameter (not every parameter on the
    // element), which is what keeps query_elements tractable on very large models.
    private static string ResolveParameterValueByName(Document doc, Element element, string name)
    {
        var instanceValue = TryReadNamedParameter(doc, element, name);
        if (instanceValue is not null)
        {
            return instanceValue;
        }

        var typeId = element.GetTypeId();
        if (typeId != ElementId.InvalidElementId && doc.GetElement(typeId) is Element typeElement)
        {
            var typeValue = TryReadNamedParameter(doc, typeElement, name);
            if (typeValue is not null)
            {
                return typeValue;
            }
        }

        return string.Empty;
    }

    // Returns the string-coerced value of the named parameter on this element, or null if the
    // element has no such parameter. Fast path: LookupParameter (exact-case, hashed). Only on
    // a miss does it fall back to a case-insensitive scan, honouring the case-insensitive
    // matching contract without paying for an enumeration on the common exact-case call.
    private static string? TryReadNamedParameter(Document doc, Element element, string name)
    {
        var direct = element.LookupParameter(name);
        if (direct is not null)
        {
            return direct.HasValue ? ParameterValueToString(doc, direct) : string.Empty;
        }

        foreach (Parameter p in element.Parameters)
        {
            if (p?.Definition is null)
            {
                continue;
            }

            if (string.Equals(p.Definition.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return p.HasValue ? ParameterValueToString(doc, p) : string.Empty;
            }
        }

        return null;
    }

    // Case-insensitive comparison for the four supported operators.
    private static bool MatchesParameterFilter(string actual, ParameterFilter filter)
    {
        var target = filter.Value ?? string.Empty;
        var op = (filter.Operator ?? "equals").Trim();

        if (op.Equals("equals", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(actual, target, StringComparison.OrdinalIgnoreCase);
        }

        if (op.Equals("notEquals", StringComparison.OrdinalIgnoreCase))
        {
            return !string.Equals(actual, target, StringComparison.OrdinalIgnoreCase);
        }

        if (op.Equals("contains", StringComparison.OrdinalIgnoreCase))
        {
            return actual.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (op.Equals("startsWith", StringComparison.OrdinalIgnoreCase))
        {
            return actual.StartsWith(target, StringComparison.OrdinalIgnoreCase);
        }

        throw new InvalidOperationException(
            $"Unknown operator '{filter.Operator}'. Use equals, contains, startsWith, or notEquals.");
    }
}
