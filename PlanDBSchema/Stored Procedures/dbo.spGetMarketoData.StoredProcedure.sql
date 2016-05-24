USE [MRPDev]
GO
/****** Object:  StoredProcedure [dbo].[spGetMarketoData]    Script Date: 05/24/2016 6:59:20 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetMarketoData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetMarketoData]
GO
/****** Object:  StoredProcedure [dbo].[spGetMarketoData]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetMarketoData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetMarketoData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetMarketoData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)='In-Progress'
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit='0'
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit ='0'
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''
		Declare @dynResultQuery nvarchar(max)=''

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)='PushTacticData'
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)='Name'
		Declare @colDescription varchar(50)='Description'
		Declare @colCampgnFolder varchar(255)='CampaignFolder'
		Declare @actNote varchar(255)='Note'
		Declare @actCostStartDate varchar(255)='CostStartDate'
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables
	END
	-- END: Declare local variables
	

	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								TacticCustomName varchar(max),
								LinkedTacticId int,
								LinkedPlanId int
								)
	
	-- Start: Set IsAutoSync or not
	--BEGIN
	--	select @nullGUID=cast(cast(0 as binary) as uniqueidentifier)
	--	If((@userId Is NULL) OR (@userId =@nullGUID))
	--	BEGIN
	--		SET @isAutoSync='1'
	--	END
	--END
	-- End: Set IsAutoSync or not

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Tactic or Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
				--IF NOT EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1' and  LastSyncStatus=@syncStatusInProgress)
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Instance Id')
					SET @instanceId= @id

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
					;WITH tblTact AS (
						Select tact.PlanTacticId,
							   tact.PlanProgramId,
								tact.TacticCustomName,
								tact.LinkedTacticId ,
								tact.LinkedPlanId
						from [Model] as mdl
						join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
						Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
						join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
						join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncMarketo='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
						where  mdl.IntegrationInstanceMarketoID=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
					),
					 tactList AS (
						(
							SELECT tact1.PlanTacticId,
									tact1.PlanProgramId,
									tact1.TacticCustomName,
										tact1.LinkedTacticId ,
										tact1.LinkedPlanId
							FROM tblTact as tact1 
							WHERE IsNull(Tact1.LinkedTacticId,0) <=0
						 )
						 UNION
						 (
							SELECT tact1.PlanTacticId,
									tact1.PlanProgramId,
									tact1.TacticCustomName,
									tact1.LinkedTacticId ,
									tact1.LinkedPlanId 
							FROM tblTact as tact1 
							WHERE (tact1.LinkedTacticId > 0) --and (MAX(Cast(tact1.PlanYear as int)))
						 )
					)
					Insert into @tblTaclist select * from tactList;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				--ELSE
				--BEGIN
				--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance already in running process')
				--END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			--ELSE
			--BEGIN
			--	IF(@id>0)
			--	BEGIN
			--		Insert Into IntegrationInstanceLog values(@id,GETDATE(),GETDATE(),@syncStatusError,@msgInactiveStatus,GETDATE(),@userId,@isAutoSync)

			--		-- Update Instance value with values: LastSyncDate,LastSyncStatus,ForceSyncUser
			--		Insert Into @tblSyncError values(0,@entityTypeTac,@entityTypeIntegrationInstance,@msgInactiveStatus,@syncStatusError,GETDATE())

			--		-- Get Integration Instance Name
			--		Declare @strInstanceName nvarchar(max)=''
			--		Select @strInstanceName = IsNULL(Instance,'') from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1'

			--		-- Push Instance Name log to temp table
			--		Declare @msgStartSyncInstance nvarchar(max)='<tr><td width=''50%''><b>Instance Name used for syncing:</b></td><td width=''40%''>'+@strInstanceName+'</td></tr>'
			--		Insert Into @tblSyncError values(0,@entityTypeTac,'',@msgStartSyncInstance,@syncStatusHeader,GETDATE())

			--	END

			--END
		END	
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeTac))
		BEGIN
			
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Tactic Id')
			
			BEGIN TRY
			INSERT INTO @tblTaclist 
			SELECT tact.PlanTacticId,
					tact.PlanProgramId,
					tact.TacticCustomName,
					tact.LinkedTacticId ,
					tact.LinkedPlanId
			FROM Plan_Campaign_Program_Tactic as tact 
			WHERE tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncMarketo='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete') and tact.PlanTacticId=@id
			
			-- Get Integration Instance Id based on Tactic Id.
			SELECT @instanceId=mdl.IntegrationInstanceMarketoID
			FROM [Model] as mdl
			INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
			INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
			INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0
			INNER JOIN @tblTaclist as tac ON prg.PlanProgramId = tac.PlanProgramId
			END TRY
			BEGIN CATCH
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
			END CATCH

			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Tactic Id')
		END

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Tactic or Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId

			-- Update LastSync Status of Integration Instance
			--Update IntegrationInstance set LastSyncStatus = @syncStatusInProgress where IntegrationInstanceId=@instanceId
			
		END
		-- END: Get IntegrationTypeId

		

		-- START: GET result data based on Mapping fields
		BEGIN
			IF EXISTS(Select PlanTacticId from @tblTaclist)
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						select  @ColumnName = dbo.GetTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					-- START: Get TacticIds
					SELECT @entIds= ISNULL(@entIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist) AS PlanTacticIds
					-- END: Get TacticIds
					

					----- START: Get Updte CustomName TacIds -----
					SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where IsNull(TacticCustomName,'')='') AS PlanTacticIds
					----- END: Get Updte CustomName TacIds -----
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

					BEGIN TRY
						-- START: Update Tactic Name --
						UPDATE Plan_Campaign_Program_Tactic 
						SET TacticCustomName = T1.CustomName 
						FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
						INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId
						-- END: Update Tactic Name --
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')

					--SELECT * from  [GetSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'94016,94028,94029,94030',3,1176)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT MarketoProgramId,SourceId,Mode,[CampaignFolder],'+@actNote+','+@actCostStartDate+','+@ColumnName+' 
																FROM (
																		SELECT ActualFieldName,TacValue,SourceId 
																		FROM [GetSourceTargetMappingData](''Tactic'','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''')) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (MarketoProgramId,Mode,[CampaignFolder],'+@actNote+','+@actCostStartDate+','+@ColumnName+')
								 ) AS PVTTable
								 '
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()+',SQL Query-'+ (Select @dynResultQuery)))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Create final result Pivot Query')
					--PRINT @dynResultQuery  
					--Execute the Dynamic Pivot Query
					--EXEC sp_executesql @dynResultQuery
					
				END
				ELSE
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for marketo instance')
				END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Data does not exist')
			END
		END

		-- END: GET result data based on Mapping fields

	END
	-- END
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push marketo')
	EXEC(@dynResultQuery)
	--select * from @tblSyncError
	--SELECT @logStartInstanceLogId as 'InstanceLogStartId'
END

GO
