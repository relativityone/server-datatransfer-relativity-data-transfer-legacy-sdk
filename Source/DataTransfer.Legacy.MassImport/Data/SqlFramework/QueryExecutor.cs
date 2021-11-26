using System;
using System.Data;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using Relativity.Logging;

namespace Relativity.MassImport.Data.SqlFramework
{
	// Tests: https://git.kcura.com/projects/DTX/repos/relativity-massimport-core/browse/source/MassImport.NUnit.Integration/Data/SqlFramework/QueryExecutorTests.cs
	internal class QueryExecutor : IQueryExecutor
	{
		private readonly IBaseContextFacade _baseContext;
		private readonly ILog _logger;

		public QueryExecutor(BaseContext baseContext, ILog logger) : this(new BaseContextFacade(baseContext), logger)
		{
		}

		public QueryExecutor(IBaseContextFacade baseContext, ILog logger)
		{
			_baseContext = baseContext;
			_logger = logger;
		}

		/// <inheritdoc/>
		public T ExecuteBatchOfSqlStatementsAsScalar<T>(ISqlQueryPart query, int timeoutInSeconds = -1)
		{
			string queryAsString = query.BuildQuery();
			return ExecuteBatchOfSqlStatementsAsScalarInternal<T>(queryAsString, parameters: null, timeoutInSeconds);
		}

		/// <inheritdoc/>
		public T ExecuteBatchOfSqlStatementsAsScalar<T>(ISqlQueryPart query, SqlParameter[] parameters, int timeoutInSeconds = -1)
		{
			string queryAsString = query.BuildQuery();
			return ExecuteBatchOfSqlStatementsAsScalarInternal<T>(queryAsString, parameters, timeoutInSeconds);
		}

		/// <inheritdoc/>
		public T ExecuteBatchOfSqlStatementsAsScalar<T>(string query, SqlParameter[] parameters, int timeoutInSeconds = -1)
		{
			return ExecuteBatchOfSqlStatementsAsScalarInternal<T>(query, parameters, timeoutInSeconds);
		}

		private T ExecuteBatchOfSqlStatementsAsScalarInternal<T>(string query, SqlParameter[] parameters, int timeoutInSeconds)
		{
			IDataReader reader = parameters != null
				? _baseContext.ExecuteSQLStatementAsReader(query, parameters, timeoutInSeconds)
				: _baseContext.ExecuteSQLStatementAsReader(query, timeoutInSeconds);

			try
			{
				return ReadScalarValueFromDataReader<T>(reader, query);
			}
			finally
			{
				reader.Dispose();
				_baseContext.ReleaseConnection();
			}
		}

		private T ReadScalarValueFromDataReader<T>(IDataReader reader, string query)
		{
			T result = default;
			bool resultAssigned = false;

			if (reader.Read())
			{
				object value = reader.GetValue(0);

				try
				{
					result = (T)Convert.ChangeType(value, typeof(T));
					resultAssigned = true;
				}
				catch (Exception ex)
				{
					_logger.LogError("Error occurred when processing {query} result. Unable to convert scalar value to type '{type}'.", query, typeof(T));
					throw new FormatException($"Error occurred when processing query result. Unable to convert scalar value to type '{typeof(T)}'.", ex);
				}
			}

			bool hasMoreResults;
			do
			{
				try
				{
					hasMoreResults = reader.NextResult();
				}
				catch (Exception ex)
				{
					throw new ExecuteSQLStatementFailedException(ex, query);
				}

				if (hasMoreResults)
				{
					_logger.LogWarning("Query: {query} was executed as a scalar, but it returned more than one result.", query);
				}
			} while (hasMoreResults);

			if (!resultAssigned)
			{
				_logger.LogWarning("Query: {query} was executed as a scalar, but it has not returned any value. Using default value for '{type}'.", query, typeof(T));
			}

			return result;
		}
	}
}
