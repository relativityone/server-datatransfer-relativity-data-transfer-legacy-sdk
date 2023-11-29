using System;
using System.Net;
using kCura.WinEDDS.Service;
using NUnit.Framework;
using Relativity.Testing.Framework;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Wrappers
{
    public class WebApiServiceWrapper
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _webApiUrl;
        private readonly int _webApiTimeoutMs;

        public WebApiServiceWrapper()
        {
            _username = RelativityFacade.Instance.Config.RelativityInstance.AdminUsername;
            _password = RelativityFacade.Instance.Config.RelativityInstance.AdminPassword;
            _webApiUrl = $"{RelativityFacade.Instance.Config.RelativityInstance.ServerBindingType}://{RelativityFacade.Instance.Config.RelativityInstance.WebApiHostAddress}/RelativityWebApi/";
            _webApiTimeoutMs = Settings.DefaultTimeOut;

            TestContext.WriteLine($"Use WebApiServiceWrapper with username={_username}, webServiceUrl={_webApiUrl}, webApiTimeoutMs={_webApiTimeoutMs}");
        }

        public void PerformDataRequest<T>(Action<T> action) where T : IDisposable
        {
            try
            {
                var credentials = new NetworkCredential(_username, _password);
                var cookieContainer = new CookieContainer();

                using (var userManager = new UserManager(credentials, cookieContainer, _webApiUrl))
                {
                    userManager.Login(_username, _password);
                }

                var managerType = typeof(T);

                using (var manager = (T)Activator.CreateInstance(managerType))
                {
                    SetInstanceProperty(manager, "Credentials", credentials);
                    SetInstanceProperty(manager, "CookieContainer", cookieContainer);
                    SetInstanceProperty(manager, "Url", $"{_webApiUrl}{managerType.Name}.asmx");
                    SetInstanceProperty(manager, "Timeout", _webApiTimeoutMs);

                    action(manager);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Exception occurred when retrieving data from RelativityWebApi endpoint: {ex}");
                throw;
            }
        }

        private static void SetInstanceProperty<T>(T instance, string propertyName, object propertyValue)
        {
            var property = typeof(T).GetProperty(propertyName);
            property?.SetValue(instance, propertyValue);
        }
    }
}
