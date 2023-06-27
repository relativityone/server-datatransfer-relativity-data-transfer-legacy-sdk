namespace Relativity.MassImport.DTO
{
	public class ImportedDocumentInfo
	{
		public string Identifier { get; set; }

		public long Status { get; set; }

		public ImportedDocumentInfo()
		{
			
		}
		public ImportedDocumentInfo(string identifier, long status)
		{
			Identifier = identifier;
			Status = status;
		}
	}

}