// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class ArmResource
    {
        public delegate Task AfterResourceTokenChangedHandler();
        public event AfterResourceTokenChangedHandler AfterResourceTokenChanged;

        private JToken _ResourceToken;
        private AzureSubscription _AzureSubscription;
        private Location _Location;

        private ArmResource() { }

        internal ArmResource(AzureSubscription azureSubscription, JToken resourceToken)
        {
            _AzureSubscription = azureSubscription;
            _ResourceToken = resourceToken;

            this.ResourceGroup = this.AzureSubscription.GetAzureARMResourceGroup(this.Id).Result;

            if (this.LocationString != null && this.LocationString.Length > 0)
                this.Location = this.AzureSubscription.GetAzureARMLocation(this.LocationString);

        }
        internal virtual async Task InitializeChildrenAsync()
        {
            return; // Todo russell now what?  get rid of?
        }

        internal async Task SetResourceToken(JToken resourceToken)
        {
            _ResourceToken = resourceToken;

            if (AfterResourceTokenChanged != null)
                await AfterResourceTokenChanged();
        }

        public AzureSubscription AzureSubscription
        {
            get { return _AzureSubscription; }
        }

        public JToken ResourceToken => _ResourceToken;
        public string Id => (string)_ResourceToken.SelectToken("id");
        public string Name => (string)_ResourceToken.SelectToken("name");
        public string Type => (string)_ResourceToken.SelectToken("type");
        private string LocationString => (string)_ResourceToken.SelectToken("location");

        public Location Location
        {
            get
            {
                return _Location;
            }
            private set
            {
                _Location = value;
            }
        }

        public ResourceGroup ResourceGroup { get; internal set; }

        public async Task RefreshToken()
        {
            JObject resourceToken = await this.AzureSubscription.GetArmResourceJson(this.Id);
            await SetResourceToken(resourceToken);
        }

        public override string ToString()
        {
            return this.Name;
        }

    }
}

