using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Transactions;
using System.Data;
using System.Text.RegularExpressions;
using Integration;

namespace RevenuePlanner.Helpers
{
    public class Common
    {
        #region Declarations

        public const string InvalidCharactersForEmail = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string InvalidCharactersForAnswer = "^[^~^|]+$";
        public const string InvalidCharactersForAnswerMsg = "~|^ ";
        public const string InvalidCharactersForEntryMode = "^[^<>~%^;/|]+$";
        public const string InvalidCharactersForEntryModeEmail = "^[^<>~%^;|]+$";
        public const string InvalidCharactersForEntryModeMsg = "<>~%;/|^ ";
        public const string InvalidCharactersForEntryModeEmailMsg = "<>~%;|^ ";
        public const string InvalidCharactersForSearchMode = "^[^\"()<>%_~&;/|^]+$";
        public const string InvalidCharactersForSearchModeMsg = "<>~%^&()/|\"";
        public const string InvalidCharactersForAddress = "^[^<>~@%^;/|]+$";
        public const string InvalidCharactersForAddressMsg = "<>~@%^;/|";
        public const string InvalidCharactersForMeta = "^[^<>~%^]+$";
        public const string InvalidCharactersForMetaMsg = "<>~%^ ";
        public const string BRK = "";
        public static readonly string strSMTPServer = System.Configuration.ConfigurationManager.AppSettings.Get("SMTPServerIP"); //"192.168.100.225"
        public static readonly string APIAcessKey = System.Configuration.ConfigurationManager.AppSettings.Get("APIAcessKey"); //"9F42A936-EAD0-49BA-B3D5-4992120F76EB"
        public static readonly string FromMail = System.Configuration.ConfigurationManager.AppSettings.Get("FromMail"); //"192.168.100.225"
        public static readonly string SupportMail = System.Configuration.ConfigurationManager.AppSettings.Get("SupportEmail"); // email address of recipient of support mails
        public static readonly string FromAlias = System.Configuration.ConfigurationManager.AppSettings.Get("FromAlias"); //"192.168.100.225"
        public static readonly string EvoKey = System.Configuration.ConfigurationManager.AppSettings.Get("EvoKey");
        public static readonly string FromSupportMail = System.Configuration.ConfigurationManager.AppSettings.Get("FromSupportMail");
        public static readonly bool IsOffline = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsOffline"]); ////Added by :- Pratik Chauhan on 22/09/2014 for PL ticket #468 to display maintenance page

        public const string OptionTextRegex = "^[^<>]+";
        public const string MessageForOptionTextRegex = "<> characters are not allowed";


        public static Message objCached = new Message();
        //public static string xmlMsgFilePath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLCommonMsgFilePath"));
        //public static string xmlBenchmarkFilePath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLBenchmarkFilePath"));

        public static string xmlMsgFilePath = HttpContext.Current.Request.ApplicationPath == null ? string.Empty : HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLCommonMsgFilePath"));
        public static string xmlBenchmarkFilePath = HttpContext.Current.Request.ApplicationPath == null ? string.Empty : HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLBenchmarkFilePath"));


        public static readonly int imgWidth = 50;
        public static readonly int imgHeight = 50;

        public const string GANTT_BAR_CSS_CLASS_PREFIX = "color";

        ////Modified By Maninder Singh Wadhva PL Ticket#47, 337
        public const string GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT = "improvementcolor";
        public const string COLORC6EBF3_WITH_BORDER = "colorC6EBF3-with-border";
        ////Modified By Maninder Singh Wadhva PL Ticket#47, 337
        public const string COLORC6EBF3_WITH_BORDER_IMPROVEMENT = "improvementcolorC6EBF3-with-border";
        public const string COLOR27A4E5 = "color27a4e5";

        ////Modified By Pratik PL Ticket#810
        public const string DateFormat = "MM/dd/yyyy";
        public const string DateFormatDatePicker = "mm/dd/yyyy";

        //Added BY Bhavesh
        /// <summary>
        /// Modified By Maninder Singh Wadhva for TFS Bug#300
        /// </summary>
        public const string maxLengthDollar = "13";
        public const string maxLengthPriceValue = "12";
        public const string maxLengthValue = "10";
        public const string maxLengthPercentageValue = "5";
        public const string copySuffix = "_Copy";

        /*----------------------- Temp variables juned ---------------------------- */

        private static string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
        private static string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
        private static string PASSWORD_CHARS_NUMERIC = "23456789";
        private static string PASSWORD_CHARS_SPECIAL = "*$-+?_&=!{}";

        /*----------------------- Temp section ends here -------------------------- */

        //----------------------- Constants for Report Conversion Summary ----------------------//
        public const string Actuals = "Actuals";
        public const string Plan = "Plan";
        public const string Trend = "Trend";

        // Constants for Parent tab
        public const string BusinessUnit = "Business Unit";
        public const string Audience = "Audience";
        public const string Geography = "Geography";
        public const string Vertical = "Vertical";

        /*-------------#region ReportRevenue---------------*/
        public static string RevenueBusinessUnit = "Business Unit";
        public static string RevenueGeography = "Geography";
        public static string RevenuePlans = "Plans";
        public static string RevenueCampaign = "Campaign";
        public static string RevenueProgram = "Program";
        public static string RevenueTactic = "Tactic";
        public static string RevenueAudience = "Audience";
        public static string RevenueVertical = "Vertical";
        public static string RevenueOrganization = "Organization";

        public static string SourcePerformanceActual = "Actual";
        public static string SourcePerformancePlan = "Plan";
        public static string SourcePerformanceTrend = "Trend";

        //Suffix for Plan Improvement - Added By Kuber 
        public const string defaultImprovementSuffix = "_Imp";

        // Default Improvement campaign Title
        public static string ImprovementActivities = "Improvement Activities";
        public static string ImprovementProgram = "Improvement Program";

        // Default Other line item title
        public static string DefaultLineItemTitle = "Other";

        // Label text for Unallocated Budget label
        public static string UnallocatedBudgetLabelText = "Unallocated";

        //Added by Mitesh Vaishnav for PL ticket #659 - static values for Model integrationinstance,integrationInstancetype ot Last sync are null. 2)dateformate for Last sync values
        public static string TextForModelIntegrationInstanceNull = "None";
        public static string TextForModelIntegrationInstanceTypeOrLastSyncNull = "---";
        public static string DateFormatForModelIntegrationLastSync = "MM/dd/yyyy hh:mm tt";
        private const string GameplanIntegrationService = "Gameplan Integration Service";
        public static string DateFormateForInspectPopupDescription = "MMMM dd";

        //Added By Kalpesh Sharma
        public const string CustomTitle = "Custom";
        public const string CampaignCustomTitle = "CampaignCustom";
        public const string ProgramCustomTitle = "ProgramCustom";
        public const string TacticCustomTitle = "TacticCustom";

        //Added By Sohel Pathan
        public static string ColorCodeForCustomField = "";

        public static string RedirectOnServiceUnavailibilityPage = "~/Login/ServiceUnavailable";
        public static string RedirectOnDBServiceUnavailibilityPage = "~/Login/DBServiceUnavailable";
        #endregion

        #region Functions for File and IO Handling

        /// <summary>
        /// Read Template
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string ReadTemplate(string FilePath)
        {
            string tempReadTemplate = null;

            tempReadTemplate = "";
            if (System.IO.File.Exists(FilePath))
            {
                StreamReader objStreamReader = null;
                objStreamReader = File.OpenText(FilePath);
                string contents = objStreamReader.ReadToEnd().ToString();
                objStreamReader.Close();
                tempReadTemplate = contents;
            }
            return tempReadTemplate;
        }

        /// <summary>
        /// Create a folder
        /// </summary>
        /// <param name="FolderPath"></param>
        public static void CreateFolder(string FolderPath)
        {
            if (System.IO.Directory.Exists(FolderPath) == false)
            {
                System.IO.Directory.CreateDirectory(FolderPath);
            }
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static byte[] ReadFile(string sPath)
        {
            //Initialize byte array with a null value initially.
            byte[] data = null;

            //Use FileInfo object to get file size.
            FileInfo fInfo = new FileInfo(sPath);
            long numBytes = fInfo.Length;

            //Open FileStream to read file
            FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fStream);
            data = br.ReadBytes((int)numBytes);

            return data;
        }

        /// <summary>
        /// Convert to byte array
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Byte[] ConvertToByteArray(HttpPostedFileBase source)
        {
            Byte[] destination = new Byte[source.ContentLength];
            source.InputStream.Position = 0;
            source.InputStream.Read(destination, 0, source.ContentLength);
            return destination;
        }

        /// <summary>
        /// convert image into bytes 
        /// </summary>
        /// <param name="imageIn"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }

        }

