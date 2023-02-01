using System;
using MassImportManagerLockKey = Relativity.MassImport.Core.MassImportManagerLockKey;

namespace Relativity.MassImport.Data
{
	internal interface ILockHelper
	{
		void Lock(Relativity.Core.BaseContext context, MassImportManagerLockKey.LockType operationType, Action lockedAction);
	}
}
