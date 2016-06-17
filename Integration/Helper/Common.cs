using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RevenuePlanner.Models;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Web;
using System.Net.Configuration;
using System.Configuration;
using System.Net;
using System.Data.SqlClient;

namespace Integration.Helper
{
    class Common
    {
        public static string StageINQ = "INQ";
        public static string StageProjectedStageValue = "ProjectedStageValue";
        public static string Responded = "Responded";
        public static string ClosedWon = "Closed Won";
        public static string StageCW = "CW";
        public static string StageRevenue = "Revenue";

        public static string StageMQL = "MQL";
        public static string MQLStageValue = "MQL";

        public static string UnableToUpdate = "Unable to update";
        public static string UnableToDelete = "Unable to delete";
        public static string TacticSyncedComment = "Tactic synced with ";
        public static string TacticUpdatedComment = "Tactic updated with ";
        public static string ProgramSyncedComment = "Program synced with ";
        public static string ProgramUpdatedComment = "Program updated with ";
        public static string CampaignSyncedComment = "Campaign synced with ";
        public static string CampaignUpdatedComment = "Campaign updated with ";
        public static string ImprovementTacticSyncedComment = "Improvement Tactic synced with ";
        public static string ImprovementTacticUpdatedComment = "Improvement Tactic updated with ";

        public static string OpportunityObjectError = "Opportunity : ";
        public static string CampaignMemberObjectError = "Campaign Member : ";
        public static string SalesForceCampaignObject = "Campaign";

        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic

        // Added by Dharmraj on 20-8-2014,#684, Common Error/Success messages
        public static string msgExternalServerNotConfigured = "Error: External server not configured";
        public static string msgDirectoryNotFound = "Error: Directory {0} not found";
        public static string msgNotConnectToExternalServer = "Error: Could not connect to Integration Instance External Server";
        public static string msgRequiredColumnNotExistEloquaPullResponse = "Error: Response doesn't contains EloquaCampaignID or ExternalCampaignID or ResponseDateTime columns";
        public static string msgFileNotFound = "File not found";
        public static string msgMappingNotFoundForSalesforcePullResponse = "Mapping not found for CampaignId or FirstRespondedDate or Status";
        public static string msgMappingNotFoundForSalesforcePullCW = "Mapping not found for CampaignId or CloseDate or Amount or StageName or Response Date or Last Modified Date";
        public static string msgMappingNotFoundForEloquaPullMQL = "Error: Mapping not found for CampaignId or MQLDateId or ViewId or ListId";
        public static string msgChildLevelError = "Error: Error occurred while processing Tactic/Improvement Tactic.";
        public static readonly int CustomNameLimitSet = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CustomNameLimitSet"]); ////Added by :- Pratik Chauhan on 04/02/2015 for PL ticket #1147
        public static readonly string DateFormatForSalesforce = "yyyy-MM-ddTHH:mm:ss.000+0000";
        public static bool IsAutoSync = false;
        public static string eloquaClientIdLabel = "ClientId";
        public static string eloquaClientSecretLabel = "ClientSecret";
        /// <summary>
        /// Added by Bhavesh
        /// Date: 28/7/2015
        /// Ticket : #1385	Enable TLS 1.1 or higher Encryption for Salesforce
        /// </summary>
        public static readonly string EnableTLS1AndHigher = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EnableTLS1AndHigher"]);

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

