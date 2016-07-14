using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Elmah;
using Newtonsoft.Json;
using System.Transactions;
using RevenuePlanner.BDSService;
using System.Web;
using Integration;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Reflection;
using System.Data.SqlClient;



namespace RevenuePlanner.Controllers
{
    public class InspectController : CommonController
    {
        public InspectController()
        {

            if (System.Web.HttpContext.Current.Cache["CommonMsg"] == null)
            {

                Common.xmlMsgFilePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLCommonMsgFilePath");//Modify by Akashdeep Kadia on 09/05/2016 to resolve PL ticket #989.
                Common.objCached.loadMsg(Common.xmlMsgFilePath);
                System.Web.HttpContext.Current.Cache["CommonMsg"] = Common.objCached;
                CacheDependency dependency = new CacheDependency(Common.xmlMsgFilePath);
                System.Web.HttpContext.Current.Cache.Insert("CommonMsg", Common.objCached, dependency);
            }
            else
            {
                Common.objCached = (Message)System.Web.HttpContext.Current.Cache["CommonMsg"];
            }
        }
        //
        // GET: /Inspect/

        #region Variables
        private MRPEntities db = new MRPEntities();
        private BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
        private string DefaultLineItemTitle = "Line Item";
        private string PeriodChar = "Y";
        CacheObject objCache = new CacheObject(); // Add By Nishant Sheth // Desc:: For get values from cache
        StoredProcedure objSp = new StoredProcedure();// Add By Nishant Sheth // Desc:: For get values with storedprocedure
        List<BudgetCheckedItem> ListbudgetCheckedItem = new List<BudgetCheckedItem>(); // Add By Nishant Sheth #2325
        #endregion

        #region "Inspect Index"
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region "Plan related Functions"

        #region Load Setup tab for Plan Inspect Pop up
        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 07/11/2014
        /// Action to Load Setup Tab for Plan.
        /// </summary>
        /// <param name="id">Plan Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadPlanSetup(int id, string InspectPopupMode = "")
        {
            InspectModel _inspectmodel;
            //// Load Inspect Model data.
            if (TempData["PlanModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["PlanModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Plan).ToLower());
            }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ViewBag.PlanDetails = _inspectmodel;

            if (InspectPopupMode == Enums.InspectPopupMode.ReadOnly.ToString())
            {
                ViewBag.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
            }
            else if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
            {
                ViewBag.InspectMode = Enums.InspectPopupMode.Edit.ToString();
                //Added by Rahul Shah on 09/03/2016 for PL #1939
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); // PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                if (lstClientUsers.Count() > 0)
                {

                    ViewBag.IsServiceUnavailable = false;
                    string strUserList = string.Join(",", lstClientUsers);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati                   
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        ViewBag.OwnerList = lstPreparedOwners;
                    }
                    else
                    {
                        ViewBag.OwnerList = new List<User>();
                    }
                }
                else
                {
                    ViewBag.OwnerList = new List<User>();
                }
            }
            else
            {
                ViewBag.InspectMode = "";
            }

            return PartialView("_SetupPlan", _inspectmodel);
        }
        #endregion

        // Commented by Arpita Soni for Ticket #2236 on 06/20/2016
        //#region Load Budget tab for Plan Inspect Pop up
        ///// <summary>
        ///// Added By : Sohel Pathan
        ///// Added Date : 07/11/2014
        ///// Action to Load Budget Tab for Plan.
        ///// </summary>
        ///// <param name="id">Plan Id.</param>
        ///// <param name="InspectPopupMode"></param>
        ///// <returns>Returns Partial View Of Budget Tab.</returns>
        //public ActionResult LoadPlanBudget(int id, string InspectPopupMode = "")
        //{
        //    InspectModel _inspectmodel;
        //    // Load Inspect Model data.
        //    if (TempData["PlanModel"] != null)
        //    {
        //        _inspectmodel = (InspectModel)TempData["PlanModel"];
        //    }
        //    else
        //    {
        //        _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Plan).ToLower());
        //    }

        //    try
        //    {
        //        _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);

        //        //// To handle unavailability of BDSService
        //        if (e is System.ServiceModel.EndpointNotFoundException)
        //        {
        //            //// Flag to indicate unavailability of web service.
        //            //// Added By: Maninder Singh Wadhva on 11/24/2014.
        //            //// Ticket: 942 Exception handeling in Gameplan.
        //            return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //    ViewBag.PlanDetails = _inspectmodel;

        //    if (InspectPopupMode == Enums.InspectPopupMode.ReadOnly.ToString())
        //    {
        //        ViewBag.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
        //    }
        //    else if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
        //    {
        //        ViewBag.InspectMode = Enums.InspectPopupMode.Edit.ToString();
        //    }
        //    else
        //    {
        //        ViewBag.InspectMode = "";
        //    }

        //    double TotalAllocatedCampaignBudget = 0;
        //    // Change by Nishant sheth
        //    // Desc :: #1765 - add period condition to get value
        //    var PlanCampaignBudgetList = db.Plan_Campaign_Budget.Where(pcb => pcb.Plan_Campaign.PlanId == _inspectmodel.PlanId && pcb.Plan_Campaign.IsDeleted == false).Select(budget => budget).ToList();

        //    if (PlanCampaignBudgetList.Count > 0)
        //    {
        //        TotalAllocatedCampaignBudget = PlanCampaignBudgetList.Select(a => a.Value).Sum();

        //        ViewBag.YearDiffrence = PlanCampaignBudgetList.Select(a => Convert.ToInt32(Convert.ToInt32(a.Plan_Campaign.EndDate.Year) - Convert.ToInt32(a.Plan_Campaign.StartDate.Year))).Max();
        //        ViewBag.StartYear = PlanCampaignBudgetList.Select(a => Convert.ToInt32(a.Plan_Campaign.StartDate.Year)).Min();
        //    }
        //    else
        //    {
        //        var PlanDetail = db.Plans.Where(a => a.PlanId == id).Select(a => a).FirstOrDefault();
        //        ViewBag.YearDiffrence = 0;
        //        ViewBag.StartYear = PlanDetail.Year;
        //    }
        //    ViewBag.TotalAllocatedCampaignBudget = TotalAllocatedCampaignBudget;

        //    return PartialView("_BudgetPlan", _inspectmodel);
        //}
        //#endregion

        #region Save Plan Details other than Budget Allocation
        /// <summary>
        /// Save Plan Details other than Budget Allocation
        /// </summary>
        /// <param name="objPlanModel"></param>
        /// <param name="BudgetInputValues"></param>
        /// <param name="planBudget"></param>
        /// <param name="RedirectType"></param>
        /// <param name="UserId"></param> Added by Sohel Pathan on 07/08/2014 for PL ticket #672
        /// <returns></returns>
        [HttpPost]
        public JsonResult SavePlanDetails(InspectModel objPlanModel, string BudgetInputValues = "", string planBudget = "", string RedirectType = "", string UserId = "", string AllocatedBy = "", int YearDiffrence = 0)
        {
            //// check whether UserId is current loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    Plan plan = new Plan();
                    //// Get Plan Updated message.
                    string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.

                    if (objPlanModel.PlanId > 0)
                    {
                        //// Get Plan list by PlanId.
                        plan = db.Plans.Where(_plan => _plan.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();
                        //Modified by Rahul Shah on 09/03/2016 for PL #1939
                        Guid oldOwnerId = plan.CreatedBy;
                        plan.Title = objPlanModel.Title.Trim();
                        plan.Budget = objPlanModel.Budget;
                        plan.ModifiedBy = Sessions.User.UserId;
                        plan.ModifiedDate = System.DateTime.Now;

                        if (BudgetInputValues == "" && planBudget.ToString() == "") //// Setup Tab
                        {
                            plan.CreatedBy = objPlanModel.OwnerId;
                            plan.Description = objPlanModel.Description;
                        }
                        else   //// Budget Tab
                        {
                            //// Get budget updated message.
                            strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            plan.Budget = Convert.ToDouble(planBudget.ToString().Trim().Replace(",", "").Replace("$", ""));

                            #region Update Budget Allocation Value
                            if (BudgetInputValues != "")
                            {
                                string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                                //// Get Previous budget allocation data by PlanId.
                                var PrevPlanBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == objPlanModel.PlanId).Select(pb => pb).ToList();
                                // Change by Nishant sheth
                                // Desc :: #1765 - to replace the lenth of array to allocated by
                                if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower()) // if current input values are monthly.
                                {
                                    bool isExists;
                                    Plan_Budget updatePlanBudget, objPlanBudget;
                                    double newValue = 0;
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        isExists = false;
                                        if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            //// Get budget value periodically.
                                            updatePlanBudget = new Plan_Budget();
                                            updatePlanBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                            if (updatePlanBudget != null)
                                            {
                                                if (arrBudgetInputValues[i] != "")
                                                {
                                                    //// Get current inputed value.
                                                    newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                    if (updatePlanBudget.Value != newValue)
                                                    {
                                                        //// Update previous budget value with current value.
                                                        updatePlanBudget.Value = newValue;
                                                        db.Entry(updatePlanBudget).State = EntityState.Modified;
                                                    }
                                                }
                                                else
                                                {
                                                    db.Entry(updatePlanBudget).State = EntityState.Deleted;
                                                }
                                                isExists = true;
                                            }
                                        }
                                        //// if previous values does not exist then insert new values.
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                            objPlanBudget = new Plan_Budget();
                                            objPlanBudget.PlanId = objPlanModel.PlanId;
                                            objPlanBudget.Period = PeriodChar + (i + 1);
                                            objPlanBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                                // Change by Nishant sheth
                                // Desc :: #1765 - to replace the lenth of array to allocated by
                                else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower()) //// if current input values are Quarterly.
                                {
                                    int QuarterCnt = 1, j = 1;
                                    // Change by Nishant sheth
                                    // Desc :: #1765 - for add/update the multiple year period value
                                    int m = 0;
                                    for (int k = 1; k <= (YearDiffrence + 1); k++)
                                    {
                                        bool isExists;
                                        List<Plan_Budget> thisQuartersMonthList;
                                        Plan_Budget thisQuarterFirstMonthBudget, objPlanBudget;
                                        double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
                                        for (int i = m; i < (4 * k); i++)
                                        {
                                            if ((i + 1) % 4 == 0)
                                            {
                                                m = i + 1;
                                            }
                                            //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                            isExists = false;
                                            if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                                            {
                                                //// Get current Quarter months budget.
                                                thisQuartersMonthList = new List<Plan_Budget>();
                                                thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                                thisQuarterFirstMonthBudget = new Plan_Budget();
                                                thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                                if (thisQuarterFirstMonthBudget != null)
                                                {
                                                    if (arrBudgetInputValues[i] != "")
                                                    {
                                                        thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                        //// Get quarter total budget. 
                                                        thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                        newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                        if (thisQuarterTotalBudget != newValue)
                                                        {
                                                            //// Get budget difference.
                                                            BudgetDiff = newValue - thisQuarterTotalBudget;
                                                            if (BudgetDiff > 0)
                                                            {
                                                                //// Set quarter first month budget value.
                                                                thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                            }
                                                            else
                                                            {
                                                                j = 1;
                                                                while (BudgetDiff < 0)
                                                                {
                                                                    if (thisQuarterFirstMonthBudget != null)
                                                                    {
                                                                        BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                                        if (BudgetDiff <= 0)
                                                                            thisQuarterFirstMonthBudget.Value = 0;
                                                                        else
                                                                            thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                                        db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                                    }
                                                                    if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                                    {
                                                                        thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                                    }

                                                                    j = j + 1;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                                    }
                                                    isExists = true;
                                                }
                                            }
                                            //// if previous values does not exist then insert new values.
                                            if (!isExists && arrBudgetInputValues[i] != "")
                                            {
                                                //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                                objPlanBudget = new Plan_Budget();
                                                objPlanBudget.PlanId = objPlanModel.PlanId;
                                                objPlanBudget.Period = PeriodChar + QuarterCnt;
                                                objPlanBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                                objPlanBudget.CreatedBy = Sessions.User.UserId;
                                                objPlanBudget.CreatedDate = DateTime.Now;
                                                db.Entry(objPlanBudget).State = EntityState.Added;
                                            }
                                            QuarterCnt = QuarterCnt + 3;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        db.Entry(plan).State = EntityState.Modified;
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                        //Modified by Rahul Shah on 09/03/2016 for PL #1939
                        int result = db.SaveChanges();
                        // Add By Nishant Sheth
                        // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                        DataSet dsPlanCampProgTac = new DataSet();
                        dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                        objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);
                        if (result > 0)
                        {
                            #region "Send Email Notification For Owner changed"

                            //Send Email Notification For Owner changed.
                            if (objPlanModel.OwnerId != oldOwnerId && objPlanModel.OwnerId != Guid.Empty)
                            {
                                if (Sessions.User != null)
                                {
                                    List<string> lstRecepientEmail = new List<string>();
                                    List<User> UsersDetails = new List<BDSService.User>();
                                    var csv = string.Concat(objPlanModel.OwnerId.ToString(), ",", oldOwnerId.ToString(), ",", Sessions.User.UserId.ToString());

                                    try
                                    {
                                        UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorSignal.FromCurrentContext().Raise(e);

                                        //To handle unavailability of BDSService
                                        if (e is System.ServiceModel.EndpointNotFoundException)
                                        {
                                            //// Flag to indicate unavailability of web service.
                                            return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                        }
                                    }

                                    var NewOwner = UsersDetails.Where(u => u.UserId == objPlanModel.OwnerId).Select(u => u).FirstOrDefault();
                                    var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                    if (NewOwner.Email != string.Empty)
                                    {
                                        lstRecepientEmail.Add(NewOwner.Email);
                                    }
                                    string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                    string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                    string PlanTitle = plan.Title.ToString();
                                    //string CampaignTitle = pcobj.Title.ToString();
                                    //string ProgramTitle = pcobj.Title.ToString();
                                    if (lstRecepientEmail.Count > 0)
                                    {
                                        string strURL = GetNotificationURLbyStatus(plan.PlanId, plan.PlanId, Enums.Section.Plan.ToString().ToLower());
                                        Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, PlanTitle, PlanTitle, PlanTitle, PlanTitle, Enums.Section.Plan.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    }
                                }
                            }
                            #endregion
                            if (RedirectType.ToLower() == "budgeting")
                            {
                                TempData["SuccessMessage"] = Common.objCached.PlanSaved;
                                return Json(new { id = plan.PlanId, redirect = Url.Action("Budgeting", new { PlanId = plan.PlanId }) });
                            }
                            else if (RedirectType.ToLower() == "")
                            {

                                return Json(new { id = plan.PlanId, succmsg = strMessage, redirect = "" });
                            }
                            else
                            {
                                return Json(new { id = plan.PlanId, redirect = Url.Action("Assortment", new { ismsg = "Plan Saved Successfully." }) });
                            }
                        }
                        else
                        {
                            return Json(new { id = 0, errormsg = Common.objCached.ErrorOccured.ToString() });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { id = 0 });
        }
        #endregion
        #endregion

        #region "Campaign related Functions"

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Campaign Setup Tab.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupCampaign(int id)
        {
            InspectModel _inspectmodel;
            // Load Inspect Model data.
            //if (TempData["CampaignModel"] != null)
            //{
            //    _inspectmodel = (InspectModel)TempData["CampaignModel"];
            //}
            //else
            //{
            _inspectmodel = GetInspectModel(id, "campaign", false);
            //  }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            //// Set Last Sync Date.
            if (_inspectmodel.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = Common.objCached.LastSynced.Replace("{0}", Common.GetFormatedDate(_inspectmodel.LastSyncDate)); //// Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            #region "Set values in ViewBag"

            List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == id && tactic.IsDeleted == false).ToList());
            ViewBag.MQLs = PlanTacticValuesList.Sum(tactic => tactic.MQL);
            ViewBag.Cost = Common.CalculateCampaignCost(id); //// Modified for PL#440 by Dharmraj
            ViewBag.Revenue = Math.Round(PlanTacticValuesList.Sum(tactic => tactic.Revenue)); ////  Update by Bhavesh to Display Revenue

            ViewBag.CampaignDetail = _inspectmodel;
            double? objCampaign = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == id).FirstOrDefault().CampaignBudget;
            ViewBag.CampaignBudget = objCampaign != null ? objCampaign : 0;

            #endregion

            return PartialView("_SetupCampaign", _inspectmodel);
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Campaign Review Tab.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReviewCampaign(int id)
        {
            InspectModel _inspectmodel;
            // Load Inspect Model data.
            //if (TempData["CampaignModel"] != null)
            //{
            //    _inspectmodel = (InspectModel)TempData["CampaignModel"];
            //}
            //else
            //{
            _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Campaign).ToLower(), false);
            //  }

            //// Get Tactic comment by PlanCampaignId from Plan_Campaign_Program_Tactic_Comment table.
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanCampaignId == id && tc.PlanCampaignId.HasValue
                                 select tc).ToArray();

            //// Get Users list.
            List<Guid> userListId = new List<Guid>();
            userListId = (from tc in tacticComment select tc.CreatedBy).ToList<Guid>();
            userListId.Add(_inspectmodel.OwnerId);
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Set InspectReviewModel in ViewBag.
            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanCampaignId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanCampaignId = Convert.ToInt32(tc.PlanCampaignId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Any() ? userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault() : Common.GameplanIntegrationService,
                                       CreatedBy = tc.CreatedBy
                                   }).OrderByDescending(x => x.CommentDate).ToList(); //Modified By komal Rawal for 2043 resort comment in desc order
            //// Get Owner name by OwnerId from Username list.
            var ownername = (from user in userName
                             where user.UserId == _inspectmodel.OwnerId
                             select user.FirstName + " " + user.LastName).FirstOrDefault();
            if (ownername != null)
            {
                _inspectmodel.Owner = ownername.ToString();
            }
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Campaign_Program_Tactic> PlanTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == id && tactic.IsDeleted == false).ToList();
            _inspectmodel.MQLs = Common.GetMQLValueTacticList(PlanTacticIds).Sum(t => t.MQL);
            ViewBag.CampaignDetail = _inspectmodel;

            bool isValidOwner = false;
            if (_inspectmodel.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            ViewBag.IsModelDeploy = _inspectmodel.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = new List<Guid>();
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(_inspectmodel.OwnerId) && Common.IsSectionApprovable(lstSubOrdinatesPeers, id, Enums.Section.Campaign.ToString()))////Modified by Sohel Pathan for PL ticket #688 and #689
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Modified by komal Rawal for #1158
            bool IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);

            if (IsCommentsViewEditAuthorized)
            {
                List<int> lstAllowedPermissionids = new List<int>();
                List<int> planTacticIds = new List<int>();
                planTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && tactic.Plan_Campaign_Program.PlanCampaignId == id)
                                                                                     .Select(tactic => tactic.PlanTacticId).ToList();
                lstAllowedPermissionids = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                if (lstAllowedPermissionids.Count != planTacticIds.Count)
                {
                    IsCommentsViewEditAuthorized = false;
                }

            }
            ViewBag.IsCommentsViewEditAuthorized = IsCommentsViewEditAuthorized;
            // End


            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = new List<Guid>();
            try
            {
                lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool IsCampaignEditable = false;
            if (_inspectmodel.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsCampaignEditable = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsCampaignEditable = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(_inspectmodel.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsCampaignEditable = true;
                }
            }
            ViewBag.IsCampaignEditable = IsCampaignEditable;

            #region "Show Sync button in Review page only Integration InstanceType "Salesforce"
            bool isInstanceSalesforce = false;
            if (_inspectmodel.IsDeployedToIntegration && _inspectmodel.IsIntegrationInstanceExist == "Yes")
            {
                //modified by Rahul shah On 10/11/2015 for Pl #1630
                string integrationType = string.Empty;
                int? integrationInstanceId = db.Plan_Campaign.FirstOrDefault(t => t.PlanCampaignId == _inspectmodel.PlanCampaignId).Plan.Model.IntegrationInstanceId;
                if (integrationInstanceId != null)
                {
                    integrationType = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == integrationInstanceId).IntegrationType.Code;
                }
                if (integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
                {
                    isInstanceSalesforce = true;
                    #region "Set Campagin Last Synced Timestamp to ViewBag variable"
                    string campaignEntityType = Enums.Section.Campaign.ToString();
                    var planEntityLogList = db.IntegrationInstancePlanEntityLogs.Where(ipt => ipt.EntityId == _inspectmodel.PlanCampaignId && ipt.EntityType == campaignEntityType).ToList();
                    if (planEntityLogList.Any())
                    {
                        ViewBag.CampaignLastSync = planEntityLogList.OrderByDescending(log => log.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
                    }
                    #endregion
                }
            }
            ViewBag.IsInstanceSalesfore = isInstanceSalesforce;
            #endregion

            return PartialView("_ReviewCampaign");
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Create Campaign.
        /// </summary>
        /// <param name="id">Plan Id</param>
        /// <returns>Returns Partial View Of Campaign.</returns>
        /// modified by Rahul Shah on 17/03/2016 for PL #2032 
        public ActionResult CreateCampaign(int id)
        {
            //// Get Plan by Id.
            int planId = id;
            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == planId);

            #region "Set values in ViewBag"

            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            ViewBag.IsDeployedToIntegration = false;
            ViewBag.IsCreated = true;
            ViewBag.RedirectType = false;
            ViewBag.IsOwner = true;
            ViewBag.Year = objPlan.Year;
            ViewBag.PlanTitle = objPlan.Title;
            #endregion



            ViewBag.OwnerName = Sessions.User.FirstName + " " + Sessions.User.LastName;//Common.GetUserName(Sessions.User.UserId.ToString());

            //// Set Plan_CampaignModel data to pass into partialview.
            Plan_CampaignModel pc = new Plan_CampaignModel();
            pc.PlanId = planId;
            pc.StartDate = GetCurrentDateBasedOnPlan(false, planId);
            pc.EndDate = GetCurrentDateBasedOnPlan(true, planId);
            pc.CampaignBudget = 0;
            pc.AllocatedBy = objPlan.AllocatedBy;
            pc.OwnerId = Sessions.User.UserId;
            #region "Calculate Plan remaining budget by plan Id"
            var lstAllCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanId == planId && campaign.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(campaign => campaign.CampaignBudget);
            double planBudget = objPlan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;
            #endregion
            // Added by Rahul Shah on 17/03/2016 for PL #2032 
            ViewBag.IsCampaignEdit = true;
            #region "Owner List"
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList();
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                if (lstClientUsers.Count() > 0)
                {
                    ViewBag.IsServiceUnavailable = false;
                    string strUserList = string.Join(",", lstClientUsers);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati                   
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        ViewBag.OwnerList = lstPreparedOwners;
                    }
                    else
                    {
                        ViewBag.OwnerList = new List<User>();
                    }
                }
                else
                {
                    ViewBag.OwnerList = new List<User>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    ViewBag.IsServiceUnavailable = true;
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            #endregion
            return PartialView("_EditSetupCampaign", pc);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Load Campaign Setup Tab in edit mode.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of edit Setup Tab.</returns>
        public ActionResult LoadEditSetupCampaign(int id)
        {
            ViewBag.IsCreated = false;
            //// Get Campaign list by Id.
            Plan_Campaign pc = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(id) && pcobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pc == null)
            {
                return null;
            }

            try
            {
                ViewBag.OwnerName = Common.GetUserName(pc.CreatedBy.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                }
            }
            #region "Set values in ViewBag"
            ViewBag.Year = pc.Plan.Year;
            ViewBag.PlanTitle = pc.Plan.Title;
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pc.Plan.Model);
            #endregion

            //// Set Plan_CampaignModel data to pass into partialview.
            Plan_CampaignModel pcm = new Plan_CampaignModel();
            pcm.PlanCampaignId = pc.PlanCampaignId;
            pcm.Title = HttpUtility.HtmlDecode(pc.Title);
            pcm.Description = HttpUtility.HtmlDecode(pc.Description);
            pcm.IsDeployedToIntegration = pc.IsDeployedToIntegration;
            ViewBag.IsDeployedToIntegration = pcm.IsDeployedToIntegration;
            pcm.StartDate = pc.StartDate;
            pcm.EndDate = pc.EndDate;
            pcm.OwnerId = pc.CreatedBy;
            pcm.Status = pc.Status;




            var programs = db.Plan_Campaign_Program.Where(program => program.PlanCampaignId == id && program.IsDeleted.Equals(false)).ToList();
            var tactic = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.Plan_Campaign_Program.PlanCampaignId == id && _tactic.IsDeleted.Equals(false)).ToList();

            //// Set Program Start date & End date.
            var _programdata = (from _program in programs select _program);
            if (_programdata.Count() > 0)
            {
                pcm.PStartDate = (from opsd in _programdata select opsd.StartDate).Min();
                pcm.PEndDate = (from opsd in _programdata select opsd.EndDate).Max();
            }

            //// Tactic Start date & End date.
            var _tacticdata = (from _tactic in tactic select _tactic);
            if (_tacticdata.Count() > 0)
            {
                pcm.TStartDate = (from _tactic in _tacticdata select _tactic.StartDate).Min();
                pcm.TEndDate = (from _tactic in _tacticdata select _tactic.EndDate).Max();
            }


            List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(tactic);
            pcm.MQLs = PlanTacticValuesList.Sum(tm => tm.MQL);
            pcm.Cost = Common.CalculateCampaignCost(pc.PlanCampaignId); //pc.Cost; // Modified for PL#440 by Dharmraj
            // Start Added By Dharmraj #567 : Budget allocation for campaign
            pcm.CampaignBudget = pc.CampaignBudget;
            pcm.AllocatedBy = pc.Plan.AllocatedBy;

            #region "Calculate plan remaining budget"
            var lstAllCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanId == pc.PlanId && campaign.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(campaign => campaign.CampaignBudget);
            double planBudget = pc.Plan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;
            #endregion

            pcm.Revenue = Math.Round(PlanTacticValuesList.Sum(tm => tm.Revenue)); //  Update by Bhavesh to Display Revenue
            // End Added By Dharmraj #567 : Budget allocation for campaign

            if (Sessions.User.UserId == pc.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }

            //Verify that existing user has created activity or it has subordinate permission and activity owner is subordinate of existing user
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            List<Guid> lstSubordinatesIds = new List<Guid>();
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            }

            if (lstSubordinatesIds.Contains(pc.CreatedBy))
            {
                ViewBag.IsAuthorized = true;
            }

            /*Modified By : Kalpesh Sharma :: Optimize the code and performance of application*/
            ViewBag.Year = pc.Plan.Year;
            //Added by Komal Rawal for #711
            ViewBag.IsCampaignEdit = true;
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); // PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                if (lstClientUsers.Count() > 0)
                {

                    ViewBag.IsServiceUnavailable = false;

                    string strUserList = string.Join(",", lstClientUsers);
                    //List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati
                    // lstUserDetails = lstUserDetails.Where(i => !i.IsDeleted).ToList();
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        ViewBag.OwnerList = lstPreparedOwners;
                    }
                    else
                    {
                        ViewBag.OwnerList = new List<User>();
                    }
                }
                else
                {
                    ViewBag.OwnerList = new List<User>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.

                    ViewBag.IsServiceUnavailable = true;
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            return PartialView("_EditSetupCampaign", pcm);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Save Campaign.
        /// </summary>
        /// <param name="form">Form object of Plan_CampaignModel.</param>
        /// <param name="UserId">User Id.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <param name="title"></param>
        /// <param name="customFieldInputs"></param>
        /// <param name="planId"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveCampaign(Plan_CampaignModel form, string title, string customFieldInputs, string UserId = "", int planId = 0)
        {
            //// check whether UserId is current loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);

                //// Add New Record
                if (form.PlanCampaignId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// check same record exist or not.
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(planId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();

                            //// if record exist then return with duplication message.
                            if (pc != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { isSaved = false, msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Insert New Record to Plan_Campaign table"
                                Plan_Campaign pcobj = new Plan_Campaign();

                                pcobj.PlanId = form.PlanId; //Sessions.PlanId;
                                pcobj.Title = form.Title;
                                pcobj.Description = form.Description;
                                pcobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcobj.StartDate = form.StartDate;
                                pcobj.EndDate = form.EndDate;
                                //pcobj.CreatedBy = Sessions.User.UserId; // Commented by Rahul Shah on 17/03/2016 for PL #2032 
                                pcobj.CreatedBy = form.OwnerId; // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                pcobj.CreatedDate = DateTime.Now;
                                pcobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); // status field in Plan_Campaign table 
                                pcobj.CampaignBudget = form.CampaignBudget;
                                db.Entry(pcobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                #endregion

                                int campaignid = pcobj.PlanCampaignId;
                                result = Common.InsertChangeLog(form.PlanId, null, campaignid, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                Plan pcp = db.Plans.Where(pcobj1 => pcobj1.PlanId.Equals(pcobj.PlanId) && pcobj1.IsDeleted.Equals(false)).FirstOrDefault();

                                #region "Send Email Notification For Owner changed"
                                if (result > 0)
                                {
                                    // Add By Nishant Sheth
                                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                    DataSet dsPlanCampProgTac = new DataSet();
                                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != Sessions.User.UserId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", Sessions.User.UserId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcobj.Plan.Title.ToString();
                                            string CampaignTitle = pcobj.Title.ToString();
                                            string ProgramTitle = pcobj.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcobj.PlanId, campaignid, Enums.Section.Campaign.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcobj.Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Campaign.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }
                                    }
                                }

                                #endregion
                                #region "Save custom field to CustomField_Entity table"
                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = campaignid;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                db.SaveChanges();
                                #endregion
                                // Added by Rahul Shah on 17/03/2016 for PL #2032 

                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { isSaved = true, msg = strMessage, CampaignID = campaignid });
                            }
                        }
                    }
                }
                else  //// Update record.
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
                        var TransactionOption = new System.Transactions.TransactionOptions();
                        TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

                        using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
                        {
                            //// Get PlanId by PlanCampaignId.
                            planId = db.Plan_Campaign.Where(_plan => _plan.PlanCampaignId.Equals(form.PlanCampaignId)).FirstOrDefault().PlanId;
                            //// check for duplicate record.
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(planId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();

                            //// If record exist then return duplicatino message.
                            if (pc != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { isSaved = false, msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Update record into Plan_Campaign table"
                                Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(form.PlanCampaignId) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
                                // Add By Nishant Sheth
                                // Desc::#1765 - To remove pervious data from db if end date year difference is less then to compare end date.
                                int EndDateYear = pcobj.EndDate.Year;
                                int FormEndDateYear = form.EndDate.Year;
                                int EndDatediff = EndDateYear - FormEndDateYear;
                                if (EndDatediff > 0)
                                {
                                    listMonthDynamic lstMonthlyDynamic = new listMonthDynamic();

                                    List<string> lstMonthlyExtended = new List<string>();
                                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcobj.EndDate.Year) - Convert.ToInt32(pcobj.StartDate.Year));
                                    string periodPrefix = "Y";
                                    int baseYear = 0;
                                    for (int i = 0; i < (YearDiffrence + 1); i++)
                                    {
                                        for (int j = 1; j <= 12; j++)
                                        {
                                            lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                                        }
                                        baseYear = baseYear + 12;
                                    }
                                    lstMonthlyDynamic.Id = pcobj.PlanCampaignId;
                                    lstMonthlyDynamic.listMonthly = lstMonthlyExtended.AsEnumerable().Reverse().ToList();

                                    List<string> deleteperiodmonth = new List<string>();
                                    for (int i = 0; i < EndDatediff; i++)
                                    {
                                        var listofperiod = lstMonthlyDynamic.listMonthly.Skip(i * 12).Take(12).ToList();
                                        listofperiod.ForEach(a => { deleteperiodmonth.Add(a); });
                                    }

                                    var listBudget = db.Plan_Campaign_Budget.Where(a => a.PlanCampaignId == pcobj.PlanCampaignId && deleteperiodmonth.Contains(a.Period)).ToList();
                                    listBudget.ForEach(a => { db.Entry(a).State = EntityState.Deleted; });
                                }
                                // End By Nishant Sheth
                                pcobj.Title = title;
                                Guid oldOwnerId = pcobj.CreatedBy;
                                pcobj.Description = form.Description;
                                pcobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcobj.StartDate = form.StartDate;
                                pcobj.EndDate = form.EndDate;
                                pcobj.ModifiedBy = Sessions.User.UserId;
                                pcobj.ModifiedDate = DateTime.Now;
                                pcobj.CampaignBudget = form.CampaignBudget;
                                pcobj.CreatedBy = form.OwnerId;
                                db.Entry(pcobj).State = EntityState.Modified;
                                #endregion

                                Common.InsertChangeLog(form.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                                #region "Remove previous custom fields by PlanCampaignId"
                                string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(custmfield => custmfield.EntityId == pcobj.PlanCampaignId && custmfield.CustomField.EntityType == entityTypeCampaign).ToList();
                                prevCustomFieldList.ForEach(custmfield => db.Entry(custmfield).State = EntityState.Deleted);
                                #endregion

                                #region "Save Custom fields to CustomField_Entity table"
                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = pcobj.PlanCampaignId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }

                                int result = db.SaveChanges();
                                #endregion

                                #region "Send Email Notification For Owner changed"

                                // Start - Added by Pratik on 11/03/2014 for PL ticket #711
                                if (result > 0)
                                {
                                    // Add By Nishant Sheth
                                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                    DataSet dsPlanCampProgTac = new DataSet();
                                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != oldOwnerId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", oldOwnerId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcobj.Plan.Title.ToString();
                                            string CampaignTitle = pcobj.Title.ToString();
                                            string ProgramTitle = pcobj.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcobj.PlanId, form.PlanCampaignId, Enums.Section.Campaign.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcobj.Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Campaign.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }
                                    }
                                }

                                // End - Added by Pratik on 11/03/2014 for PL ticket #711

                                #endregion

                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { isSaved = true, msg = strMessage, EndDatediff = EndDatediff });// Modified By Nishant Sheth Desc:: #1812 refresh time frame dropdown
                            }

                        }
                    }
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { isSaved = false });
        }

        #region Budget Allocation for Campaign Tab
        ///// <summary>
        ///// Fetch the Campaign Budget Allocation 
        ///// </summary>
        ///// <param name="id">Campaign Id</param>
        ///// <returns></returns>
        //public PartialViewResult LoadCampaignBudgetAllocation(int id = 0)
        //{
        //    //// Get Budget tab data of Camapaign by Id
        //    Plan_Campaign pcp = db.Plan_Campaign.Where(pcpobj => pcpobj.PlanCampaignId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
        //    if (pcp == null)
        //    {
        //        return null;
        //    }
        //    //// Set Plan_CampaignModel to pass into partialview.
        //    Plan_CampaignModel pcpm = new Plan_CampaignModel();
        //    pcpm.PlanCampaignId = pcp.PlanCampaignId;
        //    pcpm.CampaignBudget = pcp.CampaignBudget;
        //    // Add by Nishant sheth
        //    // Desc :: #1765 - to get the year diffrence between item start date and end date
        //    ViewBag.YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcp.EndDate.Year) - Convert.ToInt32(pcp.StartDate.Year));
        //    ViewBag.StartYear = Convert.ToInt32(pcp.StartDate.Year);

        //    // Get Plan Allocated from Plan table by PlanId.
        //    var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == pcp.PlanId);
        //    pcpm.AllocatedBy = objPlan.AllocatedBy;

        //    #region "Calculate Plan remaining budget"
        //    var lstAllCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanId == pcp.PlanId && campaign.IsDeleted == false).ToList();
        //    double allCampaignBudget = lstAllCampaign.Sum(campaign => campaign.CampaignBudget);
        //    double planBudget = objPlan.Budget;
        //    double planRemainingBudget = planBudget - allCampaignBudget;
        //    ViewBag.planRemainingBudget = planRemainingBudget;
        //    #endregion

        //    return PartialView("_SetupCampaignBudgetAllocation", pcpm);
        //}

        ///// <summary>
        ///// Action to Save Campaign Allocation.
        ///// </summary>
        ///// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        ///// <param name="BudgetInputValues">Budget Input Values.</param>
        ///// <param name="UserId">User Id.</param>
        ///// <param name="title"></param>
        ///// <returns>Returns Action Result.</returns>
        //[HttpPost]
        //public ActionResult SaveCampaignBudgetAllocation(Plan_CampaignModel form, string BudgetInputValues, string UserId = "", string title = "", string AllocatedBy = "", int YearDiffrence = 0)
        //{
        //    //// check whether UserId is loggined user or not.
        //    if (!string.IsNullOrEmpty(UserId))
        //    {
        //        ////Compare login user with userid.
        //        if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
        //        {
        //            return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    try
        //    {
        //        using (MRPEntities mrp = new MRPEntities())
        //        {
        //            //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
        //            var TransactionOption = new System.Transactions.TransactionOptions();
        //            TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

        //            using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
        //            // using (var scope = new TransactionScope())
        //            {
        //                //// Get Campaign data to check duplication.
        //                var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(db.Plan_Campaign.Where(_campaign => _campaign.PlanCampaignId == form.PlanCampaignId).Select(_campaign => _campaign.PlanId).FirstOrDefault()) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();
        //                string[] arrBudgetInputValues = BudgetInputValues.Split(',');

        //                //// if duplicate record exist then return duplication message.
        //                if (pc != null)
        //                {
        //                    string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
        //                    return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    #region " Update record to Plan_Campaign table"
        //                    Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(form.PlanCampaignId) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
        //                    pcobj.Title = title;
        //                    pcobj.ModifiedBy = Sessions.User.UserId;
        //                    pcobj.ModifiedDate = DateTime.Now;
        //                    pcobj.CampaignBudget = form.CampaignBudget;
        //                    db.Entry(pcobj).State = EntityState.Modified;
        //                    Common.InsertChangeLog(pcobj.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
        //                    #endregion

        //                    if (arrBudgetInputValues.Length > 0)    // Added by Sohel Pathan on 27/08/2014 for PL ticket #758
        //                    {
        //                        // Start Added By Dharmraj #567 : Budget allocation for campaign
        //                        //// Get Previous budget allocation list.
        //                        var PrevAllocationList = db.Plan_Campaign_Budget.Where(campBudget => campBudget.PlanCampaignId == form.PlanCampaignId).Select(campBudget => campBudget).ToList();

        //                        //// Process for Monthly budget values.
        //                        // Change by Nishant sheth
        //                        // Desc :: #1765 - to replace the lenth of array to allocated by
        //                        if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                        {
        //                            bool isExists;
        //                            Plan_Campaign_Budget updatePlanCampaignBudget, objPlanCampaignBudget;
        //                            double newValue = 0;
        //                            for (int i = 0; i < arrBudgetInputValues.Length; i++)
        //                            {
        //                                // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
        //                                isExists = false;
        //                                if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                                {
        //                                    //// Get previous campaign budget values by Period.
        //                                    updatePlanCampaignBudget = new Plan_Campaign_Budget();
        //                                    updatePlanCampaignBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
        //                                    if (updatePlanCampaignBudget != null)
        //                                    {
        //                                        if (arrBudgetInputValues[i] != "")
        //                                        {
        //                                            //// Update budget value with old value.
        //                                            newValue = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                            if (updatePlanCampaignBudget.Value != newValue)
        //                                            {
        //                                                updatePlanCampaignBudget.Value = newValue;
        //                                                db.Entry(updatePlanCampaignBudget).State = EntityState.Modified;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            db.Entry(updatePlanCampaignBudget).State = EntityState.Deleted; //// Added by Sohel Pathan on 01/09/2014 for PL ticket #758
        //                                        }
        //                                        isExists = true;
        //                                    }
        //                                }
        //                                //// if Old budget value does not exist then insert new value to table.
        //                                if (!isExists && arrBudgetInputValues[i] != "")
        //                                {
        //                                    // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
        //                                    objPlanCampaignBudget = new Plan_Campaign_Budget();
        //                                    objPlanCampaignBudget.PlanCampaignId = form.PlanCampaignId;
        //                                    objPlanCampaignBudget.Period = PeriodChar + (i + 1);
        //                                    objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                    objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
        //                                    objPlanCampaignBudget.CreatedDate = DateTime.Now;
        //                                    db.Entry(objPlanCampaignBudget).State = EntityState.Added;
        //                                }
        //                            }
        //                        }
        //                        // Change by Nishant sheth
        //                        // Desc :: #1765 - to replace the lenth of array to allocated by
        //                        else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower())  //// Process for Quarterly budget values.
        //                        {
        //                            int QuarterCnt = 1, j = 1;
        //                            int m = 0;

        //                            for (int k = 1; k <= (YearDiffrence + 1); k++)
        //                            {

        //                                bool isExists;
        //                                List<Plan_Campaign_Budget> thisQuartersMonthList;
        //                                Plan_Campaign_Budget thisQuarterFirstMonthBudget, objPlanCampaignBudget;
        //                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
        //                                for (int i = m; i < (4 * k); i++)
        //                                {
        //                                    if ((i + 1) % 4 == 0)
        //                                    {
        //                                        m = i + 1;
        //                                    }
        //                                    // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
        //                                    isExists = false;
        //                                    if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                                    {
        //                                        //// Get Quarter budget list.
        //                                        thisQuartersMonthList = new List<Plan_Campaign_Budget>();
        //                                        thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();

        //                                        //// Get First month values from Quarterly budget list.
        //                                        thisQuarterFirstMonthBudget = new Plan_Campaign_Budget();
        //                                        thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

        //                                        if (thisQuarterFirstMonthBudget != null)
        //                                        {
        //                                            if (arrBudgetInputValues[i] != "")
        //                                            {
        //                                                thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(budget => budget.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(budget => budget.Value);
        //                                                thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
        //                                                newValue = Convert.ToDouble(arrBudgetInputValues[i]);

        //                                                if (thisQuarterTotalBudget != newValue)
        //                                                {
        //                                                    BudgetDiff = newValue - thisQuarterTotalBudget;
        //                                                    if (BudgetDiff > 0)
        //                                                    {
        //                                                        thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
        //                                                        db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        j = 1;
        //                                                        while (BudgetDiff < 0)
        //                                                        {
        //                                                            if (thisQuarterFirstMonthBudget != null)
        //                                                            {
        //                                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

        //                                                                if (BudgetDiff <= 0)
        //                                                                    thisQuarterFirstMonthBudget.Value = 0;
        //                                                                else
        //                                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;

        //                                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                            }
        //                                                            if ((QuarterCnt + j) <= (QuarterCnt + 2))
        //                                                            {
        //                                                                thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
        //                                                            }

        //                                                            j = j + 1;
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
        //                                            }
        //                                            isExists = true;
        //                                        }
        //                                    }
        //                                    //// if Old budget value does not exist then insert new value to table.
        //                                    if (!isExists && arrBudgetInputValues[i] != "")
        //                                    {
        //                                        // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
        //                                        objPlanCampaignBudget = new Plan_Campaign_Budget();
        //                                        objPlanCampaignBudget.PlanCampaignId = form.PlanCampaignId;
        //                                        objPlanCampaignBudget.Period = PeriodChar + QuarterCnt;
        //                                        objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                        objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
        //                                        objPlanCampaignBudget.CreatedDate = DateTime.Now;
        //                                        db.Entry(objPlanCampaignBudget).State = EntityState.Added;
        //                                    }
        //                                    QuarterCnt = QuarterCnt + 3;
        //                                }
        //                            }
        //                        }
        //                    }

        //                    db.SaveChanges();
        //                    scope.Complete();
        //                    string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
        //                    return Json(new { IsSaved = true, msg = strMessage, planCampaignId = form.PlanCampaignId, JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }

        //    return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        //}

        #endregion
        #endregion

        #region "Program related Functions"

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Program Setup Tab.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupProgram(int id)
        {
            InspectModel _inspectmodel;
            //// Load Inspect Model data.
            //if (TempData["ProgramModel"] != null)
            //{
            //    _inspectmodel = (InspectModel)TempData["ProgramModel"];
            //}
            //else
            //{
            _inspectmodel = GetInspectModel(id, "program", false);
            //  }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.ProgramDetail = _inspectmodel;
            ViewBag.OwnerName = _inspectmodel.Owner;
            if (_inspectmodel.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = Common.objCached.LastSynced.Replace("{0}", Common.GetFormatedDate(_inspectmodel.LastSyncDate)); ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }
            double? objPlanProgramBudget = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == id).FirstOrDefault().ProgramBudget;

            List<Plan_Tactic_Values> lstTacticValues = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.PlanProgramId == id && t.IsDeleted == false).ToList());

            ViewBag.MQLs = lstTacticValues.Sum(tm => tm.MQL);
            ViewBag.Cost = Common.CalculateProgramCost(id); //pcp.Cost; modified for PL #440 by dharmraj 

            //Added By : Kalpesh Sharma : PL #605 : 07/29/2014
            //List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanProgramId == id && t.IsDeleted == false).ToList());
            ViewBag.Revenue = Math.Round(lstTacticValues.Sum(tm => tm.Revenue));


            ViewBag.ProgramBudget = objPlanProgramBudget != null ? objPlanProgramBudget : 0;

            return PartialView("_SetupProgram", _inspectmodel);
        }

        /// <summary>
        /// Action to Load Program Setup Tab in Edit Mode with data.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public PartialViewResult LoadSetupProgramEdit(int id = 0)
        {
            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }
            ViewBag.IsCreated = false;
            ViewBag.RedirectType = false;
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pcp.Plan_Campaign.Plan.Model);

            #region "Set Plan_Campaign_ProgramModel to pass into partialview"
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanProgramId = pcp.PlanProgramId;
            pcpm.PlanCampaignId = pcp.PlanCampaignId;
            pcpm.Title = HttpUtility.HtmlDecode(pcp.Title);
            pcpm.Description = HttpUtility.HtmlDecode(pcp.Description);
            pcpm.OwnerId = pcp.CreatedBy;
            pcpm.StartDate = pcp.StartDate;
            pcpm.EndDate = pcp.EndDate;
            pcpm.CStartDate = pcp.Plan_Campaign.StartDate;
            pcpm.CEndDate = pcp.Plan_Campaign.EndDate;
            pcpm.Status = pcp.Status;
            List<Plan_Campaign_Program_Tactic> lstTactic = (from tac in db.Plan_Campaign_Program_Tactic where tac.PlanProgramId == id && tac.IsDeleted.Equals(false) select tac).ToList();
            if (lstTactic != null && lstTactic.Count() > 0)
            {
                pcpm.TStartDate = (from otsd in lstTactic select otsd.StartDate).Min();
                pcpm.TEndDate = (from otsd in lstTactic select otsd.EndDate).Max();
            }

            List<Plan_Tactic_Values> lstPlanTacticValues = Common.GetMQLValueTacticList(lstTactic);
            pcpm.MQLs = lstPlanTacticValues.Sum(tm => tm.MQL);
            pcpm.Cost = Common.CalculateProgramCost(pcp.PlanProgramId);

            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcp.Plan_Campaign.Title);

            //List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == pcp.PlanCampaignId &&
            //    t.Plan_Campaign_Program.PlanProgramId == pcp.PlanProgramId && t.IsDeleted == false).ToList());
            pcpm.Revenue = Math.Round(lstPlanTacticValues.Sum(tm => tm.Revenue));

            pcpm.ProgramBudget = pcp.ProgramBudget;
            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == pcp.Plan_Campaign.PlanId);
            pcpm.AllocatedBy = objPlan.AllocatedBy;

            pcpm.IsDeployedToIntegration = pcp.IsDeployedToIntegration;

            #endregion

            if (Sessions.User.UserId == pcp.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcp.Plan_Campaign.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Year = objPlan.Year;

            //Verify that existing user has created activity or it has subordinate permission and activity owner is subordinate of existing user
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            List<Guid> lstSubordinatesIds = new List<Guid>();
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            }

            if (lstSubordinatesIds.Contains(pcp.CreatedBy))
            {
                ViewBag.IsAuthorized = true;
            }

            var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == pcp.PlanCampaignId);
            double lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == pcp.PlanCampaignId && p.IsDeleted == false).ToList().Sum(c => c.ProgramBudget);
            ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - lstSelectedProgram);
            //Added by Komal Rawal for #711
            ViewBag.IsProgramEdit = true;
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();


                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); //PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                if (lstClientUsers.Count() > 0)
                {

                    ViewBag.IsServiceUnavailable = false;
                    ViewBag.OwnerName = Common.GetUserName(pcp.CreatedBy.ToString());


                    string strUserList = string.Join(",", lstClientUsers);
                    //List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    //lstUserDetails = lstUserDetails.Where(i => !i.IsDeleted).ToList();
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        ViewBag.OwnerList = lstPreparedOwners;
                    }
                    else
                    {
                        ViewBag.OwnerList = new List<User>();
                    }
                }
                else
                {
                    ViewBag.OwnerList = new List<User>();
                }
            }

            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = true;

                }
            }




            return PartialView("_EditSetupProgram", pcpm);
        }

        /// <summary>
        /// Action to Save/Update Program Setup Tab Data.
        /// </summary>
        /// <param name="form">Form data.</param>
        /// <param name="customFieldInputs">CustomFields.</param>
        /// <param name="UserId">UserId.</param>
        /// <param name="title">Title of Program.</param>
        /// <returns>Returns Save/Error message.</returns>
        [HttpPost]
        public ActionResult SetupSaveProgram(Plan_Campaign_ProgramModel form, string customFieldInputs, string UserId = "", string title = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    return Json(new { IsSaved = false, errormsg = Common.objCached.LoginWithSameSession }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                //Deserialize customFieldInputs json string to  KeyValuePair List
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);
                int campaignId = form.PlanCampaignId;

                int planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == campaignId && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();

                //// if programId null then insert new record.
                if (form.PlanProgramId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record.
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcp.IsDeleted.Equals(false)
                                          && pc.PlanCampaignId == form.PlanCampaignId       //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            //// if duplicate record exist then return with duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsSaved = false, errormsg = strDuplicateMessage, PlanId = planid }); //Modified By Komal Rawal for #2174 to get correct PlanId
                            }
                            else
                            {

                                #region "Insert new record to Plan_Campaign_Program table"
                                Plan_Campaign_Program pcpobj = new Plan_Campaign_Program();
                                pcpobj.PlanCampaignId = form.PlanCampaignId;
                                pcpobj.Title = form.Title;
                                pcpobj.Description = form.Description;
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;
                                //pcpobj.CreatedBy = Sessions.User.UserId;  // Commented by Rahul Shah on 17/03/2016 for PL #2032 
                                pcpobj.CreatedBy = form.OwnerId;            // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                pcpobj.CreatedDate = DateTime.Now;
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); //status field added for Plan_Campaign_Program table
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.ProgramBudget = form.ProgramBudget;
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                #endregion

                                #region "Insert custom field records to CustomField_Entity table"
                                int programid = pcpobj.PlanProgramId;
                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = programid;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                #endregion

                                #region " Set campaign Start and End Date"
                                Plan_Campaign pcp = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(pcpobj.PlanCampaignId) && pcobj.IsDeleted.Equals(false)).FirstOrDefault();
                                if (pcp != null)
                                {
                                    if (pcp.StartDate > form.StartDate)
                                    {
                                        pcp.StartDate = form.StartDate;
                                    }

                                    if (form.EndDate > pcp.EndDate)
                                    {
                                        pcp.EndDate = form.EndDate;
                                    }
                                    db.Entry(pcp).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                }
                                // End - Added by Sohel Pathan on 09/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen 
                                #endregion

                                result = Common.InsertChangeLog(planid, null, programid, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                #region "Send Email Notification For Owner changed"
                                if (result > 0)
                                {
                                    // Add By Nishant Sheth
                                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                    DataSet dsPlanCampProgTac = new DataSet();
                                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != Sessions.User.UserId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", Sessions.User.UserId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.
                                                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                                                    //// Ticket: 942 Exception handeling in Gameplan.
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcpobj.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = pcpobj.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = pcpobj.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign.PlanId, programid, Enums.Section.Program.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcpobj.Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Program.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }
                                    }
                                }
                                #endregion
                                Common.ChangeCampaignStatus(pcpobj.PlanCampaignId, false);     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { IsSaved = true, Msg = strMessage, programID = programid, campaignID = campaignId, PlanId = planid }, JsonRequestBehavior.AllowGet); //Modified By Komal Rawal for #2174 to get correct PlanId
                            }
                        }
                    }
                }
                else    //// if programId not null then update record.
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
                        var TransactionOption = new System.Transactions.TransactionOptions();
                        TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

                        using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
                        // using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record.
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(form.PlanProgramId) && pcp.IsDeleted.Equals(false)
                                          && pc.PlanCampaignId == form.PlanCampaignId
                                          select pcp).FirstOrDefault();
                            //// if duplicate record exist then return with duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsSaved = false, errormsg = strDuplicateMessage, PlanId = planid }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                #region "Update record to Plan_Campaign_Program table"
                                Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(form.PlanProgramId)).FirstOrDefault();
                                // Add By Nishant Sheth
                                // Desc::#1765 - To remove pervious data from db if end date year difference is less then to compare end date.
                                int EndDateYear = pcpobj.EndDate.Year;
                                int FormEndDateYear = form.EndDate.Year;
                                int EndDatediff = EndDateYear - FormEndDateYear;
                                if (EndDatediff > 0)
                                {
                                    listMonthDynamic lstMonthlyDynamic = new listMonthDynamic();

                                    List<string> lstMonthlyExtended = new List<string>();
                                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(pcpobj.StartDate.Year));
                                    string periodPrefix = "Y";
                                    int baseYear = 0;
                                    for (int i = 0; i < (YearDiffrence + 1); i++)
                                    {
                                        for (int j = 1; j <= 12; j++)
                                        {
                                            lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                                        }
                                        baseYear = baseYear + 12;
                                    }
                                    lstMonthlyDynamic.Id = pcpobj.PlanProgramId;
                                    lstMonthlyDynamic.listMonthly = lstMonthlyExtended.AsEnumerable().Reverse().ToList();

                                    List<string> deleteperiodmonth = new List<string>();
                                    for (int i = 0; i < EndDatediff; i++)
                                    {
                                        var listofperiod = lstMonthlyDynamic.listMonthly.Skip(i * 12).Take(12).ToList();
                                        listofperiod.ForEach(a => { deleteperiodmonth.Add(a); });
                                    }

                                    var listBudget = db.Plan_Campaign_Program_Budget.Where(a => a.PlanProgramId == pcpobj.PlanProgramId && deleteperiodmonth.Contains(a.Period)).ToList();
                                    listBudget.ForEach(a => { db.Entry(a).State = EntityState.Deleted; });
                                }
                                pcpobj.Title = title;
                                Guid oldOwnerId = pcpobj.CreatedBy;
                                pcpobj.Description = form.Description;
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;
                                if (form.CStartDate > form.StartDate)
                                {
                                    pcpobj.Plan_Campaign.StartDate = form.StartDate;
                                }
                                if (form.EndDate > form.CEndDate)
                                {
                                    pcpobj.Plan_Campaign.EndDate = form.EndDate;
                                }
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                pcpobj.ProgramBudget = form.ProgramBudget;
                                pcpobj.CreatedBy = form.OwnerId;
                                db.Entry(pcpobj).State = EntityState.Modified;
                                #endregion

                                #region "Save Custom fields to CustomField_Entity table"
                                //// Delete previous custom fields values.
                                string entityTypeProgram = Enums.EntityType.Program.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(c => c.EntityId == form.PlanProgramId && c.CustomField.EntityType == entityTypeProgram).ToList();
                                prevCustomFieldList.ForEach(c => db.Entry(c).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = form.PlanProgramId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;
                                    }
                                }
                                int result = db.SaveChanges();
                                #endregion

                                result = Common.InsertChangeLog(planid, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                                // Start - Added by Pratik on 11/03/2014 for PL ticket #711
                                if (result > 0)
                                {
                                    // Add By Nishant Sheth
                                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                    DataSet dsPlanCampProgTac = new DataSet();
                                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != oldOwnerId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", oldOwnerId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.
                                                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                                                    //// Ticket: 942 Exception handeling in Gameplan.
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcpobj.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = pcpobj.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = pcpobj.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign.PlanId, form.PlanProgramId, Enums.Section.Program.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcpobj.Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Program.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }
                                    }
                                }
                                // End - Added by Pratik on 11/03/2014 for PL ticket #711

                                if (result >= 1)
                                {
                                    Common.ChangeCampaignStatus(pcpobj.PlanCampaignId, false);
                                    scope.Complete();
                                    string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { IsSaved = true, Msg = strMessage, campaignID = campaignId, EndDatediff = EndDatediff, PlanId = planid }, JsonRequestBehavior.AllowGet);// Modified By Nishant Sheth Desc:: #1812 refresh time frame dropdown
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, errormsg = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Program Review Tab.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReviewProgram(int id)
        {
            InspectModel im;
            //// Load Inspect Model data.
            //if (TempData["ProgramModel"] != null)
            //{
            //    im = (InspectModel)TempData["ProgramModel"];
            //}
            //else
            //{
            im = GetInspectModel(id, Convert.ToString(Enums.Section.Program).ToLower(), false);
            //  }

            //// Get Tactic comments list by PlanProgramId.
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanProgramId == id && tc.PlanProgramId.HasValue
                                 select tc).ToArray();

            //// Get Userslist using Tactic comment list.
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in tacticComment select ta.CreatedBy).ToList<Guid>();
            userListId.Add(im.OwnerId);
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Load InspectReviewModel to ViewBag.
            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanProgramId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanProgramId = Convert.ToInt32(tc.PlanProgramId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Any() ? userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault() : Common.GameplanIntegrationService,
                                       CreatedBy = tc.CreatedBy
                                   }).OrderByDescending(x => x.CommentDate).ToList(); //Modified By komal Rawal for 2043 resort comment in desc order

            //// Set Owner name to InspectModel.
            var ownername = (from u in userName
                             where u.UserId == im.OwnerId
                             select u.FirstName + " " + u.LastName).FirstOrDefault();
            if (ownername != null)
            {
                im.Owner = ownername.ToString();
            }
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Campaign_Program_Tactic> PlanTacticIds = db.Plan_Campaign_Program_Tactic.Where(ppt => ppt.PlanProgramId == id && ppt.IsDeleted == false).ToList();
            im.MQLs = Common.GetMQLValueTacticList(PlanTacticIds).Sum(t => t.MQL);

            #region "Load ViewBags"
            ViewBag.ProgramDetail = im;

            bool isValidOwner = false;
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = new List<Guid>();
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(im.OwnerId) && Common.IsSectionApprovable(lstSubOrdinatesPeers, id, Enums.Section.Program.ToString()))////Modified by Sohel Pathan for PL ticket #688 and #689
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Modified by komal Rawal for #1158
            bool IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);

            if (IsCommentsViewEditAuthorized)
            {


                List<int> lstAllowedPermissionids = new List<int>();
                List<int> planTacticIds = new List<int>();
                planTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && tactic.Plan_Campaign_Program.PlanProgramId == id)
                                                                                  .Select(tactic => tactic.PlanTacticId).ToList();
                lstAllowedPermissionids = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                if (lstAllowedPermissionids.Count != planTacticIds.Count)
                {
                    IsCommentsViewEditAuthorized = false;
                }

            }
            ViewBag.IsCommentsViewEditAuthorized = IsCommentsViewEditAuthorized;
            // End

            #endregion
            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537


            //Get all subordinates of current user upto n level
            var lstSubOrdinates = new List<Guid>();
            try
            {
                lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            bool IsProgramEditable = false;
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);

            if (im.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsProgramEditable = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsProgramEditable = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsProgramEditable = true;
                }
            }

            ViewBag.IsProgramEditable = IsProgramEditable;

            #region "Show Sync button in Review page only Integration InstanceType "Salesforce"
            bool isInstanceSalesforce = false;
            if (im.IsDeployedToIntegration && im.IsIntegrationInstanceExist == "Yes")
            {
                //modified by Rahul shah On 10/11/2015 for Pl #1630
                string integrationType = string.Empty;
                int? integrationInstanceId = db.Plan_Campaign.FirstOrDefault(t => t.PlanCampaignId == im.PlanCampaignId).Plan.Model.IntegrationInstanceId;
                if (integrationInstanceId != null)
                {
                    integrationType = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == integrationInstanceId).IntegrationType.Code;
                }
                if (integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
                {
                    isInstanceSalesforce = true;
                    #region "Set Program Last Synced Timestamp to ViewBag variable"
                    string programEntityType = Enums.Section.Program.ToString();
                    var planEntityLogList = db.IntegrationInstancePlanEntityLogs.Where(ipt => ipt.EntityId == im.PlanProgramId && ipt.EntityType == programEntityType).ToList();
                    if (planEntityLogList.Any())
                    {
                        ViewBag.ProgramLastSync = planEntityLogList.OrderByDescending(log => log.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
                    }
                    #endregion
                }
            }
            ViewBag.IsInstanceSalesfore = isInstanceSalesforce;
            #endregion

            return PartialView("_ReviewProgram");
        }

        #region Budget Allocation in Program Tab
        ///// <summary>
        ///// Load the Program Budget Allocation values.
        ///// </summary>
        ///// <param name="id">Program Id</param>
        ///// <returns></returns>
        //public PartialViewResult LoadSetupProgramBudget(int id = 0)
        //{
        //    Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
        //    if (pcp == null)
        //    {
        //        return null;
        //    }

        //    #region "Set Plan_Campaign_ProgramModel to pass into partialview"
        //    Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
        //    pcpm.PlanProgramId = pcp.PlanProgramId;
        //    pcpm.PlanCampaignId = pcp.PlanCampaignId;

        //    pcpm.ProgramBudget = pcp.ProgramBudget;
        //    // Add by Nishant sheth
        //    // Desc :: #1765 - to get the year diffrence between item start date and end date
        //    ViewBag.YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcp.EndDate.Year) - Convert.ToInt32(pcp.StartDate.Year));
        //    ViewBag.StartYear = Convert.ToInt32(pcp.StartDate.Year);

        //    var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == pcp.Plan_Campaign.PlanId);
        //    pcpm.AllocatedBy = objPlan.AllocatedBy;
        //    #endregion

        //    #region "Calculate Plan Remaining Budget"
        //    var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == pcp.PlanCampaignId);
        //    var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == pcp.PlanCampaignId && p.IsDeleted == false).ToList();
        //    double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
        //    ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);
        //    #endregion

        //    return PartialView("_SetupProgramBudget", pcpm);
        //}

        ///// <summary>
        ///// Action to Save Program.
        ///// </summary>
        ///// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        ///// <param name="BudgetInputValues">Budget allocation inputs values.</param>
        ///// <param name="title"></param>
        ///// <param name="UserId"></param>
        ///// <returns>Returns Action Result.</returns>
        //[HttpPost]
        //public ActionResult SaveProgramBudgetAllocation(Plan_Campaign_ProgramModel form, string BudgetInputValues, string UserId = "", string title = "", string AllocatedBy = "", int YearDiffrence = 0)
        //{
        //    //// check whether UserId is loggined user or not.
        //    if (!string.IsNullOrEmpty(UserId))
        //    {
        //        if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
        //        {
        //            return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    try
        //    {
        //        using (MRPEntities mrp = new MRPEntities())
        //        {
        //            //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
        //            var TransactionOption = new System.Transactions.TransactionOptions();
        //            TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

        //            using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
        //            // using (var scope = new TransactionScope())
        //            {
        //                //// Get duplicate record.
        //                var pcpvar = (from pcp in db.Plan_Campaign_Program
        //                              join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
        //                              where pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(form.PlanProgramId) && pcp.IsDeleted.Equals(false)
        //                              && pc.PlanCampaignId == form.PlanCampaignId   //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
        //                              select pcp).FirstOrDefault();
        //                //// if duplicate record exist then return with duplication message.
        //                if (pcpvar != null)
        //                {
        //                    string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
        //                    return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    #region "Update record to Plan_Campaign_Program table"
        //                    Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(form.PlanProgramId)).FirstOrDefault();
        //                    string[] arrBudgetInputValues = BudgetInputValues.Split(',');
        //                    pcpobj.ModifiedBy = Sessions.User.UserId;
        //                    pcpobj.ModifiedDate = DateTime.Now;
        //                    pcpobj.Title = title;
        //                    pcpobj.ProgramBudget = form.ProgramBudget;
        //                    #endregion

        //                    //Start added by Kalpesh  #608: Budget allocation for Program
        //                    //// Get Previous budget allocation list.
        //                    List<Plan_Campaign_Program_Budget> PrevAllocationList = db.Plan_Campaign_Program_Budget.Where(c => c.PlanProgramId == form.PlanProgramId).Select(c => c).ToList();    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

        //                    //// Process for Monthly budget values.
        //                    // Change by Nishant sheth
        //                    // Desc :: #1765 - to replace the lenth of array to allocated by
        //                    if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                    {
        //                        bool isExists = false;
        //                        Plan_Campaign_Program_Budget objPlanCampaignProgramBudget, updatePlanProgramBudget;
        //                        double newValue = 0;
        //                        for (int i = 0; i < arrBudgetInputValues.Length; i++)
        //                        {
        //                            // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
        //                            isExists = false;
        //                            if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                            {
        //                                //// Get previous campaign budget values by Period.
        //                                updatePlanProgramBudget = new Plan_Campaign_Program_Budget();
        //                                updatePlanProgramBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
        //                                if (updatePlanProgramBudget != null)
        //                                {
        //                                    if (arrBudgetInputValues[i] != "")
        //                                    {
        //                                        //// Update budget value with old value.
        //                                        newValue = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                        if (updatePlanProgramBudget.Value != newValue)
        //                                        {
        //                                            updatePlanProgramBudget.Value = newValue;
        //                                            db.Entry(updatePlanProgramBudget).State = EntityState.Modified;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        db.Entry(updatePlanProgramBudget).State = EntityState.Deleted;
        //                                    }
        //                                    isExists = true;
        //                                }
        //                            }
        //                            //// if Old budget value does not exist then insert new value to table.
        //                            if (!isExists && arrBudgetInputValues[i] != "")
        //                            {
        //                                // End - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
        //                                objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
        //                                objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
        //                                objPlanCampaignProgramBudget.Period = PeriodChar + (i + 1);
        //                                objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
        //                                objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
        //                                db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
        //                            }
        //                        }
        //                    }
        //                    // Change by Nishant sheth
        //                    // Desc :: #1765 - to replace the lenth of array to allocated by
        //                    else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower()) //// Process for Quarterly budget values.
        //                    {
        //                        int BudgetInputValuesCounter = 1;
        //                        int m = 0;
        //                        for (int k = 1; k <= (YearDiffrence + 1); k++)
        //                        {
        //                            bool isExists = false;
        //                            List<Plan_Campaign_Program_Budget> thisQuartersMonthList;
        //                            Plan_Campaign_Program_Budget thisQuarterFirstMonthBudget, objPlanCampaignProgramBudget;
        //                            double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
        //                            int j;


        //                            for (int i = m; i < (4 * k); i++)
        //                            {
        //                                if ((i + 1) % 4 == 0)
        //                                {
        //                                    m = i + 1;
        //                                }
        //                                // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
        //                                isExists = false;
        //                                if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                                {
        //                                    //// Get Quarter budget list.
        //                                    thisQuartersMonthList = new List<Plan_Campaign_Program_Budget>();
        //                                    thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 1)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 2))).ToList().OrderBy(a => a.Period).ToList();

        //                                    //// Get First month values from Quarterly budget list.
        //                                    thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Budget();
        //                                    thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

        //                                    if (thisQuarterFirstMonthBudget != null)
        //                                    {
        //                                        if (arrBudgetInputValues[i] != "")
        //                                        {
        //                                            thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
        //                                            thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
        //                                            newValue = Convert.ToDouble(arrBudgetInputValues[i]);

        //                                            if (thisQuarterTotalBudget != newValue)
        //                                            {
        //                                                BudgetDiff = newValue - thisQuarterTotalBudget;
        //                                                if (BudgetDiff > 0)
        //                                                {
        //                                                    thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
        //                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                }
        //                                                else
        //                                                {
        //                                                    j = 1;
        //                                                    while (BudgetDiff < 0)
        //                                                    {
        //                                                        if (thisQuarterFirstMonthBudget != null)
        //                                                        {
        //                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

        //                                                            if (BudgetDiff <= 0)
        //                                                                thisQuarterFirstMonthBudget.Value = 0;
        //                                                            else
        //                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

        //                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                        }
        //                                                        if ((BudgetInputValuesCounter + j) <= (BudgetInputValuesCounter + 2))
        //                                                        {
        //                                                            thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter + j))).FirstOrDefault();
        //                                                        }

        //                                                        j = j + 1;
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
        //                                        }
        //                                        isExists = true;
        //                                    }
        //                                }
        //                                //// if Old budget value does not exist then insert new value to table.
        //                                if (!isExists && arrBudgetInputValues[i] != "")
        //                                {
        //                                    // End - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
        //                                    objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
        //                                    objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
        //                                    objPlanCampaignProgramBudget.Period = PeriodChar + BudgetInputValuesCounter;
        //                                    objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                    objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
        //                                    objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
        //                                    db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
        //                                }
        //                                BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
        //                            }
        //                        }
        //                    }

        //                    db.Entry(pcpobj).State = EntityState.Modified;
        //                    int result = db.SaveChanges();
        //                    int planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == form.PlanCampaignId && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
        //                    result = Common.InsertChangeLog(planid, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
        //                    scope.Complete();
        //                    string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
        //                    return Json(new { IsSaved = true, msg = strMessage, JsonRequestBehavior.AllowGet, PlanProgramId = form.PlanProgramId, PlanCampaignId = form.PlanCampaignId });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }

        //    return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        //}
        #endregion

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Create Program.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns Partial View Of Program.</returns>
        public ActionResult CreateProgram(int id = 0)
        {
            Plan_Campaign pcp = db.Plan_Campaign.Where(pcpobj => pcpobj.PlanCampaignId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }
            //// Set External Integration Service by PlanModel
            var objPlan = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == id).FirstOrDefault().Plan;
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

            ViewBag.IsCreated = true;
            ViewBag.CampaignTitle = pcp.Title;

            ViewBag.OwnerName = Sessions.User.FirstName + " " + Sessions.User.LastName;

            #region "Set Plan_Campaign_ProgramModel to pass into Partialview"
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanCampaignId = id;
            pcpm.IsDeployedToIntegration = false;
            pcpm.StartDate = pcp.StartDate;
            pcpm.EndDate = pcp.EndDate;
            //pcpm.StartDate = GetCurrentDateBasedOnPlan();
            //pcpm.EndDate = GetCurrentDateBasedOnPlan(true);
            pcpm.CStartDate = pcp.StartDate;
            pcpm.CEndDate = pcp.EndDate;
            pcpm.ProgramBudget = 0;
            pcpm.AllocatedBy = objPlan.AllocatedBy;
            pcpm.OwnerId = Sessions.User.UserId;
            #endregion

            ViewBag.IsOwner = true;
            ViewBag.RedirectType = false;
            ViewBag.Year = db.Plans.Single(plan => plan.PlanId.Equals(objPlan.PlanId)).Year;//Sessions.PlanId

            #region "Calculate Plan Remaining Budget"
            var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == id);
            var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == id && p.IsDeleted == false).ToList();
            double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
            ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);
            #endregion
            // Added by Rahul Shah on 17/03/2016 for PL #2032 
            ViewBag.IsProgramEdit = true;
            #region "Owner list"
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); //PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                if (lstClientUsers.Count() > 0)
                {
                    ViewBag.IsServiceUnavailable = false;
                    ViewBag.OwnerName = Common.GetUserName(pcp.CreatedBy.ToString());
                    string strUserList = string.Join(",", lstClientUsers);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        ViewBag.OwnerList = lstPreparedOwners;
                    }
                    else
                    {
                        ViewBag.OwnerList = new List<User>();
                    }
                }
                else
                {
                    ViewBag.OwnerList = new List<User>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    ViewBag.IsServiceUnavailable = true;
                }
            }
            #endregion
            return PartialView("_EditSetupProgram", pcpm);
        }
        #endregion

        #region "Tactic related Functions"
        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Setup Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="Mode"></param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetup(int id)
        {
            InspectModel _inspetmodel;
            //// Load InspectModel data.
            _inspetmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);

            try
            {
                _inspetmodel.Owner = Common.GetUserName(_inspetmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ViewBag.TacticDetail = _inspetmodel;
            ViewBag.IsTackticAddEdit = false;

            /* Added by Mitesh Vaishnav for PL ticket #1143
             Add number of stages for advance/Basic attributes waightage related to tacticType*/
            string entityType = Enums.Section.Tactic.ToString();
            /*Get existing value of Advance/Basic waightage of tactic's attributes*/
            string customFieldType = Enums.CustomFieldType.DropDownList.ToString();
            var customFieldEntity = (from customentity in db.CustomField_Entity
                                     where customentity.EntityId == id &&
                                     customentity.CustomField.EntityType == entityType &&
                                     customentity.CustomField.CustomFieldType.Name == customFieldType
                                     select customentity).ToList();
            var customfieldids = customFieldEntity.Select(customentity => customentity.CustomFieldId).ToList();
            var customOptionFieldList = db.CustomFieldOptions.Where(option => customfieldids.Contains(option.CustomFieldId) && option.IsDeleted == false).ToList().Select(option => option.CustomFieldOptionId.ToString()).ToList();

            var customFeildsWeightage = customFieldEntity.Where(cfs => customOptionFieldList.Contains(cfs.Value)).Select(cfs => new
            {
                optionId = cfs.Value,
                CostWeight = cfs.CostWeightage,
                Weight = cfs.Weightage
            }).ToList();
            ViewBag.customFieldWeightage = JsonConvert.SerializeObject(customFeildsWeightage);
            /*End : Added by Mitesh Vaishnav for PL ticket #1143*/
            return PartialView("SetUp", _inspetmodel);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Review Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReview(int id)
        {
            InspectModel _inspectmodel;
            //// Load InspectModel data.
            _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);

            //// Get Tactic comment by PlanCampaignId from Plan_Campaign_Program_Tactic_Comment table.
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanTacticId == id
                                 select tc).ToArray();

            //// Get Users list.
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in tacticComment select ta.CreatedBy).ToList<Guid>();
            userListId.Add(_inspectmodel.OwnerId);
            string userList = string.Join(",", userListId.Select(userid => userid.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanTacticId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanTacticId = Convert.ToInt32(tc.PlanTacticId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Any() ? userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault() : Common.GameplanIntegrationService,
                                       CreatedBy = tc.CreatedBy
                                   }).OrderByDescending(x => x.CommentDate).ToList(); //Modified By komal Rawal for 2043 resort comment in desc order

            //// Get Owner name by OwnerId from Username list.
            var ownername = (from user in userName
                             where user.UserId == _inspectmodel.OwnerId
                             select user.FirstName + " " + user.LastName).FirstOrDefault();
            if (ownername != null)
            {
                _inspectmodel.Owner = ownername.ToString();
            }

            //// Calculate MQL at runtime 
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            int MQLStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == TitleMQL && stage.IsDeleted == false).Level);
            //Compareing MQL stage level with tactic stage level
            if (_inspectmodel.StageLevel < MQLStageLevel)
            {
                ViewBag.ShowMQL = true;
            }
            else
            {
                ViewBag.ShowMQL = false;
            }

            ViewBag.TacticDetail = _inspectmodel;
            ViewBag.IsModelDeploy = _inspectmodel.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            bool isValidOwner = false;
            bool isEditable = false;
            if (_inspectmodel.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
                isEditable = true;
            }
            ViewBag.IsValidOwner = isValidOwner;
            /*Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model*/
            var pcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId.Equals(id)).FirstOrDefault();
            var tacticType = db.TacticTypes.Where(tt => tt.TacticTypeId == pcpt.TacticTypeId).FirstOrDefault();
            if (pcpt.StageId == tacticType.StageId)
            {
                ViewBag.IsDiffrentStageType = false;
            }
            else
            {
                ViewBag.IsDiffrentStageType = true;
            }
            /*End Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model */

            // To get permission status for Approve tactic , By dharmraj PL #538
            var lstSubOrdinatesPeers = new List<Guid>();
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(_inspectmodel.OwnerId))
            {
                isValidManagerUser = true;

            }
            ViewBag.IsValidManagerUser = isValidManagerUser;



            // Modified by komal Rawal for #1158
            bool IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);

            if (IsCommentsViewEditAuthorized)
            {
                List<int> lstAllowedPermissionids = new List<int>();
                List<int> planTacticIds = new List<int>();
                //planTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == id).Select(tactic => tactic.PlanTacticId).ToList();
                planTacticIds.Add(pcpt.PlanTacticId);
                lstAllowedPermissionids = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                if (lstAllowedPermissionids.Count != planTacticIds.Count)
                {
                    IsCommentsViewEditAuthorized = false;
                }

            }
            ViewBag.IsCommentsViewEditAuthorized = IsCommentsViewEditAuthorized;

            // End

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = new List<Guid>();
            try
            {
                lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                if (lstSubOrdinates.Contains(_inspectmodel.OwnerId))
                {
                    isEditable = true;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            if (isEditable && !isValidOwner)
            {
                List<int> planTacticIds = new List<int>();
                List<int> lstAllowedEntityIds = new List<int>();

                //planTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == id).Select(tactic => tactic.PlanTacticId).ToList();
                planTacticIds.Add(pcpt.PlanTacticId);
                lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                if (lstAllowedEntityIds.Count != planTacticIds.Count)
                {
                    isEditable = false;
                }

            }
            ViewBag.IsEditable = isEditable;

            bool IsDeployToIntegrationVisible = false;
            if (_inspectmodel.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(_inspectmodel.OwnerId))
                {
                    IsDeployToIntegrationVisible = true;
                }
            }

            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;

            // Start - Added by Viral Kadiya on 22nd Jan 2016 for Pl ticket #1919.
            bool isSyncSF = false, isSyncEloqua = false, isSyncWorkFront = false, isSyncMarketo = false; //added isSyncWorkFront - 22 Jan 2016 Brad Gray PL#1922
            if (IsDeployToIntegrationVisible)
            {
                if (pcpt.IsSyncSalesForce != null && pcpt.IsSyncSalesForce.HasValue && pcpt.IsSyncSalesForce.Value)     // Get IsSyncSalesforce flag
                    isSyncSF = true;
                if (pcpt.IsSyncEloqua != null && pcpt.IsSyncEloqua.HasValue && pcpt.IsSyncEloqua.Value)                 // Get IsSyncEloqua flag
                    isSyncEloqua = true;
                if (pcpt.IsSyncWorkFront != null && pcpt.IsSyncWorkFront.HasValue && pcpt.IsSyncWorkFront.Value)        // Get IsSyncWorkFront flag - added 22 Jan 2016 Brad Gray PL#1922
                {
                    isSyncWorkFront = true;
                }
                if (pcpt.IsSyncMarketo != null && pcpt.IsSyncMarketo.HasValue && pcpt.IsSyncMarketo.Value)              // Get IsSyncMarketo flag
                    isSyncMarketo = true;
            }
            ViewBag.IsSyncSF = isSyncSF;
            ViewBag.IsSyncEloqua = isSyncEloqua;
            ViewBag.IsSyncMarketo = isSyncMarketo;
            ViewBag.IsSyncWorkFront = isSyncWorkFront; //added 22 Jan 2016 Brad Gray PL#1922
            // End - Added by Viral Kadiya on 22nd Jan 2016 for Pl ticket #1919.

            //add the appropriate WorkFront information to the model - PL#1922, 1872 - Brad Gray 24 Jan 2016

            //setup WorkFront information to keep from making redundant calls;
            IntegrationWorkFrontTacticSetting tSetting;
            IntegrationWorkFrontRequest tRequest;
            if (isSyncWorkFront)
            {
                tSetting = db.IntegrationWorkFrontTacticSettings.Where(s => s.TacticId == pcpt.PlanTacticId && s.IsDeleted == false).FirstOrDefault();
                tRequest = db.IntegrationWorkFrontRequests.Where(a => a.IntegrationInstanceId == pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt
                        && a.PlanTacticId == pcpt.PlanTacticId && a.IsDeleted == false).FirstOrDefault();


                if (tSetting != null)
                {
                    _inspectmodel.WorkFrontTacticApprovalBehavior = tSetting.TacticApprovalObject;
                }
                if (_inspectmodel.WorkFrontTacticApprovalBehavior == Integration.Helper.Enums.WorkFrontTacticApprovalObject.Request.ToString())
                {
                    if (tRequest != null)
                    {
                        _inspectmodel.WorkFrontRequestQueueId = tRequest.QueueId;
                        _inspectmodel.WorkFrontRequestAssignee = tRequest.WorkFrontUserId;
                    }
                }
            }
            else
            {
                tSetting = null;
                tRequest = null;
            }

            ///Begin Added by Brad Gray 08-10-2015 for PL#1462
            Dictionary<string, string> IntegrationLinkDictionary = new Dictionary<string, string>();

            //provide a list of tactic integration Id and workfront project 
            List<IntegrationInstance> modelIntegrationList = new List<IntegrationInstance>();
            if (pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance != null && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance.IsDeleted == false) { modelIntegrationList.Add(pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance); }

            //integrationinstance - Push Tactic Data Salesforce
            //integrationinstance11 - Push Tactic Data Eloqua
            //integrationinstance4 - Project Management
            if (pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance11 != null && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance11.IsDeleted == false) { modelIntegrationList.Add(pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance11); }
            // Instance Marketo
            if (pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance41 != null && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance41.IsDeleted == false)
            {

                modelIntegrationList.Add(pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance41);

            }

            if (pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4 != null && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4.IsDeleted == false)
            {
                modelIntegrationList.Add(pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4);
                ViewBag.IsModelIntegratedWorkFront = true; //Added 29 Dec 2015 by Brad Gray PL#1851

                if (pcpt.TacticType.IntegrationWorkFrontTemplate != null && pcpt.TacticType.IntegrationWorkFrontTemplate.Template_Name != null)
                {
                    _inspectmodel.WorkFrontTemplate = pcpt.TacticType.IntegrationWorkFrontTemplate.Template_Name;
                }
                else
                {
                    _inspectmodel.WorkFrontTemplate = "None Assigned";
                }
                //_inspectmodel.WorkFrontTemplate = pcpt.TacticType.IntegrationWorkFrontTemplate.Template_Name;

                // add 1/10/2016 by Brad Gray PL#1856 - get a list of active Requeust Queues for instance ID, creating a dictionary of database id & name, order by name. Will use in dropdown select box
                ViewBag.WorkFrontRequestQueueList = db.IntegrationWorkFrontRequestQueues.Where(q => q.IntegrationInstanceId == pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4.IntegrationInstanceId
                                    && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4.IsDeleted == false && q.IsDeleted == false).Select(modelQ => new { modelQ.Id, modelQ.RequestQueueName })
                                        .Distinct().OrderBy(q => q.RequestQueueName).ToList();
                // add 1/13/2016 by Brad Gray PL#1895 - get a list of active WorkFront users for instance ID, creating a dictionary of database id & name, order by name. Will use in dropdown select box
                ViewBag.WorkFrontUserList = db.IntegrationWorkFrontUsers.Where(q => q.IntegrationInstanceId == pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4.IntegrationInstanceId
                                    && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance4.IsDeleted == false).Select(user => new { user.Id, user.WorkFrontUserName })
                                        .Distinct().OrderBy(u => u.WorkFrontUserName).ToList();




            }
            else { ViewBag.IsModelIntegratedWorkFront = false; } //Added 29 Dec 2015 by Brad Gray PL#1851
            ViewBag.IntegrationInstances = modelIntegrationList;

            //create a dictionary of each instance type name ("Salesforce", "WorkFront", etc) and the front end urls)
            foreach (IntegrationInstance instance in modelIntegrationList)
            {
                string url = instance.IntegrationType.FrontEndUrl;
                if (instance.IntegrationType.Code == Enums.IntegrationInstanceType.WorkFront.ToString())
                {
                    int workFrontCompanyNameAttributeId = db.IntegrationTypeAttributes.Where(att => att.IntegrationTypeId == instance.IntegrationTypeId && att.Attribute == "Company Name").FirstOrDefault().IntegrationTypeAttributeId;
                    string prepend = db.IntegrationInstance_Attribute.Where(inst => inst.IntegrationInstanceId == instance.IntegrationInstanceId &&
                       inst.IntegrationTypeAttributeId == workFrontCompanyNameAttributeId).FirstOrDefault().Value;
                    string append = String.Empty;

                    if (pcpt.IntegrationWorkFrontProjectID == null) //updates to link to request. PL#1896 - Brad Gray 24 Jan 2016
                    {
                        if (tSetting != null && tSetting.TacticApprovalObject == Integration.Helper.Enums.WorkFrontTacticApprovalObject.Request.ToString())
                        {
                            if (tRequest != null && tRequest.RequestId != null)
                            {
                                append = "/issue/view?ID=" + tRequest.RequestId;
                            }
                        }
                    }
                    else
                    {
                        append = "/project/view?ID=" + pcpt.IntegrationWorkFrontProjectID;
                    }
                    url = string.Concat("https://", prepend, url, append);
                }
                else if (instance.IntegrationType.Code == Enums.IntegrationInstanceType.Salesforce.ToString())
                {
                    string append = "/" + pcpt.IntegrationInstanceTacticId;
                    url = string.Concat(url, append);
                }
                else if (instance.IntegrationType.Code == Enums.IntegrationInstanceType.Eloqua.ToString())
                {
                    string append = "/" + pcpt.IntegrationInstanceEloquaId;
                    url = string.Concat(url, append);
                    #region get base url from EntityIntegration_Attribute table
                    //insertation start #2310 Kausha 23/06/2013 following is added to get eloqua base url from    EntityIntegration_Attribute
                    //if it will not be availbale in table then it will be work as previously.
                    string strentityType = Convert.ToString(Integration.Helper.Enums.EntityType.IntegrationInstance);
                    var instanceData = db.EntityIntegration_Attribute.Where(data => data.EntityId == instance.IntegrationInstanceId && data.IntegrationinstanceId == instance.IntegrationInstanceId && data.EntityType.ToLower() == strentityType.ToLower()).FirstOrDefault();
                    if (instanceData != null)
                    {
                        string baseurlvalue = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EloquaUrl"]);
                        if (!string.IsNullOrEmpty(baseurlvalue))
                        {
                            baseurlvalue = baseurlvalue.ToLower().Replace("#andpercent#", "&");
                            url = instanceData.AttrValue + baseurlvalue + pcpt.IntegrationInstanceEloquaId;
                        }

                    }
                    #endregion
                    //insertation end #2310 Kausha 23/06/2013
                }
                else if (instance.IntegrationType.Code == Enums.IntegrationInstanceType.Marketo.ToString())
                {
                    // Add By Nishant Sheth
                    // Desc :: For Pushed Tactic in marketo user click on marketo logo it will be redirect on marketo program/entity.
                    string AttributeType = Enums.EntityIntegrationAttribute.MarketoUrl.ToString().ToLower();
                    string EntityType = Enums.EntityType.Tactic.ToString().ToLower();
                    var MarketoUrl = db.EntityIntegration_Attribute.Where(entity => entity.EntityId == pcpt.PlanTacticId && entity.EntityType.ToLower() == EntityType
                        && entity.AttrType.ToLower() == AttributeType && entity.IntegrationinstanceId == instance.IntegrationInstanceId).Select(entity => entity.AttrValue).FirstOrDefault();
                    if (MarketoUrl != null)
                    {
                        url = MarketoUrl;
                    }
                    else
                    {
                        string append = "/"; // Modified By Nishant Sheth // Remove Integration Intance id from url // #2134 observation 1
                        url = string.Concat(url, append);
                    }
                }
                if (!IntegrationLinkDictionary.ContainsKey(instance.IntegrationType.Code))
                {
                    IntegrationLinkDictionary.Add(instance.IntegrationType.Code, url);
                }
            }
            ViewBag.IntegrationTypeLinks = IntegrationLinkDictionary;


            ///End Added by Brad Gray 08-10-2015 for PL#1462

            ////Start : Added by Mitesh Vaishnav for PL ticket #690 Model Interface - Integration
            ViewBag.TacticIntegrationInstance = pcpt.IntegrationInstanceTacticId;
            ViewBag.TacticEloquaInstance = pcpt.IntegrationInstanceEloquaId;
            ViewBag.TacticMarketoInstance = pcpt.IntegrationInstanceMarketoID;
            if (pcpt.IntegrationWorkFrontProjectID != null) //modified 24 Jan 2016 by Brad Gray PL#1851
            {
                ViewBag.TacticIntegrationProjMgmtInstance = pcpt.IntegrationWorkFrontProjectID;
            }
            else if (tRequest != null && tRequest.RequestId != null)
            {
                ViewBag.TacticIntegrationProjMgmtInstance = tRequest.RequestId;
            }
            else
            {
                ViewBag.TacticIntegrationProjMgmtInstance = null;
            }


            string pullResponses = Operation.Pull_Responses.ToString();
            string pullClosedWon = Operation.Pull_ClosedWon.ToString();
            string pullQualifiedLeads = Operation.Pull_QualifiedLeads.ToString();
            string planEntityType = Enums.Section.Tactic.ToString();
            List<int> linktacticids = new List<int>();
            linktacticids.Add(_inspectmodel.PlanTacticId);
            if (pcpt.LinkedTacticId.HasValue && pcpt.LinkedTacticId.Value > 0)
            {
                linktacticids.Add(pcpt.LinkedTacticId.Value);
            }

            var planEntityLogList = db.IntegrationInstancePlanEntityLogs.Where(ipt => linktacticids.Contains(ipt.EntityId) && ipt.EntityType == planEntityType).ToList();
            if (planEntityLogList.Where(planLog => planLog.Operation == pullResponses).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull responses
                ViewBag.INQLastSync = planEntityLogList.Where(planLog => planLog.Operation == pullResponses).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
            }
            if (planEntityLogList.Where(planLog => planLog.Operation == pullClosedWon).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull closed won
                ViewBag.CWLastSync = planEntityLogList.Where(planLog => planLog.Operation == pullClosedWon).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
            }
            if (planEntityLogList.Where(planLog => planLog.Operation == pullQualifiedLeads).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull qualified leads
                ViewBag.MQLLastSync = planEntityLogList.Where(planLog => planLog.Operation == pullQualifiedLeads).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
            }

            if (planEntityLogList.Any())
            {
                ViewBag.TacticLastSync = planEntityLogList.OrderByDescending(log => log.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
            }

            ////End : Added by Mitesh Vaishnav for PL ticket #690 Model Interface - Integration
            string entityType = Enums.EntityType.Tactic.ToString();

            var topThreeCustomFields = db.CustomFields.Where(cf => cf.IsDefault == true && cf.IsDeleted == false && cf.IsRequired == true && cf.ClientId == Sessions.User.ClientId && cf.EntityType == entityType).Take(3).ToList().Select((cf, Index) => new CustomFieldReviewTab()
            {
                CustomFieldId = cf.CustomFieldId,
                Name = cf.Name,
                Class = "customfield-review" + (Index + 1).ToString()
                // cf.CustomField_Entity.Where(ct => ct.EntityId == id).Select(ct => ct.Value).Any() ? string.Join(",", cf.CustomFieldOptions.Where(a => a.IsDeleted == false && cf.CustomField_Entity.Where(ct => ct.EntityId == id).Select(ct => ct.Value).ToList().Contains(a.CustomFieldOptionId.ToString())).Select(a => a.Value).ToList()) : "N/A"
            }).ToList();
            var customFieldIds = topThreeCustomFields.Select(Tcf => Tcf.CustomFieldId).ToList();
            var customFieldValues = db.CustomField_Entity.Where(ct => ct.EntityId == id && customFieldIds.Contains(ct.CustomFieldId)).Select(ct => new { ct.Value, ct.CustomFieldId }).ToList();
            var customFieldOption = db.CustomFieldOptions.Where(co => co.IsDeleted.Equals(false) && customFieldIds.Contains(co.CustomFieldId)).Select(co => new { co.Value, co.CustomFieldOptionId }).ToList();
            foreach (var customField in topThreeCustomFields)
            {
                if (customFieldValues.Where(ct => ct.CustomFieldId == customField.CustomFieldId).Any())
                {
                    var ExistingCustomFieldValues = customFieldValues.Where(ct => ct.CustomFieldId == customField.CustomFieldId).Select(ct => ct.Value).ToList();
                    customField.Value = string.Join(",", customFieldOption.Where(a => ExistingCustomFieldValues.Contains(a.CustomFieldOptionId.ToString())).Select(a => a.Value).ToList());
                }
                else
                {
                    customField.Value = "N/A";
                }
            }
            ViewBag.TopThreeCustomFields = topThreeCustomFields;

            // Added by Viral Kadiya related to PL ticket #2108: Scenario1.
            int sfdcInstanceId = 0, elqaInstanceId = 0, workfrontInstanceId = 0, marketoInstanceId = 0;
            #region "Get SFDC, Elqoua, & WorkFront InstanceId from Model"
            Model objModel = new Model();
            objModel = pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Model;
            if (objModel != null)
            {
                if (objModel.IntegrationInstanceId.HasValue)
                    sfdcInstanceId = objModel.IntegrationInstanceId.Value;
                if (objModel.IntegrationInstanceEloquaId.HasValue)
                    elqaInstanceId = objModel.IntegrationInstanceEloquaId.Value;
                if (objModel.IntegrationInstanceIdProjMgmt.HasValue)
                    workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt.Value;
                if (objModel.IntegrationInstanceMarketoID.HasValue)
                    marketoInstanceId = objModel.IntegrationInstanceMarketoID.Value;
            }
            ViewBag.SFDCId = sfdcInstanceId;
            ViewBag.ElouqaId = elqaInstanceId;
            ViewBag.WorkfrontId = workfrontInstanceId;
            ViewBag.MarketoId = marketoInstanceId;
            #endregion

            return PartialView("Review", _inspectmodel);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Actuals Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Actuals Tab.</returns>
        public ActionResult LoadActuals(int id)
        {
            InspectModel _inspectmodel;
            //// Load InspectModel Data.
            _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);

            ViewBag.TacticStageId = _inspectmodel.StageId;
            ViewBag.TacticStageTitle = _inspectmodel.StageTitle;

            List<string> lstStageTitle = new List<string>();
            lstStageTitle = Common.GetTacticStageTitle(id);
            ViewBag.StageTitle = lstStageTitle;

            List<Plan_Campaign_Program_Tactic> tid = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == id).ToList();
            List<ProjectedRevenueClass> tacticList = Common.ProjectedRevenueCalculateList(tid);
            _inspectmodel.Revenues = Math.Round(tacticList.Where(_tactic => _tactic.PlanTacticId == id).Select(_tactic => _tactic.ProjectedRevenue).FirstOrDefault(), 2); // Modified by Sohel Pathan on 15/09/2014 for PL ticket #760
            tacticList = Common.ProjectedRevenueCalculateList(tid, true);

            // Add By Nishant Sheth 
            // Desc:: for add multiple years regarding #1765
            // To create the period of the year dynamically base on tactic period
            List<listMonthDynamic> lstMonthlyDynamic = new List<listMonthDynamic>();
            tid.ForEach(tactic =>
            {
                List<string> lstMonthlyExtended = new List<string>();
                int YearDiffrence = Convert.ToInt32(Convert.ToInt32(tactic.EndDate.Year) - Convert.ToInt32(tactic.StartDate.Year));
                string periodPrefix = "Y";
                int baseYear = 0;
                for (int i = 0; i < (YearDiffrence + 1); i++)
                {
                    for (int j = 1; j <= 12; j++)
                    {
                        lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                    }
                    baseYear = baseYear + 12;
                }
                lstMonthlyDynamic.Add(new listMonthDynamic { Id = tactic.PlanTacticId, listMonthly = lstMonthlyExtended });
            });

            var tatcicMonth = lstMonthlyDynamic.Select(a => a.listMonthly).FirstOrDefault();
            ViewBag.StartYear = tid.Select(a => a.StartDate.Year).FirstOrDefault();// Add By Nishant sheth
            ViewBag.YearDiffrence = tid.Select(a => (Convert.ToInt32(a.EndDate.Year) - Convert.ToInt32(a.StartDate.Year))).FirstOrDefault();

            string TitleProjectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            string TitleCW = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            string TitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

            ////Modified by Mitesh Vaishnav for PL ticket #695
            // Change by Nishant sheth
            // Desc :: #1765 - add period condition to get value
            var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(planTacticActuals => planTacticActuals.PlanTacticId == id
                && tatcicMonth.Contains(planTacticActuals.Period)).ToList();
            //tacticActualList = tacticActualList.Where(planTacticActuals => lstMonthlyDynamic.Select(a => a.listMonthly).FirstOrDefault().Contains(planTacticActuals.Period)).ToList();

            _inspectmodel.ProjectedStageValueActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleProjectedStageValue).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.CWsActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleCW).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.RevenuesActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleRevenue).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.MQLsActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleMQL).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.CWs = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).FirstOrDefault(), 1);

            string modifiedBy = string.Empty;
            try
            {
                modifiedBy = Common.TacticModificationMessage(_inspectmodel.PlanTacticId);////Modified by Mitesh Vaishnav for PL ticket #743 Actuals Inspect: User Name for Scheduler Integration
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                }
            }

            ViewBag.UpdatedBy = modifiedBy != string.Empty ? modifiedBy : null;////Modified by Mitesh Vaishnav for PL ticket #743 Actuals Inspect: User Name for Scheduler Integration

            // Modified by dharmraj for implement new formula to calculate ROI, #533
            if (_inspectmodel.Cost > 0)
            {
                _inspectmodel.ROI = (_inspectmodel.Revenues - _inspectmodel.Cost) / _inspectmodel.Cost;
            }
            else
                _inspectmodel.ROI = 0;
            ////Start Modified by Mitesh Vaishnav For PL ticket #695
            double tacticCostActual = 0;
            //// Checking whether line item actuals exists.
            // Change by Nishant sheth
            // Desc :: #1765 - add period condition to get value
            var lineItemListActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineitemActual => lineitemActual.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId == id &&
                                                                                            lineitemActual.Plan_Campaign_Program_Tactic_LineItem.IsDeleted == false
                                                                                            && tatcicMonth.Contains(lineitemActual.Period))
                                                                                            .ToList();
            if (lineItemListActuals.Count != 0)
            {
                tacticCostActual = lineItemListActuals.Sum(lineitemActual => lineitemActual.Value);
            }
            else
            {
                ////If line item actual is not exist for tactic than cost actual will be total of tactic cost actual
                string costStageTitle = Enums.InspectStage.Cost.ToString();
                var tacticActualCostList = tacticActualList.Where(tacticActual => tacticActual.StageTitle == costStageTitle).ToList();
                if (tacticActualCostList.Count != 0)
                {
                    tacticCostActual = tacticActualCostList.Sum(tacticActual => tacticActual.Actualvalue);
                }
            }
            if (tacticCostActual > 0)
            {
                _inspectmodel.ROIActual = (_inspectmodel.RevenuesActual - tacticCostActual) / tacticCostActual;
            }
            else
            {
                _inspectmodel.ROIActual = 0;
            }
            ////End Modified by Mitesh Vaishnav For PL ticket #695
            ViewBag.TacticDetail = _inspectmodel;
            bool isValidUser = true;
            if (_inspectmodel.OwnerId != Sessions.User.UserId) isValidUser = false;
            ViewBag.IsValidUser = isValidUser;

            ViewBag.IsModelDeploy = _inspectmodel.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690
            if (_inspectmodel.LastSyncDate != null)
            {
                ViewBag.LastSync = Common.objCached.LastSynced.Replace("{0}", Common.GetFormatedDate(_inspectmodel.LastSyncDate)); ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            ViewBag.LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanTacticId == id && lineitem.IsDeleted == false).OrderByDescending(lineitem => lineitem.LineItemTypeId).ToList();
            return PartialView("Actual");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Actual Value Of Tactic.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Json Result of tactic Actual Value.</returns>
        public JsonResult GetActualTacticData(int id)
        {
            listMonthDynamic listMonthDynamic = new listMonthDynamic();

            var dtTactic = (from tacActual in db.Plan_Campaign_Program_Tactic_Actual
                            where tacActual.PlanTacticId == id
                            select new { tacActual.CreatedBy, tacActual.CreatedDate, tacActual.Plan_Campaign_Program_Tactic }).FirstOrDefault();
            // Add By Nishant Sheth 
            // Desc:: for add multiple years regarding #1765
            // To create the period of the year dynamically base on tactic period
            if (dtTactic != null)
            {
                List<string> lstMonthlyExtended = new List<string>();
                int YearDiffrence = Convert.ToInt32(Convert.ToInt32(dtTactic.Plan_Campaign_Program_Tactic.EndDate.Year) - Convert.ToInt32(dtTactic.Plan_Campaign_Program_Tactic.StartDate.Year));
                string periodPrefix = "Y";
                int baseYear = 0;
                for (int i = 0; i < (YearDiffrence + 1); i++)
                {
                    for (int j = 1; j <= 12; j++)
                    {
                        lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                    }
                    baseYear = baseYear + 12;
                }

                listMonthDynamic.Id = id;
                listMonthDynamic.listMonthly = lstMonthlyExtended;
            }
            var tacticMonth = listMonthDynamic.listMonthly != null ? listMonthDynamic.listMonthly.Select(a => a).ToList() : null;

            var lineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == id && lineItem.IsDeleted == false).Select(lineItem => lineItem.PlanLineItemId).ToList();
            // Change by Nishant sheth
            // Desc :: #1765 - add period condition to get value
            var dtlineItemActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => lineItemIds.Contains(lineActual.PlanLineItemId)).ToList()
                .Where(lineActual => (tacticMonth != null ? tacticMonth.Contains(lineActual.Period) : lineActual.Period.Contains(lineActual.Period))).ToList();
            if (dtTactic != null || dtlineItemActuals != null)
            {
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var ActualData = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => pcpta.PlanTacticId == id)
                    .Select(tacActual => new
                {
                    id = tacActual.PlanTacticId,
                    stageTitle = tacActual.StageTitle,
                    period = tacActual.Period,
                    actualValue = tacActual.Actualvalue
                }).ToList().Where(pcpta => (tacticMonth != null ? tacticMonth.Contains(pcpta.period) : pcpta.period.Contains(pcpta.period))).ToList();

                ////// start-Added by Mitesh Vaishnav for PL ticket #571
                //// Actual cost portion added exact under "lstMonthly" array because Actual cost portion is independent from the monthly/quarterly selection made by the user at the plan level.
                bool isLineItemForTactic = false;////flag for line items count of tactic.If tactic has any line item than flag set to true else false
                if (lineItemIds.Count == 0)
                {
                    var objBudgetAllocationData = new { actualData = ActualData, IsLineItemForTactic = isLineItemForTactic };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ////object for filling input of Actual Cost Allocation
                    var LineItemactualCost = dtlineItemActuals.Select(al => new
                    {
                        PlanLineItemId = al.PlanLineItemId,
                        Period = al.Period,
                        Value = al.Value,
                        Title = al.Plan_Campaign_Program_Tactic_LineItem.Title
                    }).ToList();
                    isLineItemForTactic = true;
                    var objBudgetAllocationData = new { actualData = ActualData, ActualCostAllocationData = LineItemactualCost, IsLineItemForTactic = isLineItemForTactic };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                //// End-Added by Mitesh Vaishnav for PL ticket #571

            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to UpdateResult.
        /// </summary>
        /// <param name="tacticactual">List of InspectActual.</param>
        /// <param name="lineItemActual"></param>
        /// <param name="tactictitle"></param>
        /// <param name="UserId"></param>
        /// <returns>Returns JsonResult.</returns>
        [HttpPost]
        public JsonResult UploadResult(List<InspectActual> tacticactual, List<Plan_Campaign_Program_Tactic_LineItem_Actual> lineItemActual, string UserId = "", string tactictitle = "")
        {
            bool isLineItemForTactic = false;
            //// check whether UserId is current loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                if (tacticactual != null)
                {
                    var actualResult = (from tacActual in tacticactual
                                        select new { tacActual.PlanTacticId, tacActual.TotalProjectedStageValueActual, tacActual.TotalMQLActual, tacActual.TotalCWActual, tacActual.TotalRevenueActual, tacActual.TotalCostActual, tacActual.ROI, tacActual.ROIActual, tacActual.IsActual }).FirstOrDefault();
                    var objpcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == actualResult.PlanTacticId).FirstOrDefault();

                    #region "Retrieve linkedTactic"
                    //List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.IsDeleted == false).ToList();
                    int linkedTacticId = 0, PlanTacticId = 0;
                    PlanTacticId = actualResult.PlanTacticId;
                    linkedTacticId = (objpcpt != null && objpcpt.LinkedTacticId.HasValue) ? objpcpt.LinkedTacticId.Value : 0;
                    //if (linkedTacticId <= 0)
                    //{
                    //    var lnkPCPT = tblPlanTactic.Where(tac => tac.LinkedTacticId == PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                    //    linkedTacticId = (lnkPCPT != null && lnkPCPT.LinkedTacticId.HasValue) ? lnkPCPT.LinkedTacticId.Value : 0;
                    //}
                    #endregion

                    //// Get Tactic duplicate record.
                    var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                  join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                  join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                  where pcpt.Title.Trim().ToLower().Equals(tactictitle.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(actualResult.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                  && pcp.PlanProgramId == objpcpt.PlanProgramId
                                  select pcp).FirstOrDefault();

                    //// Get Linked Tactic duplicate record.
                    Plan_Campaign_Program_Tactic dupLinkedTactic = null;
                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                    bool isMultiYearlinkedTactic = false;
                    int yearDiff = 0, perdNum = 12, cntr = 0;
                    List<string> lstLinkedPeriods = new List<string>();
                    if (linkedTacticId > 0)
                    {
                        linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault();

                        dupLinkedTactic = (from pcpt in db.Plan_Campaign_Program_Tactic
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                           where pcpt.Title.Trim().ToLower().Equals(tactictitle.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(linkedTacticId) && pcpt.IsDeleted.Equals(false)
                                           && pcp.PlanProgramId == linkedTactic.PlanProgramId
                                           select pcpt).FirstOrDefault();
                        yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                        isMultiYearlinkedTactic = yearDiff > 0 ? true : false;

                        cntr = 12 * yearDiff;
                        for (int i = 1; i <= cntr; i++)
                        {
                            lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                        }
                    }


                    //// if duplicate record exist then return duplication message.
                    if (dupLinkedTactic != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.LinkedPlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else if (pcpvar != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else
                    {
                        Dictionary<int, int?> linkedLineItemMappinglist = new Dictionary<int, int?>();
                        #region " If Duplicate name does not exist"
                        if (lineItemActual != null && lineItemActual.Count > 0)
                        {
                            lineItemActual.ForEach(al => { al.CreatedBy = Sessions.User.UserId; al.CreatedDate = DateTime.Now; });
                            List<int> lstLineItemIds = lineItemActual.Select(al => al.PlanLineItemId).Distinct().ToList();
                            var prevlineItemActual = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => lstLineItemIds.Contains(al.PlanLineItemId)).ToList();
                            prevlineItemActual.ForEach(al => db.Entry(al).State = EntityState.Deleted);
                            lineItemActual.ForEach(al => db.Entry(al).State = EntityState.Added);
                            lineItemActual.ForEach(al => db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(al));
                            isLineItemForTactic = true;
                            db.SaveChanges();
                            // Remove old Linked LineItmes Actual record and Add new
                            if (linkedTacticId > 0)
                            {
                                //Get Linked LineItem Ids.
                                linkedLineItemMappinglist = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => lstLineItemIds.Contains(line.PlanLineItemId)).ToDictionary(key => key.PlanLineItemId, val => val.LinkedLineItemId);
                                List<int> linkedLineItemIds = linkedLineItemMappinglist.Where(line => line.Value.HasValue).Select(line => line.Value.Value).ToList();

                                if (linkedLineItemIds != null && linkedLineItemIds.Count > 0)
                                {
                                    if (isMultiYearlinkedTactic)    // if Multi year tactic then remove only linked actuals value with orgional.
                                    {
                                        var prevlinkedlineItemActual = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => linkedLineItemIds.Contains(al.PlanLineItemId) && lstLinkedPeriods.Contains(al.Period)).ToList();
                                        if (prevlinkedlineItemActual != null && prevlinkedlineItemActual.Count > 0)
                                            prevlinkedlineItemActual.ForEach(al => db.Entry(al).State = EntityState.Deleted);
                                    }
                                    else
                                    {
                                        var prevlinkedlineItemActual = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => linkedLineItemIds.Contains(al.PlanLineItemId)).ToList();
                                        if (prevlinkedlineItemActual != null && prevlinkedlineItemActual.Count > 0)
                                            prevlinkedlineItemActual.ForEach(al => db.Entry(al).State = EntityState.Deleted);
                                    }
                                }
                                List<Plan_Campaign_Program_Tactic_LineItem_Actual> linkedLineItemActuals = lineItemActual.ToList();
                                int? linkedLineItemId = 0;
                                Plan_Campaign_Program_Tactic_LineItem_Actual objLinkedActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                foreach (Plan_Campaign_Program_Tactic_LineItem_Actual actual in linkedLineItemActuals)
                                {
                                    string orgPeriod = actual.Period;
                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                    int NumPeriod = int.Parse(numPeriod);
                                    if (isMultiYearlinkedTactic)
                                    {
                                        linkedLineItemId = linkedLineItemMappinglist.Where(lnk => lnk.Key == actual.PlanLineItemId).Select(line => line.Value).FirstOrDefault();
                                        if (linkedLineItemId != null && linkedLineItemId.HasValue)
                                        {
                                            objLinkedActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                            objLinkedActual.PlanLineItemId = linkedLineItemId.Value;
                                            objLinkedActual.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                            objLinkedActual.Value = actual.Value;
                                            objLinkedActual.CreatedDate = actual.CreatedDate;
                                            objLinkedActual.CreatedBy = actual.CreatedBy;
                                            //actual.PlanLineItemId = linkedLineItemId.Value;
                                            //actual.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                            db.Entry(objLinkedActual).State = EntityState.Added;
                                            db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objLinkedActual);
                                        }
                                    }
                                    else
                                    {
                                        if (NumPeriod > 12)
                                        {
                                            int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                            int div = NumPeriod / 12;    // In case of 24, Y12.
                                            if (rem > 0 || div > 1)
                                            {
                                                linkedLineItemId = linkedLineItemMappinglist.Where(lnk => lnk.Key == actual.PlanLineItemId).Select(line => line.Value).FirstOrDefault();
                                                if (linkedLineItemId != null && linkedLineItemId.HasValue)
                                                {
                                                    objLinkedActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                                    objLinkedActual.PlanLineItemId = linkedLineItemId.Value;
                                                    objLinkedActual.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3)     
                                                    objLinkedActual.Value = actual.Value;
                                                    objLinkedActual.CreatedDate = actual.CreatedDate;
                                                    objLinkedActual.CreatedBy = actual.CreatedBy;
                                                    //actual.PlanLineItemId = linkedLineItemId.Value;
                                                    //actual.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3)     
                                                    db.Entry(objLinkedActual).State = EntityState.Added;
                                                    db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objLinkedActual);
                                                }
                                            }
                                        }
                                    }
                                }
                                //linkedLineItemActuals.ForEach(al => al.PlanLineItemId =  db.Entry(al).State = EntityState.Added);
                                //linkedLineItemActuals.ForEach(al => db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(al));
                            }
                            db.SaveChanges();
                        }
                        if (isLineItemForTactic)
                        {
                            tacticactual = tacticactual.Where(ta => ta.StageTitle != Enums.InspectStage.Cost.ToString()).ToList();
                        }
                        if (tacticactual != null)
                        {
                            bool isMQL = false; // Tactic stage is MQL or not
                            string inspectStageMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                            int stageId = tacticactual[0].StageId;
                            var objStage = db.Stages.FirstOrDefault(s => s.StageId == stageId);
                            if (objStage.Code == inspectStageMQL)
                            {
                                isMQL = true;
                            }

                            using (MRPEntities mrp = new MRPEntities())
                            {
                                using (var scope = new TransactionScope())
                                {

                                    if (linkedTacticId > 0)
                                    {
                                        if (isMultiYearlinkedTactic)
                                        {
                                            // Remove Tactic Actual list.
                                            var tacticlnkedActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => (ta.PlanTacticId == linkedTacticId) && lstLinkedPeriods.Contains(ta.Period)).ToList();
                                            tacticlnkedActualList.ForEach(ta => db.Entry(ta).State = EntityState.Deleted);

                                            #region "Commented line Item delete"
                                            // Remove Tactic LineItems.
                                            //List<int> tacticlinkedLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem.Where(ta => (ta.PlanTacticId == linkedTacticId)).ToList().Select(a => a.PlanLineItemId).ToList();
                                            //var deletelinkedMarkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => tacticlinkedLineItemActualList.Contains(c.PlanLineItemId) && lstLinkedPeriods.Contains(c.Period)).ToList();
                                            //if (deletelinkedMarkedLineItem != null && deletelinkedMarkedLineItem.Count > 0)
                                            //    deletelinkedMarkedLineItem.ForEach(ta => db.Entry(ta).State = EntityState.Deleted);
                                            #endregion
                                        }
                                        else
                                        {
                                            // Remove Tactic Actual list.
                                            var tacticlnkedActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => (ta.PlanTacticId == linkedTacticId)).ToList();
                                            tacticlnkedActualList.ForEach(ta => db.Entry(ta).State = EntityState.Deleted);

                                            // Remove Tactic LineItems.
                                            #region "Commented line Item delete"
                                            //List<int> tacticlinkedLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem.Where(ta => (ta.PlanTacticId == linkedTacticId)).ToList().Select(a => a.PlanLineItemId).ToList();
                                            //var deletelinkedMarkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => tacticlinkedLineItemActualList.Contains(c.PlanLineItemId)).ToList();
                                            //if (deletelinkedMarkedLineItem != null && deletelinkedMarkedLineItem.Count > 0)
                                            //    deletelinkedMarkedLineItem.ForEach(ta => db.Entry(ta).State = EntityState.Deleted); 
                                            #endregion
                                        }
                                    }
                                    //modified by Mitesh vaishnav for functional review point - removing sp
                                    var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => ta.PlanTacticId == actualResult.PlanTacticId).ToList();
                                    tacticActualList.ForEach(ta => db.Entry(ta).State = EntityState.Deleted);

                                    #region "Commented line Item delete"
                                    //    //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                    //    List<int> tacticLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem.Where(ta => ta.PlanTacticId == actualResult.PlanTacticId).ToList().Select(a => a.PlanLineItemId).ToList();
                                    //    var deleteMarkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => tacticLineItemActualList.Contains(c.PlanLineItemId)).ToList();
                                    //if (deleteMarkedLineItem != null && deleteMarkedLineItem.Count > 0)
                                    //    deleteMarkedLineItem.ForEach(ta => db.Entry(ta).State = EntityState.Deleted); 
                                    #endregion

                                    //db.SaveChanges();
                                    //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                    //Int64 projectedStageValue = 0, mql = 0, cw = 0, cost = 0;
                                    double revenue = 0, projectedStageValue = 0, mql = 0, cw = 0, cost = 0;
                                    List<string> tempList = new List<string>();
                                    //// If Actuals value exist then save Actuals values.
                                    if (actualResult.IsActual)
                                    {
                                        if (isMQL)
                                        {
                                            foreach (var t in tacticactual)
                                            {
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString())
                                                {
                                                    Plan_Campaign_Program_Tactic_Actual objpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                    objpcpta.PlanTacticId = t.PlanTacticId;
                                                    objpcpta.StageTitle = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                                                    objpcpta.Period = t.Period;
                                                    objpcpta.Actualvalue = t.ActualValue;
                                                    objpcpta.CreatedDate = DateTime.Now;
                                                    objpcpta.CreatedBy = Sessions.User.UserId;
                                                    db.Entry(objpcpta).State = EntityState.Added;
                                                    db.Plan_Campaign_Program_Tactic_Actual.Add(objpcpta);

                                                    if (linkedTacticId > 0)
                                                    {
                                                        string orgPeriod = t.Period;
                                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                        int NumPeriod = int.Parse(numPeriod);
                                                        if (isMultiYearlinkedTactic)
                                                        {
                                                            //PeriodChar + ((12 * yearDiff) + int.Parse(numPeriod)).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                            Plan_Campaign_Program_Tactic_Actual lnkpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                            lnkpcpta.PlanTacticId = linkedTacticId;
                                                            lnkpcpta.StageTitle = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                                                            lnkpcpta.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                            lnkpcpta.Actualvalue = t.ActualValue;
                                                            lnkpcpta.CreatedDate = DateTime.Now;
                                                            lnkpcpta.CreatedBy = Sessions.User.UserId;
                                                            db.Entry(lnkpcpta).State = EntityState.Added;
                                                            db.Plan_Campaign_Program_Tactic_Actual.Add(lnkpcpta);
                                                        }
                                                        else
                                                        {
                                                            if (NumPeriod > 12)
                                                            {
                                                                int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                                int div = NumPeriod / 12;    // In case of 24, Y12.
                                                                if (rem > 0 || div > 1)
                                                                {
                                                                    Plan_Campaign_Program_Tactic_Actual lnkpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                                    lnkpcpta.PlanTacticId = linkedTacticId;
                                                                    lnkpcpta.StageTitle = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                                                                    lnkpcpta.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3) 
                                                                    lnkpcpta.Actualvalue = t.ActualValue;
                                                                    lnkpcpta.CreatedDate = DateTime.Now;
                                                                    lnkpcpta.CreatedBy = Sessions.User.UserId;
                                                                    db.Entry(lnkpcpta).State = EntityState.Added;
                                                                    db.Plan_Campaign_Program_Tactic_Actual.Add(lnkpcpta);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //// Save Tactic Actuals values.
                                        int? lnkedLineItemId = 0;
                                        foreach (var t in tacticactual)
                                        {
                                            //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                            if (t.StageTitle != Enums.InspectStage.ProjectedStageValue.ToString() &&
                                                t.StageTitle != Enums.InspectStage.MQL.ToString() &&
                                                t.StageTitle != Enums.InspectStage.CW.ToString() &&
                                                t.StageTitle != Enums.InspectStage.Revenue.ToString() &&
                                                t.StageTitle != Enums.InspectStage.Cost.ToString() &&
                                                t.StageTitle != Enums.InspectStage.INQ.ToString())
                                            {
                                                //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                                // If stage title is number and not matched up with the pre define stages then save data in Plan_Campaign_Program_Tactic_LineItem_Actual
                                                SaveActualLineItem(t);

                                                if (linkedTacticId > 0)
                                                {
                                                    lnkedLineItemId = linkedLineItemMappinglist.Where(lnk => lnk.Key == t.PlanLineItemId).Select(line => line.Value).FirstOrDefault();
                                                    if (lnkedLineItemId != null && lnkedLineItemId.HasValue)
                                                    {
                                                        string orgPeriod = t.Period;
                                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                        int NumPeriod = int.Parse(numPeriod);
                                                        if (isMultiYearlinkedTactic)
                                                        {
                                                            Plan_Campaign_Program_Tactic_LineItem_Actual objPlan_LineItem_Actual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                                            objPlan_LineItem_Actual.PlanLineItemId = lnkedLineItemId.Value;
                                                            objPlan_LineItem_Actual.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                            objPlan_LineItem_Actual.CreatedDate = DateTime.Now;
                                                            objPlan_LineItem_Actual.CreatedBy = Sessions.User.UserId;
                                                            objPlan_LineItem_Actual.Value = t.ActualValue;
                                                            db.Entry(objPlan_LineItem_Actual).State = EntityState.Added;
                                                            db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objPlan_LineItem_Actual);
                                                        }
                                                        else
                                                        {
                                                            if (NumPeriod > 12)
                                                            {
                                                                int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                                int div = NumPeriod / 12;    // In case of 24, Y12.
                                                                if (rem > 0 || div > 1)
                                                                {
                                                                    Plan_Campaign_Program_Tactic_LineItem_Actual objPlan_LineItem_Actual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                                                    objPlan_LineItem_Actual.PlanLineItemId = lnkedLineItemId.Value;
                                                                    objPlan_LineItem_Actual.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3) 
                                                                    objPlan_LineItem_Actual.CreatedDate = DateTime.Now;
                                                                    objPlan_LineItem_Actual.CreatedBy = Sessions.User.UserId;
                                                                    objPlan_LineItem_Actual.Value = t.ActualValue;
                                                                    db.Entry(objPlan_LineItem_Actual).State = EntityState.Added;
                                                                    db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objPlan_LineItem_Actual);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Actual objpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                objpcpta.PlanTacticId = t.PlanTacticId;
                                                objpcpta.StageTitle = t.StageTitle;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()) projectedStageValue += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()) mql += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()) cw += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()) revenue += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStage.Revenue.ToString()) cost += t.ActualValue;

                                                objpcpta.Period = t.Period;
                                                objpcpta.Actualvalue = t.ActualValue;
                                                objpcpta.CreatedDate = DateTime.Now;
                                                objpcpta.CreatedBy = Sessions.User.UserId;
                                                db.Entry(objpcpta).State = EntityState.Added;
                                                db.Plan_Campaign_Program_Tactic_Actual.Add(objpcpta);

                                                if (linkedTacticId > 0)
                                                {
                                                    string orgPeriod = t.Period;
                                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                    string strPeriod = string.Empty;
                                                    int NumPeriod = int.Parse(numPeriod);
                                                    if (isMultiYearlinkedTactic)
                                                        strPeriod = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                    else
                                                    {
                                                        if (NumPeriod > 12)
                                                        {
                                                            int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                            int div = NumPeriod / 12;
                                                            strPeriod = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3) 
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(strPeriod))
                                                    {
                                                        Plan_Campaign_Program_Tactic_Actual lnkdpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                        lnkdpcpta.PlanTacticId = linkedTacticId;
                                                        lnkdpcpta.StageTitle = t.StageTitle;
                                                        //if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()) projectedStageValue += t.ActualValue;
                                                        //if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()) mql += t.ActualValue;
                                                        //if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()) cw += t.ActualValue;
                                                        //if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()) revenue += t.ActualValue;
                                                        //if (t.StageTitle == Enums.InspectStage.Revenue.ToString()) cost += t.ActualValue;
                                                        lnkdpcpta.Period = strPeriod;
                                                        lnkdpcpta.Actualvalue = t.ActualValue;
                                                        lnkdpcpta.CreatedDate = DateTime.Now;
                                                        lnkdpcpta.CreatedBy = Sessions.User.UserId;
                                                        db.Entry(lnkdpcpta).State = EntityState.Added;
                                                        db.Plan_Campaign_Program_Tactic_Actual.Add(lnkdpcpta);
                                                        if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString())
                                                            tempList.Add(strPeriod);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    db.SaveChanges();

                                    //Plan_Campaign_Program_Tactic objPCPT = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == actualResult.PlanTacticId).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(tactictitle)) // Added by Viral kadiya on 11/12/2014 to update tactic title for PL ticket #946.
                                        objpcpt.Title = tactictitle;
                                    objpcpt.ModifiedBy = Sessions.User.UserId;
                                    objpcpt.ModifiedDate = DateTime.Now;
                                    db.Entry(objpcpt).State = EntityState.Modified;

                                    // Update linked Tactic Title.
                                    if (linkedTacticId > 0)
                                    {
                                        Plan_Campaign_Program_Tactic linkedPCPT = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(tactictitle)) // Added by Viral kadiya on 11/12/2014 to update tactic title for PL ticket #946.
                                            linkedPCPT.Title = tactictitle;
                                        linkedPCPT.ModifiedBy = Sessions.User.UserId;
                                        linkedPCPT.ModifiedDate = DateTime.Now;
                                        db.Entry(linkedPCPT).State = EntityState.Modified;
                                    }

                                    int result = db.SaveChanges();
                                    result = Common.InsertChangeLog(objpcpt.Plan_Campaign_Program.Plan_Campaign.PlanId, null, actualResult.PlanTacticId, objpcpt.Title, Enums.ChangeLog_ComponentType.tacticresults, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                    scope.Complete();
                                    string strMessage = Common.objCached.PlanEntityActualsUpdated.Replace("{0}", Enums.PlanEntity.Tactic.ToString());    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { id = actualResult.PlanTacticId, TabValue = "Actuals", msg = strMessage });
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { id = 0 });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Tactic.
        /// </summary>
        /// <param name="id">Tactic Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <param name="CalledFromBudget"></param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public ActionResult EditTactic(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            Inspect_Popup_Plan_Campaign_Program_TacticModel ippctm = new Inspect_Popup_Plan_Campaign_Program_TacticModel();

            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id) && pcptobj.IsDeleted == false).FirstOrDefault();
            if (pcpt == null)
                return null;
            //Modified By Komal Rawal for Link Tactic feature
            var LinkedTacticId = pcpt.LinkedTacticId;
            if (LinkedTacticId != null && LinkedTacticId > 0)
            {
                ippctm.IsLinkedTactic = true;
            }
            else
            {
                ippctm.IsLinkedTactic = false;
            }
            //End
            Plan_Campaign plancampaignobj = pcpt.Plan_Campaign_Program.Plan_Campaign;

            Plan_Campaign_Program planprogramobj = pcpt.Plan_Campaign_Program;

            int planId = plancampaignobj.PlanId;


            ippctm.CalledFromBudget = CalledFromBudget;


            ippctm.IsCreated = false;

            if (RedirectType == "Assortment")
            {

                ippctm.RedirectType = false;
            }
            else
            {

                ippctm.RedirectType = true;
            }

            int modelid = plancampaignobj.Plan.ModelId;
            List<TacticType> tblTacticTypes = db.TacticTypes.Where(tactype => (tactype.IsDeleted == null || tactype.IsDeleted == false) && tactype.ModelId == modelid).ToList();
            //// Get those Tactic types whose ModelId exist in Plan table and IsDeployedToModel = true.
            var lstTactic = (from tacType in tblTacticTypes
                             where tacType.IsDeployedToModel == true
                             orderby tacType.Title
                             select tacType).ToList();


            // Check whether current TacticId related TacticType exist or not.
            if (!lstTactic.Any(tacType => tacType.TacticTypeId == pcpt.TacticTypeId))
            {
                //// Get list of Tactic Types based on PlanID.
                var tacticTypeSpecial = (from _tacType in lstTactic
                                         where _tacType.TacticTypeId == pcpt.TacticTypeId
                                         orderby _tacType.Title
                                         select _tacType).ToList();
                lstTactic = lstTactic.Concat<TacticType>(tacticTypeSpecial).ToList();
                lstTactic = lstTactic.OrderBy(a => a.Title).ToList();
            }

            ippctm.IsTacticAfterApproved = Common.CheckAfterApprovedStatus(pcpt.Status);


            foreach (var item in lstTactic)
                item.Title = HttpUtility.HtmlDecode(item.Title);


            ippctm.ExtIntService = Common.CheckModelIntegrationExist(pcpt.TacticType.Model);

            /* Added by Mitesh Vaishnav for PL ticket #1073
             Add number of stages for advance/Basic attributes waightage related to tacticType*/
            string entityType = Enums.Section.Tactic.ToString();
            /*Get existing value of Advance/Basic waightage of tactic's attributes*/
            string customFieldType = Enums.CustomFieldType.DropDownList.ToString();
            var customFeildsWeightage = db.CustomField_Entity.Where(cfs => cfs.EntityId == pcpt.PlanTacticId && cfs.CustomField.EntityType == entityType && cfs.CustomField.CustomFieldType.Name == customFieldType).Select(cfs => new
            {
                optionId = cfs.Value,
                CostWeight = cfs.CostWeightage,
                Weight = cfs.Weightage
            }).ToList();

            ippctm.customFieldWeightage = JsonConvert.SerializeObject(customFeildsWeightage);

            /*End : Added by Mitesh Vaishnav for PL ticket #1073*/
            // Added by Arpita Soni for Ticket #2212 on 05/24/2016 
            ippctm.PlanId = pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId;
            ippctm.PlanProgramId = pcpt.PlanProgramId;
            ippctm.ProgramTitle = HttpUtility.HtmlDecode(planprogramobj.Title);
            ippctm.PlanCampaignId = plancampaignobj.PlanCampaignId;
            ippctm.CampaignTitle = HttpUtility.HtmlDecode(plancampaignobj.Title);
            ippctm.PlanTacticId = pcpt.PlanTacticId;
            ippctm.TacticTitle = HttpUtility.HtmlDecode(pcpt.Title);
            ippctm.TacticTypeId = pcpt.TacticTypeId;
            ippctm.Description = HttpUtility.HtmlDecode(pcpt.Description);
            ippctm.OwnerId = pcpt.CreatedBy;
            ippctm.StartDate = pcpt.StartDate;
            ippctm.EndDate = pcpt.EndDate;
            ippctm.PStartDate = planprogramobj.StartDate;
            ippctm.PEndDate = planprogramobj.EndDate;
            ippctm.CStartDate = plancampaignobj.StartDate;
            ippctm.CEndDate = plancampaignobj.EndDate;
            ippctm.Status = pcpt.Status;

            //User userName = new User();
            try
            {
                ippctm.Owner = Common.GetUserName(pcpt.CreatedBy.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            //Updated By Bhavesh Dobariya Performance Issue
            TacticStageValue varTacticStageValue = Common.GetTacticStageRelationForSingleTactic(pcpt, false);
            List<Stage> tblStage = db.Stages.Where(stg => stg.IsDeleted.Equals(false)).ToList();
            //// Set MQL
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = tblStage.Single(s => s.ClientId == Sessions.User.ClientId && s.Code == stageMQL).Level.Value;
            int tacticStageLevel = Convert.ToInt32(pcpt.Stage.Level);
            if (tacticStageLevel < levelMQL)
            {
                ippctm.MQLs = varTacticStageValue.MQLValue;
            }
            else if (tacticStageLevel == levelMQL)
            {
                ippctm.MQLs = Convert.ToDouble(pcpt.ProjectedStageValue);
            }
            else if (tacticStageLevel > levelMQL)
            {
                ippctm.MQLs = 0;
                TempData["TacticMQL"] = "N/A";
            }
            ippctm.MQLs = Math.Round((double)ippctm.MQLs, 0, MidpointRounding.AwayFromZero);
            //// Set Revenue
            ippctm.Revenue = Math.Round(varTacticStageValue.RevenueValue, 2);

            string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
            string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            double budgetAllocation = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == id).ToList().Sum(_tacCost => _tacCost.Value);

            // modified by Viral for PL ticket #2112.
            ippctm.Cost = (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpt.PlanTacticId && lineItem.IsDeleted == false)).Count() > 0
                && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByNone && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByDefault
                ?
                (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost)
                : pcpt.Cost;

            //ippctm.Cost = pcpt.Cost; //modified by komal rawal

            ippctm.IsDeployedToIntegration = pcpt.IsDeployedToIntegration;
            ippctm.StageId = Convert.ToInt32(pcpt.StageId);
            ippctm.StageTitle = tblStage.FirstOrDefault(varS => varS.StageId == pcpt.StageId).Title;
            ippctm.ProjectedStageValue = Convert.ToDouble(pcpt.ProjectedStageValue);

            var modelTacticStageType = lstTactic.Where(_tacType => _tacType.TacticTypeId == pcpt.TacticTypeId).Select(a => a.StageId).FirstOrDefault();
            var plantacticStageType = pcpt.StageId;
            if (modelTacticStageType == plantacticStageType)
            {

                ippctm.IsDiffrentStageType = false;
            }
            else
            {

                ippctm.IsDiffrentStageType = true;
            }

            if (Sessions.User.UserId == pcpt.CreatedBy)
            {

                ippctm.IsOwner = true;
            }
            else
            {

                ippctm.IsOwner = false;
            }

            List<TacticType> tnewList = lstTactic.ToList();
            // if lstTactics contains the same Title tactic then remove from the new Tactic list.
            TacticType tobj = tblTacticTypes.Where(_tacType => _tacType.TacticTypeId == ippctm.TacticTypeId).FirstOrDefault();
            if (tobj != null)
            {
                TacticType tSameExist = tnewList.Where(_newTacType => _newTacType.Title.Equals(tobj.Title)).FirstOrDefault();
                //// if same Title exist then remove that TacticType from New Tactic list.
                if (tSameExist != null)
                    tnewList.Remove(tSameExist);
                tnewList.Add(tobj);
            }



            ippctm.Tactics = tnewList.OrderBy(t => t.Title).ToList();
            ippctm.Year = plancampaignobj.Plan.Year;
            ippctm.TacticCost = pcpt.Cost;
            ippctm.AllocatedBy = plancampaignobj.Plan.AllocatedBy;

            #region "Calculate Plan remaining budget"
            var CostTacticsBudget = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == pcpt.PlanProgramId).Sum(c => c.Cost);
            double? objPlanCampaignProgram = db.Plan_Campaign_Program.FirstOrDefault(p => p.PlanProgramId == pcpt.PlanProgramId).ProgramBudget;
            objPlanCampaignProgram = objPlanCampaignProgram != null ? objPlanCampaignProgram : 0;

            ippctm.planRemainingBudget = (objPlanCampaignProgram - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));
            #endregion

            // Start - Added by Sohel Pathan on 14/11/2014 for PL ticket #708

            ippctm.IsTackticAddEdit = true;
            var campaignList = db.Plan_Campaign.Where(pc => pc.IsDeleted.Equals(false) && pc.PlanId == planId).Select(pc => new
            {
                pc.PlanCampaignId,
                pc.Title
            }).ToList().Select(c => new
            {
                PlanCampaignId = c.PlanCampaignId,
                Title = HttpUtility.HtmlDecode(c.Title)
            }).OrderBy(pc => pc.Title).ToList();
            //var programList = db.Plan_Campaign_Program.Where(pcp => pcp.IsDeleted.Equals(false) && pcp.PlanCampaignId == pcpt.Plan_Campaign_Program.PlanCampaignId).Select(pcp => new
            //{
            //    pcp.PlanProgramId,
            //    pcp.Title
            //}).OrderBy(pcp => pcp.Title).ToList();

            var programList = db.Plan_Campaign_Program.Where(pcp => pcp.IsDeleted.Equals(false) && pcp.PlanCampaignId == pcpt.Plan_Campaign_Program.PlanCampaignId).Select(pcp => new
            {
                pcp.PlanProgramId,
                pcp.Title
            }).ToList().Select(c => new
            {
                PlanProgramId = c.PlanProgramId,
                Title = HttpUtility.HtmlDecode(c.Title)
            }).OrderBy(pc => pc.Title).ToList();    // added by dashrath prajapati for pl#1916 Ampersand

            ippctm.PlanCampaignList = campaignList.Select(c => new SelectListValue { Id = c.PlanCampaignId, Title = c.Title }).ToList();
            ippctm.CampaignProgramList = programList.Select(p => new SelectListValue { Id = p.PlanProgramId, Title = p.Title }).ToList();

            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                //Modified By Komal Rawal for #1360
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); // PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);

                if (lstClientUsers.Count() > 0)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = false;


                    string strUserList = string.Join(",", lstClientUsers);
                    //List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    //lstUserDetails = lstUserDetails.Where(i => !i.IsDeleted).ToList();
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();

                        ippctm.OwnerList = lstPreparedOwners.Select(u => new SelectListUser { Name = u.DisplayName, Id = u.UserId }).ToList();

                    }
                    else
                    {

                        ippctm.OwnerList = new List<SelectListUser>();
                    }
                }
                else
                {

                    ippctm.OwnerList = new List<SelectListUser>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = true;
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            // End - Added by Sohel Pathan on 14/11/2014 for PL ticket #708

            return PartialView("SetupEditAdd", ippctm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Tactic.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_Program_TacticModel.</param>
        /// <param name="lineitems"></param>
        /// <param name="closedTask"></param>
        /// <param name="customFieldInputs"></param>
        /// <param name="UserId"></param>
        /// <param name="strDescription"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SetupSaveTactic(Inspect_Popup_Plan_Campaign_Program_TacticModel form, string lineitems, string closedTask, string customFieldInputs, string UserId = "", string strDescription = "", bool resubmission = false)
        {
            //// check whether UserId is current loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {

                int cid = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == form.PlanProgramId).Select(program => program.PlanCampaignId).FirstOrDefault();
                int planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == cid && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
                int pid = form.PlanProgramId;
                var customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(customFieldInputs);

                //// if PlanTacticId is null then Insert New record.
                if (form.PlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get Duplicate record to check duplication
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcpt.Title.Trim().ToLower().Equals(form.TacticTitle.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                          && pcp.PlanProgramId == form.PlanProgramId
                                          select pcp).FirstOrDefault();

                            //// if duplicate record exist then return duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage, planCampaignId = cid, planProgramId = pid });
                            }
                            else
                            {

                                // Added by Viral Kadiya related to PL ticket #2002: When we create a new tactic, then the integration need to look at the model. If under model integration, there are any integration mapped then the switched for these needs to be turned on as well.
                                // WorkFront added 19 Feb 2016 for PL#2002. On tactic creation, look at WorkFront integration and set tactic defaults in the same manner as Salesforce and Eloqua
                                int sfdcInstanceId = 0, elqaInstanceId = 0, workfrontInstanceId = 0, marketoInstanceId = 0;
                                #region "Get SFDC, Elqoua, & WorkFront InstanceId from Model by Plan"
                                if (planid > 0)
                                {
                                    Model objModel = new Model();
                                    Plan objPlan = new Plan();

                                    objPlan = db.Plans.Where(plan => plan.PlanId == planid).FirstOrDefault();
                                    if (objPlan != null)
                                    {
                                        objModel = objPlan.Model;
                                        if (objModel != null)
                                        {
                                            if (objModel.IntegrationInstanceId.HasValue)
                                                sfdcInstanceId = objModel.IntegrationInstanceId.Value;
                                            if (objModel.IntegrationInstanceEloquaId.HasValue)
                                                elqaInstanceId = objModel.IntegrationInstanceEloquaId.Value;
                                            if (objModel.IntegrationInstanceMarketoID.HasValue)
                                                marketoInstanceId = objModel.IntegrationInstanceMarketoID.Value;
                                            if (objModel.IntegrationInstanceIdProjMgmt.HasValue)
                                                workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt.Value;
                                        }
                                    }
                                }
                                #endregion

                                #region "Get IsDeployedToIntegration by TacticTypeId"
                                int TacticTypeId = 0;
                                bool isDeployedToIntegration = false;
                                if (form.TacticTypeId > 0)
                                {
                                    TacticTypeId = form.TacticTypeId;
                                    TacticType objTacType = new TacticType();
                                    objTacType = db.TacticTypes.Where(tacType => tacType.TacticTypeId == form.TacticTypeId).FirstOrDefault();
                                    if (objTacType != null && objTacType.IsDeployedToIntegration)
                                    {
                                        isDeployedToIntegration = true;
                                    }
                                }
                                #endregion

                                #region "Save New record to Plan_Campaign_Program_Tactic table"
                                Plan_Campaign_Program_Tactic pcpobj = new Plan_Campaign_Program_Tactic();
                                pcpobj.PlanProgramId = form.PlanProgramId;
                                pcpobj.Title = form.TacticTitle;
                                pcpobj.TacticTypeId = form.TacticTypeId;
                                pcpobj.Description = form.Description;
                                pcpobj.Cost = form.Cost;
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                pcpobj.IsDeployedToIntegration = isDeployedToIntegration;
                                pcpobj.StageId = form.StageId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                //pcpobj.CreatedBy = Sessions.User.UserId;   // commented by Rahul Shah on 17/03/2016 for PL #2032 
                                pcpobj.CreatedBy = form.OwnerId;             // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                pcpobj.CreatedDate = DateTime.Now;
                                pcpobj.TacticBudget = form.Cost; //modified for 1229
                                if (isDeployedToIntegration)
                                {
                                    if (sfdcInstanceId > 0)
                                        pcpobj.IsSyncSalesForce = true;         // Set SFDC setting to True if Salesforce instance mapped under Tactic's Model.
                                    if (elqaInstanceId > 0)
                                        pcpobj.IsSyncEloqua = true;             // Set Eloqua setting to True if Eloqua instance mapped under Tactic's Model.
                                    if (marketoInstanceId > 0)
                                        pcpobj.IsSyncMarketo = true;             // Set Marketo setting to True if Marketo instance mapped under Tactic's Model.
                                    if (workfrontInstanceId > 0)
                                        pcpobj.IsSyncWorkFront = true;          // Set WorkFront setting to True if WorkFront instance mapped under Tactic's Model.
                                }
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                #endregion
                                int tacticId = pcpobj.PlanTacticId;

                                //Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                //objNewLineitem.PlanTacticId = tacticId;
                                //objNewLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
                                //objNewLineitem.Cost = pcpobj.Cost;
                                //objNewLineitem.Description = string.Empty;
                                //objNewLineitem.CreatedBy = Sessions.User.UserId;
                                //objNewLineitem.CreatedDate = DateTime.Now;
                                //db.Entry(objNewLineitem).State = EntityState.Added;

                                //// Insert LineItem for the Tactic.
                                if (pcpobj.Cost > 0)
                                {

                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = tacticId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
                                    objNewLineitem.Cost = pcpobj.Cost;
                                    objNewLineitem.Description = string.Empty;
                                    //objNewLineitem.CreatedBy = Sessions.User.UserId;
                                    objNewLineitem.CreatedBy = form.OwnerId;//modified by Rahul Shah on 21/03/2016 for PL #2032 observation.
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;

                                    //Added by Komal Rawal for #1217
                                    int startmonth = pcpobj.StartDate.Month;
                                    Plan_Campaign_Program_Tactic_Budget obPlanCampaignProgramTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
                                    obPlanCampaignProgramTacticBudget.PlanTacticId = tacticId;
                                    obPlanCampaignProgramTacticBudget.Period = PeriodChar + startmonth;
                                    obPlanCampaignProgramTacticBudget.Value = pcpobj.TacticBudget; //modified for 1229
                                    obPlanCampaignProgramTacticBudget.CreatedBy = Sessions.User.UserId;
                                    obPlanCampaignProgramTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(obPlanCampaignProgramTacticBudget).State = EntityState.Added;

                                    Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                    obPlanCampaignProgramTacticCost.PlanTacticId = tacticId;
                                    obPlanCampaignProgramTacticCost.Period = PeriodChar + startmonth;
                                    obPlanCampaignProgramTacticCost.Value = pcpobj.TacticBudget; //modified for 1229
                                    obPlanCampaignProgramTacticCost.CreatedBy = Sessions.User.UserId;
                                    obPlanCampaignProgramTacticCost.CreatedDate = DateTime.Now;
                                    db.Entry(obPlanCampaignProgramTacticCost).State = EntityState.Added;


                                    //end
                                }
                                db.SaveChanges();
                                #region "Update Start & End Date for planCampaignProgramDetails table"
                                var planCampaignProgramDetails = (from pcp in db.Plan_Campaign_Program
                                                                  join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                                                  where pcp.PlanProgramId == pcpobj.PlanProgramId
                                                                  select pcp).FirstOrDefault();

                                if (planCampaignProgramDetails.StartDate > pcpobj.StartDate)
                                {
                                    planCampaignProgramDetails.StartDate = pcpobj.StartDate;
                                }
                                if (planCampaignProgramDetails.Plan_Campaign.StartDate > pcpobj.StartDate)
                                {
                                    planCampaignProgramDetails.Plan_Campaign.StartDate = pcpobj.StartDate;
                                }
                                if (pcpobj.EndDate > planCampaignProgramDetails.EndDate)
                                {
                                    planCampaignProgramDetails.EndDate = pcpobj.EndDate;
                                }
                                if (pcpobj.EndDate > planCampaignProgramDetails.Plan_Campaign.EndDate)
                                {
                                    planCampaignProgramDetails.Plan_Campaign.EndDate = pcpobj.EndDate;
                                }
                                db.Entry(planCampaignProgramDetails).State = EntityState.Modified;
                                #endregion

                                //// Save custom fields value for particular Tactic
                                if (customFields.Count != 0)
                                {
                                    CustomField_Entity objcustomFieldEntity;
                                    foreach (var item in customFields)
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = tacticId;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        objcustomFieldEntity.Weightage = (byte)item.Weight;
                                        objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;
                                    }
                                }

                                db.SaveChanges();

                                result = Common.InsertChangeLog(planid, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);

                                // Check whether the lineitmes is empty or not . if lineitems have any instance at that time call the SaveLineItems function and insert the data into the lineItems table. 
                                if (lineitems != string.Empty)
                                {
                                    result = SaveLineItems(form, lineitems, tacticId);
                                }

                                if (result >= 1)
                                {
                                    // Add By Nishant Sheth
                                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                    DataSet dsPlanCampProgTac = new DataSet();
                                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

                                    // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                    #region "Send Email Notification For Owner changed"
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != Sessions.User.UserId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", Sessions.User.UserId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.
                                                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                                                    //// Ticket: 942 Exception handeling in Gameplan.
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = pcpobj.Plan_Campaign_Program.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = pcpobj.Plan_Campaign_Program.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign_Program.Plan_Campaign.PlanId, tacticId, Enums.Section.Tactic.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcpobj.Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Tactic.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }

                                    }
                                    #endregion
                                    Common.ChangeProgramStatus(pcpobj.PlanProgramId, false);
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == pcpobj.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                                    Common.ChangeCampaignStatus(PlanCampaignId, false);

                                    scope.Complete();
                                    string strMessag = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { IsDuplicate = false, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }), Msg = strMessag, planTacticId = pcpobj.PlanTacticId, planCampaignId = cid, planProgramId = pid, PlanId = planid });
                                }
                            }
                        }
                    }
                }
                else    //// Update record for Tactic
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
                        var TransactionOption = new System.Transactions.TransactionOptions();
                        TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

                        using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
                        {
                            int linkedTacticId = 0;
                            //List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.IsDeleted == false).ToList();
                            Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId.Equals(form.PlanTacticId)).FirstOrDefault();

                            #region "Retrieve linkedTactic"
                            linkedTacticId = (pcpobj != null && pcpobj.LinkedTacticId.HasValue) ? pcpobj.LinkedTacticId.Value : 0;
                            //if (linkedTacticId <= 0)
                            //{
                            //    var lnkPCPT = tblPlanTactic.Where(tac => tac.LinkedTacticId == form.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can only linked with single plan.
                            //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                            //}
                            #endregion

                            //// Get Duplicate record to check duplication
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcpt.Title.Trim().ToLower().Equals(form.TacticTitle.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(form.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                          && pcp.PlanProgramId == form.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            //// Get Linked Tactic duplicate record.
                            Plan_Campaign_Program_Tactic dupLinkedTactic = null;
                            Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                            if (linkedTacticId > 0)
                            {
                                linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault(); // Get LinkedTactic object

                                dupLinkedTactic = (from pcpt in db.Plan_Campaign_Program_Tactic
                                                   join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                                   join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                                   where pcpt.Title.Trim().ToLower().Equals(form.TacticTitle.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(linkedTacticId) && pcpt.IsDeleted.Equals(false)
                                              && pcp.PlanProgramId == linkedTactic.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                                   select pcpt).FirstOrDefault();
                            }

                            //// if duplicate record exist then return duplication message.
                            if (dupLinkedTactic != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.LinkedPlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                            }
                            else if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsDuplicate = true, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }), errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Variable Initialize"
                                bool isReSubmission = false;
                                string status = string.Empty;
                                int oldProgramId = 0;
                                string oldProgramTitle = "";
                                int oldCampaignId = 0;
                                int oldTacticTypeId = 0;
                                #endregion
                                //Start - Added by Mitesh Vaishnav for PL ticket #1137
                                if (resubmission)
                                {
                                    isReSubmission = true;
                                }

                                //Plan_Campaign_Program_Tactic pcpobj = tblPlanTactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).FirstOrDefault();
                                // Add By Nishant Sheth
                                // Desc::#1765 - To remove pervious data from db if end date year difference is less then to compare end date.
                                int EndDateYear = pcpobj.EndDate.Year;
                                int FormEndDateYear = form.EndDate.Year;
                                int EndDatediff = EndDateYear - FormEndDateYear;
                                if (EndDatediff > 0)
                                {
                                    listMonthDynamic lstMonthlyDynamic = new listMonthDynamic();

                                    List<string> lstMonthlyExtended = new List<string>();
                                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(pcpobj.StartDate.Year));
                                    string periodPrefix = "Y";
                                    int baseYear = 0;
                                    for (int i = 0; i < (YearDiffrence + 1); i++)
                                    {
                                        for (int j = 1; j <= 12; j++)
                                        {
                                            lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                                        }
                                        baseYear = baseYear + 12;
                                    }
                                    lstMonthlyDynamic.Id = pcpobj.PlanTacticId;
                                    lstMonthlyDynamic.listMonthly = lstMonthlyExtended.AsEnumerable().Reverse().ToList();

                                    List<string> deleteperiodmonth = new List<string>();
                                    for (int i = 0; i < EndDatediff; i++)
                                    {
                                        var listofperiod = lstMonthlyDynamic.listMonthly.Skip(i * 12).Take(12).ToList();
                                        listofperiod.ForEach(a => { deleteperiodmonth.Add(a); });
                                    }
                                    var listActual = db.Plan_Campaign_Program_Tactic_Actual.Where(a => a.PlanTacticId == pcpobj.PlanTacticId && deleteperiodmonth.Contains(a.Period)).ToList();
                                    listActual.ForEach(a => { db.Entry(a).State = EntityState.Deleted; });
                                    var listBudget = db.Plan_Campaign_Program_Tactic_Budget.Where(a => a.PlanTacticId == pcpobj.PlanTacticId && deleteperiodmonth.Contains(a.Period)).ToList();
                                    listBudget.ForEach(a => { db.Entry(a).State = EntityState.Deleted; });
                                    var listLineITemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(a => a.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId == pcpobj.PlanTacticId && deleteperiodmonth.Contains(a.Period)).ToList();
                                    listLineITemCost.ForEach(a => { db.Entry(a).State = EntityState.Deleted; });
                                }
                                // End By Nishant Sheth
                                pcpobj.Title = form.TacticTitle;
                                status = pcpobj.Status;
                                pcpobj.Description = form.Description;
                                Guid oldOwnerId = pcpobj.CreatedBy;
                                //Start - Added by Mitesh Vaishnav - Remove old resubmission condition and combine it for PL ticket #1137
                                oldTacticTypeId = pcpobj.TacticTypeId;
                                pcpobj.TacticTypeId = form.TacticTypeId;
                                pcpobj.CreatedBy = form.OwnerId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                if (pcpobj.PlanProgramId != form.PlanProgramId)
                                {
                                    oldProgramId = pcpobj.PlanProgramId;
                                    oldProgramTitle = pcpobj.Plan_Campaign_Program.Title;
                                    oldCampaignId = pcpobj.Plan_Campaign_Program.PlanCampaignId;
                                    pcpobj.PlanProgramId = form.PlanProgramId;
                                    db.Entry(pcpobj).State = EntityState.Modified;
                                    db.SaveChanges();
                                    pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).FirstOrDefault();
                                    form.PStartDate = pcpobj.Plan_Campaign_Program.StartDate;
                                    form.PEndDate = pcpobj.Plan_Campaign_Program.EndDate;
                                    form.CStartDate = pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate;
                                    form.CEndDate = pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate;
                                }
                                //End - Added by Mitesh Vaishnav - Remove old resubmission condition and combine it



                                DateTime todaydate = DateTime.Now;

                                /// Modified by:   Dharmraj
                                /// Modified date: 2-Sep-2014
                                /// Purpose:       #625 Changing the dates on an approved tactic needs to go through the approval process
                                // To check whether status is Approved or not
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    // Modified by Mitesh Vaishnav for PL ticket #1137 - Add resubmission flag in if condition
                                    // If any changes in start/end dates then tactic will go through the approval process
                                    if (!isReSubmission && pcpobj.EndDate == form.EndDate && pcpobj.StartDate == form.StartDate)
                                    {
                                        if (todaydate > form.StartDate && todaydate < form.EndDate)
                                        {
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                        }
                                        else if (todaydate > form.EndDate)
                                        {
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                        }
                                    }
                                }

                                #region "Set pcobj Start & End Date."
                                if (pcpobj.LinkedTacticId != null)
                                {
                                    if (Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(pcpobj.StartDate.Year) > 0)
                                    {

                                        if (Convert.ToInt32(form.EndDate.Year) - Convert.ToInt32(form.StartDate.Year) == 0)
                                        {
                                            pcpobj.LinkedTacticId = null;
                                            pcpobj.LinkedPlanId = null;
                                            linkedTactic.LinkedPlanId = null;
                                            linkedTactic.LinkedTacticId = null;

                                            pcpobj.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                                                pcptl =>
                                                {
                                                    pcptl.LinkedLineItemId = null;

                                                });
                                            linkedTactic.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                                                pcpt2 =>
                                                {
                                                    pcpt2.LinkedLineItemId = null;
                                                });
                                        }
                                    }
                                    else
                                    {

                                        if (Convert.ToInt32(form.EndDate.Year) - Convert.ToInt32(pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Year) > 0)
                                        {
                                            //string linkedYear = string.Format(Common.objCached.LinkedTacticExtendedYear, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                            return Json(new { IsExtended = true, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }) });
                                        }

                                    }
                                }
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;

                                if (form.PStartDate > form.StartDate)
                                {
                                    pcpobj.Plan_Campaign_Program.StartDate = form.StartDate;
                                }

                                if (form.EndDate > form.PEndDate)
                                {
                                    pcpobj.Plan_Campaign_Program.EndDate = form.EndDate;
                                }

                                if (form.CStartDate > form.StartDate)
                                {
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate = form.StartDate;
                                }

                                if (form.EndDate > form.CEndDate)
                                {
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate = form.EndDate;
                                }
                                #endregion

                                #region "Update DeployToIntegration & Instnace toggle on Tactic Type update"
                                if (form.TacticTypeId > 0 && oldTacticTypeId != form.TacticTypeId)
                                {
                                    // Added by Viral Kadiya related to PL ticket #2108: When we update tactic type, then the integration need to look at the model. If under model integration, there are any integration mapped then the switched for these needs to be turned on as well.
                                    int sfdcInstanceId = 0, elqaInstanceId = 0, workfrontInstanceId = 0, marketoInstanceId = 0;
                                    #region "Get SFDC, Elqoua, & WorkFront InstanceId from Model by Plan"
                                    if (planid > 0)
                                    {
                                        Model objModel = new Model();
                                        Plan objPlan = new Plan();

                                        objPlan = db.Plans.Where(plan => plan.PlanId == planid).FirstOrDefault();
                                        if (objPlan != null)
                                        {
                                            objModel = objPlan.Model;
                                            if (objModel != null)
                                            {
                                                if (objModel.IntegrationInstanceId.HasValue)
                                                    sfdcInstanceId = objModel.IntegrationInstanceId.Value;
                                                if (objModel.IntegrationInstanceEloquaId.HasValue)
                                                    elqaInstanceId = objModel.IntegrationInstanceEloquaId.Value;
                                                if (objModel.IntegrationInstanceMarketoID.HasValue)
                                                    marketoInstanceId = objModel.IntegrationInstanceMarketoID.Value;
                                                if (objModel.IntegrationInstanceIdProjMgmt.HasValue)
                                                    workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt.Value;
                                            }
                                        }
                                    }
                                    #endregion

                                    #region "Get IsDeployedToIntegration by TacticTypeId"
                                    int TacticTypeId = 0;
                                    bool isDeployedToIntegration = false;

                                    TacticTypeId = form.TacticTypeId;
                                    TacticType objTacType = new TacticType();
                                    objTacType = db.TacticTypes.Where(tacType => tacType.TacticTypeId == form.TacticTypeId).FirstOrDefault();
                                    if (objTacType != null && objTacType.IsDeployedToIntegration)
                                    {
                                        isDeployedToIntegration = true;
                                    }
                                    pcpobj.IsDeployedToIntegration = isDeployedToIntegration;
                                    #endregion

                                    #region "Update Instnce toggle based on TacticType & Model settings"
                                    if (isDeployedToIntegration)
                                    {
                                        if (sfdcInstanceId > 0)
                                            pcpobj.IsSyncSalesForce = true;         // Set SFDC setting to True if Salesforce instance mapped under Tactic's Model.
                                        if (elqaInstanceId > 0)
                                            pcpobj.IsSyncEloqua = true;             // Set Eloqua setting to True if Eloqua instance mapped under Tactic's Model.
                                        if (marketoInstanceId > 0)
                                            pcpobj.IsSyncMarketo = true;             // Set Marketo setting to True if Marketo instance mapped under Tactic's Model.
                                        if (workfrontInstanceId > 0)
                                            pcpobj.IsSyncWorkFront = true;          // Set WorkFront setting to True if WorkFront instance mapped under Tactic's Model.
                                    }
                                    else
                                    {
                                        pcpobj.IsSyncSalesForce = false;         // Set SFDC setting to false if isDeployedToIntegration false.
                                        pcpobj.IsSyncEloqua = false;             // Set Eloqua setting to True if isDeployedToIntegration false.
                                        pcpobj.IsSyncMarketo = false;             // Set Marketo setting to True if isDeployedToIntegration false.
                                        pcpobj.IsSyncWorkFront = false;          // Set WorkFront setting to True if isDeployedToIntegration false.
                                    }
                                    #endregion
                                }
                                #endregion

                                //// check that Tactic cost count greater than 0 OR Plan's AllocatedBy is None or Defaults.
                                if ((db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).ToList()).Count() == 0 ||
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower()
                                    || pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                                {
                                    pcpobj.Cost = form.Cost;
                                }

                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                                double totalLineitemCost = 0;
                                tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                //Modified By komal rawal
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();
                                //End

                                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                                if (totalLineitemCost > form.Cost)
                                {
                                    // Added by Viral Kadiya for Pl ticket #1970.
                                    string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                    return Json(new { IsError = true, errormsg = strReduceTacticPlannedCostMessage });
                                    //form.Cost = totalLineitemCost;
                                }
                                //Added By komal Rawal for #1249
                                if (form.Cost > pcpobj.Cost)
                                {
                                    var diffcost = form.Cost - pcpobj.Cost;
                                    int startmonth = pcpobj.StartDate.Month;

                                    if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                    {
                                        pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;

                                    }
                                    else
                                    {
                                        Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                        objTacticCost.PlanTacticId = pcpobj.PlanTacticId;
                                        objTacticCost.Period = PeriodChar + startmonth;
                                        objTacticCost.Value = diffcost;
                                        objTacticCost.CreatedBy = Sessions.User.UserId;
                                        objTacticCost.CreatedDate = DateTime.Now;
                                        db.Entry(objTacticCost).State = EntityState.Added;
                                    }

                                    //Add linked Tactic TacticCost data
                                    int yearDiff = 0;
                                    bool isMultiYearlinkedTactic = false;
                                    if (linkedTacticId > 0)
                                    {
                                        yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                        isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                                        if (isMultiYearlinkedTactic)
                                        {
                                            string linkedstartmonth = ((12 * yearDiff) + startmonth).ToString();
                                            if (linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedstartmonth).Any())
                                            {
                                                linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedstartmonth).FirstOrDefault().Value += diffcost;
                                            }
                                            //else
                                            //{
                                            //    Plan_Campaign_Program_Tactic_Cost lnkTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            //    lnkTacticCost.PlanTacticId = linkedTacticId;
                                            //    lnkTacticCost.Period = PeriodChar + linkedstartmonth;
                                            //    lnkTacticCost.Value = diffcost;
                                            //    lnkTacticCost.CreatedBy = Sessions.User.UserId;
                                            //    lnkTacticCost.CreatedDate = DateTime.Now;
                                            //    db.Entry(lnkTacticCost).State = EntityState.Added;
                                            //}
                                        }
                                    }
                                }
                                else if (form.Cost < pcpobj.Cost)
                                {
                                    var diffcost = pcpobj.Cost - form.Cost;
                                    double diffLinkCost = diffcost;
                                    int endmonth = 12;
                                    while (diffcost > 0 && endmonth != 0)
                                    {
                                        if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                        {
                                            //Modified by komal Rawal
                                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                                            double objtacticcost = pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                            var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                            if (DiffMonthCost > 0)
                                            {
                                                if (DiffMonthCost > diffcost)
                                                {
                                                    pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                                    diffcost = 0;
                                                }
                                                else
                                                {
                                                    pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                                    diffcost = diffcost - DiffMonthCost;
                                                }
                                            }
                                            //END
                                        }

                                        int yearDiff = 0;
                                        bool isMultiYearlinkedTactic = false;
                                        if (linkedTacticId > 0)
                                        {
                                            yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                            isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                                            string linkedendmonth = ((12 * yearDiff) + endmonth).ToString();
                                            if (isMultiYearlinkedTactic)
                                            {
                                                if (linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).Any())
                                                {
                                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + linkedendmonth).Sum(lineitem => lineitem.Value);
                                                    double objtacticcost = linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).FirstOrDefault().Value;
                                                    var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                                    if (DiffMonthCost > 0)
                                                    {
                                                        if (DiffMonthCost > diffLinkCost)
                                                        {
                                                            linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).FirstOrDefault().Value = objtacticcost - diffLinkCost;
                                                            diffLinkCost = 0;
                                                        }
                                                        else
                                                        {
                                                            linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                                            diffLinkCost = diffLinkCost - DiffMonthCost;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (endmonth > 0)
                                        {
                                            endmonth -= 1;
                                        }

                                    }

                                }
                                // Calculate Tactic Cost.
                                #region "Calculate LineItem cost for Linked Tactic"
                                //if (linkedTacticId > 0)
                                //{
                                //    List<Plan_Campaign_Program_Tactic_LineItem> tbllinkedTacLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                                //    double totalLinkedLineitemCost = 0;
                                //    tbllinkedTacLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == linkedTacticId).ToList();
                                //    List<Plan_Campaign_Program_Tactic_LineItem> objtotalLinkedLineitemCost = tbllinkedTacLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                //    //Modified By komal rawal
                                //    var linkedlineitemidlist = objtotalLinkedLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                //    List<Plan_Campaign_Program_Tactic_LineItem_Cost> LinkedLineitemCostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => linkedlineitemidlist.Contains(lic.PlanLineItemId)).ToList();
                                //    //End

                                //    if (objtotalLinkedLineitemCost != null && objtotalLinkedLineitemCost.Count() > 0)
                                //        totalLinkedLineitemCost = objtotalLinkedLineitemCost.Sum(l => l.Cost);
                                //    if (totalLinkedLineitemCost > linkedTactic.Cost)
                                //    {
                                //        form.Cost = totalLinkedLineitemCost;
                                //    }
                                //    //Added By komal Rawal for #1249
                                //    if (form.Cost > pcpobj.Cost)
                                //    {
                                //        var diffcost = form.Cost - pcpobj.Cost;
                                //        int startmonth = pcpobj.StartDate.Month;

                                //        if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                //        {
                                //            pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;

                                //        }
                                //        else
                                //        {
                                //            Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                //            objTacticCost.PlanTacticId = pcpobj.PlanTacticId;
                                //            objTacticCost.Period = PeriodChar + startmonth;
                                //            objTacticCost.Value = diffcost;
                                //            objTacticCost.CreatedBy = Sessions.User.UserId;
                                //            objTacticCost.CreatedDate = DateTime.Now;
                                //            db.Entry(objTacticCost).State = EntityState.Added;
                                //        }

                                //        //Add linked Tactic TacticCost data
                                //        int yearDiff = 0, perdNum = 12, cntr = 0;
                                //        bool isMultiYearlinkedTactic = false;
                                //        if (linkedTacticId > 0)
                                //        {
                                //            yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                //            isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                                //            if (isMultiYearlinkedTactic)
                                //            {
                                //                string linkedstartmonth = ((12 * yearDiff) + startmonth).ToString();
                                //                if (linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedstartmonth).Any())
                                //                {
                                //                    linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedstartmonth).FirstOrDefault().Value += diffcost;
                                //                }
                                //                //else
                                //                //{
                                //                //    Plan_Campaign_Program_Tactic_Cost lnkTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                //                //    lnkTacticCost.PlanTacticId = linkedTacticId;
                                //                //    lnkTacticCost.Period = PeriodChar + linkedstartmonth;
                                //                //    lnkTacticCost.Value = diffcost;
                                //                //    lnkTacticCost.CreatedBy = Sessions.User.UserId;
                                //                //    lnkTacticCost.CreatedDate = DateTime.Now;
                                //                //    db.Entry(lnkTacticCost).State = EntityState.Added;
                                //                //}
                                //            }
                                //        }
                                //    }
                                //    else if (form.Cost < pcpobj.Cost)
                                //    {
                                //        var diffcost = pcpobj.Cost - form.Cost;
                                //        double diffLinkCost = diffcost;
                                //        int endmonth = 12;
                                //        while (diffcost > 0 && endmonth != 0)
                                //        {
                                //            if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                //            {
                                //                //Modified by komal Rawal
                                //                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                                //                double objtacticcost = pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                //                var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                //                if (DiffMonthCost > 0)
                                //                {
                                //                    if (DiffMonthCost > diffcost)
                                //                    {
                                //                        pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                //                        diffcost = 0;
                                //                    }
                                //                    else
                                //                    {
                                //                        pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                //                        diffcost = diffcost - DiffMonthCost;
                                //                    }
                                //                }
                                //                //END
                                //            }

                                //            int yearDiff = 0, perdNum = 12, cntr = 0;
                                //            bool isMultiYearlinkedTactic = false;
                                //            if (linkedTacticId > 0)
                                //            {
                                //                yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                //                isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                                //                string linkedendmonth = ((12 * yearDiff) + endmonth).ToString();
                                //                if (isMultiYearlinkedTactic)
                                //                {
                                //                    if (linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).Any())
                                //                    {
                                //                        double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + linkedendmonth).Sum(lineitem => lineitem.Value);
                                //                        double objtacticcost = linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).FirstOrDefault().Value;
                                //                        var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                //                        if (DiffMonthCost > 0)
                                //                        {
                                //                            if (DiffMonthCost > diffLinkCost)
                                //                            {
                                //                                linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).FirstOrDefault().Value = objtacticcost - diffLinkCost;
                                //                                diffLinkCost = 0;
                                //                            }
                                //                            else
                                //                            {
                                //                                linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + linkedendmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                //                                diffLinkCost = diffLinkCost - DiffMonthCost;
                                //                            }
                                //                        }
                                //                    }
                                //                }
                                //            }

                                //            if (endmonth > 0)
                                //            {
                                //                endmonth -= 1;
                                //            }

                                //        }

                                //    }
                                //} 
                                #endregion


                                pcpobj.Cost = form.Cost;

                                //End

                                //pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.StageId = form.StageId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Modified;

                                if (linkedTacticId > 0)
                                {
                                    int yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                    linkedTactic.Title = pcpobj.Title;
                                    linkedTactic.Description = pcpobj.Description;
                                    //linkedTactic.TacticTypeId = pcpobj.TacticTypeId;

                                    #region "Update linked TacticType"
                                    int destModelId = linkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;
                                    string srcTacticTypeTitle = db.TacticTypes.FirstOrDefault(type => type.TacticTypeId == pcpobj.TacticTypeId).Title;
                                    TacticType destTacticType = db.TacticTypes.FirstOrDefault(_tacType => _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.IsDeployedToModel == true && _tacType.Title == srcTacticTypeTitle);
                                    //// Check whether source Entity TacticType in list of TacticType of destination Model exist or not.
                                    if (destTacticType != null)
                                    {
                                        linkedTactic.TacticTypeId = destTacticType.TacticTypeId;
                                        linkedTactic.ProjectedStageValue = destTacticType.ProjectedStageValue == null ? 0 : destTacticType.ProjectedStageValue;
                                        linkedTactic.StageId = destTacticType.StageId == null ? 0 : (int)destTacticType.StageId;
                                    }
                                    #endregion

                                    linkedTactic.CreatedBy = pcpobj.CreatedBy;
                                    //linkedTactic.ProjectedStageValue = pcpobj.ProjectedStageValue;
                                    linkedTactic.Status = pcpobj.Status;
                                    //linkedTactic.StartDate = pcpobj.StartDate;
                                    //linkedTactic.EndDate = pcpobj.EndDate;
                                    if (linkedTactic.Plan_Campaign_Program.StartDate > linkedTactic.StartDate)
                                    {
                                        linkedTactic.Plan_Campaign_Program.StartDate = linkedTactic.StartDate;
                                    }

                                    if (linkedTactic.EndDate > linkedTactic.Plan_Campaign_Program.EndDate)
                                    {
                                        linkedTactic.Plan_Campaign_Program.EndDate = linkedTactic.EndDate;
                                    }

                                    if (linkedTactic.Plan_Campaign_Program.Plan_Campaign.StartDate > linkedTactic.StartDate)
                                    {
                                        linkedTactic.Plan_Campaign_Program.Plan_Campaign.StartDate = linkedTactic.StartDate;
                                    }

                                    if (linkedTactic.EndDate > linkedTactic.Plan_Campaign_Program.Plan_Campaign.EndDate)
                                    {
                                        linkedTactic.Plan_Campaign_Program.Plan_Campaign.EndDate = linkedTactic.EndDate;
                                    }
                                    List<Plan_Campaign_Program_Tactic_Cost> lstLinkeTac = new List<Plan_Campaign_Program_Tactic_Cost>();
                                    lstLinkeTac = db.Plan_Campaign_Program_Tactic_Cost.Where(per => per.PlanTacticId == linkedTacticId).ToList();
                                    if (yearDiff > 0 && lstLinkeTac != null && lstLinkeTac.Count > 0) // is MultiYear Tactic
                                    {
                                        lstLinkeTac = lstLinkeTac.Where(per => int.Parse(per.Period.Replace(PeriodChar, string.Empty)) > 12).ToList();
                                    }

                                    if (lstLinkeTac != null && lstLinkeTac.Count > 0)
                                    {
                                        linkedTactic.Cost = lstLinkeTac.Sum(tac => tac.Value);
                                    }
                                    linkedTactic.IsDeployedToIntegration = pcpobj.IsDeployedToIntegration;
                                    linkedTactic.IsSyncSalesForce = pcpobj.IsSyncSalesForce;
                                    linkedTactic.IsSyncEloqua = pcpobj.IsSyncEloqua;
                                    linkedTactic.IsSyncMarketo = pcpobj.IsSyncMarketo;
                                    linkedTactic.IsSyncWorkFront = pcpobj.IsSyncWorkFront;
                                    //linkedTactic.StageId = form.StageId;
                                    linkedTactic.ProjectedStageValue = form.ProjectedStageValue;
                                    linkedTactic.ModifiedBy = Sessions.User.UserId;
                                    linkedTactic.ModifiedDate = DateTime.Now;
                                    db.Entry(pcpobj).State = EntityState.Modified;
                                }



                                //Start by Kalpesh Sharma #605: Cost allocation for Tactic
                                var PrevAllocationList = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).Select(_tacCost => _tacCost).ToList();  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759

                                int result;
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    result = Common.InsertChangeLog(planid, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                }

                                if (isReSubmission && Common.CheckAfterApprovedStatus(status))
                                {
                                    pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                    //// Get URL for Tactic to send in Email.
                                    string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign_Program.Plan_Campaign.PlanId, form.PlanTacticId, Enums.Section.Tactic.ToString().ToLower());
                                    Common.mailSendForTactic(pcpobj.PlanTacticId, pcpobj.Status, pcpobj.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower(), URL: strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                }
                                result = db.SaveChanges();
                                // Start - Added by Sohel Pathan on 14/11/2014 for PL ticket #708
                                if (result > 0)
                                {

                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != oldOwnerId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", oldOwnerId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.
                                                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                                                    //// Ticket: 942 Exception handeling in Gameplan.
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = pcpobj.Plan_Campaign_Program.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = pcpobj.Plan_Campaign_Program.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign_Program.Plan_Campaign.PlanId, form.PlanTacticId, Enums.Section.Tactic.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcpobj.Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Tactic.ToString().ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }

                                    }

                                }
                                // End - Added by Sohel Pathan on 14/11/2014 for PL ticket #708
                                // Start Added by dharmraj for ticket #644

                                //// Calculate TotalLineItem cost.



                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);

                                if (objOtherLineItem == null)
                                {
                                    if (pcpobj.Cost > 0)// Add condition by Nishant sheth // desc :: To restrict other line item is not created when tactic planned cost is 0
                                    {
                                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        objNewLineitem.PlanTacticId = pcpobj.PlanTacticId;
                                        objNewLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
                                        if (pcpobj.Cost > totalLineitemCost)
                                        {
                                            objNewLineitem.Cost = pcpobj.Cost - totalLineitemCost;
                                        }
                                        else
                                        {
                                            objNewLineitem.Cost = 0;
                                        }
                                        objNewLineitem.Description = string.Empty;
                                        objNewLineitem.CreatedBy = Sessions.User.UserId;
                                        objNewLineitem.CreatedDate = DateTime.Now;
                                        db.Entry(objNewLineitem).State = EntityState.Added;

                                        if (linkedTacticId > 0)
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem objNewLinkedLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                            objNewLinkedLineitem.PlanTacticId = linkedTacticId;
                                            objNewLinkedLineitem.Title = objNewLineitem.Title;
                                            objNewLinkedLineitem.Cost = objNewLineitem.Cost;
                                            objNewLinkedLineitem.Description = string.Empty;
                                            objNewLinkedLineitem.CreatedBy = Sessions.User.UserId;
                                            objNewLinkedLineitem.CreatedDate = DateTime.Now;
                                            objNewLinkedLineitem.LinkedLineItemId = objNewLineitem.PlanLineItemId;
                                            db.Entry(objNewLinkedLineitem).State = EntityState.Added;
                                            db.SaveChanges();

                                            objNewLineitem.LinkedLineItemId = objNewLinkedLineitem.PlanLineItemId;
                                            db.Entry(objNewLineitem).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                    }
                                }
                                else
                                {
                                    objOtherLineItem.IsDeleted = false;
                                    if (pcpobj.Cost > totalLineitemCost)
                                    {
                                        objOtherLineItem.Cost = pcpobj.Cost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;

                                    #region "Updte linked other lineItem"
                                    if (linkedTacticId > 0)
                                    {
                                        List<Plan_Campaign_Program_Tactic_LineItem> tblLinkedTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                                        //double totalLineitemCost = 0;
                                        tblLinkedTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == linkedTacticId).ToList();

                                        List<Plan_Campaign_Program_Tactic_LineItem> objtotalLinkedLineitemCost = tblLinkedTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                        //objtotalLinkedLineitemCost
                                        double totalLinkedLineitemCost = 0;
                                        if (objtotalLinkedLineitemCost != null && objtotalLinkedLineitemCost.Count() > 0)
                                            totalLinkedLineitemCost = objtotalLinkedLineitemCost.Sum(l => l.Cost);

                                        Plan_Campaign_Program_Tactic_LineItem objLinkedOtherLineItem = tblLinkedTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);

                                        objLinkedOtherLineItem.IsDeleted = false;
                                        if (linkedTactic.Cost > totalLineitemCost)
                                        {
                                            objLinkedOtherLineItem.Cost = linkedTactic.Cost - totalLineitemCost;
                                        }
                                        else
                                        {
                                            objLinkedOtherLineItem.Cost = 0;
                                            objLinkedOtherLineItem.IsDeleted = true;
                                        }
                                        db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
                                    }
                                    #endregion

                                }

                                // End Added by dharmraj for ticket #644

                                ////Start modified by Mitesh Vaishnav for PL ticket #1073 multiselect attributes
                                //// delete previous custom field values and save modified custom fields value for particular Tactic
                                string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                                List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == pcpobj.PlanTacticId && custField.CustomField.EntityType == entityTypeTactic).ToList();
                                prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    CustomField_Entity objcustomFieldEntity;
                                    foreach (var item in customFields)
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = pcpobj.PlanTacticId;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        objcustomFieldEntity.Weightage = (byte)item.Weight;
                                        objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                                ////End modified by Mitesh Vaishnav for PL ticket #1073 multiselect attributes

                                if (linkedTacticId > 0)
                                {
                                    List<CustomField_Entity> prevLinkCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == linkedTacticId && custField.CustomField.EntityType == entityTypeTactic).ToList();
                                    prevLinkCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);

                                    if (customFields.Count != 0)
                                    {
                                        CustomField_Entity objcustomFieldEntity;
                                        foreach (var item in customFields)
                                        {
                                            objcustomFieldEntity = new CustomField_Entity();
                                            objcustomFieldEntity.EntityId = linkedTacticId;
                                            objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                            objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                            objcustomFieldEntity.CreatedDate = DateTime.Now;
                                            objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                            objcustomFieldEntity.Weightage = (byte)item.Weight;
                                            objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                            db.CustomField_Entity.Add(objcustomFieldEntity);
                                        }
                                    }
                                }

                                db.SaveChanges();

                                if (result >= 1)
                                {
                                    // Add By Nishant Sheth
                                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                    DataSet dsPlanCampProgTac = new DataSet();
                                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

                                    //// Start - Added by :- Mitesh Vaishnav on 19/05/2015 for PL ticket #546
                                    if (pcpobj.IntegrationInstanceTacticId != null && oldProgramId > 0)
                                    {
                                        if (pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance.IntegrationType.Code == Enums.IntegrationInstanceType.Salesforce.ToString())
                                        {
                                            ExternalIntegration externalIntegration = new ExternalIntegration(pcpobj.PlanTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Tactic, true);
                                            externalIntegration.Sync();
                                        }
                                    }

                                    if (oldProgramId > 0)
                                    {
                                        var actionSuffix = oldProgramTitle + " to " + pcpobj.Plan_Campaign_Program.Title;
                                        Common.InsertChangeLog(planid, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.moved, actionSuffix);
                                    }
                                    //// End - Added by :- Mitesh Vaishnav on 19/05/2015 for PL ticket #546
                                    //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                    Common.ChangeProgramStatus(pcpobj.PlanProgramId, false);
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == pcpobj.PlanProgramId).Select(program => program.PlanCampaignId).Single();
                                    Common.ChangeCampaignStatus(PlanCampaignId, false);
                                    if (oldProgramId > 0)
                                    {
                                        Common.ChangeProgramStatus(oldProgramId, false);
                                        Common.ChangeCampaignStatus(oldCampaignId, false);
                                    }
                                    //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                    scope.Complete();
                                    string strMessag = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { IsDuplicate = false, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }), Msg = strMessag, planTacticId = pcpobj.PlanTacticId, planCampaignId = cid, planProgramId = pid, tacticStatus = pcpobj.Status, EndDatediff = EndDatediff, PlanId = planid });// Modified By Nishant Sheth Desc:: #1812 refresh time frame dropdown
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { });
        }

        ///    <summary>
        /// Added By: Pratik Chauhan.
        /// Action to Create Tactic.
        /// </summary>
        /// <param name="id">Plan Program Id</param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        /// modified by Rahul Shah on 17/03/2016 for PL #2032 
        public ActionResult CreateTactic(int id = 0)
        {

            Plan_Campaign_Program pcpt = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id)).FirstOrDefault();
            //// Get PlanId by PlanCampaignId.
            Inspect_Popup_Plan_Campaign_Program_TacticModel pcptm = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            var objPlan = pcpt.Plan_Campaign.Plan;
            int PlanId = objPlan.PlanId;
            //// Get those Tactic types whose ModelId exist in Plan table and IsDeployedToModel = true.
            List<TacticType> tactics = (from _tacType in db.TacticTypes
                                        where _tacType.ModelId == objPlan.ModelId && (_tacType.IsDeleted == null || _tacType.IsDeleted == false) && _tacType.IsDeployedToModel == true //// Modified by Sohel Pathan on 17/07/2014 for PL ticket #594
                                        select _tacType).OrderBy(t => t.Title).ToList();
            foreach (var item in tactics)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }



            if (pcpt == null)
            {
                return null;
            }



            #region "Set Inspect_Popup_Plan_Campaign_Program_TacticModel to pass into Partialview"
            pcptm.PlanProgramId = id;
            pcptm.IsDeployedToIntegration = false;
            pcptm.StageId = 0;
            pcptm.StageTitle = "Stage";

            pcptm.ProgramTitle = HttpUtility.HtmlDecode(pcpt.Title);
            pcptm.CampaignTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign.Title);

            pcptm.StartDate = GetCurrentDateBasedOnPlan(false, PlanId);
            pcptm.EndDate = GetCurrentDateBasedOnPlan(true, PlanId);

            pcptm.TacticCost = 0;
            pcptm.AllocatedBy = objPlan.AllocatedBy;


            pcptm.Owner = (Sessions.User.FirstName + " " + Sessions.User.LastName).ToString();
            pcptm.Tactics = tactics;
            pcptm.IsCreated = true;
            pcptm.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            pcptm.IsOwner = true;
            pcptm.RedirectType = false;
            pcptm.Year = db.Plans.Single(p => p.PlanId.Equals(PlanId)).Year;
            pcptm.OwnerId = Sessions.User.UserId;

            #endregion

            //Commented By Maitri Gandhi on 28/3/2016 for #2073
            //if (tactics.ToList().Count == 1)
            //{
            //    pcptm.TacticTypeId = tactics.FirstOrDefault().TacticTypeId;
            //}
            // Added by Rahul Shah on 17/03/2016 for PL #2032 
            pcptm.IsTackticAddEdit = true;
            #region "Owner List"
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); // PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);

                if (lstClientUsers.Count() > 0)
                {
                    //// Flag to indicate unavailability of web service.
                    ViewBag.IsServiceUnavailable = false;
                    string strUserList = string.Join(",", lstClientUsers);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId);
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        pcptm.OwnerList = lstPreparedOwners.Select(u => new SelectListUser { Name = u.DisplayName, Id = u.UserId }).ToList();
                    }
                    else
                    {
                        pcptm.OwnerList = new List<SelectListUser>();
                    }
                }
                else
                {
                    pcptm.OwnerList = new List<SelectListUser>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    ViewBag.IsServiceUnavailable = true;
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            #endregion
            return PartialView("SetupEditAdd", pcptm);
        }

        /// <summary>
        /// Calculate MQL Conerstion Rate based on Session Plan Id.
        /// Added by Bhavesh Dobariya.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="projectedStageValue"></param>
        /// <param name="RedirectType"></param>
        /// <param name="isTacticTypeChange"></param>
        /// <returns>JsonResult MQl Rate.</returns>
        public JsonResult CalculateMQL(Inspect_Popup_Plan_Campaign_Program_TacticModel form, double projectedStageValue, bool RedirectType, bool isTacticTypeChange)
        {
            DateTime StartDate = new DateTime();
            string stageMQL = Enums.Stage.MQL.ToString();
            int tacticStageLevel = 0;
            int levelMQL = db.Stages.Single(stage => stage.ClientId.Equals(Sessions.User.ClientId) && stage.Code.Equals(stageMQL) && stage.IsDeleted == false).Level.Value;

            int planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == (db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == form.PlanProgramId && pcp.IsDeleted.Equals(false)).Select(pcp => pcp.PlanCampaignId).FirstOrDefault()) && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            if (form.PlanTacticId != 0)
            {
                if (isTacticTypeChange)
                {
                    tacticStageLevel = Convert.ToInt32(db.TacticTypes.FirstOrDefault(t => t.TacticTypeId == form.TacticTypeId).Stage.Level);
                }
                else
                {
                    tacticStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(t => t.StageId == form.StageId).Level);
                }

                if (RedirectType)
                {
                    StartDate = form.StartDate;
                }
                else
                {
                    StartDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == form.PlanTacticId).Select(t => t.StartDate).FirstOrDefault();
                }



                int modelId = db.Plans.Where(p => p.PlanId == planid).Select(p => p.ModelId).FirstOrDefault();//Sessions.PlanId
                /// Added by Dharmraj on 4-Sep-2014
                /// #760 Advanced budgeting – show correct revenue in Tactic fly out
                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                objTactic.StartDate = StartDate;
                objTactic.EndDate = form.EndDate;
                objTactic.StageId = form.StageId;
                objTactic.Plan_Campaign_Program = new Plan_Campaign_Program() { Plan_Campaign = new Plan_Campaign() { PlanId = planid, Plan = new Plan() { } } };// Sessions.PlanId
                objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId = modelId;
                objTactic.ProjectedStageValue = projectedStageValue;
                var lstTacticStageRelation = Common.GetTacticStageRelationForSingleTactic(objTactic, false);
                double calculatedMQL = 0;
                double CalculatedRevenue = 0;
                calculatedMQL = lstTacticStageRelation.MQLValue;
                CalculatedRevenue = lstTacticStageRelation.RevenueValue;
                CalculatedRevenue = Math.Round(CalculatedRevenue, 2); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #760
                calculatedMQL = Math.Round(calculatedMQL, 0, MidpointRounding.AwayFromZero);
                if (tacticStageLevel < levelMQL)
                {
                    return Json(new { mql = calculatedMQL, revenue = CalculatedRevenue });
                }
                else if (tacticStageLevel == levelMQL)
                {
                    return Json(new { mql = projectedStageValue, revenue = CalculatedRevenue });
                }
                else if (tacticStageLevel > levelMQL)
                {
                    return Json(new { mql = "N/A", revenue = CalculatedRevenue });
                }
                else
                {
                    return Json(new { mql = 0, revenue = CalculatedRevenue });
                }
            }
            else
            {
                tacticStageLevel = Convert.ToInt32(db.TacticTypes.FirstOrDefault(_tacType => _tacType.TacticTypeId == form.TacticTypeId).Stage.Level);

                /// Added by Dharmraj on 4-Sep-2014
                /// #760 Advanced budgeting – show correct revenue in Tactic fly out
                int modelId = db.Plans.Where(plan => plan.PlanId == planid).Select(plan => plan.ModelId).FirstOrDefault();//Sessions.PlanId

                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();

                //// Set Tactic Start date.
                if (tacticStageLevel < levelMQL)
                    objTactic.StartDate = DateTime.Now;
                else
                    objTactic.StartDate = StartDate;

                objTactic.EndDate = form.EndDate;
                objTactic.StageId = form.StageId;
                objTactic.Plan_Campaign_Program = new Plan_Campaign_Program() { Plan_Campaign = new Plan_Campaign() { PlanId = planid, Plan = new Plan() { } } };//Sessions.PlanId
                objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId = modelId;
                objTactic.ProjectedStageValue = projectedStageValue;
                var lstTacticStageRelation = Common.GetTacticStageRelationForSingleTactic(objTactic, false);

                #region "Set MQL & Revenue value"

                double calculatedMQL = 0;
                double CalculatedRevenue = 0;
                calculatedMQL = lstTacticStageRelation.MQLValue;
                CalculatedRevenue = lstTacticStageRelation.RevenueValue;
                CalculatedRevenue = Math.Round(CalculatedRevenue, 2); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #760

                if (tacticStageLevel < levelMQL)
                {
                    return Json(new { mql = calculatedMQL, revenue = CalculatedRevenue });
                }
                else if (tacticStageLevel == levelMQL)
                {
                    return Json(new { mql = projectedStageValue, revenue = CalculatedRevenue });
                }
                else if (tacticStageLevel > levelMQL)
                {
                    return Json(new { mql = "N/A", revenue = CalculatedRevenue });
                }
                else
                {
                    return Json(new { mql = 0, revenue = CalculatedRevenue });
                }

                #endregion
            }
        }

        #region Budget Tactic for Campaign Tab
        // Start - Commented by Arpita Soni for Ticket #2236 on 06/07/2016

        ///// <summary>
        ///// Fetch the Tactic Budget Allocation 
        ///// </summary>
        ///// <param name="id">Campaign Id</param>
        ///// <returns></returns>
        //public PartialViewResult LoadTacticBudgetAllocation(int id = 0)
        //{
        //    Plan_Campaign_Program_Tactic pcp = db.Plan_Campaign_Program_Tactic.Where(pcpobj => pcpobj.PlanTacticId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
        //    if (pcp == null)
        //    {
        //        return null;
        //    }
        //    Plan_Campaign_Program_TacticModel pcpm = new Plan_Campaign_Program_TacticModel();
        //    pcpm.PlanProgramId = pcp.PlanProgramId;
        //    pcpm.PlanTacticId = pcp.PlanTacticId;

        //    // Add by Nishant sheth
        //    // Desc :: #1765 - to get the year diffrence between item start date and end date
        //    ViewBag.YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcp.EndDate.Year) - Convert.ToInt32(pcp.StartDate.Year));
        //    ViewBag.StartYear = Convert.ToInt32(pcp.StartDate.Year);

        //    string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
        //    string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
        //    //Modified BY komal Rawal
        //    pcpm.TacticBudget = pcp.TacticBudget;
        //    //End
        //    pcpm.AllocatedBy = pcp.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy;

        //    #region "Calculate Plan remaining budget"
        //    //Added By : Kalpesh Sharma Functioan and code review #693
        //    var CostTacticsBudget = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == pcpm.PlanProgramId && c.IsDeleted == false).ToList().Sum(c => c.TacticBudget);
        //    var objPlanCampaignProgram = db.Plan_Campaign_Program.FirstOrDefault(p => p.PlanProgramId == pcpm.PlanProgramId);
        //    ViewBag.planRemainingBudget = (objPlanCampaignProgram.ProgramBudget - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));
        //    #endregion

        //    return PartialView("_SetupTacticBudgetAllocation", pcpm);
        //}

        ///// <summary>
        ///// Action to Save Campaign Allocation.
        ///// </summary>
        ///// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        ///// <param name="BudgetInputValues">Budget Input Values.</param>
        ///// <param name="UserId">User Id.</param>
        ///// <param name="title"></param>
        ///// <returns>Returns Action Result.</returns>
        //[HttpPost]
        //public ActionResult SaveTacticBudgetAllocation(Plan_Campaign_Program_TacticModel form, string BudgetInputValues, string UserId = "", string title = "", string AllocatedBy = "", int YearDiffrence = 0)
        //{
        //    //// check whether UserId is loggined user or not.
        //    if (!string.IsNullOrEmpty(UserId))
        //    {
        //        if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
        //        {
        //            return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
        //        }
        //    }

        //    try
        //    {
        //        string[] arrBudgetInputValues = BudgetInputValues.Split(',');
        //        //Added by Komal Rawal for #1217
        //        string budgetvalue = BudgetInputValues.Replace(',', ' ').Trim();
        //        bool isvalueempty = budgetvalue != string.Empty ? true : false;
        //        //end
        //        using (MRPEntities mrp = new MRPEntities())
        //        {
        //            //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
        //            var TransactionOption = new System.Transactions.TransactionOptions();
        //            TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

        //            using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
        //            // using (var scope = new TransactionScope())
        //            {

        //                //List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.IsDeleted == false).ToList();
        //                int linkedTacticId = 0;
        //                Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).FirstOrDefault();

        //                #region "Retrieve linkedTactic"
        //                linkedTacticId = (pcpobj != null && pcpobj.LinkedTacticId.HasValue) ? pcpobj.LinkedTacticId.Value : 0;
        //                //if (linkedTacticId <= 0)
        //                //{
        //                //    var lnkPCPT = tblPlanTactic.Where(tac => tac.LinkedTacticId == form.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
        //                //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
        //                //}
        //                #endregion

        //                //// Get duplicate record to check duplication.
        //                var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
        //                              join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
        //                              join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
        //                              where pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(form.PlanTacticId) && pcpt.IsDeleted.Equals(false)
        //                              && pcp.PlanProgramId == form.PlanProgramId
        //                              select pcp).FirstOrDefault();

        //                //// Get Linked Tactic duplicate record.
        //                Plan_Campaign_Program_Tactic dupLinkedTactic = null;
        //                Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
        //                int yearDiff = 0, perdNum = 12, cntr = 0;
        //                bool isMultiYearlinkedTactic = false;
        //                List<string> lstLinkedPeriods = new List<string>();

        //                if (linkedTacticId > 0)
        //                {
        //                    linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault();

        //                    dupLinkedTactic = (from pcpt in db.Plan_Campaign_Program_Tactic
        //                                       join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
        //                                       join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
        //                                       where pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(linkedTacticId) && pcpt.IsDeleted.Equals(false)
        //                                       && pcp.PlanProgramId == linkedTactic.PlanProgramId
        //                                       select pcpt).FirstOrDefault();

        //                    yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
        //                    isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
        //                    cntr = 12 * yearDiff;
        //                    for (int i = 1; i <= cntr; i++)
        //                    {
        //                        lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
        //                    }
        //                }

        //                //// if duplicate record exist then return duplication message.
        //                if (dupLinkedTactic != null)
        //                {
        //                    string strDuplicateMessage = string.Format(Common.objCached.LinkedPlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
        //                    return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
        //                }
        //                else if (pcpvar != null)
        //                {
        //                    string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
        //                    return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    string status = string.Empty;
        //                    List<Plan_Campaign_Program_Tactic_Budget> tblTacticBudget = new List<Plan_Campaign_Program_Tactic_Budget>();

        //                    pcpobj.Title = title;
        //                    pcpobj.TacticBudget = form.TacticBudget; // modified for 1229
        //                    pcpobj.ModifiedBy = Sessions.User.UserId;
        //                    pcpobj.ModifiedDate = DateTime.Now;

        //                    //if (linkedTacticId > 0)
        //                    //{
        //                    //    Plan_Campaign_Program_Tactic objLinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(linkedTacticId)).FirstOrDefault();
        //                    //    objLinkedTactic.Title = title;
        //                    //    objLinkedTactic.TacticBudget = form.TacticBudget; // modified for 1229
        //                    //    objLinkedTactic.ModifiedBy = Sessions.User.UserId;
        //                    //    objLinkedTactic.ModifiedDate = pcpobj.ModifiedDate;
        //                    //}

        //                    int startmonth = pcpobj.StartDate.Month;
        //                    if (linkedTacticId > 0)
        //                        tblTacticBudget = db.Plan_Campaign_Program_Tactic_Budget.Where(_tacCost => (_tacCost.PlanTacticId == form.PlanTacticId) || (_tacCost.PlanTacticId == linkedTacticId)).ToList();
        //                    else
        //                        tblTacticBudget = db.Plan_Campaign_Program_Tactic_Budget.Where(_tacCost => (_tacCost.PlanTacticId == form.PlanTacticId)).ToList();

        //                    //Start by Kalpesh Sharma #605: Cost allocation for Tactic
        //                    List<Plan_Campaign_Program_Tactic_Budget> PrevAllocationList = tblTacticBudget.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).Select(_tacCost => _tacCost).ToList();  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759

        //                    //// Process for Monthly budget values.
        //                    // Change by Nishant sheth
        //                    // Desc :: #1765 - to replace the lenth of array to allocated by
        //                    if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                    {
        //                        bool isExists;
        //                        Plan_Campaign_Program_Tactic_Budget updatePlanTacticBudget, obPlanCampaignProgramTacticBudget;
        //                        double newValue = 0;
        //                        for (int i = 0; i < arrBudgetInputValues.Length; i++)
        //                        {
        //                            // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
        //                            isExists = false;
        //                            if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                            {
        //                                //// Get previous campaign budget values by Period.
        //                                updatePlanTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                                updatePlanTacticBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
        //                                if (updatePlanTacticBudget != null)
        //                                {
        //                                    if (arrBudgetInputValues[i] != "")
        //                                    {
        //                                        newValue = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                        if (updatePlanTacticBudget.Value != newValue)
        //                                        {
        //                                            //// Update Tactic budget value with newValue.
        //                                            updatePlanTacticBudget.Value = newValue;
        //                                            db.Entry(updatePlanTacticBudget).State = EntityState.Modified;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        db.Entry(updatePlanTacticBudget).State = EntityState.Deleted;
        //                                    }
        //                                    isExists = true;
        //                                }
        //                            }

        //                            //// if Old budget value does not exist then insert new value to table.
        //                            if (!isExists && arrBudgetInputValues[i] != "")
        //                            {
        //                                // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
        //                                obPlanCampaignProgramTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                                obPlanCampaignProgramTacticBudget.PlanTacticId = form.PlanTacticId;
        //                                obPlanCampaignProgramTacticBudget.Period = PeriodChar + (i + 1);
        //                                obPlanCampaignProgramTacticBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                obPlanCampaignProgramTacticBudget.CreatedBy = Sessions.User.UserId;
        //                                obPlanCampaignProgramTacticBudget.CreatedDate = DateTime.Now;
        //                                db.Entry(obPlanCampaignProgramTacticBudget).State = EntityState.Added;
        //                            }
        //                        }
        //                    }
        //                    // Change by Nishant sheth
        //                    // Desc :: #1765 - to replace the lenth of array to allocated by
        //                    else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower())
        //                    {
        //                        int m = 0;
        //                        int BudgetInputValuesCounter = 1, j = 1;
        //                        for (int k = 1; k <= (YearDiffrence + 1); k++)
        //                        {
        //                            //Added by Komal Rawal for #1217
        //                            if (startmonth >= 10 * k)
        //                            {
        //                                startmonth = 10 * k;
        //                            }
        //                            else if (startmonth >= 7 * k)
        //                            {
        //                                startmonth = 7 * k;
        //                            }
        //                            else if (startmonth >= 4 * k)
        //                            {
        //                                startmonth = 4 * k;
        //                            }
        //                            else
        //                            {
        //                                startmonth = 1 * k;
        //                            }
        //                            //End

        //                            bool isExists;
        //                            List<Plan_Campaign_Program_Tactic_Budget> thisQuartersMonthList;
        //                            Plan_Campaign_Program_Tactic_Budget thisQuarterFirstMonthBudget, obPlanCampaignProgramTacticBudget;
        //                            double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
        //                            for (int i = m; i < (4 * k); i++)
        //                            {
        //                                if ((i + 1) % 4 == 0)
        //                                {
        //                                    m = i + 1;
        //                                }
        //                                // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
        //                                isExists = false;
        //                                if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                                {
        //                                    //// Get Quarter budget list.
        //                                    thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_Budget>();
        //                                    thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 1)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 2))).ToList().OrderBy(a => a.Period).ToList();

        //                                    //// Get First month values from Quarterly budget list.
        //                                    thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                                    thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

        //                                    if (thisQuarterFirstMonthBudget != null)
        //                                    {
        //                                        if (arrBudgetInputValues[i] != "")
        //                                        {
        //                                            thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
        //                                            thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
        //                                            newValue = Convert.ToDouble(arrBudgetInputValues[i]);

        //                                            if (thisQuarterTotalBudget != newValue)
        //                                            {
        //                                                BudgetDiff = newValue - thisQuarterTotalBudget;
        //                                                if (BudgetDiff > 0)
        //                                                {
        //                                                    thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
        //                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                }
        //                                                else
        //                                                {
        //                                                    j = 1;
        //                                                    while (BudgetDiff < 0)
        //                                                    {
        //                                                        if (thisQuarterFirstMonthBudget != null)
        //                                                        {
        //                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

        //                                                            if (BudgetDiff <= 0)
        //                                                                thisQuarterFirstMonthBudget.Value = 0;
        //                                                            else
        //                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

        //                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                        }
        //                                                        if ((BudgetInputValuesCounter + j) <= (BudgetInputValuesCounter + 2))
        //                                                        {
        //                                                            thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter + j))).FirstOrDefault();
        //                                                        }

        //                                                        j = j + 1;
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
        //                                        }
        //                                        isExists = true;
        //                                    }
        //                                }

        //                                //// if Old budget value does not exist then insert new value to table.
        //                                if (!isExists && arrBudgetInputValues[i] != "")
        //                                {
        //                                    // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
        //                                    obPlanCampaignProgramTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                                    obPlanCampaignProgramTacticBudget.PlanTacticId = form.PlanTacticId;
        //                                    obPlanCampaignProgramTacticBudget.Period = PeriodChar + BudgetInputValuesCounter;
        //                                    obPlanCampaignProgramTacticBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
        //                                    obPlanCampaignProgramTacticBudget.CreatedBy = Sessions.User.UserId;
        //                                    obPlanCampaignProgramTacticBudget.CreatedDate = DateTime.Now;
        //                                    db.Entry(obPlanCampaignProgramTacticBudget).State = EntityState.Added;
        //                                }
        //                                BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
        //                            }
        //                        }
        //                    }

        //                    //Added by Komal Rawal for #1217
        //                    if (!isvalueempty && pcpobj.TacticBudget > 0)
        //                    {
        //                        Plan_Campaign_Program_Tactic_Budget obPlanCampaignProgramTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                        var isBudExist = tblTacticBudget.Where(tact => tact.Period == PeriodChar + startmonth).Any();
        //                        if (isBudExist)
        //                        {
        //                            obPlanCampaignProgramTacticBudget = tblTacticBudget.Where(tact => tact.Period == PeriodChar + startmonth).FirstOrDefault();
        //                            obPlanCampaignProgramTacticBudget.Value = pcpobj.TacticBudget;
        //                            db.Entry(obPlanCampaignProgramTacticBudget).State = EntityState.Modified;
        //                        }
        //                        else
        //                        {
        //                            //Plan_Campaign_Program_Tactic_Budget obPlanCampaignProgramTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                            obPlanCampaignProgramTacticBudget.PlanTacticId = form.PlanTacticId;
        //                            obPlanCampaignProgramTacticBudget.Period = PeriodChar + startmonth;
        //                            obPlanCampaignProgramTacticBudget.Value = pcpobj.TacticBudget;
        //                            obPlanCampaignProgramTacticBudget.CreatedBy = Sessions.User.UserId;
        //                            obPlanCampaignProgramTacticBudget.CreatedDate = DateTime.Now;
        //                            db.Entry(obPlanCampaignProgramTacticBudget).State = EntityState.Added;
        //                        }
        //                    }
        //                    //End

        //                    db.Entry(pcpobj).State = EntityState.Modified;
        //                    int result;
        //                    result = db.SaveChanges();

        //                    List<Plan_Campaign_Program_Tactic_Budget> lstsrcBudgetData = db.Plan_Campaign_Program_Tactic_Budget.Where(tac => (tac.PlanTacticId == form.PlanTacticId)).ToList();
        //                    //List<Plan_Campaign_Program_Tactic_Budget> lstSrcBudgetData = lstAllBudgetData.Where(tac => tac.PlanTacticId == form.PlanTacticId).ToList();
        //                    double totalSrcBudget = 0;  // Reset value.
        //                    if (lstsrcBudgetData != null && lstsrcBudgetData.Count > 0)
        //                        totalSrcBudget = lstsrcBudgetData.Sum(bdgt => bdgt.Value);
        //                    pcpobj.TacticBudget = totalSrcBudget;

        //                    if (linkedTacticId > 0)
        //                    {
        //                        #region "Update Linked Tactic Budget data"

        //                        // Remove old linked tactic budget data.
        //                        List<Plan_Campaign_Program_Tactic_Budget> lstBudgetData = db.Plan_Campaign_Program_Tactic_Budget.Where(tac => tac.PlanTacticId == form.PlanTacticId).ToList();
        //                        if (lstBudgetData != null && lstBudgetData.Count > 0)
        //                        {
        //                            //double totalLinkedBudget = 0;
        //                            //bool islinkedBudgetModified = false;
        //                            List<Plan_Campaign_Program_Tactic_Budget> linkedBudgetData = new List<Plan_Campaign_Program_Tactic_Budget>();
        //                            Plan_Campaign_Program_Tactic_Budget objlinkedBudget = null;
        //                            if (isMultiYearlinkedTactic)
        //                            {
        //                                // Delete old budget data.
        //                                linkedBudgetData = tblTacticBudget.Where(tac => tac.PlanTacticId == linkedTacticId && lstLinkedPeriods.Contains(tac.Period)).ToList();
        //                                if (linkedBudgetData != null && linkedBudgetData.Count > 0)
        //                                    linkedBudgetData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);

        //                                foreach (Plan_Campaign_Program_Tactic_Budget budget in lstBudgetData)
        //                                {
        //                                    string orgPeriod = budget.Period;
        //                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
        //                                    int NumPeriod = int.Parse(numPeriod);

        //                                    objlinkedBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                                    objlinkedBudget.PlanTacticId = linkedTacticId;
        //                                    objlinkedBudget.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
        //                                    objlinkedBudget.Value = budget.Value;
        //                                    objlinkedBudget.CreatedDate = budget.CreatedDate;
        //                                    objlinkedBudget.CreatedBy = budget.CreatedBy;
        //                                    db.Entry(objlinkedBudget).State = EntityState.Added;
        //                                    //islinkedBudgetModified = true;
        //                                    //totalLinkedBudget += budget.Value;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                // Delete old budget data.
        //                                linkedBudgetData = tblTacticBudget.Where(tac => tac.PlanTacticId == linkedTacticId).ToList();
        //                                if (linkedBudgetData != null && linkedBudgetData.Count > 0)
        //                                    linkedBudgetData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                foreach (Plan_Campaign_Program_Tactic_Budget budget in lstBudgetData)
        //                                {
        //                                    string orgPeriod = budget.Period;
        //                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
        //                                    int NumPeriod = int.Parse(numPeriod);
        //                                    if (NumPeriod > 12)
        //                                    {
        //                                        int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
        //                                        int div = NumPeriod / 12;    // In case of 24, Y12.
        //                                        if (rem > 0 || div > 1)
        //                                        {
        //                                            objlinkedBudget = new Plan_Campaign_Program_Tactic_Budget();
        //                                            objlinkedBudget.PlanTacticId = linkedTacticId;
        //                                            objlinkedBudget.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3)     
        //                                            objlinkedBudget.Value = budget.Value;
        //                                            objlinkedBudget.CreatedDate = budget.CreatedDate;
        //                                            objlinkedBudget.CreatedBy = budget.CreatedBy;
        //                                            db.Entry(objlinkedBudget).State = EntityState.Added;
        //                                            //islinkedBudgetModified = true;
        //                                            //totalLinkedBudget += budget.Value;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            //lstBudgetData.ForEach(bdgt => { bdgt.PlanTacticId = linkedTacticId; db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_Budget.Add(bdgt); });
        //                            db.SaveChanges();
        //                        }
        //                        else
        //                        {
        //                            // if linked tactic Multiyear tactic then remove all common year values.
        //                            if (isMultiYearlinkedTactic)
        //                            {
        //                                List<Plan_Campaign_Program_Tactic_Budget> linkedBudgetData = new List<Plan_Campaign_Program_Tactic_Budget>();
        //                                // Delete old budget data.
        //                                linkedBudgetData = tblTacticBudget.Where(tac => tac.PlanTacticId == linkedTacticId && lstLinkedPeriods.Contains(tac.Period)).ToList();
        //                                if (linkedBudgetData != null && linkedBudgetData.Count > 0)
        //                                {
        //                                    linkedBudgetData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                    db.SaveChanges();
        //                                }
        //                            }
        //                            //if (linkedTacticId > 0)
        //                            //{
        //                            //    double totalLinkedBudget = 0;
        //                            //    List<Plan_Campaign_Program_Tactic_Budget> lstNewLinkedBudgetData = db.Plan_Campaign_Program_Tactic_Budget.Where(tac => tac.PlanTacticId == linkedTacticId).ToList();
        //                            //    if (lstNewLinkedBudgetData != null && lstNewLinkedBudgetData.Count > 0)
        //                            //        totalLinkedBudget = lstNewLinkedBudgetData.Sum(bdgt => bdgt.Value);
        //                            //    //Plan_Campaign_Program_Tactic objLinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(linkedTacticId)).FirstOrDefault();
        //                            //    linkedTactic.Title = title;
        //                            //    linkedTactic.TacticBudget = totalLinkedBudget; // modified for 1229
        //                            //    linkedTactic.ModifiedBy = Sessions.User.UserId;
        //                            //    linkedTactic.ModifiedDate = pcpobj.ModifiedDate;
        //                            //}
        //                        }
        //                        if (linkedTacticId > 0)
        //                        {
        //                            double totalLinkedBudget = 0;
        //                            List<Plan_Campaign_Program_Tactic_Budget> lstNewLinkedBudgetData = db.Plan_Campaign_Program_Tactic_Budget.Where(tac => tac.PlanTacticId == linkedTacticId).ToList();
        //                            if (lstNewLinkedBudgetData != null && lstNewLinkedBudgetData.Count > 0)
        //                                totalLinkedBudget = lstNewLinkedBudgetData.Sum(bdgt => bdgt.Value);
        //                            //Plan_Campaign_Program_Tactic objLinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(linkedTacticId)).FirstOrDefault();
        //                            linkedTactic.Title = title;
        //                            linkedTactic.TacticBudget = totalLinkedBudget; // modified for 1229
        //                            linkedTactic.ModifiedBy = Sessions.User.UserId;
        //                            linkedTactic.ModifiedDate = pcpobj.ModifiedDate;
        //                        }
        //                        #endregion
        //                    }

        //                    // Start Added by dharmraj for ticket #644
        //                    //// Calculate Total LineItem Cost.
        //                    List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
        //                    if (linkedTacticId > 0)
        //                    {
        //                        tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => (lineItem.PlanTacticId == pcpobj.PlanTacticId) || (lineItem.PlanTacticId == linkedTacticId)).ToList();
        //                    }
        //                    else
        //                    {
        //                        tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId).ToList();
        //                    }
        //                    Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.Where(line => line.PlanTacticId == pcpobj.PlanTacticId).FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
        //                    var objtotalLoneitemCost = tblTacticLineItem.Where(line => line.PlanTacticId == pcpobj.PlanTacticId).Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false);
        //                    double totalLoneitemCost = 0;
        //                    if (objtotalLoneitemCost != null && objtotalLoneitemCost.Count() > 0)
        //                    {
        //                        totalLoneitemCost = objtotalLoneitemCost.Sum(l => l.Cost);
        //                    }

        //                    double totalLinkedLineitemCost = 0;
        //                    if (linkedTacticId > 0)
        //                    {
        //                        Plan_Campaign_Program_Tactic_LineItem objOtherLinkedLineItem = tblTacticLineItem.Where(line => line.PlanTacticId == linkedTacticId).FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
        //                        var objtotalLinkedLineitemCost = tblTacticLineItem.Where(line => line.PlanTacticId == pcpobj.PlanTacticId).Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false);
        //                        if (objtotalLinkedLineitemCost != null && objtotalLinkedLineitemCost.Count() > 0)
        //                        {
        //                            totalLinkedLineitemCost = objtotalLinkedLineitemCost.Sum(l => l.Cost);
        //                        }
        //                    }

        //                    if (objOtherLineItem == null)
        //                    {
        //                        Plan_Campaign_Program_Tactic_LineItem objLinkedLineitem = null;
        //                        if (linkedTacticId > 0)
        //                        {
        //                            objLinkedLineitem = new Plan_Campaign_Program_Tactic_LineItem();
        //                            objLinkedLineitem.PlanTacticId = linkedTacticId;
        //                            objLinkedLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
        //                            if (linkedTactic.Cost > totalLinkedLineitemCost)
        //                            {
        //                                objLinkedLineitem.Cost = linkedTactic.Cost - totalLinkedLineitemCost;
        //                            }
        //                            else
        //                            {
        //                                objLinkedLineitem.Cost = 0;
        //                            }
        //                            objLinkedLineitem.Description = string.Empty;
        //                            objLinkedLineitem.CreatedBy = Sessions.User.UserId;
        //                            objLinkedLineitem.CreatedDate = DateTime.Now;
        //                            db.Entry(objLinkedLineitem).State = EntityState.Added;
        //                            db.SaveChanges();
        //                        }

        //                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
        //                        objNewLineitem.PlanTacticId = pcpobj.PlanTacticId;
        //                        objNewLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
        //                        if (pcpobj.Cost > totalLoneitemCost)
        //                        {
        //                            objNewLineitem.Cost = pcpobj.Cost - totalLoneitemCost;
        //                        }
        //                        else
        //                        {
        //                            objNewLineitem.Cost = 0;
        //                        }
        //                        objNewLineitem.Description = string.Empty;
        //                        objNewLineitem.CreatedBy = Sessions.User.UserId;
        //                        objNewLineitem.CreatedDate = DateTime.Now;
        //                        if (objLinkedLineitem != null)
        //                            objNewLineitem.LinkedLineItemId = objLinkedLineitem.PlanLineItemId; // Insert linked Id.
        //                        db.Entry(objNewLineitem).State = EntityState.Added;

        //                        if (linkedTacticId > 0 && objLinkedLineitem != null)
        //                        {
        //                            db.SaveChanges();
        //                            objLinkedLineitem.LinkedLineItemId = objNewLineitem.PlanLineItemId;
        //                            db.Entry(objLinkedLineitem).State = EntityState.Modified;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        objOtherLineItem.IsDeleted = false;
        //                        if (pcpobj.Cost > totalLoneitemCost)
        //                        {
        //                            objOtherLineItem.Cost = pcpobj.Cost - totalLoneitemCost;
        //                        }
        //                        else
        //                        {
        //                            objOtherLineItem.Cost = 0;
        //                            objOtherLineItem.IsDeleted = true;
        //                        }
        //                        db.Entry(objOtherLineItem).State = EntityState.Modified;

        //                        if (linkedTacticId > 0)
        //                        {
        //                            int linkedLineItemId = 0;
        //                            #region "Retrieve linkedTactic"
        //                            linkedLineItemId = (objOtherLineItem != null && objOtherLineItem.LinkedLineItemId.HasValue) ? objOtherLineItem.LinkedLineItemId.Value : 0;
        //                            if (linkedLineItemId > 0)
        //                            {
        //                                var lnkLineItem = tblTacticLineItem.Where(tac => tac.PlanLineItemId == linkedLineItemId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
        //                                if (lnkLineItem != null)
        //                                {
        //                                    lnkLineItem.IsDeleted = false;
        //                                    if (linkedTactic.Cost > totalLoneitemCost)
        //                                    {
        //                                        lnkLineItem.Cost = pcpobj.Cost - totalLinkedLineitemCost;
        //                                    }
        //                                    else
        //                                    {
        //                                        lnkLineItem.Cost = 0;
        //                                    }
        //                                    db.Entry(lnkLineItem).State = EntityState.Modified;
        //                                }
        //                            }
        //                            //if (linkedLineItemId <= 0)
        //                            //{
        //                            //    List<Plan_Campaign_Program_Tactic_LineItem> fltrLineItems = tblTacticLineItem.Where(line => line.PlanTacticId == linkedTacticId).ToList();
        //                            //    var lnkLineItem = fltrLineItems.Where(tac => tac.LinkedLineItemId == objOtherLineItem.PlanLineItemId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
        //                            //    linkedLineItemId = lnkLineItem != null ? lnkLineItem.PlanLineItemId : 0;

        //                            //    if (lnkLineItem != null)
        //                            //    {
        //                            //        lnkLineItem.IsDeleted = false;
        //                            //        lnkLineItem.Cost = objOtherLineItem.Cost;
        //                            //        db.Entry(lnkLineItem).State = EntityState.Modified; 
        //                            //    }
        //                            //}
        //                            #endregion
        //                        }
        //                    }

        //                    db.SaveChanges();
        //                    scope.Complete();
        //                    string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
        //                    return Json(new { IsSaved = true, msg = strMessage, planTacticId = form.PlanTacticId, JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }

        //    return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        //}

        // End - Commented by Arpita Soni for Ticket #2236 on 06/07/2016

        /// <summary>
        /// Kalpesh Sharma : #752 Update line item cost with the total cost from the monthly/quarterly allocation
        /// </summary>
        /// <param name="arrBudgetInputValues">budget allocation value</param>
        /// <param name="EnteredCost">entered budget cost</param>
        /// <returns>Return the Sum of Budget Allocation </returns>
        public double UpdateBugdetAllocationCost(string[] arrBudgetInputValues, double enteredCost)
        {
            //Check the budget allocation value is greater then 0
            if (arrBudgetInputValues.Length > 0)
            {
                List<double> BudgetValues = new List<double>();
                bool IsExplcitValue = false;
                //Iterate all the values of  budget allocation.
                foreach (string item in arrBudgetInputValues)
                {
                    //If  budget allocation value is "" then Skip those value 
                    if (item != "")
                    {
                        BudgetValues.Add(Convert.ToDouble(item));
                        IsExplcitValue = true;
                    }
                }
                //Get the sum of budget allocation value
                double BugdetAllocationSum = BudgetValues.Sum();
                if (BugdetAllocationSum == 0 && !IsExplcitValue)
                {
                    // Return the entered budget cost
                    return enteredCost;
                }
                return BugdetAllocationSum;
            }
            return enteredCost;
        }

        #endregion

        #region fill Owner list
        /// <summary>
        /// fill Owner list based on custom fields with ViewEdit rights
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>18/11/2014</CreatedDate>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public JsonResult fillOwner(string UserId = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                if (lstClientUsers.Count() > 0)
                {
                    string strUserList = string.Join(",", lstClientUsers);

                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        return Json(new { isSuccess = true, lstOwner = lstPreparedOwners }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { isSuccess = true, lstOwner = new List<User>() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// Added By: Maninder Singh.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Action to Save Comment & Update Status as of selected tactic.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="isApprove"></param>
        /// <param name="isImprovement"></param>
        /// <returns>Returns flag to indicate whether operation was successfull or not.</returns>
        [HttpPost]
        public JsonResult ApproveDeclineTactic(int planTacticId, bool isApprove, bool isImprovement)
        {
            int result = 0;
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (ModelState.IsValid)
                        {
                            string status = isApprove ? Enums.TacticStatusValues.Single(t => t.Key.Equals(Enums.TacticStatus.Approved.ToString())).Value : Enums.TacticStatusValues.Single(t => t.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

                            /// Modified By Maninder Singh Wadhva PL Ticket#47
                            /// Check whether Tactic is Improvement Tactic or not.
                            if (isImprovement)
                            {
                                Plan_Improvement_Campaign_Program_Tactic improvementPlanTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId.Equals(planTacticId)).FirstOrDefault();
                                improvementPlanTactic.Status = status;
                                improvementPlanTactic.ModifiedBy = Sessions.User.UserId;
                                improvementPlanTactic.ModifiedDate = DateTime.Now;

                                #region "Insert ImprovementTactic Comment to Plan_Improvement_Campaign_Program_Tactic_Comment table"
                                Plan_Improvement_Campaign_Program_Tactic_Comment improvementPlanTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment()
                                {
                                    ImprovementPlanTacticId = planTacticId,
                                    //changes done from displayname to FName and Lname by uday for internal issue on 27-6-2014
                                    Comment = string.Format("Improvement Tactic {0} by {1}", status, Sessions.User.FirstName + " " + Sessions.User.LastName),
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = Sessions.User.UserId
                                };

                                db.Entry(improvementPlanTacticComment).State = EntityState.Added;
                                #endregion

                                db.Entry(improvementPlanTactic).State = EntityState.Modified;
                                result = db.SaveChanges();
                                if (result == 2)
                                {
                                    if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planTacticId, improvementPlanTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved);
                                        //added by uday for #532
                                        if (improvementPlanTactic.IsDeployedToIntegration == true)
                                        {
                                            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.ImprovementTactic);
                                            externalIntegration.Sync();
                                        }
                                        //added by uday for #532
                                    }
                                    else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planTacticId, improvementPlanTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);
                                    }
                                    //// Get URL for Tactic to send to Email.
                                    string strUrl = GetNotificationURLbyStatus(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, planTacticId, Convert.ToString(Enums.Section.ImprovementTactic).ToLower()); // Added by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    Common.mailSendForTactic(planTacticId, status, improvementPlanTactic.Title, section: Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), URL: strUrl); // Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    if (result >= 1)
                                    {
                                        // Add By Nishant Sheth
                                        // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                        DataSet dsPlanCampProgTac = new DataSet();
                                        dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                        objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                        List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                        objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                        var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                        objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                        var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                        objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                        var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                        objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                        var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                        objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

                                        scope.Complete();
                                        return Json(new { result = true }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic tactic = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == planTacticId).FirstOrDefault();
                                bool isApproved = false;
                                DateTime todaydate = DateTime.Now;
                                if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                {
                                    tactic.Status = status;
                                    isApproved = true;
                                    if (todaydate > tactic.StartDate && todaydate < tactic.EndDate)
                                    {
                                        tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                    }
                                    else if (todaydate > tactic.EndDate)
                                    {
                                        tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                    }
                                }
                                else
                                {
                                    tactic.Status = status;
                                }
                                tactic.ModifiedBy = Sessions.User.UserId;
                                tactic.ModifiedDate = DateTime.Now;

                                Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment()
                                {
                                    PlanTacticId = planTacticId,
                                    //changes done from displayname to FName and Lname by uday for internal issue on 27-6-2014
                                    Comment = string.Format("Tactic {0} by {1}", status, Sessions.User.FirstName + " " + Sessions.User.LastName),
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = Sessions.User.UserId
                                };

                                db.Entry(pcptc).State = EntityState.Added;
                                db.Entry(tactic).State = EntityState.Modified;
                                result = db.SaveChanges();
                                if (result == 2)
                                {
                                    if (isApproved)
                                    {
                                        result = Common.InsertChangeLog(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, 0, planTacticId, tactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved);
                                        //// added by uday for #532
                                        if (tactic.IsDeployedToIntegration == true)
                                        {
                                            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Tactic);
                                            externalIntegration.Sync();
                                        }
                                        //// End by uday for #532
                                    }
                                    else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, 0, planTacticId, tactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);
                                    }
                                    //// Get URL for Tactic to send to Email.
                                    string strUrl = GetNotificationURLbyStatus(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, planTacticId, Convert.ToString(Enums.Section.Tactic).ToLower());
                                    Common.mailSendForTactic(planTacticId, status, tactic.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower(), URL: strUrl); // Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.

                                    ////Start Added by Mitesh Vaishnav for PL ticket #766 Different Behavior for Approve Tactics via Request tab
                                    ////Update Program status according to the tactic status
                                    Common.ChangeProgramStatus(tactic.PlanProgramId, false);

                                    ////Update Campaign status according to the tactic and program status
                                    var PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId;
                                    Common.ChangeCampaignStatus(PlanCampaignId, false);
                                    ////End Added by Mitesh Vaishnav for PL ticket #766 Different Behavior for Approve Tactics via Request tab

                                    if (result >= 1)
                                    {
                                        // Add By Nishant Sheth
                                        // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                        DataSet dsPlanCampProgTac = new DataSet();
                                        dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                        objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                        List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                        objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                        var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                        objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                        var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                        objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                        var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                        objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                        var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                        objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

                                        scope.Complete();
                                        return Json(new { result = true }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            /// Modified By Maninder Singh Wadhva PL Ticket#47
                            return Json(new { result = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            return null;
        }

        public JsonResult SaveReviewIntegrationInfo(string title = "", string Id = "", string isDeployToIntegration = "", string isSyncSF = "", string isSyncEloqua = "", string isSyncWorkFront = "", string isSyncMarketo = "", string approvalBehaviorWorkFront = "", string requestQueueWF = "", string assigneeWF = "")
        {
            bool IsSyncSF = false, IsSyncEloqua = false, IsSyncWorkFront = false, IsDeployToIntegration = false, IsDuplicate = false, IsSyncMarketo = false; // Declare local variables.
            try
            {
                // Save Tactic Title.
                JsonResult objJasonResult = SaveTacticTitle(Id, title, ref IsDuplicate);
                if (objJasonResult != null && IsDuplicate)
                    return objJasonResult;

                int planTacticId = !string.IsNullOrEmpty(Id) ? Convert.ToInt32(Id) : 0;
                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId.Equals(planTacticId) && tac.IsDeleted == false).FirstOrDefault();
                if (objTactic != null && Sessions.User != null) //Added by komal to check session is null for #2299
                {
                    IsDeployToIntegration = !string.IsNullOrEmpty(isDeployToIntegration) ? bool.Parse(isDeployToIntegration) : false; // Parse isDeployToIntegration value.
                    IsSyncSF = !string.IsNullOrEmpty(isSyncSF) ? bool.Parse(isSyncSF) : false;                                        // Parse isSyncSF value
                    IsSyncEloqua = !string.IsNullOrEmpty(isSyncEloqua) ? bool.Parse(isSyncEloqua) : false;                            // Parse isSyncEloqua value
                    IsSyncWorkFront = !string.IsNullOrEmpty(isSyncWorkFront) ? bool.Parse(isSyncWorkFront) : false;                   // Parse isSyncWorkFront value
                    IsSyncMarketo = !string.IsNullOrEmpty(isSyncMarketo) ? bool.Parse(isSyncMarketo) : false;                         // Parse isSyncMarketo value
                    objTactic.IsDeployedToIntegration = IsDeployToIntegration;
                    objTactic.IsSyncEloqua = IsSyncEloqua;
                    objTactic.IsSyncSalesForce = IsSyncSF;
                    objTactic.IsSyncWorkFront = IsSyncWorkFront;
                    objTactic.IsSyncMarketo = IsSyncMarketo;
                    objTactic.ModifiedBy = Sessions.User.UserId;
                    objTactic.ModifiedDate = DateTime.Now;
                    db.Entry(objTactic).State = EntityState.Modified;

                    if (IsSyncWorkFront)
                    {
                        SaveWorkFrontTacticReviewSettings(objTactic, approvalBehaviorWorkFront, requestQueueWF, assigneeWF);  //If integrated to WF, update the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray
                    }




                    #region "Update linked Tactic Integration Settings"
                    if (objTactic.LinkedTacticId != null && objTactic.LinkedTacticId.HasValue && objTactic.LinkedTacticId.Value > 0)
                    {
                        int LinkedTacticId = objTactic.LinkedTacticId.Value;
                        Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                        objLinkedTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId.Equals(LinkedTacticId) && tac.IsDeleted == false).FirstOrDefault();
                        objLinkedTactic.IsDeployedToIntegration = IsDeployToIntegration;
                        objLinkedTactic.IsSyncEloqua = IsSyncEloqua;
                        objLinkedTactic.IsSyncSalesForce = IsSyncSF;
                        objLinkedTactic.IsSyncWorkFront = IsSyncWorkFront;
                        objLinkedTactic.IsSyncMarketo = IsSyncMarketo;
                        objLinkedTactic.ModifiedBy = Sessions.User.UserId;
                        objLinkedTactic.ModifiedDate = DateTime.Now;
                        db.Entry(objLinkedTactic).State = EntityState.Modified;

                        //If integrated to WF, updated the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray
                        if (IsSyncWorkFront)
                        {
                            SaveWorkFrontTacticReviewSettings(objLinkedTactic, approvalBehaviorWorkFront, requestQueueWF, assigneeWF);
                        }


                    }
                    #endregion

                    db.SaveChanges();

                    return objJasonResult;  // if successfully tactic updated then send TacticUpdte message redirected from "SaveTacticTitle" function.
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { id = 0 });
        }


        /// <summary>
        /// If integrated to WF, update the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray PL#1922
        /// Tactics integrated with WorkFront need to be tied to either a project or a request
        /// If tied to a request, the request needs to be linked to a request queue - comes across as an int: the id from the IntegrationWorkFrontRequestQueues table
        /// If tied to a request, the request needs to be linked to a workfront user - comes in as an int: the id from the IntegrationWorkFrontUsers table
        /// </summary>
        /// <param name="objTactic"></param>
        /// <param name="approvalBehaviorWorkFront"></param>
        /// <param name="requestQueueWF"></param>
        /// <param name="assigneeWF"></param>
        public void SaveWorkFrontTacticReviewSettings(Plan_Campaign_Program_Tactic objTactic, string approvalBehaviorWorkFront = "", string requestQueueWF = "", string assigneeWF = "")
        {
            try
            {
                IntegrationWorkFrontTacticSetting wfSetting = db.IntegrationWorkFrontTacticSettings.Where(set => set.TacticId == objTactic.PlanTacticId && set.IsDeleted == false).FirstOrDefault();

                //verify we have the information we need
                if (approvalBehaviorWorkFront != Integration.Helper.Enums.WorkFrontTacticApprovalObject.Project.ToString()
                    && approvalBehaviorWorkFront != Integration.Helper.Enums.WorkFrontTacticApprovalObject.Request.ToString()
                    && approvalBehaviorWorkFront != Integration.Helper.Enums.WorkFrontTacticApprovalObject.Project2.ToString())
                {
                    throw new Exception("Attempted to save WorkFront information without valid Tactic Approval Options");
                }
                if (approvalBehaviorWorkFront == Integration.Helper.Enums.WorkFrontTacticApprovalObject.Request.ToString())
                {
                    if (requestQueueWF == string.Empty || requestQueueWF == "undefined")
                    {
                        throw new Exception("Attempted to save WorkFront Request information without valid Request Queue");
                    }
                    if (assigneeWF == string.Empty || assigneeWF == "undefined")
                    {
                        throw new Exception("Attempted to save WorkFront Request information without valid Assignee");
                    }
                }

                IntegrationWorkFrontTacticSetting settings;
                if (wfSetting == null) //no tactic settings for WorkFront in the database. Create an entry
                {
                    settings = new IntegrationWorkFrontTacticSetting();
                    db.Entry(settings).State = EntityState.Added;
                }
                else
                {
                    settings = db.IntegrationWorkFrontTacticSettings.Where(s => s.TacticId == objTactic.PlanTacticId && s.IsDeleted == false).FirstOrDefault();
                    db.Entry(settings).State = EntityState.Modified;
                }
                settings.TacticId = objTactic.PlanTacticId;
                settings.IsDeleted = false;
                settings.TacticApprovalObject = approvalBehaviorWorkFront;
                if (settings.TacticApprovalObject == Integration.Helper.Enums.WorkFrontTacticApprovalObject.Request.ToString()) //need to create  or update the request
                {
                    int intInstanceId = (int)objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt;
                    IntegrationWorkFrontRequest req = db.IntegrationWorkFrontRequests.Where(r => r.PlanTacticId == objTactic.PlanTacticId && r.IsDeleted == false && r.IntegrationInstanceId == intInstanceId).FirstOrDefault();
                    if (req == null) //create a new request
                    {
                        req = new IntegrationWorkFrontRequest();
                        req.PlanTacticId = objTactic.PlanTacticId;
                        req.RequestName = objTactic.Title;
                        req.IntegrationInstanceId = intInstanceId;
                        req.IsDeleted = false;
                        req.QueueId = Int32.Parse(requestQueueWF);
                        req.WorkFrontUserId = Int32.Parse(assigneeWF);
                        db.Entry(req).State = EntityState.Added;
                    }
                    else //update current request entry
                    {
                        req.QueueId = Int32.Parse(requestQueueWF);
                        req.WorkFrontUserId = Int32.Parse(assigneeWF);
                        db.Entry(req).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }


        #endregion

        #region "Improvement Tactic related Functions"

        /// <summary>
        /// Added By: Pratik Chauhan.
        /// Action to Save Improvement Tactic.
        /// </summary>
        /// <param name="form">Form object of PlanImprovementTactic.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveImprovementTactic(InspectModel form, bool RedirectType)
        {
            try
            {
                //// If ImprovementPlanTacticId is null then insert new record to Table.
                if (form.ImprovementPlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            int planId = (from p in db.Plan_Improvement_Campaign_Program
                                          join c in db.Plan_Improvement_Campaign on p.ImprovementPlanCampaignId equals c.ImprovementPlanCampaignId
                                          where p.ImprovementPlanProgramId == form.ImprovementPlanProgramId
                                          select c.ImprovePlanId).FirstOrDefault();

                            //// Check for duplicate exist or not.
                            Plan_Improvement_Campaign_Program_Tactic pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                                                                               where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                                                               select pcpt).FirstOrDefault();

                            //// if duplicate record exist then return duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { isSaved = false, redirect = Url.Action("Assortment"), errormsg = strDuplicateMessage });  // Modified by Viral Kadiya on 11/18/2014 to resolve Internal Review Points.
                            }
                            else
                            {
                                Plan_Improvement_Campaign_Program_Tactic picpt = new Plan_Improvement_Campaign_Program_Tactic();
                                picpt.ImprovementPlanProgramId = form.ImprovementPlanProgramId;
                                picpt.Title = form.Title;
                                picpt.ImprovementTacticTypeId = form.ImprovementTacticTypeId;
                                picpt.Description = form.Description;
                                picpt.Cost = form.Cost ?? 0;
                                picpt.EffectiveDate = form.EffectiveDate;
                                picpt.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                picpt.CreatedBy = Sessions.User.UserId;
                                picpt.CreatedDate = DateTime.Now;
                                picpt.IsDeployedToIntegration = form.IsDeployedToIntegration;

                                db.Entry(picpt).State = EntityState.Added;
                                int result = db.SaveChanges();

                                // Set isDeployedToIntegration in improvement program and improvement campaign
                                Plan_Improvement_Campaign_Program objIProgram = db.Plan_Improvement_Campaign_Program.FirstOrDefault(_program => _program.ImprovementPlanProgramId == picpt.ImprovementPlanProgramId);
                                Plan_Improvement_Campaign objICampaign = db.Plan_Improvement_Campaign.FirstOrDefault(_campaign => _campaign.ImprovementPlanCampaignId == objIProgram.ImprovementPlanCampaignId);
                                if (form.IsDeployedToIntegration)
                                {
                                    objIProgram.IsDeployedToIntegration = true;
                                    db.Entry(objIProgram).State = EntityState.Modified;
                                    db.SaveChanges();

                                    objICampaign.IsDeployedToIntegration = true;
                                    db.Entry(objICampaign).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    bool flag = false;
                                    flag = objIProgram.Plan_Improvement_Campaign_Program_Tactic.Any(_tactic => _tactic.IsDeployedToIntegration == true && _tactic.IsDeleted == false);
                                    if (!flag)
                                    {
                                        objIProgram.IsDeployedToIntegration = false;
                                        db.Entry(objIProgram).State = EntityState.Modified;
                                        db.SaveChanges();

                                        objICampaign.IsDeployedToIntegration = false;
                                        db.Entry(objICampaign).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                                //// Insert change log entry.
                                result = Common.InsertChangeLog(planId, null, picpt.ImprovementPlanTacticId, picpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (result >= 1)
                                {
                                    scope.Complete();
                                    string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { isSaved = true, redirect = Url.Action("Assortment"), msg = strMessage, id = picpt.ImprovementPlanTacticId });
                                }
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
                        var TransactionOption = new System.Transactions.TransactionOptions();
                        TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

                        using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
                        {

                            int planId = (from p in db.Plan_Improvement_Campaign_Program
                                          join c in db.Plan_Improvement_Campaign on p.ImprovementPlanCampaignId equals c.ImprovementPlanCampaignId
                                          where p.ImprovementPlanProgramId == form.ImprovementPlanProgramId
                                          select c.ImprovePlanId).FirstOrDefault();

                            //// Check for Duplicate or not.
                            Plan_Improvement_Campaign_Program_Tactic pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                                                                               where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.ImprovementPlanTacticId.Equals(form.ImprovementPlanTacticId) && pcpt.IsDeleted.Equals(false)
                                                                               select pcpt).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                bool isReSubmission = false;
                                bool isManagerLevelUser = false;
                                string status = string.Empty;

                                Plan_Improvement_Campaign_Program_Tactic pcpobj = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.ImprovementPlanTacticId.Equals(form.ImprovementPlanTacticId)).FirstOrDefault();

                                //If improvement tacitc modified by immediate manager then no resubmission will take place, By dharmraj, Ticket #537
                                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                                var lstUserHierarchy = objBDSServiceClient.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                                var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId).Select(u => u.UserId).ToList();
                                if (lstSubordinates.Contains(pcpobj.CreatedBy))
                                {
                                    isManagerLevelUser = true;
                                }

                                pcpobj.Title = form.Title;
                                status = pcpobj.Status;

                                if (pcpobj.ImprovementTacticTypeId != form.ImprovementTacticTypeId)
                                {
                                    pcpobj.ImprovementTacticTypeId = form.ImprovementTacticTypeId;
                                    if (!isManagerLevelUser) isReSubmission = true;
                                }
                                pcpobj.Description = form.Description;

                                if (pcpobj.EffectiveDate != form.EffectiveDate)
                                {
                                    pcpobj.EffectiveDate = form.EffectiveDate;
                                    if (!isManagerLevelUser) isReSubmission = true;
                                }

                                if (pcpobj.Cost != form.Cost)
                                {
                                    pcpobj.Cost = form.Cost ?? 0;
                                    if (!isManagerLevelUser) isReSubmission = true;
                                }

                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                db.Entry(pcpobj).State = EntityState.Modified;
                                int result;
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    result = Common.InsertChangeLog(planId, null, pcpobj.ImprovementPlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                }
                                if (isReSubmission && Common.CheckAfterApprovedStatus(status))
                                {
                                    pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                    Common.mailSendForTactic(pcpobj.ImprovementPlanTacticId, pcpobj.Status, pcpobj.Title, section: Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                                }
                                result = db.SaveChanges();


                                // Set isDeployedToIntegration in improvement program and improvement campaign
                                Plan_Improvement_Campaign_Program objIProgram = db.Plan_Improvement_Campaign_Program.FirstOrDefault(_program => _program.ImprovementPlanProgramId == pcpobj.ImprovementPlanProgramId);
                                Plan_Improvement_Campaign objICampaign = db.Plan_Improvement_Campaign.FirstOrDefault(_campaign => _campaign.ImprovementPlanCampaignId == objIProgram.ImprovementPlanCampaignId);
                                if (form.IsDeployedToIntegration)
                                {
                                    objIProgram.IsDeployedToIntegration = true;
                                    db.Entry(objIProgram).State = EntityState.Modified;
                                    db.SaveChanges();

                                    objICampaign.IsDeployedToIntegration = true;
                                    db.Entry(objICampaign).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    bool flag = false;
                                    flag = objIProgram.Plan_Improvement_Campaign_Program_Tactic.Any(_tactic => _tactic.IsDeployedToIntegration == true && _tactic.IsDeleted == false);
                                    if (!flag)
                                    {
                                        objIProgram.IsDeployedToIntegration = false;
                                        db.Entry(objIProgram).State = EntityState.Modified;
                                        db.SaveChanges();

                                        objICampaign.IsDeployedToIntegration = false;
                                        db.Entry(objICampaign).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                                if (result >= 1)
                                {
                                    scope.Complete();
                                    string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { isSaved = true, msg = strMessage, tacticStatus = pcpobj.Status });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Setup Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadImprovementSetup(int id, string InspectPopupMode = "")
        {
            ViewBag.InspectMode = InspectPopupMode;

            if (InspectPopupMode == Enums.InspectPopupMode.Add.ToString())
            {
                var planId = (from pc in db.Plan_Improvement_Campaign where pc.ImprovementPlanCampaignId == ((from pcp in db.Plan_Improvement_Campaign_Program where pcp.ImprovementPlanProgramId == id select pcp.ImprovementPlanCampaignId).FirstOrDefault()) select pc.ImprovePlanId).FirstOrDefault();
                //// Get Improvement Tactic list for Current Plan.
                List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(_tactic => _tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && _tactic.IsDeleted == false).Select(_tactic => _tactic.ImprovementTacticTypeId).ToList();// Sessions.PlanId

                //// load Improvement tactic list to ViewBag excluding current plan Improvement Tactics.
                ViewBag.Tactics = from _imprvTactic in db.ImprovementTacticTypes
                                  where _imprvTactic.ClientId == Sessions.User.ClientId && _imprvTactic.IsDeployed == true && !impTacticList.Contains(_imprvTactic.ImprovementTacticTypeId)
                                  && _imprvTactic.IsDeleted == false
                                  orderby _imprvTactic.Title, new AlphaNumericComparer()
                                  select _imprvTactic;
                ViewBag.IsCreated = true;

                //// Get Plan by PlanId.
                var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == planId);// Sessions.PlanId
                ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

                InspectModel pitm = new InspectModel();
                pitm.ImprovementPlanProgramId = id;
                pitm.CampaignTitle = (from pc in db.Plan_Improvement_Campaign where pc.ImprovePlanId == planId select pc.Title).FirstOrDefault().ToString();

                // Set today date as default for effective date.
                pitm.EffectiveDate = DateTime.Now;
                pitm.IsDeployedToIntegration = false;

                ViewBag.IsOwner = true;
                ViewBag.RedirectType = false;
                ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(planId)).Year;// Sessions.PlanId

                pitm.Owner = Sessions.User.FirstName + " " + Sessions.User.LastName;
                ViewBag.TacticDetail = pitm;
                return PartialView("_SetupImprovementTactic", pitm);
            }
            else
            {
                InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                List<Guid> userListId = new List<Guid>();
                userListId.Add(im.OwnerId);
                User userName = new User();
                try
                {
                    userName = objBDSUserRepository.GetTeamMemberDetails(im.OwnerId, Sessions.ApplicationId);
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);

                    //To handle unavailability of BDSService
                    if (e is System.ServiceModel.EndpointNotFoundException)
                    {
                        //// Flag to indicate unavailability of web service.
                        //// Added By: Maninder Singh Wadhva on 11/24/2014.
                        //// Ticket: 942 Exception handeling in Gameplan.
                        return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                    }
                }
                im.Owner = (userName.FirstName + " " + userName.LastName).ToString();
                im.EffectiveDate = im.EffectiveDate;
                ViewBag.TacticDetail = im;

                var objPlan = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.ImprovementPlanTacticId.Equals(id) && _imprvTactic.IsDeleted.Equals(false)).Select(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan).FirstOrDefault();
                int planId = objPlan.PlanId;

                ViewBag.ApprovedStatus = true;
                ViewBag.NoOfTacticBoosts = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.IsDeleted == false && _tactic.StartDate >= im.StartDate && _tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList().Count();


                ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690
                if (im.LastSyncDate != null)
                {
                    TimeZone localZone = TimeZone.CurrentTimeZone;

                    ViewBag.LastSync = Common.objCached.LastSynced.Replace("{0}", Common.GetFormatedDate(im.LastSyncDate));////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
                }
                else
                {
                    ViewBag.LastSync = string.Empty;
                }
                if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
                {
                    ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

                    List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && _imprvTactic.IsDeleted == false && _imprvTactic.ImprovementPlanTacticId != id).Select(_imprvTactic => _imprvTactic.ImprovementTacticTypeId).ToList();

                    //// Get Improvement Tactic Type list.
                    var tactics = from _imprvTacticType in db.ImprovementTacticTypes
                                  where _imprvTacticType.ClientId == Sessions.User.ClientId && _imprvTacticType.IsDeployed == true && !impTacticList.Contains(_imprvTacticType.ImprovementTacticTypeId)
                                  && _imprvTacticType.IsDeleted == false
                                  select _imprvTacticType;

                    foreach (var item in tactics)
                    {
                        item.Title = HttpUtility.HtmlDecode(item.Title);
                    }

                    //// Add other Improvement Tactic Type to tactics list.
                    if (!tactics.Any(a => a.ImprovementTacticTypeId == im.ImprovementTacticTypeId))
                    {
                        var tacticTypeSpecial = from t in db.ImprovementTacticTypes
                                                where t.ClientId == Sessions.User.ClientId && t.ImprovementTacticTypeId == im.ImprovementTacticTypeId
                                                orderby t.Title
                                                select t;

                        tactics = tactics.Concat<ImprovementTacticType>(tacticTypeSpecial);
                        tactics.OrderBy(a => a.Title);
                    }

                    ViewBag.Tactics = tactics;
                    ViewBag.InspectMode = InspectPopupMode;

                    if (Sessions.User.UserId == im.OwnerId)
                    {
                        ViewBag.IsOwner = true;
                    }
                    else
                    {
                        ViewBag.IsOwner = false;
                    }
                    ViewBag.Year = objPlan.Year;
                }

                if (ViewBag.Year == null)
                {
                    ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(planId) && p.IsDeleted == false).Year;
                }

                return PartialView("_SetupImprovementTactic", im);
            }
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Review Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadImprovementReview(int id, string InspectPopupMode = "")
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());

            //// Get Userlist using Improvement Tactic Comment list.
            var tacticComment = (from _imprvTacCmnt in db.Plan_Improvement_Campaign_Program_Tactic_Comment
                                 where _imprvTacCmnt.ImprovementPlanTacticId == id
                                 select _imprvTacCmnt).ToArray();
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in tacticComment select ta.CreatedBy).ToList<Guid>();
            userListId.Add(im.OwnerId);
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
            ViewBag.ReviewModel = (from tc in tacticComment
                                   select new InspectReviewModel
                                   {
                                       PlanTacticId = Convert.ToInt32(tc.ImprovementPlanTacticId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Any() ? userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault() : Common.GameplanIntegrationService,
                                       CreatedBy = tc.CreatedBy
                                   }).OrderByDescending(x => x.CommentDate).ToList(); //Modified By komal Rawal for 2043 resort comment in desc order

            var ownername = (from u in userName
                             where u.UserId == im.OwnerId
                             select u.FirstName + " " + u.LastName).FirstOrDefault();
            if (ownername != null)
            {
                im.Owner = ownername.ToString();
            }
            ViewBag.TacticDetail = im;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            bool isValidOwner = false;
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            var lstSubOrdinatesPeers = new List<Guid>();
            //To get permission status for Approve campaign , By dharmraj PL #538
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(im.OwnerId))
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;


            //Modified By komal Rawal for #1158
            bool IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);
            ViewBag.IsCommentsViewEditAuthorized = IsCommentsViewEditAuthorized;
            // End 

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            //Get all subordinates of current user upto n level
            var lstSubOrdinates = new List<Guid>();
            try
            {
                lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool IsDeployToIntegrationVisible = false;
            if (im.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsDeployToIntegrationVisible = true;
                }
            }
            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;

            #region "Set ImprovementTactic Last sync TimeStamap"
            if (im.IsDeployedToIntegration && im.IsIntegrationInstanceExist == "Yes")
            {
                string ImprovementTacticEntityType = Enums.Section.ImprovementTactic.ToString();
                var planEntityLogList = db.IntegrationInstancePlanEntityLogs.Where(ipt => ipt.EntityId == im.ImprovementPlanTacticId && ipt.EntityType == ImprovementTacticEntityType).ToList();
                if (planEntityLogList.Any())
                {
                    ViewBag.ImprovementTacticLastSync = planEntityLogList.OrderByDescending(log => log.IntegrationInstancePlanLogEntityId).FirstOrDefault().SyncTimeStamp;
                }
            }
            #endregion

            return PartialView("_ReviewImprovementTactic");
        }

        /// <summary>
        /// Action to Load Improvement Impact View.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadImprovementImpact(int id, string InspectPopupMode = "")
        {
            return PartialView("_ImpactImprovementTactic");
        }

        /// <summary>
        /// Calculate Improvenet For Tactic Type & Date.
        /// Added by Bhavesh Dobariya.
        /// </summary>
        /// <param name="ImprovementPlanTacticId"></param>
        /// <returns>JsonResult.</returns>
        public JsonResult LoadImpactImprovementStages(int ImprovementPlanTacticId)
        {
            int ImprovementTacticTypeId = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId).Select(t => t.ImprovementTacticTypeId).FirstOrDefault();
            DateTime EffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId).Select(t => t.EffectiveDate).FirstOrDefault();
            PlanController pc = new PlanController();
            List<ImprovementStage> ImprovementMetric = pc.GetImprovementStages(ImprovementPlanTacticId, ImprovementTacticTypeId, EffectiveDate);

            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            string Size = Enums.StageType.Size.ToString();
            var tacticobj = ImprovementMetric.Select(p => new
            {
                MetricId = p.StageId,
                MetricCode = p.StageCode,
                MetricName = p.StageName,
                MetricType = p.StageType,
                BaseLineRate = p.BaseLineRate,
                PlanWithoutTactic = p.PlanWithoutTactic,
                PlanWithTactic = p.PlanWithTactic,
                Rank = p.StageType == CR ? 1 : (p.StageType == SV ? 2 : 3),
            }).Select(p => p).Distinct().OrderBy(p => p.Rank).ToList();

            return Json(new { data = tacticobj }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Improvement Tactic Type related value using Improvement Tactic Type id.
        /// Added By : Sohel Pathan
        /// Added Date : 06/11/2014
        /// </summary>
        /// <param name="ImprovementTacticTypeId">ImprovementTacticTypeId</param>
        /// <returns>JsonResult.</returns>
        public JsonResult LoadImprovementTacticTypeData(int ImprovementTacticTypeId)
        {
            try
            {
                var objImprovementTacticType = db.ImprovementTacticTypes.Where(itt => itt.ImprovementTacticTypeId == ImprovementTacticTypeId && itt.IsDeleted.Equals(false)).FirstOrDefault();
                double Cost = objImprovementTacticType == null ? 0 : objImprovementTacticType.Cost;
                bool isDeployedToIntegration = objImprovementTacticType == null ? false : objImprovementTacticType.IsDeployedToIntegration;

                return Json(new { isSuccess = true, cost = Cost, isDeployedToIntegration = isDeployedToIntegration }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region "LineItem related Functions"
        /// <summary>
        /// Load line item grid in tactic inspect popup in all screens
        /// Added by Arpita Soni for Ticket #2237 on 06/09/2016
        /// </summary>
        /// <param name="tacticId"></param>
        /// <param name="ownerIds"></param>
        /// <param name="TacticTypeid"></param>
        /// <param name="StatusIds"></param>
        /// <param name="customFieldIds"></param>
        /// <param name="budgetTab"></param>
        /// <returns>Returns partial view</returns>
        public PartialViewResult LoadLineItemTabFromTacticPopup(int tacticId, bool isLocked = true, bool IsPlanCreateAll = false)
        {
            PlanController objPlanController = new PlanController();
            Plangrid objplangrid = new Plangrid();
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();

            try
            {
                // Generate Json Header
                objPlanMainDHTMLXGrid.head = objPlanController.GenerateJsonHeader("", 0, null, "", false);

                Plan_Campaign_Program_Tactic objTactic = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId.Equals(tacticId) && _tactic.IsDeleted.Equals(false)).FirstOrDefault();
                // Add Condition by nishant sheth
                // Desc :: To resolve test case when tactic object is null
                if (objTactic != null)
                {
                    string type = string.Empty;
                    List<Plan_Campaign_Program_Tactic_LineItem> finalLineitem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                    string cellTextColor = string.Empty;
                    // Declare Variable for XML to JSON
                    List<PlanDHTMLXGridDataModel> lineitemrowsobjlist = new List<PlanDHTMLXGridDataModel>();
                    PlanDHTMLXGridDataModel lineitemrowsobj = new PlanDHTMLXGridDataModel();

                    string lockedstateone = "1";
                    string bgcolorLineItem = "#ffffff";

                    string stylecolorblack = "color:#000";
                    string stylecolorgray = "color:#999"; // Add By Nishant Sheth #1987

                    finalLineitem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanTacticId == tacticId && lineitem.IsDeleted == false).OrderBy(l => l.Title).ToList(); // Ticket #1753 : Add default sorting for task name : Added By Bhavesh : Date - 17-Nov-2015 : Addd orderby clause for line item title

                    if (objTactic != null)
                    {

                        cellTextColor = isLocked ? stylecolorgray : stylecolorblack;// Modified By Nishant Sheth #1987

                        if (finalLineitem != null && finalLineitem.Count > 0)
                        {
                            var lstLineItemTaskData = finalLineitem.Select((taskdata, index) => new
                            {
                                index = index,
                                Cost = taskdata.Cost,
                                lineitemtype = taskdata.LineItemTypeId,
                                PlanLineItemId = taskdata.PlanLineItemId,
                                title = taskdata.Title,
                                Typeid = taskdata.LineItemTypeId,
                                Type = taskdata.LineItemTypeId != null ? taskdata.LineItemType.Title : "",
                                CreatedBy = taskdata.CreatedBy,
                                IstactEditable = (taskdata.CreatedBy.Equals(Sessions.User.UserId) || !isLocked) == true ? "0" : "1"//Tactic created by condition add for ticket #1968 , Date : 05-02-2016, Bhavesh
                            });

                            #region LineItems
                            foreach (var lineitem in lstLineItemTaskData)
                            {
                                cellTextColor = lineitem.IstactEditable == lockedstateone ? stylecolorgray : stylecolorblack;// Modified By Nishant Sheth #1987

                                lineitemrowsobj = new PlanDHTMLXGridDataModel();
                                lineitemrowsobj.id = "line." + lineitem.index;
                                lineitemrowsobj.bgColor = bgcolorLineItem;
                                List<Plandataobj> lineitemdataobjlist = new List<Plandataobj>();
                                Plandataobj lineitemdataobj = new Plandataobj();

                                lineitemdataobj.value = "LineItem";
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemdataobj = new Plandataobj();
                                lineitemdataobj.value = HttpUtility.HtmlEncode(lineitem.title);
                                lineitemdataobj.locked = lineitem.IstactEditable;
                                lineitemdataobj.style = cellTextColor;
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemdataobj = new Plandataobj();
                                lineitemdataobj.value = "<div class=grid_Search id=LP></div>" + (IsPlanCreateAll ? "<div class=grid_add id=Line1 onclick=javascript:OpenLineItemGridPopup(this,event) alt=" + tacticId + "_" + lineitem.PlanLineItemId + " lt=" + ((lineitem.lineitemtype == null) ? 0 : lineitem.lineitemtype) + " dt=" + HttpUtility.HtmlEncode(lineitem.title) + " per=" + IsPlanCreateAll.ToString().ToLower() + "></div>" : "");
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemdataobj = new Plandataobj();
                                lineitemdataobj.value = lineitem.PlanLineItemId.ToString();
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemdataobj = new Plandataobj();
                                lineitemdataobj.value = lineitem.Cost.ToString();
                                lineitemdataobj.locked = ((lineitem.Type == null || lineitem.Type == "") ? lockedstateone : lineitem.IstactEditable);
                                lineitemdataobj.type = "edn";
                                lineitemdataobj.style = cellTextColor;
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemdataobj = new Plandataobj();
                                lineitemdataobj.value = HttpUtility.HtmlEncode(lineitem.Type);
                                lineitemdataobj.style = cellTextColor;
                                lineitemdataobj.locked = ((lineitem.Type == null || lineitem.Type == "") ? lockedstateone : lineitem.IstactEditable);
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemdataobj = new Plandataobj();
                                lineitemdataobj.value = objPlanController.GetUserName(lineitem.CreatedBy);
                                lineitemdataobj.locked = lineitem.IstactEditable;
                                lineitemdataobj.style = cellTextColor;
                                lineitemdataobjlist.Add(lineitemdataobj);

                                lineitemrowsobj.data = lineitemdataobjlist;

                                Planuserdatagrid lineitemuserdata = new Planuserdatagrid();
                                lineitemuserdata.IsOther = ((lineitem.Type == null || lineitem.Type == "") ? true : false).ToString();
                                lineitemrowsobj.userdata = lineitemuserdata;

                                lineitemrowsobjlist.Add(lineitemrowsobj);
                            }
                            #endregion
                        }
                    }
                    objPlanMainDHTMLXGrid.rows = lineitemrowsobjlist;
                    objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
                    // Modified by Arpita Soni for Ticket #2237 on 06/22/2016
                    var lstLineItemType = db.LineItemTypes.
                                            Where(litemtype => litemtype.ModelId == objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId
                                            && litemtype.IsDeleted == false).
                                            Select(lineitemtype => new { lineitemtype.LineItemTypeId, lineitemtype.Title }).ToList();
                    TempData["lineItemTypes"] = lstLineItemType;
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return PartialView("_TacticLineItemListing", objplangrid);
        }

        /// <summary>
        /// Save Line item Actual 
        /// Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
        /// Save the data into the Plan_Campaign_Program_Tactic_LineItem_Actual
        /// </summary>
        /// <param name="objInspectActual"></param>
        public void SaveActualLineItem(InspectActual objInspectActual)
        {
            Plan_Campaign_Program_Tactic_LineItem_Actual objPlan_LineItem_Actual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
            objPlan_LineItem_Actual.PlanLineItemId = Convert.ToInt32(objInspectActual.PlanLineItemId);
            objPlan_LineItem_Actual.Period = objInspectActual.Period;
            objPlan_LineItem_Actual.CreatedDate = DateTime.Now;
            objPlan_LineItem_Actual.CreatedBy = Sessions.User.UserId;
            objPlan_LineItem_Actual.Value = objInspectActual.ActualValue;
            db.Entry(objPlan_LineItem_Actual).State = EntityState.Added;
            db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objPlan_LineItem_Actual);
        }

        /// <summary>
        /// Added By : Kalpesh Sharma : Functional Review Points #697
        /// This method is responsible to add the Line items in Tactic.
        /// </summary>
        /// <param name="form">Pass the Tactic form model for fetching Start date and end date values</param>
        /// <param name="lineitems">pass the lineitems</param>
        /// <param name="tacticId">Tactic Id that used to insert into Lineitem table </param>
        public int SaveLineItems(Inspect_Popup_Plan_Campaign_Program_TacticModel form, string lineitems, int tacticId)
        {
            int Result = 0;

            //Fetch the lineitems and store into array of string
            string[] lineitem = lineitems.Split(',');

            // Fetch the current plan object and based on it's ModelID we have fetch the lineitems.
            Plan objPlan = db.Plans.Where(p => p.PlanId == (db.Plan_Campaign.Where(pc => pc.PlanCampaignId == form.PlanCampaignId && pc.PlanCampaignId.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault())).FirstOrDefault();
            var lineItemType = db.LineItemTypes.Where(l => l.ModelId == objPlan.ModelId).ToList();

            //Iterating the collection of lineitems that we hasd perviously fetch.
            foreach (string li in lineitem)
            {
                //insert new lineItem into the LineItem Table. 
                Plan_Campaign_Program_Tactic_LineItem pcptlobj = new Plan_Campaign_Program_Tactic_LineItem();
                pcptlobj.PlanTacticId = tacticId;
                int lineItemTypeid = Convert.ToInt32(li);
                pcptlobj.LineItemTypeId = lineItemTypeid;

                //Fetch the LineItemType by it's ID
                LineItemType lit = lineItemType.Where(m => m.LineItemTypeId == lineItemTypeid).FirstOrDefault();
                pcptlobj.Title = (lit.Title == Enums.LineItemTypes.None.ToString()) ? DefaultLineItemTitle : lit.Title;
                pcptlobj.Cost = 0;
                pcptlobj.StartDate = form.StartDate;
                pcptlobj.EndDate = form.EndDate;
                pcptlobj.CreatedBy = Sessions.User.UserId;
                pcptlobj.CreatedDate = DateTime.Now;
                db.Entry(pcptlobj).State = EntityState.Added;

                //Save the database changes and get new inserted PlanLineItemId   
                Result = db.SaveChanges();
                int liid = pcptlobj.PlanLineItemId;

                //insert chnage log into the database 
                Result = Common.InsertChangeLog(objPlan.PlanId, null, liid, pcptlobj.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
            }

            //retturn the reuslt status
            return Result;
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Load Lineitem Setup Tab.
        /// </summary>
        /// <param name="id">Plan Lineitem Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupLineitem(int id)
        {

            ViewBag.IsCreated = false;
            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
            if (pcptl == null)
            {
                return null;
            }

            var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId);

            //// Get list of LineItem Types based on ModelId for current PlanId.
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select lit;

            string lineItemTypeName = "";

            foreach (var item in lineItemTypes)
            {
                if (pcptl.LineItemTypeId.ToString() == item.LineItemTypeId.ToString())
                {
                    lineItemTypeName = HttpUtility.HtmlDecode(item.Title);
                }
            }

            //// Set respected values to ViewBag.
            ViewBag.lineItemTypes = lineItemTypeName;
            ViewBag.TacticTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Title);
            ViewBag.ProgramTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title);
            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title);
            ViewBag.PlanTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title);


            //// Set Values to Plan_Campaign_Program_Tactic_LineItemModel to pass into PartialView.
            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            pcptlm.PlanLineItemId = pcptl.PlanLineItemId;
            pcptlm.PlanTacticId = pcptl.PlanTacticId;
            pcptlm.LineItemTypeId = pcptl.LineItemTypeId == null ? 0 : Convert.ToInt32(pcptl.LineItemTypeId);
            pcptlm.Title = HttpUtility.HtmlDecode(pcptl.Title);
            pcptlm.Description = HttpUtility.HtmlDecode(pcptl.Description);
            pcptlm.StartDate = Convert.ToDateTime(pcptl.StartDate);
            pcptlm.EndDate = Convert.ToDateTime(pcptl.EndDate);
            pcptlm.Cost = pcptl.Cost;
            pcptlm.TStartDate = pcptl.Plan_Campaign_Program_Tactic.StartDate;
            pcptlm.TEndDate = pcptl.Plan_Campaign_Program_Tactic.EndDate;
            pcptlm.PStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.StartDate;
            pcptlm.PEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.EndDate;
            pcptlm.CStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.StartDate;
            pcptlm.CEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.EndDate;
            pcptlm.IsOtherLineItem = pcptl.LineItemTypeId != null ? false : true;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(objPlan.PlanId)).Year; //Sessions.PlanId
            // Added by Arpita Soni for Ticket #2212 on 05/24/2016 
            pcptlm.PlanId = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;

            User userName = new User();
            try
            {
                userName = objBDSUserRepository.GetTeamMemberDetails(pcptl.CreatedBy, Sessions.ApplicationId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.Owner = userName.FirstName + " " + userName.LastName;
            return PartialView("_SetupLineitem", pcptlm);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Load Lineitem Setup Tab in edit mode.
        /// </summary>
        /// <param name="id">Plan Lineitem Id.</param>
        /// <returns>Returns Partial View Of edit Setup Tab.</returns>
        public ActionResult LoadEditSetupLineitem(int id)
        {
            ViewBag.IsCreated = false;

            //// Get LineItem Data by ID.
            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
            if (pcptl == null)
            {
                return null;
            }

            //// Get Plan data by PlanId.
            var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId);

            //// Get list of LineItem Types based on ModelId for current PlanId.
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select lit;
            foreach (var item in lineItemTypes)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.lineItemTypes = lineItemTypes;

            ViewBag.TacticTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Title);
            ViewBag.ProgramTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title);
            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title);
            ViewBag.Year = objPlan.Year;

            //// Set data to Plan_Campaign_Program_Tactic_LineItemModel to pass into PartialView.
            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            if (pcptl.LineItemTypeId == null)
            {
                pcptlm.IsOtherLineItem = true;
            }
            else
            {
                pcptlm.IsOtherLineItem = false;
            }
            pcptlm.PlanLineItemId = pcptl.PlanLineItemId;
            pcptlm.PlanTacticId = pcptl.PlanTacticId;
            pcptlm.LineItemTypeId = pcptl.LineItemTypeId == null ? 0 : Convert.ToInt32(pcptl.LineItemTypeId);
            pcptlm.Title = HttpUtility.HtmlDecode(pcptl.Title);
            pcptlm.Description = HttpUtility.HtmlDecode(pcptl.Description);
            pcptlm.StartDate = Convert.ToDateTime(pcptl.StartDate);
            pcptlm.EndDate = Convert.ToDateTime(pcptl.EndDate);
            pcptlm.Cost = pcptl.Cost;
            pcptlm.TStartDate = pcptl.Plan_Campaign_Program_Tactic.StartDate;
            pcptlm.TEndDate = pcptl.Plan_Campaign_Program_Tactic.EndDate;
            pcptlm.PStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.StartDate;
            pcptlm.PEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.EndDate;
            pcptlm.CStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.StartDate;
            pcptlm.CEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.EndDate;
            pcptlm.IsLineItemAddEdit = true;
            pcptlm.OwnerId = pcptl.CreatedBy;
            // Added by Arpita Soni for Ticket #2212 on 05/24/2016 
            pcptlm.PlanId = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
            User userName = new User();
            try
            {
                userName = objBDSUserRepository.GetTeamMemberDetails(pcptl.CreatedBy, Sessions.ApplicationId);
                //Added By Komal Rawal for #1974
                //Desc: To Enable edit owner feature from Lineitem popup.
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                //Modified By Komal Rawal for #1360
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); // PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);

                if (lstClientUsers.Count() > 0)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = false;


                    string strUserList = string.Join(",", lstClientUsers);
                    //List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    //lstUserDetails = lstUserDetails.Where(i => !i.IsDeleted).ToList();
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();

                        pcptlm.OwnerList = lstPreparedOwners.Select(u => new SelectListUser { Name = u.DisplayName, Id = u.UserId }).ToList();

                    }
                    else
                    {

                        pcptlm.OwnerList = new List<SelectListUser>();
                    }
                }
                else
                {

                    pcptlm.OwnerList = new List<SelectListUser>();
                }
                //End
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.Owner = userName.FirstName + " " + userName.LastName;
            return PartialView("_EditSetupLineitem", pcptlm);
        }

        /// <summary>
        /// Action to Save LineItem data.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="tacticId">Tactic Id</param>
        /// <param name="UserId"></param>
        /// <param name="title"></param>
        /// <returns>Returns Partial View Of edit Setup Tab.</returns>
        [HttpPost]

        public ActionResult SaveLineitem(Plan_Campaign_Program_Tactic_LineItemModel form, string title, string FieldMappingValues, string customFieldInputs, string UserId = "", int tacticId = 0)
        {
            //// Check whether current user is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                #region " Get CampaignId, ProgramID and TacticId"
                int cid = 0;
                int pid = 0;
                int tid = 0;

                var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == form.PlanTacticId);
                int? LinkedTacticId;
                if (objTactic != null)
                {
                    cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                    pid = objTactic.PlanProgramId;
                    tid = form.PlanTacticId;
                    LinkedTacticId = objTactic.LinkedTacticId;

                }
                else
                {
                    objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == tacticId);
                    tid = tacticId;
                    cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                    pid = objTactic.PlanProgramId;
                    LinkedTacticId = objTactic.LinkedTacticId;

                }

                int planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == cid && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
                #endregion

                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);
                //  //Added By Komal Rawal for LinkedLineItem change PL ticket #1853
                Plan_Campaign_Program_Tactic_LineItem Lineitemobj = new Plan_Campaign_Program_Tactic_LineItem();
                Plan_Campaign_Program_Tactic ObjLinkedTactic = new Plan_Campaign_Program_Tactic();
                int yearDiff = 0, perdNum = 12, cntr = 0;
                bool isMultiYearlinkedTactic = false;
                List<string> lstLinkedPeriods = new List<string>();
                //List<Plan_Campaign_Program_Tactic_LineItem> tblLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(list => list.IsDeleted == false).ToList();

                var LinkedLineitemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanLineItemId == form.PlanLineItemId && id.IsDeleted == false).Select(id => id.LinkedLineItemId).FirstOrDefault();
                var TblTactic = db.Plan_Campaign_Program_Tactic.Where(id => (id.PlanTacticId == LinkedTacticId || id.LinkedTacticId == form.PlanTacticId || id.PlanTacticId == form.PlanTacticId) && id.IsDeleted == false).ToList();
                if (LinkedLineitemId != null)
                {
                    Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanLineItemId == LinkedLineitemId && id.IsDeleted == false).ToList().FirstOrDefault();


                }
                if (LinkedTacticId != null && LinkedTacticId > 0)
                {


                    ObjLinkedTactic = TblTactic.Where(id => id.PlanTacticId == LinkedTacticId).ToList().FirstOrDefault();
                    yearDiff = ObjLinkedTactic.EndDate.Year - ObjLinkedTactic.StartDate.Year;
                    isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                    cntr = 12 * yearDiff;
                    for (int i = 1; i <= cntr; i++)
                    {
                        lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                    }
                }
                //else
                //{
                //    Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.LinkedLineItemId == form.PlanLineItemId && id.IsDeleted == false).ToList().FirstOrDefault();
                //    if (Lineitemobj != null)
                //    {

                //        LinkedLineitemId = Lineitemobj.PlanLineItemId;
                //        ObjLinkedTactic = Lineitemobj.Plan_Campaign_Program_Tactic;
                //        LinkedTacticId = ObjLinkedTactic.PlanTacticId;
                //    }
                //}
                //if (LinkedTacticId == null || Lineitemobj == null)
                //{
                //    ObjLinkedTactic = TblTactic.Where(id => id.LinkedTacticId == form.PlanTacticId && id.IsDeleted == false).ToList().FirstOrDefault();
                //    if (ObjLinkedTactic != null)
                //    {
                //        LinkedTacticId = ObjLinkedTactic.PlanTacticId;
                //    }
                //    else
                //    {
                //        ObjLinkedTactic = TblTactic.Where(id => id.PlanTacticId == LinkedTacticId).ToList().FirstOrDefault();
                //    }

                //}

                //End

                //// if  PlanLineItemId is null then insert new record to table.
                if (form.PlanLineItemId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        int lineItemId = 0;
                        int? NewLinkLineItemID = 0;
                        using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record to check duplication.
                            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           where pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcptl.IsDeleted.Equals(false)
                                           && pcptl.PlanTacticId == tid
                                           select pcptl).FirstOrDefault();
                            //Check duplicate for Linked LineItem 
                            //Added By Komal Rawal for PL ticket #1853
                            //var  LinkedLi = new Plan_Campaign_Program_Tactic_LineItem();
                            //if(LinkedTacticId != null)
                            //{
                            //     LinkedLi = (from pcptli in db.Plan_Campaign_Program_Tactic_LineItem
                            //                 where pcptli.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcptli.PlanLineItemId != LinkedLineitemId && pcptli.IsDeleted.Equals(false)
                            //                   && pcptli.PlanTacticId == LinkedTacticId
                            //                    select pcptli).FirstOrDefault();

                            //}
                            //else
                            //{
                            //    LinkedLi = null;
                            //}

                            //End
                            //// if duplicate record exist then return duplication message.
                            if (pcptvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                //string strduplicatedlinkedlineitem = "";
                                //if (LinkedLi != null)
                                //{
                                //    strduplicatedlinkedlineitem = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()] + " in the linkedtactic");    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                //}
                                return Json(new { msg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic_LineItem LinkedobjLineitem = null;
                                if (LinkedTacticId != null)
                                {
                                    int linkedPlanTacticId = Convert.ToInt32(LinkedTacticId);
                                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                                    linkedTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == linkedPlanTacticId).FirstOrDefault();

                                    #region " Insert Link LineItem record for Specific Linked Tactic."
                                    LinkedobjLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    LinkedobjLineitem.PlanTacticId = linkedPlanTacticId;
                                    LinkedobjLineitem.Title = form.Title;
                                    //LinkedobjLineitem.LineItemTypeId = form.LineItemTypeId;
                                    #region "update linked lineitem lineItem Type"
                                    if (form.LineItemTypeId > 0 && linkedTactic != null)
                                    {
                                        int destModelId = linkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;
                                        string srcLineItemTypeTitle = db.LineItemTypes.FirstOrDefault(type => type.LineItemTypeId == form.LineItemTypeId).Title;
                                        LineItemType destLineItemType = db.LineItemTypes.FirstOrDefault(_tacType => _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.Title == srcLineItemTypeTitle);
                                        if (destLineItemType != null)
                                        {
                                            LinkedobjLineitem.LineItemTypeId = destLineItemType.LineItemTypeId;
                                        }
                                    }
                                    #endregion

                                    LinkedobjLineitem.Description = form.Description;
                                    //Added By :Kalpesh Sharma #890 Line Item Dates need to go away
                                    LinkedobjLineitem.StartDate = null;
                                    LinkedobjLineitem.EndDate = null;
                                    // LinkedobjLineitem.Cost = form.Cost;
                                    LinkedobjLineitem.CreatedBy = Sessions.User.UserId;
                                    //LinkedobjLineitem.CreatedBy = form.OwnerId;
                                    LinkedobjLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(LinkedobjLineitem).State = EntityState.Added;
                                    int Finalresult = db.SaveChanges();
                                    NewLinkLineItemID = LinkedobjLineitem.PlanLineItemId;
                                    #endregion


                                }
                                #region " Insert LineItem record for Specific Improvement Tactic."
                                Plan_Campaign_Program_Tactic_LineItem objLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                objLineitem.PlanTacticId = tacticId;
                                objLineitem.Title = form.Title;
                                objLineitem.LineItemTypeId = form.LineItemTypeId;
                                objLineitem.Description = form.Description;
                                //Added By :Kalpesh Sharma #890 Line Item Dates need to go away
                                objLineitem.StartDate = null;
                                objLineitem.EndDate = null;
                                objLineitem.Cost = form.Cost;
                                //objLineitem.CreatedBy = Sessions.User.UserId;  //commented by Rahul Shah on 17/03/2016 for PL #2032
                                objLineitem.CreatedBy = form.OwnerId;            //added by Rahul Shah on 17/03/2016 for PL #2032
                                objLineitem.CreatedDate = DateTime.Now;
                                objLineitem.LinkedLineItemId = NewLinkLineItemID == 0 ? null : NewLinkLineItemID;
                                db.Entry(objLineitem).State = EntityState.Added;
                                int result = db.SaveChanges();
                                lineItemId = objLineitem.PlanLineItemId;
                                //Added by Rahul Shah on 17/03/2016 for PL #2068
                                if (result > 0)
                                {
                                    #region "Send Email Notification For Owner changed"
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != Sessions.User.UserId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", Sessions.User.UserId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.                                                    
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title.ToString();
                                            string TacticTitle = objLineitem.Plan_Campaign_Program_Tactic.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, lineItemId, Enums.Section.LineItem.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, TacticTitle, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.LineItem.ToString().ToLower(), strURL, objLineitem.Title);
                                            }
                                        }

                                    }
                                    #endregion
                                }
                                #endregion

                                if (LinkedobjLineitem != null)
                                {
                                    LinkedobjLineitem.LinkedLineItemId = lineItemId;
                                    if (isMultiYearlinkedTactic)
                                    {
                                        //cntr = 12 * yearDiff;
                                        //for (int i = 1; i <= cntr; i++)
                                        //{
                                        //    lstLinkedPeriods.Add(PeriodChar + (i).ToString());
                                        //    lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                        //}

                                        //var TotalCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(id => (id.PlanLineItemId == lineItemId) && lstLinkedPeriods.Contains(id.Period)).ToList().Sum(l => l.Value);
                                        LinkedobjLineitem.Cost = form.Cost;

                                    }
                                    db.Entry(LinkedobjLineitem).State = EntityState.Modified;
                                    int Linkedresult = Common.InsertChangeLog(planid, null, LinkedobjLineitem.PlanLineItemId, LinkedobjLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                    db.SaveChanges();

                                }

                                #region Save Field Mapping Details
                                var MappingFields = JsonConvert.DeserializeObject<List<BudgetAccountMapping>>(FieldMappingValues);
                                LineItem_Budget LineitemBudgetMapping = new LineItem_Budget();
                                if (MappingFields.Count > 0)
                                {
                                    foreach (var item in MappingFields)
                                    {
                                        LineitemBudgetMapping = new LineItem_Budget();
                                        LineitemBudgetMapping.BudgetDetailId = item.Id;
                                        LineitemBudgetMapping.PlanLineItemId = lineItemId;
                                        LineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                        LineitemBudgetMapping.CreatedDate = DateTime.Now;
                                        LineitemBudgetMapping.Weightage = (byte)item.Weightage;
                                        db.Entry(LineitemBudgetMapping).State = EntityState.Added;
                                    }
                                }
                                else
                                {
                                    // Add By Nishant Sheth
                                    // Desc : #1672 if any Budget line item not selected then assoicated with other line item.
                                    int OtherId = Common.GetOtherBudgetId();
                                    LineitemBudgetMapping.BudgetDetailId = OtherId;
                                    LineitemBudgetMapping.PlanLineItemId = lineItemId;
                                    LineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                    LineitemBudgetMapping.CreatedDate = DateTime.Now;
                                    LineitemBudgetMapping.Weightage = 100;
                                    db.Entry(LineitemBudgetMapping).State = EntityState.Added;
                                }

                                db.SaveChanges();

                                #endregion

                                #region "Save custom field to CustomField_Entity table"
                                if (customFields.Count != 0)
                                {
                                    CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                    foreach (var item in customFields)
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = lineItemId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                db.SaveChanges();
                                #endregion

                                //// Calculate TotalLineItemCost.
                                var objOtherLineItem = objTactic.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == tid && l.LineItemTypeId == null);
                                double totalLoneitemCost = objTactic.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == tid && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == objTactic.PlanTacticId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                                List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                                int startmonth = objTactic.StartDate.Month;
                                //Added By komal Rawal for #1249
                                if (objLineitem.Cost > 0)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                    objlineitemCost.PlanLineItemId = objLineitem.PlanLineItemId;
                                    objlineitemCost.Period = PeriodChar + startmonth;
                                    objlineitemCost.Value = form.Cost;
                                    objlineitemCost.CreatedBy = Sessions.User.UserId;
                                    objlineitemCost.CreatedDate = DateTime.Now;
                                    db.Entry(objlineitemCost).State = EntityState.Added;
                                }

                                if (tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                {
                                    var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != lineItemId && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + form.Cost;
                                    if (tacticlineitemcostmonth > tacticmonthcost)
                                    {
                                        tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value = tacticlineitemcostmonth;
                                        if (objTactic.Cost < tacticlineitemcostmonth)
                                        {
                                            objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                        }
                                    }
                                }
                                else
                                {
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != lineItemId && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + form.Cost;
                                    Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                    objtacticCost.PlanTacticId = objTactic.PlanTacticId;
                                    objtacticCost.Period = PeriodChar + startmonth;
                                    objtacticCost.Value = tacticlineitemcostmonth;
                                    objtacticCost.CreatedBy = Sessions.User.UserId;
                                    objtacticCost.CreatedDate = DateTime.Now;
                                    db.Entry(objtacticCost).State = EntityState.Added;
                                    objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                                }
                                objTactic.ModifiedBy = Sessions.User.UserId;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;
                                double LinkedtotalLineitemCost = 0;
                                if (LinkedTacticId != null)
                                {
                                    //// Calculate TotalLineItemCost.
                                    LinkedtotalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                                }
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objLinkedNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    if (LinkedTacticId != null)
                                    {
                                        objLinkedNewLineitem.PlanTacticId = Convert.ToInt32(LinkedTacticId);
                                        objLinkedNewLineitem.Title = Common.LineItemTitleDefault + ObjLinkedTactic.Title;
                                        if (ObjLinkedTactic.Cost > LinkedtotalLineitemCost)
                                        {
                                            objLinkedNewLineitem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
                                        }
                                        else
                                        {
                                            objLinkedNewLineitem.Cost = 0;
                                        }
                                        objLinkedNewLineitem.Description = string.Empty;
                                        //objLinkedNewLineitem.CreatedBy = Sessions.User.UserId;  // commented by Rahul Shah on 17/03/2016 for PL #2032 
                                        objLinkedNewLineitem.CreatedBy = form.OwnerId;            // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                        objLinkedNewLineitem.CreatedDate = DateTime.Now;
                                        db.Entry(objLinkedNewLineitem).State = EntityState.Added;
                                        ObjLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                        ObjLinkedTactic.ModifiedDate = DateTime.Now;
                                        db.Entry(ObjLinkedTactic).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = tacticId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                                    if (objTactic.Cost > totalLoneitemCost)
                                    {
                                        objNewLineitem.Cost = objTactic.Cost - totalLoneitemCost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    //objNewLineitem.CreatedBy = Sessions.User.UserId;  // commented by Rahul Shah on 17/03/2016 for PL #2032 
                                    objNewLineitem.CreatedBy = form.OwnerId;            // Added by Rahul Shah on 17/03/2016 for PL #2032 
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    objNewLineitem.LinkedLineItemId = objLinkedNewLineitem.PlanLineItemId;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                }
                                else
                                {
                                    if (LinkedTacticId != null)
                                    {
                                        var objLinkedOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId == null && l.IsDeleted == false);
                                        if (objLinkedOtherLineItem != null)
                                        {
                                            objLinkedOtherLineItem.IsDeleted = false;
                                            if (objLinkedOtherLineItem.Cost > LinkedtotalLineitemCost)
                                            {
                                                objLinkedOtherLineItem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
                                            }
                                            else
                                            {
                                                objLinkedOtherLineItem.Cost = 0;
                                                objLinkedOtherLineItem.IsDeleted = true;
                                            }
                                            db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
                                        }
                                        ObjLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                        ObjLinkedTactic.ModifiedDate = DateTime.Now;
                                        db.Entry(ObjLinkedTactic).State = EntityState.Modified;

                                    }
                                    objOtherLineItem.IsDeleted = false;
                                    if (objTactic.Cost > totalLoneitemCost)
                                    {
                                        objOtherLineItem.Cost = objTactic.Cost - totalLoneitemCost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                }


                                //// Insert Chnage Log to DB.
                                result = Common.InsertChangeLog(planid, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                db.SaveChanges();

                                if (LinkedTacticId != null)
                                {

                                    #region Save Field Mapping Details
                                    var LinkedMappingFields = JsonConvert.DeserializeObject<List<BudgetAccountMapping>>(FieldMappingValues);
                                    LineItem_Budget LineitemBudgetMappingobj = new LineItem_Budget();
                                    if (LinkedMappingFields.Count > 0)
                                    {
                                        foreach (var item in LinkedMappingFields)
                                        {
                                            LineitemBudgetMappingobj = new LineItem_Budget();
                                            LineitemBudgetMappingobj.BudgetDetailId = item.Id;
                                            LineitemBudgetMappingobj.PlanLineItemId = Convert.ToInt32(NewLinkLineItemID);
                                            LineitemBudgetMappingobj.CreatedBy = Sessions.User.UserId;
                                            LineitemBudgetMappingobj.CreatedDate = DateTime.Now;
                                            LineitemBudgetMappingobj.Weightage = (byte)item.Weightage;
                                            db.Entry(LineitemBudgetMappingobj).State = EntityState.Added;
                                        }
                                    }
                                    else
                                    {
                                        // Add By Nishant Sheth
                                        // Desc : #1672 if any Budget line item not selected then assoicated with other line item.
                                        int OtherId = Common.GetOtherBudgetId();
                                        LineitemBudgetMapping.BudgetDetailId = OtherId;
                                        LineitemBudgetMapping.PlanLineItemId = Convert.ToInt32(NewLinkLineItemID);
                                        LineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                        LineitemBudgetMapping.CreatedDate = DateTime.Now;
                                        LineitemBudgetMapping.Weightage = 100;
                                        db.Entry(LineitemBudgetMapping).State = EntityState.Added;
                                    }

                                    //db.SaveChanges();

                                    #endregion

                                    #region "Save custom field to CustomField_Entity table"
                                    if (customFields.Count != 0)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        foreach (var item in customFields)
                                        {
                                            objcustomFieldEntity = new CustomField_Entity();
                                            objcustomFieldEntity.EntityId = Convert.ToInt32(NewLinkLineItemID);
                                            objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                            objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                            objcustomFieldEntity.CreatedDate = DateTime.Now;
                                            objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                            db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                        }
                                    }
                                    db.SaveChanges();
                                    #endregion

                                    //#region "Update Linked Tactic Budget data"


                                    //list<plan_campaign_program_tactic_lineitem_cost> tbllineitemdata = db.plan_campaign_program_tactic_lineitem_cost.where(id => (id.planlineitemid == lineitemid) || (id.planlineitemid == newlinklineitemid)).tolist();
                                    //list<plan_campaign_program_tactic_lineitem_cost> lstlineitemdata = tbllineitemdata.where(id => id.planlineitemid == lineitemid).tolist();

                                    //list<plan_campaign_program_tactic_cost> tbltacticdata = db.plan_campaign_program_tactic_cost.where(id => id.plantacticid == objtactic.plantacticid || id.plantacticid == linkedtacticid).tolist();
                                    //list<plan_campaign_program_tactic_cost> tacticdata = tbltacticdata.where(id => id.plantacticid == objtactic.plantacticid).tolist();

                                    //if (lstlineitemdata != null && lstlineitemdata.count > 0)
                                    //{
                                    //    list<plan_campaign_program_tactic_lineitem_cost> linkedlineitemdata = tbllineitemdata.where(id => id.planlineitemid == newlinklineitemid).tolist();
                                    //    linkedlineitemdata.foreach(bdgt => db.entry(bdgt).state = entitystate.deleted);
                                    //    lstlineitemdata.foreach(bdgt => { bdgt.planlineitemid = convert.toint32(newlinklineitemid); db.entry(bdgt).state = entitystate.added; db.plan_campaign_program_tactic_lineitem_cost.add(bdgt); });
                                    //    if (!tacticostslist.where(pcptc => pcptc.period == periodchar + startmonth).any())
                                    //    {
                                    //        list<plan_campaign_program_tactic_cost> linkedtacticdata = tbltacticdata.where(id => id.plantacticid == linkedtacticid).tolist();
                                    //        linkedtacticdata.foreach(bdgt => db.entry(bdgt).state = entitystate.deleted);
                                    //        tacticdata.foreach(bdgt => { bdgt.plantacticid = convert.toint32(linkedtacticid); db.entry(bdgt).state = entitystate.added; db.plan_campaign_program_tactic_cost.add(bdgt); });
                                    //    }
                                    //    db.savechanges();
                                    //}
                                    //#endregion
                                    #region "Update Linked Tactic Budget data"
                                    List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemData = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(id => (id.PlanLineItemId == form.PlanLineItemId) || (id.PlanLineItemId == LinkedLineitemId)).ToList();
                                    List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == form.PlanLineItemId).ToList();
                                    List<Plan_Campaign_Program_Tactic_Cost> linkedSrcTacticCostData = TblTactic.FirstOrDefault(id => id.PlanTacticId == form.PlanTacticId).Plan_Campaign_Program_Tactic_Cost.ToList();
                                    List<Plan_Campaign_Program_Tactic_Cost> linkedTacticCostData = TblTactic.FirstOrDefault(id => id.PlanTacticId == LinkedTacticId).Plan_Campaign_Program_Tactic_Cost.ToList();
                                    //List<Plan_Campaign_Program_Tactic_Cost> tblTacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId || id.PlanTacticId == LinkedTacticId).ToList();
                                    //List<Plan_Campaign_Program_Tactic_Cost> TacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId).ToList();

                                    if (lstLineItemData != null && lstLineItemData.Count > 0)
                                    {
                                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> linkedCostData = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
                                        Plan_Campaign_Program_Tactic_LineItem_Cost objlinkedCost = null;
                                        if (isMultiYearlinkedTactic)
                                        {
                                            linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId && lstLinkedPeriods.Contains(line.Period)).ToList();
                                            if (linkedCostData != null && linkedCostData.Count > 0)
                                                linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                            foreach (Plan_Campaign_Program_Tactic_LineItem_Cost cost in lstLineItemData)
                                            {
                                                string orgPeriod = cost.Period;
                                                string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                int NumPeriod = int.Parse(numPeriod);

                                                objlinkedCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                objlinkedCost.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
                                                objlinkedCost.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                objlinkedCost.Value = cost.Value;
                                                objlinkedCost.CreatedDate = cost.CreatedDate;
                                                objlinkedCost.CreatedBy = cost.CreatedBy;
                                                db.Entry(objlinkedCost).State = EntityState.Added;
                                            }
                                            // Delete old budget data.


                                            // Tactic Cost Data

                                            //linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);

                                            linkedTacticCostData = linkedTacticCostData.Where(line => lstLinkedPeriods.Contains(line.Period)).ToList();
                                            if (linkedTacticCostData != null && linkedTacticCostData.Count > 0)
                                                linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                            Plan_Campaign_Program_Tactic_Cost objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                            foreach (Plan_Campaign_Program_Tactic_Cost cost in linkedSrcTacticCostData)
                                            {
                                                string orgPeriod = cost.Period;
                                                string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                int NumPeriod = int.Parse(numPeriod);

                                                objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                                objlinkedTacCost.PlanTacticId = LinkedTacticId.Value;
                                                objlinkedTacCost.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                objlinkedTacCost.Value = cost.Value;
                                                objlinkedTacCost.CreatedDate = cost.CreatedDate;
                                                objlinkedTacCost.CreatedBy = cost.CreatedBy;
                                                db.Entry(objlinkedTacCost).State = EntityState.Added;
                                            }

                                        }
                                        else
                                        {
                                            linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId).ToList();
                                            if (linkedCostData != null && linkedCostData.Count > 0)
                                                linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                            foreach (Plan_Campaign_Program_Tactic_LineItem_Cost cost in lstLineItemData)
                                            {
                                                string orgPeriod = cost.Period;
                                                string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                int NumPeriod = int.Parse(numPeriod);
                                                if (NumPeriod > 12)
                                                {
                                                    int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                    int div = NumPeriod / 12;    // In case of 24, Y12.
                                                    if (rem > 0 || div > 1)
                                                    {
                                                        objlinkedCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                        objlinkedCost.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
                                                        objlinkedCost.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());     // (12*1)+3 = 15 => For March(Y15) month.
                                                        objlinkedCost.Value = cost.Value;
                                                        objlinkedCost.CreatedDate = cost.CreatedDate;
                                                        objlinkedCost.CreatedBy = cost.CreatedBy;
                                                        db.Entry(objlinkedCost).State = EntityState.Added;
                                                    }
                                                }
                                            }

                                            linkedTacticCostData = linkedTacticCostData.Where(line => lstLinkedPeriods.Contains(line.Period)).ToList();
                                            if (linkedTacticCostData != null && linkedTacticCostData.Count > 0)
                                                linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                            Plan_Campaign_Program_Tactic_Cost objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                            foreach (Plan_Campaign_Program_Tactic_Cost cost in linkedSrcTacticCostData)
                                            {
                                                string orgPeriod = cost.Period;
                                                string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                int NumPeriod = int.Parse(numPeriod);
                                                if (NumPeriod > 12)
                                                {
                                                    int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                    int div = NumPeriod / 12;    // In case of 24, Y12.
                                                    if (rem > 0 || div > 1)
                                                    {
                                                        objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                                        objlinkedTacCost.PlanTacticId = LinkedTacticId.Value;
                                                        objlinkedTacCost.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());   // (12*1)+3 = 15 => For March(Y15) month.
                                                        objlinkedTacCost.Value = cost.Value;
                                                        objlinkedTacCost.CreatedDate = cost.CreatedDate;
                                                        objlinkedTacCost.CreatedBy = cost.CreatedBy;
                                                        db.Entry(objlinkedTacCost).State = EntityState.Added;
                                                    }
                                                }
                                            }


                                        }
                                        ObjLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                        ObjLinkedTactic.ModifiedDate = DateTime.Now;
                                        db.Entry(ObjLinkedTactic).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    #endregion


                                }

                            }

                            scope.Complete();
                            string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            return Json(new { isSaved = true, msg = strMessage, planLineitemID = lineItemId, planCampaignID = cid, planProgramID = pid, planTacticID = tid });
                        }
                    }
                }
                else    //// Update LineItem Record.
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        //Modified By Komal Rawal for #2166 Transaction deadlock elmah error
                        var TransactionOption = new System.Transactions.TransactionOptions();
                        TransactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

                        using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionOption))
                        {


                            //// Get duplicate record to check duplication.
                            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           where pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(form.PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                           && pcptl.PlanTacticId == tid
                                           select pcptl).FirstOrDefault();

                            //Added By Komal Rawal for PL ticket #1853
                            //var LinkedLi = new Plan_Campaign_Program_Tactic_LineItem();
                            //if (LinkedTacticId != null)
                            //{
                            //    LinkedLi = (from pcptli in db.Plan_Campaign_Program_Tactic_LineItem
                            //                where pcptli.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcptli.PlanLineItemId != LinkedLineitemId && pcptli.IsDeleted.Equals(false)
                            //               && pcptli.PlanTacticId == LinkedTacticId
                            //                select pcptli).FirstOrDefault();

                            //}
                            //else
                            //{
                            //    LinkedLi = null;
                            //}
                            //End
                            //// if duplicate record exist then return duplication message.
                            if (pcptvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                //string strduplicatedlinkedlineitem = "";
                                //if (LinkedLi != null)
                                //{
                                //    strduplicatedlinkedlineitem = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()] + " in the linkedtactic");    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                //}
                                return Json(new { msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Update record to Plan_Campaign_Program_Tactic_LineItem table."
                                Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(form.PlanLineItemId));
                                objLineitem.Description = form.Description;
                                objLineitem.Title = form.Title;
                                if (!form.IsOtherLineItem)
                                {

                                    objLineitem.LineItemTypeId = form.LineItemTypeId;

                                    //if ((db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(t => t.PlanLineItemId == form.PlanLineItemId).ToList()).Count() == 0 ||
                                    //    objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower()
                                    //|| objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                                    //{
                                    //    objLineitem.Cost = form.Cost;
                                    //}



                                    //Added By komal Rawal for #1249
                                    if (form.Cost > objLineitem.Cost)
                                    {
                                        var diffcost = form.Cost - objLineitem.Cost;
                                        int startmonth = objTactic.StartDate.Month; ;

                                        if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                            objlineitemCost.PlanLineItemId = objLineitem.PlanLineItemId;
                                            objlineitemCost.Period = PeriodChar + startmonth;
                                            objlineitemCost.Value = diffcost;
                                            objlineitemCost.CreatedBy = Sessions.User.UserId;
                                            objlineitemCost.CreatedDate = DateTime.Now;
                                            db.Entry(objlineitemCost).State = EntityState.Added;
                                        }

                                        List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == objTactic.PlanTacticId).ToList();
                                        List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                        var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                                        List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                                        double tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();

                                        if (tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
                                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != form.PlanLineItemId && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + form.Cost;
                                            if (tacticlineitemcostmonth > tacticmonthcost)
                                            {
                                                tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value = tacticlineitemcostmonth;
                                                objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                            }
                                        }
                                        else
                                        {
                                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != form.PlanLineItemId && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + form.Cost;
                                            Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            objtacticCost.PlanTacticId = objTactic.PlanTacticId;
                                            objtacticCost.Period = PeriodChar + startmonth;
                                            objtacticCost.Value = tacticlineitemcostmonth;
                                            objtacticCost.CreatedBy = Sessions.User.UserId;
                                            objtacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(objtacticCost).State = EntityState.Added;
                                            objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                                        }

                                    }
                                    else if (form.Cost < objLineitem.Cost)
                                    {
                                        var diffcost = objLineitem.Cost - form.Cost;
                                        int endmonth = 12;
                                        while (diffcost > 0 && endmonth != 0)
                                        {
                                            if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                            {
                                                double objtacticcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                                if (objtacticcost > diffcost)
                                                {
                                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                                    diffcost = 0;
                                                }
                                                else
                                                {
                                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = 0;
                                                    diffcost = diffcost - objtacticcost;
                                                }
                                            }
                                            if (endmonth > 0)
                                            {
                                                endmonth -= 1;
                                            }

                                        }

                                    }

                                    objLineitem.Cost = form.Cost;

                                    //End
                                }
                                objTactic.ModifiedBy = Sessions.User.UserId;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;

                                Guid oldOwnerId = objLineitem.CreatedBy;  //Added by Rahul Shah on 17/03/2016 for PL #2068 
                                objLineitem.ModifiedBy = Sessions.User.UserId;
                                objLineitem.CreatedBy = form.OwnerId;
                                objLineitem.ModifiedDate = DateTime.Now;
                                db.Entry(objLineitem).State = EntityState.Modified;
                                #endregion



                                #region Save Field Mapping Details
                                List<LineItem_Budget> ExistingValues = db.LineItem_Budget.Where(a => a.PlanLineItemId == form.PlanLineItemId).ToList();
                                ExistingValues.ForEach(Field => db.Entry(Field).State = EntityState.Deleted);
                                var MappingFields = JsonConvert.DeserializeObject<List<BudgetAccountMapping>>(FieldMappingValues);

                                LineItem_Budget LineitemBudgetMapping = new LineItem_Budget();

                                if (MappingFields.Count > 0)
                                {
                                    foreach (var item in MappingFields)
                                    {
                                        LineitemBudgetMapping = new LineItem_Budget();
                                        LineitemBudgetMapping.BudgetDetailId = item.Id;
                                        LineitemBudgetMapping.PlanLineItemId = form.PlanLineItemId;
                                        LineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                        LineitemBudgetMapping.CreatedDate = DateTime.Now;
                                        LineitemBudgetMapping.Weightage = (byte)item.Weightage;
                                        db.Entry(LineitemBudgetMapping).State = EntityState.Added;
                                    }
                                }
                                else
                                {
                                    // Add By Nishant Sheth
                                    // Desc : #1672 if any Budget line item not selected then assoicated with other line item.
                                    int OtherId = Common.GetOtherBudgetId();
                                    LineitemBudgetMapping.BudgetDetailId = OtherId;
                                    LineitemBudgetMapping.PlanLineItemId = form.PlanLineItemId; ;
                                    LineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                    LineitemBudgetMapping.CreatedDate = DateTime.Now;
                                    LineitemBudgetMapping.Weightage = 100;
                                    db.Entry(LineitemBudgetMapping).State = EntityState.Added;

                                }

                                db.SaveChanges();

                                #endregion

                                #region "Remove previous custom fields by PlanCampaignId"
                                string entityTypeLineitem = Enums.EntityType.Lineitem.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(custmfield => custmfield.EntityId == form.PlanLineItemId && custmfield.CustomField.EntityType == entityTypeLineitem).ToList();
                                prevCustomFieldList.ForEach(custmfield => db.Entry(custmfield).State = EntityState.Deleted);
                                #endregion

                                #region "Save Custom fields to CustomField_Entity table"
                                if (customFields.Count != 0)
                                {
                                    CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                    foreach (var item in customFields)
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = form.PlanLineItemId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }

                                db.SaveChanges();
                                #endregion

                                int result;
                                result = Common.InsertChangeLog(planid, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                db.SaveChanges();
                                //Added by Rahul Shah on 17/03/2016 for PL #2068 
                                if (result > 0)
                                {
                                    #region "Send Email Notification For Owner changed"
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != oldOwnerId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", oldOwnerId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(e);

                                                //To handle unavailability of BDSService
                                                if (e is System.ServiceModel.EndpointNotFoundException)
                                                {
                                                    //// Flag to indicate unavailability of web service.                                                    
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title.ToString();
                                            string TacticTitle = objLineitem.Plan_Campaign_Program_Tactic.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, form.PlanLineItemId, Enums.Section.LineItem.ToString().ToLower());
                                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, TacticTitle, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.LineItem.ToString().ToLower(), strURL, objLineitem.Title);
                                            }
                                        }

                                    }
                                    #endregion
                                }
                                if (LinkedLineitemId != null)
                                {
                                    if (isMultiYearlinkedTactic)
                                    {
                                        cntr = 12 * yearDiff;
                                        lstLinkedPeriods = new List<string>();
                                        for (int i = 1; i <= cntr; i++)
                                        {

                                            lstLinkedPeriods.Add(PeriodChar + (i).ToString());
                                            lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                        }

                                        var TotalCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(id => (id.PlanLineItemId == LinkedLineitemId) && lstLinkedPeriods.Contains(id.Period)).ToList().Sum(l => l.Value);
                                        Lineitemobj.Cost = TotalCost;

                                    }


                                }
                                if (!form.IsOtherLineItem)
                                {
                                    //// Calculate TotalLineItemCost.
                                    double totalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == tid && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                                    //// Insert or Modified LineItem Data.
                                    var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == tid && l.LineItemTypeId == null);
                                    double LinkedtotalLineitemCost = 0;
                                    if (LinkedLineitemId != null)
                                    {
                                        //// Calculate TotalLineItemCost.
                                        LinkedtotalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                                    }
                                    if (objOtherLineItem == null)
                                    {
                                        Plan_Campaign_Program_Tactic_LineItem objLinkedNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        if (LinkedLineitemId != null)
                                        {
                                            objLinkedNewLineitem.PlanTacticId = Convert.ToInt32(LinkedTacticId);
                                            objLinkedNewLineitem.Title = Common.LineItemTitleDefault + ObjLinkedTactic.Title;
                                            if (ObjLinkedTactic.Cost > LinkedtotalLineitemCost)
                                            {
                                                objLinkedNewLineitem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
                                            }
                                            else
                                            {
                                                objLinkedNewLineitem.Cost = 0;
                                            }
                                            objLinkedNewLineitem.Description = string.Empty;
                                            objLinkedNewLineitem.CreatedBy = Sessions.User.UserId;
                                            objLinkedNewLineitem.CreatedDate = DateTime.Now;
                                            db.Entry(objLinkedNewLineitem).State = EntityState.Added;
                                            ObjLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                            ObjLinkedTactic.ModifiedDate = DateTime.Now;
                                            db.Entry(ObjLinkedTactic).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        //// Insert New record to table.
                                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        objNewLineitem.PlanTacticId = form.PlanTacticId;
                                        objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                                        if (objTactic.Cost > totalLineitemCost)
                                        {
                                            objNewLineitem.Cost = objTactic.Cost - totalLineitemCost;
                                        }
                                        else
                                        {
                                            objNewLineitem.Cost = 0;
                                        }
                                        objNewLineitem.Description = string.Empty;
                                        objNewLineitem.CreatedBy = Sessions.User.UserId;
                                        objNewLineitem.CreatedDate = DateTime.Now;
                                        objNewLineitem.LinkedLineItemId = objLinkedNewLineitem.PlanLineItemId;
                                        db.Entry(objNewLineitem).State = EntityState.Added;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        if (LinkedLineitemId != null)
                                        {
                                            var objLinkedOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId == null && l.IsDeleted == false);
                                            if (objLinkedOtherLineItem != null)
                                            {
                                                objLinkedOtherLineItem.IsDeleted = false;
                                                if (objLinkedOtherLineItem.Cost > LinkedtotalLineitemCost)
                                                {
                                                    objLinkedOtherLineItem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
                                                }
                                                else
                                                {
                                                    objLinkedOtherLineItem.Cost = 0;
                                                    objLinkedOtherLineItem.IsDeleted = true;
                                                }
                                                db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
                                            }
                                            ObjLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                            ObjLinkedTactic.ModifiedDate = DateTime.Now;
                                            db.Entry(ObjLinkedTactic).State = EntityState.Modified;
                                        }

                                        objOtherLineItem.IsDeleted = false;
                                        if (objTactic.Cost > totalLineitemCost)
                                        {
                                            objOtherLineItem.Cost = objTactic.Cost - totalLineitemCost;
                                        }
                                        else
                                        {
                                            objOtherLineItem.Cost = 0;
                                            objOtherLineItem.IsDeleted = true;
                                        }
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                                    }
                                    db.SaveChanges();

                                }
                                //Modified By Komal Rawal for #1853
                                if (LinkedLineitemId != null && LinkedLineitemId.HasValue && LinkedLineitemId > 0)  //Modified by Maitri Gandhi for #1888 Observation 2 on 21/03/2016
                                {
                                    #region "Update record to Plan_Campaign_Program_Tactic_LineItem table."

                                    Lineitemobj.Description = form.Description;
                                    Lineitemobj.Title = form.Title;
                                    if (!form.IsOtherLineItem)
                                    {

                                        //Lineitemobj.LineItemTypeId = form.LineItemTypeId;

                                        #region "update linked lineitem lineItem Type"
                                        if (form.LineItemTypeId > 0)
                                        {
                                            int destModelId = Lineitemobj.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;
                                            string srcLineItemTypeTitle = db.LineItemTypes.FirstOrDefault(type => type.LineItemTypeId == form.LineItemTypeId).Title;
                                            LineItemType destLineItemType = db.LineItemTypes.FirstOrDefault(_tacType => _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.Title == srcLineItemTypeTitle);
                                            if (destLineItemType != null)
                                            {
                                                Lineitemobj.LineItemTypeId = destLineItemType.LineItemTypeId;
                                            }
                                        }
                                        #endregion

                                        //#region "Update Linked Tactic Budget data"


                                        //List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemData = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(id => (id.PlanLineItemId == form.PlanLineItemId) || (id.PlanLineItemId == LinkedLineitemId)).ToList();
                                        //List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == form.PlanLineItemId).ToList();

                                        //List<Plan_Campaign_Program_Tactic_Cost> tblTacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId || id.PlanTacticId == LinkedTacticId).ToList();
                                        //List<Plan_Campaign_Program_Tactic_Cost> TacticData = tblTacticData.Where(id => id.PlanTacticId == form.PlanTacticId).ToList();

                                        //if (lstLineItemData != null && lstLineItemData.Count > 0)
                                        //{
                                        //    List<Plan_Campaign_Program_Tactic_LineItem_Cost> linkedLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == LinkedLineitemId).ToList();
                                        //    linkedLineItemData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                        //    lstLineItemData.ForEach(bdgt => { bdgt.PlanLineItemId = Convert.ToInt32(LinkedLineitemId); db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_LineItem_Cost.Add(bdgt); });
                                        //    if (form.Cost > Lineitemobj.Cost)
                                        //    {
                                        //        List<Plan_Campaign_Program_Tactic_Cost> linkedTacticData = tblTacticData.Where(id => id.PlanTacticId == LinkedTacticId).ToList();
                                        //        linkedTacticData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                        //        TacticData.ForEach(bdgt => { bdgt.PlanTacticId = Convert.ToInt32(LinkedTacticId); db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_Cost.Add(bdgt); });
                                        //    }
                                        //    db.SaveChanges();
                                        //}
                                        //#endregion

                                        #region "Update Linked Tactic Budget data"
                                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemData = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(id => (id.PlanLineItemId == form.PlanLineItemId) || (id.PlanLineItemId == LinkedLineitemId)).ToList();
                                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == form.PlanLineItemId).ToList();
                                        List<Plan_Campaign_Program_Tactic_Cost> linkedSrcTacticCostData = TblTactic.FirstOrDefault(id => id.PlanTacticId == form.PlanTacticId).Plan_Campaign_Program_Tactic_Cost.ToList();
                                        List<Plan_Campaign_Program_Tactic_Cost> linkedTacticCostData = TblTactic.FirstOrDefault(id => id.PlanTacticId == LinkedTacticId).Plan_Campaign_Program_Tactic_Cost.ToList();
                                        //List<Plan_Campaign_Program_Tactic_Cost> tblTacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId || id.PlanTacticId == LinkedTacticId).ToList();
                                        //List<Plan_Campaign_Program_Tactic_Cost> TacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId).ToList();

                                        if (lstLineItemData != null && lstLineItemData.Count > 0)
                                        {
                                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> linkedCostData = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
                                            Plan_Campaign_Program_Tactic_LineItem_Cost objlinkedCost = null;
                                            List<string> lstTempPeriod = new List<string>();
                                            if (isMultiYearlinkedTactic)
                                            {
                                                linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId).ToList();
                                                if (isMultiYearlinkedTactic)
                                                {
                                                    lstLinkedPeriods = new List<string>();
                                                    cntr = 12 * yearDiff;
                                                    for (int i = 1; i <= cntr; i++)
                                                    {
                                                        lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                                    }

                                                    linkedCostData = linkedCostData.Where(id => lstLinkedPeriods.Contains(id.Period)).ToList();
                                                    if (linkedCostData != null && linkedCostData.Count > 0)
                                                        linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                }
                                                else
                                                {
                                                    if (linkedCostData != null && linkedCostData.Count > 0)
                                                        linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                }

                                                //if (linkedCostData != null && linkedCostData.Count > 0)
                                                //    linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                foreach (Plan_Campaign_Program_Tactic_LineItem_Cost cost in lstLineItemData)
                                                {
                                                    string orgPeriod = cost.Period;
                                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                    int NumPeriod = int.Parse(numPeriod);

                                                    objlinkedCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                    objlinkedCost.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
                                                    objlinkedCost.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                    objlinkedCost.Value = cost.Value;
                                                    objlinkedCost.CreatedDate = cost.CreatedDate;
                                                    objlinkedCost.CreatedBy = cost.CreatedBy;
                                                    db.Entry(objlinkedCost).State = EntityState.Added;
                                                }
                                                // Delete old budget data.


                                                // Tactic Cost Data

                                                //linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);

                                                linkedTacticCostData = linkedTacticCostData.Where(line => lstLinkedPeriods.Contains(line.Period)).ToList();
                                                if (linkedTacticCostData != null && linkedTacticCostData.Count > 0)
                                                    linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                Plan_Campaign_Program_Tactic_Cost objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                                foreach (Plan_Campaign_Program_Tactic_Cost cost in linkedSrcTacticCostData)
                                                {
                                                    string orgPeriod = cost.Period;
                                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                    int NumPeriod = int.Parse(numPeriod);

                                                    objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                                    objlinkedTacCost.PlanTacticId = LinkedTacticId.Value;
                                                    objlinkedTacCost.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                    objlinkedTacCost.Value = cost.Value;
                                                    objlinkedTacCost.CreatedDate = cost.CreatedDate;
                                                    objlinkedTacCost.CreatedBy = cost.CreatedBy;
                                                    db.Entry(objlinkedTacCost).State = EntityState.Added;
                                                }

                                            }
                                            else
                                            {
                                                //  linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId).ToList();
                                                linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId).ToList();
                                                if (isMultiYearlinkedTactic)
                                                {
                                                    lstLinkedPeriods = new List<string>();
                                                    cntr = 12 * yearDiff;
                                                    for (int i = 1; i <= cntr; i++)
                                                    {
                                                        lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                                    }

                                                    linkedCostData = linkedCostData.Where(id => lstLinkedPeriods.Contains(id.Period)).ToList();
                                                    if (linkedCostData != null && linkedCostData.Count > 0)
                                                        linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                }
                                                else
                                                {
                                                    if (linkedCostData != null && linkedCostData.Count > 0)
                                                        linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                }

                                                foreach (Plan_Campaign_Program_Tactic_LineItem_Cost cost in lstLineItemData)
                                                {
                                                    string orgPeriod = cost.Period;
                                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                    int NumPeriod = int.Parse(numPeriod);
                                                    if (NumPeriod > 12)
                                                    {
                                                        int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                        int div = NumPeriod / 12;    // In case of 24, Y12.
                                                        if (rem > 0 || div > 1)
                                                        {
                                                            objlinkedCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                            objlinkedCost.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
                                                            objlinkedCost.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());     // (12*1)+3 = 15 => For March(Y15) month.
                                                            objlinkedCost.Value = cost.Value;
                                                            objlinkedCost.CreatedDate = cost.CreatedDate;
                                                            objlinkedCost.CreatedBy = cost.CreatedBy;
                                                            db.Entry(objlinkedCost).State = EntityState.Added;
                                                        }
                                                    }
                                                }

                                                //linkedTacticCostData = linkedTacticCostData.Where(line => lstLinkedPeriods.Contains(line.Period)).ToList();
                                                if (linkedTacticCostData != null && linkedTacticCostData.Count > 0)
                                                    linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                                Plan_Campaign_Program_Tactic_Cost objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                                foreach (Plan_Campaign_Program_Tactic_Cost cost in linkedSrcTacticCostData)
                                                {
                                                    string orgPeriod = cost.Period;
                                                    string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                    int NumPeriod = int.Parse(numPeriod);
                                                    if (NumPeriod > 12)
                                                    {
                                                        int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                        int div = NumPeriod / 12;    // In case of 24, Y12.
                                                        if (rem > 0 || div > 1)
                                                        {
                                                            objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
                                                            objlinkedTacCost.PlanTacticId = LinkedTacticId.Value;
                                                            objlinkedTacCost.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());   // (12*1)+3 = 15 => For March(Y15) month.
                                                            objlinkedTacCost.Value = cost.Value;
                                                            objlinkedTacCost.CreatedDate = cost.CreatedDate;
                                                            objlinkedTacCost.CreatedBy = cost.CreatedBy;
                                                            db.Entry(objlinkedTacCost).State = EntityState.Added;
                                                            lstTempPeriod.Add(objlinkedTacCost.Period);
                                                        }
                                                    }
                                                }


                                            }

                                            db.SaveChanges();

                                            //List<Plan_Campaign_Program_Tactic_LineItem_Cost> linkedLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == LinkedLineitemId).ToList();
                                            //linkedLineItemData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                            //lstLineItemData.ForEach(bdgt => { bdgt.PlanLineItemId = Convert.ToInt32(LinkedLineitemId); db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_LineItem_Cost.Add(bdgt); });


                                            //List<Plan_Campaign_Program_Tactic_Cost> linkedTacticData = tblTacticData.Where(id => id.PlanTacticId == LinkedTacticId).ToList();
                                            //linkedTacticData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                                            //TacticData.ForEach(bdgt => { bdgt.PlanTacticId = Convert.ToInt32(LinkedTacticId); db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_Cost.Add(bdgt); });

                                            //db.SaveChanges();
                                        }
                                        #endregion


                                        //  Lineitemobj.Cost = form.Cost;

                                        //End
                                    }

                                    Lineitemobj.ModifiedBy = Sessions.User.UserId;
                                    Lineitemobj.ModifiedDate = DateTime.Now;
                                    Lineitemobj.CreatedBy = form.OwnerId;
                                    db.Entry(Lineitemobj).State = EntityState.Modified;

                                    if (LinkedTacticId > 0)
                                    {
                                        ObjLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                        ObjLinkedTactic.ModifiedDate = DateTime.Now;
                                        db.Entry(ObjLinkedTactic).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    #endregion



                                    #region Save Field Mapping Details
                                    List<LineItem_Budget> LinkedExistingValues = db.LineItem_Budget.Where(a => a.PlanLineItemId == LinkedLineitemId).ToList();
                                    LinkedExistingValues.ForEach(Field => db.Entry(Field).State = EntityState.Deleted);
                                    var LinkedMappingFields = JsonConvert.DeserializeObject<List<BudgetAccountMapping>>(FieldMappingValues);

                                    LineItem_Budget LinkedLineitemBudgetMapping = new LineItem_Budget();

                                    if (LinkedMappingFields.Count > 0)
                                    {
                                        foreach (var item in LinkedMappingFields)
                                        {
                                            LinkedLineitemBudgetMapping = new LineItem_Budget();
                                            LinkedLineitemBudgetMapping.BudgetDetailId = item.Id;
                                            LinkedLineitemBudgetMapping.PlanLineItemId = LinkedLineitemId.Value;
                                            LinkedLineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                            LinkedLineitemBudgetMapping.CreatedDate = DateTime.Now;
                                            LinkedLineitemBudgetMapping.Weightage = (byte)item.Weightage;
                                            db.Entry(LinkedLineitemBudgetMapping).State = EntityState.Added;
                                        }
                                    }
                                    else
                                    {
                                        // Add By Nishant Sheth
                                        // Desc : #1672 if any Budget line item not selected then assoicated with other line item.
                                        int OtherId = Common.GetOtherBudgetId();
                                        LinkedLineitemBudgetMapping.BudgetDetailId = OtherId;
                                        LinkedLineitemBudgetMapping.PlanLineItemId = LinkedLineitemId.Value;
                                        LinkedLineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                                        LinkedLineitemBudgetMapping.CreatedDate = DateTime.Now;
                                        LinkedLineitemBudgetMapping.Weightage = 100;
                                        db.Entry(LinkedLineitemBudgetMapping).State = EntityState.Added;
                                    }

                                    db.SaveChanges();

                                    #endregion

                                    #region "Remove previous custom fields by PlanCampaignId"
                                    string LinkedentityTypeLineitem = Enums.EntityType.Lineitem.ToString();
                                    var LinkedprevCustomFieldList = db.CustomField_Entity.Where(custmfield => custmfield.EntityId == LinkedLineitemId && custmfield.CustomField.EntityType == LinkedentityTypeLineitem).ToList();
                                    LinkedprevCustomFieldList.ForEach(custmfield => db.Entry(custmfield).State = EntityState.Deleted);
                                    #endregion

                                    #region "Save Custom fields to CustomField_Entity table"
                                    if (customFields.Count != 0)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        foreach (var item in customFields)
                                        {
                                            objcustomFieldEntity = new CustomField_Entity();
                                            objcustomFieldEntity.EntityId = Lineitemobj.PlanLineItemId;
                                            objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                            objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                            objcustomFieldEntity.CreatedDate = DateTime.Now;
                                            objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                            db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                        }
                                    }

                                    db.SaveChanges();
                                    #endregion

                                    int Finalresult;
                                    Finalresult = Common.InsertChangeLog(planid, null, Lineitemobj.PlanLineItemId, Lineitemobj.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                    Finalresult = db.SaveChanges();


                                }

                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { isSaved = true, msg = strMessage, planLineitemID = form.PlanLineItemId, planCampaignID = cid, planProgramID = pid, planTacticID = tid });

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { });
        }

        #region Budget Allocation for Line Item Tab
        // Commented by Arpita Soni for Ticket #2236 on 06/20/2016
        ///// <summary>
        ///// Fetch the Line Item Budget Allocation 
        ///// </summary>
        ///// <param name="id">Line Item Id</param>
        ///// <returns></returns>
        //public PartialViewResult LoadLineItemBudgetAllocation(int id = 0)
        //{
        //    Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
        //    if (pcptl == null)
        //    {
        //        return null;
        //    }

        //    if (Sessions.User.UserId == pcptl.CreatedBy)
        //    {
        //        ViewBag.IsOwner = true;
        //    }
        //    else
        //    {
        //        ViewBag.IsOwner = false;
        //    }

        //    List<int> lstEditableTactic = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, new List<int>() { pcptl.Plan_Campaign_Program_Tactic.PlanTacticId }, false);
        //    if (lstEditableTactic.Contains(pcptl.Plan_Campaign_Program_Tactic.PlanTacticId))
        //    {
        //        ViewBag.IsAllowCustomRestriction = true;
        //    }
        //    else
        //    {
        //        ViewBag.IsAllowCustomRestriction = false;
        //    }

        //    Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
        //    if (pcptl.LineItemTypeId == null)
        //    {
        //        pcptlm.IsOtherLineItem = true;
        //    }
        //    else
        //    {
        //        pcptlm.IsOtherLineItem = false;
        //    }

        //    pcptlm.PlanTacticId = pcptl.PlanTacticId;
        //    pcptlm.PlanLineItemId = pcptl.PlanLineItemId;

        //    var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId);
        //    pcptlm.AllocatedBy = objPlan.AllocatedBy;

        //    double totalLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == pcptl.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
        //    double TacticCost = pcptl.Plan_Campaign_Program_Tactic.Cost;
        //    double diffCost = TacticCost - totalLineItemCost;
        //    double otherLineItemCost = diffCost < 0 ? 0 : diffCost;

        //    ViewBag.tacticCost = TacticCost;
        //    ViewBag.totalLineItemCost = totalLineItemCost;
        //    ViewBag.otherLineItemCost = otherLineItemCost;
        //    // Add By Nishant Sheth
        //    // Desc :: To add multiple year with budget allocation
        //    var TacticEndYear = pcptl.Plan_Campaign_Program_Tactic.EndDate.Year;
        //    var TacticStartYear = pcptl.Plan_Campaign_Program_Tactic.StartDate.Year;
        //    ViewBag.YearDiffrence = Convert.ToInt32(Convert.ToInt32(TacticEndYear) - Convert.ToInt32(TacticStartYear));
        //    ViewBag.StartYear = Convert.ToInt32(TacticStartYear);

        //    pcptlm.Cost = pcptl.Cost;

        //    return PartialView("_SetupLineitemBudgetAllocation", pcptlm);
        //}

        ///// <summary>
        ///// Action to Save Line Item Allocation.
        ///// </summary>
        ///// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        ///// <param name="CostInputValues">Cost Input Values.</param>
        ///// <param name="UserId">User Id</param>
        ///// <param name="title"></param>
        ///// <returns>Returns Action Result.</returns>
        //[HttpPost]
        //public ActionResult SaveLineItemBudgetAllocation(Plan_Campaign_Program_Tactic_LineItemModel form, string CostInputValues, string UserId = "", string title = "", string AllocatedBy = "", int YearDiffrence = 0)
        //{
        //    //// check whether current userId is loggined user or not.
        //    if (!string.IsNullOrEmpty(UserId))
        //    {
        //        if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
        //        {
        //            return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    try
        //    {
        //        string[] arrCostInputValues = CostInputValues.Split(',');

        //        double tacticost = 0;
        //        var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == form.PlanTacticId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
        //        List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == form.PlanTacticId).ToList();

        //        int? LinkedTacticId;
        //        int cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
        //        int pid = objTactic.PlanProgramId;
        //        int tid = form.PlanTacticId;
        //        LinkedTacticId = objTactic.LinkedTacticId;
        //        int planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == cid && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
        //        int yearDiff = 0, perdNum = 12, cntr = 0;
        //        bool isMultiYearlinkedTactic = false;
        //        List<string> lstLinkedPeriods = new List<string>();
        //        //Added By Komal Rawal for LinkedLineItem change PL ticket #1853
        //        Plan_Campaign_Program_Tactic_LineItem Lineitemobj = new Plan_Campaign_Program_Tactic_LineItem();
        //        Plan_Campaign_Program_Tactic ObjLinkedTactic = new Plan_Campaign_Program_Tactic();

        //        var LinkedLineitemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanLineItemId == form.PlanLineItemId && id.IsDeleted == false).Select(id => id.LinkedLineItemId).FirstOrDefault();

        //        var TblTactic = db.Plan_Campaign_Program_Tactic.Where(id => (id.PlanTacticId == LinkedTacticId || id.LinkedTacticId == form.PlanTacticId || id.PlanTacticId == form.PlanTacticId) && id.IsDeleted == false).ToList();
        //        if (LinkedLineitemId != null)
        //        {
        //            Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanLineItemId == LinkedLineitemId && id.IsDeleted == false).ToList().FirstOrDefault();


        //        }
        //        if (LinkedTacticId != null && LinkedTacticId > 0)
        //        {

        //            ObjLinkedTactic = TblTactic.Where(id => id.PlanTacticId == LinkedTacticId).ToList().FirstOrDefault();
        //            //ObjLinkedTactic.Plan_Campaign_Program_Tactic_Cost.Count()
        //            yearDiff = ObjLinkedTactic.EndDate.Year - ObjLinkedTactic.StartDate.Year;
        //            isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
        //            cntr = 12 * yearDiff;
        //            for (int i = 1; i <= cntr; i++)
        //            {
        //                lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
        //            }
        //        }
        //        //else
        //        //{
        //        //    Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.LinkedLineItemId == form.PlanLineItemId && id.IsDeleted == false).ToList().FirstOrDefault();
        //        //    if (Lineitemobj != null)
        //        //    {

        //        //        LinkedLineitemId = Lineitemobj.PlanLineItemId;
        //        //        ObjLinkedTactic = Lineitemobj.Plan_Campaign_Program_Tactic;
        //        //        LinkedTacticId = ObjLinkedTactic.PlanTacticId;
        //        //    }

        //        //}

        //        //if (LinkedTacticId == null || Lineitemobj == null)
        //        //{
        //        //    ObjLinkedTactic = TblTactic.Where(Linkid => Linkid.LinkedTacticId == form.PlanTacticId && Linkid.IsDeleted == false).ToList().FirstOrDefault();
        //        //    if (ObjLinkedTactic != null)
        //        //    {
        //        //        LinkedTacticId = ObjLinkedTactic.PlanTacticId;
        //        //    }
        //        //    else
        //        //    {
        //        //        ObjLinkedTactic = TblTactic.Where(Linkid => Linkid.PlanTacticId == LinkedTacticId).ToList().FirstOrDefault();
        //        //    }

        //        //}


        //        //End
        //        using (MRPEntities mrp = new MRPEntities())
        //        {
        //            //using (var scope = new TransactionScope())
        //            {
        //                //// Get duplicate record to check duplication.
        //                var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
        //                               where pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(form.PlanLineItemId) && pcptl.IsDeleted.Equals(false)
        //                               && pcptl.PlanTacticId == form.PlanTacticId
        //                               select pcptl).FirstOrDefault();
        //                //Check duplicate for Linked LineItem 
        //                //Added By Komal Rawal for PL ticket #1853
        //                //var LinkedLi = new Plan_Campaign_Program_Tactic_LineItem();
        //                //if (LinkedTacticId != null)
        //                //{
        //                //    LinkedLi = (from pcptli in db.Plan_Campaign_Program_Tactic_LineItem
        //                //                where pcptli.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcptli.PlanLineItemId != LinkedLineitemId && pcptli.IsDeleted.Equals(false)
        //                //               && pcptli.PlanTacticId == LinkedTacticId.Value
        //                //                select pcptli).FirstOrDefault();

        //                //}
        //                //else
        //                //{
        //                //    LinkedLi = null;
        //                //}
        //                //End

        //                //// If duplicate record exist then return dupication message.
        //                if (pcptvar != null)
        //                {
        //                    string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
        //                    string strduplicatedlinkedlineitem = "";
        //                    //if (LinkedLi != null)
        //                    //{
        //                    //    strduplicatedlinkedlineitem = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()] + " in the linkedtactic");    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
        //                    //}
        //                    return Json(new { IsSaved = false, msg = strDuplicateMessage, strmsg = strduplicatedlinkedlineitem, JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    //if (LinkedTacticId != null)
        //                    //{
        //                    //    Plan_Campaign_Program_Tactic_LineItem objLinkedLineitem = Lineitemobj;
        //                    //    double Linkdiffcost = 0;
        //                    //    if (!form.IsOtherLineItem)
        //                    //    {
        //                    //        objLinkedLineitem.Title = title;
        //                    //        if (objLinkedLineitem.Cost != form.Cost)
        //                    //        {
        //                    //            Linkdiffcost = form.Cost - UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
        //                    //            objLinkedLineitem.Cost = form.Cost;
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            objLinkedLineitem.Cost = UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
        //                    //        }
        //                    //    }

        //                    //    objLinkedLineitem.ModifiedBy = Sessions.User.UserId;
        //                    //    objLinkedLineitem.ModifiedDate = DateTime.Now;
        //                    //    db.Entry(objLinkedLineitem).State = EntityState.Modified;
        //                    //    int Linkedresult;

        //                    //    Linkedresult = Common.InsertChangeLog(planid, null, objLinkedLineitem.PlanLineItemId, objLinkedLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
        //                    //    Linkedresult = db.SaveChanges();
        //                    //}
        //                    Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(form.PlanLineItemId));
        //                    double diffcost = 0;
        //                    if (!form.IsOtherLineItem)
        //                    {
        //                        objLineitem.Title = title;
        //                        if (objLineitem.Cost != form.Cost)
        //                        {
        //                            diffcost = form.Cost - UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
        //                            objLineitem.Cost = form.Cost;
        //                        }
        //                        else
        //                        {
        //                            objLineitem.Cost = UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
        //                        }
        //                    }

        //                    objLineitem.ModifiedBy = Sessions.User.UserId;
        //                    objLineitem.ModifiedDate = DateTime.Now;
        //                    db.Entry(objLineitem).State = EntityState.Modified;
        //                    int result;

        //                    result = Common.InsertChangeLog(planid, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
        //                    result = db.SaveChanges();

        //                    if (!form.IsOtherLineItem)
        //                    {
        //                        var objOtherLineItem = tblTacticLineItem.FirstOrDefault(l => l.LineItemTypeId == null);
        //                        double totalLoneitemCost = tblTacticLineItem.Where(l => l.LineItemTypeId != null && l.PlanLineItemId != form.PlanLineItemId && l.IsDeleted == false).ToList().Sum(l => l.Cost) + objLineitem.Cost;
        //                        double LinkedtotalLineitemCost = 0;
        //                        if (LinkedLineitemId != null)
        //                        {
        //                            //// Calculate TotalLineItemCost.
        //                            LinkedtotalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
        //                        }
        //                        //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem.  
        //                        if (objOtherLineItem == null)
        //                        {
        //                            //if (LinkedTacticId != null)
        //                            //{
        //                            //    Plan_Campaign_Program_Tactic_LineItem objLinkedNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
        //                            //    objLinkedNewLineitem.PlanTacticId = Convert.ToInt32(LinkedTacticId);
        //                            //    objLinkedNewLineitem.Title = Common.LineItemTitleDefault + ObjLinkedTactic.Title;
        //                            //    if (ObjLinkedTactic.Cost > LinkedtotalLineitemCost)
        //                            //    {
        //                            //        objLinkedNewLineitem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
        //                            //    }
        //                            //    else
        //                            //    {
        //                            //        objLinkedNewLineitem.Cost = 0;
        //                            //    }
        //                            //    objLinkedNewLineitem.Description = string.Empty;
        //                            //    objLinkedNewLineitem.CreatedBy = Sessions.User.UserId;
        //                            //    objLinkedNewLineitem.CreatedDate = DateTime.Now;
        //                            //    db.Entry(objLinkedNewLineitem).State = EntityState.Added;
        //                            //    db.SaveChanges();
        //                            //}
        //                            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
        //                            objNewLineitem.PlanTacticId = form.PlanTacticId;
        //                            objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
        //                            if (objTactic.Cost > totalLoneitemCost)
        //                            {
        //                                objNewLineitem.Cost = objTactic.Cost - totalLoneitemCost;
        //                            }
        //                            else
        //                            {
        //                                objNewLineitem.Cost = 0;
        //                            }
        //                            objNewLineitem.Description = string.Empty;
        //                            objNewLineitem.CreatedBy = Sessions.User.UserId;
        //                            objNewLineitem.CreatedDate = DateTime.Now;
        //                            db.Entry(objNewLineitem).State = EntityState.Added;
        //                            db.SaveChanges();
        //                        }
        //                        else
        //                        {
        //                            //if (LinkedTacticId != null)
        //                            //{
        //                            //    var objLinkedOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId == null);
        //                            //    objLinkedOtherLineItem.IsDeleted = false;
        //                            //    if (objLinkedOtherLineItem.Cost > LinkedtotalLineitemCost)
        //                            //    {
        //                            //        objLinkedOtherLineItem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
        //                            //    }
        //                            //    else
        //                            //    {
        //                            //        objLinkedOtherLineItem.Cost = 0;
        //                            //    }
        //                            //    db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
        //                            //}
        //                            objOtherLineItem.IsDeleted = false;
        //                            if (objTactic.Cost > totalLoneitemCost)
        //                            {
        //                                objOtherLineItem.Cost = objTactic.Cost - totalLoneitemCost;
        //                            }
        //                            else
        //                            {
        //                                objOtherLineItem.Cost = 0;
        //                                objOtherLineItem.IsDeleted = true;
        //                            }
        //                            db.Entry(objOtherLineItem).State = EntityState.Modified;
        //                            db.SaveChanges();
        //                        }

        //                    }

        //                    if (result >= 1)
        //                    {
        //                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> PrevAllocationList = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(c => c.PlanLineItemId == form.PlanLineItemId).Select(c => c).ToList();

        //                        //// Process for Monthly budget values.
        //                        // Change by Nishant sheth
        //                        // Desc :: #1765 - to replace the lenth of array to allocated by
        //                        if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                        {
        //                            bool isExists;
        //                            Plan_Campaign_Program_Tactic_LineItem_Cost updatePlanTacticBudget, objlineItemCost;
        //                            double newValue = 0;
        //                            for (int i = 0; i < arrCostInputValues.Length; i++)
        //                            {
        //                                // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
        //                                isExists = false;
        //                                if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                                {
        //                                    updatePlanTacticBudget = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                    updatePlanTacticBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
        //                                    if (updatePlanTacticBudget != null)
        //                                    {
        //                                        if (arrCostInputValues[i] != "")
        //                                        {
        //                                            newValue = Convert.ToDouble(arrCostInputValues[i]);
        //                                            if (updatePlanTacticBudget.Value != newValue)
        //                                            {
        //                                                updatePlanTacticBudget.Value = newValue;
        //                                                db.Entry(updatePlanTacticBudget).State = EntityState.Modified;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            db.Entry(updatePlanTacticBudget).State = EntityState.Deleted;
        //                                        }
        //                                        isExists = true;
        //                                    }
        //                                }
        //                                if (!isExists && arrCostInputValues[i] != "")
        //                                {
        //                                    // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
        //                                    objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                    objlineItemCost.PlanLineItemId = form.PlanLineItemId;
        //                                    objlineItemCost.Period = PeriodChar + (i + 1);
        //                                    objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
        //                                    objlineItemCost.CreatedBy = Sessions.User.UserId;
        //                                    objlineItemCost.CreatedDate = DateTime.Now;
        //                                    db.Entry(objlineItemCost).State = EntityState.Added;
        //                                }
        //                            }
        //                        }
        //                        // Change by Nishant sheth
        //                        // Desc :: #1765 - to replace the lenth of array to allocated by
        //                        else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower())  //// Process for Quarterly budget values.
        //                        {
        //                            int QuarterCnt = 1, j = 1;
        //                            int m = 0;
        //                            ////QurterList which contains list of month as per quarter. e.g. for Q1, list is Y1,Y2 and Y3

        //                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> thisQuartersMonthList;
        //                            Plan_Campaign_Program_Tactic_LineItem_Cost thisQuarterFirstMonthBudget, objlineItemCost;
        //                            for (int k = 1; k <= (YearDiffrence + 1); k++)
        //                            {
        //                                bool isExists;
        //                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
        //                                for (int i = m; i < (4 * k); i++)
        //                                {
        //                                    if ((i + 1) % 4 == 0)
        //                                    {
        //                                        m = i + 1;
        //                                    }
        //                                    // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
        //                                    isExists = false;
        //                                    if (PrevAllocationList != null && PrevAllocationList.Count > 0)
        //                                    {
        //                                        thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
        //                                        thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                        thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
        //                                        thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

        //                                        if (thisQuarterFirstMonthBudget != null)
        //                                        {
        //                                            if (arrCostInputValues[i] != "")
        //                                            {
        //                                                thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(quartBudget => quartBudget.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(quartBudget => quartBudget.Value);
        //                                                thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
        //                                                newValue = Convert.ToDouble(arrCostInputValues[i]);

        //                                                if (thisQuarterTotalBudget != newValue)
        //                                                {
        //                                                    BudgetDiff = newValue - thisQuarterTotalBudget;
        //                                                    if (BudgetDiff > 0)
        //                                                    {
        //                                                        thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
        //                                                        db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        j = 1;
        //                                                        while (BudgetDiff < 0)
        //                                                        {
        //                                                            if (thisQuarterFirstMonthBudget != null)
        //                                                            {
        //                                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

        //                                                                if (BudgetDiff <= 0)
        //                                                                    thisQuarterFirstMonthBudget.Value = 0;
        //                                                                else
        //                                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;

        //                                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
        //                                                            }
        //                                                            if ((QuarterCnt + j) <= (QuarterCnt + 2))
        //                                                            {
        //                                                                thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
        //                                                            }

        //                                                            j = j + 1;
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
        //                                            }
        //                                            isExists = true;
        //                                        }
        //                                    }
        //                                    //// If record does not exist then insert new record to table.
        //                                    if (!isExists && arrCostInputValues[i] != "")
        //                                    {
        //                                        // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
        //                                        objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                        objlineItemCost.PlanLineItemId = form.PlanLineItemId;
        //                                        objlineItemCost.Period = PeriodChar + QuarterCnt;
        //                                        objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
        //                                        objlineItemCost.CreatedBy = Sessions.User.UserId;
        //                                        objlineItemCost.CreatedDate = DateTime.Now;
        //                                        db.Entry(objlineItemCost).State = EntityState.Added;
        //                                    }
        //                                    QuarterCnt = QuarterCnt + 3;
        //                                }
        //                            }
        //                        }

        //                        db.SaveChanges();
        //                        objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanLineItemId == form.PlanLineItemId).FirstOrDefault();
        //                        List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
        //                        tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();

        //                        tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == form.PlanTacticId).ToList();
        //                        List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
        //                        var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
        //                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

        //                        //// Check when monthly planned cost of tactic is lower than total monthly planned cost of line items than update monthly planned cost of tactic
        //                        List<int> lineItemIds = tblTacticLineItem.Where(a => a.IsDeleted == false && a.PlanLineItemId != form.PlanLineItemId).Select(a => a.PlanLineItemId).ToList();

        //                        ////list of Total monthly planned cost
        //                        var lstMonthlyLineItemCost = lineitemcostlist
        //                            .Where(lineCost => lineCost.PlanLineItemId != form.PlanLineItemId)
        //                            .GroupBy(lineCost => lineCost.Period)
        //                            .Select(lineCost => new
        //                            {
        //                                Period = lineCost.Key,
        //                                Cost = lineCost.Sum(b => b.Value)
        //                            }).ToList();

        //                        ////List of monthly plaaned cost of tactic
        //                        var lstMonthlyTacticCost = tacticostslist.ToList();
        //                        double TotalTacticCost = lstMonthlyTacticCost.Sum(a => a.Value);

        //                        if (diffcost > 0)
        //                        {
        //                            int startmonth = objTactic.StartDate.Month;
        //                            if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
        //                            {
        //                                double lineitemstartmonthcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
        //                                objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
        //                            }
        //                            else
        //                            {
        //                                Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                objlineitemCost.PlanLineItemId = form.PlanLineItemId;
        //                                objlineitemCost.Period = PeriodChar + startmonth;
        //                                objlineitemCost.Value = diffcost;
        //                                objlineitemCost.CreatedBy = Sessions.User.UserId;
        //                                objlineitemCost.CreatedDate = DateTime.Now;
        //                                db.Entry(objlineitemCost).State = EntityState.Added;
        //                            }
        //                            if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                            {
        //                                if (arrCostInputValues[startmonth - 1] != "")
        //                                {
        //                                    arrCostInputValues[startmonth - 1] = (Convert.ToInt32(arrCostInputValues[startmonth - 1]) + diffcost).ToString();
        //                                }
        //                            }
        //                            else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower())
        //                            {
        //                                if (arrCostInputValues[(startmonth - 1) / 3] != "")
        //                                {
        //                                    arrCostInputValues[(startmonth - 1) / 3] = (Convert.ToInt32(arrCostInputValues[(startmonth - 1) / 3]) + diffcost).ToString();
        //                                }
        //                            }
        //                        }
        //                        else if (diffcost < 0)
        //                        {
        //                            int endmonth = (12 * (YearDiffrence + 1));
        //                            diffcost = Math.Abs(diffcost);
        //                            double objlineitemcostarr = 0;
        //                            while (diffcost > 0 && endmonth != 0)
        //                            {
        //                                objlineitemcostarr = 0;
        //                                if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
        //                                {
        //                                    if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                                    {
        //                                        if (arrCostInputValues[endmonth - 1] != "")
        //                                        {
        //                                            objlineitemcostarr = Convert.ToInt32(arrCostInputValues[endmonth - 1]);
        //                                            if (objlineitemcostarr > diffcost)
        //                                            {
        //                                                arrCostInputValues[endmonth - 1] = (objlineitemcostarr - diffcost).ToString();
        //                                            }
        //                                            else
        //                                            {
        //                                                arrCostInputValues[endmonth - 1] = (0).ToString();
        //                                            }
        //                                        }
        //                                    }
        //                                    else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower())
        //                                    {
        //                                        if (arrCostInputValues[(endmonth - 1) / 3] != "")
        //                                        {
        //                                            objlineitemcostarr = Convert.ToInt32(arrCostInputValues[(endmonth - 1) / 3]);
        //                                            if (objlineitemcostarr > diffcost)
        //                                            {
        //                                                arrCostInputValues[(endmonth - 1) / 3] = (objlineitemcostarr - diffcost).ToString();
        //                                            }
        //                                            else
        //                                            {
        //                                                arrCostInputValues[(endmonth - 1) / 3] = (0).ToString();
        //                                            }
        //                                        }
        //                                    }
        //                                    double objlineitemcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
        //                                    if (objlineitemcost > diffcost)
        //                                    {
        //                                        objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objlineitemcost - diffcost;
        //                                        diffcost = 0;
        //                                    }
        //                                    else
        //                                    {
        //                                        objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = 0;
        //                                        diffcost = diffcost - objlineitemcost;
        //                                    }
        //                                }
        //                                if (endmonth > 0)
        //                                {
        //                                    endmonth -= 1;
        //                                }
        //                            }
        //                        }

        //                        ////check budget allocation type e.g. month,Quarter etc
        //                        if (AllocatedBy == Enums.PlanAllocatedBy.months.ToString().ToLower())
        //                        {
        //                            string period = string.Empty;
        //                            double monthlyTotalLineItemCost = 0, monthlyTotalTacticCost = 0;
        //                            for (int i = 0; i < (12 * (YearDiffrence + 1)); i++)
        //                            {
        //                                if (arrCostInputValues[i] != "")
        //                                {
        //                                    period = PeriodChar + (i + 1).ToString();
        //                                    monthlyTotalLineItemCost = lstMonthlyLineItemCost.Where(lineCost => lineCost.Period == period).FirstOrDefault() == null ? 0 : lstMonthlyLineItemCost.Where(lineCost => lineCost.Period == period).FirstOrDefault().Cost;
        //                                    monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(arrCostInputValues[i]);
        //                                    monthlyTotalTacticCost = lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).FirstOrDefault() == null ? 0 : lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).FirstOrDefault().Value;

        //                                    bool isAddMode = false;

        //                                    if (lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().Count() <= 0)
        //                                    {
        //                                        isAddMode = true;
        //                                    }

        //                                    if (monthlyTotalLineItemCost > monthlyTotalTacticCost)
        //                                    {
        //                                        if (isAddMode)
        //                                        {
        //                                            Plan_Campaign_Program_Tactic_Cost objTacti_Cost = new Plan_Campaign_Program_Tactic_Cost();
        //                                            objTacti_Cost.PlanTacticId = form.PlanTacticId;
        //                                            objTacti_Cost.Period = period;
        //                                            objTacti_Cost.CreatedBy = Sessions.User.UserId;
        //                                            objTacti_Cost.CreatedDate = DateTime.Now;
        //                                            objTacti_Cost.Value = monthlyTotalLineItemCost;
        //                                            db.Entry(objTacti_Cost).State = EntityState.Added;
        //                                        }
        //                                        else
        //                                        {
        //                                            lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = monthlyTotalLineItemCost; db.Entry(_tacCost).State = EntityState.Modified; });
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString().ToLower())
        //                        {
        //                            int QuarterCnt = 1, periodCount = 0;
        //                            List<string> QuarterList;
        //                            double monthlyTotalLineItemCost = 0, monthlyTotalTacticCost = 0, diffCost = 0, tacticCost = 0;
        //                            string period = string.Empty;
        //                            for (int i = 0; i < (4 * (YearDiffrence + 1)); i++)
        //                            {
        //                                if (arrCostInputValues[i] != "")
        //                                {
        //                                    ////QurterList which contains list of month as per quarter. e.g. for Q1, list is Y1,Y2 and Y3
        //                                    QuarterList = new List<string>();
        //                                    for (int J = 0; J < 3; J++)
        //                                    {
        //                                        QuarterList.Add(PeriodChar + (QuarterCnt + J).ToString());
        //                                    }
        //                                    //string period = PeriodChar + QuarterCnt.ToString();
        //                                    monthlyTotalLineItemCost = lstMonthlyLineItemCost.Where(lineCost => QuarterList.Contains(lineCost.Period)).ToList().Sum(lineCost => lineCost.Cost);
        //                                    monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(arrCostInputValues[i]);
        //                                    monthlyTotalTacticCost = lstMonthlyTacticCost.Where(_tacCost => QuarterList.Contains(_tacCost.Period)).ToList().Sum(_tacCost => _tacCost.Value);

        //                                    bool isAddMode = false;


        //                                    if (monthlyTotalLineItemCost > monthlyTotalTacticCost)
        //                                    {
        //                                        period = QuarterList[0].ToString();
        //                                        diffCost = monthlyTotalLineItemCost - monthlyTotalTacticCost;
        //                                        if (lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().Count() <= 0)
        //                                        {
        //                                            isAddMode = true;
        //                                        }
        //                                        if (diffCost >= 0)
        //                                        {
        //                                            if (isAddMode)
        //                                            {
        //                                                Plan_Campaign_Program_Tactic_Cost objTacti_Cost = new Plan_Campaign_Program_Tactic_Cost();
        //                                                objTacti_Cost.PlanTacticId = form.PlanTacticId;
        //                                                objTacti_Cost.Period = period;
        //                                                objTacti_Cost.CreatedBy = Sessions.User.UserId;
        //                                                objTacti_Cost.CreatedDate = DateTime.Now;
        //                                                objTacti_Cost.Value = diffCost;
        //                                                db.Entry(objTacti_Cost).State = EntityState.Added;
        //                                            }
        //                                            else
        //                                            {
        //                                                lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = _tacCost.Value + diffCost; db.Entry(_tacCost).State = EntityState.Modified; });
        //                                            }
        //                                        }
        //                                        periodCount = 0;
        //                                        ////If cost diffrence is lower than 0 than reduce it from quarter in series of 1st month of quarter,2nd month of quarter...
        //                                        while (diffCost < 0)
        //                                        {
        //                                            period = QuarterList[periodCount].ToString();
        //                                            tacticCost = lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().Sum(_tacCost => _tacCost.Value);
        //                                            if ((diffCost + tacticCost) >= 0)
        //                                            {
        //                                                lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = _tacCost.Value + diffCost; db.Entry(_tacCost).State = EntityState.Modified; });
        //                                            }
        //                                            else
        //                                            {
        //                                                lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = 0; db.Entry(_tacCost).State = EntityState.Modified; });
        //                                            }

        //                                            diffCost = diffCost + tacticCost;
        //                                            periodCount = periodCount + 1;
        //                                        }
        //                                    }
        //                                }

        //                                QuarterCnt = QuarterCnt + 3;
        //                            }
        //                        }


        //                        ////update tactic cost as per its monthly total planned cost
        //                        objTactic.Cost = lstMonthlyTacticCost.Sum(a => a.Value);
        //                        db.SaveChanges();
        //                        //Added By Komal
        //                        if (LinkedLineitemId != null)
        //                        {
        //                            #region "Update Linked Tactic Budget data"
        //                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemData = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(id => (id.PlanLineItemId == form.PlanLineItemId) || (id.PlanLineItemId == LinkedLineitemId)).ToList();
        //                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == form.PlanLineItemId).ToList();
        //                            List<Plan_Campaign_Program_Tactic_Cost> linkedSrcTacticCostData = TblTactic.FirstOrDefault(id => id.PlanTacticId == form.PlanTacticId).Plan_Campaign_Program_Tactic_Cost.ToList();
        //                            List<Plan_Campaign_Program_Tactic_Cost> linkedTacticCostData = TblTactic.FirstOrDefault(id => id.PlanTacticId == LinkedTacticId).Plan_Campaign_Program_Tactic_Cost.ToList();
        //                            //List<Plan_Campaign_Program_Tactic_Cost> tblTacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId || id.PlanTacticId == LinkedTacticId).ToList();
        //                            //List<Plan_Campaign_Program_Tactic_Cost> TacticData = db.Plan_Campaign_Program_Tactic_Cost.Where(id => id.PlanTacticId == form.PlanTacticId).ToList();

        //                            if (lstLineItemData != null && lstLineItemData.Count > 0)
        //                            {
        //                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> linkedCostData = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
        //                                Plan_Campaign_Program_Tactic_LineItem_Cost objlinkedCost = null;
        //                                if (isMultiYearlinkedTactic)
        //                                {
        //                                    linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId && lstLinkedPeriods.Contains(line.Period)).ToList();
        //                                    if (linkedCostData != null && linkedCostData.Count > 0)
        //                                        linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                    foreach (Plan_Campaign_Program_Tactic_LineItem_Cost cost in lstLineItemData)
        //                                    {
        //                                        string orgPeriod = cost.Period;
        //                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
        //                                        int NumPeriod = int.Parse(numPeriod);

        //                                        objlinkedCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                        objlinkedCost.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
        //                                        objlinkedCost.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
        //                                        objlinkedCost.Value = cost.Value;
        //                                        objlinkedCost.CreatedDate = cost.CreatedDate;
        //                                        objlinkedCost.CreatedBy = cost.CreatedBy;
        //                                        db.Entry(objlinkedCost).State = EntityState.Added;
        //                                    }
        //                                    // Delete old budget data.


        //                                    // Tactic Cost Data

        //                                    //linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);

        //                                    linkedTacticCostData = linkedTacticCostData.Where(line => lstLinkedPeriods.Contains(line.Period)).ToList();
        //                                    if (linkedTacticCostData != null && linkedTacticCostData.Count > 0)
        //                                        linkedTacticCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                    Plan_Campaign_Program_Tactic_Cost objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
        //                                    foreach (Plan_Campaign_Program_Tactic_Cost cost in linkedSrcTacticCostData)
        //                                    {
        //                                        string orgPeriod = cost.Period;
        //                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
        //                                        int NumPeriod = int.Parse(numPeriod);

        //                                        objlinkedTacCost = new Plan_Campaign_Program_Tactic_Cost();
        //                                        objlinkedTacCost.PlanTacticId = LinkedTacticId.Value;
        //                                        objlinkedTacCost.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
        //                                        objlinkedTacCost.Value = cost.Value;
        //                                        objlinkedTacCost.CreatedDate = cost.CreatedDate;
        //                                        objlinkedTacCost.CreatedBy = cost.CreatedBy;
        //                                        db.Entry(objlinkedTacCost).State = EntityState.Added;
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    linkedCostData = tblLineItemData.Where(line => line.PlanLineItemId == LinkedLineitemId).ToList();
        //                                    if (linkedCostData != null && linkedCostData.Count > 0)
        //                                        linkedCostData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                    foreach (Plan_Campaign_Program_Tactic_LineItem_Cost cost in lstLineItemData)
        //                                    {
        //                                        string orgPeriod = cost.Period;
        //                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
        //                                        int NumPeriod = int.Parse(numPeriod);
        //                                        if (NumPeriod > 12)
        //                                        {
        //                                            int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
        //                                            int div = NumPeriod / 12;    // In case of 24, Y12.
        //                                            if (rem > 0 || div > 1)
        //                                            {
        //                                                objlinkedCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
        //                                                objlinkedCost.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
        //                                                objlinkedCost.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());     // (12*1)+3 = 15 => For March(Y15) month.
        //                                                objlinkedCost.Value = cost.Value;
        //                                                objlinkedCost.CreatedDate = cost.CreatedDate;
        //                                                objlinkedCost.CreatedBy = cost.CreatedBy;
        //                                                db.Entry(objlinkedCost).State = EntityState.Added;
        //                                            }
        //                                        }
        //                                    }

        //                                }

        //                                db.SaveChanges();

        //                                //List<Plan_Campaign_Program_Tactic_LineItem_Cost> linkedLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == LinkedLineitemId).ToList();
        //                                //linkedLineItemData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                //lstLineItemData.ForEach(bdgt => { bdgt.PlanLineItemId = Convert.ToInt32(LinkedLineitemId); db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_LineItem_Cost.Add(bdgt); });


        //                                //List<Plan_Campaign_Program_Tactic_Cost> linkedTacticData = tblTacticData.Where(id => id.PlanTacticId == LinkedTacticId).ToList();
        //                                //linkedTacticData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
        //                                //TacticData.ForEach(bdgt => { bdgt.PlanTacticId = Convert.ToInt32(LinkedTacticId); db.Entry(bdgt).State = EntityState.Added; db.Plan_Campaign_Program_Tactic_Cost.Add(bdgt); });

        //                                //db.SaveChanges();
        //                            }
        //                            #endregion
        //                        }

        //                        if (LinkedTacticId != null)
        //                        {
        //                            //  Plan_Campaign_Program_Tactic_LineItem objLinkedLineitem = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => line.PlanLineItemId == LinkedLineitemId).FirstOrDefault();
        //                            Lineitemobj.Cost = Lineitemobj.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(cost => cost.Value).Sum();
        //                            db.Entry(Lineitemobj).State = EntityState.Modified;

        //                            if (!form.IsOtherLineItem)
        //                            {
        //                                var objOtherLineItem = tblTacticLineItem.FirstOrDefault(l => l.LineItemTypeId == null);
        //                                double totalLoneitemCost = tblTacticLineItem.Where(l => l.LineItemTypeId != null && l.PlanLineItemId != form.PlanLineItemId && l.IsDeleted == false).ToList().Sum(l => l.Cost) + objLineitem.Cost;
        //                                double LinkedtotalLineitemCost = 0;
        //                                if (LinkedLineitemId != null)
        //                                {
        //                                    //// Calculate TotalLineItemCost.
        //                                    LinkedtotalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
        //                                }
        //                                //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem.  
        //                                if (objOtherLineItem == null)
        //                                {
        //                                    if (LinkedTacticId != null)
        //                                    {
        //                                        Plan_Campaign_Program_Tactic_LineItem objLinkedNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
        //                                        objLinkedNewLineitem.PlanTacticId = Convert.ToInt32(LinkedTacticId);
        //                                        objLinkedNewLineitem.Title = Common.LineItemTitleDefault + ObjLinkedTactic.Title;
        //                                        if (ObjLinkedTactic.Cost > LinkedtotalLineitemCost)
        //                                        {
        //                                            objLinkedNewLineitem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
        //                                        }
        //                                        else
        //                                        {
        //                                            objLinkedNewLineitem.Cost = 0;
        //                                        }
        //                                        objLinkedNewLineitem.Description = string.Empty;
        //                                        objLinkedNewLineitem.CreatedBy = Sessions.User.UserId;
        //                                        objLinkedNewLineitem.CreatedDate = DateTime.Now;
        //                                        db.Entry(objLinkedNewLineitem).State = EntityState.Added;
        //                                        db.SaveChanges();
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    if (LinkedTacticId != null)
        //                                    {
        //                                        var objLinkedOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId == null);
        //                                        if (objLinkedOtherLineItem != null)
        //                                        {
        //                                            objLinkedOtherLineItem.IsDeleted = false;
        //                                            if (objLinkedOtherLineItem.Cost > LinkedtotalLineitemCost)
        //                                            {
        //                                                objLinkedOtherLineItem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
        //                                            }
        //                                            else
        //                                            {
        //                                                objLinkedOtherLineItem.Cost = 0;
        //                                                objLinkedOtherLineItem.IsDeleted = true;
        //                                            }

        //                                            db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
        //                                        }
        //                                    }


        //                                }
        //                                db.SaveChanges();
        //                            }
        //                        }

        //                        //End

        //                        //scope.Complete();
        //                        string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
        //                        return Json(new
        //                        {
        //                            IsSaved = true,
        //                            CamapignId = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId,
        //                            ProgramId = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanProgramId,
        //                            TacticId = objLineitem.PlanTacticId,
        //                            PlanLineItemId = form.PlanLineItemId,
        //                            msg = strMessage,
        //                            JsonRequestBehavior.AllowGet
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }

        //    return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        //}


        #endregion

        #region "Actuals Tab for LineItem"
        /// <summary>
        /// Added By: Viral Kadiya on 11/11/2014.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="id">Plan line item Id.</param>
        /// <returns>Returns PartialView Result of line item actuals Value.</returns>
        public ActionResult LoadActualsLineItem(int id)
        {
            try
            {
                ViewBag.ParentTacticStatus = GetTacticStatusByPlanLineItemId(id);

                //// Get LineItem data by Id.
                Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
                bool isOtherLineItem = true;

                //// Set isOtherLineItem flag.
                if (pcptl != null && pcptl.LineItemTypeId != null)
                    isOtherLineItem = false;

                ViewBag.IsOtherLineItem = isOtherLineItem;

                if (pcptl != null)
                {


                    // Add by Nishant sheth
                    // Desc :: #1765 - to get the year diffrence between item start date and end date
                    ViewBag.YearDiffrence = Convert.ToInt32(Convert.ToInt32(pcptl.Plan_Campaign_Program_Tactic.EndDate.Year) - Convert.ToInt32(pcptl.Plan_Campaign_Program_Tactic.StartDate.Year));
                    ViewBag.StartYear = Convert.ToInt32(pcptl.Plan_Campaign_Program_Tactic.StartDate.Year);
                }

                return PartialView("_ActualLineitem");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Added By: Viral Kadiya on 11/11/2014.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="planlineitemid">Plan line item Id.</param>
        /// <returns>Returns Parent Tactic Status.</returns>
        public string GetTacticStatusByPlanLineItemId(int planlineitemid)
        {
            string strTacticStatus = string.Empty;
            try
            {
                //// Get Tactic status by PlanLineItemId.
                var lstPCPT = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                               join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                               where pcptl.PlanLineItemId == planlineitemid && pcpt.IsDeleted == false
                               select pcpt).FirstOrDefault();
                if (lstPCPT != null)
                    strTacticStatus = lstPCPT.Status;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strTacticStatus;
        }
        #endregion

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Create Campaign.
        /// </summary>
        /// <param name="id">Tactic Id</param>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public ActionResult createLineitem(int id)
        {
            int tacticId = id;
            var pcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId.Equals(tacticId) && _tactic.IsDeleted == false).FirstOrDefault();
            var objPlan = pcpt != null ? pcpt.Plan_Campaign_Program.Plan_Campaign.Plan : null;
            if (objPlan == null)
            {
                return null;
            }
            ViewBag.IsCreated = true;
            ViewBag.RedirectType = false;

            /// Get LineItemTypes List by ModelId for current PlanId.
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title, new AlphaNumericComparer()
                                select lit;
            foreach (var item in lineItemTypes)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.lineItemTypes = lineItemTypes;

            ViewBag.Owner = Sessions.User.FirstName + " " + Sessions.User.LastName;

            #region "Set data to Plan_Campaign_Program_Tactic_LineItemModel to pass into PartialView"
            Plan_Campaign_Program_Tactic_LineItemModel pc = new Plan_Campaign_Program_Tactic_LineItemModel();
            pc.PlanTacticId = tacticId;
            ViewBag.TacticTitle = HttpUtility.HtmlDecode(pcpt.Title);
            ViewBag.ProgramTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Title);
            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Plan_Campaign.Title);
            pc.StartDate = GetCurrentDateBasedOnPlan(false, objPlan.PlanId);
            pc.EndDate = GetCurrentDateBasedOnPlan(true, objPlan.PlanId);
            pc.Cost = 0;
            pc.AllocatedBy = objPlan.AllocatedBy;
            pc.IsOtherLineItem = false;
            pc.AllocatedBy = objPlan.AllocatedBy;
            pc.IsLineItemAddEdit = true;//modified by Rahul Shah on 17/03/2016 for PL #2032 
            pc.OwnerId = Sessions.User.UserId;
            #endregion
            //Added by Rahul Shah on 17/03/2016 for PL #2032 
            #region "Owner List"
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => !i.IsDeleted).ToList(); // PL #1532 Dashrath Prajapati
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);

                if (lstClientUsers.Count() > 0)
                {
                    //// Flag to indicate unavailability of web service.                   
                    ViewBag.IsServiceUnavailable = false;
                    string strUserList = string.Join(",", lstClientUsers);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId); //PL #1532 Dashrath Prajapati
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        pc.OwnerList = lstPreparedOwners.Select(u => new SelectListUser { Name = u.DisplayName, Id = u.UserId }).ToList();
                    }
                    else
                    {

                        pc.OwnerList = new List<SelectListUser>();
                    }
                }
                else
                {

                    pc.OwnerList = new List<SelectListUser>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.                  
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            #endregion
            return PartialView("_EditSetupLineitem", pc);
        }
        #endregion

        [HttpPost]
        public JsonResult SaveTitle(string ActivePopup, string title = "", string Id = "")
        {
            MRPEntities db = new MRPEntities();
            try
            {
                if (ActivePopup == "Tactic")
                {
                    int Tacticid = Convert.ToInt32(Id);
                    int linkedTacticId = 0;

                    //   List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Select(tac => tac).ToList();
                    var objpcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == Tacticid).FirstOrDefault();
                    int pid = objpcpt.PlanProgramId;
                    int cid = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == pid).Select(program => program.PlanCampaignId).FirstOrDefault();

                    #region "Retrieve linkedTactic"
                    linkedTacticId = (objpcpt != null && objpcpt.LinkedTacticId.HasValue) ? objpcpt.LinkedTacticId.Value : 0;
                    //if (linkedTacticId <= 0)
                    //{
                    //    var lnkPCPT = tblPlanTactic.Where(tac => tac.LinkedTacticId == Tacticid).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                    //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                    //}
                    #endregion
                    //// Get Tactic duplicate record.
                    var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                  join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                  join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                  where pcpt.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(Tacticid) && pcpt.IsDeleted.Equals(false)
                                  && pcp.PlanProgramId == objpcpt.PlanProgramId
                                  select pcp).FirstOrDefault();

                    //// Get Linked Tactic duplicate record.
                    Plan_Campaign_Program_Tactic dupLinkedTactic = null;
                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                    if (linkedTacticId > 0)
                    {
                        linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault();

                        dupLinkedTactic = (from pcpt in db.Plan_Campaign_Program_Tactic
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                           where pcpt.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(linkedTacticId) && pcpt.IsDeleted.Equals(false)
                                           && pcp.PlanProgramId == linkedTactic.PlanProgramId
                                           select pcpt).FirstOrDefault();
                    }

                    //// if duplicate record exist then return duplication message.
                    if (dupLinkedTactic != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.LinkedPlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else if (pcpvar != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else
                    {

                        Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(Tacticid)).FirstOrDefault();
                        pcpobj.Title = title;
                        db.Entry(pcpobj).State = EntityState.Modified;

                        #region "update linked Tactic"
                        if (linkedTacticId > 0)
                        {
                            Plan_Campaign_Program_Tactic lnkPCPT = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault();
                            lnkPCPT.Title = title;
                            db.Entry(lnkPCPT).State = EntityState.Modified;
                        }
                        #endregion

                        db.SaveChanges();
                        string strMessag = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                        return Json(new { IsDuplicate = false, redirect = Url.Action("LoadSetup", new { id = Tacticid }), Msg = strMessag, planTacticId = pcpobj.PlanTacticId, planCampaignId = cid, planProgramId = pid, tacticStatus = pcpobj.Status });
                    }
                }
                else if (ActivePopup == "LineItem")
                {
                    int PlanLineItemId = int.Parse(Id);
                    int tid = db.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanLineItemId == PlanLineItemId).FirstOrDefault().PlanTacticId;
                    int cid = 0;
                    int pid = 0;
                    //int linkedTacticId = 0;
                    //  List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Select(tac => tac).ToList();

                    var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == tid);
                    if (objTactic != null)
                    {
                        cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                        pid = objTactic.PlanProgramId;

                    }
                    else
                    {
                        objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == PlanLineItemId);

                        cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                        pid = objTactic.PlanProgramId;
                    }
                    //   List<Plan_Campaign_Program_Tactic_LineItem> tblPlanLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => line.IsDeleted == false).ToList();
                    //// Get Duplicate record to check duplication.
                    var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                   join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                   join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                   join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                   where pcptl.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                                   && pcpt.PlanTacticId == tid
                                   select pcpt).FirstOrDefault();

                    Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(PlanLineItemId));

                    #region "Retrieve Linked Plan Line Item"
                    int linkedLineItemId = 0;
                    linkedLineItemId = (objLineitem != null && objLineitem.LinkedLineItemId.HasValue) ? objLineitem.LinkedLineItemId.Value : 0;
                    if (linkedLineItemId <= 0)
                    {
                        var lnkPlanLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(tac => tac.LinkedLineItemId == objLineitem.PlanLineItemId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                        linkedLineItemId = lnkPlanLineItem != null ? lnkPlanLineItem.PlanLineItemId : 0;
                    }
                    #endregion

                    Plan_Campaign_Program_Tactic duplTactic = null;

                    #region "Retrieve linkedTactic"
                    int linkedTacticId = 0;
                    linkedTacticId = (objTactic != null && objTactic.LinkedTacticId.HasValue) ? objTactic.LinkedTacticId.Value : 0;
                    //if (linkedTacticId <= 0)
                    //{
                    //    var lnkPCPT = tblPlanTactic.Where(tac => tac.LinkedTacticId == tid).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                    //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                    //}
                    #endregion

                    //// Get Duplicate record to check duplication.
                    if (linkedLineItemId > 0)
                    {
                        duplTactic = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                      join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                      join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                      join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                      where pcptl.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(linkedLineItemId) && pcptl.IsDeleted.Equals(false)
                                                      && pcpt.PlanTacticId == linkedTacticId
                                      select pcpt).FirstOrDefault();
                    }

                    //// if duplicate record exist then return Duplicate message.
                    if (duplTactic != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()] + " in the linkedtactic");
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else if (pcptvar != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else
                    {
                        objLineitem.Title = title;
                        objLineitem.ModifiedBy = Sessions.User.UserId;
                        objLineitem.ModifiedDate = DateTime.Now;
                        db.Entry(objLineitem).State = EntityState.Modified;

                        // Update linked LineItem Title.
                        if (linkedLineItemId > 0)
                        {
                            Plan_Campaign_Program_Tactic_LineItem objLinkedLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(line => line.PlanLineItemId.Equals(linkedLineItemId));
                            objLinkedLineitem.Title = title;
                            objLinkedLineitem.ModifiedBy = Sessions.User.UserId;
                            objLinkedLineitem.ModifiedDate = objLineitem.ModifiedDate;
                            db.Entry(objLinkedLineitem).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                        string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);
                        return Json(new { IsDuplicate = false, msg = strMessage, planLineitemID = PlanLineItemId, planCampaignID = cid, planProgramID = pid, planTacticID = tid });

                    }
                }
                else if (ActivePopup == "Program")
                {

                    int Planprogramid = Convert.ToInt32(Id);
                    int PlanCampaignid = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == Planprogramid).Select(program => program.PlanCampaignId).FirstOrDefault();

                    //// Get duplicate record.
                    var pcpvar = (from pcp in db.Plan_Campaign_Program
                                  join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                  where pcp.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(Planprogramid) && pcp.IsDeleted.Equals(false)
                                  && pc.PlanCampaignId == PlanCampaignid
                                  select pcp).FirstOrDefault();

                    //Get program id
                    //var Planprogramid = (from pcp in db.Plan_Campaign_Program
                    //              join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                    //              where pcp.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && pcp.IsDeleted.Equals(false)
                    //               && pcp.PlanCampaignId == PlanCampaignid
                    //              select pcp.PlanProgramId).FirstOrDefault();





                    //// if duplicate record exist then return with duplication message.
                    if (pcpvar != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else
                    {
                        #region "Update record to Plan_Campaign_Program table"
                        Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(Planprogramid)).FirstOrDefault();
                        pcpobj.Title = title;
                        Guid oldOwnerId = pcpobj.CreatedBy;
                        pcpobj.ModifiedBy = Sessions.User.UserId;
                        pcpobj.ModifiedDate = DateTime.Now;
                        db.Entry(pcpobj).State = EntityState.Modified;
                        db.SaveChanges();
                        string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                        return Json(new { IsSaved = true, Msg = strMessage, campaignID = PlanCampaignid }, JsonRequestBehavior.AllowGet);
                        #endregion
                    }
                }
                else if (ActivePopup == "Campaign")
                {
                    int Campaignid = Convert.ToInt32(Id);
                    //// Get PlanId by PlanCampaignId.
                    var planId = db.Plan_Campaign.Where(_plan => _plan.PlanCampaignId.Equals(Campaignid)).FirstOrDefault().PlanId;
                    //// check for duplicate record.
                    var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(planId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(Campaignid))).FirstOrDefault();

                    //// if record exist then return with duplication message.
                    if (pc != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { IsDuplicate = true, msg = strDuplicateMessage });
                    }
                    else
                    {
                        #region "Update record into Plan_Campaign table"
                        Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(Campaignid) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
                        pcobj.Title = title;
                        Guid oldOwnerId = pcobj.CreatedBy;
                        pcobj.ModifiedBy = Sessions.User.UserId;
                        pcobj.ModifiedDate = DateTime.Now;
                        db.Entry(pcobj).State = EntityState.Modified;
                        db.SaveChanges();
                        string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                        return Json(new { IsDuplicate = false, msg = strMessage });
                        #endregion
                    }
                }
                else if (ActivePopup == "ImprovementTactic")
                {
                    int Improvementid = Convert.ToInt32(Id);

                    //get PlanId from database.
                    int planId = (from t in db.Plan_Improvement_Campaign_Program_Tactic
                                  join p in db.Plan_Improvement_Campaign_Program on t.ImprovementPlanProgramId equals p.ImprovementPlanProgramId
                                  join c in db.Plan_Improvement_Campaign on p.ImprovementPlanCampaignId equals c.ImprovementPlanCampaignId
                                  where t.ImprovementPlanTacticId == Improvementid && t.IsDeleted == false
                                  select c.ImprovePlanId).FirstOrDefault();

                    //// Check for duplicate exist or not.
                    Plan_Improvement_Campaign_Program_Tactic pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                                                                       where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && pcpt.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcpt.ImprovementPlanTacticId.Equals(Improvementid) && pcpt.IsDeleted.Equals(false)
                                                                       select pcpt).FirstOrDefault();

                    //// if duplicate record exist then return duplication message.
                    if (pcpvar != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { IsDuplicate = true, redirect = Url.Action("Assortment"), errormsg = strDuplicateMessage });  // Modified by Viral Kadiya on 11/18/2014 to resolve Internal Review Points.
                    }
                    else
                    {
                        string status = string.Empty;
                        Plan_Improvement_Campaign_Program_Tactic pcpobj = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.ImprovementPlanTacticId.Equals(Improvementid)).FirstOrDefault();

                        //If improvement tacitc modified by immediate manager then no resubmission will take place, By dharmraj, Ticket #537
                        BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                        var lstUserHierarchy = objBDSServiceClient.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                        var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId).Select(u => u.UserId).ToList();

                        pcpobj.Title = title;
                        status = pcpobj.Status;
                        pcpobj.ModifiedBy = Sessions.User.UserId;
                        pcpobj.ModifiedDate = DateTime.Now;
                        db.Entry(pcpobj).State = EntityState.Modified;
                        db.SaveChanges();
                        string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                        return Json(new { IsDuplicate = false, redirect = Url.Action("Assortment"), msg = strMessage });

                    }
                }


            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { id = 0 });
        }

        public JsonResult SaveTacticTitle(string Id, string title, ref bool isDuplicate)
        {
            //bool isResultError = false;
            try
            {
                int Tacticid = Convert.ToInt32(Id);
                int linkedTacticId = 0;

                //   List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Select(tac => tac).ToList();
                var objpcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == Tacticid).FirstOrDefault();
                int pid = objpcpt.PlanProgramId;
                int cid = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == pid).Select(program => program.PlanCampaignId).FirstOrDefault();

                #region "Retrieve linkedTactic"
                linkedTacticId = (objpcpt != null && objpcpt.LinkedTacticId.HasValue) ? objpcpt.LinkedTacticId.Value : 0;

                #endregion
                //// Get Tactic duplicate record.
                var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                              join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                              join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                              where pcpt.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(Tacticid) && pcpt.IsDeleted.Equals(false)
                              && pcp.PlanProgramId == objpcpt.PlanProgramId
                              select pcp).FirstOrDefault();

                //// Get Linked Tactic duplicate record.
                Plan_Campaign_Program_Tactic dupLinkedTactic = null;
                Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                if (linkedTacticId > 0)
                {
                    linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault();

                    dupLinkedTactic = (from pcpt in db.Plan_Campaign_Program_Tactic
                                       join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                       join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                       where pcpt.Title.Trim().ToLower().Equals(title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(linkedTacticId) && pcpt.IsDeleted.Equals(false)
                                       && pcp.PlanProgramId == linkedTactic.PlanProgramId
                                       select pcpt).FirstOrDefault();
                }

                //// if duplicate record exist then return duplication message.
                if (dupLinkedTactic != null)
                {
                    isDuplicate = true;
                    string strDuplicateMessage = string.Format(Common.objCached.LinkedPlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                    return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                }
                else if (pcpvar != null)
                {
                    isDuplicate = true;
                    string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                    return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                }
                else
                {

                    Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(Tacticid)).FirstOrDefault();
                    pcpobj.Title = title;
                    db.Entry(pcpobj).State = EntityState.Modified;

                    #region "update linked Tactic"
                    if (linkedTacticId > 0)
                    {
                        Plan_Campaign_Program_Tactic lnkPCPT = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault();
                        lnkPCPT.Title = title;
                        db.Entry(lnkPCPT).State = EntityState.Modified;
                    }
                    #endregion

                    db.SaveChanges();
                    string strMessag = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    return Json(new { IsDuplicate = false, redirect = Url.Action("LoadSetup", new { id = Tacticid }), Msg = strMessag, planTacticId = pcpobj.PlanTacticId, planCampaignId = cid, planProgramId = pid, tacticStatus = pcpobj.Status });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { id = 0 });
        }

        /// <summary>
        /// Load program title and Id based on campaign Id 
        /// </summary>
        /// <param name="ParentId">selected Campaign Id </param>
        /// <returns></returns>
        public JsonResult LoadProgramList(string ParentId)
        {
            int parentCampaignId = 0;
            if (!string.IsNullOrEmpty(ParentId))
            {
                parentCampaignId = Convert.ToInt32(ParentId);
            }
            var programList = db.Plan_Campaign_Program.Where(p => p.IsDeleted.Equals(false) && p.PlanCampaignId == parentCampaignId).Select(p => new
            {
                p.PlanProgramId,
                p.Title
            }).OrderBy(p => p.Title).ToList();
            return Json(programList, JsonRequestBehavior.AllowGet);
        }

        #region "Common Functions"

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Inspect Popup for all sections.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="section">Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="TabValue">Tab value of Popup.</param>
        /// <param name="InspectPopupMode"></param>
        /// <param name="parentId"></param>
        /// <param name="RequestedModule"></param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        public ActionResult LoadInspectPopup(int id, string section, string TabValue = "Setup", string InspectPopupMode = "", int parentId = 0, string RequestedModule = "")
        {
            #region "Initialize variables"

            bool IsPlanEditable = false;
            bool IsPlanCreateAll = false;
            List<Guid> lstSubordinatesIds = new List<Guid>();

            Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = null;
            Plan_Campaign_Program objPlan_Campaign_Program = null;
            Plan_Campaign objPlan_Campaign = null;
            Plan_Improvement_Campaign_Program_Tactic objPlan_Improvement_Campaign_Program_Tactic = null;
            Plan_Campaign_Program_Tactic_LineItem objPlan_Campaign_Program_Tactic_LineItem = null;
            bool IsOwner = false;
            InspectModel im = new InspectModel();
            #endregion
            try
            {




                bool IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);

                //// If Id is null then return section respective PartialView.
                if (id == 0)
                {
                    if (!string.IsNullOrEmpty(RequestedModule))
                    {
                        im.RedirectType = RequestedModule;
                    }
                    im.InspectMode = InspectPopupMode;
                    im.InspectPopup = TabValue;
                    // Added by Arpita Soni for Ticket #2236 on 06/20/2016
                    im.InspectPopup = (TabValue == "Budget" ? "Setup" : TabValue);
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        im.PlanId = parentId;
                        return PartialView("_InspectPopupCampaign", im);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        im.PlanCampaignId = parentId;
                        return PartialView("_InspectPopupProgram", im);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        im.PlanProgramId = parentId;
                        ViewBag.isClientMediaCodesPermission = IsClientMediaCodePermission();   // Added by Viral for PL ticket #2366.
                        ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
                        return PartialView("InspectPopup", im);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        im.PlanProgramId = parentId;

                        return PartialView("_InspectPopupImprovementTactic", im);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
                    {
                        im.PlanTacticId = parentId;

                        ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
                        return PartialView("_InspectPopupLineitem", im);
                    }
                }


                //// Added by Pratik Chauhan for functional review points



                //// load section wise data to ViewBag.
                if (Convert.ToString(section) != "")
                {
                    DateTime todaydate = DateTime.Now;

                    //Verify that existing user has created activity or it has subordinate permission and activity owner is subordinate of existing user
                    bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    if (IsTacticAllowForSubordinates || Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Plan).ToLower())
                    {
                        lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
                    }

                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).FirstOrDefault();


                        if (IsPlanCreateAllAuthorized)
                        {
                            IsPlanCreateAll = true;
                        }
                        else
                        {
                            if (objPlan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign_Program_Tactic.CreatedBy))
                            {
                                IsPlanCreateAll = true;
                            }
                            else
                            {
                                IsPlanCreateAll = false;
                            }

                        }

                        if (objPlan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsOwner = true;
                        }

                        //Modify by Mitesh Vaishnav for PL ticket 746
                        if (lstSubordinatesIds.Contains(objPlan_Campaign_Program_Tactic.CreatedBy))
                        {
                            IsPlanEditable = true;
                        }


                        // To get permission status for Add/Edit Actual, By dharmraj PL #519
                        ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);

                        //// if Tactic status based on Function CheckAfterApprovedStatus
                        if (Common.CheckAfterApprovedStatus(objPlan_Campaign_Program_Tactic.Status))
                        {
                            if (todaydate > objPlan_Campaign_Program_Tactic.StartDate && todaydate < objPlan_Campaign_Program_Tactic.EndDate)
                            {
                                objPlan_Campaign_Program_Tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                            }
                            else if (todaydate > objPlan_Campaign_Program_Tactic.EndDate)
                            {
                                objPlan_Campaign_Program_Tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                            }

                            db.Entry(objPlan_Campaign_Program_Tactic).State = EntityState.Modified;
                            int result = db.SaveChanges();
                        }

                        ViewBag.isClientMediaCodesPermission = IsClientMediaCodePermission();   // Added by Viral for PL ticket #2366.
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).FirstOrDefault();


                        if (IsPlanCreateAllAuthorized)
                        {
                            IsPlanCreateAll = true;
                        }
                        else
                        {
                            if (objPlan_Campaign_Program.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign_Program.CreatedBy))
                            {
                                IsPlanCreateAll = true;
                            }
                            else
                            {
                                IsPlanCreateAll = false;
                            }

                        }
                        if (objPlan_Campaign_Program.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsOwner = true;
                        }

                        if (lstSubordinatesIds.Contains(objPlan_Campaign_Program.CreatedBy))
                        {
                            IsPlanEditable = true;
                        }


                        if (Common.CheckAfterApprovedStatus(objPlan_Campaign_Program.Status))
                        {
                            if (todaydate > objPlan_Campaign_Program.StartDate && todaydate < objPlan_Campaign_Program.EndDate)
                            {
                                objPlan_Campaign_Program.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                            }
                            else if (todaydate > objPlan_Campaign_Program.EndDate)
                            {
                                objPlan_Campaign_Program.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                            }

                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            int result = db.SaveChanges();
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        objPlan_Campaign = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(id)).FirstOrDefault();


                        if (IsPlanCreateAllAuthorized)
                        {
                            IsPlanCreateAll = true;
                        }
                        else
                        {
                            if (objPlan_Campaign.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign.CreatedBy))
                            {
                                IsPlanCreateAll = true;
                            }
                            else
                            {
                                IsPlanCreateAll = false;
                            }

                        }

                        if (objPlan_Campaign.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsOwner = true;
                        }

                        if (lstSubordinatesIds.Contains(objPlan_Campaign.CreatedBy))
                        {
                            IsPlanEditable = true;
                        }

                        if (Common.CheckAfterApprovedStatus(objPlan_Campaign.Status))
                        {
                            if (todaydate > objPlan_Campaign.StartDate && todaydate < objPlan_Campaign.EndDate)
                            {
                                objPlan_Campaign.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                            }
                            else if (todaydate > objPlan_Campaign.EndDate)
                            {
                                objPlan_Campaign.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                            }

                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            int result = db.SaveChanges();
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        objPlan_Improvement_Campaign_Program_Tactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(picpobjw => picpobjw.ImprovementPlanTacticId.Equals(id)).FirstOrDefault();


                        if (objPlan_Improvement_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanEditable = true;
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
                    {
                        objPlan_Campaign_Program_Tactic_LineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => pcptl.PlanLineItemId.Equals(id)).FirstOrDefault();

                        if (IsPlanCreateAllAuthorized)
                        {
                            IsPlanCreateAll = true;
                        }
                        else
                        {
                            if (objPlan_Campaign_Program_Tactic_LineItem.CreatedBy.Equals(Sessions.User.UserId) || objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId))//Tactic created by condition add for ticket #1968 , Date : 05-02-2016, Bhavesh
                            {
                                IsPlanCreateAll = true;
                            }
                            else
                            {
                                IsPlanCreateAll = false;
                            }

                        }

                        if (objPlan_Campaign_Program_Tactic_LineItem.CreatedBy.Equals(Sessions.User.UserId) || objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId))//Tactic created by condition add for ticket #1968 , Date : 05-02-2016, Bhavesh
                        {
                            IsPlanEditable = true;
                        }

                        if (objPlan_Campaign_Program_Tactic_LineItem.LineItemTypeId == null)
                        {
                            ViewBag.IsOtherLineItem = true;
                        }
                        else
                        {
                            ViewBag.IsOtherLineItem = false;
                        }
                        // To get permission status for Add/Edit Actual, By dharmraj PL #519
                        ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
                        //Tactic editable condition add for ticket #1968 , Date : 05-02-2016, Bhavesh
                        if (lstSubordinatesIds.Contains(objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.CreatedBy))
                        {
                            IsPlanEditable = true;
                        }
                    }


                    //// Custom Restrictions
                    if (IsPlanEditable && !IsOwner)
                    {
                        //// Start - Added by Sohel Pathan on 27/01/2015 for PL ticket #1140
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        int itemId = 0;

                        if (section.ToString().Equals(Enums.Section.Campaign.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign != null)
                        {
                            if (objPlan_Campaign.Plan_Campaign_Program != null)
                            {
                                List<int> programIds = objPlan_Campaign.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false)).Select(program => program.PlanProgramId).ToList();
                                if (programIds.Count() > 0)
                                {
                                    planTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programIds.Contains(tactic.Plan_Campaign_Program.PlanProgramId))
                                                                                    .Select(tactic => tactic.PlanTacticId).ToList();
                                    lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                                    if (lstAllowedEntityIds.Count != planTacticIds.Count)
                                    {
                                        IsPlanEditable = false;
                                    }

                                }
                            }
                        }
                        else if (section.ToString().Equals(Enums.Section.Program.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign_Program != null)
                        {
                            if (objPlan_Campaign_Program.Plan_Campaign_Program_Tactic != null)
                            {
                                planTacticIds = objPlan_Campaign_Program.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false)).Select(tactic => tactic.PlanTacticId).ToList();
                                lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                                if (lstAllowedEntityIds.Count != planTacticIds.Count)
                                {
                                    IsPlanEditable = false;
                                }

                            }
                        }
                        else if (section.ToString().Equals(Enums.Section.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign_Program_Tactic != null)
                        {
                            itemId = objPlan_Campaign_Program_Tactic.PlanTacticId;
                            planTacticIds.Add(itemId);
                            lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                            if (lstAllowedEntityIds.Contains(itemId))
                            {
                                IsPlanEditable = true;
                            }
                            else
                            {
                                IsPlanEditable = false;
                            }
                        }
                        else if (section.ToString().Equals(Enums.Section.LineItem.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign_Program_Tactic_LineItem != null)
                        {
                            if (objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic != null)
                            {
                                itemId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.PlanTacticId;
                                planTacticIds.Add(itemId);
                                lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                                if (lstAllowedEntityIds.Contains(itemId))
                                {
                                    IsPlanEditable = true;
                                }
                                else
                                {
                                    IsPlanEditable = false;
                                }
                            }
                        }
                        //// End - Added by Sohel Pathan on 27/01/2015 for PL ticket #1140
                    }
                }
                if (IsOwner)
                {
                    IsPlanEditable = true;
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                }
            }

            im = GetInspectModel(id, section, false);      //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
            if (Convert.ToString(section).Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                IsPlanEditable = false;
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                // To get permission status for Plan Edit, By dharmraj PL #519
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

                if (im.OwnerId.Equals(Sessions.User.UserId))
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstSubordinatesIds.Contains(im.OwnerId))
                    {
                        IsPlanEditable = true;
                    }
                }
            }
            if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
            {
                if (!IsPlanEditable)
                {
                    InspectPopupMode = Enums.InspectPopupMode.ReadOnly.ToString();
                }
            }

            im.IsPlanEditable = IsPlanEditable;
            im.IsPlanCreateAll = IsPlanCreateAll;
            im.InspectMode = InspectPopupMode;
            // Added by Arpita Soni for Ticket #2236 on 06/20/2016
            im.InspectPopup = (TabValue == "Budget" ? "Setup" : TabValue);
            if (!string.IsNullOrEmpty(RequestedModule))
            {
                im.RedirectType = RequestedModule;
            }
            if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
            {
                im.PlanId = objPlan_Campaign_Program.Plan_Campaign.PlanId;
                return PartialView("_InspectPopupProgram", im);
            }
            else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                im.PlanId = objPlan_Campaign.PlanId;
                return PartialView("_InspectPopupCampaign", im);
            }
            else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
            {
                return PartialView("_InspectPopupImprovementTactic", im);
            }
            else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
            {
                im.PlanLineitemId = objPlan_Campaign_Program_Tactic_LineItem.PlanLineItemId;
                im.Title = objPlan_Campaign_Program_Tactic_LineItem.Title;

                im.PlanTacticId = objPlan_Campaign_Program_Tactic_LineItem.PlanTacticId;
                im.PlanCampaignId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                im.PlanProgramId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.PlanProgramId;

                return PartialView("_InspectPopupLineitem", im);
            }
            // Start - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
            else if (Convert.ToString(section).Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);

                return PartialView("_InspectPopupPlan", im);
            }
            //Added by Rahul Shah on 12/04/2016 for PL #2038
            im.LinkedTacticId = objPlan_Campaign_Program_Tactic.LinkedTacticId;
            im.PlanId = objPlan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
            return PartialView("InspectPopup", im);
        }

        /// <summary>
        /// Action to Save SyncToIntegration value for all Inspect Popup.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="section">Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="IsDeployedToIntegration">bool value</param>
        public JsonResult SaveSyncToIntegration(int id, string section, string isDeployToIntegration = "", string isSyncSF = "", string isSyncEloqua = "", string isSyncWorkFront = "", string isSyncMarketo = "", string approvalBehaviorWorkFront = "", string requestQueueWF = "", string assigneeWF = "")
        {
            bool returnValue = false;
            string strPlanEntity = string.Empty;
            try
            {
                if (Sessions.User != null && Sessions.ApplicationId != null) //Added by komal to check session is null for #2299
                {
                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        //Start - Added by Viral Kadiya for PL ticket #2002 - Save integration settings on "Sync" button click.
                        #region "Save integration settings"
                        #region "Declare local variables"
                        bool IsDeployedToIntegration = false, IsSyncSF = false, IsSyncEloqua = false, IsSyncWorkFront = false, IsSyncMarketo = false;
                        #endregion
                        var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(_tactic => _tactic.PlanTacticId == id);
                        if (objTactic != null && objTactic.PlanTacticId > 0)
                        {
                            if (!string.IsNullOrEmpty(isDeployToIntegration))
                                IsDeployedToIntegration = Convert.ToBoolean(isDeployToIntegration);
                            if (!string.IsNullOrEmpty(isSyncSF))
                                IsSyncSF = Convert.ToBoolean(isSyncSF);
                            if (!string.IsNullOrEmpty(isSyncEloqua))
                                IsSyncEloqua = Convert.ToBoolean(isSyncEloqua);
                            if (!string.IsNullOrEmpty(isSyncWorkFront))
                                IsSyncWorkFront = Convert.ToBoolean(isSyncWorkFront);
                            if (!string.IsNullOrEmpty(isSyncMarketo))
                                IsSyncMarketo = Convert.ToBoolean(isSyncMarketo);
                            if (IsSyncWorkFront)
                            {
                                SaveWorkFrontTacticReviewSettings(objTactic, approvalBehaviorWorkFront, requestQueueWF, assigneeWF);  //If integrated to WF, update the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray
                            }

                            objTactic.IsDeployedToIntegration = IsDeployedToIntegration;
                            objTactic.IsSyncSalesForce = IsSyncSF;
                            objTactic.IsSyncEloqua = IsSyncEloqua;
                            objTactic.IsSyncWorkFront = IsSyncWorkFront;
                            objTactic.IsSyncMarketo = IsSyncMarketo;
                            db.Entry(objTactic).State = EntityState.Modified;


                            #region "Update settings for linked tactic"
                            var LinkedTacticId = objTactic.LinkedTacticId;
                            if (LinkedTacticId.HasValue && LinkedTacticId.Value > 0)
                            {
                                Plan_Campaign_Program_Tactic objLinkedTactic = db.Plan_Campaign_Program_Tactic.Where(tacid => tacid.PlanTacticId == LinkedTacticId.Value).FirstOrDefault();
                                objLinkedTactic.IsDeployedToIntegration = IsDeployedToIntegration;
                                objLinkedTactic.IsSyncEloqua = IsSyncEloqua;
                                objLinkedTactic.IsSyncSalesForce = IsSyncSF;
                                objLinkedTactic.IsSyncWorkFront = IsSyncWorkFront;
                                objTactic.IsSyncMarketo = IsSyncMarketo;
                                objLinkedTactic.ModifiedBy = Sessions.User.UserId;
                                objLinkedTactic.ModifiedDate = DateTime.Now;

                                if (IsSyncWorkFront)
                                {
                                    SaveWorkFrontTacticReviewSettings(objLinkedTactic, approvalBehaviorWorkFront, requestQueueWF, assigneeWF);  //If integrated to WF, update the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray
                                }

                                db.Entry(objLinkedTactic).State = EntityState.Modified;
                            }
                            #endregion

                            db.SaveChanges();
                        }
                        #endregion
                        //End - Added by Viral Kadiya for PL ticket #2002 - Save integration settings on "Sync" button click.

                        #region "Sync Tactic to respective Integration Instance"
                        ExternalIntegration externalIntegration = new ExternalIntegration(id, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Tactic); //Modified 1/17/2016 PL#1907 Brad Gray
                        externalIntegration.Sync();
                        #endregion
                        returnValue = true;
                        strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]; // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    }
                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        //var objProgram = db.Plan_Campaign_Program.FirstOrDefault(_program => _program.PlanProgramId == id);
                        //objProgram.IsDeployedToIntegration = IsDeployedToIntegration;
                        //db.Entry(objProgram).State = EntityState.Modified;
                        //db.SaveChanges();

                        #region "Sync Tactic to respective Integration Instance"
                        ExternalIntegration externalIntegration = new ExternalIntegration(id, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Program);
                        externalIntegration.Sync();
                        #endregion

                        returnValue = true;
                        strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()];    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    }
                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        //var objCampaign = db.Plan_Campaign.FirstOrDefault(_campaign => _campaign.PlanCampaignId == id);
                        //objCampaign.IsDeployedToIntegration = IsDeployedToIntegration;
                        //db.Entry(objCampaign).State = EntityState.Modified;
                        //db.SaveChanges();

                        #region "Sync Tactic to respective Integration Instance"
                        ExternalIntegration externalIntegration = new ExternalIntegration(id, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Campaign);
                        externalIntegration.Sync();
                        #endregion

                        returnValue = true;
                        strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()];   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    }
                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        //var objITactic = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(_imprvTactic => _imprvTactic.ImprovementPlanTacticId == id);
                        //objITactic.IsDeployedToIntegration = IsDeployedToIntegration;
                        //db.Entry(objITactic).State = EntityState.Modified;
                        //db.SaveChanges();

                        #region "Sync Tactic to respective Integration Instance"
                        ExternalIntegration externalIntegration = new ExternalIntegration(id, Sessions.ApplicationId, Sessions.User.UserId, EntityType.ImprovementTactic);
                        externalIntegration.Sync();
                        #endregion

                        returnValue = true;
                        strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()];  // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    }

                    #region "Save synced comment to Plan_Campaign_Program_Tactic_Comment table - 1468"
                    string syncComment = string.Empty;

                    if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        //// Save Comment for Improvement Tactic.
                        Plan_Improvement_Campaign_Program_Tactic_Comment pcptc = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                        syncComment = Convert.ToString(Enums.Section.ImprovementTactic) + " synced by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                        pcptc.ImprovementPlanTacticId = id;
                        pcptc.Comment = syncComment;
                        DateTime currentdate = DateTime.Now;
                        pcptc.CreatedDate = currentdate;
                        string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                        pcptc.CreatedBy = Sessions.User.UserId;
                        db.Entry(pcptc).State = EntityState.Added;
                        db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(pcptc);
                    }
                    else
                    {

                        //// Save Comment for Tactic,Program,Campaign.
                        Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();

                        if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                        {
                            pcptc.PlanTacticId = id;
                            syncComment = Convert.ToString(Enums.Section.Tactic) + " synced by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                        }
                        else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                        {

                            pcptc.PlanProgramId = id;
                            syncComment = Convert.ToString(Enums.Section.Program) + " synced by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                        }
                        else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                        {

                            pcptc.PlanCampaignId = id;
                            syncComment = Convert.ToString(Enums.Section.Campaign) + " synced by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                        }
                        pcptc.Comment = syncComment;
                        DateTime currentdate = DateTime.Now;
                        pcptc.CreatedDate = currentdate;
                        string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                        pcptc.CreatedBy = Sessions.User.UserId;
                        db.Entry(pcptc).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                    }
                    db.SaveChanges();
                    #endregion

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", strPlanEntity);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
            return Json(new { result = returnValue, msg = strMessage }, JsonRequestBehavior.AllowGet); // Modified by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get InspectModel.
        /// </summary>
        /// Modifled by :- Sohel Pathan on 27/05/2014 for PL ticket #425, Default parameter added named isStatusChange
        /// <param name="id">Plan Tactic Id.</param>
        /// <param section="id">.Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="section"></param>
        /// <param name="isStatusChange"></param>
        /// <returns>Returns InspectModel.</returns>
        public InspectModel GetInspectModel(int id, string section, bool isStatusChange = true)
        {

            InspectModel imodel = new InspectModel();
            string statusapproved = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Approved.ToString()].ToString();
            string statusinprogress = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.InProgress.ToString()].ToString();
            string statuscomplete = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Complete.ToString()].ToString();
            string statusdecline = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Decline.ToString()].ToString();
            string statussubmit = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Submitted.ToString()].ToString();
            string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
            string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            //Added By komal Rawal for #1282
            Dictionary<string, string> ColorCodelist = db.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            var PlanColor = ColorCodelist[Enums.EntityType.Plan.ToString().ToLower()];
            var ProgramColor = ColorCodelist[Enums.EntityType.Program.ToString().ToLower()];
            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];
            var CampaignColor = ColorCodelist[Enums.EntityType.Campaign.ToString().ToLower()];
            var ImprovementTacticColor = ColorCodelist[Enums.EntityType.ImprovementTactic.ToString().ToLower()];
            //Emd
            try
            {
                //// Get Inspect Model for Tactic InspectPopup.
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    double budgetAllocation = db.Plan_Campaign_Program_Tactic_Cost.Where(tacCost => tacCost.PlanTacticId == id).ToList().Sum(tacCost => tacCost.Value);
                    Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id) && pcptobj.IsDeleted == false).FirstOrDefault();

                    imodel = new InspectModel()
                              {
                                  // Added by Arpita Soni for Ticket #2212 on 05/24/2016 
                                  PlanId = pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId,
                                  PlanTacticId = pcpt.PlanTacticId,
                                  TacticTitle = pcpt.Title,
                                  TacticTypeTitle = pcpt.TacticType.Title,
                                  CampaignTitle = pcpt.Plan_Campaign_Program.Plan_Campaign.Title,
                                  ProgramTitle = pcpt.Plan_Campaign_Program.Title,
                                  Status = pcpt.Status,
                                  TacticTypeId = pcpt.TacticTypeId,
                                  ColorCode = TacticColor,
                                  Description = pcpt.Description,
                                  PlanCampaignId = pcpt.Plan_Campaign_Program.PlanCampaignId,
                                  PlanProgramId = pcpt.PlanProgramId,
                                  OwnerId = pcpt.CreatedBy,
                                  //Modified By : Kalpesh Sharma #864 Add Actuals: Unable to update actuals % 864_Actuals.jpg %
                                  // If tactic has a line item at that time we have consider Project cost as sum of all the active line items
                                  // Modified by Arpita Soni for Ticket #2237 on 06/09/2016
                                  Cost = (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Count() > 0
                                   && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByNone && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByDefault
                                   ?
                                   (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost)
                                    : pcpt.Cost,
                                  StartDate = pcpt.StartDate,
                                  EndDate = pcpt.EndDate,
                                  CostActual = 0,
                                  IsDeployedToIntegration = pcpt.IsDeployedToIntegration,
                                  LastSyncDate = pcpt.LastSyncDate,
                                  StageId = pcpt.StageId,
                                  StageTitle = pcpt.Stage.Title,
                                  StageLevel = pcpt.Stage.Level,
                                  ProjectedStageValue = pcpt.ProjectedStageValue,
                                  TacticCustomName = pcpt.TacticCustomName,
                                  ROIType=pcpt.TacticType.AssetType
                              };


                    TacticStageValue varTacticStageValue = Common.GetTacticStageRelationForSingleTactic(pcpt, false);
                    //// Set MQL
                    string stageMQL = Enums.Stage.MQL.ToString();
                    int levelMQL = db.Stages.Single(stage => stage.ClientId.Equals(Sessions.User.ClientId) && stage.Code.Equals(stageMQL) && stage.IsDeleted == false).Level.Value;
                    int tacticStageLevel = Convert.ToInt32(pcpt.Stage.Level);
                    if (tacticStageLevel < levelMQL)
                    {
                        imodel.MQLs = varTacticStageValue.MQLValue;
                    }
                    else if (tacticStageLevel == levelMQL)
                    {
                        imodel.MQLs = Convert.ToDouble(imodel.ProjectedStageValue);
                    }
                    else if (tacticStageLevel > levelMQL)
                    {
                        imodel.MQLs = 0;
                        TempData["TacticMQL"] = "N/A";
                    }
                    imodel.MQLs = Math.Round((double)imodel.MQLs, 0, MidpointRounding.AwayFromZero);
                    // Set Revenue
                    imodel.Revenues = Math.Round(varTacticStageValue.RevenueValue, 2);

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(pcpt.TacticType.Model);
                }
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())  //// Get Inspect Model for Program InspectPopup.
                {
                    var objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == id && pcp.IsDeleted == false).FirstOrDefault();

                    if (isStatusChange)     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                    {
                        var tacticobjList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false).ToList();
                        int cntSumbitTacticStatus = tacticobjList.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                        int cntApproveTacticStatus = tacticobjList.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                        int cntDeclineTacticStatus = tacticobjList.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();

                        if (cntSumbitTacticStatus == 0)
                        {
                            objPlan_Campaign_Program.Status = statussubmit;
                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntApproveTacticStatus == 0)
                        {
                            objPlan_Campaign_Program.Status = statusapproved;
                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntDeclineTacticStatus == 0)
                        {
                            objPlan_Campaign_Program.Status = statusdecline;
                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    imodel.ProgramTitle = objPlan_Campaign_Program.Title;
                    imodel.CampaignTitle = objPlan_Campaign_Program.Plan_Campaign.Title;
                    imodel.Status = objPlan_Campaign_Program.Status;
                    imodel.ColorCode = ProgramColor;
                    imodel.Description = objPlan_Campaign_Program.Description;
                    imodel.PlanCampaignId = objPlan_Campaign_Program.PlanCampaignId;
                    imodel.PlanProgramId = objPlan_Campaign_Program.PlanProgramId;
                    imodel.OwnerId = objPlan_Campaign_Program.CreatedBy;
                    imodel.Cost = Common.CalculateProgramCost(objPlan_Campaign_Program.PlanProgramId); //objPlan_Campaign_Program.Cost; // Modified for PL#440 by Dharmraj
                    imodel.StartDate = objPlan_Campaign_Program.StartDate;
                    imodel.EndDate = objPlan_Campaign_Program.EndDate;

                    imodel.IsDeployedToIntegration = objPlan_Campaign_Program.IsDeployedToIntegration;
                    imodel.LastSyncDate = objPlan_Campaign_Program.LastSyncDate;

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(objPlan_Campaign_Program.Plan_Campaign.Plan.Model);
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower()) //// Get Inspect Model for Campaign InspectPopup.
                {

                    var objPlan_Campaign = db.Plan_Campaign.Where(pcp => pcp.PlanCampaignId == id && pcp.IsDeleted == false).FirstOrDefault();

                    if (isStatusChange)     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                    {
                        var planCampaignProgramObj = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false).ToList();
                        // Number of program with status is not 'Submitted' 
                        int cntSumbitProgramStatus = planCampaignProgramObj.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                        List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == id && pcpt.IsDeleted == false).ToList();
                        // Number of tactic with status is not 'Submitted'
                        int cntSumbitTacticStatus = tblTactic.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();

                        // Number of program with status is not 'Approved', 'in-progress', 'complete'
                        int cntApproveProgramStatus = planCampaignProgramObj.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                        // Number of tactic with status is not 'Approved', 'in-progress', 'complete'
                        int cntApproveTacticStatus = tblTactic.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();

                        // Number of program with status is not 'Declained'
                        int cntDeclineProgramStatus = planCampaignProgramObj.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();
                        // Number of tactic with status is not 'Declained'
                        int cntDeclineTacticStatus = tblTactic.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();

                        if (cntSumbitProgramStatus == 0 && cntSumbitTacticStatus == 0)
                        {
                            objPlan_Campaign.Status = statussubmit;
                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntApproveProgramStatus == 0 && cntApproveTacticStatus == 0)
                        {
                            objPlan_Campaign.Status = statusapproved;
                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntDeclineProgramStatus == 0 && cntDeclineTacticStatus == 0)
                        {
                            objPlan_Campaign.Status = statusdecline;
                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    imodel.CampaignTitle = objPlan_Campaign.Title;
                    imodel.Status = objPlan_Campaign.Status;
                    imodel.ColorCode = CampaignColor;
                    imodel.Description = objPlan_Campaign.Description;
                    imodel.PlanCampaignId = objPlan_Campaign.PlanCampaignId;
                    imodel.OwnerId = objPlan_Campaign.CreatedBy;
                    imodel.Cost = Common.CalculateCampaignCost(objPlan_Campaign.PlanCampaignId); //objPlan_Campaign.Cost; // Modified for PL#440 by Dharmraj
                    imodel.StartDate = objPlan_Campaign.StartDate;
                    imodel.EndDate = objPlan_Campaign.EndDate;

                    imodel.IsDeployedToIntegration = objPlan_Campaign.IsDeployedToIntegration;
                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(objPlan_Campaign.Plan.Model);
                    imodel.LastSyncDate = objPlan_Campaign.LastSyncDate;

                }
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())    //// Get Inspect Model for ImprovementTactic InspectPopup.
                {
                    imodel = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                              where pcpt.ImprovementPlanTacticId == id && pcpt.IsDeleted == false
                              select new InspectModel
                              {
                                  PlanTacticId = pcpt.ImprovementPlanTacticId,
                                  TacticTitle = pcpt.Title,
                                  TacticTypeTitle = pcpt.ImprovementTacticType.Title,
                                  CampaignTitle = pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                                  ProgramTitle = pcpt.Plan_Improvement_Campaign_Program.Title,
                                  Status = pcpt.Status,
                                  TacticTypeId = pcpt.ImprovementTacticTypeId,
                                  ColorCode = ImprovementTacticColor,
                                  Description = pcpt.Description,
                                  PlanCampaignId = pcpt.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                                  PlanProgramId = pcpt.ImprovementPlanProgramId,
                                  OwnerId = pcpt.CreatedBy,
                                  Cost = pcpt.Cost,
                                  StartDate = pcpt.EffectiveDate,
                                  IsDeployedToIntegration = pcpt.IsDeployedToIntegration,
                                  LastSyncDate = pcpt.LastSyncDate,
                                  ImprovementPlanProgramId = pcpt.ImprovementPlanProgramId,
                                  ImprovementPlanTacticId = pcpt.ImprovementPlanTacticId,
                                  ImprovementTacticTypeId = pcpt.ImprovementTacticTypeId,
                                  EffectiveDate = pcpt.EffectiveDate,
                                  Title = pcpt.Title
                              }).FirstOrDefault();

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(varT => varT.ImprovementPlanTacticId == id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model);

                }
                // Start - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
                else if (section.Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase)) //// Get Inspect Model for Plan InspectPopup.
                {
                    var objPlan = (from p in db.Plans
                                   where p.PlanId == id && p.IsDeleted == false
                                   select p).FirstOrDefault();

                    imodel.PlanId = objPlan.PlanId;
                    imodel.ColorCode = PlanColor;
                    imodel.Description = objPlan.Description;
                    imodel.OwnerId = objPlan.CreatedBy;
                    imodel.Title = objPlan.Title;
                    imodel.ModelId = objPlan.ModelId;
                    imodel.ModelTitle = objPlan.Model.Title + " " + objPlan.Model.Version;
                    imodel.GoalType = objPlan.GoalType;
                    imodel.GoalValue = objPlan.GoalValue.ToString();
                    imodel.Budget = objPlan.Budget;
                    imodel.AllocatedBy = objPlan.AllocatedBy;
                }
                // End - Added by Sohel Pathan on 07/11/2014 for PL ticket #811

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return imodel;
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Comment in Review Tab.
        /// </summary>
        /// <param name="comment">Comment.</param>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="section">Decide for which section (tactic,program or campaign comment will be added)</param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        [HttpPost]
        public JsonResult SaveComment(string comment, int planTacticId, string section)
        {
            int result = 0;
            int? LinkedTacticid = db.Plan_Campaign_Program_Tactic.Where(Linkid => Linkid.PlanTacticId == planTacticId && Linkid.IsDeleted == false).Select(LinkId => LinkId.LinkedTacticId).FirstOrDefault();
            try
            {
                ////Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #569: make urls in tactic notes hyperlinks
                string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,_\[\]\(\)\!\$\*\|##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";   // Modified by Viral Kadiya on PL ticket to resolve issue #794.
                Regex r = new Regex(regex, RegexOptions.IgnoreCase);
                comment = r.Replace(comment, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"//www");



                ////End Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #569: make urls in tactic notes hyperlinks
                if (ModelState.IsValid)
                {
                    if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        //// save comment for ImprovementTactic section.
                        Plan_Improvement_Campaign_Program_Tactic_Comment pcpitc = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                        pcpitc.ImprovementPlanTacticId = planTacticId;
                        pcpitc.Comment = comment;
                        DateTime currentdate = DateTime.Now;
                        pcpitc.CreatedDate = currentdate;
                        string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                        pcpitc.CreatedBy = Sessions.User.UserId;
                        db.Entry(pcpitc).State = EntityState.Added;
                        db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(pcpitc);
                        result = db.SaveChanges();
                    }
                    else
                    {
                        //// save comment for Tactic,Program,Campaign section.
                        Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
                        DateTime currentdate = DateTime.Now;
                        string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                        if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                        {
                            pcptc.PlanTacticId = planTacticId;

                            //Comment for Linked Tactic added by komal rawal
                            if (LinkedTacticid != null && LinkedTacticid > 0)
                            {

                                Plan_Campaign_Program_Tactic_Comment objLinkedtactic = new Plan_Campaign_Program_Tactic_Comment();
                                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                {
                                    objLinkedtactic.PlanTacticId = LinkedTacticid;
                                }
                                objLinkedtactic.Comment = comment;
                                objLinkedtactic.CreatedDate = currentdate;
                                objLinkedtactic.CreatedBy = Sessions.User.UserId;
                                db.Entry(objLinkedtactic).State = EntityState.Added;
                                db.Plan_Campaign_Program_Tactic_Comment.Add(objLinkedtactic);

                            }
                            //End
                        }
                        else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                        {
                            pcptc.PlanProgramId = planTacticId;
                        }
                        else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                        {
                            pcptc.PlanCampaignId = planTacticId;
                        }
                        pcptc.Comment = comment;
                        pcptc.CreatedDate = currentdate;
                        pcptc.CreatedBy = Sessions.User.UserId;
                        db.Entry(pcptc).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                        result = db.SaveChanges();



                    }

                    if (result >= 1)
                    {
                        //// Send Comment Addedd Email to Users.
                        if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                        {
                            var PlanIds = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId || pcpt.PlanTacticId == LinkedTacticid).Select(_tactic => _tactic).ToList();
                            var ListOfTactics = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == planTacticId || _tactic.PlanTacticId == LinkedTacticid).ToList();
                            int PlanId = PlanIds.Where(pcpt => pcpt.PlanTacticId == planTacticId).Select(_tactic => _tactic.Plan_Campaign_Program.Plan_Campaign.PlanId).FirstOrDefault();
                            Plan_Campaign_Program_Tactic pct = ListOfTactics.Where(_tactic => _tactic.PlanTacticId == planTacticId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.TacticCommentAdded.ToString(), pct.Title, true, comment, Convert.ToString(Enums.Section.Tactic).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            //Comment for Linked Tactic added by komal rawal
                            //if (LinkedTacticid != null && LinkedTacticid > 0)
                            //{
                            //    int LinkedPlanId = PlanIds.Where(pcpt => pcpt.PlanTacticId == LinkedTacticid).Select(_tactic => _tactic.Plan_Campaign_Program.Plan_Campaign.PlanId).FirstOrDefault();
                            //    Plan_Campaign_Program_Tactic LinkedTactic = ListOfTactics.Where(_tactic => _tactic.PlanTacticId == planTacticId).FirstOrDefault();
                            //    //string LinkedstrUrl = GetNotificationURLbyStatus(LinkedPlanId, Convert.ToInt32(LinkedTacticid), section);
                            //  //  Common.mailSendForTactic(Convert.ToInt32(LinkedTacticid), Enums.Custom_Notification.TacticCommentAdded.ToString(), LinkedTactic.Title, true, comment, Convert.ToString(Enums.Section.Tactic).ToLower(), LinkedstrUrl);

                            //}
                            //End


                        }
                        else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                        {
                            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == planTacticId).FirstOrDefault();
                            int PlanId = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanProgramId == planTacticId).Select(program => program.Plan_Campaign.PlanId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.ProgramCommentAdded.ToString(), pcp.Title, true, comment, Convert.ToString(Enums.Section.Program).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                        else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                        {
                            Plan_Campaign pc = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == planTacticId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(pc.PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.CampaignCommentAdded.ToString(), pc.Title, true, comment, Convert.ToString(Enums.Section.Campaign).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                        else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                        {
                            Plan_Improvement_Campaign_Program_Tactic pc = db.Plan_Improvement_Campaign_Program_Tactic.Where(imprvTactic => imprvTactic.ImprovementPlanTacticId == planTacticId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString(), pc.Title, true, comment, Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                    }
                    return Json(new { id = planTacticId, TabValue = "Review", msg = Common.objCached.EmptyFieldCommentAdded });      // Modified by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { id = 0 });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Comment & Update Status as Per Selected Value.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="status">status.</param>
        /// <param name="section">Decide for wich section (tactic,program or campaign) status will be updated)</param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        [HttpPost]
        public JsonResult ApprovedTactic(int planTacticId, string status, string section, string isDeployToIntegration = "", string isSyncSF = "", string isSyncEloqua = "", string isSyncWorkFront = "", string isSyncMarketo = "", string approvalBehaviorWorkFront = "", string requestQueueWF = "", string assigneeWF = "")
        {
            int planid = 0;
            int result = 0;
            string approvedComment = "";
            string strmessage = "";
            bool Addcomment = false;
            var LinkedTacticId = db.Plan_Campaign_Program_Tactic.Where(tacid => tacid.PlanTacticId == planTacticId).Select(tacid => tacid.LinkedTacticId).FirstOrDefault();
            Plan_Campaign_Program program = db.Plan_Campaign_Program.Where(pt => pt.PlanProgramId == planTacticId).FirstOrDefault();
            if (program != null)
            {
                planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == program.PlanCampaignId && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }
            try
            {
                if (Sessions.User != null)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        //using (var scope = new TransactionScope())
                        {
                            if (ModelState.IsValid)
                            {
                                if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                                {
                                    //// Save Comment for Improvement Tactic.
                                    Plan_Improvement_Campaign_Program_Tactic_Comment pcptc = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                                    approvedComment = Convert.ToString(Enums.Section.ImprovementTactic) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                    pcptc.ImprovementPlanTacticId = planTacticId;
                                    pcptc.Comment = approvedComment;
                                    DateTime currentdate = DateTime.Now;
                                    pcptc.CreatedDate = currentdate;
                                    string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                                    pcptc.CreatedBy = Sessions.User.UserId;
                                    db.Entry(pcptc).State = EntityState.Added;
                                    db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(pcptc);
                                    result = db.SaveChanges();
                                }
                                else
                                {
                                    DateTime currentdate = DateTime.Now;
                                    //// Save Comment for Tactic,Program,Campaign.
                                    Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
                                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                    {
                                        pcptc.PlanTacticId = planTacticId;
                                        approvedComment = Convert.ToString(Enums.Section.Tactic) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                        if (LinkedTacticId != null)
                                        {
                                            Plan_Campaign_Program_Tactic_Comment ObjLinkedTactic = new Plan_Campaign_Program_Tactic_Comment();
                                            ObjLinkedTactic.PlanTacticId = LinkedTacticId;
                                            ObjLinkedTactic.Comment = approvedComment;
                                            ObjLinkedTactic.CreatedDate = currentdate;
                                            ObjLinkedTactic.CreatedBy = Sessions.User.UserId;
                                            db.Entry(ObjLinkedTactic).State = EntityState.Added;
                                            db.Plan_Campaign_Program_Tactic_Comment.Add(ObjLinkedTactic);

                                        }

                                    }
                                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                    {

                                        pcptc.PlanProgramId = planTacticId;
                                        approvedComment = Convert.ToString(Enums.Section.Program) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                    {

                                        pcptc.PlanCampaignId = planTacticId;
                                        approvedComment = Convert.ToString(Enums.Section.Campaign) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                    }
                                    pcptc.Comment = approvedComment;
                                    pcptc.CreatedDate = currentdate;
                                    string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                                    pcptc.CreatedBy = Sessions.User.UserId;
                                    db.Entry(pcptc).State = EntityState.Added;
                                    db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                                    result = db.SaveChanges();
                                }
                                if (result >= 1)
                                {
                                    //// Save Status for all section.
                                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                    {
                                        var Tacticlist = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == planTacticId || pt.PlanTacticId == LinkedTacticId).ToList();
                                        Plan_Campaign_Program_Tactic Linkedtacticobj = Tacticlist.Where(pt => pt.PlanTacticId == LinkedTacticId).FirstOrDefault();
                                        Plan_Campaign_Program_Tactic tactic = Tacticlist.Where(pt => pt.PlanTacticId == planTacticId).FirstOrDefault();
                                        bool isApproved = false;
                                        DateTime todaydate = DateTime.Now;

                                        #region "Update Tactic Integration Settings"
                                        bool IsSyncSF = false, IsSyncEloqua = false, IsDeployToIntegration = false, IsSyncWorkFront = false, IsSyncMarketo = false;              // Declare local variables.
                                        IsDeployToIntegration = !string.IsNullOrEmpty(isDeployToIntegration) ? bool.Parse(isDeployToIntegration) : false; // Parse isDeployToIntegration value.
                                        IsSyncSF = !string.IsNullOrEmpty(isSyncSF) ? bool.Parse(isSyncSF) : false;                                        // Parse isSyncSF value
                                        IsSyncEloqua = !string.IsNullOrEmpty(isSyncEloqua) ? bool.Parse(isSyncEloqua) : false;                            // Parse isSyncEloqua value
                                        IsSyncWorkFront = !string.IsNullOrEmpty(isSyncWorkFront) ? bool.Parse(isSyncWorkFront) : false;                   // Parse isSyncWorkFront value
                                        IsSyncMarketo = !string.IsNullOrEmpty(isSyncMarketo) ? bool.Parse(isSyncMarketo) : false;                         // Parse isSyncMarketo value

                                        if (IsSyncWorkFront)
                                        {
                                            SaveWorkFrontTacticReviewSettings(tactic, approvalBehaviorWorkFront, requestQueueWF, assigneeWF);  //If integrated to WF, update the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray
                                        }


                                        tactic.IsDeployedToIntegration = IsDeployToIntegration;
                                        tactic.IsSyncEloqua = IsSyncEloqua;
                                        tactic.IsSyncSalesForce = IsSyncSF;
                                        tactic.IsSyncWorkFront = IsSyncWorkFront;
                                        tactic.IsSyncMarketo = IsSyncMarketo;
                                        tactic.ModifiedBy = Sessions.User.UserId;
                                        tactic.ModifiedDate = DateTime.Now;
                                        db.Entry(tactic).State = EntityState.Modified;

                                        #region "Update linked Tactic Integration Settings"
                                        if (Linkedtacticobj != null)
                                        {
                                            Linkedtacticobj.IsDeployedToIntegration = IsDeployToIntegration;
                                            Linkedtacticobj.IsSyncEloqua = IsSyncEloqua;
                                            Linkedtacticobj.IsSyncSalesForce = IsSyncSF;
                                            Linkedtacticobj.IsSyncWorkFront = IsSyncWorkFront;
                                            Linkedtacticobj.IsSyncMarketo = IsSyncMarketo;
                                            Linkedtacticobj.ModifiedBy = Sessions.User.UserId;
                                            Linkedtacticobj.ModifiedDate = DateTime.Now;

                                            if (IsSyncWorkFront)
                                            {
                                                SaveWorkFrontTacticReviewSettings(Linkedtacticobj, approvalBehaviorWorkFront, requestQueueWF, assigneeWF);  //If integrated to WF, update the IntegrationWorkFrontTactic Settings - added 24 Jan 2016 by Brad Gray
                                            }


                                            db.Entry(Linkedtacticobj).State = EntityState.Modified;
                                        }
                                        #endregion

                                        db.SaveChanges();

                                        #endregion

                                        if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                        {
                                            tactic.Status = status;
                                            if (LinkedTacticId != null)
                                            {
                                                Linkedtacticobj.Status = status;
                                            }
                                            isApproved = true;
                                            if (todaydate > tactic.StartDate && todaydate < tactic.EndDate)
                                            {
                                                tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                                if (LinkedTacticId != null)
                                                {
                                                    Linkedtacticobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                                }
                                            }
                                            else if (todaydate > tactic.EndDate)
                                            {
                                                tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                                if (LinkedTacticId != null)
                                                {
                                                    Linkedtacticobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tactic.Status = status;
                                            if (LinkedTacticId != null)
                                            {
                                                Linkedtacticobj.Status = status;
                                            }
                                        }
                                        tactic.ModifiedBy = Sessions.User.UserId;
                                        tactic.ModifiedDate = DateTime.Now;
                                        db.Entry(tactic).State = EntityState.Modified;

                                        if (LinkedTacticId != null)
                                        {
                                            Linkedtacticobj.ModifiedBy = Sessions.User.UserId;
                                            Linkedtacticobj.ModifiedDate = DateTime.Now;
                                            db.Entry(Linkedtacticobj).State = EntityState.Modified;
                                        }

                                        result = db.SaveChanges();

                                        planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == (db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == (db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt.PlanProgramId).FirstOrDefault()) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp.PlanCampaignId).FirstOrDefault()) && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
                                        if (result >= 1)
                                        {
                                            if (isApproved)
                                            {
                                                result = Common.InsertChangeLog(planid, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                                //added by uday for #532 
                                                if (tactic.IsDeployedToIntegration == true)
                                                {
                                                    ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Tactic);
                                                    externalIntegration.Sync();
                                                }
                                                //end
                                            }
                                            else if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                            {
                                                result = Common.InsertChangeLog(planid, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined, null);
                                            }
                                        }
                                        if (result >= 1)
                                        {
                                            //// Send Notification Url for Tactic.
                                            string strURL = GetNotificationURLbyStatus(planid, planTacticId, section);//Url.Action("Index", "Home", new { currentPlanId = Sessions.PlanId, planTacticId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                                            Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.Tactic).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                        }
                                        strmessage = Common.objCached.TacticStatusSuccessfully.Replace("{0}", status);

                                        // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                        string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                        if (!string.IsNullOrEmpty(strStatusMessage))
                                        {
                                            strmessage = strStatusMessage;
                                            strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                        }
                                        // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.

                                        if (!tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString()))
                                        {
                                            Addcomment = true;
                                        }
                                        //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                        //-- Update Program status according to the tactic status
                                        Common.ChangeProgramStatus(tactic.PlanProgramId, Addcomment);

                                        //-- Update Campaign status according to the tactic and program status
                                        var PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId;
                                        Common.ChangeCampaignStatus(PlanCampaignId, Addcomment);
                                        //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                        if (LinkedTacticId != null)
                                        {

                                            if (!Linkedtacticobj.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString()))
                                            {
                                                Addcomment = true;
                                            }
                                            //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                            //-- Update Program status according to the tactic status
                                            Common.ChangeProgramStatus(Linkedtacticobj.PlanProgramId, Addcomment);

                                            //-- Update Campaign status according to the tactic and program status
                                            var LinkedPlanCampaignId = Linkedtacticobj.Plan_Campaign_Program.PlanCampaignId;
                                            Common.ChangeCampaignStatus(LinkedPlanCampaignId, Addcomment);
                                        }

                                    }
                                    else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                                    {
                                        Plan_Improvement_Campaign_Program_Tactic tactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(pt => pt.ImprovementPlanTacticId == planTacticId).FirstOrDefault();
                                        tactic.Status = status;
                                        tactic.ModifiedBy = Sessions.User.UserId;
                                        tactic.ModifiedDate = DateTime.Now;
                                        db.Entry(tactic).State = EntityState.Modified;
                                        result = db.SaveChanges();

                                        planid = (from t in db.Plan_Improvement_Campaign_Program_Tactic
                                                  join p in db.Plan_Improvement_Campaign_Program on t.ImprovementPlanProgramId equals p.ImprovementPlanProgramId
                                                  join c in db.Plan_Improvement_Campaign on p.ImprovementPlanCampaignId equals c.ImprovementPlanCampaignId
                                                  where t.ImprovementPlanTacticId == planTacticId && t.IsDeleted == false
                                                  select c.ImprovePlanId).FirstOrDefault();

                                        if (result == 1)
                                        {
                                            if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                            {
                                                result = Common.InsertChangeLog(planid, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                                //added by uday for #532
                                                if (tactic.IsDeployedToIntegration == true)
                                                {
                                                    ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.ImprovementTactic);
                                                    externalIntegration.Sync();
                                                }
                                                //end by uday for #532
                                            }
                                            else if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                            {
                                                result = Common.InsertChangeLog(planid, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined, null);
                                            }
                                        }
                                        if (result >= 1)
                                        {
                                            string strURL = GetNotificationURLbyStatus(planid, planTacticId, section);
                                            Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                        }
                                        strmessage = Common.objCached.ImprovementTacticStatusSuccessfully.Replace("{0}", status);
                                        // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                        string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                        if (!string.IsNullOrEmpty(strStatusMessage))
                                        {
                                            strmessage = strStatusMessage;
                                            strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);
                                        }
                                        // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                    {
                                        // Plan_Campaign_Program program = db.Plan_Campaign_Program.Where(pt => pt.PlanProgramId == planTacticId).FirstOrDefault();
                                        program.Status = status;
                                        program.ModifiedBy = Sessions.User.UserId;
                                        program.ModifiedDate = DateTime.Now;
                                        db.Entry(program).State = EntityState.Modified;
                                        result = db.SaveChanges();
                                        if (result == 1)
                                        {
                                            if (program.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                            {
                                                Addcomment = true;
                                                string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
                                                UpdateChildEntityStatusByParent(section, strstatus, new List<int> { planTacticId }); // Update child tactics status.
                                                AddComment(strstatus, planTacticId, Enums.Section.Program.ToString().ToLower(), planid);
                                                result = Common.InsertChangeLog(planid, null, planTacticId, program.Title.ToString(), Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);

                                                if (program.IsDeployedToIntegration == true)
                                                {
                                                    ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Program);
                                                    externalIntegration.Sync();
                                                }
                                            }
                                            else if (program.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString()))
                                            {
                                                Addcomment = true;
                                                string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                                UpdateChildEntityStatusByParent(section, strstatus, new List<int> { planTacticId }); // Update child tactics status.
                                                AddComment(strstatus, planTacticId, Enums.Section.Program.ToString().ToLower(), planid);

                                            }
                                            else if (program.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                            {
                                                Addcomment = true;
                                                string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString();
                                                UpdateChildEntityStatusByParent(section, strstatus, new List<int> { planTacticId }); // Update child tactics status.
                                                AddComment(strstatus, planTacticId, Enums.Section.Program.ToString().ToLower(), planid);
                                                Common.InsertChangeLog(program.Plan_Campaign.PlanId, 0, program.PlanProgramId, program.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);

                                            }

                                            //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                            Common.ChangeCampaignStatus(program.PlanCampaignId, Addcomment);
                                            //-- Update Campaign status according to the tactic and program status

                                        }
                                        if (result >= 1)
                                        {
                                            string strURL = GetNotificationURLbyStatus(planid, planTacticId, section);
                                            Common.mailSendForTactic(planTacticId, status, program.Title, false, "", Convert.ToString(Enums.Section.Program).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                        }
                                        strmessage = Common.objCached.ProgramStatusSuccessfully.Replace("{0}", status);
                                        // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                        string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                        if (!string.IsNullOrEmpty(strStatusMessage))
                                        {
                                            strmessage = strStatusMessage;
                                            strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);
                                        }
                                        // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                    {
                                        Plan_Campaign campaign = db.Plan_Campaign.Where(pt => pt.PlanCampaignId == planTacticId).FirstOrDefault();
                                        campaign.Status = status;
                                        campaign.ModifiedBy = Sessions.User.UserId;
                                        campaign.ModifiedDate = DateTime.Now;
                                        db.Entry(campaign).State = EntityState.Modified;
                                        result = db.SaveChanges();
                                        if (result == 1)
                                        {
                                            if (campaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                            {
                                                string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
                                                UpdateChildEntityStatusByParent(section, strstatus, new List<int> { planTacticId });    // Update Program & Tactic status.
                                                AddComment(strstatus, planTacticId, Enums.Section.Campaign.ToString().ToLower(), campaign.PlanId);
                                                result = Common.InsertChangeLog(campaign.PlanId, null, planTacticId, campaign.Title.ToString(), Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                                if (campaign.IsDeployedToIntegration == true)
                                                {
                                                    ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.UserId, EntityType.Campaign);
                                                    externalIntegration.Sync();
                                                }
                                            }
                                            else if (campaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString()))
                                            {
                                                string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                                UpdateChildEntityStatusByParent(section, strstatus, new List<int> { planTacticId });    // Update Program & Tactic status.
                                                AddComment(strstatus, planTacticId, Enums.Section.Campaign.ToString().ToLower(), campaign.PlanId);

                                            }
                                            else if (campaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                            {
                                                string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString();
                                                UpdateChildEntityStatusByParent(section, strstatus, new List<int> { planTacticId });    // Update Program & Tactic status.
                                                AddComment(strstatus, planTacticId, Enums.Section.Campaign.ToString().ToLower(), campaign.PlanId);
                                                Common.InsertChangeLog(campaign.PlanId, 0, campaign.PlanCampaignId, campaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);

                                            }
                                        }
                                        if (result >= 1)
                                        {
                                            string strURL = GetNotificationURLbyStatus(campaign.PlanId, planTacticId, section);
                                            Common.mailSendForTactic(planTacticId, status, campaign.Title, false, "", Convert.ToString(Enums.Section.Campaign).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                        }


                                        strmessage = Common.objCached.CampaignStatusSuccessfully.Replace("{0}", status);
                                        // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                        string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                        if (!string.IsNullOrEmpty(strStatusMessage))
                                        {
                                            strmessage = strStatusMessage;
                                            strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);
                                        }
                                        // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    }
                                }
                                // Add By Nishant Sheth
                                // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                                DataSet dsPlanCampProgTac = new DataSet();
                                dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                                objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                                List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                                objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                                var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                                objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                                var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                                objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                                var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                                objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                                var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                                objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                                //scope.Complete();
                                return Json(new { id = planTacticId, TabValue = "Review", msg = strmessage });
                            }
                        }
                    }
                }
                else
                {
                    return Json(new { id = 0 });
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { id = 0 });
        }

        /// <summary>
        /// Update child entity status on Approving parent entity.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="status"></param>
        /// <param name="prntEntityIds"></param>
        /// <returns></returns>
        public void UpdateChildEntityStatusByParent(string section, string status, List<int> prntEntityIds)
        {
            try
            {
                if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    if (prntEntityIds != null && prntEntityIds.Count > 0)
                    {
                        List<Plan_Campaign_Program_Tactic> lstTactics = db.Plan_Campaign_Program_Tactic.Where(pcpt => prntEntityIds.Contains(pcpt.PlanProgramId)).ToList();
                        #region "Get LinkedTactic list"
                        if (lstTactics != null && lstTactics.Count > 0)
                        {
                            List<int> LinkedtacIds = lstTactics.Where(tac => tac.LinkedTacticId.HasValue).Select(tac => tac.LinkedTacticId.Value).ToList();
                            if (LinkedtacIds != null && LinkedtacIds.Count > 0)
                            {
                                List<Plan_Campaign_Program_Tactic> lstLinkedTactics = db.Plan_Campaign_Program_Tactic.Where(tac => LinkedtacIds.Contains(tac.PlanTacticId)).ToList();
                                lstTactics.AddRange(lstLinkedTactics);
                            }
                        }
                        #endregion

                        #region "Update Tactic Status"
                        try
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            lstTactics.ForEach(pcpt => { pcpt.Status = status; pcpt.ModifiedBy = Sessions.User.UserId; pcpt.ModifiedDate = DateTime.Now; });
                        }
                        finally
                        {
                            db.Configuration.AutoDetectChangesEnabled = true;
                        }
                        db.SaveChanges();
                        #endregion
                    }
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    if (prntEntityIds != null && prntEntityIds.Count > 0)
                    {
                        List<Plan_Campaign_Program> lstPrograms = db.Plan_Campaign_Program.Where(pcp => prntEntityIds.Contains(pcp.PlanCampaignId)).ToList();
                        if (lstPrograms != null && lstPrograms.Count > 0)
                        {
                            lstPrograms.ForEach(pcp => { pcp.Status = status; pcp.ModifiedBy = Sessions.User.UserId; pcp.ModifiedDate = DateTime.Now; });
                            List<int> programIds = lstPrograms.Select(prg => prg.PlanProgramId).ToList();
                            UpdateChildEntityStatusByParent(Enums.Section.Program.ToString().ToLower(), status, programIds); // update child tactic status.
                            //db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        /// <summary>
        /// Get Current Date Based on Plan Year.
        /// </summary>
        /// <param name="isEndDate"></param>
        /// <returns></returns>
        private DateTime GetCurrentDateBasedOnPlan(bool isEndDate = false, int planId = 0)//int planId,
        {
            string Year = db.Plans.Where(plan => plan.PlanId == planId).Select(plan => plan.Year).FirstOrDefault();//Sessions.PlanId
            DateTime CurrentDate = DateTime.Now;
            int CurrentYear = CurrentDate.Year;
            int diffYear = Convert.ToInt32(Year) - CurrentYear;
            DateTime returnDate = DateTime.Now;
            if (isEndDate && diffYear == 0)
            {
                DateTime lastEndDate = new DateTime(CurrentDate.AddYears(diffYear).Year, 12, 31);
                DateTime endDate = CurrentDate.AddYears(diffYear).AddMonths(1);
                returnDate = endDate > lastEndDate ? lastEndDate : endDate;
            }
            else if (diffYear == 0)
            {
                returnDate = DateTime.Now.AddYears(diffYear);
            }
            return returnDate;
        }

        /// <summary>
        /// Added By: Pratik Chauhan.
        /// Action to open resubmission popup.
        /// </summary>
        /// <param name="redirectionType">From where it open.</param>
        /// <param name="labelValues">Changed control label value(s).</param>
        /// <returns>Returns Partial View Of resubmission popup.</returns>
        public PartialViewResult LoadResubmission(string redirectionType, string labelValues)
        {
            var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(labelValues);

            //// Create list for changed control label(s).
            List<string> listLabelValue = new List<string>();

            if (customFields.Count != 0)
            {
                foreach (var item in customFields)
                {
                    listLabelValue.Add(item.Value.Replace("_", " "));
                }
            }

            ViewBag.RedirectionType = redirectionType;
            ViewBag.resubmissionValues = listLabelValue;

            return PartialView("_ResubmissionPopup");
        }

        #region "Share Tactic"
        /// <summary>
        /// Share Tactic,Campaign or Program based on the section passed.
        /// </summary>
        /// <param name="planTacticId"></param>
        /// <param name="toEmailIds"></param>
        /// <param name="optionalMessage"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public JsonResult ShareTactic(int planTacticId, string toEmailIds, string optionalMessage, string section)
        {
            int result = 0;
            string notificationShare = "";
            string emailBody = "";
            Notification notification = new Notification();
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (ModelState.IsValid)
                        {
                            //// Added by Sohel on 2nd April for PL#398 to decode the optionalMessage text
                            optionalMessage = HttpUtility.UrlDecode(optionalMessage, System.Text.Encoding.Default);

                            string strURL = string.Empty;
                            Plan plan = new Plan();
                            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                            {
                                plan = db.Plan_Campaign_Program_Tactic.Single(_tactic => _tactic.PlanTacticId.Equals(planTacticId)).Plan_Campaign_Program.Plan_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareTactic.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.

                            }
                            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                            {
                                plan = db.Plan_Campaign_Program.Single(_program => _program.PlanProgramId.Equals(planTacticId)).Plan_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareProgram.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            }
                            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                            {
                                plan = db.Plan_Campaign.Single(pt => pt.PlanCampaignId.Equals(planTacticId)).Plan;
                                notificationShare = Enums.Custom_Notification.ShareCampaign.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            }
                            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                            {
                                plan = db.Plan_Improvement_Campaign_Program_Tactic.Single(_tactic => _tactic.ImprovementPlanTacticId.Equals(planTacticId)).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareImprovementTactic.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            }

                            //// Send Share Notification Email to ToEmailIds list.
                            foreach (string toEmail in toEmailIds.Split(','))
                            {
                                if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                                {
                                    Plan_Improvement_Campaign_Program_Tactic_Share improvementTacticShare = new Plan_Improvement_Campaign_Program_Tactic_Share();
                                    improvementTacticShare.ImprovementPlanTacticId = planTacticId;
                                    improvementTacticShare.EmailId = toEmail;
                                    //// Modified by Sohel on 3rd April for PL#398 to encode the email body while inserting into DB.
                                    improvementTacticShare.EmailBody = HttpUtility.HtmlEncode(emailBody);
                                    ////
                                    improvementTacticShare.CreatedDate = DateTime.Now;
                                    improvementTacticShare.CreatedBy = Sessions.User.UserId;
                                    db.Entry(improvementTacticShare).State = EntityState.Added;
                                    db.Plan_Improvement_Campaign_Program_Tactic_Share.Add(improvementTacticShare);
                                    result = db.SaveChanges();
                                }
                                else
                                {
                                    Tactic_Share tacticShare = new Tactic_Share();
                                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                    {
                                        tacticShare.PlanTacticId = planTacticId;
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                    {
                                        tacticShare.PlanProgramId = planTacticId;
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                    {
                                        tacticShare.PlanCampaignId = planTacticId;
                                    }
                                    tacticShare.EmailId = toEmail;
                                    //// Modified by Sohel on 3rd April for PL#398 to encode the email body while inserting into DB.
                                    tacticShare.EmailBody = HttpUtility.HtmlEncode(emailBody);
                                    ////
                                    tacticShare.CreatedDate = DateTime.Now;
                                    tacticShare.CreatedBy = Sessions.User.UserId;
                                    db.Entry(tacticShare).State = EntityState.Added;
                                    db.Tactic_Share.Add(tacticShare);
                                    result = db.SaveChanges();
                                }

                                if (result == 1)
                                {
                                    Common.sendMail(toEmail, Common.FromMail, emailBody, notification.Subject, string.Empty);
                                }
                            }

                            scope.Complete();
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Show share Tactic,Program or Campaign based on the passed section
        /// </summary>
        /// <param name="planTacticId"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public ActionResult ShowShareTactic(int planTacticId, string section)
        {
            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
            {
                Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Single(_tactic => _tactic.PlanTacticId.Equals(planTacticId));
                ViewBag.PlanTacticId = planTacticId;
                ViewBag.TacticTitle = planTactic.Title;

                //// Modified By Maninder Singh Wadhva PL Ticket#47
                ViewBag.IsImprovement = false;
            }

            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
            {
                Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Single(_program => _program.PlanProgramId.Equals(planTacticId));
                ViewBag.PlanProgramId = planTacticId;
                ViewBag.ProgramTitle = planProgram.Title;
            }

            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                Plan_Campaign planCampaign = db.Plan_Campaign.Single(_campaign => _campaign.PlanCampaignId.Equals(planTacticId));
                ViewBag.PlanCampaignId = planTacticId;
                ViewBag.CampaignTitle = planCampaign.Title;
            }
            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
            {
                Plan_Improvement_Campaign_Program_Tactic improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Single(_tactic => _tactic.ImprovementPlanTacticId.Equals(planTacticId));
                ViewBag.PlanTacticId = planTacticId;
                ViewBag.TacticTitle = improvementTactic.Title;

                //// Modified By Maninder Singh Wadhva PL Ticket#47
                ViewBag.IsImprovement = true;
            }

            try
            {
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                ViewBag.IsServiceUnavailable = false;

                BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();

                var individuals = new List<RevenuePlanner.BDSService.User>();
                if (Sessions.User != null && Sessions.User.ClientId != null && Sessions.ApplicationId != null && Sessions.User.UserId != null) //Added by komal to check session is null for #2299
                {
                    individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);
                }
                if (individuals.Count != 0)
                {
                    ViewBag.EmailIds = individuals.Select(member => member.Email).ToList<string>();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = true;
                }
            }

            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
            {
                return PartialView("ShareTactic");
            }
            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
            {
                return PartialView("_ShareProgram");
            }
            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                return PartialView("_ShareCampaign");
            }

            return PartialView("ShareTactic");
        }
        #endregion

        /// <summary>
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="CloneType"></param>
        /// <param name="Id"></param>
        /// <param name="title"></param>
        /// <param name="CalledFromBudget"></param>
        /// <param name="RequsetedModule"></param>
        /// <param name="planid"></param>
        /// <returns></returns>
        public ActionResult Clone(string CloneType, int Id, string title, string CalledFromBudget = "", string RequsetedModule = "", int planid = 0)
        {
            int rtResult = 0;
            bool IsCampaign = (CloneType == Enums.Section.Campaign.ToString()) ? true : false;
            bool IsProgram = (CloneType == Enums.Section.Program.ToString()) ? true : false; ;
            bool IsTactic = (CloneType == Enums.Section.Tactic.ToString()) ? true : false; ;
            bool IsLineitem = (CloneType == Enums.Section.LineItem.ToString()) ? true : false; ;

            if (IsCampaign)
            {
                planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == Id && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }
            else if (IsProgram)
            {
                planid = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == Id && p.IsDeleted.Equals(false)).Select(p => p.Plan_Campaign.PlanId).FirstOrDefault();
                //planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == (db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == Id && pcp.IsDeleted.Equals(false)).Select(pcp => pcp.PlanCampaignId).FirstOrDefault()) && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }
            else if (IsTactic)
            {
                planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == (db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == (db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == Id && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt.PlanProgramId).FirstOrDefault()) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp.PlanCampaignId).FirstOrDefault()) && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }
            else if (IsLineitem)
            {
                Plan_Campaign_Program_Tactic_LineItem objPlanCampaignProgramTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.AsNoTracking().First(p => p.PlanLineItemId == Id && p.IsDeleted == false);
                if (objPlanCampaignProgramTacticLineItem != null)
                {
                    planid = objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                }
            }

            if (Sessions.User == null)
            {
                TempData["ErrorMessage"] = Common.objCached.SessionExpired;
                return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (!string.IsNullOrEmpty(CloneType) && Id > 0)
                {
                    Clonehelper objClonehelper = new Clonehelper();

                    //// Create Clone by CloneType e.g Plan,Campaign,Program,Tactic,LineItem
                    rtResult = objClonehelper.ToClone("", CloneType, Id, planid);
                    if (CloneType == Enums.DuplicationModule.Plan.ToString())
                    {
                        Plan objPlan = db.Plans.Where(p => p.PlanId == Id).FirstOrDefault();
                        title = objPlan != null ? objPlan.Title : string.Empty;
                    }
                    if (CloneType == Enums.DuplicationModule.LineItem.ToString())
                    {
                        CloneType = "Line Item";
                    }

                }

                if (rtResult >= 1)
                {
                    title = HttpUtility.HtmlDecode(title);
                    string strMessage = string.Format(Common.objCached.CloneDuplicated, CloneType);     // Modified by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                    // Add By Nishant Sheth
                    // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                    DataSet dsPlanCampProgTac = new DataSet();
                    dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                    objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                    List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                    objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                    var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                    objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                    var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                    objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                    var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                    objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                    var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                    objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);

                    if (!string.IsNullOrEmpty(CalledFromBudget))
                    {
                        TempData["SuccessMessage"] = strMessage;
                        TempData["SuccessMessageDeletedPlan"] = "";

                        string expand = CloneType.ToLower().Replace(" ", "");
                        if (expand == "campaign")
                            return Json(new { IsSuccess = true, type = CalledFromBudget, Id = rtResult, expand = expand + rtResult.ToString(), msg = strMessage });
                        else
                            return Json(new { IsSuccess = true, type = CalledFromBudget, Id = rtResult, expand = expand + rtResult.ToString(), msg = strMessage });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(RequsetedModule) && RequsetedModule == Enums.InspectPopupRequestedModules.Index.ToString())
                        {
                            return Json(new { IsSuccess = true, redirect = Url.Action("Index"), msg = strMessage, opt = Enums.InspectPopupRequestedModules.Index.ToString(), Id = rtResult });
                        }
                        else if (!string.IsNullOrEmpty(RequsetedModule) && RequsetedModule == Enums.InspectPopupRequestedModules.ApplyToCalendar.ToString())
                        {
                            TempData["SuccessMessageDeletedPlan"] = strMessage;
                            return Json(new { IsSuccess = true, msg = strMessage, redirect = Url.Action("ApplyToCalendar", "Plan"), Id = rtResult });
                        }
                        else
                        {
                            TempData["SuccessMessageDeletedPlan"] = strMessage;
                            return Json(new { IsSuccess = true, Id = rtResult, redirect = Url.Action("Assortment", "Plan"), planId = planid, opt = Enums.InspectPopupRequestedModules.Assortment.ToString(), msg = strMessage });
                        }
                    }
                }
                else
                {
                    string strErrorMessage = string.Format("{0} not duplicated.", CloneType);    // Modified by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                    return Json(new { IsSuccess = false, msg = strErrorMessage, opt = RequsetedModule });

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return Json(new { IsSuccess = false, msg = e.Message.ToString(), opt = RequsetedModule });
            }
        }

        /// <summary>
        /// Delete Plan,Tactic,Campaign,Program by Section.
        /// </summary>
        /// <param name="DeleteType"></param>
        /// <param name="id"></param>
        /// <param name="UserId"></param>
        /// <param name="closedTask"></param>
        /// <param name="CalledFromBudget"></param>
        /// <param name="IsIndex"></param>
        /// <param name="RedirectType"></param>
        /// <returns></returns>
        public ActionResult DeleteSection(int id = 0, string DeleteType = "", string UserId = "", string closedTask = null, string CalledFromBudget = "", bool IsIndex = false, bool RedirectType = false)
        {
            //// check whether UserId is currently loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        #region "Initialize Variables"
                        int returnValue = 0;
                        string Title = "";
                        string strMessage = "";
                        int cid = 0;
                        int pid = 0;
                        int tid = 0;
                        int tempLocalVariable = 0;
                        int planid = 0;
                        bool IsPlan = (DeleteType == Enums.Section.Plan.ToString()) ? true : false; // Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                        bool IsCampaign = (DeleteType == Enums.Section.Campaign.ToString()) ? true : false;
                        bool IsProgram = (DeleteType == Enums.Section.Program.ToString()) ? true : false;
                        bool IsTactic = (DeleteType == Enums.Section.Tactic.ToString()) ? true : false;
                        bool IsLineItem = (DeleteType == Enums.Section.LineItem.ToString()) ? true : false;
                        #endregion
                        //Added By Komal Rawal for LinkedLineItem change PL ticket #1853
                        Plan_Campaign_Program_Tactic_LineItem Lineitemobj = new Plan_Campaign_Program_Tactic_LineItem();
                        Plan_Campaign_Program_Tactic ObjLinkedTactic = new Plan_Campaign_Program_Tactic();
                        int? LinkedTacticId;

                        var LinkedLineitemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(linkid => linkid.PlanLineItemId == id && linkid.IsDeleted == false).Select(linkid => linkid.LinkedLineItemId).FirstOrDefault();

                        // Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                        //// Delete sections e.g Plan,Campaign,Program,Tactic,LineItem.
                        if (IsPlan)
                        {
                            planid = id;
                            returnValue = Common.PlanTaskDelete(Enums.Section.Plan.ToString(), id);
                            if (returnValue != 0)
                            {

                                var planTitle = db.Plans.Where(p => p.PlanId == id).ToList().Select(p => p.Title).FirstOrDefault();
                                returnValue = Common.InsertChangeLog(id, null, id, planTitle, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        // End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                        else if (IsCampaign)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Campaign.ToString(), id);
                            if (returnValue != 0)
                            {
                                Plan_Campaign pc = db.Plan_Campaign.Where(p => p.PlanCampaignId == id).FirstOrDefault();
                                Title = pc.Title;
                                planid = pc.PlanId;
                                returnValue = Common.InsertChangeLog(pc.PlanId, null, pc.PlanCampaignId, pc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        else if (IsProgram)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Program.ToString(), id);
                            if (returnValue != 0)
                            {
                                Plan_Campaign_Program pc = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == id).FirstOrDefault();
                                cid = pc.PlanCampaignId;
                                planid = db.Plan_Campaign.Where(p => p.PlanCampaignId == cid && p.IsDeleted.Equals(false)).Select(p => p.PlanId).FirstOrDefault();
                                Title = pc.Title;
                                returnValue = Common.InsertChangeLog(planid, null, pc.PlanProgramId, pc.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        else if (IsTactic)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Tactic.ToString(), id);

                            if (returnValue != 0)
                            {
                                Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.PlanTacticId == id).FirstOrDefault();
                                cid = pcpt.Plan_Campaign_Program.PlanCampaignId;
                                planid = db.Plan_Campaign.Where(p => p.PlanCampaignId == cid && p.IsDeleted.Equals(false)).Select(p => p.PlanId).FirstOrDefault();
                                pid = pcpt.PlanProgramId;
                                Title = pcpt.Title;
                                returnValue = Common.InsertChangeLog(planid, null, pcpt.PlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        else if (IsLineItem)
                        {


                            if (LinkedLineitemId != null)
                            {
                                Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(linkid => linkid.PlanLineItemId == LinkedLineitemId && linkid.IsDeleted == false).ToList().FirstOrDefault();


                            }
                            //else
                            //{
                            //    Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(linkid => linkid.LinkedLineItemId == id && linkid.IsDeleted == false).ToList().FirstOrDefault();
                            //    if (Lineitemobj != null)
                            //    {

                            //        LinkedLineitemId = Lineitemobj.PlanLineItemId;

                            //    }

                            //}

                            if (LinkedLineitemId > 0)
                            {
                                int returnLinkDataValue = Common.PlanTaskDelete(Enums.Section.LineItem.ToString(), Convert.ToInt32(LinkedLineitemId));
                            }


                            returnValue = Common.PlanTaskDelete(Enums.Section.LineItem.ToString(), id);
                            if (returnValue != 0)
                            {
                                Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanLineItemId == id).FirstOrDefault();
                                LinkedTacticId = pcptl.Plan_Campaign_Program_Tactic.LinkedTacticId;
                                var TblTactic = db.Plan_Campaign_Program_Tactic.Where(linkid => (linkid.PlanTacticId == LinkedTacticId || linkid.LinkedTacticId == id || linkid.PlanTacticId == id) && linkid.IsDeleted == false).ToList();
                                if (LinkedTacticId != null && LinkedTacticId > 0)
                                {

                                    ObjLinkedTactic = TblTactic.Where(linkid => linkid.PlanTacticId == LinkedTacticId && linkid.IsDeleted == false).ToList().FirstOrDefault();

                                }
                                //else
                                //{

                                //    if (Lineitemobj != null)
                                //    {
                                //        ObjLinkedTactic = Lineitemobj.Plan_Campaign_Program_Tactic;
                                //        LinkedTacticId = ObjLinkedTactic.PlanTacticId;
                                //    }

                                //}
                                //if (LinkedTacticId == null || Lineitemobj == null)
                                //{
                                //    ObjLinkedTactic = TblTactic.Where(Linkid => Linkid.LinkedTacticId == id && Linkid.IsDeleted == false).ToList().FirstOrDefault();
                                //    if (ObjLinkedTactic != null)
                                //    {
                                //        LinkedTacticId = ObjLinkedTactic.PlanTacticId;
                                //    }
                                //    else
                                //    {
                                //        ObjLinkedTactic = TblTactic.Where(Linkid => Linkid.PlanTacticId == LinkedTacticId).ToList().FirstOrDefault();
                                //    }

                                //}

                                //End
                                var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(lineitem => lineitem.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && lineitem.LineItemTypeId == null);
                                double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && lineitem.LineItemTypeId != null && lineitem.IsDeleted == false).ToList().Sum(lineitem => lineitem.Cost);
                                double LinkedtotalLineitemCost = 0;
                                if (LinkedLineitemId != null)
                                {
                                    //// Calculate TotalLineItemCost.
                                    LinkedtotalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                                }

                                if (objOtherLineItem == null)
                                {
                                    if (LinkedTacticId != null)
                                    {
                                        Plan_Campaign_Program_Tactic_LineItem objLinkedNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        objLinkedNewLineitem.PlanTacticId = Convert.ToInt32(LinkedTacticId);
                                        objLinkedNewLineitem.Title = Common.LineItemTitleDefault + ObjLinkedTactic.Title;
                                        if (ObjLinkedTactic.Cost > LinkedtotalLineitemCost)
                                        {
                                            objLinkedNewLineitem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
                                        }
                                        else
                                        {
                                            objLinkedNewLineitem.Cost = 0;
                                        }
                                        objLinkedNewLineitem.Description = string.Empty;
                                        objLinkedNewLineitem.CreatedBy = Sessions.User.UserId;
                                        objLinkedNewLineitem.CreatedDate = DateTime.Now;
                                        db.Entry(objLinkedNewLineitem).State = EntityState.Added;

                                    }
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = pcptl.Plan_Campaign_Program_Tactic.PlanTacticId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + pcptl.Plan_Campaign_Program_Tactic.Title;
                                    if (pcptl.Plan_Campaign_Program_Tactic.Cost > totalLoneitemCost)
                                    {
                                        objNewLineitem.Cost = pcptl.Plan_Campaign_Program_Tactic.Cost - totalLoneitemCost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    if (LinkedTacticId != null)
                                    {
                                        var objLinkedOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == LinkedTacticId && l.LineItemTypeId == null && l.IsDeleted == false);
                                        if (objLinkedOtherLineItem != null)
                                        {
                                            objLinkedOtherLineItem.IsDeleted = false;
                                            if (objLinkedOtherLineItem.Cost > LinkedtotalLineitemCost)
                                            {
                                                objLinkedOtherLineItem.Cost = ObjLinkedTactic.Cost - LinkedtotalLineitemCost;
                                            }
                                            else
                                            {
                                                objLinkedOtherLineItem.Cost = 0;
                                                objLinkedOtherLineItem.IsDeleted = true;
                                            }
                                            db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
                                        }

                                    }
                                    objOtherLineItem.IsDeleted = false;
                                    if (pcptl.Plan_Campaign_Program_Tactic.Cost > totalLoneitemCost)
                                    {
                                        objOtherLineItem.Cost = pcptl.Plan_Campaign_Program_Tactic.Cost - totalLoneitemCost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                cid = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                                planid = db.Plan_Campaign.Where(p => p.PlanCampaignId == cid && p.IsDeleted.Equals(false)).Select(p => p.PlanId).FirstOrDefault();
                                pid = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                                tid = pcptl.PlanTacticId;
                                Title = pcptl.Title;
                                returnValue = Common.InsertChangeLog(planid, null, pcptl.PlanLineItemId, pcptl.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);

                                if (LinkedLineitemId != null)
                                {
                                    cid = Lineitemobj.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                                    pid = Lineitemobj.Plan_Campaign_Program_Tactic.PlanProgramId;
                                    tid = Lineitemobj.PlanTacticId;
                                    Title = Lineitemobj.Title;
                                    Common.InsertChangeLog(Sessions.PlanId, null, Lineitemobj.PlanLineItemId, Lineitemobj.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);

                                }

                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                tempLocalVariable = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                            }
                        }

                        if (returnValue >= 1)
                        {
                            DataSet dsPlanCampProgTac = new DataSet();
                            dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                            objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                            List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                            objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                            var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                            objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                            var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                            objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                            var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                            objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);

                            var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                            objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                            //// Change Parent section status by ID.
                            if (IsProgram)
                            {
                                Common.ChangeCampaignStatus(cid, false);
                            }

                            if (IsTactic)
                            {
                                Common.ChangeProgramStatus(pid, false);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == pid).Select(program => program.PlanCampaignId).Single();
                                Common.ChangeCampaignStatus(PlanCampaignId, false);
                            }

                            if (IsLineItem)
                            {
                                var planProgramId = tempLocalVariable;
                                Common.ChangeProgramStatus(planProgramId, false);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == tempLocalVariable).Select(program => program.PlanCampaignId).Single();
                                Common.ChangeCampaignStatus(PlanCampaignId, false);

                                if (LinkedLineitemId != null)
                                {
                                    var LinkedplanProgramId = Lineitemobj.Plan_Campaign_Program_Tactic.PlanProgramId;
                                    Common.ChangeProgramStatus(LinkedplanProgramId, false);
                                    var LinkedPlanCampaignId = db.Plan_Campaign_Program.Where(_prgrm => _prgrm.IsDeleted.Equals(false) && _prgrm.PlanProgramId == Lineitemobj.Plan_Campaign_Program_Tactic.PlanProgramId).Select(_prgrm => _prgrm.PlanCampaignId).FirstOrDefault();
                                    Common.ChangeCampaignStatus(LinkedPlanCampaignId, false);
                                }
                            }

                            scope.Complete();

                            ViewBag.CampaignID = cid;
                            ViewBag.ProgramID = pid;
                            ViewBag.TacticID = tid;

                            if (!string.IsNullOrEmpty(CalledFromBudget))
                            {
                                TempData["SuccessMessage"] = strMessage;
                                // Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                                if (IsPlan)
                                {
                                    TempData["SuccessMessageDeletedPlan"] = strMessage;
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("PlanSelector", "Plan", new { PlanId = planid, type = CalledFromBudget }) });
                                }
                                // End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                                else if (IsCampaign)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { PlanId = planid, type = CalledFromBudget }) });
                                }
                                else if (IsProgram)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { PlanId = planid, type = CalledFromBudget }), expand = "campaign" + cid.ToString() });
                                }
                                else if (IsTactic)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { PlanId = planid, type = CalledFromBudget }), expand = "program" + pid.ToString() });
                                }
                                else if (IsLineItem)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { PlanId = planid, type = CalledFromBudget }), expand = "tactic" + tid.ToString() });
                                }
                            }
                            else if (IsIndex)
                            {
                                //Modified by Mitesh Vaishnav for PL ticket 966
                                TempData["SuccessMessageDeletedPlan"] = "";
                                return Json(new { IsSuccess = true, redirect = Url.Action("Index"), msg = strMessage, opt = Enums.InspectPopupRequestedModules.Index.ToString() });
                            }
                            else
                            {
                                TempData["SuccessMessageDeletedPlan"] = strMessage;
                                if (RedirectType)
                                {
                                    if (closedTask != null)
                                    {
                                        TempData["ClosedTask"] = closedTask;
                                    }
                                    return Json(new { IsSuccess = true, msg = strMessage, redirect = Url.Action("ApplyToCalendar", "Plan"), opt = Enums.InspectPopupRequestedModules.ApplyToCalendar.ToString() });
                                }
                                else
                                {
                                    if (IsLineItem)
                                    {
                                        return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Assortment.ToString(), redirect = Url.Action("Assortment", "Plan", new { campaignId = cid, programId = pid, tacticId = tid }) });
                                    }

                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Assortment.ToString(), redirect = Url.Action("Assortment", "Plan", new { campaignId = cid, programId = pid }) });
                                }
                            }
                        }
                        return Json(new { IsSuccess = false, msg = Common.objCached.ErrorOccured });
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSuccess = false, msg = Common.objCached.ErrorOccured });
        }

        /// <summary>
        /// Function to get Notification URL.
        /// Added By: Viral Kadiya on 12/4/2014.
        /// </summary>
        /// <param name="planId">Plan Id.</param>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="section">Section.</param>
        /// <returns>Return NotificationURL.</returns>
        public string GetNotificationURLbyStatus(int planId = 0, int planTacticId = 0, string section = "")
        {
            string strURL = string.Empty;
            try
            {
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planTacticId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planProgramId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planCampaignId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planTacticId = planTacticId, isImprovement = true, activeMenu = "Plan" }, Request.Url.Scheme);
                //modified by Rahul Shah on 09/03/2016 for PL #1939
                else if (section == Convert.ToString(Enums.Section.Plan).ToLower())
                {
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, activeMenu = "Plan" }, Request.Url.Scheme);
                }
                //Added by Rahul Shah on 17/03/2016 for PL #2068
                else if (section == Convert.ToString(Enums.Section.LineItem).ToLower())
                {
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planLineItemId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return strURL;
        }

        /// <summary>
        /// Return integration instance exist for model
        /// Added by Mitesh Vaishnav on 12/08/2014 for PL ticket #690
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public string CheckIntegrationInstanceExist(Model objModel)
        {
            string returnValue = string.Empty;

            if (objModel.IntegrationInstanceId == null && objModel.IntegrationInstanceIdCW == null && objModel.IntegrationInstanceIdINQ == null && objModel.IntegrationInstanceIdMQL == null && objModel.IntegrationInstanceIdProjMgmt == null && objModel.IntegrationInstanceEloquaId == null && objModel.IntegrationInstanceMarketoID == null) ////Modiefied by Mitesh Vaishnav on 12/08/2014 for PL ticket #690 and Brad Gray 07/23/2015 PL#1448 and Viral Kadiya 09/04/2015 PL ticket #1583.
                returnValue = "N/A";
            else
            {
                returnValue = "Yes";
            }

            return returnValue;
        }

        //Added By Komal Rawal for #1357 - To Add Comment
        public void AddComment(string status, int Id, string Section, int planid)
        {
            string approvedComment = "";
            Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
            if (Section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {

                //Modified By Komal Rawal for #1357
                var Program = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == Id && id.Status == status && id.IsDeleted == false).Select(program => program).ToList();
                foreach (var item in Program)
                {
                    approvedComment = Convert.ToString(Enums.Section.Program) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                    pcptc.Comment = approvedComment;
                    DateTime Currentdate = DateTime.Now;
                    pcptc.CreatedDate = Currentdate;
                    string DisplayDate = Currentdate.ToString("MMM dd") + " at " + Currentdate.ToString("hh:mmtt");
                    pcptc.CreatedBy = Sessions.User.UserId;
                    pcptc.PlanProgramId = item.PlanProgramId;
                    db.Entry(pcptc).State = EntityState.Added;
                    db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                    db.SaveChanges();

                }
                var Programids = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == Id && id.Status == status).Select(program => program.PlanProgramId).ToList();
                var Tactics = db.Plan_Campaign_Program_Tactic.Where(id => Programids.Contains(id.PlanProgramId)).Select(tactic => tactic).ToList();
                DateTime todaydate = DateTime.Now;
                foreach (var item in Tactics)
                {
                    if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                    {
                        item.Status = status;
                        if (todaydate > item.StartDate && todaydate < item.EndDate)
                        {
                            item.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                        }
                        else if (todaydate > item.EndDate)
                        {
                            item.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                        }
                    }
                    else
                    {
                        item.Status = status;
                    }
                    item.ModifiedBy = Sessions.User.UserId;
                    item.ModifiedDate = DateTime.Now;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();

                    approvedComment = Convert.ToString(Enums.Section.Tactic) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                    pcptc.Comment = approvedComment;
                    DateTime Currentdate = DateTime.Now;
                    pcptc.CreatedDate = Currentdate;
                    string DisplayDate = Currentdate.ToString("MMM dd") + " at " + Currentdate.ToString("hh:mmtt");
                    pcptc.CreatedBy = Sessions.User.UserId;
                    pcptc.PlanTacticId = item.PlanTacticId;
                    pcptc.PlanProgramId = null;
                    db.Entry(pcptc).State = EntityState.Added;
                    db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                    db.SaveChanges();

                    //if (isApproved)
                    //{
                    //    Common.InsertChangeLog(Sessions.PlanId, null, item.PlanTacticId, item.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                    //    if (item.IsDeployedToIntegration == true)
                    //    {
                    //        ExternalIntegration externalIntegration = new ExternalIntegration(item.PlanTacticId, Sessions.ApplicationId, new Guid(), EntityType.Tactic);
                    //        externalIntegration.Sync();
                    //    }
                    //}
                    //else 
                    if (item.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                    {
                        Common.InsertChangeLog(planid, null, item.PlanTacticId, item.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined, null);
                    }

                }


            }
            else if (Section == Convert.ToString(Enums.Section.Program).ToLower())
            {

                var Tactics = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == Id && id.Status == status && id.IsDeleted == false).Select(id => id).ToList();
                DateTime todaydate = DateTime.Now;
                foreach (var item in Tactics)
                {
                    if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                    {
                        item.Status = status;
                        if (todaydate > item.StartDate && todaydate < item.EndDate)
                        {
                            item.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                        }
                        else if (todaydate > item.EndDate)
                        {
                            item.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                        }
                    }
                    else
                    {
                        item.Status = status;
                    }
                    item.ModifiedBy = Sessions.User.UserId;
                    item.ModifiedDate = DateTime.Now;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();

                    approvedComment = Convert.ToString(Enums.Section.Tactic) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                    pcptc.Comment = approvedComment;
                    DateTime Currentdate = DateTime.Now;
                    pcptc.CreatedDate = Currentdate;
                    string DisplayDate = Currentdate.ToString("MMM dd") + " at " + Currentdate.ToString("hh:mmtt");
                    pcptc.CreatedBy = Sessions.User.UserId;
                    pcptc.PlanTacticId = item.PlanTacticId;
                    db.Entry(pcptc).State = EntityState.Added;
                    db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                    db.SaveChanges();

                    //if (isApproved)
                    //{
                    //    Common.InsertChangeLog(Sessions.PlanId, null, item.PlanTacticId, item.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                    //    if (item.IsDeployedToIntegration == true)
                    //    {
                    //        ExternalIntegration externalIntegration = new ExternalIntegration(item.PlanTacticId, Sessions.ApplicationId, new Guid(), EntityType.Tactic);
                    //        externalIntegration.Sync();
                    //    }
                    //}
                    //else 
                    if (item.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                    {
                        Common.InsertChangeLog(planid, null, item.PlanTacticId, item.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined, null);
                    }

                }


            }

        }


        // Added by Viral for #2366
        private bool IsClientMediaCodePermission()
        {
            bool isMediaCodePermission = false;
            try
            {
                if(Sessions.User.ClientId != Guid.Empty)
                {
                    int appActivityId=0;
                    BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
                    string strMediaCodeActivity = Enums.clientAcivityType.MediaCodes.ToString().ToLower();
                     var ApplicationActivityList = objBDSservice.GetClientApplicationactivitylist(Sessions.ApplicationId);
                    if(ApplicationActivityList != null && ApplicationActivityList.Count>0)
                        appActivityId=  ApplicationActivityList.Where(act => act.Code.ToLower() == strMediaCodeActivity).Select(act => act.ApplicationActivityId).FirstOrDefault();
                         //GetClientApplicationactivitylist(_applicationId);

                    if (db.Client_Activity.Any(act => act.ClientId == Sessions.User.ClientId && act.ApplicationActivityId == appActivityId))
                        isMediaCodePermission = true;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return isMediaCodePermission;
        }
        #endregion

        #region TreeGridView for dropdown
        //Added By Komal Rawal for #1617
        public JsonResult LoadBudgetDropdown(int BudgetId = 0, int PlanLineItemID = 0)
        {
            MRPEntities db = new MRPEntities();
            DhtmlXGridRowModel budgetMain = new DhtmlXGridRowModel();
            var MinParentid = 0;

            var dataTableMain = new DataTable();

            #region Old Code
            // Commented By Nishant Sheth
            // Desc ::  For Line item performance issue
            //dataTableMain.Columns.Add("Id", typeof(Int32));
            //dataTableMain.Columns.Add("ParentId", typeof(Int32));
            //dataTableMain.Columns.Add("Name", typeof(String));
            //dataTableMain.Columns.Add("Weightage", typeof(String));

            //List<int> BudgetDetailsIds = db.Budgets.Where(a => a.ClientId == Sessions.User.ClientId).Select(a => a.Id).ToList();
            //List<Budget_Detail> BudgetDetailList = db.Budget_Detail.Where(a => a.BudgetId == (BudgetId > 0 ? BudgetId : a.BudgetId) && BudgetDetailsIds.Contains(a.BudgetId) && a.IsDeleted == false).Select(a => a).ToList();
            //List<int> BudgetDetailids = BudgetDetailList.Select(a => a.Id).ToList();
            //modified by Rahul shah on 22/04/2016 for code refactoring.
            //List<LineItem_Budget> SelectedLineItemBudgetNew = db.LineItem_Budget.Select(a => a).ToList();
            //List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            //List<int> LineItemids = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();

            //var Query = BudgetDetailList.Select(a => new { a.Id, a.ParentId, a.Name }).ToList();
            //BudgetAmount objBudgetAmount;

            //foreach (var item in Query)
            //{

            //    objBudgetAmount = new BudgetAmount();
            //    //List<int> PlanLineItemsId = LineItemidBudgetList.Where(a => a.BudgetDetailId == item.Id).Select(a => a.PlanLineItemId).ToList();
            //    dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name });
            //}
            #endregion

            // Add By Nishant Sheth
            // Desc :: For performance issues #2128
            DataSet dsBudgetItems = new DataSet();
            dsBudgetItems = objSp.GetBudgetListAndLineItemBudgetList(BudgetId);
            dataTableMain = dsBudgetItems.Tables[0];
            List<Custom_LineItem_Budget> LineItemidBudgetList = Common.GetSpLineItemBudgetList(dsBudgetItems.Tables[1]);
            // End By Nishant Sheth            

            var items = GetTopLevelRows(dataTableMain, MinParentid)
                        .Select(row => CreateItem_New(dataTableMain, row, PlanLineItemID, LineItemidBudgetList))
                        .ToList();

            budgetMain.rows = items;
            return Json(new { data = budgetMain, ListOfCheckedItem = ListbudgetCheckedItem }, JsonRequestBehavior.AllowGet);
        }

        IEnumerable<DataRow> GetTopLevelRows(DataTable dataTable, int minParentId = 0)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Int32>("ParentId") == minParentId);
        }
        IEnumerable<DataRow> GetChildren(DataTable dataTable, Int32 parentId)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Int32>("ParentId") == parentId);
        }
        DhtmlxGridRowDataModel CreateItem(DataTable dataTable, DataRow row, int PlanLineItemID)
        {
            var enableCheck = string.Empty;
            var value = string.Empty;
            var id = row.Field<Int32>("Id");
            var name = row.Field<String>("Name");
            var weightage = row.Field<String>("Weightage");
            List<LineItem_Budget> SelectedLineItemBudget = db.LineItem_Budget.Where(a => a.BudgetDetailId == id && a.PlanLineItemId == PlanLineItemID).Select(a => a).ToList();
            int SelectedID = SelectedLineItemBudget.Select(a => a.BudgetDetailId).FirstOrDefault();
            var SelectedWeightage = SelectedLineItemBudget.Select(a => a.Weightage).FirstOrDefault();

            if (id == SelectedID)
            {
                enableCheck = "checked=\"checked\"";
                value = SelectedWeightage.ToString();
            }
            else
            {
                enableCheck = string.Empty;
                value = string.Empty;
            }
            var temp = "<input id=" + id + " title='" + name + "' " + enableCheck + "  type=checkbox  />" + name;
            var AddWeightage = " <input value='" + value + "' type='text'  id= wt_" + id + " align='center' style='margin-top:9px; padding-right:4px;'>";

            List<string> datalist = new List<string>();

            var children = GetChildren(dataTable, id)
              .Select(r => CreateItem(dataTable, r, PlanLineItemID))
              .ToList();

            var item = children.Count > 0 ? name : temp;
            weightage = children.Count > 0 ? "" : AddWeightage;
            datalist.Add("<input  type=checkbox /><span>" + item != null ? Convert.ToString(item) : "No" + "</span>" + name);
            datalist.Add(weightage);
            return new DhtmlxGridRowDataModel { id = Convert.ToString(id), data = datalist, rows = children };

        }
        DhtmlxGridRowDataModel CreateItem_New(DataTable dataTable, DataRow row, int PlanLineItemID, List<Custom_LineItem_Budget> SelectedLineItemBudget)
        {
            var enableCheck = string.Empty;
            var value = string.Empty;
            var id = row.Field<Int32>("Id");
            var name = row.Field<String>("Name");
            var weightage = row.Field<String>("Weightage");
            //Modified by Maitri Gandhi on 22/3/2016 for #2076
            List<Custom_LineItem_Budget> _SelectedLineItemBudget = new List<Custom_LineItem_Budget>();
            _SelectedLineItemBudget = SelectedLineItemBudget.Where(a => a.BudgetDetailId == id && a.PlanLineItemId == PlanLineItemID).Select(a => a).ToList();
            int SelectedID = _SelectedLineItemBudget.Select(a => a.BudgetDetailId).FirstOrDefault();
            var SelectedWeightage = _SelectedLineItemBudget.Select(a => a.Weightage).FirstOrDefault();
            string textboxclass = string.Empty;

            if (id == SelectedID)
            {
                enableCheck = "checked=\"checked\"";
                value = SelectedWeightage.ToString();
                // Add By Nishant Sheth
                // #2325 :: to add class on textbox for highlight
                textboxclass = "class=\"mappingtextboxblue\"";
                ListbudgetCheckedItem.Add(new BudgetCheckedItem { Id = Convert.ToString(SelectedID), Title = name, Values = value });
            }
            else
            {
                enableCheck = string.Empty;
                value = string.Empty;
                textboxclass = string.Empty;
            }
            // Modified By Nishant Sheth
            // #2325 :: add on click event for check uncheck box of budget dropdown
            var temp = "<input id=" + id + " title='" + name + "' " + enableCheck + "  type=checkbox  onclick='" + string.Format("BudgetChekBoxClick({0})", Convert.ToString(id)) + "'/>" + name; 
            var AddWeightage = " <input value='" + value + "' type='text' alt_id = 'txtweight'  id= wt_" + id + " align='center' style='margin-top:9px; padding-right:4px;' " + textboxclass + ">";

            List<string> datalist = new List<string>();

            var children = GetChildren(dataTable, id)
              .Select(r => CreateItem_New(dataTable, r, PlanLineItemID, SelectedLineItemBudget))
              .ToList();

            var item = children.Count > 0 ? name : temp;
            weightage = children.Count > 0 ? "" : AddWeightage;
            datalist.Add("<input  type=checkbox /><span>" + item != null ? Convert.ToString(item) : "No" + "</span>" + name);
            datalist.Add(weightage);
            return new DhtmlxGridRowDataModel { id = Convert.ToString(id), data = datalist, rows = children };

        }
        #endregion

        #region Media code related Functions and methods #2290
        #region Method to load mediacode for tactic
        public PartialViewResult LoadMediaCodeFromTacticPopup(int tacticId, string InsepectMode, bool IsPlanCreateAll = false, bool IsUnarchive = false, string MediaCodeId = "0")
        {
          
            Plangrid objplangrid = new Plangrid();
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            ViewBag.TacticID = Convert.ToString(tacticId);
            bool IsArchive = true;
            try
            {
                if (IsUnarchive)
                {
                    IsArchive = ArchiveUnarchiveMediaCode(MediaCodeId, tacticId, false);

                }

                //list of custom fields for particular Tactic
                //  List<CustomFieldModel> customFieldList = Common.GetCustomFields(tacticId, section, Status);
                var mainlist = db.Tactic_MediaCodes.Where(a => a.TacticId == tacticId && a.IsDeleted==false).ToList();
                List<TacticMediaCodeCustomField> MediaCodecustomFieldList = mainlist.Select(a => new TacticMediaCodeCustomField
                    {
                        TacticId = a.TacticId,

                        MediaCodeId = a.MediaCodeId,
                        MediaCode = a.MediaCode,
                        CustomFieldList = a.Tactic_MediaCodes_CustomFieldMapping.Where(aa => aa.TacticId == a.TacticId).Select(aa => new CustomeFieldList
                        {
                            CustomFieldId = aa.CustomFieldId != null ? Convert.ToInt32(aa.CustomFieldId) : 0,
                            CustomFieldValue = aa.CustomFieldValue

                        }).ToList()

                    }).ToList();

                var lstmediaCodeCustomfield = db.MediaCodes_CustomField_Configuration.Where(a => a.ClientId == Sessions.User.ClientId).ToList().Select(a => new TacticCustomfieldConfig
                {
                    CustomFieldId = a.CustomFieldId,
                    CustomFieldName = a.CustomField.Name,
                    CustomFieldTypeName = a.CustomField.CustomFieldType.Name,
                    IsRequired = a.CustomField.IsRequired,
                    Sequence=a.Sequence,
                    Option = a.CustomField.CustomFieldOptions.Select(opt => new CustomFieldOptionList
                    {
                        CustomFieldOptionId = opt.CustomFieldOptionId,
                        CustomFieldOptionValue = opt.Value
                    }).ToList()
                }).OrderBy(a=>a.Sequence).ToList();
                if (MediaCodecustomFieldList.Count != 0 && lstmediaCodeCustomfield.Count!=0)
                {
                    objplangrid = LoadAllMediacodeGrid(tacticId, MediaCodecustomFieldList, lstmediaCodeCustomfield, InsepectMode, false);
                }
                        else
                {
                    if (InsepectMode == Convert.ToString(Enums.InspectPopupMode.Edit) && lstmediaCodeCustomfield.Count != 0)
                    {
                        objPlanMainDHTMLXGrid = AddNewRow(lstmediaCodeCustomfield, tacticId);
                objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
                    }
                    else
                        objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
                }
                    if (lstmediaCodeCustomfield.Count == 0)
                        ViewBag.NoCustomField = true;
                else
                    ViewBag.CustomFieldCount = lstmediaCodeCustomfield.Count;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return PartialView("_TacticMediaCode", objplangrid);
        }
        #endregion
       

        #region method to generate blank/new row for media code

        public PlanMainDHTMLXGrid AddNewRow(List<TacticCustomfieldConfig> customfieldlist, int TacticID)
        {
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            List<PlanHead> headobjlist = new List<PlanHead>();
            List<PlanDHTMLXGridDataModel> lineitemrowsobjlist = new List<PlanDHTMLXGridDataModel>();
            PlanDHTMLXGridDataModel lineitemrowsobj = new PlanDHTMLXGridDataModel();
            string Gridheder = string.Empty;
            string coltype = string.Empty;
            string initwidth = string.Empty;
            string colsorttype = string.Empty;
            int cnt = 0;
            bool isRequire = false;
            List<PlanOptions> viewoptionlist = new List<PlanOptions>();
            try
            {
                lineitemrowsobj = new PlanDHTMLXGridDataModel();
                lineitemrowsobj.id = "mediacode." + cnt;
                List<Plandataobj> lineitemdataobjlist = new List<Plandataobj>();
                Plandataobj lineitemdataobj = new Plandataobj();

                // add other button
                PlanHead headobjother = new PlanHead();
                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "MediacodeId";
                headobjother.sort = "na";
                headobjother.width = 0;
                headobjother.value = "";

                headobjlist.Add(headobjother);

               

                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "selectall";
                headobjother.sort = "na";
                headobjother.width = 100;
                headobjother.value = "<input type='checkbox' value='SelectAll'  title='SelectAll' onchange='SelectAllHoneyComb();' class='selectInput'><span class='selectall'> Select All</span></input>";
                // headobjother.value = "<input type='checkbox' title='Select All'/>";
                headobjlist.Add(headobjother);

                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "generateMediaCode";
                headobjother.sort = "na";
                headobjother.width = 100;
                headobjother.value = "Media Code";
                headobjlist.Add(headobjother);

                lineitemdataobj = new Plandataobj();
                lineitemdataobj.value = "0";
                lineitemdataobjlist.Add(lineitemdataobj);

                lineitemdataobj = new Plandataobj();
                lineitemdataobj.value = "<span alt=" + TacticID.ToString() + " class='plus-circle disabled' title='Add Media Code'><i class='fa fa-plus-circle CodeNew' title='Add Media Code' aria-hidden='true' rowid=mediacode." + cnt + " onclick='javascript:OpenGridPopup(event)'></i></span><div class='honeycombbox-icon-gantt disabled' title='Select' rowid=mediacode." + cnt + " ></div><span class='archive disabled' title='Archive'><i class='fa fa-archive CodeArchive' title='Archive' aria-hidden='true' rowid=mediacode." + cnt + " ></i></span>";
                lineitemdataobjlist.Add(lineitemdataobj);

                lineitemdataobj = new Plandataobj();
                lineitemdataobj.value = "<input type='button' value='Generate'  class='GenerateMediaCode btn' rowid='mediacode." + cnt + "' onclick='javascript:GenerateMediaCode(this)' disabled='disabled'/>";
                lineitemdataobjlist.Add(lineitemdataobj);

                // end
                foreach (var item in customfieldlist)
                {

                    cnt = cnt + 1;
                    Gridheder = item.CustomFieldName;
                    if (item.CustomFieldTypeName == Convert.ToString(Enums.CustomFieldType.TextBox))
                    {
                        coltype = "ed";

                        isRequire = item.IsRequired;

                    }
                    else if (item.CustomFieldTypeName == Convert.ToString(Enums.CustomFieldType.DropDownList))
                    {

                        coltype = "coro";
                        viewoptionlist = item.Option.Select(a => new PlanOptions
                       {
                           id = Convert.ToString(a.CustomFieldOptionId),
                           value = a.CustomFieldOptionValue
                       }).ToList();

                        isRequire = item.IsRequired;

                    }



                    PlanHead headobj = new PlanHead();
                    headobj.type = coltype;
                    headobj.id = "customfield_" + item.CustomFieldId;
                    headobj.sort = "na";
                    headobj.width = 200;
                    headobj.value = Gridheder;
                    if (viewoptionlist != null && viewoptionlist.Count > 0)
                        headobj.options = viewoptionlist;
                    headobjlist.Add(headobj);


                    lineitemdataobj = new Plandataobj();

                    lineitemdataobj.value = "--";
                    lineitemdataobj.actval = Convert.ToString(isRequire);

                    lineitemdataobjlist.Add(lineitemdataobj);


                }

                lineitemrowsobj.data = lineitemdataobjlist;
                lineitemrowsobjlist.Add(lineitemrowsobj);
                objPlanMainDHTMLXGrid.head = headobjlist;
                objPlanMainDHTMLXGrid.rows = lineitemrowsobjlist;

            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return objPlanMainDHTMLXGrid;
        }
        #endregion

        #region Method to generate media code and save media code customfield values
        [HttpPost]
        public JsonResult GenerateMediaCode(string TacticId, List<CustomFieldValue> lstcustomfieldvalue)
        {
            string NewMediacode = string.Empty;
            bool IsValid = true;
            List<TacticMediaCodeModel> lstMediaCodeCustomField = new List<TacticMediaCodeModel>();
            try
            {
                List<int> lstcustomfieldid = lstcustomfieldvalue.Select(a => a.CustomFieldId).ToList();
                List<MediaCodes_CustomField_Configuration> lstmediacodecustomfield = db.MediaCodes_CustomField_Configuration.Where(a => a.ClientId == Sessions.User.ClientId).ToList();
                List<CustomFieldOption> lstCustomFieldOption = db.CustomFieldOptions.Where(a => lstcustomfieldid.Contains(a.CustomFieldId)).ToList();
                if (lstcustomfieldvalue != null && lstcustomfieldvalue.Count > 0)
                {
                    foreach (CustomFieldValue item in lstcustomfieldvalue)
                    {
                        TacticMediaCodeModel objmediacodecustomField = new TacticMediaCodeModel();
                        var length = lstmediacodecustomfield.Where(a => a.CustomFieldId == item.CustomFieldId).Select(a => a.Length).FirstOrDefault();
                        if (item.CustomFieldType == Convert.ToString(Enums.CustomFieldType.TextBox))
                        {
                            if (length != null && Convert.ToString(item.CustomFieldOptionValue).Trim().Length > length)
                                NewMediacode = NewMediacode + Convert.ToString(item.CustomFieldOptionValue).Trim().Substring(0, Convert.ToInt32(length)) + '_';
                            else
                                NewMediacode = NewMediacode + Convert.ToString(item.CustomFieldOptionValue).Trim() + '_';
                        }
                        else if (item.CustomFieldType == Convert.ToString(Enums.CustomFieldType.DropDownList))
                        {
                            var customfieldvalue = Convert.ToInt32(item.CustomFieldOptionValue);
                            var optionvalue = lstCustomFieldOption.Where(a => a.CustomFieldOptionId == customfieldvalue).Select(a => a.Value).FirstOrDefault();
                            if (length != null && Convert.ToString(item.CustomFieldOptionValue).Trim().Length > length)
                                NewMediacode = NewMediacode + optionvalue.ToString().Trim().Substring(0, Convert.ToInt32(length)) + '_';
                            else
                                NewMediacode = NewMediacode + optionvalue.ToString().Trim() + '_';
                        }
                        objmediacodecustomField.CustomFieldId = item.CustomFieldId;
                        objmediacodecustomField.CustomFieldValue = item.CustomFieldOptionValue;
                        objmediacodecustomField.TacticId = Convert.ToInt32(TacticId);
                        lstMediaCodeCustomField.Add(objmediacodecustomField);
                    }
                    NewMediacode = NewMediacode.TrimEnd('_');
                    IsValid = IsValidMediaCode(NewMediacode, Convert.ToInt32(TacticId));
                    if (IsValid)
                    {
                        Tactic_MediaCodes objMediaCode = new Tactic_MediaCodes();
                        objMediaCode.CreatedBy = Sessions.User.UserId;
                        objMediaCode.CreatedDate = DateTime.Now;
                        objMediaCode.IsDeleted = false;
                        objMediaCode.MediaCode = NewMediacode;
                        objMediaCode.TacticId = Convert.ToInt32(TacticId);
                        db.Entry(objMediaCode).State = EntityState.Added;
                        int result = db.SaveChanges();
                        int MediaCodeId = objMediaCode.MediaCodeId;
                        if (result > 0)
                        {

                            foreach (var CustomField in lstMediaCodeCustomField)
                            {
                                CustomField.MediaCodeId = MediaCodeId;
                                Tactic_MediaCodes_CustomFieldMapping objmediaCodeCustomField = new Tactic_MediaCodes_CustomFieldMapping();
                                objmediaCodeCustomField.CustomFieldId = CustomField.CustomFieldId;
                                objmediaCodeCustomField.CustomFieldValue = CustomField.CustomFieldValue;
                                objmediaCodeCustomField.MediaCodeId = MediaCodeId;
                                objmediaCodeCustomField.TacticId = Convert.ToInt32(TacticId);
                                db.Entry(objmediaCodeCustomField).State = EntityState.Added;
                            }
                            int finalresult = db.SaveChanges();
                            if (finalresult > 0)
                                return Json(new { Success = true, MediaCode = NewMediacode.ToString(), SuccessMessage = Common.objCached.SuccessMediacode, NewMediaCodeId = MediaCodeId });
                            else
                                return Json(new { Success = false });

                        }
                        else
                            return Json(new { Success = false });
                    }
                    else
                        return Json(new { Success = true, ErrorMessage = Common.objCached.DuplicateMediacode });
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return Json(new { Success = true });
        }
        #endregion

        #region method to validate Media code with existing media code
        public bool IsValidMediaCode(string MediaCode, int TacticId)
        {
            int result = 0;
            try
            {
                DataTable dtLogDetails = new DataTable();
                ///If connection is closed then it will be open
                var Connection = db.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();

                SqlCommand command = new SqlCommand("SP_CheckExisting_MediaCode", Connection);

                try
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ClientId", Sessions.User.ClientId);
                    command.Parameters.AddWithValue("@MediaCode", MediaCode);
                    //Add the output parameter to the command object
                    SqlParameter outPutParameter = new SqlParameter();
                    outPutParameter.ParameterName = "@IsExists";
                    outPutParameter.SqlDbType = System.Data.SqlDbType.Int;
                    outPutParameter.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outPutParameter);

                    command.ExecuteNonQuery();

                    result = Convert.ToInt16(outPutParameter.Value.ToString());

                    if (result == 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                    return false;
                }


            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                return false;
            }


        }
        #endregion

        #region  Method to generate Media Code Header
        public List<PlanHead> GenerateHeader(List<TacticCustomfieldConfig> lstmediaCodeCustomfield, string Mode, bool IsArchive = false)
        {
            List<PlanHead> headobjlist = new List<PlanHead>();
            string coltype = string.Empty;
            List<PlanOptions> viewoptionlist = new List<PlanOptions>();
            string mode = Mode;
            try
            {
                PlanHead headobjother = new PlanHead();
                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "MediacodeId";
                headobjother.sort = "na";
                headobjother.width = 0;
                headobjother.value = "Id";
                headobjlist.Add(headobjother);

               
                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "selectall";
                headobjother.sort = "na";
                headobjother.width = 100;
                headobjother.value = "<input type='checkbox' value='SelectAll'  title='SelectAll' onchange='SelectAllHoneyComb();' class='selectInput'><span class='selectall'> Select All</span></input>";
                headobjlist.Add(headobjother);

                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "generateMediaCode";
                headobjother.sort = "na";
                headobjother.width = 200;
                headobjother.value = "Media Code";

                headobjlist.Add(headobjother);

                foreach (var Cust in lstmediaCodeCustomfield)
                {
                    if (Convert.ToString(Cust.CustomFieldTypeName) == Convert.ToString(Enums.CustomFieldType.TextBox))
                    {
                        if (mode == Convert.ToString(Enums.InspectPopupMode.Edit))
                            coltype = "ed";
                        else
                            coltype = "ro";
                        if (IsArchive)
                            coltype = "ro";

                    }
                    else if (Convert.ToString(Cust.CustomFieldTypeName) == Convert.ToString(Enums.CustomFieldType.DropDownList))
                    {
                        if (IsArchive == false)
                        {

                            coltype = "coro";


                        if (Cust.Option != null && Cust.Option.Count > 0)
                            viewoptionlist = Cust.Option.Select(a => new PlanOptions
                            {
                                id = Convert.ToString(a.CustomFieldOptionId),
                                value = a.CustomFieldOptionValue
                            }).ToList();
                        }
                        else
                        {
                            coltype = "ro";

                        }
                    }


                    PlanHead headobj = new PlanHead();
                    headobj.type = coltype;
                    headobj.id = "customfield_" + Cust.CustomFieldId;
                    headobj.sort = "str";
                    headobj.width = 150;
                    headobj.value = Cust.CustomFieldName;

                    if (viewoptionlist != null && viewoptionlist.Count > 0)
                        headobj.options = viewoptionlist;
                    headobjlist.Add(headobj);
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return headobjlist;

        }
        #endregion

        #region Method to archive mediacode and bind Archive media code list
        public PartialViewResult ArchiveMediaCode(int tacticId, string MediaCodeId, string Mode)
        {
            List<PlanHead> headobjlist = new List<PlanHead>();
            List<PlanDHTMLXGridDataModel> lineitemrowsobjlist = new List<PlanDHTMLXGridDataModel>();
            PlanDHTMLXGridDataModel lineitemrowsobj = new PlanDHTMLXGridDataModel();
            PlanController objPlanController = new PlanController();
            Plangrid objplangrid = new Plangrid();
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            string mode = Enums.InspectPopupMode.ReadOnly.ToString();
            string customFieldEntityValue = string.Empty;
            string DropdowList = Enums.CustomFieldType.DropDownList.ToString();
            try
            {

                ArchiveUnarchiveMediaCode(MediaCodeId, tacticId, true);
                #region bind list archive mediacode
                var mainlist = db.Tactic_MediaCodes.Where(a => a.TacticId == tacticId && a.IsDeleted == true).ToList();
                List<TacticMediaCodeCustomField> MediaCodecustomFieldList = mainlist.Select(a => new TacticMediaCodeCustomField
                {
                    TacticId = a.TacticId,

                    MediaCodeId = a.MediaCodeId,
                    MediaCode = a.MediaCode,
                    CustomFieldList = a.Tactic_MediaCodes_CustomFieldMapping.Where(aa => aa.TacticId == a.TacticId).Select(aa => new CustomeFieldList
                    {
                        CustomFieldId = aa.CustomFieldId != null ? Convert.ToInt32(aa.CustomFieldId) : 0,
                        CustomFieldValue = aa.CustomFieldValue

                    }).ToList()

                }).ToList();


                var lstmediaCodeCustomfield = db.MediaCodes_CustomField_Configuration.Where(a => a.ClientId == Sessions.User.ClientId).ToList().Select(a => new TacticCustomfieldConfig
                {
                    CustomFieldId = a.CustomFieldId,
                    CustomFieldName = a.CustomField.Name,
                    CustomFieldTypeName = a.CustomField.CustomFieldType.Name,
                    IsRequired = a.CustomField.IsRequired,
                    Sequence = a.Sequence,
                    Option = a.CustomField.CustomFieldOptions.Select(opt => new CustomFieldOptionList
                    {
                        CustomFieldOptionId = opt.CustomFieldOptionId,
                        CustomFieldOptionValue = opt.Value
                    }).ToList()
                }).OrderBy(a => a.Sequence).ToList();

                if (MediaCodecustomFieldList.Count != 0)
                {
                    objplangrid = LoadAllMediacodeGrid(tacticId, MediaCodecustomFieldList, lstmediaCodeCustomfield, Mode, true);
                }


                #endregion

            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return PartialView("_ArchiveMediaCode", objplangrid);

        }
        #endregion

        #region code for making active code to archive media code
        public bool ArchiveUnarchiveMediaCode(string MediaCodeId, int tacticId, bool Isarchive)
        {
            bool isarchived = false;
            try
            {
                if (MediaCodeId != null && MediaCodeId != "0" && MediaCodeId != "")
                {
                    int CodeId = Convert.ToInt32(MediaCodeId);
                    var MediacodeObj = db.Tactic_MediaCodes.Where(a => a.TacticId == tacticId && a.MediaCodeId == CodeId).FirstOrDefault();
                    if (MediacodeObj != null)
                    {

                        if (Isarchive)
                            MediacodeObj.IsDeleted = true;
                        else
                        {
                            string MediaCode = MediacodeObj.MediaCode;
                            bool IsValid = IsValidMediaCode(MediaCode, tacticId);
                            if (IsValid)
                            {
                                MediacodeObj.IsDeleted = false;
                            }
                            else
                            {
                                var obj = db.Tactic_MediaCodes.Where(a => a.MediaCode == MediaCode && a.TacticId == tacticId && a.IsDeleted == false).FirstOrDefault();
                                var mediacodeid = obj.MediaCodeId;
                                db.Entry(obj).State = EntityState.Deleted;
                                var objmediacodecustomfield = db.Tactic_MediaCodes_CustomFieldMapping.Where(a => a.MediaCodeId == mediacodeid).ToList();
                                foreach (var item in objmediacodecustomfield)
                                    db.Entry(item).State = EntityState.Deleted;
                                db.SaveChanges();
                                if (Isarchive)
                                    MediacodeObj.IsDeleted = false;
                                else
                                    MediacodeObj.IsDeleted = true;
                            }
                        }
                        db.Entry(MediacodeObj).State = EntityState.Modified;
                        int result = db.SaveChanges();
                        if (result > 0)
                            isarchived = true;
                    }
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return isarchived;
        }
        #endregion

        #region method to load all mediacode grid (i.e archive media code/active Media code)
        public Plangrid LoadAllMediacodeGrid(int tacticId, List<TacticMediaCodeCustomField> MediaCodecustomFieldList, List<TacticCustomfieldConfig> lstmediaCodeCustomfield, string InsepectMode, bool IsArchive = false)
        {
            string mode = InsepectMode;
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
                if (MediaCodecustomFieldList.Count != 0)
                {

                    bool isRequire = false;

                    string Gridheder = string.Empty;
                    string coltype = string.Empty;

                    if (IsArchive)
                        headobjlist = GenerateHeader(lstmediaCodeCustomfield, mode, true);
                    else
                        headobjlist = GenerateHeader(lstmediaCodeCustomfield, mode);

                    foreach (var item in MediaCodecustomFieldList)
                    {
                        string RowID = "mediacode." + item.MediaCodeId;
                        lineitemrowsobj = new PlanDHTMLXGridDataModel();
                        List<Plandataobj> lineitemdataobjlist = new List<Plandataobj>();


                        lineitemrowsobj.id = RowID;

                        Plandataobj lineitemdataobj = new Plandataobj();
                        lineitemdataobj = new Plandataobj();
                        lineitemdataobj.value = item.MediaCodeId.ToString();
                        lineitemdataobjlist.Add(lineitemdataobj);


                        if (IsArchive == false)
                        {
                            lineitemdataobj = new Plandataobj();
                            if (mode == Convert.ToString(Enums.InspectPopupMode.Edit))
                                lineitemdataobj.value = "<span alt=" + Convert.ToString(tacticId) + " class='plus-circle' title='Add Media Code'><i class='fa fa-plus-circle CodeNew' aria-hidden='true' rowid=" + RowID + " onclick='javascript:OpenGridPopup(event)' title='Add Media Code'></i></span><div class='honeycombbox-icon-gantt' rowid=" + RowID + " onclick='javascript:AddRemoveMediaCode(this);' title='Select' altid=" + item.MediaCodeId.ToString() + "></div><span class='archive' title='Archive' ><i class='fa fa-archive CodeArchive' aria-hidden='true' rowid=" + RowID + " onclick='javascript:ArchiveMediaCode(event)' title='Archive'></i></span>";
                            else
                                lineitemdataobj.value = "<span alt=" + Convert.ToString(tacticId) + " class='plus-circle disabled' title='Add Media Code'><i class='fa fa-plus-circle CodeNew' aria-hidden='true' rowid=" + RowID + " title='Add Media Code'></i></span><div class='honeycombbox-icon-gantt' rowid=" + RowID + " onclick='javascript:AddRemoveMediaCode(this);' title='Select' altid=" + item.MediaCodeId.ToString() + "></div><span class='archive disabled' title='Archive'><i class='fa fa-archive CodeArchive' aria-hidden='true' rowid=" + RowID + " title='Archive'></i></span>";

                            lineitemdataobjlist.Add(lineitemdataobj);
                        }
                        else
                        {
                            lineitemdataobj = new Plandataobj();
                            if (mode == Convert.ToString(Enums.InspectPopupMode.Edit))
                                lineitemdataobj.value = "<span class='archive' title='Unarchive'><i class='fa fa-archive UnarchiveMediaCode' aria-hidden='true' rowid=" + RowID + " onclick='javascript:UnarchiveMediaCode(event)' title='Unarchive'></i></span>";
                            else
                                lineitemdataobj.value = "<span class='archive disabled' title='Unarchive'><i class='fa fa-archive UnarchiveMediaCode' aria-hidden='true' rowid=" + RowID + " title='Unarchive'/>";

                            lineitemdataobjlist.Add(lineitemdataobj);
                        }

                        lineitemdataobj = new Plandataobj();
                        lineitemdataobj.value = "<span class='spnMediacode'>"+item.MediaCode+"</span>";
                        lineitemdataobj.actval = item.MediaCode;
                        lineitemdataobjlist.Add(lineitemdataobj);
                       

                        foreach (var Cust in lstmediaCodeCustomfield)
                        {
                            if (Cust.CustomFieldTypeName.ToString() == Convert.ToString(Enums.CustomFieldType.TextBox))
                            {
                                if (mode == Convert.ToString(Enums.InspectPopupMode.Edit))
                                {
                                    coltype = "ed";
                                }
                                else
                                {
                                    coltype = "ro";
                                }

                                isRequire = Cust.IsRequired;


                                if (IsArchive)
                                    coltype = "ro";

                                var optionValue = item.CustomFieldList.Where(o => o.CustomFieldId == Cust.CustomFieldId).Select(o => o.CustomFieldValue).FirstOrDefault();
                                customFieldEntityValue = (optionValue != null) ? optionValue.Replace("\"", "&quot;") : string.Empty;

                            }
                            else if (Convert.ToString(Cust.CustomFieldTypeName) == Convert.ToString(Enums.CustomFieldType.DropDownList))
                            {
                                if (IsArchive == false)
                                {

                                    coltype = "coro";

                                    var optionValue = item.CustomFieldList.Where(o => o.CustomFieldId == Cust.CustomFieldId).Select(o => o.CustomFieldValue).FirstOrDefault();
                                    customFieldEntityValue = (optionValue != null) ? optionValue.Replace("\"", "&quot;") : string.Empty;
                                    isRequire = Cust.IsRequired;

                                }
                                else
                                {
                                    coltype = "ro";

                                    var objoptionID = item.CustomFieldList.Where(o => o.CustomFieldId == Cust.CustomFieldId).FirstOrDefault();
                                    if (objoptionID != null)
                                    {
                                        int intoptionID = Convert.ToInt32(objoptionID.CustomFieldValue);
                                        string OptionValue = Cust.Option.Where(a => a.CustomFieldOptionId == intoptionID).Select(a => a.CustomFieldOptionValue).FirstOrDefault();
                                        customFieldEntityValue = (OptionValue != null) ? OptionValue.Replace("\"", "&quot;") : string.Empty;
                                    }
                                    else
                                        customFieldEntityValue = string.Empty;
                                }
                            }

                            lineitemdataobj = new Plandataobj();
                            if (!string.IsNullOrEmpty(customFieldEntityValue))
                                lineitemdataobj.value = HttpUtility.HtmlEncode(customFieldEntityValue);
                            else
                                lineitemdataobj.value = "--";
                            lineitemdataobj.locked = "1";
                            lineitemdataobj.actval = Convert.ToString(isRequire);
                            lineitemdataobjlist.Add(lineitemdataobj);
                        }
                        lineitemrowsobj.data = lineitemdataobjlist;
                        lineitemrowsobjlist.Add(lineitemrowsobj);

                    }


                    objPlanMainDHTMLXGrid.head = headobjlist;
                    objPlanMainDHTMLXGrid.rows = lineitemrowsobjlist;

                }

                objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return objplangrid;
        }
        #endregion
        #endregion
    }
}
