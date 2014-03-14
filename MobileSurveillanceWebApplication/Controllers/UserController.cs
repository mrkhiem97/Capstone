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
        private readonly EntityContext context = new EntityContext();
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
            // get User
            var account = this.context.Accounts.SingleOrDefault(a => a.Username == User.Identity.Name);
            //get all friends of user

            var listUser = new List<FriendShip>();
            var listFriend = new List<FriendShip>();
            // Condition
            if (!String.IsNullOrEmpty(searchUserModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchUserModel.SearchKeyword.Trim()))
            {
                listUser = account.FriendShips1.Where(x => x.Account.Fullname.Contains(searchUserModel.SearchKeyword.Trim())
                    || x.Account.Username.Contains(searchUserModel.SearchKeyword.Trim()) && x.Status == "1").ToList();
            }
            else
            {
                listUser = account.FriendShips1.Where(x => x.Status == "1").ToList();
            }
            // For Pagination
            searchUserModel.PageCount = listUser.Count / pageSize;
            listUser = listUser.Skip((searchUserModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();

            //Add who is friend to ListFriend
            foreach (var user in listUser)
            {
                var friendAccount = user.Account.FriendShips1.Where(x => x.Status == "1" && x.Account.Id == account.Id).FirstOrDefault();
                listFriend.Add(friendAccount);
            }

            //List all friend
            foreach (var user in listFriend)
            {
                var userViewModel = new UserViewModel();
                userViewModel.Id = user.Account.Id;
                userViewModel.Username = user.Account.Username;
                userViewModel.Avatar = user.Account.Avatar;
                userViewModel.Fullname = user.Account.Fullname;
                userViewModel.Address = user.Account.Address;
                userViewModel.Birthday = user.Account.Birthday;

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
        public ActionResult ListFriendTrajectory(long friendId)
        {
            return RedirectToAction("ListTrajectory", "Trajectory", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0, UserId = friendId, DateFrom = " ", DateTo = " " });
        }

        public JsonResult GetCountItems()
        {
            string username = User.Identity.Name;
            var account = this.context.Accounts.SingleOrDefault(a => a.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            var listUser = account.FriendShips1.Where(x => x.Status == "1").ToList();
            var countItem = new
            {
                countInbox = account.FriendShips1.Where(x => x.Status == "0").Count(),
                countFriends = account.FriendShips1.Where(x => x.Status == "1").Count(),
            };
            return Json(countItem, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Make Friend Request
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        public ActionResult MakeFriendRequest(long friendId)
        {
            return RedirectToAction("ListTrajectory", "Trajectory", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0, UserId = friendId, DateFrom = " ", DateTo = " " });
        }
    }
}
