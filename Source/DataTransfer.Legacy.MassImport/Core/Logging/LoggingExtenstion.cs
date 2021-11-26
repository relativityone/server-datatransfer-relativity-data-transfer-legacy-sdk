using System;
using System.Reflection;

namespace Relativity.Core.Service.MassImport.Logging
{
	// TODO: change to internal and correct namespace, https://jira.kcura.com/browse/REL-482642
	public static class LoggingExtenstion
	{
		private const string _DATAEXCHANGE_IMPORT_LOG_PREFIX = "DataExchange.Import.";

		public static IDisposable LogContextPushProperties(this Relativity.Logging.ILog logger, object obj)
		{
			var stackOfDisposables = new Logging.StackOfDisposables();
			var type = obj.GetType();
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo property in properties)
			{
				string propertyName = property.Name;
				string propertyValue = property.GetValue(obj, null)?.ToString() ?? string.Empty;
				var disposable = logger.LogContextPushProperty($"{_DATAEXCHANGE_IMPORT_LOG_PREFIX}{propertyName}", propertyValue);
				stackOfDisposables.Push(disposable);
			}

			return stackOfDisposables;
		}
	}
}