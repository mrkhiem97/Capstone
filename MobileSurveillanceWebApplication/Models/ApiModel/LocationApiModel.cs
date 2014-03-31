using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class LocationApiModel
    {
        public string Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public String CreatedDate { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
    }
}