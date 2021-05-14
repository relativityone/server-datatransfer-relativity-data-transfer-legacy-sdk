using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Ping service for DataTransfer.Legacy")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("ping")]
	public interface IPingService : IDisposable
	{
		[HttpPost]
		Task<string> PingAsync();
	}
}
