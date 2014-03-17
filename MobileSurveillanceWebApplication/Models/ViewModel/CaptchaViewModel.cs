using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Web.Mvc;
using System.ComponentModel;

namespace MobileSurveillanceWebApplication.Models.ViewModel
{
    /*
     * Create by LinhNTT
     * Update on 26/03/2013
     */
    /// <summary>
    /// Represent the captcha model
    /// </summary>
    public class CaptchaViewModel
    {
        /// <summary>
        /// Captcha Value String
        /// </summary>
        [Display(Name = "Security Code", Order = 20)]
        [Remote("ValidateCaptcha", "Captcha", "", ErrorMessage = "Invalid security value")]
        [Required(ErrorMessage = "Please type security code exactly")]
        public virtual string CaptchaValue { get; set; }
    }
}