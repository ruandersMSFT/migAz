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
    public class NetworkSecurityGroup : Core.MigrationTarget, IMigrationNetworkSecurityGroup
    {
        //private INetworkSecurityGroup _SourceNetworkSecurityGroup;
        private List<NetworkSecurityGroupRule> _Rules = new List<NetworkSecurityGroupRule>();

        private NetworkSecurityGroup() : base(null, ArmConst.MicrosoftNetwork, ArmConst.NetworkSecurityGroups, null, null) { }

        public NetworkSecurityGroup(AzureSubscription azureSubscription, Asm.NetworkSecurityGroup source, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.NetworkSecurityGroups, targetSettings, logProvider)
        {
            this.Source = source;
            this.SetTargetName(source.Name, targetSettings);

            foreach (Asm.NetworkSecurityGroupRule sourceRule in source.Rules)
            {
                NetworkSecurityGroupRule targetRule = new NetworkSecurityGroupRule(this.AzureSubscription, sourceRule, targetSettings, logProvider);
                this.Rules.Add(targetRule);
            }
        }

        public NetworkSecurityGroup(AzureSubscription azureSubscription, Arm.NetworkSecurityGroup source, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.NetworkSecurityGroups, targetSettings, logProvider)
        {
            this.Source = source;
            this.SetTargetName(source.Name, targetSettings);
        }

        public List<NetworkSecurityGroupRule> Rules
        {
            get { return _Rules; }
        }
        
        public override string ImageKey { get { return "NetworkSecurityGroup"; } }

        public override string FriendlyObjectName { get { return "Network Security Group"; } }


        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName + targetSettings.NetworkSecurityGroupSuffix;
        }

        public override async Task RefreshFromSource()
        {
            if (this.Source != null)
            {
                if (this.Source.GetType() == typeof(Arm.NetworkSecurityGroup))
                {
                    this.Rules.Clear();

                    Arm.NetworkSecurityGroup armNetworkSecurityGroup = (Arm.NetworkSecurityGroup)this.Source;
                    foreach (Arm.NetworkSecurityGroupRule sourceRule in armNetworkSecurityGroup.Rules)
                    {
                        NetworkSecurityGroupRule targetRule = new NetworkSecurityGroupRule(this.AzureSubscription, sourceRule, this.TargetSettings, this.LogProvider);
                        this.Rules.Add(targetRule);
                    }
                }
            }
        }
    }
}

