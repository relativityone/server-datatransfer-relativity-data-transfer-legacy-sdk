using System;
using System.Collections.Generic;
using System.Linq;
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
	        JObject keplerJObject = null;
	        JObject webApiJObject = null;
	        
	        if (keplerObject is JObject && webApiObject is JObject)
	        {
		        keplerJObject = keplerObject;
		        webApiJObject = webApiObject;
            }

	        if (keplerObject is JArray && webApiObject is JArray)
	        {
		        keplerJObject = ((JArray)keplerObject).Children<JObject>().FirstOrDefault();
		        webApiJObject = ((JArray)webApiObject).Children<JObject>().FirstOrDefault();
	        }

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
                
                if (!webApiObjectProperties.ContainsKey(keplerPropertyKey))
		        {
			        return false;
		        }
                
		        var webApiPropertyValue = webApiObjectProperties[keplerPropertyKey];

                // ignore RunID when comparing results - it is different for every WebApi or Kepler request
                if (keplerPropertyKey == "RunID")
                {
	                continue;
                }

                if (keplerPropertyKey == "ExceptionDetail" || keplerPropertyKey == "Details")
                {
	                var innerResult = AreDynamicObjectsEqual(keplerPropertyValue, webApiPropertyValue);
	                if (innerResult == false)
	                {
		                return false;
	                }

                    continue;
                }

                if (keplerPropertyValue?.ToString() != webApiPropertyValue?.ToString())
                {
	                return false;
                }
	        }

	        return true;
        }
    }
}
