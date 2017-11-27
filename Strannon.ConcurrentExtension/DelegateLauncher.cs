using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public sealed class DelegateLauncher : IDisposable
    {
        private readonly BlockingCollection<DelegateData> _queue;
        private readonly Task _task;

        public DelegateLauncher()
        {
            _queue = new BlockingCollection<DelegateData>();
            _task = Task.Run(() => RunConsumer());
        }

        private void RunConsumer()
        {
            DelegateData runningDelegate = null;

            foreach (var data in _queue.GetConsumingEnumerable())
            {
                if (runningDelegate != null)
                {
                    runningDelegate.CancelIfNotCompletedAndWait();
                    runningDelegate = null;
                }

                if (_queue.Count > 0)
                {
                    data.CancelIfNotCompletedAndWait();
                }
                else
                {
                    runningDelegate = data;
                    runningDelegate.Run();
                }
            }
        }

        public void Launch(Action<CancellationToken> action, Action<Task> onFinished)
        {
            _queue.Add(new DelegateData(action, onFinished));
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _queue.Dispose();
            _task.Wait();
        }

        private sealed class DelegateData
        {
            private readonly Action<CancellationToken> _action;
            private readonly Action<Task> _onFinished;
            private CancellationTokenSource _cts;
            private Task _task;

            public DelegateData(Action<CancellationToken> action, Action<Task> onFinished)
            {
                _action = action;
                _onFinished = onFinished;
            }

            internal void Run()
            {
                _cts = new CancellationTokenSource();
                _task = Task.Run(() => _action(_cts.Token));
            }

            internal void CancelIfNotCompletedAndWait()
            {
                if (_task == null)
                {
                    _onFinished(Task.FromCanceled(CancellationToken.None));
                    return;
                }

                if (!_task.IsCompleted)
                {
                    _cts.Cancel();
                    _task.WaitWithoutException();
                }

                _onFinished(_task);
            }
        }
    }
}
