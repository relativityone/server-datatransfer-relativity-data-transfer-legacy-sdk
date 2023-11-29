using System;
using System.Collections;
using System.Text;

namespace Relativity.MassImport.Data
{
    internal class ImportAuditor
    {
        private kCura.Data.RowDataGateway.BaseContext _context;
        private readonly Logging.ILog _correlationLogger;

        public ImportAuditor(kCura.Data.RowDataGateway.BaseContext dbContext, Logging.ILog correlationLogger)
        {
            _context = dbContext;
            _correlationLogger = correlationLogger;
        }

        public string GetImportAuditXmlSnapshot(Relativity.MassImport.DTO.ImportStatistics statistics, bool success)
        {
            IDictionary auditDictionary = this.GetImportAuditDictionary(statistics);
            if (success)
                auditDictionary.Add("Load Completion", "Success");
            else
                auditDictionary.Add("Load Completion", "Failure");
            return kCura.Utility.XmlHelper.GenerateAuditElement("import", "item", auditDictionary);
        }

        public string GetAuditCsvSnapshot(Relativity.MassImport.DTO.ImportStatistics statistics)
        {
            IDictionary auditDictionary = this.GetImportAuditDictionary(statistics);
            string[] keys = new string[auditDictionary.Keys.Count - 1 + 1];
            auditDictionary.Keys.CopyTo(keys, 0);
            Array.Sort(keys);
            StringBuilder retval = new StringBuilder();
            foreach (string key in keys)
                retval.AppendFormat("\"{0}\",\"{1}\"{2}", kCura.Utility.Strings.ToCsvCellContents(key), kCura.Utility.Strings.ToCsvCellContents(auditDictionary[key].ToString()), "\r\n");
            return retval.ToString();
        }


