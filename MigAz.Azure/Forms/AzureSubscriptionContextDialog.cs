// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigAz.Azure.Forms
{
    public partial class AzureSubscriptionContextDialog : Form
    {
        AzureContext _AzureContext;

        public AzureSubscriptionContextDialog()
        {
            InitializeComponent();
        }

        public async Task InitializeDialog(AzureContext azureContext)
        {
            _AzureContext = azureContext;
            _AzureContext.LogProvider.WriteLog("InitializeDialog", "Start AzureSubscriptionContextDialog InitializeDialog");

            lblAzureEnvironment.Text = _AzureContext.AzureEnvironment.ToString();

            if (_AzureContext.TokenProvider.LastAccount != null)
            {
                lblAzureUsername.Text = _AzureContext.TokenProvider.LastAccount.Username;
            }

            cboTenant.Items.Clear();
            if (_AzureContext.AzureRetriever != null && _AzureContext.TokenProvider != null)
            {
                _AzureContext.LogProvider.WriteLog("InitializeDialog", "Loading Azure Tenants");
                foreach (AzureTenant azureTenant in await _AzureContext.GetAzureARMTenants())
                {
                    if (azureTenant.Subscriptions.Count > 0) // Only add Tenants that have one or more Subscriptions
                    {
                        cboTenant.Items.Add(azureTenant);
                        _AzureContext.LogProvider.WriteLog("InitializeDialog", "Added Azure Tenant '" + azureTenant.ToString() + "'");
                    }
                    else
                    {
                        _AzureContext.LogProvider.WriteLog("InitializeDialog", "Not adding Azure Tenant '" + azureTenant.ToString() + "'.  Contains no subscriptions.");
                    }
                }
                cboTenant.Enabled = true;

                if (_AzureContext.AzureTenant != null)
                {
                    foreach (AzureTenant azureTenant in cboTenant.Items)
                    {
                        if (_AzureContext.AzureTenant == azureTenant)
                            cboTenant.SelectedItem = azureTenant;
                    }
                }
            }

            _AzureContext.LogProvider.WriteLog("InitializeDialog", "End AzureSubscriptionContextDialog InitializeDialog");
            _AzureContext.StatusProvider.UpdateStatus("Ready");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AzureContextARMDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (_AzureContext.AzureSubscription == null && cmbSubscriptions.Items.Count > 0)
                    e.Cancel = true;

                if (e.Cancel)
                    MessageBox.Show("An Azure Subscription must be selected from the Source (ASM) or Target (ARM) login.");
            }
        }

        private async void cmbSubscriptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            await _AzureContext.SetSubscriptionContext((AzureSubscription)cmbSubscriptions.SelectedItem);
        }

        private async void cboTenant_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbSubscriptions.Items.Clear();
            if (cboTenant.SelectedItem != null)
            {
                AzureTenant azureTenant = (AzureTenant)cboTenant.SelectedItem;
                if (_AzureContext.AzureRetriever != null)
                {
                    foreach (AzureSubscription azureSubscription in azureTenant.Subscriptions)
                    {
                        cmbSubscriptions.Items.Add(azureSubscription);
                    }
                    cmbSubscriptions.Enabled = true;
                }

                if (_AzureContext.AzureSubscription != null)
                {
                    foreach (AzureSubscription azureSubscription in cmbSubscriptions.Items)
                    {
                        if (_AzureContext.AzureSubscription == azureSubscription)
                            cmbSubscriptions.SelectedItem = azureSubscription;
                    }
                }

                if (cmbSubscriptions.SelectedItem == null && cmbSubscriptions.Items.Count == 1)
                    cmbSubscriptions.SelectedIndex = 0;
            }

        }
    }
}

