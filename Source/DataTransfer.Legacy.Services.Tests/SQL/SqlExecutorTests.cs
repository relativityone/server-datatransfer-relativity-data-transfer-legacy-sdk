using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Tests.SQL
{
	[TestFixture]
	public class SqlExecutorTests
	{
		private Mock<ISqlExecutor> _sqlExecutorMock;
		private const int TestWorkspaceId = 1;
		private const string TestQuery = "SELECT * FROM TestTable";
		private List<SqlParameter> TestParameters = new List<SqlParameter> { new SqlParameter("@param1", "value1") };

		[SetUp]
		public void SetUp()
		{
			_sqlExecutorMock = new Mock<ISqlExecutor>();

			_sqlExecutorMock.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<int>(), It.IsAny<string>())).Returns(1);
			_sqlExecutorMock.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>())).Returns(1);
			_sqlExecutorMock.Setup(x => x.ExecuteReader(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>(), It.IsAny<Func<IDataRecord, int>>())).Returns(new List<int> { 1 });
		}

		[Test]
		public void ExecuteNonQuerySQLStatement_WithoutParameters_ReturnsExpectedValue()
		{
			var result = _sqlExecutorMock.Object.ExecuteNonQuerySQLStatement(TestWorkspaceId, TestQuery);
			Assert.AreEqual(1, result);
		}

		[Test]
		public void ExecuteNonQuerySQLStatement_WithParameters_ReturnsExpectedValue()
		{
			var result = _sqlExecutorMock.Object.ExecuteNonQuerySQLStatement(TestWorkspaceId, TestQuery, TestParameters);
			Assert.AreEqual(1, result);
		}

		[Test]
		public void ExecuteReader_ReturnsExpectedValue()
		{
			var result = _sqlExecutorMock.Object.ExecuteReader(TestWorkspaceId, TestQuery, TestParameters, record => (int)record[0]);
			Assert.AreEqual(1, result[0]);
		}
	}
}