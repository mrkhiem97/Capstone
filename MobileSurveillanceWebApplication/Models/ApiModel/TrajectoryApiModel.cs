using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class TrajectoryApiModel
    {
        public String Id { get; set; }

        public String Name { get; set; }

        public String CreatedDate { get; set; }

        public String LastUpdated { get; set; }

        public String Description { get; set; }

        public String Status { get; set; }

        public bool IsActive { get; set; }
    }
}