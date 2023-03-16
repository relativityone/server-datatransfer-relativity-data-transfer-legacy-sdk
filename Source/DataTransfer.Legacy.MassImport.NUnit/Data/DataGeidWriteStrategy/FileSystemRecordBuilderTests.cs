using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Utility.Streaming;
using Moq;
using NUnit.Framework;
using Relativity.DataGrid;
using Relativity.MassImport.Data.DataGridWriteStrategy;

namespace Relativity.MassImport.NUnit.Data.DataGeidWriteStrategy
{
	[TestFixture]
	public class FileSystemRecordBuilderTests
	{
		private string _testFilePath;
		private const string _FIELD1_VALUE = "string data";
		private const string _FIELD2_VALUE = "stream data";
		private const int _ARTIFACTID_1 = 4136120;
		private const int _WRITE_PARALLELISM = 4;
		private const int _SHORT_FIELD_LENGTH_LIMIT = 16384; // 16KB

		private DataGridFieldInfo _field;
		private Mock<IDataGridWriter> _mockWriter;
		private FileSystemRecordBuilder _sut;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			string directoryPath = Path.GetDirectoryName(path);
			_testFilePath = $@"{directoryPath}\Resources\{"DataGridBulkLoadTestFile.txt"}";
		}

		[SetUp]
		public void SetUp()
		{
			_field = new DataGridFieldInfo(0, "Fields", "Field1", string.Empty);
			_mockWriter = new Mock<IDataGridWriter>();
			_sut = new FileSystemRecordBuilder(_mockWriter.Object, _SHORT_FIELD_LENGTH_LIMIT, _WRITE_PARALLELISM);
		}

		[Test]
		public async Task SingleDocument_ThreeFields_Flush_CheckRecords()
		{
			var actualRecords = new List<IDataGridRecord>();
			_mockWriter.Setup(w => w.Write(It.IsAny<IEnumerable<IDataGridRecord>>()))
				.Callback<IEnumerable<IDataGridRecord>>(records =>
				{
					lock (actualRecords) actualRecords.AddRange(records);
				})
				.Returns(() => Task.FromResult(Enumerable.Empty<DataGridWriteResult>()));

			Stream textStream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(_FIELD2_VALUE));
			string batchID = "01234567-89ab-cdef-0123-456789abcdef";

			await _sut.AddDocument(_ARTIFACTID_1, "document", batchID);
			await _sut.AddField(new DataGridFieldInfo(0, "Fields", "Field1", string.Empty), _FIELD1_VALUE, false);
			await _sut.AddField(new DataGridFieldInfo(0, "Fields", "Field2", string.Empty), textStream);
			await _sut.AddField(new DataGridFieldInfo(0, "Fields", "Field3", string.Empty), _testFilePath, true);
			await _sut.Flush();

			Assert.AreEqual(3, actualRecords.Count);
			Assert.IsInstanceOf<DataGridRecord>(actualRecords.ElementAt(0));
			Assert.IsInstanceOf<DataGridRecord>(actualRecords.ElementAt(1));
			Assert.IsInstanceOf<DataGridRecord>(actualRecords.ElementAt(2));
			var resultRecords = actualRecords.OfType<DataGridRecord>().ToArray();

