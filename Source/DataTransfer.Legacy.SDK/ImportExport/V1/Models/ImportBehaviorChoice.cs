namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public enum ImportBehaviorChoice
	{
		LeaveBlankValuesUnchanged = 1,
		ReplaceBlankValuesWithIdentifier = 2,
		ObjectFieldContainsArtifactId = 3,
		ChoiceFieldIgnoreDuplicates = 4,
	}
}