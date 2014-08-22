--------------1_Internal_Review_Point.sql
-- Created by : Kalpesh Sharma 
-- Ticket : Internal Reviews Point  
-- This script will be check that ClientID field is exists or not in Role Table . 
-- if it will be not in that table at that time we have to insert that field with mirgation of Existing data.

--Check that ClientId is exists in Role Table or not.
IF NOT EXISTS (
		SELECT *
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Role'
			AND COLUMN_NAME = 'ClientId'
		)
	begin
	ALTER TABLE dbo.ROLE ADD ClientId UNIQUEIDENTIFIER CONSTRAINT ClientId_fk REFERENCES dbo.Client (ClientId)
	end
go 

Declare @Application_ID UNIQUEIDENTIFIER
Declare @Client_ID UNIQUEIDENTIFIER
Set @Application_ID = (Select ApplicationId from Application where Code ='MRP')
Set @Client_ID = (Select ClientId from Client where Code = 'BLD')


IF ((SELECT count(*) FROM [ROLE] WHERE ClientId is not null) = 0)
	begin
-- Check that Role_Transform table is exists or not.
-- If table is exists at that time we have apply Cross join on Role and Client Table


IF OBJECT_ID(N'TempDB.dbo.#Role_Temp', N'U') IS NOT NULL
BEGIN
  DROP TABLE #Role_Temp
END

Create table #Role_Temp
(
  RoleId uniqueidentifier,
  Code nvarchar(25),
  Title nvarchar(255),
  Description nvarchar(4000),
 IsDeleted bit,
 CreatedDate datetime,
 CreatedBy uniqueidentifier,
 ModifiedDate DateTime,
 ModifiedBy	uniqueidentifier,
 ColorCode nchar(10),
 ClientId uniqueidentifier
)

insert into #Role_Temp
SELECT r.RoleId
		,r.Code
		,r.Title
		,r.Description
		,r.IsDeleted
		,r.CreatedDate
		,r.CreatedBy
		,r.ModifiedDate
		,r.ModifiedBy
		,r.ColorCode
		,c.ClientId
	FROM Client c
	CROSS JOIN ROLE r
	WHERE c.ClientId <>  @Client_ID

-- Update the exisiting role with Default Client ID (BullDog Client )
UPDATE ROLE
SET ClientID = @Client_ID
WHERE ClientId IS NULL

	INSERT INTO ROLE
	SELECT NEWID() RoleId
		,Code
		,Title
		,Description
		,IsDeleted
		,CreatedDate
		,CreatedBy
		,ModifiedDate
		,ModifiedBy
		,ColorCode
		,ClientId
	FROM #Role_Temp

--Insert New RoleID into the Application Role Table
INSERT INTO Application_Role
SELECT AppRole.ApplicationId
	,SecondRole.RoleId
	,SecondRole.CreatedDate
	,SecondRole.CreatedBy
	,SecondRole.ModifiedDate
	,SecondRole.ModifiedBy
	,SecondRole.IsDeleted
FROM Application_Role AppRole
INNER JOIN ROLE firstRole ON firstRole.RoleId = AppRole.RoleId
INNER JOIN ROLE SecondRole ON SecondRole.Title = firstRole.Title
WHERE AppRole.ApplicationId = @Application_ID
	AND SecondRole.ClientId <> @Client_ID AND firstRole.RoleId <> SecondRole.RoleId

--Insert New RoleID into the Application Role Table
INSERT INTO Role_Permission
SELECT SecondRole.RoleId
	,ROLEPER.MenuApplicationId
	,ROLEPER.PermissionCode
	,SecondRole.CreatedDate
	,SecondRole.CreatedBy
	,SecondRole.ModifiedDate
	,SecondRole.ModifiedBy
	,SecondRole.IsDeleted
FROM Role_Permission ROLEPER
INNER JOIN ROLE firstRole ON firstRole.RoleId = ROLEPER.RoleId
INNER JOIN ROLE SecondRole ON SecondRole.Title = firstRole.Title
WHERE SecondRole.ClientId <>  @Client_ID AND firstRole.RoleId <> SecondRole.RoleId  --(Select ClientId from Client where Code = 'BLD')


