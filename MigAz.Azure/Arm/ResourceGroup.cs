// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure;
using MigAz.Azure.Interface;
using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MigAz.Azure.Arm
{
    public class ResourceGroup : IResourceGroup
    {
        private JObject _ResourceGroupJson;
        private AzureEnvironment _AzureEnvironment;
        private AzureSubscription _AzureSubscription;
        private List<ArmResource> _Resources = new List<ArmResource>();

        internal ResourceGroup(JObject resourceGroupJson, AzureEnvironment azureEnvironment, AzureSubscription azureSubscription)
        {
            _ResourceGroupJson = resourceGroupJson;
            _AzureEnvironment = azureEnvironment;
            _AzureSubscription = azureSubscription;
        }

        public async Task InitializeChildrenAsync()
        {
            this.Location = this.AzureSubscription.GetAzureARMLocation(this.LocationString);
        }

        private ResourceGroup() { }

        public AzureSubscription AzureSubscription
        {
            get { return _AzureSubscription; }
        }

        public string Name => (string)_ResourceGroupJson["name"];
        private string LocationString => (string)_ResourceGroupJson["location"];
        public string Id => (string)_ResourceGroupJson["id"];

        public Location Location
        {
            get;
            private set;
        }

        public List<ArmResource> Resources
        {
            get { return _Resources; }
        }

        public override string ToString()
        {
            return this.Name + " (" + this.Location + ")";
        }
    }
}

