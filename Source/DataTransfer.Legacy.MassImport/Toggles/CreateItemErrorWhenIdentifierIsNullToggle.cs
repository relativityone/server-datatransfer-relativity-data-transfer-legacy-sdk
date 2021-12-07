using Relativity.Toggles;

namespace Relativity.MassImport.Toggles
{
	/// <summary>
	/// Toggle for REL-596362
	/// </summary>
	[DefaultValue(true), ExpectedRemovalDate(2022,3,30)]
	public class CreateItemErrorWhenIdentifierIsNullToggle : IToggle
	{
	}
}
