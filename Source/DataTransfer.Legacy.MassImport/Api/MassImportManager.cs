using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Core.Service.MassImport;
using Relativity.Data;
using Relativity.Data.MassImport;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Api
{
	/// <summary>
	/// This class is entry point for every MassImport use case.
	/// TODO: Join this class with existing MassImportManager and MassImporter classes after moving them to c#. Consider separating DG logic into another class.
	/// </summary>
	public class MassImportManager : IMassImportManager
	{
		private readonly ILog _logger;
		private readonly IArtifactManager _artifactManager;
		private readonly Relativity.Core.Service.MassImportManager _massImportManager;
		private readonly BaseContext _context;
		private readonly IHelper _helper;

		public MassImportManager(ILog logger, IArtifactManager artifactManager, BaseContext context,IHelper helper)
		{
			_logger = logger ?? Relativity.Logging.Log.Logger;
			_artifactManager = artifactManager;
			_context = context;
			_helper = helper;
			_massImportManager = new Relativity.Core.Service.MassImportManager(collectIDsOnCreate: true);
		}

		public Task<MassImportResults> RunMassImportAsync(IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings, CancellationToken cancel, IProgress<MassImportProgress> progress)
		{
			if (artifacts == null)
			{
				throw new ArgumentNullException(nameof(artifacts));
			}

			if (string.IsNullOrEmpty(settings.RunID))
			{
				settings.RunID = Guid.NewGuid().ToString().Replace("-", "_");
			}

			using (_logger?.LogContextPushProperty("CorrelationID", settings.RunID))
			using (_logger?.LogContextPushProperty("WorkspaceID", _context.AppArtifactID))
			{
				var affectedArtifactIds = new List<int>();
				MassImportResults massImportResults = new MassImportResults()
				{
					// TODO: return item errors (not required for OM)
					ItemErrors = null,
					RunId = settings.RunID,
					AffectedArtifactIds = affectedArtifactIds,
					KeyFieldToArtifactIdsMappings = settings.ReturnKeyFieldToArtifactIdsMappings
						? new Dictionary<string, IEnumerable<int>>()
						: null
				};

				try
				{
					foreach (var artifactsBatch in artifacts.Batch(settings.BatchSize))
					{
						if (cancel.IsCancellationRequested)
						{
							cancel.ThrowIfCancellationRequested();
							break;
						}

						MassImportManagerBase.MassImportResults batchResults = RunImport(artifactsBatch, settings);

						massImportResults.FilesProcessed += batchResults.FilesProcessed;
						massImportResults.ArtifactsProcessed += artifactsBatch.Count();
						massImportResults.ArtifactsCreated += batchResults.ArtifactsCreated;
						massImportResults.ArtifactsUpdated += batchResults.ArtifactsUpdated;
						if (batchResults is MassImportManagerBase.DetailedMassImportResults detailedBatchResults)
						{
							affectedArtifactIds.AddRange(detailedBatchResults.AffectedIDs);
							if (settings.ReturnKeyFieldToArtifactIdsMappings)
							{
								SyncKeyFieldToArtifactIdsMappingDictionary(
									massImportResults.KeyFieldToArtifactIdsMappings,
									detailedBatchResults.KeyFieldToArtifactIDMapping);
							}
						}

						progress?.Report(new MassImportProgress(massImportResults.AffectedArtifactIds));

						if (batchResults.ExceptionDetail != null)
						{
							massImportResults.ExceptionDetail = new MassImportExceptionDetail()
							{
								ExceptionMessage = batchResults.ExceptionDetail.ExceptionMessage,
								Details = batchResults.ExceptionDetail.Details,
								ExceptionFullText = batchResults.ExceptionDetail.ExceptionFullText,
								ExceptionTrace = batchResults.ExceptionDetail.ExceptionTrace,
								ExceptionType = batchResults.ExceptionDetail.ExceptionType
							};
							return Task.FromResult(massImportResults);
						}
					}
				}
				finally
				{
					try
					{
						_massImportManager.DisposeRunTempTables(_context, settings.RunID);
					}
					catch (System.Exception e)
					{
						_logger?.LogWarning(e, "Cleanup failed, workspaceID:{workspaceID} jobID:{jobID}",
							_context?.AppArtifactID, settings.RunID);
					}
				}

				return Task.FromResult(massImportResults);
			}
		}

		private void SyncKeyFieldToArtifactIdsMappingDictionary(IDictionary<string, IEnumerable<int>> target, Dictionary<string, List<int>> source)
		{
			foreach (KeyValuePair<string, List<int>> keyValuePair in source)
			{
				if (target.ContainsKey(keyValuePair.Key))
				{
					target[keyValuePair.Key] = target[keyValuePair.Key].Concat(keyValuePair.Value);
				}
				else
				{
					target.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}

		private MassImportManagerBase.MassImportResults RunImport(IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings)
		{
			int artifactTypeID = settings.ArtifactTypeID;

			// TODO: move to pipeline
			var popoluteStagingTablesStage = new PopulateStagingTablesStage<TableNames>(_context, artifacts, settings, _artifactManager);
			void LoadStagingTablesAction(TableNames tableNames) => popoluteStagingTablesStage.Execute(tableNames.Native, tableNames.Code, tableNames.Objects);

			MassImportManagerBase.MassImportResults internalResult;
			if (artifactTypeID == (int)ArtifactType.Document)
			{
				DataGridReader dataGridReader = null;
				dataGridReader = GetDataGridReader(artifacts, settings);
				internalResult = MassImporter.ImportNativesForObjectManager(_context, settings, LoadStagingTablesAction, dataGridReader);
			}
			else
			{
				var objectSettings = (Relativity.MassImport.DTO.ObjectLoadInfo)settings;
				internalResult = MassImporter.ImportObjectsForObjectManager(_context, objectSettings, true, LoadStagingTablesAction);
			}

			MassImportManagerBase.MassImportResults results = internalResult is MassImportManagerBase.DetailedMassImportResults detailedInternalResult ?
				new MassImportManagerBase.DetailedMassImportResults(detailedInternalResult) :
				new MassImportManagerBase.MassImportResults(internalResult);

			return results;
		}

		private DataGridReader GetDataGridReader(IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings)
		{
			var dataGridFields = settings.MappedFields.Where(field => field.EnableDataGrid).ToArray();
			if (!dataGridFields.Any())
			{
				return null;
			}

			int keyFieldIndex = Array.FindIndex(settings.MappedFields, field => field.ArtifactID == settings.KeyFieldArtifactID);
			DataGridContext dgContext = new DataGridContext(_context.DBContext, false);
			DataTable dataTable = GetDataTableWithDataGridFields(artifacts, settings, keyFieldIndex);
			DataGridReaderOptions options = new DataGridReaderOptions()
			{
				IdentifierColumnName = settings.MappedFields[keyFieldIndex].GetColumnName(),
				MappedDataGridFields = dataGridFields
			};

			return new DataGridReader(dgContext, _context.DBContext.Clone(), options, dataTable.CreateDataReader(), _logger, new List<FieldInfo>(), null);
		}

		private DataTable GetDataTableWithDataGridFields(IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings, int keyFieldIndex)
		{
			// TODO: remove DataTable, implement lazy load
			DataTable dataTable = new DataTable();

			string identifierColumnName = settings.MappedFields[keyFieldIndex].GetColumnName();
			IEnumerable<int> dataGridFieldIndexes = Enumerable.Range(0, settings.MappedFields.Length).Where(columnIndex => settings.MappedFields[columnIndex].EnableDataGrid);

			dataTable.Columns.Add(identifierColumnName);
			foreach (int fieldIndex in dataGridFieldIndexes)
			{
				dataTable.Columns.Add(settings.MappedFields[fieldIndex].GetColumnName(), typeof(byte[]));
			}

			foreach (MassImportArtifact artifact in artifacts)
			{
				DataRow row = dataTable.NewRow();
				row[identifierColumnName] = artifact.FieldValues[keyFieldIndex];

				foreach (Int32 fieldIndex in dataGridFieldIndexes)
				{
					string valueString = Convert.ToString(artifact.FieldValues[fieldIndex]);
					row[settings.MappedFields[fieldIndex].GetColumnName()] = System.Text.Encoding.Unicode.GetBytes(valueString);
				}

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}
	}
}