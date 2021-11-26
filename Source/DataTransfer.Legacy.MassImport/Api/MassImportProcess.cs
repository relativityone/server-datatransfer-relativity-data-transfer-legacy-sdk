using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Core;
using Relativity.Core.Exception;
using Relativity.Core.Process;

namespace Relativity.MassImport.Api
{
	/// <summary>
	/// Wrap MassImportManager class with process interface
	/// </summary>
	public class MassImportProcess : ProcessBase
	{
		private readonly IMassImportManager _massImportManager;
		private IEnumerable<MassImportArtifact> _artifacts;
		private MassImportSettings _settings;
		private CancellationToken _cancel;

		public MassImportProcess(IMassImportManager massImportManager)
		{
			_massImportManager = massImportManager;
		}
		public MassImportProcess(IMassImportManager massImportManager, ProcessState processState) : base(processState)
		{
			_massImportManager = massImportManager;
		}

		public void Initialize(IEnumerable<MassImportArtifact> artifacts, MassImportSettings settings, CancellationToken cancel)
		{
			_artifacts = artifacts;
			_settings = settings;
			_cancel = cancel;
		}

		public override void Execute()
		{
			ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private async Task ExecuteAsync()
		{
			try
			{
				this.ProcessState.TotalOperations = _artifacts.Count();
				this.ProcessState.State = ProcessState.ProcessStateValue.Running;
				MassImportResults results = await _massImportManager.RunMassImportAsync(_artifacts, _settings, _cancel,
					new Progress<MassImportProgress>((progress) =>
					{
						this.ProcessState.OperationsCompleted = progress.AffectedArtifactIds.Count();
					})).ConfigureAwait(false);

				if (results.ExceptionDetail != null)
				{
					this.ProcessState.Exception = new BaseException(results.ExceptionDetail.ExceptionMessage);
					this.ProcessState.State = ProcessState.ProcessStateValue.CompletedWithError;
					return;
				}

				if (_cancel.IsCancellationRequested)
				{
					this.ProcessState.IsCanceled = true;
					this.ProcessState.State = ProcessState.ProcessStateValue.Canceled;
					return;
				}

				this.ProcessState.State = ProcessState.ProcessStateValue.Completed;
			}
			catch (System.Exception ex)
			{
				this.ProcessState.Exception = ex;
				this.ProcessState.State = ProcessState.ProcessStateValue.UnhandledException;
				throw;
			}
		}
	}
}