        public IDictionary GetImportAuditDictionary(Relativity.MassImport.DTO.ImportStatistics statistics)
        {
            System.Collections.Specialized.HybridDictionary auditDictionary = new System.Collections.Specialized.HybridDictionary();
            if (statistics.DestinationFolderArtifactID > 0)
            {
	            try
	            {
		            auditDictionary.Add("Destination Folder", @"\" + Relativity.Data.Folder.GetFolderPath(_context, statistics.DestinationFolderArtifactID));
                }
	            catch (InvalidCastException ex)
	            {
		            auditDictionary.Add("Destination Folder", statistics.DestinationFolderArtifactID);
                    _correlationLogger.LogWarning(ex, "Error getting the Folder.GetFolderPath, with param: {DestinationFolderArtifactID}", statistics.DestinationFolderArtifactID);
	            }
            }

            string filesCopiedToRepository = statistics.FilesCopiedToRepository;
            if (filesCopiedToRepository == string.Empty)
            {
	            filesCopiedToRepository = "Left in original location";
            }
            auditDictionary.Add("Files Copied To Repository", statistics.FilesCopiedToRepository.ToString());

            auditDictionary.Add("Load File Name", statistics.LoadFileName);
            auditDictionary.Add("Number of Records Created", statistics.NumberOfDocumentsCreated.ToString());
            auditDictionary.Add("Number of Records Updated", statistics.NumberOfDocumentsUpdated.ToString());
            auditDictionary.Add("Number of Load Errors", statistics.NumberOfErrors.ToString());
            auditDictionary.Add("Number of Files Loaded", statistics.NumberOfFilesLoaded.ToString());
            auditDictionary.Add("Number of Load Warnings", statistics.NumberOfWarnings.ToString());

            try
            {
                if (statistics.Overwrite != Relativity.MassImport.DTO.OverwriteType.Append)
                {
                    Relativity.Data.Field field = new Relativity.Data.Field(_context, statistics.OverlayIdentifierFieldArtifactID);
                    auditDictionary.Add("Load Overlay Identifier", field.DisplayName);
                }
            }
            catch (Exception)
            {
            }
            auditDictionary.Add("Load Method", Relativity.MassImport.DTO.OverwriteTypeHelper.ConvertToDisplayString(statistics.Overwrite));
            auditDictionary.Add("Load Mode", statistics.RepositoryConnection.ToString());
            auditDictionary.Add("Load Began on Record #", statistics.StartLine.ToString());
            auditDictionary.Add("Total Bytes Loaded - Images or Natives", statistics.TotalFileSize.ToString());
            auditDictionary.Add("Total Bytes Loaded - Metadata and Extracted Text", statistics.TotalMetadataBytes.ToString());
            if (statistics is Relativity.MassImport.DTO.ImageImportStatistics)
            {
	            this.AddImageImportAuditItems(auditDictionary, (Relativity.MassImport.DTO.ImageImportStatistics)statistics);
            }
            else if (statistics is Relativity.MassImport.DTO.ObjectImportStatistics)
            {
	            this.AddObjectImportAuditItems(auditDictionary, (Relativity.MassImport.DTO.ObjectImportStatistics)statistics);
            }

            if (statistics.BatchSizes.Length > 0)
            {
	            auditDictionary.Add("Batch Size History", FormatBatchSizes(statistics.BatchSizes));
            }
            auditDictionary.Add("Multi-Select Field Overlay Behavior", Relativity.MassImport.DTO.OverlayBehaviorHelper.ConvertToDisplayString(statistics.OverlayBehavior));
            return auditDictionary;
        }

        private void AddImageImportAuditItems(IDictionary auditDictionary, Relativity.MassImport.DTO.ImageImportStatistics statistics)
        {
            string extractedTextReplaced = "No";
            if (statistics.ExtractedTextReplaced)
            {
	            extractedTextReplaced = "Yes";
            }
            auditDictionary.Add("Extracted Text Replaced", extractedTextReplaced);
            if (statistics.ExtractedTextReplaced && statistics.ExtractedTextDefaultEncodingCodePageID > 0)
            {
	            auditDictionary.Add("Extracted Text Default Encoding", Encoding.GetEncoding(statistics.ExtractedTextDefaultEncodingCodePageID).EncodingName);
            }

            string supportImageAutoNumbering = "No";
            if (statistics.SupportImageAutoNumbering)
            {
	            supportImageAutoNumbering = "Yes";
            }
            auditDictionary.Add("Support Image Auto-Numbering", supportImageAutoNumbering);
            if (statistics.DestinationProductionArtifactID > 0) 
            {
                Relativity.Data.Artifact prod = new Relativity.Data.Artifact(_context, statistics.DestinationProductionArtifactID);
                auditDictionary.Add("Destination Production", string.Format("[ID: {0}] {1}", prod.ArtifactID, prod.TextIdentifier));
                auditDictionary.Add("Load Type", "Production");
            }
            else
            {
	            auditDictionary.Add("Load Type", "Image");
            }
        }
        private void AddObjectImportAuditItems(IDictionary auditDictionary, Relativity.MassImport.DTO.ObjectImportStatistics statistics)
        {
            auditDictionary.Add("Load Object", Relativity.Data.ObjectType.GetDisplayNameFromArtifactTypeID(_context, statistics.ArtifactTypeID));
            auditDictionary.Add("Load File Column Delimiter", string.Format("{0} ({1})", statistics.Delimiter, (int) statistics.Delimiter));
            auditDictionary.Add("Load File Quote Delimiter", string.Format("{0} ({1})", statistics.Bound, (int) statistics.Bound));
            auditDictionary.Add("Load File Newline Delimiter", string.Format("{0} ({1})", statistics.NewlineProxy, (int) statistics.NewlineProxy));
            auditDictionary.Add("Load File Multi-Value Delimiter", string.Format("{0} ({1})", statistics.MultiValueDelimiter, (int) statistics.MultiValueDelimiter));
            auditDictionary.Add("Load File Nested Value Delimiter", string.Format("{0} ({1})", statistics.NestedValueDelimiter, (int) statistics.NestedValueDelimiter));
            auditDictionary.Add("Load File Encoding", System.Text.Encoding.GetEncoding(statistics.LoadFileEncodingCodePageID).EncodingName);
            auditDictionary.Add("Load File Native File Column", statistics.FileFieldColumnName);

            if (statistics.ArtifactTypeID == (int) Relativity.ArtifactType.Document)
            {
                string extractedTextPointsToFile = "Load file contained Extracted Text";
                if (statistics.ExtractedTextPointsToFile)
                {
	                extractedTextPointsToFile = "Load file contained path to separate Extracted Text files";
                }
                auditDictionary.Add("Extracted Text Load Method", extractedTextPointsToFile);
                auditDictionary.Add("Number of Folders Created", statistics.NumberOfFoldersCreated.ToString());
                auditDictionary.Add("Load File Folder Information Column", statistics.FolderColumnName);
            }
            else
            {
                auditDictionary.Add("Load File Parent Information Column", statistics.FolderColumnName);
                if (auditDictionary.Contains("Total Bytes Loaded - Metadata and Extracted Text"))
                {
	                auditDictionary.Remove("Total Bytes Loaded - Metadata and Extracted Text");
                }
                auditDictionary.Add("Total Bytes Loaded - Metadata", statistics.TotalMetadataBytes.ToString());
                if (auditDictionary.Contains("Total Bytes Loaded - Images or Natives"))
                {
	                auditDictionary.Remove("Total Bytes Loaded - Images or Natives");
                }
                auditDictionary.Add("Total Bytes Loaded - Object Files", statistics.TotalFileSize.ToString());
            }

            if (statistics.ExtractedTextPointsToFile)
            {
	            auditDictionary.Add("Extracted Text File Encoding", Encoding.GetEncoding(statistics.ExtractedTextFileEncodingCodePageID).EncodingName);
            }
            auditDictionary.Add("Number of Choices Created", statistics.NumberOfChoicesCreated.ToString());
            StringBuilder fieldMap = new StringBuilder();
            if (statistics.FieldsMapped != null)
            {
	            foreach (int[] mapping in statistics.FieldsMapped)
	            {
		            fieldMap.AppendFormat("Load file column {0} to field {1} ({2}){3}", mapping[0] + 1, new Relativity.Data.Field(_context, mapping[1]).DisplayName, mapping[1], Environment.NewLine);
	            }
            }
            auditDictionary.Add("Fields Mapped", fieldMap.ToString());
            if (statistics.FileFieldColumnName != "")
                auditDictionary.Add("Load Type", "Metadata and Native");
            else
                auditDictionary.Add("Load Type", "Metadata");
        }

        private string FormatBatchSizes(int[] batchSizes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int size in batchSizes)
            {
                string prefix = Array.IndexOf(batchSizes, size) == 0 ? "" : "Reduced to ";
                sb.AppendFormat("{0}{1} Files{2}", prefix, size, "\r\n");
            }

            return sb.ToString();
        }
    }
}