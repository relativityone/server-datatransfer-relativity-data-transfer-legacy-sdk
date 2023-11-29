using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using kCura.Data.RowDataGateway;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Data.StagingTables
{
	internal class NativeStagingTableRepository : BaseStagingTableRepository, IStagingTableRepository
	{
		public NativeStagingTableRepository(BaseContext context, TableNames tableNames, ImportMeasurements importMeasurements) : base(context, tableNames, importMeasurements)
		{
		}

		public override string Insert(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool excludeFolderPathForOldClient)
		{
			try
			{
				this.ImportMeasurements.SqlBulkImportTime.Start();
				string importNativeFileName = Path.Combine(settings.Repository, settings.DataFileName);
				string importCodeFileName = Path.Combine(settings.Repository, settings.CodeFileName);
				var sr = new NativeTempFileReader(importNativeFileName, settings.MappedFields);
				while (!sr.Eof)
				{
					var cells = sr.ReadLine(settings.MappedFields);
					var sql = new System.Text.StringBuilder();
					sql.AppendFormat("INSERT INTO [Resource].[{0}] (" + Environment.NewLine, this.TableNames.Native);
					bool isFirst = true;
					foreach (NativeTempFileReader.Cell cell in cells)
					{
						if (!cell.IsLargeValue)
						{
							if (!isFirst)
							{
								sql.Append("," + Environment.NewLine);
							}

							isFirst = false;
							sql.Append("\t");
							sql.Append("[");
							sql.Append(cell.ColumnName);
							sql.Append("]");
						}
					}

					sql.Append(Environment.NewLine);
					sql.Append(") VALUES (");
					isFirst = true;
					foreach (NativeTempFileReader.Cell cell in cells)
					{
						if (!cell.IsLargeValue)
						{
							if (!isFirst)
							{
								sql.Append("," + Environment.NewLine);
							}

							isFirst = false;
							sql.Append("\t");
							sql.Append(cell.ParameterName);
						}
					}

					sql.Append(")");
					var parameters = new List<SqlParameter>();
					foreach (NativeTempFileReader.Cell cell in cells)
					{
						if (!cell.IsLargeValue)
						{
							var param = new SqlParameter();
							param.ParameterName = cell.ParameterName;
							var field = GetFieldMappedToCell(settings, cell);
							if (field != null && IsFieldUnicodeStringType(field))
							{
								param.SqlDbType = SqlDbType.NVarChar;
							}

							param.Value = cell.Value;
							parameters.Add(param);
						}
					}

					this.Context.ExecuteNonQuerySQLStatement(sql.ToString(), parameters, this.QueryTimeout);
					int originalLineNumber = int.Parse(cells[3].Value.ToString());
					foreach (NativeTempFileReader.Cell cell in cells)
					{
						if (cell.IsLargeValue)
						{
							// sr.Pause()
							NativeTempFileReader.StreamMarker cellParam = (NativeTempFileReader.StreamMarker)cell.Value;
							using (var textreader = new StreamReader(importNativeFileName, System.Text.Encoding.Unicode, true))
							{
								textreader.BaseStream.Seek(cellParam.Start, SeekOrigin.Begin);
								int length = 0;
								bool isFirstTimeThrough = true;
								var sb = new System.Text.StringBuilder();
								var field = GetFieldMappedToCell(settings, cell);
								while (length < cellParam.Length)
								{
									sb.Append((char)textreader.Read());
									if (sb.Length > 999999)
									{
										if (isFirstTimeThrough)
										{
											this.Context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [{1}] = @textBlock WHERE [kCura_Import_OriginalLineNumber] = {2}", this.TableNames.Native, cell.ColumnName, originalLineNumber), GetTextBlockParameter(field, sb), this.QueryTimeout);
											isFirstTimeThrough = false;
										}
										else
										{
											this.Context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [{1}] .WRITE(@textBlock, NULL, 0) WHERE [kCura_Import_OriginalLineNumber] = {2}", this.TableNames.Native, cell.ColumnName, originalLineNumber), GetTextBlockParameter(field, sb), this.QueryTimeout);
										}

										sb = new System.Text.StringBuilder();
									}

									length += 1;
								}

								if (sb.Length > 0)
								{
									this.Context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [{1}] .WRITE(@textBlock, NULL, 0) WHERE [kCura_Import_OriginalLineNumber] = {2}", this.TableNames.Native, cell.ColumnName, (object)originalLineNumber), GetTextBlockParameter(field, sb), this.QueryTimeout);
								}
							}
						}
					}
					// sr.Play()
				}

				sr.Close();
				sr = new NativeTempFileReader(importCodeFileName, settings.MappedFields);
				while (!sr.Eof)
				{
					var line = sr.ReadCodeLine;
					var parameters = new SqlParameter[3];
					parameters[0] = new SqlParameter("@documentIdentifier", line[0]);
					parameters[1] = new SqlParameter("@codeArtifactID", line[1]);
					parameters[2] = new SqlParameter("@codeTypeID", line[2]);
					if (GetIdentifierField(settings).IsUnicodeEnabled)
					{
						parameters[0].SqlDbType = SqlDbType.NVarChar;
						parameters[0].Value = line[0];
					}

					this.Context.ExecuteNonQuerySQLStatement(string.Format("INSERT INTO [Resource].[{0}] ([DocumentIdentifier], [CodeArtifactID], [CodeTypeID]) VALUES (@documentIdentifier, @codeArtifactID, @codeTypeID)", this.TableNames.Code), parameters, this.QueryTimeout);
				}

				sr.Close();
				kCura.Utility.File.Instance.Delete(importNativeFileName);
				kCura.Utility.File.Instance.Delete(importCodeFileName);
				return this.TableNames.RunId;
			}
			finally
			{
				this.ImportMeasurements.SqlBulkImportTime.Stop();
			}
		}

		public override void CreateStagingTables(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool includeExtractedTextEncoding, bool excludeFolderPathForOldClient)
		{
			var metadataColumnClause = new System.Text.StringBuilder();
			foreach (FieldInfo mappedField in settings.MappedFields)
			{
				if (!mappedField.EnableDataGrid)
				{
					metadataColumnClause.AppendFormat("		[{0}] {1}," + Environment.NewLine, mappedField.GetColumnName(), this.GetColumnDefinition(columnDefinitionCache, mappedField));
				}
			}

			if (includeExtractedTextEncoding)
			{
				metadataColumnClause.AppendLine("		[ExtractedTextEncodingPageCode] INT NULL,");
			}

			// If this is an old client, replace the Folder Path column placeholder with null,
			// so that the bulk load file can be properly inserted.  Failure to do this results in
			// only every other row being imported, and would lead to the creation of very strange
			// folders, if somehow the folder creation logic in the mass importer were executed.
			if (!excludeFolderPathForOldClient)
			{
				metadataColumnClause.AppendLine("		[kCura_Import_ParentFolderPath] NVARCHAR(MAX),");
			}

			this.CreateStagingTablesBase(columnDefinitionCache, settings.MappedFields, metadataColumnClause.ToString(), settings.KeyFieldArtifactID, settings.LoadImportedFullTextFromServer);
		}

		private FieldInfo GetFieldMappedToCell(Relativity.MassImport.DTO.NativeLoadInfo settings, NativeTempFileReader.Cell cell)
		{
			foreach (FieldInfo field in settings.MappedFields)
			{
				if (field.GetColumnName() == cell.ColumnName)
				{
					return field;
				}
			}

			return null;
		}

		private bool IsFieldUnicodeStringType(FieldInfo field)
		{
			if (field.IsUnicodeEnabled == false) return false;
			if (field.Type == FieldTypeHelper.FieldType.Text) return true;
			if (field.Type == FieldTypeHelper.FieldType.Varchar) return true;
			return false;
		}

		private SqlParameter[] GetTextBlockParameter(FieldInfo field, System.Text.StringBuilder value)
		{
			var param = new SqlParameter();
			param.ParameterName = "@textBlock";
			if (field != null && IsFieldUnicodeStringType(field))
			{
				param.SqlDbType = SqlDbType.NVarChar;
			}

			param.Value = value.ToString();
			return new[] { param };
		}

		private FieldInfo GetIdentifierField(Relativity.MassImport.DTO.NativeLoadInfo settings)
		{
			foreach (FieldInfo mappedField in settings.MappedFields)
			{
				if (mappedField.Category == FieldCategory.Identifier)
				{
					return mappedField;
				}
			}

			int ArtifactTypeID = (int) ArtifactType.Document; // TODO make sure it can be hardcoded
			var idField = new FieldInfo();
			SqlDataReader reader = null;
			try
			{
				reader = this.Context.ExecuteSQLStatementAsReader(string.Format("SELECT * FROM [Field] WHERE FieldArtifactTypeID = {0} AND FieldCategoryID = {1}", ArtifactTypeID, (int) FieldCategory.Identifier));

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
				this.Context.ReleaseConnection();
			}

			return idField;
		}
	}
}