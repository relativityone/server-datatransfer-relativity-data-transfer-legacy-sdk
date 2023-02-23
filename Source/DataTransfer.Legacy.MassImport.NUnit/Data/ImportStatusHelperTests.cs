using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Logging;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class ImportStatusHelperTests
	{
		private Mock<ILog> _logMock;

		[SetUp]
		public void Setup()
		{
			_logMock = new Mock<ILog>();
		}

		[Test]
		public void GetCsvErrorLineShouldReturnGenericMessage()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorOverwrite, "someIdentifier", "", -1, "DocIdentifier", null, "");
			result.Should().Be(" - This document identifier does not exist in the workspace - no document to overwrite");
		}

		[Test]
		public void GetCsvErrorLineShouldReturnDocumentIdentifierFormattedMessage()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorAppend, "someIdentifier", "", -1, "DocIdentifier", null, "");
			result.Should().Be(" - An item with identifier DocIdentifier already exists in the workspace");
		}

		[Test]
		public void GetCsvErrorLineShouldReturnErrorDataFormattedMessage()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorDuplicateAssociatedObject, "someIdentifier", "", -1, "DocIdentifier", null, "Object1|Object2|Field");
			result.Should().Be(" - A non unique associated object 'Object1' is specified for the 'Object2' object in the field 'Field'");
		}

		[Test]
		public void GetCsvErrorLineShouldReturnTemplateMessageInCaseOfEmptyErrorData()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorDuplicateAssociatedObject, "someIdentifier", "", -1, "DocIdentifier", null, "");
			result.Should().Be(" - A non unique associated object '{0}' is specified for the '{1}' object in the field '{2}'");

			_logMock.Verify(x => x.LogError(It.IsAny<FormatException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void GetCsvErrorLineShouldReturnTemplateMessageInCaseOfFormatException()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorDuplicateAssociatedObject, "someIdentifier", "", -1, "DocIdentifier", null, "Object1|Object2");
			result.Should().Be(" - A non unique associated object '{0}' is specified for the '{1}' object in the field '{2}'");

			_logMock.Verify(x => x.LogError(It.IsAny<FormatException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void GetCsvErrorLineShouldReturnGenericErrorBatesMessage()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorBates, "someIdentifier", "", -1, "DocIdentifier", null, "");
			result.Should().Be(" - This image was not imported; other images in this document have page identifiers already in use in the workspace");
		}

		[Test]
		public void GetCsvErrorLineShouldReturnErrorBatesMessageWithErrorBatesIdentifierName()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorBates, "someIdentifier", "Error_Bates_Identifier", -1, "DocIdentifier", null, "");
			result.Should().Be(" - This image was not imported; the page identifier someIdentifier already exists for Error_Bates_Identifier");
		}

		[Test]
		public void GetCsvErrorLineShouldReturnErrorBatesMessageWithErrorBatesIdentifier()
		{
			var result = DTO.ImportStatusHelper.GetCsvErrorLine(_logMock.Object, (long)ImportStatus.ErrorBates, "someIdentifier", "", 90210, "DocIdentifier", null, "");
			result.Should().Be(" - This image was not imported; the page identifier someIdentifier already exists for the document with Artifact ID 90210");
		}
	}
}