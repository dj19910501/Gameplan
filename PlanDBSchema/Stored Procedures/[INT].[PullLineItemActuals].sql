IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[PullLineItemActuals]'))
	DROP PROCEDURE [INT].[PullLineItemActuals]
GO

CREATE PROCEDURE [INT].[PullLineItemActuals](@DataSource NVARCHAR(255), @ClientID INT, @UserID INT, @IntegrationInstanceID INT)
AS
BEGIN 
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='

		DECLARE @Updated INT;
		DECLARE @Inserted INT;
		DECLARE @Start DATETIME = GETDATE();

		UPDATE A
			SET A.Value = V.Amount
				, A.CreatedDate = GETDATE()
				, A.CreatedBy = '+STR(@UserId)+'
		FROM  Plan_Campaign_Program_Tactic_LineItem_Actual A 
				JOIN Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = A.PlanLineItemId
				JOIN Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
				JOIN '+@DataSource+' V ON V.LineItemID = A.PlanLineItemId AND A.Period = [INT].Period(T.StartDate, V.AccountingDate)

		SET @Updated = @@ROWCOUNT; 

		INSERT INTO  Plan_Campaign_Program_Tactic_LineItem_Actual (PlanLineItemId, Period, Value, CreatedDate, CreatedBy)
		SELECT	V.LineItemId, [INT].Period(T.StartDate, V.AccountingDate), V.Amount, GETDATE(), '+STR(@UserId)+'
		FROM	'+@DataSource+'  V
					JOIN Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = V.LineItemId
					JOIN Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual A ON V.LineItemID = A.PlanLineItemId AND [INT].Period(T.StartDate, V.AccountingDate) = A.Period 
		WHERE A.PlanLineItemId IS NULL

		SET @Inserted = @@ROWCOUNT;

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
			, ''Pulled LineItemActual From'' +''' + @DataSource + '''+STR(@Updated) + '' Updated.'' + STR(@Inserted) + '' Inserted.'' 
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