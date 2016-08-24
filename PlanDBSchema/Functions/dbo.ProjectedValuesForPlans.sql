=========================
-- Author:		Arpita Soni
-- Create date: 08/17/2016
-- Description:	List of plans with projected and actual values for all entities involved in alert rules
-- =============================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[ProjectedValuesForPlans]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[ProjectedValuesForPlans]
GO
CREATE FUNCTION [dbo].[ProjectedValuesForPlans](
	@TempEntityTable [TacticForRuleEntities] READONLY
)
RETURNS @TempTable TABLE
(
	PlanTacticId INT NULL,
	PlanProgramId INT NULL,
	PlanCampaignId INT NULL,
	PlanId INT,
	Indicator NVARCHAR(50),
	ProjectedStageValue FLOAT,
	ActualStageValue FLOAT NULL
)
AS
BEGIN
		INSERT INTO @TempTable
		SELECT NULL, NULL, NULL, PlanId, Indicator, ProjectedStageValue ,NULL
		FROM
		(
			SELECT [Plan].PlanId, 
			[dbo].[PlanIndicatorProjectedValue]([Plan].PlanId,[Model].ModelId, [Plan].GoalType,RuleEntityTable.Indicator,[Model].ClientId) AS ProjectedStageValue 
			,RuleEntityTable.Indicator
			FROM @TempEntityTable RuleEntityTable 
			CROSS APPLY (SELECT PlanId,ModelId,GoalType From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = RuleEntityTable.EntityId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			WHERE RuleEntityTable.EntityType = 'Plan'  
			GROUP BY [Plan].PlanId,RuleEntityTable.Indicator,Model.ModelId,Model.ClientId,[Plan].GoalType
		) P
		GROUP BY PlanId, Indicator, ProjectedStageValue
		RETURN 
END
GO