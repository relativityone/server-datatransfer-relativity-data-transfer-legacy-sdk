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
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	public class RelativityService : BaseService, IRelativityService
	{
		public RelativityService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
		}

		public Task<string> RetrieveCurrencySymbolAsync(string correlationID)
		{
			return ExecuteAsync(
				() => CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol,
				null, correlationID);
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
			return ExecuteAsync(
				() => Config.SendNotificationOnImportCompletion,
				null, correlationID);
		}

		public Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID)
		{
			return ExecuteAsync(
				() => WebAPIHelper.RetrieveRdcConfiguration(GetBaseServiceContext(AdminWorkspace)),
				null, correlationID);
		}

		public Task<string> GetRelativityUrlAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}
	}
}