// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
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
    public class AdminSubscription : AzureSubscription
    {
        private AzureStackContext _AzureStackContext;

        #region Constructors

        private AdminSubscription() : base(null, null, new AzureEnvironment(AzureEnvironmentType.Azure, "NotAvailable", String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty), String.Empty, String.Empty)
        { }

        public AdminSubscription(AzureStackContext azureStackContext, JObject subscriptionJson, AzureTenant parentAzureTenant, AzureEnvironment azureEnvironment, String apiUrl, String tokenResourceUrl) : base(subscriptionJson, parentAzureTenant, azureEnvironment, apiUrl, tokenResourceUrl)
        {
            _AzureStackContext = azureStackContext;
        }

        #endregion

        public AzureStackContext AzureStackContext
        {
            get { return _AzureStackContext; }
        }

        public async Task<List<AzureSubscription>> GetAzureStackUserSubscriptions()
        {
            this.AzureStackContext.LogProvider.WriteLog("GetAzureStackUserSubscriptions", "Start Stack Subscription: " + this.ToString());

            String subscriptionsUrl = this.AzureStackContext.GetARMServiceManagementUrl() + "/subscriptions/" + this.SubscriptionId.ToString() + "/providers/Microsoft.Subscriptions.Admin/subscriptions?$filter=&api-version=2015-11-01";
            AuthenticationResult authenticationResult = await this.AzureStackContext.TokenProvider.GetToken(this.AzureStackContext.GetARMTokenResourceUrl(), "user_impersonation", Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior.Auto);

            this.AzureStackContext.StatusProvider.UpdateStatus("BUSY: Getting Subscriptions...");

            AzureRestRequest azureRestRequest = new AzureRestRequest(subscriptionsUrl, authenticationResult, "GET", false);
            AzureRestResponse azureRestResponse = await this.AzureStackContext.AzureRetriever.GetAzureRestResponse(azureRestRequest);
            JObject subscriptionsJson = JObject.Parse(azureRestResponse.Response);

            var subscriptions = from subscription in subscriptionsJson["value"]
                                select subscription;

            List<AzureSubscription> userSubscriptions = new List<AzureSubscription>();

            foreach (JObject azureSubscriptionJson in subscriptions)
            {
                AzureSubscription azureSubscription = new AzureSubscription(azureSubscriptionJson, this.AzureTenant, this.AzureEnvironment, _AzureStackContext.GetARMServiceManagementUrl(), _AzureStackContext.GetARMTokenResourceUrl());
                userSubscriptions.Add(azureSubscription);
            }

            return userSubscriptions;
        }

        //        

    }
}

