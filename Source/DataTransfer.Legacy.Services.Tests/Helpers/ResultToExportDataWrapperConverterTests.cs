using System;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Helpers
{
	[TestFixture]
	public class ResultToExportDataWrapperConverterTests
	{
		private Mock<IAPILog> _loggerMock;
		private ResultToExportDataWrapperConverter _sut;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_sut = new ResultToExportDataWrapperConverter(_loggerMock.Object);
		}

		[Test]
		public void Convert_ReturnsEmptyDataSetWrapper_ForNullResult()
		{
			// act
			var actual = _sut.Convert(null);

			// assert
			actual.SerializedDataLength.Should().BeNull();
			var unwrapped = actual.Unwrap();
			unwrapped.Should().BeNull();
		}

		[Test]
		public void Convert_NotChangeResultLength_IfNotExceedThreshold()
		{
			// arrange
			var result = GenerateResult(250);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(1024);
		}

		[Test]
		public void Convert_ReduceResultLength_IfExceedsDeserializationThreshold()
		{
			// arrange
			var result = GenerateResult(400);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(512);
		}

		[Test]
		public void Convert_ReduceResultLengthTwice_IfExceedsDeserializationThreshold()
		{
			// arrange
			var result = GenerateResult(800);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(512);
		}

		[Test]
		public void Convert_ReduceResultLengthTwice_IfExceedsSerializationThreshold()
		{
			// arrange
			var result = GenerateResult(1000);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(256);
		}

		[TestCase(0, 1024)]
		[TestCase(1, 512)]
		[TestCase(2, 256)]
		[TestCase(3, 128)]
		[TestCase(4, 64)]
		[TestCase(5, 32)]
		public void Convert_ReduceResultLength_IfOutOfMemoryExceptionWasThrown(int exceptionsCount, int expectedLength)
		{
			// arrange
			int count = 0;
			_loggerMock.Setup(x => x.LogVerbose(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() =>
			{
				if (count < exceptionsCount)
				{
					count++;
					throw new OutOfMemoryException("OOM");
				}
			});
			var result = GenerateResult(1);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(expectedLength);
			count.Should().Be(exceptionsCount);
		}

		[TestCase(0, 1024)]
		[TestCase(1, 512)]
		[TestCase(2, 256)]
		[TestCase(3, 128)]
		[TestCase(4, 64)]
		[TestCase(5, 32)]
		public void Convert_ReduceResultLength_IfOutOfMemoryExceptionInInnerExceptionWasThrown(int exceptionsCount, int expectedLength)
		{
			// arrange
			int count = 0;
			_loggerMock.Setup(x => x.LogVerbose(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() =>
			{
				if (count < exceptionsCount)
				{
					count++;
					Exception exception = new OutOfMemoryException("OOM");
					for (int i = 0; i < exceptionsCount; i++)
					{
						exception = new InvalidOperationException($"OuterException{i}", exception);
					}

					throw exception;
				}
			});
			var result = GenerateResult(1);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(expectedLength);
			count.Should().Be(exceptionsCount);
		}

		[Test]
		public void Convert_ShouldThrow_IfNotOutOfMemoryException()
		{
			// arrange
			_loggerMock.Setup(x => x.LogVerbose(It.IsAny<string>(), It.IsAny<object[]>()))
				.Throws(new ArgumentException("Not supported"));
			var result = GenerateResult(1);

			// act & assert
			_sut.Invoking(x => x.Convert(result))
				.Should().Throw<ArgumentException>().WithMessage("Not supported");
		}

		private static object[] GenerateResult(int elementSize)
		{
			const int length = 1024;

			var sb = new StringBuilder(elementSize);
			sb.Append('a', elementSize);
			var element = sb.ToString();

			var result = new object[length][];
			for (int i = 0; i < length; i++)
			{
				result[i] = new object[length];
				for (int j = 0; j < length; j++)
				{
					result[i][j] = element;
				}
			}

			return result;
		}
	}
}
