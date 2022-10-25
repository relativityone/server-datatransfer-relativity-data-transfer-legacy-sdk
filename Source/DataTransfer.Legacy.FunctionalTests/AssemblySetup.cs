using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.Helpers;
using Relativity.Logging;

namespace Relativity.DataTransfer.Legacy.FunctionalTests
{
	[SetUpFixture]
	public class AssemblySetup
	{
		private static Lazy<Task<TestWorkspace>> _testWorkspaceLazy;

		public static IntegrationTestParameters TestParameters { get; private set; }

		public static Task<TestWorkspace> TestWorkspaceAsync => _testWorkspaceLazy.Value;

		public static ILog TestLogger { get; private set; }

		[OneTimeSetUp]
		public static void OneTimeSetUp()
		{
			TestParameters = CreateTestParameters();
			TestLogger = InitializeLogger();

			_testWorkspaceLazy = new Lazy<Task<TestWorkspace>>(
				valueFactory: () => WorkspaceHelper.CreateTestWorkspaceAsync(TestParameters)
			);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			if (_testWorkspaceLazy.IsValueCreated)
			{
				var testWorkspace = await _testWorkspaceLazy.Value.ConfigureAwait(false);
				await WorkspaceHelper.DeleteWorkspaceAsync(TestParameters, testWorkspace.WorkspaceId).ConfigureAwait(false);
			}
		}

		private static IntegrationTestParameters CreateTestParameters()
		{
			string solutionPath = Directory
				.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)
				.Parent.Parent.Parent.FullName;

			string pluginPath = Path.Combine(solutionPath, "TestParameters.json");

			Debug.WriteLine(pluginPath);
			Console.WriteLine(pluginPath);
			if (System.IO.File.Exists(pluginPath))
			{
				var text = System.IO.File.ReadAllText(pluginPath);
				Console.WriteLine(text);
				return Newtonsoft.Json.JsonConvert.DeserializeObject<IntegrationTestParameters>(text);
			}

			return new IntegrationTestParameters
			{
				RelativityUrl = ConfigurationManager.AppSettings["RelativityUrl"],
				RelativityRestUrl = ConfigurationManager.AppSettings["RelativityRestUrl"],
				RelativityUserName = ConfigurationManager.AppSettings["RelativityUserName"],
				RelativityPassword = ConfigurationManager.AppSettings["RelativityPassword"],
				WorkspaceTemplateName = ConfigurationManager.AppSettings["WorkspaceTemplateName"],
				SqlInstanceName = ConfigurationManager.AppSettings["SqlInstanceName"],
				SqlEddsdboUserName = ConfigurationManager.AppSettings["SqlEddsdboUserName"],
				SqlEddsdboPassword = ConfigurationManager.AppSettings["SqlEddsdboPassword"],
				BcpSharePath = ConfigurationManager.AppSettings["BcpSharePath"],
			};
		}

		private static ILog InitializeLogger()
		{
			var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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