﻿using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;
using Relativity.Services.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	[TestFixture]
	public class ToggleCheckInterceptorTests
	{
		private Mock<IAPILog> _loggerMock;
		private IToggleCheckInterceptorTestClass _interceptedObject;
		private Mock<ICommunicationModeStorage> _communicationModeStorage;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_communicationModeStorage = new Mock<ICommunicationModeStorage>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<ICommunicationModeStorage>().Instance(_communicationModeStorage.Object));
			container.Register(Component.For<ToggleCheckInterceptor>());
			container.Register(Component.For<IToggleCheckInterceptorTestClass>().ImplementedBy<ToggleCheckInterceptorTestClass>());

			_interceptedObject = container.Resolve<IToggleCheckInterceptorTestClass>();
		}

		[Test]
		public void ShouldThrowServiceExceptionWhenReadingCommunicationModeFailed()
		{
			_communicationModeStorage.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((false, Any.ValueOf<IAPICommunicationMode>())));

			FluentActions.Invoking(() => _interceptedObject.Execute()).Should().Throw<ServiceException>()
				.WithMessage("Unable to determine IAPI communication mode toggle value. Please verify that instance setting is properly set (Section: DataTransfer.Legacy, Name: IAPICommunicationMode)");
		}

		[Test]
		public void ShouldThrowServiceExceptionWhenReadingCommunicationModeReturnForceWebAPI()
		{
			_communicationModeStorage.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));

			FluentActions.Invoking(() => _interceptedObject.Execute()).Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");
		}

		[Test]
		public void ShouldNotThrowServiceExceptionWhenReadingCommunicationModeReturnCommunicationTypeOtherThanForceWebAPI()
		{
			_communicationModeStorage.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));

			FluentActions.Invoking(() => _interceptedObject.Execute()).Should().NotThrow<Exception>();
		}
	}
}
