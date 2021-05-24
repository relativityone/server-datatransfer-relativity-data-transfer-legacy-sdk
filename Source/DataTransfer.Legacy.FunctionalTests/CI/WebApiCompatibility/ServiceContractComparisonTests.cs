using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.CaseManagerBase;
using kCura.EDDS.WebAPI.CodeManagerBase;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility
{
    [TestFixture]
    [TestExecutionCategory.CI]
    [TestLevel.L3]
    public class ServiceContractComparisonTests : BaseServiceCompatibilityTest
    {
        private readonly WebApiRandomObjectGenerator _randomObjectGenerator;

        public ServiceContractComparisonTests()
        {
            _randomObjectGenerator = new WebApiRandomObjectGenerator();
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

        private async Task CompareServiceContract<TKeplerService, TWebApiManager>() where TKeplerService : IDisposable where TWebApiManager : IDisposable
        {
            var serviceMethodComparisonResults = new List<ServiceMethodComparisonResult>();

            var keplerMethodsToCompare = typeof(TKeplerService).GetMethods().Select(m => m.Name);
            foreach (var keplerMethodName in keplerMethodsToCompare)
            {
                var methodComparisonResult = new ServiceMethodComparisonResult
                {
                    KeplerMethodExecutionInfo = await ExecuteKeplerServiceMethod<TKeplerService>(keplerMethodName),
                    WebApiMethodExecutionInfo = await ExecuteWebApiServiceMethod<TWebApiManager>(keplerMethodName.Replace("Async", ""))
                };

                serviceMethodComparisonResults.Add(methodComparisonResult);
            }

            DisplayServiceMethodComparisonResults(serviceMethodComparisonResults);
            Assert.IsTrue(serviceMethodComparisonResults.All(r => r.IsValid));
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteKeplerServiceMethod<T>(string methodName) where T: IDisposable
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

                if (result is DataSetWrapper dataSetWrapper
                )
                {
                    return dataSetWrapper.Unwrap();
                }

                return result;
            });
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteWebApiServiceMethod<T>(string methodName) where T : IDisposable
        {
            return await ExecuteServiceMethod<T>(methodName, (method, parameters) =>
            {
                object result = null;

                WebApiServiceWrapper.PerformDataRequest<T>(manager =>
                {
                    result = method.Invoke(manager, parameters);
                });

                return Task.FromResult(result);
            });
        }

        private async Task<ServiceMethodExecutionInfo> ExecuteServiceMethod<T>(string methodName, Func<MethodInfo, object[], Task<object>> action) where T : IDisposable
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
                var result = await action(method, parameters);
                methodExecutionInfo.ExecutionResult = JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                // It's difficult to prepare test environment to retrieve all data.
                // Sometimes we get error but even though we must ensure that error is the same for both Kepler and WebApi endpoints.

                // WebApi returns "real" error message in InnerException
                if (ex.Message.Contains("Exception has been thrown by the target of an invocation") && ex.InnerException?.Message != null)
                {
                    methodExecutionInfo.ExecutionResult = ex.InnerException.Message;
                }
                else
                // Kepler
                {
                    methodExecutionInfo.ExecutionResult = ex.Message;
                }

                methodExecutionInfo.ExecutionResult = methodExecutionInfo.ExecutionResult.Replace("\r", "").Replace("\n", "");
            }

            methodExecutionInfo.ExecutionTime = stopwatch.Elapsed;

            return methodExecutionInfo;
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
                        parameterValues.Add(await GetTestWorkspaceId());
                        continue;
                    case "correlationID":
                        parameterValues.Add("TestCorrelationId");
                        continue;
                    default:
                        parameterValues.Add(_randomObjectGenerator.Generate(parameter.ParameterType));
                        break;
                }
            }

            return parameterValues.ToArray();
        }

        private static void DisplayServiceMethodComparisonResults(List<ServiceMethodComparisonResult> serviceMethodComparisonResults)
        {
            var sb = new StringBuilder();

            foreach (var methodComparisonResult in serviceMethodComparisonResults)
            {
                sb.AppendLine($"==> {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} vs {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} : " + (methodComparisonResult.IsValid ? "OK" : "FAILED"));

                if (methodComparisonResult.IsValid)
                {
                    sb.AppendLine($"  > {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} execution time: {methodComparisonResult.KeplerMethodExecutionInfo.ExecutionTime}");
                    sb.AppendLine($"  > {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} execution time: {methodComparisonResult.WebApiMethodExecutionInfo.ExecutionTime}");
                }

                if (!methodComparisonResult.IsValid)
                {
                    sb.AppendLine($"  > {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} input parameters: {string.Join(",", methodComparisonResult.KeplerMethodExecutionInfo.InputParameters)}");
                    sb.AppendLine($"  > {methodComparisonResult.KeplerMethodExecutionInfo.MethodName} execution result: {methodComparisonResult.KeplerMethodExecutionInfo.ExecutionResult}");
                    sb.AppendLine($"  > {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} input parameters: {string.Join(",", methodComparisonResult.WebApiMethodExecutionInfo.InputParameters)}");
                    sb.AppendLine($"  > {methodComparisonResult.WebApiMethodExecutionInfo.MethodName} execution result: {methodComparisonResult.WebApiMethodExecutionInfo.ExecutionResult}");
                    sb.AppendLine();
                }
            }

            TestContext.WriteLine(sb.ToString());
        }
    }
}
