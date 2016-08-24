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
using System.Web.Caching;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Xml;
using RevenuePlanner.Services;

namespace RevenuePlanner.Helpers
{
    public class Common
    {
        #region Declarations
        public static RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        public static double PlanExchangeRate = 0;
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

        public static string xmlMsgFilePath = HttpContext.Current.Request.ApplicationPath == null ? string.Empty : HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLCommonMsgFilePath"));
        public static string xmlBenchmarkFilePath = HttpContext.Current.Request.ApplicationPath == null ? string.Empty : HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLBenchmarkFilePath"));


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

        //----------------------- Constants for Report Conversion Summary ----------------------//
        public const string Actuals = "Actuals";
        public const string Plan = "Plan";
        public const string Trend = "Trend";

        /*-------------#region ReportRevenue---------------*/
        public static string RevenuePlans = "Plans";
        public static string RevenueCampaign = "Campaign";
        public static string RevenueProgram = "Program";
        public static string RevenueTactic = "Tactic";
        public static string RevenueROIPackage = "ROIPackage";
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
        public static string LineItemTitleDefault = "Line item _";

        // Label text for Unallocated Budget label
        public static string UnallocatedBudgetLabelText = "Unallocated";

        //Added by Mitesh Vaishnav for PL ticket #659 - static values for Model integrationinstance,integrationInstancetype ot Last sync are null. 2)dateformate for Last sync values
        public static string TextForModelIntegrationInstanceNull = "None";
        public static string TextForModelIntegrationInstanceTypeOrLastSyncNull = "---";
        public static string DateFormatForModelIntegrationLastSync = "MM/dd/yyyy hh:mm tt";
        public const string GameplanIntegrationService = "Plan Integration Service";
        public static string DateFormateForInspectPopupDescription = "MMMM dd";

        //Added By Kalpesh Sharma
        public const string CustomTitle = "Custom";
        public const string CampaignCustomTitle = "CampaignCustom";
        public const string ProgramCustomTitle = "ProgramCustom";
        public const string TacticCustomTitle = "TacticCustom";
        public const string LineitemCustomTitle = "LineitemCustom";

        //Added By Sohel Pathan
        public static string ColorCodeForCustomField = "";

        public static string RedirectOnServiceUnavailibilityPage = "~/Login/ServiceUnavailable";
        public static string RedirectOnDBServiceUnavailibilityPage = "~/Login/DBServiceUnavailable";

        /// <summary>
        /// Color code list for get random color .
        /// </summary>
        public static List<string> ColorcodeList = new List<string> { "27a4e5", "6ae11f", "bbb748", "bf6a4b", "ca3cce", "7c4bbf", "1af3c9", "f1eb13", "c7893b", "e42233", "a636d6", "2940e2", "0b3d58", "244c0a", "414018", "472519", "4b134d", "2c1947", "055e4d", "555305", "452f14", "520a10", "3e1152", "0c1556", "73c4ee", "9ceb6a", "d2cf86", "d59e89", "dc80df", "a989d5", "6bf7dc", "f6f263", "dab17d", "eb6e7a", "c57de4", "7483ec", "1472a3", "479714", "7f7c2f", "86472f", "8e2590", "542f86", "09af8f", "a6a10a", "875c26", "9e1320", "741f98", "1627a0" };
        public static string ActivityNextYearChartColor = "#407B22";
        //public static string ActivityChartColor = "#c633c9";// Bg color of chart changed #2312 - Bhumika 
        public static string ActivityChartColor = "#5F91B3";
        public static string Campaign_InspectPopup_Flag_Color = "C6EBF3";
        public static string Plan_InspectPopup_Flag_Color = "C6EBF3";
        public static string Program_InspectPopup_Flag_Color = "3DB9D3";

        public static List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };
        public static string[] quarterPeriods = new string[] { "Y1", "Y2", "Y3" };

        #region Constant Month declaration
        public const string Jan = "Y1";
        public const string Feb = "Y2";
        public const string Mar = "Y3";
        public const string Apr = "Y4";
        public const string May = "Y5";
        public const string Jun = "Y6";
        public const string Jul = "Y7";
        public const string Aug = "Y8";
        public const string Sep = "Y9";
        public const string Oct = "Y10";
        public const string Nov = "Y11";
        public const string Dec = "Y12";
        #endregion
        public static string labelThisYear = "(this year)";
        public const string MonthlyBudgetForEntity = "BudgetMonth";
        public const string YearlyBudgetForEntity = "BudgetYear";
        public const string MonthlyCostForEntity = "CostMonth";
        public const string YearlyCostForEntity = "CostYear";

        public static List<string> TOPRevenueColumnList = new List<string>() { "Name", "Revenue", "Trend" };
        public static List<string> TOPPerformanceColumnList = new List<string>() { "Name", "Proj. vs Goal", "Trend" };
        public static List<string> TOPCostColumnList = new List<string>() { "Name", "Cost", "Trend" };
        public static List<string> TOPROIColumnList = new List<string>() { "Name", "ROI", "Trend" };

