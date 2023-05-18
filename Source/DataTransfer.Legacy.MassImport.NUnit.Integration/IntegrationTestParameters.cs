using System;

namespace MassImport.NUnit.Integration
{
	public class IntegrationTestParameters
	{
		private readonly Lazy<Relativity.Infrastructure.V1.SQLPrimary.Models.SqlPrimaryServerResponse> _sqlPrimaryFactory;

		public IntegrationTestParameters(Lazy<Relativity.Infrastructure.V1.SQLPrimary.Models.SqlPrimaryServerResponse> sqlPrimaryFactory)
		{
			_sqlPrimaryFactory = sqlPrimaryFactory ?? throw new ArgumentNullException(nameof(sqlPrimaryFactory));
		}

		public string RelativityUrl { get; set; }
		public string RelativityRestUrl { get; set; }
		public string RelativityUserName { get; set; }
		public string RelativityPassword { get; set; }
		public string WorkspaceTemplateName { get; set; }
		public string SqlInstanceName { get; set; }
		public string SqlEddsdboUserName { get; set; }
		public string SqlEddsdboPassword { get; set; }
		public string BcpSharePath => _sqlPrimaryFactory.Value.BcpPath;
	}
}