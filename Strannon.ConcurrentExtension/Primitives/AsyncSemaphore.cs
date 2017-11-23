using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncSemaphore
    {
        private readonly int _maxClientsCount;
        private readonly TimeSpan _eternityTimeSpan;
        private readonly object _lock;
        private readonly Queue<TaskCompletionSource<object>> _waitingClientsQueue;
        private int _clientsCount;

        public AsyncSemaphore(int maxClientsCount)
        {
            if (maxClientsCount <= 0)
            {
                throw new ArgumentOutOfRangeException("clientsCount can not be less or equal to zero.");
            }

            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
            _maxClientsCount = maxClientsCount;
            _clientsCount = 0;
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

        public Task WaitAsync(TimeSpan timeout, CancellationToken token)
        {
            lock (_lock)
            {
                _clientsCount++;
                if (_clientsCount > _maxClientsCount)
                {
                    var tcs = TaskHelper.CreateTaskCompletitionSource();
                    _waitingClientsQueue.Enqueue(tcs);
                    return tcs.WaitWithTimeoutAndCancelAsync(timeout, token);
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                _clientsCount--;
                if (_clientsCount < 0)
                {
                    throw new InvalidOperationException();
                }

                var waitingClient = GetWaitingClientFromQueue();
                if (waitingClient != null)
                {
                    waitingClient.SetResult(null);
                }
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
