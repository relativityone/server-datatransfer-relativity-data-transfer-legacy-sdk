using System.IO;
using System.Threading.Tasks;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	// TODO: change to internal and adjust namespace, https://jira.kcura.com/browse/REL-477112 
	public interface IDataGridRecordBuilder
	{
		Task AddDocument(int artifactID, string type, string batchID);
		Task AddField(DataGridFieldInfo field, Relativity.DataGrid.FieldInfo fieldValue);
		Task AddField(DataGridFieldInfo field, string fieldValue, bool isFileLink);
		Task AddField(DataGridFieldInfo field, Stream fieldValue);
		Task Flush();
	}
}