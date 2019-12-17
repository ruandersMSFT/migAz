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
    public partial class SubnetProperties : TargetPropertyControl
    {
        private MigrationTarget.Subnet _Subnet;

        public SubnetProperties()
        {
            InitializeComponent();
        }

        internal void Bind(Subnet targetSubnet, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;
                _TargetTreeView = targetTreeView;
                _Subnet = targetSubnet;

                txtTargetName.Text = targetSubnet.TargetName;
                txtAddressSpace.Text = targetSubnet.AddressPrefix;

                if (targetSubnet.Source != null)
                {
                    if (targetSubnet.Source.GetType() == typeof(Azure.Asm.Subnet))
                    {
                        Asm.Subnet asmSubnet = (Asm.Subnet)targetSubnet.Source;

                        lblSourceName.Text = asmSubnet.Name;
                        lblAddressSpace.Text = asmSubnet.AddressPrefix;
                    }
                    else if (targetSubnet.Source.GetType() == typeof(Azure.Arm.Subnet))
                    {
                        Arm.Subnet armSubnet = (Arm.Subnet)targetSubnet.Source;

                        lblSourceName.Text = armSubnet.Name;
                        lblAddressSpace.Text = armSubnet.AddressPrefix;
                    }
                }

                if (String.Compare(txtTargetName.Text, Core.ArmTemplate.ArmConst.GatewaySubnetName, true) == 0)
                {
                    // if gateway subnet, the name can't be changed
                    txtTargetName.Enabled = false;
                }

                // todo now russell
                //networkSecurityGroup.Bind(_Subnet.NetworkSecurityGroup, _TargetTreeView);
                //routeTable.Bind(_Subnet.RouteTable, _TargetTreeView);
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            _Subnet.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_Subnet);
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

