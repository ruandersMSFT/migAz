// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class NetworkSecurityGroupRule
    {
        private JToken _NetworkSecurityGroupRuleToken;

        private NetworkSecurityGroupRule() { }

        public NetworkSecurityGroupRule(JToken networkSecurityGroupRuleToken)
        {
            _NetworkSecurityGroupRuleToken = networkSecurityGroupRuleToken;
        }

        public string Id => (string)_NetworkSecurityGroupRuleToken.SelectToken("id");
        public string Name => (string)_NetworkSecurityGroupRuleToken.SelectToken("name");
        public Int32 Priority => Convert.ToInt32((string)_NetworkSecurityGroupRuleToken.SelectToken("properties.priority"));
        public string Direction => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.direction");
        public string Access => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.access");
        public string SourceAddressPrefix => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.sourceAddressPrefix");
        public string DestinationAddressPrefix => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.destinationAddressPrefix");
        public string SourcePortRange => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.sourcePortRange");
        public string DestinationPortRange => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.destinationPortRange");
        public string Protocol => (string)_NetworkSecurityGroupRuleToken.SelectToken("properties.protocol");


    }
}

