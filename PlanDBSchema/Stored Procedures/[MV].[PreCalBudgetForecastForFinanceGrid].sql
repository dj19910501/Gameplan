-- DROP AND CREATE STORED PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[PreCalBudgetForecastForFinanceGrid]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to insert/update precalculated data for marketing budget grid
-- =============================================
-- [MV].[PreCalBudgetForecastForFinanceGrid] 755,2016,'Y1',5000,0
CREATE PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
	@BudgetDetailId INT,		-- Id of the Budget_Detail table
	@Year INT,					-- Year 
	@Period VARCHAR(5),			-- Period in case of editing Monthly/Quarterly allocation
	@BudgetValue FLOAT=0,		-- New value for Budget
	@ForecastValue FLOAT= 0		-- New value for Forecast
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @InsertQuery		NVARCHAR(MAX), 
			@BudgetColumnName	NVARCHAR(50),
			@ForecastColumnName	NVARCHAR(50)

	SET @BudgetColumnName = @Period+'_Budget'
	SET @ForecastColumnName = @Period+'_Forecast'
	
	-- Insert Budget/Forecast values in PreCalculatedMarketingBudget table if already exists 
	SET @InsertQuery = ' MERGE INTO [MV].[PreCalculatedMarketingBudget] AS T1
						 USING
						 (
							SELECT ' + CAST(@BudgetDetailId AS VARCHAR(30))+ ' AS BudgetDetailId, 
							' + CAST(@Year AS VARCHAR(30))+ ' AS [Year], 
							'+CAST(@BudgetValue AS VARCHAR(30))+' AS NewBudgetValue,
							'+CAST(@ForecastValue AS VARCHAR(30))+' AS NewForecastValue
						 ) AS T2
						 ON (T2.BudgetDetailId = T1.BudgetDetailId AND T2.Year = T1.Year)
						 WHEN MATCHED THEN
						 UPDATE SET T1.' + @BudgetColumnName + ' = T2.NewBudgetValue,
									T1.' + @ForecastColumnName + ' = T2.NewForecastValue
						 WHEN NOT MATCHED THEN  
						 INSERT (BudgetDetailId, [Year], ' + @BudgetColumnName + ',' + @ForecastColumnName + ')
						 VALUES (BudgetDetailId, [Year], T2.NewBudgetValue,T2.NewForecastValue);'
	
	EXEC (@InsertQuery)
END
GO