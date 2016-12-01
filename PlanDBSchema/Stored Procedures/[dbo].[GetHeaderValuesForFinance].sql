-- DROP AND CREATE STORED PROCEDURE [dbo].[GetHeaderValuesForFinance]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetHeaderValuesForFinance]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetHeaderValuesForFinance]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/30/2016
-- Description:	Get HUD values for finance grid
-- =============================================
-- GetHeaderValuesForFinance 85
CREATE PROCEDURE [dbo].[GetHeaderValuesForFinance]
	-- Add the parameters for the stored procedure here
	@BudgetId		INT,
	@CurrencyRate	FLOAT = 1.0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Budget		FLOAT,
			@Forecast	FLOAT,
			@Planned	FLOAT,
			@Actual		FLOAT

	-- Get Budget, Forecast for Header
	SELECT @Budget = SUM(TotalBudget) * @CurrencyRate, @Forecast = SUM(TotalForecast) * @CurrencyRate
	FROM Budget_Detail 
	WHERE BudgetId = @BudgetId AND IsDeleted = 0

	-- Get Planned Cost for Header
	SELECT @Planned = SUM((ISNULL(PCPTL.Cost,0) * CAST(Weightage AS FLOAT)/100)) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0

	-- Get Actual for Header
	SELECT @Actual = SUM((ISNULL(PCPTLA.Value,0) * CAST(Weightage AS FLOAT)/100)) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0
	AND REPLACE(Period,'Y','') < 13

	SELECT ISNULL(@Budget,0) AS Budget, ISNULL(@Forecast,0) AS Forecast, ISNULL(@Planned,0) AS Planned, ISNULL(@Actual,0) AS Actual
END
GO
