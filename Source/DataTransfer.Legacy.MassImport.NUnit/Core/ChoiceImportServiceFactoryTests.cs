using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Data.MassImport;
using Relativity.MassImport.Core;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.Choices;
using Relativity.MassImport.Toggles;
using Relativity.Toggles;

namespace Relativity.MassImport.NUnit.Core
{
	[TestFixture]
	public class ChoiceImportServiceFactoryTests
	{
		private ChoiceImportServiceFactory _sut;

		private Mock<IToggleProvider> _toggleProviderMock;
		private MassImportContext _context;
		private NativeLoadInfo _settings;
		private IColumnDefinitionCache _columnDefinitionCache;

		[SetUp]
		public void SetUp()
		{
			var config = new Dictionary<string, object>
			{
				["MassImportSqlTimeout"] = 1
			};
			Relativity.Data.Config.InjectConfigSettings(config);

			_toggleProviderMock = new Mock<IToggleProvider>();

			_settings = null; // value not needed
			_columnDefinitionCache = null; // value not needed
			LoggingContext loggingContext = null; // value not needed
			int caseSystemArtifactId = 0; // value not needed

			var tableNames = new TableNames();
			var jobDetails = new MassImportJobDetails(tableNames, "TestSystem", "TestImport");
			var baseContextMock = new Mock<BaseContext>
			{
				DefaultValue = DefaultValue.Mock
			};
			_context = new MassImportContext(baseContextMock.Object, loggingContext, jobDetails, caseSystemArtifactId);
			
			_sut = new ChoiceImportServiceFactory(_toggleProviderMock.Object);
		}

		[Test]
		public void ShouldReturnNewImplementationWhenToggleIsEnabled()
		{
			// arrange
			_toggleProviderMock
				.Setup(x => x.IsEnabled<UseNewChoicesQueryToggle>())
				.Returns(true);

			// act
			var actual = _sut.Create(_context, _settings, _columnDefinitionCache);

			// assert
			Assert.That(actual, Is.InstanceOf<ChoicesImportService>());
		}

		[Test]
		public void ShouldReturnOldImplementationWhenToggleIsDisabled()
		{
			// arrange
			_toggleProviderMock
				.Setup(x => x.IsEnabled<UseNewChoicesQueryToggle>())
				.Returns(false);

			// act
			var actual = _sut.Create(_context, _settings, _columnDefinitionCache);

			// assert
			Assert.That(actual, Is.InstanceOf<OldChoicesImportService>());
		}
	}
}
