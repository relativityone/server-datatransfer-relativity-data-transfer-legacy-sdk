using System.Collections.Generic;
using Moq;
using Relativity.Toggles;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class SettingsHelper
	{
		private const int MASS_IMPORT_SQL_TIME_OUT_DEFAULT_VALUE = 60;
		private const bool ENFORCE_DOCUMENT_LIMIT_DEFAULT_VALUE = false;
		private const int DOCUMENT_LIMIT_DEFAULT_VALUE = 0;
		private const int MASS_MOVE_BATCH_AMOUNT = 10;

		private static readonly Dictionary<string, object> ConfigSettings = new Dictionary<string, object>();

		private static readonly Mock<IToggleProvider> ToggleProviderMock = new Mock<IToggleProvider>();

		public static void SetDefaultSettings()
		{
			SetMassImportSqlTimeOut(MASS_IMPORT_SQL_TIME_OUT_DEFAULT_VALUE);
			SetEnforceDocumentLimit(ENFORCE_DOCUMENT_LIMIT_DEFAULT_VALUE);
			SetDefaultDocumentLimit(DOCUMENT_LIMIT_DEFAULT_VALUE);
			SetMassMoveBatchAmount(MASS_MOVE_BATCH_AMOUNT);
		}

		public static void SetToggle<T>(bool toggleValue) where T : IToggle
		{
			ToggleProviderMock.Setup(x => x.IsEnabled<T>()).Returns(toggleValue);
			ToggleProvider.Current = ToggleProviderMock.Object;
		}

		public static void SetMassImportSqlTimeOut(int valueInSeconds)
		{
			ConfigSettings["MassImportSqlTimeout"] = valueInSeconds;
			InjectConfigSettings(ConfigSettings);
		}

		public static void SetEnforceDocumentLimit(bool value)
		{
			ConfigSettings["EnforceDocumentLimit"] = value;
			InjectConfigSettings(ConfigSettings);
		}

		public static void SetDefaultDocumentLimit(int value)
		{
			ConfigSettings["DefaultDocumentLimit"] = value;
			InjectConfigSettings(ConfigSettings);
		}

		public static void SetMassMoveBatchAmount(int value)
		{
			ConfigSettings["MassMoveBatchAmount"] = value;
			InjectConfigSettings(ConfigSettings);
		}

		private static void InjectConfigSettings(Dictionary<string, object> settings)
		{
			Relativity.Core.Config.InjectConfigSettings(settings);
			Relativity.Data.Config.InjectConfigSettings(settings);
		}
	}
}