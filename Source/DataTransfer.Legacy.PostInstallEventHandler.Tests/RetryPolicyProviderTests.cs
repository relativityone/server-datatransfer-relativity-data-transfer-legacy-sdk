using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace DataTransfer.Legacy.PostInstallEventHandler.Tests
{
	[TestFixture]
	public class RetryPolicyProviderTests
	{
		private IRetryPolicyProvider _sut;
		private Mock<IAPILog> _loggerMock;
		private TimeSpan[] _retryTimes = new[]
		{
			TimeSpan.FromSeconds(0),
			TimeSpan.FromSeconds(0)
		};

		[SetUp]
		public void RetryPolicyProviderTestsSetup()
		{
			this._loggerMock = new Mock<IAPILog>();
			this._sut = new RetryPolicyProvider(_loggerMock.Object, _retryTimes);
		}

		[Test]
		[Category("Unit")]
		public void GetAsyncRetryPolicy_ReturnPolicy_WhenNameDefinedExplicitly()
		{
			// Arrange
			this._loggerMock.ResetCalls();

			// Act
			var result = this._sut.GetAsyncRetryPolicy("Pizza");

			// Assert
			Assert.ThrowsAsync<Exception>(() =>result.ExecuteAsync(() => this.Unsuccessful()));
			Assert.That(result, Is.Not.Null);
			Assert.That(result.PolicyKey, Is.Not.Null);
			this._loggerMock.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(this._retryTimes.Length));
		}

		[Test]
		[Category("Unit")]
		public void GetAsyncRetryPolicy_ReturnPolicy_WhenNameNotDefined()
		{
			// Arrange
			this._loggerMock.ResetCalls();

			// Act
			var result = this._sut.GetAsyncRetryPolicy();

			// Assert
			Assert.ThrowsAsync<Exception>(() => result.ExecuteAsync(() => this.Unsuccessful()));
			Assert.That(result, Is.Not.Null);
			Assert.That(result.PolicyKey, Is.Not.Null);
			this._loggerMock.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(this._retryTimes.Length));
		}

		private Task<int> Unsuccessful()
		{
			throw new Exception("Exemplary exception");
		}
	}
}