        #region "Eloqua ClientID & ClientSecret"
        public static string eloquaClientIdLabel = "ClientId";
        public static string eloquaClientSecretLabel = "ClientSecret";
        #endregion

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
            catch (Exception)
            {
                //// Changed By: Maninder Singh Wadhva to avoid argument null exception.
                //Elmah.ErrorLog.GetDefault(null).Log(new Error(ex));
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
            MRPEntities db = new MRPEntities();
            Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(Action));
            //string emailBody, email, Username; //Commented by Rahul Shah on 24/09/2015 for PL #1620
            for (int i = 0; i <= EmailIds.Count - 1; i++)
            {
                String emailBody = string.Empty, email, Username; //modified by Rahul Shah on 24/09/2015 for PL #1620    line nuumber 554

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
                email = EmailIds.ElementAt(i);
                Username = CollaboratorUserName.ElementAt(i);
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
        public static void SendNotificationMailForOwnerChanged(List<string> EmailIds, string NewOwnerName, string ModifierName, string TacticName, string ProgramName, string CampaignName, string PlanName, string Section, string URL, string LineItemName = "") //Modified by Rahul Shah on 03/09/2015 fo PL Ticket #1521. passed URL
        {
            string emailBody, OwnerChanged, email;
            MRPEntities db = new MRPEntities();
            OwnerChanged = Enums.Custom_Notification.TacticOwnerChanged.ToString();
            if (Enums.Section.Program.ToString().ToLower() == Section)
            {
                OwnerChanged = Enums.Custom_Notification.ProgramOwnerChanged.ToString();
            }
            else if (Enums.Section.Campaign.ToString().ToLower() == Section)
            {
                OwnerChanged = Enums.Custom_Notification.CampaignOwnerChanged.ToString();
            }
            //Added by Rahul Shah on 09/03/2016 for PL #1939
            else if (Enums.Section.Plan.ToString().ToLower() == Section)
            {
                OwnerChanged = Enums.Custom_Notification.PlanOwnerChanged.ToString();
            }
            else if (Enums.Section.LineItem.ToString().ToLower() == Section)
            {
                OwnerChanged = Enums.Custom_Notification.LineItemOwnerChanged.ToString();
            }

            Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(OwnerChanged));

            for (int i = 0; i <= EmailIds.Count - 1; i++)
            {
                emailBody = email = string.Empty;
                emailBody = notification.EmailContent;
                emailBody = emailBody.Replace("[NameToBeReplaced]", NewOwnerName);
                emailBody = emailBody.Replace("[ModifierName]", ModifierName);
                emailBody = emailBody.Replace("[campaignname]", CampaignName);
                emailBody = emailBody.Replace("[planname]", PlanName);
                emailBody = emailBody.Replace("[URL]", URL);

                if (Enums.Section.Program.ToString().ToLower() == Section || Enums.Section.Tactic.ToString().ToLower() == Section || Enums.Section.LineItem.ToString().ToLower() == Section)
                {
                    emailBody = emailBody.Replace("[programname]", ProgramName);
                }

                if (Enums.Section.Tactic.ToString().ToLower() == Section || Enums.Section.LineItem.ToString().ToLower() == Section)
                {
                    emailBody = emailBody.Replace("[tacticname]", TacticName);
                }
                if (Enums.Section.LineItem.ToString().ToLower() == Section)
                {
                    emailBody = emailBody.Replace("[lineitemname]", LineItemName);
                }
                email = EmailIds.ElementAt(i);
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
            List<string> List_NotificationUserIds = new List<string>();
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

            if(Sessions.User != null && lst_CollaboratorId.Contains(Convert.ToString(Sessions.User.UserId)))
            {
                lst_CollaboratorId.Remove(Convert.ToString(Sessions.User.UserId));
            }

            if (lst_CollaboratorId.Count > 0)
            {
               
                var csv = string.Join(", ", lst_CollaboratorId);
                var NotificationName = status;

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
                        List_NotificationUserIds = GetAllNotificationUserIds(lst_CollaboratorId, Enums.Custom_Notification.TacticIsApproved.ToString().ToLower());
                        if (List_NotificationUserIds.Count > 0)
                        {
                            lst_CollaboratorEmail = UsersDetails.Where(ids => List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                        }

                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticApproved.ToString(), "", Convert.ToString(Enums.Section.Tactic).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        List_NotificationUserIds = GetAllNotificationUserIds(lst_CollaboratorId, Enums.Custom_Notification.ProgramIsApproved.ToString().ToLower());
                        if (List_NotificationUserIds.Count > 0)
                        {
                            lst_CollaboratorEmail = UsersDetails.Where(ids => List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                        }

                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramApproved.ToString(), "", Convert.ToString(Enums.Section.Program).ToLower(), planTacticId, PlanId, URL);
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        List_NotificationUserIds = GetAllNotificationUserIds(lst_CollaboratorId, Enums.Custom_Notification.CampaignIsApproved.ToString().ToLower());
                        if (List_NotificationUserIds.Count > 0)
                        {
                            lst_CollaboratorEmail = UsersDetails.Where(ids => List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                        }

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
                    List<string> UserName = new List<string>();
                    List<string> EmailId = new List<string>();
                    List<string> UserId = new List<string>();
                    UserName.Add(UsersDetails.FirstOrDefault(u => u.UserId == createdBy).FirstName);
                    EmailId.Add(UsersDetails.FirstOrDefault(u => u.UserId == createdBy).Email);
                    UserId.Add(createdBy.ToString());
                    // To add manager's email address, By dharmraj, Ticket #537
                    var lstUserHierarchy = objBDSUserRepository.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                    var objOwnerUser = lstUserHierarchy.FirstOrDefault(u => u.UserId == createdBy);
                    if (objOwnerUser.ManagerId != null)
                    {
                        EmailId.Add(lstUserHierarchy.FirstOrDefault(u => u.UserId == objOwnerUser.ManagerId).Email);
                        UserName.Add(lstUserHierarchy.FirstOrDefault(u => u.UserId == objOwnerUser.ManagerId).FirstName);
                        UserId.Add(objOwnerUser.ManagerId.ToString());

                        lst_CollaboratorEmail = EmailId;
                        lst_CollaboratorUserName = UserName;
                    }

                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                         List_NotificationUserIds = GetAllNotificationUserIds(UserId, Enums.Custom_Notification.TacticIsSubmitted.ToString().ToLower());
                        if (List_NotificationUserIds.Count > 0)
                        {
                            lst_CollaboratorEmail = UsersDetails.Where(ids => List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                        }

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
                else if (status.Equals(Enums.Custom_Notification.CommentAddedToTactic.ToString()) && iscomment)
                {
                    List_NotificationUserIds = GetAllNotificationUserIds(lst_CollaboratorId, NotificationName.ToLower());
                    if (List_NotificationUserIds.Count > 0)
                    {
                        lst_CollaboratorEmail = UsersDetails.Where(ids=> List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                    }

                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.TacticCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Tactic).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.CommentAddedToProgram.ToString()) && iscomment)
                {
                    List_NotificationUserIds = GetAllNotificationUserIds(lst_CollaboratorId, NotificationName.ToLower());
                    if (List_NotificationUserIds.Count > 0)
                    {
                        lst_CollaboratorEmail = UsersDetails.Where(ids => List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                    }

                    if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        SendNotificationMail(lst_CollaboratorEmail, lst_CollaboratorUserName, title, PlanName, Enums.Custom_Notification.ProgramCommentAdded.ToString(), comment, Convert.ToString(Enums.Section.Program).ToLower(), planTacticId, PlanId, URL);
                    }
                }
                else if (status.Equals(Enums.Custom_Notification.CommentAddedToCampaign.ToString()) && iscomment)
                {
                    List_NotificationUserIds = GetAllNotificationUserIds(lst_CollaboratorId, NotificationName.ToLower());
                    if (List_NotificationUserIds.Count > 0)
                    {
                        lst_CollaboratorEmail = UsersDetails.Where(ids => List_NotificationUserIds.Contains(Convert.ToString(ids.UserId))).Select(u => u.Email).ToList();
                    }

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


        /// <summary>
        /// Added by Komal to get users who has notification on .
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        #region Method to get users who has notification on

        public static List<string> GetAllNotificationUserIds(List<string> UserIds,string NotificationName)
        {
            List<string> userids = new List<string>();
            MRPEntities db = new MRPEntities();
           var users = (from Uids in db.User_Notification
                       join Nids in db.Notifications on Uids.NotificationId equals Nids.NotificationId
                       where Nids.IsDeleted == false && Nids.NotificationType == "AM" && Nids.NotificationInternalUseOnly == NotificationName
                       select Uids
                      ).ToList();
            userids = users.Where(Uids => UserIds.Contains(Convert.ToString(Uids.UserId))).Select(UIds => Convert.ToString(UIds.UserId)).Distinct().ToList();
            return userids;
        }
        #endregion

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

            //// Getting active model of client. 
            var modelIds = db.Models.Where(m => m.ClientId == Sessions.User.ClientId && m.IsDeleted == false).Select(m => m.ModelId).ToList();

            //// Getting active plans of ModelId(s).
            var activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).Select(p => p).ToList();
            return activePlan;
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
                //endDate = startDate.AddMonths(month).AddTicks(-1); // Commented By Nishant Sheth Desc:: Give wrong enddate
                endDate = startDate.AddMonths(1).AddDays(-1); // Add By Nishant Sheth Desc :: Get Month EndDate
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
            else
            {
                string[] PlanYears = currentView.Split('-');
                startDate = new DateTime(Convert.ToInt32(PlanYears[0]), 1, 1);
                endDate = new DateTime(Convert.ToInt32(PlanYears[1]), 12, 31);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetEndDateAsPerCalendar(DateTime calendarStartDate, DateTime calendarEndDate, DateTime startDate, DateTime endDate)
        {
            startDate = startDate < calendarStartDate ? calendarStartDate : startDate;
            double re = endDate > calendarEndDate ? ((calendarEndDate.AddDays(1).AddTicks(-1)) - startDate).TotalDays : ((endDate.AddDays(1).AddTicks(-1)) - startDate).TotalDays;
            return re;
        }

        /// <summary>
        /// Function to get last updated date time for current plan.
        /// Modified By Maninder Singh Wadhva to Address PL#203
        /// </summary>
        /// <param name="plan">Plan.</param>
        /// <returns>Returns last updated date time.</returns>
        public static DateTime GetLastUpdatedDate(int planId)
        {
            CacheObject dataCache = new CacheObject();
            MRPEntities db = new MRPEntities();
            //var plan = db.Plans.FirstOrDefault(p => p.PlanId.Equals(planId));
            DataSet dsPlanCampProgTac = (DataSet)dataCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            var plan = dsPlanCampProgTac.Tables[0].AsEnumerable().Select(row => new Plan
            {
                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])),
                IsActive = Convert.ToBoolean(Convert.ToString(row["IsActive"])),
                IsDeleted = Convert.ToBoolean(Convert.ToString(row["IsDeleted"])),
                ModelId = int.Parse(Convert.ToString(row["ModelId"])),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanId = int.Parse(Convert.ToString(row["PlanId"]))
            }).Where(row => row.PlanId.Equals(planId)).FirstOrDefault();
            List<DateTime?> lastUpdatedDate = new List<DateTime?>();
            if (plan.CreatedDate != null)
            {
                lastUpdatedDate.Add(plan.CreatedDate);
            }

            if (plan.ModifiedDate != null)
            {
                lastUpdatedDate.Add(plan.ModifiedDate);
            }

            var campaignList = dsPlanCampProgTac.Tables[1].AsEnumerable().Select(row => new Plan_Campaign
            {
                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                PlanId = Convert.ToInt32(row["PlanId"])
            }).ToList();

            var programList = dsPlanCampProgTac.Tables[2].AsEnumerable().Select(row => new Plan_Campaign_Program
            {

                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                PlanProgramId = Convert.ToInt32(row["PlanProgramId"])
            }).ToList();

            var tacticList = dsPlanCampProgTac.Tables[3].AsEnumerable().Select(row => new Custom_Plan_Campaign_Program_Tactic
            {
                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                LinkedPlanId = Convert.ToInt32(string.IsNullOrEmpty(Convert.ToString(row["LinkedPlanId"])) ? (int?)null : row["LinkedPlanId"]),
                LinkedTacticId = Convert.ToInt32(string.IsNullOrEmpty(Convert.ToString(row["LinkedTacticId"])) ? (int?)null : row["LinkedTacticId"]),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanProgramId = Convert.ToInt32(row["PlanProgramId"]),
                PlanTacticId = Convert.ToInt32(row["PlanTacticId"]),
                PlanId = Convert.ToInt32(row["PlanId"])
            }).ToList();

            if (tacticList.Count() > 0)
            {

                var planTacticModifiedDate = tacticList.Select(t => t.ModifiedDate).Max();
                lastUpdatedDate.Add(planTacticModifiedDate);

                var planTacticCreatedDate = tacticList.Select(t => t.CreatedDate).Max();
                lastUpdatedDate.Add(planTacticCreatedDate);

                var planProgramModifiedDate = programList.Select(t => t.ModifiedDate).Max();
                lastUpdatedDate.Add(planProgramModifiedDate);

                var planProgramCreatedDate = programList.Select(t => t.CreatedDate).Max();
                lastUpdatedDate.Add(planProgramCreatedDate);

                var planCampaignModifiedDate = campaignList.Select(t => t.ModifiedDate).Max();
                lastUpdatedDate.Add(planCampaignModifiedDate);

                var planCampaignCreatedDate = campaignList.Select(t => t.CreatedDate).Max();
                lastUpdatedDate.Add(planCampaignCreatedDate);

                //var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                //                                                               .Select(pc => pc);
                var planTacticComment = db.GetCollaboratorId(planId).ToList();
                if (planTacticComment.Count() > 0)
                {
                    var planTacticCommentCreatedDate = planTacticComment.Select(pc => pc.CreatedDate).Max();
                    lastUpdatedDate.Add(planTacticCommentCreatedDate);
                }
            }

            return Convert.ToDateTime(lastUpdatedDate.Max());
        }

        public static List<string> GetCollaboratorId(int planId)
        {
            MRPEntities db = new MRPEntities();

            // Add By Nishant Sheth
            // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
            CacheObject dataCache = new CacheObject();
            StoredProcedure objSp = new StoredProcedure();
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(planId.ToString());
            dataCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

            //var plan = db.Plans.FirstOrDefault(p => p.PlanId.Equals(planId) && p.IsDeleted.Equals(false));

            var plan = dsPlanCampProgTac.Tables[0].AsEnumerable().Select(row => new Plan
            {
                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])),
                IsActive = Convert.ToBoolean(Convert.ToString(row["IsActive"])),
                IsDeleted = Convert.ToBoolean(Convert.ToString(row["IsDeleted"])),
                ModelId = int.Parse(Convert.ToString(row["ModelId"])),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanId = int.Parse(Convert.ToString(row["PlanId"]))
            }).FirstOrDefault();

            List<string> collaboratorId = new List<string>();
            if (plan.ModifiedBy != null)
            {
                collaboratorId.Add(plan.ModifiedBy.ToString());
            }

            if (plan.CreatedBy != null)
            {
                collaboratorId.Add(plan.CreatedBy.ToString());
            }

            var campaignList = dsPlanCampProgTac.Tables[1].AsEnumerable().Select(row => new Plan_Campaign
            {
                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                PlanId = Convert.ToInt32(row["PlanId"])
            }).ToList();

            var programList = dsPlanCampProgTac.Tables[2].AsEnumerable().Select(row => new Plan_Campaign_Program
            {

                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                PlanProgramId = Convert.ToInt32(row["PlanProgramId"])
            }).ToList();

            var tacticList = dsPlanCampProgTac.Tables[3].AsEnumerable().Select(row => new Custom_Plan_Campaign_Program_Tactic
            {
                CreatedBy = Guid.Parse(Convert.ToString(row["CreatedBy"])),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                LinkedPlanId = Convert.ToInt32(string.IsNullOrEmpty(Convert.ToString(row["LinkedPlanId"])) ? (int?)null : row["LinkedPlanId"]),
                LinkedTacticId = Convert.ToInt32(string.IsNullOrEmpty(Convert.ToString(row["LinkedTacticId"])) ? (int?)null : row["LinkedTacticId"]),
                ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                PlanProgramId = Convert.ToInt32(row["PlanProgramId"]),
                PlanTacticId = Convert.ToInt32(row["PlanTacticId"]),
                PlanId = Convert.ToInt32(row["PlanId"])
            }).ToList();

            //List<Plan_Campaign_Program_Tactic> planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId) && t.IsDeleted.Equals(false)).Select(t => t).ToList();

            if (tacticList != null && tacticList.Any())
            {
                var planTacticModifiedBy = tacticList.Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
                var planTacticCreatedBy = tacticList.Select(t => t.CreatedBy.ToString()).ToList();

                var planProgramModifiedBy = programList.Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
                var planProgramCreatedBy = programList.Select(t => t.CreatedBy.ToString()).ToList();

                var planCampaignModifiedBy = campaignList.Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
                var planCampaignCreatedBy = campaignList.Select(t => t.CreatedBy.ToString()).ToList();

                collaboratorId.AddRange(planTacticCreatedBy);
                collaboratorId.AddRange(planTacticModifiedBy);
                collaboratorId.AddRange(planProgramCreatedBy);
                collaboratorId.AddRange(planProgramModifiedBy);
                collaboratorId.AddRange(planCampaignCreatedBy);
                collaboratorId.AddRange(planCampaignModifiedBy);
            }


            //List<Plan_Campaign_Program_Tactic_Comment> planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
            //                                                               .Select(pc => pc).ToList();
            // Add By Nishant Sheth
            // Desc :: #1915 for performance
            var planTacticComment = db.GetCollaboratorId(planId);
            var planTacticCommentCreatedBy = planTacticComment.Select(pc => pc.CreatedBy.ToString()).ToList();
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
                BDSServiceClient objBDSUserRepository = new BDSServiceClient();
                MRPEntities db = new MRPEntities();
                List<string> newCollaboratorId = new List<string>();
                List<object> data = new List<object>();
                {
                    List<string> collaboratorIds = Common.GetCollaboratorId(planId).Distinct().ToList();
                    List<User> lstUserDetails = new List<User>();
                    lstUserDetails = objBDSUserRepository.GetMultipleTeamMemberNameByApplicationId(string.Join(",", collaboratorIds), Sessions.ApplicationId);
                    foreach (string userId in lstUserDetails.Select(x => x.UserId.ToString()))
                    {
                        if (System.Web.HttpContext.Current.Cache[userId + "_photo"] != null)
                        {
                            var userData = new { imageBytes = System.Web.HttpContext.Current.Cache[userId + "_photo"], name = System.Web.HttpContext.Current.Cache[userId + "_name"], jobTitle = System.Web.HttpContext.Current.Cache[userId + "_jtitle"] }; //uday #416
                            data.Add(userData);
                        }
                        else
                        {
                            newCollaboratorId.Add(userId);
                        }
                    }
                }

                byte[] imageBytesUserImageNotFound = Common.ReadFile(HttpContext.Current.Server.MapPath("~") + "/content/images/user_image_not_found.png");

                List<User> users = objBDSUserRepository.GetMultipleTeamMemberDetails(string.Join(",", newCollaboratorId), Sessions.ApplicationId);

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

                    string imageBytesBase64String = Convert.ToBase64String(imageBytes);
                    System.Web.HttpContext.Current.Cache[user.UserId + "_photo"] = imageBytesBase64String;
                    System.Web.HttpContext.Current.Cache[user.UserId + "_name"] = user.FirstName + " " + user.LastName;
                    //////System.Web.HttpContext.Current.Cache[user.UserId + "_bu"] = busititle;//uday #416
                    System.Web.HttpContext.Current.Cache[user.UserId + "_jtitle"] = user.JobTitle;//uday #416
                    var userData = new { imageBytes = imageBytesBase64String, name = user.FirstName + " " + user.LastName, jobTitle = user.JobTitle };//added by uday buid & title #416 };
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

        public static int InsertChangeLog(int? objectId, int? parentObjectId, int? componentId, string componentTitle, Enums.ChangeLog_ComponentType componentType, Enums.ChangeLog_TableName TableName, Enums.ChangeLog_Actions action, string actionSuffix = "", string EntityOwnerId = "", string ReportRecipientUserIds = "")
        {
            /************************** Get value of component type ******************************/
            var type = typeof(Enums.ChangeLog_ComponentType);
            var memInfo = type.GetMember(componentType.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)attributes[0]).Description;
            /*************************************************************************************/
            //Modified by komal rawal on 16-08-2016 regarding #2484 save notifications 
            int retval = 0;
            MRPEntities db = new MRPEntities();
            StoredProcedure objSp = new StoredProcedure();
            //ChangeLog c1 = new ChangeLog();
            //c1.ActionName = action.ToString();
            //c1.ActionSuffix = actionSuffix;
            //c1.ComponentId = componentId;
            //c1.ComponentTitle = componentTitle;
            //c1.ComponentType = description;
            //c1.IsDeleted = false;
            //c1.ObjectId = objectId;
            //c1.ParentObjectId = parentObjectId;
            //c1.TableName = TableName.ToString();
            //c1.TimeStamp = DateTime.Now;
            //if (Sessions.User != null)
            //{
            //    c1.ClientId = Sessions.User.ClientId;
            //    c1.UserId = Sessions.User.UserId;
            //}
            //else
            //{
            //    c1.ClientId = null;
            //    c1.UserId = null;
            //}

            //db.ChangeLogs.Add(c1);
            var OwnerID = EntityOwnerId;
            var UserName = Convert.ToString(Sessions.User.FirstName + " " + Sessions.User.LastName);
            int ret = objSp.SaveLogNoticationdata(action.ToString(), actionSuffix, componentId, componentTitle, description, objectId, parentObjectId, TableName.ToString(), Sessions.User.ClientId, Sessions.User.UserId, UserName, OwnerID, ReportRecipientUserIds);
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
            List<int?> lst_Objects = new List<int?>();

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
            ChangeLog_ViewModel clvm = new ChangeLog_ViewModel();
            User user = new User();
            foreach (var cl in lst_ChangeLog)
            {
                clvm = new ChangeLog_ViewModel();
                user = new User();
                user = userName.Where(u => u.UserId == cl.UserId).Select(u => u).FirstOrDefault(); //bdsservice.GetTeamMemberDetails(cl.UserId, Sessions.ApplicationId);
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
        public static HomePlanModelHeader GetPlanHeaderValue(int planId, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", bool onlyplan = false, string TabId = "", bool IsHeaderActuals = false)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();
            MRPEntities objDbMrpEntities = new MRPEntities();
            //List<string> tacticStatus = GetStatusListAfterApproved();  // Commented by Rahul Shah on 16/09/2015 for PL #1610
            //Added By Komal Rawal for new UI of homepage
            List<SelectListItem> planList = new List<SelectListItem>();
            // Desc :: To resolve the select and deselct all owner issues
            List<Guid> filterOwner = new List<Guid>();
            string PlanLabel = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();
            //var SetOfPlanSelected = Common.PlanUserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();// Add By Nishant Sheth #1915

            /*Commented By Komal Rawal on 25/2/2016 to get data for all owners*/
            //string planselectedowner = SetOfPlanSelected.Where(view => view.FilterName == Enums.FilterLabel.Owner.ToString()).Select(view => view.FilterValues).FirstOrDefault();
            filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //if (planselectedowner == null)
            //{
            //    filterOwner = Sessions.User.UserId.ToString().Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //}
            // End By Nishant Sheth
            var lstPlanAll = Common.GetPlan();
            if (lstPlanAll.Count > 0)
            {
                planList = lstPlanAll.Select(plan => new SelectListItem() { Text = plan.Title, Value = plan.PlanId.ToString() }).OrderBy(plan => plan.Text).ToList();

                var objexists = planList.Where(plan => plan.Value == planId.ToString()).ToList();
                if (objexists.Count != 0)
                {
                    planList.Single(plan => plan.Value.Equals(planId.ToString())).Selected = true;
                }
                else
                {
                    planList.FirstOrDefault().Selected = true;
                }

                //// Set Plan dropdown values
                if (planList != null)
                    planList = planList.Where(plan => !string.IsNullOrEmpty(plan.Text)).OrderBy(plan => plan.Text, new AlphaNumericComparer()).ToList();
            }
            objHomePlanModelHeader.plans = planList;
            //End
            if (onlyplan)
            {
                return objHomePlanModelHeader;
            }
            var objPlan = lstPlanAll.Where(plan => plan.PlanId == planId).Select(plan => plan).FirstOrDefault();
            if (objPlan != null)
            {
                //List<Plan_Campaign_Program_Tactic> planTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tacticStatus.Contains(tactic.Status) && tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList(); // Commented By Rahul Shah on 16/09/2015 for PL #1610
                List<Plan_Campaign_Program_Tactic> planTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId
                    && tactic.Plan_Campaign_Program.IsDeleted == false && tactic.Plan_Campaign_Program.Plan_Campaign.IsDeleted == false).ToList(); // Added By Rahul Shah on 16/09/2015 for PL #1610

                // Add By Nishant Sheth for Plan Year
                //Modified BY Komal rawal for #1929 proper Hud chart and count
                var CampaignList = planTacticIds.Select(ids => ids.Plan_Campaign_Program.Plan_Campaign).ToList();
                // End By Nishant Sheth
                if (CampaignList.Count > 0)
                {
                    // Add By Nishant Sheth Desc header value wrong with plan tab
                    if (TabId == "liGrid")
                    {
                        int StartYear = CampaignList.Select(camp => camp.StartDate.Year).Min();
                        int EndYear = CampaignList.Select(camp => camp.EndDate.Year).Max();

                        if (EndYear != StartYear)
                        {
                            year = StartYear + "-" + EndYear;
                        }
                        else
                        {
                            year = Convert.ToString(StartYear);

                        }
                    }
                }
                //End
                string planYear = string.Empty;

                if (year != "" && year != null)
                {
                    int Year;
                    // Modified By Komal Rawal to get proper HUd values for #1788
                    string[] ListYear = year.Split('-');

                    bool isNumeric = int.TryParse(year, out Year);

                    if (isNumeric)
                    {
                        planYear = Convert.ToString(year);
                    }
                    else
                    {
                        // Add By Nishant Sheth
                        // Desc :: To Resolved gantt chart year issue
                        if (int.TryParse(ListYear[0], out Year))
                        {
                            planYear = ListYear[0];
                        }
                        else
                        {
                            planYear = DateTime.Now.Year.ToString();
                        }
                        // End By Nishant Sheth
                    }

                    DateTime StartDate;
                    DateTime EndDate;
                    StartDate = EndDate = DateTime.Now;
                    Common.GetPlanGanttStartEndDate(planYear, year, ref StartDate, ref EndDate);

                    planTacticIds = planTacticIds.Where(t => (!((t.EndDate < StartDate) || (t.StartDate > EndDate)))).ToList();

                }
                else
                {
                    //Added by Arpita Soni for solving multiyear issue in budget tab
                    List<Plan> lstPlans = new List<Plan>();
                    lstPlans = Common.GetPlan();
                    Plan objectPlan = lstPlans.Where(x => x.PlanId == planId).FirstOrDefault();
                    if (objectPlan != null)
                    {
                        planYear = objectPlan.Year;
                        year = planYear;
                    }

                    DateTime StartDate;
                    DateTime EndDate;
                    StartDate = EndDate = DateTime.Now;
                    Common.GetPlanGanttStartEndDate(planYear, year, ref StartDate, ref EndDate);

                    planTacticIds = planTacticIds.Where(t => (!((t.EndDate < StartDate) || (t.StartDate > EndDate)))).ToList();
                    //End 
                }
                //Modified By Komal Rawal for #1447
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<int> lstTacticIds = new List<int>();

                //// Owner filter criteria.
                //List<Guid> filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

                //TacticType filter criteria
                List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeids) ? new List<int>() : TacticTypeids.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

                //Status filter criteria
                List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

                //// Custom Field Filter Criteria.
                List<string> filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldId) ? new List<string>() : CustomFieldId.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
                if (filteredCustomFields.Count > 0)
                {
                    filteredCustomFields.ForEach(customField =>
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    });

                }
                lstTacticIds = planTacticIds.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0) && IsHeaderActuals != true)
                {

                    planTacticIds = planTacticIds.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                             (filterTacticType.Contains(pcptobj.TacticType.TacticTypeId)) &&
                                             (filterStatus.Contains(pcptobj.Status))).ToList();

                    //// Apply Custom restriction for None type
                    if (planTacticIds.Count() > 0)
                    {

                        if (filteredCustomFields.Count > 0)
                        {
                            lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                            //// get Allowed Entity Ids
                            planTacticIds = planTacticIds.Where(tacticlist => lstTacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                        }

                    }
                    lstTacticIds = planTacticIds.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    //planTacticIds = planTacticIds.Where(pcptobj => lstAllowedEntityIds.Contains(pcptobj.PlanTacticId) || pcptobj.CreatedBy == Sessions.User.UserId).ToList();
                    // Modified By Nishant sheth
                    // Desc :: to match calendar and grid heads up values
                    planTacticIds = planTacticIds.Where(pcptobj => lstAllowedEntityIds.Contains(pcptobj.PlanTacticId) || (filterOwner.Contains(pcptobj.CreatedBy))).ToList();
                }
                else
                {
                    //bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    //List<string> collaboratorIds = GetAllCollaborators(lstTacticIds).Distinct().ToList();
                    //List<Guid> lstSubordinatesIds = new List<Guid>();
                    //if (IsTacticAllowForSubordinates)
                    //{
                    //    lstSubordinatesIds = GetAllSubordinates(Sessions.User.UserId);

                    //}
                    //List<int> lsteditableEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    // Modified By Nishant Sheth
                    planTacticIds = planTacticIds.Where(tactic => (IsHeaderActuals == true || filterOwner.Contains(tactic.CreatedBy))).Select(tactic => tactic).ToList();


                }

                //End

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
                        string CR = Enums.StageType.CR.ToString();
                        List<Model_Stage> modelFunnelStageList = objDbMrpEntities.Model_Stage.Where(modelfunnelstage => modelfunnelstage.ModelId == objPlan.ModelId && modelfunnelstage.StageType == CR).ToList();


                        double ADSValue = objPlan.Model.AverageDealSize;
                        List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                        objHomePlanModelHeader.MQLs = Common.CalculateMQLOnly(objPlan.ModelId, objPlan.GoalType, objPlan.GoalValue.ToString(), ADSValue, stageList, modelFunnelStageList);
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
                    objHomePlanModelHeader.Budget = objCurrency.GetValueByExchangeRate(objPlan.Budget, PlanExchangeRate);// Modified By Nishant Sheth #2497
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
                        //objHomePlanModelHeader.Budget = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => tacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).Sum(lineItem => lineItem.Cost);//Commented by Rahul Shah on 18/09/2015 for PL #1615
                        ////End Modified by Mitesh Vaishnav for PL ticket #736 Budgeting - Changes to plan header to accomodate budgeting changes

                        //Added by Rahul Shah on 18/09/2015 for PL #1615
                        List<Plan_Campaign_Program_Tactic_LineItem> planTacticLineItemIds = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => tacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).ToList();
                        if (planTacticLineItemIds.Count() > 0)
                        {
                            objHomePlanModelHeader.Budget = objCurrency.GetValueByExchangeRate(planTacticLineItemIds.Sum(lineItem => lineItem.Cost), PlanExchangeRate);// Modified By Nishant Sheth #2497
                        }

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
            else
            {
                string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                if (string.IsNullOrEmpty(MQLStageLabel))
                {
                    objHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.ProjectedMQLLabel.ToString()].ToString();
                }
                else
                {
                    objHomePlanModelHeader.mqlLabel = "Projected " + MQLStageLabel;
                }

                objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Budget.ToString()].ToString();

            }
            return objHomePlanModelHeader;
        }
        // Add By Nishant Sheth
        // Desc :: For Get Header value from cache objcet #2111
        public static HomePlanModelHeader GetPlanHeaderValuePerformance(int planId, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", bool onlyplan = false, string TabId = "", bool IsHeaderActuals = false)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();
            MRPEntities objDbMrpEntities = new MRPEntities();
            CacheObject dataCache = new CacheObject();
            StoredProcedure sp = new StoredProcedure();
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)dataCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());

            //List<string> tacticStatus = GetStatusListAfterApproved();  // Commented by Rahul Shah on 16/09/2015 for PL #1610
            //Added By Komal Rawal for new UI of homepage
            List<SelectListItem> planList = new List<SelectListItem>();
            // Desc :: To resolve the select and deselct all owner issues
            List<Guid> filterOwner = new List<Guid>();
            string PlanLabel = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();
            //Commented by Rahul Shah to resolve Elmah error. 
            //var SetOfPlanSelected = Common.PlanUserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();// Add By Nishant Sheth #1915

            /*Commented By Komal Rawal on 25/2/2016 to get data for all owners*/
            //string planselectedowner = SetOfPlanSelected.Where(view => view.FilterName == Enums.FilterLabel.Owner.ToString()).Select(view => view.FilterValues).FirstOrDefault();
            filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //if (planselectedowner == null)
            //{
            //    filterOwner = Sessions.User.UserId.ToString().Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //}
            // End By Nishant Sheth
            //var lstPlanAll = Common.GetPlan();
            var lstPlanAll = dataCache.Returncache(Enums.CacheObject.Plan.ToString()) as List<Plan>; ;
            if (lstPlanAll.Count > 0)
            {
                planList = lstPlanAll.Select(plan => new SelectListItem() { Text = plan.Title, Value = plan.PlanId.ToString() }).OrderBy(plan => plan.Text).ToList();

                var objexists = planList.Where(plan => plan.Value == planId.ToString()).ToList();
                if (objexists.Count != 0)
                {
                    planList.Single(plan => plan.Value.Equals(planId.ToString())).Selected = true;
                }
                else
                {
                    planList.FirstOrDefault().Selected = true;
                }

                //// Set Plan dropdown values
                if (planList != null)
                    planList = planList.Where(plan => !string.IsNullOrEmpty(plan.Text)).OrderBy(plan => plan.Text, new AlphaNumericComparer()).ToList();
            }
            objHomePlanModelHeader.plans = planList;
            //End
            if (onlyplan)
            {
                return objHomePlanModelHeader;
            }
            var objPlan = lstPlanAll.Where(plan => plan.PlanId == planId).Select(plan => plan).FirstOrDefault();
            if (objPlan != null)
            {
                //List<Plan_Campaign_Program_Tactic> planTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tacticStatus.Contains(tactic.Status) && tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList(); // Commented By Rahul Shah on 16/09/2015 for PL #1610
                //List<Plan_Campaign_Program_Tactic> planTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId
                //    && tactic.Plan_Campaign_Program.IsDeleted == false && tactic.Plan_Campaign_Program.Plan_Campaign.IsDeleted == false).ToList(); // Added By Rahul Shah on 16/09/2015 for PL #1610
                var planTacticIds = ((List<Custom_Plan_Campaign_Program_Tactic>)dataCache.Returncache(Enums.CacheObject.CustomTactic.ToString())).Where(t => t.IsDeleted == false).ToList();
                // Add By Nishant Sheth for Plan Year
                //Modified BY Komal rawal for #1929 proper Hud chart and count
                var CampaignList = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                // End By Nishant Sheth
                if (CampaignList.Count > 0)
                {
                    // Add By Nishant Sheth Desc header value wrong with plan tab
                    if (TabId == "liGrid")
                    {
                        int StartYear = CampaignList.Select(camp => camp.StartDate.Year).Min();
                        int EndYear = CampaignList.Select(camp => camp.EndDate.Year).Max();

                        if (EndYear != StartYear)
                        {
                            year = StartYear + "-" + EndYear;
                        }
                        else
                        {
                            year = Convert.ToString(StartYear);

                        }
                    }
                }
                //End
                if (year != "" && year != null)
                {
                    int Year;
                    // Modified By Komal Rawal to get proper HUd values for #1788
                    string[] ListYear = year.Split('-');
                    string planYear = string.Empty;

                    bool isNumeric = int.TryParse(year, out Year);

                    if (isNumeric)
                    {
                        planYear = Convert.ToString(year);
                    }
                    else
                    {
                        // Add By Nishant Sheth
                        // Desc :: To Resolved gantt chart year issue
                        if (int.TryParse(ListYear[0], out Year))
                        {
                            planYear = ListYear[0];
                        }
                        else
                        {
                            planYear = DateTime.Now.Year.ToString();
                        }
                        // End By Nishant Sheth
                    }

                    DateTime StartDate;
                    DateTime EndDate;
                    StartDate = EndDate = DateTime.Now;
                    Common.GetPlanGanttStartEndDate(planYear, year, ref StartDate, ref EndDate);

                    planTacticIds = planTacticIds.Where(t => (!((t.EndDate < StartDate) || (t.StartDate > EndDate)))).ToList();



                }
                //Modified By Komal Rawal for #1447
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<int> lstTacticIds = new List<int>();

                //// Owner filter criteria.
                //List<Guid> filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

                //TacticType filter criteria
                List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeids) ? new List<int>() : TacticTypeids.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

                //Status filter criteria
                List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

                //// Custom Field Filter Criteria.
                List<string> filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldId) ? new List<string>() : CustomFieldId.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
                if (filteredCustomFields.Count > 0)
                {
                    filteredCustomFields.ForEach(customField =>
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    });

                }
                lstTacticIds = planTacticIds.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                if ((filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0) && IsHeaderActuals != true)
                {

                    planTacticIds = planTacticIds.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                             (filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                             (filterStatus.Contains(pcptobj.Status))).ToList();

                    //// Apply Custom restriction for None type
                    if (planTacticIds.Count() > 0)
                    {

                        if (filteredCustomFields.Count > 0)
                        {
                            lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                            //// get Allowed Entity Ids
                            planTacticIds = planTacticIds.Where(tacticlist => lstTacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                        }

                    }
                    lstTacticIds = planTacticIds.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    //planTacticIds = planTacticIds.Where(pcptobj => lstAllowedEntityIds.Contains(pcptobj.PlanTacticId) || pcptobj.CreatedBy == Sessions.User.UserId).ToList();
                    // Modified By Nishant sheth
                    // Desc :: to match calendar and grid heads up values
                    planTacticIds = planTacticIds.Where(pcptobj => lstAllowedEntityIds.Contains(pcptobj.PlanTacticId) || (filterOwner.Contains(pcptobj.CreatedBy))).ToList();
                }
                else
                {
                    //bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    //List<string> collaboratorIds = GetAllCollaborators(lstTacticIds).Distinct().ToList();
                    //List<Guid> lstSubordinatesIds = new List<Guid>();
                    //if (IsTacticAllowForSubordinates)
                    //{
                    //    lstSubordinatesIds = GetAllSubordinates(Sessions.User.UserId);

                    //}
                    //List<int> lsteditableEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    // Modified By Nishant Sheth
                    planTacticIds = planTacticIds.Where(tactic => (IsHeaderActuals == true || filterOwner.Contains(tactic.CreatedBy))).Select(tactic => tactic).ToList();


                }

                //End

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
                        string CR = Enums.StageType.CR.ToString();
                        List<Model_Stage> modelFunnelStageList = objDbMrpEntities.Model_Stage.Where(modelfunnelstage => modelfunnelstage.ModelId == objPlan.ModelId && modelfunnelstage.StageType == CR).ToList();


                        double ADSValue = objPlan.Model.AverageDealSize;
                        List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                        objHomePlanModelHeader.MQLs = Common.CalculateMQLOnly(objPlan.ModelId, objPlan.GoalType, objPlan.GoalValue.ToString(), ADSValue, stageList, modelFunnelStageList);
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
                    objHomePlanModelHeader.Budget = objCurrency.GetValueByExchangeRate(objPlan.Budget, PlanExchangeRate);// Modified By Nishant Sheth #2497
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Budget.ToString()].ToString();
                }
                else
                {
                    //// Added BY Bhavesh
                    //// Calculate MQL at runtime #376
                    objHomePlanModelHeader.MQLs = GetTacticStageRelationPerformance(planTacticIds, false).Sum(tactic => tactic.MQLValue);
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
                        //objHomePlanModelHeader.Budget = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => tacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).Sum(lineItem => lineItem.Cost);//Commented by Rahul Shah on 18/09/2015 for PL #1615
                        ////End Modified by Mitesh Vaishnav for PL ticket #736 Budgeting - Changes to plan header to accomodate budgeting changes

                        //Added by Rahul Shah on 18/09/2015 for PL #1615
                        List<Plan_Campaign_Program_Tactic_LineItem> planTacticLineItemIds = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => tacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).ToList();
                        if (planTacticLineItemIds.Count() > 0)
                        {
                            objHomePlanModelHeader.Budget = objCurrency.GetValueByExchangeRate(planTacticLineItemIds.Sum(lineItem => lineItem.Cost), PlanExchangeRate);// Modified By Nishant Sheth #2497
                        }

                    }
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Cost.ToString()].ToString();
                }

                var improvementTacticList = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && !improvementTactic.IsDeleted).ToList();
                if (improvementTacticList.Count > 0)
                {
                    //// Added By: Maninder Singh Wadhva
                    //// Addressed PL Ticket: 37,38,47,49
                    //// Getting improved MQL.
                    double? improvedMQL = GetTacticStageRelationPerformance(planTacticIds, true).Sum(tactic => tactic.MQLValue);

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
            else
            {
                string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                if (string.IsNullOrEmpty(MQLStageLabel))
                {
                    objHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.ProjectedMQLLabel.ToString()].ToString();
                }
                else
                {
                    objHomePlanModelHeader.mqlLabel = "Projected " + MQLStageLabel;
                }

                objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Budget.ToString()].ToString();

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
        public static HomePlanModelHeader GetPlanHeaderValueForMultiplePlans(List<int> planIds, string activeMenu, string year, string CustomFieldId, string OwnerIds, string TacticTypeids, string StatusIds)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            HomePlanModelHeader newHomePlanModelHeader = new HomePlanModelHeader();
            MRPEntities db = new MRPEntities();
            //List<string> tacticStatus = GetStatusListAfterApproved();// Commented By Rahul Shah on 16/09/2015 for PL #1610
            int Year;
            // Add By Nishant sheth
            DateTime StartDate;
            DateTime EndDate;
            string[] ListYear = year.Split('-');
            // Modified By Nishant Sheth
            // #1825 stuck of loading overlay with 'this month' and 'this quarter'
            string planYear = string.Empty;

            bool isNumeric = int.TryParse(year, out Year);

            if (isNumeric)
            {
                planYear = Convert.ToString(year);
            }
            else
            {
                // Add By Nishant Sheth
                // Desc :: To Resolved gantt chart year issue
                if (int.TryParse(ListYear[0], out Year))
                {
                    planYear = ListYear[0];
                }
                else
                {
                    planYear = DateTime.Now.Year.ToString();
                }
                // End By Nishant Sheth
            }


            StartDate = EndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, year, ref StartDate, ref EndDate);
            //End
            double TotalMQLs = 0, TotalBudget = 0;
            double? TotalPercentageMQLImproved = 0;
            int TotalTacticCount = 0;


            // Modify by Nishant sheth
            // Desc :: to get correct count for tactic and for multiple years #1750
            var planList = db.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
            var planData = planList.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == year).Select(a => a.PlanId).ToList();

            var campplist = db.Plan_Campaign.Where(camp => (!((camp.EndDate < StartDate) || (camp.StartDate > EndDate))) && planIds.Contains(camp.PlanId)).Select(a => new { PlanCampaignId = a.PlanCampaignId, PlanId = a.PlanId }).ToList();
            var campplanid = campplist.Select(a => a.PlanId).ToList();
            var campid = campplist.Select(a => a.PlanCampaignId).ToList();
            // Desc :: To resolve the select and deselct all owner issues
            List<Guid> filterOwner = new List<Guid>();
            string PlanLabel = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = db.Plan_UserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();
            var SetOfPlanSelected = Common.PlanUserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();// Add By Nishant Sheth #1915
            /*Commented By Komal Rawal on 25/2/2016 to get data for all owners*/
            //string planselectedowner = SetOfPlanSelected.Where(view => view.FilterName == Enums.FilterLabel.Owner.ToString()).Select(view => view.FilterValues).FirstOrDefault();
            filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //if (planselectedowner == null)
            //{
            //    filterOwner = Sessions.User.UserId.ToString().Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //}
            // End By Nishant Sheth

            // End by Nishant Sheth
            //var planList = db.Plans.Where(p => planIds.Contains(p.PlanId) && p.IsDeleted == false && p.IsActive == true && p.Year == year).Select(m => m).ToList();

            if (planList != null && planList.Count > 0)
            {
                // Modify by Nishant Sheth
                // Desc :: to manage multiple years plan id #1750
                var innerplanids = planList.Where(a => campplanid.Count > 0 ? campplanid.Contains(a.PlanId) : planIds.Contains(a.PlanId)).Select(plan => plan.PlanId).ToList();
                //Modified By Komal Rawal for #1447
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<int> lstTacticIds = new List<int>();


                //// Owner filter criteria.
                //filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

                //TacticType filter criteria
                List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeids) ? new List<int>() : TacticTypeids.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

                //Status filter criteria
                List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

                //// Custom Field Filter Criteria.
                List<string> filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldId) ? new List<string>() : CustomFieldId.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
                if (filteredCustomFields.Count > 0)
                {
                    filteredCustomFields.ForEach(customField =>
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    });

                }


                //List<Plan_Tactic> planTacticsList = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && tacticStatus.Contains(t.Status) && innerplanids.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => new Plan_Tactic { objPlanTactic = tactic, PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId }).ToList();// Commented By Rahul Shah on 16/09/2015 for PL #1610
                // Modify by Nishant Sheth
                // Desc :: to manage multiple years plan id and to get correct tactic count #1750
                List<Plan_Tactic> planTacticsList = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && innerplanids.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && (!((t.EndDate < StartDate) || (t.StartDate > EndDate)))).Select(tactic => new Plan_Tactic { objPlanTactic = tactic, PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId }).ToList(); // Added By Rahul Shah on 16/09/2015 for PL #1610

                lstTacticIds = planTacticsList.Select(tacticlist => tacticlist.objPlanTactic.PlanTacticId).ToList();
                if (filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0)
                {

                    planTacticsList = planTacticsList.Where(pcptobj => (filterOwner.Contains(pcptobj.objPlanTactic.CreatedBy)) &&
                                             (filterTacticType.Contains(pcptobj.objPlanTactic.TacticType.TacticTypeId)) &&
                                             (filterStatus.Contains(pcptobj.objPlanTactic.Status))).ToList();


                    //// Apply Custom restriction for None type
                    if (planTacticsList.Count() > 0)
                    {

                        if (filteredCustomFields.Count > 0)
                        {
                            lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                            //// get Allowed Entity Ids
                            planTacticsList = planTacticsList.Where(tacticlist => lstTacticIds.Contains(tacticlist.objPlanTactic.PlanTacticId)).ToList();
                        }

                    }
                    lstTacticIds = planTacticsList.Select(tacticlist => tacticlist.objPlanTactic.PlanTacticId).ToList();
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    planTacticsList = planTacticsList.Where(pcptobj => lstAllowedEntityIds.Contains(pcptobj.objPlanTactic.PlanTacticId) || (filterOwner.Contains(pcptobj.objPlanTactic.CreatedBy))).ToList();// Modified By Nishant Sheth
                }
                else
                {


                    planTacticsList = planTacticsList.Where(tactic => (filterOwner.Contains(tactic.objPlanTactic.CreatedBy))).Select(tactic => tactic).ToList();// Modified By Nishant Sheth


                }

                //End



                var impprogramlist = db.Plan_Improvement_Campaign_Program.Where(imp => innerplanids.Contains(imp.Plan_Improvement_Campaign.ImprovePlanId)).Select(imp => imp.ImprovementPlanProgramId).ToList();
                var improvementTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(imp => impprogramlist.Contains(imp.ImprovementPlanProgramId) && imp.IsDeleted == false).ToList();


                var LineItemList = (from li in db.Plan_Campaign_Program_Tactic_LineItem
                                    where !li.IsDeleted && planIds.Contains(li.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                                    select new
                                    {
                                        PlanTacticId = li.PlanTacticId,
                                        Cost = li.Cost
                                    }).ToList();
                Double MQLs = 0;
                List<Plan_Campaign_Program_Tactic> planTacticIds = new List<Plan_Campaign_Program_Tactic>();
                List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

                //Added By Bhavesh For Performance Issue #Home
                List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
                List<StageList> stageListType = Common.GetStageList();
                var ModelList = db.Models.Where(m => m.IsDeleted == false && m.ClientId == Sessions.User.ClientId).Select(m => new { ModelId = m.ModelId, ParentModelId = m.ParentModelId, EffectiveDate = m.EffectiveDate }).ToList();

                var improvementTacticTypeIds = improvementTacticList.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
                List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
                string size = Enums.StageType.Size.ToString();
                List<ModelDateList> modelDateList = new List<ModelDateList>();
                int? ModelId;
                int MainModelId = 0;
                List<ModelStageRelationList> modleStageRelationList = new List<ModelStageRelationList>();
                List<Plan_Improvement_Campaign_Program_Tactic> impList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                foreach (var plan in planList)
                {
                    //HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();

                    planTacticIds = planTacticsList.Where(t => t.PlanId == plan.PlanId).Select(tactic => tactic.objPlanTactic).ToList();

                    ModelId = plan.ModelId;
                    modelDateList = new List<ModelDateList>();

                    MainModelId = (int)ModelId;
                    while (ModelId != null)
                    {
                        var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                        modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                        ModelId = model.ParentModelId;
                    }
                    modleStageRelationList = new List<ModelStageRelationList>();
                    modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());
                    impList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                    impList = improvementTacticList.Where(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == plan.PlanId).ToList();

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
            newHomePlanModelHeader.Budget = objCurrency.GetValueByExchangeRate(TotalBudget, PlanExchangeRate); // To set multi-currency
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

        // Add By Nishant Sheth
        // Desc :: for performance with cache data
        public static HomePlanModelHeader GetPlanHeaderValueForMultiplePlansPer(List<int> planIds, string activeMenu, string year, string CustomFieldId, string OwnerIds, string TacticTypeids, string StatusIds)
        {
            HomePlanModelHeader newHomePlanModelHeader = new HomePlanModelHeader();
            CacheObject dataCache = new CacheObject();
            StoredProcedure sp = new StoredProcedure();

            MRPEntities db = new MRPEntities();
            //List<string> tacticStatus = GetStatusListAfterApproved();// Commented By Rahul Shah on 16/09/2015 for PL #1610
            int Year;
            // Add By Nishant sheth
            DateTime StartDate;
            DateTime EndDate;
            string[] ListYear = year.Split('-');
            // Modified By Nishant Sheth
            // #1825 stuck of loading overlay with 'this month' and 'this quarter'
            string planYear = string.Empty;

            bool isNumeric = int.TryParse(year, out Year);

            if (isNumeric)
            {
                planYear = Convert.ToString(year);
            }
            else
            {
                // Add By Nishant Sheth
                // Desc :: To Resolved gantt chart year issue
                if (int.TryParse(ListYear[0], out Year))
                {
                    planYear = ListYear[0];
                }
                else
                {
                    planYear = DateTime.Now.Year.ToString();
                }
                // End By Nishant Sheth
            }


            StartDate = EndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, year, ref StartDate, ref EndDate);
            //End
            double TotalMQLs = 0, TotalBudget = 0;
            double? TotalPercentageMQLImproved = 0;
            int TotalTacticCount = 0;
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)dataCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            // Modify by Nishant sheth
            // Desc :: to get correct count for tactic and for multiple years #1750                      

            var planList = dataCache.Returncache(Enums.CacheObject.Plan.ToString()) as List<Plan>;
            var planData = planList.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == year).Select(a => a.PlanId).ToList();
            planIds = planList.Select(a => a.PlanId).ToList();

            var campplist = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).Where(campaign => (!((campaign.EndDate < StartDate) || (campaign.StartDate > EndDate))) && planIds.Contains(campaign.PlanId)).ToList();
            //var campplist = ((List<Plan_Campaign>)dataCache.Returncache(Enums.CacheObject.Campaign.ToString())).Where(camp => (!((camp.EndDate < StartDate) || (camp.StartDate > EndDate))) && planIds.Contains(camp.PlanId)).Select(a => a).ToList();

            //campplist = campplist.Where(camp => (!((camp.EndDate < StartDate) || (camp.StartDate > EndDate))) && planIds.Contains(camp.PlanId)).Select(a => new { PlanCampaignId = a.PlanCampaignId, PlanId = a.PlanId }).ToList();
            //campplist = campplist.Where(camp => (!((camp.EndDate < StartDate) || (camp.StartDate > EndDate))) && planIds.Contains(camp.PlanId)).Select(a => a).ToList();
            var campplanid = campplist.Select(a => a.PlanId).ToList();
            var campid = campplist.Select(a => a.PlanCampaignId).ToList();
            // Desc :: To resolve the select and deselct all owner issues
            List<Guid> filterOwner = new List<Guid>();
            string PlanLabel = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = db.Plan_UserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();
            var SetOfPlanSelected = Common.PlanUserSavedViews.Where(view => view.FilterName != PlanLabel && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View).ToList();// Add By Nishant Sheth #1915
            /*Commented By Komal Rawal on 25/2/2016 to get data for all owners*/
            //string planselectedowner = SetOfPlanSelected.Where(view => view.FilterName == Enums.FilterLabel.Owner.ToString()).Select(view => view.FilterValues).FirstOrDefault();
            filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //if (planselectedowner == null)
            //{
            //    filterOwner = Sessions.User.UserId.ToString().Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //}
            // End By Nishant Sheth

            // End by Nishant Sheth
            //var planList = db.Plans.Where(p => planIds.Contains(p.PlanId) && p.IsDeleted == false && p.IsActive == true && p.Year == year).Select(m => m).ToList();

            if (planList != null && planList.Count > 0)
            {
                // Modify by Nishant Sheth
                // Desc :: to manage multiple years plan id #1750
                var innerplanids = planList.Where(a => campplanid.Count > 0 ? campplanid.Contains(a.PlanId) : planIds.Contains(a.PlanId)).Select(plan => plan.PlanId).ToList();
                //Modified By Komal Rawal for #1447
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<int> lstTacticIds = new List<int>();


                //// Owner filter criteria.
                //filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

                //TacticType filter criteria
                List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeids) ? new List<int>() : TacticTypeids.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

                //Status filter criteria
                List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

                //// Custom Field Filter Criteria.
                List<string> filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldId) ? new List<string>() : CustomFieldId.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
                if (filteredCustomFields.Count > 0)
                {
                    filteredCustomFields.ForEach(customField =>
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    });

                }


                //List<Plan_Tactic> planTacticsList = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && tacticStatus.Contains(t.Status) && innerplanids.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => new Plan_Tactic { objPlanTactic = tactic, PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId }).ToList();// Commented By Rahul Shah on 16/09/2015 for PL #1610
                // Modify by Nishant Sheth
                // Desc :: to manage multiple years plan id and to get correct tactic count #1750
                //List<Custom_Plan_Campaign_Program_Tactic> planTacticsList = ((List<Custom_Plan_Campaign_Program_Tactic>)dataCache.Returncache(Enums.CacheObject.Tactic.ToString())).Where(t => t.IsDeleted == false && innerplanids.Contains(t.PlanId) && (!((t.EndDate < StartDate) || (t.StartDate > EndDate)))).Select(tactic => tactic).ToList(); // Added By Rahul Shah on 16/09/2015 for PL #1610
                List<Custom_Plan_Campaign_Program_Tactic> customtacticList = (List<Custom_Plan_Campaign_Program_Tactic>)dataCache.Returncache(Enums.CacheObject.CustomTactic.ToString());
                var planTacticsList = ((List<Custom_Plan_Campaign_Program_Tactic>)dataCache.Returncache(Enums.CacheObject.CustomTactic.ToString())).Where(t => t.IsDeleted == false && innerplanids.Contains(t.PlanId) && (!((t.EndDate < StartDate) || (t.StartDate > EndDate)))).ToList();

                lstTacticIds = planTacticsList.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                if (filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0)
                {

                    planTacticsList = planTacticsList.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                             (filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                             (filterStatus.Contains(pcptobj.Status))).ToList();


                    //// Apply Custom restriction for None type
                    if (planTacticsList.Count() > 0)
                    {

                        if (filteredCustomFields.Count > 0)
                        {
                            lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                            //// get Allowed Entity Ids
                            planTacticsList = planTacticsList.Where(tacticlist => lstTacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                        }

                    }
                    lstTacticIds = planTacticsList.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    planTacticsList = planTacticsList.Where(pcptobj => lstAllowedEntityIds.Contains(pcptobj.PlanTacticId) || (filterOwner.Contains(pcptobj.CreatedBy))).ToList();// Modified By Nishant Sheth
                }
                else
                {
                    planTacticsList = planTacticsList.Where(tactic => (filterOwner.Contains(tactic.CreatedBy))).Select(tactic => tactic).ToList();// Modified By Nishant Sheth
                }

                //End

                var impprogramlist = db.Plan_Improvement_Campaign_Program.Where(imp => innerplanids.Contains(imp.Plan_Improvement_Campaign.ImprovePlanId)).Select(imp => imp.ImprovementPlanProgramId).ToList();
                var improvementTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(imp => impprogramlist.Contains(imp.ImprovementPlanProgramId) && imp.IsDeleted == false).ToList();
                //var improvementTacticList = ((List<Plan_Improvement_Campaign_Program_Tactic>)dataCache.Returncache(Enums.CacheObject.ImprovementTactic.ToString())).Where(imp => imp.IsDeleted == false).ToList();

                // Add By Nishant Sheth
                // Desc :: To get performance. 
                //var LineItemList = sp.GetLineItemList(string.Join(",", planIds)).AsEnumerable().Select(row => new
                //{
                //    PlanTacticId = int.Parse(row["PlanTacticId"].ToString()),
                //    Cost = double.Parse(row["Cost"].ToString())
                //}).ToList();
                var LineItemList = sp.GetLineItemList(string.Join(",", planIds));


                //var LineItemList1 = (from li in db.Plan_Campaign_Program_Tactic_LineItem
                //                    where !li.IsDeleted && planIds.Contains(li.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                //                    select new
                //                    {
                //                        PlanTacticId = li.PlanTacticId,
                //                        Cost = li.Cost
                //                    }).ToList();


                Double MQLs = 0;
                List<Custom_Plan_Campaign_Program_Tactic> planTacticIds = new List<Custom_Plan_Campaign_Program_Tactic>();
                List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

                //Added By Bhavesh For Performance Issue #Home
                List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
                List<StageList> stageListType = Common.GetStageList();
                var ModelList = db.Models.Where(m => m.IsDeleted == false && m.ClientId == Sessions.User.ClientId).Select(m => new { ModelId = m.ModelId, ParentModelId = m.ParentModelId, EffectiveDate = m.EffectiveDate }).ToList();

                var improvementTacticTypeIds = improvementTacticList.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
                List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
                string size = Enums.StageType.Size.ToString();
                List<ModelDateList> modelDateList = new List<ModelDateList>();
                int? ModelId;
                int MainModelId = 0;
                List<ModelStageRelationList> modleStageRelationList = new List<ModelStageRelationList>();
                List<Plan_Improvement_Campaign_Program_Tactic> impList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                foreach (var plan in planList)
                {
                    //HomePlanModelHeader objHomePlanModelHeader = new HomePlanModelHeader();

                    planTacticIds = planTacticsList.Where(t => t.PlanId == plan.PlanId).Select(tactic => tactic).ToList();

                    ModelId = plan.ModelId;
                    modelDateList = new List<ModelDateList>();

                    MainModelId = (int)ModelId;
                    while (ModelId != null)
                    {
                        var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                        modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                        ModelId = model.ParentModelId;
                    }
                    modleStageRelationList = new List<ModelStageRelationList>();
                    modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());
                    impList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                    impList = improvementTacticList.Where(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == plan.PlanId).ToList();

                    MQLs = Common.GetTacticStageRelationForSinglePlanPerformance(planTacticIds, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, impList, modelDateList, MainModelId, stageList, false).Sum(t => t.MQLValue);

                    if (planTacticIds.Count() > 0)
                    {
                        var tacticIds = planTacticIds.Select(t => t.PlanTacticId).ToList();
                        TotalBudget += LineItemList.Where(l => tacticIds.Contains(l.PlanTacticId)).Sum(l => l.Cost);
                    }


                    if (impList.Count > 0)
                    {
                        //// Getting improved MQL.
                        double? improvedMQL = Common.GetTacticStageRelationForSinglePlanPerformance(planTacticIds, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, impList, modelDateList, MainModelId, stageList, true).Sum(t => t.MQLValue);

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
            newHomePlanModelHeader.Budget = objCurrency.GetValueByExchangeRate(TotalBudget, PlanExchangeRate);// Modified By Nishant Sheth #2497
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



        #region get all Collaborators of tactic
        /// <summary>
        /// Added By: KOmal Rawal.
        /// Action to get all collaborators of tactic.
        /// </summary>
        /// <param name="PlanId">Plan Id</param>
        public static List<string> GetAllCollaborators(List<int> tacticIds)
        {
            MRPEntities db = new MRPEntities();
            List<string> collaboratorId = new List<string>();
            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId));
            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();
            var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => tacticIds.Contains(pc.Plan_Campaign_Program_Tactic.PlanTacticId));
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();
            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
        }

        #endregion

        #region DefaultRedirecr

        public static MVCUrl DefaultRedirectURL(Enums.ActiveMenu from)
        {
            if (Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.MRP.ToString()).Any())
            {
                MRPEntities db = new MRPEntities();

                try
                {
                    string modelPublished = Enums.ModelStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
                    string modelDraft = Enums.ModelStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
                    string planPublished = Enums.PlanStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                    string planDraft = Enums.PlanStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;

                    var models = db.Models.Where(m => m.ClientId == Sessions.User.ClientId && m.IsDeleted == false).Select(m => m);

                    var allModelIds = models.Select(m => m.ModelId).ToList();
                    if (allModelIds == null || allModelIds.Count == 0 && from == Enums.ActiveMenu.None)
                    {
                        return new MVCUrl { actionName = "HomeZero", controllerName = "Home", queryString = "" };
                    }
                    else
                    {
                        if (allModelIds == null || allModelIds.Count == 0 && from == Enums.ActiveMenu.Home)
                        {
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

                        var tblPlan = db.Plans.Where(p => (publishedModelIds.Contains(p.Model.ModelId) || draftModelIds.Contains(p.Model.ModelId)) && p.IsDeleted.Equals(false));
                        var draftPlan = tblPlan.Where(p => p.Status.Equals(planDraft));
                        var publishedPlan = tblPlan.Where(p => p.Status.Equals(planPublished));

                        if (publishedPlan != null && publishedPlan.Any())
                        {
                            return new MVCUrl { actionName = "Index", controllerName = "Home", queryString = "Home" };
                        }
                        else if (draftPlan != null && draftPlan.Any())
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
                    db = null;
                }
                return null;
            }
            else if (Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Any())
            {
                return new MVCUrl { actionName = "MeasureReport", controllerName = "Report", queryString = "" };
            }
            else if (Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.OPT.ToString()).Any())
            { return null; }
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

        /// <summary>
        /// Add By Nishant Sheth
        /// Fuction to get list of default dimension
        /// </summary>
        /// <returns>returns list of default dimensions</returns>
        public static List<string> GetDefaultDimensionList()
        {
            List<string> Dimension = new List<string>();
            Dimension.Add(Enums.DimensionValues[Enums.Dimension.ClientId.ToString()].ToString());
            Dimension.Add(Enums.DimensionValues[Enums.Dimension.CreatedBy.ToString()].ToString());
            Dimension.Add(Enums.DimensionValues[Enums.Dimension.PlanId.ToString()].ToString());
            Dimension.Add(Enums.DimensionValues[Enums.Dimension.StartDate.ToString()].ToString());
            Dimension.Add(Enums.DimensionValues[Enums.Dimension.TacticTypeId.ToString()].ToString());
            return Dimension;
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
            catch (Exception)
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
                return Convert.ToDateTime(objDate).ToString("MMM dd, yyyy") + " at " + Convert.ToDateTime(objDate).ToString("hh:mm tt");
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
        public static bool DeleteIntegrationInstance(int integrationInstanceId, bool deleteIntegrationInstanceModel = false, int modelId = 0, bool iseloquainstance = false, bool isMarketoinstance = false)
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
                            if (iseloquainstance)
                            {
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceEloquaId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                            else if (isMarketoinstance) //Added by Komal Rawal for PL#2190
                            {
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceMarketoID = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                            else
                            {
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }

                            var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId &&
                                a.IntegrationInstanceProgramId != null).ToList();
                            Plan_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_CampaignList = db.Plan_Campaign.Where(a => a.IsDeleted.Equals(false) && a.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceCampaignId != null).ToList();
                            Plan_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });


                            var Plan_Improvement_Campaign_Program_TacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                a.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceTacticId != null).ToList();
                            if (iseloquainstance)
                            {
                                Plan_Improvement_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceEloquaId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                            else
                            {
                                Plan_Improvement_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }

                            var Plan_Improvement_Campaign_ProgramList = db.Plan_Improvement_Campaign_Program.Where(a => a.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId == integrationInstanceId &&
                                a.IntegrationInstanceProgramId != null).ToList();
                            Plan_Improvement_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; });

                            var Plan_Improvement_CampaignList = db.Plan_Improvement_Campaign.Where(a => a.Plan.Model.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceCampaignId != null).ToList();
                            Plan_Improvement_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; });

                            if (deleteIntegrationInstanceModel == true)
                            {
                                List<Model> ModelsList = db.Models.Where(a => a.IsDeleted.Equals(false)).ToList();
                                if (iseloquainstance)
                                {
                                    // Get Eloqua related Models and set null value for that records.
                                    List<Model> elqModelsList = ModelsList.Where(a => a.IntegrationInstanceEloquaId == integrationInstanceId && a.IntegrationInstanceEloquaId != null).ToList();
                                    elqModelsList.ForEach(a => { a.IntegrationInstanceEloquaId = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                                }
                                else if (isMarketoinstance) //Added by Komal Rawal for PL#2190
                                {
                                    // Get Marketo related Models and set null value for that records.
                                    List<Model> MarketoModelList = ModelsList.Where(a => a.IntegrationInstanceMarketoID == integrationInstanceId && a.IntegrationInstanceMarketoID != null).ToList();
                                    MarketoModelList.ForEach(a => { a.IntegrationInstanceMarketoID = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                }
                                else
                                {
                                    // Get Salesforce related Models and set null value for that records.
                                    List<Model> salModelsList = ModelsList.Where(a => a.IntegrationInstanceId == integrationInstanceId && a.IntegrationInstanceId != null).ToList();
                                    salModelsList.ForEach(a => { a.IntegrationInstanceId = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                                }
                            }
                        }
                        else
                        {
                            var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                                                                                              a.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId == modelId
                                                                                                        ).ToList();
                            if (iseloquainstance)
                            {
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceEloquaId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                            else if (isMarketoinstance) //Added by Komal Rawal for PL#2190
                            {
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceMarketoID = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                            else
                            {
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }

                            var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) &&
                                                                                                a.Plan_Campaign.Plan.ModelId == modelId
                                                                                          ).ToList();
                            Plan_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                            var Plan_CampaignList = db.Plan_Campaign.Where(a => a.IsDeleted.Equals(false) && a.Plan.ModelId == modelId).ToList();
                            Plan_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });


                            var Plan_Improvement_Campaign_Program_TacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) &&
                                                                                                                                      a.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.ModelId == modelId
                                                                                                                                ).ToList();
                            if (iseloquainstance)
                            {
                                Plan_Improvement_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceEloquaId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }
                            else
                            {
                                Plan_Improvement_Campaign_Program_TacticList.ForEach(a => { a.IntegrationInstanceTacticId = null; a.LastSyncDate = null; a.ModifiedDate = DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                            }

                            var Plan_Improvement_Campaign_ProgramList = db.Plan_Improvement_Campaign_Program.Where(a => a.Plan_Improvement_Campaign.Plan.ModelId == modelId
                                                                                                                  ).ToList();
                            Plan_Improvement_Campaign_ProgramList.ForEach(a => { a.IntegrationInstanceProgramId = null; a.LastSyncDate = null; });

                            var Plan_Improvement_CampaignList = db.Plan_Improvement_Campaign.Where(a => a.Plan.ModelId == modelId).ToList();
                            Plan_Improvement_CampaignList.ForEach(a => { a.IntegrationInstanceCampaignId = null; a.LastSyncDate = null; });
                        }

                        if (isMarketoinstance && modelId > 0)
                        {
                            #region "Declare local variables"
                            string EntityType = Enums.FilterLabel.TacticType.ToString();
                            List<int> tacTypeIds = new List<int>();
                            List<MarketoEntityValueMapping> DeleteTacticTypeMapping = new List<MarketoEntityValueMapping>();
                            #endregion

                            // Get Tactic Type Ids list based on Model Id.
                            tacTypeIds = db.TacticTypes.Where(typ => typ.ModelId == modelId && typ.IsDeleted.Value == false).Select(typ => typ.TacticTypeId).ToList();
                            if (tacTypeIds != null && tacTypeIds.Count > 0)
                                DeleteTacticTypeMapping = db.MarketoEntityValueMappings.Where(Entity => Entity.IntegrationInstanceId == integrationInstanceId && Entity.EntityType == EntityType && Entity.EntityID.HasValue && tacTypeIds.Contains(Entity.EntityID.Value)).ToList();

                            // Delete TacticType records from MarketoEntityValueMappings table related to current model.
                            if (DeleteTacticTypeMapping != null)
                            {
                                DeleteTacticTypeMapping.ForEach(DeleteMapping => db.Entry(DeleteMapping).State = EntityState.Deleted);
                            }
                        }

                        db.SaveChanges();
                        scope.Complete();

                        return true;
                    }
                }
            }
            catch (Exception)
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
        public static void ChangeProgramStatus(int PlanProgramId, bool AddComment)
        {
            using (MRPEntities db = new MRPEntities())
            {
                var objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == PlanProgramId && pcp.IsDeleted.Equals(false)).FirstOrDefault();
                Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
                if (objPlan_Campaign_Program != null)
                {
                    string newProgramStatus = Common.GetProgramStatus(objPlan_Campaign_Program);
                    if (newProgramStatus != string.Empty)
                    {
                        objPlan_Campaign_Program.Status = newProgramStatus;
                        objPlan_Campaign_Program.ModifiedDate = DateTime.Now;
                        objPlan_Campaign_Program.ModifiedBy = Sessions.User.UserId;
                        db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                        if (AddComment)  //Added By Komal Rawal for #1357 - To Add Comment
                        {
                            string approvedComment = Convert.ToString(Enums.Section.Program) + " " + newProgramStatus + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                            pcptc.Comment = approvedComment;
                            DateTime Currentdate = DateTime.Now;
                            pcptc.CreatedDate = Currentdate;
                            string DisplayDate = Currentdate.ToString("MMM dd") + " at " + Currentdate.ToString("hh:mmtt");
                            pcptc.CreatedBy = Sessions.User.UserId;
                            pcptc.PlanProgramId = PlanProgramId;
                            db.Entry(pcptc).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                        }
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
        public static void ChangeCampaignStatus(int PlanCampaignId, bool AddComment)
        {
            using (MRPEntities db = new MRPEntities())
            {
                var objPlan_Campaign = db.Plan_Campaign.Where(pcp => pcp.PlanCampaignId == PlanCampaignId && pcp.IsDeleted.Equals(false)).FirstOrDefault();
                Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
                if (objPlan_Campaign != null)
                {
                    string newCampaignStatus = Common.GetCampaignStatus(objPlan_Campaign);
                    if (newCampaignStatus != string.Empty)
                    {
                        objPlan_Campaign.Status = newCampaignStatus;
                        objPlan_Campaign.ModifiedDate = DateTime.Now;
                        objPlan_Campaign.ModifiedBy = Sessions.User.UserId;
                        db.Entry(objPlan_Campaign).State = EntityState.Modified;
                        if (AddComment)  //Added By Komal Rawal for #1357 - To Add Comment
                        {
                            string approvedComment = Convert.ToString(Enums.Section.Campaign) + " " + newCampaignStatus + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                            pcptc.Comment = approvedComment;
                            DateTime Currentdate = DateTime.Now;
                            pcptc.CreatedDate = Currentdate;
                            string DisplayDate = Currentdate.ToString("MMM dd") + " at " + Currentdate.ToString("hh:mmtt");
                            pcptc.CreatedBy = Sessions.User.UserId;
                            pcptc.PlanCampaignId = PlanCampaignId;
                            db.Entry(pcptc).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                        }
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
                string statusCreated = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Created.ToString()].ToString();
                try
                {
                    using (MRPEntities db = new MRPEntities())
                    {
                        var lstTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == objPlan_Campaign_Program.PlanProgramId && pcpt.IsDeleted == false).ToList();

                        if (lstTactic != null)
                        {
                            if (lstTactic.Count > 0)
                            {
                                int cntAllCreateTacticStatus = lstTactic.Where(pcpt => !pcpt.Status.Equals(statusCreated)).Count();
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
                                else if (cntAllCreateTacticStatus == 0)
                                {
                                    status = statusCreated;
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
                string statusCreated = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Created.ToString()].ToString();
                try
                {
                    using (MRPEntities db = new MRPEntities())
                    {
                        var lstProgram = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted.Equals(false)).ToList();

                        if (lstProgram != null && lstProgram.Count > 0)
                        {
                            List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted == false).ToList();
                            // Number of program with status is not 'Created' 
                            int cntCreatedProgramStatus = lstProgram.Where(pcpt => !pcpt.Status.Equals(statusCreated)).Count();
                            // Number of tactic with status is not 'Created'
                            int cntCreatedTacticStatus = tblTactic.Where(pcpt => !pcpt.Status.Equals(statusCreated)).Count();
                            // Number of program with status is not 'Submitted' 
                            int cntSumbitProgramStatus = lstProgram.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                            // Number of tactic with status is not 'Submitted'
                            int cntSumbitTacticStatus = tblTactic.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();

                            // Number of program with status is not 'Approved', 'in-progress', 'complete'
                            int cntApproveProgramStatus = lstProgram.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                            // Number of tactic with status is not 'Approved', 'in-progress', 'complete'
                            int cntApproveTacticStatus = tblTactic.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();

                            // Number of program with status is not 'Declained'
                            int cntDeclineProgramStatus = lstProgram.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();
                            // Number of tactic with status is not 'Declained'
                            int cntDeclineTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == objPlan_Campaign.PlanCampaignId && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();

                            List<string> lstPStatus = new List<string>();
                            string tmpStatus;
                            foreach (var p in lstProgram)
                            {
                                tmpStatus = GetProgramStatus(p);
                                if (!string.IsNullOrEmpty(tmpStatus))
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
                            else if (cntCreatedProgramStatus == 0 && cntCreatedTacticStatus == 0)
                            {

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
                double cost = 0;
                PlanExchangeRate = Sessions.PlanExchangeRate;
                cost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId == planCampaignId && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                return objCurrency.GetValueByExchangeRate(cost, PlanExchangeRate); //Modified by Rahul Shah for PL #2511 to apply multi currency
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
                PlanExchangeRate = Sessions.PlanExchangeRate;
                var lstTactic = db.Plan_Campaign_Program.FirstOrDefault(varC => varC.PlanProgramId == planProgramId)
                                                 .Plan_Campaign_Program_Tactic
                                                 .Where(varP => varP.IsDeleted == false)
                                                 .ToList();

                double cost = 0;
                /*Modified by Mitesh Vaishnav on 29/07/2014 for PL ticket #619*/
                lstTactic.ForEach(varT => cost = cost + varT.Plan_Campaign_Program_Tactic_LineItem.Where(varL => varL.IsDeleted == false).Sum(varL => varL.Cost));

                return objCurrency.GetValueByExchangeRate(cost, PlanExchangeRate); //Modified by Rahul Shah for PL #2511 to apply multi currency

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
            int levelMQL = db.Stages.FirstOrDefault(s => s.ClientId.Equals(Sessions.User.ClientId) && s.IsDeleted == false && s.Code.Equals(stageMQL)).Level.Value;

            if (tacticStageLevel < levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString());
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString());
            }
            else if (tacticStageLevel == levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString());
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

        #region TacticStageTitleforActualTab
        /// <summary>
        /// Function to get dynamic stage titles.
        /// added by Rahul Shah Ticket #2111
        /// </summary>
        /// <param name="tacticId"></param>
        /// <param name="tStagelevel"></param>
        /// <param name="levelMQL"></param>
        /// <returns></returns>
        public static List<string> GetTacticStageTitleActual(int tacticId, int tStagelevel, int levelMQL)
        {
            List<string> lstStageTitle = new List<string>();
            int tacticStageLevel = Convert.ToInt32(tStagelevel);
            if (tacticStageLevel < levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString());
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString());
            }
            else if (tacticStageLevel == levelMQL)
            {
                lstStageTitle.Add(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString());
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

        #region "PlanLevel"
        public static string BelowPlan = "Below Plan";
        public static string AbovePlan = "Above Plan";
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
        public static List<TacticStageValue> GetTacticStageRelation(List<Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true, bool IsReport = false)
        {
            MRPEntities objDbMRPEntities = new MRPEntities();
            //// Compute the tactic relation list
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculation(tlist, isIncludeImprovement);
            // Add By Nishant Sheth
            // Desc :: To get performance regarding #2111 add stagelist into cache memory
            CacheObject dataCache = new CacheObject();
            List<Stage> stageList = dataCache.Returncache(Enums.CacheObject.StageList.ToString()) as List<Stage>;
            if (stageList == null)
            {
                stageList = objDbMRPEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
            }
            dataCache.AddCache(Enums.CacheObject.StageList.ToString(), stageList);
            //// Fetch the tactic stages and it's value
            //// Return finalized TacticStageValue list to the Parent method 
            return GetTacticStageValueList(tlist, tacticValueRelationList, stageList, false, IsReport);
        }
        public static List<TacticStageValue> GetTacticStageRelationPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true, bool IsReport = false)
        {
            MRPEntities objDbMRPEntities = new MRPEntities();
            //// Compute the tactic relation list
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculationPerformance(tlist, isIncludeImprovement);
            List<Stage> stageList = objDbMRPEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
            //// Fetch the tactic stages and it's value
            //// Return finalized TacticStageValue list to the Parent method 
            return GetTacticStageValueListPerformance(tlist, tacticValueRelationList, stageList, false, IsReport);
        }

        /// <summary>
        /// Get values of tactic stages
        /// Created By : Bhavesh Dobariya
        /// Calculate MQL,CW,Revenue etc. for each tactic of single plan.
        /// </summary>
        /// <param name="tlist">List collection of tactics</param>
        /// <param name="isIncludeImprovement">boolean flag that indicate tactic included imporvement sections</param>
        /// <returns></returns>
        public static List<TacticStageValue> GetTacticStageRelationForSinglePlan(List<Plan_Campaign_Program_Tactic> tlist, List<StageRelation> bestInClassStageRelation, List<StageList> stageListType, List<ModelStageRelationList> modleStageRelationList, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ModelDateList> modelDateList, int ModelId, List<Stage> stageList, bool isIncludeImprovement = true, bool IsReport = false)
        {
            //Compute the tactic relation list
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculationForSinglePlan(tlist, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, ModelId, isIncludeImprovement);
            //fetch the tactic stages and it's value
            //Return finalized TacticStageValue list to the Parent method 
            return GetTacticStageValueList(tlist, tacticValueRelationList, stageList, true, IsReport);
        }

        public static List<TacticStageValue> GetTacticStageRelationForSinglePlanPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist, List<StageRelation> bestInClassStageRelation, List<StageList> stageListType, List<ModelStageRelationList> modleStageRelationList, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ModelDateList> modelDateList, int ModelId, List<Stage> stageList, bool isIncludeImprovement = true, bool IsReport = false)
        {
            //Compute the tactic relation list
            List<TacticStageValueRelation> tacticValueRelationList = GetCalculationForSinglePlanPerformance(tlist, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, ModelId, isIncludeImprovement);
            //fetch the tactic stages and it's value
            //Return finalized TacticStageValue list to the Parent method 
            return GetTacticStageValueListPerformance(tlist, tacticValueRelationList, stageList, true, IsReport);
        }

        public static List<TacticStageValue> GetTacticStageValueList(List<Plan_Campaign_Program_Tactic> tlist, List<TacticStageValueRelation> tacticValueRelationList, List<Stage> stageList, bool isSinglePlan = false, bool IsReport = false)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
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
            List<int> cwVelocityStagelist = stageList.Where(s => s.Level >= levelINQ && s.Level <= levelCW).Select(s => s.StageId).ToList(); //Modified By komal Rawal for #2124 caculate cw from INQ

            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            string Size = Enums.StageType.Size.ToString();
            List<Plan_Campaign_Program_Tactic_Actual> actualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            List<int> TacticIds = new List<int>();
            List<CustomField_Entity> tblCustomFieldEntities = new List<CustomField_Entity>();
            TacticIds = tlist.Select(t => t.PlanTacticId).ToList();
            if (!isSinglePlan)
            {

                actualTacticList = (from t in TacticIds
                                    join ta in dbStage.Plan_Campaign_Program_Tactic_Actual on t equals ta.PlanTacticId
                                    select ta).ToList();
            }

            if (IsReport)
            {
                string EntTacticType = Enums.EntityType.Tactic.ToString();

                var customfiedlids = dbStage.CustomFields.Where(c => c.ClientId == Sessions.User.ClientId && c.EntityType == EntTacticType && c.IsDeleted == false).Select(c => c.CustomFieldId).ToList();

                tblCustomFieldEntities = dbStage.CustomField_Entity.Where(CustEnt => TacticIds.Contains(CustEnt.EntityId) && customfiedlids.Contains(CustEnt.CustomFieldId)).Select(c => c).ToList();
            }
            List<StageRelation> stageRelation;
            TacticStageValue tacticStageValueObj;
            //Ittrate the Plan_Campaign_Program_Tactic list and Assign it to TacticStageValue 
            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                stageRelation = new List<StageRelation>();
                stageRelation = tacticValueRelationList.Where(t => t.TacticObj.PlanTacticId == tactic.PlanTacticId).Select(t => t.StageValueList).FirstOrDefault();
                int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                //int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                inqStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelINQ).Select(s => s.StageId).ToList();
                mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                revenueStagelist = stageList.Where(s => (s.Level >= projectedStageLevel && s.Level <= levelCW) || s.Level == null).Select(s => s.StageId).ToList();

                tacticStageValueObj = new TacticStageValue();
                tacticStageValueObj.TacticObj = tactic;
                tacticStageValueObj.INQValue = projectedStageLevel <= levelINQ ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => inqStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.MQLValue = projectedStageLevel <= levelMQL ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => mqlStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.CWValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => cwStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.RevenueValue = objCurrency.GetValueByExchangeRate((projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => revenueStagelist.Contains(sr.StageId) && (sr.StageType == CR || sr.StageType == Size)).Aggregate(1.0, (x, y) => x * y.Value)) : 0), PlanExchangeRate); // Modified By Nishant Sheth //#2508 Convert value as per reporting exchange rate
                tacticStageValueObj.INQVelocity = stageRelation.Where(sr => inqVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.MQLVelocity = stageRelation.Where(sr => mqlVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.CWVelocity = stageRelation.Where(sr => cwVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.ADSValue = stageRelation.Where(sr => sr.StageType == Size).Sum(sr => sr.Value);

                if (!isSinglePlan)
                {
                    tacticStageValueObj.ActualTacticList = actualTacticList.Where(a => a.PlanTacticId == tactic.PlanTacticId).ToList();
                }

                //// If Page request called from Report page then set Stage weightages.
                if (IsReport)
                {
                    tacticStageValueObj.TacticYear = tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;

                    #region "Get Tactic Stage-Weightage"
                    tacticStageValueObj.TacticStageWeightages = tblCustomFieldEntities.Where(CustEnt => CustEnt.EntityId == tactic.PlanTacticId).Select(_customfield =>
                                                                                                  new TacticCustomFieldStageWeightage()
                                                                                                  {
                                                                                                      CustomFieldId = _customfield.CustomFieldId,
                                                                                                      Value = _customfield.Value,
                                                                                                      CostWeightage = _customfield.CostWeightage != null ? _customfield.CostWeightage.Value : 0,
                                                                                                      CVRWeightage = _customfield.Weightage != null ? _customfield.Weightage.Value : 0
                                                                                                  }).ToList();
                    #endregion
                }
                tacticStageList.Add(tacticStageValueObj);
            }
            //Return finalized TacticStageValue list to the Parent method 
            return tacticStageList;
        }
        public static List<TacticStageValue> GetTacticStageValueListPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist, List<TacticStageValueRelation> tacticValueRelationList, List<Stage> stageList, bool isSinglePlan = false, bool IsReport = false)
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
            List<int> TacticIds = new List<int>();
            List<CustomField_Entity> tblCustomFieldEntities = new List<CustomField_Entity>();
            TacticIds = tlist.Select(t => t.PlanTacticId).ToList();
            if (!isSinglePlan)
            {

                actualTacticList = (from t in TacticIds
                                    join ta in dbStage.Plan_Campaign_Program_Tactic_Actual on t equals ta.PlanTacticId
                                    select ta).ToList();
            }

            if (IsReport)
            {
                string EntTacticType = Enums.EntityType.Tactic.ToString();

                var customfiedlids = dbStage.CustomFields.Where(c => c.ClientId == Sessions.User.ClientId && c.EntityType == EntTacticType && c.IsDeleted == false).Select(c => c.CustomFieldId).ToList();

                tblCustomFieldEntities = dbStage.CustomField_Entity.Where(CustEnt => TacticIds.Contains(CustEnt.EntityId) && customfiedlids.Contains(CustEnt.CustomFieldId)).Select(c => c).ToList();
            }
            List<StageRelation> stageRelation;
            TacticStageValue tacticStageValueObj;
            //Ittrate the Plan_Campaign_Program_Tactic list and Assign it to TacticStageValue 
            foreach (Custom_Plan_Campaign_Program_Tactic tactic in tlist)
            {
                stageRelation = new List<StageRelation>();
                stageRelation = tacticValueRelationList.Where(t => t.TacticObj.PlanTacticId == tactic.PlanTacticId).Select(t => t.StageValueList).FirstOrDefault();
                int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                //int projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                inqStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelINQ).Select(s => s.StageId).ToList();
                mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                revenueStagelist = stageList.Where(s => (s.Level >= projectedStageLevel && s.Level <= levelCW) || s.Level == null).Select(s => s.StageId).ToList();

                tacticStageValueObj = new TacticStageValue();
                tacticStageValueObj.TacticObj = GetTacticFromCustomTacticModel(tactic);
                tacticStageValueObj.INQValue = projectedStageLevel <= levelINQ ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => inqStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.MQLValue = projectedStageLevel <= levelMQL ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => mqlStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.CWValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => cwStagelist.Contains(sr.StageId) && sr.StageType == CR).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.RevenueValue = projectedStageLevel < levelCW ? Convert.ToDouble(tactic.ProjectedStageValue) * (stageRelation.Where(sr => revenueStagelist.Contains(sr.StageId) && (sr.StageType == CR || sr.StageType == Size)).Aggregate(1.0, (x, y) => x * y.Value)) : 0;
                tacticStageValueObj.INQVelocity = stageRelation.Where(sr => inqVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.MQLVelocity = stageRelation.Where(sr => mqlVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.CWVelocity = stageRelation.Where(sr => cwVelocityStagelist.Contains(sr.StageId) && sr.StageType == SV).Sum(sr => sr.Value);
                tacticStageValueObj.ADSValue = stageRelation.Where(sr => sr.StageType == Size).Sum(sr => sr.Value);

                if (!isSinglePlan)
                {
                    tacticStageValueObj.ActualTacticList = actualTacticList.Where(a => a.PlanTacticId == tactic.PlanTacticId).ToList();
                }

                //// If Page request called from Report page then set Stage weightages.
                if (IsReport)
                {
                    tacticStageValueObj.TacticYear = tactic.PlanYear;

                    #region "Get Tactic Stage-Weightage"
                    tacticStageValueObj.TacticStageWeightages = tblCustomFieldEntities.Where(CustEnt => CustEnt.EntityId == tactic.PlanTacticId).Select(_customfield =>
                                                                                                  new TacticCustomFieldStageWeightage()
                                                                                                  {
                                                                                                      CustomFieldId = _customfield.CustomFieldId,
                                                                                                      Value = _customfield.Value,
                                                                                                      CostWeightage = _customfield.CostWeightage != null ? _customfield.CostWeightage.Value : 0,
                                                                                                      CVRWeightage = _customfield.Weightage != null ? _customfield.Weightage.Value : 0
                                                                                                  }).ToList();
                    #endregion
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

            #region "Declare Local Variables"
            int modelId = 0;
            List<StageRelation> stageModelRelation;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementList;
            TacticStageValueRelation tacticStageObj;
            List<StageRelation> stageRelationList;
            List<int> improvementTypeList;
            List<ImprovementTacticType_Metric> improvementIdsWeighList;
            StageRelation stageRelationObj;
            List<ImprovementTacticType_Metric> stageimplist;
            double impcount = 0, impWeight = 0, improvementValue = 0;
            #endregion

            //foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            //{
            //    modelId = GetModelIdFromList(modelDateList, tactic.StartDate, ModelId);
            //    stageModelRelation = new List<StageRelation>();
            //    stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
            //    improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
            //    improvementList = improvementActivities.Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
            //    if (improvementList.Count() > 0 && isIncludeImprovement)
            //    {
            //        tacticStageObj = new TacticStageValueRelation();
            //        tacticStageObj.TacticObj = tactic;
            //        improvementTypeList = new List<int>();
            //        improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
            //        improvementIdsWeighList = new List<ImprovementTacticType_Metric>();
            //        improvementIdsWeighList = improvementTacticTypeMetric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId)).Select(imptype => imptype).ToList();
            //        stageRelationList = new List<StageRelation>();
            //        foreach (StageList stage in stageList)
            //        {
            //            stageRelationObj = new StageRelation();
            //            stageRelationObj.StageId = stage.StageId;
            //            stageRelationObj.StageType = stage.StageType;
            //            stageimplist = new List<ImprovementTacticType_Metric>();
            //            stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Weight > 0).ToList();
            //            impcount = stageimplist.Count();
            //            impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
            //            improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Where(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
            //            stageRelationObj.Value = improvementValue;
            //            stageRelationList.Add(stageRelationObj);
            //        }

            //        tacticStageObj.StageValueList = stageRelationList;
            //        TacticSatgeValueList.Add(tacticStageObj);
            //    }
            //    else
            //    {
            //        tacticStageObj = new TacticStageValueRelation();
            //        tacticStageObj.TacticObj = tactic;
            //        tacticStageObj.StageValueList = stageModelRelation;
            //        TacticSatgeValueList.Add(tacticStageObj);
            //    }
            //}

            for (int i = 0; i < tlist.Count; i++)
            {
                modelId = GetModelIdFromList(modelDateList, tlist[i].StartDate, ModelId);
                stageModelRelation = new List<StageRelation>();
                stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
                improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementList = improvementActivities.Where(it => it.EffectiveDate <= tlist[i].StartDate).ToList();
                if (improvementList.Count() > 0 && isIncludeImprovement)
                {
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tlist[i];
                    improvementTypeList = new List<int>();
                    improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    improvementIdsWeighList = new List<ImprovementTacticType_Metric>();
                    improvementIdsWeighList = improvementTacticTypeMetric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId)).Select(imptype => imptype).ToList();
                    stageRelationList = new List<StageRelation>();
                    for (int j = 0; j < stageList.Count; j++)
                    {
                        stageRelationObj = new StageRelation();
                        stageRelationObj.StageId = stageList[j].StageId;
                        stageRelationObj.StageType = stageList[j].StageType;
                        stageimplist = new List<ImprovementTacticType_Metric>();
                        stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stageList[j].StageId && impweight.StageType == stageList[j].StageType && impweight.Weight > 0).ToList();
                        impcount = stageimplist.Count();
                        impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
                        improvementValue = GetImprovement(stageList[j].StageType, bestInClassStageRelation.Where(b => b.StageId == stageList[j].StageId && b.StageType == stageList[j].StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stageList[j].StageId && s.StageType == stageList[j].StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
                        stageRelationObj.Value = improvementValue;
                        stageRelationList.Add(stageRelationObj);
                    }

                    tacticStageObj.StageValueList = stageRelationList;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
                else
                {
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tlist[i];
                    tacticStageObj.StageValueList = stageModelRelation;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
            }
            return TacticSatgeValueList;
        }
        public static List<TacticStageValueRelation> GetCalculationForSinglePlanPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist, List<StageRelation> bestInClassStageRelation, List<StageList> stageList, List<ModelStageRelationList> modleStageRelationList, List<ImprovementTacticType_Metric> improvementTacticTypeMetric, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, List<ModelDateList> modelDateList, int ModelId, bool isIncludeImprovement = true)
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

            #region "Declare Local Variables"
            int modelId = 0;
            List<StageRelation> stageModelRelation;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementList;
            TacticStageValueRelation tacticStageObj;
            List<StageRelation> stageRelationList;
            List<int> improvementTypeList;
            List<ImprovementTacticType_Metric> improvementIdsWeighList;
            StageRelation stageRelationObj;
            List<ImprovementTacticType_Metric> stageimplist;
            double impcount = 0, impWeight = 0, improvementValue = 0;
            #endregion

            //foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            //{
            //    modelId = GetModelIdFromList(modelDateList, tactic.StartDate, ModelId);
            //    stageModelRelation = new List<StageRelation>();
            //    stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
            //    improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
            //    improvementList = improvementActivities.Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
            //    if (improvementList.Count() > 0 && isIncludeImprovement)
            //    {
            //        tacticStageObj = new TacticStageValueRelation();
            //        tacticStageObj.TacticObj = tactic;
            //        improvementTypeList = new List<int>();
            //        improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
            //        improvementIdsWeighList = new List<ImprovementTacticType_Metric>();
            //        improvementIdsWeighList = improvementTacticTypeMetric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId)).Select(imptype => imptype).ToList();
            //        stageRelationList = new List<StageRelation>();
            //        foreach (StageList stage in stageList)
            //        {
            //            stageRelationObj = new StageRelation();
            //            stageRelationObj.StageId = stage.StageId;
            //            stageRelationObj.StageType = stage.StageType;
            //            stageimplist = new List<ImprovementTacticType_Metric>();
            //            stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Weight > 0).ToList();
            //            impcount = stageimplist.Count();
            //            impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
            //            improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Where(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
            //            stageRelationObj.Value = improvementValue;
            //            stageRelationList.Add(stageRelationObj);
            //        }

            //        tacticStageObj.StageValueList = stageRelationList;
            //        TacticSatgeValueList.Add(tacticStageObj);
            //    }
            //    else
            //    {
            //        tacticStageObj = new TacticStageValueRelation();
            //        tacticStageObj.TacticObj = tactic;
            //        tacticStageObj.StageValueList = stageModelRelation;
            //        TacticSatgeValueList.Add(tacticStageObj);
            //    }
            //}

            for (int i = 0; i < tlist.Count; i++)
            {
                modelId = GetModelIdFromList(modelDateList, tlist[i].StartDate, ModelId);
                stageModelRelation = new List<StageRelation>();
                stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
                improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementList = improvementActivities.Where(it => it.EffectiveDate <= tlist[i].StartDate).ToList();
                if (improvementList.Count() > 0 && isIncludeImprovement)
                {
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = GetTacticFromCustomTacticModel(tlist[i]);
                    improvementTypeList = new List<int>();
                    improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    improvementIdsWeighList = new List<ImprovementTacticType_Metric>();
                    improvementIdsWeighList = improvementTacticTypeMetric.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTacticTypeId)).Select(imptype => imptype).ToList();
                    stageRelationList = new List<StageRelation>();
                    for (int j = 0; j < stageList.Count; j++)
                    {
                        stageRelationObj = new StageRelation();
                        stageRelationObj.StageId = stageList[j].StageId;
                        stageRelationObj.StageType = stageList[j].StageType;
                        stageimplist = new List<ImprovementTacticType_Metric>();
                        stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stageList[j].StageId && impweight.StageType == stageList[j].StageType && impweight.Weight > 0).ToList();
                        impcount = stageimplist.Count();
                        impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
                        improvementValue = GetImprovement(stageList[j].StageType, bestInClassStageRelation.Where(b => b.StageId == stageList[j].StageId && b.StageType == stageList[j].StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stageList[j].StageId && s.StageType == stageList[j].StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
                        stageRelationObj.Value = improvementValue;
                        stageRelationList.Add(stageRelationObj);
                    }

                    tacticStageObj.StageValueList = stageRelationList;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
                else
                {
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = GetTacticFromCustomTacticModel(tlist[i]);
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

            #region "Declare Local Variables"
            int planId = 0, modelId = 0;
            List<StageRelation> stageModelRelation;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementList;
            TacticStageValueRelation tacticStageObj;
            List<int> improvementTypeList;
            List<ImprovementTypeWeightList> improvementIdsWeighList;
            List<StageRelation> stageRelationList;
            StageRelation stageRelationObj;
            #endregion

            foreach (Plan_Campaign_Program_Tactic tactic in tlist)
            {
                planId = tacticPlanList.Where(t => t.PlanTacticId == tactic.PlanTacticId).Select(t => t.PlanId).FirstOrDefault();
                modelId = tacticModelList.Where(t => t.PlanTacticId == tactic.PlanTacticId).Select(t => t.ModelId).FirstOrDefault();
                stageModelRelation = new List<StageRelation>();
                stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
                improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementList = (planIMPTacticList.Where(p => p.PlanId == planId).Select(p => p.ImprovementTacticList).FirstOrDefault()).Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
                if (improvementList.Count() > 0 && isIncludeImprovement)
                {
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tactic;

                    improvementTypeList = new List<int>();
                    improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    improvementIdsWeighList = new List<ImprovementTypeWeightList>();
                    improvementIdsWeighList = improvementTypeWeightList.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTypeId) && imptype.isDeploy).Select(imptype => imptype).ToList();
                    stageRelationList = new List<StageRelation>();
                    foreach (StageList stage in stageList)
                    {
                        stageRelationObj = new StageRelation();
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
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = tactic;
                    tacticStageObj.StageValueList = stageModelRelation;
                    TacticSatgeValueList.Add(tacticStageObj);
                }
            }
            return TacticSatgeValueList;
        }
        public static List<TacticStageValueRelation> GetCalculationPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist, bool isIncludeImprovement = true)
        {
            List<TacticPlanRelation> tacticPlanList = GetTacticPlanRelationListPerformance(tlist);
            List<TacticModelRelation> tacticModelList = GetTacticModelRelationListPerformance(tlist, tacticPlanList);
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

            #region "Declare Local Variables"
            int planId = 0, modelId = 0;
            List<StageRelation> stageModelRelation;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementList;
            TacticStageValueRelation tacticStageObj;
            List<int> improvementTypeList;
            List<ImprovementTypeWeightList> improvementIdsWeighList;
            List<StageRelation> stageRelationList;
            StageRelation stageRelationObj;
            #endregion

            foreach (Custom_Plan_Campaign_Program_Tactic tactic in tlist)
            {
                planId = tacticPlanList.Where(t => t.PlanTacticId == tactic.PlanTacticId).Select(t => t.PlanId).FirstOrDefault();
                modelId = tacticModelList.Where(t => t.PlanTacticId == tactic.PlanTacticId).Select(t => t.ModelId).FirstOrDefault();
                stageModelRelation = new List<StageRelation>();
                stageModelRelation = modleStageRelationList.Where(m => m.ModelId == modelId).Select(m => m.StageList).FirstOrDefault();
                improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementList = (planIMPTacticList.Where(p => p.PlanId == planId).Select(p => p.ImprovementTacticList).FirstOrDefault()).Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
                if (improvementList.Count() > 0 && isIncludeImprovement)
                {
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = GetTacticFromCustomTacticModel(tactic);

                    improvementTypeList = new List<int>();
                    improvementTypeList = improvementList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    improvementIdsWeighList = new List<ImprovementTypeWeightList>();
                    improvementIdsWeighList = improvementTypeWeightList.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTypeId) && imptype.isDeploy).Select(imptype => imptype).ToList();
                    stageRelationList = new List<StageRelation>();
                    foreach (StageList stage in stageList)
                    {
                        stageRelationObj = new StageRelation();
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
                    tacticStageObj = new TacticStageValueRelation();
                    tacticStageObj.TacticObj = GetTacticFromCustomTacticModel(tactic);
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
        public static List<TacticPlanRelation> GetTacticPlanRelationListPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist)
        {
            List<TacticPlanRelation> tacticPlanlist = (from t in tlist
                                                       select new TacticPlanRelation
                                                       {
                                                           PlanTacticId = t.PlanTacticId,
                                                           PlanId = Convert.ToInt32(t.PlanId.ToString())
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
            PlanIMPTacticListRelation planTacticIMP;

            var implist = dbStage.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => planIds.Contains(imptactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && !imptactic.IsDeleted).ToList();

            foreach (int planId in planIds)
            {
                planTacticIMP = new PlanIMPTacticListRelation();
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
            #region "Declare Local Variables"
            MRPEntities dbStage = new MRPEntities();
            string size = Enums.StageType.Size.ToString();
            string CR = Enums.StageType.CR.ToString();
            string ADS = Enums.Stage.ADS.ToString();
            List<ModelStageRelationList> modleStageRelationist = new List<ModelStageRelationList>();
            double ads = 0;
            ModelStageRelationList modelStageObj;
            List<Model> tblModels = new List<Model>();
            List<Stage> stagelist = new List<Stage>();
            #endregion

            stagelist = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false && stage.Code.Equals(ADS)).ToList();
            int adsStageId = stagelist.Select(stage => stage.StageId).FirstOrDefault();
            var ModelFunnelList = dbStage.Model_Stage.Where(m => modleIds.Contains(m.ModelId)).ToList();

            tblModels = dbStage.Models.Where(m => modleIds.Contains(m.ModelId)).ToList();
            foreach (int modelId in modleIds)
            {
                modelStageObj = new ModelStageRelationList();
                modelStageObj.ModelId = modelId;
                modelStageObj.StageList = ModelFunnelList.Where(m => m.ModelId == modelId).Select(m => new StageRelation
                {
                    StageId = m.StageId,
                    StageType = m.StageType,
                    Value = m.StageType == CR ? m.Value / 100 : m.Value
                }
                    ).ToList();

                ads = tblModels.Where(m => m.ModelId == modelId).Select(m => m.AverageDealSize).FirstOrDefault();
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
            bool isDeployed = false;
            List<ImprovementTypeWeightList> improvementTypeWeightList = new List<ImprovementTypeWeightList>();
            List<ImprovementTacticType_Metric> innerList;
            var improvementTacticTypeList = dbStage.ImprovementTacticTypes.Where(imp => improvementTacticTypeIds.Contains(imp.ImprovementTacticTypeId)).ToList();
            var improvementTacticTypeWeightList = dbStage.ImprovementTacticType_Metric.Where(imp => improvementTacticTypeIds.Contains(imp.ImprovementTacticTypeId)).ToList();
            foreach (int improvementTacticTypeId in improvementTacticTypeIds)
            {
                innerList = new List<ImprovementTacticType_Metric>();
                isDeployed = improvementTacticTypeList.Where(imp => imp.ImprovementTacticTypeId == improvementTacticTypeId).Select(imp => imp.IsDeployed).FirstOrDefault();
                innerList = improvementTacticTypeWeightList.Where(imp => imp.ImprovementTacticTypeId == improvementTacticTypeId).Select(imp => imp).ToList();
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
            List<Stage> fltrStageList = new List<Stage>();
            var Stage = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).ToList();
            string CW = Enums.Stage.CW.ToString();
            string CR = Enums.StageType.CR.ToString();
            fltrStageList = Stage.Where(st => st.Level != null && st.Code != CW).ToList();
            StageList sl;
            foreach (Stage s in fltrStageList)
            {
                sl = new StageList();
                sl.StageId = s.StageId;
                sl.StageType = CR;
                sl.Level = s.Level;
                stageList.Add(sl);
            }
            string SV = Enums.StageType.SV.ToString();
            foreach (Stage s in fltrStageList)
            {
                sl = new StageList();
                sl.StageId = s.StageId;
                sl.StageType = SV;
                sl.Level = s.Level;
                stageList.Add(sl);
            }
            fltrStageList = new List<Stage>();
            fltrStageList = Stage.Where(st => st.Level == null).ToList();
            string Size = Enums.StageType.Size.ToString();
            foreach (Stage s in fltrStageList)
            {
                sl = new StageList();
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

            string stageCR = Enums.MetricType.CR.ToString(), stageSV = Enums.MetricType.SV.ToString(), stageSize = Enums.MetricType.Size.ToString();
            //// Calculate boostGap
            if (stageType == stageCR)
            {
                boostGap = bestInClassValue - modelvalue;
            }
            else if (stageType == stageSV)
            {
                boostGap = modelvalue - bestInClassValue;
            }
            else if (stageType == stageSize)
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
            if (stageType == stageCR)
            {
                improvementValue = modelvalue + improvement;
            }
            else if (stageType == stageSV)
            {
                improvementValue = modelvalue - improvement;
            }
            else if (stageType == stageSize)
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
            List<PlanADSRelation> planADSList = new List<PlanADSRelation>();
            List<int> planIds = planIMPList.Select(p => p.PlanId).ToList();
            List<PlanModelRelation> planModelRelation = dbStage.Plans.Where(p => planIds.Contains(p.PlanId)).Select(p => new PlanModelRelation { PlanId = p.PlanId, ModelId = p.ModelId }).ToList();
            List<int> modelIds = planModelRelation.Select(mdl => mdl.ModelId).ToList();
            List<Model> tblModel = new List<Model>();
            bool isImprovementExits = false;
            int modelId = 0;
            double ADSValue = 0, impcount = 0, impWeight = 0;
            tblModel = dbStage.Models.Where(mf => modelIds.Contains(mf.ModelId)).ToList();
            List<int> improvementTypeList;
            List<ImprovementTypeWeightList> improvementIdsWeighList;
            List<ImprovementTypeWeightList> stageimplist;
            foreach (PlanIMPTacticListRelation planIMP in planIMPList)
            {
                isImprovementExits = false;
                modelId = planModelRelation.Where(p => p.PlanId == planIMP.PlanId).Select(p => p.ModelId).FirstOrDefault();
                if (planIMP.ImprovementTacticList.Count > 0)
                {
                    //// Get Model id based on effective date From.
                    modelId = GetModelId(planIMP.ImprovementTacticList.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), modelId);
                }
                ADSValue = tblModel.Where(mf => mf.ModelId == modelId).Select(mf => mf.AverageDealSize).FirstOrDefault();
                if (planIMP.ImprovementTacticList.Count > 0)
                {
                    improvementTypeList = new List<int>();
                    improvementTypeList = planIMP.ImprovementTacticList.Select(imptactic => imptactic.ImprovementTacticTypeId).ToList();
                    improvementIdsWeighList = new List<ImprovementTypeWeightList>();
                    improvementIdsWeighList = improvementTypeWeightList.Where(imptype => improvementTypeList.Contains(imptype.ImprovementTypeId) && imptype.isDeploy).ToList();
                    stageimplist = new List<ImprovementTypeWeightList>();
                    stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == ADSStageId && impweight.StageType == Size && impweight.Value > 0).ToList();
                    impcount = stageimplist.Count();
                    impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Value);
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
            List<Model> ModelList = db.Models.Where(m => m.IsDeleted == false).ToList();
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
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
            Model _mdlData = new Model();
            _mdlData = mdbt.Models.Where(m => m.ModelId == ModelId).FirstOrDefault();
            DateTime? effectiveDate = _mdlData.EffectiveDate;
            if (effectiveDate == null || StartDate >= effectiveDate)
                return ModelId;

            int? ParentModelId = _mdlData.ParentModelId;
            if (ParentModelId != null)
            {
                return GetModelId(StartDate, (int)ParentModelId);
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
            var ModelList = dbStage.Models.Where(m => m.ClientId == Sessions.User.ClientId && m.IsDeleted == false).Select(m => new { m.ModelId, m.ParentModelId, m.EffectiveDate }).ToList();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            foreach (var modelid in modelids)
            {
                int? ModelId = (int)modelid;
                while (ModelId != null)
                {
                    var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                    if (model != null) // Add Condtion for failure test case by nishant Sheth
                    {
                        modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                        ModelId = model.ParentModelId;
                    }
                    ModelId = (int?)null; // Add By nishant Sheth due to failure test case
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
        public static List<TacticModelRelation> GetTacticModelRelationListPerformance(List<Custom_Plan_Campaign_Program_Tactic> tlist, List<TacticPlanRelation> tacticPlanList)
        {
            MRPEntities dbStage = new MRPEntities();
            var planids = tacticPlanList.Select(t => t.PlanId).Distinct().ToList();
            var modelids = dbStage.Plans.Where(p => planids.Contains(p.PlanId)).Select(p => p.ModelId).Distinct();
            var ModelList = dbStage.Models.Where(m => m.ClientId == Sessions.User.ClientId && m.IsDeleted == false).Select(m => new { m.ModelId, m.ParentModelId, m.EffectiveDate }).ToList();
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
                                                             ModelId = GetModelIdFromList(modelDateList, t.StartDate, t.ModelId)
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

            List<Plan_Improvement_Campaign_Program_Tactic> improvementList;
            List<StageRelation> stageRelation;
            int projectedStageLevel = 0;
            TacticStageValue tacticStageValueObj;
            foreach (Plan_Campaign_Program_Tactic tactic in marketingActivities)
            {
                improvementList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementList = improvementActivities.Where(it => it.EffectiveDate <= tactic.StartDate).ToList();
                stageRelation = new List<StageRelation>();
                stageRelation = CalculateStageValueForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, ModelId, modleStageRelationList, improvementList, improvementTacticTypeMetric, true);
                projectedStageLevel = stageList.Where(s => s.StageId == tactic.StageId).Select(s => s.Level.Value).FirstOrDefault();
                inqStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelINQ).Select(s => s.StageId).ToList();
                mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                cwStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level <= levelCW).Select(s => s.StageId).ToList();
                revenueStagelist = stageList.Where(s => (s.Level >= projectedStageLevel && s.Level <= levelCW) || s.Level == null).Select(s => s.StageId).ToList();

                tacticStageValueObj = new TacticStageValue();
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
                StageRelation stageRelationObj;
                List<ImprovementTacticType_Metric> stageimplist;
                double impcount = 0, impWeight = 0, improvementValue = 0;
                foreach (StageList stage in stageList)
                {
                    stageRelationObj = new StageRelation();
                    stageimplist = new List<ImprovementTacticType_Metric>();
                    stageimplist = improvementIdsWeighList.Where(impweight => impweight.StageId == stage.StageId && impweight.StageType == stage.StageType && impweight.Weight > 0).ToList();
                    impcount = stageimplist.Count();
                    impWeight = impcount <= 0 ? 0 : stageimplist.Sum(s => s.Weight);
                    improvementValue = GetImprovement(stage.StageType, bestInClassStageRelation.Where(b => b.StageId == stage.StageId && b.StageType == stage.StageType).Select(b => b.Value).FirstOrDefault(), stageModelRelation.Where(s => s.StageId == stage.StageId && s.StageType == stage.StageType).Select(s => s.Value).FirstOrDefault(), impcount, impWeight);
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
            DateTime? effectiveDate = modelDateList.Where(m => m.ModelId == ModelId).Select(m => m.EffectiveDate).FirstOrDefault();
            if (effectiveDate != null)
            {
                if (StartDate >= effectiveDate)
                {
                    return ModelId;
                }
                else
                {
                    int? ParentModelId = modelDateList.Where(m => m.ModelId == ModelId).Select(m => m.ParentModelId).FirstOrDefault();
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

        public static bool CheckTacticIsAllowedAndIsEditable(int planTacticId)
        {
            bool returnValue = true;
            List<int> lstEditableTacticIds = new List<int>();
            lstEditableTacticIds = Common.GetViewEditEntityList(Sessions.User.UserId, new List<int>() { planTacticId });

            if (lstEditableTacticIds.Count > 0 && !lstEditableTacticIds.Contains(planTacticId))
            {
                returnValue = false;
            }

            return returnValue;
        }

        public static bool CheckTacticIsAllowedAndIsNotRestricted(int planTacticId)
        {
            bool returnValue = true;
            List<int> lstRestrictedTacticIds = new List<int>();
            bool isCustomRestrictionSet = true;
            lstRestrictedTacticIds = Common.GetRestrictedEntityList(Sessions.User.UserId, out isCustomRestrictionSet);

            if (!isCustomRestrictionSet || (lstRestrictedTacticIds.Count > 0 && !lstRestrictedTacticIds.Contains(planTacticId)))
            {
                returnValue = false;
            }

            return returnValue;
        }

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
            string revGoalType = Enums.PlanGoalTypeList[Enums.PlanGoalType.Revenue.ToString()].ToString();
            objStage.Title = revGoalType.ToLower();
            objStage.Code = revGoalType.ToUpper();
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
        public static BudgetAllocationModel CalculateBudgetInputs(int modelId, string goalType, string goalValue, double averageDealSize, bool IsCw = false)
        {
            BudgetAllocationModel objBudgetAllocationModel = new BudgetAllocationModel();
            try
            {
                MRPEntities dbStage = new MRPEntities();
                PlanExchangeRate = Sessions.PlanExchangeRate;
                List<Stage> stageList = dbStage.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                string stageINQ = Enums.Stage.INQ.ToString();
                int levelINQ = stageList.FirstOrDefault(s => s.Code.Equals(stageINQ)).Level.Value;
                string stageMQL = Enums.Stage.MQL.ToString();
                int levelMQL = stageList.FirstOrDefault(s => s.Code.Equals(stageMQL)).Level.Value;
                string stageCW = Enums.Stage.CW.ToString();
                int levelCW = stageList.FirstOrDefault(s => s.Code.Equals(stageCW)).Level.Value;

                List<int> mqlStagelist = new List<int>();
                List<int> cwStagelist = new List<int>();

                string CR = Enums.StageType.CR.ToString();
                var ModelFunnelStageList = dbStage.Model_Stage.Where(mfs => mfs.ModelId == modelId && mfs.StageType == CR).ToList();

                if (goalType != "" && goalValue != "" && goalValue != "0")
                {
                    double inputValue = Convert.ToInt64(goalValue.Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, "")); //Modified by Rahul Shah for PL #2499

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

                        // Calculate CW   -- Added by devanshi gandhi for pl #1430
                        if (IsCw == true)
                        {
                            double CWValue = (inputValue) * (modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                            CWValue = (CWValue.Equals(double.NaN) || CWValue.Equals(double.NegativeInfinity) || CWValue.Equals(double.PositiveInfinity)) ? 0 : CWValue;
                            objBudgetAllocationModel.CWValue = CWValue;
                        }
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

                        // Calculate CW   -- Added by devanshi gandhi for pl #1430
                        if (IsCw == true)
                        {
                            double CWValue = (inputValue) * (modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100)));
                            CWValue = (CWValue.Equals(double.NaN) || CWValue.Equals(double.NegativeInfinity) || CWValue.Equals(double.PositiveInfinity)) ? 0 : CWValue;
                            objBudgetAllocationModel.CWValue = CWValue;
                        }
                    }
                    else if (goalType == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                    {
                        // Calculate INQ
                        double avdDealSize = objCurrency.GetValueByExchangeRate(averageDealSize, PlanExchangeRate);
                        cwStagelist = stageList.Where(s => s.Level >= levelINQ && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListCW = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double convalue = ((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * avdDealSize);
                        double INQValue = (inputValue) / convalue;
                        INQValue = (INQValue.Equals(double.NaN) || INQValue.Equals(double.NegativeInfinity) || INQValue.Equals(double.PositiveInfinity)) ? 0 : INQValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.INQValue = INQValue;

                        // Calculate MQL
                        cwStagelist = stageList.Where(s => s.Level >= levelMQL && s.Level <= levelCW).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = ModelFunnelStageList.Where(mfs => cwStagelist.Contains(mfs.StageId)).ToList();
                        double MQLValue = (inputValue) / (modelFunnelStageListMQL.Aggregate(1.0, (x, y) => x * (y.Value / 100)) * avdDealSize); // Modified by Sohel Pathan on 12/09/2014 for PL ticket #775
                        MQLValue = (MQLValue.Equals(double.NaN) || MQLValue.Equals(double.NegativeInfinity) || MQLValue.Equals(double.PositiveInfinity)) ? 0 : MQLValue;  // Added by Sohel Pathan on 12/12/2014 for PL ticket #975
                        objBudgetAllocationModel.MQLValue = MQLValue;

                        // Calculate CW   -- Added by devanshi gandhi for pl #1430
                        if (IsCw == true)
                        {
                            double CWValue = (inputValue) / avdDealSize;
                            CWValue = (CWValue.Equals(double.NaN) || CWValue.Equals(double.NegativeInfinity) || CWValue.Equals(double.PositiveInfinity)) ? 0 : CWValue;
                            objBudgetAllocationModel.CWValue = CWValue;
                        }
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
        public static double CalculateMQLOnly(int modelId, string goalType, string goalValue, double averageDealSize, List<Stage> stageList, List<Model_Stage> modelFunnelStageList)
        {
            try
            {
                string stageINQ = Enums.Stage.INQ.ToString();
                int levelINQ = stageList.FirstOrDefault(stage => stage.Code.Equals(stageINQ)).Level.Value;
                string stageMQL = Enums.Stage.MQL.ToString();
                int levelMQL = stageList.FirstOrDefault(stage => stage.Code.Equals(stageMQL)).Level.Value;
                string stageCW = Enums.Stage.CW.ToString();
                int levelCW = stageList.FirstOrDefault(stage => stage.Code.Equals(stageCW)).Level.Value;

                List<int> mqlStagelist = new List<int>();
                List<int> cwStagelist = new List<int>();

                if (goalType != "" && goalValue != "" && goalValue != "0")
                {
                    string CR = Enums.StageType.CR.ToString();
                    double inputValue = Convert.ToInt64(goalValue.Trim().Replace(",", "").Replace("$", ""));
                    if (goalType.Equals(Enums.PlanGoalType.INQ.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //// Calculate MQL
                        int projectedStageLevel = levelINQ;
                        mqlStagelist = stageList.Where(s => s.Level >= projectedStageLevel && s.Level < levelMQL).Select(s => s.StageId).ToList();
                        var modelFunnelStageListMQL = modelFunnelStageList.Where(mfs => mfs.ModelId == modelId && mqlStagelist.Contains(mfs.StageId)).ToList();
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
                        var modelFunnelStageListCW = modelFunnelStageList.Where(mfs => mfs.ModelId == modelId && cwStagelist.Contains(mfs.StageId)).ToList();
                        double INQValue = (inputValue) / ((modelFunnelStageListCW.Aggregate(1.0, (x, y) => x * (y.Value / 100))) * averageDealSize);
                        INQValue = (INQValue.Equals(double.NaN) || INQValue.Equals(double.NegativeInfinity) || INQValue.Equals(double.PositiveInfinity)) ? 0 : INQValue;    // Added by Sohel Pathan on 12/12/2014 for PL ticket #975

                        // Calculate MQL
                        var modelFunnelStageListMQL = modelFunnelStageList.Where(mfs => mfs.ModelId == modelId && mqlStagelist.Contains(mfs.StageId)).ToList();
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
        public static List<CustomFieldModel> GetCustomFields(int id, string section, string Status)
        {
            MRPEntities db = new MRPEntities();
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            List<CustomFieldDependency> DependencyList = db.CustomFieldDependencies.Where(a => a.IsDeleted == false).Select(a => a).ToList();
            //Modified By Komal Rawal for #1864
            var lstCustomFields = db.CustomFields.Where(customField => customField.EntityType.ToLower() == section && customField.ClientId == Sessions.User.ClientId && customField.IsDeleted == false && (customField.CustomFieldType.Name.Equals(DropDownList) ? customField.CustomFieldOptions.Count() > 0 : true)).ToList().Select(a => new CustomFieldModel
            {
                customFieldId = a.CustomFieldId,
                name = a.Name,
                customFieldType = a.CustomFieldType.Name,
                description = a.Description,
                isRequired = a.IsRequired,
                entityType = a.EntityType,
                isChild = DependencyList.Select(list => list.ChildCustomFieldId).ToList().Contains(a.CustomFieldId) ? true : false,
                ParentId = DependencyList.Where(b => b.ChildCustomFieldId == a.CustomFieldId).Select(b => b.ParentCustomFieldId).FirstOrDefault() == null ? 0 : DependencyList.Where(b => b.ChildCustomFieldId == a.CustomFieldId).Select(b => b.ParentCustomFieldId).FirstOrDefault(),
                ParentOptionId = DependencyList.Where(list => list.ChildCustomFieldId == a.CustomFieldId && list.ChildOptionId == null).Select(list => list.ParentOptionId).ToList(),

                //Modified By Komal Rawal for #1292 dont apply isdeleted flag for tactics that are completed.
                option = GetCustomFieldOptions(Status, a.CustomFieldOptions).Select(o => new CustomFieldOptionModel
                {
                    ChildOptionId = DependencyList.Select(list => list.ChildOptionId).ToList().Contains(o.CustomFieldOptionId) ? true : false,
                    ParentOptionId = DependencyList.Where(b => b.ChildOptionId == o.CustomFieldOptionId).Select(b => b.ParentOptionId).ToList(),
                    customFieldOptionId = o.CustomFieldOptionId,
                    ChildOptionIds = DependencyList.Where(Child => Child.ParentOptionId == o.CustomFieldOptionId).Select(list => list.ChildOptionId).ToList(),
                    value = o.Value,
                    customFieldId = o.CustomFieldId
                }).OrderBy(o => o.value).ToList()

            }).OrderBy(a => a.name, new AlphaNumericComparer()).ToList();
            //End
            List<int> customFieldIds = lstCustomFields.Select(cs => cs.customFieldId).ToList();
            var EntityValue = db.CustomField_Entity.Where(ct => ct.EntityId == id && customFieldIds.Contains(ct.CustomFieldId)).Select(ct => new { ct.Value, ct.CustomFieldId }).ToList();
            foreach (var CustomFieldId in customFieldIds)
            {
                lstCustomFields.Where(c => c.customFieldId == CustomFieldId).FirstOrDefault().value = EntityValue.Where(ev => ev.CustomFieldId == CustomFieldId).Select(ev => ev.Value).ToList();

            }

            //foreach (var item in EntityValue)
            //{
            //    bool IsSelected = false;
            //    if (ListIDs.Where(v => entityvalues.Contains(v)).Any())
            //    {
            //        IsSelected = childid.Contains(item.CustomFieldId) && childOptionid.Contains(item.Value);
            //    }
            //    lstCustomFields.Where(c => c.customFieldId == item.CustomFieldId).FirstOrDefault().IsSelected = IsSelected;

            //}
            List<CustomFieldModel> finalList = new List<CustomFieldModel>();
            foreach (var item in lstCustomFields.Where(cfParent => cfParent.ParentId == 0).ToList())
            {
                finalList.Add(item);
                setCustomFieldHierarchy(item.customFieldId, lstCustomFields, ref finalList);
                //foreach (var childItem in lstCustomFields.Where(cfParent => cfParent.ParentId == item.customFieldId).ToList())
                //{
                //    finalList.Add(childItem);
                //}
            }

            return finalList;
        }

        //Added By Komal Rawal for #1292 dont apply isdeleted flag for tactics that are completed.
        public static List<CustomFieldOption> GetCustomFieldOptions(string Status, ICollection<CustomFieldOption> Customfieldoptionlist)
        {
            //if (Status == Enums.TacticStatus.Complete.ToString())
            //{
            //    return Customfieldoptionlist.ToList();
            //}
            //else
            //{
            return Customfieldoptionlist.Where(a => a.IsDeleted == false).ToList();

            //  }
        }
        //End

        public static void setCustomFieldHierarchy(int parentId, List<CustomFieldModel> lstCustomField, ref List<CustomFieldModel> finalList)
        {
            foreach (var item in lstCustomField.Where(cf => cf.ParentId == parentId).ToList())
            {
                finalList.Add(item);
                var x = lstCustomField.Where(cf => cf.ParentId == item.customFieldId).ToList();
                if (lstCustomField.Where(cf => cf.ParentId == item.customFieldId).Any())
                {
                    setCustomFieldHierarchy(item.customFieldId, lstCustomField, ref finalList);
                }
            }
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
            lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Stage.ToString(), Value = PlanGanttTypes.Stage.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Status.ToString(), Value = PlanGanttTypes.Status.ToString() });
            // lstViewByTab.Add(new ViewByModel { Text = PlanGanttTypes.Request.ToString(), Value = PlanGanttTypes.Request.ToString() });
            //lstViewByTab = lstViewByTab.Where(viewBy => !string.IsNullOrEmpty(viewBy.Text)).OrderBy(viewBy => viewBy.Text, new AlphaNumericComparer()).ToList();

            //// Check that if list of PlanTactic is not null then we are going to fetch the Custom Fields
            if (lstTactic != null && lstTactic.Count > 0)
            {
                var campaignId = lstTactic.Select(tactic => tactic.PlanCampaignId).ToList();
                var programId = lstTactic.Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList();
                var lstcustomfields = GetCustomFields(lstTactic.Select(tactic => tactic.objPlanTactic.PlanTacticId).ToList(), programId, campaignId);

                //// Concat the Default list with newly fetched custom fields. 
                lstViewByTab = lstViewByTab.Concat(lstcustomfields).ToList();
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
                                select cf).Distinct().OrderBy(cf => cf.Name).ToList();

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
        public static List<ViewByModel> GetCustomFields(List<int> tacticids, List<int> programids, List<int> campaignids, bool IsBudgetTab = false)
        {
            MRPEntities db = new MRPEntities();
            StoredProcedure objSp = new StoredProcedure();
            //List<int> LineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(tactic => tacticids.Contains(tactic.PlanTacticId)).Select(lineitem => lineitem.PlanLineItemId).ToList();
            // Add By Nishant Sheth
            // Desc :: get line item records from Stored procedure
            List<int> LineItemIds = objSp.GetTacticLineItemList(string.Join(",", tacticids)).Select(lineitem => lineitem.PlanLineItemId).ToList();

            List<ViewByModel> lstCustomFieldsViewByTab = new List<ViewByModel>();
            string CampaignCustomText = Enums.EntityType.Campaign.ToString().ToLower(),
                ProgramCustomText = Enums.EntityType.Program.ToString().ToLower(),
                TacticCustomText = Enums.EntityType.Tactic.ToString().ToLower(),
               LineItemCustomText = Enums.EntityType.Lineitem.ToString().ToLower();

            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            var customfieldlist = db.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.ClientId
                && customfield.IsDeleted == false
                && customfield.IsDisplayForFilter == true //Modified by Mitesh for PL ticket 1020 (add filter of IsDisplayForFilter)
                && customfield.CustomFieldType.Name == DropDownList).ToList(); //Modified by Arpita Soni for PL ticket 1148 (added filter of CustomFieldTypeId)

            List<int> customfieldids = customfieldlist.Select(cfl => cfl.CustomFieldId).ToList();
            List<int> allentityids = new List<int>();
            if (tacticids.Count > 0) // // To handle object null reference exception  - Dashrath Prajapati - 29/01/2016
            {
                // Check tacticid exists or not then use concat
                allentityids = tacticids.Concat(programids).Concat(campaignids).Concat(LineItemIds).ToList();
            }
            var customfieldentity = db.CustomField_Entity.Where(cfe => customfieldids.Contains(cfe.CustomFieldId)).Select(cfe => new { EntityId = cfe.EntityId, CustomFieldId = cfe.CustomFieldId }).ToList();

            //var fcustomfieldentity = customfieldentity.Where(cf => allentityids.Contains(cf.EntityId)).ToList();

            customfieldentity = (from cf in customfieldentity
                                 join ae in allentityids on cf.EntityId equals ae
                                 select cf).Distinct().ToList();

            var campaigncustomids = customfieldentity.Where(cfe => campaignids.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabCampaign = customfieldlist.Where(cf => cf.EntityType.ToLower() == CampaignCustomText && campaigncustomids.Contains(cf.CustomFieldId))
                .Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = CampaignCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabCampaign = lstCustomFieldsViewByTabCampaign.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            var programcustomids = customfieldentity.Where(cfe => programids.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabProgram = customfieldlist.Where(cf => cf.EntityType.ToLower() == ProgramCustomText && programcustomids.Contains(cf.CustomFieldId))
                .Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = ProgramCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabProgram = lstCustomFieldsViewByTabProgram.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            var tacticcustomids = customfieldentity.Where(cfe => tacticids.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabTactic = customfieldlist.Where(cf => cf.EntityType.ToLower() == TacticCustomText && tacticcustomids.Contains(cf.CustomFieldId))
                .Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = TacticCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabTactic = lstCustomFieldsViewByTabTactic.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            var LineItemcustomids = customfieldentity.Where(cfe => LineItemIds.Contains(cfe.EntityId)).Select(cfe => cfe.CustomFieldId).Distinct().ToList();
            List<ViewByModel> lstCustomFieldsViewByTabLineItem = customfieldlist.Where(cf => cf.EntityType.ToLower() == LineItemCustomText && LineItemcustomids.Contains(cf.CustomFieldId))
                .Select(cf => new ViewByModel { Text = cf.Name.ToString(), Value = LineitemCustomTitle + cf.CustomFieldId.ToString() }).ToList();
            lstCustomFieldsViewByTabLineItem = lstCustomFieldsViewByTabLineItem.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            if (IsBudgetTab)
            {
                lstCustomFieldsViewByTab = lstCustomFieldsViewByTab.Concat(lstCustomFieldsViewByTabLineItem).Concat(lstCustomFieldsViewByTabTactic).Concat(lstCustomFieldsViewByTabProgram).Concat(lstCustomFieldsViewByTabCampaign).ToList();

            }
            else
            {
                lstCustomFieldsViewByTab = lstCustomFieldsViewByTab.Concat(lstCustomFieldsViewByTabTactic).Concat(lstCustomFieldsViewByTabProgram).Concat(lstCustomFieldsViewByTabCampaign).ToList();
            }
            return lstCustomFieldsViewByTab;
        }

        /// <summary>
        /// Get the list of budget
        /// </summary>
        /// <returns>Return the list of Budget list</returns>
        public static List<ViewByModel> GetParentBudgetlist(int BudgetId = 0)
        {
            MRPEntities db = new MRPEntities();

            List<ViewByModel> lstBudget = new List<ViewByModel>();

            var budgeparentids = db.Budgets.Where(m => m.ClientId == Sessions.User.ClientId && (m.IsDeleted == false || m.IsDeleted == null)).Select(m => m.Id).ToList();
            var tblBudgetDetail = db.Budget_Detail.Where(a => a.IsDeleted == false && budgeparentids.Contains(a.BudgetId)).Select(a => a).ToList();
            int? ParentId = 0;
            var checkParent = tblBudgetDetail.Where(a => a.Id == BudgetId && (a.IsDeleted == false)).Select(a => a.ParentId).ToList();
            ParentId = checkParent.Count > 0 ? checkParent[0] : 0;

            var customfieldlist = tblBudgetDetail.Where(a => (ParentId > 0 ? a.ParentId == (ParentId != null ? ParentId : null) : a.ParentId == null) && (a.IsDeleted == false) && !string.IsNullOrEmpty(a.Name)).Select(a => new { a.Id, a.Name }).ToList();

            lstBudget = customfieldlist.Select(budget => new ViewByModel { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();

            return lstBudget;
        }
        /// <summary>
        /// Get the list of budget
        /// </summary>
        /// <returns>Return the list of Budget list</returns>
        public static List<ViewByModel> GetBudgetlist()
        {
            MRPEntities db = new MRPEntities();
            List<ViewByModel> lstBudget = new List<ViewByModel>();
            var customfieldlist = db.Budgets.Where(bdgt => bdgt.ClientId == Sessions.User.ClientId && (bdgt.IsDeleted == false || bdgt.IsDeleted == null) && !string.IsNullOrEmpty(bdgt.Name)).ToList();
            lstBudget = customfieldlist.Select(budget => new ViewByModel { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return lstBudget;
        }
        public static List<ViewByModel> GetChildBudgetlist(int ParentId)
        {
            MRPEntities db = new MRPEntities();
            List<ViewByModel> lstBudget = new List<ViewByModel>();
            //var customfieldlist = (from parent in db.Budgets
            //                       join child in db.Budget_Detail on parent.Id equals child.ParentId
            //                       orderby parent.Name
            //                       select new { child.Name, child.Id }).Distinct().ToList();

            var customfieldlist = db.Budget_Detail.Where(a => a.ParentId == ParentId && (a.IsDeleted == false || a.IsDeleted == null) && !string.IsNullOrEmpty(a.Name)).Select(a => new { a.Id, a.Name }).ToList();
            lstBudget = customfieldlist.Select(budget => new ViewByModel { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return lstBudget;
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
            if (objModel.IntegrationInstanceId == null && objModel.IntegrationInstanceIdCW == null && objModel.IntegrationInstanceIdINQ == null && objModel.IntegrationInstanceIdMQL == null && objModel.IntegrationInstanceIdProjMgmt == null && objModel.IntegrationInstanceEloquaId == null) //Modified Brad Gray 7/23/2015 PL#1448
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
                            //List<CustomField_Entity> tblCustomField = new List<CustomField_Entity>();
                            if (section == Enums.Section.Plan.ToString() && id != 0)
                            {
                                var PlanId = id;
                                string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                                string entityTypeProgram = Enums.EntityType.Program.ToString();
                                string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                                //List<CustomField_Entity> customFieldList = new List<CustomField_Entity>();
                                //    List<Plan_Campaign_Program_Tactic_LineItem> tblLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false)).ToList();
                                var lineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false) && lineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId).ToList();

                                lineItemList.ForEach(lineItem => { lineItem.IsDeleted = true; lineItem.ModifiedDate = System.DateTime.Now; lineItem.ModifiedBy = Sessions.User.UserId; });
                                List<int> lineItemsIds = lineItemList.Select(lineItem => lineItem.PlanLineItemId).ToList();
                                var lineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineItemActual => lineItemsIds.Contains(lineItemActual.PlanLineItemId)).ToList();
                                lineItemActualList.ForEach(lineItemActual => db.Entry(lineItemActual).State = EntityState.Deleted);

                                #region "Remove linked line items & Acutals value"
                                List<int> linkedLineItemIds = lineItemList.Where(tac => tac.LinkedLineItemId.HasValue).Select(line => line.LinkedLineItemId.Value).Distinct().ToList();
                                if (linkedLineItemIds != null && linkedLineItemIds.Count > 0)
                                {
                                    var linkedLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false) && linkedLineItemIds.Contains(lineItem.PlanLineItemId)).ToList();
                                    linkedLineItems.ForEach(lineItem => { lineItem.IsDeleted = true; lineItem.ModifiedDate = System.DateTime.Now; lineItem.ModifiedBy = Sessions.User.UserId; });
                                    var linkedActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lnkd => linkedLineItemIds.Contains(lnkd.PlanLineItemId)).ToList();
                                    linkedActualList.ForEach(lnkd => db.Entry(lnkd).State = EntityState.Deleted);
                                }
                                #endregion

                                //  List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted == false).ToList();

                                var tacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted == false && pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId).ToList();
                                // Added by Arpita Soni for Ticket #2354 on 07/14/2016
                                RemoveTacticsFromPackage(tacticList);
                                tacticList.ForEach(pcpt => { pcpt.IsDeleted = true; pcpt.ModifiedDate = System.DateTime.Now; pcpt.ModifiedBy = Sessions.User.UserId; });
                                var tacticIds = tacticList.Select(a => a.PlanTacticId).ToList();

                                #region "Remove linked Tactic"
                                List<int> linkedTacticIds = tacticList.Where(tac => tac.LinkedTacticId.HasValue).Select(line => line.LinkedTacticId.Value).Distinct().ToList();
                                if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                {
                                    var linkedTactics = db.Plan_Campaign_Program_Tactic.Where(lnkdtac => lnkdtac.IsDeleted == false && linkedTacticIds.Contains(lnkdtac.PlanTacticId)).ToList();
                                    linkedTactics.ForEach(lnkedTactic => { lnkedTactic.IsDeleted = true; lnkedTactic.ModifiedDate = System.DateTime.Now; lnkedTactic.ModifiedBy = Sessions.User.UserId; });

                                    RemoveTacticMediaCode(linkedTacticIds);
                                }
                                else
                                    linkedTacticIds = new List<int>();
                                #endregion

                                // added by devanshi #2386 Remove media codes
                                RemoveTacticMediaCode(tacticIds);
                                //end
                                //tblCustomField.Where(a => tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeTactic).ToList().ForEach(a => customFieldList.Add(a));
                                RemoveAlertRule(tacticIds);

                                var programList = db.Plan_Campaign_Program.Where(pcp => pcp.Plan_Campaign.PlanId == PlanId && pcp.IsDeleted == false).ToList();
                                programList.ForEach(pcp => { pcp.IsDeleted = true; pcp.ModifiedDate = System.DateTime.Now; pcp.ModifiedBy = Sessions.User.UserId; });
                                var programIds = programList.Select(a => a.PlanProgramId).ToList();
                                //tblCustomField.Where(a => programIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeProgram).ToList().ForEach(a => customFieldList.Add(a));
                                RemoveAlertRule(programIds);

                                var campaignList = db.Plan_Campaign.Where(pc => pc.PlanId == PlanId && pc.IsDeleted == false).ToList();
                                campaignList.ForEach(pc => { pc.IsDeleted = true; pc.ModifiedDate = System.DateTime.Now; pc.ModifiedBy = Sessions.User.UserId; });
                                var campaignIds = campaignList.Select(a => a.PlanCampaignId).ToList();
                                //tblCustomField.Where(a => campaignIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeCampaign).ToList().ForEach(a => customFieldList.Add(a));
                                RemoveAlertRule(campaignIds);

                                db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false).ToList().ForEach(p => p.IsDeleted = true);
                                List<int> lstplanid = new List<int>(PlanId);
                                RemoveAlertRule(lstplanid);

                                var customFieldList = db.CustomField_Entity.Where(a => (campaignIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeCampaign) || (programIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeProgram) || (tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeTactic) || (linkedTacticIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeTactic)).ToList();
                                customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                ///Delete entry in Default Table of that plan
                                var Label = Enums.FilterLabel.Plan.ToString();

                                Plan_UserSavedViews PlanList = db.Plan_UserSavedViews.FirstOrDefault(plan => plan.FilterName.Equals(Label) && plan.Userid == Sessions.User.UserId);
                                var DefaultPlanList = "";
                                if (PlanList != null)
                                {
                                    DefaultPlanList = PlanList.FilterValues;
                                }

                                var GetDefaultPlanList = new List<string>();
                                if (DefaultPlanList != null && DefaultPlanList != "")
                                {
                                    GetDefaultPlanList = DefaultPlanList.Split(',').ToList();
                                }
                                if (GetDefaultPlanList.Count > 0 && GetDefaultPlanList.Contains(PlanId.ToString()))
                                {
                                    GetDefaultPlanList.Remove(PlanId.ToString());
                                    var FinalList = string.Join(",", GetDefaultPlanList.Select(user => user.ToString()));
                                    if (FinalList != "" && FinalList != null)
                                    {
                                        PlanList.FilterValues = FinalList;
                                        db.Entry(PlanList).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        db.Entry(PlanList).State = EntityState.Deleted;
                                    }

                                }

                                //End


                            }
                            //// End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                            else if (section == Enums.Section.Campaign.ToString() && id != 0)
                            {

                                // List<Plan_Campaign_Program_Tactic_LineItem> tblLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false)).ToList();

                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId == id && a.IsDeleted.Equals(false)).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                #region "Remove linked line items & Acutals value"
                                List<int> linkedLineItemIds = plan_campaign_Program_Tactic_LineItemList.Where(tac => tac.LinkedLineItemId.HasValue).Select(line => line.LinkedLineItemId.Value).Distinct().ToList();
                                if (linkedLineItemIds != null && linkedLineItemIds.Count > 0)
                                {
                                    var linkedLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => linkedLineItemIds.Contains(lineItem.PlanLineItemId) && lineItem.IsDeleted.Equals(false)).ToList();
                                    linkedLineItems.ForEach(lineItem => { lineItem.IsDeleted = true; lineItem.ModifiedDate = System.DateTime.Now; lineItem.ModifiedBy = Sessions.User.UserId; });
                                    var linkedActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lnkd => linkedLineItemIds.Contains(lnkd.PlanLineItemId)).ToList();
                                    linkedActualList.ForEach(lnkd => db.Entry(lnkd).State = EntityState.Deleted);
                                }
                                #endregion

                                //   List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted == false).ToList();

                                var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.Plan_Campaign_Program.Plan_Campaign.PlanCampaignId == id && a.IsDeleted == false).ToList();
                                // Added by Arpita Soni for Ticket #2354 on 07/14/2016
                                RemoveTacticsFromPackage(Plan_Campaign_Program_TacticList);
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });


                                #region "Remove linked Tactic"
                                List<int> linkedTacticIds = Plan_Campaign_Program_TacticList.Where(tac => tac.LinkedTacticId.HasValue).Select(line => line.LinkedTacticId.Value).Distinct().ToList();
                                if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                {
                                    var linkedTactics = db.Plan_Campaign_Program_Tactic.Where(lnkdtac => linkedTacticIds.Contains(lnkdtac.PlanTacticId) && lnkdtac.IsDeleted == false).ToList();
                                    linkedTactics.ForEach(lnkedTactic => { lnkedTactic.IsDeleted = true; lnkedTactic.ModifiedDate = System.DateTime.Now; lnkedTactic.ModifiedBy = Sessions.User.UserId; });
                                    RemoveTacticMediaCode(linkedTacticIds);
                                }
                                else
                                    linkedTacticIds = new List<int>();
                                #endregion

                                var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.Plan_Campaign.PlanCampaignId == id).ToList();
                                Plan_Campaign_ProgramList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                                var programIds = Plan_Campaign_ProgramList.Select(a => a.PlanProgramId).ToList();
                                RemoveAlertRule(programIds);

                                var Plan_CampaignList = db.Plan_Campaign.Where(a => a.IsDeleted.Equals(false) && a.PlanCampaignId == id).ToList();
                                Plan_CampaignList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                                var campaignIds = Plan_CampaignList.Select(a => a.PlanCampaignId).ToList();
                                RemoveAlertRule(campaignIds);

                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var lineItemIds = plan_campaign_Program_Tactic_LineItemList.Select(a => a.PlanLineItemId).ToList();
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => lineItemIds.Contains(a.PlanLineItemId)).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                RemoveAlertRule(lineItemIds);

                                ////Start Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns
                                //// when campaign deleted then custom field's value for this campaign and custom field's value of appropriate program and tactic will be deleted

                                string sectionProgram = Enums.EntityType.Program.ToString();
                                var tacticIds = Plan_Campaign_Program_TacticList.Select(a => a.PlanTacticId).ToList();
                                string sectionTactic = Enums.EntityType.Tactic.ToString();

                                var campaign_customFieldList = db.CustomField_Entity.Where(a => (a.EntityId == id && a.CustomField.EntityType == section) || (programIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionProgram) || (tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic) || (linkedTacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic)).ToList();
                                campaign_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                ////End Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns

                                // added by devanshi #2386 Remove media codes
                                RemoveTacticMediaCode(tacticIds);
                                //end

                                RemoveAlertRule(tacticIds);

                            }
                            else if (section == Enums.Section.Program.ToString() && id != 0)
                            {
                                //  List<Plan_Campaign_Program_Tactic_LineItem> tblLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false)).ToList();

                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanProgramId == id && a.IsDeleted.Equals(false)).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                #region "Remove linked line items & Acutals value"
                                List<int> linkedLineItemIds = plan_campaign_Program_Tactic_LineItemList.Where(tac => tac.LinkedLineItemId.HasValue).Select(line => line.LinkedLineItemId.Value).Distinct().ToList();
                                if (linkedLineItemIds != null && linkedLineItemIds.Count > 0)
                                {
                                    var linkedLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => linkedLineItemIds.Contains(lineItem.PlanLineItemId) && lineItem.IsDeleted.Equals(false)).ToList();
                                    linkedLineItems.ForEach(lineItem => { lineItem.IsDeleted = true; lineItem.ModifiedDate = System.DateTime.Now; lineItem.ModifiedBy = Sessions.User.UserId; });
                                    var linkedActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lnkd => linkedLineItemIds.Contains(lnkd.PlanLineItemId)).ToList();
                                    linkedActualList.ForEach(lnkd => db.Entry(lnkd).State = EntityState.Deleted);
                                }
                                #endregion
                                // List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted == false).ToList();

                                var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.Plan_Campaign_Program.PlanProgramId == id && a.IsDeleted == false).ToList();
                                // Added by Arpita Soni for Ticket #2354 on 07/14/2016
                                RemoveTacticsFromPackage(Plan_Campaign_Program_TacticList);
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                #region "Remove linked Tactic"
                                List<int> linkedTacticIds = Plan_Campaign_Program_TacticList.Where(tac => tac.LinkedTacticId.HasValue).Select(line => line.LinkedTacticId.Value).Distinct().ToList();
                                if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                {
                                    var linkedTactics = db.Plan_Campaign_Program_Tactic.Where(lnkdtac => linkedTacticIds.Contains(lnkdtac.PlanTacticId) && lnkdtac.IsDeleted == false).ToList();
                                    linkedTactics.ForEach(lnkedTactic => { lnkedTactic.IsDeleted = true; lnkedTactic.ModifiedDate = System.DateTime.Now; lnkedTactic.ModifiedBy = Sessions.User.UserId; });
                                    RemoveTacticMediaCode(linkedTacticIds);
                                }
                                else
                                    linkedTacticIds = new List<int>();
                                #endregion

                                var Plan_Campaign_ProgramList = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == id).ToList();
                                Plan_Campaign_ProgramList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });
                                RemoveAlertRule(Plan_Campaign_ProgramList.Select(a => a.PlanProgramId).ToList());

                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var lineItemIds = plan_campaign_Program_Tactic_LineItemList.Select(a => a.PlanLineItemId).ToList();
                                RemoveAlertRule(lineItemIds);
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => lineItemIds.Contains(a.PlanLineItemId)).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                ////Start Added by Mitesh Vaishnav for PL ticket #719 Custom fields for programs
                                //// when program deleted then custom field's value for this program and custom field's value of appropriate tactic will be deleted
                                var tacticIds = Plan_Campaign_Program_TacticList.Select(a => a.PlanTacticId).ToList();
                                string sectionTactic = Enums.EntityType.Tactic.ToString();
                                var program_customFieldList = db.CustomField_Entity.Where(a => (a.EntityId == id && a.CustomField.EntityType == section) || (tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic) || (linkedTacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic)).ToList();
                                program_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                RemoveTacticMediaCode(tacticIds);
                                RemoveAlertRule(tacticIds);
                                //var tactic_customFieldList = tblCustomField.Where(a => tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == sectionTactic).ToList();
                                //tactic_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                ////End Added by Mitesh Vaishnav for PL ticket #719 Custom fields for programs
                            }
                            else if (section == Enums.Section.Tactic.ToString() && id != 0)
                            {
                                //   List<Plan_Campaign_Program_Tactic_LineItem> tblLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted.Equals(false)).ToList();
                                var plan_campaign_Program_Tactic_LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.PlanTacticId == id && a.IsDeleted.Equals(false)).ToList();
                                plan_campaign_Program_Tactic_LineItemList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                #region "Remove linked line items & Acutals value"
                                List<int> linkedLineItemIds = plan_campaign_Program_Tactic_LineItemList.Where(tac => tac.LinkedLineItemId.HasValue).Select(line => line.LinkedLineItemId.Value).Distinct().ToList();
                                if (linkedLineItemIds != null && linkedLineItemIds.Count > 0)
                                {
                                    var linkedLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => linkedLineItemIds.Contains(lineItem.PlanLineItemId) && lineItem.IsDeleted.Equals(false)).ToList();
                                    linkedLineItems.ForEach(lineItem => { lineItem.IsDeleted = true; lineItem.ModifiedDate = System.DateTime.Now; lineItem.ModifiedBy = Sessions.User.UserId; });
                                    var linkedActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lnkd => linkedLineItemIds.Contains(lnkd.PlanLineItemId)).ToList();
                                    linkedActualList.ForEach(lnkd => db.Entry(lnkd).State = EntityState.Deleted);
                                }
                                #endregion

                                //  List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted == false).ToList();
                                var Plan_Campaign_Program_TacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted.Equals(false) && a.PlanTacticId == id).ToList();
                                // Added by Arpita Soni for Ticket #2354 on 07/14/2016
                                RemoveTacticsFromPackage(Plan_Campaign_Program_TacticList);
                                Plan_Campaign_Program_TacticList.ForEach(a => { a.IsDeleted = true; a.ModifiedDate = System.DateTime.Now; a.ModifiedBy = Sessions.User.UserId; });

                                #region "Remove linked Tactic"
                                List<int> linkedTacticIds = Plan_Campaign_Program_TacticList.Where(tac => tac.LinkedTacticId.HasValue).Select(line => line.LinkedTacticId.Value).Distinct().ToList();
                                if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                {
                                    var linkedTactics = db.Plan_Campaign_Program_Tactic.Where(lnkdtac => linkedTacticIds.Contains(lnkdtac.PlanTacticId) && lnkdtac.LinkedTacticId == id).ToList();
                                    linkedTactics.ForEach(lnkedTactic => { lnkedTactic.IsDeleted = true; lnkedTactic.ModifiedDate = System.DateTime.Now; lnkedTactic.ModifiedBy = Sessions.User.UserId; });
                                    RemoveTacticMediaCode(linkedTacticIds);
                                }
                                else
                                    linkedTacticIds = new List<int>();
                                #endregion
                                ////Added by Mitesh Vaishnav for PL ticket #571 Input actual costs - Tactics.
                                var lineItemIds = plan_campaign_Program_Tactic_LineItemList.Select(a => a.PlanLineItemId).ToList();
                                RemoveAlertRule(lineItemIds);
                                var plan_campaign_Program_Tactic_LineItem_ActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => lineItemIds.Contains(a.PlanLineItemId)).ToList();
                                plan_campaign_Program_Tactic_LineItem_ActualList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                ////Start Added by Mitesh Vaishnav for PL ticket #720 Custom fields for tactics
                                //// when tactic deleted then custom field's value for this tactic will be deleted 
                                var tactic_customFieldList = db.CustomField_Entity.Where(a => a.EntityId == id && a.CustomField.EntityType == section || (linkedTacticIds.Contains(a.EntityId) && a.CustomField.EntityType == section)).ToList();
                                tactic_customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                ////End Added by Mitesh Vaishnav for PL ticket #720 Custom fields for tactic

                                // added by devanshi #2386 Remove media codes
                                #region remove MediaCode
                                var MediaCodeCustomfields = db.Tactic_MediaCodes_CustomFieldMapping.Where(a => a.TacticId == id).ToList();
                                MediaCodeCustomfields.ForEach(custmfield => db.Entry(custmfield).State = EntityState.Deleted);
                                var TacticMediacode = db.Tactic_MediaCodes.Where(a => a.TacticId == id).ToList();
                                TacticMediacode.ForEach(mediacode => db.Entry(mediacode).State = EntityState.Deleted);
                                #endregion
                                //end
                                RemoveAlertRule(Plan_Campaign_Program_TacticList.Select(a => a.PlanTacticId).ToList());
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
            catch (Exception)
            {
                return returnValue;
            }
            return returnValue;
        }

        #region ROI Package
        /// <summary>
        /// Delete tactics from package when tactic/program/campaign/plan
        /// Added By : Arpita Soni 
        /// Ticket : #2354
        /// </summary>
        /// <param name="lstTactics"></param>
        /// <returns></returns>
        public static void RemoveTacticsFromPackage(List<Plan_Campaign_Program_Tactic> lstTactics)
        {
            MRPEntities db = new MRPEntities();
            List<ROI_PackageDetail> lstROIPackage = new List<ROI_PackageDetail>();
            List<ROI_PackageDetail> delROIPackage = new List<ROI_PackageDetail>();

            // Get list of packages for given tactic list
            lstTactics.ForEach(x => lstROIPackage.AddRange(x.ROI_PackageDetail));

            // Remove all tactics from the package when anchor tactic is deleted
            delROIPackage.AddRange((from pkg in lstROIPackage
                                    join package in db.ROI_PackageDetail on pkg.AnchorTacticID equals package.AnchorTacticID
                                    where pkg.AnchorTacticID == pkg.PlanTacticId
                                    select package).ToList());

            // Remove only one tactic from the package when promotion tactic is deleted
            delROIPackage.AddRange((from pkg in lstROIPackage
                                    join package in db.ROI_PackageDetail on pkg.PlanTacticId equals package.PlanTacticId
                                    select package).ToList());

            // Update AnchorTacticId of tactics in cache
            Dictionary<int, int> planTacAnchorTac = new Dictionary<int, int>();
            delROIPackage.Distinct().ToList().ForEach(pkg => planTacAnchorTac.Add(pkg.PlanTacticId, 0));
            Common.UpdateAnchorTacticInCache(planTacAnchorTac);

            delROIPackage.ForEach(x => db.Entry(x).State = EntityState.Deleted);
            db.SaveChanges();
        }

        /// <summary>
        /// Update package details in tactic list in cache
        /// Added By : Arpita Soni 
        /// Ticket : #2357
        /// </summary>
        /// <param name="dictPlanTacticAnchorTactic"></param>
        public static void UpdateAnchorTacticInCache(Dictionary<int, int> dictPlanTacticAnchorTactic)
        {
            CacheObject objCache = new CacheObject();
            List<Custom_Plan_Campaign_Program_Tactic> lstTacticPer = new List<Custom_Plan_Campaign_Program_Tactic>();
            lstTacticPer = (List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.CustomTactic.ToString());
            if (lstTacticPer != null && lstTacticPer.Count > 0)
            {
                lstTacticPer = lstTacticPer.Where(tactic => tactic.IsDeleted.Equals(false)).ToList();

                (from dict in dictPlanTacticAnchorTactic
                 join tactic in lstTacticPer on dict.Key equals tactic.PlanTacticId
                 select tactic).ToList()
                 .ForEach(tac =>
                 {
                     tac.AnchorTacticId = dictPlanTacticAnchorTactic[tac.PlanTacticId];
                     tac.PackageTitle = lstTacticPer.Where(x => x.PlanTacticId == dictPlanTacticAnchorTactic[tac.PlanTacticId]).
                                                     Select(t => t.Title).FirstOrDefault();
                 });

                objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), lstTacticPer);
            }

            //Added by komal rawal for #2358 to update cache object
            List<Custom_Plan_Campaign_Program_Tactic> PlanTacticListforpackageing = new List<Custom_Plan_Campaign_Program_Tactic>();

            PlanTacticListforpackageing = (List<Custom_Plan_Campaign_Program_Tactic>)objCache.Returncache(Enums.CacheObject.PlanTacticListforpackageing.ToString());
            if (PlanTacticListforpackageing != null && PlanTacticListforpackageing.Count > 0)
            {
                (from dict in dictPlanTacticAnchorTactic
                 join tactic in PlanTacticListforpackageing on dict.Key equals tactic.PlanTacticId
                 select tactic).ToList()
                 .ForEach(tac =>
                 {
                     tac.AnchorTacticId = dictPlanTacticAnchorTactic[tac.PlanTacticId];
                     tac.PackageTitle = PlanTacticListforpackageing.Where(x => x.PlanTacticId == dictPlanTacticAnchorTactic[tac.PlanTacticId]).
                                                     Select(t => t.Title).FirstOrDefault();
                 });

                objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), PlanTacticListforpackageing);
            }
            //End

            // Added by Arpita Soni for Ticket #2358 on 07/27/2016
            // To update dsPlanCampProgTac object into cache which is used to show activity distribution chart
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = (DataSet)objCache.Returncache(Enums.CacheObject.dsPlanCampProgTac.ToString());
            if (dsPlanCampProgTac != null && dsPlanCampProgTac.Tables.Count > 0)
            {
                (from dict in dictPlanTacticAnchorTactic
                 join tactic in dsPlanCampProgTac.Tables[3].AsEnumerable() on dict.Key equals tactic.Field<int>("PlanTacticId")
                 select tactic).ToList<DataRow>()
                     .ForEach(tac =>
                     {
                         tac["AnchorTacticId"] = dictPlanTacticAnchorTactic[tac.Field<int>("PlanTacticId")];
                         tac["PackageTitle"] = dsPlanCampProgTac.Tables[3].AsEnumerable().Where(x => x.Field<int>("PlanTacticId") == dictPlanTacticAnchorTactic[tac.Field<int>("PlanTacticId")]).
                                                                 Select(t => t.Field<string>("Title")).FirstOrDefault();
                     });
                objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);
            }
        }
        #endregion
        // added by devanshi #2386 Remove media codes
        #region remove MediaCode
        public static void RemoveTacticMediaCode(List<int> TacticIDs)
        {
            MRPEntities db = new MRPEntities();
            var MediaCodeCustomfields = db.Tactic_MediaCodes_CustomFieldMapping.Where(a => TacticIDs.Contains(a.TacticId)).ToList();
            MediaCodeCustomfields.ForEach(custmfield => db.Entry(custmfield).State = EntityState.Deleted);
            int resultmedia = db.SaveChanges();
            if (resultmedia > 0)
            {
                var TacticMediacode = db.Tactic_MediaCodes.Where(a => TacticIDs.Contains(a.TacticId)).ToList();
                TacticMediacode.ForEach(mediacode => db.Entry(mediacode).State = EntityState.Deleted);
                db.SaveChanges();
            }

        }
        #endregion
        //end
        // added by devanshi Remove Alerts and alertRule
        #region remove Alert Rule
        public static void RemoveAlertRule(List<int> Ids)
        {
            MRPEntities db = new MRPEntities();
            if (Ids.Count > 0)
            {
                var AlertsRule = db.Alert_Rules.Where(a => Ids.Contains(a.EntityId)).ToList();
                foreach (var objrule in AlertsRule)
                {
                    var lstAlerts = db.Alerts.Where(a => a.RuleId == objrule.RuleId).ToList();
                    lstAlerts.ForEach(alert => db.Entry(alert).State = EntityState.Deleted);
                    db.SaveChanges();
                    db.Entry(objrule).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }

        }
        #endregion
        //end
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
                                                                                     .OrderByDescending(la => la.CreatedDate).FirstOrDefault();
            if (lineItemListActuals != null)
            {
                modifiedDate = lineItemListActuals.CreatedDate;
                createdBy = lineItemListActuals.CreatedBy.ToString();
            }
            else
            {
                ////Checking whether Tactic Actual exist.
                var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(tacticActual => tacticActual.PlanTacticId == tacticId)
                                                                             .OrderByDescending(ta => ta.ModifiedDate)
                                                                             .ThenBy(ta => ta.CreatedDate)
                                                                             .FirstOrDefault();
                if (tacticActualList != null)
                {
                    if (tacticActualList.ModifiedDate != null)
                    {
                        modifiedDate = tacticActualList.ModifiedDate;
                        createdBy = tacticActualList.ModifiedBy.ToString();
                    }
                    else
                    {
                        modifiedDate = tacticActualList.CreatedDate;
                        createdBy = tacticActualList.CreatedBy.ToString();
                    }
                }

            }

            if (createdBy != null && modifiedDate != null)
            {
                ////Checking if created by is empty then system generated modification
                if (Guid.Parse(createdBy) == Guid.Empty)
                {
                    lastModifiedMessage = string.Format("{0} {1} by {2}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd, yyyy"), GameplanIntegrationService);
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
                            lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd, yyyy"), userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().FirstName, userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().LastName);
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
                        lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd, yyyy"), objUser.FirstName, objUser.LastName);
                        return lastModifiedMessage;
                    }
                }
            }

            return lastModifiedMessage;
        }
        /// <summary>
        /// Function to generate message for modification of tactic from Actual tab.
        /// Added by Rahul Shah         
        /// </summary>
        /// <param name="tacticId">Plan Tactic Id.</param>
        /// <param name="userList">List of user object.</param>
        /// <param name="userList">List of lineitemActual object.</param>
        /// <param name="userList">List of tactic Actual object.</param>
        /// <returns>If any tactic actual or LineItem exist then it return string message else empty string </returns>
        public static string TacticModificationMessageActual(int tacticId, List<User> userList = null, List<Plan_Campaign_Program_Tactic_LineItem_Actual> lineItemActList = null, List<Plan_Campaign_Program_Tactic_Actual> tacticActList = null)
        {
            string lastModifiedMessage = string.Empty;
            string createdBy = null;
            DateTime? modifiedDate = null;

            //// Checking whether line item actuals exists.
            var lineItemListActuals = lineItemActList.Where(lineitemActual => lineitemActual.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId == tacticId &&
                                                                                            lineitemActual.Plan_Campaign_Program_Tactic_LineItem.IsDeleted == false)
                                                                                     .OrderByDescending(la => la.CreatedDate).FirstOrDefault();
            if (lineItemListActuals != null)
            {
                modifiedDate = lineItemListActuals.CreatedDate;
                createdBy = lineItemListActuals.CreatedBy.ToString();
            }
            else
            {
                ////Checking whether Tactic Actual exist.
                var tacticActualList = tacticActList.Where(tacticActual => tacticActual.PlanTacticId == tacticId)
                                                                             .OrderByDescending(ta => ta.ModifiedDate)
                                                                             .ThenBy(ta => ta.CreatedDate)
                                                                             .FirstOrDefault();
                if (tacticActualList != null)
                {
                    if (tacticActualList.ModifiedDate != null)
                    {
                        modifiedDate = tacticActualList.ModifiedDate;
                        createdBy = tacticActualList.ModifiedBy.ToString();
                    }
                    else
                    {
                        modifiedDate = tacticActualList.CreatedDate;
                        createdBy = tacticActualList.CreatedBy.ToString();
                    }
                }

            }

            if (createdBy != null && modifiedDate != null)
            {
                ////Checking if created by is empty then system generated modification
                if (Guid.Parse(createdBy) == Guid.Empty)
                {
                    lastModifiedMessage = string.Format("{0} {1} by {2}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd, yyyy"), GameplanIntegrationService);
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
                            lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd, yyyy"), userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().FirstName, userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().LastName);
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
                        lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd, yyyy"), objUser.FirstName, objUser.LastName);
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
                    lastModifiedMessage = string.Format("{0} {1} by {2}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd,yyyy"), GameplanIntegrationService);
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
                            lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd,yyyy"), userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().FirstName, userList.Where(u => u.UserId == Guid.Parse(createdBy)).FirstOrDefault().LastName);
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
                        lastModifiedMessage = string.Format("{0} {1} by {2} {3}", "Last updated", Convert.ToDateTime(modifiedDate).ToString("MMM dd,yyyy"), objUser.FirstName, objUser.LastName);
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
                result = r.Replace(result, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"//www");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// Function to generate HTML with Anchor link if text contains url.
        /// Method used in all inspect popup to generate HTML based text.
        /// Added by Ravindra on 02/07/2015
        /// </summary>
        /// <param name="description">attribute Text.</param>
        public static string GenerateHTMLAttribute(string attribute = "")
        {
            bool isvalidformate = false;//PL #1341 added by dashrath prajapati
            string result = string.Empty;
            if (string.IsNullOrEmpty(attribute))
                return result;
            try
            {
                string[] _attribute = attribute.Split(' ');
                for (int i = 0; i < _attribute.Length; i++)
                {
                    //isvalidformate = Regex.IsMatch(_attribute[i], @"^(?<http>(http:[/][/]|www.)([a-z]|[A-Z]|[0-9]|[/.]|[~])*)$");
                    isvalidformate = Regex.IsMatch(_attribute[i], @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,_\[\]\(\)\!\$\*\|##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])");
                    if (isvalidformate)
                    {
                        result += _attribute[i];
                        string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,_\[\]\(\)\!\$\*\|##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
                        Regex r = new Regex(regex, RegexOptions.IgnoreCase);
                        result = result.Replace("\n", "<br />");
                        result = r.Replace(result, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"//www");
                        if (!result.Contains("www"))
                        {
                            result = "<a href=" + result + " title='Click to open in a new window or tab' target='_blank'>" + result + " </a>";
                        }
                        result = result + "&nbsp;";
                    }
                    else
                    {
                        result += _attribute[i] + "&nbsp;";
                    }
                }
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
            //userName = bdsservice.GetMultipleTeamMemberName(UserGuid);
            userName = bdsservice.GetMultipleTeamMemberNameByApplicationId(UserGuid, Sessions.ApplicationId);
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

        #region OtherBudgetItem
        // Add By Nishant Sheth
        public static int GetOtherBudgetId()
        {
            MRPEntities db = new MRPEntities();

            var item = (from ParentBudget in db.Budgets
                        join
                            ChildBudget in db.Budget_Detail on ParentBudget.Id equals ChildBudget.BudgetId
                        where ParentBudget.ClientId == Sessions.User.ClientId
                        && (ParentBudget.IsDeleted == false || ParentBudget.IsDeleted == null)
                        && ParentBudget.IsOther == true
                        select new
                        {
                            Id = ChildBudget.Id
                        }).FirstOrDefault();
            return item.Id;
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
                        var entityObj = objMRPEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == InspectEntityId).Select(tactic => new { tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId, tactic.IsDeleted, tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId });
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
                        var entityObj = objMRPEntities.Plan_Campaign_Program.Where(program => program.PlanProgramId == InspectEntityId).Select(program => new { program.Plan_Campaign.Plan.Model.ClientId, program.IsDeleted, program.Plan_Campaign.Plan.PlanId });
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
                        var entityObj = objMRPEntities.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == InspectEntityId).Select(campaign => new { campaign.Plan.Model.ClientId, campaign.IsDeleted, campaign.Plan.PlanId });
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
                        var entityObj = objMRPEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId == InspectEntityId).Select(improvementTactic => new { improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.ClientId, improvementTactic.IsDeleted, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId });
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
                    if (EntityType.Equals(Enums.PlanEntity.LineItem.ToString()))
                    {
                        var entityObj = objMRPEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanLineItemId == InspectEntityId).Select(lineItem => new { lineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId, lineItem.IsDeleted, lineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId });
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
            SelectListItem objSelectListItem;
            if (lstEloquaContactFields != null)
            {
                //// Prepare select list for contact list 
                foreach (var contact in lstEloquaContactFields)
                {
                    objSelectListItem = new SelectListItem();
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

        #region Custom Restriction Settings

        #region Get Custom Restriction List
        /// <summary>
        /// Function to retrieve the list of Custom Restriction of a user.
        /// </summary>
        /// <param name="userId">user id of a user for which custom restriction list to be retrieved</param>
        /// <returns>returns list of CustomRestriction objects</returns>
        public static List<Models.CustomRestriction> GetUserCustomRestrictionsList(Guid userId, bool IsRequiredOnly = false)
        {
            List<Models.CustomRestriction> lstCustomRestriction = new List<Models.CustomRestriction>();
            try
            {
                using (MRPEntities objDB = new MRPEntities())
                {
                    lstCustomRestriction = objDB.CustomRestrictions.Where(customRestriction => customRestriction.UserId == userId && (IsRequiredOnly ? customRestriction.CustomField.IsRequired.Equals(true) : true))
                                                                    .Select(customRestriction => customRestriction).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstCustomRestriction;
        }

        /// <summary>
        /// Function to get list of userIds for the given client using Custom Restrictions
        /// </summary>
        /// <param name="clientId">client Id</param>
        /// <returns></returns>
        public static List<Guid> GetClientUserListUsingCustomRestrictions(Guid clientId, List<User> lstUsers)
        {
            List<Guid> lstClientUsers = new List<Guid>();
            List<Guid> lstClient = new List<Guid>();
            List<Guid> lstClientWithoutAnyPermissios = new List<Guid>();

            try
            {
                using (MRPEntities objDB = new MRPEntities())
                {
                    //// List of custom fields of all user of logged in client
                    List<int> lstCustomFields = objDB.CustomFields.Where(customField => customField.ClientId == clientId && customField.IsDeleted == false).Select(customField => customField.CustomFieldId).ToList();
                    List<Guid> lstUserIDs = lstUsers.Select(user => user.UserId).ToList();

                    if (lstCustomFields.Count() > 0)
                    {
                        //Modified By Komal Rawal for #1360
                        //// Get list of users who have View and ViewEdit rights

                        int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                        int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                        int NoPermission = (int)Enums.CustomRestrictionPermission.None;
                        var lstcustomrestriction = objDB.CustomRestrictions.Where(customRestriction => lstCustomFields.Contains(customRestriction.CustomFieldId) && lstUserIDs.Contains(customRestriction.UserId))
                                                                        .Select(customRestriction => new { customRestriction.UserId, customRestriction.Permission }).ToList();
                        lstClientUsers = lstcustomrestriction.Where(customRestriction => (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission))
                                                                        .Select(customRestriction => customRestriction.UserId).Distinct().ToList();

                        lstClient = lstcustomrestriction.Where(customRestriction => (customRestriction.Permission == NoPermission))
                                                                       .Select(customRestriction => customRestriction.UserId).Distinct().ToList();
                        lstClientWithoutAnyPermissios = lstUsers.Where(user => !lstClient.Contains(user.UserId) && !lstClientUsers.Contains(user.UserId)).Select(user => user.UserId).ToList().Distinct().ToList();

                        //// Get default custom restriction is viewable or not
                        bool isDefaultRestrictionsViewable = IsDefaultCustomRestrictionsViewable();

                        if (isDefaultRestrictionsViewable)
                        {
                            return lstClientUsers.Concat(lstClientWithoutAnyPermissios).ToList();

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstClientUsers;
        }
        #endregion

        #region Get Custom Restriction user list based on permission
        /// <summary>
        /// Function to retrieve list of custom restriction of None type
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="permissionType">permission type</param>
        /// <returns>returns list of CustomRestriction object</returns>
        public static List<Models.CustomRestriction> GetCustomRestrictionListByPermission(Guid userId, Enums.CustomRestrictionPermission permissionType)
        {
            List<Models.CustomRestriction> lstCustomRestriction = new List<Models.CustomRestriction>();
            try
            {
                int Permission = -1;

                if (permissionType.Equals(Enums.CustomRestrictionPermission.None))
                {
                    Permission = (int)Enums.CustomRestrictionPermission.None;
                }
                else if (permissionType.Equals(Enums.CustomRestrictionPermission.ViewOnly))
                {
                    Permission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                }
                else if (permissionType.Equals(Enums.CustomRestrictionPermission.ViewEdit))
                {
                    Permission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                }

                using (MRPEntities objDB = new MRPEntities())
                {
                    lstCustomRestriction = objDB.CustomRestrictions.Where(customRestriction => customRestriction.UserId == userId && customRestriction.Permission == Permission)
                                                                    .Select(customRestriction => customRestriction).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstCustomRestriction;
        }
        #endregion

        #region Get Restricted EntityId list
        /// <summary>
        /// Function to retrieve list of restricted entities
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>returns list of entities</returns>
        public static List<int> GetRestrictedEntityList(Guid userId, out bool isCustomRestrictionSet)
        {
            List<int> lstEntityIds = new List<int>();
            isCustomRestrictionSet = true;
            try
            {
                List<Models.CustomRestriction> lstNoneCustomRestriction = new List<Models.CustomRestriction>();
                lstNoneCustomRestriction = GetCustomRestrictionListByPermission(userId, Enums.CustomRestrictionPermission.None);

                if (lstNoneCustomRestriction.Count > 0)
                {
                    List<int> lstCustomFieldIds = new List<int>();
                    List<string> lstCustomFieldOptionIds = new List<string>();

                    lstNoneCustomRestriction.ForEach(customRestriction =>
                    {
                        lstCustomFieldIds.Add(customRestriction.CustomFieldId);
                        lstCustomFieldOptionIds.Add(customRestriction.CustomFieldOptionId.ToString());
                    });

                    using (MRPEntities objDB = new MRPEntities())
                    {
                        string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                        string EntityTypeTactic = Enums.EntityType.Tactic.ToString();

                        lstEntityIds = objDB.CustomField_Entity.Where(customFieldEntity => lstCustomFieldIds.Contains(customFieldEntity.CustomFieldId) &&
                            //customFieldEntity.CustomField.IsDisplayForFilter.Equals(true) && 
                                                                    customFieldEntity.CustomField.EntityType == EntityTypeTactic && customFieldEntity.CustomField.CustomFieldType.Name == DropDownList &&
                                                                        lstCustomFieldOptionIds.Contains(customFieldEntity.Value))
                                                                .Select(customFieldEntity => customFieldEntity.EntityId).ToList();
                    }
                }
                else
                {
                    using (MRPEntities objDB = new MRPEntities())
                    {
                        isCustomRestrictionSet = objDB.CustomRestrictions.Where(customRestriction => customRestriction.UserId == userId).Any();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstEntityIds;
        }
        #endregion

        #region Get ViewEdit EntityId list
        /// <summary>
        /// Function to retrieve list of ViewEdit entities
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>returns list of entities</returns>
        public static List<int> GetViewEditEntityList(Guid userId, List<int> lstTacticId)
        {
            List<int> lstEntityIds = new List<int>();
            try
            {
                List<Models.CustomRestriction> lstViewEditCustomRestriction = new List<Models.CustomRestriction>();
                lstViewEditCustomRestriction = GetCustomRestrictionListByPermission(userId, Enums.CustomRestrictionPermission.ViewEdit);

                if (lstViewEditCustomRestriction.Count > 0)
                {
                    List<int> lstCustomFieldIds = new List<int>();
                    List<string> lstCustomFieldOptionIds = new List<string>();

                    lstViewEditCustomRestriction.ForEach(customRestriction =>
                    {
                        lstCustomFieldIds.Add(customRestriction.CustomFieldId);
                        lstCustomFieldOptionIds.Add(customRestriction.CustomFieldOptionId.ToString());
                    });

                    using (MRPEntities objDB = new MRPEntities())
                    {
                        string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                        string EntityTypeTactic = Enums.EntityType.Tactic.ToString();

                        lstEntityIds = objDB.CustomField_Entity.Where(customFieldEntity => lstCustomFieldIds.Contains(customFieldEntity.CustomFieldId) &&
                            //customFieldEntity.CustomField.IsDisplayForFilter.Equals(true) &&
                                                                    customFieldEntity.CustomField.EntityType == EntityTypeTactic && customFieldEntity.CustomField.CustomFieldType.Name == DropDownList &&
                                                                    lstCustomFieldOptionIds.Contains(customFieldEntity.Value) && lstTacticId.Contains(customFieldEntity.EntityId))
                                                                .Select(customFieldEntity => customFieldEntity.EntityId).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstEntityIds;
        }
        #endregion

        #region Get list of Viewable entity Ids(tactic)
        /// <summary>
        /// Function to retrieve list of viewable entity ids(tactic)
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="clientId">client id</param>
        /// <returns></returns>
        public static List<int> GetViewableTacticList(Guid userId, Guid clientId, List<int> lstTactic, bool isDisplayForFilter = true, List<CustomField_Entity> customfieldlEntityist = null)
        {
            List<int> lstAllowedEntityIds = new List<int>();
            CacheObject dataCache = new CacheObject();
            StoredProcedure objSp = new StoredProcedure();
            try
            {
                using (MRPEntities objDbMrpEntities = new MRPEntities())
                {
                    if (lstTactic.Count() > 0 && Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), clientId))
                    {
                        //// Get list customFieldEntity List for given tactic list
                        string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                        string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
                        //Added by Komal Rawal
                        var customfieldlist = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == clientId && customfield.EntityType.Equals(EntityTypeTactic)).Select(customfield => customfield).ToList();
                        var CustomFieldexists = customfieldlist.Where(customfield => (customfield.IsRequired && !isDisplayForFilter)).Select(customfield => customfield).Any();
                        if (!CustomFieldexists)
                        {
                            return lstTactic;
                        }


                        var customfieldList = customfieldlist.Where(customField => customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                        customField.IsDeleted.Equals(false) &&
                                                                                                        customField.EntityType.Equals(EntityTypeTactic) &&
                                                                                                        customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                        (isDisplayForFilter ? customField.IsDisplayForFilter.Equals(true) : true)).Select(customField => customField.CustomFieldId).ToList();


                        var tblCustomFieldEntity = customfieldlEntityist != null && customfieldlEntityist.Count() > 0 ? customfieldlEntityist.Where(customFieldEntity => customfieldList.Contains(customFieldEntity.CustomFieldId))
                            .Select(customFieldEntity => new { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList() :
                            objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldList.Contains(customFieldEntity.CustomFieldId))
                            .Select(customFieldEntity => new { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList();
                        tblCustomFieldEntity = (from tbl in tblCustomFieldEntity
                                                join lst in lstTactic on tbl.EntityId equals lst
                                                select tbl).ToList();
                        // Add By Nishant Sheth
                        // Desc :: owner lists are wrong 
                        var distinctcustomfieldids = tblCustomFieldEntity.Select(a => a.EntityId).Distinct().ToList();
                        var customexpecttactic = lstTactic.Where(tactic => !distinctcustomfieldids.Contains(tactic)).ToList();
                        lstAllowedEntityIds.AddRange(customexpecttactic);
                        // End By Nishant Sheth
                        if (tblCustomFieldEntity == null || !tblCustomFieldEntity.Any())
                        {
                            return lstTactic;

                        }
                        //End

                        if (tblCustomFieldEntity.Count > 0)
                        {
                            //// Get Custom Restrictions
                            var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(userId, true);

                            //// Get default custom restriction is viewable or not
                            bool isDefaultRestrictionsViewable = IsDefaultCustomRestrictionsViewable();

                            if (userCustomRestrictionList.Count() > 0)
                            {
                                //// Get list of tactic Ids
                                List<int> lstTacticIds = tblCustomFieldEntity.Select(entity => entity.EntityId).Distinct().ToList();

                                //// Get list of all custom field Ids
                                var lstAllCustomFieldIds = tblCustomFieldEntity.Select(entity => entity.CustomFieldId).Distinct().ToList();

                                //// Get list of all custom field option Ids
                                var lstAllCustomFieldOptionIds = objDbMrpEntities.CustomFieldOptions.Where(option => lstAllCustomFieldIds.Contains(option.CustomFieldId)).Select(option => option.CustomFieldOptionId).Distinct().ToList();

                                //bool isViewableEntity = true, isViewable;
                                int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                                int NonePermission = (int)Enums.CustomRestrictionPermission.None;
                                //List<int> currentTacticCustomFields = new List<int>();
                                // List<string> AllowedRightsForCurrentCustomField = new List<string>();
                                // List<int> lstRestrictedCustomFieldOptionIds = new List<int>();
                                // List<Models.CustomRestriction> fltrCustomRestriction = new List<Models.CustomRestriction>();


                                var vieweditoptionid = userCustomRestrictionList.Where(restriction => restriction.Permission == ViewOnlyPermission || restriction.Permission == ViewEditPermission).Select(res => res.CustomFieldOptionId).ToList();
                                var noneoptionid = userCustomRestrictionList.Where(restriction => restriction.Permission == NonePermission).Select(res => res.CustomFieldOptionId).ToList();


                                if (isDefaultRestrictionsViewable)
                                {
                                    var onlyviewtacticids = tblCustomFieldEntity.Where(tac => vieweditoptionid.Contains(int.Parse(tac.Value)) && !noneoptionid.Contains(int.Parse(tac.Value))).Select(tac => tac.EntityId).Distinct().ToList();
                                    //// set list of viewable tactic Ids
                                    lstAllowedEntityIds.AddRange(onlyviewtacticids);
                                }
                                else
                                {
                                    var onlyviewtacticids = tblCustomFieldEntity.Where(tac => (vieweditoptionid.Contains(int.Parse(tac.Value)) || lstAllCustomFieldOptionIds.Contains(int.Parse(tac.Value))) && !noneoptionid.Contains(int.Parse(tac.Value))).Select(tac => tac.EntityId).Distinct().ToList();
                                    lstAllowedEntityIds.AddRange(onlyviewtacticids);
                                }


                                //foreach (int tacticId in lstTactic)
                                //{
                                //    isViewableEntity = true;
                                //    var currentTacticEntities = tblCustomFieldEntity.Where(entity => entity.EntityId == tacticId).ToList();
                                //    //// Get list of CustomFieldEntities for current selected tactic
                                //    currentTacticCustomFields = currentTacticEntities.Select(entity => entity.CustomFieldId).Distinct().ToList();

                                //    foreach (int currentCustomField in currentTacticCustomFields)
                                //    {
                                //        fltrCustomRestriction = new List<Models.CustomRestriction>();
                                //        fltrCustomRestriction = userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == currentCustomField).ToList();
                                //        //// Get Allowed CustomFieldOptionId list for current selected customField of selected tactic
                                //        if (fltrCustomRestriction != null && fltrCustomRestriction.Any())
                                //        {
                                //            AllowedRightsForCurrentCustomField = new List<string>();
                                //            AllowedRightsForCurrentCustomField = fltrCustomRestriction.Where(restriction =>(restriction.Permission == ViewEditPermission || restriction.Permission == ViewOnlyPermission))
                                //                                                                            .Select(restriction => restriction.CustomFieldOptionId.ToString()).ToList();

                                //            lstRestrictedCustomFieldOptionIds = new List<int>();
                                //            lstRestrictedCustomFieldOptionIds = fltrCustomRestriction.Where(customRestriction => customRestriction.Permission == NonePermission)
                                //                                                                    .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

                                //            //// Check for currentField tactic is viewable or not
                                //            isViewable = currentTacticEntities.Where(entity => entity.CustomFieldId == currentCustomField &&
                                //                                                        (AllowedRightsForCurrentCustomField.Contains(entity.Value) || lstAllCustomFieldOptionIds.Contains(int.Parse(entity.Value)))
                                //                                                        && !lstRestrictedCustomFieldOptionIds.Contains(int.Parse(entity.Value))).Any();
                                //            if (isViewable == false)
                                //            {
                                //                isViewableEntity = false;
                                //                break;
                                //            }
                                //        }
                                //        else if (!isDefaultRestrictionsViewable)
                                //        {
                                //            isViewableEntity = false;
                                //            break;
                                //        }
                                //    }

                                //    //// If tactic is viewable then add it into final Entity list
                                //    if (isViewableEntity)
                                //    {
                                //        lstAllowedEntityIds.Add(tacticId);
                                //    }


                                //}

                            }
                            else
                            {
                                //// Check default custom restrictions is set to viewable
                                if (isDefaultRestrictionsViewable)
                                {
                                    //// set list of viewable tactic Ids
                                    lstAllowedEntityIds = tblCustomFieldEntity.Select(entity => entity.EntityId).Distinct().ToList();
                                }
                            }
                        }
                    }
                    else
                    {
                        return lstTactic;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstAllowedEntityIds;
        }
        #endregion

        #region Check default custom restriction is viewable
        /// <summary>
        /// Function to check default custom restriction is viewable or not
        /// </summary>
        /// <returns></returns>
        public static bool IsDefaultCustomRestrictionsViewable()
        {
            var defaultCustomResriction = ConfigurationManager.AppSettings.Get("DefaultCustomRestriction");
            if (defaultCustomResriction != null)
            {
                if (int.Parse(defaultCustomResriction.ToString()) == (int)Enums.CustomRestrictionPermission.ViewOnly ||
                    int.Parse(defaultCustomResriction.ToString()) == (int)Enums.CustomRestrictionPermission.ViewEdit)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Check default custom restriction is editable or not
        /// <summary>
        /// Function to check default custom restriction is editable or not
        /// </summary>
        /// <returns></returns>
        public static bool IsDefaultCustomRestrictionsEditable()
        {
            var defaultCustomResriction = ConfigurationManager.AppSettings.Get("DefaultCustomRestriction");
            if (defaultCustomResriction != null)
            {
                if (int.Parse(defaultCustomResriction.ToString()) == (int)Enums.CustomRestrictionPermission.ViewEdit)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Get default custom restriction type
        /// <summary>
        /// Function to get default custom restriction type
        /// </summary>
        /// <returns></returns>
        public static Enums.CustomRestrictionPermission GetDefaultCustomRestrictionType()
        {
            var defaultCustomResriction = ConfigurationManager.AppSettings.Get("DefaultCustomRestriction");
            if (defaultCustomResriction != null)
            {
                if (int.Parse(defaultCustomResriction.ToString()) == (int)Enums.CustomRestrictionPermission.ViewEdit)
                {
                    return Enums.CustomRestrictionPermission.ViewEdit;
                }
                else if (int.Parse(defaultCustomResriction.ToString()) == (int)Enums.CustomRestrictionPermission.ViewOnly)
                {
                    return Enums.CustomRestrictionPermission.ViewOnly;
                }
                else if (int.Parse(defaultCustomResriction.ToString()) == (int)Enums.CustomRestrictionPermission.None)
                {
                    return Enums.CustomRestrictionPermission.None;
                }
            }
            return Enums.CustomRestrictionPermission.None;
        }
        #endregion

        #region Get list of Editable entity Ids(tactic)
        /// <summary>
        /// Function to retrieve list of Editable entity ids(tactic)
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="clientId">client id</param>
        /// <returns></returns>
        public static List<int> GetEditableTacticList(Guid userId, Guid clientId, List<int> lstTactic, bool isDisplayForFilter = true, List<CustomField_Entity> customfieldEntitylist = null)
        {
            List<int> lstEditableEntityIds = new List<int>();

            try
            {
                using (MRPEntities objDbMrpEntities = new MRPEntities())
                {

                    bool isCustomFieldExist = Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), clientId);
                    string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                    string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
                    if (isCustomFieldExist)
                    {
                        var customfieldlist = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == clientId && customfield.EntityType.Equals(EntityTypeTactic)
                                                                                    && customfield.IsDeleted.Equals(false)).ToList();
                        var CustomFieldexists = customfieldlist.Where(customfield => (customfield.IsRequired && !isDisplayForFilter)).Any();
                        if (CustomFieldexists)
                        {
                            //    List<CustomField_Entity> tblCustomfieldEntity = objDbMrpEntities.CustomField_Entity.Where(entityid => lstTactic.Contains(entityid.EntityId)).ToList();
                            //var lstAllTacticCustomFieldEntities = tblCustomfieldEntity.Where(customFieldEntity => customFieldEntity.CustomField.ClientId == clientId &&
                            //                                                                            customFieldEntity.CustomField.IsDeleted.Equals(false) &&
                            //                                                                            customFieldEntity.CustomField.EntityType.Equals(EntityTypeTactic) &&
                            //                                                                            customFieldEntity.CustomField.CustomFieldType.Name.Equals(DropDownList) &&
                            //                                                                            (isDisplayForFilter ? customFieldEntity.CustomField.IsDisplayForFilter.Equals(true) : true))
                            //                                                                    .Select(customFieldEntity => customFieldEntity).Distinct().ToList();

                            var customfieldList = customfieldlist.Where(customField => customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                      (isDisplayForFilter ? customField.IsDisplayForFilter.Equals(true) : true)).Select(customField => customField.CustomFieldId).ToList();
                            //  List<CustomField_Entity> tblCustomfieldEntity = objDbMrpEntities.CustomField_Entity.Where(entityid => lstTactic.Contains(entityid.EntityId)).ToList();



                            var lstAllTacticCustomFieldEntitiesanony = customfieldEntitylist != null && customfieldEntitylist.Count() > 0 ? customfieldEntitylist.Where(customFieldEntity => customfieldList.Contains(customFieldEntity.CustomFieldId))
                                                                                                       .Select(customFieldEntity => new { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList()
                                                                                                       : objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customfieldList.Contains(customFieldEntity.CustomFieldId))
                                                                                                       .Select(customFieldEntity => new { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList();
                            var lstAllTacticCustomFieldEntities = (from tbl in lstAllTacticCustomFieldEntitiesanony
                                                                   join lst in lstTactic on tbl.EntityId equals lst
                                                                   select new CustomField_Entity
                                                                   {
                                                                       EntityId = tbl.EntityId,
                                                                       CustomFieldId = tbl.CustomFieldId,
                                                                       Value = tbl.Value
                                                                   }).ToList();

                            //// Get Custom Restrictions
                            var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(userId, true);
                            return GetEditableTacticListPO(userId, clientId, lstTactic, isCustomFieldExist, CustomFieldexists, lstAllTacticCustomFieldEntities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, isDisplayForFilter);
                        }
                        return lstTactic;
                    }
                    return lstTactic;
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstEditableEntityIds;
        }


        public static List<int> GetEditableTacticListPO(Guid userId, Guid clientId, List<int> lstTactic, bool IsCustomFeildExist, bool CustomFieldexists, List<CustomField_Entity> Entities, List<CustomField_Entity> lstAllTacticCustomFieldEntities, List<Models.CustomRestriction> userCustomRestrictionList, bool isDisplayForFilter = true)
        {
            List<int> lstEditableEntityIds = new List<int>();
            List<int> lstEntityIds = new List<int>();
            try
            {

                //using (MRPEntities objDbMrpEntities = new MRPEntities())
                //{
                if (lstTactic.Count() > 0 && IsCustomFeildExist) //todo : able to move up
                {
                    //// Get list customFieldEntity List for given tactic list
                    string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                    string EntityTypeTactic = Enums.EntityType.Tactic.ToString();

                    //Added by Komal Rawal
                    //var CustomFieldexists = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == clientId && customfield.EntityType.Equals(EntityTypeTactic) &&
                    //                                                            (customfield.IsRequired && !isDisplayForFilter) && customfield.IsDeleted.Equals(false)
                    //                                                             ).Any(); //todo : able to move up
                    if (!CustomFieldexists)
                    {
                        return lstTactic;
                    }

                    //For #774
                    //bool Entityid = Entities.Where(entityid => lstTactic.Contains(entityid.EntityId)).Select(entityid => entityid).Any();
                    bool Entityid = (from e in Entities
                                     join t in lstTactic on e.EntityId equals t
                                     select 1).Any();
                    //todo : able to move up

                    if (!Entityid)
                    {
                        return lstTactic;
                    }



                    //End
                    //var lstAllTacticCustomFieldEntities = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customFieldEntity.CustomField.ClientId == clientId &&
                    //                                                                                customFieldEntity.CustomField.IsDeleted.Equals(false) &&
                    //                                                                                customFieldEntity.CustomField.EntityType.Equals(EntityTypeTactic) &&
                    //                                                                                customFieldEntity.CustomField.CustomFieldType.Name.Equals(DropDownList) &&
                    //                                                                                (isDisplayForFilter ? customFieldEntity.CustomField.IsDisplayForFilter.Equals(true) : true) &&
                    //                                                                                lstTactic.Contains(customFieldEntity.EntityId))
                    //                                                                        .Select(customFieldEntity => customFieldEntity).Distinct().ToList(); //todo : able to move up
                    //lstAllTacticCustomFieldEntities = lstAllTacticCustomFieldEntities.Where(customFieldEntity => lstTactic.Contains(customFieldEntity.EntityId)).ToList();
                    lstAllTacticCustomFieldEntities = (from c in lstAllTacticCustomFieldEntities
                                                       join t in lstTactic on c.EntityId equals t
                                                       select c).ToList();
                    var entityids = lstAllTacticCustomFieldEntities.Select(entity => entity.EntityId).ToList();
                    List<int> othertacticIds = lstTactic.Where(tac => !entityids.Contains(tac)).Select(tac => tac).ToList();
                    if (lstAllTacticCustomFieldEntities.Count > 0)
                    {
                        //// Get Custom Restrictions
                        //var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(userId, true);//todo : able to move up

                        //// Check default custom restriction is editable or not
                        bool isDefaultRestrictionsEditable = IsDefaultCustomRestrictionsEditable();
                        if (userCustomRestrictionList.Count() > 0)
                        {

                            //var df = from lst in lstAllTacticCustomFieldEntities
                            //         join uc in userCustomRestrictionList on int.Parse(lst.Value) equals uc.CustomFieldOptionId
                            //         where 
                            // int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                            int NonePermission = (int)Enums.CustomRestrictionPermission.None;
                            var viewnoneoptionid = userCustomRestrictionList.Where(restriction => restriction.Permission == ViewOnlyPermission || restriction.Permission == NonePermission).Select(res => res.CustomFieldOptionId).ToList();
                            //var onlyviewnonetacticids = lstAllTacticCustomFieldEntities.Where(tac => viewnoneoptionid.Contains(int.Parse(tac.Value))).Select(tac => tac.EntityId).ToList();
                            var onlyviewnonetacticids = (from c in lstAllTacticCustomFieldEntities
                                                         join v in viewnoneoptionid on int.Parse(c.Value) equals v
                                                         select c.EntityId).ToList();
                            //var onlyedittactic = lstAllTacticCustomFieldEntities.Where(tac => !onlyviewnonetacticids.Contains(tac.EntityId)).Select(tac => tac.EntityId).Distinct().ToList();
                            //Added By Manoj & John 
                            var onlyedittactic = (from c in lstAllTacticCustomFieldEntities
                                                  join v in onlyviewnonetacticids on c.EntityId equals v into cv
                                                  from f in cv.DefaultIfEmpty(-1)
                                                  select new { c.EntityId, f }).Where(x => x.f == -1).Select(x => x.EntityId).Distinct().ToList();
                            if (isDefaultRestrictionsEditable)
                            {
                                lstEditableEntityIds = onlyedittactic;
                            }
                            else
                            {
                                var emptyoptionids = userCustomRestrictionList.Select(res => res.CustomFieldOptionId).ToList();
                                var getemptyentityids = lstAllTacticCustomFieldEntities.Where(tac => emptyoptionids.Contains(int.Parse(tac.Value))).Select(tac => tac.EntityId).Distinct().ToList();
                                onlyedittactic = onlyedittactic.Where(tac => !getemptyentityids.Contains(tac)).ToList();
                                lstEditableEntityIds = onlyedittactic;
                            }
                            // Get list of tactic Ids
                            //   List<int> lstTacticIds = lstAllTacticCustomFieldEntities.Select(entity => entity.EntityId).Distinct().ToList();

                            //   bool isEditableEntity = true, IsEditable;
                            //   List<CustomField_Entity> currentTacticEntities;
                            //   List<Models.CustomRestriction> _CustomRestriction;

                            //   foreach (int tacticId in lstTactic)
                            //   {
                            //       isEditableEntity = true;
                            //       currentTacticEntities = new List<CustomField_Entity>();
                            //       //// Get list of CustomFieldEntities for current selected tactic
                            //       currentTacticEntities = lstAllTacticCustomFieldEntities.Where(entity => entity.EntityId == tacticId).ToList();

                            //       foreach (CustomField_Entity entity in currentTacticEntities)
                            //       {
                            //           _CustomRestriction = new List<Models.CustomRestriction>();
                            //           _CustomRestriction = userCustomRestrictionList.Where(option => option.CustomFieldOptionId == int.Parse(entity.Value)).ToList();
                            //           if (_CustomRestriction != null && _CustomRestriction.Any())
                            //           {
                            //               IsEditable = _CustomRestriction.Where(restriction => restriction.Permission == ViewEditPermission).Any();
                            //               if (IsEditable == false)
                            //               {
                            //                   isEditableEntity = false;
                            //                   break;
                            //               }
                            //           }
                            //           else if (!isDefaultRestrictionsEditable)
                            //           {
                            //               isEditableEntity = false;
                            //               break;
                            //           }
                            //       }

                            //       //// If tactic is viewable then add it into final Entity list
                            //       if (isEditableEntity)
                            //       {
                            //           lstEditableEntityIds.Add(tacticId);
                            //       }


                            //   }



                        }
                        else if (isDefaultRestrictionsEditable)
                        {
                            //// Check default custom restrictions is set to editable
                            //// set list of editable tactic Ids
                            lstEditableEntityIds = lstAllTacticCustomFieldEntities.Select(entity => entity.EntityId).Distinct().ToList();
                        }
                    }
                    lstEditableEntityIds = lstEditableEntityIds.Concat(othertacticIds).ToList();
                }
                else
                {
                    return lstTactic;
                }

            }
            //}
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return lstEditableEntityIds;
        }
        /// <summary>
        /// Function to check that custom field exist for particular entity type.
        /// </summary>
        /// <param name="entityType">string entitytype e.g. Tactic,Program</param>
        /// <param name="clientId">Guid client Id</param>
        /// <returns></returns>
        public static bool IsCustomFeildExist(string entityType, Guid clientId)
        {
            using (MRPEntities objDbMrpEntities = new MRPEntities())
            {
                bool isCustomFeildExist = objDbMrpEntities.CustomFields.Where(c => c.IsDeleted.Equals(false) && c.ClientId == clientId).Any();
                return isCustomFeildExist;
            }
        }
        #endregion

        #endregion

        #region "Common Get Tactic By Filter Custom Field"

        public static List<int> GetTacticBYCustomFieldFilter(List<CustomFieldFilter> lstCustomFieldFilter, List<int> tacticIds, List<CustomField_Entity> customfieldEntitylist = null)
        {
            MRPEntities db = new MRPEntities();

            if (lstCustomFieldFilter.Count() > 0)
            {
                List<int> lstCustomFieldIds = new List<int>();
                List<int> lstEntityIds = new List<int>();
                List<string> optionIds = new List<string>();
                lstCustomFieldIds = lstCustomFieldFilter.Select(cust => cust.CustomFieldId).Distinct().ToList();

                List<CustomField_Entity> customfieldentitieslist = new List<CustomField_Entity>();
                customfieldentitieslist = customfieldEntitylist != null && customfieldEntitylist.Count() > 0 ? customfieldEntitylist.Where(entity => lstCustomFieldIds.Contains(entity.CustomFieldId) && tacticIds.Contains(entity.EntityId)).ToList() : db.CustomField_Entity.Where(entity => lstCustomFieldIds.Contains(entity.CustomFieldId) && tacticIds.Contains(entity.EntityId)).ToList();
                bool isListExits = false;
                string optionvalue;
                List<CustomField_Entity> lstEntityData;
                //foreach (var item in lstCustomFieldIds)
                //{
                //    optionvalue = string.Empty;
                //    optionvalue = lstCustomFieldFilter.Where(x => x.CustomFieldId == item).Select(x => x.OptionId).FirstOrDefault();
                //    if (optionvalue != "" && optionvalue != string.Empty)
                //    {
                //        optionIds = lstCustomFieldFilter.Where(x => x.CustomFieldId == item).Select(x => x.OptionId.Split('_').Last()).ToList();// Modified by Nishant Sheth #1863
                //        lstEntityData = new List<CustomField_Entity>();
                //        if (isListExits)
                //        {
                //            lstEntityData = customfieldentitieslist.Where(x => x.CustomFieldId == item &&
                //                          optionIds.Contains(x.Value) && lstEntityIds.Contains(x.EntityId)).ToList();
                //            lstEntityIds = lstEntityData.Select(x => x.EntityId).Distinct().ToList();
                //        }
                //        else
                //        {
                //            lstEntityData = customfieldentitieslist.Where(x => x.CustomFieldId == item &&
                //                          optionIds.Contains(x.Value)).ToList();
                //            lstEntityIds = lstEntityData.Select(x => x.EntityId).Distinct().ToList();
                //            isListExits = true;
                //        }
                //    }
                //}
                var EntityValues = db.CustomField_Entity.Where(tac => tacticIds.Contains(tac.EntityId)).Select(val => new
                {
                    CustomFieldid = val.CustomFieldId,
                    TacticId = val.EntityId
                }).ToList();
                var AllTacticIDs = tacticIds;
                for (int i = 0; i < lstCustomFieldIds.Count; i++)
                {
                    optionvalue = string.Empty;
                    optionvalue = lstCustomFieldFilter.Where(x => x.CustomFieldId == lstCustomFieldIds[i]).Select(x => x.OptionId).FirstOrDefault();
                    if (optionvalue != "" && optionvalue != string.Empty && optionvalue != "null" && optionvalue != null)
                    {
                        optionIds = lstCustomFieldFilter.Where(x => x.CustomFieldId == lstCustomFieldIds[i]).Select(x => x.OptionId.Split('_').Last()).ToList();// Modified by Nishant Sheth #1863
                        lstEntityData = new List<CustomField_Entity>();
                        if (isListExits)
                        {
                            lstEntityData = customfieldentitieslist.Where(x => x.CustomFieldId == lstCustomFieldIds[i] &&
                                          optionIds.Contains(x.Value) && lstEntityIds.Contains(x.EntityId)).ToList();
                            lstEntityIds = lstEntityData.Select(x => x.EntityId).Distinct().ToList();
                        }
                        else
                        {
                            lstEntityData = customfieldentitieslist.Where(x => x.CustomFieldId == lstCustomFieldIds[i] &&
                                          optionIds.Contains(x.Value)).ToList();
                            lstEntityIds = lstEntityData.Select(x => x.EntityId).Distinct().ToList();
                            isListExits = true;
                        }
                    }
                    else if (optionvalue == "null" || optionvalue == null)
                    {
                        var TacticIds = EntityValues.Where(entity => entity.CustomFieldid == lstCustomFieldIds[i]).Select(tac => tac.TacticId).Distinct().ToList();
                        lstEntityIds = AllTacticIDs.Except(TacticIds).ToList();
                        AllTacticIDs = lstEntityIds;
                    }

                }
                tacticIds = tacticIds.Where(tactic => lstEntityIds.Contains(tactic)).ToList();
            }
            return tacticIds;
        }

        #endregion

        #region "Get Actual Cost and Tactic mapping list"
        /// <summary>
        /// Created By : Viral Kadiya
        /// Created Date : 27/02/2014
        /// Desciption : Calculate the ActualCost of the tactic list
        /// </summary>
        /// <param name="PlanTacticId"></param>
        /// <returns>Actual cost of a Tactic</returns>
        public static List<TacticActualCostModel> CalculateActualCostTacticslist(List<int> PlanTacticIds, List<TacticStageValue> Tacticdata, string timeframe = "")
        {
            Dictionary<int, string> dicTactic_ActualCost = new Dictionary<int, string>();
            List<int> lstLineItems = new List<int>();
            //List<Plan_Campaign_Program_Tactic_LineItem_Actual> lstLineItemActuals = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            string[] ListYear = timeframe.Split(',');

            List<TacticActualCostModel> TacticActualCostList = new List<TacticActualCostModel>();
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    List<Plan_Campaign_Program_Tactic_LineItem> tblLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(li => PlanTacticIds.Contains(li.PlanTacticId) && li.IsDeleted.Equals(false)).ToList();
                    var lineitemids = tblLineItems.Select(line => line.PlanLineItemId).ToList();
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lia => lineitemids.Contains(lia.PlanLineItemId)).ToList();

                    TacticActualCostModel objTacticActualCost;
                    List<BudgetedValue> lstActulalValue;
                    foreach (int keyTactic in PlanTacticIds)
                    {
                        lstActulalValue = new List<BudgetedValue>();
                        objTacticActualCost = new TacticActualCostModel();
                        objTacticActualCost.PlanTacticId = keyTactic;
                        lstLineItems = new List<int>();
                        lstLineItems = tblLineItems.Where(line => line.PlanTacticId.Equals(keyTactic)).Select(li => li.PlanLineItemId).ToList();
                        if (lstLineItems.Count > 0)
                        {
                            //lstLineItemActuals = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                            var lstLineItemActuals = tblLineItemActuals.Where(lia => lstLineItems.Contains(lia.PlanLineItemId)).ToList()
                                .Select(tac => new
                                {
                                    Period = Convert.ToInt32(tac.Period.Replace("Y", "")),
                                    TacticId = tac.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId,
                                    Value = tac.Value,
                                    StartYear = tac.Plan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.StartDate.Year
                                }).ToList().Select(tac => new
                                {
                                    Period = tac.Period,
                                    NumPeriod = (tac.Period / 13),
                                    TacticId = tac.TacticId,
                                    Value = tac.Value,
                                    StartYear = tac.StartYear
                                }).ToList().Select(tact => new
                                {
                                    Period = "Y" + (tact.Period > 12 ? ((tact.Period + 1) - (13 * tact.NumPeriod)) : (tact.Period) - (13 * tact.NumPeriod)),
                                    Year = tact.StartYear + tact.NumPeriod,
                                    TacticId = tact.TacticId,
                                    Value = tact.Value
                                }).Where(tac => ListYear.Contains(Convert.ToString(tac.Year))).ToList();
                            if (lstLineItemActuals.Any())
                            {
                                lstActulalValue = lstLineItemActuals.Select(actual => new BudgetedValue { Period = actual.Period, Value = actual.Value, Year = actual.Year }).ToList();
                            }

                        }
                        // Commented By Nishant Sheth
                        // Desc :: Match overview's finance actual value with finace tab #1541
                        //else
                        //{
                        //    // Modified By Nishant Sheth
                        //    // Desc :: #2052 To Resolve Actual value show 0
                        //    var lstPlanTacticsActualsData = Tacticdata.Where(pta => pta.TacticObj.PlanTacticId.Equals(keyTactic)).Select(pta => pta.ActualTacticList).FirstOrDefault();
                        //    List<BudgetedValue> lstPlanTacticsActuals = new List<BudgetedValue>();
                        //    if (lstPlanTacticsActualsData != null)
                        //    {
                        //        lstPlanTacticsActuals = lstPlanTacticsActualsData.ToList().Select(tac => new
                        //        {
                        //            Period = Convert.ToInt32(tac.Period.Replace("Y", "")),
                        //            TacticId = tac.PlanTacticId,
                        //            Value = tac.Actualvalue,
                        //            StartYear = tac.Plan_Campaign_Program_Tactic.StartDate.Year
                        //        }).ToList().Select(tac => new
                        //        {
                        //            Period = tac.Period,
                        //            NumPeriod = (tac.Period / 13),
                        //            TacticId = tac.TacticId,
                        //            Value = tac.Value,
                        //            StartYear = tac.StartYear
                        //        }).ToList().Select(tact => new BudgetedValue
                        //        {
                        //            Period = "Y" + (tact.Period > 12 ? ((tact.Period + 1) - (13 * tact.NumPeriod)) : (tact.Period) - (13 * tact.NumPeriod)),
                        //            Year = tact.StartYear + tact.NumPeriod,
                        //            //TacticId = tact.TacticId,
                        //            Value = tact.Value
                        //        }).Where(tac => ListYear.Contains(Convert.ToString(tac.Year.ToString()))).ToList();
                        //    }

                        //    lstActulalValue = new List<BudgetedValue>();
                        //    if (lstPlanTacticsActuals != null)
                        //    {
                        //        lstActulalValue = lstPlanTacticsActuals.ToList();
                        //    }
                        //}
                        objTacticActualCost.ActualList = lstActulalValue;
                        TacticActualCostList.Add(objTacticActualCost);
                    }
                }
                return TacticActualCostList;
            }
            catch
            {
                return TacticActualCostList;
            }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Desc :: Get Planned cost list #1541
        /// </summary>
        /// <param name="PlanTacticIds"></param>
        /// <returns></returns>
        public static List<MultiYearModel> CalculatePlannedCostTacticslist(List<int> PlanTacticIds)
        {

            List<MultiYearModel> MultiYearList = new List<MultiYearModel>();
            List<String> ApprovedTacticStatus = Common.GetStatusListAfterApproved();
            string PeriodPrefix = "Y";
            using (MRPEntities db = new MRPEntities())
            {
                int TacticLength = PlanTacticIds.Count;
                for (int i = 0; i < TacticLength; i++)
                {
                    int TacticId = PlanTacticIds[i];
                    var listlineitems = db.Plan_Campaign_Program_Tactic_LineItem.Where(ln => ln.PlanTacticId.Equals(TacticId) && !ln.IsDeleted && ln.LineItemTypeId != null).Select(ln => ln).ToList();
                    List<int> lineitemIds = listlineitems.Select(ln => ln.PlanLineItemId).ToList();
                    var tacCostData = db.Plan_Campaign_Program_Tactic_Cost.Where(Cost => Cost.PlanTacticId.Equals(TacticId)).Select(cost => cost).ToList();
                    var LineCostListData = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(Cost => lineitemIds.Contains(Cost.PlanLineItemId)).ToList();
                    if (lineitemIds.Count > 0)
                    {

                        if (LineCostListData.Count > 0)
                        {
                            var LineItemCostData = LineCostListData.Select(tac => new
                            {
                                Period = Convert.ToInt32(tac.Period.Replace("Y", "")),
                                LineItemId = tac.PlanLineItemId,
                                Value = tac.Value,
                                StartYear = tac.Plan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.StartDate.Year
                            }).ToList().Select(tac => new
                            {
                                Period = tac.Period,
                                NumPeriod = (tac.Period / 13),
                                LineItemId = tac.LineItemId,
                                Value = tac.Value,
                                StartYear = tac.StartYear
                            }).ToList().Select(tact => new MultiYearModel
                            {
                                Period = PeriodPrefix + Common.ReportMultiyearMonth(tact.Period, tact.NumPeriod),
                                Year = tact.StartYear + tact.NumPeriod,
                                EntityId = tact.LineItemId,
                                Value = tact.Value
                            }).ToList();
                            MultiYearList.AddRange(LineItemCostData);
                        }
                    }

                    var LineItemCostSum = LineCostListData.Select(a => a.Value).Sum();
                    var OtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(ln => ln.PlanTacticId.Equals(TacticId) && ln.LineItemTypeId == null && ln.IsDeleted.Equals(false)).FirstOrDefault();
                    if (OtherLineItem != null)
                    {
                        foreach (var taccost in tacCostData)
                        {
                            var lineDataList = LineCostListData.Where(a => a.Period == taccost.Period).ToList();
                            var lineCostSum = lineDataList.Select(a => a.Value).Sum();
                            if (taccost.Value >= lineCostSum)
                            {
                                MultiYearList.Add(new MultiYearModel
                                {
                                    EntityId = OtherLineItem.PlanTacticId,
                                    Period = taccost.Period,
                                    Value = taccost.Value - lineCostSum,
                                    Year = OtherLineItem.Plan_Campaign_Program_Tactic.StartDate.Year
                                });
                            }
                        }
                    }
                }

            }
            return MultiYearList;
        }
        #endregion

        #region "Get Finance Header Value"
        public static FinanceModelHeaders GetFinanceHeaderValue(int budgetId = 0, string timeFrameOption = "", string isQuarterly = "Quarterly", bool isMain = false)
        {
            //FinanceModelHeaders objfinanceheader = new FinanceModelHeaders();
            FinanceModelHeaders objfinanceheader = new FinanceModelHeaders();
            //List<Plan_Campaign_Program_Tactic_LineItem_Actual> actualCostAllocationData = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            //List<Plan_Campaign_Program_Tactic_LineItem_Cost> plannedCostAllocationData = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
            //List<string> lstMonthly = Common.lstMonthly;
            //MRPEntities db = new MRPEntities();
            //var budgeparentids = db.Budgets.Where(m => m.ClientId == Sessions.User.ClientId && m.IsDeleted == false).Select(m => m.Id).ToList();
            //var MainBudgetid = isMain == true ? db.Budget_Detail.Where(a => a.BudgetId == budgetId && budgeparentids.Contains(a.BudgetId) && a.ParentId == null).Select(a => a.Id).FirstOrDefault() : 0;
            //var varBudgetDetails = db.Budget_Detail.Where(a => a.Id == (isMain == true ? MainBudgetid : (budgetId > 0 ? budgetId : a.BudgetId)) && budgeparentids.Contains(a.BudgetId)).Select(a => a.Id).ToList();

            ////var budgetdetailid = varBudgetDetails.Where(i => i. == budgetId).Select(o => o.Id).ToList();
            //var budgetamout = db.Budget_DetailAmount.Where(amt => varBudgetDetails.Contains(amt.BudgetDetailId)).ToList();

            //var budgetlineit = db.LineItem_Budget.Where(itemid => varBudgetDetails.Contains(itemid.BudgetDetailId)).Select(i => i.PlanLineItemId).ToList();
            //var planlineitemid = db.Plan_Campaign_Program_Tactic_LineItem.Where(i => budgetlineit.Contains(i.PlanLineItemId)).Select(l => l.PlanLineItemId).ToList();

            //var actualLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => planlineitemid.Contains(al.PlanLineItemId)).ToList();

            ////bool IsQuarterly = true;
            ////if (!string.IsNullOrEmpty(isQuarterly) && isQuarterly.Equals(Enums.ViewByAllocated.Monthly.ToString()))
            ////    IsQuarterly = false;

            ////if (IsQuarterly)
            ////{
            ////    List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            ////    List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            ////    List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            ////    List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            ////}
            ////double actulaTotal= actualLineItem.Sum(tot => tot.Value);


            //actualCostAllocationData = lstMonthly.Select(m => new Plan_Campaign_Program_Tactic_LineItem_Actual
            //{
            //    Period = m,
            //    Value = actualLineItem.Where(al => al.Period == m).Sum(al => al.Value)
            //}).ToList();

            //var planneditem = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(al => planlineitemid.Contains(al.PlanLineItemId)).ToList();

            ////double plantotal = planneditem.Sum(tot => tot.Value);

            //plannedCostAllocationData = lstMonthly.Select(m => new Plan_Campaign_Program_Tactic_LineItem_Cost
            //{
            //    Period = m,
            //    Value = planneditem.Where(al => al.Period == m).Sum(al => al.Value)
            //}).ToList();

            //objfinanceheader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            //objfinanceheader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            //objfinanceheader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            //objfinanceheader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            //objfinanceheader.Budget = Convert.ToDouble(budgetamout.Sum(t => t.Budget));
            ////objfinanceheader.Actual = actualCostAllocationData.Sum(s => s.Value);
            //objfinanceheader.Actual = actualLineItem.Sum(a => a.Value);
            //objfinanceheader.Forecast = Convert.ToDouble(budgetamout.Sum(t => t.Forecast));
            ////objfinanceheader.Planned = plannedCostAllocationData.Sum(t => t.Value);
            //objfinanceheader.Planned = planneditem.Sum(a => a.Value);
            return objfinanceheader;
        }

        public static FinanceModelHeaders CommonGetFinanceHeaderValue(FinanceModelHeaders objFinaceHeaderValue)
        {
            FinanceModelHeaders objfinanceheader = new FinanceModelHeaders();

            objfinanceheader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            objfinanceheader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objfinanceheader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objfinanceheader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            if (objFinaceHeaderValue != null)
            {
                objfinanceheader.Budget = objFinaceHeaderValue.Budget;
                objfinanceheader.Actual = objFinaceHeaderValue.Actual;
                objfinanceheader.Forecast = objFinaceHeaderValue.Forecast;
                objfinanceheader.Planned = objFinaceHeaderValue.Planned;
            }
            return objfinanceheader;
        }
        #endregion

        #region "Finance: Get Parent & Child dropdown list for LineItem page "
        /// <summary>
        /// Get the list of Parent dropdown for LineItem screen
        /// </summary>
        /// <returns>Return the list of Budget Details list</returns>
        public static LineItemDropdownModel GetParentLineItemBudgetDetailslist(int BudgetDetailId = 0)
        {
            MRPEntities db = new MRPEntities();

            List<Budget_Detail> tblBudgetDetails = new List<Budget_Detail>();
            tblBudgetDetails = db.Budget_Detail.Where(a => a.Budget.ClientId == Sessions.User.ClientId && a.IsDeleted == false).ToList();
            List<ViewByModel> lstParentItems = new List<ViewByModel>();
            LineItemDropdownModel objParentListModel = new LineItemDropdownModel();
            int? ParentId = 0, mostParentId = 0;
            ParentId = tblBudgetDetails.Where(dtl => dtl.Id == BudgetDetailId).Select(dtl => dtl.ParentId).FirstOrDefault();
            mostParentId = tblBudgetDetails.Where(dtl => dtl.Id == ParentId).Select(dtl => dtl.ParentId).FirstOrDefault();
            var filterParentList = (from detail1 in tblBudgetDetails
                                    where detail1.ParentId == mostParentId && detail1.IsDeleted == false && !string.IsNullOrEmpty(detail1.Name)
                                    select new { detail1.Name, detail1.Id }).Distinct().ToList();
            lstParentItems = filterParentList.Select(budget => new ViewByModel { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();

            objParentListModel.list = lstParentItems;
            objParentListModel.parentId = ParentId.HasValue ? ParentId.Value : 0;
            return objParentListModel;
        }

        /// <summary>
        /// Get the list of Child dropdown for LineItem screen
        /// </summary>
        /// <returns>Return the list of Budget Details list</returns>
        public static List<ViewByModel> GetChildLineItemBudgetDetailslist(int ParentBudgetDetailId = 0)
        {
            MRPEntities db = new MRPEntities();
            List<ViewByModel> lstChildItems = new List<ViewByModel>();
            var filterChildList = (from detail1 in db.Budget_Detail
                                   where detail1.ParentId == ParentBudgetDetailId && detail1.IsDeleted == false && !string.IsNullOrEmpty(detail1.Name) && detail1.Budget.ClientId == Sessions.User.ClientId
                                   select new { detail1.Name, detail1.Id }).Distinct().ToList();
            lstChildItems = filterChildList.Select(budget => new ViewByModel { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return lstChildItems;
        }
        #endregion

        /// <summary>
        /// Added By: Nishant Sheth.
        /// Function to get plan report start and end date as per timeframe list.
        /// </summary>
        /// <param name="timeframe">time frame.</param>

        public static void GetReportStartEndDate(string timeframe, ref DateTime startDate1, ref DateTime endDate1, ref DateTime startDate2, ref DateTime endDate2)
        {
            int year;
            string[] ListYears = timeframe.Split(',');
            bool isNumeric = int.TryParse(ListYears[0], out year);

            startDate1 = new DateTime(Convert.ToInt32(DateTime.Now.Year), 1, 1);
            endDate1 = new DateTime(Convert.ToInt32(DateTime.Now.Year), 12, 31);

            startDate2 = startDate1;
            endDate2 = endDate1;
            if (ListYears.Length > 0 && isNumeric)
            {
                startDate1 = new DateTime(Convert.ToInt32(ListYears[0]), 1, 1);
                endDate1 = new DateTime(Convert.ToInt32(ListYears[0]), 12, 31);
                if (ListYears.Length <= 1)
                {
                    startDate2 = startDate1;
                    endDate2 = endDate1;
                }
                if (ListYears.Length > 1)
                {
                    startDate2 = new DateTime(Convert.ToInt32(ListYears[ListYears.Length - 1]), 1, 1);
                    endDate2 = new DateTime(Convert.ToInt32(ListYears[ListYears.Length - 1]), 12, 31);
                }
            }

        }
        /// <summary>
        /// Added By: Nishant Sheth.
        /// Function to Get list of selcted Years
        /// </summary>
        /// <param name="timeframe">time frame.</param>

        public static void GetselectedYearList(string timeframe, ref string[] selectedYearList)
        {
            selectedYearList = timeframe.Split(',');
        }
        // Add By Nishant Sheth #1915
        public static List<Plan_UserSavedViews> PlanUserSavedViews { get; set; }

        #region GetTimeFrame For Report
        // Add By Nishant Sheth
        // Desc :: To get the time frame option for selected plan ticket #1957
        public static string GetTimeFrameOption(string options, List<Plan> Plan)
        {
            MRPEntities objDbMrpEntities = new MRPEntities();
            if (string.IsNullOrEmpty(options))
            {
                options = System.DateTime.Now.Year.ToString();
            }
            if (Plan == null || !(Plan.Count > 0))
            {
                Plan = objDbMrpEntities.Plans.Where(plan => Sessions.ReportPlanIds.Contains(plan.PlanId)).Select(plan => plan).ToList();// Get selectred plan's list of plan year 
            }
            string[] ListYear = options.Split(',');

            var ListPlanYear = Plan.Select(plan => plan.Year).Distinct().ToList();// Get selectred plan's list of plan year 
            var ListCampYear = objDbMrpEntities.Plan_Campaign.Where(camp => Sessions.ReportPlanIds.Contains(camp.PlanId)).Select(camp => camp.EndDate.Year).Distinct().ToList(); // Get selected plan's max campaign date
            string timeframeOption = string.Empty;
            List<string> NewListYear = new List<string>();
            foreach (var Years in ListYear)
            {
                if (ListPlanYear.Contains(Years) && !NewListYear.Contains(Years))
                {
                    NewListYear.Add(Years);
                }

                if (ListCampYear.Contains(Convert.ToInt32(Years)) && !NewListYear.Contains(Years))
                {
                    NewListYear.Add(Years);
                }
            }
            timeframeOption = string.Join(",", NewListYear);
            return timeframeOption;
        }


        #endregion

        #region Get Report Month for multi year
        // Add By Nishant Sheth
        // Desc :: #2047 To avoid terneary operator as per code review
        public static int ReportMultiyearMonth(int Period, int NumPeriod)
        {
            int returnvalue = 0;
            if (Period > 12)
            {
                returnvalue = ((Period + 1) - (13 * NumPeriod));
            }
            else
            {
                returnvalue = ((Period) - (13 * NumPeriod));
            }
            return returnvalue;
        }
        #endregion

        #region Get Custom model

        public static Plan_Campaign_Program_Tactic GetTacticFromCustomTacticModel(Custom_Plan_Campaign_Program_Tactic CustomTactic)
        {
            Plan_Campaign_Program_Tactic tacRecord = new Plan_Campaign_Program_Tactic();
            tacRecord.Cost = CustomTactic.Cost;
            tacRecord.CreatedBy = CustomTactic.CreatedBy;
            tacRecord.CreatedDate = CustomTactic.CreatedDate;
            tacRecord.Description = CustomTactic.Description;
            tacRecord.EndDate = CustomTactic.EndDate;
            tacRecord.IntegrationInstanceEloquaId = CustomTactic.IntegrationInstanceEloquaId;
            tacRecord.IntegrationInstanceTacticId = CustomTactic.IntegrationInstanceTacticId;
            tacRecord.IntegrationWorkFrontProjectID = CustomTactic.IntegrationWorkFrontProjectID;
            tacRecord.IsDeleted = CustomTactic.IsDeleted;
            tacRecord.IsDeployedToIntegration = CustomTactic.IsDeployedToIntegration;
            tacRecord.IsSyncEloqua = CustomTactic.IsSyncEloqua;
            tacRecord.IsSyncSalesForce = CustomTactic.IsSyncSalesForce;
            tacRecord.IsSyncWorkFront = CustomTactic.IsSyncWorkFront;
            tacRecord.LastSyncDate = CustomTactic.LastSyncDate;
            tacRecord.LinkedPlanId = CustomTactic.LinkedPlanId;
            tacRecord.LinkedTacticId = CustomTactic.LinkedTacticId;
            tacRecord.ModifiedBy = CustomTactic.ModifiedBy;
            tacRecord.ModifiedDate = CustomTactic.ModifiedDate;
            tacRecord.PlanProgramId = CustomTactic.PlanProgramId;
            tacRecord.PlanTacticId = CustomTactic.PlanTacticId;
            tacRecord.ProjectedStageValue = CustomTactic.ProjectedStageValue;
            tacRecord.StageId = CustomTactic.StageId;
            tacRecord.StartDate = CustomTactic.StartDate;
            tacRecord.Status = CustomTactic.Status;
            tacRecord.TacticBudget = CustomTactic.TacticBudget;
            tacRecord.TacticCustomName = CustomTactic.TacticCustomName;
            tacRecord.TacticTypeId = CustomTactic.TacticTypeId;
            tacRecord.Title = CustomTactic.Title;
            return tacRecord;
        }

        public static List<Plan_Campaign_Program_Tactic> GetTacticFromCustomTacticList(List<Custom_Plan_Campaign_Program_Tactic> CustomTacticList)
        {
            List<Plan_Campaign_Program_Tactic> tacList = new List<Plan_Campaign_Program_Tactic>();
            tacList = CustomTacticList.Select(CustomTactic => new Plan_Campaign_Program_Tactic
            {
                Cost = CustomTactic.Cost,
                CreatedBy = CustomTactic.CreatedBy,
                CreatedDate = CustomTactic.CreatedDate,
                Description = CustomTactic.Description,
                EndDate = CustomTactic.EndDate,
                IntegrationInstanceEloquaId = CustomTactic.IntegrationInstanceEloquaId,
                IntegrationInstanceTacticId = CustomTactic.IntegrationInstanceTacticId,
                IntegrationWorkFrontProjectID = CustomTactic.IntegrationWorkFrontProjectID,
                IsDeleted = CustomTactic.IsDeleted,
                IsDeployedToIntegration = CustomTactic.IsDeployedToIntegration,
                IsSyncEloqua = CustomTactic.IsSyncEloqua,
                IsSyncSalesForce = CustomTactic.IsSyncSalesForce,
                IsSyncWorkFront = CustomTactic.IsSyncWorkFront,
                LastSyncDate = CustomTactic.LastSyncDate,
                LinkedPlanId = CustomTactic.LinkedPlanId,
                LinkedTacticId = CustomTactic.LinkedTacticId,
                ModifiedBy = CustomTactic.ModifiedBy,
                ModifiedDate = CustomTactic.ModifiedDate,
                PlanProgramId = CustomTactic.PlanProgramId,
                PlanTacticId = CustomTactic.PlanTacticId,
                ProjectedStageValue = CustomTactic.ProjectedStageValue,
                StageId = CustomTactic.StageId,
                StartDate = CustomTactic.StartDate,
                Status = CustomTactic.Status,
                TacticBudget = CustomTactic.TacticBudget,
                TacticCustomName = CustomTactic.TacticCustomName,
                TacticTypeId = CustomTactic.TacticTypeId,
                Title = CustomTactic.Title
            }).ToList();
            return tacList;
        }

        public static List<Plan_Campaign_Program> GetProgramFromCustomPreogramList(List<Custom_Plan_Campaign_Program> CustomProgramList)
        {
            List<Plan_Campaign_Program> ProgList = new List<Plan_Campaign_Program>();
            ProgList = CustomProgramList.Select(CustomProgram => new Plan_Campaign_Program
            {
                Abbreviation = CustomProgram.Abbreviation,
                CreatedBy = CustomProgram.CreatedBy,
                CreatedDate = CustomProgram.CreatedDate,
                Description = CustomProgram.Description,
                EndDate = CustomProgram.EndDate,
                IntegrationInstanceProgramId = CustomProgram.IntegrationInstanceProgramId,
                IsDeleted = CustomProgram.IsDeleted,
                IsDeployedToIntegration = CustomProgram.IsDeployedToIntegration,
                LastSyncDate = CustomProgram.LastSyncDate,
                ModifiedBy = CustomProgram.ModifiedBy,
                ModifiedDate = CustomProgram.ModifiedDate,
                PlanCampaignId = CustomProgram.PlanCampaignId,
                PlanProgramId = CustomProgram.PlanProgramId,
                ProgramBudget = CustomProgram.ProgramBudget,
                StartDate = CustomProgram.StartDate,
                Status = CustomProgram.Status,
                Title = CustomProgram.Title
            }).ToList();
            return ProgList;
        }

        public static List<Plan> GetSpPlanList(DataTable dsPlanCampProgTac)
        {
            var PlanList = new List<Plan>();

            if (dsPlanCampProgTac != null && dsPlanCampProgTac.Rows.Count > 0)
            {
                PlanList = dsPlanCampProgTac.AsEnumerable().Select(row => new Plan
                {
                    AllocatedBy = Convert.ToString(row["AllocatedBy"]),
                    Budget = Convert.ToDouble(Convert.ToString(row["Budget"])), // Change
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])), // Change
                    CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])),
                    DependencyDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["DependencyDate"])) ? (DateTime?)null : row["DependencyDate"]),
                    Description = (Convert.ToString(row["Description"])),
                    EloquaFolderPath = (Convert.ToString(row["EloquaFolderPath"])),
                    GoalType = (Convert.ToString(row["GoalType"])),
                    GoalValue = Convert.ToDouble(Convert.ToString(row["GoalValue"])),
                    IsActive = Convert.ToBoolean(Convert.ToString(row["IsActive"])),
                    IsDeleted = Convert.ToBoolean(Convert.ToString(row["IsDeleted"])),
                    ModelId = Convert.ToInt32(Convert.ToString(row["ModelId"])), // Change
                    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                    PlanId = Convert.ToInt32(Convert.ToString(row["PlanId"])), // Change
                    Status = Convert.ToString(row["Status"]),
                    Title = Convert.ToString(row["Title"]),
                    Version = Convert.ToString(row["Version"]),
                    Year = Convert.ToString(row["Year"])
                }).ToList();
            }

            return PlanList;
        }

        public static List<Plan_Campaign> GetSpCampaignList(DataTable dsPlanCampProgTac)
        {
            var lstCampaign = new List<Plan_Campaign>();

            if (dsPlanCampProgTac != null && dsPlanCampProgTac.Rows.Count > 0)
            {
                lstCampaign = dsPlanCampProgTac.AsEnumerable().Select(row => new Plan_Campaign
                {
                    Abbreviation = Convert.ToString(row["Abbreviation"]),
                    CampaignBudget = Convert.ToDouble(Convert.ToString(row["CampaignBudget"])),
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])), // Change
                    CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])), // Change
                    Description = Convert.ToString(Convert.ToString(row["Description"])),
                    EndDate = Convert.ToDateTime(Convert.ToString(row["EndDate"])), // Change
                    IntegrationInstanceCampaignId = Convert.ToString(row["IntegrationInstanceCampaignId"]),
                    IsDeleted = Convert.ToBoolean(Convert.ToString(row["IsDeleted"])), // Change
                    IsDeployedToIntegration = Convert.ToBoolean(Convert.ToString(row["IsDeployedToIntegration"])),
                    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
                    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                    PlanCampaignId = Convert.ToInt32(Convert.ToString(row["PlanCampaignId"])), // Change
                    PlanId = Convert.ToInt32(Convert.ToString(row["PlanId"])), // Change
                    StartDate = Convert.ToDateTime(Convert.ToString(row["StartDate"])), // Change
                    Status = Convert.ToString(row["Status"]),
                    Title = Convert.ToString(row["Title"])
                }).ToList();
            }

            return lstCampaign;
        }

        public static List<Plan_Campaign_Program> GetSpProgramList(DataTable dsPlanCampProgTac)
        {
            var programList = new List<Plan_Campaign_Program>();

            if (dsPlanCampProgTac != null && dsPlanCampProgTac.Rows.Count > 0)
            {
                programList = dsPlanCampProgTac.AsEnumerable().Select(row => new Plan_Campaign_Program
                {
                    Abbreviation = Convert.ToString(row["Abbreviation"]),
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])), // Change
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    Description = Convert.ToString(row["Description"]),
                    EndDate = Convert.ToDateTime(row["EndDate"]),
                    IntegrationInstanceProgramId = Convert.ToString(row["IntegrationInstanceProgramId"]),
                    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
                    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
                    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                    PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                    PlanProgramId = Convert.ToInt32(row["PlanProgramId"]),
                    ProgramBudget = Convert.ToDouble(row["ProgramBudget"]),
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    Status = Convert.ToString(row["Status"]),
                    Title = Convert.ToString(row["Title"])
                }).ToList();
            }

            return programList;
        }

        public static List<Custom_Plan_Campaign_Program> GetSpCustomProgramList(DataTable dsPlanCampProgTac)
        {
            var lstProgramPer = new List<Custom_Plan_Campaign_Program>();

            if (dsPlanCampProgTac != null && dsPlanCampProgTac.Rows.Count > 0)
            {
                lstProgramPer = dsPlanCampProgTac.AsEnumerable().Select(row => new Custom_Plan_Campaign_Program
                {
                    Abbreviation = Convert.ToString(row["Abbreviation"]),
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    Description = Convert.ToString(row["Description"]),
                    EndDate = Convert.ToDateTime(row["EndDate"]),
                    IntegrationInstanceProgramId = Convert.ToString(row["IntegrationInstanceProgramId"]),
                    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
                    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
                    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                    PlanCampaignId = Convert.ToInt32(row["PlanCampaignId"]),
                    PlanProgramId = Convert.ToInt32(row["PlanProgramId"]),
                    ProgramBudget = Convert.ToDouble(row["ProgramBudget"]),
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    Status = Convert.ToString(row["Status"]),
                    Title = Convert.ToString(row["Title"]),
                    PlanId = Convert.ToInt32(row["PlanId"])
                }).ToList();
            }

            return lstProgramPer;
        }

        public static List<Custom_Plan_Campaign_Program_Tactic> GetSpCustomTacticList(DataTable dsPlanCampProgTac)
        {
            var customtacticList = new List<Custom_Plan_Campaign_Program_Tactic>();

            if (dsPlanCampProgTac != null && dsPlanCampProgTac.Rows.Count > 0)
            {
                customtacticList = dsPlanCampProgTac.AsEnumerable().Select(row => new Custom_Plan_Campaign_Program_Tactic
                {
                    Cost = Convert.ToDouble(row["Cost"]),
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Convert.ToString(Guid.Empty) : Convert.ToString(row["CreatedBy"])),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    Description = Convert.ToString(row["Description"]),
                    EndDate = Convert.ToDateTime(row["EndDate"]),
                    IntegrationInstanceEloquaId = Convert.ToString(row["IntegrationInstanceEloquaId"]),
                    IntegrationInstanceTacticId = Convert.ToString(row["IntegrationInstanceEloquaId"]),
                    IntegrationWorkFrontProjectID = Convert.ToString(row["IntegrationWorkFrontProjectID"]),
                    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                    IsDeployedToIntegration = Convert.ToBoolean(row["IsDeployedToIntegration"]),
                    IsSyncEloqua = Convert.ToBoolean(string.IsNullOrEmpty(Convert.ToString(row["IsSyncEloqua"])) ? (bool?)null : row["IsSyncEloqua"]),
                    IsSyncSalesForce = Convert.ToBoolean(string.IsNullOrEmpty(Convert.ToString(row["IsSyncSalesForce"])) ? (bool?)null : row["IsSyncSalesForce"]),
                    IsSyncWorkFront = Convert.ToBoolean(string.IsNullOrEmpty(Convert.ToString(row["IsSyncWorkFront"])) ? (bool?)null : row["IsSyncWorkFront"]),
                    LastSyncDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["LastSyncDate"])) ? (DateTime?)null : row["LastSyncDate"]),
                    LinkedPlanId = string.IsNullOrEmpty(Convert.ToString(row["LinkedPlanId"])) ? (int?)null : int.Parse(Convert.ToString(row["LinkedPlanId"])),
                    LinkedTacticId = string.IsNullOrEmpty(Convert.ToString(row["LinkedTacticId"])) ? (int?)null : int.Parse(Convert.ToString(row["LinkedTacticId"])),
                    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Convert.ToString(Guid.Empty) : Convert.ToString(row["ModifiedBy"])),
                    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                    PlanProgramId = Convert.ToInt32(Convert.ToString(row["PlanProgramId"])),
                    PlanTacticId = Convert.ToInt32(Convert.ToString(row["PlanTacticId"])),
                    ProjectedStageValue = string.IsNullOrEmpty(Convert.ToString(row["ProjectedStageValue"])) ? (double?)null : double.Parse(Convert.ToString(row["ProjectedStageValue"])),
                    StageId = Convert.ToInt32(Convert.ToString(row["StageId"])),
                    StartDate = Convert.ToDateTime(Convert.ToString(row["StartDate"])),
                    Status = Convert.ToString(row["Status"]),
                    TacticBudget = Convert.ToDouble(row["TacticBudget"]),
                    TacticCustomName = Convert.ToString(row["TacticCustomName"]),
                    TacticTypeId = Convert.ToInt32(Convert.ToString(row["TacticTypeId"])),
                    Title = Convert.ToString(row["Title"]),
                    PlanId = Convert.ToInt32(Convert.ToString(row["PlanId"])),
                    PlanCampaignId = Convert.ToInt32(Convert.ToString(row["PlanCampaignId"])),
                    TacticTypeTtile = Convert.ToString(row["TacticTypeTtile"]),
                    ColorCode = Convert.ToString(row["ColorCode"]),
                    PlanYear = Convert.ToString(row["PlanYear"]),
                    ModelId = Convert.ToInt32(Convert.ToString(row["ModelId"])),
                    CampaignTitle = Convert.ToString(row["CampaignTitle"]),
                    ProgramTitle = Convert.ToString(row["ProgramTitle"]),
                    PlanTitle = Convert.ToString(row["PlanTitle"]),
                    StageTitle = Convert.ToString(row["StageTitle"]),
                    // Added by Arpita Soni for Ticket #2357 on 07/14/2016
                    AnchorTacticId = row["AnchorTacticId"] == DBNull.Value ? 0 : Convert.ToInt32(row["AnchorTacticId"]),
                    PackageTitle = row["PackageTitle"] == DBNull.Value ? "" : Convert.ToString(row["PackageTitle"]),
                    AssetType = Convert.ToString(row["AssetType"])  //Added by Komal Rawal for #2358 show all tactics in package even if they are not filtered
                }).ToList();
            }

            return customtacticList;
        }

        public static List<CustomField> GetSpCustomFieldList(DataTable dsCustomfield)
        {
            var CustomFieldList = new List<CustomField>();

            if (dsCustomfield != null && dsCustomfield.Rows.Count > 0)
            {
                CustomFieldList = dsCustomfield.AsEnumerable().Select(row => new CustomField
                {
                    AbbreviationForMulti = Convert.ToString(row["AbbreviationForMulti"]),
                    ClientId = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ClientId"])) ? Guid.Empty.ToString() : Convert.ToString(row["ClientId"])),
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    CustomFieldId = Convert.ToInt32(row["CustomFieldId"]),
                    CustomFieldTypeId = Convert.ToInt32(row["CustomFieldTypeId"]),
                    Description = Convert.ToString(row["Description"]),
                    EntityType = Convert.ToString(row["EntityType"]),
                    IsDefault = Convert.ToBoolean(row["IsDefault"]),
                    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                    IsDisplayForFilter = Convert.ToBoolean(row["IsDisplayForFilter"]),
                    IsGet = Convert.ToBoolean(row["IsGet"]),
                    IsRequired = Convert.ToBoolean(row["IsRequired"]),
                    ModifiedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["ModifiedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["ModifiedBy"])),
                    ModifiedDate = Convert.ToDateTime(string.IsNullOrEmpty(Convert.ToString(row["ModifiedDate"])) ? (DateTime?)null : row["ModifiedDate"]),
                    Name = Convert.ToString(row["Name"])
                }).ToList();
            }

            return CustomFieldList;
        }

        public static List<CacheCustomField> GetSpCustomFieldEntityList(DataTable dsCustomfieldEntity)
        {
            var CustomFieldEntityList = new List<CacheCustomField>();

            if (dsCustomfieldEntity != null && dsCustomfieldEntity.Rows.Count > 0)
            {
                CustomFieldEntityList = dsCustomfieldEntity.AsEnumerable().Select(row => new CacheCustomField
                {
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])),
                    CustomFieldEntityId = Convert.ToInt32(row["CustomFieldEntityId"]),
                    CustomFieldId = Convert.ToInt32(row["CustomFieldId"]),
                    EntityId = Convert.ToInt32(row["EntityId"]),
                    Value = Convert.ToString(row["Value"])
                }).ToList();
            }

            return CustomFieldEntityList;
        }

        public static List<Custom_LineItem_Budget> GetSpLineItemBudgetList(DataTable dt)
        {
            var LineItemBudgetList = new List<Custom_LineItem_Budget>();

            if (dt != null && dt.Rows.Count > 0)
            {
                LineItemBudgetList = dt.AsEnumerable().Select(row => new Custom_LineItem_Budget
                {
                    BudgetDetailId = Convert.ToInt32(row["BudgetDetailId"]),
                    CreatedBy = Guid.Parse(string.IsNullOrEmpty(Convert.ToString(row["CreatedBy"])) ? Guid.Empty.ToString() : Convert.ToString(row["CreatedBy"])),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    Id = Convert.ToInt32(row["Id"]),
                    PlanLineItemId = Convert.ToInt32(row["PlanLineItemId"]),
                    Weightage = string.IsNullOrEmpty(Convert.ToString(row["Weightage"])) ? (byte?)null : byte.Parse(Convert.ToString(row["Weightage"]))
                }).ToList();
            }

            return LineItemBudgetList;
        }
        /// <summary>
        /// Get list of dashboard with client specefic
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="DashboardID"></param>
        /// <returns></returns>
        public static List<Custom_Dashboard> GetSpDashboarData(string UserId, int DashboardID = 0)
        {
            var DashboardList = new List<Custom_Dashboard>();
            StoredProcedure objSp = new StoredProcedure();
            DataTable dt = objSp.GetDashboarContentData(UserId, DashboardID).Tables[0];
            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 1)
            {
                DashboardList = dt.AsEnumerable().Select(row => new Custom_Dashboard
                {
                    Id = Convert.ToInt32(row["id"]),
                    Name = Convert.ToString(row["Name"]),
                    DisplayName = Convert.ToString(row["DisplayName"]),
                    DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                    CustomCSS = Convert.ToString(row["CustomCSS"]),
                    Rows = string.IsNullOrEmpty(Convert.ToString(row["Rows"])) ? (int?)null : Convert.ToInt32(row["Rows"]),
                    Columns = string.IsNullOrEmpty(Convert.ToString(row["Columns"])) ? (int?)null : Convert.ToInt32(row["Columns"]),
                    ParentDashboardId = string.IsNullOrEmpty(Convert.ToString(row["ParentDashboardId"])) ? (int?)null : Convert.ToInt32(row["ParentDashboardId"]),
                    IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                    IsComparisonDisplay = string.IsNullOrEmpty(Convert.ToString(row["IsComparisonDisplay"])) ? (bool?)null : Convert.ToBoolean(row["IsComparisonDisplay"]),
                    HelpTextId = string.IsNullOrEmpty(Convert.ToString(row["HelpTextId"])) ? (int?)null : Convert.ToInt32(row["HelpTextId"])
                }).ToList();
            }
            return DashboardList;
        }
        #endregion

        /// <summary>
        /// Added by devanshi for formating the timestemp for Alert & Notification on 16-8-2016
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        #region Method for timestamp formation for Alert & Notifications
        public static string TimeAgo(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;
            if (span.Days > 0 && span.Days != 1)
            {
                return dt.ToString("MMM d yyyy");
            }
            if (span.Days > 0 && span.Days == 1)
                return "Yesterday";
            if (span.Hours > 0)
                return String.Format("{0} {1} ago",
                span.Hours, span.Hours == 1 ? "hour" : "hours");
            if (span.Minutes > 0)
                return String.Format("{0} {1} ago",
                span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            if (span.Seconds > 5)
                return String.Format("{0} seconds ago", span.Seconds);
            if (span.Seconds <= 5)
                return "Just now";
            return string.Empty;
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
            bool first_IsSpecialChar = false;
            bool second_IsSpecialChar = false;
            if (!string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(second))
            {
                first_IsSpecialChar = !Regex.IsMatch(first[0].ToString(), strRegExpPattern, RegexOptions.IgnoreCase);
                second_IsSpecialChar = !Regex.IsMatch(second[0].ToString(), strRegExpPattern, RegexOptions.IgnoreCase);
            }


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
            return secondIsNumber ? 1 : first != null ? first.CompareTo(second) : 0;
            // End
        }


    }
    // Add By Nishant Sheth
    // Desc :: common methods for cache memory
    #region Cache methods
    public class CacheObject
    {
        //Modified By Komal Rawal for #2138 solve caching issue for different browser.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Returncache(string objectName)
        {
            var obj = HttpRuntime.Cache.Get(objectName + "-" + Sessions.User.UserId.ToString() + "-" + HttpContext.Current.Session.Contents.SessionID.ToString());
            if (obj != null)
            {
                return obj;
            }
            else
                return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCache(string objectName, object CacheObject)
        {
            var obj = HttpRuntime.Cache.Get(objectName + "-" + Sessions.User.UserId.ToString() + "-" + HttpContext.Current.Session.Contents.SessionID.ToString());
            if (obj != null)
            {
                HttpRuntime.Cache.Remove(objectName + "-" + Sessions.User.UserId.ToString() + "-" + HttpContext.Current.Session.Contents.SessionID.ToString());
            }
            HttpRuntime.Cache.Insert(objectName + "-" + Sessions.User.UserId.ToString() + "-" + HttpContext.Current.Session.Contents.SessionID.ToString(), CacheObject, null, DateTime.Now.AddHours(3), Cache.NoSlidingExpiration);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllCurrentUserCache()
        {
            if (Sessions.User != null && Sessions.User.UserId != null && HttpContext.Current.Session.Contents.SessionID != null)
            {
                string[] names = Enum.GetNames(typeof(Enums.CacheObject));
                for (int i = 0; i < names.Length; i++)
                {
                    HttpRuntime.Cache.Remove(names[i] + "-" + Sessions.User.UserId.ToString() + "-" + HttpContext.Current.Session.Contents.SessionID.ToString());
                }
            }
        }
    }
    #endregion

    // Add By Nishant Sheth
    // Desc :: common methods for stroed procedures
    #region stored procedures methods
    public class StoredProcedure
    {
        /// <summary>
        /// Add By Nishant Sheth 
        /// Desc:: Import Marketing finance Data from excel 
        /// </summary>
        /// <param name="XMLData"></param>
        /// <param name="ImportBudgetCol"></param>
        /// <returns></returns>
        public int ImportMarketingFinance(XmlDocument XMLData, DataTable ImportBudgetCol, int BudgetDetailId = 0, bool IsMonthly = false)
        {
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;
            int ExecuteCommand = 0;
            try
            {
                if (!IsMonthly)
                {
                    command = new SqlCommand("ImportMarketingBudgetQuarter", Connection);
                }
                else
                {
                    command = new SqlCommand("ImportMarketingBudgetMonthly", Connection);
                }
                using (command)
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", Sessions.User.UserId);
                    command.Parameters.AddWithValue("@ClientId", Sessions.User.ClientId);
                    command.Parameters.AddWithValue("@XMLData", XMLData.InnerXml);
                    command.Parameters.AddWithValue("@ImportBudgetCol", ImportBudgetCol);
                    command.Parameters.AddWithValue("@BudgetDetailId", BudgetDetailId);
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;
                    ExecuteCommand = command.ExecuteNonQuery();
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            return ExecuteCommand;
        }

        // Get List of Line Items
        public List<Plan_Campaign_Program_Tactic_LineItem> GetLineItemList(string planid)
        {
            DataTable datatable = new DataTable();
            MRPEntities db = new MRPEntities();

            //List<SqlParameter> para = new List<SqlParameter>();
            //para.Add(new SqlParameter { ParameterName = "@PlanId", Value = planid });
            SqlParameter[] para = new SqlParameter[1];

            para[0] = new SqlParameter
            {
                ParameterName = "PlanId",
                Value = planid
            };

            var data = db.Database.SqlQuery<RevenuePlanner.Models.Plan_Campaign_Program_Tactic_LineItem>("GetLineItemList @PlanId", para).ToList();


            ///If connection is closed then it will be open
            //    var Connection = db.Database.Connection as SqlConnection;
            //    if (Connection.State == System.Data.ConnectionState.Closed)
            //        Connection.Open();
            //    SqlCommand command = null;

            //    command = new SqlCommand("GetLineItemList", Connection);

            //    using (command)
            //    {

            //        command.CommandType = CommandType.StoredProcedure;
            //        command.Parameters.AddWithValue("@PlanId", planid);
            //        SqlDataAdapter adp = new SqlDataAdapter(command);
            //        command.CommandTimeout = 0;
            //        adp.Fill(datatable);
            //        if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();

            //    }
            return data;
        }

        // Get List of Plan, Campaign, Program, Tactic
        public DataSet GetListPlanCampaignProgramTactic(string planid)
        {
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetListPlanCampaignProgramTactic", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", planid);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.ClientId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dataset;
        }

        // Get Custom field entity
        public DataSet GetCustomFieldEntityList(string EntityType, int? CustomTypeId = null)
        {

            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetCustomFieldEntityList", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CustomTypeId", CustomTypeId);
                command.Parameters.AddWithValue("@EntityType", EntityType);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.ClientId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dataset;
        }

        /// <summary>
        /// Added by Mitesh Vaishnav reg. PL ticket 1646
        /// This function returns datatable which contains details reg. plan, campaign, program, tactic and line item's planned cost and actual 
        /// </summary>
        /// <param name="PlanId">int unique planid of plan which data will be return</param>
        /// <param name="budgetTab">string which contains value like Planned or Actual</param>
        /// <returns></returns>
        public DataTable GetPlannedActualDetail(int PlanId, string budgetTab)
        {
            DataTable dtPlanHirarchy = new DataTable();

            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("Plan_Budget_Cost_Actual_Detail", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanId);
                command.Parameters.AddWithValue("@UserId", Sessions.User.UserId.ToString());
                command.Parameters.AddWithValue("@SelectedTab", budgetTab);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dtPlanHirarchy);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dtPlanHirarchy;
        }

        // Get Tactic line ite,
        public List<Plan_Campaign_Program_Tactic_LineItem> GetTacticLineItemList(string tacticId)
        {

            MRPEntities db = new MRPEntities();

            SqlParameter[] para = new SqlParameter[1];

            para[0] = new SqlParameter
            {
                ParameterName = "tacticId",
                Value = tacticId
            };

            var data = db.Database.SqlQuery<RevenuePlanner.Models.Plan_Campaign_Program_Tactic_LineItem>("GetTacticLineItemList @tacticId", para).ToList();

            return data;
        }

        // Get List of tactic type
        public List<TacticTypeModel> GetTacticTypeList(string lstAllowedEntityIds)
        {
            MRPEntities db = new MRPEntities();
            SqlParameter[] para = new SqlParameter[1];

            para[0] = new SqlParameter
            {
                ParameterName = "TacticIds",
                Value = lstAllowedEntityIds
            };

            var data = db.Database.SqlQuery<RevenuePlanner.Models.TacticTypeModel>("GetTacticTypeList @TacticIds", para).ToList();
            return data;
        }

        // Get list of view by 
        public List<ViewByModel> spViewByDropDownList(string planId)
        {
            MRPEntities db = new MRPEntities();
            List<ViewByModel> viewByListResult = new List<ViewByModel>();
            viewByListResult.Add(new ViewByModel { Text = PlanGanttTypes.Tactic.ToString(), Value = PlanGanttTypes.Tactic.ToString() });
            viewByListResult.Add(new ViewByModel { Text = PlanGanttTypes.Stage.ToString(), Value = PlanGanttTypes.Stage.ToString() });
            viewByListResult.Add(new ViewByModel { Text = PlanGanttTypes.Status.ToString(), Value = PlanGanttTypes.Status.ToString() });
            // Added by Arpita Soni for Ticket #2357 on 07/12/2016
            viewByListResult.Add(new ViewByModel { Text = Enums.DictPlanGanttTypes[PlanGanttTypes.ROIPackage.ToString()].ToString(), Value = Enums.DictPlanGanttTypes[PlanGanttTypes.ROIPackage.ToString()].ToString() });

            SqlParameter[] para = new SqlParameter[2];

            para[0] = new SqlParameter()
            {
                ParameterName = "PlanId",
                Value = string.Join(",", planId)
            };
            para[1] = new SqlParameter()
            {
                ParameterName = "ClientId",
                Value = Sessions.User.ClientId
            };
            var customViewBy = db.Database.SqlQuery<ViewByModel>("spViewByDropDownList @PlanId,@ClientId", para).ToList();
            return viewByListResult = viewByListResult.Concat(customViewBy).ToList();
        }

        public DataSet GetExportCSV(int PlanId, string HoneyCombids = null)
        {
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("ExportToCSV", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanId);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.ClientId);
                command.Parameters.AddWithValue("@HoneyCombids", HoneyCombids);
                command.Parameters.AddWithValue("@CurrencyExchangeRate", Sessions.PlanExchangeRate);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
            return dataset;
        }
        /// <summary>
        /// Add By Nishant Sheth 
        /// Desc:: Get list of budget list and line item budget list
        /// </summary>
        /// <param name="BudgetId"></param>
        /// <returns></returns>
        public DataSet GetBudgetListAndLineItemBudgetList(int BudgetId = 0)
        {
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetBudgetListAndLineItemBudgetList", Connection);

            using (command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ClientId", Sessions.User.ClientId);
                command.Parameters.AddWithValue("@BudgetId", BudgetId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
            return dataset;
        }

        public DataSet GetDashboardContent(int HomepageId = 0, int DashboardId = 0, int DashboardPageId = 0)
        {
            DataSet ds = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetDashboardContent", Connection);

            using (command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@HomepageId", 0);
                command.Parameters.AddWithValue("@DashboardId", DashboardId);
                command.Parameters.AddWithValue("@DashboardPageId", 0);
                command.Parameters.AddWithValue("@UserId", Sessions.User.UserId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(ds);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
            return ds;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Desc :: Get list of dashbaod menu for measure reports
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="DashboardID"></param>
        /// <returns></returns>
        public DataSet GetDashboarContentData(string UserId, int DashboardID = 0)
        {
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetDashboardContentData", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", UserId);
                command.Parameters.AddWithValue("@DashboardID", DashboardID);
                command.CommandTimeout = 0;
                SqlDataAdapter adp = new SqlDataAdapter(command);
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dataset;
        }

        /// <summary>
        /// Added by Rushil Bhuptani on 15/06/2016 for #2227 
        /// Method to updated imported excel data in database.
        /// </summary>
        /// <param name="dtNew">Datatable containing excel data.</param>
        /// <param name="isMonthly">Flag to indicate whether data is monthly or quarterly.</param>
        /// <param name="userId">Id of user.</param>
        /// <returns>Dataset with conflicted ActivityIds.</returns>
        public DataSet GetPlanBudgetList(DataTable dtNew, bool isMonthly, Guid userId)
        {
            try
            {
                DataTable datatable = new DataTable();
                DataSet dataset = new DataSet();
                MRPEntities db = new MRPEntities();
                ///If connection is closed then it will be open
                var Connection = db.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                SqlCommand command = null;
                if (!isMonthly)
                {
                    command = new SqlCommand("Sp_GetPlanBudgetDataQuarterly", Connection);
                }
                else
                {
                    command = new SqlCommand("Sp_GetPlanBudgetDataMonthly", Connection);
                }

                using (command)
                {

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PlanId", Convert.ToInt32(dtNew.Rows[0][0]));
                    command.Parameters.AddWithValue("@ImportData", dtNew);
                    command.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;

                    // Modified by Rushil Bhuptani on 21/06/2016 for ticket #2267 for showing message for conflicting data.
                    adp.Fill(dataset);
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }
                return dataset;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetColumnValue(string Query)
        {
            SqlConnection DbConn = new SqlConnection();
            SqlDataAdapter DbAdapter = new SqlDataAdapter();
            SqlCommand DbCommand = new SqlCommand();
            try
            {
                if (DbConn.State == 0)
                {
                    try
                    {
                        MRPEntities mp = new MRPEntities();
                        DbConn.ConnectionString = mp.Database.Connection.ConnectionString;
                        DbConn.Open();
                    }
                    catch (Exception exp)
                    {
                        throw exp;
                    }
                }
                DbCommand.CommandTimeout = 120;
                DbCommand.Connection = DbConn;
                DbCommand.CommandType = CommandType.Text;
                DbCommand.CommandText = Query;


                object objResult = DbCommand.ExecuteScalar();
                if (objResult == null)
                {
                    return "";
                }
                if (objResult == System.DBNull.Value)
                {
                    return "";
                }
                else
                {
                    return Convert.ToString(objResult);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                DbAdapter.Dispose();
                DbConn.Close();
            }
        }

        public List<CustomDashboardModel> GetCustomDashboardsClientwise(Guid UserId, Guid ClientId)
        {
            DataTable datatable = new DataTable();
            MRPEntities db = new MRPEntities();

            SqlParameter[] para = new SqlParameter[2];

            para[0] = new SqlParameter
            {
                ParameterName = "UserId",
                Value = UserId
            };

            para[1] = new SqlParameter
            {
                ParameterName = "ClientId",
                Value = ClientId
            };

            var data = db.Database.SqlQuery<RevenuePlanner.Models.CustomDashboardModel>("GetCustomDashboardsClientwise @UserId,@ClientId", para).ToList();
            return data;
        }
        //Added by komal rawal on 16-08-2016 regarding #2484 save notifications 
        public int SaveLogNoticationdata(string action, string actionSuffix, int? componentId, string componentTitle, string description, int? objectid, int? parentObjectId, string TableName, Guid ClientId, Guid User, string UserName, string EntityOwnerID, string ReportRecipientUserIds)
        {
            int returnvalue = 0;
            MRPEntities db = new MRPEntities();
            List<string> lst_RecipientId = new List<string>();
            if (description == Convert.ToString(Enums.ChangeLog_ComponentType.tactic).ToLower() && componentId != null)
            {
                if(action == Convert.ToString(Enums.ChangeLog_Actions.submitted))
                {
                    BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                    var lstUserHierarchy = objBDSUserRepository.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                    var objOwnerUser = lstUserHierarchy.FirstOrDefault(u => u.UserId == Guid.Parse(EntityOwnerID));
                    lst_RecipientId.Add(Convert.ToString(objOwnerUser.ManagerId));
                    lst_RecipientId.Add(EntityOwnerID);
                }
                else
                {
                lst_RecipientId = Common.GetCollaboratorForTactic(Convert.ToInt32(componentId));
                }
            }
            else if (description == Convert.ToString(Enums.ChangeLog_ComponentType.program).ToLower() && componentId != null)
            {
                lst_RecipientId = Common.GetCollaboratorForProgram(Convert.ToInt32(componentId));
            }
            else if (description == Convert.ToString(Enums.ChangeLog_ComponentType.campaign).ToLower() && componentId != null)
            {
                lst_RecipientId = Common.GetCollaboratorForCampaign(Convert.ToInt32(componentId));
            }
            string RecipientIds = null;

            if (lst_RecipientId.Count > 0)
            {
                RecipientIds = String.Join(",", lst_RecipientId);

            }
            else if (TableName == Convert.ToString(Enums.ChangeLog_TableName.Report) && action == Convert.ToString(Enums.ChangeLog_Actions.shared))
            {
                RecipientIds = ReportRecipientUserIds;
            }
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("SaveLogNoticationdata", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@action", action);
                command.Parameters.AddWithValue("@actionSuffix", actionSuffix);
                command.Parameters.AddWithValue("@componentId", componentId);
                command.Parameters.AddWithValue("@componentTitle", componentTitle);
                command.Parameters.AddWithValue("@description", description);
                command.Parameters.AddWithValue("@objectId", objectid);
                command.Parameters.AddWithValue("@parentObjectId", parentObjectId);
                command.Parameters.AddWithValue("@TableName", TableName);
                command.Parameters.AddWithValue("@Userid", User);
                command.Parameters.AddWithValue("@ClientId", ClientId);
                command.Parameters.AddWithValue("@UserName", UserName);
                command.Parameters.AddWithValue("@RecipientIDs", RecipientIds);
                command.Parameters.AddWithValue("@EntityOwnerID", EntityOwnerID);
                string returnvalue1 = command.ExecuteScalar().ToString();
                //  adp.Fill(dataset);
                returnvalue = Convert.ToInt32(returnvalue1);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return returnvalue;
        }

        //ENd

    }
    #endregion






}