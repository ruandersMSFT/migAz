// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MigAz.Azure.Core.Interface;
using MigAz.Azure.MigrationTarget;
using MigAz.Azure.Core.Generator;
using MigAz.Azure.Core;

namespace MigAz.Azure.UserControls
{
    public partial class TargetTreeView : UserControl
    {
        private ExportArtifacts _ExportArtifacts;
        private ResourceGroup _TargetResourceGroup;
        private TargetSettings _TargetSettings;

        public delegate Task AfterTargetSelectedHandler(TargetTreeView sender, TreeNode selectedNode);
        public event AfterTargetSelectedHandler AfterTargetSelected;

        public delegate Task AfterNewTargetResourceAddedHandler(TargetTreeView sender, TreeNode selectedNode);
        public event AfterNewTargetResourceAddedHandler AfterNewTargetResourceAdded;

        public delegate Task AfterExportArtifactRefreshHandler(TargetTreeView sender);
        public event AfterExportArtifactRefreshHandler AfterExportArtifactRefresh;

        public delegate Task AfterSourceNodeRemovedHandler(TargetTreeView sender, TreeNode removedNode);
        public event AfterSourceNodeRemovedHandler AfterSourceNodeRemoved;

        public TargetTreeView()
        {
            InitializeComponent();
        }

        public ILogProvider LogProvider
        {
            get; set;
        }

        public IStatusProvider StatusProvider
        {
            get; set;
        }

        public ImageList ImageList
        {
            get { return this.treeTargetARM.ImageList; }
            set
            {
                this.treeTargetARM.ImageList = value;
            }
        }

        public ResourceGroup TargetResourceGroup
        {
            get { return _TargetResourceGroup; }
        }

        public string TargetBlobStorageNamespace { get; set; }

        public TreeNodeCollection Nodes
        {
            get { return this.treeTargetARM.Nodes; }
        }

        public TreeNode SelectedNode
        {
            get { return this.treeTargetARM.SelectedNode; }
            set { this.treeTargetARM.SelectedNode = value; }
        }

        public TreeNode ResourceGroupNode
        {
            get
            {
                if (this.Nodes.Count == 1)
                    return this.Nodes[0];
                else
                    return null;
            }
        }

        public new bool Enabled
        {
            get { return treeTargetARM.Enabled; }
            set
            {
                treeTargetARM.Enabled = value;
            }
        }

        public ISettingsProvider SettingsProvider { get; set; }
        public bool HasStorageAccount
        {
            get
            {
                bool exists = false;

                foreach (TreeNode treeNode in treeTargetARM.Nodes)
                {
                    exists = this.ContainTagTypeRecursive(treeNode, typeof(StorageAccount));
                    if (exists)
                        return true;
                }

                return exists;
            }
        }

        internal Arm.ProviderResourceType GetTargetProvider(Core.MigrationTarget migrationTarget)
        {
            if (this.TargetSubscription == null)
                return null;

            return this.TargetSubscription.GetProviderResourceType(migrationTarget.ProviderNamespace, migrationTarget.ResourceType);
        }

        private bool ContainTagTypeRecursive(TreeNode parentTreeNode, System.Type tagType)
        {
            if (parentTreeNode.Tag != null && parentTreeNode.Tag.GetType() == tagType)
                return true;
            else
            {
                foreach (TreeNode selectedNode in parentTreeNode.Nodes)
                {
                    if (ContainTagTypeRecursive(selectedNode, tagType))
                        return true;
                }

                return false;
            }
        }
        private void GetExportArtifactsRecursive(TreeNode parentTreeNode, ref ExportArtifacts exportArtifacts)
        {
            foreach (TreeNode selectedNode in parentTreeNode.Nodes)
            {
                Type tagType = selectedNode.Tag.GetType();

                if (tagType == typeof(VirtualNetworkGateway))
                {
                    exportArtifacts.VirtualNetworkGateways.Add((VirtualNetworkGateway)selectedNode.Tag);
                }
                else if (tagType == typeof(VirtualNetworkGatewayConnection))
                {
                    exportArtifacts.VirtualNetworkGatewayConnections.Add((VirtualNetworkGatewayConnection)selectedNode.Tag);
                }
                else if (tagType == typeof(LocalNetworkGateway))
                {
                    exportArtifacts.LocalNetworkGateways.Add((LocalNetworkGateway)selectedNode.Tag);
                }
                else if (tagType == typeof(VirtualNetwork))
                {
                    exportArtifacts.VirtualNetworks.Add((VirtualNetwork)selectedNode.Tag);
                }
                else if (tagType == typeof(StorageAccount))
                {
                    exportArtifacts.StorageAccounts.Add((StorageAccount)selectedNode.Tag);
                }
                else if (tagType == typeof(ApplicationSecurityGroup))
                {
                    exportArtifacts.ApplicationSecurityGroups.Add((ApplicationSecurityGroup)selectedNode.Tag);
                }
                else if (tagType == typeof(NetworkSecurityGroup))
                {
                    exportArtifacts.NetworkSecurityGroups.Add((NetworkSecurityGroup)selectedNode.Tag);
                }
                else if (tagType == typeof(AvailabilitySet))
                {
                    exportArtifacts.AvailablitySets.Add((AvailabilitySet)selectedNode.Tag);
                }
                else if (tagType == typeof(VirtualMachine))
                {
                    exportArtifacts.VirtualMachines.Add((VirtualMachine)selectedNode.Tag);
                }
                else if (tagType == typeof(LoadBalancer))
                {
                    exportArtifacts.LoadBalancers.Add((LoadBalancer)selectedNode.Tag);
                }
                else if (tagType == typeof(PublicIp))
                {
                    exportArtifacts.PublicIPs.Add((PublicIp)selectedNode.Tag);
                }
                else if (tagType == typeof(Disk))
                {
                    Disk targetDisk = (Disk)selectedNode.Tag;
                    if (targetDisk.TargetStorage != null && targetDisk.TargetStorage.GetType() == typeof(ManagedDiskStorage))
                    {
                        // We are only adding the Target Disk into the Export Artifacts as a Disk object if the Target Disk is targeted as a Managed Disk.
                        // Otherwise, it is a Classic Disk and will be generated as part of the Virtual Machine JSON (does not have a stand alone Disk object).
                        exportArtifacts.Disks.Add((Disk)selectedNode.Tag);
                    }
                }
                else if (tagType == typeof(NetworkInterface))
                {
                    exportArtifacts.NetworkInterfaces.Add((NetworkInterface)selectedNode.Tag);
                }
                else if (tagType == typeof(RouteTable))
                {
                    exportArtifacts.RouteTables.Add((RouteTable)selectedNode.Tag);
                }
            }

            foreach (TreeNode treeNode in parentTreeNode.Nodes)
            {
                GetExportArtifactsRecursive(treeNode, ref exportArtifacts);
            }
        }

