using Relativity.Toggles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.Toggles
{
	[Description("Enables optimized version of UpdateMetadata method", "")]
	[DefaultValue(true)]
	[ExpectedRemovalDate(2024, 3, 1)]
	public class EnableUpdateMetadataOptimization : IToggle
	{
	}
}
