using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AutoProxy.Api.Extensions
{
    public static class HttpClientExtensions
    {
        public static HttpClient GetWithAuth(AppSettings appSettings)
        {
            HttpClient httpClient = null;
            if (appSettings.Auth != null)
            {
                switch (appSettings.Auth.AuthType)
                {
                    case AuthType.Bearer:
                        if (!string.IsNullOrEmpty(appSettings.Auth.Token))
                        {
                            httpClient = new HttpClient();
                            httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", appSettings.Auth.Token);
                            return httpClient;
                        }
                        break;
                    case AuthType.Ntlm:
                        var handler = new HttpClientHandler();
                        if (appSettings.Auth.UseImpersonation)
                        {
                            handler.UseDefaultCredentials = true;
                        }
                        else
                        {
                            handler.Credentials = new NetworkCredential(appSettings.Auth.User, appSettings.Auth.Password,
                                appSettings.Auth.Domain);
                            handler.UseDefaultCredentials = false;
                        }
                        httpClient = new HttpClient(handler);
                        break;
                }
            }

            httpClient = httpClient ?? new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            return httpClient;
        }
    }
}