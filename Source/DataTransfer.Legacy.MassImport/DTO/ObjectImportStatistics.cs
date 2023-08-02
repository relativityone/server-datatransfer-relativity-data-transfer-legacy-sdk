
namespace Relativity.MassImport.DTO
{
	public class ObjectImportStatistics : ImportStatistics
	{
		private int _artifactTypeID;
		private char _delimiter;
		private char _bound;
		private char _newlineProxy;
		private char _multiValueDelimiter;
		private char _nestedValueDelimiter;
		private int _loadFileEncodingCodePageID;
		private int _extractedTextFileEncodingCodePageID;
		private string _folderColumnName;
		private string _fileFieldColumnName;
		private bool _extractedTextPointsToFile;
		private int _numberOfChoicesCreated;
		private int _numberOfFoldersCreated;
		private int[][] _fieldsMapped;

		public int ArtifactTypeID
		{
			get
			{
				return _artifactTypeID;
			}

			set
			{
				_artifactTypeID = value;
			}
		}

		public char Delimiter
		{
			get
			{
				return _delimiter;
			}

			set
			{
				_delimiter = value;
			}
		}

		public char Bound
		{
			get
			{
				return _bound;
			}

			set
			{
				_bound = value;
			}
		}

		public char NewlineProxy
		{
			get
			{
				return _newlineProxy;
			}

			set
			{
				_newlineProxy = value;
			}
		}

		public char MultiValueDelimiter
		{
			get
			{
				return _multiValueDelimiter;
			}

			set
			{
				_multiValueDelimiter = value;
			}
		}

		public int LoadFileEncodingCodePageID
		{
			get
			{
				return _loadFileEncodingCodePageID;
			}

			set
			{
				_loadFileEncodingCodePageID = value;
			}
		}

		public int ExtractedTextFileEncodingCodePageID
		{
			get
			{
				return _extractedTextFileEncodingCodePageID;
			}

			set
			{
				_extractedTextFileEncodingCodePageID = value;
			}
		}

		public string FolderColumnName
		{
			get
			{
				return _folderColumnName;
			}

			set
			{
				_folderColumnName = value;
			}
		}

		public string FileFieldColumnName
		{
			get
			{
				return _fileFieldColumnName;
			}

			set
			{
				_fileFieldColumnName = value;
			}
		}

		public bool ExtractedTextPointsToFile
		{
			get
			{
				return _extractedTextPointsToFile;
			}

			set
			{
				_extractedTextPointsToFile = value;
			}
		}

		public int NumberOfChoicesCreated
		{
			get
			{
				return _numberOfChoicesCreated;
			}

			set
			{
				_numberOfChoicesCreated = value;
			}
		}

		public int NumberOfFoldersCreated
		{
			get
			{
				return _numberOfFoldersCreated;
			}

			set
			{
				_numberOfFoldersCreated = value;
			}
		}

		public int[][] FieldsMapped
		{
			get
			{
				return _fieldsMapped;
			}

			set
			{
				_fieldsMapped = value;
			}
		}

		public char NestedValueDelimiter
		{
			get
			{
				return _nestedValueDelimiter;
			}

			set
			{
				_nestedValueDelimiter = value;
			}
		}
	}
}