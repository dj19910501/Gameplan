 
 -- Created By : Brad Gray
-- Created Date : 09/04/2015
-- Description : Database changes for PL#1368
-- ======================================================================================


-- Remove GameplanDataType 'Owner' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete FROM IntegrationInstanceDataTypeMapping where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Owner')

	delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Owner')
 end
 go

 -- Remove GameplanDataType 'Start Date' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete FROM IntegrationInstanceDataTypeMapping where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Start Date')

	delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Start Date')
 end
 go

 -- Remove GameplanDataType 'End Date' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
  delete FROM IntegrationInstanceDataTypeMapping where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'End Date')

    delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'End Date')
 end
 go

 -- Viral Kadiya
 -- Three Way Integration
 -- 1.) Insert_IntegrationInstanceEloquaID_Column_Script_09_08_2015
 Go
 IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Model]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Model]
	ADD IntegrationInstanceEloquaId int NULL
END

IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic]
	ADD IntegrationInstanceEloquaId nvarchar(50) NULL
END

IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Improvement_Campaign_Program_Tactic]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]
	ADD IntegrationInstanceEloquaId nvarchar(50) NULL
END

GO

-- 2.) Udate_Data_Script_Model_Table_EloquaInstance_Move_09-08-2015
Declare @eloquaInstanceType varchar(10) ='Eloqua'
UPDATE Model set IntegrationInstanceId=null,IntegrationInstanceEloquaId = inst.IntegrationInstanceId FROM Model as mdl
INNER JOIN IntegrationInstance as inst on mdl.IntegrationInstanceId = inst.IntegrationInstanceId and inst.IntegrationTypeId=(SELECT IntegrationTypeId FROM IntegrationType WHERE Code=@eloquaInstanceType)
	
-- 3.) Update_Data_Script_Plan_Campaign_Program_Tactic_Table_Move_EloquaID_09-08-2015
/* ------------- Start: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Campaign_Program_Tactic  ------------- */
GO
UPDATE src set IntegrationInstanceTacticId=NULL,IntegrationInstanceEloquaId = elq.IntegrationInstanceTacticId FROM Plan_Campaign_Program_Tactic as src
INNER JOIN Plan_Campaign_Program_Tactic as elq on src.PlanTacticId = elq.PlanTacticId AND ISNUMERIC(elq.IntegrationInstanceTacticId)=1 AND (elq.IntegrationInstanceTacticId  IS NOT NULL)

/* ------------- End: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Campaign_Program_Tactic  ------------- */

/* ------------- Start: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Improvement_Campaign_Program_Tactic  ------------- */
GO
UPDATE src set IntegrationInstanceTacticId=NULL,IntegrationInstanceEloquaId = elq.IntegrationInstanceTacticId FROM Plan_Improvement_Campaign_Program_Tactic as src
INNER JOIN Plan_Improvement_Campaign_Program_Tactic as elq on src.ImprovementPlanTacticId = elq.ImprovementPlanTacticId AND ISNUMERIC(elq.IntegrationInstanceTacticId)=1 AND (elq.IntegrationInstanceTacticId  IS NOT NULL)

/* ------------- End: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Improvement_Campaign_Program_Tactic  ------------- */
GO


-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 09/11/2015
-- Description : Add new table for custom field dependency feature
-- ======================================================================================
Go
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN
        CREATE TABLE [dbo].[CustomFieldDependency](
	[ParentCustomFieldId] [int] NULL,
	[ParentOptionId] [int] NOT NULL,
	[ChildCustomFieldId] [int] NOT NULL,
	[ChildOptionId] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL CONSTRAINT [DF_CustomFieldDependency_IsDeleted]  DEFAULT ((0))
) ON [PRIMARY]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N' Refers to associated CustomFieldId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ParentCustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated CustomFieldOptionId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ParentOptionId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N' Refers to associated CustomFieldId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ChildCustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'If customField will be dropdown then it Refers to associated CustomFieldOptionId else it will be NULL.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ChildOptionId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'IsDeleted'

END

GO
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_CustomFieldDependency_CustomField')
   AND parent_object_id = OBJECT_ID(N'dbo.CustomFieldDependency')
)
BEGIN

ALTER TABLE [dbo].[CustomFieldDependency]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldDependency_CustomField] FOREIGN KEY([ParentCustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

END

IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_CustomFieldDependency_CustomField1')
   AND parent_object_id = OBJECT_ID(N'dbo.CustomFieldDependency')
)
BEGIN

ALTER TABLE [dbo].[CustomFieldDependency]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldDependency_CustomField1] FOREIGN KEY([ChildCustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

END


IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_CustomFieldDependency_CustomFieldOption1')
   AND parent_object_id = OBJECT_ID(N'dbo.CustomFieldDependency')
)
BEGIN

ALTER TABLE [dbo].[CustomFieldDependency]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldDependency_CustomFieldOption1] FOREIGN KEY([ChildOptionId])
REFERENCES [dbo].[CustomFieldOption] ([CustomFieldOptionId])

END

Go
-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 24/09/2015
-- Description : Add column and unique key for custom field dependency table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'DependencyId' AND [object_id] = OBJECT_ID(N'CustomFieldDependency'))
			 BEGIN
		     ALTER TABLE [dbo].[CustomFieldDependency] ADD  DependencyId int NOT NULL IDENTITY (1, 1)
			 CONSTRAINT [PK_CustomFieldDependency] PRIMARY KEY CLUSTERED (DependencyId)
		    END
