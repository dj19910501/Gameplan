﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Web;
using RevenuePlanner.Models;

namespace RevenuePlanner.Helpers
{
    public class Clonehelper
    {
        public static MRPEntities db = new MRPEntities();

        /// <summary>
        /// This method is identify the clone type 
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="CopyClone"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static int ToClone(int PlanId = 0, string Suffix = "", string CopyClone = "", int ID = 0)
        {
            Guid UserId = Sessions.User.UserId;
            Suffix = !string.IsNullOrEmpty(Suffix) ? Suffix : Common.copySuffix + Common.GetTimeStamp();
            string PlanStatus = Enums.PlanStatus.Draft.ToString();
            string TacticStatus = Enums.TacticStatus.Created.ToString();
            switch (CopyClone)
            {
                case "Plan":
                    return PlanClone(PlanId, Suffix, UserId, PlanStatus, TacticStatus);

                case "Campaign":
                    return CampaignClone(PlanId, Suffix, UserId, ID, TacticStatus);

                case "Program":
                    return ProgramClone(PlanId, Suffix, UserId, ID);

                case "Tactic":
                    return TacticClone(PlanId, Suffix, UserId, ID, TacticStatus);

                case "LineItem":
                    return LineItemClone(Suffix, UserId, ID, TacticStatus);
            }
            return 0;
        }

