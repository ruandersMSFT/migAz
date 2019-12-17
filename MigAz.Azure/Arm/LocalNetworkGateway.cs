using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class LocalNetworkGateway : ArmResource, ILocalNetworkGateway
    {
        private LocalNetworkGateway() : base(null, null) { }

        public LocalNetworkGateway(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }
    }
}
