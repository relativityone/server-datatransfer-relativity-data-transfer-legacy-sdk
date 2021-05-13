using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;
using Relativity.Kepler.Transport;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Web Distributed replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("web-distributed")]
	public interface IWebDistributedService : IDisposable
	{
		[HttpPost]
		Task<IKeplerStream> DownloadFieldFileAsync(int workspaceID, int objectArtifactID, int fileID, int fileFieldArtifactId, string correlationID);

		[Obsolete("In RDC and IAPI SDK extracted text is downloaded using Object Manager")]
		[HttpPost]
		Task<IKeplerStream> DownloadFullTextAsync(int workspaceID, int artifactID, string correlationID);

		[Obsolete("In RDC and IAPI SDK long text is downloaded using Object Manager")]
		[HttpPost]
		Task<IKeplerStream> DownloadLongTextFieldAsync(int workspaceID, int artifactID, int longTextFieldArtifactID, string correlationID);

		[HttpPost]
		Task<IKeplerStream> DownloadNativeFileAsync(int workspaceID, int artifactID, Guid remoteGuid, string correlationID);

		[HttpPost]
		Task<IKeplerStream> DownloadTempFileAsync(int workspaceID, Guid remoteGuid, string correlationID);
	}
}