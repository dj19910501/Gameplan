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
	END
	ELSE
	BEGIN
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
	END

END
GO
