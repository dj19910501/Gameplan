/****** Object:  StoredProcedure [dbo].[GetLinkedLineItemsForTransaction]    Script Date: 11/20/2016 4:09:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[GetLinkedLineItemsForTransaction]'))
DROP PROCEDURE [dbo].[GetLinkedLineItemsForTransaction];
GO 

CREATE PROCEDURE [dbo].[GetLinkedLineItemsForTransaction](@TransactionId INT)
AS 
BEGIN 

	--dataset 1: tactic data in context of a transaction
	SELECT T.PlanTacticId AS TacticId
			, T.Title
			, T.Cost AS PlannedCost
			, SUM(ISNULL(M.Amount, 0.0)) AS TotalLinkedCost --only the portion of the transaction that is linked to this tactic 
			, SUM(LA.Value) AS TotalActual 
 
	FROM Plan_Campaign_Program_tactic T 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanTacticId = T.PlanTacticId
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		LEFT JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId 
	WHERE M.TransactionId = @TransactionId
	GROUP BY T.PlanTacticId, T.Title, T.Cost

	--dataset 2: line items linked to the @transaction 
	SELECT    L.PlanTacticId AS TacticId
			, L.PlanLineItemId -- this the prmary key, the rest of non aggregate columns are auxiliary info. 
			, L.Title
			, L.Cost AS Cost
			, SUM(M.Amount) AS TotalLinkedCost -- SUM is a no-op as a transaction can only be linked once per line item 
			, SUM(LA.Value) AS TotalActual 
 
	FROM dbo.Plan_Campaign_Program_Tactic_LineItem L 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId 
	WHERE M.TransactionId = @TransactionId
	GROUP BY L.PlanTacticId, L.PlanLineItemId, L.Title, L.Cost

END 
GO


