
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 09/08/2016
-- Description:	This store proc. return data for budget tab for repective plan, campaign, program and tactic
-- =============================================
ALTER PROCEDURE [dbo].[GetPlanBudget]--[GetPlanBudget] '20212,20203,19569'
	(
	@PlanId NVARCHAR(MAX),
	@ownerIds nvarchar(max)='',
	@tactictypeIds varchar(max)='',
	@statusIds varchar(max)='',
	@UserID INT = 0
	)
AS
BEGIN
	
DECLARE @tmp TABLE
(
			EntityId		BIGINT,
			ParentEntityId	BIGINT,
			EntityType NVARCHAR(50),
			StartDate DATETIME,
			EndDate DATETIME,
			ColorCode NVARCHAR(50)
)

INSERT INTO @tmp
--SELECT * FROM fnGetFilterEntityHierarchy( @PlanId,@ownerIds,@tactictypeIds,@statusIds)
SELECT EntityId,ParentEntityId,EntityType,StartDate,EndDate,ColorCode FROM fnGetEntitieHirarchyByPlanId(@PlanId)

SELECT		Id
			,ActivityId
			,ActivityType
			,Title
			,ParentActivityId
			,StartDate
			,EndDate
			,ColorCode
			,0 LinkTacticId
			,0 TacticTypeId
			,NULL MachineName
			,CreatedBy
			,NULL LineItemTypeId
			,0 IsAfterApproved
			,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
			,Budget
			,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
			,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalUnallocatedBudget
			,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
			,0 TotalAllocationCost
			,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
			,0 TotalAllocationActual
		FROM 
				(SELECT 
					P.PlanId Id
					,H.EntityId ActivityId
					,P.Title
					,'plan' as ActivityType
					,H.ParentEntityId ParentActivityId
					,ISNULL(H.StartDate, GETDATE()) AS StartDate
					,ISNULL(H.EndDate, GETDATE()) AS EndDate
					,H.ColorCode
					,P.CreatedBy
					, Budget
					,PB.Value
					,PB.Period
				FROM @tmp H 
					INNER JOIN [Plan] P ON H.EntityId=P.PlanId 
					LEFT JOIN Plan_Budget PB ON P.PlanId=PB.PlanId
				WHERE H.EntityType='Plan' 
				)Pln
				PIVOT
				(
					sum(value)
					for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
				)PLNMain
