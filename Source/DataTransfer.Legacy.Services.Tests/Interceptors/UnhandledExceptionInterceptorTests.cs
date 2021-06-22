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
				.WithMessage("Error during call Execute")
				.WithInnerException<Exception>();
		}
	}
}
