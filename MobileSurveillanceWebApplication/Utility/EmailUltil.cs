using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace MobileSurveillanceWebApplication.Utility
{
    public class EmailUltil
    {
        public static bool SendMail(String addressFrom, String addressTo, String receiverName, String subject, String body, bool isHtmlBody)
        {
            bool retVal = false;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            MailAddress from = new MailAddress(addressFrom, "MobileSurveillance Team");
            MailAddress to = new MailAddress(addressTo, receiverName);
            MailMessage message = new MailMessage(from, to);
            message.Body = body;
            message.Subject = subject;
            message.IsBodyHtml = isHtmlBody;
            NetworkCredential myCredential = new NetworkCredential("mobilesurveillance.group4@gmail.com", "motnamchin", "");
            client.Credentials = myCredential;
            try
            {
                client.Send(message);
                retVal = true;
            }
            catch (ArgumentNullException)
            {
                retVal = false;
            }
            catch (InvalidOperationException)
            {
                retVal = false;
            }
            catch (SmtpException)
            {
                retVal = false;
            }
            catch (Exception)
            {
                retVal = false;
            }
            return retVal;
        }
    }
}