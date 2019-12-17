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
    
    public class VirtualNetworkGateway : Core.MigrationTarget, IMigrationVirtualNetworkGateway
    {
        //private IVirtualNetworkGateway _SourceVirtualNetworkGateway;
        private List<Subnet> _TargetSubnets = new List<Subnet>();
        private VirtualNetworkGatewayType _GatewayType = VirtualNetworkGatewayType.Vpn;
        private bool _EnableBgp = false;
        private bool _ActiveActive = false;
        private VirtualNetworkGatewaySkuType _SkuName = VirtualNetworkGatewaySkuType.Basic;
        private VirtualNetworkGatewaySkuType _SkuTier = VirtualNetworkGatewaySkuType.Basic;
        private int _SkuCapacity = 2;
        private VirtualNetworkGatewayVpnType _VirtualNetworkGatewayVpnType = VirtualNetworkGatewayVpnType.RouteBased;

        #region Constructors

        private VirtualNetworkGateway() : base(null, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworkGateways, null, null) { }

        public VirtualNetworkGateway(AzureSubscription azureSubscription, IVirtualNetworkGateway virtualNetworkGateway, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworkGateways, targetSettings, logProvider)
        {
            this.Source = virtualNetworkGateway;
            this.SetTargetName(this.SourceName, targetSettings);
        }

        public VirtualNetworkGateway(AzureSubscription azureSubscription, Arm.VirtualNetworkGateway virtualNetworkGateway, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.VirtualNetworkGateways, targetSettings, logProvider)
        {
            this.Source = virtualNetworkGateway;
            this.SetTargetName(this.SourceName, targetSettings);

            if (virtualNetworkGateway.EnableBgp.HasValue)
                this.EnableBgp = virtualNetworkGateway.EnableBgp.Value;

            if (virtualNetworkGateway.ActiveActive.HasValue)
                this.ActiveActive = virtualNetworkGateway.ActiveActive.Value;

            switch (virtualNetworkGateway.GatewayType)
            {
                case "ExpressRoute":
                    this.GatewayType = VirtualNetworkGatewayType.ExpressRoute;
                    break;
                case "Vpn":
                default:
                    this.GatewayType = VirtualNetworkGatewayType.Vpn;
                    break;
            }

            switch (virtualNetworkGateway.VpnType)
            {
                case "PolicyBased":
                    this.VpnType = VirtualNetworkGatewayVpnType.PolicyBased;
                    break;
                case "RouteBased":
                default:
                    this.VpnType = VirtualNetworkGatewayVpnType.RouteBased;
                    break;
            }

            switch (virtualNetworkGateway.SkuName)
            {
                case "VpnGw1":
                    this.SkuName = VirtualNetworkGatewaySkuType.VpnGw1;
                    break;
                case "VpnGw2":
                    this.SkuName = VirtualNetworkGatewaySkuType.VpnGw2;
                    break;
                case "VpnGw3":
                    this.SkuName = VirtualNetworkGatewaySkuType.VpnGw3;
                    break;
                case "Basic":
                default:
                    this.SkuName = VirtualNetworkGatewaySkuType.Basic;
                    break;
            }

            switch (virtualNetworkGateway.SkuName)
            {
                case "VpnGw1":
                    this.SkuTier = VirtualNetworkGatewaySkuType.VpnGw1;
                    break;
                case "VpnGw2":
                    this.SkuTier = VirtualNetworkGatewaySkuType.VpnGw2;
                    break;
                case "VpnGw3":
                    this.SkuTier = VirtualNetworkGatewaySkuType.VpnGw3;
                    break;
                case "Basic":
                default:
                    this.SkuTier = VirtualNetworkGatewaySkuType.Basic;
                    break;
            }

            if (virtualNetworkGateway.SkuCapacity.HasValue)
                this.SkuCapacity = virtualNetworkGateway.SkuCapacity.Value;

            foreach (IpConfiguration gatewayIpConfiguration in virtualNetworkGateway.IpConfigurations)
            {

            }
        }

        #endregion

        public VirtualNetworkGatewaySkuType SkuName
        {
            get { return _SkuName; }
            set { _SkuName = value; }
        }
        public VirtualNetworkGatewaySkuType SkuTier
        {
            get { return _SkuTier; }
            set { _SkuTier = value; }
        }
        public int SkuCapacity
        {
            get { return _SkuCapacity; }
            set { _SkuCapacity = value; }
        }

        public VirtualNetworkGatewayType GatewayType
        {
            get { return _GatewayType; }
            set { _GatewayType = value; }
        }

        public bool EnableBgp
        {
            get { return _EnableBgp; }
            set { _EnableBgp = value; }
        }
        public bool ActiveActive
        {
            get { return _ActiveActive; }
            set { _ActiveActive = value; }
        }

        public VirtualNetworkGatewayVpnType VpnType
        {
            get { return _VirtualNetworkGatewayVpnType; }
            set { _VirtualNetworkGatewayVpnType = value; }
        }

        public override string ImageKey { get { return "VirtualNetworkGateway"; } }

        public override string FriendlyObjectName { get { return "Virtual Network Gateway"; } }


        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }

        public override async Task RefreshFromSource()
        {
            throw new NotImplementedException();
        }
    }
}