        public async Task RefreshExportArtifacts()
        {
            _ExportArtifacts = new ExportArtifacts(this.TargetSubscription);

            if (this.Nodes.Count == 1 && this.Nodes[0].Tag != null)
                _ExportArtifacts.ResourceGroup = (MigrationTarget.ResourceGroup)this.Nodes[0].Tag;

            foreach (TreeNode treeNode in treeTargetARM.Nodes)
            {
                GetExportArtifactsRecursive(treeNode, ref _ExportArtifacts);
            }

            foreach (StorageAccount targetStorageAccount in _ExportArtifacts.StorageAccounts)
            {
                targetStorageAccount.BlobStorageNamespace = this.TargetBlobStorageNamespace;
            }

            await _ExportArtifacts.ValidateAzureResources();
            await AfterExportArtifactRefresh?.Invoke(this);
        }

        public ExportArtifacts ExportArtifacts
        {
            get
            {
                if (_ExportArtifacts == null)
                    return null;

                return _ExportArtifacts;
            }
        }

        public static Arm.VirtualNetwork test(Arm.ArmResource armResource)
        {
            return (Arm.VirtualNetwork)armResource;
        }

        internal List<Arm.VirtualNetwork> GetExistingVirtualNetworksInTargetLocation()
        {
            return TargetSubscription.ArmResourceGroups.SelectMany(x => x.Resources.Where(y => y.GetType() == typeof(Arm.VirtualNetwork) && y.Location == TargetResourceGroup.TargetLocation)).ToList().ConvertAll(new Converter<Arm.ArmResource, Arm.VirtualNetwork>(test));
        }
        internal List<Arm.PublicIP> GetExistingPublicIpsInTargetLocation()
        {
            List<Arm.PublicIP> existingPublicIPs = new List<Arm.PublicIP>();

            if (this.TargetResourceGroup != null && this.TargetResourceGroup.TargetLocation != null && this.TargetSubscription != null)
            {
                foreach (Arm.PublicIP armPublicIp in this.TargetSubscription.FilterArmPublicIPs(this.TargetResourceGroup.TargetLocation))
                {
                    existingPublicIPs.Add(armPublicIp);
                }
            }

            return existingPublicIPs;
        }

        internal void TransitionToManagedDisk(Disk disk)
        {
            TreeNode targetTreeNode = this.SeekMigrationTargetTreeNode(disk);
            if (targetTreeNode == null || targetTreeNode.Tag == null || targetTreeNode.Tag.GetType() != typeof(Disk))
                throw new ArgumentException("Invalid Treenode.  Treenode Tag must contain a Migration Disk object.");

            Disk targetDisk = (Disk)targetTreeNode.Tag;

            if (targetDisk.Source != null)
            {
                TreeNode diskParentNode = targetTreeNode.Parent;

                while (diskParentNode != null && diskParentNode.Tag != null && diskParentNode.Tag.GetType() != typeof(ResourceGroup))
                {
                    diskParentNode = diskParentNode.Parent;
                }

                // Move the disk treenode to be a child under the resource group, not under the VM, as we are making it a Managed Disk
                if (diskParentNode != null)
                {
                    treeTargetARM.Nodes.Remove(targetTreeNode);
                    diskParentNode.Nodes.Add(targetTreeNode);
                    treeTargetARM.SelectedNode = targetTreeNode;
                }
            }
        }

        internal void TransitionToClassicDisk(Disk disk)
        {
            TreeNode targetTreeNode = this.SeekMigrationTargetTreeNode(disk);
            if (targetTreeNode == null || targetTreeNode.Tag == null || targetTreeNode.Tag.GetType() != typeof(Disk))
                throw new ArgumentException("Invalid Treenode.  Treenode Tag must contain a Migration Disk object.");

            Disk targetDisk = (Disk)targetTreeNode.Tag;

            if (targetDisk.Source != null)
            {
                TreeNode diskParentNode = targetTreeNode.Parent;

                // Move the disk treenode to be a child under the resource group, not under the VM, as we are making it a Managed Disk
                if (targetTreeNode.Parent.Tag != null && targetTreeNode.Parent.Tag.GetType() == typeof(ResourceGroup))
                {
                    // Seek the Virtual Machine Tree Node
                    TreeNode virtualMachineNode = SeekARMChildTreeNode(targetTreeNode.Parent.Nodes, targetDisk.ParentVirtualMachine.ToString(), targetDisk.ParentVirtualMachine.ToString(), targetDisk.ParentVirtualMachine, false);

                    if (virtualMachineNode != null)
                    {
                        treeTargetARM.Nodes.Remove(targetTreeNode);
                        virtualMachineNode.Nodes.Add(targetTreeNode);
                        treeTargetARM.SelectedNode = targetTreeNode;
                    }
                }
            }
        }

