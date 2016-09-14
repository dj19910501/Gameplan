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
    		((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<BestInClass> BestInClasses { get; set; }
        public DbSet<CampaignNameConvention> CampaignNameConventions { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Client_Activity> Client_Activity { get; set; }
        public DbSet<Client_Integration_Permission> Client_Integration_Permission { get; set; }
        public DbSet<ClientTacticType> ClientTacticTypes { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<CustomFieldOption> CustomFieldOptions { get; set; }
        public DbSet<CustomFieldType> CustomFieldTypes { get; set; }
        public DbSet<CustomRestriction> CustomRestrictions { get; set; }
        public DbSet<GameplanDataType> GameplanDataTypes { get; set; }
        public DbSet<GameplanDataTypePull> GameplanDataTypePulls { get; set; }
        public DbSet<ImprovementTacticType> ImprovementTacticTypes { get; set; }
        public DbSet<ImprovementTacticType_Metric> ImprovementTacticType_Metric { get; set; }
        public DbSet<IntegrationInstance> IntegrationInstances { get; set; }
        public DbSet<IntegrationInstance_Attribute> IntegrationInstance_Attribute { get; set; }
        public DbSet<IntegrationInstanceDataTypeMapping> IntegrationInstanceDataTypeMappings { get; set; }
        public DbSet<IntegrationInstanceDataTypeMappingPull> IntegrationInstanceDataTypeMappingPulls { get; set; }
        public DbSet<IntegrationInstanceExternalServer> IntegrationInstanceExternalServers { get; set; }
        public DbSet<IntegrationInstanceLog> IntegrationInstanceLogs { get; set; }
        public DbSet<IntegrationInstancePlanEntityLog> IntegrationInstancePlanEntityLogs { get; set; }
        public DbSet<IntegrationInstanceSection> IntegrationInstanceSections { get; set; }
        public DbSet<IntegrationType> IntegrationTypes { get; set; }
        public DbSet<IntegrationTypeAttribute> IntegrationTypeAttributes { get; set; }
        public DbSet<LineItemType> LineItemTypes { get; set; }
        public DbSet<MasterTacticType> MasterTacticTypes { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Model_Stage> Model_Stage { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Plan_Budget> Plan_Budget { get; set; }
        public DbSet<Plan_Campaign> Plan_Campaign { get; set; }
        public DbSet<Plan_Campaign_Budget> Plan_Campaign_Budget { get; set; }
        public DbSet<Plan_Campaign_Program> Plan_Campaign_Program { get; set; }
        public DbSet<Plan_Campaign_Program_Budget> Plan_Campaign_Program_Budget { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Actual> Plan_Campaign_Program_Tactic_Actual { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Budget> Plan_Campaign_Program_Tactic_Budget { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Comment> Plan_Campaign_Program_Tactic_Comment { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Cost> Plan_Campaign_Program_Tactic_Cost { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem_Actual> Plan_Campaign_Program_Tactic_LineItem_Actual { get; set; }
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
        public DbSet<User_Filter> User_Filter { get; set; }
        public DbSet<User_Notification> User_Notification { get; set; }
        public DbSet<EntityTypeColor> EntityTypeColors { get; set; }
        public DbSet<ELMAH_Error> ELMAH_Error { get; set; }
        public DbSet<IntegrationInstance_UnprocessData> IntegrationInstance_UnprocessData { get; set; }
        public DbSet<IntegrationWorkFrontTemplate> IntegrationWorkFrontTemplates { get; set; }
        public DbSet<CustomField_Entity> CustomField_Entity { get; set; }
        public DbSet<IntegrationWorkFrontPortfolio_Mapping> IntegrationWorkFrontPortfolio_Mapping { get; set; }
        public DbSet<IntegrationWorkFrontPortfolio> IntegrationWorkFrontPortfolios { get; set; }
        public DbSet<IntegrationInstanceLogDetail> IntegrationInstanceLogDetails { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Budget_DetailAmount> Budget_DetailAmount { get; set; }
        public DbSet<CustomFieldDependency> CustomFieldDependencies { get; set; }
        public DbSet<LineItem_Budget> LineItem_Budget { get; set; }
        public DbSet<Budget_Detail> Budget_Detail { get; set; }
        public DbSet<Budget_Columns> Budget_Columns { get; set; }
        public DbSet<Budget_ColumnSet> Budget_ColumnSet { get; set; }
        public DbSet<Budget_Permission> Budget_Permission { get; set; }
        public DbSet<Plan_UserSavedViews> Plan_UserSavedViews { get; set; }
        public DbSet<IntegrationWorkFrontRequestQueue> IntegrationWorkFrontRequestQueues { get; set; }
        public DbSet<IntegrationWorkFrontUser> IntegrationWorkFrontUsers { get; set; }
        public DbSet<IntegrationWorkFrontTacticSetting> IntegrationWorkFrontTacticSettings { get; set; }
        public DbSet<IntegrationWorkFrontRequest> IntegrationWorkFrontRequests { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic> Plan_Campaign_Program_Tactic { get; set; }
        public DbSet<MarketoEntityValueMapping> MarketoEntityValueMappings { get; set; }
        public DbSet<EntityIntegration_Attribute> EntityIntegration_Attribute { get; set; }
        public DbSet<IntegrationWorkFrontProgram_Mapping> IntegrationWorkFrontProgram_Mapping { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<DimensionValue> DimensionValues { get; set; }
        public DbSet<AggregationStatu> AggregationStatus { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem> Plan_Campaign_Program_Tactic_LineItem { get; set; }
        public DbSet<TacticType> TacticTypes { get; set; }
        public DbSet<ROI_PackageDetail> ROI_PackageDetail { get; set; }
        public DbSet<MediaCodes_CustomField_Configuration> MediaCodes_CustomField_Configuration { get; set; }
        public DbSet<Tactic_MediaCodes_CustomFieldMapping> Tactic_MediaCodes_CustomFieldMapping { get; set; }
        public DbSet<User_Permission> User_Permission { get; set; }
        public DbSet<Tactic_MediaCodes> Tactic_MediaCodes { get; set; }
        public DbSet<vClientWise_Tactic> vClientWise_Tactic { get; set; }
        public DbSet<Alert_Rules> Alert_Rules { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<User_Notification_Messages> User_Notification_Messages { get; set; }
        public DbSet<vClientWise_EntityList> vClientWise_EntityList { get; set; }
        public DbSet<User_CoulmnView> User_CoulmnView { get; set; }
    
        public virtual ObjectResult<string> ELMAH_GetErrorsXml(string application, Nullable<int> pageIndex, Nullable<int> pageSize, ObjectParameter totalCount)
        {
            var applicationParameter = application != null ?
                new ObjectParameter("Application", application) :
                new ObjectParameter("Application", typeof(string));
    
            var pageIndexParameter = pageIndex.HasValue ?
                new ObjectParameter("PageIndex", pageIndex) :
                new ObjectParameter("PageIndex", typeof(int));
    
            var pageSizeParameter = pageSize.HasValue ?
                new ObjectParameter("PageSize", pageSize) :
                new ObjectParameter("PageSize", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("ELMAH_GetErrorsXml", applicationParameter, pageIndexParameter, pageSizeParameter, totalCount);
        }
    
        public virtual ObjectResult<string> ELMAH_GetErrorXml(string application, Nullable<System.Guid> errorId)
        {
            var applicationParameter = application != null ?
                new ObjectParameter("Application", application) :
                new ObjectParameter("Application", typeof(string));
    
            var errorIdParameter = errorId.HasValue ?
                new ObjectParameter("ErrorId", errorId) :
                new ObjectParameter("ErrorId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("ELMAH_GetErrorXml", applicationParameter, errorIdParameter);
        }
    
        public virtual int ELMAH_LogError(Nullable<System.Guid> errorId, string application, string host, string type, string source, string message, string user, string allXml, Nullable<int> statusCode, Nullable<System.DateTime> timeUtc, string client)
        {
            var errorIdParameter = errorId.HasValue ?
                new ObjectParameter("ErrorId", errorId) :
                new ObjectParameter("ErrorId", typeof(System.Guid));
    
            var applicationParameter = application != null ?
                new ObjectParameter("Application", application) :
                new ObjectParameter("Application", typeof(string));
    
            var hostParameter = host != null ?
                new ObjectParameter("Host", host) :
                new ObjectParameter("Host", typeof(string));
    
            var typeParameter = type != null ?
                new ObjectParameter("Type", type) :
                new ObjectParameter("Type", typeof(string));
    
            var sourceParameter = source != null ?
                new ObjectParameter("Source", source) :
                new ObjectParameter("Source", typeof(string));
    
            var messageParameter = message != null ?
                new ObjectParameter("Message", message) :
                new ObjectParameter("Message", typeof(string));
    
            var userParameter = user != null ?
                new ObjectParameter("User", user) :
                new ObjectParameter("User", typeof(string));
    
            var allXmlParameter = allXml != null ?
                new ObjectParameter("AllXml", allXml) :
                new ObjectParameter("AllXml", typeof(string));
    
            var statusCodeParameter = statusCode.HasValue ?
                new ObjectParameter("StatusCode", statusCode) :
                new ObjectParameter("StatusCode", typeof(int));
    
            var timeUtcParameter = timeUtc.HasValue ?
                new ObjectParameter("TimeUtc", timeUtc) :
                new ObjectParameter("TimeUtc", typeof(System.DateTime));
    
            var clientParameter = client != null ?
                new ObjectParameter("Client", client) :
                new ObjectParameter("Client", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ELMAH_LogError", errorIdParameter, applicationParameter, hostParameter, typeParameter, sourceParameter, messageParameter, userParameter, allXmlParameter, statusCodeParameter, timeUtcParameter, clientParameter);
        }
    
        public virtual int DeleteBudget(Nullable<int> budgetDetailId, string clientId)
        {
            var budgetDetailIdParameter = budgetDetailId.HasValue ?
                new ObjectParameter("BudgetDetailId", budgetDetailId) :
                new ObjectParameter("BudgetDetailId", typeof(int));
    
            var clientIdParameter = clientId != null ?
                new ObjectParameter("ClientId", clientId) :
                new ObjectParameter("ClientId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("DeleteBudget", budgetDetailIdParameter, clientIdParameter);
        }
    
        public virtual ObjectResult<GetCollaboratorId_Result> GetCollaboratorId(Nullable<int> planId)
        {
            var planIdParameter = planId.HasValue ?
                new ObjectParameter("PlanId", planId) :
                new ObjectParameter("PlanId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCollaboratorId_Result>("GetCollaboratorId", planIdParameter);
        }
    
        public virtual int DeleteLastViewedData(string userId, string previousIds)
        {
            var userIdParameter = userId != null ?
                new ObjectParameter("UserId", userId) :
                new ObjectParameter("UserId", typeof(string));
    
            var previousIdsParameter = previousIds != null ?
                new ObjectParameter("PreviousIds", previousIds) :
                new ObjectParameter("PreviousIds", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("DeleteLastViewedData", userIdParameter, previousIdsParameter);
        }
    
        public virtual int PublishModel(Nullable<int> newModelId, Nullable<System.Guid> userId)
        {
            var newModelIdParameter = newModelId.HasValue ?
                new ObjectParameter("NewModelId", newModelId) :
                new ObjectParameter("NewModelId", typeof(int));
    
            var userIdParameter = userId.HasValue ?
                new ObjectParameter("UserId", userId) :
                new ObjectParameter("UserId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PublishModel", newModelIdParameter, userIdParameter);
        }
    
        public virtual int SaveuserBudgetPermission(Nullable<int> budgetDetailId, Nullable<int> permissionCode, Nullable<System.Guid> createdBy)
        {
            var budgetDetailIdParameter = budgetDetailId.HasValue ?
                new ObjectParameter("BudgetDetailId", budgetDetailId) :
                new ObjectParameter("BudgetDetailId", typeof(int));
    
            var permissionCodeParameter = permissionCode.HasValue ?
                new ObjectParameter("PermissionCode", permissionCode) :
                new ObjectParameter("PermissionCode", typeof(int));
    
            var createdByParameter = createdBy.HasValue ?
                new ObjectParameter("CreatedBy", createdBy) :
                new ObjectParameter("CreatedBy", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SaveuserBudgetPermission", budgetDetailIdParameter, permissionCodeParameter, createdByParameter);
        }
    }
}
