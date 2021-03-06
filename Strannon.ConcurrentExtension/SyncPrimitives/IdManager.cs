﻿using System.Threading;

namespace Strannon.ConcurrentExtension.SyncPrimitives
{
    internal static class IdManager<T>
    {
        private static long _id;

        internal static long GenerateId()
        {
            return Interlocked.Increment(ref _id);
        }
    }
}
