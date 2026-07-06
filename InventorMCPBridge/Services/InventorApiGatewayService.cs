using System.Net;
using System.Text;
using System.Text.Json;
using System.IO;
using InventorMCPBridge.Settings;
using InventorMCPBridge.Utilities;

namespace InventorMCPBridge.Services;

public sealed class InventorApiGatewayService
{
    private readonly SettingsStore _settingsStore;
    private readonly BridgeLogger _logger;
    private readonly InventorRequestDispatcher _dispatcher;
    private readonly ILogicExecutionService _iLogicExecutionService;
    private readonly ParameterReplicationService _parameterReplicationService;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public InventorApiGatewayService(
        SettingsStore settingsStore,
        BridgeLogger logger,
        InventorRequestDispatcher dispatcher,
        ILogicExecutionService iLogicExecutionService,
        ParameterReplicationService parameterReplicationService)
    {
        _settingsStore = settingsStore;
        _logger = logger;
        _dispatcher = dispatcher;
        _iLogicExecutionService = iLogicExecutionService;
        _parameterReplicationService = parameterReplicationService;
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
            _logger.Info("Inventor gateway disabled by settings.");
            return;
        }

        var listener = new HttpListener();
        var prefix = $"{BaseUrl}/";
        listener.Prefixes.Add(prefix);
        listener.Start();

        _cts = new CancellationTokenSource();
        _listener = listener;
        _loopTask = Task.Run(() => AcceptLoop(listener, _cts.Token));
        _logger.Info($"Inventor gateway listening on {prefix}");
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
                await WriteJson(context, 200, new GatewayEnvelope<object>
                {
                    Success = true,
                    Message = "ok",
                    Data = new { gateway = "inventor-mcp-bridge", connected = true },
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
                case "/api/get_active_document_info":
                    await HandleVoid(
                        context,
                        () => _dispatcher.Enqueue(() => _iLogicExecutionService.GetActiveDocumentInfo()));
                    return;
                case "/api/run_ilogic_rule":
                    await HandleTyped<RunILogicRuleRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.RunRule(req.RuleName, req.RunMode)));
                    return;
                case "/api/show_ilogic_form":
                    await HandleTyped<ShowILogicFormRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.ShowForm(req.FormName)));
                    return;
                case "/api/write_ilogic_rule":
                    await HandleTyped<WriteILogicRuleRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.WriteRule(req.RuleName, req.Code, req.Overwrite)));
                    return;
                case "/api/get_parameters":
                    await HandleTyped<GetParametersRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue<object>(() => _iLogicExecutionService.GetParameters(req)));
                    return;
                case "/api/set_parameters":
                    await HandleTyped<SetParametersRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.SetParameters(req), timeoutMs: 30000));
                    return;
                case "/api/replicate_parameters_from_revit_payload":
                    await HandleTyped<ReplicateParametersRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() =>
                        {
                            var setRequest = _parameterReplicationService.BuildSetRequestFromRevitPayload(req);
                            return _iLogicExecutionService.SetParameters(setRequest);
                        }, timeoutMs: 30000));
                    return;
                case "/api/prepare_informed_design_publish":
                    await HandleTyped<PreparePublishRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue<object>(() => BuildPublishReadiness(req)));
                    return;
                case "/api/list_drawing_views":
                    await HandleVoid(
                        context,
                        () => _dispatcher.Enqueue(() => _iLogicExecutionService.ListDrawingViews()));
                    return;
                case "/api/add_view_overall_dimensions":
                    await HandleTyped<AddDrawingViewOverallDimensionsRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.AddViewOverallDimensions(req), timeoutMs: 30000));
                    return;
                case "/api/add_all_views_overall_dimensions":
                    await HandleTyped<AddAllDrawingViewsOverallDimensionsRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.AddAllViewsOverallDimensions(req), timeoutMs: 30000));
                    return;
                case "/api/add_diameter_dimension":
                    await HandleTyped<AddDiameterDimensionRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.AddDiameterDimension(req), timeoutMs: 30000));
                    return;
                case "/api/add_linear_dimension":
                    await HandleTyped<AddLinearDimensionRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.AddLinearDimension(req), timeoutMs: 30000));
                    return;
                case "/api/get_document_settings":
                    await HandleTypedNoBody<DocumentSettingsRecord>(
                        context,
                        () => _dispatcher.Enqueue(() => _iLogicExecutionService.GetDocumentSettings()));
                    return;
                case "/api/set_length_units":
                    await HandleTyped<SetLengthUnitsRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.SetLengthUnits(req)));
                    return;
                case "/api/set_angle_units":
                    await HandleTyped<SetAngleUnitsRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.SetAngleUnits(req)));
                    return;
                case "/api/set_mass_units":
                    await HandleTyped<SetMassUnitsRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.SetMassUnits(req)));
                    return;
                case "/api/set_dimension_precision":
                    await HandleTyped<SetDimensionPrecisionRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.SetDimensionPrecision(req)));
                    return;
                case "/api/set_modeling_display":
                    await HandleTyped<SetModelingDisplayRequest>(
                        context,
                        body,
                        req => _dispatcher.Enqueue(() => _iLogicExecutionService.SetModelingDisplay(req)));
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
            await WriteJson(context, 500, new GatewayEnvelope<object>
            {
                Success = false,
                Message = ex.GetBaseException().Message,
            });
        }
    }

    private async Task HandleVoid(HttpListenerContext context, Func<Task<object>> action)
    {
        var data = await action();
        await WriteJson(context, 200, new GatewayEnvelope<object>
        {
            Success = true,
            Message = "ok",
            Data = data,
        });
    }

    private async Task HandleTypedNoBody<T>(HttpListenerContext context, Func<Task<T>> action)
    {
        var data = await action();
        await WriteJson(context, 200, new GatewayEnvelope<object>
        {
            Success = true,
            Message = "ok",
            Data = data,
        });
    }

    private async Task HandleTyped<T>(
        HttpListenerContext context,
        string body,
        Func<T, Task<object>> handler) where T : class, new()
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
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.OutputStream.Close();
    }

    private PreparePublishResult BuildPublishReadiness(PreparePublishRequest request)
    {
        var docInfo = _iLogicExecutionService.GetActiveDocumentInfo();
        var docName = docInfo.GetType().GetProperty("displayName")?.GetValue(docInfo)?.ToString() ?? "unknown";
        var parameters = _iLogicExecutionService.GetParameters(new GetParametersRequest
        {
            IncludeModelParameters = false,
            IncludeUserParameters = true,
        });

        return new PreparePublishResult
        {
            DocumentName = docName,
            Validation = new PublishValidation
            {
                HasParameters = parameters.Count > 0,
                HasModelStateHint = true,
                HasRunLabel = !string.IsNullOrWhiteSpace(request.RunLabel),
            },
            Checklist =
            [
                "Open Informed Design add-in in Inventor.",
                "Confirm exposed parameters match manufacturer-approved options.",
                "Select BIM-friendly model state for Revit representation.",
                "Publish package to ACC/Fusion Team target folder.",
                "Validate resulting RFA in Revit with one sample configuration."
            ],
        };
    }
}
