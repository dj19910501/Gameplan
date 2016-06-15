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
Update CustomField set Name =REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Name,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') where ClientId=@ClientId-- This is to Special charachter En Dash replace with Hyphen in CustomField Name
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
,@UserId uniqueidentifier=''
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
	OPTION(MAXRECURSION 0)

-- Update Tactic Type for Default saved views
DECLARE  @TacticTypeIds NVARCHAR(MAX)=''
SELECT @TacticTypeIds = FilterValues From Plan_UserSavedViews WHERE Userid=@UserId AND FilterName='TacticType'

DECLARE   @FilterValues NVARCHAR(MAX)
SELECT    @FilterValues = COALESCE(@FilterValues + ',', '') + CAST(TacticTypeId AS NVARCHAR) FROM TacticType 
WHERE PreviousTacticTypeId IN(SELECT val FROM dbo.comma_split(@TacticTypeIds,','))
AND ModelId=@NewModelId

IF @FilterValues <>'' 
BEGIN
	UPDATE Plan_UserSavedViews SET FilterValues=@FilterValues WHERE Userid=@UserId AND FilterName='TacticType'
END

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




-- Added by Komal Rawal
-- Added on :: 08-June-2016
-- Desc :: On creation of new item in Marketing budget give permission as per parent items.
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveuserBudgetPermission]
GO
/****** Object:  StoredProcedure [dbo].[SaveuserBudgetPermission]    Script Date: 06/09/2016 15:30:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveuserBudgetPermission] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveuserBudgetPermission]
@BudgetDetailId int  = 0,
@PermissionCode int = 0,
@CreatedBy uniqueidentifier 
AS
BEGIN
WITH CTE AS
(
 
    SELECT ParentId 
    FROM Budget_Detail
    WHERE id= @BudgetDetailId
    UNION ALL
    --This is called multiple times until the condition is met
    SELECT g.ParentId 
    FROM CTE c, Budget_Detail g
    WHERE g.id= c.parentid

)

Select * into #tempbudgetdata from CTE
option (maxrecursion 0)

select * from #tempbudgetdata where ParentId is not null

insert into Budget_Permission select Distinct UserId,@BudgetDetailId,GETDATE(),@CreatedBy,@PermissionCode,
Case WHEN UserId = @CreatedBy
THEN 
 1
 ELSE
 0 END
from Budget_Permission where BudgetDetailId in (select ParentId from #tempbudgetdata)
UNION
select  @CreatedBy,@BudgetDetailId,GETDATE(),@CreatedBy,@PermissionCode,1 from Budget_Permission 

IF OBJECT_ID('tempdb..##tempbudgetdata') IS NOT NULL
Drop Table #tempbudgetdata

END

GO


IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Report_Intergration_Conf'))
BEGIN
CREATE TABLE [dbo].[Report_Intergration_Conf](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[IdentifierColumn] [nvarchar](255) NOT NULL,
	[IdentifierValue] [nvarchar](1000) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MeasureApiConfiguration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_TableName_IdentifierColumn_IdentifierValue] UNIQUE NONCLUSTERED 
(
	[TableName] ASC,
	[IdentifierColumn] ASC,
	[IdentifierValue] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboarContentData]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[GetDashboarContentData]
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetDashboarContentData]
	-- Add the parameters for the stored procedure here
	@ClientId varchar(max),
	@DashboardID int = 0    
AS
BEGIN
	IF (@ClientId IS NOT NULL AND @ClientId != '')
	BEGIN
		IF (ISNULL(@DashboardID, 0) > 0)
		BEGIN
			IF EXISTS (SELECT db.id
				FROM Dashboard db
				INNER JOIN Report_Intergration_Conf AS RIC ON (RIC.IdentifierValue = db.id AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId)
				WHERE IsDeleted = 0 AND db.id = @DashboardID)
			BEGIN
				SELECT db.id, Name, DisplayName, DisplayOrder, CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted, IsComparisonDisplay, HelpTextId
				FROM Dashboard db
				INNER JOIN Report_Intergration_Conf AS RIC ON (RIC.IdentifierValue = db.id AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId)
				WHERE IsDeleted = 0 AND db.id = @DashboardID

				SELECT dc.id, dc.DisplayName, dc.DashboardId, dc.DisplayOrder, dc.ReportTableId, dc.ReportGraphId, dc.Height, dc.Width, dc.Position, dc.IsCumulativeData, dc.IsCommunicativeData, dc.DashboardPageID,					dc.IsDeleted, dc.DisplayIfZero, dc.KeyDataId, dc.HelpTextId
				FROM DashboardContents AS dc 
				INNER JOIN Dashboard AS db ON db.id = dc.DashboardId AND db.IsDeleted = 0
				INNER JOIN Report_Intergration_Conf AS RIC ON (RIC.IdentifierValue = db.id AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId)
				WHERE dc.IsDeleted = 0 AND dc.DashboardId = @DashboardID
			END
			ELSE
			BEGIN
				SELECT 'Client Not Authorize to Access Dashboard'
			END
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT db.id
						FROM Dashboard AS db
						INNER JOIN Report_Intergration_Conf RIC ON db.id = RIC.IdentifierValue AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId
						WHERE db.IsDeleted = 0)
			BEGIN

				SELECT db.id, Name, DisplayName, DisplayOrder, CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted, IsComparisonDisplay, HelpTextId
					FROM Dashboard AS db
					INNER JOIN Report_Intergration_Conf RIC ON db.id = RIC.IdentifierValue AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId
					WHERE db.IsDeleted = 0
			END
			ELSE
			BEGIN
				SELECT 'No Dashboard Configured For Client'
			END
		END
	END
	ELSE
	BEGIN
		SELECT 'Please Provide Proper ClientId'
	END
END

GO

--- START: PL ticket #2251 related SPs & Functions --------------------
-- Created By: Viral 
-- Created On: 06/10/2016
-- Description: PL ticket #2251: Prepared Data before push to SFDC through Integration Web API.

/****** Object:  StoredProcedure [dbo].[spGetSalesforceMarketo3WayData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceMarketo3WayData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceMarketo3WayData]
GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacticActualCostMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCTacticResultColumns]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
(
	@entityType varchar(255)='''',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255,	-- default value 255
	@isCustomNameAllow bit =''0'',
	@isClientAllowCustomName bit =''0''
)

--SELECT * from  [GetSFDCSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''101371'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementCampaign'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''16404'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementProgram'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''16402'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementTactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''7864'',2,1203,255,0,0)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS

BEGIN

------- START:- Declare local variables 
	BEGIN
		Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),CustomFieldId INT,IsImprovement bit)
		Declare @tacActCostTable Table(PlanTacticId int,ActualCost varchar(50))
		Declare @ColumnName nvarchar(max)
		-- START: Declare Fixed columns SFDC variables
		Declare @actSFDCID varchar(50)=''SalesforceId''
		--Declare @trgtSFDCID varchar(50)=''SalesforceId''
		Declare @actSourceParentId varchar(50)=''SourceParentId''
		--Declare @trgtSourceParentId varchar(50)=''''
		Declare @actTitle varchar(255)=''Title''
		Declare @actMode varchar(255)=''Mode''
		--Declare @trgtMode varchar(255)=''''
		Declare @actObjType varchar(255)=''ObjectType''
		--Declare @trgtObjType varchar(255)=''''
		Declare @actStartDate varchar(255)=''StartDate''
		Declare @actEndDate varchar(255)=''EndDate''
		 -- END: Declare Fixed columns SFDC variables
		Declare @modeCREATE varchar(20)=''Create''
		Declare @modeUPDATE varchar(20)=''Update''
		Declare @actCreatedBy varchar(255)=''CreatedBy''
		Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''
		Declare @tblGlobal varchar(100)=''Global''
		 -- START:- Declare entityType variables
		Declare @entTactic varchar(20 )=''Tactic''
		Declare @entProgram varchar(20 )=''Program''
		Declare @entCampaign varchar(20 )=''Campaign''
		Declare @entImprvTactic varchar(255)=''ImprovementTactic''
		Declare @entImprvProgram varchar(255)=''ImprovementProgram''
		Declare @entImprvCampaign varchar(255)=''ImprovementCampaign''
		-- END:- Declare entityType variables

		-- START: Plan Entity Status Variables
		Declare @declined varchar(50)=''Declined''
		Declare @InProgress varchar(50)=''In-Progress''
		Declare @completed varchar(50)=''Complete''
		Declare @sfdcAborted varchar(50)=''Aborted''
		Declare @sfdcInProgress varchar(50)=''In Progress''
		Declare @sfdcCompleted varchar(50)=''Completed''
		Declare @sfdcPlanned varchar(50)=''Planned''
		-- END: Plan Entity Status Variables

		 -- START:- Improvement Variable declaration
		 --Cost Field
		Declare @imprvCost varchar(20)=''ImprvCost''
		Declare @actImprvCost varchar(20)=''Cost''

		 -- Static Status
		 Declare @imprvPlannedStatus varchar(50)=''Planned''
		 Declare @tblImprvTactic varchar(200)=''Plan_Improvement_Campaign_Program_Tactic''
		 Declare @tblImprvProgram varchar(200)=''Plan_Improvement_Campaign_Program''
		 Declare @tblImprvCampaign varchar(200)=''Plan_Improvement_Campaign''

		 -- Imprv. Tactic table Actual Fields
		 Declare @actEffectiveDate varchar(50)=''EffectiveDate''
		 -- END: Improvement Variable declaration
	END

 
------- END:- Declare local variables 

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	(
			-- Select GLOBAL standard fields from IntegrationInstanceDataTypeMapping table.

				Select  IntegrationInstanceID,
						IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
						TableName,
						gpDataType.ActualFieldName,
						IsNull(mapp.CustomFieldId,0) as CustomFieldId,
						IsNull(gpDataType.IsImprovement,''0'') as IsImprovement
				FROM GamePlanDataType as gpDataType
				JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
				Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@tblGlobal and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
			--END
			
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId,
					''0'' as IsImprovement
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	)
	insert into @Table 
	select * from ResultTable

	-- IF EntityType is ''Tactic'' then add Tacic related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF(@entityType=@entTactic)
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''0'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=''Plan_Campaign_Program_Tactic'' and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

	-- IF EntityType is ''Improvement Campaign, Program or Tactic'' then add respective entity related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF((@entityType=@entImprvTactic) OR (@entityType=@entImprvProgram) OR (@entityType=@entImprvCampaign))
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				CASE 
					WHEN ((gpDataType.TableName=@tblImprvTactic) AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''1'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@entityType) and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

END
-------- END: Get Standard & CustomField Mappings data --------

-------- START: Insert fixed SFDC fields to Mapping list. -------- 
IF((@entityType=@entImprvTactic) OR (@entityType=@entImprvProgram) OR (@entityType=@entImprvCampaign))
BEGIN
	-- Insert table name ''Global'' and IsImprovement flag ''1'' in case of Improvement entities
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
END
ELSE
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
END
-------- END: Insert fixed SFDC fields to Mapping list. -------- 

-------- START: Get Tacticwise ActualCost. -------- 

Declare @actCost varchar(20)=''CostActual''
Declare @actCostGPTypeId int=0
Select @actCostGPTypeId = GameplanDataTypeId from GameplanDataType where IntegrationTypeId=@integrationTypeId and IsDeleted=''0'' and TableName=@tblTactic and ActualFieldName=@actCost

-- Calculate Tactiwise ActualCost in case of If user has made ActualCost mapping and EntityType is Tactic 
IF EXISTS(Select * from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@id and GameplanDataTypeId=@actCostGPTypeId)AND(@entityType=@entTactic)
BEGIN
	INSERT INTO @tacActCostTable
	SELECT * FROM [dbo].[GetTacticActualCostMappingData](@EntityIds)
END
-------- END: Get Tacticwise ActualCost. -------- 

;WITH entTbl as(
	(
		-- Get Tactics
		SELECT 
			T.PlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program_Tactic T 
		WHERE @entityType=@entTactic and PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Programs
		SELECT 
			P.PlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program P 
		WHERE @entityType=@entProgram and PlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Campaigns
		SELECT 
			C.PlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign C 
		WHERE @entityType=@entCampaign and PlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Campaigns
		SELECT 
			IC.ImprovementPlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign IC 
		WHERE @entityType=@entImprvCampaign and ImprovementPlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Programs
		SELECT 
			IP.ImprovementPlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign_Program IP 
		WHERE @entityType=@entImprvProgram and ImprovementPlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Tactics
		SELECT 
			IT.ImprovementPlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign_Program_Tactic IT 
		WHERE @entityType=@entImprvTactic and ImprovementPlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Static_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
SELECT * FROM 
(
	(
		-- GET Tactic Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (CASE
																		WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																		ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																   END)
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),Tac.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),Tac.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Tac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(Tac.PlanProgramId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(0 as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entTactic
		LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.SourceId = Tac.PlanTacticId
		LEFT JOIN @tacActCostTable as acost ON T.SourceId = acost.PlanTacticId
		LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(prg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(prg.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),prg.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),prg.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(prg.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(prg.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(prg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(prg.PlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entProgram
		LEFT JOIN Plan_Campaign_Program as prg ON T.SourceId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(cmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(cmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(cmpgn.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(cmpgn.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(cmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entCampaign
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = T.SourceId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Improvement Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(Imprvcmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Imprvcmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Imprvcmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Imprvcmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvCampaign
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = T.SourceId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvCampaign)
	)
	UNION
	(
		-- GET Improvement Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(ImprvPrg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(ImprvPrg.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(ImprvPrg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(ImprvPrg.ImprovementPlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(ImprvPrg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvProgram
		LEFT JOIN Plan_Improvement_Campaign_Program as ImprvPrg ON ImprvPrg.ImprovementPlanProgramId = T.SourceId
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvProgram)
	)
	UNION
	(
		-- GET Improvement Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(ImprvTac.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(ImprvTac.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actImprvCost THEN ISNull(Cast(ImprvTac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=@actEffectiveDate THEN ISNull(CONVERT(VARCHAR(19),ImprvTac.EffectiveDate),'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(ImprvTac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(ImprvTac.ImprovementPlanProgramId as nvarchar(255)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(ImprvTac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvTactic
		LEFT JOIN Plan_Improvement_Campaign_Program_Tactic as ImprvTac ON ImprvTac.ImprovementPlanTacticId = T.SourceId
		LEFT JOIN Plan_Improvement_Campaign_Program as ImprvPrg ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvTactic)
	)
) as result;



Update @src_trgt_mappdata Set TacValue=
								CASE 
									WHEN TacValue=@declined THEN @sfdcAborted
									WHEN TacValue=@InProgress THEN @sfdcInProgress
									WHEN TacValue=@completed THEN @sfdcCompleted
									ELSE @sfdcPlanned
								END 
WHERE ActualFieldName=''Status''


RETURN
END
' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
(
	@entityType varchar(255)='''',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255,	-- default value 255
	@isCustomNameAllow bit =''0'',
	@isClientAllowCustomName bit =''0''
)

--SELECT * from  [GetSFDCSourceTargetMappingData_Marketo3Way](''Program'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''32029'',2,1203,255,0,0)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS

BEGIN

------- START:- Declare local variables 
	BEGIN
		Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),CustomFieldId INT,IsImprovement bit)
		Declare @tacActCostTable Table(PlanTacticId int,ActualCost varchar(50))
		Declare @ColumnName nvarchar(max)
		-- START: Declare Fixed columns SFDC variables
		Declare @actSFDCID varchar(50)=''SalesforceId''
		Declare @actSourceParentId varchar(50)=''SourceParentId''
		Declare @actTitle varchar(255)=''Title''
		Declare @actMode varchar(255)=''Mode''
		Declare @actObjType varchar(255)=''ObjectType''
		Declare @actStartDate varchar(255)=''StartDate''
		Declare @actEndDate varchar(255)=''EndDate''
		Declare @actsfdcParentId varchar(50)=''ParentId''
		 -- END: Declare Fixed columns SFDC variables
		Declare @modeCREATE varchar(20)=''Create''
		Declare @modeUPDATE varchar(20)=''Update''
		Declare @actCreatedBy varchar(255)=''CreatedBy''
		Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''
		Declare @tblGlobal varchar(100)=''Global''
		 -- START:- Declare entityType variables
		Declare @entTactic varchar(20 )=''Tactic''
		Declare @entProgram varchar(20 )=''Program''
		Declare @entCampaign varchar(20 )=''Campaign''
		-- END:- Declare entityType variables

		-- START: Plan Entity Status Variables
		Declare @declined varchar(50)=''Declined''
		Declare @InProgress varchar(50)=''In-Progress''
		Declare @completed varchar(50)=''Complete''
		Declare @sfdcAborted varchar(50)=''Aborted''
		Declare @sfdcInProgress varchar(50)=''In Progress''
		Declare @sfdcCompleted varchar(50)=''Completed''
		Declare @sfdcPlanned varchar(50)=''Planned''
		-- END: Plan Entity Status Variables
		
	END

 
------- END:- Declare local variables 

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	(
			-- Select GLOBAL standard fields from IntegrationInstanceDataTypeMapping table.

				Select  IntegrationInstanceID,
						IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
						TableName,
						gpDataType.ActualFieldName,
						IsNull(mapp.CustomFieldId,0) as CustomFieldId,
						IsNull(gpDataType.IsImprovement,''0'') as IsImprovement
				FROM GamePlanDataType as gpDataType
				JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
				Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@tblGlobal and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
			--END
			
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId,
					''0'' as IsImprovement
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	)
	insert into @Table 
	select * from ResultTable

	-- IF EntityType is ''Tactic'' then add Tacic related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF(@entityType=@entTactic)
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''0'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=''Plan_Campaign_Program_Tactic'' and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

END
-------- END: Get Standard & CustomField Mappings data --------

-------- START: Insert fixed SFDC fields to Mapping list. -------- 
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actsfdcParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
END
-------- END: Insert fixed SFDC fields to Mapping list. -------- 

-------- START: Get Tacticwise ActualCost. -------- 

Declare @actCost varchar(20)=''CostActual''
Declare @actCostGPTypeId int=0
Select @actCostGPTypeId = GameplanDataTypeId from GameplanDataType where IntegrationTypeId=@integrationTypeId and IsDeleted=''0'' and TableName=@tblTactic and ActualFieldName=@actCost

-- Calculate Tactiwise ActualCost in case of If user has made ActualCost mapping and EntityType is Tactic 
IF EXISTS(Select * from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@id and GameplanDataTypeId=@actCostGPTypeId)AND(@entityType=@entTactic)
BEGIN
	INSERT INTO @tacActCostTable
	SELECT * FROM [dbo].[GetTacticActualCostMappingData](@EntityIds)
END
-------- END: Get Tacticwise ActualCost. -------- 

;WITH entTbl as(
	(
		-- Get Tactics
		SELECT 
			T.PlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program_Tactic T 
		WHERE @entityType=@entTactic and PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Programs
		SELECT 
			P.PlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program P 
		WHERE @entityType=@entProgram and PlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Campaigns
		SELECT 
			C.PlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign C 
		WHERE @entityType=@entCampaign and PlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Static_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
SELECT * FROM 
(
	(
		-- GET Tactic Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (CASE
																		WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																		ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																   END)
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),Tac.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),Tac.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Tac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(Tac.PlanProgramId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(0 as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN (Mapp.ActualFieldName=@actsfdcParentId) AND (ISNULL(Tac.IntegrationInstanceTacticId,'''')<>'''') THEN prg.IntegrationInstanceProgramId		-- In case of Marketo-SFDC 3-Way integration, Add Program SFDCID to create hierearchy in SFDC
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entTactic
		LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.SourceId = Tac.PlanTacticId
		LEFT JOIN @tacActCostTable as acost ON T.SourceId = acost.PlanTacticId
		LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(prg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(prg.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),prg.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),prg.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(prg.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(prg.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(prg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(prg.PlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN (Mapp.ActualFieldName=@actsfdcParentId) AND (ISNULL(prg.IntegrationInstanceProgramId,'''')<>'''') THEN cmpgn.IntegrationInstanceCampaignId		-- In case of Marketo-SFDC 3-Way integration, Add Program SFDCID to create hierearchy in SFDC
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entProgram
		LEFT JOIN Plan_Campaign_Program as prg ON T.SourceId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(cmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(cmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(cmpgn.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(cmpgn.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(cmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entCampaign
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = T.SourceId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
) as result;

Update @src_trgt_mappdata Set TacValue=
								CASE 
									WHEN TacValue=@declined THEN @sfdcAborted
									WHEN TacValue=@InProgress THEN @sfdcInProgress
									WHEN TacValue=@completed THEN @sfdcCompleted
									ELSE @sfdcPlanned
								END 
WHERE ActualFieldName=''Status''

RETURN
END
' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- SElect [GetSFDCTacticResultColumns] (1203,''464EB808-AD1F-4481-9365-6AADA15023BD'',2)
CREATE FUNCTION [dbo].[GetSFDCTacticResultColumns]
(
	@id int,
	@clientId uniqueidentifier,
	@integrationTypeId int
)
RETURNS nvarchar(max)
AS
BEGIN
Declare @imprvCost varchar(20)=''ImprvCost''
Declare @actImprvCost varchar(20)=''Cost''
declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
declare @ColumnName nvarchar(max)

	;With ResultTable as(

(
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				-- Rename actualfield ''Cost'' to ''ImprvCost''  in case of Table Name ''Plan_Improvement_Campaign_Program_Tactic'' to ignore conflict of same name ''Cost'' actual field of both Tactic & Improvement Tactic table
				CASE 
					WHEN  ((gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'') AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost 
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Plan_Campaign_Program'' OR gpDataType.TableName=''Plan_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		SELECT  mapp.IntegrationInstanceId,
				0 as GameplanDataTypeId,
				Null as TableName,
				custm.Name as ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
				
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and (custm.EntityType=''Tactic'' or custm.EntityType=''Campaign'' or custm.EntityType=''Program'')
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
  
  SELECT @ColumnName= ISNULL(@ColumnName + '','','''') 
       + QUOTENAME(ActualFieldName)
FROM (Select Distinct ActualFieldName FROM @Table) AS ActualFields
RETURN @ColumnName
END

' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetTacticActualCostMappingData]
(
	@entIds varchar(max)=''''
)
RETURNS @tac_actualcost_mappingtbl Table(
PlanTacticId int,
ActualCost varchar(50)
)

AS
BEGIN
	Declare @costStage varchar(20)=''Cost''

	-- Get Tactic & Tactic Actual Cost Mapping data 
	-- If Tactic has lineitems then Sum up of LineItem Actual''s value else Tactic Actual''s value.

	INSERT INTO @tac_actualcost_mappingtbl
	SELECT tac.PlanTacticId,
	   	   CASE 
			WHEN COUNT(distinct line.PlanLineItemId) >0 THEN  Cast(IsNULL(SUM(lActl.Value),0) as varchar(50)) ELSE  Cast(IsNULL(SUM(tActl.Actualvalue),0) as varchar(50))
		   END as ActualCost
	FROM Plan_Campaign_Program_Tactic as tac
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem as line on tac.PlanTacticId = line.PlanTacticId and line.IsDeleted=''0''
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual as lActl on line.PlanLineItemId = lActl.PlanLineItemId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual as tActl on tac.PlanTacticId = tActl.PlanTacticId and  tActl.StageTitle=@costStage
	WHERE tac.PlanTacticId IN (select val from comma_split(@entIds,'',''))
	GROUP BY tac.PlanTacticId
	RETURN 
END
' 
END

GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetSalesforceData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetSalesforceData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeProg varchar(255)='Program'
		Declare @entityTypeCampgn varchar(255)='Campaign'
		Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
		Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
		Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
		Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)='In-Progress'
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit='0'
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit ='0'
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''
		Declare @dynResultQuery nvarchar(max)=''

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)='PushTacticData'
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)='Name'
		Declare @colDescription varchar(50)='Description'
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)='Tactic'
		Declare @prg varchar(20)='Program'
		Declare @cmpgn varchar(20)='Campaign'
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''
		Declare @entPrgIds nvarchar(max)=''
		Declare @entCmpgnIds nvarchar(max)=''
		Declare @entImrvmntTacIds nvarchar(max)=''
		Declare @entImrvmntPrgIds nvarchar(max)=''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''
		-- End: Entity Ids

	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Store Improvement Entities related data
	Declare @tblImprvEntity table (
									ImprovementPlanTacticId int,
									ImprovementPlanProgramId int,
									ImprovementPlanCampaignId int,
									ObjectType varchar(50)
								  )

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Campaign,Program,Tactic,Improvement Tactic or Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Instance Id')
					SET @instanceId= @id

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- Get Improvement TacticIds
							BEGIN
								Insert into @tblImprvEntity
								Select Imprvtact.ImprovementPlanTacticId,
									   Imprvtact.ImprovementPlanProgramId,
									   Imprvcampgn.ImprovementPlanCampaignId,
									   @entityTypeImprvTac as ObjectType
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Improvement_Campaign as Imprvcampgn ON Imprvcampgn.ImprovePlanId = pln.PlanId 
								join Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
								join Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
								where mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							END

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			
		END	
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeTac))
		BEGIN
			
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Tactic Id')
			BEGIN TRY

				-- Pick latest year tactic in case of linked Tactic and push to SFDC.
				IF EXISTS(SELECT LinkedTacticId from Plan_Campaign_Program_Tactic where PlanTacticId=@id)
				BEGIN
					
					DECLARE @tac_lnkdIds varchar(20)=''
					SELECT @tac_lnkdIds=cast(PlanTacticId as varchar)+','+Cast(ISNULL(LinkedTacticId,0) as varchar) 
					FROM Plan_Campaign_Program_Tactic where PlanTacticId=@id
					;WITH tbl as(
								SELECT tact.PlanTacticId,tact.LinkedTacticId,tact.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tact
								WHERE PlanTacticId IN (select val from comma_split(@tac_lnkdIds,',')) and tact.IsDeleted=0
								UNION ALL
								SELECT tac.PlanTacticId,tac.LinkedTacticId,tac.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tac 
								INNER JOIN tbl as lnk on tac.LinkedTacticId=lnk.PlanTacticId
								WHERE tac.PlanTacticId=@id
								)
					-- Set latest year tactic to @id variable
					SELECT TOP 1 @id=LinkedTacticId 
					FROM tbl
					INNER JOIN [Plan] as pln on tbl.LinkedPlanId = pln.PlanId and pln.IsDeleted=0
					ORDER BY [Year] DESC
				END
			
				INSERT INTO @tblTaclist 
				SELECT tact.PlanTacticId,
						tact.PlanProgramId,
						prg.PlanCampaignId,
						tact.LinkedTacticId ,
						tact.LinkedPlanId,
						Null as PlanYear,
						@tact as ObjectType,
						1 as RN
				FROM Plan_Campaign_Program_Tactic as tact 
				INNER JOIN Plan_Campaign_Program as prg on tact.PlanProgramId = prg.PlanProgramId and prg.IsDeleted='0'
				WHERE tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete') and tact.PlanTacticId=@id
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
				INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0
				INNER JOIN @tblTaclist as tac ON prg.PlanProgramId = tac.PlanProgramId
			END TRY
			BEGIN CATCH
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
			END CATCH

			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Tactic Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeProg))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Program Id')

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where pln.IsDeleted=0
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Plan] as pln
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- START: Get list of Campaigns not pushed into SFDC.
							BEGIN
								Insert into @tblTaclist 
								select Null as PlanTacticId,
									   Null as PlanProgramId,
									   cmpgn.PlanCampaignId,
									   Null as LinkedTacticId ,
									   Null as LinkedPlanId,
									   tac.PlanYear,
									   @cmpgn as ObjectType,
									   RN= 1
								from @tblTaclist as tac
								INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
								INNER JOIN Plan_Campaign_Program as P on tac.PlanProgramId=P.PlanProgramId and (IsNull(P.IntegrationInstanceProgramId,'')='')
								where tac.ObjectType=@prg
							END
							-- END: Get list of Campaigns not pushed into SFDC.
							

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
					INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0 and prg.PlanProgramId=@id
					
					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Program Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeCampgn))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Campaign Id')
					
					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln 
								INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
								INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								INNER JOIN Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  pln.IsDeleted=0
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where pln.IsDeleted=0
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0 and cmpgn.PlanCampaignId=@id

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Campaign Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeImprvTac))
		BEGIN
			-- Get Improvement TacticIds
			BEGIN
				Insert into @tblImprvEntity
				Select Imprvtact.ImprovementPlanTacticId,
					   Imprvtact.ImprovementPlanProgramId,
					   Imprvcampgn.ImprovementPlanCampaignId,
					   @entityTypeImprvTac as ObjectType
				from Plan_Improvement_Campaign as Imprvcampgn
				INNER JOIN Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
				INNER JOIN Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.ImprovementPlanTacticId=@id and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Improvement_Campaign] as Imprvcmpgn ON pln.PlanId = Imprvcmpgn.ImprovePlanId
				INNER JOIN [Plan_Improvement_Campaign_Program] as ImprvPrg ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId 
				INNER JOIN @tblImprvEntity as ImprvTac ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
			END
		END
		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Tactic or Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: Add list of Improvement Programs not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Imprvprg.ImprovementPlanProgramId,
						   Imprvprg.ImprovementPlanCampaignId,
							@entityTypeImprvProg as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign_Program as Imprvprg on Imprvtac.ImprovementPlanProgramId = Imprvprg.ImprovementPlanProgramId and IsNull(Imprvprg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Improvement Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Null as ImprovementPlanProgramId,
						   ImprvCmpgn.ImprovementPlanCampaignId,
							@entityTypeImprvCamp as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign as ImprvCmpgn on Imprvtac.ImprovementPlanCampaignId = ImprvCmpgn.ImprovementPlanCampaignId and IsNull(ImprvCmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist)) OR (EXISTS(Select 1 from @tblImprvEntity))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
										
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + ',','') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + ',','') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids

					-- Get Improvement Ids
					BEGIN
						-- START: Get ImprvmntTacticIds
						SELECT @entImrvmntTacIds = ISNULL(@entImrvmntTacIds  + ',','') + (ImprvTac)
						FROM (Select DISTINCT Cast (ImprovementPlanTacticId as varchar(max)) ImprvTac FROM @tblImprvEntity where ObjectType=@entityTypeImprvTac) AS PlanTacticIds
						-- END: Get ImprvmntTacticIds

						-- START: Get ImprvmntCampaign Ids
						SELECT @entImrvmntCmpgnIds = ISNULL(@entImrvmntCmpgnIds  + ',','') + (ImprvCampgn)
						FROM (Select DISTINCT Cast (ImprovementPlanCampaignId as varchar(max)) ImprvCampgn FROM @tblImprvEntity where ObjectType=@entityTypeImprvCamp) AS PlanCampaignIds
						-- END: Get ImprvmntCampaign Ids

						-- START: Get ImprvmntProgram Ids
						SELECT @entImrvmntPrgIds= ISNULL(@entImrvmntPrgIds + ',','') + (ImprvPrgrm)
						FROM (Select DISTINCT Cast (ImprovementPlanProgramId as varchar(max)) ImprvPrgrm FROM @tblImprvEntity where ObjectType=@entityTypeImprvProg) AS PlanProgramIds
						-- END: Get ImprvmntProgram Ids
					END
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,'0')='1' AND IsNull(@isClientAllowCustomName,'0')='1')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

						BEGIN TRY
							-- START: Update Tactic Name --
							UPDATE Plan_Campaign_Program_Tactic 
							SET TacticCustomName = T1.CustomName 
							FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
							INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'')=''
							-- END: Update Tactic Name --
						END TRY
						BEGIN CATCH
							Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
						END CATCH

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'101371',2,1203,255,0,0)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT distinct SourceId,SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @tact +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @prg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @cmpgn +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvCamp +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvProg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvTac +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+')
								 ) AS PVTTable
								 '
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()+',SQL Query-'+ (Select @dynResultQuery)))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Create final result Pivot Query')
					--PRINT @dynResultQuery  
					--Execute the Dynamic Pivot Query
					--EXEC sp_executesql @dynResultQuery
					
				END
				ELSE
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for Salesforce instance')
				END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Data does not exist')
			END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push Salesforce')
	EXEC(@dynResultQuery)
	--select * from @tblSyncError
	--SELECT @logStartInstanceLogId as 'InstanceLogStartId'
END



GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceMarketo3WayData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceMarketo3WayData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetSalesforceMarketo3WayData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetSalesforceMarketo3WayData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeProg varchar(255)='Program'
		Declare @entityTypeCampgn varchar(255)='Campaign'
		Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
		Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
		Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
		Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)='In-Progress'
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit='0'
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit ='0'
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''
		Declare @dynResultQuery nvarchar(max)=''

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)='PushTacticData'
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)='Name'
		Declare @colDescription varchar(50)='Description'
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)='Tactic'
		Declare @prg varchar(20)='Program'
		Declare @cmpgn varchar(20)='Campaign'
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''
		Declare @entPrgIds nvarchar(max)=''
		Declare @entCmpgnIds nvarchar(max)=''
		Declare @entImrvmntTacIds nvarchar(max)=''
		Declare @entImrvmntPrgIds nvarchar(max)=''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''
		-- End: Entity Ids

	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Instance Id')
					SET @instanceId= @id

					-- START: Identify 3 Way integration between Marketo & SFDC - Create hierarchy in SFDC 
					BEGIN
						Declare @isSyncSFDCWithMarketo bit='1'
						Declare @ModelIds varchar(max)=''
						SELECT @ModelIds = ISNULL(@ModelIds  + ',','') + (mdlId)
						FROM (Select DISTINCT Cast (ModelId as varchar(max)) mdlId from Model where ((IsNull(IsDeleted,'0')='0') AND (IsNull(IntegrationInstanceId,0)=0) AND (IntegrationInstanceMarketoID>0) AND IntegrationInstanceMarketoID<>@id AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id))) AS planIds
					END

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncMarketo='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  mdl.ModelId IN (
														Select DISTINCT ModelId from Model where ((IsNull(IsDeleted,'0')='0') AND (IsNull(IntegrationInstanceId,0)=0) AND (IntegrationInstanceMarketoID>0) AND IntegrationInstanceMarketoID<>@id AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id))
													   ) and mdl.[Status]='Published' and mdl.IsActive='1'
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			
		END	
		
		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
										
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + ',','') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + ',','') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,'0')='1' AND IsNull(@isClientAllowCustomName,'0')='1')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

						BEGIN TRY
							-- START: Update Tactic Name --
							UPDATE Plan_Campaign_Program_Tactic 
							SET TacticCustomName = T1.CustomName 
							FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
							INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'')=''
							-- END: Update Tactic Name --
						END TRY
						BEGIN CATCH
							Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
						END CATCH

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'101371',2,1203,255,0,0)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT distinct SourceId,SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way]('''+ @tact +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way]('''+ @prg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way]('''+ @cmpgn +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+')
								 ) AS PVTTable
								 '
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()+',SQL Query-'+ (Select @dynResultQuery)))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Create final result Pivot Query')
										
				END
				ELSE
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for Salesforce instance')
				END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Data does not exist')
			END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push Salesforce')
	EXEC(@dynResultQuery)
	
END



GO
--- END: PL ticket #2251 related SPs & Functions --------------------

-- Added by Komal Rawal
-- Added on :: 13-June-2016
-- Desc :: Add is owner column to the table.
IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'IsOwner' AND OBJECT_ID = OBJECT_ID(N'[Budget_Permission]'))
BEGIN
ALTER TABLE [dbo].[Budget_Permission]
ADD [IsOwner] [bit] NOT NULL CONSTRAINT [DF_Budget_Permission_IsOwner]  DEFAULT 0
END 
GO

-- Added by Komal Rawal
-- Added on :: 13-June-2016
-- Desc :: Update isowner flag for existing data.
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '[Budget_Permission]'))
BEGIN
UPDATE [dbo].[Budget_Permission]
SET IsOwner = 1 WHERE UserId = CreatedBy
END 
GO

