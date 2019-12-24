// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using MigAz.Azure.Core.Interface;
using MigAz.Azure.Core.ArmTemplate;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;
using MigAz.Azure.Core.Generator;
using System.Collections;
using MigAz.Azure.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using MigAz.Azure.Core;
using MigAz.Azure.Interface;
using Microsoft.Identity.Client;

namespace MigAz.Azure.Generator
{
    public class TemplateGenerator
    {
        private Guid _ExecutionGuid = Guid.NewGuid();
        private List<ArmResource> _Resources = new List<ArmResource>();
        private Dictionary<string, Core.ArmTemplate.Parameter> _Parameters = new Dictionary<string, Core.ArmTemplate.Parameter>();
        private List<MigrationTarget.StorageAccount> _TemporaryStorageAccounts = new List<MigrationTarget.StorageAccount>();
        private ILogProvider _logProvider;
        private IStatusProvider _statusProvider;
        private Dictionary<string, MemoryStream> _TemplateStreams = new Dictionary<string, MemoryStream>();
        private string _OutputDirectory = String.Empty;
        private List<BlobCopyDetail> _CopyBlobDetails = new List<BlobCopyDetail>();
        private Int32 _AccessSASTokenLifetime = 3600;
        private ExportArtifacts _ExportArtifacts;
        private bool _BuildEmpty = false;
        private AzureTokenProvider _TargetAzureTokenProvider = null;

        public delegate Task AfterTemplateChangedHandler(TemplateGenerator sender);
        public event EventHandler AfterTemplateChanged;

        private TemplateGenerator() { }

        public TemplateGenerator(ILogProvider logProvider, IStatusProvider statusProvider)
        {
            _logProvider = logProvider;
            _statusProvider = statusProvider;
        }

        public ILogProvider LogProvider
        {
            get { return _logProvider; }
        }

        public IStatusProvider StatusProvider
        {
            get { return _statusProvider; }
        }


        public AzureSubscription TargetSubscription
        {
            get
            {
                if (_ExportArtifacts == null)
                    return null;

                return _ExportArtifacts.TargetSubscription;
            }
        }

        public Guid ExecutionGuid
        {
            get { return _ExecutionGuid; }
        }

        public bool BuildEmpty
        {
            get { return _BuildEmpty; }
            set { _BuildEmpty = value; }
        }
        public Int32 AccessSASTokenLifetimeSeconds
        {
            get { return _AccessSASTokenLifetime; }
            set { _AccessSASTokenLifetime = value; }
        }

        public AzureTokenProvider TargetAzureTokenProvider
        {
            get { return _TargetAzureTokenProvider; }
            set { _TargetAzureTokenProvider = value; }
        }

