--Added EntityType table for performance reason #2695 - performance improvement for grid view
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF  NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE table_name = 'EntityType' and TABLE_SCHEMA = 'dbo')
CREATE TABLE [dbo].[EntityType](
	[EntityTypeID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[Description] [varchar](150) NULL,
 CONSTRAINT [PK_EntityType] PRIMARY KEY CLUSTERED 
(
	[EntityTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
IF NOT EXISTS (SELECT 1 FROM EntityType)
BEGIN 
	SET IDENTITY_INSERT [dbo].[EntityType] ON 
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (1, N'Plan', NULL)
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (2, N'Campaign', NULL)
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (3, N'Program', NULL)
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (4, N'Tactic', NULL)
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (5, N'ImprovementTactic', NULL)
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (6, N'Lineitem', NULL)
	INSERT [dbo].[EntityType] ([EntityTypeID], [Name], [Description]) VALUES (7, N'MediaCode', NULL)
	SET IDENTITY_INSERT [dbo].[EntityType] OFF
END 
GO

--#2695 - performance improvement for grid view
--
--Add VARCHAR columns to speed up search 
--This done on dbo space on tow tables, CustomFieldOption and CustomRestriction
-- 
SET ANSI_PADDING ON;
GO

IF (NOT exists (SELECT 1 FROM sys.columns where name = 'CustomFieldOptionIDX' AND Object_ID = Object_ID(N'CustomFieldOption')))
ALTER TABLE CustomFieldOption 
ADD [CustomFieldOptionIDX]  AS (CONVERT([varchar](10),[CustomFieldOptionId],0)) PERSISTED
GO 

IF (NOT exists (SELECT 1 FROM sys.indexes where name = 'IDX_CustomFieldOption_CustomFieldOptionIDX' AND Object_ID = Object_ID(N'CustomFieldOption')))
CREATE NONCLUSTERED INDEX [IDX_CustomFieldOption_CustomFieldOptionIDX] ON [dbo].[CustomFieldOption]
(
	[CustomFieldOptionIDX] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

IF (NOT exists (SELECT 1 FROM sys.columns where name = 'CustomFieldOptionIDX' AND Object_ID = Object_ID(N'CustomRestriction')))
ALTER TABLE CustomRestriction 
ADD	[CustomFieldOptionIDX]  AS (CONVERT([varchar](50),[CustomFieldOptionId],0)) PERSISTED
GO 

IF (NOT exists (SELECT 1 FROM sys.indexes where name = 'IDX_CustomRestriction_CustomFieldOptionIDX' AND Object_ID = Object_ID(N'CustomRestriction')))
CREATE NONCLUSTERED INDEX [IDX_CustomRestriction_CustomFieldOptionIDX] ON [dbo].[CustomRestriction]
(
	[CustomFieldOptionIDX] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

--
--
--Create MV SCHEMA - a new schema to house all materialized views 
--
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'MV')
BEGIN
	EXEC('CREATE SCHEMA MV')
END
GO

--
--HELPER FUNCTION 1 to calculate value per entity and customField
--
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[fnCustomFieldEntityValue]'))
DROP FUNCTION [MV].[fnCustomFieldEntityValue]
GO

CREATE FUNCTION [MV].[fnCustomFieldEntityValue] (@EntityId INT, @CustomFieldId INT)
RETURNS VARCHAR(MAX)
AS
BEGIN
	RETURN SUBSTRING((	SELECT ',' + CAST(R.Value AS VARCHAR) FROM CustomField_Entity R
						WHERE R.EntityId = @EntityId
								AND R.CustomFieldId = @CustomFieldId
						FOR XML PATH('')), 2,900000) 
END
GO 

--
--HELPER FUNCTION 2 to calculate value per entity, customField and user
--
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[fnCustomFieldEntityUnrestrictedText]'))
DROP FUNCTION [MV].[fnCustomFieldEntityUnrestrictedText];
GO 

--
--This function returns all option values as if there is custom restriction
--
CREATE FUNCTION [MV].[fnCustomFieldEntityUnrestrictedText] (@EntityId INT, @CustomFieldId INT)
RETURNS VARCHAR(MAX)
AS
BEGIN
	RETURN SUBSTRING((	SELECT ',' + 
							CASE WHEN C.CustomFieldTypeId = 1 --text box 
								THEN R.Value
								ELSE CAST(CCP.Value AS VARCHAR(50)) 
							END
						FROM CustomField_Entity R
							JOIN CustomField C ON C.CustomFieldId = R.CustomFieldId
							LEFT JOIN CustomFieldOption CCP ON R.Value = CCP.CustomFieldOptionIDX

						WHERE R.EntityId = @EntityId AND R.CustomFieldId = @CustomFieldId
						FOR XML PATH('')), 2,900000) 
END
GO

--
-- This function returns onky allowed custom field options 
--
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[fnCustomFieldEntityRestrictedTextByUser]'))
DROP FUNCTION [MV].[fnCustomFieldEntityRestrictedTextByUser];
GO 

CREATE FUNCTION [MV].[fnCustomFieldEntityRestrictedTextByUser] (@CustomFieldId INT, @UserId INT)
RETURNS VARCHAR(MAX)
AS
BEGIN
	RETURN SUBSTRING((  SELECT ','+MAX(CFO.Value)
						FROM CustomFieldOption CFO
								INNER JOIN CustomRestriction CR ON CR.CustomFieldId = CFO.CustomFieldId
									AND CR.CustomFieldOptionId = CFO.CustomFieldOptionId
						WHERE CR.Permission=2 
								AND @CustomFieldId = CR.CustomFieldId
								AND UserId = @UserId
						GROUP BY CFO.CustomFieldOptionId
						FOR XML PATH('')),2,90000)
END 
GO

--
--Crearte [MV].[CustomFieldData] table to house the materialized view for custom field data
-- 
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[CustomFieldData]'))
DROP TABLE [MV].[CustomFieldData]
GO
 
CREATE TABLE [MV].[CustomFieldData](
	[EntityId] [INT] NOT NULL,
	[CustomFieldId] [INT] NOT NULL,
	[Value] [VARCHAR](MAX) NULL,
	[UnrestrictedText] [VARCHAR](MAX) NULL,
 CONSTRAINT [PK_CustomFieldData] PRIMARY KEY CLUSTERED 
(
	[EntityId] ASC,
	[CustomFieldId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

--
-- Create table [MV].[CustomFieldDataRestrictedUserOptions] - Materialized view 
--
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[CustomFieldEntityRestrictedTextByUser]'))
DROP TABLE [MV].[CustomFieldEntityRestrictedTextByUser]
GO

CREATE TABLE [MV].[CustomFieldEntityRestrictedTextByUser](
	[CustomFieldId] [INT] NOT NULL,
	[UserId] [INT] NOT NULL, 
	[Text] [VARCHAR](MAX) NULL,
 CONSTRAINT [PK_CustomFieldEntityRestrictedTextByUser] PRIMARY KEY CLUSTERED 
(
	[CustomFieldId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

--
--Procedute to populate MV data - in case we need refresh the view manually. 
--
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[spRePouplateMVData]'))
DROP PROCEDURE [MV].[spRePouplateMVData]
GO

CREATE PROCEDURE [MV].[spRePouplateMVData]
AS
BEGIN 
	--Repopulate [MV].CustomFieldData
	DELETE FROM [MV].CustomFieldData

	INSERT INTO [MV].CustomFieldData(EntityId, CustomFieldId, Value, UnrestrictedText)
	SELECT	 A.EntityId
			,A.CustomFieldId
			,[MV].fnCustomFieldEntityValue(A.EntityId, A.CustomFieldId) AS Value
			,[MV].fnCustomFieldEntityUnrestrictedText(A.EntityId, A.CustomFieldId) as [Text]
	FROM (  SELECT DISTINCT EntityID, CustomFieldId 
			FROM CustomField_Entity
		  ) A

	--re-Populate The [MV].[CustomFieldEntityRestrictedTextByUser] 
	DELETE FROM [MV].[CustomFieldEntityRestrictedTextByUser]

	INSERT INTO [MV].[CustomFieldEntityRestrictedTextByUser](CustomFieldId, UserId, [Text])
	SELECT	CR.CustomFieldId
			,CR.UserId
			,[MV].fnCustomFieldEntityRestrictedTextByUser(CR.CustomFieldId, CR.UserId) AS [Text]  
	FROM (SELECT DISTINCT CustomFieldId, UserId FROM CustomRestriction) CR 

END 
GO

--
--Populate MV data initually (for 4.9 release)
-- 
EXEC [MV].[spRePouplateMVData]
GO 

--
--Add trigger to update CustomFieldData materialized view  
--
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		John Zhang
-- Create date: Oct 7, 2016 
-- Description:	The trigger updates the BigViewCustomFieldData
-- =============================================

IF EXISTS (SELECT *FROM sys.triggers WHERE OBJECT_ID = OBJECT_ID('[dbo].UpdateCustomFieldData'))
	DROP TRIGGER  [dbo].UpdateCustomFieldData
GO

CREATE TRIGGER [dbo].UpdateCustomFieldData 
   ON  dbo.CustomField_Entity 
   AFTER INSERT,UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	---Handle changes 
	UPDATE  A 
	SET		A.[Value] = MV.fnCustomFieldEntityValue(B.EntityId, B.CustomFieldId)
		  , A.[UnrestrictedText] = MV.fnCustomFieldEntityUnrestrictedText(B.EntityId, B.CustomFieldId)
	FROM [MV].CustomFieldData A JOIN (		SELECT CE.EntityId, CE.CustomFieldId 
											FROM INSERTED CE 
												 JOIN CustomField CF ON CF.CustomFieldId = CE.CustomFieldId
											GROUP BY CE.EntityId, CE.CustomFieldId) B ON B.EntityId = A.EntityId AND B.CustomFieldId = A.CustomFieldId

	-- Handle inserts 
	INSERT INTO  [MV].CustomFieldData(EntityId, CustomFieldId, Value, UnrestrictedText)
	SELECT    CE.EntityId
			, CE.CustomFieldId
			, [MV].fnCustomFieldEntityValue(CE.EntityId, CE.CustomFieldId)
			, [MV].fnCustomFieldEntityUnrestrictedText(CE.EntityId, CE.CustomFieldId)
	FROM INSERTED CE 
			JOIN CustomField CF ON CF.CustomFieldId = CE.CustomFieldId
			LEFT JOIN [MV].CustomFieldData BV ON BV.EntityId = CE.EntityId AND BV.CustomFieldId = CE.CustomFieldId 
	WHERE BV.EntityId IS NULL
	GROUP BY CE.EntityId, CE.CustomFieldId

	--Handle delete 
	DELETE [MV].CustomFieldData
	FROM DELETED D LEFT JOIN INSERTED I ON D.EntityId = I.EntityId AND D.CustomFieldId = I.CustomFieldId
	WHERE [MV].CustomFieldData.EntityId = D.EntityId 
		AND [MV].CustomFieldData.CustomFieldId = D.CustomFieldId 
		AND I.EntityId IS NULL

END
GO

--
-- Add trigger to update [CustomFieldEntityRestrictedTextByUser] materialized view 
--
IF EXISTS (SELECT *FROM sys.triggers WHERE OBJECT_ID = OBJECT_ID('[dbo].UpdateCustomFieldEntityRestrictedTextByUser'))
	DROP TRIGGER  [dbo].UpdateCustomFieldEntityRestrictedTextByUser
GO

CREATE TRIGGER [dbo].UpdateCustomFieldEntityRestrictedTextByUser 
   ON  dbo.CustomRestriction 
   AFTER INSERT,UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	---Handle changes 
	UPDATE A 
	SET  A.[Text] = [MV].fnCustomFieldEntityRestrictedTextByUser(CR.CustomFieldId, CR.UserId)
	FROM [MV].CustomFieldEntityRestrictedTextByUser A 
			JOIN (SELECT DISTINCT UserId, CustomFieldID FROM INSERTED) CR ON CR.UserId = A.UserId AND CR.CustomFieldId = A.CustomFieldId

	-- Handle inserts 
	INSERT INTO  [MV].CustomFieldEntityRestrictedTextByUser(UserId, CustomFieldId, [Text])
	SELECT    I.UserId
			, I.CustomFieldId
			, [MV].fnCustomFieldEntityRestrictedTextByUser(I.CustomFieldId, I.UserId)
	FROM INSERTED I 
			JOIN CustomRestriction CR ON CR.CustomFieldId = I.CustomFieldId AND CR.UserId = I.UserId
			LEFT JOIN [MV].CustomFieldEntityRestrictedTextByUser BV ON BV.UserId = I.UserId AND BV.CustomFieldId = I.CustomFieldId 
	WHERE BV.CustomFieldId IS NULL
	GROUP BY I.UserId, I.CustomFieldId

	--Handle delete 
	DELETE [MV].CustomFieldEntityRestrictedTextByUser
	FROM DELETED D LEFT JOIN INSERTED I ON D.UserId = I.UserId AND D.CustomFieldId = I.CustomFieldId
	WHERE [MV].CustomFieldEntityRestrictedTextByUser.UserId = D.UserId 
		AND [MV].CustomFieldEntityRestrictedTextByUser.CustomFieldId = D.CustomFieldId 
		AND I.CustomFieldId IS NULL

END
GO

-------------------- Start -------------------- 
-- =============================================
-- Author: Viral Kadiya
-- Create date: 12-Oct-2016
-- Description:	Create Indexes on Improvement Campaign & Program Table
-- =============================================

IF (NOT exists (SELECT 1 FROM sys.indexes where name = 'NONCLUSTEREDINDEX_Plan_Improvement_Campaign_ImprovementPlanCampaignId' AND Object_ID = Object_ID(N'Plan_Improvement_Campaign')))
BEGIN
	CREATE NONCLUSTERED INDEX [NONCLUSTEREDINDEX_Plan_Improvement_Campaign_ImprovementPlanCampaignId]
	ON [dbo].[Plan_Improvement_Campaign] ([ImprovePlanId])
	INCLUDE ([ImprovementPlanCampaignId])
END
GO

IF (NOT exists (SELECT 1 FROM sys.indexes where name = 'NONCLUSTEREDINDEX_Plan_Improvement_Campaign_Program_ImprovementPlanProgramId' AND Object_ID = Object_ID(N'Plan_Improvement_Campaign_Program')))
BEGIN
	CREATE NONCLUSTERED INDEX [NONCLUSTEREDINDEX_Plan_Improvement_Campaign_Program_ImprovementPlanProgramId]
	ON [dbo].[Plan_Improvement_Campaign_Program] ([ImprovementPlanCampaignId])
	INCLUDE ([ImprovementPlanProgramId])
END
GO

-------------------- End -------------------- 


--#2695 END OF MV SCRIPT - Contact John Zhang if you have any questions. 

--#2557 - pulling finance data 
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[PullLineItemActuals]'))
	DROP PROCEDURE [INT].[PullLineItemActuals]
GO

CREATE PROCEDURE [INT].[PullLineItemActuals](@DataSource NVARCHAR(255), @ClientID INT, @UserID INT, @IntegrationInstanceID INT)
AS
BEGIN 
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='

		DECLARE @Updated INT;
		DECLARE @Inserted INT;
		DECLARE @Start DATETIME = GETDATE();

		UPDATE A
			SET A.Value = V.Amount
				, A.CreatedDate = GETDATE()
				, A.CreatedBy = '+STR(@UserId)+'
		FROM  Plan_Campaign_Program_Tactic_LineItem_Actual A 
				JOIN Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = A.PlanLineItemId
				JOIN Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
				JOIN '+@DataSource+' V ON V.LineItemID = A.PlanLineItemId AND A.Period = [INT].Period(T.StartDate, V.AccountingDate)

		SET @Updated = @@ROWCOUNT; 

		INSERT INTO  Plan_Campaign_Program_Tactic_LineItem_Actual (PlanLineItemId, Period, Value, CreatedDate, CreatedBy)
		SELECT	V.LineItemId, [INT].Period(T.StartDate, V.AccountingDate), V.Amount, GETDATE(), '+STR(@UserId)+'
		FROM	'+@DataSource+'  V
					JOIN Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = V.LineItemId
					JOIN Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual A ON V.LineItemID = A.PlanLineItemId AND [INT].Period(T.StartDate, V.AccountingDate) = A.Period 
		WHERE A.PlanLineItemId IS NULL

		SET @Inserted = @@ROWCOUNT;

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
			, ''Pulled LineItemActual From'' +''' + @DataSource + '''+STR(@Updated) + '' Updated.'' + STR(@Inserted) + '' Inserted.'' 
			, GETDATE()
			, ' + STR(@UserID) + '
			, 1  


	'
		BEGIN TRY 
			--PRINT @CustomQuery;
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

GO




--refs #2666 - re-order the insert columns due to column position shift 
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[UpdateTacticInstanceTacticId_Comment_API]'))
	DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
GO

CREATE  PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
	@strCreatedTacIds nvarchar(max)='',
	@strUpdatedTacIds nvarchar(max)='',
	@strCrtCampaignIds nvarchar(max)='',
	@strUpdCampaignIds nvarchar(max)='',
	@strCrtProgramIds nvarchar(max)='',
	@strUpdProgramIds nvarchar(max)='',
	@strCrtImprvmntTacIds nvarchar(max)='',
	@strUpdImprvmntTacIds nvarchar(max)='',
	@isAutoSync bit='0',
	@UserId INT,
	@integrationType varchar(100)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @instanceTypeMarketo varchar(50)='Marketo'
	Declare @instanceTypeSalesforce varchar(50)='Salesforce'
	Declare @AttrType varchar(50)='MarketoUrl'
	Declare @entType varchar(20)='Tactic'
	Declare @strAllPlanTacIds nvarchar(max)=''

	-- Start:- Declare comment related variables for each required entity.
	Declare @TacticSyncedComment varchar(500) = 'Tactic synced with ' + @integrationType
	Declare @TacticUpdatedComment varchar(500) = 'Tactic updated with ' + @integrationType
	Declare @ProgramSyncedComment varchar(500) = 'Program synced with ' + @integrationType
	Declare @ProgramUpdatedComment varchar(500) = 'Program updated with ' + @integrationType
	Declare @CampaignSyncedComment varchar(500) = 'Campaign synced with ' + @integrationType
	Declare @CampaignUpdatedComment varchar(500) = 'Campaign updated with ' + @integrationType
	Declare @ImprovementTacticSyncedComment varchar(500) = 'Improvement Tactic synced with ' + @integrationType
	Declare @ImprovementTacticUpdatedComment varchar(500) = 'Improvement Tactic updated with ' + @integrationType
	-- End:- Declare comment related variables for each required entity.

	IF(@strCreatedTacIds<>'')
	BEGIN
		SET @strAllPlanTacIds = @strCreatedTacIds
	END
	IF(@strUpdatedTacIds<>'')
	BEGIN
		IF(@strAllPlanTacIds<>'')
		BEGIN
			SET @strAllPlanTacIds = @strAllPlanTacIds+','+@strUpdatedTacIds
		END
		ELSE
		BEGIN
			SET @strAllPlanTacIds = @strUpdatedTacIds
		END
	END

	IF(@integrationType = @instanceTypeSalesforce)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))
	END
	ELSE IF(@integrationType = @instanceTypeMarketo)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceMarketoID=tac1.IntegrationInstanceMarketoID,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Update Marketo URL for Linked tactic
		Update lnkEnt set lnkEnt.AttrValue=orgEnt.AttrValue from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		INNER JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Marketo URL for Linked tactic
		INSERT INTO EntityIntegration_Attribute(EntityId,EntityType,IntegrationinstanceId,AttrType,AttrValue,CreatedDate) 
		SELECT tac1.LinkedTacticId,@entType,orgEnt.IntegrationinstanceId,orgEnt.AttrType,orgEnt.AttrValue,GETDATE()
		from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		LEFT JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ',')) and lnkEnt.EntityId IS NULL AND tac1.LinkedTacticId > 0

	END

	IF(@isAutoSync =0)
	BEGIN
		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
		Create Table #tmp_Plan_Campaign_Program_Tactic_Comment(CommentId int,Tacticid int)
		
		-- Insert comment for PlanTactic
		Insert Into Plan_Campaign_Program_Tactic_Comment ([PlanTacticId]
														  ,[Comment]
														  ,[CreatedDate]
														  ,[CreatedBy]
														  ,[PlanProgramId]
														  ,[PlanCampaignId])
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@TacticSyncedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@TacticUpdatedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment ([PlanTacticId]
														  ,[Comment]
														  ,[CreatedDate]
														  ,[CreatedBy]
														  ,[PlanProgramId]
														  ,[PlanCampaignId])
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Comment for Plan Campaign
		IF( (@strCrtCampaignIds <>'') OR (@strUpdCampaignIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment([PlanTacticId]
															  ,[Comment]
															  ,[CreatedDate]
															  ,[CreatedBy]
															  ,[PlanProgramId]
															  ,[PlanCampaignId])
			SELECT null,@CampaignSyncedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strCrtCampaignIds, ','))
			UNION
			SELECT null,@CampaignUpdatedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strUpdCampaignIds, ','))
		END

		-- Insert Comment for Plan Program
		IF( (@strCrtProgramIds <>'') OR (@strUpdProgramIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment ([PlanTacticId]
															  ,[Comment]
															  ,[CreatedDate]
															  ,[CreatedBy]
															  ,[PlanProgramId]
															  ,[PlanCampaignId])
			SELECT null,@ProgramSyncedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strCrtProgramIds, ','))
			UNION
			SELECT null,@ProgramUpdatedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strUpdProgramIds, ','))
		END

		-- Insert Comment for Improvement Tactic
		IF( (@strCrtImprvmntTacIds <>'') OR (@strUpdImprvmntTacIds <>'') )
		BEGIN
			INSERT Into Plan_Improvement_Campaign_Program_Tactic_Comment ( [ImprovementPlanTacticId]
																		  ,[Comment]
																		  ,[CreatedDate]
																		  ,[CreatedBy])
			SELECT ImprovementPlanTacticId,@ImprovementTacticSyncedComment,GETDATE(),@userId from Plan_Improvement_Campaign_Program_Tactic where ImprovementPlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCrtImprvmntTacIds, ','))
			UNION
			SELECT ImprovementPlanTacticId,@ImprovementTacticUpdatedComment,GETDATE(),@userId from Plan_Improvement_Campaign_Program_Tactic where ImprovementPlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdImprvmntTacIds, ','))
		END

		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
	END
    
END
GO
--production issue - tactic start date is earlier than actuals 
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[PullActuals]'))
	DROP PROCEDURE [INT].[PullActuals]
GO

CREATE PROCEDURE [INT].[PullActuals](@DataSource NVARCHAR(255), @StageTitle NVARCHAR(255),  @ClientID INT, @UserID INT, @IntegrationInstanceID INT)
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='
	
		DECLARE @Start DATETIME = GETDATE();
		DECLARE @Deleted INT = 0;
		DECLARE @Updated INT = 0;
		DECLARE @Inserted INT = 0;

		DELETE A
		FROM dbo.Plan_Campaign_Program_Tactic_Actual A JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = A.PlanTacticId
				LEFT JOIN ' + @DataSource + ' R ON R.PulleeID = T.IntegrationInstanceTacticId 
		WHERE  (A.CreatedBy = ' + STR(@UserID) + ' OR A.ModifiedBy = ' + STR(@UserID) + ' ) AND A.StageTitle = ''' + @StageTitle + ''' AND R.PulleeID IS NULL 

		SET @Deleted = @@ROWCOUNT;

		UPDATE dbo.Plan_Campaign_Program_Tactic_Actual 
		SET Actualvalue = V.ActualValue
			, CreatedDate = V.ModifiedDate
			, ModifiedBy = ' + STR(@UserID)+ ' 
			, ModifiedDate = GETDATE()
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + ' V ON V.PulleeID = T.IntegrationInstanceTacticId
		WHERE dbo.Plan_Campaign_Program_Tactic_Actual.PlanTacticId = T.PlanTacticId
			  AND dbo.Plan_Campaign_Program_Tactic_Actual.StageTitle = V.StageTitle
			  AND dbo.Plan_Campaign_Program_Tactic_Actual.Period = [INT].Period(T.StartDate, V.ModifiedDate) 
			  AND T.StartDate <= V.ModifiedDate

		SET @Updated = @@ROWCOUNT;

		INSERT INTO  dbo.Plan_Campaign_Program_Tactic_Actual ([PlanTacticId]
														  ,[StageTitle]
														  ,[Period]
														  ,[Actualvalue]
														  ,[CreatedDate]
														  ,[CreatedBy]
														  ,[ModifiedDate]
														  ,[ModifiedBy])
		SELECT T.PlanTacticId
				, V.StageTitle
				, [INT].Period(T.StartDate, V.ModifiedDate)
				, V.ActualValue
				, V.ModifiedDate
				, ' + STR(@UserID) + '
				, GETDATE()
				, ' + STR(@UserID) + '
		FROM dbo.Plan_Campaign_Program_Tactic T JOIN ' + @DataSource + ' V ON V.PulleeID = T.IntegrationInstanceTacticId
			 LEFT JOIN dbo.Plan_Campaign_Program_Tactic_Actual A ON A.PlanTacticId = T.PlanTacticId AND A.StageTitle = V.StageTitle AND A.Period = [INT].Period(T.StartDate, V.ModifiedDate) 
		WHERE A.StageTitle IS NULL AND T.StartDate <= V.ModifiedDate

		SET @Inserted = @@ROWCOUNT;

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
			, ''Pulled '' + ''' + @StageTitle + '. From ' + @DataSource + ''' + STR(@Deleted) + '' Deleted.'' + STR(@Updated) + '' Updated.'' + STR(@Inserted) + '' Inserted.'' 
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

