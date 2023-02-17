using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.Utility;
using Polly;
using Relativity.Data.MassImport;
using Relativity.DataGrid;
using Relativity.DataGrid.Helpers;
using Relativity.DataGrid.Helpers.DGFS;
using Relativity.DataGrid.Implementations.DGFS.ReadBackend;
using Relativity.Logging;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.DataGrid;
using Relativity.MassImport.Data.DataGridWriteStrategy;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.MassImport.Extensions;
using DGImportFileInfo = Relativity.MassImport.Data.DataGrid.DGImportFileInfo;
using DataTransfer.Legacy.MassImport.Data.Cache;
using System.Data.Common;
using DataTransfer.Legacy.MassImport.Toggles;
using Relativity.Toggles;
using Relativity.API;

namespace Relativity.MassImport.Data
{
	internal abstract class ObjectBase : IObjectBase, IDataGridInputReaderProvider
	{
		private readonly kCura.Data.RowDataGateway.BaseContext _context;
		public TableNames _tableNames;
		private string _auditRecordCollation = "";
		private int? _timeoutValue;
		private string _artifactTypeTableName = "";
		protected readonly int _artifactTypeID = 0;
		private Relativity.Data.DataGridContext _dgContext;
		private FieldInfo _fullTextField;
		private Relativity.MassImport.DTO.NativeLoadInfo _settings;
		private FieldInfo _identifierField;
		private const string _STATUS_COLUMN_NAME = "kCura_Import_Status";
		private const bool _FILTER_BY_ORDER = false;
		private DataGridImportHelper _dgImportHelper;
		private Relativity.Data.DataGridMappingMultiDictionary _dataGridMappings;
		private const int _retryCount = 3;
		private const int _retrySleepDurationMilliseconds = 500;
		private Lazy<List<FieldInfo>> _mismatchedDataGridFieldsLazy;
		protected const int TopFieldArtifactID = 0;

		#region Constructors
		protected ObjectBase(
			kCura.Data.RowDataGateway.BaseContext context,
			IQueryExecutor queryExecutor,
			Relativity.MassImport.DTO.NativeLoadInfo settings,
			NativeImportSql importSql,
			int artifactTypeID,
			int importUpdateAuditAction,
			ImportMeasurements importMeasurements,
			ColumnDefinitionCache columnDefinitionCache,
			int caseSystemArtifactId,
			IHelper helper,
			bool useLegacyDG = true)
		{
			_context = context;
			QueryExecutor = queryExecutor;
			_tableNames = new TableNames(settings.RunID);
			_settings = settings;
			_artifactTypeID = artifactTypeID;
			_mismatchedDataGridFieldsLazy = new Lazy<List<FieldInfo>>(GetMismatchedDataGridFields);
			_fullTextField = settings.MappedFields != null ? settings.MappedFields.FirstOrDefault(x => x.Category == FieldCategory.FullText) : null;
			ImportSql = importSql;
			ImportUpdateAuditAction = importUpdateAuditAction;
			ImportMeasurements = importMeasurements;
			ColumnDefinitionCache = columnDefinitionCache;
			CaseSystemArtifactId = caseSystemArtifactId;
			if (settings.HasDataGridWorkToDo)
			{
				var dgSqlFactory = new DataGridSqlContextFactory((i) => Context.Clone());
				DGFieldInformationLookupFactory = new Relativity.MassImport.Data.DataGrid.DGFieldInformationLookupFactory(new FieldInformationLookupFactory(dgSqlFactory));
				_dataGridMappings = new Relativity.Data.DataGridMappingMultiDictionary();
				DGRelativityRepository = new Relativity.MassImport.Data.DataGrid.DGRelativityRepository();
				var fml = new FieldMappingLookup(dgSqlFactory);
				var dgfsSqlReader = new SqlBackend(Relativity.Data.Config.DataGridConfiguration, dgSqlFactory);
				DataGridBufferPool dataGridBufferPool = null;

				DataGridContextBase dataGridContextBase;

				if (ToggleProvider.Current.IsEnabled<DisableCALToggle>() || useLegacyDG)
				{
					ImportMeasurements.StartMeasure("DataGridContextLegacyInitialization");

					dataGridContextBase = new FileSystemContext("document", ref dataGridBufferPool, Relativity.Data.Config.DataGridConfiguration, DGRelativityRepository, _dataGridMappings, DGFieldInformationLookupFactory, fml, dgfsSqlReader);

					ImportMeasurements.StartMeasure("DataGridContextLegacyInitialization");
				}
				else
				{
					ImportMeasurements.StartMeasure("DataGridContextInitialization");

					var fileHelper = new Relativity.DataGrid.Helpers.DGFS.ADLS.DataGridFileHelper(Relativity.Data.Config.DataGridConfiguration, helper);
					dataGridContextBase = new FileSystemContext("document", ref dataGridBufferPool, Relativity.Data.Config.DataGridConfiguration, DGRelativityRepository, _dataGridMappings, DGFieldInformationLookupFactory, fml, dgfsSqlReader, fileHelper);

					ImportMeasurements.StopMeasure("DataGridContextInitialization");
				}

				_dgContext = new Relativity.Data.DataGridContext(dataGridContextBase);
				_dgImportHelper = new DataGridImportHelper(_dgContext, Context, ImportMeasurements, new Relativity.Data.TextMigrationVerifier(Context));
			}
		}
		#endregion

