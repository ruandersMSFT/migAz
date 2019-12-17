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
    public class PublicIp : Core.MigrationTarget, IMigrationPublicIp
    {
        private String _DomainNameLabel = String.Empty;
        private IPAllocationMethodEnum _IPAllocationMethod = IPAllocationMethodEnum.Dynamic;

        #region Constructors

        public PublicIp() : base(null, ArmConst.MicrosoftNetwork, ArmConst.PublicIPAddresses, null, null) { }

        public PublicIp(AzureSubscription azureSubscription, string targetName, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.PublicIPAddresses, targetSettings, logProvider)
        {
            this.SetTargetName(targetName, targetSettings);
        }

        public PublicIp(AzureSubscription azureSubscription, Arm.PublicIP armPublicIP, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.PublicIPAddresses, targetSettings, logProvider)
        {
            this.Source = armPublicIP;
            this.SetTargetName(armPublicIP.Name, targetSettings);
        }

        #endregion

        public String DomainNameLabel
        {
            get { return _DomainNameLabel; }
            set
            {
                if (value == null)
                    _DomainNameLabel = null;
                else
                    _DomainNameLabel = value.ToLower();
            }
        }

        public IPAllocationMethodEnum IPAllocationMethod
        {
            get { return _IPAllocationMethod; }
            set { _IPAllocationMethod = value; }
        }

        public override string ImageKey { get { return "PublicIp"; } }

        public override string FriendlyObjectName { get { return "Public IP"; } }

        public override async Task RefreshFromSource()
        {
            if (this.Source != null)
            {
                if (this.Source.GetType() == typeof(Arm.PublicIP))
                {
                    Arm.PublicIP armPublicIP = (Arm.PublicIP)this.Source;

                    this.DomainNameLabel = armPublicIP.DomainNameLabel;

                    if (String.Compare(armPublicIP.PublicIPAllocationMethod, "Static", true) == 0)
                        _IPAllocationMethod = IPAllocationMethodEnum.Static;
                }
            }
        }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }
    }
}

