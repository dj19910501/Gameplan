
/****** Object:  StoredProcedure [MV].[GetFinanceGridData]    Script Date: 12/09/2016 2:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to fetch finance grid data
-- =============================================
ALTER PROCEDURE [MV].[GetFinanceGridData]
	@BudgetId		INT,
	@ClientId		INT,
	@timeframe		VARCHAR(50) = 'Yearly',
	@lstUserIds		NVARCHAR(MAX) = '',
	@UserId			INT,
	@CurrencyRate	FLOAT = 1.0
AS
BEGIN
	
	-- EXEC MV.[GetFinanceGridData] 2807,24,'months','470,308,104',470,0.5
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Declare local variables
	BEGIN

		-- Start: declare timeframe related variables
		Declare @ThisYear varchar(100) ='Yearly' -- This Year
		Declare @ThisQuarters varchar(100) ='quarters' -- This Year (Quarterly)
		Declare @ThisMonthly varchar(100) ='months' -- This Year (Monthly)
		Declare @Quarter1 varchar(100) ='Quarter1' -- Quarter1
		Declare @Quarter2 varchar(100) ='Quarter2' -- Quarter2
		Declare @Quarter3 varchar(100) ='Quarter3' -- Quarter3
		Declare @Quarter4 varchar(100) ='Quarter4' -- Quarter4
		-- End: declare timeframe related variables

	END
	
	IF(@timeframe = @ThisYear)	-- This Year
	BEGIN
		SELECT * FROM [dbo].GetFinanceThisYearData(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
	END
	IF(@timeframe = @ThisQuarters)	-- This Year (Quarterly)
	BEGIN
		SELECT * FROM [dbo].GetFinanceQuarterlyData(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
	END
	ELSE IF(@timeframe = @ThisMonthly)	-- This Year (Monthly)
	BEGIN
		SELECT * FROM [dbo].GetFinanceMonthlyData(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
	END
	ELSE IF(@timeframe = @Quarter1)	-- Quarter1
	BEGIN
		SELECT * FROM [dbo].GetFinanceQuarter1Data(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
	END
	ELSE IF(@timeframe = @Quarter2)	-- Quarter2
	BEGIN
		SELECT * FROM [dbo].GetFinanceQuarter2Data(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
		
	END
	ELSE IF(@timeframe = @Quarter3)	-- Quarter3
	BEGIN
		SELECT * FROM [dbo].GetFinanceQuarter3Data(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
	END
	ELSE IF(@timeframe = @Quarter4)	-- Quarter 4
	BEGIN
		SELECT * FROM [dbo].GetFinanceQuarter4Data(@CurrencyRate,@BudgetId,@lstUserIds,@UserId)
	END

	-- Get custom columns data
	 EXEC [dbo].[GetFinanceCustomfieldColumnsData] @BudgetId, @ClientId

END
