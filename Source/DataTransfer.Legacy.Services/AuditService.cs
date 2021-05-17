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

		public async Task<bool> AuditExportAsync(int workspaceID, bool isFatalError, ExportStatistics exportStatistics, string correlationID)
		{
			return await ExecuteAsync(
				() => _massExportManager.AuditExport(GetBaseServiceContext(workspaceID), isFatalError, exportStatistics.Map<MassImport.ExportStatistics>()),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<bool> AuditObjectImportAsync(int workspaceID, string runID, bool isFatalError, ObjectImportStatistics importStatistics, string correlationID)
		{
			return await ExecuteAsync(
				() => _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<MassImport.ObjectImportStatistics>()),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<bool> AuditImageImportAsync(int workspaceID, string runID, bool isFatalError, ImageImportStatistics importStatistics, string correlationID)
		{
			return await ExecuteAsync(
				() => _massImportManager.AuditImport(GetBaseServiceContext(workspaceID), runID, isFatalError, importStatistics.Map<MassImport.ImageImportStatistics>()),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task DeleteAuditTokenAsync(string token, string correlationID)
		{
			await ExecuteAsync(
				() => RelativityServicesAuthenticationTokenManager.DeleteSingleTokenForAuditSpoofing(token),
				null, correlationID).ConfigureAwait(false);
		}
	}
}