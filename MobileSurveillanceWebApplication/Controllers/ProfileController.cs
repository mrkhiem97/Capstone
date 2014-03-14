using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class ProfileController : Controller
    {
        private readonly EntityContext context = new EntityContext();
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
            return RedirectToAction("ListTrajectory", "Trajectory", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0, UserId = account.Id, DateFrom = " ", DateTo = " " });
        }

        /// <summary>
        /// Modify information of Profile
        /// </summary>
        /// <param name="address"></param>
        /// <param name="fullname"></param>
        /// <param name="birthday"></param>
        /// <returns></returns>
        public JsonResult SaveProfile(string address, string fullname)
        {
            var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));
            account.Address = address;
            account.Fullname = fullname;
         

            int result = this.context.SaveChanges();
            var message = "";
            if (result > 0)
            {
                message = "Change saved.";
            }
            else
            {
                message = "Nothing changes.";
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}
