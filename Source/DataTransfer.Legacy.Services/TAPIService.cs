using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using IInstanceSettingsBundle = Relativity.API.IInstanceSettingsBundle;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	public class TAPIService : BaseService, ITAPIService
	{
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;
		private readonly ITraceGenerator _traceGenerator;

		private const string TapiMaxAllowedTargetDataRateMbpsSettingSection = "Relativity.DataTransfer";
		private const string TapiMaxAllowedTargetDataRateMbpsSettingName = "TapiMaxAllowedTargetDataRateMbps";

		public TAPIService(IServiceContextFactory serviceContextFactory, IInstanceSettingsBundle instanceSettingsBundle, ITraceGenerator traceGenerator)
			: base(serviceContextFactory)
		{
			_instanceSettingsBundle = instanceSettingsBundle;

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public async Task<TAPIConfiguration> RetrieveConfigurationAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.TAPI.RetrieveConfiguration", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				bool cloudInstance = Relativity.Core.Config.CloudInstance;
				uint? tapiMaxAllowedTargetDataRateMbps = await _instanceSettingsBundle
					.GetUIntAsync(TapiMaxAllowedTargetDataRateMbpsSettingSection,
						TapiMaxAllowedTargetDataRateMbpsSettingName).ConfigureAwait(false);

				return new TAPIConfiguration()
				{
					IsCloudInstance = cloudInstance,
					TapiMaxAllowedTargetDataRateMbps = tapiMaxAllowedTargetDataRateMbps
				};
			}
		}
    }
}
