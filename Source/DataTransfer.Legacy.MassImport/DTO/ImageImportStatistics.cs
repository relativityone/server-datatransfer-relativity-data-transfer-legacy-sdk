namespace Relativity.MassImport.DTO
{
	public class ImageImportStatistics : ImportStatistics
	{
		private bool _extractedTextReplaced;
		private bool _supportImageAutoNumbering;
		private int _destinationProductionArtifactID;
		private int _extractedTextDefaultEncodingCodePageID;

		public bool ExtractedTextReplaced
		{
			get
			{
				return _extractedTextReplaced;
			}

			set
			{
				_extractedTextReplaced = value;
			}
		}

		public bool SupportImageAutoNumbering
		{
			get
			{
				return _supportImageAutoNumbering;
			}

			set
			{
				_supportImageAutoNumbering = value;
			}
		}

		public int DestinationProductionArtifactID
		{
			get
			{
				return _destinationProductionArtifactID;
			}

			set
			{
				_destinationProductionArtifactID = value;
			}
		}

		public int ExtractedTextDefaultEncodingCodePageID
		{
			get
			{
				return _extractedTextDefaultEncodingCodePageID;
			}

			set
			{
				_extractedTextDefaultEncodingCodePageID = value;
			}
		}
	}
}