GO

/****** Object:  UserDefinedFunction [dbo].[fnGetPlanGanttStartEndDate]    Script Date: 10/05/2016 5:17:16 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetPlanGanttStartEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetPlanGanttStartEndDate]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetPlanGanttStartEndDate]    Script Date: 10/05/2016 5:17:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetPlanGanttStartEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
-- select * from [dbo].[fnGetPlanGanttStartEndDate_V](''thismonth'')

CREATE FUNCTION [dbo].[fnGetPlanGanttStartEndDate]
(
	@timeframe varchar(255)
)
RETURNS @start_endDate Table(
	startdate datetime,
	enddate datetime
)
AS
BEGIN
	
	-- This line is keep commented to test this function and verify data from SQL.
	--SELECT * from fnGetPlanGanttStartEndDate(''thisyear'') 

	-- Declare local variables
	BEGIN
		Declare @varThisYear varchar(10)=''thisyear''
		Declare @varThisQuarter varchar(15)=''thisquarter''
		Declare @varThisMonth varchar(10)=''thismonth''
		Declare @Start_Date datetime
		Declare @End_Date datetime
	END
	
	-- Check that if timeframe value is blank then set ''thisyear'' as default timeframe for Calendar
	IF(IsNull(@timeframe,'''')='''')
	BEGIN
		SET @timeframe =@varThisYear
	END


	-- Check that if timeframe value is ''thisyear'' then set Start Date & End date to ''01/01/2016'' to ''31/12/2016'' respectively
	IF(@timeframe = @varThisYear)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), 1, 1)
		SET @End_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), 12, 31)	
	END
	
	-- Check that if timeframe value is ''thisquarter'' then set Start Date & End date to ''01/07/2016'' to ''30/09/2016'' respectively
	ELSE IF(@timeframe = @varThisQuarter)
	BEGIN

		Declare @currentQuarter int 
		SET @currentQuarter = ((DATEPART(MM,GETDATE()) - 1) / 3) + 1
		
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), (@currentQuarter - 1) * 3 + 1, 1)
		SET @End_Date = DATEADD(DD,-1,DATEADD(MM,3,@Start_Date))		
	END
	
	-- Check that if timeframe value is ''thismonth'' then set Start Date & End date to ''01/09/2016'' to ''30/09/2016'' respectively
	ELSE IF(@timeframe = @varThisMonth)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), DATEPART(MM,GETDATE()), 1)
		SET @End_Date = DATEADD(DD,-1,DATEADD(MM,1,@Start_Date))
	END

	-- Check that if timeframe value is ''2016'' then set Start Date & End date to ''01/01/2016'' to ''31/12/2016'' respectively
	ELSE IF(ISNUMERIC(@timeframe) = 1)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (CAST(@timeframe as int), 1, 1)
		SET @End_Date = DATEFROMPARTS (CAST(@timeframe as int), 12, 31)		
	END
	
	-- When timeframe multiyear(ex. 2016-2017) then set Start Date & End date to ''01/01/2016'' to ''31/12/2017'' respectively
	ELSE
	BEGIN

		Declare @Year1 int
		Declare @Year2 int
		
		SELECT  @Year1 = MIN(Cast(val as int)), @Year2 = MAX(Cast(val as int)) FROM [dbo].comma_split(@timeframe,''-'')	-- Split timeframe ''2016-2017'' value.
		
		SET @Start_Date = DATEFROMPARTS (@Year1, 1, 1)
		SET @End_Date = DATEFROMPARTS (@Year2 , 12, 31)		
	END

	INSERT INTO @start_endDate  SELECT @Start_Date ,@End_Date 
	RETURN 
END
' 
END

GO

-- Modified by Viral to correct the function name so remove old function name
-- Remove the below function if exist
/****** Object:  UserDefinedFunction [dbo].[fnGetEntitieHirarchyByPlanId]    Script Date: 10/17/2016 7:59:14 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntitieHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntityHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetEntityHirarchyByPlanId]
GO

-- Merge Filter EntityHierarchy and EntityHierarchy function into single function
/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 10/26/2016 7:15:03 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetFilterEntityHierarchy]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 10/26/2016 7:15:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[fnGetFilterEntityHierarchy]
(
	@planIds varchar(max)='''',
	@ownerIds nvarchar(max)='''',
	@tactictypeIds varchar(max)='''',
	@statusIds varchar(max)='''',
	@TimeFrame varchar(20)='''',
	@isGrid bit=0
)

RETURNS @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15),
			EntityTypeID	INT, 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		INT,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT,
			EloquaId		nvarchar(100),
			MarketoId		nvarchar(100),
			WorkfrontID		nvarchar(100),
			SalesforceId	nvarchar(100),
			ROIPackageIds	Varchar(max)
		)
AS
BEGIN


BEGIN
	DECLARE @TimeFrameDatesAndYear Table
	(
		PlanYear varchar(10)
	)

	DECLARE @MinYear VARCHAR(10)--minimum year passed in timeframe perameter
	DECLARE @MaxYear VARCHAR(10)--Maximum year passed in timeframe perameter
	DECLARE @StartDate DateTime --Start date perameter to filter entities e.g. campaign,program and tactic
	DECLARE @EndDate DateTime   --End date perameter to filter entities e.g. campaign,program and tactic

	--Set first year with first start date of year and last year with last date of year
	IF (@TimeFrame IS NOT NULL AND @TimeFrame <>'''')
		BEGIN
			INSERT INTO @TimeFrameDatesAndYear
			SELECT Item as PlanYear from dbo.SplitString(@TimeFrame,''-'')--split timeframe parameter e.g. 2015-2016
			
			--Set Minimum & Maximumyear
			SELECT @MinYear=CONVERT(VARCHAR,( MIN(CONVERT(INT,PlanYear)))),@MaxYear=CONVERT(VARCHAR,( MAX(CONVERT(INT,PlanYear)))) FROM @TimeFrameDatesAndYear 
	
			SET @StartDate= CONVERT(DATETIME,@MinYear+''-01-01 00:00:00'') --Set first date of minimum year
			SET @EndDate= CONVERT(DATETIME,@MaxYear+''-12-31 00:00:00'')   --Set Last date of maximum year

		END
	

	-- Insert Plan Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,EntityType,[Status],CreatedBy,AltId
										,TaskId,PlanId,ModelId)
		SELECT ''P_'' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId
				,P.PlanId EntityId
				, P.Title EntityTitle
				,''Plan'' EntityType
				, P.Status
				, P.CreatedBy 
				,CAST(P.PlanId AS NVARCHAR(50)) AS AltId
				,''L''+CAST(P.PlanId AS NVARCHAR(50)) AS TaskId
				,P.PlanId
				,P.ModelId
		FROM [Plan] P 
		WHERE P.IsDeleted = 0 
				AND (
						@PlanIds IS NULL 
						OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,'',''))
					) 
	END

	-- Insert Campaign Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,[Status],StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId,WorkfrontID,SalesforceId)
		SELECT ''P_C_'' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId
				,C.PlanCampaignId EntityId
				, C.Title EntityTitle
				, P.EntityId ParentEntityId
				,P.UniqueId ParentUniqueId
				,''Campaign'' EntityType
				, C.Status
				, C.StartDate StartDate
				, C.EndDate EndDate
				,C.CreatedBy 
			,CAST(C.PlanId AS NVARCHAR(500))+''_''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS AltId
			,CAST(P.TaskId AS NVARCHAR(500))+''_C''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS TaskId
			,''L''+CAST(C.PlanId  AS NVARCHAR(500)) AS ParentTaskId
			,P.PlanId
			,P.ModelId
			,c.IntegrationWorkFrontProgramID as WorkFrontid,c.IntegrationInstanceCampaignId as Salesforceid

			FROM Plan_Campaign C
			INNER JOIN @Entities P ON P.EntityId = C.PlanId and P.EntityType=''Plan''
			WHERE C.IsDeleted = 0 AND (@isGrid=1 OR (C.StartDate>=@StartDate AND C.StartDate<=@EndDate) OR (C.EndDate>=@StartDate AND C.EndDate<=@EndDate))
	END

	-- Insert Program Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,[Status],StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId,SalesforceId)

		SELECT ''P_C_P_'' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId
			,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId,''Program'' EntityType, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy 
			,CAST(P.PlanCampaignId AS NVARCHAR(500))+''_''+CAST(P.PlanProgramId AS NVARCHAR(50)) As AltId
			,CAST(C.TaskId AS NVARCHAR(500))+''_P''+CAST(P.PlanProgramId AS NVARCHAR(50)) As TaskId
			,CAST(C.ParentTaskId AS NVARCHAR(500))+''_C''+CAST(P.PlanCampaignId AS NVARCHAR(50)) As ParentTaskId
			,C.PlanId
			,C.ModelId
			,P.IntegrationInstanceProgramId as Salesforceid
			FROM Plan_Campaign_Program P
			INNER JOIN @Entities C ON C.EntityId = P.PlanCampaignId and C.EntityType=''Campaign''
			WHERE P.IsDeleted = 0 AND (@isGrid=1 OR (P.StartDate>=@StartDate AND P.StartDate<=@EndDate) OR (P.EndDate>=@StartDate AND P.EndDate<=@EndDate))
	END

	-- Insert Tactic Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,[Status],StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId,EloquaId,MarketoId,WorkfrontID,SalesforceId,ROIPackageIds)

		SELECT ''P_C_P_T_'' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId,''Tactic'' EntityType, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy 
			,CAST(T.PlanProgramId AS NVARCHAR(500))+''_''+CAST(T.PlanTacticId AS NVARCHAR(50)) As AltId
			,CAST(P.TaskId AS NVARCHAR(500))+''_T''+CAST(T.PlanTacticId AS NVARCHAR(50)) As TaskId
			,CAST(P.ParentTaskId AS NVARCHAR(500))+''_P''+CAST(T.PlanProgramId AS NVARCHAR(50)) As ParentTaskId
			,P.PlanId
			,P.ModelId
			,T.IntegrationInstanceEloquaId as Eloquaid,T.IntegrationInstanceMarketoID as Marketoid,T.IntegrationWorkFrontProjectID as WorkFrontid,T.IntegrationInstanceTacticId as Salesforceid
			,R.PackageIds as ROIPackageIds
			FROM Plan_Campaign_Program_Tactic T
			INNER JOIN @Entities P ON P.EntityId = T.PlanProgramId and P.EntityType=''Program''
			INNER JOIN [TacticType] as typ on T.TacticTypeId = typ.TacticTypeId and typ.IsDeleted=''0'' and 
			(@tactictypeIds = ''All'' OR typ.[TacticTypeId] IN (select val from comma_split(@tactictypeIds,'','')))

			LEFT JOIN (SELECT AnchorTacticID,PackageIds=
					STUFF((SELECT '', '' + Cast(PlanTacticId as varchar)
					       FROM ROI_PackageDetail b 
					       WHERE b.AnchorTacticID = a.AnchorTacticID 
					      FOR XML PATH('''')), 1, 2, '''')
					FROM @Entities as P1
					JOIN Plan_Campaign_Program_Tactic T2 on P1.EntityId = T2.PlanProgramId and P1.EntityType=''Program''
					JOIN ROI_PackageDetail as a on T2.PlanTacticId = a.AnchorTacticID
					GROUP BY a.AnchorTacticID
					
				) as R on T.PlanTacticId = R.AnchorTacticId

			WHERE T.IsDeleted = 0 AND (@isGrid=1 OR (T.StartDate>=@StartDate AND T.StartDate<=@EndDate) OR (T.EndDate>=@StartDate AND T.EndDate<=@EndDate))
					AND T.[Status] IN (select val from comma_split(@statusIds,'','')) and  
					(@ownerIds = ''All'' OR T.[CreatedBy] IN (select case when val = '''' then null else Convert(int,val) end from comma_split(@ownerIds,'','')))
	END
	
	

	-- Insert LineItem Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId)
		SELECT ''P_C_P_T_L_'' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId,''LineItem'' EntityType, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy 
			,CAST(L.PlanTacticId AS NVARCHAR(500))+''_''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As AltId
			,CAST(T.TaskId AS NVARCHAR(500))+''_X''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As TaskId
			,CAST(T.ParentTaskId AS NVARCHAR(500))+''_T''+CAST(L.PlanTacticId AS NVARCHAR(50)) As ParentTaskId
			,T.PlanId
			,T.ModelId
			
			FROM Plan_Campaign_Program_Tactic_LineItem L
			INNER JOIN @Entities T ON T.EntityId = L.PlanTacticId and T.EntityType=''Tactic''
			WHERE L.IsDeleted = 0
	END

	-- Update ColorCode & EntityTypeId value
	BEGIN
		Update @Entities SET ColorCode = C.ColorCode, EntityTypeID = T.EntityTypeID
		FROM @Entities E
		LEFT JOIN EntityTypeColor C ON C.EntityType = E.EntityType
		LEFT JOIN EntityType T ON T.Name = E.EntityType
	END
END


RETURN

END

' 
END

GO



/****** Object:  UserDefinedFunction [dbo].[fnViewByEntityHierarchy]    Script Date: 10/18/2016 1:56:42 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnViewByEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnViewByEntityHierarchy]
GO

-- =============================================
-- Author:		Viral
-- Create date: 09/23/2016
-- Description:	Create Function to return data based on view by value.
-- =============================================
CREATE FUNCTION [dbo].[fnViewByEntityHierarchy]
(
	@planIds varchar(max),
	@ownerIds nvarchar(max),
	@tactictypeIds varchar(max),
	@statusIds varchar(max),
	@ViewBy varchar(500),
	@TimeFrame varchar(20)='',
	@isGrid bit=0
)
RETURNS 
@ResultEntities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			EntityTypeId	INT, 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		INT,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT,
			--EloquaId		nvarchar(100),
			--MarketoId		nvarchar(100),
			--WorkfrontID		nvarchar(100),
			--SalesforceId	nvarchar(100),
			ROIPackageIds	Varchar(max)
		)
AS
BEGIN

	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','Tactic')	
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','Status','2016',0)
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','Stage')
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','TacticCustom71')
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','ProgramCustom18')
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','CampaignCustom3')
	
	-- Declare Local variables
	BEGIN
			Declare @stage varchar(10)='Stage'
			Declare @ROIPackage varchar(20)='ROI Package'
			Declare @Status varchar(20)='Status'
			Declare @custom varchar(50)='Custom'
			
			Declare @entTactic varchar(50)='Tactic'
			Declare @entProgram varchar(50)='Program'
			Declare @entCampaign varchar(50)='Campaign'
			Declare @entPlan varchar(50)='Plan'
			
			Declare @entTypeId INT
			Declare @entTacticId INT = 4
			Declare @entProgramId INT = 3
			Declare @entCampaignId INT = 2
			Declare @entPlanId INT = 1
			
			Declare @custmEntityTypeId int 
			Declare @custCampaign varchar(20)='CampaignCustom'
			Declare @custProgram varchar(20)='ProgramCustom'
			Declare @custTactic varchar(20)='TacticCustom'
			Declare @isCustom bit='0'
			
			Declare @ResultViewByHierarchyEntities TABLE (
						UniqueId		NVARCHAR(30), 
						EntityId		BIGINT,
						EntityTitle		NVARCHAR(1000),
						ParentEntityId	BIGINT, 
						ParentUniqueId	NVARCHAR(30),
						EntityType		NVARCHAR(15), 
						EntityTypeId	INT, 
						ColorCode		NVARCHAR(7),
						[Status]		NVARCHAR(15), 
						StartDate		DATETIME, 
						EndDate			DATETIME, 
						CreatedBy		INT,
						AltId			NVARCHAR(500),
						TaskId			NVARCHAR(500),
						ParentTaskId	NVARCHAR(500),
						PlanId			BIGINT,
						ModelId			BIGINT,
						--EloquaId		nvarchar(100),
						--MarketoId		nvarchar(100),
						--WorkfrontID		nvarchar(100),
						--SalesforceId	nvarchar(100),
						ViewByTitle		NVARCHAR(500),
						ROIPackageIds	Varchar(max)
					)
			
			
			Declare @vwEntities TABLE (
						UniqueId		NVARCHAR(30), 
						EntityId		BIGINT,
						EntityTitle		NVARCHAR(1000),
						ParentEntityId	BIGINT, 
						ParentUniqueId	NVARCHAR(30),
						EntityType		NVARCHAR(15), 
						EntityTypeId	INT, 
						ColorCode		NVARCHAR(7),
						[Status]		NVARCHAR(15), 
						StartDate		DATETIME, 
						EndDate			DATETIME, 
						CreatedBy		INT,
						AltId			NVARCHAR(500),
						TaskId			NVARCHAR(500),
						ParentTaskId	NVARCHAR(500),
						PlanId			BIGINT,
						ModelId			BIGINT,
						--EloquaId		nvarchar(100),
						--MarketoId		nvarchar(100),
						--WorkfrontID		nvarchar(100),
						--SalesforceId	nvarchar(100),
						ROIPackageIds	Varchar(max)
					)
			
					Declare @distViewByValues Table(
					 ViewByTitle NVarchar(max),
					 ViewById Varchar(max)
					)
							
					Declare @tblEntityViewByMapping Table(
						EntityId bigint,
						ViewByValue Nvarchar(1000) 
					)
			
	END
			-- If Viewby is Tactic then return Filter result set table
			IF(@ViewBy = @entTactic)
			BEGIN
				
				INSERT Into @ResultEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)		
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds
				FROM		fnGetFilterEntityHierarchy(@planIds,@ownerIds,@tactictypeIds,@statusIds,@TimeFrame,@isGrid)
	
				RETURN
			END
			ELSE
			BEGIN
				-- GET Data with applying required filter and insert into local table to re use for further process.
				INSERT Into @vwEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)		
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds
				FROM		fnGetFilterEntityHierarchy(@planIds,@ownerIds,@tactictypeIds,@statusIds,@TimeFrame,@isGrid)
			END
	
	
			-- Insert distinct ViewBy values of Tactics.
			If(@ViewBy = @Status)
			BEGIN
				SET @entTypeId = @entTacticId	-- Prepared View by structure based on Tactic
	
				Insert Into @distViewByValues(ViewByTitle,ViewById) 
				Select Distinct [Status],[Status] from @vwEntities where EntityTypeId=@entTypeId
	
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct EntityId,[Status] from @vwEntities where EntityTypeId=@entTypeId
			END
			
			ELSE If(@ViewBy = @stage)
			BEGIN
				SET @entTypeId = @entTacticId	-- Prepared View by structure based on Tactic
			
			-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
				Insert Into @distViewByValues(ViewByTitle,ViewById)
				Select Distinct S.Title,Cast(S.StageId as varchar) from @vwEntities as H
				Join Plan_Campaign_Program_Tactic as T on H.EntityId = T.PlanTacticId and EntityTypeId=@entTypeId
				JOIN Stage as S on T.StageId = S.StageId and S.IsDeleted='0'
	
				-- Insert Entity and ViewBy value mapping records to local table for further process.
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct H.EntityId,Cast(S.StageId as varchar) from @vwEntities as H
				Join Plan_Campaign_Program_Tactic as T on H.EntityId = T.PlanTacticId and EntityTypeId=@entTypeId
				JOIN Stage as S on T.StageId = S.StageId and S.IsDeleted='0'
	
			END
			
			ELSE If(@ViewBy = @ROIPackage)
			Begin
				SET @entTypeId = @entTacticId	-- Prepared View by structure based on Tactic
	
				-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
				Insert Into @distViewByValues(ViewByTitle,ViewById)
				Select Distinct H.EntityTitle,Cast(H.EntityId  as varchar(max)) FROM @vwEntities as H
				JOIN ROI_PackageDetail as ROI ON H.EntityId = ROI.AnchorTacticID
				WHERE H.EntityTypeId=@entTypeId
	
				-- Insert Entity and ViewBy value mapping records to local table for further process.
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct H.EntityId,Cast(ROI.AnchorTacticID as varchar)  FROM @vwEntities as H
				JOIN ROI_PackageDetail as ROI ON H.EntityId = ROI.PlanTacticId
				WHERE H.EntityTypeId=@entTypeId
			END
	
			ELSE 
			BEGIN
				
				-- Identify that view by custom value is Campaign type or not
				IF EXISTS(SELECT 1 WHERE @ViewBy like '%'+@custCampaign+'%')
				BEGIN
					SET @entTypeId = @entCampaignId
					SET @isCustom = '1'
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custCampaign,'')
				END
	
				-- Identify that view by custom value is Program type or not
				ELSE IF EXISTS(SELECT 1 WHERE @ViewBy like '%'+@custProgram+'%')
				BEGIN
					SET @entTypeId = @entProgramId
					SET @isCustom = '1'
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custProgram,'')
				END
				
				-- Identify that view by custom value is Tactic type or not
				ELSE IF EXISTS(SELECT 1 WHERE @ViewBy like '%'+@custTactic+'%')
				BEGIN
					SET @entTypeId = @entTacticId
					SET @isCustom = '1'
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custTactic,'')
				END
	
				IF(@isCustom ='1')	-- If View by selection is Customfield then 
				BEGIN
	
					-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
					Insert Into @distViewByValues(ViewByTitle,ViewById) 
					Select Distinct CFO.Value,Cast(CFO.CustomFieldOptionId as varchar) from @vwEntities as H
					JOIN CustomField_Entity as CE on H.EntityId = CE.EntityId and H.EntityTypeId=@entTypeId and CE.CustomFieldId=@custmEntityTypeId
					JOIN CustomFieldOption as CFO on CE.CustomFieldId = CFO.CustomFieldId and CFO.CustomFieldOptionId=CE.Value and CFO.IsDeleted='0'
					where H.EntityTypeId=@entTypeId
	
					-- Insert Entity and ViewBy value mapping records to local table for further process.
					Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
					Select Distinct H.EntityId,Cast(CFO.CustomFieldOptionId as varchar) from @vwEntities as H
					JOIN CustomField_Entity as CE on H.EntityId = CE.EntityId and H.EntityTypeId=@entTypeId and CE.CustomFieldId=@custmEntityTypeId
					JOIN CustomFieldOption as CFO on CE.CustomFieldId = CFO.CustomFieldId and CFO.CustomFieldOptionId=CE.Value and CFO.IsDeleted='0'
					where H.EntityTypeId=@entTypeId
				END
			END
	
			
			-- Insert Distinct view by values to Result set.
			INSERT INTO @ResultViewByHierarchyEntities(
						EntityTitle,[EntityType],ViewByTitle,TaskId)
			SELECT		ViewByTitle,@ViewBy,ViewByTitle,'Z'+ViewById 
			FROM		@distViewByValues
	
			-- Insert Entity(based on value @entType set) for all ViewBy
			INSERT INTO @ResultViewByHierarchyEntities
			SELECT		H.UniqueId ,H.EntityId ,H.EntityTitle ,H.ParentEntityId ,H.ParentUniqueId ,H.EntityType, H.EntityTypeId,H.ColorCode,H.[Status],H.StartDate,H.EndDate,H.CreatedBy,H.AltId			
						,'Z'+R.ViewByValue+'_'+H.TaskId		
						,'Z'+R.ViewByValue+'_'+H.ParentTaskId
						,H.PlanId ,H.ModelId ,
						R.ViewByValue,H.ROIPackageIds
			FROM		@distViewByValues as DV
			JOIN		@tblEntityViewByMapping as R on DV.ViewById = R.ViewByValue
			JOIN		@vwEntities as H on R.EntityId = H.EntityId and H.EntityTypeId=@entTypeId
			
	
			-- Get Parent Hierarchy
			BEGIN
	
				-- Get Distinct Entity(based on value @entType set) ParentEntityId by ViewBy value
				Declare @prntEntityTable Table (
					ParentUniqueId NVARCHAR(500),
					ViewByTitle Nvarchar(max)
				)
	
				-- Create ParentEntity distinct Unique ids into local table to create parent hierarchy
				Insert Into @prntEntityTable(ParentUniqueId,ViewByTitle)
				Select Distinct R.ParentUniqueId,R.ViewByTitle
				FROM @ResultViewByHierarchyEntities as R
				JOIN @distViewByValues as V on R.ViewByTitle  = V.ViewById and R.EntityTypeId=@entTypeId
				Group By R.ViewByTitle,R.ParentUniqueId
	
				
				;WITH prnt AS 
				(
						
						(
							SELECT	H.UniqueId ,H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId			
									,'Z'+C.ViewByTitle+'_'+H.TaskId as TaskId
									,'Z'+C.ViewByTitle+'_'+H.ParentTaskId as ParentTaskId
									,H.PlanId, H.ModelId, C.ViewByTitle,
									H.ROIPackageIds
							FROM @vwEntities H
							JOIN @prntEntityTable as C ON H.UniqueId = C.ParentUniqueId
						
						)
	
							UNION ALL 
	
						(
							-- Get recursive parents data based
							SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy ,H.AltId		
									,'Z'+P.ViewByTitle+'_'+H.TaskId as TaskId
									,'Z'+P.ViewByTitle+'_'+H.ParentTaskId as ParentTaskId
									,H.PlanId, H.ModelId, P.ViewByTitle,
									H.ROIPackageIds
							FROM @vwEntities H
							JOIN prnt as P ON H.UniqueId = P.ParentUniqueId
							
						)
				
				)
				
	
				INSERT INTO @ResultViewByHierarchyEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds)
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds 
				FROM		prnt
			
			END
	
			-- Get Child hierarchy data
			BEGIN
				IF(@isCustom ='1')	-- Identify that view by is Custom field or not
				BEGIN
	
					;WITH child AS 
					(
						(
							-- Get Parent records from @ResultViewByHierarchyEntities to create child hierarchy data.
							SELECT	UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds
							FROM	@ResultViewByHierarchyEntities 
							WHERE	EntityTypeId=@entTypeId
						)
							UNION ALL 
	
						(
							-- Get recursive child data based on above parents query
							SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId,
									Cast('Z'+C.ViewByTitle+'_'+H.TaskId as nvarchar(500))  as TaskId,
									C.TaskId as ParentTaskId,
									H.PlanId, H.ModelId, C.ViewByTitle,
									H.ROIPackageIds
							FROM	@vwEntities as H
							JOIN	child C on C.UniqueId = H.ParentUniqueId
						)
					)
					
					--select * from child
	
					INSERT INTO @ResultViewByHierarchyEntities (
								UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds)
					SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds 
					FROM		child 
					WHERE EntityTypeId <> @entTypeId
				END
				ELSE
				BEGIN
					INSERT INTO @ResultViewByHierarchyEntities (
								UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds)
				
					SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType,H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId,
							'Z'+R.ViewByTitle+'_'+H.TaskId,
							R.TaskId, H.PlanId, H.ModelId, R.ViewByTitle,
							H.ROIPackageIds
					FROM	@ResultViewByHierarchyEntities as R
					JOIN	@vwEntities H on R.UniqueId  = H.ParentUniqueId and R.EntityTypeId=@entTypeId
				END
			END
	
			-- Update Unique & ParentUniqueId
			Update @ResultViewByHierarchyEntities SET UniqueId='Z'+ViewByTitle+'_'+UniqueId,ParentUniqueId='Z'+ViewByTitle+'_'+ParentUniqueId
			where EntityType <> @ViewBy


			-- Update Plan ParentUniqueId & ParentTaskID value
			Update @ResultViewByHierarchyEntities set ParentTaskId = 'Z'+ViewByTitle,ParentUniqueId = 'Z'+ViewByTitle 
			WHERE	EntityType=@entPlan 

			-- Update UniqueId value
			Update @ResultViewByHierarchyEntities set UniqueId = TaskId 
			WHERE  EntityType = @ViewBy
	
			-- Insert data to result set.
			Insert INTO @ResultEntities (
						UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)
			SELECT		Distinct UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds
			FROM		@ResultViewByHierarchyEntities
	
	
		RETURN 
END

GO

/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 10/26/2016 7:15:03 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGridFilters]
GO

/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 10/26/2016 7:15:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridFilters] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetGridFilters] 
	@userId int
	,@ClientId int 
AS --Todo: New user login then need to some more 
BEGIN
	
	--EXEC GetGridFilters 549,4

	SET NOCOUNT ON;

		Declare @PlanId NVARCHAR(MAX) = ''
		Declare @OwnerIds NVARCHAR(MAX) = ''
		Declare @TacticTypeIds varchar(max)=''
		Declare @StatusIds varchar(max)=''
		Declare @customFields varchar(max)=''

		Declare @viewname  varchar(max)

		Declare @tblUserSavedViews Table(
		Id INT
		,ViewName NVARCHAR(max)
		,FilterName NVARCHAR(1000)
		,FilterValues NVARCHAR(max)
		,LastModifiedDate DATETIME
		,IsDefaultPreset BIT
		,Userid INT
		)

		Declare @keyPlan varchar(30)='Plan'
		Declare @keyOwner varchar(30)='Owner'
		Declare @keyStatus varchar(30)='Status'
		Declare @keyTacticType varchar(30)='TacticType'
		Declare @keyAll varchar(10)='All'
		Declare @keyCustomField varchar(100)='CustomField'
		

		select TOP 1 @viewname =  ViewName from Plan_UserSavedViews where Userid=@userId AND IsDefaultPreset = 1
		SET @viewname = ISNULL(@viewname,'')

		-- Insert user filters to local variables.
		INSERT INTO @tblUserSavedViews(Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid)
		select Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid from Plan_UserSavedViews where Userid=@userId AND ISNULL(ViewName,'') = @viewname

		IF EXISTS(select Id from @tblUserSavedViews)
		BEGIN
			
		

		-- Get PlanIds that user has selected under filter.
		SELECT TOP 1 @PlanId = FilterValues from @tblUserSavedViews where FilterName=@keyPlan

		-- Get OwnerIds that user has selected under filter.
		SELECT TOP 1 @OwnerIds = FilterValues from @tblUserSavedViews where FilterName=@keyOwner

		-- Get TacticTypeIds that user has selected under filter.
		SELECT TOP 1 @TacticTypeIds = FilterValues from @tblUserSavedViews where FilterName=@keyTacticType

		-- Get Status that user has selected under filter.
		SELECT TOP 1 @StatusIds = FilterValues from @tblUserSavedViews where FilterName=@keyStatus

		-- Get Status that user has selected under filter.
		SET @customFields = ''


		BEGIN

			Declare @customFieldId varchar(10)
			Declare @FilterValues varchar(max)
			Declare @cntFiltr INT
			
			
			DECLARE db_cursor CURSOR FOR  
			select REPLACE(FilterName,'CF_',''),FilterValues from @tblUserSavedViews where Userid=@userId and FilterName like'CF_%'
			
			OPEN db_cursor   
			FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues   
			
			WHILE @@FETCH_STATUS = 0   
			BEGIN   
					
				   select @cntFiltr = count(*) from CustomRestriction as CR
					JOIN CustomField as C on CR.CustomFieldId = C.CustomFieldId and ClientId=@ClientId and IsDeleted='0' and C.IsRequired='1' and ( (CR.Permission = 1)  OR (CR.Permission = 2) )
					where UserId = @userId and C.CustomFieldId=Cast(@customFieldId as INT) and cr.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
			
				IF (IsNull(@cntFiltr,0) > 0)
				BEGIN
					SET @customFields = @customFields + ',' + @customFieldId + '_' + REPLACE(@FilterValues,',',','+@customFieldId + '_' ) 
				END
			
			       FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues      
			END   
			
			CLOSE db_cursor   
			DEALLOCATE db_cursor
			if(LEN(@customFields) > 2)
			SET @customFields = SUBSTRING(@customFields,2, LEN(@customFields)-1) 

		END

		END
		ELSE
		BEGIN
			
			SELECT Top 1 @PlanId = p.PlanId from Model as M
			JOIN [Plan] as P on M.ModelId = P.ModelId and M.ClientId=@ClientId and P.IsDeleted='0' and M.IsDeleted='0'
			Order by P.Year desc, P.Title
			
			SET @OwnerIds=@keyAll
			SET @TacticTypeIds=@keyAll
			SET @StatusIds='Created,Submitted,Approved,In-Progress,Complete,Declined'
			SET @customFields = ''
		END

		select @PlanId PlanIds,@OwnerIds OwnerIds,@StatusIds StatusIds,@TacticTypeIds TacticTypeIds,@customFields CustomFieldIds

    
END

GO



--#2666
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[UpdateTacticInstanceTacticId_Comment]'))
	DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
GO

CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
	@strCreatedTacIds nvarchar(max),
	@strUpdatedTacIds nvarchar(max),
	@strUpdateComment nvarchar(max),
	@strCreateComment nvarchar(max),
	@isAutoSync bit='0',
	@UserId INT,
	@integrationType varchar(100)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @instanceTypeMarketo varchar(50)='Marketo'
	Declare @instanceTypeSalesforce varchar(50)='Salesforce'
	Declare @AttrType varchar(50)='MarketoUrl'
	Declare @entType varchar(20)='Tactic'

	Declare @strAllPlanTacIds nvarchar(max)=''
	IF(@strCreatedTacIds<>'')
	BEGIN
		SET @strAllPlanTacIds = @strCreatedTacIds
	END
	IF(@strUpdatedTacIds<>'')
	BEGIN
		IF(@strAllPlanTacIds<>'')
		BEGIN
			SET @strAllPlanTacIds = @strAllPlanTacIds+','+@strUpdatedTacIds
		END
		ELSE
		BEGIN
			SET @strAllPlanTacIds = @strUpdatedTacIds
		END
	END

	IF(@integrationType = @instanceTypeSalesforce)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))
	END
	ELSE IF(@integrationType = @instanceTypeMarketo)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceMarketoID=tac1.IntegrationInstanceMarketoID,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Update Marketo URL for Linked tactic
		Update lnkEnt set lnkEnt.AttrValue=orgEnt.AttrValue from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		INNER JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Marketo URL for Linked tactic
		INSERT INTO EntityIntegration_Attribute(EntityId,EntityType,IntegrationinstanceId,AttrType,AttrValue,CreatedDate) 
		SELECT tac1.LinkedTacticId,@entType,orgEnt.IntegrationinstanceId,orgEnt.AttrType,orgEnt.AttrValue,GETDATE()
		from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		LEFT JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ',')) and lnkEnt.EntityId IS NULL AND tac1.LinkedTacticId > 0

	END

	IF(@isAutoSync =0)
	BEGIN
		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
		Create Table #tmp_Plan_Campaign_Program_Tactic_Comment(CommentId int,Tacticid int)
		
		-- Insert comment for PlanTactic
		Insert Into Plan_Campaign_Program_Tactic_Comment ([PlanTacticId]
														  ,[Comment]
														  ,[CreatedDate]
														  ,[PlanProgramId]
														  ,[PlanCampaignId]
														  ,[CreatedBy])
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@strCreateComment,GETDATE(),null,null,@userId from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@strUpdateComment,GETDATE(),null,null,@userId from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment ([PlanTacticId]
														  ,[Comment]
														  ,[CreatedDate]
														  ,[PlanProgramId]
														  ,[PlanCampaignId]
														  ,[CreatedBy])
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.PlanProgramId,cmnt.PlanCampaignId,cmnt.CreatedBy from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
	END
    
END
GO

/****** Object:  Table [dbo].[User_CoulmnView]    Script Date: 09/14/2016 1:40:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_CoulmnView]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[User_CoulmnView](
	[ViewId] [int] IDENTITY(1,1) NOT NULL,
	[ViewName] [nvarchar](50) NULL,
	[CreatedBy] INT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifyBy] INT NULL,
	[ModifyDate] [datetime] NULL,
	[IsDefault] [bit] NULL,
	[GridAttribute] [xml] NULL,
	[BudgetAttribute] [xml] NULL,
 CONSTRAINT [PK_User_CoulmnView] PRIMARY KEY CLUSTERED 
(
	[ViewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

/****** Object:  StoredProcedure [dbo].[sp_GetCustomFieldList]    Script Date: 09/08/2016 12:46:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetCustomFieldList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[sp_GetCustomFieldList] AS' 
END
GO

ALTER PROCEDURE [dbo].[sp_GetCustomFieldList]
@ClientId INT 
AS
BEGIN
SET NOCOUNT ON;

Declare @CustomfieldType int
set @CustomfieldType=(select TOP 1 CustomFieldTypeId from CustomFieldType where Name='DropDownList')

SELECT distinct CustomField.CustomFieldId,CustomField.Name,CustomField.IsRequired,case when  CustomField.EntityType='LineItem' then 'Line Item' else CustomField.EntityType end as EntityType,ISnull(CustomFieldDependency.ParentCustomFieldId,0) as ParentId ,CustomFieldType.Name as CustomFieldType,
case when  CustomField.EntityType='Campaign' then 1
when  CustomField.EntityType='Program' then 2
when  CustomField.EntityType='Tactic' then 3
when  CustomField.EntityType='LineItem' then 4
end as entityorder
FROM CustomField (NOLOCK) 
inner join CustomFieldType on CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId
LEFT join CustomFieldDependency on
CustomField.CustomFieldId= CustomFieldDependency.ChildCustomFieldId
Left join CustomFieldOption on CustomField.CustomFieldId = CustomFieldOption.CustomFieldId 
 WHERE 
CustomField.IsDeleted=0 and (CustomField.CustomFieldTypeId <> @CustomfieldType or (CustomFieldOptionId IS NOT NULL AND CustomField.CustomFieldTypeId=@CustomfieldType ))
and ClientId= CASE WHEN @ClientId IS NULL THEN ClientId ELSE @ClientId END and CustomField.EntityType in('Campaign','Program','Tactic','LineItem')
group by CustomField.CustomFieldId,CustomField.Name,CustomField.IsRequired,CustomField.EntityType,CustomFieldDependency.ParentCustomFieldId,CustomFieldType.Name
order by entityorder,CustomField.Name

END

--select * from CustomFieldType where CustomFieldId=29
-- EXEC sp_GetCustomFieldList '464eb808-ad1f-4481-9365-6aada15023bd'
GO


/* Start- Added by Viral for Ticket #2595 on 10/14/2016 */

