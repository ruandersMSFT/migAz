// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.Arm
{
    public class RouteTable : ArmResource, IRouteTable, IMigrationRouteTable
    {
        private List<Route> _Routes = new List<Route>();


        private RouteTable() : base(null, null) { }

        public RouteTable(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken)
        {
            if (ResourceToken["properties"] != null)
            {
                if (ResourceToken["properties"]["routes"] != null)
                {
                    var routes = from route in ResourceToken["properties"]["routes"]
                                 select route;

                    foreach (var route in routes)
                    {
                        Route armRoute = new Route(this, route);
                        _Routes.Add(armRoute);
                    }
                }
            }
        }

        public List<Route> Routes => _Routes;

    }
}

