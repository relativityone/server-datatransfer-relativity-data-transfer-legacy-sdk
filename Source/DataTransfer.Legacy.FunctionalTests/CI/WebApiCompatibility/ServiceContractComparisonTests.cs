﻿using System;
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

        private async Task CompareServiceContract<TKeplerService, TWebApiManager>(Func<string, string, string> adjustMethodResultBeforeCompare = null, IEnumerable<string> keplerMethodsToCompare = null) where TKeplerService : IDisposable where TWebApiManager : IDisposable
        {
            var serviceMethodComparisonResults = new List<ServiceMethodComparisonResult>();

            if (keplerMethodsToCompare == null)
            {
                keplerMethodsToCompare = GetNonDeprecatedMethods<TKeplerService>();
            }
            
            foreach (var keplerMethodName in keplerMethodsToCompare)
            {
                var methodComparisonResult = new ServiceMethodComparisonResult
                {
                    KeplerMethodExecutionInfo = await ExecuteKeplerServiceMethod<TKeplerService>(keplerMethodName, adjustMethodResultBeforeCompare),
                    WebApiMethodExecutionInfo = await ExecuteWebApiServiceMethod<TWebApiManager>(ConvertKeplerMethodName2WebApiMethodName(keplerMethodName), adjustMethodResultBeforeCompare)
                };

                serviceMethodComparisonResults.Add(methodComparisonResult);
            }

            DisplayServiceMethodComparisonResults(serviceMethodComparisonResults);
            Assert.IsTrue(serviceMethodComparisonResults.All(r => r.IsValid));
        }

        private static IEnumerable<string> GetNonDeprecatedMethods<T>()
        {
            return typeof(T).GetMethods()
                .Where(m => m.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Select(m => m.Name);
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

        private async Task<ServiceMethodExecutionInfo> ExecuteKeplerServiceMethod<T>(string methodName, Func<string, string, string> adjustMethodResultBeforeCompare) where T: IDisposable
        {
            return await ExecuteServiceMethod<T>(methodName, async (method, parameters) =>
            {
                object result = null;

                await KeplerServiceWrapper.PerformDataRequest<T>(async service =>
                {
                    var task = (Task)method.Invoke(service, parameters);
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

        private async Task<ServiceMethodExecutionInfo> ExecuteWebApiServiceMethod<T>(string methodName, Func<string, string, string> adjustMethodResultBeforeCompare) where T : IDisposable
        {
            return await ExecuteServiceMethod<T>(methodName, (method, parameters) =>
            {
                object result = null;

                WebApiServiceWrapper.PerformDataRequest<T>(manager =>
                {
                    result = method.Invoke(manager, parameters);
                });

                return Task.FromResult(result);
            }, adjustMethodResultBeforeCompare);
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteServiceMethod<T>(string methodName, Func<MethodInfo, object[], Task<object>> action, Func<string, string, string> adjustMethodResultBeforeCompare) where T : IDisposable
        {
            var serviceType = typeof(T);
            var method = serviceType.GetMethod(methodName);
            var parameters = await PopulateMethodParameters(method);

            var methodExecutionInfo = new ServiceMethodExecutionInfo
            {
                MethodName = $"{serviceType.Name}.{methodName}",
                InputParameters = parameters
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var result = await action(method, parameters) ?? new { };

                if (result is DataSet dataSet)
                {
                    SortDataSetByAllColumns(dataSet);
                }
                
                var jsonResult = JsonConvert.SerializeObject(result);

                if (adjustMethodResultBeforeCompare != null)
                {
                    jsonResult = adjustMethodResultBeforeCompare.Invoke(methodName, jsonResult);
                }

                methodExecutionInfo.SuccessResult = jsonResult;
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
                normalizedErrorMessage = keplerException.ErrorDetails.Message;
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
            return normalizedErrorMessage?.Replace("\r", "").Replace("\n", "");
        }

        private async Task<object[]> PopulateMethodParameters(MethodInfo method)
        {
            var parameterValues = new List<object>();

            var methodParameters = method.GetParameters();
            foreach (var parameter in methodParameters)
            {
                switch (parameter.Name)
                {
                    case "workspaceID":
                    case "caseArtifactID":
                    case "caseContextArtifactID":
                    case "appID":
                        parameterValues.Add(await GetTestWorkspaceId());
                        continue;
                    case "correlationID":
                        parameterValues.Add("TestCorrelationId");
                        continue;
                    default:
                        parameterValues.Add(GetDefaultValue(parameter.ParameterType));
                        break;
                }
            }

            return parameterValues.ToArray();
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            if (t == typeof(int[]))
            {
                return new int[] { };
            }

            return null;

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

        private static void DisplayServiceMethodComparisonResults(List<ServiceMethodComparisonResult> serviceMethodComparisonResults)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Comparison passed for {serviceMethodComparisonResults.Count(r => r.IsValid)} method(s).");
            sb.AppendLine($"Comparison failed for {serviceMethodComparisonResults.Count(r => !r.IsValid)} method(s).");

            foreach (var methodComparisonResult in serviceMethodComparisonResults)
            {
                sb.Append($"==> {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} vs {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} : ");

                if (methodComparisonResult.IsValid)
                {
                    if (!string.IsNullOrEmpty(methodComparisonResult.KeplerMethodExecutionInfo.SuccessResult) &&
                        !string.IsNullOrEmpty(methodComparisonResult.WebApiMethodExecutionInfo.SuccessResult))
                    {
                        sb.Append("OK (same result)");
                    }

                    if (!string.IsNullOrEmpty(methodComparisonResult.KeplerMethodExecutionInfo.ErrorMessage) &&
                        !string.IsNullOrEmpty(methodComparisonResult.WebApiMethodExecutionInfo.ErrorMessage))
                    {
                        sb.Append("OK (same error)");
                        sb.AppendLine();
                        sb.Append($"    Execution error: {methodComparisonResult.KeplerMethodExecutionInfo.ErrorMessage}");
                    }
                }
                else
                {
                    sb.Append("FAILURE");
                }

                sb.AppendLine();
                
                if (methodComparisonResult.IsValid)
                {
                    sb.AppendLine($"    ExecutionTime for {methodComparisonResult.KeplerMethodExecutionInfo.MethodName}: {methodComparisonResult.KeplerMethodExecutionInfo.ExecutionTime}");
                    sb.AppendLine($"    ExecutionTime for {methodComparisonResult.WebApiMethodExecutionInfo.MethodName}: {methodComparisonResult.WebApiMethodExecutionInfo.ExecutionTime}");
                }

                if (!methodComparisonResult.IsValid)
                {
                    sb.AppendLine($"  > {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} method details:");
                    sb.AppendLine($"    InputParameters: {string.Join(",", methodComparisonResult.KeplerMethodExecutionInfo.InputParameters)}");
                    sb.AppendLine($"    SuccessResult: {methodComparisonResult.KeplerMethodExecutionInfo.SuccessResult}");
                    sb.AppendLine($"    ErrorMessage: {methodComparisonResult.KeplerMethodExecutionInfo.ErrorMessage}");
                    sb.AppendLine($"    ExecutionTime: {methodComparisonResult.KeplerMethodExecutionInfo.ExecutionTime}");

                    sb.AppendLine($"  > {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} method details:");
                    sb.AppendLine($"    InputParameters: {string.Join(",", methodComparisonResult.WebApiMethodExecutionInfo.InputParameters)}");
                    sb.AppendLine($"    SuccessResult: {methodComparisonResult.WebApiMethodExecutionInfo.SuccessResult}");
                    sb.AppendLine($"    ErrorMessage: {methodComparisonResult.WebApiMethodExecutionInfo.ErrorMessage}");
                    sb.AppendLine($"    ExecutionTime: {methodComparisonResult.WebApiMethodExecutionInfo.ExecutionTime}");
                }
            }

            TestContext.WriteLine(sb.ToString());
        }
    }
}
