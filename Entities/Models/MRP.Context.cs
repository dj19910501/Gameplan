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
    
        public DbSet<BestInClass> BestInClasses { get; set; }
        public DbSet<CampaignNameConvention> CampaignNameConventions { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Client_Activity> Client_Activity { get; set; }
        public DbSet<Client_Integration_Permission> Client_Integration_Permission { get; set; }
        public DbSet<ClientTacticType> ClientTacticTypes { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<CustomField_Entity> CustomField_Entity { get; set; }
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Plan_Budget> Plan_Budget { get; set; }
        public DbSet<Plan_Campaign_Budget> Plan_Campaign_Budget { get; set; }
        public DbSet<Plan_Campaign_Program_Budget> Plan_Campaign_Program_Budget { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Actual> Plan_Campaign_Program_Tactic_Actual { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Comment> Plan_Campaign_Program_Tactic_Comment { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_Cost> Plan_Campaign_Program_Tactic_Cost { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem> Plan_Campaign_Program_Tactic_LineItem { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem_Actual> Plan_Campaign_Program_Tactic_LineItem_Actual { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic_LineItem_Cost> Plan_Campaign_Program_Tactic_LineItem_Cost { get; set; }
        public DbSet<Plan_Improvement_Campaign> Plan_Improvement_Campaign { get; set; }
        public DbSet<Plan_Improvement_Campaign_Program> Plan_Improvement_Campaign_Program { get; set; }
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
        public DbSet<Plan_Campaign> Plan_Campaign { get; set; }
        public DbSet<Plan_Campaign_Program> Plan_Campaign_Program { get; set; }
        public DbSet<Plan_Campaign_Program_Tactic> Plan_Campaign_Program_Tactic { get; set; }
        public DbSet<Plan_Improvement_Campaign_Program_Tactic> Plan_Improvement_Campaign_Program_Tactic { get; set; }
        public DbSet<Model_Stage> Model_Stage { get; set; }
    }
}
