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
-- =============================================
-- Author: Rahul
-- Create date: 04/05/2016
-- Description: Get list of Data for CSV
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGridDataList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGridDataList]
GO

-- =============================================
-- Author:<Author,Akashdeep>
-- Create date: <Create Date,21-Mar-2016,>
-- Description:	<Description,,>
--
--Exec spGridDataList '12950'
-- =============================================
CREATE PROCEDURE  [dbo].[spGridDataList] 
	-- Add the parameters for the stored procedure here
	@PlanId int	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	
	SET NOCOUNT ON;

SELECT [Section]='Plan',[Plan].PlanId As Id,null As ParentId,[Plan].Title As 'Plan',null AS 'Campaign', null AS 'Program', null AS 'Tactic', null AS 'LineItem',
StartDate = Convert(nvarchar(10),Campaign.MinStartDate,101),
EndDate = Convert(nvarchar(10),Campaign.MaxEndDate,101),null as 'Type',[Plan].CreatedBy AS 'CreatedBy',null AS 'Owner',[Plan].Budget, 
PlanCost=(
	select sum(cost)  from Plan_Campaign_Program_Tactic_LineItem where PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where PlanCampaignId in (select PlanCampaignId from Plan_Campaign where PlanId = @PlanId and IsDeleted=0) and IsDeleted=0) and IsDeleted=0) and IsDeleted=0
),null As 'StageId',null AS 'TargetStageValue',null AS 'MQLS',null AS 'Revenue',null As 'ExternalName',null As EloquaId,null AS SFDCId,[Plan].ModelId AS 'ModelId'
FROM [Plan] AS [Plan] WITH (NOLOCK) 
CROSS APPLY (SELECT min(StartDate) as 'MinStartDate',max(EndDate) as 'MaxEndDate' FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
WHERE [Plan].PlanId IN (@PlanId)
 AND [Plan].IsDeleted=0 

union 
select [Section]='Campaign', Campaign.PlanCampaignId,Campaign.PlanId as ParentId, [Plan].Title AS 'Plan',Campaign.Title AS 'Campaign', null AS 'Program', null AS 'Tactic', null AS 'LineItem',Convert(nvarchar(10),StartDate,101),Convert(nvarchar(10),EndDate,101),null as 'Type',
Campaign.CreatedBy AS 'CreatedBy',null AS 'Owner',null,null,null As 'StageId',null AS 'TargetStageValue',null AS 'MQLS',null AS 'Revenue',null As 'ExternalName',null As EloquaId,IntegrationInstanceCampaignId AS SFDCId,null AS 'ModelId' from Plan_Campaign Campaign
CROSS APPLY (SELECT * From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
WHERE Campaign.IsDeleted=0

union
SELECT [Section]='Program', Program.PlanProgramId AS Id,Program.PlanCampaignId as ParentId,[Plan].Title AS 'Plan',Campaign.Title AS 'Campaign',Program.Title AS 'Program',null AS 'Tactic', null AS 'LineItem', 
Convert(nvarchar(10),Program.StartDate,101),Convert(nvarchar(10),Program.EndDate,101),null as 'Type',Program.CreatedBy AS 'CreatedBy',null AS 'Owner',null,null,null As 'StageId',null AS 'TargetStageValue',null AS 'MQLS',null AS 'Revenue',null As 'ExternalName',null As EloquaId,IntegrationInstanceProgramId AS SFDCId,null AS 'ModelId' FROM Plan_Campaign_Program Program WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
WHERE Program.IsDeleted=0
union

SELECT [Section]='Tactic',Tactic.PlanTacticId AS Id,Tactic.PlanProgramId AS ParentId,[Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',Program.Title AS 'Program',Tactic.Title AS 'Tactic',null AS 'LineItem',
Convert(nvarchar(10),StartDate,101),Convert(nvarchar(10),EndDate,101),[TacticType].[Title] AS 'Type',[Tactic].CreatedBy AS 'CreatedBy',null AS 'Owner',Tactic.ProjectedStageValue,Tactic.Cost,
[Tactic].StageId As 'StageId',CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageValue',null AS 'MQLS',null AS 'Revenue',Tactic.TacticCustomName As 'ExternalName',Tactic.IntegrationInstanceEloquaId AS EloquaId,Tactic.IntegrationInstanceTacticId AS SFDCId,null AS 'ModelId'
FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
OUTER APPLY (SELECT [TacticTypeId],[Title] FROM [TacticType] WITH (NOLOCK) Where [Tactic].TacticTypeId=TacticType.TacticTypeId AND IsDeleted=0) [TacticType]
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
WHERE Tactic.IsDeleted=0

union
SELECT [Section]='LineItem', LineItem.PlanLineItemId AS Id,LineItem.PlanTacticId AS ParentId,[Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',Program.Title AS 'Program',Tactic.Title AS 'Tactic'
,LineItem.Title AS 'LineItem',Convert(nvarchar(10),StartDate,101),Convert(nvarchar(10),EndDate,101),LineItemType.Title as 'Type',[LineItem].CreatedBy AS 'CreatedBy',null AS 'Owner',null,LineItem.Cost,null As 'StageId',
null AS 'TargetStageValue',null AS 'MQLS',null AS 'Revenue',null As 'ExternalName',null As EloquaId,null AS SFDCId,null AS 'ModelId'
FROM Plan_Campaign_Program_Tactic_LineItem AS LineItem WITH (NOLOCK) 
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanTacticId=LineItem.PlanTacticId AND Tactic.IsDeleted=0) Tactic
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId,[Year],Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
OUTER APPLY (SELECT [LineItemTypeId],[Title] FROM [LineItemType] WITH (NOLOCK) Where [LineItem].LineItemTypeId=[LineItemType].LineItemTypeId AND IsDeleted=0) [LineItemType]
WHERE LineItem.IsDeleted=0
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCustomfieldData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCustomfieldData] 
GO

-- =============================================
-- Author:		<Author,,Akashdeep>
-- Create date: <Create Date,04-Apr-2016,>
-- Description:	<Description,,>
--
--Exec spCustomfieldData '12950','464EB808-AD1F-4481-9365-6AADA15023BD'
-- =============================================
CREATE PROCEDURE  [dbo].[spCustomfieldData] 
	-- Add the parameters for the stored procedure here
	@PlanId nvarchar(max),
	@ClientId varchar(max)

	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	
	SET NOCOUNT ON;	

select * into TempData from(

select Distinct(a.Name) as [Text],EntityType,a.CustomFieldId,c.EntityId,c.Value as CustomValue,b.Name, CASE b.Name WHEN 'DropDownList' THEN

(select DropDownText from  dbo.DDLValueName(c.Value))
 ELSE 
c.Value  END as DDLText
 from CustomField as a
 inner join CustomFieldType as b on a.CustomFieldTypeId = b.CustomFieldTypeId
 right join CustomField_Entity as c on a.CustomFieldId = c.CustomFieldId and EntityId in(


select Campaign.PlanCampaignId from Plan_Campaign Campaign
CROSS APPLY (SELECT * From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
WHERE Campaign.IsDeleted=0

union
SELECT Program.PlanProgramId AS Id FROM Plan_Campaign_Program Program WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
WHERE Program.IsDeleted=0
union

SELECT Tactic.PlanTacticId AS Id FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
OUTER APPLY (SELECT [TacticTypeId],[Title] FROM [TacticType] WITH (NOLOCK) Where [Tactic].TacticTypeId=TacticType.TacticTypeId AND IsDeleted=0) [TacticType]
WHERE Tactic.IsDeleted=0

union
SELECT  LineItem.PlanLineItemId AS Id FROM Plan_Campaign_Program_Tactic_LineItem AS LineItem WITH (NOLOCK) 
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanTacticId=LineItem.PlanTacticId AND Tactic.IsDeleted=0) Tactic
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId,[Year],Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (@PlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
OUTER APPLY (SELECT [LineItemTypeId],[Title] FROM [LineItemType] WITH (NOLOCK) Where [LineItem].LineItemTypeId=[LineItemType].LineItemTypeId AND IsDeleted=0) [LineItemType]
WHERE LineItem.IsDeleted=0
 
 )
 where a.ClientId=@ClientId and a.IsDeleted=0 --and EntityType in ('Tactic','Campaign','Program')
 
 
 
 ) as Temp

 select  EntityId,      

       (SELECT STUFF((SELECT ',' + [Text]
                      FROM TempData tn2 WHERE EntityId = t.EntityId 
                                     
            FOR XML PATH('')) ,1,1,'')) as Header,
		(SELECT STUFF((SELECT ',' + [DDLText]
                      FROM TempData tn2 WHERE EntityId = t.EntityId 
                                     
            FOR XML PATH('')) ,1,1,'')) as Value,
	EntityType
       
from TempData t
GROUP BY t.EntityId,t.EntityType
DROP TABLE TempData
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