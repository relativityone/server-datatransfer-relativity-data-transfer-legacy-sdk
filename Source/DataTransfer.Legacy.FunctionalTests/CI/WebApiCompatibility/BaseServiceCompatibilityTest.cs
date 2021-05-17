using Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils;

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
    }
}