		#region Accessors
		protected Relativity.MassImport.DTO.NativeLoadInfo Settings => _settings;

		protected NativeImportSql ImportSql { get; set; }

		protected FieldInfo IdentifierField => _identifierField ?? (_identifierField = GetIdentifierField());

		protected IQueryExecutor QueryExecutor { get; }

		protected string ArtifactTypeTableName
		{
			get
			{
				if (string.IsNullOrEmpty(_artifactTypeTableName))
				{
					_artifactTypeTableName = GetArtifactTypeTableNameFromArtifactTypeId();
				}
				return _artifactTypeTableName;
			}
		}

		protected int ArtifactTypeID => _artifactTypeID;

		public DGRelativityRepository DGRelativityRepository { get; set; }
		public DataGrid.DGFieldInformationLookupFactory DGFieldInformationLookupFactory { get; set; }

		public int QueryTimeout
		{
			get
			{
				if (!_timeoutValue.HasValue)
				{
					_timeoutValue = InstanceSettings.MassImportSqlTimeout;
				}
				return _timeoutValue.Value;
			}

			set => _timeoutValue = value;
		}

		protected ColumnDefinitionCache ColumnDefinitionCache { get; private set; }
		public int CaseSystemArtifactId { get; private set; }

		public string TableNameNativeTemp => _tableNames.Native;

		public string TableNameFullTextTemp => _tableNames.FullText;

		public string TableNameCodeTemp => _tableNames.Code;

		public string TableNameObjectsTemp => _tableNames.Objects;

		protected string AuditRecordDetailsCollation
		{
			get
			{
				if (string.IsNullOrEmpty(_auditRecordCollation))
				{
					_auditRecordCollation = ExecuteSqlStatementAsScalar<string>("SELECT collation_name FROM sys.columns WHERE [name] = 'Details' AND [object_id] = OBJECT_ID('[EDDSDBO].[AuditRecord]')");
				}

				return _auditRecordCollation;
			}
		}

		public string RunID => _tableNames.RunId;

		protected kCura.Data.RowDataGateway.BaseContext Context => _context;

		protected string FullTextFieldColumnName => _fullTextField?.GetColumnName();

		protected bool LoadImportedFullTextFromServer => Settings.LoadImportedFullTextFromServer && _fullTextField != null && !_fullTextField.EnableDataGrid;

		protected int WorkspaceArtifactId
		{
			get
			{
				int workspaceId = -1;
				int.TryParse(Context.Database.Replace("EDDS", ""), out workspaceId);
				return workspaceId;
			}
		}

		protected List<FieldInfo> MismatchedDataGridFields => _mismatchedDataGridFieldsLazy.Value;

		public ImportMeasurements ImportMeasurements { get; set; }
		#endregion

		public InlineSqlQuery PopulatePartTable()
		{
			return ImportSql.PopulatePartTable(_tableNames, ArtifactTypeTableName, TopFieldArtifactID, GetKeyField().GetColumnName());
		}

		public InlineSqlQuery PopulateParentTable()
		{
			return ImportSql.PopulateParentTable(_tableNames);
		}

		public InlineSqlQuery ManageCheckAddingPermissions(int userID)
		{
			return MassImportSqlHelper.CheckAddingObjectPermission(_tableNames, userID, ArtifactTypeID);
		}

		public InlineSqlQuery ManageUpdateOverlayPermissions(int userID)
		{
			return ImportSql.UpdateOverlayPermissions(_tableNames, ArtifactTypeID, userID, TopFieldArtifactID);
		}

		public InlineSqlQuery ManageCheckParentIsFolder()
		{
			return MassImportSqlHelper.CheckParentIsFolder(_tableNames);
		}

		public void ExecuteQueryAndSendMetrics(ISqlQueryPart sql)
		{
			using (new QueryMetricsCollector(Context, ImportMeasurements))
			{
				Context.ExecuteNonQuerySQLStatement(sql.BuildQuery(), QueryTimeout);
			}	
		}

