using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.Utility.Streaming;
using Relativity.Logging;

namespace Relativity.MassImport.Data
{
	internal class DataGridTempFileDataReader : System.Data.Common.DbDataReader
	{
		private readonly DataGridReaderOptions _options;
		private readonly byte[] _fieldDelimiter;
		private readonly byte[] _rowDelimiter;
		private int _currentOrdinal;
		private readonly ILog _correlationLogger;
		private readonly int _massImportOnFileLockRetryWaitTimeInMilliseconds;
		private readonly int _massImportOnFileLockRetryCount;
		private long? _sizeThreshold;
		private readonly Lazy<Stream> _fileStream;
		private long _fileSize;
		public Lazy<Stream> _delimitedStream;
		private Lazy<Stream> _byteLimitedStream;
		private const int _BUFFER_SIZE = 40960; // 40KB
		private readonly System.Text.Encoding _encoding;
		private long _startPosition = 0;
		private long _length = 0;

		public DataGridTempFileDataReader(
			DataGridReaderOptions options, 
			string fieldDelimiter, 
			string lineDelimiter, 
			string dataGridFilePath, 
			ILog correlationLogger) : this(
				options, 
				fieldDelimiter, 
				lineDelimiter, 
				dataGridFilePath, 
				correlationLogger, 
				Relativity.Data.Config.MassImportOnFileLockRetryWaitTimeInMilliseconds, 
				Relativity.Data.Config.MassImportOnFileLockRetryCount)
		{
		}

		public DataGridTempFileDataReader(
			DataGridReaderOptions options, 
			string fieldDelimiter, 
			string lineDelimiter, 
			string dataGridFilePath, 
			ILog correlationLogger, 
			int massImportOnFileLockRetryCount, 
			int massImportOnFileLockRetryWaitTimeInMilliseconds)
		{
			_encoding = System.Text.Encoding.Unicode;
			_fieldDelimiter = _encoding.GetBytes(fieldDelimiter);
			_rowDelimiter = _encoding.GetBytes(lineDelimiter);
			_correlationLogger = correlationLogger;
			_massImportOnFileLockRetryWaitTimeInMilliseconds = massImportOnFileLockRetryWaitTimeInMilliseconds;
			_massImportOnFileLockRetryCount = massImportOnFileLockRetryCount;

			_options = options;
			_currentOrdinal = 0;
			_fileStream = new Lazy<Stream>(() => OpenDataGridFile(dataGridFilePath));
			SetDelimitedStream();
		}

		public long StartPosition => _startPosition;

		public long Length => _length;

		private List<string> FieldOrder
		{
			get
			{
				return new[] { _options.IdentifierColumnName, _options.DataGridIDColumnName }.Concat(_options.MappedDataGridFields.Select(f => f.GetColumnName())).ToList();
			}
		}

		private void SetDelimitedStream()
		{
			_delimitedStream?.Value.Dispose();
			_delimitedStream = new Lazy<Stream>(() => new DelimitedReadStreamDecorator(FileStream, Delimiter));
			SetByteLimitedStream();
		}

		private void SetByteLimitedStream()
		{
			_byteLimitedStream?.Value.Dispose();
			_byteLimitedStream = new Lazy<Stream>(() => new ByteLimitedReadStream(DelimitedStream, MaximumDataGridFieldSize, _correlationLogger));
		}

		public Stream FileStream => _fileStream.Value;

		private Stream DelimitedStream => _delimitedStream.Value;

		private Stream ByteLimitedStream
		{
			get
			{
				if (!_byteLimitedStream?.Value.CanRead == true)
				{
					SetByteLimitedStream();
				}

				return _byteLimitedStream.Value;
			}
		}

		private byte[] Delimiter => _currentOrdinal < FieldOrder.Count - 1 ? _fieldDelimiter : _rowDelimiter;

		public long MaximumDataGridFieldSize
		{
			get
			{
				if (!_sizeThreshold.HasValue)
				{
					_sizeThreshold = long.MaxValue;
				}

				return _sizeThreshold.Value;
			}

			set => _sizeThreshold = value;
		}

		private bool CanRead => FileStream.Position < _fileSize;

		private long SeekToOrdinal(int i)
		{
			if (i < _currentOrdinal && i != 0 && _currentOrdinal != FieldCount - 1)
			{
				throw new InvalidOperationException($"Invalid attempt to read column ordinal '{i}' after reading ordinal '{_currentOrdinal}'. Only sequential access is permitted.");
			}
			else if (i > FieldCount - 1)
			{
				throw new IndexOutOfRangeException($"Invalid column ordinal '{i}' is out of bounds.");
			}

			long totalBytesRead = 0L;
			while (_currentOrdinal != i && CanRead)
			{
				if (DelimitedStream.ReadByte() == -1) // seek to end of field's stream
				{
					_currentOrdinal = (_currentOrdinal + 1) % FieldCount;
					SetDelimitedStream();
				}

				totalBytesRead += 1L;
			}

			return totalBytesRead;
		}