        public String OutputDirectory
        {
            get { return _OutputDirectory; }
            set
            {
                if (value == null)
                    throw new ArgumentException("OutputDirectory cannot be null.");

                if (value.EndsWith(@"\"))
                    _OutputDirectory = value;
                else
                    _OutputDirectory = value + @"\";
            }
        }

        public List<ArmResource> Resources { get { return _Resources; } }
        public Dictionary<string, Core.ArmTemplate.Parameter> Parameters { get { return _Parameters; } }
        public Dictionary<string, MemoryStream> TemplateStreams { get { return _TemplateStreams; } }

        public bool IsProcessed(ArmResource resource)
        {
            return this.Resources.Contains(resource);
        }

        private void AddResource(ArmResource resource)
        {
            if (!IsProcessed(resource))
            {
                this.Resources.Add(resource);
                _logProvider.WriteLog("TemplateResult.AddResource", resource.type + resource.name + " added.");
            }
            else
                _logProvider.WriteLog("TemplateResult.AddResource", resource.type + resource.name + " already exists.");

        }

        public bool ResourceExists(Type type, string objectName)
        {
            object resource = GetResource(type, objectName);
            return resource != null;
        }

        public object GetResource(Type type, string objectName)
        {
            foreach (ArmResource armResource in this.Resources)
            {
                if (armResource.GetType() == type && armResource.name == objectName)
                    return armResource;
            }

            return null;
        }

        public async Task GenerateStreams()
        {
            _ExecutionGuid = Guid.NewGuid();

            LogProvider.WriteLog("GenerateStreams", "Start - Execution " + this.ExecutionGuid.ToString());

            TemplateStreams.Clear();
            Resources.Clear();
            Parameters.Clear();
            _CopyBlobDetails.Clear();
            _TemporaryStorageAccounts.Clear();

            if (_ExportArtifacts != null)
            {
                if (_ExportArtifacts.HasErrors)
                {
                    throw new InvalidOperationException("Export Streams cannot be generated when there are error(s).  Please resolve all template error(s) to enable export stream generation.");
                } 


                LogProvider.WriteLog("GenerateStreams", "Start processing selected Virtual Network Gateway(s)");
                foreach (MigrationTarget.VirtualNetworkGateway targetVirtualNetworkGateway in _ExportArtifacts.VirtualNetworkGateways)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Virtual Network Gateway : " + targetVirtualNetworkGateway.ToString());
                    await BuildVirtualNetworkGateway(targetVirtualNetworkGateway);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Virtual Network Gateway(s)");


                LogProvider.WriteLog("GenerateStreams", "Start processing selected Virtual Network Gateway Connections(s)");
                foreach (MigrationTarget.VirtualNetworkGatewayConnection targetVirtualNetworkGatewayConnection in _ExportArtifacts.VirtualNetworkGatewayConnections)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Virtual Network Gateway Connection : " + targetVirtualNetworkGatewayConnection.ToString());
                    await BuildVirtualNetworkGatewayConnection(targetVirtualNetworkGatewayConnection);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Virtual Network Gateway Connections(s)");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Local Network Gateway(s)");
                foreach (MigrationTarget.LocalNetworkGateway targetLocalNetworkGateway in _ExportArtifacts.LocalNetworkGateways)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Local Network Gateway : " + targetLocalNetworkGateway.ToString());
                    await BuildLocalNetworkGateway(targetLocalNetworkGateway);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Local Network Gateway(s)");

                foreach (MigrationTarget.StorageAccount targetStorageAccount in _ExportArtifacts.StorageAccounts)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Storage Account : " + targetStorageAccount.ToString());
                    if (!_ExportArtifacts.IsStorageAccountVmDiskTarget(targetStorageAccount))
                    {
                        // We are only going to add the Storage Account into the Resourge Group Deployment JSON if the Storage Account IS NOT A TARGET Storage Account for VM Disks
                        // This is done so the ResourceGroupDeployment does not fail with error that the storage account defined in the JSON Template already exists.
                        // Any VM targeted Storage Account will be created via the MigAz.ps1 execution and should not be included in the JSON for Resource Group Deployment.
                        BuildStorageAccountObject(targetStorageAccount);
                    }
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Storage Accounts");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Application Security Groups");
                foreach (MigrationTarget.ApplicationSecurityGroup targetApplicationSecurityGroup in _ExportArtifacts.ApplicationSecurityGroups)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Application Security Group : " + targetApplicationSecurityGroup.ToString());
                    await BuildApplicationSecurityGroup(targetApplicationSecurityGroup);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Application Security Groups");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Network Security Groups");
                foreach (MigrationTarget.NetworkSecurityGroup targetNetworkSecurityGroup in _ExportArtifacts.NetworkSecurityGroups)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Network Security Group : " + targetNetworkSecurityGroup.ToString());
                    await BuildNetworkSecurityGroup(targetNetworkSecurityGroup);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Network Security Groups");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Route Tables");
                foreach (MigrationTarget.RouteTable targetRouteTable in _ExportArtifacts.RouteTables)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Route Tables : " + targetRouteTable.ToString());
                    await BuildRouteTable(targetRouteTable);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Route Tables");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Public IPs");
                foreach (MigrationTarget.PublicIp targetPublicIp in _ExportArtifacts.PublicIPs)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Public IP : " + targetPublicIp.ToString());
                    await BuildPublicIPAddressObject(targetPublicIp);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Public IPs");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Virtual Networks");
                foreach (Azure.MigrationTarget.VirtualNetwork virtualNetwork in _ExportArtifacts.VirtualNetworks)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Virtual Network : " + virtualNetwork.ToString());
                    await BuildVirtualNetworkObject(virtualNetwork);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Virtual Networks");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Load Balancers");
                foreach (Azure.MigrationTarget.LoadBalancer loadBalancer in _ExportArtifacts.LoadBalancers)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Load Balancer : " + loadBalancer.ToString());
                    await BuildLoadBalancerObject(loadBalancer);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Load Balancers");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Availablity Sets");
                foreach (Azure.MigrationTarget.AvailabilitySet availablitySet in _ExportArtifacts.AvailablitySets)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Availablity Set : " + availablitySet.ToString());
                    BuildAvailabilitySetObject(availablitySet);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Availablity Sets");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Network Interfaces");
                foreach (Azure.MigrationTarget.NetworkInterface networkInterface in _ExportArtifacts.NetworkInterfaces)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Network Interface : " + networkInterface.ToString());
                    BuildNetworkInterfaceObject(networkInterface);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Network Interfaces");


                LogProvider.WriteLog("GenerateStreams", "Start Exporting Managed Disk Definition(s)");
                foreach (Azure.MigrationTarget.Disk targetDisk in _ExportArtifacts.Disks)
                {
                    StatusProvider.UpdateStatus("BUSY: Creating Copy Blob Details for Disk : " + targetDisk.ToString());
                    if (!this.BuildEmpty)
                    {
                        this._CopyBlobDetails.Add(await BuildCopyBlob(targetDisk, _ExportArtifacts.ResourceGroup));
                    }

                    if (targetDisk.IsManagedDisk)
                    {
                        StatusProvider.UpdateStatus("BUSY: Creating ARM Managed Disk : " + targetDisk.ToString());
                        await BuildManagedDiskObject(targetDisk);
                    }
                }
                LogProvider.WriteLog("GenerateStreams", "End Exporting Managed Disk Definition(s)");

                LogProvider.WriteLog("GenerateStreams", "Start processing selected Cloud Services / Virtual Machines");
                foreach (Azure.MigrationTarget.VirtualMachine virtualMachine in _ExportArtifacts.VirtualMachines)
                {
                    StatusProvider.UpdateStatus("BUSY: Exporting Virtual Machine : " + virtualMachine.ToString());
                    await BuildVirtualMachineObject(virtualMachine, _ExportArtifacts.ResourceGroup);
                }
                LogProvider.WriteLog("GenerateStreams", "End processing selected Cloud Services / Virtual Machines");

                // Serialize

                TemplateStreams.Clear();

                await SerializeDeploymentInstructions();

                if (HasBlobCopyDetails)
                {
                    await SerializeBlobCopyDetails(); // Serialize blob copy details
                }

                await SerializeExportTemplate();
                await SerializeParameterTemplate();
                await SerializeMigAzPowerShell();
            }
            else
                LogProvider.WriteLog("GenerateStreams", "ExportArtifacts is null, nothing to export.");

            StatusProvider.UpdateStatus("Ready");
            LogProvider.WriteLog("GenerateStreams", "End - Execution " + this.ExecutionGuid.ToString());
        }

        private async Task<ApplicationSecurityGroup> BuildApplicationSecurityGroup(MigrationTarget.ApplicationSecurityGroup targetApplicationSecurityGroup)
        {
            LogProvider.WriteLog("BuildApplicationSecurityGroup", "Start");

            ApplicationSecurityGroup applicationSecurityGroup = new ApplicationSecurityGroup(this.ExecutionGuid)
            {
                name = targetApplicationSecurityGroup.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetApplicationSecurityGroup.ApiVersion,
            };

            this.AddResource(applicationSecurityGroup);

            LogProvider.WriteLog("BuildApplicationSecurityGroup", "End");

            return applicationSecurityGroup;
        }

        public void Write()
        {
            if (!Directory.Exists(_OutputDirectory))
            {
                Directory.CreateDirectory(_OutputDirectory);
            }

            foreach (string key in TemplateStreams.Keys)
            {
                MemoryStream ms = TemplateStreams[key];
                using (FileStream file = new FileStream(_OutputDirectory + key, FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] bytes = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public string BuildMigAzMessages()
        {
            if (_ExportArtifacts.Alerts.Count == 0)
                return String.Empty;

            StringBuilder sbMigAzMessageResult = new StringBuilder();

            sbMigAzMessageResult.Append("<p>MigAz has identified the following advisements during template generation for review:</p>");

            sbMigAzMessageResult.Append("<p>");
            sbMigAzMessageResult.Append("<ul>");
            foreach (MigAzGeneratorAlert migAzMessage in _ExportArtifacts.Alerts)
            {
                sbMigAzMessageResult.Append("<li>");
                sbMigAzMessageResult.Append(migAzMessage.AlertType + " - " + migAzMessage.Message);
                sbMigAzMessageResult.Append("</li>");
            }
            sbMigAzMessageResult.Append("</ul>");
            sbMigAzMessageResult.Append("</p>");

            return sbMigAzMessageResult.ToString();
        }

        private AvailabilitySet BuildAvailabilitySetObject(Azure.MigrationTarget.AvailabilitySet targetAvailabilitySet)
        {
            LogProvider.WriteLog("BuildAvailabilitySetObject", "Start");

            AvailabilitySet_Properties availabilitySet_Properties = new AvailabilitySet_Properties
            {
                platformFaultDomainCount = targetAvailabilitySet.PlatformFaultDomainCount,
                platformUpdateDomainCount = targetAvailabilitySet.PlatformUpdateDomainCount
            };

            AvailabilitySet availabilitySet = new AvailabilitySet(this.ExecutionGuid)
            {
                name = targetAvailabilitySet.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetAvailabilitySet.ApiVersion,
                properties = availabilitySet_Properties
            };

            if (targetAvailabilitySet.IsManagedDisks)
            {
                // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/using-managed-disks-template-deployments
                // see "Create managed availability sets with VMs using managed disks"

                Dictionary<string, string> availabilitySetSku = new Dictionary<string, string>();
                availabilitySet.sku = availabilitySetSku;
                availabilitySetSku.Add("name", "Aligned");
            }

            this.AddResource(availabilitySet);

            LogProvider.WriteLog("BuildAvailabilitySetObject", "End");

            return availabilitySet;
        }

        private async Task BuildPublicIPAddressObject(MigrationTarget.PublicIp publicIp)
        {
            LogProvider.WriteLog("BuildPublicIPAddressObject", "Start " + ArmConst.ProviderLoadBalancers + publicIp.ToString());

            PublicIPAddress_Properties publicipaddress_properties = new PublicIPAddress_Properties
            {
                publicIPAllocationMethod = publicIp.IPAllocationMethod.ToString()
            };

            if (publicIp.DomainNameLabel != String.Empty)
            {
                Hashtable dnssettings = new Hashtable();
                dnssettings.Add("domainNameLabel", publicIp.DomainNameLabel);
                publicipaddress_properties.dnsSettings = dnssettings;
            }

            PublicIPAddress publicipaddress = new PublicIPAddress(this.ExecutionGuid)
            {
                name = publicIp.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = publicIp.ApiVersion,
                properties = publicipaddress_properties
            };

            this.AddResource(publicipaddress);

            LogProvider.WriteLog("BuildPublicIPAddressObject", "End " + ArmConst.ProviderLoadBalancers + publicIp.ToString());
        }

        private async Task BuildLoadBalancerObject(Azure.MigrationTarget.LoadBalancer loadBalancer)
        {
            LogProvider.WriteLog("BuildLoadBalancerObject", "Start " + ArmConst.ProviderLoadBalancers + loadBalancer.ToString());

            List<string> dependson = new List<string>();

            LoadBalancer_Properties loadbalancer_properties = new LoadBalancer_Properties();

            List<FrontendIPConfiguration> frontendipconfigurations = new List<FrontendIPConfiguration>();
            List<Hashtable> backendaddresspools = new List<Hashtable>();
            List<InboundNatRule> inboundnatrules = new List<InboundNatRule>();
            List<LoadBalancingRule> loadbalancingrules = new List<LoadBalancingRule>();
            List<Probe> probes = new List<Probe>();

            loadbalancer_properties.frontendIPConfigurations = frontendipconfigurations;
            loadbalancer_properties.backendAddressPools = backendaddresspools;
            loadbalancer_properties.inboundNatRules = inboundnatrules;
            loadbalancer_properties.loadBalancingRules = loadbalancingrules;
            loadbalancer_properties.probes = probes;

            if (loadBalancer.FrontEndIpConfigurations != null)
            {
                foreach (Azure.MigrationTarget.FrontEndIpConfiguration targetFrontEndIpConfiguration in loadBalancer.FrontEndIpConfigurations)
                {
                    FrontendIPConfiguration frontendipconfiguration = new FrontendIPConfiguration();
                    frontendipconfiguration.name = targetFrontEndIpConfiguration.Name;
                    frontendipconfigurations.Add(frontendipconfiguration);

                    FrontendIPConfiguration_Properties frontendipconfiguration_properties = new FrontendIPConfiguration_Properties();
                    frontendipconfiguration.properties = frontendipconfiguration_properties;

                    if (loadBalancer.LoadBalancerType == MigrationTarget.LoadBalancerType.Internal)
                    {
                        frontendipconfiguration_properties.privateIPAllocationMethod = targetFrontEndIpConfiguration.TargetPrivateIPAllocationMethod.ToString();
                        frontendipconfiguration_properties.privateIPAddress = targetFrontEndIpConfiguration.TargetPrivateIpAddress;

                        if (targetFrontEndIpConfiguration.TargetVirtualNetwork != null && targetFrontEndIpConfiguration.TargetVirtualNetwork.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork))
                            dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderVirtualNetwork + targetFrontEndIpConfiguration.TargetVirtualNetwork.ToString() + "')]");

                        Reference subnet_ref = new Reference();
                        frontendipconfiguration_properties.subnet = subnet_ref;

                        if (targetFrontEndIpConfiguration.TargetSubnet != null)
                        {
                            subnet_ref.id = targetFrontEndIpConfiguration.TargetSubnet.TargetId;
                        }
                    }
                    else
                    {
                        if (targetFrontEndIpConfiguration.PublicIp != null)
                        {
                            if (targetFrontEndIpConfiguration.PublicIp.GetType() == typeof(MigrationTarget.PublicIp))
                            {
                                await BuildPublicIPAddressObject((MigrationTarget.PublicIp)targetFrontEndIpConfiguration.PublicIp);

                                Reference publicipaddress_ref = new Reference();
                                publicipaddress_ref.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderPublicIpAddress + targetFrontEndIpConfiguration.PublicIp.ToString() + "')]";
                                frontendipconfiguration_properties.publicIPAddress = publicipaddress_ref;

                                dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderPublicIpAddress + targetFrontEndIpConfiguration.PublicIp.ToString() + "')]");
                            }
                            else
                            {
                                int todo = 0;
                            }

                        }
                    }
                }
            }

            if (loadBalancer.BackEndAddressPools != null)
            {
                foreach (Azure.MigrationTarget.BackEndAddressPool targetBackEndAddressPool in loadBalancer.BackEndAddressPools)
                {
                    Hashtable backendaddresspool = new Hashtable();
                    backendaddresspool.Add("name", targetBackEndAddressPool.Name);
                    backendaddresspools.Add(backendaddresspool);
                }
            }

            // Add Inbound Nat Rules
            if (loadBalancer.InboundNatRules != null)
            {
                foreach (Azure.MigrationTarget.InboundNatRule inboundNatRule in loadBalancer.InboundNatRules)
                {
                    InboundNatRule_Properties inboundnatrule_properties = new InboundNatRule_Properties
                    {
                        frontendPort = inboundNatRule.FrontEndPort,
                        backendPort = inboundNatRule.BackEndPort,
                        protocol = inboundNatRule.Protocol
                    };

                    if (inboundNatRule.FrontEndIpConfiguration != null)
                    {
                        Reference frontendIPConfiguration = new Reference();
                        frontendIPConfiguration.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLoadBalancers + loadBalancer.ToString() + "/frontendIPConfigurations/default')]";
                        inboundnatrule_properties.frontendIPConfiguration = frontendIPConfiguration;
                    }

                    InboundNatRule inboundnatrule = new InboundNatRule
                    {
                        name = inboundNatRule.Name,
                        properties = inboundnatrule_properties
                    };

                    loadbalancer_properties.inboundNatRules.Add(inboundnatrule);
                }
            }

            if (loadBalancer.Probes != null)
            {
                foreach (Azure.MigrationTarget.Probe targetProbe in loadBalancer.Probes)
                {
                    Probe_Properties probe_properties = new Probe_Properties
                    {
                        port = targetProbe.Port,
                        protocol = targetProbe.Protocol,
                        intervalInSeconds = targetProbe.IntervalInSeconds,
                        numberOfProbes = targetProbe.NumberOfProbes
                    };

                    if (targetProbe.RequestPath != null && targetProbe.RequestPath != String.Empty)
                        probe_properties.requestPath = targetProbe.RequestPath;

                    Probe probe = new Probe
                    {
                        name = targetProbe.Name,
                        properties = probe_properties
                    };

                    loadbalancer_properties.probes.Add(probe);
                }
            }

            if (loadBalancer.LoadBalancingRules != null)
            {
                foreach (Azure.MigrationTarget.LoadBalancingRule targetLoadBalancingRule in loadBalancer.LoadBalancingRules)
                {
                    Reference frontendipconfiguration_ref = new Reference
                    {
                        id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLoadBalancers + loadBalancer.ToString() + "/frontendIPConfigurations/" + targetLoadBalancingRule.FrontEndIpConfiguration.Name + "')]"
                    };

                    Reference backendaddresspool_ref = new Reference
                    {
                        id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLoadBalancers + loadBalancer.ToString() + "/backendAddressPools/" + targetLoadBalancingRule.BackEndAddressPool.Name + "')]"
                    };

                    Reference probe_ref = new Reference
                    {
                        id = "[concat(" + ArmConst.ResourceGroupId + ",'" + ArmConst.ProviderLoadBalancers + loadBalancer.ToString() + "/probes/" + targetLoadBalancingRule.Probe.Name + "')]"
                    };

                    LoadBalancingRule_Properties loadbalancingrule_properties = new LoadBalancingRule_Properties
                    {
                        frontendIPConfiguration = frontendipconfiguration_ref,
                        backendAddressPool = backendaddresspool_ref,
                        probe = probe_ref,
                        frontendPort = targetLoadBalancingRule.FrontEndPort,
                        backendPort = targetLoadBalancingRule.BackEndPort,
                        protocol = targetLoadBalancingRule.Protocol,
                        enableFloatingIP = targetLoadBalancingRule.EnableFloatingIP,
                        idleTimeoutInMinutes = targetLoadBalancingRule.IdleTimeoutInMinutes
                    };

                    LoadBalancingRule loadbalancingrule = new LoadBalancingRule
                    {
                        name = targetLoadBalancingRule.Name,
                        properties = loadbalancingrule_properties
                    };

                    loadbalancer_properties.loadBalancingRules.Add(loadbalancingrule);
                }
            }

            LoadBalancer loadbalancer = new LoadBalancer(this.ExecutionGuid)
            {
                name = loadBalancer.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = loadBalancer.ApiVersion,
                dependsOn = dependson,
                properties = loadbalancer_properties
            };

            this.AddResource(loadbalancer);

            LogProvider.WriteLog("BuildLoadBalancerObject", "End " + ArmConst.ProviderLoadBalancers + loadBalancer.ToString());
        }

        private async Task BuildVirtualNetworkObject(Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork)
        {
            LogProvider.WriteLog("BuildVirtualNetworkObject", "Start Microsoft.Network/virtualNetworks/" + targetVirtualNetwork.ToString());

            List<string> dependson = new List<string>();

            AddressSpace addressspace = new AddressSpace
            {
                addressPrefixes = targetVirtualNetwork.AddressPrefixes
            };

            VirtualNetwork_dhcpOptions dhcpoptions = new VirtualNetwork_dhcpOptions
            {
                dnsServers = targetVirtualNetwork.DnsServers
            };

            List<Subnet> subnets = new List<Subnet>();
            foreach (Azure.MigrationTarget.Subnet targetSubnet in targetVirtualNetwork.TargetSubnets)
            {
                Subnet_Properties properties = new Subnet_Properties
                {
                    addressPrefix = targetSubnet.AddressPrefix
                };

                Subnet subnet = new Subnet
                {
                    name = targetSubnet.TargetName,
                    properties = properties
                };

                subnets.Add(subnet);

                // add Network Security Group if exists
                if (targetSubnet.NetworkSecurityGroup != null)
                {
                    // Add NSG reference to the subnet
                    Reference networksecuritygroup_ref = new Reference();
                    networksecuritygroup_ref.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderNetworkSecurityGroups + "todonowrussell" + "')]";

                    properties.networkSecurityGroup = networksecuritygroup_ref;

                    // Add NSG dependsOn to the Virtual Network object
                    if (!dependson.Contains(networksecuritygroup_ref.id))
                    {
                        dependson.Add(networksecuritygroup_ref.id);
                    }
                }

                // add Route Table if exists
                if (targetSubnet.RouteTable != null)
                {
                    // Add Route Table reference to the subnet
                    Reference routetable_ref = new Reference();
                    routetable_ref.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderRouteTables + targetSubnet.RouteTable.ToString() + "')]";

                    properties.routeTable = routetable_ref;

                    // Add Route Table dependsOn to the Virtual Network object
                    if (!dependson.Contains(routetable_ref.id))
                    {
                        dependson.Add(routetable_ref.id);
                    }
                }
            }

            VirtualNetwork_Properties virtualnetwork_properties = new VirtualNetwork_Properties
            {
                addressSpace = addressspace,
                subnets = subnets,
                dhcpOptions = dhcpoptions
            };

            VirtualNetwork virtualnetwork = new VirtualNetwork(this.ExecutionGuid)
            {
                name = targetVirtualNetwork.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetVirtualNetwork.ApiVersion,
                dependsOn = dependson,
                properties = virtualnetwork_properties
            };

            this.AddResource(virtualnetwork);

            LogProvider.WriteLog("BuildVirtualNetworkObject", "End Microsoft.Network/virtualNetworks/" + targetVirtualNetwork.ToString());
        }

        private async Task BuildLocalNetworkGateway(Azure.MigrationTarget.LocalNetworkGateway localNetworkGateway)
        {
            LogProvider.WriteLog("BuildLocalNetworkGateway", "Start " + localNetworkGateway.ProviderNamespace + localNetworkGateway.ToString());

            AddressSpace localnetworkaddressspace = new AddressSpace
            {
                addressPrefixes = localNetworkGateway.AddressPrefixes
            };

            LocalNetworkGateway_Properties localnetworkgateway_properties = new LocalNetworkGateway_Properties
            {
                localNetworkAddressSpace = localnetworkaddressspace,
                gatewayIpAddress = localNetworkGateway.GatewayIpAddress
            };

            LocalNetworkGateway localnetworkgateway = new LocalNetworkGateway(this.ExecutionGuid)
            {
                name = localNetworkGateway.ToString(),
                apiVersion = localNetworkGateway.ApiVersion,
                location = "[resourceGroup().location]",
                properties = localnetworkgateway_properties
            };

            this.AddResource(localnetworkgateway);

            LogProvider.WriteLog("BuildLocalNetworkGateway", "End " + localNetworkGateway.ProviderNamespace + localNetworkGateway.ToString());
        }

        private async Task BuildVirtualNetworkGateway(Azure.MigrationTarget.VirtualNetworkGateway virtualNetworkGateway)
        {
            LogProvider.WriteLog("BuildVirtualNetworkGateway", "Start " + virtualNetworkGateway.ProviderNamespace + virtualNetworkGateway.ToString());

            VirtualNetworkGateway_Sku virtualnetworkgateway_sku = new VirtualNetworkGateway_Sku
            {
                name = virtualNetworkGateway.SkuName.ToString(),
                tier = virtualNetworkGateway.SkuTier.ToString(),
                capacity = virtualNetworkGateway.SkuCapacity
            };

            Reference subnet_ref = new Reference
            {
                // todonow id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderVirtualNetwork + templateVirtualNetwork.name + "/subnets/" + ArmConst.GatewaySubnetName + "')]"
            };

            Reference publicipaddress_ref = new Reference
            {
                // todonow id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderPublicIpAddress + publicipaddress.name + "')]"
            };

            IpConfiguration_Properties ipconfiguration_properties = new IpConfiguration_Properties
            {
                privateIPAllocationMethod = "Dynamic",
                subnet = subnet_ref,
                publicIPAddress = publicipaddress_ref
            };

            IpConfiguration virtualnetworkgateway_ipconfiguration = new IpConfiguration
            {
                name = "GatewayIPConfig",
                properties = ipconfiguration_properties
            };

            List<IpConfiguration> virtualnetworkgateway_ipconfigurations = new List<IpConfiguration>();
            virtualnetworkgateway_ipconfigurations.Add(virtualnetworkgateway_ipconfiguration);

            VirtualNetworkGateway_Properties virtualnetworkgateway_properties = new VirtualNetworkGateway_Properties
            {
                ipConfigurations = virtualnetworkgateway_ipconfigurations,
                sku = virtualnetworkgateway_sku,
                gatewayType = virtualNetworkGateway.GatewayType.ToString(),
                enableBgp = virtualNetworkGateway.EnableBgp.ToString(),
                activeActive = virtualNetworkGateway.ActiveActive.ToString(),
                vpnType = virtualNetworkGateway.VpnType.ToString()
            };

            VirtualNetworkGateway virtualnetworkgateway = new VirtualNetworkGateway(this.ExecutionGuid)
            {
                location = "[resourceGroup().location]",
                name = virtualNetworkGateway.ToString(),
                properties = virtualnetworkgateway_properties,
                apiVersion = virtualNetworkGateway.ApiVersion
            };

            var dependson = new List<string>();
            //dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderVirtualNetwork + templateVirtualNetwork.name + "')]");
            //dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderPublicIpAddress + publicipaddress.name + "')]");

            virtualnetworkgateway.dependsOn = dependson;

            this.AddResource(virtualnetworkgateway);

            LogProvider.WriteLog("BuildVirtualNetworkGateway", "End " + virtualNetworkGateway.ProviderNamespace + virtualNetworkGateway.ToString());
        }

        private async Task BuildVirtualNetworkGatewayConnection(Azure.MigrationTarget.VirtualNetworkGatewayConnection virtualNetworkGatewayConnection)
        {
            LogProvider.WriteLog("BuildVirtualNetworkGatewayConnection", "Start " + virtualNetworkGatewayConnection.ProviderNamespace + virtualNetworkGatewayConnection.ToString());

            // TODO: Commenting out until next build, we need to test / rework this export officially

            //Reference virtualNetworkGatewayReference = new Reference
            //{
            //    id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderVirtualNetworkGateways + virtualNetworkGatewayConnection.VirtualNetworkGateway.ToString() + "')]"
            //};

            //Reference localNetworkGatewayReference = new Reference
            //{
            //    id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLocalNetworkGateways + virtualNetworkGatewayConnection.LocalNetworkGateway.ToString() + "')]"
            //};

            //GatewayConnection_Properties gatewayConnection_Properties = new GatewayConnection_Properties
            //{
            //    connectionType = virtualNetworkGatewayConnection.ConnectionType.ToString(),
            //    routingWeight = virtualNetworkGatewayConnection.RoutingWeight,
            //    virtualNetworkGateway1 = virtualNetworkGatewayReference,
            //    localNetworkGateway2 = localNetworkGatewayReference,
            //    sharedKey = "[parameters('" + virtualNetworkGatewayConnection.ToString() + "_SharedKey" + "')]"
            //};

            //Parameter parameter = new Parameter
            //{
            //    type = "string",
            //    value = virtualNetworkGatewayConnection.SharedKey
            //};
            //this.Parameters.Add(virtualNetworkGatewayConnection.ToString() + "_SharedKey", parameter);

            //GatewayConnection gatewayConnection = new GatewayConnection(this.ExecutionGuid)
            //{
            //    name = virtualNetworkGatewayConnection.ToString(),
            //    apiVersion = virtualNetworkGatewayConnection.ApiVersion,
            //    location = "[resourceGroup().location]",
            //    properties = gatewayConnection_Properties
            //};

            //this.AddResource(gatewayConnection);

            LogProvider.WriteLog("BuildVirtualNetworkGatewayConnection", "End " + virtualNetworkGatewayConnection.ProviderNamespace + virtualNetworkGatewayConnection.ToString());
        }

        private async Task<NetworkSecurityGroup> BuildNetworkSecurityGroup(MigrationTarget.NetworkSecurityGroup targetNetworkSecurityGroup)
        {
            LogProvider.WriteLog("BuildNetworkSecurityGroup", "Start");

            NetworkSecurityGroup_Properties networksecuritygroup_properties = new NetworkSecurityGroup_Properties
            {
                securityRules = new List<SecurityRule>()
            };

            // for each rule
            foreach (MigrationTarget.NetworkSecurityGroupRule targetNetworkSecurityGroupRule in targetNetworkSecurityGroup.Rules.OrderBy(a => a.Direction).ThenBy(a => a.Priority))
            {
                // if not system rule
                if (!targetNetworkSecurityGroupRule.IsSystemRule)
                {
                    List<Reference> sourceApplicationSecurityGroups = null;
                    List<Reference> destinationApplicationSecurityGroups = null;

                    if (targetNetworkSecurityGroupRule.SourceApplicationSecurityGroups != null)
                    {
                        sourceApplicationSecurityGroups = new List<Reference>();
                        foreach (MigrationTarget.ApplicationSecurityGroup applicationSecurityGroup in targetNetworkSecurityGroupRule.SourceApplicationSecurityGroups)
                        {
                            sourceApplicationSecurityGroups.Add(
                                new Reference()
                                {
                                    id = "" //applicationSecurityGroup.Id
                                }
                            );
                        }
                    }

                    if (targetNetworkSecurityGroupRule.DestinationApplicationSecurityGroups != null)
                    {
                        destinationApplicationSecurityGroups = new List<Reference>();
                        foreach (MigrationTarget.ApplicationSecurityGroup applicationSecurityGroup in targetNetworkSecurityGroupRule.DestinationApplicationSecurityGroups)
                        {
                            destinationApplicationSecurityGroups.Add(
                                new Reference()
                                {
                                    id = "" // applicationSecurityGroup.Id
                                }
                            ); 
                        }
                    }

                    SecurityRule_Properties securityrule_properties = new SecurityRule_Properties
                    {
                        description = targetNetworkSecurityGroupRule.ToString(),
                        direction = targetNetworkSecurityGroupRule.Direction,
                        priority = targetNetworkSecurityGroupRule.Priority,
                        access = targetNetworkSecurityGroupRule.Access,
                        sourceAddressPrefix = targetNetworkSecurityGroupRule.SourceAddressPrefix,
                        sourceAddressPrefixes = targetNetworkSecurityGroupRule.SourceAddressPrefixes,
                        sourcePortRange = targetNetworkSecurityGroupRule.SourcePortRange,
                        sourcePortRanges = targetNetworkSecurityGroupRule.SourcePortRanges,
                        sourceApplicationSecurityGroups = sourceApplicationSecurityGroups,
                        destinationAddressPrefix = targetNetworkSecurityGroupRule.DestinationAddressPrefix,
                        destinationAddressPrefixes = targetNetworkSecurityGroupRule.DestinationAddressPrefixes,
                        destinationApplicationSecurityGroups = destinationApplicationSecurityGroups,
                        destinationPortRange = targetNetworkSecurityGroupRule.DestinationPortRange,
                        protocol = targetNetworkSecurityGroupRule.Protocol
                    };

                    SecurityRule securityrule = new SecurityRule
                    {
                        name = targetNetworkSecurityGroupRule.ToString(),
                        properties = securityrule_properties
                    };

                    networksecuritygroup_properties.securityRules.Add(securityrule);
                }
            }

            NetworkSecurityGroup networksecuritygroup = new NetworkSecurityGroup(this.ExecutionGuid)
            {
                name = targetNetworkSecurityGroup.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetNetworkSecurityGroup.ApiVersion,
                properties = networksecuritygroup_properties
            };

            this.AddResource(networksecuritygroup);

            LogProvider.WriteLog("BuildNetworkSecurityGroup", "End");

            return networksecuritygroup;
        }

        private async Task<RouteTable> BuildRouteTable(MigrationTarget.RouteTable routeTable)
        {
            LogProvider.WriteLog("BuildRouteTable", "Start");

            RouteTable_Properties routetable_properties = new RouteTable_Properties
            {
                routes = new List<Route>()
            };

            // for each route
            foreach (MigrationTarget.Route migrationRoute in routeTable.Routes)
            {
                Route_Properties route_properties = new Route_Properties
                {
                    addressPrefix = migrationRoute.AddressPrefix,
                    nextHopType = migrationRoute.NextHopType.ToString()
                };

                if (route_properties.nextHopType == "VirtualAppliance")
                    route_properties.nextHopIpAddress = migrationRoute.NextHopIpAddress;

                Route route = new Route
                {
                    name = migrationRoute.ToString(),
                    properties = route_properties
                };

                routetable_properties.routes.Add(route);
            }

            RouteTable routetable = new RouteTable(this.ExecutionGuid)
            {
                name = routeTable.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = routeTable.ApiVersion,
                properties = routetable_properties
            };

            this.AddResource(routetable);

            LogProvider.WriteLog("BuildRouteTable", "End");

            return routetable;
        }

       
        private NetworkInterface BuildNetworkInterfaceObject(Azure.MigrationTarget.NetworkInterface targetNetworkInterface)
        {
            LogProvider.WriteLog("BuildNetworkInterfaceObject", "Start " + ArmConst.ProviderNetworkInterfaces + targetNetworkInterface.ToString());

            List<string> dependson = new List<string>();

            List<IpConfiguration> ipConfigurations = new List<IpConfiguration>();
            foreach (Azure.MigrationTarget.NetworkInterfaceIpConfiguration ipConfiguration in targetNetworkInterface.TargetNetworkInterfaceIpConfigurations)
            {
                IpConfiguration ipconfiguration = new IpConfiguration
                {
                    name = ipConfiguration.ToString()
                };
                IpConfiguration_Properties ipconfiguration_properties = new IpConfiguration_Properties();
                ipconfiguration.properties = ipconfiguration_properties;
                Reference subnet_ref = new Reference();
                ipconfiguration_properties.subnet = subnet_ref;

                if (ipConfiguration.TargetSubnet != null)
                {
                    subnet_ref.id = ipConfiguration.TargetSubnet.TargetId;
                }

                ipconfiguration_properties.privateIPAllocationMethod = ipConfiguration.TargetPrivateIPAllocationMethod.ToString();
                if (ipConfiguration.TargetPrivateIPAllocationMethod == MigrationTarget.IPAllocationMethodEnum.Static)
                    ipconfiguration_properties.privateIPAddress = ipConfiguration.TargetPrivateIpAddress;

                if (ipConfiguration.TargetVirtualNetwork != null)
                {
                    if (ipConfiguration.TargetVirtualNetwork.GetType() == typeof(MigrationTarget.VirtualNetwork))
                    {
                        // only adding VNet DependsOn here because as it is a resource in the target migration (resource group)
                        MigrationTarget.VirtualNetwork targetVirtualNetwork = (MigrationTarget.VirtualNetwork)ipConfiguration.TargetVirtualNetwork;
                        dependson.Add(targetVirtualNetwork.TargetId);
                    }
                }

                if (targetNetworkInterface.BackEndAddressPool != null)
                {
                    if (_ExportArtifacts.ContainsLoadBalancer(targetNetworkInterface.BackEndAddressPool.LoadBalancer))
                    {
                        // If there is at least one endpoint add the reference to the LB backend pool
                        List<Reference> loadBalancerBackendAddressPools = new List<Reference>();
                        ipconfiguration_properties.loadBalancerBackendAddressPools = loadBalancerBackendAddressPools;

                        Reference loadBalancerBackendAddressPool = new Reference();
                        loadBalancerBackendAddressPool.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLoadBalancers + targetNetworkInterface.BackEndAddressPool.LoadBalancer.TargetName + "/backendAddressPools/" + targetNetworkInterface.BackEndAddressPool.Name + "')]";

                        loadBalancerBackendAddressPools.Add(loadBalancerBackendAddressPool);

                        dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLoadBalancers + targetNetworkInterface.BackEndAddressPool.LoadBalancer.TargetName + "')]");
                    }
                }

                // Adds the references to the inboud nat rules
                List<Reference> loadBalancerInboundNatRules = new List<Reference>();
                foreach (MigrationTarget.InboundNatRule inboundNatRule in targetNetworkInterface.InboundNatRules)
                {
                    if (_ExportArtifacts.ContainsLoadBalancer(inboundNatRule.LoadBalancer))
                    {
                        Reference loadBalancerInboundNatRule = new Reference();
                        loadBalancerInboundNatRule.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderLoadBalancers + inboundNatRule.LoadBalancer.TargetName + "/inboundNatRules/" + inboundNatRule.Name + "')]";

                        loadBalancerInboundNatRules.Add(loadBalancerInboundNatRule);
                    }
                }

                if (loadBalancerInboundNatRules.Count > 0)
                {
                    ipconfiguration_properties.loadBalancerInboundNatRules = loadBalancerInboundNatRules;
                }

                if (ipConfiguration.TargetPublicIp != null)
                {
                    Core.ArmTemplate.Reference publicIPAddressReference = new Core.ArmTemplate.Reference();
                    publicIPAddressReference.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderPublicIpAddress + ipConfiguration.TargetPublicIp.ToString() + "')]";
                    ipconfiguration_properties.publicIPAddress = publicIPAddressReference;

                    dependson.Add(publicIPAddressReference.id);
                }

                ipConfigurations.Add(ipconfiguration);
            }

            NetworkInterface_Properties networkinterface_properties = new NetworkInterface_Properties();
            networkinterface_properties.ipConfigurations = ipConfigurations;
            networkinterface_properties.enableIPForwarding = targetNetworkInterface.EnableIPForwarding;

            if (targetNetworkInterface.AllowAcceleratedNetworking)
                networkinterface_properties.enableAcceleratedNetworking = targetNetworkInterface.EnableAcceleratedNetworking;

            if (targetNetworkInterface.NetworkSecurityGroup != null)
            {
                // Add NSG reference to the network interface
                Reference networksecuritygroup_ref = new Reference();
                networksecuritygroup_ref.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderNetworkSecurityGroups + targetNetworkInterface.NetworkSecurityGroup.ToString() + "')]";

                networkinterface_properties.NetworkSecurityGroup = networksecuritygroup_ref;

                dependson.Add(networksecuritygroup_ref.id);
            }

            NetworkInterface networkInterface = new NetworkInterface(this.ExecutionGuid)
            {
                name = targetNetworkInterface.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetNetworkInterface.ApiVersion,
                properties = networkinterface_properties,
                dependsOn = dependson
            };

            this.AddResource(networkInterface);

            LogProvider.WriteLog("BuildNetworkInterfaceObject", "End " + ArmConst.ProviderNetworkInterfaces + targetNetworkInterface.ToString());

            return networkInterface;
        }

        private async Task BuildVirtualMachineObject(Azure.MigrationTarget.VirtualMachine targetVirtualMachine, Azure.MigrationTarget.ResourceGroup targetResourceGroup)
        {
            LogProvider.WriteLog("BuildVirtualMachineObject", "Start Microsoft.Compute/virtualMachines/" + targetVirtualMachine.ToString());

            List<string> dependson = new List<string>();

            // process network interface
            List<NetworkProfile_NetworkInterface> networkinterfaces = new List<NetworkProfile_NetworkInterface>();

            foreach (MigrationTarget.NetworkInterface targetNetworkInterface in targetVirtualMachine.NetworkInterfaces)
            {
                NetworkProfile_NetworkInterface_Properties networkinterface_ref_properties = new NetworkProfile_NetworkInterface_Properties();
                networkinterface_ref_properties.primary = targetNetworkInterface.IsPrimary;

                NetworkProfile_NetworkInterface networkinterface_ref = new NetworkProfile_NetworkInterface
                {
                    id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderNetworkInterfaces + targetNetworkInterface.ToString() + "')]",
                    properties = networkinterface_ref_properties
                };

                networkinterfaces.Add(networkinterface_ref);

                dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderNetworkInterfaces + targetNetworkInterface.ToString() + "')]");
            }

            HardwareProfile hardwareprofile = new HardwareProfile
            {
                vmSize = targetVirtualMachine.TargetSize.ToString()
            };

            NetworkProfile networkprofile = new NetworkProfile
            {
                networkInterfaces = networkinterfaces
            };


            OsDisk osdisk = new OsDisk
            {
                caching = targetVirtualMachine.OSVirtualHardDisk.HostCaching
            };
            if (targetVirtualMachine.OSVirtualHardDisk.IsEncrypted)
            {
                DiskEncrpytionSettings osDiskEncryptionSettings = new DiskEncrpytionSettings();
                osdisk.encryptionSettings = osDiskEncryptionSettings;

                osDiskEncryptionSettings.enabled = targetVirtualMachine.OSVirtualHardDisk.IsEncrypted;

                DiskEncryptionKeySettings diskEncryptionKeySettings = new DiskEncryptionKeySettings();
                osDiskEncryptionSettings.diskEncryptionKey = diskEncryptionKeySettings;

                diskEncryptionKeySettings.secretUrl = targetVirtualMachine.OSVirtualHardDisk.DiskEncryptionKeySecretUrl;

                diskEncryptionKeySettings.sourceVault = new Reference();
                diskEncryptionKeySettings.sourceVault.id = targetVirtualMachine.OSVirtualHardDisk.DiskEncryptionKeySourceVaultId;

                KeyEncryptionKeySettings keyEncryptionKeySettings = new KeyEncryptionKeySettings();
                osDiskEncryptionSettings.keyEncryptionKey = keyEncryptionKeySettings;

                keyEncryptionKeySettings.keyUrl = targetVirtualMachine.OSVirtualHardDisk.KeyEncryptionKeyKeyUrl;

                keyEncryptionKeySettings.sourceVault = new Reference();
                keyEncryptionKeySettings.sourceVault.id = targetVirtualMachine.OSVirtualHardDisk.KeyEncryptionKeySourceVaultId;
            }

            ImageReference imagereference = new ImageReference();
            OsProfile osprofile = new OsProfile();

            // if the tool is configured to create new VMs with empty data disks
            if (this.BuildEmpty)
            {
                osdisk.name = targetVirtualMachine.OSVirtualHardDisk.ToString();
                osdisk.createOption = "FromImage";

                osprofile.computerName = targetVirtualMachine.ToString();
                osprofile.adminUsername = "[parameters('adminUsername')]";
                osprofile.adminPassword = "[parameters('adminPassword')]";

                if (!this.Parameters.ContainsKey("adminUsername"))
                {
                    Parameter parameter = new Parameter();
                    parameter.type = "string";
                    this.Parameters.Add("adminUsername", parameter);
                }

                if (!this.Parameters.ContainsKey("adminPassword"))
                {
                    Parameter parameter = new Parameter();
                    parameter.type = "securestring";
                    this.Parameters.Add("adminPassword", parameter);
                }

                if (targetVirtualMachine.OSVirtualHardDiskOS == "Windows")
                {
                    imagereference.publisher = "MicrosoftWindowsServer";
                    imagereference.offer = "WindowsServer";
                    imagereference.sku = "2016-Datacenter";
                    imagereference.version = "latest";
                }
                else if (targetVirtualMachine.OSVirtualHardDiskOS == "Linux")
                {
                    imagereference.publisher = "Canonical";
                    imagereference.offer = "UbuntuServer";
                    imagereference.sku = "16.04.0-LTS";
                    imagereference.version = "latest";
                }
                else
                {
                    imagereference.publisher = "<publisher>";
                    imagereference.offer = "<offer>";
                    imagereference.sku = "<sku>";
                    imagereference.version = "<version>";
                }
            }
            // if the tool is configured to attach copied disks
            else
            {
                osdisk.createOption = "Attach";
                osdisk.osType = targetVirtualMachine.OSVirtualHardDiskOS;

                if (targetVirtualMachine.OSVirtualHardDisk.IsUnmanagedDisk)
                {
                    osdisk.name = targetVirtualMachine.OSVirtualHardDisk.ToString();

                    Vhd vhd = new Vhd();
                    osdisk.vhd = vhd;
                    vhd.uri = targetVirtualMachine.OSVirtualHardDisk.TargetMediaLink;
                    
                    if (targetVirtualMachine.OSVirtualHardDisk.TargetStorage != null && (targetVirtualMachine.OSVirtualHardDisk.TargetStorage.GetType() == typeof(Arm.StorageAccount) || targetVirtualMachine.OSVirtualHardDisk.TargetStorage.GetType() == typeof(MigrationTarget.StorageAccount)))
                    {
                        // BuildBlobCopy is only called here for migration to Existing ARM Storage Accounts, as call to BuildBlobCopy for ManagedDisks is already called via the "foreach (ManagedDisk in ManagedDisks)" in GenerateStreams to ensure all ManagedDisks are exported
                        this._CopyBlobDetails.Add(await BuildCopyBlob(targetVirtualMachine.OSVirtualHardDisk, targetResourceGroup));
                    }
                }
                else if (targetVirtualMachine.OSVirtualHardDisk.IsManagedDisk)
                {
                    Reference managedDiskReference = new Reference
                    {
                        id = targetVirtualMachine.OSVirtualHardDisk.ReferenceId
                    };

                    osdisk.managedDisk = managedDiskReference;

                    dependson.Add(targetVirtualMachine.OSVirtualHardDisk.ReferenceId);
                }
            }

            // process data disks
            List<DataDisk> datadisks = new List<DataDisk>();
            foreach (MigrationTarget.Disk dataDisk in targetVirtualMachine.DataDisks)
            {
                if (dataDisk.TargetStorage != null)
                {
                    DataDisk datadisk = new DataDisk
                    {
                        name = dataDisk.ToString(),
                        caching = dataDisk.HostCaching,
                        diskSizeGB = dataDisk.DiskSizeInGB
                    };
                    if (dataDisk.Lun.HasValue)
                        datadisk.lun = dataDisk.Lun.Value;

                    // if the tool is configured to create new VMs with empty data disks
                    if (this.BuildEmpty)
                    {
                        datadisk.createOption = "Empty";
                    }
                    // if the tool is configured to attach copied disks
                    else
                    {
                        datadisk.createOption = "Attach";
                    }

                    if (dataDisk.IsUnmanagedDisk)
                    {
                        Vhd vhd = new Vhd
                        {
                            uri = dataDisk.TargetMediaLink
                        };
                        datadisk.vhd = vhd;

                        if (dataDisk.TargetStorage != null && (dataDisk.TargetStorage.GetType() == typeof(Arm.StorageAccount) || dataDisk.TargetStorage.GetType() == typeof(MigrationTarget.StorageAccount)))
                        {
                            // BuildBlobCopy is only called here for migration to Existing ARM Storage Accounts, as call to BuildBlobCopy for ManagedDisks is already called via the "foreach (ManagedDisk in ManagedDisks)" in GenerateStreams to ensure all ManagedDisks are exported
                            this._CopyBlobDetails.Add(await BuildCopyBlob(dataDisk, targetResourceGroup));
                        }
                    }
                    else if (dataDisk.IsManagedDisk)
                    {
                        Reference managedDiskReference = new Reference
                        {
                            id = dataDisk.ReferenceId
                        };

                        datadisk.diskSizeGB = null;
                        datadisk.managedDisk = managedDiskReference;

                        dependson.Add(dataDisk.ReferenceId);
                    }

                    datadisks.Add(datadisk);
                }
            }

            StorageProfile storageprofile = new StorageProfile
            {
                osDisk = osdisk,
                dataDisks = datadisks
            };
            if (this.BuildEmpty) { storageprofile.imageReference = imagereference; }

            VirtualMachine_Properties virtualmachine_properties = new VirtualMachine_Properties
            {
                hardwareProfile = hardwareprofile,
                networkProfile = networkprofile,
                storageProfile = storageprofile
            };
            if (this.BuildEmpty) { virtualmachine_properties.osProfile = osprofile; }

            // process availability set
            if (targetVirtualMachine.TargetAvailabilitySet != null)
            {
                Reference availabilitySetReference = new Reference();
                virtualmachine_properties.availabilitySet = availabilitySetReference;
                availabilitySetReference.id = "[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderAvailabilitySets + targetVirtualMachine.TargetAvailabilitySet.ToString() + "')]";
                dependson.Add("[concat(" + ArmConst.ResourceGroupId + ", '" + ArmConst.ProviderAvailabilitySets + targetVirtualMachine.TargetAvailabilitySet.ToString() + "')]");
            }

            VirtualMachine templateVirtualMachine = new VirtualMachine(this.ExecutionGuid)
            {
                name = targetVirtualMachine.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetVirtualMachine.ApiVersion,
                properties = virtualmachine_properties,
                dependsOn = dependson,
                resources = new List<ArmResource>()
            };

            // Virtual Machine Plan Attributes (i.e. VM is an Azure Marketplace item that has a Marketplace plan associated
            templateVirtualMachine.plan = targetVirtualMachine.PlanAttributes;

            // Diagnostics Extension
            Extension extension_iaasdiagnostics = null;
            if (extension_iaasdiagnostics != null) { templateVirtualMachine.resources.Add(extension_iaasdiagnostics); }

            if (targetVirtualMachine.IsManagedDisks)
            {
                // using API Version "2017-03-30" per current documentation at https://docs.microsoft.com/en-us/azure/storage/storage-using-managed-disks-template-deployments
                templateVirtualMachine.apiVersion = "2017-03-30";
            }

            this.AddResource(templateVirtualMachine);

            LogProvider.WriteLog("BuildVirtualMachineObject", "End Microsoft.Compute/virtualMachines/" + targetVirtualMachine.ToString());
        }

        private async Task<ManagedDisk> BuildManagedDiskObject(Azure.MigrationTarget.Disk targetManagedDisk)
        {
            // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/using-managed-disks-template-deployments
            // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/template-description

            LogProvider.WriteLog("BuildManagedDiskObject", "Start Microsoft.Compute/disks/" + targetManagedDisk.ToString());

            Dictionary<string, string> managedDiskSku = new Dictionary<string, string>();
            managedDiskSku.Add("name", targetManagedDisk.StorageAccountType.ToString());

            ManagedDisk_Properties templateManagedDiskProperties = new ManagedDisk_Properties
            {
                diskSizeGb = targetManagedDisk.DiskSizeInGB
            };

            ManagedDiskCreationData_Properties templateManageDiskCreationDataProperties = new ManagedDiskCreationData_Properties();
            templateManagedDiskProperties.creationData = templateManageDiskCreationDataProperties;

            if (targetManagedDisk.TargetStorage != null && targetManagedDisk.TargetStorage.GetType() == typeof(Azure.MigrationTarget.ManagedDiskStorage))
            {
                string managedDiskSourceUriParameterName = targetManagedDisk.ToString() + "_SourceUri";

                if (!this.Parameters.ContainsKey(managedDiskSourceUriParameterName))
                {
                    Parameter parameter = new Parameter
                    {
                        type = "string",
                        value = managedDiskSourceUriParameterName + "_BlobCopyResult"
                    };

                    this.Parameters.Add(managedDiskSourceUriParameterName, parameter);
                }

                templateManageDiskCreationDataProperties.createOption = "Import";
                templateManageDiskCreationDataProperties.sourceUri = "[parameters('" + managedDiskSourceUriParameterName + "')]";
            }

            // sample json with encryption
            // https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/201-create-encrypted-managed-disk/CreateEncryptedManagedDisk-kek.json

            ManagedDisk templateManagedDisk = new ManagedDisk(this.ExecutionGuid)
            {
                name = targetManagedDisk.ToString(),
                location = "[resourceGroup().location]",
                apiVersion = targetManagedDisk.ApiVersion,
                sku = managedDiskSku,
                properties = templateManagedDiskProperties
            };

            this.AddResource(templateManagedDisk);

            LogProvider.WriteLog("BuildManagedDiskObject", "End Microsoft.Compute/disks/" + targetManagedDisk.ToString());

            return templateManagedDisk;
        }

        private async Task<BlobCopyDetail> BuildCopyBlob(MigrationTarget.Disk disk, MigrationTarget.ResourceGroup resourceGroup)
        {
            if (disk.Source == null)
                return null;

            BlobCopyDetail copyblobdetail = new BlobCopyDetail();
            
            if (disk.SourceStorageAccount != null && disk.SourceStorageAccount.AzureSubscription != null && disk.SourceStorageAccount.AzureSubscription.AzureEnvironment != null)
                copyblobdetail.SourceEnvironment = disk.SourceStorageAccount.AzureSubscription.AzureEnvironment.ToString();

            copyblobdetail.TargetResourceGroup = resourceGroup.ToString();
            copyblobdetail.TargetLocation = resourceGroup.TargetLocation.Name.ToString();

            if (disk.Source != null)
            {
                if (disk.Source.GetType() == typeof(Asm.Disk))
                {
                    Asm.Disk asmClassicDisk = (Asm.Disk)disk.Source;

                    copyblobdetail.SourceStorageAccount = asmClassicDisk.StorageAccountName;
                    copyblobdetail.SourceContainer = asmClassicDisk.StorageAccountContainer;
                    copyblobdetail.SourceBlob = asmClassicDisk.StorageAccountBlob;

                    if (asmClassicDisk.SourceStorageAccount != null && asmClassicDisk.SourceStorageAccount.Keys != null)
                        copyblobdetail.SourceKey = asmClassicDisk.SourceStorageAccount.Keys.Primary;
                }
                else if (disk.Source.GetType() == typeof(Arm.ClassicDisk))
                {
                    Arm.ClassicDisk armClassicDisk = (Arm.ClassicDisk)disk.Source;

                    copyblobdetail.SourceStorageAccount = armClassicDisk.StorageAccountName;
                    copyblobdetail.SourceContainer = armClassicDisk.StorageAccountContainer;
                    copyblobdetail.SourceBlob = armClassicDisk.StorageAccountBlob;

                    if (armClassicDisk.SourceStorageAccount != null && armClassicDisk.SourceStorageAccount.Keys != null)
                        copyblobdetail.SourceKey = armClassicDisk.SourceStorageAccount.Keys[0].Value;
                }
                else if (disk.Source.GetType() == typeof(Arm.ManagedDisk))
                {
                    Arm.ManagedDisk armManagedDisk = (Arm.ManagedDisk)disk.Source;

                    copyblobdetail.SourceAbsoluteUri = await armManagedDisk.GetSASUrlAsync(_AccessSASTokenLifetime);
                    copyblobdetail.SourceExpiration = DateTime.UtcNow.AddSeconds(this.AccessSASTokenLifetimeSeconds);
                }
            }

            copyblobdetail.TargetContainer = disk.TargetStorageAccountContainer;
            copyblobdetail.TargetBlob = disk.TargetStorageAccountBlob;

            copyblobdetail.TargetEndpoint = this.TargetSubscription.AzureEnvironment.StorageEndpointSuffix;

            if (disk.TargetStorage != null)
            {
                if (disk.TargetStorage.GetType() == typeof(Arm.StorageAccount))
                {
                    Arm.StorageAccount armStorageAccount = (Arm.StorageAccount)disk.TargetStorage;
                    copyblobdetail.TargetResourceGroup = armStorageAccount.ResourceGroup.Name;
                    copyblobdetail.TargetLocation = armStorageAccount.ResourceGroup.Location.Name.ToString();
                    copyblobdetail.TargetStorageAccount = disk.TargetStorage.ToString();
                }
                else if (disk.TargetStorage.GetType() == typeof(MigrationTarget.StorageAccount))
                {
                    copyblobdetail.TargetStorageAccount = disk.TargetStorage.ToString();
                    copyblobdetail.TargetStorageAccountType = disk.TargetStorage.StorageAccountType.ToString();
                }
                else if (disk.TargetStorage.GetType() == typeof(MigrationTarget.ManagedDiskStorage))
                {
                    MigrationTarget.ManagedDiskStorage managedDiskStorage = (MigrationTarget.ManagedDiskStorage)disk.TargetStorage;

                    // We are going to use a temporary storage account to stage the VHD file(s), which will then be deleted after Disk Creation
                    MigrationTarget.StorageAccount targetTemporaryStorageAccount = GetTempStorageAccountName(disk);
                    copyblobdetail.TargetStorageAccount = targetTemporaryStorageAccount.ToString();
                    copyblobdetail.TargetStorageAccountType = targetTemporaryStorageAccount.StorageAccountType.ToString();
                    copyblobdetail.TargetBlob = disk.TargetName + ".vhd";
                    copyblobdetail.OutputParameterName = disk.ToString() + "_SourceUri_BlobCopyResult";
                }
            }

            return copyblobdetail;
        }

        private MigrationTarget.StorageAccount GetTempStorageAccountName(MigrationTarget.Disk disk)
        {
            foreach (MigrationTarget.StorageAccount temporaryStorageAccount in this._TemporaryStorageAccounts)
            {
                if (temporaryStorageAccount.StorageAccountType == disk.StorageAccountType)
                {
                    return temporaryStorageAccount;
                }
            }

            MigrationTarget.StorageAccount newTemporaryStorageAccount = new MigrationTarget.StorageAccount(this.TargetSubscription, disk.StorageAccountType, disk.TargetSettings, this.LogProvider);
            _TemporaryStorageAccounts.Add(newTemporaryStorageAccount);

            return newTemporaryStorageAccount;
        }

        private void BuildStorageAccountObject(MigrationTarget.StorageAccount targetStorageAccount)
        {
            LogProvider.WriteLog("BuildStorageAccountObject", "Start Microsoft.Storage/storageAccounts/" + targetStorageAccount.ToString());

            StorageAccount_Properties storageaccount_properties = new StorageAccount_Properties
            {
                accountType = targetStorageAccount.StorageAccountType.ToString()
            };

            StorageAccount storageaccount = new StorageAccount(this.ExecutionGuid)
            {
                name = targetStorageAccount.ToString(),
                location = "[resourceGroup().location]",
                properties = storageaccount_properties,
                apiVersion = targetStorageAccount.ApiVersion
            };

            this.AddResource(storageaccount);

            LogProvider.WriteLog("BuildStorageAccountObject", "End");
        }

        private bool HasBlobCopyDetails
        {
            get
            {
                return _CopyBlobDetails.Count > 0;
            }
        }

        private async Task SerializeBlobCopyDetails()
        {
            LogProvider.WriteLog("SerializeBlobCopyDetails", "Start");

            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            // Only generate Blob Copy Detail file if it contains disks that are being copied
            if (_CopyBlobDetails.Count > 0)
            {
                StatusProvider.UpdateStatus("BUSY:  Generating " + this.GetBlobCopyDetailFilename());
                LogProvider.WriteLog("SerializeStreams", "Start " + this.GetBlobCopyDetailFilename() + " stream");

                string jsontext = JsonConvert.SerializeObject(this._CopyBlobDetails, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
                byte[] b = asciiEncoding.GetBytes(jsontext);
                MemoryStream copyBlobDetailStream = new MemoryStream();
                await copyBlobDetailStream.WriteAsync(b, 0, b.Length);
                TemplateStreams.Add(this.GetBlobCopyDetailFilename(), copyBlobDetailStream);

                LogProvider.WriteLog("SerializeStreams", "End " + this.GetBlobCopyDetailFilename() + " stream");
            }

            LogProvider.WriteLog("SerializeBlobCopyDetails", "End");
        }

        private async Task SerializeExportTemplate()
        {
            LogProvider.WriteLog("SerializeExportTemplate", "Start");

            StatusProvider.UpdateStatus("BUSY:  Generating" + this.GetTemplateFilename());

            String templateString = await GetTemplateString();
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();
            byte[] a = asciiEncoding.GetBytes(templateString);
            MemoryStream templateStream = new MemoryStream();
            await templateStream.WriteAsync(a, 0, a.Length);
            TemplateStreams.Add(this.GetTemplateFilename(), templateStream);

            LogProvider.WriteLog("SerializeExportTemplate", "End");
        }
        private async Task SerializeParameterTemplate()
        {
            LogProvider.WriteLog("SerializeParameterTemplate", "Start");

            if (this.Parameters.Count > 0)
            {
                StatusProvider.UpdateStatus("BUSY:  Generating " + this.GetTemplateParameterFilename());

                String templateString = await GetParameterString();
                ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                byte[] a = asciiEncoding.GetBytes(templateString);
                MemoryStream templateStream = new MemoryStream();
                await templateStream.WriteAsync(a, 0, a.Length);
                TemplateStreams.Add(this.GetTemplateParameterFilename(), templateStream);
            }

            LogProvider.WriteLog("SerializeParameterTemplate", "End");
        }

        private async Task SerializeMigAzPowerShell()
        {
            LogProvider.WriteLog("SerializeMigAzPowerShell", "Start");

            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            StatusProvider.UpdateStatus("BUSY:  Generating MigAz.ps1");
            LogProvider.WriteLog("SerializeStreams", "Start MigAz.ps1 stream");

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MigAz.Azure.Generator.MigAz.ps1";
            string blobCopyPowerShell;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                blobCopyPowerShell = reader.ReadToEnd();
            }

            byte[] c = asciiEncoding.GetBytes(blobCopyPowerShell);
            MemoryStream blobCopyPowerShellStream = new MemoryStream();
            await blobCopyPowerShellStream.WriteAsync(c, 0, c.Length);
            TemplateStreams.Add("MigAz.ps1", blobCopyPowerShellStream);

            LogProvider.WriteLog("SerializeMigAzPowerShell", "End");
        }

        private async Task SerializeDeploymentInstructions()
        {
            LogProvider.WriteLog("SerializeDeploymentInstructions", "Start");

            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            StatusProvider.UpdateStatus("BUSY:  Generating " + this.GetDeployInstructionFilename());
            LogProvider.WriteLog("SerializeStreams", "Start " + this.GetDeployInstructionFilename() + " stream");

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MigAz.Azure.Generator.DeployDocTemplate.html";
            string instructionContent;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                instructionContent = reader.ReadToEnd();
            }

            string azureEnvironmentParameter = String.Empty;
            string tenantParameter = String.Empty;
            string subscriptionParameter = String.Empty;
            string accountIdParameter = String.Empty;
            string accessTokenParameter = String.Empty;
            StringBuilder sbCustomAzureEnvironment = new StringBuilder();

            if (this.TargetSubscription != null)
            {
                if (this.TargetAzureTokenProvider != null)
                {
                    AuthenticationResult authenticationResult = this.TargetAzureTokenProvider.GetToken(this.TargetSubscription.TokenResourceUrl, this.TargetSubscription.AzureTenant.TenantId).Result;
                    if (authenticationResult != null)
                    {
                        accountIdParameter = " -AccountId '" + authenticationResult.Account.Username + "'";
                        accessTokenParameter = " -AccessToken '" + authenticationResult.AccessToken + "'";
                    }
                }

                subscriptionParameter = " -SubscriptionId '" + this.TargetSubscription.SubscriptionId + "'";

                if (this.TargetSubscription.AzureEnvironment.Name != "AzureCloud")
                    azureEnvironmentParameter = " -Environment " + this.TargetSubscription.AzureEnvironment.ToString();

                if (this.TargetSubscription.AzureEnvironment.IsUserDefined)
                {
                    sbCustomAzureEnvironment.Append("<li>");
                    sbCustomAzureEnvironment.Append("<p>Your Azure Resource Manager deployment utilized a custom Azure Environment.  Ensure Azure PowerShell has this custom Azure Environment added:</p>");

                    // https://docs.microsoft.com/en-us/powershell/module/azurerm.profile/add-azurermenvironment?view=azurermps-4.4.1
                    sbCustomAzureEnvironment.Append("<pre class=\"wrap\">");
                    sbCustomAzureEnvironment.Append("Add-AzureRmEnvironment");
                    sbCustomAzureEnvironment.Append(" -Name ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.Name);
                    sbCustomAzureEnvironment.Append(" -AdTenant ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.AdTenant);
                    sbCustomAzureEnvironment.Append(" -ResourceManagerEndpoint ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.ResourceManagerEndpoint);
                    sbCustomAzureEnvironment.Append(" -GraphEndpoint ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.GraphEndpoint);
                    sbCustomAzureEnvironment.Append(" -GraphEndpointResourceId ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.GraphEndpoint);
                    sbCustomAzureEnvironment.Append(" -ActiveDirectoryEndpoint ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.ActiveDirectoryEndpoint);
                    sbCustomAzureEnvironment.Append(" -StorageEndpointSuffix ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.StorageEndpointSuffix);
                    sbCustomAzureEnvironment.Append(" -SqlDatabaseDnsSuffix ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.SqlDatabaseDnsSuffix);
                    sbCustomAzureEnvironment.Append(" -TrafficManagerDnsSuffix ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.TrafficManagerDnsSuffix);
                    sbCustomAzureEnvironment.Append(" -AzureKeyVaultDnsSuffix ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.AzureKeyVaultDnsSuffix);
                    sbCustomAzureEnvironment.Append(" -ServiceManagementUrl ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.ServiceManagementUrl);
                    sbCustomAzureEnvironment.Append(" -ActiveDirectoryServiceEndpointResourceId ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.ActiveDirectoryServiceEndpointResourceId);
                    sbCustomAzureEnvironment.Append(" -GalleryUrl ");
                    sbCustomAzureEnvironment.Append(this.TargetSubscription.AzureEnvironment.GalleryUrl);

                    sbCustomAzureEnvironment.Append("</pre>");

                    sbCustomAzureEnvironment.Append("</li>");
                }

                if (this.TargetSubscription.AzureAdTenantId != Guid.Empty)
                    tenantParameter = " -TenantId '" + this.TargetSubscription.AzureAdTenantId.ToString() + "'";
            }

            instructionContent = instructionContent.Replace("{AddCustomEnvironment}", sbCustomAzureEnvironment.ToString());
            instructionContent = instructionContent.Replace("{azureEnvironmentParameter}", azureEnvironmentParameter);
            instructionContent = instructionContent.Replace("{tenantParameter}", tenantParameter);
            instructionContent = instructionContent.Replace("{subscriptionParameter}", subscriptionParameter);
            instructionContent = instructionContent.Replace("{accountIdParameter}", accountIdParameter);
            instructionContent = instructionContent.Replace("{accessTokenParameter}", accessTokenParameter);
            instructionContent = instructionContent.Replace("{templatePath}", GetTemplatePath());
            instructionContent = instructionContent.Replace("{blobDetailsPath}", GetCopyBlobDetailPath());
            instructionContent = instructionContent.Replace("{resourceGroupName}", this.TargetResourceGroupName);
            instructionContent = instructionContent.Replace("{location}", this.TargetResourceGroupLocation);
            instructionContent = instructionContent.Replace("{migAzPath}", AppDomain.CurrentDomain.BaseDirectory);
            instructionContent = instructionContent.Replace("{exportPath}", _OutputDirectory);
            instructionContent = instructionContent.Replace("{migAzMessages}", BuildMigAzMessages());
            instructionContent = instructionContent.Replace("{resourceGroupNameParameter}", " -ResourceGroupName \"" + this.TargetResourceGroupName + "\"");
            instructionContent = instructionContent.Replace("{resourceGroupLocationParameter}", " -ResourceGroupLocation \"" + this.TargetResourceGroupLocation + "\"");
            instructionContent = instructionContent.Replace("{templateFileParameter}", " -TemplateFile \"" + GetTemplatePath() + "\"");

            if (this.BuildEmpty)
            {
                instructionContent = instructionContent.Replace("{blobCopyFileParameter}", String.Empty); // In Empty Build, we don't do any blob copies
            }
            else
            {
                if (this.HasBlobCopyDetails)
                    instructionContent = instructionContent.Replace("{blobCopyFileParameter}", " -BlobCopyFile \"" + GetCopyBlobDetailPath() + "\"");
                else
                    instructionContent = instructionContent.Replace("{blobCopyFileParameter}", String.Empty);
            }

            instructionContent = instructionContent.Replace("{migazExecutionCommand}", "&amp; '" + _OutputDirectory + "MigAz.ps1'");

            if (this.Parameters.Count == 0)
                instructionContent = instructionContent.Replace("{templateParameterFileParameter}", String.Empty);
            else
                instructionContent = instructionContent.Replace("{templateParameterFileParameter}", " -TemplateParameterFile \"" + GetTemplateParameterPath() + "\"");


            byte[] c = asciiEncoding.GetBytes(instructionContent);
            MemoryStream instructionStream = new MemoryStream();
            await instructionStream.WriteAsync(c, 0, c.Length);

            if (TemplateStreams.Keys.Contains(this.GetDeployInstructionFilename()))
            {
                TemplateStreams.Remove(this.GetDeployInstructionFilename());
            }

            TemplateStreams.Add(this.GetDeployInstructionFilename(), instructionStream);

            LogProvider.WriteLog("SerializeDeploymentInstructions", "End");
        }

        public string GetDeployInstructionFilename()
        {
            return this.TargetResourceGroupName.Replace(" ", "_") + " Deployment Instructions.html";
        }

        public string GetBlobCopyDetailFilename()
        {
            if (this.HasBlobCopyDetails)
                return this.TargetResourceGroupName.Replace(" ", "_") + "_BlobCopy.json";
            else
                return String.Empty;
        }

        public string GetCopyBlobDetailPath()
        {
            if (this.HasBlobCopyDetails)
                return Path.Combine(this.OutputDirectory, this.GetBlobCopyDetailFilename());
            else
                return String.Empty;
        }

        public string GetTemplateFilename()
        {
            return this.TargetResourceGroupName.Replace(" ", "_") + ".json";
        }

        public string GetTemplatePath()
        {
            return Path.Combine(this.OutputDirectory, GetTemplateFilename());
        }

        public string GetTemplateParameterFilename()
        {
            return this.TargetResourceGroupName.Replace(" ", "_") + "_Parameters.json";
        }

        public string GetTemplateParameterPath()
        {
            return Path.Combine(this.OutputDirectory, GetTemplateParameterFilename());
        }

        public string GetInstructionPath()
        {
            return Path.Combine(this.OutputDirectory, this.GetDeployInstructionFilename());
        }

        public async Task<string> GetTemplateString()
        {
            Template template = new Template()
            {
                resources = this.Resources,
                parameters = GetParamatersWithNoValues(this.Parameters)
            };

            // save JSON template
            string jsontext = JsonConvert.SerializeObject(template, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            jsontext = jsontext.Replace("schemalink", "$schema");

            return jsontext;
        }

        private Dictionary<string, Parameter> GetParamatersWithNoValues(Dictionary<string, Parameter> parameters)
        {
            Dictionary<string, Parameter> paramsNoValues = new Dictionary<string, Parameter>();

            foreach (string key in parameters.Keys)
            {
                Parameter parameter;
                parameters.TryGetValue(key, out parameter);

                if (parameter != null)
                {
                    Parameter newParameter = new Parameter();
                    newParameter.type = parameter.type;

                    paramsNoValues.Add(key, newParameter);
                }
            }

            return paramsNoValues;
        }

        private async Task<string> GetParameterString()
        {
            Template template = new Template()
            {
                parameters = this.Parameters
            };

            // save JSON template
            string jsontext = JsonConvert.SerializeObject(template, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            jsontext = jsontext.Replace("schemalink", "$schema");

            return jsontext;
        }


        public async Task<string> GetStorageAccountTemplateString()
        {
            List<ArmResource> storageAccountResources = new List<ArmResource>();
            foreach (ArmResource armResource in this.Resources)
            {
                if (armResource.GetType() == typeof(MigAz.Azure.Core.ArmTemplate.StorageAccount))
                {
                    storageAccountResources.Add(armResource);
                }
            }

            Template template = new Template()
            {
                resources = storageAccountResources,
                parameters = new Dictionary<string, Parameter>()
            };

            // save JSON template
            string jsontext = JsonConvert.SerializeObject(template, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            jsontext = jsontext.Replace("schemalink", "$schema");

            return jsontext;
        }

        public string TargetResourceGroupName
        {
            get
            {
                if (_ExportArtifacts != null && _ExportArtifacts.ResourceGroup != null && _ExportArtifacts.ResourceGroup.TargetLocation != null)
                    return _ExportArtifacts.ResourceGroup.ToString();
                else
                    return String.Empty;

            }
        }

        public string TargetResourceGroupLocation
        {
            get
            {
                if (_ExportArtifacts != null && _ExportArtifacts.ResourceGroup != null && _ExportArtifacts.ResourceGroup.TargetLocation != null)
                    return _ExportArtifacts.ResourceGroup.TargetLocation.Name;
                else
                    return String.Empty;
            }
        }

        public ExportArtifacts ExportArtifacts
        {
            get { return _ExportArtifacts; }
            set { _ExportArtifacts = value; }
        }

        protected void OnTemplateChanged()
        {
            // Call the base class event invocation method.
            EventHandler handler = AfterTemplateChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        ////The event-invoking method that derived classes can override.
        //protected virtual void OnTemplateChanged()
        //{
        //    // Make a temporary copy of the event to avoid possibility of
        //    // a race condition if the last subscriber unsubscribes
        //    // immediately after the null check and before the event is raised.
        //    EventHandler handler = AfterTemplateChanged;
        //    if (handler != null)
        //    {
        //        handler(this, null);
        //    }
        //}
    }
}


// This method was from ASM to ARM, need to reconstruct, but must now use Target Objects to facilitate template build
//private async Task AddGatewaysToVirtualNetwork(MigrationTarget.VirtualNetwork targetVirtualNetwork, VirtualNetwork templateVirtualNetwork)
//{
//    if (targetVirtualNetwork != null)
//    {
//        if (targetVirtualNetwork.SourceVirtualNetwork != null)
//        {
//            if (targetVirtualNetwork.SourceVirtualNetwork.GetType() == typeof(Azure.Asm.VirtualNetwork))
//            {
//                Asm.VirtualNetwork asmVirtualNetwork = (Asm.VirtualNetwork)targetVirtualNetwork.SourceVirtualNetwork;

//                // Process Virtual Network Gateway, if exists
//                if ((asmVirtualNetwork.Gateway != null) && (asmVirtualNetwork.Gateway.IsProvisioned))
//                {
//                    //// Gateway Public IP Address
//                    //PublicIPAddress_Properties publicipaddress_properties = new PublicIPAddress_Properties();
//                    //publicipaddress_properties.publicIPAllocationMethod = "Dynamic";

//                    //PublicIPAddress publicipaddress = new PublicIPAddress(this.ExecutionGuid);
//                    //publicipaddress.name = targetVirtualNetwork.TargetName; // todo now  + _settingsProvider.VirtualNetworkGatewaySuffix + _settingsProvider.PublicIPSuffix;
//                    //publicipaddress.location = "[resourceGroup().location]";
//                    //publicipaddress.properties = publicipaddress_properties;

//                    //this.AddResource(publicipaddress);

//                    // If there is VPN Client configuration
//                    //if (asmVirtualNetwork.VPNClientAddressPrefixes.Count > 0)
//                    //{
//                    //    AddressSpace vpnclientaddresspool = new AddressSpace();
//                    //    vpnclientaddresspool.addressPrefixes = asmVirtualNetwork.VPNClientAddressPrefixes;

//                    //    VPNClientConfiguration vpnclientconfiguration = new VPNClientConfiguration();
//                    //    vpnclientconfiguration.vpnClientAddressPool = vpnclientaddresspool;

//                    //    //Process vpnClientRootCertificates
//                    //    List<VPNClientCertificate> vpnclientrootcertificates = new List<VPNClientCertificate>();
//                    //    foreach (Asm.ClientRootCertificate certificate in asmVirtualNetwork.ClientRootCertificates)
//                    //    {
//                    //        VPNClientCertificate_Properties vpnclientcertificate_properties = new VPNClientCertificate_Properties();
//                    //        vpnclientcertificate_properties.PublicCertData = certificate.PublicCertData;

//                    //        VPNClientCertificate vpnclientcertificate = new VPNClientCertificate();
//                    //        vpnclientcertificate.name = certificate.TargetSubject;
//                    //        vpnclientcertificate.properties = vpnclientcertificate_properties;

//                    //        vpnclientrootcertificates.Add(vpnclientcertificate);
//                    //    }

//                    //    vpnclientconfiguration.vpnClientRootCertificates = vpnclientrootcertificates;

//                    //    virtualnetworkgateway_properties.vpnClientConfiguration = vpnclientconfiguration;
//                    //}

//                    //if (asmVirtualNetwork.LocalNetworkSites.Count > 0 && asmVirtualNetwork.LocalNetworkSites[0].ConnectionType == "Dedicated")
//                    //{
//                    //    //virtualnetworkgateway_properties.gatewayType = "ExpressRoute";
//                    //    //virtualnetworkgateway_properties.enableBgp = null;
//                    //    //virtualnetworkgateway_properties.vpnType = null;
//                    //}
//                    //else
//                    //{
//                    //    //virtualnetworkgateway_properties.gatewayType = "Vpn";
//                    //    //string vpnType = asmVirtualNetwork.Gateway.GatewayType;
//                    //    //if (vpnType == "StaticRouting")
//                    //    //{
//                    //    //    vpnType = "PolicyBased";
//                    //    //}
//                    //    //else if (vpnType == "DynamicRouting")
//                    //    //{
//                    //    //    vpnType = "RouteBased";
//                    //    //}
//                    //    //virtualnetworkgateway_properties.vpnType = vpnType;
//                    //}

//                    // todo now move to ExportArtifactValidation
//                    //if (!asmVirtualNetwork.HasGatewaySubnet)
//                    //    this.AddAlert(AlertType.Error, "The Virtual Network '" + targetVirtualNetwork.TargetName + "' does not contain the necessary '" + ArmConst.GatewaySubnetName + "' subnet for deployment of the '" + virtualnetworkgateway.name + "' Gateway.", asmVirtualNetwork);
//                }
//            }
//        }
//    }
//}
