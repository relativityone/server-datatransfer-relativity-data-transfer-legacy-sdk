using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Audit Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("audit")]
	public interface IAuditService : IDisposable
	{
		/// <example>
		/// Example REST request:
		/// [POST] /Relativity.REST/api/webapi-replacement/audit
		/// Request body:
		/// { "workspaceID": 123456, "isFatalError": false, "exportStatistics": {...}, "correlationID":
		/// "CE2E5412-3422-4721-9E05-317D74416E73" }
		/// </example>
		[HttpPost]
		[Route("AuditExportAsync")]
		Task<bool> AuditExportAsync(int workspaceID, bool isFatalError, [AuditExportData] ExportStatistics exportStatistics, string correlationID);

		[HttpPost]
		[Route("AuditObjectImportAsync")]
		Task<bool> AuditObjectImportAsync(int workspaceID, string runID, bool isFatalError, [AuditObjectImportData] ObjectImportStatistics importStatistics, string correlationID);

		[HttpPost]
		[Route("AuditImageImportAsync")]
		Task<bool> AuditImageImportAsync(int workspaceID, string runID, bool isFatalError, [AuditImageImportData] ImageImportStatistics importStatistics, string correlationID);

		[HttpPost]
		[Route("DeleteAuditTokenAsync")]
		Task DeleteAuditTokenAsync(string token, string correlationID);
	}
}