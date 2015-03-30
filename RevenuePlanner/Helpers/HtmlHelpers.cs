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
        /// <param name="MSize"></param>
        /// <returns>MvcHtmlString</returns>
        /// datatype of MSize,TSize and SSize is modified from int to double
        public static MvcHtmlString GenerateFunnel(this HtmlHelper helper, double MSize)
        {

            StringBuilder sb = new StringBuilder();
            StringBuilder sbHidden = new StringBuilder();

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

        #endregion

        #region Advance Budgeting

        static string formatThousand = "#,#0.##";
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
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList())
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
            //if (Tab == "0")
            //{
            //    needAccrodian = false;
            //}
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList();
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
                    //if (needAccrodian)
                    //{
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
                    //}
                    //else
                    //{
                    //    aLink.Attributes.Add("style", "cursor:pointer;");
                    //}

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
                        div.InnerHtml += ActivityProgram(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, Tab).ToString();
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
        public static MvcHtmlString PlanMonth(this HtmlHelper helper, string ActivityType, string ActivityId, BudgetMonth obj, BudgetMonth parent, BudgetMonth budgetMonth, string AllocatedBy, string strTab)
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
                    TagBuilder span = new TagBuilder("span");
                    divHeader.InnerHtml = dt.ToString("MMM").ToUpper();
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    string className = "event-row";
                    if (i == 1)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Jan.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jan.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Jan < obj.Jan)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Feb.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Feb.ToString(formatThousand));
                        divValue.InnerHtml = obj.Feb.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Feb < obj.Feb)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Feb <= parent.Feb ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Mar.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Mar.ToString(formatThousand));
                        divValue.InnerHtml = obj.Mar.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Mar < obj.Mar)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Mar <= parent.Mar ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Apr.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = obj.Apr.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Apr < obj.Apr)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 5)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.May.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.May.ToString(formatThousand));
                        divValue.InnerHtml = obj.May.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.May < obj.May)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.May <= parent.May ? className : className + budgetError;
                    }
                    else if (i == 6)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Jun.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Jun.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jun.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Jun < obj.Jun)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Jun <= parent.Jun ? className : className + budgetError;
                    }
                    else if (i == 7)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Jul.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jul.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Jul < obj.Jul)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 8)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Aug.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Aug.ToString(formatThousand));
                        divValue.InnerHtml = obj.Aug.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Aug < obj.Aug)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Aug <= parent.Aug ? className : className + budgetError;
                    }
                    else if (i == 9)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Sep.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Sep.ToString(formatThousand));
                        divValue.InnerHtml = obj.Sep.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Sep < obj.Sep)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Sep <= parent.Sep ? className : className + budgetError;
                    }
                    else if (i == 10)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Oct.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = obj.Oct.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Oct < obj.Oct)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
                    else if (i == 11)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Nov.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Nov.ToString(formatThousand));
                        divValue.InnerHtml = obj.Nov.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Nov < obj.Nov)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Nov <= parent.Nov ? className : className + budgetError;
                    }
                    else if (i == 12)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Dec.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Dec.ToString(formatThousand));
                        divValue.InnerHtml = obj.Dec.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Dec < obj.Dec)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Dec <= parent.Dec ? className : className + budgetError;
                    }
                    //if (className.Contains("budgetError"))
                    //{
                    //    className = className.Replace(budgetError, "");
                    //    divValue.AddCssClass("budgetError");
                    //}
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    divValue.InnerHtml += span.ToString();
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
                    TagBuilder span = new TagBuilder("span");
                    divHeader.InnerHtml = "Q" + i.ToString();
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    if (i == 1)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Jan.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jan.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Jan < obj.Jan)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Apr.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = obj.Apr.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Apr < obj.Apr)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Jul.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = obj.Jul.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Jul < obj.Jul)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        divValue.Attributes.Add("mainbudget", budgetMonth.Oct.ToString(formatThousand));
                        divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = obj.Oct.ToString(formatThousand);
                        if (strTab == "1" && budgetMonth.Oct < obj.Oct)
                        {
                            span.AddCssClass("red-corner-budget");
                        }
                        //className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
                    //if (className.Contains("budgetError"))
                    //{
                    //    className = className.Replace(budgetError, "");
                    //    divValue.AddCssClass("budgetError");
                    //}
                    tdValue.AddCssClass(className);
                    tdHeader.InnerHtml += divHeader.ToString();
                    trHeader.InnerHtml += tdHeader.ToString();

                    divValue.InnerHtml += span.ToString();
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
            bool isPlannedTab = ((int)Enums.BudgetTab.Planned).ToString() == strTab ? true : false;
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList())
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
                        TagBuilder span = new TagBuilder("span");
                        if (i == 1)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Jan.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jan.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.January.ToString(), c.Month.Jan.ToString(formatThousand)) :
                            //className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Jan < c.Month.Jan)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Feb.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Feb.ToString(formatThousand));
                            div.InnerHtml = c.Month.Feb.ToString(formatThousand);// isPlannedTab ? ClueTipAnchorTag(Enums.Months.February.ToString(), c.Month.Feb.ToString(formatThousand)) :
                            //className = c.Month.Feb <= c.ParentMonth.Feb ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Feb < c.Month.Feb)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Mar.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Mar.ToString(formatThousand));
                            div.InnerHtml = c.Month.Mar.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.March.ToString(), c.Month.Mar.ToString(formatThousand)) : 
                            //className = c.Month.Mar <= c.ParentMonth.Mar ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Mar < c.Month.Mar)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Apr.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.Month.Apr.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.April.ToString(), c.Month.Apr.ToString(formatThousand)) : 
                            //className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Apr < c.Month.Apr)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 5)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.May.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.May.ToString(formatThousand));
                            div.InnerHtml = c.Month.May.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.May.ToString(), c.Month.May.ToString(formatThousand)) :
                            //className = c.Month.May <= c.ParentMonth.May ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.May < c.Month.May)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 6)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Jun.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Jun.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jun.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.June.ToString(), c.Month.Jun.ToString(formatThousand)) :
                            //className = c.Month.Jun <= c.ParentMonth.Jun ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Jun < c.Month.Jun)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 7)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Jul.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jul.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.July.ToString(), c.Month.Jul.ToString(formatThousand)) : 
                            //className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Jul < c.Month.Jul)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 8)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Aug.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Aug.ToString(formatThousand));
                            div.InnerHtml = c.Month.Aug.ToString(formatThousand);// isPlannedTab ? ClueTipAnchorTag(Enums.Months.August.ToString(), c.Month.Aug.ToString(formatThousand)) : 
                            //className = c.Month.Aug <= c.ParentMonth.Aug ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Aug < c.Month.Aug)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 9)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Sep.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Sep.ToString(formatThousand));
                            div.InnerHtml = c.Month.Sep.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.September.ToString(), c.Month.Sep.ToString(formatThousand)) :
                            className = c.Month.Sep <= c.ParentMonth.Sep ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Sep < c.Month.Sep)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 10)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Oct.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.Month.Oct.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.October.ToString(), c.Month.Oct.ToString(formatThousand)) : 
                            //className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Oct < c.Month.Oct)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 11)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Nov.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Nov.ToString(formatThousand));
                            div.InnerHtml = c.Month.Nov.ToString(formatThousand);//isPlannedTab ? ClueTipAnchorTag(Enums.Months.November.ToString(), c.Month.Nov.ToString(formatThousand)) :
                            //className = c.Month.Nov <= c.ParentMonth.Nov ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Nov < c.Month.Nov)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 12)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Dec.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Dec.ToString(formatThousand));
                            div.InnerHtml = c.Month.Dec.ToString(formatThousand);// isPlannedTab ? ClueTipAnchorTag(Enums.Months.December.ToString(), c.Month.Dec.ToString(formatThousand)) :
                            //className = c.Month.Dec <= c.ParentMonth.Dec ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Dec < c.Month.Dec)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        div.AddCssClass(className);
                        div.InnerHtml += span.ToString();
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
                        TagBuilder span = new TagBuilder("span");
                        if (i == 1)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Jan.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                            //className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Jan < c.Month.Jan)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Apr.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                            //className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Apr < c.Month.Apr)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Jul.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                            //className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Jul < c.Month.Jul)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("mainbudget", c.BudgetMonth.Oct.ToString(formatThousand));
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                            //className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                            if (strTab == "1" && c.BudgetMonth.Oct < c.Month.Oct)
                            {
                                span.AddCssClass("red-corner-budget");
                            }
                        }
                        div.AddCssClass(className);
                        div.InnerHtml += span.ToString();
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
            bool isPlannedTab = ((int)Enums.BudgetTab.Planned).ToString() == strTab ? true : false;
            bool isTactic = ActivityType == Helpers.ActivityType.ActivityTactic ? true : false;
            bool isLineItem = ActivityType == Helpers.ActivityType.ActivityLineItem ? true : false;
            if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityTactic)
            {
                mainClass = "sub tactic-lvl";
                innerClass = isPlannedTab ? "tacticLevel clueplanned" : "tacticLevel clueactual";
                parentClassName = "program";
            }
            else if (ActivityType == Helpers.ActivityType.ActivityLineItem)
            {
                mainClass = "sub lineItem-lvl";
                innerClass = isPlannedTab ? "lineitemLevel clueplanned" : "lineitemLevel clueactual";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    if (!isPlannedTab && ActivityType == Helpers.ActivityType.ActivityTactic)
                    {
                        if (model.Where(m => m.ActivityType == Helpers.ActivityType.ActivityLineItem && m.ParentActivityId == p.ActivityId).ToList().Count == 0)
                        {
                            isLineItem = true;
                        }
                    }
                    TagBuilder divProgram = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = innerClass;
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jan.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.January.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Jan < p.Month.Jan && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 2)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Feb.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Feb <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.February.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Feb.ToString(formatThousand));
                                    //className = p.Month.Feb <= p.ParentMonth.Feb ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.Feb.ToString(formatThousand)) : p.Month.Feb.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Feb < p.Month.Feb && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 3)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Mar.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Mar <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.March.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Mar.ToString(formatThousand));
                                    //className = p.Month.Mar <= p.ParentMonth.Mar ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.Mar.ToString(formatThousand)) : p.Month.Mar.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Mar < p.Month.Mar && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 4)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Apr.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.April.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Apr < p.Month.Apr && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 5)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.May.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.May <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.May.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.May.ToString(formatThousand));
                                    //className = p.Month.May <= p.ParentMonth.May ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.May.ToString(formatThousand)) : p.Month.May.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.May < p.Month.May && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 6)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jun.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jun <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.June.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jun.ToString(formatThousand));
                                    //className = p.Month.Jun <= p.ParentMonth.Jun ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.Jun.ToString(formatThousand)) : p.Month.Jun.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Jun < p.Month.Jun && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 7)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jul.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.July.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Jul < p.Month.Jul && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 8)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Aug.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Aug <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.August.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Aug.ToString(formatThousand));
                                    //className = p.Month.Aug <= p.ParentMonth.Aug ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.Aug.ToString(formatThousand)) : p.Month.Aug.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Aug < p.Month.Aug && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 9)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Sep.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Sep <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.September.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Sep.ToString(formatThousand));
                                    //className = p.Month.Sep <= p.ParentMonth.Sep ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.Sep.ToString(formatThousand)) : p.Month.Sep.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Sep < p.Month.Sep && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }


                        }
                        else if (month == 10)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Oct.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.October.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Oct < p.Month.Oct && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 11)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Nov.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Nov <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.November.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Nov.ToString(formatThousand));
                                    //className = p.Month.Nov <= p.ParentMonth.Nov ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.Nov.ToString(formatThousand)) : p.Month.Nov.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Nov < p.Month.Nov && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }
                        }
                        else if (month == 12)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Dec.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Dec <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.December.ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Dec.ToString(formatThousand));
                                    //className = p.Month.Dec <= p.ParentMonth.Dec ? className : className + budgetError;
                                }

                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.Dec.ToString(formatThousand)) : p.Month.Dec.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Dec < p.Month.Dec && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }
                        }
                    }
                    else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jan.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                                    //className = p.Month.Jan <= p.ParentMonth.Jan ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Jan < p.Month.Jan && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 2)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Apr.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                                    //className = p.Month.Apr <= p.ParentMonth.Apr ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Apr < p.Month.Apr && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 3)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jul.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                                    //className = p.Month.Jul <= p.ParentMonth.Jul ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Jul < p.Month.Jul && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
                                }
                            }

                        }
                        else if (month == 4)
                        {
                            if (ActivityType != Helpers.ActivityType.ActivityLineItem)
                            {
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Oct.ToString(formatThousand));
                            }
                            if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                            {
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), "---") : "---";
                            }
                            else
                            {
                                if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                                {
                                    divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                                    //className = p.Month.Oct <= p.ParentMonth.Oct ? className : className + budgetError;
                                }
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString(formatThousand);
                                if (strTab == "1" && p.BudgetMonth.Oct < p.Month.Oct && !isLineItem)
                                {
                                    span.AddCssClass("red-corner-budget");
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
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jan <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.January.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
                                }

                            }
                            else if (month == 2)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Feb <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.February.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.Feb.ToString(formatThousand)) : p.Month.Feb.ToString(formatThousand);
                                }

                            }
                            else if (month == 3)
                            {

                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Mar <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.March.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.Mar.ToString(formatThousand)) : p.Month.Mar.ToString(formatThousand);
                                }

                            }
                            else if (month == 4)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.April.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
                                }

                            }
                            else if (month == 5)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.May <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.May.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.May.ToString(formatThousand)) : p.Month.May.ToString(formatThousand);
                                }

                            }
                            else if (month == 6)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jun <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.June.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.Jun.ToString(formatThousand)) : p.Month.Jun.ToString(formatThousand);
                                }

                            }
                            else if (month == 7)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.July.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
                                }

                            }
                            else if (month == 8)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Aug <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.August.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.Aug.ToString(formatThousand)) : p.Month.Aug.ToString(formatThousand);
                                }

                            }
                            else if (month == 9)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Sep <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.September.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.Sep.ToString(formatThousand)) : p.Month.Sep.ToString(formatThousand);
                                }


                            }
                            else if (month == 10)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.October.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString();
                                }

                            }
                            else if (month == 11)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Nov <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.November.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.Nov.ToString(formatThousand)) : p.Month.Nov.ToString(formatThousand);
                                }
                            }
                            else if (month == 12)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Dec <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.December.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && isLineItem && p.isAfterApproved ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.Dec.ToString(formatThousand)) : p.Month.Dec.ToString(formatThousand);
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
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="Tab"></param>
        /// <returns></returns>
        public static MvcHtmlString CampaignBudgetSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string Tab)
        {
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.Where(pl => pl.ActivityType == Helpers.ActivityType.ActivityPlan).SingleOrDefault();
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", plan.ActivityType + ParentActivityId.ToString());
                td.AddCssClass("event-row");
                TagBuilder span = new TagBuilder("span");
                if (Tab == "0")
                {
                    div.InnerHtml = plan.isEditable ? ClueTipAnchorTag(string.Empty, plan.Allocated.ToString(formatThousand)) : plan.Allocated.ToString(formatThousand);
                    div.AddCssClass("planLevel");
                    div.AddCssClass("clueallocatedbudget");
                }
                else
                {
                    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    {
                        if (Tab == "2")
                        {
                            double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                            div.InnerHtml = sumMonth.ToString(formatThousand);
                        }
                        else
                        {
                            div.InnerHtml = plan.Allocated.ToString(formatThousand);
                        }
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
                }
                if (Tab == "0")
                {
                    var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).Select(p => p.Allocated).Sum();
                    if (childTotalAllocated > plan.Allocated)
                    {
                        span.Attributes.Add("class", "orange-corner-budget");
                    }
                }
                else if (Tab == "1" && AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    div.Attributes.Add("mainbudget", plan.MainBudgeted.ToString());
                    if (plan.Allocated > plan.MainBudgeted)
                    {
                        span.Attributes.Add("class", "red-corner-budget");
                    }
                }
                div.InnerHtml += span.ToString();
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");

                TagBuilder tdLast = new TagBuilder("td");
                tdLast.AddCssClass("campaign-row");
                TagBuilder span = new TagBuilder("span");
                TagBuilder divLast = new TagBuilder("div");
                divLast.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                divLast.AddCssClass("campaignLevel");
                if (Tab == "0")
                {
                    divLast.InnerHtml = c.isEditable ? ClueTipAnchorTag(string.Empty, c.Allocated.ToString(formatThousand)) : c.Allocated.ToString(formatThousand);
                    divLast.AddCssClass("clueallocatedbudget");
                }
                else
                {
                    if (AllocatedBy != "default")
                    {
                        if (Tab == "2")
                        {
                            double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                            divLast.InnerHtml = sumMonth.ToString(formatThousand);
                        }
                        else
                        {
                            divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                        }
                    }
                    else
                    {
                        divLast.AddCssClass("firstLevel");
                        if (Tab == "2")
                        {
                            double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                            divLast.InnerHtml = sumMonth.ToString(formatThousand);
                        }
                        else
                        {
                            divLast.InnerHtml = "---";
                        }
                    }
                }
                if (Tab == "0")
                {
                    var childTotlaAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Allocated).Sum();
                    if (childTotlaAllocated > c.Allocated)
                    {
                        span.Attributes.Add("class", "orange-corner-budget");
                    }
                }
                else if (Tab == "1" && AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    divLast.Attributes.Add("mainbudget", c.MainBudgeted.ToString());
                    if (c.Allocated > c.MainBudgeted)
                    {
                        span.Attributes.Add("class", "red-corner-budget");
                    }
                }
                divLast.InnerHtml += span.ToString();
                tdLast.InnerHtml = divLast.ToString();
                tdLast.InnerHtml += ProgramBudgetSummary(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, "last", AllocatedBy, Tab).ToString();

                tr.InnerHtml += tdLast.ToString();

                sb.AppendLine(tr.ToString());
            }
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="ActivityType"></param>
        /// <param name="ParentActivityId"></param>
        /// <param name="model"></param>
        /// <param name="mode"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="Tab"></param>
        /// <returns></returns>
        public static MvcHtmlString ProgramBudgetSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string mode, string AllocatedBy, string Tab)
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            bool isProgram = false;
            bool isOtherLineItem = false;
            if (ActivityType == Helpers.ActivityType.ActivityProgram)
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel";
                parentClassName = "campaign";
                isProgram = true;
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
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");

                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    if (p.ActivityType == Helpers.ActivityType.ActivityLineItem && p.ActivityName == Common.DefaultLineItemTitle)
                    {
                        isOtherLineItem = true;
                    }
                    TagBuilder span = new TagBuilder("span");
                    TagBuilder divProgram = new TagBuilder("div");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    if (Tab == "0")
                    {
                        if (ActivityType == Helpers.ActivityType.ActivityLineItem)//|| ActivityType == Helpers.ActivityType.ActivityTactic
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
                        if (AllocatedBy != "default")
                        {
                            if (Tab == "2")
                            {
                                double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                                divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            }
                            else
                            {
                                divProgram.InnerHtml +=p.isEditable && !isOtherLineItem && !isProgram ? ClueTipAnchorTag(string.Empty, p.Allocated.ToString(formatThousand)): p.Allocated.ToString(formatThousand) ;
                                divProgram.AddCssClass("clueallocatedCost");
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
                        }
                    }
                    if (Tab == "0")
                    {
                        var childTotalAllocated = 0.0;
                        if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                        {
                            childTotalAllocated = model.Where(c => c.ActivityType == Helpers.ActivityType.ActivityTactic.ToString() && c.ParentActivityId == p.ActivityId).Select(c => c.Allocated).Sum();
                        }
                        else if (p.ActivityType == Helpers.ActivityType.ActivityTactic.ToString())
                        {
                            childTotalAllocated = model.Where(c => c.ActivityType == Helpers.ActivityType.ActivityLineItem.ToString() && c.ParentActivityId == p.ActivityId).Select(c => c.Allocated).Sum();
                        }
                        if (childTotalAllocated > p.Allocated)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                    }
                    else if (Tab == "1" && AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    {
                        divProgram.Attributes.Add("mainbudget", p.MainBudgeted.ToString());
                        if (p.Allocated > p.MainBudgeted && ActivityType != Helpers.ActivityType.ActivityLineItem.ToString())
                        {
                            span.Attributes.Add("class", "red-corner-budget");
                        }
                    }
                    //divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                    divProgram.AddCssClass(innerClass);
                    divProgram.InnerHtml += span.ToString();
                    div.InnerHtml += divProgram.ToString();

                    if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ProgramBudgetSummary(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, mode, AllocatedBy, Tab).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ProgramBudgetSummary(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, mode, AllocatedBy, Tab).ToString();
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
                TagBuilder span = new TagBuilder("span");
                div.Attributes.Add("id", plan.ActivityType + ParentActivityId.ToString());
                if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    double unallocated = 0;
                    double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                    if (Tab == "0")
                    {

                        unallocated = plan.Allocated - sumMonth;
                        if (unallocated > 0)
                        {
                            //div.AddCssClass("campaignLevel budgetError");
                            span.AddCssClass("blue-corner-budget");
                        }
                        else if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    else
                    {
                        unallocated = Tab == "2" ? plan.MainBudgeted - sumMonth : plan.MainBudgeted - plan.Allocated;
                        if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    div.InnerHtml = unallocated.ToString(formatThousand);
                    //TagBuilder span = new TagBuilder("span");

                    //double dblProgress = 0;
                    //dblProgress = (sumMonth == 0 && plan.Allocated == 0) ? 0 : (sumMonth > 0 && plan.Allocated == 0) ? 101 : sumMonth / plan.Allocated * 100;
                    //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    //if (dblProgress > 100)
                    //{
                    //    div.AddCssClass("budgetError");
                    //    span.AddCssClass("progressBar budgetError");
                    //}
                    //else
                    //{
                    //    span.AddCssClass("progressBar");
                    //}
                    //div.InnerHtml += span.ToString();
                }
                else
                {
                    div.AddCssClass("firstLevel");
                    if (Tab == "2")
                    {
                        double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
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


                //td = new TagBuilder("td");
                //td.AddCssClass("event-row");
                //div = new TagBuilder("div");
                //if (Tab == "0")
                //{
                //    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                //}
                //else
                //{
                //    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                //    {
                //        div.InnerHtml = plan.Allocated.ToString(formatThousand);
                //    }
                //    else
                //    {
                //        div.AddCssClass("firstLevel");
                //        div.InnerHtml = "---";
                //    }
                //}
                //td.InnerHtml = div.ToString();
                //tr.InnerHtml += td.ToString();

                sb.AppendLine(tr.ToString());
            }
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");

                //First
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("campaign-row");
                TagBuilder span = new TagBuilder("span");
                TagBuilder div = new TagBuilder("div");
                div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());

                if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    //div.InnerHtml = c.Budgeted.ToString();
                    double unallocated = 0;
                    div.AddCssClass("campaignLevel");
                    double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                    if (Tab == "0")
                    {
                        unallocated = c.Allocated - sumMonth;
                        if (unallocated > 0)
                        {
                            //div.AddCssClass("campaignLevel budgetError");
                            span.AddCssClass("blue-corner-budget");
                        }
                        else if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    else
                    {
                        unallocated = Tab == "2" ? c.MainBudgeted - sumMonth : c.MainBudgeted - c.Allocated;
                        if (unallocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
                    }
                    //else
                    //{
                    //    div.AddCssClass("campaignLevel");
                    //}
                    //TagBuilder span = new TagBuilder("span");

                    //double dblProgress = 0;
                    //dblProgress = (sumMonth == 0 && c.Allocated == 0) ? 0 : (sumMonth > 0 && c.Allocated == 0) ? 101 : sumMonth / c.Allocated * 100;
                    //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    //if (dblProgress > 100)
                    //{
                    //    div.AddCssClass("campaignLevel budgetError");
                    //    span.AddCssClass("progressBar budgetError");
                    //}
                    //else
                    //{
                    //    div.AddCssClass("campaignLevel");
                    //    span.AddCssClass("progressBar");
                    //}
                    div.InnerHtml += unallocated.ToString(formatThousand);
                    //div.InnerHtml += span.ToString();
                }
                else
                {
                    if (Tab == "2")
                    {
                        double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
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

                td.InnerHtml += ProgramSummary(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, "first", AllocatedBy, Tab).ToString();

                tr.InnerHtml += td.ToString();

                //Last
                //TagBuilder tdLast = new TagBuilder("td");
                //tdLast.AddCssClass("campaign-row");

                //TagBuilder divLast = new TagBuilder("div");
                //divLast.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                //divLast.AddCssClass("campaignLevel");
                //if (Tab == "0")
                //{
                //    divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                //}
                //else
                //{
                //    if (AllocatedBy != "default")
                //    {
                //        divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                //    }
                //    else
                //    {
                //        divLast.AddCssClass("firstLevel");
                //        divLast.InnerHtml = "---";
                //    }
                //}
                //tdLast.InnerHtml = divLast.ToString();
                //tdLast.InnerHtml += ProgramSummary(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, "last", AllocatedBy, Tab).ToString();

                //tr.InnerHtml += tdLast.ToString();

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
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {
                    TagBuilder divProgram = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    //divProgram.AddCssClass(innerClass);

                    if (mode == "first")
                    {
                        if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower() && (ActivityType != Helpers.ActivityType.ActivityLineItem))
                        {
                            double unAllocated = 0;
                            divProgram.AddCssClass(innerClass);
                            double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                            if (Tab == "0")
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
                                unAllocated = Tab == "2" ? p.MainBudgeted - sumMonth : p.MainBudgeted - p.Allocated;
                                if (unAllocated < 0)
                                {
                                    divProgram.AddCssClass("budgetError");
                                }
                            }
                            //else
                            //{
                            //    divProgram.AddCssClass(innerClass);
                            //}
                            //TagBuilder span = new TagBuilder("span");
                            //double dblProgress = 0;
                            //dblProgress = (sumMonth == 0 && p.Allocated == 0) ? 0 : (sumMonth > 0 && p.Allocated == 0) ? 101 : sumMonth / p.Allocated * 100;
                            //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                            //if (dblProgress > 100)
                            //{
                            //    if (ActivityType != "lineitem" && ActivityType != "tactic")
                            //    {
                            //        divProgram.AddCssClass(innerClass + budgetError);
                            //        span.AddCssClass("progressBar budgetError");
                            //    }
                            //    else
                            //    {
                            //        divProgram.AddCssClass(innerClass);
                            //        span.AddCssClass("progressBar");
                            //    }
                            //}
                            //else
                            //{
                            //    divProgram.AddCssClass(innerClass);
                            //    span.AddCssClass("progressBar");
                            //}
                            divProgram.InnerHtml = unAllocated.ToString(formatThousand);
                            //if (ActivityType != Helpers.ActivityType.ActivityLineItem && ActivityType != Helpers.ActivityType.ActivityTactic)
                            //{
                            //    divProgram.InnerHtml += span.ToString();
                            //}
                        }
                        else
                        {
                            if (Tab == "2" && ActivityType != Helpers.ActivityType.ActivityLineItem.ToString())
                            {
                                double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
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
                    //else
                    //{
                    //    if (Tab == "0")
                    //    {
                    //        if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
                    //        {
                    //            divProgram.InnerHtml += "---";
                    //        }
                    //        else
                    //        {
                    //divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (AllocatedBy != "default")
                    //        {
                    //            if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
                    //            {
                    //                divProgram.InnerHtml += "---";
                    //            }
                    //            else
                    //            {
                    //                divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            divProgram.InnerHtml += "---";
                    //        }
                    //    }
                    //    //divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                    //    divProgram.AddCssClass(innerClass);
                    //}
                    divProgram.InnerHtml += span.ToString();
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
        public static MvcHtmlString AllocatedPlanMonth(this HtmlHelper helper, string ActivityType, string ActivityId, BudgetMonth obj, BudgetMonth parent, string AllocatedBy, List<BudgetModel> model)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder trHeader = new TagBuilder("tr");
            TagBuilder trValue = new TagBuilder("tr");
            bool isEditable = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityPlan.ToString()).Select(p => p.isEditable).FirstOrDefault();
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
                    divValue.Attributes.Add("class", "planLevel clueallocated");
                    string className = "event-row";
                    TagBuilder span = new TagBuilder("span");
                    if (i == 1)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));

                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.January.ToString(), obj.Jan.ToString(formatThousand)) : obj.Jan.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Jan).ToList().Sum();
                        if (totalChildBudget > obj.Jan)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        //divValue.Attributes.Add("allocated", parent.Feb.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.February.ToString(), obj.Feb.ToString(formatThousand)) : obj.Feb.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Feb).ToList().Sum();
                        if (totalChildBudget > obj.Feb)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Feb <= parent.Feb ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        //divValue.Attributes.Add("allocated", parent.Mar.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.March.ToString(), obj.Mar.ToString(formatThousand)) : obj.Mar.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Mar).ToList().Sum();
                        if (totalChildBudget > obj.Mar)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Mar <= parent.Mar ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        //divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.April.ToString(), obj.Apr.ToString(formatThousand)) : obj.Apr.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Apr).ToList().Sum();
                        if (totalChildBudget > obj.Apr)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 5)
                    {
                        //divValue.Attributes.Add("allocated", parent.May.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.May.ToString(), obj.May.ToString(formatThousand)) : obj.May.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.May).ToList().Sum();
                        if (totalChildBudget > obj.May)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.May <= parent.May ? className : className + budgetError;
                    }
                    else if (i == 6)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jun.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.June.ToString(), obj.Jun.ToString(formatThousand)) : obj.Jun.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Jun).ToList().Sum();
                        if (totalChildBudget > obj.Jun)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Jun <= parent.Jun ? className : className + budgetError;
                    }
                    else if (i == 7)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.July.ToString(), obj.Jul.ToString(formatThousand)) : obj.Jul.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Jul).ToList().Sum();
                        if (totalChildBudget > obj.Jul)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 8)
                    {
                        //divValue.Attributes.Add("allocated", parent.Aug.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.August.ToString(), obj.Aug.ToString(formatThousand)) : obj.Aug.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Aug).ToList().Sum();
                        if (totalChildBudget > obj.Aug)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Aug <= parent.Aug ? className : className + budgetError;
                    }
                    else if (i == 9)
                    {
                        //divValue.Attributes.Add("allocated", parent.Sep.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.September.ToString(), obj.Sep.ToString(formatThousand)) : obj.Sep.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Sep).ToList().Sum();
                        if (totalChildBudget > obj.Sep)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Sep <= parent.Sep ? className : className + budgetError;
                    }
                    else if (i == 10)
                    {
                        //divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.October.ToString(), obj.Oct.ToString(formatThousand)) : obj.Oct.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Oct).ToList().Sum();
                        if (totalChildBudget > obj.Oct)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
                    else if (i == 11)
                    {
                        //divValue.Attributes.Add("allocated", parent.Nov.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.November.ToString(), obj.Nov.ToString(formatThousand)) : obj.Nov.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Nov).ToList().Sum();
                        if (totalChildBudget > obj.Nov)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Nov <= parent.Nov ? className : className + budgetError;
                    }
                    else if (i == 12)
                    {
                        //divValue.Attributes.Add("allocated", parent.Dec.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Months.December.ToString(), obj.Dec.ToString(formatThousand)) : obj.Dec.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Dec).ToList().Sum();
                        if (totalChildBudget > obj.Dec)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Dec <= parent.Dec ? className : className + budgetError;
                    }
                    //if (className.Contains("budgetError"))
                    //{
                    //    className = className.Replace("budgetError", "");
                    //    divValue.AddCssClass("budgetError");
                    //}
                    divValue.InnerHtml += span.ToString();
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
                    divValue.Attributes.Add("id", ActivityType + ActivityId.ToString());
                    divValue.Attributes.Add("class", "planLevel clueallocated");
                    divHeader.InnerHtml = "Q" + i.ToString();
                    TagBuilder span = new TagBuilder("span");
                    if (i == 1)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jan.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), obj.Jan.ToString(formatThousand)) : obj.Jan.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Jan).ToList().Sum();
                        if (totalChildBudget > obj.Jan)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Jan <= parent.Jan ? className : className + budgetError;
                    }
                    else if (i == 2)
                    {
                        //divValue.Attributes.Add("allocated", parent.Apr.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), obj.Apr.ToString(formatThousand)) : obj.Apr.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Apr).ToList().Sum();
                        if (totalChildBudget > obj.Apr)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Apr <= parent.Apr ? className : className + budgetError;
                    }
                    else if (i == 3)
                    {
                        //divValue.Attributes.Add("allocated", parent.Jul.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), obj.Jul.ToString(formatThousand)) : obj.Jul.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Jul).ToList().Sum();
                        if (totalChildBudget > obj.Jul)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Jul <= parent.Jul ? className : className + budgetError;
                    }
                    else if (i == 4)
                    {
                        //divValue.Attributes.Add("allocated", parent.Oct.ToString(formatThousand));
                        divValue.InnerHtml = isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), obj.Oct.ToString(formatThousand)) : obj.Oct.ToString(formatThousand);
                        var totalChildBudget = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ActivityId).Select(c => c.Month.Oct).ToList().Sum();
                        if (totalChildBudget > obj.Oct)
                        {
                            span.Attributes.Add("class", "orange-corner-budget");
                        }
                        //className = obj.Oct <= parent.Oct ? className : className + budgetError;
                    }
                    //if (className.Contains("budgetError"))
                    //{
                    //    className = className.Replace(budgetError, "");
                    //    divValue.AddCssClass("budgetError");
                    //}
                    divValue.InnerHtml += span.ToString();
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
            foreach (BudgetModel c in model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityCampaign && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList())
            {
                TagBuilder tr = new TagBuilder("tr");
                if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        string className = "campaignLevel clueallocated";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                        TagBuilder span = new TagBuilder("span");
                        if (i == 1)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.January.ToString(), c.Month.Jan.ToString(formatThousand)) : c.Month.Jan.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Jan).Sum();
                            if (c.Month.Jan < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Jan >= 0 ? className : className + budgetError;
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Feb.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.February.ToString(), c.Month.Feb.ToString(formatThousand)) : c.Month.Feb.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Feb).Sum();
                            if (c.Month.Feb < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Feb >= 0 ? className : className + budgetError;
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Mar.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.March.ToString(), c.Month.Mar.ToString(formatThousand)) : c.Month.Mar.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Mar).Sum();
                            if (c.Month.Mar < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Mar >= 0 ? className : className + budgetError;
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.April.ToString(), c.Month.Apr.ToString(formatThousand)) : c.Month.Apr.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Apr).Sum();
                            if (c.Month.Apr < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Apr >= 0 ? className : className + budgetError;
                        }
                        else if (i == 5)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.May.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.May.ToString(), c.Month.May.ToString(formatThousand)) : c.Month.May.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.May).Sum();
                            if (c.Month.May < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.May >= 0 ? className : className + budgetError;
                        }
                        else if (i == 6)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jun.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.June.ToString(), c.Month.Jun.ToString(formatThousand)) : c.Month.Jun.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Jun).Sum();
                            if (c.Month.Jun < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Jun >= 0 ? className : className + budgetError;
                        }
                        else if (i == 7)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.July.ToString(), c.Month.Jul.ToString(formatThousand)) : c.Month.Jul.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Jul).Sum();
                            if (c.Month.Jul < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Jul >= 0 ? className : className + budgetError;
                        }
                        else if (i == 8)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Aug.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.August.ToString(), c.Month.Aug.ToString(formatThousand)) : c.Month.Aug.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Aug).Sum();
                            if (c.Month.Aug < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Aug >= 0 ? className : className + budgetError;
                        }
                        else if (i == 9)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Sep.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.September.ToString(), c.Month.Sep.ToString(formatThousand)) : c.Month.Sep.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Sep).Sum();
                            if (c.Month.Sep < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Sep >= 0 ? className : className + budgetError;
                        }
                        else if (i == 10)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.October.ToString(), c.Month.Oct.ToString(formatThousand)) : c.Month.Oct.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Oct).Sum();
                            if (c.Month.Oct < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Oct >= 0 ? className : className + budgetError;
                        }
                        else if (i == 11)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Nov.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.November.ToString(), c.Month.Nov.ToString(formatThousand)) : c.Month.Nov.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Nov).Sum();
                            if (c.Month.Nov < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Nov >= 0 ? className : className + budgetError;
                        }
                        else if (i == 12)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Dec.ToString(formatThousand));

                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Months.December.ToString(), c.Month.Dec.ToString(formatThousand)) : c.Month.Dec.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Dec).Sum();
                            if (c.Month.Dec < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                            //className = c.ParentMonth.Dec >= 0 ? className : className + budgetError;
                        }
                        div.AddCssClass(className);
                        div.InnerHtml += span.ToString();
                        td.InnerHtml = div.ToString();

                        td.InnerHtml += AllocatedProgramMonth(helper, Helpers.ActivityType.ActivityProgram, c.ActivityId, model, AllocatedBy, i).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                }
                else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        string className = "campaignLevel clueallocated";
                        TagBuilder td = new TagBuilder("td");
                        td.AddCssClass("campaign-row");

                        TagBuilder div = new TagBuilder("div");
                        div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                        TagBuilder span = new TagBuilder("span");
                        if (i == 1)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jan.ToString(formatThousand));
                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), c.Month.Jan.ToString(formatThousand)) : c.Month.Jan.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Jan).Sum();
                            //className = c.ParentMonth.Jan >= 0 ? className : className + budgetError;
                            if (c.Month.Jan < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                        }
                        else if (i == 2)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), c.Month.Apr.ToString(formatThousand)) : c.Month.Apr.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Apr).Sum();
                            //className = c.ParentMonth.Apr >= 0 ? className : className + budgetError;
                            if (c.Month.Apr < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                        }
                        else if (i == 3)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), c.Month.Jul.ToString(formatThousand)) : c.Month.Jul.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Jul).Sum();
                            //className = c.ParentMonth.Jul >= 0 ? className : className + budgetError;
                            if (c.Month.Jul < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                        }
                        else if (i == 4)
                        {
                            div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                            div.InnerHtml = c.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), c.Month.Oct.ToString(formatThousand)) : c.Month.Oct.ToString(formatThousand);
                            var childTotalAllocated = model.Where(p => p.ActivityType == Helpers.ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).Select(p => p.Month.Oct).Sum();
                            //className = c.ParentMonth.Oct >= 0 ? className : className + budgetError;
                            if (c.Month.Oct < childTotalAllocated)
                            {
                                span.Attributes.Add("class", "orange-corner-budget");
                            }
                        }
                        div.AddCssClass(className);
                        div.InnerHtml += span.ToString();
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
            string innerClass = "programLevel clueallocated";
            string parentClassName = "campaign";
            if (ActivityType == "program")
            {
                mainClass = "sub program-lvl";
                innerClass = "programLevel clueallocated";
                parentClassName = "campaign";
            }
            else if (ActivityType == "tactic")
            {
                mainClass = "sub tactic-lvl";
                innerClass = "tacticLevel clueallocated";
                parentClassName = "program";
            }
            else if (ActivityType == "lineitem")
            {
                mainClass = "sub lineitem-lvl";
                innerClass = "lineitemLevel clueallocated";
                parentClassName = "tactic";
            }
            List<BudgetModel> lst = model.Where(p => p.ActivityType == ActivityType && p.ParentActivityId == ParentActivityId).OrderBy(p => p.ActivityName).ToList();
            if (lst.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(mainClass);
                div.Attributes.Add("data-parent", parentClassName + ParentActivityId.ToString());
                foreach (BudgetModel p in lst)
                {

                    TagBuilder divProgram = new TagBuilder("div");
                    TagBuilder span = new TagBuilder("span");
                    divProgram.Attributes.Add("id", ActivityType + p.ActivityId.ToString());
                    string className = innerClass;
                    if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Jan).Sum();
                                if (childTotal > p.Month.Jan)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            //className = p.ParentMonth.Jan >= 0 ? className : className + budgetError;

                        }
                        else if (month == 2)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Feb.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.Feb.ToString(formatThousand)) : p.Month.Feb.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Feb).Sum();
                                if (childTotal > p.Month.Feb)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            //className = p.ParentMonth.Feb >= 0 ? className : className + budgetError;
                        }
                        else if (month == 3)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Mar.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.Mar.ToString(formatThousand)) : p.Month.Mar.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Mar).Sum();
                                if (childTotal > p.Month.Mar)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Mar >= 0 ? className : className + budgetError;
                        }
                        else if (month == 4)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Apr).Sum();
                                if (childTotal > p.Month.Apr)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            //className = p.ParentMonth.Apr >= 0 ? className : className + budgetError;
                        }
                        else if (month == 5)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.May.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.May.ToString(formatThousand)) : p.Month.May.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.May).Sum();
                                if (childTotal > p.Month.May)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.May >= 0 ? className : className + budgetError;
                        }
                        else if (month == 6)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jun.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.Jun.ToString(formatThousand)) : p.Month.Jun.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Jun).Sum();
                                if (childTotal > p.Month.Jun)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Jun >= 0 ? className : className + budgetError;
                        }
                        else if (month == 7)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Jul).Sum();
                                if (childTotal > p.Month.Jul)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Jul >= 0 ? className : className + budgetError;
                        }
                        else if (month == 8)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Aug.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.Aug.ToString(formatThousand)) : p.Month.Aug.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Aug).Sum();
                                if (childTotal > p.Month.Aug)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Aug >= 0 ? className : className + budgetError;
                        }
                        else if (month == 9)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Sep.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.Sep.ToString(formatThousand)) : p.Month.Sep.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Sep).Sum();
                                if (childTotal > p.Month.Sep)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Sep >= 0 ? className : className + budgetError;
                        }
                        else if (month == 10)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Oct).Sum();
                                if (childTotal > p.Month.Oct)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            //className = p.ParentMonth.Oct >= 0 ? className : className + budgetError;
                        }
                        else if (month == 11)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Nov.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.Nov.ToString(formatThousand)) : p.Month.Nov.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Nov).Sum();
                                if (childTotal > p.Month.Nov)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Nov >= 0 ? className : className + budgetError;
                        }
                        else if (month == 12)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Dec.ToString(formatThousand));

                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.Dec.ToString(formatThousand)) : p.Month.Dec.ToString(formatThousand);
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Dec).Sum();
                                if (childTotal > p.Month.Dec)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                            // className = p.ParentMonth.Dec >= 0 ? className : className + budgetError;
                        }
                    }
                    else if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        if (month == 1)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jan.ToString(formatThousand));
                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()].ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
                            //className = p.ParentMonth.Jan >= 0 ? className : className + budgetError;
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Jan).Sum();
                                if (childTotal > p.Month.Jan)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                        }
                        else if (month == 2)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Apr.ToString(formatThousand));
                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()].ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
                            //className = p.ParentMonth.Apr >= 0 ? className : className + budgetError;
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Apr).Sum();
                                if (childTotal > p.Month.Apr)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                        }
                        else if (month == 3)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Jul.ToString(formatThousand));
                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()].ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
                            //className = p.ParentMonth.Jul >= 0 ? className : className + budgetError;
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Jul).Sum();
                                if (childTotal > p.Month.Jul)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
                                }
                            }
                        }
                        else if (month == 4)
                        {
                            divProgram.Attributes.Add("allocated", p.ParentMonth.Oct.ToString(formatThousand));
                            divProgram.InnerHtml = p.isEditable ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()].ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString(formatThousand);
                            //className = p.ParentMonth.Oct >= 0 ? className : className + budgetError;
                            if (p.ActivityType == Helpers.ActivityType.ActivityProgram.ToString())
                            {
                                var childTotal = model.Where(a => a.ActivityType == Helpers.ActivityType.ActivityTactic && a.ParentActivityId == p.ActivityId).Select(a => a.Month.Oct).Sum();
                                if (childTotal > p.Month.Oct)
                                {
                                    span.Attributes.Add("class", "orange-corner-budget");
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

        #region Custom fields

        #region Column1 Custom fields

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
            strViewBy = Helpers.ActivityType.ActivityCustomField;
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
                parentClassName = Helpers.ActivityType.ActivityCustomField;
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

        #region Column2 Custom fields

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
            strViewBy = Helpers.ActivityType.ActivityCustomField;
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
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Jan.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                                //className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                            }
                            else if (i == 2)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Feb.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Feb.ToString(formatThousand));
                                div.InnerHtml = c.Month.Feb.ToString(formatThousand);
                                //className = c.Month.Feb <= c.ParentMonth.Feb ? className : className + budgetError;
                            }
                            else if (i == 3)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Mar.ToString(formatThousand));\
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Mar.ToString(formatThousand));
                                div.InnerHtml = c.Month.Mar.ToString(formatThousand);
                                //className = c.Month.Mar <= c.ParentMonth.Mar ? className : className + budgetError;
                            }
                            else if (i == 4)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Apr.ToString(formatThousand));
                                div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                                //className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                            }
                            else if (i == 5)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.May.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.May.ToString(formatThousand));
                                div.InnerHtml = c.Month.May.ToString(formatThousand);
                                // className = c.Month.May <= c.ParentMonth.May ? className : className + budgetError;
                            }
                            else if (i == 6)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Jun.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Jun.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jun.ToString(formatThousand);
                                // className = c.Month.Jun <= c.ParentMonth.Jun ? className : className + budgetError;
                            }
                            else if (i == 7)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Jul.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                                // className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                            }
                            else if (i == 8)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Aug.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Aug.ToString(formatThousand));
                                div.InnerHtml = c.Month.Aug.ToString(formatThousand);
                                // className = c.Month.Aug <= c.ParentMonth.Aug ? className : className + budgetError;
                            }
                            else if (i == 9)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Sep.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Sep.ToString(formatThousand));
                                div.InnerHtml = c.Month.Sep.ToString(formatThousand);
                                // className = c.Month.Sep <= c.ParentMonth.Sep ? className : className + budgetError;
                            }
                            else if (i == 10)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Oct.ToString(formatThousand));
                                div.InnerHtml = c.Month.Oct.ToString(formatThousand);
                                // className = c.Month.Oct <= c.ParentMonth.Oct ? className : className + budgetError;
                            }
                            else if (i == 11)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Nov.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Nov.ToString(formatThousand));
                                div.InnerHtml = c.Month.Nov.ToString(formatThousand);
                                // className = c.Month.Nov <= c.ParentMonth.Nov ? className : className + budgetError;
                            }
                            else if (i == 12)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Dec.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Dec.ToString(formatThousand));
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
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Jan.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jan.ToString(formatThousand);
                                //className = c.Month.Jan <= c.ParentMonth.Jan ? className : className + budgetError;
                            }
                            else if (i == 2)
                            {
                                // div.Attributes.Add("allocated", c.ParentMonth.Apr.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Apr.ToString(formatThousand));
                                div.InnerHtml = c.Month.Apr.ToString(formatThousand);
                                // className = c.Month.Apr <= c.ParentMonth.Apr ? className : className + budgetError;
                            }
                            else if (i == 3)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Jul.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Jul.ToString(formatThousand));
                                div.InnerHtml = c.Month.Jul.ToString(formatThousand);
                                //className = c.Month.Jul <= c.ParentMonth.Jul ? className : className + budgetError;
                            }
                            else if (i == 4)
                            {
                                //div.Attributes.Add("allocated", c.ParentMonth.Oct.ToString(formatThousand));
                                div.Attributes.Add("mainbudget", c.BudgetMonth.Oct.ToString(formatThousand));
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
            bool isTactic = false;
            bool isLineItem = false;
            bool isPlannedTab = strTab == "1" ? true : false;
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                parentClassName = Helpers.ActivityType.ActivityCustomField;
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
                innerClass = isPlannedTab ? "tacticLevel clueplanned" : "tacticLevel clueactual";
                parentClassName = "program";
                isTactic = true;
            }
            else if (ActivityType == "lineitem")
            {
                mainClass = "sub lineItem-lvl";
                innerClass = isPlannedTab ? "lineitemLevel clueplanned" : "lineitemLevel clueactual";
                parentClassName = "tactic";
                isLineItem = true;
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
                    if (!isPlannedTab && ActivityType == Helpers.ActivityType.ActivityTactic)
                    {
                        if (model.Where(m => m.ActivityType == Helpers.ActivityType.ActivityLineItem && m.ParentActivityId == p.ActivityId).ToList().Count == 0)
                        {
                            isLineItem = true;
                        }
                    }
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jan.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Feb.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.Feb.ToString(formatThousand)) : p.Month.Feb.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Mar.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.Mar.ToString(formatThousand)) : p.Month.Mar.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Apr.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.May.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.May.ToString(formatThousand)) : p.Month.May.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jun.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.Jun.ToString(formatThousand)) : p.Month.Jun.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jul.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Aug.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.Aug.ToString(formatThousand)) : p.Month.Aug.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Sep.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.Sep.ToString(formatThousand)) : p.Month.Sep.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Oct.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString();
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Nov.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.Nov.ToString(formatThousand)) : p.Month.Nov.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Dec.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.Dec.ToString(formatThousand)) : p.Month.Dec.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jan.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter1.ToString()], p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Apr.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter2.ToString()], p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Jul.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter3.ToString()], p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
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
                                divProgram.Attributes.Add("mainbudget", p.BudgetMonth.Aug.ToString(formatThousand));
                                divProgram.InnerHtml = p.isEditable && ((isPlannedTab && (isTactic || (isLineItem && p.ActivityName != Common.DefaultLineItemTitle.ToString()))) || (!isPlannedTab && isLineItem && p.isAfterApproved)) ? ClueTipAnchorTag(Enums.Quarters[Enums.QuarterWithSpace.Quarter4.ToString()], p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString(formatThousand);
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
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.January.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.January.ToString(), p.Month.Jan.ToString(formatThousand)) : p.Month.Jan.ToString(formatThousand);
                                }

                            }
                            else if (month == 2)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Feb <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.February.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.February.ToString(), p.Month.Feb.ToString(formatThousand)) : p.Month.Feb.ToString(formatThousand);
                                }

                            }
                            else if (month == 3)
                            {

                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Mar <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.March.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.March.ToString(), p.Month.Mar.ToString(formatThousand)) : p.Month.Mar.ToString(formatThousand);
                                }

                            }
                            else if (month == 4)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Apr <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.April.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.April.ToString(), p.Month.Apr.ToString(formatThousand)) : p.Month.Apr.ToString(formatThousand);
                                }

                            }
                            else if (month == 5)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.May <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.May.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.May.ToString(), p.Month.May.ToString(formatThousand)) : p.Month.May.ToString(formatThousand);
                                }

                            }
                            else if (month == 6)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jun <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.June.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.June.ToString(), p.Month.Jun.ToString(formatThousand)) : p.Month.Jun.ToString(formatThousand);
                                }

                            }
                            else if (month == 7)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Jul <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.July.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.July.ToString(), p.Month.Jul.ToString(formatThousand)) : p.Month.Jul.ToString(formatThousand);
                                }

                            }
                            else if (month == 8)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Aug <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.August.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.August.ToString(), p.Month.Aug.ToString(formatThousand)) : p.Month.Aug.ToString(formatThousand);
                                }

                            }
                            else if (month == 9)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Sep <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.September.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.September.ToString(), p.Month.Sep.ToString(formatThousand)) : p.Month.Sep.ToString(formatThousand);
                                }


                            }
                            else if (month == 10)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Oct <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.October.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.October.ToString(), p.Month.Oct.ToString(formatThousand)) : p.Month.Oct.ToString();
                                }

                            }
                            else if (month == 11)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Nov <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.November.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.November.ToString(), p.Month.Nov.ToString(formatThousand)) : p.Month.Nov.ToString(formatThousand);
                                }
                            }
                            else if (month == 12)
                            {
                                if (ActivityType == Helpers.ActivityType.ActivityLineItem && p.Month.Dec <= 0)
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.December.ToString(), "---") : "---";
                                }
                                else
                                {
                                    divProgram.InnerHtml = p.isEditable && (!isPlannedTab && isLineItem && p.isAfterApproved) ? ClueTipAnchorTag(Enums.Months.December.ToString(), p.Month.Dec.ToString(formatThousand)) : p.Month.Dec.ToString(formatThousand);
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

        #region Column3 Custom fields

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
            strViewBy = Helpers.ActivityType.ActivityCustomField;
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
                    double unAllocated = plan.MainBudgeted - sumMonth;
                    div.InnerHtml = unAllocated.ToString(formatThousand);
                    if (unAllocated < 0)
                    {
                        div.AddCssClass("budgetError");
                    }
                    //TagBuilder span = new TagBuilder("span");

                    //double dblProgress = 0;
                    //dblProgress = (sumMonth == 0 && plan.Allocated == 0) ? 0 : (sumMonth > 0 && plan.Allocated == 0) ? 101 : sumMonth / plan.Allocated * 100;
                    //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    //if (dblProgress > 100)
                    //{
                    //    div.AddCssClass("budgetError");
                    //    span.AddCssClass("progressBar budgetError");
                    //}
                    //else
                    //{
                    //    span.AddCssClass("progressBar");
                    //}
                    //div.InnerHtml += span.ToString();
                }
                else
                {
                    //if (Tab == "2")
                    //{
                    //double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                    //div.InnerHtml = sumMonth.ToString(formatThousand);
                    //}
                    //else
                    //{
                    div.InnerHtml = "---";
                    //}
                    div.AddCssClass("firstLevel");

                }
                td.InnerHtml = div.ToString();
                tr.InnerHtml += td.ToString();


                //td = new TagBuilder("td");
                //td.AddCssClass("event-row");
                //div = new TagBuilder("div");
                //div.AddCssClass("firstLevel");
                //if (Tab == "0")
                //{
                //    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                //}
                //else
                //{
                //    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                //    {
                //        div.InnerHtml = plan.Allocated.ToString(formatThousand);
                //    }
                //    else
                //    {
                //        div.InnerHtml = "---";
                //    }
                //}
                //td.InnerHtml = div.ToString();
                //tr.InnerHtml += td.ToString();

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
                        double unAllocated = 0;//c.MainBudgeted - sumMonth;
                        double childBudget = 0;
                        var campaignIds = model.Where(m => m.ActivityType == Helpers.ActivityType.ActivityCampaign && m.ParentActivityId == c.ActivityId).Select(m => m.ActivityId).ToList();
                        var childProgramIds = model.Where(m => m.ActivityType == Helpers.ActivityType.ActivityProgram && campaignIds.Contains(m.ParentActivityId)).Select(m => m.ActivityId).ToList();
                        childBudget = model.Where(m => m.ActivityType == Helpers.ActivityType.ActivityTactic && childProgramIds.Contains(m.ParentActivityId)).Select(m => m.MainBudgeted).Sum();
                        unAllocated = childBudget - sumMonth;
                        if (unAllocated < 0)
                        {
                            div.AddCssClass("budgetError");
                        }
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

                        div.InnerHtml = unAllocated.ToString(formatThousand);
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
                    //TagBuilder tdLast = new TagBuilder("td");
                    //tdLast.AddCssClass("campaign-row audience");

                    //TagBuilder divLast = new TagBuilder("div");
                    //divLast.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    //divLast.AddCssClass("firstLevel");
                    //if (Tab == "0")
                    //{
                    //    divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                    //}
                    //else
                    //{
                    //    if (AllocatedBy != "default")
                    //    {
                    //        divLast.InnerHtml = "---"; c.Allocated.ToString(formatThousand);
                    //    }
                    //    else
                    //    {
                    //        divLast.InnerHtml = "---";
                    //    }
                    //}
                    //tdLast.InnerHtml = divLast.ToString();
                    //tdLast.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "last", AllocatedBy, Tab, View).ToString();

                    //tr.InnerHtml += tdLast.ToString();

                    sb.AppendLine(tr.ToString());
                }
            }
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
        public static MvcHtmlString ParentCostSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string AllocatedBy, string Tab, string View = "0")
        {
            string strViewBy = "";
            strViewBy = Helpers.ActivityType.ActivityCustomField;
            StringBuilder sb = new StringBuilder();
            BudgetModel plan = model.Where(pl => pl.ActivityType == Helpers.ActivityType.ActivityPlan).SingleOrDefault();
            if (plan != null)
            {
                TagBuilder tr = new TagBuilder("tr");
                TagBuilder td = new TagBuilder("td");
                td.AddCssClass("event-row");
                TagBuilder div = new TagBuilder("div");
                //if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                //{
                //    double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                //    div.InnerHtml = sumMonth.ToString(formatThousand);
                //    TagBuilder span = new TagBuilder("span");

                //    //double dblProgress = 0;
                //    //dblProgress = (sumMonth == 0 && plan.Allocated == 0) ? 0 : (sumMonth > 0 && plan.Allocated == 0) ? 101 : sumMonth / plan.Allocated * 100;
                //    //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                //    //if (dblProgress > 100)
                //    //{
                //    //    div.AddCssClass("budgetError");
                //    //    span.AddCssClass("progressBar budgetError");
                //    //}
                //    //else
                //    //{
                //    //    span.AddCssClass("progressBar");
                //    //}
                //    //div.InnerHtml += span.ToString();
                //}
                //else
                //{
                //    if (Tab == "2")
                //    {
                //        double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                //        div.InnerHtml = sumMonth.ToString(formatThousand);
                //    }
                //    else
                //    {
                //        div.InnerHtml = "---";
                //    }
                //    div.AddCssClass("firstLevel");

                //}
                //td.InnerHtml = div.ToString();
                //tr.InnerHtml += td.ToString();


                //td = new TagBuilder("td");
                //td.AddCssClass("event-row");
                //div = new TagBuilder("div");
                div.AddCssClass("firstLevel");
                if (Tab == "0")
                {
                    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                }
                else
                {
                    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    {
                        //if (Tab == "2")
                        //{
                            double sumMonth = plan.Month.Jan + plan.Month.Feb + plan.Month.Mar + plan.Month.Apr + plan.Month.May + plan.Month.Jun + plan.Month.Jul + plan.Month.Aug + plan.Month.Sep + plan.Month.Oct + plan.Month.Nov + plan.Month.Dec;
                            div.InnerHtml = sumMonth.ToString(formatThousand);
                        //}
                        //else
                        //{
                        //    div.InnerHtml = plan.Allocated.ToString(formatThousand);
                        //}
                    }
                    else
                    {
                        div.InnerHtml = "---";
                    }
                }
                if (Tab == "1")
                {
                    div.Attributes.Add("mainbudget", plan.MainBudgeted.ToString());
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
                    //TagBuilder td = new TagBuilder("td");
                    //td.AddCssClass("campaign-row audience");

                    //TagBuilder div = new TagBuilder("div");
                    //div.Attributes.Add("id", ActivityType + c.ActivityId.ToString());
                    //div.AddCssClass("firstLevel");
                    //if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                    //{
                    //    //div.InnerHtml = c.Budgeted.ToString();
                    //    double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                    //    //TagBuilder span = new TagBuilder("span");

                    //    //double dblProgress = 0;
                    //    //dblProgress = (sumMonth == 0 && c.Allocated == 0) ? 0 : (sumMonth > 0 && c.Allocated == 0) ? 101 : sumMonth / c.Allocated * 100;
                    //    //span.Attributes.Add("style", "width:" + dblProgress.ToString() + "%;");
                    //    //if (dblProgress > 100)
                    //    //{
                    //    //    div.AddCssClass("main budgetError");
                    //    //    span.AddCssClass("progressBar budgetError");
                    //    //}
                    //    //else
                    //    //{
                    //    //    div.AddCssClass("mainLevel");
                    //    //    span.AddCssClass("progressBar");
                    //    //}

                    //    div.InnerHtml = sumMonth.ToString(formatThousand);
                    //    //div.InnerHtml += sumMonth.ToString(formatThousand);
                    //    //div.InnerHtml += span.ToString();
                    //}
                    //else
                    //{
                    //    div.InnerHtml += "---";
                    //}

                    //td.InnerHtml = div.ToString();

                    //td.InnerHtml += ChildSummary(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "first", AllocatedBy, Tab, View).ToString();

                    //tr.InnerHtml += td.ToString();

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
                            //if (Tab == "2")
                            //{
                                double sumMonth = c.Month.Jan + c.Month.Feb + c.Month.Mar + c.Month.Apr + c.Month.May + c.Month.Jun + c.Month.Jul + c.Month.Aug + c.Month.Sep + c.Month.Oct + c.Month.Nov + c.Month.Dec;
                                divLast.InnerHtml = sumMonth.ToString(formatThousand);
                            //}
                            //else
                            //{
                            //    divLast.InnerHtml = c.Allocated.ToString(formatThousand);
                            //}
                        }
                        else
                        {
                            divLast.InnerHtml = "---";
                        }
                    }
                    if (Tab == "1")
                    {
                        divLast.Attributes.Add("mainbudget", c.MainBudgeted.ToString());
                    }
                    tdLast.InnerHtml = divLast.ToString();
                    tdLast.InnerHtml += ChildCostSummary(helper, Helpers.ActivityType.ActivityCampaign, c.ActivityId, model, "last", AllocatedBy, Tab, View).ToString();

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
                parentClassName = Helpers.ActivityType.ActivityCustomField;
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

                    if (AllocatedBy.ToLower() != Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower() && p.ActivityType != Helpers.ActivityType.ActivityLineItem.ToString())
                    {
                        double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                        double unAllocated = 0;//p.MainBudgeted - sumMonth;
                        double childBudget = 0;
                        if (p.ActivityType == Helpers.ActivityType.ActivityCampaign)
                        {
                            var subProgramIds = model.Where(m => m.ActivityType == Helpers.ActivityType.ActivityProgram && m.ParentActivityId == p.ActivityId).Select(m => m.ActivityId).ToList();
                            childBudget = model.Where(t => t.ActivityType == Helpers.ActivityType.ActivityTactic && subProgramIds.Contains(t.ParentActivityId)).Select(t => t.MainBudgeted).Sum();
                            unAllocated = childBudget - sumMonth;
                        }
                        else if (p.ActivityType == Helpers.ActivityType.ActivityProgram)
                        {
                            childBudget = model.Where(t => t.ActivityType == Helpers.ActivityType.ActivityTactic && t.ParentActivityId == p.ActivityId).Select(t => t.MainBudgeted).Sum();
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
                        //if (Tab == "2")
                        //{
                        //    double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                        //    divProgram.InnerHtml = (p.MainBudgeted - sumMonth).ToString(formatThousand);
                        //}
                        //else
                        //{
                        divProgram.InnerHtml += "---";
                        //}
                        divProgram.AddCssClass(innerClass + " firstLevel");
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
        public static MvcHtmlString ChildCostSummary(this HtmlHelper helper, string ActivityType, string ParentActivityId, List<BudgetModel> model, string mode, string AllocatedBy, string Tab, string View = "0")
        {
            string mainClass = "sub program-lvl";
            string innerClass = "programLevel";
            string parentClassName = "campaign";
            if (ActivityType == Helpers.ActivityType.ActivityCampaign)
            {
                mainClass = "sub campaign-lvl";
                innerClass = "campaignLevel";
                parentClassName = Helpers.ActivityType.ActivityCustomField;
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
                            //if (ActivityType == Helpers.ActivityType.ActivityLineItem || ActivityType == Helpers.ActivityType.ActivityTactic)
                            //{
                            //    divProgram.InnerHtml += "---";
                            //}
                            //else
                            //{
                            //if (Tab == "2")
                            //{
                                double sumMonth = p.Month.Jan + p.Month.Feb + p.Month.Mar + p.Month.Apr + p.Month.May + p.Month.Jun + p.Month.Jul + p.Month.Aug + p.Month.Sep + p.Month.Oct + p.Month.Nov + p.Month.Dec;
                                divProgram.InnerHtml = sumMonth.ToString(formatThousand);
                            //}
                            //else
                            //{
                            //    divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                            //}
                            //}
                        }
                        else
                        {
                            divProgram.InnerHtml += "---";
                        }
                    }
                    if (Tab == "1")
                    {
                        divProgram.Attributes.Add("mainbudget", p.MainBudgeted.ToString());
                    }
                    //divProgram.InnerHtml += p.Allocated.ToString(formatThousand);
                    divProgram.AddCssClass(innerClass);

                    div.InnerHtml += divProgram.ToString();
                    if (ActivityType == Helpers.ActivityType.ActivityCampaign)
                        div.InnerHtml += ChildCostSummary(helper, Helpers.ActivityType.ActivityProgram, p.ActivityId, model, mode, AllocatedBy, Tab, View).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityProgram)
                        div.InnerHtml += ChildCostSummary(helper, Helpers.ActivityType.ActivityTactic, p.ActivityId, model, mode, AllocatedBy, Tab, View).ToString();
                    else if (ActivityType == Helpers.ActivityType.ActivityTactic)
                        div.InnerHtml += ChildCostSummary(helper, Helpers.ActivityType.ActivityLineItem, p.ActivityId, model, mode, AllocatedBy, Tab, View).ToString();


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
        public static MvcHtmlString GenerateCustomFieldsForInspectPopup(int id, string section, int fieldCounter = 0, string mode = "ReadOnly")
        {
            //list of custom fields for particular campaign or Program or Tactic
            List<CustomFieldModel> customFieldList = Common.GetCustomFields(id, section);
            StringBuilder sb = new StringBuilder(string.Empty);

            //fieldCounter variable for defining raw style
            if (customFieldList.Count != 0)
            {
                //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                //// User custom Restrictions
                var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.UserId, true);

                //// Start - Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                List<Models.CustomRestriction> lstEditableRestrictions = new List<CustomRestriction>();
                if (mode != Enums.InspectPopupMode.ReadOnly.ToString() && section == Enums.EntityType.Tactic.ToString())
                {
                    lstEditableRestrictions = userCustomRestrictionList.Where(restriction => restriction.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit).ToList();
                }
                //// End - Added by Sohel Pathan on 28/01/2015 for PL ticket #1140

                //// Added by Sohel Pathan on 02/02/2015 for PL ticket #1156
                bool IsDefaultCustomRestrictionsEditable = Common.IsDefaultCustomRestrictionsEditable();

                foreach (var item in customFieldList)
                {
                    string className = "span3 margin-top10";
                    if (fieldCounter % 4 != 0 && fieldCounter != 0)
                    {
                        className += " paddingleft25px";
                    }
                    else
                    {
                        className += "\" style=\"clear:both;";
                    }

                    //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                    bool editableOptions = false;
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
                        string inputclassName = "span12 input-small";
                        inputclassName += item.isRequired ? " resubmission" : string.Empty;
                        //When item value contains double quots then it would be replaced 
                        string customFieldEntityValue = (item.value != null && item.value.Count > 0) ? item.value.First().Replace("\"", "&quot;") : string.Empty;
                        if (mode != Enums.InspectPopupMode.Edit.ToString())
                        {
                            sb.Append("<input type=\"text\" readonly = \"true\" title=\"" + customFieldEntityValue + "\" value=\"" + customFieldEntityValue + "\" style=\"background:#F2F2F2;\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"" + inputclassName + "\"");
                        }
                        else
                        {
                            inputclassName += " input-setup";
                            sb.Append("<input type=\"text\" maxlength =\"255\" title=\"" + customFieldEntityValue + "\" value=\"" + customFieldEntityValue + "\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"" + inputclassName + "\"");
                        }

                        //If custom field is required than add attribute require
                        if (item.isRequired)
                        {
                            sb.Append(" require=\"true\" oldValue=\"" + item.value + "\" label=\"" + item.name + "\"");
                        }
                        sb.Append("></div>");
                        sb = sb.Replace("#VIEW_DETAIL_LINK#", "");
                        fieldCounter = fieldCounter + 1;
                    }
                    else if (item.customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        if (mode == Enums.InspectPopupMode.Edit.ToString() && editableOptions == true)
                        {
                            string DropDownStyle = " style=\"";
                            string divPosition = "";
                            string require = "";
                            string name = "";
                            string addResubmissionClass = "";
                            if (item.isRequired)
                            {
                                require = " require=\"true\" oldValue=\"#OLD_VALUE#\"";
                                addResubmissionClass = "resubmission";
                            }
                            if (fieldCounter % 4 == 3)
                            {
                                DropDownStyle += "top:0px;margin-top:40px;";
                                divPosition = "style=\"position:relative;\"";
                            }

                            string displayCheckbox = "";
                            string selectionMode = "Multi";
                            string footerText = "< Single-selection";
                            string singlehover = "";
                            string trhover = "";
                            string footerclose = "";
                            if ((item.value == null) || (item.value != null && item.value.Count <= 1))
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

                            if (section == Enums.EntityType.Tactic.ToString())
                            {

                                sb.Append("<div " + divPosition + "><a class=\"dropdown_new_btn " + addResubmissionClass + "\"" + require + "  label=\"" + item.name + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                                sb.Append("<div class=\"dropdown-wrapper paddingBottom20px editdropdown minimum-width215\"" + DropDownStyle + "\"><div class=\"drop-down_header\"><table border=\"0\" class=\"table_drpdwn\"> <thead class=\"top_head_attribute\" style=\"display:none;\"><tr><td scope=\"col\" class=\"value_header top-head-attribute-header2\" style=\"display:none;\"><span>Value</span></td><td scope=\"col\" class=\"revenue_header top-head-attribute-cvr\" code=\"cvr\" title=\"CVR(%)\">CVR(%)</td><td scope=\"col\" class=\"cost_header top-head-attribute-cost\" code=\"" + Enums.InspectStage.Cost.ToString() + "\" title=\"Cost(%)\">Cost(%)</td></tr></thead><tbody class=\"top_spacing_geography\">");

                                foreach (var objOption in item.option)
                                {
                                    //// Added by Sohel Pathan on 28/01/2015 for PL ticket #1140
                                    bool isEditable = false;
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
                                        string enableCheck = string.Empty;
                                        string inputcolorcss = "class=\"multiselect-input-text-color-grey\"";
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

                                sb.Append("<div " + divPosition + "><a class=\"dropdown_new_btn\"" + require + "  label=\"" + item.name + "\"><p title=\"#HEADER_OF_DROPDOWN#\">#HEADER_OF_DROPDOWN#</p></a>");
                                sb.Append("<div class=\"dropdown-wrapper editdropdown paddingBottom20px\"" + DropDownStyle + "min-width:150px !important;\"><div class=\"drop-down_header\"><table border=\"0\" class=\"table_drpdwn\"><tbody class=\"tbodycampaignprogram\">");

                                foreach (var objOption in item.option)
                                {
                                    string enableCheck = string.Empty;

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
                            string customFieldEntityValue = "";
                            if (item.option.Count != 0)
                            {
                                sb.Append("<input type=\"text\" readonly = \"true\" value=\"#CUSTOMFEILD_VALUE#\" title=\"#CUSTOMFEILD_VALUE#\" style=\"background:#F2F2F2;\" id=\"cf_" + item.customFieldId + "\" cf_id=\"" + item.customFieldId + "\" class=\"span12 input-small\"/>");

                                #region tactic inspect pop up

                                if (section == Enums.EntityType.Tactic.ToString() && item.value != null && item.value.Count > 1)
                                {
                                    string DropDownStyle = "";
                                    string divPosition = "";
                                    if (fieldCounter % 4 == 3)
                                    {
                                        DropDownStyle = " style=\"top:0px;\"";
                                        divPosition = "style=\"position:relative;\"";
                                    }

                                    sb.Append("<div " + divPosition + "><div class=\"dropdown-wrapper\"" + DropDownStyle + "><div class=\"drop-down_header viewmodedropdown geography_popup\"><table border=\"0\" class=\"table_drpdwn\"> <thead class=\"top_head_attribute\" style=\"display:none;\"><tr><td scope=\"col\" class=\"value_header top-head-attribute-header2 padding-left20\" style=\"display:none;\"><span>Value</span></td><td scope=\"col\" class=\"revenue_header top-head-attribute-cvr\" code=\"cvr\" title=\"CVR(%)\">CVR(%)</td><td scope=\"col\" class=\"cost_header top-head-attribute-cost\" code=\"" + Enums.InspectStage.Cost.ToString() + "\" title=\"Cost(%)\">Cost(%)</td></tr></thead><tbody class=\"top_spacing_geography\">");
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

                                    sb = sb.Replace("#VIEW_DETAIL_LINK#", "<span class=\"new_tag\"><a href=\"#\">View Attribution</a></span>");

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