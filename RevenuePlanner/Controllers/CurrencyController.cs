using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class CurrencyController : Controller
    {
        //
        // GET: /Currency/
        
        /// <summary>
        /// Add By Nishant Sheth
        /// To Get Plan Exchange rate and it's currency rate
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPlanCurrencyDetail()
        {
            try
            {
                var ExchangeRate = Sessions.PlanExchangeRate;
                var CurrencySymbol = Sessions.PlanCurrencySymbol;
                var jsonresult = Json(new
                {
                    ExchangeRate,
                    CurrencySymbol
                }, JsonRequestBehavior.AllowGet);
                return jsonresult;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

        #region Currency
        //insertation start 09/08/2016 kausha #2492 Following  is added to get and save currency.
        public ActionResult Currency()
        {
            List<CurrencyModel> lstCurrency = new List<CurrencyModel>();
            IEnumerable<BDSService.Currency> lstCurrencydata = objBDSServiceClient.GetAllCurrency();
            IEnumerable<BDSService.Currency> lstClientCurrency = objBDSServiceClient.GetClientCurrency(Sessions.User.ClientId);

            if (lstCurrencydata != null)
            {

                foreach (var item in lstCurrencydata)
                {
                    CurrencyModel objCurrency = new CurrencyModel();
                    //    objCurrency.CurrencyId = item.CurrencyId;
                    objCurrency.CurrencySymbol = item.CurrencySymbol;
                    objCurrency.CurrencyDetail = item.CurrencyDetail;
                    objCurrency.ISOCurrencyCode = item.ISOCurrencyCode;
                    var defaultCurrency = lstClientCurrency.Where(w => w.IsDefault == true && w.ISOCurrencyCode == item.ISOCurrencyCode).FirstOrDefault();
                    if (defaultCurrency != null)
                        objCurrency.IsDefault = true;
                    else
                        objCurrency.IsDefault = false;
                    if (lstClientCurrency.Where(w => w.ISOCurrencyCode == item.ISOCurrencyCode).Any())
                        objCurrency.ClientId = Sessions.User.ClientId;
                    objCurrency.ISOCurrencyCode = item.ISOCurrencyCode;
                    lstCurrency.Add(objCurrency);
                }

                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.MultiCurrencyEdit))
                    ViewData["permission"] = Enums.ApplicationActivity.MultiCurrencyEdit.ToString();

                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.MultiCurrencyViewOnly))
                    ViewData["permission"] = Enums.ApplicationActivity.MultiCurrencyViewOnly.ToString();



            }
            return View("Currency", lstCurrency.AsEnumerable());
        }
        public JsonResult SaveClientCurrency(List<string> curruncies)
        {
            bool status = false;
            try
            {
                status = objBDSServiceClient.SaveClientCurrency(curruncies, Sessions.User.ClientId, Sessions.User.UserId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
            }
            TempData["SuccessMessage"] = Common.objCached.CurrencySaved;
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExchangeRate()
        {
            //lstYearViewBag.yearList = new List<SelectListItem> { };
            List<SelectListItem> lstYear = new List<SelectListItem>();
            SelectListItem obj;
            for (int i = 2000; i <= DateTime.Now.Year; i++)
            {
                obj = new SelectListItem();
                obj.Text = i.ToString();
                obj.Value = i.ToString();
                if (i == DateTime.Now.Year)
                    obj.Selected = true;
                lstYear.Add(obj);
            }
            ViewBag.yearList = lstYear;

            return View("ExchangeRate", "");
        }
        public ActionResult PlanGrid(string year)
        {
            return View("_PlanCurrencyGrid", GenerateGrid(year, Enums.CurrencyComponent.Plan.ToString()));
        }
        public ActionResult ReportGrid(string year)
        {
            return View("_ReportCurrencyGrid", GenerateGrid(year, Enums.CurrencyComponent.Report.ToString()));
        }

        private Plangrid GenerateGrid(string year, string component)
        {
            //  string mode = InsepectMode;
            Plangrid objplangrid = new Plangrid();
            List<PlanHead> headobjlist = new List<PlanHead>();
            List<PlanDHTMLXGridDataModel> lineitemrowsobjlist = new List<PlanDHTMLXGridDataModel>();
            PlanDHTMLXGridDataModel lineitemrowsobj = new PlanDHTMLXGridDataModel();
            PlanController objPlanController = new PlanController();
            string customFieldEntityValue = string.Empty;
            string DropdowList = Enums.CustomFieldType.DropDownList.ToString();
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            
            try
            {
                string Gridheder = string.Empty;
                string coltype = string.Empty;
                headobjlist = GenerateHeader();
                IEnumerable<BDSService.Currency> lstClientCurrency = objBDSServiceClient.GetClientCurrency(Sessions.User.ClientId);
                BDSService.Currency lstConversionRate = objBDSServiceClient.GetCurrencyExchangeRate(Sessions.User.ClientId, Sessions.User.UserId);
                for (int i = 0; i < lstClientCurrency.Count(); i++)
                {

                    string RowID = lstClientCurrency.ElementAt(i).ISOCurrencyCode;
                    lineitemrowsobj = new PlanDHTMLXGridDataModel();
                    List<Plandataobj> lineitemdataobjlist = new List<Plandataobj>();
                    lineitemrowsobj.id = RowID;
                    Plandataobj lineitemdataobj = new Plandataobj();
                    lineitemdataobj = new Plandataobj();
                    lineitemdataobj.value = "<span><b>" + lstClientCurrency.ElementAt(i).CurrencySymbol + "</b></span>" + " " + lstClientCurrency.ElementAt(i).ISOCurrencyCode + " (" + lstClientCurrency.ElementAt(i).CurrencyDetail + ")";
                    lineitemdataobjlist.Add(lineitemdataobj);


                    for (int j = 1; j < headobjlist.Count(); j++)
                    {
                        int month = 0;

                        #region MonthNo
                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Jan + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Feb + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Mar + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.April + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.May + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Jun + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.July + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Aug + 1;
                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Sep + 1;
                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Oct + 1;

                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Nov + 1;
                        if (headobjlist.ElementAt(j).id == Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString())
                            month = (int)Enums.ReportMonthDisplay.Dec + 1;
                        #endregion


                        var startDate = new DateTime(Convert.ToInt32(year), month, 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);

                        var data = lstConversionRate.UserCurrency.CurrencyExchangeRate.Where(w => w.StartDate == startDate && w.EndDate == endDate && w.Component.ToLower() == component.ToLower() && w.CurrencyCode.ToLower() == lstClientCurrency.ElementAt(i).ISOCurrencyCode.ToLower()).FirstOrDefault();
                        lineitemdataobj = new Plandataobj();
                        if (data != null)
                        {
                            lineitemdataobj.value = Convert.ToString(data.ExchangeRate);
                        }
                        else
                            lineitemdataobj.value = "1";
                        //lineitemdataobj.actval = month.ToString();
                        lineitemdataobj.actval = month.ToString();
                        lineitemdataobj.style = "allownumericwithdecimal";
                        lineitemdataobjlist.Add(lineitemdataobj);
                    }

                    lineitemrowsobj.data = lineitemdataobjlist;
                    lineitemrowsobjlist.Add(lineitemrowsobj);
                }

                objPlanMainDHTMLXGrid.head = headobjlist;
                objPlanMainDHTMLXGrid.rows = lineitemrowsobjlist;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
            return objplangrid;
        }
        public void SaveExchangeRate(string year, string component, string currencyCode, string oldValue, string newValue, string month)
        {
            try
            {
                ExchangeRate_Log obj = new ExchangeRate_Log();
                obj.ISOCurrencyCode = currencyCode;
                obj.ClientId = Sessions.User.ClientId;
                obj.Component = component;
                int monthNo = 0;
                if (!string.IsNullOrEmpty(month))
                    monthNo = Convert.ToInt32(month);
                var startDate = new DateTime(Convert.ToInt32(year), monthNo, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                obj.StartDate = startDate;
                obj.EndDate = endDate;
                if (!string.IsNullOrEmpty(oldValue))
                    obj.OldExchangeRate = Convert.ToDouble(oldValue);
                //else
                //    obj.OldExchangeRate = null;
                if (!string.IsNullOrEmpty(newValue))
                    obj.NewExchangeRate = Convert.ToDouble(newValue);
                //else
                //    obj.NewExchangeRate = null;

                obj.CreatedBy = Sessions.User.UserId;
                obj.CreatedDate = DateTime.Now;
                bool status = objBDSServiceClient.SaveExchangeRate(obj);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }
        public List<PlanHead> GenerateHeader()
        {
            List<PlanHead> headobjlist = new List<PlanHead>();
            string coltype = string.Empty;
            List<PlanOptions> viewoptionlist = new List<PlanOptions>();
            // string mode = Mode;
            
            List<RequriedCustomField> lstRequiredcustomfield = new List<RequriedCustomField>();
            try
            {

                PlanHead headobjother = new PlanHead();

                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "Currency";
                headobjother.sort = "str";
                headobjother.width = 140;
                headobjother.value = "Currency";
                headobjlist.Add(headobjother);


                bool isEdit = false;

                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.MultiCurrencyEdit))
                    isEdit = true;

                else if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.MultiCurrencyViewOnly))
                    isEdit = false;


                for (int i = 0; i < 12; i++)
                {
                    headobjother = new PlanHead();
                    if (isEdit)
                        headobjother.type = "edn";
                    else
                        //if (datanew.row.Code == Enums.ApplicationActivity.MultiCurrencyViewOnly.ToString())
                        headobjother.type = "ro";

                    headobjother.sort = "int";
                    headobjother.width = 80;

                    if (i == 0)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString();
                    }
                    if (i == 1)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString();
                    }

                    if (i == 2)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString();
                    }

                    if (i == 3)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString();
                    }
                    if (i == 4)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString();
                    }
                    if (i == 5)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString();
                    }
                    if (i == 6)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString();
                    }
                    if (i == 7)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString();
                    }
                    if (i == 8)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString();
                    }
                    if (i == 9)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString();
                    }
                    if (i == 10)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString();
                    }
                    if (i == 11)
                    {
                        headobjother.id = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString();
                        headobjother.value = Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString();
                    }
                    headobjlist.Add(headobjother);
                }


                //var columncnt = lstmediaCodeCustomfield.Count;
                
                //if (columncnt != 0 && columncnt < 4)
                //{
                //    colwidth = 725 / columncnt;
                //}

                if (lstRequiredcustomfield != null && lstRequiredcustomfield.Count > 0)
                    ViewBag.RequiredList = lstRequiredcustomfield;
                else
                    ViewBag.RequiredList = "";
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return headobjlist;

        }
        //insertation end 09/08/2016 kausha #2492 Following  is added to get and save currency.
        #endregion

    }
}
