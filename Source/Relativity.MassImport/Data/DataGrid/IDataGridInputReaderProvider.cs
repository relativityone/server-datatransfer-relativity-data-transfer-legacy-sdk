
namespace Relativity.MassImport.Data.DataGrid
{
	internal interface IDataGridInputReaderProvider
	{
		bool IsDataGridInputValid();
		DataGridReader CreateDataGridInputReader(string bulkFileShareFolderPath, Logging.ILog correlationLogger);
		void CleanupDataGridInput(string bulkFileShareFolderPath, Logging.ILog correlationLogger);
	}
}