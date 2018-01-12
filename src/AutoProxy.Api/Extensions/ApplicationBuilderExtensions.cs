using Microsoft.AspNetCore.Builder;

namespace AutoProxy.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureCors(this IApplicationBuilder app, AppSettings settings)
        {
            if (settings.Cors.Enabled)
            {
                app.UseCors(builder =>
                {
                    if (settings.Cors.Headers != null)
                    {
                        builder.WithHeaders(settings.Cors.Headers.Split(';'));
                    }
                    else
                    {
                        builder.AllowAnyHeader();
                    }
                    
                    if (settings.Cors.Methods != null)
                    {
                        builder.WithMethods(settings.Cors.Methods.Split(';'));
                    }
                    else
                    {
                        builder.AllowAnyMethod();
                    }
                    
                    if (settings.Cors.Origins != null)
                    {
                        builder.WithOrigins(settings.Cors.Origins.Split(';'));
                    }
                    else
                    {
                        builder.AllowAnyOrigin();
                    }
                    
                    if (settings.Cors.Credentials)
                    {
                        builder.AllowCredentials();
                    }
                });
            }
        }
    }
}