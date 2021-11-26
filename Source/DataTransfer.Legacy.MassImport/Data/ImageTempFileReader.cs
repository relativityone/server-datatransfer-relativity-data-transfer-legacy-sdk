using System;

namespace Relativity.MassImport.Data
{
	internal class ImageTempFileReader
	{
		private System.IO.StreamReader _reader;

		public ImageTempFileReader(string tempFilePath)
		{
			_reader = new System.IO.StreamReader(tempFilePath, System.Text.Encoding.Unicode, true);
		}

		public void FinishLine()
		{
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_reader.Read();
			_reader.Read();
			while (_reader.Peek() == 10)
			{
				_reader.Read();
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

		public long ReadStatus()
		{
			return ReadInt64();
		}

		public bool ReadIsNew()
		{
			_reader.Read();
			_reader.Read();
			return false;
		}

		public int ReadOriginalLineNumber()
		{
			return ReadInt32();
		}

		public int ReadArtifactID()
		{
			return ReadInt32();
		}

		public string ReadDocumentIdentifier()
		{
			return ReadString();
		}

		public string ReadFileIdentifier()
		{
			return ReadString();
		}

		public string ReadGuid()
		{
			return ReadString();
		}

		public string ReadFilename()
		{
			return ReadString();
		}

		public int ReadOrder()
		{
			return ReadInt32();
		}

		public int ReadOffset()
		{
			return ReadInt32();
		}

		public int ReadFilesize()
		{
			return ReadInt32();
		}

		public string ReadLocation()
		{
			return ReadString();
		}

		public string ReadOriginalLocation()
		{
			return ReadString();
		}

		public Tuple<bool, System.Data.SqlClient.SqlParameter> ReadFullTextStringBlock()
		{
			var sb = new System.Text.StringBuilder();
			char c;
			var i = default(int);
			do
			{
				i += 1;
				c = (char)_reader.Read();
				switch (c)
				{
					case 'þ':
						{
							string s = c.ToString();
							s += (char)_reader.Read();
							s += (char)_reader.Read();
							s += (char)_reader.Read();
							s += (char)_reader.Read();
							s += (char)_reader.Read();
							if (s.TrimEnd() == "þþKþþ")
							{
								while (_reader.Peek() == 10)
								{
									_reader.Read();
								}
								return Tuple.Create(true, new System.Data.SqlClient.SqlParameter("@textBlock", sb.ToString()));
							}

							break;
						}

					default:
						{
							sb.Append(c);
							if (i > 1000000)
							{
								return Tuple.Create(false, new System.Data.SqlClient.SqlParameter("@textBlock", sb.ToString()));
							}
							break;
						}
				}
			}
			while (true);
		}

		public System.Data.SqlClient.SqlParameter[] GetFixedValues()
		{
			var retval = new System.Data.SqlClient.SqlParameter[13];
			retval[0] = new System.Data.SqlClient.SqlParameter("@Status", ReadStatus());
			retval[1] = new System.Data.SqlClient.SqlParameter("@IsNew", ReadIsNew());
			retval[2] = new System.Data.SqlClient.SqlParameter("@ArtifactID", ReadArtifactID());
			retval[3] = new System.Data.SqlClient.SqlParameter("@OriginalLineNumber", ReadOriginalLineNumber());
			retval[4] = new System.Data.SqlClient.SqlParameter("@DocumentIdentifier", ReadDocumentIdentifier());
			retval[5] = new System.Data.SqlClient.SqlParameter("@FileIdentifier", ReadFileIdentifier());
			retval[6] = new System.Data.SqlClient.SqlParameter("@Guid", ReadGuid());
			retval[7] = new System.Data.SqlClient.SqlParameter("@Filename", ReadFilename());
			retval[8] = new System.Data.SqlClient.SqlParameter("@Order", ReadOrder());
			retval[9] = new System.Data.SqlClient.SqlParameter("@Offset", ReadOffset());
			retval[10] = new System.Data.SqlClient.SqlParameter("@Filesize", ReadFilesize());
			retval[11] = new System.Data.SqlClient.SqlParameter("@Location", ReadLocation());
			retval[12] = new System.Data.SqlClient.SqlParameter("@OriginalFileLocation", ReadOriginalLocation());
			return retval;
		}

		#region Utility
		private int ReadInt32()
		{
			return int.Parse(ReadString());
		}

		private long ReadInt64()
		{
			return long.Parse(ReadString());
		}

		private string ReadString()
		{
			var retval = new System.Text.StringBuilder();
			char c = (char)_reader.Read();
			while (c != ',')
			{
				retval.Append(c);
				c = (char)_reader.Read();
			}

			return retval.ToString();
		}
		#endregion
	}
}