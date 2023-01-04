using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.Core;
using kCura.LongPath;
using kCura.Utility;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.Kepler.Transport;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class WebDistributedService : BaseService, IWebDistributedService
	{
		private readonly ArtifactManager _artifactManager;
		private readonly CaseManager _caseManager;
		private readonly DynamicFieldsFileManager _fieldsFileManager;
		private readonly FileManager _fileManager;
		private readonly ITraceGenerator _traceGenerator;

		public WebDistributedService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_artifactManager = new ArtifactManager();
			_caseManager = new CaseManager();
			_fileManager = new FileManager();
			_fieldsFileManager = new DynamicFieldsFileManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<IKeplerStream> DownloadFullTextAsync(int workspaceID, int artifactID, string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<IKeplerStream> DownloadLongTextFieldAsync(int workspaceID, int artifactID, int longTextFieldArtifactID, string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<IKeplerStream> DownloadFieldFileAsync(int workspaceID, int objectArtifactID, int fileID,
			int fileFieldArtifactId, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.WebDistributed.DownloadFieldFile", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var workspaceServiceContext = GetBaseServiceContext(workspaceID);
				var artifactTypeID = _artifactManager.Read(workspaceServiceContext, objectArtifactID).ArtifactTypeID;
				var hasPermission = _artifactManager.GetPermissionByArtifactTypeID(workspaceServiceContext,
					objectArtifactID, (int)ArtifactManager.PermissionType.View, artifactTypeID);

				if (!hasPermission)
				{
					throw new NotAuthorizedException("kcuraaccessdeniedmarker");
				}

				var file = _fieldsFileManager.GetFileDTO(workspaceServiceContext, fileFieldArtifactId, fileID);

				if (!LongPath.FileExists(file.Location))
				{
					throw new ConflictException("File not found");
				}

				AuditDownload(workspaceServiceContext, file.Filename);

				var result = GetKeplerStream(file.Location);
				return Task.FromResult(result);
			}
		}

		public Task<IKeplerStream> DownloadNativeFileAsync(int workspaceID, int artifactID, Guid remoteGuid,
			string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.WebDistributed.DownloadNativeFile", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var workspaceServiceContext = GetBaseServiceContext(workspaceID);

				var artifactTypeId = _artifactManager.ReadArtifact(workspaceServiceContext, artifactID).ArtifactTypeID;
				if (!PermissionsHelper.HasPermissionToView(workspaceServiceContext, artifactID, artifactTypeId))
				{
					throw new NotAuthorizedException("kcuraaccessdeniedmarker");
				}

				var file = _fileManager.Read(workspaceServiceContext, remoteGuid.ToString());
				if (file.DocumentArtifactID != artifactID)
				{
					throw new ServiceException("ArtifactID does not match file name");
				}

				var filePath = file.Location.Replace("file://", "");
				if (!LongPath.FileExists(filePath))
				{
					throw new ConflictException("File not found");
				}

				AuditDownload(workspaceServiceContext, file.Filename);

				var result = GetKeplerStream(filePath);
				return Task.FromResult(result);
			}
		}

		public Task<IKeplerStream> DownloadTempFileAsync(int workspaceID, Guid remoteGuid, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.WebDistributed.DownloadTempFile", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var workspaceServiceContext = GetBaseServiceContext(workspaceID);
				var instanceServiceContext = GetBaseServiceContext(AdminWorkspace);

				var fileExistsInDatabase = _fileManager.Exists(workspaceServiceContext, remoteGuid.ToString());

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
					throw new ConflictException("File not found");
				}

				AuditDownload(workspaceServiceContext, remoteGuid.ToString());

				var result = GetKeplerStream(filePath);
				return Task.FromResult(result);
			}
		}

		private static IKeplerStream GetKeplerStream(string filePath)
		{
			var fileStream = new LongFileStream(filePath, FileMode.Open, FileAccess.Read);
			var keplerStream = new KeplerStream(fileStream)
			{
				Headers = new NameValueCollection {{HttpResponseHeader.ContentLength.ToString(), fileStream.Length.ToString()}}
			};
			return keplerStream;
		}

		private static void AuditDownload(BaseServiceContext serviceContext, string fileName)
		{
			AuditHelper.CreateAuditRecord(serviceContext, -1, (int) AuditAction.File_Download, XmlHelper.GenerateAuditElement($"File {fileName} downloaded by {ClaimsPrincipal.Current.Claims.UserArtifactID()}"));
		}
	}
}