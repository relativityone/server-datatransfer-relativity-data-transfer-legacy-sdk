using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using MassImport.NUnit.Integration.Helpers;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

using Relativity.Logging;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Configuration;

namespace MassImport.NUnit.Integration
{
	[SetUpFixture]
	public class OneTimeSetup
	{
		private static Lazy<Task<TestWorkspace>> _testWorkspaceLazy;

		public static IntegrationTestParameters TestParameters { get; private set; }

		public static Task<TestWorkspace> TestWorkspaceAsync => _testWorkspaceLazy.Value;

		public static ILog TestLogger { get; private set; }

		[OneTimeSetUp]
		public static void OneTimeSetUp()
		{
			IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddNUnitParameters()
			                                                                 .AddEnvironmentVariables()
			                                                                 .AddInMemoryCollection()
			                                                                 .Build();
			RelativityFacade.Instance.RelyOn(new CoreComponent { ConfigurationRoot = configurationRoot });
			if (string.IsNullOrWhiteSpace(RelativityFacade.Instance.Config.RelativityInstance.RelativityHostAddress))
			{
				Assert.Fail(
					"The functional tests cannot be run because the RTF configuration is null or empty. Ensure .\\DevelopmentScripts\\New-TestSettings.ps1 is executed and retry.");
			}

			Uri relativityBaseUri = new Uri(
				$"{RelativityFacade.Instance.Config.RelativityInstance.ServerBindingType}://{RelativityFacade.Instance.Config.RelativityInstance.RestServicesHostAddress}");
			Uri relativityRestApi = new Uri(relativityBaseUri, new Uri("relativity.rest/api", UriKind.Relative));
			TestParameters =
				new IntegrationTestParameters(
					new Lazy<Relativity.Infrastructure.V1.SQLPrimary.Models.SqlPrimaryServerResponse>(
						valueFactory: () =>
							CreateSqlPrimaryServerResponse(RelativityFacade.Instance, relativityRestApi)))
					{
						RelativityUrl = relativityBaseUri.ToString(),
						RelativityRestUrl = relativityRestApi.ToString(),
						RelativityUserName = RelativityFacade.Instance.Config.RelativityInstance.AdminUsername,
						RelativityPassword = RelativityFacade.Instance.Config.RelativityInstance.AdminPassword,
						SqlInstanceName = RelativityFacade.Instance.Config.RelativityInstance.SqlServer,
						SqlEddsdboUserName = RelativityFacade.Instance.Config.RelativityInstance.SqlUsername,
						SqlEddsdboPassword = RelativityFacade.Instance.Config.RelativityInstance.SqlPassword,
						WorkspaceTemplateName = "Relativity Starter Template",
					};
			TestLogger = InitializeLogger();
			_testWorkspaceLazy = new Lazy<Task<TestWorkspace>>(
				valueFactory: () => WorkspaceHelper.CreateTestWorkspaceAsync(TestParameters));
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			if (_testWorkspaceLazy.IsValueCreated)
			{
				var testWorkspace = await _testWorkspaceLazy.Value.ConfigureAwait(false);
				await WorkspaceHelper.DeleteWorkspaceAsync(TestParameters, testWorkspace.WorkspaceId)
				                     .ConfigureAwait(false);
			}
		}

		private static Relativity.Services.ServiceProxy.ServiceFactory CreateServiceFactory(
			Relativity.Testing.Framework.IRelativityFacade relativityInstance,
			Uri relativityRestApi)
		{
			Relativity.Services.ServiceProxy.UsernamePasswordCredentials credentials =
				new Relativity.Services.ServiceProxy.UsernamePasswordCredentials(
					relativityInstance.Config.RelativityInstance.AdminUsername,
					relativityInstance.Config.RelativityInstance.AdminPassword);
			Relativity.Services.ServiceProxy.ServiceFactorySettings settings =
				new Relativity.Services.ServiceProxy.ServiceFactorySettings(relativityRestApi, credentials);
			return new Relativity.Services.ServiceProxy.ServiceFactory(settings);
		}

		private static Relativity.Infrastructure.V1.SQLPrimary.Models.SqlPrimaryServerResponse
			CreateSqlPrimaryServerResponse(
				Relativity.Testing.Framework.IRelativityFacade relativityInstance,
				Uri relativityRestApi)
		{
			Relativity.Services.ServiceProxy.ServiceFactory serviceFactory = CreateServiceFactory(
				relativityInstance,
				relativityRestApi);
			Relativity.Infrastructure.V1.SQLPrimary.ISqlPrimaryServerManager sqlPrimaryServerManager =
				serviceFactory.CreateProxy<Relativity.Infrastructure.V1.SQLPrimary.ISqlPrimaryServerManager>();
			Relativity.Infrastructure.V1.SQLPrimary.Models.SqlPrimaryServerResponse sqlPrimaryServerResponse =
				sqlPrimaryServerManager.ReadAsync(serverID: 1015096) // This should be constant for TestVM/Hopper SUTs
				                       .ConfigureAwait(false)
				                       .GetAwaiter()
				                       .GetResult();
			if (sqlPrimaryServerResponse == null || sqlPrimaryServerResponse.ArtifactID == 0)
			{
				Assert.Fail(
					"The functional tests cannot be run because the SQL resource server cannot be determined and indicates a SUT configuration problem.");
			}

			if (string.IsNullOrEmpty(sqlPrimaryServerResponse.BcpPath))
			{
				Assert.Fail(
					"The functional tests cannot be run because the SQL resource server is non-null but the BCP share is null or empty and indicates a SUT configuration problem.");
			}

            return sqlPrimaryServerResponse;
		}

		private static ILog InitializeLogger()
		{
			var baseDirectory = Path.GetDirectoryName(
				Assembly.GetExecutingAssembly()
				        .Location);
			var loggerOptions = new LoggerOptions
				                    {
					                    Application = "25f08b36-2ad6-4cc2-9b81-38c301b9125f",
					                    ConfigurationFileLocation = Path.Combine(baseDirectory, "LogConfig.xml"),
					                    System = "MassImport",
					                    SubSystem = "Tests",
				                    };

			// Configure the optional SEQ sink to periodically send logs to the local SEQ server for improved debugging.
			// See https://getseq.net/ for more details.
			loggerOptions.AddSinkParameter(
				Relativity.Logging.Configuration.SeqSinkConfig.ServerUrlSinkParameterKey,
				new Uri("http://localhost:5341"));

			ILog logger = Relativity.Logging.Factory.LogFactory.GetLogger(loggerOptions);
			Log.Logger = logger;
			return logger;
		}
	}
}