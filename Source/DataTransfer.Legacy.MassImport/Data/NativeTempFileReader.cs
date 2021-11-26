using System;
using System.Globalization;

namespace Relativity.MassImport.Data
{
	internal class NativeTempFileReader
	{
		private System.IO.StreamReader _reader;
		private string _path;
		private long _offset = 0;
		private bool _isPaused = false;
		public NativeTempFileReader(string tempFilePath, FieldInfo[] mappedFields)
		{
			_path = tempFilePath;
			_reader = new System.IO.StreamReader(tempFilePath, System.Text.Encoding.Unicode, true);
		}

		public void Pause()
		{
			if (!_isPaused)
			{
				_offset = _reader.BaseStream.Position;
				_isPaused = true;
				try
				{
					_reader.Close();
				}
				catch
				{
				}
			}
		}

		public void Play()
		{
			if (_isPaused)
			{
				_reader = new System.IO.StreamReader(_path, System.Text.Encoding.Unicode, true);
				_reader.BaseStream.Seek(_offset, System.IO.SeekOrigin.Begin);
				_isPaused = false;
			}
		}

		private object ReadToCellDelimiter()
		{
			System.Text.StringBuilder retval = new System.Text.StringBuilder();
			Int64 startpos = _offset + 2;
			while (_reader.Peek() != -1)
			{
				if (_reader.Peek() == 254)
				{
					string possibleCellTerm = this.ReadPossibleCellTerm();
					if (possibleCellTerm == "")
					{
						return retval.ToString();
					}
					else
					{
						retval.Append(possibleCellTerm);
					}
				}
				else
				{
					retval.Append((char)_reader.Read());
					_offset += 2;
				}
				if (retval.Length > 1999999)
				{
					long currentLength = retval.Length;
					retval = null;
					return ReadCellLongValue(startpos, currentLength);
				}
			}
			return retval.ToString();
		}

		private StreamMarker ReadCellLongValue(Int64 start, Int64 currentLength)
		{
			while (_reader.Peek() != -1)
			{
				if (_reader.Peek() == 254)
				{
					string possibleCellTerm = this.ReadPossibleCellTerm();
					if (possibleCellTerm == "")
					{
						return new StreamMarker(start, currentLength);
					}
					else
					{
						currentLength += possibleCellTerm.Length;
						_reader.Read();
						_offset += 2;
					}
				}
				else
				{
					currentLength += 1;
					_reader.Read();
					_offset += 2;
				}
			}
			return null;
		}

		private string ReadPossibleCellTerm()
		{
			_offset += 10;
			Int32[] chars = new Int32[5];
			chars[0] = _reader.Read();
			chars[1] = _reader.Read();
			chars[2] = _reader.Read();
			chars[3] = _reader.Read();
			chars[4] = _reader.Read();
			if (chars[0] == 254 && chars[1] == 254 && chars[2] == (int)'K' && chars[3] == 254 && chars[4] == 254)
			{
				return "";
			}
			else
			{
				string retstring = "";
				retstring += (char)0;
				retstring += (char)1;
				retstring += (char)2;
				retstring += (char)3;
				retstring += (char)4;
				return retstring;
			}
		}

		public void FinishLine()
		{
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_offset += 12;
			while (_reader.Peek() == 10)
			{
				_reader.Read();
				_offset += 2;
			}
		}

		public bool Eof
		{
			get
			{
				try
				{
					return _reader.Peek() == -1;
				}
				catch
				{
					return false;
				}
			}
		}

		// CREATE TABLE [{0}](
		// [kCura_Import_Status] INT NOT NULL,
		// [kCura_Import_IsNew] BIT NOT NULL,
		// [ArtifactID] INT NOT NULL,
		// [kCura_Import_OriginalLineNumber] INT NOT NULL,
		// [kCura_Import_FileGuid] NVARCHAR(100) NOT NULL,
		// [kCura_Import_Filename] NVARCHAR(200) NOT NULL,
		// [kCura_Import_Location] NVARCHAR(2000),
		// [kCura_Import_OriginalFileLocation] NVARCHAR(2000),
		// [kCura_Import_ParentFolderID] INT NOT NULL{1}
		// )

