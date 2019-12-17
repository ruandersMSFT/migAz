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
    public class ApplicationSecurityGroup : ArmResource, IAvailabilitySetSource
    {
        public ApplicationSecurityGroup(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }
    }
}

