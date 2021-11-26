using System;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.NUnit.Data.SqlFramework
{
	[TestFixture]
	public class ISqlQueryPartExtensionsTests
	{
		[Test]
		public void BuildQuery_ShouldReturnCorrectString()
		{
			// arrange
			string expectedQuery = @"TEST
QUERY";
			var query = new InlineSqlQuery($"{expectedQuery}");

			// act
			var actualQuery = query.BuildQuery();

			// assert
			Assert.That(actualQuery, Is.EqualTo(expectedQuery));
		}

		[Test]
		public void BuildQuery_ShouldThrowExceptionWhenQueryIsNull()
		{
			// arrange
			ISqlQueryPart query = null;

			// act & assert
			Assert.Throws<ArgumentNullException>(() => query.BuildQuery());
		}
	}
}
