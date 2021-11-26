using System;
using System.Text;
using NUnit.Framework;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.NUnit.Data.SqlFramework
{
	[TestFixture]
	public class IfQueryTests
	{
		private readonly ISqlQueryPart condition = new InlineSqlQuery($"Condition");
		private readonly ISqlQueryPart falsePart = new InlineSqlQuery($"False");
		private readonly ISqlQueryPart truePart = new InlineSqlQuery($"True");

		[Test]
		public void ShouldThrowExceptionWhenConditionIsNull()
		{
			// act & assert
			Assert.That(() => new IfQuery(condition: null, truePart, falsePart), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void ShouldThrowExceptionWhenTruePartIsNull()
		{
			// act & assert
			Assert.That(() => new IfQuery(condition, truePart: null, falsePart), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void ShouldThrowExceptionWhenFalsePartIsNull()
		{
			// act & assert
			Assert.That(() => new IfQuery(condition, truePart, falsePart: null), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void ShouldBuildQuery()
		{
			// arrange
			var sut = new IfQuery(condition, truePart, falsePart);

			var queryBuilder = new StringBuilder();

			// act
			sut.WriteTo(queryBuilder);

			// assert
			var actualQuery = queryBuilder.ToString();
			string expectedQuery = @"IF Condition
BEGIN
True
END
ELSE
BEGIN
False
END
";
			Assert.That(actualQuery, Is.EqualTo(expectedQuery));
		}
	}
}
