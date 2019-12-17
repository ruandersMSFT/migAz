// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.EC2.Model;
using System.Windows.Forms;
using MigAz.Azure.Core.Interface;
using Amazon.ElasticLoadBalancing.Model;
using Amazon.ElasticLoadBalancing;
using System.Net;
using MigAz.Azure.MigrationTarget;

namespace MigAz.AWS.Generator
{
    public class AWSRetriever
    {
        private AwsObjectRetriever _awsObjectRetriever;
        private IAmazonElasticLoadBalancing _LBservice;
        private ILogProvider _LogProvider;
        private IStatusProvider _StatusProvider;

        private AWSRetriever() { }

        public AWSRetriever(ILogProvider logProvider, IStatusProvider statusProvider)
        {
            _LogProvider = logProvider;
            _StatusProvider = statusProvider;
        }

        private DescribeVolumesResponse getEbsVolumes()
        {
            return _awsObjectRetriever.Volumes;
        }

        private DescribeVpcsResponse getVPCs()
        {
            return _awsObjectRetriever.Vpcs;
        }

        private DescribeInstancesResponse getEC2Instances()
        {
            return _awsObjectRetriever.Instances;
        }

        private IAmazonElasticLoadBalancing createLBClient(string accessKeyID, string secretKeyID, Amazon.RegionEndpoint region)
        {
            try
            {
                _LBservice = new AmazonElasticLoadBalancingClient(accessKeyID, secretKeyID, new AmazonElasticLoadBalancingConfig()
                {
                    RegionEndpoint = region
                });
                return _LBservice;
            }
            catch (AmazonElasticLoadBalancingException ex)
            {
                throw ex;
            }
        }



