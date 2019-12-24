// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using MigAz.Azure.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using MigAz.Azure.Core.Interface;
using MigAz.Azure.Core.ArmTemplate;
using System;
using System.Linq;
using Microsoft.Identity.Client;

namespace MigAz.Azure.Arm
{
    public class Location : ILocation
    {
        private AzureSubscription _AzureSubscription;
        private JToken _LocationToken;
        private List<VMSize> _ArmVmSizes;

        #region Constructors

        private Location() { }

        public Location(AzureSubscription azureSubscription, JToken locationToken)
        {
            this._AzureSubscription = azureSubscription;
            this._LocationToken = locationToken;
        }

        #endregion

        #region Properties

        public string Id
        {
            get { return (string)_LocationToken["id"]; }
        }

        public string DisplayName
        {
            get { return (string)_LocationToken["displayName"]; }
        }

        public string Longitude
        {
            get { return (string)_LocationToken["longitude"]; }
        }

        public string Latitude
        {
            get { return (string)_LocationToken["latitude"]; }
        }

        public string Name
        {
            get { return (string)_LocationToken["name"]; }
        }


        public AzureSubscription AzureSubscription
        {
            get { return _AzureSubscription; }
        }

        public List<VMSize> VMSizes
        {
            get { return _ArmVmSizes; }
        }

        #endregion

        #region Methods

        private async Task InitializeARMVMSizes()
        {
            AzureContext azureContext = this.AzureSubscription.AzureTenant.AzureContext;
            azureContext.LogProvider.WriteLog("InitializeARMVMSizes", "Start - Location : " + this.Name);

            ProviderResourceType provideResourceType = this.AzureSubscription.GetProviderResourceType(ArmConst.MicrosoftCompute, "locations/vmSizes");
            if (provideResourceType == null)
            {
                azureContext.LogProvider.WriteLog("InitializeARMVMSizes", "Unable to locate Provider Resource Type - Provider : '" + ArmConst.MicrosoftCompute + "' Resource Type: '" + "locations/vmSizes" + "'");
            }
            else if (!provideResourceType.IsLocationSupported(this))
            {
                azureContext.LogProvider.WriteLog("InitializeARMVMSizes", "Provider Resource Type not supported in Location. - Provider : '" + ArmConst.MicrosoftCompute + "' Resource Type: '" + "locations/vmSizes" + "' Location: '" + this.ToString() + "'");
            }
            else
            {
                if (_ArmVmSizes == null)
                {
                    AuthenticationResult armToken = await azureContext.TokenProvider.GetToken(this.AzureSubscription.TokenResourceUrl, "user_impersonation");

                    // https://docs.microsoft.com/en-us/rest/api/compute/virtualmachines/virtualmachines-list-sizes-region
                    string url = this.AzureSubscription.ApiUrl + "subscriptions/" + this.AzureSubscription.SubscriptionId + String.Format(ArmConst.ProviderVMSizes, this.Name) + "?api-version=" + this.AzureSubscription.GetProviderMaxApiVersion(ArmConst.MicrosoftCompute, "locations/vmSizes");
                    azureContext.StatusProvider.UpdateStatus("BUSY: Getting ARM VMSizes Location : " + this.ToString());

                    AzureRestRequest azureRestRequest = new AzureRestRequest(url, armToken);
                    AzureRestResponse azureRestResponse = await azureContext.AzureRetriever.GetAzureRestResponse(azureRestRequest);
                    JObject locationsVMSizesJson = JObject.Parse(azureRestResponse.Response);

                    azureContext.StatusProvider.UpdateStatus("BUSY: Loading VMSizes for Location: " + this.ToString());

                    var VMSizes = from VMSize in locationsVMSizesJson["value"]
                                  select VMSize;

                    List<VMSize> vmSizes = new List<VMSize>();
                    foreach (var VMSize in VMSizes)
                    {
                        Arm.VMSize armVMSize = new Arm.VMSize(VMSize);
                        vmSizes.Add(armVMSize);

                        azureContext.StatusProvider.UpdateStatus("BUSY: Instantiated VMSize '" + armVMSize.ToString() +"' for Location: " + this.ToString());
                    }

                    _ArmVmSizes = vmSizes.OrderBy(a => a.Name).ToList();
                }
            }

            azureContext.LogProvider.WriteLog("InitializeARMVMSizes", "End - Location : " + this.Name);
            azureContext.StatusProvider.UpdateStatus("Ready");
        }

        public async Task<VMSize> SeekVmSize(string name)
        {
            if (_ArmVmSizes == null)
                await this.InitializeARMVMSizes();

            return _ArmVmSizes.Where(a => a.Name == name).FirstOrDefault();
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        #endregion
    }
}

