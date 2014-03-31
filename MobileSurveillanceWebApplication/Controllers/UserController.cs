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
        private long fId;
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
            int pageSize = 8;
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
                if (account.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).SingleOrDefault().Status == IS_FRIEND
                    && friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    listMyFriend.Add(friendship);
                }
            }
            // For Pagination
            searchUserModel.PageCount = (listMyFriend.Count - 1) / pageSize + 1;
            listMyFriend = listMyFriend.Skip((searchUserModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();

            //List all friends
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
            listUserModel.ListNotFriend = LoadSuggestFriend();
            //listUserModel.ListMutualFriend = ListMutualFriend()
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

        /// <summary>
        /// Get count for Inbox and for FriendList
        /// </summary>
        /// <returns></returns>
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
        public ActionResult ListInbox(SearchCriteriaViewModel searchUserModel)
        {
            var listUserModel = new ListUserViewModel();
            // get User
            var account = this.context.Accounts.SingleOrDefault(a => a.Username == User.Identity.Name);                   
            var listInbox = new List<FriendShip>();
            var listFriendShip = account.FriendShips.Where(x => x.Status == IS_FRIEND).ToList();

            //Add who is waiting confirm to ListInbox
            foreach (var friendship in listFriendShip)
            {
                if (!account.FriendShips1.Any(x => x.Account.Id == friendship.Account1.Id) || account.FriendShips1.Where(x => x.Account.Id == friendship.Account1.Id).SingleOrDefault().Status == NOT_FRIEND
                  && friendship.Account1.FriendShips1.Any(x => x.Account.Id == account.Id) && friendship.Account1.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    listInbox.Add(friendship);
                }
            }
            listInbox = listInbox.OrderByDescending(x => x.FriendDate).ToList();

            //List all inbox
            foreach (var user in listInbox)
            {
                var userViewModel = new UserViewModel();
                userViewModel.Id = user.Account1.Id;
                userViewModel.Username = user.Account1.Username;
                userViewModel.Avatar = user.Account1.Avatar;
                userViewModel.Fullname = user.Account1.Fullname;

                listUserModel.ListUser.Add(userViewModel);
            }
            ViewBag.SearchCriteriaViewModel = searchUserModel;
            return View(listUserModel);
        }

        /// <summary>
        /// Load Suggest Friend according to number of trajectories or has mutual friend
        /// </summary>
        /// <returns></returns>
        public List<FriendViewModel> LoadSuggestFriend()
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            var listNotFriend = new List<FriendViewModel>();
            var listMutualFriendViewModel = new List<FriendViewModel>();
            var listUser = this.context.Accounts.ToList();
            listUser.Remove(account);

            foreach (var user in listUser)
            {
                var friendViewModel = new FriendViewModel();
                friendViewModel.Id = user.Id;
                friendViewModel.Username = user.Username;
                friendViewModel.Avatar = user.Avatar;
                friendViewModel.Fullname = user.Fullname;
                friendViewModel.CountTrajectory = user.Trajectories.Count(x => x.IsActive);

                if (user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND
                    && (!account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == NOT_FRIEND))
                {

                }
                // if me (1) - you (0) --> requestSent
                else if ((!user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() || user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == NOT_FRIEND)
                          && account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND)
                {

                }
                // if me (1) - you (1) --> friend
                else if (account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND
                         && user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {

                }
                // other cases is notFriend
                else
                {
                    listNotFriend.Add(friendViewModel);

                    //Create PeopleFriendList
                    var listPeopleFriend = new List<Account>();
                    var listPeopleFriendship = user.FriendShips1.Where(x => x.Status == IS_FRIEND).ToList();
                    foreach (var friendship in listPeopleFriendship)
                    {
                        if (user.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).SingleOrDefault().Status == IS_FRIEND
                            && friendship.Account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() && friendship.Account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND)
                        {
                            listPeopleFriend.Add(friendship.Account);
                        }
                    }

                    //Create MyFriendList
                    var listMyFriend = new List<Account>();
                    var listMyFriendShip = account.FriendShips1.Where(x => x.Status == IS_FRIEND).ToList();
                    foreach (var friendship in listMyFriendShip)
                    {
                        if (account.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).SingleOrDefault().Status == IS_FRIEND
                            && friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                        {
                            listMyFriend.Add(friendship.Account);
                        }
                    }



                    //Comparison for MutualFriend
                    foreach (var peopleFriend in listPeopleFriend)
                    {
                        if (listMyFriend.Contains(peopleFriend))
                        {
                            friendViewModel.CountMutualFriend++;
                        }
                    }
                }
            }
            return listNotFriend.Take(5).OrderByDescending(x => x.CountMutualFriend).ToList();
        }

        /// <summary>
        /// load List of Mutual Friends
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        public ActionResult ListMutualFriend(long friendId)
        {
            fId = friendId;
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            var friendAccount = this.context.Accounts.Where(x => x.Id.Equals(friendId)).SingleOrDefault();

            var listMutualFriendViewModel = new ListUserViewModel();

            //Create PeopleFriendList
            var listPeopleFriend = new List<Account>();
            var listPeopleFriendship = friendAccount.FriendShips1.Where(x => x.Status == IS_FRIEND).ToList();
            foreach (var friendship in listPeopleFriendship)
            {
                if (friendAccount.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).Any() && friendAccount.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).SingleOrDefault().Status == IS_FRIEND
                    && friendship.Account.FriendShips1.Where(x => x.Account.Id == friendId).Any() && friendship.Account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status == IS_FRIEND)
                {
                    listPeopleFriend.Add(friendship.Account);
                }
            }


            //Create MyFriendList
            var listMyFriend = new List<Account>();
            var listMyFriendShip = account.FriendShips1.Where(x => x.Status == IS_FRIEND).ToList();
            foreach (var friendship in listMyFriendShip)
            {
                if (account.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == friendship.Account.Id).SingleOrDefault().Status == IS_FRIEND
                    && friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && friendship.Account.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    listMyFriend.Add(friendship.Account);
                }
            }

            var listMutualFriend = new List<Account>();

            //Comparison for MutualFriend
            foreach (var peopleFriend in listPeopleFriend)
            {
                if (listMyFriend.Contains(peopleFriend))
                {
                    listMutualFriend.Add(peopleFriend);
                }
            }

            //Create List of Mutual Friend
            foreach (var mutualFriend in listMutualFriend)
            {
                var mutualFriendViewModel = new FriendViewModel();
                mutualFriendViewModel.Fullname = mutualFriend.Fullname;
                mutualFriendViewModel.Avatar = mutualFriend.Avatar;
                mutualFriendViewModel.Id = mutualFriend.Id;
                mutualFriendViewModel.Username = mutualFriend.Username;
                listMutualFriendViewModel.ListMutualFriend.Add(mutualFriendViewModel);
            }
            return View(listMutualFriendViewModel);
        }

    }
}
