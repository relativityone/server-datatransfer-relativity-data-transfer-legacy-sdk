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
		public void Convert_ReduceResultLength_IfExceedsThreshold()
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
		public void Convert_ReduceResultLengthTwice_IfExceedsThreshold()
		{
			// arrange
			var result = GenerateResult(800);

			// act
			var actual = _sut.Convert(result);

			// assert
			var unwrapped = actual.Unwrap();
			unwrapped.Length.Should().Be(512);
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
