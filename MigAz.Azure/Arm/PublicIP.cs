// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class PublicIP : ArmResource, IMigrationPublicIp
    {
        public PublicIP(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }

        public string IpAddress
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.ipAddress");
            }
        }

        public string PublicIPAddressVersion
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.publicIPAddressVersion");
            }
        }

        public string PublicIPAllocationMethod
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.publicIPAllocationMethod");
            }
        }

        public string IdleTimeoutInMinutes
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.idleTimeoutInMinutes");
            }
        }

        public string DomainNameLabel
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.dnsSettings.domainNameLabel");
            }
        }

        public string IpConfigurationId
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.ipConfiguration.id");
            }
        }

        public string FQDN
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.dnsSettings.fqdn");
            }
        }
    }
}

