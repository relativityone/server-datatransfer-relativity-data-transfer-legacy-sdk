using System.Data;
using System.Data.SqlClient;

namespace Relativity.MassImport.Data
{
	internal interface IBaseContextFacade
	{
		IDataReader ExecuteSQLStatementAsReader(string query, int timeoutInSeconds);
		IDataReader ExecuteSQLStatementAsReader(string query, SqlParameter[] parameters, int timeoutInSeconds);
		void ReleaseConnection();
	}
}
