// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class VirtualMachineImage : ArmResource, IVirtualMachineImage
    {
        private VirtualMachineImage() : base(null, null) { }

        public VirtualMachineImage(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }

    }
}

