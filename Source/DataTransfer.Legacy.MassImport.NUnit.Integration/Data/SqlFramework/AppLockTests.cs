using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Data.SqlFramework;

namespace MassImport.NUnit.Integration.Data.SqlFramework
{
	[TestFixture]
	public class AppLockTests : MassImportTestBase
	{
		private const string ResourceName = "applock";
		private const int Timeout = 10000;
		private List<string> _messages;

		[SetUp]
		public void SetUp()
		{
			_messages = new List<string>();
		}

		[Test]
		public void ShouldNotWaitForLock()
		{
			// arrange 
			using (var semaphore = new SemaphoreSlim(1))
			{
				semaphore.Wait();

				// act
				var task1 = Task.Run(() =>
				{
					RunSynchronizedQuery(
						task: "task1",
						onGet: () => { },
						resourceName: ResourceName);
					semaphore.Release();
				});

				var task2 = Task.Run(() =>
				{
					semaphore.Wait();
					RunSynchronizedQuery(
						task: "task2",
						onGet: () => { },
						resourceName: ResourceName);
				});

				Task.WaitAll(task1, task2);
			}

			// assert
			Assert.AreEqual("task1 begin", _messages[0]);
			Assert.AreEqual("task1 get", _messages[1]);
			Assert.AreEqual("task1 release", _messages[2]);
			Assert.AreEqual("task2 begin", _messages[3]);
			Assert.AreEqual("task2 get", _messages[4]);
			Assert.AreEqual("task2 release", _messages[5]);
		}

		[Test]
		public void ShouldWaitForLockAndReturnTimeout()
		{
			// arrange
			using (var beginSemaphore = new SemaphoreSlim(1))
			using (var getSemaphore = new SemaphoreSlim(1))
			{
				beginSemaphore.Wait();
				getSemaphore.Wait();

				// act
				var task1 = Task.Run(() =>
				{
					RunSynchronizedQuery(
						task: "task1",
						onGet: () =>
						{
							beginSemaphore.Release();
							getSemaphore.Wait();
						},
						resourceName: ResourceName);
				});

				var task2 = Task.Run(() =>
				{
					beginSemaphore.Wait();
					RunSynchronizedQuery(
						task: "task2",
						onGet: () => { },
						resourceName: ResourceName);
					getSemaphore.Release();
				});

				Task.WaitAll(task1, task2);
			}

			// assert
			Assert.AreEqual("task1 begin", _messages[0]);
			Assert.AreEqual("task1 get", _messages[1]);
			Assert.AreEqual("task2 begin", _messages[2]);
			Assert.AreEqual("Failed to acquire app lock for applock. The status of -1 (The lock request timed out.) is returned.", _messages[3]);
			Assert.AreEqual("task1 release", _messages[4]);
		}

		[Test]
		public void ShouldReturnDeadlock()
		{
			using (var beginSemaphore = new SemaphoreSlim(1))
			using (var getSemaphore = new SemaphoreSlim(1))
			{
				beginSemaphore.Wait();
				getSemaphore.Wait();

				// act
				var task1 = Task.Run(() =>
				{
					RunSynchronizedQueryWithDoubleApplock(
						task:"task1", 
						onGet: () =>
						{
							beginSemaphore.Release();
							getSemaphore.Wait();
						}, 
						resourceName1:"applock1", 
						resourceName2:"applock2");
				});

				var task2 = Task.Run(() =>
				{
					beginSemaphore.Wait();
					RunSynchronizedQueryWithDoubleApplock(
						task:"task2", 
						onGet: () =>
						{
							getSemaphore.Release();
						}, 
						resourceName1:"applock2", 
						resourceName2:"applock1");
				});

				Task.WaitAll(task1, task2);
			}

			// assert
			// We cannot determine which tread will be chosen as a deadlock victim
			Assert.That(_messages, Does.Contain("Failed to acquire app lock for applock1. The status of -3 (The lock request was chosen as a deadlock victim.) is returned.")
				.Or.Contain("Failed to acquire app lock for applock2. The status of -3 (The lock request was chosen as a deadlock victim.) is returned."));
		}

		private void RunSynchronizedQuery(string task, Action onGet, string resourceName)
		{
			Context context = new Context(this.TestWorkspace.ConnectionString);
			context.BeginTransaction();
			AddMessage($"{task} begin");

			try
			{
				using (AppLock appLock = new AppLock(context, resourceName, IsTransactionActive, ShouldReleaseApplock, Log.Logger, Timeout))
				{
					AddMessage($"{task} get");
					onGet.Invoke();
					AddMessage($"{task} release");
				}

				context.CommitTransaction();
			}
			catch (ExecuteSQLStatementFailedException ex)
			{
				context.RollbackTransaction();
				AddMessage(ex.Message);
			}
		}

		private void RunSynchronizedQueryWithDoubleApplock(string task, Action onGet, string resourceName1, string resourceName2)
		{
			Context context = new Context(this.TestWorkspace.ConnectionString);
			context.BeginTransaction();
			AddMessage($"{task} begin {resourceName1}");

			try
			{
				using (AppLock appLock1 = new AppLock(context, resourceName1, IsTransactionActive, ShouldReleaseApplock, Log.Logger, Timeout))
				{
					AddMessage($"{task} get {resourceName1}");
					onGet.Invoke();
					AddMessage($"{task} begin {resourceName2}");

					using (AppLock appLock2 = new AppLock(context, resourceName2, IsTransactionActive, ShouldReleaseApplock, Log.Logger, Timeout))
					{
						AddMessage($"{task} get {resourceName2}");
						AddMessage($"{task} release {resourceName2}");
					}

					AddMessage($"{task} release {resourceName1}");
				}

				context.CommitTransaction();
			}
			catch (ExecuteSQLStatementFailedException ex)
			{
				context.RollbackTransaction();
				AddMessage(ex.Message);
			}
		}

		private bool IsTransactionActive(BaseContext c) => c.GetTransaction() != null;

		private bool ShouldReleaseApplock(BaseContext c)
		{
			// REL-270023: use null-conditional operators to store the ref and avoid possible NullReferenceException.
			var connection = c?.GetTransaction()?.Connection;

			// REL-276758: release the lock by explicitly checking State values vs HasFlag (see comment in ConnectionState MSDN docs).
			var connectionState = connection is object ? connection.State : ConnectionState.Closed;
			return connection is object && connectionState != ConnectionState.Closed && connectionState != ConnectionState.Broken;
		}

		private void AddMessage(string message)
		{
			Console.WriteLine($"{DateTime.Now:hh.mm.ss.ffffff}: {message}");
			lock (_messages)
			{
				_messages.Add(message);
			}
		}
	}
}