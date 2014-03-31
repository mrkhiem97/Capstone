using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class LoadApiModel
    {
        private String id;
        public String Id
        {
            get
            {
                return id;
            }
            set
            {
                id = HttpUtility.UrlDecode(value, Encoding.UTF8);
            }
        }
    }
}