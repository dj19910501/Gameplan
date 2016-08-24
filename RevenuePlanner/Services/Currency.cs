using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace RevenuePlanner.Services
{
    public class Currency : ICurrency
    {
        private MRPEntities objDbMrpEntities;
        private CacheObject objCache = new CacheObject();
        public Currency()
        {
            objDbMrpEntities = new MRPEntities();
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// #2496 : Set the cache of currency exchange rate for users's preferd currency
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="UserId"></param>
        public void SetUserCurrencyCache(Guid ClientId, Guid UserId)
        {
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                BDSService.Currency objUserCurrency = new BDSService.Currency();

                DateTime now = DateTime.Now;
                DateTime CurrentMonthStartDate = GetFirstDayOfMonth(now.Month, now.Year);
                DateTime CurrentMonthEndDate = GetLastDayOfMonth(now.Month, now.Year);

                objUserCurrency = objBDSServiceClient.GetCurrencyExchangeRate(ClientId, UserId); // Call the BDS Sevice for get exchange rate 

                if (objUserCurrency.UserCurrency != null)
                {
                    var Currency = objUserCurrency.UserCurrency;
                    // Get Planning Exchange rate // With Current Start Date and end date
                    RevenuePlanner.Models.CurrencyModel.PlanCurrency UserPlanCurrency = Currency.CurrencyExchangeRate
                        .Where(curr => curr.CurrencyCode == Currency.UserPreferredCurrencyCode
                                && curr.Component == Enums.CurrencyComponent.Plan.ToString().ToLower()
                                && (curr.StartDate >= CurrentMonthStartDate && curr.EndDate <= CurrentMonthEndDate))
                                .Select(curr =>
                                    new RevenuePlanner.Models.CurrencyModel.PlanCurrency
                                    {
                                        CurrencyCode = curr.CurrencyCode,
                                        CurrencySymbol = curr.CurrencySymbol,
                                        ExchangeRate = curr.ExchangeRate
                                    }).FirstOrDefault();

                    if (UserPlanCurrency != null)
                    {
                        objCache.AddCache(Convert.ToString(Enums.CacheObject.UserPlanCurrency), UserPlanCurrency);
                        Sessions.PlanExchangeRate = UserPlanCurrency.ExchangeRate;
                    }
                    else
                    {
                        Sessions.PlanExchangeRate = 1;
                    }

                    // Get Reporting Exchange rate
                    List<RevenuePlanner.Models.CurrencyModel.ClientCurrency> UserReportCurrency = Currency.CurrencyExchangeRate
                        .Where(curr => curr.CurrencyCode == Currency.UserPreferredCurrencyCode
                                && curr.Component == Enums.CurrencyComponent.Report.ToString().ToLower())
                                .Select(curr =>
                                    new RevenuePlanner.Models.CurrencyModel.ClientCurrency
                                    {
                                        ClientId = curr.ClientId,
                                        Component = curr.Component,
                                        CurrencyCode = curr.CurrencyCode,
                                        CurrencySymbol = curr.CurrencySymbol,
                                        EndDate = curr.EndDate,
                                        ExchangeRate = curr.ExchangeRate,
                                        StartDate = curr.StartDate
                                    }).ToList();

                    if (UserReportCurrency != null)
                    {
                        objCache.AddCache(Enums.CacheObject.ListUserReportCurrency.ToString(), UserReportCurrency);
                    }
                    Sessions.PlanCurrencySymbol = Currency.UserPreferredCurrencySymbol;
                }
                else
                {
                    Sessions.PlanExchangeRate = 1;
                    Sessions.PlanCurrencySymbol = Convert.ToString(Enums.CurrencySymbolsValues[Enums.CurrencySymbols.USD.ToString()]);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Convert Value From USD dollar to Other currency
        /// </summary>
        /// <param name="DataValue"></param>
        /// <param name="ExchangeRate"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetValueByExchangeRate(double DataValue = 0, double ExchangeRate = 1)
        {
            double ConvertedValue = DataValue;
            if (ExchangeRate != 0)
            {
                ConvertedValue = Math.Round((DataValue * ExchangeRate), 2);
            }
            return ConvertedValue;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Convert Value From other currency to USD Dollar
        /// </summary>
        /// <param name="DataValue"></param>
        /// <param name="ExchangeRate"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double SetValueByExchangeRate(double DataValue = 0, double ExchangeRate = 1)
        {
            double ConvertedValue = DataValue;
            if (ExchangeRate != 0)
            {
                ConvertedValue = DataValue / ExchangeRate;
            }
            return ConvertedValue;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get Exchange rate with start date, end date 
        /// </summary>
        /// <param name="DataValue"></param>
        /// <param name="StartDate"></param>
        /// <param name="Period"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetReportValueByExchangeRate(DateTime StartDate, double DataValue = 0, int Period = 0)
        {
            double ConvertedValue = DataValue;
            DateTime date = StartDate;
            DateTime MonthStartDate, MonthEndDate = DateTime.Now.Date;
            if (StartDate != null)
            {
                MonthStartDate = GetFirstDayOfMonth(Period, StartDate.Year);
                MonthEndDate = GetLastDayOfMonth(MonthStartDate.Month, MonthStartDate.Year);
                List<RevenuePlanner.Models.CurrencyModel.ClientCurrency> objReportCache = (List<RevenuePlanner.Models.CurrencyModel.ClientCurrency>)objCache.Returncache(Enums.CacheObject.ListUserReportCurrency.ToString());
                if (objReportCache != null)
                {
                    var GetExchangeRate = objReportCache.Where(a => a.StartDate >= MonthStartDate && a.EndDate <= MonthEndDate)
                                                .Select(a => a)
                                                .FirstOrDefault();
                    if (GetExchangeRate != null)
                    {
                        ConvertedValue = Math.Round((DataValue * GetExchangeRate.ExchangeRate), 2);
                    }
                }
            }
            return ConvertedValue;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set Exchange rate with start date, end date 
        /// </summary>
        /// <param name="DataValue"></param>
        /// <param name="StartDate"></param>
        /// <param name="Period"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double SetReportValueByExchangeRate(DateTime StartDate, double DataValue = 0, int Period = 0)
        {
            double ConvertedValue = DataValue;
            DateTime date = StartDate;
            DateTime MonthStartDate, MonthEndDate = DateTime.Now.Date;
            if (StartDate != null)
            {
                MonthStartDate = GetFirstDayOfMonth(Period, StartDate.Year);
                MonthEndDate = GetLastDayOfMonth(MonthStartDate.Month, MonthStartDate.Year);
                List<RevenuePlanner.Models.CurrencyModel.ClientCurrency> objReportCache = (List<RevenuePlanner.Models.CurrencyModel.ClientCurrency>)objCache.Returncache(Enums.CacheObject.ListUserReportCurrency.ToString());
                if (objReportCache != null)
                {
                    var GetExchangeRate = objReportCache.Where(a => a.StartDate >= MonthStartDate && a.EndDate <= MonthEndDate)
                                                .Select(a => a)
                                                .FirstOrDefault();
                    if (GetExchangeRate != null)
                    {
                        ConvertedValue = DataValue / GetExchangeRate.ExchangeRate;
                    }
                }
            }
            return ConvertedValue;
        }

        /// <summary>
        /// Get the first day of the month for a
        /// month passed by it's integer value
        /// </summary>
        /// <param name="iMonth"></param>
        /// <param name="iYear"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTime GetFirstDayOfMonth(int iMonth, int iYear)
        {
            // set return value to the last day of the month
            // for any date passed in to the method
            // If multi year months
            int NumPeriod = iMonth / 13;
            if (NumPeriod >= 1)
            {
                iYear += NumPeriod;
                iMonth = Common.ReportMultiyearMonth(iMonth, NumPeriod);
            }
            // End multi year
            // create a datetime variable set to the passed in date
            DateTime dtFrom = new DateTime(iYear, iMonth, 1);

            // remove all of the days in the month
            // except the first day and set the
            // variable to hold that date
            dtFrom = dtFrom.AddDays(-(dtFrom.Day - 1));

            // return the first day of the month
            return dtFrom;
        }

        /// <summary>
        /// Get the last day of a month expressed by it's
        /// integer value
        /// </summary>
        /// <param name="iMonth"></param>
        /// <param name="iYear"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTime GetLastDayOfMonth(int iMonth, int iYear)
        {

            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime dtTo = new DateTime(iYear, iMonth, 1);

            // overshoot the date by a month
            dtTo = dtTo.AddMonths(1);

            // remove all of the days in the next month
            // to get bumped down to the last day of the 
            // previous month
            dtTo = dtTo.AddDays(-(dtTo.Day));

            // return the last day of the month
            return dtTo;

        }

        /// <summary>
        /// Add By Nandish Shah
        /// Get User Currency Month wise
        /// </summary>
        /// <returns>List<CurrencyModel.ClientCurrency></returns>
        public List<CurrencyModel.ClientCurrency> GetUserCurrencyMonthwise(string StartDate, string EndDate)
        {
            List<RevenuePlanner.Models.CurrencyModel.ClientCurrency> UserReportCurrency = (List<RevenuePlanner.Models.CurrencyModel.ClientCurrency>)objCache.Returncache(Enums.CacheObject.ListUserReportCurrency.ToString());
            int TotalMonths = (DateTime.Parse(EndDate).Month) + 12 * (DateTime.Parse(EndDate).Year - DateTime.Parse(StartDate).Year);
            List<RevenuePlanner.Models.CurrencyModel.ClientCurrency> MonthWiseUserReportCurrency = new List<CurrencyModel.ClientCurrency>();

            if (UserReportCurrency != null && UserReportCurrency.Count > 0)
            {
                for (int i = DateTime.Parse(StartDate).Month; i <= TotalMonths; i++)
                {
                    int TotalMonth = 12;
                    int YearNo = 0;
                    if (i % TotalMonth != 0)
                    {
                        YearNo = i / TotalMonth;
                    }
                    else
                    {
                        YearNo = (i - 1) / TotalMonth;
                    }
                    DateTime dt = new DateTime(DateTime.Parse(StartDate).Year + YearNo, (i % TotalMonth) == 0 ? 12 : (i % TotalMonth), 1);
                    if (!UserReportCurrency.Where(w => w.StartDate == dt).Any())
                    {
                        CurrencyModel.ClientCurrency cc = new CurrencyModel.ClientCurrency();
                        cc.StartDate = dt;
                        cc.EndDate = dt.AddMonths(1).AddDays(-1);
                        cc.CurrencySymbol = UserReportCurrency[0].CurrencySymbol;
                        cc.ExchangeRate = 1.0;
                        MonthWiseUserReportCurrency.Add(cc);
                    }
                    else
                    {
                        CurrencyModel.ClientCurrency cc = new CurrencyModel.ClientCurrency();
                        cc = (CurrencyModel.ClientCurrency)UserReportCurrency.Where(w => w.StartDate == dt).FirstOrDefault();
                        MonthWiseUserReportCurrency.Add(cc);
                    }
                }
            }
            else
            {
                for (int i = DateTime.Parse(StartDate).Month; i <= TotalMonths; i++)
                {
                    int TotalMonth = 12;
                    int YearNo = 0;
                    if (i % TotalMonth != 0)
                    {
                        YearNo = i / TotalMonth;
                    }
                    else
                    {
                        YearNo = (i - 1) / TotalMonth;
                    }
                    DateTime dt = new DateTime(DateTime.Parse(StartDate).Year + YearNo, (i % TotalMonth) == 0 ? 12 : (i % TotalMonth), 1);
                    CurrencyModel.ClientCurrency cc = new CurrencyModel.ClientCurrency();
                    cc.StartDate = dt;
                    cc.EndDate = dt.AddMonths(1).AddDays(-1);
                    cc.CurrencySymbol = "";
                    cc.ExchangeRate = 1.0;
                    MonthWiseUserReportCurrency.Add(cc);
                }
            }
            return MonthWiseUserReportCurrency;
        }

    }
}