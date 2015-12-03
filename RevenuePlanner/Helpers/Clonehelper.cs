using System;
using System.Collections.Generic;
using System.Data;
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
                    return ProgramClone(PlanId, Suffix, UserId, ID, TacticStatus);

                case "Tactic":
                    return TacticClone(PlanId, Suffix, UserId, ID, TacticStatus);

                case "LineItem":
                    return LineItemClone(PlanId, Suffix, UserId, ID, TacticStatus);
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
                    //// Start - Added by Arpita Soni on 01/13/2015 for PL ticket #1127
                    proj.ModifiedDate = null;
                    proj.ModifiedBy = null;
                    //// End - Added by Arpita Soni on 01/13/2015 for PL ticket #1127
                    //// Start - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                    proj.Year = DateTime.Now.Year.ToString();
                    //// End - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                    proj.Plan_Campaign.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.CreatedDate = DateTime.Now;
                            t.Status = TacticStatus;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.Tactic_Share = null;
                            //// Start - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                            t.ModifiedDate = null;
                            t.ModifiedBy = null;
                            //// End - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                            ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                            t.IntegrationInstanceCampaignId = null;
                            t.LastSyncDate = null;
                            ////End- Added by Mitesh Vaishnav for PL ticket #1129
                            //// Start - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                            t.StartDate = t.StartDate.AddYears(DateTime.Now.Year - t.StartDate.Year);
                            t.EndDate = t.EndDate.AddYears(DateTime.Now.Year - t.EndDate.Year);
                            //// End - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                            t.Plan_Campaign_Budget = t.Plan_Campaign_Budget.ToList();
                            t.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(pcp =>
                            {
                                pcp.Plan_Campaign_Program_Tactic_Comment = null;
                                pcp.Tactic_Share = null;
                                pcp.Status = TacticStatus;
                                //// Start - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                                pcp.CreatedDate = DateTime.Now;
                                pcp.ModifiedDate = null;
                                pcp.ModifiedBy = null;
                                //// End - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                                ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                                pcp.IntegrationInstanceProgramId = null;
                                pcp.LastSyncDate = null;
                                ////End- Added by Mitesh Vaishnav for PL ticket #1129
                                //// Start - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                                pcp.StartDate = pcp.StartDate.AddYears(DateTime.Now.Year - pcp.StartDate.Year);
                                pcp.EndDate = pcp.EndDate.AddYears(DateTime.Now.Year - pcp.EndDate.Year);
                                //// End - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                                pcp.Plan_Campaign_Program_Budget = pcp.Plan_Campaign_Program_Budget.ToList();
                                pcp.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                                {
                                    pcpt.Plan_Campaign_Program_Tactic_Actual = null;
                                    pcpt.Plan_Campaign_Program_Tactic_Comment = null;
                                    pcpt.Tactic_Share = null;
                                    pcpt.Plan_Campaign_Program_Tactic1 = null;
                                    pcpt.Plan_Campaign_Program_Tactic2 = null;
                                    pcpt.Stage = null;
                                    pcpt.TacticType = null;
                                    pcpt.Status = TacticStatus;
                                    //// Start - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                                    pcpt.CreatedDate = DateTime.Now;
                                    pcpt.ModifiedDate = null;
                                    pcpt.ModifiedBy = null;
                                    pcpt.TacticCustomName = null;
                                    //// End - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                                    ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                                    pcpt.IntegrationInstanceTacticId = null;
                                    pcpt.IntegrationInstanceEloquaId = null;
                                    pcpt.LastSyncDate = null;
                                    ////End- Added by Mitesh Vaishnav for PL ticket #1129
                                    //// Start - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                                    pcpt.StartDate = pcpt.StartDate.AddYears(DateTime.Now.Year - pcpt.StartDate.Year);
                                    pcpt.EndDate = pcpt.EndDate.AddYears(DateTime.Now.Year - pcpt.EndDate.Year);
                                    //// End - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                                    pcpt.Plan_Campaign_Program_Tactic_Cost = pcpt.Plan_Campaign_Program_Tactic_Cost.ToList();
                                    pcpt.Plan_Campaign_Program_Tactic_Budget = pcpt.Plan_Campaign_Program_Tactic_Budget.ToList();
                                    pcpt.Plan_Campaign_Program_Tactic_LineItem = pcpt.Plan_Campaign_Program_Tactic_LineItem.ToList();
                                    pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                                    {
                                        //// Start - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                                        pcptl.CreatedDate = DateTime.Now;
                                        pcptl.ModifiedDate = null;
                                        pcptl.ModifiedBy = null;
                                        //// End - Added by Arpita Soni on 01/13/2015 for PL ticket #1128
                                        //// Start - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                                        pcptl.StartDate = pcptl.StartDate.HasValue ? pcptl.StartDate.Value.AddYears(DateTime.Now.Year - pcptl.StartDate.Value.Year) : pcptl.StartDate;
                                        pcptl.EndDate = pcptl.EndDate.HasValue ? pcptl.EndDate.Value.AddYears(DateTime.Now.Year - pcptl.EndDate.Value.Year) : pcptl.EndDate;
                                        //// End - Added by Sohel Pathan on 08/01/2015 for PL ticket #1102
                                        pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                        pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();// Add  By Nishant Sheth
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
                    string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();
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

                            List<int> tacticIdlist = tacticList.Select(a => a.PlanTacticId).ToList();
                            List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => tacticIdlist.Contains(p.PlanTacticId) && p.IsDeleted == false).ToList();

                            foreach (var tactic in tacticList)
                            {
                                var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                                if (clonedProgram.Plan_Campaign_Program_Tactic.Count > 0)
                                {
                                    var clonedTacticId = clonedProgram.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault();
                                    tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId.PlanTacticId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                                    // Add By Nishant Sheth
                                    // clone custom attributes
                                    var PlanLitemData = objPlanTacticLineItem.Where(p => p.PlanTacticId == tactic.PlanTacticId).ToList();
                                    foreach (var Lineitem in PlanLitemData)
                                    {
                                        var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                                        var clonedPlanLineItemId = clonedTacticId.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                                        LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                                    }
                                    // end by nishant Sheth
                                }
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
                    var Title = objPlanCampaign.Title;
                    if (objPlanCampaign.Title.Length > 234)
                    {
                        Title = objPlanCampaign.Title.Substring(0, 234);
                    }
                    objPlanCampaign.Title = (Title + Suffix);
                    objPlanCampaign.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaign.Plan = null;
                    //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaign.ModifiedDate = null;
                    objPlanCampaign.ModifiedBy = null;
                    //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaign.Plan_Campaign_Budget = objPlanCampaign.Plan_Campaign_Budget.ToList();
                    objPlanCampaign.Status = TacticStatus;
                    objPlanCampaign.IntegrationInstanceCampaignId = null;
                    ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                    objPlanCampaign.LastSyncDate = null;
                    ////End- Added by Mitesh Vaishnav for PL ticket #1129
                    objPlanCampaign.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.Tactic_Share = null;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.CreatedDate = DateTime.Now;
                            t.Status = TacticStatus;
                            //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                            t.ModifiedDate = null;
                            t.ModifiedBy = null;
                            //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                            ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                            t.IntegrationInstanceProgramId = null;
                            t.LastSyncDate = null;
                            ////End- Added by Mitesh Vaishnav for PL ticket #1129
                            t.Plan_Campaign_Program_Budget = t.Plan_Campaign_Program_Budget;
                            t.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                            {
                                pcpt.Plan_Campaign_Program_Tactic_Actual = null;
                                pcpt.Plan_Campaign_Program_Tactic_Comment = null;
                                pcpt.Tactic_Share = null;
                                pcpt.Plan_Campaign_Program_Tactic1 = null;
                                pcpt.Plan_Campaign_Program_Tactic2 = null;
                                pcpt.Stage = null;
                                pcpt.TacticType = null;
                                //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                                pcpt.CreatedDate = DateTime.Now;
                                pcpt.ModifiedDate = null;
                                pcpt.ModifiedBy = null;
                                pcpt.TacticCustomName = null;
                                //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                                ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                                pcpt.IntegrationInstanceTacticId = null;
                                pcpt.IntegrationInstanceEloquaId = null;
                                pcpt.LastSyncDate = null;
                                ////End- Added by Mitesh Vaishnav for PL ticket #1129
                                pcpt.Status = TacticStatus;
                                pcpt.Plan_Campaign_Program_Tactic_Cost = pcpt.Plan_Campaign_Program_Tactic_Cost.ToList();
                                pcpt.Plan_Campaign_Program_Tactic_Budget = pcpt.Plan_Campaign_Program_Tactic_Budget.ToList();
                                pcpt.Plan_Campaign_Program_Tactic_LineItem = pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted == false).ToList();
                                pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                                {
                                    pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                    pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();// Add  By Nishant Sheth
                                });
                            });
                            t.Plan_Campaign_Program_Tactic = t.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted == false).ToList();
                        });

                    objPlanCampaign.Plan_Campaign_Program = objPlanCampaign.Plan_Campaign_Program.Where(prgram => prgram.IsDeleted == false).ToList();
                    db.Plan_Campaign.Add(objPlanCampaign);
                    db.SaveChanges();
                    var PlanCampaignId = objPlanCampaign.PlanCampaignId;
                    HttpContext.Current.Session["CampaignID"] = PlanCampaignId;
                    ////Start Added by Mitesh Vaishnav for PL ticket #718
                    ////cloning custom field values for particular campaign,program of particular campaign and tactic of particular program
                    string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && a.CustomField.EntityType == entityTypeCampaign).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaign.PlanCampaignId; db.Entry(a).State = EntityState.Added; });

                    programList = db.Plan_Campaign_Program.Where(a => a.PlanCampaignId == ID && a.IsDeleted == false).ToList();
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();
                    foreach (var program in programList)
                    {
                        var programCustomField = db.CustomField_Entity.Where(a => a.EntityId == program.PlanProgramId && a.CustomField.EntityType == entityTypeProgram).ToList();
                        var clonedProgram = objPlanCampaign.Plan_Campaign_Program.Where(a => a.Title == program.Title).ToList().FirstOrDefault();
                        programCustomField.ForEach(a => { a.EntityId = clonedProgram.PlanProgramId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                        var tacticList = program.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted == false).ToList();

                        List<int> tacticIdlist = tacticList.Select(a => a.PlanTacticId).ToList();
                        List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => tacticIdlist.Contains(p.PlanTacticId) && p.IsDeleted == false).ToList();

                        if (clonedProgram != null && clonedProgram.Plan_Campaign_Program_Tactic.Count > 0)
                        {
                            foreach (var tactic in tacticList)
                            {
                                var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                                var clonedTacticId = clonedProgram.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault();
                                tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId.PlanTacticId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                                // Add By Nishant Sheth
                                // clone custom attributes
                                var PlanLitemData = objPlanTacticLineItem.Where(p => p.PlanTacticId == tactic.PlanTacticId).ToList();
                                foreach (var Lineitem in PlanLitemData)
                                {
                                    var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                                    var clonedPlanLineItemId = clonedTacticId.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                                    LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                                }
                                // end by nishant Sheth

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
        public int ProgramClone(int planId,string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;

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
                    HttpContext.Current.Session["CampaignID"] = objPlanCampaignPrograms.Plan_Campaign.PlanCampaignId;
                    objPlanCampaignPrograms.CreatedBy = UserId;
                    objPlanCampaignPrograms.CreatedDate = DateTime.Now;
                    var Title = objPlanCampaignPrograms.Title;
                    if (objPlanCampaignPrograms.Title.Length > 234)
                    {
                        Title = objPlanCampaignPrograms.Title.Substring(0, 234);
                    }
                    objPlanCampaignPrograms.Title = (Title + Suffix);
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaignPrograms.Plan_Campaign = null;
                    objPlanCampaignPrograms.Tactic_Share = null;
                    objPlanCampaignPrograms.Status = TacticStatus;
                    objPlanCampaignPrograms.IntegrationInstanceProgramId = null;
                    ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                    objPlanCampaignPrograms.LastSyncDate = null;
                    ////End- Added by Mitesh Vaishnav for PL ticket #1129
                    //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaignPrograms.ModifiedDate = null;
                    objPlanCampaignPrograms.ModifiedBy = null;
                    //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaignPrograms.Plan_Campaign_Program_Budget = objPlanCampaignPrograms.Plan_Campaign_Program_Budget.ToList();
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.CreatedDate = DateTime.Now;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.Plan_Campaign_Program_Tactic_Actual = null;
                            t.Plan_Campaign_Program_Tactic1 = null;
                            t.Plan_Campaign_Program_Tactic2 = null;
                            t.Stage = null;
                            t.Tactic_Share = null;
                            t.TacticType = null;
                            t.Status = TacticStatus;
                            //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                            t.ModifiedDate = null;
                            t.ModifiedBy = null;
                            t.TacticCustomName = null;
                            //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                            ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                            t.IntegrationInstanceTacticId = null;
                            t.IntegrationInstanceEloquaId = null;
                            t.LastSyncDate = null;
                            ////End- Added by Mitesh Vaishnav for PL ticket #1129
                            t.Plan_Campaign_Program_Tactic_Cost = t.Plan_Campaign_Program_Tactic_Cost.ToList();
                            t.Plan_Campaign_Program_Tactic_Budget = t.Plan_Campaign_Program_Tactic_Budget.ToList();
                            t.Plan_Campaign_Program_Tactic_LineItem = t.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted == false).ToList();
                            t.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                            {
                                pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();// Add  By Nishant Sheth
                            });
                        });
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted == false).ToList();
                    db.Plan_Campaign_Program.Add(objPlanCampaignPrograms);
                    db.SaveChanges();

                    int PlanProgramId = objPlanCampaignPrograms.PlanProgramId;
                    HttpContext.Current.Session["ProgramID"] = PlanProgramId;
                    ////Start Added by Mitesh Vaishnav for PL ticket #719
                    ////cloning custom field values for particular program and tactic of particular program
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();

                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && a.CustomField.EntityType == entityTypeProgram).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaignPrograms.PlanProgramId; db.Entry(a).State = EntityState.Added; });

                    tacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.PlanProgramId == ID && a.IsDeleted == false).ToList();
                    List<int> tacticIdlist = tacticList.Select(a => a.PlanTacticId).ToList();
                    List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => tacticIdlist.Contains(p.PlanTacticId) && p.IsDeleted == false).ToList();
                    foreach (var tactic in tacticList)
                    {
                        var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                        var clonedTacticId = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault();
                        tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId.PlanTacticId; db.Entry(a).State = EntityState.Added; });
                        // Add By Nishant Sheth
                        // clone custom attributes

                        var PlanLitemData = objPlanTacticLineItem.Where(p => p.PlanTacticId == tactic.PlanTacticId).ToList();
                        foreach (var Lineitem in PlanLitemData)
                        {
                            var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                            var clonedPlanLineItemId = clonedTacticId.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                            LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                        }

                        // end by nishant Sheth
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
        public int TacticClone(int planid, string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;
            if (ID == 0)
                return returnFlag;
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Plan_Campaign_Program_Tactic objPlanCampaignProgramTactic = db.Plan_Campaign_Program_Tactic.AsNoTracking().First(p => p.PlanTacticId == ID && p.IsDeleted == false);

                if (objPlanCampaignProgramTactic != null)
                {
                    planid = objPlanCampaignProgramTactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    HttpContext.Current.Session["ProgramID"] = objPlanCampaignProgramTactic.Plan_Campaign_Program.PlanProgramId;
                    HttpContext.Current.Session["CampaignID"] = objPlanCampaignProgramTactic.Plan_Campaign_Program.PlanCampaignId;
                    objPlanCampaignProgramTactic.Stage = null;
                    objPlanCampaignProgramTactic.Status = TacticStatus;
                    objPlanCampaignProgramTactic.CreatedBy = UserId;
                    objPlanCampaignProgramTactic.CreatedDate = DateTime.Now;
                    var Length = objPlanCampaignProgramTactic.Title.Length;
                    var Title = objPlanCampaignProgramTactic.Title;
                    if (objPlanCampaignProgramTactic.Title.Length > 234)
                    {
                        Title = objPlanCampaignProgramTactic.Title.Substring(0, 234);
                    }
                    objPlanCampaignProgramTactic.Title = Title;
                    objPlanCampaignProgramTactic.Title = (objPlanCampaignProgramTactic.Title + Suffix);
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Actual = null;
                    objPlanCampaignProgramTactic.Plan_Campaign_Program = null;
                    objPlanCampaignProgramTactic.TacticType = null;
                    objPlanCampaignProgramTactic.Tactic_Share = null;
                    objPlanCampaignProgramTactic.TacticCustomName = null;
                    objPlanCampaignProgramTactic.IntegrationInstanceTacticId = null;
                    objPlanCampaignProgramTactic.IntegrationInstanceEloquaId = null;
                    ////Start- Added by Mitesh Vaishnav for PL ticket #1129
                    objPlanCampaignProgramTactic.LastSyncDate = null;
                    ////End- Added by Mitesh Vaishnav for PL ticket #1129

                    //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaignProgramTactic.ModifiedDate = null;
                    objPlanCampaignProgramTactic.ModifiedBy = null;
                    //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                        pcptl =>
                        {
                            pcptl.LineItemType = null;
                            pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                            pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();// Add  By Nishant Sheth
                        });


                }
                objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Cost = objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Budget = objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_Budget.ToList();
                db.Plan_Campaign_Program_Tactic.Add(objPlanCampaignProgramTactic);
                db.SaveChanges();

                int planTacticId = objPlanCampaignProgramTactic.PlanTacticId;
                HttpContext.Current.Session["TacticID"] = planTacticId;
                ////Start Added by Mitesh Vaishnav for PL ticket #720
                ////cloning custom field values for particular tactic.
                string entityTypeTactic = Enums.EntityType.Tactic.ToString();

                var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && (a.CustomField.EntityType == entityTypeTactic)).ToList();
                CustomFieldsList.ForEach(a =>
                {
                    a.EntityId = objPlanCampaignProgramTactic.PlanTacticId; db.Entry(a).State = EntityState.Added;

                });

                // Add By Nishant Sheth
                // clone custom attributes
                string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();

                List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => p.PlanTacticId == ID && p.IsDeleted == false).ToList();
                foreach (var Lineitem in objPlanTacticLineItem)
                {
                    var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                    //int clonedTacticId = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault().PlanTacticId;
                    var clonedPlanLineItemId = objPlanCampaignProgramTactic.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                    LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                }
                // end by nishant Sheth

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
        public int LineItemClone( int planid,string Suffix, Guid UserId, int ID, string TacticStatus)
        {
            int returnFlag = 0;

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
                    HttpContext.Current.Session["ProgramID"] = objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanProgramId;
                    HttpContext.Current.Session["CampaignID"] = objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                    HttpContext.Current.Session["TacticID"] = TacticId;
                    objPlanCampaignProgramTacticLineItem.CreatedBy = UserId;
                    objPlanCampaignProgramTacticLineItem.CreatedDate = DateTime.Now;
                    var Title = objPlanCampaignProgramTacticLineItem.Title;
                    if (objPlanCampaignProgramTacticLineItem.Title.Length > 234)
                    {
                        Title = objPlanCampaignProgramTacticLineItem.Title.Substring(0, 234);
                    }
                    objPlanCampaignProgramTacticLineItem.Title = (Title + Suffix);
                    objPlanCampaignProgramTacticLineItem.LineItemType = null;
                    objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic = null;
                    //// Start - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaignProgramTacticLineItem.ModifiedDate = null;
                    objPlanCampaignProgramTacticLineItem.ModifiedBy = null;
                    //// End - Added by Arpita Soni on 01/15/2015 for PL ticket #1128
                    objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost = objPlanCampaignProgramTacticLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                    db.Plan_Campaign_Program_Tactic_LineItem.Add(objPlanCampaignProgramTacticLineItem);
                    db.SaveChanges();
                    returnFlag = objPlanCampaignProgramTacticLineItem.PlanLineItemId;
                    // Add By Nishant Sheth
                    // Desc : Copy Line Item Budget
                    CloneLineItemBudget(ID, returnFlag);

                    string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();
                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == ID && a.CustomField.EntityType == entityTypeLineItem).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaignProgramTacticLineItem.PlanLineItemId; db.Entry(a).State = EntityState.Added; });
                    db.SaveChanges();

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
        /// Clone Line Item Budget
        /// Added : By Nishant Sheth
        /// </summary>
        /// <param name="CopyPlanLineItemId"></param>
        /// <param name="NewPlanLineItemId"></param>
        public void CloneLineItemBudget(int ExistID, int NewID)
        {

            try
            {
                LineItem_Budget LineitemBudgetMapping = new LineItem_Budget();
                //var LineItemList = db.LineItem_Budget.Where(a => a.PlanLineItemId == ExistID).Select(a => a).ToList();
                var LineItemList = (from BudgetDetail in db.Budget_Detail
                                    join
                                        LineItemBudget in db.LineItem_Budget on BudgetDetail.Id equals LineItemBudget.BudgetDetailId
                                    where BudgetDetail.IsDeleted == false
                                    && LineItemBudget.PlanLineItemId == ExistID
                                    select new
                                    {
                                        LineItemBudget.BudgetDetailId,
                                        LineItemBudget.Weightage
                                    }).ToList();

                foreach (var item in LineItemList)
                {
                    LineitemBudgetMapping = new LineItem_Budget();
                    LineitemBudgetMapping.BudgetDetailId = item.BudgetDetailId;
                    LineitemBudgetMapping.PlanLineItemId = NewID;
                    LineitemBudgetMapping.CreatedBy = Sessions.User.UserId;
                    LineitemBudgetMapping.CreatedDate = DateTime.Now;
                    LineitemBudgetMapping.Weightage = (byte)item.Weightage;
                    db.Entry(LineitemBudgetMapping).State = EntityState.Added;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
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

        #region "Method related to Copy entities to other Plan"

        /// <summary>
        /// This method is identify the clone type 
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="CopyClone"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int CloneToOtherPlan(List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping, string cloneType = "", int entityId = 0, int PlanId = 0, int parentEntityId = 0, bool isdifferModel = false)
        {
            Guid UserId = Sessions.User.UserId;
            if (PlanId == 0)
            {
                PlanId = Sessions.PlanId;
            }
            string entityStatus = Enums.TacticStatus.Created.ToString();
            switch (cloneType)
            {
                case "Campaign":
                    return CloneCampaignToOtherPlan(PlanId, UserId, entityId, parentEntityId, entityStatus, isdifferModel, lstTacticTypeMapping);

                case "Program":
                    return CloneProgramToOtherPlan(PlanId, UserId, entityId, parentEntityId, entityStatus, isdifferModel, lstTacticTypeMapping);

                case "Tactic":
                    return CloneTacticToOtherPlan(PlanId, UserId, entityId, parentEntityId, entityStatus, isdifferModel, lstTacticTypeMapping);
            }
            return 0;
        }


        /// <summary>
        /// Clone the Tactic and it's All Child element
        /// Added : By Viral Kadiya Ticket #1748 Cloning Tactic to Same or Other Plan
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="ID"></param>
        /// <param name="TacticStatus"></param>
        /// <returns></returns>
        public int CloneTacticToOtherPlan(int planid, Guid UserId, int entityId, int parentEntityId, string TacticStatus, bool isdifferModel, List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping)
        {
            int returnFlag = 0;
            if (entityId == 0)
                return returnFlag;
            string Suffix = Common.copySuffix + Common.GetTimeStamp();
            DateTime startDate = new DateTime(), endDate=new DateTime();
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                //Plan_Campaign_Program objParentProgram =new Plan_Campaign_Program();
                //if (parentEntityId > 0)
                //    objParentProgram = db.Plan_Campaign_Program.Where(prg => prg.PlanProgramId == parentEntityId).FirstOrDefault();
                //if(objParentProgram != null)
                //{
                //    startDate = objParentProgram.StartDate;
                //    endDate = objParentProgram.EndDate;
                //}
                
                Plan_Campaign_Program_Tactic objPlanTactic = db.Plan_Campaign_Program_Tactic.AsNoTracking().First(p => p.PlanTacticId == entityId && p.IsDeleted == false);

                if (objPlanTactic != null)
                {
                    planid = objPlanTactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    HttpContext.Current.Session["ProgramID"] = objPlanTactic.Plan_Campaign_Program.PlanProgramId;
                    HttpContext.Current.Session["CampaignID"] = objPlanTactic.Plan_Campaign_Program.PlanCampaignId;
                    objPlanTactic.Stage = null;
                    objPlanTactic.Status = TacticStatus;
                    objPlanTactic.CreatedBy = UserId;
                    objPlanTactic.CreatedDate = DateTime.Now;
                    objPlanTactic.Title = (objPlanTactic.Title + Suffix);
                    objPlanTactic.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanTactic.Plan_Campaign_Program_Tactic_Actual = null;
                    objPlanTactic.Plan_Campaign_Program = null;
                    objPlanTactic.TacticType = null;
                    objPlanTactic.Tactic_Share = null;
                    objPlanTactic.TacticCustomName = null;
                    objPlanTactic.IntegrationInstanceTacticId = null;
                    objPlanTactic.IntegrationInstanceEloquaId = null;
                    objPlanTactic.IntegrationWorkFrontProjectID = null;
                    objPlanTactic.PlanProgramId = parentEntityId;
                    objPlanTactic.LastSyncDate = null;
                    objPlanTactic.ModifiedDate = null;
                    objPlanTactic.ModifiedBy = null;
                    //objPlanTactic.StartDate = startDate;
                    //objPlanTactic.EndDate = endDate;
                    objPlanTactic.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                        pcptl =>
                        {
                            pcptl.LineItemType = null;
                            pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                            pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();
                        });
                    if (isdifferModel)
                    {
                        PlanTactic_TacticTypeMapping objTacticTypeMapping = lstTacticTypeMapping.Where(tac => tac.PlanTacticId == objPlanTactic.PlanTacticId).FirstOrDefault();
                        int destTacticTypeId = objTacticTypeMapping.TacticTypeId; // Get destination model TacticTypeId.
                        objPlanTactic.TacticTypeId = destTacticTypeId; // update TacticTypeId as per destination Model TacticTypeId.

                        // Handle Tactic TargetStage scenario.
                        int? destTargetStageId = objTacticTypeMapping.TargetStageId;
                        if (!destTargetStageId.HasValue || !objPlanTactic.StageId.Equals(destTargetStageId.Value)) 
                        {
                            objPlanTactic.ProjectedStageValue = null; // if destination Model TargetStage value null Or Source & Destionation Model Target Stage are different then update ProjectedStageValue to be null.
                            objPlanTactic.StageId = destTargetStageId.HasValue? destTargetStageId.Value : 0; // Update Target StageId with destination model value.
                        }
                    }
                }
                objPlanTactic.Plan_Campaign_Program_Tactic_Cost = null; //objPlanTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                objPlanTactic.Plan_Campaign_Program_Tactic_Budget = null;//objPlanTactic.Plan_Campaign_Program_Tactic_Budget.ToList();
                db.Plan_Campaign_Program_Tactic.Add(objPlanTactic);
                db.SaveChanges();

                int planTacticId = objPlanTactic.PlanTacticId;
                HttpContext.Current.Session["TacticID"] = planTacticId;
                ////cloning custom field values for particular tactic.
                string entityTypeTactic = Enums.EntityType.Tactic.ToString();

                var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == entityId && (a.CustomField.EntityType == entityTypeTactic)).ToList();
                CustomFieldsList.ForEach(a =>
                {
                    a.EntityId = objPlanTactic.PlanTacticId; db.Entry(a).State = EntityState.Added;

                });

                // clone custom attributes
                string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();

                List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => p.PlanTacticId == entityId && p.IsDeleted == false).ToList();
                foreach (var Lineitem in objPlanTacticLineItem)
                {
                    var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                    var clonedPlanLineItemId = objPlanTactic.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                    LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                }

                db.SaveChanges();

                Common.InsertChangeLog(planid, null, returnFlag, objPlanTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
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
        /// Clone the Campaign and it's All Child element
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="Suffix"></param>
        /// <param name="UserId"></param>
        /// <param name="ID"></param>
        /// <param name="TacticStatus"></param>
        /// <returns></returns>
        public int CloneCampaignToOtherPlan(int planid, Guid UserId, int entityId, int parentEntityId, string TacticStatus, bool isdifferModel, List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping)
        {
            int returnFlag = 0;
            string title = string.Empty;

            if (planid == 0 || entityId == 0)
                return returnFlag;
            string Suffix = Common.copySuffix + Common.GetTimeStamp();
            List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                //Plan objPlan = new Plan();
                //if (parentEntityId > 0)
                //    objPlan = db.Plans.Where(pln => pln.PlanId == parentEntityId).FirstOrDefault();
                //if (objPlan != null)
                //{
                //    int count = 
                //    startDate = objParentProgram.StartDate;
                //    endDate = objParentProgram.EndDate;
                //}

                Plan_Campaign objPlanCampaign = db.Plan_Campaign.AsNoTracking().First(p => p.PlanCampaignId == entityId && p.IsDeleted == false);
                if (objPlanCampaign != null)
                {
                    objPlanCampaign.Tactic_Share = null;
                    objPlanCampaign.CreatedBy = UserId;
                    objPlanCampaign.CreatedDate = DateTime.Now;
                    objPlanCampaign.Title = (objPlanCampaign.Title + Suffix);
                    objPlanCampaign.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaign.Plan = null;
                    objPlanCampaign.ModifiedDate = null;
                    objPlanCampaign.ModifiedBy = null;
                    objPlanCampaign.Plan_Campaign_Budget = objPlanCampaign.Plan_Campaign_Budget.ToList();
                    objPlanCampaign.Status = TacticStatus;
                    objPlanCampaign.IntegrationInstanceCampaignId = null;
                    objPlanCampaign.LastSyncDate = null;
                    objPlanCampaign.PlanId = parentEntityId;
                    objPlanCampaign.Plan_Campaign_Program.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.Tactic_Share = null;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.CreatedDate = DateTime.Now;
                            t.Status = TacticStatus;
                            t.ModifiedDate = null;
                            t.ModifiedBy = null;
                            t.IntegrationInstanceProgramId = null;
                            t.LastSyncDate = null;
                            t.Plan_Campaign_Program_Budget = t.Plan_Campaign_Program_Budget;
                            t.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(pcpt =>
                            {
                                pcpt.Plan_Campaign_Program_Tactic_Actual = null;
                                pcpt.Plan_Campaign_Program_Tactic_Comment = null;
                                pcpt.Tactic_Share = null;
                                pcpt.Plan_Campaign_Program_Tactic1 = null;
                                pcpt.Plan_Campaign_Program_Tactic2 = null;
                                pcpt.Stage = null;
                                pcpt.TacticType = null;
                                pcpt.CreatedDate = DateTime.Now;
                                pcpt.ModifiedDate = null;
                                pcpt.ModifiedBy = null;
                                pcpt.TacticCustomName = null;
                                pcpt.IntegrationInstanceTacticId = null;
                                pcpt.IntegrationInstanceEloquaId = null;
                                pcpt.IntegrationWorkFrontProjectID = null;
                                pcpt.LastSyncDate = null;
                                pcpt.Status = TacticStatus;
                                pcpt.Plan_Campaign_Program_Tactic_Cost = null;//pcpt.Plan_Campaign_Program_Tactic_Cost.ToList();
                                pcpt.Plan_Campaign_Program_Tactic_Budget = null;//pcpt.Plan_Campaign_Program_Tactic_Budget.ToList();
                                pcpt.Plan_Campaign_Program_Tactic_LineItem = pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted == false).ToList();
                                pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                                {
                                    pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                    pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();
                                });
                                if (isdifferModel)
                                {
                                    PlanTactic_TacticTypeMapping objTacticTypeMapping = lstTacticTypeMapping.Where(tac => tac.PlanTacticId == pcpt.PlanTacticId).FirstOrDefault(); // Get destination model TacticTypeId.
                                    int destTacticTypeId = objTacticTypeMapping.TacticTypeId; // Get destination model TacticTypeId.
                                    pcpt.TacticTypeId = destTacticTypeId; // update TacticTypeId as per destination Model TacticTypeId.

                                    // Handle Tactic TargetStage scenario.
                                    int? destTargetStageId = objTacticTypeMapping.TargetStageId;
                                    if (!destTargetStageId.HasValue || !pcpt.StageId.Equals(destTargetStageId.Value))
                                    {
                                        pcpt.ProjectedStageValue = null; // if destination Model TargetStage value null Or Source & Destionation Model Target Stage are different then update ProjectedStageValue to be null.
                                        pcpt.StageId = destTargetStageId.HasValue ? destTargetStageId.Value : 0; // Update Target StageId with destination model value.
                                    }
                                }
                            });
                            t.Plan_Campaign_Program_Tactic = t.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted == false).ToList();
                        });

                    objPlanCampaign.Plan_Campaign_Program = objPlanCampaign.Plan_Campaign_Program.Where(prgram => prgram.IsDeleted == false).ToList();
                    db.Plan_Campaign.Add(objPlanCampaign);
                    db.SaveChanges();
                    var PlanCampaignId = objPlanCampaign.PlanCampaignId;
                    HttpContext.Current.Session["CampaignID"] = PlanCampaignId;
                    ////cloning custom field values for particular campaign,program of particular campaign and tactic of particular program
                    string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == entityId && a.CustomField.EntityType == entityTypeCampaign).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaign.PlanCampaignId; db.Entry(a).State = EntityState.Added; });

                    programList = db.Plan_Campaign_Program.Where(a => a.PlanCampaignId == entityId && a.IsDeleted == false).ToList();
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();
                    foreach (var program in programList)
                    {
                        var programCustomField = db.CustomField_Entity.Where(a => a.EntityId == program.PlanProgramId && a.CustomField.EntityType == entityTypeProgram).ToList();
                        var clonedProgram = objPlanCampaign.Plan_Campaign_Program.Where(a => a.Title == program.Title).ToList().FirstOrDefault();
                        programCustomField.ForEach(a => { a.EntityId = clonedProgram.PlanProgramId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                        var tacticList = program.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted == false).ToList();

                        List<int> tacticIdlist = tacticList.Select(a => a.PlanTacticId).ToList();
                        List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => tacticIdlist.Contains(p.PlanTacticId) && p.IsDeleted == false).ToList();

                        if (clonedProgram != null && clonedProgram.Plan_Campaign_Program_Tactic.Count > 0)
                        {
                            foreach (var tactic in tacticList)
                            {
                                var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                                var clonedTacticId = clonedProgram.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault();
                                tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId.PlanTacticId; db.Entry(a).State = EntityState.Added; CustomFieldsList.Add(a); });
                                // clone custom attributes
                                var PlanLitemData = objPlanTacticLineItem.Where(p => p.PlanTacticId == tactic.PlanTacticId).ToList();
                                foreach (var Lineitem in PlanLitemData)
                                {
                                    var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                                    var clonedPlanLineItemId = clonedTacticId.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                                    LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                                }

                            }
                        }
                    }

                    db.SaveChanges();

                    Common.InsertChangeLog(planid, null, returnFlag, objPlanCampaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
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
        public int CloneProgramToOtherPlan(int planid, Guid UserId, int entityId, int parentEntityId, string TacticStatus, bool isdifferModel, List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping)
        {
            int returnFlag = 0;

            if (entityId == 0)
                return returnFlag;
            string Suffix = Common.copySuffix + Common.GetTimeStamp();
            DateTime startDate = new DateTime(), endDate = new DateTime();
            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                //Plan_Campaign objParentCampaign = new Plan_Campaign();
                //if (parentEntityId > 0)
                //    objParentCampaign = db.Plan_Campaign.Where(cmpgn => cmpgn.PlanCampaignId == parentEntityId).FirstOrDefault();
                //if (objParentCampaign != null)
                //{
                //    startDate = objParentCampaign.StartDate;
                //    endDate = objParentCampaign.EndDate;
                //}

                Plan_Campaign_Program objPlanCampaignPrograms = db.Plan_Campaign_Program.AsNoTracking().First(p => p.PlanProgramId == entityId && p.IsDeleted == false);

                if (objPlanCampaignPrograms != null)
                {
                    planid = objPlanCampaignPrograms.Plan_Campaign.PlanId;
                    HttpContext.Current.Session["CampaignID"] = objPlanCampaignPrograms.Plan_Campaign.PlanCampaignId;
                    objPlanCampaignPrograms.CreatedBy = UserId;
                    objPlanCampaignPrograms.CreatedDate = DateTime.Now;
                    objPlanCampaignPrograms.Title = (objPlanCampaignPrograms.Title + Suffix);
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic_Comment = null;
                    objPlanCampaignPrograms.Plan_Campaign = null;
                    objPlanCampaignPrograms.Tactic_Share = null;
                    objPlanCampaignPrograms.Status = TacticStatus;
                    objPlanCampaignPrograms.IntegrationInstanceProgramId = null;
                    objPlanCampaignPrograms.LastSyncDate = null;
                    objPlanCampaignPrograms.ModifiedDate = null;
                    objPlanCampaignPrograms.ModifiedBy = null;
                    objPlanCampaignPrograms.PlanCampaignId = parentEntityId;
                    //objPlanCampaignPrograms.StartDate = startDate;
                    //objPlanCampaignPrograms.EndDate = endDate;
                    objPlanCampaignPrograms.Plan_Campaign_Program_Budget = objPlanCampaignPrograms.Plan_Campaign_Program_Budget.ToList();
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(s => s.IsDeleted == false).ToList().ForEach(
                        t =>
                        {
                            t.CreatedDate = DateTime.Now;
                            t.Plan_Campaign_Program_Tactic_Comment = null;
                            t.Plan_Campaign_Program_Tactic_Actual = null;
                            t.Plan_Campaign_Program_Tactic1 = null;
                            t.Plan_Campaign_Program_Tactic2 = null;
                            t.Stage = null;
                            t.Tactic_Share = null;
                            t.TacticType = null;
                            t.Status = TacticStatus;
                            t.ModifiedDate = null;
                            t.ModifiedBy = null;
                            t.TacticCustomName = null;
                            t.IntegrationInstanceTacticId = null;
                            t.IntegrationInstanceEloquaId = null;
                            t.IntegrationWorkFrontProjectID = null;
                            t.LastSyncDate = null;
                            t.Plan_Campaign_Program_Tactic_Cost = null;//t.Plan_Campaign_Program_Tactic_Cost.ToList();
                            t.Plan_Campaign_Program_Tactic_Budget = null; //t.Plan_Campaign_Program_Tactic_Budget.ToList();
                            //t.StartDate = startDate;
                            //t.EndDate = endDate;
                            t.Plan_Campaign_Program_Tactic_LineItem = t.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.IsDeleted == false).ToList();
                            t.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.IsDeleted == false).ToList().ForEach(pcptl =>
                            {
                                pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost = pcptl.Plan_Campaign_Program_Tactic_LineItem_Cost.ToList();
                                pcptl.LineItem_Budget = pcptl.LineItem_Budget.ToList();
                            });
                            if (isdifferModel)
                            {
                                PlanTactic_TacticTypeMapping objTacticTypeMapping = lstTacticTypeMapping.Where(tac => tac.PlanTacticId == t.PlanTacticId).FirstOrDefault(); // Get destination model TacticTypeId.
                                int destTacticTypeId = objTacticTypeMapping.TacticTypeId; // Get destination model TacticTypeId.
                                t.TacticTypeId = destTacticTypeId; // update TacticTypeId as per destination Model TacticTypeId.

                                // Handle Tactic TargetStage scenario.
                                int? destTargetStageId = objTacticTypeMapping.TargetStageId;
                                if (!destTargetStageId.HasValue || !t.StageId.Equals(destTargetStageId.Value))
                                {
                                    t.ProjectedStageValue = null; // if destination Model TargetStage value null Or Source & Destionation Model Target Stage are different then update ProjectedStageValue to be null.
                                    t.StageId = destTargetStageId.HasValue ? destTargetStageId.Value : 0; // Update Target StageId with destination model value.
                                }
                            }
                        });
                    objPlanCampaignPrograms.Plan_Campaign_Program_Tactic = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted == false).ToList();
                    db.Plan_Campaign_Program.Add(objPlanCampaignPrograms);
                    db.SaveChanges();

                    int PlanProgramId = objPlanCampaignPrograms.PlanProgramId;
                    HttpContext.Current.Session["ProgramID"] = PlanProgramId;
                    ////cloning custom field values for particular program and tactic of particular program
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    string entityTypeLineItem = Enums.EntityType.Lineitem.ToString();

                    var CustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == entityId && a.CustomField.EntityType == entityTypeProgram).ToList();
                    CustomFieldsList.ForEach(a => { a.EntityId = objPlanCampaignPrograms.PlanProgramId; db.Entry(a).State = EntityState.Added; });

                    tacticList = db.Plan_Campaign_Program_Tactic.Where(a => a.PlanProgramId == entityId && a.IsDeleted == false).ToList();
                    List<int> tacticIdlist = tacticList.Select(a => a.PlanTacticId).ToList();
                    List<Plan_Campaign_Program_Tactic_LineItem> objPlanTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => tacticIdlist.Contains(p.PlanTacticId) && p.IsDeleted == false).ToList();
                    foreach (var tactic in tacticList)
                    {
                        var tacticCustomField = db.CustomField_Entity.Where(a => a.EntityId == tactic.PlanTacticId && a.CustomField.EntityType == entityTypeTactic).ToList();
                        var clonedTacticId = objPlanCampaignPrograms.Plan_Campaign_Program_Tactic.Where(a => a.Title == tactic.Title).ToList().FirstOrDefault();
                        tacticCustomField.ForEach(a => { a.EntityId = clonedTacticId.PlanTacticId; db.Entry(a).State = EntityState.Added; });
                        // clone custom attributes
                        var PlanLitemData = objPlanTacticLineItem.Where(p => p.PlanTacticId == tactic.PlanTacticId).ToList();
                        foreach (var Lineitem in PlanLitemData)
                        {
                            var LineItemCustomFieldsList = db.CustomField_Entity.Where(a => a.EntityId == Lineitem.PlanLineItemId && a.CustomField.EntityType == entityTypeLineItem).ToList();
                            var clonedPlanLineItemId = clonedTacticId.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Title == Lineitem.Title).FirstOrDefault().PlanLineItemId;
                            LineItemCustomFieldsList.ForEach(a => { a.EntityId = clonedPlanLineItemId; db.Entry(a).State = EntityState.Added; });
                        }
                    }

                    db.SaveChanges();

                    Common.InsertChangeLog(planid, null, returnFlag, objPlanCampaignPrograms.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
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

        #endregion
    }
}