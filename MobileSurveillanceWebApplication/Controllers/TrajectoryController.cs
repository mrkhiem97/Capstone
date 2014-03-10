using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace MobileSurveillanceWebApplication.Controllers
{
    public class TrajectoryController : Controller
    {
        //
        // GET: /Trajectory/
        private readonly MobileSurveillanceEntities context = new MobileSurveillanceEntities();
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
            this.context.Trajectories.Remove(trajectory);
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

        public ActionResult ListTrajectories()
        {

            return RedirectToAction("ListTrajectory", new { SearchKeyword = " ", PageNumber = 1, PageCount = 0 });
        }

        [Authorize]

        public ActionResult ListTrajectory(TrajectSearchCriteriaViewModel searchUserModel)
        {
            //page size
            int pageSize = 3;

            // List trajectory
            var listTrajectModel = new ListTrajectoryViewModel();
            // Get username
            var username = User.Identity.Name;
            // Get account
            var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            // Get trajectory of the username
            var listTraject = new List<Trajectory>();
            // Condition
            if (!String.IsNullOrEmpty(searchUserModel.SearchKeyword.Trim()) && !String.IsNullOrWhiteSpace(searchUserModel.SearchKeyword.Trim()))
            {
                listTraject = account.Trajectories.Where(x => x.TrajectoryName.Contains(searchUserModel.SearchKeyword.Trim()) || x.TrajectoryName.Contains(searchUserModel.SearchKeyword.Trim())).ToList();
            }
            else
            {
                listTraject = account.Trajectories.ToList();
            }
            searchUserModel.PageCount = listTraject.Count / pageSize;
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
                int count = trajectory.Locations.Count - 1;
                if (trajectory.Locations.Count > 0)
                {
                    for (int i = count; i >= 0; i--)
                    {
                        trajectoryViewModel.locateList.Add(trajectory.Locations.ElementAt(i));
                    }
                }
                listTrajectModel.ListTrajectory.Add(trajectoryViewModel);
            }

            ViewBag.SearchCriteriaViewModel = searchUserModel;
            return View(listTrajectModel);
        }

        [Authorize]
        public ActionResult ListTrajectoryByUser(long userId)
        {
            var model = new ListTrajectoryViewModel();

            var listTrajectory = this.context.Trajectories.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedDate);

            foreach (var trajectory in listTrajectory)
            {
                var trajectoryViewModel = new TrajectoryViewModel();
                trajectoryViewModel.Id = trajectory.Id;
                trajectoryViewModel.TrajectoryName = trajectory.TrajectoryName;
                trajectoryViewModel.Description = trajectory.Description;
                trajectoryViewModel.Status = trajectory.Status;
                trajectoryViewModel.CreateDate = trajectory.CreatedDate.ToString();
                trajectoryViewModel.LastUpdate = trajectory.LastUpdated.ToString();
                int count = trajectory.Locations.Count - 1;
                if (trajectory.Locations.Count > 0)
                {
                    for (int i = count; i >= 0; i--)
                    {
                        trajectoryViewModel.locateList.Add(trajectory.Locations.ElementAt(i));
                    }
                }
                model.ListTrajectory.Add(trajectoryViewModel);
            }
            return View(model);
        }


        public JsonResult GetLocationList(string trajectId)
        {
            var locateList = new List<LocationViewModel>();
            var listLocation = this.context.Locations
                .Where(x => x.TrajectoryId.Equals(trajectId))
                .OrderBy(x => x.CreatedDate).ToList();
            for (int i = 0; i < listLocation.Count; i++)
            {
                var model = new LocationViewModel();
                model.id = listLocation[i].Id;
                model.Latitude = listLocation[i].Latitude;
                model.Longitude = listLocation[i].Longitude;
                model.CreatedDate = listLocation[i].CreatedDate.ToString();

                locateList.Add(model);
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
                imgList.Add(i.ImageUrl);
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

        public JsonResult SaveTrajectory(string trajectId, string name, string description, string status)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectId)).SingleOrDefault();
            trajectory.TrajectoryName = name;
            trajectory.Description = description;
            trajectory.Status = status.Trim();

            int result = this.context.SaveChanges();
            var message = "";
            if (result > 0)
            {
                message = "Change saved.";
            } else {
                message = "Nothing changes.";
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }

    }
}
