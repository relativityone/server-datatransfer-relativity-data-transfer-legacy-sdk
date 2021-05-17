using Relativity.Core;
using Relativity.Core.Service;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	internal class RdoImportCoordinator : IImportCoordinator
	{
		private readonly bool _inRepository;
		private readonly DataTransfer.Legacy.SDK.ImportExport.V1.Models.ObjectLoadInfo _settings;

		public RdoImportCoordinator(bool inRepository, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ObjectLoadInfo settings)
		{
			_inRepository = inRepository;
			_settings = settings;
		}

		public DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportAuditLevel AuditLevel => _settings.AuditLevel;
		public bool DisableUserSecurityCheck => _settings.DisableUserSecurityCheck;
		public int ArtifactTypeID => _settings.ArtifactTypeID;

		public bool ImportHasLinkedFiles()
		{
			return !_inRepository && _settings.UploadFiles || _settings.LoadImportedFullTextFromServer;
		}

		public MassImportManagerBase.MassImportResults RunImport(BaseServiceContext serviceContext, MassImportManager massImportManager)
		{
			return massImportManager.RunObjectImport(serviceContext, _settings.Map<ObjectLoadInfo>(), _inRepository);
		}
	}
}