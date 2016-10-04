
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
	,Cost
	,StartDate
	,EndDate
	,LinkTacticId
	,ISNULL([Y1],0) [Y1],ISNULL([Y2],0) [Y2],ISNULL([Y3],0) [Y3],ISNULL([Y4],0) [Y4],ISNULL([Y5],0) [Y5],ISNULL([Y6],0) [Y6],
	ISNULL([Y7],0) [Y7],ISNULL([Y8],0) [Y8],ISNULL([Y9],0) [Y9],ISNULL([Y10],0) [Y10],ISNULL([Y11],0) [Y11],ISNULL([Y12],0) [Y12],
	ISNULL([Y13],0) [Y13],ISNULL([Y14],0) [Y14],ISNULL([Y15],0) [Y15],ISNULL([Y16],0) [Y16],ISNULL([Y17],0) [Y17],
	ISNULL([Y18],0) [Y18],ISNULL([Y19],0) [Y19],ISNULL([Y20],0) [Y20],ISNULL([Y21],0) [Y21],ISNULL([Y22],0) [Y22],
	ISNULL([Y23],0) [Y23],ISNULL([Y24],0) [Y24]
	,LineItemTypeId
	FROM
	(
		-- Tactic cost allocation
		SELECT DISTINCT CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
			,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
			,PT.Title AS ActivityName
			,'tactic' ActivityType
			,PT.PlanProgramId AS ParentActivityId
			,PT.CreatedBy 
			,PT.Cost
			,PTCst.Period as Period
			,PTCst.Value as Value
			,0 LineItemTypeId
			,PT.StartDate 
			,PT.EndDate
			,PT.LinkedTacticId AS LinkTacticId
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
			,PL.Cost
			,PLC.period as period 
			,PLC.Value
			,PL.LineItemTypeId as LineItemTypeId
			,PT.StartDate 
			,PT.EndDate
			,0 AS LinkTacticId
		FROM Plan_Campaign_Program_Tactic_LineItem PL
		INNER JOIN Plan_Campaign_Program_Tactic PT ON PL.PlanTacticId = PT.PlanTacticId 
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId = PLC.PlanLineItemId
		WHERE PT.PlanTacticId = @PlanTacticId AND PL.IsDeleted = 0 AND PT.IsDeleted = 0

	) TacticLineItems
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12], 
		[Y13],[Y14],[Y15],[Y16],[Y17],[Y18],[Y19],[Y20],[Y21],[Y22],[Y23],[Y24])
	) PivotLineItem
END
GO