using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class AccountApiModel
    {
        private String username;
        private String password;

        public String Username
        {
            get
            {
                return this.username;
            }
            set
            {
                this.username = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }
        public String Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }
    }
}