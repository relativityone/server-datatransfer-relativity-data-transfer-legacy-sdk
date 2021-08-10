using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("IAPI communication mode service for DataTransfer.Legacy")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("iapicommunicationmode")]
	public interface IIAPICommunicationModeService : IDisposable
	{
		[HttpPost]
		[Route("")]
		Task<IAPICommunicationMode> GetIAPICommunicationModeAsync(string correlationId);
	}
}
