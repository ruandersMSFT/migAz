// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Interface;
using MigAz.Azure.Core;
using MigAz.Azure.Core.ArmTemplate;
using MigAz.Azure.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.MigrationTarget
{
    public class ResourceGroup : Core.MigrationTarget, IResourceGroup
    {
        private Arm.Location _TargetLocation;
        private List<Core.MigrationTarget> _Resources;

        #region Constructors

        public ResourceGroup(AzureSubscription azureSubscription, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            this.SetTargetName("NewResourceGroup", targetSettings);
        }

        public ResourceGroup(AzureSubscription azureSubscription, Arm.ResourceGroup resourceGroup, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            this.Source = resourceGroup;
            this.SetTargetName(resourceGroup.Name, targetSettings);
        }

        #endregion

        public Arm.Location TargetLocation
        {
            get { return _TargetLocation; }
            set { _TargetLocation = value; }
        }

        public List<Core.MigrationTarget> Resources
        {
            get { return _Resources; }
        }

        public override string ImageKey { get { return "ResourceGroup"; } }

        public override string FriendlyObjectName { get { return "Resource Group"; } }

        public override async Task RefreshFromSource()
        {
            //throw new NotImplementedException();
        }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName + targetSettings.AvailabilitySetSuffix;
        }

    }
}

