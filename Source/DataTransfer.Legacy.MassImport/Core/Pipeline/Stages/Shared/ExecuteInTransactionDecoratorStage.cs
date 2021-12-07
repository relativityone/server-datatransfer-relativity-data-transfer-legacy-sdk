using System;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	/// <summary>
	/// When there is no transaction, it starts new transaction, executes inner stage and commits/rollbacks transaction.
	/// If transaction already exists it executes inner stage.
	/// </summary>
	internal class ExecuteInTransactionDecoratorStage<TInput, TOutput> : DecoratorStage<TInput, TOutput>
	{
		private readonly MassImportContext _context;

		public ExecuteInTransactionDecoratorStage(
			IPipelineStage<TInput, TOutput> innerStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context) : base(pipelineExecutor, innerStage)
		{
			_context = context;
		}

		public override TOutput Execute(TInput input)
		{
			// this logic is required due to REL-615765 (Analytics calls mass import from a transaction).
			return IsTransactionOpen()
				? ExecuteInExistingTransaction(input)
				: StartTransactionAndExecute(input);
		}

		private bool IsTransactionOpen()
		{
			bool isTransactionOpen = _context.BaseContext.DBContext.GetTransaction() != null;
			_context.Logger.LogDebug("ExecuteInTransactionDecoratorStage. Is transaction open: {isTransactionOpen}", isTransactionOpen);
			return isTransactionOpen;
		}

		private TOutput ExecuteInExistingTransaction(TInput input)
		{
			return base.Execute(input);
		}

		private TOutput StartTransactionAndExecute(TInput input)
		{
			TOutput result;
			_context.BaseContext.BeginTransaction();

			try
			{
				result = base.Execute(input);
			}
			catch
			{
				TryRollbackTransaction();
				throw;
			}

			CommitTransaction();

			return result;
		}

		private void CommitTransaction()
		{
			try
			{
				_context.BaseContext.CommitTransaction();
			}
			catch (Exception ex)
			{
				_context.Logger.LogError(ex, "Exception occured when commiting a transaction. Trying to rollback.");
				TryRollbackTransaction();
				throw ex;
			}
		}

		private void TryRollbackTransaction()
		{
			try
			{
				_context.BaseContext.RollbackTransaction();
			}
			catch (Exception ex)
			{
				_context.Logger.LogError(ex, "Exception occured when rolling back a transaction.");
			}
		}
	}

	internal static class ExecuteInTransactionDecoratorStage
	{
		public static ExecuteInTransactionDecoratorStage<T1, T2> New<T1, T2>(
			IPipelineStage<T1, T2> innerStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context)
		{
			return new ExecuteInTransactionDecoratorStage<T1, T2>(innerStage, pipelineExecutor, context);
		}
	}
}
