using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RevenuePlanner.Models
{
    public class FinanceModelHeaders
    {
        public double Budget { get; set; }
        public double Forecast { get; set; }
        public double Planned { get; set; }
        public double Actual { get; set; }
        public string BudgetTitle { get; set; }
        public string ForecastTitle { get; set; }
        public string PlannedTitle { get; set; }
        public string ActualTitle { get; set; }

    }
    public class FinanceModel
    {
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }
        public DhtmlXGridRowModel DhtmlXGridRowModelObj { get; set; }

    }
    //public class FinanceParentChildModel
    //{
    //    public Int32 Id { get; set; }

    //    public String Name { get; set; }

    //    public List<double?> Budget { get; set; }

    //    public List<double?> ForeCast { get; set; }

    //    public double BudgetTotal { get; set; }

    //    public double ForeCastTotal { get; set; }

    //    public IEnumerable<FinanceParentChildModel> Children { get; set; }
    //}
    public class DhtmlXGridRowModel
    {
        public List<DhtmlxGridRowDataModel> rows { get; set; }
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }// Add By Nishant Sheth
    }
    public class DhtmlxGridRowDataModel
    {
        public string id { get; set; }
        public List<string> data { get; set; }
        public userdata userdata { get; set; }
        public List<row_attrs> row_attrs { get; set; }
        public List<DhtmlxGridRowDataModel> rows { get; set; }
        public string Detailid { get; set; }
    }
   
    public class userdata
    {
        public string id { get; set; }
        public string idwithName { get; set; }
        public string row_attrs { get; set; }
        public string row_locked { get; set; }
        public string isTitleEdit { get; set; }
    }
    public class row_attrs
    {
        public string id { get; set; }
    }
    public class BudgetAmount
    {
        public List<double?> Budget { get; set; }
        public List<double?> ForeCast { get; set; }
        public List<double?> Plan { get; set; }
        public List<double?> Actual { get; set; }
    }
    public class DeleteRowID
    {
        public int Id { get; set; }

    }
}