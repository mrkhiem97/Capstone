using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MobileSurveillanceWebApplication.Utility
{
    public class CaptchaManager
    {
        /// <summary>
        /// Captcha Height
        /// </summary>
        private const int HEIGHT = 30;

        /// <summary>
        /// Captcha Width
        /// </summary>
        private const int WIDTH = 80;

        /// <summary>
        /// Captcha Length
        /// </summary>
        private const int LENGTH = 4;

        /// <summary>
        /// String which contains Captcha's characters
        /// </summary>
        private const string CAPTCHA_CHARACTERS = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";

        /// <summary>
        /// Get the Byte[] of CaptchaImages
        /// </summary>
        /// <returns></returns>
        public static Byte[] GetByteCaptchaImages(String captchaText)
        {
            var random = new Random();
            var fonts = new[] { "Verdana", "Times New Roman" };
            float orientationAngle = random.Next(0, 359);

            var index0 = random.Next(0, fonts.Length);
            var familyName = fonts[index0];

            using (var bmpOut = new Bitmap(WIDTH, HEIGHT))
            {
                var g = Graphics.FromImage(bmpOut);
                var gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, WIDTH, HEIGHT), Color.White, Color.DarkGray, orientationAngle);
                g.FillRectangle(gradientBrush, 0, 0, WIDTH, HEIGHT);

                //Draw a random line to the Rectangle
                DrawRandomLines(ref g, WIDTH, HEIGHT);

                //Draw captcha string to the Rectangle
                g.DrawString(captchaText, new Font(familyName, 18), new SolidBrush(Color.Gray), 0, 2);

                //Save Bitmap to Memory Stream
                var ms = new MemoryStream();
                bmpOut.Save(ms, ImageFormat.Png);

                //Convert this Memory Stream to Byte
                var bmpBytes = ms.GetBuffer();
                bmpOut.Dispose();
                ms.Close();

                return bmpBytes;
            }
        }

        /// <summary>
        /// Check for valid CaptchaValue
        /// </summary>
        /// <param name="captchaValue">Captcha Value</param>
        /// <param name="expectedHash">Expacted Hascode</param>
        /// <returns>value indicate Valid Captcha</returns>
        public static bool IsValidCaptchaValue(string captchaValue, String expectedHash)
        {
            var hash = ComputeMd5Hash(captchaValue);
            return hash.Equals(expectedHash);
        }

        /// <summary>
        /// Validate user invisible captcha
        /// </summary>
        /// <param name="captchaValue"></param>
        /// <returns></returns>
        public static bool IsValidInvisibleCaptchaValue(string captchaValue)
        {
            return String.IsNullOrEmpty(captchaValue);
        }

        /// <summary>
        /// Drawn a random line on Rectangle captcha
        /// </summary>
        /// <param name="graphic">Graphic</param>
        /// <param name="width">Width of the Rectangle</param>
        /// <param name="height">Height of the Rectangle</param>
        private static void DrawRandomLines(ref Graphics graphic, int width, int height)
        {
            var rnd = new Random();
            var pen = new Pen(Color.Gray);
            for (var i = 0; i < 10; i++)
            {
                graphic.DrawLine(pen, rnd.Next(0, width), rnd.Next(0, height), rnd.Next(0, width), rnd.Next(0, height));
            }
        }

        /// <summary>
        /// Using MD5 to compute the hash code
        /// </summary>
        /// <param name="input">Input value to compute hash code</param>
        /// <returns></returns>
        public static string ComputeMd5Hash(string input)
        {
            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(input);
            HashAlgorithm md5Hasher = MD5.Create();
            return BitConverter.ToString(md5Hasher.ComputeHash(bytes));
        }

        /// <summary>
        /// Generate Random Text by Text Length
        /// </summary>
        /// <param name="textLength">Lenght of Text</param>
        /// <returns>String</returns>
        public static string GenerateRandomText()
        {
            var random = new Random();
            var result = new string(Enumerable.Repeat(CAPTCHA_CHARACTERS, LENGTH).Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }
    }
}