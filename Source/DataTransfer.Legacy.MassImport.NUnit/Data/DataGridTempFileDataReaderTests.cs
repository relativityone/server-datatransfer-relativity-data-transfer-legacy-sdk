using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq;
using NUnit.Framework;
using Relativity.Data.MassImportOld;
using Relativity.Logging;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class DataGridTempFileDataReaderTests
	{
		private string _testFilePath;
		private string _testFileWithDataGridIDPath;
		private string _testFileWithDataGridIDNoFieldsPath;
		private string _testFilePathDestructive;

		private IToggleProvider _originalMockValue;

		[SetUp]
		public void Setup()
		{
			_originalMockValue = ToggleProvider.Current;
			ToggleProvider.Current = new AlwaysDisabledToggleProvider();
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			string directoryPath = Path.GetDirectoryName(path);
			string tempResourcesPath = string.Format(@"{0}\Resources", directoryPath);

			_testFilePath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile.txt");
			_testFileWithDataGridIDPath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile_WithDataGridID.txt");
			_testFileWithDataGridIDNoFieldsPath = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFile_WithDataGridID_NoFields.txt");
			_testFilePathDestructive = string.Format(@"{0}\{1}", tempResourcesPath, "DataGridBulkLoadTestFileDestructive.txt");
		}

		[TearDown]
		public void TearDown()
		{
			ToggleProvider.Current = _originalMockValue;
		}

		private DataTable GetTestDataTable(DataGridReaderOptions options)
		{
			var resultsTable = new DataTable();
			resultsTable.Columns.Add(new DataColumn(options.IdentifierColumnName, typeof(string)));
			resultsTable.Columns.Add(new DataColumn(options.DataGridIDColumnName, typeof(string)));
			foreach (FieldInfo field in options.MappedDataGridFields)
			{
				resultsTable.Columns.Add(new DataColumn(field.GetColumnName(), typeof(string)));
			}
			return resultsTable;
		}

		private static IEnumerable<TestCaseData> GetFileLockData()
		{
			foreach (FileMode fileMode in Enum.GetValues(typeof(FileMode)).Cast<FileMode>())
			{
				foreach (FileMode fileAccess in Enum.GetValues(typeof(FileAccess)).Cast<FileAccess>())
				{
					foreach (FileMode fileShare in Enum.GetValues(typeof(FileShare)).Cast<FileShare>())
					{
						yield return new TestCaseData(fileMode, fileAccess, fileShare);
					}
				}
			}
		}

		[Explicit("It sleeps for 1 second")]
		[Test]
		public void ReadFile_HasSingleField_RetryOnce()
		{
			// Arrange
			var mockLogger = new Mock<ILog>();
			FileStream fs = null;
			mockLogger
				.Setup(mock => mock.LogWarning(
					It.IsAny<Exception>(),
					"ReadDataGridDocumentsFromBulkFile Retrying Error after 1 second",
					It.IsAny<object[]>()))
				.Callback(new Action<Exception, string, object[]>((ex, template, @params) => fs?.Close()));
			
			// Act
			try
			{
				fs = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
				var mappedFields = new[]
				{
					new FieldInfo { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" }
				};
				var options = new DataGridReaderOptions
				{
					DataGridIDColumnName = "_DataGridID_",
					IdentifierColumnName = "_DocumentIdentifier_",
					MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
					LinkDataGridRecords = false,
					ReadFullTextFromFileLocation = false
				};
				var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, mockLogger.Object, 0, 0) { MaximumDataGridFieldSize = 104857600L };
				var resultsTable = GetTestDataTable(options);

				// Act
				resultsTable.Load(reader);

				// Assert
				Assert.AreEqual(3, resultsTable.Rows.Count);
			}
			finally
			{
				if (fs is object)
				{
					fs.Close();
				}
				// Assert
				mockLogger.Verify(mock => mock.LogWarning(
					It.IsAny<Exception>(),
					"ReadDataGridDocumentsFromBulkFile Retrying Error after 1 second",
					It.IsAny<object[]>()));
			}
		}

		// This test exercises the behavior of our code when another process has locked the load file.
		[Explicit("This test explors what happens when other processes lock our files. It's not a real test")]
		[Test]
		[TestCaseSource(nameof(GetFileLockData))]
		public void ReadFile_HasSingleField_CheckFileLockBehavior(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			var fileUtility = kCura.Utility.File.Instance;
			try
			{
				// Arrange
				fileUtility.Copy(_testFilePath, _testFilePathDestructive);
				using (var fs = new FileStream(_testFilePathDestructive, fileMode, fileAccess, fileShare))
				{
					var mappedFields = new[] { new FieldInfo { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" } };
					var options = new DataGridReaderOptions
					{
						DataGridIDColumnName = "_DataGridID_",
						IdentifierColumnName = "_DocumentIdentifier_",
						MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
						LinkDataGridRecords = false,
						ReadFullTextFromFileLocation = false
					};
					var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFilePathDestructive, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
					var resultsTable = GetTestDataTable(options);

					// Act
					resultsTable.Load(reader);
				}
			}
			catch (ArgumentException)
			{
			}
			// OK
			catch (IOException)
			{
			}
			// Also OK
			finally
			{
				fileUtility.Delete(_testFilePathDestructive);
			}
		}

		[Test]
		public void ReadFile_DataGridFields_WithID_CheckResults()
		{
			// Arrange
			var mappedFields = new[] { new FieldInfo { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" }, new FieldInfo { ArtifactID = 1002, EnableDataGrid = true, DisplayName = "field 2" }, new FieldInfo { ArtifactID = 1003, EnableDataGrid = true, DisplayName = "field 3" } };
			var options = new DataGridReaderOptions
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = true,
				ReadFullTextFromFileLocation = false
			};
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var resultsTable = GetTestDataTable(options);

			// Act
			resultsTable.Load(reader);

			// Assert
			Assert.AreEqual(3, resultsTable.Rows.Count);
			Assert.AreEqual("DOCAAA1", resultsTable.Select().ElementAtOrDefault(0)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA2", resultsTable.Select().ElementAtOrDefault(1)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA3", resultsTable.Select().ElementAtOrDefault(2)[options.IdentifierColumnName]);
			Assert.AreEqual("86E1B194-554D-416B-8213-2CC1DE822992", resultsTable.Select().ElementAtOrDefault(0)[options.DataGridIDColumnName]);
			Assert.AreEqual("86E1B194-554D-416B-8213-2CC1DE822992", resultsTable.Select().ElementAtOrDefault(1)[options.DataGridIDColumnName]);
			Assert.AreEqual("2F102973-E995-4CC7-996F-2B42E27F5384", resultsTable.Select().ElementAtOrDefault(2)[options.DataGridIDColumnName]);
			Assert.AreEqual("long text 1", resultsTable.Select().ElementAtOrDefault(0)[options.MappedDataGridFields.ElementAt(0).GetColumnName()]);
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(0)[options.MappedDataGridFields.ElementAt(1).GetColumnName()]);
			Assert.AreEqual("longtext threeee|þeeeeeeeeeeþ", resultsTable.Select().ElementAtOrDefault(0)[options.MappedDataGridFields.ElementAt(2).GetColumnName()]);
			BaseSqlQueryTest.ThenSQLsAreEqual(resultsTable.Select().ElementAtOrDefault(1)[options.MappedDataGridFields.ElementAt(0).GetColumnName()].ToString(), @"long
...???
..
			stuff?? ...

	....
text");
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(1)[options.MappedDataGridFields.ElementAt(1).GetColumnName()]);
			Assert.AreEqual("longكريtext", resultsTable.Select().ElementAtOrDefault(1)[options.MappedDataGridFields.ElementAt(2).GetColumnName()]);
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(2)[options.MappedDataGridFields.ElementAt(0).GetColumnName()]);
			BaseSqlQueryTest.ThenSQLsAreEqual(resultsTable.Select().ElementAtOrDefault(2)[options.MappedDataGridFields.ElementAt(1).GetColumnName()].ToString(),@"日本語の音韻は、「っ」「ん」を除いて母音で終わる開音節言語の性格が強く、また共通語を含め多くの方言がモーラを持つ。アクセントは高低アクセントである。古来の大和言葉では、原則として

   1. 「ら行」音が語頭に立たない（しりとり遊びで「ら行」で始まる言葉が見つけにくいのはこのため。「らく（楽）」「らっぱ」「りんご」などは大和言葉でない）
   2. 濁音が語頭に立たない（「抱（だ）く」「どれ」「ば（場）」「ばら（薔薇）」などは後世の変化）
   3. 同一語根内に母音が連続しない（「あお（青）」「かい（貝）」は古くは [awo], [kapi, kaɸi]）

などの特徴があった（「系統」および「音韻」の節参照）。

文は、「主語・修飾語・述語」の語順で構成される。修飾語は被修飾語の前に位置する。また、名詞の格を示すためには、語順や語尾を変化させるのでなく、文法的な機能を示す機能語（助詞）を後ろにつけ加える（膠着させる）。これらのことから、言語類型論上は、語順の点ではSOV型の言語に、形態の点では膠着語に分類される（「文法」の節参照）。

語彙は、古来の大和言葉のほか、中国から渡来した漢語がおびただしく、さらに近代以降には西洋語を中心とする外来語が増大している（「語種」の節参照）。

待遇表現の面では、文法的・語彙的に発達した敬語体系があり、叙述される人物同士の微妙な関係を表現する（「待遇表現」の節参照）。

方言は、日本の東西および琉球地方で大きく異なる。さらに詳細に見れば、地方ごとに多様な方言的特色がある（「方言」の節参照）。

他の多くの言語と異なる点としては、まず、表記体系の複雑さが挙げられる。漢字（音読みおよび訓読みで用いられる）や平仮名、片仮名のほか、ラテン文字（ローマ字）など、常に3種類以上の文字を組み合わせて表記する言語は無類と言ってよい（「字種」の節参照）。また、人称表現が「わたくし・わたし・ぼく・おれ」「あなた・あんた・きみ・おまえ」などと多様であるのも特徴である（「人称語彙」の節参照）。");
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(2)[options.MappedDataGridFields.ElementAt(2).GetColumnName()]);
		}

		[Test]
		public void ReadFile_HasSingleField_CheckResults()
		{
			// Arrange
			var mappedFields = new[] { new FieldInfo { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" } };
			var options = new DataGridReaderOptions
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var resultsTable = GetTestDataTable(options);

			// Act
			resultsTable.Load(reader);

			// Assert
			Assert.AreEqual(3, resultsTable.Rows.Count);
			Assert.AreEqual("DOCAAA1", resultsTable.Select().ElementAtOrDefault(0)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA2", resultsTable.Select().ElementAtOrDefault(1)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA3", resultsTable.Select().ElementAtOrDefault(2)[options.IdentifierColumnName]);
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(0)[options.DataGridIDColumnName]);
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(1)[options.DataGridIDColumnName]);
			Assert.AreEqual(string.Empty, resultsTable.Select().ElementAtOrDefault(2)[options.DataGridIDColumnName]);
		}

		[Test]
		public void ReadFile_HasNoFieldsField_WithID_CheckResults()
		{
			// Arrange
			var mappedFields = Array.Empty<FieldInfo>();
			var options = new DataGridReaderOptions
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};
			var reader = new DataGridTempFileDataReader(options, "|x", Relativity.Constants.ENDLINETERMSTRING, _testFileWithDataGridIDNoFieldsPath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = 104857600L };
			var resultsTable = GetTestDataTable(options);

			// Act
			resultsTable.Load(reader);

			// Assert
			Assert.AreEqual(3, resultsTable.Rows.Count);
			Assert.AreEqual("DOCAAA1", resultsTable.Select().ElementAtOrDefault(0)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA2", resultsTable.Select().ElementAtOrDefault(1)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA3", resultsTable.Select().ElementAtOrDefault(2)[options.IdentifierColumnName]);
			Assert.AreEqual("86E1B194-554D-416B-8213-2CC1DE822992", resultsTable.Select().ElementAtOrDefault(0)[options.DataGridIDColumnName]);
			Assert.AreEqual("86E1B194-554D-416B-8213-2CC1DE822992", resultsTable.Select().ElementAtOrDefault(1)[options.DataGridIDColumnName]);
			Assert.AreEqual("2F102973-E995-4CC7-996F-2B42E27F5384", resultsTable.Select().ElementAtOrDefault(2)[options.DataGridIDColumnName]);
		}

		[Test]
		[TestCase(23)]
		[TestCase(25)]
		[TestCase(2000)]
		public void ReadFile_HasTinyByteLimit_CheckResults(long byteThreshold)
		{
			// Arrange
			var mappedFields = new[] { new FieldInfo { ArtifactID = 1001, EnableDataGrid = true, DisplayName = "field 1" }, new FieldInfo { ArtifactID = 1002, EnableDataGrid = true, DisplayName = "field 2" }, new FieldInfo { ArtifactID = 1003, EnableDataGrid = true, DisplayName = "field 3" } };
			var options = new DataGridReaderOptions
			{
				DataGridIDColumnName = "_DataGridID_",
				IdentifierColumnName = "_DocumentIdentifier_",
				MappedDataGridFields = mappedFields.Where(f => f.EnableDataGrid).ToList(),
				LinkDataGridRecords = false,
				ReadFullTextFromFileLocation = false
			};
			var reader = new DataGridTempFileDataReader(options, "|", Relativity.Constants.ENDLINETERMSTRING, _testFilePath, new NullLogger(), 0, 0) { MaximumDataGridFieldSize = byteThreshold };
			var resultsTable = GetTestDataTable(options);

			// Act
			resultsTable.Load(reader);

			// Assert
			Assert.AreEqual(3, resultsTable.Rows.Count);
			Assert.AreEqual("DOCAAA1", resultsTable.Select().ElementAtOrDefault(0)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA2", resultsTable.Select().ElementAtOrDefault(1)[options.IdentifierColumnName]);
			Assert.AreEqual("DOCAAA3", resultsTable.Select().ElementAtOrDefault(2)[options.IdentifierColumnName]);
			Assert.AreEqual("x", resultsTable.Select().ElementAtOrDefault(0)[options.DataGridIDColumnName]);
			Assert.AreEqual("x", resultsTable.Select().ElementAtOrDefault(1)[options.DataGridIDColumnName]);
			Assert.AreEqual("x", resultsTable.Select().ElementAtOrDefault(2)[options.DataGridIDColumnName]);
			foreach (DataRow row in resultsTable.Rows)
			{
				foreach (FieldInfo fieldName in options.MappedDataGridFields)
				{
					Assert.That(Encoding.Unicode.GetByteCount(row[fieldName.GetColumnName()].ToString()), Is.EqualTo(byteThreshold + 1L).Or.LessThan(byteThreshold));
				}
			}
		}
	}
}