/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 10/14/2016 2:19:01 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spViewByDropDownList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spViewByDropDownList]
GO
/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 10/14/2016 2:19:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spViewByDropDownList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spViewByDropDownList] AS' 
END
GO


ALTER PROCEDURE  [dbo].[spViewByDropDownList] 
	-- Add the parameters for the stored procedure here
	@PlanId NVARCHAR(max),
	@ClientId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tblCustomerFieldIDs TABLE ( EntityType NVARCHAR(120), EntityID INT, CustomFieldID INT) 
	DECLARE @entCampaign varchar(20)='Campaign'
	DECLARE @entProgram varchar(20)='Program'
	DECLARE @entTactic varchar(20)='Tactic'
	DECLARE @PlanIds Table(
		PlanId int
	)

	INSERT INTO @PlanIds SELECT Cast(IsNUll(val,0) as int) FROM dbo.comma_split(@Planid, ',')

	INSERT INTO @tblCustomerFieldIDs
	SELECT @entCampaign, A.PlanCampaignId, B.CustomFieldId 
		FROM Plan_Campaign A 
		JOIN CustomField_Entity B ON A.PlanCampaignId = B.EntityId
		JOIN CustomField CS ON B.CustomFieldId = CS.CustomFieldId and CS.EntityType=@entCampaign
		WHERE A.IsDeleted='0' and A.PlanId IN (SELECT PlanId FROM @PlanIds)
	
	INSERT INTO @tblCustomerFieldIDs
	SELECT @entProgram, A.PlanProgramId, B.CustomFieldId 
		FROM Plan_Campaign_Program A 
		JOIN CustomField_Entity B ON A.PlanProgramId = B.EntityId
		JOIN CustomField CS ON B.CustomFieldId = CS.CustomFieldId and CS.EntityType=@entProgram
		JOIN Plan_Campaign C ON A.PlanCampaignId = C.PlanCampaignId and C.IsDeleted='0' and C.PlanId IN (SELECT PlanId FROM @PlanIds)
		WHERE A.IsDeleted='0'
	
	INSERT INTO @tblCustomerFieldIDs
	SELECT @entTactic, A.PlanTacticId, B.CustomFieldId 
		FROM Plan_Campaign_Program_Tactic A 
		JOIN CustomField_Entity B ON A.PlanTacticId = B.EntityId
		JOIN CustomField CS ON B.CustomFieldId = CS.CustomFieldId and CS.EntityType=@entTactic
		JOIN Plan_Campaign_Program P ON A.PlanProgramId = P.PlanProgramId and P.IsDeleted='0'
		JOIN Plan_Campaign C ON P.PlanCampaignId = C.PlanCampaignId and C.IsDeleted='0' and C.PlanId IN (SELECT PlanId FROM @PlanIds)
		WHERE A.IsDeleted='0'
	
	    SELECT DISTINCT(A.Name) AS [Text],A.EntityType +'Custom'+ Cast(A.CustomFieldId as nvarchar(50)) as Value  
		FROM CustomField A JOIN CustomFieldType B 
		ON A.CustomFieldTypeId = B.CustomFieldTypeId JOIN @tblCustomerFieldIDs C 
		ON C.CustomFieldID = A.CustomFieldId
		WHERE A.ClientId=@ClientId AND A.IsDeleted=0 AND A.IsDisplayForFilter=1 AND A.EntityType IN ('Tactic','Campaign','Program') and B.Name='DropDownList' 
		ORDER BY Value DESC 

 END



GO

/* End- Added by Viral for Ticket #2595 on 10/14/2016 */

-- =============================================
-- Author: Viral Kadiya
-- Create date: 09/16/2016
-- Description:	This store proc. return Goal Header section data.
-- =============================================

