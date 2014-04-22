using Microsoft.AspNet.SignalR;
using MobileSurveillanceWebApplication.Hubs;
using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class SearchResultController : Controller
    {
        private const string IS_FRIEND = "1";
        private const string NOT_FRIEND = "0";
        private const string CONFIRM_NEED = "confirmNeed";
        private const string REQUEST_SENT = "requestSent";
        private const string FRIEND = "friend";
        private const string NOT_YET_FRIEND = "notFriend";
        
        //
        // GET: /SearchResult/
        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();
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
            int pageSize = 12;
            var listFriendViewModel = new ListFriendViewModel();
            var listUser = new List<Account>();

            //condition (list all user match searchkeyword except current user)
            if (!String.IsNullOrEmpty(searchCriteriaViewModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchCriteriaViewModel.SearchKeyword.Trim()))
            {
                listUser = this.context.Accounts.Where(x => x.Username.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower()) || x.Fullname.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower())).ToList();
                //listUser.Remove(account);
            }
            else
            {
                listUser = this.context.Accounts.ToList();
                //listUser.Remove(account);
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
                if (user.FriendShips1.Any(x => x.Account.Id == account.Id) && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND
                    && (!account.FriendShips1.Any(x => x.Account.Id == user.Id) || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == NOT_FRIEND))
                {
                    friendViewModel.FriendStatus = CONFIRM_NEED;
                }
                // if me (1) - you (0) --> requestSent
                else if ((!user.FriendShips1.Any(x => x.Account.Id == account.Id) || user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == NOT_FRIEND)
                    && account.FriendShips1.Any(x => x.Account.Id == user.Id) && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    friendViewModel.FriendStatus = REQUEST_SENT;
                }
                // if me (1) - you (1) --> friend
                else if (account.FriendShips1.Any(x => x.Account.Id == user.Id) && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == IS_FRIEND
                    && user.FriendShips1.Any(x => x.Account.Id == account.Id) && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    friendViewModel.FriendStatus = FRIEND;
                }
                // other cases is notFriend
                else
                {
                    friendViewModel.FriendStatus = NOT_YET_FRIEND;
                }

                listFriendViewModel.ListFriend.Add(friendViewModel);               
            }
            listFriendViewModel.ListFriend = listFriendViewModel.ListFriend.OrderBy(x => x.Fullname).ToList();
            ViewBag.SearchCriteriaViewModel = searchCriteriaViewModel;
            listFriendViewModel.SearchCriteriaViewModel = searchCriteriaViewModel;
            return View(listFriendViewModel);
        }

        /// <summary>
        /// Add Friend function/ ConfirmNeed function
        /// </summary>
        /// <returns></returns>
        public JsonResult AddFriend(long friendId, string requestType, SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();

            //check if there are friendId
            if (account.FriendShips1.Any(x => x.Account.Id == friendId))
            {
                //change friendship status to 1
                account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = IS_FRIEND;
                account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().FriendDate = DateTime.Now;

            }
            else
            {
                //create new friendship
                var frienship = new FriendShip();
                frienship.MyFriendId = friendId;
                frienship.MyId = account.Id;
                frienship.Status = IS_FRIEND;
                frienship.FriendDate = DateTime.Now;
                account.FriendShips1.Add(frienship);
            }
            int result = this.context.SaveChanges();
            if (result > 0)
            {
                string myName = User.Identity.Name;
                var friendAccount = this.context.Accounts.Where(x => x.Id == friendId).SingleOrDefault();
                foreach (var connectionId in NotificationHub.ConnectionMapper.GetConnections(friendAccount.Username))
                {
                    var signalRContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    if (requestType.Equals("addFriend", StringComparison.InvariantCultureIgnoreCase))
                    {
                        signalRContext.Clients.Client(connectionId).notifyAddFriendRequest(account.Fullname + " has sent you friend request!");
                    }
                    else if (requestType.Equals("confirmRequest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        signalRContext.Clients.Client(connectionId).notifyAcceptFriendRequest(account.Fullname + " has accepted you friend request!");
                    }
                }
                return ReturnJsonResult(searchCriteriaViewModel);
            }
            else
            {
                var message = "Nothing changes.";
                return Json(message, JsonRequestBehavior.AllowGet);
            }

        }



        /// <summary>
        /// Cancel Friend Request
        /// </summary>
        /// <returns></returns>
        public JsonResult CancelRequest(long friendId, SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            //change status to 0 
            account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = NOT_FRIEND;
            int result = this.context.SaveChanges();
            if (result > 0)
            {
                var friendAccount = this.context.Accounts.Where(x => x.Id == friendId).SingleOrDefault();
                foreach (var connectionId in NotificationHub.ConnectionMapper.GetConnections(friendAccount.Username))
                {
                    var signalRContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    signalRContext.Clients.Client(connectionId).notifyCancelFriendRequest();
                }
                return ReturnJsonResult(searchCriteriaViewModel);
            }
            else
            {
                var message = "Nothing changes.";
                return Json(message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Unfriend function
        /// </summary>
        /// <returns></returns>
        public JsonResult Unfriend(long friendId, TrajectorySearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            var friendAccount = this.context.Accounts.Where(x => x.Id == friendId).SingleOrDefault();
            //change status to 0 
            account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = NOT_FRIEND;
            friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status = NOT_FRIEND;
            int result = this.context.SaveChanges();
            if (result > 0)
            {
                var returnJson = new
                {
                    SearchKeyword = searchCriteriaViewModel.SearchKeyword,
                    PageNumber = searchCriteriaViewModel.PageNumber,
                    PageCount = searchCriteriaViewModel.PageCount,
                    DateFrom = searchCriteriaViewModel.DateFrom,
                    DateTo = searchCriteriaViewModel.DateTo,
                    UserId = searchCriteriaViewModel.UserId
                };
                foreach (var connectionId in NotificationHub.ConnectionMapper.GetConnections(friendAccount.Username))
                {
                    var signalRContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    signalRContext.Clients.Client(connectionId).notifyCancelFriendRequest();
                }
                return Json(returnJson, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var message = "Nothing changes.";
                return Json(message, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Deny Request friend
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="searchCriteriaViewModel"></param>
        /// <returns></returns>
        public JsonResult DenyRequest(long friendId, TrajectorySearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            var friendAccount = this.context.Accounts.Where(x => x.Id.Equals(friendId)).SingleOrDefault();
            //change status to 0 
            friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status = NOT_FRIEND;
            int result = this.context.SaveChanges();
            if (result > 0)
            {
                foreach (var connectionId in NotificationHub.ConnectionMapper.GetConnections(friendAccount.Username))
                {
                    var signalRContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    signalRContext.Clients.Client(connectionId).notifyDenyFriendRequest();
                }
                return ReturnJsonResult(searchCriteriaViewModel);
            }
            else
            {
                var message = "Nothing changes.";
                return Json(message, JsonRequestBehavior.AllowGet);
            }
        }

        private JsonResult ReturnJsonResult(SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var returnJson = new
            {
                SearchKeyword = searchCriteriaViewModel.SearchKeyword,
                PageNumber = searchCriteriaViewModel.PageNumber,
                PageCount = searchCriteriaViewModel.PageCount
            };
            return Json(returnJson, JsonRequestBehavior.AllowGet);
        }


    }
}
