using Microsoft.Extensions.DependencyInjection;
using Outrage.Futures.Services;

namespace Outrage.Futures
{
    public static class StartupExtensions
    {
        public static void AddFutures(this IServiceCollection services, int retryCount = 10)
        {
            services.AddSingleton<IFuturesStorage>(new InMemoryFuturesStorageService(retryCount));
            services.AddHostedService<FuturesHostedService>();
            services.AddScoped<IFuturesService, FuturesService>();
        }

        public static void AddFutures<TStorageType>(this IServiceCollection services, int retryCount = 10, TStorageType? storageInstance = null) where TStorageType: class, IFuturesStorage
        {
            if (storageInstance == null)
                services.AddSingleton<IFuturesStorage, TStorageType>();
            else
                services.AddSingleton<IFuturesStorage>(storageInstance);
            services.AddHostedService<FuturesHostedService>();
            services.AddScoped<IFuturesService, FuturesService>();
        }
    }
}
