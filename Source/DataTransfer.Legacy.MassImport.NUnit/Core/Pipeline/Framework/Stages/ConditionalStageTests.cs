using Moq;
using NUnit.Framework;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Framework.Stages
{
	[TestFixture]
	public class ConditionalStageTests
	{
		private IPipelineExecutor _pipelineExecutor;
		private Mock<IPipelineStage<int>> _innerStage;

		[SetUp]
		public void SetUp()
		{
			_pipelineExecutor = new PipelineExecutor();
			_innerStage = new Mock<IPipelineStage<int>>();
		}

		[Test]
		public void ShouldNotExecuteInnerStage()
		{
			// Arrange
			var sut = CreateSut(shouldExecute: false);

			// Act
			sut.Execute(42);

			// Assert
			_innerStage.Verify(x => x.Execute(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public void ShouldExecuteInnerStage()
		{
			// Arrange
			const int expectedInput = 42;
			const int expectedOutput = 7;

			_innerStage.Setup(x => x.Execute(expectedInput)).Returns(expectedOutput);

			var sut = CreateSut(shouldExecute: true);

			// Act
			var actualOutput = sut.Execute(expectedInput);

			// Assert
			Assert.That(actualOutput, Is.EqualTo(expectedOutput));
			_innerStage.Verify(x => x.Execute(expectedInput), Times.Once);
		}

		[Test]
		public void ShouldRethrowExceptionFromInnerNode()
		{
			const int expectedInput = 42;
			var expectedException = new System.Exception();

			_innerStage
				.Setup(x => x.Execute(expectedInput))
				.Throws(expectedException);

			var sut = CreateSut(shouldExecute: true);

			// Act
			Assert.That(() => sut.Execute(expectedInput), Throws.Exception.EqualTo(expectedException), "It should rethrow exception from inner stage");
		}

		private Sut CreateSut(bool shouldExecute)
		{
			return new Sut(_pipelineExecutor, _innerStage.Object, shouldExecute);
		}

		private class Sut : ConditionalStage<int>
		{
			private readonly bool _shouldExecute;

			public Sut(IPipelineExecutor pipelineExecutor, IPipelineStage<int> innerStage, bool shouldExecute) : base(pipelineExecutor, innerStage)
			{
				_shouldExecute = shouldExecute;
			}

			protected override bool ShouldExecute(int input)
			{
				return _shouldExecute;
			}
		}
	}
}
