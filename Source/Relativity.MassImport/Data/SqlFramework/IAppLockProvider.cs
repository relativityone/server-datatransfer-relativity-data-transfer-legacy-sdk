using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Logging;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal interface IAppLockProvider
	{
		IDisposable GetAppLock(kCura.Data.RowDataGateway.BaseContext context, string resourceName);
	}
}
