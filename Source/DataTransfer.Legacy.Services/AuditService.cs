using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	using Relativity.API;

	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class AuditService : BaseService, IAuditService
	{
		private readonly IMassExportManager _massExportManager;
		private readonly MassImportManager _massImportManager;

		public AuditService(IServiceContextFactory serviceContextFactory, IHelper helper)
			: base(serviceContextFactory)
		{
			_massExportManager = new MassExportManager();
			_massImportManager = new MassImportManager(false, helper);
		}

		public Task<bool> AuditExportAsync(int workspaceID, bool isFatalError, ExportStatistics exportStatistics, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.JobType, exportStatistics.Type);

			var result = _massExportManager.AuditExport(GetBaseServiceContext(workspaceID), isFatalError, exportStatistics.Map<MassImport.ExportStatistics>());
				return Task.FromResult(result);
		}

		public Task<bool> AuditObjectImportAsync(int workspaceID, string runID, bool isFatalError, ObjectImportStatistics importStatistics, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<Relativity.MassImport.DTO.ObjectImportStatistics>());
				return Task.FromResult(result);
		}

		public Task<bool> AuditImageImportAsync(int workspaceID, string runID, bool isFatalError, ImageImportStatistics importStatistics, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<Relativity.MassImport.DTO.ImageImportStatistics>());
				return Task.FromResult(result);
		}

		public Task DeleteAuditTokenAsync(string token, string correlationID)
		{
				RelativityServicesAuthenticationTokenManager.DeleteSingleTokenForAuditSpoofing(token);
				return Task.CompletedTask;
		}
	}
}