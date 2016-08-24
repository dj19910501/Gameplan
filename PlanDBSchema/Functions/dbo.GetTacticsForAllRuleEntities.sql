
-- ======================================================================================================
-- Author:		Arpita Soni
-- Create date: 08/17/2016
-- Description:	List of tactics with projected and actual values for all entities involved in alert rules
-- ======================================================================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetTacticsForAllRuleEntities]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetTacticsForAllRuleEntities]
GO
CREATE FUNCTION [dbo].[GetTacticsForAllRuleEntities](
	@TempEntityTable [TacticForRuleEntities] READONLY
)
RETURNS @TempTable TABLE
(
	PlanLineItemId INT,
	PlanTacticId INT,
	PlanProgramId INT,
	PlanCampaignId INT,
	PlanId INT,
	Indicator NVARCHAR(50),
	ProjectedStageValue FLOAT,
	ActualStageValue FLOAT
)
AS
BEGIN
		INSERT INTO @TempTable
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue
		FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = RuleEntityTable.EntityId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanId = [Plan].PlanId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanCampaignId=Campaign.PlanCampaignId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK)
						 WHERE Tactic.PlanProgramId = Program.PlanProgramId AND Tactic.IsDeleted = 0
						 AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')
													THEN 'PROJECTEDSTAGEVALUE'
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Plan'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,
			Model.ModelId,Model.ClientId,Stage.Code
		) P
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		
		UNION 
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = RuleEntityTable.EntityId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanCampaignId=Campaign.PlanCampaignId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanProgramId=Program.PlanProgramId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Campaign'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,Model.ModelId,Model.ClientId,Stage.Code
		) PC
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		
		UNION 
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=RuleEntityTable.EntityId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanCampaignId,PlanId  FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanProgramId=Program.PlanProgramId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Program'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,Model.ModelId,Model.ClientId,Stage.Code
		) PCP
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		
		UNION 
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanTacticId=RuleEntityTable.EntityId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanCampaignId,PlanId  FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Tactic'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,Model.ModelId,Model.ClientId,Stage.Code
		) PCPT
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		
		UNION 
		SELECT PlanLineItemId, NULL, NULL, NULL, NULL, Indicator, ProjectedStageValue, 
		SUM(LineItemActuals) AS ActualStageValue FROM
		(
			SELECT PlanLineItemId, RuleEntityTable.Indicator, Cost AS ProjectedStageValue
			, SUM(ActualValue) AS LineItemActuals
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanLineItemId,Cost,PlanTacticId FROM [Plan_Campaign_Program_Tactic_LineItem] AS LineItem WITH (NOLOCK) WHERE LineItem.PlanLineItemId=RuleEntityTable.EntityId AND LineItem.IsDeleted=0) LineItem
			OUTER APPLY (SELECT Value AS ActualValue FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual 
				WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId
			) Actual
			WHERE RuleEntityTable.EntityType = 'LineItem' AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			GROUP BY PlanLineItemId, RuleEntityTable.Indicator, Cost
		) PCPTL
		GROUP BY PlanLineItemId, Indicator, ProjectedStageValue

	RETURN
END

GO
