using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Core.Service;
using MassImportManagerLockKey = Relativity.MassImport.Core.MassImportManagerLockKey;

namespace Relativity.MassImport.Data
{
	internal interface ILockHelper
	{
		void Lock(Relativity.Core.BaseContext context, MassImportManagerLockKey.LockType operationType, Action lockedAction);
	}
}