--Insert New RoleID into the Application Role Table

update User_Application
Set RoleId = NewRoles.RoleId
from (
SELECT UA.UserId,R2.RoleId
FROM User_Application UA
INNER JOIN [User] U ON U.UserId = UA.UserId
INNER JOIN Role R1 ON R1.RoleId = UA.RoleId
INNER JOIN Role R2 ON R2.Title = R1.Title AND R2.ClientId = U.ClientId
WHERE R1.RoleId <> R2.RoleId) as NewRoles
where User_Application.UserId = NewRoles.UserId

end
	else
	begin
	print('Script is already executed.')
end
 GO



 --------------1 Budget AllocationPlan & Campaign.sql
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

--------------2 Budget Allocation Program.sql

-- Add Column in Plan_Program --
GO
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ProgramBudget' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
      ALTER TABLE Plan_Campaign_Program ADD ProgramBudget float NULL
END
GO
IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'ProgramBudget' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program') 
AND (SELECT COUNT(*) FROM Plan_Campaign_Program where ProgramBudget != 0) = 0)
BEGIN
       UPDATE Plan_Campaign_Program SET ProgramBudget = 0

       ALTER TABLE Plan_Campaign_Program ALTER COLUMN ProgramBudget float NOT NULL

       EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated budget for the Program.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program', @level2type=N'COLUMN',@level2name=N'ProgramBudget'

END

-- Add Table --

GO

/****** Object:  Table [dbo].[[Plan_Campaign_Program_Budget]]    Script Date: 7/24/2014 5:46:30 PM ******/

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Budget')
BEGIN

CREATE TABLE [dbo].[Plan_Campaign_Program_Budget](
	[PlanProgramBudgetId] [int] IDENTITY(1,1) NOT NULL,
	[PlanProgramId] [int] NOT NULL,
	[Period] [nvarchar](5) NOT NULL,
	[Value] [float] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Plan_Campaign_Program_Budget] PRIMARY KEY CLUSTERED 
(
	[PlanProgramBudgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Plan_Campaign_Program_Budget] UNIQUE NONCLUSTERED 
(
	[PlanProgramId] ASC,
	[Period] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Plan_Campaign_Program_Budget]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Budget_Plan_Campaign_Program_Budget] FOREIGN KEY([PlanProgramId])
REFERENCES [dbo].[Plan_Campaign_Program] ([PlanProgramId])

ALTER TABLE [dbo].[Plan_Campaign_Program_Budget] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Budget_Plan_Campaign_Program_Budget]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify budget allocation for a particular period for a Program.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Budget', @level2type=N'COLUMN',@level2name=N'PlanProgramBudgetId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated Program.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Budget', @level2type=N'COLUMN',@level2name=N'PlanProgramId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of budget allocation. Period can be Y1, Y2, Y3 up to Y12.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Budget', @level2type=N'COLUMN',@level2name=N'Period'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated budget for the period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Budget', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Budget', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Budget', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END

--------------3 Cost Allocation Tactic.sql
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_Cost')
BEGIN

CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_Cost](
	[PlanTacticBudgetId] [int] IDENTITY(1,1) NOT NULL,
	[PlanTacticId] [int] NOT NULL,
	[Period] [nvarchar](5) NOT NULL,
	[Value] [float] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Plan_Campaign_Program_Tactic_Budget] PRIMARY KEY CLUSTERED 
(
	[PlanTacticBudgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Plan_Campaign_Program_Tactic_Budget] UNIQUE NONCLUSTERED 
(
	[PlanTacticId] ASC,
	[Period] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Cost]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Budget_Plan_Campaign_Program_Tactic] FOREIGN KEY([PlanTacticId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Cost] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Budget_Plan_Campaign_Program_Tactic]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify cost allocation for a particular period for a Tactic.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Cost', @level2type=N'COLUMN',@level2name=N'PlanTacticBudgetId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated Tactic.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Cost', @level2type=N'COLUMN',@level2name=N'PlanTacticId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of cost allocation. Period can be Y1, Y2, Y3 up to Y12.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Cost', @level2type=N'COLUMN',@level2name=N'Period'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated cost for the period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Cost', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Cost', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Cost', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END


