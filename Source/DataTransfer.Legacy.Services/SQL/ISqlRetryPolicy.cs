using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.SQL
{
	public interface ISqlRetryPolicy
	{
		Task<T> ExecuteAsync<T>(Func<Task<T>> function, [CallerMemberName] string operationName = "");

		Task ExecuteAsync(Func<Task> function, [CallerMemberName] string operationName = "");

		void Execute(Action function, [CallerMemberName] string operationName = "");

		T Execute<T>(Func<T> function, [CallerMemberName] string operationName = "");
	}
}