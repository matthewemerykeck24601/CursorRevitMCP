using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Services;

public sealed class RevitRequestDispatcher : IExternalEventHandler
{
    private readonly object _lock = new();
    private readonly Queue<QueuedRequest> _queue = new();
    private readonly ExternalEvent _externalEvent;
    private UIApplication? _lastUiApp;

    public RevitRequestDispatcher()
    {
        _externalEvent = ExternalEvent.Create(this);
    }

    public string GetName() => "RevitPublicMCPBridge.RequestDispatcher";

    public Task<T> Enqueue<T>(Func<UIApplication, T> action, int timeoutMs = 15000)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_lock)
        {
            _queue.Enqueue(new QueuedRequest(
                app => action(app),
                tcs));
        }

        _externalEvent.Raise();
        return WaitWithTimeout<T>(tcs.Task, timeoutMs);
    }

    public void Execute(UIApplication app)
    {
        _lastUiApp = app;
        while (true)
        {
            QueuedRequest? request = null;
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    request = _queue.Dequeue();
                }
            }

            if (request is null)
            {
                return;
            }

            try
            {
                var result = request.Action(app);
                request.Completion.TrySetResult(result);
            }
            catch (Exception ex)
            {
                request.Completion.TrySetException(ex);
            }
        }
    }

    public UIApplication? LastUiApp => _lastUiApp;

    public async Task<bool> Ping(int timeoutMs = 1000)
    {
        var result = await Enqueue(_ => true, timeoutMs);
        return result;
    }

    private static async Task<T> WaitWithTimeout<T>(Task<object?> task, int timeoutMs)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
        if (completed != task)
        {
            throw new TimeoutException("Timed out waiting for Revit API execution.");
        }

        var raw = await task;
        return raw is T value
            ? value
            : throw new InvalidCastException($"Unexpected result type {raw?.GetType().FullName ?? "null"}.");
    }

    private sealed class QueuedRequest
    {
        public QueuedRequest(Func<UIApplication, object?> action, TaskCompletionSource<object?> completion)
        {
            Action = action;
            Completion = completion;
        }

        public Func<UIApplication, object?> Action { get; }

        public TaskCompletionSource<object?> Completion { get; }
    }
}
