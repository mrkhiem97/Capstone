using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ApiModel
{
    public class PagingApiModel
    {
        private String searchQuery;
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
                    searchQuery = value;
                }
            }
        }

        public String Username
        {
            get;
            set;
        }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}