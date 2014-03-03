using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Transactions;
using System.Data.Objects;



namespace RevenuePlanner.Controllers
{
    public class PlanController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();

        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;
        #endregion

        #region List

        #endregion

        #region Create
        /// <summary>
        /// Function to create Plan
        /// </summary>
        /// <returns></returns>
        /// added id parameter by kunal on 01/17/2014 for edit plan
        public ActionResult Create(int id = 0)
        {
            PlanModel objPlanModel = new PlanModel();
            try
            {
                ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;


                var List = GetModelName();
                if (List == null || List.Count == 0)
                {
                    return RedirectToAction("NoModel");
                }
                TempData["selectList"] = new SelectList(List, "ModelId", "ModelTitle");
                /* added by Nirav Shah 12/20/2013*/
                List<int> Listyear = new List<int>();
                int yr = DateTime.Now.Year;
                for (int i = 0; i < 5; i++)
                    Listyear.Add(yr + i);
                var year = Listyear;
                TempData["selectYearList"] = new SelectList(year);
                /*end*/

                //added by kunal to fill the plan data in edit mode - 01/17/2014
                if (id != 0)
                {
                    var objplan = db.Plans.Where(m => m.PlanId == Sessions.PlanId && m.IsDeleted == false).FirstOrDefault();
                    objPlanModel.PlanId = objplan.PlanId;
                    objPlanModel.ModelId = objplan.ModelId;
                    objPlanModel.Title = objplan.Title;
                    objPlanModel.Year = objplan.Year;
                    objPlanModel.MQls = Convert.ToString(objplan.MQLs);
                    objPlanModel.Budget = objplan.Budget;
                    objPlanModel.Version = objplan.Version;
                    objPlanModel.ModelTitle = objplan.Model.Title + " " + objplan.Model.Version;
                    #region "In edit mode, plan year might be of previous year. We included previous year"
                    int planYear = 0; //plan's year
                    int.TryParse(objplan.Year, out planYear);
                    if (planYear != 0 && planYear < yr)
                    {
                        for (int i = planYear; i < yr; i++)
                            Listyear.Insert(0, i);

                        year = Listyear;
                        TempData["selectYearList"] = new SelectList(year);
                    }
                    #endregion

                }
                else
                {
                    objPlanModel.Title = "Plan Title";
                    objPlanModel.MQls = "0";
                    objPlanModel.Budget = 0;

                }
                //end
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View(objPlanModel);

        }

        #region NoModel
        /// <summary>
        /// Call NoModel when no baseline model exist for current client.
        /// </summary>
        /// <returns></returns>
        public ActionResult NoModel()
        {
            return View();
        }
        #endregion

        /// <summary>
        /// POST: Save Plan
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SavePlan(PlanModel objPlanModel)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    Plan plan = new Plan();

                    if (objPlanModel.PlanId != 0)
                    {
                        plan = db.Plans.Where(m => m.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();
                    }
                    else
                    {
                        string planDraftStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
                        plan.Status = planDraftStatus;
                        plan.CreatedDate = System.DateTime.Now;
                        plan.CreatedBy = Sessions.User.UserId;
                        plan.IsActive = true;
                        plan.IsDeleted = false;
                        double version = 0;
                        var plantable = db.Plans.Where(m => m.ModelId == objPlanModel.ModelId && m.IsActive == true && m.IsDeleted == false).FirstOrDefault();
                        if (plantable != null)
                        {
                            version = Convert.ToDouble(plantable.Version) + 0.1;
                        }
                        else
                        {
                            version = 1;
                        }
                        plan.Version = version.ToString();
                    }

                    plan.Title = objPlanModel.Title.Trim();
                    plan.MQLs = Convert.ToInt32(objPlanModel.MQls.Trim().Replace(",", "").Replace("$", ""));
                    plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                    plan.ModelId = objPlanModel.ModelId;
                    plan.Year = objPlanModel.Year;
                    if (objPlanModel.PlanId == 0)
                    {
                        db.Plans.Add(plan);
                    }
                    else
                    {
                        plan.ModifiedBy = Sessions.User.UserId;
                        plan.ModifiedDate = System.DateTime.Now;
                        db.Entry(plan).State = EntityState.Modified;
                    }

                    int result = db.SaveChanges();
                    Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                    if (result > 0)
                    {
                        Sessions.PlanId = plan.PlanId;

                        //Create default Plan Improvement Campaign, Program and Tactic
                        //ImprovementController improvementController = new ImprovementController();
                        //int improvementPlanCampaignId = improvementController.CreatePlanImprovementCampaign();
                        //int improvementPlanProgramId = improvementController.CreatePlanImprovementProgram(improvementPlanCampaignId);
                    }

                    return Json(new { id = Sessions.PlanId, redirect = Url.Action("Assortment", new { ismsg = "Plan Saved Successfully." }) });
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { id = 0 });
        }
        #endregion

        #region GetModelName
        /*Added by Nirav shah on 20 feb 2014 for TFS Point 252 : editing a published model*/
        /// <summary>
        /// Get last child
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Model GetLatestModelVersion(Model obj)
        {
            Model objModel = (from m in db.Models where m.ParentModelId == obj.ModelId select m).FirstOrDefault();
            if (objModel != null)
            {
                return GetLatestModelVersion(objModel);
            }
            else
            {
                return obj;
            }

        }

