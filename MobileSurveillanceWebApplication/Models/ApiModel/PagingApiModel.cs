using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class PagingApiModel
    {
        public String SearchQuery { get; set; }

        public String Page { get; set; }

        public String PageSize { get; set; }
    }
}