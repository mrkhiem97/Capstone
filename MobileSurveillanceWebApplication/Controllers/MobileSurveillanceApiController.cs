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
using System.Drawing;
using MobileSurveillanceWebApplication.Models.ViewModel;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class MobileSurveillanceApiController : ApiController
    {
        private const String USER_DATA_FOLDER = "UserData";

        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();

        /// <summary>
        /// Login into the system
        /// </summary>
        /// <param name="accountModel"></param>
        /// <returns></returns>
        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage Login()
        {
            var account = this.context.Accounts.SingleOrDefault(x => x.Id == ((BasicAuthenticationIdentity)User.Identity).Id);
            account.LastLogin = DateTime.Now;
            this.context.SaveChanges();
            HttpResponseMessage retVal = Request.CreateResponse(HttpStatusCode.OK, ((BasicAuthenticationIdentity)User.Identity).Id, Configuration.Formatters.JsonFormatter);
            return retVal;
        }

        /// <summary>
        /// Update trajectory include add, edit, remove
        /// </summary>
        /// <param name="trajectoryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage UpdateTrajectory(TrajectoryApiModel trajectoryModel)
        {
            HttpResponseMessage retVal = null;
            TrajectoryApiModel trajectoryApiModel = null;
            long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;

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
                        trajectoryApiModel = trajectoryModel;
                    }
                    else
                    {
                        retVal = Request.CreateResponse(HttpStatusCode.NotModified, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
                        return retVal;
                    }
                }
                catch (Exception)
                {
                    retVal = Request.CreateResponse(HttpStatusCode.BadRequest, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
                    return retVal;
                }
            }
            else
            {
                if (!trajectoryModel.IsActive)
                {
                    trajectory.IsActive = false;
                    this.context.SaveChanges();
                    trajectoryApiModel = trajectoryModel;
                    // Update lại trajectory
                    retVal = Request.CreateResponse(HttpStatusCode.NotModified, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
                    return retVal;
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
                                trajectoryApiModel = trajectoryModel;
                            }
                            else
                            {
                                retVal = Request.CreateResponse(HttpStatusCode.NotModified, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
                                return retVal;
                            }
                        }
                        else if (lastUpdated == trajectory.LastUpdated)
                        {
                            trajectoryApiModel = trajectoryModel;
                        }
                        else
                        {
                            trajectoryModel.LastUpdated = trajectory.LastUpdated.ToString(SupportUtility.TIME_FORMAT);
                            trajectoryApiModel = trajectoryModel;
                        }
                    }
                    else
                    {
                        retVal = Request.CreateResponse(HttpStatusCode.NotModified, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
                        return retVal;
                    }
                }
            }
            if (trajectoryApiModel != null)
            {
                retVal = Request.CreateResponse(HttpStatusCode.OK, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
            }
            return retVal;
        }

        /// <summary>
        /// Load trajectories
        /// </summary>
        /// <param name="pagingModel"></param>
        /// <returns></returns>
        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage LoadTrajectories([FromBody]PagingApiModel pagingModel)
        {
            HttpResponseMessage retVal = null;
            var list = new List<TrajectoryApiModel>();
            int page = pagingModel.Page;
            int pageSize = pagingModel.PageSize;
            var account = this.context.Accounts.SingleOrDefault(x => x.Username.Equals(pagingModel.Username));
            ICollection<Trajectory> listTrajectory = null;
            if (String.IsNullOrEmpty(pagingModel.SearchQuery))
            {
                if (!User.Identity.Name.Equals(pagingModel.Username, StringComparison.InvariantCultureIgnoreCase))
                {
                    listTrajectory = account.Trajectories.Where(x => x.IsActive && x.Status.Equals("public", StringComparison.InvariantCultureIgnoreCase))
                    .OrderByDescending(x => x.LastUpdated)
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(page * pageSize).Take(pageSize)
                    .ToList();
                }
                else
                {
                    listTrajectory = account.Trajectories.Where(x => x.IsActive)
                    .OrderByDescending(x => x.LastUpdated)
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(page * pageSize).Take(pageSize)
                    .ToList();
                    var listNotActiveTrajectories = account.Trajectories.Where(x => !x.IsActive)
                        .OrderByDescending(x => x.LastUpdated)
                        .Take(20).ToList();
                    for (int i = 0; i < listNotActiveTrajectories.Count; i++)
                    {
                        listTrajectory.Add(listNotActiveTrajectories[i]);
                    }
                }

            }
            else
            {
                if (!User.Identity.Name.Equals(pagingModel.Username, StringComparison.InvariantCultureIgnoreCase))
                {
                    listTrajectory = account.Trajectories
                    .Where(x => x.IsActive && x.TrajectoryName.ToLower().Contains(pagingModel.SearchQuery)
                        && x.Status.Equals("public", StringComparison.InvariantCultureIgnoreCase))
                    .OrderByDescending(x => x.LastUpdated)
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(page * pageSize).Take(pageSize)
                    .ToList();
                }
                else
                {
                    listTrajectory = account.Trajectories
                    .Where(x => x.IsActive && x.TrajectoryName.ToLower().Contains(pagingModel.SearchQuery))
                    .OrderByDescending(x => x.LastUpdated)
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(page * pageSize).Take(pageSize)
                    .ToList();
                    var listNotActiveTrajectories = account.Trajectories.Where(x => !x.IsActive && x.TrajectoryName.ToLower()
                        .Contains(pagingModel.SearchQuery))
                        .OrderByDescending(x => x.LastUpdated)
                        .Take(20).ToList();
                    for (int i = 0; i < listNotActiveTrajectories.Count; i++)
                    {
                        listTrajectory.Add(listNotActiveTrajectories[i]);
                    }
                }
            }

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
            retVal = Request.CreateResponse(HttpStatusCode.OK, list, Configuration.Formatters.JsonFormatter);
            return retVal;
        }


        /// <summary>
        /// Load Friends
        /// </summary>
        /// <param name="pagingModel"></param>
        /// <returns></returns>
        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage LoadFriends([FromBody]PagingApiModel pagingModel)
        {
            HttpResponseMessage retVal = null;
            var list = new List<AccountFriendApiModel>();
            int page = pagingModel.Page;
            int pageSize = pagingModel.PageSize;
            long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;
            // Get user account
            ICollection<FriendShip> listFriendShip = null;
            if (String.IsNullOrEmpty(pagingModel.SearchQuery))
            {
                listFriendShip = this.context.FriendShips
                    .Where(x => x.MyId == accountId)
                    .Where(x => x.Status.Equals(FriendStatus.IS_FRIEND, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(x => x.Account.Username)
                    .OrderBy(x => x.Account.Fullname)
                    .ToList();
            }
            else
            {
                listFriendShip = this.context.FriendShips
                    .Where(x => x.MyId == accountId)
                    .Where(x => x.Status.Equals(FriendStatus.IS_FRIEND, StringComparison.InvariantCultureIgnoreCase))
                    .Where(x => x.Account.Username.ToLower().Contains(pagingModel.SearchQuery) || x.Account.Fullname.ToLower().Contains(pagingModel.SearchQuery) || x.Account.Email.ToLower().Contains(pagingModel.SearchQuery))
                    .OrderBy(x => x.Account.Username)
                    .OrderBy(x => x.Account.Fullname)
                    .ToList();
            }

            for (int i = 0; i < listFriendShip.Count; i++)
            {
                var myId = listFriendShip.ElementAt(i).MyId;
                var myFriendId = listFriendShip.ElementAt(i).MyFriendId;
                var isBothFriend = this.context.FriendShips
                    .Where(x => x.MyId == myFriendId)
                    .Where(x => x.MyFriendId == myId)
                    .Where(x => x.Status.Equals(FriendStatus.IS_FRIEND, StringComparison.InvariantCultureIgnoreCase))
                    .Any();
                if (isBothFriend)
                {
                    var accountFriendApiModel = new AccountFriendApiModel();
                    accountFriendApiModel.Id = listFriendShip.ElementAt(i).Account.Id;
                    accountFriendApiModel.Username = listFriendShip.ElementAt(i).Account.Username;
                    accountFriendApiModel.Fullname = listFriendShip.ElementAt(i).Account.Fullname;
                    accountFriendApiModel.Email = listFriendShip.ElementAt(i).Account.Email;
                    accountFriendApiModel.Avatar = listFriendShip.ElementAt(i).Account.Avatar.Remove(0, 1);
                    list.Add(accountFriendApiModel);
                }
            }
            list = list.Skip(page * pageSize).Take(pageSize).ToList();
            retVal = Request.CreateResponse(HttpStatusCode.OK, list, Configuration.Formatters.JsonFormatter);
            return retVal;
        }


        /// <summary>
        /// Load Friends
        /// </summary>
        /// <param name="pagingModel"></param>
        /// <returns></returns>
        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage LoadPeople([FromBody]PagingApiModel pagingModel)
        {
            HttpResponseMessage retVal = null;
            var list = new List<AccountFriendApiModel>();
            int page = pagingModel.Page;
            int pageSize = pagingModel.PageSize;
            long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;
            // Get user account
            ICollection<Account> listAccount = null;
            if (String.IsNullOrEmpty(pagingModel.SearchQuery))
            {
                listAccount = this.context.Accounts
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Username)
                    .OrderBy(x => x.Fullname)
                    .Skip(page * pageSize).Take(pageSize)
                    .ToList();
            }
            else
            {
                listAccount = this.context.Accounts
                    .Where(x => x.IsActive)
                    .Where(x => x.Username.ToLower().Contains(pagingModel.SearchQuery) || x.Fullname.ToLower().Contains(pagingModel.SearchQuery) || x.Email.ToLower().Contains(pagingModel.SearchQuery))
                    .OrderBy(x => x.Username)
                    .OrderBy(x => x.Fullname)
                    .Skip(page * pageSize).Take(pageSize)
                    .ToList();
            }
            for (int i = 0; i < listAccount.Count; i++)
            {
                var accountFriendApiModel = new AccountFriendApiModel();
                accountFriendApiModel.Id = listAccount.ElementAt(i).Id;
                accountFriendApiModel.Username = listAccount.ElementAt(i).Username;
                accountFriendApiModel.Fullname = listAccount.ElementAt(i).Fullname;
                accountFriendApiModel.Email = listAccount.ElementAt(i).Email;
                accountFriendApiModel.Avatar = listAccount.ElementAt(i).Avatar.Remove(0, 1);
                list.Add(accountFriendApiModel);
            }
            retVal = Request.CreateResponse(HttpStatusCode.OK, list, Configuration.Formatters.JsonFormatter);
            return retVal;
        }


        /// <summary>
        /// Load Friends
        /// </summary>
        /// <param name="pagingModel"></param>
        /// <returns></returns>
        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage LoadImages([FromBody]LoadApiModel model)
        {
            HttpResponseMessage retVal = null;
            var list = new List<ImageApiModel>();
            var listImages = this.context.CapturedImages
                .Where(x => x.LocationId.Equals(model.Id, StringComparison.InvariantCultureIgnoreCase) && x.IsActive)
                .OrderBy(x => x.CreatedDate).ToList();
            for (int i = 0; i < listImages.Count; i++)
            {
                var image = new ImageApiModel();
                image.Id = listImages[i].Id;
                image.ImageUrl = listImages[i].ImageUrl;
                image.Address = listImages[i].Location.Address;
                image.CreatedDate = listImages[i].CreatedDate.ToString(SupportUtility.TIME_FORMAT);
                list.Add(image);
            }
            retVal = Request.CreateResponse(HttpStatusCode.OK, list, Configuration.Formatters.JsonFormatter);
            return retVal;
        }

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public async Task<HttpResponseMessage> LoadLocations([FromBody]LoadApiModel model)
        {
            HttpResponseMessage retVal = null;
            var list = new List<LocationApiModel>();
            var listLocations = this.context.Locations
                .Where(x => x.TrajectoryId.Equals(model.Id, StringComparison.InvariantCultureIgnoreCase) && x.IsActive)
                .OrderBy(x => x.CreatedDate).ToList();
            for (int i = 0; i < listLocations.Count; i++)
            {
                var location = new LocationApiModel();
                location.Id = listLocations[i].Id;
                location.Latitude = listLocations[i].Latitude;
                location.Longitude = listLocations[i].Longitude;
                if (String.IsNullOrEmpty(listLocations[i].Address))
                {
                    listLocations[i].Address = await ReverseGeoCoding.GetAddress(listLocations[i].Latitude, listLocations[i].Longitude);
                    this.context.SaveChanges();
                }
                location.Address = listLocations[i].Address;
                location.IsActive = listLocations[i].IsActive;
                location.CreatedDate = listLocations[i].CreatedDate.ToString(SupportUtility.TIME_FORMAT);
                list.Add(location);
            }
            retVal = Request.CreateResponse(HttpStatusCode.OK, list, Configuration.Formatters.JsonFormatter);
            return retVal;
        }

        /// <summary>
        /// Upload multipart form data
        /// </summary>
        /// <returns></returns>
        [BasicAuthenticationFilter(true)]
        public async Task<HttpResponseMessage> UploadMultiPartFormData()
        {
            HttpResponseMessage retVal = null;
            long accountId = ((BasicAuthenticationIdentity)User.Identity).Id;


            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            String path = String.Format("~/{0}/{1}/{2}", USER_DATA_FOLDER, ((BasicAuthenticationIdentity)User.Identity).Name, "Image");

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
                            retVal = await this.RegisterNewLocation(imageLocationApiModel);
                        }
                    }
                    else
                    {
                        retVal = await this.RegisterNewLocation(imageLocationApiModel);
                    }
                }
                else
                {
                    var trajectoryApiModel = new TrajectoryApiModel()
                    {
                        Id = trajectory.Id,
                        Description = trajectory.Description,
                        Status = trajectory.Status,
                        IsActive = trajectory.IsActive,
                        Name = trajectory.TrajectoryName,
                        CreatedDate = trajectory.CreatedDate.ToString(SupportUtility.TIME_FORMAT),
                        LastUpdated = trajectory.LastUpdated.ToString(SupportUtility.TIME_FORMAT)
                    };
                    retVal = Request.CreateResponse(HttpStatusCode.Forbidden, trajectoryApiModel, Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                retVal = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message, Configuration.Formatters.JsonFormatter);
            }

            return retVal;
        }

        private HttpResponseMessage UpdateLocation(ImageLocationApiModel imageLocationApiModel, Location bestLocation)
        {
            HttpResponseMessage retVal = null;
            var captureImage = new CapturedImage();
            captureImage.ImageUrl = imageLocationApiModel.ImageUrl;
            captureImage.CreatedDate = imageLocationApiModel.CreatedDate;
            captureImage.IsActive = true;
            captureImage.Width = imageLocationApiModel.Width;
            captureImage.Height = imageLocationApiModel.Height;
            if (String.IsNullOrEmpty(bestLocation.Address) || String.IsNullOrWhiteSpace(bestLocation.Address))
            {
                bestLocation.Address = imageLocationApiModel.Address;
            }
            bestLocation.CapturedImages.Add(captureImage);

            // Add location successfull
            try
            {
                if (this.context.SaveChanges() > 0)
                {
                    retVal = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    retVal = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                }
            }
            catch (Exception)
            {
                retVal = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            }
            return retVal;
        }

        private Location FindBestLocation(ImageLocationApiModel imageLocationApiModel)
        {
            var listLocation = this.context.Locations
                .Where(x => x.IsActive)
                .Where(x => x.TrajectoryId.Equals(imageLocationApiModel.TrajectoryId, StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(x => x.CreatedDate).ToList();
            Location bestLocation = null;
            for (int i = 0; i < listLocation.Count; i++)
            {
                var location = listLocation[i];
                var distance = SupportUtility.Distance(imageLocationApiModel.Latitude, imageLocationApiModel.Longitude, location.Latitude, location.Longitude, DistanceUnit.Meter);
                if (distance <= imageLocationApiModel.CompactDistance)
                {
                    bestLocation = location;
                    break;
                }
            }
            return bestLocation;
        }

        private async Task<HttpResponseMessage> RegisterNewLocation(ImageLocationApiModel imageLocationApiModel)
        {
            HttpResponseMessage retVal = null;
            imageLocationApiModel.Address = await ReverseGeoCoding.GetAddress(imageLocationApiModel.Latitude, imageLocationApiModel.Longitude);
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
                    retVal = new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            return retVal;
        }

        private HttpResponseMessage AddNewLocation(ImageLocationApiModel imageLocationApiModel)
        {
            HttpResponseMessage retVal = null;
            var location = new Location();
            location.TrajectoryId = imageLocationApiModel.TrajectoryId;
            location.Id = imageLocationApiModel.LocationId;
            location.Latitude = imageLocationApiModel.Latitude;
            location.Longitude = imageLocationApiModel.Longitude;
            location.CreatedDate = imageLocationApiModel.CreatedDate;
            location.Address = imageLocationApiModel.Address;
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
                    retVal = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    retVal = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                }
            }
            catch (Exception)
            {
                retVal = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
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
                        imageLocationApiModel.TrajectoryId = HttpUtility.UrlDecode(value, Encoding.UTF8);
                    }
                    else if (key.Equals("locationid", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.LocationId = HttpUtility.UrlDecode(value, Encoding.UTF8);
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
                    else if (key.Equals("address", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageLocationApiModel.Address = HttpUtility.UrlDecode(value, Encoding.UTF8);
                    }
                }
            }

            // This illustrates how to get the file names for uploaded files.
            foreach (var file in provider.FileData)
            {
                String imageUrl = String.Format("/{0}/{1}/{2}", username, "Image", provider.GetFilename());
                imageLocationApiModel.ImageUrl = imageUrl;
                var image = Image.FromFile(file.LocalFileName, true);
                imageLocationApiModel.Width = image.Width;
                imageLocationApiModel.Height = image.Height;
                image.Dispose();
            }
            return imageLocationApiModel;
        }

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage UpdateRoutingResult([FromBody] RoutingApiModel routingApiModel)
        {
            HttpResponseMessage retVal = null;
            var locationRoute = this.context.LocationRoutes
                .Where(x => x.StartLocationId.Equals(routingApiModel.StartLocationId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.DestinationLocationId.Equals(routingApiModel.DestinationLocationId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.TravelMode.Equals(routingApiModel.TravelMode, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Type == false)
                .SingleOrDefault();
            var startLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(routingApiModel.StartLocationId, StringComparison.InvariantCultureIgnoreCase));
            var desLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(routingApiModel.DestinationLocationId, StringComparison.InvariantCultureIgnoreCase));
            if (startLocation.IsActive && desLocation.IsActive)
            {
                if (locationRoute == null)
                {
                    locationRoute = new LocationRoute()
                    {
                        StartLocationId = routingApiModel.StartLocationId,
                        DestinationLocationId = routingApiModel.DestinationLocationId,
                        TravelMode = routingApiModel.TravelMode,
                        RouteString = routingApiModel.RouteString,
                        TrajectoryId = routingApiModel.TrajectoryId,
                        Type = false
                    };

                    this.context.LocationRoutes.Add(locationRoute);
                }
                else
                {
                    locationRoute.RouteString = routingApiModel.RouteString;
                }

                try
                {
                    // Save changed
                    if (this.context.SaveChanges() > 0)
                    {
                        retVal = Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        retVal = Request.CreateResponse(HttpStatusCode.NotAcceptable);
                    }
                }
                catch (Exception)
                {
                    retVal = Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                retVal = Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            return retVal;
        }


        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage DeleteLocation([FromBody]DeleteApiModel deleteApiModel)
        {
            HttpResponseMessage retVal = null;
            var location = this.context.Locations.SingleOrDefault(x => x.Id.Equals(deleteApiModel.Id, StringComparison.InvariantCultureIgnoreCase));
            location.IsActive = false;
            for (int i = 0; i < location.CapturedImages.Count; i++)
            {
                location.CapturedImages.ElementAt(i).IsActive = false;
            }
            try
            {
                // Save changed
                if (this.context.SaveChanges() > 0)
                {
                    retVal = Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    retVal = Request.CreateResponse(HttpStatusCode.NotAcceptable);
                }
            }
            catch (Exception)
            {
                retVal = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return retVal;
        }


        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage DeleteImage([FromBody]DeleteApiModel deleteApiModel)
        {
            HttpResponseMessage retVal = null;
            int id = int.Parse(deleteApiModel.Id);
            var capturedImage = this.context.CapturedImages.SingleOrDefault(x => x.Id == id);
            capturedImage.IsActive = false;
            try
            {
                // Save changed
                if (this.context.SaveChanges() > 0)
                {
                    retVal = Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    retVal = Request.CreateResponse(HttpStatusCode.NotAcceptable);
                }
            }
            catch (Exception)
            {
                retVal = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return retVal;
        }

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage LoadListRouting()
        {
            HttpResponseMessage retVal = null;
            List<LoadRoutingApiModel> model = GetListLoadRoutingApiModel();


            var list = new List<RoutingApiModel>();
            for (int i = 0; i < model.Count; i++)
            {
                var loadRoutingApiModel = model.ElementAt(i);
                var locationRoute = this.context.LocationRoutes
                                        .Where(x => x.StartLocationId.Equals(loadRoutingApiModel.StartLocationId, StringComparison.InvariantCultureIgnoreCase))
                                        .Where(x => x.DestinationLocationId.Equals(loadRoutingApiModel.DestinationLocationId, StringComparison.InvariantCultureIgnoreCase))
                                        .Where(x => x.TravelMode.Equals(loadRoutingApiModel.TravelMode, StringComparison.InvariantCultureIgnoreCase))
                                        .Where(x => x.Type == false)
                                        .SingleOrDefault();

                var startLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(loadRoutingApiModel.StartLocationId, StringComparison.InvariantCultureIgnoreCase));
                var desLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(loadRoutingApiModel.DestinationLocationId, StringComparison.InvariantCultureIgnoreCase));
                if (startLocation.IsActive && desLocation.IsActive)
                {
                    if (locationRoute != null)
                    {
                        var routingApimodel = new RoutingApiModel()
                        {
                            StartLocationId = locationRoute.StartLocationId,
                            DestinationLocationId = locationRoute.DestinationLocationId,
                            TravelMode = locationRoute.TravelMode,
                            RouteString = locationRoute.RouteString,
                            TrajectoryId = locationRoute.TrajectoryId,
                            Index = loadRoutingApiModel.Index
                        };
                        list.Add(routingApimodel);
                    }
                    else
                    {
                        var routingApimodel = new RoutingApiModel()
                        {
                            StartLocationId = loadRoutingApiModel.StartLocationId,
                            DestinationLocationId = loadRoutingApiModel.DestinationLocationId,
                            TravelMode = loadRoutingApiModel.TravelMode,
                            RouteString = "Empty",
                            TrajectoryId = "Empty",
                            Index = loadRoutingApiModel.Index
                        };
                        list.Add(routingApimodel);
                    }
                }
                else
                {
                    retVal = Request.CreateResponse(HttpStatusCode.NotFound);
                    return retVal;
                }
            }

            retVal = Request.CreateResponse(HttpStatusCode.OK, list, Configuration.Formatters.JsonFormatter);
            return retVal;
        }

        private List<LoadRoutingApiModel> GetListLoadRoutingApiModel()
        {
            List<LoadRoutingApiModel> model = new List<LoadRoutingApiModel>();

            var message = Request.Content.ReadAsFormDataAsync().Result;
            var arrStartLocationId = message.GetValues("StartLocationId").ToList<String>();
            var arrDesLocationId = message.GetValues("DestinationLocationId").ToList<String>();
            var arrTravelMode = message.GetValues("TravelMode").ToList<String>();
            var arrIndex = message.GetValues("Index").ToList<String>();

            for (int i = 0; i < arrStartLocationId.Count; i++)
            {
                var loadRoutingApiModel = new LoadRoutingApiModel();
                loadRoutingApiModel.StartLocationId = arrStartLocationId[i];
                loadRoutingApiModel.DestinationLocationId = arrDesLocationId[i];
                loadRoutingApiModel.TravelMode = arrTravelMode[i];
                loadRoutingApiModel.Index = int.Parse(arrIndex[i]);
                model.Add(loadRoutingApiModel);
            }
            return model;
        }

        [HttpPost]
        [BasicAuthenticationFilter(true)]
        public HttpResponseMessage LoadRouting([FromBody]LoadRoutingApiModel model)
        {
            HttpResponseMessage retVal = null;
            var locationRoute = this.context.LocationRoutes
                .Where(x => x.StartLocationId.Equals(model.StartLocationId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.DestinationLocationId.Equals(model.DestinationLocationId, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.TravelMode.Equals(model.TravelMode, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Type == false)
                .SingleOrDefault();

            var startLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(model.StartLocationId, StringComparison.InvariantCultureIgnoreCase));
            var desLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(model.DestinationLocationId, StringComparison.InvariantCultureIgnoreCase));
            if (startLocation.IsActive && desLocation.IsActive && locationRoute != null)
            {
                var routingApimodel = new RoutingApiModel()
                {
                    StartLocationId = locationRoute.StartLocationId,
                    DestinationLocationId = locationRoute.DestinationLocationId,
                    TravelMode = locationRoute.TravelMode,
                    RouteString = locationRoute.RouteString,
                    TrajectoryId = locationRoute.TrajectoryId,
                    Index = model.Index
                };
                retVal = Request.CreateResponse(HttpStatusCode.OK, routingApimodel, Configuration.Formatters.JsonFormatter);
            }
            else
            {
                retVal = Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return retVal;
        }

    }
}
