using System.ComponentModel;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public enum IAPICommunicationMode
	{
		[Description("WebAPI")] WebAPI,
		[Description("Kepler")] Kepler,
		[Description("ForceWebAPI")] ForceWebAPI,
		[Description("ForceKepler")] ForceKepler
	}
}