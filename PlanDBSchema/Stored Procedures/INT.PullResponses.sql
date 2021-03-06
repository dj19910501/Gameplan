
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'INT.PullResponses') AND xtype IN (N'P'))
    DROP PROCEDURE [INT].[PullResponses]
GO

CREATE PROCEDURE [INT].PullResponses(@DataSource NVARCHAR(255), @ClientID NVARCHAR(255), @UserID NVARCHAR(255))
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='
	
		DELETE A
		FROM dbo.Plan_Campaign_Program_Tactic_Actual A JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = A.PlanTacticId
				LEFT JOIN ' + @DataSource + '.[dbo].[vTacticResponses] R ON R.PulleeID = T.IntegrationInstanceTacticId 
		WHERE  A.ModifiedBy = ''' + @UserID + ''' AND A.StageTitle = ''ProjectedStageValue'' AND R.PulleeID IS NULL 

		UPDATE dbo.Plan_Campaign_Program_Tactic_Actual
		SET Actualvalue = V.ActualValue
			, ModifiedBy = ''' + @UserID+ ''' 
			, ModifiedDate = GETDATE()
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + '.[dbo].vTacticResponses V ON V.PulleeID = T.IntegrationInstanceTacticId
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
				, GETDATE()
				, ''' + @UserID + '''
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + '.[dbo].vTacticResponses V ON V.PulleeID = T.IntegrationInstanceTacticId
			 LEFT JOIN dbo.Plan_Campaign_Program_Tactic_Actual A ON A.PlanTacticId = T.PlanTacticId AND A.StageTitle = V.StageTitle AND A.Period = [INT].Period(T.StartDate, V.ModifiedDate) 
		WHERE A.StageTitle IS NULL 

	'

		EXEC (@CustomQuery)

END