END

Go

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN
  IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'IX_CustomFieldDependency')
  BEGIN
ALTER TABLE [dbo].[CustomFieldDependency] ADD  CONSTRAINT [IX_CustomFieldDependency] UNIQUE NONCLUSTERED 
(
	[ChildCustomFieldId] ASC,
	[ChildOptionId] ASC,
	[ParentCustomFieldId] ASC,
	[ParentOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
END

Go


-- ======================================================================================
-- Created By : Nishant Sheth
-- Created Date : 01/10/2015
-- Description : Add Budget Tables
-- ======================================================================================

IF Not EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = 'Budget')
Begin                  
CREATE TABLE [dbo].[Budget](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Desc] [nvarchar](4000) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[IsDeleted] [bit] NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Budget] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Budget]  WITH CHECK ADD  CONSTRAINT [FK_Budget_Budget] FOREIGN KEY([Id])
REFERENCES [dbo].[Budget] ([Id])


ALTER TABLE [dbo].[Budget] CHECK CONSTRAINT [FK_Budget_Budget]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique identifier
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget', @level2type=N'COLUMN',@level2name=N'Id'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'client id to differenentiate different budgets
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget', @level2type=N'COLUMN',@level2name=N'ClientId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of the budget as set by the user
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget', @level2type=N'COLUMN',@level2name=N'Name'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description if required
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget', @level2type=N'COLUMN',@level2name=N'Desc'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Creation date for the budget
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'If the budget is deleted, a flag to identify that
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget', @level2type=N'COLUMN',@level2name=N'IsDeleted'

End

--To Check Budget_Detail Table Exist or not

IF Not EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = 'Budget_Detail')
Begin
   
CREATE TABLE [dbo].[Budget_Detail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BudgetId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ParentId] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Budget_Detail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Budget_Detail]  WITH CHECK ADD  CONSTRAINT [FK_Budget_Detail_Budget1] FOREIGN KEY([BudgetId])
REFERENCES [dbo].[Budget] ([Id])

ALTER TABLE [dbo].[Budget_Detail] CHECK CONSTRAINT [FK_Budget_Detail_Budget1]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Auto increase primary key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget_Detail', @level2type=N'COLUMN',@level2name=N'Id'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK Id of Budget table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget_Detail', @level2type=N'COLUMN',@level2name=N'BudgetId'
           
End

--To Check Budget_DetailAmount Table Exist or not

IF Not EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = 'Budget_DetailAmount')
Begin
   CREATE TABLE [dbo].[Budget_DetailAmount](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BudgetDetailId] [int] NOT NULL,
	[Period] [nvarchar](5) NOT NULL,
	[Budget] [float] NULL,
	[Forecast] [float] NULL,
 CONSTRAINT [PK_Budget_DetailAmount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Budget_DetailAmount]  WITH CHECK ADD  CONSTRAINT [FK_Budget_DetailAmount_Budget_Detail] FOREIGN KEY([BudgetDetailId])
REFERENCES [dbo].[Budget_Detail] ([Id])

ALTER TABLE [dbo].[Budget_DetailAmount] CHECK CONSTRAINT [FK_Budget_DetailAmount_Budget_Detail]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Auto increase primary key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Budget_DetailAmount', @level2type=N'COLUMN',@level2name=N'Id'
              
End

--To Check LineItem_Budget Table Exist or not

IF Not EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = 'LineItem_Budget')
Begin
  CREATE TABLE [dbo].[LineItem_Budget](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BudgetDetailId] [int] NOT NULL,
	[PlanLineItemId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_LineItem_Budget] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[LineItem_Budget]  WITH CHECK ADD  CONSTRAINT [FK_LineItem_Budget_Budget_Detail] FOREIGN KEY([BudgetDetailId])
REFERENCES [dbo].[Budget_Detail] ([Id])

ALTER TABLE [dbo].[LineItem_Budget] CHECK CONSTRAINT [FK_LineItem_Budget_Budget_Detail]

ALTER TABLE [dbo].[LineItem_Budget]  WITH CHECK ADD  CONSTRAINT [FK_LineItem_Budget_Plan_Campaign_Program_Tactic_LineItem1] FOREIGN KEY([PlanLineItemId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])

ALTER TABLE [dbo].[LineItem_Budget] CHECK CONSTRAINT [FK_LineItem_Budget_Plan_Campaign_Program_Tactic_LineItem1]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Auto increase primary key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItem_Budget', @level2type=N'COLUMN',@level2name=N'Id'
                
End



-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 01/10/2015
-- Description : Add column in LineItem_Budget table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='LineItem_Budget')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Weightage' AND [object_id] = OBJECT_ID(N'LineItem_Budget'))
			 BEGIN
		     ALTER TABLE [dbo].[LineItem_Budget] ADD  Weightage tinyint  NULL 
		    END
END

Go

-- ======================================================================================
-- Created By : Dashrath Prajapati
-- Created Date : 07/10/2015
-- Description : Change Gameplan text to Plan related to PL #1645
-- ======================================================================================
UPDATE [dbo].[Notification] SET [EmailContent] = REPLACE([EmailContent], N'GamePlan', 'Plan') 
Go