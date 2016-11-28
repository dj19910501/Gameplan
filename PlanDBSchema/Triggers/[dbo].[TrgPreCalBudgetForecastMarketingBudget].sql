IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalBudgetForecastMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita
-- Create date: 24/11/2016
-- Description:	Trigger which Insert records into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalBudgetForecastMarketingBudget]
   ON  [dbo].[Budget_DetailAmount]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Period VARCHAR(30),
			@BudgetValue FLOAT,
			@ForecastValue FLOAT,
			@BudgetDetailId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX),
			@BudgetColumnName	NVARCHAR(50),
			@ForecastColumnName	NVARCHAR(50)

	SET @BudgetColumnName = @Period+'_Budget'
	SET @ForecastColumnName = @Period+'_Forecast'	

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @BudgetValue = Budget,
			   @ForecastValue = Forecast,
			   @BudgetDetailId = BudgetDetailId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED I
		INNER JOIN Budget_Detail BD ON I.BudgetDetailId = BD.Id

		-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
		EXEC [MV].[PreCalBudgetForecastForFinanceGrid] @BudgetDetailId, @Year, @Period, @BudgetValue, @ForecastValue
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period,
			   @BudgetDetailId = BudgetDetailId,
			   @Year = YEAR(CreatedDate) FROM DELETED D
			   INNER JOIN Budget_Detail BD ON D.BudgetDetailId = BD.Id

		-- Delete/Update record into pre-calculated table while Cost entry is deleted
		SET @DeleteQuery = 'UPDATE P SET ' +@Period + '_'+@BudgetColumnName +' = NULL, 
										 ' +@Period + '_'+@ForecastColumnName +' = NULL  
							FROM [MV].[PreCalculatedMarketingBudget] P
							WHERE P.BudgetDetailId = ' +CAST(@BudgetDetailId AS VARCHAR(30)) +' AND P.Year = ' + CAST(@Year AS VARCHAR(30)) 
		EXEC (@DeleteQuery)
	END

END
GO
