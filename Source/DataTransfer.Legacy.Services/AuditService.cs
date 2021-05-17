using System.Threading.Tasks;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class AuditService : BaseService, IAuditService
	{
		private readonly IMassExportManager _massExportManager;
		private readonly MassImportManager _massImportManager;

		public AuditService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_massExportManager = new MassExportManager();
			_massImportManager = new MassImportManager();
		}

		public Task<bool> AuditExportAsync(int workspaceID, bool isFatalError, ExportStatistics exportStatistics, string correlationID)
		{
			return ExecuteAsync(
				() => _massExportManager.AuditExport(GetBaseServiceContext(workspaceID), isFatalError, exportStatistics.Map<MassImport.ExportStatistics>()),
				workspaceID, correlationID);
		}

		public Task<bool> AuditObjectImportAsync(int workspaceID, string runID, bool isFatalError, ObjectImportStatistics importStatistics, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<MassImport.ObjectImportStatistics>()),
				workspaceID, correlationID);
		}

		public Task<bool> AuditImageImportAsync(int workspaceID, string runID, bool isFatalError, ImageImportStatistics importStatistics, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<MassImport.ImageImportStatistics>()),
				workspaceID, correlationID);
		}

		public Task DeleteAuditTokenAsync(string token, string correlationID)
		{
			return ExecuteAsync(
				() => RelativityServicesAuthenticationTokenManager.DeleteSingleTokenForAuditSpoofing(token),
				null, correlationID);
		}
	}
}