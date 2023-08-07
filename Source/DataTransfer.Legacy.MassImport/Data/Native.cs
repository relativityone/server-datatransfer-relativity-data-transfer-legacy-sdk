using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Relativity.Core.Service;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.SqlFramework;
using MassImportManagerLockKey = Relativity.MassImport.Core.MassImportManagerLockKey;
using DataTransfer.Legacy.MassImport.Toggles;
using Relativity.Toggles;

namespace Relativity.MassImport.Data
{
	internal class Native : ObjectBase
	{
		#region Members
		public const string exrtactedTextCodePageColumnName = "ExtractedTextEncodingPageCode";
		private FieldInfo[] _copyIdrelationalColumns;
		private FieldInfo[] _allRelationalFields;
		private readonly Relativity.Core.BaseContext _context;
		private readonly ILockHelper _lockHelper;

		public FieldInfo[] AllRelationalColumns
		{
			get
			{
				if (_allRelationalFields is null)
				{
					_allRelationalFields = Relativity.MassImport.Data.Helper.GetFieldsForArtifactTypeByCategory(this.Context, this.ArtifactTypeID, FieldCategory.Relational).ToArray();
				}

				return _allRelationalFields;
			}

			set => _allRelationalFields = value;
		}

		private FieldInfo[] CopyIdRelationalColumns
		{
			get
			{
				if (_copyIdrelationalColumns is null)
				{
					_copyIdrelationalColumns = (from field in AllRelationalColumns
												where field.ImportBehavior.HasValue && (int?)field.ImportBehavior != (int?)FieldInfo.ImportBehaviorChoice.LeaveBlankValuesUnchanged
												select field).ToArray();
				}

				return _copyIdrelationalColumns;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new native file loader.
		/// </summary>
		/// <param name="context">The Row Data Gateway context that should be used when saving
		/// or loading data</param>
		/// <param name="settings">A collection of settings for the native loader</param>
		public Native(
			Relativity.Core.BaseContext context, 
			IQueryExecutor queryExecutor,
			Relativity.MassImport.DTO.NativeLoadInfo settings,
			int importUpdateAuditAction,
			ImportMeasurements importMeasurements,
			ColumnDefinitionCache columnDefinitionCache,
			int caseSystemArtifactId,
			ILockHelper lockHelper) : base(
				context.DBContext, 
				queryExecutor,
				settings,
				new NativeImportSql(),
				(int) Relativity.ArtifactType.Document,
				importUpdateAuditAction,
				importMeasurements,
				columnDefinitionCache,
				caseSystemArtifactId)
		{
			_context = context;
			_lockHelper = lockHelper;
		}
		#endregion

		protected override string GetArtifactTypeTableNameFromArtifactTypeId()
		{
			return "Document";
		}

		public ISqlQueryPart GetCreateDocumentsSqlStatement(string requestOrigination, string recordOrigination, bool performAudit, bool includeExtractedTextEncoding, string codeArtifactTableName)
		{
			var sql = new SerialSqlQuery();
			sql.Add(new InlineSqlQuery($"DECLARE @now DATETIME = GETUTCDATE()"));
			sql.Add(new InlineSqlQuery($"DECLARE @hasImagesCodeTypeID INT = (SELECT TOP 1 CodeTypeID FROM CodeType WHERE [Name] = 'HasImages')"));
			sql.Add(new InlineSqlQuery($"DECLARE @hasImagesCodeArtifactID INT = (SELECT TOP 1 [ArtifactID] FROM [Code] WHERE [Name]='No' AND CodeTypeID = @hasImagesCodeTypeID )"));
			
			sql.Add(ArtifactTableInsertSql.WithDocument(
				this._tableNames, 
				this.IdentifierField.GetColumnName(), 
				ObjectBase.TopFieldArtifactID));

			sql.Add(GetInsertDocumentsSqlStatement());

			if (ToggleProvider.Current.IsEnabled<UseLegacyInsertAncestorsQueryToggle>())
			{
				sql.Add(this.ImportSql.InsertAncestorsOfTopLevelObjectsLegacy(this._tableNames));
			}
			else
			{
				sql.Add(this.ImportSql.InsertAncestorsOfTopLevelObjects(this._tableNames));
			}

			if (performAudit && this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				if (includeExtractedTextEncoding)
				{
					string fullTextOverlayDetail = this.Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit ? this.GetExtractedTextDetail(exrtactedTextCodePageColumnName) : "''";
					sql.Add(this.ImportSql.CreateAuditClauseWithEnabledExtractedText(this._tableNames, ObjectBase.TopFieldArtifactID, fullTextOverlayDetail, recordOrigination, recordOrigination));
				}
				else
				{
					sql.Add(this.ImportSql.CreateAuditClause(this._tableNames, ObjectBase.TopFieldArtifactID, requestOrigination, recordOrigination));
				}
			}

			sql.Add(this.ImportSql.InsertDataGridRecordMapping(this._tableNames, ObjectBase.TopFieldArtifactID));
			sql.Add(this.ImportSql.InsertIntoCodeArtifactTableForDocImages(this._tableNames, codeArtifactTableName, ObjectBase.TopFieldArtifactID));

			return sql;
		}

		private ISqlQueryPart GetInsertDocumentsSqlStatement()
		{
			var unmappedRelationalFields = this.GetUnmappedRelationalFields(this.Settings.MappedFields, CopyIdRelationalColumns);
			var selectClause = new StringBuilder();
			foreach (FieldInfo field in unmappedRelationalFields)
			{
				selectClause.Append($",{Environment.NewLine}\t[{field.GetColumnName()}]");
			}
			foreach (FieldInfo field in this.Settings.MappedFields){
			{
				if (this.FieldIsOnObjectTable(field))
				{
					selectClause.Append($",{Environment.NewLine}\t[{field.GetColumnName()}]");
				}
			}}

			if (this.Settings.UploadFiles)
			{
				selectClause.Append($",{Environment.NewLine}\t[FileIcon]");
			}

			var setClause = new StringBuilder();
			foreach (FieldInfo field in unmappedRelationalFields)
			{
				setClause.Append($",{Environment.NewLine}\tN.[{this.IdentifierField.GetColumnName()}]");
			}
			foreach (FieldInfo field in this.Settings.MappedFields)
			{
				if (field.Category == FieldCategory.Relational && (int?)field.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ReplaceBlankValuesWithIdentifier)
				{
					setClause.Append($",{Environment.NewLine}\tCASE WHEN N.[{field.GetColumnName()}] IS NULL THEN N.[{this.IdentifierField.GetColumnName()}] COLLATE {this.ColumnDefinitionCache[field.ArtifactID].CollationName} WHEN N.[{field.GetColumnName()}] = '' THEN N.[{this.IdentifierField.GetColumnName()}] COLLATE {this.ColumnDefinitionCache[field.ArtifactID].CollationName} ELSE N.[{field.GetColumnName()}] END");
				}
				else if (this.FieldIsOnObjectTable(field))
				{
					setClause.Append($",{Environment.NewLine}\tN.[{field.GetColumnName()}]");
				}
			}

			if (this.Settings.UploadFiles)
			{
				setClause.Append($",{Environment.NewLine}\tN.[kCura_Import_Filename]");
			}

			return this.ImportSql.InsertDocuments(this.ArtifactTypeTableName, selectClause.ToString(), setClause.ToString(), this._tableNames, ObjectBase.TopFieldArtifactID.ToString());
		}

		public int CreateDocuments(int userID, int? auditUserID, string requestOrigination, string recordOrigination, bool performAudit, bool includeExtractedTextEncoding)
		{
			this.ImportMeasurements.StartMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			kCura.Utility.InjectionManager.Instance.Evaluate("b52ff194-2a14-4c62-bf3a-6a100a376cb3");
			if (!auditUserID.HasValue)
			{
				auditUserID = userID;
			}
			string codeArtifactTableName = Relativity.Data.CodeHelper.GetCodeArtifactTableNameByCodeTypeName(this.Context, "HasImages");

			var sql = GetCreateDocumentsSqlStatement(requestOrigination, recordOrigination, performAudit, includeExtractedTextEncoding, codeArtifactTableName);
			var parameters = new[]
			{
				new SqlParameter("@userID", userID), 
				new SqlParameter("@auditUserID", auditUserID.Value), 
				new SqlParameter("@containerArtifactId", (object)this.ColumnDefinitionCache.TopLevelParentArtifactId)
			};
			int createdDocumentsCount = this.QueryExecutor.ExecuteBatchOfSqlStatementsAsScalar<int>(sql, parameters, this.QueryTimeout);

			kCura.Utility.InjectionManager.Instance.Evaluate("5ece280c-db9c-4d59-ab19-03ce3178b4bd");
			this.ImportMeasurements.StopMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();

			return createdDocumentsCount;
		}

		private string GetExtractedTextDetail(string detailsValueColumnName)
		{
			return $@"
[{detailsValueColumnName}] =
	 CASE
			WHEN [{detailsValueColumnName}] IS NULL THEN '<auditElement><extractedTextEncodingPageCode>' + '-1' + '</extractedTextEncodingPageCode></auditElement>'
			else '<auditElement><extractedTextEncodingPageCode>' +  CAST([{detailsValueColumnName}] as varchar(200)) + '</extractedTextEncodingPageCode></auditElement>'
	 END";
		}

		public override void CreateAssociatedObjectsForSingleObjectFieldByName(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit)
		{
			this.ImportMeasurements.StartMeasure();

			string objectTypeName = this.ColumnDefinitionCache[field.ArtifactID].ObjectTypeName;
			string associatedObjectTable = Relativity.Data.FieldHelper.GetColumnName(objectTypeName);
			string idFieldColumnName = this.ColumnDefinitionCache[field.ArtifactID].ColumnName;
			int associatedArtifactTypeId = this.ColumnDefinitionCache[field.ArtifactID].ObjectTypeDescriptorArtifactTypeID;

			var sql = new SerialSqlQuery();
			sql.Add(new InlineSqlQuery($"DECLARE @now DATETIME = GETUTCDATE()"));

			sql.Add(this.ImportSql.PopulateAssociatedPartTable(this._tableNames, field.ArtifactID, associatedObjectTable, field.GetColumnName(), idFieldColumnName));
			sql.Add(AssociatedObjectsValidationSql.ValidateAssociatedObjectsForSingleObjectField(this._tableNames, field, associatedArtifactTypeId));

			sql.Add(ArtifactTableInsertSql.WithAssociatedObjects(
				this._tableNames, 
				field.GetColumnName(), 
				field.ArtifactID, 
				associatedArtifactTypeId, 
				this.ColumnDefinitionCache.TopLevelParentArtifactId, 
				this.ColumnDefinitionCache.TopLevelParentAccessControlListId));
			
			sql.Add(this.ImportSql.InsertAssociatedObjects(this._tableNames, associatedObjectTable, idFieldColumnName, field));
			sql.Add(this.ImportSql.InsertAncestorsOfAssociateObjects(this._tableNames, field.ArtifactID.ToString(), this.ColumnDefinitionCache.TopLevelParentArtifactId.ToString()));
			
			if (performAudit && this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				sql.Add(this.ImportSql.CreateAuditClause(this._tableNames, field.ArtifactID, requestOrigination, recordOrigination));
			}

			var sqlParameters = new[]
			{
				new SqlParameter("@auditUserID", auditUserId.Value), 
				new SqlParameter("@containerArtifactId", ColumnDefinitionCache.TopLevelParentArtifactId), 
				new SqlParameter("@fieldDisplayName", field.DisplayName)
			};

			_lockHelper.Lock(this._context, MassImportManagerLockKey.LockType.SingleObjectField, () =>
			{
				this.Context.ExecuteNonQuerySQLStatement(sql.ToString(), sqlParameters, this.QueryTimeout);
			});

			this.Context.ExecuteNonQuerySQLStatement(this.ImportSql.ConvertObjectFieldsToArtifactIDs(this._tableNames, field).ToString(), this.QueryTimeout);
			this.ImportMeasurements.StopMeasure();
		}

		public int UpdateDocumentMetadata(int userID, int? auditUserID, string reqOrig, string recOrig, bool performAudit, bool includeExtractedTextEncoding)
		{
			this.ImportMeasurements.StartMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			kCura.Utility.InjectionManager.Instance.Evaluate("6d06740d-c141-4194-b16c-07255de7ce8c");
			if (!auditUserID.HasValue)
			{
				auditUserID = userID;
			}
			string sqlFormat = this.ImportSql.UpdateMetadata();
			var keyField = this.GetKeyField();
			
			var setClause = new StringBuilder();
			foreach (FieldInfo mappedField in this.Settings.MappedFields)
			{
				if (this.FieldIsOnObjectTable(mappedField))
				{
					if (mappedField.Category != FieldCategory.Identifier)
					{
						switch (mappedField.Category)
						{
							case FieldCategory.Relational:
								{
									if ((int?)mappedField.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ReplaceBlankValuesWithIdentifier == true)
									{
										setClause.AppendFormat("	D.[{0}] = CASE WHEN N.[{0}] IS NULL THEN N.[{1}] COLLATE {2} WHEN N.[{0}] = '' THEN N.[{1}] COLLATE {2} ELSE N.[{0}] END,", mappedField.GetColumnName(), keyField.GetColumnName(), this.ColumnDefinitionCache[mappedField.ArtifactID].CollationName);
										setClause.AppendLine();
									}
									else
									{
										setClause.AppendFormat("	D.[{0}] = N.[{0}],", mappedField.GetColumnName());
										setClause.AppendLine();
									}

									break;
								}

							case FieldCategory.AutoCreate:
								{
									if (this.Settings.UploadFiles)
									{
										setClause.AppendFormat("	D.[{0}] = N.[{0}],", mappedField.GetColumnName());
										setClause.AppendLine();
									}

									break;
								}

							default:
								{
									if (this.Settings.LoadImportedFullTextFromServer && mappedField.Category == FieldCategory.FullText)
									{
									}
									// if we are reading the file paths directly from their share location, skip the update clause for the text field and insead update the text afterward
									else
									{
										setClause.AppendFormat("	D.[{0}] = N.[{0}],", mappedField.GetColumnName());
										setClause.AppendLine();
									}

									break;
								}
						}
					}
				}
			}

			if (this.Settings.UploadFiles)
			{
				setClause.AppendLine("	D.[FileIcon] = N.[kCura_Import_Filename],");
			}

			var auditBuilder = new AuditDetailsBuilder(this.Context, this.Settings, this.ColumnDefinitionCache, _tableNames, base.ArtifactTypeID);
			var auditClauses = auditBuilder.GenerateAuditDetails(performAudit, includeExtractedTextEncoding);
			string auditDetailsClause = auditClauses.Item1;
			string auditMapClause = auditClauses.Item2;

			// Check if the object table has to change
			if (setClause.Length > 0)
			{
				// Add UPDATE statement
				sqlFormat = sqlFormat.Replace("/* UpdateObjectOrDocTable */", this.ImportSql.UpdateObjectOrDocTable());
				// Insert to AuditRecord using OUTPUT INTO
				if (performAudit)
				{
					if (this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
					{
						sqlFormat = sqlFormat.Replace("/* UpdateAuditRecordsMerge */", this.ImportSql.UpdateAuditClauseMerge(this.ImportUpdateAuditAction, auditDetailsClause));
					}

					if (this.Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit)
					{
						sqlFormat = sqlFormat.Replace("/* MapFieldsAuditJoin */", this.ImportSql.MapFieldsAuditJoin(auditMapClause, this._tableNames.Map));
					}
				}
			}
			// Insert to AuditRecord using regular INSERT
			else if (performAudit)
			{
				if (this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
				{
					sqlFormat = sqlFormat.Replace("/* UpdateAuditRecordsInsert */", this.ImportSql.UpdateAuditClauseInsert(this._tableNames.Native, this.ImportUpdateAuditAction, auditDetailsClause));
				}

				if (this.Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit)
				{
					sqlFormat = sqlFormat.Replace("/* MapFieldsAuditJoin */", this.ImportSql.MapFieldsAuditJoin(auditMapClause, this._tableNames.Map));
				}
			}

			if (MismatchedDataGridFields.Any())
			{
				// If there is a mismatched field, then the field was switch to DG in the middle of an import by Text Migration
				// If the column hasn't been dropped yet, then we need to update the field in SQL to prevent verification errors in Text Migration
				var mismatchedStringUpdate = new StringBuilder();
				bool prependComma = false;
				string columnNames = string.Join(",", MismatchedDataGridFields.Select(field => $"'{field.GetColumnName()}'"));
				var columnExistParameters = new[] { new SqlParameter("@columnNames", SqlDbType.VarChar) { Value = columnNames } };
				var columnsThatExist = this.Context.ExecuteSqlStatementAsList<string>(this.ImportSql.DoesColumnExistOnDocumentTable(), reader => reader.GetString(0), columnExistParameters);
				foreach (FieldInfo mismatchedField in MismatchedDataGridFields.Where(field => columnsThatExist.Contains(field.GetColumnName())))
				{
					if (prependComma)
					{
						mismatchedStringUpdate.Append(", ");
					}

					mismatchedStringUpdate.AppendFormat("[Document].[{0}] = tmp.[{0}]", mismatchedField.GetColumnName());
					prependComma = true;
				}

				if (mismatchedStringUpdate.Length > 0)
				{
					string updateSql = string.Format(this.ImportSql.UpdateMismatchedDataGridFields(this._tableNames.Native, mismatchedStringUpdate.ToString()));
					sqlFormat = sqlFormat.Replace("/* UpdateMismatchedFields */", updateSql);
				}
			}

			sqlFormat = string.Format(
				sqlFormat, 
				_tableNames.Native, 
				setClause.Length == 0 ? string.Empty : setClause.ToString(0, setClause.Length - (",".Length + Environment.NewLine.Length)), 
				auditDetailsClause, 
				ArtifactTypeTableName, 
				_tableNames.Part, 
				TopFieldArtifactID, 
				ImportUpdateAuditAction, 
				_tableNames.Map, 
				auditMapClause);

			var sqlParameters = new[]
			{
				new SqlParameter("@userID", userID), 
				new SqlParameter("@auditUserID", auditUserID), 
				new SqlParameter("@requestOrig", reqOrig), 
				new SqlParameter("@recordOrig", recOrig)
			};

			int updateDocumentsCount = this.QueryExecutor.ExecuteBatchOfSqlStatementsAsScalar<int>(sqlFormat, sqlParameters, this.QueryTimeout);

			kCura.Utility.InjectionManager.Instance.Evaluate("16183222-ff77-4f6f-a311-6bb0770ae71d");
			this.ImportMeasurements.StopMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();

			return updateDocumentsCount;
		}

		protected override void UpdateSynclockSensitiveMultiObjectArtifacts(FieldInfo field, int userID, string associatedObjectTable, string idFieldColumnName, int artifactTypeID, string requestOrigination, string recordOrigination, bool performAudit)
		{
			this.ImportMeasurements.StartMeasure();
			_lockHelper.Lock(this._context, MassImportManagerLockKey.LockType.MultiObjectField, () =>
			{
				base.UpdateSynclockSensitiveMultiObjectArtifacts(field, userID, associatedObjectTable, idFieldColumnName, artifactTypeID, requestOrigination, recordOrigination, performAudit);
			});
			this.ImportMeasurements.StopMeasure();
		}

		private string CreateOffTableExtractedTextSql(FieldInfo mappedField)
		{
			string textTable = new Relativity.Data.SqlGeneration.TableNameGenerator().GetHangingTableName(mappedField);
			string singleFieldSql = $@"
DELETE FROM
	[{ textTable }]
FROM
	[{ textTable }] TxtTable
INNER JOIN [Resource].[{ _tableNames.Native }] Tmp ON
	Tmp.[ArtifactID] = TxtTable.[ArtifactID]
WHERE
	Tmp.[kCura_Import_IsNew] = 0
	AND
	Tmp.[kCura_Import_Status] = { (long)Relativity.MassImport.DTO.ImportStatus.Pending }

INSERT [{ textTable }] (
	[ArtifactID],
	[TextData]
) SELECT
	[ArtifactID],
	[{ mappedField.GetColumnName() }] /* do collate clause here */
FROM
	[Resource].[{ _tableNames.Native }] Tmp
WHERE
	NOT Tmp.[{ mappedField.GetColumnName() }] IS NULL
	AND
	Tmp.[kCura_Import_Status] = { (long)Relativity.MassImport.DTO.ImportStatus.Pending }
";
			return singleFieldSql;
		}

		public void ManageOffTableExtractedTextFields()
		{
			this.ImportMeasurements.StartMeasure();
			foreach (string offTableSqlStatement in this.Settings.MappedFields
				.Where(f => f.Type == FieldTypeHelper.FieldType.OffTableText)
				.Select(f => this.CreateOffTableExtractedTextSql(f)))
			{
				this.Context.ExecuteNonQuerySQLStatement(offTableSqlStatement, Relativity.Data.Config.MassImportSqlTimeout);
			}
			this.ImportMeasurements.StopMeasure();
		}

		public void DeleteFilesNotImported()
		{

			// retrieve kcura_import_location field from records from relnattmp table with an import status of 4
			string sql = $"SELECT [kCura_Import_Location] FROM [Resource].[{ _tableNames.Native }] WHERE [kCura_Import_Status] > 1";

			var dt = this.Context.ExecuteSqlStatementAsDataTable(sql);

			var fileList = new List<string>();

			if (dt != null)
			{
				foreach (DataRow row in dt.Rows)
				{
					if (!Convert.IsDBNull(row[0]))
					{
						string path = ((string)row[0]).Trim();
						if (path.Length > 0)
						{
							fileList.Add(path);
						}
					}
				}

				if (fileList.Count > 0)
				{
					kCura.Utility.File.Instance.DeleteFilesTask(fileList);
				}
			}
		}

		public void DeleteExistingNativeFiles(int userID, bool auditEnabled, string requestOrig, string recordOrig)
		{
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			this.ImportMeasurements.StartMeasure();
			string sqlFormat = this.ImportSql.DeleteExistingNativeFiles();
			string auditInnerString = string.Empty;

			if (auditEnabled && this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				sqlFormat = this.ImportSql.AuditWrapper(sqlFormat);
				auditInnerString = Relativity.MassImport.Data.Helper.GenerateOutputDeletedIntoClause(17, userID, requestOrig, recordOrig);
			}

			sqlFormat = sqlFormat.Replace("/*NativeImportAuditIntoClause*/", auditInnerString);
			this.Context.ExecuteNonQuerySQLStatement(string.Format(sqlFormat, this._tableNames.Native), this.QueryTimeout);
			this.ImportMeasurements.StopMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}

		public int CreateNativeFiles(int userID, bool auditEnabled, string requestOrig, string recordOrig, bool inRepository)
		{
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string sqlFormat = this.ImportSql.CreateNativeFileRows();
			string auditString = "";
			if (auditEnabled && this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				var sb = new StringBuilder(Relativity.MassImport.Data.Helper.GenerateAuditInsertClause(16, userID, requestOrig, recordOrig, this._tableNames.Native));
				sb.AppendFormat("WHERE{0}", Environment.NewLine);
				sb.AppendFormat("{0}[{1}].[kCura_Import_Status] = {3}{2}", "\t", this._tableNames.Native, Environment.NewLine, (object)(long)Relativity.MassImport.DTO.ImportStatus.Pending);
				sb.AppendFormat("{0}AND{1}", "\t", Environment.NewLine);
				sb.AppendFormat("{0}NOT ISNULL([kCura_Import_FileGuid], '') = ''{1}", "\t", Environment.NewLine);
				auditString = sb.ToString();
			}

			sqlFormat = sqlFormat.Replace("/*NativeImportAuditClause*/", auditString);

			sqlFormat = string.Format(sqlFormat, this._tableNames.Native, (object)(inRepository ? 1 : 0), (object)(this.Settings.Billable ? 1 : 0));

			int createdNativeFilesCount = this.QueryExecutor.ExecuteBatchOfSqlStatementsAsScalar<int>(sqlFormat, null, this.QueryTimeout);

			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();
			return createdNativeFilesCount;
		}

		public int[] GetReturnReport()
		{
			var retval = new int[3];
			var dt = this.Context.ExecuteSqlStatementAsDataTable(string.Format(this.ImportSql.GetReturnReport(), this._tableNames.Native, this._tableNames.Part, (object)ObjectBase.TopFieldArtifactID), this.QueryTimeout);
			if (dt.Rows.Count > 0)
			{
				var returnResultRow = dt.Rows[0]; // May contain DBNull values when table was empty
				retval[0] = this.ConvertToInt32(returnResultRow["NewDocument"]);
				retval[1] = this.ConvertToInt32(returnResultRow["UpdatedDocument"]);
				retval[2] = this.ConvertToInt32(returnResultRow["FileCount"]);
			}

			return retval;
		}

		private int ConvertToInt32(object value)
		{
			return value is null || Convert.IsDBNull(value) ? 0 : Convert.ToInt32(value);
		}

		private FieldInfo[] GetUnmappedRelationalFields(FieldInfo[] mappedFields, FieldInfo[] relationalFields)
		{
			var mappedRelationalFieldIds = from field in mappedFields
										   where field.Category == FieldCategory.Relational 
										         && (int?)field.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ReplaceBlankValuesWithIdentifier
										   select field.ArtifactID;
			var unmappedRelationalFields = from field in relationalFields
										   where field.Category == FieldCategory.Relational 
										         && (int?)field.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ReplaceBlankValuesWithIdentifier 
										         && !mappedRelationalFieldIds.Contains(field.ArtifactID)
										   select field;
			return unmappedRelationalFields.ToArray();
		}

		public static SqlDataReader GenerateErrorReader(kCura.Data.RowDataGateway.BaseContext context, string runID, int keyFieldID)
		{
			string sql = Relativity.MassImport.Data.Helper.ErrorSql(context, runID, keyFieldID);
			return context.ExecuteSQLStatementAsReader(sql);
		}
	}
}