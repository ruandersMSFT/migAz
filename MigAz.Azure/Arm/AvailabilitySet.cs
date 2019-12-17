// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MigAz.Azure.Core.Interface;

namespace MigAz.Azure.Arm
{
    public class AvailabilitySet : ArmResource, IAvailabilitySetSource
    {
        public AvailabilitySet(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }

        public Int32? PlatformUpdateDomainCount
        {
            get
            {
                return (Int32?)ResourceToken.SelectToken("properties.platformUpdateDomainCount");
            }
        }

        public Int32? PlatformFaultDomainCount
        {
            get
            {
                return (Int32?)ResourceToken.SelectToken("properties.platformFaultDomainCount");
            }
        }

        public string SkuName
        {
            get { return (string)ResourceToken.SelectToken("sku.name"); }
        }

        public List<string> VirtualMachineIds
        {
            get
            {
                List<string> virtualMachineIds = ResourceToken.SelectToken("properties.virtualMachines").Select(x => (string)x.SelectToken("id")).ToList();
                return virtualMachineIds;
            }
        }

    }
}

