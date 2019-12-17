// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigAz.Azure.Generator.AsmToArm;
using MigAz.Tests.Fakes;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using MigAz.Azure;
using MigAz.Azure.UserControls;
using MigAz.Azure.MigrationTarget;
using MigAz.Azure.Core;
using MigAz.Azure.Core.Interface;

namespace MigAz.Tests
{
    [TestClass]
    public class StorageTests
    {
        [TestMethod]
        public async Task LoadASMObjectsFromSampleOfflineFile()
        {
            string restResponseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDocs\\NewTest1\\AsmObjectsOffline.json");
            TargetSettings targetSettings = new FakeSettingsProvider().GetTargetSettings();
            AzureEnvironment azureEnvironment = AzureEnvironment.GetAzureEnvironments()[0];
            AzureContext azureContextUSCommercial = await TestHelper.SetupAzureContext(azureEnvironment, restResponseFile);
            await azureContextUSCommercial.AzureSubscription.InitializeChildrenAsync(true);
            await azureContextUSCommercial.AzureSubscription.BindAsmResources(targetSettings);

            AzureGenerator templateGenerator = await TestHelper.SetupTemplateGenerator(azureContextUSCommercial);

            var artifacts = new ExportArtifacts(azureContextUSCommercial.AzureSubscription);
            artifacts.ResourceGroup = await TestHelper.GetTargetResourceGroup(azureContextUSCommercial);
            //foreach (Azure.MigrationTarget.StorageAccount s in azureContextUSCommercial.AzureRetriever.AsmTargetStorageAccounts)
            //{
            //    artifacts.StorageAccounts.Add(s);
            //}
            await artifacts.ValidateAzureResources();
            Assert.IsFalse(artifacts.HasErrors, "Template Generation cannot occur as the are error(s).");

            templateGenerator.ExportArtifacts = artifacts;
            await templateGenerator.GenerateStreams();

            JObject templateJson = JObject.Parse(await templateGenerator.GetTemplateString());

            Assert.AreEqual(0, templateJson["resources"].Children().Count());

            //var resource = templateJson["resources"].Single();
            //Assert.AreEqual("Microsoft.Storage/storageAccounts", resource["type"].Value<string>());
            //Assert.AreEqual("asmtest8155v2", resource["name"].Value<string>());
            //Assert.AreEqual("[resourceGroup().location]", resource["location"].Value<string>());
            //Assert.AreEqual("Standard_LRS", resource["properties"]["accountType"].Value<string>());
        }

