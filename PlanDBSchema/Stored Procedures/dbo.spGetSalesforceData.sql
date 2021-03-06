/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/24/2016 1:53:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceData]
GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetSalesforceData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetSalesforceData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0,
	@isSyncSFDCWithMarketo bit=0
AS
BEGIN

	--Exec [spGetSalesforceData_06_23_2016] 'IntegrationInstance',1211,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,'1'
	--Exec [spGetSalesforceData_06_23_2016] 'IntegrationInstance',1212,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,'1'
	--Exec [spGetSalesforceData_06_23_2016] 'IntegrationInstance',1203,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,'0'
	--Exec [spGetSalesforceData_06_23_2016] 'Tactic',105422,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,0
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeProg varchar(255)='Program'
		Declare @entityTypeCampgn varchar(255)='Campaign'
		Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
		Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
		Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
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
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)='Tactic'
		Declare @prg varchar(20)='Program'
		Declare @cmpgn varchar(20)='Campaign'
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''
		Declare @entPrgIds nvarchar(max)=''
		Declare @entCmpgnIds nvarchar(max)=''
		Declare @entImrvmntTacIds nvarchar(max)=''
		Declare @entImrvmntPrgIds nvarchar(max)=''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''
		-- End: Entity Ids
		Declare @query nvarchar(max)=''
		
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

	-- Store Improvement Entities related data
	Declare @tblImprvEntity table (
									ImprovementPlanTacticId int,
									ImprovementPlanProgramId int,
									ImprovementPlanCampaignId int,
									ObjectType varchar(50)
								  )

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Campaign,Program,Tactic,Improvement Tactic or Integration Instance')
		
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
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
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
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
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
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
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
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- Get Improvement TacticIds
							BEGIN
								Insert into @tblImprvEntity
								Select Imprvtact.ImprovementPlanTacticId,
									   Imprvtact.ImprovementPlanProgramId,
									   Imprvcampgn.ImprovementPlanCampaignId,
									   @entityTypeImprvTac as ObjectType
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Improvement_Campaign as Imprvcampgn ON Imprvcampgn.ImprovePlanId = pln.PlanId 
								join Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
								join Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
								where mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							END

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

				-- Pick latest year tactic in case of linked Tactic and push to SFDC.
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
						prg.PlanCampaignId,
						tact.LinkedTacticId ,
						tact.LinkedPlanId,
						Null as PlanYear,
						@tact as ObjectType,
						1 as RN
				FROM Plan_Campaign_Program_Tactic as tact 
				INNER JOIN Plan_Campaign_Program as prg on tact.PlanProgramId = prg.PlanProgramId and prg.IsDeleted='0'
				WHERE tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete') and tact.PlanTacticId=@id
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
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
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeProg))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Program Id')

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where pln.IsDeleted=0
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
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
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
									from [Plan] as pln
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

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
								INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
								INNER JOIN Plan_Campaign_Program as P on tac.PlanProgramId=P.PlanProgramId and (IsNull(P.IntegrationInstanceProgramId,'')='')
								where tac.ObjectType=@prg
							END
							-- END: Get list of Campaigns not pushed into SFDC.
							

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
					INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0 and prg.PlanProgramId=@id
					
					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Program Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeCampgn))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Campaign Id')
					
					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln 
								INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
								INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								INNER JOIN Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  pln.IsDeleted=0
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
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
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
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where pln.IsDeleted=0
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
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0 and cmpgn.PlanCampaignId=@id

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Campaign Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeImprvTac))
		BEGIN
			-- Get Improvement TacticIds
			BEGIN
				Insert into @tblImprvEntity
				Select Imprvtact.ImprovementPlanTacticId,
					   Imprvtact.ImprovementPlanProgramId,
					   Imprvcampgn.ImprovementPlanCampaignId,
					   @entityTypeImprvTac as ObjectType
				from Plan_Improvement_Campaign as Imprvcampgn
				INNER JOIN Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
				INNER JOIN Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.ImprovementPlanTacticId=@id and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Improvement_Campaign] as Imprvcmpgn ON pln.PlanId = Imprvcmpgn.ImprovePlanId
				INNER JOIN [Plan_Improvement_Campaign_Program] as ImprvPrg ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId 
				INNER JOIN @tblImprvEntity as ImprvTac ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
			END
		END
		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Tactic or Integration Instance')

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
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
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
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: Add list of Improvement Programs not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Imprvprg.ImprovementPlanProgramId,
						   Imprvprg.ImprovementPlanCampaignId,
							@entityTypeImprvProg as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign_Program as Imprvprg on Imprvtac.ImprovementPlanProgramId = Imprvprg.ImprovementPlanProgramId and IsNull(Imprvprg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Improvement Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Null as ImprovementPlanProgramId,
						   ImprvCmpgn.ImprovementPlanCampaignId,
							@entityTypeImprvCamp as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign as ImprvCmpgn on Imprvtac.ImprovementPlanCampaignId = ImprvCmpgn.ImprovementPlanCampaignId and IsNull(ImprvCmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist)) OR (EXISTS(Select 1 from @tblImprvEntity))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
										
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + ',','') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + ',','') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids

					-- Get Improvement Ids
					BEGIN
						-- START: Get ImprvmntTacticIds
						SELECT @entImrvmntTacIds = ISNULL(@entImrvmntTacIds  + ',','') + (ImprvTac)
						FROM (Select DISTINCT Cast (ImprovementPlanTacticId as varchar(max)) ImprvTac FROM @tblImprvEntity where ObjectType=@entityTypeImprvTac) AS PlanTacticIds
						-- END: Get ImprvmntTacticIds

						-- START: Get ImprvmntCampaign Ids
						SELECT @entImrvmntCmpgnIds = ISNULL(@entImrvmntCmpgnIds  + ',','') + (ImprvCampgn)
						FROM (Select DISTINCT Cast (ImprovementPlanCampaignId as varchar(max)) ImprvCampgn FROM @tblImprvEntity where ObjectType=@entityTypeImprvCamp) AS PlanCampaignIds
						-- END: Get ImprvmntCampaign Ids

						-- START: Get ImprvmntProgram Ids
						SELECT @entImrvmntPrgIds= ISNULL(@entImrvmntPrgIds + ',','') + (ImprvPrgrm)
						FROM (Select DISTINCT Cast (ImprovementPlanProgramId as varchar(max)) ImprvPrgrm FROM @tblImprvEntity where ObjectType=@entityTypeImprvProg) AS PlanProgramIds
						-- END: Get ImprvmntProgram Ids
					END
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,'0')='1' AND IsNull(@isClientAllowCustomName,'0')='1')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

						BEGIN TRY
							-- START: Update Tactic Name --
							UPDATE Plan_Campaign_Program_Tactic 
							SET TacticCustomName = T1.CustomName 
							FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
							INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'')=''
							-- END: Update Tactic Name --
						END TRY
						BEGIN CATCH
							Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
						END CATCH

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'101371',2,1203,255,0,0)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT distinct SourceId,SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,'+@ColumnName+' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @tact +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @prg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @cmpgn +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvCamp +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvProg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvTac +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,'+@ColumnName+')
								 ) AS PVTTable
								 '
								 -- IF Marketo-SFDC 3way data exist then get marketo related tactics by below function.
								 IF(@isSyncSFDCWithMarketo='1')
								 BEGIN

									select @query = query from [fnGetSalesforceMarketo3WayData](@entityType,@id,@clientId,@SFDCTitleLengthLimit,@integrationInstanceLogId,@isClientAllowCustomName) as T1
									--select @query
									IF(ISNULL(@dynResultQuery,'')<>'')
									BEGIN
										IF(ISNULL(@query,'')<>'')
										BEGIN
											SET @dynResultQuery = @dynResultQuery + ' UNION ' + @query
										END
									END
									ELSE IF(ISNULL(@query,'')<>'')
									BEGIN
										SET @dynResultQuery = @query
									END

								 END
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
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for Salesforce instance')
				END
			END
			ELSE IF(@isSyncSFDCWithMarketo='1')
			BEGIN
				
				select @query = query from [fnGetSalesforceMarketo3WayData](@entityType,@id,@clientId,@SFDCTitleLengthLimit,@integrationInstanceLogId,@isClientAllowCustomName) as T1
				--select @query
				IF(ISNULL(@query,'')<>'')
				BEGIN
					SET @dynResultQuery = @query
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
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push Salesforce')
	EXEC(@dynResultQuery)
	--select * from @tblSyncError
	--SELECT @logStartInstanceLogId as 'InstanceLogStartId'
END




GO