		public void SqlInfoMessageEventHandler(object sender, SqlInfoMessageEventArgs args)
		{
			ImportMeasurements.ParseTimeStatistics(args.Message);
		}

		protected abstract string GetArtifactTypeTableNameFromArtifactTypeId();

		public void CreateAssociatedObjects(int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit)
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.SecondaryArtifactCreationTime.Start();
			InjectionManager.Instance.Evaluate("2befb863-e597-49be-a166-55e8291cbabf");
			if (!auditUserId.HasValue)
			{
				auditUserId = userID;
			}
			foreach (FieldInfo field in Settings.MappedFields)
			{
				if (field.Type == FieldTypeHelper.FieldType.Object)
				{
					ProcessSingleObjectField(field, userID, auditUserId, requestOrigination, recordOrigination, performAudit);
				}
				else if (field.Type == FieldTypeHelper.FieldType.Objects)
				{
					ProcessMultiObjectField(field, userID, auditUserId, requestOrigination, recordOrigination, performAudit);
				}
			}

			InjectionManager.Instance.Evaluate("077bbb8e-680f-48cb-b807-478a88645c14");
			ImportMeasurements.StopMeasure();
			ImportMeasurements.SecondaryArtifactCreationTime.Stop();
		}

		public void ProcessMultiObjectField(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit)
		{
			if ((int?)field.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ObjectFieldContainsArtifactId == true)
			{
				VerifyExistenceOfAssociatedObjectsForMultiObjectByArtifactId(field, userID, auditUserId);
			}
			else
			{
				CreateAssociatedObjectsForMultiObjectFieldByName(field, userID, requestOrigination, recordOrigination, performAudit);
			}
		}

		public void ProcessSingleObjectField(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit)
		{
			if ((int?)field.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ObjectFieldContainsArtifactId == true)
			{
				VerifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactId(field, userID, auditUserId);
			}
			else
			{
				CreateAssociatedObjectsForSingleObjectFieldByName(field, userID, auditUserId, requestOrigination, recordOrigination, performAudit);
			}
		}

		public abstract void CreateAssociatedObjectsForSingleObjectFieldByName(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit);

		public virtual void VerifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactId(FieldInfo field, int userID, int? auditUserId)
		{
			string idFieldLinkArg = ColumnDefinitionCache[field.ArtifactID].LinkArg;
			string associatedObjectTable = Relativity.Data.FieldHelper.GetColumnName(ColumnDefinitionCache[field.ArtifactID].ObjectTypeName);

			string query = AssociatedObjectsValidationSql.ValidateAssociatedObjectsReferencedByArtifactIdExist(_tableNames, field, associatedObjectTable, idFieldLinkArg);
			ExecuteNonQuerySQLStatement(query);
		}

		public virtual void CreateAssociatedObjectsForMultiObjectFieldByName(FieldInfo field, int userID, string requestOrigination, string recordOrigination, bool performAudit)
		{
			string objectTypeName = ColumnDefinitionCache[field.ArtifactID].ObjectTypeName;
			string associatedObjectTable = Relativity.Data.FieldHelper.GetColumnName(objectTypeName);
			string idFieldColumnName = ColumnDefinitionCache[field.ArtifactID].ColumnName;
			int artifactTypeID = ColumnDefinitionCache[field.ArtifactID].ObjectTypeDescriptorArtifactTypeID;

			UpdateSynclockSensitiveMultiObjectArtifacts(field, userID, associatedObjectTable, idFieldColumnName, artifactTypeID, requestOrigination, recordOrigination, performAudit);
		}

		protected virtual void UpdateSynclockSensitiveMultiObjectArtifacts(FieldInfo field, int userID, string associatedObjectTable, string idFieldColumnName, int artifactTypeID, string requestOrigination, string recordOrigination, bool performAudit)
		{
			UpdateKnownArtifacts(field, associatedObjectTable, idFieldColumnName, artifactTypeID.ToString());
			CheckForChildAssociatedObjects(artifactTypeID, field.DisplayName, field.ArtifactID);

			int parentId = ColumnDefinitionCache.TopLevelParentArtifactId;
			int parentAccessControlListId = ColumnDefinitionCache.TopLevelParentAccessControlListId;

			string sql = ImportSql.CreateMultipleAssociativeObjects(_tableNames, associatedObjectTable, idFieldColumnName, GetKeyField().GetColumnName());

			if (artifactTypeID == ArtifactTypeID)
			{
				sql = sql.Replace("/* SelfReferencedMultiField */", ImportSql.InsertMultiSelfReferencedObjects(_tableNames.Native, _tableNames.Part, idFieldColumnName, parentAccessControlListId));
			}

			if (performAudit)
			{
				sql = sql.Replace("/* ImportAuditClause */", ImportSql.CreateAuditClauseForMultiObject(requestOrigination, recordOrigination));
			}

			var parameters = new[]
			{
				new SqlParameter("@userID", userID), 
				new SqlParameter("@artifactType", artifactTypeID), 
				new SqlParameter("@fieldID", field.ArtifactID), 
				new SqlParameter("@auditUserID", userID), 
				new SqlParameter("@parentId", parentId), 
				new SqlParameter("@parentAccessControlListId", parentAccessControlListId)
			};
			ExecuteNonQuerySQLStatement(sql, parameters);
		}

