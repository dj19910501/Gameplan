-- DROP AND CREATE STORED PROCEDURE [MV].[GetFinanceGridData]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[GetFinanceGridData]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[GetFinanceGridData]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to fetch finance grid data
-- =============================================
CREATE PROCEDURE [MV].[GetFinanceGridData]
	@BudgetId		INT,
	@ClientId		INT,
	@timeframe		VARCHAR(50),
	@lstUserIds		NVARCHAR(MAX),
	@UserId			INT,
	@CurrencyRate	FLOAT
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
		SELECT 
			F.Permission	
			,F.BudgetDetailId
			,F.ParentId
			,F.Name
			,F.TotalBudget * @CurrencyRate as Budget
			,F.TotalForecast * @CurrencyRate as Forecast
			,F.TotalPlanned * @CurrencyRate as Planned
			,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate  as Actual
			,F.[User]
			,F.LineItems
			,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	IF(@timeframe = @ThisQuarters)	-- This Year (Quarterly)
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals for Quarter 1
				,(ISNULL(Y1_Budget,0)+ISNULL(Y2_Budget,0)+ISNULL(Y3_Budget,0)) * @CurrencyRate as Q1_Budget
				,(ISNULL(Y1_Forecast,0)+ISNULL(Y2_Forecast,0)+ISNULL(Y3_Forecast,0)) * @CurrencyRate as Q1_Forecast
				,(ISNULL(Y1_Planned,0)+ISNULL(Y2_Planned,0)+ISNULL(Y3_Planned,0)) * @CurrencyRate as Q1_Planned
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0)) * @CurrencyRate as Q1_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 2
				,(ISNULL(Y4_Budget,0)+ISNULL(Y5_Budget,0)+ISNULL(Y6_Budget,0)) * @CurrencyRate as Q2_Budget
				,(ISNULL(Y4_Forecast,0)+ISNULL(Y5_Forecast,0)+ISNULL(Y6_Forecast,0)) * @CurrencyRate as Q2_Forecast
				,(ISNULL(Y4_Planned,0)+ISNULL(Y5_Planned,0)+ISNULL(Y6_Planned,0)) * @CurrencyRate as Q2_Planned
				,(ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0)) * @CurrencyRate as Q2_Actual
				
				-- Budget, Forecast, Planned, Actuals for Quarter 3
				,(ISNULL(Y7_Budget,0)+ISNULL(Y8_Budget,0)+ISNULL(Y9_Budget,0)) * @CurrencyRate as Q3_Budget
				,(ISNULL(Y7_Forecast,0)+ISNULL(Y8_Forecast,0)+ISNULL(Y9_Forecast,0)) * @CurrencyRate as Q3_Forecast
				,(ISNULL(Y7_Planned,0)+ISNULL(Y8_Planned,0)+ISNULL(Y9_Planned,0)) * @CurrencyRate as Q3_Planned
				,(ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0)) * @CurrencyRate as Q3_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 4
				,(ISNULL(Y10_Budget,0)+ISNULL(Y11_Budget,0)+ISNULL(Y12_Budget,0)) * @CurrencyRate as Q4_Budget
				,(ISNULL(Y10_Forecast,0)+ISNULL(Y11_Forecast,0)+ISNULL(Y12_Forecast,0)) * @CurrencyRate as Q4_Forecast
				,(ISNULL(Y10_Planned,0)+ISNULL(Y11_Planned,0)+ISNULL(Y12_Planned,0)) * @CurrencyRate as Q4_Planned
				,(ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as Q4_Actual

				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'

				,F.[User]
				,F.LineItems
				,F.[Owner]

		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @ThisMonthly)	-- This Year (Monthly)
	BEGIN
		SELECT 
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals month wise columns 
				,(Y1_Budget * @CurrencyRate  ) AS Y1_Budget,  (Y1_Forecast * @CurrencyRate ) AS Y1_Forecast
				,(Y1_Planned * @CurrencyRate ) AS Y1_Planned, (Y1_Actual * @CurrencyRate   ) AS Y1_Actual 
				,(Y2_Budget * @CurrencyRate  ) AS Y2_Budget,  (Y2_Forecast * @CurrencyRate ) AS Y2_Forecast 
				,(Y2_Planned * @CurrencyRate ) AS Y2_Planned, (Y2_Actual * @CurrencyRate   ) AS Y2_Actual
				,(Y3_Budget * @CurrencyRate  ) AS Y3_Budget,  (Y3_Forecast * @CurrencyRate ) AS Y3_Forecast
				,(Y3_Planned * @CurrencyRate ) AS Y3_Planned, (Y3_Actual * @CurrencyRate   ) AS Y3_Actual
				,(Y4_Budget * @CurrencyRate  ) AS Y4_Budget,  (Y4_Forecast * @CurrencyRate ) AS Y4_Forecast 
				,(Y4_Planned * @CurrencyRate ) AS Y4_Planned, (Y4_Actual * @CurrencyRate   ) AS Y4_Actual 
				,(Y5_Budget * @CurrencyRate  ) AS Y5_Budget,  (Y5_Forecast * @CurrencyRate ) AS Y5_Forecast 
				,(Y5_Planned * @CurrencyRate ) AS Y5_Planned, (Y5_Actual * @CurrencyRate   ) AS Y5_Actual 
				,(Y6_Budget * @CurrencyRate  ) AS Y6_Budget,  (Y6_Forecast * @CurrencyRate ) AS Y6_Forecast 
				,(Y6_Planned * @CurrencyRate ) AS Y6_Planned, (Y6_Actual * @CurrencyRate   ) AS Y6_Actual 
				,(Y7_Budget * @CurrencyRate  ) AS Y7_Budget,  (Y7_Forecast * @CurrencyRate ) AS Y7_Forecast 
				,(Y7_Planned * @CurrencyRate ) AS Y7_Planned, (Y7_Actual * @CurrencyRate   ) AS Y7_Actual 
				,(Y8_Budget * @CurrencyRate  ) AS Y8_Budget,  (Y8_Forecast * @CurrencyRate ) AS Y8_Forecast 
				,(Y8_Planned * @CurrencyRate ) AS Y8_Planned, (Y8_Actual * @CurrencyRate   ) AS Y8_Actual 
				,(Y9_Budget * @CurrencyRate  ) AS Y9_Budget,  (Y9_Forecast * @CurrencyRate ) AS Y9_Forecast 
				,(Y9_Planned * @CurrencyRate ) AS Y9_Planned, (Y9_Actual * @CurrencyRate   ) AS Y9_Actual 
				,(Y10_Budget * @CurrencyRate ) AS Y10_Budget, (Y10_Forecast * @CurrencyRate) AS  Y10_Forecast 
				,(Y10_Planned * @CurrencyRate) AS Y10_Planned,(Y10_Actual * @CurrencyRate  ) AS Y10_Actual 
				,(Y11_Budget * @CurrencyRate ) AS Y11_Budget, (Y11_Forecast * @CurrencyRate) AS  Y11_Forecast 
				,(Y11_Planned * @CurrencyRate) AS Y11_Planned,(Y11_Actual * @CurrencyRate  ) AS Y11_Actual 
				,(Y12_Budget * @CurrencyRate ) AS Y12_Budget, (Y12_Forecast * @CurrencyRate) AS  Y12_Forecast 
				,(Y12_Planned * @CurrencyRate) AS Y12_Planned,(Y12_Actual * @CurrencyRate  ) AS Y12_Actual 

				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'

				,F.[User]
				,F.LineItems
				,F.[Owner]

		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter1)	-- Quarter1
	BEGIN
		SELECT 
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 1
				,(Y1_Budget * @CurrencyRate) AS Y1_Budget,(Y1_Forecast * @CurrencyRate) AS Y1_Forecast,(Y1_Planned * @CurrencyRate) AS Y1_Planned,(Y1_Actual * @CurrencyRate) AS Y1_Actual 
				,(Y2_Budget * @CurrencyRate) AS Y2_Budget,(Y2_Forecast * @CurrencyRate) AS Y2_Forecast,(Y2_Planned * @CurrencyRate) AS Y2_Planned,(Y2_Actual * @CurrencyRate) AS Y2_Actual 
				,(Y3_Budget * @CurrencyRate) AS Y3_Budget,(Y3_Forecast * @CurrencyRate) AS Y3_Forecast,(Y3_Planned * @CurrencyRate) AS Y3_Planned,(Y3_Actual * @CurrencyRate) AS Y3_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter2)	-- Quarter2
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 2
				,(Y4_Budget * @CurrencyRate) AS Y4_Budget,(Y4_Forecast * @CurrencyRate) AS Y4_Forecast,(Y4_Planned * @CurrencyRate) AS Y4_Planned,(Y4_Actual * @CurrencyRate) AS Y4_Actual
				,(Y5_Budget * @CurrencyRate) AS Y5_Budget,(Y5_Forecast * @CurrencyRate) AS Y5_Forecast,(Y5_Planned * @CurrencyRate) AS Y5_Planned,(Y5_Actual * @CurrencyRate) AS Y5_Actual
				,(Y6_Budget * @CurrencyRate) AS Y6_Budget,(Y6_Forecast * @CurrencyRate) AS Y6_Forecast,(Y6_Planned * @CurrencyRate) AS Y6_Planned,(Y6_Actual * @CurrencyRate) AS Y6_Actual
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter3)	-- Quarter3
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name
				-- Budget, Forecast, Planned, Actuals columns for Quarter 3
				,(Y7_Budget * @CurrencyRate) AS Y7_Budget ,(Y7_Forecast * @CurrencyRate) AS Y7_Forecast ,(Y7_Planned * @CurrencyRate) AS Y7_Planned ,(Y7_Actual * @CurrencyRate) AS Y7_Actual 
				,(Y8_Budget * @CurrencyRate) AS Y8_Budget ,(Y8_Forecast * @CurrencyRate) AS Y8_Forecast ,(Y8_Planned * @CurrencyRate) AS Y8_Planned ,(Y8_Actual * @CurrencyRate) AS Y8_Actual 
				,(Y9_Budget * @CurrencyRate) AS Y9_Budget ,(Y9_Forecast * @CurrencyRate) AS Y9_Forecast ,(Y9_Planned * @CurrencyRate) AS Y9_Planned ,(Y9_Actual * @CurrencyRate) AS Y9_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter4)	-- Quarter 4
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name
				-- Budget, Forecast, Planned, Actuals columns for Quarter 4
				,(Y10_Budget * @CurrencyRate) AS Y10_Budget ,(Y10_Forecast * @CurrencyRate) AS Y10_Forecast ,(Y10_Planned * @CurrencyRate) AS Y10_Planned ,(Y10_Actual * @CurrencyRate) AS Y10_Actual 
				,(Y11_Budget * @CurrencyRate) AS Y11_Budget ,(Y11_Forecast * @CurrencyRate) AS Y11_Forecast ,(Y11_Planned * @CurrencyRate) AS Y11_Planned ,(Y11_Actual * @CurrencyRate) AS Y11_Actual 
				,(Y12_Budget * @CurrencyRate) AS Y12_Budget ,(Y12_Forecast * @CurrencyRate) AS Y12_Forecast ,(Y12_Planned * @CurrencyRate) AS Y12_Planned ,(Y12_Actual * @CurrencyRate) AS Y12_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END

	-- Get custom columns data
	 EXEC [dbo].[GetFinanceCustomfieldColumnsData] @BudgetId, @ClientId


END
GO