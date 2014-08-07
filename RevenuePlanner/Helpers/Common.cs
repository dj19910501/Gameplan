using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Models;
using RevenuePlanner.Controllers;
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

        public const string OptionTextRegex = "^[^<>]+";
        public const string MessageForOptionTextRegex = "<> characters are not allowed";


        public static Message objCached = new Message();
        public static string xmlMsgFilePath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLCommonMsgFilePath"));
        public static string xmlBenchmarkFilePath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLBenchmarkFilePath"));

        public static readonly int imgWidth = 50;
        public static readonly int imgHeight = 50;

        public const string GANTT_BAR_CSS_CLASS_PREFIX = "color";

        ////Modified By Maninder Singh Wadhva PL Ticket#47, 337
        public const string GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT = "improvementcolor";
        public const string COLORC6EBF3_WITH_BORDER = "colorC6EBF3-with-border";
        ////Modified By Maninder Singh Wadhva PL Ticket#47, 337
        public const string COLORC6EBF3_WITH_BORDER_IMPROVEMENT = "improvementcolorC6EBF3-with-border";
        public const string COLOR27A4E5 = "color27a4e5";

        public const string dateFormat = "mm/dd/yyyy";

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
        #endregion

        //#region Enums

        //public enum Permission
        //{
        //    NoAccess = 0,
        //    ViewOnly = 1,
        //    FullAccess = 2,
        //    NotAnEntity = 3
        //}
        //#endregion

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
                }
                else if (Section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", CollaboratorUserName.ElementAt(i)).Replace("[CampaignNameToBeReplaced]", TacticName).Replace("[PlanNameToBeReplaced]", PlanName).Replace("[UserNameToBeReplaced]", Sessions.User.FirstName + " " + Sessions.User.LastName).Replace("[CommentToBeReplaced]", Comment);
                }
                else if (Section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    ////Modified By Maninder Singh Wadhva PL Ticket#47
                    emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", CollaboratorUserName.ElementAt(i)).Replace("[ImprovementTacticNameToBeReplaced]", TacticName).Replace("[PlanNameToBeReplaced]", PlanName).Replace("[UserNameToBeReplaced]", Sessions.User.FirstName + " " + Sessions.User.LastName).Replace("[CommentToBeReplaced]", Comment);
                }
                string email = EmailIds.ElementAt(i);
                string Username = CollaboratorUserName.ElementAt(i);
                //Common.SendMailToMultipleUser(EmailIds, Common.FromMail, emailBody, notification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High));
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
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticApproved.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramApproved.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignApproved.ToString(), "", Convert.ToString(Enums.Section.Campaign).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticApproved.ToString(), "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                    }
                }
                else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                {

                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticDeclined.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramDeclined.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignDeclined.ToString(), "", Convert.ToString(Enums.Section.Campaign).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticDeclined.ToString(), "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
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
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticSubmitted.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramSubmitted.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignSubmitted.ToString(), "", Convert.ToString(Enums.Section.Campaign).ToLower());
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticSubmitted.ToString(), "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
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
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Program).ToLower());
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.CampaignCommentAdded.ToString()) && iscomment)
                {
                    if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.CampaignCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Campaign).ToLower());
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString()) && iscomment)
                {
                    if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
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
        public static void SetCookie(string cookieName, string cookieValue)
        {
            HttpCookie objCookie = null;
            objCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (objCookie != null)
            {
                objCookie.Value = cookieValue;
                objCookie.Expires = System.Convert.ToDateTime(System.DateTime.Now.Date).AddMonths(6);
                objCookie.Path = "/";
                HttpContext.Current.Response.Cookies.Add(objCookie);
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
        public static List<Plan> GetPlan()
        {
            MRPEntities db = new MRPEntities();
            List<Guid> businessUnitIds = new List<Guid>();

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
            return startDate < calendarStartDate ? string.Format("{0}", calendarStartDate.ToShortDateString()) : string.Format("{0}", startDate.ToShortDateString());
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
            var plan = db.Plans.Single(p => p.PlanId.Equals(planId));

            List<string> collaboratorId = new List<string>();
            if (plan.ModifiedBy != null)
            {
                collaboratorId.Add(plan.ModifiedBy.ToString());
            }

            if (plan.CreatedBy != null)
            {
                collaboratorId.Add(plan.CreatedBy.ToString());
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

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

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = data;
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

            foreach (var cl in lst_ChangeLog)
            {
                ChangeLog_ViewModel clvm = new ChangeLog_ViewModel();
                var user = bdsservice.GetTeamMemberDetails(cl.UserId, Sessions.ApplicationId);
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

        public static HomePlanModelHeader GetPlanHeaderValue(int planId)
        {
            HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();
            MRPEntities db = new MRPEntities();
            List<string> tacticStatus = GetStatusListAfterApproved();

            var plan = db.Plans.Where(p => p.PlanId == planId && p.IsDeleted == false && p.IsActive == true).Select(m => m).FirstOrDefault();
            if (plan != null)
            {
                List<Plan_Campaign_Program_Tactic> planTacticIds = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && tacticStatus.Contains(t.Status) && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList();

                if (plan.Status == Enums.PlanStatusValues[Enums.PlanStatus.Draft.ToString()].ToString())
                {
                    //objHomePlanModelHeader.MQLs = plan.MQLs;  // Commented by Sohel Pathan on 15/07/2014 for PL ticket #566
                    // Start - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                    if (plan.GoalType.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                    {
                        objHomePlanModelHeader.MQLs = plan.GoalValue;
                    }
                    else
                    {
                        // Get ADS value
                        string marketing = Enums.Funnel.Marketing.ToString();
                        double ADSValue = db.Model_Funnel.Single(mf => mf.ModelId == plan.ModelId && mf.Funnel.Title == marketing).AverageDealSize;

                        objHomePlanModelHeader.MQLs = Common.CalculateMQLOnly(plan.ModelId, plan.GoalType, plan.GoalValue.ToString(), ADSValue);
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
                    objHomePlanModelHeader.Budget = plan.Budget;
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Budget.ToString()].ToString();
                }
                else
                {
                    // Added BY Bhavesh
                    // Calculate MQL at runtime #376
                    objHomePlanModelHeader.MQLs = GetTacticStageRelation(planTacticIds, false).Sum(t => t.MQLValue); ;// Common.GetMQLValueTacticList(planTacticIds).Sum(tm => tm.MQL);
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
                        objHomePlanModelHeader.Budget = planTacticIds.Sum(t => t.Cost);
                    }
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Cost.ToString()].ToString();
                }

                var impList = db.Plan_Improvement_Campaign_Program_Tactic.Where(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && !imp.IsDeleted).ToList();
                if (impList.Count > 0)
                {
                    /// Added By: Maninder Singh Wadhva
                    /// Addressed PL Ticket: 37,38,47,49
                    //// Getting improved MQL.
                    double? improvedMQL = GetTacticStageRelation(planTacticIds, true).Sum(t => t.MQLValue);

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

        public static List<string> GetStatusListAfterApproved()
        {
            List<string> tacticStatus = new List<string>();
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            tacticStatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            return tacticStatus;
        }

        #endregion
        /* added by nirav Shah on 7 Jan 2014*/
        public static List<SelectListItem> GetUpcomingActivity()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.planYear.ToString()].ToString(), Value = Enums.UpcomingActivities.planYear.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisyear.ToString()].ToString(), Value = Enums.UpcomingActivities.thisyear.ToString(), Selected = true });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisquarter.ToString()].ToString(), Value = Enums.UpcomingActivities.thisquarter.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thismonth.ToString()].ToString(), Value = Enums.UpcomingActivities.thismonth.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.nextyear.ToString()].ToString(), Value = Enums.UpcomingActivities.nextyear.ToString(), Selected = false });
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
        /// <param name="Mode"></param>
        /// <returns></returns>
        public static string GetLabel(int Mode) //1. INQ, 2.MQL, 3.CW
        {
            MRPEntities db = new MRPEntities();
            string strStageLabel = string.Empty;
            try
            {
                string StageCode = "";
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
                strStageLabel = db.Stages.Where(s => s.Code == StageCode && s.IsDeleted == false && s.ClientId == Sessions.User.ClientId).Select(s => s.Title).FirstOrDefault();
                return strStageLabel;
            }
            catch (Exception)
            {
                db = null;
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

        #region "MQL Calculate"

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
        /// Calculate MQL for One Tactic.
        /// PL Ticket #376 Remove storing of pre-calculated MQL to DB
        /// Date 8-4-2014.
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="INQ"></param>
        /// <param name="StartDate"></param>
        /// <param name="PlanTacticId"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        public static double CalculateMQLTactic(double projecteStageValue, DateTime StartDate, int PlanTacticId, int stageId, int ModelId = 0)
        {
            MRPEntities dbm = new MRPEntities();
            if (ModelId == 0)
            {
                ModelId = dbm.Plan_Campaign_Program_Tactic.Where(p => p.PlanTacticId == PlanTacticId).Select(p => p.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).SingleOrDefault();
            }
            
            return Math.Round(projecteStageValue * GetMQLConversionRate(StartDate, ModelId, stageId), 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Get MQL Converion Rate
        /// PL Ticket #376 Remove storing of pre-calculated MQL to DB
        /// Date 8-4-2014.
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        public static double GetMQLConversionRate(DateTime StartDate, int ModelId, int stageId)
        {
            ModelId = GetModelId(StartDate, ModelId);

            MRPEntities dbStage = new MRPEntities();
            List<Stage> stageList = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = stageList.Single(s => s.Code.Equals(stageMQL)).Level.Value;
            List<int> mqlStagelist = new List<int>();
            string CR = Enums.StageType.CR.ToString();
            string marketing = Enums.Funnel.Marketing.ToString();

            List<StageRelation> stageRelationList = dbStage.Model_Funnel_Stage.Where(m => m.Model_Funnel.Funnel.Title.Equals(marketing) && m.Model_Funnel.ModelId == ModelId && m.StageType == CR).Select(m => new StageRelation
            {
                StageId = m.StageId,
                StageType = m.StageType,
                Value = m.Value / 100
            }
                    ).ToList();
                
            int projectedStageLevel = stageList.Single(s => s.StageId == stageId).Level.Value;
            mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();

            return projectedStageLevel <= levelMQL ? 1 * (stageRelationList.Where(sr => mqlStagelist.Contains(sr.StageId)).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
            
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
        public static List<TacticModelRelation> GetTacticModelRelationList(List<Plan_Campaign_Program_Tactic> tlist)
        {
            List<TacticModelRelation> tacticModellist = (from t in tlist
                                                         select new TacticModelRelation
                                                         {
                                                             PlanTacticId = t.PlanTacticId,
                                                             ModelId = GetModelId(t.StartDate, t.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId)
                                                         }).ToList();

            return tacticModellist;
        }
        #endregion

        #region "Report Calculation"

        ///// <summary>
        ///// Get Conversion Rate for Model Ids.
        ///// Addded By Bhavesh Dobariya
        ///// </summary>
        ///// <param name="ModelIds"></param>
        ///// <param name="stageCode"></param>
        ///// <returns></returns>
        //public static List<ModelConvertionRateRelation> GetModelConversionRate(List<int> ModelIds, string stageCode)
        //{
        //    MRPEntities dbStage = new MRPEntities();
        //    List<int> Levelelist = new List<int>();
        //    string stageINQ = Enums.Stage.INQ.ToString();
        //    int levelINQ = dbStage.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageINQ)).Level.Value;
        //    if (stageCode == Enums.Stage.INQ.ToString())
        //    {
        //        for (int i = 1; i < levelINQ; i++)
        //        {
        //            Levelelist.Add(i);
        //        }
        //    }
        //    else if (stageCode == Enums.Stage.MQL.ToString())
        //    {
        //        string stageMQL = Enums.Stage.MQL.ToString();
        //        int levelMQL = dbStage.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
        //        for (int i = levelINQ; i < levelMQL; i++)
        //        {
        //            Levelelist.Add(i);
        //        }
        //    }
        //    else
        //    {
        //        string stageCW = Enums.Stage.CW.ToString();
        //        int levelCW = dbStage.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageCW)).Level.Value;
        //        for (int i = levelINQ; i <= levelCW; i++)
        //        {
        //            Levelelist.Add(i);
        //        }
        //    }
        //    string stageTypeCR = Enums.StageType.CR.ToString();
        //    string marketing = Enums.Funnel.Marketing.ToString();

        //    var MlistStageValue = (from modelFunnelStage in dbStage.Model_Funnel_Stage
        //                           join stage in dbStage.Stages on modelFunnelStage.StageId equals stage.StageId
        //                           where ModelIds.Contains(modelFunnelStage.Model_Funnel.ModelId) && modelFunnelStage.StageType.Equals(stageTypeCR) &&
        //                                                            stage.ClientId.Equals(Sessions.User.ClientId) &&
        //                                                            Levelelist.Contains((int)stage.Level) && modelFunnelStage.Model_Funnel.Funnel.Title.Equals(marketing)
        //                           select new
        //                           {
        //                               ModelId = modelFunnelStage.Model_Funnel.ModelId,
        //                               Value = modelFunnelStage.Value
        //                           }).ToList().GroupBy(rl => new { ModelId = rl.ModelId }).ToList().Select(r => new
        //                           {
        //                               ModelId = r.Key.ModelId,
        //                               Value = (r.Aggregate(1.0, (s1, s2) => s1 * (s2.Value / 100)))
        //                           }).ToList();

        //    List<ModelConvertionRateRelation> modellist = (from m in ModelIds
        //                                                   join modelFunnel in dbStage.Model_Funnel on m equals modelFunnel.ModelId
        //                                                   where modelFunnel.Funnel.Title.Equals(marketing)
        //                                                   select new ModelConvertionRateRelation
        //                                             {
        //                                                 ModelId = m,
        //                                                 AverageDealSize = modelFunnel.AverageDealSize,
        //                                                 ConversionRate = MlistStageValue.Where(ms => ms.ModelId == m).Count() > 0 ? MlistStageValue.Where(ms => ms.ModelId == m).Select(ms => ms.Value).SingleOrDefault() : 1
        //                                             }).ToList();

        //    return modellist;
        //}

        ///// <summary>
        ///// Get Velocity for Model Ids.
        ///// Addded By Bhavesh Dobariya
        ///// </summary>
        ///// <param name="ModelIds"></param>
        ///// <param name="stageCode"></param>
        ///// <returns></returns>
        //public static List<ModelVelocityRelation> GetModelVelocity(List<int> ModelIds, string stageCode)
        //{
        //    MRPEntities dbStage = new MRPEntities();
        //    List<int> Levelelist = new List<int>();
        //    string stageINQ = Enums.Stage.INQ.ToString();
        //    int levelINQ = dbStage.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageINQ)).Level.Value;
        //    if (stageCode == Enums.Stage.INQ.ToString())
        //    {
        //        for (int i = 1; i < levelINQ; i++)
        //        {
        //            Levelelist.Add(i);
        //        }
        //    }
        //    else if (stageCode == Enums.Stage.MQL.ToString())
        //    {
        //        string stageMQL = Enums.Stage.MQL.ToString();
        //        int levelMQL = dbStage.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
        //        for (int i = levelINQ; i < levelMQL; i++)
        //        {
        //            Levelelist.Add(i);
        //        }
        //    }
        //    else
        //    {
        //        string stageCW = Enums.Stage.CW.ToString();
        //        int levelCW = dbStage.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageCW)).Level.Value;
        //        for (int i = levelINQ; i <= levelCW; i++)
        //        {
        //            Levelelist.Add(i);
        //        }
        //    }
        //    string stageTypeSV = Enums.StageType.SV.ToString();
        //    string marketing = Enums.Funnel.Marketing.ToString();

        //    var MlistStageValue = (from modelFunnelStage in dbStage.Model_Funnel_Stage
        //                           join stage in dbStage.Stages on modelFunnelStage.StageId equals stage.StageId
        //                           where ModelIds.Contains(modelFunnelStage.Model_Funnel.ModelId) && modelFunnelStage.StageType.Equals(stageTypeSV) &&
        //                                                            stage.ClientId.Equals(Sessions.User.ClientId) &&
        //                                                            Levelelist.Contains((int)stage.Level) && modelFunnelStage.Model_Funnel.Funnel.Title.Equals(marketing)
        //                           select new
        //                           {
        //                               ModelId = modelFunnelStage.Model_Funnel.ModelId,
        //                               value = modelFunnelStage.Value
        //                           }).ToList();

        //    List<ModelVelocityRelation> modellist = (from m in ModelIds
        //                                             select new ModelVelocityRelation
        //                                             {
        //                                                 ModelId = m,
        //                                                 Velocity = MlistStageValue.Where(ms => ms.ModelId == m).Count() > 0 ? MlistStageValue.Where(ms => ms.ModelId == m).Sum(ms => ms.value) : 0
        //                                             }).ToList();

        //    return modellist;
        //}

        /// <summary>
        /// Calculate Projected Revenue of Tactic List.
        /// Addded By Bhavesh Dobariya
        /// </summary>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public static List<ProjectedRevenueClass> ProjectedRevenueCalculateList(List<Plan_Campaign_Program_Tactic> tlist, bool isCW = false)
        {

            List<TacticStageValue> tacticlValueList =  GetTacticStageRelation(tlist, false);
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
            else if(obj.Contains("->"))
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
            try
            {
                Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                return objBDSServiceClient.GetApplicationReleaseVersion(applicationId);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return string.Empty;
            }
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
                lstProgram.ForEach(varP => varP.Plan_Campaign_Program_Tactic.Where(varT => varT.IsDeleted == false).ToList().ForEach(varT=>cost=cost+ varT.Plan_Campaign_Program_Tactic_LineItem.Where(varL=>varL.IsDeleted==false).Sum(varL=>varL.Cost)));

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
                 lstTactic.ForEach(varT => cost = cost + varT.Plan_Campaign_Program_Tactic_LineItem.Where(varL=>varL.IsDeleted==false).Sum(varL=>varL.Cost));

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

        public static List<TacticStageValue> GetTacticStageRelation(List<Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true)
        {
            MRPEntities dbStage = new MRPEntities();
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculation(tlist, isIncludeImprovement);
            List<Stage> stageList = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            List<TacticStageValue> tacticStageList = new List<TacticStageValue>();
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = stageList.Single(s => s.Code.Equals(stageINQ)).Level.Value;
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = stageList.Single(s => s.Code.Equals(stageMQL)).Level.Value;
            string stageCW = Enums.Stage.CW.ToString();
            int levelCW = stageList.Single(s => s.Code.Equals(stageCW)).Level.Value;
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
            List<int> TacticIds = tlist.Select(t => t.PlanTacticId).ToList();
            List<Plan_Campaign_Program_Tactic_Actual> actualTacticList = dbStage.Plan_Campaign_Program_Tactic_Actual.Where(a => TacticIds.Contains(a.PlanTacticId)).ToList();
            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                List<StageRelation> stageRelation = tacticValueRelationList.Single(t => t.TacticObj.PlanTacticId == tactic.PlanTacticId).StageValueList;
                int projectedStageLevel = stageList.Single(s => s.StageId == tactic.StageId).Level.Value;
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
                tacticStageValueObj.ActualTacticList = actualTacticList.Where(a => a.PlanTacticId == tactic.PlanTacticId).ToList();
                tacticStageValueObj.TacticYear = tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;

                tacticStageList.Add(tacticStageValueObj);
            }
            return tacticStageList;
        }

        public static List<TacticStageValueRelation> GetCalculation(List<Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true)
        {
            List<TacticPlanRelation> tacticPlanList = GetTacticPlanRelationList(tlist);
            List<TacticModelRelation> tacticModelList = GetTacticModelRelationList(tlist);
            List<PlanIMPTacticListRelation> planIMPTacticList = GetPlanImprovementTacticList(tacticPlanList.Select(p => p.PlanId).Distinct().ToList());
            List<StageRelation> bestInClassStageRelation = GetBestInClassValue();
            List<ModelStageRelationList> modleStageRelationList = GetModelStageRelation(tacticModelList.Select(m => m.ModelId).Distinct().ToList());
            List<int> impids = new List<int>();
            planIMPTacticList.ForEach(t => t.ImprovementTacticList.ForEach(imp => impids.Add(imp.ImprovementTacticTypeId)));
            List<ImprovementTypeWeightList> improvementTypeWeightList = GetImprovementTacticWeightList(impids);
            List<StageList> stageList = GetStageList();
            List<TacticStageValueRelation> TacticSatgeValueList = new List<TacticStageValueRelation>();
            string Size = Enums.StageType.Size.ToString();
            int ADSStageId = stageList.Single(s => s.Level == null && s.StageType == Size).StageId;

            //Modified By Kalpesh Sharma : Internal Review Point #83 Error on home page when user login for Other Clients

            double bestInClassAdsValue = 0;

            var objbestInClassAdsValue = bestInClassStageRelation.SingleOrDefault(b => b.StageId == ADSStageId);

            if (!string.IsNullOrEmpty(Convert.ToString(objbestInClassAdsValue)))
            {
                bestInClassAdsValue = objbestInClassAdsValue.Value;
            }
            
            List<PlanADSRelation> planADSList = GetPlanADSList(planIMPTacticList, improvementTypeWeightList, ADSStageId, Size, bestInClassAdsValue);

            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                int planId = tacticPlanList.Single(t => t.PlanTacticId == tactic.PlanTacticId).PlanId;
                int modelId = tacticModelList.Single(t => t.PlanTacticId == tactic.PlanTacticId).ModelId;
                List<StageRelation> stageModelRelation = modleStageRelationList.Single(m => m.ModelId == modelId).StageList;
                List<Plan_Improvement_Campaign_Program_Tactic> improvementList = (planIMPTacticList.Single(p => p.PlanId == planId).ImprovementTacticList).Where(it => it.EffectiveDate <= tactic.EndDate).ToList();
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
                        if (stage.StageType == Size && ADSStageId == stage.StageId && planADSList.Single(p => p.PlanId == planId).isImprovementExits)
                        {
                            stageRelationObj.Value = planADSList.Single(p => p.PlanId == planId).ADS;
                        }
                        else
                        {
                        var stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Value > 0).ToList();
                        double impcount = stageimplist.Count();
                        double impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Value);
                        double improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Single(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Value, stageModelRelation.Single(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Value, impcount, impWeight);
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

        public static List<PlanIMPTacticListRelation> GetPlanImprovementTacticList(List<int> planIds)
        {
            MRPEntities dbStage = new MRPEntities();

            List<PlanIMPTacticListRelation> planIMPTacticList = new List<PlanIMPTacticListRelation>();
            foreach (int planId in planIds)
            {
                PlanIMPTacticListRelation planTacticIMP = new PlanIMPTacticListRelation();
                planTacticIMP.PlanId = planId;
                planTacticIMP.ImprovementTacticList = dbStage.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => imptactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && !imptactic.IsDeleted).ToList();
                planIMPTacticList.Add(planTacticIMP);
            }

            return planIMPTacticList;
        }

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

        public static List<ModelStageRelationList> GetModelStageRelation(List<int> modleIds)
        {
            MRPEntities dbStage = new MRPEntities();
            string marketing = Enums.Funnel.Marketing.ToString();
            string size = Enums.StageType.Size.ToString();
            string CR = Enums.StageType.CR.ToString();
            string ADS = Enums.Stage.ADS.ToString();
            int adsStageId = dbStage.Stages.Single(stage => stage.Code.Equals(ADS) && stage.ClientId == Sessions.User.ClientId).StageId;
            List<ModelStageRelationList> modleStageRelationist = new List<ModelStageRelationList>();
            foreach (int modelId in modleIds)
            {
                ModelStageRelationList modelStageObj = new ModelStageRelationList();
                modelStageObj.ModelId = modelId;
                modelStageObj.StageList = dbStage.Model_Funnel_Stage.Where(m => m.Model_Funnel.Funnel.Title.Equals(marketing) && m.Model_Funnel.ModelId == modelId).Select(m => new StageRelation
                {
                    StageId = m.StageId,
                    StageType = m.StageType,
                    Value = m.StageType == CR ? m.Value / 100 : m.Value
                }
                    ).ToList();

                double ads = dbStage.Model_Funnel.Single(m => m.Funnel.Title.Equals(marketing) && m.ModelId == modelId).AverageDealSize;
                modelStageObj.StageList.Add(new StageRelation { StageId = adsStageId, StageType = size, Value = ads });
                modleStageRelationist.Add(modelStageObj);
            }

            return modleStageRelationist;
        }

        public static List<ImprovementTypeWeightList> GetImprovementTacticWeightList(List<int> improvementTacticTypeIds)
        {
            MRPEntities dbStage = new MRPEntities();
            List<ImprovementTypeWeightList> improvementTypeWeightList = new List<ImprovementTypeWeightList>();
            var improvementTacticTypeList = dbStage.ImprovementTacticTypes.Where(imp => improvementTacticTypeIds.Contains(imp.ImprovementTacticTypeId)).ToList();
            var improvementTacticTypeWeightList = dbStage.ImprovementTacticType_Metric.Where(imp => improvementTacticTypeIds.Contains(imp.ImprovementTacticTypeId)).ToList();
            foreach (int improvementTacticTypeId in improvementTacticTypeIds)
            {
                bool isDeployed = improvementTacticTypeList.Single(imp => imp.ImprovementTacticTypeId == improvementTacticTypeId).IsDeployed;
                var innerList = improvementTacticTypeWeightList.Where(imp => imp.ImprovementTacticTypeId == improvementTacticTypeId).Select(imp => imp).ToList();
                innerList.ForEach(innerl => improvementTypeWeightList.Add(new ImprovementTypeWeightList { ImprovementTypeId = improvementTacticTypeId, isDeploy = isDeployed, StageId = innerl.StageId, StageType = innerl.StageType, Value = innerl.Weight }));
            }

            return improvementTypeWeightList;
        }

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

        public static List<PlanADSRelation> GetPlanADSList(List<PlanIMPTacticListRelation> planIMPList,List<ImprovementTypeWeightList> improvementTypeWeightList,int ADSStageId,string Size, double bestInClassSizeValue)
        {
            MRPEntities dbStage = new MRPEntities();
            string marketing = Enums.Funnel.Marketing.ToString();
            List<PlanADSRelation> planADSList = new List<PlanADSRelation>();
            List<int> planIds = planIMPList.Select(p => p.PlanId).ToList();
            List<PlanModelRelation> planModelRelation = dbStage.Plans.Where(p => planIds.Contains(p.PlanId)).Select(p => new PlanModelRelation { PlanId = p.PlanId, ModelId = p.ModelId }).ToList();
            foreach (PlanIMPTacticListRelation planIMP in planIMPList)
            {
                bool isImprovementExits = false;
                int modelId = planModelRelation.Single(p => p.PlanId == planIMP.PlanId).ModelId;
                if (planIMP.ImprovementTacticList.Count > 0)
                {
                     //// Get Model id based on effective date From.
                    modelId = GetModelId(planIMP.ImprovementTacticList.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), modelId);
                }
                double ADSValue = dbStage.Model_Funnel.Single(mf => mf.ModelId == modelId && mf.Funnel.Title == marketing).AverageDealSize;
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

        #endregion

        #region "Improvement Customized Stage"

        public static double GetCalculatedValueImproved(int planId, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, string stageType, bool isIncludeImprovement = true)
        {
            List<StageRelation> stagevalueList = CalculateStageValue(planId, improvementActivities, isIncludeImprovement);
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
        public static List<StageRelation> CalculateStageValue(int planId, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, bool isIncludeImprovement = true)
        {
            MRPEntities db = new MRPEntities();

            List<StageRelation> bestInClassStageRelation = GetBestInClassValue();
            List<StageList> stageList = GetStageList();
            //// Getting model based on plan id.
            int ModelId = db.Plans.Where(p => p.PlanId == planId).Select(p => p.ModelId).SingleOrDefault();
            if (improvementActivities.Count > 0)
            {
            //// Get Model id based on effective date From.
            ModelId = GetModelId(improvementActivities.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), ModelId);
            }
            List<int> modelids = new List<int>();
            modelids.Add(ModelId);
            List<ModelStageRelationList> modleStageRelationList = GetModelStageRelation(modelids);
            List<StageRelation> stageModelRelation = modleStageRelationList.Single(m => m.ModelId == ModelId).StageList;
            List<StageRelation> finalStageResult = new List<StageRelation>();
            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0 && isIncludeImprovement)
            {
                var improvementTypeList = improvementActivities.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                var improvementIdsWeighList = db.ImprovementTacticType_Metric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();

                foreach (StageList stage in stageList)
                {
                    StageRelation stageRelationObj = new StageRelation();
                    var stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Weight > 0).ToList();
                    double impcount = stageimplist.Count();
                    double impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
                    double improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Single(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Value, stageModelRelation.Single(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Value, impcount, impWeight);
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

        public static List<TacticStageValue> GetTacticStageValueListForImprovement(List<Plan_Campaign_Program_Tactic> marketingActivities, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities)
        {
            MRPEntities dbStage = new MRPEntities();
            List<StageRelation> stageRelation = CalculateStageValue(Sessions.PlanId, improvementActivities, true);
            List<Stage> stageList = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            List<TacticStageValue> tacticStageList = new List<TacticStageValue>();
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = stageList.Single(s => s.Code.Equals(stageINQ)).Level.Value;
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = stageList.Single(s => s.Code.Equals(stageMQL)).Level.Value;
            string stageCW = Enums.Stage.CW.ToString();
            int levelCW = stageList.Single(s => s.Code.Equals(stageCW)).Level.Value;
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
                int projectedStageLevel = stageList.Single(s => s.StageId == tactic.StageId).Level.Value;
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
        /// <returns></returns>
        public static List<Guid> GetSubOrdinatesWithPeersNLevel()
        {
            //Get all subordinates of current user
            BDSService.BDSServiceClient objBDSService = new BDSServiceClient();
            List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
            lstUserHierarchy = objBDSService.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
            List<Guid> lstSubordinaresId = new List<Guid>();
            List<Guid> lstSubOrdinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId)
                                                         .ToList()
                                                         .Select(u => u.UserId).ToList();

            while (lstSubOrdinates.Count > 0)
            {
                lstSubOrdinates.ForEach(u => lstSubordinaresId.Add(u));

                lstSubOrdinates = lstUserHierarchy.Where(u => lstSubOrdinates.Contains(u.ManagerId.GetValueOrDefault(Guid.Empty))).ToList().Select(u => u.UserId).ToList();
            }


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
                        lstSubordinaresId = lstSubordinaresId.Concat(lstPeersSubOrdinatesId).ToList();
                    }
                }
            }

            return lstSubordinaresId;
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
            return objBDSServiceClient.GetUserCustomRestrictionList(Sessions.User.UserId, Sessions.ApplicationId).Select(c => new UserCustomRestrictionModel
                                                                                                                        {
                                                                                                                            CustomField = c.CustomField,
                                                                                                                            CustomFieldId = c.CustomFieldId,
                                                                                                                            Permission = c.Permission
                                                                                                                        }).ToList();

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
        public static List<string> GetViewEditBusinessUnitList()
        {
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            return lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId).ToList();
        }
        #endregion

        #region Check ViewEdit rights on businessUnit
        /// <summary>
        /// Check wheather ViewEdit rights on businessUnit is given otr not - for custo restriction
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>02/07/2014</CreatedDate>
        /// <param name="businessUnitId"></param>
        /// <returns></returns>
        public static bool IsBusinessUnitEditable(Guid businessUnitId)
        {
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedBusinessUnits = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId).ToList();
            if (lstAllowedBusinessUnits.Count > 0)
            {
                List<Guid> lstViewEditBusinessUnits = new List<Guid>();
                lstAllowedBusinessUnits.ForEach(g => lstViewEditBusinessUnits.Add(Guid.Parse(g)));
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
        /// <param name="modelId"></param>
        /// <param name="goalType"></param>
        /// <param name="goalValue"></param>
        /// <returns></returns>
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
                        objBudgetAllocationModel.MQLValue = MQLValue;

                        // Calculate Revenue
                        cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double RevenueValue = (inputValue) * (modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize;
                        objBudgetAllocationModel.RevenueValue = RevenueValue;
                    }
                    else if (goalType == Enums.PlanGoalType.MQL.ToString())
                    {
                        // Calculate INQ
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        var modelFunnelStageListINQ = ModelFunnelStageList.Where(mfs => mqlStagelist.Contains(mfs.StageId)).ToList();
                        double INQValue = (inputValue) / (modelFunnelStageListINQ.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                        objBudgetAllocationModel.INQValue = INQValue;

                        // Calculate Revenue
                        cwStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double RevenueValue = (INQValue) * (modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize;
                        objBudgetAllocationModel.RevenueValue = RevenueValue;

                    }
                    else if (goalType == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                    {
                        // Calculate INQ
                        cwStagelist = stageList.Where(s => s.Level >= levelINQ && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double convalue = ((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize);
                        double INQValue = (inputValue) / convalue;//((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize);
                        objBudgetAllocationModel.INQValue = INQValue;

                        // Calculate MQL
                        cwStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double MQLValue = (INQValue) * (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
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
        public static double CalculateMQLOnly(int modelId, string goalType, string goalValue, double averageDealSize)
        {
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
                
                if (goalType != "" && goalValue != "" && goalValue != "0")
                {
                    string CR = Enums.StageType.CR.ToString();
                    double inputValue = Convert.ToInt64(goalValue.Trim().Replace(",", "").Replace("$", ""));

                    if (goalType == Enums.PlanGoalType.INQ.ToString())
                    {
                        // Calculate MQL
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = dbStage.Model_Funnel_Stage.Where(mfs => mfs.Model_Funnel.ModelId == modelId && mqlStagelist.Contains(mfs.StageId) && mfs.StageType == CR).ToList();
                        double MQLValue = (inputValue) * (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                        return MQLValue;
                    }
                    else if (goalType == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                    {
                        // Calculate INQ
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = dbStage.Model_Funnel_Stage.Where(mfs => mfs.Model_Funnel.ModelId == modelId && mqlStagelist.Contains(mfs.StageId) && cwStagelist.Contains(mfs.StageId) && mfs.StageType == CR).ToList();
                        double INQValue = (inputValue) / ((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize);
                        
                        // Calculate MQL
                        var modelFunnelStageListMQL = dbStage.Model_Funnel_Stage.Where(mfs => mfs.Model_Funnel.ModelId == modelId && mqlStagelist.Contains(mfs.StageId) && mfs.StageType == CR).ToList();
                        double MQLValue = (INQValue) * (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
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
            items.Add(new SelectListItem { Text = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString(), Value = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString(), Value = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString(), Selected = true });
            items.Add(new SelectListItem { Text = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString(), Value = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString(), Selected = false });
            return items;
        }
        #endregion

        #endregion
    }

    ////Start Manoj PL #490 Date:27May2014
    ///// <summary>
    ///// Class to maintain the session
    ///// </summary>
    //public class LoginSession
    //{
    //    public string SessionId { get; set; }
    //    public string UserId { get; set; }

    //    /// <summary>
    //    /// Add current session details
    //    /// </summary>
    //    /// <param name="SessionId"></param>
    //    /// <param name="UserId"></param>
    //    /// <returns></returns>
    //    public bool AddSession(string SessionId, string UserId)
    //    {
    //        bool isSessionExist = false;
    //        List<LoginSession> a = (List<LoginSession>)HttpContext.Current.Application["CurrentSession"];
    //        if (a == null || a.Count <= 0)
    //        {
    //            a = new List<LoginSession>();
    //        }
    //        LoginSession l = new LoginSession();
    //        l.SessionId = SessionId;
    //        l.UserId = UserId;
    //        if (a.Count > 0)
    //        {
    //            if (a.Find(l1 => l1.SessionId == SessionId) == null)
    //            {
    //                a.Add(l);
    //            }
    //            else
    //            {
    //                isSessionExist = true;
    //            }
    //        }
    //        else
    //        {
    //            a.Add(l);
    //        }
    //        HttpContext.Current.Application["CurrentSession"] = a;
    //        return isSessionExist;
    //    }

    //    /// <summary>
    //    /// Remove session details
    //    /// </summary>
    //    /// <param name="SessionId"></param>
    //    /// <param name="UserId"></param>
    //    public void RemoveSession(string SessionId, string UserId)
    //    {
    //        List<LoginSession> a = (List<LoginSession>)HttpContext.Current.Application["CurrentSession"];
    //        if (a != null)
    //        {
    //            if (a.Find(l1 => l1.SessionId == SessionId) != null)
    //            {
    //                a.Remove(a.Find(l1 => l1.SessionId == SessionId));
    //                HttpContext.Current.Application["CurrentSession"] = a;
    //            }
    //        }

    //    }
    //}
    ////End Manoj PL #490 Date:27May2014
}