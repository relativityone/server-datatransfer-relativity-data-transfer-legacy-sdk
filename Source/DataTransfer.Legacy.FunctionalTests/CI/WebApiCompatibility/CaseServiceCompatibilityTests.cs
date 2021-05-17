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

            // assert
            DataSetAssertHelper.AreEqual(webApiData, keplerData.Unwrap());
        }

        [IdentifiedTest("EE590619-6890-4872-97A8-09FE90D789E3")]
        public async Task GetAllDocumentFolderPaths_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            string[] webApiData = null;
            string[] keplerData = null;

            WebApiServiceWrapper.PerformDataRequest((credentials, cookieContainer) =>
            {
                var caseManager = new CaseManager(credentials, cookieContainer);
                webApiData = caseManager.GetAllDocumentFolderPaths();
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerData = await service.GetAllDocumentFolderPathsAsync(string.Empty);
            });

            // assert
            CollectionAssert.AreEqual(webApiData, keplerData);
        }

        [IdentifiedTest("")]
        public async Task GetAllDocumentFolderPathsForCase_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            var workspaceId = await GetTestWorkspaceId();
            
            string[] webApiData = null;
            string[] keplerData = null;

            WebApiServiceWrapper.PerformDataRequest((credentials, cookieContainer) =>
            {
                var caseManager = new CaseManager(credentials, cookieContainer);
                webApiData = caseManager.GetAllDocumentFolderPathsForCase(workspaceId);
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerData = await service.GetAllDocumentFolderPathsForCaseAsync(workspaceId, string.Empty);
            });

            // assert
            CollectionAssert.AreEqual(webApiData, keplerData);
        }

        [IdentifiedTest("")]
        public async Task Read_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            var workspaceId = await GetTestWorkspaceId();

            Relativity.DataExchange.Service.CaseInfo webApiData = null;
            DataTransfer.Legacy.SDK.ImportExport.V1.Models.CaseInfo keplerData = null;

            WebApiServiceWrapper.PerformDataRequest((credentials, cookieContainer) =>
            {
                var caseManager = new CaseManager(credentials, cookieContainer);
                webApiData = caseManager.Read(workspaceId);
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerData = await service.ReadAsync(workspaceId, string.Empty);
            });

            // assert
            //CollectionAssert.AreEqual(webApiData, keplerData);
        }
    }
}
