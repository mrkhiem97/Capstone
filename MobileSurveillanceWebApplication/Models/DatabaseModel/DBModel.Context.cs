﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EntityContext : DbContext
    {
        public EntityContext()
            : base("name=MobileSurveillanceEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<FriendShip> FriendShips { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Trajectory> Trajectories { get; set; }
        public virtual DbSet<CapturedImage> CapturedImages { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
    }
}
