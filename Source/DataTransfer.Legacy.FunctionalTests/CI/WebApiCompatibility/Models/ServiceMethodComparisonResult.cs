﻿using System;
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

                return SuccessResultEqualsForKeplerAndWebApi() ||
                       ErrorMessageEqualsForKeplerAndWebApi() ||
                       ExceptionDetailsEqualsForKeplerAndWebApi();
            }
        }

        public ServiceMethodComparisonResult()
        {
            KeplerMethodExecutionInfo = new ServiceMethodExecutionInfo();
            WebApiMethodExecutionInfo = new ServiceMethodExecutionInfo();
        }

        private bool SuccessResultEqualsForKeplerAndWebApi()
        {
	        return !string.IsNullOrEmpty(KeplerMethodExecutionInfo.SuccessResult) &&
	               !string.IsNullOrEmpty(WebApiMethodExecutionInfo.SuccessResult) &&
	               AreMessagesEquals(KeplerMethodExecutionInfo.SuccessResult, WebApiMethodExecutionInfo.SuccessResult);
        }

        private bool ErrorMessageEqualsForKeplerAndWebApi()
        {
	        return !string.IsNullOrEmpty(KeplerMethodExecutionInfo.ErrorMessage) &&
	               !string.IsNullOrEmpty(WebApiMethodExecutionInfo.ErrorMessage) &&
	               AreMessagesEquals(KeplerMethodExecutionInfo.ErrorMessage, WebApiMethodExecutionInfo.ErrorMessage);
        }

        private bool ExceptionDetailsEqualsForKeplerAndWebApi()
        {
	        return !string.IsNullOrEmpty(KeplerMethodExecutionInfo.SuccessResult) &&
	               !string.IsNullOrEmpty(WebApiMethodExecutionInfo.ErrorMessage) &&
	               AreMessagesEquals(KeplerMethodExecutionInfo.SuccessResult, WebApiMethodExecutionInfo.ErrorMessage);
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

	        if (keplerObject is JObject keplerJObject && webApiObject is JObject webApiJObject)
	        {
		        return AreDynamicObjectsEqual(keplerJObject, webApiJObject);
            }

	        if (keplerObject is JArray keplerJArray && webApiObject is JArray webApiJArray)
	        {
		        return AreDynamicArraysEqual(keplerJArray, webApiJArray);
            }

	        return false;
        }
        
        private bool AreDynamicObjectsEqual(JObject keplerJObject, JObject webApiJObject)
        {
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

                if (keplerPropertyValue is JObject keplerJObjecInner && webApiPropertyValue is JObject webApiJObjectInner)
                {
	                var innerResult = AreDynamicObjectsEqual(keplerJObjecInner, webApiJObjectInner);
	                if (!innerResult)
	                {
		                return false;
	                }

	                continue;
                }

                if (keplerPropertyValue is JArray keplerJArray && webApiPropertyValue is JArray webApiJArray)
                {
	                var innerResult = AreDynamicArraysEqual(keplerJArray, webApiJArray);
	                if (!innerResult)
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

        private bool AreDynamicArraysEqual(JArray keplerJArray, JArray webApiJArray)
        {
	        var keplerJArrayItems = keplerJArray.Children<JObject>();
	        var webApiJArrayItems = webApiJArray.Children<JObject>();
			
	        if (keplerJArrayItems.Count() != webApiJArrayItems.Count())
	        {
		        return false;
	        }

	        if (keplerJArrayItems.Count() == 0)
	        {
		        return true;
	        }

			if (keplerJArrayItems.Count() == 1)
	        {
		        return AreDynamicObjectsEqual(keplerJArrayItems.First(), webApiJArrayItems.First());
            }

            var sortColumnName1 = keplerJArrayItems.First().Properties().ToArray()[0].Name;
            var sortColumnName2 = keplerJArrayItems.First().Properties().ToArray()[1].Name;
            var keplerJArrayItemsSorted = new JArray(keplerJArrayItems.OrderBy(obj => (string)obj[sortColumnName1]).ThenBy(obj => (string)obj[sortColumnName2]));
            var webApiJArrayItemsSorted = new JArray(webApiJArrayItems.OrderBy(obj => (string)obj[sortColumnName1]).ThenBy(obj => (string)obj[sortColumnName2]));
            
            for (var i = 0; i < keplerJArrayItemsSorted.Count; i++)
            {
	            var areItemsEqual = AreDynamicObjectsEqual((JObject)keplerJArrayItemsSorted[i], (JObject)webApiJArrayItemsSorted[i]);
	            if (!areItemsEqual)
	            {
		            return false;
	            }
            }

            return true;
        }
    }
}
