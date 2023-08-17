using Relativity.Toggles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Toggles
{
	[Description("Disable usage of new Cache for MassImport result", "Holy Data Acquisition")]
	[DefaultValue(false)]
	internal class DisableBatchResultCacheToggle : IToggle
	{
	}
}
