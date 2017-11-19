using System;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var firstCompletedTask = await Task.WhenAny(task, Task.Delay(timeout));

            if (!firstCompletedTask.IsCanceled && firstCompletedTask != task)
            {
                throw new TimeoutException();
            }

            await task;
        }
    }
}
