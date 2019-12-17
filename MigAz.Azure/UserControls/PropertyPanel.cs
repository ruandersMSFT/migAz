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

namespace MigAz.Azure.UserControls
{
    public partial class PropertyPanel : UserControl
    {
        private TargetTreeView _TargetTreeView;
        private Core.MigrationTarget _MigrationTarget;

        public delegate Task AfterPropertyChanged(Core.MigrationTarget migrationTarget);
        public event AfterPropertyChanged PropertyChanged;

        public PropertyPanel()
        {
            InitializeComponent();
        }

        public IStatusProvider StatusProvider
        {
            get; set;
        }

        public TargetTreeView TargetTreeView
        {
            get { return _TargetTreeView; }
            set { _TargetTreeView = value; }
        }

        public Core.MigrationTarget MigrationTarget
        {
            get { return _MigrationTarget; }
            private set { _MigrationTarget = value; }
        }

        public Image ResourceImage
        {
            get { return pictureBox1.Image; }
            private set { pictureBox1.Image = value; }
        }

        public string ResourceText
        {
            get { return lblAzureObjectName.Text; }
            private set { lblAzureObjectName.Text = value; }
        }

        public Control PropertyDetailControl
        {
            get
            {
                if (this.pnlProperties.Controls.Count == 0)
                    return null;
                else
                    return this.pnlProperties.Controls[0];
            }
            private set
            {
                this.pnlProperties.Controls.Clear();

                if (value != null)
                {
                    this.pnlProperties.Controls.Add(value);
                }
            }
        }

        public void Clear()
        {
            this._MigrationTarget = null;
            this.ResourceImage = null;
            this.ResourceText = String.Empty;
            this.lblResourceType.Text = String.Empty;
            this.pnlProperties.Controls.Clear();
            this.cmbApiVersions.Items.Clear();
            this.lblTargetAPIVersion.Visible = false;
            this.cmbApiVersions.Items.Clear();
            this.cmbApiVersions.Visible = false;
        }

        private void PropertyPanel_Resize(object sender, EventArgs e)
        {
            groupBox1.Width = this.Width - 5;
            groupBox1.Height = this.Height - 5;
        }

        private void groupBox1_Resize(object sender, EventArgs e)
        {
            pnlProperties.Width = groupBox1.Width - 15;
            pnlProperties.Height = groupBox1.Height - 90;
        }

        public bool IsBound
        {
            get { return _TargetTreeView != null && _MigrationTarget != null; }
        }

