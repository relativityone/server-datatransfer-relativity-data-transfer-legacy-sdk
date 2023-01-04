using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class FolderService : BaseService, IFolderService
	{
		private readonly FolderManager _folderManager;
		private readonly ITraceGenerator _traceGenerator;

		public FolderService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_folderManager = new FolderManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<DataSetWrapper> RetrieveFolderAndDescendantsAsync(int workspaceID, int folderID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.RetrieveFolderAndDescendants", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _folderManager.ExtenernalRetrieveFolderAndDescendants(GetBaseServiceContext(workspaceID), folderID);
				return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
			}
		}

		public Task<Folder> ReadAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.Read", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var folder = FolderManager.Read(GetBaseServiceContext(workspaceID), folderArtifactID);
				folder.Name = XmlHelper.StripIllegalXmlCharacters(folder.Name);
				folder.TextIdentifier = XmlHelper.StripIllegalXmlCharacters(folder.TextIdentifier);
				var result = folder.Map<Folder>();
				return Task.FromResult(result);
			}
		}

		public Task<int> ReadIDAsync(int workspaceID, int parentArtifactID, string name, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.ReadID", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _folderManager.Read(GetBaseServiceContext(workspaceID), parentArtifactID, name);
				return Task.FromResult(result);
			}
		}

		public Task<int> CreateAsync(int workspaceID, int parentArtifactID, string folderName, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.Create", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _folderManager.CheckFolderExistenceThenCreateWithoutDuplicates(GetBaseServiceContext(workspaceID), parentArtifactID, folderName);
				return Task.FromResult(result);
			}
		}

		public Task<bool> ExistsAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.Exists", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _folderManager.Exists(GetBaseServiceContext(workspaceID), folderArtifactID);
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> RetrieveInitialChunkAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.RetrieveInitialChunk", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveNextChunkAsync(int workspaceID, int lastFolderID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Folder.RetrieveNextChunk", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID), lastFolderID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}
	}
}