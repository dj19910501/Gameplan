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
-- Author: Nishant 
-- Create date: 04/06/2016
-- Description: Get list Custom fields and Custom field entity list
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 4/6/2016 11:04:51 AM ******/

-- EXEC ExportToCSV 14071,'E5EF88EB-4748-4436-9ACC-ABA6B2C5F6A9','16114','Created,Submitted,Approved,In-Progress,Complete,Declined','34','1212','c251ab18-0683-4d1d-9f1e-06709d59fd53'
CREATE PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@OwnerId nvarchar(max)=''
,@TacticTypeId nvarchar(max)=''
,@Status nvarchar(max)=''
,@CustomFields nvarchar(max)=''
,@CustomFieldOptionIds nvarchar(max)=''
,@ClientId nvarchar(max)=''
AS
BEGIN

SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..##tblCustomData') IS NOT NULL
   DROP TABLE ##tblCustomData

IF OBJECT_ID('tempdb..##tblCsvData') IS NOT NULL
	DROP TABLE ##tblCsvData 

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..##tblStuffData') IS NOT NULL
	DROP TABLE ##tblStuffData 

IF OBJECT_ID('tempdb..##tblCSVOriginalData') IS NOT NULL
	DROP TABLE ##tblCSVOriginalData
	
SELECT ROW_NUMBER() OVER(ORDER BY CustomFieldEntityId) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',NULL As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' ) [CustomField]
WHERE [Plan].PlanId IN (@PlanId)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField_Entity.CustomFieldId = CustomField_Entity.CustomFieldEntityId AND CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' ) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE [Plan].PlanId IN(@PlanId)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost',null AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.CustomFieldId=CustomField_Entity.CustomFieldId AND IsDeleted=0 AND CustomField.ClientId=@ClientId AND CustomField.EntityType='Program') [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE [Plan].PlanId IN(@PlanId)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',[Tactic].Cost As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.CustomFieldId=CustomField_Entity.CustomFieldId AND IsDeleted=0 AND CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic') [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE [Plan].PlanId IN(@PlanId)
--WHERE EntityId=PlanCampaignId
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',[lineitem].Cost As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.CustomFieldId=CustomField_Entity.CustomFieldId AND IsDeleted=0 AND CustomField.ClientId=@ClientId  AND CustomField.EntityType='Lineitem') [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE [Plan].PlanId IN(@PlanId)
) tblUnion

--Select * FROM #tblPivot 

DECLARE   @ConcatString NVARCHAR(Max)=''
--SELECT   @ConcatString = COALESCE(@ConcatString + ', ', '') + ColName FROM #tblPivot 
--SELECT   @ConcatString AS Fruits

Declare @RowCount int , @Count int=1
SELECT ROW_NUMBER() OVER(ORDER BY ColName) AS ROWNUM,* into #tblColName FROM (SELECT Distinct ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName
SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';


While @Count<=@RowCount
BEGIN
IF(@Count=@RowCount)
BEGIN
SET @Delimeter='';
END

