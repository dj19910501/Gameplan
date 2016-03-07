
DECLARE @ClientId UNIQUEIDENTIFIER;

SET @ClientId = 'C251AB18-0683-4D1D-9F1E-06709D59FD53';  -- Set ClientId for Zebra.
IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Campaign_Program_Tactic_BKP_15Feb2015'))
BEGIN
   SELECT * INTO Plan_Campaign_Program_Tactic_BKP_15Feb2015 FROM Plan_Campaign_Program_Tactic;
END

;WITH RawTable AS (
	SELECT 
		T.PlanTacticId, T.Title Tactic, T.IsDeployedToIntegration, T.IsSyncSalesForce, T.IsSyncEloqua,M.IntegrationInstanceId,M.IntegrationInstanceEloquaId,M.IntegrationInstanceIdProjMgmt
	FROM Plan_Campaign_Program_Tactic T
		INNER JOIN Plan_Campaign_Program PP ON PP.PlanProgramId = T.PlanProgramId AND PP.IsDeleted = 0
		INNER JOIN Plan_Campaign C ON C.PlanCampaignId = PP.PlanCampaignId AND C.IsDeleted = 0
		INNER JOIN [Plan] P ON P.PlanId = C.PlanId AND P.IsDeleted = 0 --AND P.Status = 'Published'
		INNER JOIN Model M ON M.ModelId = P.ModelId AND M.IsDeleted = 0 AND (M.IntegrationInstanceId IS NOT NULL OR M.IntegrationInstanceEloquaId IS NOT NULL OR M.IntegrationInstanceIdProjMgmt IS NOT NULL) AND M.ClientId = @ClientId
	WHERE T.IsDeleted = 0
	AND T.IsDeployedToIntegration = 1
)
UPDATE Plan_Campaign_Program_Tactic 
	SET 
		IsSyncSalesForce	= ISNULL(RawTable.IntegrationInstanceId,0),
		IsSyncEloqua		= ISNULL(RawTable.IntegrationInstanceEloquaId,0),
		IsSyncWorkFront		= ISNULL(RawTable.IntegrationInstanceIdProjMgmt,0)
FROM Plan_Campaign_Program_Tactic T
INNER JOIN RawTable ON RawTable.PlanTacticId = T.PlanTacticId

GO
IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[comma_split]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
 DROP FUNCTION [dbo].[comma_split]
 GO
-- Create By Nishant Sheth
-- Created on : 03-Feb-2016
-- Desc ::  To split comma sperate value
CREATE function [dbo].[comma_split]
(
@param nvarchar(max), 
@delimiter char(1)
)
returns @t table (val nvarchar(max), seq int)
as
begin
set @param += @delimiter

;with a as
(
select cast(1 as bigint) f, charindex(@delimiter, @param) t, 1 seq
union all
select t + 1, charindex(@delimiter, @param, t + 1), seq + 1
from a
where charindex(@delimiter, @param, t + 1) > 0
)
insert @t
select substring(@param, f, t - f), seq from a
option (maxrecursion 0)
return
end
GO

-- Start GetCustomFieldEntityList
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetCustomFieldEntityList]') AND type in (N'P', N'PC'))
DROP PROCEDURE GetCustomFieldEntityList
GO
-- Created by nishant Sheth
-- Created on :: 03-Mar-2016
-- Desc :: Get list of entity values
CREATE PROCEDURE GetCustomFieldEntityList
@customfieldId nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;
SELECT * FROM CustomField_Entity Where CustomFieldId in (SELECT val FROM dbo.comma_split(@customfieldId, ','))
END

GO
-- END GetCustomFieldEntityList

-- Start GetLineItemList
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetLineItemList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetLineItemList]
GO
-- Created by nishant Sheth
-- Created on :: 03-Feb-2016
-- Desc :: Get list of line item with plan ids
CREATE PROCEDURE [dbo].[GetLineItemList]
@PlanId nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;
SELECT LineItem.* FROM Plan_Campaign_Program_Tactic_LineItem LineItem WITH (NOLOCK)
CROSS APPLY (SELECT PlanProgramId,PlanTacticId FROM Plan_Campaign_Program_Tactic  AS Tactic WITH (NOLOCK) WHERE Tactic.PlanTacticId=LineItem.PlanTacticId AND Tactic.IsDeleted=0) Tactic
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId iN ((SELECT val FROM dbo.comma_split(@Planid, ','))) AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
WHERE 
LineItem.IsDeleted=0
END
GO
-- END GetLineItemList

-- Start GetListPlanCampaignProgramTactic

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetListPlanCampaignProgramTactic]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetListPlanCampaignProgramTactic]
GO
-- Created by nishant Sheth
-- Created on :: 03-Feb-2016
-- Desc :: Get list of Plans,Campaigns,Prgorams,Tactics
CREATE PROCEDURE [dbo].[GetListPlanCampaignProgramTactic]
@PlanId nvarchar(max),
@ClientId nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;
-- Plan Details
SELECT * FROM [Plan] AS [Plan] WITH (NOLOCK) 
CROSS APPLY (SELECT ModelId,ClientId From [Model] WHERE [Model].ModelId = [Plan].ModelId AND ClientId=(SELECT val FROM dbo.comma_split(@ClientId, ','))) [Model]
WHERE [Plan].PlanId IN (SELECT val FROM dbo.comma_split(@Planid, ',')) AND [Plan].IsDeleted=0

