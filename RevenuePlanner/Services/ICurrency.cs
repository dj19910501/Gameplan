using RevenuePlanner.Models;
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
        double GetValueByExchangeRate(double DataValue = 0, double ExchangeRate = 1);
        double SetValueByExchangeRate(double DataValue = 0, double ExchangeRate = 1);
        double GetReportValueByExchangeRate(DateTime StartDate, double DataValue = 0, int Period = 0);
        double SetReportValueByExchangeRate(DateTime StartDate, double DataValue = 0, int Period = 0);
        List<CurrencyModel.ClientCurrency> GetUserCurrencyMonthwise(string StartDate, string EndDate);
    }
}