        internal static List<string> GetStatusListAfterApproved()
        {
            List<string> tacticStatus = new List<string>();
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Approved.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.InProgress.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Complete.ToString()].ToString());
            return tacticStatus;
        }

        internal static Enums.Mode GetMode(string integrationInstanceTacticId)
        {
            // Status = After Approve - Is Deploy = true -  integrationInstanceTacticId = null - isDeleted = false
            if (string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Enums.Mode.Create;
            }
            // Status = After Approve - Is Deploy = true -  integrationInstanceTacticId = yes - isDeleted = false
            else
            {
                return Enums.Mode.Update;
            }
        }

        /// <summary>
        /// Dharmraj, #658, 8-8-2014
        /// Function to convert CSV or xls file to data table
        /// </summary>
        /// <param name="FullfilePath"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(string FullfilePath)
        {
            string fileName = System.IO.Path.GetFileName(FullfilePath).ToString();
            string ext = Path.GetExtension(fileName);
            DataTable dt = new DataTable();

            if (ext.ToLower() == ".csv")
            {
                StreamReader sr = new StreamReader(FullfilePath);
                string line = sr.ReadLine();
                string[] value = line.Replace("\"", "").Split(',');
                if (value.Length > 0)
                {
                    dt.TableName = fileName.ToString();
                    DataRow row;
                    foreach (string dc in value)
                    {
                        dt.Columns.Add(new DataColumn(dc));
                    }
                    if (!sr.EndOfStream)
                    {
                        while (!sr.EndOfStream)
                        {
                            value = sr.ReadLine().Replace("\"", "").Split(',');
                            if (value.Length == dt.Columns.Count)
                            {
                                row = dt.NewRow();
                                row.ItemArray = value;
                                dt.Rows.Add(row);
                            }
                        }
                    }
                }

                sr.Close();
                sr.Dispose();
            }
            else
            {
                System.IO.FileStream fs = new System.IO.FileStream(FullfilePath, System.IO.FileMode.Open);
                IExcelDataReader excelReader2007 = null;
                if (ext.ToLower() == ".xlsx")
                {
                    excelReader2007 = ExcelReaderFactory.CreateOpenXmlReader(fs);
                }
                else
                {
                    excelReader2007 = ExcelReaderFactory.CreateBinaryReader(fs);
                }

                excelReader2007.IsFirstRowAsColumnNames = true;
                DataSet result = excelReader2007.AsDataSet();
                if (result.Tables.Count > 0)
                {
                    dt = result.Tables[0];
                }
                fs.Close();
                fs.Dispose();
            }

            return dt;
        }

        /// <summary>
        /// To insert log in IntegrationInstanceSection
        /// Added by Dharmraj on 18-8-2014, PL#684
        /// </summary>
        /// <param name="IntegartionInstanceLogId">IntegartionInstanceLogId</param>
        /// <param name="IntegartionInstanceId">Integration Instance Id</param>
        /// <param name="SectionName">Name of section (Entity) for which sync starts</param>
        /// <param name="SyncStart">sync start timestamp</param>
        /// <param name="CreateBy">logged in user id</param>
        /// <returns>IntegrationInstanceSectionId</returns>
        public static int CreateIntegrationInstanceSection(int IntegartionInstanceLogId, int IntegartionInstanceId, string SectionName, DateTime SyncStart, Guid CreateBy)
        {
            using (MRPEntities db = new MRPEntities())
            {
                IntegrationInstanceSection objIntegrationInstanceSection = new IntegrationInstanceSection();
                objIntegrationInstanceSection.IntegrationInstanceLogId = IntegartionInstanceLogId;
                objIntegrationInstanceSection.IntegrationInstanceId = IntegartionInstanceId;
                objIntegrationInstanceSection.SectionName = SectionName;
                objIntegrationInstanceSection.SyncStart = SyncStart;
                objIntegrationInstanceSection.CreatedDate = DateTime.Now;
                objIntegrationInstanceSection.CreateBy = CreateBy;
                db.Entry(objIntegrationInstanceSection).State = EntityState.Added;
                int resulValue = db.SaveChanges();
                if (resulValue > 0)
                {
                    return objIntegrationInstanceSection.IntegrationInstanceSectionId;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// To update log in IntegrationInstanceSection
        /// Added by Dharmraj on 18-8-2014, PL#684
        /// </summary>
        /// <param name="IntegrationInstanceSectionId"></param>
        /// <param name="Status"></param>
        /// <param name="SyncEnd"></param>
        /// <param name="Description"></param>
        public static void UpdateIntegrationInstanceSection(int IntegrationInstanceSectionId, StatusResult Status, string Description)
        {
            using (MRPEntities db = new MRPEntities())
            {
                IntegrationInstanceSection objIntegrationInstanceSection = db.IntegrationInstanceSections.FirstOrDefault(i => i.IntegrationInstanceSectionId == IntegrationInstanceSectionId);
                objIntegrationInstanceSection.Status = Status.ToString();
                objIntegrationInstanceSection.Description = Description;
                objIntegrationInstanceSection.SyncEnd = DateTime.Now;
                db.Entry(objIntegrationInstanceSection).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Created By : Viral Kadiya
        /// Created Date : 27/02/2014
        /// Desciption : Calculate the ActualCost of the tactic list
        /// </summary>
        /// <param name="PlanTacticId"></param>
        /// <returns>Actual cost of a Tactic</returns>
        public static Dictionary<int, string> CalculateActualCostTacticslist(List<int> PlanTacticIds)
        {
            string cost = "Cost";
            string strActualCost = "0";
            Dictionary<int, string> dicTactic_ActualCost = new Dictionary<int, string>();
            List<int> lstLineItems = new List<int>();
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> lstLineItemActuals = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            List<Plan_Campaign_Program_Tactic_Actual> lstPlanTacticsActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    List<Plan_Campaign_Program_Tactic_LineItem> tblLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(li => PlanTacticIds.Contains(li.PlanTacticId) && li.IsDeleted.Equals(false)).ToList();
                    /// Added By Bhavesh Date: 21/07/2015 - remove tolist from actual table
                    List<int> lineItemIds = tblLineItems.Select(lineitem => lineitem.PlanLineItemId).ToList();
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lia => lineItemIds.Contains(lia.PlanLineItemId)).ToList();
                    List<Plan_Campaign_Program_Tactic_Actual> tblPlanTacticsActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(pta => PlanTacticIds.Contains(pta.PlanTacticId) && pta.StageTitle.Equals(cost)).ToList();
                    foreach (int keyTactic in PlanTacticIds)
                    {
                        lstLineItems = new List<int>();
                        lstLineItems = tblLineItems.Where(line => line.PlanTacticId.Equals(keyTactic)).Select(li => li.PlanLineItemId).ToList();
                        if (lstLineItems.Count > 0)
                        {
                            lstLineItemActuals = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                            lstLineItemActuals = tblLineItemActuals.Where(lia => lstLineItems.Contains(lia.PlanLineItemId)).ToList();
                            if (lstLineItemActuals.Count > 0)
                            {
                                var actualCostSum = lstLineItemActuals.Sum(lia => lia.Value);
                                strActualCost = actualCostSum.ToString();
                            }
                        }
                        else
                        {
                            lstPlanTacticsActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                            lstPlanTacticsActuals = tblPlanTacticsActuals.Where(pta => pta.PlanTacticId.Equals(keyTactic)).ToList();
                            if (lstPlanTacticsActuals.Count > 0)
                            {
                                var actualCostSum = lstPlanTacticsActuals.Sum(pta => pta.Actualvalue);
                                strActualCost = actualCostSum.ToString();
                            }
                        }
                        dicTactic_ActualCost.Add(keyTactic, strActualCost);
                    }
                }
                return dicTactic_ActualCost;
            }
            catch
            {
                return dicTactic_ActualCost;
            }
        }

        #region Custom Naming Structure
        /// <summary>
        /// Function generate custom name for tactic using abbreviation.
        /// Added by Mitesh Vaishnav on 04/12/2014
        /// #1000 - Custom naming: Campaign name structure
        /// </summary>
        /// <param name="objTactic">Contains details of tactic</param>
        /// <param name="CustomFields">Contains details of custom fields of particular tactic</param>
        /// <returns></returns>
        public static string GenerateCustomName(Plan_Campaign_Program_Tactic objTactic, List<CampaignNameConvention> SequencialOrderedTableList, List<CustomFiledMapping> mappingCustomFields)
        {
            StringBuilder customTacticName = new StringBuilder();
            int fieldlength;
            if (objTactic != null)
            {
                //Fetch custom name convention sequence
                if (SequencialOrderedTableList != null) //Added by rahul Shah
                {
                    if (SequencialOrderedTableList.Count > 0)
                    {
                        foreach (CampaignNameConvention objCampaignNameConvention in SequencialOrderedTableList)
                        {
                            fieldlength = 0;
                            if (objCampaignNameConvention.CustomNameCharNo.HasValue)
                                fieldlength = objCampaignNameConvention.CustomNameCharNo.Value;

                            if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.CustomField.ToString())
                            {
                                var customobj = mappingCustomFields.Where(a => a.CustomFieldId == objCampaignNameConvention.CustomFieldId && a.EntityId == objTactic.PlanTacticId).FirstOrDefault();
                                if (customobj != null)
                                {
                                    // Added by Viral: #2053: Trim/add charcters while generating external name based on configuration
                                    if (fieldlength != 0 && !string.IsNullOrEmpty(customobj.CustomNameValue) && (customobj.CustomNameValue.Length > fieldlength))
                                    {
                                        customobj.CustomNameValue = customobj.CustomNameValue.Substring(0, fieldlength);
                                    }
                                    customTacticName.Append(RemoveSpaceAndUppercaseFirst(customobj.CustomNameValue) + "_");
                                }
                            }
                            else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.Plan_Campaign_Program_Tactic.ToString())
                            {
                                string tacticTitle = RemoveSpaceAndUppercaseFirst(System.Web.HttpUtility.HtmlDecode(objTactic.Title));
                                // Added by Viral: #2053: Trim/add charcters while generating external name based on configuration
                                if ((fieldlength != 0) && (!string.IsNullOrEmpty(tacticTitle)) && (tacticTitle.Length > fieldlength))
                                {
                                    tacticTitle = tacticTitle.Substring(0, fieldlength);
                                }
                                customTacticName.Append(tacticTitle + "_");
                            }
                            else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.TacticType.ToString())
                            {
                                string tacticTypeTitle = !string.IsNullOrEmpty(objTactic.TacticType.Abbreviation) ? RemoveSpaceAndUppercaseFirst(objTactic.TacticType.Abbreviation) : RemoveSpaceAndUppercaseFirst(objTactic.TacticType.Title);
                                // Added by Viral: #2053: Trim/add charcters while generating external name based on configuration
                                if (fieldlength != 0 && !string.IsNullOrEmpty(tacticTypeTitle) && (tacticTypeTitle.Length > fieldlength))
                                {
                                    tacticTypeTitle = tacticTypeTitle.Substring(0, fieldlength);
                                }
                                customTacticName.Append(tacticTypeTitle + "_");
                            }
                        }
                        if (customTacticName.ToString().Length > 0)
                        {
                            var index = customTacticName.ToString().LastIndexOf('_');
                            if (index > 0)
                            {
                                customTacticName.Remove(index, 1);
                                string replaceMultipleUnderscore = Regex.Replace(customTacticName.ToString(), "_+", "_");
                                customTacticName.Clear();
                                customTacticName.Append(replaceMultipleUnderscore);
                            }
                        }
                    }
                    else
                    {
                        objTactic.Title = RemoveSpaceAndUppercaseFirst(objTactic.Title);
                        customTacticName.Append(objTactic.Title);
                    }
                }
            }

            return customTacticName.ToString();
        }

        /// <summary>
        /// This function will remove the spaces from words (taactic name) and make the first character in upper case & remove the special characters
        /// </summary>
        /// <param name="title">space will be removed and make first uppercase for this string</param>
        /// <returns></returns>
        private static string RemoveSpaceAndUppercaseFirst(string title)
        {
            //if we want to replace the _ from the string it self then please un comment the following line
            //if (!string.IsNullOrEmpty(s)) {  s = s.Replace("_"," "); }

            string[] arrString = title.Split(' ');
            string returnString = string.Empty;
            if (arrString.Length > 0)
            {
                foreach (string tmpString in arrString)
                {
                    if (!string.IsNullOrEmpty(tmpString))
                    {
                        returnString += char.ToUpper(tmpString[0]) + tmpString.Substring(1);
                    }
                }
            }
            else
            {
                returnString = title;
            }
            returnString = Regex.Replace(returnString, @"[^0-9a-zA-Z_]+", "");
            return returnString;
        }

        #endregion

        #region Send Email

        /// <summary>
        /// Function to send email
        /// </summary>
        /// <param name="emailid">email id of receiver</param>
        /// <param name="fromemailid">email id of sender</param>
        /// <param name="strMsg">email content</param>
        /// <param name="Subject">email subject</param>
        /// <param name="Priority">email priority to be sent</param>
        /// <param name="CustomAlias">alias name of sender</param>
        /// <param name="isSupportMail">flag to specify support mail</param>
        public static void SendMail(string emailidlist, string fromemailid, string strMsg, string Subject, string Priority)
        {
            MailMessage objEmail = new MailMessage();
            try
            {
                string FromAlias = System.Configuration.ConfigurationManager.AppSettings.Get("FromAlias");
                objEmail.From = new MailAddress(fromemailid);
                //// Add alias
                if (!string.IsNullOrEmpty(FromAlias))
                {
                    objEmail.From = new MailAddress(fromemailid, FromAlias);
                }

                objEmail.To.Add(new MailAddress(emailidlist));
                objEmail.Subject = HttpUtility.HtmlDecode(Subject);
                objEmail.Body = strMsg;
                objEmail.IsBodyHtml = true;
                objEmail.Priority = MailPriority.Normal;

                //// Get appropriate SmtpSection for mail sending
                SmtpSection smtpSection = GetSmtpSection();
                if (smtpSection != null)
                {
                    SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                    smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                    smtpClient.EnableSsl = smtpSection.Network.EnableSsl;
                    smtpClient.Send(objEmail);
                }
            }
            catch
            {
                //// Error log when exception occurs in email sending
                return;
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
        public static SmtpSection GetSmtpSection()
        {
            SmtpSection smtpSection = new SmtpSection();
            smtpSection = (SmtpSection)ConfigurationManager.GetSection("mailSettings/smtp_other");
            return smtpSection;
        }

        #endregion

        /// <summary>
        /// Function to get instance name using integation instance id
        /// </summary>
        /// <param name="integrationInstanceId">integration instance id</param>
        /// <returns></returns>
        public static string GetInstanceName(int integrationInstanceId)
        {
            MRPEntities db = new MRPEntities();
            return db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == integrationInstanceId).Instance;
        }

        #region Eloqua sync error email

        /// <summary>
        /// 
        /// </summary>
        /// <param name="EntityId">entity id</param>
        /// <param name="EntityType">entity type</param>
        /// <param name="Message">error/info message</param>
        /// <param name="SyncStatus">error status flag</param>
        /// <param name="TimeStamp">timestamp</param>
        /// <returns>returns SyncError object</returns>
        public static SyncError PrepareSyncErrorList(int? EntityId, Enums.EntityType EntityType, string SectionName, string Message, Enums.SyncStatus SyncStatus, DateTime TimeStamp)
        {
            SyncError objSyncError = new SyncError();

            objSyncError.EntityId = EntityId;
            objSyncError.EntityType = EntityType;
            objSyncError.SectionName = SectionName.ToString();
            objSyncError.Message = Message;
            objSyncError.SyncStatus = SyncStatus;
            objSyncError.TimeStamp = TimeStamp;

            return objSyncError;
        }

        /// <summary>
        /// Function to prepare Error email body header row
        /// </summary>
        /// <param name="InfoHeader"></param>
        /// <param name="InfoValue"></param>
        /// <returns></returns>
        public static string PrepareInfoRow(string InfoHeader, string InfoValue)
        {
            string row = string.Format("<tr><td width='50%'><b>{0}:</b></td><td width='40%'>{1}</td></tr>", InfoHeader, InfoValue);
            return row;
        }

        /// <summary>
        /// Function to prepare error email body
        /// </summary>
        /// <param name="lstSyncError"></param>
        /// <returns></returns>
        public static string PrepareSyncErroEmailBody(List<SyncError> lstSyncError)
        {
            if (lstSyncError.Count > 0)
            {
                StringBuilder sbErroBody = new StringBuilder(string.Empty);
                List<string> lstInfo = lstSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Header).Select(syncError => syncError.Message).ToList();
                if (lstInfo.Count > 0)
                {
                    sbErroBody.Append("<table width='100%' border='1' color='#908d88' cellspacing='0' cellpadding='0'>");
                    lstInfo.ForEach(info => sbErroBody.Append(info));
                    sbErroBody.Append("</table>");
                }

                var lstError = lstSyncError.Where(syncError => syncError.SyncStatus != Enums.SyncStatus.Header && syncError.SyncStatus != Enums.SyncStatus.Success)
                                            .GroupBy(item => new { SyncStatus = item.SyncStatus, Message = item.Message })
                                            .Select(groupItem => new
                                            {
                                                SyncStatus = groupItem.Key.SyncStatus,
                                                TimeStamp = groupItem.Select(item => item.TimeStamp).FirstOrDefault(),
                                                Message = groupItem.Select(item => item.Message).FirstOrDefault(),
                                            }).Distinct().ToList();

                if (lstError.Count > 0)
                {
                    sbErroBody.Append("<br>");
                    sbErroBody.Append("<table width='100%' border='1' color='#908d88' cellspacing='0' cellpadding='0'>");
                    sbErroBody.Append("<tr>");
                    sbErroBody.Append("<th width='70%'>Description</th>");
                    sbErroBody.Append("<th width='10%'>Status</th>");
                    sbErroBody.Append("<th width='20%'>Timestamp</th>");
                    sbErroBody.Append("</tr>");

                    foreach (var item in lstError)
                    {
                        sbErroBody.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", item.Message, item.SyncStatus.ToString(), item.TimeStamp));
                    }

                    sbErroBody.Append("</table>");
                }

                return sbErroBody.ToString();
            }

            return string.Empty;
        }

        #endregion

        /// <summary>
        /// Truncate name up to valid limit.
        /// </summary>
        /// <param name="name">Name of activity.</param>
        /// <returns>Truncated Name.</returns>
        public static string TruncateName(string name)
        {
            name = name.Substring(0, ((name.Length > (CustomNameLimitSet + 1)) ? CustomNameLimitSet : name.Length));
            return name;
        }

        /// <summary>
        /// </summary>
        public static string GetClosedWon(Guid _clientId)
        {
            string ClosedWonTitle = ClosedWon;
            using (MRPEntities db = new MRPEntities())
            {
                ClosedWonTitle = db.Stages.FirstOrDefault(i => i.ClientId == _clientId && !i.IsDeleted && i.Code == StageCW).Title;
                if (string.IsNullOrEmpty(ClosedWonTitle))
                {
                    ClosedWonTitle = ClosedWon;
                }
            }
            return ClosedWonTitle;
        }

        /// <summary>
        /// </summary>
        public static string GetClosedWonMappingField(int integrationInstanceId)
        {
            string cwTargetType = string.Empty, gpCWActualFieldName = "CW", gpCWType="CW";
            int integrationTypeId = 0;
            using (MRPEntities db = new MRPEntities())
            {
                IntegrationInstance objInstance = new IntegrationInstance();
                objInstance = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == integrationInstanceId);
                integrationTypeId = objInstance.IntegrationTypeId;
                GameplanDataTypePull objGPTypePull = new GameplanDataTypePull();
                objGPTypePull = db.GameplanDataTypePulls.FirstOrDefault(item => item.IntegrationTypeId == integrationTypeId && item.ActualFieldName == gpCWActualFieldName && item.Type == gpCWType && item.IsDeleted == false);
                int gpDataTypePullId = objGPTypePull != null ? objGPTypePull.GameplanDataTypePullId : 0;

                IntegrationInstanceDataTypeMappingPull objPUll = new IntegrationInstanceDataTypeMappingPull();
                objPUll = db.IntegrationInstanceDataTypeMappingPulls.FirstOrDefault(item => item.IntegrationInstanceId == integrationInstanceId && item.GameplanDataTypePullId == gpDataTypePullId);
                cwTargetType = objPUll != null ? objPUll.TargetDataType : string.Empty;
            }
            return cwTargetType;
        }


        #region "Save IntegrationInstance Log Details Function"
        public static void SaveIntegrationInstanceLogDetails(int _entityId, int? IntegrationInstanceLogId, Enums.MessageOperation MsgOprtn, string functionName, Enums.MessageLabel MsgLabel, string logMsg)
        {
            string logDescription = string.Empty, preMessage = string.Empty;
            try
            {
                if (MsgOprtn.Equals(Enums.MessageOperation.None))
                    preMessage = (MsgLabel.Equals(Enums.MessageLabel.None) ? string.Empty : MsgLabel.ToString() + " : ") + "---";   // if message operation "None" than Message prefix should be "---" ex: . 
                else
                    preMessage = (MsgLabel.Equals(Enums.MessageLabel.None) ? string.Empty : (MsgOprtn.Equals(Enums.MessageOperation.Start)) ? string.Empty : (MsgLabel.ToString() + " : ")) + MsgOprtn.ToString() + " :";

                logDescription = preMessage + " " + functionName + " : " + logMsg;
                using (MRPEntities db = new MRPEntities())
                {
                    IntegrationInstanceLogDetail objLogDetails = new IntegrationInstanceLogDetail();
                    objLogDetails.EntityId = _entityId;
                    objLogDetails.IntegrationInstanceLogId = IntegrationInstanceLogId;
                    objLogDetails.LogTime = System.DateTime.Now;
                    objLogDetails.LogDescription = logDescription;
                    db.Entry(objLogDetails).State = System.Data.EntityState.Added;
                    db.SaveChanges();
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region "Get Most inner level Exception"
        public static string GetInnermostException(Exception e)
        {
            if (e == null)
            {
                return new ArgumentNullException("e").Message;
            }

            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            return e.Message;
        }
        #endregion
    }

    // Add By Nishant Sheth
    // Desc :: common methods for stroed procedures
    #region stored procedures methods
    public class StoredProcedure
    {
        public void ExecuteStoreProcedure(MRPEntities context, string storeProcName, params object[] parameters)
        {

            string command = "EXEC " + storeProcName;
            foreach (var parameter in parameters)
            {
                command += " @" + parameter + ",";
            }
            command = command.TrimEnd(',');
            context.Database.ExecuteSqlCommand(command, parameters);
            context.SaveChanges();
        }
        // Add By Rahul Shah
        // Desc :: get FieldMappings data for Marketo
        public DataSet GetFieldMappings(string entityType, Guid clientId, int IntegrationTypeId, int IntegrationInstanceID)
        {
            string clientid = clientId.ToString();
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetFieldMappings", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@entityType", entityType);
                command.Parameters.AddWithValue("@clientId", clientid);
                command.Parameters.AddWithValue("@integrationTypeId", IntegrationTypeId);
                command.Parameters.AddWithValue("@id", IntegrationInstanceID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
            return dataset;
        }       
        // Added By Viral 
        // Desc :: Get SFDC Field Mappings list.
        public DataSet GetSFDCFieldMappings(Guid clientId, int IntegrationTypeId, int IntegrationInstanceID,bool isSFDCSyncMarketo)
        {
            string clientid = clientId.ToString();
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetSFDCFieldMappings", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@clientId", clientid);
                command.Parameters.AddWithValue("@integrationTypeId", IntegrationTypeId);
                command.Parameters.AddWithValue("@id", IntegrationInstanceID);
                command.Parameters.AddWithValue("@isSFDCMarketoIntegration", isSFDCSyncMarketo);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
            return dataset;
        }      
    }
    #endregion
    public class CRM_EloquaMapping
    {
        public string CRMId { get; set; }
        public string EloquaId { get; set; }
        public int PlanTacticId { get; set; }
        public DateTime? StartDate { get; set; }
        public string ShortCRMId { get; set; }
    }

    public class EloquaIntegrationInstanceTactic_Model_Mapping
    {
        public string EloquaIntegrationInstanceTacticId { get; set; }
        public int ModelIntegrationInstanceId { get; set; }
        public int PlanTacticId { get; set; }
    }

    public class SyncError
    {
        public int? EntityId { get; set; }
        public Enums.EntityType EntityType { get; set; }
        public string SectionName { get; set; }
        public string Message { get; set; }
        public Enums.SyncStatus SyncStatus { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class CustomFiledMapping
    {
        public int CustomFieldId { get; set; }
        public int EntityId { get; set; }
        public string Value { get; set; }
        public string CustomNameValue { get; set; }
    }

    public class Eloqua_RefreshToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
    public class PullClosedDealModel
    {
        public string fieldname { get; set; }
        public bool IsPicklistExist { get; set; }
        public List<string> pickList { get; set; }
    }
    public class SpParameters
    {
        public string name { get; set; }
        public dynamic parameterValue { get; set; }
     
    }
}
