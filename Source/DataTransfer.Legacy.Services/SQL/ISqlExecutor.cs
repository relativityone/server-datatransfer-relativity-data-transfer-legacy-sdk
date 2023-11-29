using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Relativity.DataTransfer.Legacy.Services.SQL
{
	public interface ISqlExecutor
	{
		int ExecuteNonQuerySQLStatement(int workspaceId, string query);

		int ExecuteNonQuerySQLStatement(int workspaceId, string query, IEnumerable<SqlParameter> parameters);

		List<T> ExecuteReader<T>(int workspaceId, string query, IEnumerable<SqlParameter> parameters, Func<IDataRecord, T> converter);
	}
}
