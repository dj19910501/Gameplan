using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

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
        #endregion
    }
}