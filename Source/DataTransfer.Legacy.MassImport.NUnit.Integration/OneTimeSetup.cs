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
					new Lazy<string>(
						valueFactory: () =>
							CreateSqlPrimaryServerBcpPath(RelativityFacade.Instance)))
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

		private static string CreateSqlPrimaryServerBcpPath(Relativity.Testing.Framework.IRelativityFacade relativityInstance)
		{
			System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder
				                                                       {
					                                                       DataSource =
						                                                       relativityInstance.Config
							                                                       .RelativityInstance.SqlServer,
					                                                       InitialCatalog = "edds",
					                                                       IntegratedSecurity = false,
					                                                       UserID = relativityInstance.Config
						                                                       .RelativityInstance.SqlUsername,
					                                                       Password = relativityInstance.Config
						                                                       .RelativityInstance.SqlPassword
				                                                       };
			using (System.Data.SqlClient.SqlConnection conn =
			       new System.Data.SqlClient.SqlConnection(csb.ConnectionString))
			{
				conn.Open();
				using (System.Data.SqlClient.SqlCommand command = conn.CreateCommand())
				{
					command.CommandText = @"
SELECT TOP 1	
	rs.[TemporaryDirectory] AS BcpPath
FROM [EDDS].[eddsdbo].[ResourceGroupSQLServers]
INNER JOIN [EDDS].[eddsdbo].[ResourceServer] as rs
	ON rs.[ArtifactID] = [ResourceGroupSQLServers].[SQLServerArtifactID]
INNER JOIN [EDDS].[eddsdbo].[Code] c1
	ON c1.[ArtifactID] = rs.[Type]
INNER JOIN [EDDS].[eddsdbo].[Code] c2
	ON c2.[ArtifactID] = rs.[Status]
WHERE c2.[Name] = N'Active' AND c1.Name = N'SQL - Primary'
";
					string bcpPath = command.ExecuteScalar() as string;
					if (string.IsNullOrEmpty(bcpPath))
					{
						Assert.Fail(
							"The functional tests cannot be run because the BCP share is null or empty and indicates a SUT configuration problem.");
					}

                    return bcpPath;
				}
			}
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