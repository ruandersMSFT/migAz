// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MigAz.Azure.Core.Interface;
using System.Collections.Generic;
using MigAz.Azure.MigrationTarget;
using System.Linq;

namespace MigAz.Azure.UserControls
{
    public partial class VirtualMachineProperties : TargetPropertyControl
    {
        private VirtualMachine _VirtualMachine;

        public VirtualMachineProperties()
        {
            InitializeComponent();
        }

        public async Task Bind(VirtualMachine virtualMachine, TargetTreeView targetTreeView)
        {
            try
            {
                this.IsBinding = true;
                _TargetTreeView = targetTreeView;
                _VirtualMachine = virtualMachine;

                txtTargetName.Text = _VirtualMachine.TargetName;

                if (_VirtualMachine.Source != null)
                {
                    if (_VirtualMachine.Source.GetType() == typeof(Azure.Asm.VirtualMachine))
                    {
                        Azure.Asm.VirtualMachine asmVirtualMachine = (Azure.Asm.VirtualMachine)_VirtualMachine.Source;

                        if (asmVirtualMachine.RoleSize != null)
                        {
                            lblRoleSize.Text = asmVirtualMachine.RoleSize.Name;
                            lblSourceCPUCores.Text = asmVirtualMachine.RoleSize.Cores.ToString();
                            lblSourceMemoryInGb.Text = ((double)asmVirtualMachine.RoleSize.MemoryInMb / 1024).ToString();
                            lblSourceMaxDataDisks.Text = asmVirtualMachine.RoleSize.MaxDataDiskCount.ToString();
                        }

                        lblOS.Text = asmVirtualMachine.OSVirtualHardDiskOS;
                    }
                    else if (_VirtualMachine.Source.GetType() == typeof(Azure.Arm.VirtualMachine))
                    {
                        Azure.Arm.VirtualMachine armVirtualMachine = (Azure.Arm.VirtualMachine)_VirtualMachine.Source;

                        if (armVirtualMachine.VmSize != null)
                        {
                            lblRoleSize.Text = armVirtualMachine.VmSize.ToString();
                            lblSourceCPUCores.Text = armVirtualMachine.VmSize.NumberOfCores.ToString();
                            lblSourceMemoryInGb.Text = ((double)armVirtualMachine.VmSize.memoryInMB / 1024).ToString();
                            lblSourceMaxDataDisks.Text = armVirtualMachine.VmSize.maxDataDiskCount.ToString();
                        }

                        lblOS.Text = armVirtualMachine.OSVirtualHardDiskOS;
                    }
                }

                cbRoleSizes.Items.Clear();
                if (targetTreeView.TargetResourceGroup != null && targetTreeView.TargetResourceGroup.TargetLocation != null)
                {
                    cbRoleSizes.Enabled = true;
                    cbRoleSizes.Visible = true;
                    lblTargetLocationRequired.Enabled = false;
                    lblTargetLocationRequired.Visible = false;

                    if (targetTreeView.TargetResourceGroup.TargetLocation.VMSizes != null && _VirtualMachine.OSVirtualHardDisk != null)
                    {
                        foreach (Arm.VMSize vmSize in targetTreeView.TargetResourceGroup.TargetLocation.VMSizes)
                        {
                            if (vmSize.IsStorageTypeSupported(_VirtualMachine.OSVirtualHardDisk.StorageAccountType))
                            {
                                cbRoleSizes.Items.Add(vmSize);
                            }
                        }
                    }

                    if (_VirtualMachine.TargetSize != null)
                    {
                        int sizeIndex = cbRoleSizes.FindStringExact(_VirtualMachine.TargetSize.ToString());
                        cbRoleSizes.SelectedIndex = sizeIndex;
                    }
                }
                else
                {
                    cbRoleSizes.Enabled = false;
                    cbRoleSizes.Visible = false;
                    lblTargetLocationRequired.Enabled = true;
                    lblTargetLocationRequired.Visible = true;
                }

                availabilitySetSummary.Bind(virtualMachine.TargetAvailabilitySet, _TargetTreeView, true, true, _TargetTreeView.ExportArtifacts.AvailablitySets.Cast<Core.MigrationTarget>().ToList());
                osDiskSummary.Bind(virtualMachine.OSVirtualHardDisk, _TargetTreeView, false, false, null);
                primaryNICSummary.Bind(virtualMachine.PrimaryNetworkInterface, _TargetTreeView, false, false, null);

                foreach (Disk targetDisk in virtualMachine.DataDisks)
                {
                    AddResourceSummary(new ResourceSummary(targetDisk, targetTreeView));
                }
                foreach (NetworkInterface targetNIC in virtualMachine.NetworkInterfaces)
                {
                    if (!targetNIC.IsPrimary)
                        AddResourceSummary(new ResourceSummary(targetNIC, targetTreeView));
                }

                label15.Visible = pictureBox1.Controls.Count > 0;
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        private void AddResourceSummary(ResourceSummary resourceSummary)
        {
            if (pictureBox1.Controls.Count > 0)
            {
                resourceSummary.Top = pictureBox1.Controls[pictureBox1.Controls.Count - 1].Top + pictureBox1.Controls[pictureBox1.Controls.Count - 1].Height;
            }

            pictureBox1.Controls.Add(resourceSummary);
        }

        private void txtTargetName_TextChanged(object sender, EventArgs e)
        {
            _VirtualMachine.SetTargetName(txtTargetName.Text, _TargetTreeView.TargetSettings);

            this.RaisePropertyChangedEvent(_VirtualMachine);
        }

        private void txtTargetName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void cbRoleSizes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbRoleSizes.SelectedItem != null)
            {
                Arm.VMSize selectedSize = (Arm.VMSize)cbRoleSizes.SelectedItem;

                lblTargetNumberOfCores.Text = selectedSize.NumberOfCores.ToString();
                lblTargetMemoryInGb.Text = ((double)selectedSize.memoryInMB / 1024).ToString();
                lblTargetMaxDataDisks.Text = selectedSize.maxDataDiskCount.ToString();

                _VirtualMachine.TargetSize = selectedSize;
            }
            else
                _VirtualMachine.TargetSize = null;

            this.RaisePropertyChangedEvent(_VirtualMachine);
        }

        private async Task availabilitySetSummary_AfterMigrationTargetChanged(ResourceSummary sender, Core.MigrationTarget selectedResource)
        {
            _VirtualMachine.TargetAvailabilitySet = (AvailabilitySet)selectedResource;

            this.RaisePropertyChangedEvent(_VirtualMachine);
        }
    }
}

