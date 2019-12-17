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
using MigAz.Azure.Interface;
using System.Net;
using MigAz.Azure.Arm;
using MigAz.Azure.Forms;
using MigAz.Azure.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MigAz.Azure.UserControls
{
    public partial class TreeViewSourceResourceManager : UserControl
    {
        //private AzureContext _AzureContextSource;
        private List<TreeNode> _SelectedNodes = new List<TreeNode>();
        private TreeNode _SourceAsmNode;
        private TargetSettings _TargetSettings;
        private ImageList _ImageList;
        private ArmDiskType _DefaultTargetDiskType = ArmDiskType.ManagedDisk;
        private bool _AutoSelectDependencies = true;
        private bool _IsAuthenticated = false;
        private ILogProvider _LogProvider;
        private IStatusProvider _StatusProvider;

        #region New Events from MigrationSourceAzure

        public delegate Task AfterNodeCheckedHandler(Core.MigrationTarget sender);
        public event AfterNodeCheckedHandler AfterNodeChecked;

        public delegate Task AfterNodeUncheckedHandler(Core.MigrationTarget sender);
        public event AfterNodeUncheckedHandler AfterNodeUnchecked;

        public delegate Task AfterNodeChangedHandler(Core.MigrationTarget sender);
        public event AfterNodeChangedHandler AfterNodeChanged;

        public delegate void ClearContextHandler();
        public event ClearContextHandler ClearContext;

        public delegate void AfterContextChangedHandler(UserControl sender);
        public event AfterContextChangedHandler AfterContextChanged;

        public delegate void BeforeExpandChangedHandler(object sender, TreeViewCancelEventArgs e);
        public event BeforeExpandChangedHandler BeforeExpand;

        #endregion

        #region Constructor(s)

        public TreeViewSourceResourceManager()
        {
            InitializeComponent();
            treeAzureARM.Width = this.Width;
            treeAzureARM.Height = this.Height;

        }

        #endregion

        #region Properties

        public ILogProvider LogProvider
        {
            get { return _LogProvider; }
        }

        public bool IsSourceContextAuthenticated
        {
            get { return _IsAuthenticated; }
            set { _IsAuthenticated = value; }
        }

        public ArmDiskType DefaultTargetDiskType
        {
            get { return _DefaultTargetDiskType; }
            set { _DefaultTargetDiskType = value; }
        }

        public bool AutoSelectDependencies
        {
            get { return _AutoSelectDependencies; }
            set { _AutoSelectDependencies = value; }
        }

        #endregion

        #region Methods

        public void Bind(ILogProvider logProvider, IStatusProvider statusProvider, TargetSettings targetSettings, ImageList imageList, PromptBehavior promptBehavior)
        {
            _TargetSettings = targetSettings;
            _LogProvider = logProvider;
            _StatusProvider = statusProvider;
            _ImageList = imageList;

            treeAzureARM.ImageList = _ImageList;
        }

        public void ResetForm()
        {
            treeAzureARM.Enabled = false;
            treeAzureARM.Nodes.Clear();

            if (_SelectedNodes != null)
                _SelectedNodes.Clear();

            ClearContext?.Invoke();
        }

        #endregion

        #region Event Handlers
        
        public async Task BindArmResources(AzureContext azureContext, AzureSubscription azureSubscription, TargetSettings targetSettings)
        {
            treeAzureARM.Nodes.Clear();

            try
            {
                if (azureSubscription != null)
                {
                    await azureSubscription.BindArmResources(targetSettings);

                    if (azureSubscription != null)
                    {
                        TreeNode subscriptionNodeARM = new TreeNode(azureSubscription.Name);
                        subscriptionNodeARM.ImageKey = "Subscription";
                        subscriptionNodeARM.SelectedImageKey = "Subscription";
                        treeAzureARM.Nodes.Add(subscriptionNodeARM);
                        subscriptionNodeARM.Expand();

                        foreach (MigrationTarget.ResourceGroup resourceGroup in azureSubscription.TargetResources.Where(x => x.GetType() == typeof(MigrationTarget.ResourceGroup)))
                        {
                            TreeNode resourceGroupNode = GetResourceGroupTreeNode(subscriptionNodeARM, resourceGroup);
                            resourceGroupNode.Nodes.Add("Loading");
                            resourceGroupNode.Collapse();
                        }

                        subscriptionNodeARM.Expand();
                        treeAzureARM.Sort();
                        treeAzureARM.Enabled = true;
                    }
                }
            }
            catch (Exception exc)
            {
                UnhandledExceptionDialog exceptionDialog = new UnhandledExceptionDialog(_LogProvider, exc);
                exceptionDialog.ShowDialog();
            }

            _StatusProvider.UpdateStatus("Ready");
        }

        #endregion

        #region Source Resource TreeView Methods

        private async Task AsmSelectDependencies(TreeNode selectedNode)
        {
            if (this.AutoSelectDependencies && (selectedNode.Checked) && (selectedNode.Tag != null))
            {
                if (selectedNode.Tag.GetType() == typeof(Azure.MigrationTarget.AvailabilitySet))
                {
                    Azure.MigrationTarget.AvailabilitySet targetAvailabilitySet = (Azure.MigrationTarget.AvailabilitySet)selectedNode.Tag;

                    foreach (Azure.MigrationTarget.VirtualMachine targetVirtualMachine in targetAvailabilitySet.TargetVirtualMachines)
                    {
                        foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(targetVirtualMachine.TargetName, true))
                        {
                            if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualMachine)))
                            {
                                if (!treeNode.Checked)
                                    treeNode.Checked = true;
                            }
                        }
                    }
                }
                if (selectedNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualMachine))
                {
                    Azure.MigrationTarget.VirtualMachine targetVirtualMachine = (Azure.MigrationTarget.VirtualMachine)selectedNode.Tag;

                    if (targetVirtualMachine.Source != null)
                    {
                        if (targetVirtualMachine.Source.GetType() == typeof(Azure.Arm.VirtualMachine))
                        {
                            Azure.Arm.VirtualMachine armVirtualMachine = (Azure.Arm.VirtualMachine)targetVirtualMachine.Source;

                            #region process virtual network

                            // This source code to auto select the Virtual Network as a Parent node has been commented out purposefully.
                            // It was observed through use of MigAz that users were not aware that "including the Virtual Network" in multiple 
                            // migrations was actually creating new / independent / non-connected versions of the same Virtual Network.
                            // It is valid that the user would want to migrate the Virtual Network during the first run migration; however, beyond
                            // that first pass (the Azure ARM Virtual Network exists) the user is more likely to need to migrate the Virtual Machine(s)
                            // into an existing Azure Virtual Network.  In order to guide the user in this direction, we do not want to auto select the 
                            // source Virtual Network to be included in the Migration Template as a new Virtual Network.  We want the user to explicitly
                            // select and include the source Azure Virtual Network into the Migration Template (if they want to include it), or utilize
                            // the built in property editor dialogs to "select an existing Virtual Network and Subnet".

                            //foreach (Azure.Arm.NetworkInterface networkInterface in armVirtualMachine.NetworkInterfaces)
                            //{
                            //    foreach (Azure.Arm.NetworkInterfaceIpConfiguration ipConfiguration in networkInterface.NetworkInterfaceIpConfigurations)
                            //    {
                            //        if (ipConfiguration.VirtualNetwork != null)
                            //        {
                            //            foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(ipConfiguration.VirtualNetwork.Name, true))
                            //            {
                            //                if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork)))
                            //                {
                            //                    if (!treeNode.Checked)
                            //                        treeNode.Checked = true;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}

                            #endregion

                            #region process managed disks

                            if (armVirtualMachine.OSVirtualHardDisk != null && armVirtualMachine.OSVirtualHardDisk.GetType() == typeof(Azure.Arm.ManagedDisk))
                            {
                                foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(((Azure.Arm.ManagedDisk)armVirtualMachine.OSVirtualHardDisk).Name, true))
                                {
                                    if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.Disk)))
                                    {
                                        if (!treeNode.Checked)
                                            treeNode.Checked = true;
                                    }
                                }
                            }

                            foreach (Azure.Interface.IArmDisk dataDisk in armVirtualMachine.DataDisks)
                            {
                                if (dataDisk.GetType() == typeof(Azure.Arm.ManagedDisk))
                                {
                                    Azure.Arm.ManagedDisk managedDisk = (Azure.Arm.ManagedDisk)dataDisk;

                                    foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(managedDisk.Name, true))
                                    {
                                        if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.Disk)))
                                        {
                                            if (!treeNode.Checked)
                                                treeNode.Checked = true;
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region OS Disk Storage Account

                            if (armVirtualMachine.OSVirtualHardDisk != null && armVirtualMachine.OSVirtualHardDisk.GetType() == typeof(Azure.Arm.ClassicDisk)) // Disk in a Storage Account, not a Managed Disk
                            {
                                foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(((Azure.Arm.ClassicDisk)armVirtualMachine.OSVirtualHardDisk).StorageAccountName, true))
                                {
                                    if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.StorageAccount)))
                                    {
                                        if (!treeNode.Checked)
                                            treeNode.Checked = true;
                                    }
                                }
                            }

                            #endregion

                            #region Data Disk(s) Storage Account(s)

                            foreach (IArmDisk dataDisk in armVirtualMachine.DataDisks)
                            {
                                if (dataDisk.GetType() == typeof(Azure.Arm.ClassicDisk))
                                {
                                    Azure.Arm.ClassicDisk classicDisk = (Azure.Arm.ClassicDisk)dataDisk;
                                    foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(classicDisk.StorageAccountName, true))
                                    {
                                        if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.StorageAccount)))
                                        {
                                            if (!treeNode.Checked)
                                                treeNode.Checked = true;
                                        }
                                    }
                                }
                                else if (dataDisk.GetType() == typeof(Azure.Arm.ManagedDisk))
                                {
                                    Azure.Arm.ManagedDisk managedDisk = (Azure.Arm.ManagedDisk)dataDisk;
                                    foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(managedDisk.Name, true))
                                    {
                                        if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.Disk)))
                                        {
                                            if (!treeNode.Checked)
                                                treeNode.Checked = true;
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region Network Interface Card(s)

                            // todo now russell
                            //foreach (Azure.Arm.NetworkInterface networkInterface in armVirtualMachine.NetworkInterfaces)
                            //{
                            //    foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(networkInterface.Name, true))
                            //    {
                            //        if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.NetworkInterface)))
                            //        {
                            //            if (!treeNode.Checked)
                            //                treeNode.Checked = true;
                            //        }
                            //    }

                            //    if (networkInterface.NetworkSecurityGroup != null)
                            //    {
                            //        foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(networkInterface.NetworkSecurityGroup.Name, true))
                            //        {
                            //            if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.NetworkSecurityGroup)))
                            //            {
                            //                if (!treeNode.Checked)
                            //                    treeNode.Checked = true;
                            //            }
                            //        }
                            //    }

                            //    foreach (Azure.Arm.NetworkInterfaceIpConfiguration ipConfiguration in networkInterface.NetworkInterfaceIpConfigurations)
                            //    {
                            //        if (ipConfiguration.BackEndAddressPool != null && ipConfiguration.BackEndAddressPool.LoadBalancer != null)
                            //        {
                            //            foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(ipConfiguration.BackEndAddressPool.LoadBalancer.Name, true))
                            //            {
                            //                if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.LoadBalancer)))
                            //                {
                            //                    if (!treeNode.Checked)
                            //                        treeNode.Checked = true;
                            //                }
                            //            }
                            //        }

                            //        if (ipConfiguration.PublicIP != null)
                            //        {
                            //            foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(ipConfiguration.PublicIP.Name, true))
                            //            {
                            //                if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.PublicIp)))
                            //                {
                            //                    if (!treeNode.Checked)
                            //                        treeNode.Checked = true;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}

                            #endregion

                            #region Parent Availability Set

                            // Todo Test, this was changed from Name To Id, won't work as coded
                            if (armVirtualMachine.AvailabilitySetId != null && armVirtualMachine.AvailabilitySetId != String.Empty)
                            {
                                foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(armVirtualMachine.AvailabilitySetId, true))
                                {
                                    if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.AvailabilitySet)))
                                    {
                                        if (!treeNode.Checked)
                                            treeNode.Checked = true;
                                    }
                                }
                            }

                            #endregion
                        }
                    }
                }
                else if (selectedNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork))
                {
                    Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork = (Azure.MigrationTarget.VirtualNetwork)selectedNode.Tag;

                    foreach (Azure.MigrationTarget.Subnet targetSubnet in targetVirtualNetwork.TargetSubnets)
                    {
                        if (targetSubnet.NetworkSecurityGroup != null)
                        {
                            // todo now russell
                            //if (targetSubnet.NetworkSecurityGroup.Source != null)
                            //{
                            //    if (targetSubnet.NetworkSecurityGroup.Source.GetType() == typeof(Azure.Arm.NetworkSecurityGroup))
                            //    {
                            //        foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(targetSubnet.NetworkSecurityGroup.SourceName, true))
                            //        {
                            //            if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.NetworkSecurityGroup)))
                            //            {
                            //                if (!treeNode.Checked)
                            //                    treeNode.Checked = true;
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
                else if (selectedNode.Tag.GetType() == typeof(Azure.MigrationTarget.LoadBalancer))
                {
                    Azure.MigrationTarget.LoadBalancer targetLoadBalancer = (Azure.MigrationTarget.LoadBalancer)selectedNode.Tag;

                    if (targetLoadBalancer.Source != null)
                    {
                        if (targetLoadBalancer.Source.GetType() == typeof(Azure.Arm.LoadBalancer))
                        {
                            Azure.Arm.LoadBalancer armLoadBalaner = (Azure.Arm.LoadBalancer)targetLoadBalancer.Source;

                            foreach (Azure.Arm.FrontEndIpConfiguration frontEndIpConfiguration in armLoadBalaner.FrontEndIpConfigurations)
                            {
                                if (frontEndIpConfiguration.PublicIP != null)
                                {
                                    foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(frontEndIpConfiguration.PublicIP.Name, true))
                                    {
                                        if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.PublicIp)))
                                        {
                                            if (!treeNode.Checked)
                                                treeNode.Checked = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _StatusProvider.UpdateStatus("Ready");
            }
        }

        private void treeAzureARM_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            BeforeExpand?.Invoke(sender, e);
        }

        private async void treeAzureResourcesSource_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_SourceAsmNode == null)
            {
                _SourceAsmNode = e.Node;
            }

            if (e.Node.Checked)
                await AsmSelectDependencies(e.Node);

            TreeNode resultUpdateARMTree = null;
            
            Core.MigrationTarget migrationTarget = null;
            if (e.Node.Tag != null && e.Node.Tag.GetType().BaseType == typeof(Core.MigrationTarget))
            {
                migrationTarget = (Core.MigrationTarget)e.Node.Tag;

                if (e.Node.Checked)
                {
                    if (migrationTarget.Source != null)
                    {
                        if (migrationTarget.Source.GetType().BaseType == typeof(Arm.ArmResource))
                        {
                            Arm.ArmResource armResource = (Arm.ArmResource)migrationTarget.Source;
                            await armResource.RefreshToken();
                        }
                    }

                    resultUpdateARMTree = e.Node;
                    AfterNodeChecked?.Invoke(migrationTarget);
                }
                else
                {
                    AfterNodeUnchecked?.Invoke(migrationTarget);
                }
            }

            if (_SourceAsmNode != null && _SourceAsmNode == e.Node)
            {
                if (e.Node.Checked)
                {
                    await RecursiveCheckToggleDown(e.Node, e.Node.Checked);
                    FillUpIfFullDown(e.Node);
                    treeAzureARM.SelectedNode = e.Node;
                }
                else
                {
                    await RecursiveCheckToggleUp(e.Node, e.Node.Checked);
                    await RecursiveCheckToggleDown(e.Node, e.Node.Checked);
                }

                _SelectedNodes = this.GetSelectedNodes(treeAzureARM);

                _SourceAsmNode = null;

                AfterNodeChanged?.Invoke(migrationTarget);
            }
        }

        private List<TreeNode> GetSelectedNodes(TreeView treeView)
        {
            List<TreeNode> selectedNodes = new List<TreeNode>();
            foreach (TreeNode treeNode in treeView.Nodes)
            {
                RecursiveNodeSelectedAdd(ref selectedNodes, treeNode);
            }
            return selectedNodes;
        }


        private void RecursiveNodeSelectedAdd(ref List<TreeNode> selectedNodes, TreeNode parentNode)
        {
            if (parentNode.Checked && parentNode.Tag != null &&
                (parentNode.Tag.GetType() == typeof(Azure.MigrationTarget.NetworkSecurityGroup) ||
                parentNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork) ||
                parentNode.Tag.GetType() == typeof(Azure.MigrationTarget.StorageAccount) ||
                parentNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualMachine)
                ))
                selectedNodes.Add(parentNode);

            foreach (TreeNode childNode in parentNode.Nodes)
            {
                RecursiveNodeSelectedAdd(ref selectedNodes, childNode);
            }
        }

        #endregion

        private TreeNode GetDataCenterTreeViewNode(TreeNode subscriptionNode, string dataCenter, string containerName)
        {
            TreeNode dataCenterNode = null;

            foreach (TreeNode treeNode in subscriptionNode.Nodes)
            {
                if (treeNode.Text == dataCenter && treeNode.Tag.ToString() == "DataCenter")
                {
                    dataCenterNode = treeNode;

                    foreach (TreeNode dataCenterContainerNode in treeNode.Nodes)
                    {
                        if (dataCenterContainerNode.Text == containerName)
                            return dataCenterContainerNode;
                    }
                }
            }

            if (dataCenterNode == null)
            {
                dataCenterNode = new TreeNode(dataCenter);
                dataCenterNode.Tag = "DataCenter";
                subscriptionNode.Nodes.Add(dataCenterNode);
                dataCenterNode.Expand();
            }

            TreeNode containerNode = new TreeNode(containerName);
            dataCenterNode.Nodes.Add(containerNode);
            containerNode.Expand();

            return containerNode;
        }

        private TreeNode GetResourceGroupTreeNode(TreeNode subscriptionNode, IResourceGroup resourceGroup)
        {
            foreach (TreeNode treeNode in subscriptionNode.Nodes)
            {
                if (treeNode.Tag != null)
                {
                    if (treeNode.Tag.GetType() == resourceGroup.GetType() && treeNode.Text == resourceGroup.ToString())
                        return treeNode;
                }
            }

            TreeNode tnResourceGroup = new TreeNode(resourceGroup.ToString());
            tnResourceGroup.Text = resourceGroup.ToString();
            tnResourceGroup.Tag = resourceGroup;
            tnResourceGroup.ImageKey = "ResourceGroup";
            tnResourceGroup.SelectedImageKey = "ResourceGroup";

            subscriptionNode.Nodes.Add(tnResourceGroup);
            return tnResourceGroup;
        }


        private void FillUpIfFullDown(TreeNode node)
        {
            if (IsSelectedFullDown(node) && (node.Parent != null))
            {
                node = node.Parent;

                while (node != null)
                {
                    if (AllChildrenChecked(node))
                    {
                        node.Checked = true;
                        node = node.Parent;
                    }
                    else
                        node = null;
                }
            }
        }

        private bool AllChildrenChecked(TreeNode node)
        {
            foreach (TreeNode childNode in node.Nodes)
                if (!childNode.Checked)
                    return false;

            return true;
        }

        private bool IsSelectedFullDown(TreeNode node)
        {
            if (!node.Checked)
                return false;

            foreach (TreeNode childNode in node.Nodes)
            {
                if (!IsSelectedFullDown(childNode))
                    return false;
            }

            return true;
        }

        private async Task RecursiveCheckToggleDown(TreeNode node, bool isChecked)
        {
            if (node.Checked != isChecked)
            {
                node.Checked = isChecked;
            }

            foreach (TreeNode subNode in node.Nodes)
            {
                await RecursiveCheckToggleDown(subNode, isChecked);
            }
        }
        private async Task RecursiveCheckToggleUp(TreeNode node, bool isChecked)
        {
            if (node.Checked != isChecked)
            {
                node.Checked = isChecked;
            }

            if (node.Parent != null)
                await RecursiveCheckToggleUp(node.Parent, isChecked);
        }

        public async Task UncheckMigrationTarget(Core.MigrationTarget migrationTarget)
        {
            LogProvider.WriteLog("UncheckMigrationTarget", "Start");

            TreeNode sourceMigrationNode = null;
            
            if (treeAzureARM.Enabled)
            {
                LogProvider.WriteLog("UncheckMigrationTarget", "Seeking Originating MigrationTarget TreeNode in treeAzureARM");
                sourceMigrationNode = TargetTreeView.SeekMigrationTargetTreeNode(treeAzureARM.Nodes, migrationTarget);
            }

            if (sourceMigrationNode == null)
            {
                LogProvider.WriteLog("UncheckMigrationTarget", "Originating MigrationTarget TreeNode not found.");
            }
            else
            {
                LogProvider.WriteLog("UncheckMigrationTarget", "Seeking Originating MigrationTarget TreeNode");
                sourceMigrationNode.Checked = false;
            }

            LogProvider.WriteLog("UncheckMigrationTarget", "End");
        }

        private void TreeViewSourceResourceManager_Resize(object sender, EventArgs e)
        {
            treeAzureARM.Width = this.Width;
            treeAzureARM.Height = this.Height;
        }
    }
}

