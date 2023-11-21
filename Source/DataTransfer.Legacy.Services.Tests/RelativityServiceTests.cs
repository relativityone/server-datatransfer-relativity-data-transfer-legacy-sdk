using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	using System;
	using Moq;
	using Relativity.DataTransfer.Legacy.Services.Toggles;

	[TestFixture]
	public class RelativityServiceTests : ServicesBaseTests
	{
		private IRelativityService _uut;

		[SetUp]
		public void SetUp()
		{
			_uut = Container.Resolve<IRelativityService>();
		}

		[Test]
		public void ShouldThrowServiceExceptionOnAllEndpointsWhenCommunicationModeTurnsKeplerServicesOff()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
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

			FluentActions.Invoking(() =>
					_uut.RetrieveCurrencySymbolV2Async(Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			// GetImportExportWebApiVersionAsync, ValidateSuccessfulLoginAsync, GetRelativityUrlAsync omitted as they are marked as obsolete
		}

		[Test]
		public void RetrieveCurrencySymbolAsync_ShouldThrowNotFoundException_WhenDisableRdcAndImportApiToggleIsOn()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceKepler)));

			ToggleProviderMock.Setup(x => x.IsEnabledAsync<DisableRdcAndImportApiToggle>())
				.ReturnsAsync(true);

			FluentActions.Invoking(() =>
					_uut.RetrieveCurrencySymbolAsync(Any.String()))
				.Should().Throw<NotFoundException>()
				.WithMessage(Constants.ErrorMessages.RdcDeprecatedDisplayMessage);
		}

		[Test]
        public void RetrieveCurrencySymbolAsync_ShouldReturnCurrencySymbol_WhenDisableRdcAndImportApiToggleIsOff()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceKepler)));

			ToggleProviderMock.Setup(x => x.IsEnabledAsync<DisableRdcAndImportApiToggle>())
				.ReturnsAsync(false);

			FluentActions.Invoking(() =>
					_uut.RetrieveCurrencySymbolAsync(Any.String()))
				.Should().NotThrow<Exception>();
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task RetrieveCurrencySymbolV2Async_ShouldReturnCurrencySymbol(bool toggleValue)
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceKepler)));

			ToggleProviderMock.Setup(x => x.IsEnabledAsync<DisableRdcAndImportApiToggle>())
				.ReturnsAsync(toggleValue);

			var result = await _uut.RetrieveCurrencySymbolV2Async(Any.String());
			result.Should().NotBeEmpty();
		}
	}
}
