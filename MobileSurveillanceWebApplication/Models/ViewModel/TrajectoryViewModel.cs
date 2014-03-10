using MobileSurveillanceWebApplication.Models.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class TrajectoryViewModel
    {
        public string Id { get; set; }
        public string TrajectoryName { get; set; }
        public string CreateDate { get; set; }
        public string LastUpdate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public List<Location> locateList { get; set; }

        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }

        public TrajectoryViewModel()
        {
            locateList = new List<Location>();
        }
    }

    public class ListTrajectoryViewModel
    {
        public List<TrajectoryViewModel> ListTrajectory { get; set; }

        public ListTrajectoryViewModel()
        {
            this.ListTrajectory = new List<TrajectoryViewModel>();
        }

    }


}