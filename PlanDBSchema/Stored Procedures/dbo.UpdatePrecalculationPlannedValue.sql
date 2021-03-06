/****** Object:  StoredProcedure [dbo].[UpdatePrecalculationPlannedValue]    Script Date: 12/12/2016 8:22:48 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdatePrecalculationPlannedValue]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdatePrecalculationPlannedValue]
GO
/****** Object:  StoredProcedure [dbo].[UpdatePrecalculationPlannedValue]    Script Date: 12/12/2016 8:22:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdatePrecalculationPlannedValue]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdatePrecalculationPlannedValue] AS' 
END
GO
-- =============================================
-- Author:		Viral
-- Create date: 12/12/2016
-- Description:	To update Planned values to Precalculation table
-- =============================================
ALTER PROCEDURE [dbo].[UpdatePrecalculationPlannedValue]  
	-- Add the parameters for the stored procedure here
	@weightage float, -- lineitem budget weightage
	@budgetDetailId int, -- lineitem budget detail id
	@planLineItemId int,
	@IsAdd bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	-- Update Planned values into [MV].[PreCalculatedMarketingBudget] table
-- Reduce the old budget detail value from PreCalcuation table.
	IF(@IsAdd=0)
	BEGIN
			-- Update Actual values into [MV].[PreCalculatedMarketingBudget] table
			-- Reduce the old budget detail value from PreCalcuation table.
			UPDATE PreCal SET Y1_Planned = IsNull(Y1_Planned,0) - IsNull([Y1],0),Y2_Planned = IsNull(Y2_Planned,0) - IsNull([Y2],0),
							  Y3_Planned = IsNull(Y3_Planned,0) - IsNull([Y3],0),Y4_Planned = IsNull(Y4_Planned,0) - IsNull([Y4],0),
							  Y5_Planned = IsNull(Y5_Planned,0) - IsNull([Y5],0),Y6_Planned = IsNull(Y6_Planned,0) - IsNull([Y6],0),
							  Y7_Planned = IsNull(Y7_Planned,0) - IsNull([Y7],0),Y8_Planned = IsNull(Y8_Planned,0) - IsNull([Y8],0),
							  Y9_Planned = IsNull(Y9_Planned,0) - IsNull([Y9],0),Y10_Planned = IsNull(Y10_Planned,0) - IsNull([Y10],0),
							  Y11_Planned = IsNull(Y11_Planned,0) - IsNull([Y11],0),Y12_Planned = IsNull(Y12_Planned,0) - IsNull([Y12],0)
			FROM [MV].PreCalculatedMarketingBudget PreCal 
			INNER JOIN 
			(
				-- Get monthly actuals amount with pivoting
				-- Apply weightage on actual and sum up actuals for all line items associated to the single budget 
				SELECT @budgetDetailId as BudgetDetailId, LC.Period, (Value * CAST(@weightage AS FLOAT)/100) AS Value 
				FROM Plan_Campaign_Program_Tactic_LineItem L
				JOIN Plan_Campaign_Program_Tactic_LineItem_Cost LC on L.PlanLineItemId = LC.PlanLineItemId
				WHERE L.PlanLineItemId=@planLineItemId and L.IsDeleted=0 and L.LineItemTypeId IS NOT NULL
				
			) P
			PIVOT
			(
				MIN(Value)
				FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
			) AS Pvt
			ON PreCal.BudgetDetailId = Pvt.BudgetDetailId and PreCal.BudgetDetailId =@budgetDetailId
	END
	ELSE
	BEGIN
			-- Update Actual values into [MV].[PreCalculatedMarketingBudget] table
			-- Reduce the old budget detail value from PreCalcuation table.
			UPDATE PreCal SET Y1_Planned = IsNull(Y1_Planned,0) + IsNull([Y1],0),Y2_Planned = IsNull(Y2_Planned,0) + IsNull([Y2],0),
							  Y3_Planned = IsNull(Y3_Planned,0) + IsNull([Y3],0),Y4_Planned = IsNull(Y4_Planned,0) + IsNull([Y4],0),
							  Y5_Planned = IsNull(Y5_Planned,0) + IsNull([Y5],0),Y6_Planned = IsNull(Y6_Planned,0) + IsNull([Y6],0),
							  Y7_Planned = IsNull(Y7_Planned,0) + IsNull([Y7],0),Y8_Planned = IsNull(Y8_Planned,0) + IsNull([Y8],0),
							  Y9_Planned = IsNull(Y9_Planned,0) + IsNull([Y9],0),Y10_Planned = IsNull(Y10_Planned,0) + IsNull([Y10],0),
							  Y11_Planned = IsNull(Y11_Planned,0) + IsNull([Y11],0),Y12_Planned = IsNull(Y12_Planned,0) + IsNull([Y12],0)
			FROM [MV].PreCalculatedMarketingBudget PreCal 
			INNER JOIN 
			(
				-- Get monthly actuals amount with pivoting
				-- Apply weightage on actual and sum up actuals for all line items associated to the single budget 
				SELECT @budgetDetailId as BudgetDetailId, LC.Period, (Value * CAST(@weightage AS FLOAT)/100) AS Value 
				FROM Plan_Campaign_Program_Tactic_LineItem L
				JOIN Plan_Campaign_Program_Tactic_LineItem_Cost LC on L.PlanLineItemId = LC.PlanLineItemId
				WHERE L.PlanLineItemId=@planLineItemId and L.IsDeleted=0 and L.LineItemTypeId IS NOT NULL
				
			) P
			PIVOT
			(
				MIN(Value)
				FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
			) AS Pvt
			ON PreCal.BudgetDetailId = Pvt.BudgetDetailId and PreCal.BudgetDetailId =@budgetDetailId
	END
END

GO
