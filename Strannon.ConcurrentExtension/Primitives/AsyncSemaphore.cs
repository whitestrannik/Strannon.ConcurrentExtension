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
        private readonly Queue<TaskCompletionSource<object>> _waitedClients;
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
            _waitedClients = new Queue<TaskCompletionSource<object>>();
        }

        public void Wait()
        {
            var tcs = TaskHelper.CreateTaskCompletitionSource();

            lock (_lock)
            {
                _clientsCount++;
                if (_clientsCount > _maxClientsCount)
                {
                    _waitedClients.Enqueue(tcs);
                }
                else
                {
                    return;
                }
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
            var count = 0;
            var tcs = TaskHelper.CreateTaskCompletitionSource();

            lock (_lock)
            {
                count = ++_clientsCount;
                if (count > _maxClientsCount)
                {
                    _waitedClients.Enqueue(tcs);
                }
            }

            if (count > _maxClientsCount)
            {
                await Task.CompletedTask;
            }
            else
            {
                using (token.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task.WaitWithTimeoutAsync(timeOut);
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

                if (_waitedClients.Count > 0)
                {
                    _waitedClients.Dequeue().SetResult(null);
                }
            }
        }
    }
}