UNION ALL
SELECT 
		Id,
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,0 LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,NULL LineItemTypeId
		,0 IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,Budget
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalUnallocatedBudget
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
		,0 TotalAllocationCost
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
		,0 TotalAllocationActual
	 FROM
			(SELECT 
				PC.PlanCampaignId Id	
				,H.EntityId ActivityId
				,PC.Title
				,'campaign' as ActivityType
				,H.ParentEntityId ParentActivityId
				,ISNULL(H.StartDate, GETDATE()) AS StartDate
				,ISNULL(H.EndDate, GETDATE()) AS EndDate
				,H.ColorCode
				,PC.CreatedBy
				,CampaignBudget Budget
				,PCB.Value
				,PCB.Period
			FROM @tmp H
				INNER JOIN Plan_Campaign PC  ON H.EntityId=PC.PlanCampaignId 
				LEFT JOIN Plan_Campaign_Budget PCB ON PC.PlanCampaignId=PCB.PlanCampaignId  
			WHERE H.EntityType='Campaign'
			)Campaign
			PIVOT
			(
				sum(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
			)CampaignMain
UNION ALL
	SELECT 
		Id,
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,0 LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,NULL LineItemTypeId
		,0 IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,Budget
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalUnallocatedBudget
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
		,0 TotalAllocationCost
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
		,0 TotalAllocationActuals
	 FROM
			(SELECT 
				PCP.PlanProgramId Id
				,H.EntityId ActivityId
				,PCP.Title
				,'program' as ActivityType
				,H.ParentEntityId ParentActivityId
				,ISNULL(H.StartDate, GETDATE()) AS StartDate
				,ISNULL(H.EndDate, GETDATE()) AS EndDate
				,H.ColorCode
				,PCP.CreatedBy
				,PCP.ProgramBudget Budget
				,PCPB.Value
				,PCPB.Period
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program PCP ON H.EntityId=PCP.PlanProgramId 
				LEFT JOIN Plan_Campaign_Program_Budget PCPB ON PCP.PlanProgramId=PCPB.PlanProgramId
			WHERE H.EntityType='Program'
			)Program
			PIVOT
			(
				sum(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
			)ProgramMain
UNION ALL
	SELECT 
		Id,
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,ISNULL(LinkedTacticId, 0) LinkTacticId
		,ISNULL(TacticTypeId, 0) TacticTypeId
		,TacticCustomName MachineName
		,CreatedBy
		,NULL LineItemTypeId
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,IsAfterApproved
		,Budget
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalUnallocatedBudget
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,(ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0)) TotalAllocationCost
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) TotalAllocationActuals
	 FROM
			(SELECT
				PCPT.PlanTacticId Id 
				,H.EntityId ActivityId
				,PCPT.Title
				,'tactic' as ActivityType
				,H.ParentEntityId ParentActivityId
				,ISNULL(H.StartDate, GETDATE()) AS StartDate
				,ISNULL(H.EndDate, GETDATE()) AS EndDate
				,H.ColorCode
				,PCPT.LinkedTacticId
				,PCPT.TacticTypeId
				,PCPT.TacticCustomName 
				,PCPT.CreatedBy
				,PCPT.TacticBudget Budget
				,CASE WHEN PCPT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
				,PCPTB.Value
				,PCPTB.Period
				,PCPTC.Value as CValue
				,'Cost'+PCPTC.Period as CPeriod
				,PCPTA.Actualvalue as AValue
				,'A'+PCPTA.Period as APeriod
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program_Tactic PCPT ON H.EntityId=PCPT.PlanTacticId 
				LEFT JOIN Plan_Campaign_Program_Tactic_Budget PCPTB ON PCPT.PlanTacticId=PCPTB.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Cost PCPTC ON PCPT.PlanTacticId=PCPTC.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Actual PCPTA ON PCPT.PlanTacticId=PCPTA.PlanTacticId AND PCPTA.StageTitle='Cost'
			WHERE H.EntityType='Tactic'
			)Tactic
			PIVOT
			(
				sum(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
			)TacticMain
			PIVOT
			(
				sum(CValue)
				for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12])
			)TacticMain1
			PIVOT
			(
				sum(AValue)
				for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12])
			)TacticMain2

UNION ALL
	SELECT 
		Id,
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,0 LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,LineItemTypeId
		,IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,0 Budget
		,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12]
		,0 TotalUnallocatedBudget
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,(ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0)) TotalAllocationCost
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) TotalAllocationActuals
	 FROM
		 (SELECT	PCPTL.PlanLineItemId Id
					,H.EntityId ActivityId
					,PCPTL.Title
					,'lineitem' as ActivityType
					,H.ParentEntityId ParentActivityId
					,ISNULL(H.StartDate, GETDATE()) AS StartDate
					,ISNULL(H.EndDate, GETDATE()) AS EndDate
					,H.ColorCode
					,PCPTL.CreatedBy
					,PCPTL.LineItemTypeId LineItemTypeId
					,CASE WHEN PCPT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
					,PCPTLC.Value as CValue
					,'Cost'+PCPTLC.Period as CPeriod
					,PCPTLA.Value as AValue
					,'A'+PCPTLA.Period as APeriod
				FROM @tmp H
					INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON H.EntityId=PCPTL.PlanLineItemId 
					INNER JOIN Plan_Campaign_Program_Tactic PCPT ON PCPTL.PlanTacticId=PCPT.PlanTacticId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PCPTLC ON PCPTL.PlanLineItemId=PCPTLC.PlanLineItemId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId=PCPTLA.PlanLineItemId
				WHERE H.EntityType='LineItem'
				)LineItem
				PIVOT
				(
				 sum(CValue)
				  for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12])
				)LineItemMain
				PIVOT
				(
				 sum(AValue)
				  for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12])
				)LineItemMain
END
GO
