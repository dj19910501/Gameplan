using RevenuePlanner.Models;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Net;
using RevenuePlanner.BAL;
using System.Collections.ObjectModel;
using System.Configuration;
using Elmah;

namespace RevenuePlanner.Helpers
{
    public static class HtmlHelpers
    {
        public static RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();//Added by Rahul Shah for PL #2498.
        public static double PlanExchangeRate = Sessions.PlanExchangeRate;
        /// <summary>
        /// To truncate the string 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Truncate(this HtmlHelper helper, string input, int length)
        {
            if (input.Length <= length)
            {
                return input;
            }
            else
            {
                return input.Substring(0, length) + "...";
            }

        }
        /// <summary>
        /// function to generate model funnel block
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="MSize"></param>
        /// <returns>MvcHtmlString</returns>
        /// datatype of MSize,TSize and SSize is modified from int to double
        public static MvcHtmlString GenerateFunnel(this HtmlHelper helper, double MSize)
        {

            StringBuilder sb = new StringBuilder();
            StringBuilder sbHidden = new StringBuilder();
            PlanExchangeRate = Sessions.PlanExchangeRate;
            TagBuilder textboxMarketingDealSize = new TagBuilder("input");
            textboxMarketingDealSize.Attributes.Add("type", "text");
            textboxMarketingDealSize.Attributes.Add("id", "MarketingDealSize");
            textboxMarketingDealSize.Attributes.Add("name", "txtMarketing");
            textboxMarketingDealSize.Attributes.Add("datadefault", "MS");
            textboxMarketingDealSize.Attributes.Add("datasubline", "For example: <em>" + Sessions.PlanCurrencySymbol + "100,000</em>"); //Modified by Rahul Shah for PL #2498.
            if (MSize > 0)
            {
                //Modified by Rahul Shah for PL #2498.
                textboxMarketingDealSize.Attributes.Add("value", objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(MSize)), PlanExchangeRate).ToString());
                textboxMarketingDealSize.Attributes.Add("placeholder", objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(MSize)), PlanExchangeRate).ToString());
            }
            else
            {
                textboxMarketingDealSize.Attributes.Add("value", "");
                textboxMarketingDealSize.Attributes.Add("placeholder", "0");
            }
            textboxMarketingDealSize.Attributes.Add("formatType", "currency_dollar");
            textboxMarketingDealSize.Attributes.Add("maxlength", Common.maxLengthDollar);
            textboxMarketingDealSize.Attributes.Add("isedit", "true");
            sb.Append(Common.objCached.MarketingDealDescription + "<br>");
            sb.Replace("#MarketingDealSize", textboxMarketingDealSize.ToString());

            return new MvcHtmlString(sb.ToString());
        }

        #region Manoj Limbachiya TFS:263

        /// <summary>
        /// Helper for INQ lable client wise
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="OriginalText"></param>
        /// <returns></returns>
        public static string LabelForINQ(this HtmlHelper helper, string OriginalText)
        {
            string StageLabel = Common.GetLabel(Common.StageModeINQ);
            if (string.IsNullOrEmpty(StageLabel))
            {
                return OriginalText;
            }
            else
            {
                return StageLabel;
            }
        }

        /// <summary>
        /// Helper for MQL lable client wise
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="OriginalText"></param>
        /// <returns></returns>
        public static string LabelForMQL(this HtmlHelper helper, string OriginalText)
        {
            string StageLabel = Common.GetLabel(Common.StageModeMQL);
            if (string.IsNullOrEmpty(StageLabel))
            {
                return OriginalText;
            }
            else
            {
                return StageLabel;
            }
        }

        /// <summary>
        /// Helper for CW lable client wise
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="OriginalText"></param>
        /// <returns></returns>
        public static string LabelForCW(this HtmlHelper helper, string OriginalText)
        {
            string StageLabel = Common.GetLabel(Common.StageModeCW);
            if (string.IsNullOrEmpty(StageLabel))
            {
                return OriginalText;
            }
            else
            {
                return StageLabel;
            }
        }

        //Added By : Kalpesh Sharma :: #607 Code optimization :: Make a control for bind the Budget Allocation Controls
        /// <summary>
        /// Method for bind control of Budget Allocation Controls     
        /// </summary>
        /// <param name="isMonthlyAllocation"></param>
        /// <returns></returns>
        public static MvcHtmlString GenerateBudgetAllocationControl(string isMonthlyAllocation, int YearDiffrence = 0, int StartYear = 0)
        {
            string[] lstMonths = "Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec".Split(',');

            string[] lstQuarters = "Q1,Q2,Q3,Q4".Split(',');

            string QuarterPrefix = "Q";

            string sb = string.Empty;

            int month = 0;

            int baseYear = StartYear;

            if (isMonthlyAllocation == Enums.PlanAllocatedBy.months.ToString())
            {
                // Change by Nishant Sheth
                // Desc ::#1765- to create month/quarter view as per year diffrence
                for (int i = 0; i < ((YearDiffrence + 1)); i++)
                {
                    if ((i + 1) % 2 == 0)
                    {
                        month = month + 12;
                    }
                    baseYear = baseYear + i;
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Jan.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Jan) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Feb.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Feb) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Mar.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Mar) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Apr.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Apr) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.May.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.May) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Jun.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Jun) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Jul.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Jul) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Aug.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Aug) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Sep.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Sep) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Oct.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Oct) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Nov.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Nov) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + Enums.ReportMonthDisplayFinance.Dec.ToString() + " - " + baseYear + "</span><span class=\"light-blue-background\"><input id=\"Y" + ((Convert.ToInt32(Enums.ReportMonthDisplayFinance.Dec) + 1) + month) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";



                }
            }
            else
            {
                int quarterCounter = 1;
                // Change by Nishant Sheth
                // Desc ::#1765- to create month/quarter view as per year diffrence
                bool nextyearflag = false;
                int y = 0;
                int Quarteryear = StartYear;
                for (int i = 0; i < (4 * (YearDiffrence + 1)); i++)
                {
                    if (i % 4 == 0 && i != 0)
                    {
                        Quarteryear += 1;
                        nextyearflag = true;
                    }
                    if (nextyearflag)
                    {
                        sb += "<div class=\"budget-month\"><span class=\"month\">" + Convert.ToString(QuarterPrefix + (y + 1)) + "-" + Quarteryear + "</span><span class=\"light-blue-background\"><input id=\"Y" + quarterCounter + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\" /></span></div>";
                        y++;
                    }
                    else
                    {
                        sb += "<div class=\"budget-month\"><span class=\"month\">" + Convert.ToString(QuarterPrefix + (i + 1)) + "-" + Quarteryear + "</span><span class=\"light-blue-background\"><input id=\"Y" + quarterCounter + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\" /></span></div>";

                    }

                    quarterCounter = quarterCounter + 3;
                }
            }

            return new MvcHtmlString(sb.ToString());
        }

        #endregion

        #region Advance Budgeting

        static string formatThousand = "#,#0.##";
        static string budgetError = " budgetError";

        #region Column1

        /// <summary>
        /// Render activity names for all campaigns
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityCampaign(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string tab = "1")
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("campaign-row");


                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", activityType + c.ActivityId);
                div.AddCssClass("campaignLevel");
                TagBuilder aLink = new TagBuilder("a");
                if (model.Any(p => p.ActivityType == ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId))
                {
                    TagBuilder aAccordian = new TagBuilder("a");
                    aAccordian.AddCssClass("accordionClick");
                    div.InnerHtml = aAccordian.ToString();
                    aLink.Attributes.Add("style", "cursor:pointer;");
                }
                else
                {
                    aLink.Attributes.Add("style", "padding-left:20px;cursor:pointer;");
                }
                if (tab == "2")
                    aLink.Attributes.Add("id", c.Id);
                else
                    aLink.Attributes.Add("id", c.ActivityId);

                aLink.Attributes.Add("linktype", "campaign");
                aLink.InnerHtml = c.ActivityName;

                div.InnerHtml += aLink.ToString();

                td.InnerHtml = div.ToString();

                td.InnerHtml += ActivityProgram(helper, ActivityType.ActivityProgram, c.ActivityId, model, tab).ToString();
                tr.InnerHtml = td.ToString();
                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Render activity names for all children
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityProgram(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string tab = "2")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            string childActivity = "tactic";
            if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                childActivity = "tactic";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
                childActivity = "lineitem";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
                childActivity = "";
            }

            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    divProgram.AddCssClass(innerClass);

                    TagBuilder aLink = new TagBuilder("a");
                    if (model.Any(p1 => p1.ActivityType == childActivity && p1.ParentActivityId == p.ActivityId))
                    {
                        TagBuilder aAccordian = new TagBuilder("a");
                        aAccordian.AddCssClass("accordionClick");
                        divProgram.InnerHtml = aAccordian.ToString();
                        aLink.Attributes.Add("style", "cursor:pointer;");
                    }
                    else
                    {
                        aLink.Attributes.Add("style", "padding-left:20px;cursor:pointer;");
                    }
                    aLink.InnerHtml = p.ActivityName;

                    if (tab == "2")
                        aLink.Attributes.Add("id", p.Id);
                    else
                        aLink.Attributes.Add("id", p.ActivityId);

                    aLink.Attributes.Add("linktype", activityType);

                    divProgram.InnerHtml += aLink.ToString();

                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ActivityProgram(helper, ActivityType.ActivityTactic, p.ActivityId, model, tab).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ActivityProgram(helper, ActivityType.ActivityLineItem, p.ActivityId, model).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        #endregion

        #region Column2

        /// <summary>
        /// Render month header and plans month values
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="activityId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static MvcHtmlString PlanMonth(this HtmlHelper helper, string activityType, string activityId, BudgetMonth obj, BudgetMonth parent, BudgetMonth budgetMonth, string allocatedBy, string strTab)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
            {
                double mainBudget = 0;
                double allocated = 0;
                double monthlyPlanValue = 0;
                for (int i = 1; i <= 12; i++)
                {
                    DateTime dt = new DateTime(2012, i, 1);
                    TagBuilder tdHeader = new TagBuilder("td");
                    //tdHeader.AddCssClass("event-row");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");

                    TagBuilder divValue = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divHeader.InnerHtml = dt.ToString("MMM").ToUpper();
                    divValue.Attributes.Add("id", activityType + activityId);
                    string className = "event-row";
                    if (i == 1)
                    {
                        mainBudget = budgetMonth.BudgetY1;
                        allocated = parent.BudgetY1;
                        monthlyPlanValue = obj.BudgetY1;
                    }
                    else if (i == 2)
                    {
                        mainBudget = budgetMonth.BudgetY2;
                        allocated = parent.BudgetY2;
                        monthlyPlanValue = obj.BudgetY2;
                    }
                    else if (i == 3)
                    {
                        mainBudget = budgetMonth.BudgetY3;
                        allocated = parent.BudgetY3;
                        monthlyPlanValue = obj.BudgetY3;
                    }
                    else if (i == 4)
                    {
                        mainBudget = budgetMonth.BudgetY4;
                        allocated = parent.BudgetY4;
                        monthlyPlanValue = obj.BudgetY4;
                    }
                    else if (i == 5)
                    {
                        mainBudget = budgetMonth.BudgetY5;
                        allocated = parent.BudgetY5;
                        monthlyPlanValue = obj.BudgetY5;
                    }
                    else if (i == 6)
                    {
                        mainBudget = budgetMonth.BudgetY6;
                        allocated = parent.BudgetY6;
                        monthlyPlanValue = obj.BudgetY6;
                    }
                    else if (i == 7)
                    {
                        mainBudget = budgetMonth.BudgetY7;
                        allocated = parent.BudgetY7;
                        monthlyPlanValue = obj.BudgetY7;
                    }
                    else if (i == 8)
                    {
                        mainBudget = budgetMonth.BudgetY8;
                        allocated = parent.BudgetY8;
                        monthlyPlanValue = obj.BudgetY8;
                    }
                    else if (i == 9)
                    {
                        mainBudget = budgetMonth.BudgetY9;
                        allocated = parent.BudgetY9;
                        monthlyPlanValue = obj.BudgetY9;
                    }
                    else if (i == 10)
                    {
                        mainBudget = budgetMonth.BudgetY10;
                        allocated = parent.BudgetY10;
                        monthlyPlanValue = obj.BudgetY10;
                    }
                    else if (i == 11)
                    {
                        mainBudget = budgetMonth.BudgetY11;
                        allocated = parent.BudgetY11;
                        monthlyPlanValue = obj.BudgetY11;
                    }
                    else if (i == 12)
                    {
                        mainBudget = budgetMonth.BudgetY12;
                        allocated = parent.BudgetY12;
                        monthlyPlanValue = obj.BudgetY12;
                    }

                    divValue.Attributes.Add("mainbudget", mainBudget.ToString(formatThousand));
                    divValue.Attributes.Add("allocated", allocated.ToString(formatThousand));
                    divValue.InnerHtml = monthlyPlanValue.ToString(formatThousand);
                    if (strTab == "1" && mainBudget < monthlyPlanValue)
                    {
                        span.AddCssClass("red-corner-budget clickme");
                        span.Attributes.Add("rel", "#loadme");
                        span.Attributes.Add("mnth", Enums.YearMonths[i]);
                    }

                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();
                    if (strTab == "1")
                    {
                        divValue.AddCssClass("planLevel clueplanned");
                    }
                    divValue.InnerHtml += span.ToString();
                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                }
            }
            else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
            {
                double mainBudget = 0;
                double allocated = 0;
                double monthlyPlanValue = 0;
                int quarterCounter = 1;
                for (int i = 1; i <= 11; i += 3)
                {
                    string className = "event-row";
                    TagBuilder tdHeader = new TagBuilder("td");
                    //tdHeader.AddCssClass("event-row");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");
                    //tdValue.AddCssClass("campaign-row");
                    TagBuilder divValue = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divHeader.InnerHtml = "Q" + quarterCounter;
                    divValue.Attributes.Add("id", activityType + activityId);
                    if (i == 1)
                    {
                        mainBudget = budgetMonth.BudgetY1;
                        allocated = parent.BudgetY1;
                        monthlyPlanValue = obj.BudgetY1;
                    }
                    else if (i == 4)
                    {
                        mainBudget = budgetMonth.BudgetY4;
                        allocated = parent.BudgetY4;
                        monthlyPlanValue = obj.BudgetY4;
                    }
                    else if (i == 7)
                    {
                        mainBudget = budgetMonth.BudgetY7;
                        allocated = parent.BudgetY7;
                        monthlyPlanValue = obj.BudgetY7;
                    }
                    else if (i == 10)
                    {
                        mainBudget = budgetMonth.BudgetY10;
                        allocated = parent.BudgetY10;
                        monthlyPlanValue = obj.BudgetY10;
                    }
                    divValue.Attributes.Add("mainbudget", mainBudget.ToString(formatThousand));
                    divValue.Attributes.Add("allocated", allocated.ToString(formatThousand));
                    divValue.InnerHtml = monthlyPlanValue.ToString(formatThousand);
                    if (strTab == "1" && mainBudget < monthlyPlanValue)
                    {
                        span.AddCssClass("red-corner-budget clickme");
                        span.Attributes.Add("rel", "#loadme");
                        span.Attributes.Add("mnth", Enums.YearQuarters[i]);
                    }

                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();
                    if (strTab == "1")
                    {
                        divValue.AddCssClass("planLevel clueplanned");
                    }
                    divValue.InnerHtml += span.ToString();
                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                    quarterCounter += 1;
                }
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    DateTime dt = new DateTime(2012, i, 1);
                    TagBuilder tdHeader = new TagBuilder("td");
                    //tdHeader.AddCssClass("event-row");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");

                    TagBuilder divValue = new TagBuilder("div");
                    divHeader.InnerHtml = dt.ToString("MMM").ToUpper();
                    divValue.Attributes.Add("id", activityType + activityId);
                    string className = "event-row";
                    if (strTab == "2")
                    {
                        if (i == 1)
                        {
                            divValue.InnerHtml = obj.BudgetY1.ToString(formatThousand);

                        }
                        else if (i == 2)
                        {
                            divValue.InnerHtml = obj.BudgetY2.ToString(formatThousand);

                        }
                        else if (i == 3)
                        {
                            divValue.InnerHtml = obj.BudgetY3.ToString(formatThousand);

                        }
                        else if (i == 4)
                        {
                            divValue.InnerHtml = obj.BudgetY4.ToString(formatThousand);

                        }
                        else if (i == 5)
                        {
                            divValue.InnerHtml = obj.BudgetY5.ToString(formatThousand);

                        }
                        else if (i == 6)
                        {
                            divValue.InnerHtml = obj.BudgetY6.ToString(formatThousand);

                        }
                        else if (i == 7)
                        {
                            divValue.InnerHtml = obj.BudgetY7.ToString(formatThousand);

                        }
                        else if (i == 8)
                        {
                            divValue.InnerHtml = obj.BudgetY8.ToString(formatThousand);

                        }
                        else if (i == 9)
                        {
                            divValue.InnerHtml = obj.BudgetY9.ToString(formatThousand);

                        }
                        else if (i == 10)
                        {
                            divValue.InnerHtml = obj.BudgetY10.ToString(formatThousand);

                        }
                        else if (i == 11)
                        {
                            divValue.InnerHtml = obj.BudgetY11.ToString(formatThousand);

                        }
                        else if (i == 12)
                        {
                            divValue.InnerHtml = obj.BudgetY12.ToString(formatThousand);

                        }
                    }
                    else
                    {
                        divValue.InnerHtml = "---";
                    }
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                }
            }

            sb.AppendLine(trHeader.ToString());
            sb.AppendLine(trValue.ToString());
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignMonth(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, string strTab)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
                {
                    double mainBudget = 0;
                    double allocated = 0;
                    double monthlyCampaignValue = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);
                        TagBuilder span = new TagBuilder("span");
                        if (i == 1)
                        {
                            mainBudget = c.BudgetMonth.BudgetY1;
                            allocated = c.ParentMonth.BudgetY1;
                            monthlyCampaignValue = c.Month.BudgetY1;
                        }
                        else if (i == 2)
                        {
                            mainBudget = c.BudgetMonth.BudgetY2;
                            allocated = c.ParentMonth.BudgetY2;
                            monthlyCampaignValue = c.Month.BudgetY2;
                        }
                        else if (i == 3)
                        {
                            mainBudget = c.BudgetMonth.BudgetY3;
                            allocated = c.ParentMonth.BudgetY3;
                            monthlyCampaignValue = c.Month.BudgetY3;
                        }
                        else if (i == 4)
                        {
                            mainBudget = c.BudgetMonth.BudgetY4;
                            allocated = c.ParentMonth.BudgetY4;
                            monthlyCampaignValue = c.Month.BudgetY4;
                        }
                        else if (i == 5)
                        {
                            mainBudget = c.BudgetMonth.BudgetY5;
                            allocated = c.ParentMonth.BudgetY5;
                            monthlyCampaignValue = c.Month.BudgetY5;
                        }
                        else if (i == 6)
                        {
                            mainBudget = c.BudgetMonth.BudgetY6;
                            allocated = c.ParentMonth.BudgetY6;
                            monthlyCampaignValue = c.Month.BudgetY6;
                        }
                        else if (i == 7)
                        {
                            mainBudget = c.BudgetMonth.BudgetY7;
                            allocated = c.ParentMonth.BudgetY7;
                            monthlyCampaignValue = c.Month.BudgetY7;
                        }
                        else if (i == 8)
                        {
                            mainBudget = c.BudgetMonth.BudgetY8;
                            allocated = c.ParentMonth.BudgetY8;
                            monthlyCampaignValue = c.Month.BudgetY8;
                        }
                        else if (i == 9)
                        {
                            mainBudget = c.BudgetMonth.BudgetY9;
                            allocated = c.ParentMonth.BudgetY9;
                            monthlyCampaignValue = c.Month.BudgetY9;
                        }
                        else if (i == 10)
                        {
                            mainBudget = c.BudgetMonth.BudgetY10;
                            allocated = c.ParentMonth.BudgetY10;
                            monthlyCampaignValue = c.Month.BudgetY10;
                        }
                        else if (i == 11)
                        {
                            mainBudget = c.BudgetMonth.BudgetY11;
                            allocated = c.ParentMonth.BudgetY11;
                            monthlyCampaignValue = c.Month.BudgetY11;
                        }
                        else if (i == 12)
                        {
                            mainBudget = c.BudgetMonth.BudgetY12;
                            allocated = c.ParentMonth.BudgetY12;
                            monthlyCampaignValue = c.Month.BudgetY12;
                        }
                        div.Attributes.Add("mainbudget", mainBudget.ToString(formatThousand));
                        div.Attributes.Add("allocated", allocated.ToString(formatThousand));
                        div.InnerHtml = monthlyCampaignValue.ToString(formatThousand);
                        if (strTab == "1" && mainBudget < monthlyCampaignValue)
                        {
                            span.AddCssClass("red-corner-budget clickme");
                            span.Attributes.Add("rel", "#loadme");
                            span.Attributes.Add("mnth", Enums.YearMonths[i]);
                        }

                        div.AddCssClass(className);
                        if (strTab == "1")
                        {
                            div.AddCssClass("clueplanned");
                        }
                        div.InnerHtml += span.ToString();
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += ProgramMonth(helper, ActivityType.ActivityProgram, c.ActivityId, model, allocatedBy, i, strTab).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }
                else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
                {
                    double mainBudget = 0;
                    double allocated = 0;
                    double monthlyCampaignValue = 0;
                    int quartercount = 1;
                    for (int i = 1; i <= 11; i += 3)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);
                        TagBuilder span = new TagBuilder("span");
                        if (i == 1)
                        {
                            mainBudget = c.BudgetMonth.BudgetY1;
                            allocated = c.ParentMonth.BudgetY1;
                            monthlyCampaignValue = c.Month.BudgetY1;
                        }
                        else if (i == 4)
                        {
                            mainBudget = c.BudgetMonth.BudgetY4;
                            allocated = c.ParentMonth.BudgetY4;
                            monthlyCampaignValue = c.Month.BudgetY4;
                        }
                        else if (i == 7)
                        {
                            mainBudget = c.BudgetMonth.BudgetY7;
                            allocated = c.ParentMonth.BudgetY7;
                            monthlyCampaignValue = c.Month.BudgetY7;
                        }
                        else if (i == 10)
                        {
                            mainBudget = c.BudgetMonth.BudgetY10;
                            allocated = c.ParentMonth.BudgetY10;
                            monthlyCampaignValue = c.Month.BudgetY10;
                        }
                        div.Attributes.Add("mainbudget", mainBudget.ToString(formatThousand));
                        div.Attributes.Add("allocated", allocated.ToString(formatThousand));
                        div.InnerHtml = monthlyCampaignValue.ToString(formatThousand);
                        if (strTab == "1" && mainBudget < monthlyCampaignValue)
                        {
                            span.AddCssClass("red-corner-budget clickme");
                            span.Attributes.Add("rel", "#loadme");
                            span.Attributes.Add("mnth", Enums.YearQuarters[i]);
                        }

                        div.AddCssClass(className);
                        if (strTab == "1")
                        {
                            div.AddCssClass("clueplanned");
                        }
                        div.InnerHtml += span.ToString();
                        td.InnerHtml = div.ToString();
                        td.InnerHtml += ProgramMonth(helper, ActivityType.ActivityProgram, c.ActivityId, model, allocatedBy, quartercount, strTab).ToString();
                        tr.InnerHtml += td.ToString();
                        quartercount += 1;
                    }
                }
                else
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);

                        if (strTab == "2")
                        {
                            if (i == 1)
                            {
                                div.InnerHtml = c.Month.BudgetY1.ToString(formatThousand);

                            }
                            else if (i == 2)
                            {
                                div.InnerHtml = c.Month.BudgetY2.ToString(formatThousand);

                            }
                            else if (i == 3)
                            {
                                div.InnerHtml = c.Month.BudgetY3.ToString(formatThousand);

                            }
                            else if (i == 4)
                            {
                                div.InnerHtml = c.Month.BudgetY4.ToString(formatThousand);

                            }
                            else if (i == 5)
                            {
                                div.InnerHtml = c.Month.BudgetY5.ToString(formatThousand);

                            }
                            else if (i == 6)
                            {
                                div.InnerHtml = c.Month.BudgetY6.ToString(formatThousand);

                            }
                            else if (i == 7)
                            {
                                div.InnerHtml = c.Month.BudgetY7.ToString(formatThousand);

                            }
                            else if (i == 8)
                            {
                                div.InnerHtml = c.Month.BudgetY8.ToString(formatThousand);

                            }
                            else if (i == 9)
                            {
                                div.InnerHtml = c.Month.BudgetY9.ToString(formatThousand);

                            }
                            else if (i == 10)
                            {
                                div.InnerHtml = c.Month.BudgetY10.ToString(formatThousand);

                            }
                            else if (i == 11)
                            {
                                div.InnerHtml = c.Month.BudgetY11.ToString(formatThousand);

                            }
                            else if (i == 12)
                            {
                                div.InnerHtml = c.Month.BudgetY12.ToString(formatThousand);

                            }
                        }
                        else
                        {
                            div.InnerHtml = "---";
                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += ProgramMonth(helper, ActivityType.ActivityProgram, c.ActivityId, model, allocatedBy, i, strTab).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }

                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <param name="strTab"></param>
        /// <returns></returns>
        public static MvcHtmlString ProgramMonth(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, int month, string strTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            bool isPlannedTab = ((int)Enums.BudgetTab.Planned).ToString() == strTab ? true : false;
            bool isTactic = activityType == Helpers.ActivityType.ActivityTactic ? true : false;
            bool isLineItem = activityType == Helpers.ActivityType.ActivityLineItem ? true : false;
            if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = isPlannedTab ? "programLevel clueplanned" : "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = isPlannedTab ? "tacticLevel clueplanned" : "tacticLevel clueactual";
                parentClassName = "program";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = isPlannedTab ? "lineitemLevel clueplanned" : "lineitemLevel clueactual";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    if (!isPlannedTab && activityType == ActivityType.ActivityTactic)
                    {
                        if (model.Where(m => m.ActivityType == ActivityType.ActivityLineItem && m.ParentActivityId == p.ActivityId).ToList().Count == 0)
                        {
                            isLineItem = true;
                        }
                        else if (activityType != ActivityType.ActivityLineItem)
                        {
                            isLineItem = false;
                        }
                    }
                    TagBuilder divProgram = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId.ToString());
                    string className = innerClass;
                    if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
                    {
                        if (month == 1)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY1.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY1 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.January.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY1.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.BudgetY1.ToString(formatThousand)) : p.Month.BudgetY1.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY1 < p.Month.BudgetY1 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.January.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY1.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 2)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY2.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY2 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.February.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY2.ToString(formatThousand));
                                    //className = p.Month.Feb <= p.ParentMonth.Feb ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.BudgetY2.ToString(formatThousand)) : p.Month.BudgetY2.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY2 < p.Month.BudgetY2 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.February.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY2.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 3)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY3.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY3 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.March.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY3.ToString(formatThousand));
                                    //className = p.Month.Mar <= p.ParentMonth.Mar ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.BudgetY3.ToString(formatThousand)) : p.Month.BudgetY3.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY3 < p.Month.BudgetY3 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.March.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY3.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 4)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY4.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY4 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.April.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY4.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.BudgetY4.ToString(formatThousand)) : p.Month.BudgetY4.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY4 < p.Month.BudgetY4 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.April.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY4.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 5)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY5.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY5 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.May.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY5.ToString(formatThousand));
                                    //className = p.Month.May <= p.ParentMonth.May ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.BudgetY5.ToString(formatThousand)) : p.Month.BudgetY5.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY5 < p.Month.BudgetY5 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.May.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY5.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 6)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY6.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY6 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.June.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY6.ToString(formatThousand));
                                    //className = p.Month.Jun <= p.ParentMonth.Jun ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.BudgetY6.ToString(formatThousand)) : p.Month.BudgetY6.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY6 < p.Month.BudgetY6 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.June.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY6.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 7)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY7.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY7 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.July.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY7.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.BudgetY7.ToString(formatThousand)) : p.Month.BudgetY7.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY7 < p.Month.BudgetY7 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.July.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY7.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 8)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY8.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY8 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.August.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY8.ToString(formatThousand));
                                    //className = p.Month.Aug <= p.ParentMonth.Aug ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.BudgetY8.ToString(formatThousand)) : p.Month.BudgetY8.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY8 < p.Month.BudgetY8 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.August.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY8.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 9)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY9.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY9 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.September.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY9.ToString(formatThousand));
                                    //className = p.Month.Sep <= p.ParentMonth.Sep ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.BudgetY9.ToString(formatThousand)) : p.Month.BudgetY9.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY9 < p.Month.BudgetY9 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.September.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY9.ToString());
                                    }
                                }
                            }


                        }
                        else if (month == 10)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY10.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY10 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.October.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY10.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.BudgetY10.ToString(formatThousand)) : p.Month.BudgetY10.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY10 < p.Month.BudgetY10 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.October.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY10.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 11)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY11.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY11 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.November.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY11.ToString(formatThousand));
                                    //className = p.Month.Nov <= p.ParentMonth.Nov ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.BudgetY11.ToString(formatThousand)) : p.Month.BudgetY11.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY11 < p.Month.BudgetY11 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.November.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY11.ToString());
                                    }
                                }
                            }
                        }
                        else if (month == 12)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY12.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY12 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.December.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY12.ToString(formatThousand));
                                    //className = p.Month.Dec <= p.ParentMonth.Dec ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.BudgetY12.ToString(formatThousand)) : p.Month.BudgetY12.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY12 < p.Month.BudgetY12 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Months.December.ToString());
                                        span.Attributes.Add("txt", p.Month.BudgetY12.ToString());
                                    }
                                }
                            }
                        }
                    }
                    else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY1.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY1 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY1.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), p.Month.BudgetY1.ToString(formatThousand)) : p.Month.BudgetY1.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY1 < p.Month.BudgetY1 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()]);
                                        span.Attributes.Add("txt", p.Month.BudgetY1.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 2)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY4.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY4 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY4.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), p.Month.BudgetY4.ToString(formatThousand)) : p.Month.BudgetY4.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY4 < p.Month.BudgetY4 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()]);
                                        span.Attributes.Add("txt", p.Month.BudgetY4.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 3)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY7.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY7 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY7.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), p.Month.BudgetY7.ToString(formatThousand)) : p.Month.BudgetY7.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY7 < p.Month.BudgetY7 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()]);
                                        span.Attributes.Add("txt", p.Month.BudgetY7.ToString());
                                    }
                                }
                            }

                        }
                        else if (month == 4)
                        {
                            if (!isLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY10.ToString(formatThousand));
                            }
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY10 <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY10.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), p.Month.BudgetY10.ToString(formatThousand)) : p.Month.BudgetY10.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.BudgetY10 < p.Month.BudgetY10 && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                    if (p.isEditable || p.ActivityType == ActivityType.ActivityProgram)
                                    {
                                        span.AddCssClass("clickme");
                                        span.Attributes.Add("rel", "#loadme");
                                        span.Attributes.Add("mnth", Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()]);
                                        span.Attributes.Add("txt", p.Month.BudgetY12.ToString());
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        if (strTab == "2")
                        {
                            if (month == 1)
                            {
                                if (isLineItem && p.Month.BudgetY1 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.January.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.BudgetY1.ToString(formatThousand)) : p.Month.BudgetY1.ToString(formatThousand);
                                }

                            }
                            else if (month == 2)
                            {
                                if (isLineItem && p.Month.BudgetY2 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.February.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.BudgetY2.ToString(formatThousand)) : p.Month.BudgetY2.ToString(formatThousand);
                                }

                            }
                            else if (month == 3)
                            {

                                if (isLineItem && p.Month.BudgetY3 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.March.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.BudgetY3.ToString(formatThousand)) : p.Month.BudgetY3.ToString(formatThousand);
                                }

                            }
                            else if (month == 4)
                            {
                                if (isLineItem && p.Month.BudgetY4 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.April.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.BudgetY4.ToString(formatThousand)) : p.Month.BudgetY4.ToString(formatThousand);
                                }

                            }
                            else if (month == 5)
                            {
                                if (isLineItem && p.Month.BudgetY5 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.May.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.BudgetY5.ToString(formatThousand)) : p.Month.BudgetY5.ToString(formatThousand);
                                }

                            }
                            else if (month == 6)
                            {
                                if (isLineItem && p.Month.BudgetY6 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.June.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.BudgetY6.ToString(formatThousand)) : p.Month.BudgetY6.ToString(formatThousand);
                                }

                            }
                            else if (month == 7)
                            {
                                if (isLineItem && p.Month.BudgetY7 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.July.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.BudgetY7.ToString(formatThousand)) : p.Month.BudgetY7.ToString(formatThousand);
                                }

                            }
                            else if (month == 8)
                            {
                                if (isLineItem && p.Month.BudgetY8 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.August.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.BudgetY8.ToString(formatThousand)) : p.Month.BudgetY8.ToString(formatThousand);
                                }

                            }
                            else if (month == 9)
                            {
                                if (isLineItem && p.Month.BudgetY9 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.September.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.BudgetY9.ToString(formatThousand)) : p.Month.BudgetY9.ToString(formatThousand);
                                }


                            }
                            else if (month == 10)
                            {
                                if (isLineItem && p.Month.BudgetY10 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.October.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.BudgetY10.ToString(formatThousand)) : p.Month.BudgetY10.ToString();
                                }

                            }
                            else if (month == 11)
                            {
                                if (isLineItem && p.Month.BudgetY11 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.November.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.BudgetY11.ToString(formatThousand)) : p.Month.BudgetY11.ToString(formatThousand);
                                }
                            }
                            else if (month == 12)
                            {
                                if (isLineItem && p.Month.BudgetY12 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.December.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.BudgetY12.ToString(formatThousand)) : p.Month.BudgetY12.ToString(formatThousand);
                                }
                            }
                        }
                        else
                        {
                            divProgram.InnerHtml = "---";
                        }
                    }
                    divProgram.AddCssClass(className);
                    divProgram.InnerHtml += span.ToString();
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramMonth(helper, ActivityType.ActivityTactic, p.ActivityId, model, allocatedBy, month, strTab).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramMonth(helper, ActivityType.ActivityLineItem, p.ActivityId, model, allocatedBy, month, strTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        public static string ClueTipAnchorTag(string month, string innerHtml)
        {
            TagBuilder anchor = new TagBuilder("a");
            anchor.AddCssClass("clickme");
            anchor.Attributes.Add("rel", "#loadme");
            anchor.Attributes.Add("mnth", month);
            anchor.InnerHtml = innerHtml;
            return anchor.ToString();
        }

        #endregion
        #region Column4
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignBudgetSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, string tab)
        {
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.SingleOrDefault(pl => pl.ActivityType == ActivityType.ActivityPlan);
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", plan.ActivityType + parentActivityId);
                td.AddCssClass("event-row");
                TagBuilder span = new TagBuilder("span");
                if (tab == "0")
                {
                    div.InnerHtml = plan.isEditable ? ClueTipAnchorTag(string.Empty, plan.Allocated.ToString(formatThousand)) : plan.Allocated.ToString(formatThousand);
                    div.AddCssClass("planLevel");
                    div.AddCssClass("clueallocatedbudget");
                    var childTotalAllocated = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == parentActivityId).Select(p => p.Allocated).Sum();
                    if (childTotalAllocated > plan.Allocated)
                    {
                        span.Attributes.Add("class", "orange-corner-budget");
                        if (plan.isEditable)
                        {
                            span.AddCssClass("clickme");
                            span.Attributes.Add("rel", "#loadme");
                            span.Attributes.Add("mnth", string.Empty);
                            span.Attributes.Add("txt", plan.Allocated.ToString());
                        }
                    }
                }
                else
                {
                    if (tab == "2")
                    {
                        double sumMonth = plan.Month.BudgetY1 + plan.Month.BudgetY2 + plan.Month.BudgetY3 + plan.Month.BudgetY4 + plan.Month.BudgetY5 + plan.Month.BudgetY6 + plan.Month.BudgetY7 + plan.Month.BudgetY8 + plan.Month.BudgetY9 + plan.Month.BudgetY10 + plan.Month.BudgetY11 + plan.Month.BudgetY12;
                        div.InnerHtml = sumMonth.ToString(formatThousand);
                        if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                        {
                            div.AddCssClass("firstLevel");
                        }
                    }
                    else if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                    {
                        div.InnerHtml = plan.Allocated.ToString(formatThousand);
                        if (tab == "1")
                        {
                            div.Attributes.Add("mainbudget", plan.MainBudgeted.ToString());
                            if (plan.Allocated > plan.MainBudgeted)
                            {
                                span.Attributes.Add("class", "red-corner-budget clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", string.Empty);
                                div.AddCssClass("planLevel clueallocatedCost");
                            }
                        }
                    }
                    else
                    {
                        div.AddCssClass("firstLevel");
                        div.InnerHtml = "---";
                    }
                }
                div.InnerHtml += span.ToString();
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");

                TagBuilder tdLast = new TagBuilder("td");
                tdLast.AddCssClass("campaign-row");
                TagBuilder span = new TagBuilder("span");
                TagBuilder divLast = new TagBuilder("div");
                divLast.Attributes.Add("id", activityType + c.ActivityId);
                divLast.AddCssClass("campaignLevel");
                if (tab == "0")
                {
                    divLast.InnerHtml = c.isEditable ? ClueTipAnchorTag(string.Empty, c.Allocated.ToString(formatThousand)) : c.Allocated.ToString(formatThousand);
                    divLast.AddCssClass("clueallocatedbudget");
                    var childTotlaAllocated = model.Where(p => p.ActivityType == ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Allocated).Sum();
                    if (childTotlaAllocated > c.Allocated)
                    {
                        span.Attributes.Add("class", "orange-corner-budget");
                        if (c.isEditable)
                        {
                            span.AddCssClass("clickme");
                            span.Attributes.Add("rel", "#loadme");
                            span.Attributes.Add("mnth", string.Empty);
                            span.Attributes.Add("txt", c.Allocated.ToString());
                        }
                    }
                }
                else
                {
                    if (tab == "2")
                    {
                        double sumMonth = c.Month.BudgetY1 + c.Month.BudgetY2 + c.Month.BudgetY3 + c.Month.BudgetY4 + c.Month.BudgetY5 + c.Month.BudgetY6 + c.Month.BudgetY7 + c.Month.BudgetY8 + c.Month.BudgetY9 + c.Month.BudgetY10 + c.Month.BudgetY11 + c.Month.BudgetY12;
                        divLast.InnerHtml = sumMonth.ToString(formatThousand);
                        if (allocatedBy == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                        {
                            divLast.AddCssClass("firstLevel");
                        }
                    }
                    else if (allocatedBy != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                    {
                        divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                        if (tab == "1")
                        {
                            divLast.Attributes.Add("mainbudget", c.MainBudgeted.ToString());
                            if (c.Allocated > c.MainBudgeted)
                            {
                                span.Attributes.Add("class", "red-corner-budget clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", string.Empty);
                                divLast.AddCssClass("clueallocatedCost");
                            }
                        }
                    }
                    else
                    {
                        divLast.AddCssClass("firstLevel");
                        divLast.InnerHtml = "---";
                    }
                }
                divLast.InnerHtml += span.ToString();
                tdLast.InnerHtml = divLast.ToString();
                tdLast.InnerHtml += ProgramBudgetSummary(helper, ActivityType.ActivityProgram, c.ActivityId, model, "last", allocatedBy, tab).ToString();

                tr.InnerHtml += tdLast.ToString();

                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="mode"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        public static MvcHtmlString ProgramBudgetSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string mode, string allocatedBy, string tab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            bool isProgram = false;
            bool isOtherLineItem = false;
            if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                isProgram = true;
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");

                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    if (p.ActivityType == ActivityType.ActivityLineItem && p.LineItemTypeId == null)
                    {
                        isOtherLineItem = true;
                    }
                    TagBuilder span = new TagBuilder("span");
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    if (tab == "0")
                    {
                        if (activityType == ActivityType.ActivityLineItem)
                        {
                            divProgram.InnerHtml += "---";
                        }
                        else
                        {
                            divProgram.InnerHtml += p.isEditable ? ClueTipAnchorTag(string.Empty, p.Allocated.ToString(formatThousand)) : p.Allocated.ToString(formatThousand);
                            divProgram.AddCssClass("clueallocatedbudget");
                        }
                    }
                    else
                    {
                        if (allocatedBy != "default")
                        {
                            if (tab == "2")
                            {
                                double sumMonth = p.Month.BudgetY1 + p.Month.BudgetY2 + p.Month.BudgetY3 + p.Month.BudgetY4 + p.Month.BudgetY5 + p.Month.BudgetY6 + p.Month.BudgetY7 + p.Month.BudgetY8 + p.Month.BudgetY9 + p.Month.BudgetY10 + p.Month.BudgetY11 + p.Month.BudgetY12;
                                divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            }
                            else
                            {
                                divProgram.InnerHtml += p.isEditable && !isOtherLineItem && !isProgram ? ClueTipAnchorTag(string.Empty, p.Allocated.ToString(formatThousand)) : p.Allocated.ToString(formatThousand);
                                divProgram.AddCssClass("clueallocatedCost");
                            }
                        }
                        else
                        {
                            if (tab == "2")
                            {
                                double sumMonth = p.Month.BudgetY1 + p.Month.BudgetY2 + p.Month.BudgetY3 + p.Month.BudgetY4 + p.Month.BudgetY5 + p.Month.BudgetY6 + p.Month.BudgetY7 + p.Month.BudgetY8 + p.Month.BudgetY9 + p.Month.BudgetY10 + p.Month.BudgetY11 + p.Month.BudgetY12;
                                divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            }
                            else
                            {
                                divProgram.InnerHtml += "---";
                            }
                        }
                    }
                    if (tab == "0")
                    {
                        var childTotalAllocated = 0.0;
                        if (p.ActivityType == ActivityType.ActivityProgram)
                        {
                            childTotalAllocated = model.Where(c => c.ActivityType == ActivityType.ActivityTactic.ToString() && c.ParentActivityId == p.ActivityId).Select(c => c.Allocated).Sum();
                        }
                        else if (p.ActivityType == ActivityType.ActivityTactic)
                        {
                            childTotalAllocated = model.Where(c => c.ActivityType == ActivityType.ActivityLineItem.ToString() && c.ParentActivityId == p.ActivityId).Select(c => c.Allocated).Sum();
                        }
                        if (childTotalAllocated > p.Allocated)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                            if (p.isEditable)
                            {
                                span.AddCssClass("clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", string.Empty);
                                span.Attributes.Add("txt", p.Allocated.ToString());
                            }
                        }
                    }
                    else if (tab == "1" && allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                    {
                        divProgram.Attributes.Add("mainbudget", p.MainBudgeted.ToString());
                        if (p.Allocated > p.MainBudgeted && activityType != ActivityType.ActivityLineItem)
                        {
                            span.Attributes.Add("class", "red-corner-budget");
                            if (ActivityType.ActivityTactic != activityType)
                            {
                                span.AddCssClass("clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", string.Empty);
                            }
                            else if (p.isEditable)
                            {
                                span.AddCssClass("clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", string.Empty);
                                span.Attributes.Add("txt", p.Allocated.ToString());
                            }
                        }
                    }
                    divProgram.AddCssClass(innerClass);
                    divProgram.InnerHtml += span.ToString();
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramBudgetSummary(helper, ActivityType.ActivityTactic, p.ActivityId, model, mode, allocatedBy, tab).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramBudgetSummary(helper, ActivityType.ActivityLineItem, p.ActivityId, model, mode, allocatedBy, tab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }
        #endregion
        #region Column3

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, string tab)
        {
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.SingleOrDefault(pl => pl.ActivityType == ActivityType.ActivityPlan);
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("event-row");
                TagBuilder div = new TagBuilder("div");
                TagBuilder span = new TagBuilder("span");
                div.Attributes.Add("id", plan.ActivityType + parentActivityId);
                if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                {
                    double unallocated = 0;
                    double sumMonth = plan.Month.BudgetY1 + plan.Month.BudgetY2 + plan.Month.BudgetY3 + plan.Month.BudgetY4 + plan.Month.BudgetY5 + plan.Month.BudgetY6 + plan.Month.BudgetY7 + plan.Month.BudgetY8 + plan.Month.BudgetY9 + plan.Month.BudgetY10 + plan.Month.BudgetY11 + plan.Month.BudgetY12;
                    if (tab == "0")
                    {

                        unallocated = plan.Allocated - sumMonth;
                        if (unallocated > 0)
                        {
                            span.AddCssClass("blue-corner-budget");
                        }
                        else if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    else
                    {
                        unallocated = tab == "2" ? plan.MainBudgeted - sumMonth : plan.MainBudgeted - plan.Allocated;
                        if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    div.InnerHtml = unallocated.ToString(formatThousand);
                }
                else
                {
                    div.AddCssClass("firstLevel");
                    if (tab == "2")
                    {
                        double sumMonth = plan.Month.BudgetY1 + plan.Month.BudgetY2 + plan.Month.BudgetY3 + plan.Month.BudgetY4 + plan.Month.BudgetY5 + plan.Month.BudgetY6 + plan.Month.BudgetY7 + plan.Month.BudgetY8 + plan.Month.BudgetY9 + plan.Month.BudgetY10 + plan.Month.BudgetY11 + plan.Month.BudgetY12;
                        double unAllocated = plan.MainBudgeted - sumMonth;
                        div.InnerHtml = unAllocated.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }

                }
                div.InnerHtml += span.ToString();
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();
                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");

                //First
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("campaign-row");
                TagBuilder span = new TagBuilder("span");
                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", activityType + c.ActivityId);

                if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                {
                    double unallocated = 0;
                    div.AddCssClass("campaignLevel");
                    double sumMonth = c.Month.BudgetY1 + c.Month.BudgetY2 + c.Month.BudgetY3 + c.Month.BudgetY4 + c.Month.BudgetY5 + c.Month.BudgetY6 + c.Month.BudgetY7 + c.Month.BudgetY8 + c.Month.BudgetY9 + c.Month.BudgetY10 + c.Month.BudgetY11 + c.Month.BudgetY12;
                    if (tab == "0")
                    {
                        unallocated = c.Allocated - sumMonth;
                        if (unallocated > 0)
                        {
                            span.AddCssClass("blue-corner-budget");
                        }
                        else if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    else
                    {
                        unallocated = tab == "2" ? c.MainBudgeted - sumMonth : c.MainBudgeted - c.Allocated;
                        if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    div.InnerHtml += unallocated.ToString(formatThousand);
                }
                else
                {
                    if (tab == "2")
                    {
                        double sumMonth = c.Month.BudgetY1 + c.Month.BudgetY2 + c.Month.BudgetY3 + c.Month.BudgetY4 + c.Month.BudgetY5 + c.Month.BudgetY6 + c.Month.BudgetY7 + c.Month.BudgetY8 + c.Month.BudgetY9 + c.Month.BudgetY10 + c.Month.BudgetY11 + c.Month.BudgetY12;
                        double unAllocated = c.MainBudgeted - sumMonth;
                        div.InnerHtml += unAllocated.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml += "---";
                    }
                    div.AddCssClass("firstLevel");

                }
                div.InnerHtml += span.ToString();
                td.InnerHtml = div.ToString();

                td.InnerHtml += ProgramSummary(helper, ActivityType.ActivityProgram, c.ActivityId, model, "first", allocatedBy, tab).ToString();
                tr.InnerHtml += td.ToString();
                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ProgramSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string mode, string allocatedBy, string tab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    //divProgram.AddCssClass(innerClass);

                    if (mode == "first")
                    {
                        if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower() && (activityType != ActivityType.ActivityLineItem))
                        {
                            double unAllocated = 0;
                            divProgram.AddCssClass(innerClass);
                            double sumMonth = p.Month.BudgetY1 + p.Month.BudgetY2 + p.Month.BudgetY3 + p.Month.BudgetY4 + p.Month.BudgetY5 + p.Month.BudgetY6 + p.Month.BudgetY7 + p.Month.BudgetY8 + p.Month.BudgetY9 + p.Month.BudgetY10 + p.Month.BudgetY11 + p.Month.BudgetY12;
                            if (tab == "0")
                            {
                                unAllocated = p.Allocated - sumMonth;

                                if (unAllocated > 0)
                                {
                                    // divProgram.AddCssClass(innerClass + budgetError);
                                    span.AddCssClass("blue-corner-budget");
                                }
                                else if (unAllocated < 0)
                                {
                                    divProgram.AddCssClass("budgetError");
                                }
                            }
                            else
                            {
                                unAllocated = tab == "2" ? p.MainBudgeted - sumMonth : p.MainBudgeted - p.Allocated;
                                if (unAllocated < 0)
                                {
                                    divProgram.AddCssClass("budgetError");
                                }
                            }
                            divProgram.InnerHtml = unAllocated.ToString(formatThousand);
                        }
                        else
                        {
                            if (tab == "2" && activityType != ActivityType.ActivityLineItem)
                            {
                                double sumMonth = p.Month.BudgetY1 + p.Month.BudgetY2 + p.Month.BudgetY3 + p.Month.BudgetY4 + p.Month.BudgetY5 + p.Month.BudgetY6 + p.Month.BudgetY7 + p.Month.BudgetY8 + p.Month.BudgetY9 + p.Month.BudgetY10 + p.Month.BudgetY11 + p.Month.BudgetY12;
                                double unAllocated = p.MainBudgeted - sumMonth;
                                divProgram.InnerHtml = unAllocated.ToString(formatThousand);
                            }
                            else
                            {
                                divProgram.InnerHtml += "---";
                            }
                            divProgram.AddCssClass(innerClass);
                        }
                    }
                    divProgram.InnerHtml += span.ToString();
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramSummary(helper, ActivityType.ActivityTactic, p.ActivityId, model, mode, allocatedBy, tab).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramSummary(helper, ActivityType.ActivityLineItem, p.ActivityId, model, mode, allocatedBy, tab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
        }

        #endregion

        #region Column2 Allocated

        /// <summary>
        /// Render month header and plans month values
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="activityId"></param>
        /// <param name="obj"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString AllocatedPlanMonth(this HtmlHelper helper, string activityType, string activityId, BudgetMonth obj, BudgetMonth parent, string allocatedBy, List<BudgetModel> model)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            bool isEditable = model.Where(p => p.ActivityType == ActivityType.ActivityPlan.ToString()).Select(p => p.isEditable).FirstOrDefault();
            if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
            {
                Dictionary<int, double> planMonth = new Dictionary<int, double>()
                    {
                        {1,obj.BudgetY1},
                        {2,obj.BudgetY2},
                        {3,obj.BudgetY3},
                        {4,obj.BudgetY4},
                        {5,obj.BudgetY5},
                        {6,obj.BudgetY6},
                        {7,obj.BudgetY7},
                        {8,obj.BudgetY8},
                        {9,obj.BudgetY9},
                        {10,obj.BudgetY10},
                        {11,obj.BudgetY11},
                        {12,obj.BudgetY12}
                    };
                for (int i = 1; i <= 12; i++)
                {
                    DateTime dt = new DateTime(2012, i, 1);
                    TagBuilder tdHeader = new TagBuilder("td");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");

                    TagBuilder divValue = new TagBuilder("div");
                    divHeader.InnerHtml = dt.ToString("MMM").ToUpper();
                    divValue.Attributes.Add("id", activityType + activityId);
                    divValue.Attributes.Add("class", "planLevel clueallocated");
                    string className = "event-row";
                    TagBuilder span = new TagBuilder("span");
                    //if (i == 1)
                    //{
                    divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.YearMonths[i], planMonth[i].ToString(formatThousand)) : planMonth[i].ToString(formatThousand);
                    double totalChildBudget = 0;
                    if (i == 1)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY1).ToList().Sum();
                    }
                    else if (i == 2)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY2).ToList().Sum();
                    }
                    else if (i == 3)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY3).ToList().Sum();
                    }
                    else if (i == 4)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY4).ToList().Sum();
                    }
                    else if (i == 5)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY5).ToList().Sum();
                    }
                    else if (i == 6)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY6).ToList().Sum();
                    }
                    else if (i == 7)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY7).ToList().Sum();
                    }
                    else if (i == 8)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY8).ToList().Sum();
                    }
                    else if (i == 9)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY9).ToList().Sum();
                    }
                    else if (i == 10)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY10).ToList().Sum();
                    }
                    else if (i == 11)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY11).ToList().Sum();
                    }
                    else if (i == 12)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY12).ToList().Sum();
                    }
                    if (totalChildBudget > planMonth[i])
                    {
                        span.Attributes.Add("class", "orange-corner-budget");
                        if (isEditable)
                        {
                            span.AddCssClass("clickme");
                            span.Attributes.Add("rel", "#loadme");
                            span.Attributes.Add("mnth", Enums.YearMonths[i]);
                            span.Attributes.Add("txt", planMonth[i].ToString());
                        }
                    }

                    divValue.InnerHtml += span.ToString();
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                }
            }
            else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
            {
                Dictionary<int, double> campaignMonth = new Dictionary<int, double>()
                    {
                        {1,obj.BudgetY1},
                        {4,obj.BudgetY4},
                        {7,obj.BudgetY7},
                        {10,obj.BudgetY10}
                        
                    };
                int quarterCounter = 1;
                for (int i = 1; i <= 11; i += 3)
                {
                    string className = "event-row";
                    TagBuilder tdHeader = new TagBuilder("td");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");
                    TagBuilder divValue = new TagBuilder("div");
                    divValue.Attributes.Add("id", activityType + activityId.ToString());
                    divValue.Attributes.Add("class", "planLevel clueallocated");
                    divHeader.InnerHtml = "Q" + quarterCounter.ToString();
                    TagBuilder span = new TagBuilder("span");

                    divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.YearQuarters[i], campaignMonth[i].ToString(formatThousand)) : campaignMonth[i].ToString(formatThousand);
                    double totalChildBudget = 0;

                    if (i == 1)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY1).ToList().Sum();
                    }
                    else if (i == 4)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY4).ToList().Sum();
                    }
                    else if (i == 7)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY7).ToList().Sum();
                    }
                    else if (i == 11)
                    {
                        totalChildBudget = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == activityId).Select(c => c.Month.BudgetY10).ToList().Sum();
                    }
                    if (totalChildBudget > obj.BudgetY1)
                    {
                        span.Attributes.Add("class", "orange-corner-budget");
                        if (isEditable)
                        {
                            span.AddCssClass("clickme");
                            span.Attributes.Add("rel", "#loadme");
                            span.Attributes.Add("mnth", Enums.YearQuarters[i]);
                            span.Attributes.Add("txt", campaignMonth[i].ToString());
                        }
                    }

                    divValue.InnerHtml += span.ToString();
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                    quarterCounter += 1;
                }
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    DateTime dt = new DateTime(2012, i, 1);
                    TagBuilder tdHeader = new TagBuilder("td");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");

                    TagBuilder divValue = new TagBuilder("div");
                    divHeader.InnerHtml = dt.ToString("MMM").ToUpper();
                    divValue.Attributes.Add("id", activityType + activityId);
                    string className = "event-row";

                    divValue.InnerHtml = "---";

                    if (className.Contains("budgetError"))
                    {
                        className = className.Replace(budgetError, "");
                        divValue.AddCssClass("budgetError");
                    }
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                }
            }
            sb.AppendLine(trHeader.ToString());
            sb.AppendLine(trValue.ToString());
            return new MvcHtmlString(sb.ToString());
        }
        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString AllocatedCampaignMonth(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                {
                    Dictionary<int, double> campaignMonth = new Dictionary<int, double>()
                    {
                        {1,c.Month.BudgetY1},
                        {2,c.Month.BudgetY2},
                        {3,c.Month.BudgetY3},
                        {4,c.Month.BudgetY4},
                        {5,c.Month.BudgetY5},
                        {6,c.Month.BudgetY6},
                        {7,c.Month.BudgetY7},
                        {8,c.Month.BudgetY8},
                        {9,c.Month.BudgetY9},
                        {10,c.Month.BudgetY10},
                        {11,c.Month.BudgetY11},
                        {12,c.Month.BudgetY12}
                    };
                    Dictionary<int, double> campaignParentMonth = new Dictionary<int, double>()
                    {
                        {1,c.ParentMonth.BudgetY1},
                        {2,c.ParentMonth.BudgetY2},
                        {3,c.ParentMonth.BudgetY3},
                        {4,c.ParentMonth.BudgetY4},
                        {5,c.ParentMonth.BudgetY5},
                        {6,c.ParentMonth.BudgetY6},
                        {7,c.ParentMonth.BudgetY7},
                        {8,c.ParentMonth.BudgetY8},
                        {9,c.ParentMonth.BudgetY9},
                        {10,c.ParentMonth.BudgetY10},
                        {11,c.ParentMonth.BudgetY11},
                        {12,c.ParentMonth.BudgetY12}
                    };
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel clueallocated";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);
                        TagBuilder span = new TagBuilder("span");
                        div.Attributes.Add("allocated", campaignParentMonth[i].ToString(formatThousand));
                        div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.YearMonths[i], campaignMonth[i].ToString(formatThousand)) : campaignMonth[i].ToString(formatThousand);
                        double childTotalAllocated = 0;
                        if (i == 1)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY1).Sum();
                        }
                        else if (i == 2)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY2).Sum();
                        }
                        else if (i == 3)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY3).Sum();
                        }
                        else if (i == 4)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY4).Sum();
                        }
                        else if (i == 5)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY5).Sum();
                        }
                        else if (i == 6)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY6).Sum();
                        }
                        else if (i == 7)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY7).Sum();
                        }
                        else if (i == 8)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY8).Sum();
                        }
                        else if (i == 9)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY9).Sum();
                        }
                        else if (i == 10)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY10).Sum();
                        }
                        else if (i == 11)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY11).Sum();
                        }
                        else if (i == 12)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY12).Sum();
                        }

                        if (campaignMonth[i] < childTotalAllocated)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                            if (c.isEditable)
                            {
                                span.AddCssClass("clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", Enums.YearMonths[i]);
                                span.Attributes.Add("txt", campaignMonth[i].ToString());
                            }
                        }
                        div.AddCssClass(className);
                        div.InnerHtml += span.ToString();
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, ActivityType.ActivityProgram, c.ActivityId, model, allocatedBy, i).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }
                else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
                {
                    for (int i = 1; i <= 11; i += 3)
                    {
                        Dictionary<int, double> campaignMonth = new Dictionary<int, double>()
                    {
                        {1,c.Month.BudgetY1},
                        {4,c.Month.BudgetY4},
                        {7,c.Month.BudgetY7},
                        {10,c.Month.BudgetY10}
                        
                    };
                        Dictionary<int, double> campaignParentMonth = new Dictionary<int, double>()
                    {
                        {1,c.ParentMonth.BudgetY1},
                        {4,c.ParentMonth.BudgetY4},
                        {7,c.ParentMonth.BudgetY7},
                        {10,c.ParentMonth.BudgetY10}
                    };
                        string className = "campaignLevel clueallocated";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);
                        TagBuilder span = new TagBuilder("span");

                        div.Attributes.Add("allocated", campaignParentMonth[i].ToString(formatThousand));
                        div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.YearQuarters[i], campaignMonth[i].ToString(formatThousand)) : campaignMonth[i].ToString(formatThousand);
                        double childTotalAllocated = 0;
                        if (i == 1)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY1).Sum();
                        }
                        else if (i == 4)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY4).Sum();
                        }
                        else if (i == 7)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY7).Sum();
                        }
                        else if (i == 10)
                        {
                            childTotalAllocated =
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityProgram &&
                                        p.ParentActivityId == c.ActivityId).Select(p => p.Month.BudgetY11).Sum();
                        }
                        if (campaignMonth[i] < childTotalAllocated)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                            if (c.isEditable)
                            {
                                span.AddCssClass("clickme");
                                span.Attributes.Add("rel", "#loadme");
                                span.Attributes.Add("mnth", Enums.YearQuarters[i]);
                                span.Attributes.Add("txt", campaignMonth[i].ToString());
                            }
                        }

                        div.AddCssClass(className);
                        div.InnerHtml += span.ToString();
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, ActivityType.ActivityProgram, c.ActivityId, model, allocatedBy, i).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }
                else
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);

                        div.InnerHtml = "---";

                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, ActivityType.ActivityProgram, c.ActivityId, model, allocatedBy, i).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }

                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString AllocatedProgramMonth(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, int month)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel clueallocated";
            string parentClassName = "campaign";
            if (activityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel clueallocated";
                parentClassName = "campaign";
            }
            else if (activityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel clueallocated";
                parentClassName = "program";
            }
            else if (activityType == "lineitem")
            {
                mainClass = "sub lineitem-lvl";
                innerClass = "lineitemLevel clueallocated";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {

                    TagBuilder divProgram = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    string className = innerClass;
                    if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
                    {
                        double allocatedValue = 0;
                        double monthlyValue = 0;
                        double childTotal = 0;
                        if (month == 1)
                        {
                            allocatedValue = p.ParentMonth.BudgetY1;
                            monthlyValue = p.Month.BudgetY1;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY1).Sum();
                            }
                        }
                        else if (month == 2)
                        {
                            allocatedValue = p.ParentMonth.BudgetY2;
                            monthlyValue = p.Month.BudgetY2;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY2).Sum();
                            }
                        }
                        else if (month == 3)
                        {
                            allocatedValue = p.ParentMonth.BudgetY3;
                            monthlyValue = p.Month.BudgetY3;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY3).Sum();
                            }
                        }
                        else if (month == 4)
                        {
                            allocatedValue = p.ParentMonth.BudgetY4;
                            monthlyValue = p.Month.BudgetY4;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY4).Sum();
                            }
                        }
                        else if (month == 5)
                        {
                            allocatedValue = p.ParentMonth.BudgetY5;
                            monthlyValue = p.Month.BudgetY5;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY5).Sum();
                            }
                        }
                        else if (month == 6)
                        {
                            allocatedValue = p.ParentMonth.BudgetY6;
                            monthlyValue = p.Month.BudgetY6;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY6).Sum();
                            }
                        }
                        else if (month == 7)
                        {
                            allocatedValue = p.ParentMonth.BudgetY7;
                            monthlyValue = p.Month.BudgetY7;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY7).Sum();
                            }
                        }
                        else if (month == 8)
                        {
                            allocatedValue = p.ParentMonth.BudgetY8;
                            monthlyValue = p.Month.BudgetY8;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY8).Sum();
                            }
                        }
                        else if (month == 9)
                        {
                            allocatedValue = p.ParentMonth.BudgetY9;
                            monthlyValue = p.Month.BudgetY9;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY9).Sum();
                            }
                        }
                        else if (month == 10)
                        {
                            allocatedValue = p.ParentMonth.BudgetY10;
                            monthlyValue = p.Month.BudgetY10;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY10).Sum();
                            }
                        }
                        else if (month == 11)
                        {
                            allocatedValue = p.ParentMonth.BudgetY11;
                            monthlyValue = p.Month.BudgetY11;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY11).Sum();
                            }
                        }
                        else if (month == 12)
                        {
                            allocatedValue = p.ParentMonth.BudgetY12;
                            monthlyValue = p.Month.BudgetY12;
                            if (p.ActivityType == ActivityType.ActivityProgram)
                            {
                                childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY12).Sum();
                            }
                        }

                        divProgram.Attributes.Add("allocated", allocatedValue.ToString(formatThousand));

                        divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.YearMonths[month], monthlyValue.ToString(formatThousand)) : monthlyValue.ToString(formatThousand);
                        if (p.ActivityType == ActivityType.ActivityProgram)
                        {
                            if (childTotal > monthlyValue)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                                if (p.isEditable)
                                {
                                    span.AddCssClass("clickme");
                                    span.Attributes.Add("rel", "#loadme");
                                    span.Attributes.Add("mnth", Enums.YearMonths[month]);
                                    span.Attributes.Add("txt", monthlyValue.ToString());
                                }
                            }
                        }

                    }
                    else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
                    {
                        double allocatedValue = 0;
                        double monthlyValue = 0;
                        double childTotal = 0;
                        if (month == 1)
                        {
                            allocatedValue = p.ParentMonth.BudgetY1;
                            monthlyValue = p.Month.BudgetY1;
                            childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY1).Sum();
                        }
                        else if (month == 4)
                        {
                            allocatedValue = p.ParentMonth.BudgetY4;
                            monthlyValue = p.Month.BudgetY4;
                            childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY4).Sum();
                        }
                        else if (month == 7)
                        {
                            allocatedValue = p.ParentMonth.BudgetY7;
                            monthlyValue = p.Month.BudgetY7;
                            childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY7).Sum();
                        }
                        else if (month == 10)
                        {
                            allocatedValue = p.ParentMonth.BudgetY10;
                            monthlyValue = p.Month.BudgetY10;
                            childTotal = model.Where(a => a.ActivityType == ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.BudgetY10).Sum();
                        }

                        divProgram.Attributes.Add("allocated", allocatedValue.ToString(formatThousand));
                        divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.YearQuarters[month], monthlyValue.ToString(formatThousand)) : monthlyValue.ToString(formatThousand);
                        if (p.ActivityType == ActivityType.ActivityProgram)
                        {
                            if (childTotal > monthlyValue)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                                if (p.isEditable)
                                {
                                    span.AddCssClass("clickme");
                                    span.Attributes.Add("rel", "#loadme");
                                    span.Attributes.Add("mnth", Enums.YearQuarters[month]);
                                    span.Attributes.Add("txt", monthlyValue.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        divProgram.InnerHtml = "---";
                    }
                    divProgram.AddCssClass(className);
                    divProgram.InnerHtml += span.ToString();
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += AllocatedProgramMonth(helper, ActivityType.ActivityTactic, p.ActivityId, model, allocatedBy, month).ToString();
                    else if (activityType == "tactic")
                        div.InnerHtml += AllocatedProgramMonth(helper, ActivityType.ActivityLineItem, p.ActivityId, model, allocatedBy, month).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        #endregion

        #region Custom fields

        #region Column1 Custom fields

        /// <summary>
        /// Render activity names for all campaigns
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityMainParent(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string tab = "2", string view = "0")
        {
            string strViewBy = "";
            strViewBy = ActivityType.ActivityCustomField;
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {
                    TagBuilder tr = new TagBuilder("tr");
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("campaign-row audience");


                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", activityType + c.ActivityId);
                    div.AddCssClass("firstLevel");
                    TagBuilder aLink = new TagBuilder("span");
                    if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                    {
                        TagBuilder aAccordian = new TagBuilder("a");
                        aAccordian.AddCssClass("accordionClick");
                        div.InnerHtml = aAccordian.ToString();
                        aLink.Attributes.Add("style", "cursor:pointer;");
                    }
                    else
                    {
                        aLink.Attributes.Add("style", "padding-left:20px;cursor:pointer;");
                    }
                    aLink.InnerHtml = c.ActivityName;

                    div.InnerHtml += aLink.ToString();

                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ActivityChild(helper, ActivityType.ActivityCampaign, c.ActivityId, model, tab, view).ToString();
                    tr.InnerHtml = td.ToString();
                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Render activity names for all children
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityChild(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string tab = "2", string view = "0")
        {

            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            string childActivity = "tactic";
            bool needAccrodian = true;
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                parentClassName = ActivityType.ActivityCustomField;
                childActivity = "program";
            }
            else if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                childActivity = "tactic";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
                childActivity = "lineitem";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
                needAccrodian = false;
                childActivity = "";
            }
            if (tab == "0")
            {
                needAccrodian = false;
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    divProgram.AddCssClass(innerClass);

                    TagBuilder aLink = new TagBuilder("a");
                    if (needAccrodian)
                    {
                        if (model.Any(p1 => p1.ActivityType == childActivity && p1.ParentActivityId == p.ActivityId))
                        {
                            TagBuilder aAccordian = new TagBuilder("a");
                            //aAccordian.Attributes.Add("href", "#");
                            aAccordian.AddCssClass("accordionClick");
                            divProgram.InnerHtml = aAccordian.ToString();
                            aLink.Attributes.Add("style", "cursor:pointer;");
                        }
                        else
                        {
                            aLink.Attributes.Add("style", "padding-left:20px;cursor:pointer;");
                        }
                    }
                    else
                    {
                        aLink.Attributes.Add("style", "cursor:pointer;");
                    }

                    //aLink.Attributes.Add("href", "#");
                    aLink.InnerHtml = p.ActivityName;


                    aLink.Attributes.Add("id", p.Id);
                    aLink.Attributes.Add("linktype", activityType);

                    divProgram.InnerHtml += aLink.ToString();

                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ActivityChild(helper, ActivityType.ActivityProgram, p.ActivityId, model, tab, view).ToString();
                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ActivityChild(helper, ActivityType.ActivityTactic, p.ActivityId, model, tab, view).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ActivityChild(helper, ActivityType.ActivityLineItem, p.ActivityId, model, tab, view).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        #endregion

        #region Column2 Custom fields

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentMonth(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, string strTab, string view = "0")
        {
            string strViewBy = "";
            strViewBy = ActivityType.ActivityCustomField;
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {

                    TagBuilder tr = new TagBuilder("tr");
                    if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            string className = "firstLevel";
                            TagBuilder td = new TagBuilder("td");
                            td.AddCssClass("campaign-row audience");

                            TagBuilder div = new TagBuilder("div");
                            div.Attributes.Add("id", activityType + c.ActivityId);

                            if (i == 1)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY1.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY1.ToString(formatThousand);
                            }
                            else if (i == 2)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY2.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY2.ToString(formatThousand);
                            }
                            else if (i == 3)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY3.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY3.ToString(formatThousand);
                            }
                            else if (i == 4)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY4.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY4.ToString(formatThousand);
                            }
                            else if (i == 5)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY5.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY5.ToString(formatThousand);
                            }
                            else if (i == 6)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY6.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY6.ToString(formatThousand);
                            }
                            else if (i == 7)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY7.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY7.ToString(formatThousand);
                            }
                            else if (i == 8)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY8.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY8.ToString(formatThousand);
                            }
                            else if (i == 9)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY9.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY9.ToString(formatThousand);
                            }
                            else if (i == 10)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY10.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY10.ToString(formatThousand);
                            }
                            else if (i == 11)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY11.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY11.ToString(formatThousand);
                            }
                            else if (i == 12)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY12.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY12.ToString(formatThousand);
                            }
                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, ActivityType.ActivityCampaign, c.ActivityId, model, allocatedBy, i, strTab, view).ToString();
                            tr.InnerHtml += td.ToString();
                        }
                    }
                    else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            string className = "firstLevel";
                            TagBuilder td = new TagBuilder("td");
                            td.AddCssClass("campaign-row audience");

                            TagBuilder div = new TagBuilder("div");
                            div.Attributes.Add("id", activityType + c.ActivityId);

                            if (i == 1)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY1.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY1.ToString(formatThousand);
                            }
                            else if (i == 2)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY4.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY4.ToString(formatThousand);
                            }
                            else if (i == 3)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY7.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY7.ToString(formatThousand);
                            }
                            else if (i == 4)
                            {
                                div.Attributes.Add("mainbudget", c.BudgetMonth.BudgetY10.ToString(formatThousand));
                                div.InnerHtml = c.Month.BudgetY10.ToString(formatThousand);
                            }
                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, ActivityType.ActivityCampaign, c.ActivityId, model, allocatedBy, i, strTab, view).ToString();
                            tr.InnerHtml += td.ToString();
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            string className = "firstLevel";
                            TagBuilder td = new TagBuilder("td");
                            td.AddCssClass("campaign-row audience");

                            TagBuilder div = new TagBuilder("div");
                            div.Attributes.Add("id", activityType + c.ActivityId);

                            if (strTab == "2")
                            {
                                if (i == 1)
                                {
                                    div.InnerHtml = c.Month.BudgetY1.ToString(formatThousand);

                                }
                                else if (i == 2)
                                {
                                    div.InnerHtml = c.Month.BudgetY2.ToString(formatThousand);

                                }
                                else if (i == 3)
                                {
                                    div.InnerHtml = c.Month.BudgetY3.ToString(formatThousand);

                                }
                                else if (i == 4)
                                {
                                    div.InnerHtml = c.Month.BudgetY4.ToString(formatThousand);

                                }
                                else if (i == 5)
                                {
                                    div.InnerHtml = c.Month.BudgetY5.ToString(formatThousand);

                                }
                                else if (i == 6)
                                {
                                    div.InnerHtml = c.Month.BudgetY6.ToString(formatThousand);

                                }
                                else if (i == 7)
                                {
                                    div.InnerHtml = c.Month.BudgetY7.ToString(formatThousand);

                                }
                                else if (i == 8)
                                {
                                    div.InnerHtml = c.Month.BudgetY8.ToString(formatThousand);

                                }
                                else if (i == 9)
                                {
                                    div.InnerHtml = c.Month.BudgetY9.ToString(formatThousand);

                                }
                                else if (i == 10)
                                {
                                    div.InnerHtml = c.Month.BudgetY10.ToString(formatThousand);

                                }
                                else if (i == 11)
                                {
                                    div.InnerHtml = c.Month.BudgetY11.ToString(formatThousand);

                                }
                                else if (i == 12)
                                {
                                    div.InnerHtml = c.Month.BudgetY12.ToString(formatThousand);

                                }
                            }
                            else
                            {
                                div.InnerHtml = "---";
                            }

                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, ActivityType.ActivityCampaign, c.ActivityId, model, allocatedBy, i, strTab, view).ToString();
                            tr.InnerHtml += td.ToString();
                        }
                    }
                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildMonth(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, int month, string strTab, string view = "0")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            bool isTactic = false;
            bool isLineItem = false;
            bool isPlannedTab = strTab == "1" ? true : false;
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                parentClassName = ActivityType.ActivityCustomField;
            }
            else if (activityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = isPlannedTab ? "tacticLevel clueplanned" : "tacticLevel clueactual";
                parentClassName = "program";
                isTactic = true;
            }
            else if (activityType == "lineitem")
            {
                mainClass = "sub lineItem-lvl";
                innerClass = isPlannedTab ? "lineitemLevel clueplanned" : "lineitemLevel clueactual";
                parentClassName = "tactic";
                isLineItem = true;
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    if (!isPlannedTab && activityType == ActivityType.ActivityTactic)
                    {
                        if (model.Where(m => m.ActivityType == ActivityType.ActivityLineItem && m.ParentActivityId == p.ActivityId).ToList().Count == 0)
                        {
                            isLineItem = true;
                        }
                    }
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    string className = innerClass;
                    if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToLower())
                    {
                        if (month == 1)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY1 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY1.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY1.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.BudgetY1.ToString(formatThousand)) : p.Month.BudgetY1.ToString(formatThousand);
                            }

                        }
                        else if (month == 2)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY2 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY2.ToString(formatThousand));
                                    //className = p.Month.Feb <= p.ParentMonth.Feb ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY2.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.BudgetY2.ToString(formatThousand)) : p.Month.BudgetY2.ToString(formatThousand);
                            }

                        }
                        else if (month == 3)
                        {

                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY3 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY3.ToString(formatThousand));
                                    //className = p.Month.Mar <= p.ParentMonth.Mar ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY3.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.BudgetY3.ToString(formatThousand)) : p.Month.BudgetY3.ToString(formatThousand);
                            }

                        }
                        else if (month == 4)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY4 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY4.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY4.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.BudgetY4.ToString(formatThousand)) : p.Month.BudgetY4.ToString(formatThousand);
                            }

                        }
                        else if (month == 5)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY5 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY5.ToString(formatThousand));
                                    //className = p.Month.May <= p.ParentMonth.May ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY5.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.BudgetY5.ToString(formatThousand)) : p.Month.BudgetY5.ToString(formatThousand);
                            }

                        }
                        else if (month == 6)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY6 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY6.ToString(formatThousand));
                                    //className = p.Month.Jun <= p.ParentMonth.Jun ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY6.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.BudgetY6.ToString(formatThousand)) : p.Month.BudgetY6.ToString(formatThousand);
                            }

                        }
                        else if (month == 7)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY7 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY7.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY7.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.BudgetY7.ToString(formatThousand)) : p.Month.BudgetY7.ToString(formatThousand);
                            }

                        }
                        else if (month == 8)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY8 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY8.ToString(formatThousand));
                                    //className = p.Month.Aug <= p.ParentMonth.Aug ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY8.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.BudgetY8.ToString(formatThousand)) : p.Month.BudgetY8.ToString(formatThousand);
                            }

                        }
                        else if (month == 9)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY9 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY9.ToString(formatThousand));
                                    //className = p.Month.Sep <= p.ParentMonth.Sep ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY9.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.BudgetY9.ToString(formatThousand)) : p.Month.BudgetY9.ToString(formatThousand);
                            }


                        }
                        else if (month == 10)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY10 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY10.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY10.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.BudgetY10.ToString(formatThousand)) : p.Month.BudgetY10.ToString();
                            }

                        }
                        else if (month == 11)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY11 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY11.ToString(formatThousand));
                                    //className = p.Month.Nov <= p.ParentMonth.Nov ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY11.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.BudgetY11.ToString(formatThousand)) : p.Month.BudgetY11.ToString(formatThousand);
                            }
                        }
                        else if (month == 12)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY12 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY12.ToString(formatThousand));
                                    //className = p.Month.Dec <= p.ParentMonth.Dec ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY12.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.BudgetY12.ToString(formatThousand)) : p.Month.BudgetY12.ToString(formatThousand);
                            }
                        }
                    }
                    else if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
                    {
                        if (month == 1)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY1 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY1.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY1.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()], p.Month.BudgetY1.ToString(formatThousand)) : p.Month.BudgetY1.ToString(formatThousand);
                            }

                        }
                        else if (month == 2)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY4 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY4.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY4.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()], p.Month.BudgetY4.ToString(formatThousand)) : p.Month.BudgetY4.ToString(formatThousand);
                            }

                        }
                        else if (month == 3)
                        {
                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY7 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY7.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY7.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()], p.Month.BudgetY7.ToString(formatThousand)) : p.Month.BudgetY7.ToString(formatThousand);
                            }

                        }
                        else if (month == 4)
                        {

                            if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY10 <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.BudgetY10.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.BudgetY8.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.LineItemTypeId != null))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()], p.Month.BudgetY10.ToString(formatThousand)) : p.Month.BudgetY10.ToString(formatThousand);
                            }

                        }
                    }
                    else
                    {
                        if (strTab == "2")
                        {
                            if (month == 1)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY1 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.January.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.BudgetY1.ToString(formatThousand)) : p.Month.BudgetY1.ToString(formatThousand);
                                }

                            }
                            else if (month == 2)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY2 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.February.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.BudgetY2.ToString(formatThousand)) : p.Month.BudgetY2.ToString(formatThousand);
                                }

                            }
                            else if (month == 3)
                            {

                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY3 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.March.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.BudgetY3.ToString(formatThousand)) : p.Month.BudgetY3.ToString(formatThousand);
                                }

                            }
                            else if (month == 4)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY4 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.April.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.BudgetY4.ToString(formatThousand)) : p.Month.BudgetY4.ToString(formatThousand);
                                }

                            }
                            else if (month == 5)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY5 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.May.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.BudgetY5.ToString(formatThousand)) : p.Month.BudgetY5.ToString(formatThousand);
                                }

                            }
                            else if (month == 6)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY6 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.June.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.BudgetY6.ToString(formatThousand)) : p.Month.BudgetY6.ToString(formatThousand);
                                }

                            }
                            else if (month == 7)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY7 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.July.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.BudgetY7.ToString(formatThousand)) : p.Month.BudgetY7.ToString(formatThousand);
                                }

                            }
                            else if (month == 8)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY8 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.August.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.BudgetY8.ToString(formatThousand)) : p.Month.BudgetY8.ToString(formatThousand);
                                }

                            }
                            else if (month == 9)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY9 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.September.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.BudgetY9.ToString(formatThousand)) : p.Month.BudgetY9.ToString(formatThousand);
                                }


                            }
                            else if (month == 10)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY10 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.October.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.BudgetY10.ToString(formatThousand)) : p.Month.BudgetY10.ToString();
                                }

                            }
                            else if (month == 11)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY11 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.November.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.BudgetY11.ToString(formatThousand)) : p.Month.BudgetY11.ToString(formatThousand);
                                }
                            }
                            else if (month == 12)
                            {
                                if (activityType == ActivityType.ActivityLineItem && p.Month.BudgetY12 <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.December.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.BudgetY12.ToString(formatThousand)) : p.Month.BudgetY12.ToString(formatThousand);
                                }
                            }
                        }
                        else
                        {
                            divProgram.InnerHtml = "---";
                        }
                    }
                    divProgram.AddCssClass(className);
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonth(helper, ActivityType.ActivityProgram, p.ActivityId, model, allocatedBy, month, strTab, view).ToString();

                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonth(helper, ActivityType.ActivityTactic, p.ActivityId, model, allocatedBy, month, strTab, view).ToString();

                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonth(helper, ActivityType.ActivityLineItem, p.ActivityId, model, allocatedBy, month, strTab, view).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        #endregion

        #region Column3 Custom fields

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, string Tab, string view = "0")
        {
            string strViewBy = "";
            strViewBy = ActivityType.ActivityCustomField;
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.SingleOrDefault(pl => pl.ActivityType == ActivityType.ActivityPlan);
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("event-row");
                TagBuilder div = new TagBuilder("div");
                if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                {
                    double sumMonth = plan.Month.BudgetY1 + plan.Month.BudgetY2 + plan.Month.BudgetY3 + plan.Month.BudgetY4 + plan.Month.BudgetY5 + plan.Month.BudgetY6 + plan.Month.BudgetY7 + plan.Month.BudgetY8 + plan.Month.BudgetY9 + plan.Month.BudgetY10 + plan.Month.BudgetY11 + plan.Month.BudgetY12;
                    double unAllocated = plan.MainBudgeted - sumMonth;
                    div.InnerHtml = unAllocated.ToString(formatThousand);
                    if (unAllocated < 0)
                    {
                        div.AddCssClass("budgetError");
                    }
                }
                else
                {
                    div.InnerHtml = "---";
                    div.AddCssClass("firstLevel");

                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {

                    TagBuilder tr = new TagBuilder("tr");

                    //First
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("campaign-row audience");

                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", activityType + c.ActivityId);
                    div.AddCssClass("firstLevel");
                    if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                    {
                        double sumMonth = c.Month.BudgetY1 + c.Month.BudgetY2 + c.Month.BudgetY3 + c.Month.BudgetY4 + c.Month.BudgetY5 + c.Month.BudgetY6 + c.Month.BudgetY7 + c.Month.BudgetY8 + c.Month.BudgetY9 + c.Month.BudgetY10 + c.Month.BudgetY11 + c.Month.BudgetY12;
                        double unAllocated = 0;//c.MainBudgeted - sumMonth;
                        double childBudget = 0;
                        var campaignIds = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign && m.ParentActivityId == c.ActivityId).Select(m => m.ActivityId).ToList();
                        var childProgramIds = model.Where(m => m.ActivityType == ActivityType.ActivityProgram && campaignIds.Contains(m.ParentActivityId)).Select(m => m.ActivityId).ToList();
                        childBudget = model.Where(m => m.ActivityType == ActivityType.ActivityTactic && childProgramIds.Contains(m.ParentActivityId)).Select(m => m.MainBudgeted).Sum();
                        unAllocated = childBudget - sumMonth;
                        if (unAllocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }

                        div.InnerHtml = unAllocated.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml += "---";
                    }

                    td.InnerHtml = div.ToString();
                    td.InnerHtml += ChildSummary(helper, ActivityType.ActivityCampaign, c.ActivityId, model, "first", allocatedBy, Tab, view).ToString();
                    tr.InnerHtml += td.ToString();
                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentCostSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string allocatedBy, string tab, string view = "0")
        {
            string strViewBy = "";
            strViewBy = ActivityType.ActivityCustomField;
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.SingleOrDefault(pl => pl.ActivityType == ActivityType.ActivityPlan);
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("event-row");
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass("firstLevel");
                if (tab == "0")
                {
                    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                }
                else
                {
                    if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower())
                    {
                        double sumMonth = plan.Month.BudgetY1 + plan.Month.BudgetY2 + plan.Month.BudgetY3 + plan.Month.BudgetY4 + plan.Month.BudgetY5 + plan.Month.BudgetY6 + plan.Month.BudgetY7 + plan.Month.BudgetY8 + plan.Month.BudgetY9 + plan.Month.BudgetY10 + plan.Month.BudgetY11 + plan.Month.BudgetY12;
                        div.InnerHtml = sumMonth.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }
                }
                if (tab == "1")
                {
                    div.Attributes.Add("mainbudget", plan.MainBudgeted.ToString());
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }

            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {
                    TagBuilder tr = new TagBuilder("tr");

                    //Last
                    TagBuilder tdLast = new TagBuilder("td");
                    tdLast.AddCssClass("campaign-row audience");

                    TagBuilder divLast = new TagBuilder("div");
                    divLast.Attributes.Add("id", activityType + c.ActivityId);
                    divLast.AddCssClass("firstLevel");
                    if (tab == "0")
                    {
                        divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                    }
                    else
                    {
                        if (allocatedBy != "default")
                        {
                            double sumMonth = c.Month.BudgetY1 + c.Month.BudgetY2 + c.Month.BudgetY3 + c.Month.BudgetY4 + c.Month.BudgetY5 + c.Month.BudgetY6 + c.Month.BudgetY7 + c.Month.BudgetY8 + c.Month.BudgetY9 + c.Month.BudgetY10 + c.Month.BudgetY11 + c.Month.BudgetY12;
                            divLast.InnerHtml = sumMonth.ToString(formatThousand);
                        }
                        else
                        {
                            divLast.InnerHtml = "---";
                        }
                    }
                    if (tab == "1")
                    {
                        divLast.Attributes.Add("mainbudget", c.MainBudgeted.ToString());
                    }
                    tdLast.InnerHtml = divLast.ToString();
                    tdLast.InnerHtml += ChildCostSummary(helper, ActivityType.ActivityCampaign, c.ActivityId, model, "last", allocatedBy, tab, view).ToString();

                    tr.InnerHtml += tdLast.ToString();

                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }


        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string mode, string allocatedBy, string tab, string view = "0")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                parentClassName = ActivityType.ActivityCustomField;
            }
            else if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);

                    if (allocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToLower() && p.ActivityType != ActivityType.ActivityLineItem)
                    {
                        double sumMonth = p.Month.BudgetY1 + p.Month.BudgetY2 + p.Month.BudgetY3 + p.Month.BudgetY4 + p.Month.BudgetY5 + p.Month.BudgetY6 + p.Month.BudgetY7 + p.Month.BudgetY8 + p.Month.BudgetY9 + p.Month.BudgetY10 + p.Month.BudgetY11 + p.Month.BudgetY12;
                        double unAllocated = 0;//p.MainBudgeted - sumMonth;
                        double childBudget = 0;
                        if (p.ActivityType == Helpers.ActivityType.ActivityCampaign)
                        {
                            var subProgramIds = model.Where(m => m.ActivityType == ActivityType.ActivityProgram && m.ParentActivityId == p.ActivityId).Select(m => m.ActivityId).ToList();
                            childBudget = model.Where(t => t.ActivityType == ActivityType.ActivityTactic && subProgramIds.Contains(t.ParentActivityId)).Select(t => t.MainBudgeted).Sum();
                            unAllocated = childBudget - sumMonth;
                        }
                        else if (p.ActivityType == Helpers.ActivityType.ActivityProgram)
                        {
                            childBudget = model.Where(t => t.ActivityType == ActivityType.ActivityTactic && t.ParentActivityId == p.ActivityId).Select(t => t.MainBudgeted).Sum();
                            unAllocated = childBudget - sumMonth;
                        }
                        else
                        {
                            unAllocated = p.MainBudgeted - sumMonth;
                        }

                        divProgram.AddCssClass(innerClass);
                        if (unAllocated < 0)
                        {
                            divProgram.AddCssClass("budgetError");
                        }
                        divProgram.InnerHtml = unAllocated.ToString(formatThousand);
                    }
                    else
                    {
                        divProgram.InnerHtml += "---";
                        divProgram.AddCssClass(innerClass + " firstLevel");
                    }
                    div.InnerHtml += divProgram.ToString();
                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildSummary(helper, ActivityType.ActivityProgram, p.ActivityId, model, mode, allocatedBy, tab, view).ToString();
                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildSummary(helper, ActivityType.ActivityTactic, p.ActivityId, model, mode, allocatedBy, tab, view).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildSummary(helper, ActivityType.ActivityLineItem, p.ActivityId, model, mode, allocatedBy, tab, view).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildCostSummary(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModel> model, string mode, string allocatedBy, string tab, string view = "0")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                parentClassName = ActivityType.ActivityCustomField;
            }
            else if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    if (tab == "0")
                    {
                        if (activityType == ActivityType.ActivityLineItem || activityType == ActivityType.ActivityTactic)
                        {
                            divProgram.InnerHtml += "---";
                        }
                        else
                        {
                            divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                        }
                    }
                    else
                    {
                        if (allocatedBy != "default")
                        {
                            double sumMonth = p.Month.BudgetY1 + p.Month.BudgetY2 + p.Month.BudgetY3 + p.Month.BudgetY4 + p.Month.BudgetY5 + p.Month.BudgetY6 + p.Month.BudgetY7 + p.Month.BudgetY8 + p.Month.BudgetY9 + p.Month.BudgetY10 + p.Month.BudgetY11 + p.Month.BudgetY12;
                            divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                        }
                        else
                        {
                            divProgram.InnerHtml += "---";
                        }
                    }
                    if (tab == "1")
                    {
                        divProgram.Attributes.Add("mainbudget", p.MainBudgeted.ToString());
                    }
                    divProgram.AddCssClass(innerClass);

                    div.InnerHtml += divProgram.ToString();
                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildCostSummary(helper, ActivityType.ActivityProgram, p.ActivityId, model, mode, allocatedBy, tab, view).ToString();
                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildCostSummary(helper, ActivityType.ActivityTactic, p.ActivityId, model, mode, allocatedBy, tab, view).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildCostSummary(helper, ActivityType.ActivityLineItem, p.ActivityId, model, mode, allocatedBy, tab, view).ToString();


                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }


        #endregion

        #endregion


        #endregion //Advance Budgeting

        #region Custom Fields

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #844
        /// function generate html output for custom fields of campaign,program or tactic into inspect popup screen
        /// </summary>
        /// <param name="id">Plan Tactic Id or Plan Campaign Id or Plan Program Id</param>
        /// <param name="section">Parameter contains value from enum EntityType like Campaign or Program or Tactic.</param>
        /// <returns>If Plan Tactic or Plan Campaign or Plan Program contains custom fields than returns html string else empty string</returns>
        public static MvcHtmlString GenerateCustomFieldsForInspectPopup(int id, string section, string Status, int fieldCounter = 0, string mode = "ReadOnly")
        {
            //list of custom fields for particular campaign or Program or Tactic
            //Modified By Komal Rawal for #1292 dont apply isdeleted flag for tactics that are completed.
            List<CustomFieldModel> customFieldList = Common.GetCustomFields(id, section, Status);
            StringBuilder sb = new StringBuilder(string.Empty);


            DateTime? EntityCreatedDate = null;


            //fieldCounter variable for defining raw style
            if (customFieldList.Count != 0)
            {
                //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                //// User custom Restrictions
                var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.ID, true);
                MRPEntities db = new MRPEntities();
                string TacticType = "";
                int PlanID = 0;
                //if(id != 0)
                //{
                //    PlanID = Sessions.PlanId;
                //}
                if (section == Enums.EntityType.Tactic.ToString().ToLower() && id != 0)
                {
                    Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id) && pcptobj.IsDeleted == false).FirstOrDefault();
                    TacticType = pcpt.TacticTypeId.ToString();
                    EntityCreatedDate = pcpt.CreatedDate;
                    //var programId = pcpt.PlanProgramId;
                    //var CampignId = db.Plan_Campaign_Program.Where(pid => pid.PlanProgramId == programId).Select(pid => pid.PlanCampaignId).FirstOrDefault();
                    PlanID = pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId;

                }

                if (section == Enums.EntityType.Program.ToString().ToLower() && id != 0)
                {

                    Plan_Campaign_Program Program = db.Plan_Campaign_Program.Where(pid => pid.PlanProgramId == id).FirstOrDefault();
                    EntityCreatedDate = Program.CreatedDate;
                    PlanID = Program.Plan_Campaign.PlanId;

                }

                if (section == Enums.EntityType.Campaign.ToString().ToLower() && id != 0)
                {
                    Plan_Campaign Campaign = db.Plan_Campaign.Where(pid => pid.PlanCampaignId == id).FirstOrDefault();
                    PlanID = Campaign.PlanId;
                    EntityCreatedDate = Campaign.CreatedDate;

                }

                if (section == Enums.EntityType.Lineitem.ToString().ToLower() && id != 0)
                {
                    Plan_Campaign_Program_Tactic_LineItem Lineitem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lid => lid.PlanLineItemId.Equals(id)).FirstOrDefault();
                    EntityCreatedDate = Lineitem.CreatedDate;
                    PlanID = Lineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                }
                DateTime? DependencyDate = null;
                if (PlanID > 0)
                {
                    DependencyDate = db.Plans.Where(pid => pid.PlanId.Equals(PlanID)).Select(pid => pid.DependencyDate).FirstOrDefault();

                    if (DependencyDate != null)
                    {
                        DependencyDate = DependencyDate.Value.Date;
                    }

                }

                if (EntityCreatedDate != null)
                {
                    EntityCreatedDate = EntityCreatedDate.Value.Date;
                }
                List<int> customFieldIds = customFieldList.Select(cs => cs.customFieldId).ToList();
                var EntityValue = db.CustomField_Entity.Where(ct => ct.EntityId == id && customFieldIds.Contains(ct.CustomFieldId)).Select(ct => new { ct.Value, ct.CustomFieldId }).ToList();
                List<string> entityvalues = EntityValue.Select(a => a.Value).ToList();

                //// Start - Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                List<Models.CustomRestriction> lstEditableRestrictions = new List<CustomRestriction>();
                if (mode != Enums.InspectPopupMode.ReadOnly.ToString() && section == Enums.EntityType.Tactic.ToString().ToLower())
                {
                    lstEditableRestrictions = userCustomRestrictionList.Where(restriction => restriction.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit).ToList();
                }
                //// End - Added by Sohel Pathan on 28/01/2015 for PL ticket #1140

                //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                bool IsDefaultCustomRestrictionsEditable = Common.IsDefaultCustomRestrictionsEditable();
                string className, inputclassName, customFieldEntityValue, DropDownStyle, divPosition, require, name,
                        displayCheckbox, selectionMode, footerText, singlehover, trhover, footerclose, enableCheck, inputcolorcss, DisplayStyle;
                bool editableOptions, isEditable;
                string ParentField = "CustomField";
                if ((EntityCreatedDate >= DependencyDate) || id == 0 || DependencyDate == null)
                {


                    foreach (var item in customFieldList)
                    {
                        className = "span3 margin-top10";

                        DisplayStyle = " style=\"";
                        var IsSelected = false;
                        var ParentOptionID = item.option.Where(a => item.value.Contains(a.customFieldOptionId.ToString())).FirstOrDefault() != null ? item.option.Where(a => item.value.Contains(a.customFieldOptionId.ToString())).FirstOrDefault().ParentOptionId : new List<int>();
                        var ParentCustomFieldID = item.ParentId;
                        //List<string> SelectedTacticIds = item.option.Where(a => TacticType.Contains(a.ParentOptionId.ToString())) != null ? item.option.Where(a => TacticType.Contains(a.ParentOptionId.ToString())).Select(a => a.ParentOptionId.ToString()).ToList() : new List<string>();
                        //bool IsDefaultTacticType = SelectedTacticIds.Contains(TacticType);
                        List<string> val = new List<string>();
                        if (item.isChild && item.ParentId == 0)
                        {
                            val.Add(TacticType.ToString());
                            entityvalues.Add(TacticType.ToString());
                            ParentField = "TacticType";

                        }
                        else
                        {
                            ParentField = "CustomField";
                            val = customFieldList.Where(a => a.customFieldId == ParentCustomFieldID).FirstOrDefault() != null ? customFieldList.Where(a => a.customFieldId == ParentCustomFieldID).FirstOrDefault().value : new List<string>();
                        }
                        //   List<string> Childoptionstring = (objOption.ParentOptionId == null || objOption.ParentOptionId.Count() == 0 ? new List<string>() : objOption.ParentOptionId.Select(l => l.ToString()).ToList());
                        var IsSelectedParentsChild = item.option.Where(op => val.Intersect(op.ParentOptionId.Select(l => l.ToString()).ToList()).Any()).Any();
                        if (item.customFieldType == "TextBox" && item.isChild)
                        {
                            IsSelectedParentsChild = val.Intersect(item.ParentOptionId.Select(l => l.ToString()).ToList()).Any();
                        }
                        else
                        {
                            IsSelected = val.Intersect(ParentOptionID.Select(l => l.ToString()).ToList()).Any();
                        }

                        if (item.isChild == true && !IsSelectedParentsChild)
                        {

                            DisplayStyle += "display:none;";
                        }
                        else
                        {
                            DisplayStyle += "display:inline-block;";

                        }

                        //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                        editableOptions = false;
                        if (userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == item.customFieldId).Any())  //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                        {
                            editableOptions = lstEditableRestrictions.Where(customRestriction => customRestriction.CustomFieldId == item.customFieldId).Any();
                        }
                        else if (IsDefaultCustomRestrictionsEditable)   //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                        {
                            editableOptions = true;
                        }

                        if (item.customFieldType == Enums.CustomFieldType.TextBox.ToString() || editableOptions == true || (mode == Enums.InspectPopupMode.ReadOnly.ToString() && item.option.Count > 0))
                        {
                            //Modified By Komal Rawal for #1864
                            var ParentOptionId = (item.ParentOptionId == null || item.ParentOptionId.Count() == 0 ? "0" : string.Join(",", item.ParentOptionId));
                            if (item.isRequired)
                                sb.Append("<div class=\"" + className + "\"" + DisplayStyle + "\"ParentId =\"" + item.ParentId + "\"ParentOptionId =\"" + ParentOptionId + "\" ParentField =\"" + ParentField + "\"><p title=\"" + item.name + "\" class=\"ellipsis-left\">" + item.name + "</p> <span class='required-asterisk'>*</span>#VIEW_DETAIL_LINK#");
                            else
                                sb.Append("<div class=\"" + className + "\" " + DisplayStyle + "\"ParentId =\"" + item.ParentId + "\"ParentOptionId =\"" + ParentOptionId + "\" ParentField =\"" + ParentField + "\"><p title=\"" + item.name + "\" class=\"ellipsis\">" + item.name + "</p>");
                        }

                        //check if custom field type is textbox then generate textbox and if custom field type is dropdownlist then generate dropdownlist
                        if (item.customFieldType == Enums.CustomFieldType.TextBox.ToString())
                        {
                            inputclassName = "span12 input-small";
                            inputclassName += item.isRequired ? " resubmission" : string.Empty;
                            //When item value contains double quots then it would be replaced 
                            customFieldEntityValue = (item.value != null && item.value.Count > 0) ? item.value.First().Replace("\"", "&quot;") : string.Empty;
                            if (mode != Enums.InspectPopupMode.Edit.ToString())
                            {
                                // sb.Append("<input type=\"text\" readonly = \"true\" title=\"" + customFieldEntityValue + "\" value=\"" + customFieldEntityValue + "\" style=\"background:#F2F2F2;\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"" + inputclassName + "\"");
                                var _editdescription = Common.GenerateHTMLAttribute(customFieldEntityValue);
                                sb.Append("<div class='Attribute-content-text' id=\"cf_" + item.customFieldId + "\">" + WebUtility.HtmlDecode(_editdescription) + " </div>");
                            }
                            else
                            {
                                inputclassName += " input-setup";
                                sb.Append("<input type=\"text\" maxlength =\"255\" title=\"" + customFieldEntityValue + "\" value=\"" + customFieldEntityValue + "\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"" + inputclassName + "\"");
                                //If custom field is required than add attribute require
                                if (item.isRequired)
                                {
                                    sb.Append(" require=\"true\" oldValue=\"" + item.value + "\" label=\"" + item.name + "\"");
                                }
                                sb.Append("</input>");
                            }
                            sb.Append("</div>");
                            sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                            fieldCounter = fieldCounter + 1;
                        }
                        else if (item.customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                        {
                            if (mode == Enums.InspectPopupMode.Edit.ToString() && editableOptions == true)
                            {
                                //Added By komal Rawal
                                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                                List<int> viewoptionid = userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == item.customFieldId && restriction.Permission == ViewEditPermission).Select(res => res.CustomFieldOptionId).ToList();
                                List<int> Values = item.value.Select(int.Parse).ToList();

                                //   var itemvaluelist = item.value.Where(a => viewoptionid.Contains(int.Parse(a))).Select(a=>a).ToList();
                                var itemvaluelist = viewoptionid.Where(a => item.value.Contains(Convert.ToString(a))).Select(a => a).ToList();

                                //End


                                DropDownStyle = " style=\"";
                                divPosition = "style=\"position:relative;\"";
                                require = "";
                                name = "";
                                string addResubmissionClass = "";
                                if (item.isRequired)
                                {
                                    require = " require=\"true\" oldValue=\"#OLD_VALUE#\"";
                                    addResubmissionClass = "resubmission";
                                }
                                if (fieldCounter % 4 == 3)
                                {
                                    DropDownStyle += "top:0px;margin-top:40px;";
                                    //divPosition = "style=\"position:relative;\"";
                                }

                                displayCheckbox = "";
                                selectionMode = "Multi";
                                footerText = "< Single-selection";
                                singlehover = "";
                                trhover = "";
                                footerclose = "";
                                if ((item.value == null) || (item.value.Count <= 1 && itemvaluelist.Count <= 1))//Modified BY komal rawal for #1962 design issue
                                {
                                    displayCheckbox = "style=\"display:none;\"";
                                    selectionMode = "Single";
                                    footerText = "> Multi-selection";
                                    singlehover = "single-p";
                                    trhover = "trdropdownhover";
                                    footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:none;\"><span class=\"swap-text\">X close</span></a>";

                                }
                                else
                                {
                                    displayCheckbox = "";
                                    selectionMode = "Multi";
                                    footerText = "< Single-selection";
                                    singlehover = "";
                                    trhover = "";
                                    footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:block;\"><span class=\"swap-text\">X close</span></a>";
                                }
                                var IsDisplayBlock = false;
                                var NameList = new List<string>();
                                #region tactic inspect pop up

                                if (section == Enums.EntityType.Tactic.ToString().ToLower())
                                {

                                    sb.Append("<div tabindex=\"0\"" + divPosition + "><a class=\"dropdown_new_btn " + addResubmissionClass + "\"" + require + "  label=\"" + item.name + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                                    sb.Append("<div class=\"dropdown-wrapper paddingBottom20px editdropdown minimum-width215\"" + DropDownStyle + "\"><div class=\"drop-down_header\"><table border=\"0\" class=\"table_drpdwn\"> <thead class=\"top_head_attribute\" style=\"display:none;\"><tr><td scope=\"col\" class=\"value_header top-head-attribute-header2\" style=\"display:none;\"><span>Value</span></td><td scope=\"col\" class=\"revenue_header top-head-attribute-cvr\" code=\"cvr\" title=\"CVR(%)\">CVR(%)</td><td scope=\"col\" class=\"cost_header top-head-attribute-cost\" code=\"" + Enums.InspectStage.Cost.ToString() + "\" title=\"Cost(%)\">Cost(%)</td></tr></thead><tbody class=\"top_spacing_geography\">");
                                    //Added by Rahul shah on 05/11/2015 for PL #1731
                                    #region "Add Please Select option to list"
                                    CustomFieldOptionModel objSelectOption = new CustomFieldOptionModel();
                                    objSelectOption.value = "Please Select";
                                    objSelectOption.IsDefaultOption = true;
                                    #endregion

                                    item.option.Insert(0, objSelectOption);

                                    foreach (var objOption in item.option)
                                    {

                                        IsDisplayBlock = false;
                                        DisplayStyle = " style=\"";
                                        List<string> optionstring = (objOption.ParentOptionId == null || objOption.ParentOptionId.Count() == 0 ? new List<string>() : objOption.ParentOptionId.Select(l => l.ToString()).ToList());
                                        if (item.isChild == true)
                                        {
                                            if ((objOption.ChildOptionId == true && entityvalues.Intersect(optionstring).Any() || objOption.value.ToString() == "Please Select"))
                                            {
                                                DisplayStyle += "display:block;";
                                                IsDisplayBlock = true;
                                            }
                                            else
                                            {
                                                DisplayStyle += "display:none;";

                                            }
                                        }
                                        else
                                        {
                                            DisplayStyle += "display:block;";
                                            IsDisplayBlock = true;

                                        }
                                        //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                                        isEditable = false;
                                        if (userCustomRestrictionList.Count() == 0 && IsDefaultCustomRestrictionsEditable)  //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                                        {
                                            isEditable = true;
                                        }
                                        else if (userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == item.customFieldId && restriction.CustomFieldOptionId == objOption.customFieldOptionId).Count() == 0 && IsDefaultCustomRestrictionsEditable)   //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                                        {
                                            isEditable = true;
                                        }
                                        else
                                        {
                                            isEditable = lstEditableRestrictions.Where(customRestriction => customRestriction.CustomFieldId == item.customFieldId && customRestriction.CustomFieldOptionId == objOption.customFieldOptionId).Any();
                                        }

                                        if (isEditable) //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                                        {

                                            enableCheck = string.Empty;
                                            inputcolorcss = "class=\"multiselect-input-text-color-grey\"";
                                            if ((item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString())) || (item.option.Count == 1 && item.isRequired))
                                            {

                                                //var ParentOptionID = item.option.Where(a => item.value.Contains(a.customFieldOptionId.ToString() )).FirstOrDefault().ParentOptionId;

                                                //// var ParentCustomFieldID = customFieldList.Where(a => a.customFieldId == a.option.Where(b => b.ParentOptionId == ParentOptionID).Select(b => b.customFieldId).FirstOrDefault()).Select(a => a.ParentId).Distinct();
                                                // var ParentCustomFieldID = item.ParentId;
                                                // List<string> val = customFieldList.Where(a => a.customFieldId == ParentCustomFieldID).FirstOrDefault() != null ? customFieldList.Where(a => a.customFieldId == ParentCustomFieldID).FirstOrDefault().value : new List<string>();
                                                // List<string> ListIDs = objOption.ChildOptionIds.Select(a => a.ToString()).Distinct().ToList();
                                                // var IsSelected = val.Contains(ParentOptionID.ToString());

                                                if (item.isChild && !IsSelected && item.value.Contains(objOption.customFieldOptionId.ToString()) == false)
                                                {
                                                    name += "Please Select" + ", ";
                                                    inputcolorcss = string.Empty;
                                                    item.value.Clear();
                                                    selectionMode = "Single";
                                                    footerText = "> Multi-selection";
                                                    singlehover = "single-p";
                                                    trhover = "trdropdownhover";
                                                    footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:none;\"><span class=\"swap-text\">X close</span></a>";

                                                }
                                                else
                                                {
                                                    if (IsDisplayBlock)
                                                    {
                                                        name += objOption.value + ", ";
                                                        enableCheck = "checked=\"checked\"";
                                                        inputcolorcss = string.Empty;
                                                    }



                                                }
                                            }
                                            NameList = new List<string>();
                                            if (name != "")
                                            {
                                                NameList = name.Remove(name.Length - 2, 2).Split(',').ToList();
                                            }

                                            if (NameList.Count <= 1 && selectionMode != "Single" && item.value.Count <= 1) //Modified BY komal rawal for #1962 design issue
                                            {

                                                displayCheckbox = "style=\"display:none;\"";
                                                selectionMode = "Single";
                                                footerText = "> Multi-selection";
                                                singlehover = "single-p";
                                                trhover = "trdropdownhover";
                                                footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:none;\"><span class=\"swap-text\">X close</span></a>";
                                            }

                                            //Modified By Komal Rawal for #1864
                                            var ParentOption = (objOption.ParentOptionId == null || objOption.ParentOptionId.Count() == 0 ? "0" : string.Join(",", objOption.ParentOptionId));
                                            sb.Append("<tr class=\"" + trhover + "\"" + DisplayStyle + "\"ParentId =\"" + ParentOption + "\"><td class=\"first_show\"><label class=\"lblCustomCheckbox\"><input cf_id=\"" + item.customFieldId + "\" name=\"" + item.customFieldId + "\" type=\"checkbox\" value=\"" + objOption.customFieldOptionId + "\" class=\"  technology_chkbx\" " + enableCheck + " style=\"display:none;\" ><label class=\"lable_inline\"><p class=\"text_ellipsis " + singlehover + " minmax-width200\" title=\"" + objOption.value + "\">" + objOption.value + "</p></label></label></td><td class=\"first_hide\"><input " + inputcolorcss + " id=\"" + objOption.customFieldOptionId + "_cvr\" maxlength =\"3\" type=\"text\" name=\"textfield10\"></td><td class=\"first_hide\"> <input " + inputcolorcss + " id=\"" + objOption.customFieldOptionId + "_" + Enums.InspectStage.Cost.ToString() + "\" maxlength =\"3\" type=\"text\" name=\"textfield13\"></td></tr>");
                                        }
                                    }

                                    if (NameList.Count > 1)
                                    {
                                        footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:block;\"><span class=\"swap-text\">X close</span></a>";
                                    }

                                    sb.Append("</tbody><tfoot class=\"dropdown-table-footer\"><tr><td colspan=\"3\" class=\"advance\"><a href=\"#\" class=\"advance_a\" mode=\"" + selectionMode + "\"><span class=\"swap-text\">" + footerText + "</span>" + footerclose + "</a></td></tr></tfoot></table></div></div></div>");
                                    if (name.Length > 0)
                                    {
                                        name = name.Remove(name.Length - 2, 2);
                                    }
                                    else
                                    {
                                        name = "Please Select";
                                    }
                                    sb.Replace("#HEADER_OF_DROPDOWN#", name);
                                    sb.Replace("#OLD_VALUE#", name);
                                    sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                                    sb.Append("</div>");
                                    fieldCounter = fieldCounter + 1;
                                }
                                #endregion

                                #region Campaign/Program inspect popup
                                else
                                {

                                    sb.Append("<div tabindex=\"0\"" + divPosition + "><a class=\"dropdown_new_btn\"" + require + "  label=\"" + item.name + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                                    sb.Append("<div class=\"dropdown-wrapper editdropdown paddingBottom20px\"" + DropDownStyle + "min-width:150px !important;\"><div class=\"drop-down_header\"><table border=\"0\" class=\"table_drpdwn\"><tbody class=\"tbodycampaignprogram\">");
                                    //Added by Rahul shah on 05/11/2015 for PL #1731
                                    #region "Add Please Select option to list"
                                    CustomFieldOptionModel objSelectOption = new CustomFieldOptionModel();
                                    objSelectOption.value = "Please Select";
                                    objSelectOption.IsDefaultOption = true;
                                    #endregion

                                    item.option.Insert(0, objSelectOption);
                                    foreach (var objOption in item.option)
                                    {
                                        IsDisplayBlock = false;
                                        DisplayStyle = " style=\"";
                                        List<string> optionstring = (objOption.ParentOptionId == null || objOption.ParentOptionId.Count() == 0 ? new List<string>() : objOption.ParentOptionId.Select(l => l.ToString()).ToList());
                                        if (item.isChild == true)
                                        {
                                            if ((objOption.ChildOptionId == true && entityvalues.Intersect(optionstring).Any() || objOption.value.ToString() == "Please Select"))
                                            {
                                                DisplayStyle += "display:block;";
                                                IsDisplayBlock = true;
                                            }
                                            else
                                            {
                                                DisplayStyle += "display:none;";

                                            }
                                        }
                                        else
                                        {
                                            DisplayStyle += "display:block;";
                                            IsDisplayBlock = true;

                                        }
                                        enableCheck = string.Empty;

                                        if ((item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString())) || (item.option.Count == 1 && item.isRequired))
                                        {
                                            //List<string> ListIDs = objOption.ChildOptionIds.Select(a => a.ToString()).Distinct().ToList();
                                            //var IsSelected = item.value.Where(v => ListIDs.Contains(v)).Any();
                                            if (item.isChild && !IsSelected && item.value.Contains(objOption.customFieldOptionId.ToString()) == false)
                                            {
                                                name += "Please Select" + ", ";
                                                item.value.Clear();
                                            }
                                            else
                                            {
                                                if (IsDisplayBlock)
                                                {
                                                    name += objOption.value + ", ";
                                                    enableCheck = "checked=\"checked\"";
                                                }

                                            }

                                        }

                                        NameList = new List<string>();
                                        if (name != "")
                                        {
                                            NameList = name.Remove(name.Length - 2, 2).Split(',').ToList();
                                        }

                                        if (NameList.Count <= 1 && selectionMode != "Single")
                                        {

                                            displayCheckbox = "style=\"display:none;\"";
                                            selectionMode = "Single";
                                            footerText = "> Multi-selection";
                                            singlehover = "single-p";
                                            trhover = "trdropdownhover";
                                            footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:none;\"><span class=\"swap-text\">X close</span></a>";
                                        }
                                        //Modified By Komal Rawal for #1864
                                        var ParentOption = (objOption.ParentOptionId == null || objOption.ParentOptionId.Count() == 0 ? "0" : string.Join(",", objOption.ParentOptionId));
                                        sb.Append("<tr class=\"" + trhover + "\"" + DisplayStyle + "\"ParentId =\"" + ParentOption + "\"><td class=\"first_show\"><label class=\"lblCustomCheckbox\"><input cf_id=\"" + item.customFieldId + "\" name=\"" + item.customFieldId + "\" type=\"checkbox\" value=\"" + objOption.customFieldOptionId + "\" class=\"  technology_chkbx\" " + enableCheck + "" + displayCheckbox + "><label class=\"lable_inline\"><p class=\"text_ellipsis " + singlehover + " minmax-width200-program\" title=\"" + objOption.value + "\">" + objOption.value + "</p></label></label></td></tr>");
                                    }

                                    if (NameList.Count > 1)
                                    {
                                        footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:block;\"><span class=\"swap-text\">X close</span></a>";
                                    }
                                    sb.Append("</tbody><tfoot class=\"programcampaignborder\"><tr><td colspan=\"3\" class=\"advance\"><a href=\"#\" class=\"advance_a\" mode=\"" + selectionMode + "\"><span class=\"swap-text\">" + footerText + "</span></a></td></tr></tfoot></table></div></div></div>");
                                    if (name.Length > 0)
                                    {
                                        name = name.Remove(name.Length - 2, 2);
                                    }
                                    else
                                    {
                                        name = "Please Select";
                                    }
                                    sb.Replace("#HEADER_OF_DROPDOWN#", name);
                                    sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                                    sb.Append("</div>");
                                    fieldCounter = fieldCounter + 1;
                                }
                                #endregion
                            }
                            else if (mode == Enums.InspectPopupMode.ReadOnly.ToString())
                            {
                                customFieldEntityValue = "";
                                if (item.option.Count != 0)
                                {
                                    sb.Append("<input type=\"text\" readonly = \"true\" value=\"#CUSTOMFEILD_VALUE#\" title=\"#CUSTOMFEILD_VALUE#\" style=\"background:#F2F2F2;\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"span12 input-small\"/>");

                                    #region tactic inspect pop up

                                    if (section == Enums.EntityType.Tactic.ToString().ToLower() && item.value != null && item.value.Count > 1)
                                    {
                                        DropDownStyle = " style=\"position:absolute;";
                                        divPosition = "style=\"position:relative;\"";//modified by mitesh vaishnav for PL ticket 1497
                                        if (fieldCounter % 4 == 3)
                                        {
                                            DropDownStyle = " top:0px;";
                                            // divPosition = "style=\"position:relative;\"";//commented by mitesh vaishnav for PL ticket 1497
                                        }

                                        sb.Append("<div " + divPosition + "><div class=\"dropdown-wrapper\"" + DropDownStyle + "\"><div class=\"drop-down_header viewmodedropdown geography_popup\"><table border=\"0\" class=\"table_drpdwn\"> <thead class=\"top_head_attribute\" style=\"display:none;\"><tr><td scope=\"col\" class=\"value_header top-head-attribute-header2 padding-left20\" style=\"display:none;\"><span>Value</span></td><td scope=\"col\" class=\"revenue_header top-head-attribute-cvr\" code=\"cvr\" title=\"CVR(%)\">CVR(%)</td><td scope=\"col\" class=\"cost_header top-head-attribute-cost\" code=\"" + Enums.InspectStage.Cost.ToString() + "\" title=\"Cost(%)\">Cost(%)</td></tr></thead><tbody class=\"top_spacing_geography\">");
                                        foreach (var objOption in item.option)
                                        {
                                            //check - if custom field's value inserted before from dropdownlist then set it as selected
                                            if (item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString()))
                                            {
                                                sb.Append("<tr><td class=\"first_show\"><label class=\"lable_inline\" optionId=\"" + objOption.customFieldOptionId + "\"><p class=\"text_ellipsis attributes-view-p\" title=\"" + objOption.value + "\">" + objOption.value + "</p></label></td><td class=\"first_hide\"><input id=\"" + objOption.customFieldOptionId + "_cvr\" disabled=\"disabled\" maxlength =\"3\" type=\"text\" name=\"textfield10\"></td><td class=\"first_hide\"> <input id=\"" + objOption.customFieldOptionId + "_" + Enums.InspectStage.Cost.ToString() + "\" disabled=\"disabled\" maxlength =\"3\" type=\"text\" name=\"textfield13\"></td></tr>");
                                                customFieldEntityValue += item.value != null ? objOption.value.Replace("\"", "&quot;") + ", " : string.Empty;
                                            }
                                        }
                                        sb.Append("</tbody> <tfoot><tr><td colspan=\"3\" class=\"advance\"><a href=\"#\" class=\"advance_a\"><span class=\"swap-text\">X close</span></a></td></tr></tfoot></table></div></div></div>");

                                        sb = sb.Replace("#VIEW_DETAIL_LINK#", "<span class=\"new_tag\"><a href=\"#\" class=\"viewattrlnk\">View Attribution</a></span>");

                                    }
                                    #endregion
                                    else
                                    {
                                        foreach (var objOption in item.option)
                                        {
                                            //check - if custom field's value inserted before from dropdownlist then set it as selected
                                            if (item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString()))
                                            {
                                                customFieldEntityValue += item.value != null ? objOption.value.Replace("\"", "&quot;") + ", " : string.Empty;
                                            }
                                        }
                                        sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                                    }
                                    if (customFieldEntityValue.Length > 0)
                                    {
                                        customFieldEntityValue = customFieldEntityValue.Remove(customFieldEntityValue.Length - 2, 2);
                                    }

                                    sb = sb.Replace("#CUSTOMFEILD_VALUE#", customFieldEntityValue);
                                    sb.Append("</div>");
                                    fieldCounter = fieldCounter + 1;
                                }
                            }
                        }
                    }
                }

                else
                {
                    foreach (var item in customFieldList)
                    {

                        className = "span3 margin-top10";
                        //if (fieldCounter % 4 != 0 && fieldCounter != 0)
                        //{
                        //    className += " paddingleft25px";
                        //}
                        //else
                        //{
                        //    className += "\" style=\"clear:both;";
                        //}

                        //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                        editableOptions = false;
                        if (userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == item.customFieldId).Any())  //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                        {
                            editableOptions = lstEditableRestrictions.Where(customRestriction => customRestriction.CustomFieldId == item.customFieldId).Any();
                        }
                        else if (IsDefaultCustomRestrictionsEditable)   //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                        {
                            editableOptions = true;
                        }

                        if (item.customFieldType == Enums.CustomFieldType.TextBox.ToString() || editableOptions == true || (mode == Enums.InspectPopupMode.ReadOnly.ToString() && item.option.Count > 0))
                        {
                            if (item.isRequired)
                                sb.Append("<div class=\"" + className + "\"><p title=\"" + item.name + "\" class=\"ellipsis-left\">" + item.name + "</p> <span class='required-asterisk'>*</span>#VIEW_DETAIL_LINK#");
                            else
                                sb.Append("<div class=\"" + className + "\"><p title=\"" + item.name + "\" class=\"ellipsis\">" + item.name + "</p>");
                        }

                        //check if custom field type is textbox then generate textbox and if custom field type is dropdownlist then generate dropdownlist
                        if (item.customFieldType == Enums.CustomFieldType.TextBox.ToString())
                        {
                            inputclassName = "span12 input-small";
                            inputclassName += item.isRequired ? " resubmission" : string.Empty;
                            //When item value contains double quots then it would be replaced 
                            customFieldEntityValue = (item.value != null && item.value.Count > 0) ? item.value.First().Replace("\"", "&quot;") : string.Empty;
                            if (mode != Enums.InspectPopupMode.Edit.ToString())
                            {
                                // sb.Append("<input type=\"text\" readonly = \"true\" title=\"" + customFieldEntityValue + "\" value=\"" + customFieldEntityValue + "\" style=\"background:#F2F2F2;\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"" + inputclassName + "\"");
                                var _editdescription = Common.GenerateHTMLAttribute(customFieldEntityValue);
                                sb.Append("<div class='Attribute-content-text' id=\"cf_" + item.customFieldId + "\">" + WebUtility.HtmlDecode(_editdescription) + " </div>");
                            }
                            else
                            {
                                inputclassName += " input-setup";
                                sb.Append("<input type=\"text\" maxlength =\"255\" title=\"" + customFieldEntityValue + "\" value=\"" + customFieldEntityValue + "\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"" + inputclassName + "\"");
                                //If custom field is required than add attribute require
                                if (item.isRequired)
                                {
                                    sb.Append(" require=\"true\" oldValue=\"" + item.value + "\" label=\"" + item.name + "\"");
                                }
                                sb.Append("</input>");
                            }
                            sb.Append("</div>");
                            sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                            fieldCounter = fieldCounter + 1;
                        }
                        else if (item.customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                        {
                            if (mode == Enums.InspectPopupMode.Edit.ToString() && editableOptions == true)
                            {
                                //Added By komal Rawal
                                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                                List<int> viewoptionid = userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == item.customFieldId && restriction.Permission == ViewEditPermission).Select(res => res.CustomFieldOptionId).ToList();
                                List<int> Values = item.value.Select(int.Parse).ToList();

                                //   var itemvaluelist = item.value.Where(a => viewoptionid.Contains(int.Parse(a))).Select(a=>a).ToList();
                                var itemvaluelist = viewoptionid.Where(a => item.value.Contains(Convert.ToString(a))).Select(a => a).ToList();

                                //End


                                DropDownStyle = " style=\"";
                                divPosition = "style=\"position:relative;\"";
                                require = "";
                                name = "";
                                string addResubmissionClass = "";
                                if (item.isRequired)
                                {
                                    require = " require=\"true\" oldValue=\"#OLD_VALUE#\"";
                                    addResubmissionClass = "resubmission";
                                }
                                if (fieldCounter % 4 == 3)
                                {
                                    DropDownStyle += "top:0px;margin-top:40px;";
                                    //divPosition = "style=\"position:relative;\"";
                                }
                                displayCheckbox = "";
                                selectionMode = "Multi";
                                footerText = "< Single-selection";
                                singlehover = "";
                                trhover = "";
                                footerclose = "";
                                if ((item.value == null) || (item.value != null && itemvaluelist.Count <= 1))
                                {
                                    displayCheckbox = "style=\"display:none;\"";
                                    selectionMode = "Single";
                                    footerText = "> Multi-selection";
                                    singlehover = "single-p";
                                    trhover = "trdropdownhover";
                                    footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:none;\"><span class=\"swap-text\">X close</span></a>";
                                }
                                else
                                {
                                    displayCheckbox = "";
                                    selectionMode = "Multi";
                                    footerText = "< Single-selection";
                                    singlehover = "";
                                    trhover = "";
                                    footerclose = "<a id=\"aclose_tag\" href=\"#\" class=\"close_a\" style=\"display:block;\"><span class=\"swap-text\">X close</span></a>";
                                }

                                #region tactic inspect pop up

                                if (section == Enums.EntityType.Tactic.ToString().ToLower())
                                {

                                    sb.Append("<div tabindex=\"0\"" + divPosition + "><a class=\"dropdown_new_btn " + addResubmissionClass + "\"" + require + "  label=\"" + item.name + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                                    sb.Append("<div class=\"dropdown-wrapper paddingBottom20px editdropdown minimum-width215\"" + DropDownStyle + "\"><div class=\"drop-down_header\"><table border=\"0\" class=\"table_drpdwn\"> <thead class=\"top_head_attribute\" style=\"display:none;\"><tr><td scope=\"col\" class=\"value_header top-head-attribute-header2\" style=\"display:none;\"><span>Value</span></td><td scope=\"col\" class=\"revenue_header top-head-attribute-cvr\" code=\"cvr\" title=\"CVR(%)\">CVR(%)</td><td scope=\"col\" class=\"cost_header top-head-attribute-cost\" code=\"" + Enums.InspectStage.Cost.ToString() + "\" title=\"Cost(%)\">Cost(%)</td></tr></thead><tbody class=\"top_spacing_geography\">");
                                    //Added by Rahul shah on 05/11/2015 for PL #1731
                                    #region "Add Please Select option to list"
                                    CustomFieldOptionModel objSelectOption = new CustomFieldOptionModel();
                                    objSelectOption.value = "Please Select";
                                    objSelectOption.IsDefaultOption = true;
                                    #endregion

                                    item.option.Insert(0, objSelectOption);

                                    foreach (var objOption in item.option)
                                    {
                                        //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                                        isEditable = false;
                                        if (userCustomRestrictionList.Count() == 0 && IsDefaultCustomRestrictionsEditable)  //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                                        {
                                            isEditable = true;
                                        }
                                        else if (userCustomRestrictionList.Where(restriction => restriction.CustomFieldId == item.customFieldId && restriction.CustomFieldOptionId == objOption.customFieldOptionId).Count() == 0 && IsDefaultCustomRestrictionsEditable)   //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                                        {
                                            isEditable = true;
                                        }
                                        else
                                        {
                                            isEditable = lstEditableRestrictions.Where(customRestriction => customRestriction.CustomFieldId == item.customFieldId && customRestriction.CustomFieldOptionId == objOption.customFieldOptionId).Any();
                                        }

                                        if (isEditable) //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                                        {
                                            enableCheck = string.Empty;
                                            inputcolorcss = "class=\"multiselect-input-text-color-grey\"";
                                            if ((item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString())) || (item.option.Count == 1 && item.isRequired))
                                            {
                                                name += objOption.value + ", ";
                                                enableCheck = "checked=\"checked\"";
                                                inputcolorcss = string.Empty;
                                            }

                                            sb.Append("<tr class=\"" + trhover + "\"><td class=\"first_show\"><label class=\"lblCustomCheckbox\"><input cf_id=\"" + item.customFieldId + "\" name=\"" + item.customFieldId + "\" type=\"checkbox\" value=\"" + objOption.customFieldOptionId + "\" class=\"  technology_chkbx\" " + enableCheck + " style=\"display:none;\" ><label class=\"lable_inline\"><p class=\"text_ellipsis " + singlehover + " minmax-width200\" title=\"" + objOption.value + "\">" + objOption.value + "</p></label></label></td><td class=\"first_hide\"><input " + inputcolorcss + " id=\"" + objOption.customFieldOptionId + "_cvr\" maxlength =\"3\" type=\"text\" name=\"textfield10\"></td><td class=\"first_hide\"> <input " + inputcolorcss + " id=\"" + objOption.customFieldOptionId + "_" + Enums.InspectStage.Cost.ToString() + "\" maxlength =\"3\" type=\"text\" name=\"textfield13\"></td></tr>");
                                        }
                                    }
                                    sb.Append("</tbody><tfoot class=\"dropdown-table-footer\"><tr><td colspan=\"3\" class=\"advance\"><a href=\"#\" class=\"advance_a\" mode=\"" + selectionMode + "\"><span class=\"swap-text\">" + footerText + "</span>" + footerclose + "</a></td></tr></tfoot></table></div></div></div>");
                                    if (name.Length > 0)
                                    {
                                        name = name.Remove(name.Length - 2, 2);
                                    }
                                    else
                                    {
                                        name = "Please Select";
                                    }
                                    sb.Replace("#HEADER_OF_DROPDOWN#", name);
                                    sb.Replace("#OLD_VALUE#", name);
                                    sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                                    sb.Append("</div>");
                                    fieldCounter = fieldCounter + 1;
                                }
                                #endregion

                                #region Campaign/Program inspect popup
                                else
                                {

                                    sb.Append("<div tabindex=\"0\"" + divPosition + "><a class=\"dropdown_new_btn\"" + require + "  label=\"" + item.name + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                                    sb.Append("<div class=\"dropdown-wrapper editdropdown paddingBottom20px\"" + DropDownStyle + "min-width:150px !important;\"><div class=\"drop-down_header\"><table border=\"0\" class=\"table_drpdwn\"><tbody class=\"tbodycampaignprogram\">");
                                    //Added by Rahul shah on 05/11/2015 for PL #1731
                                    #region "Add Please Select option to list"
                                    CustomFieldOptionModel objSelectOption = new CustomFieldOptionModel();
                                    objSelectOption.value = "Please Select";
                                    objSelectOption.IsDefaultOption = true;
                                    #endregion

                                    item.option.Insert(0, objSelectOption);
                                    foreach (var objOption in item.option)
                                    {
                                        enableCheck = string.Empty;

                                        if ((item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString())) || (item.option.Count == 1 && item.isRequired))
                                        {
                                            name += objOption.value + ", ";
                                            enableCheck = "checked=\"checked\"";

                                        }
                                        sb.Append("<tr class=\"" + trhover + "\"><td class=\"first_show\"><label class=\"lblCustomCheckbox\"><input cf_id=\"" + item.customFieldId + "\" name=\"" + item.customFieldId + "\" type=\"checkbox\" value=\"" + objOption.customFieldOptionId + "\" class=\"  technology_chkbx\" " + enableCheck + "" + displayCheckbox + "><label class=\"lable_inline\"><p class=\"text_ellipsis " + singlehover + "\" title=\"" + objOption.value + "\">" + objOption.value + "</p></label></label></td></tr>");
                                    }
                                    sb.Append("</tbody><tfoot class=\"programcampaignborder\"><tr><td colspan=\"3\" class=\"advance\"><a href=\"#\" class=\"advance_a\" mode=\"" + selectionMode + "\"><span class=\"swap-text\">" + footerText + "</span></a></td></tr></tfoot></table></div></div></div>");
                                    if (name.Length > 0)
                                    {
                                        name = name.Remove(name.Length - 2, 2);
                                    }
                                    else
                                    {
                                        name = "Please Select";
                                    }
                                    sb.Replace("#HEADER_OF_DROPDOWN#", name);
                                    sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                                    sb.Append("</div>");
                                    fieldCounter = fieldCounter + 1;
                                }
                                #endregion
                            }
                            else if (mode == Enums.InspectPopupMode.ReadOnly.ToString())
                            {
                                customFieldEntityValue = "";
                                if (item.option.Count != 0)
                                {
                                    sb.Append("<input type=\"text\" readonly = \"true\" value=\"#CUSTOMFEILD_VALUE#\" title=\"#CUSTOMFEILD_VALUE#\" style=\"background:#F2F2F2;\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"span12 input-small\"/>");

                                    #region tactic inspect pop up

                                    if (section == Enums.EntityType.Tactic.ToString().ToLower() && item.value != null && item.value.Count > 1)
                                    {
                                        DropDownStyle = " style=\"position:absolute;";
                                        divPosition = "style=\"position:relative;\"";//modified by mitesh vaishnav for PL ticket 1497
                                        if (fieldCounter % 4 == 3)
                                        {
                                            DropDownStyle = " top:0px;";
                                            // divPosition = "style=\"position:relative;\"";//commented by mitesh vaishnav for PL ticket 1497
                                        }

                                        sb.Append("<div " + divPosition + "><div class=\"dropdown-wrapper\"" + DropDownStyle + "\"><div class=\"drop-down_header viewmodedropdown geography_popup\"><table border=\"0\" class=\"table_drpdwn\"> <thead class=\"top_head_attribute\" style=\"display:none;\"><tr><td scope=\"col\" class=\"value_header top-head-attribute-header2 padding-left20\" style=\"display:none;\"><span>Value</span></td><td scope=\"col\" class=\"revenue_header top-head-attribute-cvr\" code=\"cvr\" title=\"CVR(%)\">CVR(%)</td><td scope=\"col\" class=\"cost_header top-head-attribute-cost\" code=\"" + Enums.InspectStage.Cost.ToString() + "\" title=\"Cost(%)\">Cost(%)</td></tr></thead><tbody class=\"top_spacing_geography\">");
                                        foreach (var objOption in item.option)
                                        {
                                            //check - if custom field's value inserted before from dropdownlist then set it as selected
                                            if (item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString()))
                                            {
                                                sb.Append("<tr><td class=\"first_show\"><label class=\"lable_inline\" optionId=\"" + objOption.customFieldOptionId + "\"><p class=\"text_ellipsis attributes-view-p\" title=\"" + objOption.value + "\">" + objOption.value + "</p></label></td><td class=\"first_hide\"><input id=\"" + objOption.customFieldOptionId + "_cvr\" disabled=\"disabled\" maxlength =\"3\" type=\"text\" name=\"textfield10\"></td><td class=\"first_hide\"> <input id=\"" + objOption.customFieldOptionId + "_" + Enums.InspectStage.Cost.ToString() + "\" disabled=\"disabled\" maxlength =\"3\" type=\"text\" name=\"textfield13\"></td></tr>");
                                                customFieldEntityValue += item.value != null ? objOption.value.Replace("\"", "&quot;") + ", " : string.Empty;
                                            }
                                        }
                                        sb.Append("</tbody> <tfoot><tr><td colspan=\"3\" class=\"advance\"><a href=\"#\" class=\"advance_a\"><span class=\"swap-text\">X close</span></a></td></tr></tfoot></table></div></div></div>");

                                        sb = sb.Replace("#VIEW_DETAIL_LINK#", "<span class=\"new_tag\"><a href=\"#\" class=\"viewattrlnk\">View Attribution</a></span>");

                                    }
                                    #endregion
                                    else
                                    {
                                        foreach (var objOption in item.option)
                                        {
                                            //check - if custom field's value inserted before from dropdownlist then set it as selected
                                            if (item.value != null && item.value.Contains(objOption.customFieldOptionId.ToString()))
                                            {
                                                customFieldEntityValue += item.value != null ? objOption.value.Replace("\"", "&quot;") + ", " : string.Empty;
                                            }
                                        }
                                        sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                                    }
                                    if (customFieldEntityValue.Length > 0)
                                    {
                                        customFieldEntityValue = customFieldEntityValue.Remove(customFieldEntityValue.Length - 2, 2);
                                    }

                                    sb = sb.Replace("#CUSTOMFEILD_VALUE#", customFieldEntityValue);
                                    sb.Append("</div>");
                                    fieldCounter = fieldCounter + 1;
                                }




                            }
                        }



                    }
                }
            }
            return new MvcHtmlString(sb.ToString());
        }


        #endregion

        #region Budgeting Report

        /// <summary>
        /// Render activity names for all campaigns
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityMainParentReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModelReport c in model.Where(p => p.ActivityType == ActivityType.ActivityPlan && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {
                    TagBuilder tr = new TagBuilder("tr");
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("campaign-rowReport audience");


                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", activityType + c.ActivityId);
                    div.AddCssClass("firstLevel");
                    TagBuilder aLink = new TagBuilder("span");
                    if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                    {
                        TagBuilder aAccordian = new TagBuilder("a");
                        aAccordian.AddCssClass("accordionClick");
                        div.InnerHtml = aAccordian.ToString();
                    }
                    else
                    {
                        aLink.Attributes.Add("style", "padding-left:20px;");
                    }
                    aLink.InnerHtml = c.ActivityName;
                    div.InnerHtml += aLink.ToString();
                    td.InnerHtml = div.ToString();
                    td.InnerHtml += ActivityChildReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model).ToString();
                    tr.InnerHtml = td.ToString();
                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Render activity names for all children
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityChildReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model)
        {

            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            string childActivity = "tactic";
            bool needAccrodian = true;
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevelReport";
                childActivity = "program";
            }
            else if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                childActivity = "tactic";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
                childActivity = "lineitem";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
                needAccrodian = false;
                childActivity = "";
            }

            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    divProgram.AddCssClass(innerClass);

                    TagBuilder aLink = new TagBuilder("span");
                    if (needAccrodian)
                    {
                        if (model.Any(p1 => p1.ActivityType == childActivity && p1.ParentActivityId == p.ActivityId))
                        {
                            TagBuilder aAccordian = new TagBuilder("a");
                            aAccordian.AddCssClass("accordionClick");
                            divProgram.InnerHtml = aAccordian.ToString();
                        }
                        else
                        {
                            aLink.Attributes.Add("style", "padding-left:20px;");
                        }
                    }

                    aLink.InnerHtml = p.ActivityName;
                    divProgram.InnerHtml += aLink.ToString();
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ActivityChildReport(helper, ActivityType.ActivityProgram, p.ActivityId, model).ToString();
                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ActivityChildReport(helper, ActivityType.ActivityTactic, p.ActivityId, model).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ActivityChildReport(helper, ActivityType.ActivityLineItem, p.ActivityId, model).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Render month header and plans month values
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="activityId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static MvcHtmlString PlanMonthReport(this HtmlHelper helper, string activityType, string activityId, List<BudgetModelReport> model, string allocatedBy, bool isPlanTab)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            TagBuilder trInnerHeader = new TagBuilder("tr");
            int incrementCount = 1;
            bool isQuarter = false;
            if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
            {
                incrementCount = 3;
                isQuarter = true;
            }
            BudgetModelReport objMain = model.FirstOrDefault(main => main.ActivityType == activityType && main.ActivityId == activityId);
            string className = "";// "event-row";
            double actualValue = 0;
            double plannedValue = 0;
            double childAllocatedValue = 0;
            for (int i = 1; i <= 12; i += incrementCount)
            {
                TagBuilder tdHeader = new TagBuilder("td");
                //tdHeader.AddCssClass("event-row");
                TagBuilder divHeader = new TagBuilder("div");

                if (isQuarter)
                {
                    divHeader.InnerHtml = "Q" + ((i / incrementCount) + 1);
                }
                else
                {
                    DateTime dt = new DateTime(2012, i, 1);
                    divHeader.InnerHtml = dt.ToString("MMM").ToUpper();
                }

                TagBuilder tdHeaderInnerActual = new TagBuilder("td");
                TagBuilder divHeaderInnerActual = new TagBuilder("div");
                TagBuilder tdValueInnerActual = new TagBuilder("td");
                TagBuilder divValueInnerActual = new TagBuilder("div");
                divHeaderInnerActual.InnerHtml = "Actual";
                TagBuilder aActual = new TagBuilder("a");
                aActual.AddCssClass("UpperArrowReport");
                aActual.Attributes.Add("id", "Actual_Up_" + i);
                divHeaderInnerActual.InnerHtml += aActual;
                aActual = new TagBuilder("a");
                aActual.AddCssClass("DownArrowReport");
                aActual.Attributes.Add("id", "Actual_Down_" + i);
                divHeaderInnerActual.InnerHtml += aActual;

                divValueInnerActual.Attributes.Add("id", activityType + activityId);

                switch (i)
                {
                    case 1:
                        actualValue = objMain.MonthActual.BudgetY1;
                        plannedValue = objMain.MonthPlanned.BudgetY1;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY1;
                        break;
                    case 2:
                        actualValue = objMain.MonthActual.BudgetY2;
                        plannedValue = objMain.MonthPlanned.BudgetY2;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY2;
                        break;
                    case 3:
                        actualValue = objMain.MonthActual.BudgetY3;
                        plannedValue = objMain.MonthPlanned.BudgetY3;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY3;
                        break;
                    case 4:
                        actualValue = objMain.MonthActual.BudgetY4;
                        plannedValue = objMain.MonthPlanned.BudgetY4;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY4;
                        break;
                    case 5:
                        actualValue = objMain.MonthActual.BudgetY5;
                        plannedValue = objMain.MonthPlanned.BudgetY5;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY5;
                        break;
                    case 6:
                        actualValue = objMain.MonthActual.BudgetY6;
                        plannedValue = objMain.MonthPlanned.BudgetY6;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY6;
                        break;
                    case 7:
                        actualValue = objMain.MonthActual.BudgetY7;
                        plannedValue = objMain.MonthPlanned.BudgetY7;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY7;
                        break;
                    case 8:
                        actualValue = objMain.MonthActual.BudgetY8;
                        plannedValue = objMain.MonthPlanned.BudgetY8;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY8;
                        break;
                    case 9:
                        actualValue = objMain.MonthActual.BudgetY9;
                        plannedValue = objMain.MonthPlanned.BudgetY9;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY9;
                        break;
                    case 10:
                        actualValue = objMain.MonthActual.BudgetY10;
                        plannedValue = objMain.MonthPlanned.BudgetY10;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY10;
                        break;
                    case 11:
                        actualValue = objMain.MonthActual.BudgetY11;
                        plannedValue = objMain.MonthPlanned.BudgetY11;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY11;
                        break;
                    case 12:
                        actualValue = objMain.MonthActual.BudgetY12;
                        plannedValue = objMain.MonthPlanned.BudgetY12;
                        childAllocatedValue = objMain.ChildMonthAllocated.BudgetY12;
                        break;
                }

                TagBuilder span = new TagBuilder("span");
                double dblProgress = 0;
                // Actual
                className = "";
                divValueInnerActual.InnerHtml = actualValue.ToString(formatThousand);
                if (isPlanTab)
                {
                    if (actualValue > childAllocatedValue)
                    {
                        className += budgetError;
                        divValueInnerActual.Attributes.Add("OverBudget", Math.Abs(childAllocatedValue - actualValue).ToString(formatThousand));
                    }
                    span = new TagBuilder("span");
                    dblProgress = (actualValue == 0 && childAllocatedValue == 0) ? 0 : (actualValue > 0 && childAllocatedValue == 0) ? 101 : actualValue / childAllocatedValue * 100;

                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        span.AddCssClass("progressBar");
                    }
                    divValueInnerActual.InnerHtml += span.ToString();

                }
                divValueInnerActual.AddCssClass(className);
                tdHeader.InnerHtml += divHeader.ToString();
                tdHeader.Attributes.Add("Colspan", "3");
                trHeader.InnerHtml += tdHeader.ToString();

                tdHeaderInnerActual.InnerHtml += divHeaderInnerActual.ToString();
                trInnerHeader.InnerHtml += tdHeaderInnerActual.ToString();

                tdValueInnerActual.InnerHtml += divValueInnerActual.ToString();
                trValue.InnerHtml += tdValueInnerActual.ToString();


                // For Planned
                className = "";
                TagBuilder tdHeaderInnerPlanned = new TagBuilder("td");
                TagBuilder divHeaderInnerPlanned = new TagBuilder("div");
                TagBuilder tdValueInnerPlanned = new TagBuilder("td");
                TagBuilder divValueInnerPlanned = new TagBuilder("div");
                divHeaderInnerPlanned.InnerHtml = "Planned";
                TagBuilder aPlanned = new TagBuilder("a");
                aPlanned.AddCssClass("UpperArrowReport");
                aPlanned.Attributes.Add("id", "Planned_Up_" + i);
                divHeaderInnerPlanned.InnerHtml += aPlanned;
                aPlanned = new TagBuilder("a");
                aPlanned.AddCssClass("DownArrowReport");
                aPlanned.Attributes.Add("id", "Planned_Down_" + i);
                divHeaderInnerPlanned.InnerHtml += aPlanned;

                divValueInnerPlanned.Attributes.Add("id", activityType + activityId);
                divValueInnerPlanned.InnerHtml = plannedValue.ToString(formatThousand);
                if (isPlanTab)
                {
                    if (plannedValue > childAllocatedValue)
                    {
                        className += budgetError;
                        divValueInnerPlanned.Attributes.Add("OverBudget", Math.Abs(childAllocatedValue - plannedValue).ToString(formatThousand));
                    }
                    span = new TagBuilder("span");
                    dblProgress = 0;
                    dblProgress = (plannedValue == 0 && childAllocatedValue == 0) ? 0 : (plannedValue > 0 && childAllocatedValue == 0) ? 101 : plannedValue / childAllocatedValue * 100;

                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        span.AddCssClass("progressBar");
                    }
                    divValueInnerPlanned.InnerHtml += span.ToString();

                }
                divValueInnerPlanned.AddCssClass(className);
                tdHeaderInnerPlanned.InnerHtml += divHeaderInnerPlanned.ToString();
                trInnerHeader.InnerHtml += tdHeaderInnerPlanned.ToString();

                tdValueInnerPlanned.InnerHtml += divValueInnerPlanned.ToString();
                trValue.InnerHtml += tdValueInnerPlanned.ToString();

                // For Allocated
                className = "";
                TagBuilder tdHeaderInnerAllocated = new TagBuilder("td");
                TagBuilder divHeaderInnerAllocated = new TagBuilder("div");
                TagBuilder tdValueInnerAllocated = new TagBuilder("td");
                TagBuilder divValueInnerAllocated = new TagBuilder("div");
                divHeaderInnerAllocated.InnerHtml = "Budget"; //*Allocated

                TagBuilder aAllocated = new TagBuilder("a");
                aAllocated.AddCssClass("UpperArrowReport");
                aAllocated.Attributes.Add("id", "Allocated_Up_" + i);
                divHeaderInnerAllocated.InnerHtml += aAllocated;
                aAllocated = new TagBuilder("a");
                aAllocated.AddCssClass("DownArrowReport");
                aAllocated.Attributes.Add("id", "Allocated_Down_" + i);
                divHeaderInnerAllocated.InnerHtml += aAllocated;

                divValueInnerAllocated.Attributes.Add("id", activityType + activityId);
                if (isPlanTab)
                {
                    divValueInnerAllocated.InnerHtml = childAllocatedValue.ToString(formatThousand);
                }
                else
                {
                    divValueInnerAllocated.InnerHtml = "---";
                }
                divValueInnerAllocated.AddCssClass(className);

                tdHeaderInnerAllocated.InnerHtml += divHeaderInnerAllocated.ToString();
                trInnerHeader.InnerHtml += tdHeaderInnerAllocated.ToString();

                tdValueInnerAllocated.InnerHtml += divValueInnerAllocated.ToString();
                trValue.InnerHtml += tdValueInnerAllocated.ToString();

            }

            sb.AppendLine(trHeader.ToString());
            sb.AppendLine(trInnerHeader.ToString());
            sb.AppendLine(trValue.ToString());
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentMonthReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model, string allocatedBy, bool isPlanTab)
        {

            StringBuilder sb = new StringBuilder();
            foreach (BudgetModelReport c in model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {
                    int incrementCount = 1;
                    if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
                    {
                        incrementCount = 3;
                    }
                    TagBuilder tr = new TagBuilder("tr");
                    for (int i = 1; i <= 12; i += incrementCount)
                    {
                        string className = "firstLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("event-rowReport");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", activityType + c.ActivityId);

                        double allocatedValue = 0;
                        double actualValue = 0;
                        double plannedValue = 0;
                        double childAllocatedValue = 0;

                        switch (i)
                        {
                            case 1:
                                actualValue = c.MonthActual.BudgetY1;
                                allocatedValue = c.MonthAllocated.BudgetY1;
                                plannedValue = c.MonthPlanned.BudgetY1;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY1;
                                break;
                            case 2:
                                actualValue = c.MonthActual.BudgetY2;
                                allocatedValue = c.MonthAllocated.BudgetY2;
                                plannedValue = c.MonthPlanned.BudgetY2;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY2;
                                break;
                            case 3:
                                actualValue = c.MonthActual.BudgetY3;
                                allocatedValue = c.MonthAllocated.BudgetY3;
                                plannedValue = c.MonthPlanned.BudgetY3;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY3;
                                break;
                            case 4:
                                actualValue = c.MonthActual.BudgetY4;
                                allocatedValue = c.MonthAllocated.BudgetY4;
                                plannedValue = c.MonthPlanned.BudgetY4;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY4;
                                break;
                            case 5:
                                actualValue = c.MonthActual.BudgetY5;
                                allocatedValue = c.MonthAllocated.BudgetY5;
                                plannedValue = c.MonthPlanned.BudgetY5;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY5;
                                break;
                            case 6:
                                actualValue = c.MonthActual.BudgetY6;
                                allocatedValue = c.MonthAllocated.BudgetY6;
                                plannedValue = c.MonthPlanned.BudgetY6;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY6;
                                break;
                            case 7:
                                actualValue = c.MonthActual.BudgetY7;
                                allocatedValue = c.MonthAllocated.BudgetY7;
                                plannedValue = c.MonthPlanned.BudgetY7;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY7;
                                break;
                            case 8:
                                actualValue = c.MonthActual.BudgetY8;
                                allocatedValue = c.MonthAllocated.BudgetY8;
                                plannedValue = c.MonthPlanned.BudgetY8;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY8;
                                break;
                            case 9:
                                actualValue = c.MonthActual.BudgetY9;
                                allocatedValue = c.MonthAllocated.BudgetY9;
                                plannedValue = c.MonthPlanned.BudgetY9;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY9;
                                break;
                            case 10:
                                actualValue = c.MonthActual.BudgetY10;
                                allocatedValue = c.MonthAllocated.BudgetY10;
                                plannedValue = c.MonthPlanned.BudgetY10;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY10;
                                break;
                            case 11:
                                actualValue = c.MonthActual.BudgetY11;
                                allocatedValue = c.MonthAllocated.BudgetY11;
                                plannedValue = c.MonthPlanned.BudgetY11;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY11;
                                break;
                            case 12:
                                actualValue = c.MonthActual.BudgetY12;
                                allocatedValue = c.MonthAllocated.BudgetY12;
                                plannedValue = c.MonthPlanned.BudgetY12;
                                childAllocatedValue = c.ChildMonthAllocated.BudgetY12;
                                break;
                        }

                        TagBuilder span = new TagBuilder("span");
                        double dblProgress = 0;
                        //Actual
                        div.InnerHtml = actualValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (actualValue > allocatedValue)
                            {
                                className += budgetError;
                                div.Attributes.Add("OverBudget", Math.Abs(allocatedValue - actualValue).ToString(formatThousand));
                            }
                            span = new TagBuilder("span");
                            dblProgress = (actualValue == 0 && allocatedValue == 0) ? 0 : (actualValue > 0 && allocatedValue == 0) ? 101 : actualValue / allocatedValue * 100;

                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                span.AddCssClass("progressBar budgetError");
                            }
                            else
                            {
                                span.AddCssClass("progressBar");
                            }
                            div.InnerHtml += span.ToString();


                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();
                        td.InnerHtml += ChildMonthActualReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model, allocatedBy, i, isPlanTab).ToString();
                        tr.InnerHtml += td.ToString();

                        // Planned
                        className = "firstLevel";
                        TagBuilder tdPlanned = new TagBuilder("td");
                        tdPlanned.AddCssClass("event-rowReport");

                        TagBuilder divPlanned = new TagBuilder("div");
                        divPlanned.Attributes.Add("id", activityType + c.ActivityId);

                        divPlanned.InnerHtml = plannedValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (plannedValue > allocatedValue)
                            {
                                className += budgetError;
                                divPlanned.Attributes.Add("OverBudget", Math.Abs(allocatedValue - plannedValue).ToString(formatThousand));
                            }
                            span = new TagBuilder("span");
                            dblProgress = 0;
                            dblProgress = (plannedValue == 0 && allocatedValue == 0) ? 0 : (plannedValue > 0 && allocatedValue == 0) ? 101 : plannedValue / allocatedValue * 100;

                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                span.AddCssClass("progressBar budgetError");
                            }
                            else
                            {
                                span.AddCssClass("progressBar");
                            }
                            divPlanned.InnerHtml += span.ToString();

                        }
                        divPlanned.AddCssClass(className);
                        tdPlanned.InnerHtml = divPlanned.ToString();
                        tdPlanned.InnerHtml += ChildMonthPlannedReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model, allocatedBy, i, isPlanTab).ToString();
                        tr.InnerHtml += tdPlanned.ToString();

                        // Allocated
                        className = "firstLevel";
                        TagBuilder tdAllocated = new TagBuilder("td");
                        tdAllocated.AddCssClass("event-rowReport");

                        TagBuilder divAllocated = new TagBuilder("div");
                        divAllocated.Attributes.Add("id", activityType + c.ActivityId);
                        if (isPlanTab)
                        {
                            divAllocated.InnerHtml = allocatedValue.ToString(formatThousand);

                            if (allocatedValue < childAllocatedValue)
                            {
                                className += budgetError;
                                divAllocated.Attributes.Add("Allocated", childAllocatedValue.ToString(formatThousand));
                            }
                            else if (allocatedValue > childAllocatedValue)
                            {
                                divAllocated.Attributes.Add("Remaining", (allocatedValue - childAllocatedValue).ToString(formatThousand));
                            }


                        }
                        else
                        {
                            divAllocated.InnerHtml = "---";
                        }
                        divAllocated.AddCssClass(className);
                        tdAllocated.InnerHtml = divAllocated.ToString();

                        tdAllocated.InnerHtml += ChildMonthAllocatedReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model, allocatedBy, i, isPlanTab).ToString();
                        tr.InnerHtml += tdAllocated.ToString();

                    }
                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildMonthActualReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model, string allocatedBy, int month, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
            }
            else if (activityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == "lineitem")
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    string className = "";
                    TagBuilder span = new TagBuilder("span");
                    double dblProgress = 0;
                    double ActualPlannedValue = 0;
                    double AllocatedValue = 0;
                    switch (month)
                    {
                        case 1:
                            ActualPlannedValue = p.MonthActual.BudgetY1;
                            AllocatedValue = p.MonthAllocated.BudgetY1;
                            break;
                        case 2:
                            ActualPlannedValue = p.MonthActual.BudgetY2;
                            AllocatedValue = p.MonthAllocated.BudgetY2;
                            break;
                        case 3:
                            ActualPlannedValue = p.MonthActual.BudgetY3;
                            AllocatedValue = p.MonthAllocated.BudgetY3;
                            break;
                        case 4:
                            ActualPlannedValue = p.MonthActual.BudgetY4;
                            AllocatedValue = p.MonthAllocated.BudgetY4;
                            break;
                        case 5:
                            ActualPlannedValue = p.MonthActual.BudgetY5;
                            AllocatedValue = p.MonthAllocated.BudgetY5;
                            break;
                        case 6:
                            ActualPlannedValue = p.MonthActual.BudgetY6;
                            AllocatedValue = p.MonthAllocated.BudgetY6;
                            break;
                        case 7:
                            ActualPlannedValue = p.MonthActual.BudgetY7;
                            AllocatedValue = p.MonthAllocated.BudgetY7;
                            break;
                        case 8:
                            ActualPlannedValue = p.MonthActual.BudgetY8;
                            AllocatedValue = p.MonthAllocated.BudgetY8;
                            break;
                        case 9:
                            ActualPlannedValue = p.MonthActual.BudgetY9;
                            AllocatedValue = p.MonthAllocated.BudgetY9;
                            break;
                        case 10:
                            ActualPlannedValue = p.MonthActual.BudgetY10;
                            AllocatedValue = p.MonthAllocated.BudgetY10;
                            break;
                        case 11:
                            ActualPlannedValue = p.MonthActual.BudgetY11;
                            AllocatedValue = p.MonthAllocated.BudgetY11;
                            break;
                        case 12:
                            ActualPlannedValue = p.MonthActual.BudgetY12;
                            AllocatedValue = p.MonthAllocated.BudgetY12;
                            break;
                    }


                    if (activityType == Helpers.ActivityType.ActivityLineItem && ActualPlannedValue <= 0)
                    {
                        divProgram.InnerHtml = "---";
                    }
                    else if (activityType != Helpers.ActivityType.ActivityLineItem && activityType != Helpers.ActivityType.ActivityTactic)
                    {
                        divProgram.InnerHtml = ActualPlannedValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (ActualPlannedValue > AllocatedValue)
                            {
                                className += budgetError;
                                divProgram.Attributes.Add("OverBudget", Math.Abs(AllocatedValue - ActualPlannedValue).ToString(formatThousand));
                            }
                            dblProgress = (ActualPlannedValue == 0 && AllocatedValue == 0) ? 0 : (ActualPlannedValue > 0 && AllocatedValue == 0) ? 101 : ActualPlannedValue / AllocatedValue * 100;

                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                span.AddCssClass("progressBar budgetError");
                            }
                            else
                            {
                                span.AddCssClass("progressBar");
                            }
                            divProgram.InnerHtml += span.ToString();
                        }
                    }
                    else
                    {
                        divProgram.InnerHtml = ActualPlannedValue.ToString(formatThousand);
                    }

                    divProgram.AddCssClass(className);
                    divProgram.AddCssClass(innerClass);

                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonthActualReport(helper, ActivityType.ActivityProgram, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();

                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonthActualReport(helper, ActivityType.ActivityTactic, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();

                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonthActualReport(helper, ActivityType.ActivityLineItem, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildMonthPlannedReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model, string allocatedBy, int month, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (activityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
            }
            else if (activityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == "lineitem")
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    string className = "";
                    TagBuilder span = new TagBuilder("span");
                    double dblProgress = 0;
                    double plannedValue = 0;
                    double allocatedValue = 0;
                    switch (month)
                    {
                        case 1:
                            plannedValue = p.MonthPlanned.BudgetY1;
                            allocatedValue = p.MonthAllocated.BudgetY1;
                            break;
                        case 2:
                            plannedValue = p.MonthPlanned.BudgetY2;
                            allocatedValue = p.MonthAllocated.BudgetY2;
                            break;
                        case 3:
                            plannedValue = p.MonthPlanned.BudgetY3;
                            allocatedValue = p.MonthAllocated.BudgetY3;
                            break;
                        case 4:
                            plannedValue = p.MonthPlanned.BudgetY4;
                            allocatedValue = p.MonthAllocated.BudgetY4;
                            break;
                        case 5:
                            plannedValue = p.MonthPlanned.BudgetY5;
                            allocatedValue = p.MonthAllocated.BudgetY5;
                            break;
                        case 6:
                            plannedValue = p.MonthPlanned.BudgetY6;
                            allocatedValue = p.MonthAllocated.BudgetY6;
                            break;
                        case 7:
                            plannedValue = p.MonthPlanned.BudgetY7;
                            allocatedValue = p.MonthAllocated.BudgetY7;
                            break;
                        case 8:
                            plannedValue = p.MonthPlanned.BudgetY8;
                            allocatedValue = p.MonthAllocated.BudgetY8;
                            break;
                        case 9:
                            plannedValue = p.MonthPlanned.BudgetY9;
                            allocatedValue = p.MonthAllocated.BudgetY9;
                            break;
                        case 10:
                            plannedValue = p.MonthPlanned.BudgetY10;
                            allocatedValue = p.MonthAllocated.BudgetY10;
                            break;
                        case 11:
                            plannedValue = p.MonthPlanned.BudgetY11;
                            allocatedValue = p.MonthAllocated.BudgetY11;
                            break;
                        case 12:
                            plannedValue = p.MonthPlanned.BudgetY12;
                            allocatedValue = p.MonthAllocated.BudgetY12;
                            break;
                    }

                    if (activityType == ActivityType.ActivityLineItem && plannedValue <= 0)
                    {
                        divProgram.InnerHtml = "---";
                    }
                    else if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                    {
                        divProgram.InnerHtml = plannedValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (plannedValue > allocatedValue)
                            {
                                className += budgetError;
                                divProgram.Attributes.Add("OverBudget", Math.Abs(allocatedValue - plannedValue).ToString(formatThousand));
                            }
                            dblProgress = (plannedValue == 0 && allocatedValue == 0) ? 0 : (plannedValue > 0 && allocatedValue == 0) ? 101 : plannedValue / allocatedValue * 100;

                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                span.AddCssClass("progressBar budgetError");
                            }
                            else
                            {
                                span.AddCssClass("progressBar");
                            }
                            divProgram.InnerHtml += span.ToString();
                        }
                    }
                    else
                    {
                        divProgram.InnerHtml = plannedValue.ToString(formatThousand);
                    }

                    divProgram.AddCssClass(className);
                    divProgram.AddCssClass(innerClass);

                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonthPlannedReport(helper, ActivityType.ActivityProgram, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();

                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonthPlannedReport(helper, ActivityType.ActivityTactic, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();

                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonthPlannedReport(helper, ActivityType.ActivityLineItem, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildMonthAllocatedReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model, string allocatedBy, int month, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (activityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
            }
            else if (activityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == "lineitem")
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    string className = "";
                    double allocatedValue = 0;
                    double childAllocatedValue = 0;
                    switch (month)
                    {
                        case 1:
                            allocatedValue = p.MonthAllocated.BudgetY1;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY1;
                            break;
                        case 2:
                            allocatedValue = p.MonthAllocated.BudgetY2;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY2;
                            break;
                        case 3:
                            allocatedValue = p.MonthAllocated.BudgetY3;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY3;
                            break;
                        case 4:
                            allocatedValue = p.MonthAllocated.BudgetY4;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY4;
                            break;
                        case 5:
                            allocatedValue = p.MonthAllocated.BudgetY5;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY5;
                            break;
                        case 6:
                            allocatedValue = p.MonthAllocated.BudgetY6;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY6;
                            break;
                        case 7:
                            allocatedValue = p.MonthAllocated.BudgetY7;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY7;
                            break;
                        case 8:
                            allocatedValue = p.MonthAllocated.BudgetY8;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY8;
                            break;
                        case 9:
                            allocatedValue = p.MonthAllocated.BudgetY9;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY9;
                            break;
                        case 10:
                            allocatedValue = p.MonthAllocated.BudgetY10;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY10;
                            break;
                        case 11:
                            allocatedValue = p.MonthAllocated.BudgetY11;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY11;
                            break;
                        case 12:
                            allocatedValue = p.MonthAllocated.BudgetY12;
                            childAllocatedValue = p.ChildMonthAllocated.BudgetY12;
                            break;
                    }


                    if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic && isPlanTab)
                    {
                        if (activityType != ActivityType.ActivityProgram)
                        {
                            if (allocatedValue < childAllocatedValue)
                            {
                                className += budgetError;
                                divProgram.Attributes.Add("Allocated", childAllocatedValue.ToString(formatThousand));
                            }
                            else if (allocatedValue > childAllocatedValue)
                            {
                                divProgram.Attributes.Add("Remaining", (allocatedValue - childAllocatedValue).ToString(formatThousand));
                            }
                        }

                        divProgram.InnerHtml = allocatedValue.ToString(formatThousand);
                    }
                    else
                    {
                        divProgram.InnerHtml = "---";
                    }

                    divProgram.AddCssClass(className);
                    divProgram.AddCssClass(innerClass);
                    div.InnerHtml += divProgram.ToString();

                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonthAllocatedReport(helper, ActivityType.ActivityProgram, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();

                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonthAllocatedReport(helper, ActivityType.ActivityTactic, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();

                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonthAllocatedReport(helper, ActivityType.ActivityLineItem, p.ActivityId, model, allocatedBy, month, isPlanTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentSummaryReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model, string allocatedBy, bool isPlanTab)
        {
            StringBuilder sb = new StringBuilder();
            BudgetModelReport plan = model.SingleOrDefault(pl => pl.ActivityType == ActivityType.ActivityMain);
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");

                //First Actual
                TagBuilder td = new TagBuilder("td");
                TagBuilder div = new TagBuilder("div");

                double sumMonthActual = plan.MonthActual.BudgetY1 + plan.MonthActual.BudgetY2 + plan.MonthActual.BudgetY3 + plan.MonthActual.BudgetY4 + plan.MonthActual.BudgetY5 + plan.MonthActual.BudgetY6 + plan.MonthActual.BudgetY7 + plan.MonthActual.BudgetY8 + plan.MonthActual.BudgetY9 + plan.MonthActual.BudgetY10 + plan.MonthActual.BudgetY11 + plan.MonthActual.BudgetY12;
                double sumMonthPlanned = plan.MonthPlanned.BudgetY1 + plan.MonthPlanned.BudgetY2 + plan.MonthPlanned.BudgetY3 + plan.MonthPlanned.BudgetY4 + plan.MonthPlanned.BudgetY5 + plan.MonthPlanned.BudgetY6 + plan.MonthPlanned.BudgetY7 + plan.MonthPlanned.BudgetY8 + plan.MonthPlanned.BudgetY9 + plan.MonthPlanned.BudgetY10 + plan.MonthPlanned.BudgetY11 + plan.MonthPlanned.BudgetY12;
                double sumMonthAllocated = plan.ChildMonthAllocated.BudgetY1 + plan.ChildMonthAllocated.BudgetY2 + plan.ChildMonthAllocated.BudgetY3 + plan.ChildMonthAllocated.BudgetY4 + plan.ChildMonthAllocated.BudgetY5 + plan.ChildMonthAllocated.BudgetY6 + plan.ChildMonthAllocated.BudgetY7 + plan.ChildMonthAllocated.BudgetY8 + plan.ChildMonthAllocated.BudgetY9 + plan.ChildMonthAllocated.BudgetY10 + plan.ChildMonthAllocated.BudgetY11 + plan.ChildMonthAllocated.BudgetY12;
                TagBuilder span = new TagBuilder("span");

                double dblProgress = 0;
                div.InnerHtml = sumMonthActual.ToString(formatThousand);
                if (isPlanTab)
                {
                    span = new TagBuilder("span");

                    dblProgress = (sumMonthActual == 0 && sumMonthAllocated == 0) ? 0 : (sumMonthActual > 0 && sumMonthAllocated == 0) ? 101 : sumMonthActual / sumMonthAllocated * 100;
                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        div.AddCssClass("budgetError");
                        div.Attributes.Add("OverBudget", Math.Abs(sumMonthAllocated - sumMonthActual).ToString(formatThousand));
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        span.AddCssClass("progressBar");
                    }
                    div.InnerHtml += span.ToString();
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                // Second Planned
                td = new TagBuilder("td");
                div = new TagBuilder("div");

                div.InnerHtml = sumMonthPlanned.ToString(formatThousand);
                if (isPlanTab)
                {
                    span = new TagBuilder("span");

                    dblProgress = 0;
                    dblProgress = (sumMonthPlanned == 0 && sumMonthAllocated == 0) ? 0 : (sumMonthPlanned > 0 && sumMonthAllocated == 0) ? 101 : sumMonthPlanned / sumMonthAllocated * 100;
                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        div.AddCssClass("budgetError");
                        div.Attributes.Add("OverBudget", Math.Abs(sumMonthAllocated - sumMonthPlanned).ToString(formatThousand));
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        span.AddCssClass("progressBar");
                    }
                    div.InnerHtml += span.ToString();
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                //Third Allocated
                td = new TagBuilder("td");
                div = new TagBuilder("div");
                if (isPlanTab)
                {
                    div.InnerHtml = sumMonthAllocated.ToString(formatThousand);
                }
                else
                {
                    div.InnerHtml = "---";
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModelReport c in model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList())
            {
                if (model.Any(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId))
                {
                    TagBuilder tr = new TagBuilder("tr");
                    //First Actual
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("event-rowReport");

                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", activityType + c.ActivityId);
                    div.AddCssClass("firstLevel");
                    double sumMonthActual = c.MonthActual.BudgetY1 + c.MonthActual.BudgetY2 + c.MonthActual.BudgetY3 + c.MonthActual.BudgetY4 + c.MonthActual.BudgetY5 + c.MonthActual.BudgetY6 + c.MonthActual.BudgetY7 + c.MonthActual.BudgetY8 + c.MonthActual.BudgetY9 + c.MonthActual.BudgetY10 + c.MonthActual.BudgetY11 + c.MonthActual.BudgetY12;
                    double sumMonthPlanned = c.MonthPlanned.BudgetY1 + c.MonthPlanned.BudgetY2 + c.MonthPlanned.BudgetY3 + c.MonthPlanned.BudgetY4 + c.MonthPlanned.BudgetY5 + c.MonthPlanned.BudgetY6 + c.MonthPlanned.BudgetY7 + c.MonthPlanned.BudgetY8 + c.MonthPlanned.BudgetY9 + c.MonthPlanned.BudgetY10 + c.MonthPlanned.BudgetY11 + c.MonthPlanned.BudgetY12;
                    double sumMonthAllocated = c.MonthAllocated.BudgetY1 + c.MonthAllocated.BudgetY2 + c.MonthAllocated.BudgetY3 + c.MonthAllocated.BudgetY4 + c.MonthAllocated.BudgetY5 + c.MonthAllocated.BudgetY6 + c.MonthAllocated.BudgetY7 + c.MonthAllocated.BudgetY8 + c.MonthAllocated.BudgetY9 + c.MonthAllocated.BudgetY10 + c.MonthAllocated.BudgetY11 + c.MonthAllocated.BudgetY12;
                    double sumMonthChildAllocated = c.ChildMonthAllocated.BudgetY1 + c.ChildMonthAllocated.BudgetY2 + c.ChildMonthAllocated.BudgetY3 + c.ChildMonthAllocated.BudgetY4 + c.ChildMonthAllocated.BudgetY5 + c.ChildMonthAllocated.BudgetY6 + c.ChildMonthAllocated.BudgetY7 + c.ChildMonthAllocated.BudgetY8 + c.ChildMonthAllocated.BudgetY9 + c.ChildMonthAllocated.BudgetY10 + c.ChildMonthAllocated.BudgetY11 + c.ChildMonthAllocated.BudgetY12;

                    div.InnerHtml = sumMonthActual.ToString(formatThousand);
                    TagBuilder span = new TagBuilder("span");

                    double dblProgress = 0;
                    if (isPlanTab)
                    {
                        dblProgress = (sumMonthActual == 0 && sumMonthAllocated == 0) ? 0 : (sumMonthActual > 0 && sumMonthAllocated == 0) ? 101 : sumMonthActual / sumMonthAllocated * 100;
                        span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                        if (dblProgress > 100)
                        {
                            div.AddCssClass("budgetError");
                            div.Attributes.Add("OverBudget", Math.Abs(sumMonthAllocated - sumMonthActual).ToString(formatThousand));
                            span.AddCssClass("progressBar budgetError");
                        }
                        else
                        {
                            span.AddCssClass("progressBar");
                        }
                        div.InnerHtml += span.ToString();
                    }
                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ChildSummaryReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model, "first", allocatedBy, isPlanTab).ToString();

                    tr.InnerHtml += td.ToString();

                    // Second Planned
                    td = new TagBuilder("td");
                    td.AddCssClass("event-rowReport");

                    div = new TagBuilder("div");
                    div.Attributes.Add("id", activityType + c.ActivityId);
                    div.AddCssClass("firstLevel");

                    div.InnerHtml = sumMonthPlanned.ToString(formatThousand);
                    if (isPlanTab)
                    {
                        span = new TagBuilder("span");

                        dblProgress = 0;
                        dblProgress = (sumMonthPlanned == 0 && sumMonthAllocated == 0) ? 0 : (sumMonthPlanned > 0 && sumMonthAllocated == 0) ? 101 : sumMonthPlanned / sumMonthAllocated * 100;
                        span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                        if (dblProgress > 100)
                        {
                            div.AddCssClass("budgetError");
                            div.Attributes.Add("OverBudget", Math.Abs(sumMonthAllocated - sumMonthPlanned).ToString(formatThousand));
                            span.AddCssClass("progressBar budgetError");
                        }
                        else
                        {
                            span.AddCssClass("progressBar");
                        }
                        div.InnerHtml += span.ToString();
                    }
                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ChildSummaryReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model, "Second", allocatedBy, isPlanTab).ToString();

                    tr.InnerHtml += td.ToString();

                    //Third Allocated

                    td = new TagBuilder("td");
                    td.AddCssClass("event-rowReport");

                    div = new TagBuilder("div");
                    div.Attributes.Add("id", activityType + c.ActivityId);
                    div.AddCssClass("firstLevel");
                    if (isPlanTab)
                    {
                        if (sumMonthAllocated < sumMonthChildAllocated)
                        {
                            div.AddCssClass(budgetError);
                            div.Attributes.Add("Allocated", sumMonthChildAllocated.ToString(formatThousand));
                        }
                        else if (sumMonthAllocated > sumMonthChildAllocated)
                        {
                            div.Attributes.Add("Remaining", (sumMonthAllocated - sumMonthChildAllocated).ToString(formatThousand));
                        }
                        div.InnerHtml = sumMonthAllocated.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }
                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ChildSummaryReport(helper, ActivityType.ActivityCampaign, c.ActivityId, model, "Third", allocatedBy, isPlanTab).ToString();

                    tr.InnerHtml += td.ToString();
                    sb.AppendLine(tr.ToString());
                }
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="activityType"></param>
        /// <param name="parentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="allocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildSummaryReport(this HtmlHelper helper, string activityType, string parentActivityId, List<BudgetModelReport> model, string mode, string allocatedBy, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (activityType == ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
            }
            else if (activityType == ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (activityType == ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (activityType == ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == activityType && p.ParentActivityId == parentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + parentActivityId);
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", activityType + p.ActivityId);
                    double sumMonthAllocated = p.MonthAllocated.BudgetY1 + p.MonthAllocated.BudgetY2 + p.MonthAllocated.BudgetY3 + p.MonthAllocated.BudgetY4 + p.MonthAllocated.BudgetY5 + p.MonthAllocated.BudgetY6 + p.MonthAllocated.BudgetY7 + p.MonthAllocated.BudgetY8 + p.MonthAllocated.BudgetY9 + p.MonthAllocated.BudgetY10 + p.MonthAllocated.BudgetY11 + p.MonthAllocated.BudgetY12;
                    if (mode == "first")
                    {
                        double sumMonthActual = p.MonthActual.BudgetY1 + p.MonthActual.BudgetY2 + p.MonthActual.BudgetY3 + p.MonthActual.BudgetY4 + p.MonthActual.BudgetY5 + p.MonthActual.BudgetY6 + p.MonthActual.BudgetY7 + p.MonthActual.BudgetY8 + p.MonthActual.BudgetY9 + p.MonthActual.BudgetY10 + p.MonthActual.BudgetY11 + p.MonthActual.BudgetY12;
                        divProgram.InnerHtml = sumMonthActual.ToString(formatThousand);
                        divProgram.AddCssClass(innerClass);
                        if (isPlanTab)
                        {
                            TagBuilder span = new TagBuilder("span");
                            double dblProgress = 0;
                            dblProgress = (sumMonthActual == 0 && sumMonthAllocated == 0) ? 0 : (sumMonthActual > 0 && sumMonthAllocated == 0) ? 101 : sumMonthActual / sumMonthAllocated * 100;
                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.AddCssClass(budgetError);
                                    divProgram.Attributes.Add("OverBudget", Math.Abs(sumMonthAllocated - sumMonthActual).ToString(formatThousand));
                                    span.AddCssClass("progressBar budgetError");
                                }
                                else
                                {
                                    span.AddCssClass("progressBar");
                                }
                            }
                            else
                            {
                                span.AddCssClass("progressBar");
                            }

                            if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                            {
                                divProgram.InnerHtml += span.ToString();
                            }
                        }

                    }
                    else if (mode == "Second")
                    {
                        double sumMonthPlanned = p.MonthPlanned.BudgetY1 + p.MonthPlanned.BudgetY2 + p.MonthPlanned.BudgetY3 + p.MonthPlanned.BudgetY4 + p.MonthPlanned.BudgetY5 + p.MonthPlanned.BudgetY6 + p.MonthPlanned.BudgetY7 + p.MonthPlanned.BudgetY8 + p.MonthPlanned.BudgetY9 + p.MonthPlanned.BudgetY10 + p.MonthPlanned.BudgetY11 + p.MonthPlanned.BudgetY12;
                        divProgram.InnerHtml = sumMonthPlanned.ToString(formatThousand);
                        divProgram.AddCssClass(innerClass);
                        if (isPlanTab)
                        {
                            TagBuilder span = new TagBuilder("span");
                            double dblProgress = 0;
                            dblProgress = (sumMonthPlanned == 0 && sumMonthAllocated == 0) ? 0 : (sumMonthPlanned > 0 && sumMonthAllocated == 0) ? 101 : sumMonthPlanned / sumMonthAllocated * 100;
                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                                {
                                    divProgram.AddCssClass(budgetError);
                                    divProgram.Attributes.Add("OverBudget", Math.Abs(sumMonthAllocated - sumMonthPlanned).ToString(formatThousand));
                                    span.AddCssClass("progressBar budgetError");
                                }
                                else
                                {
                                    span.AddCssClass("progressBar");
                                }
                            }
                            else
                            {
                                span.AddCssClass("progressBar");
                            }


                            if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic)
                            {
                                divProgram.InnerHtml += span.ToString();
                            }
                        }
                    }
                    else
                    {
                        if (activityType != ActivityType.ActivityLineItem && activityType != ActivityType.ActivityTactic && isPlanTab)
                        {
                            if (activityType != ActivityType.ActivityProgram)
                            {
                                double sumMonthChildAllocated = p.ChildMonthAllocated.BudgetY1 + p.ChildMonthAllocated.BudgetY2 + p.ChildMonthAllocated.BudgetY3 + p.ChildMonthAllocated.BudgetY4 + p.ChildMonthAllocated.BudgetY5 + p.ChildMonthAllocated.BudgetY6 + p.ChildMonthAllocated.BudgetY7 + p.ChildMonthAllocated.BudgetY8 + p.ChildMonthAllocated.BudgetY9 + p.ChildMonthAllocated.BudgetY10 + p.ChildMonthAllocated.BudgetY11 + p.ChildMonthAllocated.BudgetY12;

                                if (sumMonthAllocated < sumMonthChildAllocated)
                                {
                                    divProgram.AddCssClass(budgetError);
                                    divProgram.Attributes.Add("Allocated", sumMonthChildAllocated.ToString(formatThousand));
                                }
                                else if (sumMonthAllocated > sumMonthChildAllocated)
                                {
                                    divProgram.Attributes.Add("Remaining", (sumMonthAllocated - sumMonthChildAllocated).ToString(formatThousand));
                                }
                            }
                            divProgram.AddCssClass(innerClass);
                            divProgram.InnerHtml = sumMonthAllocated.ToString(formatThousand);
                        }
                        else
                        {
                            divProgram.AddCssClass(innerClass);
                            divProgram.InnerHtml += "---";
                        }

                    }
                    div.InnerHtml += divProgram.ToString();
                    if (activityType == ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildSummaryReport(helper, ActivityType.ActivityProgram, p.ActivityId, model, mode, allocatedBy, isPlanTab).ToString();
                    else if (activityType == ActivityType.ActivityProgram)
                        div.InnerHtml += ChildSummaryReport(helper, ActivityType.ActivityTactic, p.ActivityId, model, mode, allocatedBy, isPlanTab).ToString();
                    else if (activityType == ActivityType.ActivityTactic)
                        div.InnerHtml += ChildSummaryReport(helper, ActivityType.ActivityLineItem, p.ActivityId, model, mode, allocatedBy, isPlanTab).ToString();

                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            return new MvcHtmlString(string.Empty);
        }

        #endregion //Budgeting Report

        #region Field Mapping for LineItem Popup
        //Added By Komal Rawal for #1617.
        public static MvcHtmlString GenerateMappingFieldsForInspectPopup(int Id, string mode = "ReadOnly")
        {

            MRPEntities db = new MRPEntities();
            List<int> BudgetDetailsIds = db.Budgets.Where(a => a.ClientId == Sessions.User.CID && a.IsDeleted == false).Select(a => a.Id).ToList();
            List<Budget_Detail> BudgetDetails = db.Budget_Detail.Where(a => BudgetDetailsIds.Contains(a.BudgetId) && a.IsDeleted == false).Select(a => a).ToList();
            List<int> SelectedOptionIDs = db.LineItem_Budget.Where(list => list.PlanLineItemId == Id).Select(list => list.BudgetDetailId).ToList();
            List<string> SelectedOptionValues = BudgetDetails.Where(Detaillist => SelectedOptionIDs.Contains(Detaillist.Id)).Select(Detaillist => Detaillist.Name).ToList();

            StringBuilder sb = new StringBuilder(string.Empty);
            string DropDownStyle, divPosition, require, name, customFieldEntityValue;

            string className = "span3 margin-top10";
            sb.Append("<div class=\"" + className + "\" ><p title=\"" + "Budget Account" + "\" class=\"ellipsis\">" + "Budget Account" + "</p>");


            DropDownStyle = "style=\"border-top: 1px solid #d4d4d4;border-bottom: 1px solid #d4d4d4;display:none;";
            divPosition = "style=\"position:relative;\"";
            require = "";
            name = "";

            if (mode == Enums.InspectPopupMode.Edit.ToString())
            {
                sb.Append("<div tabindex=\"0\"" + divPosition + "><a id=\"dropdown_new_btn_FieldMapping\" class=\"dropdown_new_btn" + "" + "\"" + require + "  label=\"" + "" + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                if (SelectedOptionIDs.Count() == 0 || SelectedOptionIDs == null || SelectedOptionValues.Count() == 0)
                {
                    name += "Please Select" + ", ";
                }
                else
                {

                    name += string.Join(",", SelectedOptionValues) + ", ";

                }
                sb.Append("<div id=\"treeviewddl\"" + DropDownStyle + "\"></div>");
                if (name.Length > 0)
                {
                    name = name.Remove(name.Length - 2, 2);
                }
                else
                {
                    name = "Please Select";
                }
                sb.Replace("#HEADER_OF_DROPDOWN#", name);
                sb.Append("</div></div>");

            }
            else if (mode == Enums.InspectPopupMode.ReadOnly.ToString())
            {

                sb.Append("<input type=\"text\" readonly = \"true\" value=\"#CUSTOMFEILD_VALUE#\" title=\"#CUSTOMFEILD_VALUE#\" style=\"background:#F2F2F2;\" class=\"span12 input-small\"/>");


                customFieldEntityValue = "";

                customFieldEntityValue += string.Join(",", SelectedOptionValues);

                sb = sb.Replace("#CUSTOMFEILD_VALUE#", customFieldEntityValue.Replace("\"", "&quot;"));
                sb.Append("</div>");

            }

            return new MvcHtmlString(sb.ToString());
        }
        #endregion

        public static string GetLastLogDate(this HtmlHelper helper)
        {
            string ZoneName = string.Empty;

            //DateTime eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Eastern Standard Time");
            CustomDashboard obj = new CustomDashboard();
            string retValue = obj.GetLatestLog();
            if (!string.IsNullOrEmpty(retValue))
            {
                string GMTOffSet = "(UTC" + retValue.Split('#')[2] + ")";
                ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
                foreach (TimeZoneInfo timeZoneInfo in timeZones)
                {
                    if (timeZoneInfo.DisplayName.Contains(GMTOffSet))
                    {
                        ZoneName = timeZoneInfo.StandardName;
                        break;
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<p class=\"timecol grey\">as of " + retValue.Split('#')[0] + "</p>");
                sb.AppendLine("<p class=\"timecol grey\">" + retValue.Split('#')[1] + " " + GetAbbrivatedZoneName(ZoneName) + "</p>");
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }

        public static string GetFilterData(this HtmlHelper helper, int DashboardID, int DashboardPageID)
        {
            WebClient client = new WebClient();
            string regularConnectionString = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Select(o => o.ConnectionString).FirstOrDefault();
            string ReportDBConnString = string.Empty;
            if (!string.IsNullOrEmpty(Convert.ToString(regularConnectionString)))
            {
                ReportDBConnString = Convert.ToString(regularConnectionString);
            }
            string AuthorizedReportAPIUserName = string.Empty;
            string AuthorizedReportAPIPassword = string.Empty;
            string ApiUrl = string.Empty;
            if (ConfigurationManager.AppSettings.Count > 0)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["AuthorizedReportAPIUserName"])))
                {
                    AuthorizedReportAPIUserName = System.Configuration.ConfigurationManager.AppSettings.Get("AuthorizedReportAPIUserName");
                }
                if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["AuthorizedReportAPIPassword"])))
                {
                    AuthorizedReportAPIPassword = System.Configuration.ConfigurationManager.AppSettings.Get("AuthorizedReportAPIPassword");
                }
                if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["IntegrationApi"])))
                {
                    ApiUrl = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
                    if (!string.IsNullOrEmpty(ApiUrl) && !ApiUrl.EndsWith("/"))
                    {
                        ApiUrl += "/";
                    }
                }
            }
            string result = string.Empty;
            string url = ApiUrl + "api/Dashboard/GetFilterdashboardWise?DashboardId=" + DashboardID + "&UserId=" + Sessions.User.ID + "&RoleId=" + Sessions.User.RoleId + "&StartDate=" + Sessions.StartDate + "&EndDate=" + Sessions.EndDate + "&ConnectionString=" + ReportDBConnString + "&UserName=" + AuthorizedReportAPIUserName + "&Password=" + AuthorizedReportAPIPassword;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                result = client.DownloadString(url);
                result = result.Substring(1);
                result = result.Remove(result.Length - 1);

                if (string.IsNullOrEmpty(Sessions.StartDate))
                {
                    Sessions.StartDate = DateTime.Now.AddMonths(6).ToString("MM/dd/yyyy");
                    Sessions.EndDate = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy");
                    if (Convert.ToDateTime(Sessions.StartDate) > Convert.ToDateTime(Sessions.EndDate))
                    {
                        Sessions.StartDate = Convert.ToDateTime(Sessions.EndDate).AddMonths(-6).ToString("MM/dd/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return result;
        }

        public static string GetMenuString(this HtmlHelper helper, int MenuNo, int TotalMenuCnt, RevenuePlanner.BDSService.Menu o)
        {

            string MenuStr = string.Empty;
            if (MenuNo == 11)
            {
                MenuStr = MenuStr + "<li class='Other fix-width dropdown'><a href='#'><span class='fa fa-gear'></span><span class='nav-text'>OTHER</span><span class='dd-arrow'><i class='fa fa-caret-down'></i></span></a><ul class='dropdown-menu'>";
            }
            string hrefLink = string.Empty;
            if (!Sessions.AppMenus.Where(x => x.ParentApplicationId == o.MenuApplicationId).Any())
            {
                if (o.Description != null)
                {
                    hrefLink = o.Description;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(o.ActionName)) && Convert.ToString(o.ActionName).ToLower() == "index")
                    {
                        hrefLink = "/" + Convert.ToString(o.ControllerName);
                    }
                    else
                    {
                        hrefLink = "/" + Convert.ToString(o.ControllerName) + "/" + Convert.ToString(o.ActionName);
                    }
                }
            }
            string classname = Convert.ToString(o.Code.ToLower().Replace(" ", ""));
            classname = "'" + classname + " fix-width";
            if (Sessions.AppMenus.Where(x => x.ParentApplicationId == o.MenuApplicationId).Count() > 0)
            {
                classname = classname + " dropdown";
            }
            classname = classname + "'";
            string DashFrom = "Plan";
            string LiId = "Measure_" + o.MenuApplicationId;
            string CusCss = "fa fa-pie-chart";
            if (!string.IsNullOrEmpty(o.CustomCss))
            {
                CusCss = o.CustomCss;
                DashFrom = "Measure";
            }
            MenuStr = MenuStr + "<li class=" + classname + " id=" + LiId + " DashFrom=" + DashFrom + "> <a href=" + hrefLink + "> <span class='" + CusCss + "'></span> <span class='nav-text'> " + Convert.ToString(o.Name).ToUpper() + " </span>";
            if (Sessions.AppMenus.Where(x => x.ParentApplicationId == o.MenuApplicationId).Count() > 0)
            {
                MenuStr = MenuStr + "<span class='dd-arrow'><i class='fa fa-caret-down'></i></span>";
            }
            MenuStr = MenuStr + "</a></li>";

            if (MenuNo == TotalMenuCnt)
            {
                MenuStr = MenuStr + "</ul></li>";
            }

            return MenuStr;
        }

        private static string GetAbbrivatedZoneName(string StandandName)
        {
            if (StandandName.ToLower() == "dateline standard time") return "DST";
            else if (StandandName.ToLower() == "utc-11" || StandandName.ToLower() == "utc-02") return "UTC";
            else if (StandandName.ToLower() == "hawaiian standard time") return "HST";
            else if (StandandName.ToLower() == "alaskan standard time") return "AST";
            else if (StandandName.ToLower() == "pacific standard time (mexico)" || StandandName.ToLower() == "Pacific Standard time") return "PST";
            else if (StandandName.ToLower() == "us mountain standard time") return "MST";
            else if (StandandName.ToLower() == "mountain standard time (sexico)") return "MST";
            else if (StandandName.ToLower() == "mountain standard time") return "MST";
            else if (StandandName.ToLower() == "central america standard time") return "CST";
            else if (StandandName.ToLower() == "central standard time") return "CST";
            else if (StandandName.ToLower() == "central standard time (mexico)") return "CST";
            else if (StandandName.ToLower() == "canada Central Standard time") return "CST";
            else if (StandandName.ToLower() == "sa Pacific Standard time") return "PST";
            else if (StandandName.ToLower() == "eastern Standard time") return "EST";
            else if (StandandName.ToLower() == "us eastern standard time") return "EST";
            else if (StandandName.ToLower() == "venezuela standard time") return "VST";
            else if (StandandName.ToLower() == "paraguay standard time") return "PST";
            else if (StandandName.ToLower() == "atlantic standard time") return "AST";
            else if (StandandName.ToLower() == "central brazilian standard time") return "CST";
            else if (StandandName.ToLower() == "sa western standard time") return "WST";
            else if (StandandName.ToLower() == "pacific sa standard time") return "PST";
            else if (StandandName.ToLower() == "newfoundland standard time") return "NST";
            else if (StandandName.ToLower() == "e. south america standard time") return "AST";
            else if (StandandName.ToLower() == "argentina standard time") return "AST";
            else if (StandandName.ToLower() == "sa eastern standard time") return "EST";
            else if (StandandName.ToLower() == "greenland standard time") return "GST";
            else if (StandandName.ToLower() == "montevideo standard time") return "MST";
            else if (StandandName.ToLower() == "bahia standard time") return "BST";
            else if (StandandName.ToLower() == "mid-atlantic standard time") return "AST";
            else if (StandandName.ToLower() == "azores standard time") return "AST";
            else if (StandandName.ToLower() == "cape verde standard time") return "";
            else if (StandandName.ToLower() == "morocco standard time") return "";
            else if (StandandName.ToLower() == "coordinated universal time") return "";
            else if (StandandName.ToLower() == "gmt standard time") return "GMT";
            else if (StandandName.ToLower() == "greenwich standard time") return "GMT";
            else if (StandandName.ToLower() == "w. europe standard time") return "EST";
            else if (StandandName.ToLower() == "central europe standard time") return "EST";
            else if (StandandName.ToLower() == "romance standard time") return "RST";
            else if (StandandName.ToLower() == "central european standard time") return "CST";
            else if (StandandName.ToLower() == "w. central africa standard time") return "CST";
            else if (StandandName.ToLower() == "namibia standard time") return "NST";
            else if (StandandName.ToLower() == "jordan standard time") return "JST";
            else if (StandandName.ToLower() == "gtb standard time") return "GTB";
            else if (StandandName.ToLower() == "middle East standard time") return "EST";
            else if (StandandName.ToLower() == "egypt standard time") return "EST";
            else if (StandandName.ToLower() == "syria standard time") return "SST";
            else if (StandandName.ToLower() == "e. europe standard time") return "EST";
            else if (StandandName.ToLower() == "South Africa standard time") return "";
            else if (StandandName.ToLower() == "FLE standard time") return "FST";
            else if (StandandName.ToLower() == "turkey standard time") return "TST";
            else if (StandandName.ToLower() == "jerusalem standard time") return "JST";
            else if (StandandName.ToLower() == "libya standard time") return "LST";
            else if (StandandName.ToLower() == "arabic standard time") return "AST";
            else if (StandandName.ToLower() == "kaliningrad standard time") return "KST";
            else if (StandandName.ToLower() == "arab standard time") return "AST";
            else if (StandandName.ToLower() == "e. africa standard time") return "EST";
            else if (StandandName.ToLower() == "iran standard time") return "IST";
            else if (StandandName.ToLower() == "arabian standard time") return "AST";
            else if (StandandName.ToLower() == "caucasus standard time") return "CST";
            else if (StandandName.ToLower() == "georgian standard time") return "GST";
            else if (StandandName.ToLower() == "mauritius standard time") return "MST";
            else if (StandandName.ToLower() == "russian standard time") return "RST";
            else if (StandandName.ToLower() == "azerbaijan standard time") return "AST";
            else if (StandandName.ToLower() == "nepal standard time") return "NST";
            else if (StandandName.ToLower() == "sri lanka standard time") return "IST";
            else if (StandandName.ToLower() == "india standard time") return "IST";
            else if (StandandName.ToLower() == "pakistan standard time") return "PST";
            else if (StandandName.ToLower() == "central asia standard time") return "AST";
            else if (StandandName.ToLower() == "ekaterinburg standard time") return "EST";
            else if (StandandName.ToLower() == "myanmar standard time") return "MST";
            else if (StandandName.ToLower() == "se asia standard time") return "AST";
            else if (StandandName.ToLower() == "n. central Asia standard time") return "AST";
            else if (StandandName.ToLower() == "china standard time") return "CST";
            else if (StandandName.ToLower() == "bangladesh standard time") return "BST";
            else if (StandandName.ToLower() == "afghanistan standard time") return "AST";
            else if (StandandName.ToLower() == "west Asia standard time") return "WST";
            else if (StandandName.ToLower() == "north asia standard time") return "AST";
            else if (StandandName.ToLower() == "malay peninsula standard time") return "MST";
            else if (StandandName.ToLower() == "w. australia standard time") return "AST";
            else if (StandandName.ToLower() == "taipei standard time") return "TST";
            else if (StandandName.ToLower() == "ulaanbaatar standard time") return "UST";
            else if (StandandName.ToLower() == "north asia east standard time") return "EST";
            else if (StandandName.ToLower() == "tokyo standard time") return "TST";
            else if (StandandName.ToLower() == "korea standard time") return "KST";
            else if (StandandName.ToLower() == "cen. australia standard time") return "AST";
            else if (StandandName.ToLower() == "aus central standard time") return "CST";
            else if (StandandName.ToLower() == "e. australia standard time") return "AST";
            else if (StandandName.ToLower() == "aus eastern standard time") return "EST";
            else if (StandandName.ToLower() == "west Pacific standard time") return "PST";
            else if (StandandName.ToLower() == "tasmania standard time") return "TST";
            else if (StandandName.ToLower() == "yakutsk standard time") return "YST";
            else if (StandandName.ToLower() == "central Pacific standard time") return "PST";
            else if (StandandName.ToLower() == "vladivostok standard time") return "VST";
            else if (StandandName.ToLower() == "new zealand standard time") return "NST";
            else if (StandandName.ToLower() == "utc+12") return "UTC";
            else if (StandandName.ToLower() == "fiji standard time") return "FST";
            else if (StandandName.ToLower() == "magadan standard time") return "MST";
            else if (StandandName.ToLower() == "kamchatka standard time") return "KST";
            else if (StandandName.ToLower() == "tonga standard time") return "TST";
            else if (StandandName.ToLower() == "samoa standard time") return "SST";
            else if (StandandName.ToLower() == "line islands standard time") return "LST";
            else return "";
        }
    }


}