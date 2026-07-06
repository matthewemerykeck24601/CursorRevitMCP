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
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        SynchronizationContext? context;
        Dispatcher? dispatcher;
        lock (_sync)
        {
            context = _uiContext;
            dispatcher = _uiDispatcher;
        }

        _pending.Enqueue(() =>
        {
            try
            {
                tcs.TrySetResult(action());
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return null;
        });

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

        return WaitWithTimeout(tcs.Task, timeoutMs);
    }

    private static async Task<T> WaitWithTimeout<T>(Task<T> task, int timeoutMs)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
        if (completed != task)
        {
            throw new TimeoutException("Timed out waiting for Inventor API execution.");
        }

        return await task;
    }
}
