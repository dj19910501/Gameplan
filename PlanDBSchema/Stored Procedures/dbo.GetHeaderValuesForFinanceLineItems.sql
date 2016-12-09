
/****** Object:  StoredProcedure [dbo].[GetHeaderValuesForFinanceLineItems]    Script Date: 12/09/2016 5:00:55 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetHeaderValuesForFinanceLineItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetHeaderValuesForFinanceLineItems]
GO

/****** Object:  StoredProcedure [dbo].[GetHeaderValuesForFinanceLineItems]    Script Date: 12/09/2016 5:00:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 12/09/2016
-- Description:	Get HUD values for Line item grid for marketing budget
-- =============================================
CREATE PROCEDURE [dbo].[GetHeaderValuesForFinanceLineItems]
	-- Add the parameters for the stored procedure here
	@BudgetDetailId		INT,
	@lstUserIds		NVARCHAR(MAX)='',
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

	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	-- Get Budget, Forecast for Header
	SELECT  @Forecast = SUM(TotalForecast) * @CurrencyRate
	FROM Budget_Detail BD
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	WHERE Id = @BudgetDetailId AND IsDeleted = 0

	-- Get Planned Cost for Header
	SELECT @Planned = SUM(PCPTL.Cost * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.Id = @BudgetDetailId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0

	-- Get Actual for Header
	SELECT @Actual = SUM(PCPTLA.Value * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.Id = @BudgetDetailId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0
	AND REPLACE(Period,'Y','') < 13

	SELECT ISNULL(@Budget,0) AS Budget, ISNULL(@Forecast,0) AS Forecast, ISNULL(@Planned,0) AS Planned, ISNULL(@Actual,0) AS Actual
END

GO


