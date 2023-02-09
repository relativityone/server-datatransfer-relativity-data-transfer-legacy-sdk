using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;
using Relativity.MassImport.Api;
using MassImportManager = Relativity.MassImport.Api.MassImportManager;
using Moq;
using Relativity.API;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
	public class CollationEdgeCasesTests : MassImportTestBase
	{
		private int _rootFolderId;
		private MassImportField _identifierField;

		private IMassImportManager _sut;

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			_rootFolderId = await FolderHelper.ReadRootFolderIdAsync(TestParameters, TestWorkspace).ConfigureAwait(false);
			_identifierField = await Helpers.FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace).ConfigureAwait(false);
		}

		[SetUp]
		public void SetUp()
		{
			var artifactManager = new ArtifactManager();
			var baseContext = CoreContext.ChicagoContext;

			_sut = new MassImportManager(AssemblySetup.TestLogger, artifactManager, baseContext, new Mock<IHelper>().Object);
		}

		[Test]
		public async Task Import_ThrowsException_WhenControlNumbersAreDuplicated()
		{
			// arrange
			var settings = new MassImportSettings
			{
				ArtifactTypeID = (int)ArtifactType.Document,
				Overlay = OverwriteType.Append,
				MappedFields = new FieldInfo[]
				{
					_identifierField,
				},
			};

			MassImportArtifact CreateMassImportArtifact(string identifier) =>
				new MassImportArtifact(new List<object> { identifier }, parentFolderId: _rootFolderId);

			MassImportArtifact[] recordsToImport =
			{
				CreateMassImportArtifact("Somestring"),
				CreateMassImportArtifact("some😀string😀"),
			};

			// act
			var result = await _sut.RunMassImportAsync(recordsToImport, settings, CancellationToken.None, null).ConfigureAwait(false);

			// assert
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(2));
			Assert.That(result.ArtifactsCreated, Is.EqualTo(0));
			Assert.That(result.ExceptionDetail, Is.Not.Null);
			Assert.That(result.ExceptionDetail.ExceptionMessage, Contains.Substring("Cannot insert duplicate key row in object 'EDDSDBO.Document'"));
		}
	}
}
