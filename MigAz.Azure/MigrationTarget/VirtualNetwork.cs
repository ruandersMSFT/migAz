// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class VirtualNetwork : Core.MigrationTarget, IMigrationVirtualNetwork
    {
        private List<VirtualNetworkGateway> _TargetVirtualNetworkGateways = new List<VirtualNetworkGateway>();
        private List<Subnet> _TargetSubnets = new List<Subnet>();
        List<string> _AddressPrefixes = new List<string>();
        List<string> _DnsServers = new List<string>();

        #region Constructors

        public VirtualNetwork() : base(null, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworks, null, null) { }

        public VirtualNetwork(AzureSubscription azureSubscription, Asm.VirtualNetwork virtualNetwork, List<NetworkSecurityGroup> networkSecurityGroups, List<RouteTable> routeTables, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworks, targetSettings, logProvider)
        {
            this.Source = virtualNetwork;
            this.SetTargetName(virtualNetwork.Name, targetSettings);

            if (virtualNetwork.Gateways2 != null)
            {
                foreach (Asm.VirtualNetworkGateway virtualNetworkGateway in virtualNetwork.Gateways2)
                {
                    TargetVirtualNetworkGateways.Add(new VirtualNetworkGateway(this.AzureSubscription, virtualNetworkGateway, targetSettings, logProvider));
                }
            }

            foreach (Asm.Subnet subnet in virtualNetwork.Subnets)
            {
                this.TargetSubnets.Add(new Subnet(this.AzureSubscription, this, subnet, targetSettings, logProvider));
            }

            foreach (String addressPrefix in virtualNetwork.AddressPrefixes)
            {
                this.AddressPrefixes.Add(addressPrefix);
            }

            foreach (String dnsServer in virtualNetwork.DnsServers)
            {
                this.DnsServers.Add(dnsServer);
            }
        }

        public VirtualNetwork(AzureSubscription azureSubscription, Arm.VirtualNetwork virtualNetwork, List<NetworkSecurityGroup> networkSecurityGroups, List<RouteTable> routeTables, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworks, targetSettings, logProvider)
        {
            this.Source = virtualNetwork;
            this.SetTargetName(virtualNetwork.Name, targetSettings);
        }

        public VirtualNetwork(AzureSubscription azureSubscription, IVirtualNetwork virtualNetwork, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworks, targetSettings, logProvider)
        {
            this.Source = virtualNetwork;
            this.SetTargetName(virtualNetwork.Name, targetSettings);
            foreach (String addressPrefix in virtualNetwork.AddressPrefixes)
            {
                this.AddressPrefixes.Add(addressPrefix);
            }
            foreach (String dnsServer in virtualNetwork.DnsServers)
            {
                this.DnsServers.Add(dnsServer);
            }

            foreach (ISubnet sourceSubnet in virtualNetwork.Subnets)
            {
                MigrationTarget.Subnet targetSubnet = new Subnet(this.AzureSubscription, this, sourceSubnet, targetSettings, logProvider);
                this.TargetSubnets.Add(targetSubnet);
            }
        }

        #endregion

        public List<VirtualNetworkGateway> TargetVirtualNetworkGateways
        {
            get { return _TargetVirtualNetworkGateways; }
        }

        public List<Subnet> TargetSubnets
        {
            get { return _TargetSubnets; }
        }

        public string TargetId
        {
            get { return "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderVirtualNetwork + this.ToString() + "')]"; }
        }



        public List<string> AddressPrefixes
        {
            get
            {

                return _AddressPrefixes;
            }
        }

        public List<string> DnsServers
        {
            get
            {
                return _DnsServers;
            }
        }

        public override string ImageKey { get { return "VirtualNetwork"; } }

        public override string FriendlyObjectName { get { return "Virtual Network"; } }


        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName + targetSettings.VirtualNetworkSuffix;
        }

        public override async Task RefreshFromSource()
        {
            if (this.Source != null)
            {
                if (this.Source.GetType() == typeof(Arm.VirtualNetwork))
                {
                    Arm.VirtualNetwork virtualNetwork = (Arm.VirtualNetwork)this.Source;

                    //foreach (Arm.VirtualNetworkGateway virtualNetworkGateway in virtualNetwork.VirtualNetworkGateways)
                    //{
                    //    TargetVirtualNetworkGateways.Add(new VirtualNetworkGateway(this.AzureSubscription, virtualNetworkGateway, targetSettings));
                    //}

                    this.TargetSubnets.Clear();
                    foreach (Arm.Subnet subnet in virtualNetwork.Subnets)
                    {
                        this.TargetSubnets.Add(new Subnet(this.AzureSubscription, this, subnet, this.TargetSettings, this.LogProvider));
                    }

                    this.AddressPrefixes.Clear();
                    foreach (String addressPrefix in virtualNetwork.AddressPrefixes)
                    {
                        this.AddressPrefixes.Add(addressPrefix);
                    }

                    this.DnsServers.Clear();
                    foreach (String dnsServer in virtualNetwork.DnsServers)
                    {
                        this.DnsServers.Add(dnsServer);
                    }
                }
            }
        }
    }
}

