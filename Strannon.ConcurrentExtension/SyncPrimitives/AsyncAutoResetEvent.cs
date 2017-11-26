using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.SyncPrimitives
{
    public sealed class AsyncAutoResetEvent : SynchronizationPrimitive<AsyncAutoResetEvent>
    {
        private readonly Queue<AsyncTaskCompletionSource> _waitingClientsQueue;
        private readonly object _lock;
        private bool _isSignalState;

        public AsyncAutoResetEvent()
        {
            _lock = new object();
            _waitingClientsQueue = new Queue<AsyncTaskCompletionSource>();
        }

        public override bool IsSignaled => _isSignalState;

        public void Wait()
        {
            WaitAsync().WaitAndUnwrapException();
        }

        public void Wait(TimeSpan timeout)
        {
            WaitAsync(timeout).WaitAndUnwrapException();
        }

        public void Wait(CancellationToken token)
        {
            WaitAsync(token).WaitAndUnwrapException();
        }

        public void Wait(TimeSpan timeout, CancellationToken token)
        {
            WaitAsync(timeout, token).WaitAndUnwrapException();
        }

        public Task WaitAsync()
        {
            return WaitAsync(Consts.EternityTimeSpan, CancellationToken.None);
        }

        public Task WaitAsync(TimeSpan timeOut)
        {
            return WaitAsync(timeOut, CancellationToken.None);
        }

        public Task WaitAsync(CancellationToken token)
        {
            return WaitAsync(Consts.EternityTimeSpan, token);
        }

        public Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            AsyncTaskCompletionSource tcs = null;

            lock (_lock)
            {
                if (_isSignalState)
                {
                    _isSignalState = false;
                    return Task.CompletedTask;
                }
                else
                {
                    tcs = new AsyncTaskCompletionSource();
                    _waitingClientsQueue.Enqueue(tcs);
                    return tcs.WaitWithTimeoutAndCancelAsync(timeOut, token);
                }
            }
        }

        public void Set()
        {
            AsyncTaskCompletionSource tcs = null;

            lock (_lock)
            {
                if (!_isSignalState)
                {
                    tcs = GetWaitingClientFromQueue();
                    if (tcs == null)
                    {
                        _isSignalState = true;
                    }
                    else
                    {
                        tcs.TrySetResult();
                    }
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _isSignalState = false;
            }
        }

        private AsyncTaskCompletionSource GetWaitingClientFromQueue()
        {
            AsyncTaskCompletionSource tcs = null;

            while (_waitingClientsQueue.TryDequeue(out tcs))
            {
                if (!tcs.Task.IsCompleted)
                {
                    return tcs;
                }
            }

            return null;
        }
    }
}
