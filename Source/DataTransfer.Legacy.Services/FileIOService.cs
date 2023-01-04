using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using System.ServiceModel.Syndication;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class FileIOService : BaseService, IFileIOService
	{
		private readonly ExternalIO _externalIo;
		private readonly ITraceGenerator _traceGenerator;

		public FileIOService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_externalIo = new ExternalIO();
			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<IoResponse> BeginFillAsync(int workspaceID, byte[] b, string documentDirectory, string fileName, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.BeginFill", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _externalIo.ExternalBeginFill(GetBaseServiceContext(workspaceID), b, documentDirectory, workspaceID, fileName).Map<IoResponse>();
				return Task.FromResult(result);
			}
		}

		public Task<IoResponse> FileFillAsync(int workspaceID, string documentDirectory, string fileName, byte[] b, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.FileFill", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _externalIo.ExternalFileFill(GetBaseServiceContext(workspaceID), documentDirectory, fileName, b, workspaceID).Map<IoResponse>();
				return Task.FromResult(result);
			}
		}

		public Task RemoveFillAsync(int workspaceID, string documentDirectory, string fileName, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.RemoveFill", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				_externalIo.ExternalRemoveFill(GetBaseServiceContext(workspaceID), documentDirectory, fileName, workspaceID);
				return Task.CompletedTask;
			}
		}

		public Task RemoveTempFileAsync(int workspaceID, string fileName, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.RemoveTempFile", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var documentDirectory = GetDefaultDocumentDirectory(workspaceID);
				_externalIo.ExternalRemoveFill(GetBaseServiceContext(workspaceID), documentDirectory, fileName,
					workspaceID);
				return Task.CompletedTask;
			}
		}

		public Task<string[][]> GetDefaultRepositorySpaceReportAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.GetDefaultRepositorySpaceReport", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var documentDirectory = GetDefaultDocumentDirectory(workspaceID);
				var result = _externalIo.GetRepositoryReport(documentDirectory);
				return Task.FromResult(result);
			}
		}

		public Task<string[][]> GetBcpShareSpaceReportAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.GetBcpShareSpaceReport", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result =
				_externalIo.GetRepositoryReport(GetBaseServiceContext(workspaceID).ChicagoContext.GetBcpSharePath());
				return Task.FromResult(result);
			}
		}

		public Task<string> GetBcpSharePathAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.GetBcpSharePath", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = GetBaseServiceContext(workspaceID).ChicagoContext.GetBcpSharePath();
				return Task.FromResult(result);
			}
		}

		public Task<bool> ValidateBcpShareAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.ValidateBcpShare", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = GetBaseServiceContext(workspaceID).ChicagoContext.ValidateBcpShare();
				return Task.FromResult(result);
			}
		}

		public Task<int> RepositoryVolumeMaxAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.FileIO.RepositoryVolumeMax", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = Config.RepositoryVolumeMax;
				return Task.FromResult(result);
			}
		}

		private string GetDefaultDocumentDirectory(int workspaceID)
		{
			var serviceContext = GetBaseServiceContext(AdminWorkspace);
			var caseManager = new CaseManager();
			var fileLocationCodeArtifactId = caseManager.Read(serviceContext, workspaceID).DefaultFileLocationCodeArtifactID;
			var resourceServer = ResourceServerManager.Read(serviceContext, fileLocationCodeArtifactId);
			return resourceServer.URL;
		}
	}
}