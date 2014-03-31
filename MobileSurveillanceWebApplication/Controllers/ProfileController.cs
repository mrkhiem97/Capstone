using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web.Helpers;

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
                account.Gender = model.Gender;
                string oldImgURL = account.Avatar;

                // for Upload Image          
                string folder = "/UserData/" + User.Identity.Name + "/Avatar";
                if (avatar != null && avatar.ContentLength > 0 && validateExtension(Path.GetExtension(avatar.FileName)))
                {
                    try
                    {
                        //create folder (/UserData/username/Avatar/abc.jpg)      

                        string pathString = Server.MapPath(folder);
                        Directory.CreateDirectory(pathString);

                        //upload Image to folder
                        var name = Path.GetFileName(avatar.FileName);
                        var path = Path.Combine(pathString, name);
                        account.Avatar = Path.Combine(folder, name);
                        avatar.SaveAs(path);

                        WebImage imgOriginal = new WebImage(avatar.InputStream);
                        imgOriginal.Resize(512, 512);
                        imgOriginal.Save(path);

                    }
                    catch (Exception)
                    {
                        account.Avatar = oldImgURL;
                        throw;
                    }
                }
                else
                {
                    account.Avatar = oldImgURL;
                }
            }

            int result = this.context.SaveChanges();
            Response.Cookies["FullName"].Value = account.Fullname;
            Response.Cookies["FullName"].Expires = DateTime.Now.AddDays(30);
            Response.Cookies["Role"].Value = account.Role.RoleName;
            Response.Cookies["Role"].Expires = DateTime.Now.AddDays(30);
            return RedirectToAction("UserProfile");
        }

        /// <summary>
        /// validate for Image Extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static bool validateExtension(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }


    }
}
