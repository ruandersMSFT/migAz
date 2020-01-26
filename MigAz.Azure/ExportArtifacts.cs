// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.MigrationTarget;
using MigAz.Azure.Core.Interface;
using System.Collections.Generic;
using System;
using MigAz.Azure.Core.Generator;
using System.Threading.Tasks;
using MigAz.Azure.Core;
using System.Linq;

namespace MigAz.Azure
{
    public class ExportArtifacts : IExportArtifacts
    {
        private List<MigAzGeneratorAlert> _Alerts = new List<MigAzGeneratorAlert>();

        public delegate Task AfterResourceValidationHandler();
        public event AfterResourceValidationHandler AfterResourceValidation;

        public ExportArtifacts(AzureSubscription targetSubscription)
        {
            VirtualNetworkGateways = new List<VirtualNetworkGateway>();
            VirtualNetworkGatewayConnections = new List<VirtualNetworkGatewayConnection>();
            LocalNetworkGateways = new List<LocalNetworkGateway>();
            ApplicationSecurityGroups = new List<ApplicationSecurityGroup>();
            NetworkSecurityGroups = new List<NetworkSecurityGroup>();
            RouteTables = new List<RouteTable>();
            StorageAccounts = new List<StorageAccount>();
            VirtualNetworks = new List<VirtualNetwork>();
            AvailablitySets = new List<AvailabilitySet>();
            VirtualMachines = new List<VirtualMachine>();
            LoadBalancers = new List<LoadBalancer>();
            PublicIPs = new List<PublicIp>();
            Disks = new List<Disk>();
            NetworkInterfaces = new List<NetworkInterface>();
            RouteTables = new List<RouteTable>();

            this.TargetSubscription = targetSubscription;
        }


        public ResourceGroup ResourceGroup { get; set; }
        public List<VirtualNetworkGateway> VirtualNetworkGateways { get; }
        public List<VirtualNetworkGatewayConnection> VirtualNetworkGatewayConnections { get; }
        public List<LocalNetworkGateway> LocalNetworkGateways { get; }
        public List<ApplicationSecurityGroup> ApplicationSecurityGroups { get; }
        public List<NetworkSecurityGroup> NetworkSecurityGroups { get; }
        public List<StorageAccount> StorageAccounts { get; }
        public List<VirtualNetwork> VirtualNetworks { get; }
        public List<AvailabilitySet> AvailablitySets { get; }
        public List<VirtualMachine> VirtualMachines { get; }
        public List<LoadBalancer> LoadBalancers { get; }
        public List<PublicIp> PublicIPs { get; }
        public List<Disk> Disks { get; }
        public List<NetworkInterface> NetworkInterfaces { get; }
        public List<RouteTable> RouteTables { get; }

        public List<MigAzGeneratorAlert> Alerts
        {
            get { return _Alerts; }
        }

        public AzureSubscription TargetSubscription
        {
            get;
            private set;
        }

        public NetworkSecurityGroup SeekNetworkSecurityGroup(string sourceName)
        {
            foreach (NetworkSecurityGroup networkSecurityGroup in NetworkSecurityGroups)
            {
                if (networkSecurityGroup.ToString() == sourceName)
                    return networkSecurityGroup;
            }

            return null;
        }

        public RouteTable SeekRouteTable(string sourceName)
        {
            foreach (RouteTable routeTable in RouteTables)
            {
                if (routeTable.ToString() == sourceName)
                    return routeTable;
            }

            return null;
        }

        internal PublicIp SeekPublicIp(string sourceName)
        {
            foreach (PublicIp publicIp in PublicIPs)
            {
                if (publicIp.ToString() == sourceName)
                    return publicIp;
            }

            return null;
        }

        internal bool ContainsLoadBalancer(LoadBalancer loadBalancer)
        {
            foreach (LoadBalancer exportArtifactLoadBalancer in LoadBalancers)
            {
                if (exportArtifactLoadBalancer.ToString() == loadBalancer.ToString())
                    return true;
            }

            return false;
        }


        public bool HasErrors
        {
            get
            {
                foreach (MigAzGeneratorAlert alert in this.Alerts)
                {
                    if (alert.AlertType == AlertType.Error)
                        return true;
                }

                return false;
            }
        }

        public MigAzGeneratorAlert SeekAlert(string alertMessage)
        {
            foreach (MigAzGeneratorAlert alert in this.Alerts)
            {
                if (alert.Message.Contains(alertMessage))
                    return alert;
            }

            return null;
        }

