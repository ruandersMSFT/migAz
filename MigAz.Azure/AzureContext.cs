// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Interface;
using MigAz.Azure.Core;
using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MigAz.Azure
{
    public class AzureContext
    {
        private AzureEnvironment _AzureEnvironment;
        private ArmDiskType _DefaultTargetDiskType = ArmDiskType.ManagedDisk;
        private PromptBehavior _LoginPromptBehavior = PromptBehavior.Auto;

        private AzureTenant _AzureTenant;
        private AzureSubscription _AzureSubscription;
        private AzureRetriever _AzureRetriever;
        private TargetSettings _TargetSettings;
        private ILogProvider _LogProvider;
        private IStatusProvider _StatusProvider;
        private ITokenProvider _TokenProvider;
        private List<AzureTenant> _ArmTenants;

        public delegate Task BeforeAzureTenantChangedHandler(AzureContext sender);
        public event BeforeAzureTenantChangedHandler BeforeAzureTenantChange;

        public delegate Task AfterAzureTenantChangedHandler(AzureContext sender);
        public event AfterAzureTenantChangedHandler AfterAzureTenantChange;

        public delegate Task BeforeAzureSubscriptionChangedHandler(AzureContext sender);
        public event BeforeAzureSubscriptionChangedHandler BeforeAzureSubscriptionChange;

        public delegate Task AfterAzureSubscriptionChangedHandler(AzureContext sender);
        public event AfterAzureSubscriptionChangedHandler AfterAzureSubscriptionChange;

        public delegate Task AzureEnvironmentChangedHandler(AzureContext sender);
        public event AzureEnvironmentChangedHandler AzureEnvironmentChanged;

        public delegate Task UserAuthenticatedHandler(AzureContext sender);
        public event UserAuthenticatedHandler UserAuthenticated;

        public delegate Task BeforeUserSignOutHandler();
        public event BeforeUserSignOutHandler BeforeUserSignOut;

        public delegate Task AfterUserSignOutHandler();
        public event AfterUserSignOutHandler AfterUserSignOut;

        #region Constructors

        private AzureContext() { }

        public AzureContext(AzureRetriever azureRetriever, TargetSettings targetSettings, PromptBehavior defaultPromptBehavior = PromptBehavior.Always)
        {
            _AzureRetriever = azureRetriever;
            _TargetSettings = targetSettings;
            _LogProvider = azureRetriever.LogProvider;
            _StatusProvider = azureRetriever.StatusProvider;
            _LoginPromptBehavior = defaultPromptBehavior;
        }

        #endregion

        #region Properties

        public ITokenProvider TokenProvider
        {
            get { return _TokenProvider; }
            set { _TokenProvider = value; }
        }

        public AzureEnvironment AzureEnvironment
        {
            get { return _AzureEnvironment; }
            set
            {
                if (_AzureEnvironment != value)
                {
                    _AzureEnvironment = value;
                    this.TokenProvider = new AzureTokenProvider(this.AzureEnvironment, _LogProvider);

                    AzureEnvironmentChanged?.Invoke(this);
                }
            }
        }

        public AzureTenant AzureTenant
        {
            get { return _AzureTenant; }
        }

        public AzureSubscription AzureSubscription
        {
            get { return _AzureSubscription; }
        }

        public AzureRetriever AzureRetriever
        {
            get { return _AzureRetriever; }
            set { _AzureRetriever = value; }
        }

        public ILogProvider LogProvider
        {
            get { return _LogProvider; }
        }

        public IStatusProvider StatusProvider
        {
            get { return _StatusProvider; }
        }

        public PromptBehavior LoginPromptBehavior
        {
            get { return _LoginPromptBehavior; }
            set { _LoginPromptBehavior = value; }
        }

        public ArmDiskType DefaultTargetDiskType
        {
            get { return _DefaultTargetDiskType; }
            set { _DefaultTargetDiskType = value; }
        }

        #endregion

        #region Methods

        public async Task Login(string resourceUrl)
        {
            if (this.TokenProvider == null)
                this.TokenProvider = new AzureTokenProvider(this.AzureEnvironment, this.LogProvider);

            await this.TokenProvider.Login(resourceUrl, this.LoginPromptBehavior);
            UserAuthenticated?.Invoke(this);

            this.StatusProvider.UpdateStatus("Ready");
        }

        public async Task SetTenantContext(AzureTenant azureTenant)
        {
            if (azureTenant == null && this._AzureSubscription != null)
                await this.SetSubscriptionContext(null);

            if (BeforeAzureTenantChange != null)
                await BeforeAzureTenantChange?.Invoke(this);

            _AzureTenant = azureTenant;

            if (AfterAzureTenantChange != null)
                await AfterAzureTenantChange?.Invoke(this);
        }

        public virtual string GetARMTokenResourceUrl()
        {
            return this.AzureEnvironment.ResourceManagerEndpoint;
        }

        public virtual string GetARMServiceManagementUrl()
        {
            return this.AzureEnvironment.ResourceManagerEndpoint;
        }

        public async Task SetSubscriptionContext(AzureSubscription azureSubscription)
        {
            if (azureSubscription != _AzureSubscription)
            {
                if (BeforeAzureSubscriptionChange != null)
                    await BeforeAzureSubscriptionChange?.Invoke(this);

                if (azureSubscription != null)
                {
                    if (azureSubscription.AzureTenant != this.AzureTenant)
                        await SetTenantContext(azureSubscription.AzureTenant);
                }

                _AzureSubscription = azureSubscription;

                if (AfterAzureSubscriptionChange != null)
                    await AfterAzureSubscriptionChange?.Invoke(this);
            }
        }

        public async Task Logout()
        {
            if (BeforeUserSignOut != null)
                await BeforeUserSignOut.Invoke();

            await this.SetTenantContext(null);

            _ArmTenants = null;
            _AzureRetriever.ClearCache();
            this.TokenProvider = null;

            if (AfterUserSignOut != null)
                await AfterUserSignOut?.Invoke();
        }


        public async Task<List<AzureTenant>> GetAzureARMTenants(bool allowRestCacheUse = false)
        {
            this.LogProvider.WriteLog("GetAzureARMTenants", "Start");

            if (_ArmTenants != null)
                return _ArmTenants;

            if (this.TokenProvider == null)
                throw new ArgumentNullException("TokenProvider Context is null.  Unable to call Azure API without TokenProvider.");

            Microsoft.Identity.Client.AuthenticationResult tenantAuthenticationResult = await this.TokenProvider.GetToken(this.AzureEnvironment.ResourceManagerEndpoint, "user_impersonation");

            String tenantUrl = this.AzureEnvironment.ResourceManagerEndpoint + "tenants?api-version=2015-01-01";
            this.StatusProvider.UpdateStatus("BUSY: Getting Tenants...");

            AzureRestRequest azureRestRequest = new AzureRestRequest(tenantUrl, tenantAuthenticationResult, "GET", allowRestCacheUse);
            AzureRestResponse azureRestResponse = await this.AzureRetriever.GetAzureRestResponse(azureRestRequest);
            JObject tenantsJson = JObject.Parse(azureRestResponse.Response);

            var tenants = from tenant in tenantsJson["value"]
                          select tenant;

            _ArmTenants = new List<AzureTenant>();

            foreach (JObject tenantJson in tenants)
            {
                try
                {
                    AzureTenant azureTenant = new AzureTenant(this, tenantJson);
                    await azureTenant.InitializeChildren(this, allowRestCacheUse);
                    _ArmTenants.Add(azureTenant);
                }
                catch (Exception exc)
                {
                    this.LogProvider.WriteLog("GetAzureARMTenants", "Error getting Azure Tenant: '" + exc.Message);
                }
            }

            return _ArmTenants;
        }
        #endregion
    }
}

