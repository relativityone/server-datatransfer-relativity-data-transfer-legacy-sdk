using System.Drawing;
using Relativity.Services.Layout.DataContracts.Builder;

namespace DataTransfer.Legacy.PostInstallEventHandler
{
	using System;
	using System.Threading.Tasks;
	using Relativity.API;
	using System.Linq;
	using Relativity.Services.InstanceSetting;

	public class InstanceSettingsService : IInstanceSettingsService
	{
		private readonly IAPILog _logger;
		private readonly IHelper _helper;
		private readonly IRetryPolicyProvider _retryPolicyProvider;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="helper"></param>
		/// <param name="retryPolicyProvider"></param>
		public InstanceSettingsService(IAPILog logger, IHelper helper, IRetryPolicyProvider retryPolicyProvider)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this._helper = helper ?? throw new ArgumentNullException(nameof(helper));
			this._retryPolicyProvider = retryPolicyProvider ?? throw new ArgumentNullException(nameof(retryPolicyProvider));
		}

		/// <inheritdoc />
		public async Task<bool> CreateInstanceSettingsTextType(string name, string section, string value, string description)
		{
			try
			{
				_logger.LogInformation("DataTransfer.Legacy PostInstallEventHandler: start upserting instance setting {name}.", name);
				var setting = new InstanceSetting
				{
					Name = name,
					Section = section,
					ValueType = Relativity.Services.InstanceSetting.ValueType.Text,
					Value = value,
					Description = description,
				};

				var result = await this.CreateFromInstanceSettingRequest(setting).ConfigureAwait(false);
				_logger.LogInformation("DataTransfer.Legacy PostInstallEventHandler: end upserting instance setting {name}.", name);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while creating Instance Setting");
				throw;
			}
		}

		private async Task<bool> CreateFromInstanceSettingRequest(InstanceSetting settings)
		{
			using (var instanceSettingManager = this._helper.GetServicesManager().CreateProxy<IInstanceSettingManager>(ExecutionIdentity.System))
			{
				var query = new Relativity.Services.Query { Condition = $"'Name' == '{settings.Name}' AND 'Section' == '{settings.Section}'" };
				InstanceSettingQueryResultSet instanceSettings = await this._retryPolicyProvider.GetAsyncRetryPolicy().ExecuteAsync(() => instanceSettingManager.QueryAsync(query)).ConfigureAwait(false);
				if (!instanceSettings.Success)
				{
					_logger.LogWarning("DataTransfer.Legacy PostInstallEventHandler: could not read instance settings from db.");
					return false;
				}

				if (instanceSettings.Results.Count > 0)
				{
					_logger.LogInformation("DataTransfer.Legacy PostInstallEventHandler: instance setting {settings.Name} already exists.", settings.Name);
					return true;
				}

				var result = await this._retryPolicyProvider.GetAsyncRetryPolicy().ExecuteAsync(() => instanceSettingManager.CreateSingleAsync(settings)).ConfigureAwait(false) > 0;
				string resultInterpretation = result ? "created" : "not created";
				_logger.LogInformation("DataTransfer.Legacy PostInstallEventHandler: instance setting {settings.Name} {resultInterpretation}.", settings.Name, resultInterpretation);
				return result;
			}
		}
	}
}