using NUnit.Framework;

namespace Relativity.MassImport.NUnit.Core
{
	public class CollationStringComparerTests
	{
		[TestCase(null, null)]
		[TestCase("", "")]
		[TestCase("Case Insensitive", "case insensitive")]
		[TestCase("E=mc2", "E=mc²")]
		[TestCase("Trailing space", "Trailing space    ")]
		[TestCase("こんにちは", "コンニチハ")]
		[TestCase("Straße", "Strasse")]
		public void StringsShouldBeEqual(string x, string y)
		{
			Relativity.MassImport.Core.CollationStringComparer comparer = Relativity.MassImport.Core.CollationStringComparer.SQL_Latin1_General_CP1_CI_AS;

			bool result = comparer.Equals(x, y);
			int hashX = comparer.GetHashCode(x);
			int hashY = comparer.GetHashCode(y);

			Assert.That(result, Is.True);
			Assert.That(hashX, Is.EqualTo(hashY));
		}

		[TestCase("", null)]
		[TestCase("Dzień dobry", "Dzien dobry")]
		public void StringsShouldNotBeEqual(string x, string y)
		{
			Relativity.MassImport.Core.CollationStringComparer comparer = Relativity.MassImport.Core.CollationStringComparer.SQL_Latin1_General_CP1_CI_AS;

			bool result = comparer.Equals(x, y);

			Assert.That(result, Is.False);
		}
	}
}
