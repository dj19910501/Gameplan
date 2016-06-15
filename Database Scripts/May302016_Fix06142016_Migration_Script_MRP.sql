----===============================================================================================================

-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================

/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 05/30/2016 7:56:15 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 05/30/2016 7:56:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment] AS' 
END
GO

ALTER PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
	@strCreatedTacIds nvarchar(max),
	@strUpdatedTacIds nvarchar(max),
	@strUpdateComment nvarchar(max),
	@strCreateComment nvarchar(max),
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
		SElect PlanTacticId,@strCreateComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@strUpdateComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
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

GO
-- ========================================================================================================

-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
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
set @release = 'May302016_Fix06142016.2016'
set @version = 'May302016_Fix06142016.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO