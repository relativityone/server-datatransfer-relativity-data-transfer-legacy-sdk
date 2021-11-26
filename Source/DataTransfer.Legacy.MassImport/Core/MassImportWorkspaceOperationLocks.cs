using System.Collections.Concurrent;
using Relativity.Logging;

namespace Relativity.MassImport.Core
{

	/// <summary>
	/// In memory workspace lock for mass import.
	/// IMPORTANT : This class will not handle synchronizing operations across processes or web server. So you would be better off using AppLock instead of this class.
	/// </summary>
	internal static class MassImportWorkspaceOperationLocks
	{
		private readonly static ConcurrentDictionary<MassImportManagerLockKey, object> WorkspaceOperationLocks = new ConcurrentDictionary<MassImportManagerLockKey, object>();

		/// <summary>
		/// Gets an object to lock on for a specified workspace and operation.
		/// </summary>
		/// <param name="workspaceArtifactId">The ID of the workspace for which to obtain a lock</param>
		/// <param name="operationType">The type of import operation for which to obtain a lock</param>
		/// <returns>An object that can be locked on to prevent concurrent
		/// actions on the same workspace</returns>
		public static object GetWorkspaceOperationLock(int workspaceArtifactId, MassImportManagerLockKey.LockType operationType, ILog logger)
		{
			logger.LogDebug("Returning Mass Import object lock for '{operationType}' operation and Workspace Id: {workspaceArtifactId}", operationType, (object)workspaceArtifactId);
			var lockKey = new MassImportManagerLockKey(workspaceArtifactId, operationType);
			if (!WorkspaceOperationLocks.ContainsKey(lockKey))
			{
				WorkspaceOperationLocks.TryAdd(lockKey, new object());
			}

			return WorkspaceOperationLocks[lockKey];
		}
	}
}