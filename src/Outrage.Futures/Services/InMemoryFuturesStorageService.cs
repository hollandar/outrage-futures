using Outrage.Futures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outrage.Futures.Services
{
    public class InMemoryFuturesStorageService : IFuturesStorage
    {
        readonly TimeSpan fallbackTimespan = TimeSpan.FromSeconds(20);
        readonly int retryCount = 10;
        readonly List<TaskReference> taskReferences = new List<TaskReference>();
        readonly ReaderWriterLockSlim readerWriterLockSlim = new();

        public InMemoryFuturesStorageService(int retryCount = 10)
        {
            this.retryCount = retryCount;
        }

        public bool HasNext
        {
            get
            {
                try
                {
                    readerWriterLockSlim.EnterReadLock();
                    if (!taskReferences.Any()) return false;
                    return taskReferences.OrderBy(r => r.RunTime).First().RunTime < DateTimeOffset.UtcNow;
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public Task<TaskReference> ReadNextAsync()
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();
                var result = this.taskReferences.OrderBy(r => r.RunTime).First();
                this.taskReferences.Remove(result);

                return Task.FromResult(result);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public Task WriteCallAsync(TaskReference typeReference)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();
                this.taskReferences.Add(typeReference);

                return Task.CompletedTask;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public Task RetryAsync(TaskReference next)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();
                next.RunTime = DateTimeOffset.UtcNow + fallbackTimespan;
                next.Retry++;
                if (next.Retry < retryCount)
                    this.taskReferences.Add(next);

                return Task.CompletedTask;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public Task CompleteAsync(TaskReference next)
        {
            return Task.CompletedTask;
        }

    }
}
