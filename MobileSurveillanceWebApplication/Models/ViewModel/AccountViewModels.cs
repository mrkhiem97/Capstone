﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    public class LocalPasswordViewModel
    {
        [Required]
        [AllowHtml]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }


        [Required]
        [AllowHtml]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [AllowHtml]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [AllowHtml]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [AllowHtml]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        [RegularExpression("^[a-z0-9_-]{1,50}$", ErrorMessage = "Invalid characters. Only a-z 0-9 is allowed")]
        [Remote("ValidateUsername", "Account", ErrorMessage = "This user has already existed")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [AllowHtml]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [AllowHtml]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Remote("ValidateUserEmail", "Account", ErrorMessage = "This user has already existed")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", ErrorMessage = "Invalid email pattern")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Fullname is required")]
        [RegularExpression("^[^-!$@%^&*()_+|~=`{}\\[\\]:\";'<>?,\\/]*$", ErrorMessage = "Invalid name characters")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Fullname")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        [Display(Name = "Birthday")]
        [DataType(DataType.Text)]
        public DateTime Birthday { get; set; }

        [Display(Name = "Address")]
        [AllowHtml]
        [StringLength(1000)]
        [DataType(DataType.Text)]
        public String Address
        {
            get;
            set;
        }

        [Display(Name = "Gender")]
        [Required(ErrorMessage = "Gender is required")]
        public bool Gender
        {
            get;
            set;
        }
    }

    public class RetrievePasswordViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", ErrorMessage = "Invalid email pattern")]
        [Display(Name = "Email")]
        public string Email { get; set; }

    }
    public class ResetPasswordModel
    {
        [Required]
        [AllowHtml]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [AllowHtml]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "New password and confirmation does not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [AllowHtml]
        [DataType(DataType.Text)]
        public string Key { get; set; }
    }

}
