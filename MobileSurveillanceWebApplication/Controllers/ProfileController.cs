using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.IO;

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
            return RedirectToAction("ListTrajectory", "Trajectory", new { SearchKeyword = "", PageNumber = 1, PageCount = 0, UserId = account.Id, DateFrom = " ", DateTo = " " });
        }

        /// <summary>
        /// Modify information of Profile
        /// </summary>
        /// <param name="model"></param>
        /// <param name="avatar"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveProfile(UserViewModel model, HttpPostedFileBase avatar)
        {
            var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));
            if (ModelState.IsValid)
            {
                account.Address = model.Address;
                account.Fullname = model.Fullname;
                account.Birthday = model.Birthday;
                if (avatar.ContentLength > 0)
                {
                    var name = Path.GetFileName(avatar.FileName);
                    var path = Path.Combine(Server.MapPath("~/DefaultUserData/Avatar"), name);
                    account.Avatar = Path.Combine("/DefaultUserData/Avatar", name);
                    avatar.SaveAs(path);
                }
                else
                {
                    account.Avatar = "/DefaultUserData/Avatar/Avatar.png";
                }
            }
            int result = this.context.SaveChanges();      

            return RedirectToAction("UserProfile");
        }
    }
}