		public virtual void VerifyExistenceOfAssociatedObjectsForMultiObjectByArtifactId(FieldInfo field, int userID, int? auditUserId)
		{
			string importedIdentifierColumn = IdentifierField.GetColumnName();
			string objectTypeName = ColumnDefinitionCache[field.ArtifactID].ObjectTypeName;
			string associatedObjectTable = Relativity.Data.FieldHelper.GetColumnName(objectTypeName);
			string idFieldColumnName = ColumnDefinitionCache[field.ArtifactID].ColumnName;

			// create errors for associated objects that do not exist
			string query = ImportSql.VerifyExistenceOfAssociatedMultiObjects(_tableNames, importedIdentifierColumn, idFieldColumnName, associatedObjectTable, field);
			ExecuteNonQuerySQLStatement(query);
		}

		private void CheckForChildAssociatedObjects(int artifactTypeID, string fieldName, int fieldArtifactId)
		{
			int associatedParentId = ColumnDefinitionCache[fieldArtifactId].AssociatedParentID;

			if (associatedParentId != (int) ArtifactType.Case)
			{
				// Child object! WHOA
				long errorStatusCode = (long)(artifactTypeID == (int) ArtifactType.Document ? Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsDocument : Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsChild);
				string childObjectCreations = string.Format("SELECT DISTINCT [DocumentIdentifier] FROM [Resource].[{0}] WHERE [ObjectTypeID] = @artifactTypeID AND [ObjectArtifactID] = -1", _tableNames.Objects);
				string produceLineErrors = string.Format("UPDATE [Resource].[{0}] SET [kCura_Import_Status] = [kCura_Import_Status] | {1}, [kCura_Import_ErrorData] = '{4}' WHERE [{2}] in ({3})", 
					_tableNames.Native, (object)errorStatusCode, IdentifierField.GetColumnName(), childObjectCreations, fieldName);
				
				ExecuteNonQuerySQLStatement(produceLineErrors, new[] { new SqlParameter("@artifactTypeID", artifactTypeID) });

				// Delete the problem documents from the Objects creation (need the inner select to prevent the document from getting borked during objects import
				string sql = string.Format("DELETE FROM [Resource].[{0}] WHERE [DocumentIdentifier] in ({1})", _tableNames.Objects, childObjectCreations);
				ExecuteNonQuerySQLStatement(sql, new[] { new SqlParameter("@artifactTypeID", artifactTypeID) });
			}
		}

		protected void UpdateKnownArtifacts(FieldInfo field, string associatedObjectTable, string idFieldColumnName, string associatedArtifactTypeID)
		{
			try
			{
				// this method updates the temp artifact table to add ids for any known RDOs that may be about to be imported from a single or multi object field.
				string sql = new SerialSqlQuery(
					new InlineSqlQuery(ImportSql.ValidateReferencedObjectsAreNotDuplicated(_tableNames, GetKeyField().GetColumnName(), associatedObjectTable, idFieldColumnName, associatedArtifactTypeID, field.DisplayName)),
					new InlineSqlQuery(ImportSql.SetArtifactIdForExistingMultiObjects(_tableNames, GetKeyField().GetColumnName(), associatedObjectTable, idFieldColumnName, associatedArtifactTypeID))
					).ToString();
				ExecuteNonQuerySQLStatement(sql);
			}
			catch (kCura.Data.RowDataGateway.ExecuteSQLStatementFailedException ex)
			{
				if (ex.InnerException != null && ((SqlException)ex.InnerException).Number == 512)
				{
					string fieldType;
					switch (field.Type)
					{
						case FieldTypeHelper.FieldType.Objects:
							{
								fieldType = "multi-object";
								break;
							}

						case FieldTypeHelper.FieldType.Object:
							{
								fieldType = "single-object";
								break;
							}

						default:
							{
								fieldType = field.Type.ToString().ToLowerInvariant();
								break;
							}
					}

					// Enhance the error message when there are duplicate records.
					string message = $"Failed to create the associated {fieldType} artifact type '{ArtifactTypeID}' because " + $"the '{associatedObjectTable}' table contains duplicate records. Ensure that all " + $"'{idFieldColumnName}' fields are distinct or consider using field values that are more unique.";

					throw new kCura.Data.RowDataGateway.ExecuteSQLStatementFailedException(message, ex.ExecutedStatement, ex.SQLParameters, ex.InnerException);
				}
				else
				{
					throw;
				}
			}
		}

