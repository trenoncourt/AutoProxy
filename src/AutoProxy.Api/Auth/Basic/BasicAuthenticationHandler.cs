// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoProxy.Api.Auth.Basic.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using ISystemClock = Microsoft.AspNetCore.Authentication.ISystemClock;

namespace AutoProxy.Api.Auth.Basic
{
    internal class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private const string _Scheme = "Basic";

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new BasicAuthenticationEvents Events
        {
            get { return (BasicAuthenticationEvents)base.Events; }
            set { base.Events = value; }
        }

        /// <summary>
        /// Creates a new instance of the events instance.
        /// </summary>
        /// <returns>A new instance of the events instance.</returns>
        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new BasicAuthenticationEvents());

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        //protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        //{
        //    string authorizationHeader = Request.Headers["Authorization"];
        //    if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith(_Scheme + ' ', StringComparison.OrdinalIgnoreCase))
        //    {
        //        return Task.FromResult(AuthenticateResult.NoResult());
        //    }

        //    if (!Request.IsHttps && !Options.AllowInsecureProtocol)
        //    {
        //        const string insecureProtocolMessage = "Request is HTTP, Basic Authentication will not respond.";
        //        Logger.LogInformation(insecureProtocolMessage);
        //        Response.StatusCode = 500;
        //        var encodedResponseText = Encoding.UTF8.GetBytes(insecureProtocolMessage);
        //        Response.Body.Write(encodedResponseText, 0, encodedResponseText.Length);
        //    }
        //    else
        //    {
        //        Response.StatusCode = 401;

        //        var headerValue = _Scheme + $" realm=\"{Options.Realm}\"";
        //        Response.Headers.Append(HeaderNames.WWWAuthenticate, headerValue);
        //    }
        //    return Task.FromResult(AuthenticateResult.Fail(""));
        //}


        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (!Request.IsHttps && !Options.AllowInsecureProtocol)
            {
                const string insecureProtocolMessage = "Request is HTTP, Basic Authentication will not respond.";
                Logger.LogInformation(insecureProtocolMessage);
                Response.StatusCode = 500;
                var encodedResponseText = Encoding.UTF8.GetBytes(insecureProtocolMessage);
                Response.Body.Write(encodedResponseText, 0, encodedResponseText.Length);
            }
            else
            {
                Response.StatusCode = 401;

                var headerValue = _Scheme + $" realm=\"{Options.Realm}\"";
                Response.Headers.Append(HeaderNames.WWWAuthenticate, headerValue);
            }

            return Task.CompletedTask; ;
        }
    }
}