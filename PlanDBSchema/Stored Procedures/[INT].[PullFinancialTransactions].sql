/****** Object:  StoredProcedure [INT].[PullFinancialTransactions]    Script Date: 11/28/2016 12:26:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[PullFinancialTransactions]'))
	DROP PROCEDURE [INT].[PullFinancialTransactions]
GO

CREATE PROCEDURE [INT].[PullFinancialTransactions](@DataSource NVARCHAR(255), @ClientID INT, @UserID INT, @IntegrationInstanceID INT)
AS
BEGIN 
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='

		DECLARE @Updated INT;
		DECLARE @Inserted INT;
		DECLARE @Start DATETIME = GETDATE();

		DECLARE @LastDate DATETIME

		SELECT @LastDate = MAX(DateCreated) FROM dbo.Transactions;
		IF (@LastDate IS NULL) SET @LastDate = DATEADD(YEAR,-2,GETDATE()) --goes 2 years back 

		UPDATE dbo.Transactions 
		SET 
			 ClientTransactionID = V.ClientTransactionID 
			,TransactionDescription = V.TransactionDescription
			,Amount = V.Amount 
			,Account = V.Account
			,AccountDescription = V.AccountDescription 
			,SubAccount = V.SubAccount 
			,Department = V.Department 
			,TransactionDate = V.TransactionDate 
			,AccountingDate = V.AccountingDate 
			,Vendor = V.Vendor 
			,PurchaseOrder = V.PurchaseOrder 
			,LineItemId = V.LineItemId 
			,CustomField1 = V.CustomField1 
			,CustomField2 = V.CustomField2 
			,CustomField3 = V.CustomField3 
			,CustomField4 = V.CustomField4 
			,CustomField5 = V.CustomField5 
			,CustomField6 = V.CustomField6
			,DateCreated = V.DateCreated 
	
		FROM '+@DataSource+' V
		WHERE V.DateCreated > @LastDate 
			AND dbo.Transactions.ClientTransactionID = V.ClientTransactionID
			AND dbo.Transactions.ClientId = '+STR(@ClientId)+'

		SET @Updated = @@ROWCOUNT; 

		INSERT INTO dbo.Transactions
				   ( ClientID 
					,ClientTransactionID
					,TransactionDescription
					,Amount
					,Account 
					,AccountDescription 
					,SubAccount 
					,Department 
					,TransactionDate 
					,AccountingDate 
					,Vendor 
					,PurchaseOrder 
					,CustomField1 
					,CustomField2 
					,CustomField3 
					,CustomField4 
					,CustomField5 
					,CustomField6 
					,LineItemId 
					,DateCreated
				)

		SELECT  ' + STR(@ClientID) + ' 
				,V.ClientTransactionID 
				,V.TransactionDescription 
				,V.Amount 
				,V.Account 
				,V.AccountDescription 
				,V.SubAccount 
				,V.Department 
				,V.TransactionDate 
				,V.AccountingDate 
				,V.Vendor 
				,V.PurchaseOrder 
				,V.CustomField1 
				,V.CustomField2 
				,V.CustomField3 
				,V.CustomField4 
				,V.CustomField5 
				,V.CustomField6 
				,V.LineItemId 
				,V.DateCreated
		FROM ' + @DataSource + ' V 
			LEFT JOIN dbo.Transactions T ON T.ClientTransactionID = T.ClientTransactionID
		WHERE  V.DateCreated > @LastDate 
			AND T.ClientTransactionID IS NULL


		SET @Inserted = @@ROWCOUNT;

		--direct line item attribution 
		EXEC [INT].[TransactionLineItemDirectAttribution] ' + STR(@ClientID) +', '+STR(@UserId)+', @LastDate

		IF (@Inserted+@Updated > 0) EXEC [TransactionNotification] '+STR(@ClientID)+', '+STR(@UserID)+', @LastDate, @Inserted, @Updated

		INSERT INTO [dbo].[IntegrationInstanceLog] ( 
			   [IntegrationInstanceID]
			  ,[SyncStart]
			  ,[SyncEnd]
			  ,[Status]
			  ,[ErrorDescription]
			  ,[CreatedDate]
			  ,[CreatedBy]
			  ,[IsAutoSync]) 
		SELECT ' + STR(@IntegrationInstanceID) + '
			, @Start
			, GETDATE()
			, ''SUCCESS'' 
			, ''Pulled Financial Transactions From'' +''' + @DataSource + '''+STR(@Updated) + '' Updated.'' + STR(@Inserted) + '' Inserted.'' 
			, GETDATE()
			, ' + STR(@UserID) + '
			, 1  

	'
		BEGIN TRY 
			--PRINT @CustomQuery;
			EXEC (@CustomQuery)
		END TRY 

		BEGIN CATCH 
			INSERT INTO [dbo].[IntegrationInstanceLog] ( 
				   [IntegrationInstanceID]
				  ,[SyncStart]
				  ,[SyncEnd]
				  ,[Status]
				  ,[ErrorDescription]
				  ,[CreatedDate]
				  ,[CreatedBy]
				  ,[IsAutoSync]) 
			SELECT @IntegrationInstanceID, @Start, GETDATE(), 'ERROR' ,ERROR_MESSAGE(), GETDATE(), @UserID, 1  
		END CATCH 

END 

