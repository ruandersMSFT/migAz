// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MigAz.Azure.UserControls
{
    partial class TargetTreeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TargetTreeView));
            this.treeTargetARM = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.virtualNetworkMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.asdfasdfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resourceGroupMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newLoadBalancerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPublicIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newAvailabilitySetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newStorageAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newVirtualNetworkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.virtualNetworkMenuStrip.SuspendLayout();
            this.resourceGroupMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeTargetARM
            // 
            this.treeTargetARM.AllowDrop = true;
            this.treeTargetARM.Enabled = false;
            this.treeTargetARM.Location = new System.Drawing.Point(5, 45);
            this.treeTargetARM.Name = "treeTargetARM";
            this.treeTargetARM.Size = new System.Drawing.Size(128, 129);
            this.treeTargetARM.TabIndex = 58;
            this.treeTargetARM.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeTargetARM_ItemDrag);
            this.treeTargetARM.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TargetTreeView_AfterSelect);
            this.treeTargetARM.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeTargetARM_DragDrop);
            this.treeTargetARM.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeTargetARM_DragEnter);
            this.treeTargetARM.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeTargetARM_KeyUp);
            this.treeTargetARM.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeTargetARM_MouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 20);
            this.label1.TabIndex = 59;
            this.label1.Text = "Target Resource(s):";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ResourceGroup");
            this.imageList1.Images.SetKeyName(1, "Disk");
            this.imageList1.Images.SetKeyName(2, "LoadBalancer");
            this.imageList1.Images.SetKeyName(3, "NetworkInterface");
            this.imageList1.Images.SetKeyName(4, "NetworkSecurityGroup");
            this.imageList1.Images.SetKeyName(5, "PublicIp");
            this.imageList1.Images.SetKeyName(6, "StorageAccount");
            this.imageList1.Images.SetKeyName(7, "VirtualMachine");
            this.imageList1.Images.SetKeyName(8, "AvailabilitySet");
            this.imageList1.Images.SetKeyName(9, "VirtualNetwork");
            this.imageList1.Images.SetKeyName(10, "RouteTable");
            this.imageList1.Images.SetKeyName(11, "VirtualMachineImage");
            // 
            // virtualNetworkMenuStrip
            // 
            this.virtualNetworkMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.virtualNetworkMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asdfasdfToolStripMenuItem,
            this.toolStripMenuItem1,
            this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem});
            this.virtualNetworkMenuStrip.Name = "virtualNetworkMenuStrip";
            this.virtualNetworkMenuStrip.Size = new System.Drawing.Size(504, 70);
            // 
            // NewSubnetToolStripMenuItem
            // 
            this.asdfasdfToolStripMenuItem.Name = "NewSubnetToolStripMenuItem";
            this.asdfasdfToolStripMenuItem.Size = new System.Drawing.Size(503, 30);
            this.asdfasdfToolStripMenuItem.Text = "New &Subnet";
            this.asdfasdfToolStripMenuItem.Click += new System.EventHandler(this.NewSubnetToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(500, 6);
            // 
            // removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem
            // 
            this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem.Name = "removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem";
            this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem.Size = new System.Drawing.Size(503, 30);
            this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem.Text = "Remove Virtual Network from Target Resource Group";
            this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem.Click += new System.EventHandler(this.removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem_Click);
            // 
            // resourceGroupMenuStrip
            // 
            this.resourceGroupMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.resourceGroupMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem});
            this.resourceGroupMenuStrip.Name = "resourceGroupMenuStrip";
            this.resourceGroupMenuStrip.Size = new System.Drawing.Size(199, 67);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newAvailabilitySetToolStripMenuItem,
            this.newLoadBalancerToolStripMenuItem,
            this.newPublicIPToolStripMenuItem,
            this.newStorageAccountToolStripMenuItem,
            this.newVirtualNetworkToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(198, 30);
            this.newToolStripMenuItem.Text = "&New";
            // 
            // newLoadBalancerToolStripMenuItem
            // 
            this.newLoadBalancerToolStripMenuItem.Image = global::MigAz.Azure.Properties.Resources.LoadBalancer;
            this.newLoadBalancerToolStripMenuItem.Name = "newLoadBalancerToolStripMenuItem";
            this.newLoadBalancerToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.newLoadBalancerToolStripMenuItem.Text = "Load Balancer";
            this.newLoadBalancerToolStripMenuItem.Click += new System.EventHandler(this.newLoadBalancerToolStripMenuItem_Click);
            // 
            // newPublicIPToolStripMenuItem
            // 
            this.newPublicIPToolStripMenuItem.Image = global::MigAz.Azure.Properties.Resources.PublicIp;
            this.newPublicIPToolStripMenuItem.Name = "newPublicIPToolStripMenuItem";
            this.newPublicIPToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.newPublicIPToolStripMenuItem.Text = "Public IP";
            this.newPublicIPToolStripMenuItem.Click += new System.EventHandler(this.newPublicIPToolStripMenuItem_Click);
            // 
            // newAvailabilitySetToolStripMenuItem
            // 
            this.newAvailabilitySetToolStripMenuItem.Image = global::MigAz.Azure.Properties.Resources.AvailabilitySet;
            this.newAvailabilitySetToolStripMenuItem.Name = "newAvailabilitySetToolStripMenuItem";
            this.newAvailabilitySetToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.newAvailabilitySetToolStripMenuItem.Text = "Availability Set";
            this.newAvailabilitySetToolStripMenuItem.Click += new System.EventHandler(this.newAvailabilitySetToolStripMenuItem_Click);
            // 
            // newStorageAccountToolStripMenuItem
            // 
            this.newStorageAccountToolStripMenuItem.Image = global::MigAz.Azure.Properties.Resources.StorageAccount;
            this.newStorageAccountToolStripMenuItem.Name = "newStorageAccountToolStripMenuItem";
            this.newStorageAccountToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.newStorageAccountToolStripMenuItem.Text = "Storage Account";
            this.newStorageAccountToolStripMenuItem.Click += new System.EventHandler(this.newStorageAccountToolStripMenuItem_Click);
            // 
            // newVirtualNetworkToolStripMenuItem
            // 
            this.newVirtualNetworkToolStripMenuItem.Image = global::MigAz.Azure.Properties.Resources.VirtualNetwork;
            this.newVirtualNetworkToolStripMenuItem.Name = "newVirtualNetworkToolStripMenuItem";
            this.newVirtualNetworkToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.newVirtualNetworkToolStripMenuItem.Text = "Virtual Network";
            this.newVirtualNetworkToolStripMenuItem.Click += new System.EventHandler(this.newVirtualNetworkToolStripMenuItem_Click);
            // 
            // TargetTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeTargetARM);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TargetTreeView";
            this.Size = new System.Drawing.Size(378, 374);
            this.Resize += new System.EventHandler(this.TargetTreeView_Resize);
            this.virtualNetworkMenuStrip.ResumeLayout(false);
            this.resourceGroupMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeTargetARM;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip virtualNetworkMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem asdfasdfToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem removeVirtualNetworkFromTargetResourceGroupToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip resourceGroupMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newAvailabilitySetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newLoadBalancerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newPublicIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newStorageAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newVirtualNetworkToolStripMenuItem;
    }
}