		public InlineSqlQuery ManageAppendErrors()
		{
			return ImportSql.AppendOnlyErrors(_tableNames);
		}

		public InlineSqlQuery ManageOverwriteErrors()
		{
			return ImportSql.OverwriteOnlyErrors(_tableNames);
		}

		public ISqlQueryPart ValidateIdentifiersAreNonEmpty()
		{
			var result = new SerialSqlQuery();

			bool isIdentifierFieldMapped = this.Settings.MappedFields.Any(x => x.ArtifactID == IdentifierField.ArtifactID);
			if (isIdentifierFieldMapped)
			{
				result.Add(ImportSql.ValidateIdentifierIsNonNull(_tableNames, IdentifierField.GetColumnName()));
			}

			var keyField = this.GetKeyField();
			bool isOverlayIdentifierDifferentThanIdentifier = keyField != null && keyField.ArtifactID != this.IdentifierField.ArtifactID;
			if (isOverlayIdentifierDifferentThanIdentifier)
			{
				result.Add(ImportSql.ValidateOverlayIdentifierIsNonNull(_tableNames, keyField.GetColumnName()));
			}

			return result;
		}

		private void CopyBulkLoadedFullText()
		{
			string updateSql = $@"UPDATE
	[{ ArtifactTypeTableName }]
SET
	[{ ArtifactTypeTableName }].[{ FullTextFieldColumnName }] = [{ _tableNames.FullText }].[{ FullTextFieldColumnName }]
FROM
	[{ ArtifactTypeTableName }]
	LEFT JOIN
	[Resource].[{ _tableNames.Native }]
		ON [{ ArtifactTypeTableName }].[ArtifactID] = [{ _tableNames.Native }].[ArtifactID]
	LEFT JOIN
	[Resource].[{ _tableNames.FullText }]
		ON [{ _tableNames.Native }].[kCura_Import_ID] = [{ _tableNames.FullText }].[kCura_Import_ID]
WHERE
	[{ _tableNames.Native }].[ArtifactID] IS NOT NULL
";
			ExecuteNonQuerySQLStatement(updateSql);
		}

		public void UpdateFullTextFromFileShareLocation()
		{
			if (LoadImportedFullTextFromServer)
			{
				ImportMeasurements.StartMeasure();
				CopyBulkLoadedFullText();
				ImportMeasurements.StopMeasure();
			}
		}

		public void CopyFullTextFromFileShareLocation()
		{
			if (LoadImportedFullTextFromServer)
			{
				ImportMeasurements.StartMeasure();
				var filePathResults = ExecuteSqlStatementAsDataTable(string.Format("SELECT [kCura_Import_ID], [{0}] FROM [Resource].[{1}] WHERE [kCura_Import_Status] = {2} AND [{0}] IS NOT NULL", 
					FullTextFieldColumnName, _tableNames.Native, (object)(long)Relativity.MassImport.DTO.ImportStatus.Pending));

				DbDataReader reader;
				if (ToggleProvider.Current.IsEnabled<DisableCALToggle>())
				{
					reader = new FullTextFileImportDataReader(filePathResults);
					Log.Logger.LogWarning("CAL is disabled");
				}
				else
				{
					reader = new FullTextFileImportCALDataReader(filePathResults);
				}
				
				var parameters = new kCura.Data.RowDataGateway.SqlBulkCopyParameters() { EnableStreaming = true, DestinationTableName = $"[Resource].[{_tableNames.FullText}]" };
				Context.ExecuteBulkCopy(reader, parameters);
				ImportMeasurements.StopMeasure();
			}
		}

