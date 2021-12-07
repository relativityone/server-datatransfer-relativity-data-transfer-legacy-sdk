using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.DataGrid;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class ByteMeasuringWriter : IDataGridWriter
	{
		private readonly ImportMeasurements _measurements;
		private readonly IDataGridWriter _writer;

		public ByteMeasuringWriter(IDataGridWriter writer, ImportMeasurements measurements)
		{
			_writer = writer;
			_measurements = measurements;
		}

		public async Task<IEnumerable<DataGridWriteResult>> Write(IEnumerable<IDataGridRecord> documentsToWrite)
		{
			var results = await _writer.Write(documentsToWrite);
			long bytes = results
				.Sum(recordResult => recordResult.FieldWriteResults
					.Sum(fieldresult => fieldresult.FieldByteSize));

			long dataGridFileSize = _measurements.DataGridFileSize;
			System.Threading.Interlocked.Add(ref dataGridFileSize, bytes);
			_measurements.DataGridFileSize = dataGridFileSize;

			return results;
		}
	}
}