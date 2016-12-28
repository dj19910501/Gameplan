
/****** Object:  UserDefinedFunction [dbo].[GetSourceTargetMappingData]    Script Date: 12/27/2016 3:40:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [dbo].[GetSourceTargetMappingData]
(
	@entityType varchar(255)='Tactic',
	@ClientId INT,
	@EntityIds varchar(max)='',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255	-- default value 255
)

--SELECT * from  [GetSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'94016,94028,94029,94030',3,1176,255)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int DEFAULT(1),
ProgramInitiationOption INT, 
InitialProgramId NVARCHAR(50)
)
AS
BEGIN

Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
Declare @ColumnName nvarchar(max)
Declare @actCampaignFolder varchar(255)='CampaignFolder'
Declare @trgtCampaignFolder varchar(255)='Id'
Declare @actMarketoProgramId varchar(255)='MarketoProgramId'
Declare @trgtMarketoProgramId varchar(255)=''
Declare @actMode varchar(255)='Mode'
Declare @trgtMode varchar(255)=''
Declare @modeCREATE varchar(20)='Create'
Declare @modeUPDATE varchar(20)='Update'
Declare @actNote varchar(255)='Note'
Declare @trgtNote varchar(255)='note'
Declare @actCostStartDate varchar(255)='CostStartDate'
Declare @trgtCostStartDate varchar(255)='CostStartDate'
Declare @actCreatedBy varchar(255)='CreatedBy'
Declare @tblTactic varchar(255)='Plan_Campaign_Program_Tactic'

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	
	(
			Select  IntegrationInstanceID,
					IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
					TableName,
					gpDataType.ActualFieldName,
					TargetDataType,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId
			FROM GamePlanDataType as gpDataType
			JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
			Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName='Plan_Campaign_Program_Tactic' OR gpDataType.TableName='Global') and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					TargetDataType,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId
					
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	
	)
	insert into @Table 
	select * from ResultTable
END
-------- END: Get Standard & CustomField Mappings data --------
-------- START: Insert fixed Marketo fields to Mapping list. -------- 
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMarketoProgramId as ActualFieldName,@trgtMarketoProgramId as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,@trgtMode as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actCampaignFolder as ActualFieldName,@trgtCampaignFolder as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actNote as ActualFieldName,@trgtNote as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actCostStartDate as ActualFieldName,@trgtCostStartDate as TargetDataType,0 as CustomFieldId
END
-------- END: Insert fixed Marketo fields to Mapping list. -------- 

;WITH tact as(
SELECT 
	T.PlanTacticId,
	T.Title,
	T.MarketoProgramInitiationOption,
	T.MarketoInitialProgramId,
	'Tactic_Mapp' as Link
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,','))
),
IntegMapp as(
	SELECT 
		Mapp.*,
		'Tactic_Mapp' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +'-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]='TextBox' then Extent1.Value     
			when Extent3.[Name]='DropDownList' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]='TextBox' then Extent1.Value
			when Extent3.[Name]='DropDownList' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +'-'  + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,','))) 
					Group by SUBSTRING(@entityType,1,1) +'-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +'-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,','))
)
)

INSERT INTO @src_trgt_mappdata
select Mapp.ActualFieldName,
		Mapp.CustomFieldId,
		 CASE 
			WHEN Mapp.ActualFieldName=@actMarketoProgramId THEN ISNull(Tac.IntegrationInstanceMarketoID,'')
			WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
														WHEN ISNULL(Tac.IntegrationInstanceMarketoID,'')='' THEN @modeCREATE 
														ELSE @modeUPDATE 
													 END)  
			WHEN Mapp.ActualFieldName='ProgramType' THEN ISNull(marktType.ProgramType,'')
			WHEN Mapp.ActualFieldName='Channel' THEN ISNull(marktType.Channel,'')
			WHEN Mapp.ActualFieldName='Status' THEN ISNull(Tac.[Status],'')
			WHEN Mapp.ActualFieldName='ActivityType' THEN 'Tactic'
			WHEN Mapp.ActualFieldName='PlanName' THEN ISNull(pln.Title,'')
			WHEN Mapp.ActualFieldName='Cost' THEN ISNull(Cast(Tac.Cost as varchar(255)),'')
			WHEN Mapp.ActualFieldName='TacticType' THEN ISNull(TT.Title,'')
			WHEN Mapp.ActualFieldName='Name' THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'')
			WHEN Mapp.ActualFieldName='Description' THEN ISNull(Tac.[Description],'')
			WHEN Mapp.ActualFieldName=@actCampaignFolder THEN ISNull(marktFolder.MarketoCampaignFolderId,'')
			WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'None')
			WHEN Mapp.ActualFieldName=@actNote THEN 'Planned Cost'
			WHEN Mapp.ActualFieldName=@actCostStartDate THEN ISNull(CONVERT(VARCHAR(19),Tac.StartDate),'')
			WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'')
		END AS TacValue,
		T.PlanTacticId as SourceId, 
		T.MarketoProgramInitiationOption As ProgramInitiationOption,
		T.MarketoInitialProgramId as InitialProgramId
from IntegMapp as Mapp
INNER JOIN tact as T ON Mapp.Link = T.Link
LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.PlanTacticId = Tac.PlanTacticId
LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.PlanTacticId = custm.EntityId
LEFT JOIN MarketoEntityValueMapping as marktFolder ON pln.PlanId = marktFolder.EntityID and marktFolder.EntityType='Plan' and marktFolder.IntegrationInstanceId =@id 
LEFT JOIN MarketoEntityValueMapping as marktType ON TT.TacticTypeId = marktType.EntityID and marktType.EntityType='TacticType' 
	RETURN 
END
GO

ALTER PROCEDURE [dbo].[spGetMarketoData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId INT,
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
								LinkedPlanId int,
								PlanYear int,
								RN int
								)

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Tactic or Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
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
									tact.LinkedPlanId,
									pln.[Year] as PlanYear
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
											tact1.LinkedPlanId,
											tact1.PlanYear,
											RN= 1
								FROM tblTact as tact1 
								WHERE IsNull(Tact1.LinkedTacticId,0) <=0
							 )
							 UNION
							 (
								SELECT tact1.PlanTacticId,
										tact1.PlanProgramId,
										tact1.TacticCustomName,
										tact1.LinkedTacticId ,
										tact1.LinkedPlanId,
										tact1.PlanYear,
										-- Get latest year tactic
										RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																				WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																				ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																			 END 
																ORDER BY PlanYear DESC) 
								FROM tblTact as tact1 
								WHERE (tact1.LinkedTacticId > 0)
							 )
						)
					Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			
		END	
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeTac))
		BEGIN
			
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Tactic Id')
			BEGIN TRY
				IF EXISTS(SELECT LinkedTacticId from Plan_Campaign_Program_Tactic where PlanTacticId=@id)
				BEGIN
					
					DECLARE @tac_lnkdIds varchar(20)=''
					SELECT @tac_lnkdIds=cast(PlanTacticId as varchar)+','+Cast(ISNULL(LinkedTacticId,0) as varchar) 
					FROM Plan_Campaign_Program_Tactic where PlanTacticId=@id
					;WITH tbl as(
								SELECT tact.PlanTacticId,tact.LinkedTacticId,tact.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tact
								WHERE PlanTacticId IN (select val from comma_split(@tac_lnkdIds,',')) and tact.IsDeleted=0
								UNION ALL
								SELECT tac.PlanTacticId,tac.LinkedTacticId,tac.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tac 
								INNER JOIN tbl as lnk on tac.LinkedTacticId=lnk.PlanTacticId
								WHERE tac.PlanTacticId=@id
								)
					-- Set latest year tactic to @id variable
					SELECT TOP 1 @id=LinkedTacticId 
					FROM tbl
					INNER JOIN [Plan] as pln on tbl.LinkedPlanId = pln.PlanId and pln.IsDeleted=0
					ORDER BY [Year] DESC
				END
			
				INSERT INTO @tblTaclist 
				SELECT tact.PlanTacticId,
						tact.PlanProgramId,
						tact.TacticCustomName,
						tact.LinkedTacticId ,
						tact.LinkedPlanId,
						Null as PlanYear,
						1 as RN
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
						SET @dynResultQuery ='SELECT MarketoProgramId,SourceId,ProgramInitiationOption,InitialProgramId,Mode,[CampaignFolder],'+@actNote+','+@actCostStartDate+','+@ColumnName+' 
																FROM (
																		SELECT ActualFieldName,TacValue,SourceId, ProgramInitiationOption, InitialProgramId
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

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE Table_name = 'Plan_Campaign_program_tactic' and CoLumn_name = 'MarketoProgramInitiationOption')
ALTER TABLE dbo.Plan_Campaign_program_tactic
ADD MarketoProgramInitiationOption INT NOT NULL DEFAULT(1)
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE Table_name = 'Plan_Campaign_program_tactic' and CoLumn_name = 'MarketoInitialProgramId')
ALTER TABLE dbo.Plan_Campaign_program_tactic
ADD MarketoInitialProgramID NVARCHAR(50) NULL
GO

--Added By Preet Shah on 22/12/2016. For Save Marketing Budget Column Attribute
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'User_CoulmnView' AND COLUMN_NAME = 'MarketingBudgetAttribute')
BEGIN

    ALTER TABLE User_CoulmnView ADD [MarketingBudgetAttribute] [xml] NULL 
   
END

GO
--Replace thsi line with your script 

----UPDATE DB SCHEMA VERSION FOR THE RELEASE 
declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'December.2016'
set @version = 'December.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END



GO

