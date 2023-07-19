using System;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class TransactionHelper
	{
		public static async Task<T> WrapInTransaction<T>(Func<Task<T>> taskProvider, Context context)
		{
			T result;
			await context.BeginTransactionAsync();

			try
			{
				result = await taskProvider().ConfigureAwait(false);

				context.CommitTransaction();
			}
			catch (Exception)
			{
				context.RollbackTransaction();

				throw;
			}

			return result;
		}
	}
}
