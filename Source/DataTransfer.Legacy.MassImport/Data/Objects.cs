using System.Data;
using System.Data.SqlClient;
using System.Text;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class Objects : ObjectBase
	{
		#region Constructors
		/// <summary>
		/// Constructs a new object loader
		/// </summary>
		/// <param name="context">The Row Data Gateway context that should be used when saving
		/// or loading data</param>
		/// <param name="settings">A collection of settings for the loader</param>
		public Objects(
			kCura.Data.RowDataGateway.BaseContext context,
			IQueryExecutor queryExecutor,
			Relativity.MassImport.DTO.ObjectLoadInfo settings,
			int importUpdateAuditAction,
			ImportMeasurements importMeasurements,
			ColumnDefinitionCache columnDefinitionCache,
			int caseSystemArtifactId) : base(
				context,
				queryExecutor,
				settings,
				new ObjectImportSql(),
				settings.ArtifactTypeID,
				importUpdateAuditAction,
				importMeasurements,
				columnDefinitionCache,
				caseSystemArtifactId)
		{
		}

		protected override string GetArtifactTypeTableNameFromArtifactTypeId()
		{
			return new Relativity.Data.ArtifactType(this.Context, this._artifactTypeID).ArtifactType;
		}
		#endregion

		public ISqlQueryPart GetCreateObjectsSqlStatement(string requestOrigination, string recordOrigination, bool performAudit)
		{
			var keyField = this.GetKeyField();
			if (keyField is null)
			{
				keyField = this.IdentifierField;
			}
			string keyFieldCheck = keyField.ArtifactID != this.IdentifierField.ArtifactID ? $" AND N.[{keyField.GetColumnName()}] IS NOT NULL" : string.Empty;
			
			var sql = new SerialSqlQuery();
			sql.Add(new InlineSqlQuery($"DECLARE @now DATETIME = GETUTCDATE()"));
			
			sql.Add(ArtifactTableInsertSql.WithObject(
				this._tableNames, 
				this.IdentifierField.GetColumnName(), 
				keyField.GetColumnName(),
				ObjectBase.TopFieldArtifactID, 
				this.ArtifactTypeID, 
				keyFieldCheck));

			sql.Add(GetInsertObjectsSqlStatement());
			sql.Add(this.ImportSql.InsertAncestorsOfTopLevelObjects(this._tableNames));

			if (performAudit && this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				sql.Add(this.ImportSql.CreateAuditClause(this._tableNames, ObjectBase.TopFieldArtifactID, requestOrigination, recordOrigination));
			}

			return sql;
		}

		private ISqlQueryPart GetInsertObjectsSqlStatement()
		{
			var selectClause = new StringBuilder();
			foreach (FieldInfo field in this.Settings.MappedFields)
			{
				if (field.Type != FieldTypeHelper.FieldType.File && this.FieldIsOnObjectTable(field))
				{
					selectClause.Append($",{System.Environment.NewLine}\t[{field.GetColumnName()}]");
				}
			}

			var setClause = new StringBuilder();
			foreach (FieldInfo field in this.Settings.MappedFields)
			{
				if (field.Type != FieldTypeHelper.FieldType.File)
				{
					if (field.Category == FieldCategory.Relational)
					{
						setClause.Append($",{System.Environment.NewLine}\tCASE WHEN N.[{field.GetColumnName()}] IS NULL THEN N.[{this.IdentifierField.GetColumnName()}] COLLATE {this.ColumnDefinitionCache[field.ArtifactID].CollationName} WHEN N.[{field.GetColumnName()}] = '' THEN N.[{this.IdentifierField.GetColumnName()}] COLLATE {this.ColumnDefinitionCache[field.ArtifactID].CollationName} ELSE N.[{field.GetColumnName()}] END");
					}
					else if (this.FieldIsOnObjectTable(field))
					{
						setClause.Append($@",{System.Environment.NewLine}{"\t"}N.[{field.GetColumnName()}]");
					}
				}
			}

			return this.ImportSql.InsertGenericObjects(this.ArtifactTypeTableName, this._tableNames, selectClause.ToString(), setClause.ToString(), ObjectBase.TopFieldArtifactID);
		}

		public int CreateObjects(int userID, int? auditUserID, string requestOrigination, string recordOrigination, bool performAudit)
		{
			this.ImportMeasurements.StartMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			kCura.Utility.InjectionManager.Instance.Evaluate("6d9c8b05-753a-4ff8-8377-607b5aaf085a");
			if (!auditUserID.HasValue)
			{
				auditUserID = userID;
			}
			
			var sql = GetCreateObjectsSqlStatement(requestOrigination, recordOrigination, performAudit);
			var parameters = new[]
			{
				new SqlParameter("@userID", userID), 
				new SqlParameter("@auditUserID", auditUserID.Value), 
				new SqlParameter("@containerArtifactId", ColumnDefinitionCache.TopLevelParentArtifactId)
			};
			int createdObjectsCount = this.QueryExecutor.ExecuteBatchOfSqlStatementsAsScalar<int>(sql, parameters, this.QueryTimeout);
			
			kCura.Utility.InjectionManager.Instance.Evaluate("6d6bd9ea-cac6-4108-8412-a6772474208d");
			this.ImportMeasurements.StopMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();
			return createdObjectsCount;
		}
		
		public override void CreateAssociatedObjectsForSingleObjectFieldByName(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit)
		{
			string objectTypeName = this.ColumnDefinitionCache[field.ArtifactID].ObjectTypeName;
			string associatedObjectTable = Relativity.Data.FieldHelper.GetColumnName(objectTypeName);
			int associatedArtifactTypeId = this.ColumnDefinitionCache[field.ArtifactID].ObjectTypeDescriptorArtifactTypeID;

			string idFieldColumnName;
			if (associatedArtifactTypeId == (int) ArtifactType.Document)
			{
				idFieldColumnName = SqlNameHelper.GetSqlFriendlyName(Relativity.Data.DocumentQuery.GetDisplayNameFromFieldCategoryIDFieldArtifactTypeID(this.Context, (int)FieldCategory.Identifier, (int) Relativity.ArtifactType.Document));
			}
			else
			{
				idFieldColumnName = this.Context.ExecuteSqlStatementAsScalar<string>($"SELECT TOP 1 ColumnName FROM [ArtifactViewField] INNER JOIN [Field] ON [Field].[ArtifactViewFieldID] = [ArtifactViewField].[ArtifactViewFieldID] WHERE Field.FieldArtifactTypeID = {associatedArtifactTypeId} AND Field.FieldCategoryID = {(int)FieldCategory.Identifier}", this.QueryTimeout);
			}

			var sql = new SerialSqlQuery();
			sql.Add(new InlineSqlQuery($"DECLARE @now DATETIME = GETUTCDATE()"));

			sql.Add(this.ImportSql.PopulateAssociatedPartTable(this._tableNames, field.ArtifactID, associatedObjectTable, field.GetColumnName(), idFieldColumnName));
			sql.Add(AssociatedObjectsValidationSql.ValidateAssociatedObjectsForSingleObjectField(this._tableNames, field, associatedArtifactTypeId));

			if (associatedArtifactTypeId == (int) ArtifactType.Document)
			{
				sql.Add(AssociatedObjectsValidationSql.ValidateAssociatedDocumentForSingleObjectFieldExists(this._tableNames, field));
			}

			sql.Add(ArtifactTableInsertSql.WithAssociatedObjects(
				this._tableNames, 
				field.GetColumnName(), 
				field.ArtifactID, 
				associatedArtifactTypeId, 
				this.ColumnDefinitionCache.TopLevelParentArtifactId, 
				this.ColumnDefinitionCache.TopLevelParentAccessControlListId));

			// This query fixes the problem with the duplicated records REL-438573 which does not occur in Overlay mode, however in Overlay mode it causes an SQL exception REL-558408
			if (associatedArtifactTypeId == this.ArtifactTypeID && Settings.Overlay != Relativity.MassImport.DTO.OverwriteType.Overlay)
			{
				sql.Add(this.ImportSql.InsertSelfReferencedObjects(this._tableNames, idFieldColumnName, field, this.ColumnDefinitionCache.TopLevelParentAccessControlListId));
			}

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

			this.Context.ExecuteNonQuerySQLStatement(sql.ToString(), sqlParameters, this.QueryTimeout);

			this.Context.ExecuteNonQuerySQLStatement(this.ImportSql.ConvertObjectFieldsToArtifactIDs(this._tableNames, field).ToString(), this.QueryTimeout);
		}

		public int UpdateObjectMetadata(int userID, int? auditUserID, string reqOrig, string recOrig, bool performAudit)
		{
			this.ImportMeasurements.StartMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			kCura.Utility.InjectionManager.Instance.Evaluate("21c880cd-5404-420d-92c5-d2bc08bd5058");
			if (!auditUserID.HasValue)
			{
				auditUserID = userID;
			}
			string sqlFormat = this.ImportSql.UpdateMetadata();
			var keyField = this.GetKeyField();
			if (keyField is null)
			{
				keyField = this.IdentifierField;
			}
			bool keyFieldIsIdentifierField = keyField.ArtifactID == this.IdentifierField.ArtifactID;
			string fileFieldAuditSql = null;
			var setClause = new StringBuilder();
			foreach (FieldInfo mappedField in this.Settings.MappedFields)
			{
				if (mappedField.Type == FieldTypeHelper.FieldType.File)
				{
					fileFieldAuditSql = this.ImportSql.FileFieldAuditJoin("File" + mappedField.ArtifactID);
				}
				else if (this.FieldIsOnObjectTable(mappedField))
				{
					if (mappedField.Category != FieldCategory.Identifier)
					{
						if (this.Settings.LoadImportedFullTextFromServer && mappedField.Category == FieldCategory.FullText)
						{
							// if we are reading the file paths directly from their share location, skip the update clause for the text field and instead update the text afterward
						}
						else
						{
							setClause.AppendFormat("	D.[{0}] = N.[{0}],", mappedField.GetColumnName());
							setClause.AppendLine();
						}
					}
					else if (!keyFieldIsIdentifierField)
					{
						setClause.AppendFormat("	D.[{0}] = N.[{0}],", mappedField.GetColumnName());
						setClause.AppendLine();
						sqlFormat = sqlFormat.Replace("/* UpdateTextIdentifier */", $", A.[TextIdentifier] = N.[{mappedField.GetColumnName()}]");
					}
				}
			}

			var auditBuilder = new AuditDetailsBuilder(this.Context, this.Settings, ColumnDefinitionCache, _tableNames, base.ArtifactTypeID);
			var auditClauses = auditBuilder.GenerateAuditDetails(performAudit, false);
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
					if (this.Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit)
					{
						if (fileFieldAuditSql != null)
						{
							sqlFormat = sqlFormat.Replace("/* FileFieldAuditJoin */", fileFieldAuditSql);
						}

						sqlFormat = sqlFormat.Replace("/* MapFieldsAuditJoin */", this.ImportSql.MapFieldsAuditJoin(auditMapClause, this._tableNames.Map));
					}

					if (this.Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
					{
						sqlFormat = sqlFormat.Replace("/* UpdateAuditRecordsMerge */", this.ImportSql.UpdateAuditClauseMerge(this.ImportUpdateAuditAction, auditDetailsClause));
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
					if (fileFieldAuditSql != null)
					{
						sqlFormat = sqlFormat.Replace("/* FileFieldAuditJoin */", fileFieldAuditSql);
					}

					sqlFormat = sqlFormat.Replace("/* MapFieldsAuditJoin */", this.ImportSql.MapFieldsAuditJoin(auditMapClause, this._tableNames.Map));
				}
			}

			sqlFormat = string.Format(
				sqlFormat, 
				this._tableNames.Native, 
				setClause.Length == 0 ? string.Empty : setClause.ToString(0, setClause.Length - (",".Length + System.Environment.NewLine.Length)), 
				auditDetailsClause, 
				this.ArtifactTypeTableName, 
				this._tableNames.Part, 
				TopFieldArtifactID, 
				ImportUpdateAuditAction, 
				this._tableNames.Map, 
				auditMapClause);

			var sqlParameters = new[]
			{
				new SqlParameter("@userID", userID), 
				new SqlParameter("@auditUserID", auditUserID), 
				new SqlParameter("@requestOrig", reqOrig), 
				new SqlParameter("@recordOrig", recOrig)
			};
			
			int updatedDocumentsCount = this.QueryExecutor.ExecuteBatchOfSqlStatementsAsScalar<int>(sqlFormat, sqlParameters, this.QueryTimeout);

			kCura.Utility.InjectionManager.Instance.Evaluate("92862d53-d152-4f13-b77d-ed689f5ef432");
			this.ImportMeasurements.StopMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();

			return updatedDocumentsCount;
		}

		public int PopulateFileTables(int userID, bool auditEnabled, string requestOrig, string recordOrig, string masterDbPrepend)
		{
			int filesCount = 0;
			this.ImportMeasurements.StartMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Start();
			foreach (FieldInfo mappedField in this.Settings.MappedFields)
			{
				if (mappedField.Type == FieldTypeHelper.FieldType.File)
				{
					string auditDeleteFilesString = "";
					string auditCreateFilesString = "";
					if (auditEnabled)
					{
						var sb = new StringBuilder();
						if (this.Settings.Overlay != Relativity.MassImport.DTO.OverwriteType.Append)
						{
							sb.Append(Helper.GenerateAuditInsertClause(17, userID, requestOrig, recordOrig, this._tableNames.Native));
							sb.AppendFormat("WHERE{0}", System.Environment.NewLine);
							sb.AppendFormat("{0}[kCura_Import_Status] = 0{1}", "\t", System.Environment.NewLine);
							auditDeleteFilesString = sb.ToString();
						}

						sb = new StringBuilder(Helper.GenerateAuditInsertClause(16, userID, requestOrig, recordOrig, this._tableNames.Native));
						sb.AppendFormat("WHERE{0}", System.Environment.NewLine);
						sb.AppendFormat("{0}[kCura_Import_Status] = 0{1}", "\t", System.Environment.NewLine);
						sb.AppendFormat("{0}AND NOT ISNULL([{1}_ImportObject_FileName], '') = ''{2}", "\t", mappedField.GetColumnName(), System.Environment.NewLine);
						auditCreateFilesString = sb.ToString();
					}

					string sqlFormat = this.ImportSql.PopulateFileTables();
					sqlFormat = sqlFormat.Replace("/*CreateFileAuditClause*/", auditCreateFilesString);
					if (this.Settings.Overlay != Relativity.MassImport.DTO.OverwriteType.Append)
					{
						sqlFormat = sqlFormat.Replace("/*DeleteFileAuditClause*/", auditDeleteFilesString);
					}

					filesCount += this.QueryExecutor.ExecuteBatchOfSqlStatementsAsScalar<int>(string.Format(
						sqlFormat, 
						"File" + mappedField.ArtifactID, 
						this._tableNames.Native, 
						mappedField.GetColumnName() + "_ImportObject_FileName", 
						mappedField.GetColumnName() + "_ImportObject_FileSize", 
						mappedField.GetColumnName() + "_ImportObject_FileLocation", 
						this.ArtifactTypeTableName, mappedField.GetColumnName(), masterDbPrepend), 
						new[] { new SqlParameter("@ObjectArtifactTypeID", (object)this.ArtifactTypeID) }, this.QueryTimeout);
				}
			}

			this.ImportMeasurements.StopMeasure();
			this.ImportMeasurements.PrimaryArtifactCreationTime.Stop();
			return filesCount;
		}

		public InlineSqlQuery ManageAppendParentMissingErrors()
		{
			return this.ImportSql.AppendParentMissingErrors(this._tableNames);
		}

		public InlineSqlQuery ManageAppendOverlayParentMissingErrors()
		{
			return this.ImportSql.AppendOverlayParentMissingErrors(this._tableNames);
		}

		public void ClearTempTableAndSaveErrors()
		{
			// Leave blank for now
		}

		/// <summary>
		/// Exposed so that DetailedObjectImportReportGenerator can actually transform the information, and the existing "GetReturnReport" can continued to be called.
		/// </summary>
		/// <param name="useVerboseReport">Set to "true" to pull back entire list of affected ids, set to "false" for the summary/count version</param>
		/// <returns></returns>
		public DataTable GetReturnReportData(bool useVerboseReport)
		{
			FieldInfo fileField = null;
			foreach (FieldInfo field in this.Settings.MappedFields)
			{
				if (field.Type == FieldTypeHelper.FieldType.File)
				{
					fileField = field;
					break;
				}
			}

			DataTable dt;
			string sqlString = useVerboseReport 
				? string.Format(
					this.ImportSql.GetDetailedReturnReport(), 
					this._tableNames.Native, 
					this._tableNames.Part, 
					this.GetKeyField().GetColumnName(), 
					(object)ObjectBase.TopFieldArtifactID) 
				: string.Format(
					this.ImportSql.GetReturnReport(), 
					this._tableNames.Native, 
					this._tableNames.Part, 
					(object)ObjectBase.TopFieldArtifactID);

			if (fileField != null)
			{
				sqlString = string.Format(sqlString.Replace("kCura_Import_FileGuid", "{0}_ImportObject_FileName"), SqlNameHelper.GetSqlFriendlyName(fileField.DisplayName));
			}

			dt = this.Context.ExecuteSqlStatementAsDataTable(sqlString, this.QueryTimeout);
			return dt;
		}
	}
}