SET @ConcatString= @ConcatString +'['+ (SELECT ColName FROM #tblColName WHERE ROWNUM=@Count)+']'+@Delimeter ;
SET @Count=@Count+1;
END

DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT * into ##tblCustomData FROM
    (SELECT     
        ROW_NUMBER() OVER(ORDER BY ColName) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		Type,
		SFDCId,
		EloquaId,
        CustomFieldEntityId, 
		CustomFieldId,
		CreatedBy,
		TargetStageGoal,
		ModelId,
		MQL,
		Revenue,
		Owner,
		ExternalName,
              EntityId,
			  EntityType,
			  ParentId,
			  ColName,
              CONVERT(NVARCHAR(MAX),Value) AS Value
    FROM #tblPivot WITH (NOLOCK))X
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P 
	'
	
	EXEC SP_EXECUTESQL @query	
	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
	
	
	select ROW_NUMBER() OVER(ORDER BY name) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..##tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';
	
	While @Count<=@RowCount
	BEGIN
		IF(@Count=@RowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tbldynamicColumns WHERE ROWNUM=@Count)
		IF(@Val!='CustomFieldId')
		BEGIN
		IF(@Val ='CustomFieldEntityId' OR @Val='EntityId' OR @Val='EndDate' OR @Val='StartDate' OR @Val='Plan' OR @Val='Campaign' OR @Val='Program' OR @Val='Tactic' OR @Val='LineItem' OR @Val='EntityType' OR @Val='ROWNUM' OR @Val='PlannedCost' OR @Val='Section' OR @Val='Type' OR @Val='EloquaId' OR @Val='SFDCId' OR @Val='ParentId' OR @Val='CreatedBy' OR @Val='TargetStageGoal' OR @Val='ModelId' OR @Val='ExternalName' OR @val='MQL' OR @val='Revenue' OR @Val='Owner')
		BEGIN
			IF @Val!='EndDate'
			BEGIN 
				SET @SqlStuff= @SqlStuff +'MIN(t.['+ @Val+']) AS ['+ @Val+'] '+@Delimeter ;
			END
			ELSE 
			BEGIN
				SET @SqlStuff= @SqlStuff +'MAX(t.['+ @Val+']) AS ['+ @Val+'] '+@Delimeter ;
			END
		END
		ELSE
		BEGIN
			SET @SqlStuff= @SqlStuff +'(SELECT STUFF((SELECT '';'' + CONVERT(nvarchar(max),['+@Val+']) FROM ##tblCustomData tn2 WHERE EntityId = t.EntityId AND EntityType=t.EntityType AND CustomFieldId=t.CustomFieldId FOR XML PATH('''')) ,1,1,'''')) AS ['+@Val+'] '+@Delimeter;
		END

		END
		SET @Count=@Count+1;
	END
	SET @SqlStuff+=' INTO ##tblStuffData FROM ##tblCustomData t WITH (NOLOCK) GROUP BY t.EntityId, t.EntityType, t.CustomFieldId'
	
	PRINT(@SqlStuff)
	EXEC(@SqlStuff)
	
	SET @Count=1
	
	SET @ConcatString= ''
	SET @Delimeter=',';
	While @Count<=@RowCount
	BEGIN
		IF(@Count=@RowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tbldynamicColumns WHERE ROWNUM=@Count)
		IF(@Val!='CustomFieldId')
		BEGIN
		IF(@Val ='CustomFieldEntityId' OR @Val='EntityId' OR @Val='EndDate' OR @Val='StartDate' OR @Val='Plan' OR @Val='Campaign' OR @Val='Program' OR @Val='Tactic' OR @Val='LineItem' OR @Val='EntityType' OR @Val='ROWNUM' OR @Val='PlannedCost' OR @Val='Section' OR @Val='Type' OR @Val='EloquaId' OR @Val='SFDCId' OR @Val='ParentId' OR @Val='CreatedBy' OR @Val='TargetStageGoal' OR @Val='ModelId' OR @Val='ExternalName' OR @val='MQL' OR @val='Revenue' OR @Val='Owner' OR @Val='CustomFieldId')
		BEGIN
			SET @ConcatString= @ConcatString +'MIN(M.['+ @Val+']) AS ['+ @Val+'] '+@Delimeter ;
		END
		ELSE
		BEGIN
			SET @ConcatString= @ConcatString +'MIN(M.['+ @Val+']) AS ['+@Val+'] '+@Delimeter ;
		END
		END
		SET @Count=@Count+1;
	END

	--SELECT @ConcatString 
	DEClARE @SqlGroup nvarchar(max)=''
	SET @SqlGroup='SELECT '+@ConcatString+'  FROM ##tblStuffData M WITH (NOLOCK) JOIN ##tblStuffData C WITH (NOLOCK) ON M.EntityId=C.EntityId
	GROUP BY M.EntityID,M.EntityType'
	EXEC(@SqlGroup)


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