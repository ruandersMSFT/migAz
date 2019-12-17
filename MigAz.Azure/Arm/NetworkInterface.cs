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

    public class NetworkInterface : ArmResource, INetworkInterface
    {
        private NetworkInterface() : base(null, null) { }

        public NetworkInterface(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {

        }

        public bool EnableIPForwarding
        {
            get
            {
                try
                {
                    return (bool)ResourceToken.SelectToken("properties.enableIPForwarding"); 
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool EnableAcceleratedNetworking
        {
            get
            {
                try
                {
                    return (bool)ResourceToken.SelectToken("properties.enableAcceleratedNetworking");
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool? IsPrimary
        {
            get
            {
                try
                {
                    return (bool)ResourceToken.SelectToken("properties.primary");
                }
                catch
                {
                    return false;
                }
            }
        }

        public List<NetworkInterfaceIpConfiguration> NetworkInterfaceIpConfigurations
        {
            get
            {
                List<NetworkInterfaceIpConfiguration> networkInterfaceIpConfigurations = new List<NetworkInterfaceIpConfiguration>();

                foreach (JToken networkInterfaceIpConfigurationToken in ResourceToken["properties"]["ipConfigurations"])
                {
                    NetworkInterfaceIpConfiguration networkInterfaceIpConfiguration = new NetworkInterfaceIpConfiguration(this.AzureSubscription, networkInterfaceIpConfigurationToken);
                    networkInterfaceIpConfigurations.Add(networkInterfaceIpConfiguration);
                }

                return networkInterfaceIpConfigurations;
            }
        }

        public string NetworkSecurityGroupId
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.networkSecurityGroup.id");
            }
        }
    }
}

