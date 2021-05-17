using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility
{
    public abstract class BaseServiceCompatibilityTest
    {
        protected KeplerServiceWrapper KeplerServiceWrapper;
        protected WebApiServiceWrapper WebApiServiceWrapper;

        protected BaseServiceCompatibilityTest()
        {
            KeplerServiceWrapper = new KeplerServiceWrapper();
            WebApiServiceWrapper = new WebApiServiceWrapper();
        }

        protected async Task<int> GetTestWorkspaceId()
        {
            DataSetWrapper workspaces = null;
            await KeplerServiceWrapper.PerformDataRequest<ICaseService>(async service =>
            {
                workspaces = await service.RetrieveAllEnabledAsync(string.Empty);
            });

            if (workspaces == null ||
                workspaces.Unwrap() == null ||
                workspaces.Unwrap().Tables.Count == 0 ||
                workspaces.Unwrap().Tables[0].Rows.Count == 0)
            {
                throw new NotFoundException("Test Workspace could not be found.");
            }

            var workspace = workspaces.Unwrap().Tables[0].Rows[0].ItemArray;
            return (int)workspace.GetValue(0);
        }
    }
}
