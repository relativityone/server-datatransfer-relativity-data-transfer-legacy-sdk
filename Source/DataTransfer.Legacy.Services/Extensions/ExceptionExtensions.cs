using System;
using System.Collections.Generic;

namespace Relativity.DataTransfer.Legacy.Services.Extensions
{
	public static class ExceptionExtensions
	{
		public static IEnumerable<Exception> GetAllExceptionsInChain(this Exception exception)
		{
			const int MaxChainLength = 100;

			int chainLength = 0;
			Exception currentException = exception;
			while (currentException != null && chainLength < MaxChainLength)
			{
				yield return currentException;

				chainLength++;
				currentException = currentException.InnerException;
			}
		}
	}
}
