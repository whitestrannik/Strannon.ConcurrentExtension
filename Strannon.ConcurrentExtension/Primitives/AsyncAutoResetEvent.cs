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
            TaskCompletionSource<object> tcs = null;

            lock (_lock)
            {
                if (_isSignalState)
                {
                    _isSignalState = false;
                    return;
                }

                tcs = TaskHelper.CreateTaskCompletitionSource();
                _waitingClientsQueue.Enqueue(tcs);
            }

            tcs.Task.Wait();
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

        public async Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            TaskCompletionSource<object> tcs = null;
            bool isSignalState;

            lock (_lock)
            {
                isSignalState = _isSignalState;
                if (_isSignalState)
                {
                    _isSignalState = false;
                }
                else
                {
                    tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
                    _waitingClientsQueue.Enqueue(tcs);
                }
            }

            if (isSignalState)
            {
                await Task.CompletedTask;
            }
            else
            {
                // leave the cancelled tcs in queue
                using (token.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task.WithTimeout(timeOut);
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
                    _isSignalState = true;
                }
            }

            if (tcs != null)
            {
                tcs.TrySetResult(null);
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
