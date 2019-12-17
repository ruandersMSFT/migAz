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
using MigAz.Azure.Interface;

namespace MigAz.Azure.UserControls
{
    public partial class DiskProperties : TargetPropertyControl
    {
        Disk _TargetDisk;
        bool _ShowSizeInGb = true;

        public DiskProperties()
        {
            InitializeComponent();
        }

        public bool ShowSizeInGb
        {
            get
            {
                return _ShowSizeInGb;
            }
            set
            {
                _ShowSizeInGb = value;

                lblSourceSizeGb.Visible = _ShowSizeInGb;
                txtTargetSize.Visible = _ShowSizeInGb;
            }
        }

        public IStorageAccount TargetStorageAccount
        {
            get
            {
                if (cmbTargetStorage.SelectedItem == null)
                    return null;
                else
                    return (IStorageAccount)cmbTargetStorage.SelectedItem;
            }
        }

        internal async Task Bind(Disk targetDisk, TargetTreeView targetTreeView)
        {
            _TargetTreeView = targetTreeView;
            _TargetDisk = targetDisk;

            // LUN Index cannot be higher than the number of Data Disks supported by the Azure VM Size, 0 based index, thus - 1
            if (targetDisk.ParentVirtualMachine == null || targetDisk.ParentVirtualMachine.TargetSize == null)
                upDownLUN.Maximum = 0;
            else
                upDownLUN.Maximum = targetDisk.ParentVirtualMachine.TargetSize.maxDataDiskCount - 1;

            await BindCommon();
        }

        private async Task BindCommon()
        {
            if (_TargetDisk == null)
                throw new ArgumentException("MigrationTarget Disk object cannot be null.");

            this.IsBinding = true;

            if (_TargetDisk.TargetStorage != null && _TargetDisk.TargetStorage.GetType() == typeof(Azure.MigrationTarget.ManagedDiskStorage))
                rbManagedDisk.Checked = true;
            if (_TargetDisk.TargetStorage != null && _TargetDisk.TargetStorage.GetType() == typeof(Azure.MigrationTarget.StorageAccount))
                rbStorageAccountInMigration.Checked = true;
            else if (_TargetDisk.TargetStorage != null && _TargetDisk.TargetStorage.GetType() == typeof(Azure.Arm.StorageAccount))
                rbExistingARMStorageAccount.Checked = true;

            txtTargetDiskName.Text = _TargetDisk.TargetName;
            txtBlobName.Text = _TargetDisk.TargetStorageAccountBlob;

            txtTargetSize.Text = _TargetDisk.DiskSizeInGB.ToString();
            txtDiskEncryptionKeySecretUrl.Text = _TargetDisk.DiskEncryptionKeySecretUrl;
            txtDiskEncryptionKeySourceVaultId.Text = _TargetDisk.DiskEncryptionKeySourceVaultId;
            txtKeyEncryptionKeyKeyUrl.Text = _TargetDisk.KeyEncryptionKeyKeyUrl;
            txtKeyEncryptionKeySourceVaultId.Text = _TargetDisk.KeyEncryptionKeySourceVaultId;

            lblAsmStorageAccount.Text = String.Empty;
            if (_TargetDisk.Source != null)
            {
                IDisk sourceDisk = (IDisk)_TargetDisk.Source;

                lblDiskName.Text = _TargetDisk.TargetName;
                lblHostCaching.Text = sourceDisk.HostCaching;
                lblLUN.Text = sourceDisk.Lun.ToString();
                lblSourceSizeGb.Text = sourceDisk.DiskSizeGb.ToString();

                if (sourceDisk.GetType() == typeof(Azure.Asm.Disk))
                {
                    Azure.Asm.Disk asmDisk = (Azure.Asm.Disk)sourceDisk;
                    if (asmDisk.SourceStorageAccount != null)
                        lblAsmStorageAccount.Text = asmDisk.SourceStorageAccount.Name;
                }
                else if (sourceDisk.GetType() == typeof(Azure.Arm.ClassicDisk))
                {
                    Azure.Arm.ClassicDisk armDisk = (Azure.Arm.ClassicDisk)sourceDisk;
                    if (armDisk.SourceStorageAccount != null)
                        lblAsmStorageAccount.Text = armDisk.SourceStorageAccount.Name;
                }
            }
            else
            {
                lblDiskName.Text = String.Empty;
                lblHostCaching.Text = String.Empty;
                lblLUN.Text = String.Empty;
                lblSourceSizeGb.Text = String.Empty;
                lblAsmStorageAccount.Text = String.Empty;
            }

            if (_TargetTreeView.TargetResourceGroup != null && _TargetTreeView.TargetResourceGroup.TargetLocation != null)
            {
                rbExistingARMStorageAccount.Text = "Existing Storage in " + _TargetTreeView.TargetResourceGroup.TargetLocation.DisplayName;
                rbExistingARMStorageAccount.Enabled = _TargetTreeView.GetExistingArmStorageAccounts().Count > 0;
            }
            else
            {
                // Cannot use existing ARM Storage without Target Location
                rbExistingARMStorageAccount.Enabled = false;
                rbExistingARMStorageAccount.Text = "<Set Resource Group Location>";
            }

            if (!_TargetTreeView.HasStorageAccount)
            {
                rbStorageAccountInMigration.Enabled = false;
            }

            upDownLUN.Visible = _TargetDisk.Lun.HasValue;
            upDownLUN.Enabled = _TargetDisk.Lun.HasValue;
            if (_TargetDisk.Lun.HasValue)
            {
                // There is a chance that the assigned LUN Value exceeds the Max Allowed LUN Value.
                // This can happen, for example, if the Disk LUN was index 15 and the VM is resized from DS3 to DS2
                // thus exceeding the Max allowed.  In this case, we're going to temporarily increase the Max value
                // to avoid value assignment error, and then handle downsizing / scaling back the Max allowed value
                // as the value is decreased (max value reassignment to occur in the UpDown ValueChanged Event).
                // We are utilizing the MigAz Error validation here to tell the user their LUN index exceeds that of the VM Size.
                if (upDownLUN.Maximum < _TargetDisk.Lun.Value)
                {
                    // Since it exceeds, we'll allow the Max value increase, and downsize in the UpDown ValueChanged Event
                    upDownLUN.Maximum = _TargetDisk.Lun.Value;
                }

                if (_TargetDisk.Lun.Value == -1)
                    upDownLUN.Minimum = -1;
                else
                    upDownLUN.Minimum = 0;

                upDownLUN.Value = _TargetDisk.Lun.Value;
            }

            virtualMachineSummary.Bind(_TargetDisk.ParentVirtualMachine, _TargetTreeView, false);

            this.IsBinding = false;
        }

