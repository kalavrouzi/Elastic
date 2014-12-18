﻿using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class CustomLoginProvider : LoginProvider
    {
        public static string ProviderName = "custom";

        public override string Name{
        get{
            return ProviderName;
        }
        }

        public CustomLoginProvider(IServiceTokenHandler tokenHandler )
            : base(tokenHandler)
        {
            this.TokenLifetime = new TimeSpan(30, 0, 0, 0);
        }

        public override void ConfigureMiddleware(IAppBuilder appBuilder, ServiceSettingsDictionary settings)
        {
            return;
        }

        public override ProviderCredentials ParseCredentials(JObject serialized)
        {
            if (serialized == null)
            {
                throw new ArgumentNullException("serialized");
            }

            return serialized.ToObject<CustomLoginProviderCredentials>();
        }
        public override ProviderCredentials CreateCredentials(ClaimsIdentity claimsIdentity )
        {
            
            if (claimsIdentity == null)
            {
                throw new ArgumentNullException("claimsIdentity");
            }

            string username = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            CustomLoginProviderCredentials credentials = new CustomLoginProviderCredentials
            {
                UserId = this.TokenHandler.CreateUserId(this.Name, username),
                ShardKey = claimsIdentity.FindFirst("shardKey").Value
            };
            return credentials;
        }

    }

    public class CustomLoginProviderCredentials : ProviderCredentials
    {
        public string ShardKey { get; set; }
        public CustomLoginProviderCredentials()
            : base(CustomLoginProvider.ProviderName)
        {
        }
    }
}