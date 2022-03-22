using Relativity.Core;
using Relativity.Core.Service;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	internal class ProductionImportCoordinator : IImportCoordinator
	{
		private readonly bool _inRepository;
		private readonly int _productionArtifactID;
		private readonly DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImageLoadInfo _settings;

		public ProductionImportCoordinator(bool inRepository, int productionArtifactID, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImageLoadInfo settings)
		{
			_inRepository = inRepository;
			_productionArtifactID = productionArtifactID;
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
			return massImportManager.RunProductionImageImport(serviceContext, _settings.Map<Relativity.MassImport.DTO.ImageLoadInfo>(), _productionArtifactID, _inRepository, _settings.BulkFileSharePath);
		}
	}
}