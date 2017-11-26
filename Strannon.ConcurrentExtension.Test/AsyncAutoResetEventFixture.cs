using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentExtension.SyncPrimitives;
using Strannon.ConcurrentTestExtension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public class AsyncAutoResetEventFixture
    {
        [TestMethod]
        public void WhenEventIsSignaled_ItShouldNotMakeWaitedAsyncOnlySingleClient()
        {
            var sut = new AsyncAutoResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsSignaled_ItShouldNotMakeWaitedOnlySingleClient()
        {
            var sut = new AsyncAutoResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsBecameSignaled_WaitedClientsShouldStopWaitedwithFIFO()
        {
            var sut = new AsyncAutoResetEvent();

            var task1 = Task.Run(() => sut.Wait());
            Thread.Sleep(300);
            var task2 = Task.Run(() => sut.Wait());
            Thread.Sleep(300);
            var task3 = Task.Run(() => sut.Wait());
            Thread.Sleep(300);

            var task = Task.WhenAny(task1, task2, task3);
            sut.Set();
            var finishedTask = task.Result;
            Assert.AreEqual(finishedTask, task1);

            task = Task.WhenAny(task2, task3);
            sut.Set();
            finishedTask = task.Result;
            Assert.AreEqual(finishedTask, task2);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedAsyncWithCancel()
        {
            var sut = new AsyncAutoResetEvent();

            sut.Reset();
            var ct = new CancellationTokenSource();
            var task = sut.WaitAsync(ct.Token);

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task);

            ct.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedAsyncWithTimeout()
        {
            var sut = new AsyncAutoResetEvent();
            sut.Reset();

            var task = sut.WaitAsync(TimeSpan.FromMilliseconds(300));

            Assert.ThrowsExceptionAsync<TimeoutException>(() => task);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedAsyncWithTimeoutAndCancel()
        {
            var sut = new AsyncAutoResetEvent();
            sut.Reset();
            var ct = new CancellationTokenSource();
            var task = sut.WaitAsync(TimeSpan.FromMilliseconds(1000), ct.Token);

            ConcurrentAssert.EnsureThatTaskIsNotCompletedIn(task, TimeSpan.FromMilliseconds(500));

            ct.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public void WhenEventIsDefault_ItShouldBeNonSignaled()
        {
            var sut = new AsyncAutoResetEvent();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedWithCancel()
        {
            var sut = new AsyncAutoResetEvent();
            sut.Reset();
            var ct = new CancellationTokenSource();

            ct.Cancel();

            Assert.ThrowsException<TaskCanceledException>(() => sut.Wait(ct.Token));
        }
    }
}
