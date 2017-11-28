using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public sealed class SingleRunningDelegateLauncher : IDisposable
    {
        private readonly BlockingCollection<WorkItem> _queue;
        private readonly CancellationTokenSource _cts;
        private readonly Task _task;

        public SingleRunningDelegateLauncher()
        {
            _queue = new BlockingCollection<WorkItem>();
            _cts = new CancellationTokenSource();
            _task = Task.Run(() => RunConsumer());
        }

        private void RunConsumer()
        {
            WorkItem runningDelegate = null;

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
            _queue.Add(new WorkItem(action, onFinished, _cts.Token));
        }

        public void Cancel()
        {
            _queue.Add(new WorkItem(c => { }, t => { }, CancellationToken.None));
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _cts.Cancel();
            _cts.Dispose();
            _queue.Dispose();
            _task.Wait();
        }

        private sealed class WorkItem
        {
            private readonly Action<CancellationToken> _action;
            private readonly Action<Task> _onFinished;
            private CancellationTokenSource _cts;
            private readonly CancellationToken _token;
            private Task _task;
            

            public WorkItem(Action<CancellationToken> action, Action<Task> onFinished, CancellationToken token)
            {
                _action = action;
                _onFinished = onFinished;
                _token = token;
            }

            internal void Run()
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(_token);
                _task = Task.Run(() => _action(_cts.Token)).ContinueWith(task => _onFinished(task));
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
                    _cts.Dispose();
                    _task.WaitWithoutException();
                }
            }
        }
    }
}
