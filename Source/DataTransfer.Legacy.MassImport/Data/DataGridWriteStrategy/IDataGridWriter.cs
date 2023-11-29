using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.DataGrid;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal interface IDataGridWriter
	{
		Task<IEnumerable<DataGridWriteResult>> Write(IEnumerable<IDataGridRecord> documentsToWrite);
	}
}