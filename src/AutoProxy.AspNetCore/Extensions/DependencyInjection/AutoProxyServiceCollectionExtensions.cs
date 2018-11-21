using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AutoProxy.AspNetCore.Extensions.DependencyInjection
{
    public static class AutoProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddAutoProxy(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var autoProxySection = configuration.GetSection("AutoProxy");
            var autoProxySettings = autoProxySection.Get<AutoProxySettings>();
            
            services.Configure<AutoProxySettings>(autoProxySection);
            foreach (var proxy in autoProxySettings.Proxies)
            {
                if (proxy.Auth?.AuthType == AuthType.BasicToNtlm)
                {
                    continue;
                }
                var builder = services.AddHttpClient(proxy.Name, (provider, client) =>
                {
                    client.BaseAddress = new Uri(proxy.BaseUrl);
                    client.DefaultRequestHeaders.Clear();
                    if (proxy.Auth?.AuthType == AuthType.Bearer && !string.IsNullOrEmpty(proxy.Auth.Token))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", proxy.Auth.Token);
                    }
                });
                if (proxy.Auth?.AuthType == AuthType.Ntlm)
                {
                    if (proxy.Auth.UseImpersonation)
                    {          
                        builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            UseDefaultCredentials = true
                        });
                    }
                    else
                    {  
                        builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            Credentials = new NetworkCredential(proxy.Auth.User, proxy.Auth.Password, proxy.Auth.Domain),
                            UseDefaultCredentials = false
                        });
                    }
                }
            }
            
            return services;
        }
    }
}