--------------4 Cost Allocation LineItem.sql
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='LineItemType')
BEGIN

CREATE TABLE [dbo].[LineItemType](
	[LineItemTypeId] [int] IDENTITY(1,1) NOT NULL,
	[ModelId] [int] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](4000) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_LineItemType] PRIMARY KEY CLUSTERED 
(
	[LineItemTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[LineItemType] ADD  CONSTRAINT [DF_LineItemType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

ALTER TABLE [dbo].[LineItemType]  WITH CHECK ADD  CONSTRAINT [FK_LineItemType_Model] FOREIGN KEY([ModelId])
REFERENCES [dbo].[Model] ([ModelId])

ALTER TABLE [dbo].[LineItemType] CHECK CONSTRAINT [FK_LineItemType_Model]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify LineItemType for a model.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'LineItemTypeId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated Model.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'ModelId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Title of line item type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'Title'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description of line item type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'Description'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'IsDeleted'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'ModifiedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LineItemType', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

END


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_LineItem')
BEGIN

CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem](
	[PlanLineItemId] [int] IDENTITY(1,1) NOT NULL,
	[PlanTacticId] [int] NOT NULL,
	[LineItemTypeId] [int] NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](4000) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Cost] [float] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Plan_Campaign_Program_Tactic_LineItem] PRIMARY KEY CLUSTERED 
(
	[PlanLineItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem] ADD  CONSTRAINT [DF_Plan_Campaign_Program_Tactic_LineItem_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_LineItemType] FOREIGN KEY([LineItemTypeId])
REFERENCES [dbo].[LineItemType] ([LineItemTypeId])

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_LineItemType]

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Plan_Campaign_Program_Tactic] FOREIGN KEY([PlanTacticId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Plan_Campaign_Program_Tactic]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify LineItem for a tactic.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'PlanLineItemId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated Tactic.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'PlanTacticId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated LineItem Type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'LineItemTypeId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Title of line item type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'Title'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description of line item type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'Description'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the line item starts.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'StartDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the line item ends.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'EndDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cost of Line item.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'Cost'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'IsDeleted'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified.
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'ModifiedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_LineItem_Cost')
BEGIN

CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Cost](
	[PlanLineItemBudgetId] [int] IDENTITY(1,1) NOT NULL,
	[PlanLineItemId] [int] NOT NULL,
	[Period] [nvarchar](5) NOT NULL,
	[Value] [float] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Plan_Campaign_Program_Tactic_LineItem_Budget_1] PRIMARY KEY CLUSTERED 
(
	[PlanLineItemBudgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Plan_Campaign_Program_Tactic_LineItem_Budget] UNIQUE NONCLUSTERED 
(
	[PlanLineItemId] ASC,
	[Period] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Cost]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Budget_Plan_Campaign_Program_Tactic_LineItem] FOREIGN KEY([PlanLineItemId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])

ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Cost] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Budget_Plan_Campaign_Program_Tactic_LineItem]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify cost allocation for a particular period for a LineItem.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Cost', @level2type=N'COLUMN',@level2name=N'PlanLineItemBudgetId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated LineItem.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Cost', @level2type=N'COLUMN',@level2name=N'PlanLineItemId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of cost allocation. Period can be Y1, Y2, Y3 up to Y12.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Cost', @level2type=N'COLUMN',@level2name=N'Period'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated cost for the period' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Cost', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Cost', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Cost', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END




