using System;
using System.Linq;
using System.Text;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Data
{
	using DataTransfer.Legacy.MassImport.Data.Cache;

	internal class AuditDetailsBuilder
	{
		private kCura.Data.RowDataGateway.BaseContext Context { get; }
		private Relativity.MassImport.DTO.NativeLoadInfo Settings { get; }
		protected IColumnDefinitionCache ColumnDefinitionCache { get; private set; }
		private bool IsDocument { get; }

		public const string ExtractedTextCodePageColumnName = "ExtractedTextEncodingPageCode";
		private readonly TableNames _tableNames;
		private int? _timeoutValue;

		public AuditDetailsBuilder(
			kCura.Data.RowDataGateway.BaseContext context,
			Relativity.MassImport.DTO.NativeLoadInfo settings,
			IColumnDefinitionCache columnDefinitionCache,
			TableNames tableNames,
			int artifactTypeId)
		{
			Context = context;
			Settings = settings;
			ColumnDefinitionCache = columnDefinitionCache;
			_tableNames = tableNames;
			IsDocument = artifactTypeId == (int)Relativity.ArtifactType.Document;
		}

		public Tuple<string, string> GenerateAuditDetails(bool performAudit, bool includeExtractedTextEncoding)
		{
			var auditRecordDetailsCollation = ExecuteSqlStatementAsScalar<string>(
				"SELECT collation_name FROM sys.columns WHERE [name] = 'Details' AND [object_id] = OBJECT_ID('[EDDSDBO].[AuditRecord]')");
			var detailsClause = new System.Text.StringBuilder();
			var mapClause = new StringBuilder();
			detailsClause.AppendLine("	CAST(N'<auditElement>' AS NVARCHAR(MAX)) +");

			if (performAudit && Settings.AuditLevel == Relativity.MassImport.DTO.ImportAuditLevel.FullAudit)
			{
				foreach (FieldInfo mappedField in Settings.MappedFields)
				{
					if (mappedField.Category == FieldCategory.AutoCreate && !Settings.UploadFiles)
					{
					}
					else if (mappedField.ArtifactID == this.GetKeyField().ArtifactID)
					{
						// do not audit overlays of the import identifier itself, it can't be changed
					}

					else if (Settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Overlay && mappedField.Category == FieldCategory.Identifier && mappedField.ArtifactID != this.GetKeyField().ArtifactID && IsDocument)
					{
						// do not audit identifier field when overlaying on different field as value won't be changed
					}
					else
					{
						detailsClause.AppendFormat("		'<field id=\"{0}\" ", mappedField.ArtifactID);
						detailsClause.AppendFormat("type=\"{0}\" ", (int)mappedField.Type);
						// Fully XML encoded, refactor for XML Attribute Encoding only
						if (mappedField.EnableDataGrid)
						{
							detailsClause.Append("datagridenabled=\"true\" ");
						}
						detailsClause.AppendFormat("name=\"{0}\" ", System.Security.SecurityElement.Escape(mappedField.DisplayName));
						detailsClause.AppendFormat("formatstring=\"{0}\">' + ", mappedField.FormatString.Replace("'", "''"));
						detailsClause.AppendLine();
						string map = null;
						string detail;
						switch (mappedField.Type)
						{
							case FieldTypeHelper.FieldType.Boolean:
								{
									detail = "'<{0}Value>' + CASE {1}.[{2}] WHEN 1 THEN 'True' WHEN 0 THEN 'False' ELSE '' END + '</{0}Value>'";
									break;
								}

							case FieldTypeHelper.FieldType.Code:
								{
									map = $@",
		'<unsetChoice>' + (SELECT CAST(MappedArtifactID AS VARCHAR(10)) FROM [Resource].[{_tableNames.Map}] M1 WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = {mappedField.ArtifactID} AND M1.IsNew = 0) + '</unsetChoice>' [{mappedField.GetColumnName()}]";
									detail = $@"			ISNULL('<setChoice>' + NULLIF(N.[{mappedField.GetColumnName()}] COLLATE {auditRecordDetailsCollation}, '') + '</setChoice>', '') +
			ISNULL(GM.[{mappedField.GetColumnName()}] COLLATE {auditRecordDetailsCollation}, '') +";
									break;
								}

							case FieldTypeHelper.FieldType.MultiCode:
								{
									map = $@",
		(
			SELECT
				MappedArtifactID [setChoice]
			FROM [Resource].[{_tableNames.Map}] M1
			WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = {mappedField.ArtifactID} AND M1.IsNew = 1
			FOR XML PATH (''), TYPE
		) [{mappedField.GetColumnName()} IsNew]";
									detail = $"			ISNULL(CAST(GM.[{mappedField.GetColumnName()} IsNew] AS NVARCHAR(MAX)) COLLATE {auditRecordDetailsCollation}, '') +";
									if (!Helper.IsMergeOverlayBehavior(Settings.OverlayBehavior, mappedField.Type, ColumnDefinitionCache[mappedField.ArtifactID].OverlayMergeValues))
									{
										map = $@"{map},
		(
			SELECT
				MappedArtifactID [unsetChoice]
			FROM [Resource].[{_tableNames.Map}] M1
			WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = {mappedField.ArtifactID} AND M1.IsNew = 0
			FOR XML PATH (''), TYPE
		) [{mappedField.GetColumnName()}]";
										detail = $@"{detail}
			ISNULL(CAST(GM.[{mappedField.GetColumnName()}] AS NVARCHAR(MAX)) COLLATE {auditRecordDetailsCollation}, '') +";
									}

									break;
								}

							case FieldTypeHelper.FieldType.Date:
								{
									detail = $"'<{{0}}Value>' + ISNULL(CONVERT(NVARCHAR(23), {{1}}.[{{2}}], 120) COLLATE {auditRecordDetailsCollation}, '') + '</{{0}}Value>'";
									break;
								}

							case FieldTypeHelper.FieldType.Varchar:
								{
									detail = $"'<{{0}}Value><![CDATA[' + ISNULL({{1}}.[{{2}}] COLLATE {auditRecordDetailsCollation}, '') + ']]></{{0}}Value>'";
									break;
								}

							case FieldTypeHelper.FieldType.Text:
								{
									detail = $"'<{{0}}Value>' + IIF(DATALENGTH({{1}}.[{{2}}]) > 1000000, '<![CDATA[NOTE:Contents of field are longer than 1MB; value-audit skipped]]>', '<![CDATA[' + ISNULL({{1}}.[{{2}}] COLLATE {auditRecordDetailsCollation}, '') + ']]>') + '</{{0}}Value>'";
									break;
								}

							case FieldTypeHelper.FieldType.File:
								{
									detail = $"'<{{0}}Value>' + ISNULL(CONVERT(NVARCHAR(200), {{1}}.[{{2}}]) COLLATE {auditRecordDetailsCollation}, '') + '</{{0}}Value>'";
									break;
								}

							case FieldTypeHelper.FieldType.Objects:
								{
									map = $@",
		(
			SELECT
				MappedArtifactID [set]
			FROM [Resource].[{_tableNames.Map}] M1
			WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = {mappedField.ArtifactID} AND M1.IsNew = 1
			FOR XML PATH (''), TYPE
		) [{mappedField.GetColumnName()} IsNew]";
									detail = $"			ISNULL(CAST(GM.[{mappedField.GetColumnName()} IsNew] AS NVARCHAR(MAX)) COLLATE {auditRecordDetailsCollation}, '') +";
									if (!Helper.IsMergeOverlayBehavior(Settings.OverlayBehavior, mappedField.Type, ColumnDefinitionCache[mappedField.ArtifactID].OverlayMergeValues))
									{
										map = $@"{map},
		(
			SELECT
				MappedArtifactID [unset]
			FROM [Resource].[{_tableNames.Map}] M1
			WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = {mappedField.ArtifactID} AND M1.IsNew = 0
			FOR XML PATH (''), TYPE
		) [{mappedField.GetColumnName()}]";
										detail = $@"{detail}
			ISNULL(CAST(GM.[{mappedField.GetColumnName()}] AS NVARCHAR(MAX)) COLLATE {auditRecordDetailsCollation}, '') +";
									}

									break;
								}

							default:
								{
									detail = $"'<{{0}}Value>' + ISNULL(CONVERT(NVARCHAR(MAX), {{1}}.[{{2}}]) COLLATE {auditRecordDetailsCollation}, '') + '</{{0}}Value>'";
									break;
								}
						}

						switch (mappedField.Type)
						{
							case FieldTypeHelper.FieldType.Code:
							case FieldTypeHelper.FieldType.MultiCode:
								{
									mapClause.Append(map);
									detailsClause.AppendLine(detail);
									break;
								}

							case FieldTypeHelper.FieldType.File:
								{
									detailsClause.Append("			");
									detailsClause.AppendFormat(detail, "old", "F", "FileName");
									detailsClause.AppendLine(" +");
									detailsClause.Append("			");
									detailsClause.AppendFormat(detail, "new", "N", mappedField.GetColumnName() + "_ImportObject_FileName");
									detailsClause.AppendLine(" +");
									break;
								}

							case FieldTypeHelper.FieldType.Objects:
								{
									mapClause.Append(map);
									detailsClause.Append(detail);
									detailsClause.AppendLine(" +");
									break;
								}

							default:
								{
									if (!mappedField.EnableDataGrid)
									{
										detailsClause.Append("			");
										detailsClause.AppendFormat(detail, "old", " DELETED", mappedField.GetColumnName());
										detailsClause.AppendLine(" +");
										detailsClause.Append("			");
										detailsClause.AppendFormat(detail, "new", "INSERTED", mappedField.GetColumnName());
										detailsClause.AppendLine(" +");
									}

									break;
								}
						}

						detailsClause.AppendLine("		'</field>' +");
					}
				}
			}

			if (includeExtractedTextEncoding)
			{
				detailsClause.Append("	'<extractedTextEncodingPageCode>' + ");
				detailsClause.AppendFormat("ISNULL(CAST(N.[{0}] AS NVARCHAR(200)), '-1') + ", ExtractedTextCodePageColumnName);
				detailsClause.AppendLine("'</extractedTextEncodingPageCode>' +");
			}

			detailsClause.Append("	'</auditElement>',");
			return Tuple.Create(detailsClause.ToString(), mapClause.ToString());
		}

		private FieldInfo GetKeyField()
		{
			return Settings.MappedFields.FirstOrDefault(f => f.ArtifactID == Settings.KeyFieldArtifactID);
		}

		private T ExecuteSqlStatementAsScalar<T>(string statement)
		{
			return Context.ExecuteSqlStatementAsScalar<T>(statement, null, QueryTimeout);
		}

		private int QueryTimeout
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
	}
}