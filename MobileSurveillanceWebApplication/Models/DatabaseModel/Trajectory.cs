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
    
    public partial class Trajectory
    {
        public Trajectory()
        {
            this.Locations = new HashSet<Location>();
        }
    
        public string Id { get; set; }
        public string TrajectoryName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public long UserId { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
    }
}