using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class ImageApiModel
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public string CreatedDate { get; set; }
    }
}