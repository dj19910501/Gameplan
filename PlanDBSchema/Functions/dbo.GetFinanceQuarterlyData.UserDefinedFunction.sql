/****** Object:  UserDefinedFunction [dbo].[GetFinanceQuarterlyData]    Script Date: 12/10/2016 4:29:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFinanceQuarterlyData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetFinanceQuarterlyData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetFinanceQuarterlyData]    Script Date: 12/10/2016 4:29:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFinanceQuarterlyData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Viral
-- Create date: 12/09/2016
-- Description:	Get Finance data for Timeframe "ThisYear(Quarterly)"
-- =============================================
CREATE FUNCTION [dbo].[GetFinanceQuarterlyData] 
(
	-- Add the parameters for the function here
	@CurrencyRate FLOAT, 
	@BudgetId int,
	@lstUserIds		NVARCHAR(MAX) = '''',
	@UserId			INT
)
RETURNS 
 TABLE 

AS
-- Fill the table variable with the rows for your result set
RETURN (
	SELECT		Distinct
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals for Quarter 1
				-- Select ''Null'' Budget, Forecast, Planned, Actuals value in case for ParentID records while all monthly allocated Budget, Forecast, Planned, Actuals are null.
				,CASE 
					-- Check Null Budget for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y1_Budget,0)+ISNULL(Y2_Budget,0)+ISNULL(Y3_Budget,0)) * @CurrencyRate
				 END as Q1_Budget
				 
				 ,CASE 
					-- Check Null Forecast for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y1_Forecast,0)+ISNULL(Y2_Forecast,0)+ISNULL(Y3_Forecast,0)) * @CurrencyRate
				 END as Q1_Forecast
				 
				 ,CASE 
					-- Check Null Planned for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y1_Planned,0)+ISNULL(Y2_Planned,0)+ISNULL(Y3_Planned,0)) * @CurrencyRate
				 END as Q1_Planned
				 
				 ,CASE 
					-- Check Null actuals for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0)) * @CurrencyRate
				 END as Q1_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 2
				-- Select ''Null'' Budget, Forecast, Planned, Actuals value in case for ParentID records while all monthly allocated Budget, Forecast, Planned, Actuals are null.
				,CASE 
					-- Check Null Budget for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y4_Budget,0)+ISNULL(Y5_Budget,0)+ISNULL(Y6_Budget,0)) * @CurrencyRate
				 END as Q2_Budget
				 
				 ,CASE 
					-- Check Null Forecast for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y4_Forecast,0)+ISNULL(Y5_Forecast,0)+ISNULL(Y6_Forecast,0)) * @CurrencyRate
				 END as Q2_Forecast
				 
				 ,CASE 
					-- Check Null Planned for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y4_Planned,0)+ISNULL(Y5_Planned,0)+ISNULL(Y6_Planned,0)) * @CurrencyRate
				 END as Q2_Planned
				 
				 ,CASE 
					-- Check Null actuals for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0)) * @CurrencyRate
				 END as Q2_Actual
				
				-- Budget, Forecast, Planned, Actuals for Quarter 3
				-- Select ''Null'' Budget, Forecast, Planned, Actuals value in case for ParentID records while all monthly allocated Budget, Forecast, Planned, Actuals are null.
				,CASE 
					-- Check Null Budget for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y7_Budget,0)+ISNULL(Y8_Budget,0)+ISNULL(Y9_Budget,0)) * @CurrencyRate
				 END as Q3_Budget
				 
				 ,CASE 
					-- Check Null Forecast for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y7_Forecast,0)+ISNULL(Y8_Forecast,0)+ISNULL(Y9_Forecast,0)) * @CurrencyRate
				 END as Q3_Forecast
				 
				 ,CASE 
					-- Check Null Planned for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y7_Planned,0)+ISNULL(Y8_Planned,0)+ISNULL(Y9_Planned,0)) * @CurrencyRate
				 END as Q3_Planned
				 
				 ,CASE 
					-- Check Null actuals for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0)) * @CurrencyRate
				 END as Q3_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 4
				-- Select ''Null'' Budget, Forecast, Planned, Actuals value in case for ParentID records while all monthly allocated Budget, Forecast, Planned, Actuals are null.
				,CASE 
					-- Check Null Budget for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y10_Budget,0)+ISNULL(Y11_Budget,0)+ISNULL(Y12_Budget,0)) * @CurrencyRate
				 END as Q4_Budget
				 
				 ,CASE 
					-- Check Null Forecast for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y10_Forecast,0)+ISNULL(Y11_Forecast,0)+ISNULL(Y12_Forecast,0)) * @CurrencyRate
				 END as Q4_Forecast
				 
				 ,CASE 
					-- Check Null Planned for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y10_Planned,0)+ISNULL(Y11_Planned,0)+ISNULL(Y12_Planned,0)) * @CurrencyRate
				 END as Q4_Planned
				 
				 ,CASE 
					-- Check Null actuals for parent records only.
					WHEN ( BD2.Id IS NOT NULL )
					THEN NULL
					ELSE
						(ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate
				 END as Q4_Actual

				,F.TotalBudget * @CurrencyRate as ''Total_Budget''
				,F.TotalForecast * @CurrencyRate as ''Total_Forecast''
				,
				CASE 
					-- Check Null Planned for parent records only.
					WHEN ( BD2.Id IS NOT NULL) THEN NULL
					ELSE IsNull(F.TotalPlanned,0) * @CurrencyRate 
				END as ''Total_Planned''
				-- Total Actual
				-- Select ''Null'' actual value in case for ParentID records while all monthly allocated actuals are null.
				,CASE 
					-- Check Null actuals for parent records only.
					WHEN ( BD2.Id IS NOT NULL ) 
					THEN NULL
					ELSE
						(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
						ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
						ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
						ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate
				END as ''Total_Actual''
				,P.Users
				,F.LineItems
				,F.[Owner]
				
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
		LEFT JOIN Budget_Detail(NOLOCK) BD2 on F.BudgetDetailId=BD2.ParentId and BD2.IsDeleted=0
)
' 
END

GO
