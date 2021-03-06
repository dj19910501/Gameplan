CREATE PROCEDURE [dbo].[Plan_Budget_Cost_Actual_Detail]
( 
@PlanId INT ,
@UserId NVARCHAR(36),
@SelectedTab NVARCHAR(50)
)
AS
BEGIN
	

--If tab is planned then planned cost value return in query

IF (@SelectedTab='Planned')

	BEGIN

	SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,MainBudgeted as Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
			(
			SELECT 
					CAST(P.PlanId as NVARCHAR(20)) as Id
					,'plan_'+CAST(P.PlanId AS NVARCHAR(20)) as ActivityId
					,P.Title as ActivityName 
					,'plan' as ActivityType
					,'0' ParentActivityId
					,Budget as MainBudgeted
					,1 as IsOwner
					,P.CreatedBy
					,0 as IsAfterApproved
					,0 as IsEditable
					,Value 
					,Period
					
					
			  FROM [Plan] P
			  LEFT JOIN  plan_budget PB on P.PlanId=PB.PlanId
			  WHERE P.PlanId = @PlanId
			) Plan_Main
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
(
 
  SELECT 
		 'c_'+CAST(PC.PlanCampaignId as NVARCHAR(20)) as ActivityId
		 ,CAST(PC.PlanCampaignId as NVARCHAR(20))  Id
		 ,PC.Title as ActivityName
		 ,'campaign' as ActivityType
		 ,'plan_'+CAST(@PlanId as NVARCHAR(25)) ParentActivityId
		 ,CASE WHEN @UserId=PC.CreatedBy THEN 1 ELSE 0 END IsOwner
		 ,CampaignBudget as MainBudgeted
		 ,PC.CreatedBy
		 ,0 as IsAfterApproved
		 ,0 as IsEditable
		 ,value
		 ,period
		 ,0 Cost
		  FROM Plan_Campaign PC
		  LEFT JOIN Plan_Campaign_Budget PCB ON PC.planCampaignid = PCB.PlanCampaignId where PC.PlanId = @PlanId and IsDeleted = 0
  
) Campaign_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
UNION ALL
SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum 
,0 TotalCostSum FROM
(
SELECT   CAST(PG.PlanProgramId as NVARCHAR(20)) Id,
		 'cp_'+CAST(PG.PlanProgramId as NVARCHAR(20)) ActivityId
		,PG.Title as ActivityName
		,'program' as ActivityType
		,'c_'+CAST(PG.PlanCampaignId as NVARCHAR(25)) ParentActivityId
		,CASE WHEN @UserId=PG.CreatedBy THEN 1 ELSE 0 END IsOwner
		,PG.ProgramBudget as MainBudgeted
		,PG.CreatedBy
		,0 as IsAfterApproved
		,0 as IsEditable
		,Value,Period
		,0 Cost
		FROM Plan_Campaign_Program PG
		INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PG.PlanCampaignId AND PC.IsDeleted=0
		LEFT JOIN Plan_Campaign_Program_Budget PGB ON PG.PlanProgramId=PGB.PlanProgramId
		WHERE PC.PlanId=@PlanId AND PG.IsDeleted=0 AND PC.IsDeleted=0
  
) Program_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
UNION ALL

select Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]

