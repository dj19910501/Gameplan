/****** Object:  UserDefinedFunction [dbo].[GetFinanceMonthlyData]    Script Date: 12/10/2016 4:29:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFinanceMonthlyData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetFinanceMonthlyData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetFinanceMonthlyData]    Script Date: 12/10/2016 4:29:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFinanceMonthlyData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Viral
-- Create date: 12/09/2016
-- Description:	Get Finance data for Timeframe "ThisYear(Monthly)"
-- =============================================


CREATE FUNCTION [dbo].[GetFinanceMonthlyData] 
(
	-- Add the parameters for the function here
	@CurrencyRate FLOAT, 
	@BudgetId int,
	@lstUserIds		NVARCHAR(MAX) = '''',
	@UserId			INT
)
RETURNS @tblFinanceMonthlyData 
 TABLE (
	Permission	varchar(255),
	BudgetDetailId int,
	ParentId int,
	Name nvarchar(max),
	Y1_Budget float,Y2_Budget float,Y3_Budget float,Y4_Budget float,Y5_Budget float,Y6_Budget float,Y7_Budget float,Y8_Budget float,Y9_Budget float,Y10_Budget float,Y11_Budget float,Y12_Budget float
	,Y1_Forecast float,Y2_Forecast float,Y3_Forecast float,Y4_Forecast float,Y5_Forecast float,Y6_Forecast float,Y7_Forecast float,Y8_Forecast float,Y9_Forecast float,Y10_Forecast float,Y11_Forecast float,Y12_Forecast float
	,Y1_Planned float,Y2_Planned float,Y3_Planned float,Y4_Planned float,Y5_Planned float,Y6_Planned float,Y7_Planned float,Y8_Planned float,Y9_Planned float,Y10_Planned float,Y11_Planned float,Y12_Planned float
	,Y1_Actual float,Y2_Actual float,Y3_Actual float,Y4_Actual float,Y5_Actual float,Y6_Actual float,Y7_Actual float,Y8_Actual float,Y9_Actual float,Y10_Actual float,Y11_Actual float,Y12_Actual float
	,Total_Budget float
	,Total_Forecast float
	,Total_Planned float
	,Total_Actual float
	,[Users] int
	,LineItems int
	,[Owner] int
 )

AS
-- Fill the table variable with the rows for your result set
BEGIN
		Declare @FinanceMonthlyData table(
				Permission	varchar(255),
				BudgetDetailId int,
				ParentId int,
				Name nvarchar(max),
				Y1_Budget float,Y2_Budget float,Y3_Budget float,Y4_Budget float,Y5_Budget float,Y6_Budget float,Y7_Budget float,Y8_Budget float,Y9_Budget float,Y10_Budget float,Y11_Budget float,Y12_Budget float
				,Y1_Forecast float,Y2_Forecast float,Y3_Forecast float,Y4_Forecast float,Y5_Forecast float,Y6_Forecast float,Y7_Forecast float,Y8_Forecast float,Y9_Forecast float,Y10_Forecast float,Y11_Forecast float,Y12_Forecast float
				,Y1_Planned float,Y2_Planned float,Y3_Planned float,Y4_Planned float,Y5_Planned float,Y6_Planned float,Y7_Planned float,Y8_Planned float,Y9_Planned float,Y10_Planned float,Y11_Planned float,Y12_Planned float
				,Y1_Actual float,Y2_Actual float,Y3_Actual float,Y4_Actual float,Y5_Actual float,Y6_Actual float,Y7_Actual float,Y8_Actual float,Y9_Actual float,Y10_Actual float,Y11_Actual float,Y12_Actual float
				,Total_Budget float
				,Total_Forecast float
				,Total_Planned float
				,Total_Actual float
				,[Users] int
				,LineItems int
				,[Owner] int
		)
		
		INSERT INTO @FinanceMonthlyData(
		Permission,BudgetDetailId,ParentId,Name
		,Y1_Budget,Y2_Budget,Y3_Budget,Y4_Budget,Y5_Budget,Y6_Budget,Y7_Budget,Y8_Budget,Y9_Budget,Y10_Budget,Y11_Budget,Y12_Budget
		,Y1_Forecast,Y2_Forecast,Y3_Forecast,Y4_Forecast,Y5_Forecast,Y6_Forecast,Y7_Forecast,Y8_Forecast,Y9_Forecast,Y10_Forecast,Y11_Forecast,Y12_Forecast
		,Y1_Planned,Y2_Planned,Y3_Planned,Y4_Planned,Y5_Planned,Y6_Planned,Y7_Planned,Y8_Planned,Y9_Planned,Y10_Planned,Y11_Planned,Y12_Planned
		,Y1_Actual,Y2_Actual,Y3_Actual,Y4_Actual,Y5_Actual,Y6_Actual,Y7_Actual,Y8_Actual,Y9_Actual,Y10_Actual,Y11_Actual,Y12_Actual
		,Total_Budget 
		,Total_Forecast 
		,Total_Planned 
		,Total_Actual 
		,[Users] 
		,LineItems
		,[Owner] 
		)
		
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
		
						,F.TotalBudget * @CurrencyRate as ''Total_Budget''
						,F.TotalForecast * @CurrencyRate as ''Total_Forecast''
						,F.TotalPlanned * @CurrencyRate as ''Total_Planned''
						,
						((ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
								ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
								ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
								ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate) as ''Total_Actual''
						,P.[Users]
						,P.LineItems
						,F.[Owner]
		
				FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
				INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
		
		
		INSERT INTO @tblFinanceMonthlyData
		-- Insert Child records to Result table
		Select	F.Permission,F.BudgetDetailId,F.ParentId,F.Name
				,IsNull(Y1_Budget,0) Y1_Budget ,IsNull(Y2_Budget,0) Y2_Budget ,IsNull(Y3_Budget,0) Y3_Budget ,IsNull(Y4_Budget,0) Y4_Budget ,IsNull(Y5_Budget,0) Y5_Budget ,IsNull(Y6_Budget,0) Y6_Budget
				,IsNull(Y7_Budget,0) Y7_Budget ,IsNull(Y8_Budget,0) Y8_Budget ,IsNull(Y9_Budget,0) Y9_Budget ,IsNull(Y10_Budget,0) Y10_Budget ,IsNull(Y11_Budget,0) Y11_Budget ,IsNull(Y12_Budget,0) Y12_Budget
				,IsNull(Y1_Forecast,0) Y1_Forecast ,IsNull(Y2_Forecast,0) Y2_Forecast ,IsNull(Y3_Forecast,0) Y3_Forecast ,IsNull(Y4_Forecast,0) Y4_Forecast ,IsNull(Y5_Forecast,0) Y5_Forecast ,IsNull(Y6_Forecast,0) Y6_Forecast 
				,IsNull(Y7_Forecast,0) Y7_Forecast ,IsNull(Y8_Forecast,0) Y8_Forecast ,IsNull(Y9_Forecast,0) Y9_Forecast,IsNull(Y10_Forecast,0) Y10_Forecast,IsNull(Y11_Forecast,0) Y11_Forecast ,IsNull(Y12_Forecast,0) Y12_Forecast
				,IsNull(Y1_Planned,0) Y1_Planned ,IsNull(Y2_Planned,0) Y2_Planned ,IsNull(Y3_Planned,0) Y3_Planned ,IsNull(Y4_Planned,0) Y4_Planned ,IsNull(Y5_Planned,0) Y5_Planned ,IsNull(Y6_Planned,0) Y6_Planned 
				,IsNull(Y7_Planned,0) Y7_Planned ,IsNull(Y8_Planned,0) Y8_Planned ,IsNull(Y9_Planned,0) Y9_Planned ,IsNull(Y10_Planned,0) Y10_Planned,IsNull(Y11_Planned,0) Y11_Planned ,IsNull(Y12_Planned,0) Y12_Planned
				,IsNull(Y1_Actual,0) Y1_Actual ,IsNull(Y2_Actual,0) Y2_Actual ,IsNull(Y3_Actual,0) Y3_Actual ,IsNull(Y4_Actual,0) Y4_Actual ,IsNull(Y5_Actual,0) Y5_Actual ,IsNull(Y6_Actual,0) Y6_Actual 
				,IsNull(Y7_Actual,0) Y7_Actual ,IsNull(Y8_Actual,0) Y8_Actual ,IsNull(Y9_Actual,0) Y9_Actual ,IsNull(Y10_Actual,0) Y10_Actual ,IsNull(Y11_Actual,0) Y11_Actual ,IsNull(Y12_Actual,0) Y12_Actual
				,IsNull(Total_Budget,0) Total_Budget ,IsNull(Total_Forecast,0) Total_Forecast ,IsNull(Total_Planned,0) Total_Planned ,IsNull(Total_Actual,0) as Total_Actual ,[Users] ,LineItems,[Owner] 
		
		FROM @FinanceMonthlyData F
		LEFT JOIN Budget_Detail(NOLOCK) BD2 on F.BudgetDetailId=BD2.ParentId and BD2.IsDeleted=0
		WHERE BD2.Id is null 
		
		UNION 
		
		 --Insert Parent records to Result table
		Select F.Permission,F.BudgetDetailId,F.ParentId,F.Name
				,NUll Y1_Budget, NULL Y2_Budget, NULL Y3_Budget, NULL Y4_Budget, NULL Y5_Budget, NULL Y6_Budget, NULL Y7_Budget, NULL Y8_Budget, NULL Y9_Budget, NULL Y10_Budget, NULL Y11_Budget, NULL Y12_Budget
				, NULL Y1_Forecast, NULL Y2_Forecast, NULL Y3_Forecast, NULL Y4_Forecast, NULL Y5_Forecast, NULL Y6_Forecast, NULL Y7_Forecast, NULL Y8_Forecast, NULL Y9_Forecast, NULL Y10_Forecast, NULL Y11_Forecast, NULL Y12_Forecast
				, NULL Y1_Planned, NULL Y2_Planned, NULL Y3_Planned, NULL Y4_Planned, NULL Y5_Planned, NULL Y6_Planned, NULL Y7_Planned, NULL Y8_Planned, NULL Y9_Planned, NULL Y10_Planned, NULL Y11_Planned, NULL Y12_Planned
				, NULL Y1_Actual, NULL Y2_Actual, NULL Y3_Actual, NULL Y4_Actual, NULL Y5_Actual, NULL Y6_Actual, NULL Y7_Actual, NULL Y8_Actual, NULL Y9_Actual, NULL Y10_Actual, NULL Y11_Actual, NULL Y12_Actual
				, NULL Total_Budget , NULL Total_Forecast , NULL Total_Planned , NULL Total_Actual ,[Users] ,LineItems,[Owner] 
		FROM @FinanceMonthlyData F
		INNER JOIN Budget_Detail(NOLOCK) BD2 on F.BudgetDetailId=BD2.ParentId and BD2.IsDeleted=0
		WHERE BD2.ParentId IS NOT NULL
		
		RETURN 
		
END
		
		' 
END

GO
