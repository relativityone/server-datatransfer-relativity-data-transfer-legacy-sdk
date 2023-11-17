using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V2;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	using Moq;
	using Relativity.DataTransfer.Legacy.Services.Toggles;

	[TestFixture]
	public class RelativityServiceV2Tests : ServicesBaseTests
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
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task RetrieveCurrencySymbolAsync_Return_RegardlessOfDisableRdcAndIApiToggleValue(bool toggleValue)
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceKepler)));

			ToggleProviderMock.Setup(x => x.IsEnabledAsync<DisableRdcAndImportApiToggle>())
				.ReturnsAsync(toggleValue);

			var result = await _uut.RetrieveCurrencySymbolAsync(Any.String());
			result.Should().NotBeEmpty();
		}
	}
}