/****** Object:  StoredProcedure [dbo].[spGetGoalValuesForPlan]    Script Date: 09/16/2016 12:26:44 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetGoalValuesForPlan]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetGoalValuesForPlan] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetGoalValuesForPlan]
	@PlanIds VARCHAR(max),
	@ClientId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	--EXEC spGetGoalValuesForPlan '20220','464eb808-ad1f-4481-9365-6aada15023bd'

	SET NOCOUNT ON;

    DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(10) = 'REVENUE',
			@INQValue FLOAT = 0,
			@MQLValue FLOAT = 0,
			@CWValue FLOAT = 0,
			@RevenueValue FLOAT = 0,
			@INQTotal FLOAT = 0,
			@MQLTotal FLOAT = 0,
			@CWTotal FLOAT = 0,
			@RevenueTotal FLOAT = 0,
			@PlanId INT,
			@ModelId INT,
			@PlanStageCode VARCHAR(20),
			@inqTitle varchar(255)='',
			@mqlTitle varchar(255)='',
			@cwTitle varchar(255)='',
			@revenue varchar(20)='Revenue'
	
	Declare @plans TABLE(
		PlanId INT,
		ModelId INT,
		PlanStageCode varchar(10)
	)


	INSERT INTO @plans 
	SELECT P.PlanId,P.ModelId,P.GoalType
	FROM [dbo].[comma_split](@PlanIds,',') as V
	INNER JOIN [Plan] as P on Cast(V.val as INT) = P.PlanId

	BEGIN
		DECLARE planGoal CURSOR FOR  
		SELECT PlanId,ModelId,PlanStageCode 
		FROM @plans 

		OPEN planGoal   
		FETCH NEXT FROM planGoal INTO @PlanId, @ModelId, @PlanStageCode  
		
		WHILE @@FETCH_STATUS = 0   
		BEGIN   

		SELECT @INQValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@INQCode,@ClientId)
		SELECT @MQLValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@MQLCode,@ClientId)
		SELECT @CWValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@CWCode,@ClientId)
		SELECT @RevenueValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@RevenueCode,@ClientId)
		
		SET @INQTotal = @INQTotal + @INQValue
		SET @MQLTotal = @MQLTotal + @MQLValue
		SET @CWTotal = @CWTotal + @CWValue
		SET @RevenueTotal = @RevenueTotal + @RevenueValue

		FETCH NEXT FROM planGoal INTO @PlanId, @ModelId, @PlanStageCode 
		END   
		
		CLOSE planGoal   
		DEALLOCATE planGoal
	END
		
	Select @inqTitle = Title from Stage where ClientId=@ClientId and Code=@INQCode and IsDeleted='0'
	Select @mqlTitle = Title from Stage where ClientId=@ClientId and Code=@MQLCode and IsDeleted='0'
	Select @cwTitle = Title from Stage where ClientId=@ClientId and Code=@CWCode and IsDeleted='0'


	SELECT @inqTitle as Title, @INQTotal as Value, @INQCode as StageCode
	UNION ALL
	SELECT @mqlTitle as Title, @MQLTotal as Value, @MQLCode as StageCode
	UNION ALL
	SELECT @cwTitle as Title, @CWTotal as Value, @CWCode as StageCode
	UNION ALL
	SELECT @revenue as Title, @RevenueTotal as Value, @revenue as StageCode
	
END
GO

--Index Add to improve performance for Getbudget store proc.
IF NOT EXISTS (  SELECT top 1 * 
FROM sys.indexes 
WHERE name='IX_Plan_Campaign_Program_Tactic_Budget_1' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic_Budget'))
BEGIN
CREATE NONCLUSTERED INDEX [IX_Plan_Campaign_Program_Tactic_Budget_1] ON [dbo].[Plan_Campaign_Program_Tactic_Budget]
(
	[PlanTacticId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'fnGetMqlByEntityTypeAndEntityId') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[fnGetMqlByEntityTypeAndEntityId]
GO

CREATE FUNCTION [dbo].[fnGetMqlByEntityTypeAndEntityId](
	 @EntityType NVARCHAR(100)=''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue FLOAT=0
	)
RETURNS @RevenueTbl TABLE(
	Value BIGINT
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	DECLARE @value FLOAT = 0 
	IF (@EntityType='Tactic' and @StageMinLevel <= @StageMaxLevel)
	BEGIN
		SELECT @AggregateValue *= (Ms.Value/100) FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE S.[Level] >= @StageMinLevel AND S.[Level] < @StageMaxLevel 
							AND S.ClientId=@ClientId
							AND S.StageId = MS.StageId) S
				WHERE Ms.ModelId=@ModelId
						AND StageType='CR'
		SET @value = ROUND(@ProjectedStageValue * @AggregateValue,0)
	END

	INSERT INTO @RevenueTbl VALUES(@value)
	RETURN
END
GO

--Function fnGetRevueneByEntityTypeAndEntityId
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetRevueneByEntityTypeAndEntityId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
	execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[fnGetRevueneByEntityTypeAndEntityId](
	@EntityType NVARCHAR(100)=''''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue decimal=0
	 ,@ADS decimal=0
	)
RETURNS @RevenueTbl TABLE(
	Value decimal(38,2)
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	--DECLARE @ADS float = 1
	DECLARE @value decimal = 0

	IF (@EntityType=''Tactic'')
	BEGIN

		SELECT @AggregateValue *= (Ms.Value/100)
		 FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE 
							(S.[Level] IS NULL AND S.ClientId=@ClientId)
							OR (S.[Level] >= @StageMinLevel AND S.[Level] <= @StageMaxLevel 
							AND S.ClientId=@ClientId)
							AND S.IsDeleted=0
							) S
			WHERE Ms.ModelId=@ModelId
				AND S.StageId=MS.StageId
						AND (StageType=''CR'' OR StageType=''Size'')

		SET @value = ((@ProjectedStageValue * @AggregateValue)* @ADS)
		INSERT INTO @RevenueTbl VALUES(@value)
	END
	
	RETURN 
END
'
END
ELSE 
BEGIN
	execute dbo.sp_executesql @statement = N'ALTER FUNCTION [dbo].[fnGetRevueneByEntityTypeAndEntityId](
	@EntityType NVARCHAR(100)=''''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue decimal=0
	 ,@ADS decimal=0
	)
RETURNS @RevenueTbl TABLE(
	Value decimal(38,2)
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	DECLARE @value decimal = 0

	IF (@EntityType=''Tactic'')
	BEGIN

		SELECT @AggregateValue *= (Ms.Value/100)
		 FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE 
							(S.[Level] IS NULL AND S.ClientId=@ClientId)
							OR (S.[Level] >= @StageMinLevel AND S.[Level] <= @StageMaxLevel 
							AND S.ClientId=@ClientId)
							AND S.IsDeleted=0
							) S
			WHERE Ms.ModelId=@ModelId
				AND S.StageId=MS.StageId
						AND (StageType=''CR'' OR StageType=''Size'')

		SET @value = ((@ProjectedStageValue * @AggregateValue)* @ADS)
		INSERT INTO @RevenueTbl VALUES(@value)
	END
	
	RETURN 
END
'
END
GO

-- View Plan_PlannedCost
IF EXISTS(select * FROM sys.views where name = 'Plan_PlannedCost')
BEGIN
	DROP VIEW [dbo].[Plan_PlannedCost]
END
GO

 CREATE VIEW [dbo].[Plan_PlannedCost] WITH SCHEMABINDING
	AS 
   SELECT [dbo].[Plan_Campaign].[PlanId], 
   SUM([dbo].[Plan_Campaign_Program_Tactic].[Cost]) AS PlannedCost
   FROM  [dbo].[Plan_Campaign], [dbo].[Plan_Campaign_Program],[dbo].[Plan_Campaign_Program_Tactic]
		WHERE [dbo].[Plan_Campaign].[IsDeleted] = 0 
		 AND [dbo].[Plan_Campaign_Program].[IsDeleted]  = 0
			AND  [dbo].[Plan_Campaign_Program_Tactic].[IsDeleted] = 0  				
			  AND  [dbo].[Plan_Campaign].[PlanCampaignId] = [dbo].[Plan_Campaign_Program].[PlanCampaignId] 
			    AND  [dbo].[Plan_Campaign_Program].[PlanProgramId] = [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId] 
			      --AND [Plan_Campaign].PlanId=19569
			 GROUP BY  [dbo].[Plan_Campaign].[PlanId]  
GO

-- View Campaign_PlannedCost
IF EXISTS(select * FROM sys.views where name = 'Campaign_PlannedCost')
BEGIN
	DROP VIEW [dbo].[Campaign_PlannedCost]
END
GO

CREATE VIEW [dbo].[Campaign_PlannedCost] WITH SCHEMABINDING
	AS 
SELECT  [dbo].[Plan_Campaign_Program].[PlanCampaignId] as [PlanCampaignId], 
	SUM([dbo].[Plan_Campaign_Program_Tactic].[Cost]) AS PlannedCost
	FROM  [dbo].[Plan_Campaign_Program_Tactic],  
	[dbo].[Plan_Campaign_Program]   
	WHERE  
	[dbo].[Plan_Campaign_Program].[IsDeleted] = 0  AND  
	[dbo].[Plan_Campaign_Program_Tactic].[IsDeleted] = 0  AND  
	[dbo].[Plan_Campaign_Program].[PlanProgramId]  = [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId]
	GROUP BY  [dbo].[Plan_Campaign_Program].[PlanCampaignId]  

GO

-- View Program_PlannedCost
IF EXISTS(select * FROM sys.views where name = 'Program_PlannedCost')
BEGIN
	DROP VIEW [dbo].[Program_PlannedCost]
END
GO

   CREATE VIEW [dbo].[Program_PlannedCost] WITH SCHEMABINDING
	AS 
   SELECT [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId], 
   SUM([dbo].[Plan_Campaign_Program_Tactic].[Cost]) AS PlannedCost
   FROM [dbo].[Plan_Campaign_Program],[dbo].[Plan_Campaign_Program_Tactic]
		WHERE [dbo].[Plan_Campaign_Program].[IsDeleted]  = 0
			AND  [dbo].[Plan_Campaign_Program_Tactic].[IsDeleted] = 0  				
			 AND   [dbo].[Plan_Campaign_Program].[PlanProgramId] = [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId] 
			 GROUP BY  [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId]  
GO

-- Indexes
IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_LineItemType_LineItemTypeId' AND object_id = OBJECT_ID('LineItemType'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_LineItemType_LineItemTypeId] ON [dbo].[LineItemType]
	(
		[IsDeleted] ASC,
		[LineItemTypeId] ASC
	)
	INCLUDE ([Title]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END 
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Campaign_Plan' AND object_id = OBJECT_ID('Plan_Campaign'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Campaign_Plan] ON [dbo].[Plan_Campaign]
	(
		[PlanId] ASC
	)
	INCLUDE ([PlanCampaignId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign' AND object_id = OBJECT_ID('Plan_Campaign_Program'))
BEGIN
	
	CREATE NONCLUSTERED INDEX [IX_Program_Campaign] ON [dbo].[Plan_Campaign_Program]
	(
		[PlanCampaignId] ASC
	)
	INCLUDE ([PlanProgramId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	
	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[PlanProgramId] ASC
	)
	INCLUDE ([PlanTacticId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic_TacticType' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	
	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic_TacticType] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[TacticTypeId] ASC
	)
	INCLUDE ([PlanTacticId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic_LineItem' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic_LineItem'))
BEGIN
	
	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic_LineItem] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]
	(
		[PlanTacticId] ASC
	)
	INCLUDE ([PlanLineItemId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic_LineItem_LineItemType' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic_LineItem'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic_LineItem_LineItemType] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]
	(
		[LineItemTypeId] ASC
	)
	INCLUDE ([PlanLineItemId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_CustomField_Entity_EntityId' AND object_id = OBJECT_ID('CustomField_Entity'))
BEGIN
	
	CREATE NONCLUSTERED INDEX [IX_CustomField_Entity_EntityId] ON [dbo].[CustomField_Entity]
	(
		[EntityId] ASC,
		[CustomFieldId] ASC
	)
	INCLUDE ( 	[CustomFieldEntityId],
		[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Plan_Campaign_32_2101582525__K2_K17_K1' AND object_id = OBJECT_ID('Plan_Campaign'))
BEGIN
	
	CREATE NONCLUSTERED INDEX [_dta_index_Plan_Campaign_32_2101582525__K2_K17_K1] ON [dbo].[Plan_Campaign]
	(
		[PlanId] ASC,
		[IsDeleted] ASC,
		[PlanCampaignId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO


IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_PlanCampaignProgram_PlanCampaignId_PlanProgramId' AND object_id = OBJECT_ID('Plan_Campaign_Program'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_PlanCampaignProgram_PlanCampaignId_PlanProgramId] ON [dbo].[Plan_Campaign_Program]
	(
		[IsDeleted] ASC,
		[PlanCampaignId] ASC,
		[PlanProgramId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Plan_Campaign_Program_Tactic_32_56387270__K15_K2_K1_8' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	CREATE NONCLUSTERED INDEX [_dta_index_Plan_Campaign_Program_Tactic_32_56387270__K15_K2_K1_8] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[IsDeleted] ASC,
		[PlanProgramId] ASC,
		[PlanTacticId] ASC
	)
	INCLUDE ( 	[Cost]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Model_Stage_32_1330103779__K13_K4_K3_K1_5' AND object_id = OBJECT_ID('Model_Stage'))
BEGIN
	CREATE NONCLUSTERED INDEX [_dta_index_Model_Stage_32_1330103779__K13_K4_K3_K1_5] ON [dbo].[Model_Stage]
	(
		[ModelId] ASC,
		[StageType] ASC,
		[StageId] ASC,
		[ModelStageId] ASC
	)
	INCLUDE ( 	[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_PlanCampaign_PlanId_PlanCampaignId' AND object_id = OBJECT_ID('Plan_Campaign'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_PlanCampaign_PlanId_PlanCampaignId] ON [dbo].[Plan_Campaign]
	(
		[PlanId] ASC,
		[IsDeleted] ASC,
		[PlanCampaignId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_PlanCampaignProgramTactic_PlanProgramId_IsDeleted' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_PlanCampaignProgramTactic_PlanProgramId_IsDeleted] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[PlanProgramId] ASC,
		[IsDeleted] ASC
	)
	INCLUDE ( 	[Cost]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_CustomField_ClientId_EntityType_CustomFieldId' AND object_id = OBJECT_ID('CustomField'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_CustomField_ClientId_EntityType_CustomFieldId] ON [dbo].[CustomField]
	(
		[ClientId] ASC,
		[EntityType] ASC,
		[IsDeleted] ASC,
		[CustomFieldId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END 
GO

IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_CustomField_ClientId_EntityType_IncludeColumns' AND object_id = OBJECT_ID('CustomField'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_CustomField_ClientId_EntityType_IncludeColumns] ON [dbo].[CustomField]
	(
		[ClientId] ASC,
		[EntityType] ASC,
		[IsDeleted] ASC
	)
	INCLUDE ( 	[CustomFieldId],
		[Name],
		[CustomFieldTypeId],
		[IsRequired],
		[AbbreviationForMulti]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO


IF NOT EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_CustomfieldEntity_EntityId_CustomFielId' AND object_id = OBJECT_ID('CustomField_Entity'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_CustomfieldEntity_EntityId_CustomFielId] ON [dbo].[CustomField_Entity]
	(
		[EntityId] ASC,
		[CustomFieldId] ASC
	)
	INCLUDE ([Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END 
GO

-- STATISTICS

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1838629593_1_17_6')
BEGIN
	CREATE STATISTICS [_dta_stat_1838629593_1_17_6] ON [dbo].[CustomField]([CustomFieldId], [ClientId], [EntityType])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1838629593_1_6')
BEGIN
	CREATE STATISTICS [_dta_stat_1838629593_1_6] ON [dbo].[CustomField]([CustomFieldId], [EntityType])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1838629593_8_17')
BEGIN
	CREATE STATISTICS [_dta_stat_1838629593_8_17] ON [dbo].[CustomField]([IsDeleted], [ClientId])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1838629593_8_6')
BEGIN
	CREATE STATISTICS [_dta_stat_1838629593_8_6] ON [dbo].[CustomField]([IsDeleted], [EntityType])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1166627199_1_3')
BEGIN
	CREATE STATISTICS [_dta_stat_1166627199_1_3] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]([PlanLineItemId], [LineItemTypeId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_581577110_7_15_12')
BEGIN
	CREATE STATISTICS [_dta_stat_581577110_7_15_12] ON [dbo].[Stage]([IsDeleted], [ClientId], [Code])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_581577110_7_1')
BEGIN
	CREATE STATISTICS [_dta_stat_581577110_7_1] ON [dbo].[Stage]([IsDeleted], [StageId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_645577338_17_1')
BEGIN
	CREATE STATISTICS [_dta_stat_645577338_17_1] ON [dbo].[TacticType]([IsDeleted], [TacticTypeId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_117575457_1_9')
BEGIN
	CREATE STATISTICS [_dta_stat_117575457_1_9] ON [dbo].[Model]([ModelId], [IsDeleted])
END 

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_2101582525_17_1')
BEGIN
	CREATE STATISTICS [_dta_stat_2101582525_17_1] ON [dbo].[Plan_Campaign]([IsDeleted], [PlanCampaignId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_2101582525_1_2_17')
BEGIN	
	CREATE STATISTICS [_dta_stat_2101582525_1_2_17] ON [dbo].[Plan_Campaign]([PlanCampaignId], [PlanId], [IsDeleted])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_2133582639_1_17')
BEGIN	
	CREATE STATISTICS [_dta_stat_2133582639_1_17] ON [dbo].[Plan_Campaign_Program]([PlanProgramId], [IsDeleted])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_437576597_1_2')
BEGIN	
	CREATE STATISTICS [_dta_stat_437576597_1_2] ON [dbo].[Plan_Campaign_Program]([PlanProgramId], [PlanCampaignId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_56387270_15_1')
BEGIN	
	CREATE STATISTICS [_dta_stat_56387270_15_1] ON [dbo].[Plan_Campaign_Program_Tactic]([IsDeleted], [PlanTacticId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_469576711_3_1_46')
BEGIN	
	CREATE STATISTICS [_dta_stat_469576711_3_1_46] ON [dbo].[Plan_Campaign_Program_Tactic]([TacticTypeId], [PlanTacticId], [CreatedBy])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_469576711_1_46')
BEGIN	
	CREATE STATISTICS [_dta_stat_469576711_1_46] ON [dbo].[Plan_Campaign_Program_Tactic]([PlanTacticId], [CreatedBy])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_469576711_3_46_18')
BEGIN	
	CREATE STATISTICS [_dta_stat_469576711_3_46_18] ON [dbo].[Plan_Campaign_Program_Tactic]([TacticTypeId], [CreatedBy], [Status])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_469576711_46_18_1_3')
BEGIN	
	CREATE STATISTICS [_dta_stat_469576711_46_18_1_3] ON [dbo].[Plan_Campaign_Program_Tactic]([CreatedBy], [Status], [PlanTacticId], [TacticTypeId])
END	
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_56387270_1_2_15')
BEGIN	
	CREATE STATISTICS [_dta_stat_56387270_1_2_15] ON [dbo].[Plan_Campaign_Program_Tactic]([PlanTacticId], [PlanProgramId], [IsDeleted])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1330103779_4_1_3_13')
BEGIN
	CREATE STATISTICS [_dta_stat_1330103779_4_1_3_13] ON [dbo].[Model_Stage]([StageType], [ModelStageId], [StageId], [ModelId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1330103779_13_3')
BEGIN
	CREATE STATISTICS [_dta_stat_1330103779_13_3] ON [dbo].[Model_Stage]([ModelId], [StageId])
END
GO
-- End By Nishant Sheth


/* Start - Added by Dhvani Raval for Ticket #2534*/
-----------------------* Add Columns in Tables *-----------------------
--Add Column in Alert Rule Table
IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'UserEmail' AND Object_ID = Object_ID(N'Alert_Rules'))
BEGIN
  ALTER TABLE Alert_Rules  Add UserEmail NVARCHAR(255)
END
GO

--Add Columns in Alert Table 
IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsEmailSent' AND Object_ID = Object_ID(N'Alerts'))
BEGIN
  ALTER TABLE Alerts  Add IsEmailSent NVARCHAR(50)
END
GO

IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'CurrentGoal' AND Object_ID = Object_ID(N'Alerts'))
BEGIN
  ALTER TABLE Alerts  Add CurrentGoal FLOAT
END
GO

IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'ActualGoal' AND Object_ID = Object_ID(N'Alerts'))
BEGIN
  ALTER TABLE Alerts  Add ActualGoal FLOAT
END
GO

-----------------------*SP RunAlertRules *-----------------------

IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'RunAlertRules') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE dbo.RunAlertRules
END
GO

/****** Object:  StoredProcedure [dbo].[RunAlertRules]    Script Date: 9/17/2016 5:23:06 PM ******/
CREATE PROCEDURE [dbo].[RunAlertRules]
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Constant variables
		DECLARE @txtLessThan	NVARCHAR(20) = 'less than',
				@txtGreaterThan NVARCHAR(20) = 'greater than',
				@txtEqualTo		NVARCHAR(20) = 'equal to'

		DECLARE @TacticsDataForRules			NVARCHAR(MAX) = '',
				@UPDATEQUERYCOMMON				NVARCHAR(MAX) = '',
				@UpdatePlanQuery				NVARCHAR(MAX) = '',
				@UpdateProjectedValuesForPlan	NVARCHAR(MAX) = '',
				@UpdateCampaignQuery			NVARCHAR(MAX) = '',
				@UpdateProgramQuery				NVARCHAR(MAX) = '',
				@UpdateTacticQuery				NVARCHAR(MAX) = '',
				@UpdateLineItemQuery			NVARCHAR(MAX) = '',
				@CalculatePercentGoalQuery		NVARCHAR(MAX) = '',
				@INSERTALERTQUERYCOMMON			NVARCHAR(MAX) = '',
				@InsertQueryForLT				NVARCHAR(MAX) = '',
				@InsertQueryForGT				NVARCHAR(MAX) = '',
				@InsertQueryForEQ				NVARCHAR(MAX) = '',
				@CommonQueryToIgnoreDuplicate	NVARCHAR(MAX) = '',
				@EmailBody	NVARCHAR(MAX) = '',
				@EmailSubject	NVARCHAR(MAX) = ''
				

		-- Get projected and actual values of tactic belongs to plan/campaign/program
		SET @TacticsDataForRules = 'DECLARE @TempEntityTable [TacticForRuleEntities];

									-- Get entities from the rule which have reached the completion goal
									INSERT INTO @TempEntityTable([RuleId],[EntityId],[EntityType],[Indicator],[IndicatorComparision],[IndicatorGoal],[CompletionGoal],
																 [Frequency],[DayOfWeek],[DateOfMonth],[UserId],[ClientId],
																 [EntityTitle],[StartDate],[EndDate],[PercentComplete]) 
									SELECT Entity.* FROM dbo.GetEntitiesReachedCompletionGoal() Entity 
									WHERE Entity.PercentComplete >= Entity.CompletionGoal

									-- Table with projected and actual values of tactic belongs to plan/campaign/program
									SELECT * INTO #TacticsDataForAllRuleEntities FROM dbo.[GetTacticsForAllRuleEntities](@TempEntityTable)
									'
		
		-- Common query to update projected and actual values of indicators for entities
		SET @UPDATEQUERYCOMMON =  ';	UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0), A.ActualStageValue = ISNULL(B.ActualStageValue,0)
										FROM @TempEntityTable A INNER JOIN  
										#TacticsDataForAllRuleEntities B
										ON A.EntityId = B.EntityId AND A.EntityType = B.EntityType AND A.Indicator = B.Indicator
										'
		-- Update IndicatorTitle based on Indicator Code
		DECLARE @UpdateIndicatorTitle NVARCHAR(MAX) = ' 
														UPDATE A SET A.IndicatorTitle = dbo.GetIndicatorTitle(A.Indicator,B.ClientId,A.EntityType)
														FROM @TempEntityTable A 
														INNER JOIN vClientWise_EntityList B ON A.EntityId = B.EntityId AND A.EntityType = B.Entity
														'

		-- For plan update projected value using different calculation rest of PLANNEDCOST
		SET @UpdateProjectedValuesForPlan = ';  UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0)
												FROM @TempEntityTable A INNER JOIN
												[dbo].[ProjectedValuesForPlans](@TempEntityTable) B ON A.EntityId = B.PlanId  
												AND A.Indicator = B.Indicator AND A.EntityType = ''Plan''
												AND A.Indicator != ''PLANNEDCOST''
												'
		-- Convert percent of goal from Projected and Actual values
		SET @CalculatePercentGoalQuery = ' UPDATE @TempEntityTable SET CalculatedPercentGoal = 
											CASE WHEN ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) = 0 THEN 0 
												 WHEN (ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) != 0) THEN 100 
												 ELSE ISNULL(ActualStageValue,0) * 100 / ProjectedStageValue END ;
										   SELECT * FROM @TempEntityTable 
										   '
		-- Common query to create alerts
		SET @INSERTALERTQUERYCOMMON = '	SELECT RuleId, ##DESCRIPTION## AS [Description], UserId,
										(CASE WHEN Frequency = ''WEEKLY'' THEN
											DATEADD(DAY,
											CASE WHEN DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek+1) < 0 THEN
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) + 7
											ELSE 
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) END
											,GETDATE()) 
										WHEN Frequency = ''MONTHLY'' THEN
											CASE WHEN DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth) < 0 THEN
												DATEADD(MONTH,1,DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE()))
											ELSE 
												DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE())  END
										ELSE GETDATE() END ) AS DisplayDate,  ProjectedStageValue , ActualStageValue
										FROM @TempEntityTable '

		-- For less than rule
		DECLARE @LessThanWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal < IndicatorGoal AND IndicatorComparision = ''LT'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '
		-- For greater than rule
		DECLARE @GreaterThanWhere	NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal > IndicatorGoal AND IndicatorComparision = ''GT'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '
		-- For equal to rule
		DECLARE @EqualToWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal = IndicatorGoal AND IndicatorComparision = ''EQ'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '

		SET @InsertQueryForLT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtLessThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @LessThanWhere
		SET @InsertQueryForGT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtGreaterThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @GreaterThanWhere
		SET @InsertQueryForEQ = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtEqualTo +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @EqualToWhere

    

		SET @CommonQueryToIgnoreDuplicate = '	MERGE INTO [dbo].Alerts AS T1
												USING
												(##INSERTQUERY##) AS T2
												ON (T2.RuleId = T1.RuleId AND T2.Description = T1.Description AND T2.UserId = T1.UserId)
												WHEN NOT MATCHED THEN  
												INSERT ([RuleId],[Description],[UserId],[DisplayDate],[CurrentGoal],[ActualGoal])
												VALUES ([RuleId],[Description],[UserId],[DisplayDate],[ActualStageValue],[ProjectedStageValue]);'
 
		SET @InsertQueryForLT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForLT)
		SET @InsertQueryForGT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForGT)
		SET @InsertQueryForEQ = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForEQ)
		
		EXEC (@TacticsDataForRules + @UPDATEQUERYCOMMON + @UpdateIndicatorTitle + @UpdateProjectedValuesForPlan + 
				@CalculatePercentGoalQuery + @InsertQueryForLT + @InsertQueryForGT +@InsertQueryForEQ)
		
	END TRY
	BEGIN CATCH
		--Get the details of the error
		 DECLARE   @ErMessage NVARCHAR(2048),
				   @ErSeverity INT,
				   @ErState INT
 
		 SELECT @ErMessage = ERROR_MESSAGE(), @ErSeverity = ERROR_SEVERITY(), @ErState = ERROR_STATE()
 
		 RAISERROR (@ErMessage, @ErSeverity, @ErState)
	END CATCH 
END
GO




-----------------------*SP SP_Save_AlertRule *-----------------------

IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'SP_Save_AlertRule') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE [dbo].[SP_Save_AlertRule]
END
GO

CREATE PROCEDURE [dbo].[SP_Save_AlertRule]

	@ClientId INT,
	@RuleId int,
	@RuleSummary nvarchar(max),
	@EntityId int,
	@EntityType nvarchar(100),
	@Indicator nvarchar(50),
	@IndicatorComparision nvarchar(10),
	@IndicatorGoal int,
	@CompletionGoal int,
	@Frequency nvarchar(50),
	@DayOfWeek tinyint=null,
	@DateOfMonth tinyint=null,
	@UserId INT,
	@CreatedBy INT,
	@ModifiedBy  INT,
	@UserEmail NVARCHAR(255),
	@IsExists int Output

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @UniqueRule nvarchar(max)
	Declare @FrequencyValue nvarchar(100)=null
	if(@DayOfWeek is not null and @DateOfMonth is null)
		set @FrequencyValue=@DayOfWeek
	else if(@DayOfWeek is null and @DateOfMonth is not null)
		set @FrequencyValue=@DateOfMonth

	set @UniqueRule=CONVERT(nvarchar(15),@EntityId)+'_'+CONVERT(nvarchar(15),@Indicator)+'_'+CONVERT(nvarchar(15),@IndicatorComparision)+'_'+CONVERT(nvarchar(15),@IndicatorGoal)+'_'+CONVERT(nvarchar(15),@CompletionGoal)+'_'+CONVERT(nvarchar(15),@Frequency)
	if(@FrequencyValue is not null)
		set @UniqueRule=@UniqueRule+'_'+@FrequencyValue
	
	If(@RuleId!=0)
	Begin --Update existing rule
		If not exists (Select RuleId from Alert_Rules where ClientId=@ClientId and  RuleId!=@RuleId and UniqueRuleCode=@UniqueRule and UserId = @UserId)
		Begin
			Update Alert_Rules set EntityId=@EntityId,EntityType=@EntityType,Indicator=@Indicator,IndicatorComparision=@IndicatorComparision,IndicatorGoal=@IndicatorGoal,
			CompletionGoal=@CompletionGoal,Frequency=@Frequency,DateOfMonth=@DateOfMonth,DayOfWeek=@DayOfWeek,ModifiedBy=@ModifiedBy,ModifiedDate=GETDATE(),
			RuleSummary=@RuleSummary,LastProcessingDate=GETDATE(),UniqueRuleCode=@UniqueRule
			where RuleId=@RuleId
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	Else
	Begin -- Isert new alert rule
		If not exists (Select RuleId from Alert_Rules where ClientId=@ClientId and UniqueRuleCode=@UniqueRule and UserId = @UserId)
		Begin
			Insert into Alert_Rules (RuleSummary,EntityId,EntityType,Indicator,IndicatorComparision,IndicatorGoal,CompletionGoal,Frequency,DayOfWeek,DateOfMonth,LastProcessingDate,
				UserId,ClientId,IsDisabled,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,UniqueRuleCode,UserEmail)
			values(@RuleSummary,@EntityId,@EntityType,@Indicator,@IndicatorComparision,@IndicatorGoal,@CompletionGoal,@Frequency,@DayOfWeek,@DateOfMonth,GETDATE(),
				@UserId,@ClientId,0,GETDATE(),@CreatedBy,GETDATE(),@CreatedBy,@UniqueRule,@UserEmail)
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	
END
GO

-----------------------*View vClientWise_EntityList *-----------------------

IF EXISTS (SELECT 1 FROM sys.views WHERE OBJECT_ID=OBJECT_ID('vClientWise_EntityList'))
BEGIN
	DROP view vClientWise_EntityList
END
GO

CREATE VIEW [dbo].[vClientWise_EntityList] AS
WITH AllPlans AS(
SELECT P.PlanId EntityId, P.Title EntityTitle, M.ClientId, 'Plan' Entity,P.CreatedDate,P.Title as PlanTitle, P.PlanId As PlanId  ,1 EntityOrder 
FROM [Plan] P 
INNER JOIN Model M ON M.ModelId = P.ModelId AND P.IsDeleted = 0
WHERE  M.IsDeleted = 0
),
AllCampaigns AS
(
       SELECT P.PlanCampaignId EntityId, P.Title EntityTitle,C.ClientId, 'Campaign' Entity,P.CreatedDate,C.PlanTitle as PlanTitle, c.PlanId As PlanId, 2 EntityOrder 
       FROM Plan_Campaign P
              INNER JOIN AllPlans C ON P.PlanId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllProgram AS
(
       SELECT P.PlanProgramId EntityId, P.Title EntityTitle,C.ClientId, 'Program' Entity,P.CreatedDate, C.PlanTitle as PlanTitle, c.PlanId As PlanId, 3 EntityOrder 
       FROM Plan_Campaign_Program P
              INNER JOIN AllCampaigns C ON P.PlanCampaignId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllLinkedTactic as
(
SELECT P.LinkedTacticId 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.Status in ('In-Progress','Approved','Complete') and P.LinkedTacticId is not null
	   and (DATEPART(year,P.EndDate)-DATEPART(year,P.StartDate))>0
),
AllTactic AS
(
       SELECT P.PlanTacticId EntityId, P.Title EntityTitle,C.ClientId, 'Tactic' Entity,P.CreatedDate,C.PlanTitle as PlanTitle,  c.PlanId As PlanId,  4 EntityOrder 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
			  LEFT OUTER JOIN AllLinkedTactic L on P.PlanTacticId=L.LinkedTacticId
       WHERE P.IsDeleted = 0 and P.Status in ('In-Progress','Approved','Complete') and L.LinkedTacticId is null
),
AllLineitem AS
(
       SELECT P.PlanLineItemId EntityId, P.Title EntityTitle, C.ClientId, 'Line Item' Entity,P.CreatedDate,C.PlanTitle as PlanTitle,  c.PlanId As PlanId,  5 EntityOrder 
       FROM Plan_Campaign_Program_Tactic_LineItem P
              INNER JOIN AllTactic C ON P.PlanTacticId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.LineItemTypeId is not null
)
SELECT * FROM AllPlans
UNION ALL 
SELECT * FROM AllCampaigns
UNION ALL 
SELECT * FROM AllProgram
UNION ALL 
SELECT * FROM AllTactic
UNION ALL 
SELECT * FROM AllLineitem
GO

-----------------------* Email Configuration *-----------------------
sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'Ole Automation Procedures', 1;
GO
RECONFIGURE;
GO



-- Add By Nishant Sheth 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveuserBudgetPermission] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveuserBudgetPermission]
@BudgetDetailId int  = 0,
@PermissionCode int = 0,
@CreatedBy INT 
AS
BEGIN

IF OBJECT_ID('tempdb..#tempbudgetdata') IS NOT NULL
Drop Table #tempbudgetdata

IF OBJECT_ID('tempdb..#AllUniqueBudgetdata') IS NOT NULL
Drop Table #AllUniqueBudgetdata

