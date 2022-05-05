using System;
using System.Threading.Tasks;

namespace Outrage.Futures.Services
{
    public interface IFuturesService
    {
        Task AddFuture(Type serviceType, string methodName, object?[]? param = null, DateTimeOffset? scheduleTime = null, int retry = 0);
        Task AddFuture<TType>(string methodName, object?[]? param = null, DateTimeOffset? scheduledTime = null, int retry = 0);
    }
}
