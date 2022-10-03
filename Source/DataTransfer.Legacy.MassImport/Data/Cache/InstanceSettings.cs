namespace DataTransfer.Legacy.MassImport.Data.Cache
{
	internal static class InstanceSettings
	{
		private const int MASS_IMPORT_SQL_TIMEOUT_DEFAULT_VALUE = 240;

		public static int MassImportSqlTimeout => Relativity.Data.Config.MassImportSqlTimeout > 0 ? Relativity.Data.Config.MassImportSqlTimeout : MASS_IMPORT_SQL_TIMEOUT_DEFAULT_VALUE;
	}
}
