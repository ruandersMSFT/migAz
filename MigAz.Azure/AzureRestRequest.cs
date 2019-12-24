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
    public class AzureRestRequest
    {
        private Guid _RequestGUID = Guid.NewGuid();
        private String _Url;
        private bool _UseCached = true;
        private AuthenticationResult _AuthenticationResult = null;
        private String _AccessToken = String.Empty;
        private String _Method = "GET";
        private Dictionary<String, String> _Headers = new Dictionary<string, string>();

        private AzureRestRequest() { }

        public AzureRestRequest(String url)
        {
            this._Url = url;
        }

        public AzureRestRequest(String url, AuthenticationResult authenticationResult)
        {
            this._Url = url;

            this._AuthenticationResult = authenticationResult;
            if (authenticationResult != null)
                this._AccessToken = authenticationResult.AccessToken;
        }

        public AzureRestRequest(String url, String accessToken, bool useCached = true)
        {
            this._Url = url;
            this._AccessToken = accessToken;
            this._UseCached = useCached;
        }

        public AzureRestRequest(String url, AuthenticationResult authenticationResult, string method, bool useCached = true)
        {
            this._Url = url;
            this._Method = method;
            this._UseCached = useCached;

            this._AuthenticationResult = authenticationResult;
            if (authenticationResult != null)
                this._AccessToken = authenticationResult.AccessToken;
        }
        public AzureRestRequest(String url, AuthenticationResult authenticationResult, bool useCached = true)
        {
            this._Url = url;
            this._UseCached = useCached;

            this._AuthenticationResult = authenticationResult;
            if (authenticationResult != null)
                this._AccessToken = authenticationResult.AccessToken;
        }

        public AzureRestRequest(String url, String accessToken, string method, bool useCached = true)
        {
            this._Url = url;
            this._AccessToken = accessToken;
            this._Method = method;
            this._UseCached = useCached;
        }


        public Guid RequestGuid
        {
            get { return _RequestGUID; }
        }
        public String Url
        {
            get { return _Url; }
        }

        public bool UseCached
        {
            get { return _UseCached; }
        }

        public String AccessToken
        {
            get { return _AccessToken; }
        }
        
        public String Method
        {
            get { return _Method; }
        }

        public Dictionary<String, String> Headers
        {
            get { return _Headers; }
        }
    }
}

