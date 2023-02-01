using System.Collections.Generic;

namespace Relativity.MassImport.Api
{
	public class MassImportArtifact
	{
		// constant fields matching staging table structure
		public string FileGuid { get; }
		public string FileName { get; }
		public string Location { get;  }
		public string OriginalFileLocation { get;  }
		public int OriginalLineNumber { get; }
		public int FileSize { get; }
		public int ParentFolderId { get; }
		
		// dynamic fields mapped by the client
		public IReadOnlyList<object> FieldValues { get; }

		public MassImportArtifact(IReadOnlyList<object> fieldValues, string fileGuid = null, string fileName = null, string location = null, string originalFileLocation = null, int originalLineNumber = 0, int fileSize = 0, int parentFolderId = 0)
		{
			FieldValues = fieldValues;
			FileGuid = fileGuid;
			FileName = fileName;
			Location = location;
			OriginalFileLocation = originalFileLocation;
			OriginalLineNumber = originalLineNumber;
			FileSize = fileSize;
			ParentFolderId = parentFolderId;
		}
	}
}
