using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Toggles
{
	[Description("Disable usage of new query for File servers in Case Service", "Import Export API")]
	[DefaultValue(false)]
	internal class DisableKeplerFileRepositoryQueryToggle : IToggle
	{
	}
}
