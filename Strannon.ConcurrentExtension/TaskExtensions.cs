using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public static class TaskExtensions
    {
        public static async Task WaitWithTimeoutAsync(this Task task, TimeSpan timeout)
        {
            var firstCompletedTask = await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);

            if (!firstCompletedTask.IsCanceled && firstCompletedTask != task)
            {
                throw new TimeoutException();
            }

            await task.ConfigureAwait(false);
        }

        public static async Task WaitWithTimeoutAndCancelAsync<T>(this TaskCompletionSource<T> tcs, TimeSpan timeout, CancellationToken token)
        {
            using (token.Register(() => tcs.TrySetCanceled(token)))
            {
                await tcs.Task.WaitWithTimeoutAsync(timeout).ConfigureAwait(false);
            }
        }

        public static async Task WaitWithTimeoutAndCancelAsync(this AsyncTaskCompletionSource tcs, TimeSpan timeout, CancellationToken token)
        {
            using (token.Register(() => tcs.TrySetCanceled(token)))
            {
                await tcs.Task.WaitWithTimeoutAsync(timeout).ConfigureAwait(false);
            }
        }

        public static void WaitAndUnwrapException(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task)
        {
           return task.GetAwaiter().GetResult();
        }

        public static void WaitWithoutException(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (Exception)
            {
            }
        }
    }
}
