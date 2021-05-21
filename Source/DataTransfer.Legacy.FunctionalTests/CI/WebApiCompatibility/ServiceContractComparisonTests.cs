using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.CaseManagerBase;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Tests.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility
{
    [TestFixture]
    [TestExecutionCategory.CI]
    [TestLevel.L3]
    public class ServiceContractComparisonTests : BaseServiceCompatibilityTest
    {
        private readonly RandomObjectGenerator _randomObjectGenerator;

        public ServiceContractComparisonTests()
        {
            _randomObjectGenerator = new RandomObjectGenerator();
        }

        [IdentifiedTest("99983EF4-7DB6-4941-8AF8-C6AF6CD64104")]
        public async Task CaseServiceMethods_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            var serviceContractComparisonResult = await CompareServiceMethods<ICaseService, CaseManager>();
            Assert.IsTrue(serviceContractComparisonResult.IsValid);
        }

        private async Task<ServiceContractComparisonResult> CompareServiceMethods<TKeplerService, TWebApiManager>() where TKeplerService : IDisposable where TWebApiManager : IDisposable
        {
            var serviceContractComparisonResult = new ServiceContractComparisonResult();

            var keplerMethodsToCompare = typeof(TKeplerService).GetMethods().Select(m => m.Name);
            foreach (var keplerMethodName in keplerMethodsToCompare)
            {
                var methodComparisonResult = new ServiceMethodComparisonResult();
                serviceContractComparisonResult.MethodComparisonResults.Add(methodComparisonResult);

                // execute Kepler method
                methodComparisonResult.KeplerMethodExecutionInfo = await ExecuteKeplerServiceMethod<TKeplerService>(keplerMethodName);

                // execute WebApi method
                var webApiMethodName = keplerMethodName.Replace("Async", "");
                methodComparisonResult.WebApiMethodExecutionInfo = await ExecuteWebApiServiceMethod<TWebApiManager>(webApiMethodName);
            }

            return serviceContractComparisonResult;
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

                if (result is DataSetWrapper dataSetWrapper)
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
            var method = typeof(T).GetMethod(methodName);
            var parameters = await PopulateMethodParameters(method);

            var methodExecutionInfo = new ServiceMethodExecutionInfo
            {
                MethodName = methodName,
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
                methodExecutionInfo.ExecutionResult = $"ErrorMessage:{ex.Message} | InnerErrorMessage:{ex.InnerException?.Message}";
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
    }
}
