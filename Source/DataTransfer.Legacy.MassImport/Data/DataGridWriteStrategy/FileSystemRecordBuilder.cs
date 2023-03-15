using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using kCura.Utility.Streaming;
using Relativity.DataGrid;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class FileSystemRecordBuilder : IDataGridRecordBuilder
	{
		private string _type;
		private int _artifactID;
		private string _batchID;
		private int _numberOfTexts;
		private int _numberOfEmptyTexts;
		private readonly IDataGridWriter _writer;
		private readonly byte[] _shortcutBuffer;
		private readonly ActionBlock<StorageRequest> _storageQueue;

		public int NumberOfEmptyTexts => _numberOfEmptyTexts;
		public int NumberOfTexts => _numberOfTexts;

		private class StorageRequest
		{
			public DataGridRecord Record { get; set; }
			public DataGridFieldInfo Field { get; set; }
			public TaskCompletionSource<bool> Completion { get; set; }
		}

		private class IndexingRequest
		{
			public string DataGridID { get; set; }
			public string Type { get; set; }
			public DataGridFieldInfo Field { get; set; }
			public FieldInfo FieldValue { get; set; }
		}

		public FileSystemRecordBuilder(IDataGridWriter writer, int shortFieldLengthLimit, int writeParallelism)
		{
			_writer = writer;
			var shortcutBuffer = new byte[shortFieldLengthLimit];
			_shortcutBuffer = shortcutBuffer;

			var options = new ExecutionDataflowBlockOptions()
			{
				BoundedCapacity = 100,
				MaxDegreeOfParallelism = writeParallelism == -1 ? 8 : writeParallelism
			};
			_storageQueue = new ActionBlock<StorageRequest>(StoreRecord, options);
		}

		public Task AddDocument(int artifactID, string type, string batchID)
		{
			_artifactID = artifactID;
			_type = type;
			_batchID = batchID;
			return Task.CompletedTask;
		}

		public async Task AddField(DataGridFieldInfo field, string fieldValue, bool isFileLink)
		{
			Interlocked.Increment(ref _numberOfTexts);

			bool validLink = isFileLink && !string.IsNullOrEmpty(fieldValue);
			long byteSize = 0L;
			if (!validLink && fieldValue != null)
			{
				byteSize = System.Text.Encoding.Unicode.GetByteCount(fieldValue);
			}

			if (byteSize == 0)
			{
				Interlocked.Increment(ref _numberOfEmptyTexts);
			}

			var fieldInfo = new Relativity.DataGrid.FieldInfo()
			{
				Value = fieldValue,
				IsValueAFileLink = validLink,
				ByteSize = byteSize
			};
			await AddField(field, fieldInfo);
		}

		public async Task AddField(DataGridFieldInfo field, Stream fieldValue)
		{
			int bytesRead = fieldValue.Read(_shortcutBuffer, 0, _shortcutBuffer.Length);
			int totalBytesRead = bytesRead;
			while (bytesRead > 0 && totalBytesRead < _shortcutBuffer.Length)
			{
				bytesRead = fieldValue.Read(_shortcutBuffer, totalBytesRead, _shortcutBuffer.Length - totalBytesRead);
				totalBytesRead += bytesRead;
			}

			if (totalBytesRead < _shortcutBuffer.Length || bytesRead == 0)
			{
				// if the field value is less than our threshold, write it as a string so it can be written in parallel
				string valueString = System.Text.Encoding.Unicode.GetString(_shortcutBuffer, 0, totalBytesRead);
				await AddField(field, valueString, false);
			}
			else
			{
				Interlocked.Increment(ref _numberOfTexts);
				var wellWeTriedStream = new PrependingReadStreamDecorator(fieldValue, _shortcutBuffer);
				await WriteLargeTextStream(field, wellWeTriedStream);
			}
		}

		public async Task Flush()
		{
			_storageQueue.Complete();
			_type = null;
			_artifactID = default;
			await _storageQueue.Completion;
		}

		private async Task AddField(DataGridFieldInfo field, Relativity.DataGrid.FieldInfo fieldValue)
		{
			await AddRecordRequest(field, fieldValue, null);
		}

		private async Task AddRecordRequest(DataGridFieldInfo field, Relativity.DataGrid.FieldInfo fieldValue, TaskCompletionSource<bool> completion)
		{
			var dgRecord = new DataGridRecord()
			{
				ArtifactID = _artifactID,
				Type = _type,
				BatchID = _batchID
			};
			dgRecord.AddField(field.FieldNamespace, field.FieldName, fieldValue);
			var request = new StorageRequest()
			{
				Field = field,
				Record = dgRecord,
				Completion = completion
			};
			await _storageQueue.SendAsync(request);
		}

		private async Task StoreRecord(StorageRequest request)
		{
			var results = await _writer.Write(new[] { request.Record });
			request.Completion?.SetResult(true);
		}

		private async Task WriteLargeTextStream(DataGridFieldInfo field, Stream fieldValue)
		{
			var completionEvent = new TaskCompletionSource<bool>();
			var bomPrependingStream = new PrependingReadStreamDecorator(fieldValue, System.Text.Encoding.Unicode.GetPreamble());

			var fieldInfo = new Relativity.DataGrid.FieldInfo()
			{
				Value = bomPrependingStream
			};

			await AddRecordRequest(field, fieldInfo, completionEvent);
			await completionEvent.Task; // only completed after the record is written, because the reader can only have one field stream open at a time
		}
	}
}