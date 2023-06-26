namespace Relativity.MassImport.DTO
{
	public class NativeImportStatus
	{
		public int ArtifactId { get; set; }

		public string Identifier { get; set; }

		public long Status { get; set; }

		public int OriginalLineNumber { get; set; }

		public NativeImportStatus(int artifactId, string identifier, long status, int originalLineNumber)
		{
			ArtifactId = artifactId;
			Identifier = identifier;
			Status = status;
			OriginalLineNumber = originalLineNumber;
		}
	}

}