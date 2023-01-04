using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.Export;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class ProductionService : BaseService, IProductionService
	{
		private readonly ProductionManager _productionManager;
		private readonly ITraceGenerator _traceGenerator;

		public ProductionService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_productionManager = new ProductionManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<ExportDataWrapper> RetrieveBatesByProductionAndDocumentAsync(int workspaceID, int[] productionIDs,
			int[] documentIDs, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Production.RetrieveBatesByProductionAndDocument", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var resultAsDataView = ProductionQuery.RetrieveBatesByProductionAndDocument(
				GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID), productionIDs, documentIDs);
				var resultAsObjectArrays =
					ToObjectArrays(resultAsDataView, ProductionDocumentBatesHelper.ToSerializableObjectArray);
				var result = new ExportDataWrapper(resultAsObjectArrays);
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> RetrieveProducedByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Production.RetrieveProducedByContextArtifactID", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _productionManager.ExternalRetrieveProduced(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveImportEligibleByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Production.RetrieveImportEligibleByContextArtifactID", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _productionManager.ExternalRetrieveImportEligible(GetBaseServiceContext(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
			}
		}

		public Task DoPostImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Production.DoPostImportProcessing", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				_productionManager.ExternalDoPostImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID);
				return Task.CompletedTask;
			}
		}

		public Task DoPreImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Production.DoPreImportProcessing", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				_productionManager.ExternalDoPreImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID);
				return Task.CompletedTask;
			}
		}

		public Task<ProductionInfo> ReadAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Production.Read", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _productionManager.ReadInfo(GetBaseServiceContext(workspaceID), productionArtifactID).Map<ProductionInfo>();
				return Task.FromResult(result);
			}
		}

        private static object[][] ToObjectArrays(kCura.Data.DataView dataView, Func<DataRow, object[]> transformer)
        {
            if (transformer == null)
            {
                transformer = row => row.ItemArray;
            }

            return dataView.Table.Select().Select(transformer).ToArray();
        }
	}
}