using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevenuePlanner.Services
{
    public interface ICurrency
    {
        void SetUserCurrencyCache(Guid ClientId, Guid UserId);
        double GetValueByExchangeRate(double DataValue = 0);
        double SetValueByExchangeRate(double DataValue = 0);
        double GetReportValueByExchangeRate(DateTime StartDate, double DataValue = 0, int Period = 0);
        double SetReportValueByExchangeRate(DateTime StartDate, double DataValue = 0, int Period = 0);
    }
}
