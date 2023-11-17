using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V2.Models;
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

		
		[HttpPost]
		[Route("IsImportEmailNotificationEnabledAsync")]
		Task<bool> IsImportEmailNotificationEnabledAsync(string correlationID);

		[HttpPost]
		[Route("RetrieveRdcConfigurationAsync")]
		Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID);
	}
}