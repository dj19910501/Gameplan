--Create ClientId column on User_Notification table 
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'User_Notification' AND COLUMN_NAME = 'ClientId') 
BEGIN 
	ALTER TABLE dbo.User_Notification
	ADD ClientId INT NULL 
END 
GO
IF NOT EXISTS(SELECT 1 FROM dbo.Notification WHERE Title = 'When new transactions arrive that I need to pay attention to')
INSERT INTO dbo.Notification
        ( Title ,
          Description ,
          NotificationType ,
          EmailContent ,
          IsDeleted ,
          CreatedDate ,
          ModifiedDate ,
          NotificationInternalUseOnly ,
          Subject ,
          CreatedBy 
        )
VALUES  ( N'When new transactions arrive that I need to pay attention to' , -- Title - nvarchar(255)
          NULL , -- Description - nvarchar(4000)
          'AM' , -- NotificationType - char(2)
          N'Dear [NameToBeReplaced],<br/><br/> There are new transactions that need your attentions for attribution. <br/><br/>Thank You,<br/>Hive9 Plan Admin' , -- EmailContent - nvarchar(4000)
          0 , -- IsDeleted - bit
          GETDATE() , -- CreatedDate - datetime
          NULL, -- ModifiedDate - datetime
          'NewTransactionsArrived' , -- NotificationInternalUseOnly - varchar(255)
          'Plan: New Transactions To Attribute' , -- Subject - varchar(255)
          0  -- CreatedBy - int
        )
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[TransactionNotification]'))
	DROP PROCEDURE [dbo].[TransactionNotification]
GO

CREATE PROCEDURE [dbo].[TransactionNotification] (@ClientID INT, @UserId INT, @LastDate DATETIME, @NewTransactionCount INT, @UpdatedTrasactionCount INT)
AS
BEGIN

	DECLARE @Message NVARCHAR (1000)
	SET @Message = ''
	IF (@NewTransactionCount > 0)
		SET @Message = @Message + STR(@NewTransactionCount) + ' new transactions imported into plan. '
	IF (@UpdatedTrasactionCount > 0) 
		SET @Message = @Message + STR(@UpdatedTrasactionCount) + ' transactions updated'

	IF (LEN(@Message) > 0)
	INSERT INTO dbo.User_Notification_Messages
			( ComponentName ,
			  ComponentId ,
			  EntityId ,
			  Description ,
			  ActionName ,
			  IsRead ,
			  ReadDate ,
			  CreatedDate ,
			  UserId ,
			  RecipientId ,
			  ClientID
			)
	SELECT  N'Plan' , -- ComponentName - nvarchar(50)
			  0 , -- ComponentId - int
			  0 , -- EntityId - int
			  @Message, -- Description - nvarchar(250)
			  N'FinancialIntegration' , -- ActionName - nvarchar(50)
			  0 , -- IsRead - bit
			  GETDATE() , -- ReadDate - datetime
			  GETDATE() , -- CreatedDate - datetime
			  @UserId , -- UserId - int
			  UN.UserId , -- RecipientId - int
			  @ClientID
	FROM dbo.User_Notification UN JOIN dbo.Notification N ON N.NotificationId = UN.NotificationId
	WHERE UN.ClientId = @ClientID AND N.IsDeleted = 0 AND N.NotificationInternalUseOnly = 'NewTransactionsArrived'

END
GO

-- START #2802: Added by Viral on 11/30/2016
-- Desc: Adde User & Owner columns for Marketing Budget screen

DECLARE @ClientId INT 	
DECLARE @CreatedBy INT
-- END: Please modify the below variable values as per requirement


DECLARE @CustomeFieldTypeId INT -- fetch CustomeFieldTypeId from CustomFieldType table used for insert in customfield table
DECLARE @ColumnSetId INT   -- fetch id from Budget_ColumnSet table used for insert in Budget_Columns table
DECLARE @UserCustomFieldId INT --fetch inserted CustomFieldId in customfield table as UserCustomFieldId for further insert in Budget_Columns table
DECLARE @OwnerCustomFieldId INT --fetch inserted CustomFieldId customfield table as UserCustomFieldId for further insert in Budget_Columns table


  DECLARE db_cursor CURSOR FOR   --create cursor for get all clientid,createdby
			select Distinct ClientId,CreatedBy 
             FROM [dbo].[CustomField] where EntityType = 'Budget' and Name IN ('Budget','Forecast','Planned','Actual') and IsDeleted='0'
			
			OPEN db_cursor   
			FETCH NEXT FROM db_cursor INTO @ClientId,@CreatedBy   
			
			WHILE @@FETCH_STATUS = 0   
			BEGIN 

				SELECT @CustomeFieldTypeId = CustomFieldTypeId  FROM [dbo].CustomFieldType WHERE Name = 'TextBox'  --get client specific CustomFieldTypeId for TextBox to insert CustomFieldTypeId for User,Owner 
				SELECT TOP 1 @ColumnSetId = [Id]  FROM [dbo].[Budget_ColumnSet] where ClientId = @ClientId and IsDeleted='0' and Name ='Finance'  --get client specific ColumnSetId from Budget_ColumnSet
				
				DELETE FROM [dbo].[Budget_Columns] WHERE CustomFieldId IN 
				(SELECT CustomFieldId FROM [dbo].[CustomField] WHERE Name = 'Users' and EntityType = 'Budget' and ClientId = @ClientId and IsDeleted = 0)

				DELETE FROM [dbo].[CustomField_Entity] WHERE CustomFieldId IN 
				(SELECT CustomFieldId FROM [dbo].[CustomField] WHERE Name = 'Users' and EntityType = 'Budget' and ClientId = @ClientId and IsDeleted = 0)

				DELETE FROM [dbo].[CustomField] WHERE Name = 'Users' and EntityType = 'Budget' and ClientId = @ClientId and IsDeleted = 0

				  IF NOT EXISTS(SELECT CustomFieldid FROM [dbo].[CustomField] WHERE Name = 'Users' and EntityType = 'Budget' and ClientId = @ClientId and IsDeleted = 0) -- check client specific User name already exists in CustomField Table
					BEGIN
					   INSERT INTO [dbo].[CustomField]
						([Name] ,[CustomFieldTypeId],[Description] ,[IsRequired] ,[EntityType],[IsDeleted],[CreatedDate] ,[ModifiedDate] ,[IsDisplayForFilter] ,
						[AbbreviationForMulti] ,[IsDefault] ,[IsGet],[ClientId],[CreatedBy],[ModifiedBy]) 

							SELECT 'Users', @CustomeFieldTypeId, null, 0, 'Budget', 0, GETDATE(), null, 0, 'MULTI', 0 , 0, @ClientId, @CreatedBy, 0  --insert new row in CustomField Table for User Name CustomField

							 SELECT @UserCustomFieldId = SCOPE_IDENTITY() --get last inserted CustomFieldId  as UserCustomFieldId for further insert in Budget_Columns table
							
							IF NOT EXISTS(SELECT Id FROM [dbo].[Budget_Columns] WHERE Column_SetId = @ColumnSetId AND IsDeleted = 0 AND CustomFieldId = @UserCustomFieldId ) --check Column_Set specific same customfield for User already exist in Budget_Columns
							BEGIN
							INSERT INTO [dbo].[Budget_Columns]  ([Column_SetId],[CustomFieldId] ,[CreatedDate] ,[IsTimeFrame],[IsDeleted]
							           ,[MapTableName],[ValueOnEditable] ,[ValidationType],[CreatedBy])
							     
								 SELECT  @ColumnSetId , @UserCustomFieldId , GETDATE() , 0, 0,'', 4, 'None', @CreatedBy --insert new row in Budget_Columns table for User Custom Field for Column_Set specific
							END
					END
					
					    IF NOT EXISTS(SELECT CustomFieldid FROM [dbo].[CustomField] WHERE Name = 'Owner' and EntityType = 'Budget' and ClientId = @ClientId and IsDeleted = 0) --check client specific Owner name already exists in CustomField Table
					BEGIN
					   INSERT INTO [dbo].[CustomField]
						([Name] ,[CustomFieldTypeId],[Description] ,[IsRequired] ,[EntityType],[IsDeleted],[CreatedDate] ,[ModifiedDate] ,[IsDisplayForFilter] ,
						[AbbreviationForMulti] ,[IsDefault] ,[IsGet],[ClientId],[CreatedBy],[ModifiedBy]) 

							SELECT 'Owner', @CustomeFieldTypeId, null, 0, 'Budget', 0, GETDATE(), null, 0, 'MULTI', 0, 0, @ClientId, @CreatedBy, 0--insert new row in CustomField Table for Owner Name CustomField

							SELECT @OwnerCustomFieldId = SCOPE_IDENTITY() --get last inserted CustomFieldId as OwnerCustomFieldId  for further insert in Budget_Columns table
							
							IF NOT EXISTS(SELECT Id FROM [dbo].[Budget_Columns] WHERE Column_SetId = @ColumnSetId AND IsDeleted = 0 AND CustomFieldId = @OwnerCustomFieldId )--check Column_Set specific same customfield for Owner already exist in Budget_Columns
							BEGIN
							INSERT INTO [dbo].[Budget_Columns]  ([Column_SetId],[CustomFieldId] ,[CreatedDate] ,[IsTimeFrame],[IsDeleted]
							           ,[MapTableName],[ValueOnEditable] ,[ValidationType],[CreatedBy])
							     
								 SELECT  @ColumnSetId , @OwnerCustomFieldId , GETDATE() , 0, 0,'', 4, 'None', @CreatedBy --insert new row in Budget_Columns table for User Custom Field for Column_Set specific
							END
					END


			FETCH NEXT FROM db_cursor INTO @ClientId,@CreatedBy      
			END   
			
			CLOSE db_cursor   
			DEALLOCATE db_cursor

-- END #2802: Added by Viral on 11/30/2016

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
GO

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

