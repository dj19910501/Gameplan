﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RevenuePlanner.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class MRPEntities : DbContext
    {
        public MRPEntities()
            : base("name=MRPEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Audience> Audiences { get; set; }
        public DbSet<BestInClass> BestInClasses { get; set; }
        public DbSet<BusinessUnit> BusinessUnits { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<ClientTacticType> ClientTacticTypes { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Funnel> Funnels { get; set; }
        public DbSet<Funnel_Field> Funnel_Field { get; set; }
        public DbSet<GameplanDataType> GameplanDataTypes { get; set; }
        public DbSet<Geography> Geographies { get; set; }
        public DbSet<ImprovementTacticType> ImprovementTacticTypes { get; set; }
        public DbSet<ImprovementTacticType_Metric> ImprovementTacticType_Metric { get; set; }
        public DbSet<IntegrationInstance> IntegrationInstances { get; set; }
        public DbSet<IntegrationInstance_Attribute> IntegrationInstance_Attribute { get; set; }
        public DbSet<IntegrationInstanceDataTypeMapping> IntegrationInstanceDataTypeMappings { get; set; }
        public DbSet<IntegrationInstanceLog> IntegrationInstanceLogs { get; set; }
        public DbSet<IntegrationInstancePlanEntityLog> IntegrationInstancePlanEntityLogs { get; set; }
        public DbSet<IntegrationType> IntegrationTypes { get; set; }
        public DbSet<IntegrationTypeAttribute> IntegrationTypeAttributes { get; set; }
        public DbSet<LineItemType> LineItemTypes { get; set; }
        public DbSet<MasterTacticType> MasterTacticTypes { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Model_Audience_Event> Model_Audience_Event { get; set; }
        public DbSet<Model_Audience_Inbound> Model_Audience_Inbound { get; set; }
        public DbSet<Model_Audience_Outbound> Model_Audience_Outbound { get; set; }
        public DbSet<Model_Funnel> Model_Funnel { get; set; }
        public DbSet<Model_Funnel_Stage> Model_Funnel_Stage { get; set; }
        public DbSet<ModelReview> ModelReviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Plan_Budget> Plan_Budget { get; set; }
        public DbSet<Plan_Campaign> Plan_Campaign { get; set; }
        public DbSet<Plan_Campaign_Budget> Plan_Campaign_Budget { get; set; }
        public DbSet<Plan_Campaign_Program> Plan_Campaign_Program { get; set; }
        public DbSet<Plan_Campaign_Program_Budget> Plan_Campaign_Program_Budget { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic> Plan_Campaign_Program_Tactic { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Actual> Plan_Campaign_Program_Tactic_Actual { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Comment> Plan_Campaign_Program_Tactic_Comment { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Cost> Plan_Campaign_Program_Tactic_Cost { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem> Plan_Campaign_Program_Tactic_LineItem { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem_Cost> Plan_Campaign_Program_Tactic_LineItem_Cost { get; set; }
        public DbSet<Plan_Improvement_Campaign> Plan_Improvement_Campaign { get; set; }
        public DbSet<Plan_Improvement_Campaign_Program> Plan_Improvement_Campaign_Program { get; set; }
        public DbSet<Plan_Improvement_Campaign_Program_Tactic> Plan_Improvement_Campaign_Program_Tactic { get; set; }
        public DbSet<Plan_Improvement_Campaign_Program_Tactic_Comment> Plan_Improvement_Campaign_Program_Tactic_Comment { get; set; }
        public DbSet<Plan_Improvement_Campaign_Program_Tactic_Share> Plan_Improvement_Campaign_Program_Tactic_Share { get; set; }
        public DbSet<Plan_Team> Plan_Team { get; set; }
        public DbSet<Report_Share> Report_Share { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<SyncFrequency> SyncFrequencies { get; set; }
        public DbSet<Tactic_Share> Tactic_Share { get; set; }
        public DbSet<TacticType> TacticTypes { get; set; }
        public DbSet<User_Filter> User_Filter { get; set; }
        public DbSet<User_Notification> User_Notification { get; set; }
        public DbSet<Vertical> Verticals { get; set; }
    
        public virtual int Plan_Campaign_Program_Tactic_ActualDelete(Nullable<int> planTacticId, ObjectParameter returnValue)
        {
            var planTacticIdParameter = planTacticId.HasValue ?
                new ObjectParameter("PlanTacticId", planTacticId) :
                new ObjectParameter("PlanTacticId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("Plan_Campaign_Program_Tactic_ActualDelete", planTacticIdParameter, returnValue);
        }
    
        public virtual int PlanDuplicate(Nullable<int> planId, string planStatus, string tacticStatus, Nullable<System.DateTime> createdDate, Nullable<System.Guid> createdBy, string suffix, string copyClone, Nullable<int> id, ObjectParameter returnValue)
        {
            var planIdParameter = planId.HasValue ?
                new ObjectParameter("PlanId", planId) :
                new ObjectParameter("PlanId", typeof(int));
    
            var planStatusParameter = planStatus != null ?
                new ObjectParameter("PlanStatus", planStatus) :
                new ObjectParameter("PlanStatus", typeof(string));
    
            var tacticStatusParameter = tacticStatus != null ?
                new ObjectParameter("TacticStatus", tacticStatus) :
                new ObjectParameter("TacticStatus", typeof(string));
    
            var createdDateParameter = createdDate.HasValue ?
                new ObjectParameter("CreatedDate", createdDate) :
                new ObjectParameter("CreatedDate", typeof(System.DateTime));
    
            var createdByParameter = createdBy.HasValue ?
                new ObjectParameter("CreatedBy", createdBy) :
                new ObjectParameter("CreatedBy", typeof(System.Guid));
    
            var suffixParameter = suffix != null ?
                new ObjectParameter("Suffix", suffix) :
                new ObjectParameter("Suffix", typeof(string));
    
            var copyCloneParameter = copyClone != null ?
                new ObjectParameter("CopyClone", copyClone) :
                new ObjectParameter("CopyClone", typeof(string));
    
            var idParameter = id.HasValue ?
                new ObjectParameter("Id", id) :
                new ObjectParameter("Id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PlanDuplicate", planIdParameter, planStatusParameter, tacticStatusParameter, createdDateParameter, createdByParameter, suffixParameter, copyCloneParameter, idParameter, returnValue);
        }
    
        public virtual int SaveModelInboundOutboundEvent(Nullable<int> oldModelId, Nullable<int> newModelId, Nullable<System.DateTime> createdDate, Nullable<System.Guid> createdBy, ObjectParameter returnValue)
        {
            var oldModelIdParameter = oldModelId.HasValue ?
                new ObjectParameter("OldModelId", oldModelId) :
                new ObjectParameter("OldModelId", typeof(int));
    
            var newModelIdParameter = newModelId.HasValue ?
                new ObjectParameter("NewModelId", newModelId) :
                new ObjectParameter("NewModelId", typeof(int));
    
            var createdDateParameter = createdDate.HasValue ?
                new ObjectParameter("CreatedDate", createdDate) :
                new ObjectParameter("CreatedDate", typeof(System.DateTime));
    
            var createdByParameter = createdBy.HasValue ?
                new ObjectParameter("CreatedBy", createdBy) :
                new ObjectParameter("CreatedBy", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SaveModelInboundOutboundEvent", oldModelIdParameter, newModelIdParameter, createdDateParameter, createdByParameter, returnValue);
        }
    
        public virtual int Plan_Task_Delete(Nullable<int> planCampaignId, Nullable<int> planProgramId, Nullable<int> planTacticId, Nullable<bool> isDelete, Nullable<System.DateTime> modifiedDate, Nullable<System.Guid> modifiedBy, ObjectParameter returnValue, Nullable<int> planLineItemId)
        {
            var planCampaignIdParameter = planCampaignId.HasValue ?
                new ObjectParameter("PlanCampaignId", planCampaignId) :
                new ObjectParameter("PlanCampaignId", typeof(int));
    
            var planProgramIdParameter = planProgramId.HasValue ?
                new ObjectParameter("PlanProgramId", planProgramId) :
                new ObjectParameter("PlanProgramId", typeof(int));
    
            var planTacticIdParameter = planTacticId.HasValue ?
                new ObjectParameter("PlanTacticId", planTacticId) :
                new ObjectParameter("PlanTacticId", typeof(int));
    
            var isDeleteParameter = isDelete.HasValue ?
                new ObjectParameter("IsDelete", isDelete) :
                new ObjectParameter("IsDelete", typeof(bool));
    
            var modifiedDateParameter = modifiedDate.HasValue ?
                new ObjectParameter("ModifiedDate", modifiedDate) :
                new ObjectParameter("ModifiedDate", typeof(System.DateTime));
    
            var modifiedByParameter = modifiedBy.HasValue ?
                new ObjectParameter("ModifiedBy", modifiedBy) :
                new ObjectParameter("ModifiedBy", typeof(System.Guid));
    
            var planLineItemIdParameter = planLineItemId.HasValue ?
                new ObjectParameter("PlanLineItemId", planLineItemId) :
                new ObjectParameter("PlanLineItemId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("Plan_Task_Delete", planCampaignIdParameter, planProgramIdParameter, planTacticIdParameter, isDeleteParameter, modifiedDateParameter, modifiedByParameter, returnValue, planLineItemIdParameter);
        }
    }
}