        private void rbManagedDIsk_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton senderButton = (RadioButton)sender;
            if (senderButton.Checked)
            {
                cmbTargetStorage.Items.Clear();
                cmbTargetStorage.Enabled = true;
                cmbTargetStorage.Items.Add("Standard_LRS");
                cmbTargetStorage.Items.Add("Premium_LRS");

                txtBlobName.Enabled = false;
                txtBlobName.Text = String.Empty;

                if (this.IsBinding)
                {
                    int comboBoxIndex = cmbTargetStorage.Items.IndexOf(_TargetDisk.TargetStorage.StorageAccountType.ToString());
                    if (comboBoxIndex >= 0)
                        cmbTargetStorage.SelectedIndex = comboBoxIndex;
                }
                else
                {
                    _TargetDisk.TargetStorage = new ManagedDiskStorage((IDisk)_TargetDisk.Source);
                    _TargetTreeView.TransitionToManagedDisk(_TargetDisk);

                    int comboBoxIndex = cmbTargetStorage.Items.IndexOf(_TargetDisk.TargetStorage.StorageAccountType.ToString());
                    if (comboBoxIndex >= 0)
                        cmbTargetStorage.SelectedIndex = comboBoxIndex;
                }

                this.RaisePropertyChangedEvent(_TargetDisk);
            }
        }

        private void rbStorageAccountInMigration_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rbSender = (RadioButton)sender;

