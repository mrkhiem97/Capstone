﻿using MobileSurveillanceWebApplication.Models.ApiModel;
using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using MobileSurveillanceWebApplication.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace MobileSurveillanceWebApplication.Controllers
{
    public class TrajectoryController : Controller
    {
        private const string IS_FRIEND = "1";
        private const string NOT_FRIEND = "0";
        private const String USER_DATA_FOLDER = "/UserData/";
        //
        // GET: /Trajectory/
        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult ViewDetail(string trajectoryId)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectoryId)).SingleOrDefault();
            var model = new TrajectoryViewModel();
            model.Id = trajectory.Id;
            model.TrajectoryName = trajectory.TrajectoryName;
            model.Status = trajectory.Status;
            model.CreateDate = trajectory.CreatedDate.ToString();
            model.Description = trajectory.Description;
            model.LastUpdate = trajectory.LastUpdated.ToString();
            return View(model);
        }


        //[Authorize]
        //public ActionResult Delete(string trajectoryId)
        //{
        //    var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectoryId)).SingleOrDefault();
        //    this.context.Trajectories.Remove(trajectory);
        //    this.context.SaveChanges();
        //    return RedirectToAction("ListTrajectory");
        //}

        [Authorize]
        public JsonResult Delete(string trajectoryId)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectoryId)).SingleOrDefault();
            trajectory.IsActive = false;
            int Result = this.context.SaveChanges();
            string Message;
            if (Result > 0)
            {
                Message = "Deleted successfully";
            }
            else
            {
                Message = "Error, please try again!";
            }
            return Json(new { message = Message, id = trajectoryId }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ListTrajectories()
        {
            var account = this.context.Accounts.Where(x => x.Username.Equals(User.Identity.Name)).SingleOrDefault();
            return RedirectToAction("ListTrajectory", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0, UserId = account.Id, DateFrom = " ", DateTo = " " });
        }


        [Authorize]
        public ActionResult ListTrajectory(TrajectSearchCriteriaViewModel searchUserModel)
        {
            //page size
            int pageSize = 7;

            if (!String.IsNullOrEmpty(searchUserModel.SearchKeyword))
            {
                searchUserModel.SearchKeyword = searchUserModel.SearchKeyword.Trim().ToLower();
            }
            // List trajectory
            var model = new ListTrajectoryViewModel();

            // Get account
            var account = this.context.Accounts.SingleOrDefault(x => x.Id == searchUserModel.UserId);
            // Get trajectory of the username
            var listTraject = new List<Trajectory>();
            var user = this.context.Accounts.Where(x => x.Username == User.Identity.Name).SingleOrDefault();
            // Condition
            if (!String.IsNullOrEmpty(searchUserModel.SearchKeyword) && !String.IsNullOrWhiteSpace(searchUserModel.SearchKeyword))
            {
                string[] trajectName = searchUserModel.SearchKeyword.Split(new string[] { "" }, StringSplitOptions.RemoveEmptyEntries);
                //listTraject = (from c in this.context.Trajectories where c.TrajectoryName.ToLower().Contains()

                listTraject = account.Trajectories.Where(x => x.TrajectoryName.ToLower().Contains(searchUserModel.SearchKeyword)
                    && x.IsActive
                    && x.Status.Equals("Public", StringComparison.InvariantCultureIgnoreCase)


                    || x.TrajectoryName.ToLower().Contains(searchUserModel.SearchKeyword)
                    && x.IsActive
                    && x.UserId == user.Id) //Private
                    .OrderByDescending(x => x.CreatedDate).ToList();
            }
            else
            {
                listTraject = account.Trajectories.Where(x => x.IsActive
                    && x.Status.Equals("Public", StringComparison.InvariantCultureIgnoreCase)

                    || x.IsActive
                    && x.UserId == user.Id)

                    .OrderByDescending(x => x.CreatedDate).ToList();
            }
            searchUserModel.PageCount = (listTraject.Count - 1) / pageSize + 1;
            listTraject = listTraject.Skip((searchUserModel.PageNumber - 1) * pageSize).Take(pageSize).ToList();


            foreach (var trajectory in listTraject)
            {
                var trajectoryViewModel = new TrajectoryViewModel();
                trajectoryViewModel.Id = trajectory.Id;
                trajectoryViewModel.TrajectoryName = trajectory.TrajectoryName;
                trajectoryViewModel.Description = trajectory.Description;
                trajectoryViewModel.Status = trajectory.Status;
                trajectoryViewModel.CreateDate = trajectory.CreatedDate.ToString();
                trajectoryViewModel.LastUpdate = trajectory.LastUpdated.ToString();
                var startLocation = trajectory.Locations.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                var endLocation = trajectory.Locations.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).LastOrDefault();
                if (startLocation != null)
                {
                    trajectoryViewModel.StartLongitude = startLocation.Longitude.ToString();
                    trajectoryViewModel.StartLatitude = startLocation.Latitude.ToString();
                    trajectoryViewModel.StartAddress = startLocation.Address;
                    trajectoryViewModel.EndAddress = endLocation.Address;
                    trajectoryViewModel.StartTime = startLocation.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss tt");
                    trajectoryViewModel.EndTime = endLocation.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss tt");
                }
                model.ListTrajectory.Add(trajectoryViewModel);
            }

            ViewBag.SearchCriteriaViewModel = searchUserModel;

            // Get User
            var userModel = GetUserViewModel(account);
            model.FriendViewModel = userModel;
            ViewBag.FriendStatus = userModel.FriendStatus;
            return View(model);
        }

        private FriendViewModel GetUserViewModel(Account account)
        {
            var myAccount = this.context.Accounts.SingleOrDefault(a => a.Username.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));
            var userModel = new FriendViewModel();
            if (String.IsNullOrEmpty(account.Avatar))
            {
                userModel.Avatar = "/DefaultUserData/Avatar/Avatar.png";
            }
            else
            {
                userModel.Avatar = account.Avatar;
            }
            userModel.Email = account.Email;
            userModel.Fullname = account.Fullname;
            userModel.IsActive = account.IsActive.ToString();
            userModel.LastLogin = account.LastLogin;
            userModel.Username = account.Username;
            userModel.Id = account.Id;
            userModel.Address = account.Address;
            userModel.Birthday = account.Birthday;
            userModel.Gender = account.Gender;
            // if user is not current user
            if (account.Id != myAccount.Id)
            {
                //check friend condition
                if (account.FriendShips1.Where(x => x.Account.Id == myAccount.Id).Any() && account.FriendShips1.Where(x => x.Account.Id == myAccount.Id).SingleOrDefault().Status == IS_FRIEND
                        && myAccount.FriendShips1.Where(x => x.Account.Id == account.Id).Any() && myAccount.FriendShips1.Where(x => x.Account.Id == account.Id).SingleOrDefault().Status == IS_FRIEND)
                {
                    userModel.FriendStatus = IS_FRIEND;
                }
                else
                {
                    userModel.FriendStatus = NOT_FRIEND;
                }
            }

            return userModel;
        }

        public JsonResult GetLocationList(string trajectId)
        {

            var locateList = new List<LocationViewModel>();
            var listLocation = this.context.Locations
                .Where(x => x.TrajectoryId.Equals(trajectId) && x.IsActive)
                .OrderBy(x => x.CreatedDate).ToList();
            for (int i = 0; i < listLocation.Count; i++)
            {
                var model = new LocationViewModel();
                model.Id = listLocation[i].Id.ToString();
                model.Latitude = listLocation[i].Latitude;
                model.Longitude = listLocation[i].Longitude;
                model.Address = listLocation[i].Address;
                model.Index = i + 1;
                DateTime dt = listLocation[i].CreatedDate;
                model.CreatedDate2 = String.Format("{0:MM/dd/yyyy}", dt);

                model.CreatedDate = String.Format("{0:f}", dt);

                locateList.Add(model);
            }


            return Json(locateList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocationListByDate(string trajectId, string createdDate)
        {

            DateTime date = Convert.ToDateTime(createdDate);
            var locateList = new List<LocationViewModel>();
            var listLocation = this.context.Locations
                .Where(x => x.TrajectoryId.Equals(trajectId) && x.IsActive)
                .OrderBy(x => x.CreatedDate).ToList();
            for (int i = 0; i < listLocation.Count; i++)
            {
                var model = new LocationViewModel();
                model.Id = listLocation[i].Id.ToString();
                model.Latitude = listLocation[i].Latitude;
                model.Longitude = listLocation[i].Longitude;
                model.Index = i + 1;
                //model.CreatedDate = listLocation[i].CreatedDate.ToString();

                DateTime dt = listLocation[i].CreatedDate;
                model.CreatedDate = String.Format("{0:f}", dt);

                if (SupportUtility.CompareDate(date, listLocation[i].CreatedDate))
                {
                    locateList.Add(model);
                }

            }

            return Json(locateList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocationListByDateRange(string trajectId, string fromDate, string toDate)
        {

            DateTime date1 = Convert.ToDateTime(fromDate);
            DateTime date2 = Convert.ToDateTime(toDate);
            var locateList = new List<LocationViewModel>();
            var listLocation = this.context.Locations
                .Where(x => x.TrajectoryId.Equals(trajectId) && x.IsActive)
                .OrderBy(x => x.CreatedDate).ToList();
            for (int i = 0; i < listLocation.Count; i++)
            {
                var model = new LocationViewModel();
                model.Id = listLocation[i].Id.ToString();
                model.Latitude = listLocation[i].Latitude;
                model.Longitude = listLocation[i].Longitude;
                model.Index = i + 1;
                //model.CreatedDate = listLocation[i].CreatedDate.ToString();

                DateTime dt = listLocation[i].CreatedDate;
                model.CreatedDate2 = String.Format("{0:MM/dd/yyyy}", dt);

                model.CreatedDate = String.Format("{0:f}", dt);
                DateTime date = Convert.ToDateTime(model.CreatedDate2);

                //if (SupportUtility.CompareDate(date, listLocation[i].CreatedDate))
                //{
                //    locateList.Add(model);
                //}
                if ((DateTime.Compare(date1, date) <= 0 &&
                    DateTime.Compare(date2, date) >= 0) ||
                    (DateTime.Compare(date2, date) <= 0 &&
                    DateTime.Compare(date1, date) >= 0)
                    )
                {
                    locateList.Add(model);
                }

            }

            return Json(locateList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetImgList(string locateId)
        {
            var imgList = new List<string>();
            var locate = this.context.Locations.Where(x => x.Id == locateId).SingleOrDefault();
            var img = locate.CapturedImages.ToList();
            foreach (var i in img)
            {
                imgList.Add(USER_DATA_FOLDER + i.ImageUrl);
            }


            return Json(imgList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTrajectory(string trajectId)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectId)).SingleOrDefault();
            var model = new TrajectoryViewModel();
            model.Id = trajectory.Id;
            model.TrajectoryName = trajectory.TrajectoryName;
            model.Status = trajectory.Status;
            model.CreateDate = trajectory.CreatedDate.ToString();
            model.Description = trajectory.Description;
            model.LastUpdate = trajectory.LastUpdated.ToString();
            model.Status = trajectory.Status;

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTrajectoryList()
        {
            var trajectory = this.context.Trajectories.ToList();
            var  trajectList = new List<string>();
            for (int i = 0; i < trajectory.Count; i++)
            {
                trajectList.Add(trajectory[i].TrajectoryName);
            }
                return Json(trajectList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveTrajectory(TrajectoryViewModel model, TrajectSearchCriteriaViewModel searchUserModel)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(model.Id)).SingleOrDefault();
            trajectory.TrajectoryName = model.TrajectoryName;
            trajectory.Description = model.Description;
            trajectory.Status = model.Status.Trim();
            trajectory.LastUpdated = DateTime.Now;

            int result = this.context.SaveChanges();

            return RedirectToAction("ListTrajectory", new { UserId = searchUserModel.UserId, SearchKeyword = searchUserModel.SearchKeyword, PageNumber = searchUserModel.PageNumber, PageCount = searchUserModel.PageCount, DateTo = searchUserModel.DateTo, DateFrom = searchUserModel.DateFrom });
        }

        public JsonResult IsMyTrajectory(string userName, string locationId)
        {
            bool result = false;
            var location = this.context.Locations.Where(x => x.Id == locationId).SingleOrDefault();
            var trajectory = this.context.Trajectories.Where(x => x.Id == location.TrajectoryId).SingleOrDefault();
            var user = this.context.Accounts.Where(x => x.Id == trajectory.UserId).SingleOrDefault();
            if (user.Username.Equals(userName))
            {
                result = true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRouteString(string startId, string desId, string mode)
        {
            string result = "";
            var locationRoute = this.context.LocationRoutes
                .Where(x => x.StartLocationId.Equals(startId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.DestinationLocationId.Equals(desId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.TravelMode.Equals(mode, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Type == true)
                .SingleOrDefault();
            var startLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(startId, StringComparison.InvariantCultureIgnoreCase));
            var desLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(desId, StringComparison.InvariantCultureIgnoreCase));
            if (startLocation.IsActive && desLocation.IsActive && locationRoute != null)
            {
                var routingApimodel = new RoutingApiModel()
                {
                    StartLocationId = locationRoute.StartLocationId,
                    DestinationLocationId = locationRoute.DestinationLocationId,
                    TravelMode = locationRoute.TravelMode,
                    RouteString = locationRoute.RouteString,
                    TrajectoryId = locationRoute.TrajectoryId,
                    Index = 0
                };
                return Json(routingApimodel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var routingApimodel = new RoutingApiModel()
                {
                    StartLocationId = startId,
                    DestinationLocationId = desId,
                    TravelMode = mode,
                    RouteString = "Empty",
                    TrajectoryId = "Empty",
                    Index = 0
                };
                return Json(routingApimodel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateRouteString(string startId, string desId, string mode, string routeString)
        {
            int result = 0;
            routeString = HttpUtility.UrlDecode(routeString, Encoding.UTF8);
            var locationRoute = this.context.LocationRoutes
                .Where(x => x.StartLocationId.Equals(startId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.DestinationLocationId.Equals(desId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.TravelMode.Equals(mode, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Type == true)
                .SingleOrDefault();
            var startLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(startId, StringComparison.InvariantCultureIgnoreCase));
            var desLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(desId, StringComparison.InvariantCultureIgnoreCase));
            if (startLocation.IsActive && desLocation.IsActive)
            {
                if (locationRoute == null)
                {
                    locationRoute = new LocationRoute()
                    {
                        StartLocationId = startId,
                        DestinationLocationId = desId,
                        TravelMode = mode,
                        RouteString = routeString,
                        TrajectoryId = desLocation.TrajectoryId,
                        Type = true
                    };
                    this.context.LocationRoutes.Add(locationRoute);
                    result = this.context.SaveChanges();
                }
            }
            return Json(result > 0 ? "OK" : "KO", JsonRequestBehavior.AllowGet);
        }
    }
}
