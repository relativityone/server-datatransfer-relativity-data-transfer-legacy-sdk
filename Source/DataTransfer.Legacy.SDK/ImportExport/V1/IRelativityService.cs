using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
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
		[Route("RetrieveCurrencySymbolV2Async")]
		Task<string> RetrieveCurrencySymbolV2Async(string correlationID);

		[Obsolete("I consider it not necessary anymore since we have Kepler versioning")]
		[HttpPost]
		[Route("GetImportExportWebApiVersionAsync")]
		Task<string> GetImportExportWebApiVersionAsync(string correlationID);

		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		[Route("ValidateSuccessfulLoginAsync")]
		Task<bool> ValidateSuccessfulLoginAsync(string correlationID);

		[HttpPost]
		[Route("IsImportEmailNotificationEnabledAsync")]
		Task<bool> IsImportEmailNotificationEnabledAsync(string correlationID);

		[HttpPost]
		[Route("RetrieveRdcConfigurationAsync")]
		Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID);

		[Obsolete("I consider it not necessary anymore since we use Kepler only")]
		[HttpPost]
		[Route("GetRelativityUrlAsync")]
		Task<string> GetRelativityUrlAsync(string correlationID);
	}
}