        [TestMethod]
        public async Task LoadARMObjectsFromSampleOfflineFile()
        {
            string restResponseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDocs\\NewTest1\\ArmObjectsOffline.json");
            TargetSettings targetSettings = new FakeSettingsProvider().GetTargetSettings();
            AzureEnvironment azureEnvironment = AzureEnvironment.GetAzureEnvironments()[0];
            AzureContext azureContextUSCommercial = await TestHelper.SetupAzureContext(azureEnvironment, restResponseFile);
            await azureContextUSCommercial.AzureSubscription.InitializeChildrenAsync(true);
            await azureContextUSCommercial.AzureSubscription.BindArmResources(targetSettings);

            AzureGenerator templateGenerator = await TestHelper.SetupTemplateGenerator(azureContextUSCommercial);

            var artifacts = new ExportArtifacts(azureContextUSCommercial.AzureSubscription);
            artifacts.ResourceGroup = await TestHelper.GetTargetResourceGroup(azureContextUSCommercial);

            //foreach (Azure.MigrationTarget.StorageAccount s in azureContextUSCommercial.AzureRetriever.ArmTargetStorageAccounts)
            //{
            //    artifacts.StorageAccounts.Add(s);
            //}

            await artifacts.ValidateAzureResources();

            Assert.IsFalse(artifacts.HasErrors, "Template Generation cannot occur as the are error(s).");

            templateGenerator.ExportArtifacts = artifacts;
            await templateGenerator.GenerateStreams();

            JObject templateJson = JObject.Parse(await templateGenerator.GetTemplateString());

            Assert.AreEqual(0, templateJson["resources"].Children().Count());

            //var resource = templateJson["resources"].First();
            //Assert.AreEqual("Microsoft.Storage/storageAccounts", resource["type"].Value<string>());
            //Assert.AreEqual("manageddiskdiag857v2", resource["name"].Value<string>());
            //Assert.AreEqual("[resourceGroup().location]", resource["location"].Value<string>());
            //Assert.AreEqual("Standard_LRS", resource["properties"]["accountType"].Value<string>());
        }
        [TestMethod]
        public async Task LoadARMObjectsFromSampleOfflineFile2()
        {
            string restResponseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDocs\\NewTest1\\temp.json");
            TargetSettings targetSettings = new FakeSettingsProvider().GetTargetSettings();
            AzureEnvironment azureEnvironment = AzureEnvironment.GetAzureEnvironments()[0];
            AzureContext azureContextUSCommercial = await TestHelper.SetupAzureContext(azureEnvironment, restResponseFile);
            await azureContextUSCommercial.AzureSubscription.InitializeChildrenAsync(true);
            await azureContextUSCommercial.AzureSubscription.BindArmResources(targetSettings);

            AzureGenerator templateGenerator = await TestHelper.SetupTemplateGenerator(azureContextUSCommercial);

            var artifacts = new ExportArtifacts(azureContextUSCommercial.AzureSubscription);
            artifacts.ResourceGroup = await TestHelper.GetTargetResourceGroup(azureContextUSCommercial);


            artifacts.VirtualMachines.Add(azureContextUSCommercial.AzureSubscription.ArmTargetVirtualMachines[0]);
            artifacts.VirtualMachines[0].OSVirtualHardDisk.DiskSizeInGB = 128;

            await artifacts.ValidateAzureResources();

            Assert.IsNotNull(artifacts.SeekAlert("Network Interface Card (NIC) 'manageddisk01549-nic' utilizes Network Security Group (NSG) 'ManagedDisk01-nsg-nsg', but the NSG resource is not added into the migration template."));
            artifacts.NetworkSecurityGroups.Add(azureContextUSCommercial.AzureSubscription.ArmTargetNetworkSecurityGroups[0]);
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Network Interface Card (NIC) 'manageddisk01549-nic' utilizes Network Security Group (NSG) 'ManagedDisk01-nsg-nsg', but the NSG resource is not added into the migration template."));

            Assert.IsNotNull(artifacts.SeekAlert("Target Virtual Network 'ManagedDiskvnet-vnet' for Virtual Machine 'ManagedDisk01-vm' Network Interface 'manageddisk01549-nic' is invalid, as it is not included in the migration / template."));
            artifacts.VirtualNetworks.Add(azureContextUSCommercial.AzureSubscription.ArmTargetVirtualNetworks[0]);
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Target Virtual Network 'ManagedDiskvnet-vnet' for Virtual Machine 'ManagedDisk01-vm' Network Interface 'manageddisk01549-nic' is invalid, as it is not included in the migration / template."));

            Assert.IsNotNull(artifacts.SeekAlert("Network Interface Card (NIC) 'manageddisk01549-nic' IP Configuration 'ipconfig1' utilizes Public IP 'ManagedDisk01-ip', but the Public IP resource is not added into the migration template."));
            artifacts.PublicIPs.Add(azureContextUSCommercial.AzureSubscription.ArmTargetPublicIPs[0]);
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Network Interface Card (NIC) 'manageddisk01549-nic' IP Configuration 'ipconfig1' utilizes Public IP 'ManagedDisk01-ip', but the Public IP resource is not added into the migration template."));

            Assert.IsNotNull(artifacts.SeekAlert("Virtual Machine 'ManagedDisk01' references Managed Disk 'ManagedDisk01_OsDisk_1_e901d155e5404b6a912afb22e7a804a6' which has not been added as an export resource."));
            artifacts.Disks.Add(azureContextUSCommercial.AzureSubscription.ArmTargetManagedDisks[1]);
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Virtual Machine 'ManagedDisk01' references Managed Disk 'ManagedDisk01_OsDisk_1_e901d155e5404b6a912afb22e7a804a6' which has not been added as an export resource."));

            Assert.IsNotNull(artifacts.SeekAlert("Virtual Machine 'ManagedDisk01' references Managed Disk 'ManagedDataDisk01' which has not been added as an export resource."));
            artifacts.Disks.Add(azureContextUSCommercial.AzureSubscription.ArmTargetManagedDisks[0]);
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Virtual Machine 'ManagedDisk01' references Managed Disk 'ManagedDataDisk01' which has not been added as an export resource."));

            Assert.IsNotNull(artifacts.SeekAlert("Network Interface Card (NIC) 'manageddisk01549-nic' is used by Virtual Machine 'ManagedDisk01-vm', but is not included in the exported resources."));
            artifacts.NetworkInterfaces.Add(azureContextUSCommercial.AzureSubscription.ArmTargetNetworkInterfaces[0]);
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Network Interface Card (NIC) 'manageddisk01549-nic' is used by Virtual Machine 'ManagedDisk01-vm', but is not included in the exported resources."));

            Assert.IsTrue(artifacts.VirtualMachines[0].TargetSize.ToString() == "Standard_A1");
            await artifacts.ValidateAzureResources();
            Assert.IsFalse(artifacts.HasErrors, "Template Generation cannot occur as the are error(s).");

            ManagedDiskStorage managedDiskStorage = new ManagedDiskStorage((IDisk)artifacts.VirtualMachines[0].OSVirtualHardDisk.Source);
            managedDiskStorage.StorageAccountType = Azure.Core.Interface.StorageAccountType.Premium_LRS;
            artifacts.VirtualMachines[0].OSVirtualHardDisk.TargetStorage = managedDiskStorage;
            await artifacts.ValidateAzureResources();
            Assert.IsNotNull(artifacts.SeekAlert("Premium Disk based Virtual Machines must be of VM Series 'B', 'DS', 'DS v2', 'DS v3', 'GS', 'GS v2', 'Ls' or 'Fs'."));

            artifacts.VirtualMachines[0].TargetSize = artifacts.ResourceGroup.TargetLocation.SeekVmSize("Standard_DS2_v2");
            await artifacts.ValidateAzureResources();
            Assert.IsNull(artifacts.SeekAlert("Premium Disk based Virtual Machines must be of VM Series 'B', 'DS', 'DS v2', 'DS v3', 'GS', 'GS v2', 'Ls' or 'Fs'."));

            Assert.IsFalse(artifacts.HasErrors, "Template Generation cannot occur as the are error(s).");

            templateGenerator.ExportArtifacts = artifacts;
            await templateGenerator.GenerateStreams();

            JObject templateJson = JObject.Parse(await templateGenerator.GetTemplateString());

            Assert.AreEqual(7, templateJson["resources"].Children().Count());

            var resource = templateJson["resources"].First();
            Assert.AreEqual("Microsoft.Network/networkSecurityGroups", resource["type"].Value<string>());
            Assert.AreEqual("ManagedDisk01-nsg-nsg", resource["name"].Value<string>());
            Assert.AreEqual("[resourceGroup().location]", resource["location"].Value<string>());
        }

