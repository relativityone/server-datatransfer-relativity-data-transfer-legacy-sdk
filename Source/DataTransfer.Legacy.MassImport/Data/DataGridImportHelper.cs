using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Relativity.Logging;
using Relativity.MassImport.Data.DataGridWriteStrategy;

namespace Relativity.MassImport.Data
{
	internal class DataGridImportHelper
	{
		private Relativity.Data.DataGridContext _dgContext;
		private kCura.Data.RowDataGateway.BaseContext _context;
		private DataGridImportResults _results;
		private readonly ImportMeasurements _measurements;
		private readonly Relativity.Data.ITextMigrationVerifier _textMigrationVerify;

		public DataGridImportHelper(Relativity.Data.DataGridContext dgContext, kCura.Data.RowDataGateway.BaseContext context, ImportMeasurements measurements, Relativity.Data.ITextMigrationVerifier textMigrationVerify)
		{
			_dgContext = dgContext;
			_context = context;
			_measurements = measurements;
			_textMigrationVerify = textMigrationVerify;
		}

		public void WriteToDataGrid(int artifactTypeID, int appID, string runID, DataGridReader loader, bool linkDocuments, bool hasMappedFields, Relativity.Data.DataGridMappingMultiDictionary dataGridMappings, ILog correlationLogger)
		{
			var logger = correlationLogger.ForContext("Method", "WriteToDataGrid", true);
			logger.LogVerbose("Starting WriteToDataGrid");
			kCura.Utility.InjectionManager.Instance.Evaluate("7c9ce32f-7f76-46c0-bfba-ce8d05215935");
			Exception exception = null;
			var errorManager = new DataGridImportErrorManager(dataGridMappings, correlationLogger);
			try
			{
				var fields = loader.GetDataGridFields();
				IEnumerable<int> fieldIds = fields.Select(field => field.ArtifactID).ToList();
				var documentIds = dataGridMappings.GetAllArtifactIds();
				var pendingDocumentIds = new Dictionary<int, List<int>>();
				var textMigrationQueueTables = _textMigrationVerify.GetMigrationTablesForFields(fieldIds).ToList();
				bool anyPendingDocuments = false;
				if (textMigrationQueueTables.Count > 0)
				{
					foreach (int fieldWithTextMigrationQueue in textMigrationQueueTables)
					{
						var pendingDocumentIdsForField = _textMigrationVerify.GetPendingMigrationFields(fieldWithTextMigrationQueue, documentIds);
						if (pendingDocumentIdsForField.Count > 0)
						{
							anyPendingDocuments = true;
						}

						pendingDocumentIds.Add(fieldWithTextMigrationQueue, pendingDocumentIdsForField);
					}
				}

				IDataGridWriter writer;
				if (linkDocuments && !hasMappedFields)
				{
					writer = new LinkedOnlyWriter(); // doesn't write to the primary data store
				}
				else
				{
					var actualWriter = new BulkDataGridWriter(_dgContext, artifactTypeID, appID, errorManager, logger, fields);
					writer = new ByteMeasuringWriter(actualWriter, _measurements);
				}

				IDataGridRecordBuilder recordBuilder = new FileSystemRecordBuilder(writer, Relativity.Data.Config.DataGridConfiguration.DataGridImportSmallFieldThreshold, Relativity.Data.Config.DataGridConfiguration.DataGridWriteParallelism);
				var foundIdentifiers = new HashSet<string>();
				try
				{
					loader.ReadDataGridDocumentsFromDataReader(recordBuilder, dataGridMappings, foundIdentifiers).Wait();
				}
				catch (AggregateException ex)
				{
					throw new Exception($"Write to Data Grid failed: {ex.InnerExceptions.First().Message}", ex.InnerExceptions.First());
				}

				correlationLogger.LogDebug("WriteToDataGrid for {totalCount} documents completed with {errorCount} errors and {validationCount} validation errors.", 
					foundIdentifiers.Count, errorManager.ErrorMessages.Keys.Count(), errorManager.ValidationStatuses.Keys.Count());

				// REL-108860: Something is happening where records don't end up in the data grid temp file.
				// This emits document level errors instead of crashing
				IEnumerable<DataGridImportIdentity> docsWithNullGuids;
				docsWithNullGuids = dataGridMappings.ExceptByDocumentIdentifier(foundIdentifiers);
				if (docsWithNullGuids.Any())
				{
					logger.LogError("Some identifiers expected in the data file were not found: [{@missingIdentifiers}]", docsWithNullGuids);
					logger.LogWarning("Document Identifiers with null guids: {nullGuids}", docsWithNullGuids.Select(doc => doc.DocumentIdentifier));
					logger.LogWarning("Import Identifiers with null guids: {nullGuids}", docsWithNullGuids.Select(doc => doc.ImportID));
					foreach (DataGridImportIdentity identifier in docsWithNullGuids)
					{
						errorManager.AddErrorStatuses(identifier.ArtifactID, "Unexpected Null Field. No Data Grid text has been written. Please retry").Wait();
					}
				}
				// End REL-108860

				if (anyPendingDocuments)
				{
					foreach (int fieldWithTextMigrationQueue in textMigrationQueueTables)
					{
						var documentErrors = new List<int>();
						if (errorManager.FieldErrors.ContainsKey(fieldWithTextMigrationQueue))
						{
							documentErrors = errorManager.FieldErrors[fieldWithTextMigrationQueue];
						}

						var pendingDocuments = pendingDocumentIds[fieldWithTextMigrationQueue].Except(documentErrors).ToList();
						_textMigrationVerify.UpdatePendingMigrationFields(fieldWithTextMigrationQueue, pendingDocuments);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Something went wrong");
				exception = ex;
			}

			_results = new DataGridImportResults()
			{
				ErrorResults = errorManager.ErrorMessages,
				ValidationErrorResults = errorManager.ValidationStatuses,
				OverallException = exception
			};
			kCura.Utility.InjectionManager.Instance.Evaluate("3335324c-822f-4190-9bb6-552d0aac4a76");
			logger.LogVerbose("Ending WriteToDataGrid");
		}

		public void UpdateErrors(string tempTableName, string statusColumnName, bool filterByOrder, ILog correlationLogger)
		{
			correlationLogger.LogVerbose("Starting UpdateErrorsAndMapping");
			kCura.Utility.InjectionManager.Instance.Evaluate("a6eb8741-6d73-40d1-9ad9-1fa944b5367e");
			try
			{
				if (_results.OverallException != null)
				{
					correlationLogger.LogDebug(_results.OverallException, "Data Grid Batch failed because of exception");
					FlagDataGridBatchAsError(_context, _results.OverallException.Message, tempTableName, statusColumnName);
					return;
				}

				foreach (ImportStatus status in _results.ValidationErrorResults.Keys)
				{
					if (_results.ValidationErrorResults.Item(status).Count > 0)
					{
						var importIDs = _results.ValidationErrorResults.Item(status);
						correlationLogger.LogDebug("Document level validation errors for IDS: {importIDs} with status: {status}", importIDs, status);
						DataGridImportHelper.FlagDataGridErrors(_context, importIDs, status, tempTableName, statusColumnName);
					}
				}

				foreach (string errorBatch in _results.ErrorResults.Keys)
				{
					if (_results.ErrorResults.Item(errorBatch).Count > 0)
					{
						var importIDs = _results.ErrorResults.Item(errorBatch);
						correlationLogger.LogDebug("Document level errors for IDS: {importIDs} for batch: {batch}", importIDs, errorBatch);
						DataGridImportHelper.FlagDataGridErrors(_context, importIDs, errorBatch, tempTableName, statusColumnName);
					}
				}
			}
			finally
			{
				kCura.Utility.InjectionManager.Instance.Evaluate("1118844f-30d2-477d-86e1-366e2210d54f");
				correlationLogger.LogVerbose("Ending UpdateErrorsAndMapping");
			}
		}

		#region SQL
		private static void FlagDataGridBatchAsError(kCura.Data.RowDataGateway.BaseContext context, string errorMessage, string tableName, string statusColumnName)
		{
			string errorSql = @"/*log errors for all documents in the batch due to an exception*/
UPDATE
	[Resource].[{0}]
SET
	[kCura_Import_DataGridException] = @dataGridError,
	[{1}] = [{1}] | @dataGridErrorFlag";

			string batchErrorUpdateSql = string.Format(errorSql, tableName, statusColumnName);
			context.ExecuteNonQuerySQLStatement(batchErrorUpdateSql, new[] { new System.Data.SqlClient.SqlParameter("@dataGridError", errorMessage), new System.Data.SqlClient.SqlParameter("@dataGridErrorFlag", (int)Relativity.MassImport.ImportStatus.DataGridExceptionOccurred) });
		}

		private static void FlagDataGridErrors(kCura.Data.RowDataGateway.BaseContext context, IEnumerable<long> objectImportIDs, Relativity.MassImport.ImportStatus errorFlag, string tableName, string statusColumnName)
		{
			if (!objectImportIDs.Any())
			{
				return;
			}

			string errorSql = @"/*create errors for documents with validation failures*/
UPDATE
	[Resource].[{0}]
SET
	[{2}] = [{2}] | @dataGridErrorFlag
WHERE
	[{0}].[kCura_Import_ID] IN ({1})";

			string batchErrorUpdateSql = string.Format(errorSql, tableName, string.Join(",", objectImportIDs), statusColumnName);
			context.ExecuteNonQuerySQLStatement(batchErrorUpdateSql, new[] { new System.Data.SqlClient.SqlParameter("@dataGridErrorFlag", (int)errorFlag) });
		}

		private static void FlagDataGridErrors(kCura.Data.RowDataGateway.BaseContext context, IEnumerable<long> objectImportIDs, string errorMessage, string tableName, string statusColumnName)
		{
			if (!objectImportIDs.Any())
			{
				return;
			}

			string errorSql = @"/*create errors for documents with error messages*/
UPDATE
	[Resource].[{0}]
SET
	[kCura_Import_DataGridException] = @dataGridError,
	[{2}] = [{2}] | @dataGridErrorFlag
WHERE
	[{0}].[kCura_Import_ID] IN ({1})";

			string batchErrorUpdateSql = string.Format(errorSql, tableName, string.Join(",", objectImportIDs), statusColumnName);
			context.ExecuteNonQuerySQLStatement(batchErrorUpdateSql, new[] { new System.Data.SqlClient.SqlParameter("@dataGridError", errorMessage), new System.Data.SqlClient.SqlParameter("@dataGridErrorFlag", (int)Relativity.MassImport.ImportStatus.DataGridExceptionOccurred) });
		}
		#endregion

		private class DataGridImportResults
		{
			public IDListDictionary<Relativity.MassImport.ImportStatus> ValidationErrorResults { get; set; }
			public IDListDictionary<string> ErrorResults { get; set; }
			public Exception OverallException { get; set; }
		}
	}
}