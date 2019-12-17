// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Core.ArmTemplate
{
    public class ArmConst
    {

        public const string ResourceGroupId = "resourceGroup().id";
        public const string Locations = "/locations";
        public const string GatewaySubnetName = "GatewaySubnet";

        public const string MicrosoftCompute = "Microsoft.Compute";
        public const string AvailabilitySets = "availabilitySets";
        public const string ProviderAvailabilitySets = "/providers/" + MicrosoftCompute + "/" + AvailabilitySets + "/";
        public const string ProviderVirtualMachineImages = "/providers/" + MicrosoftCompute + "/images/";
        public const string ProviderVMSizes = "/providers/" + MicrosoftCompute + "/locations/{0}/vmSizes/";
        public const string Disks = "disks";
        public const string ProviderManagedDisks = "/providers/" + MicrosoftCompute + "/" + Disks  + "/";
        public const string VirtualMachines = "virtualMachines";
        public const string ProviderVirtualMachines = "/providers/" + MicrosoftCompute + "/" + VirtualMachines + "/";

        public const string MicrosoftStorage = "Microsoft.Storage";
        public const string ProviderStorage = "/providers/" + MicrosoftStorage + "/";
        public const string StorageAccounts = "storageAccounts";
        public const string ProviderStorageAccounts = "/providers/" + MicrosoftStorage + "/storageAccounts/";
        public const string TypeStorageAccount = MicrosoftStorage + "/storageAccounts";

        public const string MicrosoftNetwork = "Microsoft.Network";
        public const string VirtualNetworks = "virtualNetworks";
        public const string ProviderVirtualNetwork = "/providers/" + MicrosoftNetwork + "/" + VirtualNetworks + "/";
        public const string LoadBalancers = "loadBalancers";
        public const string ProviderLoadBalancers = "/providers/" + MicrosoftNetwork + "/" + LoadBalancers + "/";
        public const string PublicIPAddresses = "publicIPAddresses";
        public const string ProviderPublicIpAddress = "/providers/" + MicrosoftNetwork + "/" + PublicIPAddresses + "/";
        public const string ApplicationSecurityGroups = "applicationSecurityGroups";
        public const string NetworkSecurityGroups = "networkSecurityGroups";
        public const string ProviderNetworkSecurityGroups = "/providers/" + MicrosoftNetwork + "/" + NetworkSecurityGroups + "/";
        public const string RouteTables = "routeTables";
        public const string ProviderRouteTables = "/providers/" + MicrosoftNetwork + "/"+ RouteTables + "/";
        public const string LocalNetworkGateways = "localNetworkGateways";
        public const string ProviderLocalNetworkGateways = "/providers/" + MicrosoftNetwork + "/"+ LocalNetworkGateways + "/";
        public const string VirtualNetworkGateways = "virtualNetworkGateways";
        public const string ProviderVirtualNetworkGateways = "/providers/" + MicrosoftNetwork + "/" + VirtualNetworkGateways + "/";
        public const string NetworkInterfaces = "networkInterfaces";
        public const string ProviderNetworkInterfaces = "/providers/" + MicrosoftNetwork + "/networkInterfaces/";
        public const string ProviderExpressRouteCircuits = "/providers/" + MicrosoftNetwork + "/expressRouteCircuits/";
        public const string Connections = "connections";
        public const string ProviderGatewayConnection = "/providers/" + MicrosoftNetwork + "/" + Connections + "/";
    }
}