--------------1_PL#686_Unable_Delete_Plan_Program.sql
/****** Object:  StoredProcedure [dbo].[Plan_Task_Delete]    Script Date: 8/4/2014 16:38:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--Stored Procedure
	ALTER PROCEDURE [dbo].[Plan_Task_Delete]
	(
		   @PlanCampaignId INT = NULL,
		   @PlanProgramId INT = NULL,
		   @PlanTacticId INT = NULL,
		   @IsDelete BIT,
		   @ModifiedDate DateTime,    
		   @ModifiedBy UNIQUEIDENTIFIER, 
		@ReturnValue INT = 0 OUT,
		@PlanLineItemId INT =NULL     
	)
	AS
	BEGIN

		   DECLARE @TranName VARCHAR(20);
		   SELECT @TranName = 'Plan_Task_Delete';
		   BEGIN TRANSACTION @TranName;
		   BEGIN TRY
						 IF(@PlanCampaignId IS NOT NULL)
						 BEGIN
						       UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanTacticId IN (SELECT PlanTacticId FROM dbo.Plan_Campaign_Program_Tactic WHERE PlanProgramId IN (SELECT PlanProgramId FROM Plan_Campaign_Program WHERE PlanCampaignId = @PlanCampaignId))

							   UPDATE Plan_Campaign_Program_Tactic SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanProgramId IN (SELECT PlanProgramId FROM Plan_Campaign_Program WHERE PlanCampaignId = @PlanCampaignId)
              
							   UPDATE Plan_Campaign_Program SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanCampaignId = @PlanCampaignId

							   UPDATE Plan_Campaign SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanCampaignId = @PlanCampaignId

						 END
						 ELSE IF(@PlanProgramId IS NOT NULL)
						 BEGIN
						       UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanTacticId IN (SELECT PlanTacticId FROM dbo.Plan_Campaign_Program_Tactic WHERE PlanProgramId= @PlanProgramId)

							   UPDATE Plan_Campaign_Program_Tactic SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanProgramId = @PlanProgramId
              
							   UPDATE Plan_Campaign_Program SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanProgramId = @PlanProgramId
						 END
						 ELSE IF(@PlanTacticId IS NOT NULL)
						 BEGIN
						       UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanTacticId=@PlanTacticId
							   UPDATE Plan_Campaign_Program_Tactic SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanTacticId = @PlanTacticId
						 END
						 ELSE IF(@PlanLineItemId IS NOT NULL)
						 BEGIN
						 UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanLineItemId=@PlanLineItemId
						 END

				  ---Successfully deleted
				  COMMIT TRANSACTION @TranName;
			   SET @ReturnValue = 1;        
			   RETURN @ReturnValue 
                
		   END TRY
		   BEGIN CATCH
				  ---Unsuccess
				  ROLLBACK TRANSACTION @TranName;
				  RETURN @ReturnValue  
		   END CATCH 
	END



--------------1_PL_#669_Integration - Create data model for integration changes.sql
/****** Object:  Table [dbo].[IntegrationInstanceExternalServer]    Script Date: 8/6/2014 6:24:29 PM ******/
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceExternalServer')
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IntegrationInstanceExternalServer](
	[IntegrationInstanceExternalServerId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[SFTPServerName] [nvarchar](255) NOT NULL,
	[SFTPFileLocation] [nvarchar](1000) NOT NULL,
	[SFTPUserName] [nvarchar](255) NOT NULL,
	[SFTPPassword] [nvarchar](255) NOT NULL,
	[SFTPPort] [nvarchar](4) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_IntegrationInstanceExternalServer] PRIMARY KEY CLUSTERED 
(
	[IntegrationInstanceExternalServerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[IntegrationInstanceExternalServer] ADD  CONSTRAINT [DF_IntegrationInstanceExternalServer_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

ALTER TABLE [dbo].[IntegrationInstanceExternalServer]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceExternalServer_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstanceExternalServer] CHECK CONSTRAINT [FK_IntegrationInstanceExternalServer_IntegrationInstance]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify external server for a integration instance.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceExternalServerId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated integration instance.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPServerName'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'File location of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPFileLocation'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'User name of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPUserName'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Password of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPPassword'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Port number of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPPort'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'IsDeleted'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'ModifiedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceIdINQ' AND OBJECT_ID = OBJECT_ID(N'Model'))
BEGIN
    ALTER TABLE [Model] ADD IntegrationInstanceIdINQ INT
	
	ALTER TABLE [dbo].[Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceINQ] FOREIGN KEY([IntegrationInstanceIdINQ])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	ALTER TABLE [dbo].[Model] CHECK CONSTRAINT [FK_Model_IntegrationInstanceINQ]
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceIdMQL' AND OBJECT_ID = OBJECT_ID(N'Model'))
BEGIN
    ALTER TABLE [Model] ADD IntegrationInstanceIdMQL INT
	ALTER TABLE [dbo].[Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceMQL] FOREIGN KEY([IntegrationInstanceIdMQL])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	ALTER TABLE [dbo].[Model] CHECK CONSTRAINT [FK_Model_IntegrationInstanceMQL]
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceIdCW' AND OBJECT_ID = OBJECT_ID(N'Model'))
BEGIN
    ALTER TABLE [Model] ADD IntegrationInstanceIdCW INT
	ALTER TABLE [dbo].[Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceCW] FOREIGN KEY([IntegrationInstanceIdCW])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	ALTER TABLE [dbo].[Model] CHECK CONSTRAINT [FK_Model_IntegrationInstanceCW]
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'ModifiedDate' AND OBJECT_ID = OBJECT_ID(N'Plan_Campaign_Program_Tactic_Actual'))
BEGIN
    ALTER TABLE [Plan_Campaign_Program_Tactic_Actual] ADD ModifiedDate datetime
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'ModifiedBy' AND OBJECT_ID = OBJECT_ID(N'Plan_Campaign_Program_Tactic_Actual'))
BEGIN
    ALTER TABLE [Plan_Campaign_Program_Tactic_Actual] ADD ModifiedBy uniqueidentifier
END

--------------01_PL_656_Integration_UI_Tactic_Detail_Screens_and_Fields_for_Eloqua.sql
-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 08/07/2014
-- Description :- Update DisplayFieldName by removing 'Gameplan.[Tablename]' from it
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType' AND COLUMN_NAME = 'DisplayFieldName')
	BEGIN
	
		IF (SELECT COUNT(1) FROM GameplanDataType WHERE PATINDEX('%.%', DisplayFieldName) > 0) > 0
		BEGIN
			
			PRINT('Done')

			UPDATE GameplanDataType SET DisplayFieldName = REPLACE(DisplayFieldName, 'Ganeplan.', 'Gameplan.')
			WHERE DisplayFieldName like '%Ganeplan.%'

			UPDATE GameplanDataType 
			SET DisplayFieldName =  REPLACE(REPLACE(DisplayFieldName, 'Gameplan.', ''), LEFT(REPLACE(DisplayFieldName, 'Gameplan.', ''),CHARINDEX('.',REPLACE(DisplayFieldName, 'Gameplan.', ''))), '')
		END

	END

END

--------------02_PL_656_Integration_UI_Tactic_Detail_Screens_and_Fields_for_Eloqua.sql
-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 11/08/2014
-- Description :- Update DisplayFieldName to separate the words by splitting the string with space. Examplae 'StartDate' will be 'Start Date'
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE SPECIFIC_NAME = 'InitCap' AND ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_SCHEMA = 'dbo')
BEGIN
	DROP FUNCTION [dbo].[InitCap] 
