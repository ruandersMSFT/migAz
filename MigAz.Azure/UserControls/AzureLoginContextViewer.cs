// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MigAz.Azure.Forms;

namespace MigAz.Azure.UserControls
{
    public enum AzureLoginChangeType
    {
        NewOrExistingContext,
        NewContext,
        SubscriptionChangeOnly
    }

    public enum AzureContextSelectedType
    {
        ExistingContext,
        SameUserDifferentSubscription,
        NewContext
    }

    public partial class AzureLoginContextViewer : UserControl
    {
        private AzureRetriever _AzureRetriever;
        private AzureContext _AzureContext;
        private AzureContext _ExistingContext;
        private AzureLoginChangeType _ChangeType = AzureLoginChangeType.NewContext;
        private AzureContextSelectedType _AzureContextSelectedType = AzureContextSelectedType.ExistingContext;
        private List<AzureEnvironment> _AzureEnvironments;
        private List<AzureEnvironment> _UserDefinedAzureEnvironments;

        public delegate Task AfterContextChangedHandler(AzureLoginContextViewer sender);
        public event AfterContextChangedHandler AfterContextChanged;

        public AzureLoginContextViewer()
        {
            InitializeComponent();
        }

        public void Bind(AzureContext azureContext, AzureRetriever azureRetriever, List<AzureEnvironment> azureEnvironments, ref List<AzureEnvironment> userDefinedAzureEnvironments)
        {
            _AzureRetriever = azureRetriever;
            _AzureEnvironments = azureEnvironments;
            _UserDefinedAzureEnvironments = userDefinedAzureEnvironments;

            _AzureContext = azureContext;

            _AzureContext.AzureEnvironmentChanged += _AzureContext_AzureEnvironmentChanged;
            _AzureContext.AfterAzureTenantChange += _AzureContext_AfterAzureTenantChange;
            _AzureContext.UserAuthenticated += _AzureContext_UserAuthenticated;
            _AzureContext.AfterUserSignOut += _AzureContext_AfterUserSignOut;
            _AzureContext.AfterAzureSubscriptionChange += _AzureContext_AfterAzureSubscriptionChange;
        }

        public AzureContextSelectedType AzureContextSelectedType
        {
            get { return _AzureContextSelectedType; }
            set
            {
                _AzureContextSelectedType = value;
            }
        }
        public AzureContext ExistingContext
        {
            get { return _ExistingContext; }
            set
            {
                _ExistingContext = value;
                UpdateLabels();
                AfterContextChanged?.Invoke(this);
            }
        }

        private async Task _AzureContext_AfterAzureTenantChange(AzureContext sender)
        {
            UpdateLabels();
        }

        private async Task _AzureContext_AfterUserSignOut()
        {
            UpdateLabels();
        }

        private async Task _AzureContext_AzureEnvironmentChanged(AzureContext sender)
        {
            UpdateLabels();
        }

        private async Task _AzureContext_UserAuthenticated(AzureContext sender)
        {
            UpdateLabels();
        }

        private async Task _AzureContext_AfterAzureSubscriptionChange(AzureContext sender)
        {
            UpdateLabels();
        }

        public void UpdateLabels()
        {
            lblSourceUser.Text = "-";
            lblSourceSubscriptionName.Text = "-";
            lblSourceSubscriptionId.Text = "-";
            lblTenantName.Text = "-";

            AzureContext selectedContext = this.SelectedAzureContext;
            if (selectedContext != null)
            {
                lblSourceEnvironment.Text = selectedContext.AzureEnvironment.ToString();

                if (selectedContext.AzureTenant != null)
                    lblTenantName.Text = selectedContext.AzureTenant.ToString();

                if (selectedContext.TokenProvider != null &&
                    selectedContext.TokenProvider.LastAccount != null)
                {
                    lblSourceUser.Text = selectedContext.TokenProvider.LastAccount.Username;
                }

                if (selectedContext.AzureSubscription != null)
                {
                    lblSourceSubscriptionName.Text = selectedContext.AzureSubscription.Name;
                    lblSourceSubscriptionId.Text = selectedContext.AzureSubscription.SubscriptionId.ToString();
                }
            }
        }

        public string Title
        {
            get { return this.groupSubscription.Text; }
            set { this.groupSubscription.Text = value; }
        }

        public AzureLoginChangeType ChangeType
        {
            get { return _ChangeType; }
            set { _ChangeType = value; }
        }

        public AzureContext SelectedAzureContext
        {
            get
            {
                if (this.ChangeType == AzureLoginChangeType.NewOrExistingContext)
                {
                    if (_ExistingContext != null && _AzureContextSelectedType == AzureContextSelectedType.ExistingContext)
                        return _ExistingContext;
                    else
                        return _AzureContext;
                }
                else
                    return _AzureContext;
            }
        }

        public AzureContext AzureContext
        {
            get { return _AzureContext; }
        }

        private async void btnAzureContext_Click(object sender, EventArgs e)
        {
            if (_AzureContext == null)
                throw new ArgumentException("Azure Context not set.  You must initiate the AzureLoginContextViewer control with the Bind Method.");

            if (_ChangeType == AzureLoginChangeType.NewOrExistingContext)
            {
                if (_ExistingContext == null)
                {
                    AzureLoginContextDialog azureLoginContextDialog = new AzureLoginContextDialog();
                    await azureLoginContextDialog.InitializeDialog(_AzureContext, _AzureEnvironments, _UserDefinedAzureEnvironments);
                    azureLoginContextDialog.ShowDialog();
                    azureLoginContextDialog.Dispose();
                }
                else
                {
                    AzureNewOrExistingLoginContextDialog azureLoginContextDialog = new AzureNewOrExistingLoginContextDialog();
                    await azureLoginContextDialog.InitializeDialog(this, _AzureEnvironments, _UserDefinedAzureEnvironments);
                    azureLoginContextDialog.ShowDialog();
                    azureLoginContextDialog.Dispose();
                }
            }
            else if (_ChangeType == AzureLoginChangeType.NewContext)
            {
                AzureLoginContextDialog azureLoginContextDialog = new AzureLoginContextDialog();
                await azureLoginContextDialog.InitializeDialog(_AzureContext, _AzureEnvironments, _UserDefinedAzureEnvironments);
                azureLoginContextDialog.ShowDialog();
                azureLoginContextDialog.Dispose();
            }
            else
            {
                AzureSubscriptionContextDialog azureSubscriptionContextDialog = new AzureSubscriptionContextDialog();
                await azureSubscriptionContextDialog.InitializeDialog(_AzureContext);
                azureSubscriptionContextDialog.ShowDialog();
                azureSubscriptionContextDialog.Dispose();
            }

            AfterContextChanged?.Invoke(this);
        }

        private void AzureLoginContextViewer_EnabledChanged(object sender, EventArgs e)
        {
            btnAzureContext.Enabled = this.Enabled;
        }

        public void ChangeAzureContext()
        {
            btnAzureContext_Click(this, null);
        }

        private void AzureLoginContextViewer_Resize(object sender, EventArgs e)
        {
            groupSubscription.Width = this.Width - 5;
            btnAzureContext.Left = this.Width - btnAzureContext.Width - 10;
        }
    }
}

