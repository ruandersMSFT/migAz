// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure
{
    public class AzureDomain
    {
        private JObject _DomainJson;
        private AzureTenant _AzureTenant;

        public AzureDomain(AzureTenant azureTenant, JObject domainJson)
        {
            _DomainJson = domainJson;
            _AzureTenant = azureTenant;
        }

        public string Name
        {
            get { return (string)_DomainJson["name"]; }
        }

        public bool IsDefault
        {
            get { return Convert.ToBoolean(_DomainJson["isDefault"]); }
        }
        public bool IsVerified
        {
            get { return Convert.ToBoolean(_DomainJson["isVerified"]); }
        }
        public bool IsRoot
        {
            get { return Convert.ToBoolean(_DomainJson["isRoot"]); }
        }
        public bool IsInitial
        {
            get { return Convert.ToBoolean(_DomainJson["isInitial"]); }
        }
        public bool IsAdminManaged
        {
            get { return Convert.ToBoolean(_DomainJson["isAdminManaged"]); }
        }

        // Not yet added to properties
          //"authenticationType": "Managed",
          //"availabilityStatus": null,
          //"supportedServices": [
          //  "Email",
          //  "OfficeCommunicationsOnline",
          //  "OrgIdAuthentication"
          //],
          //"forceDeleteState": null,
          //"state": null
    }
}

