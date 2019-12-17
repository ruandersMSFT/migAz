// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System;

namespace MigAz.Azure.Core.ArmTemplate
{
    public class Extension : ArmResource
    {
        public Extension(Guid executionGuid) : base(executionGuid)
        {
            type = "extensions";
        }
    }

    public class Extension_Properties
    {
        public string publisher;
        public string type;
        public string typeHandlerVersion;
        public bool autoUpgradeMinorVersion;
        public Dictionary<string, string> settings;
    }

    public class VirtualNetworkGateway : ArmResource
    {
        public VirtualNetworkGateway(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/virtualNetworkGateways";
        }
    }

    public class VirtualNetworkGateway_Properties
    {
        public List<IpConfiguration> ipConfigurations;
        public VirtualNetworkGateway_Sku sku;
        public string gatewayType; // VPN or ER
        public string vpnType; // RouteBased or PolicyBased
        public string enableBgp = "false";
        public string activeActive = "false";
        public VPNClientConfiguration vpnClientConfiguration;
    }

    public class VirtualNetworkGateway_Sku
    {
        public string name;
        public string tier;
        public int capacity;
    }

    public class LocalNetworkGateway : ArmResource
    {
        public LocalNetworkGateway(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/localNetworkGateways";
        }
    }

    public class LocalNetworkGateway_Properties
    {
        public AddressSpace localNetworkAddressSpace;
        public string gatewayIpAddress;
    }

    public class GatewayConnection : ArmResource
    {
        public GatewayConnection(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/connections";
        }
    }

    public class GatewayConnection_Properties
    {
        public Reference virtualNetworkGateway1;
        public Reference localNetworkGateway2;
        public string connectionType;
        public Reference peer;
        public long routingWeight = 10;
        public string sharedKey;
    }

    public class VPNClientConfiguration
    {
        public AddressSpace vpnClientAddressPool;
        public List<VPNClientCertificate> vpnClientRootCertificates;
        public List<VPNClientCertificate> vpnClientRevokedCertificates;
    }

    public class VPNClientCertificate
    {
        public string name;
        public VPNClientCertificate_Properties properties;
    }

    public class VPNClientCertificate_Properties
    {
        public string PublicCertData;
        public string Thumbprint;
    }

    public class RouteTable : ArmResource
    {
        public RouteTable(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/routeTables";
        }
    }

    public class RouteTable_Properties
    {
        public List<Route> routes;
    }

    public class Route
    {
        public string name;
        public Route_Properties properties;
    }

    public class Route_Properties
    {
        public string addressPrefix;
        public string nextHopType;
        public string nextHopIpAddress;
    }

    public class ApplicationSecurityGroup : ArmResource
    {
        public ApplicationSecurityGroup(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/applicationSecurityGroups";
        }
    }

    public class NetworkSecurityGroup : ArmResource
    {
        public NetworkSecurityGroup(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/networkSecurityGroups";
        }
    }

    public class NetworkSecurityGroup_Properties
    {
        public List<SecurityRule> securityRules;
    }

    public class SecurityRule
    {
        public string name;
        public SecurityRule_Properties properties;
    }

    public class SecurityRule_Properties
    {
        public string description;
        public string protocol;
        public string sourceAddressPrefix;
        public List<string> sourceAddressPrefixes;
        public string sourcePortRange;
        public List<string> sourcePortRanges;
        public List<Reference> sourceApplicationSecurityGroups;
        public string destinationAddressPrefix;
        public List<string> destinationAddressPrefixes;
        public List<Reference> destinationApplicationSecurityGroups;
        public string destinationPortRange;
        public string access;
        public long priority;
        public string direction;
    }

    public class VirtualNetwork : ArmResource
    {
        public VirtualNetwork(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/virtualNetworks";
        }
    }

    public class AddressSpace
    {
        public List<string> addressPrefixes;
    }

    public class VirtualNetwork_Properties
    {
        public AddressSpace addressSpace;
        public List<Subnet> subnets;
        public VirtualNetwork_dhcpOptions dhcpOptions;
    }

    public class Subnet
    {
        public string name;
        public Subnet_Properties properties;
    }

    public class Subnet_Properties
    {
        public string addressPrefix;
        public Reference networkSecurityGroup;
        public Reference routeTable;
    }

