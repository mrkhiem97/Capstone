using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class LocationController : Controller
    {
        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();
        private const String USER_DATA_FOLDER = "/UserData/";

        [Authorize]
        public ActionResult ManageLocationImages(String locationId, int index)
        {
            var location = this.context.Locations.SingleOrDefault(x => x.Id.Equals(locationId, StringComparison.InvariantCultureIgnoreCase));
            if (location == null)
            {
                return RedirectToAction("ListTrajectories", "Trajectory");
            }
            if (!location.Trajectory.Account.Username.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return RedirectToAction("ListTrajectories", "Trajectory");
            }
            var trajectoryViewModel = new TrajectoryViewModel
            {
                TrajectoryName = location.Trajectory.TrajectoryName,
                Id = location.Trajectory.Id,
                CreateDate = String.Format("{0:f}", location.Trajectory.CreatedDate)
            };
            var listImages = new List<ImageViewModel>();
            var listActiveImages = location.CapturedImages.Where(x => x.IsActive).ToList();
            for (int i = 0; i < listActiveImages.Count; i++)
            {
                var capturedImage = location.CapturedImages.ElementAt(i);
                var imageViewModel = new ImageViewModel()
                {
                    Id = capturedImage.Id,
                    CreatedDate = String.Format("{0:f}", capturedImage.CreatedDate),
                    ImageUrl = USER_DATA_FOLDER + capturedImage.ImageUrl,
                    Width = 400,
                    Height = (400 * capturedImage.Height) / capturedImage.Width
                };
                listImages.Add(imageViewModel);
            }
            var locationViewModel = new LocationViewModel()
            {
                Id = location.Id,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Index = index,
                Address = location.Address,
                CreatedDate = String.Format("{0:f}", location.CreatedDate),
            };
            ViewBag.TrajectoryViewModel = trajectoryViewModel;
            ViewBag.LocationViewModel = locationViewModel;
            return View(listImages);
        }


        public JsonResult DeleteLocation(string locationId)
        {
            var location = this.context.Locations.SingleOrDefault(x => x.Id.Equals(locationId, StringComparison.InvariantCultureIgnoreCase));
            location.IsActive = false;
            for (int i = 0; i < location.CapturedImages.Count; i++)
            {
                location.CapturedImages.ElementAt(i).IsActive = false;
            }
            int result = this.context.SaveChanges();
            string message;
            if (result > 0)
            {
                message = "Success";
                return Json(new
                {
                    Message = message,
                    TrajectoryId = location.TrajectoryId
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                message = "Error, please try again!";
                return Json(new
                {
                    Message = message,
                    TrajectoryId = location.TrajectoryId
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteImage(long imageId, String locationId, int index)
        {
            var location = this.context.Locations.SingleOrDefault(x => x.Id.Equals(locationId, StringComparison.InvariantCultureIgnoreCase));
            var image = this.context.CapturedImages.SingleOrDefault(x => x.Id == imageId);
            image.IsActive = false;
            int result = this.context.SaveChanges();
            string message;
            if (result > 0)
            {
                message = "Success";
                return Json(new
                {
                    Message = message,
                    LocationId = locationId,
                    Index = index,
                    TrajectoryId = location.TrajectoryId,
                    Count = location.CapturedImages.Count(x => x.IsActive)
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                message = "Error, please try again!";
                return Json(new
                {
                    Message = message,
                    TrajectoryId = location.TrajectoryId,
                    Count = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
