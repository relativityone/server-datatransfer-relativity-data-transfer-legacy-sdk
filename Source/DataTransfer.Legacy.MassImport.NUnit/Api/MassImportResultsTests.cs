using NUnit.Framework;
using Relativity.MassImport.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.NUnit.Api
{
	[TestFixture]
	public class MassImportResultsTests
	{
		private MassImportResults _results;

		[SetUp]
		public void SetUp()
		{
			_results = new MassImportResults();
		}

		[Test]
		public void DefaultConstructor_ShouldInitializeProperties()
		{
			// Assert default values for properties
			Assert.AreEqual(0, _results.FilesProcessed);
			Assert.AreEqual(0, _results.ArtifactsProcessed);
			Assert.AreEqual(0, _results.ArtifactsCreated);
			Assert.AreEqual(0, _results.ArtifactsUpdated);
			Assert.IsNull(_results.ExceptionDetail);
			Assert.IsNull(_results.ItemErrors);
			Assert.IsNull(_results.RunId);
			Assert.IsNull(_results.AffectedArtifactIds);
			Assert.IsNull(_results.KeyFieldToArtifactIdsMappings);
		}

		[Test]
		public void Properties_ShouldSetAndGetCorrectly()
		{
			// Arrange
			var exceptionDetail = new MassImportExceptionDetail(new System.Exception("Test exception"));
			var itemErrors = new List<string> { "Error 1", "Error 2" };
			var affectedArtifactIds = new List<int> { 1, 2, 3 };
			var keyFieldToArtifactIdsMappings = new Dictionary<string, IEnumerable<int>>
			{
				{ "Key1", new List<int> { 1, 2 } },
				{ "Key2", new List<int> { 3, 4 } }
			};

			// Act
			_results.FilesProcessed = 10;
			_results.ArtifactsProcessed = 20;
			_results.ArtifactsCreated = 5;
			_results.ArtifactsUpdated = 15;
			_results.ExceptionDetail = exceptionDetail;
			_results.ItemErrors = itemErrors;
			_results.RunId = "Run123";
			_results.AffectedArtifactIds = affectedArtifactIds;
			_results.KeyFieldToArtifactIdsMappings = keyFieldToArtifactIdsMappings;

			// Assert
			Assert.AreEqual(10, _results.FilesProcessed);
			Assert.AreEqual(20, _results.ArtifactsProcessed);
			Assert.AreEqual(5, _results.ArtifactsCreated);
			Assert.AreEqual(15, _results.ArtifactsUpdated);
			Assert.AreEqual(exceptionDetail, _results.ExceptionDetail);
			Assert.AreEqual(itemErrors, _results.ItemErrors);
			Assert.AreEqual("Run123", _results.RunId);
			Assert.AreEqual(affectedArtifactIds, _results.AffectedArtifactIds);
			Assert.AreEqual(keyFieldToArtifactIdsMappings, _results.KeyFieldToArtifactIdsMappings);
		}

		[Test]
		public void ExceptionDetail_ShouldHandleNull()
		{
			// Act
			_results.ExceptionDetail = null;

			// Assert
			Assert.IsNull(_results.ExceptionDetail);
		}

		[Test]
		public void ItemErrors_ShouldHandleNull()
		{
			// Act
			_results.ItemErrors = null;

			// Assert
			Assert.IsNull(_results.ItemErrors);
		}

		[Test]
		public void AffectedArtifactIds_ShouldHandleNull()
		{
			// Act
			_results.AffectedArtifactIds = null;

			// Assert
			Assert.IsNull(_results.AffectedArtifactIds);
		}

		[Test]
		public void KeyFieldToArtifactIdsMappings_ShouldHandleNull()
		{
			// Act
			_results.KeyFieldToArtifactIdsMappings = null;

			// Assert
			Assert.IsNull(_results.KeyFieldToArtifactIdsMappings);
		}
	}
}