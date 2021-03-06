
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'INT.PullActuals') AND xtype IN (N'P'))
    DROP PROCEDURE [INT].[PullActuals]
GO

CREATE PROCEDURE [INT].PullActuals(@DataSource NVARCHAR(255), @StageTitle NVARCHAR(255),  @ClientID NVARCHAR(255), @UserID NVARCHAR(255), @IntegrationInstanceID INT)
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='
	
		DECLARE @Start DATETIME = GETDATE();
		DECLARE @Deleted INT = 0;
		DECLARE @Updated INT = 0;
		DECLARE @Inserted INT = 0;

		DELETE A
		FROM dbo.Plan_Campaign_Program_Tactic_Actual A JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = A.PlanTacticId
				LEFT JOIN ' + @DataSource + ' R ON R.PulleeID = T.IntegrationInstanceTacticId 
		WHERE  (A.CreatedBy = ''' + @UserID + ''' OR A.ModifiedBy = ''' + @UserID + ''' ) AND A.StageTitle = ''' + @StageTitle + ''' AND R.PulleeID IS NULL 

		SET @Deleted = @@ROWCOUNT;

		UPDATE dbo.Plan_Campaign_Program_Tactic_Actual
		SET Actualvalue = V.ActualValue
			, CreatedDate = V.ModifiedDate
			, ModifiedBy = ''' + @UserID+ ''' 
			, ModifiedDate = GETDATE()
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + ' V ON V.PulleeID = T.IntegrationInstanceTacticId
		WHERE dbo.Plan_Campaign_Program_Tactic_Actual.PlanTacticId = T.PlanTacticId
			  AND dbo.Plan_Campaign_Program_Tactic_Actual.StageTitle = V.StageTitle
			  AND dbo.Plan_Campaign_Program_Tactic_Actual.Period = [INT].Period(T.StartDate, V.ModifiedDate) 

		SET @Updated = @@ROWCOUNT;

		INSERT INTO  dbo.Plan_Campaign_Program_Tactic_Actual 
		SELECT T.PlanTacticId
				, V.StageTitle
				, [INT].Period(T.StartDate, V.ModifiedDate)
				, V.ActualValue
				, V.ModifiedDate
				, ''' + @UserID + '''
				, GETDATE()
				, ''' + @UserID + '''
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + ' V ON V.PulleeID = T.IntegrationInstanceTacticId
			 LEFT JOIN dbo.Plan_Campaign_Program_Tactic_Actual A ON A.PlanTacticId = T.PlanTacticId AND A.StageTitle = V.StageTitle AND A.Period = [INT].Period(T.StartDate, V.ModifiedDate) 
		WHERE A.StageTitle IS NULL 

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
			, ''Pulled '' + ''' + @StageTitle + '. From ' + @DataSource + ''' + STR(@Deleted) + '' Deleted.'' + STR(@Updated) + '' Updated.'' + STR(@Inserted) + '' Inserted.'' 
			, GETDATE()
			, ''' + @UserID + '''
			, 1  

	'

		--PRINT @CustomQuery;
		BEGIN TRY 
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