using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("TAPI configuration service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("tapi")]
	public interface ITAPIService : IDisposable
	{
		[HttpPost]
		[Route("RetrieveConfigurationAsync")]
		Task<TAPIConfiguration> RetrieveConfigurationAsync(string correlationID);
	}
}
