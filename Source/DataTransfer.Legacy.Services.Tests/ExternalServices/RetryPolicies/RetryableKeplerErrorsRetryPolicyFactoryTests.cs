using NUnit.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using Moq;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.Services.Exceptions;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Objects.Exceptions;
using ConflictException = Relativity.Services.Exceptions.ConflictException;
using NotFoundException = Relativity.Services.Exceptions.NotFoundException;

namespace Relativity.DataTransfer.Legacy.Services.Tests.ExternalServices.RetryPolicies
{
	[TestFixture]
	public class RetryableKeplerErrorsRetryPolicyFactoryTests
	{
		private const int MaxNumberOfRetries = 3;

		private static IEnumerable NonFatalExceptions
		{
			get
			{
				yield return new ConflictException();
				yield return new DataConcurrencyException();
				yield return new InvalidOperationException();
				yield return new ServiceException();
			}
		}

		private static IEnumerable FatalExceptions
		{
			get
			{
				yield return new NotAuthorizedException();
				yield return new WireProtocolMismatchException();
				yield return new NotFoundException();
				yield return new PermissionDeniedException();
				yield return new ServiceNotFoundException();
				yield return new Exception("Test InvalidAppArtifactID exception");
				yield return new Exception("Test Bearer token should not be null or empty exception");
			}
		}

		private Mock<IAPILog> _loggerMock;
		private RetryableKeplerErrorsRetryPolicyFactory _sut;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_sut = new RetryableKeplerErrorsRetryPolicyFactory(
				_loggerMock.Object,
				MaxNumberOfRetries,
				waitTimeBaseInSeconds: 0);
		}

		[TestCaseSource(nameof(FatalExceptions))]
		public async Task CreateRetryPolicy_DoesNotRetry_OnFatalException(Exception fatalException)
		{
			// arrange
			Task Func()
			{
				throw fatalException;
			}

			// act
			var retryPolicy = _sut.CreateRetryPolicy();
			Func<Task> execute = () => retryPolicy.ExecuteAsync(Func);

			// assert
			var thrownException = await execute.Should().ThrowAsync<Exception>();
			thrownException.Which.IsSameOrEqualTo(fatalException);
			VerifyLogWarningWasCalled(Times.Never());
		}

		[TestCaseSource(nameof(FatalExceptions))]
		public async Task CreateRetryPolicyT_DoesNotRetry_OnFatalException(Exception fatalException)
		{
			// arrange
			Task<string> Func()
			{
				throw fatalException;
			}

			// act
			var retryPolicy = _sut.CreateRetryPolicy<string>();
			Func<Task> execute = () => retryPolicy.ExecuteAsync(Func);

			// assert
			var thrownException = await execute.Should().ThrowAsync<Exception>();
			thrownException.Which.IsSameOrEqualTo(fatalException);
			VerifyLogWarningWasCalled(Times.Never());
		}

		[TestCaseSource(nameof(NonFatalExceptions))]
		public async Task CreateRetryPolicy_Retries_OnRetryableException(Exception nonFatalException)
		{
			// arrange
			int counter = 0;

			Task Func()
			{
				if (counter++ == 0)
				{
					throw nonFatalException;
				}

				return Task.CompletedTask;
			}

			// act
			var retryPolicy = _sut.CreateRetryPolicy();
			await retryPolicy.ExecuteAsync(Func);

			// assert
			VerifyLogWarningWasCalled(Times.Once());
		}

		[TestCaseSource(nameof(NonFatalExceptions))]
		public async Task CreateRetryPolicyT_Retries_OnRetryableException(Exception nonFatalException)
		{
			// arrange
			const string ExpectedString = "Test";
			int counter = 0;

			Task<string> Func()
			{
				if (counter++ == 0)
				{
					throw nonFatalException;
				}

				return Task.FromResult(ExpectedString);
			}

			// act
			var retryPolicy = _sut.CreateRetryPolicy<string>();
			var result = await retryPolicy.ExecuteAsync(Func);

			// assert
			result.Should().Be(ExpectedString);
			VerifyLogWarningWasCalled(Times.Once());
		}

		private void VerifyLogWarningWasCalled(Times expectedCalls)
		{
			_loggerMock.Verify(
				x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()),
				expectedCalls);
		}
	}
}
