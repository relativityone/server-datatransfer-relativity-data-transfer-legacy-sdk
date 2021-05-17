﻿using System.Threading.Tasks;
using Relativity.Core;
using Relativity.Core.DTO;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class FileIOService : BaseService, IFileIOService
	{
		private readonly ExternalIO _externalIo;

		public FileIOService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_externalIo = new ExternalIO();
		}

		public Task<IoResponse> BeginFillAsync(int workspaceID, byte[] b, string documentDirectory, string fileGuid, string correlationID)
		{
			return ExecuteAsync(
				() => _externalIo.ExternalBeginFill(GetBaseServiceContext(workspaceID), b, documentDirectory, workspaceID, fileGuid).Map<IoResponse>(),
				workspaceID, correlationID);
		}

		public Task<IoResponse> FileFillAsync(int workspaceID, string documentDirectory, string fileGuid, byte[] b, string correlationID)
		{
			return ExecuteAsync(
				() => _externalIo.ExternalFileFill(GetBaseServiceContext(workspaceID), documentDirectory, fileGuid, b, workspaceID).Map<IoResponse>(),
				workspaceID, correlationID);
		}

		public Task RemoveFillAsync(int workspaceID, string documentDirectory, string fileGuid, string correlationID)
		{
			return ExecuteAsync(
				() => _externalIo.ExternalRemoveFill(GetBaseServiceContext(workspaceID), documentDirectory, fileGuid, workspaceID),
				workspaceID, correlationID);
		}

		public Task RemoveTempFileAsync(int workspaceID, string fileName, string correlationID)
		{
			return ExecuteAsync(() =>
				{
					var serviceContext = GetBaseServiceContext(AdminWorkspace);
					string documentDirectory = GetDefaultDocumentDirectory(serviceContext, workspaceID);
					_externalIo.ExternalRemoveFill(serviceContext, documentDirectory, fileName, workspaceID);
				},
				workspaceID, correlationID);
		}

		public Task<string[][]> GetDefaultRepositorySpaceReportAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				var serviceContext = GetBaseServiceContext(AdminWorkspace);
				string documentDirectory = GetDefaultDocumentDirectory(serviceContext, workspaceID);
				return _externalIo.GetRepositoryReport(documentDirectory);
			}, workspaceID, correlationID);
		}

		public Task<string[][]> GetBcpShareSpaceReportAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _externalIo.GetRepositoryReport(GetBaseServiceContext(workspaceID).ChicagoContext.GetBcpSharePath()),
				workspaceID, correlationID);
		}

		public Task<string> GetBcpSharePathAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(() => GetBaseServiceContext(workspaceID).ChicagoContext.GetBcpSharePath(), workspaceID, correlationID);
		}

		public Task<bool> ValidateBcpShareAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(() => GetBaseServiceContext(workspaceID).ChicagoContext.ValidateBcpShare(), workspaceID, correlationID);
		}

		public Task<int> RepositoryVolumeMaxAsync(string correlationID)
		{
			return ExecuteAsync(() => Config.RepositoryVolumeMax, null, correlationID);
		}

		private string GetDefaultDocumentDirectory(BaseServiceContext serviceContext, int workspaceID)
		{
			CaseManager caseManager = new CaseManager();
			int fileLocationCodeArtifactID = caseManager.Read(serviceContext, workspaceID).DefaultFileLocationCodeArtifactID;
			ResourceServer resourceServer = ResourceServerManager.Read(serviceContext, fileLocationCodeArtifactID);
			return resourceServer.URL;
		}
	}
}