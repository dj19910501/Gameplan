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
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + lstMonths[i] + "</span><span class=\"light-blue-background\"><input id=\"Y" + (i + 1) + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\"  /></span></div>";
                }
            }
            else
            {
                int quarterCounter = 1;
                for (int i = 0; i < 4; i++)
                {
                    sb += "<div class=\"budget-month\"><span class=\"month\">" + lstQuarters[i] + "</span><span class=\"light-blue-background\"><input id=\"Y" + quarterCounter + "\" class=\"priceValueAllowNull\" placeholder=\"- - -\" maxlength=\"" + Common.maxLengthPriceValue + "\" /></span></div>";
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
            catch (Exception ex)
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

                    if (Tab == "2")
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
        public static MvcHtmlString PlanMonth(this HtmlHelper helper, string ActivityType, string ActivityId, BudgetMonth obj, BudgetMonth parent, string AllocatedBy, string strTab)
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
                    if (strTab == "2")
                    {
                        if (i == 1)
                        {
                            divValue.InnerHtml = obj.Jan.ToString(formatThousand);

                        }
                        else if (i == 2)
                        {
                            divValue.InnerHtml = obj.Feb.ToString(formatThousand);

                        }
                        else if (i == 3)
                        {
                            divValue.InnerHtml = obj.Mar.ToString(formatThousand);

                        }
                        else if (i == 4)
                        {
                            divValue.InnerHtml = obj.Apr.ToString(formatThousand);

                        }
                        else if (i == 5)
                        {
                            divValue.InnerHtml = obj.May.ToString(formatThousand);

                        }
                        else if (i == 6)
                        {
                            divValue.InnerHtml = obj.Jun.ToString(formatThousand);

                        }
                        else if (i == 7)
                        {
                            divValue.InnerHtml = obj.Jul.ToString(formatThousand);

                        }
                        else if (i == 8)
                        {
                            divValue.InnerHtml = obj.Aug.ToString(formatThousand);

                        }
                        else if (i == 9)
                        {
                            divValue.InnerHtml = obj.Sep.ToString(formatThousand);

                        }
                        else if (i == 10)
                        {
                            divValue.InnerHtml = obj.Oct.ToString(formatThousand);

                        }
                        else if (i == 11)
                        {
                            divValue.InnerHtml = obj.Nov.ToString(formatThousand);

                        }
                        else if (i == 12)
                        {
                            divValue.InnerHtml = obj.Dec.ToString(formatThousand);

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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string strTab)
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

                        td.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i, strTab).ToString();
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

                        td.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i, strTab).ToString();
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

                        if (strTab == "2")
                        {
                            if (i == 1)
                            {
                                div.InnerHtml = c.Month.Jan.ToString(formatThousand);

                            }
                            else if (i == 2)
                            {
                                div.InnerHtml = c.Month.Feb.ToString(formatThousand);

                            }
                            else if (i == 3)
                            {
                                div.InnerHtml = c.Month.Mar.ToString(formatThousand);

                            }
                            else if (i == 4)
                            {
                                div.InnerHtml = c.Month.Apr.ToString(formatThousand);

                            }
                            else if (i == 5)
                            {
                                div.InnerHtml = c.Month.May.ToString(formatThousand);

                            }
                            else if (i == 6)
                            {
                                div.InnerHtml = c.Month.Jun.ToString(formatThousand);

                            }
                            else if (i == 7)
                            {
                                div.InnerHtml = c.Month.Jul.ToString(formatThousand);

                            }
                            else if (i == 8)
                            {
                                div.InnerHtml = c.Month.Aug.ToString(formatThousand);

                            }
                            else if (i == 9)
                            {
                                div.InnerHtml = c.Month.Sep.ToString(formatThousand);

                            }
                            else if (i == 10)
                            {
                                div.InnerHtml = c.Month.Oct.ToString(formatThousand);

                            }
                            else if (i == 11)
                            {
                                div.InnerHtml = c.Month.Nov.ToString(formatThousand);

                            }
                            else if (i == 12)
                            {
                                div.InnerHtml = c.Month.Dec.ToString(formatThousand);

                            }
                        }
                        else
                        {
                            div.InnerHtml = "---";
                        }
                        div.AddCssClass(className);
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i, strTab).ToString();
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
        public static MvcHtmlString ProgramMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, int month, string strTab)
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
                        if (strTab == "2")
                        {
                            if (month == 1)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                                {
                                    divProgram.InnerHtml = "---";
                                }
                                else
                                {
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
                                    divProgram.InnerHtml = p.Month.Dec.ToString(formatThousand);
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

                    if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month, strTab).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramMonth(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month, strTab).ToString();
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
                    if (Tab == "2")
                    {
                        double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                        div.InnerHtml = sumMonth.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }

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
                    if (Tab == "2")
                    {
                        double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                        div.InnerHtml += sumMonth.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml += "---";
                    }
                    div.AddCssClass("firstLevel");

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
                            if (Tab == "2")
                            {
                                double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                                divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            }
                            else
                            {
                                divProgram.InnerHtml += "---";
                            }
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
                        div.InnerHtml += ActivityChild(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, Tab, View).ToString();
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
        public static MvcHtmlString ParentMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string strTab, string View = "0")
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

                            td.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, strTab, View).ToString();
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

                            td.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, strTab, View).ToString();
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

                            if (strTab == "2")
                            {
                                if (i == 1)
                                {
                                    div.InnerHtml = c.Month.Jan.ToString(formatThousand);

                                }
                                else if (i == 2)
                                {
                                    div.InnerHtml = c.Month.Feb.ToString(formatThousand);

                                }
                                else if (i == 3)
                                {
                                    div.InnerHtml = c.Month.Mar.ToString(formatThousand);

                                }
                                else if (i == 4)
                                {
                                    div.InnerHtml = c.Month.Apr.ToString(formatThousand);

                                }
                                else if (i == 5)
                                {
                                    div.InnerHtml = c.Month.May.ToString(formatThousand);

                                }
                                else if (i == 6)
                                {
                                    div.InnerHtml = c.Month.Jun.ToString(formatThousand);

                                }
                                else if (i == 7)
                                {
                                    div.InnerHtml = c.Month.Jul.ToString(formatThousand);

                                }
                                else if (i == 8)
                                {
                                    div.InnerHtml = c.Month.Aug.ToString(formatThousand);

                                }
                                else if (i == 9)
                                {
                                    div.InnerHtml = c.Month.Sep.ToString(formatThousand);

                                }
                                else if (i == 10)
                                {
                                    div.InnerHtml = c.Month.Oct.ToString(formatThousand);

                                }
                                else if (i == 11)
                                {
                                    div.InnerHtml = c.Month.Nov.ToString(formatThousand);

                                }
                                else if (i == 12)
                                {
                                    div.InnerHtml = c.Month.Dec.ToString(formatThousand);

                                }
                            }
                            else
                            {
                                div.InnerHtml = "---";
                            }

                            div.AddCssClass(className);
                            td.InnerHtml = div.ToString();

                            td.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, strTab, View).ToString();
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
        public static MvcHtmlString ChildMonth(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, int month, string strTab, string View = "0")
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
                        if (strTab == "2")
                        {
                            if (month == 1)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                                {
                                    divProgram.InnerHtml = "---";
                                }
                                else
                                {
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
                                    divProgram.InnerHtml = p.Month.Dec.ToString(formatThousand);
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

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, AllocatedBy, month, strTab, View).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month, strTab, View).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonth(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month, strTab, View).ToString();
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
                    if (Tab == "2")
                    {
                        double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                        div.InnerHtml = sumMonth.ToString(formatThousand);
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }
                    div.AddCssClass("firstLevel");

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
                            if (Tab == "2")
                            {
                                double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                                divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            }
                            else
                            {
                                divProgram.InnerHtml += "---";
                            }
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
                        div.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, mode, AllocatedBy, Tab, View).ToString();
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

        #region Custom Fields

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #718
        /// function generate html output for custom fields of campaign,program or tactic
        /// </summary>
        /// <param name="id">Plan Tactic Id or Plan Campaign Id or Plan Program Id</param>
        /// <param name="section">Parameter contains value from enum EntityType like Campaign or Program or Tactic.</param>
        /// <returns>If Plan Tactic or Plan Campaign or Plan Program contains custom fields than returns html string else empty string</returns>
        public static MvcHtmlString GenerateCustomFields(int id, string section)
        {
            //list of custom fields for particular campaign or Program or Tactic
            List<CustomFieldModel> customFieldList = Common.GetCustomFields(id, section);
            string sb = string.Empty;
            if (customFieldList.Count != 0)
            {
                //count variable for defining alternate raw
                int count = 0;
                foreach (var item in customFieldList)
                {
                    string classname = "content-row";
                    if (count % 2 != 0)
                    {
                        classname += " alternate";
                    }
                    //check if custom field type is textbox then generate textbox and if custom field type is dropdownlist then generate dropdownlist
                    if (item.customFieldType == Enums.CustomFieldType.TextBox.ToString())
                    {
                        sb += "<div class=\""+classname+" \"><label class=\"padding-left4\" title=\"" + item.name + "\">" + Common.TruncateLable(item.name, 33) + "</label>";
                        //When item value contains double quots then it would be replaced 
                        string customFieldEntityValue = item.value != null ? item.value.Replace("\"", "&quot;") : string.Empty;
                        sb += "<input id=\"cf_" + item.customFieldId + "\" type=\"text\" value=\"" + customFieldEntityValue + "\" cf_id=\"" + item.customFieldId + "\" maxlength=\"255\"";
                        //If custom field is required than add attribute require
                        if (item.isRequired)
                        {
                            sb += " require=\"true\"";
                        }
                        sb += "></div>";
                    }
                    else if (item.customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        classname += " margin-bottom10";
                        sb += "<div class=\"" + classname + " \"><label class=\"padding-left4\" title=\"" + item.name + "\">" + Common.TruncateLable(item.name, 33) + "</label>";
                        sb += "<span class=\"verticalIdSelectBox\">  <select id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"ddlStyle\"";
                        //If custom field is required than add attribute require
                        if (item.isRequired)
                        {
                            sb += " require=\"true\"";
                        }
                        sb += "><option value=\"\">Please Select</option>";
                        //set dropdown option values
                        if (item.option.Count != 0)
                        {
                            foreach (var objOption in item.option)
                            {
                                //check - if custom field's value inserted before from dropdownlist then set it as selected
                                if (item.value != objOption.customFieldOptionId.ToString())
                                {
                                    sb += "<option value=\"" + objOption.customFieldOptionId + "\">" + objOption.value + "</option>";
                                }
                                else
                                {
                                    sb += "<option value=\"" + objOption.customFieldOptionId + "\" selected=true>" + objOption.value + "</option>";
                                }
                            }
                        }
                        sb += "</select></span></div>";

                    }
                    count = count + 1;
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static MvcHtmlString ActivityMainParentReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BudgetModelReport c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityPlan && p.ParentActivityId == ParentActivityId).ToList())
            {
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                {
                    TagBuilder tr = new TagBuilder("tr");
                    //tr.AddCssClass("displayRow");
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("campaign-rowReport audience");


                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    div.AddCssClass("firstLevel");
                    TagBuilder aLink = new TagBuilder("span");
                    if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                    {
                        TagBuilder aAccordian = new TagBuilder("a");
                        aAccordian.AddCssClass("accordionClick");
                       // aAccordian.AddCssClass("collapse");
                        div.InnerHtml = aAccordian.ToString();
                        //aLink.Attributes.Add("style", "cursor:pointer;");
                    }
                    else
                    {
                        aLink.Attributes.Add("style", "padding-left:20px;");
                    }
                    //aLink.Attributes.Add("id", c.ActivityId.ToString());
                    //aLink.Attributes.Add("linktype", "main");
                    aLink.InnerHtml = c.ActivityName;

                    div.InnerHtml += aLink.ToString();

                    td.InnerHtml = div.ToString();

                    td.InnerHtml += ActivityChildReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model).ToString();
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
        public static MvcHtmlString ActivityChildReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model)
        {

            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            string childActivity = "tactic";
            bool needAccrodian = true;
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevelReport";
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

            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    divProgram.AddCssClass(innerClass);

                    TagBuilder aLink = new TagBuilder("span");
                    if (needAccrodian)
                    {
                        if (model.Where(p1 => p1.ActivityType == childActivity && p1.ParentActivityId == p.ActivityId).Count() > 0)
                        {
                            TagBuilder aAccordian = new TagBuilder("a");
                            //aAccordian.Attributes.Add("href", "#");
                            aAccordian.AddCssClass("accordionClick");
                            //aAccordian.AddCssClass("collapse");
                            divProgram.InnerHtml = aAccordian.ToString();
                        }
                        else
                        {
                            aLink.Attributes.Add("style", "padding-left:20px;");
                        }
                    }

                    //aLink.Attributes.Add("href", "#");
                    aLink.InnerHtml = p.ActivityName;


                    //aLink.Attributes.Add("id", p.Id);
                    //aLink.Attributes.Add("linktype", ActivityType);

                    divProgram.InnerHtml += aLink.ToString();

                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ActivityChildReport(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ActivityChildReport(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ActivityChildReport(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
        }

        /// <summary>
        /// Render month header and plans month values
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ActivityId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static MvcHtmlString PlanMonthReport(this HtmlHelper helper, string ActivityType, string ActivityId, List<BudgetModelReport> model, string AllocatedBy, bool isPlanTab)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            TagBuilder trInnerHeader = new TagBuilder("tr");
            int IncrementCount = 1;
            bool IsQuarter = false;
            if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
            {
                IncrementCount = 3;
                IsQuarter = true;
            }
            BudgetModelReport objMain = model.Where(main => main.ActivityType == ActivityType && main.ActivityId == ActivityId).FirstOrDefault();
            string className = "";// "event-row";
            double AllocatedValue = 0;
            double ActualValue = 0;
            double PlannedValue = 0;
            double ChildAllocatedValue = 0;
            for (int i = 1; i <= 12; i += IncrementCount)
            {
                TagBuilder tdHeader = new TagBuilder("td");
                //tdHeader.AddCssClass("event-row");
                TagBuilder divHeader = new TagBuilder("div");

                if (IsQuarter)
                {
                    divHeader.InnerHtml = "Q" + ((i / IncrementCount) + 1).ToString();
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

                divValueInnerActual.Attributes.Add("id", ActivityType + ActivityId.ToString());

                AllocatedValue = 0;
                ActualValue = 0;
                PlannedValue = 0;
                ChildAllocatedValue = 0;

                switch (i)
                {
                    case 1:
                        ActualValue = objMain.MonthActual.Jan;
                        AllocatedValue = objMain.MonthAllocated.Jan;
                        PlannedValue = objMain.MonthPlanned.Jan;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Jan;
                        break;
                    case 2:
                        ActualValue = objMain.MonthActual.Feb;
                        AllocatedValue = objMain.MonthAllocated.Feb;
                        PlannedValue = objMain.MonthPlanned.Feb;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Feb;
                        break;
                    case 3:
                        ActualValue = objMain.MonthActual.Mar;
                        AllocatedValue = objMain.MonthAllocated.Mar;
                        PlannedValue = objMain.MonthPlanned.Mar;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Mar;
                        break;
                    case 4:
                        ActualValue = objMain.MonthActual.Apr;
                        AllocatedValue = objMain.MonthAllocated.Apr;
                        PlannedValue = objMain.MonthPlanned.Apr;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Apr;
                        break;
                    case 5:
                        ActualValue = objMain.MonthActual.May;
                        AllocatedValue = objMain.MonthAllocated.May;
                        PlannedValue = objMain.MonthPlanned.May;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.May;
                        break;
                    case 6:
                        ActualValue = objMain.MonthActual.Jun;
                        AllocatedValue = objMain.MonthAllocated.Jun;
                        PlannedValue = objMain.MonthPlanned.Jun;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Jun;
                        break;
                    case 7:
                        ActualValue = objMain.MonthActual.Jul;
                        AllocatedValue = objMain.MonthAllocated.Jul;
                        PlannedValue = objMain.MonthPlanned.Jul;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Jul;
                        break;
                    case 8:
                        ActualValue = objMain.MonthActual.Aug;
                        AllocatedValue = objMain.MonthAllocated.Aug;
                        PlannedValue = objMain.MonthPlanned.Aug;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Aug;
                        break;
                    case 9:
                        ActualValue = objMain.MonthActual.Sep;
                        AllocatedValue = objMain.MonthAllocated.Sep;
                        PlannedValue = objMain.MonthPlanned.Sep;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Sep;
                        break;
                    case 10:
                        ActualValue = objMain.MonthActual.Oct;
                        AllocatedValue = objMain.MonthAllocated.Oct;
                        PlannedValue = objMain.MonthPlanned.Oct;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Oct;
                        break;
                    case 11:
                        ActualValue = objMain.MonthActual.Nov;
                        AllocatedValue = objMain.MonthAllocated.Nov;
                        PlannedValue = objMain.MonthPlanned.Nov;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Nov;
                        break;
                    case 12:
                        ActualValue = objMain.MonthActual.Dec;
                        AllocatedValue = objMain.MonthAllocated.Dec;
                        PlannedValue = objMain.MonthPlanned.Dec;
                        ChildAllocatedValue = objMain.ChildMonthAllocated.Dec;
                        break;
                    default:
                        break;
                }

                TagBuilder span = new TagBuilder("span");
                double dblProgress = 0;
                // Actual
                className = "";
                divValueInnerActual.InnerHtml = ActualValue.ToString(formatThousand);
                if (isPlanTab)
                {
                    if (ActualValue > ChildAllocatedValue)
                    {
                        className += budgetError;
                        divValueInnerActual.Attributes.Add("OverBudget", Math.Abs(ChildAllocatedValue - ActualValue).ToString(formatThousand));
                    }
                    span = new TagBuilder("span");
                    dblProgress = 0;
                    dblProgress = (ActualValue == 0 && ChildAllocatedValue == 0) ? 0 : (ActualValue > 0 && ChildAllocatedValue == 0) ? 101 : ActualValue / ChildAllocatedValue * 100;

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

                divValueInnerPlanned.Attributes.Add("id", ActivityType + ActivityId.ToString());
                divValueInnerPlanned.InnerHtml = PlannedValue.ToString(formatThousand);
                if (isPlanTab)
                {
                    if (PlannedValue > ChildAllocatedValue)
                    {
                        className += budgetError;
                        divValueInnerPlanned.Attributes.Add("OverBudget", Math.Abs(ChildAllocatedValue - PlannedValue).ToString(formatThousand));
                    }
                    span = new TagBuilder("span");
                    dblProgress = 0;
                    dblProgress = (PlannedValue == 0 && ChildAllocatedValue == 0) ? 0 : (PlannedValue > 0 && ChildAllocatedValue == 0) ? 101 : PlannedValue / ChildAllocatedValue * 100;

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
                divHeaderInnerAllocated.InnerHtml = "Allocated";

                TagBuilder aAllocated = new TagBuilder("a");
                aAllocated.AddCssClass("UpperArrowReport");
                aAllocated.Attributes.Add("id", "Allocated_Up_" + i);
                divHeaderInnerAllocated.InnerHtml += aAllocated;
                aAllocated = new TagBuilder("a");
                aAllocated.AddCssClass("DownArrowReport");
                aAllocated.Attributes.Add("id", "Allocated_Down_" + i);
                divHeaderInnerAllocated.InnerHtml += aAllocated;

                divValueInnerAllocated.Attributes.Add("id", ActivityType + ActivityId.ToString());
                if (isPlanTab)
                {
                    divValueInnerAllocated.InnerHtml = ChildAllocatedValue.ToString(formatThousand);
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        public static MvcHtmlString ParentMonthReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model, string AllocatedBy, bool isPlanTab)
        {

            StringBuilder sb = new StringBuilder();
            foreach (BudgetModelReport c in model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList())
            {
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                {
                    int IncrementCount = 1;
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        IncrementCount = 3;
                    }
                    TagBuilder tr = new TagBuilder("tr");
                    for (int i = 1; i <= 12; i += IncrementCount)
                    {
                        string className = "firstLevel";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("event-rowReport");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        double AllocatedValue = 0;
                        double ActualValue = 0;
                        double PlannedValue = 0;
                        double ChildAllocatedValue = 0;

                        switch (i)
                        {
                            case 1:
                                ActualValue = c.MonthActual.Jan;
                                AllocatedValue = c.MonthAllocated.Jan;
                                PlannedValue = c.MonthPlanned.Jan;
                                ChildAllocatedValue = c.ChildMonthAllocated.Jan;
                                break;
                            case 2:
                                ActualValue = c.MonthActual.Feb;
                                AllocatedValue = c.MonthAllocated.Feb;
                                PlannedValue = c.MonthPlanned.Feb;
                                ChildAllocatedValue = c.ChildMonthAllocated.Feb;
                                break;
                            case 3:
                                ActualValue = c.MonthActual.Mar;
                                AllocatedValue = c.MonthAllocated.Mar;
                                PlannedValue = c.MonthPlanned.Mar;
                                ChildAllocatedValue = c.ChildMonthAllocated.Mar;
                                break;
                            case 4:
                                ActualValue = c.MonthActual.Apr;
                                AllocatedValue = c.MonthAllocated.Apr;
                                PlannedValue = c.MonthPlanned.Apr;
                                ChildAllocatedValue = c.ChildMonthAllocated.Apr;
                                break;
                            case 5:
                                ActualValue = c.MonthActual.May;
                                AllocatedValue = c.MonthAllocated.May;
                                PlannedValue = c.MonthPlanned.May;
                                ChildAllocatedValue = c.ChildMonthAllocated.May;
                                break;
                            case 6:
                                ActualValue = c.MonthActual.Jun;
                                AllocatedValue = c.MonthAllocated.Jun;
                                PlannedValue = c.MonthPlanned.Jun;
                                ChildAllocatedValue = c.ChildMonthAllocated.Jun;
                                break;
                            case 7:
                                ActualValue = c.MonthActual.Jul;
                                AllocatedValue = c.MonthAllocated.Jul;
                                PlannedValue = c.MonthPlanned.Jul;
                                ChildAllocatedValue = c.ChildMonthAllocated.Jul;
                                break;
                            case 8:
                                ActualValue = c.MonthActual.Aug;
                                AllocatedValue = c.MonthAllocated.Aug;
                                PlannedValue = c.MonthPlanned.Aug;
                                ChildAllocatedValue = c.ChildMonthAllocated.Aug;
                                break;
                            case 9:
                                ActualValue = c.MonthActual.Sep;
                                AllocatedValue = c.MonthAllocated.Sep;
                                PlannedValue = c.MonthPlanned.Sep;
                                ChildAllocatedValue = c.ChildMonthAllocated.Sep;
                                break;
                            case 10:
                                ActualValue = c.MonthActual.Oct;
                                AllocatedValue = c.MonthAllocated.Oct;
                                PlannedValue = c.MonthPlanned.Oct;
                                ChildAllocatedValue = c.ChildMonthAllocated.Oct;
                                break;
                            case 11:
                                ActualValue = c.MonthActual.Nov;
                                AllocatedValue = c.MonthAllocated.Nov;
                                PlannedValue = c.MonthPlanned.Nov;
                                ChildAllocatedValue = c.ChildMonthAllocated.Nov;
                                break;
                            case 12:
                                ActualValue = c.MonthActual.Dec;
                                AllocatedValue = c.MonthAllocated.Dec;
                                PlannedValue = c.MonthPlanned.Dec;
                                ChildAllocatedValue = c.ChildMonthAllocated.Dec;
                                break;
                            default:
                                break;
                        }

                        TagBuilder span = new TagBuilder("span");
                        double dblProgress = 0;
                        //Actual
                        div.InnerHtml = ActualValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (ActualValue > AllocatedValue)
                            {
                                className += budgetError;
                                div.Attributes.Add("OverBudget", Math.Abs(AllocatedValue - ActualValue).ToString(formatThousand));
                            }
                            span = new TagBuilder("span");
                            dblProgress = 0;
                            dblProgress = (ActualValue == 0 && AllocatedValue == 0) ? 0 : (ActualValue > 0 && AllocatedValue == 0) ? 101 : ActualValue / AllocatedValue * 100;

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
                        td.InnerHtml += ChildMonthActualReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, isPlanTab).ToString();
                        tr.InnerHtml += td.ToString();

                        // Planned
                        className = "firstLevel";
                        TagBuilder tdPlanned = new TagBuilder("td");
                        tdPlanned.AddCssClass("event-rowReport");

                        TagBuilder divPlanned = new TagBuilder("div");
                        divPlanned.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                        divPlanned.InnerHtml = PlannedValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (PlannedValue > AllocatedValue)
                            {
                                className += budgetError;
                                divPlanned.Attributes.Add("OverBudget", Math.Abs(AllocatedValue - PlannedValue).ToString(formatThousand));
                            }
                            span = new TagBuilder("span");
                            dblProgress = 0;
                            dblProgress = (PlannedValue == 0 && AllocatedValue == 0) ? 0 : (PlannedValue > 0 && AllocatedValue == 0) ? 101 : PlannedValue / AllocatedValue * 100;

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
                        tdPlanned.InnerHtml += ChildMonthPlannedReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, isPlanTab).ToString();
                        tr.InnerHtml += tdPlanned.ToString();

                        // Allocated
                        className = "firstLevel";
                        TagBuilder tdAllocated = new TagBuilder("td");
                        tdAllocated.AddCssClass("event-rowReport");

                        TagBuilder divAllocated = new TagBuilder("div");
                        divAllocated.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                        if (isPlanTab)
                        {
                            divAllocated.InnerHtml = AllocatedValue.ToString(formatThousand);

                            if (AllocatedValue < ChildAllocatedValue)
                            {
                                className += budgetError;
                                divAllocated.Attributes.Add("Allocated", ChildAllocatedValue.ToString(formatThousand));
                            }
                            else if (AllocatedValue > ChildAllocatedValue)
                            {
                                divAllocated.Attributes.Add("Remaining", (AllocatedValue - ChildAllocatedValue).ToString(formatThousand));
                            }


                        }
                        else
                        {
                            divAllocated.InnerHtml = "---";
                        }
                        divAllocated.AddCssClass(className);
                        tdAllocated.InnerHtml = divAllocated.ToString();

                        tdAllocated.InnerHtml += ChildMonthAllocatedReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, AllocatedBy, i, isPlanTab).ToString();
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildMonthActualReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model, string AllocatedBy, int month, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
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
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = "";
                    TagBuilder span = new TagBuilder("span");
                    double dblProgress = 0;
                    double ActualPlannedValue = 0;
                    double AllocatedValue = 0;
                    switch (month)
                    {
                        case 1:
                            ActualPlannedValue = p.MonthActual.Jan;
                            AllocatedValue = p.MonthAllocated.Jan;
                            break;
                        case 2:
                            ActualPlannedValue = p.MonthActual.Feb;
                            AllocatedValue = p.MonthAllocated.Feb;
                            break;
                        case 3:
                            ActualPlannedValue = p.MonthActual.Mar;
                            AllocatedValue = p.MonthAllocated.Mar;
                            break;
                        case 4:
                            ActualPlannedValue = p.MonthActual.Apr;
                            AllocatedValue = p.MonthAllocated.Apr;
                            break;
                        case 5:
                            ActualPlannedValue = p.MonthActual.May;
                            AllocatedValue = p.MonthAllocated.May;
                            break;
                        case 6:
                            ActualPlannedValue = p.MonthActual.Jun;
                            AllocatedValue = p.MonthAllocated.Jun;
                            break;
                        case 7:
                            ActualPlannedValue = p.MonthActual.Jul;
                            AllocatedValue = p.MonthAllocated.Jul;
                            break;
                        case 8:
                            ActualPlannedValue = p.MonthActual.Aug;
                            AllocatedValue = p.MonthAllocated.Aug;
                            break;
                        case 9:
                            ActualPlannedValue = p.MonthActual.Sep;
                            AllocatedValue = p.MonthAllocated.Sep;
                            break;
                        case 10:
                            ActualPlannedValue = p.MonthActual.Oct;
                            AllocatedValue = p.MonthAllocated.Oct;
                            break;
                        case 11:
                            ActualPlannedValue = p.MonthActual.Nov;
                            AllocatedValue = p.MonthAllocated.Nov;
                            break;
                        case 12:
                            ActualPlannedValue = p.MonthActual.Dec;
                            AllocatedValue = p.MonthAllocated.Dec;
                            break;
                        default:
                            break;
                    }


                    if (ActivityType == Helpers.ActivityType.ActivityLineItem && ActualPlannedValue <= 0)
                    {
                        divProgram.InnerHtml = "---";
                    }
                    else if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
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

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonthActualReport(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonthActualReport(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonthActualReport(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
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
        public static MvcHtmlString ChildMonthPlannedReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model, string AllocatedBy, int month, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
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
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = "";
                    TagBuilder span = new TagBuilder("span");
                    double dblProgress = 0;
                    double PlannedValue = 0;
                    double AllocatedValue = 0;
                    switch (month)
                    {
                        case 1:
                            PlannedValue = p.MonthPlanned.Jan;
                            AllocatedValue = p.MonthAllocated.Jan;
                            break;
                        case 2:
                            PlannedValue = p.MonthPlanned.Feb;
                            AllocatedValue = p.MonthAllocated.Feb;
                            break;
                        case 3:
                            PlannedValue = p.MonthPlanned.Mar;
                            AllocatedValue = p.MonthAllocated.Mar;
                            break;
                        case 4:
                            PlannedValue = p.MonthPlanned.Apr;
                            AllocatedValue = p.MonthAllocated.Apr;
                            break;
                        case 5:
                            PlannedValue = p.MonthPlanned.May;
                            AllocatedValue = p.MonthAllocated.May;
                            break;
                        case 6:
                            PlannedValue = p.MonthPlanned.Jun;
                            AllocatedValue = p.MonthAllocated.Jun;
                            break;
                        case 7:
                            PlannedValue = p.MonthPlanned.Jul;
                            AllocatedValue = p.MonthAllocated.Jul;
                            break;
                        case 8:
                            PlannedValue = p.MonthPlanned.Aug;
                            AllocatedValue = p.MonthAllocated.Aug;
                            break;
                        case 9:
                            PlannedValue = p.MonthPlanned.Sep;
                            AllocatedValue = p.MonthAllocated.Sep;
                            break;
                        case 10:
                            PlannedValue = p.MonthPlanned.Oct;
                            AllocatedValue = p.MonthAllocated.Oct;
                            break;
                        case 11:
                            PlannedValue = p.MonthPlanned.Nov;
                            AllocatedValue = p.MonthAllocated.Nov;
                            break;
                        case 12:
                            PlannedValue = p.MonthPlanned.Dec;
                            AllocatedValue = p.MonthAllocated.Dec;
                            break;
                        default:
                            break;
                    }

                    if (ActivityType == Helpers.ActivityType.ActivityLineItem && PlannedValue <= 0)
                    {
                        divProgram.InnerHtml = "---";
                    }
                    else if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                    {
                        divProgram.InnerHtml = PlannedValue.ToString(formatThousand);
                        if (isPlanTab)
                        {
                            if (PlannedValue > AllocatedValue)
                            {
                                className += budgetError;
                                divProgram.Attributes.Add("OverBudget", Math.Abs(AllocatedValue - PlannedValue).ToString(formatThousand));
                            }
                            dblProgress = (PlannedValue == 0 && AllocatedValue == 0) ? 0 : (PlannedValue > 0 && AllocatedValue == 0) ? 101 : PlannedValue / AllocatedValue * 100;

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
                        divProgram.InnerHtml = PlannedValue.ToString(formatThousand);
                    }

                    divProgram.AddCssClass(className);
                    divProgram.AddCssClass(innerClass);

                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonthPlannedReport(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonthPlannedReport(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonthPlannedReport(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
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
        public static MvcHtmlString ChildMonthAllocatedReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model, string AllocatedBy, int month, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
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
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = "";
                    double PlannedValue = 0;
                    double AllocatedValue = 0;
                    double ChildAllocatedValue = 0;
                    switch (month)
                    {
                        case 1:
                            PlannedValue = p.MonthPlanned.Jan;
                            AllocatedValue = p.MonthAllocated.Jan;
                            ChildAllocatedValue = p.ChildMonthAllocated.Jan;
                            break;
                        case 2:
                            PlannedValue = p.MonthPlanned.Feb;
                            AllocatedValue = p.MonthAllocated.Feb;
                            ChildAllocatedValue = p.ChildMonthAllocated.Feb;
                            break;
                        case 3:
                            PlannedValue = p.MonthPlanned.Mar;
                            AllocatedValue = p.MonthAllocated.Mar;
                            ChildAllocatedValue = p.ChildMonthAllocated.Mar;
                            break;
                        case 4:
                            PlannedValue = p.MonthPlanned.Apr;
                            AllocatedValue = p.MonthAllocated.Apr;
                            ChildAllocatedValue = p.ChildMonthAllocated.Apr;
                            break;
                        case 5:
                            PlannedValue = p.MonthPlanned.May;
                            AllocatedValue = p.MonthAllocated.May;
                            ChildAllocatedValue = p.ChildMonthAllocated.May;
                            break;
                        case 6:
                            PlannedValue = p.MonthPlanned.Jun;
                            AllocatedValue = p.MonthAllocated.Jun;
                            ChildAllocatedValue = p.ChildMonthAllocated.Jun;
                            break;
                        case 7:
                            PlannedValue = p.MonthPlanned.Jul;
                            AllocatedValue = p.MonthAllocated.Jul;
                            ChildAllocatedValue = p.ChildMonthAllocated.Jul;
                            break;
                        case 8:
                            PlannedValue = p.MonthPlanned.Aug;
                            AllocatedValue = p.MonthAllocated.Aug;
                            ChildAllocatedValue = p.ChildMonthAllocated.Aug;
                            break;
                        case 9:
                            PlannedValue = p.MonthPlanned.Sep;
                            AllocatedValue = p.MonthAllocated.Sep;
                            ChildAllocatedValue = p.ChildMonthAllocated.Sep;
                            break;
                        case 10:
                            PlannedValue = p.MonthPlanned.Oct;
                            AllocatedValue = p.MonthAllocated.Oct;
                            ChildAllocatedValue = p.ChildMonthAllocated.Oct;
                            break;
                        case 11:
                            PlannedValue = p.MonthPlanned.Nov;
                            AllocatedValue = p.MonthAllocated.Nov;
                            ChildAllocatedValue = p.ChildMonthAllocated.Nov;
                            break;
                        case 12:
                            PlannedValue = p.MonthPlanned.Dec;
                            AllocatedValue = p.MonthAllocated.Dec;
                            ChildAllocatedValue = p.ChildMonthAllocated.Dec;
                            break;
                        default:
                            break;
                    }


                    if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic && isPlanTab)
                    {
                        if (ActivityType != Helpers.ActivityType.ActivityProgram)
                        {
                            if (AllocatedValue < ChildAllocatedValue)
                            {
                                className += budgetError;
                                divProgram.Attributes.Add("Allocated", ChildAllocatedValue.ToString(formatThousand));
                            }
                            else if (AllocatedValue > ChildAllocatedValue)
                            {
                                divProgram.Attributes.Add("Remaining", (AllocatedValue - ChildAllocatedValue).ToString(formatThousand));
                            }
                        }

                        divProgram.InnerHtml = AllocatedValue.ToString(formatThousand);
                    }
                    else
                    {
                        divProgram.InnerHtml = "---";
                    }

                    divProgram.AddCssClass(className);
                    divProgram.AddCssClass(innerClass);
                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildMonthAllocatedReport(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildMonthAllocatedReport(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();

                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildMonthAllocatedReport(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, AllocatedBy, month, isPlanTab).ToString();
                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
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
        public static MvcHtmlString ParentSummaryReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model, string AllocatedBy, bool isPlanTab)
        {
            StringBuilder sb = new StringBuilder();
            BudgetModelReport plan = model.Where(pl => pl.ActivityType == Helpers.ActivityType.ActivityMain).SingleOrDefault();
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");

                //First Actual
                TagBuilder td = new TagBuilder("td");
                //td.AddCssClass("event-rowReport");
                TagBuilder div = new TagBuilder("div");

                double sumMonthActual = plan.MonthActual.Jan + plan.MonthActual.Feb + plan.MonthActual.Mar + plan.MonthActual.Apr + plan.MonthActual.May + plan.MonthActual.Jun + plan.MonthActual.Jul + plan.MonthActual.Aug + plan.MonthActual.Sep + plan.MonthActual.Oct + plan.MonthActual.Nov + plan.MonthActual.Dec;
                double sumMonthPlanned = plan.MonthPlanned.Jan + plan.MonthPlanned.Feb + plan.MonthPlanned.Mar + plan.MonthPlanned.Apr + plan.MonthPlanned.May + plan.MonthPlanned.Jun + plan.MonthPlanned.Jul + plan.MonthPlanned.Aug + plan.MonthPlanned.Sep + plan.MonthPlanned.Oct + plan.MonthPlanned.Nov + plan.MonthPlanned.Dec;
                double sumMonthAllocated = plan.ChildMonthAllocated.Jan + plan.ChildMonthAllocated.Feb + plan.ChildMonthAllocated.Mar + plan.ChildMonthAllocated.Apr + plan.ChildMonthAllocated.May + plan.ChildMonthAllocated.Jun + plan.ChildMonthAllocated.Jul + plan.ChildMonthAllocated.Aug + plan.ChildMonthAllocated.Sep + plan.ChildMonthAllocated.Oct + plan.ChildMonthAllocated.Nov + plan.ChildMonthAllocated.Dec;
                TagBuilder span = new TagBuilder("span");

                double dblProgress = 0;
                div.InnerHtml = sumMonthActual.ToString(formatThousand);
                if (isPlanTab)
                {
                    span = new TagBuilder("span");

                    dblProgress = 0;
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
                //  td.AddCssClass("event-rowReport");
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
                // td.AddCssClass("event-rowReport");
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
            foreach (BudgetModelReport c in model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList())
            {
                if (model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == c.ActivityId).Count() > 0)
                {

                    TagBuilder tr = new TagBuilder("tr");

                    //First Actual
                    TagBuilder td = new TagBuilder("td");
                    td.AddCssClass("event-rowReport");

                    TagBuilder div = new TagBuilder("div");
                    div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    div.AddCssClass("firstLevel");
                    double sumMonthActual = c.MonthActual.Jan + c.MonthActual.Feb + c.MonthActual.Mar + c.MonthActual.Apr + c.MonthActual.May + c.MonthActual.Jun + c.MonthActual.Jul + c.MonthActual.Aug + c.MonthActual.Sep + c.MonthActual.Oct + c.MonthActual.Nov + c.MonthActual.Dec;
                    double sumMonthPlanned = c.MonthPlanned.Jan + c.MonthPlanned.Feb + c.MonthPlanned.Mar + c.MonthPlanned.Apr + c.MonthPlanned.May + c.MonthPlanned.Jun + c.MonthPlanned.Jul + c.MonthPlanned.Aug + c.MonthPlanned.Sep + c.MonthPlanned.Oct + c.MonthPlanned.Nov + c.MonthPlanned.Dec;
                    double sumMonthAllocated = c.MonthAllocated.Jan + c.MonthAllocated.Feb + c.MonthAllocated.Mar + c.MonthAllocated.Apr + c.MonthAllocated.May + c.MonthAllocated.Jun + c.MonthAllocated.Jul + c.MonthAllocated.Aug + c.MonthAllocated.Sep + c.MonthAllocated.Oct + c.MonthAllocated.Nov + c.MonthAllocated.Dec;
                    double sumMonthChildAllocated = c.ChildMonthAllocated.Jan + c.ChildMonthAllocated.Feb + c.ChildMonthAllocated.Mar + c.ChildMonthAllocated.Apr + c.ChildMonthAllocated.May + c.ChildMonthAllocated.Jun + c.ChildMonthAllocated.Jul + c.ChildMonthAllocated.Aug + c.ChildMonthAllocated.Sep + c.ChildMonthAllocated.Oct + c.ChildMonthAllocated.Nov + c.ChildMonthAllocated.Dec;

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

                    td.InnerHtml += ChildSummaryReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "first", AllocatedBy, isPlanTab).ToString();

                    tr.InnerHtml += td.ToString();

                    // Second Planned
                    td = new TagBuilder("td");
                    td.AddCssClass("event-rowReport");

                    div = new TagBuilder("div");
                    div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
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

                    td.InnerHtml += ChildSummaryReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "Second", AllocatedBy, isPlanTab).ToString();

                    tr.InnerHtml += td.ToString();

                    //Third Allocated

                    td = new TagBuilder("td");
                    td.AddCssClass("event-rowReport");

                    div = new TagBuilder("div");
                    div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
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

                    td.InnerHtml += ChildSummaryReport(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "Third", AllocatedBy, isPlanTab).ToString();

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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MvcHtmlString ChildSummaryReport(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModelReport> model, string mode, string AllocatedBy, bool isPlanTab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "plan";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaign-row audience";
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
            List<BudgetModelReport> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModelReport p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    //divProgram.AddCssClass(innerClass);
                    double sumMonthAllocated = p.MonthAllocated.Jan + p.MonthAllocated.Feb + p.MonthAllocated.Mar + p.MonthAllocated.Apr + p.MonthAllocated.May + p.MonthAllocated.Jun + p.MonthAllocated.Jul + p.MonthAllocated.Aug + p.MonthAllocated.Sep + p.MonthAllocated.Oct + p.MonthAllocated.Nov + p.MonthAllocated.Dec;
                    if (mode == "first")
                    {
                        double sumMonthActual = p.MonthActual.Jan + p.MonthActual.Feb + p.MonthActual.Mar + p.MonthActual.Apr + p.MonthActual.May + p.MonthActual.Jun + p.MonthActual.Jul + p.MonthActual.Aug + p.MonthActual.Sep + p.MonthActual.Oct + p.MonthActual.Nov + p.MonthActual.Dec;
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
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
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

                            if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                            {
                                divProgram.InnerHtml += span.ToString();
                            }
                        }

                    }
                    else if (mode == "Second")
                    {
                        double sumMonthPlanned = p.MonthPlanned.Jan + p.MonthPlanned.Feb + p.MonthPlanned.Mar + p.MonthPlanned.Apr + p.MonthPlanned.May + p.MonthPlanned.Jun + p.MonthPlanned.Jul + p.MonthPlanned.Aug + p.MonthPlanned.Sep + p.MonthPlanned.Oct + p.MonthPlanned.Nov + p.MonthPlanned.Dec;
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
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
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


                            if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                            {
                                divProgram.InnerHtml += span.ToString();
                            }
                        }
                    }
                    else
                    {
                        if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic && isPlanTab)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityProgram)
                            {
                                double sumMonthChildAllocated = p.ChildMonthAllocated.Jan + p.ChildMonthAllocated.Feb + p.ChildMonthAllocated.Mar + p.ChildMonthAllocated.Apr + p.ChildMonthAllocated.May + p.ChildMonthAllocated.Jun + p.ChildMonthAllocated.Jul + p.ChildMonthAllocated.Aug + p.ChildMonthAllocated.Sep + p.ChildMonthAllocated.Oct + p.ChildMonthAllocated.Nov + p.ChildMonthAllocated.Dec;

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
                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildSummaryReport(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, mode, AllocatedBy, isPlanTab).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildSummaryReport(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, mode, AllocatedBy, isPlanTab).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildSummaryReport(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, mode, AllocatedBy, isPlanTab).ToString();

                }
                sb.AppendLine(div.ToString());
                return new MvcHtmlString(sb.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }

        }

        #endregion //Budgeting Report
    }
}