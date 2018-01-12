using System;

namespace AutoProxy.Api
{
    public class AppSettings
    {
        public string BaseUrl { get; set; }

        public string WhiteList { get; set; }
        
        public CorsSettings Cors { get; set; }

        public AuthSettings Auth { get; set; }
    }
    
    public class CorsSettings
    {
        public bool Enabled { get; set; }

        public string Methods { get; set; }

        public string Origins { get; set; }

        public string Headers { get; set; }

        public bool Credentials { get; set; }
    }

    public class AuthSettings
    {
        public string Type { get; set; }

        public AuthType? AuthType
        {
            get
            {
                if (Type.Equals("bearer", StringComparison.OrdinalIgnoreCase))
                {
                    return Api.AuthType.Bearer;
                }
                return null;
            }
        }

        public string Token { get; set; }
    }

    public enum AuthType
    {
        Bearer
    }
}