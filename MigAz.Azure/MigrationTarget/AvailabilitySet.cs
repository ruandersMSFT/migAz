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
    public class AvailabilitySet : Core.MigrationTarget //<IAvailabilitySetSource>
    {
        private List<VirtualMachine> _TargetVirtualMachines = new List<VirtualMachine>();
        private Int32 _PlatformUpdateDomainCount = 5;
        private Int32 _PlatformFaultDomainCount = 3;

        #region Constructors 

        public AvailabilitySet() : base(null, ArmConst.MicrosoftCompute, ArmConst.AvailabilitySets, null, null) { }

        public AvailabilitySet(AzureSubscription azureSubscription, String targetName, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftCompute, ArmConst.AvailabilitySets, targetSettings, logProvider)
        {
            this.SetTargetName(targetName, targetSettings);
        }

        public AvailabilitySet(AzureSubscription azureSubscription, Asm.CloudService asmCloudService, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftCompute, ArmConst.AvailabilitySets, targetSettings, logProvider)
        {
            this.Source = asmCloudService;
            this.SetTargetName(asmCloudService.Name, targetSettings);
        }

        public AvailabilitySet(AzureSubscription azureSubscription, Arm.AvailabilitySet availabilitySet, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftCompute, ArmConst.AvailabilitySets, targetSettings, logProvider)
        {
            this.Source = availabilitySet;

            this.SetTargetName(availabilitySet.Name, targetSettings);

            if (availabilitySet.PlatformFaultDomainCount < Constants.AvailabilitySetMinPlatformFaultDomain)
            {
                // todo future, track object translation alerts
                this.PlatformFaultDomainCount = Constants.AvailabilitySetMinPlatformFaultDomain;
            }
            else if (availabilitySet.PlatformFaultDomainCount > Constants.AvailabilitySetMaxPlatformFaultDomain)
            {
                // todo future, track object translation alerts
                this.PlatformFaultDomainCount = Constants.AvailabilitySetMaxPlatformFaultDomain;
            }
            else
            {
                if (availabilitySet.PlatformFaultDomainCount.HasValue)
                    this.PlatformFaultDomainCount = availabilitySet.PlatformFaultDomainCount.Value;
            }

            if (availabilitySet.PlatformUpdateDomainCount < Constants.AvailabilitySetMinPlatformUpdateDomain)
            {
                // todo future, track object translation alerts
                this.PlatformUpdateDomainCount = Constants.AvailabilitySetMinPlatformUpdateDomain;
            }
            else if (availabilitySet.PlatformUpdateDomainCount > Constants.AvailabilitySetMaxPlatformUpdateDomain)
            {
                if (availabilitySet.PlatformUpdateDomainCount.HasValue)
                    this.PlatformUpdateDomainCount = availabilitySet.PlatformUpdateDomainCount.Value;
            }
        }

        #endregion

        public List<VirtualMachine> TargetVirtualMachines
        {
            get { return _TargetVirtualMachines; }
        }

        // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/manage-availability?toc=%2Fazure%2Fvirtual-machines%2Fwindows%2Ftoc.json
        public Int32 PlatformUpdateDomainCount
        {
            get { return _PlatformUpdateDomainCount; }
            set
            {
                if (value >= Constants.AvailabilitySetMinPlatformUpdateDomain && value <= Constants.AvailabilitySetMaxPlatformUpdateDomain)
                    _PlatformUpdateDomainCount = value;
                else
                    throw new ArgumentException("Platform Update Domain Count must be between " + Constants.AvailabilitySetMinPlatformUpdateDomain.ToString() + " and " + Constants.AvailabilitySetMaxPlatformUpdateDomain.ToString() + ".");
            }
        }

        public Int32 PlatformFaultDomainCount
        {
            get { return _PlatformFaultDomainCount; }
            set
            {
                if (value >= Constants.AvailabilitySetMinPlatformFaultDomain && value <= Constants.AvailabilitySetMaxPlatformFaultDomain)
                    _PlatformFaultDomainCount = value;
                else
                    throw new ArgumentException("Platform Fault Domain Count must be between " + Constants.AvailabilitySetMinPlatformFaultDomain.ToString() + " and " + Constants.AvailabilitySetMaxPlatformFaultDomain.ToString() + ".");
            }
        }

        internal bool IsManagedDisks
        {
            get
            {
                foreach (VirtualMachine targetVirtualMachine in _TargetVirtualMachines)
                {
                    if (!targetVirtualMachine.IsManagedDisks)
                        return false;
                }

                return true;
            }
        }

        internal bool IsUnmanagedDisks
        {
            get
            {
                foreach (VirtualMachine targetVirtualMachine in _TargetVirtualMachines)
                {
                    if (!targetVirtualMachine.IsUnmanagedDisks)
                        return false;
                }

                return true;
            }
        }

        public override string ImageKey { get { return "AvailabilitySet"; } }

        public override string FriendlyObjectName { get { return "Availability Set"; } }

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

