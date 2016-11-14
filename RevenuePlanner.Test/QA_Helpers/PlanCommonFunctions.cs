using RevenuePlanner.Helpers;
using RevenuePlanner.Test.QA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            else
            {
                return TacticINQAmount * INQConversion * AQLConversion * TALConversion * TQLConversion * SALConversion * SQLConversion * Convert.ToInt32(drModel["ADS"].ToString());
            }

        }



    }
}