            if (rbSender.Checked)
            {
                if (_TargetDisk.TargetStorage != null && _TargetDisk.TargetStorage.GetType() != typeof(Azure.MigrationTarget.StorageAccount))
                    _TargetDisk.TargetStorage = null;

                cmbTargetStorage.Items.Clear();
                cmbTargetStorage.Enabled = true;
                txtBlobName.Enabled = true;
                txtBlobName.Text = _TargetDisk.TargetStorageAccountBlob;

                TreeNode targetResourceGroupNode = _TargetTreeView.ResourceGroupNode;

                foreach (TreeNode treeNode in targetResourceGroupNode.Nodes)
                {
                    if (treeNode.Tag != null && treeNode.Tag.GetType() == typeof(Azure.MigrationTarget.StorageAccount))
                    {
                        Azure.MigrationTarget.StorageAccount storageAccountTarget = (Azure.MigrationTarget.StorageAccount)treeNode.Tag;
                        cmbTargetStorage.Items.Add(storageAccountTarget);
                    }
                }

                if (this.IsBinding)
                {
                    if (_TargetDisk.TargetStorage != null)
                    {
                        for (int i = 0; i < cmbTargetStorage.Items.Count; i++)
                        {
                            Azure.MigrationTarget.StorageAccount cmbStorageAccount = (Azure.MigrationTarget.StorageAccount)cmbTargetStorage.Items[i];
                            if (cmbStorageAccount.ToString() == _TargetDisk.TargetStorage.ToString())
                            {
                                cmbTargetStorage.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
                else if (_TargetDisk != null)
                {
                    if (_TargetDisk.TargetStorage != null)
                    {
                        for (int i = 0; i < cmbTargetStorage.Items.Count; i++)
                        {
                            Azure.MigrationTarget.StorageAccount cmbStorageAccount = (Azure.MigrationTarget.StorageAccount)cmbTargetStorage.Items[i];
                            if (cmbStorageAccount.ToString() == _TargetDisk.TargetStorage.ToString())
                            {
                                cmbTargetStorage.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Default
                        if (_TargetDisk.SourceStorageAccount != null)
                        {
                            foreach (Azure.MigrationTarget.StorageAccount targetStorageAccount in cmbTargetStorage.Items)
                            {
                                if (targetStorageAccount.SourceName == _TargetDisk.SourceStorageAccount.Name)
                                {
                                    cmbTargetStorage.SelectedIndex = cmbTargetStorage.Items.IndexOf(targetStorageAccount);
                                    break;
                                }
                            }
                        }

                        if (cmbTargetStorage.SelectedIndex == -1 && cmbTargetStorage.Items.Count > 0)
                            cmbTargetStorage.SelectedIndex = 0;
                    }

                    _TargetTreeView.TransitionToClassicDisk(_TargetDisk);

                    this.RaisePropertyChangedEvent(_TargetDisk);
                }
            }
        }

        private async void rbExistingARMStorageAccount_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rbSender = (RadioButton)sender;

            if (rbSender.Checked)
            {
                if (_TargetDisk.TargetStorage != null && _TargetDisk.TargetStorage.GetType() != typeof(Arm.StorageAccount))
                    _TargetDisk.TargetStorage = null;

                cmbTargetStorage.Items.Clear();
                cmbTargetStorage.Enabled = true;
                txtBlobName.Enabled = true;
                txtBlobName.Text = _TargetDisk.TargetStorageAccountBlob;

                foreach (Arm.StorageAccount armStorageAccount in _TargetTreeView.GetExistingArmStorageAccounts())
                {
                    cmbTargetStorage.Items.Add(armStorageAccount);
                }

                if (this.IsBinding)
                {
                    if (_TargetDisk.TargetStorage != null)
                    {
                        if (_TargetDisk.TargetStorage.GetType() == typeof(Azure.Arm.StorageAccount))
                        {
                            for (int i = 0; i < cmbTargetStorage.Items.Count; i++)
                            {
                                if (cmbTargetStorage.Items[i].GetType() == typeof(Azure.Arm.StorageAccount))
                                {
                                    Azure.Arm.StorageAccount cmbStorageAccount = (Azure.Arm.StorageAccount)cmbTargetStorage.Items[i];
                                    if (cmbStorageAccount.ToString() == _TargetDisk.TargetStorage.ToString())
                                    {
                                        cmbTargetStorage.SelectedIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                _TargetTreeView.TransitionToClassicDisk(_TargetDisk);

                this.RaisePropertyChangedEvent(_TargetDisk);
            }
        }

        private void cmbTargetStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmbSender = (ComboBox)sender;

            if (_TargetDisk != null)
            {
                if (_TargetDisk.TargetStorage == null && cmbSender.SelectedItem != null)
                {
                    if (cmbSender.SelectedItem.GetType() == typeof(Azure.MigrationTarget.StorageAccount))
                    {
                        _TargetDisk.TargetStorage = (Azure.MigrationTarget.StorageAccount)cmbSender.SelectedItem;
                    }
                    else if (cmbSender.SelectedItem.GetType() == typeof(Azure.Arm.StorageAccount))
                    {
                        _TargetDisk.TargetStorage = (Azure.Arm.StorageAccount)cmbSender.SelectedItem;
                    }
                }


                if (_TargetDisk.TargetStorage.GetType() == typeof(Azure.MigrationTarget.ManagedDiskStorage))
                {
                    ManagedDiskStorage managedDiskStorage = (ManagedDiskStorage)_TargetDisk.TargetStorage;
                    if (cmbSender.SelectedItem.ToString() == "Premium_LRS")
                        managedDiskStorage.StorageAccountType = StorageAccountType.Premium_LRS;
                    else
                        managedDiskStorage.StorageAccountType = StorageAccountType.Standard_LRS;
                }
                else
                {
                    if (cmbSender.SelectedItem != null)
                    {
                        IStorageTarget targetStorageAccount = (IStorageTarget)cmbSender.SelectedItem;
                        _TargetDisk.TargetStorage = targetStorageAccount;
                    }
                    else
                        _TargetDisk.TargetStorage = null;
                }

                this.RaisePropertyChangedEvent(_TargetDisk);
            }
        }

        private void txtTargetDiskName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            _TargetDisk.SetTargetName(txtSender.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_TargetDisk);
        }

        private void txtBlobName_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSender = (TextBox)sender;

            if (rbExistingARMStorageAccount.Checked || rbStorageAccountInMigration.Checked) // We don't want to call / save text change on a Managed Disk, specifically when set to String.Empty would loose text value
            {
                _TargetDisk.TargetStorageAccountBlob = txtSender.Text.Trim();
                this.RaisePropertyChangedEvent(_TargetDisk);
            }
        }

        private void txtTargetDiskName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtTargetSize_TextChanged(object sender, EventArgs e)
        {
            if (_TargetDisk != null)
            {
                Int32 targetSizeInGb = -1;
                Int32.TryParse(txtTargetSize.Text, out targetSizeInGb);

                try
                {
                    _TargetDisk.DiskSizeInGB = targetSizeInGb;
                }
                catch (Exception exc)
                {
                    _TargetDisk.DiskSizeInGB = 0;
                }

                this.RaisePropertyChangedEvent(_TargetDisk);
            }
        }

        private void txtTargetSize_Validating(object sender, CancelEventArgs e)
        {
            Int32 targetSizeInGb = -1;
            Int32.TryParse(txtTargetSize.Text, out targetSizeInGb);

            if (targetSizeInGb <= 0)
            {
                MessageBox.Show("Invalid Target Size in GB.");
                e.Cancel = true;
                return;
            }

            if (targetSizeInGb > 4095)
            {
                MessageBox.Show("Target Size in GB cannot exceed 4,095.");
                e.Cancel = true;
                return;
            }
        }

        private void upDownLUN_ValueChanged(object sender, EventArgs e)
        {
            if (_TargetDisk != null)
            {
                if (!this.IsBinding)
                {
                    _TargetDisk.Lun = (long)upDownLUN.Value;

                    if (_TargetDisk.ParentVirtualMachine != null && upDownLUN.Maximum > _TargetDisk.ParentVirtualMachine.TargetSize.maxDataDiskCount - 1)
                    {
                        // Downsize the UpDown control Max value, as the user downsizes the value (do not allow re-increase, but do allow user to scale down into allowable range of Vm Size)
                        upDownLUN.Maximum = upDownLUN.Value;
                    }

                    if (upDownLUN.Minimum == -1 && _TargetDisk.Lun >= 0)
                    {
                        upDownLUN.Minimum = 0;
                    }

                    this.RaisePropertyChangedEvent(_TargetDisk);
                }
            }
        }

        private void txtDiskEncryptionKeySecretUrl_TextChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChangedEvent(_TargetDisk);
        }

        private void txtDiskEncryptionKeySourceVaultId_TextChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChangedEvent(_TargetDisk);
        }

        private void txtKeyEncryptionKeyKeyUrl_TextChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChangedEvent(_TargetDisk);
        }

        private void txtKeyEncryptionKeySourceVaultId_TextChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChangedEvent(_TargetDisk);
        }
    }
}

