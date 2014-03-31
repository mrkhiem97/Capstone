using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class ImageViewModel
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; }
        public String CreatedDate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}