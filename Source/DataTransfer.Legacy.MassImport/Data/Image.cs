using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.Utility;
using Relativity.Data.MassImport;
using Relativity.DataGrid;
using Relativity.DataGrid.Helpers;
using Relativity.DataGrid.Helpers.DGFS;
using Relativity.DataGrid.Implementations.DGFS.ReadBackend;
using Relativity.MassImport.Data.DataGrid;
using Relativity.MassImport.Data.DataGridWriteStrategy;
using DGImportFileInfo = Relativity.MassImport.Data.DataGrid.DGImportFileInfo;
using DGRelativityRepository = Relativity.MassImport.Data.DataGrid.DGRelativityRepository;
using File = kCura.Utility.File;
using ILog = Relativity.Logging.ILog;
using DataTransfer.Legacy.MassImport.Data.Cache;
using Relativity.API;

namespace Relativity.MassImport.Data
{
	internal class Image
	{
		#region Members
		public const string ExtractedTextCodePageColumnName = "ExtractedTextEncodingPageCode";
		public const string FullTextColumnName = "FullText";
		private BaseContext _context;
		private string _documentIdentifierFieldColumnName = "";
		private int _keyFieldID;
		private string _auditRecordCollation = "";
		private int? _queryTimeout;
		private const string _ROWTERMINATOR = "þþKþþ";
		private const string _FIELDTERMINATOR = ",";
		private const string _STATUS_COLUMN_NAME = "Status";
		private const bool _FILTER_BY_ORDER = true;
		private FieldInfo _fullTextField;
		private Relativity.Data.DataGridContext _dgContext;
		private Relativity.MassImport.DTO.ImageLoadInfo _settings;
		private TableNames _tableNames;
		private DataGridImportHelper _dgImportHelper;
		private Relativity.Data.DataGridMappingMultiDictionary _dataGridMappings;
		private static readonly string _PENDING_STATUS = ((long)Relativity.MassImport.DTO.ImportStatus.Pending).ToString();

		#endregion

		#region Constructors
		public Image(BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, IHelper helper)
		{
			_context = context;
			_keyFieldID = settings.KeyFieldArtifactID;
			_settings = settings;
			_tableNames = new TableNames(settings.RunID);
			ImportSql = new ImageImportSql();
			ImportMeasurements = new ImportMeasurements();
			if (FullTextField.EnableDataGrid)
			{
				var dgSqlFactory = new DataGridSqlContextFactory((i) => context.Clone());
				DGFieldInformationLookupFactory = new DataGrid.DGFieldInformationLookupFactory(new FieldInformationLookupFactory(dgSqlFactory));
				_dataGridMappings = new Relativity.Data.DataGridMappingMultiDictionary();
				DGRelativityRepository = new DGRelativityRepository();
				var fml = new FieldMappingLookup(dgSqlFactory);
				var dgfsSqlReader = new SqlBackend(Relativity.Data.Config.DataGridConfiguration, dgSqlFactory);
				DataGridBufferPool argbufferPool = null;
				
				DataGridContextBase @base;
				var fileHelper = new Relativity.DataGrid.Helpers.DGFS.ADLS.DataGridFileHelper(Relativity.Data.Config.DataGridConfiguration, helper);
				@base = new FileSystemContext("document", ref argbufferPool, Relativity.Data.Config.DataGridConfiguration, DGRelativityRepository, _dataGridMappings, DGFieldInformationLookupFactory, fml, dgfsSqlReader, fileHelper);

				_dgContext = new Relativity.Data.DataGridContext(@base);
				_dgImportHelper = new DataGridImportHelper(_dgContext, _context, ImportMeasurements, new Relativity.Data.TextMigrationVerifier(context));
			}
		}
		#endregion

		#region Private Accessors
		private string TableNameArtifactTemp => _tableNames.ImagePart;

		private Relativity.MassImport.DTO.ImageLoadInfo Settings => _settings;

		public DGRelativityRepository DGRelativityRepository { get; set; }
		public DataGrid.DGFieldInformationLookupFactory DGFieldInformationLookupFactory { get; set; }

		private string DocumentIdentifierFieldColumnName
		{
			get
			{
				if (string.IsNullOrEmpty(_documentIdentifierFieldColumnName))
				{
					_documentIdentifierFieldColumnName = _context.ExecuteSqlStatementAsScalar<string>(string.Format("SELECT TOP 1 ColumnName FROM ArtifactViewField INNER JOIN Field ON Field.ArtifactViewFieldID = ArtifactViewField.ArtifactViewFieldID AND ArtifactViewField.ArtifactTypeID = {1} AND Field.ArtifactID = {0}", _keyFieldID, (int)Relativity.ArtifactType.Document), QueryTimeout);
				}

				return _documentIdentifierFieldColumnName;
			}
		}

		private FieldInfo FullTextField
		{
			get
			{
				if (_fullTextField is null)
				{
					_fullTextField = Helper.GetFieldsForArtifactTypeByCategory(_context, (int)Relativity.ArtifactType.Document, FieldCategory.FullText)[0];
				}

				return _fullTextField;
			}
		}

		private string AuditRecordDetailsCollation
		{
			get
			{
				if (string.IsNullOrEmpty(_auditRecordCollation))
				{
					_auditRecordCollation = _context.ExecuteSqlStatementAsScalar<string>("SELECT collation_name FROM sys.columns WHERE [name] = 'Details' AND [object_id] = OBJECT_ID('[EDDSDBO].[AuditRecord]')");
				}

				return _auditRecordCollation;
			}
		}

		private ImageImportSql ImportSql { get; set; }

		private bool IsTextMigrationInProgress => !FullTextField.EnableDataGrid && DoesFullTextColumnText();
		#endregion

