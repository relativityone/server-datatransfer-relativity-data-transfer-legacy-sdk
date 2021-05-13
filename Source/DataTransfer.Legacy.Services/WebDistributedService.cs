using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using kCura.LongPath;
using kCura.Utility;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;
using Relativity.Kepler.Transport;
using Relativity.Services.Exceptions;
using File = Relativity.Core.DTO.File;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class WebDistributedService : BaseService, IWebDistributedService
	{
		private readonly ArtifactManager _artifactManager;
		private readonly CaseManager _caseManager;
		private readonly DynamicFieldsFileManager _fieldsFileManager;
		private readonly FileManager _fileManager;

		public WebDistributedService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_artifactManager = new ArtifactManager();
			_caseManager = new CaseManager();
			_fileManager = new FileManager();
			_fieldsFileManager = new DynamicFieldsFileManager();
		}

		public Task<IKeplerStream> DownloadFullTextAsync(int workspaceID, int artifactID, string correlationID)
		{
			throw new NotSupportedException();
		}

		public Task<IKeplerStream> DownloadLongTextFieldAsync(int workspaceID, int artifactID, int longTextFieldArtifactID, string correlationID)
		{
			throw new NotSupportedException();
		}

		public async Task<IKeplerStream> DownloadFieldFileAsync(int workspaceID, int objectArtifactID, int fileID, int fileFieldArtifactId, string correlationID)
		{
			return await ExecuteAsync(() =>
			{
				var workspaceServiceContext = GetBaseServiceContext(workspaceID);
				int artifactTypeID = _artifactManager.Read(workspaceServiceContext, objectArtifactID).ArtifactTypeID;
				bool hasPermission = _artifactManager.GetPermissionByArtifactTypeID(workspaceServiceContext, objectArtifactID, (int) ArtifactManager.PermissionType.View, artifactTypeID);

				if (!hasPermission)
				{
					throw new NotAuthorizedException("kcuraaccessdeniedmarker");
				}

				var file = _fieldsFileManager.GetFileDTO(workspaceServiceContext, fileFieldArtifactId, fileID);

				if (!LongPath.FileExists(file.Location))
				{
					throw new NotFoundException("File not found");
				}

				AuditDownload(workspaceServiceContext, file.Filename);

				return GetKeplerStream(file.Location);
			}, workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<IKeplerStream> DownloadNativeFileAsync(int workspaceID, int artifactID, Guid remoteGuid, string correlationID)
		{
			return await ExecuteAsync(() =>
			{
				var workspaceServiceContext = GetBaseServiceContext(workspaceID);

				int artifactTypeID = _artifactManager.ReadArtifact(workspaceServiceContext, artifactID).ArtifactTypeID;
				if (!PermissionsHelper.HasPermissionToView(workspaceServiceContext, artifactID, artifactTypeID))
				{
					throw new NotAuthorizedException("kcuraaccessdeniedmarker");
				}

				File file = _fileManager.Read(workspaceServiceContext, remoteGuid.ToString());
				if (file.DocumentArtifactID != artifactID)
				{
					throw new ServiceException("ArtifactID does not match file name");
				}

				string filePath = file.Location.Replace("file://", "");
				if (!LongPath.FileExists(filePath))
				{
					throw new NotFoundException("File not found");
				}

				AuditDownload(workspaceServiceContext, file.Filename);

				return GetKeplerStream(filePath);
			}, workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<IKeplerStream> DownloadTempFileAsync(int workspaceID, Guid remoteGuid, string correlationID)
		{
			return await ExecuteAsync(() =>
			{
				var workspaceServiceContext = GetBaseServiceContext(workspaceID);
				var instanceServiceContext = GetBaseServiceContext(AdminWorkspace);

				bool fileExistsInDatabase = _fileManager.Exists(workspaceServiceContext, remoteGuid.ToString());

				if (fileExistsInDatabase)
				{
					//if specified file is in database it means it's native not temp file and cannot be downloaded using this endpoint
					throw new NotAuthorizedException("kcuraaccessdeniedmarker");
				}

				int fileShareId = _caseManager.Read(instanceServiceContext, workspaceID).DefaultFileLocationCodeArtifactID;
				string fileSharePath = ResourceServerManager.Read(instanceServiceContext, fileShareId).URL;
				string filePath = Path.Combine(fileSharePath, remoteGuid.ToString());
				if (!LongPath.FileExists(filePath))
				{
					throw new NotFoundException("File not found");
				}

				AuditDownload(workspaceServiceContext, remoteGuid.ToString());

				return GetKeplerStream(filePath);
			}, workspaceID, correlationID).ConfigureAwait(false);
		}

		private IKeplerStream GetKeplerStream(string filePath)
		{
			var fileStream = new LongFileStream(filePath, FileMode.Open, FileAccess.Read);
			var keplerStream = new KeplerStream(fileStream)
			{
				Headers = new NameValueCollection {{HttpResponseHeader.ContentLength.ToString(), fileStream.Length.ToString()}}
			};
			return keplerStream;
		}

		private void AuditDownload(BaseServiceContext serviceContext, string fileName)
		{
			AuditHelper.CreateAuditRecord(serviceContext, -1, (int) AuditAction.File_Download, XmlHelper.GenerateAuditElement($"File {fileName} downloaded by {ClaimsPrincipal.Current.Claims.UserArtifactID()}"));
		}
	}
}