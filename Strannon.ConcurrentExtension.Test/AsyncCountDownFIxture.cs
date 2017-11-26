using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentExtension.SyncPrimitives;
using Strannon.ConcurrentTestExtension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public sealed class AsyncCountDownEventFixture
    {
        [TestMethod]
        public void WhenSignaledClientCountLessThenNecessary_EventShouldBeNonSignaled()
        {
            var clientCount = 3;
            var sut = new AsyncCountDownEvent(clientCount);
            var task = sut.WaitAsync();

            var task1 = sut.SignalAndWait();
            var task2 = sut.SignalAndWait();
            var tasks = Task.WhenAny(task1, task2);
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(tasks);
        }

        [TestMethod]
        public void WhenBarrierIsNonSignaled_ClientsShouldBeWaitedWithCancel()
        {
            var clientCount = 2;
            var sut = new AsyncBarrier(clientCount);

            var cts = new CancellationTokenSource();
            var task = sut.SignalAndWaitAsync(cts.Token);
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task);

            cts.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public void WhenBarrierIsNonSignaled_ClientsShouldBeWaitedWithTimeout()
        {
            var clientCount = 2;
            var sut = new AsyncBarrier(clientCount);

            var task = sut.SignalAndWaitAsync(TimeSpan.FromMilliseconds(300));
            Assert.ThrowsExceptionAsync<TimeoutException>(() => task);
        }

        [TestMethod]
        public void WhenSignaledClientCountEqualToNecessary_BarrierShouldBecomeSignaled()
        {
            var clientCount = 3;
            var sut = new AsyncBarrier(clientCount);

            var task1 = sut.SignalAndWaitAsync();
            var task2 = sut.SignalAndWaitAsync();
            var task3 = sut.SignalAndWaitAsync();

            var allTasks = Task.WhenAll(task1, task2, task3);
            ConcurrentAssert.EnsureThatTaskIsCompleted(allTasks);
        }
    }
}
