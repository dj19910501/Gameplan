-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [INT].[PullMQL] 
	@NewSFDCActualTableName varchar(255)='',
	@clientId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @query nvarchar(max)='' 
   SET @query =''
			SET @query= @query + '
									Declare @entTacIds nvarchar(max)=''''
									Declare @mqlStageTitle varchar(80)=''MQL''
									Declare @strtLnkMonth int = 13
									Declare @endLnkMonth int = 24
									Declare @mqlStageCode varchar(50)=''MQL''
									Declare @sfdcTypeCode varchar(50)=''Salesforce''
								 '
			SET @query= @query + '
									IF EXISTS ( SELECT ClientIntegrationPermissionId 
												FROM Client_Integration_Permission 
												WHERE IntegrationTypeId IN (
																			 SELECT IntegrationTypeId 
																			 FROM IntegrationType 
																			 WHERE Code =@sfdcTypeCode
																			) 
												AND PermissionCode = @mqlStageTitle 
												AND ClientId = '''+Cast(@clientId as varchar(50))+''''
			SET @query= @query + ' 
			BEGIN

			SELECT @entTacIds= ISNULL(@entTacIds + '','','''') + cast(tac.PlanTacticId as varchar) 
			FROM '+@NewSFDCActualTableName+' as msrData
			JOIN Plan_Campaign_Program_Tactic as tac 
			ON SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			WHERE msrData.StageTitle=@mqlStageCode

			-- START: Insert Actual values data to temp table.

			Declare @insertTacActTable Table(PlanTacticId int,StageTitle varchar(50), Period varchar(5), Actualvalue float, CreatedDate datetime, CreatedBy uniqueidentifier, ModifiedDate datetime, ModifiedBy uniqueidentifier) 

			INSERT
			INTO @insertTacActTable
			SELECT tac.PlanTacticId,
					@mqlStageTitle,
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
			AND			IsNull(Period,'''')<>''''
			WHERE		msrData.StageTitle=@mqlStageCode
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
			AND StageTitle = @mqlStageTitle
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
			SET @query= @query + ' END'
			
			EXEC (@query)
END

