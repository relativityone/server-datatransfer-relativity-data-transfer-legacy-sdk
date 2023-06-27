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
	public class BulkImportServiceTests : ServicesBaseTests
	{
		private IBulkImportService _uut;

		[SetUp]
		public void SetUp()
		{
			_uut = Container.Resolve<IBulkImportService>();
		}

		[Test]
		public void ShouldThrowServiceExceptionOnAllEndpointsWhenCommunicationModeTurnsKeplerServicesOff()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));

			FluentActions.Invoking(() =>
					_uut.BulkImportImageAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.BulkImportNativeAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.NativeLoadInfo>(),
						Any.Boolean(), Any.Boolean(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.BulkImportObjectsAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ObjectLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.BulkImportProductionImageAsync(Any.Integer(),
						Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(), Any.Integer(), Any.Boolean(),
						Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.DisposeTempTablesAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GenerateImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GenerateNonImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Integer(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.HasImportPermissionsAsync(Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.ImageRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.NativeRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GetBulkImportResultAsync(Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GetImportedNativesInfoAsync(Any.Integer(), Any.String(), Any.Integer()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GetImportedImagesInfoAsync(Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

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
					_uut.BulkImportImageAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.BulkImportNativeAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.NativeLoadInfo>(),
						Any.Boolean(), Any.Boolean(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.BulkImportObjectsAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ObjectLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.BulkImportProductionImageAsync(Any.Integer(),
						Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(), Any.Integer(), Any.Boolean(),
						Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");


			FluentActions.Invoking(() =>
					_uut.DisposeTempTablesAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GenerateImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GenerateNonImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Integer(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.HasImportPermissionsAsync(Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.ImageRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.NativeRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GetBulkImportResultAsync(Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GetImportedNativesInfoAsync(Any.Integer(), Any.String(), Any.Integer()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GetImportedImagesInfoAsync(Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");
		}
	}
}
