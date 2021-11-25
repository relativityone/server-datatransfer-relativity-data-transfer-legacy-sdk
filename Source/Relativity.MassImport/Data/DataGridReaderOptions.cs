using System.Collections.Generic;

namespace Relativity.MassImport.Data
{
	// TODO: change to internal and adjust namespace, https://jira.kcura.com/browse/REL-477112 
	public class DataGridReaderOptions
	{
		public string IdentifierColumnName { get; set; }
		public string DataGridIDColumnName { get; set; }
		public IEnumerable<FieldInfo> MappedDataGridFields { get; set; }
		public bool ReadFullTextFromFileLocation { get; set; }
		public bool LinkDataGridRecords { get; set; }
		public string SqlTempTableName { get; set; }
		public bool IsImageFullTextImport { get; set; }
	}
}