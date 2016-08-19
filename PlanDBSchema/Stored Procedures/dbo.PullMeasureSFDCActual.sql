CREATE PROCEDURE [dbo].[PullMeasureSFDCActual]
	(
	@ClientId nvarchar(36)='B14648DA-6A91-43E1-8FE2-0997C9180F55',
	@AuthDatabaseName Nvarchar(1000)='BDSAuthDev'
	)
AS
BEGIN
Declare @NewSFDCActualTableName nvarchar(255)

SET @NewSFDCActualTableName='MeasureSFDC_'
							+CONVERT(NVARCHAR(4), DATEPART(YEAR,GETDATE()))+'_'
							+CONVERT(NVARCHAR(2), DATEPART(MONTH,GETDATE()))+'_'
							+CONVERT(NVARCHAR(2), DATEPART(DAY,GETDATE()))+'_'
							+CONVERT(NVARCHAR(2), DATEPART(HOUR,GETDATE()))+'_'
							+CONVERT(NVARCHAR(2), DATEPART(MINUTE,GETDATE()))+'_'
							+CONVERT(NVARCHAR(2), DATEPART(SECOND,GETDATE()))+'_'
							+CONVERT(NVARCHAR(10), DATEPART(MILLISECOND,GETDATE()))

DECLARE @CustomQuery NVARCHAR(MAX)

		SET @CustomQuery=' DECLARE @ApplicationId nvarchar(36)=''''
DECLARE @ClientConnection NVARCHAR(1000)=''''
DECLARE @DatabaseName NVARCHAR(1000)=''''
SELECT TOP 1 @ApplicationId=ApplicationId from '+@AuthDatabaseName+'.[Dbo].[Application] WHERE Code=''RPC''
SELECT TOP 1 @ClientConnection='+@AuthDatabaseName+'.[dbo].[DecryptString](EncryptedConnectionString) from '+@AuthDatabaseName+'.[dbo].[ClientDatabase] where clientid='''+@ClientID+''' AND ApplicationId=@ApplicationId
IF (@ClientConnection IS NOT NULL AND @ClientConnection<>'''')
BEGIN

