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
    public class RouteTable : Core.MigrationTarget, IMigrationRouteTable
    {
        //private IRouteTable _SourceRouteTable;
        private List<Route> _Routes = new List<Route>();

        #region Constructors

        private RouteTable() : base(null, ArmConst.MicrosoftNetwork, ArmConst.RouteTables, null, null) { }

         public RouteTable(AzureSubscription azureSubscription, IRouteTable source, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.RouteTables, targetSettings, logProvider)
        {
            this.Source = source;
            this.SetTargetName(source.Name, targetSettings);
        }

        public RouteTable(AzureSubscription azureSubscription, Arm.RouteTable source, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftNetwork, ArmConst.RouteTables, targetSettings, logProvider)
        {
            this.Source = source;
            this.SetTargetName(source.Name, targetSettings);

            foreach (Arm.Route route in source.Routes)
            {
                _Routes.Add(new Route(azureSubscription, route, targetSettings, logProvider));
            }
        }

        #endregion

        public List<Route> Routes
        {
            get { return _Routes; }
        }

        public override string ImageKey { get { return "RouteTable"; } }

        public override string FriendlyObjectName { get { return "Route Table"; } }



        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName + targetSettings.AvailabilitySetSuffix;
        }

        public override async Task RefreshFromSource()
        {
           // throw new NotImplementedException();
        }
    }
}

