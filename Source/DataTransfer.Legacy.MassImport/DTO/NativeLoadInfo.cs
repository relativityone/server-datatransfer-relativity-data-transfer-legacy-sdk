using System;
using System.Linq;

namespace Relativity.MassImport.DTO
{
	[Serializable]
	public class NativeLoadInfo
	{
        private int _keyFieldArtifactID;

        public NativeLoadInfo()
        {
            this.AuditLevel = ImportAuditLevel.FullAudit;
            this.OverlayArtifactID = -1;
            this.OverlayBehavior = OverlayBehavior.UseRelativityDefaults;
            this.ExecutionSource = ExecutionSource.Unknown;
            this.Billable = true;
        }

        public LoadRange Range { get; set; }

        public FieldInfo[] MappedFields { get; set; }

        public OverwriteType Overlay { get; set; }

        public string Repository { get; set; }

        public string RunID { get; set; }

        public string DataFileName { get; set; }

        public bool UseBulkDataImport { get; set; }

        public bool UploadFiles { get; set; }

        public string CodeFileName { get; set; }

        public string ObjectFileName { get; set; }

        public string DataGridFileName { get; set; }

        public string DataGridOffsetFileName { get; set; }

        public bool DisableUserSecurityCheck { get; set; }

        public string OnBehalfOfUserToken { get; set; }

        public ImportAuditLevel AuditLevel { get; set; }

        public string BulkLoadFileFieldDelimiter { get; set; }

        public int OverlayArtifactID { get; set; }

        public OverlayBehavior OverlayBehavior { get; set; }

        public bool LinkDataGridRecords { get; set; }

        public bool LoadImportedFullTextFromServer { get; set; }

        public int KeyFieldArtifactID
        {
	        get
	        {
		        if (_keyFieldArtifactID == 0 && MappedFields is object)
		        {
			        foreach (FieldInfo mappedField in MappedFields)
			        {
				        if (mappedField.Category == Relativity.FieldCategory.Identifier)
				        {
					        _keyFieldArtifactID = mappedField.ArtifactID;
					        break;
				        }
			        }
		        }

		        return _keyFieldArtifactID;
	        }

	        set
	        {
		        _keyFieldArtifactID = value;
	        }
        }

        public int RootFolderID { get; set; }

        public bool MoveDocumentsInAppendOverlayMode { get; set; }

        public ExecutionSource ExecutionSource { get; set; }

        public bool Billable { get; set; }

        public string BulkFileSharePath { get; set; }

        public bool OverrideReferentialLinksRestriction { get; set; }

        public bool HaveDataGridFields
        {
	        get
	        {
		        return MappedFields is object && MappedFields.Any(f => f.EnableDataGrid);
            }
        }

        public bool HasDataGridWorkToDo => HaveDataGridFields || LinkDataGridRecords;

        public string KeyFieldColumnName
        {
            get
            {
	            var fieldInfo = MappedFields?.FirstOrDefault(field => field.ArtifactID == KeyFieldArtifactID);
	            return fieldInfo is object ? fieldInfo.GetColumnName() : string.Empty;
            }
        }

        [Serializable]
        public class LoadRange
        {
            public int StartIndex { get; set; }

            public int Count { get; set; }
        }
    }
}
