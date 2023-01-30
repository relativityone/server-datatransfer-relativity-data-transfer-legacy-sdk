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
	public class ExportServiceTests : ServicesBaseTests
	{
		private IExportService _uut;

		[SetUp]
		public void SetUp()
		{
			_uut = Container.Resolve<IExportService>();
		}

		[Test]
		public void ShouldThrowServiceExceptionOnAllEndpointsWhenCommunicationModeTurnsKeplerServicesOff()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));

			FluentActions.Invoking(() =>
					_uut.InitializeFolderExportAsync(Any.Integer(), Any.Integer(), Any.Integer(), Any.Boolean(),
						Any.Array<int>(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.HasExportPermissionsAsync(Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.InitializeProductionExportAsync(Any.Integer(), Any.Integer(), Any.Array<int>(), Any.Integer(),
						Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.InitializeSearchExportAsync(Any.Integer(), Any.Integer(), Any.Array<int>(), Any.Integer(),
						Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.RetrieveResultsBlockForProductionStartingFromIndexAsync(Any.Integer(), Any.Guid(),
						Any.Integer(), Any.Array<int>(), Any.Integer(), Any.Boolean(), Any.Char(), Any.Char(),
						Any.Array<int>(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.RetrieveResultsBlockForProductionStartingFromIndexAsync(Any.Integer(), Any.Guid(),
						Any.Integer(), Any.Array<int>(), Any.Integer(), Any.Boolean(), Any.Char(), Any.Char(),
						Any.Array<int>(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");
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
					_uut.InitializeFolderExportAsync(Any.Integer(), Any.Integer(), Any.Integer(), Any.Boolean(),
						Any.Array<int>(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.HasExportPermissionsAsync(Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.InitializeProductionExportAsync(Any.Integer(), Any.Integer(), Any.Array<int>(), Any.Integer(),
						Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.InitializeSearchExportAsync(Any.Integer(), Any.Integer(), Any.Array<int>(), Any.Integer(),
						Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.RetrieveResultsBlockForProductionStartingFromIndexAsync(Any.Integer(), Any.Guid(),
						Any.Integer(), Any.Array<int>(), Any.Integer(), Any.Boolean(), Any.Char(), Any.Char(),
						Any.Array<int>(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.RetrieveResultsBlockForProductionStartingFromIndexAsync(Any.Integer(), Any.Guid(),
						Any.Integer(), Any.Array<int>(), Any.Integer(), Any.Boolean(), Any.Char(), Any.Char(),
						Any.Array<int>(), Any.Integer(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");
		}
	}
}
