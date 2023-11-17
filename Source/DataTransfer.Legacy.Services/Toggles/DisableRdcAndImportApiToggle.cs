using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Toggles
{
	[Description("Disables Relativity Desktop Client (RDC) and Import API (IAPI).", "Import Export API")]
	[DefaultValue(false)]
	public class DisableRdcAndImportApiToggle : IToggle
	{
	}
}
