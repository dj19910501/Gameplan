
-- Create User Defined Table Type
/****** Object:  UserDefinedTableType [dbo].[TacticForRuleEntities]    Script Date: 08/19/2016 06:37:10 PM ******/
IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TacticForRuleEntities')
CREATE TYPE [dbo].[TacticForRuleEntities] AS TABLE(
	[RuleId] [int] NULL,
	[EntityId] [int] NULL,
	[EntityType] [nvarchar](50) NULL,
	[Indicator] [nvarchar](50) NULL,
	[IndicatorTitle] [nvarchar](255) NULL,
	[IndicatorComparision] [nvarchar](10) NULL,
	[IndicatorGoal] [int] NULL,
	[CompletionGoal] [int] NULL,
	[Frequency] [nvarchar](50) NULL,
	[DayOfWeek] [tinyint] NULL,
	[DateOfMonth] [tinyint] NULL,
	[UserId] [uniqueidentifier] NULL,
	[ClientId] [uniqueidentifier] NULL,
	[EntityTitle] [nvarchar](255) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[PercentComplete] [int] NULL,
	[ProjectedStageValue] [int] NULL,
	[ActualStageValue] [int] NULL,
	[CalculatedPercentGoal] [int] NULL
)
GO