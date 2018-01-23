using Microsoft.AspNetCore.Builder;

namespace AutoProxy.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureCors(this IApplicationBuilder app, AppSettings settings)
        {
            if (settings.Cors != null && settings.Cors.Enabled)
            {
                app.UseCors(builder =>
                {
                    if (settings.Cors.Headers != null)
                    {
                        builder.WithHeaders(settings.Cors.Headers);
                    }
                    else
                    {
                        builder.AllowAnyHeader();
                    }
                    
                    if (settings.Cors.Methods != null)
                    {
                        builder.WithMethods(settings.Cors.Methods);
                    }
                    else
                    {
                        builder.AllowAnyMethod();
                    }
                    
                    if (settings.Cors.Origins != null)
                    {
                        builder.WithOrigins(settings.Cors.Origins);
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