-- Created By Nishant Sheth
-- Desc :: It's sample data script for custom reports

-- Add By Nishant Sheth
-- Desc :: Create sample data logic for Cost/MQL and COST/CW
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'InsertDataForTacticTypeCosts') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].[InsertDataForTacticTypeCosts]
GO
/****** Object:  StoredProcedure [dbo].[InsertDataForTacticTypeCosts]    Script Date: 06/21/2016 18:47:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertDataForTacticTypeCosts]
AS
BEGIN

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED  

IF OBJECT_ID('tempdb..#TempDepencey') IS NOT NULL
   DROP TABLE #TempDepencey

IF OBJECT_ID('tempdb..#TempDepenceytables') IS NOT NULL
   DROP TABLE #TempDepenceytables

CREATE TABLE #TempDepencey (oType INT,	oObjName NVARCHAR(MAX),	oOwner NVARCHAR(MAX),	oSequence  INT)
IF OBJECT_ID(N'dbo.[TacticTypeCost]', N'U') IS NOT NULL
BEGIN
	INSERT INTO #TempDepencey EXEC('EXEC sp_MSdependencies N''[TacticTypeCost]'', null, 1315327')
END

SELECT  ROW_NUMBER() OVER (ORDER BY (oObjName)) AS ROWNUM, * into #TempDepenceytables FROM #TempDepencey WHERE oType=8 AND oObjName IS NOT NULL

DECLARE @DependcyCount int;
DECLARE @DependcyInitCount int=1;
Declare @DependcyTableName nvarchar(max);
Declare @DependcyTableQuery nvarchar(max)='';

SELECT @DependcyCount = Count(*) FROm #TempDepenceytables

While @DependcyInitCount<=@DependcyCount
BEGIN	

	SELECT @DependcyTableName = oObjName FROM #TempDepenceytables WHERE ROWNUM=@DependcyInitCount;

		SET @DependcyTableQuery += ' DROP TABLE '+@DependcyTableName+' ';

	SET @DependcyInitCount = @DependcyInitCount+1;
END

EXEC(@DependcyTableQuery);

IF OBJECT_ID(N'dbo.[TacticTypeCost]', N'U') IS NOT NULL
BEGIN
	DROP TABLE TacticTypeCost;
END
   
IF OBJECT_ID('tempdb..#tmpCostData') IS NOT NULL
   DROP TABLE #tmpCostData

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE  #tblCustomData

IF OBJECT_ID('tempdb..#tblCustomFieldColumn') IS NOT NULL
   DROP TABLE  #tblCustomFieldColumn

CREATE TABLE  #tblCustomData(
TacticTypeId INT,
   TacticType NVARCHAR(MAX),
   MQL float NULL,
   Cost float NULL,
   CW float NULL,
   [Cost/MQL] float NULL,
   [Cost/CW] float NULL,
   PlanId Int,
   PlanName NVARCHAR(MAX),
   StartDate date,
   ClientId INT,
   CreatedBy INT
   )

Declare @CustomFieldType NVARCHAR(100)='DropDownList'
Declare @EntityType NVARCHAR(100)='Tactic'
DECLARE @ColumnName NVARCHAR(MAX)='';
DECLARE @DynamicPivot nvarchar(MAX);

SELECT TacticTypeId
,MAX(Title) AS TacticType
,DateCol AS StartDate
,CreatedBy
,ClientId
,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId
,ISNULL(SUM(MQL),0) AS MQL
,ISNULL(SUM(COST),0) AS Cost
,ISNULL(SUM(CW),0) AS CW
, CASE WHEN SUM(COST) > 0 AND SUM(MQL) > 0 THEN SUM(COST)/SUM(MQL) ELSE 0 END AS [Cost/MQL]
, CASE WHEN SUM(COST) > 0 AND SUM(CW) > 0 THEN SUM(COST)/SUM(CW) ELSE 0 END AS [Cost/CW]
into #tmpCostData
FROM(
SELECT TacticType.TacticTypeId
,TacticType.Title
,MQL.DateCol
,MQL.CreatedBy
,MQL.ClientId
,MQL.PlanId
,MQL.PlanName
,MQL.CustomFieldName
,MQL.CustomFieldId
,MQL.CustomFieldOptionId
,MQL.MQL
,NULL AS COST
,NULL AS CW
 FROM 
TacticType
INNER JOIN ( SELECT MQLData.TacticTypeId,SUM(MQL) AS MQL,DateCol,CreatedBy,ClientId,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId FROM (SELECT Tactic.TacticTypeId,Tactic.CreatedBy,Model.ClientId,[Plan].PlanId
,[Plan].Title AS PlanName
,CustomField.Name+'_'+CAST(CustomField.CustomFieldId AS nvarchar) AS CustomFieldName
,CustomField.CustomFieldId
,CustomFieldOptionId
,(CASE  WHEN CAST(REPLACE(Actual.Period,'Y','') AS INT) <= CAST(DATEPART(m, getdate()) AS INT)
AND CAST([Plan].[Year] AS INT)<=CAST(DATEPART(YYYY, getdate()) AS INT) THEN Actual.Actualvalue ELSE NULL END) AS MQL 
,CAST((DATEADD(month,CAST(REPLACE(Actual.Period,'Y','') AS INT)-1,DATEADD(year,CAST([Plan].Year AS int)-1900,0))) AS DATE) AS DateCol
FROM Plan_Campaign_Program_Tactic Tactic 
INNER JOIN TacticType ON Tactic.TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0
INNER JOIN Model ON Model.ModelId=TacticType.ModelId AND Model.IsDeleted=0
INNER JOIN [Plan] ON Model.ModelId=[Plan].ModelId AND  [Plan].IsDeleted=0
--AND [Plan].PlanId = 19053
INNER JOIN Plan_Campaign_Program_Tactic_Actual Actual ON Tactic.PlanTacticId = Actual.PlanTacticId  AND Actual.StageTitle='MQL'
INNER JOIN CustomField WITH (NOLOCK) ON 
CustomField.CustomFieldTypeId = (SELECT CustomFieldTypeId FROM CustomFieldType WITH (NOLOCK) WHERE Name=@CustomFieldType)
--AND CustomField.ClientId=CASE WHEN ISNULL('','')='' THEN Model.ClientId  ELSE '464eb808-ad1f-4481-9365-6aada15023bd' END
AND CustomField.ClientId=Model.ClientId  
AND CustomField.IsDeleted=0 
AND CustomField.IsDisplayForFilter=1 
AND CustomField.EntityType=@EntityType
LEFT JOIN CustomField_Entity WITH (NOLOCK) ON Tactic.PlanTacticId=CustomField_Entity.EntityId
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
LEFT JOIN CustomFieldOption ON 
CustomFieldOption.CustomFieldOptionId = CASE WHEN 
ISNUMERIC(CustomField_Entity.Value) = 1 THEN  CAST(CustomField_Entity.Value AS INT) END
AND CustomFieldOption.IsDeleted=0
WHERE Tactic.IsDeleted=0 AND Tactic.[Status] IN ('Approved','In-Progress','Complete')
) AS MQLData WHERE MQLData.MQL IS NOT NULL GROUP BY TacticTypeId,DateCol,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId) AS MQL ON TacticType.TacticTypeId=MQL.TacticTypeId

WHERE TacticType.IsDeleted=0

UNION ALL

SELECT TacticType.TacticTypeId
,TacticType.Title
,CW.DateCol
,CW.CreatedBy
,CW.ClientId
,CW.PlanId
,CW.PlanName
,CustomFieldName
,CustomFieldId
,CustomFieldOptionId
,NULL AS MQL
,NULL AS COST
,CW.CW
 FROM 
TacticType
INNER JOIN ( SELECT CWData.TacticTypeId,SUM(CW) AS CW,DateCol,CreatedBy,ClientId,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId FROM (SELECT Tactic.TacticTypeId,Tactic.CreatedBy,Model.ClientId,[Plan].PlanId
,[Plan].Title AS PlanName
,CustomField.Name+'_'+CAST(CustomField.CustomFieldId AS nvarchar)AS CustomFieldName
,CustomField.CustomFieldId,CustomFieldOptionId
,(CASE  WHEN CAST(REPLACE(Actual.Period,'Y','') AS INT) <= CAST(DATEPART(m, getdate()) AS INT)
AND CAST([Plan].[Year] AS INT)<=CAST(DATEPART(YYYY, getdate()) AS INT) THEN Actual.Actualvalue ELSE NULL END) AS CW 
,CAST((DATEADD(month,CAST(REPLACE(Actual.Period,'Y','') AS INT)-1,DATEADD(year,CAST([Plan].Year AS int)-1900,0))) AS DATE) AS DateCol
FROM Plan_Campaign_Program_Tactic Tactic 
INNER JOIN TacticType ON Tactic.TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0
INNER JOIN Model ON Model.ModelId=TacticType.ModelId AND Model.IsDeleted=0
INNER JOIN [Plan] ON Model.ModelId=[Plan].ModelId AND  [Plan].IsDeleted=0
--AND [Plan].PlanId = 19053
INNER JOIN Plan_Campaign_Program_Tactic_Actual Actual ON Tactic.PlanTacticId = Actual.PlanTacticId  AND Actual.StageTitle='CW'
INNER JOIN CustomField WITH (NOLOCK) ON 
CustomField.CustomFieldTypeId = (SELECT CustomFieldTypeId FROM CustomFieldType WITH (NOLOCK) WHERE Name=@CustomFieldType)
--AND CustomField.ClientId=CASE WHEN ISNULL('464eb808-ad1f-4481-9365-6aada15023bd','')='' THEN Model.ClientId ELSE '464eb808-ad1f-4481-9365-6aada15023bd' END
AND CustomField.ClientId=Model.ClientId  
AND CustomField.IsDeleted=0 
AND CustomField.IsDisplayForFilter=1 
AND CustomField.EntityType=@EntityType
LEFT JOIN CustomField_Entity WITH (NOLOCK) ON Tactic.PlanTacticId=CustomField_Entity.EntityId
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
LEFT JOIN CustomFieldOption ON 
CustomFieldOption.CustomFieldOptionId = CASE WHEN 
ISNUMERIC(CustomField_Entity.Value) = 1 THEN  CAST(CustomField_Entity.Value AS INT) END
AND CustomFieldOption.IsDeleted=0
WHERE Tactic.IsDeleted=0 AND Tactic.[Status] IN ('Approved','In-Progress','Complete')
) AS CWData WHERE CWData.CW IS NOT NULL GROUP BY TacticTypeId,DateCol,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId) AS CW ON TacticType.TacticTypeId=CW.TacticTypeId

WHERE TacticType.IsDeleted=0

UNION ALL

SELECT TacticType.TacticTypeId
,TacticType.Title
,Cost.DateCol
,Cost.CreatedBy
,Cost.ClientId
,Cost.PlanId
,Cost.PlanName
,CustomFieldName
,CustomFieldId
,CustomFieldOptionId
,NULl AS MQL
,Cost.Cost
,NULL AS CW
 FROM 
TacticType
INNER JOIN (SELECT CostData.TacticTypeId,SUM(Cost) AS Cost,DateCol,CreatedBy,ClientId,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId FROM (SELECT Tactic.TacticTypeId,Tactic.CreatedBy,Model.ClientId,[Plan].PlanId
,[Plan].Title AS PlanName
,CustomField.Name+'_'+CAST(CustomField.CustomFieldId AS nvarchar) AS CustomFieldName
,CustomField.CustomFieldId
,CustomFieldOptionId
,(CASE  WHEN CAST(REPLACE(LineItemCost.Period,'Y','') AS INT) <= CAST(DATEPART(m, getdate()) AS INT)
AND CAST([Plan].[Year] AS INT)<=CAST(DATEPART(YYYY, getdate()) AS INT) THEN LineItemCost.Value ELSE NULL END) AS Cost
,CAST((DATEADD(month,CAST(REPLACE(LineItemCost.Period,'Y','') AS INT)-1,DATEADD(year,CAST([Plan].Year AS int)-1900,0))) AS DATE) AS DateCol
FROM Plan_Campaign_Program_Tactic Tactic 
INNER JOIN TacticType ON Tactic.TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0
INNER JOIN Model ON Model.ModelId=TacticType.ModelId AND Model.IsDeleted=0
INNER JOIN [Plan] ON Model.ModelId=[Plan].ModelId AND  [Plan].IsDeleted=0
--AND [Plan].PlanId = 19053
INNER JOIN Plan_Campaign_Program_Tactic_LineItem LineItem ON Tactic.PlanTacticId = LineItem.PlanTacticId  AND LineItem.IsDeleted=0
INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual LineItemCost ON LineItem.PlanLineItemId = LineItemCost.PlanLineItemId
INNER JOIN CustomField WITH (NOLOCK) ON 
CustomField.CustomFieldTypeId = (SELECT CustomFieldTypeId FROM CustomFieldType WITH (NOLOCK) WHERE Name=@CustomFieldType)
--AND CustomField.ClientId=CASE WHEN ISNULL('464eb808-ad1f-4481-9365-6aada15023bd','')='' THEN Model.ClientId ELSE '464eb808-ad1f-4481-9365-6aada15023bd' END
AND CustomField.ClientId=Model.ClientId  
AND CustomField.IsDeleted=0 
AND CustomField.IsDisplayForFilter=1 
AND CustomField.EntityType=@EntityType
LEFT JOIN CustomField_Entity WITH (NOLOCK) ON Tactic.PlanTacticId=CustomField_Entity.EntityId
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
LEFT JOIN CustomFieldOption ON 
CustomFieldOption.CustomFieldOptionId = CASE WHEN 
ISNUMERIC(CustomField_Entity.Value) = 1 THEN  CAST(CustomField_Entity.Value AS INT) END
AND CustomFieldOption.IsDeleted=0
WHERE Tactic.IsDeleted=0 AND Tactic.[Status] IN ('Approved','In-Progress','Complete')
) AS CostData WHERE CostData.Cost IS NOT NULL GROUP BY DateCol,TacticTypeId,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId
) AS Cost ON TacticType.TacticTypeId=Cost.TacticTypeId
WHERE TacticType.IsDeleted=0
) AS CostByMQL
GROUP BY  TacticTypeId,DateCol,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId

SELECT ROW_NUMBER() OVER (ORDER BY (CustomFieldName)) AS ROWNUM,CustomFieldName  INTO #tblCustomFieldColumn  FROM (
SELECT DISTINCT CustomFieldName
FROM #tmpCostData
WHERE CustomFieldName IS NOT NULL OR CustomFieldName='') AS DummyColumn

Declare @AlterTable NVARCHAR(MAX)='';

DECLARE @Count INT = 1
DECLARE @EntityRowCount INT;

SELECT @EntityRowCount = COUNT(*) FROM #tblCustomFieldColumn

DECLARE @CustomColumnName NVARCHAR(MAX)='';

While @Count<=@EntityRowCount
BEGIN	
	SELECT @CustomColumnName = CustomFieldName FROM #tblCustomFieldColumn WHERE ROWNUM=@Count
	
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD ['+@CustomColumnName+'] NVARCHAR(MAX) '

	IF(@Count = @EntityRowCount)
	BEGIN
		SET @ColumnName+= '['+@CustomColumnName+']';
	END
	ELSE
	BEGIN
		SET @ColumnName+= '['+@CustomColumnName+'],';
	END
	SET @Count = @Count+1;
END

--SELECT @AlterTable
EXEC(@AlterTable)

SELECT @DynamicPivot = '
INSERT INTO #tblCustomData
SELECT 
TacticTypeId,TacticType,MQL,Cost,CW,[Cost/MQL],[Cost/CW],PlanId,PlanName,StartDate,ClientId,CreatedBy,'+@ColumnName+'
FROM #tmpCostData
PIVOT(
MAX(CustomFieldOptionId)
FOR CustomFieldName IN('+@ColumnName+')
)As PVT
'

EXEC SP_EXECUTESQL @DynamicPivot

DECLARE @DynamicData NVARCHAR(MAX);

SET @DynamicData = '
SELECT ROW_NUMBER() Over (Order by TacticTypeId) As Id,* into TacticTypeCost FROM 
(
SELECT TacticTypeId,MAX(TacticType)TacticType,MAX(MQL) MQL,MAX(Cost) Cost,MAX(CW) CW
,MAX([Cost/MQL])[Cost/MQL],MAX([Cost/CW])[Cost/CW],PlanId,MAX(PlanName)PlanName,StartDate
,MAX(ClientId)ClientId,CreatedBy';

SET @Count=1;
While @Count<=@EntityRowCount
BEGIN	
	SELECT @CustomColumnName = CustomFieldName FROM #tblCustomFieldColumn WHERE ROWNUM=@Count
	
SET @DynamicData+=', STUFF((SELECT DISTINCT '','' + ISNULL(MAX(A.['+@CustomColumnName+']),'''') FROM #tblCustomData A
Where A.[TacticTypeId]=B.[TacticTypeId] Group BY PlanId FOR XML PATH('''')),1,1,'''') As ['+@CustomColumnName+'] '

	SET @Count = @Count+1;
END

SET @DynamicData+=' FROM #tblCustomData B 
GROUP BY TacticTypeId,StartDate,PlanId,CreatedBy) AS CostData'
EXEC SP_EXECUTESQL @DynamicData

IF OBJECT_ID(N'dbo.[TacticTypeCost]', N'U') IS NOT NULL
BEGIN
	ALTER TABLE [TacticTypeCost] ALTER COLUMN [Id] BIGINT NOT NULL;

	ALTER TABLE [TacticTypeCost] ADD CONSTRAINT PK_TacticTypeCos_ID PRIMARY KEY CLUSTERED (Id);  
END

END
GO
EXEC [InsertDataForTacticTypeCosts]
GO
-- Insert into Dashboard
Declare @Dashboard NVARCHAR(MAX)='TacticType' -- Change the @Dashboard varaible name 
Declare @DashboardDisplayName NVARCHAR(MAX)='Tactic Type' -- Change the @DashboardDisplayName varaible name 
IF NOT EXISTS(SELECT * FROM Dashboard WHERE Name=@Dashboard)
BEGIN
	INSERT INTO Dashboard ([Name]
      ,[DisplayName]
      ,[DisplayOrder]
      ,[Rows]
      ,[Columns]
      ,[IsDeleted])
      VALUES (@Dashboard,@DashboardDisplayName,1,2,2,0)


-- Insert into Report Graph

IF NOT EXISTS (SELECT * FROM [ReportGraph]  WHERE [GraphType] = 'PIE')	
BEGIN
INSERT [dbo].[ReportGraph] ([GraphType], [IsLableDisplay], [LabelPosition], [IsLegendVisible], [LegendPosition], [IsDataLabelVisible], [DataLablePosition], [DefaultRows], [ChartAttribute], [ConfidenceLevel], [IsGoalDisplay], [IsGoalDisplayForFilterCritaria], [DateRangeGoalOption], [IsComparisonDisplay], [CustomQuery], [IsIndicatorDisplay], [IsSortByValue], [SortOrder], [DisplayGoalAsLine], [CustomFilter], [DrillDownCustomQuery], [DrillDownXFilter], [TotalDecimalPlaces], [MagnitudeValue], [IsICustomFilterApply], [IsTrendLineDisplay]) 
VALUES (N'PIE', 1, N'LEFT', 1, N'right,middle,y', 0, N'LEFT', 5, NULL, NULL, 0, 0, 1, NULL, NULL, NULL, 1, N'DESC', NULL, N'0', NULL, NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportGraph]  WHERE [GraphType] = 'DONUT')	
BEGIN
INSERT [dbo].[ReportGraph] ([GraphType], [IsLableDisplay], [LabelPosition], [IsLegendVisible], [LegendPosition], [IsDataLabelVisible], [DataLablePosition], [DefaultRows], [ChartAttribute], [ConfidenceLevel], [IsGoalDisplay], [IsGoalDisplayForFilterCritaria], [DateRangeGoalOption], [IsComparisonDisplay], [CustomQuery], [IsIndicatorDisplay], [IsSortByValue], [SortOrder], [DisplayGoalAsLine], [CustomFilter], [DrillDownCustomQuery], [DrillDownXFilter], [TotalDecimalPlaces], [MagnitudeValue], [IsICustomFilterApply], [IsTrendLineDisplay]) 
VALUES (N'DONUT', 1, N'LEFT', 1, N'right,middle,y', 0, N'LEFT', 5, NULL, NULL, 0, 0, 1, NULL, NULL, NULL, 1, N'DESC', NULL, N'0', NULL, NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportGraph]  WHERE [GraphType] = 'STACKCOL')	
BEGIN
INSERT [dbo].[ReportGraph] ([GraphType], [IsLableDisplay], [LabelPosition], [IsLegendVisible], [LegendPosition], [IsDataLabelVisible], [DataLablePosition], [DefaultRows], [ChartAttribute], [ConfidenceLevel], [IsGoalDisplay], [IsGoalDisplayForFilterCritaria], [DateRangeGoalOption], [IsComparisonDisplay], [CustomQuery], [IsIndicatorDisplay], [IsSortByValue], [SortOrder], [DisplayGoalAsLine], [CustomFilter], [DrillDownCustomQuery], [DrillDownXFilter], [TotalDecimalPlaces], [MagnitudeValue], [IsICustomFilterApply], [IsTrendLineDisplay]) 
VALUES (N'STACKCOL', 1, N'LEFT', 1, N'right,middle,y', 0, N'LEFT', 5, NULL, NULL, 0, 0, 1, NULL, NULL, NULL, 0, N'DESC', NULL, N'0', NULL, NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportGraph]  WHERE [GraphType] = 'LINE')	
BEGIN
INSERT [dbo].[ReportGraph] ([GraphType], [IsLableDisplay], [LabelPosition], [IsLegendVisible], [LegendPosition], [IsDataLabelVisible], [DataLablePosition], [DefaultRows], [ChartAttribute], [ConfidenceLevel], [IsGoalDisplay], [IsGoalDisplayForFilterCritaria], [DateRangeGoalOption], [IsComparisonDisplay], [CustomQuery], [IsIndicatorDisplay], [IsSortByValue], [SortOrder], [DisplayGoalAsLine], [CustomFilter], [DrillDownCustomQuery], [DrillDownXFilter], [TotalDecimalPlaces], [MagnitudeValue], [IsICustomFilterApply], [IsTrendLineDisplay]) 
VALUES (N'LINE', 1, N'LEFT', 1, N'right,middle,y', 0, N'LEFT', 5, NULL, NULL, 0, 0, 1, NULL, NULL, NULL, 0, N'DESC', NULL, N'0', NULL, NULL, NULL, NULL, NULL, NULL)
END

DECLARE @GraphTypePIE int;
DECLARE @GraphTypeDONUT int;
DECLARE @GraphTypeSTACKCOL int;
DECLARE @GraphTypeLINE int;

SELECT TOP(1) @GraphTypePIE=id FROM ReportGraph WHERE GraphType='PIE' ORDER BY id DESC;
SELECT TOP(1) @GraphTypeDONUT=id FROM ReportGraph WHERE GraphType='DONUT' ORDER BY id DESC;
SELECT TOP(1) @GraphTypeSTACKCOL=id FROM ReportGraph WHERE GraphType='STACKCOL' ORDER BY id DESC;
SELECT TOP(1) @GraphTypeLINE=id FROM ReportGraph WHERE GraphType='LINE' ORDER BY id DESC;

-- Insert into Dimensions
IF NOT EXISTS (SELECT * FROM [Dimension]  WHERE [Name] = 'StartDate')	
BEGIN
INSERT [dbo].[Dimension] ([Name], [TableName], [ColumnName], [Formula], [CreatedBy], [CreatedDate], [IsDeleted], [IsDateDimension], [ValueFormula], [ComputeAllValues], [StartDate], [EndDate]) 
VALUES (N'StartDate', N'TacticTypeCost', N'StartDate', N'SELECT DISTINCT d2.Id as value, StartDate as display,d2.id as orderby FROM TacticTypeCost d1 INNER JOIN DimTime d2 ON d1.StartDate = d2.DateTime WHERE d1.StartDate IS NOT NULL'
, N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, 1, N'select d1.id as joinid, d2.id as value from TacticTypeCost d1 inner join dimtime d2 on d1.StartDate=d2.[DateTime]', 1, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Dimension]  WHERE [Name] = 'PlanId')	
BEGIN
INSERT [dbo].[Dimension] ([Name], [TableName], [ColumnName], [Formula], [CreatedBy], [CreatedDate], [IsDeleted], [IsDateDimension], [ValueFormula], [ComputeAllValues], [StartDate], [EndDate]) 
VALUES (N'PlanId', N'TacticTypeCost', N'PlanId', N'select distinct PlanId value,PlanId display,PlanId orderby from TacticTypeCost', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, 0, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Dimension]  WHERE [Name] = 'ClientId')	
BEGIN
INSERT [dbo].[Dimension] ([Name], [TableName], [ColumnName], [Formula], [CreatedBy], [CreatedDate], [IsDeleted], [IsDateDimension], [ValueFormula], [ComputeAllValues], [StartDate], [EndDate]) 
VALUES (N'ClientId', N'TacticTypeCost', N'ClientId', N'select distinct ClientId value,ClientId display,ClientId orderby from TacticTypeCost', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, 0, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Dimension]  WHERE [Name] = 'TacticTypeId')	
BEGIN
INSERT [dbo].[Dimension] ([Name], [TableName], [ColumnName], [Formula], [CreatedBy], [CreatedDate], [IsDeleted], [IsDateDimension], [ValueFormula], [ComputeAllValues], [StartDate], [EndDate]) 
VALUES (N'TacticTypeId', N'TacticTypeCost', N'TacticTypeId', N'select distinct TacticTypeId value,TacticTypeId display,TacticTypeId orderby from TacticTypeCost', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, 0, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Dimension]  WHERE [Name] = 'TacticType')	
BEGIN
INSERT [dbo].[Dimension] ([Name], [TableName], [ColumnName], [Formula], [CreatedBy], [CreatedDate], [IsDeleted], [IsDateDimension], [ValueFormula], [ComputeAllValues], [StartDate], [EndDate]) 
VALUES (N'TacticType', N'TacticTypeCost', N'TacticType', N'select distinct TacticType value,TacticType display,TacticType orderby from TacticTypeCost', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, 0, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Dimension]  WHERE [Name] = 'CreatedBy')	
BEGIN
INSERT [dbo].[Dimension] ([Name], [TableName], [ColumnName], [Formula], [CreatedBy], [CreatedDate], [IsDeleted], [IsDateDimension], [ValueFormula], [ComputeAllValues], [StartDate], [EndDate]) 
VALUES (N'CreatedBy', N'TacticTypeCost', N'CreatedBy', N'select distinct CreatedBy value,CreatedBy display,CreatedBy orderby from TacticTypeCost WHERE CreatedBy IS NOT NULL'
, N'BCG', CAST(N'1990-01-01 00:00:00.000' AS DateTime), 0, 0, NULL, NULL, NULL, NULL)
END

Declare @DimStartDate int;
Declare @DimPlanId int;
Declare @DimClientId int;
Declare @DimTacticType int;
Declare @DimCreatedBy int;

SELECT TOP(1) @DimStartDate = id FROM  Dimension WHERE Name='StartDate' ORDER BY id DESC;
SELECT TOP(1) @DimPlanId = id FROM  Dimension WHERE Name='PlanId' ORDER BY id DESC;
SELECT TOP(1) @DimClientId = id FROM  Dimension WHERE Name='ClientId' ORDER BY id DESC;
SELECT TOP(1) @DimTacticType = id FROM  Dimension WHERE Name='TacticType' ORDER BY id DESC;
SELECT TOP(1) @DimCreatedBy = id FROM  Dimension WHERE Name='CreatedBy' ORDER BY id DESC;
-- Insert into Report Axis


IF NOT EXISTS (SELECT * FROM [ReportAxis]  WHERE [ReportGraphId] = @GraphTypePIE AND [AxisName]='X' AND [Dimensionid]=@DimTacticType)	
BEGIN	
INSERT [dbo].[ReportAxis] ([ReportGraphId], [AxisName], [Dimensionid], [GroupDimensionValue]) VALUES (@GraphTypePIE, N'X', @DimTacticType, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportAxis]  WHERE [ReportGraphId] = @GraphTypeDONUT AND [AxisName]='X' AND [Dimensionid]=@DimTacticType)	
BEGIN	
INSERT [dbo].[ReportAxis] ([ReportGraphId], [AxisName], [Dimensionid], [GroupDimensionValue]) VALUES (@GraphTypeDONUT, N'X', @DimTacticType, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportAxis]  WHERE [ReportGraphId] = @GraphTypeSTACKCOL AND [AxisName]='X' AND [Dimensionid]=@DimStartDate)	
BEGIN	
INSERT [dbo].[ReportAxis] ([ReportGraphId], [AxisName], [Dimensionid], [GroupDimensionValue]) VALUES (@GraphTypeSTACKCOL, N'X', @DimStartDate, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportAxis]  WHERE [ReportGraphId] = @GraphTypeSTACKCOL AND [AxisName]='Y' AND [Dimensionid]=@DimTacticType)	
BEGIN	
INSERT [dbo].[ReportAxis] ([ReportGraphId], [AxisName], [Dimensionid], [GroupDimensionValue]) VALUES (@GraphTypeSTACKCOL, N'Y', @DimTacticType, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportAxis]  WHERE [ReportGraphId] = @GraphTypeLINE AND [AxisName]='X' AND [Dimensionid]=@DimStartDate)	
BEGIN	
INSERT [dbo].[ReportAxis] ([ReportGraphId], [AxisName], [Dimensionid], [GroupDimensionValue]) VALUES (@GraphTypeLINE, N'X', @DimStartDate, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportAxis]  WHERE [ReportGraphId] = @GraphTypeLINE AND [AxisName]='Y' AND [Dimensionid]=@DimTacticType)	
BEGIN	
INSERT [dbo].[ReportAxis] ([ReportGraphId], [AxisName], [Dimensionid], [GroupDimensionValue]) VALUES (@GraphTypeLINE, N'Y', @DimTacticType, NULL)
END
-- Insert into Measure
IF NOT EXISTS (SELECT * FROM [Measure]  WHERE [Name] = 'Count')	
BEGIN
INSERT [dbo].[Measure] ([Name], [AggregationQuery], [CreatedBy], [CreatedDate], [IsDeleted], [MeasureTableName], [AggregationType], [DisplayColorIndication], [ComputeAllValues], [ComputeAllValuesFormula], [DrillDownWhereClause], [UseRowCountFromFormula])
VALUES (N'Count', N'select d1.#DIMENSIONFIELD# , count(*) as Generated, COUNT(*) as RecordCount from TacticTypeCost_VIEW d1  where d1.#DIMENSIONFIELD# IS NOT NULL group by d1.#DIMENSIONFIELD#', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 1, N'TacticTypeCost', N'COUNT', NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Measure]  WHERE [Name] = 'Cost')	
BEGIN
INSERT [dbo].[Measure] ([Name], [AggregationQuery], [CreatedBy], [CreatedDate], [IsDeleted], [MeasureTableName], [AggregationType], [DisplayColorIndication], [ComputeAllValues], [ComputeAllValuesFormula], [DrillDownWhereClause], [UseRowCountFromFormula])
VALUES (N'Cost', N'select d1.#DIMENSIONFIELD# , SUM(ISNULL(Cost,0)) as Cost, COUNT(*) as RecordCount from TacticTypeCost_VIEW d1  where d1.#DIMENSIONFIELD# IS NOT NULL group by d1.#DIMENSIONFIELD#', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 1, N'TacticTypeCost', N'SUM', NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Measure]  WHERE [Name] = 'Cost/MQL')	
BEGIN
INSERT [dbo].[Measure] ([Name], [AggregationQuery], [CreatedBy], [CreatedDate], [IsDeleted], [MeasureTableName], [AggregationType], [DisplayColorIndication], [ComputeAllValues], [ComputeAllValuesFormula], [DrillDownWhereClause], [UseRowCountFromFormula])
VALUES (N'Cost/MQL', N'select d1.#DIMENSIONFIELD# , SUM(ISNULL([Cost/MQL],0)) as [Cost/MQL], COUNT(*) as RecordCount from TacticTypeCost_VIEW d1  where d1.#DIMENSIONFIELD# IS NOT NULL group by d1.#DIMENSIONFIELD#', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, N'TacticTypeCost', N'SUM', NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [Measure]  WHERE [Name] = 'Cost/CW')	
BEGIN
INSERT [dbo].[Measure] ([Name], [AggregationQuery], [CreatedBy], [CreatedDate], [IsDeleted], [MeasureTableName], [AggregationType], [DisplayColorIndication], [ComputeAllValues], [ComputeAllValuesFormula], [DrillDownWhereClause], [UseRowCountFromFormula])
VALUES (N'Cost/CW', N'select d1.#DIMENSIONFIELD# , SUM(ISNULL([Cost/CW],0)) as [Cost/CW], COUNT(*) as RecordCount from TacticTypeCost_VIEW d1  where d1.#DIMENSIONFIELD# IS NOT NULL group by d1.#DIMENSIONFIELD#', N'BCG', CAST(N'1900-01-01 00:00:00.000' AS DateTime), 0, N'TacticTypeCost', N'SUM', NULL, NULL, NULL, NULL, NULL)
END

DECLARE @MeaCount int;
DECLARE @MeaCost int;
DECLARE @MeaCostByMQL int;
DECLARE @MeaCostBYCW int;

SELECT TOP(1) @MeaCount = id FROM [Measure] WHERE Name='Count' ORDER BY id DESC;
SELECT TOP(1) @MeaCost = id FROM [Measure] WHERE Name='Cost' ORDER BY id DESC;
SELECT TOP(1) @MeaCostByMQL = id FROM [Measure] WHERE Name='Cost/MQL' ORDER BY id DESC;
SELECT TOP(1) @MeaCostBYCW = id FROM [Measure] WHERE Name='Cost/CW' ORDER BY id DESC;

-- Insert into Report Graph Column
IF NOT EXISTS (SELECT * FROM [ReportGraphColumn]  WHERE [ReportGraphId] = @GraphTypePIE AND [MeasureId] = @MeaCostByMQL)	
BEGIN
INSERT [dbo].[ReportGraphColumn] ([ReportGraphId], [MeasureId], [ColumnOrder], [SymbolType], [DisplayInTable], [PrevMeasureCalculation], [DisplayAsNumerator], [DisplayAsDenominator], [WeekMeasureId], [MonthMeasureId], [QuarterMeasureId], [YearMeasureId], [TotalDecimalPlaces], [MagnitudeValue]) 
VALUES (@GraphTypePIE, @MeaCostByMQL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportGraphColumn]  WHERE [ReportGraphId] = @GraphTypeDONUT AND [MeasureId] = @MeaCostBYCW)	
BEGIN
INSERT [dbo].[ReportGraphColumn] ([ReportGraphId], [MeasureId], [ColumnOrder], [SymbolType], [DisplayInTable], [PrevMeasureCalculation], [DisplayAsNumerator], [DisplayAsDenominator], [WeekMeasureId], [MonthMeasureId], [QuarterMeasureId], [YearMeasureId], [TotalDecimalPlaces], [MagnitudeValue]) 
VALUES (@GraphTypeDONUT, @MeaCostBYCW, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportGraphColumn]  WHERE [ReportGraphId] = @GraphTypeSTACKCOL AND [MeasureId] = @MeaCostByMQL)	
BEGIN
INSERT [dbo].[ReportGraphColumn] ([ReportGraphId], [MeasureId], [ColumnOrder], [SymbolType], [DisplayInTable], [PrevMeasureCalculation], [DisplayAsNumerator], [DisplayAsDenominator], [WeekMeasureId], [MonthMeasureId], [QuarterMeasureId], [YearMeasureId], [TotalDecimalPlaces], [MagnitudeValue]) 
VALUES (@GraphTypeSTACKCOL, @MeaCostByMQL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT * FROM [ReportGraphColumn]  WHERE [ReportGraphId] = @GraphTypeLINE AND [MeasureId] = @MeaCostBYCW)	
BEGIN
INSERT [dbo].[ReportGraphColumn] ([ReportGraphId], [MeasureId], [ColumnOrder], [SymbolType], [DisplayInTable], [PrevMeasureCalculation], [DisplayAsNumerator], [DisplayAsDenominator], [WeekMeasureId], [MonthMeasureId], [QuarterMeasureId], [YearMeasureId], [TotalDecimalPlaces], [MagnitudeValue]) 
VALUES (@GraphTypeLINE, @MeaCostBYCW, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
END

	Declare @DashboardId int;
	SELECT @DashboardId = id FROM Dashboard WHERE Name=@Dashboard;
	IF(@DashboardId>0)
	BEGIN	
			IF NOT EXISTS (SELECT * FROM [DashboardContents]  WHERE [DashboardId] = @DashboardId AND ReportGraphId=@GraphTypePIE AND DisplayName='Cost/MQL by Tactic Type')	
			BEGIN
				INSERT [dbo].[DashboardContents] ([DisplayName], [DashboardId], [DisplayOrder], [ReportTableId], [ReportGraphId], [Height], [Width], [Position], [IsCumulativeData], [IsCommunicativeData], [DashboardPageID], [IsDeleted], [DisplayIfZero], [KeyDataId], [HelpTextId]) 
				VALUES (N'Cost/MQL by Tactic Type', @DashboardId, 1, NULL, @GraphTypePIE, 300, 50, N'LEFT', NULL, NULL, NULL, 0, NULL, NULL, NULL)
			END

			IF NOT EXISTS (SELECT * FROM [DashboardContents]  WHERE [DashboardId] = @DashboardId  AND ReportGraphId=@GraphTypeDONUT AND DisplayName='Cost/CW by Tactic Type')	
			BEGIN
				INSERT [dbo].[DashboardContents] ([DisplayName], [DashboardId], [DisplayOrder], [ReportTableId], [ReportGraphId], [Height], [Width], [Position], [IsCumulativeData], [IsCommunicativeData], [DashboardPageID], [IsDeleted], [DisplayIfZero], [KeyDataId], [HelpTextId]) 
				VALUES (N'Cost/CW by Tactic Type', @DashboardId, 2, NULL, @GraphTypeDONUT, 300, 50, N'LEFT', NULL, NULL, NULL, 0, NULL, NULL, NULL)
			END
	END
END