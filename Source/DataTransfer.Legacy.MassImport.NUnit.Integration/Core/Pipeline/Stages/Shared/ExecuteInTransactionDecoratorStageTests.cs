using System;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.API;

namespace MassImport.NUnit.Integration.Core.Pipeline.Stages.Shared
{
	[TestFixture]
	public class ExecuteInTransactionDecoratorStageTests : MassImportTestBase
	{
		private const string GetNumberOfTransactionsQuery = "SELECT @@TRANCOUNT";
		private const int PipelineInput = 0; // it is not used
		private ExecuteInTransactionDecoratorStage<int, int> _sut;

		[SetUp]
		public void SetUp()
		{
			var loggerMock = new Mock<ILog> { DefaultValue = DefaultValue.Mock };
			var loggingContext = new LoggingContext("testCorrelationId", "testClient", loggerMock.Object);

			var context = new MassImportContext(
				this.CoreContext.ChicagoContext,
				loggingContext,
				jobDetails: null, // it is not used
				caseSystemArtifactId: 0,
				new Mock<IHelper>().Object); // it is not used
			var executor = new PipelineExecutor();
			var innerStage = new ReturnNumberOfOpenTransactionsStage(context);
			_sut = new ExecuteInTransactionDecoratorStage<int, int>(innerStage, executor, context);
		}

		[Test]
		public void ShouldStartNewTransactionWhenNoTransactionIsOpen()
		{
			// act
			int numberOfOpenTransactions = _sut.Execute(PipelineInput);

			// assert
			Assert.That(numberOfOpenTransactions, Is.EqualTo(1), "It should open single transaction.");
		}

		[Test]
		public void ShouldNotStartTransactionWhenTransactionIsAlreadyOpen()
		{
			// arrange
			this.Context.BeginTransaction();

			try
			{
				// act
				int numberOfOpenTransactions = _sut.Execute(PipelineInput);

				// assert
				Assert.That(numberOfOpenTransactions, Is.EqualTo(1), "It should use existing transaction.");
				
				numberOfOpenTransactions = Context.ExecuteSqlStatementAsScalar<int>(GetNumberOfTransactionsQuery);
				Assert.That(numberOfOpenTransactions, Is.EqualTo(1), "It should not close existing transaction.");
			}
			catch (Exception)
			{
				this.Context.RollbackTransaction();
				throw;
			}
		}

		private class ReturnNumberOfOpenTransactionsStage : IPipelineStage<int, int>
		{
			private readonly MassImportContext _context;

			public ReturnNumberOfOpenTransactionsStage(MassImportContext context)
			{
				_context = context;
			}

			public int Execute(int input)
			{
				return _context.BaseContext.DBContext.ExecuteSqlStatementAsScalar<int>(GetNumberOfTransactionsQuery);
			}
		}
	}
}
