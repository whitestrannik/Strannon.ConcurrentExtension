using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncAutoResetEvent
    {
        private readonly TimeSpan _eternityTimeSpan;
        private readonly Queue<TaskCompletionSource<object>> _waitingClientsQueue;
        private readonly object _lock;
        private bool _isSignalState;

        public AsyncAutoResetEvent()
        {
            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
            _lock = new object();
            _waitingClientsQueue = new Queue<TaskCompletionSource<object>>();
        }

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
            return WaitAsync(_eternityTimeSpan, CancellationToken.None);
        }

        public Task WaitAsync(TimeSpan timeOut)
        {
            return WaitAsync(timeOut, CancellationToken.None);
        }

        public Task WaitAsync(CancellationToken token)
        {
            return WaitAsync(_eternityTimeSpan, token);
        }

        public Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            TaskCompletionSource<object> tcs = null;

            lock (_lock)
            {
                if (_isSignalState)
                {
                    _isSignalState = false;
                    return Task.CompletedTask;
                }
                else
                {
                    tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
                    _waitingClientsQueue.Enqueue(tcs);
                    return tcs.WaitWithTimeoutAndCancelAsync(timeOut, token);
                }
            }
        }

        public void Set()
        {
            TaskCompletionSource<object> tcs = null;

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
                        tcs.TrySetResult(null);
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

        private TaskCompletionSource<object> GetWaitingClientFromQueue()
        {
            TaskCompletionSource<object> tcs = null;

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
