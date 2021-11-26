using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.Utility.Streaming;

namespace Relativity.MassImport.Data
{
	internal class DataGridServiceStorageReader : System.Data.Common.DbDataReader
	{
		private readonly List<InternalPosition> _fieldPositions;
		private DataGridTempFileDataReader _reader;
		private readonly byte[] _fieldDelimiter;
		private readonly byte[] _rowDelimiter;

		private class InternalPosition
		{
			public long startPosition { get; set; }
			public long length { get; set; }
		}

		public DataGridServiceStorageReader(DataGridTempFileDataReader reader, string fieldDelimiter, string lineDelimiter)
		{
			_reader = reader;
			_fieldPositions = new List<InternalPosition>();
			_fieldDelimiter = System.Text.Encoding.Unicode.GetBytes(fieldDelimiter);
			_rowDelimiter = System.Text.Encoding.Unicode.GetBytes(lineDelimiter);
		}

		public long GetFieldLength(int i)
		{
			DelimitedReadStreamDecorator delimitedReader = (DelimitedReadStreamDecorator)_reader._delimitedStream.Value;
			long initialPosition = delimitedReader.Position;

			long startingFieldPosition = 0L;
			int delimiterIdx = 0;
			while (delimiterIdx < _fieldPositions.Count & delimiterIdx < i)
			{
				startingFieldPosition += _fieldPositions[delimiterIdx].length;
				delimiterIdx += 1;
			}

			long currentFieldLength = 0L;
			while (delimitedReader.Position < delimitedReader.Length)
			{
				delimitedReader.ReadByte();
				currentFieldLength += 1L;
				while (delimitedReader.HasReachedDelimiter)
				{
					delimiterIdx = delimiterIdx + 1;
					long delimiterPos = delimitedReader.Position;
					long offset = delimiterPos - initialPosition;
					currentFieldLength = 0L;
				}
			}

			var interalPostion = _fieldPositions[i];

			delimitedReader.Position = initialPosition;
			return interalPostion.length;
		}

		public long StartPosition => _reader.StartPosition;

		public long Length => _reader.Length;

		public long MaximumDataGridFieldSize
		{
			get => _reader.MaximumDataGridFieldSize;
			set => _reader.MaximumDataGridFieldSize = value;
		}

		public override bool IsDBNull(int i)
		{
			return false;
		}

		public override int FieldCount => _reader.FieldCount;

		public override bool HasRows => _reader.HasRows;

		public override object this[int i] => _reader[i];

		public override object this[string name] => _reader[name];

		public override string GetName(int i)
		{
			return _reader.GetName(i);
		}

		public override string GetString(int i)
		{
			return _reader.GetString(i);
		}

		public long GetStartPosition(int i)
		{
			return _reader.GetStartPosition(i);
		}

		public override object GetValue(int i)
		{
			return _reader.GetValue(i);
		}

		public override int GetOrdinal(string name)
		{
			return _reader.GetOrdinal(name);
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public override Stream GetStream(int ordinal)
		{
			return _reader.GetStream(ordinal);
		}

		public override TextReader GetTextReader(int ordinal)
		{
			return _reader.GetTextReader(ordinal);
		}

		public override void Close()
		{
			_reader.Close();
		}

		public override bool NextResult()
		{
			return _reader.NextResult();
		}

		public override bool Read()
		{
			return _reader.Read();
		}

		public override bool IsClosed => _reader.IsClosed;

		#region Not Implemented
		public override int RecordsAffected => throw new NotImplementedException();

		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public override string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override Type GetFieldType(int i)
		{
			return _reader.GetFieldType(i);
		}

		public override int GetValues(object[] values)
		{
			return _reader.GetValues(values);
		}

		public override bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		public override byte GetByte(int i)
		{
			throw new NotImplementedException();
		}

		public override char GetChar(int i)
		{
			throw new NotImplementedException();
		}

		public override Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		public override short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		public override int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		public override long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		public override float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		public override double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		public override decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		public override DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		public override DataTable GetSchemaTable()
		{
			return _reader.GetSchemaTable();
		}

		public override int Depth => _reader.Depth;

		#endregion
	}
}