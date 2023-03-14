using System.Collections.Generic;

namespace Relativity.MassImport.Data
{
	internal class DataGridReaderOptions
	{
		public string IdentifierColumnName { get; set; }
		public string DataGridIDColumnName { get; set; }
		public IEnumerable<FieldInfo> MappedDataGridFields { get; set; }
		public bool ReadFullTextFromFileLocation { get; set; }
		public string SqlTempTableName { get; set; }
		public bool IsImageFullTextImport { get; set; }
	}
}