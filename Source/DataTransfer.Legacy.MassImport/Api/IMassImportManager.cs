using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.MassImport.Api
{
	public interface IMassImportManager
	{
		Task<MassImportResults> RunMassImportAsync(IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings, CancellationToken cancel, IProgress<MassImportProgress> progress);
	}
}