		public object[] ReadCodeLine
		{
			get
			{
				object[] retval = new object[3];
				retval[0] = this.ReadToCellDelimiter().ToString();
				retval[1] = Int32.Parse(this.ReadToCellDelimiter().ToString());
				retval[2] = Int32.Parse(this.ReadToCellDelimiter().ToString());
				_reader.Read();
				_reader.Read();
				_offset += 4;
				return retval;
			}
		}
		public Cell[] ReadLine(FieldInfo[] mappedFields)
		{
			System.Collections.ArrayList retval = new System.Collections.ArrayList();
			retval.Add(new Cell("kCura_Import_Status", "@kCura_Import_Status", System.Convert.ToInt32(this.ReadToCellDelimiter().ToString())));
			retval.Add(new Cell("kCura_Import_IsNew", "@kCura_Import_IsNew", System.Convert.ToInt32(this.ReadToCellDelimiter().ToString()) > 0));
			retval.Add(new Cell("ArtifactID", "@artifactID", System.Convert.ToInt32(this.ReadToCellDelimiter().ToString())));
			retval.Add(new Cell("kCura_Import_OriginalLineNumber", "@kCura_Import_OriginalLineNumber", System.Convert.ToInt32(this.ReadToCellDelimiter().ToString())));
			retval.Add(new Cell("kCura_Import_FileGuid", "@kCura_Import_FileGuid", this.ReadToCellDelimiter().ToString()));
			retval.Add(new Cell("kCura_Import_Filename", "@kCura_Import_Filename", this.ReadToCellDelimiter().ToString()));
			retval.Add(new Cell("kCura_Import_Location", "@kCura_Import_Location", this.ReadToCellDelimiter().ToString()));
			retval.Add(new Cell("kCura_Import_OriginalFileLocation", "@kCura_Import_OriginalFileLocation", this.ReadToCellDelimiter().ToString()));
			retval.Add(new Cell("kCura_Import_FileSize", "@kCura_Import_FileSize", System.Convert.ToInt32(this.ReadToCellDelimiter().ToString())));
			retval.Add(new Cell("kCura_Import_ParentFolderID", "@kCura_Import_ParentFolderID", System.Convert.ToInt32(this.ReadToCellDelimiter().ToString())));
			foreach (FieldInfo fieldInfo in mappedFields)
			{
				switch (fieldInfo.Type)
				{
					case FieldTypeHelper.FieldType.Boolean:
						{
							string boolString = this.ReadToCellDelimiter().ToString().Trim();
							if (boolString == "1")
							{
								boolString = "True";
							}

							if (boolString == "0")
							{
								boolString = "False";
							}

							if (boolString != "")
							{
								retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), System.Convert.ToBoolean(boolString)));
							}
							break;
						}

