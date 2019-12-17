// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Arm;
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
    public class NetworkInterface : Core.MigrationTarget
    {
        //private INetworkInterface _SourceNetworkInterface;
        private bool _EnableIPForwarding = false;
        private bool _EnableAcceleratedNetworking = false;
        private List<MigrationTarget.NetworkInterfaceIpConfiguration> _TargetNetworkInterfaceIpConfigurations = new List<MigrationTarget.NetworkInterfaceIpConfiguration>();
        private BackEndAddressPool _BackEndAddressPool = null;
        private List<InboundNatRule> _InboundNatRules = new List<InboundNatRule>();
        private VirtualMachine _ParentVirtualMachine;

        #region Constructors

        private NetworkInterface() : base(null, ArmConst.MicrosoftNetwork, ArmConst.NetworkInterfaces, null, null) { }

        public NetworkInterface(AzureSubscription azureSubscription, Asm.VirtualMachine virtualMachine, Asm.NetworkInterface networkInterface, List<VirtualNetwork> virtualNetworks, List<NetworkSecurityGroup> networkSecurityGroups, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.NetworkInterfaces, targetSettings, logProvider)
        {
            this.Source = networkInterface;
            this.SetTargetName(networkInterface.Name, targetSettings);
            this.IsPrimary = networkInterface.IsPrimary;
            this.EnableIPForwarding = networkInterface.EnableIpForwarding;
            
            foreach (Asm.NetworkInterfaceIpConfiguration asmNetworkInterfaceIpConfiguration in networkInterface.NetworkInterfaceIpConfigurations)
            {
                NetworkInterfaceIpConfiguration migrationNetworkInterfaceIpConfiguration = new NetworkInterfaceIpConfiguration(this.AzureSubscription, asmNetworkInterfaceIpConfiguration, virtualNetworks, targetSettings, logProvider);
                this.TargetNetworkInterfaceIpConfigurations.Add(migrationNetworkInterfaceIpConfiguration);
            }

            if (virtualMachine.NetworkSecurityGroup != null)
            {
                this.NetworkSecurityGroup = null; // russell todo now  NetworkSecurityGroup.SeekNetworkSecurityGroup(networkSecurityGroups, virtualMachine.NetworkSecurityGroup.ToString());
            }
        }

        public NetworkInterface(AzureSubscription azureSubscription, Arm.NetworkInterface networkInterface, List<MigrationTarget.VirtualNetwork> armVirtualNetworks, List<MigrationTarget.NetworkSecurityGroup> armNetworkSecurityGroups, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.NetworkInterfaces, targetSettings, logProvider)
        {
            this.Source = networkInterface;
            this.SetTargetName(networkInterface.Name, targetSettings);
        }

        public NetworkInterface(AzureSubscription azureSubscription, Arm.NetworkInterface networkInterface, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.NetworkInterfaces, targetSettings, logProvider)
        {
            this.Source = networkInterface;
            this.SetTargetName(networkInterface.Name, targetSettings);

            if (networkInterface.IsPrimary.HasValue)
                this.IsPrimary = networkInterface.IsPrimary.Value;

            this.EnableIPForwarding = networkInterface.EnableIPForwarding;
            this.EnableAcceleratedNetworking = networkInterface.EnableAcceleratedNetworking;

            //_TargetSettings = targetSettings;
        }

        #endregion

        public List<MigrationTarget.NetworkInterfaceIpConfiguration> TargetNetworkInterfaceIpConfigurations
        {
            get { return _TargetNetworkInterfaceIpConfigurations; }
        }

        public bool EnableIPForwarding
        {
            get { return _EnableIPForwarding; }
            set { _EnableIPForwarding = value; }
        }

        public bool AllowAcceleratedNetworking
        {
            get { return this.ApiVersion.CompareTo("2017-10-01") >= 0; }
        }

        public bool EnableAcceleratedNetworking
        {
            get { return _EnableAcceleratedNetworking; }
            set { _EnableAcceleratedNetworking = value; }
        }

        public VirtualMachine ParentVirtualMachine
        {
            get { return _ParentVirtualMachine; }
            set { _ParentVirtualMachine = value; }
        }

        public bool IsPrimary { get; set; }

        public NetworkSecurityGroup NetworkSecurityGroup
        {
            get; set;
        }

        public List<InboundNatRule> InboundNatRules
        {
            get { return _InboundNatRules; }
        }

        public BackEndAddressPool BackEndAddressPool
        {
            get { return _BackEndAddressPool; }
            set { _BackEndAddressPool = value; }
        }

        public override string ImageKey { get { return "NetworkInterface"; } }

        public override string FriendlyObjectName { get { return "Network Interface"; } }


        public NetworkSecurityGroup TargetNetworkSecurityGroup { get; set; }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName + targetSettings.NetworkInterfaceCardSuffix;
        }

        public override async Task RefreshFromSource()
        {
            if (this.Source != null)
            {
                if (this.Source.GetType() == typeof(Arm.NetworkInterface))
                {
                    Arm.NetworkInterface networkInterface = (Arm.NetworkInterface)this.Source;

                    if (networkInterface.IsPrimary.HasValue)
                        this.IsPrimary = networkInterface.IsPrimary.Value;

                    this.EnableIPForwarding = networkInterface.EnableIPForwarding;
                    this.EnableAcceleratedNetworking = networkInterface.EnableAcceleratedNetworking;
                    
                    foreach (Arm.NetworkInterfaceIpConfiguration armNetworkInterfaceIpConfiguration in networkInterface.NetworkInterfaceIpConfigurations)
                    {
                        NetworkInterfaceIpConfiguration targetNetworkInterfaceIpConfiguration = new NetworkInterfaceIpConfiguration(this.AzureSubscription, armNetworkInterfaceIpConfiguration, this.TargetSettings, this.LogProvider);
                        await targetNetworkInterfaceIpConfiguration.RefreshFromSource();

                        this.TargetNetworkInterfaceIpConfigurations.Add(targetNetworkInterfaceIpConfiguration);
                    }

                    if (networkInterface.NetworkSecurityGroupId != null)
                    {
                        this.NetworkSecurityGroup = (MigrationTarget.NetworkSecurityGroup) await this.AzureSubscription.SeekMigrationTargetBySource(networkInterface.NetworkSecurityGroupId);
                    }
                }
            }
        }
    }
}

