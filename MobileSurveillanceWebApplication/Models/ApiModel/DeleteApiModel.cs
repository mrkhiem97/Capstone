using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class DeleteApiModel
    {
        private String id;

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = HttpUtility.UrlDecode(value, Encoding.UTF8).ToLower();
            }
        }
    }
}