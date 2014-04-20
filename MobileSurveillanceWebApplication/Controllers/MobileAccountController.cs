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
using MobileSurveillanceWebApplication.Utility;

namespace MobileSurveillanceWebApplication.Controllers
{
    [Authorize]
    public class MobileAccountController : Controller
    {
        // GET: /Trajectory/
        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();

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
                        Birthday = model.Birthday,
                        Address = model.Address,
                        LastLogin = DateTime.Now,
                        Gender = model.Gender,
                        Avatar = "/DefaultUserData/Avatar/Avatar.png",
                        IsActive = false,
                        RoleId = 2
                    };
                    context.Accounts.Add(account);
                    try
                    {
                        context.SaveChanges();
                        string validateAccountToken = KeyUltil.GenerateHashKey(account.Username);
                        String url = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host;
                        String host = url + Url.Action("ValidateUserAccount", "Account", new { validateAccountToken = validateAccountToken, username = account.Username });
                        String link = String.Format("<a href=\"{0}\">{0}</a>", host);
                        string body = "Mobile Surveillance\r\n" + "Please click the link below to Activate your Account\r\n" + link;
                        String subject = "Mobile surveillance - Activate Account";
                        bool success = EmailUltil.SendMail("mobilesurveillance.group4@gmail.com", account.Email, account.Fullname, subject, body, true);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        throw;
                    }
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
