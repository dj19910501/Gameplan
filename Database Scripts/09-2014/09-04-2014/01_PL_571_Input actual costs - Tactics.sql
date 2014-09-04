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

END

--=================== End [Plan_Campaign_Program_Tactic_LineItem_Actual]==================


