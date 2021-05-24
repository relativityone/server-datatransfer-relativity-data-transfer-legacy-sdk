using AutoBogus;
using Relativity.DataTransfer.Legacy.Tests.Helpers;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
    public class WebApiRandomObjectGenerator : RandomObjectGenerator
    {
        public WebApiRandomObjectGenerator()
        {
            _generators.Add(typeof(kCura.EDDS.WebAPI.CodeManagerBase.Code), () => AutoFaker.Generate<kCura.EDDS.WebAPI.CodeManagerBase.Code>());
        }
    }
}
