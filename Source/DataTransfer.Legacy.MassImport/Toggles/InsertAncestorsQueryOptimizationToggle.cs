using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.Toggles
{
	using Relativity.Toggles;

	[Description("Use new version of InsertAncestorsOfTopLevelObjects SQL query", "")]
	[DefaultValue(false)]
	[ExpectedRemovalDate(2024, 1, 1)]
	public class UseLegacyInsertAncestorsQueryToggle : IToggle
	{
	}
}
