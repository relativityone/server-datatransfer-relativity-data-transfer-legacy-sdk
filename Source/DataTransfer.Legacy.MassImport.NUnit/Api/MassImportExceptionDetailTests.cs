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
	public class MassImportExceptionDetailTests
	{
		private Exception _innerMostException;
		private Exception _innerException;
		private Exception _outerException;
		private StringBuilder _stringBuilder;

		[SetUp]
		public void SetUp()
		{
			_innerMostException = new Exception("Innermost exception message");
			_innerException = new Exception("Inner exception message", _innerMostException);
			_outerException = new Exception("Outer exception message", _innerException);
			_stringBuilder = new StringBuilder();
		}

		[Test]
		public void DefaultConstructor_ShouldInitializeProperties()
		{
			// Act
			var exceptionDetail = new MassImportExceptionDetail();

			// Assert
			Assert.IsNull(exceptionDetail.ExceptionType);
			Assert.IsNull(exceptionDetail.ExceptionMessage);
			Assert.IsNull(exceptionDetail.ExceptionTrace);
			Assert.IsNull(exceptionDetail.ExceptionFullText);
			Assert.IsNotNull(exceptionDetail.Details);
			Assert.IsEmpty(exceptionDetail.Details);
		}

		[Test]
		public void Constructor_WithException_ShouldInitializeProperties()
		{
			// Act
			var exceptionDetail = new MassImportExceptionDetail(_outerException);

			// Assert
			Assert.AreEqual(_outerException.GetType().ToString(), exceptionDetail.ExceptionType);
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("Error: Outer exception message"));
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("Error: Inner exception message"));
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("Error: Innermost exception message"));
			Assert.IsNull(exceptionDetail.ExceptionTrace);
			Assert.AreEqual(exceptionDetail.ExceptionMessage, exceptionDetail.ExceptionFullText);
		}

		[Test]
		public void Constructor_WithException_ShouldHandleInnerExceptions()
		{
			// Act
			var exceptionDetail = new MassImportExceptionDetail(_outerException);

			// Assert
			Assert.AreEqual(_outerException.GetType().ToString(), exceptionDetail.ExceptionType);
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("Error: Outer exception message"));
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("---Additional Errors---"));
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("Error: Inner exception message"));
			Assert.IsTrue(exceptionDetail.ExceptionMessage.Contains("Error: Innermost exception message"));
			Assert.IsNull(exceptionDetail.ExceptionTrace);
			Assert.AreEqual(exceptionDetail.ExceptionMessage, exceptionDetail.ExceptionFullText);
		}

	}
}
