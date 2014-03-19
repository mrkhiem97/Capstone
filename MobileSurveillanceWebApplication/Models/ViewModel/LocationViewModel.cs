using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class LocationViewModel
    {
        public string Id { get; set; }

        public int Index { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string CreatedDate { get; set; }

        public string CreatedDate2 { get; set; }

        public List<string> ImgList { get; set; }

        public string Address { get; set; }

        public LocationViewModel()
        {
            ImgList = new List<String>();
        }
    }
}