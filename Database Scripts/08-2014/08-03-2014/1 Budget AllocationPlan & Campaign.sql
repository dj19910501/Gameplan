-- Run this script on Gameplan ===

-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 07/14/2014
-- Description :- Address PL ticket #556 - Advance budgeting.
-- NOTE :- Run this script on 'MRP' DB.
-- =======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan')
BEGIN
	
	IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'GoalType')
	BEGIN
		ALTER TABLE [Plan] ADD GoalType VARCHAR(50) NULL
	END

	IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'GoalValue')
	BEGIN
		ALTER TABLE [Plan] ADD GoalValue FLOAT NULL
	END

	IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'AllocatedBy')
	BEGIN
		ALTER TABLE [Plan] ADD AllocatedBy VARCHAR(50) NULL
	END
	
END

GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan')
BEGIN
	
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'GoalType') 
	AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'MQLs')
	BEGIN
		
		UPDATE [Plan] SET [GoalType] = 'MQL' WHERE ISNULL(IsDeleted, 0) = 0
		
		UPDATE [Plan] SET [GoalType] = '' WHERE [GoalType] IS NULL
		
		ALTER TABLE [Plan] ALTER COLUMN GoalType VARCHAR(50) NOT NULL
		
		EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Goal type of Plan. Goal type can be INQ, MQL, Revenue.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan', @level2type=N'COLUMN',@level2name=N'GoalType'

	END

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'AllocatedBy')
	AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'MQLs')
	BEGIN
		
		UPDATE [Plan] SET [AllocatedBy] = 'default' WHERE ISNULL(IsDeleted, 0) = 0
		
		UPDATE [Plan] SET [AllocatedBy] = 'default' WHERE [AllocatedBy] IS NULL
		
		ALTER TABLE [Plan] ALTER COLUMN AllocatedBy VARCHAR(50) NOT NULL
		
		EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Budget allocation method for Plan. Allocated by can be default, months, quarters.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan', @level2type=N'COLUMN',@level2name=N'AllocatedBy'

	END

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'GoalValue') 
	AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan' AND COLUMN_NAME = 'MQLs')
	BEGIN
		
		UPDATE [Plan] SET [GoalValue] = MQLs WHERE ISNULL(IsDeleted, 0) = 0
		
		UPDATE [Plan] SET [GoalValue] = 0 WHERE [GoalValue] IS NULL

		ALTER TABLE [Plan] ALTER COLUMN GoalValue FLOAT NOT NULL
		
		EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Value for goal type of Plan.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan', @level2type=N'COLUMN',@level2name=N'GoalValue'

		ALTER TABLE [Plan] DROP COLUMN MQLs 
	END

END

GO


-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 07/14/2014
-- Description :- Address PL ticket #556 - Advance budgeting.
-- NOTE :- Run this script on 'MRP' DB.
-- =======================================================================================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Budget')
BEGIN

CREATE TABLE [dbo].[Plan_Budget]
	(
		[PlanBudgetId] [int] IDENTITY(1,1) NOT NULL,
		[PlanId] [int] NOT NULL,
		[Period] [nvarchar](5) NOT NULL,
		[Value] [float] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		CONSTRAINT [PK_Plan_Budget] PRIMARY KEY CLUSTERED 
		(
			[PlanBudgetId] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
		CONSTRAINT [IX_Plan_Budget_PlanId_Period] UNIQUE NONCLUSTERED 
		(
			[PlanId] ASC,
			[Period] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

ALTER TABLE [dbo].[Plan_Budget]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Budget_Plan] FOREIGN KEY([PlanId])
REFERENCES [dbo].[Plan] ([PlanId])

ALTER TABLE [dbo].[Plan_Budget] CHECK CONSTRAINT [FK_Plan_Budget_Plan]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify budget allocation for a particular period for a plan.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Budget', @level2type=N'COLUMN',@level2name=N'PlanBudgetId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated Plan.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Budget', @level2type=N'COLUMN',@level2name=N'PlanId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of budget allocation. Period can be Y1, Y2, Y3 up to Y12.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Budget', @level2type=N'COLUMN',@level2name=N'Period'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated budget for the period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Budget', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Budget', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Budget', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END
GO
-- Add Column in Plan_Campaign --
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'CampaignBudget' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
       ALTER TABLE Plan_Campaign ADD CampaignBudget float NULL
  END
GO
IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'CampaignBudget' AND [object_id] = OBJECT_ID(N'Plan_Campaign') 
AND (SELECT COUNT(*) FROM Plan_Campaign where CampaignBudget != 0) = 0)
BEGIN    
       UPDATE Plan_Campaign SET CampaignBudget = 0

       ALTER TABLE Plan_Campaign ALTER COLUMN CampaignBudget float NOT NULL

       EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated budget for the Plan Campaign.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign', @level2type=N'COLUMN',@level2name=N'CampaignBudget'

END


-- Add Table --

GO

/****** Object:  Table [dbo].[Plan_Campaign_Budget]    Script Date: 7/24/2014 5:46:30 PM ******/

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Budget')
BEGIN
CREATE TABLE [dbo].[Plan_Campaign_Budget](
	[PlanCampaignBudgetId] [int] IDENTITY(1,1) NOT NULL,
	[PlanCampaignId] [int] NOT NULL,
	[Period] [nvarchar](5) NOT NULL,
	[Value] [float] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Plan_Campaign_Budget] PRIMARY KEY CLUSTERED 
(
	[PlanCampaignBudgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Plan_Campaign_Budget] UNIQUE NONCLUSTERED 
(
	[PlanCampaignId] ASC,
	[Period] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Plan_Campaign_Budget]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Budget_Plan_Campaign] FOREIGN KEY([PlanCampaignId])
REFERENCES [dbo].[Plan_Campaign] ([PlanCampaignId])

ALTER TABLE [dbo].[Plan_Campaign_Budget] CHECK CONSTRAINT [FK_Plan_Campaign_Budget_Plan_Campaign]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify budget allocation for a particular period for a Plan Campaign.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Budget', @level2type=N'COLUMN',@level2name=N'PlanCampaignBudgetId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated PlanCampaign.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Budget', @level2type=N'COLUMN',@level2name=N'PlanCampaignId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of budget allocation. Period can be Y1, Y2, Y3 up to Y12.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Budget', @level2type=N'COLUMN',@level2name=N'Period'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated budget for the period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Budget', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Budget', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Budget', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END
