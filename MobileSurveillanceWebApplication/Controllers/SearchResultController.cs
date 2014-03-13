using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class SearchResultController : Controller
    {
        //
        // GET: /SearchResult/
        private readonly EntityContext context = new EntityContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchResult(SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            int pageSize = 3;
            var listFriendViewModel = new ListFriendViewModel();
            var listUser = new List<Account>();

            //condition (list all user match searchkeyword except current user)
            if (!String.IsNullOrEmpty(searchCriteriaViewModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchCriteriaViewModel.SearchKeyword.Trim()))
            {
                listUser = this.context.Accounts.Where(x => x.Username.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower()) || x.Fullname.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower())).ToList();
                listUser.Remove(account);
            }
            ViewBag.SearchUserCount = listUser.Count();

            // For Pagination
            searchCriteriaViewModel.PageCount = listUser.Count / pageSize;
            listUser = listUser.Skip((searchCriteriaViewModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();

            //List
            foreach (var user in listUser)
            {
                var friendAccount = this.context.Accounts.Where(x => x.Id == user.Id).SingleOrDefault();
                var friendViewModel = new FriendViewModel();
                friendViewModel.Id = user.Id;
                friendViewModel.Username = user.Username;
                friendViewModel.Avatar = user.Avatar;
                friendViewModel.Fullname = user.Fullname;
                friendViewModel.Birthday = user.Birthday;
                friendViewModel.Address = user.Address;

                //CHECKING STATUS OF FRIENDSHIP     
                // if me (0) - you (0) or there is no account of friend or me --> not friend
                if ((!account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() || !friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).Any())
                  || (account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == "0" && friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == "0"))
                {
                    friendViewModel.FriendStatus = "notFriend";
                }
                // if me (1) - you (0) --> request sent
                else if (friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == "1"
                    && (!account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == "0"))
                {
                    friendViewModel.FriendStatus = "confirmNeed";
                }
                // if me (0) - you (1) --> confirm need
                else if ((!friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).Any() || friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == "0")
                    && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == "1")
                {
                    friendViewModel.FriendStatus = "requestSent";
                }
                // if me (1) - you (1) --> friend
                else
                {
                    friendViewModel.FriendStatus = "friend";
                }



                listFriendViewModel.ListFriend.Add(friendViewModel);

            }
            ViewBag.SearchCriteriaViewModel = searchCriteriaViewModel;
            listFriendViewModel.SearchCriteriaViewModel = searchCriteriaViewModel;
            return View(listFriendViewModel);
        }

        /// <summary>
        /// Add Friend function
        /// </summary>
        /// <returns></returns>
        public ActionResult AddFriend()
        {
            return View();
        }

        /// <summary>
        /// Accept friend request
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmRequest()
        {
            return View();
        }

        /// <summary>
        /// Cancel Friend Request
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelRequest()
        {
            return View();
        }

    }
}
