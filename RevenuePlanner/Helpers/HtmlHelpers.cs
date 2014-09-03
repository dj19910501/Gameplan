using RevenuePlanner.Models;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using System;
namespace RevenuePlanner.Helpers
{
    public static class HtmlHelpers
    {
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
        /// <param name="lst"></param>
        /// <param name="MLeads"></param>
        /// <param name="MSize"></param>
        /// <param name="TLeads"></param>
        /// <param name="TSize"></param>
        /// <param name="SLeads"></param>
        /// <param name="SSize"></param>
        /// <param name="bln"></param>
        /// <returns>MvcHtmlString</returns>
        /// datatype of MSize,TSize and SSize is modified from int to double
        public static MvcHtmlString GenerateFunnel(this HtmlHelper helper, Dictionary<int, string> lst, int MLeads, double MSize, int TLeads, double TSize, int SLeads, double SSize, bool bln)
        {

            StringBuilder sb = new StringBuilder();
            StringBuilder sbHidden = new StringBuilder();

            TagBuilder textboxMarketingLeads = new TagBuilder("input");
            textboxMarketingLeads.Attributes.Add("type", "text");
            textboxMarketingLeads.Attributes.Add("id", "MarketingLeads");
            textboxMarketingLeads.Attributes.Add("name", "txtMarketing");
            textboxMarketingLeads.Attributes.Add("datadefault", "ML");
            textboxMarketingLeads.Attributes.Add("datasubline", "For example: <em>200</em>");

            if (MLeads > 0)
            {
                textboxMarketingLeads.Attributes.Add("value", MLeads.ToString());
                textboxMarketingLeads.Attributes.Add("placeholder", MLeads.ToString());
            }
            else
            {
                textboxMarketingLeads.Attributes.Add("value", "");
                textboxMarketingLeads.Attributes.Add("placeholder", "0");
            }
            textboxMarketingLeads.Attributes.Add("formatType", "dollarValue");
            textboxMarketingLeads.Attributes.Add("maxlength", Common.maxLengthPriceValue);


            TagBuilder textboxMarketingDealSize = new TagBuilder("input");
            textboxMarketingDealSize.Attributes.Add("type", "text");
            textboxMarketingDealSize.Attributes.Add("id", "MarketingDealSize");
            textboxMarketingDealSize.Attributes.Add("name", "txtMarketing");
            textboxMarketingDealSize.Attributes.Add("datadefault", "MS");
            textboxMarketingDealSize.Attributes.Add("datasubline", "For example: <em>$100,000</em>");
            if (MSize > 0)
            {
                textboxMarketingDealSize.Attributes.Add("value", MSize.ToString());
                textboxMarketingDealSize.Attributes.Add("placeholder", MSize.ToString());
            }
            else
            {
                textboxMarketingDealSize.Attributes.Add("value", "");
                textboxMarketingDealSize.Attributes.Add("placeholder", "0");
            }
            textboxMarketingDealSize.Attributes.Add("formatType", "currency_dollar");
            textboxMarketingDealSize.Attributes.Add("maxlength", Common.maxLengthDollar);

            TagBuilder textboxTeleprospectingLeads = new TagBuilder("input");
            textboxTeleprospectingLeads.Attributes.Add("type", "text");
            textboxTeleprospectingLeads.Attributes.Add("id", "TeleprospectingLeads");
            textboxTeleprospectingLeads.Attributes.Add("name", "txtTeleprospecting");
            textboxTeleprospectingLeads.Attributes.Add("datadefault", "TL");
            textboxTeleprospectingLeads.Attributes.Add("datasubline", "For example: <em>200</em>");
            if (TLeads > 0)
            {
                textboxTeleprospectingLeads.Attributes.Add("value", TLeads.ToString());
                textboxTeleprospectingLeads.Attributes.Add("placeholder", TLeads.ToString());
            }
            else
            {
                textboxTeleprospectingLeads.Attributes.Add("value", "");
                textboxTeleprospectingLeads.Attributes.Add("placeholder", "0");
            }
            textboxTeleprospectingLeads.Attributes.Add("formatType", "dollarValue");
            textboxTeleprospectingLeads.Attributes.Add("maxlength", Common.maxLengthPriceValue);

            TagBuilder textboxTeleprospectingDealSize = new TagBuilder("input");
            textboxTeleprospectingDealSize.Attributes.Add("type", "text");
            textboxTeleprospectingDealSize.Attributes.Add("id", "TeleprospectingDealSize");
            textboxTeleprospectingDealSize.Attributes.Add("name", "txtTeleprospecting");
            textboxTeleprospectingDealSize.Attributes.Add("datadefault", "TS");
            textboxTeleprospectingDealSize.Attributes.Add("datasubline", "For example: <em>$100,000</em>");
            if (TSize > 0)
            {
                textboxTeleprospectingDealSize.Attributes.Add("value", TSize.ToString());
                textboxTeleprospectingDealSize.Attributes.Add("placeholder", TSize.ToString());
            }
            else
            {
                textboxTeleprospectingDealSize.Attributes.Add("value", "");
                textboxTeleprospectingDealSize.Attributes.Add("placeholder", "0");
            }
            textboxTeleprospectingDealSize.Attributes.Add("formatType", "currency_dollar");
            textboxTeleprospectingDealSize.Attributes.Add("maxlength", Common.maxLengthDollar);


            TagBuilder textboxSalesLeads = new TagBuilder("input");
            textboxSalesLeads.Attributes.Add("type", "text");
            textboxSalesLeads.Attributes.Add("id", "SalesLeads");
            textboxSalesLeads.Attributes.Add("name", "txtSales");
            textboxSalesLeads.Attributes.Add("datadefault", "SL");
            textboxSalesLeads.Attributes.Add("datasubline", "For example: <em>200</em>");
            if (SLeads > 0)
            {
                textboxSalesLeads.Attributes.Add("value", SLeads.ToString());
                textboxSalesLeads.Attributes.Add("placeholder", SLeads.ToString());
            }
            else
            {
                textboxSalesLeads.Attributes.Add("value", "");
                textboxSalesLeads.Attributes.Add("placeholder", "0");
            }
            textboxSalesLeads.Attributes.Add("formatType", "dollarValue");
            textboxSalesLeads.Attributes.Add("maxlength", Common.maxLengthPriceValue);

            TagBuilder textboxSalesDealSize = new TagBuilder("input");
            textboxSalesDealSize.Attributes.Add("type", "text");
            textboxSalesDealSize.Attributes.Add("id", "SalesDealSize");
            textboxSalesDealSize.Attributes.Add("name", "txtSales");
            textboxSalesDealSize.Attributes.Add("datadefault", "SS");
            textboxSalesDealSize.Attributes.Add("datasubline", "For example: <em>$100,000</em>");
            if (SSize > 0)
            {
                textboxSalesDealSize.Attributes.Add("value", SSize.ToString());
                textboxSalesDealSize.Attributes.Add("placeholder", SSize.ToString());
            }
            else
            {
                textboxSalesDealSize.Attributes.Add("value", "");
                textboxSalesDealSize.Attributes.Add("placeholder", "0");
            }
            textboxSalesDealSize.Attributes.Add("formatType", "currency_dollar");
            textboxSalesDealSize.Attributes.Add("maxlength", Common.maxLengthDollar);

            //foreach (var i in lst)
            //{
            //    sb.Append(i.ToString() + "<br>");
            //}

            foreach (KeyValuePair<int, string> item in lst)
            {
                sb.Append(item.Value + "<br>");
                TagBuilder hdn = new TagBuilder("input");
                hdn.Attributes.Add("type", "hidden");
                if (item.Value.Contains("#MarketingDealSize"))//Modified by Mitesh Vaishnav for PL Ticket #534
                {
                    hdn.Attributes.Add("id", "hdn_FunnelMarketing");
                    hdn.Attributes.Add("name", "hdn_FunnelMarketing");
                    hdn.Attributes.Add("value", item.Key.ToString());
                }
                else if (item.Value.Contains("#TeleprospectingLeads"))
                {
                    hdn.Attributes.Add("id", "hdn_FunnelTeleprospecting");
                    hdn.Attributes.Add("name", "hdn_FunnelTeleprospecting");
                    hdn.Attributes.Add("value", item.Key.ToString());
                }
                else if (item.Value.Contains("#SalesLeads"))
                {
                    hdn.Attributes.Add("id", "hdn_FunnelSales");
                    hdn.Attributes.Add("name", "hdn_FunnelSales");
                    hdn.Attributes.Add("value", item.Key.ToString());
                }
                sbHidden.Append(hdn);
                //sb.Append("<br>");
            }



            sb.Replace("#MarketingLeads", textboxMarketingLeads.ToString());
            sb.Replace("#MarketingDealSize", textboxMarketingDealSize.ToString());
            sb.Replace("#TeleprospectingLeads", textboxTeleprospectingLeads.ToString());
            sb.Replace("#TeleprospectingDealSize", textboxTeleprospectingDealSize.ToString());
            sb.Replace("#SalesLeads", textboxSalesLeads.ToString());
            sb.Replace("#SalesDealSize", textboxSalesDealSize.ToString());
            if (bln)
            {
                return new MvcHtmlString(sbHidden.ToString());
            }

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
        public static MvcHtmlString GenerateBudgetAllocationControl(string isMonthlyAllocation)
        {
            string[] lstMonths = "Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec".Split(',');

            string[] lstQuarters = "Q1,Q2,Q3,Q4".Split(',');

            string sb = string.Empty;

            if (isMonthlyAllocation == Enums.PlanAllocatedBy.months.ToString())
            {
                for (int i = 0; i < 12; i++)
                {
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + lstMonths[i] + "</span><span class=\"light-blue-background\"><input id=\"Y" + (i + 1) + "\" class=\"priceValue\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                }
            }
            else
            {
                int quarterCounter = 1;
                for (int i = 0; i < 4; i++)
                {
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + lstQuarters[i] + "</span><span class=\"light-blue-background\"><input id=\"Y" + quarterCounter + "\" class=\"priceValue\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\" /></span></div>";
                    quarterCounter = quarterCounter + 3;
                }
            }

            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Helper of Custom label client wise
        /// Added by Dharmraj, 26-8-2014
        /// #738 Custom label for audience tab
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="customLabelCode">customLabelCode enum</param>
        /// <returns></returns>
        public static string CustomLabelFor(this HtmlHelper helper, Enums.CustomLabelCode customLabelCode)
        {
            MRPEntities db = new MRPEntities();
            string code = customLabelCode.ToString();
            try
            {
                var objCustomLabel = db.CustomLabels.FirstOrDefault(l => l.Code == code && l.ClientId == Sessions.User.ClientId);
                if (objCustomLabel == null)
                {
                    return customLabelCode.ToString();
                }
                else
                {
                    return objCustomLabel.Title;
                }
            }
            catch(Exception ex)
            {
                return customLabelCode.ToString();
            }
        }

        #endregion

        #region Advance Budgeting

        static string formatThousand = "#,##0";
        static string budgetError = " budgetError";

        #region Column1

        /// <summary>
        /// Render activity names for all campaigns
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityCampaign(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string Tab = "1")
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                //tr.AddCssClass("displayRow");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("campaign-row");


                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                div.AddCssClass("campaignLevel");
                TagBuilder aLink = new TagBuilder("a");
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Count() > 0)
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
                if (Tab == "2")
                    aLink.Attributes.Add("id", c.Id);
                else
                    aLink.Attributes.Add("id", c.ActivityId);

                aLink.Attributes.Add("linktype", "campaign");
                aLink.InnerHtml = c.ActivityName;

                div.InnerHtml += aLink.ToString();

                td.InnerHtml = div.ToString();

                td.InnerHtml += ActivityProgram(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, Tab).ToString();
                tr.InnerHtml = td.ToString();
                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Render activity names for all children
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityProgram(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string Tab = "2")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            string childActivity = "tactic";
            bool needAccrodian = true;
            if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                childActivity = "tactic";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
                childActivity = "lineitem";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
                needAccrodian = false;
                childActivity = "";
            }
            if (Tab == "0")
            {
                needAccrodian = false;
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    divProgram.AddCssClass(innerClass);

                    TagBuilder aLink = new TagBuilder("a");
                    if (needAccrodian)
                    {
                        if (model.Where(p1 => p1.ActivityType == childActivity && p1.ParentActivityId == p.ActivityId).Count() > 0)
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

                    if(Tab == "2")
                        aLink.Attributes.Add("id", p.Id);
                    else
                        aLink.Attributes.Add("id", p.ActivityId);
                    
                    aLink.Attributes.Add("linktype", ActivityType);

                    divProgram.InnerHtml += aLink.ToString();

                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ActivityProgram(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ActivityProgram(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model).ToString();
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

        #region Column2

        /// <summary>
        /// Render month header and plans month values
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ActivityId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static MvcHtmlString PlanMonth(this HtmlHelper helper, string ActivityType, string ActivityId, BudgetMonth obj, BudgetMonth parent, string AllocatedBy)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
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
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    string className = "event-row";
                    if (i == 1)
                    {
                        divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jan.ToString(formatThousand);
                        className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        divValue.Attributes.Add("allocated", parent.Feb.ToString(formatThousand));
                        divValue.InnerHtml = obj.Feb.ToString(formatThousand);
                        className = obj.Feb <= parent.Feb ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        divValue.Attributes.Add("allocated", parent.Mar.ToString(formatThousand));
                        divValue.InnerHtml = obj.Mar.ToString(formatThousand);
                        className = obj.Mar <= parent.Mar ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = obj.Apr.ToString(formatThousand);
                        className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 5)
                    {
                        divValue.Attributes.Add("allocated", parent.May.ToString(formatThousand));
                        divValue.InnerHtml = obj.May.ToString(formatThousand);
                        className = obj.May <= parent.May ? className : className + budgetError;
                    }
                    else if (i == 6)
                    {
                        divValue.Attributes.Add("allocated", parent.Jun.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jun.ToString(formatThousand);
                        className = obj.Jun <= parent.Jun ? className : className + budgetError;
                    }
                    else if (i == 7)
                    {
                        divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jul.ToString(formatThousand);
                        className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 8)
                    {
                        divValue.Attributes.Add("allocated", parent.Aug.ToString(formatThousand));
                        divValue.InnerHtml = obj.Aug.ToString(formatThousand);
                        className = obj.Aug <= parent.Aug ? className : className + budgetError;
                    }
                    else if (i == 9)
                    {
                        divValue.Attributes.Add("allocated", parent.Sep.ToString(formatThousand));
                        divValue.InnerHtml = obj.Sep.ToString(formatThousand);
                        className = obj.Sep <= parent.Sep ? className : className + budgetError;
                    }
                    else if (i == 10)
                    {
                        divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = obj.Oct.ToString(formatThousand);
                        className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
                    else if (i == 11)
                    {
                        divValue.Attributes.Add("allocated", parent.Nov.ToString(formatThousand));
                        divValue.InnerHtml = obj.Nov.ToString(formatThousand);
                        className = obj.Nov <= parent.Nov ? className : className + budgetError;
                    }
                    else if (i == 12)
                    {
                        divValue.Attributes.Add("allocated", parent.Dec.ToString(formatThousand));
                        divValue.InnerHtml = obj.Dec.ToString(formatThousand);
                        className = obj.Dec <= parent.Dec ? className : className + budgetError;
                    }
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
            else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
            {
                for (int i = 1; i <= 4; i++)
                {
                    string className = "event-row";
                    TagBuilder tdHeader = new TagBuilder("td");
                    //tdHeader.AddCssClass("event-row");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");
                    //tdValue.AddCssClass("campaign-row");
                    TagBuilder divValue = new TagBuilder("div");
                    divHeader.InnerHtml = "Q" + i.ToString();
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    if (i == 1)
                    {
                        divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jan.ToString(formatThousand);
                        className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = obj.Apr.ToString(formatThousand);
                        className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jul.ToString(formatThousand);
                        className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = obj.Oct.ToString(formatThousand);
                        className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
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
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    string className = "event-row";

                    divValue.InnerHtml = "---";

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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        if (i == 1)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                            className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Feb.ToString(formatThousand));
                            div.InnerHtml = c.Month.Feb.ToString(formatThousand);
                            className = c.Month.Feb <= c.ParentMonth.Feb ? className : className + budgetError;
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Mar.ToString(formatThousand));
                            div.InnerHtml = c.Month.Mar.ToString(formatThousand);
                            className = c.Month.Mar <= c.ParentMonth.Mar ? className : className + budgetError;
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                            className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                        }
                        else if (i == 5)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.May.ToString(formatThousand));
                            div.InnerHtml = c.Month.May.ToString(formatThousand);
                            className = c.Month.May <= c.ParentMonth.May ? className : className + budgetError;
                        }
                        else if (i == 6)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jun.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jun.ToString(formatThousand);
                            className = c.Month.Jun <= c.ParentMonth.Jun ? className : className + budgetError;
                        }
                        else if (i == 7)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                            className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                        }
                        else if (i == 8)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Aug.ToString(formatThousand));
                            div.InnerHtml = c.Month.Aug.ToString(formatThousand);
                            className = c.Month.Aug <= c.ParentMonth.Aug ? className : className + budgetError;
                        }
                        else if (i == 9)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Sep.ToString(formatThousand));
                            div.InnerHtml = c.Month.Sep.ToString(formatThousand);
                            className = c.Month.Sep <= c.ParentMonth.Sep ? className : className + budgetError;
                        }
                        else if (i == 10)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                            className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                        }
                        else if (i == 11)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Nov.ToString(formatThousand));
                            div.InnerHtml = c.Month.Nov.ToString(formatThousand);
                            className = c.Month.Nov <= c.ParentMonth.Nov ? className : className + budgetError;
                        }
                        else if (i == 12)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Dec.ToString(formatThousand));
                            div.InnerHtml = c.Month.Dec.ToString(formatThousand);
                            className = c.Month.Dec <= c.ParentMonth.Dec ? className : className + budgetError;
                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }
                else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        if (i == 1)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                            className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                            className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                            className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                            className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
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
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());


                        div.InnerHtml = "---";

                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ProgramMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, int month)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = innerClass;
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                                    className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jan.ToString(formatThousand);
                            }

                        }
                        else if (month == 2)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Feb <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Feb.ToString(formatThousand));
                                    className = p.Month.Feb <= p.ParentMonth.Feb ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Feb.ToString(formatThousand);
                            }

                        }
                        else if (month == 3)
                        {

                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Mar <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Mar.ToString(formatThousand));
                                    className = p.Month.Mar <= p.ParentMonth.Mar ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Mar.ToString(formatThousand);
                            }

                        }
                        else if (month == 4)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                                    className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Apr.ToString(formatThousand);
                            }

                        }
                        else if (month == 5)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.May <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.May.ToString(formatThousand));
                                    className = p.Month.May <= p.ParentMonth.May ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.May.ToString(formatThousand);
                            }

                        }
                        else if (month == 6)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jun <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jun.ToString(formatThousand));
                                    className = p.Month.Jun <= p.ParentMonth.Jun ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jun.ToString(formatThousand);
                            }

                        }
                        else if (month == 7)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                                    className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jul.ToString(formatThousand);
                            }

                        }
                        else if (month == 8)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Aug <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Aug.ToString(formatThousand));
                                    className = p.Month.Aug <= p.ParentMonth.Aug ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Aug.ToString(formatThousand);
                            }

                        }
                        else if (month == 9)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Sep <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Sep.ToString(formatThousand));
                                    className = p.Month.Sep <= p.ParentMonth.Sep ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Sep.ToString(formatThousand);
                            }


                        }
                        else if (month == 10)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                                    className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Oct.ToString();
                            }

                        }
                        else if (month == 11)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Nov <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Nov.ToString(formatThousand));
                                    className = p.Month.Nov <= p.ParentMonth.Nov ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Nov.ToString(formatThousand);
                            }
                        }
                        else if (month == 12)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Dec <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Dec.ToString(formatThousand));
                                    className = p.Month.Dec <= p.ParentMonth.Dec ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Dec.ToString(formatThousand);
                            }
                        }
                    }
                    else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                                    className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jan.ToString(formatThousand);
                            }

                        }
                        else if (month == 2)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                                    className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Apr.ToString(formatThousand);
                            }

                        }
                        else if (month == 3)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem  && p.Month.Jul <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                                    className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jul.ToString(formatThousand);
                            }

                        }
                        else if (month == 4)
                        {

                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                                    className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Oct.ToString(formatThousand);
                            }

                        }
                    }
                    else
                    {
                        divProgram.InnerHtml = "---";
                    }
                    divProgram.AddCssClass(className);
                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month).ToString();
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

        #region Column3

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string Tab)
        {
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.Where(pl => pl.ActivityType == Helpers.ActivityType.ActivityPlan).SingleOrDefault();
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("event-row");
                TagBuilder div = new TagBuilder("div");
                if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                    div.InnerHtml = sumMonth.ToString(formatThousand);
                    TagBuilder span = new TagBuilder("span");

                    double dblProgress = 0;
                    dblProgress = (sumMonth == 0 && plan.Allocated == 0) ? 0 : (sumMonth > 0 && plan.Allocated == 0) ? 101 : sumMonth / plan.Allocated * 100;
                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        div.AddCssClass("budgetError");
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        span.AddCssClass("progressBar");
                    }
                    div.InnerHtml += span.ToString();
                }
                else
                {
                    div.AddCssClass("firstLevel");
                    div.InnerHtml = "---";
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();


                td = new TagBuilder("td");
                td.AddCssClass("event-row");
                div = new TagBuilder("div");
                if (Tab == "0")
                {
                    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                }
                else
                {
                    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    {
                        div.InnerHtml = plan.Allocated.ToString(formatThousand);
                    }
                    else
                    {
                        div.AddCssClass("firstLevel");
                        div.InnerHtml = "---";
                    }
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");

                //First
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("campaign-row");

                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    //div.InnerHtml = c.Budgeted.ToString();
                    double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                    TagBuilder span = new TagBuilder("span");

                    double dblProgress = 0;
                    dblProgress = (sumMonth == 0 && c.Allocated == 0) ? 0 : (sumMonth > 0 && c.Allocated == 0) ? 101 : sumMonth / c.Allocated * 100;
                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        div.AddCssClass("campaignLevel budgetError");
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        div.AddCssClass("campaignLevel");
                        span.AddCssClass("progressBar");
                    }
                    div.InnerHtml += sumMonth.ToString(formatThousand);
                    div.InnerHtml += span.ToString();
                }
                else
                {
                    div.AddCssClass("firstLevel");
                    div.InnerHtml += "---";
                }

                td.InnerHtml = div.ToString();

                td.InnerHtml += ProgramSummary(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, "first", AllocatedBy, Tab).ToString();

                tr.InnerHtml += td.ToString();

                //Last
                TagBuilder tdLast = new TagBuilder("td");
                tdLast.AddCssClass("campaign-row");

                TagBuilder divLast = new TagBuilder("div");
                divLast.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                divLast.AddCssClass("campaignLevel");
                if (Tab == "0")
                {
                    divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                }
                else
                {
                    if (AllocatedBy != "default")
                    {
                        divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                    }
                    else
                    {
                        divLast.AddCssClass("firstLevel");
                        divLast.InnerHtml = "---";
                    }
                }
                tdLast.InnerHtml = divLast.ToString();
                tdLast.InnerHtml += ProgramSummary(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, "last", AllocatedBy, Tab).ToString();

                tr.InnerHtml += tdLast.ToString();

                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// Recursive call to children for month
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ProgramSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string mode, string AllocatedBy, string Tab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    //divProgram.AddCssClass(innerClass);

                    if (mode == "first")
                    {
                        if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                        {
                            double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                            TagBuilder span = new TagBuilder("span");
                            double dblProgress = 0;
                            dblProgress = (sumMonth == 0 && p.Allocated == 0) ? 0 : (sumMonth > 0 && p.Allocated == 0) ? 101 : sumMonth / p.Allocated * 100;
                            span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            if (dblProgress > 100)
                            {
                                if (ActivityType != "lineitem" && ActivityType != "tactic")
                                {
                                    divProgram.AddCssClass(innerClass + budgetError);
                                    span.AddCssClass("progressBar budgetError");
                                }
                                else
                                {
                                    divProgram.AddCssClass(innerClass);
                                    span.AddCssClass("progressBar");
                                }
                            }
                            else
                            {
                                divProgram.AddCssClass(innerClass);
                                span.AddCssClass("progressBar");
                            }
                            divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                            {
                                divProgram.InnerHtml += span.ToString();
                            }
                        }
                        else
                        {
                            divProgram.InnerHtml += "---";
                            divProgram.AddCssClass(innerClass);
                        }
                    }
                    else
                    {
                        if (Tab == "0")
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
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
                            if (AllocatedBy != "default")
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
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
                                divProgram.InnerHtml += "---";
                            }
                        }
                        //divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                        divProgram.AddCssClass(innerClass);
                    }
                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramSummary(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, mode, AllocatedBy, Tab).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramSummary(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, mode, AllocatedBy, Tab).ToString();


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
        /// <param name="ActivityType"></param>
        /// <param name="ActivityId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static MvcHtmlString AllocatedPlanMonth(this HtmlHelper helper, string ActivityType, string ActivityId, BudgetMonth obj, BudgetMonth parent, string AllocatedBy)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
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
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    string className = "event-row";
                    if (i == 1)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jan.ToString(formatThousand);
                        //className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        //divValue.Attributes.Add("allocated", parent.Feb.ToString(formatThousand));
                        divValue.InnerHtml = obj.Feb.ToString(formatThousand);
                        //className = obj.Feb <= parent.Feb ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        //divValue.Attributes.Add("allocated", parent.Mar.ToString(formatThousand));
                        divValue.InnerHtml = obj.Mar.ToString(formatThousand);
                        //className = obj.Mar <= parent.Mar ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        //divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = obj.Apr.ToString(formatThousand);
                        //className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 5)
                    {
                        //divValue.Attributes.Add("allocated", parent.May.ToString(formatThousand));
                        divValue.InnerHtml = obj.May.ToString(formatThousand);
                        //className = obj.May <= parent.May ? className : className + budgetError;
                    }
                    else if (i == 6)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jun.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jun.ToString(formatThousand);
                        //className = obj.Jun <= parent.Jun ? className : className + budgetError;
                    }
                    else if (i == 7)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jul.ToString(formatThousand);
                        //className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 8)
                    {
                        //divValue.Attributes.Add("allocated", parent.Aug.ToString(formatThousand));
                        divValue.InnerHtml = obj.Aug.ToString(formatThousand);
                        //className = obj.Aug <= parent.Aug ? className : className + budgetError;
                    }
                    else if (i == 9)
                    {
                        //divValue.Attributes.Add("allocated", parent.Sep.ToString(formatThousand));
                        divValue.InnerHtml = obj.Sep.ToString(formatThousand);
                        //className = obj.Sep <= parent.Sep ? className : className + budgetError;
                    }
                    else if (i == 10)
                    {
                        //divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = obj.Oct.ToString(formatThousand);
                        //className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
                    else if (i == 11)
                    {
                        //divValue.Attributes.Add("allocated", parent.Nov.ToString(formatThousand));
                        divValue.InnerHtml = obj.Nov.ToString(formatThousand);
                        //className = obj.Nov <= parent.Nov ? className : className + budgetError;
                    }
                    else if (i == 12)
                    {
                        //divValue.Attributes.Add("allocated", parent.Dec.ToString(formatThousand));
                        divValue.InnerHtml = obj.Dec.ToString(formatThousand);
                        //className = obj.Dec <= parent.Dec ? className : className + budgetError;
                    }
                    if (className.Contains("budgetError"))
                    {
                        className = className.Replace("budgetError", "");
                        divValue.AddCssClass("budgetError");
                    }
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    tdValue.InnerHtml += divValue.ToString();
                    trValue.InnerHtml += tdValue.ToString();
                }
            }
            else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
            {
                for (int i = 1; i <= 4; i++)
                {
                    string className = "event-row";
                    TagBuilder tdHeader = new TagBuilder("td");
                    //tdHeader.AddCssClass("event-row");
                    TagBuilder divHeader = new TagBuilder("div");
                    TagBuilder tdValue = new TagBuilder("td");
                    //tdValue.AddCssClass("campaign-row");
                    TagBuilder divValue = new TagBuilder("div");
                    divHeader.InnerHtml = "Q" + i.ToString();
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    if (i == 1)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jan.ToString(formatThousand);
                        //className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        //divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = obj.Apr.ToString(formatThousand);
                        //className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jul.ToString(formatThousand);
                        //className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        //divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = obj.Oct.ToString(formatThousand);
                        //className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
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
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString AllocatedCampaignMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        if (i == 1)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                            className = c.ParentMonth.Jan >= 0 ? className : className + budgetError;
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Feb.ToString(formatThousand));
                            div.InnerHtml = c.Month.Feb.ToString(formatThousand);
                            className = c.ParentMonth.Feb >= 0 ? className : className + budgetError;
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Mar.ToString(formatThousand));
                            div.InnerHtml = c.Month.Mar.ToString(formatThousand);
                            className = c.ParentMonth.Mar >= 0 ? className : className + budgetError;
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                            className = c.ParentMonth.Apr >= 0 ? className : className + budgetError;
                        }
                        else if (i == 5)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.May.ToString(formatThousand));
                            div.InnerHtml = c.Month.May.ToString(formatThousand);
                            className = c.ParentMonth.May >= 0 ? className : className + budgetError;
                        }
                        else if (i == 6)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jun.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jun.ToString(formatThousand);
                            className = c.ParentMonth.Jun >= 0 ? className : className + budgetError;
                        }
                        else if (i == 7)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                            className = c.ParentMonth.Jul >= 0 ? className : className + budgetError;
                        }
                        else if (i == 8)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Aug.ToString(formatThousand));
                            div.InnerHtml = c.Month.Aug.ToString(formatThousand);
                            className = c.ParentMonth.Aug >= 0 ? className : className + budgetError;
                        }
                        else if (i == 9)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Sep.ToString(formatThousand));
                            div.InnerHtml = c.Month.Sep.ToString(formatThousand);
                            className = c.ParentMonth.Sep >= 0 ? className : className + budgetError;
                        }
                        else if (i == 10)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                            className = c.ParentMonth.Oct >= 0 ? className : className + budgetError;
                        }
                        else if (i == 11)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Nov.ToString(formatThousand));
                            div.InnerHtml = c.Month.Nov.ToString(formatThousand);
                            className = c.ParentMonth.Nov >= 0 ? className : className + budgetError;
                        }
                        else if (i == 12)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Dec.ToString(formatThousand));
                            div.InnerHtml = c.Month.Dec.ToString(formatThousand);
                            className = c.ParentMonth.Dec >= 0 ? className : className + budgetError;
                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }
                else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        string className = "campaignLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        if (i == 1)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                            className = c.ParentMonth.Jan >= 0 ? className : className + budgetError;
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                            className = c.ParentMonth.Apr >= 0 ? className : className + budgetError;
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                            className = c.ParentMonth.Jul >= 0 ? className : className + budgetError;
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                            className = c.ParentMonth.Oct >= 0 ? className : className + budgetError;
                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
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
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        div.InnerHtml = "---";

                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString AllocatedProgramMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, int month)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (ActivityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (ActivityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (ActivityType == "lineitem")
            {
                mainClass = "sub lineitem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = innerClass;
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Jan.ToString(formatThousand);
                            className = p.ParentMonth.Jan >= 0 ? className : className + budgetError;
                        }
                        else if (month == 2)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Feb.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Feb.ToString(formatThousand);
                            className = p.ParentMonth.Feb >= 0 ? className : className + budgetError;
                        }
                        else if (month == 3)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Mar.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Mar.ToString(formatThousand);
                            className = p.ParentMonth.Mar >= 0 ? className : className + budgetError;
                        }
                        else if (month == 4)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Apr.ToString(formatThousand);
                            className = p.ParentMonth.Apr >= 0 ? className : className + budgetError;
                        }
                        else if (month == 5)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.May.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.May.ToString();
                            className = p.ParentMonth.May >= 0 ? className : className + budgetError;
                        }
                        else if (month == 6)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jun.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Jun.ToString(formatThousand);
                            className = p.ParentMonth.Jun >= 0 ? className : className + budgetError;
                        }
                        else if (month == 7)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Jul.ToString(formatThousand);
                            className = p.ParentMonth.Jul >= 0 ? className : className + budgetError;
                        }
                        else if (month == 8)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Aug.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Aug.ToString(formatThousand);
                            className = p.ParentMonth.Aug >= 0 ? className : className + budgetError;
                        }
                        else if (month == 9)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Sep.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Sep.ToString(formatThousand);
                            className = p.ParentMonth.Sep >= 0 ? className : className + budgetError;
                        }
                        else if (month == 10)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Oct.ToString();
                            className = p.ParentMonth.Oct >= 0 ? className : className + budgetError;
                        }
                        else if (month == 11)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Nov.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Nov.ToString(formatThousand);
                            className = p.ParentMonth.Nov >= 0 ? className : className + budgetError;
                        }
                        else if (month == 12)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Dec.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Dec.ToString(formatThousand);
                            className = p.ParentMonth.Dec >= 0 ? className : className + budgetError;
                        }
                    }
                    else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Jan.ToString(formatThousand);
                            className = p.ParentMonth.Jan >= 0 ? className : className + budgetError;
                        }
                        else if (month == 2)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Apr.ToString(formatThousand);
                            className = p.ParentMonth.Apr >= 0 ? className : className + budgetError;
                        }
                        else if (month == 3)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Jul.ToString(formatThousand);
                            className = p.ParentMonth.Jul >= 0 ? className : className + budgetError;
                        }
                        else if (month == 4)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                            divProgram.InnerHtml = p.Month.Oct.ToString(formatThousand);
                            className = p.ParentMonth.Oct >= 0 ? className : className + budgetError;
                        }
                    }
                    else
                    {
                        divProgram.InnerHtml = "---";
                    }
                    divProgram.AddCssClass(className);
                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += AllocatedProgramMonth(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month).ToString();
                    else if (ActivityType == "tactic")
                        div.InnerHtml += AllocatedProgramMonth(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month).ToString();
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

        #region Audience, Vertical & Geography

        #region Column1 Audience, Vertical & Geography

        /// <summary>
        /// Render activity names for all campaigns
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityMainParent(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string Tab = "2", string View = "0")
        {
            string strViewBy = "";
            if (View == "1")
            {
                strViewBy = Helpers.ActivityType.ActivityAudience;
            }
            else if (View == "2")
            {
                strViewBy = Helpers.ActivityType.ActivityGeography;
            }
            else if (View == "3")
            {
                strViewBy = Helpers.ActivityType.ActivityVertical;
            }
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == ParentActivityId).ToList())
            {
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                {
                    TagBuilder tr = new TagBuilder("tr");
                    //tr.AddCssClass("displayRow");
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("campaign-row audience");


                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    div.AddCssClass("firstLevel");
                    TagBuilder aLink = new TagBuilder("span");
                    if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
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
                    //aLink.Attributes.Add("id", c.ActivityId.ToString());
                    //aLink.Attributes.Add("linktype", "main");
                    aLink.InnerHtml = c.ActivityName;

                    div.InnerHtml += aLink.ToString();

                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ActivityChild(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, Tab, View).ToString();
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityChild(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string Tab = "2", string View = "0")
        {
            
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            string childActivity = "tactic";
            bool needAccrodian = true;
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                if (View == "1")
                {
                    parentClassName = "audience";
                }
                else if (View == "2")
                {
                    parentClassName = "geography";
                }
                else if (View == "3")
                {
                    parentClassName = "vertical";
                }
                childActivity = "program";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                childActivity = "tactic";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
                childActivity = "lineitem";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
                needAccrodian = false;
                childActivity = "";
            }
            if (Tab == "0")
            {
                needAccrodian = false;
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    divProgram.AddCssClass(innerClass);

                    TagBuilder aLink = new TagBuilder("a");
                    if (needAccrodian)
                    {
                        if (model.Where(p1 => p1.ActivityType == childActivity && p1.ParentActivityId == p.ActivityId).Count() > 0)
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
                    aLink.Attributes.Add("linktype", ActivityType);

                    divProgram.InnerHtml += aLink.ToString();

                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ActivityChild(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model,Tab,View).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ActivityChild(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, Tab, View).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ActivityChild(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, Tab, View).ToString();
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

        #region Column2 Audience, Vertical & Geography

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string View = "0")
        {
            string strViewBy = "";
            if (View == "1")
            {
                strViewBy = Helpers.ActivityType.ActivityAudience;
            }
            else if (View == "2")
            {
                strViewBy = Helpers.ActivityType.ActivityGeography;
            }
            else if (View == "3")
            {
                strViewBy = Helpers.ActivityType.ActivityVertical;
            }
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == ParentActivityId).ToList())
            {
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                {

                    TagBuilder tr = new TagBuilder("tr");
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            string className = "firstLevel";
                            TagBuilder td = new TagBuilder("td");
                            td.AddCssClass("campaign-row audience");

                            TagBuilder div = new TagBuilder("div");
                            div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                            if (i == 1)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                                //className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                            }
                            else if (i == 2)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Feb.ToString(formatThousand));
                                div.InnerHtml = c.Month.Feb.ToString(formatThousand);
                                //className = c.Month.Feb <= c.ParentMonth.Feb ? className : className + budgetError;
                            }
                            else if (i == 3)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Mar.ToString(formatThousand));
                                div.InnerHtml = c.Month.Mar.ToString(formatThousand);
                                //className = c.Month.Mar <= c.ParentMonth.Mar ? className : className + budgetError;
                            }
                            else if (i == 4)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                                div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                                //className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                            }
                            else if (i == 5)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.May.ToString(formatThousand));
                                div.InnerHtml = c.Month.May.ToString(formatThousand);
                                // className = c.Month.May <= c.ParentMonth.May ? className : className + budgetError;
                            }
                            else if (i == 6)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Jun.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jun.ToString(formatThousand);
                                // className = c.Month.Jun <= c.ParentMonth.Jun ? className : className + budgetError;
                            }
                            else if (i == 7)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                                // className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                            }
                            else if (i == 8)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Aug.ToString(formatThousand));
                                div.InnerHtml = c.Month.Aug.ToString(formatThousand);
                                // className = c.Month.Aug <= c.ParentMonth.Aug ? className : className + budgetError;
                            }
                            else if (i == 9)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Sep.ToString(formatThousand));
                                div.InnerHtml = c.Month.Sep.ToString(formatThousand);
                                // className = c.Month.Sep <= c.ParentMonth.Sep ? className : className + budgetError;
                            }
                            else if (i == 10)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                                div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                                // className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                            }
                            else if (i == 11)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Nov.ToString(formatThousand));
                                div.InnerHtml = c.Month.Nov.ToString(formatThousand);
                                // className = c.Month.Nov <= c.ParentMonth.Nov ? className : className + budgetError;
                            }
                            else if (i == 12)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Dec.ToString(formatThousand));
                                div.InnerHtml = c.Month.Dec.ToString(formatThousand);
                                //className = c.Month.Dec <= c.ParentMonth.Dec ? className : className + budgetError;
                            }
                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i,View).ToString();
                            tr.InnerHtml += td.ToString();
                        }
                    }
                    else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            string className = "firstLevel";
                            TagBuilder td = new TagBuilder("td");
                            td.AddCssClass("campaign-row audience");

                            TagBuilder div = new TagBuilder("div");
                            div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                            if (i == 1)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                                //className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                            }
                            else if (i == 2)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                                div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                                // className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                            }
                            else if (i == 3)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                                //className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                            }
                            else if (i == 4)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                                div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                                //className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                            }
                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, View).ToString();
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
                            div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());


                            div.InnerHtml = "---";

                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, View).ToString();
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, int month, string View = "0")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                //parentClassName = "audience";
                if (View == "1")
                {
                    parentClassName = "audience";
                }
                else if (View == "2")
                {
                    parentClassName = "geography";
                }
                else if (View == "3")
                {
                    parentClassName = "vertical";
                }
            }
            else if (ActivityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (ActivityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (ActivityType == "lineitem")
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = innerClass;
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jan.ToString(formatThousand);
                            }

                        }
                        else if (month == 2)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Feb <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Feb.ToString(formatThousand));
                                    //className = p.Month.Feb <= p.ParentMonth.Feb ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Feb.ToString(formatThousand);
                            }

                        }
                        else if (month == 3)
                        {

                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Mar <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Mar.ToString(formatThousand));
                                    //className = p.Month.Mar <= p.ParentMonth.Mar ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Mar.ToString(formatThousand);
                            }

                        }
                        else if (month == 4)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Apr.ToString(formatThousand);
                            }

                        }
                        else if (month == 5)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.May <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.May.ToString(formatThousand));
                                    //className = p.Month.May <= p.ParentMonth.May ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.May.ToString(formatThousand);
                            }

                        }
                        else if (month == 6)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jun <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jun.ToString(formatThousand));
                                    //className = p.Month.Jun <= p.ParentMonth.Jun ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jun.ToString(formatThousand);
                            }

                        }
                        else if (month == 7)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jul.ToString(formatThousand);
                            }

                        }
                        else if (month == 8)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Aug <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Aug.ToString(formatThousand));
                                    //className = p.Month.Aug <= p.ParentMonth.Aug ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Aug.ToString(formatThousand);
                            }

                        }
                        else if (month == 9)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Sep <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Sep.ToString(formatThousand));
                                    //className = p.Month.Sep <= p.ParentMonth.Sep ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Sep.ToString(formatThousand);
                            }


                        }
                        else if (month == 10)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Oct.ToString();
                            }

                        }
                        else if (month == 11)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Nov <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Nov.ToString(formatThousand));
                                    //className = p.Month.Nov <= p.ParentMonth.Nov ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Nov.ToString(formatThousand);
                            }
                        }
                        else if (month == 12)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Dec <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Dec.ToString(formatThousand));
                                    //className = p.Month.Dec <= p.ParentMonth.Dec ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Dec.ToString(formatThousand);
                            }
                        }
                    }
                    else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jan.ToString(formatThousand);
                            }

                        }
                        else if (month == 2)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Apr.ToString(formatThousand);
                            }

                        }
                        else if (month == 3)
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Jul.ToString(formatThousand);
                            }

                        }
                        else if (month == 4)
                        {

                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                            {
                                divProgram.InnerHtml = "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.Month.Oct.ToString(formatThousand);
                            }

                        }
                    }
                    else
                    {
                        divProgram.InnerHtml = "---";
                    }
                    divProgram.AddCssClass(className);
                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, AllocatedBy, month, View).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month, View).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month, View).ToString();
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

        #region Column3 Audience, Vertical & Geography

        /// <summary>
        /// Get Campaign Month and call Program
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string Tab, string View = "0")
        {
            string strViewBy = "";
            if (View == "1")
            {
                strViewBy = Helpers.ActivityType.ActivityAudience;
            }
            else if (View == "2")
            {
                strViewBy = Helpers.ActivityType.ActivityGeography;
            }
            else if (View == "3")
            {
                strViewBy = Helpers.ActivityType.ActivityVertical;
            }
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.Where(pl => pl.ActivityType == Helpers.ActivityType.ActivityPlan).SingleOrDefault();
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("event-row");
                TagBuilder div = new TagBuilder("div");
                if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                    div.InnerHtml = sumMonth.ToString(formatThousand);
                    TagBuilder span = new TagBuilder("span");

                    double dblProgress = 0;
                    dblProgress = (sumMonth == 0 && plan.Allocated == 0) ? 0 : (sumMonth > 0 && plan.Allocated == 0) ? 101 : sumMonth / plan.Allocated * 100;
                    span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    if (dblProgress > 100)
                    {
                        div.AddCssClass("budgetError");
                        span.AddCssClass("progressBar budgetError");
                    }
                    else
                    {
                        span.AddCssClass("progressBar");
                    }
                    div.InnerHtml += span.ToString();
                }
                else
                {
                    div.AddCssClass("firstLevel");
                    div.InnerHtml = "---";
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();


                td = new TagBuilder("td");
                td.AddCssClass("event-row");
                div = new TagBuilder("div");
                div.AddCssClass("firstLevel");
                if (Tab == "0")
                {
                    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                }
                else
                {
                    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    {
                        div.InnerHtml = plan.Allocated.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }
                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == strViewBy && p.ParentActivityId == ParentActivityId).ToList())
            {
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                {

                    TagBuilder tr = new TagBuilder("tr");

                    //First
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("campaign-row audience");

                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    div.AddCssClass("firstLevel");
                    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    {
                        //div.InnerHtml = c.Budgeted.ToString();
                        double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                        //TagBuilder span = new TagBuilder("span");

                        //double dblProgress = 0;
                        //dblProgress = (sumMonth == 0 && c.Allocated == 0) ? 0 : (sumMonth > 0 && c.Allocated == 0) ? 101 : sumMonth / c.Allocated * 100;
                        //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                        //if (dblProgress > 100)
                        //{
                        //    div.AddCssClass("main budgetError");
                        //    span.AddCssClass("progressBar budgetError");
                        //}
                        //else
                        //{
                        //    div.AddCssClass("mainLevel");
                        //    span.AddCssClass("progressBar");
                        //}
                        
                        div.InnerHtml = sumMonth.ToString(formatThousand);
                        //div.InnerHtml += sumMonth.ToString(formatThousand);
                        //div.InnerHtml += span.ToString();
                    }
                    else
                    {
                        div.InnerHtml += "---";
                    }

                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "first", AllocatedBy, Tab, View).ToString();

                    tr.InnerHtml += td.ToString();

                    //Last
                    TagBuilder tdLast = new TagBuilder("td");
                    tdLast.AddCssClass("campaign-row audience");

                    TagBuilder divLast = new TagBuilder("div");
                    divLast.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    divLast.AddCssClass("firstLevel");
                    if (Tab == "0")
                    {
                        divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                    }
                    else
                    {
                        if (AllocatedBy != "default")
                        {
                            divLast.InnerHtml = "---"; c.Allocated.ToString(formatThousand);
                        }
                        else
                        {
                            divLast.InnerHtml = "---";
                        }
                    }
                    tdLast.InnerHtml = divLast.ToString();
                    tdLast.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "last", AllocatedBy, Tab, View).ToString();

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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string mode, string AllocatedBy, string Tab, string View = "0")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                if (View == "1")
                {
                    parentClassName = "audience";
                }
                else if (View == "2")
                {
                    parentClassName = "geography";
                }
                else if (View == "3")
                {
                    parentClassName = "vertical";
                }
            }
            else if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel";
                parentClassName = "program";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = "lineitemLevel";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    //divProgram.AddCssClass(innerClass);

                    if (mode == "first")
                    {
                        if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                        {
                            double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                            divProgram.AddCssClass(innerClass);
                            divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                        }
                        else
                        {
                            divProgram.InnerHtml += "---";
                            divProgram.AddCssClass(innerClass + " firstLevel");
                        }
                    }
                    else
                    {
                        if (Tab == "0")
                        {
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
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
                            if (AllocatedBy != "default")
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
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
                                divProgram.InnerHtml += "---";
                            }
                        }
                        //divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                        divProgram.AddCssClass(innerClass);
                    }
                    div.InnerHtml += divProgram.ToString();
                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, mode, AllocatedBy, Tab, View ).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, mode, AllocatedBy, Tab, View).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, mode, AllocatedBy, Tab, View).ToString();


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

        #endregion //Audience, Vertical & Geography


        #endregion //Advance Budgeting
    }
}