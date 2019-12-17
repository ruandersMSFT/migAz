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
    public partial class NetworkInterfaceProperties : TargetPropertyControl
    {
        private Azure.MigrationTarget.NetworkInterface _TargetNetworkInterface;

        public NetworkInterfaceProperties()
        {
            InitializeComponent();
            this.publicIpSelectionControl1.PropertyChanged += PublicIpSelectionControl1_PropertyChanged;
            this.networkSelectionControl1.PropertyChanged += NetworkSelectionControl1_PropertyChanged;
        }

        private void PublicIpSelectionControl1_PropertyChanged()
        {
            if (!this.IsBinding)
            {
                if (_TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations.Count > 0)
                {
                    _TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations[0].TargetPublicIp = publicIpSelectionControl1.PublicIp;
                }

                this.RaisePropertyChangedEvent(_TargetNetworkInterface);
            }
        }

        private void NetworkSelectionControl1_PropertyChanged()
        {
            this.RaisePropertyChangedEvent(_TargetNetworkInterface);
        }

        internal async Task Bind(NetworkInterface targetNetworkInterface, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;
                _TargetTreeView = targetTreeView;
                _TargetNetworkInterface = targetNetworkInterface;
                await publicIpSelectionControl1.Bind(targetTreeView);

                await networkSelectionControl1.Bind(targetTreeView);

                if (_TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations.Count > 0)
                {
                    networkSelectionControl1.VirtualNetworkTarget = _TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations[0];

                    if (_TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations[0].TargetPublicIp == null)
                        rbPublicIpDisabled.Checked = true;
                    else
                    {
                        rbPublicIpEnabled.Checked = true;
                    }

                    publicIpSelectionControl1.PublicIp = _TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations[0].TargetPublicIp;
                }

                lblSourceName.Text = _TargetNetworkInterface.SourceName;
                txtTargetName.Text = _TargetNetworkInterface.TargetName;

                if (_TargetNetworkInterface.EnableIPForwarding)
                    rbIPForwardingEnabled.Checked = true;
                else
                    rbIPForwardingDisabled.Checked = true;

                if (_TargetNetworkInterface.EnableAcceleratedNetworking)
                    rbAcceleratedNetworkingEnabled.Checked = true;
                else
                    rbAcceleratedNetworkingDisabled.Checked = true;

                if (_TargetNetworkInterface.Source != null)
                {
                    if (_TargetNetworkInterface.Source.GetType() == typeof(Azure.Asm.NetworkInterface))
                    {
                        Azure.Asm.NetworkInterface asmNetworkInterface = (Azure.Asm.NetworkInterface)_TargetNetworkInterface.Source;

                        lblVirtualNetworkName.Text = asmNetworkInterface.NetworkInterfaceIpConfigurations[0].VirtualNetworkName;
                        lblSubnetName.Text = asmNetworkInterface.NetworkInterfaceIpConfigurations[0].SubnetName;
                        lblStaticIpAddress.Text = asmNetworkInterface.NetworkInterfaceIpConfigurations[0].PrivateIpAddress;
                    }
                    else if (_TargetNetworkInterface.Source.GetType() == typeof(Azure.Arm.NetworkInterface))
                    {
                        Azure.Arm.NetworkInterface armNetworkInterface = (Azure.Arm.NetworkInterface)_TargetNetworkInterface.Source;

                        if (armNetworkInterface.NetworkInterfaceIpConfigurations.Count > 0)
                        {
                            // todo now russell lblVirtualNetworkName.Text = armNetworkInterface.NetworkInterfaceIpConfigurations[0].VirtualNetworkName;
                            // todo now russell lblSubnetName.Text = armNetworkInterface.NetworkInterfaceIpConfigurations[0].SubnetName;
                            lblStaticIpAddress.Text = armNetworkInterface.NetworkInterfaceIpConfigurations[0].PrivateIpAddress;
                        }
                    }
                }

                virtualMachineSummary.Bind(_TargetNetworkInterface.ParentVirtualMachine, _TargetTreeView);
                networkSecurityGroup.Bind(_TargetNetworkInterface.NetworkSecurityGroup, _TargetTreeView);

                await this.publicIpSelectionControl1.Bind(_TargetTreeView);

                this.UpdatePropertyEnablement();
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        internal override void UpdatePropertyEnablement()
        {
            this.pnlAcceleratedNetworking.Enabled = _TargetNetworkInterface.AllowAcceleratedNetworking;
            this.pnlAcceleratedNetworking.Visible = _TargetNetworkInterface.AllowAcceleratedNetworking;
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            _TargetNetworkInterface.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_TargetNetworkInterface);
        }

        private void txtTargetName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void rbIPForwardingEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rbIPForwardingEnabled.Checked)
            {
                _TargetNetworkInterface.EnableIPForwarding = true;

                this.RaisePropertyChangedEvent(_TargetNetworkInterface);
            }
        }

        private void rbIPForwardingDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rbIPForwardingDisabled.Checked)
            {
                _TargetNetworkInterface.EnableIPForwarding = false;

                this.RaisePropertyChangedEvent(_TargetNetworkInterface);
            }
        }

        private void rbAcceleratedNetworkingEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAcceleratedNetworkingEnabled.Checked)
            {
                _TargetNetworkInterface.EnableAcceleratedNetworking = true;

                this.RaisePropertyChangedEvent(_TargetNetworkInterface);
            }
        }

        private void rbAcceleratedNetworkingDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAcceleratedNetworkingDisabled.Checked)
            {
                _TargetNetworkInterface.EnableAcceleratedNetworking = false;

                this.RaisePropertyChangedEvent(_TargetNetworkInterface);
            }
        }

        private void rbPublicIpDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPublicIpDisabled.Checked)
            {
                try
                {
                    this.IsBinding = true;

                    if (_TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations.Count > 0)
                        _TargetNetworkInterface.TargetNetworkInterfaceIpConfigurations[0].TargetPublicIp = null;

                    publicIpSelectionControl1.PublicIp = null;
                    publicIpSelectionControl1.Enabled = false;
                }
                finally
                {
                    this.IsBinding = false;
                    this.RaisePropertyChangedEvent(_TargetNetworkInterface);
                }
            }
        }

        private void rbPublicIpEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPublicIpEnabled.Checked)
            {
                publicIpSelectionControl1.Enabled = true;

                this.RaisePropertyChangedEvent(_TargetNetworkInterface);
            }
        }
    }
}

