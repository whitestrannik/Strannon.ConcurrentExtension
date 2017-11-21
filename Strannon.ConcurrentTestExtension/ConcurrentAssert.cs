using System;
using System.Threading.Tasks;

namespace Strannon.ConcurrentTestExtension
{
    public static class ConcurrentAssert
    {
        public static void EnsureThatTaskIsNeverCompleted(Task task)
        {
            EnsureThatTaskNeverComplete(task, TimeSpan.FromMilliseconds(1000));
        }

        public static void EnsureThatTaskNeverComplete(Task task, TimeSpan timeSpan)
        {
            var firstTask = Task.WhenAny(task, Task.Delay(timeSpan)).Result;

            if (firstTask == task)
            {
                throw new Exception("Neverending task is unexpectedly complited");
            }
        }

        public static void EnsureThatActionIsNeverCompleted(Action action)
        {
            EnsureThatActionIsNeverCompleted(action, TimeSpan.FromMilliseconds(1000));
        }

        public static void EnsureThatActionIsNeverCompleted(Action action, TimeSpan timeSpan)
        {
            var task = Task.Run(action);
            var firstTask = Task.WhenAny(task, Task.Delay(timeSpan)).Result;

            if (firstTask == task)
            {
                throw new Exception("Neverending action is unexpectedly complited");
            }
        }

        public static void EnsureThatTaskIsCompleted(Task task)
        {
            EnsureThatTaskComplete(task, TimeSpan.FromMilliseconds(1000));
        }

        public static void EnsureThatTaskComplete(Task task, TimeSpan timeSpan)
        {
            var firstTask = Task.WhenAny(task, Task.Delay(timeSpan)).Result;

            if (firstTask != task)
            {
                throw new Exception("Task isn't complited in required time");
            }
        }

        public static void EnsureThatActionIsCompleted(Action action)
        {
            EnsureThatActionIsCompleted(action, TimeSpan.FromMilliseconds(1000));
        }

        public static void EnsureThatActionIsCompleted(Action action, TimeSpan timeSpan)
        {
            var task = Task.Run(action);
            var firstTask = Task.WhenAny(task, Task.Delay(timeSpan)).Result;

            if (firstTask != task)
            {
                throw new Exception("Action isn't complited in required time");
            }
        }
    }
}
