using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CloudApp.Filters;
using CloudApp.Models;

namespace CloudApp.Controllers
{
    public class AccountController : Controller
    {
        CloudDBEntities context = new CloudDBEntities();
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login()
        {
            Account account = new Account();
            account.Username = "khiem";
            account.Password = "khiem";
            context.Accounts.Add(account);
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                //var acc = (from account in context.Accounts where (account.Username == model.UserName) select account).SingleOrDefault();
                //if (acc != null)
                //{
                //    return RedirectToAction("Index", "Home");
                //}
                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
    }
}
