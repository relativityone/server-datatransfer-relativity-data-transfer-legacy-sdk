using System;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Core.Pipeline.Stages.Job;
using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Stages.Job
{
	[TestFixture]
	public class ExecuteIfJobNotInitializedStageTests
	{
		private NativeImportInput _input;
		private Mock<IPipelineStage<NativeImportInput>> _innerStageMock;
		private Mock<IStagingTableRepository> _stagingTableRepositoryMock;

		private ExecuteIfJobNotInitializedStage<NativeImportInput> _sut;

		[SetUp]
		public void SetUp()
		{
			var pipelineExecutor = new PipelineExecutor();
			_input = NativeImportInput.ForWebApi(new Relativity.MassImport.DTO.NativeLoadInfo(), inRepository: false, includeExtractedTextEncoding: false, string.Empty);

			_innerStageMock = new Mock<IPipelineStage<NativeImportInput>>();
			_innerStageMock.Setup(x => x.Execute(_input)).Returns(_input);

			_stagingTableRepositoryMock = new Mock<IStagingTableRepository>();

			_sut = new ExecuteIfJobNotInitializedStage<NativeImportInput>(pipelineExecutor, _innerStageMock.Object, _stagingTableRepositoryMock.Object);
		}

		[Test]
		public void ShouldExecuteInnerStageWhenStagingTableNotExist()
		{
			// arrange
			_stagingTableRepositoryMock.Setup(x => x.StagingTablesExist()).Returns(false);

			// act
			var result = _sut.Execute(_input);

			// assert
			Assert.That(result, Is.EqualTo(_input));
			_innerStageMock.Verify(x=>x.Execute(_input), Times.Once);
		}

		[Test]
		public void ShouldNotExecuteInnerStageWhenStagingTableExist()
		{
			// arrange
			_stagingTableRepositoryMock.Setup(x => x.StagingTablesExist()).Returns(true);

			// act
			var result = _sut.Execute(_input);

			// assert
			Assert.That(result, Is.EqualTo(_input));
			_innerStageMock.Verify(x => x.Execute(_input), Times.Never);
		}

		[Test]
		public void ShouldRethrowExceptions()
		{
			// arrange
			var expectedException = new Exception();
			_stagingTableRepositoryMock.Setup(x => x.StagingTablesExist()).Throws(expectedException);

			// act & assert
			Assert.That(() => _sut.Execute(_input), Throws.Exception.EqualTo(expectedException));
		}
	}
}