;WITH CTE AS
(
 
    SELECT ParentId 
    FROM Budget_Detail
    WHERE id= @BudgetDetailId
    UNION ALL
    --This is called multiple times until the condition is met
    SELECT g.ParentId 
    FROM CTE c, Budget_Detail g
    WHERE g.id= c.parentid

)

Select * into #tempbudgetdata from (select ParentId ,ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as RN from CTE) as result
option (maxrecursion 0)

select * from #tempbudgetdata where ParentId is not null

IF OBJECT_ID('tempdb..#AllUniqueBudgetdata') IS NOT NULL
Drop Table #AllUniqueBudgetdata

--Get user data of all the parents
SELECT * INTO #AllUniqueBudgetdata FROM (
select Distinct BudgetDetailId, UserId,@BudgetDetailId as bid,GETDATE() as dt,@CreatedBy as Cby,Case WHEN UserId = @CreatedBy
THEN 
@PermissionCode
 ELSE
PermisssionCode END as percode,
Case WHEN UserId = @CreatedBy
THEN 
 1
 ELSE
 0 END as usrid
from Budget_Permission where BudgetDetailId in (select ParentId from #tempbudgetdata)) as data

-- Insert unique data of parents for new item,take users from upper level parent if user not present in immediate parent
insert into Budget_Permission
(
BudgetDetailId,
CreatedDate,
PermisssionCode,
IsOwner,
UserId,
CreatedBy
)
select Uniquedata.bid,GETDATE(),Uniquedata.percode,Uniquedata.usrid,Uniquedata.UserId,Uniquedata.Cby from #AllUniqueBudgetdata as Uniquedata
JOIN #tempbudgetdata as TempBudgetTable on Uniquedata.BudgetDetailId = TempBudgetTable.ParentId and UserId NOT IN 
(
select UserId 
FROM #AllUniqueBudgetdata as Alldata
JOIN #tempbudgetdata as parent on Alldata.BudgetDetailId = parent.ParentId and RN < TempBudgetTable.RN 
)
UNION
select  @BudgetDetailId,GETDATE(),@PermissionCode,1,@CreatedBy,@CreatedBy from Budget_Permission 
END
GO

/* End - Added by Dhvani Raval for Ticket #2534*/
--Added by Manoj - Stage object in TacticType somehow didn’t get populated from data by EF 
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_TacticType_Stage')AND parent_object_id = OBJECT_ID(N'dbo.TacticType'))
BEGIN
	ALTER TABLE TacticType ADD FOREIGN KEY (StageId) REFERENCES Stage(StageId) 
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_ImprovementTacticType_Metric_ImprovementTacticType')AND parent_object_id = OBJECT_ID(N'dbo.ImprovementTacticType_Metric'))
BEGIN
	ALTER TABLE ImprovementTacticType_Metric ADD FOREIGN KEY (ImprovementTacticTypeId) REFERENCES ImprovementTacticType(ImprovementTacticTypeId) 
END
GO

-- =============================================
-- Author: Viral
-- Create date: 09/19/2016
-- Description:	Get Plan Calendar Start & End Date
-- =============================================

/****** Object:  StoredProcedure [dbo].[spGetPlanCalendarData]    Script Date: 10/13/2016 4:58:12 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetPlanCalendarData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetPlanCalendarData]
GO
/****** Object:  StoredProcedure [dbo].[spGetPlanCalendarData]    Script Date: 10/13/2016 4:58:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetPlanCalendarData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetPlanCalendarData] AS' 
END
GO
ALTER PROCEDURE [dbo].[spGetPlanCalendarData]
	@planIds varchar(max),
	@ownerIds varchar(max),
	@tactictypeIds varchar(max),
	@statusIds varchar(max),
	@timeframe varchar(20)='',
	@planYear varchar(255)='',
	@viewBy	varchar(1000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Exec spGetPlanCalendarData '20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thismonth','','Tactic'
	--Exec spGetPlanCalendarData '20365','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','2016-2017','','Tactic'
	Declare @dblDash varchar(10)='--'

	Declare @tblResult Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear int		-- PlanYear
			)

	Declare @entTactic varchar(20)='Tactic'
	Declare @entProgram varchar(20)='Program'
	Declare @entCampaign varchar(20)='Campaign'
	Declare @entPlan varchar(20)='Plan'
	Declare @tacColorCode varchar(20)='317232'
	Declare @isGrid bit =0	-- To seprate out that current screen is Grid or not. Set 'false' for calendar. This variable passed to common View By function 'fnViewByEntityHierarchy' to identify the current screen is Calendar, Plan Grid or Budget.
	-- GetPlanGanttStartEndDate
	BEGIN
		Declare @calStartDate datetime
		Declare @calEndDate datetime

		SELECT TOP 1 @calStartDate=startdate,@calEndDate=enddate from [dbo].[fnGetPlanGanttStartEndDate](@timeframe)
	END
	
    -- Insert statements for procedure here

	
	Declare @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		INT,
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ROIPackageIds	Varchar(max),
			PYear			INT				-- Plan Year
		)
	
	Declare @varThisYear varchar(10)='thisyear'
	Declare @varThisQuarter varchar(15)='thisquarter'
	Declare @varThisMonth varchar(10)='thismonth'
	Declare @varCurntTimeframe varchar(20)

	IF( (@timeframe = @varThisMonth) OR (@timeframe = @varThisQuarter) OR (@timeframe = @varThisYear) )
	BEGIN
		SET @varCurntTimeframe = DATEPART(yyyy,GETDATE())
	END
	ELSE
	BEGIN
		SET @varCurntTimeframe = @timeframe
	END
	
	INSERT INTO @Entities 
	SELECT		UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,H.Status,StartDate,EndDate,H.CreatedBy,TaskId,ParentTaskId,H.PlanId,ROIPackageIds,P.[Year] 
	FROM		[dbo].fnViewByEntityHierarchy(@planIds,@ownerIds,@tactictypeIds,@statusIds,@viewBy,@varCurntTimeframe,@isGrid) as H
	LEFT JOIN	[Plan] as P on H.PlanId = P.PlanId
	
	-- Get Plan wise MinStartEffective date from Improvement Tactic
		BEGIN
			Declare @tblImprvPlan Table(
				PlanId int,
				MinEffectiveDate datetime
			)

			Insert Into @tblImprvPlan
			SELECT Distinct ent.PlanId, Min(T.EffectiveDate) as EffectiveDate
			FROM @Entities as ent
			Inner Join Plan_Improvement_Campaign as C on ent.PlanId = C.ImprovePlanId
			Inner Join Plan_Improvement_Campaign_Program as P on C.ImprovementPlanCampaignId = P.ImprovementPlanCampaignId
			Inner Join Plan_Improvement_Campaign_Program_Tactic as T on P.ImprovementPlanProgramId = T.ImprovementPlanProgramId
			GROUP BY ent.PlanId
		END
		
	-- Get All tactics
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @submitStatus varchar(20)='Submitted'
			Declare @declineStatus varchar(20)='Decline'
			Declare @tblTactics Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy int,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear int		-- PlanYear
			)
		END

		-- Insert Tactic Data to local table @tblTactics
		BEGIN
			INSERT INTO @tblTactics

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					tac.TacticCustomName as 'machineName',
					CASE 
						WHEN (tac.StartDate < @calStartDate) 
						THEN CONVERT(VARCHAR(10),@calStartDate,101) 
						ELSE CONVERT(VARCHAR(10),tac.StartDate,101) 
					END AS 'start_date',
					tac.EndDate as 'endDate',
					Null as 'duration',
					0 as 'progress',
					'0' as 'open',
					CASE
						WHEN (tac.[Status] = @submitStatus) THEN '1' ELSE '0'
					END as 'isSubmitted',
					CASE
						WHEN (tac.[Status] = @declineStatus) THEN '1' ELSE '0'
					END as 'isDeclined',
					'0' as 'projectedStageValue',
					'0' as 'mqls',
					tac.Cost as 'cost',
					0 as 'cws',
					ent.ParentTaskId as 'parent',
					NULL as 'color',
					ent.ColorCode as 'colorcode',
					tac.PlanTacticId as 'PlanTacticId',
					NULL as 'PlanProgramId',
					NULL as 'PlanCampaignId',
					tac.[Status] as 'Status',
					tac.TacticTypeId as 'TacticTypeId',
					TP.Title as 'TacticType',
					tac.CreatedBy as 'CreatedBy',
					CASE
						WHEN ( (DATEPART(YYYY,tac.EndDate) -  DATEPART(YYYY,tac.StartDate) ) > 0 ) 
						THEN '1' ELSE '0'
					END as 'LinkTacticPermission',
					tac.LinkedTacticId as 'LinkedTacticId',
					IsNull(P.Title,'') as 'LinkedPlanName',
					@entTactic as [type],
					TP.AssetType as 'ROITacticType',
					NULL as 'OwnerName',
					RP.AnchorTacticID as 'IsAnchorTacticId',
					ent.ROIPackageIds as  'CalendarHoneycombpackageIDs',
					Null as 'Permission',
					ent.PlanId,
					ent.PYear				-- PlanYear
			FROM @Entities as ent
			INNER JOIN Plan_Campaign_Program_Tactic as tac on ent.EntityId = tac.PlanTacticId and ent.EntityType='Tactic'
			LEFT JOIN TacticType as TP on tac.TacticTypeId = TP.TacticTypeId and TP.IsDeleted='0'
			LEFT JOIN ROI_PackageDetail as RP on tac.PlanTacticId = RP.PlanTacticId 
			LEFT JOIN  [Plan] as P on tac.LinkedPlanId = P.PlanId 
			WHERE EntityType=@entTactic
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
			order by [text]
		END

		-- Update duration field
		BEGIN
			Update @tblTactics SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END

		-- Update Progress
		BEGIN
			Update @tblTactics SET progress= CASE
												 WHEN (CAST(T1.[start_date] as datetime) >= I.MinEffectiveDate) 
												 THEN 1 ELSE 0
											 END 
			FROM @tblTactics as T1
			JOIN @tblImprvPlan as I on T1.PlanId = I.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblTactics SET color =	CASE
												 WHEN (progress = 1) 
												 THEN 'stripe' ELSE ''
											END
											--WHERE 1=1 
		END
		
	END
	
	-- Get All Programs
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @tblPrograms Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear  int		-- PlanYear
			)
		END

		-- Insert Tactic Data to local table @tblPrograms
		BEGIN
			INSERT INTO @tblPrograms(id,[text],machineName,[start_date],endDate,progress,[open],parent,colorcode,PlanProgramId,[Status],TacticType,CreatedBy,[type],PlanId,PYear)

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					'' as 'machineName',
					CASE 
						WHEN (ent.StartDate < @calStartDate) 
						THEN CONVERT(VARCHAR(10),@calStartDate,101) 
						ELSE CONVERT(VARCHAR(10),ent.StartDate,101) 
					END AS 'start_date',
					ent.EndDate as 'endDate',
					--Null as 'duration',
					0 as 'progress',
					'0' as [open],
					ent.ParentTaskId as 'parent',
					--NULL as 'color',
					ent.ColorCode as 'colorcode',
					ent.EntityId as 'PlanProgramId',
					ent.[Status] as 'Status',
					@dblDash as 'TacticType',
					ent.CreatedBy as 'CreatedBy',
					@entProgram as [type],
					ent.PlanId,
					ent.PYear				-- PlanYear
			FROM @Entities as ent
			WHERE EntityType=@entProgram 
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
			order by [text]
		END

		-- Update duration field
		BEGIN
			Update @tblPrograms SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END
		
		-- Get Plan wise MinStartEffective date from Improvement Tactic
		BEGIN
			Declare @tblEntImprvPlan Table(
				EntityId Int,
				MinTacticDate datetime,
				PlanId Int
			)

			Insert Into @tblEntImprvPlan
			SELECT prg.PlanProgramId as EntityId,Min(tac.StartDate), prg.PlanId
			FROM @tblPrograms as prg
			Inner Join @Entities as tac on prg.PlanProgramId = tac.ParentEntityId and tac.EntityType='Tactic'
			GROUP BY prg.PlanId,prg.PlanProgramId
		END

		-- Calculate Progress for each Program
		BEGIN
			Update @tblPrograms SET progress = 
												CASE 
													WHEN (IP.MinTacticDate >= I.MinEffectiveDate)
													THEN 
														CASE
															WHEN (DATEDIFF(DAY,@calStartDate,IP.MinTacticDate)) > 0
															THEN (	
																	DATEDIFF(DAY,@calStartDate,IP.MinTacticDate) / P.duration
																 )
															ELSE 1
														END
													ELSE 0
												END 
			FROM @tblPrograms as P
			JOIN @tblEntImprvPlan as IP on P.PlanProgramId = IP.EntityId
			JOIN @tblImprvPlan as I on IP.PlanId = I.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblPrograms SET color =	CASE
												 WHEN (progress = 1) 
												 THEN ' stripe stripe-no-border ' 
												 ELSE 
													 CASE
														WHEN (progress > 0)
														THEN 'partialStripe' ELSE ''
													 END
											END
											--WHERE 1=1 
		END
		
		--select * from @tblPrograms
		--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
	END

	-- Get All Campaigns
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @tblCampaigns Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear  int		-- PlanYear
			)
		END

		-- Insert Campaign Data to local table @tblCampaigns
		BEGIN
			INSERT INTO @tblCampaigns(id,[text],[start_date],endDate,progress,[open],parent,colorcode,PlanCampaignId,[Status],TacticType,CreatedBy,[type],PlanId,PYear)

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					CASE 
						WHEN (ent.StartDate < @calStartDate) 
						THEN CONVERT(VARCHAR(10),@calStartDate,101) 
						ELSE CONVERT(VARCHAR(10),ent.StartDate,101) 
					END AS 'start_date',
					ent.EndDate as 'endDate',
					--Null as 'duration',
					0 as 'progress',
					'1' as [open],
					ent.ParentTaskId as 'parent',
					ent.ColorCode as 'colorcode',
					ent.EntityId as 'PlanCampaignId',
					ent.[Status] as 'Status',
					@dblDash as 'TacticType',
					ent.CreatedBy as 'CreatedBy',
					@entCampaign as [type],
					ent.PlanId,
					ent.PYear						-- Plan Year
			FROM @Entities as ent
			WHERE EntityType=@entCampaign 
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
			order by [text]
		END

		-- Update duration field
		BEGIN
			Update @tblCampaigns SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END
		
		-- Get Plan wise MinStartEffective date from Improvement Tactic
		BEGIN
			DELETE FROM @tblEntImprvPlan

			Insert Into @tblEntImprvPlan
			SELECT camp.PlanCampaignId as EntityId,Min(tac.StartDate), camp.PlanId
			FROM @tblCampaigns as camp
			Inner Join @Entities as prg on camp.PlanCampaignId = prg.ParentEntityId and prg.EntityType='Program'
			Inner Join @Entities as tac on prg.EntityId = tac.ParentEntityId and tac.EntityType='Tactic'
			GROUP BY camp.PlanId,camp.PlanCampaignId
		END

		-- Calculate Progress for each Campaign
		BEGIN
			Update @tblCampaigns SET progress = 
												CASE 
													WHEN (IC.MinTacticDate >= I.MinEffectiveDate)
													THEN 
														CASE
															WHEN (DATEDIFF(DAY,@calStartDate,IC.MinTacticDate)) > 0
															THEN (	
																	DATEDIFF(DAY,@calStartDate,IC.MinTacticDate) / C.duration
																 )
															ELSE 1
														END
													ELSE 0
												END 
			FROM @tblCampaigns as C
			JOIN @tblEntImprvPlan as IC on C.PlanCampaignId = IC.EntityId
			JOIN @tblImprvPlan as I on IC.PlanId = I.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblCampaigns SET color = CASE
											  WHEN (progress = 1) 
											  THEN ' stripe' 
											  ELSE 
											 	 CASE
											 		WHEN (progress > 0)
											 		THEN 'stripe' ELSE ''
											 	 END
											 END
											 --WHERE 1=1 
		END
		
		--select * from @tblCampaigns
		--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
	END

	-- Get All Plans
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @tblPlans Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear  int		-- PlanYear
			)
		END

		-- Insert Plan Data to local table @tblPlans
		BEGIN
			INSERT INTO @tblPlans(id,[text],progress,[open],parent,colorcode,[Status],TacticType,CreatedBy,[type],PlanId,PYear)

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					0 as 'progress',
					'1' as [open],
					ent.ParentTaskId as 'parent',
					ent.ColorCode as 'colorcode',
					ent.[Status] as 'Status',
					@dblDash as 'TacticType',
					ent.CreatedBy as 'CreatedBy',
					@entPlan as [type],
					ent.PlanId,
					ent.PYear
			FROM @Entities as ent
			WHERE ent.EntityType=@entPlan 
			order by [text]
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
		END

		BEGIN
			-- Update start_date column for Plan
			UPDATE @tblPlans SET [start_date]= ISNull( D.[start_date],DATEFROMPARTS (DATEPART(yyyy,@calStartDate), 1, 1))
			FROM @tblPlans as TP
			JOIN 
				(
					SELECT P.PlanId,
													CASE 
														WHEN (MIN(C.StartDate) < @calStartDate) 
														THEN CONVERT(VARCHAR(10),@calStartDate,101) 
														ELSE CONVERT(VARCHAR(10),MIN(C.StartDate),101) 
													END as 'start_date'
					FROM @tblPlans as P
					LEFT Join @Entities as C on P.PlanId = C.PlanId and C.EntityType='Campaign'
					GROUP BY P.PlanId
				) as D on TP.PlanId = D.PlanId
		END

		BEGIN
			-- Update enddate column for Plan
			UPDATE @tblPlans SET endDate= ISNull( D.endDate,DATEFROMPARTS (DATEPART(yyyy,@calEndDate), 12, 31))
			FROM @tblPlans as TP
			JOIN 
				(
					SELECT P.PlanId, MAX(C.EndDate) as 'endDate'
					FROM @tblPlans as P
					LEFT Join @Entities as C on P.PlanId = C.PlanId and C.EntityType='Campaign'
					GROUP BY P.PlanId
				) as D on TP.PlanId = D.PlanId
		END

		-- Update duration field
		BEGIN
			Update @tblPlans SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END

		-- Calculate Progress for each Plan
		BEGIN
			Update @tblPlans SET progress = 
											CASE 
												WHEN ( Cast(T.[start_date] as datetime) >= I.MinEffectiveDate)
												THEN 
													CASE
														WHEN ( DATEDIFF(DAY, Cast( P.[start_date] as datetime) ,Cast(T.[start_date] as datetime) ) ) > 0
														THEN (	
																DATEDIFF(DAY,Cast( P.[start_date] as datetime),Cast(T.[start_date] as datetime)) / P.duration
															 )
														ELSE 1
													END
												ELSE 0
											END 
			FROM @tblPlans as P
			JOIN @tblImprvPlan as I on P.PlanId = I.PlanId
			JOIN @tblTactics as T on P.PlanId = T.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblPlans SET color = CASE
											  WHEN (progress >0) 
											  THEN ' stripe' ELSE ''
										END
										--WHERE 1=1 
		END
		
		--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
	END

	--INSERT INTO @tblResult

	-- Check that ViewBy is not 'Tactic' type then set 1st most parent record fields values as Plan entity
	IF(@viewBy <> @entTactic)
	BEGIN
		INSERT INTO @tblResult(id,[text],[start_date],duration,progress,color,colorcode)
		SELECT	IsNull(E.TaskId,'') as id 
				,IsNull(E.EntityTitle,'') as [text],
				P.[start_date],
				duration = P.duration,
				progress = P.progress,
				color=p.color,
				colorcode=@tacColorCode		-- if ViewBy is not 'Tactic' then set parent node color code same as 'Tactic' entity color code.
		FROM	@Entities as E
		JOIN	@tblPlans as P on E.TaskId = P.parent
		WHERE	E.EntityType = @viewBy
	END

	-- Prepared Final Result Set.
	BEGIN

		INSERT INTO @tblResult
					(id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear)
		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear
		
		FROM		@tblPlans
		)
		 
		UNION ALL

		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear
		 
		FROM		@tblCampaigns
		)
		 
		UNION ALL
		
		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear

		FROM		@tblPrograms 
		)

		UNION ALL

		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear

		FROM		@tblTactics 
		)

	END

	SELECT	id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
			,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
			,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear

	FROM	@tblResult

	--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
END




GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublishModel]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[PublishModel]
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Modified by Komal Rawal
-- Created on :: 24-09-2016
-- Desc :: Update Plan model id and tactic's tactic type id with new publish version of model 
CREATE PROCEDURE [dbo].[PublishModel]
@NewModelId int = 0 
,@UserId INT= 0
AS
SET NOCOUNT ON;

BEGIN
IF OBJECT_ID(N'tempdb..#tblModelids') IS NOT NULL
BEGIN
  DROP TABLE #tblModelids
END

-- Get all parents model of new model
;WITH tblParent  AS
(
    SELECT ModelId,ParentModelId
        FROM [Model] WHERE ModelId = @NewModelId
    UNION ALL
    SELECT [Model].ModelId,[Model].ParentModelId FROM [Model]  JOIN tblParent  ON [Model].ModelId = tblParent.ParentModelId
)
SELECT ModelId into #tblModelids
    FROM tblParent 
	OPTION(MAXRECURSION 0)

-- Update Tactic Type for Default saved views
DECLARE  @TacticTypeIds NVARCHAR(MAX)=''
SELECT @TacticTypeIds = FilterValues From Plan_UserSavedViews WHERE Userid=@UserId AND FilterName='TacticType'

DECLARE   @FilterValues NVARCHAR(MAX)
IF (@TacticTypeIds != 'All')
Begin
SELECT @FilterValues = COALESCE(@FilterValues + ',', '') + CAST(TacticTypeId AS NVARCHAR) FROM TacticType 
WHERE PreviousTacticTypeId IN(SELECT val FROM dbo.comma_split(@TacticTypeIds,','))
AND ModelId=@NewModelId
End

IF @FilterValues <>'' 
BEGIN
	UPDATE Plan_UserSavedViews SET FilterValues=@FilterValues WHERE Userid=@UserId AND FilterName='TacticType'
END

-- Update Plan's ModelId with new modelid
UPDATE [Plan] SET ModelId=@NewModelId WHERE ModelId IN(SELECT ModelId FROM #tblModelids)

-- Update Tactic's Tactic Type with new model's tactic type
UPDATE Tactic SET Tactic.TacticTypeId=TacticType.TacticTypeId FROM 
Plan_Campaign_Program_Tactic Tactic 
CROSS APPLY(SELECT TacticType.TacticTypeId FROM TacticType WHERE TacticType.PreviousTacticTypeId=Tactic.TacticTypeId)TacticType
CROSS APPLY(SELECT Program.PlanProgramId,Program.PlanCampaignId FROM Plan_Campaign_Program Program WHERE Program.PlanProgramId=Tactic.PlanProgramId) Program
CROSS APPLY(SELECT Camp.PlanCampaignId,Camp.PlanId FROM Plan_Campaign Camp WHERE Camp.PlanCampaignId=Program.PlanCampaignId 
AND Camp.PlanId IN(SELECT PlanId FROM [Plan] WHERE ModelId IN(SELECT ModelId FROM #tblModelids)))Camp
WHERE Tactic.IsDeleted=0
AND Tactic.TacticTypeId IS NOT NULL

END

go



  
/* Start - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

-- DROP AND CREATE STORED PROCEDURE [dbo].[LineItem_Cost_Allocation]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_Cost_Allocation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[LineItem_Cost_Allocation]
END
GO

-- [dbo].[LineItem_Cost_Allocation] 135912,470
CREATE PROCEDURE [dbo].[LineItem_Cost_Allocation]
( 
	@PlanTacticId INT,
	@UserId INT
)
AS
BEGIN

	-- Calculate tactic and line item planned cost allocated by monthly/quarterly
	SELECT Id,ActivityId
	,ActivityName
	,ActivityType
	,ParentActivityId
	,CreatedBy
	,IsEditable
	,Cost
	,[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
	,LineItemTypeId
	FROM
	(
		-- Tactic cost allocation
		SELECT DISTINCT CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
			,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
			,PT.Title AS ActivityName
			,'tactic' ActivityType
			,NULL ParentActivityId
			,PT.CreatedBy 
			,0 as IsEditable
			,PT.Cost
			,'C'+PTCst.Period as Period
			,PTCst.Value as Value
			,0 LineItemTypeId
		FROM Plan_Campaign_Program_Tactic PT
		LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
		WHERE PT.IsDeleted = 0 AND PT.PlanTacticId = @PlanTacticId

		UNION ALL
		-- Line item cost allocation
		SELECT 
			CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
			,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
			,PL.Title as ActivityName
			,'lineitem' ActivityType
			,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
			,PL.CreatedBy
			,0 as IsEditable
			,PL.Cost
			,'C'+PLC.period as period 
			,PLC.Value
			,PL.LineItemTypeId as LineItemTypeId
		FROM Plan_Campaign_Program_Tactic_LineItem PL
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId = PLC.PlanLineItemId
		WHERE PL.PlanTacticId = @PlanTacticId AND PL.IsDeleted=0

	) TacticLineItems
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	) PivotLineItem
END
GO
--Insertation start #2623 import multiple plan
--Check Table type is exist or not
-- =============================================
-- Author:		<Kausha>
-- Create date: <27/09/2016>
-- Description:	<Following is stroprocedure and table type for multiple plan import in budget>
-- =============================================
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_GetPlanBudgetDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Sp_GetPlanBudgetDataMonthly
END
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_GetPlanBudgetDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Sp_GetPlanBudgetDataQuarterly
END
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_GetPlanActualDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Sp_GetPlanActualDataQuarterly
END
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_GetPlanActualDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Sp_GetPlanActualDataMonthly
END
GO

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Plan_BudgetQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Plan_BudgetQuarterCalculation
END
GO
CREATE PROCEDURE [dbo].[Plan_BudgetQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);
	/*Following is calculation to get quarter value and onths of quarter*/
			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataPlan_Budget') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataPlan_Budget
			END 
			SELECT * INTO #tempDataPlan_Budget FROM (SELECT * from Plan_Budget where PlanId=@EntityId) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataPlan_Budget where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataPlan_Budget where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataPlan_Budget where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataPlan_Budget where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN
	      /*Ex if Q1- if value is less then sum of (y1+y2+y3) then it will be deducted from y3->y2->y1 respectively */
			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Budget WHERE   PlanId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Budget  SET Value = 0     WHERE   PlanId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
		
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Budget WHERE   PlanId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Budget  SET Value = 0     WHERE   PlanId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Budget WHERE   PlanId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Budget  SET Value = 0     WHERE   PlanId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
			END
			END			

			END

			END
						
GO

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Plan_CampaignBudgetQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Plan_CampaignBudgetQuarterCalculation
END
GO
CREATE PROCEDURE [dbo].[Plan_CampaignBudgetQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataCampaign') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataCampaign
			END 
			SELECT * INTO #tempDataCampaign FROM (SELECT * from Plan_Campaign_Budget where PlanCampaignId=@EntityId) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataCampaign where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataCampaign where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataCampaign where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataCampaign where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempDataCampaign WHERE PlanCampaignId = @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempDataCampaign WHERE PlanCampaignId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanCampaignId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Campaign_Budget WHERE   PlanCampaignId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Campaign_Budget  SET Value = 0     WHERE   PlanCampaignId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataCampaign WHERE PlanCampaignId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataCampaign WHERE PlanCampaignId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanCampaignId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Budget WHERE   PlanCampaignId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Campaign_Budget  SET Value = 0     WHERE   PlanCampaignId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataCampaign WHERE PlanCampaignId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataCampaign WHERE PlanCampaignId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanCampaignId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Budget WHERE   PlanCampaignId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Campaign_Budget  SET Value = 0     WHERE   PlanCampaignId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
			END
			END			

			END

END
GO

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Plan_Campaign_Program_BudgetQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Plan_Campaign_Program_BudgetQuarterCalculation
END
GO
CREATE PROCEDURE [dbo].[Plan_Campaign_Program_BudgetQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataProgram') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataProgram
			END 
			SELECT * INTO #tempDataProgram FROM (SELECT * from Plan_Campaign_Program_Budget where PlanProgramId=@EntityId) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataProgram where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataProgram where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataProgram where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataProgram where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempDataProgram WHERE PlanProgramId = @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempDataProgram WHERE PlanProgramId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanProgramId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Campaign_Program_Budget WHERE   PlanProgramId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Campaign_Program_Budget  SET Value = 0     WHERE   PlanProgramId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataProgram WHERE PlanProgramId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataProgram WHERE PlanProgramId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanProgramId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Budget WHERE   PlanProgramId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Campaign_Program_Budget  SET Value = 0     WHERE   PlanProgramId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataProgram WHERE PlanProgramId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataProgram WHERE PlanProgramId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanProgramId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Budget WHERE   PlanProgramId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Campaign_Program_Budget  SET Value = 0     WHERE   PlanProgramId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
		END
		END			
	END

END

GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation
END
GO
CREATE PROCEDURE [dbo].[Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataTactic
			END 
			SELECT * INTO #tempDataTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Budget where PlanTacticId=@EntityId) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataTactic where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataTactic where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataTactic where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataTactic where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempDataTactic WHERE PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempDataTactic WHERE PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Campaign_Program_Tactic_Budget WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_Budget  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataTactic WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataTactic WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Tactic_Budget WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_Budget  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataTactic WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataTactic WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Tactic_Budget WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_Budget  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
			END
		END			
		END

END

Go

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Tactic_ActuallQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Tactic_ActuallQuarterCalculation
END
Go
CREATE PROCEDURE [dbo].[Tactic_ActuallQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempTacticActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempTacticActual
			END 
			SELECT * INTO #tempTacticActual FROM (SELECT * from Plan_Campaign_Program_Tactic_Actual where PlanTacticId=@EntityId and StageTitle='Cost') a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempTacticActual where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempTacticActual where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempTacticActual where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempTacticActual where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempTacticActual WHERE  PlanTacticId= @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Actualvalue from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Actual  SET Actualvalue = (Actualvalue-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter and StageTitle='Cost'
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=Actualvalue from Plan_Campaign_Program_Tactic_Actual WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter and StageTitle='Cost';
				UPDATE Plan_Campaign_Program_Tactic_Actual  SET Actualvalue = 0     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter and StageTitle='Cost'				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Actualvalue from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Actual  SET Actualvalue = (Actualvalue-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter and StageTitle='Cost'
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=Actualvalue from Plan_Campaign_Program_Tactic_Actual WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter and StageTitle='Cost';
				UPDATE Plan_Campaign_Program_Tactic_Actual  SET Actualvalue = 0     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter	and StageTitle='Cost'			
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Actualvalue from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Actual  SET Actualvalue = (Actualvalue-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter and StageTitle='Cost'
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=Actualvalue from Plan_Campaign_Program_Tactic_Actual WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter and StageTitle='Cost';
				UPDATE Plan_Campaign_Program_Tactic_Actual  SET Actualvalue = 0     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter		 and StageTitle='Cost'		
				END			
		END
		END			
	END

END


Go


IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_ActuallQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.LineItem_ActuallQuarterCalculation
END
Go

CREATE PROCEDURE [dbo].[LineItem_ActuallQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempLineItemActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempLineItemActual
			END 
			SELECT * INTO #tempLineItemActual FROM (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual where PlanLineItemId=@EntityId) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			--Select * from #tempLineItemActual
			IF EXISTS (SELECT * from #tempLineItemActual WHERE  PlanLineItemId= @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
	          
			 
			 
				IF((SELECT Value from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Actual  SET Value = (Value-(@DifferenceAmount-@UpdateValue))  WHERE  PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE   PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Actual  SET Value = 0     WHERE   PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
			 
			 
				IF((SELECT Value from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Actual  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE   PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Actual  SET Value = 0     WHERE   PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
			 
			  
				IF((SELECT Value from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Actual  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE   PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Actual  SET Value = 0     WHERE   PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
		END
		END			
	END

END


Go

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Tactic_CostQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Tactic_CostQuarterCalculation
END
Go
CREATE PROCEDURE [dbo].[Tactic_CostQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempTacticActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempTacticActual
			END 
			SELECT * INTO #tempTacticActual FROM (SELECT * from Plan_Campaign_Program_Tactic_Cost where PlanTacticId=@EntityId ) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempTacticActual WHERE  PlanTacticId= @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter 
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=Value from Plan_Campaign_Program_Tactic_Cost WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter ;
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter 				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter 
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=Value from Plan_Campaign_Program_Tactic_Cost WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter ;
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter		
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter 
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=Value from Plan_Campaign_Program_Tactic_Cost WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter ;
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter		 
				END			
		END
		END			
	END

END

Go


IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_CostQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.LineItem_CostQuarterCalculation
END
GO
CREATE PROCEDURE [dbo].[LineItem_CostQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempLineItemActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempLineItemActual
			END 
			SELECT * INTO #tempLineItemActual FROM (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Cost where PlanLineItemId=@EntityId) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempLineItemActual where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			--Select * from #tempLineItemActual
			IF EXISTS (SELECT * from #tempLineItemActual WHERE  PlanLineItemId= @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
	          
			 
			  
				IF((SELECT Value from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))  WHERE  PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Campaign_Program_Tactic_LineItem_Cost WHERE   PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Cost  SET Value = 0     WHERE   PlanLineItemId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
			 
			  
				IF((SELECT Value from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Tactic_LineItem_Cost WHERE   PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Cost  SET Value = 0     WHERE   PlanLineItemId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
			 
			 
				IF((SELECT Value from #tempLineItemActual WHERE PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Campaign_Program_Tactic_LineItem_Cost WHERE   PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Campaign_Program_Tactic_LineItem_Cost  SET Value = 0     WHERE   PlanLineItemId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
		END
		END			
	END

END




GO

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_ImportPlanBudgetDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[Sp_ImportPlanBudgetDataMonthly] 
END
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_ImportPlanActualDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[Sp_ImportPlanActualDataMonthly] 
END
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_ImportPlanCostDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[Sp_ImportPlanCostDataMonthly] 
END

IF  EXISTS (SELECT * FROM sys.types WHERE is_user_defined = 1 AND name = 'ImportExcelBudgetMonthData')
BEGIN
--if table type exist then check sp is exist or not then drop and create it after type creation
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanBudgetDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanBudgetDataMonthly] 
END

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanActualDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanActualDataMonthly] 
END

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanCostDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanCostDataMonthly] 
END


DROP TYPE ImportExcelBudgetMonthData;
END
go
CREATE TYPE [dbo].[ImportExcelBudgetMonthData] AS TABLE(
	[ActivityId] [int] NULL,
	[TYPE] [nvarchar](255) NULL,
	[Task Name] [nvarchar](3000) NULL,
	[Budget] [float] NULL,
	[JAN] [float] NULL,
	[FEB] [float] NULL,
	[MAR] [float] NULL,
	[APR] [float] NULL,
	[MAY] [float] NULL,
	[JUN] [float] NULL,
	[JUL] [float] NULL,
	[AUG] [float] NULL,
	[SEP] [float] NULL,
	[OCT] [float] NULL,
	[NOV] [float] NULL,
	[DEC] [float] NULL
)
GO
CREATE PROCEDURE [dbo].[ImportPlanBudgetDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Plan' as ActivityType, Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select 
Convert(varchar(max),[PlanId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
    SELECT p.planid, value , period,Budget, p.Title
  FROM  [Plan] p
  left JOIN plan_budget pb on p.PlanId=pb.PlanId  
  WHERE p.PlanId in( SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')


) d
pivot
(
   SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
) as rPlan group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
union 
select ActivityId,[Task Name],'Campaign' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanCampaignId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
  select pc.PlanCampaignId,value, period,CampaignBudget as Budget, pc.Title from Plan_Campaign pc
  left join Plan_Campaign_Budget pcb on pc.planCampaignid = pcb.PlanCampaignId where pc.PlanId in(SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted = 0
  
)
 e
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
) as rPlanCampaign group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

union
select ActivityId,[Task Name],'Program' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanProgramId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select t.PlanProgramId,t.Title,t.Budget,Value,Period from
 (
  select pc.PlanProgramId, pc.Title,ProgramBudget as Budget from Plan_Campaign_Program pc where IsDeleted=0 and PlanCampaignId in
   ( select PlanCampaignId from Plan_Campaign where PlanId in(SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0) 
  ) as t
  left join Plan_Campaign_Program_Budget pcb on t.PlanProgramId= pcb.PlanProgramBudgetId 
  
) r
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
) as rPlanCampaignProgram group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
union
select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Value,Period,b.Title,b.TacticBudget as Budget from Plan_Campaign_Program_Tactic_Budget as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in( SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12


) as ExistingData

--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType 
from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE])

 TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData


	
	--if (@EntityId = 17634)
	--begin
	--update [plan] set [version] = '555' where planid = 17634 
	--end
	
	IF ( @Type='Plan')
		BEGIN
	
		IF Exists (select top 1 PlanId from [Plan] where PlanId =  @EntityId)

			BEGIN
			
			UPDATE P SET P.Budget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Budget END
			from [Plan] P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId


			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END
        ELSE
		BEGIN
		INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END


		END
		
IF ( @Type='Campaign')
		BEGIN
		IF Exists (select top 1 PlanCampaignId from [Plan_Campaign] where PlanCampaignId =  @EntityId)
			BEGIN

			UPDATE P SET P.CampaignBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.CampaignBudget END
			from [Plan_Campaign] P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END
		ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END


		END

IF ( @Type='Program')
		BEGIN
			IF Exists (select top 1 PlanProgramId from [Plan_Campaign_Program] where PlanProgramId =  @EntityId)
			BEGIN

			UPDATE P SET P.ProgramBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.ProgramBudget END
			from [Plan_Campaign_Program] P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END

	
		END
		 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

IF ( @Type='Tactic')
		BEGIN
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId)
			BEGIN

			UPDATE P SET P.TacticBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.TacticBudget END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

		END

 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp

END
Go
CREATE PROCEDURE [dbo].[ImportPlanActualDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Actualvalue as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Actual as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId and LOWER(a.StageTitle)='cost'
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

--end tactic
UNION
--start line item
select ActivityId,[Task Name],'LineItem' as ActivityType,0 as Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_Tactic_LineItem_Actual as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData

--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType 
from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE])

 TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData



IF ( LOWER(@Type)='tactic')
	
	BEGIN
    
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId and [Status] IN('In-Progress','Complete','Approved') )
			BEGIN

			IF NOT EXISTS(Select * from Plan_Campaign_Program_Tactic_LineItem where PlanTacticId =  @EntityId  AND IsDeleted=0 and LineItemTypeId IS NOT NULL)
			BEGIN			
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y1' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.JAN 
			      from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' and StageTitle='Cost'
				END
				  ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
					
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y2' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.FEB 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2' and StageTitle='Cost'
				END
				ELSE
					BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		 
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y3' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.MAR 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3' and StageTitle='Cost'
				END
			ELSE
					BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y4' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.APR 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' and StageTitle='Cost'
				END
					ELSE
					BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y5' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.MAY 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5' and StageTitle='Cost'
				END

				ELSE
					BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y6' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.JUN 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6' and StageTitle='Cost'
				END
			ELSE
					BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y7' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.JUL 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' and StageTitle='Cost'
				END
		ELSE
					BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y8' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.AUG 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8' and StageTitle='Cost'
				END
				ELSE
					BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  --  ELSE
	
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y9' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.SEP 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9' and StageTitle='Cost'
				END
		ELSE
					BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END


			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y10' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.OCT 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' and StageTitle='Cost'
				END
	

	ELSE
					BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y11' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.NOV 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11' and StageTitle='Cost'
				END
	
					ELSE
					BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y12' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.DEC 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12' and StageTitle='Cost'
				END

					ELSE
					BEGIN
					IF ((SELECT [DEC] from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		END
		
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

	END
	

IF ( LOWER(@Type)='lineitem')
		BEGIN
			IF Exists (select top 1 PlanLineItemId from [Plan_Campaign_Program_Tactic_LineItem] where PlanLineItemId =  @EntityId)
			BEGIN		

			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN

			--Y1
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
				BEGIN
						UPDATE P SET P.Value = T.JAN 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
				END
				ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

	
             --Y2
			 	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y2')
				BEGIN
						UPDATE P SET P.Value = T.FEB 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y2'
				END
				ELSE
				BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
			---Y3
				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y3')
				BEGIN
						UPDATE P SET P.Value = T.MAR 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y3'
				END
				ELSE
				BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

----Y4

	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
				BEGIN
						UPDATE P SET P.Value = T.APR 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
				END
				ELSE
				BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
--Y5
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y5')
				BEGIN
						UPDATE P SET P.Value = T.MAY 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y5'
				END
				ELSE
				BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		

---Y6
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y6')
				BEGIN
						UPDATE P SET P.Value = T.JUN 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y6'
				END
				ELSE
				BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

---y7
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
				BEGIN
						UPDATE P SET P.Value = T.JUL 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
				END
				ELSE
				BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
--Y8
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y8')
				BEGIN
						UPDATE P SET P.Value = T.AUG 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y8'
				END
				ELSE
				BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y9
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y9')
				BEGIN
						UPDATE P SET P.Value = T.SEP 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y9'
				END
				ELSE
				BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
				--Y10
					IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
				BEGIN
						UPDATE P SET P.Value = T.OCT 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
				END
				ELSE
				BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
				--Y11
					IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y11')
				BEGIN
						UPDATE P SET P.Value = T.NOV 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y11'
				END
				ELSE
				BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y12
					IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y12')
				BEGIN
						UPDATE P SET P.Value = T.DEC 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y12'
				END
				ELSE
				BEGIN
					IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

			END

			END
		END
 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
--select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
--select * from @OutputTable
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp

END

Go

CREATE PROCEDURE [dbo].[ImportPlanCostDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, value as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Cost as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId 
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

--end tactic
UNION
--start line item
select ActivityId,[Task Name],'LineItem' as ActivityType,0 as Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_tactic_Lineitem_Cost as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData

select * into #temp2 from (select * from @ImportData )   k
--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType 
from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE])

 TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData



IF ( LOWER(@Type)='tactic')
	
	BEGIN
    
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId )
			BEGIN

			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId

			--update balance lineitem
			--update balance lineitem Cost of tactic -(sum of line item)
			
			IF((SELECT  ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=@EntityId and LineItemTypeId IS null) = 0)
				BEGIN
					  UPDATE Plan_Campaign_Program_Tactic_LineItem SET  
					  COST=((Select cost from Plan_Campaign_Program_tactic where PlanTacticId=@EntityId)-(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=@EntityId)) 
					  Where LineItemTypeId is null and PlanTacticId=@EntityId
				END


			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y1' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			      from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' 
				END
				  ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
					
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y2' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2'
				END
				ELSE
					BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		 
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y3' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3' 
				END
			ELSE
					BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y4' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' 
				END
					ELSE
					BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y5' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5' 
				END

				ELSE
					BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y6' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6' 
				END
			ELSE
					BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y7' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' 
				END
		ELSE
					BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y8' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8' 
				END
				ELSE
					BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  --  ELSE
	
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y9' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9' 
				END
		ELSE
					BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END


			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y10' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' 
				END
	

	ELSE
					BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y11' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11' 
				END
	
					ELSE
					BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y12' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12' 
				END

					ELSE
					BEGIN
					IF ((SELECT [DEC] from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	
		
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

	END
	

IF ( LOWER(@Type)='lineitem')
		BEGIN
			IF Exists (select top 1 PlanLineItemId from [Plan_Campaign_Program_Tactic_LineItem] where PlanLineItemId =  @EntityId)
			BEGIN		
			--If line item type will be other line item then it will not be updated.
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN

			--update lineitem cost
				UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic_LineItem] P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId

				--update balance row
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId) and LineItemTypeId is null) = 0)
				BEGIN
				  UPDATE Plan_Campaign_Program_Tactic_LineItem SET 
				  COST=((Select cost from Plan_Campaign_Program_tactic WHERE PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId))
				  -(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem 
				  where  PlanLineItemId=@EntityId))) 
				  Where LineItemTypeId is null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId)
			END


			--Y1
			IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
				END
				ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

	
             --Y2
			 	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y2')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y2'
				END
				ELSE
				BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
			---Y3
				IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y3')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y3'
				END
				ELSE
				BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

----Y4

	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
				END
				ELSE
				BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
--Y5
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y5')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y5'
				END
				ELSE
				BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		

---Y6
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y6')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y6'
				END
				ELSE
				BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

---y7
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
				END
				ELSE
				BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
--Y8
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y8')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y8'
				END
				ELSE
				BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y9
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y9')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y9'
				END
				ELSE
				BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
				--Y10
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
				END
				ELSE
				BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
				--Y11
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y11')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y11'
				END
				ELSE
				BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y12
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y12')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y12'
				END
				ELSE
				BEGIN
					IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 END
			END
		END
 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
--select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
--select * from @OutputTable
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp

END
Go


IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_ImportPlanBudgetDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[Sp_ImportPlanBudgetDataQuarterly] 
END

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_ImportPlanActualDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[Sp_ImportPlanActualDataQuarterly] 
END
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_ImportPlanCostDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[Sp_ImportPlanCostDataQuarterly] 
END
GO
--Check Table type is exist or not
IF  EXISTS (SELECT * FROM sys.types WHERE is_user_defined = 1 AND name = 'ImportExcelBudgetQuarterData')
BEGIN
--if table type exist then check sp is exist or not then drop and create it after type creation
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanBudgetDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[ImportPlanBudgetDataQuarterly] 
END

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanActualDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[ImportPlanActualDataQuarterly] 
END
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanCostDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[ImportPlanCostDataQuarterly] 
END

DROP TYPE ImportExcelBudgetQuarterData;

END
Go
CREATE TYPE [dbo].[ImportExcelBudgetQuarterData] AS TABLE(
	[ActivityId] [int] NULL,
	[Type] [nvarchar](255) NULL,
	[Task Name] [nvarchar](3000) NULL,
	[Budget] [float] NULL,
	[Q1] [float] NULL,
	[Q2] [float] NULL,
	[Q3] [float] NULL,
	[Q4] [float] NULL
)
GO
CREATE PROCEDURE [dbo].[ImportPlanBudgetDataQuarterly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetQuarterData READONLY,
@UserId INT 
--@ClientId INT
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Plan' as ActivityType, Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select 
Convert(varchar(max),[PlanId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
  SELECT p.planid, value , period,Budget, p.Title
  FROM  [Plan] p
  left JOIN plan_budget pb on p.PlanId=pb.PlanId
  
   WHERE p.PlanId in( SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
) d
pivot
(
   SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
) as rPlan group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
union 
select ActivityId,[Task Name],'Campaign' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanCampaignId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
  select pc.PlanCampaignId,value, period,CampaignBudget as Budget, pc.Title from Plan_Campaign pc
  left join Plan_Campaign_Budget pcb on pc.planCampaignid = pcb.PlanCampaignId where pc.PlanId in(
SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted = 0
  
) e
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
) as rPlanCampaign group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

union
select ActivityId,[Task Name],'Program' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanProgramId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select t.PlanProgramId,t.Title,t.Budget,Value,Period from
 (
  select pc.PlanProgramId, pc.Title,ProgramBudget as Budget from Plan_Campaign_Program pc where IsDeleted=0 and PlanCampaignId in
   ( select PlanCampaignId from Plan_Campaign where PlanId in( 
 SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0) 
  ) as t
  left join Plan_Campaign_Program_Budget pcb on t.PlanProgramId= pcb.PlanProgramBudgetId 
  
) r
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
) as rPlanCampaignProgram group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
union
select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Value,Period,b.Title,b.TacticBudget as Budget from Plan_Campaign_Program_Tactic_Budget as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId
in( 
SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan'

)
and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12


) as ExistingData



--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE]) TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData
Declare @Sum float;	DECLARE @newValue FLOAT;
	IF ( @Type='Plan')
		BEGIN
		IF Exists (select top 1 PlanId from [Plan] where PlanId =  @EntityId)
			BEGIN


			UPDATE P SET P.Budget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Budget END
			from [Plan] P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId

			--get data for that specific plan

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinal') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinal
			END 
			SELECT * INTO #tempDataFinal FROM (SELECT * from Plan_Budget where PlanId=@EntityId) a 


			
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataFinal where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId
			--start kausha 
			IF(@newValue!=@Sum)
			BEGIN

			if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y1')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
						IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y1',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
			END
			ELSE

			BEGIN
			EXEC Plan_BudgetQuarterCalculation @EntityId,1,@newValue			
			END
			

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataFinal where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y4')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
						IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y4',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 ELSE
				 BEGIN
					EXEC Plan_BudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END		

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataFinal where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y7')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
						IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y7',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				  ELSE
				 BEGIN
					EXEC Plan_BudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END

		
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataFinal where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y10')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
						IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y10',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				  ELSE
				 BEGIN
					EXEC Plan_BudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END

			END



		END
		
IF ( @Type='Campaign')
		BEGIN
		IF Exists (select top 1 PlanCampaignId from Plan_Campaign where PlanCampaignId =  @EntityId)
			BEGIN

			UPDATE P SET P.CampaignBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.CampaignBudget END
			from [Plan_Campaign] P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinalCampaign') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinalCampaign
			END 
			SELECT * INTO #tempDataFinalCampaign FROM (SELECT * from Plan_Campaign_Budget where PlanCampaignId=@EntityId) a 



			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalCampaign where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y1')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
						IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 ELSE
					BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalCampaign where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y4')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
						IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END

		
			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalCampaign where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y7')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
						IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				 BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END

			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalCampaign where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y10')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
						IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END
		

			END



		END

IF ( @Type='Program')
		BEGIN
		IF Exists (select top 1 PlanProgramId from Plan_Campaign_Program where PlanProgramId =  @EntityId)
			BEGIN

			UPDATE P SET P.ProgramBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.ProgramBudget END 
			from [Plan_Campaign_Program] P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId



			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinalProgram') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinalProgram
			END 
			SELECT * INTO #tempDataFinalProgram FROM (SELECT * from Plan_Campaign_Program_Budget where PlanProgramId=@EntityId) a 



			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalProgram where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y1')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
						IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				  ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,1,@newValue			
				END
			END

				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalProgram where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y4')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
						IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END
		
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalProgram where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y7')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
						IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END
		
				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalProgram where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y10')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
						IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END
		
			END



		END

IF ( @Type='Tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId)
			BEGIN

			UPDATE P SET P.TacticBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.TacticBudget END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId


				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinalTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinalTactic
			END 
			SELECT * INTO #tempDataFinalTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId) a 



			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalTactic where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalTactic where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y4')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalTactic where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y7')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(SUM(value),0) from #tempDataFinalTactic where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y10')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END			
		
			END

		END

 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End

select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
END

Go
CREATE PROCEDURE [dbo].[ImportPlanActualDataQuarterly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetQuarterData READONLY,
@UserId INT 
--@ClientId INT
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Actualvalue as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Actual as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId and LOWER(a.StageTitle)='cost'
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0))   
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
UNION
select ActivityId,[Task Name],'lineitem' as ActivityType,0 as Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
0 AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_Tactic_LineItem_Actual as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData



--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE]) TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData
Declare @Sum float;	DECLARE @newValue FLOAT;
	



IF (LOWER(@Type)='tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId and [Status] IN('In-Progress','Complete','Approved'))
			BEGIN

			IF NOT EXISTS(Select * from Plan_Campaign_Program_Tactic_LineItem where PlanTacticId =  @EntityId  AND IsDeleted=0 and LineItemTypeId IS NOT NULL)
			BEGIN			

				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataActualTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataActualTactic
			END 
			SELECT * INTO #tempDataActualTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId and StageTitle='Cost') a 
			SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempDataActualTactic where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y1' and StageTitle='Cost')
						BEGIN
					UPDATE P SET P.Actualvalue = CASE WHEN T.Q1 != '' THEN p.Actualvalue+(@newValue-@Sum) ELSE P.Actualvalue END,p.ModifiedDate=GETDATE(),p.ModifiedBy=@UserId 
			     from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' and StageTitle='Cost'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost', 'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC Tactic_ActuallQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempDataActualTactic where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y4' and StageTitle='Cost')
						BEGIN
					UPDATE P SET P.Actualvalue = CASE WHEN T.Q2 != '' THEN p.Actualvalue+(@newValue-@Sum) ELSE P.Actualvalue END ,p.ModifiedDate=GETDATE(),p.ModifiedBy=@UserId
			     from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' and StageTitle='Cost'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost', 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_ActuallQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempDataActualTactic where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y7' and StageTitle='Cost')
						BEGIN
					UPDATE P SET P.Actualvalue = CASE WHEN T.Q3 != '' THEN p.Actualvalue+(@newValue-@Sum) ELSE P.Actualvalue END ,p.ModifiedDate=GETDATE(),p.ModifiedBy=@UserId
			     from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' and StageTitle='Cost'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost', 'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_ActuallQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(SUM(Actualvalue),0) from #tempDataActualTactic where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y10' and StageTitle='Cost')
						BEGIN
					UPDATE P SET P.Actualvalue = CASE WHEN T.Q4 != '' THEN p.Actualvalue+(@newValue-@Sum) ELSE P.Actualvalue END ,p.ModifiedDate=GETDATE(),p.ModifiedBy=@UserId
			     from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' and StageTitle='Cost'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost', 'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_ActuallQuarterCalculation @EntityId,4,@newValue			
				END
			END			
		End
		 END
       --complete end
		END

IF (LOWER(@Type)='lineitem')
		BEGIN
	
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where PlanLineItemId =  @EntityId)
			BEGIN
			
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN

			IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where  PlanTacticId =(select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where PlanLineItemId= @EntityId) and [Status] IN('In-Progress','Complete','Approved'))
			BEGIN


			--UPDATE P SET P.TacticBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.TacticBudget END
			--from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId


				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataLineItemActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataLineItemActual
			END 
			
			SELECT * INTO #tempDataLineItemActual FROM (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId) a 

			

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and LOWEr([ActivityType])='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Actual (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC LineItem_ActuallQuarterCalculation @EntityId,1,@newValue			
				END
			END
			
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Actual (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_ActuallQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Actual (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_ActuallQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Actual (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_ActuallQuarterCalculation @EntityId,4,@newValue			
				END
			END		
			
			
				
		END

		END
	END

		END




 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp
END

--Insertation End #2623 import multiple plan
GO
CREATE PROCEDURE [dbo].[ImportPlanCostDataQuarterly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetQuarterData READONLY,
@UserId INT 
--@ClientId INT
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, value as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Cost as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId 
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0))   
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
UNION
select ActivityId,[Task Name],'lineitem' as ActivityType,0 as Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
0 AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_Tactic_LineItem_Cost as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData



--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE]) TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData
Declare @Sum float;	DECLARE @newValue FLOAT;
	



IF (LOWER(@Type)='tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId )
			BEGIN				

			--update tactic cost
			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId

				--update balance lineitem
			
			IF((SELECT  ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=@EntityId and LineItemTypeId IS null) = 0)
				BEGIN
					  UPDATE Plan_Campaign_Program_Tactic_LineItem SET  
					  COST=((Select cost from Plan_Campaign_Program_tactic where PlanTacticId=@EntityId)-(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=@EntityId)) 
					  Where LineItemTypeId is null and PlanTacticId=@EntityId
				END


				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataActualTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataActualTactic
			END 
			SELECT * INTO #tempDataActualTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId ) a 
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y4' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y7' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y10' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,4,@newValue			
				END
			END			
		
		 END
       --complete end
		END

IF (LOWER(@Type)='lineitem')
		BEGIN
	
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where PlanLineItemId =  @EntityId)
			BEGIN
			
			--If line item type will be other line item then it will not be updated.
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN


			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic_LineItem] P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId



			--update balance row
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem 
			Where PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId) and LineItemTypeId is null) = 0)
				BEGIN
				  UPDATE Plan_Campaign_Program_Tactic_LineItem SET 
				  COST=((Select cost from Plan_Campaign_Program_tactic WHERE PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId))
				  -(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem 
				  where  PlanLineItemId=@EntityId))) 
				  Where LineItemTypeId is null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId)
			END





				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataLineItemActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataLineItemActual
			END 
			
			SELECT * INTO #tempDataLineItemActual FROM (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Cost WHERE PlanLineItemId = @EntityId) a 

			

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and LOWEr([ActivityType])='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,1,@newValue			
				END
			END
			
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,4,@newValue			
				END
			END		
			
			
				
		END
	END

		END




 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp
