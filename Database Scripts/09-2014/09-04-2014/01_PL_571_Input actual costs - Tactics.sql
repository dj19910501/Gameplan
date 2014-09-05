--===================[Plan_Campaign_Program_Tactic_LineItem_Actual]==================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_LineItem_Actual')

BEGIN
	CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual](
		[PlanLineItemId] [int] NOT NULL,
		[Period] [nvarchar](5) NOT NULL,
		[Value] [float] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_Plan_Campaign_Program_Tactic_LineItem_Actual] PRIMARY KEY CLUSTERED 
	(
		[PlanLineItemId] ASC,
		[Period] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	 CONSTRAINT [IX_Plan_Campaign_Program_Tactic_LineItem_Actual] UNIQUE NONCLUSTERED 
	(
		[PlanLineItemId] ASC,
		[Period] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_LineItem')
	BEGIN

		ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Actual_Plan_Campaign_Program_Tactic_LineItem] FOREIGN KEY([PlanLineItemId])
		REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])
		ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Actual_Plan_Campaign_Program_Tactic_LineItem]

	END


	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated PlanLineItemId and composite PK with Period to uniquely identify line item and period combination.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'PlanLineItemId'


	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Composite PK with LineItemTypeId to uniquely identify line item and period combination.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'Period'


	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Value for a period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'Value'


	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'CreatedDate'


	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END

--=================== End [Plan_Campaign_Program_Tactic_LineItem_Actual]==================


