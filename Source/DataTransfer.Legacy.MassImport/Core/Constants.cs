namespace Relativity.MassImport.Core
{
	internal static class Constants
	{
		internal static class SystemNames
		{
			public const string WebApi = "WebAPI";
			public const string Kepler = "Kepler";
			public const string ObjectManager = "ObjectManager";
			public const string RSAPI = "RSAPI";
		}

		internal static class ImportType
		{
			public const string Natives = "Natives";
			public const string Objects = "Objects";
			public const string Images = "Images";
			public const string Production = "Production";
		}

		internal class MassImportMetricsBucketNames
		{
			public const string JobStarted = "Relativity.MassImport.JobStarted";
			public const string BatchCompleted = "Relativity.MassImport.BatchImportTime";
			public const string PreImportStagingTableDetails = "Relativity.MassImport.StagingTableDetailsBeforeImport";
			public const string FieldDetails = "Relativity.MassImport.FieldDetails";
		}

		internal class CodeTypeNames
		{
			public const string HasImagesCodeTypeName = "HasImages";
			public const string HasPDFCodeTypeName = "HasPDF";
		}

		internal class FileTypes
		{
			public const int ImageFileType = 1;
			public const int PDFFileType = 6;
		}
	}
}
