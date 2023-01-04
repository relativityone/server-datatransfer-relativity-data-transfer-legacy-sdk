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
	public class AuditService : BaseService, IAuditService
	{
		private readonly IMassExportManager _massExportManager;
		private readonly MassImportManager _massImportManager;
		private readonly ITraceGenerator _traceGenerator;

		public AuditService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator)
			: base(serviceContextFactory)
		{
			_massExportManager = new MassExportManager();
			_massImportManager = new MassImportManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<bool> AuditExportAsync(int workspaceID, bool isFatalError, ExportStatistics exportStatistics, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Audit.Export", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _massExportManager.AuditExport(GetBaseServiceContext(workspaceID), isFatalError, exportStatistics.Map<MassImport.ExportStatistics>());
				return Task.FromResult(result);
			}
		}

		public Task<bool> AuditObjectImportAsync(int workspaceID, string runID, bool isFatalError, ObjectImportStatistics importStatistics, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Audit.ObjectImport", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<Relativity.MassImport.DTO.ObjectImportStatistics>());
				return Task.FromResult(result);
			}
		}

		public Task<bool> AuditImageImportAsync(int workspaceID, string runID, bool isFatalError, ImageImportStatistics importStatistics, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Audit.ImageImport", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);
				var result = _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<Relativity.MassImport.DTO.ImageImportStatistics>());
				return Task.FromResult(result);
			}
		}

		public Task DeleteAuditTokenAsync(string token, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Audit.DeleteAuditToken", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				RelativityServicesAuthenticationTokenManager.DeleteSingleTokenForAuditSpoofing(token);
				return Task.CompletedTask;
			}
		}
	}
}