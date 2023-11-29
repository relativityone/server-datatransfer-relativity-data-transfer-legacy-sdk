using System;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class BaseContextFacadeTests
	{
		private Mock<BaseContext> _baseContextMock;
		private BaseContextFacade _sut;

		[SetUp]
		public void SetUp()
		{
			_baseContextMock = new Mock<BaseContext>();
			_sut = new BaseContextFacade(_baseContextMock.Object);
		}

		[Test]
		public void Constructor_ThrowsException_WhenBaseContextIsNull()
		{
			// act & assert
			Assert.That(() => new BaseContextFacade(null), Throws.Exception.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallBaseContext_WithQueryAndTimeout()
		{
			// arrange
			string query = "QUERY";
			int timeout = 232;

			// act
			_sut.ExecuteSQLStatementAsReader(query, timeout);

			// assert
			_baseContextMock.Verify(x=>x.ExecuteSQLStatementAsReader(query, timeout));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallBaseContext_WithQueryAndParametersAndTimeout()
		{
			// arrange
			string query = "QUERY";
			SqlParameter[] parameters =
			{
				new SqlParameter("a", 1),
				new SqlParameter("b", "c")
			};
			int timeout = 232;

			// act
			_sut.ExecuteSQLStatementAsReader(query, parameters, timeout);

			// assert
			_baseContextMock.Verify(x => x.ExecuteSQLStatementAsReader(query, parameters, timeout));
		}

		[Test]
		public void ReleaseConnection_CallBaseContext()
		{
			// act
			_sut.ReleaseConnection();

			// assert
			_baseContextMock.Verify(x => x.ReleaseConnection());
		}
	}
}
