// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MigAz.Azure.Interface;
using MigAz.Azure.Core.ArmTemplate;
using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class ManagedDisk : ArmResource, IArmDisk
    {
        private VirtualMachine _VirtualMachine;
        private JToken _VirtualMachineDiskJToken;

        private ManagedDisk() : base(null, null) { }

        public ManagedDisk(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }

        public new async Task InitializeChildrenAsync()
        {
            await base.InitializeChildrenAsync();
        }

        #region Properties

        public VirtualMachine ParentVirtualMachine
        {
            get { return _VirtualMachine; }
        }

        internal void SetParentVirtualMachine(VirtualMachine virtualMachine, JToken jToken)
        {
            _VirtualMachine = virtualMachine;
            _VirtualMachineDiskJToken = jToken;
        }

        public Int32 DiskSizeGb
        {
            get
            {
                try
                {
                    Int32 diskSizeGb = 0;
                    Int32.TryParse((string)this.ResourceToken["properties"]["diskSizeGB"], out diskSizeGb);

                    return diskSizeGb;
                }
                catch (System.NullReferenceException)
                {
                    return 0;
                }
            }
        }

        public int? Lun
        {
            get
            {
                try
                {
                    if (this._VirtualMachineDiskJToken == null || this._VirtualMachineDiskJToken["lun"] == null)
                        return null;

                    int lun = -1;
                    int.TryParse((string)this._VirtualMachineDiskJToken["lun"], out lun);

                    return lun;
                }
                catch (System.NullReferenceException)
                {
                    return -1;
                }
            }
        }
        public string HostCaching
        {
            get
            {
                if (this._VirtualMachineDiskJToken == null || this._VirtualMachineDiskJToken["caching"] == null)
                    return String.Empty;

                return (string)this._VirtualMachineDiskJToken["caching"];
            }
        }

        public string OwnerId
        {
            get { return (string)ResourceToken["properties"]["ownerId"]; }
        }

        public string ProvisioningState
        {
            get { return (string)ResourceToken["properties"]["provisioningState"]; }
        }

        public string DiskState
        {
            get { return (string)ResourceToken["properties"]["diskState"]; }
        }
        public string TimeCreated
        {
            get { return (string)ResourceToken["properties"]["timeCreated"]; }
        }
        public string AccountType
        {
            get
            {
                if (ResourceToken["properties"]["accountType"] != null)
                    return (string)ResourceToken["properties"]["accountType"];

                if (ResourceToken["sku"]["name"] != null)
                    return (string)ResourceToken["sku"]["name"];

                return String.Empty;
            }
        }

        public StorageAccountType StorageAccountType
        {
            get
            {
                if (AccountType == "Premium_LRS")
                    return StorageAccountType.Premium_LRS;
                else
                    return StorageAccountType.Standard_LRS;
            }
        }

        public string CreateOption
        {
            get { return (string)ResourceToken["properties"]["creationData"]["createOption"]; }
        }

        public string SourceUri
        {
            get { return (string)ResourceToken["properties"]["creationData"]["sourceUri"]; }
        }

        public bool IsEncrypted
        {
            get
            {
                if (this._VirtualMachineDiskJToken == null)
                    return false;

                if (this._VirtualMachineDiskJToken["encryptionSettings"] == null)
                    return false;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["enabled"] == null)
                    return false;

                return Convert.ToBoolean((string)this._VirtualMachineDiskJToken["encryptionSettings"]["enabled"]);
            }
        }


        public string DiskEncryptionKeySourceVaultId
        {
            get
            {
                if (this._VirtualMachineDiskJToken == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"]["sourceVault"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"]["sourceVault"]["id"] == null)
                    return null;

                return (string)this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"]["sourceVault"]["id"];
            }
        }
        public string DiskEncryptionKeySecretUrl
        {
            get
            {
                if (this._VirtualMachineDiskJToken == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"]["secretUrl"] == null)
                    return null;

                return (string)this._VirtualMachineDiskJToken["encryptionSettings"]["diskEncryptionKey"]["secretUrl"];
            }
        }

        public bool IsEncryptedWithKeyEncryptionKey
        {
            get
            {
                return this.KeyEncryptionKeyKeyUrl != null;
            }
        }

        public string KeyEncryptionKeySourceVaultId
        {
            get
            {
                if (this._VirtualMachineDiskJToken == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"]["sourceVault"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"]["sourceVault"]["id"] == null)
                    return null;

                return (string)this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"]["sourceVault"]["id"];
            }
        }
        public string KeyEncryptionKeyKeyUrl
        {
            get
            {
                if (this._VirtualMachineDiskJToken == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"] == null)
                    return null;

                if (this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"]["keyUrl"] == null)
                    return null;

                return (string)this._VirtualMachineDiskJToken["encryptionSettings"]["keyEncryptionKey"]["keyUrl"];
            }
        }

        #endregion

        public async Task<string> GetSASUrlAsync(int tokenDurationSeconds)
        {
            if (this.AzureSubscription == null)
                throw new ArgumentException("Managed Disk must have an Azure Subscription context to obtain SAS URL.");

            if (this.AzureSubscription.SubscriptionId == Guid.Empty)
                return String.Empty;

            AzureContext azureContext = this.AzureSubscription.AzureTenant.AzureContext;

            if (azureContext != null && azureContext.LogProvider != null)
                azureContext.LogProvider.WriteLog("GetSASUrlAsync", "Start Disk '" + this.Name + "'");

            if (azureContext != null && azureContext.StatusProvider != null)
                azureContext.StatusProvider.UpdateStatus("Getting Access SAS for Managed Disk '" + this.Name + "'");

            // https://docs.microsoft.com/en-us/rest/api/compute/manageddisks/disks/disks-grant-access
            string url = "/subscriptions/" + this.AzureSubscription.SubscriptionId + "/resourceGroups/" + this.ResourceGroup.Name + ArmConst.ProviderManagedDisks + this.Name + "/BeginGetAccess?api-version=2017-03-30";
            string strAccessSAS = String.Empty;

            AuthenticationResult authenticationResult = await azureContext.TokenProvider.GetToken(azureContext.AzureEnvironment.ResourceManagerEndpoint, azureContext.AzureSubscription.AzureAdTenantId);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri(azureContext.AzureEnvironment.ResourceManagerEndpoint);

                if (azureContext != null && azureContext.LogProvider != null)
                    azureContext.LogProvider.WriteLog("GetSASUrlAsync", "Disk '" + this.Name + "' PostAsync " + url + "");

                IEnumerable<string> requestId;
                using (var response = await client.PostAsJsonAsync(url,
                        new
                        {
                            access = "Read",
                            durationInSeconds = tokenDurationSeconds.ToString()
                        })
                    )
                {
                    //response.EnsureSuccessStatusCode();
                    response.Headers.TryGetValues("x-ms-request-id", out requestId);
                }

                String diskOperationStatus = "InProgress";

                while (diskOperationStatus == "InProgress")
                { 
                    string url2 = "/subscriptions/" + azureContext.AzureSubscription.SubscriptionId + "/providers/Microsoft.Compute/locations/" + this.Location + "/DiskOperations/" + requestId.ToList<string>()[0].ToString() + "?api-version=2017-03-30";

                    if (azureContext != null && azureContext.LogProvider != null)
                        azureContext.LogProvider.WriteLog("GetSASUrlAsync", "Disk '" + this.Name + "' GetAsync " + url2 + "");

                    using (var response2 = await client.GetAsync(url2))
                    {
                        //response2.EnsureSuccessStatusCode();
                        string responseString = await response2.Content.ReadAsStringAsync();
                        JObject responseJson = JObject.Parse(responseString);

                        if (responseJson["status"] == null && this._VirtualMachine != null)
                            throw new MigAzSASUrlException("Unable to obtain SAS Token for Disk '" + this.ToString() + "'.  Disk is attached to Virtual Machine '" + this._VirtualMachine.ToString() + "' which may be running.  MigAz can currently only obtain the SAS URL for the Managed Disk when the owning VM is stopped.  Please ensure VM is stopped.");

                        diskOperationStatus = responseJson["status"].ToString();

                        if (azureContext != null && azureContext.LogProvider != null)
                            azureContext.LogProvider.WriteLog("GetSASUrlAsync", "Disk '" + this.Name + "' Disk Operation Status " + diskOperationStatus + "");

                        if (diskOperationStatus == "InProgress")
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        else if (diskOperationStatus == "Succeeded")
                        {
                            strAccessSAS = responseJson["properties"]["output"]["accessSAS"].ToString();

                            if (azureContext != null && azureContext.LogProvider != null)
                                azureContext.LogProvider.WriteLog("GetSASUrlAsync", "Disk '" + this.Name +  "' Obtained AccessSAS " + strAccessSAS + "");
                        }
                    }
                }
            }

            if (azureContext != null && azureContext.LogProvider != null)
                azureContext.LogProvider.WriteLog("GetSASUrlAsync", "End Disk '" + this.Name + "'");

            return strAccessSAS;
        }
    }
}

