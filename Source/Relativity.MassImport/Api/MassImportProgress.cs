using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
