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
        private readonly MobileSurveillanceEntities context = new MobileSurveillanceEntities();
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
                //return RedirectToAction("ListFriend", "User");
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
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    Response.Cookies["FullName"].Value = account.Fullname;
                    Response.Cookies["FullName"].Expires = DateTime.Now.AddDays(30);
                    Response.Cookies["Role"].Value = account.Role.RoleName;
                    Response.Cookies["Role"].Expires = DateTime.Now.AddDays(30);
                    return RedirectToAction("ListTrajectories", "Trajectory");
                }
                else
                {
                    ViewBag.ErrorMessage = "Username or Password provided is incorrect!";
                    return View(model);
                }

            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
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

                try
                {
                    var account = new Account
                    {
                        Username = model.UserName,
                        Password = model.Password,
                        Email = model.Email,
                        Fullname = model.Fullname,
                        LastLogin = DateTime.Now,
                        Avatar = "/DefaultUserData/Avatar/Avatar.png",
                        IsActive = true,
                        RoleId = 2
                    };
                    context.Accounts.Add(account);
                    context.SaveChanges();
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    throw;
                }
            }
            return View(model);
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
                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                    client.EnableSsl = true;
                    MailAddress from = new MailAddress("mobilesurveillance.group4@gmail.com", "mobilesurveillance.group4@gmail.com");
                    MailAddress to = new MailAddress(account.Email, account.Username);
                    MailMessage message = new MailMessage(from, to);
                    string key = KeyUltil.GenerateNewKey();
                    message.Body = "Here is your key from Mobile surveillance: " + key;
                    message.Subject = "Mobile surveillance - Reset Password key";
                    NetworkCredential myCreds = new NetworkCredential("mobilesurveillance.group4@gmail.com", "motnamchin", "");
                    client.Credentials = myCreds;
                    try
                    {
                        client.Send(message);                        
                        Session["KeyPassword"] = key;
                        Session["Username"] = account.Username;
                        Session.Timeout = 30;
                        if(Session!=null)
                        {
                            return RedirectToAction("ResetPassword", "Account");
                        }                   
                        
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "Issue sending email: " + e.Message);
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
                if(account!=null)
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
    }
}