        /// <summary>
        /// Function to convert image into string
        /// </summary>
        /// <param name="filestream"></param>
        /// <returns></returns>
        public static string ImageToString(Stream filestream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(filestream);
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();
                string ImageString = Convert.ToBase64String(imageBytes);
                ms.Flush();
                return ImageString;
            }
        }

        /// <summary>
        /// Resize image by preserving aspect ratio
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="preserveAspectRatio"></param>
        /// <param name="preventEnlarge"></param>
        /// <returns></returns>
        public static Image ImageResize(Image image, int width, int height, bool preserveAspectRatio = false, bool preventEnlarge = false)
        {
            if (preserveAspectRatio)
            {
                double num3 = (height * 100.0) / ((double)image.Height);
                double num4 = (width * 100.0) / ((double)image.Width);
                if (num3 > num4)
                {
                    height = (int)Math.Round((double)((num4 * image.Height) / 100.0));
                }
                else if (num3 < num4)
                {
                    width = (int)Math.Round((double)((num3 * image.Width) / 100.0));
                }
            }
            if (preventEnlarge)
            {
                if (height > image.Height)
                {
                    height = image.Height;
                }
                if (width > image.Width)
                {
                    width = image.Width;
                }
            }
            if ((image.Height == height) && (image.Width == width))
            {
                return image;
            }
            bool flag = (((image.PixelFormat == PixelFormat.Format1bppIndexed) || (image.PixelFormat == PixelFormat.Format4bppIndexed)) || (image.PixelFormat == PixelFormat.Format8bppIndexed)) || (image.PixelFormat == PixelFormat.Indexed);
            Bitmap bitmap = flag ? new Bitmap(width, height) : new Bitmap(width, height, image.PixelFormat);
            if (preserveAspectRatio)
            {
                bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }
            else
            {
                bitmap.SetResolution(96f, 96f);
            }
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(Brushes.Transparent, 0, 0, width, height);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, width, height);
            }

            return bitmap;
        }

        #endregion

        #region Mail Send

        /// <summary>
        /// Send an email
        /// Modified By : Kalpesh Sharma
        /// </summary>
        /// <param name="emailid"></param>
        /// <param name="fromemailid"></param>
        /// <param name="strMsg"></param>
        /// <param name="Subject"></param>
        /// <param name="Priority"></param>
        /// <param name="CustomAlias"></param>
        /// <param name="ReplyTo"></param>
        /// <param name="isSupportMail"></param>
        public static int sendMail(string emailid, string fromemailid, string strMsg, string Subject, string Priority, string CustomAlias = "", string ReplyTo = "", bool isSupportMail = false, AlternateView htmltextview = null)
        {
            int retval = 0;
            MailMessage objEmail = new MailMessage();
            try
            {
                objEmail.From = new MailAddress(fromemailid);
                // Add alias
                if (!string.IsNullOrEmpty(FromAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, FromAlias);
                }
                else if (!string.IsNullOrEmpty(CustomAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, CustomAlias);
                }
                objEmail.To.Add(new MailAddress(emailid));
                objEmail.Subject = HttpUtility.HtmlDecode(Subject);
                objEmail.Body = strMsg;
                objEmail.IsBodyHtml = true;
                if (ReplyTo != "")
                    objEmail.ReplyToList.Add(new MailAddress(ReplyTo));
                objEmail.Priority = MailPriority.Normal;

                // Modified BY : Kalpesh Sharma
                // #453: Support request Issue field needs to be bigger
                // Send image in Email template 
                if (htmltextview != null && htmltextview.LinkedResources.Count > 0)//null cond added by uday since this parameter is utilized only for support mail but mail sending method is utilized all over application
                {
                    objEmail.AlternateViews.Add(htmltextview);
                }

                //Get appropriate SmtpSection for mail sending
                SmtpSection smtpSection = GetSmtpSection(isSupportMail);
                SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                smtpClient.EnableSsl = smtpSection.Network.EnableSsl;
                smtpClient.Send(objEmail);
                retval = 1;
                return retval;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
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
                smtpSection = (SmtpSection)ConfigurationManager.GetSection("mailSettings/smtp_support");
            }
            else
            {
                smtpSection = (SmtpSection)ConfigurationManager.GetSection("mailSettings/smtp_other");
            }
            return smtpSection;
        }

        /// <summary>
        /// Send email to multiple users
        /// </summary>
        /// <param name="emailid"></param>
        /// <param name="fromemailid"></param>
        /// <param name="strMsg"></param>
        /// <param name="Subject"></param>
        /// <param name="Priority"></param>
        /// <param name="CustomAlias"></param>
        /// <param name="isSupportMail"></param>
        public static void SendMailToMultipleUser(string emailidlist, string fromemailid, string strMsg, string Subject, string Priority, string CustomAlias = "", bool isSupportMail = false)
        {
            MailMessage objEmail = new MailMessage();
            try
            {
                objEmail.From = new MailAddress(fromemailid);
                // Add alias
                if (!string.IsNullOrEmpty(FromAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, FromAlias);
                }
                else if (!string.IsNullOrEmpty(CustomAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, CustomAlias);
                }
                objEmail.To.Add(new MailAddress(emailidlist));
                objEmail.Subject = HttpUtility.HtmlDecode(Subject);
                objEmail.Body = strMsg;
                objEmail.IsBodyHtml = true;
                objEmail.Priority = MailPriority.Normal;

                //Get appropriate SmtpSection for mail sending
                SmtpSection smtpSection = GetSmtpSection(isSupportMail);
                SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                smtpClient.EnableSsl = smtpSection.Network.EnableSsl;
                smtpClient.Send(objEmail);
            }
            catch (Exception ex)
            {
                //// Changed By: Maninder Singh Wadhva to avoid argument null exception.
                Elmah.ErrorLog.GetDefault(null).Log(new Error(ex));
            }
            finally
            {
                objEmail.Dispose();
            }
        }

        /// <summary>
        /// Send an email with attachment
        /// </summary>
        /// <param name="emailid"></param>
        /// <param name="fromemailid"></param>
        /// <param name="strMsg"></param>
        /// <param name="Subject"></param>
        /// <param name="attachment"></param>
        /// <param name="attachmentName"></param>
        /// <param name="isSupportMail"></param>
        public static void sendMail(string emailid, string fromemailid, string strMsg, string Subject, Stream attachment, string attachmentName, string CustomAlias = "", bool isSupportMail = false)
        {
            MailMessage objEmail = new MailMessage();
            try
            {
                objEmail.From = new MailAddress(fromemailid);
                // Add alias
                if (!string.IsNullOrEmpty(FromAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, FromAlias);
                }
                else if (!string.IsNullOrEmpty(CustomAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, CustomAlias);
                }

                objEmail.To.Add(new MailAddress(emailid));
                objEmail.Subject = HttpUtility.HtmlDecode(Subject);
                objEmail.Body = strMsg;
                objEmail.IsBodyHtml = true;
                objEmail.Priority = MailPriority.Normal;
                Attachment att = new Attachment(attachment, attachmentName, "application/pdf");
                objEmail.Attachments.Add(att);

                //Get appropriate SmtpSection for mail sending
                SmtpSection smtpSection = GetSmtpSection(isSupportMail);
                SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                smtpClient.EnableSsl = smtpSection.Network.EnableSsl;
                smtpClient.Send(objEmail);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            finally
            {
                objEmail.Dispose();
            }
        }
        // Section parameter added to decide to send SendNotificationMail for Tactic, Program or Campaign Section
        public static void SendNotificationMail(List<string> EmailIds, List<string> CollaboratorUserName, string TacticName, string PlanName, string Action, string Comment = "", string Section = "", int planTacticId = 0, int planId = 0, string URL = "")
        {
            for (int i = 0; i <= EmailIds.Count - 1; i++)
            {
                string emailBody = "";
                MRPEntities db = new MRPEntities();
                Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(Action));
                if (Section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", CollaboratorUserName.ElementAt(i)).Replace("[TacticNameToBeReplaced]", TacticName).Replace("[PlanNameToBeReplaced]", PlanName).Replace("[UserNameToBeReplaced]", Sessions.User.FirstName + " " + Sessions.User.LastName).Replace("[CommentToBeReplaced]", Comment);
                    emailBody = emailBody.Replace("[URL]", URL);
                }
                else if (Section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", CollaboratorUserName.ElementAt(i)).Replace("[ProgramNameToBeReplaced]", TacticName).Replace("[PlanNameToBeReplaced]", PlanName).Replace("[UserNameToBeReplaced]", Sessions.User.FirstName + " " + Sessions.User.LastName).Replace("[CommentToBeReplaced]", Comment);
                    emailBody = emailBody.Replace("[URL]", URL);
                }
                else if (Section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", CollaboratorUserName.ElementAt(i)).Replace("[CampaignNameToBeReplaced]", TacticName).Replace("[PlanNameToBeReplaced]", PlanName).Replace("[UserNameToBeReplaced]", Sessions.User.FirstName + " " + Sessions.User.LastName).Replace("[CommentToBeReplaced]", Comment);
                    emailBody = emailBody.Replace("[URL]", URL);
                }
                else if (Section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    ////Modified By Maninder Singh Wadhva PL Ticket#47
                    emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", CollaboratorUserName.ElementAt(i)).Replace("[ImprovementTacticNameToBeReplaced]", TacticName).Replace("[PlanNameToBeReplaced]", PlanName).Replace("[UserNameToBeReplaced]", Sessions.User.FirstName + " " + Sessions.User.LastName).Replace("[CommentToBeReplaced]", Comment);
                    emailBody = emailBody.Replace("[URL]", URL);
                }
                string email = EmailIds.ElementAt(i);
                string Username = CollaboratorUserName.ElementAt(i);
                //Common.SendMailToMultipleUser(EmailIds, Common.FromMail, emailBody, notification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High));
                ThreadStart threadStart = delegate() { Common.SendMailToMultipleUser(email, Common.FromMail, emailBody, notification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High)); };
                Thread thread = new Thread(threadStart);
                thread.Start();
            }
        }

        /// <summary>
        /// Send email to new owner when tactic owner changed.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>14/11/2014</CreatedDate>
        /// <param name="EmailIds">Email id to whom email to be sent</param>
        /// <param name="NewOwnerName">New tactic owmner name</param>
        /// <param name="ModifierName">Name of user who changed the tactic owner</param>
        /// <param name="TacticName">Tactic name for which owner has been changed</param>
        /// <param name="ProgramName">Program name of tactic for which owner has been changed</param>
        /// <param name="CampaignName">Campaign name of tactic for which owner has been changed</param>
        /// <param name="PlanName">Plan name of tactic for which owner has been changed</param>
        public static void SendNotificationMailForTacticOwnerChanged(List<string> EmailIds, string NewOwnerName, string ModifierName, string TacticName, string ProgramName, string CampaignName, string PlanName, string URL = "")
        {
            for (int i = 0; i <= EmailIds.Count - 1; i++)
            {
                string emailBody = "";
                MRPEntities db = new MRPEntities();
                string TacticOwnerChanged = Enums.Custom_Notification.TacticOwnerChanged.ToString();
                Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(TacticOwnerChanged));
                emailBody = notification.EmailContent;
                emailBody = emailBody.Replace("[NameToBeReplaced]", NewOwnerName);
                emailBody = emailBody.Replace("[ModifierName]", ModifierName);
                emailBody = emailBody.Replace("[tacticname]", TacticName);
                emailBody = emailBody.Replace("[programname]", ProgramName);
                emailBody = emailBody.Replace("[campaignname]", CampaignName);
                emailBody = emailBody.Replace("[planname]", PlanName);
                emailBody = emailBody.Replace("[URL]", URL);

                string email = EmailIds.ElementAt(i);
                ThreadStart threadStart = delegate() { Common.SendMailToMultipleUser(email, Common.FromMail, emailBody, notification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High)); };
                Thread thread = new Thread(threadStart);
                thread.Start();
            }
        }


        public static List<string> GetCollaboratorForTactic(int PlanTacticId)
        {
            MRPEntities db = new MRPEntities();
            List<string> collaboratorId = new List<string>();
            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == PlanTacticId);
            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();
            var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.PlanTacticId == PlanTacticId);
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();
            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
        }

        public static List<string> GetCollaboratorForImprovementTactic(int ImprovementPlanTacticId)
        {
            MRPEntities db = new MRPEntities();
            List<string> collaboratorId = new List<string>();
            var planTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId);
            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();
            var planTacticComment = db.Plan_Improvement_Campaign_Program_Tactic_Comment.Where(pc => pc.ImprovementPlanTacticId == ImprovementPlanTacticId);
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();
            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
        }

        public static List<string> GetCollaboratorForProgram(int PlanProgramId)
        {
            MRPEntities db = new MRPEntities();
            List<string> collaboratorId = new List<string>();
            var planProgram = db.Plan_Campaign_Program.Where(t => t.PlanProgramId == PlanProgramId);
            var planProgramModifiedBy = planProgram.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planProgramCreatedBy = planProgram.ToList().Select(t => t.CreatedBy.ToString()).ToList();
            var planProgramComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.PlanProgramId == PlanProgramId);
            var planProgramCommentCreatedBy = planProgramComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();
            collaboratorId.AddRange(planProgramCreatedBy);
            collaboratorId.AddRange(planProgramModifiedBy);
            collaboratorId.AddRange(planProgramCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
        }

        public static List<string> GetCollaboratorForCampaign(int PlanCampaignId)
        {
            MRPEntities db = new MRPEntities();
            List<string> collaboratorId = new List<string>();
            var planCampaign = db.Plan_Campaign.Where(t => t.PlanCampaignId == PlanCampaignId);
            var planCampaignModifiedBy = planCampaign.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planCampaignCreatedBy = planCampaign.ToList().Select(t => t.CreatedBy.ToString()).ToList();
            var planCampaignComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.PlanCampaignId == PlanCampaignId);
            var planCampaignCommentCreatedBy = planCampaignComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();
            collaboratorId.AddRange(planCampaignCreatedBy);
            collaboratorId.AddRange(planCampaignModifiedBy);
            collaboratorId.AddRange(planCampaignCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
        }

        public static void mailSendForTactic(int planTacticId, string status, string title, bool iscomment = false, string comment = "", string section = "", string URL = "")
        {
            BDSServiceClient service = new BDSServiceClient();
            BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
            MRPEntities db = new MRPEntities();
            List<string> lst_CollaboratorEmail = new List<string>();
            List<string> lst_CollaboratorUserName = new List<string>();
            //List<string> lst_CollaboratorId = GetCollaboratorForTactic(planTacticId);
            List<string> lst_CollaboratorId = new List<string>();
            Guid createdBy = new Guid();
            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
            {
                lst_CollaboratorId = GetCollaboratorForTactic(planTacticId);
            }
            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
            {
                lst_CollaboratorId = GetCollaboratorForProgram(planTacticId);
            }
            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                lst_CollaboratorId = GetCollaboratorForCampaign(planTacticId);
            }
            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
            {
                lst_CollaboratorId = GetCollaboratorForImprovementTactic(planTacticId);
            }
            if (lst_CollaboratorId.Count > 0)
            {
                var csv = string.Join(", ", lst_CollaboratorId);
                var UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                lst_CollaboratorEmail = UsersDetails.Select(u => u.Email).ToList();
                lst_CollaboratorUserName = UsersDetails.Select(u => u.FirstName).ToList();
                //var PlanName = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Title).SingleOrDefault();
                var PlanName = "";
                int PlanId = 0;
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    PlanName = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Title).SingleOrDefault();
                    PlanId = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId).SingleOrDefault();
                    createdBy = db.Plan_Campaign_Program_Tactic.FirstOrDefault(pcpt => pcpt.PlanTacticId == planTacticId).CreatedBy;
                }
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    PlanName = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == planTacticId).Select(pcp => pcp.Plan_Campaign.Plan.Title).SingleOrDefault();
                    createdBy = db.Plan_Campaign_Program.FirstOrDefault(pcpt => pcpt.PlanProgramId == planTacticId).CreatedBy;
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    PlanName = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == planTacticId).Select(pc => pc.Plan.Title).SingleOrDefault();
                    createdBy = db.Plan_Campaign.FirstOrDefault(pcpt => pcpt.PlanCampaignId == planTacticId).CreatedBy;
                }
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    PlanName = db.Plan_Improvement_Campaign_Program_Tactic.Where(pc => pc.ImprovementPlanTacticId == planTacticId).Select(pc => pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Title).SingleOrDefault();
                    createdBy = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(pcpt => pcpt.ImprovementPlanTacticId == planTacticId).CreatedBy;
                }
                if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                {
                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticApproved.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramApproved.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignApproved.ToString(), "", Convert.ToString(Enums.Section.Campaign).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticApproved.ToString(), "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                {

                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticDeclined.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramDeclined.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignDeclined.ToString(), "", Convert.ToString(Enums.Section.Campaign).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticDeclined.ToString(), "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString()))
                {
                    var clientId = Sessions.User.ClientId;
                    //var directorRoleCode = Enums.RoleCodes.D.ToString();
                    //var lst_user = objBDSUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
                    //var lst_director = lst_user.Where(ld => ld.RoleCode.Equals(directorRoleCode)).Select(l => l).ToList();
                    //foreach (var item in lst_director)
                    //{
                    //    lst_CollaboratorEmail.Add(item.Email);
                    //    lst_CollaboratorUserName.Add(item.FirstName);
                    //}
                    // To add manager's email address, By dharmraj, Ticket #537
                    var lstUserHierarchy = objBDSUserRepository.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                    var objOwnerUser = lstUserHierarchy.FirstOrDefault(u => u.UserId == createdBy);
                    if (objOwnerUser.ManagerId != null)
                    {
                        lst_CollaboratorEmail.Add(lstUserHierarchy.FirstOrDefault(u => u.UserId == objOwnerUser.ManagerId).Email);
                        lst_CollaboratorUserName.Add(lstUserHierarchy.FirstOrDefault(u => u.UserId == objOwnerUser.ManagerId).FirstName);
                    }

                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticSubmitted.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramSubmitted.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignSubmitted.ToString(), "", Convert.ToString(Enums.Section.Campaign).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticSubmitted.ToString(), "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.TacticCommentAdded.ToString()) && iscomment)
                {
                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Tactic).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.ProgramCommentAdded.ToString()) && iscomment)
                {
                    if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Program).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.CampaignCommentAdded.ToString()) && iscomment)
                {
                    if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Campaign).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString()) && iscomment)
                {
                    if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), planTacticId, PlanId, URL);
                    }
                }
            }
        }

        #endregion

        #region Cookies Related functions

        /// <summary>
        /// Set Browser cookie for specified cookie name 
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        public static void SetCookie(string cookieName, string cookieValue, bool isForDataInconsistency = false)
        {
            HttpCookie objCookie = null;
            objCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (objCookie != null)
            {
                if (!isForDataInconsistency)
                {
                    objCookie.Value = cookieValue;
                    objCookie.Expires = System.Convert.ToDateTime(System.DateTime.Now.Date).AddMonths(6);
                    objCookie.Path = "/";
                    HttpContext.Current.Response.Cookies.Add(objCookie);
                }
            }
            else
            {
                objCookie = new HttpCookie(cookieName);
                objCookie.Value = cookieValue;
                objCookie.Expires = System.Convert.ToDateTime(System.DateTime.Now.Date).AddMonths(6);
                objCookie.Path = "/";
                HttpContext.Current.Response.Cookies.Add(objCookie);
            }
        }

        /// <summary>
        /// Remove Browser cookie for specified cookie name 
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        public static void RemoveCookie(string cookieName)
        {
            HttpCookie objCookie = null;
            objCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (objCookie != null)
            {
                objCookie.Expires = DateTime.Now.AddDays(-1d);
                HttpContext.Current.Response.Cookies.Add(objCookie);
            }
        }

        #endregion

        #region Encrypt Decrypt Functions

        /// <summary>
        /// Encrypt Query String
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string EncryptQueryString(string strInput)
        {
            string tempEncryptQueryString = null;
            if (strInput.Trim() != "")
            {
                strInput = Encrypt(strInput.Trim());
                tempEncryptQueryString = HttpContext.Current.Server.UrlEncode(strInput);
            }
            else
            {
                tempEncryptQueryString = strInput.Trim();
            }
            return tempEncryptQueryString;
        }

        /// <summary>
        /// Decrypt Query String
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string DecryptQueryString(string strInput)
        {
            string tempDecryptQueryString = null;
            if (strInput.Trim() != "")
            {
                tempDecryptQueryString = Decrypt(strInput);
            }
            else
            {
                tempDecryptQueryString = strInput.Trim();
            }
            return tempDecryptQueryString;
        }

        /// <summary>
        /// Encrypt string
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string Encrypt(string strText)
        {
            byte[] byKey = null;
            byte[] IV = { 0X12, 0X34, 0X56, 0X78, 0X90, 0XAB, 0XCD, 0XEF };

            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(((string)("&%#@?,:*")).Substring(0, 8));

                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// Decrypt string
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string Decrypt(string strText)
        {
            byte[] byKey = null;
            byte[] IV = { 0X12, 0X34, 0X56, 0X78, 0X90, 0XAB, 0XCD, 0XEF };
            byte[] inputByteArray = new byte[strText.Length + 1];

            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(((string)("&%#@?,:*")).Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);

                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;

                return encoding.GetString(ms.ToArray());

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #region "Function for Enum and Values"
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/26/2013
        /// Function to get key of corresponding value using dictionary.
        /// </summary>
        /// <typeparam name="T">Type of Enum.</typeparam>
        /// <param name="dictionary">Dictionary to which value belong.</param>
        /// <param name="value">Value for which enum of type T is to be returned.</param>
        /// <returns>Enum of type T.</returns>
        public static T GetKey<T>(Dictionary<string, string> dictionary, string value)
        {
            var key = (from KeyValue in dictionary
                       where KeyValue.Value.ToUpper().Equals(value.ToUpper())
                       select KeyValue.Key.ToUpper()).FirstOrDefault();

            return (T)Enum.Parse(typeof(T), key, true);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 06/18/2014
        /// Function to get key based on string.
        /// </summary>
        /// <typeparam name="T">Enum Type.</typeparam>
        /// <param name="value">Value i.e. ToString() of enum key.</param>
        /// <returns>Returns appropriate enum key.</returns>
        public static T GetKey<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        #endregion

        #region Authorization
        /// <summary>
        /// Returns the permission code 
        /// </summary>
        /// <param name="actionItem"></param>
        /// <returns></returns>
        //public static Permission GetPermission(string actionItem)
        //{
        //    //if (Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
        //    //{
        //    //    return Permission.FullAccess;
        //    //}
        //    if (Sessions.RolePermission != null)
        //    {
        //        int PerCode = Sessions.RolePermission.Where(r => r.Code == actionItem).Select(r => r.PermissionCode).SingleOrDefault();
        //        switch (PerCode)
        //        {
        //            case 0:
        //                return Permission.NoAccess;
        //            case 1:
        //                return Permission.ViewOnly;

        //            case 2:
        //                return Permission.FullAccess;

        //            default:
        //                return Permission.NoAccess;
        //        }
        //    }
        //    return Permission.FullAccess;
        //}
        #endregion

        #region GeneralFunction

        public static string GetTimeStamp()
        {
            string timestamp = "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
            return timestamp;
        }

        #endregion

        #region Code for hashing password
        /*-------------------- Code for hash ----------------------*/

        public static string ComputeSingleHash(string SimpleText)
        {
            // Convert plain text into a byte array. (here plain text would be the single hash text)
            byte[] SingleHashTextBytes = Encoding.UTF8.GetBytes(SimpleText);

            // We're using SHA512 algorithm for hashing purpose
            HashAlgorithm hash = new SHA512Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] SinglehashBytes = hash.ComputeHash(SingleHashTextBytes);

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(SinglehashBytes);

            // Return the result.
            return hashValue;
        }

        public static byte[] GetSaltBytes()
        {
            string saltchars = "SaLt"; //fixed salt characters
            byte[] saltbytes = Encoding.UTF8.GetBytes(saltchars);
            return saltbytes;
        }

        /* ------------------------------------------------------*/
        #endregion

        #region Plan
        public static List<Plan> GetPlan(bool isFromReport = false)
        {
            MRPEntities db = new MRPEntities();
            List<Guid> businessUnitIds = new List<Guid>();
            ////start - Added by Mitesh Vaishnav for internal review point 91
            //If functional call from report section than user allowed for all business unit Id
            if (isFromReport)
            {
                businessUnitIds.Clear();
                db.BusinessUnits.Where(s => s.ClientId == Sessions.User.ClientId && s.IsDeleted == false).ToList().ForEach(s => businessUnitIds.Add(s.BusinessUnitId));
            } ////End - Added by Mitesh Vaishnav for internal review point 91
            else
            {
                var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
                if (lstAllowedBusinessUnits.Count > 0)
                    lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));

                // Modified by Dharmraj, For #537
                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnits.Count == 0)//if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
                {
                    //// Getting all business unit for client of director.
                    var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
                    businessUnitIds = clientBusinessUnit.ToList();
                }
                else
                {
                    // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    if (lstAllowedBusinessUnits.Count > 0)
                    {
                        lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                    }
                    else
                    {
                        businessUnitIds.Add(Sessions.User.BusinessUnitId);
                    }
                    // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                }
            }

            //// Getting active model of above business unit. 
            string modelPublishedStatus = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
            var models = db.Models.Where(m => businessUnitIds.Contains(m.BusinessUnitId) && m.IsDeleted == false).Select(m => m);

            //// Getting modelIds
            var modelIds = models.Select(m => m.ModelId).ToList();

            var activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).Select(p => p);
            return activePlan.ToList();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get plan gantt start and end date as per current date.
        /// </summary>
        /// <param name="planYear">Plan year.</param>
        /// <param name="currentView">Current view of calendar.</param>
        /// <param name="startDate">Start Date of calendar.</param>
        /// <param name="endDate">End date of calendar.</param>
        public static void GetPlanGanttStartEndDate(string planYear, string currentView, ref DateTime startDate, ref DateTime endDate)
        {
            int year;
            bool isNumeric = int.TryParse(currentView, out year);

            if (currentView == Enums.UpcomingActivities.thisyear.ToString())
            {
                //// Plan
                startDate = new DateTime(System.DateTime.Now.Year, 1, 1);
                endDate = new DateTime(System.DateTime.Now.Year + 1, 1, 1).AddTicks(-1);
            }
            else if (currentView == Enums.UpcomingActivities.thisquarter.ToString())
            {
                int currentQuarter = ((startDate.Month - 1) / 3) + 1;
                startDate = new DateTime(startDate.Year, (currentQuarter - 1) * 3 + 1, 1);
                endDate = startDate.AddMonths(3).AddTicks(-1);
            }
            else if (currentView == Enums.UpcomingActivities.thismonth.ToString())
            {
                //// Plan
                int month = System.DateTime.Now.Month;
                startDate = new DateTime(startDate.Year, month, 1);
                endDate = startDate.AddMonths(month).AddTicks(-1);
            }
            else if (currentView == Enums.UpcomingActivities.nextyear.ToString())
            {
                //// Plan
                startDate = new DateTime(System.DateTime.Now.Year + 1, 1, 1);
                endDate = new DateTime(System.DateTime.Now.Year + 2, 1, 1).AddTicks(-1);
            }
            else if (currentView == Enums.UpcomingActivities.planYear.ToString())
            {
                //// Plan
                startDate = new DateTime(Convert.ToInt32(planYear), 1, 1);
                endDate = new DateTime(Convert.ToInt32(planYear) + 1, 1, 1).AddTicks(-1);
            }
            else if (isNumeric)
            {
                startDate = new DateTime(Convert.ToInt32(planYear), 1, 1);
                endDate = new DateTime(Convert.ToInt32(planYear) + 1, 1, 1).AddTicks(-1);
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to Check Both Start and End Date is outside Calendar.
        /// </summary>
        /// <param name="calendarStartDate">Calendar Start Date.</param>
        /// <param name="calendarEndDate">Calendar End Date.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End Date.</param>
        /// <returns>Return flag to indicate whether Both Start and End Date is outside Calendar.</returns>
        public static bool CheckBothStartEndDateOutSideCalendar(DateTime calendarStartDate, DateTime calendarEndDate, DateTime startDate, DateTime endDate)
        {
            return (endDate < calendarStartDate) || (startDate > calendarEndDate);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get Start Date as per Calendar.
        /// </summary>
        /// <param name="calendarStartDate">Calendar start date.</param>
        /// <param name="startDate">Start date.</param>
        /// <returns>Returns start date as per calendar.</returns>
        public static string GetStartDateAsPerCalendar(DateTime calendarStartDate, DateTime startDate)
        {
            return startDate < calendarStartDate ? string.Format("{0}", calendarStartDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)) : string.Format("{0}", startDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Added By: Sohel Pathan.
        /// Function to get End Date as per Calendar.
        /// </summary>
        /// <param name="calendarEndDate">Calendar end date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>Returns end date as per calendar.</returns>
        public static DateTime GetEndDateAsPerCalendarInDateFormat(DateTime calendarEndDate, DateTime endDate)
        {
            return endDate > calendarEndDate ? calendarEndDate : endDate;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get days between start and end date as per Calendar.
        /// Modified By: Maninder Singh Wadhva to fix TFS Bug#260 Plan - Apply to Calendar - The plotting of activity bars in the calendar doesn't align with the dates mentioned.
        /// </summary>
        /// <param name="calendarStartDate">Calendar start date.</param>
        /// <param name="calendarEndDate">Calendar end date.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>Return days between start and end date.</returns>
        public static double GetEndDateAsPerCalendar(DateTime calendarStartDate, DateTime calendarEndDate, DateTime startDate, DateTime endDate)
        {
            startDate = startDate < calendarStartDate ? calendarStartDate : startDate;
            return endDate > calendarEndDate ? ((calendarEndDate.AddDays(1).AddTicks(-1)) - startDate).TotalDays : ((endDate.AddDays(1).AddTicks(-1)) - startDate).TotalDays;
        }

        /// <summary>
        /// Function to get last updated date time for current plan.
        /// Modified By Maninder Singh Wadhva to Address PL#203
        /// </summary>
        /// <param name="plan">Plan.</param>
        /// <returns>Returns last updated date time.</returns>
        public static DateTime GetLastUpdatedDate(int planId)
        {
            MRPEntities db = new MRPEntities();
            var plan = db.Plans.Single(p => p.PlanId.Equals(planId));
            List<DateTime?> lastUpdatedDate = new List<DateTime?>();
            if (plan.CreatedDate != null)
            {
                lastUpdatedDate.Add(plan.CreatedDate);
            }

            if (plan.ModifiedDate != null)
            {
                lastUpdatedDate.Add(plan.ModifiedDate);
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            if (planTactic.Count() > 0)
            {

                var planTacticModifiedDate = planTactic.ToList().Select(t => t.ModifiedDate).Max();
                lastUpdatedDate.Add(planTacticModifiedDate);

                var planTacticCreatedDate = planTactic.ToList().Select(t => t.CreatedDate).Max();
                lastUpdatedDate.Add(planTacticCreatedDate);

                var planProgramModifiedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.ModifiedDate).Max();
                lastUpdatedDate.Add(planProgramModifiedDate);

                var planProgramCreatedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedDate).Max();
                lastUpdatedDate.Add(planProgramCreatedDate);

                var planCampaignModifiedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedDate).Max();
                lastUpdatedDate.Add(planCampaignModifiedDate);

                var planCampaignCreatedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedDate).Max();
                lastUpdatedDate.Add(planCampaignCreatedDate);

                var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                               .Select(pc => pc);
                if (planTacticComment.Count() > 0)
                {
                    var planTacticCommentCreatedDate = planTacticComment.ToList().Select(pc => pc.CreatedDate).Max();
                    lastUpdatedDate.Add(planTacticCommentCreatedDate);
                }
            }

            return Convert.ToDateTime(lastUpdatedDate.Max());
        }

        public static List<string> GetCollaboratorId(int planId)
        {
            MRPEntities db = new MRPEntities();
            var plan = db.Plans.Single(p => p.PlanId.Equals(planId) && p.IsDeleted.Equals(false));

            List<string> collaboratorId = new List<string>();
            if (plan.ModifiedBy != null)
            {
                collaboratorId.Add(plan.ModifiedBy.ToString());
            }

            if (plan.CreatedBy != null)
            {
                collaboratorId.Add(plan.CreatedBy.ToString());
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId) && t.IsDeleted.Equals(false)).Select(t => t);

            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();

            var planProgramModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.ModifiedBy.ToString()).ToList();
            var planProgramCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedBy.ToString()).ToList();

            var planCampaignModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy.ToString()).ToList();
            var planCampaignCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedBy.ToString()).ToList();

            var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                           .Select(pc => pc);
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();

            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planProgramCreatedBy);
            collaboratorId.AddRange(planProgramModifiedBy);
            collaboratorId.AddRange(planCampaignCreatedBy);
            collaboratorId.AddRange(planCampaignModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);

            return collaboratorId;
        }

        /// <summary>
        /// Function to get collaborator image.
        /// </summary>
        /// <param name="planId">Plan Id.</param>
        /// <returns>Return collaborator image.</returns>
        public static JsonResult GetCollaboratorImage(int planId)
        {
            JsonResult jsonResult = new JsonResult();

            if (planId > 0)
            {

                MRPEntities db = new MRPEntities();
                List<string> newCollaboratorId = new List<string>();
                List<object> data = new List<object>();
                {
                    List<string> collaboratorIds = Common.GetCollaboratorId(planId).Distinct().ToList();
                    foreach (string userId in collaboratorIds)
                    {
                        if (System.Web.HttpContext.Current.Cache[userId + "_photo"] != null)
                        {
                            var userData = new { imageBytes = System.Web.HttpContext.Current.Cache[userId + "_photo"], name = System.Web.HttpContext.Current.Cache[userId + "_name"], businessUnit = System.Web.HttpContext.Current.Cache[userId + "_bu"], jobTitle = System.Web.HttpContext.Current.Cache[userId + "_jtitle"] }; //uday #416
                            data.Add(userData);
                        }
                        else
                        {
                            newCollaboratorId.Add(userId);
                        }
                    }
                }

                byte[] imageBytesUserImageNotFound = Common.ReadFile(HttpContext.Current.Server.MapPath("~") + "/content/images/user_image_not_found.png");
                BDSServiceClient objBDSUserRepository = new BDSServiceClient();
                List<User> users = objBDSUserRepository.GetMultipleTeamMemberDetails(string.Join(",", newCollaboratorId), Sessions.ApplicationId);

                var userlist = users.Select(u => u.BusinessUnitId).ToList();
                var businesslist = db.BusinessUnits.Where(bui => userlist.Contains(bui.BusinessUnitId)).ToList();//#416

                foreach (User user in users)
                {
                    byte[] imageBytes = null;
                    if (user != null)
                    {
                        if (user.ProfilePhoto != null)
                        {
                            imageBytes = user.ProfilePhoto;
                        }
                        else
                        {
                            imageBytes = imageBytesUserImageNotFound;
                        }
                    }

                    if (imageBytes != null)
                    {
                        using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                        {
                            ms.Write(imageBytes, 0, imageBytes.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                            image = Common.ImageResize(image, 30, 30, true, false);
                            imageBytes = Common.ImageToByteArray(image);
                        }
                    }

                    var busititle = businesslist.Single(bui => bui.BusinessUnitId == user.BusinessUnitId).Title;//#416
                    string imageBytesBase64String = Convert.ToBase64String(imageBytes);
                    System.Web.HttpContext.Current.Cache[user.UserId + "_photo"] = imageBytesBase64String;
                    System.Web.HttpContext.Current.Cache[user.UserId + "_name"] = user.FirstName + " " + user.LastName;
                    System.Web.HttpContext.Current.Cache[user.UserId + "_bu"] = busititle;//uday #416
                    System.Web.HttpContext.Current.Cache[user.UserId + "_jtitle"] = user.JobTitle;//uday #416
                    var userData = new { imageBytes = imageBytesBase64String, name = user.FirstName + " " + user.LastName, businessUnit = busititle, jobTitle = user.JobTitle };//added by uday buid & title #416 };
                    data.Add(userData);
                }
                jsonResult.Data = data;
            }
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #endregion

        #region Change log related functions

        /********************** Function to handle changelog for model & plan ************************/

        public static int InsertChangeLog(int objectId, int? parentObjectId, int componentId, string componentTitle, Enums.ChangeLog_ComponentType componentType, Enums.ChangeLog_TableName TableName, Enums.ChangeLog_Actions action, string actionSuffix = "")
        {
            /************************** Get value of component type ******************************/
            var type = typeof(Enums.ChangeLog_ComponentType);
            var memInfo = type.GetMember(componentType.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)attributes[0]).Description;
            /*************************************************************************************/

            int retval = 0;
            MRPEntities db = new MRPEntities();
            ChangeLog c1 = new ChangeLog();
            c1.ActionName = action.ToString();
            c1.ActionSuffix = actionSuffix;
            c1.ComponentId = componentId;
            c1.ComponentTitle = componentTitle;
            c1.ComponentType = description;
            c1.IsDeleted = false;
            c1.ObjectId = objectId;
            c1.ParentObjectId = parentObjectId;
            c1.TableName = TableName.ToString();
            c1.TimeStamp = DateTime.Now;
            c1.ClientId = Sessions.User.ClientId;
            c1.UserId = Sessions.User.UserId;
            db.ChangeLogs.Add(c1);
            int ret = db.SaveChanges();
            if (ret >= 1)
            {
                retval = 1;
            }
            return retval;
        }


        public static List<ChangeLog_ViewModel> GetChangeLogs(string TableName, int ObjectId = 0)
        {
            MRPEntities db = new MRPEntities();
            BDSServiceClient bdsservice = new BDSServiceClient();
            List<ChangeLog_ViewModel> lst_clvm = new List<ChangeLog_ViewModel>();
            List<ChangeLog> lst_ChangeLog = new List<ChangeLog>();
            List<int> lst_Objects = new List<int>();

            if (ObjectId > 0)
            {
                if (TableName == Enums.ChangeLog_TableName.Model.ToString())
                {
                    lst_Objects.Add(ObjectId);
                    var modellst = db.Models.Where(m => m.ModelId == ObjectId && m.IsDeleted == false).FirstOrDefault();
                    while (modellst.Model2 != null)
                    {
                        var parentId = (int)modellst.ParentModelId;
                        lst_Objects.Add(parentId);
                        modellst = modellst.Model2;
                    }
                }
                else
                {
                    lst_Objects.Add(ObjectId);
                }
            }

            if (ObjectId > 0)
            {
                lst_ChangeLog = db.ChangeLogs.Where(cl => lst_Objects.Contains(cl.ObjectId) && cl.TableName == TableName && cl.IsDeleted == false && cl.ClientId == Sessions.User.ClientId).OrderByDescending(cl => cl.TimeStamp).Take(5).ToList();
            }
            else
            {
                lst_ChangeLog = db.ChangeLogs.Where(cl => cl.TableName == TableName && cl.IsDeleted == false && cl.ClientId == Sessions.User.ClientId).OrderByDescending(cl => cl.TimeStamp).Take(5).ToList();
            }

            DateTime Current_date = DateTime.Now.Date;
            List<User> userName = new List<User>();
            string userList = string.Join(",", lst_ChangeLog.Select(cl => cl.UserId.ToString()).ToArray());
            userName = bdsservice.GetMultipleTeamMemberName(userList);
            foreach (var cl in lst_ChangeLog)
            {
                ChangeLog_ViewModel clvm = new ChangeLog_ViewModel();
                User user = userName.Where(u => u.UserId == cl.UserId).Select(u => u).FirstOrDefault(); //bdsservice.GetTeamMemberDetails(cl.UserId, Sessions.ApplicationId);
                clvm.ComponentTitle = cl.ComponentTitle;
                clvm.ComponentType = cl.ComponentType;
                clvm.Action = cl.ActionName;
                clvm.User = user.FirstName + " " + user.LastName;
                if (((DateTime.Now.Date) - (cl.TimeStamp.Date)).TotalDays == 0)
                {
                    clvm.DateStamp = "Today at " + cl.TimeStamp.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                }
                else if (((DateTime.Now.Date) - (cl.TimeStamp.Date)).TotalDays == 1)
                {
                    clvm.DateStamp = "Yesterday at " + cl.TimeStamp.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                }
                else
                {
                    clvm.DateStamp = cl.TimeStamp.ToString("MMM d") + " at " + cl.TimeStamp.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                }
                clvm.ActionSuffix = cl.ActionSuffix;
                lst_clvm.Add(clvm);
            }
            return lst_clvm;
        }

        /*********************************************************************************************/

        #endregion

        #region Plan Header

        /// <summary>
        /// Function to get Plan header values based on selected plan id
        /// </summary>
        /// <param name="planId">selected plan id</param>
        /// <returns>returns  HomePlanModelHeader object</returns>
        public static HomePlanModelHeader GetPlanHeaderValue(int planId)
        {
            HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();
            MRPEntities objDbMrpEntities = new MRPEntities();
            List<string> tacticStatus = GetStatusListAfterApproved();

            var objPlan = objDbMrpEntities.Plans.Where(plan => plan.PlanId == planId && plan.IsDeleted == false && plan.IsActive == true).Select(plan => plan).FirstOrDefault();
            if (objPlan != null)
            {
                List<Plan_Campaign_Program_Tactic> planTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tacticStatus.Contains(tactic.Status) && tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList();

                if (objPlan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Draft.ToString()].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    // Start - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                    if (objPlan.GoalType.Equals(Enums.PlanGoalType.MQL.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        objHomePlanModelHeader.MQLs = objPlan.GoalValue;
                    }
                    else
                    {
                        //// Get ADS value
                        string marketing = Enums.Funnel.Marketing.ToString();
                        var modelFunnelList = objDbMrpEntities.Model_Funnel.Where(modelfunnel => modelfunnel.ModelId == objPlan.ModelId && modelfunnel.Funnel.Title == marketing).ToList();
                        List<int> modelfunnelids = modelFunnelList.Select(modelfunnel => modelfunnel.ModelFunnelId).ToList();
                        string CR = Enums.StageType.CR.ToString();
                        List<Model_Funnel_Stage> modelFunnelStageList = objDbMrpEntities.Model_Funnel_Stage.Where(modelfunnelstage => modelfunnelids.Contains(modelfunnelstage.ModelFunnelId) && modelfunnelstage.StageType == CR).ToList();


                        double ADSValue = objDbMrpEntities.Model_Funnel.Single(mf => mf.ModelId == objPlan.ModelId && mf.Funnel.Title == marketing).AverageDealSize;
                        List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                        objHomePlanModelHeader.MQLs = Common.CalculateMQLOnly(modelfunnelids, objPlan.GoalType, objPlan.GoalValue.ToString(), ADSValue, stageList, modelFunnelStageList);
                    }
                    // End - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                    string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                    if (string.IsNullOrEmpty(MQLStageLabel))
                    {
                        objHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.ProjectedMQLLabel.ToString()].ToString();
                    }
                    else
                    {
                        objHomePlanModelHeader.mqlLabel = "Projected " + MQLStageLabel;
                    }
                    objHomePlanModelHeader.Budget = objPlan.Budget;
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Budget.ToString()].ToString();
                }
                else
                {
                    //// Added BY Bhavesh
                    //// Calculate MQL at runtime #376
                    objHomePlanModelHeader.MQLs = GetTacticStageRelation(planTacticIds, false).Sum(tactic => tactic.MQLValue);
                    string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                    if (string.IsNullOrEmpty(MQLStageLabel))
                    {
                        objHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.MQLLabel.ToString()].ToString();
                    }
                    else
                    {
                        objHomePlanModelHeader.mqlLabel = MQLStageLabel;
                    }
                    if (planTacticIds.Count() > 0)
                    {
                        ////Start Modified by Mitesh Vaishnav for PL ticket #736 Budgeting - Changes to plan header to accomodate budgeting changes
                        var tacticIds = planTacticIds.Select(tactic => tactic.PlanTacticId).ToList();
                        objHomePlanModelHeader.Budget = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => tacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).ToList().Sum(lineItem => lineItem.Cost);
                        ////End Modified by Mitesh Vaishnav for PL ticket #736 Budgeting - Changes to plan header to accomodate budgeting changes
                    }
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Cost.ToString()].ToString();
                }

                var improvementTacticList = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && !improvementTactic.IsDeleted).ToList();
                if (improvementTacticList.Count > 0)
                {
                    //// Added By: Maninder Singh Wadhva
                    //// Addressed PL Ticket: 37,38,47,49
                    //// Getting improved MQL.
                    double? improvedMQL = GetTacticStageRelation(planTacticIds, true).Sum(tactic => tactic.MQLValue);

                    //// Calculating percentage increase.
                    if (improvedMQL.HasValue && objHomePlanModelHeader.MQLs != 0)
                    {
                        objHomePlanModelHeader.PercentageMQLImproved = ((improvedMQL - objHomePlanModelHeader.MQLs) / objHomePlanModelHeader.MQLs) * 100;
                        objHomePlanModelHeader.MQLs = Convert.ToDouble(improvedMQL);
                    }
                }
                if (planTacticIds != null)
                {
                    objHomePlanModelHeader.TacticCount = planTacticIds.Count();
                }

            }
            return objHomePlanModelHeader;
        }

        #endregion

        #region Plan Header for Multiple Plans
        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 22/09/2014
        /// Description : Prepare plan header section for multiple plans
        /// </summary>
        /// <param name="planIds">list plan ids</param>
        /// <returns></returns>
        public static HomePlanModelHeader GetPlanHeaderValueForMultiplePlans(List<int> planIds, string activeMenu,string year)
        {
            HomePlanModelHeader newHomePlanModelHeader = new HomePlanModelHeader();
            MRPEntities db = new MRPEntities();
            List<string> tacticStatus = GetStatusListAfterApproved();
            int Year;
            if (!int.TryParse(year,out Year))
            {
                year = DateTime.Now.Year.ToString();
            }
            double TotalMQLs = 0, TotalBudget = 0;
            double? TotalPercentageMQLImproved = 0;
            int TotalTacticCount = 0;

            var planList = db.Plans.Where(p => planIds.Contains(p.PlanId) && p.IsDeleted == false && p.IsActive == true).Select(m => m).ToList();
            if (planList != null && planList.Count > 0)
            {
                List<Plan_Tactic> planTacticsList = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && tacticStatus.Contains(t.Status) && planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && t.Plan_Campaign_Program.Plan_Campaign.Plan.Year==year).Select(tactic => new Plan_Tactic { objPlanTactic = tactic, PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId  }).ToList();
                var improvementTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(imp => planIds.Contains(imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && imp.IsDeleted == false).ToList();
                var tacticids = planTacticsList.Select(t => t.objPlanTactic.PlanTacticId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => tacticids.Contains(l.PlanTacticId) && l.IsDeleted == false).ToList();
                Double MQLs = 0;
                List<Plan_Campaign_Program_Tactic> planTacticIds = new List<Plan_Campaign_Program_Tactic>();
                string marketing = Enums.Funnel.Marketing.ToString();
                List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

                //Added By Bhavesh For Performance Issue #Home
                List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
                List<StageList> stageListType = Common.GetStageList();
                List<Model> ModelList = db.Models.Where(m => m.IsDeleted == false).ToList();

                var improvementTacticTypeIds = improvementTacticList.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
                List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
                string size = Enums.StageType.Size.ToString();
                foreach (var plan in planList)
                {
                    //HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();

                    planTacticIds = planTacticsList.Where(t => t.PlanId == plan.PlanId).Select(tactic => tactic.objPlanTactic).ToList();

                    int? ModelId = plan.ModelId;
                    List<ModelDateList> modelDateList = new List<ModelDateList>();

                    int MainModelId = (int)ModelId;
                    while (ModelId != null)
                    {
                        var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                        modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                        ModelId = model.ParentModelId;
                    }

                    List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());
                    List<Plan_Improvement_Campaign_Program_Tactic> impList = improvementTacticList.Where(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == plan.PlanId).ToList();

                    MQLs = Common.GetTacticStageRelationForSinglePlan(planTacticIds, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, impList, modelDateList, MainModelId, stageList, false).Sum(t => t.MQLValue);

                    if (planTacticIds.Count() > 0)
                    {
                        var tacticIds = planTacticIds.Select(t => t.PlanTacticId).ToList();
                        TotalBudget += LineItemList.Where(l => tacticIds.Contains(l.PlanTacticId)).Sum(l => l.Cost);
                    }


                    if (impList.Count > 0)
                    {
                        //// Getting improved MQL.
                        double? improvedMQL = Common.GetTacticStageRelationForSinglePlan(planTacticIds, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, impList, modelDateList, MainModelId, stageList, true).Sum(t => t.MQLValue);

                        //// Calculating percentage increase.
                        if (improvedMQL.HasValue && MQLs != 0)
                        {
                            TotalPercentageMQLImproved += ((improvedMQL - MQLs) / MQLs) * 100;
                            MQLs = Convert.ToDouble(improvedMQL);
                        }
                    }

                    if (planTacticIds != null)
                    {
                        TotalTacticCount += planTacticIds.Count();
                    }

                    TotalMQLs += MQLs;

                    MQLs = 0;
                }
            }

            newHomePlanModelHeader.MQLs = TotalMQLs;
            newHomePlanModelHeader.Budget = TotalBudget;
            newHomePlanModelHeader.TacticCount = TotalTacticCount;
            newHomePlanModelHeader.PercentageMQLImproved = TotalPercentageMQLImproved;
            if (activeMenu == Enums.ActiveMenu.Home.ToString().ToLower())
            {
                string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                if (string.IsNullOrEmpty(MQLStageLabel))
                {
                    newHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.MQLLabel.ToString()].ToString();
                }
                else
                {
                    newHomePlanModelHeader.mqlLabel = MQLStageLabel;
                }
                newHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Cost.ToString()].ToString();
            }
            else if (activeMenu == Enums.ActiveMenu.Plan.ToString().ToLower())
            {
                string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                if (string.IsNullOrEmpty(MQLStageLabel))
                {
                    newHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.ProjectedMQLLabel.ToString()].ToString();
                }
                else
                {
                    newHomePlanModelHeader.mqlLabel = "Projected " + MQLStageLabel;
                }
                newHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Budget.ToString()].ToString();
            }
            return newHomePlanModelHeader;
        }
        #endregion

        #region DefaultRedirecr

        public static MVCUrl DefaultRedirectURL(Enums.ActiveMenu from)
        {
            MRPEntities mydb = new MRPEntities();
            List<Guid> businessUnitIds = new List<Guid>();
            try
            {
                // Modified by Dharmraj, For #537
                //if (Sessions.IsSystemAdmin)
                //{
                //    var clientBusinessUnit = mydb.BusinessUnits.Where(b => b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                //    businessUnitIds = clientBusinessUnit.ToList();
                //}
                var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
                if (lstAllowedBusinessUnits.Count > 0)
                    lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnits.Count == 0)//else if (Sessions.IsDirector || Sessions.IsClientAdmin)
                {
                    var clientBusinessUnit = mydb.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                    businessUnitIds = clientBusinessUnit.ToList();
                }
                else
                {
                    // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    if (lstAllowedBusinessUnits.Count > 0)
                    {
                        lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                    }
                    else
                    {
                        businessUnitIds.Add(Sessions.User.BusinessUnitId);
                    }
                    // End - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                }
                string modelPublished = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
                string modelDraft = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
                string planPublished = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                string planDraft = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;

                var models = mydb.Models.Where(m => businessUnitIds.Contains(m.BusinessUnitId) && m.IsDeleted == false).Select(m => m);

                var allModelIds = models.Select(m => m.ModelId).ToList();
                if (allModelIds == null || allModelIds.Count == 0 && from == Enums.ActiveMenu.None)
                {
                    return new MVCUrl { actionName = "HomeZero", controllerName = "Home", queryString = "" };
                }
                else
                {
                    if (allModelIds == null || allModelIds.Count == 0 && from == Enums.ActiveMenu.Home)
                    {
                        // Modified by Dharmraj, For #537
                        //if (Sessions.IsPlanner)
                        //{
                        //    return new MVCUrl { actionName = "PlanSelector", controllerName = "Plan", queryString = "" };
                        //}
                        //else
                        //{
                        //    return new MVCUrl { actionName = "ModelZero", controllerName = "Model", queryString = "" };
                        //}
                        if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate))
                        {
                            return new MVCUrl { actionName = "PlanSelector", controllerName = "Plan", queryString = "" };
                        }
                        if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit))
                        {
                            return new MVCUrl { actionName = "ModelZero", controllerName = "Model", queryString = "" };
                        }

                    }
                    var publishedModelIds = models.Where(m => m.Status.Equals(modelPublished)).Select(m => m.ModelId).ToList();
                    var draftModelIds = models.Where(m => m.Status.Equals(modelDraft)).Select(m => m.ModelId).ToList();

                    var draftPlan = mydb.Plans.Where(p => (publishedModelIds.Contains(p.Model.ModelId) || draftModelIds.Contains(p.Model.ModelId)) && p.IsDeleted.Equals(false) && p.Status.Equals(planDraft)).Select(p => p);
                    var publishedPlan = mydb.Plans.Where(p => (publishedModelIds.Contains(p.Model.ModelId) || draftModelIds.Contains(p.Model.ModelId)) && p.IsDeleted.Equals(false) && p.Status.Equals(planPublished)).Select(p => p);

                    if (publishedPlan != null && publishedPlan.Count() > 0)
                    {
                        return new MVCUrl { actionName = "Index", controllerName = "Home", queryString = "Home" };
                    }
                    else if (draftPlan != null && draftPlan.Count() > 0)
                    {
                        return new MVCUrl { actionName = "Index", controllerName = "Home", queryString = "Plan" };
                    }
                    else if (allModelIds != null || allModelIds.Count > 0)
                    {
                        return new MVCUrl { actionName = "PlanSelector", controllerName = "Plan", queryString = "" };
                    }
                    else
                    {
                        return new MVCUrl { actionName = "ModelZero", controllerName = "Model", queryString = "" };
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    throw new System.ServiceModel.EndpointNotFoundException();
                }
            }
            finally
            {
                mydb = null;
                businessUnitIds = null;
            }

            return null;
        }

        #endregion

        #region Status

        public static bool CheckAfterApprovedStatus(string status)
        {
            List<string> tacticStatus = new List<string>();
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            if (tacticStatus.Contains(status))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fuction to get list of status that comes after Approved status
        /// </summary>
        /// <returns>returns list of status</returns>
        public static List<string> GetStatusListAfterApproved()
        {
            List<string> tacticStatus = new List<string>();
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            return tacticStatus;
        }

        #endregion

        #region Manoj Limbachiya TFS:263

        public static string StageCodeINQ = "inq";
        public static string StageCodeMQL = "mql";
        public static string StageCodeCW = "cw";

        public static int StageModeINQ = 1;
        public static int StageModeMQL = 2;
        public static int StageModeCW = 3;

        /// <summary>
        /// Get Client wise stage lable name
        /// </summary>
        /// <param name="Mode">mode value in int (1 = INQ, 2 = MQL, 3 = CW)</param>
        /// <returns>returns string value for Label</returns>
        public static string GetLabel(int Mode)
        {
            MRPEntities objDbMrpEntities = new MRPEntities();
            string strStageLabel = string.Empty;
            try
            {
                string StageCode = string.Empty;
                if (Mode == StageModeINQ)
                {
                    StageCode = StageCodeINQ;
                }
                else if (Mode == StageModeMQL)
                {
                    StageCode = StageCodeMQL;
                }
                else if (Mode == StageModeCW)
                {
                    StageCode = StageCodeCW;
                }
                strStageLabel = objDbMrpEntities.Stages.Where(stage => stage.Code == StageCode && stage.IsDeleted == false && stage.ClientId == Sessions.User.ClientId).Select(stage => stage.Title).FirstOrDefault();
                return strStageLabel;
            }
            catch
            {
                objDbMrpEntities = null;
                return string.Empty;
            }
        }
        #endregion

        #region "Boost Method"
        /// <summary>
        /// Function to append improvement task data to existing task data.
        /// </summary>
        /// <param name="taskData">Current task data.</param>
        /// <param name="improvementTactics">Improvement tactic of current plan.</param>
        /// <param name="calendarStartDate">Calendar start date.</param>
        /// <param name="calendarEndDate">Calendar end date.</param>
        /// <param name="isApplyTocalendar">Flag to indicate whether it is called from apply to calendar.</param>
        /// <returns>Returns task data after appending improvement task data.</returns>
        public static List<object> AppendImprovementTaskData(List<object> taskData, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactics, DateTime calendarStartDate, DateTime calendarEndDate, bool isApplyTocalendar)
        {
            var improvementTacticTaskData = GetImprovementTacticTaskData(improvementTactics, calendarStartDate, calendarEndDate, isApplyTocalendar);
            if (improvementTacticTaskData.Count() > 0)
            {
                taskData = improvementTacticTaskData.Concat<object>(taskData).ToList<object>();
                var improvementActivityTaskData = GetImprovementActivityTaskData(improvementTactics, calendarStartDate, calendarEndDate, isApplyTocalendar);
                taskData.Insert(0, improvementActivityTaskData);
            }

            return taskData;
        }

        /// <summary>
        /// Function to get improvement tactic task data.
        /// </summary>
        /// <param name="improvementTactics">Improvement tactic of current plan.</param>
        /// <param name="calendarStartDate">Calendar start date.</param>
        /// <param name="calendarEndDate">Calendar end date.</param>
        /// <param name="isApplyTocalendar">Flag to indicate whether it is called from apply to calendar.</param>
        /// <returns>Return list of object containing improvement tactic task data.</returns>
        private static List<object> GetImprovementTacticTaskData(List<Plan_Improvement_Campaign_Program_Tactic> improvementTactics, DateTime calendarStartDate, DateTime calendarEndDate, bool isApplyTocalendar)
        {
            //// Modified By: Maninder Singh Wadhva to address Ticket 395
            MRPEntities db = new MRPEntities();
            int improvementPlanCampaignId = 0;
            if (improvementTactics.Count() > 0)
            {
                improvementPlanCampaignId = improvementTactics[0].Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId;
            }

            //// Getting Task Data.
            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

            //// Getting task data of plan improvement tactic.
            //// Modified By Maninder Singh Wadhva PL Ticket#47, 337
            var taskDataImprovementTactic = improvementTactics.Select(improvementTactic => new
            {
                id = string.Format("M{0}_I{1}_Y{2}", improvementPlanCampaignId, improvementTactic.ImprovementPlanTacticId, improvementTactic.ImprovementTacticTypeId),
                text = improvementTactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(calendarStartDate, improvementTactic.EffectiveDate),
                duration = Common.GetEndDateAsPerCalendar(calendarStartDate,
                                                          calendarEndDate,
                                                          improvementTactic.EffectiveDate,
                                                          calendarEndDate) - 1,
                progress = 0,
                open = true,
                parent = string.Format("M{0}", improvementPlanCampaignId),
                color = (isApplyTocalendar ? Common.COLORC6EBF3_WITH_BORDER_IMPROVEMENT : string.Concat(GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, improvementTactic.ImprovementTacticType.ColorCode.ToLower())),
                isSubmitted = improvementTactic.Status.Equals(tacticStatusSubmitted),
                isDeclined = improvementTactic.Status.Equals(tacticStatusDeclined),
                inqs = 0,
                mqls = 0,
                cost = improvementTactic.Cost,
                cws = 0,
                improvementTactic.ImprovementPlanTacticId,
                isImprovement = true,
                IsHideDragHandleLeft = improvementTactic.EffectiveDate < calendarStartDate,
                IsHideDragHandleRight = true,
                Status = improvementTactic.Status      //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home, Plan and ApplyToCalender screen
            }).OrderBy(t => t.text);

            return taskDataImprovementTactic.ToList<object>();
        }

        /// <summary>
        /// Function to get improvement activity task data.
        /// </summary>
        /// <param name="improvementTactics">Improvement tactic of current plan.</param>
        /// <param name="calendarStartDate">Calendar start date.</param>
        /// <param name="calendarEndDate">Calendar end date.</param>
        /// <param name="isApplyTocalendar">Flag to indicate whether it is called from apply to calendar.</param>
        /// <returns>Return improvement activity task data.</returns>
        private static object GetImprovementActivityTaskData(List<Plan_Improvement_Campaign_Program_Tactic> improvementTactics, DateTime calendarStartDate, DateTime calendarEndDate, bool isApplyTocalendar)
        {
            //// Modified By: Maninder Singh Wadhva to address Ticket 395
            MRPEntities db = new MRPEntities();
            Plan_Improvement_Campaign improvementCampaign = improvementTactics[0].Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign;

            string color = Common.COLORC6EBF3_WITH_BORDER;

            //// Getting start date for improvement activity task.
            DateTime startDate = improvementTactics.Select(improvementTactic => improvementTactic.EffectiveDate).Min();

            //// Creating task Data for the only parent of all plan improvement tactic.
            var taskDataImprovementActivity = new
            {
                id = string.Format("M{0}", improvementCampaign.ImprovementPlanCampaignId),
                text = improvementCampaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(calendarStartDate, startDate),
                duration = Common.GetEndDateAsPerCalendar(calendarStartDate,
                                                          calendarEndDate,
                                                          startDate,
                                                          calendarEndDate) - 1,
                progress = 0,
                open = true,
                color = color,
                ImprovementActivityId = improvementCampaign.ImprovementPlanCampaignId,
                isImprovement = true,
                IsHideDragHandleLeft = startDate < calendarStartDate,
                IsHideDragHandleRight = true,
                Status = improvementTactics[0].Status   //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home, Plan and ApplyToCalender screen
            };

            return taskDataImprovementActivity;
        }

        #endregion

        #region Check the status of plan before assign to Session
        public static bool IsPlanPublished(int PlanId)
        {
            MRPEntities db = new MRPEntities();
            try
            {
                var plan = db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false && p.Status.ToLower() == "published").Select(m => m).FirstOrDefault();
                if (plan != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                db = null;
                return false;
                throw;
            }
        }
        #endregion

        #region "Report Calculation"

        /// <summary>
        /// Calculate Projected Revenue of Tactic List.
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public static List<ProjectedRevenueClass> ProjectedRevenueCalculateList(List<Plan_Campaign_Program_Tactic> tlist, bool isCW = false)
        {

            List<TacticStageValue> tacticlValueList = GetTacticStageRelation(tlist, false);
            List<ProjectedRevenueClass> tacticList = tacticlValueList.Select(t => new ProjectedRevenueClass
                {
                    PlanTacticId = t.TacticObj.PlanTacticId,
                    ProjectedRevenue = isCW ? t.CWValue : t.RevenueValue
                }).ToList();

            return tacticList;
        }

        /// <summary>
        /// Get Current Quarter based on system datetime.
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentQuarter()
        {
            return ((DateTime.Now.Month - 1) / 3) + 1;
        }

        /// <summary>
        /// Get Current Month Based on system date & time.
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentMonth()
        {
            return DateTime.Now.Month;
        }

        /// <summary>
        /// Get Current Quarter based on system datetime.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentYear()
        {
            return DateTime.Now.Year.ToString();
        }

        #endregion

        #region Datetime

        /// <summary>
        /// Function to return datetime in formatted pattern.
        /// </summary>
        /// <param name="objDate"></param>
        /// <returns></returns>
        public static string GetFormatedDate(DateTime? objDate)
        {
            if (objDate == null)
                return string.Empty;
            else
                return Convert.ToDateTime(objDate).ToString("MMM dd") + " at " + Convert.ToDateTime(objDate).ToString("hh:mm tt");
        }

        #endregion

        #region replace ->

        /// <summary>
        /// function added by uday for replacing -> with other character ON 3-6-2014..
        /// </summary>
        /// <param name="objDate"></param>
        /// <returns></returns>
        public static string GetReplacedString(string obj)
        {
            if (obj == null)
                return string.Empty;
            else if (obj.Contains("->"))
            {
                return obj.Replace("->", " → ");
            }
            else
                return obj;
        }

        #endregion

        #region Delete Integration Instance
        /// <summary>
        /// Delete IntegrationInstance its relevant Model and Plan
        /// </summary>
        /// <Added By>Sohel Pathan</Added>
        /// <Date>16/05/2014</Date>
        /// <param name="integrationInstanceId"></param>
        /// <param name="deleteIntegrationInstanceModel"></param>
        /// <returns></returns>
        public static bool DeleteIntegrationInstance(int integrationInstanceId, bool deleteIntegrationInstanceModel = false, int modelId = 0)
        {
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (modelId == 0)
                        {
                            var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                a.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceTacticId != null).ToList();
                            Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId &&
                                a.IntegrationInstanceProgramId != null).ToList();
                            Plan_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_CampaignList = db.Plan_Campaign.Where(a => a.IsDeleted.Equals(false) && a.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceCampaignId != null).ToList();
                            Plan_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });


                            var Plan_Improvement_Campaign_Program_TacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                a.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceTacticId != null).ToList();
                            Plan_Improvement_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_Improvement_Campaign_ProgramList = db.Plan_Improvement_Campaign_Program.Where(a => a.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId &&
                                a.IntegrationInstanceProgramId != null).ToList();
                            Plan_Improvement_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; });

                            var Plan_Improvement_CampaignList = db.Plan_Improvement_Campaign.Where(a => a.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceCampaignId != null).ToList();
                            Plan_Improvement_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; });

                            if (deleteIntegrationInstanceModel == true)
                            {
                                var ModelsList = db.Models.Where(a => a.IsDeleted.Equals(false) && a.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceId != null).ToList();
                                ModelsList.ForEach(a => { a.IntegrationInstanceId = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                        }
                        else
                        {
                            var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                                                                                              a.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId == modelId
                                                                                                        ).ToList();
                            Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) &&
                                                                                                a.Plan_Campaign.Plan.ModelId == modelId
                                                                                          ).ToList();
                            Plan_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_CampaignList = db.Plan_Campaign.Where(a => a.IsDeleted.Equals(false) && a.Plan.ModelId == modelId).ToList();
                            Plan_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });


                            var Plan_Improvement_Campaign_Program_TacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                                                                                                                      a.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.ModelId == modelId
                                                                                                                                ).ToList();
                            Plan_Improvement_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_Improvement_Campaign_ProgramList = db.Plan_Improvement_Campaign_Program.Where(a => a.Plan_Improvement_Campaign.Plan.ModelId == modelId
                                                                                                                  ).ToList();
                            Plan_Improvement_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; });

                            var Plan_Improvement_CampaignList = db.Plan_Improvement_Campaign.Where(a => a.Plan.ModelId == modelId).ToList();
                            Plan_Improvement_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; });
                        }



                        db.SaveChanges();
                        scope.Complete();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Get Current Application Release Version
        /// <summary>
        /// Get release version detail of the current application from database.
        /// </summary>
        /// <CreartedBy>Sohel Pathan</CreartedBy>
        /// <CreatedDate>22/05/2014</CreatedDate>
        /// <returns></returns>
        public static string GetCurrentApplicationReleaseVersion()
        {
            Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            return objBDSServiceClient.GetApplicationReleaseVersion(applicationId);
        }
        #endregion

        #region Status Changes Logic for Campaign and Program

        /// <summary>
        /// Change Program status according to status of its tactics.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>26/05/201</CreatedDate>
        /// <param name="PlanProgramId"></param>
        public static void ChangeProgramStatus(int PlanProgramId)
        {
            using (MRPEntities db = new MRPEntities())
            {
                var objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == PlanProgramId && pcp.IsDeleted.Equals(false)).FirstOrDefault();

                if (objPlan_Campaign_Program != null)
                {
                    string newProgramStatus = Common.GetProgramStatus(objPlan_Campaign_Program);
                    if (newProgramStatus != string.Empty)
                    {
                        objPlan_Campaign_Program.Status = newProgramStatus;
                        objPlan_Campaign_Program.ModifiedDate = DateTime.Now;
                        objPlan_Campaign_Program.ModifiedBy = Sessions.User.UserId;
                        db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Change status of campaign according to the status of its program(s) and tactic(s)
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>26/05/201</CreatedDate>
        /// <param name="PlanCampaignId"></param>
        public static void ChangeCampaignStatus(int PlanCampaignId)
        {
            using (MRPEntities db = new MRPEntities())
            {
                var objPlan_Campaign = db.Plan_Campaign.Where(pcp => pcp.PlanCampaignId == PlanCampaignId && pcp.IsDeleted.Equals(false)).FirstOrDefault();

                if (objPlan_Campaign != null)
                {
                    string newCampaignStatus = Common.GetCampaignStatus(objPlan_Campaign);
                    if (newCampaignStatus != string.Empty)
                    {
                        objPlan_Campaign.Status = newCampaignStatus;
                        objPlan_Campaign.ModifiedDate = DateTime.Now;
                        objPlan_Campaign.ModifiedBy = Sessions.User.UserId;
                        db.Entry(objPlan_Campaign).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Function to Get program status.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>26/05/201</CreatedDate>
        /// <param name="objPlan_Campaign_Program"></param>
        /// <returns>returns status for the program</returns>
        public static string GetProgramStatus(Plan_Campaign_Program objPlan_Campaign_Program)
        {
            string status = string.Empty;
            if (objPlan_Campaign_Program != null)
            {
                string statusapproved = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Approved.ToString()].ToString();
                string statusinprogress = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.InProgress.ToString()].ToString();
                string statuscomplete = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Complete.ToString()].ToString();
                string statusdecline = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Decline.ToString()].ToString();
                string statussubmit = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Submitted.ToString()].ToString();

                try
                {
                    using (MRPEntities db = new MRPEntities())
                    {
                        var lstTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == objPlan_Campaign_Program.PlanProgramId && pcpt.IsDeleted == false).ToList();

                        if (lstTactic != null)
                        {
                            if (lstTactic.Count > 0)
                            {
                                int cntAllSumbitTacticStatus = lstTactic.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                                int cntAllApproveTacticStatus = lstTactic.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                                int cntAllDeclineTacticStatus = lstTactic.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();

                                int cntSubmitTacticStatus = lstTactic.Where(pcpt => pcpt.Status.Equals(statussubmit)).Count();
                                int cntApproveTacticStatus = lstTactic.Where(pcpt => pcpt.Status.Equals(statusapproved)).Count();
                                int cntDeclineTacticStatus = lstTactic.Where(pcpt => pcpt.Status.Equals(statusdecline)).Count();

                                bool flag = false;
                                foreach (var item in lstTactic)
                                {
                                    if (item.Status == objPlan_Campaign_Program.Status)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }

                                if (cntAllSumbitTacticStatus == 0)
                                {
                                    status = statussubmit;
                                }
                                else if (cntAllApproveTacticStatus == 0)
                                {
                                    status = statusapproved;
                                }
                                else if (cntAllDeclineTacticStatus == 0)
                                {
                                    status = statusdecline;
                                }
                                else if (!flag)
                                {
                                    if (cntSubmitTacticStatus > 0)
                                    {
                                        status = statussubmit;
                                    }
                                    else if (cntApproveTacticStatus > 0)
                                    {
                                        status = statusapproved;
                                    }
                                    else if (cntDeclineTacticStatus > 0)
                                    {
                                        status = statusdecline;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                }
            }
            return status;
        }

        /// <summary>
        /// Function to Get campaign status.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>26/05/201</CreatedDate>
        /// <param name="objPlan_Campaign"></param>
        /// <returns>returns status for the Campaign</returns>
        public static string GetCampaignStatus(Plan_Campaign objPlan_Campaign)
        {
            string status = string.Empty;
            if (objPlan_Campaign != null)
            {
                string statusapproved = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Approved.ToString()].ToString();
                string statusinprogress = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.InProgress.ToString()].ToString();
                string statuscomplete = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Complete.ToString()].ToString();
                string statusdecline = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Decline.ToString()].ToString();
                string statussubmit = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Submitted.ToString()].ToString();

                try
                {
                    using (MRPEntities db = new MRPEntities())
                    {
                        var lstProgram = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted.Equals(false)).ToList();

                        if (lstProgram != null)
                        {
                            if (lstProgram.Count > 0)
                            {
                                // Number of program with status is not 'Submitted' 
                                int cntSumbitProgramStatus = lstProgram.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                                // Number of tactic with status is not 'Submitted'
                                int cntSumbitTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();

                                // Number of program with status is not 'Approved', 'in-progress', 'complete'
                                int cntApproveProgramStatus = lstProgram.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                                // Number of tactic with status is not 'Approved', 'in-progress', 'complete'
                                int cntApproveTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();

                                // Number of program with status is not 'Declained'
                                int cntDeclineProgramStatus = lstProgram.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();
                                // Number of tactic with status is not 'Declained'
                                int cntDeclineTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();

                                List<string> lstPStatus = new List<string>();

                                foreach (var p in lstProgram)
                                {
                                    string tmpStatus = GetProgramStatus(p);
                                    if (tmpStatus != string.Empty)
                                        lstPStatus.Add(tmpStatus);
                                }

                                bool flag = false;
                                foreach (var item in lstPStatus)
                                {
                                    if (item == objPlan_Campaign.Status)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }

                                if (cntSumbitProgramStatus == 0 && cntSumbitTacticStatus == 0)
                                {
                                    status = statussubmit;
                                }
                                else if (cntApproveProgramStatus == 0 && cntApproveTacticStatus == 0)
                                {
                                    status = statusapproved;
                                }
                                else if (cntDeclineProgramStatus == 0 && cntDeclineTacticStatus == 0)
                                {
                                    status = statusdecline;
                                }
                                else if (!flag)
                                {
                                    if (lstPStatus.Contains(statussubmit))
                                    {
                                        status = statussubmit;
                                    }
                                    else if (lstPStatus.Contains(statusapproved))
                                    {
                                        status = statusapproved;
                                    }
                                    else if (lstPStatus.Contains(statusdecline))
                                    {
                                        status = statusdecline;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                }
            }
            return status;
        }

        #endregion

        #region Cost Calculation

        /// <summary>
        /// Calculate cost for Campaign
        /// Ticcket #440
        /// </summary>
        /// <param name="planCampaignId"></param>
        /// <returns></returns>
        public static double CalculateCampaignCost(int planCampaignId)
        {
            using (MRPEntities db = new MRPEntities())
            {
                var lstProgram = db.Plan_Campaign.FirstOrDefault(varC => varC.PlanCampaignId == planCampaignId)
                                                 .Plan_Campaign_Program
                                                 .Where(varP => varP.IsDeleted == false)
                                                 .ToList();

                double cost = 0;
                /*Modified by Mitesh Vaishnav on 29/07/2014 for PL ticket #619*/
                lstProgram.ForEach(varP => varP.Plan_Campaign_Program_Tactic.Where(varT => varT.IsDeleted == false).ToList().ForEach(varT => cost = cost + varT.Plan_Campaign_Program_Tactic_LineItem.Where(varL => varL.IsDeleted == false).Sum(varL => varL.Cost)));

                return cost;

            }
        }

        /// <summary>
        /// Calculate cost for Program
        /// Ticcket #440
        /// </summary>
        /// <param name="planProgramId"></param>
        /// <returns></returns>
        public static double CalculateProgramCost(int planProgramId)
        {
            using (MRPEntities db = new MRPEntities())
            {
                var lstTactic = db.Plan_Campaign_Program.FirstOrDefault(varC => varC.PlanProgramId == planProgramId)
                                                 .Plan_Campaign_Program_Tactic
                                                 .Where(varP => varP.IsDeleted == false)
                                                 .ToList();

                double cost = 0;
                /*Modified by Mitesh Vaishnav on 29/07/2014 for PL ticket #619*/
                lstTactic.ForEach(varT => cost = cost + varT.Plan_Campaign_Program_Tactic_LineItem.Where(varL => varL.IsDeleted == false).Sum(varL => varL.Cost));

                return cost;

            }
        }

        #endregion

        #region TacticStageTitle

        /// <summary>
        /// Function to get dynamic stage titles.
        /// added by Dharmraj Ticket #440
        /// </summary>
        /// <param name="tacticId"></param>
        /// <returns></returns>
        public static List<string> GetTacticStageTitle(int tacticId)
        {
            MRPEntities db = new MRPEntities();
            List<string> lstStageTitle = new List<string>();
            string stageMQL = Enums.Stage.MQL.ToString();
            int tacticStageLevel = Convert.ToInt32(db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == tacticId).Stage.Level);
            int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;

            if (tacticStageLevel < levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString());
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString());
            }
            else if (tacticStageLevel == levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString());
            }
            else if (tacticStageLevel > levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString());
            }

            lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString());
            lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString());

            return lstStageTitle;
        }

        #endregion

        #region CustomizedStage

        /// <summary>
        /// Get values of tactic stages
        /// Created By : Bhavesh Dobariya 
        /// Calculate MQL,CW,Revenue etc. for each tactic
        /// </summary>
        /// <param name="tlist">List collection of tactics</param>
        /// <param name="isIncludeImprovement">boolean flag that indicate tactic included imporvement sections</param>
        /// <returns>returns list tactic stage values</returns>
        public static List<TacticStageValue> GetTacticStageRelation(List<Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true)
        {
            MRPEntities objDbMRPEntities = new MRPEntities();
            //// Compute the tactic relation list
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculation(tlist, isIncludeImprovement);
            List<Stage> stageList = objDbMRPEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            //// Fetch the tactic stages and it's value
            //// Return finalized TacticStageValue list to the Parent method 
            return GetTacticStageValueList(tlist, tacticValueRelationList, stageList, false); ;
        }

        /// <summary>
        /// Get values of tactic stages
        /// Created By : Bhavesh Dobariya
        /// Calculate MQL,CW,Revenue etc. for each tactic of single plan.
        /// </summary>
        /// <param name="tlist">List collection of tactics</param>
        /// <param name="isIncludeImprovement">boolean flag that indicate tactic included imporvement sections</param>
        /// <returns></returns>
        public static List<TacticStageValue> GetTacticStageRelationForSinglePlan(List<Plan_Campaign_Program_Tactic> tlist, List<StageRelation> bestInClassStageRelation, List<StageList> stageListType, List<ModelStageRelationList> modleStageRelationList, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ModelDateList> modelDateList, int ModelId, List<Stage> stageList, bool isIncludeImprovement = true)
        {
            //Compute the tactic relation list
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculationForSinglePlan(tlist, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, ModelId, isIncludeImprovement);
            //fetch the tactic stages and it's value
            //Return finalized TacticStageValue list to the Parent method 
            return GetTacticStageValueList(tlist, tacticValueRelationList, stageList, true);
        }

        public static List<TacticStageValue> GetTacticStageValueList(List<Plan_Campaign_Program_Tactic> tlist, List<TacticStageValueRelation> tacticValueRelationList, List<Stage> stageList, bool isSinglePlan = false)
        {
            MRPEntities dbStage = new MRPEntities();
            List<TacticStageValue> tacticStageList = new List<TacticStageValue>();
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = stageList.Where(s => s.Code.Equals(stageINQ)).Select(s => s.Level.Value).FirstOrDefault();
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = stageList.Where(s => s.Code.Equals(stageMQL)).Select(s => s.Level.Value).FirstOrDefault();
            string stageCW = Enums.Stage.CW.ToString();
            int levelCW = stageList.Where(s => s.Code.Equals(stageCW)).Select(s => s.Level.Value).FirstOrDefault();
            List<int> inqStagelist = new List<int>();
            List<int> mqlStagelist = new List<int>();
            List<int> cwStagelist = new List<int>();
            List<int> revenueStagelist = new List<int>();
            //Select the pre defined tactic stages from the stageList list
            List<int> inqVelocityStagelist = stageList.Where(s => s.Level >= 1 && s.Level < levelINQ).Select(s => s.StageId).ToList();
            List<int> mqlVelocityStagelist = stageList.Where(s => s.Level >= levelINQ && s.Level < levelMQL).Select(s => s.StageId).ToList();
            List<int> cwVelocityStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();

            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            string Size = Enums.StageType.Size.ToString();
            List<Plan_Campaign_Program_Tactic_Actual> actualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            if (!isSinglePlan)
            {
                List<int> TacticIds = tlist.Select(t => t.PlanTacticId).ToList();
                actualTacticList = dbStage.Plan_Campaign_Program_Tactic_Actual.Where(a => TacticIds.Contains(a.PlanTacticId)).ToList();
            }
            //Ittrate the Plan_Campaign_Program_Tactic list and Assign it to TacticStageValue 
            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                List<StageRelation> stageRelation = tacticValueRelationList.Where(t => t.TacticObj.PlanTacticId == tactic.PlanTacticId).Select(t => t.StageValueList).FirstOrDefault();
                int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                //int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                inqStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelINQ).Select(s => s.StageId).ToList();
                mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                revenueStagelist = stageList.Where(s => (s.Level >= projectedStageLevel && s.Level <= levelCW) || s.Level == null).Select(s => s.StageId).ToList();

                TacticStageValue tacticStageValueObj = new TacticStageValue();
                tacticStageValueObj.TacticObj = tactic;
                tacticStageValueObj.INQValue = projectedStageLevel <= levelINQ ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => inqStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.MQLValue = projectedStageLevel <= levelMQL ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => mqlStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.CWValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => cwStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.RevenueValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => revenueStagelist.Contains(sr.StageId) && (sr.StageType == CR || sr.StageType == Size)).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.INQVelocity = stageRelation.Where(sr => inqVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.MQLVelocity = stageRelation.Where(sr => mqlVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.CWVelocity = stageRelation.Where(sr => cwVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.ADSValue = stageRelation.Where(sr => sr.StageType == Size).Sum(sr => sr.Value);
                tacticStageValueObj.TacticYear = tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;
                if (!isSinglePlan)
                {
                    tacticStageValueObj.ActualTacticList = actualTacticList.Where(a => a.PlanTacticId == tactic.PlanTacticId).ToList();
                }

                tacticStageList.Add(tacticStageValueObj);
            }
            //Return finalized TacticStageValue list to the Parent method 
            return tacticStageList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Calculate Value for single plan data
        /// </summary>
        /// <param name="tlist"></param>
        /// <param name="bestInClassStageRelation"></param>
        /// <param name="stageList"></param>
        /// <param name="modleStageRelationList"></param>
        /// <param name="improvementTacticTypeMetric"></param>
        /// <param name="improvementActivities"></param>
        /// <param name="modelDateList"></param>
        /// <param name="ModelId"></param>
        /// <param name="isIncludeImprovement"></param>
        /// <returns></returns>
        public static List<TacticStageValueRelation> GetCalculationForSinglePlan(List<Plan_Campaign_Program_Tactic> tlist, List<StageRelation> bestInClassStageRelation, List<StageList> stageList, List<ModelStageRelationList> modleStageRelationList, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ModelDateList> modelDateList, int ModelId, bool isIncludeImprovement = true)
        {
            List<TacticStageValueRelation> TacticSatgeValueList = new List<TacticStageValueRelation>();
            string Size = Enums.StageType.Size.ToString();
            int ADSStageId = stageList.Where(s => s.Level == null && s.StageType == Size).Select(s => s.StageId).FirstOrDefault();

            double bestInClassAdsValue = 0;

            var objbestInClassAdsValue = bestInClassStageRelation.Where(b => b.StageId == ADSStageId).FirstOrDefault();

            if (!string.IsNullOrEmpty(Convert.ToString(objbestInClassAdsValue)))
            {
                bestInClassAdsValue = objbestInClassAdsValue.Value;
            }

            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                int modelId = GetModelIdFromList(modelDateList, tactic.StartDate, ModelId);
                List<StageRelation> stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
                List<Plan_Improvement_Campaign_Program_Tactic> improvementList = improvementActivities.Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
                if (improvementList.Count() > 0 && isIncludeImprovement)
                {
                    TacticStageValueRelation tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tactic;

                    var improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    var improvementIdsWeighList = improvementTacticTypeMetric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId)).Select(imptype => imptype).ToList();
                    List<StageRelation> stageRelationList = new List<StageRelation>();
                    foreach (StageList stage in stageList)
                    {
                        StageRelation stageRelationObj = new StageRelation();
                        stageRelationObj.StageId = stage.StageId;
                        stageRelationObj.StageType = stage.StageType;
                        var stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Weight > 0).ToList();
                        double impcount = stageimplist.Count();
                        double impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
                        double improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Where(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
                        stageRelationObj.Value = improvementValue;
                        stageRelationList.Add(stageRelationObj);
                    }

                    tacticStageObj.StageValueList = stageRelationList;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
                else
                {
                    TacticStageValueRelation tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tactic;
                    tacticStageObj.StageValueList = stageModelRelation;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
            }
            return TacticSatgeValueList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Calculate Value for Tactic data for Multiple plan
        /// </summary>
        /// <param name="tlist">List collection of tactics</param>
        /// <param name="isIncludeImprovement">boolean flag that indicate tactic included imporvement sections</param>
        /// <returns>returns list tactic stage values</returns>
        public static List<TacticStageValueRelation> GetCalculation(List<Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true)
        {
            List<TacticPlanRelation> tacticPlanList = GetTacticPlanRelationList(tlist);
            List<TacticModelRelation> tacticModelList = GetTacticModelRelationList(tlist, tacticPlanList);
            List<PlanIMPTacticListRelation> planIMPTacticList = GetPlanImprovementTacticList(tacticPlanList.Select(p => p.PlanId).Distinct().ToList());
            List<StageRelation> bestInClassStageRelation = GetBestInClassValue();
            List<ModelStageRelationList> modleStageRelationList = GetModelStageRelation(tacticModelList.Select(m => m.ModelId).Distinct().ToList());
            List<int> impids = new List<int>();
            planIMPTacticList.ForEach(t => t.ImprovementTacticList.ForEach(imp => impids.Add(imp.ImprovementTacticTypeId)));
            List<ImprovementTypeWeightList> improvementTypeWeightList = GetImprovementTacticWeightList(impids);
            List<StageList> stageList = GetStageList();
            List<TacticStageValueRelation> TacticSatgeValueList = new List<TacticStageValueRelation>();
            string Size = Enums.StageType.Size.ToString();
            int ADSStageId = stageList.Where(s => s.Level == null && s.StageType == Size).Select(s => s.StageId).FirstOrDefault();

            //Modified By Kalpesh Sharma : Internal Review Point #83 Error on home page when user login for Other Clients

            double bestInClassAdsValue = 0;

            var objbestInClassAdsValue = bestInClassStageRelation.Where(b => b.StageId == ADSStageId).FirstOrDefault();

            if (!string.IsNullOrEmpty(Convert.ToString(objbestInClassAdsValue)))
            {
                bestInClassAdsValue = objbestInClassAdsValue.Value;
            }

            List<PlanADSRelation> planADSList = GetPlanADSList(planIMPTacticList, improvementTypeWeightList, ADSStageId, Size, bestInClassAdsValue);

            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                int planId = tacticPlanList.Where(t => t.PlanTacticId == tactic.PlanTacticId).Select(t => t.PlanId).FirstOrDefault();
                int modelId = tacticModelList.Where(t => t.PlanTacticId == tactic.PlanTacticId).Select(t => t.ModelId).FirstOrDefault();
                List<StageRelation> stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
                List<Plan_Improvement_Campaign_Program_Tactic> improvementList = (planIMPTacticList.Where(p => p.PlanId == planId).Select(p => p.ImprovementTacticList).FirstOrDefault()).Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
                if (improvementList.Count() > 0 && isIncludeImprovement)
                {
                    TacticStageValueRelation tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tactic;

                    var improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    var improvementIdsWeighList = improvementTypeWeightList.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTypeId) && imptype.isDeploy).Select(imptype => imptype).ToList();
                    List<StageRelation> stageRelationList = new List<StageRelation>();
                    foreach (StageList stage in stageList)
                    {
                        StageRelation stageRelationObj = new StageRelation();
                        stageRelationObj.StageId = stage.StageId;
                        stageRelationObj.StageType = stage.StageType;
                        if (stage.StageType == Size && ADSStageId == stage.StageId && planADSList.Where(p => p.PlanId == planId).Select(p => p.isImprovementExits).FirstOrDefault())
                        {
                            stageRelationObj.Value = planADSList.Where(p => p.PlanId == planId).Select(p => p.ADS).FirstOrDefault();
                        }
                        else
                        {
                            var stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Value > 0).ToList();
                            double impcount = stageimplist.Count();
                            double impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Value);
                            double improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Where(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
                            stageRelationObj.Value = improvementValue;
                        }
                        stageRelationList.Add(stageRelationObj);
                    }

                    tacticStageObj.StageValueList = stageRelationList;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
                else
                {
                    TacticStageValueRelation tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tactic;
                    tacticStageObj.StageValueList = stageModelRelation;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
            }
            return TacticSatgeValueList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Mapping Tactic Plan Relation
        /// </summary>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public static List<TacticPlanRelation> GetTacticPlanRelationList(List<Plan_Campaign_Program_Tactic> tlist)
        {
            List<TacticPlanRelation> tacticPlanlist = (from t in tlist
                                                       select new TacticPlanRelation
                                                       {
                                                           PlanTacticId = t.PlanTacticId,
                                                           PlanId = t.Plan_Campaign_Program.Plan_Campaign.PlanId
                                                       }).ToList();
            return tacticPlanlist;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Plan - Improvement Relation
        /// </summary>
        /// <param name="planIds"></param>
        /// <returns></returns>
        public static List<PlanIMPTacticListRelation> GetPlanImprovementTacticList(List<int> planIds)
        {
            MRPEntities dbStage = new MRPEntities();

            List<PlanIMPTacticListRelation> planIMPTacticList = new List<PlanIMPTacticListRelation>();
            var implist = dbStage.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => planIds.Contains(imptactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && !imptactic.IsDeleted).ToList();
            foreach (int planId in planIds)
            {
                PlanIMPTacticListRelation planTacticIMP = new PlanIMPTacticListRelation();
                planTacticIMP.PlanId = planId;
                planTacticIMP.ImprovementTacticList = implist.Where(imptactic => imptactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId).ToList();
                planIMPTacticList.Add(planTacticIMP);
            }

            return planIMPTacticList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Best in Class Value list
        /// </summary>
        /// <returns></returns>
        public static List<StageRelation> GetBestInClassValue()
        {
            MRPEntities dbStage = new MRPEntities();
            string size = Enums.StageType.Size.ToString();
            string CR = Enums.StageType.CR.ToString();

            return dbStage.BestInClasses.Where(best => best.Stage.ClientId == Sessions.User.ClientId).Select(best =>
                new StageRelation
                {
                    StageId = best.StageId,
                    StageType = best.StageType,
                    Value = best.StageType == CR ? best.Value / 100 : best.Value
                }
                ).ToList();
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Model & Stage code Relation with value
        /// </summary>
        /// <param name="modleIds"></param>
        /// <returns></returns>
        public static List<ModelStageRelationList> GetModelStageRelation(List<int> modleIds)
        {
            MRPEntities dbStage = new MRPEntities();
            string marketing = Enums.Funnel.Marketing.ToString();
            string size = Enums.StageType.Size.ToString();
            string CR = Enums.StageType.CR.ToString();
            string ADS = Enums.Stage.ADS.ToString();
            var stagelist = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).ToList();
            int adsStageId = stagelist.Where(stage => stage.Code.Equals(ADS)).Select(stage => stage.StageId).FirstOrDefault();
            var ModelFunnelList = dbStage.Model_Funnel_Stage.Where(m => m.Model_Funnel.Funnel.Title.Equals(marketing) && modleIds.Contains(m.Model_Funnel.ModelId)).ToList();
            List<ModelStageRelationList> modleStageRelationist = new List<ModelStageRelationList>();
            foreach (int modelId in modleIds)
            {
                ModelStageRelationList modelStageObj = new ModelStageRelationList();
                modelStageObj.ModelId = modelId;
                modelStageObj.StageList = ModelFunnelList.Where(m => m.Model_Funnel.ModelId == modelId).Select(m => new StageRelation
                {
                    StageId = m.StageId,
                    StageType = m.StageType,
                    Value = m.StageType == CR ? m.Value / 100 : m.Value
                }
                    ).ToList();

                double ads = dbStage.Model_Funnel.Where(m => m.Funnel.Title.Equals(marketing) && m.ModelId == modelId).Select(m => m.AverageDealSize).FirstOrDefault();
                modelStageObj.StageList.Add(new StageRelation { StageId = adsStageId, StageType = size, Value = ads });
                modleStageRelationist.Add(modelStageObj);
            }

            return modleStageRelationist;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Improvement Weight for Stage Code
        /// </summary>
        /// <param name="improvementTacticTypeIds"></param>
        /// <returns></returns>
        public static List<ImprovementTypeWeightList> GetImprovementTacticWeightList(List<int> improvementTacticTypeIds)
        {
            MRPEntities dbStage = new MRPEntities();
            List<ImprovementTypeWeightList> improvementTypeWeightList = new List<ImprovementTypeWeightList>();
            var improvementTacticTypeList = dbStage.ImprovementTacticTypes.Where(imp => improvementTacticTypeIds.Contains(imp.ImprovementTacticTypeId)).ToList();
            var improvementTacticTypeWeightList = dbStage.ImprovementTacticType_Metric.Where(imp => improvementTacticTypeIds.Contains(imp.ImprovementTacticTypeId)).ToList();
            foreach (int improvementTacticTypeId in improvementTacticTypeIds)
            {
                bool isDeployed = improvementTacticTypeList.Where(imp => imp.ImprovementTacticTypeId == improvementTacticTypeId).Select(imp => imp.IsDeployed).FirstOrDefault();
                var innerList = improvementTacticTypeWeightList.Where(imp => imp.ImprovementTacticTypeId == improvementTacticTypeId).Select(imp => imp).ToList();
                innerList.ForEach(innerl => improvementTypeWeightList.Add(new ImprovementTypeWeightList { ImprovementTypeId = improvementTacticTypeId, isDeploy = isDeployed, StageId = innerl.StageId, StageType = innerl.StageType, Value = innerl.Weight }));
            }

            return improvementTypeWeightList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Stage List for Client
        /// </summary>
        /// <returns></returns>
        public static List<StageList> GetStageList()
        {
            MRPEntities dbStage = new MRPEntities();
            List<StageList> stageList = new List<StageList>();
            var Stage = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).ToList();
            string CW = Enums.Stage.CW.ToString();
            string CR = Enums.StageType.CR.ToString();
            foreach (var s in Stage.Where(st => st.Level != null && st.Code != CW))
            {
                StageList sl = new StageList();
                sl.StageId = s.StageId;
                sl.StageType = CR;
                sl.Level = s.Level;
                stageList.Add(sl);
            }
            string SV = Enums.StageType.SV.ToString();
            foreach (var s in Stage.Where(st => st.Level != null && st.Code != CW))
            {
                StageList sl = new StageList();
                sl.StageId = s.StageId;
                sl.StageType = SV;
                sl.Level = s.Level;
                stageList.Add(sl);
            }
            string Size = Enums.StageType.Size.ToString();
            foreach (var s in Stage.Where(st => st.Level == null))
            {
                StageList sl = new StageList();
                sl.StageId = s.StageId;
                sl.StageType = Size;
                sl.Level = s.Level;
                stageList.Add(sl);
            }

            return stageList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Calculate Improvement
        /// </summary>
        /// <param name="stageType"></param>
        /// <param name="bestInClassValue"></param>
        /// <param name="modelvalue"></param>
        /// <param name="TotalCount"></param>
        /// <param name="TotalWeight"></param>
        /// <returns></returns>
        public static double GetImprovement(string stageType, double bestInClassValue, double modelvalue, double TotalCount, double TotalWeight)
        {
            //// Declate CFactor & rFactor Variable
            double cFactor = 0;
            double rFactor = 0;


            #region rFactor

            //// Calculate rFactor if TotalCount = 0 then 0
            //// Else if TotalWeight/TotalCount < 2 then 0.25
            //// Else if TotalWeight/TotalCount < 3 then 0.5
            //// Else if TotalWeight/TotalCount < 4 then 0.75
            //// Else  0.9
            if (TotalCount == 0)
            {
                rFactor = 0;
            }
            else
            {
                double wcValue = TotalWeight / TotalCount;
                if (wcValue < 2)
                {
                    rFactor = 0.25;
                }
                else if (wcValue >= 2 && wcValue < 3)
                {
                    rFactor = 0.5;
                }
                else if (wcValue >= 3 && wcValue < 4)
                {
                    rFactor = 0.75;
                }
                else
                {
                    rFactor = 0.9;
                }
            }

            #endregion

            #region cFactor

            //// Calculate cFactor if TotalCount < 3 then 0.4
            //// Else if TotalCount >= 3 AND TotalCount < 5 then 0.6
            //// Else if TotalCount >= 5 AND TotalCount < 8 then 0.8
            //// Else  1

            if (TotalCount < 3)
            {
                cFactor = 0.4;
            }
            else if (TotalCount >= 3 && TotalCount < 5)
            {
                cFactor = 0.6;
            }
            else if (TotalCount >= 5 && TotalCount < 8)
            {
                cFactor = 0.8;
            }
            else
            {
                cFactor = 1;
            }

            #endregion

            //// Calculate BoostFactor
            double boostFactor = cFactor * rFactor;
            double boostGap = 0;
            //// Calculate boostGap
            if (stageType == Enums.MetricType.CR.ToString())
            {
                boostGap = bestInClassValue - modelvalue;
            }
            else if (stageType == Enums.MetricType.SV.ToString())
            {
                boostGap = modelvalue - bestInClassValue;
            }
            else if (stageType == Enums.MetricType.Size.ToString())
            {
                // Divide by 100 because it percentage value
                boostGap = bestInClassValue / 100;
            }

            //// Calculate Improvement
            double improvement = boostGap * boostFactor;
            if (improvement < 0)
            {
                improvement = 0;
            }

            double improvementValue = 0;
            if (stageType == Enums.MetricType.CR.ToString())
            {
                improvementValue = modelvalue + improvement;
            }
            else if (stageType == Enums.MetricType.SV.ToString())
            {
                improvementValue = modelvalue - improvement;
            }
            else if (stageType == Enums.MetricType.Size.ToString())
            {
                improvementValue = (1 + improvement) * modelvalue;
            }

            return improvementValue;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Plan & ADS list
        /// </summary>
        /// <param name="planIMPList"></param>
        /// <param name="improvementTypeWeightList"></param>
        /// <param name="ADSStageId"></param>
        /// <param name="Size"></param>
        /// <param name="bestInClassSizeValue"></param>
        /// <returns></returns>
        public static List<PlanADSRelation> GetPlanADSList(List<PlanIMPTacticListRelation> planIMPList, List<ImprovementTypeWeightList> improvementTypeWeightList, int ADSStageId, string Size, double bestInClassSizeValue)
        {
            MRPEntities dbStage = new MRPEntities();
            string marketing = Enums.Funnel.Marketing.ToString();
            List<PlanADSRelation> planADSList = new List<PlanADSRelation>();
            List<int> planIds = planIMPList.Select(p => p.PlanId).ToList();
            List<PlanModelRelation> planModelRelation = dbStage.Plans.Where(p => planIds.Contains(p.PlanId)).Select(p => new PlanModelRelation { PlanId = p.PlanId, ModelId = p.ModelId }).ToList();
            foreach (PlanIMPTacticListRelation planIMP in planIMPList)
            {
                bool isImprovementExits = false;
                int modelId = planModelRelation.Where(p => p.PlanId == planIMP.PlanId).Select(p => p.ModelId).FirstOrDefault();
                if (planIMP.ImprovementTacticList.Count > 0)
                {
                    //// Get Model id based on effective date From.
                    modelId = GetModelId(planIMP.ImprovementTacticList.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), modelId);
                }
                double ADSValue = dbStage.Model_Funnel.Where(mf => mf.ModelId == modelId && mf.Funnel.Title == marketing).Select(mf => mf.AverageDealSize).FirstOrDefault();
                if (planIMP.ImprovementTacticList.Count > 0)
                {
                    var improvementTypeList = planIMP.ImprovementTacticList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    var improvementIdsWeighList = improvementTypeWeightList.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTypeId) && imptype.isDeploy).Select(imptype => imptype).ToList();
                    var stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == ADSStageId && impweight.StageType == Size && impweight.Value > 0).ToList();
                    double impcount = stageimplist.Count();
                    double impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Value);
                    ADSValue = GetImprovement(Size, bestInClassSizeValue, ADSValue, impcount, impWeight);
                    isImprovementExits = true;
                }
                planADSList.Add(new PlanADSRelation { PlanId = planIMP.PlanId, ADS = ADSValue, isImprovementExits = isImprovementExits });
            }

            return planADSList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Calculate value for Single Tactic
        /// </summary>
        /// <param name="objTactic"></param>
        /// <param name="isIncludeImprovement"></param>
        /// <returns></returns>
        public static TacticStageValue GetTacticStageRelationForSingleTactic(Plan_Campaign_Program_Tactic objTactic, bool isIncludeImprovement = true)
        {
            MRPEntities db = new MRPEntities();
            List<Plan_Campaign_Program_Tactic> lstTactic = new List<Plan_Campaign_Program_Tactic>();
            lstTactic.Add(objTactic);

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = GetBestInClassValue();
            List<StageList> stageListType = GetStageList();
            int? ModelId = objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(objTactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && t.IsDeleted == false).Select(t => t).ToList();

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            //End #955

            List<TacticStageValue> TacticData = GetTacticStageRelationForSinglePlan(lstTactic, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, isIncludeImprovement);
            if (TacticData.Count > 0)
            {
                return TacticData[0];
            }
            else
            {
                TacticStageValue tstage = new TacticStageValue();
                tstage.MQLValue = 0;
                tstage.RevenueValue = 0;
                tstage.CWValue = 0;
                return tstage;
            }
        }

        /// <summary>
        /// Get ModelId based on Effective Date 
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="StartDate">StartDate</param>
        /// <param name="ModelId">ModelId</param>
        /// <returns>Return Model Id</returns>
        public static int GetModelId(DateTime StartDate, int ModelId)
        {
            MRPEntities mdbt = new MRPEntities();
            DateTime? effectiveDate = mdbt.Models.Where(m => m.ModelId == ModelId).Select(m => m.EffectiveDate).SingleOrDefault();
            if (effectiveDate != null)
            {
                if (StartDate >= effectiveDate)
                {
                    return ModelId;
                }
                else
                {
                    int? ParentModelId = mdbt.Models.Where(m => m.ModelId == ModelId).Select(m => m.ParentModelId).SingleOrDefault();
                    if (ParentModelId != null)
                    {
                        return GetModelId(StartDate, (int)ParentModelId);
                    }
                    else
                    {
                        return ModelId;
                    }
                }
            }
            else
            {
                return ModelId;
            }
        }

        /// <summary>
        /// Gel Tactic with MQl value list.
        /// PL Ticket #376 Remove storing of pre-calculated MQL to DB
        /// Date 8-4-2014.
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="PlanTacticIds"></param>
        /// <param name="isRound"></param>
        /// <returns></returns>
        public static List<Plan_Tactic_Values> GetMQLValueTacticList(List<Plan_Campaign_Program_Tactic> PlanTacticList, bool isRound = true)
        {
            List<TacticStageValue> tacticStageList = GetTacticStageRelation(PlanTacticList, false);
            List<Plan_Tactic_Values> TacticMQLList = (from tactic in tacticStageList
                                                      select new Plan_Tactic_Values
                                                      {
                                                          PlanTacticId = tactic.TacticObj.PlanTacticId,
                                                          MQL = isRound ? Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero) : tactic.MQLValue,
                                                          Revenue = tactic.RevenueValue
                                                      }).ToList();
            return TacticMQLList;
        }

        /// <summary>
        /// Get Tactic & its Model based on Tactic StartDate & Model Effective Date.
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public static List<TacticModelRelation> GetTacticModelRelationList(List<Plan_Campaign_Program_Tactic> tlist, List<TacticPlanRelation> tacticPlanList)
        {
            MRPEntities dbStage = new MRPEntities();
            var planids = tacticPlanList.Select(t => t.PlanId).Distinct().ToList();
            var modelids = dbStage.Plans.Where(p => planids.Contains(p.PlanId)).Select(p => p.ModelId).Distinct();
            var ModelList = dbStage.Models.Where(m => m.IsDeleted == false);
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            foreach (var modelid in modelids)
            {
                int? ModelId = (int)modelid;
                while (ModelId != null)
                {
                    var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                    modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                    ModelId = model.ParentModelId;
                }
            }

            List<TacticModelRelation> tacticModellist = (from t in tlist
                                                         select new TacticModelRelation
                                                         {
                                                             PlanTacticId = t.PlanTacticId,
                                                             ModelId = GetModelIdFromList(modelDateList, t.StartDate, t.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId)
                                                         }).ToList();

            return tacticModellist;
        }

        #endregion

        #region "Improvement Customized Stage"

        /// <summary>
        /// Added By Bhavesh
        /// Calulate Value for Improvement Suggested
        /// </summary>
        /// <param name="bestInClassStageRelation"></param>
        /// <param name="stageListType"></param>
        /// <param name="modelDateList"></param>
        /// <param name="ModelId"></param>
        /// <param name="modleStageRelationList"></param>
        /// <param name="improvementTacticTypeMetric"></param>
        /// <param name="marketingActivities"></param>
        /// <param name="improvementActivities"></param>
        /// <param name="stageList"></param>
        /// <returns></returns>
        public static List<TacticStageValue> GetTacticStageValueListForSuggestedImprovement(List<StageRelation> bestInClassStageRelation, List<StageList> stageListType, List<ModelDateList> modelDateList, int ModelId, List<ModelStageRelationList> modleStageRelationList, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, List<Plan_Campaign_Program_Tactic> marketingActivities, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<Stage> stageList)
        {
            List<TacticStageValue> tacticStageList = new List<TacticStageValue>();
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = stageList.Where(s => s.Code.Equals(stageINQ)).Select(s => s.Level.Value).FirstOrDefault();
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = stageList.Where(s => s.Code.Equals(stageMQL)).Select(s => s.Level.Value).FirstOrDefault();
            string stageCW = Enums.Stage.CW.ToString();
            int levelCW = stageList.Where(s => s.Code.Equals(stageCW)).Select(s => s.Level.Value).FirstOrDefault();
            List<int> inqStagelist = new List<int>();
            List<int> mqlStagelist = new List<int>();
            List<int> cwStagelist = new List<int>();
            List<int> revenueStagelist = new List<int>();

            List<int> inqVelocityStagelist = stageList.Where(s => s.Level >= 1 && s.Level < levelINQ).Select(s => s.StageId).ToList();
            List<int> mqlVelocityStagelist = stageList.Where(s => s.Level >= levelINQ && s.Level < levelMQL).Select(s => s.StageId).ToList();
            List<int> cwVelocityStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();

            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            string Size = Enums.StageType.Size.ToString();
            foreach (Plan_Campaign_Program_Tactic tactic in marketingActivities)
            {
                List<Plan_Improvement_Campaign_Program_Tactic> improvementList = improvementActivities.Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
                List<StageRelation> stageRelation = CalculateStageValueForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, ModelId, modleStageRelationList, improvementList, improvementTacticTypeMetric, true);
                int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                inqStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelINQ).Select(s => s.StageId).ToList();
                mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                revenueStagelist = stageList.Where(s => (s.Level >= projectedStageLevel && s.Level <= levelCW) || s.Level == null).Select(s => s.StageId).ToList();

                TacticStageValue tacticStageValueObj = new TacticStageValue();
                tacticStageValueObj.TacticObj = tactic;
                if (inqStagelist.Count > 0)
                {
                    tacticStageValueObj.INQValue = projectedStageLevel <= levelINQ ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => inqStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                }
                if (mqlStagelist.Count > 0)
                {
                    tacticStageValueObj.MQLValue = projectedStageLevel <= levelMQL ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => mqlStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                }
                if (cwStagelist.Count > 0)
                {
                    tacticStageValueObj.CWValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => cwStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                }
                if (revenueStagelist.Count > 0)
                {
                    tacticStageValueObj.RevenueValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => revenueStagelist.Contains(sr.StageId) && (sr.StageType == CR || sr.StageType == Size)).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                }
                if (inqVelocityStagelist.Count > 0)
                {
                    tacticStageValueObj.INQVelocity = stageRelation.Where(sr => inqVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                }
                if (mqlVelocityStagelist.Count > 0)
                {
                    tacticStageValueObj.MQLVelocity = stageRelation.Where(sr => mqlVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                }
                if (cwVelocityStagelist.Count > 0)
                {
                    tacticStageValueObj.CWVelocity = stageRelation.Where(sr => cwVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                }
                tacticStageValueObj.ADSValue = stageRelation.Where(sr => sr.StageType == Size).Sum(sr => sr.Value);
                tacticStageList.Add(tacticStageValueObj);
            }
            return tacticStageList;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Improvement Value calculation
        /// </summary>
        /// <param name="bestInClassStageRelation"></param>
        /// <param name="stageList"></param>
        /// <param name="modelDateList"></param>
        /// <param name="ModelId"></param>
        /// <param name="modleStageRelationList"></param>
        /// <param name="improvementActivities"></param>
        /// <param name="improvementTacticTypeMetric"></param>
        /// <param name="stageType"></param>
        /// <param name="isIncludeImprovement"></param>
        /// <returns></returns>
        public static double GetCalculatedValueImprovement(List<StageRelation> bestInClassStageRelation, List<StageList> stageList, List<ModelDateList> modelDateList, int ModelId, List<ModelStageRelationList> modleStageRelationList, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, string stageType, bool isIncludeImprovement = true)
        {
            List<StageRelation> stagevalueList = CalculateStageValueForSuggestedImprovement(bestInClassStageRelation, stageList, modelDateList, ModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, isIncludeImprovement);
            double returnValue = 0;
            if (stagevalueList.Count > 0)
            {
                returnValue = stagevalueList.Where(s => s.StageType == stageType).Sum(s => s.Value);
            }
            return returnValue;
        }

        /// <summary>
        /// Function to calculate improved velocity.
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <returns>Returns improved velocity.</returns>
        public static List<StageRelation> CalculateStageValueForSuggestedImprovement(List<StageRelation> bestInClassStageRelation, List<StageList> stageList, List<ModelDateList> modelDateList, int ModelId, List<ModelStageRelationList> modleStageRelationList, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, bool isIncludeImprovement = true)
        {
            if (improvementActivities.Count > 0)
            {
                //// Get Model id based on effective date From.
                ModelId = GetModelIdFromList(modelDateList, improvementActivities.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), ModelId);
            }
            List<StageRelation> stageModelRelation = modleStageRelationList.Where(m => m.ModelId == ModelId).Select(m => m.StageList).FirstOrDefault();
            List<StageRelation> finalStageResult = new List<StageRelation>();
            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0 && isIncludeImprovement)
            {
                var improvementTypeList = improvementActivities.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                var improvementIdsWeighList = improvementTacticTypeMetric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId)).Select(imptype => imptype).ToList();

                foreach (StageList stage in stageList)
                {
                    StageRelation stageRelationObj = new StageRelation();
                    var stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Weight > 0).ToList();
                    double impcount = stageimplist.Count();
                    double impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
                    double improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Where(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
                    stageRelationObj.StageId = stage.StageId;
                    stageRelationObj.StageType = stage.StageType;
                    stageRelationObj.Value = improvementValue;
                    finalStageResult.Add(stageRelationObj);
                }
            }
            else
            {
                finalStageResult = stageModelRelation;
            }

            return finalStageResult;
        }

        /// <summary>
        /// Added By Bhavesh
        /// Get Model From List
        /// </summary>
        /// <param name="modelDateList"></param>
        /// <param name="StartDate"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        public static int GetModelIdFromList(List<ModelDateList> modelDateList, DateTime StartDate, int ModelId)
        {
            DateTime? effectiveDate = modelDateList.Where(m => m.ModelId == ModelId).Select(m => m.EffectiveDate).SingleOrDefault();
            if (effectiveDate != null)
            {
                if (StartDate >= effectiveDate)
                {
                    return ModelId;
                }
                else
                {
                    int? ParentModelId = modelDateList.Where(m => m.ModelId == ModelId).Select(m => m.ParentModelId).SingleOrDefault();
                    if (ParentModelId != null)
                    {
                        return GetModelIdFromList(modelDateList, StartDate, (int)ParentModelId);
                    }
                    else
                    {
                        return ModelId;
                    }
                }
            }
            else
            {
                return ModelId;
            }
        }

        #endregion

        #region Subordinares and peers

        /// <summary>
        /// Function to get all own and peers subordinates of current user
        /// Added By Dharmraj for ticket #538, 23-06-2014
        /// </summary>
        /// <returns></returns>
        public static List<Guid> GetSubOrdinatesWithPeers()
        {
            //Get all subordinates of current user
            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
            lstUserHierarchy = objBDSService.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
            var lstSubOrdinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId)
                                                        .ToList()
                                                        .Select(u => u.UserId)
                                                        .ToList();
            var ManagerId = lstUserHierarchy.FirstOrDefault(u => u.UserId == Sessions.User.UserId).ManagerId;
            if (ManagerId != null)
            {
                var lstPeersId = lstUserHierarchy.Where(u => u.ManagerId == ManagerId)
                                                        .ToList()
                                                        .Select(u => u.UserId)
                                                        .ToList();
                if (lstPeersId.Count > 0)
                {
                    var lstPeersSubOrdinatesId = lstUserHierarchy.Where(u => lstPeersId.Contains(u.ManagerId.GetValueOrDefault(Guid.Empty)))
                                                            .ToList()
                                                            .Select(u => u.UserId)
                                                            .ToList();

                    // Get current user permission for Tactic ApproveForPeers.
                    bool IsTacticApproveForPeersAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticApproveForPeers);

                    if (IsTacticApproveForPeersAuthorized)
                    {
                        lstSubOrdinates = lstSubOrdinates.Concat(lstPeersSubOrdinatesId).ToList();
                    }
                }
            }

            return lstSubOrdinates;
        }

        /// <summary>
        /// Function to get all own upto n level and peers subordinates of current user
        /// Added By Dharmraj for ticket #573, 4-07-2014
        /// </summary>
        /// <returns>returns list of userIds</returns>
        public static List<Guid> GetSubOrdinatesWithPeersNLevel()
        {
            //// Get all subordinates of current user
            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
            lstUserHierarchy = objBDSService.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
            List<Guid> lstSubordinatesId = new List<Guid>();
            //// Get list of subordinates of current logged in user
            List<Guid> lstSubOrdinates = lstUserHierarchy.Where(user => user.ManagerId == Sessions.User.UserId)
                                                         .Select(user => user.UserId).Distinct().ToList();    // Modified by Sohel Pathan on 13/08/2014 for PL ticket #689

            while (lstSubOrdinates.Count > 0)
            {
                lstSubOrdinates.ForEach(user => lstSubordinatesId.Add(user));
                lstSubOrdinates = lstUserHierarchy.Where(user => lstSubOrdinates.Contains(user.ManagerId.GetValueOrDefault(Guid.Empty))).Select(user => user.UserId).Distinct().ToList();
            }

            //// Get current user permission for Tactic ApproveForPeers.
            bool IsTacticApproveForPeersAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticApproveForPeers);
            List<Guid> lstUpperMostLevelManagers = new List<Guid>();    // Added by Sohel Pathan on 13/08/2014 for PL ticket #689
            //// Get ManagerId of current logged in user
            var ManagerId = lstUserHierarchy.FirstOrDefault(user => user.UserId == Sessions.User.UserId).ManagerId;
            if (ManagerId != null)
            {
                //// Get list of peers of manager of current logged in user 
                var lstPeersId = lstUserHierarchy.Where(user => user.ManagerId == ManagerId).Select(user => user.UserId).Distinct().ToList();

                if (lstPeersId.Count > 0)
                {
                    //// Get list of subordinates for above selected peers
                    var lstPeersSubOrdinatesId = lstUserHierarchy.Where(user => lstPeersId.Contains(user.ManagerId.GetValueOrDefault(Guid.Empty)))
                                                            .Select(user => user.UserId).Distinct().ToList(); // Modified by Sohel Pathan on 13/08/2014 for PL ticket #689

                    if (IsTacticApproveForPeersAuthorized)
                    {
                        lstSubordinatesId = lstSubordinatesId.Concat(lstPeersSubOrdinatesId).Distinct().ToList();   // Modified by Sohel Pathan on 13/08/2014 for PL ticket #689
                        lstSubordinatesId = lstSubordinatesId.Concat(lstPeersId).Distinct().ToList(); /// Added by Sohel Pathan on 08/08/201 for PL ticket #689.
                    }
                }
            }
            //// Start - Added by Sohel Pathan on 13/08/2014 for PL ticket #689
            else
            {
                if (IsTacticApproveForPeersAuthorized)
                {
                    //// Get list of user who dont have manager (i.e. most Upper level users)
                    lstUpperMostLevelManagers = lstUserHierarchy.Where(user => user.ManagerId == null).Select(user => user.UserId).Distinct().ToList();
                    if (lstUpperMostLevelManagers.Count > 0)
                    {
                        lstSubordinatesId = lstSubordinatesId.Concat(lstUpperMostLevelManagers).Distinct().ToList();

                        List<Guid> lstSubOrdinatesOfUpperMostLevelManagers = lstUserHierarchy.Where(user => lstUpperMostLevelManagers.Contains(user.ManagerId.GetValueOrDefault(Guid.Empty)))
                                                                                .Select(user => user.UserId).Distinct().ToList();

                        while (lstSubOrdinatesOfUpperMostLevelManagers.Count > 0)
                        {
                            lstSubOrdinatesOfUpperMostLevelManagers.ForEach(user => lstSubordinatesId.Add(user));
                            lstSubOrdinatesOfUpperMostLevelManagers = lstUserHierarchy.Where(user => lstSubOrdinatesOfUpperMostLevelManagers.Contains(user.ManagerId.GetValueOrDefault(Guid.Empty))).Select(user => user.UserId).Distinct().ToList();
                        }
                    }
                }
            }
            //// End - Added by Sohel Pathan on 13/08/2014 for PL ticket #689

            //// Start :Added by Mitesh Vaishnav for PL ticket #688 and #689
            bool IsTacticApproveOwn = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticApproveOwn);
            if (IsTacticApproveOwn)
            {
                lstSubordinatesId.Add(Sessions.User.UserId);
            }
            else
            {
                //// Added by Sohel Pathan on 13/08/2014 for PL ticket #689
                lstSubordinatesId.Remove(Sessions.User.UserId);
            }
            //// End :Added by Mitesh Vaishnav for PL ticket #688 and #689

            return lstSubordinatesId.Distinct().ToList();   //// Modified by Sohel Pathan on 13/08/2014 for PL ticket #689
        }

        /// <summary>
        /// Function to check for campaign/Program is approvable with given subordinates
        /// </summary>
        /// Created by Mitesh Vaishnav on 11/08/2014 for PL ticket #688 and #689 
        /// <param name="lstSubordinates"></param>
        /// <param name="id"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static bool IsSectionApprovable(List<Guid> lstSubordinates, int id, string section)
        {
            bool IsApprovable = false;
            MRPEntities db = new MRPEntities();
            if (section == Enums.Section.Campaign.ToString())
            {
                var AllTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted == false).ToList();
                if (AllTactic.Count > 0)
                {
                    var OthersTactic = AllTactic.Where(t => !lstSubordinates.Contains(t.CreatedBy)).ToList();
                    if (OthersTactic.Count == 0)
                    {
                        IsApprovable = true;
                    }

                }
            }
            else if (section == Enums.Section.Program.ToString())
            {
                var AllTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanProgramId == id && t.IsDeleted == false).ToList();
                if (AllTactic.Count > 0)
                {
                    var OthersTactic = AllTactic.Where(t => !lstSubordinates.Contains(t.CreatedBy)).ToList();
                    if (OthersTactic.Count == 0)
                    {
                        IsApprovable = true;
                    }

                }
            }
            return IsApprovable;

        }


        /// <summary>
        /// Function to get all subirdinates upto n level
        /// Added by dharmraj, 1-7-2014
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<Guid> GetAllSubordinates(Guid userId)
        {
            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
            lstUserHierarchy = objBDSService.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
            List<Guid> lstUserId = new List<Guid>();

            List<Guid> lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == userId)
                                                         .ToList()
                                                         .Select(u => u.UserId).ToList();


            while (lstSubordinates.Count > 0)
            {
                lstSubordinates.ForEach(u => lstUserId.Add(u));

                lstSubordinates = lstUserHierarchy.Where(u => lstSubordinates.Contains(u.ManagerId.GetValueOrDefault(Guid.Empty))).ToList().Select(u => u.UserId).ToList();
            }


            return lstUserId;
        }

        /// <summary>
        /// Function to get immediate subirdinates
        /// Added by dharmraj, 1-7-2014
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<Guid> GetSubordinates(Guid userId)
        {
            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
            lstUserHierarchy = objBDSService.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);

            var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == userId)
                                                          .ToList()
                                                          .Select(u => u.UserId).ToList();
            return lstSubordinates;

        }

        #endregion

        #region Custom Restriction

        /// <summary>
        /// Returns all custom restrictions for current user.
        /// Added By Dharmraj mangukiya, #538
        /// </summary>
        /// <returns></returns>
        public static List<UserCustomRestrictionModel> GetUserCustomRestriction()
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSServiceClient();
            var listCustomRestriction = objBDSServiceClient.GetUserCustomRestrictionList(Sessions.User.UserId, Sessions.ApplicationId).Select(c => new UserCustomRestrictionModel
                                                                                                                        {
                                                                                                                            CustomField = c.CustomField,
                                                                                                                            CustomFieldId = c.CustomFieldId,
                                                                                                                            Permission = c.Permission
                                                                                                                        }).ToList();
            ////Modified by Mitesh Vaishnav for PL ticket 819 BDS QA: New User Cannot Log In if Permissions Are Not Set
            ////If list has not custom restriction for user's Business unit than add user's business unit with View/Edit permission
            if (!listCustomRestriction.Any(a => a.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString() && a.CustomFieldId.ToLower() == Sessions.User.BusinessUnitId.ToString().ToLower()))
            {
                UserCustomRestrictionModel objUserCustomRestriction = new UserCustomRestrictionModel();
                objUserCustomRestriction.CustomField = Enums.CustomRestrictionType.BusinessUnit.ToString();
                objUserCustomRestriction.CustomFieldId = Sessions.User.BusinessUnitId.ToString();
                objUserCustomRestriction.Permission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                listCustomRestriction.Add(objUserCustomRestriction);
            }
            return listCustomRestriction;

        }

        public static bool GetRightsForTactic(List<UserCustomRestrictionModel> lstUserCustomRestriction, int verticalId, Guid geographyId)
        {
            bool returnValue = true;
            if (lstUserCustomRestriction.Where(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Geography.ToString().ToLower() && r.CustomFieldId.ToLower() == geographyId.ToString().ToLower()).Count() > 0)
            {
                if (lstUserCustomRestriction.Single(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Geography.ToString().ToLower() && r.CustomFieldId.ToLower() == geographyId.ToString().ToLower()).Permission != (int)Enums.CustomRestrictionPermission.ViewEdit)
                {
                    returnValue = false;
                }
            }

            if (lstUserCustomRestriction.Where(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Verticals.ToString().ToLower() && r.CustomFieldId.ToLower() == verticalId.ToString().ToLower()).Count() > 0)
            {
                if (lstUserCustomRestriction.Single(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Verticals.ToString().ToLower() && r.CustomFieldId == verticalId.ToString().ToLower()).Permission != (int)Enums.CustomRestrictionPermission.ViewEdit)
                {
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public static bool GetRightsForTacticVisibility(List<UserCustomRestrictionModel> lstUserCustomRestriction, int verticalId, Guid geographyId)
        {
            bool returnValue = true;
            if (lstUserCustomRestriction.Where(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Geography.ToString().ToLower() && r.CustomFieldId.ToLower() == geographyId.ToString().ToLower()).Count() > 0)
            {
                if (lstUserCustomRestriction.Single(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Geography.ToString().ToLower() && r.CustomFieldId.ToLower() == geographyId.ToString()).Permission == (int)Enums.CustomRestrictionPermission.None)
                {
                    returnValue = false;
                }
            }
            if (lstUserCustomRestriction.Where(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Verticals.ToString().ToLower() && r.CustomFieldId.ToLower() == verticalId.ToString().ToLower()).Count() > 0)
            {
                if (lstUserCustomRestriction.Single(r => r.CustomField.ToLower() == Enums.CustomRestrictionType.Verticals.ToString().ToLower() && r.CustomFieldId.ToLower() == verticalId.ToString().ToLower()).Permission == (int)Enums.CustomRestrictionPermission.None)
                {
                    returnValue = false;
                }
            }

            return returnValue;
        }

        #region Get list of Busieness Unit with ViewOnly and View/Edit rights
        /// <summary>
        /// Get list of Busieness Units with ViewOnly and View/Edit rights 
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>30/06/2014</CreatedDate>
        /// <returns>List of Business units</returns>
        public static List<string> GetViewEditBusinessUnitList(List<UserCustomRestrictionModel> lstUserCustomRestrictionParam = null)
        {
            List<UserCustomRestrictionModel> lstUserCustomRestriction = new List<UserCustomRestrictionModel>();
            if (lstUserCustomRestrictionParam == null)
            {
                lstUserCustomRestriction = Common.GetUserCustomRestriction();
            }
            else
            {
                lstUserCustomRestriction = lstUserCustomRestrictionParam;
            }
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            return lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId).ToList();
        }
        #endregion

        #region Check ViewEdit rights on businessUnit
        /// <summary>
        /// Check wheather ViewEdit rights on businessUnit is given otr not - for custom restriction
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>02/07/2014</CreatedDate>
        /// <param name="businessUnitId">businessUnit id</param>
        /// <returns>returns flag value</returns>
        public static bool IsBusinessUnitEditable(Guid businessUnitId)
        {
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedBusinessUnits = lstUserCustomRestriction.Where(customRestriction => customRestriction.Permission == ViewEditPermission && customRestriction.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(customRestriction => customRestriction.CustomFieldId).ToList();
            if (lstAllowedBusinessUnits.Count > 0)
            {
                List<Guid> lstViewEditBusinessUnits = new List<Guid>();
                lstAllowedBusinessUnits.ForEach(businessUnit => lstViewEditBusinessUnits.Add(Guid.Parse(businessUnit)));
                return lstViewEditBusinessUnits.Contains(businessUnitId);
            }
            return true;
        }
        #endregion

        #endregion

        #region Genrate Random Numbers

        /// <summary>
        /// Added By : Kalpesh Sharma 
        /// Generate a Random numbers 
        /// #453: Support request Issue field needs to be bigger
        /// </summary>
        /// <returns></returns>

        public static string GenerateRandomNumber()
        {
            Random rnd = new Random();
            int number1 = rnd.Next(1, 156000);
            int number2 = rnd.Next(1, 3562452);
            Int64 n = Int64.Parse(DateTime.Now.ToString("yyyymmddhhMMss"));
            return Convert.ToString(number1 + number2 + n);
        }

        #endregion

        #region Budget Calculation

        #region Get list of Goal Types
        /// <summary>
        /// Get list of Goal Types
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>14/07/2014</CreatedDate>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static SelectList GetGoalTypeList(Guid clientId)
        {
            MRPEntities db = new MRPEntities();
            var lstGoalTypes = Enum.GetValues(typeof(Enums.PlanGoalType)).Cast<Enums.PlanGoalType>().Select(a => a.ToString()).ToList();
            var lstGoalTypeListFromDB = db.Stages.Where(a => a.IsDeleted == false && a.ClientId == clientId && lstGoalTypes.Contains(a.Code)).Select(a => a).ToList();
            // new SelectListItem { Text = a.Title, Value = a.Code }).ToList();
            lstGoalTypeListFromDB.ForEach(a => a.Title = a.Title.ToLower());
            Stage objStage = new Stage();
            objStage.Title = Enums.PlanGoalTypeList[Enums.PlanGoalType.Revenue.ToString()].ToString().ToLower();
            objStage.Code = Enums.PlanGoalTypeList[Enums.PlanGoalType.Revenue.ToString()].ToString().ToUpper();
            lstGoalTypeListFromDB.Add(objStage);
            return new SelectList(lstGoalTypeListFromDB, "Code", "Title");
        }

        #endregion

        #region Calculated Budget parameters
        /// <summary>
        /// Calculate INQ, MQL and Revenue based on one there supplied.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>15/07/2014</CreatedDate>
        /// <param name="modelId">Model id of selected plan</param>
        /// <param name="goalType">goal type of selected plan</param>
        /// <param name="goalValue">goal value for goal type of selected plan</param>
        /// <returns>return BudgetAllocationModel object</returns>
        public static BudgetAllocationModel CalculateBudgetInputs(int modelId, string goalType, string goalValue, double averageDealSize)
        {
            BudgetAllocationModel objBudgetAllocationModel = new BudgetAllocationModel();
            try
            {
                MRPEntities dbStage = new MRPEntities();
                List<Stage> stageList = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                string stageINQ = Enums.Stage.INQ.ToString();
                int levelINQ = stageList.Single(s => s.Code.Equals(stageINQ)).Level.Value;
                string stageMQL = Enums.Stage.MQL.ToString();
                int levelMQL = stageList.Single(s => s.Code.Equals(stageMQL)).Level.Value;
                string stageCW = Enums.Stage.CW.ToString();
                int levelCW = stageList.Single(s => s.Code.Equals(stageCW)).Level.Value;

                List<int> mqlStagelist = new List<int>();
                List<int> cwStagelist = new List<int>();

                string CR = Enums.StageType.CR.ToString();
                string marketing = Enums.Funnel.Marketing.ToString();
                var ModelFunnelStageList = dbStage.Model_Funnel_Stage.Where(mfs => mfs.Model_Funnel.ModelId == modelId && mfs.StageType == CR && mfs.Model_Funnel.Funnel.Title == marketing).ToList();

                if (goalType != "" && goalValue != "" && goalValue != "0")
                {
                    double inputValue = Convert.ToInt64(goalValue.Trim().Replace(",", "").Replace("$", ""));

                    if (goalType == Enums.PlanGoalType.INQ.ToString())
                    {
                        // Calculate MQL
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = ModelFunnelStageList.Where(mfs => mqlStagelist.Contains(mfs.StageId)).ToList();
                        double MQLValue = (inputValue) * (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                        MQLValue = (MQLValue.Equals(double.NaN) || MQLValue.Equals(double.NegativeInfinity) || MQLValue.Equals(double.PositiveInfinity)) ? 0 : MQLValue;  // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.MQLValue = MQLValue;

                        // Calculate Revenue
                        cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double RevenueValue = (inputValue) * (modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize;
                        RevenueValue = (RevenueValue.Equals(double.NaN) || RevenueValue.Equals(double.NegativeInfinity) || RevenueValue.Equals(double.PositiveInfinity)) ? 0 : RevenueValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.RevenueValue = RevenueValue;
                    }
                    else if (goalType == Enums.PlanGoalType.MQL.ToString())
                    {
                        // Calculate INQ
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        var modelFunnelStageListINQ = ModelFunnelStageList.Where(mfs => mqlStagelist.Contains(mfs.StageId)).ToList();
                        double INQValue = (inputValue) / (modelFunnelStageListINQ.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                        INQValue = (INQValue.Equals(double.NaN) || INQValue.Equals(double.NegativeInfinity) || INQValue.Equals(double.PositiveInfinity)) ? 0 : INQValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.INQValue = INQValue;

                        // Calculate Revenue
                        cwStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double RevenueValue = (inputValue) * (modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize; // Modified by Maninder Singh Wadhva on 10/14/2014 for PL ticket #775
                        RevenueValue = (RevenueValue.Equals(double.NaN) || RevenueValue.Equals(double.NegativeInfinity) || RevenueValue.Equals(double.PositiveInfinity)) ? 0 : RevenueValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.RevenueValue = RevenueValue;

                    }
                    else if (goalType == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                    {
                        // Calculate INQ
                        cwStagelist = stageList.Where(s => s.Level >= levelINQ && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double convalue = ((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize);
                        double INQValue = (inputValue) / convalue;
                        INQValue = (INQValue.Equals(double.NaN) || INQValue.Equals(double.NegativeInfinity) || INQValue.Equals(double.PositiveInfinity)) ? 0 : INQValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.INQValue = INQValue;

                        // Calculate MQL
                        cwStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double MQLValue = (inputValue) / (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)) * averageDealSize); // Modified by Sohel Pathan on 12/09/2014 for PL ticket #775
                        MQLValue = (MQLValue.Equals(double.NaN) || MQLValue.Equals(double.NegativeInfinity) || MQLValue.Equals(double.PositiveInfinity)) ? 0 : MQLValue;  // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.MQLValue = MQLValue;
                    }
                }
                return objBudgetAllocationModel;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }
        #endregion

        #region Calculate MQL only
        /// <summary>
        /// Calculate MQL only on the base of INQ and Revenue value
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>16/07/2014</CreatedDate>
        /// <param name="modelId"></param>
        /// <param name="goalType"></param>
        /// <param name="goalValue"></param>
        /// <param name="averageDealSize"></param>
        /// <returns></returns>
        public static double CalculateMQLOnly(List<int> funnelids, string goalType, string goalValue, double averageDealSize, List<Stage> stageList, List<Model_Funnel_Stage> modelFunnelStageList)
        {
            try
            {
                string stageINQ = Enums.Stage.INQ.ToString();
                int levelINQ = stageList.Single(stage => stage.Code.Equals(stageINQ)).Level.Value;
                string stageMQL = Enums.Stage.MQL.ToString();
                int levelMQL = stageList.Single(stage => stage.Code.Equals(stageMQL)).Level.Value;
                string stageCW = Enums.Stage.CW.ToString();
                int levelCW = stageList.Single(stage => stage.Code.Equals(stageCW)).Level.Value;

                List<int> mqlStagelist = new List<int>();
                List<int> cwStagelist = new List<int>();

                if (goalType != "" && goalValue != "" && goalValue != "0")
                {
                    string CR = Enums.StageType.CR.ToString();
                    double inputValue = Convert.ToInt64(goalValue.Trim().Replace(",", "").Replace("$", ""));
                    string marketing = Enums.Funnel.Marketing.ToString();
                    if (goalType.Equals(Enums.PlanGoalType.INQ.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //// Calculate MQL
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = modelFunnelStageList.Where(mfs => funnelids.Contains(mfs.ModelFunnelId) && mqlStagelist.Contains(mfs.StageId)).ToList();
                        double MQLValue = (inputValue) * (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                        // Start - Modified by Sohel Pathan on 12/12/2014 for PL ticket #975, NegativeInfinity and PositiveInfinity check has been added
                        MQLValue = (MQLValue.Equals(double.NaN) || MQLValue.Equals(double.NegativeInfinity) || MQLValue.Equals(double.PositiveInfinity)) ? 0 : MQLValue;  // Added by Viral Kadiya on 11/24/2014 to resolve PL ticket #990.
                        // End - Modified by Sohel Pathan on 12/12/2014 for PL ticket #975, NegativeInfinity and PositiveInfinity check has been added
                        return MQLValue;
                    }
                    else if (goalType.Equals(Enums.PlanGoalType.Revenue.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //// Calculate INQ
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = modelFunnelStageList.Where(mfs => funnelids.Contains(mfs.ModelFunnelId) && cwStagelist.Contains(mfs.StageId)).ToList();
                        double INQValue = (inputValue) / ((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize);
                        INQValue = (INQValue.Equals(double.NaN) || INQValue.Equals(double.NegativeInfinity) || INQValue.Equals(double.PositiveInfinity)) ? 0 : INQValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975

                        // Calculate MQL
                        var modelFunnelStageListMQL = modelFunnelStageList.Where(mfs => funnelids.Contains(mfs.ModelFunnelId) && mqlStagelist.Contains(mfs.StageId)).ToList();
                        double MQLValue = (INQValue) * (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                        // Start - Modified by Sohel Pathan on 12/12/2014 for PL ticket #975, NegativeInfinity and PositiveInfinity check has been added
                        MQLValue = (MQLValue.Equals(double.NaN) || MQLValue.Equals(double.NegativeInfinity) || MQLValue.Equals(double.PositiveInfinity)) ? 0 : MQLValue;  // Added by Viral Kadiya on 11/24/2014 to resolve PL ticket #990.
                        // End - Modified by Sohel Pathan on 12/12/2014 for PL ticket #975, NegativeInfinity and PositiveInfinity check has been added
                        return MQLValue;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return 0;
            }
        }
        #endregion

        #region Get AllocationBy list
        /// <summary>
        /// Get list of AllocationBy values
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>05/08/2014</CreatedDate>
        /// <returns></returns>
        public static List<SelectListItem> GetAllocatedByList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString(), Value = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString(), Value = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString(), Selected = true });
            items.Add(new SelectListItem { Text = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString(), Value = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString(), Selected = false });
            return items;
        }
        #endregion

        #endregion

        #region Custom Fields

        /// <summary>
        /// Function for get custom fields of tactic,program or campaign.
        /// Added by Mitesh Vaishnav on 12/09/2014
        /// #718 - Custom fields for Campaigns
        /// </summary>
        /// <param name="id">Plan Tactic Id or Plan Campaign Id or Plan Program Id</param>
        /// <param name="section">Perameter contains value from enum like Campaign or Program or Tactic Section</param>
        /// <returns></returns>
        public static List<CustomFieldModel> GetCustomFields(int id, string section)
        {
            MRPEntities db = new MRPEntities();

            var lstCustomFields = db.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == Sessions.User.ClientId && customField.IsDeleted == false).ToList().Select(a => new CustomFieldModel
            {
                customFieldId = a.CustomFieldId,
                name = a.Name,
                customFieldType = a.CustomFieldType.Name,
                description = a.Description,
                isRequired = a.IsRequired,
                entityType = a.EntityType,
                value = a.CustomField_Entity.Where(ct => ct.EntityId == id).FirstOrDefault() != null ? a.CustomField_Entity.Where(ct => ct.EntityId == id).FirstOrDefault().Value : null,
                option = a.CustomFieldOptions.ToList().Select(o => new CustomFieldOptionModel
                {
                    customFieldOptionId = o.CustomFieldOptionId,
                    value = o.Value
                }).ToList()

            }).ToList();
            return lstCustomFields;
        }
        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #718 
        /// Function for truncate length of input string
        /// </summary>
        /// <param name="input">input string</param>
        /// <param name="length">length of string</param>
        /// <returns>if string contains less or equal length than returns input string else returns substring with ... </returns>
        public static string TruncateLable(string input, int length)
        {
            if (input.Length <= length)
            {
                return input;
            }
            else
            {
                return input.Substring(0, length) + "...";
            }
        }

        #endregion

        #region Plan Gantt Types
        /// <summary>
        /// Fetch the default Plan Gantt Types
        /// </summary>
        /// <param name="lstTactic">list of plan tactics</param>
        /// <returns>List of ViewByModel</returns>
        public static List<ViewByModel> GetDefaultGanttTypes(List<Plan_Tactic> lstTactic = null)
        {
            //// Initialize the default Plan Gantt Types
            List<ViewByModel> lstViewByTab = new List<ViewByModel>();
            lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Tactic.ToString(), Value = PlanGanttTypes.Tactic.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Vertical.ToString(), Value = PlanGanttTypes.Vertical.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Request.ToString(), Value = PlanGanttTypes.Request.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Stage.ToString(), Value = PlanGanttTypes.Stage.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = BusinessUnit.ToString(), Value = PlanGanttTypes.BusinessUnit.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = PlanGanttTypes.Audience.ToString() });
            lstViewByTab = lstViewByTab.Where(viewBy => !string.IsNullOrEmpty(viewBy.Text)).OrderBy(viewBy => viewBy.Text, new AlphaNumericComparer()).ToList();

            //// Check that if list of PlanTactic is not null then we are going to fetch the Custom Fields
            if (lstTactic != null)
            {
                if (lstTactic.Count > 0)
                {
                    var campaignId = lstTactic.Select(tactic => tactic.PlanCampaignId).ToList();
                    var programId = lstTactic.Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList();
                    var lstcustomfields = GetCustomFields(lstTactic.Select(tactic => tactic.objPlanTactic.PlanTacticId).ToList(),programId,campaignId);

                    //// Concat the Default list with newly fetched custom fields. 
                    lstViewByTab = lstViewByTab.Concat(lstcustomfields).ToList();
                }
            }

            return lstViewByTab;
        }

        /// <summary>
        /// Fetch the Custom fields based upon it's PlanTactic id
        /// </summary>
        /// <param name="planTacticIds">List of Plan Tactic id</param>
        /// <returns>List of ViewbyModel</returns>
        public static List<ViewByModel> GetTacticsCustomFields(List<int> planTacticIds)
        {
            MRPEntities db = new MRPEntities();
            List<ViewByModel> lstCustomFieldsViewByTab = new List<ViewByModel>();
            if (planTacticIds == null)
            {
                planTacticIds = new List<int>();
            }

            //Process and fetch the Custom Fields with EntityType is Tactic and PlanTactic id.
            var CustomFields = (from cf in db.CustomFields
                                join cfe in db.CustomField_Entity on cf.CustomFieldId equals cfe.CustomFieldId
                                join t in db.Plan_Campaign_Program_Tactic on cfe.EntityId equals t.PlanTacticId
                                where cf.IsDeleted == false && t.IsDeleted == false && cf.EntityType == "Tactic" && cf.ClientId == Sessions.User.ClientId && planTacticIds.Contains(t.PlanTacticId)
                                select cf).ToList().Distinct().ToList().OrderBy(cf => cf.Name).ToList();

            //ittrate the custom fields and insert into the temp list
            foreach (var item in CustomFields)
            {
                lstCustomFieldsViewByTab.Add(new ViewByModel { Text = item.Name.ToString(), Value = string.Format("{0}{1}", CustomTitle, item.CustomFieldId.ToString()) });
            }
            return lstCustomFieldsViewByTab;
        }


        /// <summary>
        /// Fetch the Custom fields based upon it's Custom field type and Id collection
        /// </summary>
        /// <param name="planTacticIds">List of id</param>
        /// <param name="type">Section name(Like Campaign,Program and Tactic)</param>
        /// <returns>List of ViewbyModel</returns>
        public static List<ViewByModel> GetCustomFields(List<int> tacticids, List<int> programids, List<int> campaignids)
        {
            MRPEntities db = new MRPEntities();
            List<ViewByModel> lstCustomFieldsViewByTab = new List<ViewByModel>();
            string CampaignCustomText = Enums.EntityType.Campaign.ToString(),
                ProgramCustomText = Enums.EntityType.Program.ToString(),
                TacticCustomText = Enums.EntityType.Tactic.ToString();

            var customfieldlist = db.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.ClientId && customfield.IsDeleted == false && customfield.IsDisplayForFilter == true).ToList(); //Modified by Mitesh for PL ticket 1020 (add filter of IsDisplayForFilter)
            var customfieldentity = (from customfield in customfieldlist
                                     join cfe in db.CustomField_Entity on customfield.CustomFieldId equals cfe.CustomFieldId
                                     select cfe).ToList();

            var campaigncustomids = customfieldentity.Where(cfe => campaignids.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabCampaign = customfieldlist.Where(cf => cf.EntityType == CampaignCustomText && campaigncustomids.Contains(cf.CustomFieldId)).ToList().Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = CampaignCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabCampaign = lstCustomFieldsViewByTabCampaign.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            var programcustomids = customfieldentity.Where(cfe => programids.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabProgram = customfieldlist.Where(cf => cf.EntityType == ProgramCustomText && programcustomids.Contains(cf.CustomFieldId)).ToList().Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = ProgramCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabProgram = lstCustomFieldsViewByTabProgram.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            var tacticcustomids = customfieldentity.Where(cfe => tacticids.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabTactic = customfieldlist.Where(cf => cf.EntityType == TacticCustomText && tacticcustomids.Contains(cf.CustomFieldId)).ToList().Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = TacticCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabTactic = lstCustomFieldsViewByTabTactic.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            lstCustomFieldsViewByTab = lstCustomFieldsViewByTab.Concat(lstCustomFieldsViewByTabCampaign).Concat(lstCustomFieldsViewByTabProgram).Concat(lstCustomFieldsViewByTabTactic).ToList();

            return lstCustomFieldsViewByTab;
        }


        /// <summary>
        /// Get the list of Tactic by passing the multiple Plan Ids
        /// </summary>
        /// <param name="strPlanIds">String with comma Sepreated plan id's</param>
        /// <returns>Return the list of PlanTactic id's</returns>
        public static List<int> GetTacticByPlanIDs(string strPlanIds)
        {
            List<int> PlanIds = (strPlanIds != "" && strPlanIds != null) ? strPlanIds.Split(',').Select(int.Parse).ToList() : new List<int>();
            MRPEntities db = new MRPEntities();
            List<int> tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic =>
                              PlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                              && tactic.IsDeleted.Equals(false)
                              ).Select(t => t.PlanTacticId).ToList();

            return tacticList;
        }

        #endregion

        #region General Added

        /* added by nirav Shah on 7 Jan 2014*/
        public static List<SelectListItem> GetUpcomingActivity()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisquarter.ToString()].ToString(), Value = Enums.UpcomingActivities.thisquarter.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thismonth.ToString()].ToString(), Value = Enums.UpcomingActivities.thismonth.ToString(), Selected = false });
            return items;
        }

        public static List<SelectListItem> GetBussinessUnitIds(Guid ClientId)
        {
            MRPEntities db = new MRPEntities();
            var list = db.BusinessUnits.Where(s => s.ClientId == ClientId && s.IsDeleted == false).ToList().Select(u => new SelectListItem
            {
                Text = u.Title,
                Value = u.BusinessUnitId.ToString()
            });
            List<SelectListItem> items = new List<SelectListItem>(list);
            return items;
        }

        /// <summary>
        /// Added property to get Debug/Release mode: Manoj Started Bug 203:Performance issue - Home screen taking too long to load
        /// </summary>
        public static bool IsDebug
        {
            get
            {
                CompilationSection compilationSection = (CompilationSection)System.Configuration.ConfigurationManager.GetSection(@"system.web/compilation");
                return compilationSection.Debug;
            }
        }
        /// <summary>
        /// Function to check integration instance of model.If model have atleast one instance, returns true.
        /// Added by Mitesh Vaishnav on 14-Aug-2014 for PL ticket #690
        /// </summary>
        /// <param name="objModel">Model Object</param>
        /// <returns>Returns true for success and false for failure</returns>
        public static bool CheckModelIntegrationExist(Model objModel)
        {
            bool isIntegrationInstanceExist = true;
            if (objModel.IntegrationInstanceId == null && objModel.IntegrationInstanceIdCW == null && objModel.IntegrationInstanceIdINQ == null && objModel.IntegrationInstanceIdMQL == null)
            {
                isIntegrationInstanceExist = false;
            }
            return isIntegrationInstanceExist;
        }

        /// <summary>
        /// Function for Delete Campaign or Program or Tactic or LineItem as per their relational hierarchy.
        /// Added by Mitesh Vaishnav for 18-Aug-2014 for functional review point - removing sp
        /// </summary>
        /// <param name="isDelete">flag for delete</param>
        /// <param name="section">section name</param>
        /// <returns>returns greater than 0 for success and 0 for failure</returns>
        public static int PlanTaskDelete(string section, int id = 0)
        {
            int returnValue = 0;
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        try
                        {
                            //// To increase performance we added this code. By Pratik Chauhan
                            db.Configuration.AutoDetectChangesEnabled = false;
                            //// Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                            if (section == Enums.Section.Plan.ToString() && id != 0)
                            {
                                var PlanId = id;
                                string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                                string entityTypeProgram = Enums.EntityType.Program.ToString();
                                string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                                List<CustomField_Entity> customFieldList = new List<CustomField_Entity>();

                                var lineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false) && lineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId).ToList();
                                lineItemList.ForEach(lineItem => { lineItem.IsDeleted = true; lineItem.ModifiedDate = System.DateTime.Now; lineItem.ModifiedBy = Sessions.User.UserId; });
                                List<int> lineItemsIds = lineItemList.Select(lineItem => lineItem.PlanLineItemId).ToList();
                                var lineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineItemActual => lineItemsIds.Contains(lineItemActual.PlanLineItemId)).ToList();
                                lineItemActualList.ForEach(lineItemActual => db.Entry(lineItemActual).State = EntityState.Deleted);

                                var tacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId && pcpt.IsDeleted == false).ToList();
                                tacticList.ForEach(pcpt => { pcpt.IsDeleted = true; pcpt.ModifiedDate = System.DateTime.Now; pcpt.ModifiedBy = Sessions.User.UserId; });
                                var tacticIds = tacticList.Select(a => a.PlanTacticId).ToList();
                                db.CustomField_Entity.Where(a => tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeTactic).ToList().ForEach(a => customFieldList.Add(a));

                                var programList = db.Plan_Campaign_Program.Where(pcp => pcp.Plan_Campaign.PlanId == PlanId && pcp.IsDeleted == false).ToList();
                                programList.ForEach(pcp => { pcp.IsDeleted = true; pcp.ModifiedDate = System.DateTime.Now; pcp.ModifiedBy = Sessions.User.UserId; });
                                var programIds = programList.Select(a => a.PlanProgramId).ToList();
                                db.CustomField_Entity.Where(a => programIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeProgram).ToList().ForEach(a => customFieldList.Add(a));

                                var campaignList = db.Plan_Campaign.Where(pc => pc.PlanId == PlanId && pc.IsDeleted == false).ToList();
                                campaignList.ForEach(pc => { pc.IsDeleted = true; pc.ModifiedDate = System.DateTime.Now; pc.ModifiedBy = Sessions.User.UserId; });
                                var campaignIds = campaignList.Select(a => a.PlanCampaignId).ToList();
                                db.CustomField_Entity.Where(a => campaignIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeCampaign).ToList().ForEach(a => customFieldList.Add(a));
                                db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false).ToList().ForEach(p => p.IsDeleted = true);

                                customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                            }
                            //// End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                            else if (section == Enums.Section.Campaign.ToString() && id != 0)
                            {
                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId == id).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign_Program.Plan_Campaign.PlanCampaignId == id).ToList();
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign.PlanCampaignId == id).ToList();
                                Plan_Campaign_ProgramList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                var Plan_CampaignList = db.Plan_Campaign.Where(a => a.IsDeleted.Equals(false) && a.PlanCampaignId == id).ToList();
                                Plan_CampaignList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var lineItemIds = plan_campaign_Program_Tactic_LineItemList.Select(a => a.PlanLineItemId).ToList();
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => lineItemIds.Contains(a.PlanLineItemId)).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                ////Start Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns
                                //// when campaign deleted then custom field's value for this campaign and custom field's value of appropriate program and tactic will be deleted
                                var campaign_customFieldList = db.CustomField_Entity.Where(a => a.EntityId == id && a.CustomField.EntityType == section).ToList();
                                campaign_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                var programIds = Plan_Campaign_ProgramList.Select(a => a.PlanProgramId).ToList();
                                string sectionProgram = Enums.EntityType.Program.ToString();
                                var program_customFieldList = db.CustomField_Entity.Where(a => programIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionProgram).ToList();
                                program_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                var tacticIds = Plan_Campaign_Program_TacticList.Select(a => a.PlanTacticId).ToList();
                                string sectionTactic = Enums.EntityType.Tactic.ToString();
                                var tactic_customFieldList = db.CustomField_Entity.Where(a => tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic).ToList();
                                tactic_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                ////End Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns

                            }
                            else if (section == Enums.Section.Program.ToString() && id != 0)
                            {
                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanProgramId == id).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign_Program.PlanProgramId == id).ToList();
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == id).ToList();
                                Plan_Campaign_ProgramList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var lineItemIds = plan_campaign_Program_Tactic_LineItemList.Select(a => a.PlanLineItemId).ToList();
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => lineItemIds.Contains(a.PlanLineItemId)).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                ////Start Added by Mitesh Vaishnav for PL ticket #719 Custom fields for programs
                                //// when program deleted then custom field's value for this program and custom field's value of appropriate tactic will be deleted
                                var program_customFieldList = db.CustomField_Entity.Where(a => a.EntityId == id && a.CustomField.EntityType == section).ToList();
                                program_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                var tacticIds = Plan_Campaign_Program_TacticList.Select(a => a.PlanTacticId).ToList();
                                string sectionTactic = Enums.EntityType.Tactic.ToString();
                                var tactic_customFieldList = db.CustomField_Entity.Where(a => tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic).ToList();
                                tactic_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                ////End Added by Mitesh Vaishnav for PL ticket #719 Custom fields for programs
                            }
                            else if (section == Enums.Section.Tactic.ToString() && id != 0)
                            {
                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted.Equals(false) && a.PlanTacticId == id).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) && a.PlanTacticId == id).ToList();
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var lineItemIds = plan_campaign_Program_Tactic_LineItemList.Select(a => a.PlanLineItemId).ToList();
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => lineItemIds.Contains(a.PlanLineItemId)).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                ////Start Added by Mitesh Vaishnav for PL ticket #720 Custom fields for tactics
                                //// when tactic deleted then custom field's value for this tactic will be deleted 
                                var tactic_customFieldList = db.CustomField_Entity.Where(a => a.EntityId == id && a.CustomField.EntityType == section).ToList();
                                tactic_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                ////End Added by Mitesh Vaishnav for PL ticket #720 Custom fields for tactic
                            }
                            else if (section == Enums.Section.LineItem.ToString() && id != 0)
                            {
                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted.Equals(false) && a.PlanLineItemId == id).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => a.PlanLineItemId == id).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                            }
                        }
                        finally
                        {
                            db.Configuration.AutoDetectChangesEnabled = true;
                        }

                        returnValue = db.SaveChanges();
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                return returnValue;
            }
            return returnValue;
        }

        /// <summary>
        /// Helper of Custom label client wise
        /// Added by Dharmraj, 26-8-2014
        /// #738 Custom label for audience tab
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="customLabelCode">customLabelCode enum</param>
        /// <returns></returns>
        public static string CustomLabelFor(Enums.CustomLabelCode customLabelCode)
        {
            MRPEntities db = new MRPEntities();
            string code = customLabelCode.ToString();
            try
            {
                var objCustomLabel = db.CustomLabels.FirstOrDefault(l => l.Code == code && l.ClientId == Sessions.User.ClientId);
                if (objCustomLabel == null)
                {
                    return customLabelCode.ToString();
                }
                else
                {
                    return objCustomLabel.Title;
                }
            }
            catch (Exception ex)
            {
                return customLabelCode.ToString();
            }
        }


        /// <summary>
        /// Function to generate message for modification of tactic.
        /// Added by Mitesh on 03/09/2014
        /// #743 Actuals Inspect: User Name for Scheduler Integration
        /// </summary>
        /// <param name="tacticId">Plan Tactic Id.</param>
        /// <param name="userList">List of user object.</param>
        /// <returns>If any tactic actual or LineItem exist then it return string message else empty string </returns>
        public static string TacticModificationMessage(int tacticId, List<User> userList = null)
        {
            MRPEntities db = new MRPEntities();
            string lastModifiedMessage = string.Empty;
            string createdBy = null;
            DateTime? modifiedDate = null;

            //// Checking whether line item actuals exists.
            var lineItemListActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineitemActual => lineitemActual.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId == tacticId &&
                                                                                            lineitemActual.Plan_Campaign_Program_Tactic_LineItem.IsDeleted == false)
                                                                                     .OrderByDescending(la => la.CreatedDate)
                                                                                     .ToList();
            if (lineItemListActuals.Count != 0)
            {
                modifiedDate = lineItemListActuals.FirstOrDefault().CreatedDate;
                createdBy = lineItemListActuals.FirstOrDefault().CreatedBy.ToString();
            }
            else
            {
                ////Checking whether Tactic Actual exist.
                var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(tacticActual => tacticActual.PlanTacticId == tacticId)
                                                                             .OrderByDescending(ta => ta.ModifiedDate)
                                                                             .ThenBy(ta => ta.CreatedDate)
                                                                             .ToList();
                if (tacticActualList.Count != 0)
                {
                    if (tacticActualList.FirstOrDefault().ModifiedDate != null)
                    {
                        modifiedDate = tacticActualList.FirstOrDefault().ModifiedDate;
                        createdBy = tacticActualList.FirstOrDefault().ModifiedBy.ToString();
                    }
                    else
                    {
                        modifiedDate = tacticActualList.FirstOrDefault().CreatedDate;
                        createdBy = tacticActualList.FirstOrDefault().CreatedBy.ToString();
                    }
                }

            }

            if (createdBy != null && modifiedDate != null)
            {
                ////Checking if created by is empty then system generated modification
                if (Guid.Parse(createdBy) == Guid.Empty)
                {
                    lastModifiedMessage = string.Format("{0} {1} by {2}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), GameplanIntegrationService);
                    return lastModifiedMessage;
                }
                else
                {
                    bool isGetFromBDSService = false;////functiona perameter userList have "createdby" user
                    if (userList != null && userList.Count != 0)
                    {
                        User user = userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault();
                        if (user != null)
                        {
                            lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().FirstName, userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().LastName);
                            return lastModifiedMessage;
                        }
                        else
                        {
                            isGetFromBDSService = true;
                        }
                    }
                    else
                    {
                        isGetFromBDSService = true;
                    }

                    if (isGetFromBDSService)
                    {
                        BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                        User objUser = objBDSUserRepository.GetTeamMemberDetails(new Guid(createdBy), Sessions.ApplicationId);
                        lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), objUser.FirstName, objUser.LastName);
                        return lastModifiedMessage;
                    }
                }
            }

            return lastModifiedMessage;
        }

        //Created By : Kalpesh Sharma
        //Get the specific char from the string based upon it's index.
        //#453: Support request Issue field needs to be bigger
        public static int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Function to generate message for modification of LineItem of Actuals.
        /// Added by Viral Kadiya on 11/11/2014
        /// </summary>
        /// <param name="PlanLineItemId">Plan Tactic Id.</param>
        /// <param name="userList">List of user object.</param>
        /// <returns>If any LineItem actual exist then it return string message else empty string </returns>
        public static string ActualLineItemModificationMessageByPlanLineItemId(int PlanLineItemId, List<User> userList = null)
        {
            MRPEntities db = new MRPEntities();
            string lastModifiedMessage = string.Empty;
            string createdBy = null;
            DateTime? modifiedDate = null;

            //// Checking whether line item actuals exists.
            var lineItemListActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineitemActual => lineitemActual.PlanLineItemId == PlanLineItemId)
                                                                                     .OrderByDescending(la => la.CreatedDate)
                                                                                     .ToList();
            if (lineItemListActuals.Count != 0)
            {
                modifiedDate = lineItemListActuals.FirstOrDefault().CreatedDate;
                createdBy = lineItemListActuals.FirstOrDefault().CreatedBy.ToString();
            }

            if (createdBy != null && modifiedDate != null)
            {
                ////Checking if created by is empty then system generated modification
                if (Guid.Parse(createdBy) == Guid.Empty)
                {
                    lastModifiedMessage = string.Format("{0} {1} by {2}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), GameplanIntegrationService);
                    return lastModifiedMessage;
                }
                else
                {
                    bool isGetFromBDSService = false;////functiona perameter userList have "createdby" user
                    if (userList != null && userList.Count != 0)
                    {
                        User user = userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault();
                        if (user != null)
                        {
                            lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().FirstName, userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().LastName);
                            return lastModifiedMessage;
                        }
                        else
                        {
                            isGetFromBDSService = true;
                        }
                    }
                    else
                    {
                        isGetFromBDSService = true;
                    }

                    if (isGetFromBDSService)
                    {
                        BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                        User objUser = objBDSUserRepository.GetTeamMemberDetails(new Guid(createdBy), Sessions.ApplicationId);
                        lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd"), objUser.FirstName, objUser.LastName);
                        return lastModifiedMessage;
                    }
                }
            }

            return lastModifiedMessage;
        }

        /// <summary>
        /// Function to generate HTML with Anchor link if text contains url.
        /// Method used in all inspect popup to generate HTML based Description text.
        /// Added by Viral Kadiya on 11/15/2014
        /// </summary>
        /// <param name="description">Description Text.</param>
        public static string GenerateHTMLDescription(string description = "")
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(description))
                return result;
            try
            {
                result = description;
                string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,_\[\]\(\)\!\$\*\|##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
                Regex r = new Regex(regex, RegexOptions.IgnoreCase);
                result = result.Replace("\n", "<br />");
                result = r.Replace(result, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// Function to return Status message.
        /// Added by Viral Kadiya on 11/17/2014 for PL ticket #947.
        /// </summary>
        /// <param name="status">Status of the Tactic.</param>\
        public static string GetStatusMessage(string status)
        {
            string strMessage = string.Empty;
            try
            {
                if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                    strMessage = Common.objCached.PlanEntityDeclined;
                else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString()))
                    strMessage = Common.objCached.PlanEntitySubmittedForApproval;
                else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                    strMessage = Common.objCached.PlanEntityApproved;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strMessage;
        }


        public static string GetUserName(string UserGuid)
        {
            BDSServiceClient bdsservice = new BDSServiceClient();
            List<User> userName = new List<User>();
            userName = bdsservice.GetMultipleTeamMemberName(UserGuid);
            if (userName.Count > 0)
            {
                return string.Concat(userName.FirstOrDefault().FirstName, " ", userName.FirstOrDefault().LastName);
            }
            return "";
        }

        public static bool SetSessionVariable()
        {
            Sessions.IsDisplayDataInconsistencyMsg = false;
            return true;
        }

        #endregion

        /// <summary>
        /// Function to set session variable application name.
        /// Added By: Maninder 12/04/2014
        /// Ticket: 942 Exception handeling in Gameplan.
        /// </summary>
        internal static void SetSessionVariableApplicationName()
        {
            if (HttpContext.Current.Session["ApplicationName"] == null)
            {
                Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                string applicationName = objBDSServiceClient.GetApplicationName(applicationId);
                HttpContext.Current.Session["ApplicationName"] = applicationName;
            }
        }

        #region Validate share link of Notification email for Inspect popup
        /// <summary>
        /// Function to validate shared link for cases such as Valid Entity, is deleted entity, or cross client login
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>11/12/2014</CreatedDate>
        /// <param name="PlanId">planId from query string parameter of shared link</param>
        /// <param name="InspectEntityId">planCampaignId/planProgramId/planTacticId from query string parameter of shared link</param>
        /// <param name="EntityType">EntityType Campaign/Program/Tactic/ImprovementTactic as per InspectEntityId from query string parameter of shared link</param>
        /// <param name="IsDeleted">out parameter : flag to indicate Entity is deleted or not</param>
        /// <param name="IsEntityExists">out parameter : flag to indicate Entity exists in DB or not</param>
        /// <param name="IsCorrectPlanId">out parameter : flag to indicate planId given in shared link is valid or not</param>
        /// <returns>returns flag to indicate user is valid or not with some out parameters</returns>
        public static bool ValidateNotificationShaedLink(int PlanId, int InspectEntityId, string EntityType, out bool IsDeleted, out bool IsEntityExists, out bool IsCorrectPlanId)
        {
            using (MRPEntities objMRPEntities = new MRPEntities())
            {
                bool isValidUser = false;
                IsDeleted = false;
                IsEntityExists = false;
                IsCorrectPlanId = false;

                try
                {
                    if (EntityType.Equals(Enums.PlanEntity.Tactic.ToString()))
                    {
                        var entityObj = objMRPEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == InspectEntityId).Select(tactic => new { tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnit.ClientId, tactic.IsDeleted, tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId });
                        if (entityObj.Count() != 0)
                        {
                            IsEntityExists = true;
                            if (entityObj.Where(entity => entity.ClientId == Sessions.User.ClientId).Any())
                            {
                                isValidUser = true;
                                IsDeleted = entityObj.Select(entity => entity.IsDeleted).First<bool>();
                                if (entityObj.Select(entity => entity.PlanId).First() == PlanId)
                                {
                                    IsCorrectPlanId = true;
                                }
                            }
                        }
                    }
                    else if (EntityType.Equals(Enums.PlanEntity.Program.ToString()))
                    {
                        var entityObj = objMRPEntities.Plan_Campaign_Program.Where(program => program.PlanProgramId == InspectEntityId).Select(program => new { program.Plan_Campaign.Plan.Model.BusinessUnit.ClientId, program.IsDeleted, program.Plan_Campaign.Plan.PlanId });
                        if (entityObj.Count() != 0)
                        {
                            IsEntityExists = true;
                            if (entityObj.Where(entity => entity.ClientId == Sessions.User.ClientId).Any())
                            {
                                isValidUser = true;
                                IsDeleted = entityObj.Select(entity => entity.IsDeleted).First<bool>();
                                if (entityObj.Select(entity => entity.PlanId).First() == PlanId)
                                {
                                    IsCorrectPlanId = true;
                                }
                            }
                        }
                    }
                    else if (EntityType.Equals(Enums.PlanEntity.Campaign.ToString()))
                    {
                        var entityObj = objMRPEntities.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == InspectEntityId).Select(campaign => new { campaign.Plan.Model.BusinessUnit.ClientId, campaign.IsDeleted, campaign.Plan.PlanId });
                        if (entityObj.Count() != 0)
                        {
                            IsEntityExists = true;
                            if (entityObj.Where(entity => entity.ClientId == Sessions.User.ClientId).Any())
                            {
                                isValidUser = true;
                                IsDeleted = entityObj.Select(entity => entity.IsDeleted).First<bool>();
                                if (entityObj.Select(entity => entity.PlanId).First() == PlanId)
                                {
                                    IsCorrectPlanId = true;
                                }
                            }
                        }
                    }
                    else if (EntityType.Equals(Enums.PlanEntity.ImprovementTactic.ToString()))
                    {
                        var entityObj = objMRPEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId == InspectEntityId).Select(improvementTactic => new { improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.BusinessUnit.ClientId, improvementTactic.IsDeleted, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId });
                        if (entityObj.Count() != 0)
                        {
                            IsEntityExists = true;
                            if (entityObj.Where(entity => entity.ClientId == Sessions.User.ClientId).Any())
                            {
                                isValidUser = true;
                                IsDeleted = entityObj.Select(entity => entity.IsDeleted).First<bool>();
                                if (entityObj.Select(entity => entity.PlanId).First() == PlanId)
                                {
                                    IsCorrectPlanId = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return isValidUser;
            }
        }
        #endregion

        #region Prepare select list of contact target mapping fields of eloqua
        /// <summary>
        /// Function to prepare select list of contacts of eloqua to be shown in dropdown
        /// </summary>
        /// <param name="integrationInstanceId">integration instance id</param>
        /// <returns>list of selectItem object</returns>
        public static List<SelectListItem> GetEloquaContactTargetDataMemberSelectList(int integrationInstanceId)
        {
            List<SelectListItem> lstExternalFieldsPulling = new List<SelectListItem>();
            Dictionary<string, string> lstEloquaContactFields = new Dictionary<string, string>();

            //// Get contact object list form Eloqua
            ExternalIntegration objEx = new ExternalIntegration(integrationInstanceId, Sessions.ApplicationId);
            lstEloquaContactFields = objEx.GetEloquaContactTargetDataMemberList();

            if (lstEloquaContactFields != null)
            {
                //// Prepare select list for contact list 
                foreach (var contact in lstEloquaContactFields)
                {
                    SelectListItem objSelectListItem = new SelectListItem();
                    objSelectListItem.Text = contact.Value;
                    objSelectListItem.Value = contact.Key;
                    lstExternalFieldsPulling.Add(objSelectListItem);
                }
            }

            return lstExternalFieldsPulling;
        }
        #endregion

        #region Update plan year value
        /// <summary>
        /// Function to update planyear of child activities of a plan
        /// </summary>
        /// <param name="PlanId">plan id of a plan</param>
        /// <param name="PlanYear">new plan year value</param>
        /// <returns>returns the status flag</returns>
        public static int UpdatePlanYearOfActivities(int PlanId, int PlanYear)
        {
            int returnFlag = 0;
            if (PlanId == 0)
            {
                return returnFlag;
            }
            
            List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
            MRPEntities db = new MRPEntities();

            try
            {
                Plan proj = db.Plans.FirstOrDefault(p => p.PlanId == PlanId && p.IsDeleted == false);
                if (proj != null)
                {
                    proj.Plan_Campaign.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.StartDate = t.StartDate.AddYears(PlanYear - t.StartDate.Year);
                            t.EndDate = t.EndDate.AddYears(PlanYear - t.EndDate.Year);
                            t.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(pcp =>
                            {
                                pcp.StartDate = pcp.StartDate.AddYears(PlanYear - pcp.StartDate.Year);
                                pcp.EndDate = pcp.EndDate.AddYears(PlanYear - pcp.EndDate.Year);
                                pcp.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                                {
                                    pcpt.StartDate = pcpt.StartDate.AddYears(PlanYear - pcpt.StartDate.Year);
                                    pcpt.EndDate = pcpt.EndDate.AddYears(PlanYear - pcpt.EndDate.Year);
                                    pcpt.Plan_Campaign_Program_Tactic_LineItem = pcpt.Plan_Campaign_Program_Tactic_LineItem.ToList();
                                    pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                                    {
                                        pcptl.StartDate = pcptl.StartDate.HasValue ? pcptl.StartDate.Value.AddYears(PlanYear - pcptl.StartDate.Value.Year) : pcptl.StartDate;
                                        pcptl.EndDate = pcptl.EndDate.HasValue ? pcptl.EndDate.Value.AddYears(PlanYear - pcptl.EndDate.Value.Year) : pcptl.EndDate;
                                    });
                                });
                            });
                        });

                    proj.Plan_Campaign = proj.Plan_Campaign.ToList();

                    proj.Plan_Improvement_Campaign.Where(improvementCampaign => improvementCampaign.ImprovePlanId.Equals(PlanId)).ToList().ForEach(improvementCampaign =>
                    {
                        improvementCampaign.Plan_Improvement_Campaign_Program.ToList().ForEach(improvementProgram =>
                            {
                                improvementProgram.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.IsDeleted.Equals(false)).ToList().ForEach(improvementTactic =>
                                {
                                    improvementTactic.EffectiveDate = improvementTactic.EffectiveDate.AddYears(PlanYear - improvementTactic.EffectiveDate.Year);
                                });
                            });
                    });
                    proj.Plan_Improvement_Campaign = proj.Plan_Improvement_Campaign.ToList();

                    db.Entry(proj).State = EntityState.Modified;

                    returnFlag = db.SaveChanges();
                    return returnFlag;
                }
                return returnFlag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }

    /// <summary>
    /// Added by Viral Kadiya on 10/17/14 to resolve issue for PL Ticket #833 to Filter values in dropdown should be Alpha sorted.
    /// </summary>
    public class AlphaNumericComparer : IComparer<string>
    {
        public int Compare(string first, string second)
        {
            /* Note: List must does not contains null items on it. */
            string strRegExpPattern = "^[a-zA-Z0-9- ]*$";
            int firstNumber, secondNumber;

            // Start - Check whether string starts with special character or not
            bool first_IsSpecialChar = !Regex.IsMatch(first[0].ToString(), strRegExpPattern, RegexOptions.IgnoreCase);
            bool second_IsSpecialChar = !Regex.IsMatch(second[0].ToString(), strRegExpPattern, RegexOptions.IgnoreCase);
            if (first_IsSpecialChar)
                return second_IsSpecialChar ? first.CompareTo(second) : -1;
            if (second_IsSpecialChar)
                return 1;
            // End

            // Start - Check whether string is numeric or not
            bool firstIsNumber = int.TryParse(first, out firstNumber);
            bool secondIsNumber = int.TryParse(second, out secondNumber);
            if (firstIsNumber)
                return secondIsNumber ? firstNumber.CompareTo(secondNumber) : -1;
            return secondIsNumber ? 1 : first.CompareTo(second);
            // End
        }
    }
}