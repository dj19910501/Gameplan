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