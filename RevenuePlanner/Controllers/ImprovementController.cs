using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

/*
    Author: Kuber Joshi
    Created Date: 02/10/2014
    Purpose: Plan Improvement
 */

namespace RevenuePlanner.Controllers
{
    public class ImprovementController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private const int DEF_COST_VALUE = 0;

        #endregion

        #region Plan Improvement Campaign

        /// <summary>
        /// Added By: Kuber Joshi
        /// Action to create default Plan Improvement Campaign
        /// </summary>
        /// <returns>Returns ImprovementPlanCampaignId or -1 if duplicate exists</returns>
        public int CreatePlanImprovementCampaign()
        {
            int retVal = -1;

            try
            {
                //Fetch Plan details
                Plan objPlan = new Plan();
                objPlan = db.Plans.Where(p => p.PlanId == Sessions.PlanId).FirstOrDefault();
                if (objPlan != null)
                {
                    string planImprovementCampaignTitle = objPlan.Title + Common.defaultImprovementSuffix + Common.GetTimeStamp();

                    //Check whether the default Plan Improvement Campaign exists or not
                    var objImprovementCampaign = db.Plan_Improvement_Campaign.Where(pic => pic.ImprovePlanId == Sessions.PlanId && pic.Title == planImprovementCampaignTitle).FirstOrDefault();
                    if (objImprovementCampaign != null)
                    {
                        retVal = -1;
                    }

                    Plan_Improvement_Campaign picobj = new Plan_Improvement_Campaign();
                    picobj.ImprovePlanId = Sessions.PlanId;
                    picobj.Title = planImprovementCampaignTitle;
                    picobj.Cost = DEF_COST_VALUE;
                    picobj.EffectiveDate = DateTime.Now;
                    picobj.CreatedBy = Sessions.User.UserId;
                    picobj.CreatedDate = DateTime.Now;
                    db.Entry(picobj).State = EntityState.Added;
                    int result = db.SaveChanges();
                    retVal = picobj.ImprovementPlanCampaignId;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return retVal;
        }

        #endregion

        #region Plan Improvement Program

        /// <summary>
        /// Added By: Kuber Joshi
        /// Action to create default Plan Improvement Program
        /// </summary>
        /// <returns>Returns ImprovementPlanProgramId or -1 if duplicate exists</returns>
        public int CreatePlanImprovementProgram(int improvementPlanCampaignId)
        {
            int retVal = -1;

            try
            {
                //Fetch Plan details
                Plan objPlan = new Plan();
                objPlan = db.Plans.Where(p => p.PlanId == Sessions.PlanId).FirstOrDefault();
                if (objPlan != null && improvementPlanCampaignId > 0)
                {
                    string planImprovementProgramTitle = objPlan.Title + Common.defaultImprovementSuffix + Common.GetTimeStamp();

                    //Check whether the default Plan Improvement Program exists or not
                    var objImprovementProgram = db.Plan_Improvement_Campaign_Program.Where(pip => pip.ImprovementPlanCampaignId == improvementPlanCampaignId && pip.Title == planImprovementProgramTitle).FirstOrDefault();
                    if (objImprovementProgram != null)
                    {
                        retVal = -1;
                    }

                    Plan_Improvement_Campaign_Program pipobj = new Plan_Improvement_Campaign_Program();
                    pipobj.Cost = DEF_COST_VALUE;
                    pipobj.CreatedBy = Sessions.User.UserId;
                    pipobj.CreatedDate = DateTime.Now;
                    pipobj.EffectiveDate = DateTime.Now;
                    pipobj.ImprovementPlanCampaignId = improvementPlanCampaignId;
                    pipobj.Title = planImprovementProgramTitle;
                    db.Entry(pipobj).State = EntityState.Added;
                    int result = db.SaveChanges();
                    retVal = pipobj.ImprovementPlanProgramId;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return retVal;
        }

        #endregion

        #region Plan Improvement Tactic

        /// <summary>
        /// Added By: Kuber Joshi
        /// Action to create Plan Improvement Tactic
        /// </summary>
        /// <returns>Returns ImprovementPlanTacticId or -1 if duplicate exists</returns>
        public int CreatePlanImprovementTactic(Plan_Improvement_Campaign_Program_Tactic improvementTactic)
        {
            int retVal = -1;

            try
            {
                if (improvementTactic != null)
                {
                    //Check whether the default Plan Improvement Tactic exists or not
                    var objImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(pit => pit.Title == improvementTactic.Title).FirstOrDefault();
                    if (objImprovementTactic != null)
                    {
                        retVal = -1;
                    }
                    db.Entry(improvementTactic).State = EntityState.Added;
                    int result = db.SaveChanges();
                    retVal = improvementTactic.ImprovementPlanTacticId;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return retVal;
        }

        #endregion
    }
}
