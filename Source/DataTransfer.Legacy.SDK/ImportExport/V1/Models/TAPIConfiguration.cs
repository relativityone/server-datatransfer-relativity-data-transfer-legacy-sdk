using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class TAPIConfiguration
	{
		public bool IsCloudInstance { get; set; }
		public uint? TapiMaxAllowedTargetDataRateMbps { get; set; }
	}
}
