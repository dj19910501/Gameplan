/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment_API]    Script Date: 06/21/2016 1:27:17 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment_API]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment_API]    Script Date: 06/21/2016 1:27:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment_API]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API] AS' 
END
GO

ALTER PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
	@strCreatedTacIds nvarchar(max)='',
	@strUpdatedTacIds nvarchar(max)='',
	@strCrtCampaignIds nvarchar(max)='',
	@strUpdCampaignIds nvarchar(max)='',
	@strCrtProgramIds nvarchar(max)='',
	@strUpdProgramIds nvarchar(max)='',
	@strCrtImprvmntTacIds nvarchar(max)='',
	@strUpdImprvmntTacIds nvarchar(max)='',
	@isAutoSync bit='0',
	@userId uniqueidentifier,
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
		Insert Into Plan_Campaign_Program_Tactic_Comment
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@TacticSyncedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@TacticUpdatedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Comment for Plan Campaign
		IF( (@strCrtCampaignIds <>'') OR (@strUpdCampaignIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment
			SELECT null,@CampaignSyncedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strCrtCampaignIds, ','))
			UNION
			SELECT null,@CampaignUpdatedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strUpdCampaignIds, ','))
		END

		-- Insert Comment for Plan Program
		IF( (@strCrtProgramIds <>'') OR (@strUpdProgramIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment
			SELECT null,@ProgramSyncedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strCrtProgramIds, ','))
			UNION
			SELECT null,@ProgramUpdatedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strUpdProgramIds, ','))
		END

		-- Insert Comment for Improvement Tactic
		IF( (@strCrtImprvmntTacIds <>'') OR (@strUpdImprvmntTacIds <>'') )
		BEGIN
			INSERT Into Plan_Improvement_Campaign_Program_Tactic_Comment
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
