using System;

namespace Outrage.Futures.Models
{
    public class TaskReference
    {
        public TaskReference(string typeName, string methodName, string paramsJson, int retry, DateTimeOffset runTime)
        {
            TypeName = typeName;
            MethodName = methodName;
            ParamsJson = paramsJson;
            Retry = retry;
            RunTime = runTime;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string TypeName { get; set; }
        public string MethodName { get; set; }
        public string ParamsJson { get; set; }
        public int Retry { get; set; } 
        public DateTimeOffset RunTime { get; set; }
        public string? FaultJson { get; set; } = null;
        public string? ResultJson { get; set; } = null;
    }
}