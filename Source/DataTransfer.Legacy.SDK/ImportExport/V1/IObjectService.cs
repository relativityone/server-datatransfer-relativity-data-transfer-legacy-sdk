﻿using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Object Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("object/workspaces/{workspaceID}")]
	public interface IObjectService : IDisposable
	{
		[HttpPost]
		Task<DataSetWrapper> RetrieveArtifactIdOfMappedObjectAsync(int workspaceID, [SensitiveData] string textIdentifier, int artifactTypeID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveTextIdentifierOfMappedObjectAsync(int workspaceID, int artifactID, int artifactTypeID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveArtifactIdOfMappedParentObjectAsync(int workspaceID, [SensitiveData] string textIdentifier, int artifactTypeID, string correlationID);
	}
}