IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalLineItemsMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalLineItemsMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 12/06/2016
-- Description:	Update line items count into pre calculation table 
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalLineItemsMarketingBudget]
   ON  [dbo].[LineItem_Budget]
   AFTER INSERT,UPDATE,DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @oldBudgetDetailId int 
	Declare @newBudgetDetailId int 
	Declare @oldWeightage float  
	Declare @newWeightage float  
	Declare @actualValue float 
	Declare @period varchar(10) 
	Declare @PlanLineItemId int

	IF((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN

		-- Update line items count into pre calculated table in case of INSERT/UPDATE 
		UPDATE PreCal SET LineItems = ISNULL(LineItemCount,0)
		FROM [MV].[PreCalculatedMarketingBudget] PreCal
		INNER JOIN INSERTED I ON PreCal.BudgetDetailId = I.BudgetDetailId
		LEFT JOIN
		(
			-- Get count of associated all line items to the budget with IsDeleted flag
			SELECT LB.BudgetDetailId,COUNT(LB.PlanLineItemId) AS LineItemCount 
			FROM LineItem_Budget LB 
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem PL ON LB.PlanLineItemId = PL.PlanLineItemId AND PL.IsDeleted=0
			GROUP BY LB.BudgetDetailId
		) TblLineItems ON PreCal.BudgetDetailId = TblLineItems.BudgetDetailId


		-- Get values which are inserted/updated
		SELECT @newBudgetDetailId = BudgetDetailId,
				@newWeightage = Weightage,
				@PlanLineItemId = PlanLineItemId
		FROM INSERTED 

		-- Update new budgetdetailId related Actual valus into [MV].[PreCalculatedMarketingBudget] table
		IF ((SELECT COUNT(id) FROM [MV].[PreCalculatedMarketingBudget](NOLOCK) WHERE BudgetDetailId = @newBudgetDetailId) > 0)
		BEGIN
			-- Update record into pre-calculated table while LineItem - Budget mapping changed
			EXEC [dbo].UpdatePrecalculationActuals @newWeightage,@newBudgetDetailId,@PlanLineItemId,1
		END		
		-- Update new budgetdetailId related Planned valus into [MV].[PreCalculatedMarketingBudget] table
		IF ((SELECT COUNT(id) FROM [MV].[PreCalculatedMarketingBudget](NOLOCK) WHERE BudgetDetailId = @newBudgetDetailId) > 0)
		BEGIN
			-- Update record into pre-calculated table while LineItem - Budget mapping changed
			EXEC [dbo].UpdatePrecalculationPlannedValue @newWeightage,@newBudgetDetailId,@PlanLineItemId,1
		END

		-- IF record updated
		IF((SELECT COUNT(*) FROM DELETED) > 0)
		BEGIN
				-- Get old values 
				SELECT @oldBudgetDetailId = BudgetDetailId,
					   @oldWeightage = Weightage
				FROM DELETED 

				-- Update Actuals into [MV].[PreCalculatedMarketingBudget] table
				BEGIN
					-- Update old budgetdetailId related Actual valus into [MV].[PreCalculatedMarketingBudget] table
					IF ((SELECT COUNT(id) FROM [MV].[PreCalculatedMarketingBudget](NOLOCK) WHERE BudgetDetailId = @oldBudgetDetailId) > 0)
					BEGIN
						-- Update record into pre-calculated table while LineItem - Budget mapping changed
						EXEC [dbo].UpdatePrecalculationActuals @oldWeightage,@oldBudgetDetailId,@PlanLineItemId,0
					END
				END

				-- Update Planned into [MV].[PreCalculatedMarketingBudget] table
				BEGIN
					-- Update old budgetdetailId related Planned valus into [MV].[PreCalculatedMarketingBudget] table
					IF ((SELECT COUNT(id) FROM [MV].[PreCalculatedMarketingBudget](NOLOCK) WHERE BudgetDetailId = @oldBudgetDetailId) > 0)
					BEGIN
						-- Update record into pre-calculated table while LineItem - Budget mapping changed
						EXEC [dbo].UpdatePrecalculationPlannedValue @oldWeightage,@oldBudgetDetailId,@PlanLineItemId,1
					END
				END

		END

	END
	ELSE
	BEGIN
		-- Get old values 
		SELECT @oldBudgetDetailId = BudgetDetailId,
				@oldWeightage = Weightage,
				@PlanLineItemId = PlanLineItemId
		FROM DELETED 

		-- Update line items count into pre calculated table in case of DELETE
		UPDATE PreCal SET LineItems = ISNULL(LineItemCount,0)
		FROM [MV].[PreCalculatedMarketingBudget] PreCal
		INNER JOIN DELETED D ON PreCal.BudgetDetailId = D.BudgetDetailId
		LEFT JOIN
		(
			-- Get count of associated all line items to the budget with IsDeleted flag
			SELECT LB.BudgetDetailId,COUNT(LB.PlanLineItemId) AS LineItemCount 
			FROM LineItem_Budget LB 
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem PL ON LB.PlanLineItemId = PL.PlanLineItemId AND PL.IsDeleted=0
			GROUP BY LB.BudgetDetailId
		) TblLineItems ON PreCal.BudgetDetailId = TblLineItems.BudgetDetailId

		-- Update old budgetdetailId related Actual valus into [MV].[PreCalculatedMarketingBudget] table
		IF ((SELECT COUNT(id) FROM [MV].[PreCalculatedMarketingBudget](NOLOCK) WHERE BudgetDetailId = @oldBudgetDetailId) > 0)
		BEGIN
			-- Update record into pre-calculated table while LineItem - Budget mapping changed
			EXEC [dbo].UpdatePrecalculationActuals @oldWeightage,@oldBudgetDetailId,@PlanLineItemId,0
		END

		-- Update old budgetdetailId related Planned valus into [MV].[PreCalculatedMarketingBudget] table
		IF ((SELECT COUNT(id) FROM [MV].[PreCalculatedMarketingBudget](NOLOCK) WHERE BudgetDetailId = @oldBudgetDetailId) > 0)
		BEGIN
			-- Update record into pre-calculated table while LineItem - Budget mapping changed
			EXEC [dbo].UpdatePrecalculationPlannedValue @oldWeightage,@oldBudgetDetailId,@PlanLineItemId,1
		END
	END

END
GO
