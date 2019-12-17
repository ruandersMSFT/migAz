using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class VirtualNetworkGatewayConnection : ArmResource, IConnection
    {
        private VirtualNetworkGatewayConnection() : base(null, null) { }

        public VirtualNetworkGatewayConnection(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
        }

    }
}
