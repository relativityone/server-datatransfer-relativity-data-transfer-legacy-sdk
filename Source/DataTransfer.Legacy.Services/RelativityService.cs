using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class RelativityService : BaseService, IRelativityService
	{
		private readonly ITraceGenerator _traceGenerator;

		public RelativityService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<string> RetrieveCurrencySymbolAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Relativity.RetrieveCurrencySymbol", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
				return Task.FromResult(result);
			}
		}

		public Task<string> GetImportExportWebApiVersionAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<bool> ValidateSuccessfulLoginAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<bool> IsImportEmailNotificationEnabledAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Relativity.IsImportEmailNotificationEnabled", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = Config.SendNotificationOnImportCompletion;
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Relativity.RetrieveRdcConfiguration", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = WebAPIHelper.RetrieveRdcConfiguration(GetBaseServiceContext(AdminWorkspace));
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<string> GetRelativityUrlAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}
	}
}