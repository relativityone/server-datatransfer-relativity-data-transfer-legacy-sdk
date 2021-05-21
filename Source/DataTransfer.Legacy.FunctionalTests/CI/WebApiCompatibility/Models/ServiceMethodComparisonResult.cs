namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models
{
    public class ServiceMethodComparisonResult
    {
        public ServiceMethodExecutionInfo KeplerMethodExecutionInfo { get; set; }

        public ServiceMethodExecutionInfo WebApiMethodExecutionInfo { get; set; }

        public bool IsValid => KeplerMethodExecutionInfo.ExecutionResult == WebApiMethodExecutionInfo.ExecutionResult;

        public ServiceMethodComparisonResult()
        {
            KeplerMethodExecutionInfo = new ServiceMethodExecutionInfo();
            WebApiMethodExecutionInfo = new ServiceMethodExecutionInfo();
        }
    }
}
