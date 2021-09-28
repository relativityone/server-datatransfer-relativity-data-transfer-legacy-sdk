﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using IInstanceSettingsBundle = Relativity.API.IInstanceSettingsBundle;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class TAPIService : BaseService, ITAPIService
	{
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;

		private const string TapiMaxAllowedTargetDataRateMbpsSettingSection = "Relativity.DataTransfer";
		private const string TapiMaxAllowedTargetDataRateMbpsSettingName = "TapiMaxAllowedTargetDataRateMbps";

		public TAPIService(IServiceContextFactory serviceContextFactory, IInstanceSettingsBundle instanceSettingsBundle)
			: base(serviceContextFactory)
		{
			_instanceSettingsBundle = instanceSettingsBundle;
		}

		public async Task<TAPIConfiguration> RetrieveConfigurationAsync(string correlationID)
		{
			bool cloudInstance = Relativity.Core.Config.CloudInstance;
			uint? tapiMaxAllowedTargetDataRateMbps = tapiMaxAllowedTargetDataRateMbps = await _instanceSettingsBundle
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
