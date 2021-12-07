using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Data;
using Relativity.Data.MassImportOld;
using Relativity.DataGrid;
using Relativity.Logging;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class DataGridReaderTests
	{
		private string _testFilePath;
		private string _testFileAlternateDelimiterPath;
		private string _testFileWithDataGridIDPath;
		private string _testFileWithDataGridIDNoFieldsPath;
		private string _testFileWithRDCDataGridTempBulkFile;

		private Mock<DataGridContext> _dgLookupContextMock;
		private Mock<kCura.Data.RowDataGateway.BaseContext> _caseContextMock;

		private IToggleProvider _originalToggleProvider;

		[SetUp()]
		public void Setup()
		{
			_originalToggleProvider = ToggleProvider.Current;
			ToggleProvider.Current = new InMemoryToggleProvider();
			string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			string directoryPath = System.IO.Path.GetDirectoryName(path);
			string tempResourcesPath = string.Format(@"{0}\Resources", directoryPath);

			_testFilePath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile.txt");
			_testFileAlternateDelimiterPath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile_AlternateDelimiter.txt");
			_testFileWithDataGridIDPath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile_WithDataGridID.txt");
			_testFileWithDataGridIDNoFieldsPath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile_WithDataGridID_NoFields.txt");
			_testFileWithRDCDataGridTempBulkFile = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridRDCBulkLoadFile.txt");

			_dgLookupContextMock = new Mock<DataGridContext>(new Mock<DataGridContextBase>().Object);		

			var mapperMock = new Mock<DataGridMappingLookupManager>();
			_caseContextMock = new Mock<kCura.Data.RowDataGateway.BaseContext>();
			var field1 = new DataGridFieldInfo(1001, "Fields", "field name 1", null);
			var field2 = new DataGridFieldInfo(1002, "Fields", "field #2", null);
			var field3 = new DataGridFieldInfo(1003, "Fields", "three", null);

			mapperMock.Setup(x => x.GetDataGridFieldInfo(1001, _caseContextMock.Object)).Returns(field1);
			mapperMock.Setup(x => x.GetDataGridFieldInfo(1002, _caseContextMock.Object)).Returns(field2);
			mapperMock.Setup(x => x.GetDataGridFieldInfo(1003, _caseContextMock.Object)).Returns(field3);

			_dgLookupContextMock.Setup(x => x.DataGridMappingLookupManager).Returns(mapperMock.Object);
		}

		[TearDown()]
		public void TearDown()
		{
			ToggleProvider.Current = _originalToggleProvider;
		}

		private DataGridMappingMultiDictionary GetMappingsFull()
		{
			var identity1 = new DataGridImportIdentity()
			{
				ImportID = 1,
				DocumentIdentifier = "DOCAAA1",
				ArtifactID = 1,
			};
			var identity2 = new DataGridImportIdentity()
			{
				ImportID = 2,
				DocumentIdentifier = "DOCAAA2",
				ArtifactID = 2,
			};
			var identity3 = new DataGridImportIdentity()
			{
				ImportID = 3,
				DocumentIdentifier = "DOCAAA3",
				ArtifactID = 3,
			};
			var identities = new List<DataGridImportIdentity>() { identity1, identity2, identity3 };

			var mappings = new DataGridMappingMultiDictionary();
			mappings.LoadCacheForImport(identities);
			return mappings;
		}

		private DataGridMappingMultiDictionary GetMappingsMissingOneRecord()
		{
			var identity1 = new DataGridImportIdentity()
			{
				ImportID = 1,
				DocumentIdentifier = "DOCAAA1",
				ArtifactID = 1,
			};
			var identity3 = new DataGridImportIdentity()
			{
				ImportID = 2,
				DocumentIdentifier = "DOCAAA3",
				ArtifactID = 2,
			};
			var identities = new List<DataGridImportIdentity>() { identity1, identity3 };

			var mappings = new DataGridMappingMultiDictionary();
			mappings.LoadCacheForImport(identities);
			return mappings;
		}

		// TODO: ReadFullTextFromFileLocation true/false wrt field values

		[Test]
		public async Task ReadFile_HasMappedSqlFields_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo()
				{
					ArtifactID = 1001,
					EnableDataGrid = true,
					DisplayName = "field 1"
				},
				new FieldInfo()
				{
					ArtifactID = 1008,
					EnableDataGrid = false,
					DisplayName = "field 2"
				},
				new FieldInfo()
				{
					ArtifactID = 1002,
					EnableDataGrid = true,
					DisplayName = "field 3"
				},
				new FieldInfo()
				{
					ArtifactID = 1003,
					EnableDataGrid = true,
					DisplayName = "field 4"
				},
				new FieldInfo()
				{
					ArtifactID = 1009,
					EnableDataGrid = false,
					DisplayName = "field 5"
				},
				new FieldInfo()
				{
					ArtifactID = 1010,
					EnableDataGrid = false,
					DisplayName = "field 6"
				}
			};

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA2", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(3, "document", It.IsAny<string>()), Times.Once());
		}

		[Test]
		public async Task ReadFile_OnlyMappedSqlFields_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo() {ArtifactID = 1008, EnableDataGrid = false, DisplayName = "field 1"},
				new FieldInfo() {ArtifactID = 1009, EnableDataGrid = false, DisplayName = "field 2"},
				new FieldInfo() {ArtifactID = 1010, EnableDataGrid = false, DisplayName = "field 3"}
			};

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			CollectionAssert.AreEquivalent(Enumerable.Empty<string>(), foundIdentifiers);
			builder.Verify(b => b.AddDocument(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public async Task ReadFile_HasSingleField_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo() { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" }
			};

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};
			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA2", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(3, "document", It.IsAny<string>()), Times.Once());
		}

		[Test]
		public async Task ReadFile_HasNoFields_CheckResultDtos()
		{
			var mappedFields = Array.Empty<FieldInfo>();
			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			CollectionAssert.AreEquivalent(Enumerable.Empty<string>(), foundIdentifiers);
			builder.Verify(b => b.AddDocument(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public async Task ReadFile_MissingDocument_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo() {ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1"},
				new FieldInfo() {ArtifactID = 1002, EnableDataGrid = true, DisplayName = "field 2"},
				new FieldInfo() {ArtifactID = 1003, EnableDataGrid = true, DisplayName = "field 3"}
			};

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|", Relativity.Constants.ENDLINETERMSTRING, _testFileAlternateDelimiterPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsMissingOneRecord(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
		}

		[Test]
		public async Task ReadFile_WithDataGridID_LinkingDocument_HasSingleField_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo() { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" }
			};

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = true,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA2", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(3, "document", It.IsAny<string>()), Times.Once());
		}

		[Test]
		public async Task ReadFile_WithDataGridID_LinkingDocument_HasNoFields_DGFS_CheckResultDtos()
		{
			var mappedFields = Array.Empty<FieldInfo>();
			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = true,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDNoFieldsPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA2", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(3, "document", It.IsAny<string>()), Times.Once());
		}

		[Test]
		public async Task ReadFile_WithDataGridID_NotLinkingDocument_HasSingleField_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo() { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" }
			};
			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA2", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(3, "document", It.IsAny<string>()), Times.Once());
		}

		[Test]
		public async Task ReadFile_WithDataGridID_NotLinkingDocument_HasNoFields_CheckResultDtos()
		{
			var mappedFields = Array.Empty<FieldInfo>();
			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			CollectionAssert.AreEquivalent(Enumerable.Empty<string>(), foundIdentifiers);
			builder.Verify(b => b.AddDocument(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public async Task ReadFile_WithDataGridID_ReadFullTextFromFile_NotLinkingDocument_HasSingleField_CheckResultDtos()
		{
			var mappedFields = new[]
			{
				new FieldInfo() { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1", Category = FieldCategory.FullText }
			};

			var options = new DataGridReaderOptions()
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = true
			};

			var foundIdentifiers = new HashSet<string>();
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var loader = new DataGridReader(_dgLookupContextMock.Object, _caseContextMock.Object, options, reader, new NullLogger(), new List<FieldInfo>(), new Mock<IDataGridSqlTempReader>().Object);
			var builder = new Mock<IDataGridRecordBuilder>();

			await loader.ReadDataGridDocumentsFromDataReader(builder.Object, GetMappingsFull(), foundIdentifiers);

			var expectedIdentifiers = new List<string>() { "DOCAAA1", "DOCAAA2", "DOCAAA3" };
			CollectionAssert.AreEquivalent(expectedIdentifiers, foundIdentifiers);
			builder.Verify(b => b.AddDocument(1, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(2, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddDocument(3, "document", It.IsAny<string>()), Times.Once());
			builder.Verify(b => b.AddField(It.IsAny<DataGridFieldInfo>(), It.IsAny<string>(), true), Times.Exactly(3));
		}
	}
}