/****** Object:  UserDefinedFunction [dbo].[GetFinanceQuarter4Data]    Script Date: 12/10/2016 4:29:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFinanceQuarter4Data]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetFinanceQuarter4Data]
GO
/****** Object:  UserDefinedFunction [dbo].[GetFinanceQuarter4Data]    Script Date: 12/10/2016 4:29:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFinanceQuarter4Data]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Viral
-- Create date: 12/09/2016
-- Description:	Get Finance data for Timeframe "Quarter4"
-- =============================================
CREATE FUNCTION [dbo].[GetFinanceQuarter4Data]
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
		SELECT 
				Distinct
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 1
				,
				CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y10_Budget,0) * @CurrencyRate)
				END AS Y10_Budget
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y10_Forecast,0) * @CurrencyRate)
				END AS Y10_Forecast
				,
				CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y10_Planned,0) * @CurrencyRate)
				END AS Y10_Planned
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y10_Actual,0) * @CurrencyRate)
				END AS Y10_Actual
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y11_Budget,0) * @CurrencyRate)
				END AS Y11_Budget
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y11_Forecast,0) * @CurrencyRate)
				END AS Y11_Forecast
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y11_Planned,0) * @CurrencyRate)
				END AS Y11_Planned
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y11_Actual,0) * @CurrencyRate)
				END AS Y11_Actual
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y12_Budget,0) * @CurrencyRate)
				END AS Y12_Budget
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y12_Forecast,0) * @CurrencyRate)
				END AS Y12_Forecast
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y12_Planned,0) * @CurrencyRate)
				END AS Y12_Planned
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(Y12_Actual,0) * @CurrencyRate)
				END AS Y12_Actual
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(F.TotalBudget,0) * @CurrencyRate)
				END AS Total_Budget
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(IsNULL(F.TotalForecast,0) * @CurrencyRate)
				END AS Total_Forecast
				,CASE 
				-- Check Null Planned for parent records only.
				WHEN ( BD2.Id IS NOT NULL) THEN NULL
				ELSE
					IsNull(F.TotalPlanned,0) * @CurrencyRate 
				END as ''Total_Planned''

				-- Total Actual
				-- Select ''Null'' actual value in case for ParentID records while all monthly allocated actuals are null.
				,CASE 
					WHEN ( BD2.Id IS NOT NULL)	THEN NULL-- Check Null actuals for parent records only.
					ELSE
						(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
						ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
						ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
						ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate
				END as ''Total_Actual''
				,P.[Users]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
		LEFT JOIN Budget_Detail(NOLOCK) BD2 on F.BudgetDetailId=BD2.ParentId and BD2.IsDeleted=0
)
' 
END

GO
