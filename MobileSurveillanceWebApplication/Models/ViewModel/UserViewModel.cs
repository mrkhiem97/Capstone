using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public static class FriendStatus
    {
        public const string IS_FRIEND = "1";
        public const string NOT_FRIEND = "0";
        public const string CONFIRM_NEED = "confirmNeed";
        public const string REQUEST_SENT = "requestSent";
        public const string FRIEND = "friend";
        public const string NOT_YET_FRIEND = "notFriend";
    }


    public class UserViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool Gender { get; set; }

        [Required(ErrorMessage = "Fullname is required")]
        [RegularExpression("^[^-!$@%^&*()_+|~=`{}\\[\\]:\";'<>?,\\/]*$", ErrorMessage = "Invalid name characters")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string Fullname { get; set; }
        public DateTime? LastLogin { get; set; }
        public string IsActive { get; set; }
        public string Avatar { get; set; }
        public string RoleId { get; set; }

        [Display(Name = "Address")]
        [AllowHtml]
        [DataType(DataType.Text)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        [Display(Name = "Birthday")]
        [DataType(DataType.Text)]
        public DateTime Birthday { get; set; }
        public int CountMutualFriend { get; set; }

        public int CountTrajectory { get; set; }
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
        public List<FriendViewModel> ListNotFriend { get; set; }
        public List<FriendViewModel> ListMutualFriend { get; set; }

        public ListUserViewModel()
        {
            this.ListUser = new List<UserViewModel>();
            this.ListNotFriend = new List<FriendViewModel>();
            this.ListMutualFriend = new List<FriendViewModel>();
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
        private String searchKeyword = String.Empty;
        public String SearchKeyword
        {
            get
            {
                if (String.IsNullOrEmpty(searchKeyword) || String.IsNullOrWhiteSpace(searchKeyword) || String.Equals(searchKeyword, "null"))
                {
                    return String.Empty;
                }
                else
                {
                    return searchKeyword;
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value) || String.Equals(value, "null"))
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

    public class TrajectorySearchCriteriaViewModel : SearchCriteriaViewModel
    {
        public long UserId { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }
    }
}