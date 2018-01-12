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
using System.Text.RegularExpressions;

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
                        string path = "/";
                        if (context.Request.Path.HasValue)
                        {
                            path = context.Request.Path.Value;
                        }

                        if (appSettings.WhiteList != null && !appSettings.WhiteList.Split(';').Any(template => Regex.IsMatch(path, template)))
                        {
                            context.Response.StatusCode = 403;
                            return;
                        }
                        var fullPath = new Uri(new Uri(appSettings.BaseUrl), path);

                        // Prepare builder.
                        var url = fullPath.AbsoluteUri;
                        if (context.Request.QueryString.HasValue)
                        {
                            url += context.Request.QueryString.Value;
                        }
                        HttpClient cli = new HttpClient();

                        // Forward headers
                        cli.AddHeaders(context.Request.Headers);

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
                            switch (context.Request.Method)
                            {
                                case "GET":
                                    responseMessageTask = cli.GetAsync(url);
                                    break;
                                case "POST":
                                    string contentTypeHeader = context.Request.Headers["Content-Type"];
                                    var streamContent = new StreamContent(context.Request.Body);
                                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(contentTypeHeader) ? "text/plain" : contentTypeHeader);
                                    responseMessageTask = cli.PostAsync(url, streamContent);
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
                            context.Response.StatusCode = (int)res.StatusCode;
                            Stream responseBufferStream = await res.Content.ReadAsStreamAsync();
                            await responseBufferStream.CopyToAsync(context.Response.Body);
                            cli.Dispose();
                        }
                    });
                })
                .Build();

            host.Run();
        }
    }
}
