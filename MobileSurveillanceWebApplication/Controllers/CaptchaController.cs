using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using MobileSurveillanceWebApplication.Utility;

namespace MobileSurveillanceWebApplication.Controllers
{
    /// <summary>
    /// This class is fully self-contained. All Captcha related code including
    /// handling of session veriables, hashing, validation etc. is located here.
    /// In order to use this class include the following image tag somwhere in
    /// your view, e.g. in the view handling the registration process:
    /// <img src='/Captcha/DisplayCaptcha' alt="" />
    /// </summary>
    public class CaptchaController : Controller
    {
        /// <summary>
        /// CAPTCHAR Hash code Name
        /// </summary>
        private const String CAPTCHA_HASH = "CaptchaHash";

        /// <summary>
        /// Reponse Html content
        /// </summary>
        private const String HTML_RESPONSE_CONTENT_TYPE = "image/png";

        /// <summary>
        /// Validate user typing captchar, return Json View
        /// </summary>
        /// <param name="captchaValue">User captcha</param>
        /// <returns></returns>
        public JsonResult ValidateCaptcha(string captchaValue)
        {
            //Get HashCode
            String expectedHash = Session[CAPTCHA_HASH].ToString();
            bool validCaptcha = CaptchaManager.IsValidCaptchaValue(captchaValue.ToUpper(), expectedHash);
            if (!validCaptcha)
            {
                return Json("Please give correct captcha value", JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Validate user typing Invisible captchar, return Json View
        /// </summary>
        /// <param name="captchaValue"></param>
        /// <returns></returns>
        public JsonResult ValidateInvisibleCaptcha(string captchaValue)
        {
            bool validCaptcha = CaptchaManager.IsValidInvisibleCaptchaValue(captchaValue);
            if (!validCaptcha)
            {
                return Json("Invalid Invisible Captcha", JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Display captcha image to View
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplayCaptcha()
        {
            //Create captcha Text
            String captchaText = CaptchaManager.GenerateRandomText();
            //Generate Hash code and save to Session
            Session[CAPTCHA_HASH] = CaptchaManager.ComputeMd5Hash(captchaText);

            var bmpBytes = CaptchaManager.GetByteCaptchaImages(captchaText);
            return new FileContentResult(bmpBytes, HTML_RESPONSE_CONTENT_TYPE);
        }
    }
}