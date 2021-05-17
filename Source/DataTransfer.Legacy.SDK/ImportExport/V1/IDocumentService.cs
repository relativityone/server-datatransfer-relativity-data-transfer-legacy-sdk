using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Document Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("document")]
	public interface IDocumentService : IDisposable
	{
		Task<int[]> RetrieveAllUnsupportedOiFileIdsAsync(string correlationID);
	}
}