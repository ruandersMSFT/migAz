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
using Newtonsoft.Json.Linq;

namespace MigAz.Azure.UserControls
{
    public partial class MigrationAzureSourceContext : UserControl, IMigrationSourceUserControl
    {
        private AzureContext _AzureContextSource;
        private List<TreeNode> _SelectedNodes = new List<TreeNode>();
        private TreeNode _SourceAsmNode;
        private TargetSettings _TargetSettings;
        private ImageList _ImageList;
        private ArmDiskType _DefaultTargetDiskType = ArmDiskType.ManagedDisk;
        private bool _AutoSelectDependencies = true;
        private bool _IsAuthenticated = false;
        private ILogProvider _LogProvider;
        private IStatusProvider _StatusProvider;

        #region Matching Events from AzureContext

        public delegate Task BeforeAzureTenantChangedHandler(AzureContext sender);
        public event BeforeAzureTenantChangedHandler BeforeAzureTenantChange;

        public delegate Task AfterAzureTenantChangedHandler(AzureContext sender);
        public event AfterAzureTenantChangedHandler AfterAzureTenantChange;

        public delegate Task BeforeAzureSubscriptionChangedHandler(AzureContext sender);
        public event BeforeAzureSubscriptionChangedHandler BeforeAzureSubscriptionChange;

        public delegate Task AfterAzureSubscriptionChangedHandler(AzureContext sender);
        public event AfterAzureSubscriptionChangedHandler AfterAzureSubscriptionChange;

        public delegate Task AzureEnvironmentChangedHandler(AzureContext sender);
        public event AzureEnvironmentChangedHandler AzureEnvironmentChanged;

        public delegate Task UserAuthenticatedHandler(AzureContext sender);
        public event UserAuthenticatedHandler UserAuthenticated;

        public delegate Task BeforeUserSignOutHandler();
        public event BeforeUserSignOutHandler BeforeUserSignOut;

        public delegate Task AfterUserSignOutHandler();
        public event AfterUserSignOutHandler AfterUserSignOut;

        #endregion

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

        #endregion

        #region Constructor(s)

