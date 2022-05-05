using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Outrage.Futures.Exceptions;
using Outrage.Futures.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outrage.Futures.Services
{
    public class FuturesHostedService : IHostedService
    {
        const int maxDelay = 10000;
        const int delayIncrement = 200;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IFuturesStorage processingStorage;
        private readonly ILogger<FuturesHostedService> logger;
        private int delay = 0;
        private bool done = false;
        private Task? backgroundTask;

        public FuturesHostedService(IServiceScopeFactory serviceScopeFactory, IFuturesStorage processingStorage, ILogger<FuturesHostedService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.processingStorage = processingStorage;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting: Background processing.");
            this.backgroundTask = Task.Factory.StartNew(() => Process(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            return Task.CompletedTask;
        }

        private async void Process(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !done)
            {
                if (this.processingStorage.HasNext)
                {
                    using (var scope = this.serviceScopeFactory.CreateScope())
                    {
                        var nextTask = await this.processingStorage.ReadNextAsync();
                        try
                        {
                            var taskType = Type.GetType(nextTask.TypeName);
                            if (taskType == null)
                                throw new Exception($"Task type could not be found by type name {nextTask.TypeName}.");

                            var method = taskType.GetMethod(nextTask.MethodName);
                            if (method == null)
                                throw new Exception($"Task method {nextTask.MethodName} could not be found on type {nextTask.TypeName}.");

                            var param = (object?[]?)JsonConvert.DeserializeObject(nextTask.ParamsJson, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                            var task = scope.ServiceProvider.GetRequiredService(taskType);
                            object? methodResult;
                            if (param != null) 
                                methodResult = method.Invoke(task, param);
                            else
                                methodResult = method.Invoke(task, null);

                            if (methodResult is Task<ITaskResult>)
                            {
                                var methodTask = (Task<ITaskResult>)methodResult;
                                methodResult = await methodTask;
                            }
                            else
                            if (methodResult is Task)
                            {
                                var methodTask = (Task)methodResult;
                                await methodTask;
                                methodResult = null;
                            }
                            if (methodResult != null)
                                nextTask.ResultJson = JsonConvert.SerializeObject(methodResult);

                            await this.processingStorage.CompleteAsync(nextTask);
                        }
                        catch (NoRetryException)
                        {
                            // dont requeue the task
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred running a future task.");
                            var faultJson = JsonConvert.SerializeObject(ex);
                            nextTask.FaultJson = faultJson;
                            
                            await this.processingStorage.RetryAsync(nextTask);
                        }

                        delay = 0;
                    }
                }
                else
                {
                    await Task.Delay(delay);
                    if (delay < maxDelay) // 10 seconds
                        delay += delayIncrement;
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping: Background processing.");
            done = true;
            while (backgroundTask != null && backgroundTask.IsCompleted == false)
            {
                await Task.Delay(delayIncrement);
            }

            this.logger.LogInformation("Stopped: Background processing.");
        }
    }
}