		#region Public Accessors
		public int QueryTimeout
		{
			get
			{
				if (!_queryTimeout.HasValue)
				{
					_queryTimeout = InstanceSettings.MassImportSqlTimeout;
				}

				return _queryTimeout.Value;
			}

			set => _queryTimeout = value;
		}

		public string SetOutsideFieldName
		{
			set => _documentIdentifierFieldColumnName = value;
		}

		public string TableNameImageTemp => _tableNames.Image;

		public ImportMeasurements ImportMeasurements;

		public bool IsNewJob { get; private set; }
		#endregion

		#region Temp Table Manipulation
		public string InitializeBulkTable(string bulkFileShareFolderPath, ILog logger)
		{
			ImportMeasurements.StartMeasure();
			CreateMassImportImageTempTables(Settings.UploadFullText, Settings.OverlayArtifactID);
			BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(() => BulkImportImageFile(bulkFileShareFolderPath, Settings.BulkFileName), logger, ImportMeasurements);
			ImportMeasurements.StopMeasure();

			return _tableNames.RunId;
		}

		public string InitializeTempTable()
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.SqlBulkImportTime.Start();
			CreateMassImportImageTempTables(Settings.UploadFullText, Settings.OverlayArtifactID);
			ImportImageFile(System.IO.Path.Combine(Settings.Repository, Settings.BulkFileName), Settings.UploadFullText);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.SqlBulkImportTime.Stop();

			return _tableNames.RunId;
		}

		public object BulkImportImageFile(string bulkFileSharePath, string bulkFileName)
		{
			ImportMeasurements.SqlBulkImportTime.Start();
			string fullFile = System.IO.Path.Combine(bulkFileSharePath, bulkFileName);
			var sql = new System.Text.StringBuilder();
			sql.Append("TRUNCATE TABLE [Resource].[");
			sql.Append(SqlNameHelper.GetSqlFriendlyName(TableNameImageTemp));
			sql.Append("]" + Environment.NewLine);
			sql.Append("BULK INSERT [Resource].[");
			sql.Append(SqlNameHelper.GetSqlFriendlyName(TableNameImageTemp));
			sql.Append("] FROM '");
			sql.Append(fullFile);
			sql.AppendFormat(@"' WITH (FIELDTERMINATOR='{0}',ROWTERMINATOR='{1}\n',DATAFILETYPE='widechar');", _FIELDTERMINATOR, _ROWTERMINATOR);
			_context.ExecuteNonQuerySQLStatement(sql.ToString(), QueryTimeout);
			if (System.IO.File.Exists(fullFile))
			{
				try
				{
					System.IO.File.Delete(fullFile);
				}
				catch
				{
					System.IO.File.Delete(fullFile);
				}
			}

			ImportMeasurements.SqlBulkImportTime.Stop();
			return null;
		}

		public object ExistingFilesLookupInitialization()
		{
			ImportMeasurements.StartMeasure();
			int result = _context.ExecuteNonQuerySQLStatement(string.Format(ImportSql.ExistingFilesLookupInitialization(), TableNameImageTemp, DocumentIdentifierFieldColumnName), QueryTimeout);
			ImportMeasurements.StopMeasure();
			return result;
		}

		public int IncomingImageCount()
		{
			string sql = string.Format(ImportSql.IncomingDocumentCount(), TableNameImageTemp, DocumentIdentifierFieldColumnName);
			int count = _context.ExecuteSqlStatementAsScalar<int>(sql, QueryTimeout);
			return count;
		}

		public void ImportImageFile(string tempFilePath, bool updateFullText)
		{
			var sr = new ImageTempFileReader(tempFilePath);
			string sql = string.Format(ImportSql.InsertImageTableFormatString(), TableNameImageTemp);
			while (!sr.Eof)
			{
				var parameters = sr.GetFixedValues();
				_context.ExecuteNonQuerySQLStatement(sql, parameters, QueryTimeout);
				bool isFirstTimeThrough = true;
				if (updateFullText)
				{
					int id = Convert.ToInt32(parameters[3].Value);
					do
					{
						var ctrl = sr.ReadFullTextStringBlock();
						if (ctrl.Item2 is null) break;
						if (string.IsNullOrEmpty(ctrl.Item2.Value.ToString())) break;
						if (isFirstTimeThrough)
						{
							_context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [FullText] = @textBlock WHERE [OriginalLineNumber] = {1}", TableNameImageTemp, id), new SqlParameter[] { ctrl.Item2 }, QueryTimeout);
							isFirstTimeThrough = false;
						}
						else
						{
							_context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [FullText] .WRITE(@textBlock, NULL, 0) WHERE [OriginalLineNumber] = {1}", TableNameImageTemp, id), new SqlParameter[] { ctrl.Item2 }, QueryTimeout);
						}

						if (ctrl.Item1) break;
					}
					while (true);
				}
				else
				{
					sr.FinishLine();
				}
			}

			sr.Close();
			if (System.IO.File.Exists(tempFilePath))
			{
				try
				{
					System.IO.File.Delete(tempFilePath);
				}
				catch
				{
					System.IO.File.Delete(tempFilePath);
				}
			}
		}

