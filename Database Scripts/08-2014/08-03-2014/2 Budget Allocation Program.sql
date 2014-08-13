
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