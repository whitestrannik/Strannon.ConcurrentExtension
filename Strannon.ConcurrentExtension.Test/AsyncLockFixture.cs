using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentExtension.SyncPrimitives;
using Strannon.ConcurrentTestExtension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public sealed class AsyncLockFixture
    {
        [TestMethod]
        public void WhenClientHasGotLockAsync_OtherClientsShouldWait()
        {
            var sut = new AsyncLock();

            var @lock = sut.LockAsync().Result;

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(Task.WhenAny(sut.LockAsync(), sut.LockAsync()));
        }

        [TestMethod]
        public void WhenClientHasDisposedLock_SingleClientShouldGetLockWithFIFO()
        {
            var sut = new AsyncLock();

            var @lock = sut.LockAsync().Result;

            var task1 = sut.LockAsync();
            var task2 = sut.LockAsync();

            @lock.Dispose();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task1);
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task2);
        }

        [TestMethod]
        public void WhenLockIsAlreadyGot_WaitedAsyncClientShouldBeAbleToCancelWaiting()
        {
            var sut = new AsyncLock();

            var @lock = sut.LockAsync().Result;

            var cts = new CancellationTokenSource();
            var task1 = sut.LockAsync(cts.Token);
            var task2 = sut.LockAsync();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(Task.WhenAny(task1, task2));

            cts.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task1);
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task2);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task1);
        }

        [TestMethod]
        public void WhenLockIsAlreadyGot_WaitedAsyncClientShouldBeStopWaitingWithTimeout()
        {
            var sut = new AsyncLock();

            var @lock = sut.LockAsync().Result;

            var task1 = sut.LockAsync(TimeSpan.FromMilliseconds(300));
            var task2 = sut.LockAsync();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task1, TimeSpan.FromMilliseconds(600));
            Assert.ThrowsExceptionAsync<TimeoutException>(() => task1);
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task2);
        }
    }
}
