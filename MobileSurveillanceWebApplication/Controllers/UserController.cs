using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        private readonly MobileSurveillanceEntities context = new MobileSurveillanceEntities();
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ListFriend redirect to ListUser
        /// </summary>
        /// <returns></returns>
        public ActionResult ListFriend()
        {

            return RedirectToAction("ListUser", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0 });
        }

        /// <summary>
        /// List All Friends
        /// </summary>
        /// <returns></returns>
        public ActionResult ListUser(SearchCriteriaViewModel searchUserModel)
        {       
            int pageSize = 3;
            var listUserModel = new ListUserViewModel();
            // get user name
            string username = User.Identity.Name;
            // get User
            var account = this.context.Accounts.SingleOrDefault(a => a.Username == User.Identity.Name);
            //get all friends of user

            var listUser = new List<FriendShip>();
            // Condition
            if (!String.IsNullOrEmpty(searchUserModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchUserModel.SearchKeyword.Trim()))
            {
                listUser = account.FriendShips1.Where(x => x.Account.Fullname.Contains(searchUserModel.SearchKeyword.Trim()) || x.Account.Username.Contains(searchUserModel.SearchKeyword.Trim()) && x.Status == "1").ToList();
            }
            else
            {
                listUser = account.FriendShips1.Where(x => x.Status == "1").ToList();
            }
            // For Pagination
            searchUserModel.PageCount = listUser.Count / pageSize;
            listUser = listUser.Skip((searchUserModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();

            //List
            foreach (var user in listUser)
            {
                var userViewModel = new UserViewModel();
                userViewModel.Id = user.Account.Id;
                userViewModel.Username = user.Account.Username;
                userViewModel.Avatar = user.Account.Avatar;
                userViewModel.Fullname = user.Account.Fullname;
                //userViewModel.LastLogin = user.Account.LastLogin;

                listUserModel.ListUser.Add(userViewModel);
            }
       
            //add searchUserModel to ViewBag
            ViewBag.SearchCriteriaViewModel = searchUserModel;
            return View(listUserModel);
        }

        /// <summary>
        /// Track User Function
        /// </summary>
        /// <param name="friendID"></param>
        /// <returns></returns>
        public ActionResult ListFriendTrajectory( long friendID )
        {               
            return RedirectToAction("ListTrajectory", "Trajectory", new { FriendID = friendID });
        }


    }
}
