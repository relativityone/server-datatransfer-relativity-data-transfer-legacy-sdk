
namespace Relativity.MassImport.Core.Pipeline.Input
{
	internal class ObjectImportInput : Input.CommonInput, Input.Interface.IImportSettingsInput<ObjectLoadInfo>, Input.Interface.ICollectCreatedIDsInput
	{
		public ObjectLoadInfo Settings { get; private set; }
		public bool CollectCreatedIDs { get; private set; }

		private ObjectImportInput(ObjectLoadInfo settings, bool collectCreatedIDs) : base(includeExtractedTextEncoding: false, importUpdateAuditAction: Relativity.Core.AuditAction.Update_Import)
		{
			Settings = settings;
			CollectCreatedIDs = collectCreatedIDs;
		}

		public static ObjectImportInput ForWebApi(ObjectLoadInfo settings, bool collectCreatedIDs)
		{
			return new ObjectImportInput(settings, collectCreatedIDs);
		}

		public static ObjectImportInput ForObjectManager(ObjectLoadInfo settings, bool collectCreatedIDs)
		{
			return new ObjectImportInput(settings, collectCreatedIDs);
		}
	}
}