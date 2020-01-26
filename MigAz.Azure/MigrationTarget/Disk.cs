// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MigAz.Azure.Interface;
using MigAz.Azure.Core;
using MigAz.Azure.Core.ArmTemplate;
using MigAz.Azure.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigAz.Azure.Arm;

namespace MigAz.Azure.MigrationTarget
{
    public class Disk : Core.MigrationTarget //<IDisk>
    {
        private Int32 _DiskSizeInGB = 0;
        private VirtualMachine _ParentVirtualMachine;

        #region Constructors

        private Disk() : base(null, ArmConst.MicrosoftCompute, ArmConst.Disks, null, null) { }

        public Disk(AzureSubscription azureSubscription, Asm.Disk sourceDisk, VirtualMachine parentVirtualMachine, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftCompute, ArmConst.Disks, targetSettings, logProvider)
        {
            this.Source = sourceDisk;
            this._ParentVirtualMachine = parentVirtualMachine;
            this.TargetStorage = new ManagedDiskStorage(sourceDisk);

            this.SetTargetName(sourceDisk.DiskName, targetSettings);
            this.Lun = sourceDisk.Lun;
            this.HostCaching = sourceDisk.HostCaching;
            this.DiskSizeInGB = sourceDisk.DiskSizeGb;
            this.TargetStorageAccountBlob = sourceDisk.StorageAccountBlob;
            this.SourceStorageAccount = sourceDisk.SourceStorageAccount;
        }

        public Disk(AzureSubscription azureSubscription, IArmDisk sourceDisk, VirtualMachine parentVirtualMachine, TargetSettings targetSettings, ILogProvider logProvider) : base(azureSubscription, ArmConst.MicrosoftCompute, ArmConst.Disks, targetSettings, logProvider)
        {
            this.Source = (IDisk)sourceDisk;
            this._ParentVirtualMachine = parentVirtualMachine;

            if (sourceDisk != null)
            {
                if (sourceDisk.GetType() == typeof(Azure.Arm.ClassicDisk))
                {
                    Azure.Arm.ClassicDisk armDisk = (Azure.Arm.ClassicDisk)sourceDisk;
                    this.SetTargetName(armDisk.Name, parentVirtualMachine.TargetSettings);
                }
                else if (sourceDisk.GetType() == typeof(Azure.Arm.ManagedDisk))
                {
                    Azure.Arm.ManagedDisk armManagedDisk = (Azure.Arm.ManagedDisk)sourceDisk;
                    this.SetTargetName(armManagedDisk.Name, targetSettings);
                }
            }
        }

        public Disk(AzureSubscription azureSubscription, IArmDisk sourceDisk, TargetSettings targetSettings, ILogProvider logProvider) : this(azureSubscription, sourceDisk, null, targetSettings, logProvider)
        {
        }

        #endregion

        public VirtualMachine ParentVirtualMachine
        {
            get { return _ParentVirtualMachine; }
            set { _ParentVirtualMachine = value; }
        }

        public string HostCaching
        {
            get;set;
        }

        public bool IsEncrypted { get; set; }

        public string DiskEncryptionKeySourceVaultId { get; set; }
        public string DiskEncryptionKeySecretUrl { get; set; }
        public string KeyEncryptionKeySourceVaultId { get; set; }
        public string KeyEncryptionKeyKeyUrl { get; set; }

        public Int64? Lun
        {
            get;
            set;
        }

        public Int32 DiskSizeInGB
        {
            get { return _DiskSizeInGB; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("DiskSizeInGB cannot be negative.");

                // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/disks-types
                // https://azure.microsoft.com/en-us/blog/announcing-ultra-ssd-the-next-generation-of-azure-disks-technology-preview/
                if (value > 65536)
                    throw new ArgumentException("DiskSizeInGB cannot be greater than 65536 GB.");
                
                _DiskSizeInGB = value;
            }
        }

        public IStorageAccount SourceStorageAccount
        {
            get; private set;
        }

        public IStorageTarget TargetStorage
        {
            get;
            set;
        }
        
        public string TargetStorageAccountContainer
        {
            get { return "vhds"; }
        }

        public string TargetStorageAccountBlob
        {
            get; set;
        }

        public StorageAccountType StorageAccountType
        {
            get {
                if (this.TargetStorage != null)
                    return this.TargetStorage.StorageAccountType;

                return StorageAccountType.Premium_LRS;
            }
        }