--NOTE: this is a correction of existing function used in integration -zz
/****** Object:  UserDefinedFunction [INT].[Period]    Script Date: 11/23/2016 2:21:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [INT].[Period](@tacticStartDate DATE, @actualDate DATE)
RETURNS VARCHAR(10)
AS 
BEGIN

    RETURN 
	CASE WHEN DATEPART(YEAR, @tacticStartDate)<DATEPART(YEAR, @actualDate)
		 THEN 'Y' + CAST(((DATEPART(YEAR, @actualDate) - DATEPART(YEAR, @tacticStartDate))*12 + DATEPART(MONTH, @actualDate)) AS VARCHAR(10))
		 ELSE 'Y' +  CAST(DATEPART(MONTH, @actualDate) AS VARCHAR(10))
	END
END
GO
/****** Object:  View [dbo].[CampaignDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[CampaignDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[CampaignDetail];
GO

CREATE VIEW [dbo].[CampaignDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, C.Title, C.Description, C.StartDate, C.EndDate, C.CreatedDate, C.ModifiedDate, C.IsDeleted, C.Status, C.IsDeployedToIntegration, C.IntegrationInstanceCampaignId, 
                  C.LastSyncDate, C.CampaignBudget, C.Abbreviation, C.IntegrationWorkFrontProgramID, C.CreatedBy, C.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId

GO
/****** Object:  View [dbo].[LineItemDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[LineItemDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[LineItemDetail];
GO

CREATE VIEW [dbo].[LineItemDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, L.PlanLineItemId, L.PlanTacticId, L.LineItemTypeId, L.Title, L.Description, L.StartDate, L.EndDate, L.Cost, L.IsDeleted, L.CreatedDate, L.ModifiedDate, 
                  L.LinkedLineItemId, L.ModifiedBy, L.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic_LineItem AS L ON L.PlanTacticId = T.PlanTacticId

GO
/****** Object:  View [dbo].[PlanDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[PlanDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[PlanDetail];
GO

CREATE VIEW [dbo].[PlanDetail]
AS
SELECT M.ClientId, P.PlanId, P.ModelId, P.Title, P.Version, P.Description, P.Budget, P.Status, P.IsActive, P.IsDeleted, P.CreatedDate, P.ModifiedDate, P.Year, P.GoalType, P.GoalValue, P.AllocatedBy, P.EloquaFolderPath, 
                  P.DependencyDate, P.CreatedBy, P.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId

GO
/****** Object:  View [dbo].[ProgramDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[ProgramDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[ProgramDetail];
GO

CREATE VIEW [dbo].[ProgramDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, Pr.PlanCampaignId AS Expr1, Pr.Title, Pr.Description, Pr.StartDate, Pr.EndDate, Pr.CreatedDate, Pr.ModifiedDate, Pr.IsDeleted, Pr.Status, 
                  Pr.IsDeployedToIntegration, Pr.IntegrationInstanceProgramId, Pr.LastSyncDate, Pr.ProgramBudget, Pr.Abbreviation, Pr.CreatedBy, Pr.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId

GO
/****** Object:  View [dbo].[TacticDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[TacticDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[TacticDetail];
GO

CREATE VIEW [dbo].[TacticDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, T.PlanTacticId, T.PlanProgramId, T.TacticTypeId, T.Title, T.Description, T.StartDate, T.EndDate, T.Cost, T.TacticBudget, T.Status, T.CreatedDate, T.ModifiedDate, T.IsDeleted, 
                  T.IsDeployedToIntegration, T.IntegrationInstanceTacticId, T.LastSyncDate, T.ProjectedStageValue, T.StageId, T.TacticCustomName, T.IntegrationWorkFrontProjectID, T.IntegrationInstanceEloquaId, T.LinkedTacticId, 
                  T.LinkedPlanId, T.IsSyncSalesForce, T.IsSyncEloqua, T.IsSyncWorkFront, T.IsSyncMarketo, T.IntegrationInstanceMarketoID, T.ModifiedBy, T.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId


GO

/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 10/27/2016 5:00:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 10/27/2016 5:00:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ExportToCSV] AS' 
END
GO
ALTER PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@clientId INT=0
,@HoneyCombids nvarchar(max)=null
,@CurrencyExchangeRate FLOAT=1
AS
BEGIN

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE #tblCustomData

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..#tblHoneyCombIds') IS NOT NULL
	DROP TABLE #tblHoneyCombIds

IF OBJECT_ID('tempdb..#tblPlanHoneyComb') IS NOT NULL
	DROP TABLE #tblPlanHoneyComb

IF OBJECT_ID('tempdb..#tblCampaignHoneyComb') IS NOT NULL
	DROP TABLE #tblCampaignHoneyComb

IF OBJECT_ID('tempdb..#tblProgramHoneyComb') IS NOT NULL
	DROP TABLE #tblProgramHoneyComb

IF OBJECT_ID('tempdb..#tblTacticHoneyComb') IS NOT NULL
	DROP TABLE #tblTacticHoneyComb

IF OBJECT_ID('tempdb.dbo.#EntityValues') IS NOT NULL 
	DROP TABLE #EntityValues 

	SELECT Item into #tblHoneyCombIds From dbo.SplitString(@HoneyCombids,',') 

	SELECT REPLACE(Item,'Plan_','') as Item into #tblPlanHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Plan%'

	SELECT REPLACE(Item,'Campaign_','') as Item into #tblCampaignHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Campaign%'

	SELECT REPLACE(Item,'Program_','') as Item into #tblProgramHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Program%'

	SELECT REPLACE(Item,'Tactic_','') as Item into #tblTacticHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Tactic%'
	


DECLARE @Entityids nvarchar(max)=''
	
SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',0 As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost','--' AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
,'--' as TacticCategory
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' AND IsDeleted=0) [CustomField]
WHERE 
--[Plan].PlanId IN (@PlanId)
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN [Plan].PlanId END) IN (SELECT Item FROm #tblPlanHoneyComb)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField.CustomFieldId,
CONVERT(NVARCHAR(800),CASE CustomFieldType.Name WHEN 'DropDownList' THEN (SELECT Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost','--' AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Campaign.PlanCampaignId END)IN(SELECT item FROM #tblCampaignHoneyComb)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost','--' AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId 
AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Program' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Program.PlanProgramId END)IN(SELECT item From #tblProgramHoneyComb)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',([Tactic].Cost*@CurrencyExchangeRate) As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,TacticType.AssetType as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE 
[Program].PlanProgramId=[Tactic].PlanProgramId 
AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title,AssetType FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',([lineitem].Cost*@CurrencyExchangeRate) As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Lineitem' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
) tblUnion

DECLARE   @ConcatString NVARCHAR(Max)=''

Declare @RowCount int , @Count int=1

SELECT ColName,ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM into #tblColName FROM (SELECT Distinct  ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName

SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';

CREATE TABLE #tblCustomData
(
ROWNUM INT,
Section NVARCHAR(MAX),
[Plan] NVARCHAR(MAX),		
Campaign NVARCHAR(MAX),
Program NVARCHAR(MAX),
Tactic NVARCHAR(MAX),
Lineitem NVARCHAR(MAX),
StartDate NVARCHAR(MAX),
EndDate NVARCHAR(MAX),
PlannedCost FLOAT,
TacticCategory NVARCHAR(MAX),
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy INT,
TargetStageGoal NVARCHAR(MAX),
ModelId INT,
MQL FLOAT,
Revenue FLOAT,
[Owner] NVARCHAR(MAX),
ExternalName NVARCHAR(MAX),
EntityId INT,
EntityType NVARCHAR(MAX),
ParentId INT,
CustomFieldType NVARCHAR(MAX)
)

DECLARE @Colname nvarchar(max)=''
DECLARE @AlterTable nvarchar(max)=''
While @Count<=@RowCount
BEGIN

SELECT @Colname = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ColName,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') FROM #tblColName WHERE ROWNUM=@Count --This is to Special charachter En Dash replace with Hyphen in CustomField Name
SET @AlterTable +=' ALTER TABLE #tblCustomData ADD ['+@Colname+'] NVARCHAR(MAX) ';
SET @ConcatString= @ConcatString +'['+ @Colname +']'+@Delimeter ;


SET @Count=@Count+1;
END


IF @ConcatString=null OR @ConcatString=''
BEGIN
	SET @ConcatString='DummyCol '
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD DummyCol NVARCHAR(MAX) '
END

SELECT @ConcatString=LEFT(@ConcatString, LEN(@ConcatString) - 1)

EXEC(@AlterTable)
DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT *  FROM
    (SELECT     
		ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		TacticCategory,
		Type,
		SFDCId,
		EloquaId,
        CustomFieldEntityId, 
		CustomFieldId,
		CreatedBy,
		TargetStageGoal,
		ModelId,
		MQL,
		Revenue,
		Owner,
		ExternalName,
              EntityId,
			  EntityType,
			  ParentId,
			  ColName,
              CONVERT(NVARCHAR(MAX),Value) AS Value,
			  CustomFieldType
    FROM #tblPivot WITH (NOLOCK))X 
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P  
	'
	
	INSERT INTO #tblCustomData EXEC SP_EXECUTESQL @query	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
		
	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..#tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';	
	select * into #EntityValues from #tblCustomData WHERE 1=0
	-- Replace Stuff Logic
	-- ADD Columns into #EntityValues
	DECLARE @InsertStatement NVARCHAR(MAX)=''
	
	SET @AlterTable=''
	SET @Colname=''
	SET @Count=1
	SET @AlterTable+=' ALTER TABLE #EntityValues ADD Col_RowGroup NVARCHAR(MAX) '
	SET @InsertStatement=' INSERT INTO #EntityValues ('
	While @Count<=@RowCount
	BEGIN
		SELECT @Colname = name FROM #tbldynamicColumns WHERE ROWNUM=@Count
		SET @AlterTable +=' ALTER TABLE #EntityValues ADD [Col_'+(SELECT REPLACE(REPLACE(@Colname,' ','#'),'-','@'))+'] NVARCHAR(MAX) ';
		
		SET @InsertStatement+='['+@Colname+']'+@Delimeter
	SET @Count=@Count+1;
	END
	SET @InsertStatement+='Col_RowGroup) '
	--PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	--PRINT(@InsertStatement)
	EXEC(@InsertStatement)

	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tblEntityColumns from tempdb.sys.columns where object_id = object_id('tempdb..#EntityValues');

	DECLARE @EntityRowCount INT=0
	SELECT  @EntityRowCount = COUNT(*) FROM #tblEntityColumns
	
	DECLARE @MergeData nvarchar(max)=''
	SET @MergeData=''

	-- Declare Dynamic Variables
	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		SET @MergeData+=' DECLARE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	PRINT(@MergeData)
	-- END Dynamic Variables
	
	-- Update #EntityValues Tables row
	DECLARE @UpdateStatement NVARCHAR(MAX)=''
	SET @UpdateStatement=@MergeData+ '	UPDATE #EntityValues SET '
	SET @Count=1;
	SET @Delimeter=',';

	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		IF CHARINDEX('Col_',@Val) > 0
		BEGIN
			IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
			BEGIN
				IF(@Val='Col_PlannedCost')
				BEGIN
					SET @UpdateStatement+='  @Col_' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
													+ ' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
													+ '] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),CAST(['
													+ (SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
													+ '] AS decimal(38,2))) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
													+ '+'';''+ CONVERT(NVARCHAR(MAX),CAST(['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
													+ '] AS decimal(38,2))) END'+@Delimeter
				END
				ELSE 
				BEGIN
					/*Add Condition - If @Val is tactic or startdate or enddate or targetstagegoal or tacticcategory then no need to concat values.*/
					IF (@Val!='Col_Tactic' AND @Val!='Col_StartDate' AND @Val!='Col_EndDate' AND @Val!='Col_TargetStageGoal' AND @Val != 'Col_TacticCategory')
					BEGIN
						SET @UpdateStatement += '  @Col_' + ( SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@') ) 
														  + ' = ['+ (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														  + '] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['
														  + ( SELECT REPLACE(REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
														  + ']) ELSE @Col_' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														  + '+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
														  + ']) END'+@Delimeter
					END
					ELSE
					BEGIN
						SET @UpdateStatement+='  @Col_' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														+ ' = [' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														+ '] = CONVERT(NVARCHAR(MAX),[' + (SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
														+ '])'+@Delimeter
					END
				END
			END
		END
		SET @Count=@Count+1;
	END

	EXEC(@UpdateStatement)
	
	-- Select With GroupBy
	SET @Count=1;
	SET @Delimeter=',';
	DECLARE @SelectGroup NVARCHAR(MAX)=''
	DECLARE @ActualColName NVARCHAR(MAX)=''
	SET @SelectGroup=' SELECT EntityId,EntityType'
	While @Count<=@EntityRowCount
	BEGIN
	SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
	IF CHARINDEX('Col_',@Val) > 0
	BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @ActualColName=REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_','');
			IF (@ActualColName!='CustomFieldId' AND @ActualColName!='CustomFieldType' AND (@Val!='Col_EntityId' AND @Val!='Col_EntityType'))
			BEGIN
			IF(@ActualColName ='CustomFieldEntityId' OR @ActualColName='EntityId' OR @ActualColName='EndDate' OR @ActualColName='StartDate' OR @ActualColName='Plan' OR @ActualColName='Campaign' OR @ActualColName='Program' OR @ActualColName='Tactic' OR @ActualColName='LineItem' OR @ActualColName='EntityType' OR @ActualColName='ROWNUM' OR @ActualColName='PlannedCost' OR @ActualColName='Section' OR @ActualColName='Type' OR @ActualColName='EloquaId' OR @ActualColName='SFDCId' OR @ActualColName='ParentId' OR @ActualColName='CreatedBy' OR @ActualColName='TargetStageGoal' OR @ActualColName='ModelId' OR @ActualColName='ExternalName' OR @ActualColName='MQL' OR @ActualColName='Revenue' OR @ActualColName='Owner')
			BEGIN
				IF @ActualColName!='EndDate'
				BEGIN 
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MIN(['+ @Val+']) ';
				END
				ELSE 
				BEGIN
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MAX(['+ @Val+']) ';
				END
			END
			ELSE
			BEGIN
				SET @SelectGroup+=@Delimeter+' ['+@ActualColName+'] = MAX(['+@Val+']) ';
			END
			END
		END
	END
		SET @Count=@Count+1;
	END
	SET @SelectGroup+=' FROM #EntityValues GROUP BY EntityId,EntityType ORDER BY (CASE EntityType WHEN ''Plan'' THEN 1'
	SET @SelectGroup+=' WHEN ''Campaign'' THEN 2'
	SET @SelectGroup+=' WHEN ''Program'' THEN 3'
	SET @SelectGroup+=' WHEN ''Tactic'' THEN 4'
	SET @SelectGroup+=' WHEN ''Lineitem'' THEN 5'
	SET @SelectGroup+=' ELSE 6 END)';
	--PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic','Lineitem')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	WHEN 'Lineitem' THEN 4
	ELSE 5 END )
  END

  ELSE 
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End
END
GO

IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransactionLineItemMapping'))
DROP TABLE [dbo].[TransactionLineItemMapping]

IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Transactions'))
DROP TABLE [dbo].[Transactions]
GO

/****** Object:  Table [dbo].[Transactions]    Script Date: 11/15/2016 6:04:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Transactions](
	[TransactionId] [int] IDENTITY(1,1) NOT NULL,
	[ClientID] [int] NOT NULL,
	[ClientTransactionID] [varchar](150) NOT NULL,
	[TransactionDescription] [varchar](250) NULL,
	[Amount] [numeric](18, 0) NOT NULL,
	[Account] [varchar](150) NULL,
	[AccountDescription] [varchar](150) NULL,
	[SubAccount] [varchar](150) NULL,
	[Department] [varchar](150) NULL,
	[TransactionDate] [datetime] NULL,
	[AccountingDate] [datetime] NOT NULL,
	[Vendor] [varchar](150) NULL,
	[PurchaseOrder] [varchar](150) NULL,
	[CustomField1] [varchar](150) NULL,
	[CustomField2] [varchar](150) NULL,
	[CustomField3] [varchar](150) NULL,
	[CustomField4] [varchar](150) NULL,
	[CustomField5] [varchar](150) NULL,
	[CustomField6] [varchar](150) NULL,
	[Error] [varchar](250) NULL,
	[LineItemId] [int] NULL,
	[DateCreated] [datetime] NOT NULL,
	[AmountAttributed] [float] NULL,
	[LastProcessed] [datetime] NULL,
 CONSTRAINT uc_ClientID_ClientTransactionId UNIQUE (ClientID, ClientTransactionId),
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[TransactionLineItemMapping]    Script Date: 11/15/2016 6:01:21 PM ******/
CREATE TABLE [dbo].[TransactionLineItemMapping](
	[TransactionLineItemMappingId] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[LineItemId] [int] NOT NULL,
	[Amount] [float] NULL,
	[DateModified] [datetime] NOT NULL,
	[ModifiedBy] [int] NOT NULL,
	[DateProcessed] [datetime] NULL,
 CONSTRAINT uc_PersonID UNIQUE (TransactionId,LineItemId),
 CONSTRAINT [PK_TransactionLineItemMapping] PRIMARY KEY CLUSTERED 
(
	[TransactionLineItemMappingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TransactionLineItemMapping]  WITH CHECK ADD  CONSTRAINT [FK_TransactionLineItemMapping_Plan_Campaign_Program_Tactic_LineItem] FOREIGN KEY([LineItemId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])
GO

ALTER TABLE [dbo].[TransactionLineItemMapping] CHECK CONSTRAINT [FK_TransactionLineItemMapping_Plan_Campaign_Program_Tactic_LineItem]
GO

ALTER TABLE [dbo].[TransactionLineItemMapping]  WITH CHECK ADD  CONSTRAINT [FK_TransactionLineItemMapping_Transactions] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[Transactions] ([TransactionId])
GO

ALTER TABLE [dbo].[TransactionLineItemMapping] CHECK CONSTRAINT [FK_TransactionLineItemMapping_Transactions]
GO

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
			, SUM(L.Cost) AS TotalLineItemCost
	FROM Plan_Campaign_Program_tactic T
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanTacticId = T.PlanTacticId
		LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId
		JOIN dbo.Transactions TR ON TR.TransactionId = M.TransactionId
			 AND LA.Period = [INT].Period(T.StartDate, TR.AccountingDate)

	WHERE M.TransactionId = @TransactionId AND TR.ClientID = @ClientId
	GROUP BY T.PlanTacticId, T.Title, T.Cost

	--dataset 2: line items linked to the @transaction

	DECLARE @LineItemDataTable TABLE (
					TransactionLineItemMappingId INT NOT NULL,
					LineItemTotalActual FLOAT NOT NULL 
				)

	INSERT @LineItemDataTable
	        ( TransactionLineItemMappingId ,
	          LineItemTotalActual 
	        )
	SELECT   M.TransactionLineItemMappingId
			, SUM(ISNULL(LA.Value, 0.0)) AS Actual

	FROM	dbo.TransactionLineItemMapping M 
			JOIN dbo.Transactions TR ON TR.TransactionId = M.TransactionId
			JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
			LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
	WHERE M.TransactionId = @TransactionId AND TR.ClientID = @ClientId
	GROUP BY M.TransactionLineItemMappingId, M.Amount


	SELECT    L.PlanTacticId AS TacticId
			, L.PlanLineItemId -- this the primary key, the rest of non aggregate columns are auxiliary info.
			, L.Title
			, L.Cost AS Cost
            , T.Title AS TacticTitle
			, P.Title AS ProgramTitle
			, C.Title AS CampaignTitle
			, PL.Title AS PlanTitle
            , M.TransactionLineItemMappingId
            , M.TransactionId
            , M.Amount AS TotalLinkedCost 
			, LIT.LineItemTotalActual AS Actual

	FROM dbo.Plan_Campaign_Program_Tactic_LineItem L
        JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
        JOIN dbo.Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId
        JOIN dbo.Plan_Campaign C ON C.PlanCampaignId = P.PlanCampaignId
        JOIN dbo.[Plan] PL ON PL.PlanId = C.PlanId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId
		JOIN @LineItemDataTable LIT ON LIT.TransactionLineItemMappingId = M.TransactionLineItemMappingId

END 
GO

/****** Object:  Trigger [dbo].[TransactionCostToLineItemAttribution]    Script Date: 12/8/2016 1:05:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[TransactionCostToLineItemAttribution]'))
DROP PROCEDURE [dbo].[GetLinkedLineItemsForTransaction];
GO 

CREATE TRIGGER [dbo].[TransactionCostToLineItemAttribution] 
   ON  [dbo].[TransactionLineItemMapping] 
   AFTER INSERT,UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	--Handle delete (reall delete on this table)
	DELETE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual
	FROM DELETED D 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = D.LineItemId
		JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId 
		JOIN dbo.Transactions TX ON TX.TransactionId = D.TransactionId
	WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = D.LineItemId 
		AND dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period = INT.Period(T.StartDate, TX.AccountingDate)  

    -- We need set the createdBy and CreatedDate for both inserts and updates. 
	-- NOTE: there is no modified on line item actuals   
    DECLARE @CreatedBy INT
	DECLARE @CreatedDate DATETIME = GETDATE()
	SELECT TOP 1  @CreatedBy = A.ModifiedBy
	FROM (SELECT Inserted.ModifiedBy FROM INSERTED UNION ALL SELECT Deleted.ModifiedBy FROM Deleted) A
	--get created by from deleted is NOT a good solution - zz
	
	---Handle updates 
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET   Value = A.Amount
		, CreatedBy = @CreatedBy
		, CreatedDate =@CreatedDate
	FROM ( 
			SELECT SUM(M.Amount) AS Amount
					, L.PlanLineItemId
					, A.Period
			FROM	dbo.TransactionLineItemMapping M 
					JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
					JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
					JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
					JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = L.PlanLineItemId
						AND A.Period = INT.Period(T.StartDate, TX.AccountingDate) 
	
			WHERE M.LineItemId IN (SELECT M.LineItemId FROM INSERTED)
			GROUP BY L.PlanLineItemId, A.Period
	) A
	WHERE A.PlanLineItemId = dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId
		AND A.Period = dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period

	-- Handle inserts 
	INSERT INTO  dbo.Plan_Campaign_Program_Tactic_LineItem_Actual(Value, PlanLineItemId, Period, CreatedDate, CreatedBy)
	SELECT SUM(M.Amount) AS Amount
			, L.PlanLineItemId
			, INT.Period(T.StartDate, TX.AccountingDate)
			, @CreatedDate
			, @CreatedBy
	FROM	dbo.TransactionLineItemMapping M 
			JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
			JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
			JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
			LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = L.PlanLineItemId 
				AND A.Period = INT.Period(T.StartDate, TX.AccountingDate) 	
	WHERE A.PlanLineItemId IS NULL 
		AND M.LineItemId IN (SELECT M.LineItemId FROM INSERTED)
	GROUP BY L.PlanLineItemId, INT.Period(T.StartDate, TX.AccountingDate)

	--Consider the update for 1 to 1 mapped transactions 
	--NOTE: this step is additive to the above 2 steps 
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET Value = Value + A.Amount
	FROM (
			SELECT SUM(TX.Amount) AS Amount
					, L.PlanLineItemId
					, INT.Period(T.StartDate, TX.AccountingDate) AS Period
			FROM dbo.Transactions TX 
				JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = TX.LineItemId
				JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId 
			WHERE TX.LineItemId IN (SELECT Inserted.LineItemId FROM INSERTED) 
			GROUP BY L.PlanLineItemId, INT.Period(T.StartDate, TX.AccountingDate)
		) A
	WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = A.PlanLineItemId 
		AND dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period = A.Period


	--update transactions table with AmountAttributed and LastProcessed

	--First set the value to 0 in case the delete is on the last line item
	UPDATE dbo.Transactions
	SET AmountAttributed = 0, 
		LastProcessed = GETDATE()
	WHERE TransactionId IN (SELECT TransactionID FROM Deleted) 

	--Update with correct logic 
	UPDATE dbo.Transactions
	SET AmountAttributed = A.TotalAttributed, 
		LastProcessed = GETDATE()
	FROM (	
			SELECT SUM(ISNULL(A.Value, 0)) AS TotalAttributed, TX.TransactionId 
			FROM dbo.Transactions TX 
				JOIN dbo.TransactionLineItemMapping M ON M.TransactionId = TX.TransactionId
				JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId 
				JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
				LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = M.LineItemId AND A.Period = [INT].Period(T.StartDate, TX.AccountingDate)
			WHERE TX.TransactionId IN (SELECT TransactionID FROM Deleted UNION ALL SELECT TransactionId FROM INSERTED)
			GROUP BY TX.TransactionId
	) A
	WHERE dbo.Transactions.TransactionId = A.TransactionId

END
GO
-- =============================================
-- Author: Rahul Shah 
-- Create date: 11/21/2016
-- Description:	to Add TotalBudget & TotalForcast in Budget_Detail table to calculate unallocated cost.
-- =============================================
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalBudget')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	ADD [TotalBudget] float null 
   
END
GO
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalForcast')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	DROP Column [TotalForcast] 
   
END
GO
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalForecast')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	ADD [TotalForecast] float null
   
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Team]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[Plan_Team]
END
GO
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanCostDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanCostDataMonthly] 
END
GO
CREATE PROCEDURE [dbo].[ImportPlanCostDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, value as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Cost as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId 
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

--end tactic
UNION
--start line item
select ActivityId,[Task Name],'LineItem' as ActivityType,0 as Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_tactic_Lineitem_Cost as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData

select * into #temp2 from (select * from @ImportData )   k
--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType 
from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE])

 TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData



IF ( LOWER(@Type)='tactic')
	
	BEGIN
    
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId )
			BEGIN

			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId

			--update balance lineitem
			--update balance lineitem Cost of tactic -(sum of line item)
			
			IF((SELECT Top 1  ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=@EntityId and LineItemTypeId IS null and isdeleted=0) = 0)
				BEGIN
					  UPDATE Plan_Campaign_Program_Tactic_LineItem SET  
					  COST=((Select cost from Plan_Campaign_Program_tactic where PlanTacticId=@EntityId)-(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=@EntityId)) 
					  Where LineItemTypeId is null and PlanTacticId=@EntityId
				END


			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y1' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			      from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' 
				END
				  ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
					
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y2' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2'
				END
				ELSE
					BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		 
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y3' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3' 
				END
			ELSE
					BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y4' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' 
				END
					ELSE
					BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y5' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5' 
				END

				ELSE
					BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y6' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6' 
				END
			ELSE
					BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y7' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' 
				END
		ELSE
					BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y8' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8' 
				END
				ELSE
					BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  --  ELSE
	
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y9' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9' 
				END
		ELSE
					BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END


			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y10' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' 
				END
	

	ELSE
					BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y11' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11' 
				END
	
					ELSE
					BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y12' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12' 
				END

					ELSE
					BEGIN
					IF ((SELECT [DEC] from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	
		
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

	END
	

IF ( LOWER(@Type)='lineitem')
		BEGIN
			IF Exists (select top 1 PlanLineItemId from [Plan_Campaign_Program_Tactic_LineItem] where PlanLineItemId =  @EntityId)
			BEGIN		
			--If line item type will be other line item then it will not be updated.
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN

			--update lineitem cost
				UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic_LineItem] P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId

				--update balance row
			IF((SELECT Top 1 ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId) and LineItemTypeId is null and isdeleted=0) = 0)
				BEGIN
				  UPDATE Plan_Campaign_Program_Tactic_LineItem SET 
				  COST=((Select cost from Plan_Campaign_Program_tactic WHERE PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId))
				  -(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem 
				  where  PlanLineItemId=@EntityId))) 
				  Where LineItemTypeId is null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId)
			END


			--Y1
			IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
				END
				ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

	
             --Y2
			 	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y2')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y2'
				END
				ELSE
				BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
			---Y3
				IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y3')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y3'
				END
				ELSE
				BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

----Y4

	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
				END
				ELSE
				BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
--Y5
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y5')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y5'
				END
				ELSE
				BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		

---Y6
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y6')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y6'
				END
				ELSE
				BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

---y7
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
				END
				ELSE
				BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
--Y8
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y8')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y8'
				END
				ELSE
				BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y9
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y9')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y9'
				END
				ELSE
				BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
				--Y10
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
				END
				ELSE
				BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
				--Y11
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y11')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y11'
				END
				ELSE
				BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y12
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y12')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y12'
				END
				ELSE
				BEGIN
					IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 END
			END
		END
 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
--select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
--select * from @OutputTable
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp

END
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanCostDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanCostDataQuarterly] 
END
GO
CREATE PROCEDURE [dbo].[ImportPlanCostDataQuarterly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetQuarterData READONLY,
@UserId INT 
--@ClientId INT
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, value as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Cost as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId 
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0))   
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
UNION
select ActivityId,[Task Name],'lineitem' as ActivityType,0 as Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
0 AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_Tactic_LineItem_Cost as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData



--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE]) TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData
Declare @Sum float;	DECLARE @newValue FLOAT;
	



IF (LOWER(@Type)='tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId )
			BEGIN				

			--update tactic cost
			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId

				--update balance lineitem
			
			IF((SELECT Top 1  ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=@EntityId and LineItemTypeId IS null and isdeleted=0) = 0)
				BEGIN
					  UPDATE Plan_Campaign_Program_Tactic_LineItem SET  
					  COST=((Select cost from Plan_Campaign_Program_tactic where PlanTacticId=@EntityId)-(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=@EntityId)) 
					  Where LineItemTypeId is null and PlanTacticId=@EntityId
				END


				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataActualTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataActualTactic
			END 
			SELECT * INTO #tempDataActualTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId ) a 
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y4' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y7' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y10' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,4,@newValue			
				END
			END			
		
		 END
       --complete end
		END

IF (LOWER(@Type)='lineitem')
		BEGIN
	
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where PlanLineItemId =  @EntityId)
			BEGIN
			
			--If line item type will be other line item then it will not be updated.
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN


			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic_LineItem] P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId



			--update balance row
			IF((SELECT top 1 ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem 
			Where PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId) and LineItemTypeId is null and isdeleted=0) = 0)
				BEGIN
				  UPDATE Plan_Campaign_Program_Tactic_LineItem SET 
				  COST=((Select cost from Plan_Campaign_Program_tactic WHERE PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId))
				  -(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem 
				  where  PlanLineItemId=@EntityId))) 
				  Where LineItemTypeId is null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId)
			END





				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataLineItemActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataLineItemActual
			END 
			
			SELECT * INTO #tempDataLineItemActual FROM (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Cost WHERE PlanLineItemId = @EntityId) a 

			

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and LOWEr([ActivityType])='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,1,@newValue			
				END
			END
			
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,4,@newValue			
				END
			END		
			
			
				
		END
	END

		END




 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp
END

--Insertation End #2623 import multiple plan
Go


/* Start - Update existing Marketing Budget data for parent records to NULL for ROLL UP */

-- 1. Back up Budget_Detail and Budget_DetailAmount tables
-- 2. Update total budget and total forecast in Budget_Detail table
-- 3. Update parent budget detail records with NULL values for budget and forecast
-- 4. Remove parent records from Budget_DetailAmount table

-- 1. Back up Budget_Detail and Budget_DetailAmount tables
	BEGIN
		IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Budget_Detail_BKP]') AND type in (N'U'))
		BEGIN
		 SELECT * INTO Budget_Detail_BKP FROM Budget_Detail(NOLOCK)
		END
		
		IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Budget_DetailAmount_BKP]') AND type in (N'U'))
		BEGIN
		 SELECT * INTO Budget_DetailAmount_BKP FROM Budget_DetailAmount(NOLOCK)
		END
	END

-- 2. Update total budget and total forecast in Budget_Detail table
	BEGIN
		UPDATE BD SET TotalBudget = TblGrpBy.Budget, TotalForecast = TblGrpBy.Forecast 
		FROM Budget_Detail BD 
		INNER JOIN (
		SELECT BD.Id BudgetDetailId, SUM(BDA.Budget) Budget,SUM(BDA.Forecast) Forecast 
		FROM Budget_Detail(NOLOCK) BD 
		INNER JOIN Budget_DetailAmount(NOLOCK) BDA ON BD.Id = BDA.BudgetDetailId
		GROUP BY BD.Id) TblGrpBy ON BD.Id = TblGrpBy.BudgetDetailId
	END

-- 3. Update Budget_Detail table to do TotalBudget & TotalForecast value to null of Parent records.
	BEGIN
		Update BD SET TotalBudget = NULL, TotalForecast = Null 
		FROM Budget_Detail(NOLOCK) BD
		INNER JOIN Budget_DetailAmount(NOLOCK) BDA ON BDA.BudgetDetailId = BD.ParentId
		WHERE ParentId IS NOT NULL AND IsDeleted = 0
	END

-- 4. Update Budget_DetailAmount table to do '0' for child records those has null value.
	BEGIN
		Update BD1 
		SET TotalBudget =
			CASE 
				WHEN  BD1.TotalBudget is NUll THEN 0 ELSE BD1.TotalBudget
			END 
			, 
			TotalForecast =
			CASE 
				WHEN  BD1.TotalForecast is NUll THEN 0 ELSE BD1.TotalForecast
			END
		FROM Budget_Detail(NOLOCK) BD1
		LEFT JOIN Budget_Detail(NOLOCK) BD2 on BD1.Id =BD2.ParentId
		WHERE BD2.Id is null and BD1.IsDeleted='0' 
	END

-- 5. Update Budget_DetailAmount table to do '0' for child records those has null value.
	BEGIN
		Update BM 
		SET Budget =
			CASE 
				WHEN  BM.Budget is NUll THEN 0 ELSE BM.Budget
			END 
			, 
			Forecast =
			CASE 
				WHEN  BM.Forecast is NUll THEN 0 ELSE BM.Forecast
			END
		FROM Budget_DetailAmount(NOLOCK)BM
		INNER JOIN Budget_Detail(NOLOCK) BD1 on BM.BudgetDetailId =BD1.Id 
		LEFT JOIN Budget_Detail(NOLOCK) BD2 on BD1.Id =BD2.ParentId
		WHERE BD2.Id is null and BD1.IsDeleted='0' 
	END


-- 6. Remove Budget & Forecast values of Parent records from Budget_DetailAmount table
	BEGIN
		DELETE FROM Budget_DetailAmount 
		WHERE Id IN (
		SELECT DISTINCT BDA.Id FROM Budget_Detail(NOLOCK) BD
		INNER JOIN Budget_DetailAmount(NOLOCK) BDA ON BDA.BudgetDetailId = BD.ParentId
		WHERE ParentId IS NOT NULL AND IsDeleted = 0)
	END

/* End - Update existing Marketing Budget data for parent records to NULL for ROLL UP */

-- Start - Added by Arpita Soni for Ticket #2790 on 11/28/2016

/****** Object:  Table [MV].[PreCalculatedMarketingBudget]    Script Date: 11/18/2016 03:00:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MV].[PreCalculatedMarketingBudget]') AND type in (N'U'))
BEGIN
	DROP TABLE [MV].[PreCalculatedMarketingBudget]
END
GO
-- Add new table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MV].[PreCalculatedMarketingBudget]') AND type in (N'U'))
BEGIN
	CREATE TABLE [MV].[PreCalculatedMarketingBudget](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[BudgetDetailId] [int] NULL,
		[Year] [int] NULL,
		[Y1_Budget] [float] NULL,
		[Y2_Budget] [float] NULL,
		[Y3_Budget] [float] NULL,
		[Y4_Budget] [float] NULL,
		[Y5_Budget] [float] NULL,
		[Y6_Budget] [float] NULL,
		[Y7_Budget] [float] NULL,
		[Y8_Budget] [float] NULL,
		[Y9_Budget] [float] NULL,
		[Y10_Budget] [float] NULL,
		[Y11_Budget] [float] NULL,
		[Y12_Budget] [float] NULL,
		[Y1_Forecast] [float] NULL,
		[Y2_Forecast] [float] NULL,
		[Y3_Forecast] [float] NULL,
		[Y4_Forecast] [float] NULL,
		[Y5_Forecast] [float] NULL,
		[Y6_Forecast] [float] NULL,
		[Y7_Forecast] [float] NULL,
		[Y8_Forecast] [float] NULL,
		[Y9_Forecast] [float] NULL,
		[Y10_Forecast] [float] NULL,
		[Y11_Forecast] [float] NULL,
		[Y12_Forecast] [float] NULL,
		[Y1_Planned] [float] NULL,
		[Y2_Planned] [float] NULL,
		[Y3_Planned] [float] NULL,
		[Y4_Planned] [float] NULL,
		[Y5_Planned] [float] NULL,
		[Y6_Planned] [float] NULL,
		[Y7_Planned] [float] NULL,
		[Y8_Planned] [float] NULL,
		[Y9_Planned] [float] NULL,
		[Y10_Planned] [float] NULL,
		[Y11_Planned] [float] NULL,
		[Y12_Planned] [float] NULL,
		[Y1_Actual] [float] NULL,
		[Y2_Actual] [float] NULL,
		[Y3_Actual] [float] NULL,
		[Y4_Actual] [float] NULL,
		[Y5_Actual] [float] NULL,
		[Y6_Actual] [float] NULL,
		[Y7_Actual] [float] NULL,
		[Y8_Actual] [float] NULL,
		[Y9_Actual] [float] NULL,
		[Y10_Actual] [float] NULL,
		[Y11_Actual] [float] NULL,
		[Y12_Actual] [float] NULL,
		[Users] [INT] DEFAULT 0,
		[LineItems] [INT] DEFAULT 0
	) ON [PRIMARY]
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MV].[PreCalculatedMarketingBudget]') AND type in (N'U'))
BEGIN
	-- Create non clustered index in table [MV].[PreCalculatedMarketingBudget]
	IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE name='IX_PreCalculatedMarketingBudget_BudgetDetailId' AND OBJECT_ID = OBJECT_ID('[MV].[PreCalculatedMarketingBudget]'))
	BEGIN
		CREATE NONCLUSTERED INDEX [IX_PreCalculatedMarketingBudget_BudgetDetailId]
		ON [MV].[PreCalculatedMarketingBudget] ([BudgetDetailId])
		INCLUDE ([Y1_Actual],[Y2_Actual],[Y3_Actual],[Y4_Actual],[Y5_Actual],[Y6_Actual],[Y7_Actual],[Y8_Actual],[Y9_Actual],[Y10_Actual],[Y11_Actual],[Y12_Actual],[Users],[LineItems])
	END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFieldOption]') AND type in (N'U'))
BEGIN
	-- Create non clustered index in table [dbo].[CustomFieldOption]
	IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE name='IX_CustomFieldOption_IsDeleted' AND OBJECT_ID = OBJECT_ID('[dbo].[CustomFieldOption]'))
	BEGIN
		CREATE NONCLUSTERED INDEX [IX_CustomFieldOption_IsDeleted]
		ON [dbo].[CustomFieldOption] ([IsDeleted])
		INCLUDE ([CustomFieldOptionId],[Value])
	END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LineItem_Budget]') AND type in (N'U'))
BEGIN
	-- Create non clustered index in table [dbo].[LineItem_Budget]
	IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE name='IX_LineItem_Budget_BudgetDetailId' AND OBJECT_ID = OBJECT_ID('[dbo].[LineItem_Budget]'))
	BEGIN
		CREATE NONCLUSTERED INDEX [IX_LineItem_Budget_BudgetDetailId]
		ON [dbo].[LineItem_Budget] ([BudgetDetailId])
	END
END
GO

--===========================================================================
-- Start - Added by Arpita Soni on 12/06/2016
-- Script which dump existing marketing budget data into pre calcualted table
-- Budget, Forecast, Planned, Actual, Users count, LineItems count
--===========================================================================

BEGIN
	-- Update Budget values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Budget = [Y1],Y2_Budget = [Y2],
					  Y3_Budget = [Y3],Y4_Budget = [Y4],
					  Y5_Budget = [Y5],Y6_Budget = [Y6],
					  Y7_Budget = [Y7],Y8_Budget = [Y8],
					  Y9_Budget = [Y9],Y10_Budget = [Y10],
					  Y11_Budget = [Y11],Y12_Budget = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly budget amount with pivoting
		SELECT * FROM 
		(
			SELECT B.Id AS BudgetDetailId, Period, Budget 
			FROM Budget(NOLOCK) A
			INNER JOIN Budget_Detail(NOLOCK) B ON A.Id = B.BudgetId AND B.IsDeleted = 0
			INNER JOIN Budget_DetailAmount(NOLOCK) C ON B.Id = C.BudgetDetailId
			WHERE A.IsDeleted = 0
		) P
		PIVOT
		(
			MIN(BUDGET)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END
GO

BEGIN
	-- Insert Budget records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Budget, Y2_Budget, Y3_Budget, Y4_Budget, Y5_Budget, 
													Y6_Budget, Y7_Budget,Y8_Budget, Y9_Budget, Y10_Budget, Y11_Budget, Y12_Budget)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly budget amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, Budget 
		FROM Budget(NOLOCK) A
		INNER JOIN Budget_Detail(NOLOCK) B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN Budget_DetailAmount(NOLOCK) C ON B.Id = C.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END
GO

BEGIN
	-- Update Forecast values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Forecast = [Y1],Y2_Forecast = [Y2],
					  Y3_Forecast = [Y3],Y4_Forecast = [Y4],
					  Y5_Forecast = [Y5],Y6_Forecast = [Y6],
					  Y7_Forecast = [Y7],Y8_Forecast = [Y8],
					  Y9_Forecast = [Y9],Y10_Forecast = [Y10],
					  Y11_Forecast = [Y11],Y12_Forecast = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly forecast amount with pivoting
		SELECT * FROM 
		(
			SELECT B.Id AS BudgetDetailId, Period, Forecast FROM Budget(NOLOCK) A
			INNER JOIN Budget_Detail(NOLOCK) B ON A.Id = B.BudgetId AND B.IsDeleted = 0
			INNER JOIN Budget_DetailAmount(NOLOCK) C ON B.Id = C.BudgetDetailId
			WHERE A.IsDeleted = 0
		) P
		PIVOT
		(
			MIN(Forecast)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END
GO

BEGIN
	-- Insert Forecast records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Forecast, Y2_Forecast, Y3_Forecast, Y4_Forecast, Y5_Forecast, 
													Y6_Forecast, Y7_Forecast,Y8_Forecast, Y9_Forecast, Y10_Forecast, Y11_Forecast, Y12_Forecast)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly forecast amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, Budget FROM Budget(NOLOCK) A
		INNER JOIN Budget_Detail(NOLOCK) B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN Budget_DetailAmount(NOLOCK) C ON B.Id = C.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END
GO

BEGIN
	-- Update Planned values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Planned = [Y1],Y2_Planned = [Y2],
					  Y3_Planned = [Y3],Y4_Planned = [Y4],
					  Y5_Planned = [Y5],Y6_Planned = [Y6],
					  Y7_Planned = [Y7],Y8_Planned = [Y8],
					  Y9_Planned = [Y9],Y10_Planned = [Y10],
					  Y11_Planned = [Y11],Y12_Planned = [Y12]
	FROM 
	[MV].PreCalculatedMarketingBudget PreCal 
	INNER JOIN 
	(
		-- Get monthly planned cost amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalPlanned
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			SELECT BD.Id AS BudgetDetailId,PCPTLC.Period, SUM((ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100)) AS TotalPlanned FROM 
			[dbo].[Budget_Detail](NOLOCK) BD 
			INNER JOIN LineItem_Budget(NOLOCK) LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Cost(NOLOCK) PCPTLC ON PCPTL.PlanLineItemId = PCPTLC.PlanLineItemId
			WHERE BD.IsDeleted = 0 AND PCPTL.IsDeleted = 0 AND PCPTL.LineItemTypeId IS NOT NULL 
			GROUP BY BD.Id, PCPTLC.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalPlanned)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
END
GO

BEGIN
	-- Insert Planned records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Planned, Y2_Planned, Y3_Planned, Y4_Planned, Y5_Planned, 
													Y6_Planned, Y7_Planned,Y8_Planned, Y9_Planned, Y10_Planned, Y11_Planned, Y12_Planned)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly planned cost amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalPlanned
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			-- Apply weightage on planned cost and sum up costs for all line items associated to the single budget 
			SELECT BD.Id AS BudgetDetailId,PCPTLC.Period, SUM(ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100) AS TotalPlanned FROM 
			[dbo].[Budget_Detail](NOLOCK) BD 
			INNER JOIN LineItem_Budget(NOLOCK) LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Cost(NOLOCK) PCPTLC ON PCPTL.PlanLineItemId = PCPTLC.PlanLineItemId
			WHERE BD.IsDeleted = 0 AND PCPTL.IsDeleted = 0 AND PCPTL.LineItemTypeId IS NOT NULL 
			GROUP BY BD.Id, PCPTLC.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalPlanned)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END
GO

BEGIN
	-- Update Actual values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Actual = [Y1],Y2_Actual = [Y2],
					  Y3_Actual = [Y3],Y4_Actual = [Y4],
					  Y5_Actual = [Y5],Y6_Actual = [Y6],
					  Y7_Actual = [Y7],Y8_Actual = [Y8],
					  Y9_Actual = [Y9],Y10_Actual = [Y10],
					  Y11_Actual = [Y11],Y12_Actual = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal 
	INNER JOIN 
	(
		-- Get monthly actuals amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalActual
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			-- Apply weightage on actual and sum up actuals for all line items associated to the single budget 
			SELECT BD.Id AS BudgetDetailId,PCPTLA.Period, SUM(ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100) AS TotalActual FROM 
			[dbo].[Budget_Detail](NOLOCK) BD 
			INNER JOIN LineItem_Budget(NOLOCK) LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual(NOLOCK) PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
			WHERE BD.IsDeleted = 0 AND PCPTL.IsDeleted = 0 AND PCPTL.LineItemTypeId IS NOT NULL 
			GROUP BY BD.Id, PCPTLA.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalActual)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
END
GO

BEGIN
	-- Insert Actual records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Actual, Y2_Actual, Y3_Actual, Y4_Actual, Y5_Actual, 
													Y6_Actual, Y7_Actual,Y8_Actual, Y9_Actual, Y10_Actual, Y11_Actual, Y12_Actual)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly actuals amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalActual
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			-- Apply weightage on actual and sum up actuals for all line items associated to the single budget 
			SELECT BD.Id AS BudgetDetailId,PCPTLA.Period, SUM(ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100) AS TotalActual FROM 
			[dbo].[Budget_Detail](NOLOCK) BD 
			INNER JOIN LineItem_Budget(NOLOCK) LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual(NOLOCK) PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
			WHERE BD.IsDeleted = 0 AND PCPTL.IsDeleted = 0 AND PCPTL.LineItemTypeId IS NOT NULL 
			GROUP BY BD.Id, PCPTLA.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalActual)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END
GO

BEGIN
	-- Update user counts into pre calculated table
	UPDATE PreCal SET Users= ISNULL(UsersCount,0)
	FROM [MV].[PreCalculatedMarketingBudget] PreCal
	LEFT JOIN 
	(
		-- Get permitted user count for each budget detail
		SELECT  BD.Id as BudgetDetailId, COUNT(BP.UserId) as UsersCount
		FROM [dbo].[Budget_Detail](NOLOCK) BD
		INNER JOIN [dbo].[Budget_Permission](NOLOCK) BP ON BD.Id = BP.BudgetDetailId 
		WHERE BD.IsDeleted = 0
		GROUP BY BD.Id
	) AS UserCount ON PreCal.BudgetDetailId = UserCount.BudgetDetailId
END
GO

BEGIN
	-- Update line items into pre calculated table
	UPDATE PreCal SET LineItems = ISNULL(LineItemCount,0)
	FROM [MV].[PreCalculatedMarketingBudget] PreCal
	LEFT JOIN 
	(
		-- Get associated line items count for each budget detail 
		SELECT LB.BudgetDetailId, COUNT(LB.PlanLineItemId) AS LineItemCount 
		FROM LineItem_Budget(NOLOCK) LB 
		INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PL ON LB.PlanLineItemId = PL.PlanLineItemId AND PL.IsDeleted=0
		GROUP BY LB.BudgetDetailId
	) TblLineItems ON PreCal.BudgetDetailId = TblLineItems.BudgetDetailId
END
GO

-- End - Added by Arpita Soni on 12/06/2016

-- DROP AND CREATE STORED PROCEDURE [MV].[PreCalPlannedActualForFinanceGrid]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[PreCalPlannedActualForFinanceGrid]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[PreCalPlannedActualForFinanceGrid]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to insert/update precalculated data for marketing budget grid
-- =============================================
-- [MV].[PreCalculatePlannedActualForFinance] 'Planned','Y1',5555, 109415
CREATE PROCEDURE [MV].[PreCalPlannedActualForFinanceGrid]
	@UpdatedColumn		VARCHAR(30),  -- Enum which is used to identify Planned/Actuals
	@Year				INT,		  -- Year 
	@Period				VARCHAR(5),   -- Period in case of editing Monthly/Quarterly allocation
	@NewValue			FLOAT,        -- New value for Budget/Forecast/Custom column
	@OldValue			FLOAT,
	@PlanLineItemId		INT           -- Line Item Id in which Budget is associated
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @InsertQuery		NVARCHAR(MAX), 
			@UpdateColumnName	NVARCHAR(50)
	
	SET @UpdateColumnName = @Period + '_' + @UpdatedColumn
	
	-- Insert Planned/Actual values in PreCalculatedMarketingBudget table if already exists 
	SET @InsertQuery = ' MERGE INTO [MV].[PreCalculatedMarketingBudget] AS T1
						 USING
						 (	
							SELECT BudgetDetailId, 
							' + CAST(@Year AS VARCHAR(30))+ ' AS [Year], 
							'+CAST(ISNULL(@OldValue,0) AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) AS OldValue, 
						 	'+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) AS '+@UpdateColumnName+' 
							FROM LineItem_Budget WHERE PlanLineItemid = '+CAST(@PlanLineItemId AS VARCHAR(30))+'
						 ) AS T2
						 ON (T2.BudgetDetailId = T1.BudgetDetailId AND T2.Year = T1.Year)
						 WHEN MATCHED THEN
						 UPDATE SET ' + @UpdateColumnName + ' = (ISNULL(T1.' + @UpdateColumnName + ',0) - T2.OldValue + ' +'T2.' + @UpdateColumnName +')
						 WHEN NOT MATCHED THEN  
						 INSERT (BudgetDetailId, [Year], ' + @UpdateColumnName + ')
						 VALUES (BudgetDetailId, [Year], ' + @UpdateColumnName + ');'
	
	EXEC(@InsertQuery)
END
GO

-- DROP AND CREATE STORED PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[PreCalBudgetForecastForFinanceGrid]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to insert/update precalculated data for marketing budget grid
-- =============================================
-- [MV].[PreCalBudgetForecastForFinanceGrid] 755,2016,'Y1',5000,0
CREATE PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
	@BudgetDetailId INT,		-- Id of the Budget_Detail table
	@Year INT,					-- Year 
	@Period VARCHAR(5),			-- Period in case of editing Monthly/Quarterly allocation
	@BudgetValue FLOAT=0,		-- New value for Budget
	@ForecastValue FLOAT= 0		-- New value for Forecast
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @InsertQuery		NVARCHAR(MAX), 
			@BudgetColumnName	NVARCHAR(50),
			@ForecastColumnName	NVARCHAR(50)

	SET @BudgetColumnName = @Period+'_Budget'
	SET @ForecastColumnName = @Period+'_Forecast'
	
	-- Insert Budget/Forecast values in PreCalculatedMarketingBudget table if already exists 
	SET @InsertQuery = ' MERGE INTO [MV].[PreCalculatedMarketingBudget] AS T1
						 USING
						 (
							SELECT ' + CAST(@BudgetDetailId AS VARCHAR(30))+ ' AS BudgetDetailId, 
							' + CAST(@Year AS VARCHAR(30))+ ' AS [Year], 
							'+CAST(@BudgetValue AS VARCHAR(30))+' AS NewBudgetValue,
							'+CAST(@ForecastValue AS VARCHAR(30))+' AS NewForecastValue
						 ) AS T2
						 ON (T2.BudgetDetailId = T1.BudgetDetailId AND T2.Year = T1.Year)
						 WHEN MATCHED THEN
						 UPDATE SET T1.' + @BudgetColumnName + ' = T2.NewBudgetValue,
									T1.' + @ForecastColumnName + ' = T2.NewForecastValue
						 WHEN NOT MATCHED THEN  
						 INSERT (BudgetDetailId, [Year], ' + @BudgetColumnName + ',' + @ForecastColumnName + ')
						 VALUES (BudgetDetailId, [Year], T2.NewBudgetValue,T2.NewForecastValue);'
	
	EXEC (@InsertQuery)
END
GO


-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetLineItemIdsByBudgetDetailId]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetLineItemIdsByBudgetDetailId]
GO

-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'GetFinanceBasicData') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetFinanceBasicData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetFinanceBasicData]
(
	@BudgetId		INT,			-- Budget Id
	@lstUserIds		NVARCHAR(MAX),	-- List of all users for this client
	@UserId			INT				-- Logged in user id 
)
RETURNS 
@ResultFinanceData TABLE 
(
	Permission		VARCHAR(20),
	BudgetDetailId	INT,
	ParentId		INT,
	Name			VARCHAR(MAX),
	[Owner]			INT,
	TotalBudget		FLOAT,
	TotalForecast	FLOAT,
	TotalPlanned	FLOAT
)
AS

BEGIN
	
	-- SELECT * FROM [dbo].[GetFinanceBasicData](1,'2,4,6,12,13,17,22,24,27,30,32,37,38,42,43,49,53,58,59,641,62,66,71,77,79,80,82,643,85,86,87,88,89,92,96,97,101,106,107,617,646,110,113,114,624,120,125,129,131,133,619,134,136,137,139,647,142,144,146,150,155,157,158,160,163,165,166,645,169,174,175,176,179,186,623,188,190,191,192,194,195,208,210,211,213,214,219,220,221,223,224,621,243,245,250,626,254,256,258,259,260,625,261,262,269,270,272,273,275,276,277,281,288,289,291,292,293,295,298,299,301,303,304,313,314,315,318,320,322,324,327,330,339,341,342,344,349,355,359,361,362,365,366,370,374,378,382,387,389,393,394,398,399,403,406,407,408,412,415,419,426,427,428,431,434,435,440,446,447,449,450,451,454,457,459,462,465,466,467,468,470,471,472,475,481,485,486,487,488,492,493,495,497,503,504,505,512,514,516,517,518,519,520,521,526,527,533,536,537,542,544,545,546,549,551,555,556,561,566,573,574,578,584,585,586,588,590,649,593,595,602,608',549)
	
	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	BEGIN
		DECLARE @tblResult TABLE
		(
			Permission		VARCHAR(20),
			BudgetDetailId	INT,
			ParentId		INT,
			Name			VARCHAR(MAX),
			[Owner]			INT,
			TotalBudget		FLOAT,
			TotalForecast	FLOAT
		)

		-- Get Budget details based on permissions
		INSERT INTO @tblResult(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast)
		SELECT
				CASE WHEN BP.PermisssionCode = 0 THEN 'Edit'
					  WHEN BP.PermisssionCode = 1 THEN 'View'
					  ELSE 'None' 
				END AS Permission	
				,BD.Id AS BudgetDetailId
				,BD.ParentId
				,BD.Name
				,U.UserId as '[Owner]'
				,BD.TotalBudget
				,BD.TotalForecast
		FROM [dbo].Budget_Detail(NOLOCK) BD 
		INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
		LEFT JOIN [dbo].[Budget_Permission](NOLOCK) BP ON BD.Id = BP.BudgetDetailId AND BP.UserId = @UserId
		WHERE BD.IsDeleted='0' AND BD.BudgetId = @BudgetId 
	END
	
	INSERT INTO @ResultFinanceData(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast,TotalPlanned)
	SELECT DISTINCT R.Permission,R.BudgetDetailId,R.ParentId,R.Name,R.[Owner],R.TotalBudget,R.TotalForecast,LineItem.TotalPlanned
	FROM @tblResult R
	LEFT JOIN 
	(	
		-- Get Planned Cost values
		SELECT BD.Id AS BudgetDetailId, SUM((PCPTL.Cost * CAST(Weightage AS FLOAT)/100)) AS TotalPlanned FROM 
		[dbo].[Budget_Detail](NOLOCK) BD 
		INNER JOIN LineItem_Budget(NOLOCK) LB ON BD.Id = LB.BudgetDetailId
		INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
		WHERE BD.IsDeleted=0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL and PCPTL.IsDeleted = 0
		GROUP BY BD.Id
	) LineItem ON R.BudgetDetailId = LineItem.BudgetDetailId

	RETURN 
END

GO



IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalBudgetForecastMarketingBudget]
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
CREATE TRIGGER [dbo].[TrgPreCalBudgetForecastMarketingBudget]
   ON  [dbo].[Budget_DetailAmount]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Period VARCHAR(30),
			@BudgetValue FLOAT,
			@ForecastValue FLOAT,
			@BudgetDetailId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX),
			@BudgetColumnName	NVARCHAR(50),
			@ForecastColumnName	NVARCHAR(50)

	SET @BudgetColumnName = @Period+'_Budget'
	SET @ForecastColumnName = @Period+'_Forecast'	
	
	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @BudgetValue = ISNULL(Budget,0),
			   @ForecastValue = ISNULL(Forecast,0),
			   @BudgetDetailId = BudgetDetailId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED I
		INNER JOIN Budget_Detail(NOLOCK) BD ON I.BudgetDetailId = BD.Id
		-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
		EXEC [MV].[PreCalBudgetForecastForFinanceGrid] @BudgetDetailId, @Year, @Period, @BudgetValue, @ForecastValue
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period,
			   @BudgetDetailId = BudgetDetailId,
			   @Year = YEAR(CreatedDate) FROM DELETED D
			   INNER JOIN Budget_Detail(NOLOCK) BD ON D.BudgetDetailId = BD.Id

		-- Delete/Update record into pre-calculated table while Cost entry is deleted
		SET @DeleteQuery = 'UPDATE P SET ' +@Period + '_'+@BudgetColumnName +' = NULL, 
										 ' +@Period + '_'+@ForecastColumnName +' = NULL  
							FROM [MV].[PreCalculatedMarketingBudget] P
							WHERE P.BudgetDetailId = ' +CAST(@BudgetDetailId AS VARCHAR(30)) +' AND P.Year = ' + CAST(@Year AS VARCHAR(30)) 
		EXEC (@DeleteQuery)
	END

END
GO

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
			@OldValue FlOAT = 0,
			@PlanLineItemId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX)

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @NewValue = Value,
			   @PlanLineItemId = PlanLineItemId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED 

		-- Get old value in case of update
		SELECT @OldValue = Value FROM DELETED

		IF ((SELECT COUNT(id) FROM LineItem_Budget(NOLOCK) WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
			EXEC [MV].[PreCalPlannedActualForFinanceGrid] @UpdatedColumn, @Year, @Period, @NewValue,@OldValue, @PlanLineItemId
		END
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period, @PlanLineItemId = PlanLineItemId,@Year = YEAR(CreatedDate) FROM DELETED 

		IF ((SELECT COUNT(id) FROM LineItem_Budget(NOLOCK) WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Delete/Update record into pre-calculated table while Cost entry is deleted
			SET @DeleteQuery = 'UPDATE P SET 
								' +@Period + '_'+@UpdatedColumn +' = (P.' +@Period + '_'+ @UpdatedColumn + ' - '+CAST(@OldValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) + '+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100))
								FROM [MV].[PreCalculatedMarketingBudget] P
								INNER JOIN [dbo].[LineItem_Budget] LB ON P.BudgetDetailId = LB.BudgetDetailId 
								WHERE LB.PlanLineItemId = ' +CAST(@PlanLineItemId AS VARCHAR(30)) + ' AND P.Year = ' + CAST(@Year AS VARCHAR(30))
			EXEC (@DeleteQuery)
		END
	END

END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalActualMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalActualMarketingBudget]
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
CREATE TRIGGER [dbo].[TrgPreCalActualMarketingBudget]
   ON  [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @UpdatedColumn VARCHAR(30) = 'Actual',
			@Period VARCHAR(30),
			@NewValue FLOAT,
			@OldValue FLOAT,
			@PlanLineItemId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX)

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @NewValue = Value,
			   @PlanLineItemId = PlanLineItemId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED 

		-- Get old value in case of update
		SELECT @OldValue = Value FROM DELETED

		IF ((SELECT COUNT(id) FROM LineItem_Budget(NOLOCK) WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
			EXEC [MV].[PreCalPlannedActualForFinanceGrid] @UpdatedColumn, @Year, @Period, @NewValue,@OldValue, @PlanLineItemId
		END
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period, @PlanLineItemId = PlanLineItemId,@Year = YEAR(CreatedDate) FROM DELETED 

		IF ((SELECT COUNT(id) FROM LineItem_Budget(NOLOCK) WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Delete/Update record into pre-calculated table while Cost entry is deleted
			SET @DeleteQuery = 'UPDATE P SET 
								' +@Period + '_'+@UpdatedColumn +' = (P.' +@Period + '_'+ @UpdatedColumn + ' - '+CAST(@OldValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) + '+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100))
								FROM [MV].[PreCalculatedMarketingBudget] P
								INNER JOIN [dbo].[LineItem_Budget] LB ON P.BudgetDetailId = LB.BudgetDetailId 
								WHERE LB.PlanLineItemId = ' +CAST(@PlanLineItemId AS VARCHAR(30)) + ' AND P.Year = ' + CAST(@Year AS VARCHAR(30))
			EXEC (@DeleteQuery)
		END
	END

END
GO


IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgInsertDeletePreCalMarketingBudget]
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
CREATE TRIGGER [dbo].[TrgInsertDeletePreCalMarketingBudget]
   ON  [dbo].[Budget_Detail]
   AFTER INSERT, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Insert new record into pre calculate when new budget is generated
		INSERT INTO [MV].[PreCalculatedMarketingBudget] (BudgetDetailId, [Year])
		SELECT Id, YEAR(CreatedDate) FROM INSERTED
	END
	ELSE
	BEGIN
		-- Delete record from pre calculate table
		DELETE P FROM [MV].[PreCalculatedMarketingBudget] P
		INNER JOIN DELETED D ON P.BudgetDetailId = D.Id
	END

END
GO

-- DROP AND CREATE STORED PROCEDURE [dbo].[GetFinanceCustomfieldColumnsData]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetFinanceCustomfieldColumnsData]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetFinanceCustomfieldColumnsData]
END
GO
/****** Object:  StoredProcedure [dbo].[GetFinanceCustomfieldColumnsData]    Script Date: 11/28/2016 06:31:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Viral
-- Create date: 11/21/2016
-- Description:	Get Finance Customfield Columns
-- =============================================
CREATE PROCEDURE [dbo].[GetFinanceCustomfieldColumnsData]
	-- Add the parameters for the stored procedure here
	@BudgetId int, 
	@ClientId int 
AS
BEGIN

	-- Exec GetFinanceCustomfieldColumnsData 2766,24
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	DECLARE @query VARCHAR(MAX)=''
		
	DECLARE @columns VARCHAR(MAX)
	DECLARE @drpdCustomType VARCHAR(50)='DropDownList'
		
	SELECT @columns= COALESCE(@columns+', ' ,'') + C.Name+ '_'+CAST(C.CustomFieldId AS NVARCHAR(30))
	FROM Budget_ColumnSet(NOLOCK) A
	INNER JOIN Budget_Columns(NOLOCK) B ON A.Id= B.Column_SetId
	INNER JOIN CustomField(NOLOCK) C ON B.CustomFieldId = C.CustomFieldId and C.EntityType='Budget'
	WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND C.IsDeleted = 0 AND A.ClientId = @clientID AND MapTableName = 'CustomField_Entity'
		
	IF(ISNULL(@columns,'')!='')
	BEGIN
		SET @query = '
		SELECT * 
		FROM (
		     SELECT C.Name as Name,CF.EntityId as BudgetDetailId, 
			 		CASE 
					WHEN CT.Name='''+@drpdCustomType+''' THEN CFO.Value ELSE CF.Value
				END as Value
			FROM Budget_ColumnSet(NOLOCK) A
			INNER JOIN Budget_Columns(NOLOCK) B ON A.Id= B.Column_SetId
			INNER JOIN CustomField(NOLOCK) C ON B.CustomFieldId = C.CustomFieldId and C.EntityType=''Budget''
			INNER JOIN CustomFieldType(NOLOCK) CT ON C.CustomFieldTypeId = CT.CustomFieldTypeId
			LEFT JOIN CustomField_Entity(NOLOCK) CF ON C.CustomFieldId = CF.CustomFieldId and EntityID IN (select Id FROM Budget_Detail where BudgetId = '+CAST(@budgetID AS VARCHAR(20)) +' AND IsDeleted=0) 
			--INNER JOIN Budget_Detail BD ON CF.EntityId = BD.Id and BD.IsDeleted=0 and BD.BudgetId ='+CAST(@budgetID AS VARCHAR(20)) +'
			LEFT JOIN CustomFieldOption(NOLOCK) CFO ON CF.Value = CAST(CFO.CustomFieldOptionId AS nvarchar(30)) and CT.Name='''+@drpdCustomType+''' AND CFO.IsDeleted = 0 
			WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND C.IsDeleted = 0 AND 
			A.ClientId = '+CAST(@clientID AS VARCHAR(20))+' AND MapTableName = ''CustomField_Entity'' and IsNUll(CF.EntityId,'''') <> ''''
		) as s
		PIVOT
		(
		    MIN(Value)
		    FOR [Name] IN ('+@columns+')
		)AS pvt'
		EXEC (@query)

		-- Get custom field list with options to bind drop downs
		SELECT CF.CustomFieldId, CFO.CustomFieldOptionId, CFO.Value 
		FROM Budget_Columns(NOLOCK) Col 
		INNER JOIN Budget_ColumnSet(NOLOCK) CS ON Col.Column_SetId = CS.Id
		INNER JOIN CustomField(NOLOCK) CF ON Col.CustomFieldId = CF.CustomFieldId AND CF.EntityType='Budget'
		INNER JOIN CustomFieldType(NOLOCK) CFT ON CF.CustomFieldTypeId = CFT.CustomFieldTypeId AND CFT.Name = @drpdCustomType
		INNER JOIN CustomFieldOption(NOLOCK) CFO ON CF.CustomFieldId = CFO.CustomFieldId AND CFO.IsDeleted = 0
		WHERE Col.IsDeleted = 0 AND CS.IsDeleted = 0 AND CF.IsDeleted = 0 
			  	AND CS.ClientId = @clientID AND MapTableName = 'CustomField_Entity'
	END
END
GO


-- DROP AND CREATE STORED PROCEDURE [MV].[GetFinanceGridData]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[GetFinanceGridData]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[GetFinanceGridData]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to fetch finance grid data
-- =============================================
CREATE PROCEDURE [MV].[GetFinanceGridData]
	@BudgetId		INT,
	@ClientId		INT,
	@timeframe		VARCHAR(50) = 'Yearly',
	@lstUserIds		NVARCHAR(MAX) = '',
	@UserId			INT,
	@CurrencyRate	FLOAT = 1.0
AS
BEGIN
	
	-- EXEC MV.[GetFinanceGridData] 2807,24,'months','470,308,104',470,0.5
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Declare local variables
	BEGIN

		-- Start: declare timeframe related variables
		Declare @ThisYear varchar(100) ='Yearly' -- This Year
		Declare @ThisQuarters varchar(100) ='quarters' -- This Year (Quarterly)
		Declare @ThisMonthly varchar(100) ='months' -- This Year (Monthly)
		Declare @Quarter1 varchar(100) ='Quarter1' -- Quarter1
		Declare @Quarter2 varchar(100) ='Quarter2' -- Quarter2
		Declare @Quarter3 varchar(100) ='Quarter3' -- Quarter3
		Declare @Quarter4 varchar(100) ='Quarter4' -- Quarter4
		-- End: declare timeframe related variables

	END
	
	IF(@timeframe = @ThisYear)	-- This Year
	BEGIN
		SELECT 
			F.Permission	
			,F.BudgetDetailId
			,F.ParentId
			,F.Name
			,F.TotalBudget * @CurrencyRate as Budget
			,F.TotalForecast * @CurrencyRate as Forecast
			,F.TotalPlanned * @CurrencyRate as Planned
			,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate  as Actual
			,P.Users
			,P.LineItems
			,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	IF(@timeframe = @ThisQuarters)	-- This Year (Quarterly)
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals for Quarter 1
				,(ISNULL(Y1_Budget,0)+ISNULL(Y2_Budget,0)+ISNULL(Y3_Budget,0)) * @CurrencyRate as Q1_Budget
				,(ISNULL(Y1_Forecast,0)+ISNULL(Y2_Forecast,0)+ISNULL(Y3_Forecast,0)) * @CurrencyRate as Q1_Forecast
				,(ISNULL(Y1_Planned,0)+ISNULL(Y2_Planned,0)+ISNULL(Y3_Planned,0)) * @CurrencyRate as Q1_Planned
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0)) * @CurrencyRate as Q1_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 2
				,(ISNULL(Y4_Budget,0)+ISNULL(Y5_Budget,0)+ISNULL(Y6_Budget,0)) * @CurrencyRate as Q2_Budget
				,(ISNULL(Y4_Forecast,0)+ISNULL(Y5_Forecast,0)+ISNULL(Y6_Forecast,0)) * @CurrencyRate as Q2_Forecast
				,(ISNULL(Y4_Planned,0)+ISNULL(Y5_Planned,0)+ISNULL(Y6_Planned,0)) * @CurrencyRate as Q2_Planned
				,(ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0)) * @CurrencyRate as Q2_Actual
				
				-- Budget, Forecast, Planned, Actuals for Quarter 3
				,(ISNULL(Y7_Budget,0)+ISNULL(Y8_Budget,0)+ISNULL(Y9_Budget,0)) * @CurrencyRate as Q3_Budget
				,(ISNULL(Y7_Forecast,0)+ISNULL(Y8_Forecast,0)+ISNULL(Y9_Forecast,0)) * @CurrencyRate as Q3_Forecast
				,(ISNULL(Y7_Planned,0)+ISNULL(Y8_Planned,0)+ISNULL(Y9_Planned,0)) * @CurrencyRate as Q3_Planned
				,(ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0)) * @CurrencyRate as Q3_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 4
				,(ISNULL(Y10_Budget,0)+ISNULL(Y11_Budget,0)+ISNULL(Y12_Budget,0)) * @CurrencyRate as Q4_Budget
				,(ISNULL(Y10_Forecast,0)+ISNULL(Y11_Forecast,0)+ISNULL(Y12_Forecast,0)) * @CurrencyRate as Q4_Forecast
				,(ISNULL(Y10_Planned,0)+ISNULL(Y11_Planned,0)+ISNULL(Y12_Planned,0)) * @CurrencyRate as Q4_Planned
				,(ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as Q4_Actual

				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,P.Users
				,P.LineItems
				,F.[Owner]
				
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @ThisMonthly)	-- This Year (Monthly)
	BEGIN
		SELECT 
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals month wise columns 
				,(Y1_Budget * @CurrencyRate  ) AS Y1_Budget,  (Y1_Forecast * @CurrencyRate ) AS Y1_Forecast
				,(Y1_Planned * @CurrencyRate ) AS Y1_Planned, (Y1_Actual * @CurrencyRate   ) AS Y1_Actual 
				,(Y2_Budget * @CurrencyRate  ) AS Y2_Budget,  (Y2_Forecast * @CurrencyRate ) AS Y2_Forecast 
				,(Y2_Planned * @CurrencyRate ) AS Y2_Planned, (Y2_Actual * @CurrencyRate   ) AS Y2_Actual
				,(Y3_Budget * @CurrencyRate  ) AS Y3_Budget,  (Y3_Forecast * @CurrencyRate ) AS Y3_Forecast
				,(Y3_Planned * @CurrencyRate ) AS Y3_Planned, (Y3_Actual * @CurrencyRate   ) AS Y3_Actual
				,(Y4_Budget * @CurrencyRate  ) AS Y4_Budget,  (Y4_Forecast * @CurrencyRate ) AS Y4_Forecast 
				,(Y4_Planned * @CurrencyRate ) AS Y4_Planned, (Y4_Actual * @CurrencyRate   ) AS Y4_Actual 
				,(Y5_Budget * @CurrencyRate  ) AS Y5_Budget,  (Y5_Forecast * @CurrencyRate ) AS Y5_Forecast 
				,(Y5_Planned * @CurrencyRate ) AS Y5_Planned, (Y5_Actual * @CurrencyRate   ) AS Y5_Actual 
				,(Y6_Budget * @CurrencyRate  ) AS Y6_Budget,  (Y6_Forecast * @CurrencyRate ) AS Y6_Forecast 
				,(Y6_Planned * @CurrencyRate ) AS Y6_Planned, (Y6_Actual * @CurrencyRate   ) AS Y6_Actual 
				,(Y7_Budget * @CurrencyRate  ) AS Y7_Budget,  (Y7_Forecast * @CurrencyRate ) AS Y7_Forecast 
				,(Y7_Planned * @CurrencyRate ) AS Y7_Planned, (Y7_Actual * @CurrencyRate   ) AS Y7_Actual 
				,(Y8_Budget * @CurrencyRate  ) AS Y8_Budget,  (Y8_Forecast * @CurrencyRate ) AS Y8_Forecast 
				,(Y8_Planned * @CurrencyRate ) AS Y8_Planned, (Y8_Actual * @CurrencyRate   ) AS Y8_Actual 
				,(Y9_Budget * @CurrencyRate  ) AS Y9_Budget,  (Y9_Forecast * @CurrencyRate ) AS Y9_Forecast 
				,(Y9_Planned * @CurrencyRate ) AS Y9_Planned, (Y9_Actual * @CurrencyRate   ) AS Y9_Actual 
				,(Y10_Budget * @CurrencyRate ) AS Y10_Budget, (Y10_Forecast * @CurrencyRate) AS  Y10_Forecast 
				,(Y10_Planned * @CurrencyRate) AS Y10_Planned,(Y10_Actual * @CurrencyRate  ) AS Y10_Actual 
				,(Y11_Budget * @CurrencyRate ) AS Y11_Budget, (Y11_Forecast * @CurrencyRate) AS  Y11_Forecast 
				,(Y11_Planned * @CurrencyRate) AS Y11_Planned,(Y11_Actual * @CurrencyRate  ) AS Y11_Actual 
				,(Y12_Budget * @CurrencyRate ) AS Y12_Budget, (Y12_Forecast * @CurrencyRate) AS  Y12_Forecast 
				,(Y12_Planned * @CurrencyRate) AS Y12_Planned,(Y12_Actual * @CurrencyRate  ) AS Y12_Actual 

				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'

				,P.[Users]
				,P.LineItems
				,F.[Owner]

		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter1)	-- Quarter1
	BEGIN
		SELECT 
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 1
				,(Y1_Budget * @CurrencyRate) AS Y1_Budget,(Y1_Forecast * @CurrencyRate) AS Y1_Forecast,(Y1_Planned * @CurrencyRate) AS Y1_Planned,(Y1_Actual * @CurrencyRate) AS Y1_Actual 
				,(Y2_Budget * @CurrencyRate) AS Y2_Budget,(Y2_Forecast * @CurrencyRate) AS Y2_Forecast,(Y2_Planned * @CurrencyRate) AS Y2_Planned,(Y2_Actual * @CurrencyRate) AS Y2_Actual 
				,(Y3_Budget * @CurrencyRate) AS Y3_Budget,(Y3_Forecast * @CurrencyRate) AS Y3_Forecast,(Y3_Planned * @CurrencyRate) AS Y3_Planned,(Y3_Actual * @CurrencyRate) AS Y3_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				
				,P.[Users]
				,P.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter2)	-- Quarter2
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 2
				,(Y4_Budget * @CurrencyRate) AS Y4_Budget,(Y4_Forecast * @CurrencyRate) AS Y4_Forecast,(Y4_Planned * @CurrencyRate) AS Y4_Planned,(Y4_Actual * @CurrencyRate) AS Y4_Actual
				,(Y5_Budget * @CurrencyRate) AS Y5_Budget,(Y5_Forecast * @CurrencyRate) AS Y5_Forecast,(Y5_Planned * @CurrencyRate) AS Y5_Planned,(Y5_Actual * @CurrencyRate) AS Y5_Actual
				,(Y6_Budget * @CurrencyRate) AS Y6_Budget,(Y6_Forecast * @CurrencyRate) AS Y6_Forecast,(Y6_Planned * @CurrencyRate) AS Y6_Planned,(Y6_Actual * @CurrencyRate) AS Y6_Actual
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,P.[Users]
				,P.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
		
	END
	ELSE IF(@timeframe = @Quarter3)	-- Quarter3
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name
				-- Budget, Forecast, Planned, Actuals columns for Quarter 3
				,(Y7_Budget * @CurrencyRate) AS Y7_Budget ,(Y7_Forecast * @CurrencyRate) AS Y7_Forecast ,(Y7_Planned * @CurrencyRate) AS Y7_Planned ,(Y7_Actual * @CurrencyRate) AS Y7_Actual 
				,(Y8_Budget * @CurrencyRate) AS Y8_Budget ,(Y8_Forecast * @CurrencyRate) AS Y8_Forecast ,(Y8_Planned * @CurrencyRate) AS Y8_Planned ,(Y8_Actual * @CurrencyRate) AS Y8_Actual 
				,(Y9_Budget * @CurrencyRate) AS Y9_Budget ,(Y9_Forecast * @CurrencyRate) AS Y9_Forecast ,(Y9_Planned * @CurrencyRate) AS Y9_Planned ,(Y9_Actual * @CurrencyRate) AS Y9_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,P.[Users]
				,P.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter4)	-- Quarter 4
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name
				-- Budget, Forecast, Planned, Actuals columns for Quarter 4
				,(Y10_Budget * @CurrencyRate) AS Y10_Budget ,(Y10_Forecast * @CurrencyRate) AS Y10_Forecast ,(Y10_Planned * @CurrencyRate) AS Y10_Planned ,(Y10_Actual * @CurrencyRate) AS Y10_Actual 
				,(Y11_Budget * @CurrencyRate) AS Y11_Budget ,(Y11_Forecast * @CurrencyRate) AS Y11_Forecast ,(Y11_Planned * @CurrencyRate) AS Y11_Planned ,(Y11_Actual * @CurrencyRate) AS Y11_Actual 
				,(Y12_Budget * @CurrencyRate) AS Y12_Budget ,(Y12_Forecast * @CurrencyRate) AS Y12_Forecast ,(Y12_Planned * @CurrencyRate) AS Y12_Planned ,(Y12_Actual * @CurrencyRate) AS Y12_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				
				,P.[Users]
				,P.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@lstUserIds,@UserId) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END

	-- Get custom columns data
	 EXEC [dbo].[GetFinanceCustomfieldColumnsData] @BudgetId, @ClientId

END
GO

-- End - Added by Arpita Soni for Ticket #2790 on 11/28/2016

Go



-- Start - Added by Viral for Ticket #2763 on 11/29/2016

/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 11/29/2016 4:12:04 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGridFilters]
GO
/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 11/29/2016 4:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridFilters] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetGridFilters] 
	@userId int
	,@ClientId int
	,@IsDefaultCustomRestrictionsViewable bit 
AS --Todo: New user login then need to some more 
BEGIN
	
	--EXEC GetGridFilters 549,4,0

	SET NOCOUNT ON;

		Declare @PlanId NVARCHAR(MAX) = ''
		Declare @OwnerIds NVARCHAR(MAX) = ''
		Declare @TacticTypeIds varchar(max)=''
		Declare @StatusIds varchar(max)=''
		Declare @customFields varchar(max)=''

		Declare @viewname  varchar(max)

		Declare @tblUserSavedViews Table(
		Id INT
		,ViewName NVARCHAR(max)
		,FilterName NVARCHAR(1000)
		,FilterValues NVARCHAR(max)
		,LastModifiedDate DATETIME
		,IsDefaultPreset BIT
		,Userid INT
		)

		Declare @keyPlan varchar(30)='Plan'
		Declare @keyOwner varchar(30)='Owner'
		Declare @keyStatus varchar(30)='Status'
		Declare @keyTacticType varchar(30)='TacticType'
		Declare @keyAll varchar(10)='All'
		Declare @keyCustomField varchar(100)='CustomField'
		

		select TOP 1 @viewname =  ViewName from Plan_UserSavedViews where Userid=@userId AND IsDefaultPreset = 1
		SET @viewname = ISNULL(@viewname,'')

		-- Insert user filters to local variables.
		INSERT INTO @tblUserSavedViews(Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid)
		select Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid from Plan_UserSavedViews where Userid=@userId AND ISNULL(ViewName,'') = IsNull(@viewname,'')

		IF EXISTS(select Id from @tblUserSavedViews)
		BEGIN
			
		

		-- Get PlanIds that user has selected under filter.
		SELECT TOP 1 @PlanId = FilterValues from @tblUserSavedViews where FilterName=@keyPlan

		-- Get OwnerIds that user has selected under filter.
		SELECT TOP 1 @OwnerIds = FilterValues from @tblUserSavedViews where FilterName=@keyOwner

		-- Get TacticTypeIds that user has selected under filter.
		SELECT TOP 1 @TacticTypeIds = FilterValues from @tblUserSavedViews where FilterName=@keyTacticType

		-- Get Status that user has selected under filter.
		SELECT TOP 1 @StatusIds = FilterValues from @tblUserSavedViews where FilterName=@keyStatus

		-- Get Status that user has selected under filter.
		SET @customFields = ''


		BEGIN

			Declare @customFieldId varchar(100)
			Declare @FilterValues varchar(max)
			Declare @cntFiltr INT
			Declare @cntPermsn INT
			Declare @CustomFilters varchar(max) 


			  Declare @CustomFieldIDs varchar(max)=''

			 SELECT  @CustomFieldIDs = COALESCE(@CustomFieldIDs + ',', '') + CONVERT(varchar(100), CF.[CustomFieldId])  + '_null'  FROM [CustomField] CF
			 inner join CustomFieldType CT on CT.CustomFieldTypeId = CF.CustomFieldTypeId and CT.name = 'DropDownList'
			  where  CF.EntityType = 'Tactic'  and CF.isdeleted = 0 and CF.IsDisplayForFilter = 1 AND CF.ClientId = @ClientId 
             AND ( CF.[CustomFieldId] NOT IN
			  (select  CAST((CASE WHEN REPLACE(FilterName,'CF_','') NOT LIKE '%[^0-9]%' THEN REPLACE(FilterName,'CF_','') END) AS INT)  from Plan_UserSavedViews where Userid=@userId and FilterName like'CF_%')
			  )

			   if( LEFT(@CustomFieldIDs,1) = ',')
			     SET  @CustomFieldIDs = substring(@CustomFieldIDs,2,LEN(@CustomFieldIDs))

			select @cntPermsn = count(*) from CustomRestriction as CR Where UserId = @userId

			


			
			DECLARE db_cursor CURSOR FOR  
			select REPLACE(FilterName,'CF_',''),FilterValues from @tblUserSavedViews where Userid=@userId and FilterName like'CF_%'
			
			OPEN db_cursor   
			FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues   
			
			WHILE @@FETCH_STATUS = 0   
			BEGIN   
				
				IF(IsNull(@cntPermsn,0) > 0)
				BEGIN
				   select @cntFiltr = count(*) from CustomRestriction as CR
					JOIN CustomField as C on CR.CustomFieldId = C.CustomFieldId and C.ClientId=@ClientId and IsDeleted='0' and C.IsRequired='1' and ( (CR.Permission = 1)  OR (CR.Permission = 2) )
					where UserId = @userId and C.CustomFieldId=Cast(@customFieldId as INT) and cr.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
			
				

				END
				ELSE IF(@IsDefaultCustomRestrictionsViewable = '1' )
				BEGIN
						SELECT @cntFiltr = count(*) from CustomField as C
						Join CustomFieldType as CT on C.CustomFieldTypeId = C.CustomFieldTypeId and CT.Name ='DropDownList'
						JOIN CustomFieldOption as CO on C.CustomFieldId = CO.CustomFieldId and CO.IsDeleted='0' and CO.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
						where ClientId=@ClientId and C.IsDeleted='0' and  EntityType='Tactic' and IsDisplayForFilter='1' and C.CustomFieldId= Cast(@customFieldId as INT)
				END

				IF (IsNull(@cntFiltr,0) > 0)
				BEGIN
					SET @customFields = @customFields + ',' + @customFieldId + '_' + REPLACE(@FilterValues,',',','+@customFieldId + '_' ) 
				END
				
			       FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues      
			END   
			
			CLOSE db_cursor   
			DEALLOCATE db_cursor
			if(LEN(@customFields) > 2)
			SET @customFields = SUBSTRING(@customFields,2, LEN(@customFields)-1) 

		END

		END
		ELSE
		BEGIN
			
			SELECT Top 1 @PlanId = p.PlanId from Model as M
			JOIN [Plan] as P on M.ModelId = P.ModelId and M.ClientId=@ClientId and P.IsDeleted='0' and M.IsDeleted='0' and P.[Year]<=Datepart(yyyy,GETDATE())
			Order by P.Year desc, P.Title
			
			SET @OwnerIds=@keyAll
			SET @TacticTypeIds=@keyAll
			SET @StatusIds='Created,Submitted,Approved,In-Progress,Complete,Declined'
			SET @customFields = ''
		END
		
		IF(@CustomFieldIDs != '')
		BEGIN
		 IF(@customFields = '')
				    BEGIN
				       SET @customFields =  @CustomFieldIDs
				   END
				   ELSE
				   BEGIN
				      SET @customFields = @customFields + ',' + @CustomFieldIDs
				   END
		END
	             	
				
			

		select @PlanId PlanIds,@OwnerIds OwnerIds,@StatusIds StatusIds,@TacticTypeIds TacticTypeIds,@customFields CustomFieldIds

    
END



GO

-- End - Added by Viral for Ticket #2763 on 11/29/2016

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'DeleteMarketingBudget') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE DeleteMarketingBudget
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rahul Shah
-- Create date: 11/30/2016
-- Description:	SP to Delete Marketing Budget data
-- =============================================

CREATE PROCEDURE [dbo].[DeleteMarketingBudget]
@BudgetDetailId int = 0,
@ClientId INT = 0
AS 

BEGIN
	SET NOCOUNT ON
	--Check the cross client and thorws an error 
	Declare @TempClientId INT=0
	SELECT @TempClientId= ClientId FROM Budget B INNER JOIN Budget_Detail bd ON bd.BudgetId=b.Id
	WHERE bd.Id=@BudgetDetailId
	--print(@TempClientId)
	IF(@ClientId<>@TempClientId)
	BEGIN
		RAISERROR ('You don''t have permission to delete this budget.', 16, 1);
	END

	Declare @ErrorMeesgae varchar(3000) = ''
	Declare @OtherBudgetId int = 0
	Declare @ParentIdCount int = 0
	Declare @LineItemBudgetCount int = 0
	Declare @NextBudgetId int = 0
	Declare @RowCount int = 0
	Declare @BudgetDetailData Table (Id int, ParentId int null, BudgetId int )
	BEGIN TRY
		-- GET 'N' Level Heirarchy for selected budget from budget detail table
		;WITH BudgetCTE AS
		( 
			SELECT Id
			,ParentId
			,BudgetId
			,IsDeleted
			FROM [dbo].[Budget_Detail] 
			WHERE ParentId = @BudgetDetailId OR Id = @BudgetDetailId
			AND IsDeleted=0 OR IsDeleted is null
			UNION ALL
			SELECT a.Id
			,a.ParentId
			,a.BudgetId
			,a.IsDeleted
			FROM [dbo].[Budget_Detail] a 
			INNER JOIN BudgetCTE s on a.ParentId=s.Id
			where Isnull(a.IsDeleted,0) = 0 and Isnull(s.IsDeleted,0) = 0
		)
		 
		INSERT INTO @BudgetDetailData SELECT Distinct Id,ParentId,BudgetId FROM BudgetCTE Order by ID asc	
		Select @RowCount = COUNT(Id) From @BudgetDetailData 
		IF @RowCount > 0 
		BEGIN
		-- check if any of the selected budgets is a root budget
		Select @ParentIdCount=Count(*) From @BudgetDetailData where ParentId is null

		--- if there is a parent/root budget found than delete that from budget table and get the next id of next budget that we should show on UI after deletion of root budget
		If @ParentIdCount > 0 
		BEGIN
			UPDATE [dbo].[Budget] SET IsDeleted=1 Where Id = (Select Top(1) BudgetId From @BudgetDetailData)	
			SELECT Top(1) @NextBudgetId = Id from Budget where ClientId=@ClientId and IsDeleted = 0 order by Name asc 	
		END
		
		-- delete budget from budget details table
		UPDATE [dbo].[Budget_Detail] SET IsDeleted=1 Where Id in (Select Id From @BudgetDetailData)
		
		Select @LineItemBudgetCount = Count(*) From [LineItem_Budget] where BudgetDetailId in (
				Select Id from @BudgetDetailData)
		
		-- If any of the selected budgets are linked to a line item, update respective Line item detail records with other budget id
		if @LineItemBudgetCount > 0
		BEGIN
		
		-- get Other budget Ids
			Select @OtherBudgetId = ChildDetail.Id From Budget_Detail ChildDetail
			INNER JOIN Budget ParentDetail on  ParentDetail.Id = ChildDetail.BudgetId
			where (ParentDetail.IsDeleted=0 OR ParentDetail.IsDeleted is null) and (ChildDetail.IsDeleted=0  OR ChildDetail.IsDeleted is null)
			and ParentDetail.IsOther=1
			and ParentDetail.ClientId=@ClientId
			IF(@OtherBudgetId Is Not Null)
			BEGIN
				Update [dbo].[LineItem_Budget] SET BudgetDetailId=@OtherBudgetId
					Where Id In(
					Select Id From [dbo].[LineItem_Budget] Where BudgetDetailId in (
						Select Id from @BudgetDetailData))
			END
		END
		
	END
	RETURN @NextBudgetId -- return next budget Id.
	
END TRY
BEGIN CATCH
	SET @ErrorMeesgae= 'Object=DeleteBudget , ErrorMessage='+ERROR_MESSAGE()
	SELECT @ErrorMeesgae as 'ErrorMessage'
END CATCH
END


GO

----added by devanshi for #2804 Import marketing budget

/****** Object:  UserDefinedFunction [dbo].[fnGetBudgetForeCast_List]    Script Date: 12/01/2016 12:47:57 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetBudgetForeCast_List]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
DROP FUNCTION [dbo].[fnGetBudgetForeCast_List]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetBudgetForeCast_List]    Script Date: 12/01/2016 12:47:57 PM ******/
SET ANSI_NULLS ON
GO
-- =============================================
-- Author:		Devanshi
-- Create date: 01 Dec 2016
-- Description:	Function to get list of all childs for marketing budget
-- =============================================
CREATE FUNCTION [dbo].[fnGetBudgetForeCast_List]
(	
	@BudgetId int,
	@ClientId int
)
RETURNS TABLE 
AS
RETURN 
(
	WITH ForeCastBudgetDetail as
	(
	SELECT Budget_Detail.id,parentid from Budget_Detail
	INNER JOIN Budget on Budget_Detail.BudgetId=Budget.Id
	WHERE Budget.ClientId=@ClientId and Budget_Detail.IsDeleted=0 and
	 Budget_Detail.Id=@BudgetId and ParentID is null

    UNION ALL

    SELECT BD.id,BD.ParentID  FROM Budget_Detail BD 
	INNER JOIN Budget on BD.BudgetId=Budget.Id
	INNER JOIN ForeCastBudgetDetail FBD	  ON FBD.Id = BD.ParentID
	WHERE Budget.ClientId=@ClientId and BD.IsDeleted=0 
	)
	SELECT convert(nvarchar(20),a.id) as Id From ForeCastBudgetDetail a
	WHERE   NOT EXISTS ( SELECT *   FROM   ForeCastBudgetDetail b  WHERE  b.parentid = a.Id )
)


GO

/****** Object:  View [dbo].[BudgetDetail]    Script Date: 12/07/2016 12:46:36 PM ******/
IF EXISTS (SELECT 1 FROM sys.views WHERE OBJECT_ID=OBJECT_ID('BudgetDetail'))
BEGIN
	DROP VIEW [dbo].[BudgetDetail]
END
GO
GO

/****** Object:  View [dbo].[BudgetDetail]    Script Date: 12/07/2016 12:46:36 PM ******/
SET ANSI_NULLS ON
GO

CREATE VIEW [dbo].[BudgetDetail]
AS
	SELECT B.Id AS BudgetDetailId, Period, Budget ,Forecast,YEAR(B.CreatedDate) AS [Year], A.Id as Id
	FROM Budget A
	INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
	INNER JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
	WHERE A.IsDeleted = 0 

GO

/****** Object:  StoredProcedure [dbo].[PreCalFinanceGridForExistingData]    Script Date: 12/01/2016 12:46:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PreCalFinanceGridForExistingData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PreCalFinanceGridForExistingData]
GO

/****** Object:  StoredProcedure [dbo].[PreCalFinanceGridForExistingData]    Script Date: 12/01/2016 12:46:46 PM ******/
SET ANSI_NULLS ON
GO

-- =============================================
-- Author:		Devanshi
-- Create date: 28/11/2016
-- Description:	Dump existing budget,forecast,planned,actual data into pre calculated table
-- =============================================
CREATE PROCEDURE [dbo].[PreCalFinanceGridForExistingData]
	@BudgetId int=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    BEGIN
	-- Update Budget values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Budget = [Y1],Y2_Budget = [Y2],
					  Y3_Budget = [Y3],Y4_Budget = [Y4],
					  Y5_Budget = [Y5],Y6_Budget = [Y6],
					  Y7_Budget = [Y7],Y8_Budget = [Y8],
					  Y9_Budget = [Y9],Y10_Budget = [Y10],
					  Y11_Budget = [Y11],Y12_Budget = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly budget amount with pivoting
		SELECT * FROM 
		(
			SELECT BudgetDetailId, Period, Budget FROM BudgetDetail	WHERE  Id=@BudgetId
		) P
		PIVOT
		(
			MIN(BUDGET)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END

BEGIN
	-- Insert Budget records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Budget, Y2_Budget, Y3_Budget, Y4_Budget, Y5_Budget, 
													Y6_Budget, Y7_Budget,Y8_Budget, Y9_Budget, Y10_Budget, Y11_Budget, Y12_Budget)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly budget amount with pivoting
		SELECT  BudgetDetailId, [Year], Period, Budget 	FROM  BudgetDetail	WHERE  Id=@BudgetId
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

BEGIN
	-- Update Forecast values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Forecast = [Y1],Y2_Forecast = [Y2],
					  Y3_Forecast = [Y3],Y4_Forecast = [Y4],
					  Y5_Forecast = [Y5],Y6_Forecast = [Y6],
					  Y7_Forecast = [Y7],Y8_Forecast = [Y8],
					  Y9_Forecast = [Y9],Y10_Forecast = [Y10],
					  Y11_Forecast = [Y11],Y12_Forecast = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly forecast amount with pivoting
		SELECT * FROM 
		(
			SELECT  BudgetDetailId, Period, Forecast FROM  BudgetDetail	WHERE  Id=@BudgetId
		) P
		PIVOT
		(
			MIN(Forecast)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END
BEGIN
	-- Insert Forecast records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Forecast, Y2_Forecast, Y3_Forecast, Y4_Forecast, Y5_Forecast, 
													Y6_Forecast, Y7_Forecast,Y8_Forecast, Y9_Forecast, Y10_Forecast, Y11_Forecast, Y12_Forecast)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly forecast amount with pivoting
		SELECT  BudgetDetailId, [Year], Period, Budget FROM  BudgetDetail	WHERE  Id=@BudgetId
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

END

GO

/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetQuarter]    Script Date: 12/01/2016 12:44:37 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportMarketingBudgetQuarter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ImportMarketingBudgetQuarter]
GO
/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetQuarter]    Script Date: 12/01/2016 12:44:37 PM ******/
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[ImportMarketingBudgetQuarter]
@XMLData AS XML
,@ImportBudgetCol MarketingBudgetColumns READONLY
,@clientId INT
,@UserId int
,@BudgetDetailId BIGINT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
	----disable trigger For Marketing Budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount DISABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail DISABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END

	----end
	CREATE TABLE #tmpXmlData (ROWNUM BIGINT) --create # table because there are dynamic columns added as per file imported for marketing budget
	CREATE TABLE  #childtempData (ROWNUM BIGINT)

	DECLARE @Textboxcol nvarchar(max)=''
	DECLARE @UpdateColumn NVARCHAR(255)
	DECLARE @CustomEntityDeleteDropdownCount BIGINT
	DECLARE @CustomEntityDeleteTextBoxCount BIGINT
	DECLARE @IsCutomFieldDrp BIT
	DECLARE @GetBudgetAmoutData NVARCHAR(MAX)=''
	DECLARE @Count Int = 1;
	DECLARE @RowCount INT;
	DECLARE @ColName nvarchar(100)
	DECLARE @IsMonth nvarchar(100)

	-- Declare variable for time frame
	DECLARE @QFirst NVARCHAR(MAX)=''
	DECLARE @QSecond NVARCHAR(MAX)=''
	DECLARE @QThird NVARCHAR(MAX)=''
	-- Declare Variable for forecast/budget column is exist or not

	DECLARE @BudgetOrForecastIndex INT

	SELECT @RowCount = COUNT(*) FROM @ImportBudgetCol

	DECLARE @XmldataQuery NVARCHAR(MAX)=''

	DECLARE @tmpXmlDataAlter NVARCHAR(MAX)=''

	SET @XmldataQuery += 'SELECT ROW_NUMBER() OVER(ORDER BY(SELECT 100)),'
	DECLARE @ConcatColumns NVARCHAR(MAX)=''

	WHILE(@Count<=@RowCount)
	BEGIN
	SELECT @ColName = ColumnName FROM @ImportBudgetCol WHERE ColumnIndex=@Count

	SET @ConcatColumns  += 'pref.value(''(value)['+CAST(@Count AS VARCHAR(50))+']'', ''nvarchar(max)'') as ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'],' --set name of dynamic column

	SET @tmpXmlDataAlter+= ' ALTER TABLE #tmpXmlData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX)
							ALTER TABLE #childtempData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) ' -- add dynamic columns into table from XML data
	SET @Count=@Count+1;
	END
	SELECT @ConcatColumns=CASE WHEN LEN(@ConcatColumns)>1
						THEN LEFT(@ConcatColumns, LEN(@ConcatColumns) - 1)
							ELSE @ConcatColumns 
						END

	SET @XmldataQuery+= @ConcatColumns+' FROM @XmlData.nodes(''/data/row'') AS People(pref);'

	EXEC(@tmpXmlDataAlter)
	
	Declare @tmpChildBudgets table(Id int,ParentId int, BudgetId int)

	;WITH tblChild AS
	(
		SELECT Id,ParentId,BudgetId
		FROM Budget_Detail WHERE Id = @BudgetDetailId 
		UNION ALL
		SELECT Budget_Detail.Id,Budget_Detail.ParentId,Budget_Detail.BudgetId FROM Budget_Detail  
		CROSS APPLY (SELECT * FROM tblChild WHERE Budget_Detail.ParentId = tblChild.Id) tblChild
	)

	insert into @tmpChildBudgets
	select * from tblChild OPTION(MAXRECURSION 0)

	INSERT INTO #tmpXmlData EXECUTE sp_executesql @XmldataQuery, N'@XmlData XML OUT', @XmlData = @XmlData  OUT
	-- Remove Other Child items which are not related to parent
	DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
	left join @tmpChildBudgets tmpChildBudgets
	on CAST(tmpXmlData.[Id#1] AS INT)= tmpChildBudgets.Id
	WHERE tmpChildBudgets.Id IS NULL
	
	-- Remove View/None Permission budgets
	DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
	left join Budget_Permission BudgetPermission
	on CAST(tmpXmlData.[Id#1] AS INT)=BudgetPermission.BudgetDetailId AND UserId=@UserId
	AND (BudgetPermission.IsOwner=1 OR BudgetPermission.PermisssionCode=0)
	WHERE BudgetPermission.Id IS NULL

	-- Add only Child/Forecast item
	Insert  into #childtempData 
	select #tmpXmlData.* from #tmpXmlData
	inner join (select * from dbo.fnGetBudgetForeCast_List (@BudgetDetailId,@clientId)) child
	on CAST(#tmpXmlData.[Id#1] AS INT) = child.Id

	-- Update Process
	DECLARE @MonthNumber varchar(2) -- variable for month number while import budget data
	DECLARE @ConvertCount nvarchar(5) --variable to set column count with casting

	SET @Count=3;
	WHILE(@Count<=@RowCount)
	BEGIN

		SELECT  @Ismonth = CASE  WHEN  ISNULL(Month,'')='' THEN '' ELSE LTRIM(RTRIM([Month])) END
			,@UpdateColumn = CASE  WHEN  ISNULL(ColumnName,'')='' THEN '' ELSE [ColumnName] END
			,@BudgetOrForecastIndex = ColumnIndex FROM @ImportBudgetCol WHERE ColumnIndex=@Count
		
		SET @ConvertCount=CAST(@Count AS varchar(5))

		IF (@Ismonth<>'')
		BEGIN
			-- Set Time frame based on Quarters
			IF(@IsMonth='Q1')
			BEGIN	
				SET @QFirst ='Y1'
				SET @QSecond ='Y2'
				SET @QThird ='Y3'
			END

			IF(@IsMonth='Q2')
			BEGIN	
				SET @QFirst ='Y4'
				SET @QSecond ='Y5'
				SET @QThird ='Y6'
			END

			IF(@IsMonth='Q3')
			BEGIN	
				SET @QFirst ='Y7'
				SET @QSecond ='Y8'
				SET @QThird ='Y9'
			END

			IF(@IsMonth='Q4')
			BEGIN	
				SET @QFirst ='Y10'
				SET @QSecond ='Y11'
				SET @QThird ='Y12'
			END

		 -- Insert/Update values for budget and forecast
		 IF((@UpdateColumn='Budget' OR @UpdateColumn='Forecast'))
		 BEGIN
			IF(@Ismonth!='' AND @Ismonth!='Total' AND @Ismonth!='Unallocated')
			BEGIN
			
				SET @GetBudgetAmoutData=' 
				-- Update the Budget Detail amount table for Forecast and Budget values
				UPDATE BudgetDetailAmount
				SET BudgetDetailAmount.['+@UpdateColumn+']=TableData.['+@UpdateColumn+']
				FROM Budget_DetailAmount BudgetDetailAmount
				CROSS APPLY(
				SELECT * FROM (
				SELECT [Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',['+@QThird+'] = CASE WHEN SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+']) > MIN(['+@IsMonth+@UpdateColumn+'])
				THEN 
					CASE WHEN SUM(['+@QThird+@UpdateColumn+']) < (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
					THEN 0
				ELSE 
					SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
				END
				ELSE SUM(['+@QThird+@UpdateColumn+'])
				END  
				,['+@QSecond+'] = CASE WHEN SUM(['+@QThird+@UpdateColumn+']) < (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
				THEN 
					CASE WHEN SUM(['+@QSecond+@UpdateColumn+']) < (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+'])))
				THEN 0
				WHEN 0 > (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
					THEN 0
				ELSE SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))) 
				END
				ELSE SUM(['+@QSecond+@UpdateColumn+'])
				END
				,['+@QFirst+'] = CASE WHEN 0 > (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
							THEN SUM(['+@QFirst+@UpdateColumn+']) + (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
					  ELSE 
							CASE WHEN SUM(['+@QFirst+@UpdateColumn+']) > SUM(['+@QFirst+@UpdateColumn+']) - (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
							THEN SUM(['+@QFirst+@UpdateColumn+'])
							ELSE
							SUM(['+@QFirst+@UpdateColumn+']) - (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
							END
						END
				' END +'

				FROM(
				SELECT [Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',SUM(ISNULL(['+@QFirst+@UpdateColumn+'],0)) AS '+@QFirst+@UpdateColumn+'
				,SUM(ISNULL(['+@QSecond+@UpdateColumn+'],0)) AS '+@QSecond+@UpdateColumn+'
				,SUM(ISNULL(['+@QThird+@UpdateColumn+'],0)) AS '+@QThird+@UpdateColumn+'
				,MIN(ISNULL(['+@IsMonth+@UpdateColumn+'],0)) AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM
				(
				-- First Month Of Quarter
				SELECT tmpXmlData.[Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN '
				,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN ['+@UpdateColumn+'] > 0 
				--THEN 
				['+@UpdateColumn+']
				--ELSE 0 END 
				ELSE 0 END AS '+@QFirst+@UpdateColumn+'
				,NULL AS '+@QSecond+@UpdateColumn+'
				,NULL AS '+@QThird+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM #childtempData tmpXmlData
				CROSS APPLY(SELECT BudgetDetailId,Period
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
				FROM Budget_DetailAmount BudgetDetailAmount WHERE 
				tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QFirst+''') BudgetDetailAmount 
				-- Second Month Of Quarter
				UNION ALL
				SELECT tmpXmlData.[Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',NULL AS '+@QFirst+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN ['+@UpdateColumn+'] > 0 
				--THEN 
				['+@UpdateColumn+']
				--ELSE 0 END 
				ELSE 0 END AS '+@QSecond+@UpdateColumn+'
				,NULL AS '+@QThird+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM #childtempData tmpXmlData
				CROSS APPLY(SELECT BudgetDetailId,Period
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
				FROM Budget_DetailAmount BudgetDetailAmount WHERE 
				tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QSecond+''') BudgetDetailAmount 
				-- Third Month Of Quarter
				UNION ALL
				SELECT tmpXmlData.[Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',NULL AS '+@QFirst+@UpdateColumn+'
				,NULL AS '+@QSecond+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN ['+@UpdateColumn+'] > 0 
				--THEN 
				['+@UpdateColumn+']
				--ELSE 0 END 
				ELSE 0 END AS '+@QThird+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850 
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM #childtempData tmpXmlData
				CROSS APPLY(SELECT BudgetDetailId,Period
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
				FROM Budget_DetailAmount BudgetDetailAmount WHERE 
				tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QThird+''') BudgetDetailAmount 
				) AS A
				GROUP BY [Id#1]
				) AS QuarterData
				GROUP BY Id#1
				) As P
				UNPIVOT(
				['+@UpdateColumn+'] FOR Period
				IN(['+@QFirst+'],['+@QSecond+'],['+@QThird+'])
				) as TableData
				WHERE TableData.[Id#1]=BudgetDetailAmount.BudgetDetailId
				AND TableData.Period=BudgetDetailAmount.Period			
				) TableData

				-- Insert into the Budget Detail amount table for Forecast and Budget values if that period values are not exist
				INSERT INTO Budget_DetailAmount (BudgetDetailId,Period,'+@UpdateColumn+')
				SELECT tmpXmlData.[Id#1],'''+@QFirst+'''
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850 
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END
				FROM #childtempData tmpXmlData
				OUTER APPLY (SELECT A.BudgetDetailId,A.Period,A.Id FROM Budget_DetailAmount A
				WHERE A.BudgetDetailId = CAST(tmpXmlData.[Id#1] AS INT) 
				AND A.Period IN('''+@QFirst+''','''+@QSecond+''','''+@QThird+''')
				) A WHERE A.Id IS NULL 
				'
				EXECUTE sp_executesql @GetBudgetAmoutData
				SET @GetBudgetAmoutData=''
			END
			Else If(@Ismonth!='' AND @Ismonth='Total' AND @Ismonth!='Unallocated') -- update total budget and Forecast for child items
			Begin
				Declare @UpdateTotal NVARCHAR(max)
				IF(@UpdateColumn='Budget')
				begin
					set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalBudget= CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
					THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
					ELSE 0 END 
					from Budget_Detail BudgetDetail
					inner join (SELECT * FROM #childtempData) tmpXmlData on BudgetDetail.Id=CAST(tmpXmlData.[Id#1] AS INT)'
					exec sp_executesql @UpdateTotal
				end
				Else
				begin
					set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalForeCast=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
					THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT)
					ELSE 0 END 
					from Budget_Detail BudgetDetail
					inner join (SELECT * FROM #childtempData) tmpXmlData on BudgetDetail.Id=CAST(tmpXmlData.[Id#1] AS INT)'
					exec sp_executesql @UpdateTotal
				End
			End
		END
		--END
		END
		-- Custom Columns
		IF((@UpdateColumn!='Budget' OR @UpdateColumn!='Forecast'))
		 BEGIN
			IF(@Ismonth='' AND @Ismonth!='Total' AND @Ismonth!='Unallocated')
			BEGIN
				SELECT @IsCutomFieldDrp = CASE WHEN CustomFieldType.Name='TextBox' THEN 0 ELSE 1 END FROM CustomField 
				CROSS APPLY(SELECT CustomFieldType.Name,CustomFieldType.CustomFieldTypeId FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) CustomFieldType
				WHERE CustomField.Name=''+@UpdateColumn+'' AND CustomField.ClientId=@ClientId AND CustomField.IsDeleted=0 AND CustomField.EntityType='Budget'

				-- Insert/Update/Delete values for custom field as dropdown
				IF(@IsCutomFieldDrp=1)
				BEGIN
					SET @GetBudgetAmoutData+=' 
					-- Get List of record which need to delete from CustomField Entity Table

					INSERT INTO @tmpCustomDeleteDropDown 
					SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
					CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''')='''') tmpXmlData
					CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
					CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
					AND CustomField.EntityType=''Budget'') CustomField
					WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
					AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

					SELECT @CustomEntityDeleteDropdownCount=COUNT(*) FROM @tmpCustomDeleteDropDown tmpCustomDelete

					-- Delete from CustomField Entity Table
					DELETE TOP(@CustomEntityDeleteDropdownCount) FROM CustomField_Entity
					WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM @tmpCustomDeleteDropDown)
					AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM @tmpCustomDeleteDropDown)

					-- Insert new values of CustomField_Entity tables 
					INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
					SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,CustOpt.CustomFieldOptionId,'''+CAST(@UserId AS VARCHAR(50))+''',GETDATE() FROM #tmpXmlData tmpXmlData 
					CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
					CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
					CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
					AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) CustOpt
					OUTER APPLY (
					SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
					AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
					)CustomFieldEntity WHERE CustomFieldEntity.CustomFieldEntityId IS NULL
				
					-- Update values of CustomField_Entity tables 
					UPDATE CustomFieldEntity SET Value=CustOpt.CustomFieldOptionId FROM
					CustomField_Entity CustomFieldEntity
					CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
					CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
					CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
					AND CustomField.EntityType=''Budget'') CustomField
					CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
					AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) CustOpt 
					WHERE CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
					'
				
				END

				-- Insert/Update/Delete values for custom field as Textbox
				IF(@IsCutomFieldDrp<>1)
				BEGIN
					SET @GetBudgetAmoutData+=' 
					-- Get List of record which need to delete from CustomField Entity Table
					INSERT INTO @tmpCustomDeleteTextBox 
					SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
					CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''')='''') tmpXmlData
					CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
					CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
					AND CustomField.EntityType=''Budget'') CustomField
					WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

					SELECT @CustomEntityDeleteTextBoxCount=COUNT(*) FROM @tmpCustomDeleteTextBox tmpCustomDelete
				
					-- Delete from CustomField Entity Table
					DELETE TOP(@CustomEntityDeleteTextBoxCount) FROM CustomField_Entity
					WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM @tmpCustomDeleteTextBox)
					AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM @tmpCustomDeleteTextBox)

					-- Insert new values of CustomField_Entity tables 
					INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
					SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''+CAST(@UserId AS VARCHAR(50))+''',GETDATE() FROM #tmpXmlData tmpXmlData 
					CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
					CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
					OUTER APPLY (
					SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
					AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
					)CustomFieldEntity WHERE tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] IS NOT NULL AND CustomFieldEntity.CustomFieldEntityId IS NULL 

					-- Update values of CustomField_Entity tables 
					UPDATE CustomFieldEntity SET Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] FROM
					CustomField_Entity CustomFieldEntity
					CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
					CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
					CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
					AND CustomField.EntityType=''Budget'') CustomField 
					WHERE tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] IS NOT NULL
					AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
					'

				END
			
			END
		END

		SET @Ismonth=''
		SET @Count=@Count+1;
		SET @MonthNumber=0;
	END

	set @GetBudgetAmoutData='
	DECLARE  @tmpCustomDeleteDropDown TABLE(EntityId BIGINT,CustomFieldId BIGINT)
	DECLARE  @tmpCustomDeleteTextBox TABLE(EntityId BIGINT,CustomFieldId BIGINT)'
	+@GetBudgetAmoutData
	EXECUTE sp_executesql @GetBudgetAmoutData, N'@CustomEntityDeleteDropdownCount BIGINT,@CustomEntityDeleteTextBoxCount BIGINT OUT', @CustomEntityDeleteDropdownCount = @CustomEntityDeleteDropdownCount,@CustomEntityDeleteTextBoxCount = @CustomEntityDeleteTextBoxCount OUT

	---call sp of pre calculation for marketing budget
	Declare @BudgetId int=(select BudgetId from budget_detail where id=@BudgetDetailId)
	exec PreCalFinanceGridForExistingData @BudgetId
	-----end

	----Enable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount ENABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail ENABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
END TRY
BEGIN CATCH
	----Enable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount ENABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail ENABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
END CATCH
END



GO



/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetMonthly]    Script Date: 12/01/2016 12:41:52 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportMarketingBudgetMonthly]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ImportMarketingBudgetMonthly]
GO

/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetMonthly]    Script Date: 12/01/2016 12:41:52 PM ******/
SET ANSI_NULLS ON
GO


CREATE  PROCEDURE [dbo].[ImportMarketingBudgetMonthly]
@XMLData AS XML
,@ImportBudgetCol MarketingBudgetColumns READONLY
,@clientId INT
,@UserId INT
,@BudgetDetailId BIGINT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
	----disable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
	ALTER TABLE Budget_DetailAmount DISABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
	ALTER TABLE Budget_Detail DISABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
	CREATE TABLE  #tmpXmlData  (ROWNUM BIGINT) --create # table because there are dynamic columns added as per file imported for marketing budget
	CREATE TABLE  #childtempData (ROWNUM BIGINT)
	DECLARE @Textboxcol nvarchar(max)=''
	DECLARE @UpdateColumn NVARCHAR(255)
	DECLARE @CustomEntityDeleteDropdownCount BIGINT
	DECLARE @CustomEntityDeleteTextBoxCount BIGINT
	DECLARE @IsCutomFieldDrp BIT
	DECLARE @GetBudgetAmoutData NVARCHAR(MAX)=''
	DECLARE @Count Int = 1;
	DECLARE @RowCount INT;
	DECLARE @ColName nvarchar(100)
	DECLARE @IsMonth nvarchar(100)

	SELECT @RowCount = COUNT(*) FROM @ImportBudgetCol
	DECLARE @XmldataQuery NVARCHAR(MAX)=''

	DECLARE @tmpXmlDataAlter NVARCHAR(MAX)=''

	SET @XmldataQuery += 'SELECT ROW_NUMBER() OVER(ORDER BY(SELECT 100)),'
	DECLARE @ConcatColumns NVARCHAR(MAX)=''

	WHILE(@Count<=@RowCount)
	BEGIN
		SELECT @ColName = ColumnName FROM @ImportBudgetCol WHERE ColumnIndex=@Count

		SET @ConcatColumns  += 'pref.value(''(value)['+CAST(@Count AS VARCHAR(50))+']'', ''nvarchar(max)'') as ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'],'

		SET @tmpXmlDataAlter+= ' ALTER TABLE #tmpXmlData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) 
								 ALTER TABLE #childtempData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	SELECT @ConcatColumns=CASE WHEN LEN(@ConcatColumns)>1
						THEN LEFT(@ConcatColumns, LEN(@ConcatColumns) - 1)
							ELSE @ConcatColumns 
						END

	SET @XmldataQuery+= @ConcatColumns+' FROM	@XmlData.nodes(''/data/row'') AS People(pref);'

	EXEC(@tmpXmlDataAlter)
	Declare @tmpChildBudgets table(Id int,ParentId int, BudgetId int)

	;WITH tblChild AS
	(
		SELECT Id,ParentId,BudgetId	FROM Budget_Detail WHERE Id = @BudgetDetailId 
		UNION ALL
		SELECT Budget_Detail.Id,Budget_Detail.ParentId,Budget_Detail.BudgetId FROM Budget_Detail  
		CROSS APPLY (SELECT * FROM tblChild WHERE Budget_Detail.ParentId = tblChild.Id) tblChild
	)
	insert into @tmpChildBudgets
	select *FROM tblChild
	OPTION(MAXRECURSION 0)
	

	INSERT INTO #tmpXmlData EXECUTE sp_executesql @XmldataQuery, N'@XmlData XML OUT', @XmlData = @XmlData  OUT
		-- Remove Other Child items which are not related to parent
		DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
		left join @tmpChildBudgets tmpChildBudgets
		on CAST(tmpXmlData.[Id#1] AS INT)= tmpChildBudgets.Id
		WHERE tmpChildBudgets.Id IS NULL

		-- Remove View/None Permission budgets
		DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
		left join Budget_Permission BudgetPermission
		on CAST(tmpXmlData.[Id#1] AS INT)=BudgetPermission.BudgetDetailId AND UserId=@UserId
		AND (BudgetPermission.IsOwner=1 OR BudgetPermission.PermisssionCode=0)
		WHERE BudgetPermission.Id IS NULL

		-- Add only Child/Forecast item and store into # table because there are dynamic columns added as per imported file.
		Insert into #childtempData 
		select #tmpXmlData.* from #tmpXmlData
		inner join (select * from dbo.fnGetBudgetForeCast_List (@BudgetDetailId,@clientId)) child
		on CAST(#tmpXmlData.[Id#1] AS INT) = child.Id

	-- Update Process
	DECLARE @MonthNumber varchar(2) -- variable for month number while import budget data
	DECLARE @ConvertCount nvarchar(5) --variable to set column count with casting
	SET @Count=2;
																																																																																																																																																																												WHILE(@Count<=@RowCount)
	BEGIN

	SELECT @UpdateColumn=[ColumnName],@Ismonth = CASE  WHEN  ISNULL(Month,'')='' THEN '' ELSE [Month] END FROM @ImportBudgetCol WHERE ColumnIndex=@Count
	-- Insert/Update values for budget and forecast
	SET @ConvertCount=CAST(@Count AS varchar(5))

	 IF((@UpdateColumn='Budget' OR @UpdateColumn='Forecast'))
	 BEGIN
		IF(@Ismonth!='' AND @Ismonth!='Total' AND @Ismonth!='Unallocated')
		BEGIN
			SELECT  @MonthNumber = CAST(DATEPART(MM,''+@IsMonth+' 01 1990') AS varchar(2))
			DECLARE @temp nvarchar(max)=''
			SET @GetBudgetAmoutData+=' 
			-- Update the Budget Detail amount table for Forecast and Budget values
			UPDATE BudgetDetailAmount SET ['+(@UpdateColumn)+']=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
			THEN 
			--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
			--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) > 0
			--THEN 
			CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
			--ELSE 0 END
			ELSE 0 END 
			FROM 
			Budget_DetailAmount BudgetDetailAmount
			CROSS APPLY (SELECT * FROM #childtempData tmpXmlData WHERE BudgetDetailAmount.BudgetDetailId=CAST([Id#1] AS INT) 
			AND BudgetDetailAmount.Period=''Y'+@MonthNumber+''' AND ISNULL(['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')<>'''' 
			AND CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
			THEN 
			--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
			--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) > 0
			--THEN 
			CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
			--ELSE 0 END
			ELSE 0 END <> ISNULL(['+(@UpdateColumn)+'],0)
			) tmpXmlData
			
			-- Insert into the Budget Detail amount table for Forecast and Budget values if that period values are not exist
			INSERT INTO Budget_DetailAmount (BudgetDetailId,Period,['+@UpdateColumn+'])
			SELECT  tmpXmlData.[Id#1] 
			,''Y'+@MonthNumber+'''
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
			THEN 
			--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
			--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) > 0
			--THEN 
			CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
			--ELSE 0 END
			ELSE 0 END
			FROM #childtempData  tmpXmlData
			OUTER APPLY (SELECT A.BudgetDetailId,A.Period,A.Id FROM Budget_DetailAmount A
			WHERE A.BudgetDetailId = CAST(tmpXmlData.[Id#1] AS INT) 
			AND A.Period=''Y'+@MonthNumber+''')
			A WHERE A.Id IS NULL 
			
			 '
		END
		Else If(@Ismonth!='' AND @Ismonth='Total' AND @Ismonth!='Unallocated') -- Update total budget and Forecast for child items
		Begin
			Declare @UpdateTotal NVARCHAR(max)
			IF(@UpdateColumn='Budget')
			begin
				set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalBudget=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
				THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
				ELSE 0 END 
				from Budget_Detail BudgetDetail
				inner join (SELECT * FROM #childtempData) tmpXmlData on BudgetDetail.Id=CAST(tmpXmlData.[Id#1] AS INT)'
				exec sp_executesql @UpdateTotal
			end
			Else
			begin
				set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalForeCast=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
				THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT)
				ELSE 0 END 
				from Budget_Detail BudgetDetail
				inner join (SELECT * FROM #childtempData) tmpXmlData on BudgetDetail.Id=CAST(tmpXmlData.[Id#1] AS INT)'
				exec sp_executesql @UpdateTotal
			End

		End
	END

	-- Custom Columns
	 IF((@UpdateColumn!='Budget' OR @UpdateColumn!='Forecast'))
	 BEGIN
		IF(@Ismonth='' AND @Ismonth!='Total' AND @Ismonth!='Unallocated')
		BEGIN

			SELECT @IsCutomFieldDrp = CASE WHEN CustomFieldType.Name='TextBox' THEN 0 ELSE 1 END FROM CustomField 
			CROSS APPLY(SELECT CustomFieldType.Name,CustomFieldType.CustomFieldTypeId FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) CustomFieldType
			WHERE CustomField.Name=''+@UpdateColumn+'' AND CustomField.ClientId=@ClientId AND CustomField.IsDeleted=0 AND CustomField.EntityType='Budget'
			
			-- Insert/Update/Delete values for custom field as dropdown
			IF(@IsCutomFieldDrp=1)
			BEGIN
				SET @GetBudgetAmoutData+=' 
				-- Get List of record which need to delete from CustomField Entity Table

				INSERT INTO @tmpCustomDeleteDropDown 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteDropdownCount=COUNT(*) FROM @tmpCustomDeleteDropDown tmpCustomDelete

				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteDropdownCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM @tmpCustomDeleteDropDown)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM @tmpCustomDeleteDropDown)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,CustOpt.CustomFieldOptionId,'''+CAST(@UserId AS VARCHAR(50))+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) CustOpt
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE CustomFieldEntity.CustomFieldEntityId IS NULL
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=CustOpt.CustomFieldOptionId FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) CustOpt 
				WHERE CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				'
			END

			-- Insert/Update/Delete values for custom field as Textbox
			IF(@IsCutomFieldDrp<>1)
			BEGIN
				SET @GetBudgetAmoutData+='  
				-- Get List of record which need to delete from CustomField Entity Table
				INSERT INTO @tmpCustomDeleteTextBox 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteTextBoxCount=COUNT(*) FROM @tmpCustomDeleteTextBox tmpCustomDelete
				
				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteTextBoxCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM @tmpCustomDeleteTextBox)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM @tmpCustomDeleteTextBox)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''+CAST(@UserId AS VARCHAR(50))+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] IS NOT NULL AND CustomFieldEntity.CustomFieldEntityId IS NULL 
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField 
				WHERE tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] IS NOT NULL
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
				'

			END
			
		END
		
	END
	SET @Ismonth=''
	SET @Count=@Count+1;
	SET @MonthNumber=0;
END
	set @GetBudgetAmoutData='
		Declare @tmpCustomDeleteDropDown TABLE(EntityId BIGINT,CustomFieldId BIGINT)
		Declare @tmpCustomDeleteTextBox TABLE(EntityId BIGINT,CustomFieldId BIGINT)'
		+ @GetBudgetAmoutData

	EXECUTE sp_executesql @GetBudgetAmoutData, N'@CustomEntityDeleteDropdownCount BIGINT,@CustomEntityDeleteTextBoxCount BIGINT OUT', @CustomEntityDeleteDropdownCount = @CustomEntityDeleteDropdownCount,@CustomEntityDeleteTextBoxCount = @CustomEntityDeleteTextBoxCount OUT

	---call sp of pre calculation for marketing budget
	Declare @BudgetId int=(select BudgetId from budget_detail where id=@BudgetDetailId)
	exec PreCalFinanceGridForExistingData @BudgetId
	---end
	----Enable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount ENABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail ENABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
END TRY
BEGIN CATCH
	----Enable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount ENABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail ENABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
END CATCH
END



GO
------end-------

-- Start - Added by Arpita Soni for Ticket #2788
-- DROP AND CREATE STORED PROCEDURE [dbo].[GetHeaderValuesForFinance]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetHeaderValuesForFinance]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetHeaderValuesForFinance]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/30/2016
-- Description:	Get HUD values for finance grid
-- =============================================
-- [dbo].[GetHeaderValuesForFinance] 1,'2,4,6,12,13,17,22,24,27,30,32,37,38,42,43,49,53,58,59,641,62,66,71,77,79,80,82,643,85,86,87,88,89,92,96,97,101,106,107,617,646,110,113,114,624,120,125,129,131,133,619,134,136,137,139,647,142,144,146,150,155,157,158,160,163,165,166,645,169,174,175,176,179,186,623,188,190,191,192,194,195,208,210,211,213,214,219,220,221,223,224,621,243,245,250,626,254,256,258,259,260,625,261,262,269,270,272,273,275,276,277,281,288,289,291,292,293,295,298,299,301,303,304,313,314,315,318,320,322,324,327,330,339,341,342,344,349,355,359,361,362,365,366,370,374,378,382,387,389,393,394,398,399,403,406,407,408,412,415,419,426,427,428,431,434,435,440,446,447,449,450,451,454,457,459,462,465,466,467,468,470,471,472,475,481,485,486,487,488,492,493,495,497,503,504,505,512,514,516,517,518,519,520,521,526,527,533,536,537,542,544,545,546,549,551,555,556,561,566,573,574,578,584,585,586,588,590,649,593,595,602,608',1
CREATE PROCEDURE [dbo].[GetHeaderValuesForFinance]
	-- Add the parameters for the stored procedure here
	@BudgetId		INT,
	@lstUserIds		NVARCHAR(MAX)='',
	@CurrencyRate	FLOAT = 1.0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Budget		FLOAT,
			@Forecast	FLOAT,
			@Planned	FLOAT,
			@Actual		FLOAT

	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	-- Get Budget, Forecast for Header
	SELECT @Budget = SUM(TotalBudget) * @CurrencyRate, @Forecast = SUM(TotalForecast) * @CurrencyRate
	FROM Budget_Detail BD
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	WHERE BudgetId = @BudgetId AND IsDeleted = 0

	-- Get Planned Cost for Header
	SELECT @Planned = SUM(PCPTL.Cost * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0

	-- Get Actual for Header
	SELECT @Actual = SUM(PCPTLA.Value * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0
	AND REPLACE(Period,'Y','') < 13

	SELECT ISNULL(@Budget,0) AS Budget, ISNULL(@Forecast,0) AS Forecast, ISNULL(@Planned,0) AS Planned, ISNULL(@Actual,0) AS Actual
END
GO


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

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalUserCountMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalUserCountMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jaymin Modi
-- Create date: 02/Dec/2016
-- Description:	Trigger which Update user count into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalUserCountMarketingBudget]
       ON [dbo].[Budget_Permission]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @BudgetDetailId INT,
			@Year INT,
			@Users INT = 0

	--Get BudgetDetailId and User Count
	SELECT @BudgetDetailId = BP.BudgetDetailId, @Users = ISNULL(COUNT(DISTINCT BP.UserId),0)
	FROM Budget_Permission BP
	INNER JOIN INSERTED I ON BP.BudgetDetailId = I.BudgetDetailId
	GROUP BY BP.BudgetDetailId 

	--Update User Count By BudgetDetailId
	UPDATE [MV].[PreCalculatedMarketingBudget] SET Users = @Users WHERE BudgetDetailId = @BudgetDetailId

	
END
GO

-- End - Added by Arpita Soni for Ticket #2788

--Added by devanshi for #2847: get heads up value for marketing budget line items

/****** Object:  StoredProcedure [dbo].[GetHeaderValuesForFinanceLineItems]    Script Date: 12/09/2016 5:00:55 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetHeaderValuesForFinanceLineItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetHeaderValuesForFinanceLineItems]
GO

/****** Object:  StoredProcedure [dbo].[GetHeaderValuesForFinanceLineItems]    Script Date: 12/09/2016 5:00:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 12/09/2016
-- Description:	Get HUD values for Line item grid for marketing budget
-- =============================================
CREATE PROCEDURE [dbo].[GetHeaderValuesForFinanceLineItems]
	-- Add the parameters for the stored procedure here
	@BudgetDetailId		INT,
	@lstUserIds		NVARCHAR(MAX)='',
	@CurrencyRate	FLOAT = 1.0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Budget		FLOAT,
			@Forecast	FLOAT,
			@Planned	FLOAT,
			@Actual		FLOAT

	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	-- Get  Forecast for Header
	SELECT  @Forecast = SUM(TotalForecast) * @CurrencyRate
	FROM Budget_Detail BD
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	WHERE Id = @BudgetDetailId AND IsDeleted = 0

	-- Get Planned Cost for Header
	SELECT @Planned = SUM(PCPTL.Cost * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.Id = @BudgetDetailId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0

	-- Get Actual for Header
	SELECT @Actual = SUM(PCPTLA.Value * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.Id = @BudgetDetailId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0
	AND REPLACE(Period,'Y','') < 13

	SELECT ISNULL(@Budget,0) AS Budget, ISNULL(@Forecast,0) AS Forecast, ISNULL(@Planned,0) AS Planned, ISNULL(@Actual,0) AS Actual
END

GO


--end

-- ===========================Please put your script above this script=============================
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](255) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](255) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'November.2016'
set @version = 'November.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO

