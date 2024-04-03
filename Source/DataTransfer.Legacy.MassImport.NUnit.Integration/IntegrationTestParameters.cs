using System;

namespace MassImport.NUnit.Integration
{
	public class IntegrationTestParameters
	{
	//	private readonly Lazy<string> _sqlBcpPath;

		//public IntegrationTestParameters(Lazy<string> sqlBcpPath)
		//{
		//	_sqlBcpPath = sqlBcpPath ?? throw new ArgumentNullException(nameof(sqlBcpPath));
		//}

		public string RelativityUrl { get; set; }
		public string RelativityRestUrl { get; set; }
		public string RelativityUserName { get; set; }
		public string RelativityPassword { get; set; }
		public string WorkspaceTemplateName { get; set; }
		public string SqlInstanceName { get; set; }
		public string SqlEddsdboUserName { get; set; }
		public string SqlEddsdboPassword { get; set; }
		public string BcpSharePath { get; set; }
	}
}