		public void CreateMassImportImageTempTables(bool updateFullText, int docFieldArtifactID)
		{
			DataRow stats;

			string fieldWhereClause;
			if (docFieldArtifactID <= 0)
			{
				fieldWhereClause = string.Format("FieldCategoryID = {0} AND Field.FieldArtifactTypeID = {1}", (int)FieldCategory.Identifier, (int)Relativity.ArtifactType.Document);
			}
			else
			{
				fieldWhereClause = "ArtifactID = " + docFieldArtifactID;
			}

			string statsSQL = $@"
SELECT
		DocumentIdentifierUnicodeMarker = CASE WHEN (SELECT UseUnicodeEncoding FROM [Field] WHERE { fieldWhereClause }) = 1 THEN 'N' ELSE '' END,
		DocumentIdentifierFieldLength = (SELECT MaxLength FROM [Field] WHERE { fieldWhereClause }),
		ExtractedTextUnicodeMarker = CASE WHEN (SELECT UseUnicodeEncoding FROM [Field] WHERE FieldCategoryID = { (int)FieldCategory.FullText } AND Field.FieldArtifactTypeID = { (int)ArtifactType.Document }) = 1 THEN 'N' ELSE '' END,
		FullTextColumnCollation = (SELECT collation_name FROM sys.columns WHERE [object_id] = OBJECT_ID(N'eddsdbo.Document', N'U') AND CAST([name] AS NVARCHAR(4000)) COLLATE SQL_Latin1_General_CP1_CI_AS = (SELECT TOP 1 CAST(ColumnName AS NVARCHAR(4000)) COLLATE SQL_Latin1_General_CP1_CI_AS FROM [ArtifactViewField] INNER JOIN [Field] ON ArtifactViewField.ArtifactViewFieldID = Field.ArtifactViewFieldID AND Field.FieldCategoryID = { (int)FieldCategory.FullText } AND Field.FieldArtifactTypeID = { (int)ArtifactType.Document })),
		DocumentIdentifierColumnCollation = (SELECT collation_name FROM sys.columns WHERE [object_id] = OBJECT_ID(N'eddsdbo.Document', N'U') AND CAST([name] AS NVARCHAR(4000)) COLLATE SQL_Latin1_General_CP1_CI_AS = (SELECT TOP 1 CAST(ColumnName AS NVARCHAR(4000)) COLLATE SQL_Latin1_General_CP1_CI_AS FROM [ArtifactViewField] INNER JOIN [Field] ON ArtifactViewField.ArtifactViewFieldID = Field.ArtifactViewFieldID AND { fieldWhereClause })),
		FileIdentifierColumnCollation = (SELECT collation_name FROM sys.columns WHERE [object_id] = OBJECT_ID(N'eddsdbo.File', N'U') AND [name] = 'Identifier')
";
			stats = _context.ExecuteSqlStatementAsDataTable(statsSQL, QueryTimeout).Rows[0];
			string documentIdentifierUnicodeMarker = stats["DocumentIdentifierUnicodeMarker"].ToString();
			int documentIdentifierFieldLength = Convert.ToInt32(stats["DocumentIdentifierFieldLength"]);
			string extractedTextUnicodeMarker = stats["ExtractedTextUnicodeMarker"].ToString();
			string fullTextColumnCollation = stats["FullTextColumnCollation"].ToString();
			string documentIdentifierColumnCollation = stats["DocumentIdentifierColumnCollation"].ToString();
			string fileIdentifierColumnCollation = stats["FileIdentifierColumnCollation"].ToString();
			string sql;
			string fullTextEncodingColumnDefinition = string.Empty;
			string fullTextColumnSql = string.Empty;
			if (updateFullText)
			{
				string collationString = string.IsNullOrEmpty(fullTextColumnCollation) ? string.Empty : string.Format(" COLLATE {0}", fullTextColumnCollation);
				fullTextEncodingColumnDefinition = string.Format("[ExtractedTextEncodingPageCode] {0}VARCHAR(MAX){1},", extractedTextUnicodeMarker, collationString);
				if (!HasDataGridWorkToDo && !string.IsNullOrWhiteSpace(fullTextColumnCollation))
				{
					fullTextColumnSql = string.Format("[FullText] {0}VARCHAR(MAX) COLLATE {1},", extractedTextUnicodeMarker, fullTextColumnCollation);
				}
			}

			sql = string.Format(ImportSql.CreateImageTableFormatString(), TableNameImageTemp, documentIdentifierUnicodeMarker, (object)documentIdentifierFieldLength, documentIdentifierColumnCollation, fileIdentifierColumnCollation, fullTextEncodingColumnDefinition, fullTextColumnSql);
			IsNewJob = _context.ExecuteSqlStatementAsScalar<bool>(sql, QueryTimeout);
			sql = string.Format(ImportSql.CreateImageArtifactsTableFormatString(), TableNameArtifactTemp, documentIdentifierColumnCollation, documentIdentifierUnicodeMarker);
			_context.ExecuteNonQuerySQLStatement(sql, QueryTimeout);
		}

		public void ClearTempTableAndSaveErrors()
		{
			// Do nothing, for now.
		}

		public void TruncateTempTables()
		{
			Helper.TruncateTempTables(_context, _tableNames.RunId);
			string dropString = string.Format("IF EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{0}_ExistingFile') BEGIN DROP TABLE [Resource].[{0}_ExistingFile] END", _tableNames.Image);
			_context.ExecuteNonQuerySQLStatement(dropString, QueryTimeout);
		}

		public int[] GetReturnReport()
		{
			var retval = new int[3];
			var dt = _context.ExecuteSqlStatementAsDataTable(string.Format(ImportSql.GetReturnReport(), TableNameImageTemp, TableNameArtifactTemp), QueryTimeout);
			if (dt.Rows.Count > 0)
			{
				var row = dt.Rows[0];
				retval[0] = Convert.ToInt32(row["NewDocument"]);
				retval[1] = Convert.ToInt32(row["UpdatedDocument"]);
				retval[2] = Convert.ToInt32(row["FileCount"]);
			}

			return retval;
		}
		#endregion

