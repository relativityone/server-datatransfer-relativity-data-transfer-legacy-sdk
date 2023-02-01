using System;
using System.Data;
using kCura.Data.RowDataGateway;
using Relativity.Logging;

namespace Relativity.MassImport.Data.SqlFramework
{
	public class AppLockProvider : IAppLockProvider 
	{
		public IDisposable GetAppLock(BaseContext context, string resourceName)
		{
			bool IsTransactionActive(BaseContext c) => c.GetTransaction() != null;

			bool ShouldReleaseApplock(BaseContext c)
			{
				// REL-270023: use null-conditional operators to store the ref and avoid possible NullReferenceException.
				var connection = c?.GetTransaction()?.Connection;

				// REL-276758: release the lock by explicitly checking State values vs HasFlag (see comment in ConnectionState MSDN docs).
				var connectionState = connection is object ? connection.State : ConnectionState.Closed;
				return connection is object && connectionState != ConnectionState.Closed && connectionState != ConnectionState.Broken;
			}

			return new AppLock(context, resourceName, IsTransactionActive, ShouldReleaseApplock, Log.Logger);
		}
	}
}