        public string TargetMediaLink
        {
            get
            {
                return "https://" + TargetStorage.ToString() + "." + TargetStorage.BlobStorageNamespace + "/vhds/" + this.TargetStorageAccountBlob;
            }
        }

        public bool IsManagedDisk
        {
            get
            {
                return this.TargetStorage != null && this.TargetStorage.GetType() == typeof(Azure.MigrationTarget.ManagedDiskStorage);
            }
        }

        public bool IsUnmanagedDisk
        {
            get
            {
                return this.TargetStorage != null && (this.TargetStorage.GetType() == typeof(Azure.MigrationTarget.StorageAccount) || this.TargetStorage.GetType() == typeof(Azure.Arm.StorageAccount));
            }
        }
        public string ReferenceId
        {
            get
            {
                if (this.TargetStorage != null && this.TargetStorage.GetType() == typeof(Azure.MigrationTarget.ManagedDiskStorage))
                    return "[concat(subscription().id, '/resourcegroups/', resourceGroup().name, '/providers/Microsoft.Compute/disks/', '" + this.ToString() + "')]";
                else return 
                        String.Empty;
            }
        }

        public bool IsSmallerThanSourceDisk
        {
            get
            {
                if (this.Source == null)
                    return false;

                if (this.DiskSizeInGB < ((IDisk)this.Source).DiskSizeGb)
                    return true;

                return false;
            }
        }

        public bool IsTargetLunDifferentThanSourceLun
        {
            get
            {
                if (this.Source == null)
                    return false; // No source disk to compare to, thus false
                else
                    return this.Lun != ((IDisk)this.Source).Lun;
            }
        }

        public override string ImageKey { get { return "Disk"; } }

        public override string FriendlyObjectName { get { return "Disk"; } }


        public override void SetTargetName(string targetName, TargetSettings targetSettings)
        {
            this.TargetName = targetName.Trim().Replace(" ", String.Empty);
            this.TargetNameResult = this.TargetName;
        }

        public override async Task RefreshFromSource()
        {
            if (this.Source != null)
            {
                if (this.Source.GetType() == typeof(Azure.Arm.ClassicDisk))
                {
                    Azure.Arm.ClassicDisk armDisk = (Azure.Arm.ClassicDisk)this.Source;

                    if (this.TargetStorage == null)
                        this.TargetStorage = new ManagedDiskStorage(armDisk);

                    //this.SetTargetName(armDisk.Name, this.TargetSettings);
                    this.Lun = armDisk.Lun;
                    this.HostCaching = armDisk.Caching;
                    this.DiskSizeInGB = armDisk.DiskSizeGb;
                    this.TargetStorageAccountBlob = armDisk.StorageAccountBlob;
                    this.SourceStorageAccount = armDisk.SourceStorageAccount;
                    this.IsEncrypted = armDisk.IsEncrypted;
                    this.DiskEncryptionKeySourceVaultId = armDisk.DiskEncryptionKeySourceVaultId;
                    this.DiskEncryptionKeySecretUrl = armDisk.DiskEncryptionKeySecretUrl;
                    this.KeyEncryptionKeySourceVaultId = armDisk.KeyEncryptionKeySourceVaultId;
                    this.KeyEncryptionKeyKeyUrl = armDisk.KeyEncryptionKeyKeyUrl;
                }
                else if (this.Source.GetType() == typeof(Azure.Arm.ManagedDisk))
                {
                    Azure.Arm.ManagedDisk armManagedDisk = (Azure.Arm.ManagedDisk)this.Source;

                    if (this.TargetStorage == null)
                        this.TargetStorage = new ManagedDiskStorage(armManagedDisk);

                    //this.SetTargetName(armManagedDisk.Name, this.TargetSettings);
                    this.DiskSizeInGB = armManagedDisk.DiskSizeGb;
                    this.IsEncrypted = armManagedDisk.IsEncrypted;
                    this.DiskEncryptionKeySourceVaultId = armManagedDisk.DiskEncryptionKeySourceVaultId;
                    this.DiskEncryptionKeySecretUrl = armManagedDisk.DiskEncryptionKeySecretUrl;
                    this.KeyEncryptionKeySourceVaultId = armManagedDisk.KeyEncryptionKeySourceVaultId;
                    this.KeyEncryptionKeyKeyUrl = armManagedDisk.KeyEncryptionKeyKeyUrl;
                }
            }
        }
    }
}

