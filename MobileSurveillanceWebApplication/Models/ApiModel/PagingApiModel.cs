using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class PagingApiModel
    {
        private String searchQuery;
        private String username;

        public String SearchQuery
        {
            get
            {
                return searchQuery;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    searchQuery = String.Empty;
                }
                else
                {
                    searchQuery = HttpUtility.UrlDecode(value, Encoding.UTF8).ToLower();
                }
            }
        }

        public String Username
        {
            get
            {
                return this.username;
            }
            set
            {
                this.username = HttpUtility.UrlDecode(value, Encoding.UTF8).ToLower();
            }
        }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}