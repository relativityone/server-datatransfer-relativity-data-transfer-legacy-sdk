using System;
using System.Threading.Tasks;
using kCura.Utility;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class IAPICommunicationModeService : BaseService, IIAPICommunicationModeService
	{
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;

		public IAPICommunicationModeService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory, IInstanceSettingsBundle instanceSettingsBundle)
			: base(methodRunner, serviceContextFactory)
		{
			_instanceSettingsBundle = instanceSettingsBundle;
		}

		public Task<IAPICommunicationMode> GetIAPICommunicationModeAsync(string correlationId)
		{
			return ExecuteAsync(async () => await GetIAPICommunicationModeAsync(), null, correlationId);
		}

		private async Task<IAPICommunicationMode> GetIAPICommunicationModeAsync()
		{
			try
			{
				var mode = await _instanceSettingsBundle.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode").ConfigureAwait(false);
				if (mode.Equals(IAPICommunicationMode.WebAPI.GetDescription(), StringComparison.InvariantCultureIgnoreCase))
				{
					return IAPICommunicationMode.WebAPI;
				}

				if (mode.Equals(IAPICommunicationMode.Kepler.GetDescription(), StringComparison.InvariantCultureIgnoreCase))
				{
					return IAPICommunicationMode.Kepler;
				}

				if (mode.Equals(IAPICommunicationMode.ForceWebAPI.GetDescription(), StringComparison.InvariantCultureIgnoreCase))
				{
					return IAPICommunicationMode.ForceWebAPI;
				}

				if (mode.Equals(IAPICommunicationMode.ForceKepler.GetDescription(), StringComparison.InvariantCultureIgnoreCase))
				{
					return IAPICommunicationMode.ForceKepler;
				}

				return IAPICommunicationMode.WebAPI;
			}
			catch
			{
				return IAPICommunicationMode.WebAPI;
			}
		}
	}
}
