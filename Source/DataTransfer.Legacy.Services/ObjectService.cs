﻿using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class ObjectService : BaseService, IObjectService
	{
		private readonly ObjectManager _objectManager;

		public ObjectService(IServiceContextFactory serviceContextFactory) 
			: base(serviceContextFactory)
		{
			_objectManager = new ObjectManager();
		}

		public Task<DataSetWrapper> RetrieveArtifactIdOfMappedObjectAsync(int workspaceID, string textIdentifier, int artifactTypeID, string correlationID)
		{
			var result = _objectManager.Query.RetrieveArtifactIdOfMappedObject(GetBaseServiceContext(workspaceID), textIdentifier, artifactTypeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveTextIdentifierOfMappedObjectAsync(int workspaceID, int artifactID, int artifactTypeID, string correlationID)
		{
			var result = _objectManager.Query.RetrieveTextIdentifierOfMappedObject(GetBaseServiceContext(workspaceID), artifactID, artifactTypeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveArtifactIdOfMappedParentObjectAsync(int workspaceID, string textIdentifier, int artifactTypeID, string correlationID)
		{
			var result = _objectManager.Query.RetrieveArtifactIdOfMappedParentObject(GetBaseServiceContext(workspaceID), textIdentifier, artifactTypeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}
	}
}