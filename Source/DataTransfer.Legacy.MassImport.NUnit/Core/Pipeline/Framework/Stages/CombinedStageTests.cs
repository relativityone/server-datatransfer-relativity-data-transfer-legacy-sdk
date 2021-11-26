using Moq;
using NUnit.Framework;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Framework.Stages
{
	[TestFixture]
	public class CombinedStageTests
	{
		private IPipelineExecutor _pipelineExecutor;
		private Mock<IPipelineStage<int, string>> _firstStageIntString;
		private Mock<IPipelineStage<string, double>> _secondStageStringDouble;
		private Mock<IPipelineStage<int>> _firstStageInt;
		private Mock<IPipelineStage<int>> _secondStageInt;
		private Mock<IPipelineStage<int>> _thirdStageInt;
		[SetUp]
		public void SetUp()
		{
			_pipelineExecutor = new PipelineExecutor();
			_firstStageIntString = new Mock<IPipelineStage<int, string>>(MockBehavior.Strict);
			_secondStageStringDouble = new Mock<IPipelineStage<string, double>>(MockBehavior.Strict);
			_firstStageInt = new Mock<IPipelineStage<int>>(MockBehavior.Strict);
			_secondStageInt = new Mock<IPipelineStage<int>>(MockBehavior.Strict);
			_thirdStageInt = new Mock<IPipelineStage<int>>(MockBehavior.Strict);
		}

		[Test]
		public void ShouldCombineTwoStagesWhenTypesAreDifferent()
		{
			// Arrange
			const int expectedInput = 1;
			const string expectedIntermediate = "value";
			const double expectedResult = 2.5;

			_firstStageIntString.Setup(x => x.Execute(expectedInput)).Returns(expectedIntermediate);
			_secondStageStringDouble.Setup(x => x.Execute(expectedIntermediate)).Returns(expectedResult);

			var sut = CombinedStage.Create(_pipelineExecutor, _firstStageIntString.Object, _secondStageStringDouble.Object);

			// Act
			double actualResult = sut.Execute(expectedInput);

			// Assert
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ShouldCombineTwoStagesWhenTypesAreIdentical()
		{
			// Arrange
			const int expectedInput = 1;
			const int expectedIntermediate = 2;
			const int expectedResult = 3;

			_firstStageInt.Setup(x => x.Execute(expectedInput)).Returns(expectedIntermediate);
			_secondStageInt.Setup(x => x.Execute(expectedIntermediate)).Returns(expectedResult);

			var sut = CombinedStage.Create(_pipelineExecutor, _firstStageInt.Object, _secondStageInt.Object);

			// Act
			double actualResult = sut.Execute(expectedInput);

			// Assert
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ShouldCombineThreeStagesWhenTypesAreIdentical()
		{
			// Arrange
			const int expectedInput = 1;
			const int expectedIntermediateOne = 2;
			const int expectedIntermediateTwo = 3;
			const int expectedResult = 4;

			_firstStageInt.Setup(x => x.Execute(expectedInput)).Returns(expectedIntermediateOne);
			_secondStageInt.Setup(x => x.Execute(expectedIntermediateOne)).Returns(expectedIntermediateTwo);
			_thirdStageInt.Setup(x => x.Execute(expectedIntermediateTwo)).Returns(expectedResult);

			var twoStagesCombined = CombinedStage.Create(_pipelineExecutor, _firstStageInt.Object, _secondStageInt.Object);
			var sut = CombinedStage.Create(_pipelineExecutor, twoStagesCombined, _thirdStageInt.Object);

			// Act
			double actualResult = sut.Execute(expectedInput);

			// Assert
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ShouldRethrowExceptionWhenFirstStageFailedWhenTypesAreDifferent()
		{
			// Arrange
			const int expectedInput = 1;
			var expectedException = new System.Exception();
			_firstStageIntString.Setup(x => x.Execute(expectedInput)).Throws(expectedException);

			var sut = CombinedStage.Create(_pipelineExecutor, _firstStageIntString.Object, _secondStageStringDouble.Object);

			// Act
			Assert.That(() => sut.Execute(expectedInput), Throws.Exception.EqualTo(expectedException), "It should rethrow exception.");
		}

		[Test]
		public void ShouldRethrowExceptionWhenSecondStageFailedWhenTypesAreDifferent()
		{
			// Arrange
			const int expectedInput = 1;
			const string expectedIntermediate = "value";
			var expectedException = new System.Exception();

			_firstStageIntString.Setup(x => x.Execute(expectedInput)).Returns(expectedIntermediate);
			_secondStageStringDouble.Setup(x => x.Execute(expectedIntermediate)).Throws(expectedException);

			var sut = CombinedStage.Create(_pipelineExecutor, _firstStageIntString.Object, _secondStageStringDouble.Object);

			// Act
			Assert.That(() => sut.Execute(expectedInput), Throws.Exception.EqualTo(expectedException), "It should rethrow exception.");
		}

		[Test]
		public void ShouldRethrowExceptionWhenStageFailedWhenTypesAreIdentical()
		{
			// Arrange
			const int expectedInput = 1;
			const int expectedIntermediateOne = 2;
			const int expectedIntermediateTwo = 3;
			const int expectedResult = 4;

			var expectedException = new System.Exception();

			_firstStageInt.Setup(x => x.Execute(expectedInput)).Returns(expectedIntermediateOne);
			_secondStageInt.Setup(x => x.Execute(expectedIntermediateOne)).Throws(expectedException);
			_thirdStageInt.Setup(x => x.Execute(expectedIntermediateTwo)).Returns(expectedResult);

			var twoStagesCombined = CombinedStage.Create(_pipelineExecutor, _firstStageInt.Object, _secondStageInt.Object);
			var sut = CombinedStage.Create(_pipelineExecutor, twoStagesCombined, _thirdStageInt.Object);

			// Act
			Assert.That(() => sut.Execute(expectedInput), Throws.Exception.EqualTo(expectedException), "It should rethrow exception.");
		}
	}
}
