using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevenuePlanner.Services
{
  public  interface IImportData
    {
        DataTable GetPlanBudgetDataByType(DataTable importData, string planBudgetType,bool isMonthly);
    }
}
