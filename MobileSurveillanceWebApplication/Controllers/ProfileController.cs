using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MobileSurveillanceEntities context = new MobileSurveillanceEntities();
        //
        // GET: /Profile/

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Show User Profile
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UserProfile()
        {
            string username;
            var user = new UserViewModel();
            username = User.Identity.Name;
            var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            //if (account != null)
            //{
            //    user.Avatar = account.Avatar;
            //    user.Email = account.Email;
            //    user.Fullname = account.Fullname;
            //    user.Username = account.Username;
            //    user.Id = account.Id;
            //}
            //return View(user);
            return RedirectToAction("ListTrajectory", "Trajectory", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0, UserId = account.Id, DateFrom = " ", DateTo = " " });
        }
    }
}
