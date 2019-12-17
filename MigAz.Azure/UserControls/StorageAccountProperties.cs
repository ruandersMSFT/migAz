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
using MigAz.Azure;
using MigAz.Azure.Core.Interface;
using MigAz.Azure.Core;
using MigAz.Azure.MigrationTarget;
using MigAz.Azure.Interface;

namespace MigAz.Azure.UserControls
{
    public partial class StorageAccountProperties : TargetPropertyControl
    {
        private StorageAccount _StorageAccount;

        public StorageAccountProperties()
        {
            InitializeComponent();
        }

        public void Bind(StorageAccount storageAccount, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;
                _TargetTreeView = targetTreeView;
                _StorageAccount = storageAccount;
                txtTargetName.MaxLength = StorageAccount.MaximumTargetNameLength(targetTreeView.TargetSettings);

                if (_StorageAccount.Source == null)
                {
                    lblAccountType.Text = String.Empty;
                    lblSourceASMName.Text = String.Empty;
                }
                else
                {
                    lblAccountType.Text = ((IStorageAccount)_StorageAccount.Source).AccountType;
                    lblSourceASMName.Text = ((IStorageAccount)_StorageAccount.Source).Name;
                }

                if (storageAccount.TargetName != null)
                {
                    if (storageAccount.TargetName.Length > txtTargetName.MaxLength)
                        txtTargetName.Text = storageAccount.TargetName.Substring(0, txtTargetName.MaxLength);
                    else
                        txtTargetName.Text = storageAccount.TargetName;
                }

                cmbAccountType.SelectedIndex = cmbAccountType.FindString(storageAccount.StorageAccountType.ToString());
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;
            _StorageAccount.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_StorageAccount);
        }

        private void txtTargetName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void cmbAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAccountType.SelectedItem.ToString() == "Premium_LRS")
                _StorageAccount.StorageAccountType = StorageAccountType.Premium_LRS;
            else
                _StorageAccount.StorageAccountType = StorageAccountType.Standard_LRS;

            this.RaisePropertyChangedEvent(_StorageAccount);
        }
    }
}

