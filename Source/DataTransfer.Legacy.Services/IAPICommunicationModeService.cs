using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]

	public class IAPICommunicationModeService : BaseService, IIAPICommunicationModeService
	{
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;
		private readonly IAPILog _logger;
		private const string IAPICommunicationModeSettingSection = "DataTransfer.Legacy";
		private const string IAPICommunicationModeSettingName = "IAPICommunicationMode";

		private readonly Dictionary<string, IAPICommunicationMode> _instanceSettingToCommunicationModeLookup =
			new Dictionary<string, IAPICommunicationMode>(StringComparer.InvariantCultureIgnoreCase)
			{
				{"WebAPI", IAPICommunicationMode.WebAPI},
				{"Kepler", IAPICommunicationMode.Kepler},
				{"ForceWebAPI", IAPICommunicationMode.ForceWebAPI},
				{"ForceKepler", IAPICommunicationMode.ForceKepler}
			};

		public IAPICommunicationModeService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory,
			IInstanceSettingsBundle instanceSettingsBundle, IAPILog logger)
			: base(methodRunner, serviceContextFactory)
		{
			_instanceSettingsBundle = instanceSettingsBundle;
			_logger = logger;
		}

		public Task<IAPICommunicationMode> GetIAPICommunicationModeAsync(string correlationId)
		{
			return ExecuteAsync(GetIAPICommunicationModeAsync, null, correlationId);
		}

		private async Task<IAPICommunicationMode> GetIAPICommunicationModeAsync()
		{
			try
			{
				var mode = await _instanceSettingsBundle.GetStringAsync(IAPICommunicationModeSettingSection, IAPICommunicationModeSettingName).ConfigureAwait(false);
				if (_instanceSettingToCommunicationModeLookup.TryGetValue(mode, out var communicationMode))
				{
					return communicationMode;
				}

				_logger.LogWarning($"Invalid IAPI communication mode in '{IAPICommunicationModeSettingSection}.{IAPICommunicationModeSettingName}' setting. WebAPI IAPI communication mode will be used.");
				return IAPICommunicationMode.WebAPI;
			}
			catch
			{
				_logger.LogWarning($"'{IAPICommunicationModeSettingSection}.{IAPICommunicationModeSettingName}' setting not found. WebAPI IAPI communication mode will be used.");
				return IAPICommunicationMode.WebAPI;
			}
		}
	}
}
