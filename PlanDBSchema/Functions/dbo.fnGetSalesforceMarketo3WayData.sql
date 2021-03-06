/****** Object:  UserDefinedFunction [dbo].[fnGetSalesforceMarketo3WayData]    Script Date: 06/24/2016 1:53:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetSalesforceMarketo3WayData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetSalesforceMarketo3WayData]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetSalesforceMarketo3WayData]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetSalesforceMarketo3WayData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create Function [dbo].[fnGetSalesforceMarketo3WayData]
(
	@entityType varchar(255)='''',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0
)
RETURNS @dataQuery table
(
	query nvarchar(max)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)=''Tactic''
		Declare @entityTypeProg varchar(255)=''Program''
		Declare @entityTypeCampgn varchar(255)=''Campaign''
		Declare @entityTypeIntegrationInstance varchar(255)=''IntegrationInstance''
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)=''In-Progress''
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit=''0''
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit =''0''
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''''
		Declare @dynResultQuery nvarchar(max)=''''
		Declare @actcParentID varchar(100)=''cParentId''				-- Added on 06/23/2016

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)=''PushTacticData''
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)=''Name''
		Declare @colDescription varchar(50)=''Description''
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)=''Start :''
		Declare @logEnd varchar(20)=''End :''
		Declare @logSP varchar(100)=''Stored Procedure Execution- ''
		Declare @logError varchar(20)=''Error :''
		Declare @logInfo varchar(20)=''Info :''
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)=''Tactic''
		Declare @prg varchar(20)=''Program''
		Declare @cmpgn varchar(20)=''Campaign''
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''''
		Declare @entPrgIds nvarchar(max)=''''
		Declare @entCmpgnIds nvarchar(max)=''''
		Declare @entImrvmntTacIds nvarchar(max)=''''
		Declare @entImrvmntPrgIds nvarchar(max)=''''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''''
		-- End: Entity Ids

	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Start: Identify Entity Type

	BEGIN

		--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Identify EnityType Integration Instance'')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted=''0'' and IsActive=''1'')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Get Tactic Data by Instance Id'')
					SET @instanceId= @id

					-- START: Identify 3 Way integration between Marketo & SFDC - Create hierarchy in SFDC 
					BEGIN
						Declare @isSyncSFDCWithMarketo bit=''1''
						--Declare @ModelIds varchar(max)=''''
						--SELECT @ModelIds = ISNULL(@ModelIds  + '','','''') + (mdlId)
						--FROM (Select DISTINCT Cast (ModelId as varchar(max)) mdlId from Model where ((IsNull(IsDeleted,''0'')=''0'') AND (IntegrationInstanceMarketoID<>@id OR (IsNull(IntegrationInstanceMarketoID,0)=0)) AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id OR IntegrationInstanceId=@id))) AS planIds
					END

					-- START: Get Tactic Data by Instance Id
					--BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration=''1'' and (tact.IsSyncMarketo=''1'') and (IsNUll(tact.IntegrationInstanceMarketoID,'''')<>'''') and (tact.[Status]=''Approved'' or tact.[Status]=''In-Progress'' or tact.[Status]=''Complete'')
								where  mdl.ModelId IN (
														Select DISTINCT ModelId from Model where ((IsNull(IsDeleted,''0'')=''0'') AND (IntegrationInstanceMarketoID<>@id OR (IsNull(IntegrationInstanceMarketoID,0)=0)) AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id))
													   ) and mdl.[Status]=''Published'' and mdl.IsActive=''1''
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + '':'' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + '':'' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and (prgrm.[Status]=''Approved'' or prgrm.[Status]=''In-Progress'' or prgrm.[Status]=''Complete'')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]=''Published'' and mdl.IsActive=''1''
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and (campgn.[Status]=''Approved'' or campgn.[Status]=''In-Progress'' or campgn.[Status]=''Complete'')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]=''Published'' and mdl.IsActive=''1''
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					--END TRY
					--BEGIN CATCH
					--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()))	
					--END CATCH

					-- END: Get Tactic Data by Instance Id
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Get Tactic Data by Instance Id'')
				END
				
			END
			--ELSE
			--BEGIN
			--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''Instance Not Exist'')
			--END
			
		END	
		
		--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Identify EnityType Integration Instance'')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'''') =''''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'''')='''')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'''') =''''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'''')='''')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''''
					Declare @updIds varchar(max)=''''

					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Get comma separated column names'')
					
					--BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					--END TRY
					--BEGIN CATCH
					--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()))	
					--END CATCH
										
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Get comma separated column names'')	
					
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Get Tactic Ids'')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + '','','''') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + '','','''') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + '','','''') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,''0'')=''1'' AND IsNull(@isClientAllowCustomName,''0'')=''1'')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + '','','''') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Get Tactic Ids'')

						--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Update Tactic CustomName'')

						--BEGIN TRY
							-- START: Update Tactic Name --
							--UPDATE Plan_Campaign_Program_Tactic 
							--SET TacticCustomName = T1.CustomName 
							--FROM GetTacCustomNameMappingList(''Tactic'',@clientId,@updIds) as T1 
							--INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'''')=''''
							-- END: Update Tactic Name --
						--END TRY
						--BEGIN CATCH
						--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()))	
						--END CATCH

						--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Update Tactic CustomName'')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''101371'',2,1203,255,0,0)

					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Create final result Pivot Query'')

					--BEGIN TRY
						SET @dynResultQuery =''SELECT distinct SourceId,SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,''+@ColumnName+'' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way](''''''+ @tact +'''''',''''''+ CAST(@clientId AS NVARCHAR(36)) +'''''',''''''+ @entTacIds +'''''',''''''+ CAST(@integrationTypeId AS NVARCHAR) +'''''',''''''+ CAST(@instanceId AS NVARCHAR) +'''''',''''''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +'''''',''''''+ CAST(@isCustomNameAllow AS NVARCHAR) +'''''',''''''+ CAST(@isClientAllowCustomName AS NVARCHAR) +'''''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way](''''''+ @prg +'''''',''''''+ CAST(@clientId AS NVARCHAR(36)) +'''''',''''''+ @entPrgIds +'''''',''''''+ CAST(@integrationTypeId AS NVARCHAR) +'''''',''''''+ CAST(@instanceId AS NVARCHAR) +'''''',''''''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +'''''',''''''+ CAST(@isCustomNameAllow AS NVARCHAR) +'''''',''''''+ CAST(@isClientAllowCustomName AS NVARCHAR) +'''''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way](''''''+ @cmpgn +'''''',''''''+ CAST(@clientId AS NVARCHAR(36)) +'''''',''''''+ @entCmpgnIds +'''''',''''''+ CAST(@integrationTypeId AS NVARCHAR) +'''''',''''''+ CAST(@instanceId AS NVARCHAR) +'''''',''''''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +'''''',''''''+ CAST(@isCustomNameAllow AS NVARCHAR) +'''''',''''''+ CAST(@isClientAllowCustomName AS NVARCHAR) +'''''')
																		)
																		
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,''+@ColumnName+'')
								 ) AS PVTTable
								 ''
					--END TRY
					--BEGIN CATCH
					--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()+'',SQL Query-''+ (Select @dynResultQuery)))	
					--END CATCH

					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Create final result Pivot Query'')
					--PRINT @dynResultQuery			
				END
				--ELSE
				--BEGIN
				--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''No single field mapped for Salesforce instance'')
				--END
			END
			--ELSE
			--BEGIN
			--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''Data does not exist'')
			--END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''Get final result data to push Salesforce'')
	--EXEC(@dynResultQuery)
	Insert Into @dataQuery
	select @dynResultQuery

	Return
END

' 
END

GO
