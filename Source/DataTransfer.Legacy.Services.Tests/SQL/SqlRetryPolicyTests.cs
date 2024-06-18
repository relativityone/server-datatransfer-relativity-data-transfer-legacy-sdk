using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.SQL;
using System;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Tests.SQL
{
	[TestFixture]
	public class SqlRetryPolicyTests
	{
		private Mock<IAPILog> _mockLogger;
		private SqlRetryPolicy _sqlRetryPolicy;

		[SetUp]
		public void SetUp()
		{
			_mockLogger = new Mock<IAPILog>();
			_sqlRetryPolicy = new SqlRetryPolicy(_mockLogger.Object);
		}

		[Test]
		public async Task ExecuteAsync_FuncTaskT_ExecutesFunction()
		{
			// Arrange
			var function = new Func<Task<int>>(() => Task.FromResult(1));
			int expected = 1;

			// Act
			int result = await _sqlRetryPolicy.ExecuteAsync(function);

			// Assert
			Assert.AreEqual(expected, result);
		}

		[Test]
		public async Task ExecuteAsync_FuncTask_ExecutesFunction()
		{
			// Arrange
			bool functionCalled = false;
			var function = new Func<Task>(() =>
			{
				functionCalled = true;
				return Task.CompletedTask;
			});

			// Act
			await _sqlRetryPolicy.ExecuteAsync(function);

			// Assert
			Assert.IsTrue(functionCalled);
		}

		[Test]
		public void Execute_Action_ExecutesFunction()
		{
			// Arrange
			bool functionCalled = false;
			var function = new Action(() => functionCalled = true);

			// Act
			_sqlRetryPolicy.Execute(function);

			// Assert
			Assert.IsTrue(functionCalled);
		}

		[Test]
		public void Execute_FuncT_ExecutesFunction()
		{
			// Arrange
			var function = new Func<int>(() => 1);
			int expected = 1;

			// Act
			int result = _sqlRetryPolicy.Execute(function);

			// Assert
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void ExponentialRetryTime_ReturnsCorrectTimeSpan()
		{
			// Arrange
			int retryAttempt = 3;
			TimeSpan expectedTimeSpan = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

			// Act
			TimeSpan actualTimeSpan = (TimeSpan)typeof(SqlRetryPolicy)
				.GetMethod("ExponentialRetryTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Invoke(_sqlRetryPolicy, new object[] { retryAttempt });

			// Assert
			Assert.AreEqual(expectedTimeSpan, actualTimeSpan);
		}
	}
}
