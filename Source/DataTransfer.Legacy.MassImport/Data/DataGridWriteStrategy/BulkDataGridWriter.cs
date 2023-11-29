using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.DataGrid;
using Relativity.DataGrid.Helpers;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class BulkDataGridWriter : IDataGridWriter
	{
		private readonly Relativity.Data.DataGridContext _dgContext;
		private readonly int _appID;
		private readonly Logging.ILog _correlationLogger;
		private readonly IEnumerable<FieldInfo> _fieldInformation;
		private readonly int _artifactTypeID;
		private readonly DataGridImportErrorManager _errorManager;

		public BulkDataGridWriter(Relativity.Data.DataGridContext dgContext, int artifactTypeID, int appID, DataGridImportErrorManager errorManager, Logging.ILog correlationLogger, IEnumerable<FieldInfo> fieldInformation)
		{
			_dgContext = dgContext;
			_artifactTypeID = artifactTypeID;
			_appID = appID;
			_correlationLogger = correlationLogger;
			_fieldInformation = fieldInformation;
			_errorManager = errorManager;
		}

		public async Task<IEnumerable<DataGridWriteResult>> Write(IEnumerable<IDataGridRecord> documentsToWrite)
		{
			IEnumerable<DataGridWriteResult> failureResults = null;
			try
			{
				_correlationLogger.LogVerbose("Writing to DataGrid");
				await Task.Yield();
				var results = _dgContext.WriteBulk(_appID, _artifactTypeID, documentsToWrite);
				_correlationLogger.LogVerbose("Wrote to DataGrid");
				return results;
			}
			catch (DataGridPartialResultsException<DataGridWriteResult> ex)
			{
				_correlationLogger.LogWarning(ex, "Not all of the documents ended up in DataGrid");
				failureResults = ex.DataGridResults;
			}

			foreach (DataGridWriteResult document in failureResults)
			{
				int artifactId = document.ArtifactID;
				var statuses = GetResultValidationStatuses(document);
				if (statuses.Any())
				{
					await _errorManager.AddValidationStatuses(artifactId, statuses);
				}

				string errors = GetDataGridErrorMessages(document);
				if (errors != null)
				{
					await _errorManager.AddErrorStatuses(artifactId, errors);
				}

				var fieldErrors = GetFieldErrorsFromResult(artifactId, document);
				if (fieldErrors.Count > 0)
				{
					await _errorManager.AddFieldErrors(fieldErrors);
				}
			}

			return failureResults;
		}

		public static string GetDataGridErrorMessages(DataGridWriteResult document)
		{
			var errorMessages = new System.Text.StringBuilder();
			if (document.ResultStatus == DataGridResult.Status.Error)
			{
				if (!string.IsNullOrEmpty(document.ResultsErrorMessage))
				{
					errorMessages.Append(document.ResultsErrorMessage + " ");
				}

				if (document.FieldWriteResults != null)
				{
					foreach (DataGridWriteResult.FieldResult fieldResult in document.FieldWriteResults)
					{
						if (fieldResult.ResultStatus == DataGridResult.Status.Error && !string.IsNullOrEmpty(fieldResult.ResultsErrorMessage))
						{
							errorMessages.AppendFormat("{2}Field [{0}] Error: {1}", fieldResult.FieldIdentifier, fieldResult.ResultsErrorMessage, errorMessages.Length == 0 ? string.Empty : Environment.NewLine);
						}
					}
				}
			}
			else
			{
				return null;
			}

			return errorMessages.ToString();
		}

		public static List<Relativity.MassImport.DTO.ImportStatus> GetResultValidationStatuses(DataGridWriteResult result)
		{
			var retval = new List<Relativity.MassImport.DTO.ImportStatus>();
			if (result.ResultStatus == DataGridResult.Status.ValidationError && (result.ResultsErrorMessage ?? "") == DataGridHelper.INVALID_RECORD_DATAGRIDID_MESSAGE)
			{
				retval.Add(Relativity.MassImport.DTO.ImportStatus.DataGridInvalidDocumentIDError);
			}

			if (result.FieldWriteResults != null)
			{
				foreach (DataGridWriteResult.FieldResult fieldResult in result.FieldWriteResults)
				{
					if (fieldResult.ResultStatus == DataGridResult.Status.ValidationError)
					{
						if ((fieldResult.ResultsErrorMessage ?? "") == DataGridHelper.INVALID_FIELD_NAME_MESSAGE)
						{
							retval.Add(Relativity.MassImport.DTO.ImportStatus.DataGridInvalidFieldNameError);
						}
						else if ((fieldResult.ResultsErrorMessage ?? "") == DataGridHelper.INVALID_FIELD_TOO_MUCH_DATA_MESSAGE)
						{
							retval.Add(Relativity.MassImport.DTO.ImportStatus.DataGridFieldMaxSizeExceeded);
						}
					}
				}
			}

			return retval.Distinct().ToList();
		}

		public Dictionary<int, List<int>> GetFieldErrorsFromResult(int artifactId, DataGridWriteResult result)
		{
			var fieldErrors = new Dictionary<int, List<int>>();
			if (result.FieldWriteResults != null)
			{
				foreach (DataGridWriteResult.FieldResult fieldResult in result.FieldWriteResults)
				{
					if (fieldResult.ResultStatus != DataGridResult.Status.Verified)
					{
						var fieldInfo = _fieldInformation.FirstOrDefault(field => string.Compare(field.DisplayName, fieldResult.FieldIdentifier, StringComparison.CurrentCultureIgnoreCase) == 0);
						if (fieldInfo != null && fieldErrors.ContainsKey(fieldInfo.ArtifactID))
						{
							fieldErrors[fieldInfo.ArtifactID].Add(artifactId);
						}
						else if (fieldInfo != null)
						{
							fieldErrors.Add(fieldInfo.ArtifactID, new List<int>());
							fieldErrors[fieldInfo.ArtifactID].Add(artifactId);
						}
					}
				}
			}

			return fieldErrors;
		}
	}
}