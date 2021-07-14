using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Configuration;
using Relativity.Testing.Framework.Models;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI
{ 
	[SetUpFixture]
	public class Setup
	{
		private const string IApiSettingsName = "IAPICommunicationMode";
		private const string IApiSettingsSection = "DataTransfer.Legacy";
		private const string IApiSettingsValue = "Kepler";
		private const InstanceSettingValueType IApiSettingsValueType = InstanceSettingValueType.Text;

		[OneTimeSetUp]
		public void SetupTests()
		{
			var additionalSettings = new Dictionary<string, string>()
			{
				["PerformRelativityVersionCheck"] = "false"
			};

			var configurationRoot = new ConfigurationBuilder().
				AddNUnitParameters().
				AddEnvironmentVariables().
				AddInMemoryCollection(additionalSettings).
				Build();

			RelativityFacade.Instance.RelyOn(new CoreComponent { ConfigurationRoot = configurationRoot });
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			var myRap = Path.Combine(RelativityFacade.Instance.Config.RelativityInstance.RapDirectory, "DataTransfer.Legacy.rap");

			var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();
			applicationService.InstallToLibrary(myRap);

			var instanceSettingsService = RelativityFacade.Instance.Resolve<IInstanceSettingsService>();
			
			var currentInstanceSetting = instanceSettingsService.Get(IApiSettingsName, IApiSettingsSection);
			if (currentInstanceSetting == null)
			{
				var iapiCommunicationMode = new Testing.Framework.Models.InstanceSetting
				{
					Name = IApiSettingsName,
					Section = IApiSettingsSection,
					Value = IApiSettingsValue,
					ValueType = IApiSettingsValueType
				};
				instanceSettingsService.Create(iapiCommunicationMode);
			}
			else
			{
				currentInstanceSetting.Value = IApiSettingsValue;
				currentInstanceSetting.ValueType = IApiSettingsValueType;
				instanceSettingsService.Update(currentInstanceSetting);
			}
		}
	}
}
