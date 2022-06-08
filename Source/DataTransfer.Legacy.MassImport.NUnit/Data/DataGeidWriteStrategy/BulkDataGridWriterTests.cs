using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Data;
using Relativity.Data.MassImportOld.DataGridWriteStrategy;
using Relativity.DataGrid;
using Relativity.DataGrid.Helpers;
using Relativity.Logging;

namespace Relativity.MassImport.NUnit.Data.DataGeidWriteStrategy
{
	[TestFixture]
	public class BulkDataGridWriterTests
	{
		#region Race conditions
		[TestCase(100)]
		public async Task Write_OnError_SupportsParallelism(int parallelism)
		{
			var idMapping = new List<DataGridImportIdentity>();
			for (int i = 0, loopTo = parallelism; i <= loopTo; i++)
			{
				var id = new DataGridImportIdentity()
				{
					ImportID = i,
					ArtifactID = i,
					DocumentIdentifier = $"DOCID{i}",
				};
				idMapping.Add(id);
			}

			var mappings = new DataGridMappingMultiDictionary();
			mappings.LoadCacheForImport(idMapping);
			var errorManager = new DataGridImportErrorManager(mappings, new NullLogger());
			var testee = new BulkDataGridWriter(new TestContext(), 10, 1000001, errorManager, new NullLogger(), new List<FieldInfo>());

			var tasks = new System.Collections.Concurrent.ConcurrentBag<Task>();
			Parallel.ForEach(
				Enumerable.Range(0, parallelism).Select(artifactID => NewDataGridRecord(artifactID)), (record) => tasks.Add(testee.Write(new[] { record })));
			await Task.WhenAll(tasks);
			var results = errorManager.ErrorMessages;

			Assert.AreEqual(1, results.Keys.Count());
			Assert.AreEqual(parallelism, results.Item(results.Keys.First()).Count);
		}

		public class TestContext : DataGridContext
		{
			public TestContext() : base((DataGridContextBase)null)
			{
			}

			public override IEnumerable<DataGridWriteResult> WriteBulk(int workspaceArtifactID, int artifactTypeID, IEnumerable<IDataGridRecord> records)
			{
				var result = new DataGridPartialResultsException<DataGridWriteResult>(new List<DataGridWriteResult>(new[] {
					new DataGridWriteResult()
					{
						ArtifactID = records.First().ArtifactID,
						ResultStatus = DataGridResult.Status.Error,
						ResultsErrorMessage = "One or more exceptions occurred lol"
					}
				}), "Error happened");
				throw result;
			}
		}

		private DataGridRecord NewDataGridRecord(int artifactID)
		{
			var record = new DataGridRecord()
			{
				ArtifactID = artifactID,
				Type = "document"
			};

			record.AddField("Fields", "ExtractedText", Guid.NewGuid().ToString());
			return record;
		}
		#endregion

