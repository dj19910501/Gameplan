using RevenuePlanner.Helpers;
using RevenuePlanner.Test.QA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevenuePlanner.Test.MockHelpers;

namespace RevenuePlanner.Test.IntegrationHelpers
{

    public class PlanCommonFunctions
    {
        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();

        public decimal ConvertModelConversionRate(DataRow drModel, string CRName)
        {
            return Convert.ToDecimal(drModel[CRName]);
        }

        public decimal CalculationForTactic(DataRow drModel, decimal TacticINQAmount, string CalculationFor)
        {
            decimal INQConversion = ConvertModelConversionRate(drModel, "INQConversion") / 100;
            decimal AQLConversion = ConvertModelConversionRate(drModel, "AQLConversion") / 100;
            decimal TALConversion = ConvertModelConversionRate(drModel, "TALConversion") / 100;
            decimal TQLConversion = ConvertModelConversionRate(drModel, "TQLConversion") / 100;
            decimal SALConversion = ConvertModelConversionRate(drModel, "SALConversion") / 100;
            decimal SQLConversion = ConvertModelConversionRate(drModel, "SQLConversion") / 100;
            if (CalculationFor == "TQL")
            {
                return TacticINQAmount * INQConversion * AQLConversion * TALConversion;
            }
            else if (CalculationFor == "CW")
            {
                return TacticINQAmount * TQLConversion * SALConversion * SQLConversion;
            }
            else
            {
                return TacticINQAmount * INQConversion * AQLConversion * TALConversion * TQLConversion * SALConversion * SQLConversion * Convert.ToInt32(drModel["ADS"].ToString());
            }

        }

        public void SetSessionData()
        {
            List<int> PlanIds = new List<int>();
            List<int> ReportOwnerIds = new List<int>();
            List<int> ReportTacticTypeIds = new List<int>();
            int PlanId = 0; int ModelId = 0;

            DataTable dtPlan = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Plan$]").Tables[0];
            if (dtPlan.Rows[0]["PlanId"].ToString() != "")
            {
                PlanId = Convert.ToInt32(dtPlan.Rows[0]["PlanId"].ToString());
            }
            // int PlanId = Convert.ToInt32(ConfigurationManager.AppSettings["PlanId"]);
            PlanIds.Add(PlanId);
            Sessions.ReportPlanIds = PlanIds;

            int OwnerId = DataHelper.GetPlanOwnerId(PlanId);
            ReportOwnerIds.Add(OwnerId);
            Sessions.ReportOwnerIds = ReportOwnerIds;

            DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
            if (dtModel.Rows[0]["ModelId"].ToString() != "")
            {
                ModelId = Convert.ToInt32(dtModel.Rows[0]["ModelId"].ToString());
            }
            //  int ModelId = Convert.ToInt32(ConfigurationManager.AppSettings["ModelId"]);
            ReportTacticTypeIds = QA_DataHelper.GetTacticTypeIds(ModelId);
            Sessions.ReportTacticTypeIds = ReportTacticTypeIds;
        }

    }
}
