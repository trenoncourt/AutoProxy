﻿using System;
using System.Collections.Generic;
using AutoProxy.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace AutoProxy.Api
{
    public class AppSettings
    {
        public string BaseUrl { get; set; }

        public string WhiteList { get; set; }
        
        public KestrelServerOptions Kestrel { get; set; }
        
        public CorsSettings Cors { get; set; }

        public ServerSettings Server { get; set; }

        public AutoProxySettings AutoProxy { get; set; }
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
                if (Type.Equals("basicToNtlm", StringComparison.OrdinalIgnoreCase))
                {
                    return Api.AuthType.BasicToNtlm;
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

    public class RequestSettings
    {
        public int? RetryingTimes { get; set; }
    }

    public enum AuthType
    {
        Bearer,
        Ntlm,
        BasicToNtlm
    }
}