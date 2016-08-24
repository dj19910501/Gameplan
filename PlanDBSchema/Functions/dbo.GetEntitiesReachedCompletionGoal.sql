
-- =================================================================================
-- Author:		Arpita Soni
-- Create date: 08/19/2016
-- Description:	Get entities from the rules which satisfy the entity completion goal
-- =================================================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetEntitiesReachedCompletionGoal]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetEntitiesReachedCompletionGoal]
GO
CREATE FUNCTION [dbo].[GetEntitiesReachedCompletionGoal]()
RETURNS @TempTable TABLE
(
    RuleId INT,
    EntityId INT,
	EntityType NVARCHAR(50),
	Indicator NVARCHAR(50),
	IndicatorComparision NVARCHAR(10),
	IndicatorGoal INT,
	CompletionGoal INT,
	ClientId UNIQUEIDENTIFIER, 
    EntityTitle NVARCHAR(255),
	StartDate DATETIME,
	EndDate DATETIME,
	PercentComplete INT
)
AS
BEGIN
	INSERT INTO @TempTable 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, P.Title, P.StartDate, P.EndDate, dbo.ConvertDateDifferenceToPercentage(P.StartDate,P.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (
				SELECT P1.PlanId, P1.Title, ISNULL(PC.StartDate,DATEADD(yy, DATEDIFF(yy,0,P1.[Year]), 0)) AS StartDate, ISNULL(PC.EndDate,DATEADD(yy, DATEDIFF(yy,0,P1.[Year]) + 1, -1)) AS EndDate FROM [Plan] P1 
				CROSS APPLY (
					SELECT MIN(PC1.StartDate) AS StartDate, MAX(PC1.EndDate) AS EndDate FROM Plan_Campaign PC1 
					WHERE P1.PlanId = PC1.PlanId AND PC1.IsDeleted = 0) PC
				WHERE P1.PlanId = AR.EntityId AND AR.EntityType = 'Plan' AND P1.IsDeleted = 0
			) P
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PC.Title, PC.StartDate, PC.EndDate, dbo.ConvertDateDifferenceToPercentage(PC.StartDate,PC.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign] PC WHERE PC.PlanCampaignId = AR.EntityId AND AR.EntityType = 'Campaign' AND PC.IsDeleted = 0) PC
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PCP.Title, PCP.StartDate, PCP.EndDate, dbo.ConvertDateDifferenceToPercentage(PCP.StartDate,PCP.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign_Program] PCP WHERE PCP.PlanProgramId = AR.EntityId AND AR.EntityType = 'Program' AND PCP.IsDeleted = 0) PCP
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PCPT.Title, PCPT.StartDate, PCPT.EndDate, dbo.ConvertDateDifferenceToPercentage(PCPT.StartDate,PCPT.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign_Program_Tactic] PCPT WHERE AR.EntityId = PCPT.PlanTacticId AND AR.EntityType = 'Tactic' AND PCPT.IsDeleted = 0 AND PCPT.[Status] IN ('In-Progress','Complete','Approved') ) PCPT
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PCPTL.Title, PCPTL.StartDate, PCPTL.EndDate, dbo.ConvertDateDifferenceToPercentage(PCPTL.StartDate,PCPTL.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (
				SELECT PCPTLineItem.PlanLineItemId, PCPTLineItem.Title, PCPTLT.StartDate, PCPTLT.EndDate FROM [Plan_Campaign_Program_Tactic_LineItem] PCPTLineItem 
				CROSS APPLY (
					SELECT PCPT.StartDate,PCPT.EndDate FROM [Plan_Campaign_Program_Tactic] PCPT 
					WHERE PCPTLineItem.PlanTacticId = PCPT.PlanTacticId AND PCPT.IsDeleted = 0 AND PCPT.[Status] IN ('In-Progress','Complete','Approved') 
				) PCPTLT
				WHERE PCPTLineItem.PlanLineItemId = AR.EntityId AND AR.EntityType = 'LineItem' AND AR.Indicator = 'PLANNEDCOST' AND PCPTLineItem.IsDeleted = 0
			) PCPTL
			WHERE AR.EntityType IN ('Plan','Campaign','Program','Tactic','LineItem')
					
	RETURN
END
GO