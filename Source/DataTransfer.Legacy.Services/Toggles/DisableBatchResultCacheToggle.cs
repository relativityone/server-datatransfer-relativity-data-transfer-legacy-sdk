using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Toggles
{
	[Description("Disable usage of new Cache for MassImport result", "Holy Data Acquisition")]
	[DefaultValue(false)]
	internal class DisableBatchResultCacheToggle : IToggle
	{
	}
}
