namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class Folder : Artifact
	{
		public Folder()
		{
			ArtifactTypeID = 9;
			Name = string.Empty;
		}

		[SensitiveData]
		public string Name { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}