        [TestMethod]
        public async Task OfflineUITargetTreeViewTest()
        {
            string restResponseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDocs\\NewTest1\\temp.json");
            TargetSettings targetSettings = new FakeSettingsProvider().GetTargetSettings();
            AzureEnvironment azureEnvironment = AzureEnvironment.GetAzureEnvironments()[0];
            AzureContext azureContextUSCommercial = await TestHelper.SetupAzureContext(azureEnvironment, restResponseFile);
            await azureContextUSCommercial.AzureSubscription.InitializeChildrenAsync(true);
            await azureContextUSCommercial.AzureSubscription.BindArmResources(targetSettings);

            AzureGenerator templateGenerator = await TestHelper.SetupTemplateGenerator(azureContextUSCommercial);

            var artifacts = new ExportArtifacts(azureContextUSCommercial.AzureSubscription);
            artifacts.ResourceGroup = await TestHelper.GetTargetResourceGroup(azureContextUSCommercial);

            TargetTreeView targetTreeView = new TargetTreeView();
            targetTreeView.TargetSettings = targetSettings;

            await targetTreeView.AddMigrationTarget(azureContextUSCommercial.AzureSubscription.ArmTargetRouteTables[0]);
            targetTreeView.SeekAlertSource(azureContextUSCommercial.AzureSubscription.ArmTargetRouteTables[0]);
            Assert.IsTrue(targetTreeView.SelectedNode != null, "Selected Node is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag != null, "Selected Node Tag is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag.GetType() == azureContextUSCommercial.AzureSubscription.ArmTargetRouteTables[0].GetType(), "Object type mismatch");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag == azureContextUSCommercial.AzureSubscription.ArmTargetRouteTables[0], "Not the correct object");

