--
-- TEST SCRIPT FOR TRANSACTION ATTRIBUTION
--
-- NOTE: THIS is based on populating data using generated data set. 
--		 One will have to run the test data script prior to running this script! 
--

--TEST DATA SET 

DECLARE @ClientId INT = 30 -- demo client 
DECLARE @PlanId INT = 1270 -- "Enterprise Big Data 2015"
DECLARE @CreatedBy INT = 297 --nlee@hive9.com

--PREPARE TRANSACTION TEST DATA FOR DEMO CLIENT
UPDATE dbo.Transactions 
SET LineItemId = NULL -- no specific line item yet 
WHERE ClientID = @ClientId

DELETE dbo.TransactionLineItemMapping 
WHERE dbo.TransactionLineItemMapping.TransactionId IN (SELECT TransactionId FROM dbo.Transactions WHERE ClientID = @ClientID)

DBCC CHECKIDENT (TransactionLineItemMapping, RESEED, 0)

--PREPARE LINE ITEM ACTUAL TABLES 
DELETE 
FROM dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId IN (
		SELECT I.PlanLineItemId 
		FROM LineItemDetail I  
		WHERE I.PlanId = @PlanId )

--TEST INSERTS 
--This step will even attribute a transaction to multiple line items (N21 mapping)
DECLARE @Increment INT 

SELECT @Increment = (SELECT COUNT(i.PlanLineItemId) FROM LineItemDetail I WHERE @PlanId = I.PlanId)
			/(SELECT COUNT(*) FROM dbo.Transactions WHERE ClientId = @ClientId)

DECLARE @TransactionId INT
DECLARE @Amount FLOAT
DECLARE @Step INT = 0

DECLARE test_cursor CURSOR FOR   
SELECT TransactionId, Amount  
FROM dbo.Transactions  
WHERE ClientID = @ClientId;  
  
OPEN test_cursor  

  
FETCH NEXT FROM test_cursor   
INTO @TransactionId, @Amount

WHILE @@FETCH_STATUS = 0  
BEGIN 
	INSERT INTO dbo.TransactionLineItemMapping
	        ( TransactionId ,
	          LineItemId ,
	          Amount ,
	          DateModified ,
	          ModifiedBy ,
	          DateProcessed
	        )
			SELECT @TransactionId 
					, A.PlanLineItemId
					, @Amount/@Increment
					, GETDATE()
					, @CreatedBy
					, GETDATE()
			FROM dbo.Plan_Campaign_Program_Tactic_LineItem A 
				JOIN LineItemDetail I ON I.PlanLineItemId = A.PlanLineItemId 
			WHERE I.PlanId = @PlanId 
				AND A.LineItemTypeId IS NOT NULL
			ORDER BY A.PlanLineItemId
			OFFSET @Step*@Increment ROWS FETCH NEXT @Increment ROWS ONLY;

			SET @Step = @Step + 1

	FETCH NEXT FROM test_cursor   
		INTO @TransactionId, @Amount  
END   

CLOSE test_cursor;  
DEALLOCATE test_cursor;  

--VALIDATE ATTRIBUTION RERSULTS 

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
SELECT * FROM dbo.Transactions WHERE ClientID = @ClientId