using System;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models
{
    public class ServiceMethodComparisonResult
    {
        public ServiceMethodExecutionInfo KeplerMethodExecutionInfo { get; set; }

        public ServiceMethodExecutionInfo WebApiMethodExecutionInfo { get; set; }

        public bool IsValid
        {
            get
            {
                if (KeplerMethodExecutionInfo == null || WebApiMethodExecutionInfo == null)
                {
                    return false;
                }

                // both have the same success result json
                if (!string.IsNullOrEmpty(KeplerMethodExecutionInfo.SuccessResult) &&
                    !string.IsNullOrEmpty(WebApiMethodExecutionInfo.SuccessResult) &&
                    string.Equals(KeplerMethodExecutionInfo.SuccessResult, WebApiMethodExecutionInfo.SuccessResult, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // both have the same error message
                if(!string.IsNullOrEmpty(KeplerMethodExecutionInfo.ErrorMessage) &&
                   !string.IsNullOrEmpty(WebApiMethodExecutionInfo.ErrorMessage) &&
                   (KeplerMethodExecutionInfo.ErrorMessage.Contains(WebApiMethodExecutionInfo.ErrorMessage) || WebApiMethodExecutionInfo.ErrorMessage.Contains(KeplerMethodExecutionInfo.ErrorMessage)))
                {
                    return true;
                }

                // sometimes Kepler service (e.g.BulkImportService) returns model with error wrapped inside but WebApi service just throws SoapException.
                // we must compare if error messages are the same
                if (!string.IsNullOrEmpty(KeplerMethodExecutionInfo.SuccessResult) &&
                    !string.IsNullOrEmpty(WebApiMethodExecutionInfo.ErrorMessage) &&
                    KeplerMethodExecutionInfo.SuccessResult.Contains(WebApiMethodExecutionInfo.ErrorMessage))
                {
                    return true;
                }

                return false;
            }
        }

        public ServiceMethodComparisonResult()
        {
            KeplerMethodExecutionInfo = new ServiceMethodExecutionInfo();
            WebApiMethodExecutionInfo = new ServiceMethodExecutionInfo();
        }
    }
}
