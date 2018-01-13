using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using AutoProxy.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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

            var host = new WebHostBuilder()
                .UseKestrel(options => options.AddServerHeader = false)
                .ConfigureLogging(loggerFactory =>
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                        loggerFactory.AddConsole();
                })
                .ConfigureServices(services =>
                {
                    if (appSettings.Cors.Enabled)
                    {
                        services.AddCors();
                    }
                })
                .Configure(app =>
                {
                    app.ConfigureCors(appSettings);

                    app.Run(async context =>
                    {
                        if (context.Request.IsInWhiteList(appSettings))
                        {
                            context.Response.StatusCode = 403;
                            return;
                        }
                        
                        // Create client with auth if needed
                        HttpClient client = HttpClientExtensions.GetWithAuth(appSettings);
                        
                        // Create request
                        HttpRequestMessage request = context.Request.ToHttpRequestMessage(appSettings);
                        
                        // Get response
                        HttpResponseMessage response = await client.SendAsync(request);
                        
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