        public async Task Bind(Core.MigrationTarget migrationTarget)
        {
            if (this.TargetTreeView == null)
                throw new ArgumentException("MigrationTarget Property must be provided.");

            this.TargetTreeView.LogProvider.WriteLog("PropertyPanel Bind", "Start");

            this.Clear();

            if (migrationTarget.ApiVersion == null || migrationTarget.ApiVersion == String.Empty)
            {
                migrationTarget.DefaultApiVersion(this.TargetTreeView.TargetSubscription);
            }

            this._MigrationTarget = migrationTarget;
            this.ResourceText = migrationTarget.ToString();
            this.ResourceImage = imageList1.Images[migrationTarget.ImageKey];
            this.lblResourceType.Text = migrationTarget.FriendlyObjectName;

            if (migrationTarget.GetType() == typeof(VirtualNetworkGateway))
            {
                VirtualNetworkGatewayProperties properties = new VirtualNetworkGatewayProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((VirtualNetworkGateway)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(LocalNetworkGateway))
            {
                LocalNetworkGatewayProperties properties = new LocalNetworkGatewayProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((LocalNetworkGateway)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if(migrationTarget.GetType() == typeof(VirtualNetworkGatewayConnection))
            {
                VirtualNetworkConnectionProperties properties = new VirtualNetworkConnectionProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((VirtualNetworkGatewayConnection)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if(migrationTarget.GetType() == typeof(VirtualMachine))
            {
                VirtualMachineProperties properties = new VirtualMachineProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                await properties.Bind((VirtualMachine)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(ApplicationSecurityGroup))
            {
                ApplicationSecurityGroupProperties properties = new ApplicationSecurityGroupProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((ApplicationSecurityGroup)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(NetworkSecurityGroup))
            {
                NetworkSecurityGroupProperties properties = new NetworkSecurityGroupProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((NetworkSecurityGroup)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            if (migrationTarget.GetType() == typeof(VirtualNetwork))
            {
                VirtualNetworkProperties properties = new VirtualNetworkProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((VirtualNetwork)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(Subnet))
            {
                SubnetProperties properties = new SubnetProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((Subnet)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(StorageAccount))
            {
                StorageAccountProperties properties = new StorageAccountProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((StorageAccount)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(AvailabilitySet))
            {
                AvailabilitySetProperties properties = new AvailabilitySetProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((AvailabilitySet)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(PublicIp))
            {
                PublicIpProperties properties = new PublicIpProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((PublicIp)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(RouteTable))
            {
                RouteTableProperties properties = new RouteTableProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                properties.Bind((RouteTable)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(NetworkInterface))
            {
                NetworkInterfaceProperties properties = new NetworkInterfaceProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                await properties.Bind((NetworkInterface)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(ResourceGroup))
            {
                ResourceGroupProperties properties = new ResourceGroupProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                await properties.Bind((ResourceGroup)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(LoadBalancer))
            {
                LoadBalancerProperties properties = new LoadBalancerProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                await properties.Bind((LoadBalancer)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            else if (migrationTarget.GetType() == typeof(Disk))
            {
                DiskProperties properties = new DiskProperties();
                properties.PropertyChanged += Properties_PropertyChanged;
                await properties.Bind((Disk)migrationTarget, _TargetTreeView);
                this.PropertyDetailControl = properties;
            }
            //else if (migrationTarget.GetType() == typeof(VirtualMachineImage))
            //{
            //    VirtualMachineImageProperties properties = new VirtualMachineImageProperties();
            //    properties.PropertyChanged += Properties_PropertyChanged;
            //    properties.Bind(_TargetTreeView, (VirtualMachineImage)migrationTarget, _TargetTreeView);
            //    this.PropertyDetailControl = properties;
            //}

            Arm.ProviderResourceType targetProvider = this.TargetTreeView.GetTargetProvider(migrationTarget);
            if (targetProvider != null)
            {
                lblTargetAPIVersion.Visible = true;
                cmbApiVersions.Visible = true;

                foreach (string apiVersion in targetProvider.ApiVersions)
                {
                    cmbApiVersions.Items.Add(apiVersion);
                }

                if (migrationTarget.ApiVersion != null && migrationTarget.ApiVersion != String.Empty)
                {
                    cmbApiVersions.SelectedIndex = cmbApiVersions.FindStringExact(migrationTarget.ApiVersion);
                }
            }

            this.TargetTreeView.LogProvider.WriteLog("PropertyPanel Bind", "End");
        }

        private async Task Properties_PropertyChanged(Core.MigrationTarget migrationTarget)
        {
            _MigrationTarget = migrationTarget; // Refresh based on property change
            await PropertyChanged(migrationTarget);
        }

        private async void cmbApiVersions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_MigrationTarget != null)
            {
                if (cmbApiVersions.SelectedItem == null)
                    _MigrationTarget.ApiVersion = String.Empty;
                else
                    _MigrationTarget.ApiVersion = cmbApiVersions.SelectedItem.ToString();
            }

            foreach (Control control in this.pnlProperties.Controls)
            {
                TargetPropertyControl targetPropertyControl = (TargetPropertyControl)control;
                targetPropertyControl.UpdatePropertyEnablement();
            }

            if (PropertyChanged != null && _MigrationTarget != null)
                await PropertyChanged(_MigrationTarget);
        }
    }
}

