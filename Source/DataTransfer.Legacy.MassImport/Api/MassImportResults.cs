using System.Collections.Generic;

namespace Relativity.MassImport.Api
{
	public class MassImportResults
	{
		public int FilesProcessed { get; set; }
		public int ArtifactsProcessed { get; set; }
		public int ArtifactsCreated { get; set; }
		public int ArtifactsUpdated { get; set; }
		public MassImportExceptionDetail ExceptionDetail { get; set; }
		public IEnumerable<string> ItemErrors { get; set; }
		public string RunId { get; set; }
		public IEnumerable<int> AffectedArtifactIds { get; set; }
		public IDictionary<string, IEnumerable<int>> KeyFieldToArtifactIdsMappings { get; set; }
	}
}