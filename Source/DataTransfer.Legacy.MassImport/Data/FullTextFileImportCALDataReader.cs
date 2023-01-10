using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Relativity.MassImport.Data
{
	using DataTransfer.Legacy.MassImport.Data;
	using Polly;
	using Relativity.Logging;
	using Relativity.Storage;
	using DataTable = System.Data.DataTable;
	using DateTime = System.DateTime;
	using Type = System.Type;

	internal class FullTextFileImportCALDataReader : DbDataReader
	{
		private readonly IEnumerator<DataRow> _rows;
		private readonly IStorageAccess<string> _storageAccess;
		private const int NumberOfRetries = 5;
		private const int ExponentialWaitTimeBase = 2;
		private readonly Policy _retryPolicy;

		public FullTextFileImportCALDataReader(DataTable filePathResults)
		{
			_rows = filePathResults.AsEnumerable().GetEnumerator();
			_storageAccess = StorageAccessProvider.GetStorageAccess();

			_retryPolicy = Policy
				.Handle<Exception>()
				.WaitAndRetry(
					NumberOfRetries,
					sleepDurationProvider: GetExponentialBackoffSleepDurationProvider(ExponentialWaitTimeBase),
					onRetry: (exception, waitTime, retryNumber, context) =>
					{
						Log.Logger.LogWarning(exception,
							"Error occurred while reading file with Storage Access (CAL). Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
							retryNumber,
							NumberOfRetries,
							waitTime);
					});
		}

		public override bool Read()
		{
			return _rows.MoveNext();
		}

		public override int FieldCount => 2;

		public override object GetValue(int i)
		{
			if (i == 1)
			{
				StorageStream stream = _retryPolicy.Execute(() => _storageAccess.OpenFileAsync(Convert.ToString(_rows.Current[i]), OpenBehavior.OpenExisting, ReadWriteMode.ReadOnly).GetAwaiter().GetResult());
				StreamReader reader = new StreamReader(stream);
				return reader.ReadToEnd();
			}
			else
			{
				return _rows.Current[i];
			}
		}

		private static Func<int, TimeSpan> GetExponentialBackoffSleepDurationProvider(int backoffBase)
		{
			return retryNumber => TimeSpan.FromSeconds(Math.Pow(backoffBase, retryNumber));
		}

		public override bool IsDBNull(int i)
		{
			return false;
		}

		public override TextReader GetTextReader(int i)
		{
			if (i != 1)
			{
				throw new IndexOutOfRangeException(nameof(i));
			}

			StorageStream stream = null;
			try
			{
				stream = _storageAccess.OpenFileAsync(Convert.ToString(_rows.Current[i]), OpenBehavior.OpenExisting, ReadWriteMode.ReadOnly).GetAwaiter().GetResult();
				var reader = new StreamReader(stream, Encoding.UTF8, true, 32 * 1024, false);
				stream = null;
				return reader;
			}
			finally
			{
				stream?.Dispose();
			}
		}

		public override bool NextResult()
		{
			throw new NotImplementedException();
		}

		public override int Depth => throw new NotImplementedException();

		public override bool IsClosed => throw new NotImplementedException();

		public override int RecordsAffected => throw new NotImplementedException();

		public override object this[int ordinal] => throw new NotImplementedException();

		public override object this[string name] => throw new NotImplementedException();

		public override bool HasRows => throw new NotImplementedException();

		public override string GetName(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public override int GetOrdinal(string name)
		{
			throw new NotImplementedException();
		}

		public override bool GetBoolean(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override byte GetByte(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override char GetChar(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override Guid GetGuid(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override short GetInt16(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override int GetInt32(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetInt64(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override string GetDataTypeName(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override Type GetFieldType(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override DateTime GetDateTime(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override string GetString(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override decimal GetDecimal(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override double GetDouble(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override float GetFloat(int ordinal)
		{
			throw new NotImplementedException();
		}
	}
}