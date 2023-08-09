using Relativity.Toggles;

namespace DataTransfer.Legacy.MassImport.Toggles
{
	[Description("Use legacy version of InsertAncestorsOfTopLevelObjects SQL query", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2024, 1, 1)]
	public class UseLegacyInsertAncestorsQueryToggle : IToggle
	{
	}
}
