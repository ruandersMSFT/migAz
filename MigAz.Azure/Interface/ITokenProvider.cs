// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Interface
{
    public interface ITokenProvider
    {
        IAccount LastAccount { get; set; }
        Task<Microsoft.Identity.Client.AuthenticationResult> GetToken(string resourceUrl, string permission, PromptBehavior promptBehavior = PromptBehavior.Auto);
        Task<Microsoft.Identity.Client.AuthenticationResult> Login(string resourceUrl, PromptBehavior promptBehavior = PromptBehavior.Auto);
    }
}

