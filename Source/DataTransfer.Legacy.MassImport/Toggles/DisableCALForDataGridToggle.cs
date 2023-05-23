namespace DataTransfer.Legacy.MassImport.Toggles
{
	using Relativity.Toggles;

	[Description("Disable CAL usage in Data Grid", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2024, 1, 1)]
	public class DisableCALForDataGridToggle : IToggle
	{
	}
}
