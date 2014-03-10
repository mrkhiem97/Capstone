//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MobileSurveillanceWebApplication.Models.DatabaseModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Location
    {
        public Location()
        {
            this.CapturedImages = new HashSet<CapturedImage>();
        }
    
        public string Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string TrajectoryId { get; set; }
    
        public virtual Trajectory Trajectory { get; set; }
        public virtual ICollection<CapturedImage> CapturedImages { get; set; }
    }
}
