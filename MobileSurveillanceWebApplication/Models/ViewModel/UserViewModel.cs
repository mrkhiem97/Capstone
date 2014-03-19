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
        public DateTime? LastLogin { get; set; }
        public string IsActive { get; set; }
        public string Avatar { get; set; }
        public string RoleId { get; set; }
        public string Address { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class FriendViewModel : UserViewModel
    {
        public string FriendStatus { get; set; }
        public long MyId { get; set; }
        public long MyFriendId { get; set; }
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
    /// View model for List User
    /// </summary>
    public class ListFriendViewModel
    {
        public List<FriendViewModel> ListFriend { get; set; }

        public SearchCriteriaViewModel SearchCriteriaViewModel { get; set; }

        public ListFriendViewModel()
        {
            this.ListFriend = new List<FriendViewModel>();
            this.SearchCriteriaViewModel = new SearchCriteriaViewModel();
        }
    }

    /// <summary>
    /// View Model for Filtering and Paging
    /// </summary>
    public class SearchCriteriaViewModel
    {
        private String searchKeyword;
        public String SearchKeyword
        {
            get
            {
                return searchKeyword;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    searchKeyword = String.Empty;
                }
                else
                {
                    searchKeyword = value;
                }
            }
        }

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