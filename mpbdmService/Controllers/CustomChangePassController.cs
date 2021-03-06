﻿using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using mpbdmService.ElasticScale;
using mpbdmService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class CustomChangePassController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }

        // POST api/CustomLogin
        public HttpResponseMessage Post(ChangePassRequest changeRequest)
        {
            string shardKey = Sharding.FindShard(User);
            // NEED TO RECHECK CONTEXT MUST DETERMINE COMPANY -> MUST FIND CORRECT DataBase
            mpbdmContext<Guid> context = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            Account account = context.Accounts.Include("User").Where(a => a.User.Email == changeRequest.email).SingleOrDefault();
            if (account != null)
            {
                byte[] incoming = CustomLoginProviderUtils.hash(changeRequest.oldpass, account.Salt);

                if (CustomLoginProviderUtils.slowEquals(incoming, account.SaltedAndHashedPassword))
                {
                    if (changeRequest.password == changeRequest.repass)
                    {
                        byte[] newpass = CustomLoginProviderUtils.hash(changeRequest.password, account.Salt);
                        account.SaltedAndHashedPassword = newpass;
                        context.SaveChanges();
                        return this.Request.CreateResponse(HttpStatusCode.Created);
                    }
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Passes don't match");
                }
            }
            return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid email or password");
        }
    }
}