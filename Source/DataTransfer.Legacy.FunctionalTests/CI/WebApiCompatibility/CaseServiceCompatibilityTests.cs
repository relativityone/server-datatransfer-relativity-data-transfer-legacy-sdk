using System.Data;
using System.Threading.Tasks;
using kCura.WinEDDS.Service;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility
{
    [TestFixture]
    [TestExecutionCategory.CI]
    [TestLevel.L3]
    public class CaseServiceCompatibilityTests : BaseServiceCompatibilityTest
    {
        [IdentifiedTest("F5312AF8-D1E1-4400-962E-43A77FE44922")]
        public async Task RetrieveAllEnabled_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            DataSet webApiData = null;
            DataSetWrapper keplerData = null;

            WebApiServiceWrapper.PerformDataRequest((credentials, cookieContainer) =>
            {
                var caseManager = new CaseManager(credentials, cookieContainer);
                webApiData = caseManager.RetrieveAllEnabled();
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service => 
            {
                 keplerData = await service.RetrieveAllEnabledAsync(string.Empty);
            });

            var id = GetTestWorkspaceId();

            // assert
            DataSetAssertHelper.AreEqual(webApiData, keplerData.Unwrap());
        }
    }
}
