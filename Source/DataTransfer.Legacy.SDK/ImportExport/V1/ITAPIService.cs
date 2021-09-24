using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("TAPI configuration service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("relativity")]
	public interface ITAPIService : IDisposable
	{
		[HttpPost]
		[Route("RetrieveTapiConfigurationAsync")]
		Task<DataSetWrapper> RetrieveTapiConfigurationAsync(string correlationID);
	}
}
