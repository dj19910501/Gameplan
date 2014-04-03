using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

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

        #endregion

        #region Enums

        public enum Permission
        {
            NoAccess = 0,
            ViewOnly = 1,
            FullAccess = 2,
            NotAnEntity = 3
        }
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
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        /// <summary>
        /// Function to convert image into string
        /// </summary>
        /// <param name="filestream"></param>
        /// <returns></returns>
        public static string ImageToString(Stream filestream)
        {
            MemoryStream ms = new MemoryStream();
            System.Drawing.Image image = System.Drawing.Image.FromStream(filestream);
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] imageBytes = ms.ToArray();
            string ImageString = Convert.ToBase64String(imageBytes);
            ms.Flush();
            return ImageString;
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
        /// </summary>
        /// <param name="emailid"></param>
        /// <param name="fromemailid"></param>
        /// <param name="strMsg"></param>
        /// <param name="Subject"></param>
        /// <param name="Priority"></param>
        public static int sendMail(string emailid, string fromemailid, string strMsg, string Subject, string Priority, string CustomAlias = "")
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
                objEmail.Priority = MailPriority.Normal;
                SmtpClient smtp = new SmtpClient(strSMTPServer);
                smtp.Send(objEmail);
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
        /// Send email to multiple users
        /// </summary>
        /// <param name="emailid"></param>
        /// <param name="fromemailid"></param>
        /// <param name="strMsg"></param>
        /// <param name="Subject"></param>
        /// <param name="Priority"></param>
        public static void SendMailToMultipleUser(string emailidlist, string fromemailid, string strMsg, string Subject, string Priority, string CustomAlias = "")
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
                SmtpClient smtp = new SmtpClient(strSMTPServer);
                smtp.Send(objEmail);
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
        public static void sendMail(string emailid, string fromemailid, string strMsg, string Subject, Stream attachment, string attachmentName, string CustomAlias = "")
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
                SmtpClient smtp = new SmtpClient(strSMTPServer);
                smtp.Send(objEmail);
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
                }
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    PlanName = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == planTacticId).Select(pcp => pcp.Plan_Campaign.Plan.Title).SingleOrDefault();
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    PlanName = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == planTacticId).Select(pc => pc.Plan.Title).SingleOrDefault();
                }
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    PlanName = db.Plan_Improvement_Campaign_Program_Tactic.Where(pc => pc.ImprovementPlanTacticId == planTacticId).Select(pc => pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Title).SingleOrDefault();
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
                    var directorRoleCode = Enums.RoleCodes.D.ToString();
                    var lst_user = objBDSUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
                    var lst_director = lst_user.Where(ld => ld.RoleCode.Equals(directorRoleCode)).Select(l => l).ToList();
                    foreach (var item in lst_director)
                    {
                        lst_CollaboratorEmail.Add(item.Email);
                        lst_CollaboratorUserName.Add(item.FirstName);
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
        /// Function to get key of corresponding value.
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
        #endregion

        #region Authorization
        /// <summary>
        /// Returns the permission code 
        /// </summary>
        /// <param name="actionItem"></param>
        /// <returns></returns>
        public static Permission GetPermission(string actionItem)
        {
            if (Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                return Permission.FullAccess;
            }
            if (Sessions.RolePermission != null)
            {
                int PerCode = Sessions.RolePermission.Where(r => r.Code == actionItem).Select(r => r.PermissionCode).SingleOrDefault();
                switch (PerCode)
                {
                    case 0:
                        return Permission.NoAccess;
                    case 1:
                        return Permission.ViewOnly;

                    case 2:
                        return Permission.FullAccess;

                    default:
                        return Permission.NoAccess;
                }
            }
            return Permission.FullAccess;
        }
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

            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                //// Getting all business unit for client of director.
                var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId)).Select(b => b.BusinessUnitId).ToList<Guid>();
                businessUnitIds = clientBusinessUnit.ToList();
            }
            else
            {
                businessUnitIds.Add(Sessions.User.BusinessUnitId);
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

                var totalValue = (from t in db.Plan_Campaign_Program_Tactic
                                  where t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId
                                      && t.IsDeleted.Equals(false) && tacticStatus.Contains(t.Status)
                                  select new { t.MQLs, t.Cost }).ToList();

                if (plan.Status == Enums.PlanStatusValues[Enums.PlanStatus.Draft.ToString()].ToString())
                {
                    objHomePlanModelHeader.MQLs = plan.MQLs;
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
                    objHomePlanModelHeader.MQLs = totalValue.Sum(tv => tv.MQLs);
                    string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                    if (string.IsNullOrEmpty(MQLStageLabel))
                    {
                        objHomePlanModelHeader.mqlLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.MQLLabel.ToString()].ToString();
                    }
                    else
                    {
                        objHomePlanModelHeader.mqlLabel = MQLStageLabel;
                    }
                    objHomePlanModelHeader.Budget = totalValue.Sum(tv => tv.Cost);
                    objHomePlanModelHeader.costLabel = Enums.PlanHeader_LabelValues[Enums.PlanHeader_Label.Cost.ToString()].ToString();
                }

                /// Added By: Maninder Singh Wadhva
                /// Addressed PL Ticket: 37,38,47,49
                //// Getting improved MQL.
                double? improvedMQL = Common.CalculateImprovedMQL(planId, true);

                //// Calculating percentage increase.
                if (improvedMQL.HasValue && objHomePlanModelHeader.MQLs != 0)
                {
                    objHomePlanModelHeader.PercentageMQLImproved = ((improvedMQL - objHomePlanModelHeader.MQLs) / objHomePlanModelHeader.MQLs) * 100;
                    objHomePlanModelHeader.MQLs = Convert.ToDouble(improvedMQL);
                }

                if (totalValue != null)
                {
                    objHomePlanModelHeader.TacticCount = totalValue.Count();
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
                if (Sessions.IsSystemAdmin)
                {
                    var clientBusinessUnit = mydb.BusinessUnits.Where(b => b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                    businessUnitIds = clientBusinessUnit.ToList();
                }
                else if (Sessions.IsDirector || Sessions.IsClientAdmin)
                {
                    var clientBusinessUnit = mydb.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                    businessUnitIds = clientBusinessUnit.ToList();
                }
                else
                {
                    businessUnitIds.Add(Sessions.User.BusinessUnitId);
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
                        if (Sessions.IsPlanner)
                        {
                            return new MVCUrl { actionName = "PlanSelector", controllerName = "Plan", queryString = "" };
                        }
                        else
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
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.planYear.ToString()].ToString(), Value = Enums.UpcomingActivities.planYear.ToString(), Selected = true });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisyear.ToString()].ToString(), Value = Enums.UpcomingActivities.thisyear.ToString(), Selected = false });
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
                IsHideDragHandleRight = true
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
                IsHideDragHandleRight = true
            };

            return taskDataImprovementActivity;
        }

        /// <summary>
        /// Function to calculate improved MQL.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <returns>Returns calculated improved MQL.</returns>
        public static double? CalculateImprovedMQL(int planId, bool isApporvedOnly)
        {
            MRPEntities db = new MRPEntities();

            //// Getting list of status.
            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            //// Getting list of approved, in-progress and completed marketing activites.
            List<Plan_Campaign_Program_Tactic> marketingActivities = null;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = null;

            if (isApporvedOnly)
            {
                marketingActivities = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) &&
                                                                                                                      t.IsDeleted == false &&
                                                                                                                      tacticStatus.Contains(t.Status))
                                                                     .Select(t => t)
                                                                     .ToList();

                //// Getting list of approved, in-progress and completed improvement activites.
                improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) && t.IsDeleted == false && tacticStatus.Contains(t.Status)).Select(t => t).ToList();
            }
            else
            {
                marketingActivities = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) &&
                                                                                                      t.IsDeleted == false)
                                                     .Select(t => t)
                                                     .ToList();

                //// Getting list of improvement activites.
                improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) && t.IsDeleted == false).Select(t => t).ToList();
            }




            double? mql = null;

            //// Checking whether marketing and improvement activities exist.
            if (marketingActivities.Count() > 0 && improvementActivities.Count() > 0)
            {
                //// Getting MQL improved based on marketing and improvement activities.
                mql = Common.GetImprovedMQL(planId, marketingActivities, improvementActivities);
                return Convert.ToDouble(mql);
            }

            return mql;
        }

        /// <summary>
        /// Function to calculate improved Projected revenue or Closed won.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// Modified By Maninder Singh Wadhva, Ticket#406.
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <param name="isProjectedRevenue">Flag to indicate whether to calculate projected revenue or closed won.</param>
        /// <returns>Returns calculated projected revenue or closed won based on isProjectedReveneue flag.</returns>
        public static double? CalculateImprovedProjectedRevenueOrCW(int planId, bool isProjectedRevenue, double averageDealSize)
        {
            MRPEntities db = new MRPEntities();

            //// Getting list of marketing activites.
            List<Plan_Campaign_Program_Tactic> marketingActivities = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) && t.IsDeleted == false).Select(t => t).ToList();

            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) && t.IsDeleted == false).Select(t => t).ToList();

            double? improvedValue = null;

            //// Checking whether marketing and improvement activities exist.
            if (marketingActivities.Count() > 0 && improvementActivities.Count() > 0)
            {
                //// Getting Projected Reveneue or Closed Won improved based on marketing and improvement activities.
                improvedValue = Common.GetImprovedProjectedRevenueOrCW(planId, marketingActivities, improvementActivities, isProjectedRevenue, averageDealSize);
                return Convert.ToDouble(improvedValue);
            }

            return improvedValue;
        }

        /// <summary>
        /// Function to calculate improved deal size.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <returns>Returns improved deal size.</returns>
        public static double? CalculateImprovedDealSize(int planId)
        {
            MRPEntities db = new MRPEntities();

            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) && t.IsDeleted == false).Select(t => t).ToList();

            double? dealSize = null;

            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0)
            {
                //// Getting deal size improved based on improvement activities.
                dealSize = Common.GetImprovedDealSize(planId, improvementActivities);
                return Convert.ToDouble(dealSize);
            }

            return dealSize;
        }

        /// <summary>
        /// Function to calculate improved velocity.
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <returns>Returns improved velocity.</returns>
        public static double? CalculateImprovedVelocity(int planId)
        {
            MRPEntities db = new MRPEntities();

            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) && t.IsDeleted == false).Select(t => t).ToList();

            double? velocity = null;

            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0)
            {
                //// Getting velocity improved based on improvement activities.
                velocity = Common.GetImprovedVelocity(planId, improvementActivities);
                return Convert.ToDouble(velocity);
            }

            return velocity;
        }

        /// <summary>
        /// Function to get Improved MQL based on marketing activites and improvement activities.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// Modified By Manidner Singh Wadhva Addressed PL Ticket: 383, 393
        /// </summary>
        /// <param name="marketingActivities">Marketing Activities.</param>
        /// <param name="improvementActivities">Improvement Activities.</param>
        /// <returns>Returns MQL value after applying improvement activities.</returns>
        private static double GetImprovedMQL(int planId, List<Plan_Campaign_Program_Tactic> marketingActivities, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities)
        {
            MRPEntities db = new MRPEntities();

            //// Getting distinct effective dates from improvement activities.
            List<DateTime> effectiveDates = improvementActivities.Select(improvementTactic => improvementTactic.EffectiveDate).Distinct().OrderBy(improvementTactic => improvementTactic).ToList<DateTime>();

            //// Sorting marketing activities based on start date.
            marketingActivities = marketingActivities.OrderBy(tactic => tactic.StartDate).ToList();

            double improvedMQL = 0;
            List<int> affectedMarketingActivityIds = new List<int>();

            //// Iterating effective dates.
            foreach (DateTime effectiveDate in effectiveDates)
            {
                //// Calculating Effective From and To date of current hypothetical model.
                DateTime hypotheticalModelEffectiveDateFrom = effectiveDate;
                int indexEffectiveTo = effectiveDates.FindIndex(improvementTacticEffective => improvementTacticEffective > hypotheticalModelEffectiveDateFrom);
                DateTime? hypotheticalModelEffectiveDateTo = null;
                if (indexEffectiveTo >= 0)
                {
                    hypotheticalModelEffectiveDateTo = effectiveDates[indexEffectiveTo];
                }

                //// Getting list of marketing activites getting affected as per Effective From and To date of current hypothetical model.
                List<Plan_Campaign_Program_Tactic> affectedMarketingActivities = marketingActivities.Where(plantactic => (plantactic.StartDate >= hypotheticalModelEffectiveDateFrom && plantactic.StartDate < hypotheticalModelEffectiveDateTo) ||
                                                                                                         (plantactic.StartDate >= hypotheticalModelEffectiveDateFrom && hypotheticalModelEffectiveDateTo == null)).ToList();

                if (affectedMarketingActivities.Count() > 0)
                {
                    //// Adding to global variable which maintain list of marketing activities getting affected by current hypothetical model.
                    affectedMarketingActivityIds.AddRange(affectedMarketingActivities.Select(tactic => tactic.PlanTacticId));

                    //// Getting list of improvement activities which need to be considered for calculating hypothetical model.
                    List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesForHypotheticalModel = improvementActivities.Where(improvementTactic => improvementTactic.EffectiveDate <= hypotheticalModelEffectiveDateFrom)
                                                                                                                                    .Select(improvementTactic => improvementTactic)
                                                                                                                                    .ToList();

                    //// Getting Model based on hypothetical model effective date From.
                    //// Getting model based on plan id.
                    int ModelId = db.Plans.Where(p => p.PlanId == planId).Select(p => p.ModelId).SingleOrDefault();
                    //// Get Model id based on effective date From.
                    ModelId = RevenuePlanner.Controllers.ReportController.GetModelId(hypotheticalModelEffectiveDateFrom, ModelId);
                    //// Getting model.
                    Model effectiveModel = db.Models.Single(model => model.ModelId.Equals(ModelId));

                    //// Getting hypothetical model - conversion rate.
                    string metricTypeCR = Enums.MetricType.CR.ToString();
                    HypotheticalModel hypotheticalModel = GetHypotheticalModel(metricTypeCR, effectiveModel, improvementActivitiesForHypotheticalModel);

                    //// Getting metric to calculate MQL and calculating the same i.e. Stage >= INQ and Stages < MQL .
                    string mqlMetricCode = Enums.Stage.MQL.ToString();
                    string inqMetricCode = Enums.Stage.INQ.ToString();
                    int? levelMQL = db.Metrics.Single(metric => metric.MetricType == metricTypeCR && metric.MetricCode == mqlMetricCode && metric.ClientId == effectiveModel.BusinessUnit.ClientId).Level;
                    int? levelINQ = db.Metrics.Single(metric => metric.MetricType == metricTypeCR && metric.MetricCode == inqMetricCode && metric.ClientId == effectiveModel.BusinessUnit.ClientId).Level;
                    var mqlConversionQuery = hypotheticalModel.ImprovedMetrics.Where(mqlConversion => mqlConversion.Level >= levelINQ && mqlConversion.Level < levelMQL)
                                                                              .OrderBy(mqlConversion => mqlConversion.Level);
                    double mqlConversionRate = 0;
                    if (mqlConversionQuery != null && mqlConversionQuery.Count() != 0)
                    {
                        mqlConversionRate = mqlConversionQuery.Aggregate(1.0, (accumulated, mqlConversion) => accumulated * (mqlConversion.Value / 100));
                    }

                    //// Adding to improved MQL 
                    //// Modified By: Maninder Singh Wadhva to Address 408.
                    improvedMQL += affectedMarketingActivities.Select(affectedTactic => Math.Round(affectedTactic.INQs * mqlConversionRate)).Sum();
                }


            }

            //// Getting MQL of all unaffected marketing activities.
            double unaffectedMarketingActivitiesMQLs = marketingActivities.Where(tactic => !affectedMarketingActivityIds.Contains(tactic.PlanTacticId)).Select(tactic => tactic.MQLs).Sum();

            //// Adding to improved MQL.
            improvedMQL += unaffectedMarketingActivitiesMQLs;

            return improvedMQL;
        }

        /// <summary>
        /// Function to get improved Projected revenue or Closed Won.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// Modified By Manidner Singh Wadhva Addressed PL Ticket: 383, 393
        /// Modified By Maninder Singh Wadhva, Ticket#406.
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <param name="marketingActivities">marketing activities.</param>
        /// <param name="improvementActivities">improvement activities.</param>
        /// <param name="isProjectedRevenue">Flag to indicate whether to get improved projected revenue or closed won.</param>
        /// <returns>Returns improved projected revenue or closed won based on isProjectedRevenue flag.</returns>
        private static double GetImprovedProjectedRevenueOrCW(int planId, List<Plan_Campaign_Program_Tactic> marketingActivities, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities, bool isProjectedRevenue, double averageDealSize)
        {
            MRPEntities db = new MRPEntities();

            //// Getting distinct effective dates from improvement activities.
            List<DateTime> effectiveDates = improvementActivities.Select(improvementTactic => improvementTactic.EffectiveDate).Distinct().OrderBy(improvementTactic => improvementTactic).ToList<DateTime>();

            //// Sorting marketing activities based on start date.
            marketingActivities = marketingActivities.OrderBy(tactic => tactic.StartDate).ToList();

            double improvedValue = 0;
            List<int> affectedMarketingActivityIds = new List<int>();

            string funnelMarketing = Enums.Funnel.Marketing.ToString();

            //// Iterating effective dates.
            foreach (DateTime effectiveDate in effectiveDates)
            {
                //// Calculating Effective From and To date of current hypothetical model.
                DateTime hypotheticalModelEffectiveDateFrom = effectiveDate;
                int indexEffectiveTo = effectiveDates.FindIndex(improvementTacticEffective => improvementTacticEffective > hypotheticalModelEffectiveDateFrom);
                DateTime? hypotheticalModelEffectiveDateTo = null;
                if (indexEffectiveTo >= 0)
                {
                    hypotheticalModelEffectiveDateTo = effectiveDates[indexEffectiveTo];
                }

                //// Getting list of marketing activites getting affected as per Effective From and To date of current hypothetical model.
                List<Plan_Campaign_Program_Tactic> affectedMarketingActivities = marketingActivities.Where(plantactic => (plantactic.StartDate >= hypotheticalModelEffectiveDateFrom && plantactic.StartDate < hypotheticalModelEffectiveDateTo) ||
                                                                                                         (plantactic.StartDate >= hypotheticalModelEffectiveDateFrom && hypotheticalModelEffectiveDateTo == null)).ToList();

                if (affectedMarketingActivities.Count() > 0)
                {
                    //// Adding to global variable which maintain list of marketing activities getting affected by current hypothetical model.
                    affectedMarketingActivityIds.AddRange(affectedMarketingActivities.Select(tactic => tactic.PlanTacticId));

                    //// Getting list of improvement activities which need to be considered for calculating hypothetical model.
                    List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesForHypotheticalModel = improvementActivities.Where(improvementTactic => improvementTactic.EffectiveDate <= hypotheticalModelEffectiveDateFrom)
                                                                                                                                    .Select(improvementTactic => improvementTactic)
                                                                                                                                    .ToList();

                    //// Getting Model based on hypothetical model effective date From.
                    //// Getting model based on plan id.
                    int ModelId = db.Plans.Where(p => p.PlanId == planId).Select(p => p.ModelId).SingleOrDefault();
                    //// Get Model id based on effective date From.
                    ModelId = RevenuePlanner.Controllers.ReportController.GetModelId(hypotheticalModelEffectiveDateFrom, ModelId);
                    //// Getting model.
                    Model effectiveModel = db.Models.Single(model => model.ModelId.Equals(ModelId));

                    //// Modified By Manidner Singh Wadhva Addressed PL Ticket: 406
                    //double averageDealSizeMarketing = 0;
                    //if (isProjectedRevenue)
                    //{
                    //    //averageDealSizeMarketing = db.Model_Funnel.Where(modelFunnel => modelFunnel.ModelId == effectiveModel.ModelId &&
                    //    //                                modelFunnel.Funnel.Title.Equals(funnelMarketing))
                    //    //          .Select(mf => mf.AverageDealSize)
                    //    //          .SingleOrDefault();

                    //    averageDealSizeMarketing = GetImprovedDealSize(planId, improvementActivitiesForHypotheticalModel);
                    //}

                    //// Getting hypothetical model - conversion rate.
                    string metricTypeCR = Enums.MetricType.CR.ToString();
                    HypotheticalModel hypotheticalModel = GetHypotheticalModel(metricTypeCR, effectiveModel, improvementActivitiesForHypotheticalModel);

                    //// Getting metric to calculate CW and calculating the same i.e. Stage >= INQ.
                    string inqMetricCode = Enums.Stage.INQ.ToString();
                    int? levelINQ = db.Metrics.Single(metric => metric.MetricType == metricTypeCR && metric.MetricCode == inqMetricCode && metric.ClientId == effectiveModel.BusinessUnit.ClientId).Level;
                    var cwConversionQuery = hypotheticalModel.ImprovedMetrics.Where(cwConversion => cwConversion.Level >= levelINQ)
                                                                              .OrderBy(cwConversion => cwConversion.Level);
                    double cwConversionRate = 0;
                    if (cwConversionQuery != null && cwConversionQuery.Count() != 0)
                    {
                        cwConversionRate = cwConversionQuery.Aggregate(1.0, (accumulated, cwConversion) => accumulated * (cwConversion.Value / 100));
                    }

                    //// Adding to improved value 
                    if (isProjectedRevenue)
                    {
                        improvedValue += affectedMarketingActivities.Select(affectedTactic => affectedTactic.INQs * cwConversionRate * averageDealSize).Sum();
                    }
                    else
                    {
                        improvedValue += affectedMarketingActivities.Select(affectedTactic => affectedTactic.INQs * cwConversionRate).Sum();
                    }
                }
            }

            List<int> unaffectedMarketingActivitiesIds = marketingActivities.Where(tactic => !affectedMarketingActivityIds.Contains(tactic.PlanTacticId)).Select(tactic => tactic.PlanTacticId).ToList();
            if (unaffectedMarketingActivitiesIds.Count > 0)
            {
                List<ProjectedRevenueClass> unaffectedMarketingActivities = null;

                if (isProjectedRevenue)
                {
                    unaffectedMarketingActivities = RevenuePlanner.Controllers.ReportController.ProjectedRevenueCalculate(unaffectedMarketingActivitiesIds);
                }
                else
                {
                    unaffectedMarketingActivities = RevenuePlanner.Controllers.ReportController.ProjectedRevenueCalculate(unaffectedMarketingActivitiesIds, true);
                }

                //// Getting CW of all unaffected marketing activities.
                double unaffectedMarketingActivitiesImprovedValues = unaffectedMarketingActivities.Select(tactic => tactic.ProjectedRevenue).Sum();

                //// Adding to improved CW.
                improvedValue += unaffectedMarketingActivitiesImprovedValues;
            }

            return improvedValue;
        }

        /// <summary>
        /// Function to get improved deal size.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// </summary>
        /// <param name="planId">Current plan id.</param>
        /// <param name="improvementActivities">Improvement activities.</param>
        /// <returns>Returns improved deal size.</returns>
        private static double GetImprovedDealSize(int planId, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities)
        {
            MRPEntities db = new MRPEntities();

            //// Getting Model based on hypothetical model effective date From.
            //// Getting model based on plan id.
            int ModelId = db.Plans.Where(p => p.PlanId == planId).Select(p => p.ModelId).SingleOrDefault();
            //// Get Model id based on effective date From.
            ModelId = RevenuePlanner.Controllers.ReportController.GetModelId(improvementActivities.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), ModelId);
            //// Getting model.
            Model effectiveModel = db.Models.Single(model => model.ModelId.Equals(ModelId));

            //// Getting hypothetical model - size.
            string metricTypeSize = Enums.MetricType.Size.ToString();
            HypotheticalModel hypotheticalModel = GetHypotheticalModel(metricTypeSize, effectiveModel, improvementActivities);

            double improvedDealSize = 0;
            if (hypotheticalModel.ImprovedMetrics.Count() > 0)
            {
                improvedDealSize = hypotheticalModel.ImprovedMetrics[0].Value;
            }

            return improvedDealSize;
        }

        /// <summary>
        /// Function to get improved velocity.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// </summary>
        /// <param name="planId">Current plan Id.</param>
        /// <param name="improvementActivities">Improvement activities.</param>
        /// <returns>Returns improved velocity.</returns>
        private static double GetImprovedVelocity(int planId, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities)
        {
            MRPEntities db = new MRPEntities();

            //// Getting Model based on hypothetical model effective date From.
            //// Getting model based on plan id.
            int ModelId = db.Plans.Where(p => p.PlanId == planId).Select(p => p.ModelId).SingleOrDefault();
            //// Get Model id based on effective date From.
            ModelId = RevenuePlanner.Controllers.ReportController.GetModelId(improvementActivities.Select(improvementActivity => improvementActivity.EffectiveDate).Max(), ModelId);
            //// Getting model.
            Model effectiveModel = db.Models.Single(model => model.ModelId.Equals(ModelId));

            //// Getting hypothetical model - stage velocity.
            string metricTypeSV = Enums.MetricType.SV.ToString();
            HypotheticalModel hypotheticalModel = GetHypotheticalModel(metricTypeSV, effectiveModel, improvementActivities);

            double improvedVelocity = hypotheticalModel.ImprovedMetrics.Select(improvedMetric => improvedMetric.Value).Sum();
            return improvedVelocity;
        }

        /// <summary>
        /// Function to get hypothetical model based on improvement activities.
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// </summary>
        /// <param name="metricType">Metric Type.</param>
        /// <param name="model">Current Model.</param>
        /// <param name="improvementActivitiesForHypotheticalModel">Improvement Activities.</param>
        /// <returns>Returns hypothetical model.</returns>
        private static HypotheticalModel GetHypotheticalModel(string metricType, Model model, List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesForHypotheticalModel)
        {
            MRPEntities db = new MRPEntities();
            //// Get List of metric associated with selected client of current model.
            List<ImprovedMetric> improvedMetrics = db.Metrics.Where(metric => metric.ClientId == model.BusinessUnit.ClientId &&
                                                 metric.MetricType == metricType)
                                          .OrderBy(metric => metric.Level)
                                          .Select(metric => new ImprovedMetric
                                          {
                                              MetricId = metric.MetricId,
                                              MetricType = metric.MetricType,
                                              MetricCode = metric.MetricCode,
                                              Level = metric.Level,
                                              Value = 0
                                          })
                                          .ToList();

            //// Getting best in class BIC value based on metric id.
            List<int> metricIds = improvedMetrics.Select(metric => metric.MetricId).ToList();
            List<BestInClass> bestInClassValues = db.BestInClasses.Where(bic => metricIds.Contains(bic.MetricId)).ToList();

            string funnelMarketing = Enums.Funnel.Marketing.ToString();
            List<Model_Funnel_Stage> modelFunnelStage = null;
            if (!metricType.Equals(Enums.MetricType.Size.ToString()))
            {
                //// Getting stage value for marketing funnel.
                int funnelId = db.Funnels.Single(f => f.Title.Equals(funnelMarketing)).FunnelId;
                modelFunnelStage = db.Model_Funnel_Stage.Where(mfs => mfs.Model_Funnel.ModelId.Equals(model.ModelId) &&
                                                                         mfs.Model_Funnel.FunnelId.Equals(funnelId))
                                                         .ToList();
            }


            //// Iterating over each improved metrics.
            foreach (ImprovedMetric improvedMetric in improvedMetrics)
            {
                //// Getting baseline value for current metric.
                double modelvalue = 0;

                if (metricType.Equals(Enums.MetricType.CR.ToString()) || metricType.Equals(Enums.MetricType.SV.ToString()))
                {
                    //// Getting baseline value for current metric.
                    modelvalue = modelFunnelStage.Where(mfs => mfs.Stage.Code.Equals(improvedMetric.MetricCode) &&
                                                                    mfs.StageType.Equals(improvedMetric.MetricType))
                                                      .Select(mfs => mfs.Value).SingleOrDefault();
                    if (metricType.Equals(Enums.MetricType.CR.ToString()))
                    {
                        modelvalue = modelvalue / 100;
                    }
                }
                else if (metricType.Equals(Enums.MetricType.Size.ToString()))
                {
                    modelvalue = db.Model_Funnel.Where(modelFunnel => modelFunnel.ModelId == model.ModelId &&
                                                                      modelFunnel.Funnel.Title.Equals(funnelMarketing))
                                                .Select(mf => mf.AverageDealSize)
                                                .SingleOrDefault();
                }

                //// Get BestInClass BIC value for current metric id.
                double bestInClassValue = bestInClassValues.Where(bic => bic.MetricId.Equals(improvedMetric.MetricId))
                                                           .Select(bic => bic.Value)
                                                           .SingleOrDefault();

                //// Get ImprovementTactic & its Weight for current metric.
                var improvementActivitiesAndWeight = (from planImprovementTactic in improvementActivitiesForHypotheticalModel
                                                      join improvementTacticTypeMetric in db.ImprovementTacticType_Metric on planImprovementTactic.ImprovementTacticTypeId equals improvementTacticTypeMetric.ImprovementTacticTypeId
                                                      where improvementTacticTypeMetric.ImprovementTacticType.IsDeployed.Equals(true) &&
                                                            improvementTacticTypeMetric.MetricId.Equals(improvedMetric.MetricId) &&
                                                            improvementTacticTypeMetric.Weight > 0
                                                      select new
                                                      {
                                                          ImprovemetPlanTacticId = planImprovementTactic.ImprovementPlanTacticId,
                                                          Weight = improvementTacticTypeMetric.Weight
                                                      }).ToList();

                //// Calculate Total ImprovementCount 
                int totalCount = improvementActivitiesAndWeight.Count();

                //// Calculate Total ImprovementWeight
                double totalWeight = improvementActivitiesAndWeight.Sum(improvementActivity => improvementActivity.Weight);

                //// Getting improved value for metric
                if (metricType.Equals(Enums.MetricType.CR.ToString()))
                {
                    improvedMetric.Value = GetImprovedMetricValue(improvedMetric, bestInClassValue, modelvalue, totalCount, totalWeight) * 100;
                }
                else
                {
                    improvedMetric.Value = GetImprovedMetricValue(improvedMetric, bestInClassValue, modelvalue, totalCount, totalWeight);
                }
            }

            HypotheticalModel hypotheticalModel = new HypotheticalModel();
            hypotheticalModel.ImprovedMetrics = improvedMetrics;
            return hypotheticalModel;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// Function to get improved value for a metric based on best in class BIC, baseline, total count and total weight.
        /// </summary>
        /// <param name="improvedMetric">Metric to be improved.</param>
        /// <param name="bestInClassValue">Best in class Value.</param>
        /// <param name="baselineValue">Baseline value</param>
        /// <param name="totalCount">Total count of improvement activities.</param>
        /// <param name="totalWeight">Total wieght of improvement activities.</param>
        /// <returns>Returns improved value for current metric.</returns>
        private static double GetImprovedMetricValue(ImprovedMetric improvedMetric, double bestInClassValue, double baselineValue, double totalCount, double totalWeight)
        {
            double cFactor = 0;
            double rFactor = 0;

            #region rFactor
            //// Calculate rFactor if TotalCount = 0 then 0
            //// Else if TotalWeight/TotalCount < 2 then 0.25
            //// Else if TotalWeight/TotalCount < 3 then 0.5
            //// Else if TotalWeight/TotalCount < 4 then 0.75
            //// Else  0.9
            if (totalCount > 0)
            {
                double wcValue = totalWeight / totalCount;
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

            if (totalCount < 3)
            {
                cFactor = 0.4;
            }
            else if (totalCount >= 3 && totalCount < 5)
            {
                cFactor = 0.6;
            }
            else if (totalCount >= 5 && totalCount < 8)
            {
                cFactor = 0.8;
            }
            else
            {
                cFactor = 1;
            }
            #endregion

            //// Calculating BoostFactor
            double boostFactor = cFactor * rFactor;
            double boostGap = 0;
            //// Calculating boostGap
            if (improvedMetric.MetricType.Equals(Enums.MetricType.CR.ToString()))
            {
                boostGap = bestInClassValue - baselineValue;
            }
            else if (improvedMetric.MetricType.Equals(Enums.MetricType.SV.ToString()))
            {
                boostGap = baselineValue - bestInClassValue;
            }
            else if (improvedMetric.MetricType.Equals(Enums.MetricType.Size.ToString()))
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
            if (improvedMetric.MetricType.Equals(Enums.MetricType.CR.ToString()))
            {
                improvementValue = baselineValue + improvement;
            }
            else if (improvedMetric.MetricType.Equals(Enums.MetricType.SV.ToString()))
            {
                improvementValue = baselineValue - improvement;
            }
            else if (improvedMetric.MetricType.Equals(Enums.MetricType.Size.ToString()))
            {
                improvementValue = (1 + improvement) * baselineValue;
            }

            return improvementValue;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// Class for hypothetical model.
        /// </summary>
        public class HypotheticalModel
        {
            /// <summary>
            /// Member to hold improved metrics
            /// </summary>
            public List<ImprovedMetric> ImprovedMetrics { get; set; }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Addressed PL Ticket: 37,38,47,49
        /// Class for improved metric.
        /// </summary>
        public class ImprovedMetric
        {
            public int MetricId { get; set; }
            public string MetricType { get; set; }
            public string MetricCode { get; set; }
            public int? Level { get; set; }
            public double Value { get; set; }
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
    }
}