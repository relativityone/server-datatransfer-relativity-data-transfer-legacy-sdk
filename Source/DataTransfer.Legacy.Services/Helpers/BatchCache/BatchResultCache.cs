using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using kCura.Utility;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.SQL;
using Relativity.Services.Exceptions;
using DateTime = System.DateTime;

namespace Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache
{
	public class BatchResultCache : IBatchResultCache
	{
		private const string BatchResultTablePrefix = "RELMASSRESULT_";
		private readonly IAPILog _logger;
		private readonly ISqlExecutor _sqlExecutor;

		public BatchResultCache(IAPILog logger, ISqlExecutor sqlExecutor)
		{
			_logger = logger;
			_sqlExecutor = sqlExecutor;
		}

		public MassImportResults GetCreateOrThrow(int workspaceID, string runID, string batchID)
		{
			if (!IsParametersValid(runID, nameof(runID))) return null;
			if (!IsParametersValid(batchID, nameof(batchID))) return null;


			var tableName = GetTableName(runID);
			var query = GetInsertQuery(batchID, tableName);
			var parameters = new List<SqlParameter> { new SqlParameter("@createdOn", DateTime.Now) };
			ResultCacheItem ConvertDataRecordToItem(IDataRecord record)
			{
				var batch = record.GetString(0);
				var createdOn = record.GetDateTime(1);
				var finishedOn = record.IsDBNull(2) ? (DateTime?)null : record.GetDateTime(2);
				var serializedResult = record.IsDBNull(3) ? null : record.GetString(3);
				var isNew = record.GetBoolean(4);

				return new ResultCacheItem(batch, createdOn, finishedOn, serializedResult, isNew);
			}

			var results = _sqlExecutor.ExecuteReader(workspaceID, query, parameters, ConvertDataRecordToItem);
			if (results.Count == 0)
			{
				_logger.LogError("There is no data from DataReader");
				TraceHelper.SetStatusError(Activity.Current, $"There is no data from DataReader");
				return null;
			}

			if (results.Count > 1)
			{
				_logger.LogError("There is more than one row: {rows}, values: {@values}, using the first row {@row}", results.Count, results, results.First());
				TraceHelper.SetStatusError(Activity.Current, $"There is more than one row: {results.Count}, values: {@results}, using the first row {results.First()}");
			}

			var result = results[0];

			if (result.IsNew)
			{
				return null;
			}

			if (result.FinishedOn.HasValue == false)
			{
				_logger.LogError("Result exists but it is not finished yet {runID}, {@result}", runID, result);
				TraceHelper.SetStatusError(Activity.Current, $"Result exists but it is not finished yet {runID}, {@result}");
				throw new ConflictException("Batch In Progress");
			}

			if (string.IsNullOrEmpty(result.SerializedResult))
			{
				_logger.LogError("Expected to deserialize result but it is empty {runID}, {@result}", runID, result);
				TraceHelper.SetStatusError(Activity.Current, $"Expected to deserialize result but it is empty {runID}, {@result}");
				throw new ServiceException("Batch result is empty");
			}

			try
			{
				_logger.LogWarning("Returning existing result for: {runID}, {batchID}, created: {createdOn}, finished: {finished}, MassImportResult: {result}", runID, result.BatchID, result.CreatedOn, result.FinishedOn, result.SerializedResult);
				return JsonConvert.DeserializeObject<MassImportResults>(result.SerializedResult);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to deserialize {runID} MassImportResults: {result}", runID, result.SerializedResult);
				TraceHelper.SetStatusError(Activity.Current, $"Failed to deserialize {runID} MassImportResults: {result.SerializedResult}: {ex.Message}", ex);
				throw new ServiceException("Failed to deserialize batch result", ex);
			}
		}

