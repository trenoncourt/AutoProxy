using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoProxy.AspNetCore.Extensions
{
    public static class HttpClientFactoryExtensions
    {
        public static HttpClient CreateClient(this IHttpClientFactory httpClientFactory, ProxySettings proxySettings, IServiceProvider provider)
        {
            if (proxySettings.Auth?.AuthType != AuthType.BasicToNtlm)
            {
                return httpClientFactory.CreateClient(proxySettings.Name);
            }

            var loggerFactory = provider.GetService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger("AutoProxyServiceCollectionExtensions");
            var contextAccessor = provider.GetService<IHttpContextAccessor>();
                
            string authorizationHeader = contextAccessor.HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                logger.LogWarning("{name} - Auth - BasicToNtlm - no authorization header", proxySettings.Name);
                return null;
            }
            if (!authorizationHeader.StartsWith(Constants.BasicScheme + ' ', StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("{name} - Auth - BasicToNtlm - authorization header must start with {basicScheme} but is {authorizationHeader}", proxySettings.Name, Constants.BasicScheme, authorizationHeader.Split(' ').FirstOrDefault());
                return null;
            }
            string encodedCredentials = authorizationHeader.Substring(Constants.BasicScheme.Length).Trim();
            if (string.IsNullOrEmpty(encodedCredentials))
            {
                logger.LogWarning("{name} - Auth - BasicToNtlm - credentials are empty", proxySettings.Name);
                return null;
            }
                
            string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));

            var delimiterIndex = decodedCredentials.IndexOf(':');
            if (delimiterIndex == -1)
            {
                logger.LogWarning("{name} - Auth - BasicToNtlm - credentials are malformed : {decodedCredentials}", proxySettings.Name, decodedCredentials);
                return null;
            }
                
            var username = decodedCredentials.Substring(0, delimiterIndex);
            var password = decodedCredentials.Substring(delimiterIndex + 1);
            var basicToNtlmhandler = new HttpClientHandler();
            logger.LogInformation("{name} - Auth - BasicToNtlm - credentials wellformed : {username} - {password}", proxySettings.Name, username, proxySettings.Auth.LogPassword ? password : "********");
            basicToNtlmhandler.Credentials = new NetworkCredential(username, password, proxySettings.Auth.Domain);
            basicToNtlmhandler.UseDefaultCredentials = false;
            return new HttpClient(basicToNtlmhandler) {BaseAddress = new Uri(proxySettings.BaseUrl)};
        }
    }
}