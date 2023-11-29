
namespace Relativity.MassImport.DTO
{
	public enum RepositoryConnectionType
	{
		Web,
		Direct
	}

	public abstract class ImportStatistics
	{
		private RepositoryConnectionType _repositoryConnection;
		private OverwriteType _overwrite;
		private int _overlayIdentifierFieldArtifactID;
		private int _destinationFolderArtifactID;
		private string _loadFileName;
		private int _startLine;
		private string _filesCopiedToRepository;
		private long _totalFileSize;
		private long _totalMetadataBytes;
		private int _numberOfDocumentsCreated;
		private int _numberOfDocumentsUpdated;
		private int _numberOfFilesLoaded;
		private long _numberOfErrors;
		private long _numberOfWarnings;
		private int _runTime;
		private bool _sendNotification;
		private OverlayBehavior? _overlayBehavior;

		public int[] BatchSizes { get; set; }

		public RepositoryConnectionType RepositoryConnection
		{
			get
			{
				return _repositoryConnection;
			}

			set
			{
				_repositoryConnection = value;
			}
		}

		public OverwriteType Overwrite
		{
			get
			{
				return _overwrite;
			}

			set
			{
				_overwrite = value;
			}
		}

		public int OverlayIdentifierFieldArtifactID
		{
			get
			{
				return _overlayIdentifierFieldArtifactID;
			}

			set
			{
				_overlayIdentifierFieldArtifactID = value;
			}
		}

		public int DestinationFolderArtifactID
		{
			get
			{
				return _destinationFolderArtifactID;
			}

			set
			{
				_destinationFolderArtifactID = value;
			}
		}

		public string LoadFileName
		{
			get
			{
				return _loadFileName;
			}

			set
			{
				_loadFileName = value;
			}
		}

		public int StartLine
		{
			get
			{
				return _startLine;
			}

			set
			{
				_startLine = value;
			}
		}

		public string FilesCopiedToRepository
		{
			get
			{
				return _filesCopiedToRepository;
			}

			set
			{
				_filesCopiedToRepository = value;
			}
		}

		public long TotalFileSize
		{
			get
			{
				return _totalFileSize;
			}

			set
			{
				_totalFileSize = value;
			}
		}

		public long TotalMetadataBytes
		{
			get
			{
				return _totalMetadataBytes;
			}

			set
			{
				_totalMetadataBytes = value;
			}
		}

		public int NumberOfDocumentsCreated
		{
			get
			{
				return _numberOfDocumentsCreated;
			}

			set
			{
				_numberOfDocumentsCreated = value;
			}
		}

		public int NumberOfDocumentsUpdated
		{
			get
			{
				return _numberOfDocumentsUpdated;
			}

			set
			{
				_numberOfDocumentsUpdated = value;
			}
		}

		public int NumberOfFilesLoaded
		{
			get
			{
				return _numberOfFilesLoaded;
			}

			set
			{
				_numberOfFilesLoaded = value;
			}
		}

		public long NumberOfErrors
		{
			get
			{
				return _numberOfErrors;
			}

			set
			{
				_numberOfErrors = value;
			}
		}

		public long NumberOfWarnings
		{
			get
			{
				return _numberOfWarnings;
			}

			set
			{
				_numberOfWarnings = value;
			}
		}

		public int RunTimeInMilliseconds
		{
			get
			{
				return _runTime;
			}

			set
			{
				_runTime = value;
			}
		}

		public bool SendNotification
		{
			get
			{
				return _sendNotification;
			}

			set
			{
				_sendNotification = value;
			}
		}

		public OverlayBehavior? OverlayBehavior
		{
			get
			{
				return _overlayBehavior;
			}

			set
			{
				_overlayBehavior = value;
			}
		}

		protected ImportStatistics()
		{
			// Satisfies Rule: Abstract types should not have constructors
		}
	}
}