        internal List<Arm.StorageAccount> GetExistingArmStorageAccounts()
        {
            List<Arm.StorageAccount> existingStorageAccounts = new List<Arm.StorageAccount>();

            if (this.TargetResourceGroup != null && this.TargetResourceGroup.TargetLocation != null && this.TargetSubscription != null)
            {
                foreach (Arm.StorageAccount armStorageAccount in this.TargetSubscription.FilterArmStorageAccounts(this.TargetResourceGroup.TargetLocation))
                {
                    existingStorageAccounts.Add(armStorageAccount);
                }
            }

            return existingStorageAccounts;
        }

        private void SeekAlertSourceRecursive(object sourceObject, TreeNodeCollection nodes)
        {
            foreach (TreeNode treeNode in nodes)
            {
                if (treeNode.Tag != null)
                {
                    object nodeObject = null;

                    if (treeNode.Tag.GetType() == typeof(TreeNode))
                    {
                        TreeNode asmTreeNode = (TreeNode)treeNode.Tag;
                        nodeObject = asmTreeNode.Tag;
                    }
                    else
                    {
                        nodeObject = treeNode.Tag;
                    }

                    // Note, this could probably be object compares, but was written this was to get it done.  Possible future change to object compares
                    if (nodeObject != null && sourceObject != null)
                    {
                        if (nodeObject.GetType() == sourceObject.GetType() && sourceObject.ToString() == nodeObject.ToString())
                                treeTargetARM.SelectedNode = treeNode;
                    }
                }

                SeekAlertSourceRecursive(sourceObject, treeNode.Nodes);
            }
        }

        public TreeNode SeekARMChildTreeNode(string name, string text, object tag, bool allowCreated = false)
        {
            return SeekARMChildTreeNode(this.treeTargetARM.Nodes, name, text, tag, allowCreated);
        }

        public TreeNode SeekARMChildTreeNode(TreeNodeCollection nodeCollection, string name, string text, object tag, bool allowCreated = false)
        {
            TreeNode[] childNodeMatch = nodeCollection.Find(name, true);

            foreach (TreeNode matchedNode in childNodeMatch)
            {
                if (matchedNode.Tag != null)
                {
                    if (matchedNode.Tag.GetType() == tag.GetType() && matchedNode.Text == text && matchedNode.Name == name)
                        return matchedNode;
                }
            }

            TreeNode childNode = null;
            if (allowCreated)
            {
                childNode = new TreeNode(text);
                childNode.Name = name;
                childNode.Text = name;
                childNode.Tag = tag;

                Core.MigrationTarget migrationTarget = (Core.MigrationTarget)tag;
                childNode.ImageKey = migrationTarget.ImageKey;
                childNode.SelectedImageKey = migrationTarget.ImageKey;

                nodeCollection.Add(childNode);
                childNode.ExpandAll();
                return childNode;
            }
            return null;
        }

        private TreeNode SeekResourceGroupTreeNode()
        {
            TreeNode targetResourceGroupNode = SeekARMChildTreeNode(treeTargetARM.Nodes, _TargetResourceGroup.ToString(), _TargetResourceGroup.ToString(), _TargetResourceGroup, true);
            return targetResourceGroupNode;
        }

        public async Task<TreeNode> AddMigrationTarget(Core.MigrationTarget migrationTarget)
        {
            if (migrationTarget == null)
                throw new ArgumentNullException("Migration Target cannot be null.");

            if (migrationTarget.ApiVersion == null || migrationTarget.ApiVersion == String.Empty)
            {
                if (this.TargetSubscription != null)
                {
                    migrationTarget.DefaultApiVersion(this.TargetSubscription);
                }
            }

            TreeNode targetResourceGroupNode = SeekResourceGroupTreeNode();

            if (migrationTarget.GetType() == typeof(VirtualNetworkGateway))
            {
                VirtualNetworkGateway targetVirtualNetworkGateway = (VirtualNetworkGateway)migrationTarget;
                TreeNode virtualNetworkGatewayNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetVirtualNetworkGateway.ToString(), targetVirtualNetworkGateway.ToString(), targetVirtualNetworkGateway, true);
                
                targetResourceGroupNode.ExpandAll();
                return virtualNetworkGatewayNode;
            }
            else if (migrationTarget.GetType() == typeof(VirtualNetwork))
            {
                VirtualNetwork targetVirtualNetwork = (VirtualNetwork)migrationTarget;
                TreeNode virtualNetworkNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetVirtualNetwork.ToString(), targetVirtualNetwork.ToString(), targetVirtualNetwork, true);

                foreach (Subnet targetSubnet in targetVirtualNetwork.TargetSubnets)
                {
                    TreeNode subnetNode = SeekARMChildTreeNode(virtualNetworkNode.Nodes, targetSubnet.ToString(), targetSubnet.ToString(), targetSubnet, true);
                }

                targetResourceGroupNode.ExpandAll();
                return virtualNetworkNode;
            }
            else if (migrationTarget.GetType() == typeof(LocalNetworkGateway))
            {
                LocalNetworkGateway targetLocalNetworkGateway = (LocalNetworkGateway)migrationTarget;
                TreeNode localNetworkGatewayNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetLocalNetworkGateway.ToString(), targetLocalNetworkGateway.ToString(), targetLocalNetworkGateway, true);

