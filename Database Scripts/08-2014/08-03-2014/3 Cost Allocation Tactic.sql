

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