            await targetTreeView.AddMigrationTarget(azureContextUSCommercial.AzureSubscription.ArmTargetVirtualNetworks[0]);
            targetTreeView.SeekAlertSource(azureContextUSCommercial.AzureSubscription.ArmTargetVirtualNetworks[0]);
            Assert.IsTrue(targetTreeView.SelectedNode != null, "Selected Node is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag != null, "Selected Node Tag is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag.GetType() == azureContextUSCommercial.AzureSubscription.ArmTargetVirtualNetworks[0].GetType(), "Object type mismatch");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag == azureContextUSCommercial.AzureSubscription.ArmTargetVirtualNetworks[0], "Not the correct object");

            await targetTreeView.AddMigrationTarget(azureContextUSCommercial.AzureSubscription.ArmTargetNetworkInterfaces[0]);
            targetTreeView.SeekAlertSource(azureContextUSCommercial.AzureSubscription.ArmTargetNetworkInterfaces[0]);
            Assert.IsTrue(targetTreeView.SelectedNode != null, "Selected Node is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag != null, "Selected Node Tag is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag.GetType() == azureContextUSCommercial.AzureSubscription.ArmTargetNetworkInterfaces[0].GetType(), "Object type mismatch");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag == azureContextUSCommercial.AzureSubscription.ArmTargetNetworkInterfaces[0], "Not the correct object");

            await targetTreeView.AddMigrationTarget(azureContextUSCommercial.AzureSubscription.ArmTargetManagedDisks[0]);
            targetTreeView.SeekAlertSource(azureContextUSCommercial.AzureSubscription.ArmTargetManagedDisks[0]);
            Assert.IsTrue(targetTreeView.SelectedNode != null, "Selected Node is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag != null, "Selected Node Tag is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag.GetType() == azureContextUSCommercial.AzureSubscription.ArmTargetManagedDisks[0].GetType(), "Object type mismatch");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag == azureContextUSCommercial.AzureSubscription.ArmTargetManagedDisks[0], "Not the correct object");

            await targetTreeView.AddMigrationTarget(azureContextUSCommercial.AzureSubscription.ArmTargetNetworkSecurityGroups[0]);
            targetTreeView.SeekAlertSource(azureContextUSCommercial.AzureSubscription.ArmTargetNetworkSecurityGroups[0]);
            Assert.IsTrue(targetTreeView.SelectedNode != null, "Selected Node is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag != null, "Selected Node Tag is null");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag.GetType() == azureContextUSCommercial.AzureSubscription.ArmTargetNetworkSecurityGroups[0].GetType(), "Object type mismatch");
            Assert.IsTrue(targetTreeView.SelectedNode.Tag == azureContextUSCommercial.AzureSubscription.ArmTargetNetworkSecurityGroups[0], "Not the correct object");

        }
    }
}