			foreach (DataGridRecord resultRecord in resultRecords)
			{
				Assert.AreEqual(_ARTIFACTID_1, resultRecord.ArtifactID);
				Assert.AreEqual(1, resultRecord.Namespaces.Count);
				Assert.AreEqual("Fields", resultRecord.Namespaces.Keys.Single());
				Assert.AreEqual(1, resultRecord["Fields"].Count);
				switch (resultRecord["Fields"].First().Key ?? "")
				{
					case "Field1":
						{
							Assert.AreEqual(_FIELD1_VALUE, resultRecord["Fields"]["Field1"].GetValue<string>());
							Assert.AreEqual(22, resultRecord["Fields"]["Field1"].ByteSize);
							break;
						}

					case "Field2":
						{
							Assert.AreEqual(1, resultRecord["Fields"].Count);
							Assert.AreEqual(_FIELD2_VALUE, resultRecord["Fields"]["Field2"].GetValue<string>());
							break;
						}

					case "FIeld3":
						{
							Assert.AreEqual(true, resultRecord["Fields"]["Field3"].IsValueAFileLink);
							Assert.AreEqual(_testFilePath, resultRecord["Fields"]["Field3"].GetValue<string>());
							break;
						}
				}
			}
		}

		[Test]
		public async Task NoDocuments_Flush_NoWrites()
		{
			string batchID = "01234567-89ab-cdef-0123-456789abcdef";

			await _sut.AddDocument(_ARTIFACTID_1, "document", batchID);
			await _sut.Flush();

			_mockWriter.Verify(w => w.Write(It.IsAny<IEnumerable<IDataGridRecord>>()), Times.Never());
		}

		[Test]
		public async Task OneDocument_NoFields_Flush_NoWrites()
		{
			await _sut.Flush();

			_mockWriter.Verify(w => w.Write(It.IsAny<IEnumerable<IDataGridRecord>>()), Times.Never());
		}

		[TestCase(2, true)]
		[TestCase(10, true)]
		[TestCase(21, true)]
		[TestCase(22, true)]
		[TestCase(23, false)]
		[TestCase(24, false)]
		[TestCase(25, false)]
		[TestCase(8192, false)]
		public async Task AddField_StreamimgThreshold_CheckResults(int shortFieldLengthLimit, bool shouldStream)
		{
			const string delimiter = "|END";
			string delimitedField2Value = $"{_FIELD2_VALUE}{delimiter}";
			Stream textStream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(delimitedField2Value));
			var delimitedStream = new DelimitedReadStreamDecorator(textStream, System.Text.Encoding.Unicode.GetBytes(delimiter));
			var preamble = System.Text.Encoding.Unicode.GetPreamble();

			_mockWriter.Setup(w => w.Write(It.IsAny<IEnumerable<IDataGridRecord>>())).Callback<IEnumerable<IDataGridRecord>>(records =>
			{
				var resultRecords = records.OfType<DataGridRecord>().ToArray();
				Assert.AreEqual(1, resultRecords.Count());
				if (shouldStream)
				{
					Assert.IsInstanceOf<Stream>(resultRecords[0]["Fields"]["Field2"].Value);
					Stream stream = (Stream)resultRecords[0]["Fields"]["Field2"].Value;
					var firstBytes = new byte[preamble.Length - 1 + 1];
					stream.Read(firstBytes, 0, preamble.Length);
					Assert.AreEqual(preamble, firstBytes, "Expected field stream to begin with the UTF-16 preamble");
					using (var reader = new StreamReader(stream, System.Text.Encoding.Unicode))
					{
						Assert.AreEqual(_FIELD2_VALUE, reader.ReadToEnd());
					}
				}
				else
				{
					Assert.IsInstanceOf<string>(resultRecords[0]["Fields"]["Field2"].Value);
					Assert.AreEqual(_FIELD2_VALUE, resultRecords[0]["Fields"]["Field2"].GetValue<string>());
				}
			}).Returns(() => Task.FromResult(Enumerable.Empty<DataGridWriteResult>()));
			var recordBuilder = new FileSystemRecordBuilder(_mockWriter.Object, shortFieldLengthLimit, _WRITE_PARALLELISM);
			string batchID = "01234567-89ab-cdef-0123-456789abcdef";

			await recordBuilder.AddDocument(_ARTIFACTID_1, "document", batchID);
			await recordBuilder.AddField(new DataGridFieldInfo(0, "Fields", "Field2", string.Empty), delimitedStream);
			await recordBuilder.Flush();

			_mockWriter.Verify(w => w.Write(It.IsAny<IEnumerable<IDataGridRecord>>()), Times.Once());
		}

		[Test]
		public async Task AddDocument_IndexerDoesNotAddDocument()
		{
			var mockIndexer = new Mock<IDataGridRecordBuilder>();

			string batchID = "01234567-89ab-cdef-0123-456789abcdef";

			await _sut.AddDocument(_ARTIFACTID_1, "document", batchID);
			await _sut.Flush();

			mockIndexer.Verify(i => i.AddDocument(_ARTIFACTID_1, "document", batchID), Times.Never());
		}

		[Test]
		public async Task Flush_FlushesIndexer()
		{
			var mockIndexer = new Mock<IDataGridRecordBuilder>();

			await _sut.Flush();

			mockIndexer.Verify(i => i.Flush(), Times.Never());
		}

		[Test]
		public async Task AddField_String_Error_IndexerDoesNotAddField()
		{
			var exampleWriteResult = new DataGridWriteResult()
			{
				ArtifactID = _ARTIFACTID_1,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					new DataGridWriteResult.FieldResult()
					{
						FieldIdentifier = "Field1",
						FieldNamespace = "Fields",
						ResultStatus = DataGridResult.Status.Error,
						ResultsErrorMessage = "a problem, very bad indeed"
					}
				}
			};
			_mockWriter.Setup(w => w.Write(It.IsAny<IEnumerable<IDataGridRecord>>()))
				.Returns(Task.FromResult<IEnumerable<DataGridWriteResult>>(new[] { exampleWriteResult }));

			var mockIndexer = new Mock<IDataGridRecordBuilder>();

			string batchID = "01234567-89ab-cdef-0123-456789abcdef";

			await _sut.AddDocument(_ARTIFACTID_1, "document", batchID);
			await _sut.AddField(new DataGridFieldInfo(0, "Fields", "Field1", string.Empty), _FIELD1_VALUE, false);
			await _sut.Flush();

			mockIndexer.Verify(i => i.AddDocument(_ARTIFACTID_1, "document", batchID), Times.Never());
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task AddField_IncrementsNumberOfEmptyTexts(bool isFileLink)
		{
			// act
			await _sut.AddField(_field, string.Empty, isFileLink);
			await _sut.AddField(_field, null, isFileLink);

			// assert
			_sut.NumberOfEmptyTexts.Should().Be(2);
			_sut.NumberOfTexts.Should().Be(2);
		}

		[Test]
		public async Task AddField_IncrementsNumberOfEmptyTexts_ForEmptyStream()
		{
			// act
			await _sut.AddField(_field, new MemoryStream());

			// assert
			_sut.NumberOfEmptyTexts.Should().Be(1);
			_sut.NumberOfTexts.Should().Be(1);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task AddField_IncrementsNumberOfTexts(bool isFileLink)
		{
			// arrange
			await _sut.AddField(_field, string.Empty, isFileLink);

			// act
			await _sut.AddField(_field, "Test", isFileLink);

			// assert
			_sut.NumberOfEmptyTexts.Should().Be(1);
			_sut.NumberOfTexts.Should().Be(2);
		}

		[Test]
		public async Task AddField_IncrementsNumberOfTexts_ForNonEmptyStream()
		{
			// act
			await _sut.AddField(_field, new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }));

			// assert
			_sut.NumberOfEmptyTexts.Should().Be(0);
			_sut.NumberOfTexts.Should().Be(1);
		}

		[Test]
		public async Task AddField_IncrementsNumberOfTextsAndEmptyTexts_ForMultipleThreads()
		{
			// arrange
			const int numberOfThreads = 8;
			const int numberOfEmptyFieldsPerThreads = 1000;

			async Task Increment()
			{
				for (int i = 0; i < numberOfEmptyFieldsPerThreads; i++)
				{
					await _sut.AddField(_field, null, isFileLink: true);
				}
			}

			// act
			var incrementTasks = Enumerable.Range(0, numberOfThreads).Select(_ => Increment());
			await Task.WhenAll(incrementTasks);

			// assert
			int expectedNumberOfEmptyFiles = numberOfThreads * numberOfEmptyFieldsPerThreads;
			_sut.NumberOfEmptyTexts.Should().Be(expectedNumberOfEmptyFiles);
			_sut.NumberOfTexts.Should().Be(expectedNumberOfEmptyFiles);
		}
	}
}