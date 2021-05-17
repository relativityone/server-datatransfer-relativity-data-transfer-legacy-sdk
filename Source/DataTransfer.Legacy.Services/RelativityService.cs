using System;
using System.Globalization;
using System.Threading.Tasks;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class RelativityService : BaseService, IRelativityService
	{
		public RelativityService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
		}

		public async Task<string> RetrieveCurrencySymbolAsync(string correlationID)
		{
			return await ExecuteAsync(
				() => CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol,
				null, correlationID).ConfigureAwait(false);
		}

		public Task<string> GetImportExportWebApiVersionAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<bool> ValidateSuccessfulLoginAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public async Task<bool> IsImportEmailNotificationEnabledAsync(string correlationID)
		{
			return await ExecuteAsync(
				() => Config.SendNotificationOnImportCompletion,
				null, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID)
		{
			return await ExecuteAsync(
				() => WebAPIHelper.RetrieveRdcConfiguration(GetBaseServiceContext(AdminWorkspace)),
				null, correlationID).ConfigureAwait(false);
		}

		public Task<string> GetRelativityUrlAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}
	}
}