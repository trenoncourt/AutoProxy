using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AutoProxy.Api.Extensions
{
    public static class HttpClientExtensions
    {
        private const string BasicSheme = "Basic";

        public static HttpClient GetWithAuth(AppSettings appSettings, HttpContext context, ILogger logger)
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
                    case AuthType.BasicToNtlm:
                        string authorizationHeader = context.Request.Headers["Authorization"];
                        if (string.IsNullOrEmpty(authorizationHeader))
                        {
                            break;
                            // todo logs
                        }
                        if (!authorizationHeader.StartsWith(BasicSheme + ' ', StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                            // todo logs
                        }
                        string encodedCredentials = authorizationHeader.Substring(BasicSheme.Length).Trim();
                        if (string.IsNullOrEmpty(encodedCredentials))
                        {
                            break;
                            // todo logs
                        }
                        string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));

                        var delimiterIndex = decodedCredentials.IndexOf(':');
                        if (delimiterIndex == -1)
                        {
                            break;
                            // todo logs
                        }

                        var username = decodedCredentials.Substring(0, delimiterIndex);
                        var password = decodedCredentials.Substring(delimiterIndex + 1);

                        var basicToNtlmhandler = new HttpClientHandler();
                        logger.LogInformation($"AUTH - Basic to ntlm - credentials : {username} - {password}");
                        basicToNtlmhandler.Credentials = new NetworkCredential(username, password, appSettings.Auth.Domain);
                        basicToNtlmhandler.UseDefaultCredentials = false;
                        httpClient = new HttpClient(basicToNtlmhandler);
                        break;
                }
            }

            httpClient = httpClient ?? new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            return httpClient;
        }
    }
}