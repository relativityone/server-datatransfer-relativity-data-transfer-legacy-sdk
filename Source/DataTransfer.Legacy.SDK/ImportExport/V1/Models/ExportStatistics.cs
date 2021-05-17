namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ExportStatistics
	{
		public string Type { get; set; }

		public int[] Fields { get; set; }

		public string DestinationFilesystemFolder { get; set; }

		public bool OverwriteFiles { get; set; }

		public string VolumePrefix { get; set; }

		public long VolumeMaxSize { get; set; }

		public string SubdirectoryImagePrefix { get; set; }

		public string SubdirectoryNativePrefix { get; set; }

		public string SubdirectoryTextPrefix { get; set; }

		public long SubdirectoryStartNumber { get; set; }

		public long SubdirectoryMaxFileCount { get; set; }

		public string FilePathSettings { get; set; }

		public char Delimiter { get; set; }

		public char Bound { get; set; }

		public char NewlineProxy { get; set; }

		public char MultiValueDelimiter { get; set; }

		public char NestedValueDelimiter { get; set; }

		public int TextAndNativeFilesNamedAfterFieldID { get; set; }

		public bool AppendOriginalFilenames { get; set; }

		public bool ExportImages { get; set; }

		public ImageLoadFileFormatType ImageLoadFileFormat { get; set; }

		public ImageFileExportType ImageFileType { get; set; }

		public bool ExportNativeFiles { get; set; }

		public LoadFileFormat MetadataLoadFileFormat { get; set; }

		public int MetadataLoadFileEncodingCodePage { get; set; }

		public bool ExportTextFieldAsFiles { get; set; }

		public int ExportedTextFileEncodingCodePage { get; set; }

		public int ExportedTextFieldID { get; set; }

		public bool ExportMultipleChoiceFieldsAsNested { get; set; }

		public long TotalFileBytesExported { get; set; }

		public long TotalMetadataBytesExported { get; set; }

		public int ErrorCount { get; set; }

		public int WarningCount { get; set; }

		public int DocumentExportCount { get; set; }

		public int FileExportCount { get; set; }

		public ImagesToExportType ImagesToExport { get; set; }

		public int[] ProductionPrecedence { get; set; }

		public int DataSourceArtifactID { get; set; }

		public int SourceRootFolderID { get; set; }

		public int RunTimeInMilliseconds { get; set; }

		public bool CopyFilesFromRepository { get; set; }

		public int StartExportAtDocumentNumber { get; set; }

		public int VolumeStartNumber { get; set; }

		public int ArtifactTypeID { get; set; }

		public string SubdirectoryPDFPrefix { get; set; }

		public bool ExportSearchablePDFs { get; set; }
	}
}