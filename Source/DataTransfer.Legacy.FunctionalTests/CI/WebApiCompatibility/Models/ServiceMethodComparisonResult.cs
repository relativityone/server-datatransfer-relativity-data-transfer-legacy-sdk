using System;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models
{
    public class ServiceMethodComparisonResult
    {
        public ServiceMethodExecutionInfo KeplerMethodExecutionInfo { get; set; }

        public ServiceMethodExecutionInfo WebApiMethodExecutionInfo { get; set; }

        public bool IsValid => KeplerMethodExecutionInfo != null &&
                               WebApiMethodExecutionInfo != null &&
                               string.Equals(KeplerMethodExecutionInfo.SuccessResult, WebApiMethodExecutionInfo.SuccessResult, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(KeplerMethodExecutionInfo.ErrorMessage, WebApiMethodExecutionInfo.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        public ServiceMethodComparisonResult()
        {
            KeplerMethodExecutionInfo = new ServiceMethodExecutionInfo();
            WebApiMethodExecutionInfo = new ServiceMethodExecutionInfo();
        }
    }
}
