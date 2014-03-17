using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class LocationViewModel
    {
        public string id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string CreatedDate { get; set; }

        public List<string> imgList { get; set; }

        public LocationViewModel()
        {
            imgList = new List<String>();
        }
    }
}