using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoProxy.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace AutoProxy.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
            AppSettings appSettings = config.Get<AppSettings>();
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
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
                        var fullPath = new Uri(appSettings.BaseUrl);
                        if (context.Request.Path.HasValue)
                        {
                            if (appSettings.WhiteList != null && !appSettings.WhiteList.Split(';').Contains(context.Request.Path.Value))
                            {
                                context.Response.StatusCode = 403;
                                return;
                            }
                            fullPath = new Uri(fullPath, context.Request.Path.Value);
                        }
                        
                        // Prepare builder.
                        var url = fullPath.AbsoluteUri;
                        HttpClient cli = new HttpClient();

                        // Add Auth if needed.
                        if (appSettings.Auth != null)
                        {
                            switch (appSettings.Auth.AuthType)
                            {
                                case AuthType.Bearer:
                                    if (!string.IsNullOrEmpty(appSettings.Auth.Token))
                                    {
                                        cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appSettings.Auth.Token);
                                    }
                                    break;
                            }
                        }
                        Task<HttpResponseMessage> responseMessageTask = null;
                        if (context.Request.Body != null)
                        {
                            //string json = context.ReadBodyAsJson();
                            switch (context.Request.Method)
                            {
                                case "GET":

                                    responseMessageTask = cli.GetAsync(url);//  url.GetAsync();
                                    break;
                                case "POST":
                                    string contentTypeHeader = context.Request.Headers["Content-Type"];
                                    var streamContent = new StreamContent(context.Request.Body);
                                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(contentTypeHeader) ? "text/plain" : contentTypeHeader);
                                    responseMessageTask = cli.PostAsync(url, streamContent);//.PostJsonAsync(json);
                                    break;
                                case "PUT":
                                    break;
                                case "PATCH":
                                    break;
                            }
                            
                        }
                        if (responseMessageTask != null)
                        {
                            HttpResponseMessage res = await responseMessageTask;
                            foreach (var header in res.Content.Headers)
                            {
                                context.Response.Headers[header.Key] = new StringValues(header.Value.ToArray());
                            }
                            context.Response.Body = await res.Content.ReadAsStreamAsync();
                            context.Response.StatusCode = (int)res.StatusCode;
                            cli.Dispose();
                        }
                    });
                })
                .Build();

            host.Run();
        }
    }
}
