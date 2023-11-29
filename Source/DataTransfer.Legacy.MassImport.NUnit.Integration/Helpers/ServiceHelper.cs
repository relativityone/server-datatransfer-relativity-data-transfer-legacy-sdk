using System;
using System.Net;
using Relativity.Services.ServiceProxy;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class ServiceHelper
	{
		/// <summary>
		/// Retrieve a service proxy.
		/// </summary>
		/// <typeparam name="T">
		/// The type of service to retrieve.
		/// </typeparam>
		/// <param name="parameters">
		/// The integration test parameters.
		/// </param>
		/// <returns>
		/// The <typeparamref name="T"/> instance.
		/// </returns>
		public static T GetServiceProxy<T>(IntegrationTestParameters parameters)
			where T : class, IDisposable
		{
			if (parameters == null)
			{
				throw new ArgumentNullException(nameof(parameters));
			}

			Credentials credentials = new UsernamePasswordCredentials(parameters.RelativityUserName, parameters.RelativityPassword);
			ServiceFactorySettings serviceFactorySettings = new ServiceFactorySettings(
				new Uri(parameters.RelativityRestUrl),
				credentials)
			{
				ProtocolVersion = Relativity.Services.Pipeline
					.WireProtocolVersion.V2,
			};
			ServiceFactory serviceFactory =
				new ServiceFactory(serviceFactorySettings);
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
													| SecurityProtocolType.Tls
													| SecurityProtocolType.Tls11
													| SecurityProtocolType.Tls12;
			T proxy = serviceFactory.CreateProxy<T>();
			return proxy;
		}
	}
}