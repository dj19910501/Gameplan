using System;
using System.Collections.Generic;
using System.Linq;
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
    public class finacemodel
    {
        public string xmlstring { get; set; }
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }
    }


  
}