END

--Insertation End #2623 import multiple plan

Go

-- =============================================
-- Author:<Author,,Rahul Shah>
-- Create date: <Create Date,29-Sept-2016,>
-- Description:	<This is a insertion script to add a color code for LineItem>
-- =============================================
DECLARE @TableName nvarchar(255) = 'EntityTypeColor'
DECLARE @EntityType nvarchar(255) = 'LineItem'
DECLARE @ColorCode nvarchar(255) = 'FFFFFF'

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName))
BEGIN
	if (NOT EXISTS(SELECT * FROM EntityTypeColor  WHERE EntityType = @EntityType))
	BEGIN
		INSERT INTO EntityTypeColor VALUES (@EntityType, @ColorCode)
	END
END

Go


/****** Object:  StoredProcedure [dbo].[GetPlanBudget]    Script Date: 10/05/2016 5:17:16 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPlanBudget]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPlanBudget]
GO
/****** Object:  StoredProcedure [dbo].[GetPlanBudget]    Script Date: 10/05/2016 5:17:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPlanBudget]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetPlanBudget] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 09/08/2016
-- Description:	This store proc. return data for budget tab for repective plan, campaign, program and tactic
-- =============================================
ALTER PROCEDURE [dbo].[GetPlanBudget]--[GetPlanBudget] '19421','98,104,66,410,308','14917,32098,32097,14918,14916,14919,14920','Created,Submitted,Approved,In-Progress,Complete,Declined',308,'Tactic','2016-2017',0
	(
	@PlanId NVARCHAR(MAX),
	@ownerIds nvarchar(max)='',
	@tactictypeIds varchar(max)='',
	@statusIds varchar(max)='',
	@UserID INT = 0,
	@ViewBy varchar(500)='',
	@TimeFrame VARCHAR(20)='',
	@isGrid bit=0
	)
AS
BEGIN
	
DECLARE @tmp TABLE
(
			EntityId		BIGINT,
			ParentEntityId	BIGINT,
			EntityType NVARCHAR(50),
			StartDate DATETIME,
			EndDate DATETIME,
			ColorCode NVARCHAR(50),
			TaskId NVARCHAR(500),
			ParentTaskId NVARCHAR(500),
			EntityTitle NVARCHAR(1000),
			ROIPackageIds	Varchar(max)
)

Declare @ViewbyTable TABLE
(
	EntityTitle NVARCHAR(1000),
	EntityType NVARCHAR(50),
	TaskId NVARCHAR(500)
)

INSERT INTO @tmp
SELECT EntityId,ParentEntityId,EntityType,StartDate,EndDate,ColorCode,TaskId,ParentTaskId,EntityTitle,ROIPackageIds FROM fnViewByEntityHierarchy(@PlanId,@ownerIds,@tactictypeIds,@statusIds,@ViewBy,@TimeFrame,@isGrid)

INSERT INTO @ViewbyTable
SELECT EntityTitle,EntityType,TaskId FROM @tmp WHERE EntityType NOT IN ('Plan','Campaign','Program','Tactic','LineItem')



SELECT		 0 Id
			,TaskId
			,NULL ParentTaskId
			,NULL ActivityId
			,EntityType ActivityType
			,EntityTitle Title
			,NULL ParentActivityId
			,GETDATE() StartDate
			,GETDATE() EndDate
			,NULL ColorCode
			,0 LinkTacticId
			,0 TacticTypeId
			,NULL MachineName
			,0 CreatedBy
			,NULL LineItemTypeId
			,0 IsAfterApproved
			,0 IsOwner
			,0 Cost
			,0 Budget
			,0 PlanYear
			--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
			,NULL [Y1],NULL [Y2],NULL [Y3],NULL [Y4],NULL [Y5],NULL [Y6],NULL [Y7],NULL [Y8],NULL [Y9],NULL [Y10],NULL [Y11],NULL [Y12]
			,NULL [Y13],NULL [Y14],NULL [Y15],NULL [Y16],NULL [Y17],NULL [Y18],NULL [Y19],NULL [Y20],NULL [Y21],NULL [Y22],NULL [Y23],NULL [Y24]

			,0 TotalUnallocatedBudget
			--Viewby entity has no cost at table level so it default set to null
			,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
			,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
			,0 TotalUnAllocationCost
			--Plan entity has no actual at table level so it default set to null
			,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
			,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
			,0 TotalAllocationActual
			,NULL ROITacticType
			,0 IsAnchorTacticId
			,NULL CalendarHoneycombpackageIDs
			,NULL LinkedPlanName
			FROM @ViewbyTable
UNION ALL

SELECT		Id
			,TaskId
			,ParentTaskId
			,ActivityId
			,ActivityType
			,Title
			,ParentActivityId
			,StartDate
			,EndDate
			,ColorCode
			,0 LinkTacticId
			,0 TacticTypeId
			,NULL MachineName
			,CreatedBy
			,NULL LineItemTypeId
			,0 IsAfterApproved
			,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
			,0 Cost
			,Budget
			,PlanYear
			--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
			,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
			,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]

			,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
			--Plan entity has no cost at table level so it default set to null
			,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
			,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
			,0 TotalUnAllocationCost
			--Plan entity has no actual at table level so it default set to null
			,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
			,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
			,0 TotalAllocationActual
			,NULL ROITacticType
			,0 IsAnchorTacticId
			,NULL CalendarHoneycombpackageIDs
			,NULL LinkedPlanName
		FROM 
				(SELECT 
					P.PlanId Id
					,H.TaskId
					,H.ParentTaskId
					,H.EntityId ActivityId
					,P.Title
					,'plan' as ActivityType
					,H.ParentEntityId ParentActivityId
					,ISNULL(H.StartDate, GETDATE()) AS StartDate
					,ISNULL(H.EndDate, GETDATE()) AS EndDate
					,H.ColorCode
					,P.CreatedBy
					,P.[Year] PlanYear
					, Budget
					,PB.Value
					,PB.Period
				FROM @tmp H 
					INNER JOIN [Plan] P ON H.EntityId=P.PlanId 
					LEFT JOIN Plan_Budget PB ON P.PlanId=PB.PlanId
				WHERE H.EntityType='Plan' 
				)Pln
				PIVOT
				(
					MAX(value)
					for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
									,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
				)PLNMain
UNION ALL
SELECT 
		Id
		,TaskId
		,ParentTaskId
		,ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,0 LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,NULL LineItemTypeId
		,0 IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,0 Cost
		,Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12],[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
		--Plan entity has no cost at table level so it default set to null
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
		,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
		,0 TotalUnAllocationCost
		--Plan entity has no actual at table level so it default set to null
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
		,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
		,0 TotalAllocationActual
		,NULL ROITacticType
		,0 IsAnchorTacticId
		,NULL CalendarHoneycombpackageIDs
		,NULL LinkedPlanName
	 FROM
			(SELECT 
				PC.PlanCampaignId Id	
				,H.TaskId
				,H.ParentTaskId
				,H.EntityId ActivityId
				,PC.Title
				,'campaign' as ActivityType
				,H.ParentEntityId ParentActivityId
				,ISNULL(H.StartDate, GETDATE()) AS StartDate
				,ISNULL(H.EndDate, GETDATE()) AS EndDate
				,H.ColorCode
				,PC.CreatedBy
				,CampaignBudget Budget
				,PCB.Value
				,PCB.Period
			FROM @tmp H
				INNER JOIN Plan_Campaign PC  ON H.EntityId=PC.PlanCampaignId 
				LEFT JOIN Plan_Campaign_Budget PCB ON PC.PlanCampaignId=PCB.PlanCampaignId  
			WHERE H.EntityType='Campaign'
			)Campaign
			PIVOT
			(
				MAX(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
								,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
			)CampaignMain
UNION ALL
	SELECT 
		Id
		,TaskId
		,ParentTaskId
		,ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,0 LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,NULL LineItemTypeId
		,0 IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,0 Cost
		,Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12],[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
		--Plan entity has no cost at table level so it default set to null
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY10], NULL [CostY11], NULL [CostY12]
		,NULL [CostY13], NULL [CostY14], NULL [CostY15], NULL [CostY16],NULL [CostY17], NULL [CostY18], NULL [CostY19], NULL [CostY20],NULL [CostY21], NULL [CostY22], NULL [CostY23], NULL [CostY24]
		,0 TotalUnAllocationCost
		--Plan entity has no actual at table level so it default set to null
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY10], NULL [ActualY11], NULL [ActualY12]
		,NULL [ActualY13], NULL [ActualY14], NULL [ActualY15], NULL [ActualY16],NULL [ActualY17], NULL [ActualY18], NULL [ActualY19], NULL [ActualY20],NULL [ActualY21], NULL [ActualY22], NULL [ActualY23], NULL [ActualY24]
		,0 TotalAllocationActuals
		,NULL ROITacticType
		,0 IsAnchorTacticId
		,NULL CalendarHoneycombpackageIDs
		,NULL LinkedPlanName
	 FROM
			(SELECT 
				PCP.PlanProgramId Id
				,H.TaskId
				,H.ParentTaskId
				,H.EntityId ActivityId
				,PCP.Title
				,'program' as ActivityType
				,H.ParentEntityId ParentActivityId
				,ISNULL(H.StartDate, GETDATE()) AS StartDate
				,ISNULL(H.EndDate, GETDATE()) AS EndDate
				,H.ColorCode
				,PCP.CreatedBy
				,PCP.ProgramBudget Budget
				,PCPB.Value
				,PCPB.Period
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program PCP ON H.EntityId=PCP.PlanProgramId 
				LEFT JOIN Plan_Campaign_Program_Budget PCPB ON PCP.PlanProgramId=PCPB.PlanProgramId
			WHERE H.EntityType='Program'
			)Program
			PIVOT
			(
				MAX(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
								,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
			)ProgramMain
UNION ALL
	SELECT 
		Id
		,TaskId
		,ParentTaskId
		,ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,ISNULL(LinkedTacticId, 0) LinkTacticId
		,ISNULL(TacticTypeId, 0) TacticTypeId
		,TacticCustomName MachineName
		,CreatedBy
		,NULL LineItemTypeId
		,IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,Cost		
		,Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24]
		
		,Budget - (ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)
						+ISNULL( [Y13],0)+ ISNULL( [Y14],0)+ ISNULL( [Y15],0)+ ISNULL( [Y16],0)+ISNULL( [Y17],0)+ ISNULL( [Y18],0)+ ISNULL( [Y19],0)+ ISNULL( [Y20],0)+ISNULL( [Y21],0)+ ISNULL( [Y22],0)+ ISNULL( [Y23],0)+ ISNULL( [Y24],0)) TotalUnallocatedBudget
		
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24]
		
		,Cost - (ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0) 
			+ISNULL([CostY13],0)+ ISNULL([CostY14],0)+ ISNULL([CostY15],0)+ ISNULL([CostY16],0)+ISNULL([CostY17],0)+ ISNULL([CostY18],0)+ ISNULL([CostY19],0)+ ISNULL([CostY20],0)+ISNULL([CostY21],0)+ ISNULL([CostY22],0)+ ISNULL([CostY23],0)+ ISNULL([CostY24],0)) TotalUnAllocationCost
		
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24]
		
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) 
			+ISNULL([ActualY13],0)+ ISNULL([ActualY14],0)+ ISNULL([ActualY15],0)+ ISNULL([ActualY16],0)+ISNULL([ActualY17],0)+ ISNULL([ActualY18],0)+ ISNULL([ActualY19],0)+ ISNULL([ActualY20],0)+ISNULL([ActualY21],0)+ ISNULL([ActualY22],0)+ ISNULL([ActualY23],0)+ ISNULL([ActualY24],0) TotalAllocationActuals
	     ,AssetType ROITacticType
		,ISNULL(AnchorTacticID,0) IsAnchorTacticId
		,CalendarHoneycombpackageIDs
		,LinkedPlanName
	 FROM
			(SELECT
				PCPT.PlanTacticId Id 
				,H.TaskId
				,H.ParentTaskId
				,H.EntityId ActivityId
				,PCPT.Title
				,'tactic' as ActivityType
				,H.ParentEntityId ParentActivityId
				,ISNULL(H.StartDate, GETDATE()) AS StartDate
				,ISNULL(H.EndDate, GETDATE()) AS EndDate
				,H.ColorCode
				,PCPT.LinkedTacticId
				,PCPT.TacticTypeId
				,PCPT.TacticCustomName 
				,PCPT.CreatedBy
				,PCPT.TacticBudget Budget
				,CASE WHEN PCPT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
				,PCPTB.Value
				,PCPTB.Period
				,PCPTC.Value as CValue
				,'Cost'+PCPTC.Period as CPeriod
				,PCPTA.Actualvalue as AValue
				,'Actual'+PCPTA.Period as APeriod
				,PCPT.Cost
				,RPD.AnchorTacticID
				,TP.AssetType
				,CalendarHoneycombpackageIDs = H.ROIPackageIds
				,P.Title as 'LinkedPlanName'
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program_Tactic PCPT ON H.EntityId=PCPT.PlanTacticId 
				LEFT JOIN Plan_Campaign_Program_Tactic_Budget PCPTB ON PCPT.PlanTacticId=PCPTB.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Cost PCPTC ON PCPT.PlanTacticId=PCPTC.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Actual PCPTA ON PCPT.PlanTacticId=PCPTA.PlanTacticId AND PCPTA.StageTitle='Cost'
				LEFT JOIN ROI_PackageDetail RPD ON RPD.PlanTacticId = PCPT.PlanTacticId
				LEFT JOIN [Plan] P ON P.PlanId = PCPT.LinkedPlanId
				INNER JOIN TacticType TP ON (TP.TacticTypeId = PCPT.TacticTypeId)				
			WHERE H.EntityType='Tactic'
			)Tactic
			PIVOT
			(
				MAX(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
								,[Y13], [Y14], [Y15], [Y16],[Y17], [Y18], [Y19], [Y20],[Y21], [Y22], [Y23], [Y24])
			)TacticMain
			PIVOT
			(
				MAX(CValue)
				for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
								,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24])
			)TacticMain1
			PIVOT
			(
				MAX(AValue)
				for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
								,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24])
			)TacticMain2

UNION ALL
	SELECT 
		Id
		,TaskId
		,ParentTaskId
		,ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,StartDate
		,EndDate
		,ColorCode
		,ISNULL(LinkedLineItemId, 0) LinkTacticId
		,0 TacticTypeId
		,NULL MachineName
		,CreatedBy
		,LineItemTypeId
		,IsAfterApproved
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
		,Cost
		,0 Budget
		,NULL PlanYear
		--Y represent year and number represent month of the year. If number is greater than 12 then its consider as next year month e.g. Y13 is Jan month for next year
		--Line item has no budget at table level so its default set to null
		,NULL [Y1],NULL [Y2],NULL [Y3],NULL [Y4],NULL [Y5],NULL [Y6],NULL [Y7],NULL [Y8],NULL [Y9],NULL [Y10],NULL [Y11],NULL [Y12]
		,NULL [Y13],NULL [Y14],NULL [Y15],NULL [Y16],NULL [Y17],NULL [Y18],NULL [Y19],NULL [Y20],NULL [Y21],NULL [Y22],NULL [Y23],NULL [Y24]
		,0 TotalUnallocatedBudget
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24]
		
		,Cost - (ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0) 
			+ISNULL([CostY13],0)+ ISNULL([CostY14],0)+ ISNULL([CostY15],0)+ ISNULL([CostY16],0)+ISNULL([CostY17],0)+ ISNULL([CostY18],0)+ ISNULL([CostY19],0)+ ISNULL([CostY20],0)+ISNULL([CostY21],0)+ ISNULL([CostY22],0)+ ISNULL([CostY23],0)+ ISNULL([CostY24],0)) TotalUnAllocationCost
		
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24]
		
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) 
			+ISNULL([ActualY13],0)+ ISNULL([ActualY14],0)+ ISNULL([ActualY15],0)+ ISNULL([ActualY16],0)+ISNULL([ActualY17],0)+ ISNULL([ActualY18],0)+ ISNULL([ActualY19],0)+ ISNULL([ActualY20],0)+ISNULL([ActualY21],0)+ ISNULL([ActualY22],0)+ ISNULL([ActualY23],0)+ ISNULL([ActualY24],0)TotalAllocationActuals
	    ,NULL ROITacticType
		,0 IsAnchorTacticId
		,NULL CalendarHoneycombpackageIDs
		,NULL LinkedPlanName
	 FROM
		 (SELECT	PCPTL.PlanLineItemId Id
					,H.TaskId
					,H.ParentTaskId
					,H.EntityId ActivityId
					,PCPTL.Title
					,'lineitem' as ActivityType
					,H.ParentEntityId ParentActivityId
					,ISNULL(H.StartDate, GETDATE()) AS StartDate
					,ISNULL(H.EndDate, GETDATE()) AS EndDate
					,H.ColorCode
					,PCPTL.CreatedBy
					,PCPTL.LineItemTypeId LineItemTypeId
					,CASE WHEN PCPT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
					,PCPTLC.Value as CValue
					,'Cost'+PCPTLC.Period as CPeriod
					,PCPTLA.Value as AValue
					,'Actual'+PCPTLA.Period as APeriod
					,PCPTL.Cost
					,PCPTL.LinkedLineItemId
				FROM @tmp H
					INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON H.EntityId=PCPTL.PlanLineItemId 
					INNER JOIN Plan_Campaign_Program_Tactic PCPT ON PCPTL.PlanTacticId=PCPT.PlanTacticId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PCPTLC ON PCPTL.PlanLineItemId=PCPTLC.PlanLineItemId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId=PCPTLA.PlanLineItemId
				WHERE H.EntityType='LineItem'
				)LineItem
				PIVOT
				(
				 MAX(CValue)
				  for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
									,[CostY13], [CostY14], [CostY15], [CostY16],[CostY17], [CostY18], [CostY19], [CostY20],[CostY21], [CostY22], [CostY23], [CostY24])
				)LineItemMain
				PIVOT
				(
				 MAX(AValue)
				  for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
									,[ActualY13], [ActualY14], [ActualY15], [ActualY16],[ActualY17], [ActualY18], [ActualY19], [ActualY20],[ActualY21], [ActualY22], [ActualY23], [ActualY24])
				)LineItemMain
END




GO

-- =============================================
-- Author:		Dhvani Raval
-- Create date: 09/26/2016
-- Description:	This store proc. is used to senf alerts mail
-- =============================================
IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'sp_Send_Mail_For_Alerts') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE dbo.sp_Send_Mail_For_Alerts
END
GO

CREATE PROCEDURE [dbo].[sp_Send_Mail_For_Alerts]
        @from Nvarchar(500),
		@pwd Nvarchar(30),
		@Url Nvarchar(max),
		@SMTPPort Nvarchar(10),
		@SMTPServerAddress Nvarchar(100)	
AS 
BEGIN

  DECLARE @imsg int
  DECLARE @hr int
  DECLARE @source varchar(255)
  DECLARE @description varchar(500)
  DECLARE @bodytype varchar(10)
  DECLARE @to varchar(500) 
  DECLARE @Comparision varchar(500)
  DECLARE @body varchar(max) 
  DECLARE @subject varchar(max) 
  DECLARE @output_desc varchar(1000)
  DECLARE @Result varchar(1000)
  DECLARE @UrlString varchar(max)
  DECLARE @SubjectComparision varchar(500)
  DECLARE @Query Nvarchar(max)
   
  SET @bodytype = 'htmlbody'
  
  BEGIN
  DECLARE @AlertId int, @AlertDescription nvarchar(max), @Email  nvarchar(255),@Indicator nvarchar(50), @IndicatorComparision nvarchar(10),@IndicatorGoal int, @EntityTitle nvarchar(500),@DisplayDate DateTime, @ActualGoal float, @CurrentGoal float,@PlanName nvarchar(255),@Entity nvarchar(255),@PlanId int,@EntityId int
  			
  DECLARE Cur_Mail CURSOR FOR	
  --Select alert data that has display date same as current date or less than that, email not sent and that has user email address 
               SELECT  al.AlertId, al.Description, ar.UserEmail, 
			   CASE WHEN ar.Indicator = 'PLANNEDCOST' 
					THEN 'PLANNED COST'
					when ar.Indicator = 'REVENUE' 
					THEN 'REVENUE'
					ELSE Stage.Title END AS
					 Indicator, ar.IndicatorComparision,ar.IndicatorGoal, vw.EntityTitle,al.DisplayDate,al.ActualGoal,al.CurrentGoal, vw.PlanTitle,vw.Entity, vw.PlanId,vw.EntityId
			   FROM  [dbo].Alerts al
			   INNER JOIN dbo.[Alert_Rules] AS ar ON ar.RuleId = al.RuleId  AND ar.UserId = al.UserId
			   LEFT JOIN dbo.Stage ON Stage.Code = ar.Indicator  AND ar.ClientId = Stage.ClientId
			   LEFT JOIN dbo.vClientWise_EntityList as vw on vw.EntityId = ar.EntityId and vw.Entity = ar.EntityType
			   WHERE al.DisplayDate <= GETDATE() AND (isnull(al.IsEmailSent,'') <> 'Success') AND (ar.UserEmail IS NOT NULL AND  ar.UserEmail <> '')
  
    
  OPEN Cur_Mail
  
        FETCH NEXT FROM Cur_Mail INTO  @AlertId, @AlertDescription, @Email, @Indicator, @IndicatorComparision, @IndicatorGoal, @EntityTitle, @DisplayDate,@ActualGoal,@CurrentGoal,@PlanName,@Entity,@PlanId,@EntityId
        
		
        WHILE @@FETCH_STATUS=0
        BEGIN


		Begin
		--print  @Email
        EXEC @hr = sp_oacreate 'cdo.message', @imsg out
        
        --SendUsing Specifies Whether to send using port (2) or using pickup directory (1)
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendusing").value','2'
        
        --SMTP Server
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpserver").value',@SMTPServerAddress 
        
		--PORT 
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpserverport").value',@SMTPPort

        --UserName
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendusername").value',@from
        
        --Password
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendpassword").value',@pwd
  
        --UseSSL
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpusessl").value','True' 
        			       
        --Requires Aunthentication None(0) / Basic(1)
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate").value','1' 	
        
	END
      
        IF @IndicatorComparision = 'GT'
		BEGIN
              set  @Comparision = '<b> greater than </b>'
			  set @SubjectComparision = 'above goal'
		END
        ELSE IF @IndicatorComparision = 'LT'
			  	 BEGIN
                  set @Comparision = '<b>  less than </b>' 
				  set @SubjectComparision = 'below goal'
				 END
              ELSE 
			     BEGIN
                  set @Comparision = '<b> equal to </b>' 
				  set @SubjectComparision = 'equal goal'
				END
		
		
        Set @subject = @EntityTitle + ' is performing ' + @SubjectComparision 

		SET @body = 'Hi, <br><br>'+ @EntityTitle + '''s <b>' + @Indicator +' </b> is '+ @Comparision + ' ' + CONVERT(nvarchar(50),@IndicatorGoal) +'% of the goal as of <b>'+ CONVERT(VARCHAR(11),@DisplayDate,106)  + '</b><br><br>Item : ' +  @EntityTitle + '<br>Plan Name : '+ @PlanName 
	    
		IF (@ActualGoal IS NOT NULL AND @ActualGoal <> '' )
		SET @body = @body +  '<br>Goal : $'+ cast(Format(@ActualGoal, '##,##0') as varchar) 

		IF (@CurrentGoal IS NOT NULL AND @CurrentGoal <> '')
		SET @body = @body + '<br>Current : $' + cast(Format(@CurrentGoal, '##,##0') as varchar)
		 

		 IF (@Url <> '' AND @Url IS NOT NULL)
		 BEGIN
			 IF @Entity <> 'Plan' 
			 BEGIN
			 print @Entity
			     SET @UrlString = @Url +'home?currentPlanId='+convert(nvarchar(max), @planId)+'&plan'+ Replace(@Entity,' ' ,'')+'Id='+convert(nvarchar(max),@EntityId)+'&activeMenu=Home&ShowPopup=true'
			 END
			 ELSE
			 BEGIN
			     SET @UrlString = @Url +'home?currentPlanId='+convert(nvarchar(max), @planId)+'&activeMenu=Home&ShowPopup=true'
			 END
		 END

		set @body = @body + '<br><br><html><body> URL : <a href=' + @UrlString +'>'+@UrlString+'</a></body></html>' 

		set @body = @body + '<br> Thank you,<br>Hive9 Plan Admin.'
        
        EXEC @hr = sp_oamethod @imsg, 'configuration.fields.update', null
        EXEC @hr = sp_oasetproperty @imsg, 'to', @Email
        EXEC @hr = sp_oasetproperty @imsg, 'from', @from
        EXEC @hr = sp_oasetproperty @imsg, 'subject', @subject
        EXEC @hr = sp_oasetproperty @imsg, @bodytype, @body
        EXEC @hr = sp_oamethod @imsg, 'send',null        
		
        -- sample error handling.	
        IF @hr <> 0   
        	BEGIN
			    EXEC @hr = sp_oageterrorinfo null, out, @description out
			    IF @hr = 0
        		BEGIN			
        		    update Alerts
            		set IsEmailSent =  @description  --'Not Sent'
        			where AlertId = @AlertId
				END
				ELSE
				BEGIN
        		    update Alerts
            		set IsEmailSent = 'FAIL'
        			where AlertId = @AlertId
				END
			END
        ELSE
        	BEGIN
        		update  Alerts
        		set IsEmailSent = 'Success'
        		where AlertId = @AlertId
        	END

             
        EXEC @hr = sp_oadestroy @imsg
        
        FETCH NEXT FROM Cur_Mail INTO  @AlertId, @AlertDescription, @Email, @Indicator, @IndicatorComparision, @IndicatorGoal, @EntityTitle, @DisplayDate, @ActualGoal, @CurrentGoal, @PlanName,@Entity,@PlanId,@EntityId
  
		END
  CLOSE Cur_Mail
  DEALLOCATE Cur_Mail
  END
  END
GO

/* Start - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

-- Stored procedure to get tactic and line items cost allocation data for tactic inspect popup

-- DROP AND CREATE STORED PROCEDURE [dbo].[LineItem_Cost_Allocation]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_Cost_Allocation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[LineItem_Cost_Allocation]
END
GO

-- [dbo].[LineItem_Cost_Allocation] 22105,621
CREATE PROCEDURE [dbo].[LineItem_Cost_Allocation]
( 
	@PlanTacticId INT,
	@UserId INT
)
AS
BEGIN

	-- Calculate tactic and line item planned cost allocated by monthly/quarterly
	SELECT Id,ActivityId
	,ActivityName
	,ActivityType
	,ParentActivityId
	,CreatedBy
	,Cost
	,StartDate
	,EndDate
	,ISNULL([Y1],0) [Y1],ISNULL([Y2],0) [Y2],ISNULL([Y3],0) [Y3],ISNULL([Y4],0) [Y4],ISNULL([Y5],0) [Y5],ISNULL([Y6],0) [Y6],
	ISNULL([Y7],0) [Y7],ISNULL([Y8],0) [Y8],ISNULL([Y9],0) [Y9],ISNULL([Y10],0) [Y10],ISNULL([Y11],0) [Y11],ISNULL([Y12],0) [Y12],
	ISNULL([Y13],0) [Y13],ISNULL([Y14],0) [Y14],ISNULL([Y15],0) [Y15],ISNULL([Y16],0) [Y16],ISNULL([Y17],0) [Y17],
	ISNULL([Y18],0) [Y18],ISNULL([Y19],0) [Y19],ISNULL([Y20],0) [Y20],ISNULL([Y21],0) [Y21],ISNULL([Y22],0) [Y22],
	ISNULL([Y23],0) [Y23],ISNULL([Y24],0) [Y24]
	,LineItemTypeId
	,LinkTacticId

	FROM
	(
		-- Tactic cost allocation
		SELECT DISTINCT CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
			,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
			,PT.Title AS ActivityName
			,'tactic' ActivityType
			,'cp_' + CAST(PT.PlanProgramId AS NVARCHAR(20)) AS ParentActivityId
			,PT.CreatedBy 
			,PT.Cost
			,PTCst.Period as Period
			,PTCst.Value as Value
			,0 LineItemTypeId
			,PT.StartDate 
			,PT.EndDate
			,PT.LinkedTacticId AS LinkTacticId
		FROM Plan_Campaign_Program_Tactic PT
		LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId = PTCst.PlanTacticId
		WHERE PT.PlanTacticId = @PlanTacticId AND PT.IsDeleted = 0 

		UNION ALL
		-- Line item cost allocation
		SELECT 
			CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
			,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
			,PL.Title as ActivityName
			,'lineitem' ActivityType
			,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(20)) ParentActivityId
			,PL.CreatedBy
			,PL.Cost
			,PLC.period as period 
			,PLC.Value
			,PL.LineItemTypeId as LineItemTypeId
			,PT.StartDate 
			,PT.EndDate
			,0 AS LinkTacticId
		FROM Plan_Campaign_Program_Tactic_LineItem PL
		INNER JOIN Plan_Campaign_Program_Tactic PT ON PL.PlanTacticId = PT.PlanTacticId 
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId = PLC.PlanLineItemId
		WHERE PT.PlanTacticId = @PlanTacticId AND PL.IsDeleted = 0 AND PT.IsDeleted = 0

	) TacticLineItems
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12], 
		[Y13],[Y14],[Y15],[Y16],[Y17],[Y18],[Y19],[Y20],[Y21],[Y22],[Y23],[Y24])
	) PivotLineItem
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'Title' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic_LineItem'))
BEGIN
	UPDATE Plan_Campaign_Program_Tactic_LineItem SET Title='Sys_Gen_Balance' WHERE LineItemTypeId IS NULL
END
GO

/* End - Added by Arpita Soni for Ticket #2622 on 10/03/2016 */


