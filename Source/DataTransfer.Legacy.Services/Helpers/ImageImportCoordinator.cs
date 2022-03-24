using Relativity.Core;
using Relativity.Core.Service;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	internal class ImageImportCoordinator : IImportCoordinator
	{
		private readonly bool _inRepository;
		private readonly DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImageLoadInfo _settings;

		public ImageImportCoordinator(bool inRepository, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImageLoadInfo settings)
		{
			_inRepository = inRepository;
			_settings = settings;
		}

		public DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportAuditLevel AuditLevel => _settings.AuditLevel;
		public bool DisableUserSecurityCheck => _settings.DisableUserSecurityCheck;
		public int ArtifactTypeID => (int) ArtifactType.Document;

		public bool ImportHasLinkedFiles()
		{
			return !_inRepository;
		}

		public MassImportManagerBase.MassImportResults RunImport(BaseServiceContext serviceContext, MassImportManager massImportManager)
		{
			return massImportManager.RunImageImport(serviceContext, _settings.Map<Relativity.MassImport.DTO.ImageLoadInfo>(), _inRepository, _settings.BulkFileSharePath);
		}
	}
}