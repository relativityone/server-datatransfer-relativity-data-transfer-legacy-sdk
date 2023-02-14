using System;
using kCura.Utility;
using Relativity.Logging;

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