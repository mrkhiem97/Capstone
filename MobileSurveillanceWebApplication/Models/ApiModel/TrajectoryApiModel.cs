using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class TrajectoryApiModel
    {
        private String id;
        private String name;
        private String description;
        private String status;

        public String Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public String Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public String CreatedDate { get; set; }

        public String LastUpdated { get; set; }

        public String Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public String Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public bool IsActive { get; set; }
    }
}