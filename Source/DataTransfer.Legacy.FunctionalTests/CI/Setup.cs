﻿using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Configuration;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI
{ 
	[SetUpFixture]
	public class Setup
	{
		private const int RELATIVITY_STARTER_TEMPLATE_ARTIFACT_ID = 1015024;

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
			var applicationArtifactId = applicationService.InstallToLibrary(myRap);
			applicationService.InstallToWorkspace(RELATIVITY_STARTER_TEMPLATE_ARTIFACT_ID, applicationArtifactId);
		}
	}
}
