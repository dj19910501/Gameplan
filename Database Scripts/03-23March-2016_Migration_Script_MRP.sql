/* ------------- Start - Related to PL ticket #1449 ------------- 
Created By: Viral
Created Date: 03/16/2016
Description: Add Columns 'LastAutoSyncDate' & 'ForceSyncUser' in table IntegrationInstance
*/

IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'LastAutoSyncDate' AND OBJECT_ID = OBJECT_ID(N'[IntegrationInstance]'))
BEGIN
Alter Table [dbo].[IntegrationInstance] 
DROP COLUMN  LastAutoSyncDate
END 
GO

IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'IsAutoSync' AND OBJECT_ID = OBJECT_ID(N'[IntegrationInstanceLog]'))
BEGIN
Alter Table [dbo].[IntegrationInstanceLog] 
ADD  IsAutoSync bit
END 
GO

IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'ForceSyncUser' AND OBJECT_ID = OBJECT_ID(N'[IntegrationInstance]'))
BEGIN
Alter Table [dbo].[IntegrationInstance] 
ADD  ForceSyncUser uniqueidentifier
END 
GO

/* ------------- Start - Related to PL ticket #2068 ------------- 
Created By: Rahul
Created Date: 03/18/2016
Description: Add Email Template for line Item
*/
GO
DECLARE @NotificationInternalUseOnly nvarchar (max) = 'LineItemOwnerChanged'
DECLARE @Title nvarchar (50) = 'Line Item Owner Changed'
DECLARE @Description nvarchar (50) = 'When owner of Line Item changed'
DECLARE @NotificationType nvarchar (10) = 'CM'
DECLARE @EmailContent nvarchar (max) = 'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following Line Item.<br><br><table><tr><td>Line Item</td><td>:</td><td>[lineitemname]</td></tr><tr><td>Tactic</td><td>:</td><td>[tacticname]</td></tr><tr><td>Program</td><td>:</td><td>[programname]</td></tr><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Hive9 Plan Admin'
DECLARE @isDeleted bit = 0
DECLARE @CreatedBy nvarchar (50) 
DECLARE @ModifiedBy nvarchar (50)
DECLARE @Subject nvarchar (50) = 'Plan : Line Item owner has been changed'

select @CreatedBy = CreatedBy from [Notification] where NotificationId = (select max(NotificationId) from [Notification])
select @ModifiedBy = ModifiedBy from [Notification] where NotificationId = (select max(NotificationId) from [Notification])

IF NOT EXISTS (SELECT * FROM [Notification] WHERE  NotificationInternalUseOnly= @NotificationInternalUseOnly AND isDeleted = @isDeleted)
BEGIN
    INSERT INTO [Notification](NotificationInternalUseOnly,Title,Description,NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,Subject) 
         VALUES(@NotificationInternalUseOnly,@Title,@Description,@NotificationType,@EmailContent,@isDeleted,getdate(),@CreatedBy,getdate(),@ModifiedBy,@Subject)
END
GO

/* ------------- Start------------- 
Created By: Maitri Gandhi
Created Date: 03/19/2016
Description: To change the size of Version table column
*/
IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'Release Name' AND OBJECT_ID = OBJECT_ID(N'[Versioning]'))
BEGIN
ALTER TABLE [Versioning]
ALTER COLUMN [Release Name] nvarchar(255)
END
GO

IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'Version' AND OBJECT_ID = OBJECT_ID(N'[Versioning]'))
BEGIN
ALTER TABLE [Versioning]
ALTER COLUMN [Version] nvarchar(255)
END
GO

IF OBJECT_ID('GetListPlanCampaignProgramTactic', 'P') IS NOT NULL
BEGIN
DROP PROC [dbo].[GetListPlanCampaignProgramTactic]
END
GO

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
SELECT Tactic.*,[Plan].PlanId,[Campaign].PlanCampaignId,[TacticType].[Title] AS 'TacticTypeTtile',[TacticType].[ColorCode],[Plan].[Year] AS 'PlanYear',[Plan].ModelId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId,[Year] From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
OUTER APPLY (SELECT [TacticTypeId],[Title],[ColorCode] FROM [TacticType] WITH (NOLOCK) Where [Tactic].TacticTypeId=TacticType.TacticTypeId AND IsDeleted=0) [TacticType]
WHERE Tactic.IsDeleted=0
END

GO

/* ------------- End - Related to PL ticket #1449 ------------- */

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