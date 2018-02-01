using System;
using System.Collections.Generic;

namespace AutoProxy.Api
{
    public class AppSettings
    {
        public string BaseUrl { get; set; }

        public string WhiteList { get; set; }
        
        public CorsSettings Cors { get; set; }

        public AuthSettings Auth { get; set; }

        public ServerSettings Server { get; set; }

        public HeadersSettings Headers { get; set; }
    }
    
    public class ServerSettings
    {
        public bool UseIIS { get; set; }
    }
    
    public class CorsSettings
    {
        public bool Enabled { get; set; }

        public string[] Methods { get; set; }

        public string[] Origins { get; set; }

        public string[] Headers { get; set; }

        public bool Credentials { get; set; }
    }

    public class HeadersSettings
    {
        public IEnumerable<string> Remove { get; set; }

        public IEnumerable<ReplaceHeadersSettings> Replace { get; set; }
    }

    public class ReplaceHeadersSettings
    {
        public string Key { get; set; }

        public string Value { get; set; }
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
                if (Type.Equals("ntlm", StringComparison.OrdinalIgnoreCase))
                {
                    return Api.AuthType.Ntlm;
                }
                return null;
            }
        }

        public string Token { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public bool UseImpersonation { get; set; }
    }

    public enum AuthType
    {
        Bearer,
        Ntlm
    }
}