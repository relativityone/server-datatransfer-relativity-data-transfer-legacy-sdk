using System.Collections.Generic;
using System.Linq;

namespace DataTransfer.Legacy.MassImport.Extensions
{
	internal static class IEnumerableExtensions
	{
		public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
		{
			var batch = new List<T>(batchSize);
			foreach (var item in source)
			{
				if (batch.Count == batchSize)
				{
					yield return batch;
					batch = new List<T>(batchSize);
				}

				batch.Add(item);
			}
			if (batch.Any())
			{
				yield return batch;
			}
		}
	}
}