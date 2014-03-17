using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class ImageLocationApiModel
    {
        public String TrajectoryId { get; set; }

        public String LocationId { get; set; }

        public double Latitude { get; set;}

        public double Longitude { get; set; }

        public DateTime CreatedDate { get; set; }

        public String ImageUrl { get; set; }

        public double CompactDistance { get; set; }

        public int Width { get; set;}

        public int Height { get; set; }

        private String address;

        public String Address
        {
            get
            {
                return address;
            }
            set
            { 
                if (value == null || value == "null")
                {
                    address = String.Empty;
                }
                else
                {
                    address = value;
                }
            }
        }
    }
}