-- Run script on MRP

GO
/****** Object:  Table [dbo].[Plan_Campaign_Program_Tactic_Budget]    Script Date: 03/19/2015 11:14:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Budget]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_Budget](
	[PlanTacticBudgetId] [int] IDENTITY(1,1) NOT NULL,
	[PlanTacticId] [int] NOT NULL,
	[Period] [nvarchar](5) NOT NULL,
	[Value] [float] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Plan_Campaign_Program_Tactic_Budget_1] PRIMARY KEY CLUSTERED 
(
	[PlanTacticBudgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Budget_Plan_Campaign_Program_Tactic_Budget]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Budget]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Budget]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Budget_Plan_Campaign_Program_Tactic_Budget] FOREIGN KEY([PlanTacticId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Budget_Plan_Campaign_Program_Tactic_Budget]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Budget]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Budget] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Budget_Plan_Campaign_Program_Tactic_Budget]
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'Plan_Campaign_Program_Tactic_Budget', N'COLUMN',N'PlanTacticBudgetId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify cost allocation for a particular period for a Tactic.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Budget', @level2type=N'COLUMN',@level2name=N'PlanTacticBudgetId'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'Plan_Campaign_Program_Tactic_Budget', N'COLUMN',N'PlanTacticId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated Tactic.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Budget', @level2type=N'COLUMN',@level2name=N'PlanTacticId'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'Plan_Campaign_Program_Tactic_Budget', N'COLUMN',N'Period'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of cost allocation. Period can be Y1, Y2, Y3 up to Y12.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Budget', @level2type=N'COLUMN',@level2name=N'Period'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'Plan_Campaign_Program_Tactic_Budget', N'COLUMN',N'Value'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Allocated cost for the period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Budget', @level2type=N'COLUMN',@level2name=N'Value'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'Plan_Campaign_Program_Tactic_Budget', N'COLUMN',N'CreatedDate'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Budget', @level2type=N'COLUMN',@level2name=N'CreatedDate'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'Plan_Campaign_Program_Tactic_Budget', N'COLUMN',N'CreatedBy'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_Budget', @level2type=N'COLUMN',@level2name=N'CreatedBy'
GO

Go
IF((NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Budget]') AND type in (N'U'))) OR ((Select Count(*) from Plan_Campaign_Program_Tactic_Budget) = 0))
BEGIN
	SET IDENTITY_INSERT [dbo].[Plan_Campaign_Program_Tactic_Budget] ON 
	Insert into Plan_Campaign_Program_Tactic_Budget(PlanTacticBudgetId,PlanTacticId,Period,Value,CreatedDate,CreatedBy) Select PlanTacticBudgetId,PlanTacticId,Period,Value,CreatedDate,CreatedBy from Plan_Campaign_Program_Tactic_Cost
	SET IDENTITY_INSERT [dbo].[Plan_Campaign_Program_Tactic_Budget] OFF
END

GO

-- Run script on MRP
Go
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic')
	begin
	IF EXISTS(SELECT * FROM sys.columns WHERE [name] =  N'CostActual'  AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
		begin
			execute ('UPDATE Plan_Campaign_Program_Tactic  SET CostActual = Cost')
			EXEC sp_RENAME 'Plan_Campaign_Program_Tactic.CostActual', 'TacticBudget', 'COLUMN'
			ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN TacticBudget float  Not NULL
		end
end
