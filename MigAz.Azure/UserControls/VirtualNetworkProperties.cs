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
using MigAz.Azure.MigrationTarget;

namespace MigAz.Azure.UserControls
{
    public partial class VirtualNetworkProperties : TargetPropertyControl
    {
        private VirtualNetwork _VirtualNetwork;

        public VirtualNetworkProperties()
        {
            InitializeComponent();
        }

        public void Bind(VirtualNetwork targetVirtualNetwork, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;
                _VirtualNetwork = targetVirtualNetwork;
                _TargetTreeView = targetTreeView;

                if (targetVirtualNetwork.Source != null)
                {
                    if (targetVirtualNetwork.Source.GetType() == typeof(Azure.Asm.VirtualNetwork))
                    {
                        Azure.Asm.VirtualNetwork asmVirtualNetwork = (Azure.Asm.VirtualNetwork)targetVirtualNetwork.Source;
                        lblVNetName.Text = asmVirtualNetwork.Name;
                    }
                    else if (targetVirtualNetwork.Source.GetType() == typeof(Azure.Arm.VirtualNetwork))
                    {
                        Azure.Arm.VirtualNetwork armVirtualNetwork = (Azure.Arm.VirtualNetwork)targetVirtualNetwork.Source;
                        lblVNetName.Text = armVirtualNetwork.Name;
                    }
                }
                else
                    lblVNetName.Text = "(None)";

                txtVirtualNetworkName.Text = targetVirtualNetwork.TargetName;
                dgvAddressSpaces.DataSource = targetVirtualNetwork.AddressPrefixes.Select(x => new { AddressPrefix = x }).ToList();
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            _VirtualNetwork.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings); ;

            this.RaisePropertyChangedEvent(_VirtualNetwork);
        }

        private void txtTargetName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}

