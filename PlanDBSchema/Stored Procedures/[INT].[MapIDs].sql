IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[MapIDs]'))
	DROP PROCEDURE [INT].[MapIDs]
GO

CREATE PROCEDURE [INT].[MapIDs](@DataSource NVARCHAR(255), @ClientID INT, @UserID INT, @IntegrationInstanceID INT)
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--Mapping custom names to integration IDs
	SET @CustomQuery='
	
		DECLARE @Start DATETIME = GETDATE();
		DECLARE @Updated INT = 0;

		UPDATE T  
		SET T.IntegrationInstanceTacticId = M.PulleeID
			, ModifiedBy = ' + STR(@UserID)+ ' 
			, ModifiedDate = GETDATE()

		FROM Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + ' M ON M.TacticCustomName = T.TacticCustomName
		WHERE T.TacticCustomName = T.Title + ''_'' + CAST(T.PlanTacticId AS NVARCHAR(32)) AND T.IntegrationInstanceTacticId IS NULL

		SET @Updated = @@ROWCOUNT;

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
			, ''Pulled IDs from '' + ''' + @DataSource + ''' + STR(@Updated) + '' IDs Updated.'' 
			, GETDATE()
			, ' + STR(@UserID) + '
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
