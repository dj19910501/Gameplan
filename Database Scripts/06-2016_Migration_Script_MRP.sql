-- Updated by Akashdeep Kadia 
-- Updated on :: 01-June-2016
-- Desc :: Special charachters replace with Hyphen in Export to CSV. 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
Create PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@ClientId nvarchar(max)=''
,@HoneyCombids nvarchar(max)=null
AS
BEGIN

SET NOCOUNT ON;
Update CustomField set Name =REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Name,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'�','-') where ClientId=@ClientId-- This is to Special charachter En Dash replace with Hyphen in CustomField Name
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
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
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
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
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
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost',null AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
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
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',[Tactic].Cost As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
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
OUTER APPLY (SELECT TacticTypeId,Title FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
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
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
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
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy Uniqueidentifier,
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

SELECT @Colname = ColName FROM #tblColName WHERE ROWNUM=@Count
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
	PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	PRINT(@InsertStatement)
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
			SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) END'+@Delimeter
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
	PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType <> 'Budget'
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
AND EntityType <> 'Budget' 
AND EntityType <> 'Lineitem'
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End

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
-- Created by Nishant Sheth
-- Created on :: 06-Jun-2016
-- Desc :: Update Plan model id and tactic's tactic type id with new publish version of model 
CREATE PROCEDURE [dbo].[PublishModel]
@NewModelId int = 0 
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
	OPTION(MAXRECURSION 32767)

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
set @release = 'June.2016'
set @version = 'June.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO