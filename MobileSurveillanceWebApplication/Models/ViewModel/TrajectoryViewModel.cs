using MobileSurveillanceWebApplication.Models.DatabaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class TrajectoryViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Trajectory name is required")]
        [StringLength(800, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Trajectory Name")]
        public string TrajectoryName { get; set; }
        public string CreateDate { get; set; }
        public string LastUpdate { get; set; }
        public string Status { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public string StartLatitude { get; set; }
        public string StartLongitude { get; set; }

        public string EndLatitude { get; set; }
        public string EndLongitude { get; set; }

        public List<Location> LocateList { get; set; }

        public String ModalEditId
        {
            get;
            set;
        }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string StartAddress { get; set; }

        public string EndAddress { get; set; }

        public int TotalLocation { get; set; }

        public String TotalTime { get; set; }

        public TrajectoryViewModel()
        {
            LocateList = new List<Location>();
        }
    }

    public class ListTrajectoryViewModel
    {
        public List<TrajectoryViewModel> ListTrajectory { get; set; }

        public FriendViewModel FriendViewModel { get; set; }
        public ListTrajectoryViewModel()
        {
            this.ListTrajectory = new List<TrajectoryViewModel>();
            this.FriendViewModel = new FriendViewModel();
        }
    }
}