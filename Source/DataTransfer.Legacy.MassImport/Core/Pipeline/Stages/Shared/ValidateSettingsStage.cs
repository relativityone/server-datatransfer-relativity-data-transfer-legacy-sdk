using kCura.Utility;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class ValidateSettingsStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>
	{
		public T Execute(T input)
		{
			var settings = input.Settings;

			ValidateSettingsStage<T>.ValidateRunId(settings);
			ValidateSettingsStage<T>.ValidateMetadataFilesNames(settings);
			this.ValidateDataGridInput(settings);

			return input;
		}

		private static void ValidateRunId(Relativity.MassImport.DTO.NativeLoadInfo settings)
		{
			if (!SQLInjectionHelper.IsValidRunId(settings.RunID))
			{
				throw new System.Exception("Invalid RunId");
			}
		}

		private static void ValidateMetadataFilesNames(Relativity.MassImport.DTO.NativeLoadInfo settings)
		{
			if (settings.CodeFileName is null || !SQLInjectionHelper.IsValidFileName(settings.CodeFileName))
			{
				throw new System.Exception("Invalid CodeFileName");
			}

			if (settings.DataFileName is null || !SQLInjectionHelper.IsValidFileName(settings.DataFileName))
			{
				throw new System.Exception("Invalid DataFileName");
			}

			if (settings.ObjectFileName is null || !SQLInjectionHelper.IsValidFileName(settings.ObjectFileName))
			{
				throw new System.Exception("Invalid ObjectFileName");
			}
		}

		public void ValidateDataGridInput(Relativity.MassImport.DTO.NativeLoadInfo settings)
		{
			if (settings.HasDataGridWorkToDo && !ObjectBase.IsDataGridInputValid(settings))
			{
				throw new System.Exception("Invalid DataGridFileName");
			}
		}
	}
}