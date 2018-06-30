﻿// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AutoProxy.Api.Auth.Basic.Events
{
    public class ValidateCredentialsContext : ResultContext<BasicAuthenticationOptions>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ValidateCredentialsContext"/>.
        /// </summary>
        /// <param name="context">The HttpContext the validate context applies too.</param>
        /// <param name="scheme">The scheme used when the Basic Authentication handler was registered.</param>
        /// <param name="options">The <see cref="BasicAuthenticationOptions"/> for the instance of
        /// <see cref="BasicAuthenticationMiddleware"/> creating this instance.</param>
        /// <param name="ticket">Contains the intial values for the identit.</param>
        public ValidateCredentialsContext(
            HttpContext context,
            AuthenticationScheme scheme,
            BasicAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        /// <summary>
        /// The user name to validate.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password to validate.
        /// </summary>
        public string Password { get; set; }
    }
}
