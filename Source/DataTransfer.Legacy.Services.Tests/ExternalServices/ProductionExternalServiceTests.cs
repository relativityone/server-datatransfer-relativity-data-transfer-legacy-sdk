using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;
using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.Kepler.Exceptions;
using Relativity.Productions.Services.Interfaces.V2.DTOs;
using Relativity.Productions.Services.V2;

namespace Relativity.DataTransfer.Legacy.Services.Tests.ExternalServices
{
	[TestFixture]
	public class ProductionExternalServiceTests
	{
		private const int WorkspaceID = 9231;
		private const int ProductionID = 231231;

		private Mock<IProductionManager> _productionManagerMock;
		private CancellationToken _cancellationToken;

		private ProductionExternalService _sut;

		[SetUp]
		public void SetUp()
		{
			var loggerMock = Mock.Of<IAPILog>();
			_productionManagerMock = new Mock<IProductionManager>();
			_cancellationToken = new CancellationToken(canceled: false);

			_sut = new ProductionExternalService(
				loggerMock,
				new RetryableKeplerErrorsRetryPolicyFactory(loggerMock, maxNumberOfRetries: 0, waitTimeBaseInSeconds: 0),
				_productionManagerMock.Object);
		}

		[Test]
		public async Task HasRedactedNativesEnabledAsync_ReturnsFalse_WhenProductionWithoutDataSource()
		{
			// arrange
			var production = new Productions.Services.Interfaces.V2.DTOs.Production
			{
				DataSources = new List<ProductionDataSource>(),
			};
			ReadSingleProductionSetup.ReturnsAsync(production);

			// act
			var result = await _sut.HasRedactedNativesEnabledAsync(WorkspaceID, ProductionID, _cancellationToken);

			// assert
			result.Should().BeFalse("Because production does not have data sources");
		}

		[Test]
		public async Task HasRedactedNativesEnabledAsync_ReturnsFalse_WhenProductionHasDataSourcesWithoutRedactedNatives()
		{
			// arrange
			var production = new Productions.Services.Interfaces.V2.DTOs.Production
			{
				DataSources = new List<ProductionDataSource>
				{
					new ProductionDataSource{BurnNativeRedactions = false},
					new ProductionDataSource{BurnNativeRedactions = false},
					new ProductionDataSource{BurnNativeRedactions = false},
					new ProductionDataSource{BurnNativeRedactions = false},
				},
			};
			ReadSingleProductionSetup.ReturnsAsync(production);

			// act
			var result = await _sut.HasRedactedNativesEnabledAsync(WorkspaceID, ProductionID, _cancellationToken);

			// assert
			result.Should().BeFalse("Because data sources does not have redacted natives");
		}

		[Test]
		public async Task HasRedactedNativesEnabledAsync_ReturnsTrue_WhenProductionHasDataSourceWithRedactedNatives()
		{
			// arrange
			var production = new Productions.Services.Interfaces.V2.DTOs.Production
			{
				DataSources = new List<ProductionDataSource>
				{
					new ProductionDataSource{BurnNativeRedactions = false},
					new ProductionDataSource{BurnNativeRedactions = false},
					new ProductionDataSource{BurnNativeRedactions = true},
					new ProductionDataSource{BurnNativeRedactions = false},
				},
			};
			ReadSingleProductionSetup.ReturnsAsync(production);

			// act
			var result = await _sut.HasRedactedNativesEnabledAsync(WorkspaceID, ProductionID, _cancellationToken);

			// assert
			result.Should().BeTrue("Because one data source has redacted natives");
		}

		[Test]
		public async Task HasRedactedNativesEnabledAsync_Throws_WhenReadingProductionThrows()
		{
			// arrange
			ReadSingleProductionSetup.Throws<ServiceNotFoundException>();

			// act
			Func<Task> call = () => _sut.HasRedactedNativesEnabledAsync(WorkspaceID, ProductionID, _cancellationToken);

			// assert
			await call.Should().ThrowAsync<ServiceNotFoundException>("Because reading production failed");
		}

		private ISetup<IProductionManager, Task<Productions.Services.Interfaces.V2.DTOs.Production>> ReadSingleProductionSetup =>
			_productionManagerMock.Setup(x => x.ReadSingleAsync(WorkspaceID, ProductionID, 1));
	}
}
