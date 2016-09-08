/* Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

-- DROP AND CREATE STORED PROCEDURE dbo.LineItem_Cost_Allocation
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_Cost_Allocation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.LineItem_Cost_Allocation
END
GO

CREATE PROCEDURE [dbo].[LineItem_Cost_Allocation]
( 
	@LineItemId INT,
	@UserId NVARCHAR(36)
)
AS
BEGIN
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
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
		WHERE PL.PlanLineItemId IN (@LineItemId) AND PL.IsDeleted=0
	) LineItem_Main
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	) PivotLineItem
END
GO