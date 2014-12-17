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
        public static string StageCost = "Cost";
        
        public static string UnableToUpdate = "Unable to update";
        public static string UnableToDelete = "Unable to delete";
        public static string TacticSyncedComment = "Tactic synced with ";
        public static string ImprovementTacticSyncedComment = "Improvement Tactic synced with ";
        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic

        // Added by Dharmraj on 20-8-2014,#684, Common Error/Success messages
        public static string msgExternalServerNotConfigured = "Error: External server not configured";
        public static string msgDirectoryNotFound = "Error: Directory {0} not found";
        public static string msgNotConnectToExternalServer = "Error: Could not connect to Integration Instance External Server";
        public static string msgRequiredColumnNotExistEloquaPullResponse = "Error: Response doesn't contains EloquaCampaignID or ExternalCampaignID or ResponseDateTime columns";
        public static string msgFileNotFound = "File not found";
        public static string msgMappingFieldsNotFound = "Mapping fields not found";
        public static string msgMappingNotFoundForSalesforcePullResponse = "Error: Mapping does not found for CampaignId or FirstRespondedDate or Status";
        public static string msgMappingNotFoundForSalesforcePullCW = "Error: Mapping does not found for CampaignId or CloseDate or Amount or StageName";

        public static bool IsAutoSync = false;
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

        internal static Enums.Mode GetMode(bool isDeleted, bool isDeployedToIntegration, string integrationInstanceTacticId, string status)
        {
            // delete reject status from list function
            List<string> statusList = Common.GetStatusListAfterApproved();
            // Status = After Approve - Is Deploy = true -  integrationInstanceTacticId = null - isDeleted = false
            if (statusList.Contains(status) && isDeployedToIntegration && string.IsNullOrWhiteSpace(integrationInstanceTacticId) && !isDeleted)
            {
                return Enums.Mode.Create;
            }
            // Status = After Approve - Is Deploy = true -  integrationInstanceTacticId = yes - isDeleted = false
            else if (statusList.Contains(status) && isDeployedToIntegration && !string.IsNullOrWhiteSpace(integrationInstanceTacticId) && !isDeleted)
            {
                return Enums.Mode.Update;
            }
            else if (!string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Enums.Mode.Delete;
            }
            else
            {
                return Enums.Mode.None;
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
                IExcelDataReader excelReader2007 = ExcelReaderFactory.CreateOpenXmlReader(fs);
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
            using (MRPEntities db=new MRPEntities())
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
        /// Created By : Sohel Pathan
        /// Created Date : 11/09/2014
        /// Desciption : Calculate the ActualCost of the tactic
        /// </summary>
        /// <param name="PlanTacticId"></param>
        /// <returns>Actual cost of a Tactic</returns>
        public static string CalculateActualCost(int PlanTacticId)
        {
            string cost = "Cost";
            string strActualCost = "0";
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    var lstLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(li => li.PlanTacticId.Equals(PlanTacticId) && li.IsDeleted.Equals(false)).ToList().Select(li => li.PlanLineItemId).ToList();
                    if (lstLineItems.Count > 0)
                    {
                        var lstLineItemActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lia => lstLineItems.Contains(lia.PlanLineItemId)).ToList();
                        if (lstLineItemActuals.Count > 0)
                        {
                            var actualCostSum = lstLineItemActuals.Sum(lia => lia.Value);
                            strActualCost = actualCostSum.ToString();
                        }
                    }
                    else
                    {
                        var lstPlanTacticsActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(pta => pta.PlanTacticId.Equals(PlanTacticId) && pta.StageTitle.Equals(cost)).ToList();
                        if (lstPlanTacticsActuals.Count > 0)
                        {
                            var actualCostSum = lstPlanTacticsActuals.Sum(pta => pta.Actualvalue);
                            strActualCost = actualCostSum.ToString();
                        }
                    }
                }
                return strActualCost;
            }
            catch
            {
                return strActualCost;
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
        public static string GenerateCustomName(Plan_Campaign_Program_Tactic objTactic, Guid clientId)
        {
            StringBuilder customTacticName = new StringBuilder();
            if (objTactic != null)
            {
                MRPEntities db = new MRPEntities();

                //Fetch custom name convention sequence
                List<CampaignNameConvention> SequencialOrderedTableList = db.CampaignNameConventions.Where(c => c.ClientId == clientId && c.IsDeleted == false).OrderBy(c => c.Sequence).ToList();
                if (SequencialOrderedTableList.Count > 0)
                {
                    var customFieldsForSequencialOrderedList = db.CustomField_Entity.ToList().Where(cf => SequencialOrderedTableList.Select(a => a.CustomFieldId).ToList().Contains(cf.CustomFieldId) && cf.EntityId == objTactic.PlanTacticId).ToList().Select(cf => new
                    {
                        CustomFieldId = cf.CustomFieldId,
                        Type = cf.CustomField.CustomFieldType.Name,
                        Abbreviation = string.Compare(cf.CustomField.CustomFieldType.Name, Enums.CustomFieldType.TextBox.ToString(), true) == 0 ? cf.Value : !string.IsNullOrEmpty(cf.CustomField.CustomFieldOptions.Where(cfo => cfo.CustomFieldOptionId.ToString() == cf.Value).Select(v => v.Abbreviation).FirstOrDefault()) ? cf.CustomField.CustomFieldOptions.Where(cfo => cfo.CustomFieldOptionId.ToString() == cf.Value).Select(v => v.Abbreviation).FirstOrDefault() : cf.CustomField.CustomFieldOptions.Where(cfo => cfo.CustomFieldOptionId.ToString() == cf.Value).Select(v => v.Value).FirstOrDefault(),
                    });

                    foreach (CampaignNameConvention objCampaignNameConvention in SequencialOrderedTableList)
                    {
                        if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.CustomField.ToString())
                        {
                            var objCustomField = customFieldsForSequencialOrderedList.Where(a => a.CustomFieldId == objCampaignNameConvention.CustomFieldId).FirstOrDefault();
                            if (objCustomField != null && !string.IsNullOrEmpty(objCustomField.Abbreviation))
                            {
                                customTacticName.Append(Regex.Replace(objCustomField.Abbreviation.Replace(" ","_"), @"[^0-9a-zA-Z_]+", "") + "_");
                            }
                        }
                        else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.Audience.ToString())
                        {
                            string audienceTitle = Regex.Replace((objTactic.Audience.Abbreviation != null ? objTactic.Audience.Abbreviation.Replace(" ", "_") : objTactic.Audience.Title.Replace(" ", "_")), @"[^0-9a-zA-Z_]+", "");
                            customTacticName.Append(audienceTitle + "_");
                        }
                        else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.BusinessUnit.ToString())
                        {
                            string businessunitTitle = Regex.Replace((objTactic.BusinessUnit.Abbreviation != null ? objTactic.BusinessUnit.Abbreviation.Replace(" ", "_") : objTactic.BusinessUnit.Title.Replace(" ", "_")), @"[^0-9a-zA-Z_]+", "");
                            customTacticName.Append(businessunitTitle + "_");
                        }
                        else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.Geography.ToString())
                        {
                            string geographyTitle = Regex.Replace((objTactic.Geography.Abbreviation != null ? objTactic.Geography.Abbreviation.Replace(" ", "_") : objTactic.Geography.Title.Replace(" ", "_")), @"[^0-9a-zA-Z_]+", "");
                            customTacticName.Append(geographyTitle + "_");
                        }
                        else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.Vertical.ToString())
                        {
                            string verticalTitle = Regex.Replace((objTactic.Vertical.Abbreviation != null ? objTactic.Vertical.Abbreviation.Replace(" ", "_") : objTactic.Vertical.Title.Replace(" ", "_")), @"[^0-9a-zA-Z_]+", "");
                            customTacticName.Append(verticalTitle + "_");
                        }
                        else if (objCampaignNameConvention.TableName == Enums.CustomNamingTables.Plan_Campaign_Program_Tactic.ToString())
                        {
                            customTacticName.Append(Regex.Replace((System.Web.HttpUtility.HtmlDecode(objTactic.Title).Replace(" ", "_")), @"[^0-9a-zA-Z_]+", "") + "_");
                        }
                    }
                    if (customTacticName.ToString().Length > 0)
                    {
                        var index = customTacticName.ToString().LastIndexOf('_');
                        if (index > 0)
                        {
                            customTacticName.Remove(index, 1);
                            string replaceMultipleUnderscore= Regex.Replace(customTacticName.ToString(), "_+", "_");
                            customTacticName.Clear();
                            customTacticName.Append(replaceMultipleUnderscore);
                        }
                    }
                }
                else
                {
                    customTacticName.Append(objTactic.Title);
                }
            }

            return customTacticName.ToString();
        }

        #endregion
    }
}