		private static string GetInsertQuery(string batchID, string tableName)
		{
			var query = $@"
IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{tableName}')
BEGIN
	CREATE TABLE [Resource].[{tableName}] (
		[BatchID] NVARCHAR(100) NOT NULL PRIMARY KEY,
		[CreatedOn] DATETIME2 NOT NULL,
		[FinishedOn] DATETIME2 NULL,
		[RunResult] NVARCHAR(MAX) NULL
	)
END

IF EXISTS(SELECT 1 FROM [Resource].[{tableName}] WHERE [BatchID] = '{batchID}' )
BEGIN
	SELECT 
	[BatchID],
	[CreatedOn],
	[FinishedOn],
	[RunResult],
	CAST(0 AS BIT) as IsNew
	FROM [Resource].[{tableName}] 
	WHERE [BatchID] = '{batchID}'
END
ELSE
BEGIN
	INSERT INTO [Resource].[{tableName}] 
	OUTPUT INSERTED.[BatchID], INSERTED.[CreatedOn], INSERTED.[FinishedOn], INSERTED.[RunResult], CAST(1 AS BIT) as IsNew
	VALUES('{batchID}',@createdOn , NULL, NULL)
END
";
			return query;
		}

		public void Update(int workspaceID, string runID, string batchID, MassImportResults massImportResult)
		{
			if (!IsParametersValid(runID, nameof(runID))) return;
			if (!IsParametersValid(batchID, nameof(batchID))) return;

			var tableName = GetTableName(runID);
			string serialized;
			try
			{
				serialized = JsonConvert.SerializeObject(massImportResult);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to serialize MassImportResults: {@result}", massImportResult);
				TraceHelper.SetStatusError(Activity.Current, $"Failed to serialize MassImportResults: {massImportResult}: {ex.Message}", ex);
				return;
			}

			var query = $@"
UPDATE [Resource].[{tableName}] 
SET 
	[FinishedOn] = @finishedOn,
	[RunResult] = @serialized
WHERE [BatchID] = '{batchID}'
";

			var parameters = new List<SqlParameter>
			{
				new SqlParameter("@finishedOn", DateTime.Now),
				new SqlParameter("@serialized", SqlDbType.NVarChar)
				{
					Value = serialized
				},
			};


			var result = _sqlExecutor.ExecuteNonQuerySQLStatement(workspaceID, query, parameters);
			if (result != 1)
			{
				_logger.LogError("Expected to update one row but: {result} returned", result);
				TraceHelper.SetStatusError(Activity.Current, $"Expected to update one row but: {result} returned");
			}
		}

		public void Cleanup(int workspaceID, string runID)
		{
			if (!IsParametersValid(runID, nameof(runID))) return;

			var tableName = GetTableName(runID);
			var query = $@"
IF EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{tableName}')
BEGIN
	DROP TABLE[Resource].[{tableName}]
END ";

			_sqlExecutor.ExecuteNonQuerySQLStatement(workspaceID, query);
		}

		private bool IsParametersValid(string param, string paramName)
		{
			// This can be empty for old clients, but old clients dont have retries implemented
			if (string.IsNullOrEmpty(param))
			{
				_logger.LogDebug("Empty parameter {paramName}", paramName);
				return false;
			}

			if (!SQLInjectionHelper.IsValidRunId(param))
			{
				throw new Exception($"Invalid {paramName}");
			}

			return true;
		}

		private string GetTableName(string runID)
		{
			return $"{BatchResultTablePrefix}{runID}";
		}
	}

	public class ResultCacheItem
	{
		public string BatchID { get; }
		public DateTime? CreatedOn { get; }
		public DateTime? FinishedOn { get; }
		public string SerializedResult { get; }
		public bool IsNew { get; }

		public ResultCacheItem(string batchID, DateTime? createdOn, DateTime? finishedOn, string serializedResult, bool isNew)
		{
			BatchID = batchID;
			CreatedOn = createdOn;
			FinishedOn = finishedOn;
			SerializedResult = serializedResult;
			IsNew = isNew;
		}
	}
}