		#region Error File Generation
		public ErrorFileKey GenerateErrorFiles(int caseArtifactID, bool writeHeader)
		{
			var retval = new ErrorFileKey();
			string errorFileName = "";
			string errorRowsName = "";
			string defaultLocation = new Context().ExecuteSqlStatementAsScalar<string>(string.Format("SELECT (SELECT TOP 1 [Url] FROM [ResourceServer] WHERE [ArtifactID] = [DefaultFileLocationCodeArtifactID]) FROM [Case] WHERE [ArtifactID] = {0}", caseArtifactID), QueryTimeout);

			SqlDataReader reader = null;

			try
			{
				reader = _context.ExecuteSQLStatementAsReader(string.Format(ImportSql.GetErrors(), TableNameImageTemp, DocumentIdentifierFieldColumnName), QueryTimeout);
				if (reader.HasRows)
				{
					errorFileName = Guid.NewGuid().ToString();
					errorRowsName = Guid.NewGuid().ToString();
					using (var errorFile = new System.IO.StreamWriter(System.IO.Path.Combine(defaultLocation, errorFileName)))
					{
						using (var errorRows = new System.IO.StreamWriter(System.IO.Path.Combine(defaultLocation, errorRowsName)))
						{
							while (reader.Read())
							{
								string identifier = GetStringValue(reader[2]);
								string documentIdentifier = GetStringValue(reader[1]);
								errorFile.WriteLine(string.Format("\"{1}{0}{2}{0}{3}{0}{4}\"", "\",\"", GetIntegerValue(reader[0]), GetStringValue(reader[1]), identifier, Relativity.MassImport.DTO.ImportStatusHelper.GetCsvErrorLine(reader.GetInt64(5), identifier, GetStringValue(reader[7]), int.Parse(GetIntegerValue(reader[8])), documentIdentifier, reader[9] == null ? null : GetStringValue(reader[9]))));
								string newRecordMarker = GetIntegerValue(reader[4]) == "0" ? "Y" : string.Empty;
								errorRows.WriteLine(string.Format("{0},{1},{2},{3},,,", identifier, _tableNames.RunId, GetStringValue(reader[6]), newRecordMarker));
							}
						}
					}
				}
			}
			finally
			{
				kCura.Data.RowDataGateway.Helper.CloseDataReader(reader);
				_context.ReleaseConnection();
			}

			retval.LogKey = errorFileName;
			retval.OpticonKey = errorRowsName;
			TruncateTempTables();
			return retval;
		}

		private string GetStringValue(object o)
		{
			return NullableTypesHelper.ToEmptyStringOrValue(NullableTypesHelper.DBNullString(o));
		}

		private string GetIntegerValue(object o)
		{
			try
			{
				return NullableTypesHelper.ToEmptyStringOrValue(NullableTypesHelper.DBNullConvertToNullable<int>(o));
			}
			catch (Exception)
			{
				return NullableTypesHelper.ToEmptyStringOrValue(NullableTypesHelper.DBNullString(o));
			}
		}
		#endregion

		#region File Processing
		public void DeleteFilesNotImported()
		{
			// retrieve location field from records from relimgtmp table with an import status > 1
			// Status 512 is InvalidImageFormat, and the image wasn't copied to the server in the first place
			// Status 256 is file not found.
			// Extracted text comes through in a field, not a file, so no need to try and delete that.

			ImportMeasurements.StartMeasure();

			long combinedStatus = (long)(Relativity.MassImport.DTO.ImportStatus.InvalidImageFormat | Relativity.MassImport.DTO.ImportStatus.FileSpecifiedDne | Relativity.MassImport.DTO.ImportStatus.IdentifierOverlap);
			string sql = $"SELECT [Location] FROM [Resource].[{ this.TableNameImageTemp }] WHERE [Status] > 1 AND ([Status] & {combinedStatus} = 0)";

			var dt = _context.ExecuteSqlStatementAsDataTable(sql);

			var fileList = new List<string>();

			if (dt != null)
			{
				foreach (DataRow row in dt.Rows)
				{
					if (row[0] != null)
					{
						string fullpath = ((string)row[0]).Trim();

						if (fullpath.Length > 0)
						{
							fileList.Add(fullpath);
						}
					}
				}

				if (fileList.Count > 0)
				{
					File.Instance.DeleteFilesTask(fileList);
				}
			}

			ImportMeasurements.StopMeasure();
		}

		public void DeleteExistingImageFiles(int userID, bool auditEnabled, string requestOrig, string recordOrig)
		{
			ImportMeasurements.StartMeasure();
			string sqlFormat = ImportSql.DeleteExistingImageFiles();
			int fileType = GetFileType(Settings.HasPDF);
			string auditString = "";
			if (auditEnabled && Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				var sb = new System.Text.StringBuilder(Helper.GenerateAuditInsertClause(14, userID, requestOrig, recordOrig, TableNameImageTemp));
				sb.Append(" WHERE" + Environment.NewLine);
				sb.AppendFormat("{0}[Status] = {2}{1}", "\t", Environment.NewLine, (long)Relativity.MassImport.DTO.ImportStatus.Pending);
				auditString = sb.ToString();
			}

			sqlFormat = sqlFormat.Replace("/*ImageInsertAuditRecords*/", auditString);
			_context.ExecuteNonQuerySQLStatement(string.Format(sqlFormat, TableNameImageTemp, fileType), QueryTimeout);
			ImportMeasurements.StopMeasure();
		}

		public void CreateImageFileRows(int userID, bool auditEnabled, string requestOrig, string recordOrig, bool inRepository)
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			var parameter = new SqlParameter("@fileLocation", Settings.Repository);
			string sqlFormat = ImportSql.CreateImageFileRows();
			int fileType = GetFileType(Settings.HasPDF);

