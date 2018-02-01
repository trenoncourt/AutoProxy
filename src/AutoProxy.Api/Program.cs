using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using AutoProxy.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoProxy.Api
{
    public class Program
    {
        public static void Main()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
            AppSettings appSettings = config.Get<AppSettings>();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseKestrel(options => options.AddServerHeader = false);

            if (appSettings.Server != null && appSettings.Server.UseIIS)
            {
                builder.UseIISIntegration();
            }

            if (appSettings.Server != null && !appSettings.Server.UseIIS && appSettings.Auth != null && appSettings.Auth.UseImpersonation)
            {
                builder.UseHttpSys(options =>
                {
                    options.Authentication.Schemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes.NTLM |
                                                     Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes
                                                         .Negotiate;
                    options.Authentication.AllowAnonymous = false;
                });
            }

            var host = builder
                .ConfigureLogging(loggerFactory =>
                {
                    loggerFactory.AddConfiguration(config.GetSection("Logging"));
                    loggerFactory.AddConsole();
                })
                .ConfigureServices(services =>
                {
                    if (appSettings.Cors != null && appSettings.Cors.Enabled)
                    {
                        services.AddCors();
                    }
                })
                .Configure(app =>
                {
                    app.ConfigureCors(appSettings);

                    app.Run(async context =>
                    {
                        ILogger logger = context.RequestServices.GetService<ILogger<Program>>();
                        logger.LogDebug($"Request: {context.Request.Method} {context.Request.Host.Value}{(context.Request.Path.HasValue ? context.Request.Path.Value : "")}");
                        if (context.Request.IsInWhiteList(appSettings))
                        {
                            context.Response.StatusCode = 403;
                            return;
                        }
                        
                        // Create client with auth if needed
                        HttpClient client = HttpClientExtensions.GetWithAuth(appSettings, context, logger);
                        
                        // Create request
                        HttpRequestMessage request = context.Request.ToHttpRequestMessage(appSettings);

                        HttpResponseMessage response = null;
                        // Get response
                        if (appSettings.Auth != null && appSettings.Auth.UseImpersonation)
                        {
                            var user = (WindowsIdentity)context.User.Identity;
                            
                            WindowsIdentity.RunImpersonated(user.AccessToken, () =>
                            {
                                response = client.SendAsync(request).GetAwaiter().GetResult();
                            });
                        }
                        else
                        {
                            response = await client.SendAsync(request);
                            logger.LogInformation($"End api response status: {response.StatusCode}");
                            if (!response.IsSuccessStatusCode && appSettings.Request?.RetryingTimes != null)
                            {
                                for (int i = 1; i <= appSettings.Request.RetryingTimes && !response.IsSuccessStatusCode; i++)
                                {
                                    logger.LogInformation($"Retrying {i} time...");
                                    request = context.Request.ToHttpRequestMessage(appSettings);
                                    response = await client.SendAsync(request);
                                }
                            }
                        }
                        
                        
                        // Forward response to client
                        await context.Response.SetResponse(response);
                        client.Dispose();
                    });
                })
                .Build();

            host.Run();
        }
    }
}
