using System.Data;
using System.Threading.Tasks;
using kCura.WinEDDS.Service;
using NUnit.Framework;
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
        public async Task ShouldGetApplicationAfterInstallation()
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

            // assert
            AssertDataSetsAreEqual(webApiData, keplerData.Unwrap(), "");
        }

        private void AssertDataSetsAreEqual(DataSet ds1, DataSet ds2, string idColumnName)
        {
            Assert.NotNull(ds1);
            Assert.NotNull(ds2);

            Assert.AreEqual(1, ds1.Tables.Count);
            Assert.AreEqual(1, ds2.Tables.Count);

            var dt1 = ds1.Tables[0];
            var dt2 = ds2.Tables[0];

            Assert.AreEqual(dt1.Columns.Count, dt2.Columns.Count);
            Assert.AreEqual(dt1.Rows.Count, dt2.Rows.Count);
            
            // TODO: compare data sets in a better way
        }
    }
}
