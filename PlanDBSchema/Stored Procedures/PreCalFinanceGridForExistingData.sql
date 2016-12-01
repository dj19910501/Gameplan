
/****** Object:  StoredProcedure [dbo].[PreCalFinanceGridForExistingData]    Script Date: 12/01/2016 12:46:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PreCalFinanceGridForExistingData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PreCalFinanceGridForExistingData]
GO

/****** Object:  StoredProcedure [dbo].[PreCalFinanceGridForExistingData]    Script Date: 12/01/2016 12:46:46 PM ******/
SET ANSI_NULLS ON
GO

-- =============================================
-- Author:		Arpita Soni
-- Create date: 28/11/2016
-- Description:	Dump existing budget,forecast,planned,actual data into pre calculated table
-- =============================================
CREATE PROCEDURE [dbo].[PreCalFinanceGridForExistingData]
	@BudgetId int=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    BEGIN
	-- Update Budget values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Budget = [Y1],Y2_Budget = [Y2],
					  Y3_Budget = [Y3],Y4_Budget = [Y4],
					  Y5_Budget = [Y5],Y6_Budget = [Y6],
					  Y7_Budget = [Y7],Y8_Budget = [Y8],
					  Y9_Budget = [Y9],Y10_Budget = [Y10],
					  Y11_Budget = [Y11],Y12_Budget = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly budget amount with pivoting
		SELECT * FROM 
		(
			SELECT B.Id AS BudgetDetailId, Period, Budget 
			FROM Budget A
			INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
			INNER JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
			WHERE A.IsDeleted = 0 and A.Id=@BudgetId
		) P
		PIVOT
		(
			MIN(BUDGET)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END

BEGIN
	-- Insert Budget records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Budget, Y2_Budget, Y3_Budget, Y4_Budget, Y5_Budget, 
													Y6_Budget, Y7_Budget,Y8_Budget, Y9_Budget, Y10_Budget, Y11_Budget, Y12_Budget)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly budget amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, Budget 
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
		WHERE A.IsDeleted = 0 and A.Id=@BudgetId
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

BEGIN
	-- Update Forecast values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Forecast = [Y1],Y2_Forecast = [Y2],
					  Y3_Forecast = [Y3],Y4_Forecast = [Y4],
					  Y5_Forecast = [Y5],Y6_Forecast = [Y6],
					  Y7_Forecast = [Y7],Y8_Forecast = [Y8],
					  Y9_Forecast = [Y9],Y10_Forecast = [Y10],
					  Y11_Forecast = [Y11],Y12_Forecast = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly forecast amount with pivoting
		SELECT * FROM 
		(
			SELECT B.Id AS BudgetDetailId, Period, Forecast FROM Budget A
			INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
			INNER JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
			WHERE A.IsDeleted = 0 and A.Id=@BudgetId
		) P
		PIVOT
		(
			MIN(Forecast)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END

BEGIN
	-- Insert Forecast records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Forecast, Y2_Forecast, Y3_Forecast, Y4_Forecast, Y5_Forecast, 
													Y6_Forecast, Y7_Forecast,Y8_Forecast, Y9_Forecast, Y10_Forecast, Y11_Forecast, Y12_Forecast)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly forecast amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, Budget FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
		WHERE A.IsDeleted = 0 and A.Id=@BudgetId
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

END

GO


