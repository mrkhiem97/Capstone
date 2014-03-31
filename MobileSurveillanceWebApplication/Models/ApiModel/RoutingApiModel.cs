using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class RoutingApiModel
    {
        private String startLocationId;
        private String destinationLocationId;
        private String travelMode;
        private String routeString;
        private String trajectoryId;

        public string StartLocationId
        {
            get
            {
                return this.startLocationId;
            }
            set
            {
                this.startLocationId = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public string DestinationLocationId
        {
            get
            {
                return this.destinationLocationId;
            }
            set
            {
                this.destinationLocationId = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public string TravelMode
        {
            get
            {
                return this.travelMode;
            }
            set
            {
                this.travelMode = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }
        public string RouteString
        {
            get
            {
                return this.routeString;
            }
            set
            {
                this.routeString = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public string TrajectoryId
        {
            get
            {
                return this.trajectoryId;
            }
            set
            {
                this.trajectoryId = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }
    }

    public class LoadRoutingApiModel
    {
        private String startLocationId;
        private String destinationLocationId;
        private String travelMode;
        public string StartLocationId
        {
            get
            {
                return this.startLocationId;
            }
            set
            {
                this.startLocationId = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public string DestinationLocationId
        {
            get
            {
                return this.destinationLocationId;
            }
            set
            {
                this.destinationLocationId = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }

        public string TravelMode
        {
            get
            {
                return this.travelMode;
            }
            set
            {
                this.travelMode = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }
    }
}