			string auditString = "";
			if (auditEnabled && Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				var sb = new System.Text.StringBuilder(Helper.GenerateAuditInsertClause(13, userID, requestOrig, recordOrig, TableNameImageTemp));
				sb.Append(" WHERE" + Environment.NewLine);
				sb.AppendFormat("{0}[Status] = {2}{1}", "\t", Environment.NewLine, (long)Relativity.MassImport.DTO.ImportStatus.Pending);
				auditString = sb.ToString();
			}

			sqlFormat = sqlFormat.Replace("/*ImageInsertAuditRecords*/", auditString);
			_context.ExecuteNonQuerySQLStatement(string.Format(sqlFormat, TableNameImageTemp, inRepository ? 1 : 0, Settings.Billable ? 1 : 0, fileType), new SqlParameter[] { parameter }, QueryTimeout);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}


		/// <summary>
		/// Add or updates the records in DB related to HasImage or HasPDF for particular document.
		/// </summary>
		public void ManageHasImagesOrHasPDF()
		{
			string codeTypeName = Settings.HasPDF ? Core.Constants.CodeTypeNames.HasPDFCodeTypeName : Core.Constants.CodeTypeNames.HasImagesCodeTypeName;
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string codeArtifactTableName = Relativity.Data.CodeHelper.GetCodeArtifactTableNameByCodeTypeName(_context, codeTypeName);
			string sqlFormat = ImportSql.ManageHasImagesOrHasPDF();

			_context.ExecuteNonQuerySQLStatement(string.Format(sqlFormat, TableNameImageTemp, codeArtifactTableName, codeTypeName), QueryTimeout);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}

		/// <summary>
		/// Add or updates the records in DB related to HasImage for particular document for production flow.
		/// </summary>
		public void ManageHasImagesForProduction()
		{
			string codeTypeName =  Core.Constants.CodeTypeNames.HasImagesCodeTypeName;
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string codeArtifactTableName =
				Relativity.Data.CodeHelper.GetCodeArtifactTableNameByCodeTypeName(_context, codeTypeName);
			string sqlFormat = ImportSql.ManageHasImagesForProductionImport();

			_context.ExecuteNonQuerySQLStatement(string.Format(sqlFormat, TableNameImageTemp, codeArtifactTableName, codeTypeName), QueryTimeout);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}

		public void UpdateImageCount()
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string sql = string.Format(ImportSql.UpdateDocumentImageCount(), TableNameImageTemp);
			_context.ExecuteNonQuerySQLStatement(sql, QueryTimeout);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}

		public void CreateProductionImageFileRows(int productionArtifactID, int userID, bool auditEnabled, string requestOrig, string recordOrig, bool inRepository)
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string sqlFormat = ImportSql.CreateProductionImageFileRows();
			int fileType = GetProducedFileType(Settings.HasPDF);

			string auditString = "";
			if (auditEnabled && Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				var sb = new System.Text.StringBuilder(Helper.GenerateAuditInsertClause(15, userID, requestOrig, recordOrig, TableNameImageTemp));
				sb.AppendFormat(" WHERE{0}", Environment.NewLine);
				sb.AppendFormat("{0}[{1}].[Status] = {3}{2}", "\t", TableNameImageTemp, Environment.NewLine, (long)Relativity.MassImport.DTO.ImportStatus.Pending);
				auditString = sb.ToString();
			}

			sqlFormat = sqlFormat.Replace("/*ImageInsertAuditRecords*/", auditString);

			var productionIdParam = new SqlParameter("@prodID", productionArtifactID);
			var productionIdXmlParam = new SqlParameter("@prodIdXml", SqlDbType.Xml);
			productionIdXmlParam.Value = "<productionid>" + productionArtifactID + "</productionid>";

			_context.ExecuteNonQuerySQLStatement(string.Format(sqlFormat, TableNameImageTemp, inRepository ? 1 : 0, Settings.Billable ? 1 : 0, fileType), new SqlParameter[] { productionIdParam, productionIdXmlParam }, QueryTimeout);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}
		#endregion

		#region Artifact Processing
		public void CreateDocumentsFromImageFile(int userArtifactID, string requestOrigination, string recordOrigination, bool auditEnabled, bool isAppendOverlayMode)
		{
			ImportMeasurements.StartMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Start();
			string extractedTextEncodingColumnName = ExtractedTextCodePageColumnName;
			var relationalColumns = _context.ExecuteSqlStatementAsDataTable(string.Format("SELECT [ArtifactID], [ColumnName], [ImportBehavior] FROM [Field] INNER JOIN [ArtifactViewField] ON [ArtifactViewField].[ArtifactViewFieldID] = [Field].[ArtifactViewFieldID] WHERE [Field].[FieldCategoryID] = {0}", (int)FieldCategory.Relational), QueryTimeout);
			string formatString = ImportSql.CreateNewDocumentsFromImageLoad();
			if (!Settings.DisableUserSecurityCheck)
			{
				formatString = formatString.Replace("/* HasPermissionsToAddCheck */", HasPermissionsToaddCheck());
			}

			if (auditEnabled && Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				if (Settings.UploadFullText)
				{
					formatString = formatString.Replace("/* InsertAuditRecords */", ImportSql.CreateWhenExtractedTextIsEnabledAuditClause());
					string fullTextOverlayDetail = Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit ? GetFullTextOverlayDetail() : "''";
					formatString = formatString.Replace("/* FullTextOverlayDetail */", fullTextOverlayDetail);
					formatString = formatString.Replace("/* DetailsValueColumnName */", extractedTextEncodingColumnName);
				}
				else
				{
					extractedTextEncodingColumnName = "''";
					formatString = formatString.Replace("/* InsertAuditRecords */", ImportSql.CreateAuditClause().Replace("/* DetailsValueColumnName */", extractedTextEncodingColumnName));
				}
			}

			if (isAppendOverlayMode && !Settings.DisableUserSecurityCheck)
			{
				formatString = formatString.Replace("/* UpdateOverlayPermissionsForAppendOverlayMode */", ImportSql.UpdateOverlayPermissionsForAppendOverlayMode());
			}

			string sql = string.Format(formatString, TableNameImageTemp, TableNameArtifactTemp, DocumentIdentifierFieldColumnName, GetRelationalColumnSelectBlock(relationalColumns), GetRelationalColumnSetBlock(relationalColumns), requestOrigination, recordOrigination);
			var parameters = new SqlParameter[]
			{
				new SqlParameter("@userID", userArtifactID),
				new SqlParameter("@parentArtifactID", Settings.DestinationFolderArtifactID)

			};
			_context.ExecuteNonQuerySQLStatement(sql, parameters, QueryTimeout);
			ImportMeasurements.StopMeasure();
			ImportMeasurements.PrimaryArtifactCreationTime.Stop();
		}

		private string GetFullTextOverlayDetail()
		{
			return $@"
[/* DetailsValueColumnName */] = CASE
WHEN [/* DetailsValueColumnName */] IS NULL THEN '<auditElement><extractedTextEncodingPageCode>' + '-1' + '</extractedTextEncodingPageCode></auditElement>'
else '<auditElement><extractedTextEncodingPageCode>' +  [/* DetailsValueColumnName */] + '</extractedTextEncodingPageCode></auditElement>'
End
";
		}

		private string HasPermissionsToaddCheck()
		{
			return $@"
IF @HasPermissionToAdd = 0 BEGIN
	UPDATE [Resource].[{{0}}] SET [Status] = [Status] + { (long)Relativity.MassImport.DTO.ImportStatus.SecurityAdd } WHERE NOT EXISTS(SELECT ArtifactID FROM [Document] (NOLOCK) WHERE [Document].[{{2}}] = [{{0}}].[DocumentIdentifier])
END
";
		}

		public void PopulateArtifactIdOnInitialTempTable(int userID, bool updateOverlayPermissions)
		{
			ImportMeasurements.StartMeasure();
			string sqlStatement = ImportSql.PopulateArtifactIdColumnOnTempTable();
			if (updateOverlayPermissions && !Settings.DisableUserSecurityCheck)
			{
				sqlStatement = sqlStatement.Replace("/* UpdateOverlayPermissions */", ImportSql.UpdateOverlayPermissions());
			}

			_context.ExecuteNonQuerySQLStatement(string.Format(sqlStatement, TableNameImageTemp, DocumentIdentifierFieldColumnName, TableNameArtifactTemp, userID, "Document"), QueryTimeout);
			ImportMeasurements.StopMeasure();
		}

		public void UpdateArtifactAuditColumns(int userID)
		{
			ImportMeasurements.StartMeasure();
			_context.ExecuteNonQuerySQLStatement(string.Format(ImportSql.UpdateArtifactAuditRecords(), TableNameImageTemp), new SqlParameter[] { new SqlParameter("@userID", userID) }, QueryTimeout);
			ImportMeasurements.StopMeasure();
		}

		public void UpdateDocumentMetadata(int userID, string reqOrig, string recOrig, bool performAudit)
		{
			string sqlFormat;
			if (performAudit && Settings.AuditLevel != Relativity.MassImport.DTO.ImportAuditLevel.NoAudit)
			{
				sqlFormat = ImportSql.UpdateAuditClause();
				if (Settings.UploadFullText)
				{
					ImportMeasurements.StartMeasure();

					string fullTextColumnDefinition = FullTextField.EnableDataGrid ? string.Empty : ",[FullText] = CASE WHEN ISNULL([ExtractedTextEncodingPageCode], '-1') <> '-1' THEN ISNULL([FullText], N'') ELSE [FullText] END ";
					string auditDetailsClause = Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit ? GenerateAuditDetailsClause() : "''";

					string toRun = string.Format(sqlFormat, TableNameImageTemp, auditDetailsClause, "Document", DocumentIdentifierFieldColumnName, fullTextColumnDefinition);
					var parameters = new SqlParameter[3];
					parameters[0] = new SqlParameter("@userID", userID);
					parameters[1] = new SqlParameter("@requestOrig", reqOrig);
					parameters[2] = new SqlParameter("@recordOrig", recOrig);
					_context.ExecuteNonQuerySQLStatement(toRun, parameters, QueryTimeout);

					ImportMeasurements.StopMeasure();
				}
			}
		}

		private string GenerateAuditDetailsClause()
		{
			var auditDetailsClause = new System.Text.StringBuilder("'<auditElement>' + ");
			auditDetailsClause.AppendFormat("'<field id=\"{0}\" ", FullTextField.ArtifactID);
			auditDetailsClause.AppendFormat("type=\"{0}\" ", (int)FullTextField.Type);
			if (FullTextField.EnableDataGrid)
			{
				auditDetailsClause.Append("datagridenabled=\"true\" ");
			}
			auditDetailsClause.AppendFormat("name=\"{0}\" ", System.Security.SecurityElement.Escape(FullTextField.DisplayName));
			auditDetailsClause.AppendFormat("formatstring=\"{0}\">' + ", FullTextField.FormatString.Replace("'", "''"));

			if (!FullTextField.EnableDataGrid)
			{
				// BIGDATA_ET_1037720
				string formatString = string.Format("'<{{0}}Value>' + CASE WHEN [{{1}}].[{{2}}] IS NULL THEN '' ELSE [{{1}}].[{{2}}] COLLATE {0} END + '</{{0}}Value>'", AuditRecordDetailsCollation);
				auditDetailsClause.AppendFormat(formatString, "old", "Document", FullTextField.GetColumnName());
				auditDetailsClause.Append(" + ");
				auditDetailsClause.AppendFormat(formatString, "new", "ImportExtractedText", "FullText");
				auditDetailsClause.Append(" + ");
			}

			auditDetailsClause.Append("'</field>' + ");

			auditDetailsClause.Append("'<extractedTextEncodingPageCode>' + ");
			string extractedTextEncodingValue = string.Format("CASE WHEN [{0}] IS NULL THEN '-1'  else [{0}] END + ", ExtractedTextCodePageColumnName);
			auditDetailsClause.Append(extractedTextEncodingValue);
			auditDetailsClause.Append("'</extractedTextEncodingPageCode>' + ");

			auditDetailsClause.Append("'</auditElement>'");
			return auditDetailsClause.ToString();
		}
		#endregion

		#region Full Text Managment
		public void ManageImageFullText()
		{
			if (IsTextMigrationInProgress)
			{
				ImportMeasurements.StartMeasure();
				ImportMeasurements.PrimaryArtifactCreationTime.Start();

				_context.ExecuteSqlStatementAsDataTable(string.Format(ImportSql.ManageImageFullText(), TableNameImageTemp, TableNameArtifactTemp, FullTextField.GetColumnName()), QueryTimeout);

				ImportMeasurements.StopMeasure();
				ImportMeasurements.PrimaryArtifactCreationTime.Stop();
			}
		}

		public IDataReader CreateDataGridMappingDataReader()
		{
			string friendlyImageTempTableName = SqlNameHelper.GetSqlFriendlyName(TableNameImageTemp);
			string sql = $@"
SELECT
	tmp.[kCura_Import_ID],
	tmp.[DocumentIdentifier],
	NULL,
	tmp.[kCura_Import_ID]
FROM
	[Resource].[{friendlyImageTempTableName}] tmp
WHERE
	tmp.[Order] = 0
	AND
	tmp.[Status] = {_PENDING_STATUS}
";
			return _context.ExecuteSQLStatementAsReader(sql);
		}

		public DataGridReader CreateDataGridReader(string bulkFileShareFolderPath, ILog correlationLogger)
		{
			correlationLogger.LogVerbose("Starting CreateDataGridReader");
			if (!HasDataGridWorkToDo)
			{
				return null;
			}

			ImportMeasurements.DataGridImportTime.Start();
			FieldInfo[] mappedFields;
			if (Settings.UploadFullText)
			{
				mappedFields = new[] { FullTextField };
			}
			else
			{
				mappedFields = new FieldInfo[] { };
			}

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "DocumentIdentifier",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				SqlTempTableName = TableNameImageTemp,
				IsImageFullTextImport = true
			};

			string dataGridFilePath = System.IO.Path.Combine(bulkFileShareFolderPath, Settings.DataGridFileName);
			correlationLogger.LogDebug("CreateDataGridTempFileReader: DataGridFilePath is: {DataGridFilePath}", dataGridFilePath);
			var reader = new DataGridTempFileDataReader(options, _FIELDTERMINATOR, Relativity.Constants.ENDLINETERMSTRING, dataGridFilePath, correlationLogger);
			var mismatchedFields = new List<FieldInfo>();

			// If we are uploading extracted text in the image and the import started before a text migration job is started, then we will have a mismatch
			// in the way the client constructs the bulk file. If this is the case, we need to tell the DataGridReader to read from SQL
			if (Settings.UploadFullText && FullTextField.EnableDataGrid && !reader.HasRows && AreRowsInSqlFile())
			{
				mismatchedFields.Add(FullTextField);
			}

			var sqlTempReader = new DataGridSqlTempReader(_context);
			var loader = new DataGridReader(_dgContext, _context.Clone(), options, reader, correlationLogger, mismatchedFields, sqlTempReader);
			ImportMeasurements.DataGridImportTime.Stop();
			correlationLogger.LogVerbose("Ending CreateDataGridTempFileReader");
			return loader;
		}

		public void UpdateDgFieldMappingRecords(IEnumerable<DGImportFileInfo> dgImportFileInfoList, ILog correlationLogger)
		{
			if (dgImportFileInfoList.Any())
			{
				ImportMeasurements.StartMeasure();
				string sqlStatement = DGRelativityRepository.UpdateDgFieldMappingRecordsSql(_tableNames.Image, "Status");

				var sqlParam = new SqlParameter("@dgImportFileInfo", dgImportFileInfoList.GetDgImportFileInfoAsDataRecord());
				sqlParam.SqlDbType = SqlDbType.Structured;
				sqlParam.TypeName = "EDDSDBO.DgImportFileInfoType";

				var filter = new HashSet<int>();

				using (var reader = _context.ExecuteSQLStatementAsReader(sqlStatement, Enumerable.Repeat(sqlParam, 1), QueryTimeout))
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

		private bool AreRowsInSqlFile()
		{
			string sql = $"SELECT COUNT(*) FROM [Resource].[{TableNameImageTemp}]";
			int numberOfRows = _context.ExecuteSqlStatementAsScalar<int>(sql);
			return numberOfRows > 0;
		}

		public bool HasDataGridWorkToDo => Settings.UploadFullText && FullTextField.EnableDataGrid;

		public bool IsDataGridInputValid()
		{
			return !string.IsNullOrEmpty(Settings.DataGridFileName) && SQLInjectionHelper.IsValidFileName(Settings.DataGridFileName);
		}

		public void WriteToDataGrid(DataGridReader loader, int appID, string bulkFileShareFolderPath, ILog correlationLogger)
		{
			try
			{
				ImportMeasurements.StartMeasure();
				ImportMeasurements.DataGridImportTime.Start();
				if (!HasDataGridWorkToDo)
				{
					return;
				}

				if (loader == null)
				{
					correlationLogger.LogError("DataGridReader is null. {HasDataGridWorkToDo}", HasDataGridWorkToDo);
					throw new ArgumentNullException(nameof(loader));
				}

				using (var mappingReader = CreateDataGridMappingDataReader())
				{
					string indexName = DataGridHelper.GetWriteIndexName(appID, (int)Relativity.ArtifactType.Document, Relativity.Data.Config.DataGridConfiguration.DataGridIndexPrefix);
					_dataGridMappings.LoadCacheForImport(mappingReader, indexName, appID);
				}

				_dgImportHelper.WriteToDataGrid((int)Relativity.ArtifactType.Document, appID, _tableNames.RunId, loader, false, true, _dataGridMappings, correlationLogger);
			}
			finally
			{
				CleanupDataGridInput(bulkFileShareFolderPath, correlationLogger);
				ImportMeasurements.StopMeasure();
				ImportMeasurements.DataGridImportTime.Stop();
			}
		}

		protected virtual void CleanupDataGridInput(string bulkFileShareFolderPath, ILog correlationLogger)
		{
			string dataGridFilePath = System.IO.Path.Combine(bulkFileShareFolderPath, Settings.DataGridFileName);
			correlationLogger.LogDebug("Deleting {dataGridTempFile}", dataGridFilePath);
			File.Instance.Delete(dataGridFilePath);
		}

		public void MapDataGridRecords(ILog correlationLogger)
		{
			if (!HasDataGridWorkToDo)
			{
				return;
			}

			ImportMeasurements.StartMeasure();
			string tempTableName = GetTempTableName();
			_dgImportHelper.UpdateErrors(tempTableName, _STATUS_COLUMN_NAME, _FILTER_BY_ORDER, correlationLogger);
			ImportMeasurements.StopMeasure();
		}

		public string GetTempTableName()
		{
			return SqlNameHelper.GetSqlFriendlyName(TableNameImageTemp);
		}
		#endregion

		#region Process Error Managment
		public void ManageOverwriteErrors()
		{
			if (!Settings.DisableUserSecurityCheck)
			{
				ImportMeasurements.StartMeasure();
				_context.ExecuteNonQuerySQLStatement(string.Format(ImportSql.OverwriteOnlyErrors(), TableNameImageTemp, DocumentIdentifierFieldColumnName), QueryTimeout);
				ImportMeasurements.StopMeasure();
			}
		}

		public void ManageAppendErrors()
		{
			if (!Settings.DisableUserSecurityCheck)
			{
				ImportMeasurements.StartMeasure();
				_context.ExecuteNonQuerySQLStatement(string.Format(ImportSql.AppendOnlyErrors(), TableNameImageTemp, DocumentIdentifierFieldColumnName), QueryTimeout);
				ImportMeasurements.StopMeasure();
			}
		}

		public void ManageRedactionErrors()
		{
			ImportMeasurements.StartMeasure();
			string statement = null;
			statement = ImportSql.RedactionOverwriteErrorsWithImprovedJoinOrder(TableNameImageTemp, DocumentIdentifierFieldColumnName);
			_context.ExecuteNonQuerySQLStatement(statement, QueryTimeout);
			ImportMeasurements.StopMeasure();
		}

		public void ManageBatesExistsErrors()
		{
			ImportMeasurements.StartMeasure();
			_context.ExecuteNonQuerySQLStatement(string.Format(ImportSql.BatesExistsErrors(), TableNameImageTemp, DocumentIdentifierFieldColumnName), QueryTimeout);
			ImportMeasurements.StopMeasure();
		}
		#endregion

		#region Utility
		private string GetRelationalColumnSelectBlock(System.Data.DataTable dt)
		{
			var sb = new System.Text.StringBuilder();
			foreach (DataRow row in dt.Rows)
			{
				if ((FieldInfo.ImportBehaviorChoice)row["ImportBehavior"] == FieldInfo.ImportBehaviorChoice.ReplaceBlankValuesWithIdentifier)
					sb.AppendFormat("\t" + "[{0}]," + Environment.NewLine, row["ColumnName"]);
			}

			return sb.ToString();
		}

		private string GetRelationalColumnSetBlock(System.Data.DataTable dt)
		{
			var sb = new System.Text.StringBuilder();
			foreach (DataRow row in dt.Rows)
			{
				if ((FieldInfo.ImportBehaviorChoice)row["ImportBehavior"] == FieldInfo.ImportBehaviorChoice.ReplaceBlankValuesWithIdentifier)
					sb.Append("\t" + "[TextIdentifier]," + Environment.NewLine);
			}

			return sb.ToString();
		}

		private bool DoesFullTextColumnText()
		{
			var columnExistParameters = new[] { new SqlParameter("@columnName", SqlDbType.VarChar) { Value = FullTextField.GetColumnName() } };
			bool doesColumnExist = _context.ExecuteSqlStatementAsScalar<int>(ImportSql.DoesColumnExistOnDocumentTable(), columnExistParameters) > 0;
			return doesColumnExist;
		}

		private static int GetFileType(bool hasPDF)
		{
			return hasPDF ? Core.Constants.FileTypes.PDFFileType : Core.Constants.FileTypes.ImageFileType;
		}

		private static int GetProducedFileType(bool hasPDF)
		{
			return hasPDF ? Core.Constants.FileTypes.ProducedPDFFileType : Core.Constants.FileTypes.ProducedImageFileType;
		}
		#endregion
	}
}