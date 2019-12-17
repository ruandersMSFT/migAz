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
    public class Subnet : Core.MigrationTarget, IMigrationSubnet
    {
        //private ISubnet _SourceSubnet;
        private MigrationTarget.VirtualNetwork _ParentVirtualNetwork;

        #region Constructors

        private Subnet() : base(null, String.Empty, String.Empty, null, null) { }

        public Subnet(AzureSubscription azureSubscription, MigrationTarget.VirtualNetwork parentVirtualNetwork, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            _ParentVirtualNetwork = parentVirtualNetwork;
            this.SetTargetName("NewSubnet", targetSettings);
        }


        public Subnet(AzureSubscription azureSubscription, VirtualNetwork parentVirtualNetwork, ISubnet sourceSubnet, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            this._ParentVirtualNetwork = parentVirtualNetwork;
            this.Source = sourceSubnet;
            this.SetTargetName(sourceSubnet.Name, targetSettings);
            this.AddressPrefix = sourceSubnet.AddressPrefix;

            if (sourceSubnet.GetType() == typeof(Arm.Subnet))
            {
                Arm.Subnet armSubnet = (Arm.Subnet)sourceSubnet;

                this.NetworkSecurityGroup = armSubnet.NetworkSecurityGroup;
                this.RouteTable = armSubnet.RouteTable;
            }
        }

        #endregion

        private NetworkSecurityGroup SeekNetworkSecurityGroup(List<NetworkSecurityGroup> networkSecurityGroups, string sourceName)
        {
            if (networkSecurityGroups == null || sourceName == null)
                return null;

            foreach (NetworkSecurityGroup networkSecurityGroup in networkSecurityGroups)
            {
                if (networkSecurityGroup.SourceName == sourceName)
                    return networkSecurityGroup;
            }

            return null;
        }
        private RouteTable SeekRouteTable(List<RouteTable> routeTables, string sourceName)
        {
            if (routeTables == null || sourceName == null)
                return null;

            foreach (RouteTable routeTable in routeTables)
            {
                if (routeTable.SourceName == sourceName)
                    return routeTable;
            }

            return null;
        }

        public String AddressPrefix { get; set; }

        public MigrationTarget.VirtualNetwork ParentVirtualNetwork
        {
            get { return _ParentVirtualNetwork; }
        }

        public string TargetId
        {
            get { return "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderVirtualNetwork + this.ParentVirtualNetwork.ToString() + "/subnets/" + this.TargetName + "')]"; }
        }

        public IMigrationRouteTable RouteTable { get; set;  }
        public IMigrationNetworkSecurityGroup NetworkSecurityGroup { get; set; }

        public bool IsGatewaySubnet
        {
            get { return this.TargetName == ArmConst.GatewaySubnetName; }
        }

        public override string ImageKey { get { return "VirtualNetwork"; } }

        public override string FriendlyObjectName { get { return "Subnet"; } }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }

        public override async Task RefreshFromSource()
        {
            //    if (source.GetType() == typeof(Asm.Subnet))
            //    {
            //        Asm.Subnet asmSubnet = (Asm.Subnet)source;

            //        if (asmSubnet.NetworkSecurityGroup != null)
            //        {
            //            this.NetworkSecurityGroup = SeekNetworkSecurityGroup(networkSecurityGroups, asmSubnet.NetworkSecurityGroup.ToString());
            //        }

            //        if (asmSubnet.RouteTable != null)
            //        {
            //            this.RouteTable = SeekRouteTable(routeTables, asmSubnet.RouteTable.ToString());
            //        }
            //    }
            //    else if (source.GetType() == typeof(Arm.Subnet))
            //    {
            //        Arm.Subnet armSubnet = (Arm.Subnet)source;

            //        if (armSubnet.NetworkSecurityGroup != null)
            //        {
            //            this.NetworkSecurityGroup = SeekNetworkSecurityGroup(networkSecurityGroups, armSubnet.NetworkSecurityGroup.ToString());
            //        }

            //        if (armSubnet.RouteTable != null)
            //        {
            //            this.RouteTable = SeekRouteTable(routeTables, armSubnet.RouteTable.ToString());
            //        }

            //    }

            //    this.AddressPrefix = source.AddressPrefix;
        }
    }
}

