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
using MobileSurveillanceWebApplication.Filters;
using MobileSurveillanceWebApplication.Models;
using MobileSurveillanceWebApplication.Models.ViewModel;
using MobileSurveillanceWebApplication.Models.DatabaseModel;

namespace MobileSurveillanceWebApplication.Controllers
{
    [Authorize]
    public class MobileAccountController : Controller
    {
        // GET: /Trajectory/
        private readonly MobileSurveillanceEntities context = new MobileSurveillanceEntities();

        [AllowAnonymous]
        public ActionResult RegisterSuccess(String username)
        {
            ViewBag.Username = username;
            return View();
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var account = new Account
                    {
                        Username = model.UserName,
                        Password = model.Password,
                        Email = model.Email,
                        Fullname = model.Fullname,
                        LastLogin = DateTime.Now,
                        Avatar = "App_Data/DefaultAvatar/Avatar.jpg",
                        IsActive = false,
                        RoleId = 2

                    };
                    context.Accounts.Add(account);
                    context.SaveChanges();

                    return RedirectToAction("RegisterSuccess", "MobileAccount", new { username = account.Username });
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    throw;
                }
            }
            return View(model);
        }
    }
}
