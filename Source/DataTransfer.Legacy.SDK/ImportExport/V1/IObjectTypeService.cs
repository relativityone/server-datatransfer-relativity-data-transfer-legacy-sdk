using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Object Type Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("object-type")]
	public interface IObjectTypeService : IDisposable
	{
		[HttpPost]
		[Route("RetrieveAllUploadableAsync")]
		Task<DataSetWrapper> RetrieveAllUploadableAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("RetrieveParentArtifactTypeIDAsync")]
		Task<DataSetWrapper> RetrieveParentArtifactTypeIDAsync(int workspaceID, int artifactTypeID, string correlationID);
	}
}