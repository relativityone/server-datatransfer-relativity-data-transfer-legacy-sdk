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
	public class MassImportArtifactTests
	{
		private IReadOnlyList<object> _fieldValues;
		private string _fileGuid;
		private string _fileName;
		private string _location;
		private string _originalFileLocation;
		private int _originalLineNumber;
		private int _fileSize;
		private int _parentFolderId;

		[SetUp]
		public void SetUp()
		{
			_fieldValues = new List<object> { "value1", 123, true };
			_fileGuid = "test-guid";
			_fileName = "test-file.txt";
			_location = "test-location";
			_originalFileLocation = "test-original-location";
			_originalLineNumber = 10;
			_fileSize = 2048;
			_parentFolderId = 5;
		}

		[Test]
		public void Constructor_ShouldInitializeProperties()
		{
			// Act
			var artifact = new MassImportArtifact(
				_fieldValues,
				_fileGuid,
				_fileName,
				_location,
				_originalFileLocation,
				_originalLineNumber,
				_fileSize,
				_parentFolderId);

			// Assert
			Assert.AreEqual(_fieldValues, artifact.FieldValues);
			Assert.AreEqual(_fileGuid, artifact.FileGuid);
			Assert.AreEqual(_fileName, artifact.FileName);
			Assert.AreEqual(_location, artifact.Location);
			Assert.AreEqual(_originalFileLocation, artifact.OriginalFileLocation);
			Assert.AreEqual(_originalLineNumber, artifact.OriginalLineNumber);
			Assert.AreEqual(_fileSize, artifact.FileSize);
			Assert.AreEqual(_parentFolderId, artifact.ParentFolderId);
		}

		[Test]
		public void Constructor_ShouldHandleNullOptionalParameters()
		{
			// Act
			var artifact = new MassImportArtifact(_fieldValues);

			// Assert
			Assert.AreEqual(_fieldValues, artifact.FieldValues);
			Assert.IsNull(artifact.FileGuid);
			Assert.IsNull(artifact.FileName);
			Assert.IsNull(artifact.Location);
			Assert.IsNull(artifact.OriginalFileLocation);
			Assert.AreEqual(0, artifact.OriginalLineNumber);
			Assert.AreEqual(0, artifact.FileSize);
			Assert.AreEqual(0, artifact.ParentFolderId);
		}

		[Test]
		public void Constructor_ShouldInitializeWithDefaultValues()
		{
			// Arrange
			var fieldValues = new List<object>();

			// Act
			var artifact = new MassImportArtifact(fieldValues);

			// Assert
			Assert.AreEqual(fieldValues, artifact.FieldValues);
			Assert.IsNull(artifact.FileGuid);
			Assert.IsNull(artifact.FileName);
			Assert.IsNull(artifact.Location);
			Assert.IsNull(artifact.OriginalFileLocation);
			Assert.AreEqual(0, artifact.OriginalLineNumber);
			Assert.AreEqual(0, artifact.FileSize);
			Assert.AreEqual(0, artifact.ParentFolderId);
		}
	}
}