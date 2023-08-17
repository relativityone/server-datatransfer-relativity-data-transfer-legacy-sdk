using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache
{
	public interface IBatchResultCache
	{
		/// <summary>
		/// If batch not exists then new entry is created and null returned.
		/// If batch exists and is finished then existing result is returned.
		/// If batch exists and is not finished yet then <see cref="ConflictException"/> is thrown.
		/// </summary>
		/// <param name="workspaceID"></param>
		/// <param name="runID"></param>
		/// <param name="batchID"></param>
		/// <returns></returns>
		MassImportResults GetCreateOrThrow(int workspaceID, string runID, string batchID);
		void Update(int workspaceID, string runID, string batchID, MassImportResults massImportResult);
		void Cleanup(int workspaceID, string runID);
	}
}
