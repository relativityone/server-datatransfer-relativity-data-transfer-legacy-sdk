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

				throw new NotFoundException(Constants.ErrorMessages.RdcDeprecatedDisplayMessage);
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

		public async Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID)
		{
			var isRdcDisabled =
				await _toggleProvider.IsEnabledAsync<DisableRdcAndImportApiToggle>();

			if (isRdcDisabled)
			{
				_logger.LogWarning("RDC and Import API have been have been deprecated in this RelativityOne instance.");

				throw new NotFoundException(Constants.ErrorMessages.RdcDeprecatedDisplayMessage);
			}

			var result = WebAPIHelper.RetrieveRdcConfiguration(GetBaseServiceContext(AdminWorkspace));
			return result != null ? new DataSetWrapper(result.ToDataSet()) : null;
		}

		public Task<string> GetRelativityUrlAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<string> RetrieveCurrencySymbolV2Async(string correlationID)
		{
			var result = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
			return Task.FromResult(result);
		}
	}
}