using Relativity.Toggles;

namespace Relativity.MassImport.Toggles
{
	/// <summary>
	/// Mass Import improvements are used in <see cref="Relativity.MassImport.Api.MassImportManager"/> when enabled.
	/// </summary>
	[DefaultValue(true)]
	public class EnableMassImportImprovementsInMassImportManager : IToggle
	{
	}
}
