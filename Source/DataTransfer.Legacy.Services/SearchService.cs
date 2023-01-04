using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
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
	public class SearchService : BaseService, ISearchService
	{
		private readonly SearchManager _searchManager;
		private readonly ViewManager _viewManager;
		private readonly ITraceGenerator _traceGenerator;

		public SearchService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_searchManager = new SearchManager();
			_viewManager = new ViewManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<bool[]> IsAssociatedSearchProviderAccessibleAsync(int workspaceID, int searchArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.IsAssociatedSearchProviderAccessible", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _searchManager.IsAssociatedSearchProviderAccessible(GetBaseServiceContext(workspaceID), searchArtifactID);
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> RetrieveNativesForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveNativesForSearch", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveNativesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrievePdfForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrievePdfForSearch", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrievePdfForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveFilesForDynamicObjects", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveFilesForDynamicObjects(GetBaseServiceContext(workspaceID), fileFieldArtifactID, objectIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveNativesForProductionAsync(int workspaceID, int productionArtifactID, string documentArtifactIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveNativesForProduction", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveNativesForProductionDocuments(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveImagesForSearchAsync(int workspaceID, int[] documentArtifactIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveImagesForSearch", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveAllImagesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveProducedImagesForDocumentAsync(int workspaceID, int documentArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveProducedImagesForDocument", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveAllByDocumentArtifactIdAndType(GetBaseServiceContext(workspaceID), documentArtifactID, (int)FileType.StampedTif);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(int workspaceID, int productionArtifactID, int[] documentArtifactIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveImagesByProductionIDsAndDocumentIDsForExportAsync(int workspaceID, int[] productionArtifactIDs, int[] documentArtifactIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveImagesByProductionIDsAndDocumentIDsForExport", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FileQuery.RetrieveByProductionIDsAndDocumentIDsForExport(GetBaseServiceContext(workspaceID), productionArtifactIDs, documentArtifactIDs);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveViewsByContextArtifactIDAsync(int workspaceID, int artifactTypeID, bool isSearch, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveViewsByContextArtifactID", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _viewManager.ExternalRetrieveViews(GetBaseServiceContext(workspaceID), artifactTypeID, isSearch);
				return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveDefaultViewFieldsForIdListAsync(int workspaceID, int artifactTypeID, int[] artifactIdList, bool isProductionList, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveDefaultViewFieldsForIdList", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _searchManager.Query.RetrieveOrderedAvfLookupByArtifactIdList(GetBaseServiceContext(workspaceID), artifactTypeID, artifactIdList, isProductionList);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}

		}

		public Task<DataSetWrapper> RetrieveAllExportableViewFieldsAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Search.RetrieveAllExportableViewFields", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _searchManager.Query.RetrieveAllExportableViewFields(GetBaseServiceContext(workspaceID), artifactTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}
	}
}