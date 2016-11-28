IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalPlannedMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalPlannedMarketingBudget]
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
CREATE TRIGGER [dbo].[TrgPreCalPlannedMarketingBudget]
   ON  [dbo].[Plan_Campaign_Program_Tactic_LineItem_Cost]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @UpdatedColumn VARCHAR(30) = 'Planned',
			@Period VARCHAR(30),
			@NewValue FLOAT,
			@PlanLineItemId INT,
			@DeleteQuery NVARCHAR(MAX)

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @NewValue = Value,
			   @PlanLineItemId = PlanLineItemId FROM INSERTED

		IF EXISTS(SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId)
		BEGIN
			-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
			EXEC [MV].[PreCalPlannedActualForFinanceGrid] @UpdatedColumn, @Period, @NewValue, @PlanLineItemId
		END
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period, @PlanLineItemId = PlanLineItemId FROM DELETED

		IF EXISTS(SELECT COUNT(id) FROM [dbo].[LineItem_Budget] WHERE PlanLineItemId = @PlanLineItemId)
		BEGIN
			-- Delete/Update record into pre-calculated table while Cost entry is deleted
			SET @DeleteQuery = 'UPDATE P SET ' +@Period + '_'+@UpdatedColumn +' = NULL FROM [MV].[PreCalculatedMarketingBudget] P
								INNER JOIN [dbo].[LineItem_Budget] LB ON P.BudgetDetailId = LB.BudgetDetailId 
								WHERE LB.PlanLineItemId = ' +CAST(@PlanLineItemId AS VARCHAR(30)) 
			EXEC (@DeleteQuery)
		END
	END

END
GO
