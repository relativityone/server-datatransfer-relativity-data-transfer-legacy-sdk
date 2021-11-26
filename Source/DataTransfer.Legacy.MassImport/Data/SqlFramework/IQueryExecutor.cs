using System;
using kCura.Data.RowDataGateway;
using System.Data.SqlClient;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal interface IQueryExecutor
	{
		/// <summary>
		/// Executes batch of SQL queries and returns result of the first query which returned value.
		/// When one of the queries fails it throws an exception.
		/// </summary>
		/// <typeparam name="T">Type of the result of the query.</typeparam>
		/// <param name="query">Batch of SQL queries.</param>
		/// <param name="timeoutInSeconds">Timeout (in seconds) of the query execution. -1 when no timeout.</param>
		/// <returns>Result of the first query which returned value.</returns>
		/// <exception cref="ExecuteSQLStatementFailedException">Thrown when of the queries failed.</exception>
		/// <exception cref="FormatException">Thrown when converting result of the query to the requested type failed.</exception>
		T ExecuteBatchOfSqlStatementsAsScalar<T>(ISqlQueryPart query, int timeoutInSeconds = -1);

		/// <summary>
		/// Executes batch of SQL queries and returns result of the first query which returned value.
		/// When one of the queries fails it throws an exception.
		/// </summary>
		/// <typeparam name="T">Type of the result of the query.</typeparam>
		/// <param name="query">Batch of SQL queries.</param>
		/// <param name="parameters">SQL query parameters</param>
		/// <param name="timeoutInSeconds">Timeout (in seconds) of the query execution. -1 when no timeout.</param>
		/// <returns>Result of the first query which returned value.</returns>
		/// <exception cref="ExecuteSQLStatementFailedException">Thrown when of the queries failed.</exception>
		/// <exception cref="FormatException">Thrown when converting result of the query to the requested type failed.</exception>
		T ExecuteBatchOfSqlStatementsAsScalar<T>(ISqlQueryPart query, SqlParameter[] parameters, int timeoutInSeconds = -1);

		/// <summary>
		/// Executes batch of SQL queries and returns result of the first query which returned value.
		/// When one of the queries fails it throws an exception.
		/// </summary>
		/// <typeparam name="T">Type of the result of the query.</typeparam>
		/// <param name="query">Batch of SQL queries.</param>
		/// <param name="parameters">SQL query parameters</param>
		/// <param name="timeoutInSeconds">Timeout (in seconds) of the query execution. -1 when no timeout.</param>
		/// <returns>Result of the first query which returned value.</returns>
		/// <exception cref="ExecuteSQLStatementFailedException">Thrown when of the queries failed.</exception>
		/// <exception cref="FormatException">Thrown when converting result of the query to the requested type failed.</exception>
		T ExecuteBatchOfSqlStatementsAsScalar<T>(string query, SqlParameter[] parameters, int timeoutInSeconds = -1);
	}
}
