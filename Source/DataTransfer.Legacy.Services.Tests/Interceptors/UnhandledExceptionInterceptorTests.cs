using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	using Relativity.Services.Objects.Exceptions;

	[TestFixture]
	public class UnhandledExceptionInterceptorTests
	{
		private Mock<IAPILog> _loggerMock;
		private IUnhandledExceptionInterceptorTestsClass _interceptedObject;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<UnhandledExceptionInterceptor>());
			container.Register(Component.For<IUnhandledExceptionInterceptorTestsClass>().ImplementedBy<UnhandledExceptionInterceptorTestsClass>());

			_interceptedObject = container.Resolve<IUnhandledExceptionInterceptorTestsClass>();
		}

		[Test]
		public void ShouldLogErrorAndThrowServiceExceptionWithInnerWhenExecutingMethodThrows()
		{
			FluentActions.Invoking(() => _interceptedObject.Execute()).Should().Throw<ServiceException>()
				.WithMessage($"Error during call {nameof(UnhandledExceptionInterceptorTestsClass)}.{nameof(IUnhandledExceptionInterceptorTestsClass.Execute)}. InnerExceptionType: System.Exception, InnerExceptionMessage: *")
				.WithInnerException<Exception>();
		}

		[Test]
		public void ShouldLogErrorAndThrowPermissionDeniedExceptionWithInnerWhenExecutingMethodThrows()
		{
			FluentActions.Invoking(() => _interceptedObject.ExecuteWithPermissionException()).Should().Throw<PermissionDeniedException>()
				.WithMessage($"Error during call {nameof(UnhandledExceptionInterceptorTestsClass)}.{nameof(IUnhandledExceptionInterceptorTestsClass.ExecuteWithPermissionException)}. InnerExceptionType: Relativity.Core.Exception.Permission, InnerExceptionMessage: You do not have permission to view this item (ArtifactID=12345678)")
				.WithInnerException<Relativity.Core.Exception.Permission>();
		}

		[Test]
		public void ShouldLogErrorAndThrowPermissionDeniedExceptionWithInnerInsufficientAccessControlListPermissionsWhenExecutingMethodThrows()
		{
			FluentActions.Invoking(() => _interceptedObject.ExecuteWithInsufficientAccessControlListPermissions()).Should().Throw<PermissionDeniedException>()
				.WithMessage($"Error during call {nameof(UnhandledExceptionInterceptorTestsClass)}.{nameof(IUnhandledExceptionInterceptorTestsClass.ExecuteWithInsufficientAccessControlListPermissions)}. InnerExceptionType: Relativity.Core.Exception.InsufficientAccessControlListPermissions, InnerExceptionMessage: Insufficient Permissions! Please ask your Relativity Administrator to allow you import permission.")
				.WithInnerException<Relativity.Core.Exception.InsufficientAccessControlListPermissions>();
		}

		[Test]
		public void ShouldLogErrorAndThrowNotFoundExceptionWithInnerWhenExecutingMethodThrows()
		{
			FluentActions.Invoking(() => _interceptedObject.ExecuteWithBaseException()).Should().Throw<NotFoundException>()
				.WithMessage($"Error during call {nameof(UnhandledExceptionInterceptorTestsClass)}.{nameof(IUnhandledExceptionInterceptorTestsClass.ExecuteWithBaseException)}. InnerExceptionType: Relativity.Core.Exception.BaseException, InnerExceptionMessage: ArtifactID 1234567 does not exist.")
				.WithInnerException<Relativity.Core.Exception.BaseException>();
		}

		[Test]
		public void ShouldLogErrorAndThrowServiceExceptionWithInnerWhenExecutingMethodThrowsBaseExceptionAndMessageIsNotArtifactIDDoesNotExist()
		{
			FluentActions.Invoking(() => _interceptedObject.ExecuteWithBaseExceptionDifferentMessage()).Should().Throw<ServiceException>()
				.WithMessage($"Error during call {nameof(UnhandledExceptionInterceptorTestsClass)}.{nameof(IUnhandledExceptionInterceptorTestsClass.ExecuteWithBaseExceptionDifferentMessage)}. InnerExceptionType: Relativity.Core.Exception.BaseException, InnerExceptionMessage: *")
				.WithInnerException<Relativity.Core.Exception.BaseException>();
		}
	}
}
