using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Data.MassImportOld;
using Relativity.Data.MassImportOld.DataGridWriteStrategy;
using Relativity.DataGrid;

namespace Relativity.MassImport.NUnit.Data.DataGeidWriteStrategy
{
	[TestFixture]
	public class ByteMeasuringWriterTests
	{
		[Test]
		public async Task OneDoc_FourFields_DGFS_CheckSum()
		{
			// arrange
			var doc = new DataGridWriteResult()
			{
				DataGridID = "1",
				ResultStatus = DataGridResult.Status.Unverified,
				FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
				{
					FieldOfSize(10),
					FieldOfSize(100),
					FieldOfSize(1000),
					FieldOfSize(10000)
				}
			};
			
			var fakeWriter = new Mock<IDataGridWriter>();
			fakeWriter.Setup(writer => writer.Write(It.IsAny<IEnumerable<IDataGridRecord>>())).ReturnsAsync(new[] { doc });
			var measurements = new ImportMeasurements();
			var testee = new ByteMeasuringWriter(fakeWriter.Object, measurements);

			// act
			await testee.Write(null);

			// assert
			Assert.AreEqual(11110, measurements.DataGridFileSize);
		}

		[Test]
		public async Task TwoDocs_TwoFields_CheckSum()
		{
			// arrange
			var docs = new List<DataGridWriteResult>()
			{
				{
					new DataGridWriteResult()
					{
						DataGridID = "1",
						ResultStatus = DataGridResult.Status.Unverified,
						FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
						{
							FieldOfSize(100), 
							FieldOfSize(10000)
						}
					}
				},
				{
					new DataGridWriteResult()
					{
						DataGridID = "1",
						ResultStatus = DataGridResult.Status.Unverified,
						FieldWriteResults = new List<DataGridWriteResult.FieldResult>()
						{
							FieldOfSize(10), 
							FieldOfSize(1000)
						}
					}
				}
			};

			var fakeWriter = new Mock<IDataGridWriter>();
			fakeWriter.Setup(writer => writer.Write(It.IsAny<IEnumerable<IDataGridRecord>>())).ReturnsAsync(docs);
			var measurements = new ImportMeasurements();
			var testee = new ByteMeasuringWriter(fakeWriter.Object, measurements);

			// act
			await testee.Write(null);

			// assert
			Assert.AreEqual(11110, measurements.DataGridFileSize);
		}

		private DataGridWriteResult.FieldResult FieldOfSize(int byteSize)
		{
			return new DataGridWriteResult.FieldResult()
			{
				FieldNamespace = "Fields",
				FieldIdentifier = Guid.NewGuid().ToString(),
				ResultStatus = DataGridResult.Status.Unverified,
				FieldByteSize = byteSize
			};
		}
	}
}