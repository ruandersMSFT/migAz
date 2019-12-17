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
    public class Route : Core.MigrationTarget
    {
        private IRoute _Source;
        private NextHopTypeEnum _NextHopType = NextHopTypeEnum.VnetLocal;
        private String _AddressPrefix = String.Empty;
        private String _NextHopIpAddress = String.Empty;

        #region Constructors

        private Route() : base(null, String.Empty, String.Empty, null, null) { }

        public Route(AzureSubscription azureSubscription, IRoute route, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, String.Empty, String.Empty, targetSettings, logProvider)
        {
            _Source = route;

            this.SetTargetName(route.Name, targetSettings);
            this.NextHopType = route.NextHopType;
            this.AddressPrefix = route.AddressPrefix;
            this.NextHopIpAddress = route.NextHopIpAddress;
        }

        #endregion

        public NextHopTypeEnum NextHopType
        {
            get { return _NextHopType; }
            set { _NextHopType = value; }
        }

        public String AddressPrefix
        {
            get { return _AddressPrefix; }
            set
            {
                if (value == null)
                    _AddressPrefix = String.Empty;
                else
                    _AddressPrefix = value.Trim();
            }
        }
        public String NextHopIpAddress
        {
            get { return _NextHopIpAddress; }
            set
            {
                if (value == null)
                    _NextHopIpAddress = String.Empty;
                else
                    _NextHopIpAddress = value.Trim();
            }
        }

        public override string ImageKey { get { return "Route"; } }

        public override string FriendlyObjectName { get { return "Route"; } }



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

