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
using System.Net.Mail;
using System.Net;
using MobileSurveillanceWebApplication.Utility;
namespace MobileSurveillanceWebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // GET: /Trajectory/
        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("ListTrajectories", "Trajectory");
            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = (from x in this.context.Accounts
                               where
                                   x.Username.Equals(model.UserName, StringComparison.InvariantCultureIgnoreCase) &&
                                   x.Password.Equals(model.Password)
                               select x).SingleOrDefault();
                if (account != null)
                {
                    if (account.IsActive)
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                        Response.Cookies["FullName"].Value = HttpUtility.UrlEncode(account.Fullname); 
                        Response.Cookies["FullName"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["Role"].Value = account.Role.RoleName;
                        Response.Cookies["Role"].Expires = DateTime.Now.AddDays(30);
                        return RedirectToAction("ListTrajectories", "Trajectory");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Your account has not been activated! Please check your email to activate your account";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "The user name or password provided is incorrect!";
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect!");
            return View(model);
        }


        [AllowAnonymous]
        public JsonResult ValidateUsername(String username)
        {
            var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(username.Trim(), StringComparison.InvariantCultureIgnoreCase));
            if (account != null)
            {
                return Json(String.Format("{0} has been taken", username), JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous]
        public JsonResult ValidateUserEmail(String email)
        {
            var account = this.context.Accounts.SingleOrDefault(x => x.Email.Equals(email.Trim(), StringComparison.InvariantCultureIgnoreCase));
            if (account != null)
            {
                return Json(String.Format("{0} has been taken", email), JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
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

                    String url = Request.Url.AbsoluteUri;
                    int index = url.IndexOf("Account");
                    String host = url.Substring(0, index) + Url.Action("ValidateUserAccount", "Account", new { validateAccountToken = validateAccountToken, username = account.Username });
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
                return RedirectToAction("RegisterUserAccountSuccess", "Account", new { fullname = account.Fullname });
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterUserAccountSuccess(String fullname)
        {
            ViewBag.Fullname = fullname;
            return View();
        }

        [AllowAnonymous]
        public ActionResult ValidateUserAccount(String validateAccountToken, String username)
        {
            if (KeyUltil.GenerateHashKey(username).Equals(validateAccountToken))
            {
                var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                account.IsActive = true;
                this.context.SaveChanges();
                return RedirectToAction("Login");
            }
            else
            {
                return RedirectToAction("Register");
            }
        }

        [AllowAnonymous]
        public ActionResult RetrievePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult RetrievePassword(string email)
        {

            if (ModelState.IsValid)
            {
                var account = this.context.Accounts.SingleOrDefault(x => x.Email.Equals(email.Trim(),
                              StringComparison.InvariantCultureIgnoreCase));
                if (account != null)
                {
                    string key = KeyUltil.GenerateNewKey();
                    string body = "Here is your key from Mobile Surveillance: " + key;
                    String subject = "Mobile surveillance - Reset Password key";
                    bool success = EmailUltil.SendMail("mobilesurveillance.group4@gmail.com", account.Email, account.Fullname, subject, body, false);

                    if (success)
                    {
                        Session["KeyPassword"] = key;
                        Session["Username"] = account.Username;
                        Session.Timeout = 30;
                        if (Session != null)
                        {
                            return RedirectToAction("ResetPassword", "Account");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Issue sending email");
                    }
                }
                else // Email not found
                {
                    ModelState.AddModelError("", "No user found by that email.");
                }

            }
            return RedirectToAction("RetrievePassword", "Account");

        }
        // GET: /Account/ResetPassword

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                string username = (string)(Session["Username"]);
                var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(username.Trim(),
                              StringComparison.InvariantCultureIgnoreCase));
                if (account != null)
                {
                    if ((String)model.key == (String)Session["KeyPassword"])
                    {
                        account.Password = model.Password;
                        context.SaveChanges();

                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        ViewBag.Message = "Something went horribly wrong!";
                    }
                }

            }
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult ChangePassword(LocalPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(User.Identity.Name,
                              StringComparison.InvariantCultureIgnoreCase));
                    if (account.Password.Equals(model.OldPassword))
                    {
                        account.Password = model.NewPassword;
                        this.context.SaveChanges();
                        return View("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Your old password does not match your current password");
                        return View(model);
                    }
            }
            else
            {
                ModelState.AddModelError("", "Invalid input");
                return View(model);
            }
        }
    }
}
