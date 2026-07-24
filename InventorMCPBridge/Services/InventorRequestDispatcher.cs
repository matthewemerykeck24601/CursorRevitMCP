using System.Collections.Concurrent;
using System.Windows.Threading;

namespace InventorMCPBridge.Services;

public sealed class InventorRequestDispatcher
{
    private readonly object _sync = new();
    private SynchronizationContext? _uiContext;
    private Dispatcher? _uiDispatcher;
    private readonly ConcurrentQueue<Func<object?>> _pending = new();

    public void BindUiContext(SynchronizationContext? context)
    {
        lock (_sync)
        {
            _uiContext = context;
        }
    }

    public void BindUiDispatcher(Dispatcher? dispatcher)
    {
        lock (_sync)
        {
            _uiDispatcher = dispatcher;
        }
    }

    public Task<T> Enqueue<T>(Func<T> action, int timeoutMs = 15000)
    {
        var request = new QueuedRequest<T>(action);
        SynchronizationContext? context;
        Dispatcher? dispatcher;
        lock (_sync)
        {
            context = _uiContext;
            dispatcher = _uiDispatcher;
        }

        _pending.Enqueue(request.Execute);

        void DrainQueue()
        {
            while (_pending.TryDequeue(out var work))
            {
                _ = work();
            }
        }

        if (context is not null)
        {
            context.Post(_ => DrainQueue(), null);
        }
        else if (dispatcher is not null)
        {
            _ = dispatcher.BeginInvoke(DrainQueue);
        }
        else
        {
            throw new InvalidOperationException(
                "Inventor UI synchronization context is not initialized.");
        }

        return WaitWithTimeout(request, timeoutMs);
    }

    private static async Task<T> WaitWithTimeout<T>(QueuedRequest<T> request, int timeoutMs)
    {
        var task = request.Completion.Task;
        var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
        if (completed != task && request.TryCancel())
        {
            throw new TimeoutException("Timed out waiting for Inventor API execution.");
        }

        // Once Inventor has started the action it cannot be cancelled safely. Wait for
        // its real result so callers never retry a mutation that is still executing.
        return await task;
    }

    private sealed class QueuedRequest<T>
    {
        private const int Pending = 0;
        private const int Running = 1;
        private const int Finished = 2;
        private const int Cancelled = 3;
        private readonly Func<T> _action;
        private int _state = Pending;

        public QueuedRequest(Func<T> action)
        {
            _action = action;
            Completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public TaskCompletionSource<T> Completion { get; }

        public object? Execute()
        {
            if (Interlocked.CompareExchange(ref _state, Running, Pending) != Pending)
            {
                return null;
            }

            try
            {
                Completion.TrySetResult(_action());
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
            finally
            {
                Volatile.Write(ref _state, Finished);
            }

            return null;
        }

        public bool TryCancel()
        {
            return Interlocked.CompareExchange(ref _state, Cancelled, Pending) == Pending;
        }
    }
}
