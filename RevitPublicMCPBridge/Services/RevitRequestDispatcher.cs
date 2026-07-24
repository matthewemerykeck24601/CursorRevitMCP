using System.Threading;
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
        var request = new QueuedRequest(
            app => action(app),
            tcs);
        lock (_lock)
        {
            _queue.Enqueue(request);
        }

        _externalEvent.Raise();
        return WaitWithTimeout<T>(request, timeoutMs);
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

            if (!request.TryStart())
            {
                continue;
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
            finally
            {
                request.MarkFinished();
            }
        }
    }

    public UIApplication? LastUiApp => _lastUiApp;

    public async Task<bool> Ping(int timeoutMs = 1000)
    {
        var result = await Enqueue(_ => true, timeoutMs);
        return result;
    }

    private static async Task<T> WaitWithTimeout<T>(QueuedRequest request, int timeoutMs)
    {
        var task = request.Completion.Task;
        var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
        if (completed != task && request.TryCancel())
        {
            throw new TimeoutException("Timed out waiting for Revit API execution.");
        }

        // Once Revit has started the action it cannot be cancelled safely. Wait for its
        // real result so callers never retry a mutation that is still executing.
        var raw = await task;
        return raw is T value
            ? value
            : throw new InvalidCastException($"Unexpected result type {raw?.GetType().FullName ?? "null"}.");
    }

    private sealed class QueuedRequest
    {
        private const int Pending = 0;
        private const int Running = 1;
        private const int Finished = 2;
        private const int Cancelled = 3;
        private int _state = Pending;

        public QueuedRequest(Func<UIApplication, object?> action, TaskCompletionSource<object?> completion)
        {
            Action = action;
            Completion = completion;
        }

        public Func<UIApplication, object?> Action { get; }

        public TaskCompletionSource<object?> Completion { get; }

        public bool TryStart()
        {
            return Interlocked.CompareExchange(ref _state, Running, Pending) == Pending;
        }

        public bool TryCancel()
        {
            return Interlocked.CompareExchange(ref _state, Cancelled, Pending) == Pending;
        }

        public void MarkFinished()
        {
            Volatile.Write(ref _state, Finished);
        }
    }
}
