using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.DataGrid;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class LinkedOnlyWriter : IDataGridWriter
	{
		public async Task<IEnumerable<DataGridWriteResult>> Write(IEnumerable<IDataGridRecord> documentsToWrite)
		{
			await Task.Yield();
			return Enumerable.Empty<DataGridWriteResult>();
		}
	}
}