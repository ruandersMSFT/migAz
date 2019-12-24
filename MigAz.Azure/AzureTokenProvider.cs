// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MigAz.Azure.Interface;
using MigAz.Azure.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MigAz.Azure.ADAL;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;

namespace MigAz.Azure
{
    public class AzureTokenProvider : ITokenProvider
    {
        private const string strReturnUrl = "urn:ietf:wg:oauth:2.0:oob";
        private const string strClientId = "cce38d81-9267-40c6-89b0-8983186075c2";
        private ILogProvider _LogProvider;
        private AzureEnvironment _AzureEnvironment;
        private AuthenticationContext _AuthenticationContext;
        private Dictionary<Guid, AuthenticationContext> _TenantAuthenticationContext = new Dictionary<Guid, AuthenticationContext>();
        private IAccount _LastAccount;
        IPublicClientApplication app;

        private AzureTokenProvider() { }

        public AzureTokenProvider(AzureEnvironment azureEnvironment, ILogProvider logProvider)
        {
            _AzureEnvironment = azureEnvironment;
            _AuthenticationContext = new AuthenticationContext(_AzureEnvironment.ActiveDirectoryEndpoint + _AzureEnvironment.AdTenant);
            _LogProvider = logProvider;
        }

        public AzureEnvironment AzureEnvironment
        {
            get { return _AzureEnvironment; }
        }

        public IAccount LastAccount
        {
            get { return _LastAccount; }
            set { _LastAccount = value; }
        }

        private AuthenticationContext GetTenantAuthenticationContext(Guid tenantGuid)
        {
            if (tenantGuid == Guid.Empty)
                return _AuthenticationContext;

            if (_TenantAuthenticationContext.Keys.Contains(tenantGuid))
                return _TenantAuthenticationContext[tenantGuid];
            else
            {
                AuthenticationContext tenantAuthenticationContext = new AuthenticationContext(_AzureEnvironment.ActiveDirectoryEndpoint + tenantGuid.ToString() + "/");
                _TenantAuthenticationContext.Add(tenantGuid, tenantAuthenticationContext);
                return tenantAuthenticationContext;
            }
        }

        public async Task<Microsoft.Identity.Client.AuthenticationResult> GetToken(string resourceUrl, Guid azureAdTenantGuid, PromptBehavior promptBehavior = PromptBehavior.Auto)
        {
            _LogProvider.WriteLog("GetToken", "Start token request : Azure AD Tenant ID " + azureAdTenantGuid.ToString());
            _LogProvider.WriteLog("GetToken", " - Resource Url: " + resourceUrl);
            _LogProvider.WriteLog("GetToken", " - Azure AD Tenant Guid: " + azureAdTenantGuid.ToString());
            _LogProvider.WriteLog("GetToken", " - Prompt Behavior: " + promptBehavior.ToString());
            if (_LastAccount == null)
            {
                _LogProvider.WriteLog("GetToken", " - Required User: N/A");
            }
            else
            {
                _LogProvider.WriteLog("GetToken", " - Required User: " + _LastAccount.Username);
            }

            string[] scopes = new string[] { resourceUrl + "user_impersonation" };

            if (app == null)
            {

                PublicClientApplicationOptions options = new PublicClientApplicationOptions();
                options.AzureCloudInstance = this.AzureEnvironment.GetAzureCloudInstance();
                options.ClientId = strClientId;
                options.RedirectUri = strReturnUrl;

                //if (azureAdTenantGuid != Guid.Empty)
                //    options.TenantId = azureAdTenantGuid.ToString();
                //else
                options.AadAuthorityAudience = AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount;

                this.app = PublicClientApplicationBuilder
                    .CreateWithApplicationOptions(options)
                    .WithRedirectUri("http://localhost")
                    .Build();

            }

            var accounts = await app.GetAccountsAsync();

            Microsoft.Identity.Client.AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                DefaultOsBrowserWebUi asdf = new DefaultOsBrowserWebUi();

                result = await app.AcquireTokenInteractive(scopes)
                    .WithCustomWebUi(asdf)
                    .ExecuteAsync();

            }

            if (result == null)
                _LastAccount = null;
            else
                _LastAccount = result.Account;

            //AuthenticationContext tenantAuthenticationContext = GetTenantAuthenticationContext(azureAdTenantGuid);

            //PlatformParameters platformParams = new PlatformParameters(promptBehavior, null);
            //Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authenticationResult;

            //if (_LastAccount == null)
            //{
            //    authenticationResult = await tenantAuthenticationContext.AcquireTokenAsync(resourceUrl, strClientId, new Uri(strReturnUrl), platformParams);
            //}
            //else
            //{
            //    UserIdentifier userIdentifier = new UserIdentifier(_LastAccount.Username, UserIdentifierType.RequiredDisplayableId);
            //    authenticationResult = await tenantAuthenticationContext.AcquireTokenAsync(resourceUrl, strClientId, new Uri(strReturnUrl), platformParams, userIdentifier);
            //}

            ////if (authenticationResult == null)
            ////    _LastAccount = null;
            ////else
            ////    _LastAccount = authenticationResult.;

            _LogProvider.WriteLog("GetToken", "End token request");
            
            return result;
        }

        public async Task<Microsoft.Identity.Client.AuthenticationResult> Login(string resourceUrl, PromptBehavior promptBehavior = PromptBehavior.Always)
        {
            _LogProvider.WriteLog("LoginAzureProvider", "Start token request");
            _LogProvider.WriteLog("GetToken", " - Resource Url: " + resourceUrl);
            _LogProvider.WriteLog("GetToken", " - Prompt Behavior: " + promptBehavior.ToString());

            string[] scopes = new string[] { "user.read" };

            if (this.app == null)
            {
                PublicClientApplicationOptions options = new PublicClientApplicationOptions();
                options.AzureCloudInstance = this.AzureEnvironment.GetAzureCloudInstance();
                options.AadAuthorityAudience = AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount;
                options.ClientId = strClientId;
                options.RedirectUri = strReturnUrl;

                this.app = PublicClientApplicationBuilder
                    .CreateWithApplicationOptions(options)
                    .WithRedirectUri("http://localhost")
                    .Build();
            }

            DefaultOsBrowserWebUi asdf = new DefaultOsBrowserWebUi();
            Microsoft.Identity.Client.AuthenticationResult result = await app.AcquireTokenInteractive(scopes)
                .WithCustomWebUi(asdf)
                .ExecuteAsync();
            

            if (result == null)
                _LastAccount = null;
            else
                _LastAccount = result.Account;

            _LogProvider.WriteLog("LoginAzureProvider", "End token request");

            //return authenticationResult;
            return result;
        }

        public static bool operator ==(AzureTokenProvider lhs, AzureTokenProvider rhs)
        {
            bool status = false;
            if (((object)lhs == null && (object)rhs == null) ||
                ((object)lhs != null && (object)rhs != null && lhs.AzureEnvironment == rhs.AzureEnvironment))
            {
                status = true;
            }
            return status;
        }

        public static bool operator !=(AzureTokenProvider lhs, AzureTokenProvider rhs)
        {
            return !(lhs == rhs);
        }
    }
}

