using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using kCura.Config;
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

		private const string CloudInstanceSettingSection = "Relativity.Core";
		private const string CloudInstanceSettingName = "CloudInstance";
		private const string TapiMaxAllowedTargetDataRateMbpsSettingSection = "Relativity.DataTransfer";
		private const string TapiMaxAllowedTargetDataRateMbpsSettingName = "TapiMaxAllowedTargetDataRateMbps";

		public TAPIService(IServiceContextFactory serviceContextFactory, IInstanceSettingsBundle instanceSettingsBundle)
			: base(serviceContextFactory)
		{
			_instanceSettingsBundle = instanceSettingsBundle;
		}

		public async Task<DataSetWrapper> RetrieveTapiConfigurationAsync(string correlationID)
		{
			var result = await RetrieveTapiConfiguration(GetBaseServiceContext(AdminWorkspace)).ConfigureAwait(false);
			return result != null ? new DataSetWrapper(result.ToDataSet()) : null;
		}

		public async Task<kCura.Data.DataView> RetrieveTapiConfiguration(BaseServiceContext sc)
		{
			System.Data.DataTable table = new System.Data.DataTable();
			table.Columns.Add(new DataColumn("Section", typeof(string)));
			table.Columns.Add(new DataColumn("Name", typeof(string)));
			table.Columns.Add(new DataColumn("Value", typeof(string)));

			var cloudInstance = await _instanceSettingsBundle
				.GetStringAsync(CloudInstanceSettingSection, CloudInstanceSettingName).ConfigureAwait(false);

			var tapiMaxAllowedTargetDataRateMbpse = await _instanceSettingsBundle
				.GetStringAsync(TapiMaxAllowedTargetDataRateMbpsSettingSection,
					TapiMaxAllowedTargetDataRateMbpsSettingSection).ConfigureAwait(false);

			table.Rows.Add((object) CloudInstanceSettingSection, (object) CloudInstanceSettingName, (object) cloudInstance);
			table.Rows.Add((object) TapiMaxAllowedTargetDataRateMbpsSettingSection, (object) TapiMaxAllowedTargetDataRateMbpsSettingName, (object) tapiMaxAllowedTargetDataRateMbpse);
			return new kCura.Data.DataView(table);
		}
    }
}
