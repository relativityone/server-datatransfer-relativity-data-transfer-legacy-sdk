namespace DataTransfer.Legacy.MassImport.Data
{
	using Castle.Windsor;
	using Relativity.API;
	using Relativity.MassImport.Api;
	using Relativity.Storage;
	using Relativity.Storage.Extensions;
	using Relativity.Storage.Extensions.Models;

	public static class StorageAccessProvider
	{
		private const string ServiceName = "data-transfer-legacy";
		private static IStorageAccess<string> _storageAccess;
		private static IWindsorContainer _container;

		public static void InitializeStorageAccess(IWindsorContainer container)
		{
			_container = container;
		}

		public static IStorageAccess<string> GetStorageAccess()
		{
			if (_container == null)
			{
				throw new MassImportException("Storage Access is not initialized");
			}

			if (_storageAccess == null)
			{
				var serviceDetails = new ApplicationDetails(ServiceName);
				const StorageAccessPermissions permissions = StorageAccessPermissions.GenericRead;
				_storageAccess =  _container.Resolve<IHelper>().GetStorageAccessorAsync(permissions, serviceDetails).GetAwaiter().GetResult();
			}

			return _storageAccess;
		}
	}
}