		private void SeekToFieldOffset(long i)
		{
			if (i < ByteLimitedStream.Position)
			{
				throw new InvalidOperationException($"Invalue attempt to read field offset '{i}' after reading offset '{ByteLimitedStream.Position}'. Only sequential access is permitted.");
			}

			if (ByteLimitedStream.Position == i)
			{
				return;
			}

			while (ByteLimitedStream.Position < i)
			{
				if (ByteLimitedStream.ReadByte() == -1) // reset if the end of the field stream is reached
				{
					if (!DelimitedStream.CanRead)
					{
						_currentOrdinal = (_currentOrdinal + 1) % FieldCount;
						SetDelimitedStream();
					}
					else
					{
						SetByteLimitedStream();
					}

					break;
				}
			}
		}

		private FileStream OpenDataGridFile(string fileName)
		{
			Func<FileStream> openFileStream = () =>
			{
				_fileSize = kCura.Utility.File.Instance.GetFileSize(fileName);
				int maxBufferSize = _fileSize < _BUFFER_SIZE ? (int)_fileSize : _BUFFER_SIZE;
				var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, maxBufferSize);
				foreach (byte bomByte in _encoding.GetPreamble())
				{
					if (bomByte != fs.ReadByte()) // advance through the file's BOM before starting to read import data
					{
						throw new Exception($"Expected the Data Grid bulk file to begin with a {_encoding.EncodingName} preamble byte sequence but it was not found.");
					}
				}

				return fs;
			};
			return kCura.Utility.RetryHelper.ExecuteFunctionWithRetry(openFileStream, _massImportOnFileLockRetryWaitTimeInMilliseconds, _massImportOnFileLockRetryCount, ExceptionIsFileLock);
		}

		private bool ExceptionIsFileLock(Exception ex)
		{
			_correlationLogger.LogWarning(ex, "ReadDataGridDocumentsFromBulkFile Retrying Error after {waitTime} milliseconds", (object)_massImportOnFileLockRetryWaitTimeInMilliseconds);
			return ex is IOException && ex.Message.Contains("because it is being used by another process");
		}

		public override bool IsDBNull(int i)
		{
			return false;
		}

		public override int FieldCount => FieldOrder.Count;

		public override bool HasRows => FileStream != null && _fileSize > _encoding.GetPreamble().Length;

		public override object this[int i] => GetValue(i);

		public override object this[string name] => GetValue(GetOrdinal(name));

		public override string GetName(int i)
		{
			return FieldOrder[i];
		}

		public override string GetString(int i)
		{
			SeekToOrdinal(i);
			using (var reader = new StreamReader(ByteLimitedStream, _encoding))
			{
				string stringVal = reader.ReadToEnd();
				return stringVal;
			}
		}

		public long GetStartPosition(int i)
		{
			return SeekToOrdinal(i);
		}

		public override object GetValue(int i)
		{
			return GetString(i);
		}

		public override int GetOrdinal(string name)
		{
			return FieldOrder.IndexOf(name);
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			SeekToOrdinal(i);
			SeekToFieldOffset(fieldOffset);
			return ByteLimitedStream.Read(buffer, bufferoffset, length);
		}

		public override Stream GetStream(int ordinal)
		{
			SeekToOrdinal(ordinal);
			return ByteLimitedStream;
		}

		public override TextReader GetTextReader(int ordinal)
		{
			var byteStream = GetStream(ordinal);
			return new StreamReader(byteStream, _encoding);
		}

		#region IDisposable Support
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_fileStream?.Value.Dispose();
			}
		}
		#endregion

		public override void Close()
		{
			_fileStream?.Value.Close();
		}

		public override bool NextResult()
		{
			return Read();
		}

		public override bool Read()
		{
			if (_currentOrdinal != 0 || DelimitedStream.Position != 0L)
			{
				SeekToOrdinal(FieldCount - 1);
			}

			return CanRead;
		}

		public override bool IsClosed => !CanRead;

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
			return typeof(string);
		}

		public override int GetValues(object[] values)
		{
			var valueArray = FieldOrder.Select(fieldName => this[fieldName]).ToArray();
			Array.Copy(valueArray, values, valueArray.Length);
			return valueArray.Length;
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
			return null;
		}

		public override int Depth => throw new NotImplementedException();
		#endregion

		private class ByteLimitedReadStream : ByteLimitingReadStreamDecorator
		{
			private readonly long _maxByteSize;
			private readonly ILog _correlationLogger;

			public ByteLimitedReadStream(Stream stream, long maxByteSize, ILog correlationLogger) : base(stream, maxByteSize)
			{
				_maxByteSize = maxByteSize;
				_correlationLogger = correlationLogger;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				int bytesRead = base.Read(buffer, offset, count);
				if (Position >= _maxByteSize)
				{
					_correlationLogger.LogDebug("Field too large (over {bytes} bytes).", (object)_maxByteSize);
				}

				return bytesRead;
			}
		}
	}
}