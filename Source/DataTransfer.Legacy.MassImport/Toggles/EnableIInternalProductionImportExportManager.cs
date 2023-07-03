namespace DataTransfer.Legacy.MassImport.Toggles
{
	using Relativity.Toggles;

	[Description("Enable using Relativity.Productions.Services.Private.V1.IInternalProductionImportExportManager", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2023, 7, 31)]
	public class EnableIInternalProductionImportExportManager : IToggle
	{
		
	}
}