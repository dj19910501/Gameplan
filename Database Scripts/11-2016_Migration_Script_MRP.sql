/****** Object:  View [dbo].[CampaignDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[CampaignDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[CampaignDetail];
GO

CREATE VIEW [dbo].[CampaignDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, C.Title, C.Description, C.StartDate, C.EndDate, C.CreatedDate, C.ModifiedDate, C.IsDeleted, C.Status, C.IsDeployedToIntegration, C.IntegrationInstanceCampaignId, 
                  C.LastSyncDate, C.CampaignBudget, C.Abbreviation, C.IntegrationWorkFrontProgramID, C.CreatedBy, C.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId

GO
/****** Object:  View [dbo].[LineItemDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[LineItemDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[LineItemDetail];
GO

CREATE VIEW [dbo].[LineItemDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, L.PlanLineItemId, L.PlanTacticId, L.LineItemTypeId, L.Title, L.Description, L.StartDate, L.EndDate, L.Cost, L.IsDeleted, L.CreatedDate, L.ModifiedDate, 
                  L.LinkedLineItemId, L.ModifiedBy, L.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic_LineItem AS L ON L.PlanTacticId = T.PlanTacticId

GO
/****** Object:  View [dbo].[PlanDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[PlanDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[PlanDetail];
GO

CREATE VIEW [dbo].[PlanDetail]
AS
SELECT M.ClientId, P.PlanId, P.ModelId, P.Title, P.Version, P.Description, P.Budget, P.Status, P.IsActive, P.IsDeleted, P.CreatedDate, P.ModifiedDate, P.Year, P.GoalType, P.GoalValue, P.AllocatedBy, P.EloquaFolderPath, 
                  P.DependencyDate, P.CreatedBy, P.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId

GO
/****** Object:  View [dbo].[ProgramDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[ProgramDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[ProgramDetail];
GO

CREATE VIEW [dbo].[ProgramDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, Pr.PlanCampaignId AS Expr1, Pr.Title, Pr.Description, Pr.StartDate, Pr.EndDate, Pr.CreatedDate, Pr.ModifiedDate, Pr.IsDeleted, Pr.Status, 
                  Pr.IsDeployedToIntegration, Pr.IntegrationInstanceProgramId, Pr.LastSyncDate, Pr.ProgramBudget, Pr.Abbreviation, Pr.CreatedBy, Pr.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId

GO
/****** Object:  View [dbo].[TacticDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[TacticDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[TacticDetail];
GO

CREATE VIEW [dbo].[TacticDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, T.PlanTacticId, T.PlanProgramId, T.TacticTypeId, T.Title, T.Description, T.StartDate, T.EndDate, T.Cost, T.TacticBudget, T.Status, T.CreatedDate, T.ModifiedDate, T.IsDeleted, 
                  T.IsDeployedToIntegration, T.IntegrationInstanceTacticId, T.LastSyncDate, T.ProjectedStageValue, T.StageId, T.TacticCustomName, T.IntegrationWorkFrontProjectID, T.IntegrationInstanceEloquaId, T.LinkedTacticId, 
                  T.LinkedPlanId, T.IsSyncSalesForce, T.IsSyncEloqua, T.IsSyncWorkFront, T.IsSyncMarketo, T.IntegrationInstanceMarketoID, T.ModifiedBy, T.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId


GO

/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 10/27/2016 5:00:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 10/27/2016 5:00:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ExportToCSV] AS' 
END
GO
ALTER PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@clientId INT=0
,@HoneyCombids nvarchar(max)=null
,@CurrencyExchangeRate FLOAT=1
AS
BEGIN

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE #tblCustomData

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..#tblHoneyCombIds') IS NOT NULL
	DROP TABLE #tblHoneyCombIds

IF OBJECT_ID('tempdb..#tblPlanHoneyComb') IS NOT NULL
	DROP TABLE #tblPlanHoneyComb

IF OBJECT_ID('tempdb..#tblCampaignHoneyComb') IS NOT NULL
	DROP TABLE #tblCampaignHoneyComb

IF OBJECT_ID('tempdb..#tblProgramHoneyComb') IS NOT NULL
	DROP TABLE #tblProgramHoneyComb

IF OBJECT_ID('tempdb..#tblTacticHoneyComb') IS NOT NULL
	DROP TABLE #tblTacticHoneyComb

IF OBJECT_ID('tempdb.dbo.#EntityValues') IS NOT NULL 
	DROP TABLE #EntityValues 

	SELECT Item into #tblHoneyCombIds From dbo.SplitString(@HoneyCombids,',') 

	SELECT REPLACE(Item,'Plan_','') as Item into #tblPlanHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Plan%'

	SELECT REPLACE(Item,'Campaign_','') as Item into #tblCampaignHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Campaign%'

	SELECT REPLACE(Item,'Program_','') as Item into #tblProgramHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Program%'

	SELECT REPLACE(Item,'Tactic_','') as Item into #tblTacticHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Tactic%'
	


DECLARE @Entityids nvarchar(max)=''
	
SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',0 As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost','--' AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
,'--' as TacticCategory
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' AND IsDeleted=0) [CustomField]
WHERE 
--[Plan].PlanId IN (@PlanId)
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN [Plan].PlanId END) IN (SELECT Item FROm #tblPlanHoneyComb)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField.CustomFieldId,
CONVERT(NVARCHAR(800),CASE CustomFieldType.Name WHEN 'DropDownList' THEN (SELECT Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost','--' AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Campaign.PlanCampaignId END)IN(SELECT item FROM #tblCampaignHoneyComb)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost','--' AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId 
AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Program' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Program.PlanProgramId END)IN(SELECT item From #tblProgramHoneyComb)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',([Tactic].Cost*@CurrencyExchangeRate) As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,TacticType.AssetType as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE 
[Program].PlanProgramId=[Tactic].PlanProgramId 
AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title,AssetType FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',([lineitem].Cost*@CurrencyExchangeRate) As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Lineitem' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
) tblUnion

DECLARE   @ConcatString NVARCHAR(Max)=''

Declare @RowCount int , @Count int=1

SELECT ColName,ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM into #tblColName FROM (SELECT Distinct  ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName

SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';

CREATE TABLE #tblCustomData
(
ROWNUM INT,
Section NVARCHAR(MAX),
[Plan] NVARCHAR(MAX),		
Campaign NVARCHAR(MAX),
Program NVARCHAR(MAX),
Tactic NVARCHAR(MAX),
Lineitem NVARCHAR(MAX),
StartDate NVARCHAR(MAX),
EndDate NVARCHAR(MAX),
PlannedCost FLOAT,
TacticCategory NVARCHAR(MAX),
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy INT,
TargetStageGoal NVARCHAR(MAX),
ModelId INT,
MQL FLOAT,
Revenue FLOAT,
[Owner] NVARCHAR(MAX),
ExternalName NVARCHAR(MAX),
EntityId INT,
EntityType NVARCHAR(MAX),
ParentId INT,
CustomFieldType NVARCHAR(MAX)
)

DECLARE @Colname nvarchar(max)=''
DECLARE @AlterTable nvarchar(max)=''
While @Count<=@RowCount
BEGIN

SELECT @Colname = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ColName,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'�','-') FROM #tblColName WHERE ROWNUM=@Count --This is to Special charachter En Dash replace with Hyphen in CustomField Name
SET @AlterTable +=' ALTER TABLE #tblCustomData ADD ['+@Colname+'] NVARCHAR(MAX) ';
SET @ConcatString= @ConcatString +'['+ @Colname +']'+@Delimeter ;


SET @Count=@Count+1;
END


IF @ConcatString=null OR @ConcatString=''
BEGIN
	SET @ConcatString='DummyCol '
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD DummyCol NVARCHAR(MAX) '
END

SELECT @ConcatString=LEFT(@ConcatString, LEN(@ConcatString) - 1)

EXEC(@AlterTable)
DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT *  FROM
    (SELECT     
		ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		TacticCategory,
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
              CONVERT(NVARCHAR(MAX),Value) AS Value,
			  CustomFieldType
    FROM #tblPivot WITH (NOLOCK))X 
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P  
	'
	
	INSERT INTO #tblCustomData EXEC SP_EXECUTESQL @query	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
		
	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..#tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';	
	select * into #EntityValues from #tblCustomData WHERE 1=0
	-- Replace Stuff Logic
	-- ADD Columns into #EntityValues
	DECLARE @InsertStatement NVARCHAR(MAX)=''
	
	SET @AlterTable=''
	SET @Colname=''
	SET @Count=1
	SET @AlterTable+=' ALTER TABLE #EntityValues ADD Col_RowGroup NVARCHAR(MAX) '
	SET @InsertStatement=' INSERT INTO #EntityValues ('
	While @Count<=@RowCount
	BEGIN
		SELECT @Colname = name FROM #tbldynamicColumns WHERE ROWNUM=@Count
		SET @AlterTable +=' ALTER TABLE #EntityValues ADD [Col_'+(SELECT REPLACE(REPLACE(@Colname,' ','#'),'-','@'))+'] NVARCHAR(MAX) ';
		
		SET @InsertStatement+='['+@Colname+']'+@Delimeter
	SET @Count=@Count+1;
	END
	SET @InsertStatement+='Col_RowGroup) '
	--PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	--PRINT(@InsertStatement)
	EXEC(@InsertStatement)

	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tblEntityColumns from tempdb.sys.columns where object_id = object_id('tempdb..#EntityValues');

	DECLARE @EntityRowCount INT=0
	SELECT  @EntityRowCount = COUNT(*) FROM #tblEntityColumns
	
	DECLARE @MergeData nvarchar(max)=''
	SET @MergeData=''

	-- Declare Dynamic Variables
	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		SET @MergeData+=' DECLARE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	PRINT(@MergeData)
	-- END Dynamic Variables
	
	-- Update #EntityValues Tables row
	DECLARE @UpdateStatement NVARCHAR(MAX)=''
	SET @UpdateStatement=@MergeData+ '	UPDATE #EntityValues SET '
	SET @Count=1;
	SET @Delimeter=',';

	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		IF CHARINDEX('Col_',@Val) > 0
		BEGIN
			IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
			BEGIN
				IF(@Val='Col_PlannedCost')
				BEGIN
					SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),CAST(['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+'] AS decimal(38,2))) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),CAST(['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+'] AS decimal(38,2))) END'+@Delimeter
				END
				ELSE 
				BEGIN
					IF (@Val!='Col_Tactic' AND @Val!='Col_StartDate' AND @Val!='Col_EndDate' AND @Val!='Col_TargetStageGoal' AND @Val != 'Col_TacticCategory')
					BEGIN
						SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) END'+@Delimeter
					END
					ELSE
					BEGIN
						SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+'])'+@Delimeter
					END
				END
			END
		END
		SET @Count=@Count+1;
	END

	EXEC(@UpdateStatement)
	
	-- Select With GroupBy
	SET @Count=1;
	SET @Delimeter=',';
	DECLARE @SelectGroup NVARCHAR(MAX)=''
	DECLARE @ActualColName NVARCHAR(MAX)=''
	SET @SelectGroup=' SELECT EntityId,EntityType'
	While @Count<=@EntityRowCount
	BEGIN
	SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
	IF CHARINDEX('Col_',@Val) > 0
	BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @ActualColName=REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_','');
			IF (@ActualColName!='CustomFieldId' AND @ActualColName!='CustomFieldType' AND (@Val!='Col_EntityId' AND @Val!='Col_EntityType'))
			BEGIN
			IF(@ActualColName ='CustomFieldEntityId' OR @ActualColName='EntityId' OR @ActualColName='EndDate' OR @ActualColName='StartDate' OR @ActualColName='Plan' OR @ActualColName='Campaign' OR @ActualColName='Program' OR @ActualColName='Tactic' OR @ActualColName='LineItem' OR @ActualColName='EntityType' OR @ActualColName='ROWNUM' OR @ActualColName='PlannedCost' OR @ActualColName='Section' OR @ActualColName='Type' OR @ActualColName='EloquaId' OR @ActualColName='SFDCId' OR @ActualColName='ParentId' OR @ActualColName='CreatedBy' OR @ActualColName='TargetStageGoal' OR @ActualColName='ModelId' OR @ActualColName='ExternalName' OR @ActualColName='MQL' OR @ActualColName='Revenue' OR @ActualColName='Owner')
			BEGIN
				IF @ActualColName!='EndDate'
				BEGIN 
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MIN(['+ @Val+']) ';
				END
				ELSE 
				BEGIN
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MAX(['+ @Val+']) ';
				END
			END
			ELSE
			BEGIN
				SET @SelectGroup+=@Delimeter+' ['+@ActualColName+'] = MAX(['+@Val+']) ';
			END
			END
		END
	END
		SET @Count=@Count+1;
	END
	SET @SelectGroup+=' FROM #EntityValues GROUP BY EntityId,EntityType ORDER BY (CASE EntityType WHEN ''Plan'' THEN 1'
	SET @SelectGroup+=' WHEN ''Campaign'' THEN 2'
	SET @SelectGroup+=' WHEN ''Program'' THEN 3'
	SET @SelectGroup+=' WHEN ''Tactic'' THEN 4'
	SET @SelectGroup+=' WHEN ''Lineitem'' THEN 5'
	SET @SelectGroup+=' ELSE 6 END)';
	--PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic','Lineitem')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	WHEN 'Lineitem' THEN 4
	ELSE 5 END )
  END

  ELSE 
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End
END
GO

IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransactionLineItemMapping'))
DROP TABLE [dbo].[TransactionLineItemMapping]

IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Transactions'))
DROP TABLE [dbo].[Transactions]
GO

/****** Object:  Table [dbo].[Transactions]    Script Date: 11/15/2016 6:04:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Transactions](
	[TransactionId] [int] IDENTITY(1,1) NOT NULL,
	[ClientID] [int] NOT NULL,
	[ClientTransactionID] [varchar](150) NOT NULL,
	[TransactionDescription] [varchar](250) NULL,
	[Amount] [numeric](18, 0) NOT NULL,
	[Account] [varchar](150) NULL,
	[AccountDescription] [varchar](150) NULL,
	[SubAccount] [varchar](150) NULL,
	[Department] [varchar](150) NULL,
	[TransactionDate] [datetime] NULL,
	[AccountingDate] [datetime] NOT NULL,
	[Vendor] [varchar](150) NULL,
	[PurchaseOrder] [varchar](150) NULL,
	[CustomField1] [varchar](150) NULL,
	[CustomField2] [varchar](150) NULL,
	[CustomField3] [varchar](150) NULL,
	[CustomField4] [varchar](150) NULL,
	[CustomField5] [varchar](150) NULL,
	[CustomField6] [varchar](150) NULL,
	[LineItemId] [int] NULL,
	[DateCreated] [datetime] NOT NULL,
	[AmountAttributed] [float] NULL,
	[LastProcessed] [datetime] NULL,
 CONSTRAINT uc_ClientID_ClientTransactionId UNIQUE (ClientID, ClientTransactionId),
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[TransactionLineItemMapping]    Script Date: 11/15/2016 6:01:21 PM ******/
CREATE TABLE [dbo].[TransactionLineItemMapping](
	[TransactionLineItemMappingId] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[LineItemId] [int] NOT NULL,
	[Amount] [float] NULL,
	[DateModified] [datetime] NOT NULL,
	[ModifiedBy] [int] NOT NULL,
	[DateProcessed] [datetime] NULL,
 CONSTRAINT uc_PersonID UNIQUE (TransactionId,LineItemId),
 CONSTRAINT [PK_TransactionLineItemMapping] PRIMARY KEY CLUSTERED 
(
	[TransactionLineItemMappingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TransactionLineItemMapping]  WITH CHECK ADD  CONSTRAINT [FK_TransactionLineItemMapping_Plan_Campaign_Program_Tactic_LineItem] FOREIGN KEY([LineItemId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])
GO

ALTER TABLE [dbo].[TransactionLineItemMapping] CHECK CONSTRAINT [FK_TransactionLineItemMapping_Plan_Campaign_Program_Tactic_LineItem]
GO

ALTER TABLE [dbo].[TransactionLineItemMapping]  WITH CHECK ADD  CONSTRAINT [FK_TransactionLineItemMapping_Transactions] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[Transactions] ([TransactionId])
GO

ALTER TABLE [dbo].[TransactionLineItemMapping] CHECK CONSTRAINT [FK_TransactionLineItemMapping_Transactions]
GO

/****** Object:  StoredProcedure [dbo].[GetLinkedLineItemsForTransaction]    Script Date: 11/20/2016 4:09:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[GetLinkedLineItemsForTransaction]'))
DROP PROCEDURE [dbo].[GetLinkedLineItemsForTransaction];
GO 

CREATE PROCEDURE [dbo].[GetLinkedLineItemsForTransaction](@TransactionId INT)
AS 
BEGIN 

	--dataset 1: tactic data in context of a transaction
	SELECT T.PlanTacticId AS TacticId
			, T.Title
			, T.Cost AS PlannedCost
			, SUM(ISNULL(M.Amount, 0.0)) AS TotalLinkedCost --only the portion of the transaction that is linked to this tactic 
			, SUM(LA.Value) AS TotalActual 
 
	FROM Plan_Campaign_Program_tactic T 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanTacticId = T.PlanTacticId
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		LEFT JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId 
	WHERE M.TransactionId = @TransactionId
	GROUP BY T.PlanTacticId, T.Title, T.Cost

	--dataset 2: line items linked to the @transaction 
	SELECT    L.PlanTacticId AS TacticId
			, L.PlanLineItemId -- this the prmary key, the rest of non aggregate columns are auxiliary info. 
			, L.Title
			, L.Cost AS Cost
			, SUM(M.Amount) AS TotalLinkedCost -- SUM is a no-op as a transaction can only be linked once per line item 
			, SUM(LA.Value) AS TotalActual 
 
	FROM dbo.Plan_Campaign_Program_Tactic_LineItem L 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId 
	WHERE M.TransactionId = @TransactionId
	GROUP BY L.PlanTacticId, L.PlanLineItemId, L.Title, L.Cost

END 
GO

/****** Object:  Trigger [dbo].[UpdateCustomFieldEntityRestrictedTextByUser]    Script Date: 11/22/2016 11:56:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.triggers WHERE OBJECT_ID = OBJECT_ID('[dbo].TransactionCostToLineItemAttribution'))
	DROP TRIGGER  [dbo].[TransactionCostToLineItemAttribution]
GO

CREATE TRIGGER [dbo].[TransactionCostToLineItemAttribution] 
   ON  [dbo].[TransactionLineItemMapping] 
   AFTER INSERT,UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	--Handle delete (reall delete on this table)
	DELETE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual
	FROM DELETED D 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = D.LineItemId
		JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId 
		JOIN dbo.Transactions TX ON TX.TransactionId = D.TransactionId
	WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = D.LineItemId 
		AND dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period = INT.Period(T.StartDate, TX.AccountingDate)  

    -- We need set the createdBy and CreatedDate for both inserts and updates. 
	-- NOTE: there is no modified on line item actuals   
    DECLARE @CreatedBy INT
	DECLARE @CreatedDate DATETIME
	SELECT TOP 1  @CreatedBy = INSERTED.ModifiedBy
			, @CreatedDate = INSERTED.ModifiedBy 
	FROM INSERTED
	
	---Handle updates 
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET   Value = A.Amount
		, CreatedBy = @CreatedBy
		, CreatedDate =@CreatedDate
	FROM ( 
			SELECT SUM(M.Amount) AS Amount
					, L.PlanLineItemId
					, A.Period
			FROM	dbo.TransactionLineItemMapping M 
					JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
					JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
					JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
					JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = L.PlanLineItemId
	
			WHERE A.Period = INT.Period(T.StartDate, TX.AccountingDate) 
				  AND M.LineItemId IN (SELECT M.LineItemId FROM INSERTED)
			GROUP BY L.PlanLineItemId, A.Period
	) A
	WHERE A.PlanLineItemId = dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId
		AND A.Period = dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period

	-- Handle inserts 
	INSERT INTO  dbo.Plan_Campaign_Program_Tactic_LineItem_Actual(Value, PlanLineItemId, Period, CreatedDate, CreatedBy)
	SELECT SUM(M.Amount) AS Amount
			, L.PlanLineItemId
			, INT.Period(T.StartDate, TX.AccountingDate)
			, @CreatedDate
			, @CreatedBy
	FROM	dbo.TransactionLineItemMapping M 
			JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
			JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
			JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
			LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = L.PlanLineItemId 
				AND A.Period = INT.Period(T.StartDate, TX.AccountingDate) 	
	WHERE A.PlanLineItemId IS NULL 
		AND M.LineItemId IN (SELECT M.LineItemId FROM INSERTED)
	GROUP BY L.PlanLineItemId, INT.Period(T.StartDate, TX.AccountingDate)

	--Consider the update for 1 to 1 mapped transactions 
	--NOTE: this step is additive to the above 2 steps 
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET Value = Value + A.Amount
	FROM (
			SELECT SUM(TX.Amount) AS Amount
					, L.PlanLineItemId
					, INT.Period(T.StartDate, TX.AccountingDate) AS Period
			FROM dbo.Transactions TX 
				JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = TX.LineItemId
				JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId 
			WHERE TX.LineItemId IN (SELECT Inserted.LineItemId FROM INSERTED) 
			GROUP BY L.PlanLineItemId, INT.Period(T.StartDate, TX.AccountingDate)
		) A
	WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = A.PlanLineItemId 
		AND dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period = A.Period

END

-- =============================================
-- Author: Rahul Shah 
-- Create date: 11/21/2016
-- Description:	to Add TotalBudget & TotalForcast in Budget_Detail table to calculate unallocated cost.
-- =============================================
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalBudget')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	ADD [TotalBudget] float null 
   
END
GO
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalForcast')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	DROP Column [TotalForcast] 
   
END
GO
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalForecast')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	ADD [TotalForecast] float null
   
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Team]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[Plan_Team]
END
GO

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
set @release = 'November.2016'
set @version = 'November.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO
