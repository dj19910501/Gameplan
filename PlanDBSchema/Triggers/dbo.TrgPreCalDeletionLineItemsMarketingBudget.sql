
-- =============================================
-- Author:		Rahul Shah
-- Create date: 12/12/2016
-- Description:	Update line items count into pre calculation table in case of line item deleted
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalDeletionLineItemsMarketingBudget]
   ON  [dbo].Plan_Campaign_Program_Tactic_LineItem
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	IF((SELECT COUNT(*) FROM INSERTED) > 0)	
	BEGIN	
		IF UPDATE(IsDeleted) --trigger fire only if isdeleted column is updated
		BEGIN
			-- Update line items count into pre calculated table in case of Line Item DELETION
			UPDATE PreCal SET LineItems = ISNULL(LineItemCount,0)
			FROM [MV].[PreCalculatedMarketingBudget] PreCal
			LEFT JOIN
			(
				-- Get count of associated all line items to the budget with IsDeleted flag
				SELECT LB.BudgetDetailId,COUNT(LB.PlanLineItemId) AS LineItemCount 
				FROM LineItem_Budget LB 
				INNER JOIN INSERTED I ON I.PlanLineItemId = LB.PlanLineItemId
				WHERE I.IsDeleted = 0
				GROUP BY LB.BudgetDetailId
			) TblLineItems ON PreCal.BudgetDetailId = TblLineItems.BudgetDetailId
			
		END
	END

END
