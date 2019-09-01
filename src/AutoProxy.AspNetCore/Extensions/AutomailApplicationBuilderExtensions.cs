using System.Net.Http;
using System.Security.Principal;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoProxy.AspNetCore.Extensions
{
    public static class AutoProxyApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAutoProxy(this IApplicationBuilder builder)
        {
            var settings = builder.ApplicationServices.GetService<IOptions<AutoProxySettings>>().Value;
            
            return builder.Map(settings.Path == null ? null : $"/{settings.Path.TrimStart('/')}", app =>
            {
                foreach (var proxy in settings.Proxies)
                {
                    app.Map(proxy.Path == null ? null : $"/{proxy.Path.TrimStart('/')}", sub =>
                    {
                        sub.Use(async (context, func) =>
                        {
                            var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                            ILogger logger = loggerFactory.CreateLogger("AutoProxyApplicationBuilderExtensions");
                            logger.LogDebug($"Request: {context.Request.Method} {context.Request.Host.Value}{(context.Request.Path.HasValue ? context.Request.Path.Value : "")}");
                            if (context.Request.Method == HttpMethod.Options.Method)
                            {
                                return;
                            }

                            if (context.Request.IsInWhiteList(proxy))
                            {
                                logger.LogWarning("{name} - Forbidden, url {url} must be in whitelist", proxy.Name, context.Request.GetPath());
                                context.Response.StatusCode = 403;
                                return;
                            }

                            var httpClientFactory = context.RequestServices.GetService<IHttpClientFactory>();

                            // Create client with auth if needed
                            var client = httpClientFactory.CreateClient(proxy, context.RequestServices);
                            if (client == null)
                            {
                                context.Response.StatusCode = 403;
                                return;
                            }

                            // Create request
                            HttpRequestMessage request = context.Request.ToHttpRequestMessage(proxy);

                            HttpResponseMessage response = null;
                            // Get response
                            if (proxy.Auth != null && proxy.Auth.UseImpersonation)
                            {
                                var user = (WindowsIdentity) context.User.Identity;

                                WindowsIdentity.RunImpersonated(user.AccessToken, () =>
                                {
                                    response = client.SendAsync(request).GetAwaiter().GetResult();
                                    client.Dispose();
                                });
                            }
                            else
                            {
                                response = await client.SendAsync(request);
                                logger.LogInformation("{name} - End api response status: {statusCode}", proxy.Name, response.StatusCode);
                                if (!response.IsSuccessStatusCode && proxy.Request?.RetryingTimes != null)
                                {
                                    for (int i = 1; i <= proxy.Request.RetryingTimes && !response.IsSuccessStatusCode; i++)
                                    {
                                        logger.LogInformation("{name} - Retrying {i} time...", proxy.Name, i); // Todo : use polly
                                        request = context.Request.ToHttpRequestMessage(proxy);
                                        response = await client.SendAsync(request);
                                    }
                                }

                                client.Dispose();
                            }

                            // Forward response to client
                            await context.Response.SetResponse(response);
                        });
                    });
                }
            });
        }
    }
}