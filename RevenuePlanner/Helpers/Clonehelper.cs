using System;
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
        public MRPEntities db = new MRPEntities();

        /// <summary>
        /// This method is identify the clone type 
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="CopyClone"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int ToClone(string Suffix = "", string CopyClone = "", int ID = 0, int PlanId = 0)
        {
            Guid UserId = Sessions.User.UserId;
            if (PlanId == 0)
            {
                PlanId = Sessions.PlanId;
            }
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
                    return ProgramClone(Suffix, UserId, ID, TacticStatus);

                case "Tactic":
                    return TacticClone(Suffix, UserId, ID, TacticStatus);

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
        public int PlanClone(int PlanId, string Suffix, Guid UserId, string PlanStatus, string TacticStatus)
        {
            int returnFlag = 0;
            if (PlanId == 0)
                return returnFlag;

            List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Plan proj = db.Plans.AsNoTracking().First(p => p.PlanId == PlanId && p.IsDeleted == false);
                if (proj != null)
                {
                    proj.CreatedBy = UserId;
                    proj.CreatedDate = DateTime.Now;
                    proj.Title = (proj.Title + Suffix);
                    proj.Status = PlanStatus;
                    proj.Model = null;
                    proj.Plan_Improvement_Campaign = null;
                    proj.Plan_Budget = proj.Plan_Budget.ToList();
                    proj.Plan_Team = null;
                    proj.Plan_Campaign.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.CreatedDate = DateTime.Now;
                            t.Status = TacticStatus;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.Tactic_Share = null;
                            t.Vertical = null;
                            t.Audience = null;
                            t.Geography = null;
                            t.Plan_Campaign_Budget = t.Plan_Campaign_Budget.ToList();
                            t.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(pcp =>
                            {
                                pcp.Plan_Campaign_Program_Tactic_Comment = null;
                                pcp.Tactic_Share = null;
                                pcp.Audience = null;
                                pcp.Geography = null;
                                pcp.Vertical = null;
                                pcp.Status = TacticStatus;
                                pcp.Plan_Campaign_Program_Budget = pcp.Plan_Campaign_Program_Budget.ToList();
                                pcp.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                                {
                                    pcpt.Plan_Campaign_Program_Tactic_Actual = null;
                                    pcpt.Plan_Campaign_Program_Tactic_Comment = null;
                                    pcpt.Tactic_Share = null;
                                    pcpt.Vertical = null;
                                    pcpt.Audience = null;
                                    pcpt.BusinessUnit = null;
                                    pcpt.Geography = null;
                                    pcpt.Plan_Campaign_Program_Tactic1 = null;
                                    pcpt.Plan_Campaign_Program_Tactic2 = null;
                                    pcpt.Stage = null;
                                    pcpt.TacticType = null;
                                    pcpt.Status = TacticStatus;
                                    pcpt.Plan_Campaign_Program_Tactic_Cost = pcpt.Plan_Campaign_Program_Tactic_Cost.ToList();
                                    pcpt.Plan_Campaign_Program_Tactic_LineItem = pcpt.Plan_Campaign_Program_Tactic_LineItem.ToList();
                                    pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                                    {
                                        pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                    });
                                });
                            });
                        });
                    proj.Plan_Campaign = proj.Plan_Campaign.ToList();
                    db.Plans.Add(proj);
                    db.SaveChanges();

                    int planId = proj.PlanId;

                    ////Start Added by Mitesh Vaishnav for PL ticket #718
                    ////cloning custom field values for particular plan's campaign,program of particular campaign and tactic of particular program
                    string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();

                    List<CustomField_Entity> CustomFieldsList = new List<CustomField_Entity>();
                    campaignList = db.Plan_Campaign.Where(a => a.PlanId == PlanId && a.IsDeleted == false).ToList();
                    foreach (var campaign in campaignList)
                    {
                        var campaignCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == campaign.PlanCampaignId && a.CustomField.EntityType == entityTypeCampaign).ToList();
                        var clonedCampaign = proj.Plan_Campaign.Where(a => a.Title == campaign.Title && a.IsDeleted == false).FirstOrDefault();
                        campaignCustomFieldsList.ForEach(a => { a.EntityId = clonedCampaign.PlanCampaignId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                        var programList = campaign.Plan_Campaign_Program.Where(a => a.IsDeleted == false).ToList();

                        foreach (var program in programList)
                        {
                            var programCustomField = db.CustomField_Entity.Where(a => a.EntityId == program.PlanProgramId && a.CustomField.EntityType == entityTypeProgram).ToList();
                            var clonedProgram = clonedCampaign.Plan_Campaign_Program.Where(a => a.Title == program.Title).ToList().FirstOrDefault();
                            programCustomField.ForEach(a => { a.EntityId = clonedProgram.PlanProgramId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                            var tacticList = program.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted == false).ToList();

                            foreach (var tactic in tacticList)
                            {
                                var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                                int clonedTacticId = clonedProgram.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault().PlanTacticId;
                                tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                            }

                        }

                    }
                    db.SaveChanges();
                    ////End Added by Mitesh Vaishnav for PL ticket #718

                    Sessions.PlanId = proj.PlanId;
                    Common.InsertChangeLog(Sessions.PlanId, null, returnFlag, proj.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                    returnFlag = planId;
                    return returnFlag;
                }
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
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
        public int CampaignClone(int PlanId, string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;
            string title = string.Empty;

            if (PlanId == 0 || ID == 0)
                return returnFlag;

            List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Plan_Campaign objPlanCampaign = db.Plan_Campaign.AsNoTracking().First(p => p.PlanId == PlanId && p.PlanCampaignId == ID && p.IsDeleted == false);
                if (objPlanCampaign != null)
                {
                    objPlanCampaign.Tactic_Share = null;
                    objPlanCampaign.CreatedBy = UserId;
                    objPlanCampaign.CreatedDate = DateTime.Now;
                    objPlanCampaign.Title = (objPlanCampaign.Title + Suffix);
                    objPlanCampaign.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaign.Plan = null;
                    objPlanCampaign.Audience = null;
                    objPlanCampaign.Geography = null;
                    objPlanCampaign.Plan_Campaign_Budget = objPlanCampaign.Plan_Campaign_Budget.ToList();
                    objPlanCampaign.Vertical = null;
                    objPlanCampaign.Status = TacticStatus;
                    objPlanCampaign.IntegrationInstanceCampaignId = null;
                    objPlanCampaign.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.Tactic_Share = null;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.CreatedDate = DateTime.Now;
                            t.Status = TacticStatus;
                            t.Vertical = null;
                            t.Audience = null;
                            t.Geography = null;
                            t.Plan_Campaign_Program_Budget = t.Plan_Campaign_Program_Budget;
                            t.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                            {
                                pcpt.Plan_Campaign_Program_Tactic_Actual = null;
                                pcpt.Plan_Campaign_Program_Tactic_Comment = null;
                                pcpt.Tactic_Share = null;
                                pcpt.Vertical = null;
                                pcpt.Audience = null;
                                pcpt.BusinessUnit = null;
                                pcpt.Geography = null;
                                pcpt.Plan_Campaign_Program_Tactic1 = null;
                                pcpt.Plan_Campaign_Program_Tactic2 = null;
                                pcpt.Stage = null;
                                pcpt.TacticType = null;
                                pcpt.Status = TacticStatus;
                                pcpt.Plan_Campaign_Program_Tactic_Cost = pcpt.Plan_Campaign_Program_Tactic_Cost.ToList();
                                pcpt.Plan_Campaign_Program_Tactic_LineItem = pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted == false).ToList();
                                pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                                {
                                    pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                });
                            });
                            t.Plan_Campaign_Program_Tactic = t.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted == false).ToList();
                        });

                    objPlanCampaign.Plan_Campaign_Program = objPlanCampaign.Plan_Campaign_Program.Where(prgram => prgram.IsDeleted == false).ToList();
                    db.Plan_Campaign.Add(objPlanCampaign);
                    db.SaveChanges();
                    var PlanCampaignId = objPlanCampaign.PlanCampaignId;
                    ////Start Added by Mitesh Vaishnav for PL ticket #718
                    ////cloning custom field values for particular campaign,program of particular campaign and tactic of particular program
                    string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && a.CustomField.EntityType == entityTypeCampaign).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaign.PlanCampaignId; db.Entry(a).State = EntityState.Added; });

                    programList = db.Plan_Campaign_Program.Where(a => a.PlanCampaignId == ID && a.IsDeleted == false).ToList();
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    foreach (var program in programList)
                    {
                        var programCustomField = db.CustomField_Entity.Where(a => a.EntityId == program.PlanProgramId && a.CustomField.EntityType == entityTypeProgram).ToList();
                        var clonedProgram = objPlanCampaign.Plan_Campaign_Program.Where(a => a.Title == program.Title).ToList().FirstOrDefault();
                        programCustomField.ForEach(a => { a.EntityId = clonedProgram.PlanProgramId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                        var tacticList = program.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted == false).ToList();

                        if (clonedProgram != null && clonedProgram.Plan_Campaign_Program_Tactic.Count > 0)
                        {
                            foreach (var tactic in tacticList)
                            {
                                var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                                int clonedTacticId = clonedProgram.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault().PlanTacticId;
                                tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                            }
                        }
                    }

                    db.SaveChanges();
                    ////End Added by Mitesh Vaishnav for PL ticket #718

                    Common.InsertChangeLog(PlanId, null, returnFlag, objPlanCampaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                    returnFlag = PlanCampaignId;
                    return returnFlag;
                }
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
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
        public int ProgramClone(string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;
            int planId = Sessions.PlanId;

            if (ID == 0)
                return returnFlag;

            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Plan_Campaign_Program objPlanCampaignPrograms = db.Plan_Campaign_Program.AsNoTracking().First(p => p.PlanProgramId == ID && p.IsDeleted == false);

                if (objPlanCampaignPrograms != null)
                {
                    planId = objPlanCampaignPrograms.Plan_Campaign.PlanId;
                    objPlanCampaignPrograms.CreatedBy = UserId;
                    objPlanCampaignPrograms.CreatedDate = DateTime.Now;
                    objPlanCampaignPrograms.Title = (objPlanCampaignPrograms.Title + Suffix);
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaignPrograms.Plan_Campaign = null;
                    objPlanCampaignPrograms.Audience = null;
                    objPlanCampaignPrograms.Geography = null;
                    objPlanCampaignPrograms.Tactic_Share = null;
                    objPlanCampaignPrograms.Vertical = null;
                    objPlanCampaignPrograms.Status = TacticStatus;
                    objPlanCampaignPrograms.IntegrationInstanceProgramId = null;
                    objPlanCampaignPrograms.Plan_Campaign_Program_Budget = objPlanCampaignPrograms.Plan_Campaign_Program_Budget.ToList();
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.CreatedDate = DateTime.Now;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.Plan_Campaign_Program_Tactic_Actual = null;
                            t.Audience = null;
                            t.BusinessUnit = null;
                            t.Geography = null;
                            t.Plan_Campaign_Program_Tactic1 = null;
                            t.Plan_Campaign_Program_Tactic2 = null;
                            t.Stage = null;
                            t.Tactic_Share = null;
                            t.TacticType = null;
                            t.Vertical = null;
                            t.Status = TacticStatus;
                            t.Plan_Campaign_Program_Tactic_Cost = t.Plan_Campaign_Program_Tactic_Cost.ToList();
                            t.Plan_Campaign_Program_Tactic_LineItem = t.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted == false).ToList();
                            t.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                            {
                                pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                            });
                        });
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted == false).ToList();
                    db.Plan_Campaign_Program.Add(objPlanCampaignPrograms);
                    db.SaveChanges();

                    int PlanProgramId = objPlanCampaignPrograms.PlanProgramId;

                    ////Start Added by Mitesh Vaishnav for PL ticket #719
                    ////cloning custom field values for particular program and tactic of particular program
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && a.CustomField.EntityType == entityTypeProgram).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaignPrograms.PlanProgramId; db.Entry(a).State = EntityState.Added; });

                    tacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.PlanProgramId == ID && a.IsDeleted == false).ToList();

                    foreach (var tactic in tacticList)
                    {
                        var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                        int clonedTacticId = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault().PlanTacticId;
                        tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                    }

                    db.SaveChanges();
                    ////End Added by Mitesh Vaishnav for PL ticket #719

                    Common.InsertChangeLog(planId, null, returnFlag, objPlanCampaignPrograms.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                    returnFlag = PlanProgramId;
                    return returnFlag;
                }
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
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
        public int TacticClone(string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;
            int planid = Sessions.PlanId;
            if (ID == 0)
                return returnFlag;
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Plan_Campaign_Program_Tactic objPlanCampaignProgramTactic = db.Plan_Campaign_Program_Tactic.AsNoTracking().First(p => p.PlanTacticId == ID && p.IsDeleted == false);

                if (objPlanCampaignProgramTactic != null)
                {
                    planid = objPlanCampaignProgramTactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
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
                    //objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic1 = null;
                    //objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic2 = null;
                    objPlanCampaignProgramTactic.Vertical = null;
                    objPlanCampaignProgramTactic.TacticType = null;
                    objPlanCampaignProgramTactic.Tactic_Share = null;
                    objPlanCampaignProgramTactic.TacticCustomName = null;
                    objPlanCampaignProgramTactic.IntegrationInstanceTacticId = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                        pcptl =>
                        {
                            pcptl.LineItemType = null;
                            pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                        });
                }
                objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Cost = objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                db.Plan_Campaign_Program_Tactic.Add(objPlanCampaignProgramTactic);
                db.SaveChanges();

                int planTacticId = objPlanCampaignProgramTactic.PlanTacticId;

                ////Start Added by Mitesh Vaishnav for PL ticket #720
                ////cloning custom field values for particular tactic.
                string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && a.CustomField.EntityType == entityTypeTactic).ToList();
                CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaignProgramTactic.PlanTacticId; db.Entry(a).State = EntityState.Added; });
                db.SaveChanges();
                ////End Added by Mitesh Vaishnav for PL ticket #720

                Common.InsertChangeLog(planid, null, returnFlag, objPlanCampaignProgramTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                returnFlag = planTacticId;
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
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
        public int LineItemClone(string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;
            int planid = Sessions.PlanId;

            if (ID == 0)
                return returnFlag;
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Plan_Campaign_Program_Tactic_LineItem objPlanCampaignProgramTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.AsNoTracking().First(p => p.PlanLineItemId == ID && p.IsDeleted == false);
                if (objPlanCampaignProgramTacticLineItem != null)
                {
                    planid = objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
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
                    Common.InsertChangeLog(planid, null, returnFlag, objPlanCampaignProgramTacticLineItem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                }
                return returnFlag;
            }
            catch (AmbiguousMatchException)
            {
                return returnFlag;
            }
            finally
            {
                db.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// Cost Calculacation for Line Items
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="TacticId"></param>
        public void CostCalculacation(int TacticId)
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
                if (objtacticLineItemOthers.Cost == 0)
                {
                    objtacticLineItemOthers.IsDeleted = true;
                }
                db.Entry(objtacticLineItemOthers).State = EntityState.Modified;
                int result = db.SaveChanges();
            }
        }

    }
}