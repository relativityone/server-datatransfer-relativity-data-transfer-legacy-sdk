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
		Task<string> RetrieveCurrencySymbolAsync(string correlationID);

		[Obsolete("I consider it not necessary anymore since we have Kepler versioning")]
		[HttpPost]
		Task<string> GetImportExportWebApiVersionAsync(string correlationID);

		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		Task<bool> ValidateSuccessfulLoginAsync(string correlationID);

		[HttpPost]
		Task<bool> IsImportEmailNotificationEnabledAsync(string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveRdcConfigurationAsync(string correlationID);

		[Obsolete("I consider it not necessary anymore since we use Kepler only")]
		[HttpPost]
		Task<string> GetRelativityUrlAsync(string correlationID);
	}
}