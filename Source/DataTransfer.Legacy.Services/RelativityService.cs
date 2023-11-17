using System;
using System.Globalization;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services
{
	using Relativity.API;
	using Relativity.DataTransfer.Legacy.Services.Toggles;
	using Relativity.Services.Exceptions;

	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class RelativityService : BaseService, IRelativityService
	{
		private readonly IToggleProvider _toggleProvider;
		private readonly IAPILog _logger;

		public RelativityService(IServiceContextFactory serviceContextFactory, IToggleProvider toggleProvider, IAPILog logger) : base(serviceContextFactory)
		{
			_toggleProvider = toggleProvider;
			_logger = logger;
		}

		public async Task<string> RetrieveCurrencySymbolAsync(string correlationID)
		{
			var isRdcDisabled = await _toggleProvider.IsEnabledAsync<DisableRdcAndImportApiToggle>();

			if (isRdcDisabled)
			{
				_logger.LogWarning("RDC and Import API have been have been deprecated in this RelativityOne instance.");

				var importExportDocUrl =
					"https://help.relativity.com/RelativityOne/Content/Relativity/Import_Export/Import_Export_Overview.htm";
				var message =
					$"The Relativity Desktop Client (RDC) and Aspera Transfer Service have been deprecated in your RelativityOne instance and are no longer operational. Please use Import/Export for data transfers in RelativityOne. {importExportDocUrl}";
				throw new NotFoundException(message);

			}
			var result = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
			return result;
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
			var result = Config.SendNotificationOnImportCompletion;
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID)
		{

			var result = WebAPIHelper.RetrieveRdcConfiguration(GetBaseServiceContext(AdminWorkspace));
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}

		public Task<string> GetRelativityUrlAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}
	}
}