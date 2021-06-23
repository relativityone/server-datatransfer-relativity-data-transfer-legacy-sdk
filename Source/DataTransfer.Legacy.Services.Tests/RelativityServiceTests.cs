using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class RelativityServiceTests
	{
		private Mock<IServiceContextFactory> _serviceContextFactoryMock;
		private Mock<ICommunicationModeStorage> _communicationModeStorageMock;
		private Mock<IRelativityPermissionHelper> _relativityPermissionHelperMock;
		private Mock<IAPILog> _loggerMock;
		private Mock<IMetricsContext> _metricsMock;
		private IRelativityService _uut;

		[SetUp]
		public void SetUp()
		{
			_serviceContextFactoryMock = new Mock<IServiceContextFactory>();
			_communicationModeStorageMock = new Mock<ICommunicationModeStorage>();
			_relativityPermissionHelperMock = new Mock<IRelativityPermissionHelper>();
			_loggerMock = new Mock<IAPILog>();
			_metricsMock = new Mock<IMetricsContext>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<IServiceContextFactory>().Instance(_serviceContextFactoryMock.Object));
			container.Register(Component.For<ICommunicationModeStorage>().Instance(_communicationModeStorageMock.Object));
			container.Register(Component.For<IRelativityPermissionHelper>().Instance(_relativityPermissionHelperMock.Object));
			container.Register(Component.For<ToggleCheckInterceptor>());
			container.Register(Component.For<PermissionCheckInterceptor>());
			container.Register(Component.For<LogInterceptor>());
			container.Register(Component.For<MetricsInterceptor>());
			container.Register(Component.For<UnhandledExceptionInterceptor>());
			container.Register(Component.For<IMetricsContext>().Instance(_metricsMock.Object));
			container.Register(Component.For<Func<IMetricsContext>>().UsingFactoryMethod(x =>
				new Func<IMetricsContext>(container.Resolve<IMetricsContext>)));
			container.Register(Component.For<IRelativityService>().ImplementedBy<RelativityService>());

			_uut = container.Resolve<IRelativityService>();
		}

		[Test]
		public void ShouldThrowServiceExceptionOnAllEndpointsWhenCommunicationModeTurnsKeplerServicesOff()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));

			FluentActions.Invoking(() =>
					_uut.RetrieveCurrencySymbolAsync(Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.IsImportEmailNotificationEnabledAsync(Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.RetrieveRdcConfigurationAsync(Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			// GetImportExportWebApiVersionAsync, ValidateSuccessfulLoginAsync, GetRelativityUrlAsync omitted as they are marked as obsolete
		}
	}
}
