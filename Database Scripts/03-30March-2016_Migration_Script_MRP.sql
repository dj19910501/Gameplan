/* ------------- Start - Related to PL ticket #2083 & #2084 ------------- 
Created By: Brad Gray
Created Date: 03/26/2016
Description: Add Custom Fields as Read-Only
*/

/*  Change the ClientId and UserId as appropriate */
Declare @ClientId  uniqueidentifier = '55B19C77-3356-4B22-8FC2-8E026138BEC2';
Declare @UserId  uniqueidentifier = '962456D6-8868-4B66-88CC-883619C4E340';
Declare @Date  datetime = GETDATE();

if not exists (Select * from [dbo].[CustomField] where ClientId = @ClientId and Name = 'Creative Project Number')
begin
 insert into [dbo].[CustomField]
 (Name, CustomFieldTypeId, IsRequired, EntityType, ClientId, IsDeleted, CreatedDate, CreatedBy, ModifiedDate,ModifiedBy, IsDisplayForFilter,
 IsDefault, IsGet)
  values('Creative Project Number', 1, 0, 'Tactic', @ClientId, 0, 
	@Date, @UserId, null, null, 0, 0, 1)
end


if not exists (Select * from [dbo].[CustomField] where ClientId = @ClientId and Name = 'Creative Cost')
begin
 insert into [dbo].[CustomField]
 (Name, CustomFieldTypeId, IsRequired, EntityType, ClientId, IsDeleted, CreatedDate, CreatedBy, ModifiedDate,ModifiedBy, IsDisplayForFilter,
 IsDefault, IsGet)
  values('Creative Cost', 1, 0, 'Tactic', @ClientId, 0, 
	@Date, @UserId, null, null, 0, 0, 1)
end
/* ------------- End - Related to PL ticket #2083 & #2084 -------------*/ 

/* ------------- Start - Related to PL ticket #2070 ------------- 
Created By: Brad Gray
Created Date: 03/20/2016
Description: Add Fields for Workfront sync mapping
*/

DECLARE @inst as int = (SELECT IntegrationTypeId FROM [dbo].[IntegrationType] where Title = 'WorkFront')

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'StartDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program_Tactic', 'StartDate', 'Tactic Start Date', 0,0,0)
 
  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'EndDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program_Tactic', 'EndDate', 'Tactic End Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ParentCampaign')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'ParentCampaign', 'Parent Campaign', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ParentProgram')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ParentProgram', 'Parent Program', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ProgramOwner')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ProgramOwner', 'Program Owner', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignOwner')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'CampaignOwner', 'Campaign Owner', 0,0,0)
     
  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignStartDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'CampaignStartDate', 'Campaign Start Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignEndDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'Campaign End Date', 'Campaign End Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ProgramStartDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ProgramStartDate', 'Program Start Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ProgramEndDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ProgramEndDate', 'Program End Date', 0,0,0)
/* ------------- End - Related to PL ticket #2070 -------------*/


IF OBJECT_ID('GetListPlanCampaignProgramTactic', 'P') IS NOT NULL
BEGIN
DROP PROC [dbo].[GetListPlanCampaignProgramTactic]
END

GO
-- Created by nishant Sheth
-- Created on :: 03-Feb-2016
-- Desc :: Get list of Plans,Campaigns,Prgorams,Tactics
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
[Campaign].Title AS 'CampaignTitle',[Program].Title AS 'ProgramTitle',[Plan].Title AS 'PlanTitle'
FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId,[Year],Title From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
OUTER APPLY (SELECT [TacticTypeId],[Title],[ColorCode] FROM [TacticType] WITH (NOLOCK) Where [Tactic].TacticTypeId=TacticType.TacticTypeId AND IsDeleted=0) [TacticType]
WHERE Tactic.IsDeleted=0
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
set @release = 'Mar30.2016'
set @version = 'Mar30.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO