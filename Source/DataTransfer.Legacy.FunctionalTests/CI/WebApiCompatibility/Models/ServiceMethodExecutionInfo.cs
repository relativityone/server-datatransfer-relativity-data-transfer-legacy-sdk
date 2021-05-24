using System;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models
{
    public class ServiceMethodExecutionInfo
    {
        public string MethodName { get; set; }

        public object[] InputParameters { get; set; }

        /// <summary>
        /// Json populated when method execution returns data as a result.
        /// </summary>
        public string SuccessResult { get; set; }

        /// <summary>
        /// Error message populated when method execution returns error because of incorrect test environment setup.
        /// </summary>
        public string ErrorMessage { get; set; }

        public TimeSpan ExecutionTime { get; set; }
    }
}
