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
                DateTime CurrentMonthStartDate = new DateTime(now.Year, now.Month, 1);
                DateTime CurrentMonthEndDate = CurrentMonthStartDate.AddMonths(1).AddDays(-1);

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

                var CacheExchangeRate = (RevenuePlanner.Models.CurrencyModel.PlanCurrency)objCache.Returncache(Enums.CacheObject.UserPlanCurrency.ToString());
                if (CacheExchangeRate != null)
                {
                    Sessions.PlanExchangeRate = CacheExchangeRate.ExchangeRate;
                }
                else
                {
                    Sessions.PlanExchangeRate = 1;
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
        public double GetValueByExchangeRate(double DataValue = 0)
        {
            double ExchangeRate = Sessions.PlanExchangeRate;
            double ConvertedValue = DataValue;
            if (DataValue > 0)
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
        public double SetValueByExchangeRate(double DataValue = 0)
        {
            double ExchangeRate = Sessions.PlanExchangeRate;
            double ConvertedValue = DataValue;
            if (DataValue > 0)
            {
                ConvertedValue = Math.Round((DataValue / ExchangeRate), 2);
            }
            return ConvertedValue;
        }
    }
}