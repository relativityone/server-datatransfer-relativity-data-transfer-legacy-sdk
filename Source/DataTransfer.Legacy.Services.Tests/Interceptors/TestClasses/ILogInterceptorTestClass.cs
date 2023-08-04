// <copyright file="ILogInterceptorTestClass.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses
{
	/// <summary>
	/// ILogInterceptorTestClass.
	/// </summary>
	public interface ILogInterceptorTestClass
	{
		/// <summary>
		/// AddValues1.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		void AddValues1(int a, int b);

		/// <summary>
		/// AddValues2.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		/// <returns>int.</returns>
		int AddValues2(int a, int b);

		/// <summary>
		/// AddValues3.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		/// <returns>Task.</returns>
		Task<int> AddValues3(int a, int b);

		/// <summary>
		/// AddValues4.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		/// <param name="result">result.</param>
		/// <returns>Task.</returns>
		Task AddValues4(int a, int b, out int result);

		/// <summary>
		/// AddValues5.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		/// <returns>Task.</returns>
		Task<int> AddValues5(int a, int b);

		/// <summary>
		/// AddValues6.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		/// <returns>Task.</returns>
		Task AddValues6(int a, int b);

		/// <summary>
		/// AddValues7.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		/// <returns>Task.</returns>
		int AddValues7(int a, int b);

		/// <summary>
		/// SensitiveData.
		/// </summary>
		/// <param name="sensitiveData">sensitiveData.</param>
		/// <param name="notSensitiveData">notSensitiveData.</param>
		void SensitiveData([SensitiveData] string sensitiveData, string notSensitiveData);

		/// <summary>
		/// SensitiveDataWithModel
		/// </summary>
		/// <param name="testClass">testClass.</param>
		void SensitiveDataWithModel(SDK.ImportExport.V1.Models.ChoiceInfo testClass);

		void Array(int?[] array);

	}

	/// <inheritdoc />
	[Interceptor(typeof(LogInterceptor))]
	public class LogInterceptorTestClass : ILogInterceptorTestClass
	{
		// Lowest delay value for deterministic execution.
		private const int SleepTime = 1000;

		/// <inheritdoc />
		public void AddValues1(int addend1, int addend2)
		{
		}

		/// <inheritdoc />
		public int AddValues2(int addend1, int addend2)
		{
			return addend1 + addend2;
		}

		/// <inheritdoc />
		public Task<int> AddValues3(int addend1, int addend2)
		{
			return Task.FromResult(addend1 + addend2);
		}

		/// <inheritdoc />
		public Task AddValues4(int addend1, int addend2, out int result)
		{
			result = addend1 + addend2;

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public async Task<int> AddValues5(int addend1, int addend2)
		{
			await Task.Delay(SleepTime);

			LogInterceptorTests.FunctionExecuted = true;

			return addend1 + addend2;
		}

		/// <inheritdoc />
		public async Task AddValues6(int addend1, int addend2)
		{
			await Task.Delay(SleepTime);

			LogInterceptorTests.FunctionExecuted = true;
		}

		/// <inheritdoc />
		public int AddValues7(int addend1, int addend2)
		{
			Thread.Sleep(SleepTime);

			LogInterceptorTests.FunctionExecuted = true;

			return addend1 + addend2;
		}

		public void SensitiveData(string sensitiveData, string notSensitiveData)
		{
		}

		public void SensitiveDataWithModel(SDK.ImportExport.V1.Models.ChoiceInfo testClass)
		{
		}

		public void Array(int?[] array)
		{
		}
	}
}