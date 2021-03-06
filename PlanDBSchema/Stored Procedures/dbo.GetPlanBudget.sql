/****** Object:  StoredProcedure [dbo].[GetPlanBudget]    Script Date: 10/05/2016 5:17:16 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPlanBudget]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPlanBudget]
GO
/****** Object:  StoredProcedure [dbo].[GetPlanBudget]    Script Date: 10/05/2016 5:17:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPlanBudget]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetPlanBudget] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 09/08/2016
-- Description:	This store proc. return data for budget tab for repective plan, campaign, program and tactic
-- =============================================
ALTER PROCEDURE [dbo].[GetPlanBudget]--[GetPlanBudget] '19421','98,104,66,410,308','14917,32098,32097,14918,14916,14919,14920','Created,Submitted,Approved,In-Progress,Complete,Declined',308,'Tactic','2016-2017',0
	(
	@PlanId NVARCHAR(MAX),
	@ownerIds nvarchar(max)='',
	@tactictypeIds varchar(max)='',
	@statusIds varchar(max)='',
	@UserID INT = 0,
	@ViewBy varchar(500)='',
	@TimeFrame VARCHAR(20)='',
	@isGrid bit=0
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
			ColorCode NVARCHAR(50),
			TaskId NVARCHAR(500),
			ParentTaskId NVARCHAR(500),
			EntityTitle NVARCHAR(1000),
			ROIPackageIds	Varchar(max)
)

Declare @ViewbyTable TABLE
(
	EntityTitle NVARCHAR(1000),
	EntityType NVARCHAR(50),
	TaskId NVARCHAR(500)
)

INSERT INTO @tmp
SELECT EntityId,ParentEntityId,EntityType,StartDate,EndDate,ColorCode,TaskId,ParentTaskId,EntityTitle,ROIPackageIds FROM fnViewByEntityHierarchy(@PlanId,@ownerIds,@tactictypeIds,@statusIds,@ViewBy,@TimeFrame,@isGrid)

INSERT INTO @ViewbyTable
SELECT EntityTitle,EntityType,TaskId FROM @tmp WHERE EntityType NOT IN ('Plan','Campaign','Program','Tactic','LineItem')



SELECT		 0 Id
			,TaskId
			,NULL ParentTaskId
			,NULL ActivityId
			,EntityType ActivityType
			,EntityTitle Title
			,NULL ParentActivityId
			,GETDATE() StartDate
			,GETDATE() EndDate
			,NULL ColorCode
			,0 LinkTacticId
			,0 TacticTypeId
			,NULL MachineName
			,0 CreatedBy
			,NULL LineItemTypeId
			,0 IsAfterApproved
			,0 IsOwner
			,0 Cost
			,0 Budget
			,0 PlanYear
			--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
			,NULL [Y1],NULL [Y2],NULL [Y3],NULL [Y4],NULL [Y5],NULL [Y6],NULL [Y7],NULL [Y8],NULL [Y9],NULL [Y10],NULL [Y11],NULL [Y12]
			,NULL [Y13],NULL [Y14],NULL [Y15],NULL [Y16],NULL [Y17],NULL [Y18],NULL [Y19],NULL [Y20],NULL [Y21],NULL [Y22],NULL [Y23],NULL [Y24]

			,0 TotalUnallocatedBudget
			--Viewby entity has no cost at table level so it default set to null
			,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
			,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
			,0 TotalUnAllocationCost
			--Plan entity has no actual at table level so it default set to null
			,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
			,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
			,0 TotalAllocationActual
			,NULL ROITacticType
			,0 IsAnchorTacticId
			,NULL CalendarHoneycombpackageIDs
			,NULL LinkedPlanName
			FROM @ViewbyTable
UNION ALL

SELECT		Id
			,TaskId
			,ParentTaskId
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
			,0 Cost
			,Budget
			,PlanYear
			--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
			,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
			,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]

			,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
			--Plan entity has no cost at table level so it default set to null
			,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
			,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
			,0 TotalUnAllocationCost
			--Plan entity has no actual at table level so it default set to null
			,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
			,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
			,0 TotalAllocationActual
			,NULL ROITacticType
			,0 IsAnchorTacticId
			,NULL CalendarHoneycombpackageIDs
			,NULL LinkedPlanName
		FROM 
				(SELECT 
					P.PlanId Id
					,H.TaskId
					,H.ParentTaskId
					,H.EntityId ActivityId
					,P.Title
					,'plan' as ActivityType
					,H.ParentEntityId ParentActivityId
					,ISNULL(H.StartDate, GETDATE()) AS StartDate
					,ISNULL(H.EndDate, GETDATE()) AS EndDate
					,H.ColorCode
					,P.CreatedBy
					,P.[Year] PlanYear
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
					MAX(value)
					for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
									,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
				)PLNMain
UNION ALL
SELECT 
		Id
		,TaskId
		,ParentTaskId
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
		,0 Cost
		,Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12],[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
		--Plan entity has no cost at table level so it default set to null
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
		,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
		,0 TotalUnAllocationCost
		--Plan entity has no actual at table level so it default set to null
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
		,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
		,0 TotalAllocationActual
		,NULL ROITacticType
		,0 IsAnchorTacticId
		,NULL CalendarHoneycombpackageIDs
		,NULL LinkedPlanName
	 FROM
			(SELECT 
				PC.PlanCampaignId Id	
				,H.TaskId
				,H.ParentTaskId
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
				MAX(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
								,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
			)CampaignMain
UNION ALL
	SELECT 
		Id
		,TaskId
		,ParentTaskId
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
		,0 Cost
		,Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12],[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
		--Plan entity has no cost at table level so it default set to null
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
		,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
		,0 TotalUnAllocationCost
		--Plan entity has no actual at table level so it default set to null
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
		,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
		,0 TotalAllocationActuals
		,NULL ROITacticType
		,0 IsAnchorTacticId
		,NULL CalendarHoneycombpackageIDs
		,NULL LinkedPlanName
	 FROM
			(SELECT 
				PCP.PlanProgramId Id
				,H.TaskId
				,H.ParentTaskId
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
				MAX(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
								,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
			)ProgramMain
UNION ALL
	SELECT 
		Id
		,TaskId
		,ParentTaskId
		,ActivityId
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
		,IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,Cost		
		,Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]
		
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
		
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24]
		
		,Cost - (ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0) 
			+ISNULL([CostY13],0)+ ISNULL([CostY14],0)+ ISNULL([CostY15],0)+ ISNULL([CostY16],0)+ISNULL([CostY17],0)+ ISNULL([CostY18],0)+ ISNULL([CostY19],0)+ ISNULL([CostY20],0)+ISNULL([CostY21],0)+ ISNULL([CostY22],0)+ ISNULL([CostY23],0)+ ISNULL([CostY24],0)) TotalUnAllocationCost
		
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24]
		
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) 
			+ISNULL([ActualY13],0)+ ISNULL([ActualY14],0)+ ISNULL([ActualY15],0)+ ISNULL([ActualY16],0)+ISNULL([ActualY17],0)+ ISNULL([ActualY18],0)+ ISNULL([ActualY19],0)+ ISNULL([ActualY20],0)+ISNULL([ActualY21],0)+ ISNULL([ActualY22],0)+ ISNULL([ActualY23],0)+ ISNULL([ActualY24],0) TotalAllocationActuals
	     ,AssetType ROITacticType
		,ISNULL(AnchorTacticID,0) IsAnchorTacticId
		,CalendarHoneycombpackageIDs
		,LinkedPlanName
	 FROM
			(SELECT
				PCPT.PlanTacticId Id 
				,H.TaskId
				,H.ParentTaskId
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
				,'Actual'+PCPTA.Period as APeriod
				,PCPT.Cost
				,RPD.AnchorTacticID
				,TP.AssetType
				,CalendarHoneycombpackageIDs = H.ROIPackageIds
				,P.Title as 'LinkedPlanName'
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program_Tactic PCPT ON H.EntityId=PCPT.PlanTacticId 
				LEFT JOIN Plan_Campaign_Program_Tactic_Budget PCPTB ON PCPT.PlanTacticId=PCPTB.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Cost PCPTC ON PCPT.PlanTacticId=PCPTC.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Actual PCPTA ON PCPT.PlanTacticId=PCPTA.PlanTacticId AND PCPTA.StageTitle='Cost'
				LEFT JOIN ROI_PackageDetail RPD ON RPD.PlanTacticId = PCPT.PlanTacticId
				LEFT JOIN [Plan] P ON P.PlanId = PCPT.LinkedPlanId
				INNER JOIN TacticType TP ON (TP.TacticTypeId = PCPT.TacticTypeId)				
			WHERE H.EntityType='Tactic'
			)Tactic
			PIVOT
			(
				MAX(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
								,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
			)TacticMain
			PIVOT
			(
				MAX(CValue)
				for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
								,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24])
			)TacticMain1
			PIVOT
			(
				MAX(AValue)
				for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
								,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24])
			)TacticMain2

UNION ALL
	SELECT 
		Id
		,TaskId
		,ParentTaskId
		,ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,ISNULL(LinkedLineItemId, 0) LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,LineItemTypeId
		,IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,Cost
		,0 Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		--Line item has no budget at table level so its default set to null
		,NULL [Y1],NULL [Y2],NULL [Y3],NULL [Y4],NULL [Y5],NULL [Y6],NULL [Y7],NULL [Y8],NULL [Y9],NULL [Y10],NULL [Y11],NULL [Y12]
		,NULL [Y13],NULL [Y14],NULL [Y15],NULL [Y16],NULL [Y17],NULL [Y18],NULL [Y19],NULL [Y20],NULL [Y21],NULL [Y22],NULL [Y23],NULL [Y24]
		,0 TotalUnallocatedBudget
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24]
		
		,Cost - (ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0) 
			+ISNULL([CostY13],0)+ ISNULL([CostY14],0)+ ISNULL([CostY15],0)+ ISNULL([CostY16],0)+ISNULL([CostY17],0)+ ISNULL([CostY18],0)+ ISNULL([CostY19],0)+ ISNULL([CostY20],0)+ISNULL([CostY21],0)+ ISNULL([CostY22],0)+ ISNULL([CostY23],0)+ ISNULL([CostY24],0)) TotalUnAllocationCost
		
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24]
		
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) 
			+ISNULL([ActualY13],0)+ ISNULL([ActualY14],0)+ ISNULL([ActualY15],0)+ ISNULL([ActualY16],0)+ISNULL([ActualY17],0)+ ISNULL([ActualY18],0)+ ISNULL([ActualY19],0)+ ISNULL([ActualY20],0)+ISNULL([ActualY21],0)+ ISNULL([ActualY22],0)+ ISNULL([ActualY23],0)+ ISNULL([ActualY24],0)TotalAllocationActuals
	    ,NULL ROITacticType
		,0 IsAnchorTacticId
		,NULL CalendarHoneycombpackageIDs
		,NULL LinkedPlanName
	 FROM
		 (SELECT	PCPTL.PlanLineItemId Id
					,H.TaskId
					,H.ParentTaskId
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
					,'Actual'+PCPTLA.Period as APeriod
					,PCPTL.Cost
					,PCPTL.LinkedLineItemId
				FROM @tmp H
					INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON H.EntityId=PCPTL.PlanLineItemId 
					INNER JOIN Plan_Campaign_Program_Tactic PCPT ON PCPTL.PlanTacticId=PCPT.PlanTacticId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PCPTLC ON PCPTL.PlanLineItemId=PCPTLC.PlanLineItemId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId=PCPTLA.PlanLineItemId
				WHERE H.EntityType='LineItem'
				)LineItem
				PIVOT
				(
				 MAX(CValue)
				  for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
									,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24])
				)LineItemMain
				PIVOT
				(
				 MAX(AValue)
				  for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
									,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24])
				)LineItemMain
END




GO
