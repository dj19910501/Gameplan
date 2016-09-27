
/* Start - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

-- DROP AND CREATE STORED PROCEDURE [dbo].[LineItem_Cost_Allocation]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_Cost_Allocation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[LineItem_Cost_Allocation]
END
GO

-- [dbo].[LineItem_Cost_Allocation] 135912,470
CREATE PROCEDURE [dbo].[LineItem_Cost_Allocation]
( 
	@PlanTacticId INT,
	@UserId INT
)
AS
BEGIN

	-- Calculate tactic and line item planned cost allocated by monthly/quarterly
	SELECT Id,ActivityId
	,ActivityName
	,ActivityType
	,ParentActivityId
	,CreatedBy
	,IsEditable
	,Cost
	,[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
	,LineItemTypeId
	FROM
	(
		-- Tactic cost allocation
		SELECT DISTINCT CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
			,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
			,PT.Title AS ActivityName
			,'tactic' ActivityType
			,NULL ParentActivityId
			,PT.CreatedBy 
			,0 as IsEditable
			,PT.Cost
			,'C'+PTCst.Period as Period
			,PTCst.Value as Value
			,0 LineItemTypeId
		FROM Plan_Campaign_Program_Tactic PT
		LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
		WHERE PT.IsDeleted = 0 AND PT.PlanTacticId = @PlanTacticId

		UNION ALL
		-- Line item cost allocation
		SELECT 
			CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
			,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
			,PL.Title as ActivityName
			,'lineitem' ActivityType
			,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
			,PL.CreatedBy
			,0 as IsEditable
			,PL.Cost
			,'C'+PLC.period as period 
			,PLC.Value
			,PL.LineItemTypeId as LineItemTypeId
		FROM Plan_Campaign_Program_Tactic_LineItem PL
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId = PLC.PlanLineItemId
		WHERE PL.PlanTacticId = @PlanTacticId AND PL.IsDeleted=0

	) TacticLineItems
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	) PivotLineItem
END
GO
