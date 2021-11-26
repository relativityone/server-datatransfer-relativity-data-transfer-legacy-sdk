using System;
using kCura.Utility;
using Relativity.Logging;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core.Command
{
	internal abstract class ImportCommandBase
	{
		protected ILog CorrelationLogger { get; private set; }
		protected Timekeeper TimeKeeper { get; private set; }
		protected Relativity.Core.BaseContext Context { get; private set; }

		protected ImportCommandBase(ILog correlationLogger, Relativity.Core.BaseContext context, Timekeeper timeKeeper)
		{
			CorrelationLogger = correlationLogger ?? new NullLogger();
			TimeKeeper = timeKeeper ?? new Timekeeper();
			Context = context;
		}

		protected void ThrowIfDocumentLimitExceeded(Relativity.Data.MassImportOld.ObjectBase importObject)
		{
			int documentImportCount = importObject.IncomingObjectCount();
			bool willExceedLimit = WillExceedDocumentLimit(documentImportCount);
			if (willExceedLimit)
			{
				throw new System.Exception("The document import was canceled. The import would have exceeded the document limit for the workspace.");
			}
		}

		protected void ThrowIfDocumentLimitExceeded(ObjectBase importObject)
		{
			int documentImportCount = importObject.IncomingObjectCount();
			bool willExceedLimit = WillExceedDocumentLimit(documentImportCount);
			if (willExceedLimit)
			{
				throw new System.Exception("The document import was canceled. The import would have exceeded the document limit for the workspace.");
			}
		}

		private bool WillExceedDocumentLimit(int documentImportCount)
		{
			bool willExceedLimit = false;
			int workspaceId = Context.AppArtifactID;
			int currentDocCount = Relativity.Core.Service.DocumentManager.RetrieveCurrentDocumentCount(Context, workspaceId);
			int countAfterImport = currentDocCount + documentImportCount;
			int docLimit = Relativity.Core.Service.DocumentManager.RetrieveDocumentLimit(Context, workspaceId);
			if (docLimit != 0 & countAfterImport > docLimit)
			{
				willExceedLimit = true;
			}

			return willExceedLimit;
		}

		protected void Execute(Action action, string actionName)
		{
			TimeKeeper.MarkStart(actionName);
			try
			{
				action();
			}
			finally
			{
				TimeKeeper.MarkEnd(actionName);
			}
		}

		protected T Execute<T>(Func<T> func, string actionName)
		{
			T ExecuteRet = default;
			TimeKeeper.MarkStart(actionName);
			try
			{
				ExecuteRet = func();
			}
			finally
			{
				TimeKeeper.MarkEnd(actionName);
			}

			return ExecuteRet;
		}
	}
}