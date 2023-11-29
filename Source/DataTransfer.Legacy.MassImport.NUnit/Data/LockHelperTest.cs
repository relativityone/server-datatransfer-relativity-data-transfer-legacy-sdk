using System;
using System.Threading;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class LockHelperTest
	{
		private readonly Mock<Relativity.Core.BaseContext> _contextMock = new Mock<Relativity.Core.BaseContext>();
		private readonly Mock<IAppLockProvider> _appLockProviderMock = new Mock<IAppLockProvider>();
		private LockHelper _sut;

		[Test]
		public void ShouldUseApplocksLock()
		{
			_sut = new LockHelper(_appLockProviderMock.Object);

			Action lockedAction = () =>
			{
				Thread.Sleep(1000);
			};

			var task1 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction));
			Thread.Sleep(100);
			var task2 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction));
			Thread.Sleep(100);
			var task3 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction));

			Task.WaitAll(task1, task2, task3);

			// assert
			_appLockProviderMock.Verify(t => t.GetAppLock(It.IsAny<BaseContext>(), It.IsAny<string>()), Times.Exactly(3));
		}
	}
}