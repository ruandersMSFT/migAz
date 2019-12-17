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
    public enum LoadBalancerType
    {
        Public,
        Internal
    }

    public class LoadBalancer : Core.MigrationTarget //ILoadBalancer
    {
        private List<FrontEndIpConfiguration> _FrontEndIpConfiguration = new List<FrontEndIpConfiguration>();
        private List<BackEndAddressPool> _BackEndAddressPools = new List<BackEndAddressPool>();
        private List<LoadBalancingRule> _LoadBalancingRules = new List<LoadBalancingRule>();
        private List<InboundNatRule> _InboundNatRules = new List<InboundNatRule>();
        private List<Probe> _Probes = new List<Probe>();
        private LoadBalancerType _LoadBalancerType = LoadBalancerType.Internal;

        #region Constructors

        public LoadBalancer() : base(null, ArmConst.MicrosoftNetwork, ArmConst.LoadBalancers, null, null)
        {
            new FrontEndIpConfiguration(this);
        }

        public LoadBalancer(AzureSubscription azureSubscription, string targetName, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.LoadBalancers, targetSettings, logProvider)
        {
            this.SetTargetName(targetName, targetSettings);
            new FrontEndIpConfiguration(this);
        }

        public LoadBalancer(AzureSubscription azureSubscription, Arm.LoadBalancer sourceLoadBalancer, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.LoadBalancers, targetSettings, logProvider)
        {
            this.Source = sourceLoadBalancer;
            this.SetTargetName(sourceLoadBalancer.Name, targetSettings);

            foreach (Arm.FrontEndIpConfiguration armFrontEndIpConfiguration in sourceLoadBalancer.FrontEndIpConfigurations)
            {
                FrontEndIpConfiguration targetFrontEndIpConfiguration = new FrontEndIpConfiguration(this, armFrontEndIpConfiguration);
                _FrontEndIpConfiguration.Add(targetFrontEndIpConfiguration);

                if (armFrontEndIpConfiguration.PublicIP != null)
                    this.LoadBalancerType = LoadBalancerType.Public;
            }

            foreach (Arm.BackEndAddressPool armBackendAddressPool in sourceLoadBalancer.BackEndAddressPools)
            {
                BackEndAddressPool targetBackendAddressPool = new BackEndAddressPool(this, armBackendAddressPool);
                _BackEndAddressPools.Add(targetBackendAddressPool);
            }

            foreach (Arm.Probe armProbe in sourceLoadBalancer.Probes)
            {
                Probe targetProbe = new Probe(this, armProbe);
                _Probes.Add(targetProbe);
            }

            foreach (Arm.LoadBalancingRule armLoadBalancingRule in sourceLoadBalancer.LoadBalancingRules)
            {
                LoadBalancingRule targetLoadBalancingRule = new LoadBalancingRule(this, armLoadBalancingRule);
                _LoadBalancingRules.Add(targetLoadBalancingRule);
            }
        }

        #endregion

        public LoadBalancerType LoadBalancerType
        {
            get { return _LoadBalancerType; }
            set { _LoadBalancerType = value; }
        }

        public List<BackEndAddressPool> BackEndAddressPools
        {
            get { return _BackEndAddressPools; }
            set { _BackEndAddressPools = value; }
        }
        public List<FrontEndIpConfiguration> FrontEndIpConfigurations
        {
            get { return _FrontEndIpConfiguration; }
            set { _FrontEndIpConfiguration = value; }
        }

        public List<InboundNatRule> InboundNatRules
        {
            get { return _InboundNatRules; }
        }

        public List<Probe> Probes
        {
            get { return _Probes; }
        }

        public List<LoadBalancingRule> LoadBalancingRules
        {
            get { return _LoadBalancingRules; }
        }

        public String StaticVirtualNetworkIPAddress
        {
            get;set;
        }

        public override string ImageKey { get { return "LoadBalancer"; } }

        public override string FriendlyObjectName { get { return "Load Balancer"; } }

        public override async Task RefreshFromSource()
        {
            //throw new NotImplementedException();
        }

        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }

    }
}

