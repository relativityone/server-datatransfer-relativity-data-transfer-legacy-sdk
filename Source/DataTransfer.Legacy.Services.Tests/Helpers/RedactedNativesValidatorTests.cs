using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Toggles;
using Relativity.Services.Exceptions;
using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Helpers
{
	[TestFixture]
	public class RedactedNativesValidatorTests
	{
		private const int WorkspaceID = 9231;
		private const int ProductionID = 231231;

		private Mock<IToggleProvider> _toggleProviderMock;
		private Mock<IProductionExternalService> _productionExternalServiceMock;
		private CancellationToken _cancellationToken;

		private IRedactedNativesValidator _sut;

		[SetUp]
		public void SetUp()
		{
			_toggleProviderMock = new Mock<IToggleProvider>();
			_productionExternalServiceMock = new Mock<IProductionExternalService>();

			_cancellationToken = new CancellationToken(canceled: false);

			_sut = new RedactedNativesValidator(
				Mock.Of<IAPILog>(),
				_toggleProviderMock.Object,
				() => _productionExternalServiceMock.Object);
		}

		[Test]
		public async Task VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync_Returns_WhenToggleDisabled()
		{
			// arrange 
			SetUpToggle(isEnabled: false);
			HasRedactedNativesEnabledSetup.ReturnsAsync(true);

			// act
			await _sut.VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync(WorkspaceID, ProductionID,
				_cancellationToken);

			// assert
			Assert.Pass("It shouldn't throw exception.");
		}

		[Test]
		public async Task VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync_Returns_WhenProductionWithoutRedactedNatives()
		{
			// arrange 
			SetUpToggle(isEnabled: true);
			HasRedactedNativesEnabledSetup.ReturnsAsync(false);

			// act
			await _sut.VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync(WorkspaceID, ProductionID,
				_cancellationToken);

			// assert
			Assert.Pass("It shouldn't throw exception.");
		}

		[Test]
		public async Task VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync_Returns_WhenCheckingProductionFails()
		{
			// arrange 
			SetUpToggle(isEnabled: true);
			HasRedactedNativesEnabledSetup.Throws<Exception>();

			// act
			await _sut.VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync(WorkspaceID, ProductionID,
				_cancellationToken);

			// assert
			Assert.Pass("It shouldn't throw exception.");
		}

		[Test]
		public async Task VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync_ThrowsNotFoundException_WhenProductionWithRedactedNatives()
		{
			// arrange 
			SetUpToggle(isEnabled: true);
			HasRedactedNativesEnabledSetup.ReturnsAsync(true);

			// act
			Func<Task> verifyAction = () => _sut.VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync(
				WorkspaceID, 
				ProductionID,
				_cancellationToken);

			// assert
			await verifyAction.Should().ThrowAsync<NotFoundException>();
		}

		private void SetUpToggle(bool isEnabled)
		{
			_toggleProviderMock
				.Setup(x => x.IsEnabledAsync<EnabledBlockingRedactedNativesExportForProduction>())
				.ReturnsAsync(isEnabled);
		}

		private ISetup<IProductionExternalService, Task<bool>> HasRedactedNativesEnabledSetup =>
			_productionExternalServiceMock.Setup(
				x => x.HasRedactedNativesEnabledAsync(
					WorkspaceID,
					ProductionID,
					_cancellationToken));
	}
}
