using System;
using Relativity.API;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Relativity.DataTransfer.Legacy.Services.SQL
{
	internal class DbContextToSqlExecutorAdapter : ISqlExecutor
	{
		private readonly IHelper _helper;
		private readonly ISqlRetryPolicy _retryPolicy;

		public DbContextToSqlExecutorAdapter(IHelper helper, ISqlRetryPolicy retryPolicy)
		{
			_helper = helper;
			_retryPolicy = retryPolicy;
		}

		public int ExecuteNonQuerySQLStatement(int workspaceId, string query)
		{
			return ExecuteNonQuerySQLStatement(workspaceId, query, Array.Empty<SqlParameter>());
		}

		public int ExecuteNonQuerySQLStatement(int workspaceId, string query, IEnumerable<SqlParameter> parameters)
		{
			return _retryPolicy.Execute(() => _helper.GetDBContext(workspaceId).ExecuteNonQuerySQLStatement(query, parameters));
		}

		public List<T> ExecuteReader<T>(int workspaceId, string query, IEnumerable<SqlParameter> parameters, Func<IDataRecord, T> converter)
		{
			SqlDataReader reader = _retryPolicy.Execute(() => _helper.GetDBContext(workspaceId).ExecuteParameterizedSQLStatementAsReader(query, parameters));
			return this.ConvertDataReaderToList(reader, converter);
		}

		private List<T> ConvertDataReaderToList<T>(IDataReader dataReader, Func<IDataRecord, T> converter)
		{
			List<T> results = new List<T>();
			using (dataReader)
			{
				while (dataReader.Read())
				{
					results.Add(converter(dataReader));
				}
			}

			return results;
		}
	}
}
