using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class WebDistributedServiceTests : ServicesBaseTests
	{
		private IWebDistributedService _uut;

		[SetUp]
		public void SetUp()
		{
			_uut = Container.Resolve<IWebDistributedService>();
		}

		[Test]
		public void ShouldThrowServiceExceptionOnAllEndpointsWhenCommunicationModeTurnsKeplerServicesOff()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));

			FluentActions.Invoking(() =>
					_uut.DownloadFieldFileAsync(Any.Integer(), Any.Integer(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.DownloadNativeFileAsync(Any.Integer(), Any.Integer(), Any.Guid(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.DownloadTempFileAsync(Any.Integer(), Any.Guid(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			// DownloadFullTextAsync, DownloadLongTextFieldAsync omitted as they are marked as obsolete
		}

		[Test]
		public void ShouldThrowPermissionDeniedExceptionOnAllEndpointsWhenCallerIsNotPermittedDoExecuteCall()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			ServiceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(false);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(false);

			FluentActions.Invoking(() =>
					_uut.DownloadFieldFileAsync(Any.Integer(), Any.Integer(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>()
				.WithMessage("User does not have permissions to use WebAPI Kepler replacement");
			FluentActions.Invoking(() =>
					_uut.DownloadNativeFileAsync(Any.Integer(), Any.Integer(), Any.Guid(), Any.String()))
				.Should().Throw<PermissionDeniedException>()
				.WithMessage("User does not have permissions to use WebAPI Kepler replacement");
			FluentActions.Invoking(() =>
					_uut.DownloadTempFileAsync(Any.Integer(), Any.Guid(), Any.String()))
				.Should().Throw<PermissionDeniedException>()
				.WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			// DownloadFullTextAsync, DownloadLongTextFieldAsync do not take workspaceId as an argument there is no possibility to check permissions - no assertion is done
		}
	}
}