END
GO

CREATE FUNCTION [dbo].[InitCap] ( @InputString varchar(4000) ) 
RETURNS VARCHAR(4000)
AS
BEGIN

	DECLARE @Index          INT
	DECLARE @Char           CHAR(1)
	DECLARE @PrevChar       CHAR(1)
	DECLARE @OutputString   VARCHAR(255)

	SET @OutputString = @InputString
	SET @Index = 1

	WHILE @Index <= LEN(@InputString)
	BEGIN
		SET @Char     = SUBSTRING(@InputString, @Index, 1)
		SET @PrevChar = CASE WHEN @Index = 1 THEN ' '
							 ELSE SUBSTRING(@InputString, @Index - 1, 1)
						END

		IF LOWER(@PrevChar) = @PrevChar COLLATE Latin1_General_CS_AS AND UPPER(@Char) = @Char COLLATE Latin1_General_CS_AS
		BEGIN
			IF @PrevChar != ' ' AND @Char != ' ' AND @PrevChar != '(' AND @PrevChar != ')' AND @Char != '(' AND @Char != ')'
				SET @OutputString = STUFF(@OutputString, @Index, 1, UPPER( ' ' + @Char))
		END

		SET @Index = @Index + 1
	END

	RETURN @OutputString

END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType' AND COLUMN_NAME = 'DisplayFieldName')
	BEGIN

		UPDATE GameplanDataType SET DisplayFieldName = dbo.[InitCap](DisplayFieldName)   
		
		UPDATE GameplanDataType Set DisplayFieldName = 'Owner' WHERE DisplayFieldName COLLATE Latin1_General_CS_AS = 'owner'

	END