,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum 
FROM
(
 
 SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
		,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
		,PT.Title AS ActivityName
		,'tactic' ActivityType
		,'cp_'+CAST(PPG.PlanProgramId AS NVARCHAR(25)) ParentActivityId
		,CASE WHEN PT.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PT.TacticBudget AS MainBudgeted
		,PT.CreatedBy 
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PTB.Value
		,PTB.Period
		,PT.Cost
		,'C'+PTCst.Period as CPeriod
		,PTCst.Value as CValue
FROM 
	Plan_Campaign_Program_Tactic PT
	INNER JOIN Plan_Campaign_Program PPG ON PPG.PlanProgramId=PT.PlanProgramId 
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
	LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0
) Tactic_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
 
) PlanCampaignProgramTacticDetails
Pivot
(
sum(CValue)
  for CPeriod in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PlanCampaignProgramTacticDetails1
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,0 TotalCostSum 
FROM
(
SELECT 
		CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
		,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
		,PL.Title as ActivityName
		,'lineitem' ActivityType
		,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
		,PL.Cost
		,0 MainBudgeted
		,CASE WHEN PL.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PL.CreatedBy
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PLC.Value
		,'C'+PLC.period as period 
	FROM Plan_Campaign_Program_Tactic_LineItem PL
	INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=PL.PlanTacticId
	INNER JOIN Plan_Campaign_Program PPG ON PT.PlanProgramId=PPG.PlanProgramId
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 AND PL.IsDeleted=0
)LineItem_Main
Pivot
(
sum (Value)
For Period in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PivotLineItem
	END

--If tab is Actual then Actual values return in query 
ELSE 

	BEGIN

		SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,MainBudgeted as Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
			(
			SELECT 
					CAST(P.PlanId as NVARCHAR(20)) as Id
					,'plan_'+CAST(P.PlanId AS NVARCHAR(20)) as ActivityId
					,P.Title as ActivityName 
					,'plan' as ActivityType
					,'0' ParentActivityId
					,Budget as MainBudgeted
					,1 as IsOwner
					,P.CreatedBy
					,0 as IsAfterApproved
					,0 as IsEditable
					,Value 
					,Period
					
					
			  FROM [Plan] P
			  LEFT JOIN  plan_budget PB on P.PlanId=PB.PlanId
			  WHERE P.PlanId = @PlanId
			) Plan_Main
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
(
 
  SELECT 
		 'c_'+CAST(PC.PlanCampaignId as NVARCHAR(20)) as ActivityId
		 ,CAST(PC.PlanCampaignId as NVARCHAR(20))  Id
		 ,PC.Title as ActivityName
		 ,'campaign' as ActivityType
		 ,'plan_'+CAST(@PlanId as NVARCHAR(25)) ParentActivityId
		 ,CASE WHEN @UserId=PC.CreatedBy THEN 1 ELSE 0 END IsOwner
		 ,CampaignBudget as MainBudgeted
		 ,PC.CreatedBy
		 ,0 as IsAfterApproved
		 ,0 as IsEditable
		 ,value
		 ,period
		 ,0 Cost
		  FROM Plan_Campaign PC
		  LEFT JOIN Plan_Campaign_Budget PCB ON PC.planCampaignid = PCB.PlanCampaignId where PC.PlanId = @PlanId and IsDeleted = 0
  
) Campaign_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
UNION ALL
SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum 
,0 TotalCostSum FROM
(
SELECT   CAST(PG.PlanProgramId as NVARCHAR(20)) Id,
		 'cp_'+CAST(PG.PlanProgramId as NVARCHAR(20)) ActivityId
		,PG.Title as ActivityName
		,'program' as ActivityType
		,'c_'+CAST(PG.PlanCampaignId as NVARCHAR(25)) ParentActivityId
		,CASE WHEN @UserId=PG.CreatedBy THEN 1 ELSE 0 END IsOwner
		,PG.ProgramBudget as MainBudgeted
		,PG.CreatedBy
		,0 as IsAfterApproved
		,0 as IsEditable
		,Value,Period
		,0 Cost
		FROM Plan_Campaign_Program PG
		INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PG.PlanCampaignId AND PC.IsDeleted=0
		LEFT JOIN Plan_Campaign_Program_Budget PGB ON PG.PlanProgramId=PGB.PlanProgramId
		WHERE PC.PlanId=@PlanId AND PG.IsDeleted=0 AND PC.IsDeleted=0
  
) Program_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
UNION ALL

select Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]

,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum 
FROM
(
 
 SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
		,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
		,PT.Title AS ActivityName
		,'tactic' ActivityType
		,'cp_'+CAST(PPG.PlanProgramId AS NVARCHAR(25)) ParentActivityId
		,CASE WHEN PT.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PT.TacticBudget AS MainBudgeted
		,PT.CreatedBy 
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PTB.Value
		,PTB.Period
		,PT.Cost
		,'C'+PTAct.Period as CPeriod
		,PTAct.ActualValue as CValue
FROM 
	Plan_Campaign_Program_Tactic PT
	INNER JOIN Plan_Campaign_Program PPG ON PPG.PlanProgramId=PT.PlanProgramId 
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual PTAct ON PT.PlanTacticId=PTAct.PlanTacticId AND PTAct.StageTitle='Cost'
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 
) Tactic_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
 
) PlanCampaignProgramTacticDetails
Pivot
(
sum(CValue)
  for CPeriod in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PlanCampaignProgramTacticDetails1
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,0 TotalCostSum 
FROM
(
SELECT 
		CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
		,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
		,PL.Title as ActivityName
		,'lineitem' ActivityType
		,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
		,PL.Cost
		,0 MainBudgeted
		,CASE WHEN PL.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PL.CreatedBy
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PLC.Value
		,'C'+PLC.period as period 
	FROM Plan_Campaign_Program_Tactic_LineItem PL
	INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=PL.PlanTacticId
	INNER JOIN Plan_Campaign_Program PPG ON PT.PlanProgramId=PPG.PlanProgramId
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 AND PL.IsDeleted=0
)LineItem_Main
Pivot
(
sum (Value)
For Period in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PivotLineItem

	END


END
