using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Data.StagingTables
{
	internal class ObjectsStagingTableRepository : BaseStagingTableRepository, IStagingTableRepository
	{
		public ObjectsStagingTableRepository(BaseContext context, TableNames tableNames, ImportMeasurements importMeasurements) 
			: base(context, tableNames, importMeasurements)
		{
		}

		public override string Insert(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool excludeFolderPathForOldClient)
		{
			string importNativeFileName = System.IO.Path.Combine(settings.Repository, settings.DataFileName);
			string importCodeFileName = System.IO.Path.Combine(settings.Repository, settings.CodeFileName);

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
							sql.Append("," + Environment.NewLine);
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
						parameters.Add(new SqlParameter(cell.ParameterName, cell.Value));
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
						using (var textReader = new System.IO.StreamReader(importNativeFileName, System.Text.Encoding.Unicode, true))
						{
							textReader.BaseStream.Seek(cellParam.Start, System.IO.SeekOrigin.Begin);
							int length = 0;
							bool isFirstTimeThrough = true;
							var sb = new System.Text.StringBuilder();
							while (length < cellParam.Length)
							{
								sb.Append((char)textReader.Read());
								if (sb.Length > 999999)
								{
									if (isFirstTimeThrough)
									{
										this.Context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [{1}] = @textBlock WHERE [kCura_Import_OriginalLineNumber] = {2}", this.TableNames.Native, cell.ColumnName, (object)originalLineNumber), new SqlParameter[] { new SqlParameter("@textBlock", sb.ToString()) }, this.QueryTimeout);
										isFirstTimeThrough = false;
									}
									else
									{
										this.Context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [{1}] .WRITE(@textBlock, NULL, 0) WHERE [kCura_Import_OriginalLineNumber] = {2}", this.TableNames.Native, cell.ColumnName, (object)originalLineNumber), new SqlParameter[] { new SqlParameter("@textBlock", sb.ToString()) }, this.QueryTimeout);
									}

									sb = new System.Text.StringBuilder();
								}

								length += 1;
							}

							if (sb.Length > 0)
							{
								this.Context.ExecuteNonQuerySQLStatement(string.Format("UPDATE [Resource].[{0}] SET [{1}] .WRITE(@textBlock, NULL, 0) WHERE [kCura_Import_OriginalLineNumber] = {2}", this.TableNames.Native, cell.ColumnName, (object)originalLineNumber), new SqlParameter[] { new SqlParameter("@textBlock", sb.ToString()) }, this.QueryTimeout);
							}
						}
					}
				}
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
				this.Context.ExecuteNonQuerySQLStatement(string.Format("INSERT INTO [Resource].[{0}] ([DocumentIdentifier], [CodeArtifactID], [CodeTypeID]) VALUES (@documentIdentifier, @codeArtifactID, @codeTypeID)", this.TableNames.Code), parameters, this.QueryTimeout);
			}

			sr.Close();
			kCura.Utility.File.Instance.Delete(importNativeFileName);
			kCura.Utility.File.Instance.Delete(importCodeFileName);
			return this.TableNames.RunId;
		}

		public override void CreateStagingTables(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool includeExtractedTextEncoding, bool excludeFolderPathForOldClient)
		{
			var metadataColumnClause = new System.Text.StringBuilder();
			foreach (FieldInfo mappedField in settings.MappedFields)
			{
				if (mappedField.Type == FieldTypeHelper.FieldType.File)
				{
					metadataColumnClause.AppendFormat("		[{0}] {1}," + Environment.NewLine, mappedField.GetColumnName() + "_ImportObject_FileName", "NVARCHAR(MAX)");
					metadataColumnClause.AppendFormat("		[{0}] {1}," + Environment.NewLine, mappedField.GetColumnName() + "_ImportObject_FileSize", "BIGINT");
					metadataColumnClause.AppendFormat("		[{0}] {1}," + Environment.NewLine, mappedField.GetColumnName() + "_ImportObject_FileLocation", "NVARCHAR(MAX)");
				}
				else
				{
					metadataColumnClause.AppendFormat("		[{0}] {1}," + Environment.NewLine, mappedField.GetColumnName(), this.GetColumnDefinition(columnDefinitionCache, mappedField));
				}
			}

			this.CreateStagingTablesBase(columnDefinitionCache, settings.MappedFields, metadataColumnClause.ToString(), settings.KeyFieldArtifactID, settings.LoadImportedFullTextFromServer);
		}
	}
}