END

GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE SPECIFIC_NAME = 'InitCap' AND ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_SCHEMA = 'dbo')
BEGIN
	DROP FUNCTION [dbo].[InitCap] 
END
GO

SELECT DisplayFieldName from GameplanDataType


--------------01_PL_681_Integration_ UI_Tactic_Detail_Screens_and_Fields_for_Salesforce.sql
-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 12/08/2014
-- Description :- Delete record where IsStage = 1 from GameplanDataType table and also remove its reference records. Also remove IsStage field
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
   EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType' AND COLUMN_NAME = 'IsStage')
	BEGIN

		DELETE 
		FROM IntegrationInstanceDataTypeMapping 
		WHERE GameplanDataTypeId IN (
									SELECT GameplanDataTypeId 
									FROM GameplanDataType 
									WHERE IsStage = 1
									)

		DELETE
		FROM GameplanDataType 
		WHERE IsStage = 1							

		IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name = 'DF_GameplanDataType_IsStage' AND [type] = 'D' AND parent_object_id IN 
				(SELECT object_id FROM sys.objects WHERE name = 'GameplanDataType' AND [type] = 'U' AND [schema_id] = '1'))
		BEGIN
				ALTER TABLE GameplanDataType DROP CONSTRAINT DF_GameplanDataType_IsStage
		END

		ALTER TABLE GameplanDataType DROP COLUMN IsStage

	END

END


--------------02_PL_658_Integration_UI_Pulling_Revenue.sql

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataTypePull')
BEGIN

CREATE TABLE [dbo].[GameplanDataTypePull](
	[GameplanDataTypePullId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationTypeId] [int] NOT NULL,
	[ActualFieldName] [nvarchar](255) NOT NULL,
	[DisplayFieldName] [nvarchar](255) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_GameplanDataTypePull] PRIMARY KEY CLUSTERED 
(
	[GameplanDataTypePullId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[GameplanDataTypePull] ADD  CONSTRAINT [DF_GameplanDataTypePull_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

ALTER TABLE [dbo].[GameplanDataTypePull]  WITH CHECK ADD  CONSTRAINT [FK_GameplanDataTypePull_IntegrationType] FOREIGN KEY([IntegrationTypeId])
REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])

ALTER TABLE [dbo].[GameplanDataTypePull] CHECK CONSTRAINT [FK_GameplanDataTypePull_IntegrationType]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify Gameplan Data Type Pull.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'GameplanDataTypePullId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationTypeId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'IntegrationTypeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Actual Field Name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'ActualFieldName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Display Field Name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'DisplayFieldName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Pull Type. Pull Type can be INQ, MQL, CW.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'Type'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'IsDeleted'

END
GO


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMappingPull')
BEGIN

CREATE TABLE [dbo].[IntegrationInstanceDataTypeMappingPull](
	[IntegrationInstanceDataTypeMappingPullId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[GameplanDataTypePullId] [int] NOT NULL,
	[TargetDataType] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_IntegrationInstanceDataTypeMappingPull] PRIMARY KEY CLUSTERED 
(
	[IntegrationInstanceDataTypeMappingPullId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_GameplanDataTypePull] FOREIGN KEY([GameplanDataTypePullId])
REFERENCES [dbo].[GameplanDataTypePull] ([GameplanDataTypePullId])

ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull] CHECK CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_GameplanDataTypePull]

ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull] CHECK CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_IntegrationInstance]


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify IntegrationInstance DataType Mapping Pull.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceDataTypeMappingPullId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated GameplanDataTypePullId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'GameplanDataTypePullId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Target DataType.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'TargetDataType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'CreatedDate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END


