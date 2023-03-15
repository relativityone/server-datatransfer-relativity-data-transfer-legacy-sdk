using System.IO;
using System.Threading.Tasks;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal interface IDataGridRecordBuilder
	{
		Task AddDocument(int artifactID, string type, string batchID);
		Task AddField(DataGridFieldInfo field, string fieldValue, bool isFileLink);
		Task AddField(DataGridFieldInfo field, Stream fieldValue);
		Task Flush();
	}
}