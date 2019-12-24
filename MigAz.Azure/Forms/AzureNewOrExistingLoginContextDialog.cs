// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MigAz.Azure.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigAz.Azure.Forms
{
    public partial class AzureNewOrExistingLoginContextDialog : Form
    {
        private bool _IsInitializing = false;
        private AzureLoginContextViewer _AzureLoginContextViewer;
        private List<AzureEnvironment> _AzureEnvironments;
        private List<AzureEnvironment> _UserDefinedAzureEnvironments;

        public AzureNewOrExistingLoginContextDialog()
        {
            InitializeComponent();
        }

        public async Task InitializeDialog(AzureLoginContextViewer azureLoginContextViewer, List<AzureEnvironment> azureEnvironments, List<AzureEnvironment> userDefinedAzureEnvironments)
        {
            try {
                _IsInitializing = true;

                _AzureLoginContextViewer = azureLoginContextViewer;
                _AzureEnvironments = azureEnvironments;
                _UserDefinedAzureEnvironments = userDefinedAzureEnvironments;

                await azureArmLoginControl1.BindContext(azureLoginContextViewer.AzureContext, azureEnvironments, userDefinedAzureEnvironments);

                azureLoginContextViewer.ExistingContext.LogProvider.WriteLog("InitializeDialog", "Start AzureSubscriptionContextDialog InitializeDialog");

                lblSameEnviroronment.Text = azureLoginContextViewer.ExistingContext.AzureEnvironment.ToString();
                lblSameTenant.Text = azureLoginContextViewer.ExistingContext.AzureTenant.ToString();
                lblSameSubscription.Text = azureLoginContextViewer.ExistingContext.AzureSubscription.ToString();

                lblSameEnvironment2.Text = azureLoginContextViewer.ExistingContext.AzureEnvironment.ToString();
                if (azureLoginContextViewer.ExistingContext.TokenProvider != null && azureLoginContextViewer.ExistingContext.TokenProvider.LastAccount != null)
                {
                    lblSameUsername.Text = azureLoginContextViewer.ExistingContext.TokenProvider.LastAccount.Username;
                    lblSameUsername2.Text = azureLoginContextViewer.ExistingContext.TokenProvider.LastAccount.Username;
                }

                int subscriptionCount = 0;
                cboTenant.Items.Clear();
                if (azureLoginContextViewer.ExistingContext.AzureRetriever != null && azureLoginContextViewer.ExistingContext.TokenProvider != null)
                {
                    azureLoginContextViewer.ExistingContext.LogProvider.WriteLog("InitializeDialog", "Loading Azure Tenants");
                    foreach (AzureTenant azureTenant in await azureLoginContextViewer.ExistingContext.GetAzureARMTenants())
                    {
                        subscriptionCount += azureTenant.Subscriptions.Count;

                        if (azureTenant.Subscriptions.Count > 0) // Only add Tenants that have one or more Subscriptions
                        {
                            if (azureTenant.Subscriptions.Count == 1 && azureTenant.Subscriptions[0] == azureLoginContextViewer.ExistingContext.AzureSubscription)
                            {
                                azureLoginContextViewer.ExistingContext.LogProvider.WriteLog("InitializeDialog", "Not adding Azure Tenant '" + azureTenant.ToString() + "', as it has only one subscription, which is the same as the Existing Azure Context.");
                            }
                            else
                            {
                                cboTenant.Items.Add(azureTenant);
                                azureLoginContextViewer.ExistingContext.LogProvider.WriteLog("InitializeDialog", "Added Azure Tenant '" + azureTenant.ToString() + "'");
                            }
                        }
                        else
                        {
                            azureLoginContextViewer.ExistingContext.LogProvider.WriteLog("InitializeDialog", "Not adding Azure Tenant '" + azureTenant.ToString() + "'.  Contains no subscriptions.");
                        }
                    }
                    cboTenant.Enabled = true;

                    if (azureLoginContextViewer.SelectedAzureContext != null && azureLoginContextViewer.SelectedAzureContext.AzureTenant != null)
                    {
                        foreach (AzureTenant azureTenant in cboTenant.Items)
                        {
                            if (azureLoginContextViewer.SelectedAzureContext.AzureTenant == azureTenant)
                                cboTenant.SelectedItem = azureTenant;
                        }
                    }
                }

                rbSameUserDifferentSubscription.Enabled = subscriptionCount > 1;

                switch (azureLoginContextViewer.AzureContextSelectedType)
                {
                    case AzureContextSelectedType.ExistingContext:
                        rbExistingContext.Checked = true;
                        break;
                    case AzureContextSelectedType.SameUserDifferentSubscription:
                        if (rbSameUserDifferentSubscription.Enabled)
                            rbSameUserDifferentSubscription.Checked = true;
                        else
                            rbExistingContext.Checked = true;
                        break;
                    case AzureContextSelectedType.NewContext:
                        rbNewContext.Checked = true;
                        break;
                }

                azureLoginContextViewer.ExistingContext.LogProvider.WriteLog("InitializeDialog", "End AzureSubscriptionContextDialog InitializeDialog");
            }
            finally
            {
                _IsInitializing = false;
            }

            if (rbSameUserDifferentSubscription.Checked && cboTenant.SelectedIndex == -1 && cboTenant.Items.Count > 0)
            {
                cboTenant.SelectedIndex = 0;
            }

            azureLoginContextViewer.ExistingContext.StatusProvider.UpdateStatus("Ready");
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rbExistingContext_CheckedChanged(object sender, EventArgs e)
        {
            if (rbExistingContext.Checked)
            {
                cboTenant.Enabled = false;
                cboSubscription.Enabled = false;
                _AzureLoginContextViewer.AzureContextSelectedType = AzureContextSelectedType.ExistingContext;
                _AzureLoginContextViewer.AzureContext.LoginPromptBehavior = PromptBehavior.Auto;
            }

            _AzureLoginContextViewer.UpdateLabels();
        }

        private async void rbSameUserDifferentSubscription_CheckedChanged(object sender, EventArgs e)
        {
            if (!_IsInitializing)
            {
                if (rbSameUserDifferentSubscription.Checked)
                {
                    _AzureLoginContextViewer.AzureContext.TokenProvider.LastAccount = _AzureLoginContextViewer.ExistingContext.TokenProvider.LastAccount;
                    _AzureLoginContextViewer.AzureContext.AzureEnvironment = _AzureLoginContextViewer.ExistingContext.AzureEnvironment;
                    _AzureLoginContextViewer.AzureContextSelectedType = AzureContextSelectedType.SameUserDifferentSubscription;
                    _AzureLoginContextViewer.AzureContext.LoginPromptBehavior = PromptBehavior.Auto;


                    if (cboTenant.SelectedIndex == -1 && cboTenant.Items.Count > 0)
                    {
                        cboTenant.SelectedIndex = 0;
                    }

                    if (cboTenant.SelectedItem != null)
                    {
                        AzureTenant azureTenant = (AzureTenant)cboTenant.SelectedItem;
                        if (_AzureLoginContextViewer.AzureContext.AzureTenant != azureTenant)
                        {
                            await _AzureLoginContextViewer.AzureContext.SetTenantContext(azureTenant);
                        }
                    }

                    if (cboSubscription.SelectedIndex == -1 && cboSubscription.Items.Count > 0)
                    {
                        cboSubscription.SelectedIndex = 0;
                    }

                    if (cboSubscription.SelectedItem != null)
                    {
                        AzureSubscription azureSubscription = (AzureSubscription)cboSubscription.SelectedItem;
                        if (_AzureLoginContextViewer.AzureContext.AzureSubscription != azureSubscription)
                        {
                            await _AzureLoginContextViewer.AzureContext.SetSubscriptionContext(azureSubscription);
                        }
                    }

                }
            }

            await azureArmLoginControl1.BindContext(_AzureLoginContextViewer.AzureContext, _AzureEnvironments, _UserDefinedAzureEnvironments);
            cboTenant.Enabled = rbSameUserDifferentSubscription.Checked;
            cboSubscription.Enabled = rbSameUserDifferentSubscription.Checked;
            _AzureLoginContextViewer.UpdateLabels();
        }

        private async void rbNewContext_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNewContext.Checked)
            {
                cboTenant.Enabled = false;
                cboSubscription.Enabled = false;
                _AzureLoginContextViewer.AzureContextSelectedType = AzureContextSelectedType.NewContext;
                _AzureLoginContextViewer.AzureContext.LoginPromptBehavior = PromptBehavior.SelectAccount;
            }

            await azureArmLoginControl1.BindContext(_AzureLoginContextViewer.AzureContext, _AzureEnvironments, _UserDefinedAzureEnvironments);

            azureArmLoginControl1.Enabled = rbNewContext.Checked;
            _AzureLoginContextViewer.UpdateLabels();
        }

        private void AzureNewOrExistingLoginContextDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool isValidTargetContext = false;

            if (rbExistingContext.Checked)
            {
                isValidTargetContext = true;
            }
            else if (rbSameUserDifferentSubscription.Checked)
            {
                isValidTargetContext = cboSubscription.SelectedIndex >= 0;
            }
            else if (rbNewContext.Checked)
            {
                isValidTargetContext = _AzureLoginContextViewer.AzureContext.AzureSubscription != null;
            }

            if (!isValidTargetContext)
            {
                e.Cancel = true;
                MessageBox.Show("You must have a valid target Azure Subscription selected.");
            }
        }

        private void cboTenant_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSubscription.Items.Clear();
            if (cboTenant.SelectedItem != null)
            {
                AzureTenant azureTenant = (AzureTenant)cboTenant.SelectedItem;
                foreach (AzureSubscription azureSubscription in azureTenant.Subscriptions)
                {
                    if (azureSubscription != _AzureLoginContextViewer.ExistingContext.AzureSubscription) // Do not add if same as existing subscription.  Combobox intent is to pick a different subscription.
                        cboSubscription.Items.Add(azureSubscription);
                }

                if (cboSubscription.Items.Count > 0)
                    cboSubscription.SelectedIndex = 0;
            }
        }

        private async void cboSubscription_SelectedIndexChanged(object sender, EventArgs e)
        {
            AzureSubscription selectedSubscription = (AzureSubscription)cboSubscription.SelectedItem;
            await _AzureLoginContextViewer.AzureContext.SetSubscriptionContext(selectedSubscription);
        }
    }
}

