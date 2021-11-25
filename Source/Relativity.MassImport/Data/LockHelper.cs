using System;
using System.Collections.Generic;
using Relativity.Core.Service;
using Relativity.Core.Toggle;
using Relativity.Logging;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.Toggles;
using MassImportManagerLockKey = Relativity.MassImport.Core.MassImportManagerLockKey;

namespace Relativity.MassImport.Data
{
	internal class LockHelper : ILockHelper
	{
		private readonly IAppLockProvider _appLockProvider;
		private readonly ILog _logger;
		private readonly bool _useApplocks;

		public LockHelper(IAppLockProvider appLockProvider, ILog logger)
		{
			_appLockProvider = appLockProvider;
			_logger = logger;
			_useApplocks = ToggleProvider.Current.IsEnabled<MassImportApplocksToggle>();
		}

		public LockHelper(IAppLockProvider appLockProvider) : this(appLockProvider, Log.Logger)
		{
		}

		public void Lock(Relativity.Core.BaseContext context, MassImportManagerLockKey.LockType operationType, Action lockedAction)
		{
			if (_useApplocks)
			{
				using (this._appLockProvider.GetAppLock(context.DBContext, operationType.ToString()))
				{
					lockedAction.Invoke();
				}
			}
			else
			{
				object operationLock = MassImportWorkspaceOperationLocks.GetWorkspaceOperationLock(context.AppArtifactID, operationType, this._logger);
				lock (operationLock)
				{
					lockedAction.Invoke();
				}
			}
		}
	}
}