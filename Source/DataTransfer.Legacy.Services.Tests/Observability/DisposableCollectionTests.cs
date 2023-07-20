using System;
using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Observability;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Observability
{
	[TestFixture]
	public class DisposableCollectionTests
	{
		[Test]
		public void ShouldDisposeAllElements()
		{
			var dispose1 = new Mock<IDisposable>();
			var dispose2 = new Mock<IDisposable>();
			var dispose3 = new Mock<IDisposable>();

			var collection = new DisposableCollection(new[]
			{
				dispose1.Object, dispose2.Object, dispose3.Object,
			});

			collection.Dispose();
			dispose1.Verify(x => x.Dispose(), Times.Once);
			dispose2.Verify(x => x.Dispose(), Times.Once);
			dispose3.Verify(x => x.Dispose(), Times.Once);
		}
	}
}
