using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MobileSurveillanceWebApplication.Utility
{
    public class KeyUltil
    {
        public static string GenerateNewKey()
        {
            string strPwdchar = "abcdefghijklmnopqrstuvwxyz0123456789#@$ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string strPwd = "";
            Random rnd = new Random();
            for (int i = 0; i <= 8; i++)
            {
                int iRandom = rnd.Next(0, strPwdchar.Length - 1);
                strPwd += strPwdchar.Substring(iRandom, 1);
            }
            return strPwd;
        }

        public static String GenerateHashKey(String initialKey)
        {
            String retVal = String.Empty;
            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(initialKey);
            HashAlgorithm md5Hasher = MD5.Create();
            retVal = BitConverter.ToString(md5Hasher.ComputeHash(bytes));
            return retVal;
        }
    }
}