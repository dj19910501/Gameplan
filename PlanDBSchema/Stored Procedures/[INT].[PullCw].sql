-- =============================================
-- Author:	Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp contains logic to insert actuals from measure to plan database
-- =============================================
ALTER PROCEDURE [INT].[PullCw]
	(
	@MeasureSFDCActualTableName NVARCHAR(255)
	)
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
	
	--Remove plan tactic actuals for stage title revenue and CW which match with measure sfdc tactics
	SET @CustomQuery=' DELETE A 
		FROM Plan_Campaign_Program_Tactic_Actual A 
		INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=A.PlanTacticId
		INNER JOIN '+@MeasureSFDCActualTableName+' T ON SUBSTRING(ISNULL(T.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) AND T.StageTitle=''CW''
		WHERE A.StageTitle=''Revenue'' OR A.StageTitle=''CW'';'

	EXEC (@CustomQuery)
	
	--insert plan tactic actuals for stage title CW which match with measure sfdc tactics	
	SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
						SELECT PT.PlanTacticId,
								M.StageTitle,
								CASE WHEN CONVERT(INT, DATEPART(YEAR, PT.StartDate))<Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))
								THEN ''Y''+CONVERT (NVARCHAR(4),((Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))-CONVERT(INT, DATEPART(YEAR, PT.StartDate)))*12)+ CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
								ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period,
									
								M.ActualValue,
								GETDATE() as CreatedDate,
								PT.Createdby,
								NULL ModifiedDate,
								NULL ModifiedBy		
						From '+@MeasureSFDCActualTableName+' M 
						INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16)
						AND PT.IsDeleted=0
						AND	PT.IsDeployedToIntegration=''1'' 
						AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
						WHERE M.StageTitle=''CW'''

		EXEC (@CustomQuery)

		--insert plan tactic actuals for stage title Revenue which match with measure sfdc tactics	
		SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
						  SELECT PT.PlanTacticId,
								M.StageTitle,
								CASE WHEN CONVERT(INT, DATEPART(YEAR, PT.StartDate))<Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))
								THEN ''Y''+CONVERT (NVARCHAR(4),((Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))-CONVERT(INT, DATEPART(YEAR, PT.StartDate)))*12)+ CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
								ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period,
								
								M.ActualValue,
								GETDATE() as CreatedDate,
								PT.Createdby,
								NULL ModifiedDate,
								NULL ModifiedBy	
							From '+@MeasureSFDCActualTableName+' M 
							INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
							AND PT.IsDeleted=0
							AND	PT.IsDeployedToIntegration=''1'' 
							AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
							WHERE M.StageTitle=''Revenue'''

		EXEC (@CustomQuery)
		
		--Remove linked plan tactic actuals for stage title revenue and CW which match with measure sfdc tactic's linked tactic
		SET @CustomQuery='DECLARE @LinkStartMnth INT=13
						  DECLARE @LinkEndMnth INT=24
					DELETE A FROM Plan_Campaign_Program_Tactic_Actual A 
					INNER JOIN
					(SELECT  PT.PlanTacticId
							,PT.LinkedTacticId
							,CASE WHEN DATEPART(YEAR, PTL.EndDate)-DATEPART(YEAR, PTL.StartDate)>0
							THEN 1
							ELSE 0 END IsMultiyear
					FROM
					'+@MeasureSFDCActualTableName+' M_SFDC 
					INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M_SFDC.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
					AND PT.IsDeleted=0
					AND	PT.IsDeployedToIntegration=''1'' 
					AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
					INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
					) LinkedTactic ON A.PlanTacticId=LinkedTactic.LinkedTacticId AND (
							(LinkedTactic.IsMultiyear=1 AND  CONVERT(INT,REPLACE(ISNULL(A.Period,''Y0''),''Y'',''''))>=@LinkStartMnth AND CONVERT(INT,REPLACE(ISNULL(A.Period,''Y0''),''Y'',''''))<=@LinkEndMnth)
							OR (LinkedTactic.IsMultiyear=0))
					WHERE A.StageTitle=''Revenue'' OR A.StageTitle=''CW'''

		EXEC (@CustomQuery)

		--insert linked plan tactic actuals for stage title CW which match with measure sfdc tactic's linked tactic	
		SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
							SELECT PT.LinkedTacticId PlanTacticId
									,M.StageTitle
									,CASE WHEN CONVERT(INT, DATEPART(YEAR, PTL.StartDate))<CONVERT(INT, DATEPART(YEAR, PTL.EndDate))
									THEN ''Y''+CONVERT(NVARCHAR(4), ((CONVERT(INT, DATEPART(YEAR, PTL.EndDate))-CONVERT(INT, DATEPART(YEAR, PTL.StartDate)))*12)+CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
									ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period
									,M.ActualValue
									,GETDATE() as CreatedDate
									,PTL.Createdby,
									NULL ModifiedDate,
									NULL ModifiedBy	
							 FROM 
							'+@MeasureSFDCActualTableName+' M 
							INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
							AND PT.IsDeleted=0
							AND	PT.IsDeployedToIntegration=''1'' 
							AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
							INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
							WHERE M.StageTitle=''CW'''

		EXEC (@CustomQuery)

		--insert linked plan tactic actuals for stage title Revenue which match with measure sfdc tactic's linked tactic	
		SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
		SELECT PT.LinkedTacticId PlanTacticId
				,M.StageTitle
				,CASE WHEN CONVERT(INT, DATEPART(YEAR, PTL.StartDate))<CONVERT(INT, DATEPART(YEAR, PTL.EndDate))
				THEN ''Y''+CONVERT(NVARCHAR(4), ((CONVERT(INT, DATEPART(YEAR, PTL.EndDate))-CONVERT(INT, DATEPART(YEAR, PTL.StartDate)))*12)+CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
				ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period
				,M.ActualValue
				,GETDATE() as CreatedDate
				,PTL.Createdby
				,NULL ModifiedDate
				,NULL ModifiedBy	
		 FROM 
		'+@MeasureSFDCActualTableName+' M 
		INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
		AND PT.IsDeleted=0
		AND	PT.IsDeployedToIntegration=''1'' 
		AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
		INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
		WHERE M.StageTitle=''Revenue'''

		EXEC (@CustomQuery)
END

