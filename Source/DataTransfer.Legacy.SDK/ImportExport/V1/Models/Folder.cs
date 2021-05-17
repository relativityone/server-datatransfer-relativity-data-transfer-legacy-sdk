namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class Folder : Artifact
	{
		public Folder()
		{
			ArtifactTypeID = 9;
			Name = string.Empty;
		}

		public string Name { get; set; }
	}
}