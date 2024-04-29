using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using Relativity.MassImport.Data.DataGridWriteStrategy;

namespace Relativity.MassImport.Data
{
	// TODO: change to internal, https://jira.kcura.com/browse/REL-477112 
	public class DataGridReader
	{
		private readonly Relativity.Data.DataGridContext _dgContext;
		private readonly BaseContext _readContext;
		private readonly System.Data.Common.DbDataReader _reader;
		private readonly DataGridReaderOptions _options;
		private readonly Logging.ILog _correlationLogger;
		private readonly List<FieldInfo> _mismatchedDataGridFields;
		private readonly IDataGridSqlTempReader _sqlTempReader;
		private readonly bool _anyMismatchedFields;

		public DataGridReader(Relativity.Data.DataGridContext dgContext, BaseContext readContext, DataGridReaderOptions options, System.Data.Common.DbDataReader reader, Logging.ILog correlationLogger, List<FieldInfo> mismatchedDataGridFields, IDataGridSqlTempReader sqlTempReader)
		{
			_dgContext = dgContext;
			_readContext = readContext;
			_reader = reader;
			_options = options;
			_correlationLogger = correlationLogger;
			_mismatchedDataGridFields = mismatchedDataGridFields;
			_anyMismatchedFields = _mismatchedDataGridFields.Count > 0;
			_sqlTempReader = sqlTempReader;
		}

		public async Task ReadDataGridDocumentsFromDataReader(IDataGridRecordBuilder builder, Relativity.Data.DataGridMappingMultiDictionary dataGridMappings, HashSet<string> foundIdentifiers, HashSet<string> skippedIdentifiers)
		{
			// Do change the value initialized to methodName variable if the method name is changed else the logs will have a different method name in them.
			string methodName = "ReadDataGridDocumentsFromDataReader";
			object methodNameObject = methodName;
			var logger = _correlationLogger.ForContext("Method", methodNameObject, true);
			logger.LogVerbose("Starting");

			kCura.Utility.InjectionManager.Instance.Evaluate("203FB622-92DB-407D-9E42-13F939048334");

			// If there is nothing in the reader for Data Grid and there are mismatched fields, then we may be dealing 
			// a DG field that currently only stored in SQL
			if (_anyMismatchedFields && !_reader.HasRows)
			{
				logger.LogWarning("Text Migration in progress - Mismatched Data Grid field found");
				await ReadFieldsFromSql(builder, dataGridMappings, foundIdentifiers, logger);
				return;
			}

			using (var dataReader = _reader)
			{
				if (!_options.MappedDataGridFields.Any() && !_options.LinkDataGridRecords)
				{
					logger.LogDebug("ReadDataGridDocumentsFromDataReader: Nothing to do");
					dataReader.Close();
					return;
				}

				var batchGUID = Guid.NewGuid();
				while (dataReader.Read())
				{
					string identifier = Convert.ToString(dataReader[_options.IdentifierColumnName]);
					logger.LogDebug("Identifier: {identifier}", identifier);

					if (!dataGridMappings.ContainsByDocumentIdentifier(identifier))
					{
						logger.LogDebug("Skipping unmapped record: {identifier}", identifier);
                        if (!string.IsNullOrEmpty(identifier))
                        {
                            skippedIdentifiers.Add(identifier);
                        }
                        continue; // skip unmapped records
					}

					// Check off the items when we see them
					foundIdentifiers.Add(identifier);

					int artifactId = (await dataGridMappings.LookupRecordByDocumentIdentifier(identifier)).ArtifactID;

					await builder.AddDocument(artifactId, Relativity.ArtifactType.Document.ToString().ToLower(), batchGUID.ToString());

					foreach (FieldInfo field in _options.MappedDataGridFields)
					{
						var dgField = _dgContext.DataGridMappingLookupManager.GetDataGridFieldInfo(field.ArtifactID, _readContext);
						if (_anyMismatchedFields && _mismatchedDataGridFields.Any(f => f.ArtifactID == field.ArtifactID))
						{
							// If the field is set to DG in the DB, but the caller of IAPI thinks it not DG enabled, the field will be stored in SQL and we have to retrieve it
							logger.LogWarning("Text Migration in progress - Mismatched Data Grid field found {fieldName}", field.DisplayName);
							string columnName = GetMismatchColumnName(field);
							string value = _sqlTempReader.GetFieldFromSqlAsString(_options.SqlTempTableName, _options.IdentifierColumnName, identifier, columnName);
							await builder.AddField(dgField, value, false);
						}
						else if (_options.ReadFullTextFromFileLocation && field.Category == FieldCategory.FullText)
						{
							string filePath = Convert.ToString(dataReader[field.GetColumnName()]);
							logger.LogDebug("For {identifier}, reading {fieldName} from file: {file}", identifier, field.DisplayName, filePath);
							await builder.AddField(dgField, filePath, true);
						}
						else
						{
							logger.LogDebug("For {identifier}, {fieldName} is being streamed from disk", identifier, field.DisplayName);
							var stream = dataReader.GetStream(dataReader.GetOrdinal(field.GetColumnName()));
							await builder.AddField(dgField, stream);
						}
					}
				}

				dataReader.Close();
			}

			await builder.Flush();
			logger.LogVerbose("Ending");
		}

		private async Task ReadFieldsFromSql(IDataGridRecordBuilder builder, Relativity.Data.DataGridMappingMultiDictionary dataGridMappings, HashSet<string> foundIdentifiers, Logging.ILog logger)
		{
			var identifiers = _sqlTempReader.GetAllIdentifiersFromSql(_options.SqlTempTableName, _options.IdentifierColumnName).ToList();
			var batchGUID = Guid.NewGuid();
			foreach (string identifier in identifiers)
			{
				foundIdentifiers.Add(identifier);
				int artifactId = (await dataGridMappings.LookupRecordByDocumentIdentifier(identifier)).ArtifactID;

				await builder.AddDocument(artifactId, Relativity.ArtifactType.Document.ToString().ToLower(), batchGUID.ToString());

				foreach (FieldInfo mismatchedField in _mismatchedDataGridFields)
				{
					var dgField = _dgContext.DataGridMappingLookupManager.GetDataGridFieldInfo(mismatchedField.ArtifactID, _readContext);
					string columnName = GetMismatchColumnName(mismatchedField);
					string value = _sqlTempReader.GetFieldFromSqlAsString(_options.SqlTempTableName, _options.IdentifierColumnName, identifier, columnName);
					await builder.AddField(dgField, value, false);
				}
			}

			await builder.Flush();
		}

		public IEnumerable<FieldInfo> GetDataGridFields()
		{
			return _options.MappedDataGridFields.ToList();
		}

		private string GetMismatchColumnName(FieldInfo fieldInfo)
		{
			string columnName = fieldInfo.GetColumnName();
			// If image import, then we only ever have one full
			if (_options.IsImageFullTextImport)
			{
				columnName = Image.FullTextColumnName;
			}

			return columnName;
		}
	}
}