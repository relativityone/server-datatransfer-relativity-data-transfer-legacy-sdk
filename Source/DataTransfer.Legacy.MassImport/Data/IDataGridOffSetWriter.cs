
namespace Relativity.MassImport.Data
{
	internal interface IDataGridOffSetWriter
	{
		void AddOffSetRecord(DataGridOffSetInfo offSetInfo);
		void Flush();
	}
}