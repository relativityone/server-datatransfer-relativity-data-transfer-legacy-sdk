using Relativity.Core;
using Relativity.Core.Service;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	internal class NativeImportCoordinator : IImportCoordinator
	{
		private readonly bool _inRepository;
		private readonly DataTransfer.Legacy.SDK.ImportExport.V1.Models.NativeLoadInfo _settings;
		private readonly bool _includeExtractedTextEncoding;

		public NativeImportCoordinator(bool inRepository, bool includeExtractedTextEncoding, DataTransfer.Legacy.SDK.ImportExport.V1.Models.NativeLoadInfo settings)
		{
			_inRepository = inRepository;
			_settings = settings;
			_includeExtractedTextEncoding = includeExtractedTextEncoding;
		}

		public DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportAuditLevel AuditLevel => _settings.AuditLevel;
		public bool DisableUserSecurityCheck => _settings.DisableUserSecurityCheck;
		public int ArtifactTypeID => (int) ArtifactType.Document;

		public bool ImportHasLinkedFiles()
		{
			return !_inRepository && _settings.UploadFiles || _settings.LoadImportedFullTextFromServer;
		}

		public MassImportManagerBase.MassImportResults RunImport(BaseServiceContext serviceContext, MassImportManager massImportManager)
		{
			return massImportManager.RunNativeImport(serviceContext, _settings.Map<NativeLoadInfo>(), _inRepository, _includeExtractedTextEncoding, _settings.BulkFileSharePath);
		}
	}
}