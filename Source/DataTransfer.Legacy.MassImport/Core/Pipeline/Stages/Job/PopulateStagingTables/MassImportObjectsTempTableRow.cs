namespace Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables
{
	internal class MassImportObjectsTempTableRow
	{
		public string ArtifactIdentifier { get; }
		public string ObjectName { get; }
		public int ObjectArtifactID { get; }
		public int ObjectTypeID { get; }
		public int FieldID { get; }

		public MassImportObjectsTempTableRow(string artifactIdentifier, string objectName, int objectArtifactID, int objectTypeID, int fieldID)
		{
			ArtifactIdentifier = artifactIdentifier;
			ObjectName = objectName;
			ObjectArtifactID = objectArtifactID;
			ObjectTypeID = objectTypeID;
			FieldID = fieldID;
		}
	}
}