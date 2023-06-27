namespace Relativity.MassImport.DTO
{
	public class ImportedDocumentInfo
	{
		public string Identifier { get; set; }

		public long Status { get; set; }

		public int OriginalLineNumber { get; set; }

		public ImportedDocumentInfo()
		{
			
		}
		public ImportedDocumentInfo(string identifier, long status, int originalLineNumber)
		{
			Identifier = identifier;
			Status = status;
			OriginalLineNumber = originalLineNumber;
		}
	}

}