-- Created By Nishant Sheth
--To Check BUDGET Table Exist or not and if not exist then create

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
print 'hello'
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
