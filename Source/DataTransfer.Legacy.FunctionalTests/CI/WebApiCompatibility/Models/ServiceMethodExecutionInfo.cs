using System;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models
{
    public class ServiceMethodExecutionInfo
    {
        public string MethodName { get; set; }

        public object[] InputParameters { get; set; }

        public string ExecutionResult { get; set; }

        public TimeSpan ExecutionTime { get; set; }
    }
}
