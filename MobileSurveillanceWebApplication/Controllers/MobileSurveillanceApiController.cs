using MobileSurveillanceWebApplication.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Utility;
using System.Web.Http.ModelBinding;
using System.Web.Http.Filters;
using MobileSurveillanceWebApplication.HttpSupportUtility;
using IOManagerLibrary;
using MobileSurveillanceWebApplication.Filters;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class MobileSurveillanceApiController : ApiController
    {
        private readonly EntityContext context = new EntityContext();

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public bool Login(AccountApiModel accountModel)
        {
            bool retVal = User.Identity.IsAuthenticated;
            return retVal;
        }

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public TrajectoryApiModel UpdateTrajectory(TrajectoryApiModel trajectoryModel)
        {
            TrajectoryApiModel retVal = null;
            if (User.Identity.IsAuthenticated)
            {
                long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;
                trajectoryModel.Id = HttpUtility.UrlDecode(trajectoryModel.Id, Encoding.UTF8);
                trajectoryModel.Name = HttpUtility.UrlDecode(trajectoryModel.Name, Encoding.UTF8);
                trajectoryModel.Description = HttpUtility.UrlDecode(trajectoryModel.Description, Encoding.UTF8);

                var trajectory = this.context.Trajectories.SingleOrDefault(x => x.Id.Equals(trajectoryModel.Id, StringComparison.InvariantCultureIgnoreCase));
                DateTime lastUpdated = SupportUtility.ConvertFormattedStringToDateTime(trajectoryModel.LastUpdated);
                // If there is no trajectory in database then insert them
                if (trajectory == null)
                {
                    // Get user id in database
                    trajectory = new Trajectory();
                    trajectory.Id = trajectoryModel.Id;
                    trajectory.TrajectoryName = trajectoryModel.Name;
                    trajectory.CreatedDate = SupportUtility.ConvertFormattedStringToDateTime(trajectoryModel.CreatedDate);
                    trajectory.LastUpdated = lastUpdated;
                    trajectory.Description = trajectoryModel.Description;
                    trajectory.Status = trajectoryModel.Status;
                    trajectory.IsActive = trajectoryModel.IsActive;
                    trajectory.UserId = accountId;
                    this.context.Trajectories.Add(trajectory);
                    try
                    {
                        if (this.context.SaveChanges() > 0)
                        {
                            retVal = trajectoryModel;
                        }
                        else
                        {
                            retVal = null;
                        }
                    }
                    catch (Exception)
                    { }
                }
                else
                {
                    if (!trajectoryModel.IsActive)
                    {
                        trajectory.IsActive = false;
                        this.context.SaveChanges();
                        retVal = null;
                    }
                    else
                    {
                        if (trajectory.IsActive)
                        {
                            // If last update time greater than databse last update time then this is new one to be edited
                            if (lastUpdated > trajectory.LastUpdated)
                            {
                                trajectory.TrajectoryName = trajectoryModel.Name;

                                trajectory.LastUpdated = lastUpdated;
                                trajectory.Description = trajectoryModel.Description;
                                trajectory.Status = trajectoryModel.Status;
                                trajectory.IsActive = trajectoryModel.IsActive;
                                if (this.context.SaveChanges() > 0)
                                {
                                    retVal = trajectoryModel;
                                }
                                else
                                {
                                    retVal = null;
                                }
                            }
                            else if (lastUpdated == trajectory.LastUpdated)
                            {
                                retVal = trajectoryModel;
                            }
                            else
                            {
                                trajectoryModel.LastUpdated = trajectory.LastUpdated.ToString(SupportUtility.TIME_FORMAT);
                                retVal = trajectoryModel;
                            }
                        }
                        else
                        {
                            retVal = null;
                        }
                    }
                }
            }
            else
            {
                retVal = null;
            }
            return retVal;
        }

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public IEnumerable<TrajectoryApiModel> LoadTrajectories([FromBody]PagingApiModel pagingModel)
        {
            var list = new List<TrajectoryApiModel>();
            pagingModel.SearchQuery = HttpUtility.UrlDecode(pagingModel.SearchQuery, Encoding.UTF8);
            int page = int.Parse(pagingModel.Page);
            int pageSize = int.Parse(pagingModel.PageSize);
            if (User.Identity.IsAuthenticated)
            {
                long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;
                // Get user account
                var account = (from x in this.context.Accounts where x.Id == accountId select x).Single();
                ICollection<Trajectory> listTrajectory = null;
                if (String.IsNullOrEmpty(pagingModel.SearchQuery))
                {
                    listTrajectory = account.Trajectories.Where(x => x.IsActive).OrderByDescending(x => x.LastUpdated).OrderByDescending(x => x.CreatedDate).Skip(page * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    //listTrajectory = account.Trajectories.Skip(page * pageSize).Take(pageSize).ToList();
                    listTrajectory = (from trajectory in account.Trajectories
                                      where
                                          trajectory.TrajectoryName.ToLower()
                                          .Contains(pagingModel.SearchQuery.ToLower())
                                      select trajectory)
                                      .Where(x => x.IsActive)
                                      .OrderByDescending(x => x.LastUpdated)
                                      .OrderByDescending(x => x.CreatedDate)
                                      .Skip(page * pageSize).Take(pageSize).ToList<Trajectory>();
                }

                if (listTrajectory.Count == 0 || listTrajectory == null)
                {
                    list = null;
                }
                else
                {
                    for (int i = 0; i < listTrajectory.Count; i++)
                    {
                        var trajectoryApiModel = new TrajectoryApiModel();
                        trajectoryApiModel.Id = listTrajectory.ElementAt(i).Id.ToString();
                        trajectoryApiModel.Name = listTrajectory.ElementAt(i).TrajectoryName;
                        trajectoryApiModel.CreatedDate = listTrajectory.ElementAt(i).CreatedDate.ToString(SupportUtility.TIME_FORMAT);
                        trajectoryApiModel.LastUpdated = listTrajectory.ElementAt(i).LastUpdated.ToString(SupportUtility.TIME_FORMAT);
                        trajectoryApiModel.Description = listTrajectory.ElementAt(i).Description;
                        trajectoryApiModel.Status = listTrajectory.ElementAt(i).Status;
                        trajectoryApiModel.IsActive = listTrajectory.ElementAt(i).IsActive;
                        list.Add(trajectoryApiModel);
                    }
                }
            }
            else
            {
                list = null;
            }
            return list;
        }


        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public IEnumerable<AccountFriendApiModel> LoadFriends([FromBody]PagingApiModel pagingModel)
        {
            var list = new List<AccountFriendApiModel>();
            if (!String.IsNullOrEmpty(pagingModel.SearchQuery))
            {
                pagingModel.SearchQuery = HttpUtility.UrlDecode(pagingModel.SearchQuery, Encoding.UTF8).ToLower();

            }
            int page = int.Parse(pagingModel.Page);
            int pageSize = int.Parse(pagingModel.PageSize);
            if (User.Identity.IsAuthenticated)
            {
                long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;
                // Get user account
                var account = (from x in this.context.Accounts where x.Id == accountId select x).Single();
                ICollection<FriendShip> listFriendShip = null;
                if (String.IsNullOrEmpty(pagingModel.SearchQuery))
                {
                    listFriendShip = account.FriendShips1.Where(x => x.Account.IsActive).OrderBy(x => x.Account.Fullname).OrderBy(x => x.Account.Username).Skip(page * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    //listTrajectory = account.Trajectories.Skip(page * pageSize).Take(pageSize).ToList();
                    listFriendShip = account.FriendShips1
                                      .Where(x => x.Account.Username.Contains(pagingModel.SearchQuery) || x.Account.Fullname.Contains(pagingModel.SearchQuery))
                                      .OrderByDescending(x => x.Account.Fullname)
                                      .OrderByDescending(x => x.Account.Username)
                                      .Skip(page * pageSize).Take(pageSize).ToList();
                }

                if (listFriendShip.Count == 0 || listFriendShip == null)
                {
                    list = null;
                }
                else
                {
                    for (int i = 0; i < listFriendShip.Count; i++)
                    {
                        var accountFriendApiModel = new AccountFriendApiModel();
                        accountFriendApiModel.Id = listFriendShip.ElementAt(i).Account.Id;
                        accountFriendApiModel.Username = listFriendShip.ElementAt(i).Account.Username;
                        accountFriendApiModel.Fullname = listFriendShip.ElementAt(i).Account.Fullname;
                        accountFriendApiModel.Avatar = listFriendShip.ElementAt(i).Account.Avatar;
                        list.Add(accountFriendApiModel);
                    }
                }
            }
            else
            {
                list = null;
            }
            return list;
        }

        [BasicAuthenticationFilter(true)]
        public async Task<bool> UploadMultiPartFormData()
        {
            bool retVal = false;
            if (User.Identity.IsAuthenticated)
            {
                long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;


                // Check if the request contains multipart/form-data.
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                String path = String.Format("~/UserData/{0}/{1}", ((BasicAuthenticationIdentity)User.Identity).Name, "Image");

                string root = HttpContext.Current.Server.MapPath(path);
                if (!IOManager.IsDirectoryExisted(root))
                {
                    IOManager.MakeDirectory(root);
                }
                var provider = new ExtendMultipartFormDataStreamProvider(root);
                try
                {
                    await Request.Content.ReadAsMultipartAsync(provider);
                    var imageLocationApiModel = GetImageLocation(provider, ((BasicAuthenticationIdentity)User.Identity).Name);
                    var trajectory = this.context.Trajectories.SingleOrDefault(x => x.Id.Equals(imageLocationApiModel.TrajectoryId, StringComparison.InvariantCultureIgnoreCase));
                    if (trajectory != null && trajectory.IsActive)
                    {
                        // Check if any distance calculation
                        // Get list location, sort by date
                        // Note that checking active of location
                        if (imageLocationApiModel.CompactDistance > 0)
                        {
                            Location bestLocation = this.FindBestLocation(imageLocationApiModel);
                            // If cannot find best location, then just add new
                            if (bestLocation != null)
                            {
                                retVal = this.UpdateLocation(imageLocationApiModel, bestLocation);
                            }
                            else
                            {
                                retVal = this.RegisterNewLocation(imageLocationApiModel);
                            }
                        }
                        else
                        {
                            retVal = this.RegisterNewLocation(imageLocationApiModel);
                        }
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                catch (Exception)
                {
                    retVal = false;
                }
            }
            else
            {
                retVal = false;
            }

            return retVal;
        }

        private bool UpdateLocation(ImageLocationApiModel imageLocationApiModel, Location bestLocation)
        {
            bool retVal = false;
            var captureImage = new CapturedImage();
            captureImage.ImageUrl = imageLocationApiModel.ImageUrl;
            captureImage.CreatedDate = imageLocationApiModel.CreatedDate;
            captureImage.IsActive = true;
            captureImage.Width = imageLocationApiModel.Width;
            captureImage.Height = imageLocationApiModel.Height;
            bestLocation.CapturedImages.Add(captureImage);

            // Add location successfull
            try
            {
                if (this.context.SaveChanges() > 0)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception)
            {
                retVal = false;
            }
            return retVal;
        }

        private Location FindBestLocation(ImageLocationApiModel imageLocationApiModel)
        {
            var listLocation = this.context.Locations
                .Where(x => x.TrajectoryId.Equals(imageLocationApiModel.TrajectoryId, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(x => x.CreatedDate).ToList();
            var minDistance = double.MaxValue;
            Location bestLocation = null;
            for (int i = 0; i < listLocation.Count; i++)
            {
                var location = listLocation[i];
                var distance = SupportUtility.Distance(imageLocationApiModel.Latitude, imageLocationApiModel.Longitude, location.Latitude, location.Longitude, DistanceUnit.Meter);
                if (distance <= imageLocationApiModel.CompactDistance && distance <= minDistance)
                {
                    minDistance = distance;
                    bestLocation = location;
                }
            }
            return bestLocation;
        }

        private bool RegisterNewLocation(ImageLocationApiModel imageLocationApiModel)
        {
            bool retVal = false;
            // Check for existed location
            var location = this.context.Locations
                .Where(x => x.Id.Equals(imageLocationApiModel.LocationId, StringComparison.InvariantCultureIgnoreCase))
                .SingleOrDefault();

            // If this location does't have in database
            if (location == null)
            {
                // Add to database
                // First add location

                retVal = this.AddNewLocation(imageLocationApiModel);
            }
            else
            {
                var capturedImage = location.CapturedImages.Where(x => x.CreatedDate == imageLocationApiModel.CreatedDate).SingleOrDefault();
                // Remove old images
                //IEnumerable<CapturedImage> listImages = location.CapturedImages.ToList();
                //this.context.CapturedImages.RemoveRange(listImages);
                if (capturedImage == null)
                {
                    retVal = this.UpdateLocation(imageLocationApiModel, location);
                }
                else
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        private bool AddNewLocation(ImageLocationApiModel imageLocationApiModel)
        {
            bool retVal = false;
            var location = new Location();
            location.TrajectoryId = imageLocationApiModel.TrajectoryId;
            location.Id = imageLocationApiModel.LocationId;
            location.Latitude = imageLocationApiModel.Latitude;
            location.Longitude = imageLocationApiModel.Longitude;
            location.CreatedDate = imageLocationApiModel.CreatedDate;
            location.IsActive = true;

            var captureImage = new CapturedImage();
            captureImage.ImageUrl = imageLocationApiModel.ImageUrl;
            captureImage.CreatedDate = imageLocationApiModel.CreatedDate;
            captureImage.IsActive = true;
            captureImage.Width = imageLocationApiModel.Width;
            captureImage.Height = imageLocationApiModel.Height;
            location.CapturedImages.Add(captureImage);

            this.context.Locations.Add(location);

            // Add location successfull
            try
            {
                if (this.context.SaveChanges() > 0)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception)
            {
                retVal = false;
            }
            return retVal;
        }

        private static ImageLocationApiModel GetImageLocation(ExtendMultipartFormDataStreamProvider provider, String username)
        {
            ImageLocationApiModel imageLocationApiModel = new ImageLocationApiModel();


            // This illustrates how to get the form data.
            foreach (var key in provider.FormData.AllKeys)
            {
                foreach (var value in provider.FormData.GetValues(key))
                {
                    if (key.Equals("trajectoryid", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.TrajectoryId = value;
                    }
                    else if (key.Equals("locationid", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.LocationId = value;
                    }
                    else if (key.Equals("latitude", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.Latitude = double.Parse(value);
                    }
                    else if (key.Equals("longitude", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.Longitude = double.Parse(value);
                    }
                    else if (key.Equals("createdate", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.CreatedDate = SupportUtility.ConvertFormattedStringToDateTime(value);
                    }
                    else if (key.Equals("compactdistance", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.CompactDistance = double.Parse(value);
                    }
                    else if (key.Equals("width", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.Width = int.Parse(value);
                    }
                    else if (key.Equals("height", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.Height = int.Parse(value);
                    }
                }
            }

            // This illustrates how to get the file names for uploaded files.
            foreach (var file in provider.FileData)
            {
                String imageUrl = String.Format("/{0}/{1}/{2}", username, "Image", provider.GetFilename());
                imageLocationApiModel.ImageUrl = imageUrl;
            }
            return imageLocationApiModel;
        }
    }
}
