using System;
using System.Data.SqlClient;

namespace Relativity.MassImport.Data
{
	using Polly;
	using Relativity.Logging;

	internal static class BulkLoadSqlErrorRetryHelper
	{
		// https://docs.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors?view=sql-server-2017
		private const int _CANNOT_BULK_LOAD_BECAUSE_FILE_LOCKED_MSG_ID = 4861;
		private const int _CANNOT_BULK_LOAD_BECAUSE_FILE_DOES_NOT_EXIST_MSG_ID = 4860;
		// https://lsuse.com/sql-error-messages/
		private const int _BAD_OR_INACCESSIBLE_LOCATION_SPECIFIED_IN_EXTERNAL_DATA_SOURCE_ID = 12704;

		private static bool IsRetryableBulkLoadError(Exception ex)
		{
			SqlException sqlException = ex.GetBaseException() as SqlException;
			if (sqlException is null)
			{
				return false;
			}

			foreach (SqlError e in sqlException.Errors)
			{
				if (e.Number == _CANNOT_BULK_LOAD_BECAUSE_FILE_LOCKED_MSG_ID || e.Number == _CANNOT_BULK_LOAD_BECAUSE_FILE_DOES_NOT_EXIST_MSG_ID || e.Number == _BAD_OR_INACCESSIBLE_LOCATION_SPECIFIED_IN_EXTERNAL_DATA_SOURCE_ID)
				{
					return true;
				}
			}

			return false;
		}

		internal static void RetryOnBulkLoadSqlTemporaryError(Action f, ILog logger)
		{
			RetryOnBulkLoadSqlTemporaryError(f, Relativity.Data.Config.MassImportOnFileLockRetryCount, Relativity.Data.Config.MassImportOnFileLockRetryWaitTimeInMilliseconds, logger);
		}

		internal static void RetryOnBulkLoadSqlTemporaryError(Action f, int retryCount, int retryWaitTimeInMilliseconds, ILog logger)
		{
			var policy =  Policy
				.Handle<Exception>(IsRetryableBulkLoadError)
				.WaitAndRetry(
					retryCount,
					waitTime =>  TimeSpan.FromMilliseconds(retryWaitTimeInMilliseconds),
					onRetry: (exception, waitTime, retryNumber, context) =>
					{
						logger.LogWarning(exception, "Error occured when executing BulkInsert. Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
							retryNumber,
							retryCount,
							waitTime);
					});

			policy.Execute(f);
		}

		internal static bool IsTooMuchDataForSqlError(Exception ex)
		{
			const int sqlErrorNumberTooBig = 7119; // Attempting to grow LOB beyond maximum allowed size of %I64d bytes. -> this indicates the object you wanted to store in SQL is simply too big to be handled by SQL.
			SqlException sqlException = null;
			var loopException = ex;
			while (sqlException is null)
			{
				sqlException = loopException as SqlException;
				if (loopException.InnerException is null)
				{
					break;
				}

				loopException = loopException.InnerException;
			}

			// we must have found a SqlException, and that exception must have the specified number to be a "too much data for sql error"
			return sqlException != null && sqlException.Number == sqlErrorNumberTooBig;
		}
	}
}