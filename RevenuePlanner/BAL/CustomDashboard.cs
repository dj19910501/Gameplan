using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Models;
using System.Data;
using RevenuePlanner.Helpers;

namespace RevenuePlanner.BAL
{
    public class CustomDashboard
    {
        public Custom_Dashboard GetMainDashBoardInfo(int DashboardId)
        {
            DataSet ds = new DataSet();
            StoredProcedure sp = new StoredProcedure();
            Custom_Dashboard model = new Custom_Dashboard();
            ds = sp.GetDashboardContent(0, Convert.ToInt32(DashboardId), 0);

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.Id = dt.Rows[0]["Id"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[0]["Id"]);
                    model.Name = Convert.ToString(dt.Rows[0]["DisplayName"]);
                    model.Rows = dt.Rows[0]["Rows"] == DBNull.Value ? 2 : Convert.ToInt32(dt.Rows[0]["Rows"]);
                    model.Columns = dt.Rows[0]["Columns"] == DBNull.Value ? 2 : Convert.ToInt32(dt.Rows[0]["Columns"]);

                    if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {
                        DataView dvDashContents = new DataView(ds.Tables[1]);
                        dvDashContents.RowFilter = "DashboardId = " + model.Id;
                        DataTable dtContents = new DataTable();
                        dtContents = dvDashContents.ToTable();
                        model.DashboardContent = GetDashboardComponents(model.Id, dtContents);
                    }
                }
            }

            return model;
        }

        public List<DashboardContentModel> GetDashboardComponents(int dashboardID, DataTable dt)
        {
            List<DashboardContentModel> model = new List<DashboardContentModel>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    DashboardContentModel tmp = new DashboardContentModel();
                    DataView dv = new DataView(dt);
                    dv.RowFilter = "DashboardContentId = " + Convert.ToInt32(dr["DashboardContentId"]);
                    DataTable dtDashContents = new DataTable();
                    dtDashContents = dv.ToTable();
                    tmp = SetReportGraphModelData(dashboardID, dtDashContents);
                    model.Add(tmp);
                }
            }
            return model;
        }

        public DashboardContentModel SetReportGraphModelData(int DashboardId, DataTable dt)
        {
            DashboardContentModel tmp = new DashboardContentModel();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    tmp.DashboardContentId = dr["DashboardContentId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["DashboardContentId"]);
                    tmp.Height = dr["CalculatedHeight"] == DBNull.Value ? 0 : Convert.ToInt32(dr["CalculatedHeight"]);
                    tmp.Width = dr["CalculatedWidth"] == DBNull.Value ? Convert.ToDecimal(0) : Convert.ToDecimal(dr["CalculatedWidth"]);
                    tmp.ReportID = dr["ReportGraphId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ReportGraphId"]);
                    tmp.DisplayName = dr["DisplayName"] == DBNull.Value ? "" : Convert.ToString(dr["DisplayName"]);
                    tmp.DashboardId = DashboardId;
                }
            }

            return tmp;
        }

        public string GetLatestLog()
        {
            StoredProcedure sp = new StoredProcedure();
            string strReportGraph = "SELECT EndDate = CONVERT(NVARCHAR,ISNULL(EndDate,GETDATE()), 101) + '#' + LTRIM(RIGHT(CONVERT(VARCHAR(20), ISNULL(EndDate,GETDATE()), 100), 7))+ '#' + RIGHT(CAST(SYSDATETIMEOFFSET() AS NVARCHAR(50)),6) FROM AggregationProcessLog WHERE Status = 'SUCCESS' ORDER BY CreatedDate Desc";
            string EndDate = sp.GetColumnValue(strReportGraph);
            return EndDate;
        }

        public List<CustomDashboardModel> GetCustomDashboardsClientwise(Guid UserId, Guid ClientId)
        {
            List<CustomDashboardModel> model = new List<CustomDashboardModel>();

            StoredProcedure sp = new StoredProcedure();
            return sp.GetCustomDashboardsClientwise(UserId, ClientId);
            
        }
    }


}