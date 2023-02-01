using System;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal interface IAppLockProvider
	{
		IDisposable GetAppLock(kCura.Data.RowDataGateway.BaseContext context, string resourceName);
	}
}
