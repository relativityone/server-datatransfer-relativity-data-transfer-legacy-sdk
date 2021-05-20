using AutoMapper;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.WebApiClients
{
    /// <summary>
    /// RelativityWebApi soap services return model from Relativity namespace (e.g.: Relativity.CaseInfo)
    /// Relativity.DataExchange.Client.SDK nuget package (which is client for soap services and which we use for tests) returns model from Relativity.DataExchange.Service
    /// We need to map to be consistent with our mapper used for Kepler services.
    /// </summary>
    public class WebApiResultMapper
    {
        private readonly IMapper _mapper;

        public WebApiResultMapper()
        {
            var config = new MapperConfiguration(ConfigureMapping);
            _mapper = config.CreateMapper();
        }

        public T Map<T>(object t1)
        {
            return _mapper.Map<T>(t1);
        }

        private static void ConfigureMapping(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<kCura.EDDS.WebAPI.CaseManagerBase.CaseInfo, Relativity.CaseInfo>();
        }
    }
}
