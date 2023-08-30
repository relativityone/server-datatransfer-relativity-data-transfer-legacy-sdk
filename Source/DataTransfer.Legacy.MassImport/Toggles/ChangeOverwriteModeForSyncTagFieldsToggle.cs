namespace DataTransfer.Legacy.MassImport.Toggles
{
	using Relativity.Toggles;

	[Description("Change overwrite mode to Append for document fields containing tagging values in Sync", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2024, 1, 1)]
	public class ChangeOverwriteModeForSyncTagFieldsToggle: IToggle
	{
	}
}
