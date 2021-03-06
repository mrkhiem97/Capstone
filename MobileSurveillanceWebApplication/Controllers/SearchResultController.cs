﻿using Microsoft.AspNet.SignalR;
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
        [ValidateInput(false)]
        [System.Web.Mvc.Authorize]
        public ActionResult SearchResult(SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            int pageSize = 12;
            var listFriendViewModel = new ListFriendViewModel();
            var listUser = new List<Account>();

            //condition (list all user match searchkeyword except current user)
            if (!String.IsNullOrEmpty(searchCriteriaViewModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchCriteriaViewModel.SearchKeyword.Trim()))
            {
                listUser = this.context.Accounts.Where(x => x.Username.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower()) || x.Fullname.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower()) || x.Email.Contains(searchCriteriaViewModel.SearchKeyword.Trim().ToLower())).ToList();
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
                friendViewModel.MyId = account.Id;
                friendViewModel.ModelSearchId = String.Format("model-search-{0}-{1}", account.Id, user.Id);
                //CHECKING STATUS OF FRIENDSHIP                    
                // if me (0) - you (1) --> confirmNeed
                if (user.FriendShips1.Any(x => x.Account.Id == account.Id) && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == FriendStatus.IS_FRIEND
                    && (!account.FriendShips1.Any(x => x.Account.Id == user.Id) || account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == FriendStatus.NOT_FRIEND))
                {
                    friendViewModel.FriendStatus = FriendStatus.CONFIRM_NEED;
                }
                // if me (1) - you (0) --> requestSent
                else if ((!user.FriendShips1.Any(x => x.Account.Id == account.Id) || user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == FriendStatus.NOT_FRIEND)
                    && account.FriendShips1.Any(x => x.Account.Id == user.Id) && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == FriendStatus.IS_FRIEND)
                {
                    friendViewModel.FriendStatus = FriendStatus.REQUEST_SENT;
                }
                // if me (1) - you (1) --> friend
                else if (account.FriendShips1.Any(x => x.Account.Id == user.Id) && account.FriendShips1.Where(x => x.Account.Id == user.Id).SingleOrDefault().Status == FriendStatus.IS_FRIEND
                    && user.FriendShips1.Any(x => x.Account.Id == account.Id) && user.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == FriendStatus.IS_FRIEND)
                {
                    friendViewModel.FriendStatus = FriendStatus.FRIEND;
                }
                // other cases is notFriend
                else
                {
                    friendViewModel.FriendStatus = FriendStatus.NOT_YET_FRIEND;
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
            var friendShipAccount = account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault();
            if (friendShipAccount != null)
            {
                //change friendship status to 1
                friendShipAccount.Status = FriendStatus.IS_FRIEND;
                friendShipAccount.FriendDate = DateTime.Now;
            }
            else
            {
                //create new friendship
                var frienship = new FriendShip();
                frienship.MyFriendId = friendId;
                frienship.MyId = account.Id;
                frienship.Status = FriendStatus.IS_FRIEND;
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
                        String url1 = Url.Action("AddFriend", "SearchResult", new { friendId = account.Id, requestType = "confirmRequest", PageNumber = searchCriteriaViewModel.PageNumber, PageCount = searchCriteriaViewModel.PageCount });
                        String url2 = Url.Action("DenyRequest", "SearchResult", new { friendId = account.Id, requestType = "denyRequest", PageNumber = searchCriteriaViewModel.PageNumber, PageCount = searchCriteriaViewModel.PageCount });
                        signalRContext.Clients.Client(connectionId).changeAddFriendRequest(String.Format("model-search-{0}-{1}", friendAccount.Id, account.Id), url1, url2);
                    }
                    else if (requestType.Equals("confirmRequest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        signalRContext.Clients.Client(connectionId).notifyAcceptFriendRequest(account.Fullname + " has accepted you friend request!");
                        String url = Url.Action("ListFriendTrajectory", "User", new { friendId = account.Id });
                        signalRContext.Clients.Client(connectionId).changeAcceptFriendRequest(String.Format("model-search-{0}-{1}", friendAccount.Id, account.Id), url);
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

        [HttpGet]
        [ValidateInput(false)]
        public JsonResult GetUserList(string query)
        {
            query = HttpUtility.UrlDecode(query);
            var accounts = this.context.Accounts.Where(x => x.IsActive).Where(x => x.Fullname.ToLower().Contains(query.ToLower()) || x.Username.ToLower().Contains(query.ToLower()) || x.Email.ToLower().Contains(query.ToLower())).ToList();
            var listResult = new List<string>();
            foreach (var item in accounts)
            {
                listResult.Add(item.Fullname);
                listResult.Add(item.Username);
                listResult.Add(item.Email);
            }
            return Json(listResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Cancel Friend Request
        /// </summary>
        /// <returns></returns>
        public JsonResult CancelRequest(long friendId, SearchCriteriaViewModel searchCriteriaViewModel)
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            //change status to 0 
            account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = FriendStatus.NOT_FRIEND;
            int result = this.context.SaveChanges();
            if (result > 0)
            {
                var friendAccount = this.context.Accounts.Where(x => x.Id == friendId).SingleOrDefault();
                foreach (var connectionId in NotificationHub.ConnectionMapper.GetConnections(friendAccount.Username))
                {
                    var signalRContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    signalRContext.Clients.Client(connectionId).notifyCancelFriendRequest();
                    String url = Url.Action("AddFriend", "SearchResult", new { friendId = account.Id, requestType = "addFriend", PageNumber = searchCriteriaViewModel.PageNumber, PageCount = searchCriteriaViewModel.PageCount });
                    signalRContext.Clients.Client(connectionId).changeDenyFriendRequest(String.Format("model-search-{0}-{1}", friendAccount.Id, account.Id), url);
                    signalRContext.Clients.Client(connectionId).changeInboxRequest(String.Format("model-listInbox-{0}-{1}", friendAccount.Id, account.Id), String.Format("model-listUser-{0}-{1}", friendAccount.Id, account.Id));
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
            account.FriendShips1.Where(x => x.Account.Id == friendId).SingleOrDefault().Status = FriendStatus.NOT_FRIEND;
            friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status = FriendStatus.NOT_FRIEND;
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
                    String url = Url.Action("AddFriend", "SearchResult", new { friendId = account.Id, requestType = "addFriend", PageNumber = searchCriteriaViewModel.PageNumber, PageCount = searchCriteriaViewModel.PageCount });
                    signalRContext.Clients.Client(connectionId).changeDenyFriendRequest(String.Format("model-search-{0}-{1}", friendAccount.Id, account.Id), url);
                    signalRContext.Clients.Client(connectionId).changeInboxRequest(String.Format("model-listInbox-{0}-{1}", friendAccount.Id, account.Id), String.Format("model-listUser-{0}-{1}", friendAccount.Id, account.Id));
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
            friendAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status = FriendStatus.NOT_FRIEND;
            int result = this.context.SaveChanges();
            if (result > 0)
            {
                foreach (var connectionId in NotificationHub.ConnectionMapper.GetConnections(friendAccount.Username))
                {
                    var signalRContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    signalRContext.Clients.Client(connectionId).notifyDenyFriendRequest();
                    String url = Url.Action("AddFriend", "SearchResult", new { friendId = account.Id, requestType = "addFriend", PageNumber = searchCriteriaViewModel.PageNumber, PageCount = searchCriteriaViewModel.PageCount });
                    signalRContext.Clients.Client(connectionId).changeDenyFriendRequest(String.Format("model-search-{0}-{1}", friendAccount.Id, account.Id), url);
                    signalRContext.Clients.Client(connectionId).changeInboxRequest(String.Format("model-listInbox-{0}-{1}", friendAccount.Id, account.Id), String.Format("model-listUser-{0}-{1}", friendAccount.Id, account.Id));
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
