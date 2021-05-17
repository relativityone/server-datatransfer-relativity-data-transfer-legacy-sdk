using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
    public class KeplerServiceWrapper
    {
        public async Task PerformDataRequest<T>(Func<T, Task> action) where T : IDisposable
        {
            try
            {
                var keplerServiceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
                using (var keplerService = keplerServiceFactory.GetServiceProxy<T>())
                {
                    await action(keplerService);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Exception occurred when retrieving data from Kepler endpoint: {ex}");
                throw;
            }
        }
    }
}
