using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoProxy.Api.Extensions;
using AutoProxy.AspNetCore.Extensions;
using AutoProxy.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AutoProxy.Api
{
    public class Program
    {
        private static IConfiguration _configuration;
        public static async Task Main()
        {
            try
            {
                // Configure configuration
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();
            
                // Configure logger
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_configuration)
                    .CreateLogger();
                Log.Information($"Starting Web host at {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")} with env {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            
                // Retreive app settings
                AppSettings appSettings = _configuration.Get<AppSettings>();
                Log.Debug("App settings used: {@appSettings}", appSettings);

                IWebHostBuilder builder = new WebHostBuilder()
                    .SuppressStatusMessages(true)
                    .UseKestrel(options => options.AddServerHeader = appSettings.Kestrel?.AddServerHeader ?? false)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseConfiguration(_configuration)
                    .UseDefaultServiceProvider((context, options) => options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
                    .UseSerilog();

                if (appSettings.Server != null && appSettings.Server.UseIIS)
                {
                    builder.UseIISIntegration();
                }

                // Configure Ntlm if needed.
                if (appSettings.Server != null && !appSettings.Server.UseIIS && appSettings.AutoProxy.Proxies.Any(p => p.Auth?.UseImpersonation == true))
                {
                    builder.UseHttpSys(options =>
                    {
                        options.Authentication.Schemes = AuthenticationSchemes.NTLM |
                                                         AuthenticationSchemes
                                                             .Negotiate;
                        options.Authentication.AllowAnonymous = false;
                    });
                }

                var host = builder
                    .ConfigureServices(services =>
                    {
                        if (appSettings.Cors != null && appSettings.Cors.Enabled)
                        {
                            services.AddCors();
                        }
                        services.AddAutoProxy(_configuration);
                    })
                    .Configure(app =>
                    {
                        app.ConfigureCors(appSettings);
                        app.UseAutoProxy();
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