--Dropping the unused tables from production database as discussed during the code read meeting on 6th October, 2016.
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Campaign_Program_Tactic_2538'))
DROP TABLE [Plan_Campaign_Program_Tactic_2538]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Campaign_Program_Tactic_Cost_2538'))
DROP TABLE [Plan_Campaign_Program_Tactic_Cost_2538]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Campaign_Program_Tactic_LineItem_2538'))
DROP TABLE [Plan_Campaign_Program_Tactic_LineItem_2538]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Campaign_Program_Tactic_LineItem_Cost_2538'))
DROP TABLE [Plan_Campaign_Program_Tactic_LineItem_Cost_2538]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Campaign_Program_Tactic_BKP_15Feb2015'))
DROP TABLE [Plan_Campaign_Program_Tactic_BKP_15Feb2015]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Campaign_Program_Tactic_Temp'))
DROP TABLE [Plan_Campaign_Program_Tactic_Temp]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('Plan_Team'))
DROP TABLE [Plan_Team]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_Campaigns'))
DROP TABLE [ZZZ_Campaigns]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_MarketoID'))
DROP TABLE [ZZZ_MarketoID]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_Tactics'))
DROP TABLE [ZZZ_Tactics]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_URL_APAC'))
DROP TABLE [ZZZ_URL_APAC]
GO


IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_URL_EMEA'))
DROP TABLE [ZZZ_URL_EMEA]
GO


IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_URL_Global'))
DROP TABLE [ZZZ_URL_Global]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_URL_LA'))
DROP TABLE [ZZZ_URL_LA]
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('ZZZ_URL_NA'))
DROP TABLE [ZZZ_URL_NA]
GO
--End

GO




/****** Object:  StoredProcedure [dbo].[GetGridData]    Script Date: 10/17/2016 8:04:18 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGridData]
GO
/****** Object:  StoredProcedure [dbo].[GetGridData]    Script Date: 10/17/2016 8:04:18 PM ******/
SET ANSI_NULLS ON
GO

-- =============================================
-- Author:		Nishant Sheth
-- Create date: 09-Sep-2016
-- Description:	Get home grid data with custom field 19910781.11
-- =============================================
CREATE PROCEDURE [dbo].[GetGridData]
	-- Add the parameters for the stored procedure here
		@PlanId NVARCHAR(MAX) = ''
		,@ClientId INT = 0
		,@OwnerIds NVARCHAR(MAX) = ''
		,@TacticTypeIds varchar(max)=''
		,@StatusIds varchar(max)=''
		,@ViewBy varchar(max)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	DECLARE @StageMqlMaxLevel INT = 1
	DECLARE @StageRevenueMaxLevel INT = 1
	-- Variables for fnGetFilterEntityHierarchy we pass defualt values because no need to pass timeframe option on grid
	DECLARE @TimeFrame VARCHAR(20)='' 
	DECLARE @Isgrid BIT=1

	SELECT @StageMqlMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='MQL'

	SELECT @StageRevenueMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='CW'

	SELECT Hireachy.UniqueId,
				Hireachy.EntityId,		
				Hireachy.EntityTitle,		
				Hireachy.ParentEntityId	,
				Hireachy.ParentUniqueId	,
				IsNull(Hireachy.EntityTypeId,0) As EntityType	,	
				Hireachy.ColorCode,		
				Hireachy.[Status],		
				Hireachy.StartDate,		
				Hireachy.EndDate,			
				Hireachy.CreatedBy AS 'Owner',
				Hireachy.AltId,			
				Hireachy.TaskId,			
				Hireachy.ParentTaskId,	
				Hireachy.PlanId	,		
				Hireachy.ModelId,			
				--TacticType.AssetType,
				TacticType.Title AS TacticType,
				Tactic.TacticTypeId,
				LineItem.LineItemTypeId,
				LineItem.LineItemType,
				CASE WHEN EntityType = 'Tactic'
						THEN Tactic.Cost 
							WHEN EntityType = 'LineItem' 
								THEN LineItem.Cost
				END AS PlannedCost
				,Tactic.ProjectedStageValue 
				,Stage.Title AS 'ProjectedStage'
				,NULL AS 'TargetStageGoal'
				,CASE WHEN Hireachy.EntityType='Tactic'
					THEN
					(SELECT Value FROM dbo.fnGetMqlByEntityTypeAndEntityId('Tactic',@ClientId,Stage.[Level],@StageMqlMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue))
					END
					AS MQL
				,CASE WHEN Hireachy.EntityType='Tactic'
					THEN (SELECT Value FROM dbo.fnGetRevueneByEntityTypeAndEntityId('Tactic',@ClientId,Stage.[Level],@StageRevenueMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue,M.AverageDealSize))
					END AS Revenue
				,Tactic.TacticCustomName AS 'MachineName'
				,Tactic.LinkedPlanId
				,Tactic.LinkedTacticId
				,P.PlanName AS 'LinkedPlanName'
				,ROI.AnchorTacticID
				--PackageTacticIds - comma saperated values selected as part of ROI package
				,Hireachy.ROIPackageIds AS PackageTacticIds 
				,PlanDetail.PlanYear
				--Hireachy.EloquaId AS 'Eloquaid',
				--Hireachy.MarketoId AS 'Marketoid',
				--Hireachy.WorkfrontId AS 'WorkFrontid',
				--Hireachy.SalesforceId AS 'Salesforceid'
				FROM [dbo].fnViewByEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds,@ViewBy,@TimeFrame,@Isgrid) Hireachy
				LEFT JOIN Model M ON Hireachy.ModelId = M.ModelId
				LEFT JOIN Plan_Campaign_Program_Tactic Tactic ON Hireachy.EntityType='Tactic'
					AND Hireachy.EntityId = Tactic.PlanTacticId
	
	OUTER APPLY (SELECT ROI.PlanTacticId
						,ROI.AnchorTacticID FROM ROI_PackageDetail ROI
						WHERE Tactic.PlanTacticId = ROI.PlanTacticId) ROI
	OUTER APPLY (SELECT Title AS 'PlanName'
						FROM [Plan] P WITH (NOLOCK)
					WHERE Tactic.LinkedPlanId = P.PlanId) P
	OUTER APPLY (SELECT [PlanDetail].[Year] AS 'PlanYear'
						FROM [Plan] PlanDetail WITH (NOLOCK)
					WHERE 
					Hireachy.EntityType = 'Plan' AND
					Hireachy.EntityId = PlanDetail.PlanId) PlanDetail
	OUTER APPLY(SELECT TacticType.TacticTypeId,
						--TacticType.AssetType,
						TacticType.Title  
						FROM TacticType WITH (NOLOCK)
						WHERE Tactic.TacticTypeId = TacticType.TacticTypeId) TacticType
	OUTER APPLY (SELECT LineItem.LineItemTypeId,
						LineItem.PlanLineItemId,
						LineItem.Cost,
						LT.Title AS 'LineItemType'
						FROM Plan_Campaign_Program_Tactic_LineItem LineItem WITH (NOLOCK)
						OUTER APPLY(SELECT LT.LineItemTypeId,LT.Title FROM LineItemType LT
									WHERE 
									LineItem.LineItemTypeId = LT.LineItemTypeId
									AND
									 LT.IsDeleted = 0)LT
						WHERE Hireachy.EntityType = 'LineItem'
						AND Hireachy.EntityId = LineItem.PlanLineItemId) LineItem
	OUTER APPLY (SELECT Stage.Title,Stage.StageId,Stage.[Level] FROM Stage WITH (NOLOCK) WHERE Tactic.StageId = Stage.StageId AND Stage.IsDeleted=0) Stage
END
GO



IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GridCustomFieldData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GridCustomFieldData] AS' 
END
GO

-- =============================================
-- Author:		Nishant Sheth
-- Create date: 16-Sep-2016
-- Description:	Get home grid customfields and it's values 
-- =============================================
ALTER PROCEDURE [dbo].[GridCustomFieldData]
	@PlanId	 NVARCHAR(MAX)=''
	,@ClientId int = 0
	,@OwnerIds NVARCHAR(MAX) = ''
	,@TacticTypeIds varchar(max)=''
	,@StatusIds varchar(max)=''
	,@UserId int = 0
	,@SelectedCustomField varchar(max)=''
AS
BEGIN

SET NOCOUNT ON;

	DECLARE @CustomFieldTypeText VARCHAR(20)= 'TextBox'
	DECLARE @CustomFieldTypeDropDown VARCHAR(20)= 'DropDownList'

	-- Variables for fnGetFilterEntityHierarchy we pass defualt values because no need to pass timeframe option on grid
	DECLARE @TimeFrame VARCHAR(20)='' 
	DECLARE @Isgrid BIT=1

	DECLARE @CustomFieldIds TABLE (
		Item BIGINT Primary Key
	)
	INSERT INTO @CustomFieldIds
		SELECT CAST(Item AS BIGINT) as Item FROM dbo.SplitString(@SelectedCustomField,',') 

	-- Get user restricted values
	DECLARE @UserRestrictedValues TABLE (CustomFieldId INT,Text NVARCHAR(MAX),Value NVARCHAR(MAX))
	;WITH CommaSaperated AS (
	SELECT CR.CustomFieldId, CFO.Value,CFO.CustomFieldOptionId FROM CustomRestriction CR
	INNER JOIN CustomFieldOption CFO ON CFO.CustomFieldId = CR.CustomFieldId AND CFO.CustomFieldOptionId = CR.CustomFieldOptionId
	WHERE CR.Userid=@UserId AND CR.Permission IN (2)
	)
	INSERT INTO @UserRestrictedValues
	SELECT CustomFieldId,
	Text = STUFF(
		(SELECT ',' + Value FROM CommaSaperated C WHERE C.CustomFieldId = P.CustomFieldId FOR XML PATH ('') ), 1, 1, ''
	) 
	,Value = STUFF(
		(SELECT ',' + CAST(CustomFieldOptionId AS NVARCHAR(MAX)) FROM CommaSaperated C WHERE C.CustomFieldId = P.CustomFieldId FOR XML PATH ('') ), 1, 1, ''
	) 
	FROM CommaSaperated P GROUP BY CustomFieldId

	-- Get List of Custom fields which are textbox type
	SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,ET.EntityTypeId AS EntityType 
			,C.AbbreviationForMulti
			,@CustomFieldTypeText As 'CustomFieldType'
			,'custom_'+CAST (C.CustomFieldId AS VARCHAR(100))+':'+C.EntityType AS CustomUniqueId
			FROM CustomField  C
			CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
						WHERE selCol.Item = C.CustomFieldId) selCol
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' FROM CustomFieldType CT
				WHERE CT.Name=@CustomFieldTypeText 
				AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			CROSS APPLY (SELECT * FROM EntityType WHERE Name = C.EntityType) ET 
			WHERE ClientId=@ClientId
					AND IsDeleted=0
			UNION ALL
		-- Get Custom fields which are dropdown type and get only that custom fields which have it's option of that custom field
		SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,ET.EntityTypeId AS EntityType 
			,C.AbbreviationForMulti
			,@CustomFieldTypeDropDown AS 'CustomFieldType'
			,'custom_'+CAST (C.CustomFieldId AS VARCHAR(100))+':'+C.EntityType AS CustomUniqueId
			FROM CustomField  C
			CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
						WHERE selCol.Item = C.CustomFieldId) selCol
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' 
							FROM CustomFieldType CT
							WHERE CT.Name=@CustomFieldTypeDropDown 
								AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			CROSS APPLY (SELECT CP.CustomFieldId 
							FROM CustomFieldOption CP
							WHERE 
								C.CustomFieldId = CP.CustomFieldId
							GROUP BY CP.CustomFieldId
							HAVING COUNT(CP.CustomFieldOptionId)>0) CP
			CROSS APPLY (SELECT * FROM EntityType WHERE Name = C.EntityType) ET
			WHERE ClientId=@ClientId
					AND IsDeleted=0
					
	-- Get list of Entity custom fields values
		SELECT 
		A.EntityId
		,MAX(A.EntityType) EntityType
		,A.CustomFieldId
		,MAX(A.Value) AS Value
		,MAX(A.UniqueId) AS UniqueId
		,MAX(A.Text) AS 'Text'
		,MAX(A.RestrictedText) AS 'RestrictedText'
		,MAX(A.RestrictedValue) AS 'RestrictedValue'
		FROM (
				SELECT CE.CustomFieldId
					,Hireachy.EntityId
					,CE.Value
					,Hireachy.EntityTypeId AS EntityType
					,C.CustomFieldType	
					,Hireachy.EntityType +'_'+CAST(CE.EntityId AS VARCHAR) AS 'UniqueId'
					,CE.UnrestrictedText AS 'Text'
					,RV.Text AS 'RestrictedText'
					,RV.Value AS 'RestrictedValue'
				FROM dbo.fnGetFilterEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds,@TimeFrame,@Isgrid) Hireachy 
				CROSS APPLY (SELECT C.CustomFieldId
									,C.EntityType
									,CT.CustomFieldType
									,C.IsRequired FROM CustomField C
							CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
											WHERE selCol.Item = C.CustomFieldId) selCol
							 CROSS APPLY(	SELECT Name AS 'CustomFieldType' 
											FROM CustomFieldType CT
											WHERE C.CustomFieldTypeId = CT.CustomFieldTypeId) CT
							 WHERE Hireachy.EntityType = C.EntityType AND C.ClientId = @ClientId
									AND C.IsDeleted=0) C
				CROSS APPLY(SELECT   CE.EntityId
										,CE.CustomFieldId
										,CE.Value
										,CE.UnrestrictedText
								FROM [MV].CustomFieldData CE
								WHERE C.CustomFieldId = CE.CustomFieldId
									AND Hireachy.EntityId = CE.EntityId ) CE
				OUTER APPLY (SELECT * FROM @UserRestrictedValues RV WHERE C.CustomFieldId = RV.CustomFieldId) RV
							UNION ALL 
							SELECT C.CustomFieldId,NULL,NULL,ET.EntityTypeId,CT.CustomFieldType
							,NULL AS 'UniqueId',NULL
							,NULL AS 'RestrictedText'
							,NULL AS 'RestrictedValue' FROM CustomField C 
							CROSS APPLY (SELECT * FROM EntityType WHERE Name = C.EntityType) ET  
							CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
											WHERE selCol.Item = C.CustomFieldId) selCol
									CROSS APPLY(SELECT Name AS 'CustomFieldType' FROM CustomFieldType CT 
											WHERE C.CustomFieldTypeId = CT.CustomFieldTypeId)CT 
									WHERE C.ClientId = @ClientId 
									AND C.IsDeleted = 0 
									AND C.EntityType IN('Campaign','Program','Tactic','Lineitem') 
				) A
	GROUP BY A.CustomFieldId, A.EntityId
END
GO
---- Added by devanshi for replacing comma from custom field options value with "|" to resolved issue for multiselect dropdown in grid------
SET ANSI_PADDING ON
GO
IF EXISTS( SELECT * FROM sys.columns  WHERE Name = N'Value' AND Object_ID = Object_ID(N'CustomFieldOption'))
BEGIN
update CustomFieldOption SET Value = REPLACE(Value,',','|') 
  where Value like '%,%'
End

Go
IF NOT EXISTS (SELECT * FROM sysconstraints WHERE OBJECT_NAME(constid) = 'Chk_CommaNotAllow' AND OBJECT_NAME(id) = 'CustomFieldOption')
ALTER TABLE CustomFieldOption
ADD CONSTRAINT Chk_CommaNotAllow CHECK(CHARINDEX(',',Value) = 0) 
Go

-- ===========================Please put your script above this script=============================
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](255) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](255) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'September.2016'
set @version = 'September.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO
