using System.Data;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.CaseManagerBase;
using Newtonsoft.Json;
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
        public async Task RetrieveAllEnabled_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            DataSet webApiResult = null;
            DataSetWrapper keplerResult = null;

            WebApiServiceWrapper.PerformDataRequest<CaseManager>(caseManager =>
            {
                webApiResult = caseManager.RetrieveAllEnabled();
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerResult = await service.RetrieveAllEnabledAsync(string.Empty);
            });

            // assert
            Assert.AreEqual(JsonConvert.SerializeObject(webApiResult), JsonConvert.SerializeObject(keplerResult.Unwrap()));
        }

        [IdentifiedTest("EE590619-6890-4872-97A8-09FE90D789E3")]
        public async Task GetAllDocumentFolderPaths_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            string[] webApiResult = null;
            string[] keplerResult = null;

            WebApiServiceWrapper.PerformDataRequest<CaseManager>(caseManager =>
            {
                webApiResult = caseManager.GetAllDocumentFolderPaths();
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerResult = await service.GetAllDocumentFolderPathsAsync(string.Empty);
            });

            // assert
            CollectionAssert.AreEqual(webApiResult, keplerResult);
        }

        [IdentifiedTest("9B442373-20E4-4F86-B477-327EDEBD66EB")]
        public async Task GetAllDocumentFolderPathsForCase_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            var workspaceId = await GetTestWorkspaceId();
            
            string[] webApiResult = null;
            string[] keplerResult = null;

            WebApiServiceWrapper.PerformDataRequest<CaseManager>(caseManager =>
            {
                webApiResult = caseManager.GetAllDocumentFolderPathsForCase(workspaceId);
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerResult = await service.GetAllDocumentFolderPathsForCaseAsync(workspaceId, string.Empty);
            });

            // assert
            CollectionAssert.AreEqual(webApiResult, keplerResult);
        }

        [IdentifiedTest("EF71DF51-3746-485F-A33F-8B68042954C1")]
        public async Task Read_ShouldReturnTheSameResultForWebApiAndKepler()
        {
            var workspaceId = await GetTestWorkspaceId();

            kCura.EDDS.WebAPI.CaseManagerBase.CaseInfo webApiResult = null;
            DataTransfer.Legacy.SDK.ImportExport.V1.Models.CaseInfo keplerResult = null;

            WebApiServiceWrapper.PerformDataRequest<CaseManager>(caseManager =>
            {
                webApiResult = caseManager.Read(workspaceId);
            });

            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                keplerResult = await service.ReadAsync(workspaceId, string.Empty);
            });

            // assert
            Assert.AreEqual(JsonConvert.SerializeObject(webApiResult), JsonConvert.SerializeObject(keplerResult));
        }
    }
}
