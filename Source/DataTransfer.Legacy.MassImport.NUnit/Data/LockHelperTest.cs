using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Core.Toggle;
using Relativity.Logging;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.Toggles;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class LockHelperTest
	{
		private readonly Mock<IToggleProvider> _toggleProviderMock = new Mock<IToggleProvider>();
		private readonly Mock<ILog> _loggerMock = new Mock<ILog>();
		private readonly Mock<Relativity.Core.BaseContext> _contextMock = new Mock<Relativity.Core.BaseContext>();
		private readonly Mock<IAppLockProvider> _appLockProviderMock = new Mock<IAppLockProvider>();
		private List<string> _messages;
		private LockHelper _sut;

		[SetUp]
		public void SetUp()
		{
			ToggleProvider.Current = _toggleProviderMock.Object;
			_messages = new List<string>();
		}

		[Test]
		public void ShouldUseApplocksLock()
		{
			_toggleProviderMock.Setup(t => t.IsEnabled<MassImportApplocksToggle>()).Returns(true);
			_sut = new LockHelper(_appLockProviderMock.Object, _loggerMock.Object);

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

		[Test]
		public void ShouldSynchronizeThreadsWithCSharpLock()
		{
			_toggleProviderMock.Setup(t => t.IsEnabled<MassImportApplocksToggle>()).Returns(false);
			_sut = new LockHelper(_appLockProviderMock.Object, _loggerMock.Object);

			Action lockedAction1 = () =>
			{
				AddMessage("begin task 1");
				Thread.Sleep(1000);
				AddMessage("end task 1");
			};
			Action lockedAction2 = () =>
			{
				AddMessage("begin task 2");
				Thread.Sleep(1000);
				AddMessage("end task 2");
			};
			Action lockedAction3 = () =>
			{
				AddMessage("begin task 3");
				Thread.Sleep(1000);
				AddMessage("end task 3");
			};

			var task1 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction1));
			Thread.Sleep(100);
			var task2 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction2));
			Thread.Sleep(100);
			var task3 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction3));

			Task.WaitAll(task1, task2, task3);

			// assert
			Assert.AreEqual("begin task 1", _messages[0]);
			Assert.AreEqual("end task 1", _messages[1]);
			Assert.AreEqual("begin task 2", _messages[2]);
			Assert.AreEqual("end task 2", _messages[3]);
			Assert.AreEqual("begin task 3", _messages[4]);
			Assert.AreEqual("end task 3", _messages[5]);
		}

		[Test]
		public void ShouldIgnoreLocksForDifferentResourceNames()
		{
			_toggleProviderMock.Setup(t => t.IsEnabled<MassImportApplocksToggle>()).Returns(false);
			_sut = new LockHelper(_appLockProviderMock.Object, _loggerMock.Object);

			Action lockedAction1 = () =>
			{
				AddMessage("begin task 1");
				Thread.Sleep(1000);
				AddMessage("end task 1");
			};
			Action lockedAction2 = () =>
			{
				AddMessage("begin task 2");
				Thread.Sleep(1000);
				AddMessage("end task 2");
			};
			Action lockedAction3 = () =>
			{
				AddMessage("begin task 3");
				Thread.Sleep(1000);
				AddMessage("end task 3");
			};

			var task1 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.SingleObjectField, lockedAction1));
			Thread.Sleep(100);
			var task2 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.MultiObjectField, lockedAction2));
			Thread.Sleep(100);
			var task3 = Task.Run(() => _sut.Lock(_contextMock.Object, MassImportManagerLockKey.LockType.Choice, lockedAction3));

			Task.WaitAll(task1, task2, task3);

			// assert
			Assert.AreEqual("begin task 1", _messages[0]);
			Assert.AreEqual("begin task 2", _messages[1]);
			Assert.AreEqual("begin task 3", _messages[2]);
			Assert.AreEqual("end task 1", _messages[3]);
			Assert.AreEqual("end task 2", _messages[4]);
			Assert.AreEqual("end task 3", _messages[5]);
		}

		private void AddMessage(string message)
		{
			lock (_messages)
			{
				_messages.Add(message);
				Console.WriteLine(message);
			}
		}
	}
}