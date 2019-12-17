// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Interface;
using MigAz.Azure.Core;
using MigAz.Azure.Core.ArmTemplate;
using MigAz.Azure.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.MigrationTarget
{
    public class NetworkInterfaceIpConfiguration : Core.MigrationTarget, IVirtualNetworkTarget
    {
        #region Constructors

        private NetworkInterfaceIpConfiguration() : base(null, String.Empty, String.Empty, null, null) { }

        public NetworkInterfaceIpConfiguration(AzureSubscription azureSubscription, Azure.Asm.NetworkInterfaceIpConfiguration ipConfiguration, List<VirtualNetwork> virtualNetworks, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            this.Source = ipConfiguration;

            this.SetTargetName(ipConfiguration.Name, targetSettings);
            if (ipConfiguration.PrivateIpAllocationMethod.Trim().ToLower() == "static")
                this.TargetPrivateIPAllocationMethod = IPAllocationMethodEnum.Static;
            else
                this.TargetPrivateIPAllocationMethod = IPAllocationMethodEnum.Dynamic;

            this.TargetPrivateIpAddress = ipConfiguration.PrivateIpAddress;

            #region Attempt to default Target Virtual Network and Target Subnet objects from source names

            this.TargetVirtualNetwork = SeekVirtualNetwork(virtualNetworks, ipConfiguration.VirtualNetworkName);
            if (this.TargetVirtualNetwork != null && this.TargetVirtualNetwork.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork)) // Should only be of this type, as we don't default to another existing ARM VNet (which would be of the base interface type also)
            {
                Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork = (Azure.MigrationTarget.VirtualNetwork)this.TargetVirtualNetwork;
                foreach (Subnet targetSubnet in targetVirtualNetwork.TargetSubnets)
                {
                    if (targetSubnet.SourceName == ipConfiguration.SubnetName)
                    {
                        this.TargetSubnet = targetSubnet;
                        break;
                    }
                }
            }

            #endregion

        }

        public NetworkInterfaceIpConfiguration(AzureSubscription azureSubscription, Azure.Arm.NetworkInterfaceIpConfiguration ipConfiguration, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            this.Source = ipConfiguration;
            this.SetTargetName(ipConfiguration.Name, targetSettings);
        }

        #endregion

        private VirtualNetwork SeekVirtualNetwork(List<VirtualNetwork> virtualNetworks, string virtualNetworkName)
        {
            foreach (VirtualNetwork targetVirtualNetwork in virtualNetworks)
            {
                if (targetVirtualNetwork.SourceName == virtualNetworkName)
                    return targetVirtualNetwork;
            }

            return null;
        }

        #region IVirtualNetworkTarget Interface Implementation
        public IMigrationVirtualNetwork TargetVirtualNetwork { get; set; }
        public IMigrationSubnet TargetSubnet { get; set; }
        public IPAllocationMethodEnum TargetPrivateIPAllocationMethod { get; set; }
        public string TargetPrivateIpAddress { get; set; }

        public override string ImageKey { get { return "NetworkInterfaceIpConfiguration"; } }

        public override string FriendlyObjectName { get { return "Network Interface IP Configuration"; } }


        #endregion

        public Core.IMigrationPublicIp TargetPublicIp { get; set; }

        public NetworkSecurityGroup TargetNetworkSecurityGroup { get; set; }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }

        public override async Task RefreshFromSource()
        {
            if (this.Source != null)
            {
                if (this.Source.GetType() == typeof(Azure.Arm.NetworkInterfaceIpConfiguration))
                {
                    Arm.NetworkInterfaceIpConfiguration ipConfiguration = (Arm.NetworkInterfaceIpConfiguration)this.Source;

                    if (ipConfiguration.PrivateIpAllocationMethod.Trim().ToLower() == "static")
                        this.TargetPrivateIPAllocationMethod = IPAllocationMethodEnum.Static;
                    else
                        this.TargetPrivateIPAllocationMethod = IPAllocationMethodEnum.Dynamic;

                    this.TargetPrivateIpAddress = ipConfiguration.PrivateIpAddress;
                    this.TargetVirtualNetwork = (Arm.VirtualNetwork)await AzureSubscription.SeekResourceById(ipConfiguration.VirtualNetworkId);
                    if (this.TargetVirtualNetwork != null && this.TargetVirtualNetwork.GetType() == typeof(Azure.Arm.VirtualNetwork))
                    {
                        Azure.Arm.VirtualNetwork armVirtualNetwork = (Azure.Arm.VirtualNetwork)this.TargetVirtualNetwork;
                        foreach (Arm.Subnet armSubnet in armVirtualNetwork.Subnets)
                        {
                            if (String.Compare(armSubnet.Id, ipConfiguration.SubnetId, true) == 0)
                            {
                                this.TargetSubnet = armSubnet;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

