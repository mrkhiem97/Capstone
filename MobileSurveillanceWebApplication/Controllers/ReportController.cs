using IOManagerLibrary;
using MobileSurveillanceWebApplication.Models.DatabaseModel;
using MobileSurveillanceWebApplication.Models.ViewModel;
using MobileSurveillanceWebApplication.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class ReportController : Controller
    {
        private const String USER_DATA_FOLDER = "UserData";
        private readonly MobileSurveillanceContext context = new MobileSurveillanceContext();

        public ActionResult Report(string trajectoryId)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectoryId, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            var trajectoryViewModel = new TrajectoryViewModel
            {
                TrajectoryName = trajectory.TrajectoryName,
                Id = trajectory.Id,
                CreateDate = trajectory.CreatedDate.ToLongDateString(),
                TotalLocation = trajectory.Locations.Count(x => x.IsActive)
            };
            return View(trajectoryViewModel);
        }

        public async Task<JsonResult> GetReportData(string trajectoryId)
        {
            var trajectory = this.context.Trajectories.Where(x => x.Id.Equals(trajectoryId, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            var list = await this.GetListReport(trajectoryId);
            return Json(new
            {
                TrajectoryName = HttpUtility.JavaScriptStringEncode(trajectory.TrajectoryName),
                ChartData = list
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ReportViewModel> GetReportViewModel(Location start, Location destination, double cummulativeDistance)
        {
            ReportViewModel retVal = null;
            var locationRoute = this.context.LocationRoutes
                .Where(x => x.StartLocationId.Equals(start.Id, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.DestinationLocationId.Equals(destination.Id, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.TravelMode.Equals("DRIVING", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Type == true)
                .SingleOrDefault();
            var startLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(start.Id, StringComparison.InvariantCultureIgnoreCase));
            var desLocation = this.context.Locations.SingleOrDefault(x => x.Id.Equals(destination.Id, StringComparison.InvariantCultureIgnoreCase));
            if (startLocation.IsActive && desLocation.IsActive && locationRoute != null)
            {
                String result = locationRoute.RouteString;
                // Parse route string
                var distance = ParseDistance(result);
                retVal = new ReportViewModel()
                {
                    TakenDate = destination.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    Distance = distance,
                    TotalDistance = Math.Round(cummulativeDistance + distance, 3)
                };
            }
            else
            {
                var distance = await GetDrivingDistanceInKilometers(start, destination);
                retVal = new ReportViewModel()
                {
                    TakenDate = destination.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    Distance = distance,
                    TotalDistance = Math.Round(cummulativeDistance + distance, 3)
                };
            }
            return retVal;
        }


        /// <summary>
        /// returns driving distance in miles
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public async Task<double> GetDrivingDistanceInKilometers(Location start, Location des)
        {
            double retVal = 0;

            String origin = String.Format("{0:0.0000000},{1:0.0000000}", start.Latitude, start.Longitude);
            String destination = String.Format("{0:0.0000000},{1:0.0000000}", des.Latitude, des.Longitude);
            string url = @"http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" +
              origin + "&destinations=" + destination +
              "&mode=driving&sensor=true&language=en-EN";

            var task = Task.Factory.StartNew(async delegate
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                WebResponse response = await request.GetResponseAsync();
                Stream dataStream = response.GetResponseStream();
                StreamReader sreader = new StreamReader(dataStream);
                string responsereader = sreader.ReadToEnd();
                response.Close();
                Console.WriteLine("Response: " + responsereader);
                this.UpdateRouteString(start.Id, des.Id, "DRIVING", responsereader);
                return ParseDistance(responsereader);
            });
            retVal = await task.Result;

            return retVal;
        }

        public double Velocity(double distance, DateTime startTime, DateTime desTime)
        {
            double retVal = 0;
            var time = (desTime - startTime).TotalHours;
            try
            {
                retVal = Math.Round(distance / time, 2);
            }
            catch (DivideByZeroException ex)
            {
                retVal = 0;
            }
            return retVal;
        }

        private static double ParseDistance(string responsereader)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(responsereader);


            if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
            {
                XmlNodeList distance = xmldoc.GetElementsByTagName("distance");
                return Convert.ToDouble(distance[0].ChildNodes[0].InnerText) / 1000;
            }
            return 0;
        }

        private void UpdateRouteString(string startId, string desId, string mode, string routeString)
        {
            int result = 0;
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
                }
                else
                {
                    locationRoute.RouteString = routeString;
                }
            }
            result = this.context.SaveChanges();

        }

        public async Task<ActionResult> ExcelReport(string trajectoryId)
        {
            String path = String.Format("~/{0}/{1}/{2}", USER_DATA_FOLDER, User.Identity.Name, "Report");

            string root = Server.MapPath(path);
            if (!IOManager.IsDirectoryExisted(root))
            {
                IOManager.MakeDirectory(root);
            }
            String newFile = String.Format("{0}\\{1}{2}", root, trajectoryId.Replace(':', 'z'), ".xls");
            if (System.IO.File.Exists(newFile))
            {
                System.IO.File.Delete(newFile);
            }
            // Copy template
            System.IO.File.Copy(Server.MapPath("~/App_Data/TemplateReport.xls"), newFile, true);

            String conectionString = String.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};Extended Properties=\"Excel 8.0;HDR=YES\"", newFile);
            DataAccessHelper helper = new DataAccessHelper(conectionString);

            var list = await GetListReport(trajectoryId);

            for (int i = 0; i < list.Count; i++)
            {
                String query = "Insert Into [Sheet1$] VALUES(@Location, @Address, @Distance, @TotalDistance, @Velocity, @TakenDate, @Latitude, @Longitude, @TimeInterval, @TotalTime)";
                var parameters = new IDataParameter[]
                                 {
                                     new OleDbParameter("@Location", OleDbType.VarChar) { Value = "Location " + (i + 1).ToString()}, 
                                     new OleDbParameter("@Address", OleDbType.VarChar) {Value = list[i].Address},
                                     new OleDbParameter("@Distance", OleDbType.Double) {Value = list[i].Distance},
                                     new OleDbParameter("@TotalDistance", OleDbType.Double) {Value = list[i].TotalDistance},
                                     new OleDbParameter("@Velocity", OleDbType.Double) {Value = list[i].Velocity},
                                     new OleDbParameter("@TakenDate", OleDbType.VarChar) {Value = list[i].TakenDate},
                                     new OleDbParameter("@Latitude", OleDbType.Double) {Value = list[i].Latitude},
                                     new OleDbParameter("@Longitude", OleDbType.Double) {Value = list[i].Longitude},
                                     new OleDbParameter("@TimeInterval", OleDbType.VarChar) {Value = list[i].TimeInterval},
                                     new OleDbParameter("@TotalTime", OleDbType.VarChar) {Value = list[i].TotalTime},
                                 };
                int result = helper.ExecuteNonQuery(query, parameters);
            }
            return File(newFile, "application/vnd.ms-excel", "Report.xls");
        }

        private async Task<List<ReportViewModel>> GetListReport(string trajectoryId)
        {
            var listLocation = this.context.Locations
                                .Where(x => x.TrajectoryId.Equals(trajectoryId) && x.IsActive)
                                .OrderBy(x => x.CreatedDate).ToList();
            var list = new List<ReportViewModel>();
            double cummulativeDistance = 0;
            if (listLocation.Count > 1)
            {
                for (int i = 0; i < listLocation.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        var startReportViewModel = new ReportViewModel()
                        {
                            TakenDate = listLocation[i].CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                            Distance = 0,
                            TotalDistance = cummulativeDistance,
                            Marker = "/Images/marker-start.png",
                            Velocity = 0,
                            Address = listLocation[i].Address,
                            Latitude = Math.Round(listLocation[i].Latitude, 5),
                            Longitude = Math.Round(listLocation[i].Longitude, 5),
                            LocationNumber = i + 1,
                            TimeInterval = SupportUtility.TotalTime(listLocation[i].CreatedDate, listLocation[i].CreatedDate),
                            TotalTime = SupportUtility.TotalTime(listLocation[i].CreatedDate, listLocation[i].CreatedDate)
                        };
                        list.Add(startReportViewModel);
                    }
                    var startLocation = listLocation[i];
                    var desLocation = listLocation[i + 1];
                    var reportViewModel = await GetReportViewModel(startLocation, desLocation, cummulativeDistance);
                    reportViewModel.Velocity = this.Velocity(reportViewModel.Distance, startLocation.CreatedDate, desLocation.CreatedDate);
                    reportViewModel.Address = desLocation.Address;
                    reportViewModel.Latitude = Math.Round(desLocation.Latitude, 5);
                    reportViewModel.Longitude = Math.Round(desLocation.Longitude, 5);
                    reportViewModel.LocationNumber = i + 2;
                    reportViewModel.TimeInterval = SupportUtility.TotalTime(startLocation.CreatedDate, desLocation.CreatedDate);
                    reportViewModel.TotalTime = SupportUtility.TotalTime(listLocation[0].CreatedDate, desLocation.CreatedDate);
                    reportViewModel.Marker = "/Images/marker-next.png";
                    if (i + 1 == listLocation.Count - 1)
                    {
                        reportViewModel.Marker = "/Images/marker-stop.png";
                    }
                    Console.WriteLine("Distance: " + reportViewModel.Distance);
                    cummulativeDistance += reportViewModel.Distance;
                    list.Add(reportViewModel);
                }
            }
            else if (listLocation.Count == 1)
            {
                var reportViewModel = new ReportViewModel()
                {
                    Address = listLocation[0].Address,
                    TotalDistance = 0,
                    Distance = 0,
                    Latitude = listLocation[0].Latitude,
                    Longitude = listLocation[0].Longitude,
                    Velocity = 0,
                    Marker = "/Images/marker-start.png",
                    TakenDate = listLocation[0].CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    LocationNumber = 1,
                    TimeInterval = SupportUtility.TotalTime(listLocation[0].CreatedDate, listLocation[0].CreatedDate),
                    TotalTime = SupportUtility.TotalTime(listLocation[0].CreatedDate, listLocation[0].CreatedDate)
                };
                Console.WriteLine("Distance: " + reportViewModel.Distance);
                list.Add(reportViewModel);
            }
            return list;
        }
    }
}
