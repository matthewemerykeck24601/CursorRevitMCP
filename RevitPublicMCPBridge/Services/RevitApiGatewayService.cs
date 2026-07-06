using System.IO;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitPublicMCPBridge.Settings;
using RevitPublicMCPBridge.Utilities;

namespace RevitPublicMCPBridge.Services;

public sealed class RevitApiGatewayService
{
    private readonly SettingsStore _settingsStore;
    private readonly BridgeLogger _logger;
    private readonly RevitRequestDispatcher _dispatcher;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public RevitApiGatewayService(
        SettingsStore settingsStore,
        BridgeLogger logger,
        RevitRequestDispatcher dispatcher)
    {
        _settingsStore = settingsStore;
        _logger = logger;
        _dispatcher = dispatcher;
    }

    public bool IsRunning => _listener?.IsListening == true;

    public int Port => _settingsStore.Current.LocalApiGatewayPort;

    public string BaseUrl => $"http://127.0.0.1:{Port}";

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        if (!_settingsStore.Current.EnableLocalApiGateway)
        {
            _logger.Info("Local API gateway disabled by settings.");
            return;
        }

        var listener = new HttpListener();
        var prefix = $"{BaseUrl}/";
        listener.Prefixes.Add(prefix);
        listener.Start();

        _cts = new CancellationTokenSource();
        _listener = listener;
        _loopTask = Task.Run(() => AcceptLoop(listener, _cts.Token));
        _logger.Info($"Local API gateway listening on {prefix}");
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
                    Data = new { gateway = "revit-public-mcp-bridge", revitConnected },
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
                case "/api/get_current_view_elements":
                    await HandleTyped<CurrentViewElementsRequest, List<CurrentViewElementRecord>>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetCurrentViewElements(app, req)));
                    return;
                case "/api/get_available_family_types":
                    await HandleTyped<FamilyTypesRequest, List<FamilyTypeRecord>>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetAvailableFamilyTypes(app, req)));
                    return;
                case "/api/create_point_based_element":
                    await HandleTyped<CreatePointElementsRequest, List<CreatePointElementResult>>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreatePointElements(app, req), timeoutMs: 30000));
                    return;
                // ─── Toolbox expansion routes (domain-prefixed to avoid family/project leaf-name collisions) ───
                case "/api/family/create_extrusion":
                    await HandleTyped<CreateExtrusionRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateExtrusion(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/create_reference_plane":
                    await HandleTyped<CreateReferencePlaneRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateReferencePlane(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_wall":
                    await HandleTyped<CreateWallRequest, CreateWallResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateWall(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_level":
                    await HandleTyped<CreateLevelRequest, CreateLevelResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateLevel(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_grid":
                    await HandleTyped<CreateGridRequest, CreateGridResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateGrid(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_floor":
                    await HandleTyped<CreateFloorRequest, CreateFloorResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateFloor(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_room":
                    await HandleTyped<CreateRoomRequest, CreateRoomResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateRoom(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_structural_column":
                    await HandleTyped<CreateStructuralColumnRequest, CreateStructuralColumnResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateStructuralColumn(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_beam":
                    await HandleTyped<CreateBeamRequest, CreateBeamResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateBeam(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_point_based_element":
                    await HandleTyped<CreatePointBasedElementRequest, CreatePointBasedElementResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreatePointBasedElement(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_opening_by_boundary":
                    await HandleTyped<CreateOpeningByBoundaryRequest, CreateOpeningByBoundaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateOpeningByBoundary(app, req), timeoutMs: 30000));
                    return;
                // ─── Group 7: Modify & Edit ───
                case "/api/project/move_elements":
                    await HandleTyped<MoveElementsRequest, MoveElementsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => MoveElements(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/rotate_elements":
                    await HandleTyped<RotateElementsRequest, RotateElementsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => RotateElements(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/mirror_elements":
                    await HandleTyped<MirrorElementsRequest, MirrorElementsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => MirrorElements(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/copy_elements_to_level":
                    await HandleTyped<CopyElementsToLevelRequest, CopyElementsToLevelResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CopyElementsToLevel(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/set_element_parameters":
                    await HandleTyped<SetElementParametersRequest, SetElementParametersResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SetElementParameters(app, req), timeoutMs: 10000));
                    return;
                case "/api/project/delete_elements":
                    await HandleTyped<DeleteElementsRequest, DeleteElementsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => DeleteElements(app, req), timeoutMs: 30000));
                    return;
                // ─── Group 8: Views, Sheets & Sheet Audit ───
                case "/api/project/create_floor_plan_view":
                    await HandleTyped<CreatePlanViewRequest, CreatePlanViewResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateFloorPlanView(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_reflected_ceiling_plan":
                    await HandleTyped<CreatePlanViewRequest, CreatePlanViewResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateReflectedCeilingPlan(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_section_view":
                    await HandleTyped<CreateSectionViewRequest, CreateViewResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateSectionView(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_3d_view":
                    await HandleTyped<Create3DViewRequest, CreateViewResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => Create3DView(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/create_sheet":
                    await HandleTyped<CreateSheetRequest, CreateSheetResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateSheet(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/place_view_on_sheet":
                    await HandleTyped<PlaceViewOnSheetRequest, PlaceViewOnSheetResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => PlaceViewOnSheet(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/set_view_crop_region":
                    await HandleTyped<SetViewCropRegionRequest, SetViewCropRegionResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SetViewCropRegion(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/get_sheets":
                    await HandleTyped<object, GetSheetsResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(app => GetSheets(app), timeoutMs: 10000));
                    return;
                case "/api/project/get_sheet_contents":
                    await HandleTyped<GetSheetContentsRequest, GetSheetContentsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetSheetContents(app, req), timeoutMs: 10000));
                    return;
                case "/api/project/open_sheet":
                    await HandleTyped<OpenSheetRequest, OpenSheetResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => OpenSheet(app, req), timeoutMs: 10000));
                    return;
                case "/api/project/get_view_contents":
                    await HandleTyped<GetViewContentsRequest, GetViewContentsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetViewContents(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/compare_sheet_to_template":
                    await HandleTyped<CompareSheetToTemplateRequest, CompareSheetToTemplateResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CompareSheetToTemplate(app, req), timeoutMs: 30000));
                    return;
                // ─── Group 5: Geometry Read & In-Place Extraction (read-only) ───
                case "/api/project/get_active_context":
                    await HandleTyped<object, GetActiveContextResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(app => GetActiveContext(app), timeoutMs: 10000));
                    return;
                case "/api/project/get_element_geometry":
                    await HandleTyped<GetElementGeometryRequest, GetElementGeometryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetElementGeometry(app, req), timeoutMs: 30000));
                    return;
                case "/api/project/get_inplace_elements":
                    await HandleTyped<GetInplaceElementsRequest, GetInplaceElementsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetInplaceElements(app, req), timeoutMs: 10000));
                    return;
                case "/api/project/reconstruct_profile":
                    await HandleTyped<ReconstructProfileRequest, ReconstructProfileResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ReconstructProfile(app, req), timeoutMs: 10000));
                    return;
                // ─── Group 11: read-only project query tools ───
                case "/api/project/get_levels":
                    await HandleTyped<object, GetLevelsResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(app => GetLevels(app), timeoutMs: 10000));
                    return;
                case "/api/project/get_grids":
                    await HandleTyped<object, GetGridsResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(app => GetGrids(app), timeoutMs: 10000));
                    return;
                case "/api/project/get_element_types":
                    await HandleTyped<GetElementTypesRequest, GetElementTypesResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetElementTypes(app, req), timeoutMs: 10000));
                    return;
                case "/api/project/query_elements":
                    await HandleTyped<QueryElementsRequest, QueryElementsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => QueryElements(app, req), timeoutMs: 10000));
                    return;
                case "/api/project/get_element_by_id":
                    await HandleTyped<GetElementByIdRequest, GetElementByIdResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => GetElementById(app, req), timeoutMs: 10000));
                    return;
                // ─── Model Snapshot Export (v18) — read-only, writes JSON to disk from C#;
                //     only a small summary crosses the MCP boundary. 120s for full-model iteration.
                case "/api/project/export_instances":
                    await HandleTyped<ExportInstancesRequest, ExportSummaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExportInstances(app, req), timeoutMs: 120000));
                    return;
                case "/api/project/export_type_parameters":
                    await HandleTyped<ExportTypeParametersRequest, ExportSummaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExportTypeParameters(app, req), timeoutMs: 120000));
                    return;
                case "/api/project/export_parameter_bindings":
                    await HandleTyped<ExportParameterBindingsRequest, ExportSummaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExportParameterBindings(app, req), timeoutMs: 120000));
                    return;
                case "/api/project/export_materials":
                    await HandleTyped<ExportMaterialsRequest, ExportSummaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExportMaterials(app, req), timeoutMs: 120000));
                    return;
                case "/api/project/export_views":
                    await HandleTyped<ExportViewsRequest, ExportSummaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExportViews(app, req), timeoutMs: 120000));
                    return;
                // ─── Instance Import (v20) — Revit 2024 rebuild driver. Long-running:
                //     600s dispatcher allowance for execute; chunked transactions with a
                //     soft time budget so a partial run returns cleanly and resumes.
                case "/api/project/import_instances":
                    await HandleTyped<ImportInstancesRequest, ImportSummaryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ImportInstances(app, req), timeoutMs: 600000));
                    return;
                // v20.1: batch datum creation from levels_grids.json (rebuild reference frame).
                case "/api/project/create_datums":
                    await HandleTyped<CreateDatumsRequest, CreateDatumsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateDatums(app, req), timeoutMs: 180000));
                    return;
                case "/api/family/create_blend":
                    await HandleTyped<CreateBlendRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateBlend(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/create_revolve":
                    await HandleTyped<CreateRevolveRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateRevolve(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/create_sweep":
                    await HandleTyped<CreateSweepRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateSweep(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/create_swept_blend":
                    await HandleTyped<CreateSweptBlendRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateSweptBlend(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/set_geometry_solid_void":
                    await HandleTyped<SetGeometrySolidVoidRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SetGeometrySolidVoid(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/create_dimension":
                    await HandleTyped<CreateDimensionRequest, ElementCreationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => CreateDimension(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/set_dimension_label":
                    await HandleTyped<SetDimensionLabelRequest, OperationResult>(
                        context,
                        body,
                        // Tight 10s timeout: labeling can stall the Revit thread on a bad
                        // parameter, so bound the wait and return a structured error.
                        req => _dispatcher.Enqueue(app => SetDimensionLabel(app, req), timeoutMs: 10000));
                    return;
                case "/api/family/lock_constraint":
                    await HandleTyped<LockConstraintRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => LockConstraint(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/add_parameter":
                    await HandleTyped<AddFamilyParameterRequest, AddFamilyParameterResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => AddFamilyParameter(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/set_parameter_value":
                    await HandleTyped<SetFamilyParameterValueRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SetFamilyParameterValue(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/add_type":
                    await HandleTyped<AddFamilyTypeRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => AddFamilyType(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/rename_type":
                    await HandleTyped<RenameFamilyTypeRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => RenameFamilyType(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/delete_type":
                    await HandleTyped<DeleteFamilyTypeRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => DeleteFamilyType(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/set_formula":
                    await HandleTyped<SetFormulaRequest, OperationResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SetFormula(app, req), timeoutMs: 30000));
                    return;
                case "/api/family/save":
                    await HandleTyped<object, SaveFamilyResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(app => SaveFamily(app), timeoutMs: 120000));
                    return;
                case "/api/family/save_as":
                    await HandleTyped<SaveFamilyAsRequest, SaveFamilyResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SaveFamilyAs(app, req), timeoutMs: 120000));
                    return;
                case "/api/family/load_into_project":
                    await HandleTyped<LoadFamilyIntoProjectRequest, LoadFamilyIntoProjectResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => LoadFamilyIntoProject(app, req), timeoutMs: 120000));
                    return;
                case "/api/family/get_document_info":
                    await HandleTyped<object, FamilyDocumentInfoResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(app => GetFamilyDocumentInfo(app)));
                    return;
                case "/api/open_selected_family_editor":
                    await HandleTyped<OpenSelectedFamilyEditorRequest, OpenSelectedFamilyEditorResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => OpenSelectedFamilyEditor(app, req), timeoutMs: 30000));
                    return;
                case "/api/open_family_editor_by_element_id":
                    await HandleTyped<OpenFamilyEditorByElementIdRequest, OpenSelectedFamilyEditorResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => OpenFamilyEditorByElementId(app, req), timeoutMs: 30000));
                    return;
                case "/api/ensure_shared_parameters":
                    await HandleTyped<EnsureSharedParametersRequest, EnsureSharedParametersResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => EnsureSharedParameters(app, req), timeoutMs: 30000));
                    return;
                case "/api/bind_shared_parameters":
                    await HandleTyped<BindSharedParametersRequest, BindSharedParametersResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => BindSharedParameters(app, req), timeoutMs: 30000));
                    return;
                case "/api/search_family_library":
                    await HandleTyped<SearchFamilyLibraryRequest, SearchFamilyLibraryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => SearchFamilyLibrary(app, req), timeoutMs: 30000));
                    return;
                case "/api/open_family_from_library":
                    await HandleTyped<OpenFamilyFromLibraryRequest, OpenFamilyFromLibraryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => OpenFamilyFromLibrary(app, req), timeoutMs: 30000));
                    return;
                case "/api/load_family_from_library":
                    await HandleTyped<LoadFamilyFromLibraryRequest, LoadFamilyFromLibraryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => LoadFamilyFromLibrary(app, req), timeoutMs: 30000));
                    return;
                case "/api/upgrade_family_library_version":
                    await HandleTyped<UpgradeFamilyLibraryVersionRequest, UpgradeFamilyLibraryVersionResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => UpgradeFamilyLibraryVersion(app, req), timeoutMs: 120000));
                    return;
                case "/api/extract_family_library_parameters":
                    await HandleTyped<ExtractFamilyLibraryParametersRequest, ExtractFamilyLibraryParametersResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExtractFamilyLibraryParameters(app, req), timeoutMs: 240000));
                    return;
                case "/api/add_shared_parameters_to_family_library":
                    await HandleTyped<AddSharedParametersToFamilyLibraryRequest, AddSharedParametersToFamilyLibraryResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => AddSharedParametersToFamilyLibrary(app, req), timeoutMs: 240000));
                    return;
                case "/api/extract_family_desc_variants":
                    await HandleTyped<ExtractFamilyDescriptionVariantsRequest, ExtractFamilyDescriptionVariantsResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ExtractFamilyDescriptionVariants(app, req), timeoutMs: 300000));
                    return;
                case "/api/get_document_metadata":
                    await HandleTyped<object, DocumentMetadataResult>(
                        context,
                        body,
                        _ => _dispatcher.Enqueue(ElementProvenanceCollector.CollectDocumentMetadata));
                    return;
                case "/api/get_element_metadata":
                    await HandleTyped<ElementMetadataRequest, List<ElementMetadataRecord>>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => ElementProvenanceCollector.CollectElementMetadata(app, req)));
                    return;
                case "/api/get_family_parameters":
                    await HandleTyped<GetFamilyParametersRequest, GetFamilyParametersResult>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(app => FamilyDocumentParameterCollector.Collect(app, req)));
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
        // 3-arg overload is available on both net8 and net48 (ReadOnlyMemory overload is not on net48).
        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        context.Response.OutputStream.Close();
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

            var elementType = doc.GetElement(element.GetTypeId()) as ElementType;
            var levelName = ResolveLevelName(doc, element);
            var familyName = elementType is FamilySymbol fs
                ? fs.FamilyName
                : elementType?.FamilyName ?? string.Empty;

            results.Add(new CurrentViewElementRecord
            {
                ElementId = ToInt(element.Id),
                Name = element.Name,
                Category = categoryName,
                FamilyName = familyName,
                TypeName = elementType?.Name ?? string.Empty,
                LevelName = levelName,
            });

            if (request.Limit is > 0 && results.Count >= request.Limit.Value)
            {
                break;
            }
        }

        return results;
    }

    private static List<FamilyTypeRecord> GetAvailableFamilyTypes(
        UIApplication app,
        FamilyTypesRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (doc.IsFamilyDocument)
        {
            // Enumerating FamilySymbols against a live family editor document hangs. Fall back
            // to the first open project document (same pattern as LoadFamilyIntoProject).
            doc = app.Application.Documents
                .Cast<Document>()
                .FirstOrDefault(d => !d.IsFamilyDocument)
                ?? throw new InvalidOperationException(
                    "The family editor is active and no project document is open to enumerate family "
                    + "types from. Open a .rvt project first.");
        }

        var categorySet = (request.CategoryList ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var familyFilter = request.FamilyNameFilter?.Trim();

        var collector = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>();

        var list = new List<FamilyTypeRecord>();
        foreach (var symbol in collector)
        {
            var categoryToken = symbol.Category is null ? string.Empty : ToCategoryToken(symbol.Category);
            if (categorySet.Count > 0 && !categorySet.Contains(categoryToken))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(familyFilter)
                && !symbol.FamilyName.Contains(familyFilter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            list.Add(new FamilyTypeRecord
            {
                TypeId = ToInt(symbol.Id),
                TypeName = symbol.Name,
                FamilyName = symbol.FamilyName,
                Category = categoryToken,
            });

            if (request.Limit is > 0 && list.Count >= request.Limit.Value)
            {
                break;
            }
        }

        return list;
    }

    private static List<CreatePointElementResult> CreatePointElements(
        UIApplication app,
        CreatePointElementsRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (request.Data.Count == 0)
        {
            return [];
        }

        var levelByElevation = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();

        var created = new List<CreatePointElementResult>();
        using var tx = new Transaction(doc, "Bridge create point-based elements");
        tx.Start();

        foreach (var item in request.Data)
        {
            var symbol = doc.GetElement(new ElementId(item.TypeId)) as FamilySymbol
                ?? throw new InvalidOperationException($"FamilySymbol {item.TypeId} not found.");
            if (!symbol.IsActive)
            {
                symbol.Activate();
                doc.Regenerate();
            }

            var point = ToInternalPoint(item.LocationPoint);
            var level = ResolveClosestLevel(levelByElevation, MmToFeet(item.BaseLevel))
                ?? throw new InvalidOperationException("No levels available for placement.");
            var offsetFeet = MmToFeet(item.BaseOffset);

            var placed = PlaceFamilyInstance(doc, symbol, point, level, item.HostWallId);
            if (Math.Abs(item.Rotation) > 0.001)
            {
                var radians = item.Rotation * Math.PI / 180.0;
                var axis = Line.CreateBound(point, point + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, placed.Id, axis, radians);
            }

            if (item.FacingFlipped && placed.CanFlipFacing)
            {
                placed.flipFacing();
            }

            var offsetParam = placed.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM)
                ?? placed.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
            if (offsetParam is not null && !offsetParam.IsReadOnly)
            {
                offsetParam.Set(offsetFeet);
            }

            created.Add(new CreatePointElementResult
            {
                Name = item.Name,
                TypeId = item.TypeId,
                ElementId = ToInt(placed.Id),
                HostWallId = item.HostWallId,
            });
        }

        tx.Commit();
        return created;
    }

    private static FamilyInstance PlaceFamilyInstance(
        Document doc,
        FamilySymbol symbol,
        XYZ point,
        Level level,
        int? hostWallId)
    {
        if (hostWallId.HasValue)
        {
            var hostWall = doc.GetElement(new ElementId(hostWallId.Value)) as Wall
                ?? throw new InvalidOperationException($"Host wall {hostWallId.Value} was not found.");
            return doc.Create.NewFamilyInstance(point, symbol, hostWall, level, StructuralType.NonStructural);
        }

        return symbol.Family.FamilyPlacementType switch
        {
            FamilyPlacementType.OneLevelBasedHosted => throw new InvalidOperationException(
                "Family type is hosted and requires hostWallId."),
            _ => doc.Create.NewFamilyInstance(point, symbol, level, StructuralType.NonStructural),
        };
    }

    private static Level? ResolveClosestLevel(List<Level> levels, double targetElevation)
    {
        Level? best = null;
        var bestDistance = double.MaxValue;
        foreach (var level in levels)
        {
            var distance = Math.Abs(level.Elevation - targetElevation);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = level;
            }
        }

        return best;
    }

    private static XYZ ToInternalPoint(PointInput p)
    {
        return new XYZ(MmToFeet(p.X), MmToFeet(p.Y), MmToFeet(p.Z));
    }

    private static double MmToFeet(double mm) => mm / 304.8;

    private static double FeetToMm(double feet) => feet * 304.8;

    // Revit internal XYZ (feet) -> output GeomPoint (mm).
    private static GeomPoint ToGeomPoint(XYZ p) => new()
    {
        X = FeetToMm(p.X),
        Y = FeetToMm(p.Y),
        Z = FeetToMm(p.Z),
    };

    // ───────────────────────────────────────────────────────────────────────
    // Toolbox expansion — vertical slice (3 representative tools).
    // Pattern for the remaining 46: validate doc kind -> build geometry in
    // internal units -> create inside `using var tx` -> return ElementCreationResult.
    // ───────────────────────────────────────────────────────────────────────

    private static Document RequireFamilyDocument(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (!doc.IsFamilyDocument)
        {
            throw new InvalidOperationException(
                "Active document is not a Revit family. Open a .rfa file first.");
        }

        return doc;
    }

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

    // Resolves the work plane for sketch-based geometry. Named reference plane if
    // supplied, otherwise the family XY plane at the origin. Returned Plane is the
    // canonical frame used to map 2D profile points into world coordinates.
    private static Plane ResolveWorkPlane(Document doc, string? workPlaneName)
    {
        if (string.IsNullOrWhiteSpace(workPlaneName))
        {
            return Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
        }

        var name = workPlaneName.Trim();
        var refPlane = new FilteredElementCollector(doc)
            .OfClass(typeof(ReferencePlane))
            .Cast<ReferencePlane>()
            .FirstOrDefault(rp => string.Equals(rp.Name, name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Reference plane '{name}' was not found in the active family document.");

        return refPlane.GetPlane();
    }

    // Maps a closed list of 2D (u,v) mm points onto a plane, returning one closed CurveArray.
    private static CurveArray BuildCurveArrayLoop(Plane plane, List<PointInput> profile)
    {
        if (profile is null || profile.Count < 3)
        {
            throw new InvalidOperationException("profile must contain at least 3 points to form a closed loop.");
        }

        XYZ Map(PointInput p) =>
            plane.Origin + (MmToFeet(p.X) * plane.XVec) + (MmToFeet(p.Y) * plane.YVec);

        var loop = new CurveArray();
        for (var i = 0; i < profile.Count; i++)
        {
            var a = Map(profile[i]);
            var b = Map(profile[(i + 1) % profile.Count]);
            if (a.IsAlmostEqualTo(b))
            {
                throw new InvalidOperationException(
                    $"profile has a zero-length segment at index {i}; remove duplicate consecutive points.");
            }

            loop.Append(Line.CreateBound(a, b));
        }

        return loop;
    }

    // Single-loop profile wrapped in a CurveArrArray (the shape NewExtrusion/NewRevolution expect).
    private static CurveArrArray BuildPlanarLoop(Plane plane, List<PointInput> profile)
    {
        var profileArray = new CurveArrArray();
        profileArray.Append(BuildCurveArrayLoop(plane, profile));
        return profileArray;
    }

    // Builds a SweepProfile from a 2D profile sketched in its own XY frame at the origin.
    private static SweepProfile BuildSweepProfileFromXy(UIApplication app, List<PointInput> profile)
    {
        var xy = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
        return app.Application.Create.NewCurveLoopsProfile(BuildPlanarLoop(xy, profile));
    }

    private static List<XYZ> ToWorldPath(List<PointInput> path)
    {
        if (path is null || path.Count < 2)
        {
            throw new InvalidOperationException("path must contain at least 2 points.");
        }

        return path.Select(ToInternalPoint).ToList();
    }

    private static CurveArray BuildPathCurves(List<XYZ> pts)
    {
        var curves = new CurveArray();
        for (var i = 0; i < pts.Count - 1; i++)
        {
            if (pts[i].IsAlmostEqualTo(pts[i + 1]))
            {
                throw new InvalidOperationException($"path has a zero-length segment at index {i}.");
            }

            curves.Append(Line.CreateBound(pts[i], pts[i + 1]));
        }

        return curves;
    }

    // A plane that contains the (assumed planar) path polyline. Falls back to an
    // arbitrary plane through a straight path when no three points are non-collinear.
    private static Plane PlaneContainingPath(List<XYZ> pts)
    {
        for (var i = 2; i < pts.Count; i++)
        {
            try
            {
                return Plane.CreateByThreePoints(pts[0], pts[1], pts[i]);
            }
            catch
            {
                // Collinear triple — keep looking.
            }
        }

        var dir = (pts[^1] - pts[0]).Normalize();
        var helper = Math.Abs(dir.DotProduct(XYZ.BasisZ)) > 0.99 ? XYZ.BasisX : XYZ.BasisZ;
        var normal = dir.CrossProduct(helper).Normalize();
        return Plane.CreateByNormalAndOrigin(normal, pts[0]);
    }

    private static ElementCreationResult CreateExtrusion(UIApplication app, CreateExtrusionRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var depthFeet = MmToFeet(request.Depth);
        if (Math.Abs(depthFeet) < 1e-9)
        {
            throw new InvalidOperationException("depth must be non-zero.");
        }

        var plane = ResolveWorkPlane(doc, request.WorkPlaneName);

        using var tx = new Transaction(doc, "Bridge create extrusion");
        tx.Start();
        var sketchPlane = SketchPlane.Create(doc, plane);
        var profile = BuildPlanarLoop(plane, request.Profile);
        var extrusion = doc.FamilyCreate.NewExtrusion(request.IsSolid, profile, sketchPlane, depthFeet);
        doc.Regenerate();
        tx.Commit();

        return new ElementCreationResult { ElementId = ToInt(extrusion.Id), Success = true };
    }

    private static ElementCreationResult CreateReferencePlane(UIApplication app, CreateReferencePlaneRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("name is required.");
        }

        var bubble = ToInternalPoint(request.BubbleEnd);
        var free = ToInternalPoint(request.FreeEnd);
        if (bubble.IsAlmostEqualTo(free))
        {
            throw new InvalidOperationException("bubbleEnd and freeEnd must differ.");
        }

        // NewReferencePlane2 takes an in-plane third point. We derive it from cutVector
        // (bubble + cutVector) when supplied, else default to a vertical plane (bubble + Z).
        var thirdPnt = request.CutVector is not null
            ? bubble + new XYZ(
                MmToFeet(request.CutVector.X),
                MmToFeet(request.CutVector.Y),
                MmToFeet(request.CutVector.Z))
            : bubble + XYZ.BasisZ;

        var view = doc.ActiveView
            ?? throw new InvalidOperationException("No active view available to host the reference plane.");

        using var tx = new Transaction(doc, "Bridge create reference plane");
        tx.Start();
        var refPlane = doc.FamilyCreate.NewReferencePlane2(bubble, free, thirdPnt, view);
        refPlane.Name = request.Name.Trim();
        TrySetReferencePlaneIsReference(refPlane, request.IsReference);
        doc.Regenerate();
        tx.Commit();

        return new ElementCreationResult { ElementId = ToInt(refPlane.Id), Success = true };
    }

    // Maps the MCP "isReference" enum onto the ELEM_REFERENCE_PLANE_IS_REFERENCE integer.
    // NOTE: this integer mapping is Revit-version sensitive, and "Center" is ambiguous
    // (Left/Right vs Front/Back vs Elevation) — verify in-product when scaling. Best-effort:
    // a failure here does not fail reference-plane creation.
    private static void TrySetReferencePlaneIsReference(ReferencePlane refPlane, string? isReference)
    {
        if (string.IsNullOrWhiteSpace(isReference))
        {
            return;
        }

        int? value = isReference.Trim() switch
        {
            "NotAReference" => 0,
            "StrongReference" => 1,
            "WeakReference" => 2,
            "Left" => 3,
            "Center" => 4,
            "Right" => 5,
            "Front" => 6,
            "Back" => 8,
            "Bottom" => 9,
            "Top" => 11,
            _ => null,
        };

        if (value is null)
        {
            return;
        }

        try
        {
            var param = refPlane.get_Parameter(BuiltInParameter.ELEM_IS_REFERENCE);
            if (param is not null && !param.IsReadOnly)
            {
                param.Set(value.Value);
            }
        }
        catch
        {
            // Best-effort: keep the created plane even if the dropdown cannot be set.
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    // Group 6 — Project model element creation. Every handler resolves the active
    // project document (RequireProjectDocument guards the family-editor context),
    // accepts level/type references by id, takes spatial inputs in mm, and wraps
    // all creation in a Transaction.
    // ───────────────────────────────────────────────────────────────────────

    private static XYZ MmPoint(double xMm, double yMm, double zMm) =>
        new(MmToFeet(xMm), MmToFeet(yMm), MmToFeet(zMm));

    // The next level whose elevation sits above the given base level, or null when
    // the base is already the topmost level. Used for level-to-level constraints.
    private static Level? NextLevelAbove(Document doc, Level baseLevel)
    {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .Where(l => l.Elevation > baseLevel.Elevation + 1e-6)
            .OrderBy(l => l.Elevation)
            .FirstOrDefault();
    }

    // Closed CurveLoop from an XY mm boundary, placed at the given elevation (feet).
    private static CurveLoop BuildCurveLoopFromXy(List<PointInput> profile, double elevationFeet)
    {
        if (profile is null || profile.Count < 3)
        {
            throw new InvalidOperationException("profile must contain at least 3 points to form a closed boundary loop.");
        }

        var pts = profile.Select(p => new XYZ(MmToFeet(p.X), MmToFeet(p.Y), elevationFeet)).ToList();
        var loop = new CurveLoop();
        for (var i = 0; i < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Count];
            if (a.IsAlmostEqualTo(b))
            {
                throw new InvalidOperationException(
                    $"profile has a zero-length segment at index {i}; remove duplicate consecutive points.");
            }

            loop.Append(Line.CreateBound(a, b));
        }

        return loop;
    }

    private static CreateLevelResult CreateLevel(UIApplication app, CreateLevelRequest request)
    {
        var doc = RequireProjectDocument(app);
        var elevationFeet = MmToFeet(request.ElevationMm);

        using var tx = new Transaction(doc, "Bridge create level");
        tx.Start();
        var level = Level.Create(doc, elevationFeet)
            ?? throw new InvalidOperationException("Level.Create returned null.");
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            try
            {
                level.Name = request.Name!.Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Level created but name '{request.Name}' could not be set (may be a duplicate): {ex.Message}");
            }
        }

        doc.Regenerate();
        tx.Commit();

        return new CreateLevelResult
        {
            Id = ToInt(level.Id),
            Name = level.Name,
            ElevationMm = FeetToMm(level.Elevation),
        };
    }

    private static CreateGridResult CreateGrid(UIApplication app, CreateGridRequest request)
    {
        var doc = RequireProjectDocument(app);

        // Grids are defined by a horizontal line in plan; Z is not meaningful for the defining curve.
        var start = MmPoint(request.StartX, request.StartY, 0);
        var end = MmPoint(request.EndX, request.EndY, 0);
        if (start.IsAlmostEqualTo(end))
        {
            throw new InvalidOperationException("Grid start and end points must differ.");
        }

        var line = Line.CreateBound(start, end);

        using var tx = new Transaction(doc, "Bridge create grid");
        tx.Start();
        var grid = Grid.Create(doc, line)
            ?? throw new InvalidOperationException("Grid.Create returned null.");
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            try
            {
                grid.Name = request.Name!.Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Grid created but name '{request.Name}' could not be set (may be a duplicate): {ex.Message}");
            }
        }

        doc.Regenerate();
        tx.Commit();

        return new CreateGridResult
        {
            Id = ToInt(grid.Id),
            Name = grid.Name,
            Start = ToGeomPoint(start),
            End = ToGeomPoint(end),
        };
    }

    private static CreateWallResult CreateWall(UIApplication app, CreateWallRequest request)
    {
        var doc = RequireProjectDocument(app);

        var start = MmPoint(request.StartX, request.StartY, request.StartZ);
        var end = MmPoint(request.EndX, request.EndY, request.EndZ);
        if (start.IsAlmostEqualTo(end))
        {
            throw new InvalidOperationException("Start and end points must differ.");
        }

        var levelId = new ElementId(request.LevelId);
        if (doc.GetElement(levelId) is not Level level)
        {
            throw new InvalidOperationException($"levelId {request.LevelId} does not resolve to a Level.");
        }

        ElementId wallTypeId;
        if (request.WallTypeId is int wt && wt > 0)
        {
            wallTypeId = new ElementId(wt);
            if (doc.GetElement(wallTypeId) is not WallType)
            {
                throw new InvalidOperationException($"wallTypeId {wt} does not resolve to a WallType.");
            }
        }
        else
        {
            wallTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.WallType);
            if (wallTypeId == ElementId.InvalidElementId)
            {
                throw new InvalidOperationException("No default wall type is set in this project; pass wallTypeId.");
            }
        }

        var line = Line.CreateBound(start, end);

        // Height: explicit when supplied; otherwise level-to-level (next level above),
        // falling back to 3000 mm when the base level is already the topmost level.
        double heightFeet;
        Level? upperLevel = null;
        if (request.Height is double h && h > 0)
        {
            heightFeet = MmToFeet(h);
        }
        else
        {
            upperLevel = NextLevelAbove(doc, level);
            heightFeet = upperLevel is not null
                ? upperLevel.Elevation - level.Elevation
                : MmToFeet(3000);
        }

        using var tx = new Transaction(doc, "Bridge create wall");
        tx.Start();
        var wall = Wall.Create(
            doc,
            line,
            wallTypeId,
            levelId,
            heightFeet,
            0,
            request.Flipped ?? false,
            request.Structural ?? false);
        if (request.Height is null && upperLevel is not null)
        {
            var topConstraint = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
            if (topConstraint is not null && !topConstraint.IsReadOnly)
            {
                topConstraint.Set(upperLevel.Id);
            }
        }

        doc.Regenerate();
        tx.Commit();

        var wallType = doc.GetElement(wallTypeId) as WallType;
        var lengthFeet = (wall.Location as LocationCurve)?.Curve.Length ?? line.Length;
        return new CreateWallResult
        {
            Id = ToInt(wall.Id),
            WallTypeName = wallType?.Name ?? string.Empty,
            LevelName = level.Name,
            LengthMm = FeetToMm(lengthFeet),
            HeightMm = FeetToMm(heightFeet),
        };
    }

    private static CreateFloorResult CreateFloor(UIApplication app, CreateFloorRequest request)
    {
        var doc = RequireProjectDocument(app);

        var levelId = new ElementId(request.LevelId);
        if (doc.GetElement(levelId) is not Level level)
        {
            throw new InvalidOperationException($"levelId {request.LevelId} does not resolve to a Level.");
        }

        ElementId floorTypeId;
        if (request.FloorTypeId is int ft && ft > 0)
        {
            floorTypeId = new ElementId(ft);
            if (doc.GetElement(floorTypeId) is not FloorType)
            {
                throw new InvalidOperationException($"floorTypeId {ft} does not resolve to a FloorType.");
            }
        }
        else
        {
            floorTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.FloorType);
            if (floorTypeId == ElementId.InvalidElementId)
            {
                throw new InvalidOperationException("No default floor type is set in this project; pass floorTypeId.");
            }
        }

        var loop = BuildCurveLoopFromXy(request.Profile, level.Elevation);

        using var tx = new Transaction(doc, "Bridge create floor");
        tx.Start();
        var floor = Floor.Create(
            doc,
            new List<CurveLoop> { loop },
            floorTypeId,
            levelId,
            request.Structural ?? false,
            null,
            0.0);
        doc.Regenerate();
        tx.Commit();

        var floorType = doc.GetElement(floorTypeId) as FloorType;
        var areaFt2 = floor.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED)?.AsDouble() ?? 0.0;
        return new CreateFloorResult
        {
            Id = ToInt(floor.Id),
            FloorTypeName = floorType?.Name ?? string.Empty,
            LevelName = level.Name,
            AreaMm2 = areaFt2 * 304.8 * 304.8,
        };
    }

    private static CreateRoomResult CreateRoom(UIApplication app, CreateRoomRequest request)
    {
        var doc = RequireProjectDocument(app);

        var levelId = new ElementId(request.LevelId);
        if (doc.GetElement(levelId) is not Level level)
        {
            throw new InvalidOperationException($"levelId {request.LevelId} does not resolve to a Level.");
        }

        using var tx = new Transaction(doc, "Bridge create room");
        tx.Start();
        var uv = new UV(MmToFeet(request.LocationX), MmToFeet(request.LocationY));
        var room = doc.Create.NewRoom(level, uv)
            ?? throw new InvalidOperationException(
                "Failed to create room. The placement point must lie inside a region bounded by "
                + "room-bounding walls or room separation lines on this level.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            room.get_Parameter(BuiltInParameter.ROOM_NAME)?.Set(request.Name.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Number))
        {
            room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.Set(request.Number.Trim());
        }

        doc.Regenerate();
        tx.Commit();

        return new CreateRoomResult
        {
            Id = ToInt(room.Id),
            Name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() ?? room.Name,
            Number = room.Number,
            LevelName = level.Name,
        };
    }

    private static CreateStructuralColumnResult CreateStructuralColumn(
        UIApplication app,
        CreateStructuralColumnRequest request)
    {
        var doc = RequireProjectDocument(app);

        var symbol = doc.GetElement(new ElementId(request.FamilyTypeId)) as FamilySymbol
            ?? throw new InvalidOperationException(
                $"familyTypeId {request.FamilyTypeId} does not resolve to a FamilySymbol. Use get_element_types to find one.");

        var levelId = new ElementId(request.LevelId);
        if (doc.GetElement(levelId) is not Level level)
        {
            throw new InvalidOperationException($"levelId {request.LevelId} does not resolve to a Level.");
        }

        var point = MmPoint(request.LocationX, request.LocationY, request.LocationZ);

        using var tx = new Transaction(doc, "Bridge create structural column");
        tx.Start();
        if (!symbol.IsActive)
        {
            symbol.Activate();
            doc.Regenerate();
        }

        var column = doc.Create.NewFamilyInstance(point, symbol, level, StructuralType.Column);

        // Explicit height -> constrain the top to the base level + offset. Otherwise leave
        // the family's default top constraint (typically level-to-level).
        if (request.HeightMm is double hmm && hmm > 0)
        {
            var topLevelParam = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
            if (topLevelParam is not null && !topLevelParam.IsReadOnly)
            {
                topLevelParam.Set(levelId);
            }

            var topOffsetParam = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
            if (topOffsetParam is not null && !topOffsetParam.IsReadOnly)
            {
                topOffsetParam.Set(MmToFeet(hmm));
            }
        }

        doc.Regenerate();
        tx.Commit();

        return new CreateStructuralColumnResult
        {
            Id = ToInt(column.Id),
            TypeName = symbol.Name,
            LevelName = level.Name,
            LocationMm = ToGeomPoint(point),
        };
    }

    private static CreateBeamResult CreateBeam(UIApplication app, CreateBeamRequest request)
    {
        var doc = RequireProjectDocument(app);

        var symbol = doc.GetElement(new ElementId(request.FamilyTypeId)) as FamilySymbol
            ?? throw new InvalidOperationException(
                $"familyTypeId {request.FamilyTypeId} does not resolve to a FamilySymbol. Use get_element_types to find one.");

        // NewFamilyInstance(line, symbol, level, StructuralType.Beam) throws an opaque
        // NullReferenceException when handed a non-StructuralFraming type. Reject anything
        // outside OST_StructuralFraming up front with a clear message. Any StructuralFraming
        // symbol is valid (girders, joists, panels, etc.) — do NOT restrict beyond category.
        var symbolCategoryToken = symbol.Category is not null ? ToCategoryToken(symbol.Category) : string.Empty;
        if (!string.Equals(symbolCategoryToken, "OST_StructuralFraming", StringComparison.Ordinal))
        {
            var actualCategoryName = symbol.Category?.Name ?? "unknown";
            throw new InvalidOperationException(
                $"familyTypeId {request.FamilyTypeId} is category {actualCategoryName}; create_beam requires "
                + "OST_StructuralFraming. Use get_element_types with category=OST_StructuralFraming to find valid type IDs.");
        }

        var levelId = new ElementId(request.LevelId);
        if (doc.GetElement(levelId) is not Level level)
        {
            throw new InvalidOperationException($"levelId {request.LevelId} does not resolve to a Level.");
        }

        var start = MmPoint(request.StartX, request.StartY, request.StartZ);
        var end = MmPoint(request.EndX, request.EndY, request.EndZ);
        if (start.IsAlmostEqualTo(end))
        {
            throw new InvalidOperationException("Start and end points must differ.");
        }

        var line = Line.CreateBound(start, end);

        using var tx = new Transaction(doc, "Bridge create beam");
        tx.Start();
        if (!symbol.IsActive)
        {
            symbol.Activate();
            doc.Regenerate();
        }

        var beam = doc.Create.NewFamilyInstance(line, symbol, level, StructuralType.Beam);
        doc.Regenerate();
        tx.Commit();

        var lengthFeet = (beam.Location as LocationCurve)?.Curve.Length ?? line.Length;
        return new CreateBeamResult
        {
            Id = ToInt(beam.Id),
            TypeName = symbol.Name,
            LevelName = level.Name,
            LengthMm = FeetToMm(lengthFeet),
        };
    }

    private static CreatePointBasedElementResult CreatePointBasedElement(
        UIApplication app,
        CreatePointBasedElementRequest request)
    {
        var doc = RequireProjectDocument(app);

        var symbol = doc.GetElement(new ElementId(request.FamilyTypeId)) as FamilySymbol
            ?? throw new InvalidOperationException(
                $"familyTypeId {request.FamilyTypeId} does not resolve to a FamilySymbol. Use get_element_types to find one.");

        var point = MmPoint(request.LocationX, request.LocationY, request.LocationZ);

        Level level;
        if (request.LevelId is int lid && lid > 0)
        {
            level = doc.GetElement(new ElementId(lid)) as Level
                ?? throw new InvalidOperationException($"levelId {lid} does not resolve to a Level.");
        }
        else
        {
            var levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            level = ResolveClosestLevel(levels, point.Z)
                ?? throw new InvalidOperationException("No levels available for placement.");
        }

        using var tx = new Transaction(doc, "Bridge create point-based element");
        tx.Start();
        if (!symbol.IsActive)
        {
            symbol.Activate();
            doc.Regenerate();
        }

        var instance = doc.Create.NewFamilyInstance(point, symbol, level, StructuralType.NonStructural);
        if (request.RotationDeg is double deg && Math.Abs(deg) > 1e-9)
        {
            var radians = deg * Math.PI / 180.0;
            var axis = Line.CreateBound(point, point + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, instance.Id, axis, radians);
        }

        doc.Regenerate();
        tx.Commit();

        return new CreatePointBasedElementResult
        {
            Id = ToInt(instance.Id),
            TypeName = symbol.Name,
            FamilyName = symbol.Family?.Name ?? string.Empty,
            LevelName = level.Name,
            LocationMm = ToGeomPoint(point),
        };
    }

    private static CreateOpeningByBoundaryResult CreateOpeningByBoundary(
        UIApplication app,
        CreateOpeningByBoundaryRequest request)
    {
        var doc = RequireProjectDocument(app);

        var host = doc.GetElement(new ElementId(request.HostId))
            ?? throw new InvalidOperationException($"hostId {request.HostId} was not found.");
        if (request.Profile is null || request.Profile.Count < 3)
        {
            throw new InvalidOperationException("profile must contain at least 3 points to form a closed boundary loop.");
        }

        var hostCategory = host.Category is not null ? ToCategoryToken(host.Category) : string.Empty;

        var curves = new CurveArray();
        var pts = request.Profile.Select(p => MmPoint(p.X, p.Y, p.Z)).ToList();
        for (var i = 0; i < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Count];
            if (a.IsAlmostEqualTo(b))
            {
                throw new InvalidOperationException(
                    $"profile has a zero-length segment at index {i}; remove duplicate consecutive points.");
            }

            curves.Append(Line.CreateBound(a, b));
        }

        using var tx = new Transaction(doc, "Bridge create opening");
        tx.Start();
        Opening opening;
        switch (host)
        {
            case RoofBase:
            case Floor:
            case Ceiling:
                // NewOpening(host, profile, bPerpendicularFace) creates a boundary opening
                // in a roof, floor, or ceiling.
                opening = doc.Create.NewOpening(host, curves, request.ProjectOntHost ?? true);
                break;
            default:
                tx.RollBack();
                throw new InvalidOperationException(
                    $"Host element {request.HostId} ({hostCategory}) is not a roof, floor, or ceiling. "
                    + "Boundary openings are supported on roofs, floors, and ceilings; wall openings use a "
                    + "rectangular two-point API and are not handled by this tool.");
        }

        doc.Regenerate();
        tx.Commit();

        return new CreateOpeningByBoundaryResult
        {
            Id = ToInt(opening.Id),
            HostId = request.HostId,
            HostCategory = hostCategory,
        };
    }

    // ───────────────────────────────────────────────────────────────────────
    // Group 7 — Modify & Edit. Each handler resolves the active project document,
    // validates every input id, and wraps the mutation in a single Transaction.
    // ───────────────────────────────────────────────────────────────────────

    // Validates a non-empty id list and that every id resolves to an element.
    private static List<ElementId> ResolveElementIds(Document doc, List<int> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            throw new InvalidOperationException("elementIds is required and must contain at least one id.");
        }

        var resolved = new List<ElementId>();
        foreach (var id in ids.Distinct())
        {
            var eid = new ElementId(id);
            if (doc.GetElement(eid) is null)
            {
                throw new InvalidOperationException($"Element {id} was not found.");
            }

            resolved.Add(eid);
        }

        return resolved;
    }

    private static XYZ ResolveAxisVector(string axis)
    {
        return (axis ?? "Z").Trim().ToUpperInvariant() switch
        {
            "X" => XYZ.BasisX,
            "Y" => XYZ.BasisY,
            "Z" => XYZ.BasisZ,
            _ => throw new InvalidOperationException($"Unknown axis '{axis}'. Use X, Y, or Z."),
        };
    }

    // Best-effort elevation of the level an element is associated with; falls back to the
    // element's bounding-box bottom. Used to align level-to-level copies.
    private static double ResolveElementLevelElevation(Document doc, Element element)
    {
        var levelParam = element.get_Parameter(BuiltInParameter.LEVEL_PARAM)
            ?? element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM)
            ?? element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
            ?? element.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
        if (levelParam is not null && levelParam.StorageType == StorageType.ElementId)
        {
            var lid = levelParam.AsElementId();
            if (lid != ElementId.InvalidElementId && doc.GetElement(lid) is Level lvl)
            {
                return lvl.Elevation;
            }
        }

        if (element.LevelId != ElementId.InvalidElementId && doc.GetElement(element.LevelId) is Level direct)
        {
            return direct.Elevation;
        }

        return element.get_BoundingBox(null)?.Min.Z ?? 0.0;
    }

    // Sets an instance parameter from a JSON value, converting mm->feet (Length) and
    // degrees->radians (Angle). Throws a short reason string on a type mismatch.
    private static void SetInstanceParameterValue(Parameter p, JsonElement value)
    {
        switch (p.StorageType)
        {
            case StorageType.Double:
            {
                double raw;
                if (value.ValueKind == JsonValueKind.Number)
                {
                    raw = value.GetDouble();
                }
                else if (value.ValueKind == JsonValueKind.String
                         && double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    raw = parsed;
                }
                else
                {
                    throw new InvalidOperationException("expects a numeric value");
                }

                var dataType = p.Definition.GetDataType();
                var converted = dataType == SpecTypeId.Length ? MmToFeet(raw)
                    : dataType == SpecTypeId.Angle ? raw * Math.PI / 180.0
                    : raw;
                p.Set(converted);
                break;
            }

            case StorageType.Integer:
            {
                int iv = value.ValueKind switch
                {
                    JsonValueKind.True => 1,
                    JsonValueKind.False => 0,
                    JsonValueKind.Number => (int)Math.Round(value.GetDouble()),
                    JsonValueKind.String when int.TryParse(value.GetString(), out var pi) => pi,
                    _ => throw new InvalidOperationException("expects an integer or boolean value"),
                };
                p.Set(iv);
                break;
            }

            case StorageType.String:
                p.Set(value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString());
                break;

            case StorageType.ElementId:
                if (value.ValueKind == JsonValueKind.Number)
                {
                    p.Set(new ElementId(value.GetInt64()));
                }
                else
                {
                    throw new InvalidOperationException("expects an element id (number)");
                }

                break;

            default:
                throw new InvalidOperationException($"unsupported storage type ({p.StorageType})");
        }
    }

    private static MoveElementsResult MoveElements(UIApplication app, MoveElementsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var ids = ResolveElementIds(doc, request.ElementIds);
        var translation = new XYZ(MmToFeet(request.DeltaX), MmToFeet(request.DeltaY), MmToFeet(request.DeltaZ));

        using var tx = new Transaction(doc, "Bridge move elements");
        tx.Start();
        ElementTransformUtils.MoveElements(doc, ids, translation);
        doc.Regenerate();
        tx.Commit();

        return new MoveElementsResult { MovedCount = ids.Count, ElementIds = ids.Select(ToInt).ToList() };
    }

    private static RotateElementsResult RotateElements(UIApplication app, RotateElementsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var ids = ResolveElementIds(doc, request.ElementIds);
        var origin = MmPoint(request.OriginX, request.OriginY, request.OriginZ);
        var axisVec = ResolveAxisVector(request.Axis);
        var axisLine = Line.CreateBound(origin, origin + axisVec);
        var angleRad = request.AngleDeg * Math.PI / 180.0;

        using var tx = new Transaction(doc, "Bridge rotate elements");
        tx.Start();
        ElementTransformUtils.RotateElements(doc, ids, axisLine, angleRad);
        doc.Regenerate();
        tx.Commit();

        return new RotateElementsResult { RotatedCount = ids.Count, ElementIds = ids.Select(ToInt).ToList() };
    }

    private static MirrorElementsResult MirrorElements(UIApplication app, MirrorElementsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var ids = ResolveElementIds(doc, request.ElementIds);
        var origin = MmPoint(request.PlaneOriginX, request.PlaneOriginY, request.PlaneOriginZ);
        var normal = ResolveAxisVector(request.PlaneNormal);
        var plane = Plane.CreateByNormalAndOrigin(normal, origin);

        using var tx = new Transaction(doc, "Bridge mirror elements");
        tx.Start();
        ElementTransformUtils.MirrorElements(doc, ids, plane, request.CreateCopy);
        doc.Regenerate();
        tx.Commit();

        return new MirrorElementsResult { MirroredCount = ids.Count, ElementIds = ids.Select(ToInt).ToList() };
    }

    private static CopyElementsToLevelResult CopyElementsToLevel(UIApplication app, CopyElementsToLevelRequest request)
    {
        var doc = RequireProjectDocument(app);
        var ids = ResolveElementIds(doc, request.ElementIds);
        if (doc.GetElement(new ElementId(request.TargetLevelId)) is not Level targetLevel)
        {
            throw new InvalidOperationException($"targetLevelId {request.TargetLevelId} does not resolve to a Level.");
        }

        // CopyElements applies one translation to the whole set, so we align to the target level
        // using the first element's level as the reference elevation.
        var refElevation = ResolveElementLevelElevation(doc, doc.GetElement(ids[0]));
        var translation = new XYZ(0, 0, targetLevel.Elevation - refElevation);

        using var tx = new Transaction(doc, "Bridge copy elements to level");
        tx.Start();
        var newIds = ElementTransformUtils.CopyElements(doc, ids, translation);
        doc.Regenerate();
        tx.Commit();

        return new CopyElementsToLevelResult
        {
            CopiedCount = newIds.Count,
            NewElementIds = newIds.Select(ToInt).ToList(),
        };
    }

    private static SetElementParametersResult SetElementParameters(UIApplication app, SetElementParametersRequest request)
    {
        var doc = RequireProjectDocument(app);
        var element = doc.GetElement(new ElementId(request.ElementId))
            ?? throw new InvalidOperationException($"Element {request.ElementId} was not found.");
        if (request.Parameters is null || request.Parameters.Count == 0)
        {
            throw new InvalidOperationException("parameters is required and must contain at least one key/value pair.");
        }

        var skipped = new List<string>();
        var setCount = 0;

        using var tx = new Transaction(doc, "Bridge set element parameters");
        tx.Start();
        foreach (var kvp in request.Parameters)
        {
            var name = kvp.Key?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var param = element.LookupParameter(name);
            if (param is null)
            {
                skipped.Add($"{name}: not found");
                continue;
            }

            if (param.IsReadOnly)
            {
                skipped.Add($"{name}: read-only");
                continue;
            }

            try
            {
                SetInstanceParameterValue(param, kvp.Value);
                setCount++;
            }
            catch (Exception ex)
            {
                skipped.Add($"{name}: {ex.GetBaseException().Message}");
            }
        }

        doc.Regenerate();
        tx.Commit();

        return new SetElementParametersResult { SetCount = setCount, Skipped = skipped };
    }

    private static DeleteElementsResult DeleteElements(UIApplication app, DeleteElementsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var ids = ResolveElementIds(doc, request.ElementIds);

        var deletedIds = new List<int>();
        var failed = new List<DeleteElementFailure>();

        using var tx = new Transaction(doc, "Bridge delete elements");
        tx.Start();
        foreach (var eid in ids)
        {
            var element = doc.GetElement(eid);
            if (element is null)
            {
                // Already gone — a prior element in this batch cascade-deleted it.
                continue;
            }

            // Views and sheets cannot always be removed via Document.Delete (the last
            // project view, system views, sheets with dependents, etc.); Revit throws
            // InvalidOperationException. Guard those per-element so one undeletable view
            // does not fail the whole batch — surface the reason so the caller knows
            // which ids require manual deletion.
            var isSheet = element.Category is not null
                && string.Equals(ToCategoryToken(element.Category), "OST_Sheets", StringComparison.Ordinal);
            var isView = element is View view && view.ViewType != ViewType.Undefined;

            if (isView || isSheet)
            {
                try
                {
                    var removed = doc.Delete(eid);
                    deletedIds.AddRange(removed.Select(ToInt));
                }
                catch (InvalidOperationException ex)
                {
                    failed.Add(new DeleteElementFailure
                    {
                        Id = ToInt(eid),
                        Reason = ex.Message,
                    });
                }
            }
            else
            {
                var removed = doc.Delete(eid);
                deletedIds.AddRange(removed.Select(ToInt));
            }
        }

        doc.Regenerate();
        tx.Commit();

        var distinctDeleted = deletedIds.Distinct().ToList();
        return new DeleteElementsResult
        {
            DeletedCount = distinctDeleted.Count,
            DeletedIds = distinctDeleted,
            Failed = failed,
        };
    }

    // ───────────────────────────────────────────────────────────────────────
    // Group 8 — Views, Sheets & Sheet Audit. Creation handlers wrap a Transaction;
    // read/audit handlers are read-only. All coordinates are millimetres.
    // ───────────────────────────────────────────────────────────────────────

    // Tolerant (case-insensitive) options for parsing the sheet-audit template JSON.
    private static readonly JsonSerializerOptions _templateJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static ViewFamilyType FirstViewFamilyType(Document doc, ViewFamily family)
    {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .FirstOrDefault(v => v.ViewFamily == family)
            ?? throw new InvalidOperationException(
                $"No {family} view family type is available in this project.");
    }

    // Sets a view/sheet name, tolerating Revit's uniqueness constraint by uniquifying on clash.
    private static void TrySetViewName(Element view, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var desired = name.Trim();
        try
        {
            view.Name = desired;
            return;
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException)
        {
            // Name already in use — fall back to a uniquified variant rather than failing creation.
        }

        for (var i = 2; i < 1000; i++)
        {
            try
            {
                view.Name = $"{desired} ({i})";
                return;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                // keep trying
            }
        }
    }

    private static FamilyInstance? GetSheetTitleBlock(Document doc, ViewSheet sheet)
    {
        return new FilteredElementCollector(doc, sheet.Id)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .FirstOrDefault();
    }

    private static BoundingBox2DData? ViewCropBox2D(View? view)
    {
        if (view is null || !view.CropBoxActive)
        {
            return null;
        }

        var cb = view.CropBox;
        if (cb is null)
        {
            return null;
        }

        return new BoundingBox2DData
        {
            MinX = FeetToMm(cb.Min.X),
            MinY = FeetToMm(cb.Min.Y),
            MaxX = FeetToMm(cb.Max.X),
            MaxY = FeetToMm(cb.Max.Y),
        };
    }

    private static double GetTextNoteFontSizeFeet(TextNote tn)
    {
        try
        {
            var type = tn.Document.GetElement(tn.GetTypeId()) as TextNoteType;
            return type?.get_Parameter(BuiltInParameter.TEXT_SIZE)?.AsDouble() ?? 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    private static bool IsDimensionLabeled(Dimension dim)
    {
        try
        {
            if (dim.NumberOfSegments == 0)
            {
                return !string.IsNullOrEmpty(dim.ValueOverride);
            }

            foreach (DimensionSegment seg in dim.Segments)
            {
                if (!string.IsNullOrEmpty(seg.ValueOverride))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeTypeName(string s) =>
        new string((s ?? string.Empty).Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();

    private static CreatePlanViewResult CreatePlanViewInternal(
        UIApplication app,
        CreatePlanViewRequest request,
        ViewFamily family)
    {
        var doc = RequireProjectDocument(app);
        if (doc.GetElement(new ElementId(request.LevelId)) is not Level level)
        {
            throw new InvalidOperationException($"levelId {request.LevelId} does not resolve to a Level.");
        }

        var vft = FirstViewFamilyType(doc, family);

        using var tx = new Transaction(doc, "Bridge create plan view");
        tx.Start();
        var view = ViewPlan.Create(doc, vft.Id, level.Id);
        TrySetViewName(view, request.Name);
        doc.Regenerate();
        tx.Commit();

        return new CreatePlanViewResult { Id = ToInt(view.Id), Name = view.Name, LevelName = level.Name };
    }

    private static CreatePlanViewResult CreateFloorPlanView(UIApplication app, CreatePlanViewRequest request) =>
        CreatePlanViewInternal(app, request, ViewFamily.FloorPlan);

    private static CreatePlanViewResult CreateReflectedCeilingPlan(UIApplication app, CreatePlanViewRequest request) =>
        CreatePlanViewInternal(app, request, ViewFamily.CeilingPlan);

    private static CreateViewResult CreateSectionView(UIApplication app, CreateSectionViewRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (request.BoundingBoxMin is not { Length: 3 } || request.BoundingBoxMax is not { Length: 3 })
        {
            throw new InvalidOperationException("boundingBoxMin and boundingBoxMax must each be [x, y, z] in mm.");
        }

        var min = MmPoint(request.BoundingBoxMin[0], request.BoundingBoxMin[1], request.BoundingBoxMin[2]);
        var max = MmPoint(request.BoundingBoxMax[0], request.BoundingBoxMax[1], request.BoundingBoxMax[2]);
        var center = (min + max) * 0.5;

        // Section looks horizontally along +Y: view right = world X, view up = world Z.
        var minExtent = MmToFeet(100);
        var width = Math.Max(Math.Abs(max.X - min.X), minExtent);
        var height = Math.Max(Math.Abs(max.Z - min.Z), minExtent);
        var depth = Math.Max(Math.Abs(max.Y - min.Y), minExtent);

        var t = Transform.Identity;
        t.Origin = center;
        t.BasisX = XYZ.BasisX;
        t.BasisY = XYZ.BasisZ;
        t.BasisZ = XYZ.BasisX.CrossProduct(XYZ.BasisZ); // (0,-1,0)

        var box = new BoundingBoxXYZ
        {
            Transform = t,
            Min = new XYZ(-width / 2, -height / 2, -depth / 2),
            Max = new XYZ(width / 2, height / 2, depth / 2),
        };

        var vft = FirstViewFamilyType(doc, ViewFamily.Section);

        using var tx = new Transaction(doc, "Bridge create section view");
        tx.Start();
        var view = ViewSection.CreateSection(doc, vft.Id, box);
        TrySetViewName(view, request.Name);
        doc.Regenerate();
        tx.Commit();

        return new CreateViewResult { Id = ToInt(view.Id), Name = view.Name };
    }

    private static CreateViewResult Create3DView(UIApplication app, Create3DViewRequest request)
    {
        var doc = RequireProjectDocument(app);
        var vft = FirstViewFamilyType(doc, ViewFamily.ThreeDimensional);

        using var tx = new Transaction(doc, "Bridge create 3D view");
        tx.Start();
        var view = request.IsOrthographic
            ? View3D.CreateIsometric(doc, vft.Id)
            : View3D.CreatePerspective(doc, vft.Id);
        TrySetViewName(view, request.Name);
        doc.Regenerate();
        tx.Commit();

        return new CreateViewResult { Id = ToInt(view.Id), Name = view.Name };
    }

    private static CreateSheetResult CreateSheet(UIApplication app, CreateSheetRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (string.IsNullOrWhiteSpace(request.SheetNumber))
        {
            throw new InvalidOperationException("sheetNumber is required.");
        }

        var titleBlockId = ElementId.InvalidElementId;
        if (request.TitleBlockTypeId is int tb && tb > 0)
        {
            titleBlockId = new ElementId(tb);
            if (doc.GetElement(titleBlockId) is not FamilySymbol)
            {
                throw new InvalidOperationException($"titleBlockTypeId {tb} does not resolve to a title block type.");
            }
        }

        using var tx = new Transaction(doc, "Bridge create sheet");
        tx.Start();
        var sheet = ViewSheet.Create(doc, titleBlockId);
        try
        {
            sheet.SheetNumber = request.SheetNumber.Trim();
        }
        catch (Exception ex)
        {
            tx.RollBack();
            throw new InvalidOperationException(
                $"Could not set sheet number '{request.SheetNumber}': {ex.GetBaseException().Message} (it may already be in use).");
        }

        if (!string.IsNullOrWhiteSpace(request.SheetName))
        {
            sheet.Name = request.SheetName.Trim();
        }

        doc.Regenerate();
        var titleBlockName = GetSheetTitleBlock(doc, sheet)?.Name ?? string.Empty;
        tx.Commit();

        return new CreateSheetResult
        {
            Id = ToInt(sheet.Id),
            SheetNumber = sheet.SheetNumber,
            SheetName = sheet.Name,
            TitleBlockName = titleBlockName,
        };
    }

    private static PlaceViewOnSheetResult PlaceViewOnSheet(UIApplication app, PlaceViewOnSheetRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (doc.GetElement(new ElementId(request.SheetId)) is not ViewSheet sheet)
        {
            throw new InvalidOperationException($"sheetId {request.SheetId} does not resolve to a sheet.");
        }

        if (doc.GetElement(new ElementId(request.ViewId)) is not View view)
        {
            throw new InvalidOperationException($"viewId {request.ViewId} does not resolve to a view.");
        }

        if (!Viewport.CanAddViewToSheet(doc, sheet.Id, view.Id))
        {
            throw new InvalidOperationException(
                $"View {request.ViewId} cannot be placed on sheet {request.SheetId} "
                + "(it may already be placed on another sheet, or is not a placeable view type).");
        }

        var point = new XYZ(MmToFeet(request.LocationX), MmToFeet(request.LocationY), 0);

        using var tx = new Transaction(doc, "Bridge place view on sheet");
        tx.Start();
        var viewport = Viewport.Create(doc, sheet.Id, view.Id, point);
        doc.Regenerate();
        tx.Commit();

        return new PlaceViewOnSheetResult
        {
            ViewportId = ToInt(viewport.Id),
            SheetNumber = sheet.SheetNumber,
            ViewName = view.Name,
        };
    }

    private static SetViewCropRegionResult SetViewCropRegion(UIApplication app, SetViewCropRegionRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (doc.GetElement(new ElementId(request.ViewId)) is not View view)
        {
            throw new InvalidOperationException($"viewId {request.ViewId} does not resolve to a view.");
        }

        using var tx = new Transaction(doc, "Bridge set view crop region");
        tx.Start();
        var crop = view.CropBox;
        crop.Min = new XYZ(MmToFeet(request.MinX), MmToFeet(request.MinY), crop.Min.Z);
        crop.Max = new XYZ(MmToFeet(request.MaxX), MmToFeet(request.MaxY), crop.Max.Z);
        view.CropBox = crop;
        view.CropBoxActive = true;
        view.CropBoxVisible = true;
        doc.Regenerate();
        tx.Commit();

        return new SetViewCropRegionResult
        {
            ViewId = ToInt(view.Id),
            CropBoxMm = new BoundingBox2DData
            {
                MinX = request.MinX,
                MinY = request.MinY,
                MaxX = request.MaxX,
                MaxY = request.MaxY,
            },
        };
    }

    private static GetSheetsResult GetSheets(UIApplication app)
    {
        var doc = RequireProjectDocument(app);
        var sheets = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            .Cast<ViewSheet>()
            .Where(s => !s.IsTemplate)
            .OrderBy(s => s.SheetNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var records = sheets.Select(s => new SheetRecord
        {
            Id = ToInt(s.Id),
            SheetNumber = s.SheetNumber,
            SheetName = s.Name,
            TitleBlock = GetSheetTitleBlock(doc, s)?.Name ?? string.Empty,
            ViewportIds = s.GetAllViewports().Select(ToInt).ToList(),
        }).ToList();

        return new GetSheetsResult { Sheets = records, TotalCount = records.Count };
    }

    private static GetSheetContentsResult GetSheetContents(UIApplication app, GetSheetContentsRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (doc.GetElement(new ElementId(request.SheetId)) is not ViewSheet sheet)
        {
            throw new InvalidOperationException($"sheetId {request.SheetId} does not resolve to a sheet.");
        }

        return CollectSheetContents(doc, sheet);
    }

    // Shared by get_sheet_contents and compare_sheet_to_template.
    private static GetSheetContentsResult CollectSheetContents(Document doc, ViewSheet sheet)
    {
        var titleBlockParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var titleBlock = GetSheetTitleBlock(doc, sheet);
        if (titleBlock is not null)
        {
            foreach (Parameter p in titleBlock.Parameters)
            {
                if (p?.Definition is null || !p.HasValue)
                {
                    continue;
                }

                var key = p.Definition.Name;
                if (string.IsNullOrWhiteSpace(key) || titleBlockParams.ContainsKey(key))
                {
                    continue;
                }

                var value = ParameterValueToString(doc, p);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    titleBlockParams[key] = value;
                }
            }
        }

        var viewports = new List<SheetViewportRecord>();
        foreach (var vpId in sheet.GetAllViewports())
        {
            if (doc.GetElement(vpId) is not Viewport vp)
            {
                continue;
            }

            var view = doc.GetElement(vp.ViewId) as View;
            viewports.Add(new SheetViewportRecord
            {
                ViewId = ToInt(vp.ViewId),
                ViewName = view?.Name ?? string.Empty,
                ViewType = view?.ViewType.ToString() ?? string.Empty,
                CenterMm = ToGeomPoint(vp.GetBoxCenter()),
                CropBoxMm = ViewCropBox2D(view),
            });
        }

        var textNotes = new FilteredElementCollector(doc, sheet.Id)
            .OfClass(typeof(TextNote))
            .Cast<TextNote>()
            .Select(tn => new SheetTextNoteRecord
            {
                Text = tn.Text ?? string.Empty,
                PositionMm = ToGeomPoint(tn.Coord),
                FontSize = FeetToMm(GetTextNoteFontSizeFeet(tn)),
            })
            .ToList();

        var tags = new List<SheetTagRecord>();
        foreach (var tag in new FilteredElementCollector(doc, sheet.Id)
                     .OfClass(typeof(IndependentTag))
                     .Cast<IndependentTag>())
        {
            var taggedId = tag.GetTaggedLocalElementIds().FirstOrDefault();
            tags.Add(new SheetTagRecord
            {
                TaggedElementId = taggedId is null || taggedId == ElementId.InvalidElementId ? 0 : ToInt(taggedId),
                PositionMm = ToGeomPoint(tag.TagHeadPosition),
            });
        }

        var filledRegions = new FilteredElementCollector(doc, sheet.Id)
            .OfClass(typeof(FilledRegion))
            .Cast<FilledRegion>()
            .Select(fr => new SheetFilledRegionRecord
            {
                Id = ToInt(fr.Id),
                PositionMm = ElementCenter(fr),
            })
            .ToList();

        return new GetSheetContentsResult
        {
            SheetId = ToInt(sheet.Id),
            SheetNumber = sheet.SheetNumber,
            SheetName = sheet.Name,
            TitleBlockParameters = titleBlockParams,
            Viewports = viewports,
            TextNotes = textNotes,
            Tags = tags,
            FilledRegions = filledRegions,
            TotalAnnotationCount = textNotes.Count + tags.Count + filledRegions.Count,
        };
    }

    private static OpenSheetResult OpenSheet(UIApplication app, OpenSheetRequest request)
    {
        var uidoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uidoc.Document;
        if (doc.IsFamilyDocument)
        {
            throw new InvalidOperationException("Active document is a family document. Open a project document first.");
        }

        ViewSheet? sheet = null;
        if (request.SheetId is int sid && sid > 0)
        {
            sheet = doc.GetElement(new ElementId(sid)) as ViewSheet;
        }
        else if (!string.IsNullOrWhiteSpace(request.SheetNumber))
        {
            var number = request.SheetNumber.Trim();
            sheet = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .FirstOrDefault(s => string.Equals(s.SheetNumber, number, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            throw new InvalidOperationException("Provide sheetId or sheetNumber.");
        }

        if (sheet is null)
        {
            throw new InvalidOperationException("Sheet not found for the given sheetId/sheetNumber.");
        }

        uidoc.ActiveView = sheet;
        return new OpenSheetResult { Id = ToInt(sheet.Id), SheetNumber = sheet.SheetNumber, SheetName = sheet.Name };
    }

    private static GetViewContentsResult GetViewContents(UIApplication app, GetViewContentsRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (doc.GetElement(new ElementId(request.ViewId)) is not View view)
        {
            throw new InvalidOperationException($"viewId {request.ViewId} does not resolve to a view.");
        }

        return CollectViewContents(doc, view, request.IncludeElements, request.IncludeAnnotations);
    }

    // Shared by get_view_contents and compare_sheet_to_template.
    private static GetViewContentsResult CollectViewContents(
        Document doc,
        View view,
        bool includeElements,
        bool includeAnnotations)
    {
        var elementsByCategory = new Dictionary<string, CategoryBucket>(StringComparer.OrdinalIgnoreCase);
        var modelElementIds = new List<ElementId>();
        var untagged = new List<UntaggedElementRecord>();
        var dimensions = new DimensionSummary();
        var tagSummary = new TagSummary();
        var textNotes = new List<ViewTextNoteRecord>();
        var totalElementCount = 0;
        var taggedIds = new HashSet<long>();

        if (includeAnnotations)
        {
            var tagElementCount = 0;
            var taggedElementIds = new List<int>();
            foreach (var tag in new FilteredElementCollector(doc, view.Id)
                         .OfClass(typeof(IndependentTag))
                         .Cast<IndependentTag>())
            {
                tagElementCount++;
                foreach (var tid in tag.GetTaggedLocalElementIds())
                {
                    if (tid is not null && tid != ElementId.InvalidElementId)
                    {
                        taggedIds.Add(tid.Value);
                        taggedElementIds.Add(ToInt(tid));
                    }
                }
            }

            tagSummary = new TagSummary
            {
                Total = tagElementCount,
                TaggedElementIds = taggedElementIds.Distinct().ToList(),
            };

            var dims = new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(Dimension))
                .Cast<Dimension>()
                .ToList();
            var labeled = dims.Count(IsDimensionLabeled);
            dimensions = new DimensionSummary { Total = dims.Count, Labeled = labeled, Unlabeled = dims.Count - labeled };

            textNotes = new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>()
                .Select(tn => new ViewTextNoteRecord { Text = tn.Text ?? string.Empty, PositionMm = ToGeomPoint(tn.Coord) })
                .ToList();
        }

        if (includeElements)
        {
            var byCat = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType())
            {
                if (e is IndependentTag or Dimension or TextNote or Viewport)
                {
                    continue;
                }

                if (e.Category is null || e.Category.CategoryType != CategoryType.Model)
                {
                    continue;
                }

                var token = ToCategoryToken(e.Category);
                if (!byCat.TryGetValue(token, out var list))
                {
                    list = new List<int>();
                    byCat[token] = list;
                }

                list.Add(ToInt(e.Id));
                modelElementIds.Add(e.Id);
            }

            foreach (var kvp in byCat)
            {
                elementsByCategory[kvp.Key] = new CategoryBucket { Count = kvp.Value.Count, Ids = kvp.Value };
            }

            totalElementCount = modelElementIds.Count;

            if (includeAnnotations)
            {
                foreach (var id in modelElementIds)
                {
                    if (taggedIds.Contains(id.Value))
                    {
                        continue;
                    }

                    var e = doc.GetElement(id);
                    untagged.Add(new UntaggedElementRecord
                    {
                        Id = ToInt(id),
                        Category = e?.Category is not null ? ToCategoryToken(e.Category) : string.Empty,
                        FamilyName = e is not null ? ResolveElementFamilyName(doc, e) : string.Empty,
                    });
                }
            }
        }

        return new GetViewContentsResult
        {
            ViewId = ToInt(view.Id),
            ViewName = view.Name,
            ViewType = view.ViewType.ToString(),
            ElementsByCategory = elementsByCategory,
            Dimensions = dimensions,
            Tags = tagSummary,
            UntaggedElements = untagged,
            TextNotes = textNotes,
            TotalElementCount = totalElementCount,
        };
    }

    private static CompareSheetToTemplateResult CompareSheetToTemplate(
        UIApplication app,
        CompareSheetToTemplateRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (doc.GetElement(new ElementId(request.SheetId)) is not ViewSheet sheet)
        {
            throw new InvalidOperationException($"sheetId {request.SheetId} does not resolve to a sheet.");
        }

        if (string.IsNullOrWhiteSpace(request.TemplateJson))
        {
            throw new InvalidOperationException("templateJson is required.");
        }

        SheetTemplate template;
        try
        {
            template = JsonSerializer.Deserialize<SheetTemplate>(request.TemplateJson, _templateJsonOptions)
                ?? new SheetTemplate();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"templateJson could not be parsed: {ex.GetBaseException().Message}");
        }

        var contents = CollectSheetContents(doc, sheet);
        var issues = new List<TemplateIssue>();

        foreach (var field in template.RequiredTitleBlockFields ?? [])
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                continue;
            }

            var present = contents.TitleBlockParameters.TryGetValue(field, out var val) && !string.IsNullOrWhiteSpace(val);
            if (!present)
            {
                issues.Add(new TemplateIssue
                {
                    Severity = "error",
                    Category = "titleBlock",
                    Description = $"Title block field '{field}' is empty or missing.",
                });
            }
        }

        var placedTypes = contents.Viewports.Select(v => NormalizeTypeName(v.ViewType)).ToHashSet();
        foreach (var rvt in template.RequiredViewTypes ?? [])
        {
            if (string.IsNullOrWhiteSpace(rvt))
            {
                continue;
            }

            if (!placedTypes.Contains(NormalizeTypeName(rvt)))
            {
                issues.Add(new TemplateIssue
                {
                    Severity = "error",
                    Category = "viewports",
                    Description = $"No viewport of required type '{rvt}' is placed on the sheet.",
                });
            }
        }

        if (template.RequiredViewCount > 0 && contents.Viewports.Count < template.RequiredViewCount)
        {
            issues.Add(new TemplateIssue
            {
                Severity = "error",
                Category = "viewports",
                Description = $"Sheet has {contents.Viewports.Count} viewport(s); template requires at least {template.RequiredViewCount}.",
            });
        }

        var ann = template.RequiredAnnotations;
        if (ann is not null)
        {
            var totalDims = 0;
            var untaggedSamples = new List<UntaggedElementRecord>();
            foreach (var vp in contents.Viewports)
            {
                if (doc.GetElement(new ElementId(vp.ViewId)) is not View v)
                {
                    continue;
                }

                var vc = CollectViewContents(doc, v, includeElements: ann.RequireAllElementsTagged, includeAnnotations: true);
                totalDims += vc.Dimensions.Total;
                if (ann.RequireAllElementsTagged)
                {
                    untaggedSamples.AddRange(vc.UntaggedElements);
                }
            }

            if (ann.MinimumDimensions > 0 && totalDims < ann.MinimumDimensions)
            {
                issues.Add(new TemplateIssue
                {
                    Severity = "warning",
                    Category = "annotations",
                    Description = $"Placed views contain {totalDims} dimension(s); template requires at least {ann.MinimumDimensions}.",
                });
            }

            if (ann.RequireAllElementsTagged)
            {
                foreach (var u in untaggedSamples.Take(50))
                {
                    issues.Add(new TemplateIssue
                    {
                        Severity = "warning",
                        Category = "tags",
                        Description = $"Untagged {u.Category} element ({u.FamilyName}).",
                        ElementId = u.Id,
                    });
                }
            }

            foreach (var required in ann.RequiredTextNotes ?? [])
            {
                if (string.IsNullOrWhiteSpace(required))
                {
                    continue;
                }

                var found = contents.TextNotes.Any(t => t.Text.Contains(required, StringComparison.OrdinalIgnoreCase));
                if (!found)
                {
                    issues.Add(new TemplateIssue
                    {
                        Severity = "warning",
                        Category = "text",
                        Description = $"Required text note '{required}' was not found on the sheet.",
                    });
                }
            }
        }

        return new CompareSheetToTemplateResult
        {
            SheetId = ToInt(sheet.Id),
            SheetNumber = sheet.SheetNumber,
            Passed = !issues.Any(i => i.Severity == "error"),
            Issues = issues,
            IssueCount = issues.Count,
        };
    }

    // ───────────────────────────────────────────────────────────────────────
    // Group 5 — Geometry Read & In-Place Extraction.
    // All three resolve their document from app.ActiveUIDocument.Document so they follow
    // Revit into an Edit-In-Place session instead of pinning to the project root.
    // ───────────────────────────────────────────────────────────────────────

    private static GetActiveContextResult GetActiveContext(UIApplication app)
    {
        var uidoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uidoc.Document;

        if (doc.IsFamilyDocument)
        {
            // A loadable family opened in the Family Editor is its own document.
            return new GetActiveContextResult
            {
                Context = "family_editor",
                DocumentTitle = doc.Title,
                Category = doc.OwnerFamily?.FamilyCategory?.Name,
            };
        }

        // NOTE: Revit 2025's public API exposes no reliable flag for "an Edit-In-Place session
        // is active." Unlike loadable families, in-place editing happens *inside* the project
        // document (IsFamilyDocument stays false), so we cannot positively distinguish it here.
        // We therefore report "project" and call out the limitation in Note. Geometry extraction
        // during an in-place edit still works: the in-place element lives in this same document,
        // so get_element_geometry (useSelection or elementId) reads it correctly.
        return new GetActiveContextResult
        {
            Context = "project",
            DocumentTitle = doc.Title,
            Note = "Revit's public API exposes no reliable Edit-In-Place session flag; an active "
                + "in-place edit is reported as 'project'. The in-place element is still in this "
                + "document and can be read via get_element_geometry.",
        };
    }

    private static GetInplaceElementsResult GetInplaceElements(UIApplication app, GetInplaceElementsRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        var limit = request.Limit <= 0 ? 100 : request.Limit;

        long? categoryFilterId = null;
        var categoryToken = request.Category?.Trim();
        if (!string.IsNullOrWhiteSpace(categoryToken)
            && Enum.TryParse<BuiltInCategory>(categoryToken, ignoreCase: true, out var bic))
        {
            categoryFilterId = (long)bic;
        }

        var inPlace = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .Where(fi => fi.Symbol?.Family?.IsInPlace == true)
            .Where(fi => categoryFilterId is null || fi.Category?.Id.Value == categoryFilterId.Value)
            .ToList();

        var records = new List<InplaceElementRecord>();
        foreach (var fi in inPlace)
        {
            if (records.Count >= limit)
            {
                break;
            }

            var box = fi.get_BoundingBox(null);
            var center = box is not null
                ? (box.Min + box.Max) * 0.5
                : (fi.Location as LocationPoint)?.Point ?? XYZ.Zero;

            records.Add(new InplaceElementRecord
            {
                Id = ToInt(fi.Id),
                Name = fi.Name,
                Category = fi.Category is not null ? ToCategoryToken(fi.Category) : string.Empty,
                FamilyName = fi.Symbol?.Family?.Name ?? string.Empty,
                Location = ToGeomPoint(center),
            });
        }

        return new GetInplaceElementsResult { Elements = records, TotalCount = inPlace.Count };
    }

    private static GetElementGeometryResult GetElementGeometry(UIApplication app, GetElementGeometryRequest request)
    {
        var uidoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uidoc.Document;

        Element element;
        if (request.UseSelection)
        {
            var selected = uidoc.Selection.GetElementIds();
            if (selected.Count == 0)
            {
                throw new InvalidOperationException(
                    "useSelection=true but nothing is selected. Select an element or pass elementId.");
            }

            element = doc.GetElement(selected.First())
                ?? throw new InvalidOperationException("The selected element could not be resolved.");
        }
        else
        {
            if (request.ElementId is null)
            {
                throw new InvalidOperationException("Provide elementId, or set useSelection=true.");
            }

            element = doc.GetElement(new ElementId(request.ElementId.Value))
                ?? throw new InvalidOperationException($"Element {request.ElementId.Value} was not found.");
        }

        var sketches = new List<SketchProfileData>();
        var notes = new List<string>();

        if (request.IncludeSketches)
        {
            try
            {
                sketches = ExtractSketches(doc, element);
            }
            catch (Exception ex)
            {
                notes.Add($"Sketch extraction failed: {ex.GetBaseException().Message}");
            }
        }

        var solids = new List<SolidData>();
        if (request.IncludeSolids || sketches.Count == 0)
        {
            try
            {
                solids = ExtractSolids(element);
            }
            catch (Exception ex)
            {
                notes.Add($"Solid extraction failed: {ex.GetBaseException().Message}");
            }
        }

        BoundingBoxData? boundingBox = null;
        var bb = element.get_BoundingBox(null);
        if (bb is not null)
        {
            boundingBox = new BoundingBoxData
            {
                MinX = FeetToMm(bb.Min.X),
                MinY = FeetToMm(bb.Min.Y),
                MinZ = FeetToMm(bb.Min.Z),
                MaxX = FeetToMm(bb.Max.X),
                MaxY = FeetToMm(bb.Max.Y),
                MaxZ = FeetToMm(bb.Max.Z),
            };
        }

        return new GetElementGeometryResult
        {
            ElementId = ToInt(element.Id),
            Context = doc.IsFamilyDocument ? "family_editor" : "project",
            BoundingBox = boundingBox,
            Sketches = sketches,
            Solids = solids,
            Note = notes.Count > 0 ? string.Join(" | ", notes) : null,
        };
    }

    // Sketch-first extraction: find Sketch elements this element generates (extrusions, sweeps,
    // walls, floors, …) and read their analytic profile loops. Preserves line/arc data exactly.
    private static List<SketchProfileData> ExtractSketches(Document doc, Element element)
    {
        var profiles = new List<SketchProfileData>();
        var sketchIds = element.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
        var profileIndex = 0;
        foreach (var sketchId in sketchIds)
        {
            if (doc.GetElement(sketchId) is not Sketch sketch)
            {
                continue;
            }

            var loops = new List<CurveLoopData>();
            foreach (CurveArray loop in sketch.Profile)
            {
                var curves = new List<CurveData>();
                foreach (Curve curve in loop)
                {
                    curves.Add(ToCurveData(curve));
                }

                if (curves.Count > 0)
                {
                    loops.Add(new CurveLoopData { Curves = curves });
                }
            }

            if (loops.Count > 0)
            {
                profiles.Add(new SketchProfileData { ProfileIndex = profileIndex++, Loops = loops });
            }
        }

        return profiles;
    }

    // Solid fallback: walk the element's geometry (descending into instances), and for each
    // real solid emit its faces with typed edge-loop curves and the face normal.
    private static List<SolidData> ExtractSolids(Element element)
    {
        var options = new Options
        {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine,
        };

        var geometry = element.get_Geometry(options);
        if (geometry is null)
        {
            return [];
        }

        var rawSolids = new List<Solid>();
        CollectSolids(geometry, rawSolids);

        var solids = new List<SolidData>();
        var solidIndex = 0;
        foreach (var solid in rawSolids)
        {
            var faces = new List<FaceData>();
            var faceIndex = 0;
            foreach (Face face in solid.Faces)
            {
                var edgeLoops = new List<CurveLoopData>();
                foreach (EdgeArray edgeArray in face.EdgeLoops)
                {
                    var curves = new List<CurveData>();
                    foreach (Edge edge in edgeArray)
                    {
                        curves.Add(ToCurveData(edge.AsCurve()));
                    }

                    if (curves.Count > 0)
                    {
                        edgeLoops.Add(new CurveLoopData { Curves = curves });
                    }
                }

                faces.Add(new FaceData
                {
                    FaceIndex = faceIndex++,
                    Normal = ToFaceNormal(face),
                    EdgeLoops = edgeLoops,
                });
            }

            solids.Add(new SolidData
            {
                SolidIndex = solidIndex++,
                Volume = FeetToMm(FeetToMm(FeetToMm(solid.Volume))), // ft³ -> mm³
                Faces = faces,
            });
        }

        return solids;
    }

    private static void CollectSolids(GeometryElement geometry, List<Solid> accumulator)
    {
        foreach (var obj in geometry)
        {
            switch (obj)
            {
                case Solid solid when solid.Volume > 1e-9 && solid.Faces.Size > 0:
                    accumulator.Add(solid);
                    break;
                case GeometryInstance instance:
                    CollectSolids(instance.GetInstanceGeometry(), accumulator);
                    break;
            }
        }
    }

    private static GeomPoint ToFaceNormal(Face face)
    {
        try
        {
            if (face is PlanarFace planar)
            {
                return ToGeomPoint(planar.FaceNormal);
            }

            var domain = face.GetBoundingBox();
            var mid = new UV(
                (domain.Min.U + domain.Max.U) * 0.5,
                (domain.Min.V + domain.Max.V) * 0.5);
            return ToGeomPoint(face.ComputeNormal(mid));
        }
        catch
        {
            return new GeomPoint();
        }
    }

    // Typed curve conversion. Lines and arcs carry clean analytic data; everything else
    // (splines, ellipses) is sampled into ordered points so the caller still gets usable geometry.
    private static CurveData ToCurveData(Curve curve)
    {
        var start = curve.GetEndPoint(0);
        var end = curve.GetEndPoint(1);

        switch (curve)
        {
            case Line:
                return new CurveData
                {
                    Type = "line",
                    Start = ToGeomPoint(start),
                    End = ToGeomPoint(end),
                };

            case Arc arc:
            {
                double AngleDeg(XYZ p)
                {
                    var v = p - arc.Center;
                    return Math.Atan2(v.DotProduct(arc.YDirection), v.DotProduct(arc.XDirection))
                        * 180.0 / Math.PI;
                }

                return new CurveData
                {
                    Type = "arc",
                    Start = ToGeomPoint(start),
                    End = ToGeomPoint(end),
                    Center = ToGeomPoint(arc.Center),
                    Radius = FeetToMm(arc.Radius),
                    StartAngleDeg = AngleDeg(start),
                    EndAngleDeg = AngleDeg(end),
                };
            }

            default:
            {
                var sampled = curve.Tessellate()
                    .Select(ToGeomPoint)
                    .ToList();
                return new CurveData
                {
                    Type = "spline",
                    Start = ToGeomPoint(start),
                    End = ToGeomPoint(end),
                    ControlPoints = sampled,
                };
            }
        }
    }

    // Turns the raw, unordered cap-face edge loop of an extruded element into an ordered,
    // closed, origin-normalised 2D profile that drops straight into create_extrusion. This
    // exists because get_element_geometry returns face edges as correct-but-unordered segment
    // pairs, and Revit's NewExtrusion requires a single chained, non-self-intersecting loop.
    private static ReconstructProfileResult ReconstructProfile(UIApplication app, ReconstructProfileRequest request)
    {
        var uidoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uidoc.Document;

        Element element;
        if (request.UseSelection)
        {
            var selected = uidoc.Selection.GetElementIds();
            if (selected.Count == 0)
            {
                throw new InvalidOperationException(
                    "useSelection=true but nothing is selected. Select an element or pass elementId.");
            }

            element = doc.GetElement(selected.First())
                ?? throw new InvalidOperationException("The selected element could not be resolved.");
        }
        else
        {
            if (request.ElementId <= 0)
            {
                throw new InvalidOperationException("Provide a positive elementId, or set useSelection=true.");
            }

            element = doc.GetElement(new ElementId(request.ElementId))
                ?? throw new InvalidOperationException($"Element {request.ElementId} was not found.");
        }

        var epsilonMm = request.EpsilonMm <= 0 ? 0.1 : request.EpsilonMm;
        var epsilonFeet = MmToFeet(epsilonMm);

        var options = new Options
        {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine,
        };
        var geometry = element.get_Geometry(options)
            ?? throw new InvalidOperationException($"Element {ToInt(element.Id)} has no geometry.");
        var solids = new List<Solid>();
        CollectSolids(geometry, solids);
        var solid = solids.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Element {ToInt(element.Id)} has no solid geometry to reconstruct.");

        // Cap face = the planar face whose normal is most aligned to a primary axis (X/Y/Z);
        // among the (typically two) cap faces, the larger-area one carries the profile.
        static double AxisAlignment(XYZ v) => Math.Max(Math.Abs(v.X), Math.Max(Math.Abs(v.Y), Math.Abs(v.Z)));
        var planarFaces = solid.Faces
            .Cast<Face>()
            .OfType<PlanarFace>()
            .Select(f => new { Face = f, Normal = f.FaceNormal.Normalize(), Area = f.Area })
            .ToList();
        if (planarFaces.Count == 0)
        {
            throw new InvalidOperationException(
                "Could not identify cap face (no planar faces on the solid).");
        }

        var maxAlign = planarFaces.Max(x => AxisAlignment(x.Normal));
        if (maxAlign < 0.9)
        {
            throw new InvalidOperationException("Could not identify cap face (all faces oblique).");
        }

        var cap = planarFaces
            .Where(x => AxisAlignment(x.Normal) >= maxAlign - 1e-3)
            .OrderByDescending(x => x.Area)
            .First();

        // Extrusion axis = the primary axis the cap normal points along.
        var nrm = cap.Normal;
        var ax = Math.Abs(nrm.X);
        var ay = Math.Abs(nrm.Y);
        var az = Math.Abs(nrm.Z);
        var axis = az >= ax && az >= ay ? "Z" : ay >= ax ? "Y" : "X";

        if (cap.Face.EdgeLoops.Size == 0)
        {
            throw new InvalidOperationException("Cap face has no edge loops.");
        }

        // Chain one edge loop into an ordered, deduped XYZ vertex ring. Each edge becomes one
        // (line) or many (arc/spline) segments; segments are matched head-to-tail by endpoint
        // (flipping when the far end matches) until the loop closes on its start point.
        (List<XYZ> Pts, bool Closed) ChainLoop(EdgeArray loop)
        {
            var segs = new List<(XYZ A, XYZ B)>();
            foreach (Edge edge in loop)
            {
                var curve = edge.AsCurve();
                if (curve is Line)
                {
                    segs.Add((curve.GetEndPoint(0), curve.GetEndPoint(1)));
                }
                else
                {
                    const int samples = 8; // arcs/splines: tessellate to >= 8 points
                    var prev = curve.Evaluate(0.0, true);
                    for (var i = 1; i <= samples; i++)
                    {
                        var pt = curve.Evaluate((double)i / samples, true);
                        segs.Add((prev, pt));
                        prev = pt;
                    }
                }
            }

            if (segs.Count == 0)
            {
                return (new List<XYZ>(), false);
            }

            var used = new bool[segs.Count];
            var ordered = new List<XYZ> { segs[0].A, segs[0].B };
            used[0] = true;
            var startPt = segs[0].A;
            var current = segs[0].B;
            var loopClosed = current.DistanceTo(startPt) <= epsilonFeet;

            while (!loopClosed)
            {
                var foundIdx = -1;
                var flip = false;
                for (var j = 0; j < segs.Count; j++)
                {
                    if (used[j])
                    {
                        continue;
                    }

                    if (segs[j].A.DistanceTo(current) <= epsilonFeet)
                    {
                        foundIdx = j;
                        flip = false;
                        break;
                    }

                    if (segs[j].B.DistanceTo(current) <= epsilonFeet)
                    {
                        foundIdx = j;
                        flip = true;
                        break;
                    }
                }

                if (foundIdx < 0)
                {
                    break; // gap — cannot continue
                }

                used[foundIdx] = true;
                current = flip ? segs[foundIdx].A : segs[foundIdx].B;
                ordered.Add(current);
                if (current.DistanceTo(startPt) <= epsilonFeet)
                {
                    loopClosed = true;
                }
            }

            // Drop consecutive near-duplicates (and the trailing closing point).
            var dedup = new List<XYZ>();
            foreach (var p in ordered)
            {
                if (dedup.Count == 0 || dedup[^1].DistanceTo(p) > epsilonFeet)
                {
                    dedup.Add(p);
                }
            }

            if (dedup.Count > 1 && dedup[^1].DistanceTo(dedup[0]) <= epsilonFeet)
            {
                dedup.RemoveAt(dedup.Count - 1);
            }

            return (dedup, loopClosed);
        }

        // Project XYZ (feet) -> 2D mm by dropping the extrusion-axis coordinate.
        double First(XYZ p) => FeetToMm(axis == "X" ? p.Y : p.X);
        double Second(XYZ p) => FeetToMm(axis == "Z" ? p.Y : p.Z);

        // Shoelace signed area; positive = counter-clockwise in (x, y).
        static double SignedArea(List<(double X, double Y)> pts)
        {
            double sum = 0;
            for (var i = 0; i < pts.Count; i++)
            {
                var j = (i + 1) % pts.Count;
                sum += (pts[i].X * pts[j].Y) - (pts[j].X * pts[i].Y);
            }

            return sum * 0.5;
        }

        // Chain EVERY edge loop on the cap face (outer boundary + interior voids).
        var rawLoops = new List<(List<(double X, double Y)> Pts, bool Closed)>();
        var anyOpen = false;
        foreach (EdgeArray loop in cap.Face.EdgeLoops)
        {
            var (xyz, loopClosed) = ChainLoop(loop);
            if (!loopClosed)
            {
                anyOpen = true;
            }

            if (xyz.Count < 3)
            {
                continue; // skip degenerate loops
            }

            rawLoops.Add((xyz.Select(p => (X: First(p), Y: Second(p))).ToList(), loopClosed));
        }

        if (rawLoops.Count == 0)
        {
            throw new InvalidOperationException("Cap face produced no usable loops.");
        }

        // Outer loop = largest absolute area; the rest are interior voids.
        var outerIndex = 0;
        var outerArea = -1.0;
        for (var i = 0; i < rawLoops.Count; i++)
        {
            var area = Math.Abs(SignedArea(rawLoops[i].Pts));
            if (area > outerArea)
            {
                outerArea = area;
                outerIndex = i;
            }
        }

        // One shared normalization shift across all loops, so voids stay positioned correctly.
        var allPts = rawLoops.SelectMany(r => r.Pts).ToList();
        var shiftX = request.NormalizeToOrigin ? allPts.Min(p => p.X) : 0.0;
        var shiftY = request.NormalizeToOrigin ? allPts.Min(p => p.Y) : 0.0;

        var loops = new List<ReconstructedLoop>();
        for (var i = 0; i < rawLoops.Count; i++)
        {
            var shifted = rawLoops[i].Pts
                .Select(p => (X: p.X - shiftX, Y: p.Y - shiftY))
                .ToList();

            // Fix mirrored output: dropping an axis can flip winding — force CCW (positive area).
            if (SignedArea(shifted) < 0)
            {
                shifted.Reverse();
            }

            loops.Add(new ReconstructedLoop
            {
                IsOuter = i == outerIndex,
                PointCount = shifted.Count,
                Profile = shifted.Select(p => new Profile2DPoint { X = p.X, Y = p.Y }).ToList(),
            });
        }

        // Extrusion depth = bounding-box extent along the extrusion axis (mm).
        var bb = element.get_BoundingBox(null);
        var depthMm = bb is null
            ? 0.0
            : FeetToMm(axis switch
            {
                "X" => bb.Max.X - bb.Min.X,
                "Y" => bb.Max.Y - bb.Min.Y,
                _ => bb.Max.Z - bb.Min.Z,
            });

        var allFinal = loops.SelectMany(l => l.Profile).ToList();
        var box2D = new BoundingBox2DData
        {
            MinX = allFinal.Min(p => p.X),
            MinY = allFinal.Min(p => p.Y),
            MaxX = allFinal.Max(p => p.X),
            MaxY = allFinal.Max(p => p.Y),
        };

        return new ReconstructProfileResult
        {
            ElementId = ToInt(element.Id),
            ExtrusionDepth = depthMm,
            ExtrusionAxis = axis,
            Loops = loops,
            BoundingBox2D = box2D,
            Closed = !anyOpen,
            // Partial data + message on failure so the caller can debug rather than get a bare 500.
            Note = anyOpen
                ? "One or more loops could not be closed — epsilon too small or geometry has gaps. "
                    + "Returned the partial chained loop(s) for inspection."
                : null,
        };
    }

    // ───────────────────────────────────────────────────────────────────────
    // Group 11 — read-only project query tools. All require an active project
    // document (RequireProjectDocument throws a clear error inside a family editor).
    // ───────────────────────────────────────────────────────────────────────

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
                Id = ToInt(l.Id),
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
                // Multi-segment / non-linear grids may not expose a single curve; leave endpoints null.
            }

            records.Add(new GridRecord
            {
                Id = ToInt(grid.Id),
                Name = grid.Name,
                Start = start,
                End = end,
            });
        }

        return new GetGridsResult { Grids = records };
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
                Id = ToInt(s.Id),
                Name = s.Name,
                FamilyName = s.FamilyName,
                Category = s.Category is not null ? ToCategoryToken(s.Category) : string.Empty,
                IsActive = s.IsActive,
            })
            .ToList();

        return new GetElementTypesResult { Types = records, TotalCount = symbols.Count };
    }

    private static QueryElementsResult QueryElements(UIApplication app, QueryElementsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var limit = request.Limit <= 0 ? 50 : request.Limit;

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

        // Bounding-box filter only when all six extents are supplied.
        if (request.BboxMinX is not null && request.BboxMinY is not null && request.BboxMinZ is not null
            && request.BboxMaxX is not null && request.BboxMaxY is not null && request.BboxMaxZ is not null)
        {
            var min = new XYZ(MmToFeet(request.BboxMinX.Value), MmToFeet(request.BboxMinY.Value), MmToFeet(request.BboxMinZ.Value));
            var max = new XYZ(MmToFeet(request.BboxMaxX.Value), MmToFeet(request.BboxMaxY.Value), MmToFeet(request.BboxMaxZ.Value));
            collector = collector.WherePasses(new BoundingBoxIntersectsFilter(new Outline(min, max)));
        }

        var matched = collector.ToElements();
        var records = matched
            .Take(limit)
            .Select(e => new QueriedElementRecord
            {
                Id = ToInt(e.Id),
                Name = e.Name,
                Category = e.Category is not null ? ToCategoryToken(e.Category) : string.Empty,
                FamilyName = ResolveElementFamilyName(doc, e),
                Location = ElementCenter(e),
            })
            .ToList();

        return new QueryElementsResult { Elements = records, TotalCount = matched.Count };
    }

    private static GetElementByIdResult GetElementById(UIApplication app, GetElementByIdRequest request)
    {
        var doc = RequireProjectDocument(app);
        var element = doc.GetElement(new ElementId(request.ElementId))
            ?? throw new InvalidOperationException($"Element {request.ElementId} was not found.");

        var typeId = element.GetTypeId();
        var elementType = typeId != ElementId.InvalidElementId ? doc.GetElement(typeId) as ElementType : null;

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (request.IncludeParameters)
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

        return new GetElementByIdResult
        {
            Id = ToInt(element.Id),
            Name = element.Name,
            Category = element.Category is not null ? ToCategoryToken(element.Category) : string.Empty,
            FamilyName = ResolveElementFamilyName(doc, element),
            TypeName = elementType?.Name ?? string.Empty,
            Location = ElementCenter(element),
            Parameters = parameters,
        };
    }

    // Parses an optional "OST_*" category token; null/empty -> no filter; unknown -> clear error.
    private static long? ResolveOptionalCategoryId(string? categoryToken)
    {
        var token = categoryToken?.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        if (Enum.TryParse<BuiltInCategory>(token, ignoreCase: true, out var bic))
        {
            return (long)bic;
        }

        throw new InvalidOperationException(
            $"Unknown category token '{token}'. Use an OST_* BuiltInCategory token (e.g. OST_Walls).");
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

    private static GeomPoint ElementCenter(Element element)
    {
        var box = element.get_BoundingBox(null);
        if (box is not null)
        {
            return ToGeomPoint((box.Min + box.Max) * 0.5);
        }

        return element.Location is LocationPoint lp ? ToGeomPoint(lp.Point) : new GeomPoint();
    }

    // Human-readable value for an instance parameter (formatted with units where applicable).
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

    // ───────────────────────────────────────────────────────────────────────
    // Model Snapshot Export (v18). Read-only, bulk. Serializes a full snapshot to
    // a caller-supplied file on disk from C#; only ExportSummaryResult returns via MCP.
    // All spatial values in the file are millimetres; angles degrees; orientation
    // vectors are unitless. Per-element try/catch — a bad element becomes an errors[]
    // entry and iteration continues.
    // ───────────────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions ExportFileJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private static ExportSummaryResult ExportInstances(UIApplication app, ExportInstancesRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (doc.IsFamilyDocument)
        {
            throw new InvalidOperationException(
                "Active document is a family document. Open a project (.rvt) document first.");
        }

        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            throw new InvalidOperationException("outputPath is required.");
        }

        var tokens = (request.Categories ?? new List<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("categories must contain at least one OST_* token.");
        }

        var bics = new List<BuiltInCategory>();
        foreach (var token in tokens)
        {
            if (Enum.TryParse<BuiltInCategory>(token, ignoreCase: true, out var bic))
            {
                bics.Add(bic);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unknown category token '{token}'. Use OST_* BuiltInCategory tokens (e.g. OST_StructuralFraming).");
            }
        }

        var collector = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .WherePasses(new ElementMulticategoryFilter(bics));

        var records = new List<object>();
        var errors = new List<object>();

        foreach (var element in collector)
        {
            try
            {
                records.Add(BuildInstanceRecord(doc, element));
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = SafeElementId(element),
                    ["message"] = ex.Message,
                });
            }
        }

        var content = new Dictionary<string, object?>
        {
            ["model"] = doc.Title,
            ["exportedAt"] = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
            ["categories"] = tokens,
            ["elementCount"] = records.Count,
            ["errorCount"] = errors.Count,
            ["instances"] = records,
            ["errors"] = errors,
        };

        return WriteExportFile(request.OutputPath, content, records.Count, errors.Count);
    }

    private static ExportSummaryResult WriteExportFile(
        string outputPath, object content, int elementCount, int errorCount)
    {
        var fullPath = Path.GetFullPath(outputPath.Trim());
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(content, ExportFileJsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json); // UTF-8 without BOM
        File.WriteAllBytes(fullPath, bytes);      // overwrite existing

        return new ExportSummaryResult
        {
            OutputPath = fullPath,
            ElementCount = elementCount,
            ByteSize = bytes.LongLength,
            ErrorCount = errorCount,
        };
    }

    private static object SafeElementId(Element element)
    {
        try
        {
            return ToInt(element.Id);
        }
        catch
        {
            return -1;
        }
    }

    private static Dictionary<string, object?> BuildInstanceRecord(Document doc, Element element)
    {
        var fi = element as FamilyInstance;
        var typeId = element.GetTypeId();
        var elementType = typeId != ElementId.InvalidElementId ? doc.GetElement(typeId) as ElementType : null;

        ResolveElementLevel(doc, element, out var levelName, out var levelId);

        string? placementType = null;
        if (fi?.Symbol?.Family is Family fam)
        {
            placementType = fam.FamilyPlacementType.ToString();
        }

        object? orientation = null;
        bool? facingFlipped = null;
        bool? handFlipped = null;
        bool? mirrored = null;
        int? hostElementId = null;
        object? hostFace = null;
        if (fi is not null)
        {
            try
            {
                orientation = new Dictionary<string, object?>
                {
                    ["facingOrientation"] = ToVector(fi.FacingOrientation),
                    ["handOrientation"] = ToVector(fi.HandOrientation),
                };
            }
            catch { /* orientation not available */ }

            try { facingFlipped = fi.FacingFlipped; } catch { }
            try { handFlipped = fi.HandFlipped; } catch { }
            try { mirrored = fi.Mirrored; } catch { }
            try { if (fi.Host is not null) hostElementId = ToInt(fi.Host.Id); } catch { }

            hostFace = BuildHostFace(doc, fi);
        }

        string? hostGuidParam = null;
        int? hostGuidDecodedId = null;
        try
        {
            var hg = element.LookupParameter("HOST_GUID");
            if (hg is not null && hg.HasValue && hg.StorageType == StorageType.String)
            {
                hostGuidParam = hg.AsString();
                hostGuidDecodedId = DecodeHostGuidSuffix(hostGuidParam);
            }
        }
        catch { /* no HOST_GUID */ }

        var phaseCreated = string.Empty;
        var phaseDemolished = string.Empty;
        try { phaseCreated = PhaseName(doc, element.CreatedPhaseId); } catch { }
        try { phaseDemolished = PhaseName(doc, element.DemolishedPhaseId); } catch { }

        return new Dictionary<string, object?>
        {
            ["elementId"] = ToInt(element.Id),
            ["uniqueId"] = element.UniqueId,
            ["category"] = element.Category is not null ? ToCategoryToken(element.Category) : string.Empty,
            ["familyName"] = ResolveElementFamilyName(doc, element),
            ["typeName"] = elementType?.Name ?? string.Empty,
            ["typeId"] = typeId != ElementId.InvalidElementId ? ToInt(typeId) : (int?)null,
            ["levelName"] = levelName,
            ["levelId"] = levelId,
            ["placementType"] = placementType,
            ["placement"] = BuildPlacement(element),
            ["orientation"] = orientation,
            ["facingFlipped"] = facingFlipped,
            ["handFlipped"] = handFlipped,
            ["mirrored"] = mirrored,
            ["phaseCreated"] = phaseCreated,
            ["phaseDemolished"] = phaseDemolished,
            ["hostElementId"] = hostElementId,
            ["hostFace"] = hostFace,
            ["hostGuidParam"] = hostGuidParam,
            ["hostGuidDecodedId"] = hostGuidDecodedId,
            ["parameters"] = BuildParameterMap(doc, element.Parameters),
        };
    }

    // Unit direction vector (unitless) straight from the API — NOT converted to mm.
    private static Dictionary<string, object?> ToVector(XYZ v) => new()
    {
        ["x"] = v.X,
        ["y"] = v.Y,
        ["z"] = v.Z,
    };

    private static object? BuildPlacement(Element element)
    {
        var loc = element.Location;
        if (loc is LocationCurve lc && lc.Curve is Curve curve)
        {
            var record = new Dictionary<string, object?>
            {
                ["kind"] = "curve",
                ["start"] = ToGeomPoint(curve.GetEndPoint(0)),
                ["end"] = ToGeomPoint(curve.GetEndPoint(1)),
            };
            if (curve is Arc arc)
            {
                record["center"] = ToGeomPoint(arc.Center);
                record["radiusMm"] = FeetToMm(arc.Radius);
                record["note"] = "arc";
            }

            return record;
        }

        if (loc is LocationPoint lp)
        {
            double rotationDeg = 0;
            try { rotationDeg = lp.Rotation * 180.0 / Math.PI; } catch { }
            return new Dictionary<string, object?>
            {
                ["kind"] = "point",
                ["point"] = ToGeomPoint(lp.Point),
                ["rotationDeg"] = rotationDeg,
            };
        }

        return null; // sketch-based (e.g. Floor) or no location
    }

    // Face/work-plane host resolution. Returns null for non-hosted instances. Face
    // references do not survive across documents, so we export normal + point (portable)
    // rather than the Reference stable string. Wrapped so a bad host never kills the row.
    private static object? BuildHostFace(Document doc, FamilyInstance fi)
    {
        try
        {
            var hostRef = fi.HostFace;
            var host = fi.Host;
            if (hostRef is null)
            {
                if (host is not null)
                {
                    return new Dictionary<string, object?>
                    {
                        ["hostElementId"] = ToInt(host.Id),
                        ["faceResolved"] = false,
                    };
                }

                return null;
            }

            var hostElement = host ?? doc.GetElement(hostRef.ElementId);
            var face = hostElement?.GetGeometryObjectFromReference(hostRef) as Face;
            var locPoint = (fi.Location as LocationPoint)?.Point;

            GeomPoint? facePoint = null;
            object? faceNormal = null;
            if (face is not null && locPoint is not null)
            {
                var projection = face.Project(locPoint);
                if (projection is not null)
                {
                    facePoint = ToGeomPoint(projection.XYZPoint);
                    faceNormal = ToVector(face.ComputeNormal(projection.UVPoint));
                }
            }

            string? workPlaneName = null;
            try
            {
                var wp = fi.LookupParameter("Work Plane");
                if (wp is not null && wp.HasValue)
                {
                    workPlaneName = wp.AsString() ?? wp.AsValueString();
                }
            }
            catch { }

            return new Dictionary<string, object?>
            {
                ["hostElementId"] = hostElement is not null ? ToInt(hostElement.Id) : (int?)null,
                ["faceNormal"] = faceNormal,
                ["facePoint"] = facePoint,
                ["workPlaneName"] = workPlaneName,
                ["faceResolved"] = face is not null,
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object?>
            {
                ["faceResolved"] = false,
                ["error"] = ex.Message,
            };
        }
    }

    // Revit UniqueId format: <episodeGuid>-<8 hex chars>; the trailing 8 hex chars are the
    // element id. Null-safe: returns null if absent or unparseable.
    private static int? DecodeHostGuidSuffix(string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid) || guid!.Length < 8)
        {
            return null;
        }

        var suffix = guid.Substring(guid.Length - 8);
        return int.TryParse(suffix, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)
            ? value
            : (int?)null;
    }

    private static string PhaseName(Document doc, ElementId phaseId)
    {
        if (phaseId is null || phaseId == ElementId.InvalidElementId)
        {
            return string.Empty;
        }

        return doc.GetElement(phaseId) is Phase phase ? phase.Name : string.Empty;
    }

    // Level via Element.LevelId, falling back to reference-level parameters. null id/name ok.
    private static void ResolveElementLevel(Document doc, Element element, out string? levelName, out int? levelId)
    {
        levelName = null;
        levelId = null;

        var lid = ElementId.InvalidElementId;
        try { lid = element.LevelId; } catch { }

        if (lid is null || lid == ElementId.InvalidElementId)
        {
            var levelParam = element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                ?? element.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM)
                ?? element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM)
                ?? element.get_Parameter(BuiltInParameter.LEVEL_PARAM);
            if (levelParam is not null && levelParam.StorageType == StorageType.ElementId)
            {
                var pid = levelParam.AsElementId();
                if (pid is not null && pid != ElementId.InvalidElementId)
                {
                    lid = pid;
                }
            }
        }

        if (lid is not null && lid != ElementId.InvalidElementId && doc.GetElement(lid) is Level level)
        {
            levelName = level.Name;
            levelId = ToInt(level.Id);
        }
    }

    // { paramName: { value: string, raw: number|null } } over a parameter set.
    private static Dictionary<string, object?> BuildParameterMap(Document doc, ParameterSet parameters)
    {
        var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (Parameter p in parameters)
        {
            try
            {
                if (p?.Definition is null || !p.HasValue)
                {
                    continue;
                }

                var key = p.Definition.Name;
                if (string.IsNullOrWhiteSpace(key) || map.ContainsKey(key))
                {
                    continue;
                }

                map[key] = new Dictionary<string, object?>
                {
                    ["value"] = ParameterValueToString(doc, p),
                    ["raw"] = RawParameterValue(p),
                };
            }
            catch { /* skip a single bad parameter */ }
        }

        return map;
    }

    // Rebuildable raw value: Double length -> mm, Double angle -> degrees, other Double
    // pass-through (internal units); Integer -> int; ElementId -> id value; String -> null.
    private static object? RawParameterValue(Parameter p)
    {
        try
        {
            switch (p.StorageType)
            {
                case StorageType.Double:
                {
                    var d = p.AsDouble();
                    var dataType = p.Definition.GetDataType();
                    if (dataType is not null && dataType.Equals(SpecTypeId.Angle))
                    {
                        return d * 180.0 / Math.PI;
                    }

                    if (dataType is not null && dataType.Equals(SpecTypeId.Length))
                    {
                        return d * 304.8;
                    }

                    return d;
                }

                case StorageType.Integer:
                    return p.AsInteger();
                case StorageType.ElementId:
                {
                    var id = p.AsElementId();
                    return id is null || id == ElementId.InvalidElementId ? (object?)null : id.Value;
                }

                case StorageType.String:
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

    // Parse OST_* tokens into a validated set (throws on unknown token). Shared by the
    // category-scoped export tools.
    private static (List<string> tokens, HashSet<string> set) ParseCategoryTokens(List<string>? categories)
    {
        var tokens = (categories ?? new List<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("categories must contain at least one OST_* token.");
        }

        foreach (var token in tokens)
        {
            if (!Enum.TryParse<BuiltInCategory>(token, ignoreCase: true, out _))
            {
                throw new InvalidOperationException(
                    $"Unknown category token '{token}'. Use OST_* BuiltInCategory tokens (e.g. OST_StructuralFraming).");
            }
        }

        return (tokens, tokens.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static ExportSummaryResult ExportTypeParameters(UIApplication app, ExportTypeParametersRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            throw new InvalidOperationException("outputPath is required.");
        }

        var (tokens, tokenSet) = ParseCategoryTokens(request.Categories);

        var symbols = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>();

        var records = new List<object>();
        var errors = new List<object>();

        foreach (var symbol in symbols)
        {
            try
            {
                var token = symbol.Category is not null ? ToCategoryToken(symbol.Category) : string.Empty;
                if (!tokenSet.Contains(token))
                {
                    continue;
                }

                records.Add(new Dictionary<string, object?>
                {
                    ["typeId"] = ToInt(symbol.Id),
                    ["familyName"] = symbol.FamilyName,
                    ["typeName"] = symbol.Name,
                    ["category"] = token,
                    ["parameters"] = BuildParameterMap(doc, symbol.Parameters),
                });
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = SafeElementId(symbol),
                    ["message"] = ex.Message,
                });
            }
        }

        var content = new Dictionary<string, object?>
        {
            ["model"] = doc.Title,
            ["exportedAt"] = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
            ["categories"] = tokens,
            ["elementCount"] = records.Count,
            ["errorCount"] = errors.Count,
            ["types"] = records,
            ["errors"] = errors,
        };

        return WriteExportFile(request.OutputPath, content, records.Count, errors.Count);
    }

    private static ExportSummaryResult ExportParameterBindings(UIApplication app, ExportParameterBindingsRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            throw new InvalidOperationException("outputPath is required.");
        }

        // Collect all shared parameter elements once; index by their internal definition id.
        var sharedElements = new FilteredElementCollector(doc)
            .OfClass(typeof(SharedParameterElement))
            .Cast<SharedParameterElement>()
            .ToList();

        var sharedByDefId = new Dictionary<long, SharedParameterElement>();
        foreach (var spe in sharedElements)
        {
            try
            {
                if (spe.GetDefinition() is InternalDefinition def)
                {
                    sharedByDefId[def.Id.Value] = spe;
                }
            }
            catch { /* skip unreadable shared parameter */ }
        }

        var bindings = new List<object>();
        var errors = new List<object>();

        var iterator = doc.ParameterBindings.ForwardIterator();
        iterator.Reset();
        while (iterator.MoveNext())
        {
            try
            {
                var definition = iterator.Key;
                var binding = iterator.Current as ElementBinding;
                var internalDef = definition as InternalDefinition;

                var name = definition?.Name ?? string.Empty;
                var isInstance = binding is InstanceBinding;

                var group = string.Empty;
                try
                {
                    var groupTypeId = definition?.GetGroupTypeId();
                    if (groupTypeId is not null && !groupTypeId.Empty())
                    {
                        group = LabelUtils.GetLabelForGroup(groupTypeId);
                    }
                }
                catch { /* group not resolvable */ }

                var dataType = string.Empty;
                try { dataType = definition?.GetDataType()?.TypeId ?? string.Empty; } catch { }

                var boundCategories = new List<string>();
                if (binding?.Categories is not null)
                {
                    foreach (Category category in binding.Categories)
                    {
                        boundCategories.Add(category.Name);
                    }
                }

                string? guid = null;
                var isShared = false;
                // Match by definition id (unique per parameter — implies name match too).
                if (internalDef is not null && sharedByDefId.TryGetValue(internalDef.Id.Value, out var spe))
                {
                    guid = spe.GuidValue.ToString();
                    isShared = true;
                }

                bindings.Add(new Dictionary<string, object?>
                {
                    ["name"] = name,
                    ["isInstance"] = isInstance,
                    ["group"] = group,
                    ["dataType"] = dataType,
                    ["boundCategories"] = boundCategories,
                    ["guid"] = guid,
                    ["isShared"] = isShared,
                });
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = -1,
                    ["message"] = ex.Message,
                });
            }
        }

        // Full shared-parameter inventory (even if unbound) — highest-value output.
        var allShared = new List<object>();
        foreach (var spe in sharedElements)
        {
            try
            {
                allShared.Add(new Dictionary<string, object?>
                {
                    ["name"] = spe.Name,
                    ["guid"] = spe.GuidValue.ToString(),
                });
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = SafeElementId(spe),
                    ["message"] = ex.Message,
                });
            }
        }

        var content = new Dictionary<string, object?>
        {
            ["model"] = doc.Title,
            ["exportedAt"] = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
            ["bindingCount"] = bindings.Count,
            ["sharedParameterCount"] = allShared.Count,
            ["errorCount"] = errors.Count,
            ["bindings"] = bindings,
            ["allSharedParameterElements"] = allShared,
            ["errors"] = errors,
        };

        // elementCount summary = number of bindings (the primary record set).
        return WriteExportFile(request.OutputPath, content, bindings.Count, errors.Count);
    }

    private static ExportSummaryResult ExportMaterials(UIApplication app, ExportMaterialsRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            throw new InvalidOperationException("outputPath is required.");
        }

        var materials = new FilteredElementCollector(doc)
            .OfClass(typeof(Material))
            .Cast<Material>();

        var records = new List<object>();
        var errors = new List<object>();

        foreach (var material in materials)
        {
            try
            {
                object? color = null;
                try
                {
                    var c = material.Color;
                    if (c is not null && c.IsValid)
                    {
                        color = new Dictionary<string, object?> { ["r"] = c.Red, ["g"] = c.Green, ["b"] = c.Blue };
                    }
                }
                catch { /* no color */ }

                string? structuralAssetName = null;
                try
                {
                    var sid = material.StructuralAssetId;
                    if (sid != ElementId.InvalidElementId)
                    {
                        structuralAssetName = doc.GetElement(sid)?.Name;
                    }
                }
                catch { }

                string? appearanceAssetName = null;
                try
                {
                    var aid = material.AppearanceAssetId;
                    if (aid != ElementId.InvalidElementId)
                    {
                        appearanceAssetName = doc.GetElement(aid)?.Name;
                    }
                }
                catch { }

                records.Add(new Dictionary<string, object?>
                {
                    ["id"] = ToInt(material.Id),
                    ["name"] = material.Name,
                    ["materialClass"] = material.MaterialClass,
                    ["color"] = color,
                    ["transparency"] = material.Transparency,
                    ["shininess"] = material.Shininess,
                    ["smoothness"] = material.Smoothness,
                    ["structuralAssetName"] = structuralAssetName,
                    ["appearanceAssetName"] = appearanceAssetName,
                });
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = SafeElementId(material),
                    ["message"] = ex.Message,
                });
            }
        }

        var content = new Dictionary<string, object?>
        {
            ["model"] = doc.Title,
            ["exportedAt"] = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
            ["elementCount"] = records.Count,
            ["errorCount"] = errors.Count,
            ["materials"] = records,
            ["errors"] = errors,
        };

        return WriteExportFile(request.OutputPath, content, records.Count, errors.Count);
    }

    private static ExportSummaryResult ExportViews(UIApplication app, ExportViewsRequest request)
    {
        var doc = RequireProjectDocument(app);
        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            throw new InvalidOperationException("outputPath is required.");
        }

        var errors = new List<object>();

        // Build viewportId -> viewId map and a viewId -> (sheetId, sheetNumber) index first.
        var viewportMap = new Dictionary<string, object?>();
        var sheetByViewId = new Dictionary<long, (int SheetId, string SheetNumber)>();
        foreach (var viewport in new FilteredElementCollector(doc).OfClass(typeof(Viewport)).Cast<Viewport>())
        {
            try
            {
                viewportMap[ToInt(viewport.Id).ToString(CultureInfo.InvariantCulture)] = ToInt(viewport.ViewId);
                var sheet = doc.GetElement(viewport.SheetId) as ViewSheet;
                sheetByViewId[viewport.ViewId.Value] = (ToInt(viewport.SheetId), sheet?.SheetNumber ?? string.Empty);
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = SafeElementId(viewport),
                    ["message"] = ex.Message,
                });
            }
        }

        var records = new List<object>();
        foreach (var view in new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>())
        {
            try
            {
                if (view.IsTemplate)
                {
                    continue;
                }

                int? scale = null;
                try { scale = view.Scale; } catch { /* view type has no scale */ }

                string? viewTemplateName = null;
                var templateId = view.ViewTemplateId;
                if (templateId != ElementId.InvalidElementId)
                {
                    viewTemplateName = doc.GetElement(templateId)?.Name;
                }

                var isOnSheet = sheetByViewId.TryGetValue(view.Id.Value, out var sheetInfo);

                records.Add(new Dictionary<string, object?>
                {
                    ["viewId"] = ToInt(view.Id),
                    ["name"] = view.Name,
                    ["viewType"] = view.ViewType.ToString(),
                    ["scale"] = scale,
                    ["viewTemplateName"] = viewTemplateName,
                    ["isOnSheet"] = isOnSheet,
                    ["sheetId"] = isOnSheet ? sheetInfo.SheetId : (int?)null,
                    ["sheetNumber"] = isOnSheet ? sheetInfo.SheetNumber : null,
                });
            }
            catch (Exception ex)
            {
                errors.Add(new Dictionary<string, object?>
                {
                    ["elementId"] = SafeElementId(view),
                    ["message"] = ex.Message,
                });
            }
        }

        var content = new Dictionary<string, object?>
        {
            ["model"] = doc.Title,
            ["exportedAt"] = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
            ["elementCount"] = records.Count,
            ["errorCount"] = errors.Count,
            ["views"] = records,
            ["viewportMap"] = viewportMap,
            ["errors"] = errors,
        };

        return WriteExportFile(request.OutputPath, content, records.Count, errors.Count);
    }

    private static ElementCreationResult CreateBlend(UIApplication app, CreateBlendRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var depthFeet = MmToFeet(request.Depth);
        if (Math.Abs(depthFeet) < 1e-9)
        {
            throw new InvalidOperationException("depth must be non-zero.");
        }

        var plane = ResolveWorkPlane(doc, request.WorkPlaneName);

        // Preferred path: a true Blend element. NewBlend has no depth argument — the
        // bottom loop sits on the sketch plane and separation is applied via TopOffset.
        try
        {
            using var tx = new Transaction(doc, "Bridge create blend");
            tx.Start();
            var sketchPlane = SketchPlane.Create(doc, plane);
            doc.Regenerate();
            var top = BuildCurveArrayLoop(plane, request.TopProfile);
            var bottom = BuildCurveArrayLoop(plane, request.BottomProfile);
            var blend = doc.FamilyCreate.NewBlend(request.IsSolid, top, bottom, sketchPlane);
            blend.TopOffset = depthFeet;
            doc.Regenerate();
            tx.Commit();
            return new ElementCreationResult { ElementId = ToInt(blend.Id), Success = true };
        }
        catch (Autodesk.Revit.Exceptions.InternalException)
        {
            // NewBlend raises "Unexpected internal error: code 1" in this environment
            // regardless of profile winding/shape. Realize identical geometry as a
            // straight swept blend along the work-plane normal (NewSweptBlend is reliable
            // here). The disposed transaction above rolls back any partial state cleanly.
        }

        using var fallbackTx = new Transaction(doc, "Bridge create blend (swept-blend fallback)");
        fallbackTx.Start();
        var start = plane.Origin;
        var end = plane.Origin + (depthFeet * plane.Normal);
        var pathLine = Line.CreateBound(start, end);
        var pathSketchPlane = SketchPlane.Create(doc, PlaneContainingPath([start, end]));
        var bottomProfile = BuildSweepProfileFromXy(app, request.BottomProfile);
        var topProfile = BuildSweepProfileFromXy(app, request.TopProfile);
        var sweptBlend = doc.FamilyCreate.NewSweptBlend(
            request.IsSolid, pathLine, pathSketchPlane, bottomProfile, topProfile);
        doc.Regenerate();
        fallbackTx.Commit();

        return new ElementCreationResult { ElementId = ToInt(sweptBlend.Id), Success = true };
    }

    private static ElementCreationResult CreateRevolve(UIApplication app, CreateRevolveRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (string.IsNullOrWhiteSpace(request.AxisReferenceName))
        {
            throw new InvalidOperationException("axisReferenceName is required.");
        }

        var plane = ResolveWorkPlane(doc, request.WorkPlaneName);
        var axisName = request.AxisReferenceName.Trim();
        var refPlanes = new FilteredElementCollector(doc)
            .OfClass(typeof(ReferencePlane))
            .Cast<ReferencePlane>()
            .ToList();
        var axisPlane = refPlanes
            .FirstOrDefault(rp => string.Equals(rp.Name, axisName, StringComparison.OrdinalIgnoreCase));
        if (axisPlane is null)
        {
            var available = refPlanes
                .Select(rp => rp.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var listing = available.Count > 0
                ? string.Join(", ", available.Select(n => $"'{n}'"))
                : "(none found in the active document)";
            throw new InvalidOperationException(
                $"Axis reference plane '{axisName}' was not found. Reference planes in the active family document: {listing}.");
        }

        var startAngle = request.StartAngleDeg * Math.PI / 180.0;
        var endAngle = request.EndAngleDeg * Math.PI / 180.0;

        using var tx = new Transaction(doc, "Bridge create revolve");
        tx.Start();
        var sketchPlane = SketchPlane.Create(doc, plane);
        var profile = BuildPlanarLoop(plane, request.Profile);
        var axis = Line.CreateBound(axisPlane.BubbleEnd, axisPlane.FreeEnd);
        var revolve = doc.FamilyCreate.NewRevolution(
            request.IsSolid, profile, sketchPlane, axis, startAngle, endAngle);
        doc.Regenerate();
        tx.Commit();

        return new ElementCreationResult { ElementId = ToInt(revolve.Id), Success = true };
    }

    private static ElementCreationResult CreateSweep(UIApplication app, CreateSweepRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var worldPath = ToWorldPath(request.Path);

        using var tx = new Transaction(doc, "Bridge create sweep");
        tx.Start();
        var pathPlane = PlaneContainingPath(worldPath);
        var pathSketchPlane = SketchPlane.Create(doc, pathPlane);
        var pathCurves = BuildPathCurves(worldPath);
        var profile = BuildSweepProfileFromXy(app, request.Profile);
        var sweep = doc.FamilyCreate.NewSweep(
            request.IsSolid, pathCurves, pathSketchPlane, profile, 0, ProfilePlaneLocation.Start);
        doc.Regenerate();
        tx.Commit();

        return new ElementCreationResult { ElementId = ToInt(sweep.Id), Success = true };
    }

    private static ElementCreationResult CreateSweptBlend(UIApplication app, CreateSweptBlendRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var worldPath = ToWorldPath(request.Path);
        // This NewSweptBlend overload accepts a single-curve path; use the first segment.
        var pathLine = Line.CreateBound(worldPath[0], worldPath[1]);

        using var tx = new Transaction(doc, "Bridge create swept blend");
        tx.Start();
        var pathPlane = PlaneContainingPath(worldPath);
        var pathSketchPlane = SketchPlane.Create(doc, pathPlane);
        var bottom = BuildSweepProfileFromXy(app, request.StartProfile);
        var top = BuildSweepProfileFromXy(app, request.EndProfile);
        var sweptBlend = doc.FamilyCreate.NewSweptBlend(
            request.IsSolid, pathLine, pathSketchPlane, bottom, top);
        doc.Regenerate();
        tx.Commit();

        return new ElementCreationResult { ElementId = ToInt(sweptBlend.Id), Success = true };
    }

    private static OperationResult SetGeometrySolidVoid(UIApplication app, SetGeometrySolidVoidRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var element = doc.GetElement(new ElementId(request.ElementId))
            ?? throw new InvalidOperationException($"Element {request.ElementId} was not found.");
        if (element is not GenericForm form)
        {
            throw new InvalidOperationException(
                $"Element {request.ElementId} is not a form (extrusion/blend/revolve/sweep/swept blend).");
        }

        if (form.IsSolid == request.IsSolid)
        {
            return new OperationResult { Success = true }; // Already in the requested state.
        }

        // GenericForm.IsSolid is read-only and there is no GEOM solid/void BuiltInParameter.
        // The only settable handle is the localized "Solid/Void" family parameter (0 = Solid, 1 = Void).
        using var tx = new Transaction(doc, "Bridge set solid/void");
        tx.Start();
        var param = form.LookupParameter("Solid/Void");
        if (param is null || param.IsReadOnly || param.StorageType != StorageType.Integer)
        {
            tx.RollBack();
            throw new InvalidOperationException(
                $"Cannot toggle solid/void on element {request.ElementId}: the Revit API exposes " +
                "GenericForm.IsSolid as read-only and no settable 'Solid/Void' parameter is available. " +
                "This change requires deleting and recreating the form.");
        }

        param.Set(request.IsSolid ? 0 : 1);
        doc.Regenerate();
        tx.Commit();

        return new OperationResult { Success = form.IsSolid == request.IsSolid };
    }

    // Resolves an element to a dimensionable Reference. Reference planes are the core
    // family-parametrization target; form edges/faces would need geometry-reference
    // extraction and are intentionally not supported yet (clear error instead of guessing).
    private static Reference? ResolveDimensionReference(Element element)
    {
        return element switch
        {
            ReferencePlane rp => rp.GetReference(),
            _ => null,
        };
    }

    // Validates that a parameter is a legal dimension label BEFORE touching the API.
    // Setting Dimension.FamilyLabel to a non-Length / built-in parameter can hang the
    // Revit thread, so we fail fast with a clear error instead.
    private static FamilyParameter ResolveLabelableLengthParameter(Document doc, string parameterName)
    {
        var name = (parameterName ?? string.Empty).Trim();
        FamilyParameter? fp = string.IsNullOrEmpty(name) ? null : doc.FamilyManager.get_Parameter(name);
        if (fp is null || fp.StorageType != StorageType.Double)
        {
            throw new InvalidOperationException(
                $"Parameter '{name}' not found or is not a Length type. " +
                "Dimension labels require a custom Length parameter.");
        }

        return fp;
    }

    private static ElementCreationResult CreateDimension(UIApplication app, CreateDimensionRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (request.ReferenceIds is null || request.ReferenceIds.Count != 2)
        {
            throw new InvalidOperationException("referenceIds must contain exactly two element ids.");
        }

        var view = doc.ActiveView
            ?? throw new InvalidOperationException("No active view available to host the dimension.");

        var references = new ReferenceArray();
        foreach (var id in request.ReferenceIds)
        {
            var element = doc.GetElement(new ElementId(id))
                ?? throw new InvalidOperationException($"Element {id} was not found in the active family document.");
            var reference = ResolveDimensionReference(element)
                ?? throw new InvalidOperationException(
                    $"Element {id} ({element.GetType().Name}) cannot be used as a dimension reference. " +
                    "Only reference planes are supported as dimension targets currently.");
            references.Append(reference);
        }

        var start = ToInternalPoint(request.Line.Start);
        var end = ToInternalPoint(request.Line.End);
        if (start.IsAlmostEqualTo(end))
        {
            throw new InvalidOperationException("line.start and line.end must differ.");
        }

        var line = Line.CreateBound(start, end);

        using var tx = new Transaction(doc, "Bridge create dimension");
        tx.Start();
        var dimension = doc.FamilyCreate.NewDimension(view, line, references);
        if (!string.IsNullOrWhiteSpace(request.LabelParameterName))
        {
            dimension.FamilyLabel = ResolveLabelableLengthParameter(doc, request.LabelParameterName);
        }

        doc.Regenerate();
        tx.Commit();

        return new ElementCreationResult { ElementId = ToInt(dimension.Id), Success = true };
    }

    private static OperationResult SetDimensionLabel(UIApplication app, SetDimensionLabelRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (string.IsNullOrWhiteSpace(request.ParameterName))
        {
            throw new InvalidOperationException("parameterName is required.");
        }

        if (doc.GetElement(new ElementId(request.DimensionId)) is not Dimension dimension)
        {
            throw new InvalidOperationException($"Element {request.DimensionId} is not a dimension.");
        }

        var familyParameter = ResolveLabelableLengthParameter(doc, request.ParameterName);

        using var tx = new Transaction(doc, "Bridge set dimension label");
        tx.Start();
        dimension.FamilyLabel = familyParameter;
        doc.Regenerate();
        tx.Commit();

        return new OperationResult { Success = true };
    }

    private static OperationResult LockConstraint(UIApplication app, LockConstraintRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (doc.GetElement(new ElementId(request.ConstraintId)) is not Dimension dimension)
        {
            throw new InvalidOperationException(
                $"Element {request.ConstraintId} is not a dimension/constraint that can be locked.");
        }

        // v9 failure (dimension 2895): a *labeled* dimension is driven by a family parameter,
        // and Revit treats that label as the controlling constraint — Dimension.IsLocked throws
        // when set on a labeled dimension. The IsLocked-in-a-transaction shape was already correct;
        // the real cause is this labeled-dimension restriction. Detect it and handle gracefully.
        bool isLabeled;
        try
        {
            isLabeled = dimension.FamilyLabel is not null;
        }
        catch
        {
            // A few dimension kinds throw on the FamilyLabel getter; treat those as unlabeled.
            isLabeled = false;
        }

        if (isLabeled)
        {
            if (request.Locked)
            {
                // Idempotent success: a parameter label is a stronger constraint than a geometric
                // lock, so the caller's intent ("keep this dimension fixed") is already satisfied.
                return new OperationResult
                {
                    Success = true,
                    Note = "Dimension is labeled by a family parameter, which already constrains it; "
                        + "an explicit lock is not applicable (Revit disallows locking a labeled dimension).",
                };
            }

            throw new InvalidOperationException(
                $"Dimension {request.ConstraintId} is labeled by a family parameter and cannot have its "
                + "lock state toggled. Remove the dimension label first if you need to unlock it.");
        }

        using var tx = new Transaction(doc, "Bridge lock constraint");
        tx.Start();
        dimension.IsLocked = request.Locked;
        doc.Regenerate();
        tx.Commit();

        return new OperationResult { Success = dimension.IsLocked == request.Locked };
    }

    // ─── Parameters & Types helpers ────────────────────────────────────────
    private static ForgeTypeId ResolveFamilyParameterSpec(string? parameterType)
    {
        return (parameterType ?? "Text").Trim().ToLowerInvariant() switch
        {
            "length" => SpecTypeId.Length,
            "angle" => SpecTypeId.Angle,
            "number" => SpecTypeId.Number,
            "integer" => SpecTypeId.Int.Integer,
            "text" => SpecTypeId.String.Text,
            "multilinetext" => SpecTypeId.String.MultilineText,
            "boolean" or "yesno" => SpecTypeId.Boolean.YesNo,
            _ => throw new InvalidOperationException(
                $"Unsupported parameterType '{parameterType}'. Supported: Length, Angle, Number, Integer, Text, MultilineText, Boolean, YesNo, Material."),
        };
    }

    private static FamilyType? FindFamilyType(FamilyManager fm, string name)
    {
        // Type names like 16" X 16" carry literal double-quote (inch) characters. Depending
        // on JSON parsing the requested name may arrive escaped (16\" X 16\") or literal
        // (16" X 16"), so a raw string compare misses. Normalize both sides — stripping
        // backslash escapes and quote characters — before matching.
        var target = NormalizeFamilyTypeName(name);
        return fm.Types.Cast<FamilyType>()
            .FirstOrDefault(t => string.Equals(
                NormalizeFamilyTypeName(t.Name), target, StringComparison.OrdinalIgnoreCase));
    }

    // Match key for family type names: removes backslash escapes and double-quote
    // characters so escaped, literal, and quote-free spellings of the same name match.
    private static string NormalizeFamilyTypeName(string? name) =>
        (name ?? string.Empty).Replace("\\", string.Empty).Replace("\"", string.Empty).Trim();

    private static void EnsureCurrentType(FamilyManager fm)
    {
        if (fm.CurrentType is not null)
        {
            return;
        }

        var first = fm.Types.IsEmpty ? null : fm.Types.Cast<FamilyType>().FirstOrDefault();
        fm.CurrentType = first ?? fm.NewType("Default");
    }

    // Converts a JSON value to the parameter's storage type and sets it on the current type.
    // Length values arrive in mm, angles in degrees — converted to Revit internal units here.
    private static void ApplyFamilyParameterValue(FamilyManager fm, FamilyParameter fp, JsonElement value)
    {
        var name = fp.Definition.Name;
        switch (fp.StorageType)
        {
            case StorageType.Double:
            {
                double raw;
                if (value.ValueKind == JsonValueKind.Number)
                {
                    raw = value.GetDouble();
                }
                else if (value.ValueKind == JsonValueKind.String
                         && double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    raw = parsed;
                }
                else
                {
                    throw new InvalidOperationException($"Parameter '{name}' expects a numeric value.");
                }

                var dataType = fp.Definition.GetDataType();
                var converted = dataType == SpecTypeId.Length ? MmToFeet(raw)
                    : dataType == SpecTypeId.Angle ? raw * Math.PI / 180.0
                    : raw;
                fm.Set(fp, converted);
                break;
            }

            case StorageType.Integer:
            {
                int iv = value.ValueKind switch
                {
                    JsonValueKind.True => 1,
                    JsonValueKind.False => 0,
                    JsonValueKind.Number => (int)Math.Round(value.GetDouble()),
                    JsonValueKind.String when int.TryParse(value.GetString(), out var pi) => pi,
                    _ => throw new InvalidOperationException($"Parameter '{name}' expects an integer or boolean value."),
                };
                fm.Set(fp, iv);
                break;
            }

            case StorageType.String:
                fm.Set(fp, value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString());
                break;

            case StorageType.ElementId:
                if (value.ValueKind == JsonValueKind.Number)
                {
                    fm.Set(fp, new ElementId(value.GetInt64()));
                }
                else
                {
                    throw new InvalidOperationException($"Parameter '{name}' expects an element id (number).");
                }

                break;

            default:
                throw new InvalidOperationException($"Parameter '{name}' has an unsupported storage type ({fp.StorageType}).");
        }
    }

    private static AddFamilyParameterResult AddFamilyParameter(UIApplication app, AddFamilyParameterRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("name is required.");
        }

        var fm = doc.FamilyManager;
        var groupTypeId = ResolveGroupTypeId((request.ParameterGroup ?? "PG_OTHER").Trim().ToUpperInvariant());
        var isMaterial = string.Equals(request.ParameterType?.Trim(), "Material", StringComparison.OrdinalIgnoreCase);

        using var tx = new Transaction(doc, "Bridge add family parameter");
        tx.Start();
        FamilyParameter fp;
        if (isMaterial)
        {
            var materials = Category.GetCategory(doc, BuiltInCategory.OST_Materials)
                ?? throw new InvalidOperationException("Materials category is unavailable in this document.");
            fp = fm.AddParameter(request.Name.Trim(), groupTypeId, materials, request.IsInstance);
        }
        else
        {
            fp = fm.AddParameter(request.Name.Trim(), groupTypeId, ResolveFamilyParameterSpec(request.ParameterType), request.IsInstance);
        }

        doc.Regenerate();
        tx.Commit();

        return new AddFamilyParameterResult
        {
            ParameterId = fp.Id.Value.ToString(CultureInfo.InvariantCulture),
            Name = fp.Definition.Name,
            Success = true,
        };
    }

    private static OperationResult SetFamilyParameterValue(UIApplication app, SetFamilyParameterValueRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var fm = doc.FamilyManager;
        if (string.IsNullOrWhiteSpace(request.ParameterName))
        {
            throw new InvalidOperationException("parameterName is required.");
        }

        if (request.Value is null)
        {
            throw new InvalidOperationException("value is required.");
        }

        var fp = fm.get_Parameter(request.ParameterName.Trim())
            ?? throw new InvalidOperationException($"Family parameter '{request.ParameterName}' was not found.");
        if (fp.IsDeterminedByFormula)
        {
            throw new InvalidOperationException(
                $"Parameter '{request.ParameterName}' is driven by a formula and cannot be set directly.");
        }

        using var tx = new Transaction(doc, "Bridge set family parameter value");
        tx.Start();
        if (!string.IsNullOrWhiteSpace(request.TypeName))
        {
            fm.CurrentType = FindFamilyType(fm, request.TypeName.Trim())
                ?? throw new InvalidOperationException($"Family type '{request.TypeName}' was not found.");
        }
        else
        {
            EnsureCurrentType(fm);
        }

        ApplyFamilyParameterValue(fm, fp, request.Value.Value);
        doc.Regenerate();
        tx.Commit();

        return new OperationResult { Success = true };
    }

    private static OperationResult AddFamilyType(UIApplication app, AddFamilyTypeRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var fm = doc.FamilyManager;
        if (string.IsNullOrWhiteSpace(request.TypeName))
        {
            throw new InvalidOperationException("typeName is required.");
        }

        using var tx = new Transaction(doc, "Bridge add family type");
        tx.Start();
        if (!string.IsNullOrWhiteSpace(request.CloneFromType))
        {
            // NewType copies the values of the current type, so switch to the clone source first.
            fm.CurrentType = FindFamilyType(fm, request.CloneFromType.Trim())
                ?? throw new InvalidOperationException($"cloneFromType '{request.CloneFromType}' was not found.");
        }

        fm.NewType(request.TypeName.Trim());
        if (request.Parameters is not null)
        {
            foreach (var kvp in request.Parameters)
            {
                var fp = fm.get_Parameter(kvp.Key)
                    ?? throw new InvalidOperationException($"Override parameter '{kvp.Key}' was not found.");
                if (fp.IsDeterminedByFormula)
                {
                    continue;
                }

                ApplyFamilyParameterValue(fm, fp, kvp.Value);
            }
        }

        doc.Regenerate();
        tx.Commit();

        return new OperationResult { Success = true };
    }

    private static OperationResult RenameFamilyType(UIApplication app, RenameFamilyTypeRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var fm = doc.FamilyManager;
        if (string.IsNullOrWhiteSpace(request.NewName))
        {
            throw new InvalidOperationException("newName is required.");
        }

        var type = FindFamilyType(fm, request.OldName.Trim())
            ?? throw new InvalidOperationException($"Family type '{request.OldName}' was not found.");

        using var tx = new Transaction(doc, "Bridge rename family type");
        tx.Start();
        fm.CurrentType = type;
        fm.RenameCurrentType(request.NewName.Trim());
        tx.Commit();

        return new OperationResult { Success = true };
    }

    private static OperationResult DeleteFamilyType(UIApplication app, DeleteFamilyTypeRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var fm = doc.FamilyManager;
        if (fm.Types.Size <= 1)
        {
            throw new InvalidOperationException("Cannot delete the only family type.");
        }

        var type = FindFamilyType(fm, request.TypeName.Trim())
            ?? throw new InvalidOperationException($"Family type '{request.TypeName}' was not found.");

        using var tx = new Transaction(doc, "Bridge delete family type");
        tx.Start();
        fm.CurrentType = type;
        fm.DeleteCurrentType();
        tx.Commit();

        return new OperationResult { Success = true };
    }

    private static OperationResult SetFormula(UIApplication app, SetFormulaRequest request)
    {
        var doc = RequireFamilyDocument(app);
        var fm = doc.FamilyManager;
        if (string.IsNullOrWhiteSpace(request.ParameterName))
        {
            throw new InvalidOperationException("parameterName is required.");
        }

        var fp = fm.get_Parameter(request.ParameterName.Trim())
            ?? throw new InvalidOperationException($"Family parameter '{request.ParameterName}' was not found.");

        using var tx = new Transaction(doc, "Bridge set formula");
        tx.Start();
        fm.SetFormula(fp, request.Formula);
        doc.Regenerate();
        tx.Commit();

        return new OperationResult { Success = true };
    }

    private static SaveFamilyResult SaveFamily(UIApplication app)
    {
        var doc = RequireFamilyDocument(app);
        if (string.IsNullOrWhiteSpace(doc.PathName))
        {
            throw new InvalidOperationException("Family has no save path. Use save_family_as first.");
        }

        doc.Save();
        return new SaveFamilyResult { FilePath = doc.PathName, Success = true };
    }

    private static SaveFamilyResult SaveFamilyAs(UIApplication app, SaveFamilyAsRequest request)
    {
        var doc = RequireFamilyDocument(app);
        if (string.IsNullOrWhiteSpace(request.FilePath))
        {
            throw new InvalidOperationException("filePath is required.");
        }

        var path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(request.FilePath.Trim()));
        if (!path.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("filePath must end with .rfa.");
        }

        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        doc.SaveAs(path, new SaveAsOptions { OverwriteExistingFile = true });
        return new SaveFamilyResult { FilePath = path, Success = true };
    }

    private static LoadFamilyIntoProjectResult LoadFamilyIntoProject(UIApplication app, LoadFamilyIntoProjectRequest request)
    {
        var familyDoc = RequireFamilyDocument(app);
        var target = app.Application.Documents
            .Cast<Document>()
            .FirstOrDefault(d => !d.IsFamilyDocument)
            ?? throw new InvalidOperationException("No project document is open to load the family into. Open a .rvt first.");

        // LoadFamily(Document, IFamilyLoadOptions) must NOT run inside a transaction.
        var family = familyDoc.LoadFamily(target, new BridgeFamilyLoadOptions(request.OverwriteParameterValues))
            ?? throw new InvalidOperationException(
                "LoadFamily returned no family (it may already be loaded unchanged, or was unsaved).");

        return new LoadFamilyIntoProjectResult { FamilyId = ToInt(family.Id), Success = true };
    }

    private static FamilyDocumentInfoResult GetFamilyDocumentInfo(UIApplication app)
    {
        var doc = RequireFamilyDocument(app);
        var family = doc.OwnerFamily;
        var placement = family.FamilyPlacementType;
        return new FamilyDocumentInfoResult
        {
            FamilyName = family.Name,
            Category = family.FamilyCategory?.Name ?? string.Empty,
            IsConceptual = family.IsConceptualMassFamily,
            // No clean "is face based" API: WorkPlaneBased is the closest placement signal.
            IsFaceBased = placement == FamilyPlacementType.WorkPlaneBased,
            HostType = placement.ToString(),
            TypeCount = doc.FamilyManager.Types.Size,
            FilePath = doc.PathName ?? string.Empty,
        };
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

    private static string ToCategoryToken(Category category)
    {
        var idValue = category.Id.Value;
        return Enum.IsDefined(typeof(BuiltInCategory), idValue)
            ? ((BuiltInCategory)idValue).ToString()
            : category.Name;
    }

    private static int ToInt(ElementId id)
    {
        return checked((int)id.Value);
    }

    private static OpenSelectedFamilyEditorResult OpenSelectedFamilyEditor(
        UIApplication app,
        OpenSelectedFamilyEditorRequest request)
    {
        _ = request;
        var uiDoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uiDoc.Document;
        var selectedIds = uiDoc.Selection.GetElementIds();
        if (selectedIds.Count == 0)
        {
            throw new InvalidOperationException("No selected element found. Select one family instance first.");
        }

        var selectedId = selectedIds.First();
        var selectedElement = doc.GetElement(selectedId)
            ?? throw new InvalidOperationException($"Selected element {selectedId.Value} not found.");

        var family = ResolveFamilyFromElement(doc, selectedElement)
            ?? throw new InvalidOperationException(
                $"Selected element {selectedElement.Id.Value} is not a family instance/type.");

        var familyDoc = doc.EditFamily(family);
        var activatedByPath = false;

        if (!string.IsNullOrWhiteSpace(familyDoc.PathName))
        {
            try
            {
                app.OpenAndActivateDocument(familyDoc.PathName);
                activatedByPath = true;
            }
            catch
            {
                // Family is open; keep success response even when explicit activation is unavailable.
            }
        }

        return new OpenSelectedFamilyEditorResult
        {
            SelectedElementId = ToInt(selectedElement.Id),
            SelectedElementName = selectedElement.Name,
            SelectedElementCategory = selectedElement.Category?.Name ?? string.Empty,
            FamilyName = family.Name,
            FamilyDocumentTitle = familyDoc.Title,
            ActivatedByPath = activatedByPath,
        };
    }

    private static OpenSelectedFamilyEditorResult OpenFamilyEditorByElementId(
        UIApplication app,
        OpenFamilyEditorByElementIdRequest request)
    {
        var uiDoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uiDoc.Document;
        var selectedElement = doc.GetElement(new ElementId(request.ElementId))
            ?? throw new InvalidOperationException($"Element {request.ElementId} not found.");

        var family = ResolveFamilyFromElement(doc, selectedElement)
            ?? throw new InvalidOperationException(
                $"Element {request.ElementId} is not a family instance/type.");

        var familyDoc = doc.EditFamily(family);
        var activatedByPath = false;

        if (!string.IsNullOrWhiteSpace(familyDoc.PathName))
        {
            try
            {
                app.OpenAndActivateDocument(familyDoc.PathName);
                activatedByPath = true;
            }
            catch
            {
                // Keep success response; document was still opened by EditFamily.
            }
        }

        return new OpenSelectedFamilyEditorResult
        {
            SelectedElementId = ToInt(selectedElement.Id),
            SelectedElementName = selectedElement.Name,
            SelectedElementCategory = selectedElement.Category?.Name ?? string.Empty,
            FamilyName = family.Name,
            FamilyDocumentTitle = familyDoc.Title,
            ActivatedByPath = activatedByPath,
        };
    }

    private static Family? ResolveFamilyFromElement(Document doc, Element element)
    {
        if (element is FamilyInstance fi)
        {
            return fi.Symbol?.Family;
        }

        if (element is FamilySymbol fs)
        {
            return fs.Family;
        }

        if (element.GetTypeId() != ElementId.InvalidElementId
            && doc.GetElement(element.GetTypeId()) is FamilySymbol typedSymbol)
        {
            return typedSymbol.Family;
        }

        return null;
    }

    private static EnsureSharedParametersResult EnsureSharedParameters(
        UIApplication app,
        EnsureSharedParametersRequest request)
    {
        if (request.Parameters.Count == 0)
        {
            throw new InvalidOperationException("parameters is required and must contain at least one item.");
        }

        var shared = EnsureSharedParameterContext(app, request.SharedParameterFilePath, request.GroupName);
        var created = new List<SharedParameterDefinitionRecord>();

        foreach (var item in request.Parameters)
        {
            var name = item.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Each shared parameter requires a non-empty name.");
            }

            var existingDefinition = shared.Group.Definitions.get_Item(name) as ExternalDefinition;
            var wasCreated = false;
            ExternalDefinition definition;

            if (existingDefinition is not null)
            {
                definition = existingDefinition;
            }
            else
            {
                var options = new ExternalDefinitionCreationOptions(name, ResolveSpecTypeId(item.DataType))
                {
                    Description = item.Description ?? string.Empty,
                    Visible = item.Visible ?? true,
                    UserModifiable = item.UserModifiable ?? true,
                };
                definition = (ExternalDefinition)shared.Group.Definitions.Create(options);
                wasCreated = true;
            }

            created.Add(new SharedParameterDefinitionRecord
            {
                Name = definition.Name,
                DataType = definition.GetDataType().TypeId,
                Guid = definition.GUID.ToString(),
                Created = wasCreated,
            });
        }

        return new EnsureSharedParametersResult
        {
            SharedParameterFilePath = shared.FilePath,
            GroupName = shared.Group.Name,
            Parameters = created,
        };
    }

    private static BindSharedParametersResult BindSharedParameters(
        UIApplication app,
        BindSharedParametersRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");
        if (doc.IsFamilyDocument)
        {
            throw new InvalidOperationException(
                "Active document is a family document. Open a project document to bind shared parameters to categories.");
        }

        if (request.ParameterNames.Count == 0)
        {
            throw new InvalidOperationException("parameterNames is required and must contain at least one name.");
        }

        if (request.CategoryList.Count == 0)
        {
            throw new InvalidOperationException("categoryList is required and must contain at least one category token.");
        }

        var shared = EnsureSharedParameterContext(app, request.SharedParameterFilePath, request.GroupName);
        var categorySet = app.Application.Create.NewCategorySet();
        var categories = new List<string>();
        foreach (var token in request.CategoryList
                     .Where(s => !string.IsNullOrWhiteSpace(s))
                     .Select(s => s.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var category = ResolveCategory(doc, token);
            categorySet.Insert(category);
            categories.Add(ToCategoryToken(category));
        }

        if (categories.Count == 0)
        {
            throw new InvalidOperationException("No valid categories were resolved from categoryList.");
        }

        var bindingType = (request.BindingType ?? "instance").Trim().ToLowerInvariant();
        var parameterGroupText = (request.ParameterGroup ?? "PG_DATA").Trim().ToUpperInvariant();
        var groupTypeId = ResolveGroupTypeId(parameterGroupText);
        var bindingResults = new List<BindSharedParameterRecord>();

        using var tx = new Transaction(doc, "Bridge bind shared parameters");
        tx.Start();

        foreach (var parameterName in request.ParameterNames
                     .Where(s => !string.IsNullOrWhiteSpace(s))
                     .Select(s => s.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var definition = shared.Group.Definitions.get_Item(parameterName) as ExternalDefinition
                ?? throw new InvalidOperationException(
                    $"Shared parameter '{parameterName}' was not found in group '{shared.Group.Name}'.");

            Binding binding = bindingType switch
            {
                "type" => app.Application.Create.NewTypeBinding(categorySet),
                _ => app.Application.Create.NewInstanceBinding(categorySet),
            };

            var inserted = doc.ParameterBindings.Insert(definition, binding, groupTypeId);
            var reInserted = false;
            if (!inserted)
            {
                reInserted = doc.ParameterBindings.ReInsert(definition, binding, groupTypeId);
            }

            bindingResults.Add(new BindSharedParameterRecord
            {
                Name = parameterName,
                Inserted = inserted,
                ReInserted = reInserted,
            });
        }

        tx.Commit();

        return new BindSharedParametersResult
        {
            SharedParameterFilePath = shared.FilePath,
            GroupName = shared.Group.Name,
            BindingType = bindingType,
            ParameterGroup = parameterGroupText,
            Categories = categories,
            Parameters = bindingResults,
        };
    }

    private static SharedParameterContext EnsureSharedParameterContext(
        UIApplication app,
        string? sharedParameterFilePath,
        string? groupName)
    {
        var targetPath = ResolveSharedParameterFilePath(app, sharedParameterFilePath);
        if (!File.Exists(targetPath))
        {
            var folder = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(targetPath, DefaultSharedParameterFileContents());
        }

        app.Application.SharedParametersFilename = targetPath;
        var definitionFile = app.Application.OpenSharedParameterFile()
            ?? throw new InvalidOperationException(
                $"Unable to open shared parameter file at '{targetPath}'.");

        var normalizedGroupName = string.IsNullOrWhiteSpace(groupName) ? "CursorMCP" : groupName.Trim();
        var group = definitionFile.Groups.get_Item(normalizedGroupName)
            ?? definitionFile.Groups.Create(normalizedGroupName);

        return new SharedParameterContext(targetPath, group);
    }

    private static string ResolveSharedParameterFilePath(UIApplication app, string? requestedPath)
    {
        var raw = string.IsNullOrWhiteSpace(requestedPath)
            ? app.Application.SharedParametersFilename
            : requestedPath;

        if (string.IsNullOrWhiteSpace(raw))
        {
            raw = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RevitPublicMCPBridge",
                "shared-parameters.txt");
        }

        var expanded = Environment.ExpandEnvironmentVariables(raw.Trim());
        return Path.GetFullPath(expanded);
    }

    private static string DefaultSharedParameterFileContents()
    {
        return string.Join(
            "\n",
            [
                "# This is a Revit shared parameter file.",
                "*META\tVERSION\tMINVERSION",
                "META\t2\t1",
                "*GROUP\tID\tNAME",
                "*PARAM\tGUID\tNAME\tDATATYPE\tDATACATEGORY\tGROUP\tVISIBLE\tDESCRIPTION\tUSERMODIFIABLE\tHIDEWHENNOVALUE",
                string.Empty,
            ]);
    }

    private static ForgeTypeId ResolveSpecTypeId(string? dataType)
    {
        var normalized = (dataType ?? "text").Trim().ToLowerInvariant();
        return normalized switch
        {
            "text" => SpecTypeId.String.Text,
            "integer" => SpecTypeId.Int.Integer,
            "number" => SpecTypeId.Number,
            "length" => SpecTypeId.Length,
            "area" => SpecTypeId.Area,
            "volume" => SpecTypeId.Volume,
            "yesno" => SpecTypeId.Boolean.YesNo,
            _ => throw new InvalidOperationException(
                $"Unsupported shared parameter dataType '{dataType}'. Supported values: text, integer, number, length, area, volume, yesno."),
        };
    }

    private static ForgeTypeId ResolveGroupTypeId(string parameterGroup)
    {
        return parameterGroup.ToUpperInvariant() switch
        {
            "PG_DATA" => GroupTypeId.Data,
            "PG_TEXT" => GroupTypeId.Text,
            "PG_GEOMETRY" => GroupTypeId.Geometry,
            "PG_IDENTITY_DATA" => GroupTypeId.IdentityData,
            "PG_CONSTRAINTS" => GroupTypeId.Constraints,
            "PG_MATERIALS" => GroupTypeId.Materials,
            "PG_CONSTRUCTION" => GroupTypeId.Construction,
            "PG_GRAPHICS" => GroupTypeId.Graphics,
            "PG_PHASING" => GroupTypeId.Phasing,
            "PG_STRUCTURAL" => GroupTypeId.Structural,
            "PG_MECHANICAL" => GroupTypeId.Mechanical,
            "PG_ELECTRICAL" => GroupTypeId.Electrical,
            "PG_PLUMBING" => GroupTypeId.Plumbing,
            "PG_IFC" => GroupTypeId.Ifc,
            "PG_OTHER" or "PG_GENERAL" or "OTHER" => GroupTypeId.General,
            _ => throw new InvalidOperationException(
                $"Unknown parameterGroup '{parameterGroup}'. Supported values include PG_GEOMETRY, PG_CONSTRAINTS, PG_IDENTITY_DATA, PG_DATA, PG_MATERIALS, PG_OTHER."),
        };
    }

    private static Category ResolveCategory(Document doc, string token)
    {
        if (token.StartsWith("OST_", StringComparison.OrdinalIgnoreCase)
            && Enum.TryParse<BuiltInCategory>(token, ignoreCase: true, out var bic))
        {
            var builtIn = Category.GetCategory(doc, bic);
            if (builtIn is not null)
            {
                return builtIn;
            }
        }

        foreach (Category category in doc.Settings.Categories)
        {
            if (string.Equals(category.Name, token, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ToCategoryToken(category), token, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        throw new InvalidOperationException($"Category '{token}' was not found in this document.");
    }

    private SearchFamilyLibraryResult SearchFamilyLibrary(
        UIApplication app,
        SearchFamilyLibraryRequest request)
    {
        _ = app;
        var query = (request.Query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new InvalidOperationException("query is required.");
        }

        var roots = ResolveLibraryRoots(request.LibraryRoots);
        var maxResults = request.MaxResults is > 0 ? Math.Min(request.MaxResults.Value, 200) : 25;
        var normalizedQuery = query.ToLowerInvariant();

        var matches = FindFamilyFiles(roots)
            .Select(path =>
            {
                var familyName = Path.GetFileNameWithoutExtension(path);
                var familyLower = familyName.ToLowerInvariant();
                var exact = string.Equals(familyName, query, StringComparison.OrdinalIgnoreCase);
                var contains = familyLower.Contains(normalizedQuery, StringComparison.Ordinal);
                var starts = familyLower.StartsWith(normalizedQuery, StringComparison.Ordinal);
                var score = exact ? 0 : starts ? 1 : contains ? 2 : 10;
                return new
                {
                    Path = path,
                    FamilyName = familyName,
                    Exact = exact,
                    Contains = contains,
                    Score = score,
                };
            })
            .Where(x => x.Exact || x.Contains)
            .OrderBy(x => x.Score)
            .ThenBy(x => x.FamilyName, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(x => new FamilyLibraryMatch
            {
                FamilyName = x.FamilyName,
                FilePath = x.Path,
                LibraryRoot = GetOwningRoot(roots, x.Path),
                ExactNameMatch = x.Exact,
            })
            .ToList();

        return new SearchFamilyLibraryResult
        {
            Query = query,
            LibraryRoots = roots,
            Matches = matches,
        };
    }

    private OpenFamilyFromLibraryResult OpenFamilyFromLibrary(
        UIApplication app,
        OpenFamilyFromLibraryRequest request)
    {
        var familyPath = ResolveFamilyPathFromRequest(request.FamilyPath, request.LibraryRoots, request.Query);
        var openedDocument = app.Application.OpenDocumentFile(familyPath);
        var activated = false;
        try
        {
            app.OpenAndActivateDocument(familyPath);
            activated = true;
        }
        catch
        {
            // Keep success response if activation is unavailable; document has already opened.
        }

        return new OpenFamilyFromLibraryResult
        {
            FamilyName = Path.GetFileNameWithoutExtension(familyPath),
            FamilyPath = familyPath,
            DocumentTitle = openedDocument.Title,
            Activated = activated,
        };
    }

    private LoadFamilyFromLibraryResult LoadFamilyFromLibrary(
        UIApplication app,
        LoadFamilyFromLibraryRequest request)
    {
        var uiDoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active Revit document.");
        var doc = uiDoc.Document;
        if (doc.IsFamilyDocument)
        {
            throw new InvalidOperationException("Active document is a family document. Open a project document first.");
        }

        var familyPath = ResolveFamilyPathFromRequest(request.FamilyPath, request.LibraryRoots, request.Query);
        var familyName = Path.GetFileNameWithoutExtension(familyPath);
        var existingFamily = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .FirstOrDefault(f => string.Equals(f.Name, familyName, StringComparison.OrdinalIgnoreCase));

        var loaded = false;
        Family? loadedFamily;
        using var tx = new Transaction(doc, "Bridge load family from library");
        tx.Start();
        loaded = doc.LoadFamily(
            familyPath,
            new BridgeFamilyLoadOptions(request.OverwriteParameterValues),
            out loadedFamily);
        tx.Commit();

        var finalFamily = loadedFamily ?? existingFamily;
        if (finalFamily is null)
        {
            throw new InvalidOperationException(
                $"Family '{familyName}' could not be resolved after loading from '{familyPath}'.");
        }

        return new LoadFamilyFromLibraryResult
        {
            FamilyName = finalFamily.Name,
            FamilyPath = familyPath,
            FamilyId = ToInt(finalFamily.Id),
            Loaded = loaded,
            FoundExisting = existingFamily is not null,
        };
    }

    private string ResolveFamilyPathFromRequest(
        string? explicitFamilyPath,
        List<string>? libraryRoots,
        string? query)
    {
        if (!string.IsNullOrWhiteSpace(explicitFamilyPath))
        {
            var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(explicitFamilyPath.Trim()));
            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException($"Family file not found: {fullPath}");
            }

            if (!fullPath.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("familyPath must point to a .rfa file.");
            }

            return fullPath;
        }

        var normalizedQuery = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            throw new InvalidOperationException("Provide either familyPath or query.");
        }

        var roots = ResolveLibraryRoots(libraryRoots);
        var matches = FindFamilyFiles(roots)
            .Select(path => new
            {
                Path = path,
                FamilyName = Path.GetFileNameWithoutExtension(path),
            })
            .Where(x => x.FamilyName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => string.Equals(x.FamilyName, normalizedQuery, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(x => x.FamilyName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"No family matching '{normalizedQuery}' was found under: {string.Join("; ", roots)}");
        }

        return matches[0].Path;
    }

    private List<string> ResolveLibraryRoots(List<string>? requestedRoots)
    {
        var roots = (requestedRoots ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => Path.GetFullPath(Environment.ExpandEnvironmentVariables(s.Trim())))
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roots.Count > 0)
        {
            return roots;
        }

        var defaults = new List<string>();
        var configured = _settingsStore.Current.DefaultFamilyLibraryPath?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            defaults.Add(configured);
        }
        defaults.AddRange(
        [
            @"C:\ProgramData\Autodesk\RVT 2026\Libraries",
            @"C:\ProgramData\Autodesk\RVT 2027\Libraries",
            @"C:\ProgramData\Autodesk\Revit\Libraries",
        ]);

        roots = defaults
            .Select(s => Path.GetFullPath(Environment.ExpandEnvironmentVariables(s)))
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roots.Count == 0)
        {
            throw new InvalidOperationException(
                "No valid library roots found. Pass libraryRoots with one or more existing folders.");
        }

        return roots;
    }

    private static IEnumerable<string> FindFamilyFiles(List<string> roots)
    {
        foreach (var root in roots)
        {
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(root, "*.rfa", SearchOption.AllDirectories);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }
        }
    }

    private static string GetOwningRoot(List<string> roots, string filePath)
    {
        foreach (var root in roots)
        {
            if (filePath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }
        }

        return string.Empty;
    }

    private UpgradeFamilyLibraryVersionResult UpgradeFamilyLibraryVersion(
        UIApplication app,
        UpgradeFamilyLibraryVersionRequest request)
    {
        var revitYear = ParseRevitYear(app.Application.VersionNumber);
        var targetYear = request.TargetYear ?? revitYear;
        if (targetYear != revitYear)
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} does not match running Revit year {revitYear}. Run this tool in Revit {targetYear}.");
        }

        if (request.SourceYear.HasValue && request.SourceYear.Value > revitYear)
        {
            throw new InvalidOperationException(
                $"Source year {request.SourceYear.Value} is newer than running Revit {revitYear}. Revit cannot downgrade families.");
        }

        var sourceRoot = ResolveSourceRoot(request.SourceRoot);
        var targetRoot = ResolveTargetRoot(request.TargetRoot, revitYear);
        var maxFiles = request.MaxFiles is > 0 ? Math.Min(request.MaxFiles.Value, 100) : 25;
        var filter = (request.Query ?? string.Empty).Trim();

        var candidates = Directory
            .EnumerateFiles(sourceRoot, "*.rfa", SearchOption.AllDirectories)
            .Where(path => string.IsNullOrWhiteSpace(filter)
                || Path.GetFileNameWithoutExtension(path).Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(maxFiles)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No family files found in '{sourceRoot}'{(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" matching '{filter}'")}.");
        }

        var processed = new List<UpgradeFamilyRecord>();
        foreach (var sourcePath in candidates)
        {
            var relativePath = NetCompat.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = Path.Combine(targetRoot, relativePath);
            var targetFolder = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (File.Exists(targetPath) && !request.OverwriteExisting)
            {
                processed.Add(new UpgradeFamilyRecord
                {
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    Upgraded = false,
                    Note = "Skipped existing target file (overwriteExisting=false).",
                });
                continue;
            }

            Document? familyDoc = null;
            try
            {
                familyDoc = app.Application.OpenDocumentFile(sourcePath);
                var saveAsOptions = new SaveAsOptions
                {
                    OverwriteExistingFile = request.OverwriteExisting,
                    Compact = true,
                };
                familyDoc.SaveAs(targetPath, saveAsOptions);
                processed.Add(new UpgradeFamilyRecord
                {
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    Upgraded = true,
                    Note = $"Upgraded to Revit {revitYear} format.",
                });
            }
            catch (Exception ex)
            {
                processed.Add(new UpgradeFamilyRecord
                {
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    Upgraded = false,
                    Note = $"Failed: {ex.GetBaseException().Message}",
                });
            }
            finally
            {
                familyDoc?.Close(false);
            }
        }

        return new UpgradeFamilyLibraryVersionResult
        {
            RunningRevitYear = revitYear,
            TargetYear = targetYear,
            SourceRoot = sourceRoot,
            TargetRoot = targetRoot,
            Processed = processed,
        };
    }

    private ExtractFamilyLibraryParametersResult ExtractFamilyLibraryParameters(
        UIApplication app,
        ExtractFamilyLibraryParametersRequest request)
    {
        var revitYear = ParseRevitYear(app.Application.VersionNumber);
        var sourceRoot = ResolveSourceRoot(request.SourceRoot);
        var maxFiles = request.MaxFiles is > 0 ? Math.Min(request.MaxFiles.Value, 500) : 100;
        var filter = (request.Query ?? string.Empty).Trim();
        var parameterNames = request.ParameterNames
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (parameterNames.Count == 0)
        {
            throw new InvalidOperationException("parameterNames is required and must contain at least one name.");
        }

        var candidates = Directory
            .EnumerateFiles(sourceRoot, "*.rfa", SearchOption.AllDirectories)
            .Where(path => string.IsNullOrWhiteSpace(filter)
                || Path.GetFileNameWithoutExtension(path).Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(maxFiles)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No family files found in '{sourceRoot}'{(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" matching '{filter}'")}.");
        }

        var processed = new List<ExtractFamilyLibraryParameterRecord>();
        foreach (var sourcePath in candidates)
        {
            Document? familyDoc = null;
            var familyName = Path.GetFileNameWithoutExtension(sourcePath);
            try
            {
                familyDoc = app.Application.OpenDocumentFile(sourcePath);
                if (!familyDoc.IsFamilyDocument)
                {
                    processed.Add(new ExtractFamilyLibraryParameterRecord
                    {
                        FamilyName = familyName,
                        FamilyPath = sourcePath,
                        Extracted = false,
                        Note = "Skipped because opened document is not a family document.",
                    });
                    continue;
                }

                var familyManager = familyDoc.FamilyManager;
                var types = familyManager.Types
                    .Cast<FamilyType>()
                    .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (types.Count == 0)
                {
                    processed.Add(new ExtractFamilyLibraryParameterRecord
                    {
                        FamilyName = familyName,
                        FamilyPath = sourcePath,
                        Extracted = false,
                        Note = "No family types found.",
                    });
                    continue;
                }

                foreach (var familyType in types)
                {
                    var parameterValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var parameterName in parameterNames)
                    {
                        var familyParameter = familyManager.get_Parameter(parameterName);
                        if (familyParameter is null)
                        {
                            parameterValues[parameterName] = string.Empty;
                            continue;
                        }

                        parameterValues[parameterName] =
                            GetFamilyParameterValueAsString(familyDoc, familyType, familyParameter);
                    }

                    processed.Add(new ExtractFamilyLibraryParameterRecord
                    {
                        FamilyName = familyName,
                        FamilyPath = sourcePath,
                        TypeName = familyType.Name,
                        ParameterValues = parameterValues,
                        Extracted = true,
                    });
                }
            }
            catch (Exception ex)
            {
                processed.Add(new ExtractFamilyLibraryParameterRecord
                {
                    FamilyName = familyName,
                    FamilyPath = sourcePath,
                    Extracted = false,
                    Note = $"Failed: {ex.GetBaseException().Message}",
                });
            }
            finally
            {
                familyDoc?.Close(false);
            }
        }

        return new ExtractFamilyLibraryParametersResult
        {
            RunningRevitYear = revitYear,
            SourceRoot = sourceRoot,
            ParameterNames = parameterNames,
            Processed = processed,
        };
    }

    private AddSharedParametersToFamilyLibraryResult AddSharedParametersToFamilyLibrary(
        UIApplication app,
        AddSharedParametersToFamilyLibraryRequest request)
    {
        var revitYear = ParseRevitYear(app.Application.VersionNumber);
        var sourceRoot = ResolveSourceRoot(request.SourceRoot);
        var maxFiles = request.MaxFiles is > 0 ? Math.Min(request.MaxFiles.Value, 500) : 100;
        var filter = (request.Query ?? string.Empty).Trim();
        var parameterNames = request.ParameterNames
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (parameterNames.Count == 0)
        {
            throw new InvalidOperationException("parameterNames is required and must contain at least one name.");
        }

        var shared = EnsureSharedParameterContext(app, request.SharedParameterFilePath, request.GroupName);
        var parameterGroupText = (request.ParameterGroup ?? "PG_DATA").Trim().ToUpperInvariant();
        var groupTypeId = ResolveGroupTypeId(parameterGroupText);

        var candidates = Directory
            .EnumerateFiles(sourceRoot, "*.rfa", SearchOption.AllDirectories)
            .Where(path => string.IsNullOrWhiteSpace(filter)
                || Path.GetFileNameWithoutExtension(path).Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(maxFiles)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No family files found in '{sourceRoot}'{(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" matching '{filter}'")}.");
        }

        var processed = new List<AddSharedParametersToFamilyLibraryRecord>();
        foreach (var sourcePath in candidates)
        {
            Document? familyDoc = null;
            var familyName = Path.GetFileNameWithoutExtension(sourcePath);
            try
            {
                familyDoc = app.Application.OpenDocumentFile(sourcePath);
                if (!familyDoc.IsFamilyDocument)
                {
                    processed.Add(new AddSharedParametersToFamilyLibraryRecord
                    {
                        FamilyName = familyName,
                        FamilyPath = sourcePath,
                        Updated = false,
                        Note = "Skipped because opened document is not a family document.",
                    });
                    continue;
                }

                var manager = familyDoc.FamilyManager;
                var added = new List<string>();
                var skippedExisting = new List<string>();
                var failed = new List<string>();

                using var tx = new Transaction(familyDoc, "Bridge add shared parameters to family");
                tx.Start();

                foreach (var parameterName in parameterNames)
                {
                    if (manager.get_Parameter(parameterName) is not null)
                    {
                        skippedExisting.Add(parameterName);
                        continue;
                    }

                    var definition = shared.Group.Definitions.get_Item(parameterName) as ExternalDefinition;
                    if (definition is null)
                    {
                        failed.Add($"{parameterName}: missing in shared parameter group '{shared.Group.Name}'.");
                        continue;
                    }

                    try
                    {
                        manager.AddParameter(definition, groupTypeId, request.IsInstance);
                        added.Add(parameterName);
                    }
                    catch (Exception ex)
                    {
                        failed.Add($"{parameterName}: {ex.GetBaseException().Message}");
                    }
                }

                if (added.Count > 0)
                {
                    tx.Commit();
                    familyDoc.Save();
                }
                else
                {
                    tx.RollBack();
                }

                processed.Add(new AddSharedParametersToFamilyLibraryRecord
                {
                    FamilyName = familyName,
                    FamilyPath = sourcePath,
                    Added = added,
                    SkippedExisting = skippedExisting,
                    Failed = failed,
                    Updated = added.Count > 0,
                    Note = added.Count > 0
                        ? $"Added {added.Count} shared parameter(s)."
                        : "No shared parameters were added.",
                });
            }
            catch (Exception ex)
            {
                processed.Add(new AddSharedParametersToFamilyLibraryRecord
                {
                    FamilyName = familyName,
                    FamilyPath = sourcePath,
                    Updated = false,
                    Note = $"Failed: {ex.GetBaseException().Message}",
                });
            }
            finally
            {
                familyDoc?.Close(false);
            }
        }

        return new AddSharedParametersToFamilyLibraryResult
        {
            RunningRevitYear = revitYear,
            SourceRoot = sourceRoot,
            SharedParameterFilePath = shared.FilePath,
            GroupName = shared.Group.Name,
            ParameterGroup = parameterGroupText,
            IsInstance = request.IsInstance,
            ParameterNames = parameterNames,
            Processed = processed,
        };
    }

    private ExtractFamilyDescriptionVariantsResult ExtractFamilyDescriptionVariants(
        UIApplication app,
        ExtractFamilyDescriptionVariantsRequest request)
    {
        var revitYear = ParseRevitYear(app.Application.VersionNumber);
        var sourceRoot = ResolveSourceRoot(request.SourceRoot);
        var maxFiles = request.MaxFiles is > 0 ? Math.Min(request.MaxFiles.Value, 500) : 100;
        var filter = (request.Query ?? string.Empty).Trim();
        var maxBooleanDrivers = request.MaxBooleanDrivers <= 0 ? 8 : Math.Min(request.MaxBooleanDrivers, 12);
        var descriptionParameterName = string.IsNullOrWhiteSpace(request.DescriptionParameterName)
            ? "IDENTITY_DESCRIPTION"
            : request.DescriptionParameterName.Trim();
        var descriptionShortParameterName = string.IsNullOrWhiteSpace(request.DescriptionShortParameterName)
            ? "IDENTITY_DESCRIPTION_SHORT"
            : request.DescriptionShortParameterName.Trim();
        var manufactureComponentParameterName = string.IsNullOrWhiteSpace(request.ManufactureComponentParameterName)
            ? "MANUFACTURE_COMPONENT"
            : request.ManufactureComponentParameterName.Trim();

        var candidates = Directory
            .EnumerateFiles(sourceRoot, "*.rfa", SearchOption.AllDirectories)
            .Where(path => string.IsNullOrWhiteSpace(filter)
                || Path.GetFileNameWithoutExtension(path).Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(maxFiles)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No family files found in '{sourceRoot}'{(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" matching '{filter}'")}.");
        }

        var processed = new List<ExtractFamilyDescriptionVariantRecord>();
        foreach (var sourcePath in candidates)
        {
            Document? familyDoc = null;
            var familyName = Path.GetFileNameWithoutExtension(sourcePath);
            try
            {
                familyDoc = app.Application.OpenDocumentFile(sourcePath);
                if (!familyDoc.IsFamilyDocument)
                {
                    processed.Add(new ExtractFamilyDescriptionVariantRecord
                    {
                        FamilyName = familyName,
                        FamilyPath = sourcePath,
                        Extracted = false,
                        Note = "Skipped because opened document is not a family document.",
                    });
                    continue;
                }

                var familyManager = familyDoc.FamilyManager;
                var allParameters = familyManager.Parameters
                    .Cast<FamilyParameter>()
                    .ToList();
                var descriptionParameter = familyManager.get_Parameter(descriptionParameterName);
                var descriptionShortParameter = familyManager.get_Parameter(descriptionShortParameterName);
                var manufactureComponentParameter = familyManager.get_Parameter(manufactureComponentParameterName);

                if (descriptionParameter is null)
                {
                    // Caller requested variant expansion by this parameter; when it does not exist,
                    // treat the family as not applicable and skip it instead of reporting a failure.
                    continue;
                }

                var types = familyManager.Types
                    .Cast<FamilyType>()
                    .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (types.Count == 0)
                {
                    processed.Add(new ExtractFamilyDescriptionVariantRecord
                    {
                        FamilyName = familyName,
                        FamilyPath = sourcePath,
                        Extracted = false,
                        Note = "No family types found.",
                    });
                    continue;
                }

                foreach (var familyType in types)
                {
                    var formula = descriptionParameter.Formula ?? string.Empty;
                    var drivers = ResolveFormulaDriverParameters(formula, allParameters)
                        .Where(fp => IsYesNoParameter(fp) && !fp.IsDeterminedByFormula)
                        .Take(maxBooleanDrivers)
                        .ToList();

                    if (drivers.Count == 0)
                    {
                        processed.Add(new ExtractFamilyDescriptionVariantRecord
                        {
                            FamilyName = familyName,
                            FamilyPath = sourcePath,
                            TypeName = familyType.Name,
                            IdentityDescription = GetFamilyParameterValueAsString(familyDoc, familyType, descriptionParameter),
                            IdentityDescriptionShort = descriptionShortParameter is null
                                ? string.Empty
                                : GetFamilyParameterValueAsString(familyDoc, familyType, descriptionShortParameter),
                            ManufactureComponent = manufactureComponentParameter is null
                                ? string.Empty
                                : GetFamilyParameterValueAsString(familyDoc, familyType, manufactureComponentParameter),
                            Extracted = true,
                        });
                        continue;
                    }

                    using var tx = new Transaction(familyDoc, "Bridge evaluate family description variants");
                    tx.Start();
                    familyManager.CurrentType = familyType;
                    var original = drivers.ToDictionary(
                        fp => fp,
                        fp => (familyManager.CurrentType.AsInteger(fp) ?? 0) != 0);
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var combos = 1 << drivers.Count;
                    for (var mask = 0; mask < combos; mask++)
                    {
                        var state = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                        for (var idx = 0; idx < drivers.Count; idx++)
                        {
                            var enabled = ((mask >> idx) & 1) == 1;
                            familyManager.Set(drivers[idx], enabled ? 1 : 0);
                            state[drivers[idx].Definition.Name] = enabled;
                        }

                        familyDoc.Regenerate();
                        var desc = GetCurrentTypeParameterValueAsString(familyDoc, familyManager, descriptionParameter);
                        var descShort = descriptionShortParameter is null
                            ? string.Empty
                            : GetCurrentTypeParameterValueAsString(familyDoc, familyManager, descriptionShortParameter);
                        var component = manufactureComponentParameter is null
                            ? string.Empty
                            : GetCurrentTypeParameterValueAsString(familyDoc, familyManager, manufactureComponentParameter);
                        var signature = $"{desc}\u001F{descShort}\u001F{component}";
                        if (!seen.Add(signature))
                        {
                            continue;
                        }

                        processed.Add(new ExtractFamilyDescriptionVariantRecord
                        {
                            FamilyName = familyName,
                            FamilyPath = sourcePath,
                            TypeName = familyType.Name,
                            IdentityDescription = desc,
                            IdentityDescriptionShort = descShort,
                            ManufactureComponent = component,
                            DriverState = state,
                            Extracted = true,
                        });
                    }

                    foreach (var kvp in original)
                    {
                        familyManager.Set(kvp.Key, kvp.Value ? 1 : 0);
                    }

                    tx.RollBack();
                }
            }
            catch (Exception ex)
            {
                processed.Add(new ExtractFamilyDescriptionVariantRecord
                {
                    FamilyName = familyName,
                    FamilyPath = sourcePath,
                    Extracted = false,
                    Note = $"Failed: {ex.GetBaseException().Message}",
                });
            }
            finally
            {
                familyDoc?.Close(false);
            }
        }

        return new ExtractFamilyDescriptionVariantsResult
        {
            RunningRevitYear = revitYear,
            SourceRoot = sourceRoot,
            Processed = processed,
        };
    }

    private string ResolveSourceRoot(string? sourceRoot)
    {
        var raw = !string.IsNullOrWhiteSpace(sourceRoot)
            ? sourceRoot.Trim()
            : _settingsStore.Current.DefaultFamilyLibraryPath?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                "sourceRoot is required unless Default family library path is set in bridge settings.");
        }

        var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(raw));
        if (!Directory.Exists(fullPath))
        {
            throw new InvalidOperationException($"sourceRoot does not exist: {fullPath}");
        }

        return fullPath;
    }

    private string ResolveTargetRoot(string? targetRoot, int revitYear)
    {
        var raw = !string.IsNullOrWhiteSpace(targetRoot)
            ? targetRoot.Trim()
            : _settingsStore.Current.DefaultFamilyUpgradeOutputPath?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            var sourceFallback = ResolveSourceRoot(null);
            raw = Path.Combine(sourceFallback, $"Upgraded-Revit-{revitYear}");
        }

        var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(raw));
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    private static int ParseRevitYear(string versionNumber)
    {
        return int.TryParse(versionNumber, out var year) && year > 2000
            ? year
            : throw new InvalidOperationException($"Unable to parse Revit version year from '{versionNumber}'.");
    }

    private static string GetFamilyParameterValueAsString(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        if (familyParameter.StorageType == StorageType.None)
        {
            return string.Empty;
        }

        try
        {
            return familyParameter.StorageType switch
            {
                StorageType.String => familyType.AsString(familyParameter) ?? string.Empty,
                StorageType.Integer => familyType.AsInteger(familyParameter)?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                StorageType.Double => FormatDoubleFamilyParameterValue(familyDoc, familyType, familyParameter),
                StorageType.ElementId => FormatElementIdFamilyParameterValue(familyDoc, familyType, familyParameter),
                _ => string.Empty,
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string FormatDoubleFamilyParameterValue(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        var value = familyType.AsDouble(familyParameter);
        if (!value.HasValue)
        {
            return string.Empty;
        }

        try
        {
            return UnitFormatUtils.Format(
                familyDoc.GetUnits(),
                familyParameter.Definition.GetDataType(),
                value.Value,
                forEditing: false);
        }
        catch
        {
            return value.Value.ToString("G17", CultureInfo.InvariantCulture);
        }
    }

    private static string FormatElementIdFamilyParameterValue(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        var id = familyType.AsElementId(familyParameter);
        if (id == ElementId.InvalidElementId || id.Value <= 0)
        {
            return string.Empty;
        }

        var element = familyDoc.GetElement(id);
        return element?.Name ?? id.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static string GetCurrentTypeParameterValueAsString(
        Document familyDoc,
        FamilyManager familyManager,
        FamilyParameter familyParameter)
    {
        var currentType = familyManager.CurrentType;
        if (currentType is null)
        {
            return string.Empty;
        }

        return GetFamilyParameterValueAsString(familyDoc, currentType, familyParameter);
    }

    private static List<FamilyParameter> ResolveFormulaDriverParameters(
        string formula,
        List<FamilyParameter> parameters)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return [];
        }

        var tokenPattern = new Regex("[A-Za-z_][A-Za-z0-9_]*", RegexOptions.CultureInvariant);
        var tokens = tokenPattern.Matches(formula)
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(t => !string.Equals(t, "if", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "and", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "or", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "not", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "true", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "false", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var byName = parameters
            .Where(p => p.Definition is not null && !string.IsNullOrWhiteSpace(p.Definition.Name))
            .GroupBy(p => p.Definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var resolved = new List<FamilyParameter>();
        foreach (var token in tokens)
        {
            if (byName.TryGetValue(token, out var parameter))
            {
                resolved.Add(parameter);
            }
        }

        return resolved;
    }

    private static bool IsYesNoParameter(FamilyParameter familyParameter)
    {
        try
        {
            return familyParameter.Definition.GetDataType() == SpecTypeId.Boolean.YesNo;
        }
        catch
        {
            return false;
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    // Instance Import (v20). Rebuilds placed instances in the active Revit 2024
    // project from the export_instances snapshot. Reads all datasets C#-side;
    // only ImportSummaryResult crosses the MCP boundary. A detailed per-element
    // report and an external remap file are written to disk. NEVER stamps source
    // ids onto elements. Chunked transactions (one per CHUNK_SIZE) with a soft
    // time budget so a long execute returns a resumable partial rather than
    // tripping the dispatcher hard-timeout.
    // ───────────────────────────────────────────────────────────────────────

    private const int ImportChunkSize = 500;
    private const long ImportSoftDeadlineMs = 540_000; // stay under the 600s dispatcher allowance

    // v20.15: per-import cache of shared sloped reference planes (for tilted DTs). Keyed by the
    // work-plane normal + offset so all coplanar DTs share ONE named reference plane. Reset at the
    // start of each execute run; survives resume calls via name lookup in EnsureSlopedPlane.
    private static Dictionary<string, Reference>? _slopePlaneRefs;
    private static View? _slopeView;

    // Built-ins consumed by placement, identity, or otherwise not writable from raw.
    // Everything else is guarded by Parameter.IsReadOnly (formula/computed/read-only).
    private static readonly HashSet<string> ImportSkipParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "Category", "Family and Type", "Family", "Type", "Type Id",
        "Level", "Schedule Level", "Work Plane", "Host Id", "Host",
        "Offset from Host", "Elevation from Level", "Base Level", "Base Offset",
        "Phase Created", "Phase Demolished", "Design Option", "Image",
        "IfcGUID", "Type IfcGUID", "Family Name", "Type Name",
        "HOST_GUID", "Export to IFC", "Export Type to IFC",
    };

    private sealed class ImportTypeMapEntry
    {
        public string SrcFamily = string.Empty;
        public string SrcType = string.Empty;
        public string? TgtFamilyFile;
        public string TgtFamily = string.Empty;
        public string TgtType = string.Empty;
        public bool CreateTypeIfMissing;
        public JsonElement? TypeParamOverrides;
    }

    private sealed class ImportResolvedType
    {
        public FamilySymbol? Symbol;
        public string Status = "unresolved"; // exact | typemap | created | unresolved | error
        public string? Reason;
        public List<string> TypeParamDiffs = new();
    }

    private sealed class ImportRemapEntry
    {
        public int SrcId { get; set; }
        public string? SrcUniqueId { get; set; }
        public int NewId { get; set; }
        public string? Family { get; set; }
        public string? Type { get; set; }
        public string? ControlMark { get; set; }
    }

    // v20.1: create levels + grids from levels_grids.json in one transaction (C#-side read).
    // Skips names that already exist. The rebuild reference frame / column lines.
    private static CreateDatumsResult CreateDatums(UIApplication app, CreateDatumsRequest request)
    {
        var doc = RequireProjectDocument(app);
        var path = request.LevelsGridsPath?.Trim();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            throw new InvalidOperationException($"levelsGridsPath not found: {path}");
        }

        var skip = request.SkipExisting ?? true;
        var doLevels = request.CreateLevels ?? true;
        var doGrids = request.CreateGrids ?? true;

        using var d = JsonDocument.Parse(File.ReadAllText(path));
        var root = d.RootElement;

        var existingLevels = new HashSet<string>(
            new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().Select(l => l.Name),
            StringComparer.OrdinalIgnoreCase);
        var existingGrids = new HashSet<string>(
            new FilteredElementCollector(doc).OfClass(typeof(Grid)).Cast<Grid>().Select(g => g.Name),
            StringComparer.OrdinalIgnoreCase);

        var lc = 0; var ls = 0; var gc = 0; var gs = 0;
        var errors = new List<string>();

        using var tx = new Transaction(doc, "Bridge create datums");
        tx.Start();

        if (doLevels && root.TryGetProperty("levels", out var levels) && levels.ValueKind == JsonValueKind.Array)
        {
            foreach (var l in levels.EnumerateArray())
            {
                var name = JStr(l, "name");
                var elev = JNum(l, "elevationMm");
                if (name is null || elev is null)
                {
                    continue;
                }

                if (skip && existingLevels.Contains(name))
                {
                    ls++;
                    continue;
                }

                try
                {
                    var lvl = Level.Create(doc, MmToFeet(elev.Value));
                    try { lvl.Name = name; } catch { }
                    lc++;
                    existingLevels.Add(name);
                }
                catch (Exception ex)
                {
                    errors.Add($"level {name}: {ex.GetBaseException().Message}");
                }
            }
        }

        if (doGrids && root.TryGetProperty("grids", out var grids) && grids.ValueKind == JsonValueKind.Array)
        {
            foreach (var g in grids.EnumerateArray())
            {
                var name = JStr(g, "name");
                if (name is null)
                {
                    continue;
                }

                if (skip && existingGrids.Contains(name))
                {
                    gs++;
                    continue;
                }

                try
                {
                    var s = g.GetProperty("start");
                    var e = g.GetProperty("end");
                    var p0 = new XYZ(MmToFeet(JNum(s, "x") ?? 0), MmToFeet(JNum(s, "y") ?? 0), 0);
                    var p1 = new XYZ(MmToFeet(JNum(e, "x") ?? 0), MmToFeet(JNum(e, "y") ?? 0), 0);
                    if (p0.IsAlmostEqualTo(p1))
                    {
                        errors.Add($"grid {name}: zero-length line");
                        continue;
                    }

                    var grid = Grid.Create(doc, Line.CreateBound(p0, p1));
                    try { grid.Name = name; } catch { }
                    gc++;
                    existingGrids.Add(name);
                }
                catch (Exception ex)
                {
                    errors.Add($"grid {name}: {ex.GetBaseException().Message}");
                }
            }
        }

        doc.Regenerate();
        tx.Commit();

        return new CreateDatumsResult
        {
            LevelsCreated = lc,
            LevelsSkipped = ls,
            GridsCreated = gc,
            GridsSkipped = gs,
            Errors = errors,
        };
    }

    private static ImportSummaryResult ImportInstances(UIApplication app, ImportInstancesRequest request)
    {
        var doc = RequireProjectDocument(app);

        if (string.IsNullOrWhiteSpace(request.InstancesPath) || !File.Exists(request.InstancesPath))
        {
            throw new InvalidOperationException($"instancesPath not found: {request.InstancesPath}");
        }

        if (string.IsNullOrWhiteSpace(request.RemapOutPath))
        {
            throw new InvalidOperationException("remapOutPath is required.");
        }

        var mode = (request.Mode ?? "dryrun").Trim().ToLowerInvariant();
        if (mode != "execute" && mode != "dryrun" && mode != "reapply")
        {
            throw new InvalidOperationException("mode must be 'dryrun', 'execute', or 'reapply'.");
        }

        var isExecute = mode == "execute";
        var hostPass = request.HostPass == 2 ? 2 : 1;
        var resumeFrom = Math.Max(0, request.ResumeFromIndex ?? 0);
        var maxElements = request.MaxElements is int me && me > 0 ? me : int.MaxValue;
        var verifyOrientation = request.MaxElements is int vm && vm > 0 && vm <= 64;

        var instancesFull = Path.GetFullPath(request.InstancesPath.Trim());
        var baseDir = Path.GetDirectoryName(instancesFull) ?? ".";
        string Sibling(string? p, string name) =>
            string.IsNullOrWhiteSpace(p) ? Path.Combine(baseDir, name) : Path.GetFullPath(p!.Trim());

        var levelsGridsPath = Sibling(request.LevelsGridsPath, "levels_grids.json");
        var familyTypesPath = Sibling(request.FamilyTypesPath, "family_types.json");
        var sharedParamsPath = Sibling(request.SharedParamsPath, "shared_project_parameters.json");
        var remapOutFull = Path.GetFullPath(request.RemapOutPath.Trim());
        var reportPath = string.IsNullOrWhiteSpace(request.ReportPath)
            ? Path.Combine(Path.GetDirectoryName(remapOutFull) ?? baseDir, $"import_report_pass{hostPass}_{mode}.json")
            : Path.GetFullPath(request.ReportPath!.Trim());

        // ---- Reference datasets (materialized so their JSON docs can be freed) ----
        var sourceLevels = LoadSourceLevels(levelsGridsPath);       // srcId -> name
        var sharedByName = LoadSharedParamGuids(sharedParamsPath);  // name -> List<Guid>
        var targetLevels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList();
        var levelByName = new Dictionary<string, Level>(StringComparer.Ordinal);
        foreach (var l in targetLevels)
        {
            levelByName[l.Name] = l;
        }

        var symbolIndex = BuildSymbolIndex(doc);                    // (family,type) -> FamilySymbol

        var categoryFilter = (request.Categories ?? new List<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // family_types.json (srcTypeId -> parameters element) and typemap kept alive here.
        using var familyTypesDoc = File.Exists(familyTypesPath)
            ? JsonDocument.Parse(File.ReadAllText(familyTypesPath))
            : null;
        var typeParamsBySrcTypeId = BuildFamilyTypeParamIndex(familyTypesDoc);

        using var typeMapDoc = !string.IsNullOrWhiteSpace(request.TypeMapPath) && File.Exists(request.TypeMapPath)
            ? JsonDocument.Parse(File.ReadAllText(request.TypeMapPath!))
            : null;
        var typeMap = BuildTypeMapIndex(typeMapDoc);

        using var instancesDoc = JsonDocument.Parse(File.ReadAllText(instancesFull));
        if (!instancesDoc.RootElement.TryGetProperty("instances", out var instancesArr)
            || instancesArr.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("instances.json is missing an 'instances' array.");
        }

        var records = instancesArr.EnumerateArray().ToList();

        // Existing remap (union across passes / resumes).
        var remap = LoadRemap(remapOutFull);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var report = new ImportReportAccumulator(mode, hostPass, instancesFull, remapOutFull, reportPath);

        // ── reapply mode (v20.4): re-write parameters onto ALREADY-PLACED elements (matched
        //    via the remap srcId -> newId), no placement. Fixes params missed on the first
        //    pass — notably case-mismatched non-shared names (source NOMINAL_LENGTH vs family
        //    Nominal_Length) now resolved case-insensitively. Ignores hostPass (all placed).
        if (mode == "reapply")
        {
            var rAttempted = 0; var rUpdated = 0; var rSkipped = 0; var rLastCommitted = -1;
            var rIndex = resumeFrom; var rProcessed = 0; var rPending = 0;
            Transaction? rtx = null;
            try
            {
                for (; rIndex < records.Count; rIndex++)
                {
                    if (rProcessed >= maxElements)
                    {
                        break;
                    }

                    if (rtx is null && sw.ElapsedMilliseconds > ImportSoftDeadlineMs)
                    {
                        break;
                    }

                    var rec = records[rIndex];
                    var category = JStr(rec, "category") ?? string.Empty;
                    if (categoryFilter.Count > 0 && !categoryFilter.Contains(category))
                    {
                        continue;
                    }

                    var srcId = JInt(rec, "elementId") ?? -1;
                    if (!remap.TryGetValue(srcId, out var entry))
                    {
                        continue; // not placed — nothing to reapply
                    }

                    var el = doc.GetElement(new ElementId(entry.NewId));
                    if (el is null)
                    {
                        rSkipped++;
                        continue;
                    }

                    rAttempted++;
                    rProcessed++;
                    if (rtx is null)
                    {
                        rtx = new Transaction(doc, "Bridge reapply parameters");
                        rtx.Start();
                    }

                    var failures = new List<string>();
                    if (rec.TryGetProperty("parameters", out var pmap) && pmap.ValueKind == JsonValueKind.Object)
                    {
                        ApplySourceParameters(el, pmap, sharedByName, failures);
                    }

                    rUpdated++;
                    rPending++;
                    var er = new Dictionary<string, object?>
                    {
                        ["srcId"] = srcId,
                        ["newId"] = entry.NewId,
                        ["status"] = "reapplied",
                    };
                    if (failures.Count > 0)
                    {
                        er["paramFailures"] = failures;
                    }

                    report.AddElement(er);

                    if (rPending >= ImportChunkSize)
                    {
                        doc.Regenerate();
                        rtx.Commit();
                        rtx.Dispose();
                        rtx = null;
                        rLastCommitted = rIndex;
                        rPending = 0;
                    }
                }

                if (rtx is not null)
                {
                    doc.Regenerate();
                    rtx.Commit();
                    rtx.Dispose();
                    rtx = null;
                    rLastCommitted = rIndex - 1;
                }
            }
            catch
            {
                if (rtx is not null)
                {
                    try { rtx.RollBack(); } catch { }
                    rtx.Dispose();
                }

                throw;
            }

            var rDone = rIndex >= records.Count;
            report.Finalize(rAttempted, rUpdated, rSkipped, 0, rLastCommitted, rDone ? records.Count : rIndex, rDone);
            report.Write();
            return new ImportSummaryResult
            {
                Mode = mode,
                HostPass = hostPass,
                Attempted = rAttempted,
                Placed = rUpdated,
                Skipped = rSkipped,
                Failed = 0,
                RemapOutPath = remapOutFull,
                ReportPath = reportPath,
                LastCommittedIndex = rLastCommitted,
                NextIndex = rDone ? records.Count : rIndex,
                Done = rDone,
            };
        }

        var attempted = 0;
        var placed = 0;
        var skipped = 0;
        var failed = 0;
        var lastCommitted = -1;
        var i = resumeFrom;
        var processedThisCall = 0;
        var pendingSinceCommit = 0;

        _slopePlaneRefs = new Dictionary<string, Reference>(); // v20.15: reset shared sloped-plane cache
        _slopeView = null;

        Transaction? tx = null;

        void CommitChunk()
        {
            if (tx is null)
            {
                return;
            }

            doc.Regenerate();
            tx.Commit();
            tx.Dispose();
            tx = null;
            lastCommitted = i - 1;
            pendingSinceCommit = 0;
            WriteRemapFile(remapOutFull, remap);
        }

        try
        {
            for (; i < records.Count; i++)
            {
                if (processedThisCall >= maxElements)
                {
                    break;
                }

                if (isExecute && tx is null && sw.ElapsedMilliseconds > ImportSoftDeadlineMs)
                {
                    break; // return a resumable partial before the dispatcher hard-timeout
                }

                var rec = records[i];
                var category = JStr(rec, "category") ?? string.Empty;
                if (categoryFilter.Count > 0 && !categoryFilter.Contains(category))
                {
                    continue;
                }

                var pass = ClassifyPass(rec, sourceLevels);
                if (pass != hostPass)
                {
                    continue; // includes pass 0 (floors / sketch-based) — handled separately
                }

                attempted++;
                processedThisCall++;

                var srcId = JInt(rec, "elementId") ?? -1;
                var srcUnique = JStr(rec, "uniqueId");
                var srcFamily = (JStr(rec, "familyName") ?? string.Empty).Trim();
                var srcType = (JStr(rec, "typeName") ?? string.Empty).Trim();
                var srcTypeId = JInt(rec, "typeId");
                var controlMark = ParamValue(rec, "CONTROL_MARK");

                var elementReport = new Dictionary<string, object?>
                {
                    ["srcId"] = srcId,
                    ["family"] = srcFamily,
                    ["type"] = srcType,
                    ["pass"] = pass,
                };
                if (controlMark is not null)
                {
                    elementReport["controlMark"] = controlMark;
                }

                // v20.11: idempotent re-runs — skip a srcId already placed (its remap entry still
                // resolves to a live element). Lets a bulk run be safely resumed from index 0 after
                // a partial failure without duplicating what already committed.
                if (isExecute && remap.TryGetValue(srcId, out var existingEntry)
                    && doc.GetElement(new ElementId(existingEntry.NewId)) is not null)
                {
                    elementReport["status"] = "already-placed";
                    elementReport["newId"] = existingEntry.NewId;
                    report.AddElement(elementReport);
                    skipped++;
                    continue;
                }

                // ---- Type resolution (loads family / creates type on demand in execute) ----
                var resolved = ResolveType(
                    doc, srcFamily, srcType, srcTypeId, symbolIndex, typeMap,
                    typeParamsBySrcTypeId, sharedByName, isExecute, ref tx);

                elementReport["typeStatus"] = resolved.Status;
                if (resolved.TypeParamDiffs.Count > 0)
                {
                    elementReport["typeParamDiffs"] = resolved.TypeParamDiffs;
                }

                if (resolved.Symbol is null)
                {
                    elementReport["status"] = "unresolved";
                    elementReport["reason"] = resolved.Reason ?? "type not resolved";
                    report.AddUnresolved(srcFamily, srcType);
                    report.AddElement(elementReport);
                    skipped++;
                    continue;
                }

                if (!isExecute)
                {
                    // Dryrun: resolve host + level + params without any transaction.
                    var dryStatus = DryrunResolve(
                        doc, rec, pass, resolved.Symbol, sourceLevels, levelByName, targetLevels,
                        remap, sharedByName, elementReport, report);
                    elementReport["status"] = dryStatus;
                    report.AddElement(elementReport);
                    if (dryStatus == "resolved")
                    {
                        placed++; // "would place"
                    }
                    else
                    {
                        skipped++;
                    }

                    continue;
                }

                // ---- Execute placement ----
                if (tx is null)
                {
                    tx = new Transaction(doc, $"Bridge import instances (pass {hostPass})");
                    tx.Start();
                    // v20.7: never let a failing element pop a blocking modal dialog.
                    var fho = tx.GetFailureHandlingOptions();
                    fho.SetFailuresPreprocessor(new ImportFailureSwallower());
                    fho.SetClearAfterRollback(true);
                    tx.SetFailureHandlingOptions(fho);
                }

                try
                {
                    var symbol = resolved.Symbol;
                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                        doc.Regenerate();
                    }

                    // v20.11: isolate each placement in a sub-transaction so a bad cut that only
                    // fails at regeneration (an invalid host solid) rolls back JUST this element
                    // instead of aborting the whole chunk commit. The chunk Transaction (tx) still
                    // batches the good elements for a single outer commit.
                    using var st = new SubTransaction(doc);
                    st.Start();

                    var newFi = PlaceRecord(
                        doc, rec, pass, symbol, sourceLevels, levelByName, targetLevels,
                        remap, elementReport, report, verifyOrientation);

                    if (newFi is null)
                    {
                        st.RollBack();
                        skipped++;
                        elementReport["status"] ??= "skipped";
                        report.AddElement(elementReport);
                        continue;
                    }

                    // Parameter writes in the same transaction.
                    var paramFailures = new List<string>();
                    if (rec.TryGetProperty("parameters", out var pmap) && pmap.ValueKind == JsonValueKind.Object)
                    {
                        ApplySourceParameters(newFi, pmap, sharedByName, paramFailures);
                    }

                    if (paramFailures.Count > 0)
                    {
                        elementReport["paramFailures"] = paramFailures;
                    }

                    // Cutting families: force a regen NOW so an invalid-cut solid surfaces as a
                    // catchable, element-scoped failure (rolled back below) rather than a
                    // RegenerationFailedException at chunk commit that would abort every element.
                    if (IsVoidCutFamily(symbol))
                    {
                        try
                        {
                            doc.Regenerate();
                        }
                        catch (Exception rex)
                        {
                            st.RollBack();
                            elementReport["status"] = "failed";
                            elementReport["reason"] = "regen/cut failed: " + rex.GetBaseException().Message;
                            report.AddElement(elementReport);
                            failed++;
                            continue;
                        }
                    }

                    var newId = ToInt(newFi.Id);
                    st.Commit();

                    elementReport["newId"] = newId;
                    elementReport["status"] = "placed";
                    remap[srcId] = new ImportRemapEntry
                    {
                        SrcId = srcId,
                        SrcUniqueId = srcUnique,
                        NewId = newId,
                        Family = srcFamily,
                        Type = srcType,
                        ControlMark = controlMark,
                    };
                    report.AddElement(elementReport);
                    placed++;
                    pendingSinceCommit++;
                }
                catch (Exception ex)
                {
                    elementReport["status"] = "failed";
                    elementReport["reason"] = ex.GetBaseException().Message;
                    report.AddElement(elementReport);
                    failed++;
                }

                if (pendingSinceCommit >= ImportChunkSize)
                {
                    CommitChunk();
                }
            }

            CommitChunk();
        }
        catch
        {
            // A chunk-level fatal: discard the uncommitted chunk so we lose at most one.
            if (tx is not null)
            {
                try { tx.RollBack(); } catch { }
                tx.Dispose();
                tx = null;
            }

            throw;
        }

        var done = i >= records.Count;
        var nextIndex = done ? records.Count : i;

        report.Finalize(attempted, placed, skipped, failed, lastCommitted, nextIndex, done);
        report.Write();

        return new ImportSummaryResult
        {
            Mode = mode,
            HostPass = hostPass,
            Attempted = attempted,
            Placed = placed,
            Skipped = skipped,
            Failed = failed,
            UnresolvedTypeCount = report.UnresolvedCount,
            HostCrossCheckFailures = report.HostCrossCheckFailures,
            RemapOutPath = remapOutFull,
            ReportPath = reportPath,
            LastCommittedIndex = lastCommitted,
            NextIndex = nextIndex,
            Done = done,
        };
    }

    // Pass 1 = level-hosted point (or curve) records; pass 2 = instance-hosted; 0 = skip.
    private static int ClassifyPass(JsonElement rec, Dictionary<int, string> sourceLevels)
    {
        if (!(rec.TryGetProperty("placement", out var placement) && placement.ValueKind == JsonValueKind.Object))
        {
            return 0; // sketch-based (floors) — handled separately
        }

        var kind = JStr(placement, "kind");
        if (string.Equals(kind, "curve", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        var hostId = JInt(rec, "hostElementId");
        var workPlaneName = HostFaceWorkPlane(rec);

        var isLevelHosted =
            !hostId.HasValue
            || sourceLevels.ContainsKey(hostId.Value)
            || (workPlaneName is not null && workPlaneName.StartsWith("Level :", StringComparison.OrdinalIgnoreCase));

        return isLevelHosted ? 1 : 2;
    }

    private static string? HostFaceWorkPlane(JsonElement rec)
    {
        if (rec.TryGetProperty("hostFace", out var hf) && hf.ValueKind == JsonValueKind.Object)
        {
            return JStr(hf, "workPlaneName");
        }

        return null;
    }

    // Resolve (srcFamily,srcType) -> target FamilySymbol. In execute, loads the
    // mapped family file and clones/creates a missing type (writing type params from
    // family_types.json by source typeId, overrides last). Never throws — returns
    // an ImportResolvedType with Symbol null and a Reason on failure.
    private static ImportResolvedType ResolveType(
        Document doc,
        string srcFamily,
        string srcType,
        int? srcTypeId,
        Dictionary<(string, string), FamilySymbol> symbolIndex,
        Dictionary<(string, string), ImportTypeMapEntry> typeMap,
        Dictionary<int, JsonElement> typeParamsBySrcTypeId,
        Dictionary<string, List<Guid>> sharedByName,
        bool isExecute,
        ref Transaction? activeTx)
    {
        // 1. Exact match against loaded symbols.
        if (symbolIndex.TryGetValue((srcFamily, srcType), out var exact))
        {
            return new ImportResolvedType { Symbol = exact, Status = "exact" };
        }

        // 2. Type map.
        if (typeMap.TryGetValue((srcFamily, srcType), out var entry))
        {
            var tgtFamily = string.IsNullOrWhiteSpace(entry.TgtFamily) ? srcFamily : entry.TgtFamily.Trim();
            var tgtType = string.IsNullOrWhiteSpace(entry.TgtType) ? srcType : entry.TgtType.Trim();

            if (symbolIndex.TryGetValue((tgtFamily, tgtType), out var mapped))
            {
                return new ImportResolvedType { Symbol = mapped, Status = "typemap" };
            }

            if (!isExecute)
            {
                // Dryrun: report the requested-vs-available type-param diff, no changes.
                var res = new ImportResolvedType
                {
                    Status = "typemap-pending",
                    Reason = $"target {tgtFamily} : {tgtType} not loaded"
                        + (entry.CreateTypeIfMissing ? " (would create)" : string.Empty),
                };

                if (srcTypeId is int stid && typeParamsBySrcTypeId.TryGetValue(stid, out var srcParams))
                {
                    // Which requested type params exist on any loaded symbol of the target family?
                    var familySymbol = symbolIndex
                        .Where(kv => kv.Key.Item1 == tgtFamily)
                        .Select(kv => kv.Value)
                        .FirstOrDefault();
                    foreach (var prop in srcParams.EnumerateObject())
                    {
                        if (ImportSkipParameters.Contains(prop.Name))
                        {
                            continue;
                        }

                        var present = familySymbol is not null
                            && (familySymbol.LookupParameter(prop.Name) is not null
                                || GuidParamPresent(familySymbol, prop.Name, sharedByName));
                        if (!present)
                        {
                            res.TypeParamDiffs.Add(prop.Name);
                        }
                    }
                }

                return res;
            }

            // Execute: load the family file if the family is absent.
            var familyLoaded = symbolIndex.Keys.Any(k => k.Item1 == tgtFamily);
            if (!familyLoaded && !string.IsNullOrWhiteSpace(entry.TgtFamilyFile))
            {
                try
                {
                    // LoadFamily manages its own transaction; must not run inside ours.
                    if (activeTx is not null)
                    {
                        activeTx.Commit();
                        activeTx.Dispose();
                        activeTx = null;
                    }

                    if (File.Exists(entry.TgtFamilyFile) && doc.LoadFamily(entry.TgtFamilyFile, out var loadedFam) && loadedFam is not null)
                    {
                        foreach (var symId in loadedFam.GetFamilySymbolIds())
                        {
                            if (doc.GetElement(symId) is FamilySymbol fsNew)
                            {
                                symbolIndex[(fsNew.FamilyName, fsNew.Name)] = fsNew;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ImportResolvedType { Status = "error", Reason = $"family load failed: {ex.Message}" };
                }
            }

            if (symbolIndex.TryGetValue((tgtFamily, tgtType), out var afterLoad))
            {
                return new ImportResolvedType { Symbol = afterLoad, Status = "typemap" };
            }

            // Clone/create the missing type from a sibling type of the same family.
            if (entry.CreateTypeIfMissing)
            {
                var sibling = symbolIndex
                    .Where(kv => kv.Key.Item1 == tgtFamily)
                    .Select(kv => kv.Value)
                    .FirstOrDefault();
                if (sibling is null)
                {
                    return new ImportResolvedType
                    {
                        Status = "unresolved",
                        Reason = $"cannot create type '{tgtType}': family '{tgtFamily}' has no loaded type to clone",
                    };
                }

                try
                {
                    var ownTx = activeTx is null;
                    if (ownTx)
                    {
                        activeTx = new Transaction(doc, "Bridge import — create type");
                        activeTx.Start();
                    }

                    var created = sibling.Duplicate(tgtType) as FamilySymbol;
                    if (created is null)
                    {
                        return new ImportResolvedType { Status = "error", Reason = "Duplicate returned null" };
                    }

                    if (srcTypeId is int stid2 && typeParamsBySrcTypeId.TryGetValue(stid2, out var srcTypeParams))
                    {
                        var ignored = new List<string>();
                        ApplySourceParameters(created, srcTypeParams, sharedByName, ignored);
                    }

                    if (entry.TypeParamOverrides is JsonElement ov && ov.ValueKind == JsonValueKind.Object)
                    {
                        ApplyOverrideParameters(created, ov, sharedByName);
                    }

                    doc.Regenerate();
                    if (ownTx)
                    {
                        activeTx!.Commit();
                        activeTx.Dispose();
                        activeTx = null;
                    }

                    symbolIndex[(created.FamilyName, created.Name)] = created;
                    return new ImportResolvedType { Symbol = created, Status = "created" };
                }
                catch (Exception ex)
                {
                    return new ImportResolvedType { Status = "error", Reason = $"type create failed: {ex.Message}" };
                }
            }

            return new ImportResolvedType
            {
                Status = "unresolved",
                Reason = $"mapped target {tgtFamily} : {tgtType} not present and createTypeIfMissing=false",
            };
        }

        // 3. No mapping.
        return new ImportResolvedType { Status = "unresolved", Reason = "no exact match and no typemap entry" };
    }

    private static bool GuidParamPresent(Element e, string name, Dictionary<string, List<Guid>> sharedByName)
    {
        if (!sharedByName.TryGetValue(name, out var guids))
        {
            return false;
        }

        foreach (var g in guids)
        {
            if (e.get_Parameter(g) is not null)
            {
                return true;
            }
        }

        return false;
    }

    // Resolve host + level (no writes). Returns "resolved" or a skip/flag reason.
    private static string DryrunResolve(
        Document doc,
        JsonElement rec,
        int pass,
        FamilySymbol symbol,
        Dictionary<int, string> sourceLevels,
        Dictionary<string, Level> levelByName,
        List<Level> targetLevels,
        Dictionary<int, ImportRemapEntry> remap,
        Dictionary<string, List<Guid>> sharedByName,
        Dictionary<string, object?> elementReport,
        ImportReportAccumulator report)
    {
        var level = ResolveTargetLevel(rec, sourceLevels, levelByName, targetLevels);
        if (level is null)
        {
            elementReport["reason"] = "no target level resolved";
            return "no-level";
        }

        elementReport["level"] = level.Name;

        if (pass == 2)
        {
            var hostCheck = ResolveAndCheckHost(doc, rec, remap, elementReport, report);
            if (!hostCheck.ok)
            {
                return hostCheck.reason;
            }
        }

        return "resolved";
    }

    private static Level? ResolveTargetLevel(
        JsonElement rec,
        Dictionary<int, string> sourceLevels,
        Dictionary<string, Level> levelByName,
        List<Level> targetLevels)
    {
        var name = JStr(rec, "levelName")
            ?? ParamValue(rec, "Level")
            ?? ParamValue(rec, "Schedule Level");

        if (string.IsNullOrWhiteSpace(name))
        {
            var hostId = JInt(rec, "hostElementId");
            if (hostId is int hid && sourceLevels.TryGetValue(hid, out var byHost))
            {
                name = byHost;
            }
        }

        if (!string.IsNullOrWhiteSpace(name) && levelByName.TryGetValue(name!, out var level))
        {
            return level;
        }

        var pt = ReadPointFeet(rec);
        if (pt is not null)
        {
            return ResolveClosestLevel(targetLevels, pt.Z);
        }

        return levelByName.Values.FirstOrDefault() ?? targetLevels.FirstOrDefault();
    }

    private static (bool ok, string reason, Element? host) ResolveAndCheckHost(
        Document doc,
        JsonElement rec,
        Dictionary<int, ImportRemapEntry> remap,
        Dictionary<string, object?> elementReport,
        ImportReportAccumulator report)
    {
        var srcHost = JInt(rec, "hostGuidDecodedId") ?? JInt(rec, "hostElementId");
        if (srcHost is null)
        {
            return (false, "no host id on record", null);
        }

        if (!remap.TryGetValue(srcHost.Value, out var hostEntry))
        {
            elementReport["reason"] = $"host {srcHost} not in remap (place pass 1 first)";
            return (false, "host-not-mapped", null);
        }

        var host = doc.GetElement(new ElementId(hostEntry.NewId));
        if (host is null)
        {
            elementReport["reason"] = $"remapped host {hostEntry.NewId} not found in model";
            return (false, "host-missing", null);
        }

        elementReport["newHostId"] = hostEntry.NewId;

        var workPlaneName = HostFaceWorkPlane(rec);
        if (!string.IsNullOrWhiteSpace(workPlaneName))
        {
            // workPlaneName is a SOURCE-document string ("SrcFamily : SrcType"). The typemap
            // may have RENAMED the host family on the target, so compare against the SOURCE
            // names stored in the remap entry (primary), accepting the target names too.
            var wp = workPlaneName!.Trim();
            var expectedSource = $"{hostEntry.Family} : {hostEntry.Type}";
            var expectedTarget = host is FamilyInstance hostFi && hostFi.Symbol is FamilySymbol hs
                ? $"{hs.FamilyName} : {hs.Name}"
                : null;
            var ok = string.Equals(wp, expectedSource, StringComparison.OrdinalIgnoreCase)
                || (expectedTarget is not null && string.Equals(wp, expectedTarget, StringComparison.OrdinalIgnoreCase));
            if (!ok)
            {
                elementReport["hostCrossCheck"] = new Dictionary<string, object?>
                {
                    ["workPlaneName"] = workPlaneName,
                    ["expectedSource"] = expectedSource,
                    ["expectedTarget"] = expectedTarget,
                };
                report.HostCrossCheckFailures++;
                return (false, "host-crosscheck-disagreement", host);
            }
        }

        return (true, "ok", host);
    }

    // Places a single record and applies orientation. Returns the new instance, or
    // null if it was skipped/flagged (elementReport carries the reason).
    private static FamilyInstance? PlaceRecord(
        Document doc,
        JsonElement rec,
        int pass,
        FamilySymbol symbol,
        Dictionary<int, string> sourceLevels,
        Dictionary<string, Level> levelByName,
        List<Level> targetLevels,
        Dictionary<int, ImportRemapEntry> remap,
        Dictionary<string, object?> elementReport,
        ImportReportAccumulator report,
        bool verifyOrientation)
    {
        if (!(rec.TryGetProperty("placement", out var placement) && placement.ValueKind == JsonValueKind.Object))
        {
            elementReport["reason"] = "no placement data";
            return null;
        }

        var kind = JStr(placement, "kind") ?? "point";

        Element? host = null;
        if (pass == 2)
        {
            var hostCheck = ResolveAndCheckHost(doc, rec, remap, elementReport, report);
            if (hostCheck.ok)
            {
                host = hostCheck.host;
            }
            else if (hostCheck.reason == "host-crosscheck-disagreement")
            {
                // Real disagreement between workPlaneName and the resolved host — do NOT place.
                elementReport["status"] = "flagged";
                return null;
            }
            else
            {
                // Host unresolvable (not mapped / no host id / host missing). The source host is
                // often a work plane or un-exported element; reproduce geometry via level-hosted
                // point placement (point + orientation). Recorded for review.
                elementReport["hostFallback"] = $"{hostCheck.reason} -> point-on-level";
            }
        }

        var level = ResolveTargetLevel(rec, sourceLevels, levelByName, targetLevels);
        if (level is null)
        {
            elementReport["reason"] = "no target level resolved";
            return null;
        }

        FamilyInstance fi;
        if (string.Equals(kind, "curve", StringComparison.OrdinalIgnoreCase))
        {
            var start = ReadPointFeet(placement, "start");
            var end = ReadPointFeet(placement, "end");
            if (start is null || end is null || start.IsAlmostEqualTo(end))
            {
                elementReport["reason"] = "invalid curve endpoints";
                return null;
            }

            var line = Line.CreateBound(start, end);
            fi = doc.Create.NewFamilyInstance(line, symbol, level, StructuralType.Beam);
            elementReport["placedAs"] = "curve";
        }
        else
        {
            var pt = ReadPointFeet(rec);
            if (pt is null)
            {
                elementReport["reason"] = "no placement point";
                return null;
            }

            // v20.7/v20.9: cutting families (voids, reveals, doors) hosted on a real surface
            // cannot be reproduced by a level-hosted point + Z-rotation (Revit forbids tilting a
            // level-hosted instance off its plane). True face-host them on the resolved host's
            // matching face — orients them correctly AND lets the void cut the host.
            //   v20.7 gated this on facing.Z != 0 (tuned for doors). But reveals have MIXED
            //   orientations: many run vertically with facing=(1,0,0)/hand=(0,0,1) (facing.Z == 0)
            //   yet still face-host on a vertical panel. v20.9: face-host ALL pass-2 cutting
            //   families with a resolved host + orientation vectors, regardless of facing.Z.
            var (facingVec, handVec) = ReadOrientationVectors(rec);

            if (pass == 2 && host is not null && facingVec is not null && handVec is not null
                && IsVoidCutFamily(symbol))
            {
                var faceFi = PlaceOnHostFace(doc, host, pt, facingVec!, handVec, symbol, elementReport);
                if (faceFi is not null)
                {
                    fi = faceFi;
                    elementReport["placedAs"] = "face-hosted";
                    return CutAndReturn(doc, fi, host, symbol, elementReport);
                }

                // Face not found — place free (flat) rather than illegally rotate; flagged.
                fi = doc.Create.NewFamilyInstance(pt, symbol, level, StructuralType.NonStructural);
                elementReport["placedAs"] = "point (face-host failed — no matching host face)";
                return CutAndReturn(doc, fi, host, symbol, elementReport);
            }

            // v20.14: WorkPlaneBased families whose source work plane is TILTED (facing.Z != 0) —
            // e.g. double-tees on a sloped roof reference plane — cannot be reproduced by a
            // level-hosted point (Revit forbids tilting a level-hosted instance off its plane, so
            // the Z-only ApplyPointOrientation drops the slope and lays them flat). Place them free
            // (not level-hosted) so the slope rotation is legal, then reconstruct the full 3D
            // orientation (facing incl. tilt, then hand about facing).
            var tilted = pass != 2 && facingVec is not null && handVec is not null
                && Math.Abs(facingVec.Z) > 1e-3;
            if (tilted)
            {
                // v20.15: a point-based structural-framing DT locks to its work plane, so rotating
                // it can't tilt it. Reproduce the source method: place it on a shared SLOPED
                // reference plane (one per unique work-plane, so all coplanar DTs share it).
                fi = PlaceOnSlopedPlane(doc, pt, facingVec!, handVec!, symbol, elementReport);
            }
            else
            {
                fi = doc.Create.NewFamilyInstance(pt, symbol, level, StructuralType.NonStructural);
                elementReport["placedAs"] = pass == 2 ? "point (host fallback)" : "point";
                if (pass == 2)
                {
                    elementReport["hostVoidCutFlag"] = "placed free (not face-hosted); verify void cut manually";
                }

                // Orientation: reconstruct from the source facing/hand unit vectors, which
                // encodes rotation AND mirror in one step (v20.1). Falls back to
                // rotationDeg + flip flags only when orientation vectors are absent.
                ApplyPointOrientation(doc, fi, pt, rec, placement);
            }
        }

        // Orientation residual (pilot only — bulk runs skip the per-element regen).
        if (verifyOrientation)
        {
            try
            {
                doc.Regenerate();
                var residual = OrientationResidual(rec, fi);
                if (residual is not null)
                {
                    elementReport["orientationResidual"] = residual;
                }

                var srcMirrored = JBool(rec, "mirrored");
                if (srcMirrored != fi.Mirrored)
                {
                    elementReport["mirrorMismatch"] = new Dictionary<string, object?>
                    {
                        ["source"] = srcMirrored,
                        ["result"] = fi.Mirrored,
                    };
                }

                // Position check (v20.2): placed insertion vs source point (mm). Catches the
                // mirror-displacement class of bug that a facing-only check missed.
                var srcPt = ReadPointFeet(rec);
                var loc = (fi.Location as LocationPoint)?.Point;
                if (srcPt is not null && loc is not null)
                {
                    var dMm = FeetToMm(loc.DistanceTo(srcPt));
                    if (dMm > 1.0)
                    {
                        elementReport["positionResidualMm"] = Math.Round(dMm, 1);
                    }
                }
            }
            catch { }
        }

        // Void cut (v20.2): GM void/reveal families placed on a resolved host carve the host
        // solid. AddInstanceVoidCut throws if the host can't be cut or the instance isn't a
        // cutting void — caught and reported so it never aborts the element.
        if (host is not null && IsVoidCutFamily(symbol))
        {
            try
            {
                InstanceVoidCutUtils.AddInstanceVoidCut(doc, host, fi);
                elementReport["voidCut"] = "applied";
            }
            catch (Exception ex)
            {
                elementReport["voidCut"] = "not applied: " + ex.GetBaseException().Message;
            }
        }

        return fi;
    }

    // Reconstructs a point instance's orientation from the source facing/hand unit
    // vectors. Rotates the family default (+Y facing / +X hand) to the source facing,
    // then flips handedness (mirror) when the source hand is opposite — reproducing
    // rotation AND mirror together. This is more reliable than LocationPoint.Rotation,
    // which is ambiguous for mirrored instances. No regen (assumes standard +Y/+X default;
    // any residual is recorded by OrientationResidual on pilot runs).
    private static void ApplyPointOrientation(Document doc, FamilyInstance fi, XYZ pt, JsonElement rec, JsonElement placement)
    {
        XYZ? facing = null;
        XYZ? hand = null;
        if (rec.TryGetProperty("orientation", out var orient) && orient.ValueKind == JsonValueKind.Object)
        {
            facing = ReadVector(orient, "facingOrientation");
            hand = ReadVector(orient, "handOrientation");
        }

        // Rotation about Z. R(theta) applied to default (0,1) gives (-sin, cos); set equal
        // to (fx, fy) => theta = atan2(-fx, fy).
        // NOTE: this Z-only path is for HORIZONTAL-facing families. Vertical-face-hosted
        // families (doors/reveals, facing.Z != 0) are placed via true face-hosting in
        // PlaceRecord (v20.7) and never reach ApplyPointOrientation.
        double theta = facing is not null
            ? Math.Atan2(-facing.X, facing.Y)
            : (JNum(placement, "rotationDeg") ?? 0.0) * Math.PI / 180.0;

        if (Math.Abs(theta) > 1e-9)
        {
            var axis = Line.CreateBound(pt, pt + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, fi.Id, axis, theta);
        }

        if (facing is not null && hand is not null)
        {
            // Expected hand if NOT mirrored = default (1,0) rotated by theta = (cos, sin).
            var expHand = new XYZ(Math.Cos(theta), Math.Sin(theta), 0);
            var srcHand = new XYZ(hand.X, hand.Y, 0);
            if (srcHand.GetLength() > 1e-9 && expHand.DotProduct(srcHand) < 0)
            {
                MirrorInPlaceFlipHand(doc, fi, pt, expHand);
            }
        }
        else
        {
            // No orientation vectors — fall back to the recorded flip flags.
            try { if (JBool(rec, "facingFlipped") && fi.CanFlipFacing) fi.flipFacing(); } catch { }
            try { if (JBool(rec, "handFlipped") && fi.CanFlipHand) fi.flipHand(); } catch { }
        }
    }

    // v20.15: place a tilted point-based family (sloped double-tee) on a SHARED sloped reference
    // plane — the only way a point-based structural-framing instance actually tilts (rotating it
    // is a no-op: it stays locked to its work plane). All coplanar DTs (same work-plane normal +
    // offset) reuse ONE named reference plane, so the roof comes out as clean continuous planes
    // rather than 679 independent tilts. Mirrors how the source model was built.
    private static FamilyInstance PlaceOnSlopedPlane(Document doc, XYZ pt, XYZ facing, XYZ hand, FamilySymbol symbol, Dictionary<string, object?> report)
    {
        var normal = facing.CrossProduct(hand);
        report["slopeDeg"] = Math.Round(Math.Asin(Math.Min(1.0, Math.Abs(facing.Normalize().Z))) * 180.0 / Math.PI, 2);

        if (normal.GetLength() < 1e-9)
        {
            report["placedAs"] = "point (slope normal degenerate)";
            return doc.Create.NewFamilyInstance(pt, symbol, StructuralType.NonStructural);
        }

        normal = normal.Normalize();
        var offsetFt = pt.DotProduct(normal);
        var key = string.Format("{0:F3}|{1:F3}|{2:F3}|{3:F1}",
            normal.X, normal.Y, normal.Z, Math.Round(offsetFt * 10.0) / 10.0);

        var pref = EnsureSlopedPlane(doc, key, pt, facing, hand, report);
        if (pref is null)
        {
            report["placedAs"] = "point (no sloped plane)";
            return doc.Create.NewFamilyInstance(pt, symbol, StructuralType.NonStructural);
        }

        var fi = doc.Create.NewFamilyInstance(pref, pt, hand, symbol);
        report["placedAs"] = "sloped-refplane";
        return fi;
    }

    // Returns (creating/reusing) a shared reference-plane Reference for the given plane key. Reuses
    // by cache first, then by an existing plane's deterministic name (so resume calls don't create
    // duplicates), else creates a new named reference plane spanning hand + facing through pt.
    private static Reference? EnsureSlopedPlane(Document doc, string key, XYZ pt, XYZ facing, XYZ hand, Dictionary<string, object?> report)
    {
        _slopePlaneRefs ??= new Dictionary<string, Reference>();
        if (_slopePlaneRefs.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var name = "DT_SLOPE_" + ((uint)key.GetHashCode()).ToString();
        var plane = new FilteredElementCollector(doc).OfClass(typeof(ReferencePlane)).Cast<ReferencePlane>()
            .FirstOrDefault(rp => string.Equals(rp.Name, name, StringComparison.Ordinal));

        if (plane is null)
        {
            if (_slopeView is null || !_slopeView.IsValidObject)
            {
                _slopeView = new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>()
                    .FirstOrDefault(v => !v.IsTemplate);
                if (_slopeView is null)
                {
                    var vft = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                        .FirstOrDefault(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                    if (vft is not null) _slopeView = View3D.CreateIsometric(doc, vft.Id);
                }
            }

            if (_slopeView is null) { report["slopePlaneError"] = "no 3D view available"; return null; }

            try
            {
                plane = doc.Create.NewReferencePlane2(pt, pt.Add(hand), pt.Add(facing), _slopeView);
                try { plane.Name = name; } catch { }
                report["slopePlaneCreated"] = name;
            }
            catch (Exception ex) { report["slopePlaneError"] = ex.GetBaseException().Message; return null; }
        }

        var r = plane.GetReference();
        _slopePlaneRefs[key] = r;
        return r;
    }

    // Reproduces mirror (handedness) WITHOUT relocating the element. v20.1 used flipHand(),
    // which RELOCATES some WorkPlaneBased families by ~100m. Instead we mirror about a plane
    // THROUGH the current insertion point (the insertion lies on the plane, so it maps to
    // itself and geometry reflects locally, flipping the hand while keeping facing), then
    // snap the insertion point back as a safety net if the transform drifted it.
    private static void MirrorInPlaceFlipHand(Document doc, FamilyInstance fi, XYZ pt, XYZ handAxis)
    {
        try
        {
            var normal = handAxis.GetLength() > 1e-9 ? handAxis.Normalize() : XYZ.BasisX;
            var plane = Plane.CreateByNormalAndOrigin(normal, pt);
            ElementTransformUtils.MirrorElements(doc, new List<ElementId> { fi.Id }, plane, false);
        }
        catch
        {
            return; // leave un-mirrored (facing + position stay correct) rather than risk a bad transform
        }

        // Snap the insertion point back to the source point pt. Mirroring about ANY plane
        // parallel to the intended one, then translating the insertion back to pt, is
        // mathematically identical to mirroring about the plane through pt — so this both
        // preserves position AND yields the correct mirror, regardless of where Revit's
        // mirror actually landed. Requires a regen first so LocationPoint reads true.
        try
        {
            doc.Regenerate();
            var after = (fi.Location as LocationPoint)?.Point;
            if (after is not null && !after.IsAlmostEqualTo(pt))
            {
                ElementTransformUtils.MoveElement(doc, fi.Id, pt - after);
            }
        }
        catch { }
    }

    // Cutting-void families (voids, reveals->VOID_*, and door families) that should carve
    // their host solid. v20.5: "Personal Doors"/"Overhead Doors" GM families are pure-void
    // (0 solids) door-opening cutters — same InstanceVoidCutUtils path as VOID_* families.
    // AddInstanceVoidCut is a no-op-safe try/catch at the call site, so a non-cutting family
    // that happens to match by name is reported "not applied" rather than aborting.
    private static bool IsVoidCutFamily(FamilySymbol symbol)
    {
        var fam = symbol.FamilyName ?? string.Empty;
        return fam.IndexOf("VOID", StringComparison.OrdinalIgnoreCase) >= 0
            || fam.IndexOf("DOOR", StringComparison.OrdinalIgnoreCase) >= 0
            || fam.IndexOf("REVEAL", StringComparison.OrdinalIgnoreCase) >= 0; // v20.10: source reveal family is named "REVEAL"
    }

    private static (XYZ? facing, XYZ? hand) ReadOrientationVectors(JsonElement rec)
    {
        if (rec.TryGetProperty("orientation", out var orient) && orient.ValueKind == JsonValueKind.Object)
        {
            return (ReadVector(orient, "facingOrientation"), ReadVector(orient, "handOrientation"));
        }

        return (null, null);
    }

    // v20.7/v20.8: true face-hosting for vertical-face-hosted void families (doors, reveals).
    // Finds the host's planar face whose plane matches the source work plane (normal = facing x
    // hand, containing the insertion point) and places the family on it — orienting the void
    // correctly (no illegal level-hosted rotation) and letting it cut the host.
    //   Attempt 1: explicit face-reference hosting, trying every parallel in-tolerance face
    //              nearest-first (a family instance's faces sometimes expose null references —
    //              e.g. nested geometry — so we don't stop at the first candidate).
    //   Attempt 2: host-element auto-face hosting (works when ALL face references are null),
    //              then correct the in-plane rotation about the face normal (in-plane rotation is
    //              legal for a face-hosted instance, unlike the level-hosted tilt that failed in
    //              v20.6) to align hand to the source vector.
    // Records diagnostics under elementReport["faceHost"]; returns null (caller falls back to
    // flat point placement) only if both attempts fail.
    private static FamilyInstance? PlaceOnHostFace(Document doc, Element host, XYZ pt, XYZ facing, XYZ? hand, FamilySymbol symbol, Dictionary<string, object?> elementReport)
    {
        var diag = new Dictionary<string, object?>();
        elementReport["faceHost"] = diag;

        if (hand is null) { diag["skip"] = "no hand vector"; return null; }
        var normal = facing.CrossProduct(hand);
        if (normal.GetLength() < 1e-9) { diag["skip"] = "degenerate work-plane normal"; return null; }
        normal = normal.Normalize();

        var opt = new Options { ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine };
        var geom = host.get_Geometry(opt);
        if (geom is null) { diag["skip"] = "no host geometry"; return null; }

        var solids = new List<Solid>();
        CollectSolids(geom, solids);
        diag["solids"] = solids.Count;

        var candidates = new List<PlanarFace>();
        var parallelCount = 0;
        var nullRefCount = 0;
        foreach (var solid in solids)
        {
            foreach (Face face in solid.Faces)
            {
                if (face is not PlanarFace pf) continue;
                if (Math.Abs(pf.FaceNormal.DotProduct(normal)) < 0.99) continue; // parallel planes only
                var dist = Math.Abs((pt - pf.Origin).DotProduct(pf.FaceNormal));
                if (dist > 1.5) continue; // within 1.5 ft of the insertion plane
                parallelCount++;
                if (pf.Reference is null) { nullRefCount++; continue; }
                candidates.Add(pf);
            }
        }

        diag["parallelFacesInTol"] = parallelCount;
        diag["nullRefFaces"] = nullRefCount;
        candidates.Sort((a, b) =>
            Math.Abs((pt - a.Origin).DotProduct(a.FaceNormal))
            .CompareTo(Math.Abs((pt - b.Origin).DotProduct(b.FaceNormal))));

        // Attempt 1 — explicit face reference (nearest first).
        foreach (var pf in candidates)
        {
            // v20.12: the reference direction MUST lie in the face plane (perpendicular to the
            // face normal) or NewFamilyInstance throws "reference direction is parallel to face
            // normal". Project hand onto the face plane; if hand is (near-)parallel to the normal
            // the projection collapses, so fall back to facing-projected, then any in-plane vector.
            var fn = pf.FaceNormal.Normalize();
            var refDir = ProjectOntoPlane(hand, fn);
            if (refDir.GetLength() < 1e-6) refDir = ProjectOntoPlane(facing, fn);
            if (refDir.GetLength() < 1e-6) refDir = PerpendicularTo(fn);
            refDir = refDir.Normalize();

            try
            {
                var fi = doc.Create.NewFamilyInstance(pf.Reference, pt, refDir, symbol);
                EnsurePenetratesHost(doc, fi, pt, fn, diag);
                diag["method"] = "face-reference";
                return fi;
            }
            catch (Exception ex) { diag["faceRefError"] = ex.GetBaseException().Message; }
        }

        // Attempt 2 — host-element auto-face hosting + in-plane hand correction.
        try
        {
            var fi = doc.Create.NewFamilyInstance(pt, symbol, host, StructuralType.NonStructural);
            AlignHandInPlane(doc, fi, pt, normal, hand);
            EnsurePenetratesHost(doc, fi, pt, normal, diag);
            diag["method"] = "host-auto";
            return fi;
        }
        catch (Exception ex) { diag["hostAutoError"] = ex.GetBaseException().Message; }

        diag["method"] = "none";
        return null;
    }

    // Rotates a face-hosted instance ABOUT its face normal (an in-plane rotation, which Revit
    // permits) so its HandOrientation aligns to the source hand vector. Used after host-auto
    // placement, which orients to the face but with an arbitrary in-plane rotation.
    private static void AlignHandInPlane(Document doc, FamilyInstance fi, XYZ pt, XYZ normal, XYZ targetHand)
    {
        try
        {
            doc.Regenerate();
            var curHand = fi.HandOrientation;
            if (curHand is null || curHand.GetLength() < 1e-9) return;
            curHand = curHand.Normalize();
            var h = targetHand.Normalize();
            var sin = curHand.CrossProduct(h).DotProduct(normal);
            var cos = curHand.DotProduct(h);
            var angle = Math.Atan2(sin, cos);
            if (Math.Abs(angle) > 1e-6)
            {
                ElementTransformUtils.RotateElement(doc, fi.Id, Line.CreateBound(pt, pt + normal), angle);
            }
        }
        catch { }
    }

    // Applies the host void-cut (if this is a cutting family) and returns the instance. Shared by
    // the v20.7 face-host and fallback paths. Face-hosted void families may already cut the host
    // on placement, so a throw is re-checked against the live cutting-void set before reporting.
    private static FamilyInstance CutAndReturn(Document doc, FamilyInstance fi, Element? host, FamilySymbol symbol, Dictionary<string, object?> elementReport)
    {
        if (host is not null && IsVoidCutFamily(symbol))
        {
            try
            {
                InstanceVoidCutUtils.AddInstanceVoidCut(doc, host, fi);
                elementReport["voidCut"] = "applied";
            }
            catch (Exception ex)
            {
                var already = false;
                try { already = InstanceVoidCutUtils.GetCuttingVoidInstances(host).Contains(fi.Id); } catch { }
                elementReport["voidCut"] = already ? "applied (auto via face-host)" : "not applied: " + ex.GetBaseException().Message;
            }
        }

        return fi;
    }

    // Component of v lying in the plane whose normal is n (n assumed unit-length).
    private static XYZ ProjectOntoPlane(XYZ v, XYZ n) => v - v.DotProduct(n) * n;

    // Any unit vector perpendicular to v.
    private static XYZ PerpendicularTo(XYZ v)
    {
        var a = Math.Abs(v.X) < 0.9 ? XYZ.BasisX : XYZ.BasisY;
        var p = v.CrossProduct(a);
        return p.GetLength() > 1e-9 ? p.Normalize() : XYZ.BasisZ;
    }

    // v20.13: a face-hosted void extrudes its depth INTO or OUT of the host depending on the
    // family's handedness (which flips with the source hand sign). When it extrudes outward it
    // never overlaps the host solid, so AddInstanceVoidCut records a cut relationship but removes
    // nothing. Detect an outward-sitting void (bbox center on the +faceNormal side of the
    // insertion point) and send it to the interior side — flipFacing if the family supports it,
    // otherwise reflect it across the face plane by a translation — so the depth actually cuts.
    private static void EnsurePenetratesHost(Document doc, FamilyInstance fi, XYZ pt, XYZ faceNormal, Dictionary<string, object?> diag)
    {
        try
        {
            doc.Regenerate();
            if (VoidOutwardOffset(fi, pt, faceNormal) <= 1e-4) return; // already penetrating inward

            if (fi.CanFlipFacing)
            {
                fi.flipFacing();
                doc.Regenerate();
                if (VoidOutwardOffset(fi, pt, faceNormal) <= 1e-4) { diag["penetrationFix"] = "flipFacing"; return; }
            }

            var off = VoidOutwardOffset(fi, pt, faceNormal);
            if (off > 1e-4)
            {
                ElementTransformUtils.MoveElement(doc, fi.Id, faceNormal.Multiply(-2.0 * off));
                doc.Regenerate();
                diag["penetrationFix"] = VoidOutwardOffset(fi, pt, faceNormal) <= 1e-4 ? "move" : "move-failed";
            }
        }
        catch (Exception ex) { diag["penetrationError"] = ex.GetBaseException().Message; }
    }

    // Signed distance (feet) of the instance's bbox center beyond the insertion point along the
    // OUTWARD face normal. >0 => void sits outside the host (won't cut); <=0 => penetrates inward.
    private static double VoidOutwardOffset(FamilyInstance fi, XYZ pt, XYZ faceNormal)
    {
        var bb = fi.get_BoundingBox(null);
        if (bb is null) return 0.0;
        var center = bb.Min.Add(bb.Max).Multiply(0.5);
        return center.Subtract(pt).DotProduct(faceNormal);
    }

    // Case-insensitive parameter lookup. Revit's LookupParameter is case-sensitive, but the
    // 2024 families were re-authored with different casing (e.g. Nominal_Length vs the source's
    // NOMINAL_LENGTH), so exact-name writes silently miss. Falls back to a case-insensitive scan.
    private static Parameter? LookupParameterCI(Element e, string name)
    {
        var exact = e.LookupParameter(name);
        if (exact is not null)
        {
            return exact;
        }

        foreach (Parameter p in e.Parameters)
        {
            try
            {
                if (p?.Definition is not null && string.Equals(p.Definition.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return p;
                }
            }
            catch { }
        }

        return null;
    }

    private static Dictionary<string, object?>? OrientationResidual(JsonElement rec, FamilyInstance fi)
    {
        if (!(rec.TryGetProperty("orientation", out var orient) && orient.ValueKind == JsonValueKind.Object))
        {
            return null;
        }

        var srcFacing = ReadVector(orient, "facingOrientation");
        if (srcFacing is null)
        {
            return null;
        }

        XYZ actual;
        try { actual = fi.FacingOrientation; }
        catch { return null; }

        var dot = Math.Max(-1.0, Math.Min(1.0, srcFacing.Normalize().DotProduct(actual.Normalize())));
        var angleDeg = Math.Acos(dot) * 180.0 / Math.PI;
        if (angleDeg < 1.0)
        {
            return null; // within tolerance
        }

        return new Dictionary<string, object?>
        {
            ["facingAngleDeg"] = Math.Round(angleDeg, 2),
            ["sourceFacing"] = new[] { srcFacing.X, srcFacing.Y, srcFacing.Z },
            ["resultFacing"] = new[] { actual.X, actual.Y, actual.Z },
        };
    }

    // Writes source parameters onto a target element (instance or type). Shared
    // params resolve by GUID (inventory), non-shared by name. raw is authoritative
    // for Length (mm->ft) and Angle (deg->rad); other doubles written as-is
    // (already internal units). Read-only/formula-driven params are skipped.
    private static void ApplySourceParameters(
        Element target,
        JsonElement paramMap,
        Dictionary<string, List<Guid>> sharedByName,
        List<string> failures)
    {
        foreach (var prop in paramMap.EnumerateObject())
        {
            var name = prop.Name;
            if (ImportSkipParameters.Contains(name) || prop.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var written = false;

            if (sharedByName.TryGetValue(name, out var guids))
            {
                foreach (var g in guids)
                {
                    var p = target.get_Parameter(g);
                    if (p is null || p.IsReadOnly)
                    {
                        continue;
                    }

                    if (TryWriteImportValue(p, prop.Value, out var err))
                    {
                        written = true;
                    }
                    else if (err is not null)
                    {
                        failures.Add($"{name} [{g}]: {err}");
                    }
                }
            }

            if (!written)
            {
                var p = LookupParameterCI(target, name);
                if (p is null || p.IsReadOnly)
                {
                    continue;
                }

                if (!TryWriteImportValue(p, prop.Value, out var err) && err is not null)
                {
                    failures.Add($"{name}: {err}");
                }
            }
        }
    }

    // Overrides are literal {name: value} (already internal units / plain values).
    private static void ApplyOverrideParameters(
        Element target,
        JsonElement overrides,
        Dictionary<string, List<Guid>> sharedByName)
    {
        foreach (var prop in overrides.EnumerateObject())
        {
            Parameter? p = null;
            if (sharedByName.TryGetValue(prop.Name, out var guids))
            {
                foreach (var g in guids)
                {
                    var cand = target.get_Parameter(g);
                    if (cand is not null && !cand.IsReadOnly)
                    {
                        p = cand;
                        break;
                    }
                }
            }

            p ??= target.LookupParameter(prop.Name);
            if (p is null || p.IsReadOnly)
            {
                continue;
            }

            try { SetInstanceParameterValue(p, prop.Value); } catch { }
        }
    }

    // Writes a { value, raw } source parameter object to a target Parameter.
    private static bool TryWriteImportValue(Parameter p, JsonElement vr, out string? error)
    {
        error = null;
        try
        {
            switch (p.StorageType)
            {
                case StorageType.String:
                {
                    var s = vr.TryGetProperty("value", out var vv) && vv.ValueKind == JsonValueKind.String
                        ? vv.GetString() ?? string.Empty
                        : string.Empty;
                    p.Set(s);
                    return true;
                }

                case StorageType.Integer:
                {
                    if (vr.TryGetProperty("raw", out var raw) && raw.ValueKind == JsonValueKind.Number)
                    {
                        p.Set((int)Math.Round(raw.GetDouble()));
                        return true;
                    }

                    var val = vr.TryGetProperty("value", out var vv) ? vv.GetString() : null;
                    if (string.Equals(val, "Yes", StringComparison.OrdinalIgnoreCase)) { p.Set(1); return true; }
                    if (string.Equals(val, "No", StringComparison.OrdinalIgnoreCase)) { p.Set(0); return true; }
                    return false;
                }

                case StorageType.Double:
                {
                    if (!vr.TryGetProperty("raw", out var raw) || raw.ValueKind != JsonValueKind.Number)
                    {
                        return false; // no numeric raw (formula/string-valued) — skip
                    }

                    var d = raw.GetDouble();
                    var dataType = p.Definition.GetDataType();
                    if (dataType is not null && dataType.Equals(SpecTypeId.Length))
                    {
                        p.Set(MmToFeet(d));
                    }
                    else if (dataType is not null && dataType.Equals(SpecTypeId.Angle))
                    {
                        p.Set(d * Math.PI / 180.0);
                    }
                    else
                    {
                        p.Set(d);
                    }

                    return true;
                }

                case StorageType.ElementId:
                    error = "elementId not remapped";
                    return false;

                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            error = ex.GetBaseException().Message;
            return false;
        }
    }

    // ---- dataset loaders / small JSON helpers ----

    private static Dictionary<(string, string), FamilySymbol> BuildSymbolIndex(Document doc)
    {
        var index = new Dictionary<(string, string), FamilySymbol>();
        foreach (var fs in new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>())
        {
            var key = ((fs.FamilyName ?? string.Empty).Trim(), (fs.Name ?? string.Empty).Trim());
            index[key] = fs; // last wins; duplicate (family,type) pairs are not expected
        }

        return index;
    }

    private static Dictionary<int, string> LoadSourceLevels(string path)
    {
        var map = new Dictionary<int, string>();
        if (!File.Exists(path))
        {
            return map;
        }

        try
        {
            using var d = JsonDocument.Parse(File.ReadAllText(path));
            if (d.RootElement.TryGetProperty("levels", out var levels) && levels.ValueKind == JsonValueKind.Array)
            {
                foreach (var l in levels.EnumerateArray())
                {
                    var id = JInt(l, "id");
                    var name = JStr(l, "name");
                    if (id is int lid && name is not null)
                    {
                        map[lid] = name;
                    }
                }
            }
        }
        catch { }

        return map;
    }

    private static Dictionary<string, List<Guid>> LoadSharedParamGuids(string path)
    {
        var map = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            return map;
        }

        try
        {
            using var d = JsonDocument.Parse(File.ReadAllText(path));
            if (d.RootElement.TryGetProperty("allSharedParameterElements", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in arr.EnumerateArray())
                {
                    var name = JStr(e, "name");
                    var guidStr = JStr(e, "guid");
                    if (name is not null && Guid.TryParse(guidStr, out var guid))
                    {
                        if (!map.TryGetValue(name, out var list))
                        {
                            list = new List<Guid>();
                            map[name] = list;
                        }

                        if (!list.Contains(guid))
                        {
                            list.Add(guid);
                        }
                    }
                }
            }
        }
        catch { }

        return map;
    }

    private static Dictionary<int, JsonElement> BuildFamilyTypeParamIndex(JsonDocument? doc)
    {
        var map = new Dictionary<int, JsonElement>();
        if (doc is null)
        {
            return map;
        }

        if (doc.RootElement.TryGetProperty("types", out var types) && types.ValueKind == JsonValueKind.Array)
        {
            foreach (var t in types.EnumerateArray())
            {
                var typeId = JInt(t, "typeId");
                if (typeId is int tid && t.TryGetProperty("parameters", out var pm) && pm.ValueKind == JsonValueKind.Object)
                {
                    map[tid] = pm;
                }
            }
        }

        return map;
    }

    private static Dictionary<(string, string), ImportTypeMapEntry> BuildTypeMapIndex(JsonDocument? doc)
    {
        var map = new Dictionary<(string, string), ImportTypeMapEntry>();
        if (doc is null)
        {
            return map;
        }

        var root = doc.RootElement;
        var arr = root.ValueKind == JsonValueKind.Array
            ? root
            : (root.TryGetProperty("entries", out var e) && e.ValueKind == JsonValueKind.Array ? e : default);
        if (arr.ValueKind != JsonValueKind.Array)
        {
            return map;
        }

        foreach (var item in arr.EnumerateArray())
        {
            var srcFamily = (JStr(item, "srcFamily") ?? string.Empty).Trim();
            var srcType = (JStr(item, "srcType") ?? string.Empty).Trim();
            if (srcFamily.Length == 0 && srcType.Length == 0)
            {
                continue;
            }

            var entry = new ImportTypeMapEntry
            {
                SrcFamily = srcFamily,
                SrcType = srcType,
                TgtFamilyFile = JStr(item, "tgtFamilyFile"),
                TgtFamily = (JStr(item, "tgtFamily") ?? string.Empty).Trim(),
                TgtType = (JStr(item, "tgtType") ?? string.Empty).Trim(),
                CreateTypeIfMissing = JBool(item, "createTypeIfMissing"),
            };
            if (item.TryGetProperty("typeParamOverrides", out var ov) && ov.ValueKind == JsonValueKind.Object)
            {
                entry.TypeParamOverrides = ov;
            }

            map[(srcFamily, srcType)] = entry;
        }

        return map;
    }

    private static Dictionary<int, ImportRemapEntry> LoadRemap(string path)
    {
        var map = new Dictionary<int, ImportRemapEntry>();
        if (!File.Exists(path))
        {
            return map;
        }

        try
        {
            using var d = JsonDocument.Parse(File.ReadAllText(path));
            if (d.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in d.RootElement.EnumerateArray())
                {
                    var srcId = JInt(e, "srcId");
                    var newId = JInt(e, "newId");
                    if (srcId is int sid && newId is int nid)
                    {
                        map[sid] = new ImportRemapEntry
                        {
                            SrcId = sid,
                            SrcUniqueId = JStr(e, "srcUniqueId"),
                            NewId = nid,
                            Family = JStr(e, "family"),
                            Type = JStr(e, "type"),
                            ControlMark = JStr(e, "controlMark"),
                        };
                    }
                }
            }
        }
        catch { }

        return map;
    }

    private static void WriteRemapFile(string path, Dictionary<int, ImportRemapEntry> remap)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var list = remap.Values.OrderBy(r => r.SrcId).ToList();
        var json = JsonSerializer.Serialize(list, ExportFileJsonOptions);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(json));
    }

    private static string? ParamValue(JsonElement rec, string name)
    {
        if (rec.TryGetProperty("parameters", out var pm) && pm.ValueKind == JsonValueKind.Object
            && pm.TryGetProperty(name, out var entry) && entry.ValueKind == JsonValueKind.Object)
        {
            return JStr(entry, "value");
        }

        return null;
    }

    private static XYZ? ReadPointFeet(JsonElement rec)
    {
        if (rec.TryGetProperty("placement", out var placement) && placement.ValueKind == JsonValueKind.Object)
        {
            return ReadPointFeet(placement, "point");
        }

        return null;
    }

    private static XYZ? ReadPointFeet(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var pt) && pt.ValueKind == JsonValueKind.Object)
        {
            var x = JNum(pt, "x");
            var y = JNum(pt, "y");
            var z = JNum(pt, "z");
            if (x.HasValue && y.HasValue && z.HasValue)
            {
                return MmPoint(x.Value, y.Value, z.Value);
            }
        }

        return null;
    }

    private static XYZ? ReadVector(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Object)
        {
            var x = JNum(v, "x");
            var y = JNum(v, "y");
            var z = JNum(v, "z");
            if (x.HasValue && y.HasValue && z.HasValue)
            {
                var vec = new XYZ(x.Value, y.Value, z.Value);
                return vec.IsZeroLength() ? null : vec;
            }
        }

        return null;
    }

    private static int? JInt(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)
            ? i
            : (int?)null;

    private static string? JStr(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static double? JNum(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : (double?)null;

    private static bool JBool(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;

    // Accumulates the per-element report and writes it to disk once per call.
    private sealed class ImportReportAccumulator
    {
        private readonly string _mode;
        private readonly int _hostPass;
        private readonly string _instancesPath;
        private readonly string _remapPath;
        private readonly string _reportPath;
        private readonly List<object?> _elements = new();
        private readonly Dictionary<string, int> _unresolved = new(StringComparer.Ordinal);
        private Dictionary<string, object?> _summary = new();

        public int HostCrossCheckFailures;

        public ImportReportAccumulator(string mode, int hostPass, string instancesPath, string remapPath, string reportPath)
        {
            _mode = mode;
            _hostPass = hostPass;
            _instancesPath = instancesPath;
            _remapPath = remapPath;
            _reportPath = reportPath;
        }

        public int UnresolvedCount => _unresolved.Count;

        public void AddElement(Dictionary<string, object?> element) => _elements.Add(element);

        public void AddUnresolved(string family, string type)
        {
            var key = $"{family} :: {type}";
            _unresolved[key] = _unresolved.TryGetValue(key, out var c) ? c + 1 : 1;
        }

        public void Finalize(
            int attempted, int placed, int skipped, int failed,
            int lastCommitted, int nextIndex, bool done)
        {
            _summary = new Dictionary<string, object?>
            {
                ["mode"] = _mode,
                ["hostPass"] = _hostPass,
                ["instancesPath"] = _instancesPath,
                ["remapOutPath"] = _remapPath,
                ["attempted"] = attempted,
                ["placed"] = placed,
                ["skipped"] = skipped,
                ["failed"] = failed,
                ["lastCommittedIndex"] = lastCommitted,
                ["nextIndex"] = nextIndex,
                ["done"] = done,
                ["hostCrossCheckFailures"] = HostCrossCheckFailures,
            };
        }

        public void Write()
        {
            var unresolvedList = _unresolved
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new Dictionary<string, object?> { ["type"] = kv.Key, ["count"] = kv.Value })
                .ToList();

            var content = new Dictionary<string, object?>
            {
                ["summary"] = _summary,
                ["unresolvedTypes"] = unresolvedList,
                ["elements"] = _elements,
            };

            var dir = Path.GetDirectoryName(_reportPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(content, ExportFileJsonOptions);
            File.WriteAllBytes(_reportPath, Encoding.UTF8.GetBytes(json));
        }
    }

    private sealed record SharedParameterContext(string FilePath, DefinitionGroup Group);

    private sealed class BridgeFamilyLoadOptions : IFamilyLoadOptions
    {
        private readonly bool _overwriteParameterValues;

        public BridgeFamilyLoadOptions(bool overwriteParameterValues)
        {
            _overwriteParameterValues = overwriteParameterValues;
        }

        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            _ = familyInUse;
            overwriteParameterValues = _overwriteParameterValues;
            return true;
        }

        public bool OnSharedFamilyFound(
            Family sharedFamily,
            bool familyInUse,
            out FamilySource source,
            out bool overwriteParameterValues)
        {
            _ = sharedFamily;
            _ = familyInUse;
            source = FamilySource.Family;
            overwriteParameterValues = _overwriteParameterValues;
            return true;
        }
    }

    // v20.7: swallow warnings and roll back on hard errors during import transactions so a failing
    // element (e.g. a void that can't cut a host, or an out-of-range placement) never posts a
    // BLOCKING modal dialog that would freeze the gateway. Warnings are deleted; errors trigger a
    // silent rollback of the current chunk (surfaced via the per-element report, not a UI prompt).
    private sealed class ImportFailureSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var hadError = false;
            foreach (var f in failuresAccessor.GetFailureMessages())
            {
                if (f.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(f);
                }
                else
                {
                    hadError = true;
                }
            }

            return hadError ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
        }
    }
}
