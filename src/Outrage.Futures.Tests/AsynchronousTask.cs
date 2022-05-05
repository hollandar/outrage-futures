using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Outrage.Futures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Outrage.Futures.Tests
{
    public class AsynchronousTaskParam
    {
        public int Value { get; set; }
    }

    public class AsyncResult: ITaskResult
    {
        public bool Success { get; set; }
    }

    public class AsynchronousTask
    {
        static HashSet<int> values = new();
        private readonly ILogger<AsynchronousTask> logger;

        public AsynchronousTask(IServiceProvider serviceProvider)
        {
            this.logger = serviceProvider.GetService<ILogger<AsynchronousTask>>();
        }

        public static HashSet<int> Values => values;

        public Task<ITaskResult> ExecuteAsync(AsynchronousTaskParam param)
        {
            this.logger.LogInformation($"AsynchronousTask complete.");
            values.Add(param.Value);
            return Task.FromResult<ITaskResult>(new AsyncResult { Success = true });
        }
    }
}
