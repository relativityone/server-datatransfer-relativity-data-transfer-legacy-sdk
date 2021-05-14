using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Configuration;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI
{ 
	[SetUpFixture]
	public class Setup
	{
		[OneTimeSetUp]
		public void SetupTests()
		{
			var additionalSettings = new Dictionary<string, string>
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

			using (var applicationManager = RelativityFacade.Instance.Resolve<ILibraryApplicationManager>())
			{
				CreateLibraryApplicationResponse response;
				using (var fileStream = File.OpenRead(myRap))
				{
					using (var keplerStream = new KeplerStream(fileStream))
					{
						response = applicationManager.CreateAsync(-1, keplerStream, false).GetAwaiter().GetResult();
					}
				}

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				while (true)
				{
					try
					{
						var installStatusResponse = applicationManager.GetLibraryInstallStatusAsync(-1, response.ApplicationIdentifier.Guids[0])
							.GetAwaiter().GetResult();
						if (IsInstallationCompleted(installStatusResponse))
						{
							break;
						}

						if (HasInstallationFailedOrTimedOut(stopwatch, installStatusResponse))
						{
							throw new InvalidOperationException("Failed to install application");
						}

						Thread.Sleep(TimeSpan.FromSeconds(5));
					}
					finally
					{
						stopwatch.Stop();
					}
				}
			}
		}

		private static bool HasInstallationFailedOrTimedOut(Stopwatch stopwatch, IHasInstallStatus installStatusResponse)
		{
			return stopwatch.Elapsed > TimeSpan.FromMinutes(5) || installStatusResponse.InstallStatus.Code == InstallStatusCode.Failed;
		}

		private static bool IsInstallationCompleted(IHasInstallStatus getInstallStatusResponse)
		{
			return getInstallStatusResponse.InstallStatus.Code == InstallStatusCode.Completed;
		}
	}
}