SELECT @DatabaseName=dimension FROM [dbo].fnSplitString(@ClientConnection,'';'') WHERE dimension like ''%initial catalog=%''
SET @DatabaseName= REPLACE(@DatabaseName,''initial catalog='','''')
END
SELECT @DatabaseName
'
--IF OBJECT_ID('tempdb..#MeasureSFDCTemp') IS NOT NULL DROP TABLE #MeasureSFDCTemp
DECLARE  @MeasureSFDCTemp TABLE (DatabaseName NVARCHAR(100))
INSERT INTO @MeasureSFDCTemp 
EXEC (@CustomQuery);

		Declare @DataBaseNameTemp NVARCHAR(1000)=''
				
		SELECT TOP 1 @DataBaseNameTemp=DatabaseName FROM @MeasureSFDCTemp
		
		SELECT @DataBaseNameTemp

		SET @CustomQuery='
		 CREATE TABLE '+@NewSFDCActualTableName+'
						(
						IntegrationType NVARCHAR(20),
						StageTitle NVARCHAR(20),
						Period NVARCHAR(10),
						ActualValue FLOAT,
						Unit VARCHAR(10),
						PusheeID NVARCHAR(255),
						PulleeID NVARCHAR(255),
						ModifiedDate DateTime
						)
		INSERT INTO '+@NewSFDCActualTableName+' 
		SELECT * FROM '+@DataBaseNameTemp+'.[INT].[GetTacticActuals](GETDATE(),GETDATE()-1);
		IF OBJECT_ID(''tempdb..#MeasureSFDCTemp'') IS NOT NULL DROP TABLE #MeasureSFDCTemp'
		
		EXEC(@CustomQuery)

	--Remove plan tactic actuals for stage title revenue and CW which match with measure sfdc tactics
	SET @CustomQuery=' DELETE A 
		FROM Plan_Campaign_Program_Tactic_Actual A 
		INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=A.PlanTacticId
		INNER JOIN '+@NewSFDCActualTableName+' T ON SUBSTRING(ISNULL(T.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) AND T.StageTitle=''CW''
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
						From '+@NewSFDCActualTableName+' M 
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
							From '+@NewSFDCActualTableName+' M 
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
					'+@NewSFDCActualTableName+' M_SFDC 
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
							'+@NewSFDCActualTableName+' M 
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
		'+@NewSFDCActualTableName+' M 
		INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
		AND PT.IsDeleted=0
		AND	PT.IsDeployedToIntegration=''1'' 
		AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
		INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
		WHERE M.StageTitle=''Revenue'''

		EXEC (@CustomQuery)


		--START:- Pulling Responses
		BEGIN
			Declare @query nvarchar(max)=''
			SET @query= @query + '
									Declare @entTacIds nvarchar(max)=''''
									Declare @inqStageTitle varchar(80)=''ProjectedStageValue''
									Declare @strtLnkMonth int = 13
									Declare @endLnkMonth int = 24
									Declare @inqStageId int = 106
									Declare @inqStageCode varchar(50)=''INQ''
								 '
			
			SET @query= @query + ' 

			-- Get INQ Stage ID based on clientID.
			SELECT TOP 1 @inqStageId=StageId from Stage where ClientId='''+Cast(@clientId as varchar(50))+''' and Code=@inqStageCode and IsDeleted=''0''

			SELECT @entTacIds= ISNULL(@entTacIds + '','','''') + cast(tac.PlanTacticId as varchar) 
			FROM '+@NewSFDCActualTableName+' as msrData
			JOIN Plan_Campaign_Program_Tactic as tac 
			ON SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			AND			IsDeleted=''0'' 
			AND			IsDeployedToIntegration=''1''
			AND			StageId=@inqStageId 
			AND			tac.[Status] IN (''In-Progress'',''Approved'',''Complete'')
			WHERE msrData.StageTitle=@inqStageCode

			-- START: Insert Actual values data to temp table.

			Declare @insertTacActTable Table(PlanTacticId int,StageTitle varchar(50), Period varchar(5), Actualvalue float, CreatedDate datetime, CreatedBy uniqueidentifier, ModifiedDate datetime, ModifiedBy uniqueidentifier) 

			INSERT
			INTO @insertTacActTable
			SELECT tac.PlanTacticId,
					@inqStageTitle,
					CASE	
						WHEN YEAR(tac.StartDate) < YEAR(tac.EndDate) 
						THEN
							CASE 
								WHEN YEAR(tac.StartDate) < CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT )	-- IF Tactic start date year less than Period year then convert Period to multi year ex. 2015 < 2016
								THEN ''Y'' + Cast( ( ( ( CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT ) - YEAR(tac.StartDate) ) * 12 ) + CAST( LEFT(msrData.Period,ChARINDEX(''-'',msrData.Period)-1) as INT ) ) as varchar(2) ) -- convert period to multi year :  = ( (2016 - 2015) * 12 ) + 3 = Y15
								ELSE ''Y'' + LEFT(msrData.Period,CHARINDEX(''-'',msrData.Period)-1)
							END
					ELSE
						CASE
							WHEN YEAR(tac.StartDate) = CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT )
							THEN ''Y'' + LEFT(msrData.Period,CHARINDEX(''-'',msrData.Period)-1)
						END
					END AS Period,
					msrData.ActualValue,
					GetDate() as CreatedDate,
					tac.CreatedBy,
					Null as ModifiedDate,
					Null as ModifiedBy
			FROM '+@NewSFDCActualTableName+' as msrData
			INNER JOIN  Plan_Campaign_Program_Tactic as tac 
			ON			SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			AND			IsDeleted=''0'' 
			AND			IsDeployedToIntegration=''1'' 
			AND			StageId=@inqStageId 
			AND			tac.[Status] IN (''In-Progress'',''Approved'',''Complete'')
			AND			IsNull(Period,'''')<>''''
			WHERE		msrData.StageTitle=@inqStageCode
			-- END: Insert Actual values data to temp table.

			-- START: Delete existing actual values.

			DELETE Plan_Campaign_Program_Tactic_Actual 
			FROM Plan_Campaign_Program_Tactic_Actual as act
			JOIN 
			(
				-- Get Multi year tactic
				SELECT tact1.PlanTacticId,RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + '':'' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + '':'' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				END 
																   ORDER BY pln.[Year] DESC)
				FROM Plan_Campaign_Program_Tactic as tact1
				JOIN Plan_Campaign_Program as prg on tact1.PlanProgramId = prg.PlanProgramId
				JOIN Plan_Campaign as camp on prg.PlanCampaignId = camp.PlanCampaignId
				JOIN [Plan] as pln on camp.PlanId = pln.PlanId
				WHERE LinkedTacticId > 0 and PlanTacticId In (select val from comma_split(@entTacIds,'',''))
			
				UNION 
				
				-- Get Single year tactics
				SELECT tact1.PlanTacticId,RN = 1 
				FROM Plan_Campaign_Program_Tactic as tact1 
				WHERE PlanTacticId In (SELECT val from comma_split(@entTacIds,'','')) and IsNUll(LinkedTacticId,0) = 0
			
			) as lnkTac 
			ON act.PlanTacticId = lnkTac.PlanTacticId
			AND StageTitle = @inqStageTitle
			JOIN @insertTacActTable as insrtAct	-- Join with Insert Actual table data to remove the duplicate data from Actual table prior to insert
			ON act.PlanTacticId = insrtAct.PlanTacticId 
			AND IsNull(insrtAct.Period,'''') <> '''' 
			AND (
					(lnkTac.RN > 1 and ( (Cast(Replace(IsNull(act.Period,''Y0''),''Y'','''') as int) between @strtLnkMonth and @endLnkMonth) OR ( insrtAct.Period = act.Period ) )  -- Remove Y13 to Y24 period actual values in case of multi year.
					OR (lnkTac.RN = 1)	-- Delete Y1 to Y12 period actual values in case of single year tactic.
					)
				) 
			
			-- END: Delete existing actual values.
			
			-- START: Insert Actual values to Actual table.
			INSERT INTO Plan_Campaign_Program_Tactic_Actual
			SELECT PlanTacticId,
					StageTitle,
					Period,
					Actualvalue,
					CreatedDate,
					CreatedBy,
					ModifiedDate,
					ModifiedBy
			FROM @insertTacActTable as insrtAct
			WHERE IsNull(insrtAct.Period,'''') <> ''''
			-- END: Insert Actual values to Actual table.
			'
			EXEC (@query)
		END
		--END:- Pulling Responses

		--Remove SFDC table which will created from Measure database function
		SET @CustomQuery='IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=''dbo'' AND TABLE_NAME='''+@NewSFDCActualTableName+''')
							BEGIN
							DROP TABLE ['+@NewSFDCActualTableName+'];
							END'
			EXEC(@CustomQuery)

END