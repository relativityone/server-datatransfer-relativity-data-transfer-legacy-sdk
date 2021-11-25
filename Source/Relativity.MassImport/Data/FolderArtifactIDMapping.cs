
namespace Relativity.MassImport.Data
{
	internal class FolderArtifactIDMapping
	{
		public int TempArtifactID { get; set; }
		public int ArtifactID { get; set; }
		public bool NewFolder { get; set; }

		public FolderArtifactIDMapping(int tempArtifactID, int artifactID, bool newFolder)
		{
			TempArtifactID = tempArtifactID;
			ArtifactID = artifactID;
			NewFolder = newFolder;
		}
	}
}