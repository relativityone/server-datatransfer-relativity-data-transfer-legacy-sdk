using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.MassImport;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.Choices;

namespace MassImport.NUnit.Integration.Data.Choices
{
	[TestFixture]
	internal class ChoicesImportServiceTests : ChoicesImportServiceTestBase
	{
		private Mock<IColumnDefinitionCache> _columnDefinitionCacheMock;

		[SetUp]
		public void SetUp()
		{
			_columnDefinitionCacheMock = new Mock<IColumnDefinitionCache>();
		}

		private protected override IChoicesImportService CreateSut(Relativity.MassImport.DTO.NativeLoadInfo settings)
		{
			return new ChoicesImportService(
				this.EddsdboContext,
				ToggleProviderMock.Object,
				TableNames,
				ImportMeasurements,
				settings,
				_columnDefinitionCacheMock.Object,
				QueryTimeoutInSeconds);
		}

		protected override Task SetOverlayBehaviorForFieldAsync(FieldInfo choiceField, Relativity.MassImport.DTO.OverlayBehavior overlayBehavior)
		{
			var columnDefinitionInfo = new ColumnDefinitionInfo
			{
				OverlayMergeValues = overlayBehavior == Relativity.MassImport.DTO.OverlayBehavior.MergeAll
			};

			_columnDefinitionCacheMock.Setup(x => x[choiceField.ArtifactID]).Returns(columnDefinitionInfo);
			return Task.CompletedTask;
		}
	}
}