                targetResourceGroupNode.ExpandAll();
                return localNetworkGatewayNode;
            }
            else if (migrationTarget.GetType() == typeof(VirtualNetworkGatewayConnection))
            {
                VirtualNetworkGatewayConnection targetConnection = (VirtualNetworkGatewayConnection)migrationTarget;
                TreeNode connectionNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetConnection.ToString(), targetConnection.ToString(), targetConnection, true);

                targetResourceGroupNode.ExpandAll();
                return connectionNode;
            }
            else if (migrationTarget.GetType() == typeof(StorageAccount))
            {
                StorageAccount targetStorageAccount = (StorageAccount)migrationTarget;

                TreeNode storageAccountNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetStorageAccount.ToString(), targetStorageAccount.ToString(), targetStorageAccount, true);

                targetResourceGroupNode.ExpandAll();
                return storageAccountNode;
            }
            else if (migrationTarget.GetType() == typeof(NetworkSecurityGroup))
            {
                NetworkSecurityGroup targetNetworkSecurityGroup = (NetworkSecurityGroup)migrationTarget;
                TreeNode networkSecurityGroupNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetNetworkSecurityGroup.ToString(), targetNetworkSecurityGroup.ToString(), targetNetworkSecurityGroup, true);

                targetResourceGroupNode.ExpandAll();
                return networkSecurityGroupNode;
            }
            else if (migrationTarget.GetType() == typeof(ApplicationSecurityGroup))
            {
                ApplicationSecurityGroup targetApplicationSecurityGroup = (ApplicationSecurityGroup)migrationTarget;
                TreeNode applicationSecurityGroupNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetApplicationSecurityGroup.ToString(), targetApplicationSecurityGroup.ToString(), targetApplicationSecurityGroup, true);

                targetResourceGroupNode.ExpandAll();
                return applicationSecurityGroupNode;
            }
            else if (migrationTarget.GetType() == typeof(LoadBalancer))
            {
                LoadBalancer targetLoadBalancer = (LoadBalancer)migrationTarget;
                TreeNode targetLoadBalancerNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetLoadBalancer.ToString(), targetLoadBalancer.ToString(), targetLoadBalancer, true);

                targetResourceGroupNode.ExpandAll();
                return targetLoadBalancerNode;
            }
            else if (migrationTarget.GetType() == typeof(PublicIp))
            {
                PublicIp targetPublicIp = (PublicIp)migrationTarget;
                TreeNode targetPublicIpNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetPublicIp.ToString(), targetPublicIp.ToString(), targetPublicIp, true);

                targetResourceGroupNode.ExpandAll();
                return targetPublicIpNode;
            }
            else if (migrationTarget.GetType() == typeof(NetworkInterface))
            {
                NetworkInterface targetNetworkInterface = (NetworkInterface)migrationTarget;
                TreeNode targetNetworkInterfaceNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetNetworkInterface.ToString(), targetNetworkInterface.ToString(), targetNetworkInterface, true);

                targetResourceGroupNode.ExpandAll();
                return targetNetworkInterfaceNode;
            }
            else if (migrationTarget.GetType() == typeof(AvailabilitySet))
            {
                AvailabilitySet targetAvailabilitySet = (AvailabilitySet)migrationTarget;
                TreeNode targetAvailabilitySetNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetAvailabilitySet.ToString(), targetAvailabilitySet.ToString(), targetAvailabilitySet, true);

                targetResourceGroupNode.ExpandAll();
                return targetAvailabilitySetNode;
            }
            else if (migrationTarget.GetType() == typeof(VirtualMachine))
            {
                VirtualMachine targetVirtualMachine = (VirtualMachine)migrationTarget;

                TreeNode virtualMachineParentNode = targetResourceGroupNode;
                TreeNode virtualMachineNode = SeekARMChildTreeNode(virtualMachineParentNode.Nodes, targetVirtualMachine.ToString(), targetVirtualMachine.ToString(), targetVirtualMachine, true);

                if (targetVirtualMachine.TargetAvailabilitySet != null)
                {
                    if (targetVirtualMachine.TargetAvailabilitySet.Source != null && targetVirtualMachine.TargetAvailabilitySet.Source.GetType() == typeof(Asm.CloudService))
                    {
                        // Adding under Virtual Machine, as it is not a managed disk
                        TreeNode dataDiskNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetVirtualMachine.TargetAvailabilitySet.ToString(), targetVirtualMachine.TargetAvailabilitySet.ToString(), targetVirtualMachine.TargetAvailabilitySet, true);
                    }
                }

                if (targetVirtualMachine.OSVirtualHardDisk != null)
                {
                    if (targetVirtualMachine.OSVirtualHardDisk.IsUnmanagedDisk)
                    {
                        // Adding under Virtual Machine, as it is not a managed disk
                        TreeNode osDiskNode = SeekARMChildTreeNode(virtualMachineNode.Nodes, targetVirtualMachine.OSVirtualHardDisk.ToString(), targetVirtualMachine.OSVirtualHardDisk.ToString(), targetVirtualMachine.OSVirtualHardDisk, true);
                    }
                    else
                    {
                        // Under Resource Group, as it is a managed Disk
                        TreeNode osDiskNode = SeekARMChildTreeNode(virtualMachineParentNode.Nodes, targetVirtualMachine.OSVirtualHardDisk.ToString(), targetVirtualMachine.OSVirtualHardDisk.ToString(), targetVirtualMachine.OSVirtualHardDisk, true);
                    }
                }

                foreach (Disk targetDisk in targetVirtualMachine.DataDisks)
                {
                    if (targetDisk.IsUnmanagedDisk)
                    {
                        // Adding under Virtual Machine, as it is not a managed disk
                        TreeNode dataDiskNode = SeekARMChildTreeNode(virtualMachineNode.Nodes, targetDisk.ToString(), targetDisk.ToString(), targetDisk, true);
                    }
                    else
                    {
                        // Under Resource Group, as it is a managed Disk
                        TreeNode dataDiskNode = SeekARMChildTreeNode(virtualMachineParentNode.Nodes, targetDisk.ToString(), targetDisk.ToString(), targetDisk, true);
                    }
                }

                foreach (NetworkInterface targetNetworkInterface in targetVirtualMachine.NetworkInterfaces)
                {
                    if (targetNetworkInterface.Source != null && targetNetworkInterface.Source.GetType() == typeof(Azure.Asm.NetworkInterface))
                    {
                        // We are only adding as a child node if it is an ASM Network Interface, otherwise we expect this to follow ARM convention in which NIC is a first class object in the resource group (not embededded under the VM).
                        TreeNode networkInterfaceNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetNetworkInterface.ToString(), targetNetworkInterface.ToString(), targetNetworkInterface, true);
                    }
                }

                targetResourceGroupNode.ExpandAll();
                return virtualMachineNode;
            }
            else if (migrationTarget.GetType() == typeof(Disk))
            {
                Disk targetDisk = (Disk)migrationTarget;
                TreeNode targetDiskNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetDisk.ToString(), targetDisk.ToString(), targetDisk, true);

                targetResourceGroupNode.ExpandAll();
                return targetDiskNode;
            }
            //else if (parentNode.GetType() == typeof(VirtualMachineImage))
            //{
            //    VirtualMachineImage targetVirtualMachineImage = (VirtualMachineImage)parentNode;
            //    TreeNode targetVirtualMachineImageNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetVirtualMachineImage.SourceName, targetVirtualMachineImage.ToString(), targetVirtualMachineImage, true);

            //    targetResourceGroupNode.ExpandAll();
            //    return targetVirtualMachineImageNode;
            //}
            else if (migrationTarget.GetType() == typeof(NetworkInterface))
            {
                NetworkInterface targetNetworkInterface = (NetworkInterface)migrationTarget;
                TreeNode targetNetworkInterfaceNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetNetworkInterface.SourceName, targetNetworkInterface.ToString(), targetNetworkInterface, true);

                targetResourceGroupNode.ExpandAll();
                return targetNetworkInterfaceNode;
            }
            else if (migrationTarget.GetType() == typeof(RouteTable))
            {
                RouteTable targetRouteTable = (RouteTable)migrationTarget;
                TreeNode targetRouteTableNode = SeekARMChildTreeNode(targetResourceGroupNode.Nodes, targetRouteTable.ToString(), targetRouteTable.ToString(), targetRouteTable, true);

                targetRouteTableNode.ExpandAll();
                return targetRouteTableNode;
            }
            else
                throw new Exception("Unhandled Node Type in AddMigrationTargetToTargetTree: " + migrationTarget.GetType());

        }

        private TreeNode GetTargetAvailabilitySetNode(TreeNode subscriptionNode, AvailabilitySet targetAvailabilitySet)
        {
            foreach (TreeNode treeNode in subscriptionNode.Nodes)
            {
                if (treeNode.Tag != null)
                {
                    if (treeNode.Tag.GetType() == typeof(AvailabilitySet) && treeNode.Text == targetAvailabilitySet.ToString())
                        return treeNode;
                }
            }

            TreeNode tnAvailabilitySet = new TreeNode(targetAvailabilitySet.ToString());
            tnAvailabilitySet.Text = targetAvailabilitySet.ToString();
            tnAvailabilitySet.Tag = targetAvailabilitySet;
            tnAvailabilitySet.ImageKey = "AvailabilitySet";
            tnAvailabilitySet.SelectedImageKey = "AvailabilitySet";

            subscriptionNode.Nodes.Add(tnAvailabilitySet);
            tnAvailabilitySet.Expand();
            return tnAvailabilitySet;
        }

        public TreeNode SeekMigrationTargetTreeNode(Core.MigrationTarget migrationTarget)
        {
            return SeekMigrationTargetTreeNode(this.treeTargetARM.Nodes, migrationTarget);
        }

        public static TreeNode SeekMigrationTargetTreeNode(TreeNodeCollection treeNodeCollection, Core.MigrationTarget migrationTarget)
        {
            foreach (TreeNode treeNode in treeNodeCollection)
            {
                TreeNode matchingNode = SeekMigrationTargetTreeNodeRecursive(treeNode, migrationTarget);
                if (matchingNode != null)
                    return matchingNode;
            }

            return null;
        }

        private static TreeNode SeekMigrationTargetTreeNodeRecursive(TreeNode treeNode, Core.MigrationTarget migrationTarget)
        {
            if (treeNode.Tag != null && treeNode.Tag.GetType().BaseType == typeof(Core.MigrationTarget))
            {
                Core.MigrationTarget treeNodeMigrationTarget = (Core.MigrationTarget)treeNode.Tag;
                if (treeNodeMigrationTarget == migrationTarget)
                    return treeNode;
            }

            foreach (TreeNode treeNodeChild in treeNode.Nodes)
            {
                TreeNode treeNodeChildMatch = SeekMigrationTargetTreeNodeRecursive(treeNodeChild, migrationTarget);
                if (treeNodeChildMatch != null)
                    return treeNodeChildMatch;
            }

            return null;
        }

        public async Task RemoveMigrationTarget(Core.MigrationTarget migrationTarget)
        {
            TreeNode matchingNode = SeekMigrationTargetTreeNode(migrationTarget);
            if (matchingNode != null)
            {
                Core.MigrationTarget matchingNodeMigrationTarget = (Core.MigrationTarget)matchingNode.Tag;
                if (matchingNodeMigrationTarget == migrationTarget)
                {
                    if (matchingNode.Tag.GetType() == typeof(VirtualMachine))
                    {
                        await RemoveTreeNodeCascadeUp(matchingNode);

                        VirtualMachine targetVirtualMachine = (VirtualMachine)matchingNode.Tag;

                        // if there are VMs that have originating Classic Disks, we may need to clean them up from the Resource Group if they were being moved to a Managed Disk (not under the VM in the Treeview)
                        await RemoveAsmDiskTurnedManagedDiskFromARMTree(targetVirtualMachine.OSVirtualHardDisk);
                        foreach (MigrationTarget.Disk disk in targetVirtualMachine.DataDisks)
                        {
                            await RemoveAsmDiskTurnedManagedDiskFromARMTree(disk);
                        }
                        foreach (MigrationTarget.NetworkInterface networkInterface in targetVirtualMachine.NetworkInterfaces)
                        {
                            await RemoveAsmNetworkTurnedArmNetworkFromARMTree(networkInterface);
                        }
                        await RemoveCloudServiceTurnedAvailabilitySetFromARMTree(targetVirtualMachine.TargetAvailabilitySet);
                    }
                    else if (matchingNode.Tag.GetType() == migrationTarget.GetType())
                        await RemoveTreeNodeCascadeUp(matchingNode);
                    else if (matchingNode.Tag.GetType() == typeof(TreeNode))
                    {
                        TreeNode childTreeNode = (TreeNode)matchingNode.Tag;
                        if (migrationTarget.GetType() == childTreeNode.Tag.GetType())
                            await RemoveTreeNodeCascadeUp(matchingNode);
                    }
                }
            }
        }

        private async Task RemoveAsmDiskTurnedManagedDiskFromARMTree(Disk disk)
        {
            if (disk.Source != null)
            {
                if (disk.Source.GetType() == typeof(Azure.Asm.Disk) || disk.Source.GetType() == typeof(Azure.Arm.ClassicDisk))
                {
                    TreeNode targetResourceGroupNode = SeekResourceGroupTreeNode();
                    if (targetResourceGroupNode != null)
                    {
                        TreeNode[] matchingNodes = targetResourceGroupNode.Nodes.Find(disk.TargetName, true);
                        foreach (TreeNode matchingNode in matchingNodes)
                        {
                            if (matchingNode.Tag != null && matchingNode.Tag.GetType() == typeof(Disk) && String.Compare(((Disk)matchingNode.Tag).SourceName, disk.SourceName, true) == 0)
                            {
                                await RemoveTreeNodeCascadeUp(matchingNode);
                            }
                        }
                    }
                }
            }
        }

        private async Task RemoveAsmNetworkTurnedArmNetworkFromARMTree(NetworkInterface networkInterface)
        {
            if (networkInterface.Source != null && networkInterface.Source.GetType() == typeof(Azure.Asm.NetworkInterface))
            {
                TreeNode targetResourceGroupNode = SeekResourceGroupTreeNode();
                if (targetResourceGroupNode != null)
                {
                    TreeNode[] matchingNodes = targetResourceGroupNode.Nodes.Find(networkInterface.ToString(), true);
                    foreach (TreeNode matchingNode in matchingNodes)
                    {
                        if (matchingNode.Tag != null && matchingNode.Tag.GetType() == typeof(NetworkInterface) && String.Compare(((NetworkInterface)matchingNode.Tag).SourceName, networkInterface.SourceName, true) == 0)
                        {
                            await RemoveTreeNodeCascadeUp(matchingNode);
                        }
                    }
                }
            }
        }

        private async Task RemoveCloudServiceTurnedAvailabilitySetFromARMTree(AvailabilitySet availabilitySet)
        {
            if (availabilitySet != null)
            {
                if (availabilitySet.Source != null && availabilitySet.Source.GetType() == typeof(Azure.Asm.CloudService))
                {
                    TreeNode targetResourceGroupNode = SeekResourceGroupTreeNode();
                    if (targetResourceGroupNode != null)
                    {
                        TreeNode[] matchingNodes = targetResourceGroupNode.Nodes.Find(availabilitySet.ToString(), true);
                        foreach (TreeNode matchingNode in matchingNodes)
                        {
                            if (matchingNode.Tag != null && matchingNode.Tag.GetType() == typeof(AvailabilitySet) && String.Compare(((AvailabilitySet)matchingNode.Tag).SourceName, availabilitySet.SourceName, true) == 0)
                            {
                                await RemoveTreeNodeCascadeUp(matchingNode);
                            }
                        }
                    }
                }
            }
        }

        private async Task RemoveTreeNodeCascadeUp(TreeNode treeNode)
        {
            TreeNode parentNode = treeNode.Parent;
            treeNode.Remove();
            await RemoveParentWhileNoChildren(parentNode);
        }

        private async Task RemoveParentWhileNoChildren(TreeNode treeNode)
        {
            if (treeNode != null)
            {
                if (treeNode.Nodes.Count == 0)
                {
                    TreeNode parentNode = treeNode.Parent;
                    treeNode.Remove();
                    await RemoveParentWhileNoChildren(parentNode);
                }
            }
        }

        public void SeekAlertSource(object sourceObject)
        {
            SeekAlertSourceRecursive(sourceObject, treeTargetARM.Nodes);
        }


        private void TargetTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AfterTargetSelected?.Invoke(this, e.Node);
        }

        private void TargetTreeView_Resize(object sender, EventArgs e)
        {
            treeTargetARM.Width = this.Width - 10;
            treeTargetARM.Height = this.Height - 40;
        }

        public List<VirtualNetwork> GetVirtualNetworksInMigration()
        {
            List<VirtualNetwork> _TargetVirtualNetworks = new List<MigrationTarget.VirtualNetwork>();

            TreeNode targetResourceGroupNode = this.ResourceGroupNode;

            if (targetResourceGroupNode != null)
            {
                foreach (TreeNode treeNode in targetResourceGroupNode.Nodes)
                {
                    if (treeNode.Tag != null && treeNode.Tag.GetType() == typeof(VirtualNetwork))
                    {
                        VirtualNetwork targetVirtualNetwork = (VirtualNetwork)treeNode.Tag;
                        _TargetVirtualNetworks.Add(targetVirtualNetwork);
                    }
                }
            }

            return _TargetVirtualNetworks;
        }
        public List<PublicIp> GetPublicIPsInMigration()
        {
            List<PublicIp> _TargetPublicIPs = new List<MigrationTarget.PublicIp>();

            TreeNode targetResourceGroupNode = this.ResourceGroupNode;

            if (targetResourceGroupNode != null)
            {
                foreach (TreeNode treeNode in targetResourceGroupNode.Nodes)
                {
                    if (treeNode.Tag != null && treeNode.Tag.GetType() == typeof(PublicIp))
                    {
                        PublicIp targetPublicIp = (PublicIp)treeNode.Tag;
                        _TargetPublicIPs.Add(targetPublicIp);
                    }
                }
            }

            return _TargetPublicIPs;
        }

        public List<StorageAccount> GetStorageAccountsInMigration()
        {
            List<StorageAccount> _TargetStorageaccounts = new List<StorageAccount>();

            TreeNode targetResourceGroupNode = this.ResourceGroupNode;

            if (targetResourceGroupNode != null)
            {
                foreach (TreeNode treeNode in targetResourceGroupNode.Nodes)
                {
                    if (treeNode.Tag != null && treeNode.Tag.GetType() == typeof(StorageAccount))
                    {
                        StorageAccount targetStorageAccount = (StorageAccount)treeNode.Tag;
                        _TargetStorageaccounts.Add(targetStorageAccount);
                    }
                }
            }

            return _TargetStorageaccounts;
        }

        public void Clear()
        {
            treeTargetARM.Nodes.Clear();
        }

        public List<MigAzGeneratorAlert> Alerts
        {
            get
            {
                if (_ExportArtifacts == null)
                    return new List<MigAzGeneratorAlert>();
                else
                    return _ExportArtifacts.Alerts;
            }
        }

        public bool HasErrors
        {
            get
            {
                if (_ExportArtifacts == null)
                    return false;
                else
                    return _ExportArtifacts.HasErrors;
            }
        }

        public TargetSettings TargetSettings
        {
            get { return _TargetSettings; }
            set
            {
                _TargetSettings = value;
                if (value != null)
                    if (_TargetResourceGroup == null)
                        _TargetResourceGroup = new ResourceGroup(this.TargetSubscription, value, this.LogProvider);
            }
        }

        public AzureSubscription TargetSubscription
        {
            get;
            set;
        }

        public static int GetChildNodeCount(TreeNode treeNode)
        {
            int childCount = 0;

            foreach (TreeNode childNode in treeNode.Nodes)
            {
                childCount += GetChildNodeCountRecursive(childNode);
            }

            return childCount;
        }

        private static int GetChildNodeCountRecursive(TreeNode treeNode)
        {
            int childCount = 0;

            childCount += 1; // Count this node
            foreach (TreeNode childNode in treeNode.Nodes)
            {
                childCount += GetChildNodeCountRecursive(childNode);
            }

            return childCount;
        }

 
        private async Task RemoveNodeStartingWithLastChildrenBackUpTree(TreeView treeTargetARM, TreeNode selectedNode)
        {
            if (selectedNode != null)
            {
                foreach (TreeNode childNode in selectedNode.Nodes)
                {
                    await RemoveNodeStartingWithLastChildrenBackUpTree(treeTargetARM, childNode);
                }

                treeTargetARM.Nodes.Remove(selectedNode);

                AfterSourceNodeRemoved?.Invoke(this, selectedNode);
            }
        }

        
        private async Task AddNewMigrationTargetResource(Core.MigrationTarget migrationTarget)
        {
            migrationTarget.SetTargetName("New" + migrationTarget.FriendlyObjectName.Replace(" ", ""), this.TargetSettings);

            // Code has been slightly strucutured with anticipate of need for reuse across different object types
            int counter = 0;
            bool nameExists = true;

            while (nameExists)
            {
                if (counter > 0)
                    migrationTarget.SetTargetName("New" + migrationTarget.FriendlyObjectName.Replace(" ", "") + counter.ToString(), this.TargetSettings);

                TreeNode[] matchingNodes = treeTargetARM.Nodes.Find(migrationTarget.ToString(), true);
                nameExists = matchingNodes.Count() > 0;
                counter++;
            }

            TreeNode newNode = await this.AddMigrationTarget(migrationTarget);

            await this.RefreshExportArtifacts();
            AfterNewTargetResourceAdded?.Invoke(this, newNode);
        }

        #region treeTargetARM TreeView Events

        private async void treeTargetARM_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (treeTargetARM.SelectedNode != null)
                {
                    Core.MigrationTarget migrationTarget = (Core.MigrationTarget)treeTargetARM.SelectedNode.Tag;

                    int countChildNodes = GetChildNodeCount(treeTargetARM.SelectedNode);
                    String strChildNodeCount = String.Empty;
                    if (countChildNodes > 0)
                        strChildNodeCount = " **AND** " + countChildNodes.ToString() + " child resource(s)";

                    string deleteConfirmationText = String.Format("Are you sure you want to remove {0} '{1}'{2} as a target resource?", new string[] { migrationTarget.FriendlyObjectName, migrationTarget.ToString(), strChildNodeCount });
                    if (MessageBox.Show(deleteConfirmationText, "Remove Target Resource(s)", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        await RemoveNodeStartingWithLastChildrenBackUpTree(treeTargetARM, treeTargetARM.SelectedNode);
                    }
                }
            }
        }
        private async void treeTargetARM_DragDrop(object sender, DragEventArgs e)
        {
            string methodName = "treeTargetARM_DragDrop";

            this.LogProvider.WriteLog(methodName, "Start");

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(pt);
                TreeNode sourceNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");

                if (sourceNode != null && sourceNode.Tag != null && sourceNode.Tag.GetType().BaseType == typeof(Core.MigrationTarget) && destinationNode != null && destinationNode.Tag != null && destinationNode.Tag.GetType().BaseType == typeof(Core.MigrationTarget))
                {
                    bool isMigAzTargetDragDropHandled = false;
                    Core.MigrationTarget sourceNodeTarget = (Core.MigrationTarget)sourceNode.Tag;
                    Core.MigrationTarget destinationNodeTarget = (Core.MigrationTarget)destinationNode.Tag;
                    this.LogProvider.WriteLog(methodName, "Source Node Tag - Name '" + sourceNodeTarget.ToString() + "' Type '" + sourceNodeTarget.GetType() + "'");
                    this.LogProvider.WriteLog(methodName, "Target Node Tag - Name '" + destinationNodeTarget.ToString() + "' Type '" + destinationNodeTarget.GetType() + "'");

                    if (sourceNodeTarget.GetType() == typeof(VirtualMachine))
                    {
                        VirtualMachine virtualMachine = (VirtualMachine)sourceNodeTarget;

                        if (destinationNodeTarget.GetType() == typeof(AvailabilitySet))
                        {
                            AvailabilitySet availabilitySet = (AvailabilitySet)destinationNodeTarget;

                            // Update Virtual Machine to reflect ownership in new target Availability Set
                            virtualMachine.TargetAvailabilitySet = availabilitySet;

                            isMigAzTargetDragDropHandled = true;
                        }
                    }
                    else if (sourceNodeTarget.GetType() == typeof(PublicIp))
                    {
                        PublicIp publicIp = (PublicIp)sourceNodeTarget;

                        if (destinationNodeTarget.GetType() == typeof(NetworkInterface))
                        {
                            NetworkInterface networkInterface = (NetworkInterface)destinationNodeTarget;

                            // Update IP Configuration to reflect use of Public IP Address
                            if (networkInterface.TargetNetworkInterfaceIpConfigurations.Count > 0)
                                networkInterface.TargetNetworkInterfaceIpConfigurations[0].TargetPublicIp = publicIp;

                            isMigAzTargetDragDropHandled = true;
                        }
                        else if (destinationNodeTarget.GetType() == typeof(LoadBalancer))
                        {
                            LoadBalancer loadBalancer = (LoadBalancer)destinationNodeTarget;

                            if (loadBalancer.LoadBalancerType == LoadBalancerType.Internal)
                            {
                                if (MessageBox.Show(String.Format("Load Balancer is currently an Internal Load Balancer and must be changed to a Public Load Balancer to utilize Public IP '{0}'.  ", publicIp.ToString()), "Convert to Public Load Balancer", MessageBoxButtons.OKCancel) == DialogResult.OK)
                                {
                                    loadBalancer.LoadBalancerType = LoadBalancerType.Public;

                                    if (loadBalancer.FrontEndIpConfigurations.Count == 0)
                                        loadBalancer.FrontEndIpConfigurations.Add(new FrontEndIpConfiguration(loadBalancer));
                                }
                            }

                            // Update IP Configuration to reflect use of Public IP Address
                            if (loadBalancer.LoadBalancerType == LoadBalancerType.Public)
                            {
                                if (loadBalancer.FrontEndIpConfigurations.Count > 0)
                                    loadBalancer.FrontEndIpConfigurations[0].PublicIp = publicIp;
                            }

                            isMigAzTargetDragDropHandled = true;
                        }
                    }

                    if (!isMigAzTargetDragDropHandled)
                        this.LogProvider.WriteLog(methodName, "Drag from " + sourceNodeTarget.GetType().ToString() + " to " + destinationNodeTarget.GetType().ToString() + " was not handled.  No DragDrop action taken.");
                    else
                        await this.RefreshExportArtifacts();

                }
                else
                {
                    if (sourceNode == null)
                        this.LogProvider.WriteLog(methodName, "SourceNode is null.  No DragDrop action taken.");
                    else if (sourceNode.Tag == null)
                        this.LogProvider.WriteLog(methodName, "SourceNode exists, but Tag is null.  No DragDrop action taken.");
                    else if (sourceNode.Tag != null)
                        this.LogProvider.WriteLog(methodName, "SourceNode exists, Tag is not null, but not of type IMigrationTarget.  No DragDrop action taken.");

                    if (destinationNode == null)
                        this.LogProvider.WriteLog(methodName, "DestinationNode is null.  No DragDrop action taken.");
                    else if (destinationNode.Tag == null)
                        this.LogProvider.WriteLog(methodName, "DestinationNode exists, but Tag is null.  No DragDrop action taken.");
                    else if (destinationNode.Tag != null)
                        this.LogProvider.WriteLog(methodName, "DestinationNode exists, Tag is not null, but not of type IMigrationTarget.  No DragDrop action taken.");
                }
            }

            this.LogProvider.WriteLog(methodName, "End");
        }

        private void treeTargetARM_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treeTargetARM_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeTargetARM_MouseClick(object sender, MouseEventArgs e)
        {
            // https://support.microsoft.com/en-us/help/810001/how-to-display-a-context-menu-that-is-specific-to-a-selected-treeview
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                TreeNode node = treeTargetARM.GetNodeAt(p);
                if (node != null)
                {
                    // Select the node the user has clicked.
                    // The node appears selected until the menu is displayed on the screen.
                    treeTargetARM.SelectedNode = node;
                    
                    if (node.Tag != null)
                    {
                        if (node.Tag.GetType() == typeof(ResourceGroup))
                        {
                            resourceGroupMenuStrip.Show(treeTargetARM, p);
                        }
                        else if (node.Tag.GetType() == typeof(VirtualNetwork))
                        {
                            virtualNetworkMenuStrip.Show(treeTargetARM, p);
                        }
                    }
                }
            }
        }

        #endregion

        #region Resource Group Context Menu Events

        private async void newAvailabilitySetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await AddNewMigrationTargetResource(new AvailabilitySet());
        }

        private async void newLoadBalancerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await AddNewMigrationTargetResource(new LoadBalancer());
        }

        private async void newPublicIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await AddNewMigrationTargetResource(new PublicIp());
        }

        private async void newStorageAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StorageAccount storageAccount = new StorageAccount();
            storageAccount.BlobStorageNamespace = this.TargetBlobStorageNamespace;
            await AddNewMigrationTargetResource(storageAccount);
        }

        private async void newVirtualNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await AddNewMigrationTargetResource(new VirtualNetwork());
        }

        #endregion

        #region Virtual Network Context Menu Events

        private void NewSubnetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VirtualNetwork virtualNetwork = (VirtualNetwork)treeTargetARM.SelectedNode.Tag;
            Subnet subnet = new Subnet(this.TargetSubscription, virtualNetwork, this.TargetSettings, this.LogProvider);
            TreeNode subnetNode = SeekARMChildTreeNode(treeTargetARM.SelectedNode.Nodes, subnet.ToString(), subnet.ToString(), subnet, true);
            treeTargetARM.SelectedNode = subnetNode;
        }

        private void removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #endregion
    }
}

