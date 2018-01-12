using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace AutoProxy.Api.Extensions
{
    public static class HttpClientExtensions
    {
        public static void AddHeaders(this HttpClient httpClient, IHeaderDictionary headers)
        {
            var authenticationHeaderValue = httpClient.DefaultRequestHeaders.Authorization;
            httpClient.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToString());
            }
            httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
        }
        
        public static HttpClient GetWithAuth(AppSettings appSettings)
        {
            if (appSettings.Auth != null)
            {
                switch (appSettings.Auth.AuthType)
                {
                    case AuthType.Bearer:
                        if (!string.IsNullOrEmpty(appSettings.Auth.Token))
                        {
                            var httpClient = new HttpClient();
                            httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", appSettings.Auth.Token);
                            return httpClient;
                        }
                        break;
                    case AuthType.Ntlm:
                        var handler = new HttpClientHandler();
                        handler.Credentials = new NetworkCredential(appSettings.Auth.User, appSettings.Auth.Password,
                            appSettings.Auth.Domain);
                        ;
                        return new HttpClient(handler);
                }
            }

            return new HttpClient();
        }
    }
}