using Newtonsoft.Json;
using Outrage.Futures.Models;
using System;
using System.Threading.Tasks;

namespace Outrage.Futures.Services
{
    internal class FuturesService : IFuturesService
    {
        private readonly IFuturesStorage processingStorage;

        public FuturesService(IFuturesStorage processingStorage)
        {
            this.processingStorage = processingStorage;
        }

        public async Task AddFuture<TType>(string methodName, object?[]? param, DateTimeOffset? scheduleTime = null, int retry = 0)
        {
            await AddFuture(typeof(TType), methodName, param, scheduleTime, retry);
        }

        public async Task AddFuture(Type serviceType, string methodName, object?[]? param, DateTimeOffset? scheduleTime = null, int retry = 0)
        {
            ArgumentNullException.ThrowIfNull(serviceType.AssemblyQualifiedName, "Generic types are not supported.");
            var typeReference = new TaskReference(
                serviceType.AssemblyQualifiedName,
                methodName,
                JsonConvert.SerializeObject(param, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
                retry, 
                scheduleTime ?? DateTimeOffset.UtcNow
            );

            await this.processingStorage.WriteCallAsync(typeReference);
        }
    }
}
