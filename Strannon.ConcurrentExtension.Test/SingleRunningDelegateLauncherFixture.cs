using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentTestExtension;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public sealed class SingleRunningDelegateLauncherFixture
    {
        [TestMethod]
        public void WhenLauncherHasQueuedDelegate_ItShouldCompleted()
        {
            var sut = new SingleRunningDelegateLauncher();
            var tcs = new AsyncTaskCompletionSource();

            sut.Launch(ct => { tcs.SetResult(); }, t => { });

            ConcurrentAssert.EnsureThatTaskIsCompleted(tcs.Task);
        }

        [TestMethod]
        public void WhenLauncherHasQueuedDelegate_TheOnFinishedActionShouldBeCalledIf()
        {
            var sut = new SingleRunningDelegateLauncher();
            var tcs = new AsyncTaskCompletionSource();
            Task task = null;

            sut.Launch(ct => { }, t => { tcs.SetResult(); task = t; });

            ConcurrentAssert.EnsureThatTaskIsCompleted(tcs.Task);
            Assert.IsNotNull(task);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
        }

        [TestMethod]
        public void WhenLauncherHasQueuedDelegate_TheOnFinishedActionShouldBeCalledAfterDelegateHasFinished()
        {
            var sut = new SingleRunningDelegateLauncher();
            var tcs = new AsyncTaskCompletionSource();
            var tcs2 = new AsyncTaskCompletionSource();
            Task task = null;

            sut.Launch(ct => { tcs.Task.Wait(); }, t => { tcs2.SetResult(); task = t; });

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(tcs2.Task);

            tcs.SetResult();

            ConcurrentAssert.EnsureThatTaskIsCompleted(tcs2.Task);
        }

        [TestMethod]
        public void WhenLauncherHasNewQueuedDelegate_ThePreviousDelegateShouldBeCancelledBefore()
        {
            var barrier = new Barrier(2);
            Task task = null;
            ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();

            var sut = new SingleRunningDelegateLauncher();
           
            sut.Launch(
                ct => 
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();
                        Thread.Sleep(200);
                    }
                },
                t => 
                {
                    Interlocked.Exchange(ref task, t);
                    _queue.Enqueue(1);
                });

            barrier.SignalAndWait();

            sut.Launch(ct => { _queue.Enqueue(2); }, t => { _queue.Enqueue(3); barrier.SignalAndWait(); });

            barrier.SignalAndWait();

            var firstTask = Interlocked.CompareExchange(ref task, null, null);

            Assert.IsNotNull(firstTask);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.IsTrue(firstTask.IsCanceled);
            CollectionAssert.AreEqual(_queue.ToArray(), new[] { 1, 2, 3 });
        }
    } 
}
