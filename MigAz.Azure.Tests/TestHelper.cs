// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure;
using MigAz.Tests.Fakes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigAz.Azure.Asm;
using MigAz.Azure.Arm;
using MigAz.Azure.Generator.AsmToArm;
using MigAz.Azure.Core.Interface;
using MigAz.Azure.Core.Generator;
using MIGAZ.Tests.Fakes;
using MigAz.Azure.Core;
using MigAz.Azure.Interface;

namespace MigAz.Tests
{
    static class TestHelper
    {
        public const string TenantId = "11111111-1111-1111-1111-111111111111";
        public const string SubscriptionId = "22222222-2222-2222-2222-222222222222";
        public static ISubscription GetTestAzureSubscription(AzureContext azureContext)
        {
            FakeAzureSubscription fakeAzureSubscription = new FakeAzureSubscription();
            fakeAzureSubscription.AzureEnvironment = azureContext.AzureEnvironment;
            return fakeAzureSubscription;
        }

        public static async Task<AzureContext> SetupAzureContext(AzureEnvironment azureEnvironment, string restResponseFile)
        {
            ILogProvider logProvider = new FakeLogProvider();
            IStatusProvider statusProvider = new FakeStatusProvider();
            TargetSettings targetSettings = new FakeSettingsProvider().GetTargetSettings();
            TestRetriever testRetriever = new TestRetriever(logProvider, statusProvider);
            AzureContext azureContext = new AzureContext(testRetriever, targetSettings);
            azureContext.AzureEnvironment = azureEnvironment;
            azureContext.TokenProvider = new FakeTokenProvider();
            azureContext.AzureRetriever.LoadRestCache(restResponseFile);
            List<AzureTenant> tenants = await azureContext.GetAzureARMTenants(true);


            List<AzureSubscription> subscriptions = tenants[0].Subscriptions;
            await azureContext.SetSubscriptionContext(subscriptions[0]);

            return azureContext;
        }
       
        public static async Task<AzureGenerator> SetupTemplateGenerator(AzureContext azureContext)
        {
            ITelemetryProvider telemetryProvider = new FakeTelemetryProvider();
            AzureGenerator azureGenerator = new AzureGenerator(azureContext.LogProvider, azureContext.StatusProvider);

            return azureGenerator;
        }

        public static JObject GetJsonData(MemoryStream closedStream)
        {
            var newStream = new MemoryStream(closedStream.ToArray());
            var reader = new StreamReader(newStream);
            var templateText = reader.ReadToEnd();
            return JObject.Parse(templateText);
        }

        internal static async Task<Azure.MigrationTarget.ResourceGroup> GetTargetResourceGroup(AzureContext azureContext)
        {
            TargetSettings targetSettings = new FakeSettingsProvider().GetTargetSettings();
            Azure.MigrationTarget.ResourceGroup targetResourceGroup = new Azure.MigrationTarget.ResourceGroup(null, targetSettings, null); // TODO: LogProvider
            targetResourceGroup.TargetLocation = azureContext.AzureSubscription.Locations[0];
            return targetResourceGroup;
        }
    }
}

