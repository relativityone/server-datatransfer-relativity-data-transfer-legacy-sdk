using System;
using System.Text.RegularExpressions;

namespace Relativity.MassImport.NUnit.TestHelpers
{
	public static class StringExtensions
	{
		public static string RemoveWhitespaces(this string x)
		{
			x = x ?? throw new ArgumentNullException(nameof(x));
			return Regex.Replace(x, @"\s", "");
		}

		public static bool IsEqualIgnoringWhitespaces(this string actual, string expected)
		{
			var normalizedResult = actual.RemoveWhitespaces();
			var normalizedExpectedResult = expected.RemoveWhitespaces();

			return normalizedResult == normalizedExpectedResult;
		}
	}
}
