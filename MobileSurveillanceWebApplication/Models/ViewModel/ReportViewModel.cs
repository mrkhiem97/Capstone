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
        public double TotalDistance { get; set; }
        public double Velocity { get; set; }
        public String Marker { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public String Address { get; set; }

        public String TimeInterval { get; set; }

        public String TotalTime { get; set; }

        public int LocationNumber { get; set; }
    }
}