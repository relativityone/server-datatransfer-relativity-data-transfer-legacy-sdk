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

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class RelativityService : BaseService, IRelativityService
	{
		public RelativityService(IServiceContextFactory serviceContextFactory) 
			: base(serviceContextFactory)
		{
		}

		public Task<string> RetrieveCurrencySymbolAsync(string correlationID)
		{
			var result = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
			return Task.FromResult(result);
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