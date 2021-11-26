using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Relativity.MassImport.NUnit
{
	/// <summary>
	/// This class makes sure NUnit can instanciate methods with generic attributes.
	/// </summary>
	/// <example>
	/// Given the function:
	/// public void MyTestFunction&lt;T&gt;() 
	/// {
	///    ShouldThrow&lt;T&gt;();
	/// }
	/// Then you can make it work by using this attribute instead of <see cref="TestCaseAttribute"/> like so:
	/// 
	/// [GenericTestCase(typeof(ArgumentException))]
	/// public void MyTestFunction&lt;T&gt;() 
	/// {
	///    ShouldThrow&lt;T&gt;();
	/// }
	/// </example>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class GenericTestCaseAttribute : TestCaseAttribute, ITestBuilder
	{
		private readonly Type _type;

		/// <summary>
		/// Describes what generic types/parameters should go into the generic test function.
		/// </summary>
		/// <param name="type">The generic type the function should have.</param>
		/// <param name="arguments">The non generic arguments for the function.</param>
		public GenericTestCaseAttribute(Type type, params object[] arguments) : base(arguments)
		{
			_type = type;
		}

		IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
		{
			if (method.IsGenericMethodDefinition && _type != null)
			{
				var gm = method.MakeGenericMethod(_type);
				return BuildFrom(gm, suite);
			}
			return BuildFrom(method, suite);
		}
	}
}