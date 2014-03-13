﻿using MobileSurveillanceWebApplication.Models.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MobileSurveillanceWebApplication.Filters
{
    /// <summary>
    /// Generic Basic Authentication filter that checks for basic authentication
    /// headers and challenges for authentication if no authentication is provided
    /// Sets the Thread Principle with a GenericAuthenticationPrincipal.
    /// 
    /// You can override the OnAuthorize method for custom auth logic that
    /// might be application specific.    
    /// </summary>
    /// <remarks>Always remember that Basic Authentication passes username and passwords
    /// from client to server in plain text, so make sure SSL is used with basic auth
    /// to encode the Authorization header on all requests (not just the login).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BasicAuthenticationFilter : AuthorizationFilterAttribute
    {
        bool Active = true;
        private readonly EntityContext context = new EntityContext();

        public BasicAuthenticationFilter()
        { }

        /// <summary>
        /// Overriden constructor to allow explicit disabling of this
        /// filter's behavior. Pass false to disable (same as no filter
        /// but declarative)
        /// </summary>
        /// <param name="active"></param>
        public BasicAuthenticationFilter(bool active)
        {
            Active = active;
        }


        /// <summary>
        /// Override to Web API filter method to handle Basic Auth check
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (Active)
            {
                var basicAccount = ParseAuthorizationHeader(actionContext);
                if (basicAccount == null)
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent("UnAuthorize", System.Text.Encoding.UTF8)
                    };
                    return;
                }
                else if (!OnAuthorizeUser(basicAccount.Username, basicAccount.Password, actionContext))
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent("UnAuthorize", System.Text.Encoding.UTF8)
                    };
                    return;
                }
                else
                {
                    var account = context.Accounts.Where(x => x.Username.Equals(basicAccount.Username, StringComparison.OrdinalIgnoreCase) && x.Password.Equals(basicAccount.Password)).SingleOrDefault();
                    var identity = new BasicAuthenticationIdentity(account.Id, account.Username, account.Password);
                    var principal = new GenericPrincipal(identity, null);

                    Thread.CurrentPrincipal = principal;

                    // inside of ASP.NET this is required
                    //if (HttpContext.Current != null)
                    //    HttpContext.Current.User = principal;

                    base.OnAuthorization(actionContext);
                }
            }
        }

        /// <summary>
        /// Base implementation for user authentication - you probably will
        /// want to override this method for application specific logic.
        /// 
        /// The base implementation merely checks for username and password
        /// present and set the Thread principal.
        /// 
        /// Override this method if you want to customize Authentication
        /// and store user data as needed in a Thread Principle or other
        /// Request specific storage.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
        {
            var account = context.Accounts.Where(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && x.Password.Equals(password)).SingleOrDefault();
            if (account != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Parses the Authorization header and creates user credentials
        /// </summary>
        /// <param name="actionContext"></param>
        protected virtual BasicAccount ParseAuthorizationHeader(HttpActionContext actionContext)
        {
            string authHeader = null;
            var auth = actionContext.Request.Headers.Authorization;
            if (auth != null && auth.Scheme == "Basic")
                authHeader = auth.Parameter;

            if (string.IsNullOrEmpty(authHeader))
                return null;

            authHeader = Encoding.Default.GetString(Convert.FromBase64String(authHeader));

            var tokens = authHeader.Split(':');
            if (tokens.Length < 2)
                return null;

            return new BasicAccount(tokens[0], tokens[1]);
        }


        /// <summary>
        /// Send the Authentication Challenge request
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionContext"></param>
        void Challenge(HttpActionContext actionContext)
        {
        }
    }

    public class BasicAuthenticationIdentity : GenericIdentity
    {
        public BasicAuthenticationIdentity(long id, string name, string password)
            : base(name, "Basic")
        {
            this.Password = password;
            this.Id = id;
        }

        /// <summary>
        /// Basic Auth Password for custom authentication
        /// </summary>
        public string Password { get; set; }

        public long Id { get; set; }
    }

    public class BasicAccount
    {
        public string Username { get; set; }
        public String Password { get; set; }

        public BasicAccount(String username, String password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
}

