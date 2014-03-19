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
        private const string IS_FRIEND = "1";
        private const string NOT_FRIEND = "0";
        //
        // GET: /SearchResult/
        private readonly EntityContext context = new EntityContext();
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Show all User match searchKeyword
        /// </summary>
        /// <param name="searchCriteriaViewModel"></param>
        /// <returns></returns>
        public ActionResult SearchResult(SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            int pageSize = 6;
            var listFriendViewModel = new ListFriendViewModel();
            var listUser = new List<Account>();

            //condition (list all user match searchkeyword except current user)
            if (!String.IsNullOrEmpty(searchCriteriaViewModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchCriteriaViewModel.SearchKeyword.Trim()))
            {
                listUser = this.context.Accounts.Where(x => x.Username.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower()) || x.Fullname.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower())).ToList();
                listUser.Remove(account);
            }
            else
            {
                listUser = this.context.Accounts.ToList();
                listUser.Remove(account);
            }
            ViewBag.SearchUserCount = listUser.Count();

            // For Pagination
            searchCriteriaViewModel.PageCount = (listUser.Count - 1) / pageSize + 1;
            listUser = listUser.Skip((searchCriteriaViewModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();

            //List
            foreach (var user in listUser)
            {
                var friendViewModel = new FriendViewModel();
                friendViewModel.Id = user.Id;
                friendViewModel.Username = user.Username;
                friendViewModel.Avatar = user.Avatar;
                friendViewModel.Fullname = user.Fullname;
                friendViewModel.Birthday = user.Birthday;
                friendViewModel.Address = user.Address;

                //CHECKING STATUS OF FRIENDSHIP                    
                // if me (0) - you (1) --> confirmNeed
                if (user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND
                    && (!account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == NOT_FRIEND))
                {
                    friendViewModel.FriendStatus = "confirmNeed";
                }
                // if me (1) - you (0) --> requestSent
                else if ((!user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() || user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == NOT_FRIEND)
                    && account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    friendViewModel.FriendStatus = "requestSent";
                }
                // if me (1) - you (1) --> friend
                else if (account.FriendShips1.Where(x => x.Account.Id == user.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND
                    && user.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    friendViewModel.FriendStatus = "friend";
                }
                // other cases is notFriend
                else
                {
                    friendViewModel.FriendStatus = "notFriend";
                }

                listFriendViewModel.ListFriend.Add(friendViewModel);
            }
            ViewBag.SearchCriteriaViewModel = searchCriteriaViewModel;
            listFriendViewModel.SearchCriteriaViewModel = searchCriteriaViewModel;
            return View(listFriendViewModel);
        }

        /// <summary>
        /// Add Friend function/ ConfirmNeed function
        /// </summary>
        /// <returns></returns>
        public JsonResult AddFriend(long friendId)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            //check if there are friendId
            if (account.FriendShips1.Where(x => x.Account.Id == friendId).Any())
            {
                //change friendship status to 1
                account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = "1";
            }
            else
            {
                //create new friendship
                var frienship = new FriendShip();
                frienship.MyFriendId = friendId;
                frienship.MyId = account.Id;
                frienship.Status = "1";
                account.FriendShips1.Add(frienship);
            }
            int result = this.context.SaveChanges();
            var message = "";
            if (result > 0)
            {
                message = "Add Friend Succcessful";
            }
            else
            {
                message = "Nothing changes.";
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Cancel Friend Request
        /// </summary>
        /// <returns></returns>
        public JsonResult CancelRequest(long friendId)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            //change status to 0 
            account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = "0";
            int result = this.context.SaveChanges();
            var message = "";
            return Json(message, JsonRequestBehavior.AllowGet);
        }

    }
}
