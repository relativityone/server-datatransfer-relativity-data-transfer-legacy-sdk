namespace Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables
{
	internal class MassImportCodeTempTableRow
	{
		public string ArtifactIdentifier { get; }
		public int CodeArtifactID { get; }
		public int CodeTypeID { get; }

		public MassImportCodeTempTableRow(string artifactIdentifier, int codeArtifactID, int codeTypeID)
		{
			ArtifactIdentifier = artifactIdentifier;
			CodeArtifactID = codeArtifactID;
			CodeTypeID = codeTypeID;
		}
	}
}