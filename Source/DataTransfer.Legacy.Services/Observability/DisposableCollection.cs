using System;
using System.Collections.Generic;

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	internal class DisposableCollection : IDisposable
	{
		private readonly List<IDisposable> _disposables;

		public DisposableCollection(IEnumerable<IDisposable> disposables)
		{
			_disposables = new List<IDisposable>(disposables);
		}

		public void Dispose()
		{
			foreach (var disposable in _disposables)
			{
				disposable.Dispose();
			}
		}
	}
}
