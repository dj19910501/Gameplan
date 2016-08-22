
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'INT.PullMQL') AND xtype IN (N'P'))
    DROP PROCEDURE [INT].[PullMQL]
GO

CREATE PROCEDURE [INT].PullMQL(@DataSource NVARCHAR(255), @ClientID NVARCHAR(255), @UserID NVARCHAR(255))
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title MQL which match with measure sfdc tactics	
	SET @CustomQuery='
	
		DELETE dbo.Plan_Campaign_Program_Tactic_Actual WHERE ModifiedBy = ''' + @UserID+ ''' AND StageTitle = ''MQL''

		UPDATE dbo.Plan_Campaign_Program_Tactic_Actual
		SET Actualvalue = V.ActualValue
			, ModifiedBy = ''' + @UserID+ ''' 
			, ModifiedDate = GETDATE()
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + '.[dbo].vTacticMQL V ON V.PulleeID = T.IntegrationInstanceTacticId
		WHERE dbo.Plan_Campaign_Program_Tactic_Actual.PlanTacticId = T.PlanTacticId
			  AND dbo.Plan_Campaign_Program_Tactic_Actual.StageTitle = V.StageTitle
			  AND dbo.Plan_Campaign_Program_Tactic_Actual.Period = [INT].Period(T.StartDate, V.ModifiedDate) 

		INSERT INTO  dbo.Plan_Campaign_Program_Tactic_Actual 
		SELECT T.PlanTacticId
				, V.StageTitle
				, [INT].Period(T.StartDate, V.ModifiedDate)
				, V.ActualValue
				, GETDATE()
				, ''' + @UserID + '''
				, NULL
				, NULL 
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + '.[dbo].vTacticMQL V ON V.PulleeID = T.IntegrationInstanceTacticId
			 LEFT JOIN dbo.Plan_Campaign_Program_Tactic_Actual A ON A.PlanTacticId = T.PlanTacticId AND A.StageTitle = V.StageTitle AND A.Period = V.Period 
		WHERE A.PlanTacticId IS NULL

	'

		EXEC (@CustomQuery)

END