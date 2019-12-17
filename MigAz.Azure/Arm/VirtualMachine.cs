// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Interface;
using MigAz.Azure.Core.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MigAz.Azure.Arm
{
    public class VirtualMachine : ArmResource, IVirtualMachine
    {
        private VMSize _VMSize;

        private VirtualMachine() : base(null, null) { }
        private VirtualMachine(AzureSubscription azureSubscription, JToken resourceToken) : base(azureSubscription, resourceToken) { }

        public static Task<VirtualMachine> CreateAsync(AzureSubscription azureSubscription, JToken resourceToken) 
        {
            var ret = new VirtualMachine(azureSubscription, resourceToken);
            return ret.InitializeAsync();
        }

        private async Task<VirtualMachine> InitializeAsync()
        {
            _VMSize = await this.ResourceGroup.Location.SeekVmSize(this.VmSizeString);
            return this;
        }

        public bool HasPlan
        {
            get
            {
                return ResourceToken["plan"] != null;
            }
        }

        public Guid VmId => new Guid((string)ResourceToken.SelectToken("properties.vmId"));
        private string VmSizeString => (string)ResourceToken.SelectToken("properties.hardwareProfile.vmSize");

        public VMSize VmSize
        {
            get { return _VMSize; }
        }

        public string OSVirtualHardDiskOS
        {
            get
            {
                if (ResourceToken["properties"] == null || ResourceToken["properties"]["storageProfile"] == null || ResourceToken["properties"]["storageProfile"]["osDisk"] == null || ResourceToken["properties"]["storageProfile"]["osDisk"]["osType"] == null)
                    return null;

                return (string)ResourceToken["properties"]["storageProfile"]["osDisk"]["osType"];
            }
        }


        internal string AvailabilitySetId
        {
            get
            {
                return (string)ResourceToken.SelectToken("properties.availabilitySet.id");
            }
        }

        public List<IArmDisk> DataDisks
        {
            get
            {
                return new List<IArmDisk>(); // todo now russell
                //if (ResourceToken["properties"] != null)
                //{
                //    if (ResourceToken["properties"]["storageProfile"] != null)
                //    {
                //        foreach (JToken dataDiskToken in ResourceToken["properties"]["storageProfile"]["dataDisks"])
                //        {
                //            this.AzureSubscription.LogProvider.WriteLog("Arm.VirutalMachine Ctor", "Constructing Data Disk");

                //            if (dataDiskToken["vhd"] == null)
                //            {
                //                // Find and Link to Managed Disk
                //                if (dataDiskToken["managedDisk"] == null)
                //                {
                //                    if (dataDiskToken["name"] != null)
                //                    {
                //                        string managedDiskName = dataDiskToken["name"].ToString();
                //                        this.AzureSubscription.LogProvider.WriteLog("Arm.VirutalMachine Ctor", "Seeking Managed Disk By Name '" + managedDiskName + "'.  Managed Disk object not available for By Id seek.");

                //                        ManagedDisk dataDisk = this.AzureSubscription.SeekManagedDiskByName(managedDiskName);
                //                        if (dataDisk != null)
                //                        {
                //                            dataDisk.SetParentVirtualMachine(this, dataDiskToken);
                //                            _DataDisks.Add(dataDisk);
                //                        }
                //                        else
                //                        {

                //                        }
                //                    }
                //                    else
                //                    {

                //                    }
                //                }
                //                else
                //                {
                //                    if (dataDiskToken["managedDisk"]["id"] != null)
                //                    {
                //                        string managedDiskId = dataDiskToken["managedDisk"]["id"].ToString();
                //                        this.AzureSubscription.LogProvider.WriteLog("Arm.VirutalMachine Ctor", "Seeking Managed Disk By Id '" + managedDiskId + "'.");

                //                        ManagedDisk dataDisk = (Arm.ManagedDisk)this.AzureSubscription.SeekResourceById(managedDiskId).Result;
                //                        if (dataDisk != null)
                //                        {
                //                            dataDisk.SetParentVirtualMachine(this, dataDiskToken);
                //                            _DataDisks.Add(dataDisk);
                //                        }
                //                        else
                //                        {

                //                        }
                //                    }

                //                }
                //            }
                //            else
                //            {
                //                _DataDisks.Add(new ClassicDisk(this, dataDiskToken));
                //            }
                //        }

                //    }
                //}
            }
        }

        public IArmDisk OSVirtualHardDisk
        {
            get
            {
                if (ResourceToken.SelectToken("properties.storageProfile.osDisk.managedDisk.id") != null)
                {
                    string managedDiskId = (string)ResourceToken.SelectToken("properties.storageProfile.osDisk.managedDisk.id");
                    this.AzureSubscription.LogProvider.WriteLog("Arm.VirutalMachine Ctor", "Seeking Managed Disk By Id '" + managedDiskId + "'.");

                    ManagedDisk osDisk = (Arm.ManagedDisk)this.AzureSubscription.SeekResourceById(managedDiskId).Result;
                    return osDisk;
                }
                else if (ResourceToken.SelectToken("properties.storageProfile.osDisk.name") != null)
                {
                    if (ResourceToken["properties"]["storageProfile"]["osDisk"]["vhd"] == null)
                    {
                        // Find and Link to Managed Disk
                        if (ResourceToken["properties"]["storageProfile"]["osDisk"]["managedDisk"] == null)
                        {
                            if (ResourceToken["properties"]["storageProfile"]["osDisk"]["name"] != null)
                            {
                                string managedDiskName = ResourceToken["properties"]["storageProfile"]["osDisk"]["name"].ToString();
                                this.AzureSubscription.LogProvider.WriteLog("Arm.VirutalMachine Ctor", "Seeking Managed Disk By Name '" + managedDiskName + "'.  Managed Disk object not available for By Id seek.");

                                ManagedDisk osDisk = this.AzureSubscription.SeekManagedDiskByName(managedDiskName);
                                return osDisk;
                            }
                        }
                        else
                        {
                            return new ClassicDisk(this, ResourceToken["properties"]["storageProfile"]["osDisk"]);
                        }
                    }
                }

                return null;
            }
        }

        public List<string> NetworkInterfaceIds
        {
            get
            {
                List<string> virtualMachineIds = ResourceToken.SelectToken("properties.networkProfile.networkInterfaces").Select(x => (string)x.SelectToken("id")).ToList();
                return virtualMachineIds;
            }
        }

        // todo now russell
        //public NetworkInterface PrimaryNetworkInterface
        //{
        //    get
        //    {
        //        foreach (NetworkInterface networkInterface in this.NetworkInterfaces)
        //        {
        //            if (networkInterface.IsPrimary.HasValue && networkInterface.IsPrimary.Value)
        //                return networkInterface;
        //        }

        //        return null;
        //    }
        //}

    }
}

