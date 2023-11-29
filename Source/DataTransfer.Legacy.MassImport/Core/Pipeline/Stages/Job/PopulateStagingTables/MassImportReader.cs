using System;
using System.Collections.Generic;
using Relativity.MassImport.Api;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables
{
	internal class MassImportReader : kCura.Data.RowDataGateway.SqlBulkCopyDataReader
	{
		private readonly IEnumerator<MassImportArtifact> _artifactsEnumerator;
		private readonly int[] _fieldIndexes;
		private readonly int _appArtifactId;
		private int _rowNumber;
		private readonly int _rootCaseArtifactId;

		public MassImportReader(IEnumerable<System.Data.SqlClient.SqlBulkCopyColumnMapping> columnMappings, IEnumerable<MassImportArtifact> artifacts, int[] fieldIndexes, int appArtifactId, int rootCaseArtifactId) : base(columnMappings)
		{
			_artifactsEnumerator = artifacts.GetEnumerator();
			_fieldIndexes = fieldIndexes;
			_appArtifactId = appArtifactId;
			_rootCaseArtifactId = rootCaseArtifactId;
		}

		public override bool Read()
		{
			_rowNumber++;
			return _artifactsEnumerator.MoveNext();
		}

		public override object GetColumnValue(int i)
		{
			object retVal;
			switch (i)
			{
				case 0: // [kCura_Import_ID]
				case 1: // [kCura_Import_Status]
				case 2: // [kCura_Import_IsNew]
				case 3: // [ArtifactID]
				case 9: // [kCura_Import_FileSize]
					{
						retVal = 0;
						break;
					}

				case 4: // [kCura_Import_OriginalLineNumber]
					{
						retVal = _rowNumber;
						break;
					}

				case 5: // [kCura_Import_FileGuid]
				case 6: // [kCura_Import_Filename]
					{
						retVal = string.Empty;
						break;
					}

				case 7: // [kCura_Import_Location]
				case 8: // [kCura_Import_OriginalFileLocation]
					{
						retVal = DBNull.Value;
						break;
					}

				case 10: // [kCura_Import_ParentFolderID]
					{
						int parentArtifactId = _artifactsEnumerator.Current?.ParentFolderId ?? 0;
						retVal = parentArtifactId == _appArtifactId ? _rootCaseArtifactId : parentArtifactId;
						break;
					}

				default:
					{
						int fieldIndex = _fieldIndexes[i - 11];
						retVal = fieldIndex >= 0 ? _artifactsEnumerator.Current?.FieldValues[fieldIndex] : null;
						break;
					}
			}
			return retVal;
		}
	}
}