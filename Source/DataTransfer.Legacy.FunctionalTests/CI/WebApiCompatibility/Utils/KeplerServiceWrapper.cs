using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using Relativity.Kepler.Exceptions;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
    public class KeplerServiceWrapper
    {
        private const int RetryPolicyMaxRetryCount = 5;
        private const int RetryPolicySleepDurationTimeSeconds = 5;

        public async Task PerformDataRequest<T>(Func<T, Task> action) where T : IDisposable
        {
            try
            {
                // Test Hopper is slow and sometimes we are getting ServiceNotFoundException so it's better to use retry policy for Kepler endpoints
                var keplerRetryPolicy = Policy
                    .Handle<ServiceNotFoundException>()
                    .WaitAndRetryAsync(
                        RetryPolicyMaxRetryCount,
                        i => TimeSpan.FromSeconds(RetryPolicySleepDurationTimeSeconds),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            TestContext.WriteLine($"Service connection failed. Retry policy triggered... Attempt #{retryCount}");
                        });

                var keplerServiceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
                await keplerRetryPolicy.ExecuteAsync(async () =>
                {
                    using (var keplerService = keplerServiceFactory.GetServiceProxy<T>())
                    {
                        await action(keplerService);
                    }
                });

            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Exception occurred when retrieving data from Kepler endpoint: {ex}");
                throw;
            }
        }
    }
}