		#region Validation statuses
		[Test]
		public void Test_ValidationErrorChecking_FieldSize()
		{
			var results = new DataGridWriteResult()
			{
				DataGridID = "5",
				ResultStatus = DataGridResult.Status.Verified,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields",
							FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields",
							FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.ValidationError,
							ResultsErrorMessage = DataGridHelper.INVALID_FIELD_TOO_MUCH_DATA_MESSAGE
						}
					}
				}
			};
			var statuses = BulkDataGridWriter.GetResultValidationStatuses(results);
			CollectionAssert.AreEquivalent(new[] { Relativity.MassImport.DTO.ImportStatus.DataGridFieldMaxSizeExceeded }, statuses);
		}

		[Test]
		public void Test_ValidationStatusChecking_FieldSizeIsNULL()
		{
			var results = new DataGridWriteResult() { DataGridID = "5", ResultStatus = DataGridResult.Status.Verified, FieldWriteResults = null };
			var statuses = BulkDataGridWriter.GetResultValidationStatuses(results);
			CollectionAssert.AreEquivalent(string.Empty, statuses);
		}

		[Test]
		public void Test_ValidationErrorChecking_FieldDataGridID()
		{
			var results = new DataGridWriteResult()
			{
				DataGridID = "5",
				ResultStatus = DataGridResult.Status.Verified,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields",
							FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields",
							FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.ValidationError,
							ResultsErrorMessage = DataGridHelper.INVALID_FIELD_NAME_MESSAGE
						}
					}
				}
			};
			var statuses = BulkDataGridWriter.GetResultValidationStatuses(results);
			CollectionAssert.AreEquivalent(new[] { Relativity.MassImport.DTO.ImportStatus.DataGridInvalidFieldNameError }, statuses);
		}

		[Test]
		public void Test_ValidationErrorChecking_DataGridID()
		{
			var results = new DataGridWriteResult()
			{
				DataGridID = "5",
				ResultStatus = DataGridResult.Status.ValidationError,
				ResultsErrorMessage = DataGridHelper.INVALID_RECORD_DATAGRIDID_MESSAGE,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields",
							FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields",
							FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			var statuses = BulkDataGridWriter.GetResultValidationStatuses(results);
			CollectionAssert.AreEquivalent(new [] { Relativity.MassImport.DTO.ImportStatus.DataGridInvalidDocumentIDError }, statuses);
		}

		[Test]
		public void Test_ValidationErrorChecking_Mixed()
		{
			var results = new DataGridWriteResult() { DataGridID = "5", ResultStatus = DataGridResult.Status.ValidationError, ResultsErrorMessage = DataGridHelper.INVALID_RECORD_DATAGRIDID_MESSAGE, FieldWriteResults = new List<DataGridWriteResult.FieldResult>() { { new DataGridWriteResult.FieldResult() { FieldNamespace = "Fields", FieldIdentifier = "1", ResultStatus = DataGridResult.Status.Verified } }, { new DataGridWriteResult.FieldResult() { FieldNamespace = "Fields", FieldIdentifier = "2", ResultStatus = DataGridResult.Status.ValidationError, ResultsErrorMessage = DataGridHelper.INVALID_FIELD_TOO_MUCH_DATA_MESSAGE } }, { new DataGridWriteResult.FieldResult() { FieldNamespace = "Fields", FieldIdentifier = "3", ResultStatus = DataGridResult.Status.ValidationError, ResultsErrorMessage = DataGridHelper.INVALID_FIELD_NAME_MESSAGE } } } };
			var statuses = BulkDataGridWriter.GetResultValidationStatuses(results);
			CollectionAssert.AreEquivalent(new[] { Relativity.MassImport.DTO.ImportStatus.DataGridFieldMaxSizeExceeded, Relativity.MassImport.ImportStatus.DataGridInvalidFieldNameError, Relativity.MassImport.ImportStatus.DataGridInvalidDocumentIDError }, statuses);
		}
		#endregion

		#region Get error messages
		[Test]
		public void NoError_OneFieldError_ShouldHaveCorrectErrorMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am dead"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("Field [2] Error: omg i am dead", actualError);
		}

		[Test]
		public void NoError_OneFieldValidationError_OneFieldError_ShouldHaveCorrectMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am dead"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.ValidationError,
							ResultsErrorMessage = DataGridHelper.INVALID_FIELD_TOO_MUCH_DATA_MESSAGE
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("Field [2] Error: omg i am dead", actualError);
		}

		[Test]
		public void DocumentError_OneFieldError_ShouldHaveCorrectErrorMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error, ResultsErrorMessage = "document exploded",
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am dead"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("document exploded " + Environment.NewLine + "Field [2] Error: omg i am dead", actualError);
		}

		[Test]
		public void DocumentError_FieldResultNULL_OneFieldError_ShouldHaveCorrectErrorMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error, ResultsErrorMessage = "document exploded",
				FieldWriteResults = null
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("document exploded ", actualError);
		}

		[Test]
		public void DocumentError_OneFieldValidationError_OneFieldError_ShouldHaveCorrectMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error, ResultsErrorMessage = "document exploded",
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am dead"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.ValidationError,
							ResultsErrorMessage = DataGridHelper.INVALID_FIELD_TOO_MUCH_DATA_MESSAGE
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("document exploded " + Environment.NewLine + "Field [2] Error: omg i am dead", actualError);
		}

		[Test]
		public void DocumentError_NoFieldError_ShouldHaveCorrectMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error, ResultsErrorMessage = "document exploded",
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("document exploded ", actualError);
		}

		[Test]
		public void DocumentError_OneFieldValidationError_ShouldHaveCorrectMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error, ResultsErrorMessage = "document exploded",
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Verified
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.ValidationError,
							ResultsErrorMessage = DataGridHelper.INVALID_FIELD_TOO_MUCH_DATA_MESSAGE
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Verified
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("document exploded ", actualError);
		}

		[Test]
		public void DocumentError_AllFieldValidationError_ShouldHaveCorrectMessage()
		{
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1", ResultStatus = DataGridResult.Status.Error, ResultsErrorMessage = "document exploded",
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "1",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am dead"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "2",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "zomg i am dead"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "3",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am dead!"
						}
					},
					{
						new DataGridWriteResult.FieldResult()
						{
							FieldNamespace = "Fields", FieldIdentifier = "4",
							ResultStatus = DataGridResult.Status.Error,
							ResultsErrorMessage = "omg i am deeeaaad"
						}
					}
				}
			};
			string actualError = BulkDataGridWriter.GetDataGridErrorMessages(doc);
			Assert.AreEqual("document exploded " + Environment.NewLine + "Field [1] Error: omg i am dead" + Environment.NewLine + "Field [2] Error: zomg i am dead" + Environment.NewLine + "Field [3] Error: omg i am dead!" + Environment.NewLine + "Field [4] Error: omg i am deeeaaad", actualError);
		}

		[Test]
		public void LookUpArtifatIDFromInMemoryMap()
		{
			var map = GetDocumentsMap();
			string lookUpKey = "6A33B50A-9559-4950-B37A-F42A9BC7CD11";

			int artifactID = (from kp in map
							  where (kp.Value.DataGridID ?? "") == (lookUpKey ?? "")
							  select kp.Key).SingleOrDefault();

			Assert.AreEqual(123456, artifactID);
		}

		private Dictionary<int, IDataGridRecord> GetDocumentsMap()
		{
			return new Dictionary<int, IDataGridRecord>() { { 123456, new DataGridRecord("document") { DataGridID = "6A33B50A-9559-4950-B37A-F42A9BC7CD11" } }, { 123457, new DataGridRecord("document") { DataGridID = "1E2C0449-6792-419A-B16B-50368CA9E12C" } }, { 123458, new DataGridRecord("document") { DataGridID = "C5D9BBFB-D4FC-4A7A-81C2-EC326AEB6CEE" } } };
		}
		#endregion
	}
}