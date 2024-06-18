using NUnit.Framework;
using Relativity.Core.Service.MassImport.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.NUnit.Core.Logging
{
	[TestFixture]
	public class ImportLogContextTests
	{
		private string _runId;
		private string _action;
		private int _workspaceId;
		private ImportLogContext _context;

		[SetUp]
		public void SetUp()
		{
			_runId = "TestRunId";
			_action = "TestAction";
			_workspaceId = 123;
			_context = new ImportLogContext(_runId, _action, _workspaceId);
		}

		[Test]
		public void Constructor_ShouldInitializeProperties()
		{
			// Assert
			Assert.AreEqual(_runId, _context.RunId);
			Assert.AreEqual(_action, _context.Type);
			Assert.AreEqual(_workspaceId, _context.WorkspaceId);
		}

		[Test]
		public void Properties_ShouldBeReadOnly()
		{
			//Assert
			// Using reflection to check if the setters are private
			var runIdProperty = typeof(ImportLogContext).GetProperty(nameof(ImportLogContext.RunId));
			var typeProperty = typeof(ImportLogContext).GetProperty(nameof(ImportLogContext.Type));
			var workspaceIdProperty = typeof(ImportLogContext).GetProperty(nameof(ImportLogContext.WorkspaceId));

			Assert.IsTrue(runIdProperty.GetSetMethod(true).IsPrivate, "RunId setter is not private");
			Assert.IsTrue(typeProperty.GetSetMethod(true).IsPrivate, "Type setter is not private");
			Assert.IsTrue(workspaceIdProperty.GetSetMethod(true).IsPrivate, "WorkspaceId setter is not private");
		}
	}
}