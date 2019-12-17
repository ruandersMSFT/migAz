using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigAz.Azure.Core;
using MigAz.Azure.Core.ArmTemplate;
using MigAz.Azure.Core.Interface;

namespace MigAz.Azure.MigrationTarget
{
    public class LocalNetworkGateway : Core.MigrationTarget
    {
        //private ILocalNetworkGateway _SourceLocalNetworkGateway;
        private string _GatewayIpAddress = String.Empty;
        private List<String> _AddressPrefixes = new List<string>();

        #region Constructors

        private LocalNetworkGateway() : base(null, ArmConst.MicrosoftNetwork, ArmConst.LocalNetworkGateways, null, null) { }

        public LocalNetworkGateway(AzureSubscription azureSubscription, ILocalNetworkGateway localNetworkGateway, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.LocalNetworkGateways, targetSettings, logProvider)
        {
            this.Source = localNetworkGateway;
            this.SetTargetName(this.SourceName, targetSettings);
        }

        #endregion


        public override string ImageKey { get { return "LocalNetworkGateway"; } }

        public override string FriendlyObjectName { get { return "Local Network Gateway"; } }

        public List<String> AddressPrefixes
        {
            get { return _AddressPrefixes; }
        }

        public string GatewayIpAddress
        {
            get { return _GatewayIpAddress; }
            set { _GatewayIpAddress = value; }
        }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }

        public override async Task RefreshFromSource()
        {
            //throw new NotImplementedException();
        }
    }
}