        public MigrationAzureSourceContext()
        {
            InitializeComponent();
            cmbAzureResourceTypeSource.SelectedIndex = 0;

            treeAzureASM.Left = 0;
            treeAzureASM.Width = this.Width;
            treeViewSourceResourceManager1.Left = 0;
            treeViewSourceResourceManager1.Width = this.Width;
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

        public String MigrationSourceType
        {
            get
            {
                return cmbAzureResourceTypeSource.SelectedItem.ToString();
            }
        }
        public AzureContext AzureContext
        {
            get { return _AzureContextSource; }
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

        public void Bind(AzureRetriever azureRetriever, IStatusProvider statusProvider, ILogProvider logProvider, TargetSettings targetSettings, ImageList imageList, PromptBehavior promptBehavior, List<AzureEnvironment> azureEnvironments, ref List<AzureEnvironment> userDefinedAzureEnvironments)
        {
            _TargetSettings = targetSettings;
            _LogProvider = logProvider;
            _StatusProvider = statusProvider;
            _ImageList = imageList;

            _AzureContextSource = new AzureContext(azureRetriever, targetSettings, promptBehavior);
            _AzureContextSource.AzureEnvironmentChanged += _AzureContext_AzureEnvironmentChanged;
            _AzureContextSource.UserAuthenticated += _AzureContext_UserAuthenticated;
            _AzureContextSource.BeforeAzureSubscriptionChange += _AzureContext_BeforeAzureSubscriptionChange;
            _AzureContextSource.AfterAzureSubscriptionChange += _AzureContext_AfterAzureSubscriptionChange;
            _AzureContextSource.BeforeUserSignOut += _AzureContext_BeforeUserSignOut;
            _AzureContextSource.AfterUserSignOut += _AzureContext_AfterUserSignOut;
            _AzureContextSource.AfterAzureTenantChange += _AzureContext_AfterAzureTenantChange;
            _AzureContextSource.BeforeAzureTenantChange += _AzureContextSource_BeforeAzureTenantChange;
            azureLoginContextViewerSource.AfterContextChanged += AzureLoginContextViewerSource_AfterContextChanged;

            azureLoginContextViewerSource.Bind(_AzureContextSource, azureRetriever, azureEnvironments, ref userDefinedAzureEnvironments);
            treeViewSourceResourceManager1.Bind(logProvider, statusProvider, targetSettings, imageList, promptBehavior);
        }

        private void ResetForm()
        {
            if (treeAzureASM != null)
            {
                treeAzureASM.Enabled = false;
                treeAzureASM.Nodes.Clear();
            }

            if (treeViewSourceResourceManager1 != null)
            {
                treeViewSourceResourceManager1.Enabled = false;
                treeViewSourceResourceManager1.ResetForm();
            }

            if (_SelectedNodes != null)
                _SelectedNodes.Clear();

            ClearContext?.Invoke();
        }

        #endregion

        #region Event Handlers

        private async Task AzureLoginContextViewerSource_AfterContextChanged(AzureLoginContextViewer sender)
        {
            AfterContextChanged?.Invoke(this);
        }


        private async Task _AzureContextSource_BeforeAzureTenantChange(AzureContext sender)
        {
            BeforeAzureTenantChange?.Invoke(sender);
        }

        private async Task _AzureContext_AfterAzureTenantChange(AzureContext sender)
        {
            AfterAzureTenantChange?.Invoke(sender);
        }

        private async Task _AzureContext_BeforeAzureSubscriptionChange(AzureContext sender)
        {
            BeforeAzureSubscriptionChange?.Invoke(sender);
        }

        private async Task _AzureContext_AzureEnvironmentChanged(AzureContext sender)
        {
            AzureEnvironmentChanged?.Invoke(sender);
        }


        private async Task _AzureContext_UserAuthenticated(AzureContext sender)
        {
            this.IsSourceContextAuthenticated = true;
            UserAuthenticated?.Invoke(sender);
        }

        private async Task _AzureContext_BeforeUserSignOut()
        {
            await BeforeUserSignOut?.Invoke();
        }

        private async Task _AzureContext_AfterUserSignOut()
        {
            ResetForm();
            this.IsSourceContextAuthenticated = false;

            await AfterUserSignOut?.Invoke();
        }

        private async Task _AzureContext_AfterAzureSubscriptionChange(AzureContext sender)
        {
            try
            {
                ResetForm();

                if (sender.AzureSubscription != null)
                {
                    switch (cmbAzureResourceTypeSource.SelectedItem.ToString())
                    {
                        case "Azure Service Management (ASM / Classic)":
                            await BindAsmResources(sender, _TargetSettings);
                            break;
                        case "Azure Resource Manager (ARM)":
                            treeViewSourceResourceManager1.Enabled = true;
                            treeViewSourceResourceManager1.Visible = true;
                            await sender.AzureSubscription.InitializeChildrenAsync();
                            await sender.AzureSubscription.BindArmResources(_TargetSettings);
                            await treeViewSourceResourceManager1.BindArmResources(sender, sender.AzureSubscription, _TargetSettings);
                            break;
                        default:
                            throw new ArgumentException("Unexpected Source Resource Tab: " + cmbAzureResourceTypeSource.SelectedValue);
                    }

                    _AzureContextSource.AzureRetriever.SaveRestCache();
                }
            }
            catch (Exception exc)
            {
                UnhandledExceptionDialog unhandledException = new UnhandledExceptionDialog(_AzureContextSource.LogProvider, exc);
                unhandledException.ShowDialog();
            }

            _AzureContextSource.StatusProvider.UpdateStatus("Ready");

            AfterAzureSubscriptionChange?.Invoke(sender);
        }


        private async Task BindAsmResources(AzureContext azureContext, TargetSettings targetSettings)
        {
            treeAzureASM.Nodes.Clear();

            try
            {
                if (_AzureContextSource != null && _AzureContextSource.AzureSubscription != null)
                {

                    await _AzureContextSource.AzureSubscription.BindAsmResources(targetSettings);

                    if (_AzureContextSource != null && _AzureContextSource.AzureSubscription != null)
                    {
                        TreeNode subscriptionNodeASM = new TreeNode(_AzureContextSource.AzureSubscription.Name);
                        treeAzureASM.Nodes.Add(subscriptionNodeASM);
                        subscriptionNodeASM.Expand();

                        foreach (Azure.MigrationTarget.NetworkSecurityGroup targetNetworkSecurityGroup in _AzureContextSource.AzureSubscription.AsmTargetNetworkSecurityGroups)
                        {
                            Azure.Asm.NetworkSecurityGroup asmNetworkSecurityGroup = (Azure.Asm.NetworkSecurityGroup)targetNetworkSecurityGroup.Source;
                            TreeNode parentNode = GetDataCenterTreeViewNode(subscriptionNodeASM, asmNetworkSecurityGroup.Location, "Network Security Groups");

                            TreeNode tnNetworkSecurityGroup = new TreeNode(targetNetworkSecurityGroup.SourceName);
                            tnNetworkSecurityGroup.Name = targetNetworkSecurityGroup.SourceName;
                            tnNetworkSecurityGroup.Tag = targetNetworkSecurityGroup;
                            parentNode.Nodes.Add(tnNetworkSecurityGroup);
                            parentNode.Expand();
                        }

                        foreach (Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork in _AzureContextSource.AzureSubscription.AsmTargetVirtualNetworks)
                        {
                            Azure.Asm.VirtualNetwork asmVirtualNetwork = (Azure.Asm.VirtualNetwork)targetVirtualNetwork.Source;
                            TreeNode parentNode = GetDataCenterTreeViewNode(subscriptionNodeASM, asmVirtualNetwork.Location, "Virtual Networks");

                            TreeNode tnVirtualNetwork = new TreeNode(targetVirtualNetwork.SourceName);
                            tnVirtualNetwork.Name = targetVirtualNetwork.SourceName;
                            tnVirtualNetwork.Text = targetVirtualNetwork.SourceName;
                            tnVirtualNetwork.Tag = targetVirtualNetwork;
                            parentNode.Nodes.Add(tnVirtualNetwork);
                            parentNode.Expand();
                        }

                        foreach (Azure.MigrationTarget.StorageAccount targetStorageAccount in _AzureContextSource.AzureSubscription.AsmTargetStorageAccounts)
                        {
                            TreeNode parentNode = GetDataCenterTreeViewNode(subscriptionNodeASM, ((IStorageAccount)targetStorageAccount.Source).PrimaryLocation, "Storage Accounts");

                            TreeNode tnStorageAccount = new TreeNode(targetStorageAccount.SourceName);
                            tnStorageAccount.Name = targetStorageAccount.SourceName;
                            tnStorageAccount.Tag = targetStorageAccount;
                            parentNode.Nodes.Add(tnStorageAccount);
                            parentNode.Expand();
                        }

                        foreach (Azure.MigrationTarget.VirtualMachine targetVirtualMachine in _AzureContextSource.AzureSubscription.AsmTargetVirtualMachines)
                        {
                            Azure.Asm.VirtualMachine asmVirtualMachine = (Azure.Asm.VirtualMachine)targetVirtualMachine.Source;
                            TreeNode parentNode = GetDataCenterTreeViewNode(subscriptionNodeASM, asmVirtualMachine.Location, "Cloud Services");
                            TreeNode[] cloudServiceNodeSearch = parentNode.Nodes.Find(targetVirtualMachine.TargetAvailabilitySet.TargetName, false);
                            TreeNode cloudServiceNode = null;
                            if (cloudServiceNodeSearch.Count() == 1)
                            {
                                cloudServiceNode = cloudServiceNodeSearch[0];
                            }

                            cloudServiceNode = new TreeNode(targetVirtualMachine.TargetAvailabilitySet.TargetName);
                            cloudServiceNode.Name = targetVirtualMachine.TargetAvailabilitySet.TargetName;
                            cloudServiceNode.Tag = targetVirtualMachine.TargetAvailabilitySet;
                            parentNode.Nodes.Add(cloudServiceNode);
                            parentNode.Expand();

                            TreeNode virtualMachineNode = new TreeNode(targetVirtualMachine.SourceName);
                            virtualMachineNode.Name = targetVirtualMachine.SourceName;
                            virtualMachineNode.Tag = targetVirtualMachine;
                            cloudServiceNode.Nodes.Add(virtualMachineNode);
                            cloudServiceNode.Expand();

                            foreach (Azure.MigrationTarget.NetworkInterface targetNetworkInterface in targetVirtualMachine.NetworkInterfaces)
                            {
                                if (targetNetworkInterface.BackEndAddressPool != null && targetNetworkInterface.BackEndAddressPool.LoadBalancer != null)
                                {
                                    TreeNode loadBalancerNode = new TreeNode(targetNetworkInterface.BackEndAddressPool.LoadBalancer.SourceName);
                                    loadBalancerNode.Name = targetNetworkInterface.BackEndAddressPool.LoadBalancer.SourceName;
                                    loadBalancerNode.Tag = targetNetworkInterface.BackEndAddressPool.LoadBalancer;
                                    cloudServiceNode.Nodes.Add(loadBalancerNode);
                                    cloudServiceNode.Expand();

                                    foreach (Azure.MigrationTarget.FrontEndIpConfiguration frontEnd in targetNetworkInterface.BackEndAddressPool.LoadBalancer.FrontEndIpConfigurations)
                                    {
                                        if (frontEnd.PublicIp != null && frontEnd.PublicIp.GetType() == typeof(MigrationTarget.PublicIp)) // if external load balancer
                                        {
                                            MigrationTarget.PublicIp targetPublicIp = (MigrationTarget.PublicIp)frontEnd.PublicIp;

                                            TreeNode publicIPAddressNode = new TreeNode(targetPublicIp.SourceName);
                                            publicIPAddressNode.Name = targetPublicIp.SourceName;
                                            publicIPAddressNode.Tag = targetPublicIp;
                                            cloudServiceNode.Nodes.Add(publicIPAddressNode);
                                            cloudServiceNode.Expand();
                                        }
                                    }
                                }
                            }
                        }

                        subscriptionNodeASM.Expand();
                        treeAzureASM.Enabled = true;
                    }
                }
            }
            catch (Exception exc)
            {
                if (exc.GetType() == typeof(System.Net.WebException))
                {
                    System.Net.WebException webException = (System.Net.WebException)exc;
                    if (webException.Response != null)
                    {
                        HttpWebResponse exceptionResponse = (HttpWebResponse)webException.Response;
                        if (exceptionResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            ASM403ForbiddenExceptionDialog forbiddenDialog = new ASM403ForbiddenExceptionDialog(_AzureContextSource.LogProvider, exc);
                            return;
                        }
                    }
                }

                UnhandledExceptionDialog exceptionDialog = new UnhandledExceptionDialog(_AzureContextSource.LogProvider, exc);
                exceptionDialog.ShowDialog();
            }

            _AzureContextSource.StatusProvider.UpdateStatus("Ready");
        }

        #endregion

        #region Source Resource TreeView Methods

        private async Task SelectDependencies(TreeNode selectedNode)
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
                        if (targetVirtualMachine.Source.GetType() == typeof(Azure.Asm.VirtualMachine))
                        {
                            Azure.Asm.VirtualMachine asmVirtualMachine = (Azure.Asm.VirtualMachine)targetVirtualMachine.Source;

                            #region process virtual network

                            foreach (Azure.MigrationTarget.NetworkInterface networkInterface in targetVirtualMachine.NetworkInterfaces)
                            {
                                #region Auto Select Virtual Network from each IpConfiguration

                                // This source code to auto select the Virtual Network as a Parent node has been commented out purposefully.
                                // It was observed through use of MigAz that users were not aware that "including the Virtual Network" in multiple 
                                // migrations was actually creating new / independent / non-connected versions of the same Virtual Network.
                                // It is valid that the user would want to migrate the Virtual Network during the first run migration; however, beyond
                                // that first pass (the Azure ARM Virtual Network exists) the user is more likely to need to migrate the Virtual Machine(s)
                                // into an existing Azure Virtual Network.  In order to guide the user in this direction, we do not want to auto select the 
                                // source Virtual Network to be included in the Migration Template as a new Virtual Network.  We want the user to explicitly
                                // select and include the source Azure Virtual Network into the Migration Template (if they want to include it), or utilize
                                // the built in property editor dialogs to "select an existing Virtual Network and Subnet".

                                //foreach (Azure.MigrationTarget.NetworkInterfaceIpConfiguration ipConfiguration in networkInterface.TargetNetworkInterfaceIpConfigurations)
                                //{
                                //    if (ipConfiguration.TargetVirtualNetwork != null && ipConfiguration.TargetVirtualNetwork.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork))
                                //    {
                                //        Azure.MigrationTarget.VirtualNetwork targetVirtualNetwork = (Azure.MigrationTarget.VirtualNetwork)ipConfiguration.TargetVirtualNetwork;
                                //        foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(targetVirtualNetwork.SourceName, true))
                                //        {
                                //            if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.VirtualNetwork)))
                                //            {
                                //                if (!treeNode.Checked)
                                //                    treeNode.Checked = true;
                                //            }
                                //        }
                                //    }
                                //}

                                #endregion

                                #region Auto Select Network Security Group

                                if (asmVirtualMachine.NetworkSecurityGroup != null)
                                {
                                    foreach (TreeNode treeNode in selectedNode.TreeView.Nodes.Find(asmVirtualMachine.NetworkSecurityGroup.Name, true))
                                    {
                                        if ((treeNode.Tag != null) && (treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.NetworkSecurityGroup)))
                                        {
                                            if (!treeNode.Checked)
                                                treeNode.Checked = true;
                                        }
                                    }
                                }
                                #endregion
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
                            //    if (targetSubnet.NetworkSecurityGroup.Source.GetType() == typeof(Azure.Asm.NetworkSecurityGroup))
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

                _AzureContextSource.StatusProvider.UpdateStatus("Ready");
            }
        }


        private async void treeAzureASM_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_SourceAsmNode == null)
            {
                _SourceAsmNode = e.Node;
            }