		public void PopulateArtifactIdOnInitialTempTable(int userID, bool updateOverlayPermissions)
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string sqlStatement = ImportSql.PopulateArtifactIdColumnOnTempTable(_tableNames);
			ExecuteNonQuerySQLStatement(sqlStatement, new[] { new SqlParameter("@userID", userID) });
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}

		public void PopulateObjectsListTable()
		{
			ImportMeasurements.StartMeasure();
			foreach (FieldInfo field in Settings.MappedFields)
			{
				if (field.Type == FieldTypeHelper.FieldType.Objects)
				{
					string objectsFieldTable = ColumnDefinitionCache[field.ArtifactID].RelationalTableSchemaName;

					string containingObjectField = ColumnDefinitionCache[field.ArtifactID].ContainingObjectField;
					string newObjectField = ColumnDefinitionCache[field.ArtifactID].NewObjectField;

					string containingObjectIdentifierColumn = Settings.KeyFieldColumnName;

					string sql = ImportSql.SetObjectsFieldEntries();

					string fieldOverlayExpression = Helper.GetFieldOverlaySwitchStatement(Settings, field.Type, ColumnDefinitionCache[field.ArtifactID].OverlayMergeValues);

					string queryToRun = string.Format(
						sql, 
						objectsFieldTable, 
						containingObjectField, 
						newObjectField, 
						containingObjectIdentifierColumn, 
						_tableNames.Objects, 
						_tableNames.Native, 
						fieldOverlayExpression, 
						_tableNames.Map);
					
					var sqlParameters = new[]
					{
						new SqlParameter("@fieldID", field.ArtifactID)
					};
					ExecuteNonQuerySQLStatement(queryToRun, sqlParameters);
				}
			}

			ImportMeasurements.StopMeasure();
		}

		protected int ImportUpdateAuditAction { get; private set; }

		#region DataGrid
		public IDataReader CreateDataGridMappingDataReader()
		{
			string identifierFieldName = (from field in Settings.MappedFields
										  where field.ArtifactID == Settings.KeyFieldArtifactID
										  select field.GetColumnName()).First();
			string tempTableName = SqlNameHelper.GetSqlFriendlyName(_tableNames.Native);
			string sql = $@"
															SELECT [kCura_Import_ID], [{ tempTableName }].[{ identifierFieldName }], NULL, [kCura_Import_ID]
															FROM [Resource].[{ tempTableName }]
															WHERE [kCura_Import_Status] = { (long)Relativity.MassImport.DTO.ImportStatus.Pending }
";
			return ExecuteSQLStatementAsReader(sql);
		}

		public DataGridReader CreateDataGridInputReader(string bulkFileShareFolderPath, ILog correlationLogger)
		{
			correlationLogger.LogVerbose("Starting CreateDataGridTempFileReader");
			try
			{
				if (!Settings.HasDataGridWorkToDo)
				{
					return null;
				}

				ImportMeasurements.StartMeasure();
				ImportMeasurements.DataGridImportTime.Start();
				InjectionManager.Instance.Evaluate("d58d0eed-5175-421a-b405-24fdc1f72a3d");
				string identifierFieldName = (from field in Settings.MappedFields
											  where field.ArtifactID == Settings.KeyFieldArtifactID
											  select field.GetColumnName()).First();
				correlationLogger.LogDebug("CreateDataGridTempFileReader: IdentifierFieldName is: {IdentifierFieldName}", identifierFieldName);
				var options = new DataGridReaderOptions()
				{
					DataGridIDColumnName = "_DataGridID_",
					IdentifierColumnName = identifierFieldName,
					MappedDataGridFields = Settings.MappedFields.Where(f => f.EnableDataGrid).ToList(),
					LinkDataGridRecords = Settings.LinkDataGridRecords,
					ReadFullTextFromFileLocation = Settings.LoadImportedFullTextFromServer,
					SqlTempTableName = _tableNames.Native
				};
				string dataGridFilePath = System.IO.Path.Combine(bulkFileShareFolderPath, Settings.DataGridFileName);
				correlationLogger.LogDebug("CreateDataGridTempFileReader: DataGridFilePath is: {DataGridFilePath}", dataGridFilePath);

				var reader = new DataGridTempFileDataReader(options, Settings.BulkLoadFileFieldDelimiter, Settings.BulkLoadFileFieldDelimiter + Environment.NewLine, dataGridFilePath, correlationLogger);
				var sqlTempReader = new DataGridSqlTempReader(Context);
				var loader = new DataGridReader(_dgContext, Context.Clone(), options, reader, correlationLogger, MismatchedDataGridFields, sqlTempReader);
				return loader;
			}
			finally
			{
				ImportMeasurements.StopMeasure();
				ImportMeasurements.DataGridImportTime.Stop();
				correlationLogger.LogVerbose("Ending CreateDataGridTempFileReader");
				InjectionManager.Instance.Evaluate("1b679a91-db1c-49ad-a16d-a66742f25190");
			}
		}

		public bool IsDataGridInputValid()
		{
			return IsDataGridInputValid(Settings);
		}

		public static bool IsDataGridInputValid(Relativity.MassImport.DTO.NativeLoadInfo settings)
		{
			return !string.IsNullOrEmpty(settings.DataGridFileName) && SQLInjectionHelper.IsValidFileName(settings.DataGridFileName);
		}

		public void WriteToDataGrid(DataGridReader loader, int appID, string bulkFileShareFolderPath, ILog correlationLogger)
		{
			try
			{
				ImportMeasurements.StartMeasure();
				ImportMeasurements.DataGridImportTime.Start();
				if (!Settings.HasDataGridWorkToDo)
				{
					return;
				}

				using (var mappingReader = CreateDataGridMappingDataReader())
				{
					string indexName = DataGridHelper.GetWriteIndexName(appID, _artifactTypeID, Relativity.Data.Config.DataGridConfiguration.DataGridIndexPrefix);
					_dataGridMappings.LoadCacheForImport(mappingReader, indexName, appID);
				}

				_dgImportHelper.WriteToDataGrid(ArtifactTypeID, appID, _tableNames.RunId, loader, Settings.LinkDataGridRecords, Settings.HaveDataGridFields, _dataGridMappings, correlationLogger);
			}
			finally
			{
				ImportMeasurements.StopMeasure();
				ImportMeasurements.DataGridImportTime.Stop();
			}
		}

		public virtual void CleanupDataGridInput(string bulkFileShareFolderPath, ILog correlationLogger)
		{
			if (!string.IsNullOrEmpty(Settings.DataGridFileName))
			{
				ImportMeasurements.StartMeasure();
				string dataGridFilePath = System.IO.Path.Combine(bulkFileShareFolderPath, Settings.DataGridFileName);
				correlationLogger.LogDebug("Deleting {dataGridTempFile}", dataGridFilePath);
				File.Instance.Delete(dataGridFilePath);
				if (!string.IsNullOrEmpty(Settings.DataGridOffsetFileName))
				{
					string dataGridOffsetFile = System.IO.Path.Combine(bulkFileShareFolderPath, Settings.DataGridOffsetFileName);
					correlationLogger.LogDebug("Deleting {dataGridOffsetFile}", dataGridOffsetFile);
					File.Instance.Delete(dataGridFilePath);
				}

				ImportMeasurements.StopMeasure();
			}
		}

		public void MapDataGridRecords(ILog correlationLogger)
		{
			if (!Settings.HasDataGridWorkToDo)
			{
				return;
			}

			string tempTableName = GetTempTableName();

			ImportMeasurements.StartMeasure();
			_dgImportHelper.UpdateErrors(tempTableName, _STATUS_COLUMN_NAME, _FILTER_BY_ORDER, correlationLogger);
			ImportMeasurements.StopMeasure();
		}

		public string GetTempTableName()
		{
			return SqlNameHelper.GetSqlFriendlyName(_tableNames.Native);
		}

		private List<FieldInfo> GetMismatchedDataGridFields()
		{
			if (Settings?.MappedFields is null)
			{
				return new List<FieldInfo>();
			}

			var verifier = new Relativity.Data.TextMigrationVerifier(Context);
			var output = Settings.MappedFields.Where(field => field.Type == FieldTypeHelper.FieldType.Text && !field.EnableDataGrid && verifier.IsFieldDataGrid(field.ArtifactID)).ToList();
			foreach (FieldInfo mismatchedDataGridField in output)
			{
				mismatchedDataGridField.EnableDataGrid = true;
			}
			return output;
		}

		public void UpdateDgFieldMappingRecords(IEnumerable<DGImportFileInfo> dgImportFileInfoList, ILog correlationLogger)
		{
			if (dgImportFileInfoList.Any())
			{
				ImportMeasurements.StartMeasure();
				string sqlStatement = DGRelativityRepository.UpdateDgFieldMappingRecordsSql(_tableNames.Native, "kCura_Import_Status");
				var sqlParam = new SqlParameter("@dgImportFileInfo", dgImportFileInfoList.GetDgImportFileInfoAsDataRecord());
				sqlParam.SqlDbType = SqlDbType.Structured;
				sqlParam.TypeName = "EDDSDBO.DgImportFileInfoType";
				var filter = new HashSet<int>();
				using (var reader = Context.ExecuteSQLStatementAsReader(sqlStatement, Enumerable.Repeat(sqlParam, 1), QueryTimeout))
				{
					while (reader.Read())
					{
						filter.Add(Convert.ToInt32(reader[0]));
					}
				}

				var dgFilesToDelete = dgImportFileInfoList.Where(info => info.FileLocation != null && !filter.Contains(info.ImportId));
				foreach (var perIndexName in dgFilesToDelete.GroupBy(g => g.IndexName))
				{
					string indexName = perIndexName.Key;
					foreach (var perFieldName in perIndexName.GroupBy(g => $"{g.FieldNamespace}.{g.FieldName}"))
					{
						string fieldName = perFieldName.Key;
						try
						{
							DGFieldInformationLookupFactory.DeleteList = perFieldName;
							var artifactIDs = perFieldName.Select(x => x.ImportId);
							_dgContext.BaseDataGridContext.TryDeleteBulk(artifactIDs, Enumerable.Repeat(fieldName, 1), indexName);
						}
						catch (Exception)
						{
							correlationLogger.LogWarning("Cleanup of extracted text that failed to import failed for index {indexName} and field {fieldName}", indexName, fieldName);
						}
						finally
						{
							DGFieldInformationLookupFactory.DeleteList = null;
						}
					}
				}

				ImportMeasurements.StopMeasure();
			}
		}
		#endregion

		#region Utility
		#region Retries
		public static T ExecuteWithRetry<T>(Func<T> action)
		{
			Policy policy = Policy.Handle<Exception>().WaitAndRetry(_retryCount, i => TimeSpan.FromMilliseconds(_retrySleepDurationMilliseconds));
			return policy.Execute(action);
		}
		#endregion

		#region Timeout Calls
		protected void ExecuteNonQuerySQLStatement(string statement)
		{
			ExecuteNonQuerySQLStatement(statement, null);
		}

		protected void ExecuteNonQuerySQLStatement(string statement, IEnumerable<SqlParameter> parameters)
		{
			Context.ExecuteNonQuerySQLStatement(statement, parameters, QueryTimeout);
		}

		protected T ExecuteSqlStatementAsScalar<T>(string statement)
		{
			return ExecuteSqlStatementAsScalar<T>(statement, null);
		}

		protected T ExecuteSqlStatementAsScalar<T>(string statement, IEnumerable<SqlParameter> parameters)
		{
			return Context.ExecuteSqlStatementAsScalar<T>(statement, parameters, QueryTimeout);
		}

		protected System.Data.DataTable ExecuteSqlStatementAsDataTable(string statement)
		{
			return ExecuteSqlStatementAsDataTable(statement, null);
		}

		protected System.Data.DataTable ExecuteSqlStatementAsDataTable(string statement, IEnumerable<SqlParameter> parameters)
		{
			return Context.ExecuteSqlStatementAsDataTable(statement, parameters, QueryTimeout);
		}

		protected SqlDataReader ExecuteSQLStatementAsReader(string statement)
		{
			return ExecuteSQLStatementAsReader(statement, null);
		}

		protected SqlDataReader ExecuteSQLStatementAsReader(string statement, IEnumerable<SqlParameter> parameters)
		{
			return Context.ExecuteSQLStatementAsReader(statement, parameters);
		}
		#endregion

		private FieldInfo GetIdentifierField()
		{
			foreach (FieldInfo mappedField in Settings.MappedFields)
			{
				if (mappedField.Category == FieldCategory.Identifier)
					return mappedField;
			}

			var idField = new FieldInfo();
			SqlDataReader reader = null;
			try
			{
				reader = ExecuteSQLStatementAsReader(string.Format("SELECT * FROM [Field] WHERE FieldArtifactTypeID = {0} AND FieldCategoryID = {1}", ArtifactTypeID, (int)FieldCategory.Identifier));
				reader.Read();
				idField.ArtifactID = Convert.ToInt32(reader["ArtifactID"]);
				idField.Category = FieldCategory.Identifier;
				idField.CodeTypeID = default;
				idField.DisplayName = Convert.ToString(reader["DisplayName"]);
				idField.FormatString = string.Empty;
				idField.IsUnicodeEnabled = Convert.ToBoolean(reader["UseUnicodeEncoding"]);
				idField.TextLength = Convert.ToInt32(reader["Maxlength"]);
				idField.Type = FieldTypeHelper.FieldType.Varchar;
			}
			finally
			{
				kCura.Data.RowDataGateway.Helper.CloseDataReader(reader);
				Context.ReleaseConnection();
			}

			return idField;
		}

		protected FieldInfo GetKeyField() => Settings.GetKeyField();

		protected bool FieldIsOnObjectTable(FieldInfo field)
		{
			if (field is null)
				return false;
			if (field.EnableDataGrid)
				return false;
			bool retval = true;
			switch (field.Type)
			{
				case FieldTypeHelper.FieldType.Code:
				case FieldTypeHelper.FieldType.MultiCode:
				case FieldTypeHelper.FieldType.Objects:
				case FieldTypeHelper.FieldType.OffTableText:
					{
						retval = false;
						break;
					}
			}

			return retval;
		}
		#endregion
	}
}