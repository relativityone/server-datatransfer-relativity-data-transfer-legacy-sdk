using Relativity.Data.MassImport;
using Relativity.Logging;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.DataGrid;

internal class DataGridInputReaderProvider : IDataGridInputReaderProvider
{
	private readonly DataGridReader _dataGridReader;

	public DataGridInputReaderProvider(DataGridReader dataGridReader)
	{
		_dataGridReader = dataGridReader;
	}

	public bool IsDataGridInputValid()
	{
		return _dataGridReader is object;
	}

	public DataGridReader CreateDataGridInputReader(string bulkFileShareFolderPath, ILog correlationLogger)
	{
		return _dataGridReader;
	}

	public void CleanupDataGridInput(string bulkFileShareFolderPath, ILog correlationLogger)
	{
		// nothing to clean up
	}
}