            if (e.Node.Checked)
                await SelectDependencies(e.Node);

            TreeNode resultUpdateARMTree = null;

            Core.MigrationTarget migrationTarget = null;
            if (e.Node.Tag != null && e.Node.Tag.GetType().BaseType == typeof(Core.MigrationTarget))
            {
                migrationTarget = (Core.MigrationTarget)e.Node.Tag;

                if (e.Node.Checked)
                {
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
                    treeAzureASM.SelectedNode = e.Node;
                }
                else
                {
                    await RecursiveCheckToggleUp(e.Node, e.Node.Checked);
                    await RecursiveCheckToggleDown(e.Node, e.Node.Checked);
                }

                _SelectedNodes = this.GetSelectedNodes(treeAzureASM);

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


        private async void cmbAzureResourceTypeSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeAzureASM.Nodes.Clear();
            treeViewSourceResourceManager1.ResetForm();

            switch (cmbAzureResourceTypeSource.SelectedItem.ToString())
            {
                case "Azure Service Management (ASM / Classic)":
                    treeAzureASM.Enabled = true;
                    treeAzureASM.Visible = true;
                    treeViewSourceResourceManager1.Enabled = false;
                    treeViewSourceResourceManager1.Visible = false;

                    await BindAsmResources(_AzureContextSource, _TargetSettings);

                    break;
                case "Azure Resource Manager (ARM)":
                    treeAzureASM.Enabled = false;
                    treeAzureASM.Visible = false;
                    treeViewSourceResourceManager1.Enabled = true;
                    treeViewSourceResourceManager1.Visible = true;

                    if (_TargetSettings != null)
                        await treeViewSourceResourceManager1.BindArmResources(_AzureContextSource, _AzureContextSource.AzureSubscription, _TargetSettings);

                    break;
                default:
                    throw new ArgumentException("Unknown Azure Resource Type Source: " + cmbAzureResourceTypeSource.SelectedItem.ToString());

            }

            ClearContext?.Invoke();
        }

