﻿using Castle.Windsor;
using Relativity.API;
using Relativity.MassImport.Api;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace DataTransfer.Legacy.MassImport.Data
{
	public static class StorageAccessProvider
	{
		private const string ServiceName = "data-transfer-legacy";
		private static IStorageAccess<string> _storageAccess;
		private static IWindsorContainer _container;
		private static readonly object _lockObject = new object();

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

			lock (_lockObject)
			{
				if (_storageAccess == null)
				{
					var serviceDetails = new ApplicationDetails(ServiceName);
					const StorageAccessPermissions permissions = StorageAccessPermissions.GenericRead;
					_storageAccess = _container.Resolve<IHelper>().GetStorageAccessorAsync(permissions, serviceDetails)
						.GetAwaiter().GetResult();
				}
			}

			return _storageAccess;
		}
	}
}