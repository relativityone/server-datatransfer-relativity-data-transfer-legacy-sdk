using System;

namespace Relativity.MassImport.Data.SqlFramework
{
	public class AppLockException : Exception
	{
		public readonly int? ErrorCode;

		public AppLockException()
		{
		}

		public AppLockException(string message) : base(message)
		{
		}

		public AppLockException(string message, Exception inner) : base(message, inner)
		{
		}

		public AppLockException(
			string message,
			int errorCode) : base(message)
		{
			this.ErrorCode = errorCode;
		}
	}
}
