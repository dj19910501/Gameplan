-- =============================================
-- Author: Viral 
-- Create date: 03/31/2016
-- Description:	Update Tactic table with IntegrationInstanceTacticId field & insert Create/Update comment to Plan_Campaign_Program_Tactic_Comment table for Tactic & Linked tactic.
-- =============================================
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 03/31/2016 2:49:19 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 03/31/2016 2:49:19 PM ******/
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
	@userId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
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
	-- update IntegrationInstanceTacticId for linked tactic 
	Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
    join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
    where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

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

-- =============================================
-- Author: Nishant 
-- Create date: 04/01/2016
-- Description: Get list of plan, program, tactic
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetListPlanCampaignProgramTactic]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetListPlanCampaignProgramTactic]
GO
-- Created by nishant Sheth
-- Created on :: 03-Feb-2016
-- Desc :: Get list of Plans,Campaigns,Prgorams,Tactics
-- EXEC [dbo].[GetListPlanCampaignProgramTactic] '12912','464EB808-AD1F-4481-9365-6AADA15023BD'
CREATE PROCEDURE [dbo].[GetListPlanCampaignProgramTactic]
 @PlanId nvarchar(max)=NULL,
 @ClientId nvarchar(max)=NULL
AS
BEGIN
SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#tempPlanId') IS NOT NULL
    DROP TABLE #tempPlanId
SELECT val into #tempPlanId FROM dbo.comma_split(@Planid, ',')

IF OBJECT_ID('tempdb..#tempClientId') IS NOT NULL
    DROP TABLE #tempClientId
SELECT val into #tempClientId FROM dbo.comma_split(@ClientId, ',')

-- Plan Details
SELECT * FROM [Plan] AS [Plan] WITH (NOLOCK) 
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)
 AND [Plan].IsDeleted=0 

-- Campaign Details
SELECT Campaign.* FROM Plan_Campaign Campaign
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
WHERE Campaign.IsDeleted=0

-- Program Details
SELECT Program.*,[Plan].PlanId FROM Plan_Campaign_Program Program WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
WHERE Program.IsDeleted=0

-- Tactic Details
SELECT Tactic.*,[Plan].PlanId,[Campaign].PlanCampaignId,[TacticType].[Title] AS 'TacticTypeTtile',[TacticType].[ColorCode],[Plan].[Year] AS 'PlanYear',[Plan].ModelId, 
[Campaign].Title AS 'CampaignTitle',[Program].Title AS 'ProgramTitle',[Plan].Title AS 'PlanTitle', [Stage].Title AS 'StageTitle',[Plan].[Status] AS 'PlanStatus'
FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId,[Year],Title,[Status] From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
OUTER APPLY (SELECT [TacticTypeId],[Title],[ColorCode] FROM [TacticType] WITH (NOLOCK) Where [Tactic].TacticTypeId=TacticType.TacticTypeId AND IsDeleted=0) [TacticType]
OUTER APPLY (SELECT [StageId],[Title] FROM Stage WHERE [Tactic].StageId=Stage.StageId) Stage
WHERE Tactic.IsDeleted=0
END

GO

-- =============================================
-- Author: Nishant 
-- Create date: 04/01/2016
-- Description: Get list Custom fields and Custom field entity list
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomFieldEntityList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomFieldEntityList]
GO
-- Created by nishant Sheth
-- Created on :: 03-Mar-2016
-- Desc :: Get list of entity values
-- EXEC GetCustomFieldEntityList 42,'Tactic','464eb808-ad1f-4481-9365-6aada15023bd'
CREATE PROCEDURE [dbo].[GetCustomFieldEntityList]
@CustomTypeId int = null
,@EntityType nvarchar(50) = ''
,@ClientId nvarchar(250)=null
AS
BEGIN
SET NOCOUNT ON;

SELECT CustomField.* FROM CustomField (NOLOCK) WHERE 
IsDeleted=0
AND CustomFieldId=CASE WHEN @CustomTypeId IS NULL THEN CustomFieldId ELSE @CustomTypeId END
AND EntityType= CASE WHEN @EntityType ='' THEN EntityType ELSE @EntityType END
AND ClientId= CASE WHEN @ClientId IS NULL THEN ClientId ELSE @ClientId END


SELECT CustomField_Entity.EntityId,CustomField_Entity.CustomFieldId,CustomField_Entity.Value
,CustomField_Entity.CreatedBy
,CustomField_Entity.CustomFieldEntityId
 FROM CustomField_Entity (NOLOCK) 
CROSS APPLY(
SELECT CustomField.CustomFieldId FROM CustomField (NOLOCK) WHERE 
IsDeleted=0
AND CustomFieldId=CASE WHEN @CustomTypeId IS NULL THEN CustomFieldId ELSE @CustomTypeId END
AND EntityType = CASE WHEN @EntityType ='' THEN EntityType ELSE @EntityType END
AND ClientId = CASE WHEN @ClientId IS NULL THEN ClientId ELSE @ClientId END
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
) CustomField 

END

GO

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
set @release = 'Apr06.2016'
set @version = 'Apr06.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO