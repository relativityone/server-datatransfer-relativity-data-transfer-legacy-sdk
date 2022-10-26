using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Wrappers;
using Relativity.DataTransfer.Legacy.FunctionalTests.Helpers;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility
{
    public abstract class BaseServiceCompatibilityTest
    {
        protected KeplerServiceWrapper KeplerServiceWrapper;
        protected WebApiServiceWrapper WebApiServiceWrapper;
        protected TestWorkspace TestWorkspace { get; private set; }

        [OneTimeSetUp]
        public async Task OneTimeBaseSetupAsync()
        {
	        TestWorkspace = await AssemblySetup.TestWorkspaceAsync.ConfigureAwait(false);
        }

		protected BaseServiceCompatibilityTest()
        {
            KeplerServiceWrapper = new KeplerServiceWrapper();
            WebApiServiceWrapper = new WebApiServiceWrapper();
        }

        /// <summary>
        /// Get Workspace ID.
        /// </summary>
        /// <returns>Id of test workspace</returns>
        protected int GetTestWorkspaceId()
        {
	        return TestWorkspace.WorkspaceId;
        }
    }
}
