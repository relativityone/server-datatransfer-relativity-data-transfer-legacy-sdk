using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.Toggles
{
	using Relativity.Toggles;

	[Description("Disable CAL usage", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2024, 1, 1)]
	public class DisableCALToggle : IToggle
	{
	}
}