IF NOT((SELECT COUNT(*) FROM GameplanDataTypePull where [Type]='CW') > 0)
BEGIN

	Declare @IntegrationTypeId int
	select TOP 1 @IntegrationTypeId  = IntegrationTypeId from IntegrationType where Title='Salesforce'

	IF (@IntegrationTypeId > 0 AND @IntegrationTypeId IS NOT NULL)
	BEGIN
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Stage', N'Stage', N'CW', 0)
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Timestamp', N'Timestamp', N'CW', 0)
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Campaign ID', N'Campaign ID', N'CW', 0)
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Revenue Amount', N'Revenue Amount', N'CW', 0)
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (2, N'Status', N'Status', N'INQ', 0)
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (2, N'Timestamp', N'Timestamp', N'INQ', 0)
		INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (2, N'CampaignID', N'CampaignID', N'INQ', 0)
	END
END

--------------01_Functional_Review_Point_Removing_SP.sql
-- =======================================================================================
-- Created By :- Mitesh Vaishnav
-- Created Date :- 08/19/2014
-- Description :- delete store procedure from database
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================
IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='Plan_Task_Delete')
BEGIN
DROP PROCEDURE dbo.Plan_Task_Delete
END

IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='PlanDuplicate')
BEGIN
DROP PROCEDURE dbo.PlanDuplicate
END

IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='SaveModelInboundOutboundEvent')
BEGIN
DROP PROCEDURE dbo.SaveModelInboundOutboundEvent
END

IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='Plan_Campaign_Program_Tactic_ActualDelete')
BEGIN
DROP PROCEDURE dbo.Plan_Campaign_Program_Tactic_ActualDelete
END

--------------01_PL_717_Pulling from Eloqua - Actual Cost.sql
Declare @EloquaIntegrationTypeId int
select TOP 1 @EloquaIntegrationTypeId  = IntegrationTypeId from IntegrationType where Title='Eloqua'
delete from GameplanDataType where integrationtypeid=@EloquaIntegrationTypeId and isget=1 and ActualFieldName='Revenue'

--------------01_PL_684_Integration_Error_handling_Logging.sql
-- db SCRIPT FOR #684 Integration - Error handling & Logging
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceSection')
BEGIN

CREATE TABLE [dbo].[IntegrationInstanceSection](
	[IntegrationInstanceSectionId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceLogId] [int] NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[SectionName] [nvarchar](255) NOT NULL,
	[SyncStart] [datetime] NOT NULL,
	[SyncEnd] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[Description] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreateBy] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_IntegrationInstanceSection] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceSectionId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

ALTER TABLE [dbo].[IntegrationInstanceSection]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstanceSection] CHECK CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstance]

ALTER TABLE [dbo].[IntegrationInstanceSection]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstanceLog] FOREIGN KEY([IntegrationInstanceLogId])
REFERENCES [dbo].[IntegrationInstanceLog] ([IntegrationInstanceLogId])

ALTER TABLE [dbo].[IntegrationInstanceSection] CHECK CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstanceLog]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify IntegrationInstance Section.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceSectionId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceLogId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceLogId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Possible values (PushTacticData,PullResponses,PullQualifiedLeads,PullClosedDeals)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'SectionName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync start time for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'SyncStart'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync end time for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'SyncEnd'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync status for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'Status'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync description for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'Description'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'CreatedDate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'CreateBy'

END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstancePlanEntityLog')
BEGIN

DROP TABLE IntegrationInstancePlanEntityLog

END
GO

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstancePlanEntityLog')
BEGIN
CREATE TABLE [dbo].[IntegrationInstancePlanEntityLog](
	[IntegrationInstancePlanLogEntityId] [bigint] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceSectionId] [int] NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[EntityId] [int] NOT NULL,
	[EntityType] [varchar](50) NULL,
	[SyncTimeStamp] [datetime] NOT NULL,
	[Operation] [varchar](50) NULL,
	[Status] [nvarchar](255) NULL,
	[ErrorDescription] [nvarchar](4000) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_IntegrationInstancePlanEntityLog] PRIMARY KEY CLUSTERED 
(
	[IntegrationInstancePlanLogEntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog] CHECK CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstance]

ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstanceSection] FOREIGN KEY([IntegrationInstanceSectionId])
REFERENCES [dbo].[IntegrationInstanceSection] ([IntegrationInstanceSectionId])

ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog] CHECK CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstanceSection]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceSectionId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstancePlanEntityLog', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceSectionId'

END
GO