        /// <summary>
        /// Get latest published
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Model GetLatestPublishedVersion(Model obj)
        {

            Model objModel = (from m in db.Models where m.ModelId == obj.ParentModelId select m).FirstOrDefault();
            if (objModel != null)
            {
                if (Convert.ToString(objModel.Status).ToLower() == Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower())
                {
                    return objModel;
                }
                else
                {
                    return GetLatestPublishedVersion(objModel);
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Function to get model list
        /// </summary>
        /// <returns></returns>
        public List<PlanModel> GetModelName()
        {

            // Customer DropDown
            List<PlanModel> lstPlanModel = new List<PlanModel>();
            //List<Model> lstmodel = new List<Model>();


            List<Model> objModelList = new List<Model>();
            List<Model> lstModels = new List<Model>();
            try
            {

                Guid clientId = Sessions.User.ClientId;
                List<Guid> objBusinessUnit = new List<Guid>();
                if (Sessions.IsSystemAdmin || Sessions.IsClientAdmin || Sessions.IsDirector)
                {
                    objBusinessUnit = db.BusinessUnits.Where(bu => bu.ClientId == clientId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                }
                else
                {
                    objBusinessUnit = db.BusinessUnits.Where(bu => bu.BusinessUnitId == Sessions.User.BusinessUnitId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                }
                //fetch published models by businessunitid
                string strPublish = Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value);
                string strDraft = Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value);
                /*Added by Nirav shah on 20 feb 2014 for TFS Point 252 : editing a published model*/
                lstModels = (from m in db.Models
                             where m.IsDeleted == false && objBusinessUnit.Contains(m.BusinessUnitId) && (m.ParentModelId == 0 || m.ParentModelId == null)
                             select m).ToList();
                if (lstModels != null && lstModels.Count > 0)
                {
                    foreach (Model obj in lstModels)
                    {
                        objModelList.Add(GetLatestModelVersion(obj));
                    }
                }

                List<Model> objModelDraftList = objModelList.Where(m => m.Status == strDraft).ToList();
                objModelList = objModelList.Where(m => m.Status == strPublish).ToList();

                if (objModelDraftList != null && objModelDraftList.Count > 0)
                {
                    foreach (Model obj in objModelDraftList)
                    {
                        objModelList.Add(GetLatestPublishedVersion(obj));
                    }
                }
                /*end Nirav changes*/
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            foreach (var v in objModelList)
            {
                if (v != null)
                {
                    PlanModel objPlanModel = new PlanModel();
                    objPlanModel.ModelId = v.ModelId;
                    objPlanModel.ModelTitle = v.Title + " " + v.Version;
                    lstPlanModel.Add(objPlanModel);
                }
            }
            return lstPlanModel;
        }
        #endregion

        #region PlanZero
        /// <summary>
        /// Called when no plan exist.
        /// </summary>
        /// <returns></returns>
        public ActionResult PlanZero(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Plan)
        {
            //// Current user is director or not.
            ViewBag.IsDirector = Sessions.User.IsDirector;

            //// Active menu is home.
            ViewBag.IsPlan = Enums.ActiveMenu.Plan.Equals(activeMenu);

            //// Setting active menu.
            ViewBag.ActiveMenu = activeMenu;
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            if (Enums.ActiveMenu.Plan.Equals(activeMenu))
            {
                //// Active menu is plan.
                ViewBag.IsPlan = true;
                //// Getting Active plan for all above models.


                List<Guid> businessUnitIds = new List<Guid>();

                if (Sessions.IsDirector || Sessions.IsSystemAdmin || Sessions.IsClientAdmin)
                {
                    //// Getting all business unit for client of director.
                    var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId)).Select(b => b.BusinessUnitId).ToList<Guid>();
                    businessUnitIds = clientBusinessUnit.ToList();
                }
                else
                {
                    businessUnitIds.Add(Sessions.User.BusinessUnitId);
                }

                //// Getting active model of above business unit. 
                string modelPublishedStatus = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
                var models = db.Models.Where(m => businessUnitIds.Contains(m.BusinessUnitId) && m.IsDeleted == false).Select(m => m);

                //// Getting modelIds
                var modelIds = models.Select(m => m.ModelId).ToList();

                //var activePlan = db.Plans.Where(p => p.Status.Equals(planPublishedStatus) && modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true)).Select(p => p);
                var activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true)).Select(p => p);

                if (activePlan.Count() != 0)
                {
                    //activePlan = activePlan.Where(p => p.Status.Equals(planPublishedStatus));
                    //if (activePlan.Count() > 0)
                    //{
                    //    ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Home;
                    //    return RedirectToAction("Index", "Home", new { activeMenu = Enums.ActiveMenu.Home });
                    //}
                    ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Plan;
                    return RedirectToAction("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan });

                }
                else
                {
                    models = models.Where(m => m.Status == modelPublishedStatus);
                    if (models != null && models.Count() > 0)
                    {
                        return RedirectToAction("Create", "Plan");
                    }
                    else
                    {
                        return RedirectToAction("ModelZero", "Model");
                    }
                    //ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Plan;
                    //return RedirectToAction("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan });
                }

            }

            string planDraftStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
            //string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;


            List<Guid> businessUnitId = new List<Guid>();

            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                //// Getting all business unit for client of director.
                var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId)).Select(b => b.BusinessUnitId).ToList<Guid>();
                businessUnitId = clientBusinessUnit.ToList();
            }
            else
            {
                businessUnitId.Add(Sessions.User.BusinessUnitId);
            }

            //// Getting active model of above business unit. 
            string modelDraftStat = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
            var varmodels = db.Models.Where(m => businessUnitId.Contains(m.BusinessUnitId)).Select(m => m);

            if (varmodels == null || varmodels.Count() <= 0)
            {
                return RedirectToAction("ModelZero", "Model");
            }
            //// Getting modelIds
            var varmodelIds = varmodels.Select(m => m.ModelId).ToList();

            //var activePlan = db.Plans.Where(p => p.Status.Equals(planPublishedStatus) && modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true)).Select(p => p).SingleOrDefault;
            //var activePlan = db.Plans.Where(p => varmodelIds.Contains(p.Model.ModelId) && p.Status.Equals(planPublishedStatus)).Select(p => p).FirstOrDefault();
            var curPlan = db.Plans.Where(p => varmodelIds.Contains(p.Model.ModelId) && p.Status.Equals(planDraftStatus)).Select(p => p).ToList();
            var actPlan = db.Plans.Where(p => varmodelIds.Contains(p.Model.ModelId) && p.Status.Equals(planPublishedStatus)).Select(p => p).ToList();

            if (actPlan != null && actPlan.Count > 0)
            {
                return RedirectToAction("PlanZero", "Plan", new { activeMenu = "home" });
            }
            else if (curPlan != null && curPlan.Count > 0)
            {
                return RedirectToAction("PlanZero", "Plan", new { activeMenu = "plan" });
            }
            else
            {
                return RedirectToAction("ModelZero", "Model");
            }

            //if (curPlan != null)
            //{
            //    if (curPlan.Count() > 0)
            //    {
            //        return RedirectToAction("Create", "Plan");
            //    }
            //    if (varmodelIds != null)
            //    {
            //        if (varmodelIds.Count() > 0)
            //        {
            //            return RedirectToAction("ModelZero", "Model");
            //        }
            //    }
            //}
            //else
            //{
            //    if (varmodelIds != null)
            //    {
            //        if (varmodelIds.Count() > 0)
            //        {
            //            return RedirectToAction("Create", "Model");
            //        }
            //    }

            //}




            return View();
        }
        #endregion

        #region GetPlanByPlanID
        /// <summary>
        /// Get plan by plan id
        /// </summary>
        /// <param name="planid"></param>
        public JsonResult GetPlanByPlanID(int planid)
        {
            try
            {
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValue(planid),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region "Apply To Calendar"
        /// <summary>
        /// Function to return ApplyToCalendar view.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <returns>Returns view as action result.</returns>
        public ActionResult ApplyToCalendar(string ismsg = "", bool isError = false)
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Plan);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Homezero", "Home");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

            var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));

            HomePlanModel planModel = new HomePlanModel();
            planModel.objplanhomemodelheader = Common.GetPlanHeaderValue(Sessions.PlanId);
            planModel.PlanId = plan.PlanId;
            planModel.PlanTitle = plan.Title;
            planModel.CollaboratorId = GetCollaborator(plan);
            planModel.LastUpdatedDate = GetLastUpdatedDate(plan);

            //List<SelectListItem> planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
            //planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;

            /*changed by nirav Shah on 9 Jan 2014*/

            // planModel.plans = planList;

            List<SelectListItem> UpcomingActivityList = Common.GetUpcomingActivity().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString() }).ToList();
            planModel.objplanhomemodelheader.UpcomingActivity = UpcomingActivityList;

            ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];
            if (TempData["SuccessMessageDuplicatePlan"] != null)
            {
                if (TempData["SuccessMessageDuplicatePlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDuplicatePlan"].ToString();
            }                   /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
                  changed by : Nirav Shah on 13 feb 2014*/
            if (TempData["SuccessMessageDeletedPlan"] != null)
            {
                if (TempData["SuccessMessageDeletedPlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDeletedPlan"].ToString();
            }
            if (TempData["ErrorMessageDuplicatePlan"] != null)
            {
                if (TempData["ErrorMessageDuplicatePlan"].ToString() != string.Empty)
                {
                    ismsg = TempData["ErrorMessageDuplicatePlan"].ToString();
                    isError = true;
                }
            }
            //planModel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId);
            if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                //// Getting all business unit for client of director.
                planModel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId);
                ViewBag.showBid = true;
            }
            else
            {
                ViewBag.showBid = true;
            }
            ViewBag.Msg = ismsg;
            ViewBag.isError = isError;
            return View(planModel);
        }
        public ActionResult PlanList(string Bid)
        {
            HomePlan objHomePlan = new HomePlan();
            objHomePlan.IsDirector = Sessions.IsDirector;
            List<SelectListItem> planList;
            if (Bid == "false")
            {
                planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == Sessions.PlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                    }
                }
            }
            else
            {
                Guid bId = new Guid(Bid);
                planList = Common.GetPlan().Where(s => s.Model.BusinessUnitId == bId).Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == Sessions.PlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                    }
                    else
                    {
                        planList.FirstOrDefault().Selected = true;
                    }
                }
            }
            objHomePlan.plans = planList;
            return PartialView("_ApplytoCalendarPlanList", objHomePlan);
        }
        /// <summary>
        /// Getting list of collaborator for current plan.
        /// </summary>
        /// <param name="plan">Plan</param>
        /// <returns>Returns list of collaborators for current plan.</returns>
        private List<string> GetCollaborator(Plan plan)
        {
            List<string> collaboratorId = new List<string>();
            if (plan.ModifiedBy != null)
            {
                collaboratorId.Add(plan.ModifiedBy.ToString());
            }

            if (plan.CreatedBy != null)
            {
                collaboratorId.Add(plan.CreatedBy.ToString());
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
            var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();

            var planProgramModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.ModifiedBy.ToString()).ToList();
            var planProgramCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedBy.ToString()).ToList();

            var planCampaignModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy.ToString()).ToList();
            var planCampaignCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedBy.ToString()).ToList();

            var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                           .Select(pc => pc);
            var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();

            collaboratorId.AddRange(planTacticCreatedBy);
            collaboratorId.AddRange(planTacticModifiedBy);
            collaboratorId.AddRange(planProgramCreatedBy);
            collaboratorId.AddRange(planProgramModifiedBy);
            collaboratorId.AddRange(planCampaignCreatedBy);
            collaboratorId.AddRange(planCampaignModifiedBy);
            collaboratorId.AddRange(planTacticCommentCreatedBy);
            return collaboratorId.Distinct().ToList<string>();
        }

        /// <summary>
        /// Function to get last updated date time for current plan.
        /// </summary>
        /// <param name="plan">Plan.</param>
        /// <returns>Returns last updated date time.</returns>
        private DateTime GetLastUpdatedDate(Plan plan)
        {
            List<DateTime?> lastUpdatedDate = new List<DateTime?>();
            if (plan.CreatedDate != null)
            {
                lastUpdatedDate.Add(plan.CreatedDate);
            }

            if (plan.ModifiedDate != null)
            {
                lastUpdatedDate.Add(plan.ModifiedDate);
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            if (planTactic.Count() > 0)
            {

                var planTacticModifiedDate = planTactic.ToList().Select(t => t.ModifiedDate).Max();
                lastUpdatedDate.Add(planTacticModifiedDate);

                var planTacticCreatedDate = planTactic.ToList().Select(t => t.CreatedDate).Max();
                lastUpdatedDate.Add(planTacticCreatedDate);

                var planProgramModifiedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.ModifiedDate).Max();
                lastUpdatedDate.Add(planProgramModifiedDate);

                var planProgramCreatedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedDate).Max();
                lastUpdatedDate.Add(planProgramCreatedDate);

                var planCampaignModifiedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedDate).Max();
                lastUpdatedDate.Add(planCampaignModifiedDate);

                var planCampaignCreatedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedDate).Max();
                lastUpdatedDate.Add(planCampaignCreatedDate);

                var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                               .Select(pc => pc);
                if (planTacticComment.Count() > 0)
                {
                    var planTacticCommentCreatedDate = planTacticComment.ToList().Select(pc => pc.CreatedDate).Max();
                    lastUpdatedDate.Add(planTacticCommentCreatedDate);
                }
            }

            return Convert.ToDateTime(lastUpdatedDate.Max());
        }

        /// <summary>
        /// Function to get gantt data.
        /// Added By: Maninde Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="planId">Plan id for which gantt data to be fetched.</param>
        /// <returns>Json Result.</returns>
        public JsonResult GetGanttData(int planId, string isQuater)
        {
            Sessions.PlanId = planId;
            Plan plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            bool isPublished = plan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());
            #region "Tactic"
            return Json(new
            {
                taskData = GetTaskDetailTactic(plan, isQuater),
                planYear = plan.Year,
                isPublished = isPublished
            }, JsonRequestBehavior.AllowGet);
            #endregion
        }


        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// Function to get GANTT chart task detail for Tactic.
        /// Modified: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="planId">Plan Id.</param>
        /// <param name="isQuarter">Flag to indicate whether to fetch data for current Quarter.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        public List<object> GetTaskDetailTactic(Plan plan, string isQuater)
        {
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;

            Common.GetPlanGanttStartEndDate(plan.Year, isQuater, ref CalendarStartDate, ref CalendarEndDate);

            var taskDataCampaign = db.Plan_Campaign.Where(c => c.PlanId.Equals(plan.PlanId) && c.IsDeleted.Equals(false))
                                                   .Select(c => c)
                                                   .ToList()
                                                   .Where(c => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            c.StartDate,
                                                                                                            c.EndDate).Equals(false))
                                                    .Select(c => new
            {
                id = string.Format("C{0}", c.PlanCampaignId),
                text = c.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, c.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          c.StartDate,
                                                          c.EndDate),
                progress = 0,
                open = true,
                color = Common.COLORC6EBF3_WITH_BORDER,
                PlanCampaignId = c.PlanCampaignId,
                IsHideDragHandleLeft = c.StartDate < CalendarStartDate,
                IsHideDragHandleRight = c.EndDate > CalendarEndDate
            }).Select(c => c).OrderBy(c => c.text);

            var taskDataProgram = db.Plan_Campaign_Program.Where(p => p.Plan_Campaign.PlanId.Equals(plan.PlanId) &&
                                                                      p.IsDeleted.Equals(false))
                                                          .Select(p => p)
                                                          .ToList()
                                                          .Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                  CalendarEndDate,
                                                                                                                  p.StartDate,
                                                                                                                  p.EndDate).Equals(false))
                                                          .Select(p => new
            {
                id = string.Format("C{0}_P{1}", p.PlanCampaignId, p.PlanProgramId),
                text = p.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, p.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          p.StartDate,
                                                          p.EndDate),
                progress = 0,
                open = true,
                parent = string.Format("C{0}", p.PlanCampaignId),
                color = Common.COLOR27A4E5,
                PlanProgramId = p.PlanProgramId,
                IsHideDragHandleLeft = p.StartDate < CalendarStartDate,
                IsHideDragHandleRight = p.EndDate > CalendarEndDate
            }).Select(p => p).Distinct().OrderBy(p => p.text);

            var taskDataTactic = db.Plan_Campaign_Program_Tactic.Where(p => p.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId) &&
                                                                            p.IsDeleted.Equals(false))
                                                                .Select(p => p)
                                                                .ToList()
                                                                .Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                        CalendarEndDate,
                                                                                                                        p.StartDate,
                                                                                                                        p.EndDate).Equals(false))
                                                                .Select(t => new
            {
                id = string.Format("C{0}_P{1}_T{2}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                          CalendarEndDate,
                                                          t.StartDate,
                                                          t.EndDate),
                progress = 0,
                open = true,
                parent = string.Format("C{0}_P{1}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId),
                color = Common.COLORC6EBF3_WITH_BORDER,
                plantacticid = t.PlanTacticId,
                IsHideDragHandleLeft = t.StartDate < CalendarStartDate,
                IsHideDragHandleRight = t.EndDate > CalendarEndDate
            }).OrderBy(t => t.text);

            return taskDataCampaign.Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
        }


        /// <summary>
        /// Function to update status of current plan.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="planId">Plan Id whose status is to be updated.</param>
        /// <returns>Returns ApplyToCalendar action result.</returns>
        [HttpPost]
        [ActionName("ApplyToCalendar")]
        public RedirectToRouteResult ApplyToCalendarPost()
        {
            var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            plan.Status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
            plan.ModifiedBy = Sessions.User.UserId;
            plan.ModifiedDate = DateTime.Now;

            //db.Entry(plan).State = EntityState.Modified;
            int returnValue = db.SaveChanges();
            Common.InsertChangeLog(Sessions.PlanId, 0, Sessions.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.published);
            ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Plan;
            return RedirectToAction("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = Sessions.PlanId });
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// Function to update start and end date for tactic.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id to be updated.</param>
        /// <param name="startDate">Start date field.</param>
        /// <param name="duration">Duration of task.</param>
        /// <returns>Returns json result that indicate whether date was updated successfully.</returns>
        public JsonResult UpdateStartEndDate(int id, string startDate, double duration, bool isPlanCampaign, bool isPlanProgram, bool isPlanTactic)
        {
            int returnValue = 0;
            if (isPlanCampaign)
            {
                //// Getting campaign to be updated.
                var planCampaign = db.Plan_Campaign.Single(pc => pc.PlanCampaignId.Equals(id));
                bool isApproved = planCampaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                planCampaign.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                //// Setting start and end date.
                planCampaign.StartDate = DateTime.Parse(startDate);

                DateTime endDate = DateTime.Parse(startDate);
                endDate = endDate.AddDays(duration);
                planCampaign.EndDate = endDate;

                //// Setting modified date and modified by field.
                planCampaign.ModifiedBy = Sessions.User.UserId;
                planCampaign.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();
                if (isApproved)
                {
                    Common.InsertChangeLog(Sessions.PlanId, 0, planCampaign.PlanCampaignId, planCampaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }

            }
            else if (isPlanProgram)
            {
                //// Getting program to be updated.
                var planProgram = db.Plan_Campaign_Program.Single(pc => pc.PlanProgramId.Equals(id));
                bool isApproved = planProgram.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                planProgram.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                //// Setting start and end date.
                planProgram.StartDate = DateTime.Parse(startDate);

                DateTime endDate = DateTime.Parse(startDate);
                endDate = endDate.AddDays(duration);
                planProgram.EndDate = endDate;

                //// Setting modified date and modified by field.
                planProgram.ModifiedBy = Sessions.User.UserId;
                planProgram.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();
                if (isApproved)
                {
                    Common.InsertChangeLog(Sessions.PlanId, 0, planProgram.PlanProgramId, planProgram.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }
            }
            else if (isPlanTactic)
            {
                //// Getting plan tactic to be updated.
                var planTactic = db.Plan_Campaign_Program_Tactic.Single(pt => pt.PlanTacticId.Equals(id));

                bool isApproved = planTactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                //// Changing status of tactic to submitted.
                planTactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();

                //// Setting start and end date.
                planTactic.StartDate = DateTime.Parse(startDate);
                int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
                double conversionRate = GetMQLConversionRate(planTactic.StartDate, modelId);
                planTactic.MQLs = Convert.ToInt32(planTactic.INQs * conversionRate);

                DateTime endDate = DateTime.Parse(startDate);
                endDate = endDate.AddDays(duration);
                planTactic.EndDate = endDate;

                //// Setting modified date and modified by field.
                planTactic.ModifiedBy = Sessions.User.UserId;
                planTactic.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();

                if (isApproved)
                {
                    Common.InsertChangeLog(Sessions.PlanId, 0, planTactic.PlanTacticId, planTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }
            }


            //// Checking whether operation was successfully or not.
            if (returnValue > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Assortment Mix

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Assortment.
        /// </summary>
        /// <returns>Returns View Of Assortment.</returns>
        public ActionResult Assortment(int campaignId = 0, int programId = 0, int tacticId = 0, string ismsg = "", string EditObject = "", bool isError = false)
        {
            Plan plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            ViewBag.PlanId = plan.PlanId;
            PlanModel pm = new PlanModel();
            pm.ModelTitle = plan.Model.Title + " " + plan.Model.Version;
            pm.Title = plan.Title;
            pm.MQLDisplay = plan.MQLs;
            pm.Budget = plan.Budget;
            pm.Year = plan.Year;
            ViewBag.PlanDefinition = pm;

            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            ViewBag.CampaignID = campaignId;
            ViewBag.ProgramID = programId;
            //ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            //ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];
            if (TempData["SuccessMessageDuplicatePlan"] != null)
            {
                if (TempData["SuccessMessageDuplicatePlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDuplicatePlan"].ToString();
            }
            /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
              changed by : Nirav Shah on 13 feb 2014*/
            if (TempData["SuccessMessageDeletedPlan"] != null)
            {
                if (TempData["SuccessMessageDeletedPlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDeletedPlan"].ToString();
            }
            if (TempData["ErrorMessageDuplicatePlan"] != null)
            {
                if (TempData["ErrorMessageDuplicatePlan"].ToString() != string.Empty)
                {
                    ismsg = TempData["ErrorMessageDuplicatePlan"].ToString();
                    isError = true;
                }
            }
            ViewBag.TacticId = tacticId;
            ViewBag.EditOjbect = EditObject;
            ViewBag.Msg = ismsg;
            ViewBag.isError = isError;
            return View("Assortment");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Camapign , Program & Tactic.
        /// </summary>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetCampaign()
        {
            var campaign = db.Plan_Campaign.ToList().Where(pc => pc.PlanId.Equals(Sessions.PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var campaignobj = campaign.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                description = p.Description,
                cost = p.Cost.HasValue ? p.Cost : 0,
                inqs = p.INQs.HasValue ? p.INQs : 0,
                mqls = p.MQLs.HasValue ? p.MQLs : 0,
                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
        changed by : Nirav Shah on 13 feb 2014*/
                isOwner = Sessions.User.UserId == p.CreatedBy ? 0 : 1,
                programs = (db.Plan_Campaign_Program.ToList().Where(pcp => pcp.PlanCampaignId.Equals(p.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pcpj => new
                {
                    id = pcpj.PlanProgramId,
                    title = pcpj.Title,
                    description = pcpj.Description,
                    cost = pcpj.Cost.HasValue ? pcpj.Cost : 0,
                    inqs = pcpj.INQs.HasValue ? pcpj.INQs : 0,
                    mqls = pcpj.MQLs.HasValue ? pcpj.MQLs : 0,
                    isOwner = Sessions.User.UserId == pcpj.CreatedBy ? 0 : 1,
                    tactics = (db.Plan_Campaign_Program_Tactic.ToList().Where(pcpt => pcpt.PlanProgramId.Equals(pcpj.PlanProgramId) && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt).ToList()).Select(pcptj => new
                    {
                        id = pcptj.PlanTacticId,
                        title = pcptj.Title,
                        description = pcptj.Description,
                        cost = pcptj.Cost,
                        inqs = pcptj.INQs,
                        mqls = pcptj.MQLs,
                        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
                         changed by : Nirav Shah on 13 feb 2014*/
                        isOwner = Sessions.User.UserId == pcptj.CreatedBy ? 0 : 1,
                    }).Select(pcptj => pcptj).Distinct().OrderBy(pcptj => pcptj.id)
                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id)
            }).Select(p => p).Distinct().OrderBy(p => p.id);

            return Json(campaignobj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Campaign.
        /// </summary>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public PartialViewResult CreateCampaign()
        {
            // Dropdown for Verticals
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = true;
            ViewBag.RedirectType = false;
            ViewBag.IsOwner = false;
            return PartialView("CampaignAssortment");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Campaign.
        /// </summary>
        /// <param name="id">Campaign Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public PartialViewResult EditCampaign(int id = 0, string RedirectType = "")
        {
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);

            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;
            }
            Plan_Campaign pc = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(id) && pcobj.IsDeleted.Equals(false)).SingleOrDefault();
            if (pc == null)
            {
                return null;
            }

            Plan_CampaignModel pcm = new Plan_CampaignModel();
            pcm.PlanCampaignId = pc.PlanCampaignId;
            pcm.Title = pc.Title;
            pcm.Description = pc.Description;
            pcm.VerticalId = pc.VerticalId;
            pcm.AudienceId = pc.AudienceId;
            pcm.GeographyId = pc.GeographyId;
            pcm.StartDate = pc.StartDate;
            pcm.EndDate = pc.EndDate;
            if (RedirectType != "Assortment")
            {
                var psd = (from p in db.Plan_Campaign_Program where p.PlanCampaignId == id && p.IsDeleted.Equals(false) select p);
                if (psd.Count() > 0)
                {
                    pcm.PStartDate = (from opsd in psd select opsd.StartDate).Min();
                }

                var ped = (from p in db.Plan_Campaign_Program where p.PlanCampaignId == id && p.IsDeleted.Equals(false) select p);
                if (ped.Count() > 0)
                {
                    pcm.PEndDate = (from oped in ped select oped.EndDate).Max();
                }
                var tsd = (from t in db.Plan_Campaign_Program_Tactic where t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted.Equals(false) select t);
                if (tsd.Count() > 0)
                {
                    pcm.TStartDate = (from otsd in tsd select otsd.StartDate).Min();
                }
                var ted = (from t in db.Plan_Campaign_Program_Tactic where t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted.Equals(false) select t);
                if (ted.Count() > 0)
                {
                    pcm.TEndDate = (from oted in ted select oted.EndDate).Max();
                }
            }
            pcm.INQs = pc.INQs;
            pcm.MQLs = pc.MQLs;
            pcm.Cost = pc.Cost;
            if (Sessions.User.UserId == pc.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            return PartialView("CampaignAssortment", pcm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get TacticType.
        /// </summary>
        /// <returns>Returns JSon Result Of TacticType.</returns>
        public JsonResult GetTacticType()
        {
            var tacticType = from t in db.TacticTypes
                             join p in db.Plans on t.ModelId equals p.ModelId
                             where p.PlanId == Sessions.PlanId
                             orderby t.Title
                             select new { t.Title, t.TacticTypeId };
            return Json(tacticType, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Campaign.
        /// </summary>
        /// <param name="form">Form object of Plan_CampaignModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveCampaign(Plan_CampaignModel form, string programs, bool RedirectType)
        {
            try
            {
                if (form.PlanCampaignId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(Sessions.PlanId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()))).FirstOrDefault();

                            if (pc != null)
                            {
                                return Json(new { errormsg = Common.objCached.DuplicateCampaignExits });
                            }
                            else
                            {
                                Plan_Campaign pcobj = new Plan_Campaign();
                                //Change Plan Id
                                pcobj.PlanId = Sessions.PlanId;
                                pcobj.Title = form.Title;
                                pcobj.Description = form.Description;
                                pcobj.VerticalId = form.VerticalId;
                                pcobj.AudienceId = form.AudienceId;
                                pcobj.GeographyId = form.GeographyId;
                                pcobj.INQs = (form.INQs == null ? 0 : form.INQs);
                                pcobj.MQLs = (form.MQLs == null ? 0 : form.MQLs);
                                pcobj.Cost = (form.Cost == null ? 0 : form.Cost);
                                pcobj.StartDate = DateTime.Now;
                                pcobj.EndDate = DateTime.Now.AddMonths(1);
                                pcobj.CreatedBy = Sessions.User.UserId;
                                pcobj.CreatedDate = DateTime.Now;
                                pcobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); // status field in Plan_Campaign table 
                                db.Entry(pcobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                int campaignid = pcobj.PlanCampaignId;
                                result = Common.InsertChangeLog(Sessions.PlanId, null, campaignid, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (result >= 1)
                                {

                                    if (programs != string.Empty)
                                    {
                                        string[] program = programs.Split(',');

                                        foreach (string prg in program)
                                        {
                                            Plan_Campaign_Program pcpobj = new Plan_Campaign_Program();
                                            //Change Plan Id
                                            pcpobj.PlanCampaignId = campaignid;
                                            pcpobj.Title = prg;
                                            pcpobj.VerticalId = form.VerticalId;
                                            pcpobj.AudienceId = form.AudienceId;
                                            pcpobj.GeographyId = form.GeographyId;
                                            pcpobj.INQs = pcpobj.MQLs = 0;
                                            pcpobj.Cost = 0;
                                            pcpobj.StartDate = DateTime.Now;
                                            pcpobj.EndDate = DateTime.Now.AddMonths(1);
                                            pcpobj.CreatedBy = Sessions.User.UserId;
                                            pcpobj.CreatedDate = DateTime.Now;
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); // status field in Plan_Campaign_Program table 
                                            db.Entry(pcpobj).State = EntityState.Added;
                                            result = db.SaveChanges();
                                            int programId = pcpobj.PlanProgramId;
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, programId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                        }
                                    }
                                }
                                scope.Complete();
                                return Json(new { redirect = Url.Action("Assortment") });
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(Sessions.PlanId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();

                            if (pc != null)
                            {
                                return Json(new { errormsg = Common.objCached.DuplicateCampaignExits });
                            }
                            else
                            {
                                Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(form.PlanCampaignId) && pcobjw.IsDeleted.Equals(false)).SingleOrDefault();
                                //Change Plan Id
                                pcobj.Title = form.Title;
                                pcobj.Description = form.Description;
                                pcobj.VerticalId = form.VerticalId;
                                pcobj.AudienceId = form.AudienceId;
                                pcobj.GeographyId = form.GeographyId;
                                if (RedirectType)
                                {
                                    pcobj.StartDate = form.StartDate;
                                    pcobj.EndDate = form.EndDate;
                                }
                                //pcobj.INQs = (form.INQs == null ? 0 : form.INQs);
                                //pcobj.MQLs = (form.MQLs == null ? 0 : form.MQLs);
                                //pcobj.Cost = (form.Cost == null ? 0 : form.Cost);
                                pcobj.ModifiedBy = Sessions.User.UserId;
                                pcobj.ModifiedDate = DateTime.Now;
                                db.Entry(pcobj).State = EntityState.Modified;
                                int result = db.SaveChanges();
                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                if (result >= 1)
                                {
                                    scope.Complete();
                                    if (RedirectType)
                                    {
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment") });
                                    }
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

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Delete Campaign.
        /// </summary>
        /// <param name="id">Campaign Id.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
            changed by : Nirav Shah on 13 feb 2014
            Changed : add new Parameter  RedirectType
         */
        public ActionResult DeleteCampaign(int id = 0, bool RedirectType = false)
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ObjectParameter parameterReturnValue = new ObjectParameter("ReturnValue", typeof(int));
                        db.Plan_Task_Delete(id,
                                            null,
                                            null,
                                            true,
                                            DateTime.Now,
                                            Sessions.User.UserId,
                                            parameterReturnValue);
                        int returnValue;
                        int cid = 0;
                        int pid = 0;
                        string Title = "";
                        Int32.TryParse(parameterReturnValue.Value.ToString(), out returnValue);
                        if (returnValue != 0)
                        {
                            Plan_Campaign pc = db.Plan_Campaign.Where(p => p.PlanCampaignId == id).SingleOrDefault();
                            Title = pc.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanCampaignId, pc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                scope.Complete();
                                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
                                  changed by : Nirav Shah on 13 feb 2014
                                  Changed : set message and based on request redirect page.
                                */
                                TempData["SuccessMessageDeletedPlan"] = "Campaign " + Title + " Deleted Successfully.";

                                //return Json(new { redirect = Url.Action("Assortment") });
                                if (RedirectType)
                                {
                                    return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                }
                                else
                                {
                                    return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Program.
        /// </summary>
        /// <returns>Returns Partial View Of Program.</returns>
        public PartialViewResult CreateProgram(int id = 0)
        {
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = true;
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanCampaignId = id;
            Plan_Campaign pc = db.Plan_Campaign.Where(pco => pco.PlanCampaignId == id).SingleOrDefault();
            pcpm.GeographyId = pc.GeographyId;
            pcpm.VerticalId = pc.VerticalId;
            pcpm.AudienceId = pc.AudienceId;
            ViewBag.IsOwner = false;      /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
            ViewBag.RedirectType = false;
            return PartialView("ProgramAssortment", pcpm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Edit Program.
        /// </summary>
        /// <param name="id">Program Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Program.</returns>
        public PartialViewResult EditProgram(int id = 0, string RedirectType = "")
        {
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;
            }
            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id) && pcpobj.IsDeleted.Equals(false)).SingleOrDefault();
            if (pcp == null)
            {
                return null;
            }

            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanProgramId = pcp.PlanProgramId;
            pcpm.PlanCampaignId = pcp.PlanCampaignId;
            pcpm.Title = pcp.Title;
            pcpm.Description = pcp.Description;
            pcpm.VerticalId = pcp.VerticalId;
            pcpm.AudienceId = pcp.AudienceId;
            pcpm.GeographyId = pcp.GeographyId;
            pcpm.StartDate = pcp.StartDate;
            pcpm.EndDate = pcp.EndDate;
            if (RedirectType != "Assortment")
            {
                pcpm.CStartDate = pcp.Plan_Campaign.StartDate;
                pcpm.CEndDate = pcp.Plan_Campaign.EndDate;
                var tsd = (from t in db.Plan_Campaign_Program_Tactic where t.PlanProgramId == id select t);
                if (tsd.Count() > 0)
                {
                    pcpm.TStartDate = (from otsd in tsd select otsd.StartDate).Min();
                }
                var ted = (from t in db.Plan_Campaign_Program_Tactic where t.PlanProgramId == id select t);
                if (ted.Count() > 0)
                {
                    pcpm.TEndDate = (from oted in ted select oted.EndDate).Max();
                }
            }
            pcpm.INQs = pcp.INQs;
            pcpm.MQLs = pcp.MQLs;
            pcpm.Cost = pcp.Cost;
            /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
            if (Sessions.User.UserId == pcp.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            ViewBag.Campaign = pcp.Plan_Campaign.Title;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            return PartialView("ProgramAssortment", pcpm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Program.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveProgram(Plan_Campaign_ProgramModel form, string tactics, bool RedirectType)
        {
            try
            {
                if (form.PlanProgramId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcp.IsDeleted.Equals(false)
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                return Json(new { errormsg = Common.objCached.DuplicateProgramExits });
                            }
                            else
                            {
                                Plan_Campaign_Program pcpobj = new Plan_Campaign_Program();
                                pcpobj.PlanCampaignId = form.PlanCampaignId;
                                pcpobj.Title = form.Title;
                                pcpobj.Description = form.Description;
                                pcpobj.VerticalId = form.VerticalId;
                                pcpobj.AudienceId = form.AudienceId;
                                pcpobj.GeographyId = form.GeographyId;
                                pcpobj.StartDate = DateTime.Now;
                                pcpobj.EndDate = DateTime.Now.AddMonths(1);
                                pcpobj.CreatedBy = Sessions.User.UserId;
                                pcpobj.CreatedDate = DateTime.Now;
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); //status field added for Plan_Campaign_Program table
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                int programid = pcpobj.PlanProgramId;
                                result = Common.InsertChangeLog(Sessions.PlanId, null, programid, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                int totalinq = 0;
                                int totalmql = 0;
                                double totalcost = 0;
                                if (tactics != string.Empty)
                                {
                                    string[] tactic = tactics.Split(',');
                                    var TacticType = db.TacticTypes;
                                    foreach (string tac in tactic)
                                    {
                                        Plan_Campaign_Program_Tactic pcptobj = new Plan_Campaign_Program_Tactic();
                                        pcptobj.PlanProgramId = programid;
                                        int tacid = Convert.ToInt32(tac);
                                        pcptobj.TacticTypeId = tacid;
                                        TacticType mt = db.TacticTypes.Where(m => m.TacticTypeId == tacid).FirstOrDefault();
                                        pcptobj.Title = mt.Title;
                                        pcptobj.VerticalId = form.VerticalId;
                                        pcptobj.AudienceId = form.AudienceId;
                                        pcptobj.GeographyId = form.GeographyId;
                                        pcptobj.INQs = mt.ProjectedInquiries == null ? 0 : Convert.ToInt32(mt.ProjectedInquiries);
                                        int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
                                        double conversionRate = GetMQLConversionRate(DateTime.Now, modelId);
                                        pcptobj.MQLs = Convert.ToInt32(pcptobj.INQs * conversionRate);
                                        pcptobj.Cost = mt.ProjectedRevenue == null ? 0 : Convert.ToDouble(mt.ProjectedRevenue);
                                        totalinq += pcptobj.INQs;
                                        totalmql += pcptobj.MQLs;
                                        totalcost += pcptobj.Cost;
                                        pcptobj.StartDate = DateTime.Now;
                                        pcptobj.EndDate = DateTime.Now.AddMonths(1);
                                        pcptobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                        pcptobj.BusinessUnitId = (from m in db.Models
                                                                  join p in db.Plans on m.ModelId equals p.ModelId
                                                                  where p.PlanId == Sessions.PlanId
                                                                  select m.BusinessUnitId).FirstOrDefault();
                                        pcptobj.CreatedBy = Sessions.User.UserId;
                                        pcptobj.CreatedDate = DateTime.Now;
                                        db.Entry(pcptobj).State = EntityState.Added;
                                        result = db.SaveChanges();
                                        int tid = pcptobj.PlanTacticId;
                                        result = Common.InsertChangeLog(Sessions.PlanId, null, tid, pcptobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                    }
                                }
                                result = TacticValueCalculate(pcpobj.PlanProgramId);
                                scope.Complete();
                                return Json(new { redirect = Url.Action("Assortment", new { campaignId = form.PlanCampaignId }) });
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(form.PlanProgramId) && pcp.IsDeleted.Equals(false)
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                return Json(new { errormsg = Common.objCached.DuplicateProgramExits });
                            }
                            else
                            {
                                Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(form.PlanProgramId)).SingleOrDefault();
                                //Change Plan Id
                                pcpobj.Title = form.Title;
                                pcpobj.Description = form.Description;
                                pcpobj.VerticalId = form.VerticalId;
                                pcpobj.AudienceId = form.AudienceId;
                                pcpobj.GeographyId = form.GeographyId;
                                if (RedirectType)
                                {
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
                                }
                                //pcpobj.INQs = (form.INQs == null ? 0 : form.INQs);
                                //pcpobj.MQLs = (form.MQLs == null ? 0 : form.MQLs);
                                //pcpobj.Cost = (form.Cost == null ? 0 : form.Cost);
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Modified;
                                int result = db.SaveChanges();
                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                if (result >= 1)
                                {
                                    scope.Complete();
                                    if (RedirectType)
                                    {
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment", new { campaignId = form.PlanCampaignId }) });
                                    }
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

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Delete Program.
        /// </summary>
        /// <param name="id">Program Id.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
        public ActionResult DeleteProgram(int id = 0, bool RedirectType = false)
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ObjectParameter parameterReturnValue = new ObjectParameter("ReturnValue", typeof(int));
                        db.Plan_Task_Delete(null,
                                            id,
                                            null,
                                            true,
                                            DateTime.Now,
                                            Sessions.User.UserId,
                                            parameterReturnValue);
                        int returnValue;
                        int cid = 0;
                        int pid = 0;
                        string Title = "";
                        Int32.TryParse(parameterReturnValue.Value.ToString(), out returnValue);
                        if (returnValue != 0)
                        {
                            Plan_Campaign_Program pc = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == id).SingleOrDefault();
                            cid = pc.PlanCampaignId;
                            Title = pc.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanProgramId, pc.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                TacticValueCalculate(pc.PlanCampaignId, false);
                                scope.Complete();
                                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
                                TempData["SuccessMessageDeletedPlan"] = "Program " + Title + " Deleted Successfully.";
                                //return Json(new { redirect = Url.Action("Assortment", new { campaignId = pc.PlanCampaignId }) });
                                if (RedirectType)
                                {
                                    return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                }
                                else
                                {
                                    return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Tactic.
        /// </summary>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult CreateTactic(int id = 0)
        {
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.Tactics = from t in db.TacticTypes
                              join p in db.Plans on t.ModelId equals p.ModelId
                              where p.PlanId == Sessions.PlanId
                              orderby t.Title
                              select t;
            ViewBag.IsCreated = true;
            Plan_Campaign_Program_TacticModel pcptm = new Plan_Campaign_Program_TacticModel();
            pcptm.PlanProgramId = id;
            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpo => pcpo.PlanProgramId == id).SingleOrDefault();
            pcptm.GeographyId = pcp.GeographyId;
            pcptm.VerticalId = pcp.VerticalId;
            pcptm.AudienceId = pcp.AudienceId;
            ViewBag.IsOwner = false;/*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
            ViewBag.RedirectType = false;
            return PartialView("TacticAssortment", pcptm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Tactic.
        /// </summary>
        /// <param name="id">Tactic Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult EditTactic(int id = 0, string RedirectType = "")
        {
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.Tactics = from t in db.TacticTypes
                              join p in db.Plans on t.ModelId equals p.ModelId
                              where p.PlanId == Sessions.PlanId
                              orderby t.Title
                              select t;
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;

            }
            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id)).SingleOrDefault();
            if (pcpt == null)
            {
                return null;
            }

            Plan_Campaign_Program_TacticModel pcptm = new Plan_Campaign_Program_TacticModel();
            pcptm.PlanProgramId = pcpt.PlanProgramId;
            pcptm.PlanTacticId = pcpt.PlanTacticId;
            pcptm.TacticTypeId = pcpt.TacticTypeId;
            pcptm.Title = pcpt.Title;
            pcptm.Description = pcpt.Description;
            pcptm.VerticalId = pcpt.VerticalId;
            pcptm.AudienceId = pcpt.AudienceId;
            pcptm.GeographyId = pcpt.GeographyId;
            pcptm.StartDate = pcpt.StartDate;
            pcptm.EndDate = pcpt.EndDate;
            if (RedirectType != "Assortment")
            {
                pcptm.PStartDate = pcpt.Plan_Campaign_Program.StartDate;
                pcptm.PEndDate = pcpt.Plan_Campaign_Program.EndDate;
                pcptm.CStartDate = pcpt.Plan_Campaign_Program.Plan_Campaign.StartDate;
                pcptm.CEndDate = pcpt.Plan_Campaign_Program.Plan_Campaign.EndDate;
            }
            pcptm.INQs = pcpt.INQs;
            pcptm.MQLs = pcpt.MQLs;
            pcptm.Cost = pcpt.Cost;
            /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
            if (Sessions.User.UserId == pcpt.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            ViewBag.Program = pcpt.Plan_Campaign_Program.Title;
            ViewBag.Campaign = pcpt.Plan_Campaign_Program.Plan_Campaign.Title;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            return PartialView("TacticAssortment", pcptm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Tactic.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_Program_TacticModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveTactic(Plan_Campaign_Program_TacticModel form, bool RedirectType)
        {
            try
            {
                int cid = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == form.PlanProgramId).Select(p => p.PlanCampaignId).FirstOrDefault();
                int pid = form.PlanProgramId;
                if (form.PlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                return Json(new { errormsg = Common.objCached.DuplicateTacticExits });
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic pcpobj = new Plan_Campaign_Program_Tactic();
                                pcpobj.PlanProgramId = form.PlanProgramId;
                                pcpobj.Title = form.Title;
                                pcpobj.TacticTypeId = form.TacticTypeId;
                                pcpobj.Description = form.Description;
                                pcpobj.VerticalId = form.VerticalId;
                                pcpobj.AudienceId = form.AudienceId;
                                pcpobj.GeographyId = form.GeographyId;
                                pcpobj.INQs = form.INQs;
                                pcpobj.MQLs = form.MQLs;
                                pcpobj.Cost = form.Cost;
                                pcpobj.StartDate = DateTime.Now;
                                pcpobj.EndDate = DateTime.Now.AddMonths(1);
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                pcpobj.BusinessUnitId = (from m in db.Models
                                                         join p in db.Plans on m.ModelId equals p.ModelId
                                                         where p.PlanId == Sessions.PlanId
                                                         select m.BusinessUnitId).FirstOrDefault();
                                pcpobj.CreatedBy = Sessions.User.UserId;
                                pcpobj.CreatedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                result = TacticValueCalculate(pcpobj.PlanProgramId);
                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (result >= 1)
                                {
                                    scope.Complete();
                                    return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
                                }
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(form.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                return Json(new { errormsg = Common.objCached.DuplicateTacticExits });
                            }
                            else
                            {
                                bool isReSubmission = false;
                                bool isDirectorLevelUser = false;
                                string status = string.Empty;
                                if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
                                {
                                    isDirectorLevelUser = true;
                                }
                                Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).SingleOrDefault();
                                pcpobj.Title = form.Title;
                                status = pcpobj.Status;
                                if (pcpobj.TacticTypeId != form.TacticTypeId)
                                {
                                    pcpobj.TacticTypeId = form.TacticTypeId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                pcpobj.Description = form.Description;
                                if (pcpobj.VerticalId != form.VerticalId)
                                {
                                    pcpobj.VerticalId = form.VerticalId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                if (pcpobj.AudienceId != form.AudienceId)
                                {
                                    pcpobj.AudienceId = form.AudienceId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                if (pcpobj.GeographyId != form.GeographyId)
                                {
                                    pcpobj.GeographyId = form.GeographyId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                if (RedirectType)
                                {

                                    DateTime todaydate = DateTime.Now;

                                    if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                    {
                                        if (todaydate > form.StartDate && todaydate < form.EndDate)
                                        {
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                            if (pcpobj.EndDate != form.EndDate)
                                            {
                                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                                Common.mailSendForTactic(pcpobj.PlanTacticId, pcpobj.Status, pcpobj.Title);
                                            }
                                        }
                                        else if (todaydate > form.EndDate)
                                        {
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
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

                                }
                                if (pcpobj.INQs != form.INQs)
                                {
                                    pcpobj.INQs = form.INQs;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                if (pcpobj.MQLs != form.MQLs)
                                {
                                    pcpobj.MQLs = form.MQLs;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                /* TFS Bug 207 : Cant override the Cost from the defaults coming out of the model
                                 * changed by Nirav shah on 10 feb 2014  
                                 */
                                if (pcpobj.Cost != form.Cost)
                                {
                                    pcpobj.Cost = form.Cost;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                /* TFS Bug 207 : end changes */
                                // pcpobj.Cost = form.Cost; 
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Modified;
                                int result;
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                }
                                if (isReSubmission && Common.CheckAfterApprovedStatus(status))
                                {
                                    pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                    Common.mailSendForTactic(pcpobj.PlanTacticId, pcpobj.Status, pcpobj.Title);
                                }
                                result = db.SaveChanges();
                                result = TacticValueCalculate(pcpobj.PlanProgramId);
                                if (result >= 1)
                                {
                                    scope.Complete();
                                    if (RedirectType)
                                    {
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
                                    }
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

            return Json(new { });
        }

        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     
         * changed by : Nirav Shah on 13 feb 2014
           Add delete tactic feature
         */
        [HttpPost]
        public ActionResult DeleteTactic(int id = 0, bool RedirectType = false)
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ObjectParameter parameterReturnValue = new ObjectParameter("ReturnValue", typeof(int));
                        db.Plan_Task_Delete(null,
                                            null,
                                            id,
                                            true,
                                            DateTime.Now,
                                            Sessions.User.UserId,
                                            parameterReturnValue);
                        int returnValue;
                        int cid = 0;
                        int pid = 0;
                        string Title = "";
                        Int32.TryParse(parameterReturnValue.Value.ToString(), out returnValue);
                        if (returnValue != 0)
                        {
                            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.PlanTacticId == id).SingleOrDefault();
                            cid = pcpt.Plan_Campaign_Program.PlanCampaignId;
                            pid = pcpt.PlanProgramId;
                            Title = pcpt.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.PlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                TacticValueCalculate(pcpt.PlanProgramId);
                                scope.Complete();
                                TempData["SuccessMessageDeletedPlan"] = "Tactic " + Title + " Deleted Successfully.";
                                if (RedirectType)
                                {
                                    return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                }
                                else
                                {
                                    return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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
            return Json(new { });
        }

        /// <summary>
        /// Total Stage Value Calculated & Update.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>return int.</returns>
        public int TacticValueCalculate(int id, bool isProgram = true)
        {
            try
            {
                if (isProgram)
                {
                    Plan_Campaign_Program pcp = new Plan_Campaign_Program();
                    pcp = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == id).SingleOrDefault();
                    var totalProgram = (from t in db.Plan_Campaign_Program_Tactic
                                        where t.PlanProgramId == id && t.IsDeleted.Equals(false)
                                        select new { t.INQs, t.MQLs, t.Cost }).ToList();
                    pcp.INQs = totalProgram.Sum(tp => tp.INQs);
                    pcp.MQLs = totalProgram.Sum(tp => tp.MQLs);
                    pcp.Cost = totalProgram.Sum(tp => tp.Cost);
                    db.Entry(pcp).State = EntityState.Modified;
                    id = pcp.PlanCampaignId;
                }

                Plan_Campaign pc = new Plan_Campaign();
                pc = db.Plan_Campaign.Where(p => p.PlanCampaignId == id).SingleOrDefault();
                var totalCampaign = (from t in db.Plan_Campaign_Program_Tactic
                                     where t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted.Equals(false)
                                     select new { t.INQs, t.MQLs, t.Cost }).ToList();
                pc.INQs = totalCampaign.Sum(tp => tp.INQs);
                pc.MQLs = totalCampaign.Sum(tp => tp.MQLs);
                pc.Cost = totalCampaign.Sum(tp => tp.Cost);

                db.Entry(pc).State = EntityState.Modified;
                return db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return 0;
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic Type INQ,MQL,Cost Value.
        /// </summary>
        /// <param name="tacticTypeId">Tactic Type Id</param>
        /// <returns>Returns Json Result of Tactic Type. </returns>
        [HttpPost]
        public JsonResult LoadTacticTypeValue(int tacticTypeId)
        {
            TacticType tt = db.TacticTypes.Where(t => t.TacticTypeId == tacticTypeId).FirstOrDefault();
            return Json(new { inq = tt.ProjectedInquiries == null ? 0 : tt.ProjectedInquiries, mql = tt.ProjectedMQLs == null ? 0 : tt.ProjectedMQLs, revenue = tt.ProjectedRevenue == null ? 0 : tt.ProjectedRevenue }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Calculate MQL Conerstion Rate based on Session Plan Id.
        /// Added by Bhavesh Dobariya.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <returns>JsonResult MQl Rate.</returns>
        public JsonResult CalculateMQL(Plan_Campaign_Program_TacticModel form, bool RedirectType)
        {
            DateTime StartDate = new DateTime();
            if (form.PlanTacticId != 0)
            {
                if (RedirectType)
                {
                    StartDate = form.StartDate;
                }
                else
                {
                    StartDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == form.PlanTacticId).Select(t => t.StartDate).SingleOrDefault();
                }
            }
            else
            {
                StartDate = DateTime.Now;
            }
            int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            return Json(new { mql = GetMQLConversionRate(StartDate, modelId) });
        }

        private double GetMQLConversionRate(DateTime StartDate, int ModelId)
        {
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageINQ)).Level.Value;
            string stageTypeCR = Enums.StageType.CR.ToString();
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
            ModelId = ReportController.GetModelId(StartDate, ModelId);
            var mqllist = (from modelFunnelStage in db.Model_Funnel_Stage
                           join stage in db.Stages on modelFunnelStage.StageId equals stage.StageId
                           where modelFunnelStage.Model_Funnel.ModelId == ModelId &&
                                           modelFunnelStage.StageType.Equals(stageTypeCR) &&
                                           stage.ClientId.Equals(Sessions.User.ClientId) &&
                                           stage.Level >= levelINQ && stage.Level < levelMQL
                           select new
                           {
                               ModelId = modelFunnelStage.Model_Funnel.ModelId,
                               value = modelFunnelStage.Value,
                           }).GroupBy(rl => new { id = rl.ModelId }).ToList().Select(r => new
                           {
                               value = (r.Aggregate(1.0, (s1, s2) => s1 * (s2.value / 100)))
                           }).Select(r => new { value = r.value }).SingleOrDefault();

            
            if (mqllist != null)
            {
                return mqllist.value;     
            }

            return 0;
        }

        #endregion

        #region "Duplicate"

        /// <summary>
        /// Function to duplicate current plan.
        /// Added By: Maninder Singh Wadhva.
        /// Updated By : Bhavesh B Dobariya.
        /// Date: 12/19/2013
        /// </summary>
        /// <param name="planId">Plan Id to be duplicated.</param>
        /// <returns>Returns ApplyToCalendar action result.</returns>
        [HttpPost]
        public ActionResult DuplicatePlan()
        {
            try
            {
                int returnValue = DuplicateClone(Sessions.PlanId, "Plan");
                if (returnValue != 0)
                {
                    Common.InsertChangeLog(returnValue, 0, returnValue, db.Plans.Single(p => p.PlanId.Equals(returnValue)).Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.created, db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Title);
                    Sessions.PlanId = returnValue;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { isSuccess = true, planId = Sessions.PlanId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to duplicate current Campaign,Program,Tactic.
        /// Added By: Bhavesh B Dobariya.
        /// Date: 09/01/2014
        /// </summary>
        /// <param name="Id">Id to be duplicated.</param>
        /// <param name="RedirectType">Redirect type.</param>
        /// <param name="CopyClone">copy of .</param>
        /// <returns>Returns action result.</returns>
        [HttpPost]
        public ActionResult DuplicateCopyClone(int id, bool RedirectType, string CopyClone)
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        int cid = 0;
                        int pid = 0;
                        int returnValue = DuplicateClone(id, CopyClone);
                        if (returnValue != 0)
                        {
                            if (CopyClone == "Tactic")
                            {
                                Plan_Campaign_Program_Tactic opcpt = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(returnValue)).SingleOrDefault();
                                cid = opcpt.Plan_Campaign_Program.PlanCampaignId;
                                pid = opcpt.PlanProgramId;
                                TacticValueCalculate(opcpt.PlanProgramId); /*Added by Nirav Shah on 17 feb 2013  for Duplicate Tactic */
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, opcpt.PlanProgramId, opcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                            }
                            else if (CopyClone == "Program")
                            {
                                Plan_Campaign_Program opcp = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(returnValue)).SingleOrDefault();
                                cid = opcp.PlanCampaignId;
                                TacticValueCalculate(opcp.PlanCampaignId, false); /*Added by Nirav Shah on 17 feb 2013  for Duplicate Program */
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, opcp.PlanProgramId, opcp.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                            }
                            else if (CopyClone == "Campaign")
                            {
                                Plan_Campaign opc = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(returnValue)).SingleOrDefault();
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, opc.PlanCampaignId, opc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                            }

                        }
                        if (returnValue >= 1)
                        {
                            scope.Complete();
                            if (RedirectType)
                            {
                                return Json(new { redirect = Url.Action("ApplyToCalendar") });
                            }
                            else
                            {
                                return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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

        /// <summary>
        /// Function to duplicate clone.
        /// Added By: Bhavesh B Dobariya.
        /// Date: 09/01/2014
        /// </summary>
        /// <param name="Id">Id to be duplicated.</param>
        /// <param name="CopyClone">copy of copyclone.</param>
        /// <returns>Returns action result.</returns>
        public int DuplicateClone(int id = 0, string CopyClone = "Plan")
        {
            int returnValue = 0;
            try
            {
                ObjectParameter parameterReturnValue = new ObjectParameter("ReturnValue", typeof(int));
                db.PlanDuplicate(Sessions.PlanId,
                                 Enums.PlanStatusValues.Single(status => status.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value,
                                 Enums.TacticStatusValues.Single(status => status.Key.Equals(Enums.TacticStatus.Created.ToString())).Value,
                                 DateTime.Now,
                                 Sessions.User.UserId,
                                 Common.copySuffix + Common.GetTimeStamp(),
                                 CopyClone,
                                 id,
                                 parameterReturnValue);

                Int32.TryParse(parameterReturnValue.Value.ToString(), out returnValue);
                if (returnValue != 0)
                {
                    TempData["SuccessMessageDuplicatePlan"] = CopyClone + " duplicated.";
                }
                else
                {
                    TempData["ErrorMessageDuplicatePlan"] = CopyClone + " with same name already exist.";
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return returnValue;
        }

        #endregion

        #region Plan Selector screen

        /// <summary>
        /// This method will return the view for Plan selector screen
        /// </summary>
        /// <returns></returns>
        public ActionResult PlanSelector()
        {
            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            ViewBag.IsViewOnly = "false";
            try
            {
                if (Sessions.RolePermission != null)
                {
                    Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                    switch (permission)
                    {
                        case Common.Permission.FullAccess:
                            break;
                        case Common.Permission.NoAccess:
                            return RedirectToAction("Index", "NoAccess");
                        case Common.Permission.NotAnEntity:
                            break;
                        case Common.Permission.ViewOnly:
                            ViewBag.IsViewOnly = "true";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View();
        }

        /// <summary>
        /// This method will return the JSON result for Plan listing
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="BusinessUnit"></param>
        /// <returns></returns>
        public JsonResult GetPlanSelectorData(string Year, string BusinessUnit)
        {
            Guid BUId = Guid.Empty;
            if (!string.IsNullOrEmpty(BusinessUnit))
            {
                BUId = Guid.Parse(BusinessUnit);
            }
            string str_Year = Convert.ToString(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);
            int Int_Year = Convert.ToInt32(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);
            List<Plan> objPlan = new List<Plan>();
            List<Plan_Selector> lstPlanSelector = new List<Plan_Selector>();
            try
            {
                Guid clientId = Sessions.User.ClientId;

                // Get the list of plan, filtered by Business Unit and Year selected
                if (!string.IsNullOrEmpty(BusinessUnit) && Int_Year > 0)
                {
                    objPlan = (from p in db.Plans
                               join m in db.Models on p.ModelId equals m.ModelId
                               join bu in db.BusinessUnits on m.BusinessUnitId equals bu.BusinessUnitId
                               where bu.ClientId == clientId && bu.IsDeleted == false && m.IsDeleted == false &&
                               p.IsDeleted == false && p.Year == str_Year && m.BusinessUnitId.Equals(BUId)
                               select p).OrderByDescending(p => p.ModifiedDate ?? p.CreatedDate).ThenBy(p => p.Title).ToList();
                }

                if (objPlan != null && objPlan.Count > 0)
                {
                    foreach (var item in objPlan)
                    {
                        var LastUpdated = !string.IsNullOrEmpty(Convert.ToString(item.ModifiedDate)) ? item.ModifiedDate : item.CreatedDate;
                        Plan_Selector objPlanSelector = new Plan_Selector();
                        objPlanSelector.PlanId = item.PlanId;
                        objPlanSelector.PlanTitle = item.Title;
                        objPlanSelector.LastUpdated = LastUpdated.Value.Date.ToString("M/d/yy");
                        objPlanSelector.MQLS = (item.MQLs).ToString("#,##0"); ;
                        objPlanSelector.Budget = (item.Budget).ToString("#,##0");
                        objPlanSelector.Status = item.Status;
                        lstPlanSelector.Add(objPlanSelector);
                    }
                }

                return Json(new { lstPlanSelector }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Method to get the list of years
        /// </summary>
        /// <returns></returns>
        public JsonResult GetYearsTab()
        {
            Guid clientId = Sessions.User.ClientId;
            var objPlan = (from p in db.Plans
                           join m in db.Models on p.ModelId equals m.ModelId
                           join bu in db.BusinessUnits on m.BusinessUnitId equals bu.BusinessUnitId
                           where bu.ClientId == clientId && bu.IsDeleted == false && m.IsDeleted == false && p.IsDeleted == false
                           select p).OrderBy(q => q.Year).ToList();

            var lstYears = objPlan.OrderBy(p => p.Year).Select(p => p.Year).Distinct().Take(10).ToList();

            return Json(lstYears, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Method to get the list of business Unit (Role wise)
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBUTab()
        {
            var returnDataGuid = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
            {
                id = b.BusinessUnitId,
                title = b.Title
            }).Select(b => b).Distinct().OrderBy(b => b.id);

            if (Sessions.IsPlanner)
            {
                returnDataGuid = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.BusinessUnitId.Equals(Sessions.User.BusinessUnitId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.id);
            }

            return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Method to delete the plan
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public JsonResult DeletePlan(int PlanId = 0)
        {
            int returnValue = 0;
            string PlanName = "";
            using (var scope = new TransactionScope())
            {
                if (PlanId > 0)
                {
                    PlanName = db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false).FirstOrDefault().Title;
                    db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId && pcpt.IsDeleted == false).ToList().ForEach(pcpt => pcpt.IsDeleted = true);
                    db.Plan_Campaign_Program.Where(pcp => pcp.Plan_Campaign.PlanId == PlanId && pcp.IsDeleted == false).ToList().ForEach(pcp => pcp.IsDeleted = true);
                    db.Plan_Campaign.Where(pc => pc.PlanId == PlanId && pc.IsDeleted == false).ToList().ForEach(pc => pc.IsDeleted = true);
                    db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false).ToList().ForEach(p => p.IsDeleted = true);
                    returnValue = db.SaveChanges();
                    scope.Complete();
                }
            }
            if (returnValue > 0)
                return Json(new { successmsg = string.Format(Common.objCached.PlanDeleteSuccessful, PlanName) }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { errorMsg = string.Format(Common.objCached.PlanDeleteError, PlanName) }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
