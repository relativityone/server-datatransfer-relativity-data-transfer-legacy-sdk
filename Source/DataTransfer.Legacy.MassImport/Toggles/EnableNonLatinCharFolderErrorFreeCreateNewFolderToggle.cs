using Relativity.Toggles;

namespace DataTransfer.Legacy.MassImport.Toggles
{
	[Description("Use new db elements(FolderCandidateTableType_21f65fdc-3016-4f2b-9698-de151a6186a2 and CreateMissingFolders_21f65fdc-3016-4f2b-9698-de151a6186a2) while creating folder structure during import.", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2024, 1, 1)]
	public class EnableNonLatinCharFolderErrorFreeCreateNewFolderToggle : IToggle
	{
		
	}
}