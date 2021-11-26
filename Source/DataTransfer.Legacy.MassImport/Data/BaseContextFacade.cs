using System;
using System.Data;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;

namespace Relativity.MassImport.Data
{
	internal class BaseContextFacade : IBaseContextFacade
	{
		private readonly BaseContext _baseContext;

		public BaseContextFacade(BaseContext baseContext)
		{
			_baseContext = baseContext ?? throw new ArgumentNullException(nameof(baseContext));
		}

		public IDataReader ExecuteSQLStatementAsReader(string query, int timeoutInSeconds) =>
			_baseContext.ExecuteSQLStatementAsReader(query, timeoutInSeconds);

		public IDataReader ExecuteSQLStatementAsReader(string query, SqlParameter[] parameters, int timeoutInSeconds) =>
			_baseContext.ExecuteSQLStatementAsReader(query, parameters, timeoutInSeconds);

		public void ReleaseConnection() => _baseContext.ReleaseConnection();
	}
}
