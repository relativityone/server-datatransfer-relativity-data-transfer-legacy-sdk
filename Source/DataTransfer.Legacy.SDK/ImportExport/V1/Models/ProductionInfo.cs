namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ProductionInfo
	{
		public bool BatesNumbering { get; set; }

		public int BeginBatesReflectedFieldId { get; set; }

		public bool DocumentsHaveRedactions { get; set; }

		public bool IncludeImageLevelNumberingForDocumentLevelNumbering { get; set; }

		public string Name { get; set; }

		public bool UseDocumentLevelNumbering { get; set; }
	}
}