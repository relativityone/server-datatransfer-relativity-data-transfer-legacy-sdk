using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Relativity.Core;
using Relativity.Core.Exception;
using Relativity.Core.Service;
using Relativity.MassImport.Api;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables
{
	// TODO: use this class instead of PopulateStagingTablesUsingActionStage, implement IPipelineStage
	internal class PopulateStagingTablesStage<T>
	{
		private readonly BaseContext _context;
		private readonly IEnumerable<MassImportArtifact> _artifacts;
		private readonly MassImportSettings _settings;
		private readonly IArtifactManager _artifactManager;

		private readonly List<int> _mappedFieldsForReaderIndexes = new List<int>();
		private readonly List<int> _codeFieldIndexes = new List<int>();
		private readonly List<int> _multiObjectFieldIndexes = new List<int>();
		private int _identifierFieldIndex = -1;

		public PopulateStagingTablesStage(BaseContext context, IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings, IArtifactManager artifactManager)
		{
			_context = context;
			_artifacts = artifacts;
			_settings = settings;
			_artifactManager = artifactManager;
		}

		public void Execute(string nativeTableName, string codeTableName, string objectsTableName)
		{
			AnalyzeFieldTypes();
			PopulateStagingTables(nativeTableName, codeTableName, objectsTableName);
		}

		private void AnalyzeFieldTypes()
		{
			for (var fieldIndex = 0; fieldIndex <= _settings.MappedFields.Count() - 1; fieldIndex++)
			{
				MassImportField field = (MassImportField) _settings.MappedFields[fieldIndex];
				if (field.Category == FieldCategory.Identifier)
				{
					_identifierFieldIndex = fieldIndex;
				}

				if (field.Type == FieldTypeHelper.FieldType.MultiCode || field.Type == FieldTypeHelper.FieldType.Code)
				{
					_codeFieldIndexes.Add(fieldIndex);
					_mappedFieldsForReaderIndexes.Add(-1);
				}
				else if (field.Type == FieldTypeHelper.FieldType.Objects)
				{
					_multiObjectFieldIndexes.Add(fieldIndex);
					_mappedFieldsForReaderIndexes.Add(-1);
				}
				else if (!field.EnableDataGrid)
				{
					_mappedFieldsForReaderIndexes.Add(fieldIndex);
				}
			}

			if (_identifierFieldIndex == -1)
			{
				throw new InvalidFieldValueException($"Object reference ({nameof(_identifierFieldIndex)}) Not Set To an instance Of an Object.");
			}
		}

		private void PopulateStagingTables(string nativeTableName, string codeTableName, string objectsTableName)
		{
			Tuple<List<MassImportCodeTempTableRow>, List<MassImportObjectsTempTableRow>> additionalLoadTables = LoadObjectAndCodeRows(_artifacts);

			// Populate MassImport temp tables using SQL Bulk Copy
			SqlBulkCopyForMassCreate(_context, _artifacts, nativeTableName, _mappedFieldsForReaderIndexes, _context.AppArtifactID);
			if (_codeFieldIndexes.Count > 0)
			{
				SqlBulkCopyCodeTempTableForMassCreate(_context, additionalLoadTables.Item1.ToArray(), codeTableName);
			}

			if (_multiObjectFieldIndexes.Count > 0)
			{
				SqlBulkCopyObjectsTempTableForMassCreate(_context, additionalLoadTables.Item2.ToArray(), objectsTableName);
			}
		}

		private Tuple<List<MassImportCodeTempTableRow>, List<MassImportObjectsTempTableRow>> LoadObjectAndCodeRows(IEnumerable<MassImportArtifact> artifacts)
		{
			List<MassImportCodeTempTableRow> codeTempTableRows = new List<MassImportCodeTempTableRow>();
			List<MassImportObjectsTempTableRow> objectsTempTableRows = new List<MassImportObjectsTempTableRow>();

			if (_codeFieldIndexes.Count > 0 || _multiObjectFieldIndexes.Count > 0)
			{
				foreach (MassImportArtifact artifactRequest in artifacts)
				{
					object identifierValueObject = artifactRequest.FieldValues[_identifierFieldIndex];
					if (identifierValueObject == null)
					{
						throw new NullReferenceException($"Object reference ({nameof(identifierValueObject)}) Not Set To an instance Of an Object.");
					}
					string identifierValue = identifierValueObject.ToString();
					if (artifactRequest.ParentFolderId < 1)
					{
						// If _defaultParentArtifactId Is Nothing Or _defaultParentArtifactId < 1 Then
						throw new InvalidFieldValueException($"ParentArtifactID was Not Set On Artifact With identifier {identifierValue}");
					}

					ProcessCodeFields(artifactRequest, codeTempTableRows);
					ProcessMultiObjectFields(artifactRequest, objectsTempTableRows);
				}
			}

			return new Tuple<List<MassImportCodeTempTableRow>, List<MassImportObjectsTempTableRow>>(codeTempTableRows, objectsTempTableRows);
		}

		private void ProcessCodeFields(MassImportArtifact artifact, List<MassImportCodeTempTableRow> codeTempTableRows)
		{
			foreach (int codeFieldIndex in _codeFieldIndexes)
			{
				object codeFieldValue = artifact.FieldValues[codeFieldIndex];
				if (codeFieldValue == null)
				{
					continue;
				}
				string identifierValue = artifact.FieldValues[_identifierFieldIndex].ToString();
				if (codeFieldValue is int singleCodeArtifactId)
				{
					codeTempTableRows.Add(new MassImportCodeTempTableRow(identifierValue, singleCodeArtifactId, _settings.MappedFields[codeFieldIndex].CodeTypeID));
				}
				else if (codeFieldValue is int[] codeArtifactIds)
				{
					foreach (int codeArtifactId in codeArtifactIds)
					{
						codeTempTableRows.Add(new MassImportCodeTempTableRow(identifierValue, codeArtifactId, _settings.MappedFields[codeFieldIndex].CodeTypeID));
					}
				}
				else
				{
					throw new InvalidFieldValueException(string.Format("Invalid value For field '{0}'", _settings.MappedFields[codeFieldIndex].DisplayName));
				}
			}
		}

		private void ProcessMultiObjectFields(MassImportArtifact artifact, List<MassImportObjectsTempTableRow> objectsTempTableRows)
		{
			foreach (int multiObjectFieldIndex in _multiObjectFieldIndexes)
			{
				object multiObjectFieldValue = artifact.FieldValues[multiObjectFieldIndex];
				if (multiObjectFieldValue == null)
				{
					continue;
				}
				string identifierValue = artifact.FieldValues[_identifierFieldIndex].ToString();
				int AssociativeArtifactTypeID = ((MassImportField) _settings.MappedFields[multiObjectFieldIndex]).AssociativeArtifactTypeID;
				int fieldArtifactId = _settings.MappedFields[multiObjectFieldIndex].ArtifactID;

				if (multiObjectFieldValue is int[] objectArtifactIds)
				{
					foreach (int objectArtifactId in objectArtifactIds)
					{
						// Preping so SQL Bulk Copy can populate the objects temp table for SQL Bulk Copy
						MassImportObjectsTempTableRow objectsTempTableRow = new MassImportObjectsTempTableRow(identifierValue, "", objectArtifactId, AssociativeArtifactTypeID, fieldArtifactId);
						objectsTempTableRows.Add(objectsTempTableRow);
					}
				}
				else
				{
					throw new InvalidFieldValueException(string.Format("Invalid value for field '{0}'", _settings.MappedFields[multiObjectFieldIndex].DisplayName));
				}
			}
		}

		private void SqlBulkCopyForMassCreate(BaseContext context, IEnumerable<MassImportArtifact> artifacts, string tableName, List<int> fieldIndexes, int appArtifactId)
		{
			IEnumerable<SqlBulkCopyColumnMapping> columnMappings = Enumerable.Range(0, 11 + fieldIndexes.Count).Select(columnIndex => new SqlBulkCopyColumnMapping(columnIndex, columnIndex));
			int rootCaseArtifactId = _artifactManager.GetRootArtifactID(_context);
			MassImportReader massCreateReader = new MassImportReader(columnMappings, artifacts, fieldIndexes.ToArray(), appArtifactId, rootCaseArtifactId);
			ExecuteSqlBulkCopy(context, columnMappings, massCreateReader, tableName);
		}

		private void SqlBulkCopyCodeTempTableForMassCreate(BaseContext context, MassImportCodeTempTableRow[] massCreateCodeTempTableRows, string tableName)
		{
			IEnumerable<SqlBulkCopyColumnMapping> columnMappings = Enumerable.Range(0, 3).Select(columnIndex => new SqlBulkCopyColumnMapping(columnIndex, columnIndex));
			MassImportReaderCodeTempTableReader massCreateReader = new MassImportReaderCodeTempTableReader(columnMappings, massCreateCodeTempTableRows);
			ExecuteSqlBulkCopy(context, columnMappings, massCreateReader, tableName);
		}

		private void SqlBulkCopyObjectsTempTableForMassCreate(BaseContext context, MassImportObjectsTempTableRow[] massCreateObjectsTempTableRows, string tableName)
		{
			IEnumerable<SqlBulkCopyColumnMapping> columnMappings = Enumerable.Range(0, 5).Select(columnIndex => new SqlBulkCopyColumnMapping(columnIndex, columnIndex));
			MassImportReaderObjectsTempTableReader massCreateReader = new MassImportReaderObjectsTempTableReader(columnMappings, massCreateObjectsTempTableRows);
			ExecuteSqlBulkCopy(context, columnMappings, massCreateReader, tableName);
		}

		private void ExecuteSqlBulkCopy(BaseContext context, IEnumerable<SqlBulkCopyColumnMapping> columnMappings, IDataReader reader, string tableName)
		{
			int timeoutInSeconds = Math.Max(kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout, 600);
			kCura.Data.RowDataGateway.SqlBulkCopyParameters sqlBulkCopyParameters = new kCura.Data.RowDataGateway.SqlBulkCopyParameters()
			{
				DestinationTableName = $"[Resource].[{tableName}]",
				Timeout = timeoutInSeconds
			};
			sqlBulkCopyParameters.ColumnMappings.AddRange(columnMappings);
			context.DBContext.ExecuteBulkCopy(reader, sqlBulkCopyParameters);
		}
	}
}