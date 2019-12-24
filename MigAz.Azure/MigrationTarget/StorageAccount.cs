// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Interface;
using MigAz.Azure.Core;
using MigAz.Azure.Core.ArmTemplate;
using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace MigAz.Azure.MigrationTarget
{
    public class StorageAccount : Core.MigrationTarget, IStorageTarget
    {
        #region Variables

        private StorageAccountType _StorageAccountType = StorageAccountType.Premium_LRS;

        #endregion

        #region Constructors

        public StorageAccount() : base(null, ArmConst.MicrosoftStorage, ArmConst.StorageAccounts, null, null)
        {
            this.TargetName = "migaz" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 19);
            this.TargetNameResult = this.TargetName;
        }
        public StorageAccount(AzureSubscription azureSubscription, StorageAccountType storageAccountType, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftStorage, ArmConst.StorageAccounts, targetSettings, logProvider)
        {
            this.TargetName = "migaz" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 19);
            this.TargetNameResult = this.TargetName;
            this.StorageAccountType = storageAccountType;
        }
        public StorageAccount(AzureSubscription azureSubscription, string name, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftStorage, ArmConst.StorageAccounts, targetSettings, logProvider)
        {
            this.SetTargetName(name, targetSettings);
        }

        public StorageAccount(AzureSubscription azureSubscription, IStorageAccount source, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftStorage, ArmConst.StorageAccounts, targetSettings, logProvider)
        {
            this.Source = source;
            this.SetTargetName(source.Name, targetSettings);
            this.StorageAccountType = MigrationTarget.StorageAccount.GetStorageAccountType(source.AccountType);
        }

        #endregion

        #region Properties

        public string BlobStorageNamespace
        {
            get;set;
        }

        public override string ImageKey { get { return "StorageAccount"; } }

        public override string FriendlyObjectName { get { return "Storage Account"; } }

        public StorageAccountType StorageAccountType
        {
            get { return _StorageAccountType; }
            set { _StorageAccountType = value; }
        }

        public bool IsNameAvailable
        {
            get;
            private set;
        }

        #endregion

        #region Static Methods

        private static StorageAccountType GetStorageAccountType(string storageAccountType)
        {
            // https://msdn.microsoft.com/en-us/library/azure/hh264518.aspx

            if (String.Compare("Premium_LRS", storageAccountType, true) == 0)
                return StorageAccountType.Premium_LRS;

            return StorageAccountType.Standard_LRS;
        }

        public static int MaximumTargetNameLength(TargetSettings targetSettings)
        {
            return 24 - targetSettings.StorageAccountSuffix.Length;
        }

        #endregion

        #region Methods

        public async Task<bool> CheckNameAvailability(AzureSubscription targetSubscription)
        {
            const String checkNameAvailability = "checkNameAvailability";

            if (targetSubscription == null)
                throw new ArgumentException("Target Subscription must be specified to check Storage Account Name Availability.");

            if (targetSubscription.SubscriptionId == Guid.Empty)
            {
                this.IsNameAvailable = true;
                return this.IsNameAvailable;
            }

            AzureContext azureContext = targetSubscription.AzureTenant.AzureContext;

            if (targetSubscription.ExistsProviderResourceType(ArmConst.MicrosoftStorage, checkNameAvailability))
            {

                if (azureContext != null && azureContext.LogProvider != null)
                    azureContext.LogProvider.WriteLog("CheckNameAvailability", "Storage Account '" + this.ToString());

                if (azureContext != null && azureContext.StatusProvider != null)
                    azureContext.StatusProvider.UpdateStatus("Checking Name Availabilty for target Storage Account '" + this.ToString() + "'");

                // https://docs.microsoft.com/en-us/rest/api/storagerp/storageaccounts/checknameavailability
                string url = "/subscriptions/" + targetSubscription.SubscriptionId + ArmConst.ProviderStorage + checkNameAvailability + "?api-version=" + targetSubscription.GetProviderMaxApiVersion(ArmConst.MicrosoftStorage, checkNameAvailability);

                AuthenticationResult authenticationResult = await azureContext.TokenProvider.GetToken(azureContext.AzureEnvironment.ResourceManagerEndpoint, "user_impersonation");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.BaseAddress = new Uri(azureContext.AzureEnvironment.ResourceManagerEndpoint);

                    if (azureContext != null && azureContext.LogProvider != null)
                        azureContext.LogProvider.WriteLog("CheckNameAvailability", "Storage Account '" + this.ToString() + "' PostAsync " + url);

                    //todonowasap
                    //using (var response = await client.PostAsJsonAsync(url,
                    //        new
                    //        {
                    //            name = this.ToString(),
                    //            type = ArmConst.TypeStorageAccount
                    //        })
                    //    )
                    //{
                    //    String strResponse = response.Content.ReadAsStringAsync().Result.ToString();
                    //    azureContext.LogProvider.WriteLog("CheckNameAvailability", "HttpStatusCode: '" + response.StatusCode);
                    //    azureContext.LogProvider.WriteLog("CheckNameAvailability", "Response:  '" + strResponse);


                    //    JObject responseJson = JObject.Parse(strResponse);
                    //    this.IsNameAvailable = (response.StatusCode == System.Net.HttpStatusCode.OK &&
                    //        responseJson != null &&
                    //        responseJson["nameAvailable"] != null &&
                    //        String.Compare(responseJson["nameAvailable"].ToString(), "True", true) == 0);
                    //}
                }

                if (azureContext != null && azureContext.StatusProvider != null)
                    azureContext.StatusProvider.UpdateStatus("Ready");

            }
            else
            {
                azureContext.LogProvider.WriteLog("CheckNameAvailability", "Provider Resource Type does not exist.  Unable to check if storage account name is available.");
                this.IsNameAvailable = true;
            }

            return this.IsNameAvailable;
        }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            int maxStorageAccountNameLength = 24;
            string value = targetName.Trim().Replace(" ", String.Empty).ToLower();

            if (targetSettings != null)
            {
                if (targetSettings.StorageAccountSuffix != null)
                {
                    if (value.Length + targetSettings.StorageAccountSuffix.Length > maxStorageAccountNameLength)
                        value = value.Substring(0, 24 - targetSettings.StorageAccountSuffix.Length);

                    this.TargetName = value;
                    this.TargetNameResult = this.TargetName + targetSettings.StorageAccountSuffix;
                }
            }

            if (value.Length > 24)
                throw new ArgumentException("Storage Account Name '" + value + "' exceeds maximum length of " + maxStorageAccountNameLength.ToString() + ".");

            this.TargetName = value;
            this.TargetNameResult = value;
        }

        public override async Task RefreshFromSource()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}

