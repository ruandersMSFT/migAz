// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MigAz.Forms
{
    partial class MigAzForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MigAzForm));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.migAzMigrationSourceSelection1 = new MigAz.UserControls.MigAzMigrationSourceSelection();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.targetAzureContextViewer = new MigAz.Azure.UserControls.AzureLoginContextViewer();
            this.targetTreeView1 = new MigAz.Azure.UserControls.TargetTreeView();
            this.tabMigAzMonitoring = new System.Windows.Forms.TabControl();
            this.tabMessages = new System.Windows.Forms.TabPage();
            this.dgvMigAzMessages = new System.Windows.Forms.DataGridView();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lblLastOutputRefresh = new System.Windows.Forms.Label();
            this.btnRefreshOutput = new System.Windows.Forms.Button();
            this.tabOutputResults = new System.Windows.Forms.TabControl();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tabRest = new System.Windows.Forms.TabPage();
            this.txtRest = new System.Windows.Forms.TextBox();
            this.propertyPanel1 = new MigAz.Azure.UserControls.PropertyPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExport = new System.Windows.Forms.Button();
            this.txtDestinationFolder = new System.Windows.Forms.TextBox();
            this.btnChoosePath = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemAzureEnvironments = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visitMigAzOnGithubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportAnIssueOnGithubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.tabMigAzMonitoring.SuspendLayout();
            this.tabMessages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMigAzMessages)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.tabRest.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
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
            this.imageList1.Images.SetKeyName(12, "VirtualNetworkGateway");
            this.imageList1.Images.SetKeyName(13, "Connection");
            this.imageList1.Images.SetKeyName(14, "LocalNetworkGateway");
            this.imageList1.Images.SetKeyName(15, "ApplicationSecurityGroup");
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 481);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 7, 0);
            this.statusStrip1.Size = new System.Drawing.Size(958, 22);
            this.statusStrip1.TabIndex = 56;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel1.Text = "Ready";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(912, 17);
            this.toolStripStatusLabel2.Spring = true;
            this.toolStripStatusLabel2.Text = "http://aka.ms/MigAz";
            this.toolStripStatusLabel2.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyPanel1);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
            this.splitContainer1.Size = new System.Drawing.Size(958, 457);
            this.splitContainer1.SplitterDistance = 612;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 57;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.AutoScroll = true;
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            this.splitContainer2.Panel1.Resize += new System.EventHandler(this.splitContainer2_Panel1_Resize);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabMigAzMonitoring);
            this.splitContainer2.Panel2.Resize += new System.EventHandler(this.splitContainer2_Panel2_Resize);
            this.splitContainer2.Size = new System.Drawing.Size(612, 457);
            this.splitContainer2.SplitterDistance = 305;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.migAzMigrationSourceSelection1);
            this.splitContainer3.Panel1.Resize += new System.EventHandler(this.splitContainer3_Panel1_Resize);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Panel2.Resize += new System.EventHandler(this.splitContainer3_Panel2_Resize);
            this.splitContainer3.Size = new System.Drawing.Size(612, 305);
            this.splitContainer3.SplitterDistance = 292;
            this.splitContainer3.TabIndex = 0;
            // 
            // migAzMigrationSourceSelection1
            // 
            this.migAzMigrationSourceSelection1.Location = new System.Drawing.Point(2, 2);
            this.migAzMigrationSourceSelection1.Margin = new System.Windows.Forms.Padding(1);
            this.migAzMigrationSourceSelection1.Name = "migAzMigrationSourceSelection1";
            this.migAzMigrationSourceSelection1.Size = new System.Drawing.Size(241, 207);
            this.migAzMigrationSourceSelection1.TabIndex = 0;
            this.migAzMigrationSourceSelection1.AfterMigrationSourceSelected += new MigAz.UserControls.MigAzMigrationSourceSelection.AfterMigrationSourceSelectedHandler(this.migAzMigrationSourceSelection1_AfterMigrationSourceSelected);
            // 
            // splitContainer4
            // 
            this.splitContainer4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.targetAzureContextViewer);
            this.splitContainer4.Panel1.Resize += new System.EventHandler(this.splitContainer4_Panel1_Resize);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.targetTreeView1);
            this.splitContainer4.Panel2.Resize += new System.EventHandler(this.splitContainer4_Panel2_Resize);
            this.splitContainer4.Size = new System.Drawing.Size(316, 305);
            this.splitContainer4.SplitterDistance = 165;
            this.splitContainer4.SplitterWidth = 3;
            this.splitContainer4.TabIndex = 0;
            // 
            // targetAzureContextViewer
            // 
            this.targetAzureContextViewer.AzureContextSelectedType = MigAz.Azure.UserControls.AzureContextSelectedType.ExistingContext;
            this.targetAzureContextViewer.ChangeType = MigAz.Azure.UserControls.AzureLoginChangeType.NewOrExistingContext;
            this.targetAzureContextViewer.Enabled = false;
            this.targetAzureContextViewer.ExistingContext = null;
            this.targetAzureContextViewer.Location = new System.Drawing.Point(3, 3);
            this.targetAzureContextViewer.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.targetAzureContextViewer.Name = "targetAzureContextViewer";
            this.targetAzureContextViewer.Size = new System.Drawing.Size(447, 110);
            this.targetAzureContextViewer.TabIndex = 0;
            this.targetAzureContextViewer.Title = "Target Azure Subscription";
            this.targetAzureContextViewer.AfterContextChanged += new MigAz.Azure.UserControls.AzureLoginContextViewer.AfterContextChangedHandler(this.targetAzureContextViewer_AfterContextChanged);
            // 
            // targetTreeView1
            // 
            this.targetTreeView1.Enabled = false;
            this.targetTreeView1.ImageList = null;
            this.targetTreeView1.Location = new System.Drawing.Point(1, 1);
            this.targetTreeView1.LogProvider = null;
            this.targetTreeView1.Name = "targetTreeView1";
            this.targetTreeView1.SelectedNode = null;
            this.targetTreeView1.SettingsProvider = null;
            this.targetTreeView1.Size = new System.Drawing.Size(311, 133);
            this.targetTreeView1.StatusProvider = null;
            this.targetTreeView1.TabIndex = 0;
            this.targetTreeView1.TargetBlobStorageNamespace = null;
            this.targetTreeView1.TargetSettings = null;
            this.targetTreeView1.TargetSubscription = null;
            this.targetTreeView1.AfterTargetSelected += new MigAz.Azure.UserControls.TargetTreeView.AfterTargetSelectedHandler(this.targetTreeView1_AfterTargetSelected);
            this.targetTreeView1.AfterNewTargetResourceAdded += new MigAz.Azure.UserControls.TargetTreeView.AfterNewTargetResourceAddedHandler(this.targetTreeView1_AfterNewTargetResourceAdded);
            this.targetTreeView1.AfterExportArtifactRefresh += new MigAz.Azure.UserControls.TargetTreeView.AfterExportArtifactRefreshHandler(this.targetTreeView1_AfterExportArtifactRefresh);
            this.targetTreeView1.AfterSourceNodeRemoved += new MigAz.Azure.UserControls.TargetTreeView.AfterSourceNodeRemovedHandler(this.targetTreeView1_AfterSourceNodeRemoved);
            // 
            // tabMigAzMonitoring
            // 
            this.tabMigAzMonitoring.Controls.Add(this.tabMessages);
            this.tabMigAzMonitoring.Controls.Add(this.tabPage1);
            this.tabMigAzMonitoring.Controls.Add(this.tabLog);
            this.tabMigAzMonitoring.Controls.Add(this.tabRest);
            this.tabMigAzMonitoring.HotTrack = true;
            this.tabMigAzMonitoring.Location = new System.Drawing.Point(2, 2);
            this.tabMigAzMonitoring.Margin = new System.Windows.Forms.Padding(2);
            this.tabMigAzMonitoring.Name = "tabMigAzMonitoring";
            this.tabMigAzMonitoring.SelectedIndex = 0;
            this.tabMigAzMonitoring.Size = new System.Drawing.Size(461, 150);
            this.tabMigAzMonitoring.TabIndex = 0;
            this.tabMigAzMonitoring.Resize += new System.EventHandler(this.tabControl1_Resize);
            // 
            // tabMessages
            // 
            this.tabMessages.Controls.Add(this.dgvMigAzMessages);
            this.tabMessages.Location = new System.Drawing.Point(4, 22);
            this.tabMessages.Margin = new System.Windows.Forms.Padding(2);
            this.tabMessages.Name = "tabMessages";
            this.tabMessages.Padding = new System.Windows.Forms.Padding(2);
            this.tabMessages.Size = new System.Drawing.Size(453, 124);
            this.tabMessages.TabIndex = 0;
            this.tabMessages.Text = "Messages";
            this.tabMessages.UseVisualStyleBackColor = true;
            // 
            // dgvMigAzMessages
            // 
            this.dgvMigAzMessages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMigAzMessages.Location = new System.Drawing.Point(0, 0);
            this.dgvMigAzMessages.Margin = new System.Windows.Forms.Padding(2);
            this.dgvMigAzMessages.Name = "dgvMigAzMessages";
            this.dgvMigAzMessages.RowTemplate.Height = 33;
            this.dgvMigAzMessages.Size = new System.Drawing.Size(445, 123);
            this.dgvMigAzMessages.TabIndex = 0;
            this.dgvMigAzMessages.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lblLastOutputRefresh);
            this.tabPage1.Controls.Add(this.btnRefreshOutput);
            this.tabPage1.Controls.Add(this.tabOutputResults);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(453, 124);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "Output";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lblLastOutputRefresh
            // 
            this.lblLastOutputRefresh.AutoSize = true;
            this.lblLastOutputRefresh.Location = new System.Drawing.Point(91, 11);
            this.lblLastOutputRefresh.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblLastOutputRefresh.Name = "lblLastOutputRefresh";
            this.lblLastOutputRefresh.Size = new System.Drawing.Size(35, 13);
            this.lblLastOutputRefresh.TabIndex = 2;
            this.lblLastOutputRefresh.Text = "label1";
            // 
            // btnRefreshOutput
            // 
            this.btnRefreshOutput.Enabled = false;
            this.btnRefreshOutput.Location = new System.Drawing.Point(2, 7);
            this.btnRefreshOutput.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefreshOutput.Name = "btnRefreshOutput";
            this.btnRefreshOutput.Size = new System.Drawing.Size(81, 21);
            this.btnRefreshOutput.TabIndex = 1;
            this.btnRefreshOutput.Text = "Refresh";
            this.btnRefreshOutput.UseVisualStyleBackColor = true;
            this.btnRefreshOutput.Click += new System.EventHandler(this.btnRefreshOutput_Click);
            // 
            // tabOutputResults
            // 
            this.tabOutputResults.HotTrack = true;
            this.tabOutputResults.Location = new System.Drawing.Point(2, 31);
            this.tabOutputResults.Margin = new System.Windows.Forms.Padding(2);
            this.tabOutputResults.Name = "tabOutputResults";
            this.tabOutputResults.SelectedIndex = 0;
            this.tabOutputResults.Size = new System.Drawing.Size(377, 74);
            this.tabOutputResults.TabIndex = 0;
            this.tabOutputResults.Resize += new System.EventHandler(this.tabOutputResults_Resize);
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.txtLog);
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Margin = new System.Windows.Forms.Padding(2);
            this.tabLog.Name = "tabLog";
            this.tabLog.Size = new System.Drawing.Size(453, 124);
            this.tabLog.TabIndex = 1;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Margin = new System.Windows.Forms.Padding(0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(454, 126);
            this.txtLog.TabIndex = 0;
            this.txtLog.WordWrap = false;
            // 
            // tabRest
            // 
            this.tabRest.Controls.Add(this.txtRest);
            this.tabRest.Location = new System.Drawing.Point(4, 22);
            this.tabRest.Margin = new System.Windows.Forms.Padding(2);
            this.tabRest.Name = "tabRest";
            this.tabRest.Size = new System.Drawing.Size(453, 124);
            this.tabRest.TabIndex = 2;
            this.tabRest.Text = "REST";
            this.tabRest.UseVisualStyleBackColor = true;
            // 
            // txtRest
            // 
            this.txtRest.Location = new System.Drawing.Point(1, 1);
            this.txtRest.Margin = new System.Windows.Forms.Padding(0);
            this.txtRest.Multiline = true;
            this.txtRest.Name = "txtRest";
            this.txtRest.ReadOnly = true;
            this.txtRest.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRest.Size = new System.Drawing.Size(454, 126);
            this.txtRest.TabIndex = 1;
            // 
            // propertyPanel1
            // 
            this.propertyPanel1.Location = new System.Drawing.Point(8, 4);
            this.propertyPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.propertyPanel1.Name = "propertyPanel1";
            this.propertyPanel1.Size = new System.Drawing.Size(300, 306);
            this.propertyPanel1.StatusProvider = null;
            this.propertyPanel1.TabIndex = 8;
            this.propertyPanel1.TargetTreeView = null;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Controls.Add(this.txtDestinationFolder);
            this.panel1.Controls.Add(this.btnChoosePath);
            this.panel1.Location = new System.Drawing.Point(0, 382);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(356, 81);
            this.panel1.TabIndex = 7;
            this.panel1.Resize += new System.EventHandler(this.panel1_Resize);
            // 
            // btnExport
            // 
            this.btnExport.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnExport.Enabled = false;
            this.btnExport.Location = new System.Drawing.Point(8, 29);
            this.btnExport.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(338, 23);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "&Export Objects";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click_1Async);
            // 
            // txtDestinationFolder
            // 
            this.txtDestinationFolder.Location = new System.Drawing.Point(8, 4);
            this.txtDestinationFolder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtDestinationFolder.Name = "txtDestinationFolder";
            this.txtDestinationFolder.Size = new System.Drawing.Size(300, 20);
            this.txtDestinationFolder.TabIndex = 7;
            // 
            // btnChoosePath
            // 
            this.btnChoosePath.Location = new System.Drawing.Point(317, 0);
            this.btnChoosePath.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnChoosePath.Name = "btnChoosePath";
            this.btnChoosePath.Size = new System.Drawing.Size(30, 23);
            this.btnChoosePath.TabIndex = 8;
            this.btnChoosePath.Text = "...";
            this.btnChoosePath.UseVisualStyleBackColor = true;
            this.btnChoosePath.Click += new System.EventHandler(this.btnChoosePath_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(67, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(958, 24);
            this.menuStrip1.TabIndex = 58;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.menuitemAzureEnvironments,
            this.optionsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Enabled = false;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem2.Text = "&New Migration";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // menuitemAzureEnvironments
            // 
            this.menuitemAzureEnvironments.Name = "menuitemAzureEnvironments";
            this.menuitemAzureEnvironments.Size = new System.Drawing.Size(180, 22);
            this.menuitemAzureEnvironments.Text = "&Azure Environments";
            this.menuitemAzureEnvironments.Click += new System.EventHandler(this.menuitemAzureEnvironments_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.optionsToolStripMenuItem.Text = "&Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.visitMigAzOnGithubToolStripMenuItem,
            this.reportAnIssueOnGithubToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // visitMigAzOnGithubToolStripMenuItem
            // 
            this.visitMigAzOnGithubToolStripMenuItem.Name = "visitMigAzOnGithubToolStripMenuItem";
            this.visitMigAzOnGithubToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.visitMigAzOnGithubToolStripMenuItem.Text = "Visit MigAz on Github";
            this.visitMigAzOnGithubToolStripMenuItem.Click += new System.EventHandler(this.visitMigAzOnGithubToolStripMenuItem_Click);
            // 
            // reportAnIssueOnGithubToolStripMenuItem
            // 
            this.reportAnIssueOnGithubToolStripMenuItem.Name = "reportAnIssueOnGithubToolStripMenuItem";
            this.reportAnIssueOnGithubToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.reportAnIssueOnGithubToolStripMenuItem.Text = "Report an issue on Github";
            this.reportAnIssueOnGithubToolStripMenuItem.Click += new System.EventHandler(this.reportAnIssueOnGithubToolStripMenuItem_Click);
            // 
            // MigAzForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(958, 503);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MaximumSize = new System.Drawing.Size(2506, 2613);
            this.MinimumSize = new System.Drawing.Size(965, 533);
            this.Name = "MigAzForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MigAz";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MigAzForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.tabMigAzMonitoring.ResumeLayout(false);
            this.tabMessages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMigAzMessages)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabLog.ResumeLayout(false);
            this.tabLog.PerformLayout();
            this.tabRest.ResumeLayout(false);
            this.tabRest.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabMigAzMonitoring;
        private System.Windows.Forms.TabPage tabMessages;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.DataGridView dgvMigAzMessages;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabPage tabRest;
        private System.Windows.Forms.TextBox txtRest;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox txtDestinationFolder;
        private System.Windows.Forms.Button btnChoosePath;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportAnIssueOnGithubToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visitMigAzOnGithubToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabControl tabOutputResults;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnRefreshOutput;
        private System.Windows.Forms.Label lblLastOutputRefresh;
        private Azure.UserControls.PropertyPanel propertyPanel1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private UserControls.MigAzMigrationSourceSelection migAzMigrationSourceSelection1;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private Azure.UserControls.TargetTreeView targetTreeView1;
        private System.Windows.Forms.ToolStripMenuItem menuitemAzureEnvironments;
        private Azure.UserControls.AzureLoginContextViewer targetAzureContextViewer;
    }
}


