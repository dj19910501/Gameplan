using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Configuration;

namespace IntegrationWinService
{

    /// <summary>
    /// Added By: Dharmraj Mangukiya.
    /// Date: 5/26/2014
    /// Enum for custom notification.
    /// </summary>
    public enum Custom_Notification
    {
        IntegrationWindowsService
    }

    class Common
    {
        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="emailid"></param>
        /// <param name="fromemailid"></param>
        /// <param name="strMsg"></param>
        /// <param name="Subject"></param>
        /// <param name="Priority"></param>
        /// <param name="CustomAlias"></param>
        /// <param name="ReplyTo"></param>
        /// <param name="isSupportMail"></param>
        public static int sendMail(string emailid, string fromemailid, string strMsg, string Subject)
        {
            int retval = 0;
            MailMessage objEmail = new MailMessage();

            //string host = System.Configuration.ConfigurationManager.AppSettings["host"].ToString();
            //int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["port"]);
            //string username = System.Configuration.ConfigurationManager.AppSettings["username"].ToString();
            //string password = System.Configuration.ConfigurationManager.AppSettings["password"].ToString();
            bool isSupportEmail = false; // For Integration Email send from Other.

            try
            {
                objEmail.From = new MailAddress(fromemailid);
                objEmail.To.Add(new MailAddress(emailid));
                objEmail.Subject = HttpUtility.HtmlDecode(Subject);
                objEmail.Body = strMsg;
                objEmail.IsBodyHtml = true;
                objEmail.Priority = MailPriority.Normal;

                //Get appropriate SmtpSection for mail sending
                SmtpSection smtpSection = GetSmtpSection(isSupportEmail);
                SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                smtpClient.EnableSsl = smtpSection.Network.EnableSsl;
                smtpClient.Send(objEmail);

                retval = 1;
                return retval;
            }
            catch (Exception)
            {
                retval = 0;
                return retval;
            }
            finally
            {
                objEmail.Dispose();
            }
        }

        /// <summary>
        /// To get appropriate SmtpSection for mail sending
        /// </summary>
        /// <param name="isSupportMail"></param>
        /// <returns>SmtpSection</returns>
        public static SmtpSection GetSmtpSection(bool isSupportMail)
        {
            SmtpSection smtpSection = new SmtpSection();
            if (isSupportMail)
            {
                smtpSection = (SmtpSection)System.Configuration.ConfigurationManager.GetSection("mailSettings/smtp_support");
            }
            else
            {
                smtpSection = (SmtpSection)System.Configuration.ConfigurationManager.GetSection("mailSettings/smtp_other");
            }
            return smtpSection;
        }

    }
}
