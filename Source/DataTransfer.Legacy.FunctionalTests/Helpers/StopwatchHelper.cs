using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	public class StopwatchHelper
	{
		public static async Task<T> RunWithStopwatchAsync<T>(Func<Task<T>> asyncFunc, Action<string> logAction, string description, Stopwatch stopwatch = null,
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
		{
			var watch = stopwatch ?? new Stopwatch();
			watch.Start();
			T retValue = await asyncFunc().ConfigureAwait(false);
			watch.Stop();
			logAction?.Invoke($"From: {memberName}, Description: {description}, TimeElapsed: {watch.Elapsed}, ");
			return retValue;
		}

		public static async Task RunWithStopwatchAsync(Func<Task> asyncFunc, Action<string> logAction, string description, Stopwatch stopwatch = null,
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
		{
			var watch = stopwatch ?? new Stopwatch();
			watch.Start();
			await asyncFunc().ConfigureAwait(false);
			watch.Stop();
			logAction?.Invoke($"From: {memberName}, Description: {description}, TimeElapsed: {watch.Elapsed}, ");
		}
	}
}