        private void MigrationSourceAzure_Resize(object sender, EventArgs e)
        {
            treeViewSourceResourceManager1.Height = this.Height - 135;
            treeViewSourceResourceManager1.Width = this.Width;
            treeAzureASM.Height = this.Height - 135;
            treeAzureASM.Width = this.Width;
            azureLoginContextViewerSource.Width = this.Width;
        }

        public async Task UncheckMigrationTarget(Core.MigrationTarget migrationTarget)
        {
            LogProvider.WriteLog("UncheckMigrationTarget", "Start");

            if (treeViewSourceResourceManager1.Enabled)
            {
                LogProvider.WriteLog("UncheckMigrationTarget", "Seeking Originating MigrationTarget TreeNode in treeViewSourceResourceManager1");
                await treeViewSourceResourceManager1.UncheckMigrationTarget(migrationTarget);
            }
            else if (treeAzureASM.Enabled)
            {
                TreeNode sourceMigrationNode = null;

                LogProvider.WriteLog("UncheckMigrationTarget", "Seeking Originating MigrationTarget TreeNode in treeAzureASM");
                sourceMigrationNode = TargetTreeView.SeekMigrationTargetTreeNode(treeAzureASM.Nodes, migrationTarget);

                if (sourceMigrationNode == null)
                {
                    LogProvider.WriteLog("UncheckMigrationTarget", "Originating MigrationTarget TreeNode not found.");
                }
                else
                {
                    LogProvider.WriteLog("UncheckMigrationTarget", "Seeking Originating MigrationTarget TreeNode");
                    sourceMigrationNode.Checked = false;
                }
            }

            LogProvider.WriteLog("UncheckMigrationTarget", "End");
        }

