using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class UserViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public  DateTime? LastLogin { get; set; }
        public string IsActive { get; set; }
        public string Avatar { get; set; }
        public string RoleId { get; set; }
    }

    /// <summary>
    /// View Model for List Friends
    /// </summary>
    public class ListUserViewModel
    {
        public List<UserViewModel> ListUser { get; set; }

        public ListUserViewModel()
        {
            this.ListUser = new List<UserViewModel>();
        }
    }

    /// <summary>
    /// View Model for Filtering and Paging
    /// </summary>
    public class SearchCriteriaViewModel
    {
        public string SearchKeyword { get; set; }

        public int PageNumber { get; set; }

        public int PageCount { get; set; }
    }

    public class TrajectSearchCriteriaViewModel : SearchCriteriaViewModel
    {

        public long UserId { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }
    }

   
}