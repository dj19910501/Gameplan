--
-- TEST SCRIPT FOR TRANSACTION ATTRIBUTION - INSERT TEST
--
-- NOTE: THIS is based on populating data using generated data set. 
-- One must run prerequisite steps priro to running this test script 
--

--TEST DATA SET 

DECLARE @ClientId INT = 30 -- demo client 
DECLARE @PlanId INT = 1270 -- "Enterprise Big Data 2015"
DECLARE @CreatedBy INT = 297 --nlee@hive9.com

UPDATE dbo.TransactionLineItemMapping
SET   Amount = 100 --NOTE: attribute fixed amount 100
	, DateModified = GETDATE()
	, DateProcessed = GETDATE()
WHERE TransactionId IN (
		SELECT TOP 6 TransactionId --ONLY the first 6 transactions
		FROM dbo.Transactions
		WHERE ClientId = @ClientId
		ORDER BY TransactionId
	)

--VALIDATE ATTRIBUTION RERSULTS (same as step 2)

--1. Look from transaction point of view 
SELECT Tx.TransactionId
		, Tx.Amount
		, A.AttributedAmount
		, B.LineItemTotalFound
		, B.NumberOfLineItemsAttributedTo
FROM dbo.Transactions TX JOIN ( 
		SELECT TX.TransactionId, SUM(M.Amount) AS AttributedAmount
		FROM dbo.TransactionLineItemMapping M
			JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
		WHERE TX.ClientID = @ClientId
		GROUP BY TX.TransactionId	 ) A ON A.TransactionId = Tx.TransactionId
	JOIN ( SELECT M.TransactionId, SUM(A.Value) AS LineItemTotalFound, COUNT(*) AS NumberOfLineItemsAttributedTo
	       FROM dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A 
				JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = A.PlanLineItemId 
			GROUP BY M.TransactionId
		) B ON B.TransactionId = TX.TransactionId

ORDER BY TX.TransactionId

--2. Look from line item point of view
SELECT  TX.TransactionId
		,TX.Amount AS TransactionAmount
		, D.PlanTacticId
		, A.PlanLineItemId
		, T.StartDate AS TacticStartDate
		, TX.AccountingDate
		, INT.Period(T.StartDate, TX.AccountingDate) AS CalculatedPeriod
		, A.Period
		, A.Value AS AmountAttributedToTheLineItem
FROM dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A
   JOIN LineItemDetail D ON D.PlanLineItemId = A.PlanLineItemId
   JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = D.PlanTacticID
   JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = A.PlanLineItemId
   JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
WHERE D.PlanId = @PlanId
ORDER BY D.PlanTacticId, D.PlanLineItemId

--List all transactions to see if AmountAttributed and LastProcessed set 
SELECT * FROM dbo.Transactions WHERE ClientID = @ClientId ORDER BY LastProcessed DESC