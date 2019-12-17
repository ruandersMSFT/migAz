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
    public partial class ApplicationSecurityGroupProperties : TargetPropertyControl
    {
        ApplicationSecurityGroup _ApplicationSecurityGroup;

        public ApplicationSecurityGroupProperties()
        {
            InitializeComponent();
        }

        internal void Bind(ApplicationSecurityGroup applicationSecurityGroup, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;

                _TargetTreeView = targetTreeView;
                _ApplicationSecurityGroup = applicationSecurityGroup;

                txtTargetName.Text = _ApplicationSecurityGroup.TargetName;
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            _ApplicationSecurityGroup.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_ApplicationSecurityGroup);
        }

    }
}

