using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class AccountFriendApiModel
    {
        public long Id { get; set; }
        public String Username { get; set; }
        public String Fullname { get; set; }
        public String Password { get; set; }
        public String Avatar { get; set; }
        public String Email { get; set; }
    }
}