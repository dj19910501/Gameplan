SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[TransactionLineItemDirectAttribution]'))
	DROP PROCEDURE [INT].[TransactionLineItemDirectAttribution]
GO

CREATE PROCEDURE [INT].[TransactionLineItemDirectAttribution] (@ClientID INT, @UserId INT, @LastDate DATETIME)
AS
BEGIN
	--Direct attribution by line item ID

	--Make sure we have the correct line item IDs or else we will make them with error 
	UPDATE TX
	SET TX.Error = 'Invalid Line Item ID: ' + STR(TX.LineItemId), 
		TX.LineItemId = NULL
	FROM dbo.Transactions TX 
		LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = TX.LineItemId
	WHERE L.PlanLineItemId IS NULL 
		AND TX.ClientID = @ClientID
		AND Tx.DateCreated > @LastDate

	--First let's handle update   
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET		  Value = A.TotalAmount
			, CreatedDate = GETDATE()
			, CreatedBy = @UserID
	FROM ( --must consider the possibilities that multiple transactions may be mapped to a line item
			SELECT A.PlanLineItemId, A.Period, SUM(TX.Amount) AS TotalAmount
			FROM dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A
				JOIN LineItemDetail D ON D.PlanLineItemId = A.PlanLineItemId
				JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = D.PlanTacticID
				JOIN Transactions TX ON TX.ClientId = D.ClientID AND TX.LineItemId = D.PlanLineItemID
			WHERE A.Period = [INT].Period(T.StartDate, TX.AccountingDate)
				AND TX.DateCreated > @LastDate
				AND TX.ClientID = @ClientID
			GROUP BY A.PlanLineItemId, A.Period
		) A
	WHERE Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = A.PlanLineItemId
		AND Plan_Campaign_Program_Tactic_LineItem_Actual.Period = A.Period 

	--Also need to consider that the above processed line items may have partial attributions from client users (manually)
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET Value = Value + A.ToptalAmount --accumulative!!! 
	FROM (
			SELECT  D.PlanLineItemId, 
					[INT].Period(T.StartDate, TX.AccountingDate) AS Period, 
					SUM(M.Amount) AS ToptalAmount
			FROM TransactionLineItemMapping M 
					JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
					JOIN LineItemDetail D ON D.PlanLineItemId = M.LineItemId 
							AND D.ClientId = TX.ClientID --client ID validation
					JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = D.PlanLineItemId
			WHERE TX.ClientID = @ClientId
					AND TX.DateCreated > @LastDate
			GROUP BY D.PlanLineItemId,  
				[INT].Period(T.StartDate, TX.AccountingDate)
		) A
	WHERE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual].PlanLineItemId = A.PlanLineItemId
		AND [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual].Period = A.Period

	--handle the inserts 
	INSERT INTO dbo.Plan_Campaign_Program_Tactic_LineItem_Actual
			( PlanLineItemId ,
			  Period ,
			  Value ,
			  CreatedDate ,
			  CreatedBy
			)
	SELECT	D.PlanLineItemID,
			D.Period,
			D.TotalAmount,
			GETDATE(),
			@UserId 
	FROM (
		SELECT  D.PlanLineItemId,
				[INT].Period(T.StartDate, TX.AccountingDate) AS Period,
				SUM(TX.Amount) AS TotalAmount 
		FROM LineItemDetail D
				JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = D.PlanTacticID
				JOIN	Transactions TX ON TX.ClientId = D.ClientID 
					AND TX.LineItemId = D.PlanLineItemID
				LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = D.PlanLineItemId
					AND A.Period = [INT].Period(T.StartDate, TX.AccountingDate)
		WHERE A.PlanLineItemId IS NULL
				AND TX.DateCreated > @LastDate
		GROUP BY D.PlanLineItemId
				, [INT].Period(T.StartDate, TX.AccountingDate) 
		) D

	--Update the transactions as processed
	UPDATE Transactions 
	SET   LastProcessed = GETDATE (),
		  AmountAttributed = Amount
	WHERE DateCreated > @LastDate 
		   AND Transactions.LineItemId IN (
											SELECT PlanLineItemId 
											FROM LineItemDetail 
											WHERE ClientId = @ClientId )
END
GO