-- Campaign Details
SELECT Campaign.* FROM Plan_Campaign Campaign
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val FROM dbo.comma_split(@Planid, ',')) AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model] WHERE [Model].ModelId = [Plan].ModelId AND ClientId=(SELECT val FROM dbo.comma_split(@ClientId, ','))) [Model]
WHERE Campaign.IsDeleted=0

-- Program Details
SELECT Program.*,[Plan].PlanId FROM Plan_Campaign_Program Program WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val FROM dbo.comma_split(@Planid, ',')) AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model] WHERE [Model].ModelId = [Plan].ModelId AND ClientId=(SELECT val FROM dbo.comma_split(@ClientId, ','))) [Model]
WHERE Program.IsDeleted=0

-- Tactic Details
SELECT Tactic.*,[Plan].PlanId  FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val FROM dbo.comma_split(@Planid, ',')) AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model] WHERE [Model].ModelId = [Plan].ModelId AND ClientId=(SELECT val FROM dbo.comma_split(@ClientId, ','))) [Model]
WHERE Tactic.IsDeleted=0
END
GO
-- END GetListPlanCampaignProgramTactic

-- Start GetTacticLineItemLis

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetTacticLineItemList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetTacticLineItemList]
GO
-- Created by nishant Sheth
-- Created on :: 03-Mar-2016
-- Desc :: Get list of line item
CREATE PROCEDURE [dbo].[GetTacticLineItemList]
@tacticId nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;
SELECT * FROM Plan_Campaign_Program_Tactic_LineItem Where IsDeleted=0 AND PlanTacticId in (SELECT val FROM dbo.comma_split(@tacticId, ','))
END
GO
-- END GetTacticLineItemLis

-- Start GetTacticTypeList

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetTacticTypeList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [GetTacticTypeList]
GO
-- Created BY Nisahnt Sheth
-- Desc :: get list of tactic type 
-- Created Date : 04-Mar-2016
CREATE PROCEDURE [dbo].[GetTacticTypeList]
@TacticIds nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;
SELECT [TacticType].Title,[TacticType].TacticTypeId,Count([TacticType].TacticTypeId) As Number FROM TacticType WITH (NOLOCK) 
CROSS APPLY (SELECT PlanTacticId,TacticTypeId FROM Plan_Campaign_Program_Tactic As Tactic WITH (NOLOCK)
WHERE TacticType.TacticTypeId=Tactic.TacticTypeId AND PlanTacticId in (Select val From dbo.comma_split(@TacticIds,',')) AND IsDeleted=0) Tactic
WHERE TacticType.IsDeleted=0
GROUP BY [TacticType].TacticTypeId,[TacticType].Title
ORDER BY [TacticType].Title
END
GO
-- END GetTacticTypeList


-- Start spViewByDropDownList

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[spViewByDropDownList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [spViewByDropDownList]
GO

-- =============================================
-- Author:		<Author,,Akashdeep>
-- Create date: <Create Date,03-March-2016,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE  [dbo].[spViewByDropDownList] 
	-- Add the parameters for the stored procedure here
	@PlanId nvarchar(max),
	@ClientId nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

select Distinct(a.Name) as [Text],EntityType +'Custom'+ Cast(a.CustomFieldId as nvarchar(50)) as Value
 from CustomField as a
 left join CustomFieldType as b on a.CustomFieldTypeId = b.CustomFieldTypeId
 right join CustomField_Entity as c on a.CustomFieldId = c.CustomFieldId and EntityId in(
	
	select PlanCampaignId from Plan_Campaign  where IsDeleted=0 and PlanId in (
		SELECT val FROM dbo.comma_split(@Planid, ','))
	union 
	select PlanProgramId from Plan_Campaign_Program where IsDeleted=0 and PlanCampaignId  in(
	select PlanCampaignId from Plan_Campaign  where IsDeleted=0 and PlanId in (
		SELECT val FROM dbo.comma_split(@Planid, ',')))
	union
	select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted=0 and PlanProgramId  in(
	select PlanProgramId from Plan_Campaign_Program where IsDeleted=0 and PlanCampaignId  in(
	select PlanCampaignId from Plan_Campaign  where IsDeleted=0 and PlanId in (
		SELECT val FROM dbo.comma_split(@Planid, ','))))
 )
  
 where a.ClientId=@ClientId and a.IsDeleted=0 and a.IsDisplayForFilter=1 and b.Name='DropDownList' and EntityType in ('Tactic','Campaign','Program')

 order by Value desc

 END
 GO
 -- END spViewByDropDownList

 -- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](50) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](50) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(10)
declare @release nvarchar(10)
set @release = 'Mar.2016'
set @version = 'Mar.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO