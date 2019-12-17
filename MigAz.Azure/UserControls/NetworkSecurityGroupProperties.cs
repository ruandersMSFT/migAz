// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MigAz.Azure.MigrationTarget;

namespace MigAz.Azure.UserControls
{
    public partial class NetworkSecurityGroupProperties : TargetPropertyControl
    {

        private NetworkSecurityGroup _NetworkSecurityGroup;

        public NetworkSecurityGroupProperties()
        {
            InitializeComponent();
        }

        internal void Bind(NetworkSecurityGroup networkSecurityGroup, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;
                _NetworkSecurityGroup = networkSecurityGroup;
                _TargetTreeView = targetTreeView;

                if (_NetworkSecurityGroup.Source != null)
                {
                    if (_NetworkSecurityGroup.Source.GetType() == typeof(Azure.Asm.NetworkSecurityGroup))
                    {
                        Azure.Asm.NetworkSecurityGroup asmNetworkSecurityGroup = (Azure.Asm.NetworkSecurityGroup)_NetworkSecurityGroup.Source;
                        lblSourceName.Text = asmNetworkSecurityGroup.Name;
                    }
                    else if (_NetworkSecurityGroup.Source.GetType() == typeof(Azure.Arm.NetworkSecurityGroup))
                    {
                        Azure.Arm.NetworkSecurityGroup armNetworkSecurityGroup = (Azure.Arm.NetworkSecurityGroup)_NetworkSecurityGroup.Source;
                        lblSourceName.Text = armNetworkSecurityGroup.Name;
                    }
                }

                txtTargetName.Text = _NetworkSecurityGroup.TargetName;
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            _NetworkSecurityGroup.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_NetworkSecurityGroup);
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

