using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Toggles
{
	[Description("Enables blocking export of production with redacted natives.", "Import Export API")]
	[DefaultValue(false)]
	public class EnabledBlockingRedactedNativesExportForProduction : IToggle
	{
	}
}
