using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    AreMessagesEquals(KeplerMethodExecutionInfo.SuccessResult, WebApiMethodExecutionInfo.SuccessResult))
                {
                    return true;
                }

                // both have the same error message
                if(!string.IsNullOrEmpty(KeplerMethodExecutionInfo.ErrorMessage) &&
                   !string.IsNullOrEmpty(WebApiMethodExecutionInfo.ErrorMessage) &&
                   AreMessagesEquals(KeplerMethodExecutionInfo.ErrorMessage, WebApiMethodExecutionInfo.ErrorMessage))
                {
                    return true;
                }

                // sometimes Kepler service (e.g.BulkImportService) returns model with error wrapped inside but WebApi service just throws SoapException.
                // we must compare if error messages are the same
                if (!string.IsNullOrEmpty(KeplerMethodExecutionInfo.SuccessResult) &&
                    !string.IsNullOrEmpty(WebApiMethodExecutionInfo.ErrorMessage) &&
                    AreMessagesEquals(KeplerMethodExecutionInfo.SuccessResult, WebApiMethodExecutionInfo.ErrorMessage))
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

        private bool AreMessagesEquals(string keplerMessage, string webApiMessage)
        {
	        if (string.Equals(keplerMessage, webApiMessage, StringComparison.OrdinalIgnoreCase))
	        {
		        return true;
	        }

	        if (keplerMessage.Contains(webApiMessage) || webApiMessage.Contains(keplerMessage))
	        {
		        return true;
	        }

	        var keplerObject = JsonConvert.DeserializeObject(keplerMessage);
	        var webApiObject = JsonConvert.DeserializeObject(webApiMessage);

	        return AreDynamicObjectsEqual(keplerObject, webApiObject);
        }

        private bool AreDynamicObjectsEqual(dynamic keplerObject, dynamic webApiObject)
        {
            JObject keplerJObject = keplerObject;
	        JObject webApiJObject = webApiObject;

	        var keplerObjectProperties = keplerJObject?.ToObject<Dictionary<string, object>>();
	        var webApiObjectProperties = webApiJObject?.ToObject<Dictionary<string, object>>();

	        if (keplerObjectProperties == null && webApiObjectProperties == null)
	        {
		        return true;
	        }

            if (keplerObjectProperties == null ||
	            webApiObjectProperties == null ||
	            keplerObjectProperties.Count != webApiObjectProperties.Count)
	        {
		        return false;
	        }

            foreach (var keplerProperty in keplerObjectProperties)
            {
	            var keplerPropertyKey = keplerProperty.Key;
	            var keplerPropertyValue = keplerProperty.Value;
                
                if (!webApiObjectProperties.ContainsKey(keplerProperty.Key))
		        {
			        return false;
		        }
                
		        var webApiPropertyValue = webApiObjectProperties[keplerProperty.Key];

		        if (webApiPropertyValue?.ToString() != keplerPropertyValue?.ToString())
		        {
			        return false;
		        }
	        }

	        return true;
        }
    }
}
