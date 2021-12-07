using Relativity.Toggles;

namespace Relativity.MassImport.Toggles
{
	/// <summary>
	/// <see cref="ChoicesImportService"/> is used when enabled.
	/// <see cref="OldChoicesImportService"/> is used when disabled.
	/// </summary>
	/// <remarks>This toggle has no effect if <see cref="MassImportImprovementsToggle"/> is disabled.</remarks>
	[DefaultValue(true)]
	public class UseNewChoicesQueryToggle : IToggle
	{
	}
}
