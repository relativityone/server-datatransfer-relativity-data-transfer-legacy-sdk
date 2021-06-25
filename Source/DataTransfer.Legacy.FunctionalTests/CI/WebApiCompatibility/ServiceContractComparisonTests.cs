using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.EDDS.WebAPI.CaseManagerBase;
using kCura.EDDS.WebAPI.CodeManagerBase;
using kCura.EDDS.WebAPI.DocumentManagerBase;
using kCura.EDDS.WebAPI.ExportManagerBase;
using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.EDDS.WebAPI.FieldQueryBase;
using kCura.EDDS.WebAPI.FileIOBase;
using kCura.EDDS.WebAPI.FolderManagerBase;
using kCura.EDDS.WebAPI.ObjectManagerBase;
using kCura.EDDS.WebAPI.ObjectTypeManagerBase;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.EDDS.WebAPI.RelativityManagerBase;
using kCura.EDDS.WebAPI.SearchManagerBase;
using kCura.EDDS.WebAPI.UserManagerBase;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Exceptions;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility
{
    [TestFixture]
    [TestExecutionCategory.CI]
    [TestLevel.L3]
    public class ServiceContractComparisonTests : BaseServiceCompatibilityTest
    {
        [IdentifiedTest("4FEC15F9-5AC3-4A0B-B06A-8343B2B34155")]
        public async Task AuditServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IAuditService, AuditManager>();
        }

        [IdentifiedTest("92FE823A-D2E1-414E-BE26-279C6189FDE8")]
        public async Task BulkImportServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IBulkImportService, BulkImportManager>();
        }

        [IdentifiedTest("99983EF4-7DB6-4941-8AF8-C6AF6CD64104")]
        public async Task CaseServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<ICaseService, CaseManager>();
        }

        [IdentifiedTest("32E8B4F6-145F-418D-B37D-392712C065AB")]
        public async Task CodeServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<ICodeService, CodeManager>();
        }

        [IdentifiedTest("6E230D38-7F6A-4BA3-9C77-128CD7EC2EE9")]
        public async Task DocumentServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IDocumentService, DocumentManager>();
        }

        [IdentifiedTest("606B4EAA-DA2A-44B9-9F1C-86A93A56AD8C")]
        public async Task ExportServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IExportService, ExportManager>();
        }

        [IdentifiedTest("52570DB9-E2A4-4568-A7AF-ED641045DC6D")]
        public async Task FieldServiceMethods_ShouldReturnTheSameResultForWebApiFieldQueryAndKepler()
        {
            await CompareServiceContract<IFieldService, FieldQuery>(null, new[] { "RetrieveAllMappableAsync", "RetrievePotentialBeginBatesFieldsAsync", "IsFieldIndexedAsync" });
        }

        [IdentifiedTest("52570DB9-E2A4-4568-A7AF-ED641045DC6D")]
        public async Task FieldServiceMethods_ShouldReturnTheSameResultForWebApiFieldManagerAndKepler()
        {
            await CompareServiceContract<IFieldService, FieldManager>(null, new[] { "ReadAsync" });
        }

        [IdentifiedTest("DDB0CBE8-BEA0-4E3C-BB5A-EAA0CD7C8B10")]
        public async Task FileIOServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IFileIOService, FileIO>((methodName, jsonResult) =>
            {
                if (methodName.Contains("GetDefaultRepositorySpaceReport") ||
                    methodName.Contains("GetBcpShareSpaceReport"))
                {
                    // Ignore this columns because it is related to the test environment (e.g.: free disk space) and may be different between Kepler and WebApi executions
                    var columnsToIgnore = new[] { "Free Space", "Used Space", "Total Space", "%Free", "%Used" };

                    var currentResult = JsonConvert.DeserializeObject<string[][]>(jsonResult);
                    foreach (var item in currentResult)
                    {
                        if (columnsToIgnore.Contains(item[0]))
                        {
                            item[1] = string.Empty;
                        }
                    }

                    return JsonConvert.SerializeObject(currentResult);
                }

                return jsonResult;
            });
        }

        [IdentifiedTest("EF1408F2-5C6E-4CD4-8DE8-187943CB1997")]
        public async Task FolderServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IFolderService, FolderManager>();
        }

        [IdentifiedTest("CDF6DD1A-1EA2-4A19-87E5-837EA220B7E9")]
        public async Task ObjectServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IObjectService, ObjectManager>();
        }

        [IdentifiedTest("3B0F9961-F0B4-4C9E-8093-2881B773ADF0")]
        public async Task ObjectTypeServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IObjectTypeService, ObjectTypeManager>();
        }

        [IdentifiedTest("506CCBBE-6E7E-44A4-82F2-E5E8DB3CEE2B")]
        public async Task ProductionServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IProductionService, ProductionManager>();
        }

        [IdentifiedTest("E10FCD24-E617-46B7-BED3-BDCB3D18699D")]
        public async Task RelativityServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IRelativityService, RelativityManager>();
        }

        [IdentifiedTest("A57E1612-3729-431E-B5CF-4EE6D6507B27")]
        public async Task SearchServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<ISearchService, SearchManager>();
        }

        [IdentifiedTest("1BC11427-732B-4815-A67D-8D2FFC0597AC")]
        public async Task UserServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            await CompareServiceContract<IUserService, UserManager>();
        }

        private async Task CompareServiceContract<TKeplerService, TWebApiManager>(Func<string, string, string> adjustMethodResultBeforeCompare = null, string[] keplerMethodsToCompare = null) where TKeplerService : IDisposable where TWebApiManager : IDisposable
        {
            var serviceMethodComparisonResults = new List<ServiceMethodComparisonResult>();

            LogInfo($"Start contract comparison test for {typeof(TKeplerService).Name} and {typeof(TWebApiManager).Name}");

            if (keplerMethodsToCompare == null)
            {
                keplerMethodsToCompare = GetNonDeprecatedMethods<TKeplerService>();
            }
            
            LogInfo($"Kepler service methods to be compared: {string.Join(", ", keplerMethodsToCompare)}");

            foreach (var keplerMethodName in keplerMethodsToCompare)
            {
	            var webApiMethodName = ConvertKeplerMethodName2WebApiMethodName(keplerMethodName);
                
                var keplerMethodParameters = await GetKeplerMethodParameters<TKeplerService>(keplerMethodName);
	            var webApiMethodParameters = GetWebApiMethodParameters<TWebApiManager>(webApiMethodName, keplerMethodParameters);

                var methodComparisonResult = new ServiceMethodComparisonResult
                {
                    KeplerMethodExecutionInfo = await ExecuteKeplerServiceMethod<TKeplerService>(keplerMethodName, keplerMethodParameters, adjustMethodResultBeforeCompare),
                    WebApiMethodExecutionInfo = await ExecuteWebApiServiceMethod<TWebApiManager>(webApiMethodName, webApiMethodParameters, adjustMethodResultBeforeCompare)
                };

                DisplayServiceMethodComparisonResult(methodComparisonResult);
                serviceMethodComparisonResults.Add(methodComparisonResult);
            }

            LogInfo($"Test summary for {typeof(TKeplerService).Name} and {typeof(TWebApiManager).Name} :");
            LogInfo($"Comparison passed for {serviceMethodComparisonResults.Count(r => r.IsValid)} method(s)");
            LogInfo($"Comparison failed for {serviceMethodComparisonResults.Count(r => !r.IsValid)} method(s)");

            Assert.IsTrue(serviceMethodComparisonResults.All(r => r.IsValid));
        }

        private static string[] GetNonDeprecatedMethods<T>()
        {
            return typeof(T).GetMethods()
                .Where(m => m.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Select(m => m.Name)
                .ToArray();
        }

        private static string ConvertKeplerMethodName2WebApiMethodName(string keplerMethodName)
        {
            var webApiMethodName = keplerMethodName.Replace("Async", "");

            if (webApiMethodName == "RetrieveInitialChunk")
            {
                return "RetrieveIntitialChunk";
            }

            return webApiMethodName;
        }

        private async Task<object[]> GetKeplerMethodParameters<TKeplerService>(string keplerMethodName)
        {
	        var keplerServiceType = typeof(TKeplerService);
	        var keplerMethod = keplerServiceType.GetMethod(keplerMethodName);
	        return await PopulateMethodParameters(keplerMethod);
        }

        private object[] GetWebApiMethodParameters<TWebApiManager>(string webApiMethodName, object[] keplerMethodParameters)
        {
	        var webApiServiceType = typeof(TWebApiManager);
	        var webApiMethod = webApiServiceType.GetMethod(webApiMethodName);
	        var webApiParameters = webApiMethod?.GetParameters();

	        var webApiParameterValues = new List<object>();
	        if (webApiParameters == null || webApiParameters.Length == 0)
	        {
		        return webApiParameterValues.ToArray();
	        }

	        for (var i = 0; i < webApiParameters.Length; i++)
	        {
		        var keplerParameterValue = keplerMethodParameters[i];
		        webApiParameterValues.Add(MapKeplerValueToWebApiValue(keplerParameterValue, webApiParameters[i].ParameterType));
	        }

	        return webApiParameterValues.ToArray();
        }

        private object MapKeplerValueToWebApiValue(object sourceValue, Type destinationType)
        {
	        var isWebApiType = destinationType.FullName != null && destinationType.FullName.Contains("kCura.EDDS.WebAPI");
	        if (!isWebApiType)
	        {
		        return sourceValue;
	        }

	        var mapper = new Mapper();
	        return mapper.Map(sourceValue, sourceValue.GetType(), destinationType);
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteKeplerServiceMethod<T>(string methodName, object[] methodParameters, Func<string, string, string> adjustMethodResultBeforeCompare) where T: IDisposable
        {
            LogInfo($"Execute Kepler service method: {typeof(T).Name}.{methodName}");

            return await ExecuteServiceMethod<T>(methodName, methodParameters, async () =>
            {
                object result = null;

                await KeplerServiceWrapper.PerformDataRequest<T>(async service =>
                {
                    var task = (Task)typeof(T).GetMethod(methodName).Invoke(service, methodParameters);
                    await task;
                    result = task.GetType().GetProperty("Result")?.GetValue(task);
                });

                if (result is DataSetWrapper dataSetWrapper)
                {
                    result = dataSetWrapper.Unwrap();
                }
                
                return result;
            }, adjustMethodResultBeforeCompare);
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteWebApiServiceMethod<T>(string methodName, object[] methodParameters, Func<string, string, string> adjustMethodResultBeforeCompare) where T : IDisposable
        {
            LogInfo($"Execute WebApi service method: {typeof(T).Name}.{methodName}");

            return await ExecuteServiceMethod<T>(methodName, methodParameters, () =>
            {
                object result = null;

                WebApiServiceWrapper.PerformDataRequest<T>(manager =>
                {
                    result = typeof(T).GetMethod(methodName).Invoke(manager, methodParameters);
                });

                return Task.FromResult(result);
            }, adjustMethodResultBeforeCompare);
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteServiceMethod<T>(string methodName, object[] methodParameters, Func<Task<object>> action, Func<string, string, string> adjustMethodResultBeforeCompare) where T : IDisposable
        {
            var methodExecutionInfo = new ServiceMethodExecutionInfo
            {
                MethodName = $"{typeof(T).Name}.{methodName}",
                InputParameters = methodParameters
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var result = await action() ?? new { };

                if (result is DataSet dataSet)
                {
                    SortDataSetByAllColumns(dataSet);
                }
                
                var jsonResult = JsonConvert.SerializeObject(result);

                if (adjustMethodResultBeforeCompare != null)
                {
                    jsonResult = adjustMethodResultBeforeCompare.Invoke(methodName, jsonResult);
                }

                // remove line breaks etc.
                methodExecutionInfo.SuccessResult = CleanupText(jsonResult);
            }
            catch (Exception ex)
            {
                // It's difficult to prepare test environment to retrieve data for all services and methods.
                // Sometimes we get error but even though we must ensure that error is the same for both Kepler and WebApi endpoints if we pass the same input parameters.
                methodExecutionInfo.ErrorMessage = HandleServiceMethodException(ex);
            }

            methodExecutionInfo.ExecutionTime = stopwatch.Elapsed;

            return methodExecutionInfo;
        }

        private string HandleServiceMethodException(Exception ex)
        {
            string normalizedErrorMessage = null;

            // Kepler exception
            if (ex is ServiceException keplerException)
            {
                // example error message:
                // Error during call BulkImportImageAsync. InnerExceptionType: Relativity.Core.Exception.InsufficientAccessControlListPermissions, InnerExceptionMessage: Insufficient Permissions! Please ask your Relativity Administrator to allow you import permission.
                const string exceptionMessageKey = "InnerExceptionMessage:";
                var exceptionMessageIndex = keplerException.Message.IndexOf(exceptionMessageKey, StringComparison.OrdinalIgnoreCase);
                normalizedErrorMessage = exceptionMessageIndex == -1 ? keplerException.Message : keplerException.Message.Substring(exceptionMessageIndex + exceptionMessageKey.Length).Trim();
            }

            // WebApi returns "real" error message hidden in InnerException
            if (ex.Message.Contains("Exception has been thrown by the target of an invocation") && ex.InnerException?.Message != null)
            {
                normalizedErrorMessage = ex.InnerException.Message;
                if (normalizedErrorMessage.Contains("Server was unable to process request"))
                {
                    normalizedErrorMessage = normalizedErrorMessage.Replace("Server was unable to process request. ---> ", "");
                }
            }

            // remove line breaks etc.
            return CleanupText(normalizedErrorMessage);
        }

        private async Task<object[]> PopulateMethodParameters(MethodInfo method)
        {
	        var parameters = new List<object>();

	        var methodParameters = method.GetParameters();
	        foreach (var parameter in methodParameters)
	        {
		        switch (parameter.Name)
		        {
			        case "workspaceID":
			        case "caseArtifactID":
			        case "caseContextArtifactID":
			        case "appID":
				        parameters.Add(await GetTestWorkspaceId());
				        continue;
			        case "correlationID":
				        parameters.Add("TestCorrelationId");
				        continue;
			        default:
				        parameters.Add(GetDefaultValue(parameter.ParameterType));
				        break;
		        }
	        }

	        return parameters.ToArray();
        }

        private static object GetDefaultValue(Type t)
        {
            try
            {
                if (t == typeof(string))
                {
                    return "test";
                }

                return Activator.CreateInstance(t);
            }
            catch (Exception ex)
            {
                LogInfo($"Error when creating default instance for {t.Name}: {ex.Message}");
                return null;
            }
        } 

        private static void SortDataSetByAllColumns(DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
            {
                return;
            }

            var dataTable = dataSet.Tables[0];
            if (dataTable.Columns.Count == 0)
            {
                return;
            }

            var sortExpression = new StringBuilder();
            foreach (DataColumn column in dataTable.Columns)
            {
                sortExpression.Append($"{column.ColumnName} ASC,");
            }

            dataTable.DefaultView.Sort = sortExpression.Remove(sortExpression.Length - 1, 1).ToString();
        }

        private static void LogInfo(string logMessage)
        {
            TestContext.WriteLine($"==> {logMessage}");
        }

        private static void DisplayServiceMethodComparisonResult(ServiceMethodComparisonResult methodComparisonResult)
        {
            var sb = new StringBuilder();

            sb.Append($"==> {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} vs {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} : ");
            sb.Append(methodComparisonResult.IsValid ? "OK" : "FAILURE");
            sb.AppendLine();

            sb.AppendLine($"  > {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} method details:");
            sb.AppendLine($"    InputParameters: {string.Join(",", methodComparisonResult.KeplerMethodExecutionInfo.InputParameters)}");
            sb.AppendLine($"    SuccessResult: {TrimTextIfTestValid(methodComparisonResult.IsValid, methodComparisonResult.KeplerMethodExecutionInfo.SuccessResult)}");
            sb.AppendLine($"    ErrorMessage: {TrimTextIfTestValid(methodComparisonResult.IsValid, methodComparisonResult.KeplerMethodExecutionInfo.ErrorMessage)}");
            sb.AppendLine($"    ExecutionTime: {methodComparisonResult.KeplerMethodExecutionInfo.ExecutionTime}");

            sb.AppendLine($"  > {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} method details:");
            sb.AppendLine($"    InputParameters: {string.Join(",", methodComparisonResult.WebApiMethodExecutionInfo.InputParameters)}");
            sb.AppendLine($"    SuccessResult: {TrimTextIfTestValid(methodComparisonResult.IsValid, methodComparisonResult.WebApiMethodExecutionInfo.SuccessResult)}");
            sb.AppendLine($"    ErrorMessage: {TrimTextIfTestValid(methodComparisonResult.IsValid, methodComparisonResult.WebApiMethodExecutionInfo.ErrorMessage)}");
            sb.AppendLine($"    ExecutionTime: {methodComparisonResult.WebApiMethodExecutionInfo.ExecutionTime}");

            TestContext.WriteLine(sb.ToString());
        }

        private static string TrimTextIfTestValid(bool isValid, string inputText)
        {
            if (string.IsNullOrEmpty(inputText) || !isValid)
            {
                return inputText;
            }

            var trimLength = Math.Min(inputText.Length, 1000);
            return inputText.Substring(0, trimLength);
        }

        private static string CleanupText(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            return inputText
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\\r", "")
                .Replace("\\n", "");
        }
    }
}
