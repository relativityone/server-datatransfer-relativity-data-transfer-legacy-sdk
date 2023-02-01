using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Toggles
{
	[Description("Disable usage of new Cache for MassImport result", "Import Export API")]
	[DefaultValue(false)]
	internal class DisableBatchResultCacheToggle : IToggle
	{
	}
}
