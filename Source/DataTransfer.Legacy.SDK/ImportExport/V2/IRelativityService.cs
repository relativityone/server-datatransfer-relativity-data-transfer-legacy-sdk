using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V2
{
	[WebService("Relativity Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("relativity")]
	public interface IRelativityService : IDisposable
	{
		[HttpPost]
		[Route("RetrieveCurrencySymbolAsync")]
		Task<string> RetrieveCurrencySymbolAsync(string correlationID);
	}
}