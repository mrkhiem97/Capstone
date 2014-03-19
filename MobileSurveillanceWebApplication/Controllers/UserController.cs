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
        private const string IS_FRIEND = "1";
        private const string NOT_FRIEND = "0";
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

            return RedirectToAction("ListUser", new { SearchKeyword = "", PageNumber = 1, PageCount = 0 });
        }

        /// <summary>
        /// List All Friends
        /// </summary>
        /// <returns></returns>
        public ActionResult ListUser(SearchCriteriaViewModel searchUserModel)
        {
            int pageSize = 6;
            var listUserModel = new ListUserViewModel();
            // get User
            var account = this.context.Accounts.SingleOrDefault(a => a.Username.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));

            var listFriendShip = new List<FriendShip>();
            var listMyFriend = new List<FriendShip>();
            // Condition
            if (!String.IsNullOrEmpty(searchUserModel.SearchKeyword) && !String.IsNullOrWhiteSpace(searchUserModel.SearchKeyword))
            {
                searchUserModel.SearchKeyword = searchUserModel.SearchKeyword.Trim().ToLower();
                listFriendShip = account.FriendShips1.Where(x => x.Account.Fullname.ToLower().Contains(searchUserModel.SearchKeyword)
                   || x.Account.Username.ToLower().Contains(searchUserModel.SearchKeyword) && x.Status == IS_FRIEND).ToList();
            }
            else
            {
                listFriendShip = account.FriendShips1.Where(x => x.Status == IS_FRIEND).ToList();
            }
        

            //Add who is friend to ListFriend
            foreach (var friendship in listFriendShip)
            {
                var myFriend = friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id && x.Status.Equals(IS_FRIEND, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                if (myFriend != null)
                {
                    listMyFriend.Add(friendship);
                }
            }
            // For Pagination
            searchUserModel.PageCount = (listMyFriend.Count - 1) / pageSize + 1;
            listMyFriend = listMyFriend.Skip((searchUserModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();

            //List all friend
            foreach (var user in listMyFriend)
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
            return RedirectToAction("ListTrajectory", "Trajectory", new { SearchKeyword = "", PageNumber = 1, PageCount = 0, UserId = friendId, DateFrom = " ", DateTo = " " });
        }

        public JsonResult GetCountItems()
        {
            string username = User.Identity.Name;
            var account = this.context.Accounts.SingleOrDefault(a => a.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            var listUser = this.context.Accounts.ToList();
            listUser.Remove(account);
            var listFriend = new List<Account>();
            var listInbox = new List<Account>();
           
            foreach (var user in listUser)
            {
                //Add who is friend to ListFriend
                if (account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND
                    && user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    listFriend.Add(user);
                }
                //Add who is waiting for confirmation to ListInbox
                else if (user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND
                    && (!account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == NOT_FRIEND))
                {
                    listInbox.Add(user);
                }
            }  

            var countItem = new
            {
                countInbox = listInbox.Count(),
                countFriends = listFriend.Count(),
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

        /// <summary>
        /// List all Inbox for confirm Request
        /// </summary>
        public ActionResult ListInbox()
        {
            var listUserModel = new ListUserViewModel();
            // get User
            var account = this.context.Accounts.SingleOrDefault(a => a.Username == User.Identity.Name);

            var listUser = this.context.Accounts.ToList();
            listUser.Remove(account);
            var listInbox = new List<Account>();
            //Add who is friend to ListFriend
            foreach (var user in listUser)
            {
                if (user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND
                   && (!account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == NOT_FRIEND))
                {
                    listInbox.Add(user);
                }
            }

            //List all inbox
            foreach (var user in listInbox)
            {
                var userViewModel = new UserViewModel();
                userViewModel.Id = user.Id;
                userViewModel.Username = user.Username;
                userViewModel.Avatar = user.Avatar;
                userViewModel.Fullname = user.Fullname;
                userViewModel.Address = user.Address;
                userViewModel.Birthday = user.Birthday;

                listUserModel.ListUser.Add(userViewModel);
            }
            return View(listUserModel);
        }
    }
}
