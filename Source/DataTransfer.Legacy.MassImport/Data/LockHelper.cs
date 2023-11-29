using System;
using Relativity.MassImport.Data.SqlFramework;
using MassImportManagerLockKey = Relativity.MassImport.Core.MassImportManagerLockKey;

namespace Relativity.MassImport.Data
{
	internal class LockHelper : ILockHelper
	{
		private readonly IAppLockProvider _appLockProvider;

		public LockHelper(IAppLockProvider appLockProvider)
		{
			_appLockProvider = appLockProvider;
		}

		public void Lock(Relativity.Core.BaseContext context, MassImportManagerLockKey.LockType operationType, Action lockedAction)
		{
			using (this._appLockProvider.GetAppLock(context.DBContext, operationType.ToString()))
			{
				lockedAction.Invoke();
			}
		}
	}
}