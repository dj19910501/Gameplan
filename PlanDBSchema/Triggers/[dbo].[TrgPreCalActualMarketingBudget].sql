IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalActualMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalActualMarketingBudget]
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
CREATE TRIGGER [dbo].[TrgPreCalActualMarketingBudget]
   ON  [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @UpdatedColumn VARCHAR(30) = 'Actual',
			@Period VARCHAR(30),
			@NewValue FLOAT,
			@OldValue FLOAT,
			@PlanLineItemId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX)

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @NewValue = Value,
			   @PlanLineItemId = PlanLineItemId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED 

		-- Get old value in case of update
		SELECT @OldValue = Value FROM DELETED

		IF ((SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
			EXEC [MV].[PreCalPlannedActualForFinanceGrid] @UpdatedColumn, @Year, @Period, @NewValue,@OldValue, @PlanLineItemId
		END
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period, @PlanLineItemId = PlanLineItemId,@Year = YEAR(CreatedDate) FROM DELETED 

		IF ((SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Delete/Update record into pre-calculated table while Cost entry is deleted
			SET @DeleteQuery = 'UPDATE P SET 
								' +@Period + '_'+@UpdatedColumn +' = (P.' +@Period + '_'+ @UpdatedColumn + ' - '+CAST(@OldValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) + '+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100))
								FROM [MV].[PreCalculatedMarketingBudget] P
								INNER JOIN [dbo].[LineItem_Budget] LB ON P.BudgetDetailId = LB.BudgetDetailId 
								WHERE LB.PlanLineItemId = ' +CAST(@PlanLineItemId AS VARCHAR(30)) + ' AND P.Year = ' + CAST(@Year AS VARCHAR(30))
			EXEC (@DeleteQuery)
		END
	END

END
GO
