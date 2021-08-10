using System;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using Castle.Windsor;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace DataTransfer.Legacy.PostInstallEventHandler
{
	[kCura.EventHandler.CustomAttributes.Description("Post Install EventHandler")]
	[System.Runtime.InteropServices.Guid("36AA8C8B-BF20-4678-BC4A-FAD8E051908F")]
	[RunTarget(kCura.EventHandler.Helper.RunTargets.Instance)]
	public class PostInstallEventHandler : kCura.EventHandler.PostInstallEventHandler
	{
		public override Response Execute()
		{
			return this.ExecuteAsync().GetAwaiter().GetResult();
		}

		private async Task<Response> ExecuteAsync()
		{
			using (var container = (IWindsorContainer)PostInstallEventHandlerInstaller.CreateContainer(() => this.Helper))
			{
				var logger = container.Resolve<IAPILog>();
				logger.LogInformation("DataTransfer.Legacy PostInstallEventHandler begin.");
				try
				{
					var instanceSettingsService = container.Resolve<IInstanceSettingsService>();
					bool result = await instanceSettingsService.CreateInstanceSettingsTextType("IAPICommunicationMode", "DataTransfer.Legacy", IAPICommunicationMode.WebAPI.ToString(), "Default communication mode for import (DataTransfer.Legacy).").ConfigureAwait(false);

					logger.LogInformation("DataTransfer.Legacy PostInstallEventHandler end.");
					return new Response { Success = result };
				}
				catch (Exception exception)
				{
					logger.LogError(exception, "Error while creating DataTransfer.Legacy instance setting.");
					throw;
				}
			}
		}
	}
}