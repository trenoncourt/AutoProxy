using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AutoProxy.Api.Auth.Basic.Events
{
    public class AuthenticationFailedContext : ResultContext<BasicAuthenticationOptions>
    {
        public AuthenticationFailedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            BasicAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        public Exception Exception { get; set; }
    }
}
