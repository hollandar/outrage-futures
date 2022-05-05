using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outrage.Futures.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Outrage.Futures.Tests
{
    [TestClass]
    public class FuturesTests
    {
        ServiceCollection PopulateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddFutures();
            services.AddScoped<AsynchronousTask>();
            return services;
        }

        [TestMethod()]
        public async Task SingleFuture()
        {
            var serviceCollection = PopulateServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var hostedService = serviceProvider.GetRequiredService<IHostedService>() as FuturesHostedService;
            var cancellationTokenSource = new CancellationTokenSource();
            await hostedService.StartAsync(cancellationTokenSource.Token);

            var futures = serviceProvider.GetRequiredService<IFuturesService>();
            var futuresProcessing = serviceProvider.GetRequiredService<IFuturesStorage>();
            await futures.AddFuture<AsynchronousTask>("ExecuteAsync", new object[] { new AsynchronousTaskParam { Value = 0 } });
            do { await Task.Delay(100); } while (futuresProcessing.HasNext);
            cancellationTokenSource.Cancel();

            Assert.IsTrue(AsynchronousTask.Values.Contains(0));

        }

        [TestMethod]
        public async Task SequentialFutures()
        {
            var serviceCollection = PopulateServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var hostedService = serviceProvider.GetRequiredService<IHostedService>() as FuturesHostedService;
            var cancellationTokenSource = new CancellationTokenSource();
            await hostedService.StartAsync(cancellationTokenSource.Token);

            var futures = serviceProvider.GetRequiredService<IFuturesService>();
            var futuresProcessing = serviceProvider.GetRequiredService<IFuturesStorage>();
            await futures.AddFuture<AsynchronousTask>("ExecuteAsync", new object[] { new AsynchronousTaskParam { Value = 1 } });
            await futures.AddFuture<AsynchronousTask>("ExecuteAsync", new object[] { new AsynchronousTaskParam { Value = 2 } });
            do { await Task.Delay(100); } while (futuresProcessing.HasNext);
            cancellationTokenSource.Cancel();

            Assert.IsTrue(AsynchronousTask.Values.Contains(1));
            Assert.IsTrue(AsynchronousTask.Values.Contains(2));

        }

        [TestMethod]
        public async Task ManyFutures()
        {
            var serviceCollection = PopulateServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var hostedService = serviceProvider.GetRequiredService<IHostedService>() as FuturesHostedService;
            var cancellationTokenSource = new CancellationTokenSource();
            await hostedService.StartAsync(cancellationTokenSource.Token);

            var futures = serviceProvider.GetRequiredService<IFuturesService>();
            var futuresProcessing = serviceProvider.GetRequiredService<IFuturesStorage>();
            for (int i = 3; i < 1003; i++)
                await futures.AddFuture<AsynchronousTask>("ExecuteAsync", new object[] { new AsynchronousTaskParam { Value = i } });
            do { await Task.Delay(100); } while (futuresProcessing.HasNext);
            cancellationTokenSource.Cancel();

            for (int i = 3; i < 1003; i++)
                Assert.IsTrue(AsynchronousTask.Values.Contains(i));

        }

    }
}