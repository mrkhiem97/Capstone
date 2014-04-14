using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class ReportViewModel
    {
        public String TakenDate { get; set; }
        public double Distance { get; set; }
        public double CummulativeDistance { get; set; }
        public double Velocity { get; set; }
        public String Marker { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public String Address { get; set; }
    }
}