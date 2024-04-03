namespace MassImport.NUnit.Integration
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

		public string ConnectionString => $"Data Source={this._testParameters.SqlInstanceName};" +
													$"Initial Catalog=EDDS{this.WorkspaceId};" +
													"Persist Security Info=False;" +
													$"User ID={this._testParameters.SqlEddsdboUserName};" +
													$"Password={this._testParameters.SqlEddsdboPassword};" +
													"Packet Size=4096;" +
													"Application Name=\"RelativityWebAPI | 10000000-1000-1000-1000-100000000000\";";

		public string EddsConnectionString => $"Data Source={this._testParameters.SqlInstanceName};" +
		                                      $"Initial Catalog=EDDS;" +
		                                      "Persist Security Info=False;" +
		                                      $"User ID={this._testParameters.SqlEddsdboUserName};" +
		                                      $"Password={this._testParameters.SqlEddsdboPassword};" +
		                                      "Packet Size=4096;" +
		                                      "Application Name=\"RelativityWebAPI | 10000000-1000-1000-1000-100000000000\";";
	}
}
