using Relativity.Core;
using Relativity.Core.Service;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	internal interface IImportCoordinator
	{
		DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportAuditLevel AuditLevel { get; }
		bool DisableUserSecurityCheck { get; }
		int ArtifactTypeID { get; }

		bool ImportHasLinkedFiles();

		MassImportManagerBase.MassImportResults RunImport(BaseServiceContext serviceContext, MassImportManager massImportManager);
	}
}