        private async Task treeViewSourceResourceManager1_AfterNodeChecked(Core.MigrationTarget sender)
        {
            await AfterNodeChecked?.Invoke(sender);

            if (_StatusProvider != null)
                _StatusProvider.UpdateStatus("Ready");
        }

        private async Task treeViewSourceResourceManager1_AfterNodeUnchecked(Core.MigrationTarget sender)
        {
            await AfterNodeUnchecked?.Invoke(sender);

            if (_StatusProvider != null)
                _StatusProvider.UpdateStatus("Ready");
        }

        private async Task treeViewSourceResourceManager1_AfterNodeChanged(Core.MigrationTarget sender)
        {
            await AfterNodeChanged?.Invoke(sender);

            if (_StatusProvider != null)
                _StatusProvider.UpdateStatus("Ready");
        }

        private void treeViewSourceResourceManager1_ClearContext()
        {
            ClearContext?.Invoke();

            if (_StatusProvider != null)
                _StatusProvider.UpdateStatus("Ready");
        }

        private void treeViewSourceResourceManager1_AfterContextChanged(UserControl sender)
        {
            AfterContextChanged?.Invoke(sender);

            if (_StatusProvider != null)
                _StatusProvider.UpdateStatus("Ready");
        }

        private async void treeViewSourceResourceManager1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Tag.GetType() == typeof(MigrationTarget.ResourceGroup) && e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading")
            {
                e.Node.Nodes.Clear();

                MigrationTarget.ResourceGroup targetResourceGroup = (MigrationTarget.ResourceGroup)e.Node.Tag;
                if (targetResourceGroup.Source != null)
                {
                    JObject resourceGroupResourcesJson = await _AzureContextSource.AzureSubscription.GetAzureARMResources("*", (Arm.ResourceGroup)targetResourceGroup.Source, null);

                    if (resourceGroupResourcesJson != null)
                    {
                        var armResources = from managedDisk in resourceGroupResourcesJson["value"]
                                           select managedDisk;

                        List<Task> resourceGroupTasks = new List<Task>();

                        foreach (var armResource in armResources)
                        {
                            switch (armResource["type"].ToString())
                            {
                                case "Microsoft.Compute/availabilitySets":
                                case "Microsoft.Compute/virtualMachines":
                                case "Microsoft.Compute/disks":
                                case "Microsoft.Storage/storageAccounts":
                                case "Microsoft.Network/applicationSecurityGroups":
                                case "Microsoft.Network/networkInterfaces":
                                case "Microsoft.Network/virtualNetworks":
                                case "Microsoft.Network/routeTables":
                                case "Microsoft.Network/networkSecurityGroups":
                                case "Microsoft.Network/publicIPAddresses":
                                case "Microsoft.Network/virtualNetworkGateways":
                                case "Microsoft.Network/localNetworkGateways":
                                case "Microsoft.Network/connections":
                                case "Microsoft.Network/loadBalancers":
                                    resourceGroupTasks.Add(LoadResourceGroupTreeNode(armResource, e.Node));
                                    break;

                                default:
                                    break;
                            }

                            
                        }

                        await Task.WhenAll(resourceGroupTasks);
                    }
                }

                e.Node.Expand();
            }

            if (_StatusProvider != null)
                _StatusProvider.UpdateStatus("Ready");
        }

        public async Task LoadResourceGroupTreeNode(JToken armResource, TreeNode sourceTreeNode)
        {
            Core.MigrationTarget migrationTarget = null;

            migrationTarget = await SeekMigrationTarget(armResource);

            if (migrationTarget != null)
            {
                TreeNode treeNode = new TreeNode();
                treeNode.Text = migrationTarget.SourceName;
                treeNode.Name = migrationTarget.SourceName;
                treeNode.Tag = migrationTarget;
                treeNode.ImageKey = migrationTarget.ImageKey;
                treeNode.SelectedImageKey = migrationTarget.ImageKey;

                sourceTreeNode.Nodes.Add(treeNode);
            }
        }

        private async Task<Core.MigrationTarget> SeekMigrationTarget(JToken armResourceToken)
        {
            Arm.ArmResource armResource = await this.AzureContext.AzureSubscription.SeekResourceById(armResourceToken);
            return this.AzureContext.AzureSubscription.SeekMigrationTargetBySource(armResource);
        }
    }
}

