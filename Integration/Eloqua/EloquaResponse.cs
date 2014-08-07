using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Web;
using System.Data;
using RevenuePlanner.Models;
using Excel;
using Integration.Helper;

namespace Integration.Eloqua
{
    public class EloquaResponse
    {
        #region Variables

        private static string archiveFolder = "/Archived/";
        private static string rootResponseFolder = ConfigurationManager.AppSettings["EloquaResponseFolderPath"].ToString();
        private static string eloquaCampaignIDColumn = "EloquaCampaignID";
        private static string externalCampaignIDColumn = "ExternalCampaignID";
        private static string eloquaResponseDateTimeColumn = "ResponseDateTime";
        private MRPEntities db = new MRPEntities();

        #endregion

        /// <summary>
        /// Added By Dharmraj, 5-8-2014
        /// </summary>
        /// <param name="_ftpURL"></param>
        /// <param name="_UserName"></param>
        /// <param name="_Password"></param>
        /// <param name="_Port"></param>
        /// <returns>True if SFTP connects successfully otherwise False</returns>
        public bool AuthenticateSFTP(string _ftpURL, string _UserName, string _Password, int _Port)
        {
            try
            {
                //connect SFTP
                Sftp client = new Sftp(_ftpURL, _UserName, _Password);
                client.Connect(_Port);
                return client.Connected;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Added By Dharmraj, 5-8-2014
        /// Function to retrive INQ response from Eloqua
        /// </summary>
        /// <param name="IntegrationInstanceId"></param>
        /// <returns></returns>
        public void GetTacticResponse(int IntegrationInstanceId, Guid _userId, int _integrationInstanceLogId)
        {
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceIdINQ == IntegrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            if (planIds.Count > 0)
            {
                var objIntegrationInstanceExternalServer = db.IntegrationInstanceExternalServers.FirstOrDefault(i => i.IntegrationInstanceId == IntegrationInstanceId);
                string InstanceId = IntegrationInstanceId.ToString();
                string _ftpURL = objIntegrationInstanceExternalServer.SFTPServerName;
                string _UserName = objIntegrationInstanceExternalServer.SFTPUserName;
                string _Password = Common.Decrypt(objIntegrationInstanceExternalServer.SFTPPassword);
                int _Port = Convert.ToInt32(objIntegrationInstanceExternalServer.SFTPPort);
                string SFTPSourcePath = objIntegrationInstanceExternalServer.SFTPFileLocation;
                if (SFTPSourcePath.Substring(SFTPSourcePath.Length - 1) != "/")
                {
                    SFTPSourcePath = SFTPSourcePath + "/";
                }
                string SFTPArchivePath = SFTPSourcePath + archiveFolder;//"Gameplan/" + archiveFolder;
                string localDestpath = rootResponseFolder;
                string extension = string.Empty;
                string filepath = string.Empty;
                ArrayList srclist = new ArrayList();
                ArrayList Listpath = new ArrayList();
                DataTable dt = new DataTable();

                //Create local directory
                if (!Directory.Exists(localDestpath + InstanceId))
                {
                    Directory.CreateDirectory(localDestpath + InstanceId);
                }
                if (!Directory.Exists(localDestpath + InstanceId + archiveFolder))
                {
                    Directory.CreateDirectory(localDestpath + InstanceId + archiveFolder);
                }
                string localRunnungPath = localDestpath + InstanceId + "/";
                string localArchivePath = localDestpath + InstanceId + archiveFolder;

                try
                {
                    Sftp client = new Sftp(_ftpURL, _UserName, _Password);
                    try
                    {
                        //connect SFTP
                        client.Connect(_Port);
                        bool isConnected = client.Connected;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error: Could not connect to Integration Instance External Server", ex.InnerException);
                    }

                    srclist = client.GetFileList(SFTPSourcePath);
                    srclist.Remove(".");
                    srclist.Remove("..");

                    if (srclist.Count > 0)
                    {
                        foreach (var objfiles in srclist)
                        {
                            extension = Path.GetExtension(objfiles.ToString());
                            if (extension.ToLower().Trim() == ".xls" || extension.ToLower().Trim() == ".xlsx" || extension.ToLower().Trim() == ".csv")
                            {
                                client.Get(SFTPSourcePath + "/" + objfiles.ToString(), localRunnungPath);
                                Listpath.Add(localRunnungPath + "/" + objfiles.ToString());
                            }
                        }

                        if (Listpath != null && Listpath.Count > 0)
                        {
                            foreach (string FullfileName in Listpath)
                            {
                                string fileName = System.IO.Path.GetFileName(FullfileName).ToString();
                                string ext = Path.GetExtension(fileName);
                                dt = new DataTable();

                                #region create DataTable from Excel
                                if (ext.ToLower() == ".csv")
                                {
                                    StreamReader sr = new StreamReader(FullfileName);
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
                                    System.IO.FileStream fs = new System.IO.FileStream(FullfileName, System.IO.FileMode.Open);
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
                                #endregion

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    var lstColumns = setarrExcelColumn(dt);
                                    if (lstColumns.Contains(eloquaCampaignIDColumn.ToLower()) && lstColumns.Contains(externalCampaignIDColumn.ToLower()) && lstColumns.Contains(eloquaResponseDateTimeColumn.ToLower()))
                                    {
                                        var lstResult = dt.AsEnumerable().GroupBy(a => new { eloquaId = a[eloquaCampaignIDColumn], externalId = a[externalCampaignIDColumn], date = Convert.ToDateTime(a[eloquaResponseDateTimeColumn]).ToString("MM/yyyy") })
                                                                      .Select(a => new { id = a.Key, items = a.ToList().Count });

                                        List<EloquaResponseModel> lstResponse = new List<EloquaResponseModel>();
                                        foreach (var item in lstResult)
                                        {
                                            lstResponse.Add(new EloquaResponseModel()
                                            {
                                                eloquaTacticId = item.id.eloquaId.ToString(),
                                                externalTacticId = item.id.externalId.ToString(),
                                                peroid = Convert.ToDateTime(item.id.date),
                                                responseCount = item.items
                                            });
                                        }

                                        if (lstResponse.Count > 0)
                                        {
                                            var lstEloquaTacticId = lstResponse.Select(t => t.eloquaTacticId).ToList();
                                            var lstExternalTacticId = lstResponse.Select(t => t.externalTacticId).ToList();
                                            string Stage_INQ = "INQ";
                                            string Stage_ProjectedStageValue = "ProjectedStageValue";
                                            List<string> lstApproveStatus = Common.GetStatusListAfterApproved();
                                            List<Plan_Campaign_Program_Tactic> lstTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                                           (lstExternalTacticId.Contains(tactic.IntegrationInstanceTacticId) || lstEloquaTacticId.Contains(tactic.IntegrationInstanceTacticId)) &&
                                                                                                                                           tactic.IsDeployedToIntegration == true &&
                                                                                                                                           lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                                           tactic.IsDeleted == false &&
                                                                                                                                           tactic.Stage.Code == Stage_INQ).ToList();
                                            // Insert or Update tactic actuals.
                                            foreach (var objTactic in lstTactic)
                                            {
                                                DateTime tacticStartDate = new DateTime(objTactic.StartDate.Year, objTactic.StartDate.Month, 1);
                                                DateTime tacticEndDate = new DateTime(objTactic.EndDate.Year, objTactic.EndDate.Month, 1);
                                                var lstTacticResponse = lstResponse.Where(r => (r.eloquaTacticId == objTactic.IntegrationInstanceTacticId || r.externalTacticId == objTactic.IntegrationInstanceTacticId) &&
                                                                                                r.peroid >= tacticStartDate && r.peroid <= tacticEndDate);
                                                foreach (var item in lstTacticResponse)
                                                {
                                                    string tmpPeriod = "Y" + item.peroid.Month.ToString();
                                                    var objTacticActual = db.Plan_Campaign_Program_Tactic_Actual.FirstOrDefault(a => a.PlanTacticId == objTactic.PlanTacticId && a.Period == tmpPeriod && a.StageTitle == Stage_ProjectedStageValue);
                                                    if (objTacticActual != null)
                                                    {
                                                        objTacticActual.Actualvalue = objTacticActual.Actualvalue + item.responseCount;
                                                        objTacticActual.ModifiedDate = DateTime.Now;
                                                        objTacticActual.ModifiedBy = _userId;
                                                        db.Entry(objTacticActual).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                                        actualTactic.Actualvalue = item.responseCount;
                                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                                        actualTactic.Period = "Y" + item.peroid.Month;
                                                        actualTactic.StageTitle = Stage_ProjectedStageValue;
                                                        actualTactic.CreatedDate = DateTime.Now;
                                                        actualTactic.CreatedBy = _userId;
                                                        db.Entry(actualTactic).State = EntityState.Added;
                                                    }
                                                }

                                                objTactic.LastSyncDate = DateTime.Now;
                                                objTactic.ModifiedDate = DateTime.Now;
                                                objTactic.ModifiedBy = _userId;

                                                // Insert Log
                                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                                instanceTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
                                                instanceTactic.IntegrationInstanceId = IntegrationInstanceId;
                                                instanceTactic.EntityId = objTactic.PlanTacticId;
                                                instanceTactic.EntityType = EntityType.Tactic.ToString();
                                                instanceTactic.Status = StatusResult.Success.ToString();
                                                instanceTactic.Operation = Operation.Import_Actuals.ToString();
                                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                                instanceTactic.CreatedDate = DateTime.Now;
                                                instanceTactic.CreatedBy = _userId;
                                                db.Entry(instanceTactic).State = EntityState.Added;
                                            }

                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Error: Response does not contains EloquaCampaignID or ExternalCampaignID or ResponseDateTime columns.");
                                    }
                                }

                                string ProcssingFilePath = FullfileName;
                                string fileext = Path.GetExtension(fileName);
                                string filen = fileName.Replace(fileext, "");

                                string strDateTime = System.DateTime.Now.Year.ToString() + "_" + System.DateTime.Now.Month.ToString() + "_" + System.DateTime.Now.Day.ToString() + "_" + System.DateTime.Now.Hour.ToString() + "_" + System.DateTime.Now.Minute.ToString() + "_" + System.DateTime.Now.Second.ToString();
                                string ArchiveFilePath = localArchivePath + filen + "_archived_" + strDateTime + fileext;
                                string SFTPArchiveFilePathNew = SFTPArchivePath + filen + "_archived_" + strDateTime + fileext;
                                if (File.Exists(ProcssingFilePath))
                                {
                                    System.IO.File.Copy(ProcssingFilePath, ArchiveFilePath, true);
                                    File.Delete(ProcssingFilePath);

                                    try
                                    {
                                        client.Mkdir(SFTPArchivePath.Remove(SFTPArchivePath.Length - 1));
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    client.Put(ArchiveFilePath, SFTPArchiveFilePathNew);

                                    var prop = client.GetType().GetProperty("SftpChannel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    var methodInfo = prop.GetGetMethod(true);
                                    var sftpChannel = methodInfo.Invoke(client, null);
                                    string rmfile = SFTPSourcePath + "/" + filen + fileext;
                                    ((ChannelSftp)sftpChannel).rm(rmfile);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #region Functions
        public List<string> setarrExcelColumn(DataTable dt)
        {
            int columnCount = dt.Columns.Count;
            List<string> ExcelColumns = new List<string>();
            for (int i = 0; i <= dt.Columns.Count - 1; i++)
            {
                string tempColumnName = Convert.ToString(dt.Columns[i]);
                tempColumnName = tempColumnName.Trim().ToLower();
                ExcelColumns.Add(tempColumnName);
            }
            return ExcelColumns;
        }
        #endregion
    }
}
