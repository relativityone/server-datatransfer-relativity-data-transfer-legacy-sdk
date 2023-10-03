using kCura.Utility;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Core.Pipeline.Stages.Natives
{
	internal class CreateFoldersStage<T> : Framework.IPipelineStage<T> where T : Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>
	{
		private readonly MassImportContext _context;

		public CreateFoldersStage(MassImportContext context)
		{
			_context = context;
		}

		public T Execute(T input)
		{
			var settings = input.Settings;
			bool useFolderLogic = settings.RootFolderID != 0; // Clients which contain old settings objects will end up passing 0 as RootFolderID
			if (useFolderLogic)
			{
				var folder = new Folder(_context.BaseContext.DBContext, _context.JobDetails.TableNames, _context.Logger);
				_context.ImportMeasurements.SecondaryArtifactCreationTime.Start();
				InjectionManager.Instance.Evaluate("88dbcdc5-e396-4457-945d-b7734ddfa7b2");
				int activeWorkspaceID = _context.BaseContext.AppArtifactID; // For anything that MUST differ by workspace (ie: folderManager caches mappings of a workspace's folders to their aritfactID)
				var folderGenerator = new ServerSideFolderGenerator();
				ILockHelper lockHelper = new LockHelper(new AppLockProvider());
				folderGenerator.CreateFolders(lockHelper, _context.Timekeeper, _context.BaseContext, activeWorkspaceID, folder, settings, _context.Logger);
				InjectionManager.Instance.Evaluate("37e91129-74aa-4dc7-8409-446afd242a9d");
				_context.ImportMeasurements.SecondaryArtifactCreationTime.Stop();
			}

			return input;
		}
	}
}