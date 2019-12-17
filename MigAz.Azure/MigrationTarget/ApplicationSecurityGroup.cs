// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class ApplicationSecurityGroup : Core.MigrationTarget //<IAvailabilitySetSource>
    {
        #region Constructors 

        public ApplicationSecurityGroup() : base(null, ArmConst.MicrosoftNetwork, ArmConst.ApplicationSecurityGroups, null, null) { }

        public ApplicationSecurityGroup(AzureSubscription azureSubscription, String targetName, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.ApplicationSecurityGroups, targetSettings, logProvider)
        {
            this.SetTargetName(targetName, targetSettings);
        }

        public ApplicationSecurityGroup(AzureSubscription azureSubscription, Arm.ApplicationSecurityGroup applicationSecurityGroup, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.ApplicationSecurityGroups, targetSettings, logProvider)
        {
            this.Source = applicationSecurityGroup;
            this.SetTargetName(applicationSecurityGroup.Name, targetSettings);
        }

        #endregion

        public override string ImageKey { get { return "ApplicationSecurityGroup"; } }

        public override string FriendlyObjectName { get { return "Application Security Group"; } }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName + targetSettings.AvailabilitySetSuffix;
        }

        public override async Task RefreshFromSource()
        {
            //throw new NotImplementedException();
        }
    }
}

