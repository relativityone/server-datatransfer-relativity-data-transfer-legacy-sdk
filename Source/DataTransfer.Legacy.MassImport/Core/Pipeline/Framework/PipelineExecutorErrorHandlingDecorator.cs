using System;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using System.Diagnostics;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.Core.Pipeline.Framework
{
	internal class PipelineExecutorErrorHandlingDecorator : IPipelineExecutor
	{
		private readonly IPipelineExecutor _decoratedObject;
		private readonly LoggingContext _loggingContext;

		public PipelineExecutorErrorHandlingDecorator(IPipelineExecutor decoratedObject, LoggingContext loggingContext)
		{
			this._decoratedObject = decoratedObject;
			_loggingContext = loggingContext;
		}

		/// <exception cref="MassImportExecutionException">Exception type thrown when execution of a pipeline stage fails</exception>
		public TOutput Execute<TInput, TOutput>(IPipelineStage<TInput, TOutput> pipelineStage, TInput input)
		{
			if (pipelineStage is IInfrastructureStage)
			{
				return _decoratedObject.Execute(pipelineStage, input);
			}

			string nodeTypeName = pipelineStage.GetUserFriendlyStageName();
			_loggingContext.Logger.LogDebug("Starting execution of {nodeTypeName}.", nodeTypeName);

			try
			{
				var result = _decoratedObject.Execute(pipelineStage, input);
				_loggingContext.Logger.LogDebug("Execution of {nodeTypeName} completed successfully.", nodeTypeName);
				return result;
			}
			catch (MassImportExecutionException)
			{
				throw; // that exception was already handled when executing nested stage.
			}
			catch (Exception ex)
			{
				_loggingContext.Logger.LogError(ex, "Exception occured while executing {nodeTypeName}.", nodeTypeName);
				TraceHelper.SetStatusError(Activity.Current, $"Exception occured while executing {nodeTypeName}: {ex.Message}", ex);
				throw MassImportExceptionHandler.CreateMassImportExecutionException(ex, nodeTypeName);
			}
		}
	}
}