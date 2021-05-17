using System.Data;
using NUnit.Framework;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
    public static class DataSetAssertHelper
    {
        /// <summary>
        /// TODO: compare data sets in a better way
        /// </summary>
        public static void AreEqual(DataSet ds1, DataSet ds2)
        {
            Assert.NotNull(ds1);
            Assert.NotNull(ds2);

            Assert.AreEqual(1, ds1.Tables.Count);
            Assert.AreEqual(1, ds2.Tables.Count);

            var dt1 = ds1.Tables[0];
            var dt2 = ds2.Tables[0];

            Assert.AreEqual(dt1.Columns.Count, dt2.Columns.Count);
            Assert.AreEqual(dt1.Rows.Count, dt2.Rows.Count);
        }
    }
}