        public async Task ValidateAzureResources()
        {
            Alerts.Clear();

            if (this.TargetSubscription == null)
            {
                this.AddAlert(AlertType.Error, "Target Azure Subscription must be provided for template generation.", this.ResourceGroup);
            }
            else
            {
                if (this.TargetSubscription.Locations == null || this.TargetSubscription.Locations.Count() == 0)
                {
                    this.AddAlert(AlertType.Error, "Target Azure Subscription must have one or more Locations instantiated.", this.ResourceGroup);
                }
            }

            if (this.ResourceGroup == null)
            {
                this.AddAlert(AlertType.Error, "Target Resource Group must be provided for template generation.", this.ResourceGroup);
            }
            else
            {
                if (this.ResourceGroup.TargetLocation == null)
                {
                    this.AddAlert(AlertType.Error, "Target Resource Group Location must be provided for template generation.", this.ResourceGroup);
                }
                else
                {
                    // It is possible that the Target Location is no longer in the Target Subscription
                    // Sample case, user first connected to Azure Commercial as source (and set as initial target)
                    // but then logged into a different account for the target and target is now USGov.
                    if (this.TargetSubscription != null && this.TargetSubscription.Locations != null)
                    {
                        if (!this.TargetSubscription.Locations.Contains(this.ResourceGroup.TargetLocation))
                        {
                            this.AddAlert(AlertType.Error, "Target Resource Group Location '" + this.ResourceGroup.TargetLocation.ToString() + "' is not available in Subscription '" + this.TargetSubscription.ToString() + "'.  Select a new Target Location.", this.ResourceGroup);
                        }
                    }
                }
            }

            foreach (MigrationTarget.StorageAccount targetStorageAccount in this.StorageAccounts)
            {
                ValidateTargetApiVersion(targetStorageAccount);
                await targetStorageAccount.CheckNameAvailability(this.TargetSubscription);

                if (!targetStorageAccount.IsNameAvailable)
                    this.AddAlert(AlertType.Error, "Target Name for Storage Account '" + targetStorageAccount.ToString() + "' already exists within Azure Environment " + this.TargetSubscription.AzureEnvironment.ToString() + ".  A new (available) target name must be specified.", targetStorageAccount);

                if (targetStorageAccount.BlobStorageNamespace == null || targetStorageAccount.BlobStorageNamespace.Trim().Length <= 0)
                    this.AddAlert(AlertType.Error, "Blob Storage Namespace for Target Storage Account '" + targetStorageAccount.ToString() + "' must be specified.", targetStorageAccount);

                if (!this.IsStorageAccountVmDiskTarget(targetStorageAccount))
                    this.AddAlert(AlertType.Warning, "Target Storage Account '" + targetStorageAccount.ToString() + "' is not utilized within this Resource Group Deployment as a Virtual Machine Disk Target.  Consider removing to avoid creation of a non-utilized Storage Account.", targetStorageAccount);
            }

            foreach (MigrationTarget.VirtualNetwork targetVirtualNetwork in this.VirtualNetworks)
            {
                ValidateTargetApiVersion(targetVirtualNetwork);

                foreach (MigrationTarget.Subnet targetSubnet in targetVirtualNetwork.TargetSubnets)
                {
                    if (targetSubnet.NetworkSecurityGroup != null)
                    {
                        if (targetSubnet.NetworkSecurityGroup.GetType() == typeof(Arm.NetworkSecurityGroup))
                        {
                            Arm.NetworkSecurityGroup networkSecurityGroup = (Arm.NetworkSecurityGroup) targetSubnet.NetworkSecurityGroup;
                            if (networkSecurityGroup.AzureSubscription.SubscriptionId != this.TargetSubscription.SubscriptionId)
                            {
                                this.AddAlert(AlertType.Error, "Virtual Network '" + targetVirtualNetwork.ToString() + "' Subnet '" + targetSubnet.ToString() + "' ARM Network Security Group (NSG) '" + networkSecurityGroup.ToString() + "'which is only available within the source Azure Subscription (" + this.TargetSubscription.ToString() + ")." + targetSubnet.NetworkSecurityGroup.ToString() + "', but the NSG resource is not added into the migration template.", targetSubnet);
                            }
                        }
                        else if (targetSubnet.NetworkSecurityGroup.GetType() == typeof(MigrationTarget.NetworkSecurityGroup))
                        {
                            MigrationTarget.NetworkSecurityGroup networkSecurityGroupInMigration = this.SeekNetworkSecurityGroup(targetSubnet.NetworkSecurityGroup.ToString());

                            if (networkSecurityGroupInMigration == null)
                            {
                                this.AddAlert(AlertType.Error, "Virtual Network '" + targetVirtualNetwork.ToString() + "' Subnet '" + targetSubnet.ToString() + "' utilizes Network Security Group (NSG) '" + targetSubnet.NetworkSecurityGroup.ToString() + "', but the NSG resource is not added into the migration template.", targetSubnet);
                            }
                        }
                    }

                    if (targetSubnet.RouteTable != null)
                    {
                        if (targetSubnet.RouteTable.GetType() == typeof(Arm.RouteTable))
                        {
                            Arm.RouteTable routeTable = (Arm.RouteTable)targetSubnet.RouteTable;
                            if (routeTable.AzureSubscription.SubscriptionId != this.TargetSubscription.SubscriptionId)
                            {
                                this.AddAlert(AlertType.Error, "Virtual Network '" + targetVirtualNetwork.ToString() + "' Subnet '" + targetSubnet.ToString() + "' ARM Route Table '" + routeTable.ToString() + "' which is only available within the source Azure Subscription (" + this.TargetSubscription.ToString() + ")." + targetSubnet.NetworkSecurityGroup.ToString() + "', but the NSG resource is not added into the migration template.", targetSubnet);
                            }

                        }
                        else if (targetSubnet.RouteTable.GetType() == typeof(MigrationTarget.RouteTable))
                        {
                            MigrationTarget.RouteTable routeTableInMigration = this.SeekRouteTable(targetSubnet.RouteTable.ToString());

                            if (routeTableInMigration == null)
                            {
                                this.AddAlert(AlertType.Error, "Virtual Network '" + targetVirtualNetwork.ToString() + "' Subnet '" + targetSubnet.ToString() + "' utilizes Route Table '" + targetSubnet.RouteTable.ToString() + "', but the Route Table resource is not added into the migration template.", targetSubnet);
                            }
                        }
                    }
                }
            }

            foreach (MigrationTarget.ApplicationSecurityGroup targetApplicationSecurityGroup in this.ApplicationSecurityGroups)
            {
                ValidateTargetApiVersion(targetApplicationSecurityGroup);

                if (targetApplicationSecurityGroup.TargetName == string.Empty)
                    this.AddAlert(AlertType.Error, "Target Name for Application Security Group must be specified.", targetApplicationSecurityGroup);
            }

            foreach (MigrationTarget.NetworkSecurityGroup targetNetworkSecurityGroup in this.NetworkSecurityGroups)
            {
                ValidateTargetApiVersion(targetNetworkSecurityGroup);

                if (targetNetworkSecurityGroup.TargetName == string.Empty)
                    this.AddAlert(AlertType.Error, "Target Name for Network Security Group must be specified.", targetNetworkSecurityGroup);
            }

            foreach (MigrationTarget.LoadBalancer targetLoadBalancer in this.LoadBalancers)
            {
                ValidateTargetApiVersion(targetLoadBalancer);

                if (targetLoadBalancer.TargetName == string.Empty)
                    this.AddAlert(AlertType.Error, "Target Name for Load Balancer must be specified.", targetLoadBalancer);

                if (targetLoadBalancer.FrontEndIpConfigurations.Count == 0)
                {
                    this.AddAlert(AlertType.Error, "Load Balancer must have a FrontEndIpConfiguration.", targetLoadBalancer);
                }
                else
                {
                    if (targetLoadBalancer.LoadBalancerType == MigrationTarget.LoadBalancerType.Internal)
                    {
                        if (targetLoadBalancer.FrontEndIpConfigurations.Count > 0)
                        {
                            if (targetLoadBalancer.FrontEndIpConfigurations[0].TargetSubnet == null)
                            {
                                this.AddAlert(AlertType.Error, "Internal Load Balancer must have an internal Subnet association.", targetLoadBalancer);
                            }
                            else
                            {
                                // russell
                                if (targetLoadBalancer.FrontEndIpConfigurations[0].TargetPrivateIPAllocationMethod == IPAllocationMethodEnum.Static)
                                {
                                    if (!IPv4CIDR.IsIpAddressInAddressPrefix(targetLoadBalancer.FrontEndIpConfigurations[0].TargetSubnet.AddressPrefix, targetLoadBalancer.FrontEndIpConfigurations[0].TargetPrivateIpAddress))
                                    {
                                        this.AddAlert(AlertType.Error, "Load Balancer '" + targetLoadBalancer.ToString() + "' IP Address '" + targetLoadBalancer.FrontEndIpConfigurations[0].TargetPrivateIpAddress + "' is not valid within Subnet '" + targetLoadBalancer.FrontEndIpConfigurations[0].TargetSubnet.ToString() + "' Address Prefix '" + targetLoadBalancer.FrontEndIpConfigurations[0].TargetSubnet.AddressPrefix + "'.", targetLoadBalancer);
                                    }
                                    else
                                    {
                                        if (targetLoadBalancer.FrontEndIpConfigurations[0].TargetVirtualNetwork != null &&
                                            targetLoadBalancer.FrontEndIpConfigurations[0].TargetVirtualNetwork.GetType() == typeof(Azure.Arm.VirtualNetwork) &&
                                            IPv4CIDR.IsValidIpAddress(targetLoadBalancer.FrontEndIpConfigurations[0].TargetPrivateIpAddress) // Only worth passing to Azure for Availability validation if the IP address is valid.
                                            )
                                        {
                                            Arm.VirtualNetwork armVirtualNetwork = (Arm.VirtualNetwork)targetLoadBalancer.FrontEndIpConfigurations[0].TargetVirtualNetwork;
                                            (bool isAvailable, List<String> availableIps) = await armVirtualNetwork.IsIpAddressAvailable(targetLoadBalancer.FrontEndIpConfigurations[0].TargetPrivateIpAddress);

                                            if (!isAvailable)
                                            {
                                                this.AddAlert(AlertType.Error, "Load Balancer '" + targetLoadBalancer.ToString() + "' IP Address '" + targetLoadBalancer.FrontEndIpConfigurations[0].TargetPrivateIpAddress + "' is not available within Virtual Network '" + targetLoadBalancer.FrontEndIpConfigurations[0].TargetVirtualNetwork.ToString() + "'.", targetLoadBalancer);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (targetLoadBalancer.FrontEndIpConfigurations[0].PublicIp == null)
                        {
                            this.AddAlert(AlertType.Error, "Public Load Balancer must have a Public IP association.", targetLoadBalancer);
                        }
                        else
                        {
                            if (targetLoadBalancer.FrontEndIpConfigurations[0].PublicIp.GetType() == typeof(MigrationTarget.PublicIp))
                            {
                                MigrationTarget.PublicIp migrationTargetPublicIp = (MigrationTarget.PublicIp)targetLoadBalancer.FrontEndIpConfigurations[0].PublicIp;

                                // Ensure the selected Public IP Address is "in the migration" as a target new Public IP Object
                                bool publicIpExistsInMigration = false;
                                foreach (Azure.MigrationTarget.PublicIp publicIp in this.PublicIPs)
                                {
                                    if (publicIp.TargetName == migrationTargetPublicIp.TargetName)
                                    {
                                        publicIpExistsInMigration = true;
                                        break;
                                    }
                                }

                                if (!publicIpExistsInMigration)
                                    this.AddAlert(AlertType.Error, "Load Balancer Public IP '" + migrationTargetPublicIp.TargetName + "' specified '" + targetLoadBalancer.ToString() + "' is not included in the migration template.", targetLoadBalancer);
                            }
                            else if (targetLoadBalancer.FrontEndIpConfigurations[0].PublicIp.GetType() == typeof(Arm.PublicIP))
                            {
                                Arm.PublicIP armPublicIp = (Arm.PublicIP)targetLoadBalancer.FrontEndIpConfigurations[0].PublicIp;

                                if (armPublicIp.IpConfigurationId != String.Empty)
                                    this.AddAlert(AlertType.Error, "Load Balancer referenced ARM Public IP '" + armPublicIp.Name + "' is already in use by ARM Resource '" + armPublicIp.IpConfigurationId + "'.", targetLoadBalancer);
                            }
                        }
                    }
                }
            }

            foreach (Azure.MigrationTarget.AvailabilitySet availablitySet in this.AvailablitySets)
            {
                ValidateTargetApiVersion(availablitySet);

                if (availablitySet.TargetVirtualMachines.Count == 0)
                {
                    this.AddAlert(AlertType.Error, "Availability Set '" + availablitySet.ToString() + "' does not contain any Virtual Machines.  Remove the Availability Set from the Target Resources for export or associate Virtual Machines to the Availability Set.", availablitySet);
                }
                else if (availablitySet.TargetVirtualMachines.Count == 1)
                {
                    this.AddAlert(AlertType.Warning, "Availability Set '" + availablitySet.ToString() + "' only contains a single VM.  Only utilize an Availability Set if additional VMs will be added; otherwise, a single VM instance should not reside within an Availability Set.", availablitySet);
                }

                if (!availablitySet.IsManagedDisks && !availablitySet.IsUnmanagedDisks)
                {
                    this.AddAlert(AlertType.Error, "All OS and Data Disks for Virtual Machines contained within Availablity Set '" + availablitySet.ToString() + "' should be either Unmanaged Disks or Managed Disks for consistent deployment.", availablitySet);
                }
            }

            foreach (Azure.MigrationTarget.VirtualMachine virtualMachine in this.VirtualMachines)
            {
                ValidateTargetApiVersion(virtualMachine);

                if (virtualMachine.TargetName == string.Empty)
                    this.AddAlert(AlertType.Error, "Target Name for Virtual Machine '" + virtualMachine.ToString() + "' must be specified.", virtualMachine);

                if (virtualMachine.OSVirtualHardDisk == null)
                {
                    this.AddAlert(AlertType.Error, "Virtual Machine '" + virtualMachine.ToString() + "' does not have an OS Disk.", virtualMachine);
                }

                if (virtualMachine.TargetAvailabilitySet == null)
                {
                    if (virtualMachine.OSVirtualHardDisk != null && virtualMachine.OSVirtualHardDisk.TargetStorage != null && virtualMachine.OSVirtualHardDisk.TargetStorage.StorageAccountType != StorageAccountType.Premium_LRS)
                        this.AddAlert(AlertType.Warning, "Virtual Machine '" + virtualMachine.ToString() + "' is not part of an Availability Set.  OS Disk should be migrated to Azure Premium Storage to receive an Azure SLA for single server deployments.  Existing configuration will receive no (0%) Service Level Agreement (SLA).", virtualMachine.OSVirtualHardDisk);

                    foreach (Azure.MigrationTarget.Disk dataDisk in virtualMachine.DataDisks)
                    {
                        if (dataDisk.TargetStorage != null && dataDisk.TargetStorage.StorageAccountType != StorageAccountType.Premium_LRS)
                            this.AddAlert(AlertType.Warning, "Virtual Machine '" + virtualMachine.ToString() + "' is not part of an Availability Set.  Data Disk '" + dataDisk.ToString() + "' should be migrated to Azure Premium Storage to receive an Azure SLA for single server deployments.  Existing configuration will receive no (0%) Service Level Agreement (SLA).", dataDisk);
                    }
                }
                else
                {
                    bool virtualMachineAvailabitySetExists = false;
                    foreach (MigrationTarget.AvailabilitySet targetAvailabilitySet in this.AvailablitySets)
                    {
                        if (targetAvailabilitySet.ToString() == virtualMachine.TargetAvailabilitySet.ToString())
                            virtualMachineAvailabitySetExists = true;
                    }

                    if (!virtualMachineAvailabitySetExists)
                        this.AddAlert(AlertType.Error, "Virtual Machine '" + virtualMachine.ToString() + "' utilizes Availability Set '" + virtualMachine.TargetAvailabilitySet.ToString() + "'; however, the Availability Set is not included in the Export.", virtualMachine);
                }

                if (virtualMachine.TargetSize == null)
                {
                    this.AddAlert(AlertType.Error, "Target Size for Virtual Machine '" + virtualMachine.ToString() + "' must be specified.", virtualMachine);
                }
                else
                {
                    // Ensure that the selected target size is available in the target Azure Location
                    if (this.ResourceGroup != null && this.ResourceGroup.TargetLocation != null)
                    {
                        if (this.ResourceGroup.TargetLocation.VMSizes == null || this.ResourceGroup.TargetLocation.VMSizes.Count == 0)
                        {
                            this.AddAlert(AlertType.Error, "No ARM VM Sizes are available for Azure Location '" + this.ResourceGroup.TargetLocation.DisplayName + "'.", virtualMachine);
                        }
                        else
                        {
                            // Ensure selected target VM Size is available in the Target Azure Location
                            Arm.VMSize matchedVmSize = await this.ResourceGroup.TargetLocation.SeekVmSize(virtualMachine.TargetSize.Name);
                            if (matchedVmSize == null)
                                this.AddAlert(AlertType.Error, "Specified VM Size '" + virtualMachine.TargetSize.Name + "' for Virtual Machine '" + virtualMachine.ToString() + "' is invalid as it is not available in Azure Location '" + this.ResourceGroup.TargetLocation.DisplayName + "'.", virtualMachine);
                        }
                    }

                    if (virtualMachine.OSVirtualHardDisk != null && virtualMachine.OSVirtualHardDisk.TargetStorage != null && virtualMachine.OSVirtualHardDisk.TargetStorage.StorageAccountType == StorageAccountType.Premium_LRS && !virtualMachine.TargetSize.IsStorageTypeSupported(virtualMachine.OSVirtualHardDisk.StorageAccountType))
                    {
                        this.AddAlert(AlertType.Error, "Premium Disk based Virtual Machines must be of VM Series 'B', 'DS', 'DS v2', 'DS v3', 'GS', 'GS v2', 'Ls' or 'Fs'.", virtualMachine);
                    }
                }

                foreach (Azure.MigrationTarget.NetworkInterface networkInterface in virtualMachine.NetworkInterfaces)
                {
                    // Seek the inclusion of the Network Interface in the export object
                    bool networkInterfaceExistsInExport = false;
                    foreach (Azure.MigrationTarget.NetworkInterface targetNetworkInterface in this.NetworkInterfaces)
                    {
                        if (String.Compare(networkInterface.SourceName, targetNetworkInterface.SourceName, true) == 0)
                        {
                            networkInterfaceExistsInExport = true;
                            break;
                        }
                    }

                    if (!networkInterfaceExistsInExport)
                    {
                        this.AddAlert(AlertType.Error, "Network Interface Card (NIC) '" + networkInterface.ToString() + "' is used by Virtual Machine '" + virtualMachine.ToString() + "', but is not included in the exported resources.", virtualMachine);
                    }

                    if (virtualMachine.TargetSize != null)
                    {
                        if (networkInterface.EnableAcceleratedNetworking && !virtualMachine.TargetSize.IsAcceleratedNetworkingSupported)
                        {
                            this.AddAlert(AlertType.Error, "Network Interface Card (NIC) '" + networkInterface.ToString() + "' has Accelerated Networking enabled, but the Virtual Machine must be of VM Series 'D', 'DSv2', 'DSv3', 'E', 'ESv3', 'F', 'FS', 'FSv2', 'Ms' or 'Mms' to support Accelerated Networking.", networkInterface);
                        }
                        else if (!networkInterface.EnableAcceleratedNetworking && virtualMachine.TargetSize.IsAcceleratedNetworkingSupported)
                        {
                            this.AddAlert(AlertType.Recommendation, "Network Interface Card (NIC) '" + networkInterface.ToString() + "' has Accelerated Networking disabled and the Virtual Machine Size '" + virtualMachine.TargetSize.ToString() + "' can support Accelerated Networking.  Consider enabling Accelerated Networking.", networkInterface);
                        }
                    }

                    if (networkInterface.NetworkSecurityGroup != null)
                    {
                        MigrationTarget.NetworkSecurityGroup networkSecurityGroupInMigration = this.SeekNetworkSecurityGroup(networkInterface.NetworkSecurityGroup.ToString());

                        if (networkSecurityGroupInMigration == null)
                        {
                            this.AddAlert(AlertType.Error, "Network Interface Card (NIC) '" + networkInterface.ToString() + "' utilizes Network Security Group (NSG) '" + networkInterface.NetworkSecurityGroup.ToString() + "', but the NSG resource is not added into the migration template.", networkInterface);
                        }
                    }

                    foreach (Azure.MigrationTarget.NetworkInterfaceIpConfiguration ipConfiguration in networkInterface.TargetNetworkInterfaceIpConfigurations)
                    {
                        if (ipConfiguration.TargetVirtualNetwork == null)
                            this.AddAlert(AlertType.Error, "Target Virtual Network for Virtual Machine '" + virtualMachine.ToString() + "' Network Interface '" + networkInterface.ToString() + "' must be specified.", networkInterface);
                        else
                        {
                            if (ipConfiguration.TargetVirtualNetwork.GetType() == typeof(MigrationTarget.VirtualNetwork))
                            {
                                MigrationTarget.VirtualNetwork virtualMachineTargetVirtualNetwork = (MigrationTarget.VirtualNetwork)ipConfiguration.TargetVirtualNetwork;
                                bool targetVNetExists = false;

                                foreach (MigrationTarget.VirtualNetwork targetVirtualNetwork in this.VirtualNetworks)
                                {
                                    if (targetVirtualNetwork.TargetName == virtualMachineTargetVirtualNetwork.TargetName)
                                    {
                                        targetVNetExists = true;
                                        break;
                                    }
                                }

                                if (!targetVNetExists)
                                    this.AddAlert(AlertType.Error, "Target Virtual Network '" + virtualMachineTargetVirtualNetwork.ToString() + "' for Virtual Machine '" + virtualMachine.ToString() + "' Network Interface '" + networkInterface.ToString() + "' is invalid, as it is not included in the migration / template.  Either include the source Virtual Network in the Migration Template (if this is the first time migration needing a new ARM Virtual Network), or select an existing ARM Virtual Network and Subnet to migrate the Virtual Machine into.", networkInterface);
                            }
                        }

                        if (ipConfiguration.TargetSubnet == null)
                            this.AddAlert(AlertType.Error, "Target Subnet for Virtual Machine '" + virtualMachine.ToString() + "' Network Interface '" + networkInterface.ToString() + "' must be specified.", networkInterface);
                        else
                        {
                            if (!IPv4CIDR.IsValidCIDR(ipConfiguration.TargetSubnet.AddressPrefix))
                            {
                                this.AddAlert(AlertType.Error, "Target Subnet '" + ipConfiguration.TargetSubnet.ToString() + "' used by Virtual Machine '" + virtualMachine.ToString() + "' has an invalid IPv4 Address Prefix: " + ipConfiguration.TargetSubnet.AddressPrefix, ipConfiguration.TargetSubnet);
                            }
                            else
                            {
                                if (ipConfiguration.TargetPrivateIPAllocationMethod == IPAllocationMethodEnum.Static)
                                {
                                    if (!IPv4CIDR.IsIpAddressInAddressPrefix(ipConfiguration.TargetSubnet.AddressPrefix, ipConfiguration.TargetPrivateIpAddress))
                                    {
                                        this.AddAlert(AlertType.Error, "Target IP Address '" + ipConfiguration.TargetPrivateIpAddress + "' is not valid in Subnet '" + ipConfiguration.TargetSubnet.ToString() + "' Address Prefix '" + ipConfiguration.TargetSubnet.AddressPrefix + "'.", networkInterface);
                                    }
                                }
                            }

                        }

                        if (ipConfiguration.TargetPublicIp != null)
                        {
                            if (ipConfiguration.TargetPublicIp.GetType().UnderlyingSystemType == typeof(MigrationTarget.PublicIp))
                            {
                                MigrationTarget.PublicIp publicIpInMigration = this.SeekPublicIp(ipConfiguration.TargetPublicIp.ToString());

                                if (publicIpInMigration == null)
                                {
                                    this.AddAlert(AlertType.Error, "Network Interface Card (NIC) '" + networkInterface.ToString() + "' IP Configuration '" + ipConfiguration.ToString() + "' utilizes Public IP '" + ipConfiguration.TargetPublicIp.ToString() + "', but the Public IP resource is not added into the migration template.", networkInterface);
                                }
                            }
                            else if (ipConfiguration.TargetPublicIp.GetType().UnderlyingSystemType == typeof(Arm.PublicIP))
                            {
                                Arm.PublicIP existingArmPublicIp = (Arm.PublicIP)ipConfiguration.TargetPublicIp;

                                if (existingArmPublicIp.IpConfigurationId != String.Empty)
                                {
                                    this.AddAlert(AlertType.Error, "Network Interface Card (NIC) '" + networkInterface.ToString() + "' IP Configuration '" + ipConfiguration.ToString() + "' utilizes existing Public IP '" + ipConfiguration.TargetPublicIp.ToString() + "', but the Public IP resource is already used by existing ARM Resource '" + existingArmPublicIp.IpConfigurationId + "'.", networkInterface);
                                }
                            }
                        }
                    }
                }

                if (virtualMachine.OSVirtualHardDisk != null)
                    ValidateVMDisk(virtualMachine.OSVirtualHardDisk);

                foreach (MigrationTarget.Disk dataDisk in virtualMachine.DataDisks)
                {
                    ValidateVMDisk(dataDisk);

                    if (!dataDisk.Lun.HasValue || dataDisk.Lun.Value == -1)
                    {
                        this.AddAlert(AlertType.Error, "Data Disk '" + dataDisk.ToString() + "' must have a valid LUN Index assigned.", dataDisk);
                    }
                    else
                    {
                        if (virtualMachine.TargetSize != null)
                        {
                            if (dataDisk.Lun > virtualMachine.TargetSize.maxDataDiskCount - 1)
                                this.AddAlert(AlertType.Error, "Data Disk '" + dataDisk.ToString() + "' LUN index " + dataDisk.Lun.Value.ToString() + " exceeds the maximum LUN of " + (virtualMachine.TargetSize.maxDataDiskCount - 1).ToString() + " allowed by VM Size '" + virtualMachine.TargetSize.ToString() + "'.", dataDisk);
                        }

                        int lunCount = virtualMachine.DataDisks.Where(a => a.Lun == dataDisk.Lun).Count();
                        if (lunCount > 1)
                        {
                            this.AddAlert(AlertType.Error, "Multiple data disks are assigned to LUN " + dataDisk.Lun.ToString() + " on Virtual Machine '" + virtualMachine.ToString() + "'.  Data Disk LUNs must be unique.", dataDisk);
                        }
                    }
                }

                if (!virtualMachine.IsManagedDisks && !virtualMachine.IsUnmanagedDisks)
                {
                    this.AddAlert(AlertType.Error, "All OS and Data Disks for Virtual Machine '" + virtualMachine.ToString() + "' should be either Unmanaged Disks or Managed Disks for consistent deployment.", virtualMachine);
                }
            }

            foreach (Azure.MigrationTarget.Disk targetDisk in this.Disks)
            {
                ValidateTargetApiVersion(targetDisk);

                ValidateDiskStandards(targetDisk);
            }

            // todo now asap - Add test for NSGs being present in Migration
            //MigrationTarget.NetworkSecurityGroup targetNetworkSecurityGroup = (MigrationTarget.NetworkSecurityGroup)this.SeekNetworkSecurityGroup(targetSubnet.NetworkSecurityGroup.ToString());
            //if (targetNetworkSecurityGroup == null)
            //{
            //    this.AddAlert(AlertType.Error, "Subnet '" + subnet.name + "' utilized ASM Network Security Group (NSG) '" + targetSubnet.NetworkSecurityGroup.ToString() + "', which has not been added to the ARM Subnet as the NSG was not included in the ARM Template (was not selected as an included resources for export).", targetNetworkSecurityGroup);
            //}

            // todo add error if existing target disk storage is not in the same data center / region as vm.

            if (AfterResourceValidation != null)
                await AfterResourceValidation.Invoke();
        }

        private void ValidateTargetApiVersion(Core.MigrationTarget migrationTarget)
        {
            if (migrationTarget.ApiVersion == null || migrationTarget.ApiVersion == String.Empty)
            {
                if (this.TargetSubscription != null)
                {
                    migrationTarget.DefaultApiVersion(this.TargetSubscription);
                }

                if (migrationTarget.ApiVersion == null || migrationTarget.ApiVersion.Length == 0)
                    this.AddAlert(AlertType.Error, "Target Azure RM API Version must be specificed for " + migrationTarget.FriendlyObjectName + " '" + migrationTarget.TargetNameResult + "'.", migrationTarget);
            }
        }

        internal bool IsStorageAccountVmDiskTarget(StorageAccount targetStorageAccount)
        {
            foreach (VirtualMachine virtrualMachine in this.VirtualMachines)
            {
                if (virtrualMachine.OSVirtualHardDisk != null)
                {
                    if (virtrualMachine.OSVirtualHardDisk.TargetStorage != null && virtrualMachine.OSVirtualHardDisk.TargetStorage.GetType() == typeof(StorageAccount))
                    {
                        StorageAccount osDiskStorageAccount = (StorageAccount)virtrualMachine.OSVirtualHardDisk.TargetStorage;
                        if (osDiskStorageAccount == targetStorageAccount)
                        {
                            return true;
                        }
                    }
                }

                foreach (Disk dataDisk in virtrualMachine.DataDisks)
                {
                    if (dataDisk.TargetStorage != null && dataDisk.TargetStorage.GetType() == typeof(StorageAccount))
                    {
                        StorageAccount dataDiskStorageAccount = (StorageAccount)dataDisk.TargetStorage;
                        if (dataDiskStorageAccount == targetStorageAccount)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void ValidateVMDisk(MigrationTarget.Disk targetDisk)
        {
            if (targetDisk.IsManagedDisk)
            {
                // All Managed Disks are included in the Export Artifacts, so we aren't including a call here to ValidateDiskStandards for Managed Disks.  
                // Only non Managed Disks are validated against Disk Standards below.

                // VM References a managed disk, ensure it is included in the Export Artifacts
                bool targetDiskInExport = false;
                foreach (Azure.MigrationTarget.Disk exportDisk in this.Disks)
                {
                    if (targetDisk.SourceName == exportDisk.SourceName)
                        targetDiskInExport = true;
                }

                if (!targetDiskInExport && targetDisk.ParentVirtualMachine != null)
                {
                    this.AddAlert(AlertType.Error, "Virtual Machine '" + targetDisk.ParentVirtualMachine.SourceName + "' references Managed Disk '" + targetDisk.SourceName + "' which has not been added as an export resource.", targetDisk.ParentVirtualMachine);
                }
            }
            else
            {
                // We are calling Validate Disk Standards here (only for non-managed disks, as noted above) as all Managed Disks are validated for Disk Standards through the ExportArtifacts.Disks Collection
                ValidateDiskStandards(targetDisk);
            }
        }

        private void ValidateDiskStandards(MigrationTarget.Disk targetDisk)
        {
            if (targetDisk.DiskSizeInGB == 0)
                this.AddAlert(AlertType.Error, "Disk '" + targetDisk.ToString() + "' does not have a Disk Size defined.  Disk Size (not to exceed 65536 GB) is required.", targetDisk);

            if (targetDisk.Source != null)
            {
                IDisk sourceDisk = (IDisk)targetDisk.Source;

                if (targetDisk.IsSmallerThanSourceDisk)
                    this.AddAlert(AlertType.Error, "Disk '" + targetDisk.ToString() + "' Size of " + targetDisk.DiskSizeInGB.ToString() + " GB cannot be smaller than the source Disk Size of " + sourceDisk.DiskSizeGb.ToString() + " GB.", targetDisk);

                if (targetDisk.IsTargetLunDifferentThanSourceLun)
                    this.AddAlert(AlertType.Warning, "Disk '" + targetDisk.ToString() + "' target LUN " + targetDisk.Lun.ToString() + " does not match the source LUN " + sourceDisk.Lun.ToString() + ".", targetDisk);

                if (sourceDisk.GetType() == typeof(Azure.Arm.ClassicDisk))
                {
                    Azure.Arm.ClassicDisk classicDisk = (Azure.Arm.ClassicDisk)targetDisk.Source;

                    if (classicDisk.IsEncrypted)
                    {
                        this.AddAlert(AlertType.Error, "Disk '" + targetDisk.ToString() + "' is encrypted.  MigAz does not contain support for moving encrypted Azure Compute VMs.", targetDisk);
                    }
                }
            }

            if (targetDisk.TargetStorage == null)
                this.AddAlert(AlertType.Error, "Disk '" + targetDisk.ToString() + "' Target Storage must be specified.", targetDisk);
            else
            {
                if (targetDisk.TargetStorage.GetType() == typeof(Azure.Arm.StorageAccount))
                {
                    Arm.StorageAccount armStorageAccount = (Arm.StorageAccount)targetDisk.TargetStorage;
                    if (armStorageAccount.Location.Name != this.ResourceGroup.TargetLocation.Name)
                    {
                        this.AddAlert(AlertType.Error, "Target Storage Account '" + armStorageAccount.Name + "' is not in the same region (" + armStorageAccount.Location.Name + ") as the Target Resource Group '" + this.ResourceGroup.ToString() + "' (" + this.ResourceGroup.TargetLocation.Name + ").", targetDisk);
                    }
                }
                //else if (targetDisk.TargetStorage.GetType() == typeof(Azure.MigrationTarget.StorageAccount))
                //{
                //    Azure.MigrationTarget.StorageAccount targetStorageAccount = (Azure.MigrationTarget.StorageAccount)targetDisk.TargetStorage;
                //    bool targetStorageExists = false;

                //    foreach (Azure.MigrationTarget.StorageAccount storageAccount in this.StorageAccounts)
                //    {
                //        if (storageAccount.ToString() == targetStorageAccount.ToString())
                //        {
                //            targetStorageExists = true;
                //            break;
                //        }
                //    }

                //    if (!targetStorageExists)
                //        this.AddAlert(AlertType.Error, "Target Storage Account '" + targetStorageAccount.ToString() + "' for Disk '" + targetDisk.ToString() + "' is invalid, as it is not included in the migration / template.", targetDisk);
                //}

                if (targetDisk.TargetStorage.GetType() == typeof(Azure.MigrationTarget.StorageAccount) ||
                    targetDisk.TargetStorage.GetType() == typeof(Azure.Arm.StorageAccount))
                {
                    if (targetDisk.TargetStorageAccountBlob == null || targetDisk.TargetStorageAccountBlob.Trim().Length == 0)
                        this.AddAlert(AlertType.Error, "Target Storage Blob Name is required.", targetDisk);
                    else if (!targetDisk.TargetStorageAccountBlob.ToLower().EndsWith(".vhd"))
                        this.AddAlert(AlertType.Error, "Target Storage Blob Name '" + targetDisk.TargetStorageAccountBlob + "' for Disk is invalid, as it must end with '.vhd'.", targetDisk);
                }

                if (targetDisk.TargetStorage.StorageAccountType == StorageAccountType.Premium_LRS)
                {
                    // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/disks-types#premium-ssd

                    if (targetDisk.DiskSizeInGB > 0 && targetDisk.DiskSizeInGB < 32)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 32 GB (P4), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 32 && targetDisk.DiskSizeInGB < 64)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 64 GB (P6), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 64 && targetDisk.DiskSizeInGB < 128)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 128 GB (P10), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 128 && targetDisk.DiskSizeInGB < 512)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 512 GB (P20), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 512 && targetDisk.DiskSizeInGB < 1023)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 1023 GB (P30), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 1023 && targetDisk.DiskSizeInGB < 2047)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 2047 GB (P40), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 2047 && targetDisk.DiskSizeInGB < 4095)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 4095 GB (P50), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 4095 && targetDisk.DiskSizeInGB < 8192)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 8192 GB (P60), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 8192 && targetDisk.DiskSizeInGB < 16384)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 16384 GB (P70), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                    else if (targetDisk.DiskSizeInGB > 16384 && targetDisk.DiskSizeInGB < 32767)
                        this.AddAlert(AlertType.Recommendation, "Consider using disk size 32767 GB (P80), as this disk will be billed at that capacity per Azure Premium Storage billing sizes.", targetDisk);
                }
            }
        }

        private void AddAlert(AlertType alertType, string message, object sourceObject)
        {
            this.Alerts.Add(new MigAzGeneratorAlert(alertType, message, sourceObject));
        }
    }
}

