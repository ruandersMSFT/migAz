// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

//using MigAz.Azure.Core.Interface;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MigAz.Azure.MigrationTarget
//{
//    public class VirtualMachineImage : IMigrationTarget
//    {
//        private string _TargetName = String.Empty;

//        private VirtualMachineImage() { }

//        public VirtualMachineImage(AzureContext azureContext, Arm.VirtualMachineImage virtualMachineImage)
//        {
//            this.Source = virtualMachineImage;
//            this.TargetName = virtualMachineImage.Name;
//        }

//        public string TargetName
//        {
//            get { return _TargetName; }
//            set { _TargetName = value.Trim().Replace(" ", String.Empty); }
//        }

//        public override string ToString()
//        {
//            return this.TargetName;
//        }
//    }
//}