					case FieldTypeHelper.FieldType.Code:
					case FieldTypeHelper.FieldType.MultiCode:
						{
							retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), this.ReadToCellDelimiter().ToString()));
							break;
						}

					case FieldTypeHelper.FieldType.Date:
						{
							string dateString = this.ReadToCellDelimiter().ToString();
							if (dateString.Trim() != "")
							{
								retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), kCura.Utility.DateTime.ParseSqlCultureNeutralString(dateString)));
							}
							break;
						}

					case FieldTypeHelper.FieldType.Currency:
					case FieldTypeHelper.FieldType.Decimal:
						{
							string decString = this.ReadToCellDelimiter().ToString().Trim();
							if (decString != "")
							{
								retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), decimal.Parse(decString, CultureInfo.InvariantCulture)));
							}
							break;
						}

					case FieldTypeHelper.FieldType.Integer:
					case FieldTypeHelper.FieldType.User:
						{
							string intstring = this.ReadToCellDelimiter().ToString();
							if (intstring != "")
							{
								retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), Int32.Parse(intstring)));
							}
							break;
						}

					case FieldTypeHelper.FieldType.Varchar:
					case FieldTypeHelper.FieldType.Text:
						{
							retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), this.ReadToCellDelimiter()));
							break;
						}

					case FieldTypeHelper.FieldType.File:
						{
							retval.Add(new Cell(fieldInfo.GetColumnName() + "_ImportObject_FileName", "@" + fieldInfo.GetColumnName() + "_ImportObject_FileName", this.ReadToCellDelimiter()));
							retval.Add(new Cell(fieldInfo.GetColumnName() + "_ImportObject_FileSize", "@" + fieldInfo.GetColumnName() + "_ImportObject_FileSize", this.ReadToCellDelimiter()));
							retval.Add(new Cell(fieldInfo.GetColumnName() + "_ImportObject_FileLocation", "@" + fieldInfo.GetColumnName() + "_ImportObject_FileLocation", this.ReadToCellDelimiter()));
							break;
						}

					case FieldTypeHelper.FieldType.Object:
						{
							object cellContent = this.ReadToCellDelimiter();
							if (cellContent is string && cellContent.ToString().Trim() == "")
								cellContent = System.DBNull.Value;
							retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), cellContent));
							break;
						}

					default:
						{
							retval.Add(new Cell(fieldInfo.GetColumnName(), "@" + fieldInfo.GetColumnName(), this.ReadToCellDelimiter()));
							break;
						}
				}
			}
			_reader.Read();
			_reader.Read();
			_offset += 2;
			return (Cell[])retval.ToArray(typeof(Cell));
		}

		public void Close()
		{
			try
			{
				_reader.Close();
			}
			catch
			{
			}
		}

		public int ReadStatus()
		{
			return this.ReadInt32();
		}

		public bool ReadIsNew()
		{
			_reader.Read();
			_reader.Read();
			_offset += 2;
			return false;
		}

		public int ReadOriginalLineNumber()
		{
			return this.ReadInt32();
		}

		public int ReadArtifactID()
		{
			return this.ReadInt32();
		}

		public string ReadDocumentIdentifier()
		{
			return this.ReadString();
		}

		public string ReadFileIdentifier()
		{
			return this.ReadString();
		}

		public string ReadGuid()
		{
			return this.ReadString();
		}

		public string ReadFilename()
		{
			return this.ReadString();
		}

		public int ReadOrder()
		{
			return this.ReadInt32();
		}

		public int ReadOffset()
		{
			return this.ReadInt32();
		}

		public int ReadFilesize()
		{
			return this.ReadInt32();
		}

		public string ReadLocation()
		{
			return this.ReadString();
		}

		public string ReadOriginalLocation()
		{
			return this.ReadString();
		}

		public System.Data.SqlClient.SqlParameter[] GetFixedValues()
		{
			System.Data.SqlClient.SqlParameter[] retval = new System.Data.SqlClient.SqlParameter[13];
			retval[0] = new System.Data.SqlClient.SqlParameter("@Status", this.ReadStatus());
			retval[1] = new System.Data.SqlClient.SqlParameter("@IsNew", this.ReadIsNew());
			retval[2] = new System.Data.SqlClient.SqlParameter("@ArtifactID", this.ReadArtifactID());
			retval[3] = new System.Data.SqlClient.SqlParameter("@OriginalLineNumber", this.ReadOriginalLineNumber());
			retval[4] = new System.Data.SqlClient.SqlParameter("@DocumentIdentifier", this.ReadDocumentIdentifier());
			retval[5] = new System.Data.SqlClient.SqlParameter("@FileIdentifier", this.ReadFileIdentifier());
			retval[6] = new System.Data.SqlClient.SqlParameter("@Guid", this.ReadGuid());
			retval[7] = new System.Data.SqlClient.SqlParameter("@Filename", this.ReadFilename());
			retval[8] = new System.Data.SqlClient.SqlParameter("@Order", this.ReadOrder());
			retval[9] = new System.Data.SqlClient.SqlParameter("@Offset", this.ReadOffset());
			retval[10] = new System.Data.SqlClient.SqlParameter("@Filesize", this.ReadFilesize());
			retval[11] = new System.Data.SqlClient.SqlParameter("@Location", this.ReadLocation());
			retval[12] = new System.Data.SqlClient.SqlParameter("@OriginalFileLocation", this.ReadOriginalLocation());
			return retval;
		}


		private int ReadInt32()
		{
			return int.Parse(this.ReadString());
		}

		private string ReadString()
		{
			System.Text.StringBuilder retval = new System.Text.StringBuilder();
			char c = (char)_reader.Read();
			_offset += 2;
			while (c != ',')
			{
				retval.Append(c);
				c = (char)_reader.Read();
				_offset += 2;
			}
			return retval.ToString();
		}


		public class Cell
		{
			private StreamMarker _streamMarker;

			public Cell(string columnName, string paramName, object value)
			{
				ColumnName = columnName;
				ParameterName = paramName;
				if (value is StreamMarker)
				{
					_streamMarker = (StreamMarker)value;
				}
				Value = value;
			}

			public string ColumnName { get; }

			public string ParameterName { get; }

			public object Value { get; }

			public bool IsLargeValue => _streamMarker != null;
		}

		public class StreamMarker
		{
			public long Start { get; }

			public long Length { get; }

			public StreamMarker(Int64 start, Int64 length)
			{
				Start = start;
				Length = length;
			}
		}
	}
}