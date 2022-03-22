
namespace Relativity.MassImport.Core.Pipeline.Input
{
	internal class ObjectImportInput : Input.CommonInput, Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.ObjectLoadInfo>, Input.Interface.ICollectCreatedIDsInput
	{
		public Relativity.MassImport.DTO.ObjectLoadInfo Settings { get; private set; }
		public bool CollectCreatedIDs { get; private set; }

		private ObjectImportInput(Relativity.MassImport.DTO.ObjectLoadInfo settings, bool collectCreatedIDs, string bulkFileSharePath = null) : base(includeExtractedTextEncoding: false, importUpdateAuditAction: Relativity.Core.AuditAction.Update_Import)
		{
			Settings = settings;
			CollectCreatedIDs = collectCreatedIDs;
			BulkFileSharePath = bulkFileSharePath;
		}

		public static ObjectImportInput ForWebApi(Relativity.MassImport.DTO.ObjectLoadInfo settings, bool collectCreatedIDs, string bulkFileSharePath)
		{
			return new ObjectImportInput(settings, collectCreatedIDs, bulkFileSharePath);
		}

		public static ObjectImportInput ForObjectManager(Relativity.MassImport.DTO.ObjectLoadInfo settings, bool collectCreatedIDs)
		{
			return new ObjectImportInput(settings, collectCreatedIDs);
		}
	}
}