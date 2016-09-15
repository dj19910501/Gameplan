
/* Start - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

-- DROP AND CREATE STORED PROCEDURE dbo.LineItem_Cost_Allocation
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_Cost_Allocation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.LineItem_Cost_Allocation
END
GO

CREATE PROCEDURE [dbo].[LineItem_Cost_Allocation]
( 
	@PlanTacticId INT,
	@UserId NVARCHAR(36)
)
AS
BEGIN

	SELECT Id
		,ActivityId
		,ActivityName
		,ActivityType
		,ParentActivityId
		,MainBudgeted
		,IsOwner
		,CreatedBy
		,IsEditable
		,Cost
		,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
	
		,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
		,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum
		,0 as LineItemTypeId 
	FROM
	(
	  SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
			,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
			,PT.Title AS ActivityName
			,'tactic' ActivityType
			,NULL ParentActivityId
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
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem PL ON PT.PlanTacticId = PL.PlanTacticId
		LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
		LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
		WHERE PT.IsDeleted=0 and pl.PlanTacticId = @PlanTacticId
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

	-- Calculate line item planned cost allocated by monthly/quarterly
	SELECT Id,ActivityId
	,ActivityName
	,ActivityType
	,ParentActivityId
	,MainBudgeted
	,IsOwner
	,CreatedBy
	,IsEditable
	,Cost
	,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
	,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
	,0 TotalCostSum 
	,LineItemTypeId
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
			,CASE WHEN PL.CreatedBy = @UserId THEN 1 ELSE 0 END IsOwner
			,PL.CreatedBy
			,0 as IsEditable
			,PLC.Value
			,'C'+PLC.period as period 
			,PL.LineItemTypeId as LineItemTypeId
		FROM Plan_Campaign_Program_Tactic_LineItem PL
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId = PLC.PlanLineItemId
		WHERE PL.PlanTacticId IN (@PlanTacticId) AND PL.IsDeleted=0
	) LineItem_Main
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	) PivotLineItem
END

GO