using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.Data
{
	using Relativity.API;
	using Relativity.MassImport.Api;
	using Relativity.Storage;
	using Relativity.Storage.Extensions;
	using Relativity.Storage.Extensions.Models;

	public static class StorageAccessProvider
	{
		private const string ServiceName = "data-transfer-legacy";
		private static IStorageAccess<string> _storageAccess;
		private static IHelper _helper;

		public static void InitializeStorageAccess(IHelper helper)
		{
			_helper = helper;
		}

		public static IStorageAccess<string> GetStorageAccess()
		{
			if (_helper == null)
			{
				throw new MassImportException("Storage Access is not initialized");
			}

			if (_storageAccess == null)
			{
				var serviceDetails = new ApplicationDetails(ServiceName);
				const StorageAccessPermissions permissions = StorageAccessPermissions.GenericRead;
				_storageAccess =  _helper.GetStorageAccessorAsync(permissions, serviceDetails).GetAwaiter().GetResult();
			}

			return _storageAccess;
		}
	}
}
