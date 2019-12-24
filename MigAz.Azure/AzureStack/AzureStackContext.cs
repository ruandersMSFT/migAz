// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MigAz.Azure;
using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.AzureStack
{
    public class AzureStackContext : AzureContext
    {
        AzureStackEndpoints _AzureStackEndpoints;

        private AzureStackContext() : base(null, null) { }

        public AzureStackContext(AzureRetriever azureRetriever, PromptBehavior defaultPromptBehavior = PromptBehavior.Always) : base(azureRetriever, null, defaultPromptBehavior)
        {
        }

        public AzureStackEndpoints AzureStackEndpoints
        {
            get { return _AzureStackEndpoints; }
        }

        public override string GetARMTokenResourceUrl()
        {
            if (this.AzureStackEndpoints == null)
                return String.Empty;
            else
                return this.AzureStackEndpoints.Audiences;
        }

        public override string GetARMServiceManagementUrl()
        {
            return _AzureStackEndpoints.ManagementEndpoint;
            //return _AzureStackEndpoints.PortalEndpoint;  // is this for user subscriptions?
        }

        public async Task Login()
        {
            // AzureStack Login via PowerShell:  https://docs.microsoft.com/en-us/azure/azure-stack/azure-stack-powershell-configure-admin
            await base.Login(_AzureStackEndpoints.Audiences);

            List<AzureTenant> azureTenants = await this.GetAzureARMTenants();
            foreach (AzureTenant azureTenant in azureTenants)
            {
                List<AzureDomain> domains = await azureTenant.GetAzureARMDomains(this);
                List<AdminSubscription> subscriptions = await GetAzureStackARMSubscriptions(azureTenant);
            }

            this.StatusProvider.UpdateStatus("Ready");
        }


        public async Task<List<AdminSubscription>> GetAzureStackARMSubscriptions(AzureTenant azureTenant)
        {
            //_AzureContext.LogProvider.WriteLog("GetAzureARMSubscriptions", "Start - azureTenant: " + azureTenant.ToString());

            String subscriptionsUrl = this.GetARMServiceManagementUrl() + "subscriptions?api-version=2015-01-01";
            Microsoft.Identity.Client.AuthenticationResult authenticationResult = await this.TokenProvider.GetToken(this.GetARMTokenResourceUrl(), "user_impersonation", Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior.Auto);

            //_AzureContext.StatusProvider.UpdateStatus("BUSY: Getting Subscriptions...");

            AzureRestRequest azureRestRequest = new AzureRestRequest(subscriptionsUrl, authenticationResult, "GET", false);
            AzureRestResponse azureRestResponse = await this.AzureRetriever.GetAzureRestResponse(azureRestRequest);
            JObject subscriptionsJson = JObject.Parse(azureRestResponse.Response);

            var subscriptions = from subscription in subscriptionsJson["value"]
                                select subscription;

            List<AdminSubscription> tenantSubscriptions = new List<AdminSubscription>();

            foreach (JObject azureSubscriptionJson in subscriptions)
            {
                AdminSubscription azureSubscription = new AdminSubscription(this, azureSubscriptionJson, azureTenant, this.AzureEnvironment, this.GetARMServiceManagementUrl(), this.GetARMTokenResourceUrl());
                await azureSubscription.GetAzureStackUserSubscriptions();
                tenantSubscriptions.Add(azureSubscription);
            }

            return tenantSubscriptions;
        }

        internal async void LoadMetadataEndpoints(string azureStackAdminManagementUrl)
        {
            _AzureStackEndpoints = await AzureStackContext.LoadMetadataEndpoints(this.AzureRetriever, azureStackAdminManagementUrl);
        }

        public static async Task<AzureStackEndpoints> LoadMetadataEndpoints(AzureRetriever azureRetriever, string azureStackAdminManagementUrl)
        {
            string metadataEndpointsUrlBase = azureStackAdminManagementUrl;

            if (!metadataEndpointsUrlBase.EndsWith("/"))
                metadataEndpointsUrlBase += "/";

            String metadataEndpointsUrl = metadataEndpointsUrlBase + "metadata/endpoints?api-version=2015-01-01";

            AzureRestRequest azureRestRequest = new AzureRestRequest(metadataEndpointsUrl);
            AzureRestResponse azureRestResponse = await azureRetriever.GetAzureRestResponse(azureRestRequest);
            return new AzureStackEndpoints(metadataEndpointsUrlBase, azureRestResponse);
        }
    }
}

