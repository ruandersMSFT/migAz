// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class NetworkInterfaceIpConfiguration : ArmResource, INetworkInterfaceIpConfiguration
    {
        private VirtualNetwork _VirtualNetwork;
        private Subnet _Subnet;
        private PublicIP _PublicIP;

        private NetworkInterfaceIpConfiguration() : base(null, null) { }

        public NetworkInterfaceIpConfiguration(AzureSubscription azureSubscription, JToken networkInterfaceIpConfiguration) : base(azureSubscription, networkInterfaceIpConfiguration)
        {
        }

        internal override async Task InitializeChildrenAsync()
        {
            if (this.PublicIpAddressId != String.Empty)
            {
                _PublicIP = await this.AzureSubscription.GetAzureARMPublicIP(this.PublicIpAddressId);
            }
        }

        public string ProvisioningState => (string)this.ResourceToken["properties"]["provisioningState"];
        public string PrivateIpAddress => (string)this.ResourceToken["properties"]["privateIPAddress"];
        public string PrivateIpAddressVersion => (string)this.ResourceToken["properties"]["privateIPAddressVersion"];
        public string PrivateIpAllocationMethod => (string)this.ResourceToken["properties"]["privateIPAllocationMethod"];
        public bool IsPrimary => Convert.ToBoolean((string)this.ResourceToken["properties"]["primary"]);
        public string SubnetId => (string)this.ResourceToken["properties"]["subnet"]["id"];
        private string PublicIpAddressId
        {
            get
            {
                if (this.ResourceToken["properties"]["publicIPAddress"] == null)
                    return String.Empty;

                return (string)this.ResourceToken["properties"]["publicIPAddress"]["id"];
            }
        }

        

        public string VirtualNetworkId
        {
            get
            {
                if (this.SubnetId.ToLower().Contains("/subnets/"))
                    return this.SubnetId.Substring(0, this.SubnetId.ToLower().IndexOf("/subnets/"));
                else
                    return String.Empty;
            }
        }

        //public string SubnetName
        //{
        //    get { return SubnetId.Split('/')[10]; }
        //}

        public BackEndAddressPool BackEndAddressPool
        {
            get;
            internal set;
        }

        public PublicIP PublicIP
        {
            get { return _PublicIP; }
            private set { _PublicIP = value; }
        }



    }
}

