using System;
using System.Collections.Generic;

namespace Relativity.Core.Service.MassImport.Logging
{
	// TODO: change to internal and correct namespace, https://jira.kcura.com/browse/REL-482642
	public class StackOfDisposables : IDisposable
	{
		private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

		public void Push(IDisposable disposable)
		{
			_disposables.Push(disposable);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~StackOfDisposables()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				while (_disposables.Count > 0)
				{
					var disposable = _disposables.Pop();
					disposable?.Dispose();
				}
			}
		}
	}
}