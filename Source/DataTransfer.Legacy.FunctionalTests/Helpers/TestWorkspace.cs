
namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	public class TestWorkspace
	{
		private readonly IntegrationTestParameters _testParameters;

		public TestWorkspace(
			IntegrationTestParameters testParameters,
			int workspaceId,
			string workspaceName,
			string defaultFileRepository)
		{
			_testParameters = testParameters;
			WorkspaceId = workspaceId;
			WorkspaceName = workspaceName;
			DefaultFileRepository = defaultFileRepository;
		}

		public int WorkspaceId { get; }
		public string WorkspaceName { get; }
		public string DefaultFileRepository { get; }
	}
}
