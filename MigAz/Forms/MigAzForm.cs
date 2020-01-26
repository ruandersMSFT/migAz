// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Windows.Forms;
using MigAz.Providers;
using MigAz.Azure.Core.Interface;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using MigAz.Azure;
using MigAz.Azure.UserControls;
using MigAz.Azure.Generator.AsmToArm;
using MigAz.Azure.Forms;
using MigAz.Azure.Core;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MigAz.Forms
{
    public partial class MigAzForm : Form
    {
        #region Variables

        private List<AzureEnvironment> _AzureEnvironments = new List<AzureEnvironment>();
        private List<AzureEnvironment> _UserDefinedAzureEnvironments = new List<AzureEnvironment>();
        private Guid _AppSessionGuid = Guid.NewGuid();
        private FileLogProvider _logProvider;
        private IStatusProvider _statusProvider;
        private AzureRetriever _AzureRetriever;
        private AppSettingsProvider _appSettingsProvider;
        private AzureTelemetryProvider _telemetryProvider = new AzureTelemetryProvider();
        private TreeNode _EventSourceNode;
        private AzureContext _TargetAzureContext;
        private AzureGenerator _AzureGenerator;

        #endregion

        #region Constructors
        public MigAzForm()
        {
            InitializeComponent();
            _logProvider = new FileLogProvider();
            _logProvider.OnMessage += _logProvider_OnMessage;
            _statusProvider = new UIStatusProvider(this.toolStripStatusLabel1);
            _appSettingsProvider = new AppSettingsProvider();
            _AzureEnvironments = AzureEnvironment.GetAzureEnvironments();
            _AzureRetriever = new AzureRetriever(_logProvider, _statusProvider);
            _TargetAzureContext = new AzureContext(_AzureRetriever, _appSettingsProvider.GetTargetSettings());
            _AzureGenerator = new AzureGenerator(_logProvider, _statusProvider);

            //todonowasap 
            //if (app.Default.UserDefinedAzureEnvironments != null && app.Default.UserDefinedAzureEnvironments != String.Empty)
            //{
            //    _UserDefinedAzureEnvironments  = JsonConvert.DeserializeObject<List<AzureEnvironment>>(app.Default.UserDefinedAzureEnvironments);
            //}

            targetAzureContextViewer.Bind(_TargetAzureContext, _AzureRetriever, _AzureEnvironments, ref _UserDefinedAzureEnvironments);

            propertyPanel1.Clear();
            splitContainer2.SplitterDistance = this.Height / 2;
            splitContainer3.SplitterDistance = splitContainer3.Width / 2;
            splitContainer4.SplitterDistance = 175;

            lblLastOutputRefresh.Text = String.Empty;
            txtDestinationFolder.Text = AppDomain.CurrentDomain.BaseDirectory;

            // Future thought, do away with the "Set"s and consolidate to a Bind?
            this.targetTreeView1.LogProvider = this.LogProvider;
            this.targetTreeView1.StatusProvider = this.StatusProvider;
            this.targetTreeView1.ImageList = this.imageList1;
            this.targetTreeView1.TargetSettings = _appSettingsProvider.GetTargetSettings();

            this.propertyPanel1.TargetTreeView = targetTreeView1;
            this.propertyPanel1.PropertyChanged += PropertyPanel1_PropertyChanged;

            AlertIfNewVersionAvailable();
        }

        #region New Version Check

        private async Task AlertIfNewVersionAvailable()
        {
            string currentVersion = "3.1.0.0";
            VersionCheck versionCheck = new VersionCheck(this.LogProvider);
            string newVersionNumber = await versionCheck.GetAvailableVersion("https://migaz.azurewebsites.net/api/v2", currentVersion);
            if (versionCheck.IsVersionNewer(currentVersion, newVersionNumber))
            {
                NewVersionAvailableDialog newVersionDialog = new NewVersionAvailableDialog();
                newVersionDialog.Bind(currentVersion, newVersionNumber);
                newVersionDialog.ShowDialog();
            }
        }

        #endregion

        #region Azure Migration Source Context Events

        private void MigrationSourceControl_ClearContext()
        {
            this.propertyPanel1.Clear();
            this.targetTreeView1.Clear();

            dgvMigAzMessages.DataSource = null;
            btnRefreshOutput.Enabled = false;
            btnExport.Enabled = false;

            foreach (TabPage t in tabOutputResults.TabPages)
            {
                tabOutputResults.TabPages.Remove(t);
            }
        }

        private async Task MigrationSourceControl_AfterNodeChecked(MigrationTarget sender)
        {
            TreeNode resultUpdateARMTree = await targetTreeView1.AddMigrationTarget(sender);
        }

        private async Task MigrationSourceControl_AfterNodeUnchecked(MigrationTarget sender)
        {
            await targetTreeView1.RemoveMigrationTarget(sender);
        }

        private async Task MigrationSourceControl_AfterNodeChanged(MigrationTarget sender)
        {
            await targetTreeView1.RefreshExportArtifacts();
        }

        private async Task MigrationSourceControl_BeforeAzureTenantChange(AzureContext sender)
        {
            targetAzureContextViewer.ExistingContext = sender;
        }

        private async Task MigrationSourceControl_AfterAzureTenantChange(AzureContext sender)
        {
            targetAzureContextViewer.ExistingContext = sender;
        }

        private async Task MigrationSourceControl_AfterUserSignOut()
        {
        }

        private async Task MigrationSourceControl_BeforeUserSignOut()
        {
        }

        private async Task MigrationSourceControl_AfterAzureSubscriptionChange(AzureContext sender)
        {
            targetAzureContextViewer.ExistingContext = sender;
        }

        private async Task MigrationSourceControl_BeforeAzureSubscriptionChange(AzureContext sender)
        {
            targetAzureContextViewer.ExistingContext = sender;
        }

        private async Task MigrationSourceControl_UserAuthenticated(AzureContext sender)
        {
            targetAzureContextViewer.ExistingContext = sender;
            this.targetTreeView1.Enabled = true;
        }

        private async Task MigrationSourceControl_AzureEnvironmentChanged(AzureContext sender)
        {
            targetAzureContextViewer.ExistingContext = sender;
        }

        #endregion

        #region Form Objects

        private IMigrationSourceUserControl MigrationSourceControl
        {
            get
            {
                foreach (Control control in splitContainer3.Panel1.Controls)
                {
                    if (control.GetType().GetInterfaces().Contains(typeof(IMigrationSourceUserControl)))
                    {
                        IMigrationSourceUserControl migrationSourceControl = (IMigrationSourceUserControl)control;
                        return migrationSourceControl;
                    }
                }

                return null;
            }
        }

        #endregion

        private async Task PropertyPanel1_PropertyChanged(Azure.Core.MigrationTarget migrationTarget)
        {
            TreeNode targetNode = this.targetTreeView1.SeekMigrationTargetTreeNode(migrationTarget);

            if (targetNode != null)
            {
                targetNode.Tag = migrationTarget;
                targetNode.Text = migrationTarget.ToString();

                if (migrationTarget.GetType() == typeof(Azure.MigrationTarget.ResourceGroup))
                    targetNode.Name = migrationTarget.ToString();
            }

            await this.targetTreeView1.RefreshExportArtifacts();
        }

        private void _logProvider_OnMessage(string message)
        {
            txtLog.AppendText(message);
            txtLog.SelectionStart = txtLog.TextLength;
        }

        private void AzureRetriever_OnRestResult(AzureRestResponse response)
        {
            txtRest.AppendText(response.RequestGuid.ToString() + " " + response.Url + Environment.NewLine);
            txtRest.AppendText(response.Response + Environment.NewLine + Environment.NewLine);
            txtRest.SelectionStart = txtRest.TextLength;
        }

        #endregion

        #region Properties

        public AzureGenerator AzureGenerator
        {
            get { return _AzureGenerator; }
        }

        public ILogProvider LogProvider
        {
            get { return _logProvider; }
        }

        public IStatusProvider StatusProvider
        {
            get { return _statusProvider; }
        }

        internal AppSettingsProvider AppSettingsProvider
        {
            get { return _appSettingsProvider; }
        }

        #endregion

        #region Form Events

        private void MigAzForm_Load(object sender, EventArgs e)
        {
            _logProvider.WriteLog("MigAzForm_Load", "Program start");
            this.Text = "MigAz";
        }

        #endregion

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://aka.ms/MigAz");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tabControl1_Resize(object sender, EventArgs e)
        {
            dgvMigAzMessages.Width = tabMigAzMonitoring.Width - 10;
            dgvMigAzMessages.Height = tabMigAzMonitoring.Height - 30;
            txtLog.Width = tabMigAzMonitoring.Width - 10;
            txtLog.Height = tabMigAzMonitoring.Height - 30;
            txtRest.Width = tabMigAzMonitoring.Width - 10;
            txtRest.Height = tabMigAzMonitoring.Height - 30;
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            btnExport.Width = panel1.Width - 15;
            btnChoosePath.Left = panel1.Width - btnChoosePath.Width - 10;
            txtDestinationFolder.Width = panel1.Width - btnChoosePath.Width - 30;
        }

        private void reportAnIssueOnGithubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Azure/migAz/issues/new");
        }

        private void visitMigAzOnGithubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://aka.ms/migaz");
        }

        private async void btnExport_Click_1Async(object sender, EventArgs e)
        {
            // We are refreshing both the MemoryStreams and the Output Tabs via this call, prior to writing to files
            if (await RefreshOutput())
            {
                if (this.AzureGenerator != null)
                {
                    this.AzureGenerator.OutputDirectory = txtDestinationFolder.Text;

                    this.AzureGenerator.Write();

                    StatusProvider.UpdateStatus("Ready");

                    var exportResults = new ExportResultsDialog(this.AzureGenerator);
                    exportResults.ShowDialog(this);
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            TargetTreeView targetTreeView = this.targetTreeView1;

            if (targetTreeView != null && e.RowIndex > -1)
            {
                object alert = dgvMigAzMessages.Rows[e.RowIndex].Cells["SourceObject"].Value;
                targetTreeView.SeekAlertSource(alert);
            }
        }

        private void tabOutputResults_Resize(object sender, EventArgs e)
        {
            foreach (TabPage tabPage in tabOutputResults.TabPages)
            {
                foreach (Control control in tabPage.Controls)
                {
                    control.Width = tabOutputResults.Width - 15;
                    control.Height = tabOutputResults.Height - 30;
                }
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog optionsDialog = new OptionsDialog();
            optionsDialog.Bind(_AzureEnvironments, _UserDefinedAzureEnvironments);
            optionsDialog.ShowDialog();

            IMigrationSourceUserControl sourceUserControl = this.MigrationSourceControl;
            if (sourceUserControl != null)
            {
                if (sourceUserControl.GetType() == typeof(MigrationAzureSourceContext))
                {
                    MigrationAzureSourceContext migrationAzureSourceContext = (MigrationAzureSourceContext)sourceUserControl;
                    migrationAzureSourceContext.AzureContext.LoginPromptBehavior = Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior.SelectAccount; //todonowasap  app.Default.LoginPromptBehavior;
                }
            }
        }

        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
                txtDestinationFolder.Text = folderBrowserDialog1.SelectedPath;
        }

        private async void btnRefreshOutput_Click(object sender, EventArgs e)
        {
            await RefreshOutput();
        }

        private async Task<bool> RefreshOutput()
        {
            if (await AssertHasTargetErrors())
            {
                return false;
            }

            IMigrationSourceUserControl migrationSourceControl = this.MigrationSourceControl;
            if (migrationSourceControl == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL MigrationSourceControl Context");

            if (targetAzureContextViewer.ExistingContext == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL Target Existing Azure Context");

            if (targetAzureContextViewer.ExistingContext.AzureSubscription == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL Target Existing Azure Context");

            if (this.AzureGenerator == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL TemplateGenerator");

            if (this.targetAzureContextViewer == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL TargetAzureContextViewer");

            if (this.targetAzureContextViewer.SelectedAzureContext == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL SelectedAzureContext on TargetAzureContextViewer");

            if (this.targetAzureContextViewer.SelectedAzureContext.TokenProvider == null)
                throw new ArgumentException("Unable to Refresh Output:  NULL TokenProvider on SelectedAzureContext");

            if (this.AzureGenerator != null)
            {
                this.AzureGenerator.AccessSASTokenLifetimeSeconds = 43200; //todonowasap  app.Default.AccessSASTokenLifetimeSeconds;
                this.AzureGenerator.ExportArtifacts = this.targetTreeView1.ExportArtifacts;
                this.AzureGenerator.OutputDirectory = this.txtDestinationFolder.Text;
                this.AzureGenerator.TargetAzureTokenProvider = (AzureTokenProvider)this.targetAzureContextViewer.SelectedAzureContext.TokenProvider;

                await this.AzureGenerator.GenerateStreams();

                foreach (TabPage tabPage in tabOutputResults.TabPages)
                {
                    if (!this.AzureGenerator.TemplateStreams.ContainsKey(tabPage.Name))
                        tabOutputResults.TabPages.Remove(tabPage);
                }

                foreach (var templateStream in this.AzureGenerator.TemplateStreams)
                {
                    TabPage tabPage = null;
                    if (!tabOutputResults.TabPages.ContainsKey(templateStream.Key))
                    {
                        tabPage = new TabPage(templateStream.Key);
                        tabPage.Name = templateStream.Key;
                        tabOutputResults.TabPages.Add(tabPage);

                        if (templateStream.Key.EndsWith(".html"))
                        {
                            WebBrowser webBrowser = new WebBrowser();
                            webBrowser.Width = tabOutputResults.Width - 15;
                            webBrowser.Height = tabOutputResults.Height - 30;
                            webBrowser.AllowNavigation = false;
                            webBrowser.ScrollBarsEnabled = true;
                            tabPage.Controls.Add(webBrowser);
                        }
                        else if (templateStream.Key.EndsWith(".json") || templateStream.Key.EndsWith(".ps1"))
                        {
                            TextBox textBox = new TextBox();
                            textBox.Width = tabOutputResults.Width - 15;
                            textBox.Height = tabOutputResults.Height - 30;
                            textBox.ReadOnly = true;
                            textBox.Multiline = true;
                            textBox.WordWrap = false;
                            textBox.ScrollBars = ScrollBars.Both;
                            tabPage.Controls.Add(textBox);
                        }
                    }
                    else
                    {
                        tabPage = tabOutputResults.TabPages[templateStream.Key];
                    }

                    if (tabPage.Controls[0].GetType() == typeof(TextBox))
                    {
                        TextBox textBox = (TextBox)tabPage.Controls[0];
                        templateStream.Value.Position = 0;
                        textBox.Text = new StreamReader(templateStream.Value).ReadToEnd();
                    }
                    else if (tabPage.Controls[0].GetType() == typeof(WebBrowser))
                    {
                        WebBrowser webBrowser = (WebBrowser)tabPage.Controls[0];
                        templateStream.Value.Position = 0;

                        if (webBrowser.Document == null)
                        {
                            webBrowser.DocumentText = new StreamReader(templateStream.Value).ReadToEnd();
                        }
                        else
                        {
                            webBrowser.Document.OpenNew(true);
                            webBrowser.Document.Write(new StreamReader(templateStream.Value).ReadToEnd());
                        }
                    }
                }

                if (tabOutputResults.TabPages.Count != this.AzureGenerator.TemplateStreams.Count)
                    throw new ArgumentException("Count mismatch between tabOutputResults TabPages and Migrator TemplateStreams.  Counts should match after addition/removal above.  tabOutputResults. TabPages Count: " + tabOutputResults.TabPages.Count + "  Migration TemplateStream Count: " + this.AzureGenerator.TemplateStreams.Count);

                // Ensure Tabs are in same order as output streams
                int streamIndex = 0;
                foreach (string templateStreamKey in this.AzureGenerator.TemplateStreams.Keys)
                {
                    int rotationCounter = 0;

                    // This while loop is to bubble the tab to the end, as to rotate the tab sequence to ensure they match the order returned from the stream outputs
                    // The addition/removal of Streams may result in order of existing tabPages being "out of order" to the streams generated, so we may need to consider reordering
                    while (tabOutputResults.TabPages[streamIndex].Name != templateStreamKey)
                    {
                        TabPage currentTabpage = tabOutputResults.TabPages[streamIndex];
                        tabOutputResults.TabPages.Remove(currentTabpage);
                        tabOutputResults.TabPages.Add(currentTabpage);

                        rotationCounter++;

                        if (rotationCounter > this.AzureGenerator.TemplateStreams.Count)
                            throw new ArgumentException("Rotated through all tabs, unabled to locate tab '" + templateStreamKey + "' while ensuring tab order/sequencing.");
                    }

                    streamIndex++;
                }


                lblLastOutputRefresh.Text = "Last Refresh Completed: " + DateTime.Now.ToString();
                btnRefreshOutput.Enabled = false;

                // post Telemetry Record to ASMtoARMToolAPI
                if (AppSettingsProvider.AllowTelemetry)
                {
                    StatusProvider.UpdateStatus("BUSY: saving telemetry information");
                    _telemetryProvider.PostTelemetryRecord(_AppSessionGuid, this.MigrationSourceControl.MigrationSourceType, targetAzureContextViewer.ExistingContext.AzureSubscription, this.AzureGenerator);
                }
            }

            StatusProvider.UpdateStatus("Ready");
            return true;
        }


        #region Split Container Resize Events

        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            propertyPanel1.Width = splitContainer1.Panel2.Width - 10;
            propertyPanel1.Height = splitContainer1.Panel2.Height - 100;
            panel1.Top = splitContainer1.Panel2.Height - panel1.Height - 15;
            panel1.Width = splitContainer1.Panel2.Width;
        }

        private void splitContainer2_Panel1_Resize(object sender, EventArgs e)
        {
            if (splitContainer2.Panel1.Controls.Count == 1)
            {
                if (splitContainer2.Panel1.Height < 300)
                    splitContainer2.Panel1.Controls[0].Height = 300;
                else
                    splitContainer2.Panel1.Controls[0].Height = splitContainer2.Panel1.Height - 20;
            }
        }

        private void splitContainer2_Panel2_Resize(object sender, EventArgs e)
        {
            this.tabMigAzMonitoring.Width = splitContainer2.Panel2.Width - 5;
            this.tabMigAzMonitoring.Height = splitContainer2.Panel2.Height - 5;
            this.tabOutputResults.Width = splitContainer2.Panel2.Width - 5;
            this.tabOutputResults.Height = splitContainer2.Panel2.Height - 55;
        }

        private void splitContainer3_Panel1_Resize(object sender, EventArgs e)
        {
            foreach (Control control in splitContainer3.Panel1.Controls)
            {
                if (splitContainer3.Panel1.Height < 300)
                    control.Height = 300;
                else
                    control.Height = splitContainer3.Panel1.Height - 10;

                control.Width = splitContainer3.Panel1.Width - 10;
            }
        }

        private void splitContainer3_Panel2_Resize(object sender, EventArgs e)
        {
            if (splitContainer3.Panel2.Controls.Count == 1)
            {
                if (splitContainer3.Panel2.Height < 300)
                    splitContainer3.Panel2.Controls[0].Height = 300;
                else
                    splitContainer3.Panel2.Controls[0].Height = splitContainer3.Panel2.Height - 20;

                splitContainer3.Panel2.Controls[0].Width = splitContainer3.Panel2.Width - 20;
            }
        }

        private void splitContainer4_Panel2_Resize(object sender, EventArgs e)
        {
            foreach (Control control in splitContainer4.Panel2.Controls)
            {
                control.Width = splitContainer4.Panel2.Width;
                control.Height = splitContainer4.Panel2.Height;
            }
        }

        private void splitContainer4_Panel1_Resize(object sender, EventArgs e)
        {
            foreach (Control control in splitContainer4.Panel1.Controls)
            {
                control.Width = splitContainer4.Panel1.Width - 10;
                control.Height = splitContainer4.Panel1.Height;
            }
        }

        #endregion

        #region Source and Target Context Selection Events + Methods

        private bool MigrationSourceSelectionControlVisible
        {
            get
            {
                foreach (Control control in splitContainer3.Panel1.Controls)
                {
                    if (control.GetType() == typeof(UserControls.MigAzMigrationSourceSelection))
                    {
                        return control.Visible;
                    }
                }

                return false;
            }
            set
            {
                foreach (Control control in splitContainer3.Panel1.Controls)
                {
                    if (control.GetType() == typeof(UserControls.MigAzMigrationSourceSelection))
                    {
                        control.Visible = value;
                        control.Enabled = value;
                    }
                }
            }
        }

        private async void migAzMigrationSourceSelection1_AfterMigrationSourceSelected(UserControl migrationSourceUserControl)
        {
            if (migrationSourceUserControl.GetType() == typeof(MigrationAzureSourceContext))
            {
                MigrationAzureSourceContext azureControl = (MigrationAzureSourceContext)migrationSourceUserControl;

                // This will move to be based on the source context (upon instantiation)
                azureControl.Bind(this._AzureRetriever, this._statusProvider, this._logProvider, this._appSettingsProvider.GetTargetSettings(), this.imageList1, Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior.SelectAccount, this._AzureEnvironments, ref this._UserDefinedAzureEnvironments);

                azureControl.AzureContext.AzureEnvironment = GetDefaultAzureEnvironment();

                azureControl.AzureEnvironmentChanged += MigrationSourceControl_AzureEnvironmentChanged;
                azureControl.UserAuthenticated += MigrationSourceControl_UserAuthenticated;
                azureControl.BeforeAzureSubscriptionChange += MigrationSourceControl_BeforeAzureSubscriptionChange;
                azureControl.AfterAzureSubscriptionChange += MigrationSourceControl_AfterAzureSubscriptionChange;
                azureControl.BeforeUserSignOut += MigrationSourceControl_BeforeUserSignOut;
                azureControl.AfterUserSignOut += MigrationSourceControl_AfterUserSignOut;
                azureControl.AfterAzureTenantChange += MigrationSourceControl_AfterAzureTenantChange;
                azureControl.BeforeAzureTenantChange += MigrationSourceControl_BeforeAzureTenantChange;
                azureControl.AfterNodeChecked += MigrationSourceControl_AfterNodeChecked;
                azureControl.AfterNodeUnchecked += MigrationSourceControl_AfterNodeUnchecked;
                azureControl.AfterNodeChanged += MigrationSourceControl_AfterNodeChanged;
                azureControl.ClearContext += MigrationSourceControl_ClearContext;
                azureControl.AfterContextChanged += AzureControl_AfterContextChanged;
                azureControl.AzureContext.AzureRetriever.OnRestResult += AzureRetriever_OnRestResult;
            }


            MigrationSourceSelectionControlVisible = false;
            splitContainer3.Panel1.Controls.Add(migrationSourceUserControl);
            migrationSourceUserControl.Top = 5;
            migrationSourceUserControl.Left = 5;
            splitContainer3_Panel1_Resize(this, null);
        }

        private AzureEnvironment GetDefaultAzureEnvironment()
        {
            AzureEnvironment defaultAzureEnvironment;

            if (_AzureEnvironments != null)
            {
                //todonowasap defaultAzureEnvironment = _AzureEnvironments.Where(a => a.Name == app.Default.AzureEnvironment).FirstOrDefault();
                //if (defaultAzureEnvironment != null)
                    //return defaultAzureEnvironment;
            }

            if (_UserDefinedAzureEnvironments != null)
            {
                //todonowasap defaultAzureEnvironment = _UserDefinedAzureEnvironments.Where(a => a.Name == app.Default.AzureEnvironment).FirstOrDefault();
                //if (defaultAzureEnvironment != null)
                    //return defaultAzureEnvironment;
            }

            return null;
        }

        private async void AzureControl_AfterContextChanged(UserControl sender)
        {
            IMigrationSourceUserControl migrationSourceUserControl = (IMigrationSourceUserControl)sender;
            targetAzureContextViewer.Enabled = migrationSourceUserControl.IsSourceContextAuthenticated;

            if (migrationSourceUserControl != null && migrationSourceUserControl.GetType() == typeof(MigrationAzureSourceContext))
            {
                MigrationAzureSourceContext azureSourceContext = (MigrationAzureSourceContext)migrationSourceUserControl;
                targetAzureContextViewer.ExistingContext = azureSourceContext.AzureContext;
            }
        }

        #endregion

        public async Task<bool> AssertHasTargetErrors()
        {
            // Refresh output first, await, to ensure subsequence call for HasErrors has updated status.
            await this.targetTreeView1.RefreshExportArtifacts();

            if (this.targetTreeView1.HasErrors)
            {
                tabMigAzMonitoring.SelectTab("tabMessages");
                MessageBox.Show("There are still one or more error(s) with the template generation.  Please resolve all errors before exporting.");
            }

            return this.targetTreeView1.HasErrors;
        }

        #region Target Tree View Events

        private async Task targetTreeView1_AfterNewTargetResourceAdded(TargetTreeView sender, TreeNode selectedNode)
        {
            // Refresh Alerts from new item
            await targetTreeView1_AfterExportArtifactRefresh(sender);
        }

        private async Task targetTreeView1_AfterExportArtifactRefresh(TargetTreeView sender)
        {
            dgvMigAzMessages.DataSource = sender.Alerts.Select(x => new { AlertType = x.AlertType, Message = x.Message, SourceObject = x.SourceObject }).ToList();
            dgvMigAzMessages.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvMigAzMessages.Columns["SourceObject"].Visible = false;

            // WordWrap Message Column
            dgvMigAzMessages.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvMigAzMessages.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dgvMigAzMessages.Columns["Message"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvMigAzMessages.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            btnRefreshOutput.Enabled = true;
            btnExport.Enabled = true;

            await RebindPropertyPanel();
        }

        private async Task RebindPropertyPanel()
        {
            //if (this.targetTreeView1 != null && this.targetTreeView1.SelectedNode != null)
               // await BindPropertyPanel(this.targetTreeView1, this.targetTreeView1.SelectedNode);
        }

        private async Task BindPropertyPanel(TargetTreeView targetTreeView, TreeNode selectedNode)
        {
            await propertyPanel1.Bind((MigrationTarget)selectedNode.Tag);
        }

        private async Task targetTreeView1_AfterTargetSelected(TargetTreeView targetTreeView, TreeNode selectedNode)
        {
            try
            {
                if (this.LogProvider != null)
                    LogProvider.WriteLog("Control_AfterTargetSelected", "Start");

                _EventSourceNode = selectedNode;
                await BindPropertyPanel(targetTreeView, selectedNode);
                _EventSourceNode = null;

                if (this.LogProvider != null)
                    LogProvider.WriteLog("Control_AfterTargetSelected", "End");

                if (this.StatusProvider != null)
                    StatusProvider.UpdateStatus("Ready");
            }
            catch (Exception exc)
            {
                UnhandledExceptionDialog unhandledExceptionDialog = new UnhandledExceptionDialog(this.LogProvider, exc);
                unhandledExceptionDialog.ShowDialog();
            }
        }

        private async Task targetTreeView1_AfterSourceNodeRemoved(TargetTreeView sender, TreeNode removedNode)
        {
            if (this.LogProvider != null)
                LogProvider.WriteLog("targetTreeView1_AfterSourceNodeRemoved", "Start");

            if (removedNode.Tag != null)
            {
                MigrationTarget migrationTarget = (MigrationTarget)removedNode.Tag;
                await this.MigrationSourceControl.UncheckMigrationTarget(migrationTarget);
            }

            if (this.LogProvider != null)
                LogProvider.WriteLog("targetTreeView1_AfterSourceNodeRemoved", "End");
        }

        #endregion

        private void menuitemAzureEnvironments_Click(object sender, EventArgs e)
        {
            AzureEnvironmentDialog azureEnvironmentDialog = new AzureEnvironmentDialog();
            azureEnvironmentDialog.Bind(_AzureRetriever, _AzureEnvironments, ref _UserDefinedAzureEnvironments);
            azureEnvironmentDialog.ShowDialog();

            // Save User Defined Azure Environments to App Settings
            //todonowasap app.Default.UserDefinedAzureEnvironments = JsonConvert.SerializeObject(_UserDefinedAzureEnvironments);
            //todonowasap             app.Default.Save();
        }

        private async Task targetAzureContextViewer_AfterContextChanged(AzureLoginContextViewer sender)
        {
            if (sender.SelectedAzureContext.AzureSubscription != null)
            {
                await sender.SelectedAzureContext.AzureSubscription.InitializeChildrenAsync();
                await sender.SelectedAzureContext.AzureSubscription.BindArmResources(targetTreeView1.TargetSettings);
            }

            targetTreeView1.TargetBlobStorageNamespace = sender.SelectedAzureContext.AzureEnvironment.BlobEndpointUrl;
            targetTreeView1.TargetSubscription = sender.SelectedAzureContext.AzureSubscription;
            await this.targetTreeView1.RefreshExportArtifacts();
        }
    }
}

