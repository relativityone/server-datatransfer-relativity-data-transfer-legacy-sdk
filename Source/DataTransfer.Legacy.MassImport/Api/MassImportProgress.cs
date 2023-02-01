using System.Collections.Generic;

namespace Relativity.MassImport.Api
{
	public class MassImportProgress
	{
		public IEnumerable<int> AffectedArtifactIds { get; }

		public MassImportProgress(IEnumerable<int> affectedArtifactIds)
		{
			AffectedArtifactIds = affectedArtifactIds;
		}
	}
}
