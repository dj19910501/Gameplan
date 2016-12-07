/****** Object:  StoredProcedure [dbo].[GetLinkedLineItemsForTransaction]    Script Date: 11/20/2016 4:09:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[GetLinkedLineItemsForTransaction]'))
DROP PROCEDURE [dbo].[GetLinkedLineItemsForTransaction];
GO 

CREATE PROCEDURE [dbo].[GetLinkedLineItemsForTransaction](@ClientID INT, @TransactionId INT)
AS 
BEGIN 

	--dataset 1: tactic data in context of a transaction
	SELECT T.PlanTacticId AS TacticId
			, T.Title
			, T.Cost AS PlannedCost
			, SUM(ISNULL(M.Amount, 0.0)) AS TotalLinkedCost --only the portion of the transaction that is linked to this tactic
			, SUM(ISNULL(LA.Value, 0.0)) AS TotalActual

	FROM Plan_Campaign_Program_tactic T
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanTacticId = T.PlanTacticId
		LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId
		JOIN dbo.Transactions TR ON TR.TransactionId = M.TransactionId
	WHERE M.TransactionId = @TransactionId AND TR.ClientID = @ClientId
	GROUP BY T.PlanTacticId, T.Title, T.Cost

	--dataset 2: line items linked to the @transaction
	SELECT    L.PlanTacticId AS TacticId
			, L.PlanLineItemId -- this the prmary key, the rest of non aggregate columns are auxiliary info.
			, L.Title
			, L.Cost AS Cost
            , T.Title AS TacticTitle
			, P.Title AS ProgramTitle
			, C.Title AS CampaignTitle
			, PL.Title AS PlanTitle
            , M.TransactionLineItemMappingId
            , M.TransactionId
            , SUM(M.Amount) AS TotalLinkedCost -- SUM is a no-op as a transaction can only be linked once per line item
			, SUM(ISNULL(LA.Value, 0.0)) AS Actual

	FROM dbo.Plan_Campaign_Program_Tactic_LineItem L
        JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
        JOIN dbo.Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId
        JOIN dbo.Plan_Campaign C ON C.PlanCampaignId = P.PlanCampaignId
        JOIN dbo.[Plan] PL ON PL.PlanId = C.PlanId
		LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId
		JOIN dbo.Transactions TR ON TR.TransactionId = M.TransactionId
	WHERE M.TransactionId = @TransactionId AND TR.ClientID = @ClientId
	GROUP BY L.PlanTacticId, L.PlanLineItemId, L.Title, L.Cost, T.Title, P.Title, C.Title, PL.Title, M.TransactionLineItemMappingId, M.TransactionId

END 
GO


