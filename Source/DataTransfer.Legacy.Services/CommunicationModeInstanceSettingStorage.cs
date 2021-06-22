using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class CommunicationModeInstanceSettingStorage : ICommunicationModeStorage
	{
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;

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

		public CommunicationModeInstanceSettingStorage(IInstanceSettingsBundle instanceSettingsBundle)
		{
			_instanceSettingsBundle = instanceSettingsBundle;
		}

		public async Task<(bool, IAPICommunicationMode)> TryGetModeAsync()
		{
			var mode = await _instanceSettingsBundle
				.GetStringAsync(IAPICommunicationModeSettingSection, IAPICommunicationModeSettingName).ConfigureAwait(false);
			if (_instanceSettingToCommunicationModeLookup.TryGetValue(mode, out var communicationMode))
			{
				{
					return (true, communicationMode);
				}
			}

			return (false, default);
		}

		public string GetStorageKey()
		{
			return $"{IAPICommunicationModeSettingSection}.{IAPICommunicationModeSettingName}";
		}
	}
}