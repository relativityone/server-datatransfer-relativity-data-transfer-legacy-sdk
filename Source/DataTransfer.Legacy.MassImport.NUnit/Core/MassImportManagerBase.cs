using Moq;
using NUnit.Framework;
using Relativity.Core;

namespace Relativity.MassImport.NUnit.Core
{
	[TestFixture()]
	public class MassImportManagerBase
	{
		private Mock<ICoreContext> _mockContext;
		private Mock<BaseContext> _mockChicagoContext;
		private Relativity.Core.Service.MassImportManagerBase.MassImportResults _massImportResults;
		private Relativity.MassImport.DTO.ImageLoadInfo _settings;
		private const string IMAGE_IMPORT_EXCEPTION_MESSAGE = "AttemptRunImageImport Failed";
		private const string PRODUCTION_IMAGE_IMPORT_EXCEPTION_MESSAGE = "AttemptRunProductionImageImport Failed";
		private const string NATIVE_IMPORT_EXCEPTION_MESSAGE = "AttemptRunNativeImport Failed";
		private const string OBJECT_IMPORT_EXCEPTION_MESSAGE = "AttemptRunObjectImport Failed";
		private const string API_OBJECT_IMPORT_EXCEPTION_MESSAGE = "ExecuteObjectImport Failed";

		[SetUp()]
		public void Setup()
		{
			_mockContext = new Mock<ICoreContext>();
			_mockChicagoContext = new Mock<BaseContext>();
			_mockContext
				.Setup(context => context.ChicagoContext)
				.Returns(_mockChicagoContext.Object);

			_massImportResults = new Relativity.Core.Service.MassImportManagerBase.MassImportResults();
			_settings = new Relativity.MassImport.DTO.ImageLoadInfo()
			{
				BulkFileName = "",
				UploadFullText = false,
				Overlay = MassImport.OverwriteType.Both,
				DestinationFolderArtifactID = 0,
				Repository = "",
				UseBulkDataImport = false,
				RunID = "0",
				KeyFieldArtifactID = 0
			};
		}

		[Test()]
		public void Mass_Image_Import_Exception_Check_Message_String()
		{
			var testMassImportManager = new MockMassImportManager();
			_massImportResults = testMassImportManager.RunImageImport(_mockContext.Object, _settings, false);
			Assert.IsTrue(_massImportResults.ExceptionDetail.ExceptionFullText.Contains(IMAGE_IMPORT_EXCEPTION_MESSAGE));
		}

		[Test()]
		public void Mass_Production_Image_Import_Exception_Check_Message_String()
		{
			var testMassImportManager = new MockMassImportManager();
			_massImportResults = testMassImportManager.RunProductionImageImport(_mockContext.Object, _settings, 0, false);
			Assert.IsTrue(_massImportResults.ExceptionDetail.ExceptionFullText.Contains(PRODUCTION_IMAGE_IMPORT_EXCEPTION_MESSAGE));
		}

		[Test()]
		public void Mass_Native_Import_Exception_Check_Message_String()
		{
			var testMassImportManager = new MockMassImportManager();
			_massImportResults = testMassImportManager.RunNativeImport(_mockContext.Object, default, false, false);
			Assert.IsTrue(_massImportResults.ExceptionDetail.ExceptionFullText.Contains(NATIVE_IMPORT_EXCEPTION_MESSAGE));
		}

		[Test()]
		public void Mass_Object_Import_Exception_Check_Message_String()
		{
			var testMassImportManager = new MockMassImportManager();
			_massImportResults = testMassImportManager.RunObjectImport(_mockContext.Object, default, false, string.Empty);
			Assert.IsTrue(_massImportResults.ExceptionDetail.ExceptionFullText.Contains(OBJECT_IMPORT_EXCEPTION_MESSAGE));
		}

		private class MockMassImportManager : Relativity.Core.Service.MassImportManagerBase
		{
			public MockMassImportManager()
			{
				// empatee
			}

			protected override MassImportResults AttemptRunImageImport(BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, bool inRepository, string bulkFileSharePath, kCura.Utility.Timekeeper timekeeper, MassImportResults retval)
			{
				throw new System.Exception("AttemptRunImageImport Failed");
			}

			protected override MassImportResults AttemptRunNativeImport(BaseContext context, Relativity.MassImport.DTO.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string bulkFileSharePath, kCura.Utility.Timekeeper timekeeper, MassImportResults retval)
			{
				throw new System.Exception("AttemptRunNativeImport Failed");
			}

			protected override MassImportResults AttemptRunObjectImport(BaseContext context, Relativity.MassImport.DTO.ObjectLoadInfo settings, bool inRepository, string bulkFileSharePath, MassImportResults retval)
			{
				throw new System.Exception("AttemptRunObjectImport Failed");
			}
			
			protected override MassImportResults AttemptRunProductionImageImport(BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, int productionArtifactID, bool inRepository, string bulkFileSharePath, MassImportResults retval)
			{
				throw new System.Exception("AttemptRunProductionImageImport Failed");
			}
		}
	}
}