        public void toname(string accessKey, string secretKey, TreeNode subscriptionNode)
        {
            List<Amazon.RegionEndpoint> regionsList = new List<Amazon.RegionEndpoint>();
            foreach (var region in Amazon.RegionEndpoint.EnumerableAllRegions)
            {
                try
                {
                    _awsObjectRetriever = new AwsObjectRetriever(accessKey, secretKey, region, _LogProvider, _StatusProvider);
                    //// todo, not needed in this method_templateGenerator = new TemplateGenerator(_logProvider, _statusProvider, _awsObjectRetriever, telemetryProvider);

                    TreeNode amazonRegionNode = new TreeNode(region.DisplayName);
                    amazonRegionNode.Text = region.DisplayName;
                    amazonRegionNode.Tag = region;

                    //DescribeVolumesResponse ebsVolumesResponse;

                    _StatusProvider.UpdateStatus("BUSY: Getting the VPC details");
                    DescribeVpcsResponse vpcResponse = getVPCs();
                    Application.DoEvents();

                    List<Azure.MigrationTarget.VirtualNetwork> targetVirtualNetworks = new List<Azure.MigrationTarget.VirtualNetwork>();
                    _StatusProvider.UpdateStatus("BUSY: Processing VPC");
                    foreach (var vpc in vpcResponse.Vpcs)
                    {
                        MigAz.AWS.MigrationSource.VirtualNetwork sourceVirtualNetwork = new MigrationSource.VirtualNetwork(_awsObjectRetriever, vpc);
                        Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork = new Azure.MigrationTarget.VirtualNetwork(null, sourceVirtualNetwork, null, null); // todo not null AzureSubscription + Log Provider
                        targetVirtualNetworks.Add(targetVirtualNetwork);

                        TreeNode vpcTreeNode = new TreeNode(sourceVirtualNetwork.Id + " - " + sourceVirtualNetwork.Name);
                        vpcTreeNode.Tag = targetVirtualNetwork;
                        vpcTreeNode.ImageKey = "VirtualNetwork";
                        vpcTreeNode.SelectedImageKey = "VirtualNetwork";
                        amazonRegionNode.Nodes.Add(vpcTreeNode);

                    //    // todo, this is foreach subnet
                    //    foreach (Subnet subnet in subnets)
                    //    {
                    //        //QUES: Single Sec group?
                    //        // add Network Security Group if exists - 2 subnets - each acl is associated with both
                    //        List<Amazon.EC2.Model.NetworkAcl> networkAcls = _awsObjectRetriever.getNetworkAcls(subnetnode.SubnetId);
                    //        List<Amazon.EC2.Model.RouteTable> routeTable = _awsObjectRetriever.getRouteTables(subnetnode.SubnetId);



                    //        //var nodes = networkAcls.SelectSingleNode("DescribeNetworkAclsResponse ").SelectSingleNode("networkAclSet").SelectNodes("item");

                    //        if (networkAcls.Count > 0)
                    //        {
                    //            NetworkSecurityGroup networksecuritygroup = BuildNetworkSecurityGroup(networkAcls[0]);

                    //            //NetworkSecurityGroup networksecuritygroup = BuildNetworkSecurityGroup(subnet.name);

                    //            // Add NSG reference to the subnet
                    //            Reference networksecuritygroup_ref = new Reference();
                    //            networksecuritygroup_ref.id = "[concat(resourceGroup().id,'/providers/Microsoft.Network/networkSecurityGroups/" + networksecuritygroup.name + "')]";

                    //            properties.networkSecurityGroup = networksecuritygroup_ref;

                    //            // Add NSG dependsOn to the Virtual Network object
                    //            if (!virtualnetwork.dependsOn.Contains(networksecuritygroup_ref.id))
                    //            {
                    //                virtualnetwork.dependsOn.Add(networksecuritygroup_ref.id);
                    //            }

                    //        }

                    //        if (routeTable.Count > 0)
                    //        {
                    //            RouteTable routetable = BuildRouteTable(routeTable[0]);

                    //            if (routetable.properties != null)
                    //            {
                    //                // Add Route Table reference to the subnet
                    //                Reference routetable_ref = new Reference();
                    //                routetable_ref.id = "[concat(resourceGroup().id,'/providers/Microsoft.Network/routeTables/" + routetable.name + "')]";

                    //                properties.routeTable = routetable_ref;

                    //                // Add Route Table dependsOn to the Virtual Network object
                    //                if (!virtualnetwork.dependsOn.Contains(routetable_ref.id))
                    //                {
                    //                    virtualnetwork.dependsOn.Add(routetable_ref.id);
                    //                }
                    //            }
                    //        }
                    //    }

                    //}

                    Application.DoEvents();
                    }

                    DescribeInstancesResponse instResponse = getEC2Instances();
                    Application.DoEvents();

                    foreach (LoadBalancerDescription loadBalancerDescription in _awsObjectRetriever.GetAllLBs().LoadBalancerDescriptions)
                    {
                        MigAz.AWS.MigrationSource.LoadBalancer sourceLoadBalancer = new MigAz.AWS.MigrationSource.LoadBalancer(loadBalancerDescription);

                        Azure.MigrationTarget.LoadBalancer targetLoadBalancer = new Azure.MigrationTarget.LoadBalancer();
                        targetLoadBalancer.SetTargetName(loadBalancerDescription.LoadBalancerName, null);

                        Azure.MigrationTarget.FrontEndIpConfiguration targetFrontEndIpConfiguration = new Azure.MigrationTarget.FrontEndIpConfiguration(targetLoadBalancer);
                        targetFrontEndIpConfiguration.Name = "ipconfig1"; // can this come from Amazon?
                        
                        if (loadBalancerDescription.Scheme != "internet-facing") // if internal load balancer
                        {
                            foreach (Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork in targetVirtualNetworks)
                            {
                                if (targetVirtualNetwork.Source != null)
                                {
                                    AWS.MigrationSource.VirtualNetwork amazonVirtualNetwork = (AWS.MigrationSource.VirtualNetwork)targetVirtualNetwork.Source;
                                    if (amazonVirtualNetwork.Id == loadBalancerDescription.VPCId)
                                    {
                                        targetFrontEndIpConfiguration.TargetVirtualNetwork = targetVirtualNetwork;

                                        foreach (Azure.MigrationTarget.Subnet targetSubnet in targetVirtualNetwork.TargetSubnets)
                                        {
                                            if (((ISubnet)targetSubnet.Source).Id == loadBalancerDescription.Subnets[0])
                                            {
                                                targetFrontEndIpConfiguration.TargetSubnet = targetSubnet;
                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }

                            targetFrontEndIpConfiguration.TargetPrivateIPAllocationMethod = IPAllocationMethodEnum.Static;
                            try
                            {
                                IPHostEntry host = Dns.GetHostEntry(loadBalancerDescription.DNSName);
                                targetFrontEndIpConfiguration.TargetPrivateIpAddress = host.AddressList[0].ToString();
                            }
                            catch
                            {
                                targetFrontEndIpConfiguration.TargetPrivateIPAllocationMethod = IPAllocationMethodEnum.Dynamic;
                            }
                        }
                        else // if external (public) load balancer
                        {
                            Azure.MigrationTarget.PublicIp targetPublicIp = new Azure.MigrationTarget.PublicIp();
                            targetPublicIp.SetTargetName(loadBalancerDescription.LoadBalancerName, null);
                            targetPublicIp.DomainNameLabel = loadBalancerDescription.LoadBalancerName;

                            targetLoadBalancer.LoadBalancerType = Azure.MigrationTarget.LoadBalancerType.Public;
                            targetFrontEndIpConfiguration.PublicIp = targetPublicIp;

                            TreeNode loadBalancerPublicIpNode = new TreeNode(targetPublicIp.TargetName);
                            loadBalancerPublicIpNode.Tag = targetPublicIp;
                            loadBalancerPublicIpNode.ImageKey = "PublicIp";
                            loadBalancerPublicIpNode.SelectedImageKey = "PublicIp";
                            amazonRegionNode.Nodes.Add(loadBalancerPublicIpNode);
                        }

                        TreeNode loadBalancerNode = new TreeNode(targetLoadBalancer.TargetName);
                        loadBalancerNode.Tag = targetLoadBalancer;
                        loadBalancerNode.ImageKey = "LoadBalancer";
                        loadBalancerNode.SelectedImageKey = "LoadBalancer";
                        amazonRegionNode.Nodes.Add(loadBalancerNode);
                    }

                    if (instResponse != null)
                    {
                        _StatusProvider.UpdateStatus("BUSY: Processing Instances");
                        if (instResponse.Reservations.Count > 0)
                        {
                            foreach (var instanceResp in instResponse.Reservations)
                            {
                                foreach (var instance in instanceResp.Instances)
                                {
                                    var selectedInstances = _awsObjectRetriever.getInstancebyId(instance.InstanceId);

//                                    List<NetworkProfile_NetworkInterface> networkinterfaces = new List<NetworkProfile_NetworkInterface>();

                                    String vpcId = selectedInstances.Instances[0].VpcId.ToString();

                                    //Process LBs
                                    var LBs = _awsObjectRetriever.GetAllLBs().LoadBalancerDescriptions;
                                    string instanceLBName = "";

                                    foreach (var LB in LBs)
                                    {
                                        foreach (var LBInstance in LB.Instances)
                                        {
                                            if ((LB.VPCId == vpcId) && (LBInstance.InstanceId == instance.InstanceId))
                                            {
                                                if (LB.Scheme == "internet-facing")
                                                {
                                                   // BuildPublicIPAddressObject(LB);
                                                }

                                                instanceLBName = LB.LoadBalancerName;
                                                //BuildLoadBalancerObject(LB, instance.InstanceId.ToString());
                                            }
                                        }
                                    }

                                    //Process Network Interface
                                    // todo now BuildNetworkInterfaceObject(selectedInstances.Instances[0], ref networkinterfaces, LBs);

                                    //Process EC2 Instance
                                    // todo now BuildVirtualMachineObject(selectedInstances.Instances[0], networkinterfaces, storageAccountName, instanceLBName);

                                    string name = "";
                                    foreach (var tag in instance.Tags)
                                    {
                                        if (tag.Key == "Name")
                                        {
                                            name = tag.Value;
                                        }
                                    }

                                    TreeNode instanceTreeNode = new TreeNode(instance.InstanceId + " - " + name);
                                    instanceTreeNode.Tag = instance;
                                    amazonRegionNode.Nodes.Add(instanceTreeNode);

                                    Application.DoEvents();
                                }
                            }
                        }
                    }




                    if (amazonRegionNode.Nodes.Count > 0)
                        subscriptionNode.Nodes.Add(amazonRegionNode);
                    else
                        _LogProvider.WriteLog("Load_Items", "Not adding Amazon Region '" + region.DisplayName + "' to Source Node list, as it contains no resources to export.");
                }
                catch (Exception exc)
                {
                    _LogProvider.WriteLog("Load_Items", "AWS Exception - " + region.DisplayName + ": " + exc.Message);
                }
            }
        }
    }
}

