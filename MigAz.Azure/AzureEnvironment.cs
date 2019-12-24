// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure
{
    public enum AzureEnvironmentType
    {
        Azure,
        AzureStack
    }

    public class AzureEnvironment
    {
        private AzureEnvironmentType _AzureEnvironmentType = AzureEnvironmentType.Azure;
        private bool _IsUserDefined = true;
        private String _AzureStackAdminManagementUrl = String.Empty;

        #region Constructors

        public AzureEnvironment()
        {
            this.Name = "NewAzureEnvironment";
            this.AzureEnvironmentType = AzureEnvironmentType.Azure;
        }

        public AzureEnvironment(
            AzureEnvironmentType azureEnvironmentType,
            String name,
            String adTenant,
            String azureLoginUrl,
            String graphApiUrl,
            String asmServiceManagementUrl,
            String armServiceManagementUrl,
            String storageEndpointUrl,
            String blobEndpointUrl,
            String activeDirectoryServiceEndpointResourceId,
            String galleryUrl,
            String sqlDatabaseDnsSuffix,
            String trafficManagerDnsSuffix,
            String azureKeyVaultDnsSuffix)
        {
            this.AzureEnvironmentType = azureEnvironmentType;
            this.Name = name;
            this.AdTenant = adTenant;
            this.ServiceManagementUrl = asmServiceManagementUrl;
            this.ResourceManagerEndpoint = armServiceManagementUrl;
            this.ActiveDirectoryEndpoint = azureLoginUrl;
            this.StorageEndpointSuffix = storageEndpointUrl;
            this.BlobEndpointUrl = blobEndpointUrl;
            this.GraphEndpoint = graphApiUrl;
            this.ActiveDirectoryServiceEndpointResourceId = activeDirectoryServiceEndpointResourceId;
            this.GalleryUrl = galleryUrl;
            this.SqlDatabaseDnsSuffix = sqlDatabaseDnsSuffix;
            this.TrafficManagerDnsSuffix = trafficManagerDnsSuffix;
            this.AzureKeyVaultDnsSuffix = azureKeyVaultDnsSuffix;
        }


        public AzureEnvironment(AzureEnvironment azureEnvironment)
        {
            this.AzureEnvironmentType = azureEnvironment.AzureEnvironmentType;
            this.Name = "ClonedAzureEnvironment";
            this.AdTenant = azureEnvironment.AdTenant;
            this.AzureStackAdminManagementUrl = azureEnvironment.AzureStackAdminManagementUrl;
            this.ServiceManagementUrl = azureEnvironment.ServiceManagementUrl;
            this.ResourceManagerEndpoint = azureEnvironment.ResourceManagerEndpoint;
            this.ActiveDirectoryEndpoint = azureEnvironment.ActiveDirectoryEndpoint;
            this.StorageEndpointSuffix = azureEnvironment.StorageEndpointSuffix;
            this.BlobEndpointUrl = azureEnvironment.BlobEndpointUrl;
            this.GraphEndpoint = azureEnvironment.GraphEndpoint;
            this.ActiveDirectoryServiceEndpointResourceId = azureEnvironment.ActiveDirectoryServiceEndpointResourceId;
            this.GalleryUrl = azureEnvironment.GalleryUrl;
            this.SqlDatabaseDnsSuffix = azureEnvironment.SqlDatabaseDnsSuffix;
            this.TrafficManagerDnsSuffix = azureEnvironment.TrafficManagerDnsSuffix;
            this.AzureKeyVaultDnsSuffix = azureEnvironment.AzureKeyVaultDnsSuffix;
        }

        #endregion

        public AzureEnvironmentType AzureEnvironmentType
        {
            get { return _AzureEnvironmentType; }
            set
            {
                _AzureEnvironmentType = value;

                switch (value)
                {
                    case AzureEnvironmentType.Azure:
                        if (value == AzureEnvironmentType.Azure)
                            this.AzureStackAdminManagementUrl = String.Empty;
                        break;
                    case AzureEnvironmentType.AzureStack:
#if DEBUG
                        if (this.AzureStackAdminManagementUrl == String.Empty)
                        {
                            this.AzureStackAdminManagementUrl = "https://adminmanagement.local.azurestack.external";
                        }
#endif
                        break;
                }

            }
        }
        public String Name { get; set; }

        public String AdTenant { get; set; }

        public String SqlDatabaseDnsSuffix { get; set; }
        public String TrafficManagerDnsSuffix { get; set; }
        public String AzureKeyVaultDnsSuffix { get; set; }

        public String AzureStackAdminManagementUrl
        {
            get { return _AzureStackAdminManagementUrl; }
            set
            {
                if (_AzureEnvironmentType == AzureEnvironmentType.AzureStack)
                    _AzureStackAdminManagementUrl = value;
                else if (value != String.Empty)
                    throw new ArgumentException("Cannot set Azure Stack Admin Management URL when AzureEnvironmentType is not AzureStack.");
            }
        }

        public String ServiceManagementUrl { get; set; }
        public String ResourceManagerEndpoint { get; set; }
        public String ActiveDirectoryEndpoint { get; set; }
        public String StorageEndpointSuffix { get; set; }
        public String BlobEndpointUrl { get; set; }
        public String GraphEndpoint { get; set; }
        public String GalleryUrl { get; set; }
        public String ActiveDirectoryServiceEndpointResourceId { get; set; }
        public bool IsUserDefined
        {
            get { return _IsUserDefined; }
            private set { _IsUserDefined = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public static List<AzureEnvironment> GetAzureEnvironments()
        {
            List<AzureEnvironment> azureEnvironments = new List<AzureEnvironment>();

            AzureEnvironment azureCloud = new AzureEnvironment(
                    AzureEnvironmentType.Azure,
                    "AzureCloud",
                    "Common", // AdTenant
                    "https://login.microsoftonline.com/", // logonUrl
                    "https://graph.windows.net/", //GraphUrl
                    "https://management.core.windows.net/", // ASM
                    "https://management.azure.com/", // ARM
                    "core.windows.net", // Storage
                    "blob.core.windows.net", // Blob
                    "https://management.core.windows.net/",  // ActiveDirectoryServiceEndpointResourceId
                    "https://gallery.azure.com/", // GalleryUrl
                    ".database.chinacloudapi.cn", // SqlDatabaseDnsSuffix
                    "trafficmanager.cn", // TrafficManagerDnsSuffix
                    "vault.azure.cn" // AzureKeyVaultDnsSuffix
                    ); 
            azureCloud.IsUserDefined = false;
            azureEnvironments.Add(azureCloud);

            // China Endpoints - https://msdn.microsoft.com/en-us/library/azure/dn578439.aspx
            AzureEnvironment azureChinaCloud = new AzureEnvironment(
                AzureEnvironmentType.Azure,
                "AzureChinaCloud",
                "Common", // AdTenant
                "https://login.chinacloudapi.cn/", // logonUrl
                "https://graph.chinacloudapi.cn/", //GraphUrl
                "https://management.core.chinacloudapi.cn/", // ASM
                "https://management.chinacloudapi.cn/", // ARM
                "core.chinacloudapi.cn", // Storage
                "blob.core.chinacloudapi.cn", // Blob
                "https://management.core.chinacloudapi.cn/",  // ActiveDirectoryServiceEndpointResourceId
                "https://gallery.azure.com/", // GalleryUrl
                ".database.windows.net", // SqlDatabaseDnsSuffix
                "trafficmanager.net", // TrafficManagerDnsSuffix
                "vault.azure.net" // AzureKeyVaultDnsSuffix
                );
            azureChinaCloud.IsUserDefined = false;
            azureEnvironments.Add(azureChinaCloud);

            // Germany Endpoints - https://blogs.msdn.microsoft.com/azuregermany/2016/08/18/endpoints-in-microsoft-cloud-germany/
            AzureEnvironment azureGermanCloud = new AzureEnvironment(
                AzureEnvironmentType.Azure,
                "AzureGermanCloud",
                "Common", // AdTenant
                "https://login.microsoftonline.de/", // logonUrl
                "https://graph.cloudapi.de/", //GraphUrl
                "https://management.core.cloudapi.de/", // ASM
                "https://management.microsoftazure.de/", // ARM
                "core.cloudapi.de", // Storage
                "blob.core.cloudapi.de", // Blob
                "https://management.core.cloudapi.de/",  // ActiveDirectoryServiceEndpointResourceId
                "https://gallery.azure.com/", // GalleryUrl
                ".database.cloudapi.de", // SqlDatabaseDnsSuffix
                "azuretrafficmanager.de", // TrafficManagerDnsSuffix
                "vault.microsoftazure.de" // AzureKeyVaultDnsSuffix
                );
            azureGermanCloud.IsUserDefined = false;
            azureEnvironments.Add(azureGermanCloud);


            // US Gov Endpoints - https://docs.microsoft.com/en-us/azure/azure-government/documentation-government-developer-guide

            AzureEnvironment azureUSGovernment = new AzureEnvironment(
                AzureEnvironmentType.Azure,
                "AzureUSGovernment",
                "Common", // AdTenant
                "https://login.microsoftonline.us/", // logonUrl
                "https://graph.windows.net/", //GraphUrl
                "https://management.core.usgovcloudapi.net/", // ASM
                "https://management.usgovcloudapi.net/", // ARM
                "core.usgovcloudapi.net", // Storage
                "blob.core.usgovcloudapi.net", // Blob
                "https://management.core.usgovcloudapi.net/",  // ActiveDirectoryServiceEndpointResourceId
                "https://gallery.azure.com/", // GalleryUrl
                ".database.usgovcloudapi.net", // SqlDatabaseDnsSuffix
                "usgovtrafficmanager.net", // TrafficManagerDnsSuffix
                "vault.usgovcloudapi.net" // AzureKeyVaultDnsSuffix
                );
            azureUSGovernment.IsUserDefined = false;
            azureEnvironments.Add(azureUSGovernment);


            return azureEnvironments;
        }

        public AzureCloudInstance GetAzureCloudInstance()
        {
            if (this.AzureEnvironmentType == AzureEnvironmentType.Azure)
            {
                switch (this.Name)
                {
                    case "AzureCloud":
                        return AzureCloudInstance.AzurePublic;
                    case "AzureChinaCloud":
                        return AzureCloudInstance.AzureChina;
                    case "AzureGermanCloud":
                        return AzureCloudInstance.AzureGermany;
                    case "AzureUSGovernment":
                        return AzureCloudInstance.AzureUsGovernment;

                }
            }

            return AzureCloudInstance.None;
        }
    }
}