using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache
{
	internal class NullBatchResultCache : IBatchResultCache
	{
		public MassImportResults GetCreateOrThrow(int workspaceID, string runID, string batchID)
		{
			return null;
		}

		public void Update(int workspaceID, string runID, string batchID, MassImportResults massImportResult)
		{
		}

		public void Cleanup(int workspaceID, string runID)
		{
		}
	}
}
