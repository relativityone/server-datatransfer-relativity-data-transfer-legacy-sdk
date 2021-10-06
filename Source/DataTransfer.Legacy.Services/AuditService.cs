using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	public class AuditService : BaseService, IAuditService
	{
		private readonly IMassExportManager _massExportManager;
		private readonly MassImportManager _massImportManager;

		public AuditService(IServiceContextFactory serviceContextFactory) 
			: base(serviceContextFactory)
		{
			_massExportManager = new MassExportManager();
			_massImportManager = new MassImportManager();
		}

		public Task<bool> AuditExportAsync(int workspaceID, bool isFatalError, ExportStatistics exportStatistics, string correlationID)
		{
			var result  = _massExportManager.AuditExport(GetBaseServiceContext(workspaceID), isFatalError, exportStatistics.Map<MassImport.ExportStatistics>());
			return Task.FromResult(result);
		}

		public Task<bool> AuditObjectImportAsync(int workspaceID, string runID, bool isFatalError, ObjectImportStatistics importStatistics, string correlationID)
		{
			var result = _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<MassImport.ObjectImportStatistics>());
			return Task.FromResult(result);
		}

		public Task<bool> AuditImageImportAsync(int workspaceID, string runID, bool isFatalError, ImageImportStatistics importStatistics, string correlationID)
		{
			var result = _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<MassImport.ImageImportStatistics>());
			return Task.FromResult(result);
		}

		public Task DeleteAuditTokenAsync(string token, string correlationID)
		{
			RelativityServicesAuthenticationTokenManager.DeleteSingleTokenForAuditSpoofing(token);
			return Task.CompletedTask;
		}
	}
}