    public class VirtualNetwork_dhcpOptions
    {
        public List<string> dnsServers;
    }

    public class StorageAccount : ArmResource
    {
        public StorageAccount(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Storage/storageAccounts";
        }
    }

    public class StorageAccount_Properties
    {
        public string accountType;
    }

    public class LoadBalancer : ArmResource
    {
        public LoadBalancer(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/loadBalancers";
        }
    }

    public class LoadBalancer_Properties
    {
        public List<FrontendIPConfiguration> frontendIPConfigurations;
        public List<Hashtable> backendAddressPools;
        public List<InboundNatRule> inboundNatRules;
        public List<LoadBalancingRule> loadBalancingRules;
        public List<Probe> probes;
    }

    public class FrontendIPConfiguration
    {
        public string name = "default";
        public FrontendIPConfiguration_Properties properties;
    }

    public class FrontendIPConfiguration_Properties
    {
        public Reference publicIPAddress;
        public string privateIPAllocationMethod;
        public string privateIPAddress;
        public Reference subnet;
    }

    public class InboundNatRule
    {
        public string name;
        public InboundNatRule_Properties properties;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(InboundNatRule))
                return false;

            return ((InboundNatRule)obj).name == this.name;
        }
    }

    public class InboundNatRule_Properties
    {
        public long frontendPort;
        public long backendPort;
        public bool enableFloatingIP;
        public long idleTimeoutInMinutes = 4; // https://azure.microsoft.com/en-us/blog/new-configurable-idle-timeout-for-azure-load-balancer/
        public string protocol;
        public Reference frontendIPConfiguration;
    }

    public class LoadBalancingRule
    {
        public string name;
        public LoadBalancingRule_Properties properties;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(LoadBalancingRule))
                return false;

            return ((LoadBalancingRule)obj).name == this.name;
        }
    }

    public class LoadBalancingRule_Properties
    {
        public Reference frontendIPConfiguration;
        public Reference backendAddressPool;
        public Reference probe;
        public string protocol;
        public long frontendPort;
        public long backendPort;
        public long idleTimeoutInMinutes = 15;
        public string loadDistribution = "SourceIP";
        public bool enableFloatingIP = false;
    }

    public class Probe
    {
        public string name;
        public Probe_Properties properties;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Probe))
                return false;

            return ((Probe)obj).name == this.name;
        }
    }

    public class Probe_Properties
    {
        public string protocol;
        public long port;
        public long intervalInSeconds = 15;
        public long numberOfProbes = 2;
        public string requestPath;
    }

    public class PublicIPAddress : ArmResource
    {
        public PublicIPAddress(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/publicIPAddresses";
        }
    }

    public class PublicIPAddress_Properties
    {
        public string publicIPAllocationMethod = "Dynamic";
        public Hashtable dnsSettings;
    }

    public class NetworkInterface : ArmResource
    {
        public NetworkInterface(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Network/networkInterfaces";
        }
    }

    public class NetworkInterface_Properties
    {
        public List<IpConfiguration> ipConfigurations;
        public bool enableIPForwarding = false;
        public bool enableAcceleratedNetworking = false;
        public Reference NetworkSecurityGroup;
    }

    public class IpConfiguration
    {
        public string name;
        public IpConfiguration_Properties properties;
    }

    public class IpConfiguration_Properties
    {
        public string privateIPAllocationMethod = "Dynamic";
        public string privateIPAddress;
        public Reference publicIPAddress;
        public Reference subnet;
        public List<Reference> loadBalancerBackendAddressPools;
        public List<Reference> loadBalancerInboundNatRules;
    }

    public class AvailabilitySet : ArmResource
    {
        public AvailabilitySet(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Compute/availabilitySets";
        }
    }

    public class AvailabilitySet_Properties
    {
        public long platformUpdateDomainCount;
        public long platformFaultDomainCount;
    }

    public class VirtualMachine : ArmResource
    {
        public List<ArmResource> resources;
        public VirtualMachine(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Compute/virtualMachines";
        }
        public Dictionary<string, string> plan;
    }

    public class VirtualMachine_Properties
    {
        public HardwareProfile hardwareProfile;
        public Reference availabilitySet;
        public OsProfile osProfile;
        public StorageProfile storageProfile;
        public NetworkProfile networkProfile;
        public DiagnosticsProfile diagnosticsProfile;
    }

    public class HardwareProfile
    {
        public string vmSize;
    }

    public class OsProfile
    {
        public string computerName;
        public string adminUsername;
        public string adminPassword;
    }

    public class StorageProfile
    {
        public ImageReference imageReference;
        public OsDisk osDisk;
        public List<DataDisk> dataDisks;
    }

    public class ImageReference
    {
        public string publisher;
        public string offer;
        public string sku;
        public string version;
    }

    public class OsDisk
    {
        public string name;
        public string osType;
        public Vhd vhd;
        public Reference managedDisk;
        public string caching;
        public string createOption;
        public DiskEncrpytionSettings encryptionSettings;
    }

    public class DataDisk
    {
        public string name;
        public Vhd vhd;
        public Reference managedDisk;
        public string caching;
        public string createOption;
        public long? diskSizeGB;
        public long lun;
        public DiskEncrpytionSettings encryptionSettings;
    }

    public class DiskEncrpytionSettings
    {
        public bool enabled;
        public DiskEncryptionKeySettings diskEncryptionKey;
        public KeyEncryptionKeySettings keyEncryptionKey;
    }

    public class KeyEncryptionKeySettings
    {
        public Reference sourceVault;
        public string keyUrl;
    }
    public class DiskEncryptionKeySettings
    {
        public Reference sourceVault;
        public string secretUrl;
    }

    public class Vhd
    {
        public string uri;
    }

    public class ManagedDisk : Disk
    {
        public ManagedDisk(Guid executionGuid) : base(executionGuid)
        {
        }
    }

    public class ManagedDisk_Sku
    {
        public string name = "Premium_LRS";
    }

    public class ManagedDisk_Properties
    {
        public ManagedDiskCreationData_Properties creationData;
        public int diskSizeGb;
    }

    public class ManagedDiskCreationData_Properties
    {
        public string createOption = "Empty";
        public string sourceUri ;
    }

    public class Snapshot : Disk
    {
        public Snapshot(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Compute/snapshots";
        }
    }

    public class Disk : ArmResource
    {
        public Disk(Guid executionGuid) : base(executionGuid)
        {
            type = "Microsoft.Compute/disks";
        }
    }

    public class Disk_Properties
    {
        public Disk_CreationData creationData;
        public string accountType;
        public string diskSizeGB;
        public string osType;
    }

    public class Disk_CreationData
    {
        public string createOption; // Copy (from Snapshot), Empty, Import (from blob)
        public string sourceUri;
        public string sourceResourceId;
    }

    public class NetworkProfile
    {
        public List<NetworkProfile_NetworkInterface> networkInterfaces;
    }

    public class NetworkProfile_NetworkInterface
    {
        public string id;
        public NetworkProfile_NetworkInterface_Properties properties;
    }

    public class NetworkProfile_NetworkInterface_Properties
    {
        public bool primary = true;
    }

    public class DiagnosticsProfile
    {
        public BootDiagnostics bootDiagnostics;
    }

    public class BootDiagnostics
    {
        public bool enabled;
        public string storageUri;
    }

    public class Reference
    {
        public string id;
    }

    public class ArmResource
    {
        public string type;
        public string apiVersion;
        public string name;
        public string location = "[resourceGroup().location]";
        public Dictionary<string, string> tags;
        public List<string> dependsOn;
        public Dictionary<string, string> sku = null;
        public object properties;

        private ArmResource() { }

        public ArmResource(Guid executionGuid)
        {
            //if (app.Default.AllowTag) // TODO
            //{
                tags = new Dictionary<string, string>();
                tags.Add("migAz", executionGuid.ToString());
            //}
        }

        public override bool Equals(object obj)
        {
            try
            {
                ArmResource other = (ArmResource)obj;
                return this.type == other.type && this.name == other.name;
            }
            catch
            {
                return false;
            }
        }
    }

    public class Parameter
    {
        public string type;
        public string value;
    }

    public class Template
    {
        public string schemalink = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#";
        public string contentVersion = "1.0.0.0";
        public Dictionary<string, Parameter> parameters;
        public Dictionary<string, string> variables;
        public List<ArmResource> resources;
    }
}