        /// <summary>
        /// Clone the Plan and it's All Child element
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="PlanStatus"></param>
        /// <param name="TacticStatus"></param>
        /// <returns></returns>
        public static int PlanClone(int PlanId, string Suffix, Guid UserId, string PlanStatus, string TacticStatus)
        {
            int returnFlag = 0;
            if (PlanId == 0)
                return returnFlag;
            try
            {
                Plan proj = db.Plans.AsNoTracking().First(p => p.PlanId == PlanId && p.IsDeleted == false);
                if (proj != null)
                {
                    proj.CreatedBy = UserId;
                    proj.CreatedDate = DateTime.Now;
                    proj.Title = (proj.Title + Suffix);
                    proj.Status = PlanStatus;
                    proj.Model = null;
                    proj.Plan_Improvement_Campaign = null;
                    proj.Plan_Team = null;
                    proj.Plan_Campaign.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.CreatedDate = DateTime.Now;
                            t.Status = TacticStatus;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.Tactic_Share = null;
                            t.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(pcp =>
                            {
                                pcp.Plan_Campaign_Program_Tactic_Comment = null;
                                pcp.Tactic_Share = null;
                                pcp.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                                {
                                    pcpt.Plan_Campaign_Program_Tactic_Actual = null;
                                    pcpt.Plan_Campaign_Program_Tactic_Comment = null;
                                    pcpt.Tactic_Share = null;
                                });
                            });
                        });
                    proj.Plan_Campaign = proj.Plan_Campaign.ToList();
                    db.Plans.Add(proj);
                    db.SaveChanges();
                    returnFlag = 1;
                }
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }

        }

        /// <summary>
        /// Clone the Campaign and it's All Child element
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="ID"></param>
        /// <param name="TacticStatus"></param>
        /// <returns></returns>
        public static int CampaignClone(int PlanId, string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;

            if (PlanId == 0 || ID == 0)
                return returnFlag;
            try
            {
                var objPlanCampaign = db.Plan_Campaign.AsNoTracking().Where(p => p.PlanId == PlanId && p.PlanCampaignId == ID && p.IsDeleted == false).ToList();
                if (objPlanCampaign != null)
                {
                    objPlanCampaign.ForEach(
                    pc =>
                    {
                        pc.Tactic_Share = null;
                        pc.CreatedBy = UserId;
                        pc.CreatedDate = DateTime.Now;
                        pc.Title = (pc.Title + Suffix);
                        pc.Plan_Campaign_Program_Tactic_Comment = null;
                        pc.Plan = null;
                        pc.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(
                            t =>
                            {
                                t.Tactic_Share = null;
                                t.Plan_Campaign_Program_Tactic_Comment = null;
                                t.CreatedDate = DateTime.Now;
                                t.Status = TacticStatus;
                            });
                        db.Plan_Campaign.Add(pc);
                    });
                    //db.SaveChanges();   
                }
                return 1;
            }
            catch (AmbiguousMatchException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Clone the Program and it's All Child element
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static int ProgramClone(int PlanId, string Suffix, Guid UserId, int ID)
        {
            int returnFlag = 0;

            if (PlanId == 0 || ID == 0)
                return returnFlag;
            try
            {
                var objPlanCampaignPrograms = db.Plan_Campaign_Program.AsNoTracking().Where(p => p.PlanProgramId == ID && p.IsDeleted == false).ToList();

                if (objPlanCampaignPrograms != null)
                {
                    objPlanCampaignPrograms.ForEach(
                        pcp =>
                        {
                            pcp.CreatedBy = UserId;
                            pcp.CreatedDate = DateTime.Now;
                            pcp.Title = (pcp.Title + Suffix);
                            pcp.Plan_Campaign_Program_Tactic_Comment = null;
                            pcp.Plan_Campaign = null;

                            pcp.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(
                                t =>
                                {
                                    t.CreatedDate = DateTime.Now;
                                    t.Plan_Campaign_Program_Tactic_Comment = null;
                                    t.Plan_Campaign_Program_Tactic_Actual = null;
                                });

                            db.Plan_Campaign_Program.Add(pcp);
                        });
                    //db.SaveChanges();   
                }
                return 1;
            }
            catch (AmbiguousMatchException)
            {
                return 0;
            }

        }

        /// <summary>
        /// Clone the Tactic and it's All Child element
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="ID"></param>
        /// <param name="TacticStatus"></param>
        /// <returns></returns>
        public static int TacticClone(int PlanId, string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;

            if (ID == 0)
                return returnFlag;
            try
            {
                Plan_Campaign_Program_Tactic objPlanCampaignProgramTactic = db.Plan_Campaign_Program_Tactic.AsNoTracking().First(p => p.PlanTacticId == ID && p.IsDeleted == false);

                if (objPlanCampaignProgramTactic != null)
                {
                    objPlanCampaignProgramTactic.Stage = null;
                    objPlanCampaignProgramTactic.Status = TacticStatus;
                    objPlanCampaignProgramTactic.CreatedBy = UserId;
                    objPlanCampaignProgramTactic.CreatedDate = DateTime.Now;
                    objPlanCampaignProgramTactic.Title = (objPlanCampaignProgramTactic.Title + Suffix);
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Actual = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program = null;
                    objPlanCampaignProgramTactic.Audience = null;
                    objPlanCampaignProgramTactic.BusinessUnit = null;
                    objPlanCampaignProgramTactic.Geography = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic1 = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic2 = null;
                    objPlanCampaignProgramTactic.Vertical = null;
                    objPlanCampaignProgramTactic.TacticType = null;
                    objPlanCampaignProgramTactic.Tactic_Share = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_LineItem.ToList().ForEach(
                        pcptl =>
                        {
                            pcptl.LineItemType = null;
                            pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                        }
                             );
                }
                objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Cost = objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                db.Plan_Campaign_Program_Tactic.Add(objPlanCampaignProgramTactic);
                db.SaveChanges();
                Common.InsertChangeLog(Sessions.PlanId, null, returnFlag, objPlanCampaignProgramTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                returnFlag = 1;
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }

        }

        /// <summary>
        /// Clone the Line Items and it's All Child element
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="ID"></param>
        /// <param name="TacticStatus"></param>
        /// <returns></returns>
        public static int LineItemClone(string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;

            if (ID == 0)
                return returnFlag;
            try
            {
                Plan_Campaign_Program_Tactic_LineItem objPlanCampaignProgramTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.AsNoTracking().First(p => p.PlanLineItemId == ID && p.IsDeleted == false);
                if (objPlanCampaignProgramTacticLineItem != null)
                {
                    int TacticId = objPlanCampaignProgramTacticLineItem.PlanTacticId;
                    objPlanCampaignProgramTacticLineItem.CreatedBy = UserId;
                    objPlanCampaignProgramTacticLineItem.CreatedDate = DateTime.Now;
                    objPlanCampaignProgramTacticLineItem.Title = (objPlanCampaignProgramTacticLineItem.Title + Suffix);
                    objPlanCampaignProgramTacticLineItem.LineItemType = null;
                    objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic = null;
                    objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost = objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                    db.Plan_Campaign_Program_Tactic_LineItem.Add(objPlanCampaignProgramTacticLineItem);
                    db.SaveChanges();
                    returnFlag = objPlanCampaignProgramTacticLineItem.PlanLineItemId;
                    CostCalculacation(TacticId);
                    Common.InsertChangeLog(Sessions.PlanId, null, returnFlag, objPlanCampaignProgramTacticLineItem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                }
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }
        }

        /// <summary>
        /// Cost Calculacation for Line Items
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="TacticId"></param>
        public static void CostCalculacation(int TacticId)
        {
            Plan_Campaign_Program_Tactic ObjTactic = db.Plan_Campaign_Program_Tactic.SingleOrDefault(t => t.PlanTacticId == TacticId);
            var objtacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == TacticId && s.IsDeleted == false && s.LineItemTypeId != null).ToList();
            Plan_Campaign_Program_Tactic_LineItem objtacticLineItemOthers = db.Plan_Campaign_Program_Tactic_LineItem.SingleOrDefault(s => s.PlanTacticId == TacticId && s.IsDeleted == false
                && s.LineItemTypeId == null);

            double CostSum = 0;
            if (objtacticLineItem.Count > 0)
            {
                CostSum = objtacticLineItem.Sum(a => a.Cost);
            }
            if (objtacticLineItemOthers != null)
            {
                objtacticLineItemOthers.Cost = (ObjTactic.Cost > CostSum) ? (ObjTactic.Cost - CostSum) : 0;
                db.Entry(objtacticLineItemOthers).State = EntityState.Modified;
                int result = db.SaveChanges();
            }
        }

    }
}